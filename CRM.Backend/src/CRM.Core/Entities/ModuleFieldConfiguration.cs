namespace CRM.Core.Entities;

/// <summary>
/// Stores field-level configuration for each module (Customers, Contacts, Leads, etc.)
/// Allows admins to enable/disable fields, set them as optional/mandatory, and reorder them
/// </summary>
public class ModuleFieldConfiguration : BaseEntity
{
    /// <summary>
    /// The module this configuration belongs to (e.g., "Customers", "Contacts", "Leads")
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;
    
    /// <summary>
    /// The field identifier (e.g., "firstName", "email", "company")
    /// </summary>
    public string FieldName { get; set; } = string.Empty;
    
    /// <summary>
    /// Display label for the field
    /// </summary>
    public string FieldLabel { get; set; } = string.Empty;
    
    /// <summary>
    /// Field type (text, email, number, date, select, checkbox, etc.)
    /// </summary>
    public string FieldType { get; set; } = "text";
    
    /// <summary>
    /// Tab index (0 = Basic Info, 1 = Business, 2 = Contact Preferences, etc.)
    /// </summary>
    public int TabIndex { get; set; } = 0;
    
    /// <summary>
    /// Tab name
    /// </summary>
    public string TabName { get; set; } = "Basic Info";
    
    /// <summary>
    /// Display order within the tab (lower numbers appear first)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this field is enabled/visible
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Whether this field is required
    /// </summary>
    public bool IsRequired { get; set; } = false;
    
    /// <summary>
    /// Grid size (1-12 for Material-UI Grid system)
    /// </summary>
    public int GridSize { get; set; } = 6;
    
    /// <summary>
    /// Placeholder text
    /// </summary>
    public string? Placeholder { get; set; }
    
    /// <summary>
    /// Help text/tooltip
    /// </summary>
    public string? HelpText { get; set; }
    
    /// <summary>
    /// For select/dropdown fields - comma-separated options or reference to enum
    /// </summary>
    public string? Options { get; set; }
    
    /// <summary>
    /// Parent field name (for conditional visibility)
    /// </summary>
    public string? ParentField { get; set; }
    
    /// <summary>
    /// Parent field value required for this field to show (for conditional visibility)
    /// </summary>
    public string? ParentFieldValue { get; set; }
    
    /// <summary>
    /// Whether this field can be reordered by admin
    /// </summary>
    public bool IsReorderable { get; set; } = true;
    
    /// <summary>
    /// Whether this field's required status can be changed by admin
    /// </summary>
    public bool IsRequiredConfigurable { get; set; } = true;
    
    /// <summary>
    /// Whether this field can be hidden by admin
    /// </summary>
    public bool IsHideable { get; set; } = true;
}

/// <summary>
/// Supported module names
/// </summary>
public static class ModuleNames
{
    public const string Customers = "Customers";
    public const string Contacts = "Contacts";
    public const string Leads = "Leads";
    public const string Opportunities = "Opportunities";
    public const string Products = "Products";
}

/// <summary>
/// Supported field types
/// </summary>
public static class FieldTypes
{
    public const string Text = "text";
    public const string Email = "email";
    public const string Number = "number";
    public const string Date = "date";
    public const string DateTime = "datetime";
    public const string Select = "select";
    public const string MultiSelect = "multiselect";
    public const string Checkbox = "checkbox";
    public const string TextArea = "textarea";
    public const string Phone = "phone";
    public const string Url = "url";
    public const string Currency = "currency";
}
