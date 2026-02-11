// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Core.DTOs.Workflow;

/// <summary>
/// Summary DTO for workflow instances in list views.
/// </summary>
public class WorkflowInstanceDto
{
    public int Id { get; set; }
    public string? CorrelationId { get; set; }
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Detailed DTO for a single workflow instance including related data.
/// </summary>
public class WorkflowInstanceDetailDto : WorkflowInstanceDto
{
    public int WorkflowVersionId { get; set; }
    public int VersionNumber { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public int? CurrentNodeId { get; set; }
    public string? CurrentNodeName { get; set; }
    public string? TriggerEvent { get; set; }
    public int? TriggeredById { get; set; }
    public string? TriggeredByName { get; set; }
    public string? InputData { get; set; }
    public string? StateData { get; set; }
    public string? OutputData { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? TimeoutAt { get; set; }
    public int Priority { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public string? ErrorStackTrace { get; set; }
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }
    public int? ParentInstanceId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<WorkflowNodeDto> Nodes { get; set; } = new();
    public List<WorkflowTransitionDto> Transitions { get; set; } = new();
    public List<WorkflowNodeInstanceDto> NodeInstances { get; set; } = new();
    public List<WorkflowTaskDto> Tasks { get; set; } = new();
    public List<WorkflowLogDto> RecentLogs { get; set; } = new();
}

/// <summary>
/// DTO for a workflow node execution instance.
/// </summary>
public class WorkflowNodeInstanceDto
{
    public int Id { get; set; }
    public int NodeId { get; set; }
    public string NodeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSkipped { get; set; }
    public string? SkipReason { get; set; }
    public int ExecutionSequence { get; set; }
    public string? WorkerId { get; set; }
}

/// <summary>
/// DTO for workflow tasks (human tasks and automated tasks).
/// </summary>
public class WorkflowTaskDto
{
    public int Id { get; set; }
    public int NodeId { get; set; }
    public string NodeName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime? DueAt { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToRole { get; set; }
    public int RetryCount { get; set; }
    public bool IsDeadLetter { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for workflow execution logs.
/// </summary>
public class WorkflowLogDto
{
    public int Id { get; set; }
    public int? WorkflowInstanceId { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? NodeName { get; set; }
    public int? NodeInstanceId { get; set; }
    public DateTime Timestamp { get; set; }
    public long? DurationMs { get; set; }
    public string? WorkerId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ExceptionType { get; set; }
}

/// <summary>
/// DTO for human tasks assigned to a user.
/// </summary>
public class HumanTaskDto
{
    public int Id { get; set; }
    public int WorkflowInstanceId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public int NodeId { get; set; }
    public string NodeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public DateTime? DueAt { get; set; }
    public string? FormSchema { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for starting a new workflow instance.
/// </summary>
public class StartWorkflowDto
{
    public int WorkflowDefinitionId { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? TriggerEvent { get; set; }
    public string? InputData { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

/// <summary>
/// Request DTO for cancelling a workflow instance.
/// </summary>
public class CancelInstanceDto
{
    public string? Reason { get; set; }
}

/// <summary>
/// Request DTO for skipping a workflow node.
/// </summary>
public class SkipNodeDto
{
    public string? Reason { get; set; }
}

/// <summary>
/// Request DTO for completing a human task.
/// </summary>
public class CompleteTaskDto
{
    public string? FormData { get; set; }
    public string? OutputData { get; set; }
}

/// <summary>
/// DTO for workflow audit log entries.
/// </summary>
public class WorkflowAuditLogDto
{
    public int Id { get; set; }
    public int? WorkflowInstanceId { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? NodeName { get; set; }
    public DateTime Timestamp { get; set; }
    public long? DurationMs { get; set; }
    public string? Data { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? WorkerId { get; set; }
    public string? ExceptionType { get; set; }
}

/// <summary>
/// DTO for the execution timeline of a workflow instance.
/// </summary>
public class ExecutionTimelineDto
{
    public int InstanceId { get; set; }
    public List<TimelineEntryDto> Entries { get; set; } = new();
}

/// <summary>
/// Individual entry in the execution timeline.
/// </summary>
public class TimelineEntryDto
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NodeType { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }
    public bool IsSkipped { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AssignedTo { get; set; }
    public int Sequence { get; set; }
}

/// <summary>
/// DTO for workflow node definitions (superset used by both WorkflowController and WorkflowInstanceController).
/// </summary>
public class WorkflowNodeDto
{
    public int Id { get; set; }
    public string NodeKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string NodeType { get; set; } = string.Empty;
    public string? NodeSubType { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public bool IsStartNode { get; set; }
    public bool IsEndNode { get; set; }
    public string? Configuration { get; set; }
    public int TimeoutMinutes { get; set; }
    public int RetryCount { get; set; }
    public int ExecutionOrder { get; set; }
}

/// <summary>
/// DTO for workflow transitions between nodes (superset used by both WorkflowController and WorkflowInstanceController).
/// </summary>
public class WorkflowTransitionDto
{
    public int Id { get; set; }
    public int SourceNodeId { get; set; }
    public int TargetNodeId { get; set; }
    public string? TransitionKey { get; set; }
    public string? Label { get; set; }
    public string ConditionType { get; set; } = string.Empty;
    public string? ConditionExpression { get; set; }
    public bool IsDefault { get; set; }
    public int Priority { get; set; }
    public string SourceHandle { get; set; } = string.Empty;
    public string TargetHandle { get; set; } = string.Empty;
    public string LineStyle { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string AnimationStyle { get; set; } = string.Empty;
}

/// <summary>
/// Comprehensive dashboard DTO for workflow execution monitoring.
/// </summary>
public class WorkflowDashboardDto
{
    /// <summary>
    /// Summary counts by status.
    /// </summary>
    public int TotalInstances { get; set; }
    public int ActiveInstances { get; set; }
    public int CompletedInstances { get; set; }
    public int FailedInstances { get; set; }
    public int CancelledInstances { get; set; }
    public int TimedOutInstances { get; set; }

    /// <summary>
    /// Rates.
    /// </summary>
    public double SuccessRate { get; set; }
    public double FailureRate { get; set; }

    /// <summary>
    /// Duration metrics (minutes).
    /// </summary>
    public double AvgDurationMinutes { get; set; }
    public double MedianDurationMinutes { get; set; }
    public double P95DurationMinutes { get; set; }

    /// <summary>
    /// Top failing workflows ranked by failure count.
    /// </summary>
    public List<TopFailingWorkflowDto> TopFailingWorkflows { get; set; } = new();

    /// <summary>
    /// Throughput trend: executions per day over the date range.
    /// </summary>
    public List<DailyThroughputDto> DailyThroughput { get; set; } = new();

    /// <summary>
    /// Most recent errors.
    /// </summary>
    public List<RecentErrorDto> RecentErrors { get; set; } = new();

    /// <summary>
    /// Per-workflow breakdown (top N by total executions).
    /// </summary>
    public List<WorkflowBreakdownDto> WorkflowBreakdown { get; set; } = new();
}

/// <summary>
/// A workflow ranked by failure count.
/// </summary>
public class TopFailingWorkflowDto
{
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public int FailureCount { get; set; }
    public int TotalExecutions { get; set; }
    public double FailureRate { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTime? LastFailureAt { get; set; }
}

/// <summary>
/// Daily execution throughput.
/// </summary>
public class DailyThroughputDto
{
    public DateTime Date { get; set; }
    public int Started { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
}

/// <summary>
/// A recent workflow failure.
/// </summary>
public class RecentErrorDto
{
    public int InstanceId { get; set; }
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? FailedAt { get; set; }
}

/// <summary>
/// Per-workflow execution breakdown.
/// </summary>
public class WorkflowBreakdownDto
{
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public int TotalExecutions { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int Running { get; set; }
    public double SuccessRate { get; set; }
    public double AvgDurationMinutes { get; set; }
}
