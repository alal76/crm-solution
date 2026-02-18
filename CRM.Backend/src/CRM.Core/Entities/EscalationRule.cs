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
/// Represents an escalation rule for service requests
/// </summary>
[Table("EscalationRules")]
public class EscalationRule : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [StringLength(1024)]
    public string Description { get; set; }

    [Required]
    [Column(TypeName = "longtext")]
    public string Condition { get; set; } // JSON condition expression

    [Required]
    public EscalationMetric ConditionMetric { get; set; }

    [Required]
    public int ThresholdValue { get; set; }

    [ForeignKey("EscalateToUser")]
    public int? EscalateToUserId { get; set; }

    [ForeignKey("EscalateToGroup")]
    public int? EscalateToGroupId { get; set; }

    public bool SendNotification { get; set; } = true;

    public bool IsActive { get; set; } = true;

    /// <summary>Foreign key to SLA Policy (if this escalation is linked to an SLA)</summary>
    public int? SLAPolicyId { get; set; }

    /// <summary>Navigation property to SLA Policy</summary>
    [ForeignKey("SLAPolicyId")]
    public virtual SLAPolicy? SLAPolicy { get; set; }

    /// <summary>Metric used to trigger escalation</summary>
    public EscalationMetric? TriggerMetric { get; set; }

    /// <summary>User to reassign to when escalating</summary>
    public int? ReassignToUserId { get; set; }

    /// <summary>Navigation property to reassign-to user</summary>
    [ForeignKey("ReassignToUserId")]
    public virtual User? ReassignToUser { get; set; }

    /// <summary>Webhook URL to call when escalation is triggered</summary>
    [MaxLength(500)]
    public string? WebhookUrl { get; set; }

    /// <summary>JSON configuration for escalation actions</summary>
    [Column(TypeName = "longtext")]
    public string? ActionConfigJson { get; set; }

    /// <summary>JSON array of email recipients for escalation notification</summary>
    [Column(TypeName = "longtext")]
    public string? EmailRecipientsJson { get; set; }

    // Navigation properties
    public virtual User EscalateToUser { get; set; }
    public virtual UserGroup EscalateToGroup { get; set; }
}

/// <summary>
/// Escalation trigger metrics
/// </summary>
public enum EscalationMetric
{
    AgeMinutes = 0,
    PriorityLevel = 1,
    AssigneeGroup = 2
}
