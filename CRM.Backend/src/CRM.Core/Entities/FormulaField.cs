// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Associates a formula expression with a <see cref="CustomFieldDefinition"/>.
/// When the engine evaluates the formula the result overwrites the field value.
/// </summary>
public class FormulaField : BaseEntity
{
    public int CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;

    /// <summary>
    /// Formula expression using {FieldName} tokens and standard arithmetic operators.
    /// Cross-object references use {Entity.FieldName} syntax.
    /// Example: "{UnitPrice} * {Quantity} * (1 - {DiscountPercent} / 100)"
    /// </summary>
    public string Formula { get; set; } = string.Empty;

    /// <summary>Expected result data type: Number, Text, Date.</summary>
    public string ResultType { get; set; } = "Number";

    public bool IsActive { get; set; } = true;
}
