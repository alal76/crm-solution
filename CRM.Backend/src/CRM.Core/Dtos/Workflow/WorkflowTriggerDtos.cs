// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities.Workflow;

namespace CRM.Core.DTOs.Workflow;

/// <summary>
/// DTO for workflow trigger data.
/// </summary>
public class WorkflowTriggerDto
{
    public int Id { get; set; }
    public int WorkflowDefinitionId { get; set; }
    public string? WorkflowName { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkflowTriggerType TriggerType { get; set; }
    public string TriggerTypeName => TriggerType.ToString();
    public string? EntityType { get; set; }
    public string? EventName { get; set; }
    public string? CronExpression { get; set; }
    public string? FilterConditions { get; set; }
    public string? WatchedField { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? Description { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public DateTime? NextScheduledAt { get; set; }
    public int ExecutionCount { get; set; }
    public int DelaySeconds { get; set; }
    public bool RunAsync { get; set; }
    public int MaxRetries { get; set; }
    public int? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new workflow trigger.
/// </summary>
public class CreateWorkflowTriggerDto
{
    [Required]
    public int WorkflowDefinitionId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public WorkflowTriggerType TriggerType { get; set; }

    [MaxLength(100)]
    public string? EntityType { get; set; }

    [MaxLength(200)]
    public string? EventName { get; set; }

    [MaxLength(100)]
    public string? CronExpression { get; set; }

    public string? FilterConditions { get; set; }

    [MaxLength(100)]
    public string? WatchedField { get; set; }

    [MaxLength(500)]
    public string? OldValue { get; set; }

    [MaxLength(500)]
    public string? NewValue { get; set; }

    public bool IsActive { get; set; } = true;

    public int Priority { get; set; } = 100;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int DelaySeconds { get; set; } = 0;

    public bool RunAsync { get; set; } = true;

    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// DTO for updating an existing workflow trigger.
/// </summary>
public class UpdateWorkflowTriggerDto
{
    [Required]
    public int Id { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    public WorkflowTriggerType? TriggerType { get; set; }

    [MaxLength(100)]
    public string? EntityType { get; set; }

    [MaxLength(200)]
    public string? EventName { get; set; }

    [MaxLength(100)]
    public string? CronExpression { get; set; }

    public string? FilterConditions { get; set; }

    [MaxLength(100)]
    public string? WatchedField { get; set; }

    [MaxLength(500)]
    public string? OldValue { get; set; }

    [MaxLength(500)]
    public string? NewValue { get; set; }

    public bool? IsActive { get; set; }

    public int? Priority { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? DelaySeconds { get; set; }

    public bool? RunAsync { get; set; }

    public int? MaxRetries { get; set; }
}

/// <summary>
/// DTO for workflow trigger execution request.
/// </summary>
public class TriggerExecutionRequest
{
    /// <summary>
    /// Entity type that triggered the workflow.
    /// </summary>
    [Required]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity that triggered the workflow.
    /// </summary>
    [Required]
    public int EntityId { get; set; }

    /// <summary>
    /// Type of trigger event.
    /// </summary>
    [Required]
    public WorkflowTriggerType TriggerType { get; set; }

    /// <summary>
    /// Event name for OnEvent triggers.
    /// </summary>
    public string? EventName { get; set; }

    /// <summary>
    /// Changed field name for OnFieldChange triggers.
    /// </summary>
    public string? ChangedField { get; set; }

    /// <summary>
    /// Old value before change.
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// New value after change.
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// User ID who initiated the action.
    /// </summary>
    public int? InitiatedById { get; set; }

    /// <summary>
    /// Additional context data as JSON.
    /// </summary>
    public string? ContextData { get; set; }
}

/// <summary>
/// DTO for workflow trigger execution result.
/// </summary>
public class TriggerExecutionResult
{
    /// <summary>
    /// Whether triggers were found and workflows were started.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of workflows triggered.
    /// </summary>
    public int WorkflowsTriggered { get; set; }

    /// <summary>
    /// IDs of workflow instances created.
    /// </summary>
    public List<int> WorkflowInstanceIds { get; set; } = new();

    /// <summary>
    /// Any errors that occurred during triggering.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Detailed results for each trigger.
    /// </summary>
    public List<TriggerResult> TriggerResults { get; set; } = new();
}

/// <summary>
/// Result of a single trigger evaluation.
/// </summary>
public class TriggerResult
{
    public int TriggerId { get; set; }
    public string TriggerName { get; set; } = string.Empty;
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public bool Matched { get; set; }
    public bool Executed { get; set; }
    public int? WorkflowInstanceId { get; set; }
    public string? SkippedReason { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO for trigger statistics.
/// </summary>
public class TriggerStatisticsDto
{
    public int TotalTriggers { get; set; }
    public int ActiveTriggers { get; set; }
    public int InactiveTriggers { get; set; }
    public int ScheduledTriggers { get; set; }
    public int RecordTriggers { get; set; }
    public int EventTriggers { get; set; }
    public int TotalExecutions { get; set; }
    public int ExecutionsToday { get; set; }
    public int ExecutionsThisWeek { get; set; }
    public DateTime? LastExecutionAt { get; set; }
    public Dictionary<WorkflowTriggerType, int> TriggersByType { get; set; } = new();
    public Dictionary<string, int> TriggersByEntityType { get; set; } = new();
}
