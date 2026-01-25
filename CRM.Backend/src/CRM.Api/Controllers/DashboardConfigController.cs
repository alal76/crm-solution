// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing dashboard configurations and widgets
/// </summary>
[ApiController]
[Route("api/dashboard-config")]
[Authorize]
public class DashboardConfigController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<DashboardConfigController> _logger;

    public DashboardConfigController(CrmDbContext context, ILogger<DashboardConfigController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
    }

    #region Dashboard CRUD

    /// <summary>
    /// Get all dashboards accessible to the current user
    /// </summary>
    [HttpGet("dashboards")]
    public async Task<IActionResult> GetDashboards()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var dashboards = await _context.Dashboards
                .Where(d => !d.IsDeleted && d.IsActive)
                .Where(d => 
                    d.Visibility == DashboardVisibility.Public ||
                    (d.Visibility == DashboardVisibility.Private && d.OwnerId == userId) ||
                    (d.Visibility == DashboardVisibility.RoleBased && 
                     (d.AllowedRoles == null || d.AllowedRoles.Contains(userRole))))
                .OrderBy(d => d.DisplayOrder)
                .ThenBy(d => d.Name)
                .Select(d => new DashboardDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    IsDefault = d.IsDefault,
                    IsSystem = d.IsSystem,
                    IconName = d.IconName,
                    DisplayOrder = d.DisplayOrder,
                    ColumnCount = d.ColumnCount,
                    RefreshIntervalSeconds = d.RefreshIntervalSeconds,
                    Visibility = d.Visibility.ToString(),
                    OwnerId = d.OwnerId,
                    OwnerName = d.Owner != null ? $"{d.Owner.FirstName} {d.Owner.LastName}" : null,
                    WidgetCount = d.Widgets.Count(w => !w.IsDeleted)
                })
                .ToListAsync();

            return Ok(dashboards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboards");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboards" });
        }
    }

    /// <summary>
    /// Get all dashboards (Admin only)
    /// </summary>
    [HttpGet("dashboards/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllDashboards()
    {
        try
        {
            var dashboards = await _context.Dashboards
                .Where(d => !d.IsDeleted)
                .Include(d => d.Owner)
                .OrderBy(d => d.DisplayOrder)
                .ThenBy(d => d.Name)
                .Select(d => new DashboardDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    IsDefault = d.IsDefault,
                    IsSystem = d.IsSystem,
                    IsActive = d.IsActive,
                    IconName = d.IconName,
                    DisplayOrder = d.DisplayOrder,
                    ColumnCount = d.ColumnCount,
                    RefreshIntervalSeconds = d.RefreshIntervalSeconds,
                    Visibility = d.Visibility.ToString(),
                    AllowedRoles = d.AllowedRoles,
                    OwnerId = d.OwnerId,
                    OwnerName = d.Owner != null ? $"{d.Owner.FirstName} {d.Owner.LastName}" : null,
                    WidgetCount = d.Widgets.Count(w => !w.IsDeleted),
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .ToListAsync();

            return Ok(dashboards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all dashboards");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboards" });
        }
    }

    /// <summary>
    /// Get a dashboard by ID with all widgets
    /// </summary>
    [HttpGet("dashboards/{id}")]
    public async Task<IActionResult> GetDashboard(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var dashboard = await _context.Dashboards
                .Where(d => d.Id == id && !d.IsDeleted)
                .Include(d => d.Widgets.Where(w => !w.IsDeleted))
                .Include(d => d.Owner)
                .FirstOrDefaultAsync();

            if (dashboard == null)
                return NotFound(new { message = "Dashboard not found" });

            // Check access
            if (dashboard.Visibility == DashboardVisibility.Private && dashboard.OwnerId != userId)
                return Forbid();

            if (dashboard.Visibility == DashboardVisibility.RoleBased && 
                dashboard.AllowedRoles != null && !dashboard.AllowedRoles.Contains(userRole))
                return Forbid();

            var dto = new DashboardDetailDto
            {
                Id = dashboard.Id,
                Name = dashboard.Name,
                Description = dashboard.Description,
                IsDefault = dashboard.IsDefault,
                IsSystem = dashboard.IsSystem,
                IsActive = dashboard.IsActive,
                IconName = dashboard.IconName,
                DisplayOrder = dashboard.DisplayOrder,
                ColumnCount = dashboard.ColumnCount,
                RefreshIntervalSeconds = dashboard.RefreshIntervalSeconds,
                LayoutConfig = dashboard.LayoutConfig,
                Visibility = dashboard.Visibility.ToString(),
                AllowedRoles = dashboard.AllowedRoles,
                OwnerId = dashboard.OwnerId,
                OwnerName = dashboard.Owner != null ? $"{dashboard.Owner.FirstName} {dashboard.Owner.LastName}" : null,
                Widgets = dashboard.Widgets.OrderBy(w => w.DisplayOrder).Select(w => new DashboardWidgetDto
                {
                    Id = w.Id,
                    Title = w.Title,
                    Subtitle = w.Subtitle,
                    WidgetType = w.WidgetType.ToString(),
                    WidgetTypeValue = (int)w.WidgetType,
                    DataSource = w.DataSource,
                    RowIndex = w.RowIndex,
                    ColumnIndex = w.ColumnIndex,
                    ColumnSpan = w.ColumnSpan,
                    RowSpan = w.RowSpan,
                    DisplayOrder = w.DisplayOrder,
                    IsVisible = w.IsVisible,
                    IconName = w.IconName,
                    Color = w.Color,
                    BackgroundColor = w.BackgroundColor,
                    NavigationLink = w.NavigationLink,
                    ConfigJson = w.ConfigJson,
                    ShowTrend = w.ShowTrend,
                    TrendPeriodDays = w.TrendPeriodDays,
                    RefreshIntervalSeconds = w.RefreshIntervalSeconds
                }).ToList()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the dashboard" });
        }
    }

    /// <summary>
    /// Get the default dashboard
    /// </summary>
    [HttpGet("dashboards/default")]
    public async Task<IActionResult> GetDefaultDashboard()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var dashboard = await _context.Dashboards
                .Where(d => d.IsDefault && !d.IsDeleted && d.IsActive)
                .Where(d => 
                    d.Visibility == DashboardVisibility.Public ||
                    (d.Visibility == DashboardVisibility.Private && d.OwnerId == userId) ||
                    (d.Visibility == DashboardVisibility.RoleBased && 
                     (d.AllowedRoles == null || d.AllowedRoles.Contains(userRole))))
                .Include(d => d.Widgets.Where(w => !w.IsDeleted && w.IsVisible))
                .Include(d => d.Owner)
                .FirstOrDefaultAsync();

            if (dashboard == null)
            {
                // Return the first available dashboard
                dashboard = await _context.Dashboards
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .Where(d => 
                        d.Visibility == DashboardVisibility.Public ||
                        (d.Visibility == DashboardVisibility.Private && d.OwnerId == userId) ||
                        (d.Visibility == DashboardVisibility.RoleBased && 
                         (d.AllowedRoles == null || d.AllowedRoles.Contains(userRole))))
                    .Include(d => d.Widgets.Where(w => !w.IsDeleted && w.IsVisible))
                    .OrderBy(d => d.DisplayOrder)
                    .FirstOrDefaultAsync();
            }

            if (dashboard == null)
                return Ok(null); // No dashboards available

            var dto = new DashboardDetailDto
            {
                Id = dashboard.Id,
                Name = dashboard.Name,
                Description = dashboard.Description,
                IsDefault = dashboard.IsDefault,
                IsSystem = dashboard.IsSystem,
                IsActive = dashboard.IsActive,
                IconName = dashboard.IconName,
                DisplayOrder = dashboard.DisplayOrder,
                ColumnCount = dashboard.ColumnCount,
                RefreshIntervalSeconds = dashboard.RefreshIntervalSeconds,
                LayoutConfig = dashboard.LayoutConfig,
                Visibility = dashboard.Visibility.ToString(),
                OwnerId = dashboard.OwnerId,
                Widgets = dashboard.Widgets.OrderBy(w => w.DisplayOrder).Select(w => new DashboardWidgetDto
                {
                    Id = w.Id,
                    Title = w.Title,
                    Subtitle = w.Subtitle,
                    WidgetType = w.WidgetType.ToString(),
                    WidgetTypeValue = (int)w.WidgetType,
                    DataSource = w.DataSource,
                    RowIndex = w.RowIndex,
                    ColumnIndex = w.ColumnIndex,
                    ColumnSpan = w.ColumnSpan,
                    RowSpan = w.RowSpan,
                    DisplayOrder = w.DisplayOrder,
                    IsVisible = w.IsVisible,
                    IconName = w.IconName,
                    Color = w.Color,
                    BackgroundColor = w.BackgroundColor,
                    NavigationLink = w.NavigationLink,
                    ConfigJson = w.ConfigJson,
                    ShowTrend = w.ShowTrend,
                    TrendPeriodDays = w.TrendPeriodDays
                }).ToList()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default dashboard");
            return StatusCode(500, new { message = "An error occurred while retrieving the default dashboard" });
        }
    }

    /// <summary>
    /// Create a new dashboard
    /// </summary>
    [HttpPost("dashboards")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateDashboard([FromBody] CreateDashboardDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Dashboard name is required" });

            var userId = GetCurrentUserId();

            // If this is set as default, unset other defaults
            if (dto.IsDefault)
            {
                var existingDefaults = await _context.Dashboards
                    .Where(d => d.IsDefault && !d.IsDeleted)
                    .ToListAsync();
                foreach (var d in existingDefaults)
                {
                    d.IsDefault = false;
                }
            }

            var dashboard = new Dashboard
            {
                Name = dto.Name,
                Description = dto.Description,
                IsDefault = dto.IsDefault,
                IsSystem = false,
                IsActive = dto.IsActive ?? true,
                IconName = dto.IconName ?? "Dashboard",
                DisplayOrder = dto.DisplayOrder ?? 0,
                ColumnCount = dto.ColumnCount ?? 3,
                RefreshIntervalSeconds = dto.RefreshIntervalSeconds ?? 300,
                LayoutConfig = dto.LayoutConfig,
                OwnerId = dto.OwnerId ?? userId,
                Visibility = Enum.TryParse<DashboardVisibility>(dto.Visibility, out var vis) 
                    ? vis : DashboardVisibility.Public,
                AllowedRoles = dto.AllowedRoles,
                CreatedAt = DateTime.UtcNow
            };

            _context.Dashboards.Add(dashboard);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dashboard {Name} created with ID {Id}", dashboard.Name, dashboard.Id);

            return CreatedAtAction(nameof(GetDashboard), new { id = dashboard.Id }, new { id = dashboard.Id, name = dashboard.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dashboard");
            return StatusCode(500, new { message = "An error occurred while creating the dashboard" });
        }
    }

    /// <summary>
    /// Update a dashboard
    /// </summary>
    [HttpPut("dashboards/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDashboard(int id, [FromBody] UpdateDashboardDto dto)
    {
        try
        {
            var dashboard = await _context.Dashboards.FindAsync(id);
            if (dashboard == null || dashboard.IsDeleted)
                return NotFound(new { message = "Dashboard not found" });

            if (dashboard.IsSystem && !User.IsInRole("Admin"))
                return Forbid();

            // If this is set as default, unset other defaults
            if (dto.IsDefault == true && !dashboard.IsDefault)
            {
                var existingDefaults = await _context.Dashboards
                    .Where(d => d.IsDefault && !d.IsDeleted && d.Id != id)
                    .ToListAsync();
                foreach (var d in existingDefaults)
                {
                    d.IsDefault = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                dashboard.Name = dto.Name;
            if (dto.Description != null)
                dashboard.Description = dto.Description;
            if (dto.IsDefault.HasValue)
                dashboard.IsDefault = dto.IsDefault.Value;
            if (dto.IsActive.HasValue)
                dashboard.IsActive = dto.IsActive.Value;
            if (!string.IsNullOrWhiteSpace(dto.IconName))
                dashboard.IconName = dto.IconName;
            if (dto.DisplayOrder.HasValue)
                dashboard.DisplayOrder = dto.DisplayOrder.Value;
            if (dto.ColumnCount.HasValue)
                dashboard.ColumnCount = dto.ColumnCount.Value;
            if (dto.RefreshIntervalSeconds.HasValue)
                dashboard.RefreshIntervalSeconds = dto.RefreshIntervalSeconds.Value;
            if (dto.LayoutConfig != null)
                dashboard.LayoutConfig = dto.LayoutConfig;
            if (dto.Visibility != null && Enum.TryParse<DashboardVisibility>(dto.Visibility, out var vis))
                dashboard.Visibility = vis;
            if (dto.AllowedRoles != null)
                dashboard.AllowedRoles = dto.AllowedRoles;

            dashboard.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dashboard {Id} updated", id);

            return Ok(new { message = "Dashboard updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dashboard {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the dashboard" });
        }
    }

    /// <summary>
    /// Delete a dashboard
    /// </summary>
    [HttpDelete("dashboards/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDashboard(int id)
    {
        try
        {
            var dashboard = await _context.Dashboards.FindAsync(id);
            if (dashboard == null || dashboard.IsDeleted)
                return NotFound(new { message = "Dashboard not found" });

            if (dashboard.IsSystem)
                return BadRequest(new { message = "Cannot delete system dashboards" });

            dashboard.IsDeleted = true;
            dashboard.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dashboard {Id} deleted", id);

            return Ok(new { message = "Dashboard deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dashboard {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the dashboard" });
        }
    }

    #endregion

    #region Widget CRUD

    /// <summary>
    /// Get all widgets for a dashboard
    /// </summary>
    [HttpGet("dashboards/{dashboardId}/widgets")]
    public async Task<IActionResult> GetWidgets(int dashboardId)
    {
        try
        {
            var widgets = await _context.DashboardWidgets
                .Where(w => w.DashboardId == dashboardId && !w.IsDeleted)
                .OrderBy(w => w.DisplayOrder)
                .Select(w => new DashboardWidgetDto
                {
                    Id = w.Id,
                    Title = w.Title,
                    Subtitle = w.Subtitle,
                    WidgetType = w.WidgetType.ToString(),
                    WidgetTypeValue = (int)w.WidgetType,
                    DataSource = w.DataSource,
                    RowIndex = w.RowIndex,
                    ColumnIndex = w.ColumnIndex,
                    ColumnSpan = w.ColumnSpan,
                    RowSpan = w.RowSpan,
                    DisplayOrder = w.DisplayOrder,
                    IsVisible = w.IsVisible,
                    IconName = w.IconName,
                    Color = w.Color,
                    BackgroundColor = w.BackgroundColor,
                    NavigationLink = w.NavigationLink,
                    ConfigJson = w.ConfigJson,
                    ShowTrend = w.ShowTrend,
                    TrendPeriodDays = w.TrendPeriodDays,
                    RefreshIntervalSeconds = w.RefreshIntervalSeconds
                })
                .ToListAsync();

            return Ok(widgets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving widgets for dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { message = "An error occurred while retrieving widgets" });
        }
    }

    /// <summary>
    /// Create a widget
    /// </summary>
    [HttpPost("widgets")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateWidget([FromBody] CreateWidgetDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Widget title is required" });

            var dashboard = await _context.Dashboards.FindAsync(dto.DashboardId);
            if (dashboard == null || dashboard.IsDeleted)
                return NotFound(new { message = "Dashboard not found" });

            var widget = new DashboardWidget
            {
                DashboardId = dto.DashboardId,
                Title = dto.Title,
                Subtitle = dto.Subtitle,
                WidgetType = Enum.TryParse<WidgetType>(dto.WidgetType, out var wt) ? wt : WidgetType.StatCard,
                DataSource = dto.DataSource ?? "",
                RowIndex = dto.RowIndex ?? 0,
                ColumnIndex = dto.ColumnIndex ?? 0,
                ColumnSpan = dto.ColumnSpan ?? 1,
                RowSpan = dto.RowSpan ?? 1,
                DisplayOrder = dto.DisplayOrder ?? 0,
                IsVisible = dto.IsVisible ?? true,
                IconName = dto.IconName,
                Color = dto.Color,
                BackgroundColor = dto.BackgroundColor,
                NavigationLink = dto.NavigationLink,
                ConfigJson = dto.ConfigJson,
                ShowTrend = dto.ShowTrend ?? false,
                TrendPeriodDays = dto.TrendPeriodDays ?? 30,
                RefreshIntervalSeconds = dto.RefreshIntervalSeconds ?? 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.DashboardWidgets.Add(widget);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Widget {Title} created with ID {Id}", widget.Title, widget.Id);

            return CreatedAtAction(nameof(GetWidgets), new { dashboardId = dto.DashboardId }, 
                new { id = widget.Id, title = widget.Title });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating widget");
            return StatusCode(500, new { message = "An error occurred while creating the widget" });
        }
    }

    /// <summary>
    /// Update a widget
    /// </summary>
    [HttpPut("widgets/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateWidget(int id, [FromBody] UpdateWidgetDto dto)
    {
        try
        {
            var widget = await _context.DashboardWidgets.FindAsync(id);
            if (widget == null || widget.IsDeleted)
                return NotFound(new { message = "Widget not found" });

            if (!string.IsNullOrWhiteSpace(dto.Title))
                widget.Title = dto.Title;
            if (dto.Subtitle != null)
                widget.Subtitle = dto.Subtitle;
            if (dto.WidgetType != null && Enum.TryParse<WidgetType>(dto.WidgetType, out var wt))
                widget.WidgetType = wt;
            if (dto.DataSource != null)
                widget.DataSource = dto.DataSource;
            if (dto.RowIndex.HasValue)
                widget.RowIndex = dto.RowIndex.Value;
            if (dto.ColumnIndex.HasValue)
                widget.ColumnIndex = dto.ColumnIndex.Value;
            if (dto.ColumnSpan.HasValue)
                widget.ColumnSpan = dto.ColumnSpan.Value;
            if (dto.RowSpan.HasValue)
                widget.RowSpan = dto.RowSpan.Value;
            if (dto.DisplayOrder.HasValue)
                widget.DisplayOrder = dto.DisplayOrder.Value;
            if (dto.IsVisible.HasValue)
                widget.IsVisible = dto.IsVisible.Value;
            if (dto.IconName != null)
                widget.IconName = dto.IconName;
            if (dto.Color != null)
                widget.Color = dto.Color;
            if (dto.BackgroundColor != null)
                widget.BackgroundColor = dto.BackgroundColor;
            if (dto.NavigationLink != null)
                widget.NavigationLink = dto.NavigationLink;
            if (dto.ConfigJson != null)
                widget.ConfigJson = dto.ConfigJson;
            if (dto.ShowTrend.HasValue)
                widget.ShowTrend = dto.ShowTrend.Value;
            if (dto.TrendPeriodDays.HasValue)
                widget.TrendPeriodDays = dto.TrendPeriodDays.Value;
            if (dto.RefreshIntervalSeconds.HasValue)
                widget.RefreshIntervalSeconds = dto.RefreshIntervalSeconds.Value;

            widget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Widget {Id} updated", id);

            return Ok(new { message = "Widget updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating widget {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the widget" });
        }
    }

    /// <summary>
    /// Delete a widget
    /// </summary>
    [HttpDelete("widgets/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteWidget(int id)
    {
        try
        {
            var widget = await _context.DashboardWidgets.FindAsync(id);
            if (widget == null || widget.IsDeleted)
                return NotFound(new { message = "Widget not found" });

            widget.IsDeleted = true;
            widget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Widget {Id} deleted", id);

            return Ok(new { message = "Widget deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting widget {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the widget" });
        }
    }

    /// <summary>
    /// Reorder widgets in a dashboard
    /// </summary>
    [HttpPost("dashboards/{dashboardId}/reorder-widgets")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReorderWidgets(int dashboardId, [FromBody] List<WidgetOrderDto> orders)
    {
        try
        {
            var widgets = await _context.DashboardWidgets
                .Where(w => w.DashboardId == dashboardId && !w.IsDeleted)
                .ToListAsync();

            foreach (var order in orders)
            {
                var widget = widgets.FirstOrDefault(w => w.Id == order.WidgetId);
                if (widget != null)
                {
                    widget.DisplayOrder = order.DisplayOrder;
                    widget.RowIndex = order.RowIndex ?? widget.RowIndex;
                    widget.ColumnIndex = order.ColumnIndex ?? widget.ColumnIndex;
                    widget.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Widgets reordered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering widgets in dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { message = "An error occurred while reordering widgets" });
        }
    }

    #endregion

    #region Widget Types and Data Sources

    /// <summary>
    /// Get available widget types
    /// </summary>
    [HttpGet("widget-types")]
    public IActionResult GetWidgetTypes()
    {
        var types = Enum.GetValues<WidgetType>()
            .Select(t => new { value = (int)t, name = t.ToString() })
            .ToList();
        return Ok(types);
    }

    /// <summary>
    /// Get available data sources for widgets
    /// </summary>
    [HttpGet("data-sources")]
    public IActionResult GetDataSources()
    {
        var dataSources = new[]
        {
            new { id = "customers.count", name = "Customer Count", category = "Customers", type = "count" },
            new { id = "customers.new_this_month", name = "New Customers This Month", category = "Customers", type = "count" },
            new { id = "customers.by_lifecycle", name = "Customers by Lifecycle Stage", category = "Customers", type = "distribution" },
            
            new { id = "contacts.count", name = "Contact Count", category = "Contacts", type = "count" },
            
            new { id = "opportunities.count", name = "Opportunity Count", category = "Opportunities", type = "count" },
            new { id = "opportunities.pipeline_value", name = "Pipeline Value", category = "Opportunities", type = "currency" },
            new { id = "opportunities.won_value", name = "Won Value", category = "Opportunities", type = "currency" },
            new { id = "opportunities.by_stage", name = "Opportunities by Stage", category = "Opportunities", type = "distribution" },
            new { id = "opportunities.pipeline_trend", name = "Pipeline Trend", category = "Opportunities", type = "trend" },
            new { id = "opportunities.close_forecast", name = "Expected Closings", category = "Opportunities", type = "forecast" },
            new { id = "opportunities.recent", name = "Recent Opportunities", category = "Opportunities", type = "list" },
            
            new { id = "campaigns.count", name = "Campaign Count", category = "Marketing", type = "count" },
            new { id = "campaigns.active", name = "Active Campaigns", category = "Marketing", type = "count" },
            new { id = "campaigns.budget_total", name = "Total Budget", category = "Marketing", type = "currency" },
            
            new { id = "tasks.count", name = "Task Count", category = "Tasks", type = "count" },
            new { id = "tasks.pending", name = "Pending Tasks", category = "Tasks", type = "count" },
            new { id = "tasks.overdue", name = "Overdue Tasks", category = "Tasks", type = "count" },
            new { id = "tasks.my_tasks", name = "My Tasks", category = "Tasks", type = "list" },
            
            new { id = "service_requests.count", name = "Service Request Count", category = "Service", type = "count" },
            new { id = "service_requests.open", name = "Open Requests", category = "Service", type = "count" },
            new { id = "service_requests.by_status", name = "Requests by Status", category = "Service", type = "distribution" },
            new { id = "service_requests.by_priority", name = "Requests by Priority", category = "Service", type = "distribution" },
            
            new { id = "products.count", name = "Product Count", category = "Products", type = "count" },
            new { id = "products.active", name = "Active Products", category = "Products", type = "count" },
            
            new { id = "users.active", name = "Active Users", category = "Users", type = "count" },
            
            new { id = "activities.recent", name = "Recent Activities", category = "Activities", type = "list" },
            
            new { id = "leads.count", name = "Lead Count", category = "Leads", type = "count" },
            new { id = "leads.this_month", name = "Leads This Month", category = "Leads", type = "count" },
            new { id = "leads.conversion_rate", name = "Lead Conversion Rate", category = "Leads", type = "percentage" }
        };

        return Ok(dataSources);
    }

    /// <summary>
    /// Initialize default dashboards (for new installations)
    /// </summary>
    [HttpPost("initialize")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> InitializeDefaultDashboards()
    {
        try
        {
            var existingCount = await _context.Dashboards.CountAsync(d => !d.IsDeleted);
            if (existingCount > 0)
                return Ok(new { message = "Dashboards already exist", count = existingCount });

            // Create default Sales Dashboard
            var salesDashboard = new Dashboard
            {
                Name = "Sales Dashboard",
                Description = "Overview of sales pipeline and opportunities",
                IsDefault = true,
                IsSystem = true,
                IsActive = true,
                IconName = "TrendingUp",
                DisplayOrder = 1,
                ColumnCount = 3,
                RefreshIntervalSeconds = 300,
                Visibility = DashboardVisibility.Public,
                CreatedAt = DateTime.UtcNow
            };
            _context.Dashboards.Add(salesDashboard);
            await _context.SaveChangesAsync();

            // Add widgets to Sales Dashboard
            var salesWidgets = new List<DashboardWidget>
            {
                new DashboardWidget { DashboardId = salesDashboard.Id, Title = "Total Pipeline", DataSource = "opportunities.pipeline_value", WidgetType = WidgetType.StatCard, IconName = "TrendingUp", Color = "#6750A4", ColumnSpan = 1, DisplayOrder = 1, NavigationLink = "/opportunities", CreatedAt = DateTime.UtcNow },
                new DashboardWidget { DashboardId = salesDashboard.Id, Title = "Won Revenue", DataSource = "opportunities.won_value", WidgetType = WidgetType.StatCard, IconName = "AttachMoney", Color = "#06A77D", ColumnSpan = 1, DisplayOrder = 2, NavigationLink = "/opportunities", CreatedAt = DateTime.UtcNow },
                new DashboardWidget { DashboardId = salesDashboard.Id, Title = "Customers", DataSource = "customers.count", WidgetType = WidgetType.StatCard, IconName = "People", Color = "#0092BC", ColumnSpan = 1, DisplayOrder = 3, NavigationLink = "/customers", CreatedAt = DateTime.UtcNow },
                new DashboardWidget { DashboardId = salesDashboard.Id, Title = "Pipeline Trend", DataSource = "opportunities.pipeline_trend", WidgetType = WidgetType.LineChart, ColumnSpan = 2, RowSpan = 2, DisplayOrder = 4, CreatedAt = DateTime.UtcNow },
                new DashboardWidget { DashboardId = salesDashboard.Id, Title = "Opportunities by Stage", DataSource = "opportunities.by_stage", WidgetType = WidgetType.PieChart, ColumnSpan = 1, RowSpan = 2, DisplayOrder = 5, CreatedAt = DateTime.UtcNow },
                new DashboardWidget { DashboardId = salesDashboard.Id, Title = "Recent Opportunities", DataSource = "opportunities.recent", WidgetType = WidgetType.DataTable, ColumnSpan = 3, DisplayOrder = 6, NavigationLink = "/opportunities", CreatedAt = DateTime.UtcNow }
            };
            _context.DashboardWidgets.AddRange(salesWidgets);

            // Create Operations Dashboard
            var opsDashboard = new Dashboard
            {
                Name = "Operations Dashboard",
                Description = "Service requests and task overview",
                IsDefault = false,
                IsSystem = true,
                IsActive = true,
                IconName = "Build",
                DisplayOrder = 2,
                ColumnCount = 3,
                RefreshIntervalSeconds = 300,
                Visibility = DashboardVisibility.Public,
                CreatedAt = DateTime.UtcNow
            };
            _context.Dashboards.Add(opsDashboard);
            await _context.SaveChangesAsync();

            var opsWidgets = new List<DashboardWidget>
            {
                new DashboardWidget { DashboardId = opsDashboard.Id, Title = "Open Requests", DataSource = "service_requests.open", WidgetType = WidgetType.StatCard, IconName = "Help", Color = "#F57C00", ColumnSpan = 1, DisplayOrder = 1, NavigationLink = "/service-requests", CreatedAt = DateTime.UtcNow },
                new DashboardWidget { DashboardId = opsDashboard.Id, Title = "Pending Tasks", DataSource = "tasks.pending", WidgetType = WidgetType.StatCard, IconName = "Assignment", Color = "#9C27B0", ColumnSpan = 1, DisplayOrder = 2, NavigationLink = "/tasks", CreatedAt = DateTime.UtcNow },
                new DashboardWidget { DashboardId = opsDashboard.Id, Title = "Active Users", DataSource = "users.active", WidgetType = WidgetType.StatCard, IconName = "Group", Color = "#2196F3", ColumnSpan = 1, DisplayOrder = 3, CreatedAt = DateTime.UtcNow },
                new DashboardWidget { DashboardId = opsDashboard.Id, Title = "Requests by Status", DataSource = "service_requests.by_status", WidgetType = WidgetType.BarChart, ColumnSpan = 2, RowSpan = 2, DisplayOrder = 4, CreatedAt = DateTime.UtcNow },
                new DashboardWidget { DashboardId = opsDashboard.Id, Title = "Requests by Priority", DataSource = "service_requests.by_priority", WidgetType = WidgetType.PieChart, ColumnSpan = 1, RowSpan = 2, DisplayOrder = 5, CreatedAt = DateTime.UtcNow }
            };
            _context.DashboardWidgets.AddRange(opsWidgets);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Initialized default dashboards");

            return Ok(new { message = "Default dashboards created successfully", count = 2 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default dashboards");
            return StatusCode(500, new { message = "An error occurred while initializing dashboards" });
        }
    }

    #endregion
}

#region DTOs

public class DashboardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public string IconName { get; set; } = "Dashboard";
    public int DisplayOrder { get; set; }
    public int ColumnCount { get; set; }
    public int RefreshIntervalSeconds { get; set; }
    public string Visibility { get; set; } = "Public";
    public string? AllowedRoles { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public int WidgetCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DashboardDetailDto : DashboardDto
{
    public string? LayoutConfig { get; set; }
    public List<DashboardWidgetDto> Widgets { get; set; } = new();
}

public class DashboardWidgetDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string WidgetType { get; set; } = "StatCard";
    public int WidgetTypeValue { get; set; }
    public string DataSource { get; set; } = string.Empty;
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public int ColumnSpan { get; set; }
    public int RowSpan { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public string? BackgroundColor { get; set; }
    public string? NavigationLink { get; set; }
    public string? ConfigJson { get; set; }
    public bool ShowTrend { get; set; }
    public int TrendPeriodDays { get; set; }
    public int RefreshIntervalSeconds { get; set; }
}

public class CreateDashboardDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool? IsActive { get; set; }
    public string? IconName { get; set; }
    public int? DisplayOrder { get; set; }
    public int? ColumnCount { get; set; }
    public int? RefreshIntervalSeconds { get; set; }
    public string? LayoutConfig { get; set; }
    public int? OwnerId { get; set; }
    public string? Visibility { get; set; }
    public string? AllowedRoles { get; set; }
}

public class UpdateDashboardDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
    public string? IconName { get; set; }
    public int? DisplayOrder { get; set; }
    public int? ColumnCount { get; set; }
    public int? RefreshIntervalSeconds { get; set; }
    public string? LayoutConfig { get; set; }
    public string? Visibility { get; set; }
    public string? AllowedRoles { get; set; }
}

public class CreateWidgetDto
{
    public int DashboardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? WidgetType { get; set; }
    public string? DataSource { get; set; }
    public int? RowIndex { get; set; }
    public int? ColumnIndex { get; set; }
    public int? ColumnSpan { get; set; }
    public int? RowSpan { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsVisible { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public string? BackgroundColor { get; set; }
    public string? NavigationLink { get; set; }
    public string? ConfigJson { get; set; }
    public bool? ShowTrend { get; set; }
    public int? TrendPeriodDays { get; set; }
    public int? RefreshIntervalSeconds { get; set; }
}

public class UpdateWidgetDto
{
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? WidgetType { get; set; }
    public string? DataSource { get; set; }
    public int? RowIndex { get; set; }
    public int? ColumnIndex { get; set; }
    public int? ColumnSpan { get; set; }
    public int? RowSpan { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsVisible { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public string? BackgroundColor { get; set; }
    public string? NavigationLink { get; set; }
    public string? ConfigJson { get; set; }
    public bool? ShowTrend { get; set; }
    public int? TrendPeriodDays { get; set; }
    public int? RefreshIntervalSeconds { get; set; }
}

public class WidgetOrderDto
{
    public int WidgetId { get; set; }
    public int DisplayOrder { get; set; }
    public int? RowIndex { get; set; }
    public int? ColumnIndex { get; set; }
}

#endregion
