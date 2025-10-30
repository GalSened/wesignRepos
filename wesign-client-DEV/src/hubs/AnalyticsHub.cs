using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeSign.Analytics.Models;
using WeSign.Analytics.Services;

namespace WeSign.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time analytics updates
    /// Provides role-based streaming with PII protection
    /// </summary>
    [Authorize] // Requires JWT authentication
    public class AnalyticsHub : Hub
    {
        private readonly IAnalyticsApiService _analyticsService;
        private readonly IRoleBasedFilterService _roleFilterService;
        private readonly ILogger<AnalyticsHub> _logger;

        // Track connected users by role for efficient broadcasting
        private static readonly ConcurrentDictionary<string, HashSet<string>> RoleGroups = new();
        private static readonly ConcurrentDictionary<string, UserConnection> ConnectedUsers = new();

        public AnalyticsHub(
            IAnalyticsApiService analyticsService,
            IRoleBasedFilterService roleFilterService,
            ILogger<AnalyticsHub> logger)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            _roleFilterService = roleFilterService ?? throw new ArgumentNullException(nameof(roleFilterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Join analytics stream with role-based groups
        /// </summary>
        public async Task JoinAnalyticsStream()
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();
                var connectionId = Context.ConnectionId;

                _logger.LogInformation("User {userId} ({role}) joining analytics stream", userId, userRole);

                // Validate role permissions
                if (!IsAuthorizedForAnalytics(userRole))
                {
                    _logger.LogWarning("Unauthorized analytics access attempt by {userId} with role {role}",
                        userId, userRole);
                    await Clients.Caller.SendAsync("Error", "Insufficient permissions for analytics access");
                    Context.Abort();
                    return;
                }

                // Add to role-based group
                var groupName = $"analytics-{userRole}";
                await Groups.AddToGroupAsync(connectionId, groupName);

                // Track connection
                var userConnection = new UserConnection
                {
                    ConnectionId = connectionId,
                    UserId = userId,
                    UserRole = userRole,
                    ConnectedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow
                };

                ConnectedUsers.TryAdd(connectionId, userConnection);
                RoleGroups.AddOrUpdate(userRole,
                    new HashSet<string> { connectionId },
                    (key, existing) =>
                    {
                        existing.Add(connectionId);
                        return existing;
                    });

                // Send initial data
                try
                {
                    var initialData = await _analyticsService.GetLatestKPIsAsync(userRole, userId, CancellationToken.None);
                    var filteredData = _roleFilterService.ApplyRoleFilter(initialData, userRole, userId);

                    await Clients.Caller.SendAsync("InitialData", new
                    {
                        type = "initial",
                        data = filteredData,
                        timestamp = DateTime.UtcNow,
                        userRole = userRole
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send initial data to {userId}", userId);
                    await Clients.Caller.SendAsync("Error", "Failed to load initial analytics data");
                }

                // Send connection confirmation
                await Clients.Caller.SendAsync("Connected", new
                {
                    message = "Connected to analytics stream",
                    userRole = userRole,
                    refreshInterval = 30, // seconds
                    timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("User {userId} successfully joined analytics stream", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during analytics stream join");
                await Clients.Caller.SendAsync("Error", "Failed to join analytics stream");
            }
        }

        /// <summary>
        /// Leave analytics stream
        /// </summary>
        public async Task LeaveAnalyticsStream()
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();
                var connectionId = Context.ConnectionId;

                _logger.LogInformation("User {userId} ({role}) leaving analytics stream", userId, userRole);

                var groupName = $"analytics-{userRole}";
                await Groups.RemoveFromGroupAsync(connectionId, groupName);

                // Remove from tracking
                RemoveConnection(connectionId);

                await Clients.Caller.SendAsync("Disconnected", new
                {
                    message = "Disconnected from analytics stream",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during analytics stream leave");
            }
        }

        /// <summary>
        /// Subscribe to specific metric updates
        /// </summary>
        public async Task SubscribeToMetric(string metricName)
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();

                _logger.LogDebug("User {userId} subscribing to metric {metric}", userId, metricName);

                // Validate metric access based on role
                if (!_roleFilterService.IsMetricAuthorized(metricName, userRole))
                {
                    await Clients.Caller.SendAsync("Error", $"Access denied for metric: {metricName}");
                    return;
                }

                var metricGroupName = $"metric-{metricName}-{userRole}";
                await Groups.AddToGroupAsync(Context.ConnectionId, metricGroupName);

                await Clients.Caller.SendAsync("MetricSubscribed", new
                {
                    metric = metricName,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to metric {metric}", metricName);
                await Clients.Caller.SendAsync("Error", "Failed to subscribe to metric");
            }
        }

        /// <summary>
        /// Update user activity timestamp
        /// </summary>
        public async Task Heartbeat()
        {
            try
            {
                if (ConnectedUsers.TryGetValue(Context.ConnectionId, out var connection))
                {
                    connection.LastActivity = DateTime.UtcNow;
                }

                await Clients.Caller.SendAsync("HeartbeatAck", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing heartbeat");
            }
        }

        /// <summary>
        /// Handle disconnection
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                var userId = ConnectedUsers.TryGetValue(connectionId, out var connection) ? connection.UserId : "unknown";

                _logger.LogInformation("User {userId} disconnected from analytics stream", userId);

                RemoveConnection(connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnection cleanup");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Static methods for broadcasting updates from background services

        /// <summary>
        /// Broadcast real-time updates to all connected users (called by background service)
        /// </summary>
        public static async Task BroadcastAnalyticsUpdate(
            IHubContext<AnalyticsHub> hubContext,
            IRoleBasedFilterService roleFilterService,
            AnalyticsSnapshot snapshot,
            ILogger logger)
        {
            try
            {
                var updateData = new
                {
                    type = "realtime-update",
                    timestamp = snapshot.Timestamp,
                    dataAge = snapshot.DataAge,
                    kpis = new
                    {
                        documentsCreated = snapshot.KPIs.DocumentsCreated,
                        documentsSent = snapshot.KPIs.DocumentsSent,
                        documentsOpened = snapshot.KPIs.DocumentsOpened,
                        documentsSigned = snapshot.KPIs.DocumentsSigned,
                        dau = snapshot.KPIs.DailyActiveUsers,
                        conversionRate = snapshot.KPIs.OverallSuccessRate
                    },
                    delta = new
                    {
                        // Calculate deltas from previous values (would be stored)
                        documentsCreated = 0, // Placeholder
                        documentsSigned = 0   // Placeholder
                    }
                };

                // Broadcast to each role group with appropriate filtering
                var roleTasks = RoleGroups.Keys.Select(async role =>
                {
                    try
                    {
                        var filteredUpdate = roleFilterService.ApplyRoleFilter(updateData, role, null);
                        await hubContext.Clients.Group($"analytics-{role}")
                            .SendAsync("RealTimeUpdate", filteredUpdate);

                        logger.LogDebug("Broadcasted update to {role} group", role);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to broadcast to {role} group", role);
                    }
                });

                await Task.WhenAll(roleTasks);

                logger.LogInformation("Broadcasted analytics update to {groupCount} role groups", RoleGroups.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to broadcast analytics update");
            }
        }

        /// <summary>
        /// Broadcast metric-specific updates
        /// </summary>
        public static async Task BroadcastMetricUpdate(
            IHubContext<AnalyticsHub> hubContext,
            string metricName,
            object metricData,
            ILogger logger)
        {
            try
            {
                var updateData = new
                {
                    type = "metric-update",
                    metric = metricName,
                    data = metricData,
                    timestamp = DateTime.UtcNow
                };

                // Broadcast to metric-specific groups
                var roles = new[] { "ProductManager", "Support", "Operations" };
                var tasks = roles.Select(async role =>
                {
                    var groupName = $"metric-{metricName}-{role}";
                    await hubContext.Clients.Group(groupName).SendAsync("MetricUpdate", updateData);
                });

                await Task.WhenAll(tasks);

                logger.LogDebug("Broadcasted {metric} update", metricName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to broadcast metric update for {metric}", metricName);
            }
        }

        /// <summary>
        /// Get connection statistics
        /// </summary>
        public static ConnectionStats GetConnectionStats()
        {
            var now = DateTime.UtcNow;
            var activeConnections = ConnectedUsers.Values
                .Where(c => (now - c.LastActivity).TotalMinutes < 5)
                .ToList();

            return new ConnectionStats
            {
                TotalConnections = ConnectedUsers.Count,
                ActiveConnections = activeConnections.Count,
                ConnectionsByRole = activeConnections.GroupBy(c => c.UserRole)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageConnectionDuration = activeConnections.Any() ?
                    activeConnections.Average(c => (now - c.ConnectedAt).TotalMinutes) : 0
            };
        }

        // Private helper methods

        private string GetUserRole()
        {
            return Context.User?.FindFirst("role")?.Value ??
                   Context.User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value ??
                   throw new UnauthorizedAccessException("User role not found");
        }

        private string GetUserId()
        {
            return Context.User?.FindFirst("sub")?.Value ??
                   Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ??
                   throw new UnauthorizedAccessException("User ID not found");
        }

        private static bool IsAuthorizedForAnalytics(string role)
        {
            return role is "ProductManager" or "Support" or "Operations";
        }

        private static void RemoveConnection(string connectionId)
        {
            if (ConnectedUsers.TryRemove(connectionId, out var connection))
            {
                // Remove from role groups
                if (RoleGroups.TryGetValue(connection.UserRole, out var roleGroup))
                {
                    roleGroup.Remove(connectionId);
                    if (roleGroup.Count == 0)
                    {
                        RoleGroups.TryRemove(connection.UserRole, out _);
                    }
                }
            }
        }
    }

    /// <summary>
    /// User connection tracking
    /// </summary>
    public class UserConnection
    {
        public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public string UserRole { get; set; }
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
    }

    /// <summary>
    /// Connection statistics
    /// </summary>
    public class ConnectionStats
    {
        public int TotalConnections { get; set; }
        public int ActiveConnections { get; set; }
        public Dictionary<string, int> ConnectionsByRole { get; set; } = new();
        public double AverageConnectionDuration { get; set; }
    }
}