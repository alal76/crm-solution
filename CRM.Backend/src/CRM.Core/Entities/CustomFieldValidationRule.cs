// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// A validation rule attached to a <see cref="CustomFieldDefinition"/>.
/// </summary>
public class CustomFieldValidationRule : BaseEntity
{
    public int CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;

    /// <summary>Rule type: Required, MinLength, MaxLength, Regex, Min, Max, Email, Url.</summary>
    public string RuleType { get; set; } = string.Empty;

    /// <summary>The threshold / pattern value (e.g. "5" for MinLength, or a regex string).</summary>
    public string? RuleValue { get; set; }

    /// <summary>User-facing error message when this rule fails.</summary>
    public string? ErrorMessage { get; set; }
}
