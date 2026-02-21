// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// DTO for UI preference settings
/// </summary>
public class UIPreferenceDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Theme { get; set; } = "auto";
    public string SidebarPosition { get; set; } = "left";
    public int SidebarWidth { get; set; } = 250;
    public string FontSize { get; set; } = "normal";
    public bool ShowBreadcrumbs { get; set; } = true;
    public bool ShowStatusBar { get; set; } = true;
    public bool ShowTopNavigation { get; set; } = true;
    public int DefaultPageSize { get; set; } = 20;
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public string TimeFormat { get; set; } = "hh:mm a";
    public string? CustomColorScheme { get; set; }
    public DateTime LastPreferenceUpdate { get; set; }
}

/// <summary>
/// DTO for creating/updating UI preferences
/// </summary>
public class CreateUpdateUIPreferenceDto
{
    public string? Theme { get; set; }
    public string? SidebarPosition { get; set; }
    public int? SidebarWidth { get; set; }
    public string? FontSize { get; set; }
    public bool? ShowBreadcrumbs { get; set; }
    public bool? ShowStatusBar { get; set; }
    public bool? ShowTopNavigation { get; set; }
    public int? DefaultPageSize { get; set; }
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public string? CustomColorScheme { get; set; }
}
