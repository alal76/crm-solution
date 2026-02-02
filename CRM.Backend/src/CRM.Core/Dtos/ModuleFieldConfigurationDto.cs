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
/// DTO for module field configuration responses
/// </summary>
public class ModuleFieldConfigurationDto
{
    public int Id { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public int TabIndex { get; set; } = 0;
    public string TabName { get; set; } = "Basic Info";
    public int DisplayOrder { get; set; } = 0;
    public bool IsEnabled { get; set; } = true;
    public bool IsRequired { get; set; } = false;
    public int GridSize { get; set; } = 6;
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? Options { get; set; }
    public string? ParentField { get; set; }
    public string? ParentFieldValue { get; set; }
    public bool IsReorderable { get; set; } = true;
    public bool IsRequiredConfigurable { get; set; } = true;
    public bool IsHideable { get; set; } = true;
}

/// <summary>
/// DTO for creating module field configuration
/// </summary>
public class CreateModuleFieldConfigurationDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public int TabIndex { get; set; } = 0;
    public string TabName { get; set; } = "Basic Info";
    public int DisplayOrder { get; set; } = 0;
    public bool IsEnabled { get; set; } = true;
    public bool IsRequired { get; set; } = false;
    public int GridSize { get; set; } = 6;
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? Options { get; set; }
    public string? ParentField { get; set; }
    public string? ParentFieldValue { get; set; }
    public bool IsReorderable { get; set; } = true;
    public bool IsRequiredConfigurable { get; set; } = true;
    public bool IsHideable { get; set; } = true;
}

/// <summary>
/// DTO for updating module field configuration
/// </summary>
public class UpdateModuleFieldConfigurationDto
{
    public string? FieldLabel { get; set; }
    public int? TabIndex { get; set; }
    public string? TabName { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsEnabled { get; set; }
    public bool? IsRequired { get; set; }
    public int? GridSize { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? Options { get; set; }
}

/// <summary>
/// DTO for bulk updating field order
/// </summary>
public class BulkUpdateFieldOrderDto
{
    public string ModuleName { get; set; } = string.Empty;
    public int TabIndex { get; set; }
    public List<FieldOrderItem> Fields { get; set; } = new();
}

public class FieldOrderItem
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }
}
