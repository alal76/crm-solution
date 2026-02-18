// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

#pragma warning disable SA1011 // Closing square bracket should be followed by a space

using CRM.Core.DTOs.Workflow;
using CRM.Core.Entities.Workflow;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for managing workflow instances and execution
/// </summary>
public interface IWorkflowInstanceService
{
    #region Instance Operations

    /// <summary>
    /// Get workflow instances with filtering
    /// </summary>
    Task<List<WorkflowInstance>> GetInstancesAsync(
        int? workflowDefinitionId = null,
        string? entityType = null,
        int? entityId = null,
        WorkflowInstanceStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 50);

    /// <summary>
    /// Get a workflow instance by ID with full details
    /// </summary>
    Task<WorkflowInstance?> GetInstanceAsync(int instanceId);

    /// <summary>
    /// Get a workflow instance by correlation ID
    /// </summary>
    Task<WorkflowInstance?> GetInstanceByCorrelationIdAsync(string correlationId);

    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    Task<WorkflowInstance> StartWorkflowAsync(
        int workflowDefinitionId,
        string entityType,
        int entityId,
        string triggerEvent,
        int? triggeredById = null,
        object? inputData = null,
        DateTime? scheduledAt = null);

    /// <summary>
    /// Cancel a workflow instance
    /// </summary>
    Task<bool> CancelInstanceAsync(int instanceId, string reason, int? userId = null);

    /// <summary>
    /// Pause a workflow instance
    /// </summary>
    Task<bool> PauseInstanceAsync(int instanceId, int? userId = null);

    /// <summary>
    /// Resume a paused workflow instance
    /// </summary>
    Task<bool> ResumeInstanceAsync(int instanceId, int? userId = null);

    /// <summary>
    /// Retry a failed workflow instance
    /// </summary>
    Task<bool> RetryInstanceAsync(int instanceId, int? userId = null);

    /// <summary>
    /// Start workflow instances for multiple entities at once.
    /// Returns a result summarising successes and failures.
    /// </summary>
    Task<BulkStartResult> BulkStartWorkflowAsync(
        int workflowDefinitionId,
        string entityType,
        List<int> entityIds,
        string triggerEvent,
        int? triggeredById = null,
        object? inputData = null);

    #endregion

    #region Node Instance Operations

    /// <summary>
    /// Start execution of a node
    /// </summary>
    Task<WorkflowNodeInstance> StartNodeExecutionAsync(int instanceId, int nodeId, string? workerId = null);

    /// <summary>
    /// Complete execution of a node instance
    /// </summary>
    Task<WorkflowNodeInstance?> CompleteNodeExecutionAsync(
        int nodeInstanceId,
        object? outputData = null,
        int? userId = null);

    /// <summary>
    /// Mark a node instance as failed
    /// </summary>
    Task<WorkflowNodeInstance?> FailNodeExecutionAsync(
        int nodeInstanceId,
        string errorMessage,
        string? stackTrace = null);

    /// <summary>
    /// Skip a node in a workflow instance
    /// </summary>
    Task<bool> SkipNodeAsync(int instanceId, int nodeId, string reason, int? userId = null);

    #endregion

    #region Task Operations

    /// <summary>
    /// Create a task for a workflow node
    /// </summary>
    Task<WorkflowTask> CreateTaskForNodeAsync(int instanceId, WorkflowNode node, int? nodeInstanceId = null);

    /// <summary>
    /// Get pending tasks with filtering
    /// </summary>
    Task<List<WorkflowTask>> GetPendingTasksAsync(
        string queue,
        int maxTasks,
        string? workerId = null);

    /// <summary>
    /// Lock a task for processing
    /// </summary>
    Task<bool> LockTaskAsync(int taskId, string workerId, TimeSpan lockDuration);

    /// <summary>
    /// Complete a task
    /// </summary>
    Task<bool> CompleteTaskAsync(int taskId, object? outputData = null);

    /// <summary>
    /// Fail a task
    /// </summary>
    Task<bool> FailTaskAsync(int taskId, string errorMessage, string? stackTrace = null);

    /// <summary>
    /// Process tasks that are due for retry
    /// </summary>
    Task<int> ProcessRetryTasksAsync();

    /// <summary>
    /// Get human tasks for a specific user
    /// </summary>
    Task<List<WorkflowTask>> GetHumanTasksForUserAsync(int userId, string[]? roles = null);

    /// <summary>
    /// Claim a human task for a user
    /// </summary>
    Task<bool> ClaimTaskAsync(int taskId, int userId);

    /// <summary>
    /// Complete a human task with form data
    /// </summary>
    Task<bool> CompleteHumanTaskAsync(int taskId, int userId, string? formData, string? outputData);

    #endregion

    #region Logging

    /// <summary>
    /// Log a workflow event
    /// </summary>
    Task LogAsync(
        int instanceId,
        WorkflowLogLevel level,
        string category,
        string message,
        int? nodeId = null,
        int? nodeInstanceId = null,
        object? details = null,
        string? workerId = null,
        int? userId = null,
        long? durationMs = null);

    /// <summary>
    /// Get logs for a workflow instance
    /// </summary>
    Task<List<WorkflowLog>> GetLogsAsync(
        int instanceId,
        WorkflowLogLevel? minLevel = null,
        string? category = null,
        int skip = 0,
        int take = 100);

    #endregion

    #region Audit & Monitoring

    /// <summary>
    /// Get audit log for a workflow definition
    /// </summary>
    Task<(List<WorkflowLog> Logs, bool HasMore)> GetAuditLogAsync(
        int definitionId,
        string? eventType = null,
        string? eventCategory = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 100);

