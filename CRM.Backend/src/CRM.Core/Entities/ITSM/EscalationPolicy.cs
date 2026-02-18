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
/// Escalation policy for defining multi-level escalation workflows.
/// </summary>
[Table("ITSMEscalationPolicies")]
public class EscalationPolicy : BaseEntity
{
    /// <summary>Policy name (e.g., "Critical Incident Escalation")</summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Policy description</summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>Whether this policy is currently active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether this is the default escalation policy</summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>Priority order when multiple policies match</summary>
    public int Priority { get; set; } = 0;

    /// <summary>Escalation levels defined for this policy</summary>
    public ICollection<EscalationLevel> Levels { get; set; } = new List<EscalationLevel>();

    /// <summary>History of escalations using this policy</summary>
    public ICollection<EscalationHistory> EscalationHistories { get; set; } = new List<EscalationHistory>();
}

/// <summary>
/// Individual escalation level within a policy.
/// </summary>
[Table("ITSMEscalationLevels")]
public class EscalationLevel : BaseEntity
{
    /// <summary>Foreign key to the parent policy</summary>
    public int PolicyId { get; set; }

    /// <summary>Navigation property to the parent policy</summary>
    [ForeignKey("PolicyId")]
    public EscalationPolicy Policy { get; set; } = null!;

    /// <summary>Level number (1, 2, 3, etc.) for ordering</summary>
    public int LevelNumber { get; set; }

    /// <summary>Level name (e.g., "L1 Support", "Manager")</summary>
    [StringLength(200)]
    public string? Name { get; set; }

    /// <summary>Minutes after which to escalate to this level</summary>
    public int EscalateAfterMinutes { get; set; }

    /// <summary>User to notify at this level</summary>
    public int? NotifyUserId { get; set; }

    /// <summary>Navigation to notify user</summary>
    [ForeignKey("NotifyUserId")]
    public User? NotifyUser { get; set; }

    /// <summary>Team/group to notify at this level</summary>
    public int? NotifyTeamId { get; set; }

    /// <summary>Navigation to notify team</summary>
    [ForeignKey("NotifyTeamId")]
    public UserGroup? NotifyTeam { get; set; }

    /// <summary>Whether to send email notification</summary>
    public bool SendEmail { get; set; } = true;

    /// <summary>Whether to send SMS notification</summary>
    public bool SendSms { get; set; } = false;

    /// <summary>Email template ID to use for notification</summary>
    public int? EmailTemplateId { get; set; }

    /// <summary>Navigation to email template</summary>
    [ForeignKey("EmailTemplateId")]
    public EmailTemplate? EmailTemplate { get; set; }

    /// <summary>Custom notification template content</summary>
    [StringLength(4000)]
    public string? NotificationTemplate { get; set; }

    /// <summary>Escalation histories at this level</summary>
    public ICollection<EscalationHistory> EscalationHistories { get; set; } = new List<EscalationHistory>();
}

/// <summary>
/// Tracks the history of escalations for auditing purposes.
/// </summary>
[Table("ITSMEscalationHistories")]
public class EscalationHistory : BaseEntity
{
    /// <summary>The incident that was escalated</summary>
    public int IncidentId { get; set; }

    /// <summary>Navigation to the incident</summary>
    [ForeignKey("IncidentId")]
    public Incident Incident { get; set; } = null!;

    /// <summary>The policy used for escalation</summary>
    public int EscalationPolicyId { get; set; }

    /// <summary>Navigation to the policy</summary>
    [ForeignKey("EscalationPolicyId")]
    public EscalationPolicy EscalationPolicy { get; set; } = null!;

    /// <summary>The specific level escalated to</summary>
    public int EscalationLevelId { get; set; }

    /// <summary>Navigation to the level</summary>
    [ForeignKey("EscalationLevelId")]
    public EscalationLevel EscalationLevel { get; set; } = null!;

    /// <summary>When the escalation occurred</summary>
    public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>User who was notified (if any)</summary>
    public int? NotifiedUserId { get; set; }

    /// <summary>Navigation to notified user</summary>
    [ForeignKey("NotifiedUserId")]
    public User? NotifiedUser { get; set; }

    /// <summary>Team that was notified (if any)</summary>
    public int? NotifiedTeamId { get; set; }

    /// <summary>Navigation to notified team</summary>
    [ForeignKey("NotifiedTeamId")]
    public UserGroup? NotifiedTeam { get; set; }

    /// <summary>Reason for escalation</summary>
    [StringLength(1000)]
    public string? Reason { get; set; }

    /// <summary>Additional notes about the escalation</summary>
    [StringLength(4000)]
    public string? Notes { get; set; }
}
