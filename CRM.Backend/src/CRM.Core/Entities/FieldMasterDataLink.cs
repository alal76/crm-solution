namespace CRM.Core.Entities;

/// <summary>
/// Links a field to master data for validation/dropdown population
/// </summary>
public class FieldMasterDataLink : BaseEntity
{
    /// <summary>
    /// Foreign key to the field configuration
    /// </summary>
    public int FieldConfigurationId { get; set; }
    
    /// <summary>
    /// Navigation property to field configuration
    /// </summary>
    public ModuleFieldConfiguration FieldConfiguration { get; set; } = null!;
    
    /// <summary>
    /// The source type for master data (Table, LookupCategory, Api)
    /// </summary>
    public string SourceType { get; set; } = "LookupCategory";
    
    /// <summary>
    /// Name of the source (table name, lookup category name, or API endpoint)
    /// For SourceType=Table: "ZipCodes", "Products", "Customers", etc.
    /// For SourceType=LookupCategory: "Currency", "LeadSource", etc.
    /// For SourceType=Api: "/api/zipcodes/countries"
    /// </summary>
    public string SourceName { get; set; } = string.Empty;
    
    /// <summary>
    /// The column/field to use as the display value
    /// </summary>
    public string DisplayField { get; set; } = "Value";
    
    /// <summary>
    /// The column/field to use as the stored value
    /// </summary>
    public string ValueField { get; set; } = "Key";
    
    /// <summary>
    /// Optional filter expression (JSON format)
    /// Example: {"CountryCode": "US"} or {"IsActive": true}
    /// </summary>
    public string? FilterExpression { get; set; }
    
    /// <summary>
    /// Field name from the same entity to use for dynamic filtering
    /// E.g., if this field is "city" and depends on "state" field
    /// </summary>
    public string? DependsOnField { get; set; }
    
    /// <summary>
    /// The source column to filter by when DependsOnField has a value
    /// E.g., "StateCode" in the master data table
    /// </summary>
    public string? DependsOnSourceColumn { get; set; }
    
    /// <summary>
    /// Whether to allow values not in the master data (free text)
    /// </summary>
    public bool AllowFreeText { get; set; } = false;
    
    /// <summary>
    /// Validation type: Required, Pattern, Custom
    /// </summary>
    public string? ValidationType { get; set; }
    
    /// <summary>
    /// Validation pattern (regex) for custom validation
    /// </summary>
    public string? ValidationPattern { get; set; }
    
    /// <summary>
    /// Custom validation error message
    /// </summary>
    public string? ValidationMessage { get; set; }
    
    /// <summary>
    /// Sort order when multiple links exist for cascading
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this link is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Master data source types
/// </summary>
public static class MasterDataSourceTypes
{
    public const string LookupCategory = "LookupCategory";
    public const string Table = "Table";
    public const string Api = "Api";
}

/// <summary>
/// Available tables that can be used as master data sources
/// </summary>
public static class MasterDataTables
{
    public const string ZipCodes = "ZipCodes";
    public const string Products = "Products";
    public const string Customers = "Customers";
    public const string Contacts = "Contacts";
    public const string Users = "Users";
    public const string ServiceRequestTypes = "ServiceRequestTypes";
    public const string LookupItems = "LookupItems";
}
