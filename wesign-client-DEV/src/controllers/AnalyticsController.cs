using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeSign.Analytics.Models;
using WeSign.Analytics.Services;
using WeSign.Controllers.Base;
using WeSign.Models.Analytics;

namespace WeSign.Controllers.Api
{
    /// <summary>
    /// High-performance analytics API for WeSign real-time dashboard
    /// Provides role-based access with PII protection and caching
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires JWT authentication
    public class AnalyticsController : BaseApiController
    {
        private readonly IAnalyticsApiService _analyticsService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AnalyticsController> _logger;
        private readonly IRoleBasedFilterService _roleFilterService;

        public AnalyticsController(
            IAnalyticsApiService analyticsService,
            IMemoryCache memoryCache,
            ILogger<AnalyticsController> logger,
            IRoleBasedFilterService roleFilterService)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _roleFilterService = roleFilterService ?? throw new ArgumentNullException(nameof(roleFilterService));
        }

        /// <summary>
        /// Get latest KPI snapshot (≤30KB, cached for 15s)
        /// This is the primary endpoint for dashboard tiles
        /// </summary>
        [HttpGet("kpi/latest")]
        [ResponseCache(Duration = 15, VaryByHeader = "Authorization", Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<LatestKPIsResponse>> GetLatestKPIs(CancellationToken cancellationToken)
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();
                var cacheKey = $"latest-kpis-{userRole}-{userId}";

                _logger.LogDebug("Getting latest KPIs for user {userId} with role {role}", userId, userRole);

                // Try L1 cache first (15-second TTL)
                if (_memoryCache.TryGetValue(cacheKey, out LatestKPIsResponse cached))
                {
                    _logger.LogDebug("Returning cached KPIs for {role}", userRole);
                    return Ok(cached);
                }

                // Get fresh data from service
                var kpis = await _analyticsService.GetLatestKPIsAsync(userRole, userId, cancellationToken);

                // Apply role-based filtering for PII protection
                var filteredKpis = _roleFilterService.ApplyRoleFilter(kpis, userRole, userId);

                var response = new LatestKPIsResponse
                {
                    Data = filteredKpis,
                    Timestamp = DateTime.UtcNow,
                    CacheAge = 0,
                    DataQuality = "fresh",
                    Metadata = new ResponseMetadata
                    {
                        QueryDuration = 0, // Will be set by service
                        Version = "1.0",
                        UserRole = userRole
                    }
                };

                // Cache for 15 seconds
                _memoryCache.Set(cacheKey, response, TimeSpan.FromSeconds(15));

                _logger.LogInformation("Served fresh KPIs to {role} user {userId}", userRole, userId);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for analytics");
                return Forbid("Insufficient permissions for analytics access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get latest KPIs");
                return StatusCode(500, new { error = "Failed to retrieve analytics data" });
            }
        }

