// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

namespace CRM.Core.Entities;

/// <summary>
/// Represents a customizable dashboard configuration
/// </summary>
public class Dashboard : BaseEntity
{
    /// <summary>
    /// Name of the dashboard
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the dashboard purpose
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this is the default dashboard shown on login
    /// </summary>
    public bool IsDefault { get; set; } = false;
    
    /// <summary>
    /// Whether this dashboard is a system dashboard (cannot be deleted)
    /// </summary>
    public bool IsSystem { get; set; } = false;
    
    /// <summary>
    /// Whether this dashboard is active and visible
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Icon name for the dashboard (Material Icon)
    /// </summary>
    public string IconName { get; set; } = "Dashboard";
    
    /// <summary>
    /// Display order for dashboard tabs
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Number of columns in the dashboard grid (1-4)
    /// </summary>
    public int ColumnCount { get; set; } = 3;
    
    /// <summary>
    /// Refresh interval in seconds (0 = no auto-refresh)
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 300;
    
    /// <summary>
    /// JSON configuration for dashboard layout
    /// </summary>
    public string? LayoutConfig { get; set; }
    
    /// <summary>
    /// Owner user ID (null for shared dashboards)
    /// </summary>
    public int? OwnerId { get; set; }
    
    /// <summary>
    /// Navigation property to owner
    /// </summary>
    public virtual User? Owner { get; set; }
    
    /// <summary>
    /// Visibility: Public, Private, Role-based
    /// </summary>
    public DashboardVisibility Visibility { get; set; } = DashboardVisibility.Public;
    
    /// <summary>
    /// Comma-separated list of role names that can view this dashboard
    /// </summary>
    public string? AllowedRoles { get; set; }
    
    /// <summary>
    /// Collection of widgets on this dashboard
    /// </summary>
    public virtual ICollection<DashboardWidget> Widgets { get; set; } = new List<DashboardWidget>();
}

/// <summary>
/// Dashboard visibility options
/// </summary>
public enum DashboardVisibility
{
    /// <summary>All users can view</summary>
    Public = 0,
    /// <summary>Only owner can view</summary>
    Private = 1,
    /// <summary>Only users with specified roles can view</summary>
    RoleBased = 2
}
