// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// Assignment strategy types for auto-assignment rules
/// </summary>
public enum AssignmentStrategy
{
    /// <summary>Cycle through agents in order</summary>
    RoundRobin = 0,

    /// <summary>Match agent skills to service request requirements</summary>
    SkillBased = 1,

    /// <summary>Assign to agent with fewest open assignments</summary>
    LeastLoaded = 2
}

/// <summary>
/// DTO for assignment rule response
/// </summary>
public class AssignmentRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Strategy { get; set; } = "RoundRobin";
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public string? CategoryFilter { get; set; }
    public string? PriorityFilter { get; set; }
    public int? QueueId { get; set; }
    public string? RequiredSkills { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating an assignment rule
/// </summary>
public class CreateAssignmentRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Strategy { get; set; } = "RoundRobin";
    public int Priority { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string? CategoryFilter { get; set; }
    public string? PriorityFilter { get; set; }
    public int? QueueId { get; set; }
    public string? RequiredSkills { get; set; }
}

/// <summary>
/// DTO for updating an assignment rule
/// </summary>
public class UpdateAssignmentRuleDto
{
    public string? Name { get; set; }
    public string? Strategy { get; set; }
    public int? Priority { get; set; }
    public bool? IsActive { get; set; }
    public string? CategoryFilter { get; set; }
    public string? PriorityFilter { get; set; }
    public int? QueueId { get; set; }
    public string? RequiredSkills { get; set; }
}

/// <summary>
/// Result of an auto-assignment operation
/// </summary>
public class AutoAssignmentResultDto
{
    public int ServiceRequestId { get; set; }
    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public string? StrategyUsed { get; set; }
    public string? MatchedRuleName { get; set; }
    public bool Success { get; set; }
    public string? Reason { get; set; }
}