        /// <summary>
        /// Get time series data with advanced filtering and aggregation
        /// Supports role-based data access and organizational filtering
        /// </summary>
        [HttpGet("kpi/series")]
        public async Task<ActionResult<SeriesResponse>> GetKPISeries(
            [FromQuery] SeriesRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate request parameters
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userRole = GetUserRole();
                var userId = GetUserId();

                _logger.LogDebug("Getting time series for {metric} from {from} to {to} for {role}",
                    request.Metric, request.From, request.To, userRole);

                // Apply role-based request filtering
                var filteredRequest = _roleFilterService.ApplyRoleFilter(request, userRole, userId);

                // Get data from service
                var series = await _analyticsService.GetTimeSeriesAsync(filteredRequest, cancellationToken);

                var response = new SeriesResponse
                {
                    Series = series,
                    Meta = new SeriesMetadata
                    {
                        Granularity = filteredRequest.Granularity,
                        Filters = filteredRequest,
                        TotalPoints = series.Sum(s => s.Points.Count),
                        UserRole = userRole
                    }
                };

                _logger.LogInformation("Served {points} time series points to {role} user",
                    response.Meta.TotalPoints, userRole);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid series request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized series access attempt");
                return Forbid("Insufficient permissions for this data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get time series data");
                return StatusCode(500, new { error = "Failed to retrieve time series data" });
            }
        }

        /// <summary>
        /// Get stuck documents analysis with role-based PII protection
        /// PM role: Document IDs are hashed, no signer information
        /// Support role: Limited PII access with audit logging
        /// </summary>
        [HttpGet("kpi/stuck")]
        [Authorize(Roles = "ProductManager,Support,Operations")]
        public async Task<ActionResult<StuckDocumentsResponse>> GetStuckDocuments(
            [FromQuery] StuckDocumentsRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();

                _logger.LogDebug("Getting stuck documents for {role} user with threshold {hours}h",
                    userRole, request.ThresholdHours);

                // Audit log for Support/Operations access
                if (userRole is "Support" or "Operations")
                {
                    _logger.LogWarning("PII access: User {userId} ({role}) accessing stuck documents",
                        userId, userRole);
                }

                var stuckDocs = await _analyticsService.GetStuckDocumentsAsync(
                    request, userRole, userId, cancellationToken);

                var response = new StuckDocumentsResponse
                {
                    Documents = stuckDocs,
                    TotalCount = stuckDocs.Count,
                    ThresholdHours = request.ThresholdHours,
                    GeneratedAt = DateTime.UtcNow,
                    UserRole = userRole
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized stuck documents access");
                return Forbid("Insufficient permissions for stuck documents data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get stuck documents");
                return StatusCode(500, new { error = "Failed to retrieve stuck documents" });
            }
        }

        /// <summary>
        /// Health endpoint for monitoring data freshness and system status
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous] // Allow monitoring systems to check health
        public async Task<ActionResult<HealthResponse>> GetHealth(CancellationToken cancellationToken)
        {
            try
            {
                var health = await _analyticsService.GetHealthStatusAsync(cancellationToken);

                var status = health.DataAgeSeconds <= 30 ? "healthy" :
                           health.DataAgeSeconds <= 90 ? "warning" : "critical";

                var response = new HealthResponse
                {
                    Status = status,
                    DataAgeSeconds = health.DataAgeSeconds,
                    CacheState = health.CacheState,
                    S3ReadLatencyMs = health.S3ReadLatencyMs,
                    LastWatermark = health.LastWatermark,
                    SystemComponents = health.SystemComponents,
                    Timestamp = DateTime.UtcNow
                };

                // Return appropriate HTTP status based on health
                return status switch
                {
                    "healthy" => Ok(response),
                    "warning" => StatusCode(200, response), // Still 200 but with warning
                    "critical" => StatusCode(503, response), // Service unavailable
                    _ => StatusCode(500, response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get health status");
                return StatusCode(500, new HealthResponse
                {
                    Status = "error",
                    DataAgeSeconds = -1,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Export analytics data in various formats (CSV, Excel, PDF)
        /// Role-based data filtering applies to exports
        /// </summary>
        [HttpPost("export")]
        [Authorize(Roles = "ProductManager,Support,Operations")]
        public async Task<ActionResult> ExportAnalytics(
            [FromBody] ExportRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();

                _logger.LogInformation("Exporting analytics data as {format} for {role} user",
                    request.Format, userRole);

                // Apply role-based filtering to export request
                var filteredRequest = _roleFilterService.ApplyRoleFilter(request, userRole, userId);

                var exportResult = await _analyticsService.ExportAnalyticsAsync(
                    filteredRequest, cancellationToken);

                var contentType = request.Format.ToLowerInvariant() switch
                {
                    "csv" => "text/csv",
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "pdf" => "application/pdf",
                    _ => "application/octet-stream"
                };

                var fileName = $"wesign-analytics-{DateTime.UtcNow:yyyy-MM-dd-HHmm}.{request.Format.ToLowerInvariant()}";

                return File(exportResult.Data, contentType, fileName);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid export request");
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized export attempt");
                return Forbid("Insufficient permissions for data export");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export analytics data");
                return StatusCode(500, new { error = "Failed to export data" });
            }
        }

        /// <summary>
        /// Server-Sent Events endpoint for real-time updates
        /// Pushes delta updates every 30 seconds
        /// </summary>
        [HttpGet("stream")]
        [Authorize(Roles = "ProductManager,Support,Operations")]
        public async Task StreamAnalytics(CancellationToken cancellationToken)
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();

                _logger.LogInformation("Starting analytics stream for {role} user {userId}", userRole, userId);

                Response.Headers.Add("Content-Type", "text/event-stream");
                Response.Headers.Add("Cache-Control", "no-cache");
                Response.Headers.Add("Connection", "keep-alive");
                Response.Headers.Add("Access-Control-Allow-Origin", "*");

                // Send initial connection event
                await WriteSSEEventAsync("connected", new { userRole, timestamp = DateTime.UtcNow });

                await foreach (var update in _analyticsService.GetRealTimeUpdatesAsync(userRole, userId, cancellationToken))
                {
                    // Apply role-based filtering to real-time updates
                    var filteredUpdate = _roleFilterService.ApplyRoleFilter(update, userRole, userId);

                    await WriteSSEEventAsync("update", filteredUpdate);
                    await Response.Body.FlushAsync(cancellationToken);

                    // 30-second intervals as per PRD
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Analytics stream cancelled for user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in analytics stream");
                await WriteSSEEventAsync("error", new { message = "Stream error occurred" });
            }
        }

        // Helper methods

        private string GetUserRole()
        {
            return User.FindFirst("role")?.Value ??
                   User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value ??
                   throw new UnauthorizedAccessException("User role not found in token");
        }

        private string GetUserId()
        {
            return User.FindFirst("sub")?.Value ??
                   User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ??
                   throw new UnauthorizedAccessException("User ID not found in token");
        }

        private async Task WriteSSEEventAsync(string eventType, object data)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var sseData = $"event: {eventType}\ndata: {json}\n\n";
            await Response.WriteAsync(sseData);
        }
    }

    // Request/Response Models

    /// <summary>
    /// Request model for time series data
    /// </summary>
    public class SeriesRequest
    {
        [Required]
        public string[] Metrics { get; set; } = Array.Empty<string>();

        [Required]
        public DateTime From { get; set; }

        [Required]
        public DateTime To { get; set; }

        public string Granularity { get; set; } = "5m";

        public string OrganizationId { get; set; }

        public string TemplateId { get; set; }

        public string DeviceType { get; set; }

        public string Channel { get; set; }
    }

    /// <summary>
    /// Request model for stuck documents
    /// </summary>
    public class StuckDocumentsRequest
    {
        public DateTime? Since { get; set; }

        [Range(1, 168)] // 1 hour to 1 week
        public int ThresholdHours { get; set; } = 24;

        public string OrganizationId { get; set; }

        public string TemplateId { get; set; }
    }

    /// <summary>
    /// Request model for data export
    /// </summary>
    public class ExportRequest
    {
        [Required]
        public string Format { get; set; } // csv, excel, pdf

        [Required]
        public AnalyticsFilterRequest Filters { get; set; }

        public string[] Metrics { get; set; } = Array.Empty<string>();

        public bool IncludeRawData { get; set; } = false;
    }

    /// <summary>
    /// Response model for latest KPIs
    /// </summary>
    public class LatestKPIsResponse : AnalyticsApiResponse<DashboardKPIs>
    {
        public string UserRole { get; set; }
    }

    /// <summary>
    /// Response model for time series data
    /// </summary>
    public class SeriesResponse
    {
        public List<TimeSeries> Series { get; set; } = new();
        public SeriesMetadata Meta { get; set; }
    }

    /// <summary>
    /// Time series data structure
    /// </summary>
    public class TimeSeries
    {
        public string Metric { get; set; }
        public List<TimeSeriesPoint> Points { get; set; } = new();
    }

    /// <summary>
    /// Metadata for series response
    /// </summary>
    public class SeriesMetadata
    {
        public string Granularity { get; set; }
        public SeriesRequest Filters { get; set; }
        public int TotalPoints { get; set; }
        public string UserRole { get; set; }
    }

    /// <summary>
    /// Response model for stuck documents
    /// </summary>
    public class StuckDocumentsResponse
    {
        public List<StuckDocumentInfo> Documents { get; set; } = new();
        public int TotalCount { get; set; }
        public int ThresholdHours { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string UserRole { get; set; }
    }

    /// <summary>
    /// Health response model
    /// </summary>
    public class HealthResponse
    {
        public string Status { get; set; }
        public double DataAgeSeconds { get; set; }
        public string CacheState { get; set; }
        public double S3ReadLatencyMs { get; set; }
        public DateTime? LastWatermark { get; set; }
        public Dictionary<string, string> SystemComponents { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Export result model
    /// </summary>
    public class ExportResult
    {
        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}