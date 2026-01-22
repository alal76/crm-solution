namespace CRM.Core.Entities;

/// <summary>
/// Stores UI configuration for each module including tab layouts, field configurations,
/// linked entities, and module-specific settings. Uses JSON for flexible configuration storage.
/// </summary>
public class ModuleUIConfig : BaseEntity
{
    /// <summary>
    /// The module this configuration belongs to (e.g., "Customers", "Contacts", "Leads")
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this module is enabled system-wide
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Display label for the module
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Module description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Icon name for the module (Material Icon name)
    /// </summary>
    public string IconName { get; set; } = "Folder";
    
    /// <summary>
    /// Display order in navigation
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// JSON array of tab configurations for the module form
    /// Format: [{"index": 0, "name": "Basic Info", "enabled": true, "order": 0}, ...]
    /// </summary>
    public string? TabsConfig { get; set; }
    
    /// <summary>
    /// JSON array of linked entities that can be associated with this module
    /// Format: [{"entityName": "Contacts", "relationshipType": "one-to-many", "enabled": true, "tabName": "Contacts"}, ...]
    /// </summary>
    public string? LinkedEntitiesConfig { get; set; }
    
    /// <summary>
    /// JSON object for list view column configuration
    /// Format: {"columns": [{"field": "name", "label": "Name", "width": 200, "visible": true, "sortable": true}, ...]}
    /// </summary>
    public string? ListViewConfig { get; set; }
    
    /// <summary>
    /// JSON object for detail view configuration
    /// Format: {"layout": "tabs", "showRelated": true, "relatedEntities": ["Contacts", "Notes"]}
    /// </summary>
    public string? DetailViewConfig { get; set; }
    
    /// <summary>
    /// JSON object for quick create form fields
    /// Format: {"fields": ["name", "email", "phone"], "requiredFields": ["name"]}
    /// </summary>
    public string? QuickCreateConfig { get; set; }
    
    /// <summary>
    /// JSON object for search and filter configuration
    /// Format: {"searchFields": ["name", "email"], "filters": [{"field": "status", "type": "select"}]}
    /// </summary>
    public string? SearchFilterConfig { get; set; }
    
    /// <summary>
    /// JSON object for module-specific settings
    /// Format: {"defaultView": "list", "pageSize": 25, "autoSave": false, ...}
    /// </summary>
    public string? ModuleSettings { get; set; }
}

/// <summary>
/// Default module configurations
/// </summary>
public static class DefaultModuleConfigs
{
    public static readonly string[] AllModules = new[]
    {
        ModuleNames.Customers,
        ModuleNames.Contacts,
        ModuleNames.Leads,
        ModuleNames.Opportunities,
        ModuleNames.Products,
        "Services",
        "Campaigns",
        "Quotes",
        "Tasks",
        "Activities",
        "Notes"
    };
    
    public static readonly Dictionary<string, string[]> DefaultLinkedEntities = new()
    {
        [ModuleNames.Customers] = new[] { "Contacts", "Opportunities", "Quotes", "Tasks", "Activities", "Notes" },
        [ModuleNames.Contacts] = new[] { "Customers", "Opportunities", "Tasks", "Activities", "Notes" },
        [ModuleNames.Leads] = new[] { "Tasks", "Activities", "Notes" },
        [ModuleNames.Opportunities] = new[] { "Customers", "Contacts", "Products", "Quotes", "Tasks", "Activities", "Notes" },
        [ModuleNames.Products] = new[] { "Opportunities", "Quotes" },
        ["Services"] = new[] { "Customers", "Quotes" },
        ["Campaigns"] = new[] { "Leads", "Contacts", "Tasks", "Activities" },
        ["Quotes"] = new[] { "Customers", "Contacts", "Products", "Opportunities" },
        ["Tasks"] = new[] { "Customers", "Contacts", "Leads", "Opportunities" },
        ["Activities"] = new[] { "Customers", "Contacts", "Leads", "Opportunities" },
        ["Notes"] = new[] { "Customers", "Contacts", "Leads", "Opportunities" }
    };
}
