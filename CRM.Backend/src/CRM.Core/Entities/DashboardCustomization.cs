// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Dashboard customization with draggable widgets and layouts
/// </summary>
[Table("DashboardCustomizations")]
public class DashboardCustomization : BaseEntity
{
    /// <summary>
    /// User ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Dashboard name (e.g., "Sales Dashboard", "Support Dashboard")
    /// </summary>
    public string DashboardName { get; set; } = string.Empty;

    /// <summary>
    /// Layout configuration as JSON (widget positions, sizes, etc.)
    /// </summary>
    public string LayoutConfig { get; set; } = string.Empty;

    /// <summary>
    /// Array of widgets to display (JSON format)
    /// </summary>
    public string Widgets { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the default dashboard for the user
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Number of columns in the grid layout
    /// </summary>
    public int GridColumns { get; set; } = 12;

    /// <summary>
    /// Whether to auto-refresh widgets
    /// </summary>
    public bool AutoRefresh { get; set; } = true;

    /// <summary>
    /// Auto-refresh interval in seconds
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Date the layout was last saved
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
