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
