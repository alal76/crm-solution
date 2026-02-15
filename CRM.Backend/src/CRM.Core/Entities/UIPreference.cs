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
/// User-specific UI preferences including theme, layout, and display options
/// </summary>
[Table("UIPreferences")]
public class UIPreference : BaseEntity
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
    /// Theme preference: 'light', 'dark', 'auto'
    /// </summary>
    public string Theme { get; set; } = "auto";

    /// <summary>
    /// Sidebar position: 'left', 'right', 'hidden'
    /// </summary>
    public string SidebarPosition { get; set; } = "left";

    /// <summary>
    /// Sidebar width in pixels
    /// </summary>
    public int SidebarWidth { get; set; } = 250;

    /// <summary>
    /// Font size adjustment: 'small', 'normal', 'large' or percentage (e.g., "110")
    /// </summary>
    public string FontSize { get; set; } = "normal";

    /// <summary>
    /// Whether to show navigation breadcrumbs
    /// </summary>
    public bool ShowBreadcrumbs { get; set; } = true;

    /// <summary>
    /// Whether to show status bar
    /// </summary>
    public bool ShowStatusBar { get; set; } = true;

    /// <summary>
    /// Whether to show top navigation menu
    /// </summary>
    public bool ShowTopNavigation { get; set; } = true;

    /// <summary>
    /// Default page size for lists
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Preferred date format
    /// </summary>
    public string DateFormat { get; set; } = "MM/dd/yyyy";

    /// <summary>
    /// Preferred time format
    /// </summary>
    public string TimeFormat { get; set; } = "hh:mm a";

    /// <summary>
    /// Color scheme for custom branding
    /// </summary>
    public string? CustomColorScheme { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastPreferenceUpdate { get; set; } = DateTime.UtcNow;
}
