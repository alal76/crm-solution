// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Defines a rollup aggregation field that summarises child record values onto a parent entity.
/// </summary>
public class RollupField : BaseEntity
{
    public string ParentEntityType { get; set; } = string.Empty;
    public string ChildEntityType { get; set; } = string.Empty;

    /// <summary>FK property name on the child that points to the parent (e.g. "AccountId").</summary>
    public string RelationshipField { get; set; } = string.Empty;

    /// <summary>Child field whose values are aggregated (e.g. "Amount").</summary>
    public string AggregateField { get; set; } = string.Empty;

    /// <summary>Aggregation function: Count, Sum, Avg, Min, Max.</summary>
    public string AggregateFunction { get; set; } = "Sum";

    /// <summary>
    /// Optional JSON filter condition applied to child records before aggregation.
    /// Format: {"field": "Status", "operator": "eq", "value": "Won"}
    /// </summary>
    public string? FilterCondition { get; set; }

    /// <summary>Target custom field definition that will receive the rollup result.</summary>
    public int? TargetCustomFieldDefinitionId { get; set; }

    public bool IsActive { get; set; } = true;
}