    /// <summary>
    /// Export audit log as CSV bytes
    /// </summary>
    Task<byte[]> ExportAuditLogCsvAsync(
        int definitionId,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Get execution timeline for a workflow instance
    /// </summary>
    Task<WorkflowInstance?> GetExecutionTimelineDataAsync(int instanceId);

    /// <summary>
    /// Get instance statistics with optional filtering
    /// </summary>
    Task<WorkflowInstanceStatistics> GetInstanceStatisticsAsync(
        int? workflowDefinitionId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Get comprehensive execution dashboard data
    /// </summary>
    Task<WorkflowDashboardDto> GetDashboardAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int topN = 10);

    #endregion

    #region Parallel Gateway & Sub-workflow

    /// <summary>
    /// Advance a workflow instance after a node completes —
    /// evaluates outgoing transitions and handles ParallelGateway / JoinGateway / Subprocess routing.
    /// Returns the list of newly-started node instances.
    /// </summary>
    Task<List<WorkflowNodeInstance>> AdvanceWorkflowAsync(int instanceId, int completedNodeInstanceId);

    /// <summary>
    /// Execute a ParallelGateway (fork) — starts all outgoing branches simultaneously.
    /// Returns node instances created for each branch.
    /// </summary>
    Task<List<WorkflowNodeInstance>> ExecuteParallelGatewayAsync(int instanceId, int gatewayNodeId, string? inputData = null);

    /// <summary>
    /// Check whether a JoinGateway can proceed — returns true when all incoming branches are complete.
    /// </summary>
    Task<bool> CheckJoinGatewayAsync(int instanceId, int joinNodeId);

    /// <summary>
    /// Start a child workflow for a Subprocess node.
    /// The Subprocess node's Configuration JSON must contain { "subWorkflowDefinitionId": int }.
    /// Returns the child WorkflowInstance.
    /// </summary>
    Task<WorkflowInstance> StartSubWorkflowAsync(int instanceId, int subprocessNodeId);

    /// <summary>
    /// Callback invoked when a child workflow completes — completes the parent's subprocess node
    /// and advances the parent workflow.
    /// </summary>
    Task OnChildWorkflowCompletedAsync(int childInstanceId);

    /// <summary>
    /// Get the status of all parallel branches for a workflow instance,
    /// optionally scoped to a specific gateway node.
    /// </summary>
    Task<ParallelBranchStatus> GetParallelBranchStatusAsync(int instanceId, int? gatewayNodeId = null);

    /// <summary>
    /// Get child workflow instances spawned by a parent instance.
    /// </summary>
    Task<List<WorkflowInstance>> GetChildInstancesAsync(int parentInstanceId);

    #endregion

    #region Wait/Timer & Timeout Processing

    /// <summary>
    /// Start a Wait/Timer node — sets it to Waiting status with a calculated resume time
    /// based on the node's Configuration JSON (delayMinutes, delayHours, waitUntil).
    /// </summary>
    Task<WorkflowNodeInstance> StartWaitNodeAsync(int instanceId, int waitNodeId);

    /// <summary>
    /// Process all waiting nodes whose resume time has arrived.
    /// Completes each due node and advances the workflow.
    /// Returns the count of nodes processed.
    /// </summary>
    Task<int> ProcessDueWaitNodesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Process all workflow instances and node instances that have exceeded their timeout.
    /// Marks timed-out instances as TimedOut and timed-out node instances as Failed.
    /// Returns the count of items processed.
    /// </summary>
    Task<int> ProcessTimedOutInstancesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all currently waiting node instances, optionally filtered by workflow instance.
    /// </summary>
    Task<List<WorkflowNodeInstance>> GetWaitingNodesAsync(int? instanceId = null);

    /// <summary>
    /// Manually resume a waiting node, skipping the remaining wait time.
    /// </summary>
    Task<WorkflowNodeInstance> ResumeWaitingNodeAsync(int nodeInstanceId);

    #endregion

}

/// <summary>
/// Workflow instance statistics model
/// </summary>
public class WorkflowInstanceStatistics
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Running { get; set; }
    public int Waiting { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int Cancelled { get; set; }
    public int TimedOut { get; set; }
    public double AverageCompletionTimeMinutes { get; set; }
    public List<WorkflowInstanceByWorkflow> ByWorkflow { get; set; } = new();
}

/// <summary>
/// Statistics grouped by workflow definition
/// </summary>
public class WorkflowInstanceByWorkflow
{
    public int WorkflowId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
}

/// <summary>
/// Status of parallel branches in a workflow instance
/// </summary>
public class ParallelBranchStatus
{
    public int InstanceId { get; set; }
    public int TotalBranches { get; set; }
    public int CompletedBranches { get; set; }
    public int RunningBranches { get; set; }
    public int FailedBranches { get; set; }
    public bool AllComplete => CompletedBranches >= TotalBranches && TotalBranches > 0;
    public List<BranchInfo> Branches { get; set; } = new();
}

/// <summary>
/// Information about a single parallel branch
/// </summary>
public class BranchInfo
{
    public int NodeInstanceId { get; set; }
    public int NodeId { get; set; }
    public string NodeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }
}

/// <summary>
/// Result of a bulk workflow start operation
/// </summary>
public class BulkStartResult
{
    public int TotalRequested { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public long ElapsedMs { get; set; }
    public List<BulkStartItemResult> Items { get; set; } = new();
}

/// <summary>
/// Result for a single entity in a bulk start
/// </summary>
public class BulkStartItemResult
{
    public int EntityId { get; set; }
    public bool Success { get; set; }
    public int? InstanceId { get; set; }
    public string? Error { get; set; }
}
