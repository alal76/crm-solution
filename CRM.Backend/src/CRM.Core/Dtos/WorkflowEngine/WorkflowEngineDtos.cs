namespace CRM.Core.Dtos.WorkflowEngine;

/// <summary>
/// DTO for workflow definition list/summary
/// </summary>
public class WorkflowDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string TriggerType { get; set; } = string.Empty;
    public string? TriggerEntityType { get; set; }
    public string? TriggerEvents { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByUserName { get; set; }
    public int StepCount { get; set; }
    public int ActiveInstanceCount { get; set; }
}

/// <summary>
/// DTO for detailed workflow definition with steps
/// </summary>
public class WorkflowDefinitionDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public string? TriggerEntityType { get; set; }
    public string? TriggerEvents { get; set; }
    public string? ScheduleCron { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string? ErrorHandlingConfig { get; set; }
    public string? NotificationConfig { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

/// <summary>
/// DTO for creating/updating workflow definition
/// </summary>
public class CreateWorkflowDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TriggerType { get; set; } = "Manual";
    public string? TriggerEntityType { get; set; }
    public string? TriggerEvents { get; set; }
    public string? ScheduleCron { get; set; }
    public int Priority { get; set; } = 100;
    public string? ErrorHandlingConfig { get; set; }
    public string? NotificationConfig { get; set; }
    public List<CreateWorkflowStepDto>? Steps { get; set; }
}

public class UpdateWorkflowDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TriggerType { get; set; } = "Manual";
    public string? TriggerEntityType { get; set; }
    public string? TriggerEvents { get; set; }
    public string? ScheduleCron { get; set; }
    public int Priority { get; set; } = 100;
    public string? ErrorHandlingConfig { get; set; }
    public string? NotificationConfig { get; set; }
    public List<CreateWorkflowStepDto>? Steps { get; set; }
}

/// <summary>
/// DTO for workflow step
/// </summary>
public class WorkflowStepDto
{
    public int Id { get; set; }
    public string StepKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StepType { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string? Configuration { get; set; }
    public string? Transitions { get; set; }
    public int TimeoutMinutes { get; set; }
    public string? RetryPolicy { get; set; }
    public bool IsStartStep { get; set; }
    public bool IsEndStep { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
}

public class CreateWorkflowStepDto
{
    public string StepKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StepType { get; set; } = "UserAction";
    public int OrderIndex { get; set; }
    public string? Configuration { get; set; }
    public string? Transitions { get; set; }
    public int TimeoutMinutes { get; set; } = 0;
    public string? RetryPolicy { get; set; }
    public bool IsStartStep { get; set; }
    public bool IsEndStep { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
}

/// <summary>
/// DTO for workflow instance
/// </summary>
public class WorkflowInstanceDto
{
    public int Id { get; set; }
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string WorkflowVersion { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? EntityReference { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CurrentStepKey { get; set; }
    public string? CurrentStepName { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int? StartedByUserId { get; set; }
    public string? StartedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public double? DurationMinutes { get; set; }
}

/// <summary>
/// DTO for detailed workflow instance with events and context
/// </summary>
public class WorkflowInstanceDetailDto : WorkflowInstanceDto
{
    public List<WorkflowEventDto> Events { get; set; } = new();
    public List<WorkflowTaskDto> Tasks { get; set; } = new();
    public Dictionary<string, object?> ContextVariables { get; set; } = new();
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

/// <summary>
/// DTO for starting a workflow
/// </summary>
public class StartWorkflowDto
{
    public int WorkflowDefinitionId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? EntityReference { get; set; }
    public string Priority { get; set; } = "Normal";
    public DateTime? DueAt { get; set; }
    public Dictionary<string, object?>? InitialContext { get; set; }
}

/// <summary>
/// DTO for workflow event
/// </summary>
public class WorkflowEventDto
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? StepKey { get; set; }
    public string? StepName { get; set; }
    public DateTime Timestamp { get; set; }
    public string ActorType { get; set; } = string.Empty;
    public string? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string? Message { get; set; }
    public string Severity { get; set; } = string.Empty;
    public long? DurationMs { get; set; }
    public string? ErrorDetails { get; set; }
}

/// <summary>
/// DTO for workflow task
/// </summary>
public class WorkflowTaskDto
{
    public int Id { get; set; }
    public int WorkflowInstanceId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string StepKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AssignmentType { get; set; } = string.Empty;
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public int? AssignedToGroupId { get; set; }
    public string? AssignedToGroupName { get; set; }
    public string? AssignedToRole { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueAt { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public int? ClaimedByUserId { get; set; }
    public string? ClaimedByUserName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ActionTaken { get; set; }
    public string? FormSchema { get; set; }
    public string? FormData { get; set; }
    public string? AvailableActions { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? EntityReference { get; set; }
}

/// <summary>
/// DTO for completing a task
/// </summary>
public class CompleteTaskDto
{
    public string ActionTaken { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public string? FormData { get; set; }
}

/// <summary>
/// DTO for reassigning a task
/// </summary>
public class ReassignTaskDto
{
    public string AssignmentType { get; set; } = "User";
    public int? AssignedToUserId { get; set; }
    public int? AssignedToGroupId { get; set; }
    public string? AssignedToRole { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for workflow analytics/metrics
/// </summary>
public class WorkflowAnalyticsDto
{
    public int ActiveWorkflows { get; set; }
    public int PendingTasks { get; set; }
    public int CompletedToday { get; set; }
    public int FailedOrStalled { get; set; }
    public double AverageCompletionTimeMinutes { get; set; }
    public List<BottleneckDto> Bottlenecks { get; set; } = new();
    public List<WorkflowEventDto> RecentActivity { get; set; } = new();
    public Dictionary<string, int> StatusDistribution { get; set; } = new();
}

public class BottleneckDto
{
    public string StepKey { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public double AverageAgeHours { get; set; }
}

/// <summary>
/// DTO for workflow definition validation result
/// </summary>
public class WorkflowValidationResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}

// Note: Step configuration DTOs (DelayStepConfig, ConditionalStepConfig, ApiCallStepConfig, etc.)
// are defined in CRM.Core.Entities.WorkflowEngine.WorkflowStep.cs
