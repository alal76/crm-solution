namespace CRM.Core.Dtos;

/// <summary>
/// DTO for module UI configuration responses
/// </summary>
public class ModuleUIConfigDto
{
    public int Id { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string IconName { get; set; } = "Folder";
    public int DisplayOrder { get; set; } = 0;
    public string? TabsConfig { get; set; }
    public string? LinkedEntitiesConfig { get; set; }
    public string? ListViewConfig { get; set; }
    public string? DetailViewConfig { get; set; }
    public string? QuickCreateConfig { get; set; }
    public string? SearchFilterConfig { get; set; }
    public string? ModuleSettings { get; set; }
}

/// <summary>
/// DTO for creating module UI configuration
/// </summary>
public class CreateModuleUIConfigDto
{
    public string ModuleName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string IconName { get; set; } = "Folder";
    public int DisplayOrder { get; set; } = 0;
    public string? TabsConfig { get; set; }
    public string? LinkedEntitiesConfig { get; set; }
    public string? ListViewConfig { get; set; }
    public string? DetailViewConfig { get; set; }
    public string? QuickCreateConfig { get; set; }
    public string? SearchFilterConfig { get; set; }
    public string? ModuleSettings { get; set; }
}

/// <summary>
/// DTO for updating module UI configuration
/// </summary>
public class UpdateModuleUIConfigDto
{
    public bool? IsEnabled { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public int? DisplayOrder { get; set; }
    public string? TabsConfig { get; set; }
    public string? LinkedEntitiesConfig { get; set; }
    public string? ListViewConfig { get; set; }
    public string? DetailViewConfig { get; set; }
    public string? QuickCreateConfig { get; set; }
    public string? SearchFilterConfig { get; set; }
    public string? ModuleSettings { get; set; }
}

/// <summary>
/// Tab configuration within a module
/// </summary>
public class TabConfigItem
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public int Order { get; set; }
    public string? Icon { get; set; }
}

/// <summary>
/// Linked entity configuration
/// </summary>
public class LinkedEntityConfigItem
{
    public string EntityName { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = "one-to-many"; // one-to-one, one-to-many, many-to-many
    public bool Enabled { get; set; } = true;
    public string? TabName { get; set; }
    public int DisplayOrder { get; set; }
    public string? ForeignKeyField { get; set; }
}

/// <summary>
/// List view column configuration
/// </summary>
public class ListViewColumnConfig
{
    public string Field { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Width { get; set; } = 150;
    public bool Visible { get; set; } = true;
    public bool Sortable { get; set; } = true;
    public int Order { get; set; }
    public string? Format { get; set; }
}

/// <summary>
/// Complete module configuration response including fields
/// </summary>
public class CompleteModuleConfigDto
{
    public ModuleUIConfigDto ModuleConfig { get; set; } = new();
    public List<ModuleFieldConfigurationDto> FieldConfigurations { get; set; } = new();
    public List<TabConfigItem> Tabs { get; set; } = new();
    public List<LinkedEntityConfigItem> LinkedEntities { get; set; } = new();
}

/// <summary>
/// Request to save complete module configuration (tabs, fields, linked entities) in a single call
/// </summary>
public class SaveCompleteModuleConfigDto
{
    public List<TabConfigItem> Tabs { get; set; } = new();
    public List<SaveFieldConfigItem> Fields { get; set; } = new();
    public List<LinkedEntityConfigItem> LinkedEntities { get; set; } = new();
}

/// <summary>
/// Field configuration item for saving
/// </summary>
public class SaveFieldConfigItem
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public int GridSize { get; set; } = 6;
    public string? FieldType { get; set; }
    public string? FieldLabel { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? Options { get; set; }
}

/// <summary>
/// Batch update request for module UI settings
/// </summary>
public class BatchModuleUIConfigUpdateDto
{
    public List<UpdateModuleUIConfigItem> Modules { get; set; } = new();
}

public class UpdateModuleUIConfigItem
{
    public string ModuleName { get; set; } = string.Empty;
    public bool? IsEnabled { get; set; }
    public int? DisplayOrder { get; set; }
}
