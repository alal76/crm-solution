// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for Escalation Rule
/// </summary>
public class EscalationRuleDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Condition { get; set; } // JSON

    public string ConditionMetric { get; set; } // AgeMinutes, PriorityLevel, AssigneeGroup

    public int ThresholdValue { get; set; }

    public int? EscalateToUserId { get; set; }

    public int? EscalateToGroupId { get; set; }

    public bool SendNotification { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create Escalation Rule DTO
/// </summary>
public class CreateEscalationRuleDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string Condition { get; set; }

    public string ConditionMetric { get; set; }

    public int ThresholdValue { get; set; }

    public int? EscalateToUserId { get; set; }

    public int? EscalateToGroupId { get; set; }

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
