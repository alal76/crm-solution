// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Defines a structured custom field for any CRM entity type.
/// This is the field *schema* record — actual values are stored in <see cref="CustomField"/>.
/// </summary>
public class CustomFieldDefinition : BaseEntity
{
    /// <summary>Target entity type: Account, Contact, Lead, Opportunity, ServiceRequest, etc.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Internal machine-readable key (snake_case), unique per EntityType.</summary>
    public string FieldKey { get; set; } = string.Empty;

    /// <summary>Human-readable label shown in UI.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Field data type: Text, Number, Date, Boolean, Dropdown, MultiSelect, Url, Email, Phone.</summary>
    public string FieldType { get; set; } = "Text";

    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    /// <summary>Default value serialized as string.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>JSON array of allowed option values for Dropdown / MultiSelect fields.</summary>
    public string? OptionsJson { get; set; }

    /// <summary>Optional section / group label for UI grouping.</summary>
    public string? GroupName { get; set; }

    /// <summary>Validation rules defined for this field.</summary>
    public ICollection<CustomFieldValidationRule> ValidationRules { get; set; } = new List<CustomFieldValidationRule>();
}
