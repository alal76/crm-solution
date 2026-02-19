// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for Escalation Rule.
/// </summary>
public class EscalationRuleDto
{
    public int Id { get; set; }

    /// <summary>
    /// Name of the escalation rule.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the escalation rule.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// JSON condition for the rule.
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Condition metric (e.g., AgeMinutes, PriorityLevel, AssigneeGroup).
    /// </summary>
    public string ConditionMetric { get; set; } = string.Empty;

    /// <summary>
    /// Threshold value for escalation.
    /// </summary>
    public int ThresholdValue { get; set; }

    /// <summary>
    /// User ID to escalate to.
    /// </summary>
    public int? EscalateToUserId { get; set; }

    /// <summary>
    /// Group ID to escalate to.
    /// </summary>
    public int? EscalateToGroupId { get; set; }

    /// <summary>
    /// Whether to send notification on escalation.
    /// </summary>
    public bool SendNotification { get; set; }

    /// <summary>
    /// Whether the rule is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date and time the rule was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time the rule was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create Escalation Rule DTO
/// </summary>
public class CreateEscalationRuleDto
{
    /// <summary>
    /// Name of the escalation rule.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the escalation rule.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// JSON condition for the rule.
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Condition metric for the rule.
    /// </summary>
    public string ConditionMetric { get; set; } = string.Empty;

    /// <summary>
    /// Threshold value for escalation.
    /// </summary>
    public int ThresholdValue { get; set; }

    /// <summary>
    /// User ID to escalate to.
    /// </summary>
    public int? EscalateToUserId { get; set; }

    /// <summary>
    /// Group ID to escalate to.
    /// </summary>
    public int? EscalateToGroupId { get; set; }

    /// <summary>
    /// Whether to send notification on escalation.
    /// </summary>
    public bool SendNotification { get; set; } = true;
}

/// <summary>
/// Update Escalation Rule DTO
/// </summary>
public class UpdateEscalationRuleDto
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Condition { get; set; }

    public string? ConditionMetric { get; set; }

    public int? ThresholdValue { get; set; }

    public int? EscalateToUserId { get; set; }

    public int? EscalateToGroupId { get; set; }

    public bool? SendNotification { get; set; }

    public bool? IsActive { get; set; }
}
