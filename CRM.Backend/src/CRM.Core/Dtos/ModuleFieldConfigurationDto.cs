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
