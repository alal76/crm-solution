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
/// DTO for field master data link
/// </summary>
public class FieldMasterDataLinkDto
{
    public int Id { get; set; }
    public int FieldConfigurationId { get; set; }
    public string SourceType { get; set; } = "LookupCategory";
    public string SourceName { get; set; } = string.Empty;
    public string DisplayField { get; set; } = "Value";
    public string ValueField { get; set; } = "Key";
    public string? FilterExpression { get; set; }
    public string? DependsOnField { get; set; }
    public string? DependsOnSourceColumn { get; set; }
    public bool AllowFreeText { get; set; }
    public string? ValidationType { get; set; }
    public string? ValidationPattern { get; set; }
    public string? ValidationMessage { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for creating/updating field master data link
/// </summary>
public class CreateFieldMasterDataLinkDto
{
    public int FieldConfigurationId { get; set; }
    public string SourceType { get; set; } = "LookupCategory";
    public string SourceName { get; set; } = string.Empty;
    public string DisplayField { get; set; } = "Value";
    public string ValueField { get; set; } = "Key";
    public string? FilterExpression { get; set; }
    public string? DependsOnField { get; set; }
    public string? DependsOnSourceColumn { get; set; }
    public bool AllowFreeText { get; set; }
    public string? ValidationType { get; set; }
    public string? ValidationPattern { get; set; }
    public string? ValidationMessage { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for available master data sources
/// </summary>
public class MasterDataSourceDto
{
    public string SourceType { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> AvailableFields { get; set; } = new();
}

/// <summary>
/// DTO for master data lookup result
/// </summary>
public class MasterDataLookupResultDto
{
    public string Value { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}
