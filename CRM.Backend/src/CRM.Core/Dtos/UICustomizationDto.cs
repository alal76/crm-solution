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
/// DTO for UI customization per module/page
/// </summary>
public class UICustomizationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string PageName { get; set; } = string.Empty;
    public string? VisibleColumns { get; set; }
    public string? DefaultSortColumn { get; set; }
    public string DefaultSortOrder { get; set; } = "asc";
    public string? StoredFilters { get; set; }
    public string? SavedSearches { get; set; }
    public string RowHeight { get; set; } = "normal";
    public bool ShowRowNumbers { get; set; } = true;
    public bool ShowFilters { get; set; } = true;
    public string? ColumnWidths { get; set; }
    public int RowsPerPage { get; set; } = 20;
}

/// <summary>
/// DTO for creating/updating UI customization
/// </summary>
public class CreateUpdateUICustomizationDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string PageName { get; set; } = string.Empty;
    public string[]? VisibleColumns { get; set; }
    public string? DefaultSortColumn { get; set; }
    public string? DefaultSortOrder { get; set; }
    public object? StoredFilters { get; set; }
    public string[]? SavedSearches { get; set; }
    public string? RowHeight { get; set; }
    public bool? ShowRowNumbers { get; set; }
    public bool? ShowFilters { get; set; }
    public Dictionary<string, int>? ColumnWidths { get; set; }
    public int? RowsPerPage { get; set; }
}
