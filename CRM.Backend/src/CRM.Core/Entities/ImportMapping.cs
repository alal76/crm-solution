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
/// Reusable import column mapping template.
/// Defines how source columns map to target entity fields during import.
/// </summary>
[Table("ImportMappings")]
public class ImportMapping : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// JSON array defining column mappings: [{sourceColumn, targetField, transform}]
    /// </summary>
    public string MappingDefinition { get; set; } = "[]";

    public bool IsDefault { get; set; }

    public int CreatedByUserId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
