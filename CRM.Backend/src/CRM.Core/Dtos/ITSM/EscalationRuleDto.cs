// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for escalation rule response
/// </summary>
public class EscalationRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Queue { get; set; }
    public int AgeInMinutes { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int? TargetId { get; set; }
    public string? TargetName { get; set; }
    public int MaxAttempts { get; set; }
    public int RetryIntervalMinutes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating escalation rule
/// </summary>
public class CreateEscalationRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Queue { get; set; }
    public int AgeInMinutes { get; set; } = 60;
    public string TargetType { get; set; } = string.Empty;
    public int? TargetId { get; set; }
    public string? TargetName { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public int RetryIntervalMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating escalation rule
/// </summary>
public class UpdateEscalationRuleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Queue { get; set; }
    public int? AgeInMinutes { get; set; }
    public string? TargetType { get; set; }
    public int? TargetId { get; set; }
    public string? TargetName { get; set; }
    public int? MaxAttempts { get; set; }
    public int? RetryIntervalMinutes { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO for escalation rule test result
/// </summary>
public class EscalationRuleTestResultDto
{
    public int RuleId { get; set; }
    public int ServiceRequestId { get; set; }
    public bool RuleMatched { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public EscalationRuleDto? Rule { get; set; }
    public string TestMessage { get; set; } = string.Empty;
}
