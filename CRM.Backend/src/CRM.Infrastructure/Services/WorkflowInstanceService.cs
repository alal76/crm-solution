// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.DTOs.Workflow;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing workflow instances and execution
/// </summary>
public class WorkflowInstanceService : IWorkflowInstanceService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<WorkflowInstanceService> _logger;
    private readonly IWorkflowService _workflowService;
    private readonly IHttpCalloutService _httpCalloutService;

    public WorkflowInstanceService(
        CrmDbContext context,
        ILogger<WorkflowInstanceService> logger,
        IWorkflowService workflowService,
        IHttpCalloutService httpCalloutService)
    {
        _context = context;
        _logger = logger;
        _workflowService = workflowService;
        _httpCalloutService = httpCalloutService;
    }

    #region Instance Operations

    /// <summary>
    /// Get workflow instances with filtering
    /// </summary>
    public async Task<List<WorkflowInstance>> GetInstancesAsync(
        int? workflowDefinitionId = null,
        string? entityType = null,
        int? entityId = null,
        WorkflowInstanceStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 50)
    {
        var query = _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.WorkflowVersion)
            .Include(i => i.CurrentNode)
            .Include(i => i.TriggeredBy)
            .Where(i => !i.IsDeleted);

        if (workflowDefinitionId.HasValue)
            query = query.Where(i => i.WorkflowDefinitionId == workflowDefinitionId.Value);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(i => i.EntityType == entityType);

        if (entityId.HasValue)
            query = query.Where(i => i.EntityId == entityId.Value);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(i => i.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.CreatedAt <= toDate.Value);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <summary>
    /// Get a specific instance with full details
    /// </summary>
    public async Task<WorkflowInstance?> GetInstanceAsync(int instanceId)
    {
        return await _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.WorkflowVersion)
                .ThenInclude(v => v.Nodes)
            .Include(i => i.WorkflowVersion)
                .ThenInclude(v => v.Transitions)
            .Include(i => i.CurrentNode)
            .Include(i => i.TriggeredBy)
            .Include(i => i.NodeInstances)
                .ThenInclude(ni => ni.WorkflowNode)
            .Include(i => i.Tasks)
            .Include(i => i.Logs.OrderByDescending(l => l.Timestamp).Take(100))
            .FirstOrDefaultAsync(i => i.Id == instanceId && !i.IsDeleted);
    }

    /// <summary>
    /// Get instance by correlation ID
    /// </summary>
    public async Task<WorkflowInstance?> GetInstanceByCorrelationIdAsync(string correlationId)
    {
        return await _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentNode)
            .FirstOrDefaultAsync(i => i.CorrelationId == correlationId && !i.IsDeleted);
    }

    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    public async Task<WorkflowInstance> StartWorkflowAsync(
        int workflowDefinitionId,
        string entityType,
        int entityId,
        string triggerEvent,
        int? triggeredById = null,
        object? inputData = null,
        DateTime? scheduledAt = null)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(workflowDefinitionId);
        if (workflow == null || workflow.Status != WorkflowStatus.Active)
            throw new InvalidOperationException("Workflow is not active");

        var version = await _workflowService.GetActiveVersionAsync(workflowDefinitionId);
        if (version == null)
            throw new InvalidOperationException("No active version found");

        // Check concurrent instance limit
        if (workflow.MaxConcurrentInstances > 0)
        {
            var runningCount = await _context.WorkflowInstances
                .CountAsync(i => i.WorkflowDefinitionId == workflowDefinitionId &&
                                 i.Status == WorkflowInstanceStatus.Running);

            if (runningCount >= workflow.MaxConcurrentInstances)
                throw new InvalidOperationException($"Maximum concurrent instances ({workflow.MaxConcurrentInstances}) reached");
        }

        var startNode = version.Nodes.FirstOrDefault(n => n.IsStartNode);
        if (startNode == null)
            throw new InvalidOperationException("No start node found in workflow");

        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = workflowDefinitionId,
            WorkflowVersionId = version.Id,
            EntityType = entityType,
            EntityId = entityId,
            Status = scheduledAt.HasValue ? WorkflowInstanceStatus.Pending : WorkflowInstanceStatus.Running,
            CurrentNodeId = startNode.Id,
            TriggerEvent = triggerEvent,
            TriggeredById = triggeredById,
            InputData = inputData != null ? JsonSerializer.Serialize(inputData) : null,
            StateData = "{}",
            ScheduledAt = scheduledAt,
            StartedAt = scheduledAt.HasValue ? null : DateTime.UtcNow,
            TimeoutAt = workflow.DefaultTimeoutHours > 0
                ? DateTime.UtcNow.AddHours(workflow.DefaultTimeoutHours)
                : null,
            Priority = workflow.Priority,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkflowInstances.Add(instance);
        await _context.SaveChangesAsync();

        // Create initial log entry
        await LogAsync(instance.Id, WorkflowLogLevel.Info, "Lifecycle",
            $"Workflow instance started: {workflow.Name}", startNode.Id);

        // Create first task if not scheduled
        if (!scheduledAt.HasValue)
        {
            await CreateTaskForNodeAsync(instance.Id, startNode);
        }

        _logger.LogInformation("Started workflow instance {InstanceId} for {EntityType}:{EntityId}",
            instance.Id, entityType, entityId);

        return instance;
    }

    /// <summary>
    /// Start workflow instances for multiple entities at once.
    /// Processes each entity sequentially; individual failures do not abort the batch.
    /// </summary>
    public async Task<BulkStartResult> BulkStartWorkflowAsync(
        int workflowDefinitionId,
        string entityType,
        List<int> entityIds,
        string triggerEvent,
        int? triggeredById = null,
        object? inputData = null)
    {
        if (entityIds == null || entityIds.Count == 0)
            throw new ArgumentException("At least one entity ID is required", nameof(entityIds));

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = new BulkStartResult { TotalRequested = entityIds.Count };

        _logger.LogInformation(
            "Bulk starting workflow {WorkflowId} for {Count} {EntityType} entities",
            workflowDefinitionId, entityIds.Count, entityType);

        foreach (var entityId in entityIds)
        {
            var item = new BulkStartItemResult { EntityId = entityId };
            try
            {
                var instance = await StartWorkflowAsync(
                    workflowDefinitionId, entityType, entityId,
                    triggerEvent, triggeredById, inputData);

                item.Success = true;
                item.InstanceId = instance.Id;
                result.Succeeded++;
            }
            catch (Exception ex)
            {
                item.Success = false;
                item.Error = ex.Message;
                result.Failed++;
                _logger.LogWarning(ex,
                    "Bulk start failed for {EntityType}:{EntityId}", entityType, entityId);
            }

            result.Items.Add(item);
        }

        sw.Stop();
        result.ElapsedMs = sw.ElapsedMilliseconds;

        _logger.LogInformation(
            "Bulk start complete: {Succeeded}/{Total} succeeded in {Elapsed}ms",
            result.Succeeded, result.TotalRequested, result.ElapsedMs);

        return result;
    }

    /// <summary>
    /// Cancel a workflow instance
    /// </summary>
    public async Task<bool> CancelInstanceAsync(int instanceId, string reason, int? userId = null)
    {
        var instance = await _context.WorkflowInstances.FindAsync(instanceId);
        if (instance == null)
            return false;

        if (instance.Status == WorkflowInstanceStatus.Completed ||
            instance.Status == WorkflowInstanceStatus.Cancelled)
            return false;

        instance.Status = WorkflowInstanceStatus.Cancelled;
        instance.IsCancelled = true;
        instance.CancellationReason = reason;
        instance.CompletedAt = DateTime.UtcNow;
        instance.UpdatedAt = DateTime.UtcNow;

        // Cancel pending tasks
        var pendingTasks = await _context.WorkflowTasks
            .Where(t => t.WorkflowInstanceId == instanceId &&
                       (t.Status == WorkflowTaskStatus.Pending || t.Status == WorkflowTaskStatus.Waiting))
            .ToListAsync();

        foreach (var task in pendingTasks)
        {
            task.Status = WorkflowTaskStatus.Cancelled;
            task.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await LogAsync(instanceId, WorkflowLogLevel.Warning, "Lifecycle",
            $"Workflow instance cancelled: {reason}", userId: userId);

        _logger.LogInformation("Cancelled workflow instance {InstanceId}: {Reason}", instanceId, reason);
        return true;
    }

    /// <summary>
    /// Pause a workflow instance
    /// </summary>
    public async Task<bool> PauseInstanceAsync(int instanceId, int? userId = null)
    {
        var instance = await _context.WorkflowInstances.FindAsync(instanceId);
        if (instance == null || instance.Status != WorkflowInstanceStatus.Running)
            return false;

        instance.Status = WorkflowInstanceStatus.Paused;
        instance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogAsync(instanceId, WorkflowLogLevel.Info, "Lifecycle", "Workflow instance paused", userId: userId);
        return true;
    }

    /// <summary>
    /// Resume a paused workflow instance
    /// </summary>
    public async Task<bool> ResumeInstanceAsync(int instanceId, int? userId = null)
    {
        var instance = await _context.WorkflowInstances.FindAsync(instanceId);
        if (instance == null || instance.Status != WorkflowInstanceStatus.Paused)
            return false;

        instance.Status = WorkflowInstanceStatus.Running;
        instance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogAsync(instanceId, WorkflowLogLevel.Info, "Lifecycle", "Workflow instance resumed", userId: userId);
        return true;
    }

    /// <summary>
    /// Retry a failed workflow instance
    /// </summary>
    public async Task<bool> RetryInstanceAsync(int instanceId, int? userId = null)
    {
        var instance = await _context.WorkflowInstances.FindAsync(instanceId);
        if (instance == null || instance.Status != WorkflowInstanceStatus.Failed)
            return false;

        instance.Status = WorkflowInstanceStatus.Running;
        instance.RetryCount++;
        instance.ErrorMessage = null;
        instance.ErrorStackTrace = null;
        instance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Re-queue the current node
        if (instance.CurrentNodeId.HasValue)
        {
            var node = await _context.WorkflowNodes.FindAsync(instance.CurrentNodeId.Value);
            if (node != null)
            {
                await CreateTaskForNodeAsync(instanceId, node);
            }
        }

        await LogAsync(instanceId, WorkflowLogLevel.Info, "Lifecycle",
            $"Workflow instance retry #{instance.RetryCount}", userId: userId);
        return true;
    }

    #endregion

    #region Node Instance Operations

    /// <summary>
    /// Record node execution start
    /// </summary>
    public async Task<WorkflowNodeInstance> StartNodeExecutionAsync(int instanceId, int nodeId, string? workerId = null)
    {
        var instance = await _context.WorkflowInstances.FindAsync(instanceId);
        if (instance == null)
            throw new ArgumentException("Instance not found");

        var sequence = await _context.WorkflowNodeInstances
            .Where(ni => ni.WorkflowInstanceId == instanceId)
            .CountAsync() + 1;

        var nodeInstance = new WorkflowNodeInstance
        {
            WorkflowInstanceId = instanceId,
            WorkflowNodeId = nodeId,
            Status = WorkflowNodeInstanceStatus.Running,
            StartedAt = DateTime.UtcNow,
            ExecutionSequence = sequence,
            WorkerId = workerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkflowNodeInstances.Add(nodeInstance);

        // Update current node on instance
        instance.CurrentNodeId = nodeId;
        instance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return nodeInstance;
    }

    /// <summary>
    /// Complete node execution
    /// </summary>
    public async Task<WorkflowNodeInstance?> CompleteNodeExecutionAsync(
        int nodeInstanceId,
        object? outputData = null,
        int? transitionTakenId = null)
    {
        var nodeInstance = await _context.WorkflowNodeInstances
            .Include(ni => ni.WorkflowInstance)
            .Include(ni => ni.WorkflowNode)
            .FirstOrDefaultAsync(ni => ni.Id == nodeInstanceId);

        if (nodeInstance == null)
            return null;

        nodeInstance.Status = WorkflowNodeInstanceStatus.Completed;
        nodeInstance.CompletedAt = DateTime.UtcNow;
        nodeInstance.DurationMs = (long)(DateTime.UtcNow - nodeInstance.StartedAt!.Value).TotalMilliseconds;
        nodeInstance.OutputData = outputData != null ? JsonSerializer.Serialize(outputData) : null;
        nodeInstance.TransitionTakenId = transitionTakenId;
        nodeInstance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await LogAsync(nodeInstance.WorkflowInstanceId, WorkflowLogLevel.Info, "Execution",
            $"Node completed: {nodeInstance.WorkflowNode.Name}", nodeInstance.WorkflowNodeId);

        return nodeInstance;
    }

    /// <summary>
    /// Fail node execution
    /// </summary>
    public async Task<WorkflowNodeInstance?> FailNodeExecutionAsync(
        int nodeInstanceId,
        string errorMessage,
        string? stackTrace = null)
    {
        var nodeInstance = await _context.WorkflowNodeInstances
            .Include(ni => ni.WorkflowInstance)
            .Include(ni => ni.WorkflowNode)
            .FirstOrDefaultAsync(ni => ni.Id == nodeInstanceId);

        if (nodeInstance == null)
            return null;

        nodeInstance.Status = WorkflowNodeInstanceStatus.Failed;
        nodeInstance.CompletedAt = DateTime.UtcNow;
        nodeInstance.DurationMs = (long)(DateTime.UtcNow - nodeInstance.StartedAt!.Value).TotalMilliseconds;
        nodeInstance.ErrorMessage = errorMessage;
        nodeInstance.ErrorStackTrace = stackTrace;
        nodeInstance.UpdatedAt = DateTime.UtcNow;

        // Check if retry is available
        var node = nodeInstance.WorkflowNode;
        if (nodeInstance.RetryCount < node.RetryCount)
        {
            nodeInstance.Status = WorkflowNodeInstanceStatus.Retrying;
            nodeInstance.RetryCount++;

            var delay = node.UseExponentialBackoff
                ? TimeSpan.FromSeconds(node.RetryDelaySeconds * Math.Pow(2, nodeInstance.RetryCount - 1))
                : TimeSpan.FromSeconds(node.RetryDelaySeconds);

            nodeInstance.NextRetryAt = DateTime.UtcNow.Add(delay);

            await LogAsync(nodeInstance.WorkflowInstanceId, WorkflowLogLevel.Warning, "Execution",
                $"Node failed, retrying in {delay.TotalSeconds}s: {errorMessage}", node.Id);
        }
        else
        {
            // Mark instance as failed
            var instance = nodeInstance.WorkflowInstance;
            instance.Status = WorkflowInstanceStatus.Failed;
            instance.ErrorMessage = errorMessage;
            instance.ErrorStackTrace = stackTrace;
            instance.CompletedAt = DateTime.UtcNow;
            instance.UpdatedAt = DateTime.UtcNow;

            await LogAsync(nodeInstance.WorkflowInstanceId, WorkflowLogLevel.Error, "Execution",
                $"Node failed permanently: {errorMessage}", node.Id);
        }

        await _context.SaveChangesAsync();
        return nodeInstance;
    }

    /// <summary>
    /// Skip a node
    /// </summary>
    public async Task<bool> SkipNodeAsync(int instanceId, int nodeId, string reason, int? userId = null)
    {
        var instance = await _context.WorkflowInstances.FindAsync(instanceId);
        if (instance == null)
            return false;

        var nodeInstance = await _context.WorkflowNodeInstances
            .FirstOrDefaultAsync(ni => ni.WorkflowInstanceId == instanceId &&
                                       ni.WorkflowNodeId == nodeId &&
                                       (ni.Status == WorkflowNodeInstanceStatus.Pending ||
                                        ni.Status == WorkflowNodeInstanceStatus.Waiting ||
                                        ni.Status == WorkflowNodeInstanceStatus.Running));

        if (nodeInstance != null)
        {
            nodeInstance.Status = WorkflowNodeInstanceStatus.Skipped;
            nodeInstance.IsSkipped = true;
            nodeInstance.SkipReason = reason;
            nodeInstance.CompletedAt = DateTime.UtcNow;
            nodeInstance.UpdatedAt = DateTime.UtcNow;
        }

        // Cancel related tasks
        var tasks = await _context.WorkflowTasks
            .Where(t => t.WorkflowInstanceId == instanceId &&
                       t.WorkflowNodeId == nodeId &&
                       (t.Status == WorkflowTaskStatus.Pending || t.Status == WorkflowTaskStatus.Waiting))
            .ToListAsync();

        foreach (var task in tasks)
        {
            task.Status = WorkflowTaskStatus.Skipped;
            task.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await LogAsync(instanceId, WorkflowLogLevel.Warning, "Execution",
            $"Node skipped: {reason}", nodeId, userId: userId);

        return true;
    }

    #endregion

    #region Task Operations

    /// <summary>
    /// Create a task for a workflow node
    /// </summary>
    public async Task<WorkflowTask> CreateTaskForNodeAsync(int instanceId, WorkflowNode node, int? nodeInstanceId = null)
    {
        var taskType = node.NodeType switch
        {
            WorkflowNodeType.HumanTask => WorkflowTaskType.Human,
            WorkflowNodeType.LLMAction => WorkflowTaskType.LLM,
            WorkflowNodeType.Wait => WorkflowTaskType.Timer,
            _ => WorkflowTaskType.Automated
        };

        var config = node.Configuration != null
            ? JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(node.Configuration)
            : null;

        var task = new WorkflowTask
        {
            WorkflowInstanceId = instanceId,
            WorkflowNodeId = node.Id,
            NodeInstanceId = nodeInstanceId,
            TaskType = taskType,
            Name = node.Name,
            Description = node.Description,
            Status = WorkflowTaskStatus.Pending,
            Priority = node.ExecutionOrder,
            QueueName = GetQueueForNodeType(node.NodeType),
            TimeoutAt = node.TimeoutMinutes > 0
                ? DateTime.UtcNow.AddMinutes(node.TimeoutMinutes)
                : null,
            MaxRetries = node.RetryCount,
            InputData = node.Configuration,
            FormSchema = config?.GetValueOrDefault("formSchema").ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkflowTasks.Add(task);
        await _context.SaveChangesAsync();

        return task;
    }

    /// <summary>
    /// Get queue name for node type
    /// </summary>
    private string GetQueueForNodeType(WorkflowNodeType nodeType)
    {
        return nodeType switch
        {
            WorkflowNodeType.LLMAction => "llm",
            WorkflowNodeType.HumanTask => "human",
            WorkflowNodeType.Action => "action",
            WorkflowNodeType.Wait => "timer",
            _ => "default"
        };
    }

    /// <summary>
    /// Get pending tasks for a worker
    /// </summary>
    public async Task<List<WorkflowTask>> GetPendingTasksAsync(
        string queueName = "default",
        int limit = 10,
        string? workerId = null)
    {
        var now = DateTime.UtcNow;

        return await _context.WorkflowTasks
            .Include(t => t.WorkflowInstance)
                .ThenInclude(i => i.WorkflowDefinition)
            .Include(t => t.WorkflowNode)
            .Where(t => !t.IsDeleted &&
                       t.Status == WorkflowTaskStatus.Pending &&
                       t.QueueName == queueName &&
                       (t.ScheduledAt == null || t.ScheduledAt <= now) &&
                       (t.LockExpiresAt == null || t.LockExpiresAt < now))
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Lock a task for processing
    /// </summary>
    public async Task<bool> LockTaskAsync(int taskId, string workerId, TimeSpan lockDuration)
    {
        var task = await _context.WorkflowTasks.FindAsync(taskId);
        if (task == null)
            return false;

        // Check if already locked
        if (task.Status == WorkflowTaskStatus.Locked &&
            task.LockExpiresAt > DateTime.UtcNow)
            return false;

        task.Status = WorkflowTaskStatus.Locked;
        task.LockedByWorkerId = workerId;
        task.PickedAt = DateTime.UtcNow;
        task.LockExpiresAt = DateTime.UtcNow.Add(lockDuration);
        task.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    /// <summary>
    /// Complete a task
    /// </summary>
    public async Task<bool> CompleteTaskAsync(int taskId, object? outputData = null)
    {
        var task = await _context.WorkflowTasks.FindAsync(taskId);
        if (task == null)
            return false;

        task.Status = WorkflowTaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        task.OutputData = outputData != null ? JsonSerializer.Serialize(outputData) : null;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Fail a task
    /// </summary>
    public async Task<bool> FailTaskAsync(int taskId, string errorMessage, string? stackTrace = null)
    {
        var task = await _context.WorkflowTasks
            .Include(t => t.WorkflowNode)
            .FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null)
            return false;

        task.ErrorMessage = errorMessage;
        task.ErrorStackTrace = stackTrace;
        task.RetryCount++;

        if (task.RetryCount >= task.MaxRetries)
        {
            // Move to dead letter queue
            task.Status = WorkflowTaskStatus.DeadLetter;
            task.IsDeadLetter = true;
            task.DeadLetterReason = $"Max retries ({task.MaxRetries}) exceeded";
            task.DeadLetterAt = DateTime.UtcNow;
        }
        else
        {
            // Schedule retry with exponential backoff
            task.Status = WorkflowTaskStatus.Retrying;
            var node = task.WorkflowNode;
            var delay = node.UseExponentialBackoff
                ? TimeSpan.FromSeconds(node.RetryDelaySeconds * Math.Pow(2, task.RetryCount - 1))
                : TimeSpan.FromSeconds(node.RetryDelaySeconds);

            task.NextRetryAt = DateTime.UtcNow.Add(delay);
            task.LockedByWorkerId = null;
            task.LockExpiresAt = null;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Move task back to pending for retry
    /// </summary>
    public async Task<int> ProcessRetryTasksAsync()
    {
        var now = DateTime.UtcNow;
        var retryTasks = await _context.WorkflowTasks
            .Where(t => t.Status == WorkflowTaskStatus.Retrying &&
                       t.NextRetryAt <= now)
            .ToListAsync();

        foreach (var task in retryTasks)
        {
            task.Status = WorkflowTaskStatus.Pending;
            task.NextRetryAt = null;
            task.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return retryTasks.Count;
    }

    /// <summary>
    /// Get human tasks for a user
    /// </summary>
    public async Task<List<WorkflowTask>> GetHumanTasksForUserAsync(int userId, string[]? roles = null)
    {
        var query = _context.WorkflowTasks
            .Include(t => t.WorkflowInstance)
                .ThenInclude(i => i.WorkflowDefinition)
            .Include(t => t.WorkflowNode)
            .Where(t => !t.IsDeleted &&
                       t.TaskType == WorkflowTaskType.Human &&
                       (t.Status == WorkflowTaskStatus.Pending || t.Status == WorkflowTaskStatus.Waiting));

        // Filter by assignment
        if (roles != null && roles.Length > 0)
        {
            // Convert to List<string> to avoid ReadOnlySpan<string> EF Core translation error with string[].Contains
            var rolesList = roles.ToList();
            query = query.Where(t => t.AssignedToId == userId ||
                                    (t.AssignedToRole != null && rolesList.Contains(t.AssignedToRole)));
        }
        else
        {
            query = query.Where(t => t.AssignedToId == userId);
        }

        return await query
            .OrderBy(t => t.DueAt)
            .ThenBy(t => t.Priority)
            .ToListAsync();
    }

    #endregion

    #region Logging

    /// <summary>
    /// Add a log entry
    /// </summary>
    public async Task LogAsync(
        int instanceId,
        WorkflowLogLevel level,
        string category,
        string message,
        int? nodeId = null,
        int? nodeInstanceId = null,
        object? details = null,
        string? workerId = null,
        int? userId = null,
        long? durationMs = null)
    {
        var log = new WorkflowLog
        {
            WorkflowInstanceId = instanceId,
            WorkflowNodeId = nodeId,
            NodeInstanceId = nodeInstanceId,
            Level = level,
            Category = category,
            Message = message,
            Details = details != null ? JsonSerializer.Serialize(details) : null,
            WorkerId = workerId,
            UserId = userId,
            DurationMs = durationMs,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkflowLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Get logs for an instance
    /// </summary>
    public async Task<List<WorkflowLog>> GetLogsAsync(
        int instanceId,
        WorkflowLogLevel? minLevel = null,
        string? category = null,
        int skip = 0,
        int take = 100)
    {
        var query = _context.WorkflowLogs
            .Include(l => l.WorkflowNode)
            .Include(l => l.User)
            .Where(l => l.WorkflowInstanceId == instanceId);

        if (minLevel.HasValue)
            query = query.Where(l => l.Level >= minLevel.Value);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(l => l.Category == category);

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    #endregion


    #region Claim & Complete Human Tasks

    /// <summary>
    /// Claim a human task for a user
    /// </summary>
    public async Task<bool> ClaimTaskAsync(int taskId, int userId)
    {
        var task = await _context.WorkflowTasks.FindAsync(taskId);
        if (task == null)
            return false;
        if (task.AssignedToId != null)
            return false;

        task.AssignedToId = userId;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} claimed by user {UserId}", taskId, userId);
        return true;
    }

    /// <summary>
    /// Complete a human task with form data
    /// </summary>
    public async Task<bool> CompleteHumanTaskAsync(int taskId, int userId, string? formData, string? outputData)
    {
        var task = await _context.WorkflowTasks.FindAsync(taskId);
        if (task == null)
            return false;
        if (task.AssignedToId != userId)
            return false;

        task.FormData = formData;
        task.OutputData = outputData;
        task.Status = WorkflowTaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Log the completion
        await LogAsync(
            task.WorkflowInstanceId,
            WorkflowLogLevel.Info,
            "HumanTask",
            "Task completed by user",
            task.WorkflowNodeId,
            userId: userId);

        _logger.LogInformation("Task {TaskId} completed by user {UserId}", taskId, userId);
        return true;
    }

    #endregion

    #region Audit & Monitoring

    /// <summary>
    /// Get audit log for a workflow definition
    /// </summary>
    public async Task<(List<WorkflowLog> Logs, bool HasMore)> GetAuditLogAsync(
        int definitionId,
        string? eventType = null,
        string? eventCategory = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 100)
    {
        var query = _context.WorkflowLogs
            .Include(l => l.WorkflowNode)
            .Include(l => l.User)
            .Where(l => l.WorkflowInstance != null &&
                       l.WorkflowInstance.WorkflowDefinitionId == definitionId)
            .OrderByDescending(l => l.Timestamp)
            .AsQueryable();

        if (!string.IsNullOrEmpty(eventCategory))
            query = query.Where(l => l.Category == eventCategory);

        if (fromDate.HasValue)
            query = query.Where(l => l.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.Timestamp <= toDate.Value);

        var logs = await query.Skip(skip).Take(take).ToListAsync();
        return (logs, logs.Count == take);
    }

    /// <summary>
    /// Export audit log as CSV bytes
    /// </summary>
    public async Task<byte[]> ExportAuditLogCsvAsync(
        int definitionId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.WorkflowLogs
            .Include(l => l.WorkflowNode)
            .Include(l => l.User)
            .Where(l => l.WorkflowInstance != null &&
                       l.WorkflowInstance.WorkflowDefinitionId == definitionId)
            .OrderByDescending(l => l.Timestamp)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(l => l.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.Timestamp <= toDate.Value);

        var logs = await query.Take(10000).ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Timestamp,Event Type,Level,Message,Actor,Node,Worker ID,Duration (ms)");

        foreach (var log in logs)
        {
            var actor = log.User != null ? $"{log.User.FirstName} {log.User.LastName}" : "";
            var node = log.WorkflowNode?.Name ?? "";
            var message = (log.Message ?? "").Replace("\"", "\"\"");

            csv.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.Category}\",\"{log.Level}\",\"{message}\",\"{actor}\",\"{node}\",\"{log.WorkerId ?? ""}\",{log.DurationMs ?? 0}");
        }

        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    /// <summary>
    /// Get execution timeline data for a workflow instance
    /// </summary>
    public async Task<WorkflowInstance?> GetExecutionTimelineDataAsync(int instanceId)
    {
        return await _context.WorkflowInstances
            .Include(i => i.NodeInstances)
            .ThenInclude(ni => ni.WorkflowNode)
            .Include(i => i.Tasks)
            .ThenInclude(t => t.WorkflowNode)
            .FirstOrDefaultAsync(i => i.Id == instanceId);
    }

    /// <summary>
    /// Get instance statistics with optional filtering
    /// </summary>
    public async Task<WorkflowInstanceStatistics> GetInstanceStatisticsAsync(
        int? workflowDefinitionId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.WorkflowInstances.Where(i => !i.IsDeleted);

        if (workflowDefinitionId.HasValue)
            query = query.Where(i => i.WorkflowDefinitionId == workflowDefinitionId.Value);

        if (fromDate.HasValue)
            query = query.Where(i => i.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.CreatedAt <= toDate.Value);

        var stats = new WorkflowInstanceStatistics
        {
            Total = await query.CountAsync(),
            Pending = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Pending),
            Running = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Running),
            Waiting = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Waiting),
            Completed = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Completed),
            Failed = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Failed),
            Cancelled = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Cancelled),
            TimedOut = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.TimedOut),
            AverageCompletionTimeMinutes = await query
                .Where(i => i.Status == WorkflowInstanceStatus.Completed &&
                           i.StartedAt != null && i.CompletedAt != null)
                .Select(i => (double)((i.CompletedAt!.Value - i.StartedAt!.Value).TotalMinutes))
                .DefaultIfEmpty(0)
                .AverageAsync(),
            ByWorkflow = await query
                .GroupBy(i => new { i.WorkflowDefinitionId, i.WorkflowDefinition.Name })
                .Select(g => new WorkflowInstanceByWorkflow
                {
                    WorkflowId = g.Key.WorkflowDefinitionId,
                    WorkflowName = g.Key.Name,
                    Total = g.Count(),
                    Completed = g.Count(i => i.Status == WorkflowInstanceStatus.Completed),
                    Failed = g.Count(i => i.Status == WorkflowInstanceStatus.Failed)
                })
                .ToListAsync()
        };

        return stats;
    }

    /// <summary>
    /// Get comprehensive execution dashboard data
    /// </summary>
    public async Task<WorkflowDashboardDto> GetDashboardAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int topN = 10)
    {
        var effectiveFrom = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var effectiveTo = toDate ?? DateTime.UtcNow;

        var query = _context.WorkflowInstances
            .Where(i => !i.IsDeleted && i.CreatedAt >= effectiveFrom && i.CreatedAt <= effectiveTo);

        // --- Summary counts ---
        var total = await query.CountAsync();
        var active = await query.CountAsync(i =>
            i.Status == WorkflowInstanceStatus.Running ||
            i.Status == WorkflowInstanceStatus.Pending ||
            i.Status == WorkflowInstanceStatus.Waiting);
        var completed = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Completed);
        var failed = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Failed);
        var cancelled = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Cancelled);
        var timedOut = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.TimedOut);

        var terminalCount = completed + failed + cancelled + timedOut;
        var successRate = terminalCount > 0 ? Math.Round((double)completed / terminalCount * 100, 1) : 0;
        var failureRate = terminalCount > 0 ? Math.Round((double)failed / terminalCount * 100, 1) : 0;

        // --- Duration metrics ---
        var durations = await query
            .Where(i => i.Status == WorkflowInstanceStatus.Completed &&
                        i.StartedAt != null && i.CompletedAt != null)
            .Select(i => (i.CompletedAt!.Value - i.StartedAt!.Value).TotalMinutes)
            .OrderBy(d => d)
            .ToListAsync();

        double avgDuration = 0, medianDuration = 0, p95Duration = 0;
        if (durations.Count > 0)
        {
            avgDuration = Math.Round(durations.Average(), 2);
            medianDuration = Math.Round(durations[durations.Count / 2], 2);
            var p95Index = (int)Math.Ceiling(durations.Count * 0.95) - 1;
            p95Duration = Math.Round(durations[Math.Max(0, p95Index)], 2);
        }

        // --- Top failing workflows ---
        var topFailing = await query
            .Where(i => i.Status == WorkflowInstanceStatus.Failed)
            .GroupBy(i => new { i.WorkflowDefinitionId, i.WorkflowDefinition.Name })
            .Select(g => new
            {
                g.Key.WorkflowDefinitionId,
                WorkflowName = g.Key.Name,
                FailureCount = g.Count(),
                LastErrorMessage = g.OrderByDescending(i => i.CompletedAt ?? i.UpdatedAt).Select(i => i.ErrorMessage).FirstOrDefault(),
                LastFailureAt = g.Max(i => i.CompletedAt ?? i.UpdatedAt)
            })
            .OrderByDescending(x => x.FailureCount)
            .Take(topN)
            .ToListAsync();

        // Enrich with total executions to compute per-workflow failure rate
        var failingWorkflowIds = topFailing.Select(f => f.WorkflowDefinitionId).ToList();
        var totalsByWorkflow = await query
            .Where(i => failingWorkflowIds.Contains(i.WorkflowDefinitionId))
            .GroupBy(i => i.WorkflowDefinitionId)
            .Select(g => new { WorkflowId = g.Key, TotalCount = g.Count() })
            .ToListAsync();

        var totalsDict = totalsByWorkflow.ToDictionary(t => t.WorkflowId, t => t.TotalCount);

        var topFailingDtos = topFailing.Select(f =>
        {
            var totalExec = totalsDict.GetValueOrDefault(f.WorkflowDefinitionId, f.FailureCount);
            return new TopFailingWorkflowDto
            {
                WorkflowDefinitionId = f.WorkflowDefinitionId,
                WorkflowName = f.WorkflowName,
                FailureCount = f.FailureCount,
                TotalExecutions = totalExec,
                FailureRate = totalExec > 0 ? Math.Round((double)f.FailureCount / totalExec * 100, 1) : 0,
                LastErrorMessage = f.LastErrorMessage,
                LastFailureAt = f.LastFailureAt
            };
        }).ToList();

        // --- Daily throughput ---
        var dailyThroughput = await query
            .GroupBy(i => i.CreatedAt.Date)
            .Select(g => new DailyThroughputDto
            {
                Date = g.Key,
                Started = g.Count(),
                Completed = g.Count(i => i.Status == WorkflowInstanceStatus.Completed),
                Failed = g.Count(i => i.Status == WorkflowInstanceStatus.Failed)
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        // --- Recent errors ---
        var recentErrors = await query
            .Where(i => i.Status == WorkflowInstanceStatus.Failed)
            .OrderByDescending(i => i.CompletedAt ?? i.UpdatedAt)
            .Take(topN)
            .Select(i => new RecentErrorDto
            {
                InstanceId = i.Id,
                WorkflowDefinitionId = i.WorkflowDefinitionId,
                WorkflowName = i.WorkflowDefinition.Name,
                EntityType = i.EntityType,
                EntityId = i.EntityId,
                ErrorMessage = i.ErrorMessage,
                FailedAt = i.CompletedAt ?? i.UpdatedAt
            })
            .ToListAsync();

        // --- Per-workflow breakdown (top N by volume) ---
        var breakdown = await query
            .GroupBy(i => new { i.WorkflowDefinitionId, i.WorkflowDefinition.Name })
            .Select(g => new
            {
                g.Key.WorkflowDefinitionId,
                WorkflowName = g.Key.Name,
                TotalExecutions = g.Count(),
                CompletedCount = g.Count(i => i.Status == WorkflowInstanceStatus.Completed),
                FailedCount = g.Count(i => i.Status == WorkflowInstanceStatus.Failed),
                RunningCount = g.Count(i => i.Status == WorkflowInstanceStatus.Running || i.Status == WorkflowInstanceStatus.Pending),
                AvgDuration = g
                    .Where(i => i.Status == WorkflowInstanceStatus.Completed && i.StartedAt != null && i.CompletedAt != null)
                    .Select(i => (i.CompletedAt!.Value - i.StartedAt!.Value).TotalMinutes)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .OrderByDescending(x => x.TotalExecutions)
            .Take(topN)
            .ToListAsync();

        var workflowBreakdown = breakdown.Select(b => new WorkflowBreakdownDto
        {
            WorkflowDefinitionId = b.WorkflowDefinitionId,
            WorkflowName = b.WorkflowName,
            TotalExecutions = b.TotalExecutions,
            Completed = b.CompletedCount,
            Failed = b.FailedCount,
            Running = b.RunningCount,
            SuccessRate = b.TotalExecutions > 0 ? Math.Round((double)b.CompletedCount / b.TotalExecutions * 100, 1) : 0,
            AvgDurationMinutes = Math.Round(b.AvgDuration, 2)
        }).ToList();

        return new WorkflowDashboardDto
        {
            TotalInstances = total,
            ActiveInstances = active,
            CompletedInstances = completed,
            FailedInstances = failed,
            CancelledInstances = cancelled,
            TimedOutInstances = timedOut,
            SuccessRate = successRate,
            FailureRate = failureRate,
            AvgDurationMinutes = avgDuration,
            MedianDurationMinutes = medianDuration,
            P95DurationMinutes = p95Duration,
            TopFailingWorkflows = topFailingDtos,
            DailyThroughput = dailyThroughput,
            RecentErrors = recentErrors,
            WorkflowBreakdown = workflowBreakdown
        };
    }

    #endregion

    #region Parallel Gateway & Sub-workflow

    /// <summary>
    /// Advance a workflow after a node completes — routes to next node(s),
    /// handling ParallelGateway fork, JoinGateway sync, and Subprocess invocation.
    /// </summary>
    public async Task<List<WorkflowNodeInstance>> AdvanceWorkflowAsync(int instanceId, int completedNodeInstanceId)
    {
        var nodeInstance = await _context.WorkflowNodeInstances
            .Include(ni => ni.WorkflowNode)
                .ThenInclude(n => n.OutgoingTransitions)
            .FirstOrDefaultAsync(ni => ni.Id == completedNodeInstanceId);

        if (nodeInstance == null)
            throw new ArgumentException($"Node instance {completedNodeInstanceId} not found");

        var instance = await _context.WorkflowInstances
            .Include(i => i.WorkflowVersion)
                .ThenInclude(v => v.Nodes)
            .Include(i => i.WorkflowVersion)
                .ThenInclude(v => v.Transitions)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null)
            throw new ArgumentException($"Workflow instance {instanceId} not found");

        var currentNode = nodeInstance.WorkflowNode;
        var startedInstances = new List<WorkflowNodeInstance>();

        // If the completed node is an End node, mark instance complete
        if (currentNode.IsEndNode || currentNode.NodeType == WorkflowNodeType.End)
        {
            instance.Status = WorkflowInstanceStatus.Completed;
            instance.CompletedAt = DateTime.UtcNow;
            instance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // If this is a child instance, notify parent
            if (instance.ParentInstanceId.HasValue)
                await OnChildWorkflowCompletedAsync(instance.Id);

            await LogAsync(instanceId, WorkflowLogLevel.Info, "Execution", "Workflow completed");
            return startedInstances;
        }

        // Get outgoing transitions from the current node, ordered by priority
        var outgoing = instance.WorkflowVersion.Transitions
            .Where(t => t.SourceNodeId == currentNode.Id && !t.IsDeleted)
            .OrderBy(t => t.Priority)
            .ToList();

        if (!outgoing.Any())
        {
            await LogAsync(instanceId, WorkflowLogLevel.Warning, "Execution",
                $"No outgoing transitions from node {currentNode.Name}");
            return startedInstances;
        }

        // Resolve target nodes
        var targetNodeIds = outgoing.Select(t => t.TargetNodeId).Distinct().ToList();
        var targetNodes = instance.WorkflowVersion.Nodes
            .Where(n => targetNodeIds.Contains(n.Id) && !n.IsDeleted)
            .ToList();

        foreach (var targetNode in targetNodes)
        {
            switch (targetNode.NodeType)
            {
                case WorkflowNodeType.ParallelGateway:
                    var branchInstances = await ExecuteParallelGatewayAsync(instanceId, targetNode.Id, nodeInstance.OutputData);
                    startedInstances.AddRange(branchInstances);
                    break;

                case WorkflowNodeType.JoinGateway:
                    var canProceed = await CheckJoinGatewayAsync(instanceId, targetNode.Id);
                    if (canProceed)
                    {
                        // Complete the join and advance past it
                        var joinNi = await StartNodeExecutionAsync(instanceId, targetNode.Id);
                        joinNi = (await CompleteNodeExecutionAsync(joinNi.Id))!;
                        var afterJoin = await AdvanceWorkflowAsync(instanceId, joinNi.Id);
                        startedInstances.Add(joinNi);
                        startedInstances.AddRange(afterJoin);
                    }
                    break;

                case WorkflowNodeType.Subprocess:
                    var subNi = await StartNodeExecutionAsync(instanceId, targetNode.Id);
                    await StartSubWorkflowAsync(instanceId, targetNode.Id);
                    startedInstances.Add(subNi);
                    break;

                case WorkflowNodeType.Action:
                    // Action node — check for HTTP callout configuration
                    var actionNi = await StartNodeExecutionAsync(instanceId, targetNode.Id);
                    startedInstances.Add(actionNi);
                    await ExecuteHttpCalloutNodeAsync(actionNi.Id, targetNode);
                    break;

                case WorkflowNodeType.Wait:
                    // Wait/Timer node — start in Waiting status with a scheduled resume time
                    var waitNi = await StartWaitNodeAsync(instanceId, targetNode.Id);
                    startedInstances.Add(waitNi);
                    break;

                default:
                    // Normal node — start execution
                    var ni = await StartNodeExecutionAsync(instanceId, targetNode.Id);
                    startedInstances.Add(ni);
                    break;
            }
        }

        return startedInstances;
    }

    /// <summary>
    /// Execute a ParallelGateway — forks into all outgoing branches simultaneously.
    /// </summary>
    public async Task<List<WorkflowNodeInstance>> ExecuteParallelGatewayAsync(
        int instanceId, int gatewayNodeId, string? inputData = null)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.WorkflowVersion)
                .ThenInclude(v => v.Nodes)
            .Include(i => i.WorkflowVersion)
                .ThenInclude(v => v.Transitions)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null)
            throw new ArgumentException($"Workflow instance {instanceId} not found");

        var gatewayNode = instance.WorkflowVersion.Nodes
            .FirstOrDefault(n => n.Id == gatewayNodeId && !n.IsDeleted);

        if (gatewayNode == null || gatewayNode.NodeType != WorkflowNodeType.ParallelGateway)
            throw new ArgumentException($"Node {gatewayNodeId} is not a ParallelGateway");

        // Record the gateway node as completed (it's a pass-through)
        var gatewayNi = await StartNodeExecutionAsync(instanceId, gatewayNodeId);
        gatewayNi = (await CompleteNodeExecutionAsync(gatewayNi.Id))!;

        // Get all outgoing transitions — each leads to a parallel branch
        var outgoing = instance.WorkflowVersion.Transitions
            .Where(t => t.SourceNodeId == gatewayNodeId && !t.IsDeleted)
            .OrderBy(t => t.Priority)
            .ToList();

        var branchInstances = new List<WorkflowNodeInstance>();

        foreach (var transition in outgoing)
        {
            var targetNode = instance.WorkflowVersion.Nodes
                .FirstOrDefault(n => n.Id == transition.TargetNodeId && !n.IsDeleted);

            if (targetNode == null)
                continue;

            var ni = await StartNodeExecutionAsync(instanceId, targetNode.Id);
            ni.InputData = inputData;
            await _context.SaveChangesAsync();

            branchInstances.Add(ni);

            await LogAsync(instanceId, WorkflowLogLevel.Info, "ParallelGateway",
                $"Started parallel branch: {targetNode.Name}", targetNode.Id, ni.Id);
        }

        await LogAsync(instanceId, WorkflowLogLevel.Info, "ParallelGateway",
            $"Forked into {branchInstances.Count} parallel branches from gateway {gatewayNode.Name}",
            gatewayNodeId);

        return branchInstances;
    }

    /// <summary>
    /// Check if all incoming branches to a JoinGateway have completed.
    /// </summary>
    public async Task<bool> CheckJoinGatewayAsync(int instanceId, int joinNodeId)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.WorkflowVersion)
                .ThenInclude(v => v.Transitions)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null)
            throw new ArgumentException($"Workflow instance {instanceId} not found");

        // Get all incoming transitions to the join node
        var incomingTransitions = instance.WorkflowVersion.Transitions
            .Where(t => t.TargetNodeId == joinNodeId && !t.IsDeleted)
            .ToList();

        if (!incomingTransitions.Any())
            return true; // No incoming = can proceed

        // Check each source node — it must have a Completed node instance
        var sourceNodeIds = incomingTransitions.Select(t => t.SourceNodeId).Distinct().ToList();

        var completedSourceNodes = await _context.WorkflowNodeInstances
            .Where(ni => ni.WorkflowInstanceId == instanceId
                      && sourceNodeIds.Contains(ni.WorkflowNodeId)
                      && ni.Status == WorkflowNodeInstanceStatus.Completed)
            .Select(ni => ni.WorkflowNodeId)
            .Distinct()
            .ToListAsync();

        var allComplete = sourceNodeIds.All(sid => completedSourceNodes.Contains(sid));

        await LogAsync(instanceId, WorkflowLogLevel.Debug, "JoinGateway",
            $"Join check: {completedSourceNodes.Count}/{sourceNodeIds.Count} branches complete",
            joinNodeId);

        return allComplete;
    }

    /// <summary>
    /// Start a child workflow for a Subprocess node.
    /// The node Configuration JSON should contain { "subWorkflowDefinitionId": int }.
    /// </summary>
    public async Task<WorkflowInstance> StartSubWorkflowAsync(int instanceId, int subprocessNodeId)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.WorkflowVersion)
                .ThenInclude(v => v.Nodes)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null)
            throw new ArgumentException($"Workflow instance {instanceId} not found");

        var subprocessNode = instance.WorkflowVersion.Nodes
            .FirstOrDefault(n => n.Id == subprocessNodeId && !n.IsDeleted);

        if (subprocessNode == null || subprocessNode.NodeType != WorkflowNodeType.Subprocess)
            throw new ArgumentException($"Node {subprocessNodeId} is not a Subprocess node");

        // Parse configuration to get the target workflow definition ID
        int subWorkflowDefinitionId;
        if (string.IsNullOrWhiteSpace(subprocessNode.Configuration))
            throw new InvalidOperationException($"Subprocess node {subprocessNode.Name} has no configuration");

        try
        {
            using var doc = JsonDocument.Parse(subprocessNode.Configuration);
            if (!doc.RootElement.TryGetProperty("subWorkflowDefinitionId", out var prop))
                throw new InvalidOperationException("Missing subWorkflowDefinitionId in configuration");
            subWorkflowDefinitionId = prop.GetInt32();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid configuration JSON on subprocess node: {ex.Message}");
        }

        // Set the subprocess node instance to Waiting
        var nodeInst = await _context.WorkflowNodeInstances
            .Where(ni => ni.WorkflowInstanceId == instanceId
                      && ni.WorkflowNodeId == subprocessNodeId
                      && ni.Status == WorkflowNodeInstanceStatus.Running)
            .OrderByDescending(ni => ni.Id)
            .FirstOrDefaultAsync();

        if (nodeInst != null)
        {
            nodeInst.Status = WorkflowNodeInstanceStatus.Waiting;
            nodeInst.UpdatedAt = DateTime.UtcNow;
        }

        // Start the child workflow instance
        var childInstance = await StartWorkflowAsync(
            workflowDefinitionId: subWorkflowDefinitionId,
            entityType: instance.EntityType,
            entityId: instance.EntityId,
            triggerEvent: "SubprocessStart",
            triggeredById: instance.TriggeredById,
            inputData: nodeInst?.InputData != null ? JsonSerializer.Deserialize<object>(nodeInst.InputData) : null);

        // Link parent ↔ child
        childInstance.ParentInstanceId = instanceId;
        await _context.SaveChangesAsync();

        await LogAsync(instanceId, WorkflowLogLevel.Info, "Subprocess",
            $"Started child workflow #{childInstance.Id} (definition #{subWorkflowDefinitionId}) from node {subprocessNode.Name}",
            subprocessNodeId);

        return childInstance;
    }

    /// <summary>
    /// Callback when a child workflow completes — completes the parent's subprocess node and advances.
    /// </summary>
    public async Task OnChildWorkflowCompletedAsync(int childInstanceId)
    {
        var child = await _context.WorkflowInstances
            .FirstOrDefaultAsync(i => i.Id == childInstanceId);

        if (child?.ParentInstanceId == null)
            return;

        var parentInstanceId = child.ParentInstanceId.Value;

        // Find the Waiting subprocess node instance in the parent
        var parentNodeInstance = await _context.WorkflowNodeInstances
            .Include(ni => ni.WorkflowNode)
            .Where(ni => ni.WorkflowInstanceId == parentInstanceId
                      && ni.WorkflowNode.NodeType == WorkflowNodeType.Subprocess
                      && ni.Status == WorkflowNodeInstanceStatus.Waiting)
            .OrderByDescending(ni => ni.Id)
            .FirstOrDefaultAsync();

        if (parentNodeInstance == null)
        {
            _logger.LogWarning("No waiting subprocess node found in parent instance {ParentId} for child {ChildId}",
                parentInstanceId, childInstanceId);
            return;
        }

        // Complete the subprocess node with child's output
        parentNodeInstance.OutputData = child.OutputData;
        var childOutputData = child.OutputData != null
            ? JsonSerializer.Deserialize<object>(child.OutputData) : null;
        await CompleteNodeExecutionAsync(parentNodeInstance.Id, outputData: childOutputData);

        await LogAsync(parentInstanceId, WorkflowLogLevel.Info, "Subprocess",
            $"Child workflow #{childInstanceId} completed, advancing parent",
            parentNodeInstance.WorkflowNodeId, parentNodeInstance.Id);

        // Advance the parent workflow past the subprocess node
        await AdvanceWorkflowAsync(parentInstanceId, parentNodeInstance.Id);
    }

    /// <summary>
    /// Get parallel branch status for a workflow instance.
    /// </summary>
    public async Task<ParallelBranchStatus> GetParallelBranchStatusAsync(int instanceId, int? gatewayNodeId = null)
    {
        var query = _context.WorkflowNodeInstances
            .Include(ni => ni.WorkflowNode)
            .Where(ni => ni.WorkflowInstanceId == instanceId);

        // If a gateway is specified, find branches started from that gateway's outgoing transitions
        List<int> branchNodeIds;
        if (gatewayNodeId.HasValue)
        {
            var outgoingTargets = await _context.WorkflowTransitions
                .Where(t => t.SourceNodeId == gatewayNodeId.Value && !t.IsDeleted)
                .Select(t => t.TargetNodeId)
                .ToListAsync();

            branchNodeIds = outgoingTargets;
            query = query.Where(ni => branchNodeIds.Contains(ni.WorkflowNodeId));
        }

        var nodeInstances = await query.ToListAsync();

        // Filter to latest execution per node (in case of retries)
        var latestPerNode = nodeInstances
            .GroupBy(ni => ni.WorkflowNodeId)
            .Select(g => g.OrderByDescending(ni => ni.Id).First())
            .ToList();

        var result = new ParallelBranchStatus
        {
            InstanceId = instanceId,
            TotalBranches = latestPerNode.Count,
            CompletedBranches = latestPerNode.Count(ni => ni.Status == WorkflowNodeInstanceStatus.Completed),
            RunningBranches = latestPerNode.Count(ni => ni.Status == WorkflowNodeInstanceStatus.Running
                                                     || ni.Status == WorkflowNodeInstanceStatus.Waiting),
            FailedBranches = latestPerNode.Count(ni => ni.Status == WorkflowNodeInstanceStatus.Failed),
            Branches = latestPerNode.Select(ni => new BranchInfo
            {
                NodeInstanceId = ni.Id,
                NodeId = ni.WorkflowNodeId,
                NodeName = ni.WorkflowNode?.Name ?? $"Node #{ni.WorkflowNodeId}",
                Status = ni.Status.ToString(),
                StartedAt = ni.StartedAt,
                CompletedAt = ni.CompletedAt,
                DurationMs = ni.DurationMs
            }).ToList()
        };

        return result;
    }

    /// <summary>
    /// Get child workflow instances spawned by a parent.
    /// </summary>
    public async Task<List<WorkflowInstance>> GetChildInstancesAsync(int parentInstanceId)
    {
        return await _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
            .Where(i => i.ParentInstanceId == parentInstanceId && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    #endregion

    #region Wait/Timer & Timeout Processing

    /// <inheritdoc/>
    public async Task<WorkflowNodeInstance> StartWaitNodeAsync(int instanceId, int waitNodeId)
    {
        var node = await _context.WorkflowNodes
            .FirstOrDefaultAsync(n => n.Id == waitNodeId && !n.IsDeleted);
        if (node == null)
            throw new ArgumentException($"Wait node {waitNodeId} not found");
        if (node.NodeType != WorkflowNodeType.Wait)
            throw new ArgumentException($"Node {waitNodeId} is not a Wait node (type: {node.NodeType})");

        // Create node instance in Running state first
        var nodeInstance = await StartNodeExecutionAsync(instanceId, waitNodeId);

        // Parse the wait configuration from the node's Configuration JSON
        var resumeAt = CalculateResumeTime(node);

        // Set the node instance to Waiting with the calculated resume time
        nodeInstance.Status = WorkflowNodeInstanceStatus.Waiting;
        nodeInstance.NextRetryAt = resumeAt; // Reuse NextRetryAt as the resume timestamp
        nodeInstance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogAsync(instanceId, WorkflowLogLevel.Info, "WaitNode",
            $"Wait node '{node.Name}' started. Will resume at {resumeAt:O}");

        return nodeInstance;
    }

    /// <inheritdoc/>
    public async Task<int> ProcessDueWaitNodesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Find all waiting node instances whose resume time has arrived
        var dueNodes = await _context.WorkflowNodeInstances
            .Include(ni => ni.WorkflowNode)
            .Include(ni => ni.WorkflowInstance)
            .Where(ni => ni.Status == WorkflowNodeInstanceStatus.Waiting
                      && ni.WorkflowNode.NodeType == WorkflowNodeType.Wait
                      && ni.NextRetryAt != null
                      && ni.NextRetryAt <= now
                      && !ni.IsDeleted)
            .ToListAsync(cancellationToken);

        if (dueNodes.Count == 0)
            return 0;

        _logger.LogInformation("Processing {Count} due wait node(s)", dueNodes.Count);

        var processed = 0;
        foreach (var nodeInstance in dueNodes)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Verify the parent instance is still running
                var instance = nodeInstance.WorkflowInstance;
                if (instance == null || instance.Status != WorkflowInstanceStatus.Running)
                {
                    _logger.LogDebug(
                        "Skipping wait node {NodeInstanceId} — parent instance status is {Status}",
                        nodeInstance.Id, instance?.Status);
                    continue;
                }

                // Complete the wait node
                nodeInstance.Status = WorkflowNodeInstanceStatus.Completed;
                nodeInstance.CompletedAt = DateTime.UtcNow;
                nodeInstance.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                await LogAsync(instance.Id, WorkflowLogLevel.Info, "WaitNode",
                    $"Wait node '{nodeInstance.WorkflowNode?.Name}' timer elapsed — resuming workflow");

                // Advance the workflow past this completed wait node
                await AdvanceWorkflowAsync(instance.Id, nodeInstance.Id);

                processed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing due wait node {NodeInstanceId}", nodeInstance.Id);
            }
        }

        if (processed > 0)
            _logger.LogInformation("Processed {Count} due wait node(s)", processed);

        return processed;
    }

    /// <inheritdoc/>
    public async Task<int> ProcessTimedOutInstancesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var processed = 0;

        // 1. Timeout workflow instances that have exceeded their TimeoutAt
        var timedOutInstances = await _context.WorkflowInstances
            .Where(i => i.Status == WorkflowInstanceStatus.Running
                     && i.TimeoutAt != null
                     && i.TimeoutAt <= now
                     && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var instance in timedOutInstances)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                instance.Status = WorkflowInstanceStatus.TimedOut;
                instance.CompletedAt = DateTime.UtcNow;
                instance.UpdatedAt = DateTime.UtcNow;
                instance.ErrorMessage = $"Workflow timed out at {now:O} (timeout was set to {instance.TimeoutAt:O})";
                await _context.SaveChangesAsync(cancellationToken);

                await LogAsync(instance.Id, WorkflowLogLevel.Warning, "Timeout",
                    $"Workflow instance timed out after exceeding deadline");

                processed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error timing out instance {InstanceId}", instance.Id);
            }
        }

        // 2. Timeout individual node instances that have exceeded their node-level timeout
        var timedOutNodes = await _context.WorkflowNodeInstances
            .Include(ni => ni.WorkflowNode)
            .Where(ni => (ni.Status == WorkflowNodeInstanceStatus.Running || ni.Status == WorkflowNodeInstanceStatus.Waiting)
                      && ni.WorkflowNode.TimeoutMinutes > 0
                      && ni.StartedAt != null
                      && ni.StartedAt.Value.AddMinutes(ni.WorkflowNode.TimeoutMinutes) <= now
                      && !ni.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var nodeInstance in timedOutNodes)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                nodeInstance.Status = WorkflowNodeInstanceStatus.Failed;
                nodeInstance.CompletedAt = DateTime.UtcNow;
                nodeInstance.UpdatedAt = DateTime.UtcNow;
                nodeInstance.ErrorMessage = $"Node timed out after {nodeInstance.WorkflowNode?.TimeoutMinutes} minutes";
                await _context.SaveChangesAsync(cancellationToken);

                await LogAsync(nodeInstance.WorkflowInstanceId, WorkflowLogLevel.Warning, "Timeout",
                    $"Node '{nodeInstance.WorkflowNode?.Name}' timed out after {nodeInstance.WorkflowNode?.TimeoutMinutes} minutes");

                processed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error timing out node instance {NodeInstanceId}", nodeInstance.Id);
            }
        }

        if (processed > 0)
        {
            _logger.LogInformation("Processed {Count} timed-out item(s) ({Instances} instances, {Nodes} nodes)",
                processed, timedOutInstances.Count, timedOutNodes.Count);
        }

        return processed;
    }

    /// <inheritdoc/>
    public async Task<List<WorkflowNodeInstance>> GetWaitingNodesAsync(int? instanceId = null)
    {
        var query = _context.WorkflowNodeInstances
            .Include(ni => ni.WorkflowNode)
            .Include(ni => ni.WorkflowInstance)
            .Where(ni => ni.Status == WorkflowNodeInstanceStatus.Waiting
                      && ni.WorkflowNode.NodeType == WorkflowNodeType.Wait
                      && !ni.IsDeleted);

        if (instanceId.HasValue)
            query = query.Where(ni => ni.WorkflowInstanceId == instanceId.Value);

        return await query.OrderBy(ni => ni.NextRetryAt).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<WorkflowNodeInstance> ResumeWaitingNodeAsync(int nodeInstanceId)
    {
        var nodeInstance = await _context.WorkflowNodeInstances
            .Include(ni => ni.WorkflowNode)
            .FirstOrDefaultAsync(ni => ni.Id == nodeInstanceId && !ni.IsDeleted);

        if (nodeInstance == null)
            throw new ArgumentException($"Node instance {nodeInstanceId} not found");

        if (nodeInstance.Status != WorkflowNodeInstanceStatus.Waiting)
        {
            throw new InvalidOperationException(
                $"Node instance {nodeInstanceId} is not in Waiting status (current: {nodeInstance.Status})");
        }

        // Complete the wait node immediately
        nodeInstance.Status = WorkflowNodeInstanceStatus.Completed;
        nodeInstance.CompletedAt = DateTime.UtcNow;
        nodeInstance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogAsync(nodeInstance.WorkflowInstanceId, WorkflowLogLevel.Info, "WaitNode",
            $"Wait node '{nodeInstance.WorkflowNode?.Name}' manually resumed by user");

        // Advance the workflow past this node
        await AdvanceWorkflowAsync(nodeInstance.WorkflowInstanceId, nodeInstance.Id);

        return nodeInstance;
    }

    /// <summary>
    /// Calculate when a wait node should resume based on its Configuration JSON.
    /// Supports: delayMinutes, delayHours, delaySeconds, waitUntil (ISO 8601).
    /// </summary>
    /// <summary>
    /// If an Action node's Configuration contains an "httpCallout" section,
    /// execute the HTTP callout, store the response as output data, and
    /// complete or fail the node accordingly.
    /// </summary>
    private async Task ExecuteHttpCalloutNodeAsync(int nodeInstanceId, WorkflowNode node)
    {
        if (string.IsNullOrWhiteSpace(node.Configuration))
            return; // No config — nothing to execute automatically

        try
        {
            using var doc = JsonDocument.Parse(node.Configuration);
            if (!doc.RootElement.TryGetProperty("httpCallout", out var calloutElement))
                return; // No httpCallout section — regular action node

            var config = JsonSerializer.Deserialize<HttpCalloutConfig>(
                calloutElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (config == null)
                return;

            config.Name ??= node.Name;

            var nodeInstance = await _context.WorkflowNodeInstances.FindAsync(nodeInstanceId);
            if (nodeInstance == null)
                return;

            _logger.LogInformation(
                "Executing HTTP callout for node instance {NodeInstanceId}: {Method} {Url}",
                nodeInstanceId, config.Method, config.Url);

            var result = await _httpCalloutService.ExecuteAsync(config);

            if (result.Success)
            {
                await CompleteNodeExecutionAsync(nodeInstanceId, new
                {
                    result.StatusCode,
                    result.ReasonPhrase,
                    result.ResponseBody,
                    result.ElapsedMs,
                    result.Attempts
                });

                // Advance workflow past this node
                await AdvanceWorkflowAsync(nodeInstance.WorkflowInstanceId, nodeInstanceId);
            }
            else
            {
                await FailNodeExecutionAsync(
                    nodeInstanceId,
                    $"HTTP callout failed: {result.ErrorMessage} (status {result.StatusCode})",
                    result.ResponseBody);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON in node {NodeId} Configuration", node.Id);
            await FailNodeExecutionAsync(nodeInstanceId,
                $"Invalid httpCallout configuration: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing HTTP callout for node instance {NodeInstanceId}", nodeInstanceId);
            await FailNodeExecutionAsync(nodeInstanceId, $"HTTP callout error: {ex.Message}");
        }
    }

    private DateTime CalculateResumeTime(WorkflowNode node)
    {
        if (string.IsNullOrWhiteSpace(node.Configuration))
        {
            // Default: 1 minute wait if no configuration
            return DateTime.UtcNow.AddMinutes(1);
        }

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(node.Configuration);
            var root = doc.RootElement;

            // Check for absolute time first
            if (root.TryGetProperty("waitUntil", out var waitUntilProp))
            {
                if (DateTime.TryParse(waitUntilProp.GetString(), out var waitUntil))
                    return waitUntil.ToUniversalTime();
            }

            // Check for relative delays
            var totalMinutes = 0.0;

            if (root.TryGetProperty("delaySeconds", out var secProp) && secProp.TryGetInt32(out var secs))
                totalMinutes += secs / 60.0;

            if (root.TryGetProperty("delayMinutes", out var minProp) && minProp.TryGetInt32(out var mins))
                totalMinutes += mins;

            if (root.TryGetProperty("delayHours", out var hrProp) && hrProp.TryGetInt32(out var hrs))
                totalMinutes += hrs * 60.0;

            if (root.TryGetProperty("delayDays", out var dayProp) && dayProp.TryGetInt32(out var days))
                totalMinutes += days * 24 * 60.0;

            if (totalMinutes > 0)
                return DateTime.UtcNow.AddMinutes(totalMinutes);
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Wait node configuration: {Config}", node.Configuration);
        }

        // Fallback: use TimeoutMinutes as a delay, or default to 1 minute
        return DateTime.UtcNow.AddMinutes(node.TimeoutMinutes > 0 ? node.TimeoutMinutes : 1);
    }

    #endregion
}
