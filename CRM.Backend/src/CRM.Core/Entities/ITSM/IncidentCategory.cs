// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.ITSM;

/// <summary>
/// Represents an incident category for ITSM incident classification.
/// </summary>
[Table("IncidentCategories")]
public class IncidentCategory : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? SubCategory { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int DefaultPriority { get; set; } = 3;

    public bool IsActive { get; set; } = true;
}
