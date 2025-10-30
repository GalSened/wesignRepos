using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeSign.Analytics.Models;
using WeSign.Data;
using WeSign.Models;

namespace WeSign.Analytics.Repositories
{
    /// <summary>
    /// High-performance repository for analytics data collection
    /// Uses read-replica database and optimized queries
    /// </summary>
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly WeSignReadOnlyContext _context;
        private readonly ILogger<AnalyticsRepository> _logger;
        private readonly IQueryBudgetTracker _queryBudgetTracker;

        public AnalyticsRepository(
            WeSignReadOnlyContext context,
            ILogger<AnalyticsRepository> logger,
            IQueryBudgetTracker queryBudgetTracker)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queryBudgetTracker = queryBudgetTracker ?? throw new ArgumentNullException(nameof(queryBudgetTracker));
        }

        public async Task<DashboardKPIs> GetKPIMetricsAsync(TimeWindow window, CancellationToken cancellationToken)
        {
            using var budget = _queryBudgetTracker.StartQuery("GetKPIMetrics", TimeSpan.FromSeconds(2));

            try
            {
                // Get document metrics for the current window
                var documentMetrics = await GetDocumentMetrics(window, cancellationToken);

                // Get active user metrics
                var activeUsers = await GetActiveUserMetrics(cancellationToken);

                // Get time-to-sign metrics
                var timeToSignMetrics = await GetTimeToSignMetrics(window, cancellationToken);

                // Get conversion rates
                var conversionRates = await GetConversionRates(window, cancellationToken);

                // Get previous period for trend calculation
                var previousWindow = new TimeWindow(
                    window.From.AddMinutes(-5),
                    window.From);
                var previousMetrics = await GetDocumentMetrics(previousWindow, cancellationToken);

                var kpis = new DashboardKPIs
                {
                    // Active Users
                    DailyActiveUsers = activeUsers.DAU,
                    WeeklyActiveUsers = activeUsers.WAU,
                    MonthlyActiveUsers = activeUsers.MAU,

                    // Document Metrics
                    DocumentsCreated = documentMetrics.Created,
                    DocumentsSent = documentMetrics.Sent,
                    DocumentsOpened = documentMetrics.Viewed, // Using 'Viewed' from WeSign enum
                    DocumentsSigned = documentMetrics.Signed,
                    DocumentsDeclined = documentMetrics.Declined,
                    DocumentsExpired = documentMetrics.SendingFailed, // Using SendingFailed for expired

                    // Conversion Rates
                    SentToOpenedRate = conversionRates.SentToViewed,
                    OpenedToSignedRate = conversionRates.ViewedToSigned,
                    OverallSuccessRate = conversionRates.CreatedToSigned,
                    AbandonmentRate = conversionRates.AbandonmentRate,

                    // Time Metrics
                    AverageTimeToSign = timeToSignMetrics.Average,
                    MedianTimeToSign = timeToSignMetrics.Median,
                    P95TimeToSign = timeToSignMetrics.P95,

                    // Trends (5-minute comparison)
                    DAUTrend = CalculateTrend(activeUsers.DAU, activeUsers.PreviousDAU),
                    MAUTrend = CalculateTrend(activeUsers.MAU, activeUsers.PreviousMAU),
                    SuccessRateTrend = CalculateTrend(conversionRates.CreatedToSigned,
                                                   previousMetrics.Created > 0 ?
                                                   (previousMetrics.Signed * 100.0 / previousMetrics.Created) : 0),
                    TimeToSignTrend = CalculateTrend(timeToSignMetrics.Average, timeToSignMetrics.PreviousAverage)
                };

                _logger.LogDebug("Collected KPI metrics in {duration}ms", budget.ElapsedMilliseconds);
                return kpis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect KPI metrics");
                throw;
            }
        }

        public async Task<UsageAnalytics> GetUsageAnalyticsAsync(TimeWindow window, CancellationToken cancellationToken)
        {
            using var budget = _queryBudgetTracker.StartQuery("GetUsageAnalytics", TimeSpan.FromSeconds(1));

            try
            {
                // Get time series data for charts (last 6 hours in 5-minute buckets)
                var timeSeriesStart = DateTime.UtcNow.AddHours(-6);

                var timeSeries = await _context.DocumentCollections
                    .Where(dc => dc.CreationTime >= timeSeriesStart)
                    .GroupBy(dc => new
                    {
                        Bucket = EF.Functions.DateDiffMinute(DateTime.MinValue, dc.CreationTime) / 5 * 5
                    })
                    .Select(g => new
                    {
                        Timestamp = DateTime.MinValue.AddMinutes(g.Key.Bucket),
                        Created = g.Count(),
                        Sent = g.Count(dc => dc.DocumentStatus >= DocStatus.Sent),
                        Signed = g.Count(dc => dc.DocumentStatus == DocStatus.Signed)
                    })
                    .OrderBy(x => x.Timestamp)
                    .ToListAsync(cancellationToken);

                // Get hourly usage patterns
                var hourlyUsage = await _context.DocumentCollections
                    .Where(dc => dc.CreationTime >= DateTime.UtcNow.AddDays(-7))
                    .GroupBy(dc => dc.CreationTime.Hour)
                    .Select(g => new HourlyUsage
                    {
                        Hour = g.Key,
                        DocumentsCount = g.Count(),
                        UsersCount = g.Select(dc => dc.UserId).Distinct().Count()
                    })
                    .OrderBy(h => h.Hour)
                    .ToListAsync(cancellationToken);

                // Get daily usage patterns
                var dailyUsage = await _context.DocumentCollections
                    .Where(dc => dc.CreationTime >= DateTime.UtcNow.AddDays(-30))
                    .GroupBy(dc => (int)dc.CreationTime.DayOfWeek)
                    .Select(g => new DailyUsage
                    {
                        DayOfWeek = g.Key,
                        DocumentsCount = g.Count(),
                        UsersCount = g.Select(dc => dc.UserId).Distinct().Count(),
                        AverageTimeToSign = g.Where(dc => dc.SignedTime.HasValue)
                                           .Average(dc => EF.Functions.DateDiffMinute(dc.CreationTime, dc.SignedTime.Value))
                    })
                    .OrderBy(d => d.DayOfWeek)
                    .ToListAsync(cancellationToken);

                return new UsageAnalytics
                {
                    DocumentCreatedSeries = timeSeries.Select(ts => new TimeSeriesPoint(ts.Timestamp, ts.Created)).ToList(),
                    DocumentSentSeries = timeSeries.Select(ts => new TimeSeriesPoint(ts.Timestamp, ts.Sent)).ToList(),
                    DocumentSignedSeries = timeSeries.Select(ts => new TimeSeriesPoint(ts.Timestamp, ts.Signed)).ToList(),
                    UserActivitySeries = timeSeries.Select(ts => new TimeSeriesPoint(ts.Timestamp, ts.Created)).ToList(), // Approximate user activity
                    PeakHours = hourlyUsage,
                    PeakDays = dailyUsage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect usage analytics");
                throw;
            }
        }

        public async Task<SegmentationData> GetSegmentationDataAsync(TimeWindow window, CancellationToken cancellationToken)
        {
            using var budget = _queryBudgetTracker.StartQuery("GetSegmentationData", TimeSpan.FromSeconds(1));

            try
            {
                var recentDate = DateTime.UtcNow.AddDays(-30);

                // Send Type Breakdown based on actual SignMode enum
                var sendTypeBreakdown = await _context.DocumentCollections
                    .Where(dc => dc.CreationTime >= recentDate)
                    .GroupBy(dc => dc.Mode)
                    .Select(g => new SegmentBreakdown
                    {
                        Name = g.Key == SignMode.SelfSign ? "Self Sign" :
                               g.Key == SignMode.Distribution ? "Distribution" :
                               g.Key == SignMode.OrderedWorkflow ? "Ordered Workflow" :
                               g.Key == SignMode.Workflow ? "Workflow" :
                               g.Key == SignMode.Online ? "Online" : "Unknown",
                        Count = g.Count(),
                        Percentage = 0, // Will calculate after query
                        SuccessRate = g.Count(dc => dc.DocumentStatus == DocStatus.Signed) * 100.0 / g.Count(),
                        AverageTimeToSign = g.Where(dc => dc.SignedTime.HasValue)
                                           .Average(dc => EF.Functions.DateDiffMinute(dc.CreationTime, dc.SignedTime.Value))
                    })
                    .ToListAsync(cancellationToken);

                // Calculate percentages
                var totalDocs = sendTypeBreakdown.Sum(s => s.Count);
                foreach (var segment in sendTypeBreakdown)
                {
                    segment.Percentage = totalDocs > 0 ? (segment.Count * 100.0 / totalDocs) : 0;
                }

                // Organization Segmentation
                var organizationBreakdown = await _context.DocumentCollections
                    .Include(dc => dc.User)
                    .Where(dc => dc.CreationTime >= recentDate)
                    .GroupBy(dc => new { dc.User.GroupId, dc.User.GroupName, dc.User.CompanyName })
                    .Select(g => new OrganizationSegment
                    {
                        OrganizationId = g.Key.GroupId,
                        OrganizationName = g.Key.GroupName ?? g.Key.CompanyName,
                        DocumentsCount = g.Count(),
                        UsersCount = g.Select(dc => dc.UserId).Distinct().Count(),
                        SuccessRate = g.Count(dc => dc.DocumentStatus == DocStatus.Signed) * 100.0 / g.Count(),
                        AverageTimeToSign = g.Where(dc => dc.SignedTime.HasValue)
                                           .Average(dc => EF.Functions.DateDiffMinute(dc.CreationTime, dc.SignedTime.Value)),
                        IsHighVolume = g.Count() > 100, // Top volume threshold
                        Tier = g.Count() >= 1000 ? "enterprise" :
                               g.Count() >= 100 ? "business" : "standard"
                    })
                    .OrderByDescending(o => o.DocumentsCount)
                    .Take(20) // Top 20 organizations
                    .ToListAsync(cancellationToken);

                // Template Usage (if templates are used)
                var templateUsage = await _context.DocumentCollections
                    .Include(dc => dc.Template)
                    .Where(dc => dc.CreationTime >= recentDate && dc.Template != null)
                    .GroupBy(dc => new { dc.Template.Id, dc.Template.Name })
                    .Select(g => new TemplateUsageData
                    {
                        TemplateId = g.Key.Id,
                        TemplateName = g.Key.Name,
                        UsageCount = g.Count(),
                        SuccessRate = g.Count(dc => dc.DocumentStatus == DocStatus.Signed) * 100.0 / g.Count(),
                        AverageTimeToSign = g.Where(dc => dc.SignedTime.HasValue)
                                           .Average(dc => EF.Functions.DateDiffMinute(dc.CreationTime, dc.SignedTime.Value)),
                        AbandonmentRate = g.Count(dc => dc.DocumentStatus == DocStatus.Viewed) * 100.0 / g.Count(),
                        LastUsed = g.Max(dc => dc.CreationTime),
                        IsPopular = g.Count() > 50, // Popular threshold
                        Complexity = g.Where(dc => dc.SignedTime.HasValue)
                                     .Average(dc => EF.Functions.DateDiffMinute(dc.CreationTime, dc.SignedTime.Value)) <= 5 ? "simple" :
                                     g.Where(dc => dc.SignedTime.HasValue)
                                     .Average(dc => EF.Functions.DateDiffMinute(dc.CreationTime, dc.SignedTime.Value)) <= 30 ? "medium" : "complex"
                    })
                    .OrderByDescending(t => t.UsageCount)
                    .Take(15) // Top 15 templates
                    .ToListAsync(cancellationToken);

                return new SegmentationData
                {
                    SendTypeBreakdown = sendTypeBreakdown,
                    OrganizationBreakdown = organizationBreakdown,
                    TemplateUsage = templateUsage,
                    DeviceBreakdown = new List<SegmentBreakdown>(), // Would need user agent data
                    GeographicBreakdown = new List<GeographicSegment>() // Would need location data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect segmentation data");
                throw;
            }
        }

        public async Task<ProcessFlowData> GetProcessFlowDataAsync(TimeWindow window, CancellationToken cancellationToken)
        {
            using var budget = _queryBudgetTracker.StartQuery("GetProcessFlowData", TimeSpan.FromSeconds(1));

            try
            {
                var recentDate = DateTime.UtcNow.AddDays(-1);

                // Funnel Analysis based on actual DocStatus enum
                var funnelData = await _context.DocumentCollections
                    .Where(dc => dc.CreationTime >= recentDate)
                    .GroupBy(dc => 1) // Single group for aggregation
                    .Select(g => new
                    {
                        Created = g.Count(),
                        Sent = g.Count(dc => dc.DocumentStatus >= DocStatus.Sent),
                        Viewed = g.Count(dc => dc.DocumentStatus >= DocStatus.Viewed),
                        Signed = g.Count(dc => dc.DocumentStatus == DocStatus.Signed),
                        Declined = g.Count(dc => dc.DocumentStatus == DocStatus.Declined),
                        Failed = g.Count(dc => dc.DocumentStatus == DocStatus.SendingFailed)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                var funnelStages = new List<FunnelStage>();

                if (funnelData != null)
                {
                    funnelStages.AddRange(new[]
                    {
                        new FunnelStage
                        {
                            StageName = "Created",
                            StageOrder = 1,
                            DocumentsCount = funnelData.Created,
                            ConversionRate = 100.0,
                            DropOffRate = funnelData.Created > 0 ? ((funnelData.Created - funnelData.Sent) * 100.0 / funnelData.Created) : 0,
                            AverageTimeInStage = 0 // Initial stage
                        },
                        new FunnelStage
                        {
                            StageName = "Sent",
                            StageOrder = 2,
                            DocumentsCount = funnelData.Sent,
                            ConversionRate = funnelData.Created > 0 ? (funnelData.Sent * 100.0 / funnelData.Created) : 0,
                            DropOffRate = funnelData.Sent > 0 ? ((funnelData.Sent - funnelData.Viewed) * 100.0 / funnelData.Sent) : 0,
                            AverageTimeInStage = 5 // Estimated
                        },
                        new FunnelStage
                        {
                            StageName = "Viewed",
                            StageOrder = 3,
                            DocumentsCount = funnelData.Viewed,
                            ConversionRate = funnelData.Sent > 0 ? (funnelData.Viewed * 100.0 / funnelData.Sent) : 0,
                            DropOffRate = funnelData.Viewed > 0 ? ((funnelData.Viewed - funnelData.Signed) * 100.0 / funnelData.Viewed) : 0,
                            AverageTimeInStage = 15 // Estimated
                        },
                        new FunnelStage
                        {
                            StageName = "Signed",
                            StageOrder = 4,
                            DocumentsCount = funnelData.Signed,
                            ConversionRate = funnelData.Viewed > 0 ? (funnelData.Signed * 100.0 / funnelData.Viewed) : 0,
                            DropOffRate = 0,
                            AverageTimeInStage = 0 // Final stage
                        }
                    });
                }

                // Stuck Documents Analysis
                var stuckDocuments = await _context.DocumentCollections
                    .Include(dc => dc.User)
                    .Where(dc => (dc.DocumentStatus == DocStatus.Sent || dc.DocumentStatus == DocStatus.Viewed) &&
                               EF.Functions.DateDiffHour(dc.CreationTime, DateTime.UtcNow) > 4)
                    .Select(dc => new StuckDocumentInfo
                    {
                        DocumentId = dc.DocumentCollectionId, // Will be hashed for PM role
                        OrganizationName = dc.User.GroupName ?? dc.User.CompanyName,
                        TemplateName = dc.Template != null ? dc.Template.Name : "Ad-hoc Document",
                        CurrentStage = dc.DocumentStatus == DocStatus.Sent ? "Sent" : "Viewed",
                        TimeStuck = EF.Functions.DateDiffHour(dc.CreationTime, DateTime.UtcNow),
                        StuckReason = dc.DocumentStatus == DocStatus.Sent ? "Not opened by signer" : "Opened but not signed",
                        IsRecoverable = true,
                        PriorityLevel = EF.Functions.DateDiffHour(dc.CreationTime, DateTime.UtcNow) > 24 ? "high" : "medium"
                    })
                    .OrderByDescending(sd => sd.TimeStuck)
                    .Take(50) // Top 50 stuck documents
                    .ToListAsync(cancellationToken);

                return new ProcessFlowData
                {
                    FunnelStages = funnelStages,
                    StuckDocuments = stuckDocuments,
                    DropOffPoints = new List<DropOffAnalysis>(), // Would need more detailed event tracking
                    StageDurations = new List<StageDurationData>() // Would need timing event data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect process flow data");
                throw;
            }
        }

        public async Task<PlatformHealthData> GetPlatformHealthDataAsync(TimeWindow window, CancellationToken cancellationToken)
        {
            // This would integrate with actual platform monitoring
            // For now, return basic health metrics
            return new PlatformHealthData
            {
                ApiErrorRate = 0.1, // Would come from logs
                ApiLatencyP95 = 150, // Would come from monitoring
                DatabaseConnectionCount = 25, // Would come from DB monitoring
                SystemHealth = "Healthy"
            };
        }

        // Helper methods

        private async Task<(int Created, int Sent, int Viewed, int Signed, int Declined, int SendingFailed)>
            GetDocumentMetrics(TimeWindow window, CancellationToken cancellationToken)
        {
            var metrics = await _context.DocumentCollections
                .Where(dc => dc.CreationTime >= window.From && dc.CreationTime < window.To)
                .GroupBy(dc => 1)
                .Select(g => new
                {
                    Created = g.Count(),
                    Sent = g.Count(dc => dc.DocumentStatus >= DocStatus.Sent),
                    Viewed = g.Count(dc => dc.DocumentStatus >= DocStatus.Viewed),
                    Signed = g.Count(dc => dc.DocumentStatus == DocStatus.Signed),
                    Declined = g.Count(dc => dc.DocumentStatus == DocStatus.Declined),
                    SendingFailed = g.Count(dc => dc.DocumentStatus == DocStatus.SendingFailed)
                })
                .FirstOrDefaultAsync(cancellationToken);

            return metrics != null ?
                (metrics.Created, metrics.Sent, metrics.Viewed, metrics.Signed, metrics.Declined, metrics.SendingFailed) :
                (0, 0, 0, 0, 0, 0);
        }

        private async Task<(int DAU, int WAU, int MAU, int PreviousDAU, int PreviousMAU)>
            GetActiveUserMetrics(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var activeUsers = await Task.WhenAll(
                // DAU
                _context.DocumentCollections
                    .Where(dc => dc.CreationTime >= now.Date)
                    .Select(dc => dc.UserId)
                    .Distinct()
                    .CountAsync(cancellationToken),

                // WAU
                _context.DocumentCollections
                    .Where(dc => dc.CreationTime >= now.AddDays(-7))
                    .Select(dc => dc.UserId)
                    .Distinct()
                    .CountAsync(cancellationToken),

                // MAU
                _context.DocumentCollections
                    .Where(dc => dc.CreationTime >= now.AddDays(-30))
                    .Select(dc => dc.UserId)
                    .Distinct()
                    .CountAsync(cancellationToken),

                // Previous DAU (yesterday)
                _context.DocumentCollections
                    .Where(dc => dc.CreationTime >= now.AddDays(-1).Date &&
                               dc.CreationTime < now.Date)
                    .Select(dc => dc.UserId)
                    .Distinct()
                    .CountAsync(cancellationToken),

                // Previous MAU (30-60 days ago)
                _context.DocumentCollections
                    .Where(dc => dc.CreationTime >= now.AddDays(-60) &&
                               dc.CreationTime < now.AddDays(-30))
                    .Select(dc => dc.UserId)
                    .Distinct()
                    .CountAsync(cancellationToken)
            );

            return (activeUsers[0], activeUsers[1], activeUsers[2], activeUsers[3], activeUsers[4]);
        }

        private async Task<(double Average, double Median, double P95, double PreviousAverage)>
            GetTimeToSignMetrics(TimeWindow window, CancellationToken cancellationToken)
        {
            var signedDocs = await _context.DocumentCollections
                .Where(dc => dc.DocumentStatus == DocStatus.Signed &&
                           dc.SignedTime.HasValue &&
                           dc.CreationTime >= window.From.AddDays(-7)) // Last 7 days for context
                .Select(dc => EF.Functions.DateDiffMinute(dc.CreationTime, dc.SignedTime.Value))
                .ToListAsync(cancellationToken);

            if (signedDocs.Count == 0)
                return (0, 0, 0, 0);

            signedDocs.Sort();

            var average = signedDocs.Average();
            var median = signedDocs[signedDocs.Count / 2];
            var p95Index = (int)(signedDocs.Count * 0.95);
            var p95 = signedDocs[Math.Min(p95Index, signedDocs.Count - 1)];

            // Get previous period average for trend
            var previousDocs = await _context.DocumentCollections
                .Where(dc => dc.DocumentStatus == DocStatus.Signed &&
                           dc.SignedTime.HasValue &&
                           dc.CreationTime >= window.From.AddDays(-14) &&
                           dc.CreationTime < window.From.AddDays(-7))
                .Select(dc => EF.Functions.DateDiffMinute(dc.CreationTime, dc.SignedTime.Value))
                .ToListAsync(cancellationToken);

            var previousAverage = previousDocs.Count > 0 ? previousDocs.Average() : average;

            return (average, median, p95, previousAverage);
        }

        private async Task<(double SentToViewed, double ViewedToSigned, double CreatedToSigned, double AbandonmentRate)>
            GetConversionRates(TimeWindow window, CancellationToken cancellationToken)
        {
            var recentDate = DateTime.UtcNow.AddDays(-1);

            var conversionData = await _context.DocumentCollections
                .Where(dc => dc.CreationTime >= recentDate)
                .GroupBy(dc => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Sent = g.Count(dc => dc.DocumentStatus >= DocStatus.Sent),
                    Viewed = g.Count(dc => dc.DocumentStatus >= DocStatus.Viewed),
                    Signed = g.Count(dc => dc.DocumentStatus == DocStatus.Signed),
                    ViewedButNotSigned = g.Count(dc => dc.DocumentStatus == DocStatus.Viewed)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (conversionData == null || conversionData.Total == 0)
                return (0, 0, 0, 0);

            var sentToViewed = conversionData.Sent > 0 ? (conversionData.Viewed * 100.0 / conversionData.Sent) : 0;
            var viewedToSigned = conversionData.Viewed > 0 ? (conversionData.Signed * 100.0 / conversionData.Viewed) : 0;
            var createdToSigned = conversionData.Total > 0 ? (conversionData.Signed * 100.0 / conversionData.Total) : 0;
            var abandonmentRate = conversionData.Viewed > 0 ? (conversionData.ViewedButNotSigned * 100.0 / conversionData.Viewed) : 0;

            return (sentToViewed, viewedToSigned, createdToSigned, abandonmentRate);
        }

        private TrendIndicator CalculateTrend(double current, double previous)
        {
            if (previous == 0)
                return new TrendIndicator { Value = 0, Direction = "stable", IsGood = true };

            var percentChange = ((current - previous) / previous) * 100;
            var direction = Math.Abs(percentChange) < 1 ? "stable" :
                           percentChange > 0 ? "up" : "down";

            return new TrendIndicator
            {
                Value = percentChange,
                Direction = direction,
                IsGood = direction == "up" || direction == "stable" // Generally up is good for most metrics
            };
        }
    }

    /// <summary>
    /// Interface for analytics repository
    /// </summary>
    public interface IAnalyticsRepository
    {
        Task<DashboardKPIs> GetKPIMetricsAsync(TimeWindow window, CancellationToken cancellationToken);
        Task<UsageAnalytics> GetUsageAnalyticsAsync(TimeWindow window, CancellationToken cancellationToken);
        Task<SegmentationData> GetSegmentationDataAsync(TimeWindow window, CancellationToken cancellationToken);
        Task<ProcessFlowData> GetProcessFlowDataAsync(TimeWindow window, CancellationToken cancellationToken);
        Task<PlatformHealthData> GetPlatformHealthDataAsync(TimeWindow window, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Query budget tracker for performance monitoring
    /// </summary>
    public interface IQueryBudgetTracker
    {
        IQueryBudget StartQuery(string queryName, TimeSpan maxDuration);
    }

    public interface IQueryBudget : IDisposable
    {
        double ElapsedMilliseconds { get; }
    }

    /// <summary>
    /// Platform health data model
    /// </summary>
    public class PlatformHealthData
    {
        public double ApiErrorRate { get; set; }
        public double ApiLatencyP95 { get; set; }
        public int DatabaseConnectionCount { get; set; }
        public string SystemHealth { get; set; }
    }
}