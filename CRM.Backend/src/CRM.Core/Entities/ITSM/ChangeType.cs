// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.ITSM;

/// <summary>
/// Represents an ITSM Change Type configuration.
/// Defines the classification and workflow requirements for changes.
/// </summary>
public class ChangeTypeEntity : BaseEntity
{
    /// <summary>
    /// The name of the change type (e.g., Standard, Normal, Emergency, Major)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the change type and when it should be used
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this change type requires CAB (Change Advisory Board) review
    /// </summary>
    public bool RequiresCAB { get; set; }

    /// <summary>
    /// Whether this change type requires approval before implementation
    /// </summary>
    public bool RequiresApproval { get; set; }

    /// <summary>
    /// Default risk level for this change type (Low, Medium, High)
    /// </summary>
    [StringLength(20)]
    public string DefaultRiskLevel { get; set; } = "Medium";

    /// <summary>
    /// Minimum lead time in days required for this change type
    /// </summary>
    public int LeadTimeDays { get; set; }

    /// <summary>
    /// Whether this change type is currently active and available for use
    /// </summary>
    public bool IsActive { get; set; } = true;
}
