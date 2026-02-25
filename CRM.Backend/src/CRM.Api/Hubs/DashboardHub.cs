// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CRM.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time dashboard updates.
/// Enables live metric updates without page refreshes.
/// TODO-RPT-06
/// </summary>
[Authorize]
public class DashboardHub : Hub
{
    private readonly ILogger<DashboardHub> _logger;

    // Track subscribed dashboards per connection
    private static readonly Dictionary<string, HashSet<string>> _connectionDashboards = new();
    private static readonly object _lock = new();

    public DashboardHub(ILogger<DashboardHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _logger.LogInformation(
            "User {UserId} connected to Dashboard hub. ConnectionId: {ConnectionId}",
            userId, Context.ConnectionId);

        lock (_lock)
        {
            _connectionDashboards[Context.ConnectionId] = new HashSet<string>();
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        _logger.LogInformation(
            "User {UserId} disconnected from Dashboard hub. ConnectionId: {ConnectionId}",
            userId, Context.ConnectionId);

        lock (_lock)
        {
            _connectionDashboards.Remove(Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to real-time updates for a specific dashboard.
    /// </summary>
    /// <param name="dashboardId">The ID of the dashboard to subscribe to.</param>
    public async Task SubscribeToDashboard(string dashboardId)
    {
        var groupName = GetDashboardGroupName(dashboardId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        lock (_lock)
        {
            if (_connectionDashboards.TryGetValue(Context.ConnectionId, out var dashboards))
            {
                dashboards.Add(dashboardId);
            }
        }

        _logger.LogDebug(
            "Connection {ConnectionId} subscribed to dashboard {DashboardId}",
            Context.ConnectionId, dashboardId);

        await Clients.Caller.SendAsync("SubscriptionConfirmed", new
        {
            DashboardId = dashboardId,
            Status = "subscribed",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Unsubscribe from a dashboard's real-time updates.
    /// </summary>
    /// <param name="dashboardId">The ID of the dashboard to unsubscribe from.</param>
    public async Task UnsubscribeFromDashboard(string dashboardId)
    {
        var groupName = GetDashboardGroupName(dashboardId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        lock (_lock)
        {
            if (_connectionDashboards.TryGetValue(Context.ConnectionId, out var dashboards))
            {
                dashboards.Remove(dashboardId);
            }
        }

        _logger.LogDebug(
            "Connection {ConnectionId} unsubscribed from dashboard {DashboardId}",
            Context.ConnectionId, dashboardId);
    }

    /// <summary>
    /// Subscribe to all dashboard updates (for admin/monitoring).
    /// </summary>
    public async Task SubscribeToAllDashboards()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "all-dashboards");

        _logger.LogDebug(
            "Connection {ConnectionId} subscribed to all dashboard updates",
            Context.ConnectionId);
    }

    /// <summary>
    /// Request an immediate metric refresh for a dashboard.
    /// </summary>
    /// <param name="dashboardId">The ID of the dashboard to refresh.</param>
    public async Task RequestMetricRefresh(string dashboardId)
    {
        _logger.LogDebug(
            "Metric refresh requested for dashboard {DashboardId} by {ConnectionId}",
            dashboardId, Context.ConnectionId);

        // Notify server-side to trigger a refresh (could be handled by a background service)
        await Clients.Caller.SendAsync("RefreshRequested", new
        {
            DashboardId = dashboardId,
            RequestedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Broadcast a metric update to all subscribers of a dashboard.
    /// Called by server-side services when metrics change.
    /// </summary>
    /// <param name="dashboardId">The dashboard ID.</param>
    /// <param name="metric">The metric update data.</param>
    public async Task BroadcastMetricUpdate(string dashboardId, DashboardMetricUpdate metric)
    {
        var groupName = GetDashboardGroupName(dashboardId);

        await Clients.Group(groupName).SendAsync("MetricUpdated", new
        {
            DashboardId = dashboardId,
            Metric = metric,
            Timestamp = DateTime.UtcNow
        });

        // Also notify all-dashboards subscribers
        await Clients.Group("all-dashboards").SendAsync("MetricUpdated", new
        {
            DashboardId = dashboardId,
            Metric = metric,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogDebug(
            "Broadcast metric update: Dashboard {DashboardId}, Metric {MetricName}",
            dashboardId, metric.MetricName);
    }

    /// <summary>
    /// Broadcast a widget data update to dashboard subscribers.
    /// </summary>
    /// <param name="dashboardId">The dashboard ID.</param>
    /// <param name="widgetId">The widget ID.</param>
    /// <param name="data">The updated widget data.</param>
    public async Task BroadcastWidgetUpdate(string dashboardId, string widgetId, object data)
    {
        var groupName = GetDashboardGroupName(dashboardId);

        await Clients.Group(groupName).SendAsync("WidgetUpdated", new
        {
            DashboardId = dashboardId,
            WidgetId = widgetId,
            Data = data,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogDebug(
            "Broadcast widget update: Dashboard {DashboardId}, Widget {WidgetId}",
            dashboardId, widgetId);
    }

    /// <summary>
    /// Broadcast an alert to dashboard subscribers.
    /// </summary>
    /// <param name="dashboardId">The dashboard ID.</param>
    /// <param name="alert">The alert data.</param>
    public async Task BroadcastAlert(string dashboardId, DashboardAlert alert)
    {
        var groupName = GetDashboardGroupName(dashboardId);

        await Clients.Group(groupName).SendAsync("AlertReceived", new
        {
            DashboardId = dashboardId,
            Alert = alert,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Broadcast alert to dashboard {DashboardId}: {AlertType} - {Message}",
            dashboardId, alert.AlertType, alert.Message);
    }

    private string GetUserId()
    {
        return Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("userId")?.Value
            ?? "unknown";
    }

    private static string GetDashboardGroupName(string dashboardId)
    {
        return $"dashboard:{dashboardId}";
    }
}

/// <summary>
/// Represents a metric update for a dashboard.
/// </summary>
public class DashboardMetricUpdate
{
    public string MetricName { get; set; } = string.Empty;
    public string MetricLabel { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? PreviousValue { get; set; }
    public decimal? ChangePercent { get; set; }
    public string? Trend { get; set; }
    public string? Format { get; set; }
}

/// <summary>
/// Represents an alert for a dashboard.
/// </summary>
public class DashboardAlert
{
    public string AlertType { get; set; } = "info"; // info, warning, error, success
    public string Message { get; set; } = string.Empty;
    public string? MetricName { get; set; }
    public decimal? ThresholdValue { get; set; }
    public decimal? ActualValue { get; set; }
    public bool RequiresAcknowledgment { get; set; }
}

/// <summary>
/// Service interface for broadcasting dashboard updates.
/// </summary>
public interface IDashboardHubService
{
    Task BroadcastMetricUpdateAsync(string dashboardId, DashboardMetricUpdate metric);
    Task BroadcastWidgetUpdateAsync(string dashboardId, string widgetId, object data);
    Task BroadcastAlertAsync(string dashboardId, DashboardAlert alert);
}

/// <summary>
/// Service implementation for broadcasting dashboard updates from backend services.
/// </summary>
public class DashboardHubService : IDashboardHubService
{
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ILogger<DashboardHubService> _logger;

    public DashboardHubService(
        IHubContext<DashboardHub> hubContext,
        ILogger<DashboardHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastMetricUpdateAsync(string dashboardId, DashboardMetricUpdate metric)
    {
        var groupName = $"dashboard:{dashboardId}";

        await _hubContext.Clients.Group(groupName).SendAsync("MetricUpdated", new
        {
            DashboardId = dashboardId,
            Metric = metric,
            Timestamp = DateTime.UtcNow
        });

        await _hubContext.Clients.Group("all-dashboards").SendAsync("MetricUpdated", new
        {
            DashboardId = dashboardId,
            Metric = metric,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task BroadcastWidgetUpdateAsync(string dashboardId, string widgetId, object data)
    {
        var groupName = $"dashboard:{dashboardId}";

        await _hubContext.Clients.Group(groupName).SendAsync("WidgetUpdated", new
        {
            DashboardId = dashboardId,
            WidgetId = widgetId,
            Data = data,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task BroadcastAlertAsync(string dashboardId, DashboardAlert alert)
    {
        var groupName = $"dashboard:{dashboardId}";

        await _hubContext.Clients.Group(groupName).SendAsync("AlertReceived", new
        {
            DashboardId = dashboardId,
            Alert = alert,
            Timestamp = DateTime.UtcNow
        });
    }
}
