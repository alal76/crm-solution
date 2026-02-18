// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Module-specific UI customization (which columns to show, sort defaults, etc.)
/// </summary>
[Table("UICustomizations")]
public class UICustomization : BaseEntity
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
    /// Module name (e.g., "Accounts", "Contacts", "Opportunities")
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// Page/view name (e.g., "ListView", "DetailPage", "DashboardView")
    /// </summary>
    public string PageName { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of visible column names
    /// </summary>
    public string? VisibleColumns { get; set; }

    /// <summary>
    /// Default sort column
    /// </summary>
    public string? DefaultSortColumn { get; set; }

    /// <summary>
    /// Default sort order: 'asc' or 'desc'
    /// </summary>
    public string DefaultSortOrder { get; set; } = "asc";

    /// <summary>
    /// Stored filter criteria in JSON format
    /// </summary>
    public string? StoredFilters { get; set; }

    /// <summary>
    /// Stored searches as JSON array
    /// </summary>
    public string? SavedSearches { get; set; }

    /// <summary>
    /// Row height preference: 'compact', 'normal', 'comfortable'
    /// </summary>
    public string RowHeight { get; set; } = "normal";

    /// <summary>
    /// Whether to show row numbers
    /// </summary>
    public bool ShowRowNumbers { get; set; } = true;

    /// <summary>
    /// Whether to show filters row
    /// </summary>
    public bool ShowFilters { get; set; } = true;

    /// <summary>
    /// Custom column widths in JSON format
    /// </summary>
    public string? ColumnWidths { get; set; }

    /// <summary>
    /// Number of rows per page preference
    /// </summary>
    public int RowsPerPage { get; set; } = 20;
}
