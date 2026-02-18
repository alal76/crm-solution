// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.ITSM;

/// <summary>
/// Enumeration for escalation target types
/// </summary>
public enum EscalationTargetType
{
    /// <summary>Escalate to specific user</summary>
    User = 0,

    /// <summary>Escalate to user group/team</summary>
    Group = 1,

    /// <summary>Escalate to manager chain</summary>
    Manager = 2,

    /// <summary>Escalate to queue</summary>
    Queue = 3
}

/// <summary>
/// Escalation rule entity for configuring escalation policies
/// </summary>
[Table("ITSMEscalationRules")]
public class EscalationRule : BaseEntity
{
    /// <summary>Rule name (e.g., "Critical Issue Auto-Escalate")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Rule description</summary>
    public string? Description { get; set; }

    /// <summary>Priority level this rule applies to (Critical, High, Medium, Low)</summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>SLA Policy ID this rule belongs to (optional)</summary>
    public int? SLAPolicyId { get; set; }

    /// <summary>Category this rule applies to (null = all)</summary>
    public string? Category { get; set; }

    /// <summary>Queue this rule applies to (null = all)</summary>
    public string? Queue { get; set; }

    /// <summary>Minutes of aging before escalation (e.g., 60 = escalate after 1 hour)</summary>
    public int AgeInMinutes { get; set; } = 60;

    /// <summary>Type of escalation target</summary>
    public EscalationTargetType TargetType { get; set; }

    /// <summary>Target ID (UserId, GroupId, or QueueId depending on TargetType)</summary>
    public int? TargetId { get; set; }

    /// <summary>Target name for reference</summary>
    public string? TargetName { get; set; }

    /// <summary>Maximum escalation attempts before giving up</summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>Minutes to wait between escalation retry attempts</summary>
    public int RetryIntervalMinutes { get; set; } = 15;

    /// <summary>Whether this rule is currently active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Optional JSON for additional conditions</summary>
    public string? Conditions { get; set; }
}
