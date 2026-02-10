// CRM Solution - Custom Dashboard Builder Service
// Phase 7, Task 7.4 - Drag-and-drop dashboard widget configuration with data from CRM entities

using System.Collections.Concurrent;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

#region Interfaces and DTOs

/// <summary>
/// Service for managing custom user dashboards with configurable widgets.
/// Supports CRUD operations on dashboards and widgets, plus widget data retrieval.
/// </summary>
public interface IDashboardBuilderService
{
    /// <summary>
    /// Gets all dashboards for a user.
    /// </summary>
    /// <param name="userId">Owner user ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of user dashboards.</returns>
    Task<IEnumerable<CustomDashboard>> GetDashboardsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Gets a specific dashboard by ID.
    /// </summary>
    /// <param name="dashboardId">Dashboard ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dashboard details, or null if not found.</returns>
    Task<CustomDashboard?> GetDashboardAsync(string dashboardId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new dashboard.
    /// </summary>
    /// <param name="dashboard">Dashboard definition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created dashboard with assigned ID.</returns>
    Task<CustomDashboard> CreateDashboardAsync(CustomDashboard dashboard, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing dashboard.
    /// </summary>
    /// <param name="dashboard">Updated dashboard definition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated dashboard, or null if not found.</returns>
    Task<CustomDashboard?> UpdateDashboardAsync(CustomDashboard dashboard, CancellationToken ct = default);

    /// <summary>
    /// Deletes a dashboard.
    /// </summary>
    /// <param name="dashboardId">Dashboard ID to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteDashboardAsync(string dashboardId, CancellationToken ct = default);

    /// <summary>
    /// Gets live data for a specific widget.
    /// </summary>
    /// <param name="widgetId">Widget ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Widget data payload, or null if widget not found.</returns>
    Task<WidgetData?> GetWidgetDataAsync(string widgetId, CancellationToken ct = default);

    /// <summary>
    /// Returns the catalog of available widget types.
    /// </summary>
    /// <returns>List of widget templates available for drag-and-drop.</returns>
    IEnumerable<WidgetTemplate> GetAvailableWidgets();
}

/// <summary>
/// A custom user dashboard containing a layout of widgets.
/// </summary>
public class CustomDashboard
{
    /// <summary>Unique dashboard identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Dashboard display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Owner user ID.</summary>
    public int UserId { get; set; }

    /// <summary>Whether this is the user's default dashboard.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Widgets on the dashboard.</summary>
    public List<DashboardWidget> Widgets { get; set; } = new();

    /// <summary>Dashboard created timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Dashboard last updated timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// A widget placed on a dashboard with position and sizing.
/// </summary>
public class DashboardWidget
{
    /// <summary>Unique widget instance identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Widget type (e.g., "pipeline-chart", "lead-summary", "recent-activities").</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Column position (0-based grid).</summary>
    public int Column { get; set; }

    /// <summary>Row position (0-based grid).</summary>
    public int Row { get; set; }

    /// <summary>Width in grid units (1-12).</summary>
    public int Width { get; set; } = 4;

    /// <summary>Height in grid units.</summary>
    public int Height { get; set; } = 2;

    /// <summary>Optional configuration (filters, date range, etc.).</summary>
    public Dictionary<string, object> Config { get; set; } = new();
}

/// <summary>
/// Catalog entry for a widget type that users can add to dashboards.
/// </summary>
public class WidgetTemplate
{
    /// <summary>Widget type identifier.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Human-readable name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Category for grouping (Sales, Marketing, Support, etc.).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Widget description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Default width in grid units.</summary>
    public int DefaultWidth { get; set; } = 4;

    /// <summary>Default height in grid units.</summary>
    public int DefaultHeight { get; set; } = 2;
}

/// <summary>
/// Data payload for a widget, fetched at display time.
/// </summary>
public class WidgetData
{
    /// <summary>Widget ID.</summary>
    public string WidgetId { get; set; } = string.Empty;

    /// <summary>Widget type.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Data payload (varies by widget type).</summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>When the data was fetched.</summary>
    public DateTime FetchedAt { get; set; }
}

#endregion

/// <summary>
/// In-memory dashboard builder service.
/// Stores dashboard configurations in ConcurrentDictionary and queries CRM data for widget content.
/// </summary>
public class DashboardBuilderService : IDashboardBuilderService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<DashboardBuilderService> _logger;

    // In-memory storage: DashboardId → Dashboard
    private readonly ConcurrentDictionary<string, CustomDashboard> _dashboards = new();

    // Widget → Dashboard reverse lookup
    private readonly ConcurrentDictionary<string, string> _widgetToDashboard = new();

    private static int _idCounter;

    private static readonly List<WidgetTemplate> WidgetCatalog = new()
    {
        new() { Type = "pipeline-chart",     Name = "Pipeline Chart",        Category = "Sales",     Description = "Opportunities by stage as a bar chart.",             DefaultWidth = 6, DefaultHeight = 3 },
        new() { Type = "lead-summary",       Name = "Lead Summary",          Category = "Sales",     Description = "Lead count grouped by status.",                      DefaultWidth = 4, DefaultHeight = 2 },
        new() { Type = "revenue-kpi",        Name = "Revenue KPI",           Category = "Sales",     Description = "Total opportunity revenue as a metric card.",         DefaultWidth = 3, DefaultHeight = 1 },
        new() { Type = "recent-activities",  Name = "Recent Activities",     Category = "General",   Description = "List of most recent CRM activities.",                DefaultWidth = 6, DefaultHeight = 3 },
        new() { Type = "account-count",      Name = "Account Count",         Category = "General",   Description = "Total active accounts counter.",                     DefaultWidth = 3, DefaultHeight = 1 },
        new() { Type = "open-tickets",       Name = "Open Tickets",          Category = "Support",   Description = "Open service request count.",                        DefaultWidth = 3, DefaultHeight = 1 },
        new() { Type = "deal-velocity",      Name = "Deal Velocity",         Category = "Sales",     Description = "Average days to close for won deals.",               DefaultWidth = 3, DefaultHeight = 1 },
        new() { Type = "top-accounts",       Name = "Top Accounts",          Category = "General",   Description = "Accounts with highest opportunity value.",           DefaultWidth = 4, DefaultHeight = 3 },
        new() { Type = "conversion-rate",    Name = "Conversion Rate",       Category = "Marketing", Description = "Lead-to-opportunity conversion percentage.",         DefaultWidth = 3, DefaultHeight = 1 },
        new() { Type = "custom-list",        Name = "Custom Entity List",    Category = "General",   Description = "Configurable list of records with filters.",         DefaultWidth = 6, DefaultHeight = 3 },
    };

    /// <summary>
    /// Initializes a new instance of DashboardBuilderService.
    /// </summary>
    public DashboardBuilderService(ICrmDbContext context, ILogger<DashboardBuilderService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<IEnumerable<CustomDashboard>> GetDashboardsAsync(int userId, CancellationToken ct = default)
    {
        var dashboards = _dashboards.Values
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.IsDefault)
            .ThenBy(d => d.Name)
            .AsEnumerable();
        return Task.FromResult(dashboards);
    }

    /// <inheritdoc />
    public Task<CustomDashboard?> GetDashboardAsync(string dashboardId, CancellationToken ct = default)
    {
        _dashboards.TryGetValue(dashboardId, out var dashboard);
        return Task.FromResult(dashboard);
    }

    /// <inheritdoc />
    public Task<CustomDashboard> CreateDashboardAsync(CustomDashboard dashboard, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(dashboard.Id))
            dashboard.Id = $"dash-{Interlocked.Increment(ref _idCounter)}";

        dashboard.CreatedAt = DateTime.UtcNow;
        dashboard.UpdatedAt = DateTime.UtcNow;

        // Assign widget IDs
        foreach (var widget in dashboard.Widgets)
        {
            if (string.IsNullOrEmpty(widget.Id))
                widget.Id = $"widget-{Interlocked.Increment(ref _idCounter)}";
            _widgetToDashboard[widget.Id] = dashboard.Id;
        }

        _dashboards[dashboard.Id] = dashboard;
        _logger.LogInformation("Created dashboard {DashboardId} for user {UserId} with {WidgetCount} widgets",
            dashboard.Id, dashboard.UserId, dashboard.Widgets.Count);

        return Task.FromResult(dashboard);
    }

    /// <inheritdoc />
    public Task<CustomDashboard?> UpdateDashboardAsync(CustomDashboard dashboard, CancellationToken ct = default)
    {
        if (!_dashboards.ContainsKey(dashboard.Id))
            return Task.FromResult<CustomDashboard?>(null);

        dashboard.UpdatedAt = DateTime.UtcNow;

        // Clear old widget mappings
        foreach (var kvp in _widgetToDashboard.Where(w => w.Value == dashboard.Id).ToList())
            _widgetToDashboard.TryRemove(kvp.Key, out _);

        // Re-register widget mappings
        foreach (var widget in dashboard.Widgets)
        {
            if (string.IsNullOrEmpty(widget.Id))
                widget.Id = $"widget-{Interlocked.Increment(ref _idCounter)}";
            _widgetToDashboard[widget.Id] = dashboard.Id;
        }

        _dashboards[dashboard.Id] = dashboard;
        return Task.FromResult<CustomDashboard?>(dashboard);
    }

    /// <inheritdoc />
    public Task<bool> DeleteDashboardAsync(string dashboardId, CancellationToken ct = default)
    {
        if (!_dashboards.TryRemove(dashboardId, out var removed))
            return Task.FromResult(false);

        foreach (var widget in removed.Widgets)
            _widgetToDashboard.TryRemove(widget.Id, out _);

        _logger.LogInformation("Deleted dashboard {DashboardId}", dashboardId);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public async Task<WidgetData?> GetWidgetDataAsync(string widgetId, CancellationToken ct = default)
    {
        if (!_widgetToDashboard.TryGetValue(widgetId, out var dashboardId))
            return null;

        if (!_dashboards.TryGetValue(dashboardId, out var dashboard))
            return null;

        var widget = dashboard.Widgets.FirstOrDefault(w => w.Id == widgetId);
        if (widget == null)
            return null;

        var data = await FetchWidgetDataAsync(widget, ct);
        return new WidgetData
        {
            WidgetId = widgetId,
            Type = widget.Type,
            Data = data,
            FetchedAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public IEnumerable<WidgetTemplate> GetAvailableWidgets() => WidgetCatalog;

    #region Private Data Fetching

    private async Task<Dictionary<string, object>> FetchWidgetDataAsync(DashboardWidget widget, CancellationToken ct)
    {
        return widget.Type switch
        {
            "pipeline-chart" => await GetPipelineDataAsync(ct),
            "lead-summary" => await GetLeadSummaryAsync(ct),
            "revenue-kpi" => await GetRevenueKpiAsync(ct),
            "account-count" => await GetAccountCountAsync(ct),
            "open-tickets" => await GetOpenTicketsAsync(ct),
            "deal-velocity" => await GetDealVelocityAsync(ct),
            "conversion-rate" => await GetConversionRateAsync(ct),
            _ => new Dictionary<string, object> { ["message"] = "Widget type not supported for live data" }
        };
    }

    private async Task<Dictionary<string, object>> GetPipelineDataAsync(CancellationToken ct)
    {
        var opps = await _context.Opportunities
            .Where(o => !o.IsDeleted && o.Stage != CRM.Core.Entities.OpportunityStage.ClosedWon && o.Stage != CRM.Core.Entities.OpportunityStage.ClosedLost)
            .ToListAsync(ct);

        var byStage = opps
            .GroupBy(o => o.Stage.ToString())
            .ToDictionary(g => g.Key, g => (object)new { count = g.Count(), value = g.Sum(o => o.Amount) });

        return new Dictionary<string, object>
        {
            ["stages"] = byStage,
            ["totalDeals"] = opps.Count,
            ["totalValue"] = opps.Sum(o => o.Amount)
        };
    }

    private async Task<Dictionary<string, object>> GetLeadSummaryAsync(CancellationToken ct)
    {
        var leads = await _context.Leads
            .Where(l => !l.IsDeleted)
            .ToListAsync(ct);

        var byStatus = leads
            .GroupBy(l => l.Status.ToString())
            .ToDictionary(g => g.Key, g => (object)g.Count());

        return new Dictionary<string, object>
        {
            ["byStatus"] = byStatus,
            ["totalLeads"] = leads.Count
        };
    }

    private async Task<Dictionary<string, object>> GetRevenueKpiAsync(CancellationToken ct)
    {
        var wonOpps = await _context.Opportunities
            .Where(o => !o.IsDeleted && o.Stage == CRM.Core.Entities.OpportunityStage.ClosedWon)
            .ToListAsync(ct);

        return new Dictionary<string, object>
        {
            ["totalRevenue"] = wonOpps.Sum(o => o.Amount),
            ["dealCount"] = wonOpps.Count,
            ["averageDeal"] = wonOpps.Count > 0 ? wonOpps.Average(o => o.Amount) : 0m
        };
    }

    private async Task<Dictionary<string, object>> GetAccountCountAsync(CancellationToken ct)
    {
        var count = await _context.Customers
            .CountAsync(c => !c.IsDeleted, ct);

        return new Dictionary<string, object>
        {
            ["count"] = count
        };
    }

    private async Task<Dictionary<string, object>> GetOpenTicketsAsync(CancellationToken ct)
    {
        var count = await _context.ServiceRequests
            .CountAsync(sr => !sr.IsDeleted && sr.Status != CRM.Core.Entities.ServiceRequestStatus.Closed && sr.Status != CRM.Core.Entities.ServiceRequestStatus.Resolved, ct);

        return new Dictionary<string, object>
        {
            ["count"] = count
        };
    }

    private async Task<Dictionary<string, object>> GetDealVelocityAsync(CancellationToken ct)
    {
        var wonOpps = await _context.Opportunities
            .Where(o => !o.IsDeleted && o.Stage == CRM.Core.Entities.OpportunityStage.ClosedWon)
            .ToListAsync(ct);

        var avgDays = wonOpps.Count > 0
            ? wonOpps.Where(o => o.UpdatedAt.HasValue).Average(o => (o.UpdatedAt!.Value - o.CreatedAt).TotalDays)
            : 0;

        return new Dictionary<string, object>
        {
            ["averageDaysToClose"] = Math.Round(avgDays, 1),
            ["sampleSize"] = wonOpps.Count
        };
    }

    private async Task<Dictionary<string, object>> GetConversionRateAsync(CancellationToken ct)
    {
        var leads = await _context.Leads
            .Where(l => !l.IsDeleted)
            .ToListAsync(ct);

        var total = leads.Count;
        var converted = leads.Count(l => l.Status == CRM.Core.Entities.LeadLifecycleStatus.Converted);
        var rate = total > 0 ? (double)converted / total * 100 : 0;

        return new Dictionary<string, object>
        {
            ["rate"] = Math.Round(rate, 1),
            ["converted"] = converted,
            ["total"] = total
        };
    }

    #endregion
}
