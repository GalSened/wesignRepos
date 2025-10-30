using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using WeSign.Analytics.Models;
using WeSign.Analytics.Repositories;
using WeSign.Analytics.Services;
using WeSign.Analytics.Configuration;

namespace WeSign.Analytics.Services
{
    /// <summary>
    /// Background service that collects analytics data every 30 seconds
    /// and publishes to S3 for real-time dashboard consumption
    /// </summary>
    public class AnalyticsCollectorService : BackgroundService
    {
        private readonly ILogger<AnalyticsCollectorService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AnalyticsConfig _config;
        private readonly IAnalyticsS3Publisher _s3Publisher;
        private readonly IMetricsValidator _metricsValidator;
        private readonly IAnalyticsNotificationService _notificationService;

        private DateTime _lastWatermark = DateTime.UtcNow.AddMinutes(-5);
        private readonly SemaphoreSlim _collectingSemaphore = new(1, 1);

        public AnalyticsCollectorService(
            ILogger<AnalyticsCollectorService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<AnalyticsConfig> config,
            IAnalyticsS3Publisher s3Publisher,
            IMetricsValidator metricsValidator,
            IAnalyticsNotificationService notificationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _s3Publisher = s3Publisher ?? throw new ArgumentNullException(nameof(s3Publisher));
            _metricsValidator = metricsValidator ?? throw new ArgumentNullException(nameof(metricsValidator));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Analytics Collector Service started at: {time}", DateTimeOffset.Now);

            // Initial delay to allow system startup
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectAndPublishAnalytics(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(_config.CollectionIntervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Analytics collection was cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during analytics collection cycle");

                    // Send alert for collection failures
                    await _notificationService.SendAlertAsync(
                        "Analytics Collection Failed",
                        $"Collection cycle failed: {ex.Message}",
                        AlertSeverity.Warning);

                    // Wait longer on error to avoid overwhelming the system
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
            }

            _logger.LogInformation("Analytics Collector Service stopped at: {time}", DateTimeOffset.Now);
        }

        private async Task CollectAndPublishAnalytics(CancellationToken cancellationToken)
        {
            if (!await _collectingSemaphore.WaitAsync(1000, cancellationToken))
            {
                _logger.LogWarning("Collection already in progress, skipping this cycle");
                return;
            }

            try
            {
                var collectionStart = DateTime.UtcNow;
                _logger.LogDebug("Starting analytics collection cycle at {time}", collectionStart);

                using var scope = _serviceScopeFactory.CreateScope();
                var analyticsRepo = scope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();

                // Load current watermark
                var currentWatermark = await LoadWatermark();
                var collectionWindow = new TimeWindow(currentWatermark, collectionStart);

                _logger.LogDebug("Collecting data from {from} to {to}",
                    collectionWindow.From, collectionWindow.To);

                // Collect data in parallel for performance
                var tasks = new[]
                {
                    CollectKPIData(analyticsRepo, collectionWindow, cancellationToken),
                    CollectUsageAnalytics(analyticsRepo, collectionWindow, cancellationToken),
                    CollectSegmentationData(analyticsRepo, collectionWindow, cancellationToken),
                    CollectProcessFlowData(analyticsRepo, collectionWindow, cancellationToken),
                    CollectPlatformHealthData(analyticsRepo, collectionWindow, cancellationToken)
                };

                var results = await Task.WhenAll(tasks);

                var analyticsSnapshot = new AnalyticsSnapshot
                {
                    Timestamp = collectionStart,
                    TimeWindow = collectionWindow,
                    KPIs = results[0],
                    UsageAnalytics = results[1],
                    SegmentationData = results[2],
                    ProcessFlowData = results[3],
                    PlatformHealth = results[4],
                    DataAge = (DateTime.UtcNow - collectionStart).TotalSeconds,
                    CollectionDurationMs = (DateTime.UtcNow - collectionStart).TotalMilliseconds
                };

                // Validate data integrity
                var validationResult = await _metricsValidator.ValidateAsync(analyticsSnapshot);
                if (!validationResult.IsValid)
                {
                    _logger.LogError("Metrics validation failed: {errors}",
                        string.Join(", ", validationResult.Errors));

                    await _notificationService.SendAlertAsync(
                        "Analytics Data Validation Failed",
                        $"Validation errors: {string.Join(", ", validationResult.Errors)}",
                        AlertSeverity.Critical);

                    return; // Don't publish invalid data
                }

                // Publish to S3
                await _s3Publisher.PublishSnapshotAsync(analyticsSnapshot, cancellationToken);
                await _s3Publisher.AppendTimeSeriesAsync(analyticsSnapshot, cancellationToken);

                // Update watermark
                await SaveWatermark(collectionStart);

                var collectionDuration = (DateTime.UtcNow - collectionStart).TotalMilliseconds;
                _logger.LogInformation(
                    "Analytics collection completed successfully in {duration:F0}ms. " +
                    "Processed {documents} documents, {users} active users",
                    collectionDuration,
                    analyticsSnapshot.KPIs.DocumentsCreated,
                    analyticsSnapshot.KPIs.DailyActiveUsers);

                // Check SLO compliance
                if (collectionDuration > _config.MaxCollectionTimeMs)
                {
                    _logger.LogWarning(
                        "Collection duration {duration:F0}ms exceeded SLO of {slo}ms",
                        collectionDuration, _config.MaxCollectionTimeMs);
                }

                if (analyticsSnapshot.DataAge > _config.MaxDataAgeSeconds)
                {
                    await _notificationService.SendAlertAsync(
                        "Analytics Data Freshness SLO Violation",
                        $"Data age {analyticsSnapshot.DataAge:F0}s exceeds SLO of {_config.MaxDataAgeSeconds}s",
                        AlertSeverity.Warning);
                }
            }
            finally
            {
                _collectingSemaphore.Release();
            }
        }

        private async Task<DashboardKPIs> CollectKPIData(
            IAnalyticsRepository repo,
            TimeWindow window,
            CancellationToken cancellationToken)
        {
            try
            {
                var kpis = await repo.GetKPIMetricsAsync(window, cancellationToken);

                _logger.LogDebug("Collected KPI data: {created} created, {signed} signed, {dau} DAU",
                    kpis.DocumentsCreated, kpis.DocumentsSigned, kpis.DailyActiveUsers);

                return kpis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect KPI data");
                throw;
            }
        }

        private async Task<UsageAnalytics> CollectUsageAnalytics(
            IAnalyticsRepository repo,
            TimeWindow window,
            CancellationToken cancellationToken)
        {
            try
            {
                return await repo.GetUsageAnalyticsAsync(window, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect usage analytics");
                throw;
            }
        }

        private async Task<SegmentationData> CollectSegmentationData(
            IAnalyticsRepository repo,
            TimeWindow window,
            CancellationToken cancellationToken)
        {
            try
            {
                return await repo.GetSegmentationDataAsync(window, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect segmentation data");
                throw;
            }
        }

        private async Task<ProcessFlowData> CollectProcessFlowData(
            IAnalyticsRepository repo,
            TimeWindow window,
            CancellationToken cancellationToken)
        {
            try
            {
                return await repo.GetProcessFlowDataAsync(window, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect process flow data");
                throw;
            }
        }

        private async Task<PlatformHealthData> CollectPlatformHealthData(
            IAnalyticsRepository repo,
            TimeWindow window,
            CancellationToken cancellationToken)
        {
            try
            {
                return await repo.GetPlatformHealthDataAsync(window, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect platform health data");
                throw;
            }
        }

        private async Task<DateTime> LoadWatermark()
        {
            try
            {
                var watermark = await _s3Publisher.GetWatermarkAsync();
                return watermark ?? _lastWatermark;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load watermark, using last known value");
                return _lastWatermark;
            }
        }

        private async Task SaveWatermark(DateTime watermark)
        {
            try
            {
                await _s3Publisher.SaveWatermarkAsync(watermark);
                _lastWatermark = watermark;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save watermark");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Analytics Collector Service is stopping...");

            // Wait for current collection to complete
            await _collectingSemaphore.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Configuration for analytics collection
    /// </summary>
    public class AnalyticsConfig
    {
        public int CollectionIntervalSeconds { get; set; } = 30;
        public int MaxCollectionTimeMs { get; set; } = 5000;
        public int MaxDataAgeSeconds { get; set; } = 90;
        public string S3BucketName { get; set; } = "wesign-analytics";
        public string Environment { get; set; } = "production";
        public bool EnableAlerts { get; set; } = true;
        public string[] AlertEmails { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Time window for data collection
    /// </summary>
    public record TimeWindow(DateTime From, DateTime To)
    {
        public TimeSpan Duration => To - From;
        public bool IsValid => To > From && Duration.TotalHours <= 24;
    }

    /// <summary>
    /// Complete analytics snapshot for a collection cycle
    /// </summary>
    public class AnalyticsSnapshot
    {
        public DateTime Timestamp { get; set; }
        public TimeWindow TimeWindow { get; set; }
        public DashboardKPIs KPIs { get; set; }
        public UsageAnalytics UsageAnalytics { get; set; }
        public SegmentationData SegmentationData { get; set; }
        public ProcessFlowData ProcessFlowData { get; set; }
        public PlatformHealthData PlatformHealth { get; set; }
        public double DataAge { get; set; }
        public double CollectionDurationMs { get; set; }
    }

    /// <summary>
    /// Alert severity levels
    /// </summary>
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }
}