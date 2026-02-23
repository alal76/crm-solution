// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Represents a field-level change log entry for tracking individual property changes
/// on any entity. Enables detailed audit trail with old/new value comparisons.
/// </summary>
[Table("FieldChangeLogs")]
public class FieldChangeLog : BaseEntity
{
    /// <summary>
    /// The type of entity that was changed (e.g., Account, Contact, Opportunity).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the entity that was changed.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// The name of the field/property that was changed.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// The previous value of the field (serialized as string). Null for newly created fields.
    /// </summary>
    [MaxLength(2000)]
    public string? OldValue { get; set; }

    /// <summary>
    /// The new value of the field (serialized as string). Null for deleted fields.
    /// </summary>
    [MaxLength(2000)]
    public string? NewValue { get; set; }

    /// <summary>
    /// The CLR data type of the field (e.g., String, Int32, DateTime).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user who made the change.
    /// </summary>
    public int ChangedByUserId { get; set; }
}
