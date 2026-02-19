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
/// Represents a Service Level Agreement policy.
/// </summary>
[Table("SLAPolicies")]
public class SLAPolicy : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1024)]
    public string? Description { get; set; }

    [Required]
    public ServicePriority Priority { get; set; }

    [Required]
    public int InitialResponseTimeMinutes { get; set; }

    [Required]
    public int ResolutionTimeMinutes { get; set; }

    public bool WorkingHoursOnly { get; set; }

    [Column(TypeName = "longtext")]
    public string EscalationPath { get; set; } = string.Empty; // JSON array of user IDs in escalation order

    public bool IsActive { get; set; } = true;

    /// <summary>JSON object with business hours configuration</summary>
    [Column(TypeName = "longtext")]
    public string? BusinessHours { get; set; }

    /// <summary>JSON array of case types this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? CaseTypesJson { get; set; }

    /// <summary>JSON array of customer segments this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? CustomerSegmentsJson { get; set; }

    /// <summary>JSON array of customer tiers this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? CustomerTiersJson { get; set; }

    /// <summary>JSON object with conditions to match for this policy</summary>
    [Column(TypeName = "longtext")]
    public string? MatchConditionsJson { get; set; }

    /// <summary>JSON array of products this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? ProductsJson { get; set; }

    /// <summary>
    /// Business hours configuration ID for this SLA policy
    /// </summary>
    public int? BusinessHoursId { get; set; }

    /// <summary>
    /// Escalation rules associated with this SLA policy
    /// </summary>
    public virtual ICollection<EscalationRule> EscalationRules { get; set; } = new List<EscalationRule>();
}

/// <summary>
/// Service request priority levels
/// </summary>
public enum ServicePriority
{
    Critical = 0,
    High = 1,
    Medium = 2,
    Low = 3
}
