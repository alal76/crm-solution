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
/// DTO for dashboard customization
/// </summary>
public class DashboardCustomizationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DashboardName { get; set; } = string.Empty;
    public string LayoutConfig { get; set; } = string.Empty;
    public string Widgets { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public int GridColumns { get; set; } = 12;
    public bool AutoRefresh { get; set; } = true;
    public int RefreshIntervalSeconds { get; set; } = 60;
    public DateTime LastModified { get; set; }
}

/// <summary>
/// DTO for creating/updating dashboard customization
/// </summary>
public class CreateUpdateDashboardCustomizationDto
{
    public string DashboardName { get; set; } = string.Empty;
    public object? LayoutConfig { get; set; }
    public object[]? Widgets { get; set; }
    public bool? IsDefault { get; set; }
    public int? GridColumns { get; set; }
    public bool? AutoRefresh { get; set; }
    public int? RefreshIntervalSeconds { get; set; }
}

/// <summary>
/// DTO for dashboard widget
/// </summary>
public class DashboardWidgetDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 3;
    public int Height { get; set; } = 2;
    public string Title { get; set; } = string.Empty;
    public object? Config { get; set; }
}
