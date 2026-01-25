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
