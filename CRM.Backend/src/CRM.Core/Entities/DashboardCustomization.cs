// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
