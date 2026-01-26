// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities.Workflow;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing workflow instances and execution
/// </summary>
public class WorkflowInstanceService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<WorkflowInstanceService> _logger;
    private readonly WorkflowService _workflowService;

    public WorkflowInstanceService(
        CrmDbContext context,
        ILogger<WorkflowInstanceService> logger,
        WorkflowService workflowService)
    {
        _context = context;
        _logger = logger;
        _workflowService = workflowService;
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
    /// Cancel a workflow instance
    /// </summary>
    public async Task<bool> CancelInstanceAsync(int instanceId, string reason, int? userId = null)
    {
        var instance = await _context.WorkflowInstances.FindAsync(instanceId);
        if (instance == null) return false;

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
        if (instance == null || instance.Status != WorkflowInstanceStatus.Running) return false;

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
        if (instance == null || instance.Status != WorkflowInstanceStatus.Paused) return false;

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
        if (instance == null || instance.Status != WorkflowInstanceStatus.Failed) return false;

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
        if (instance == null) throw new ArgumentException("Instance not found");

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

        if (nodeInstance == null) return null;

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

        if (nodeInstance == null) return null;

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
        if (instance == null) return false;

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
        if (task == null) return false;

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
        if (task == null) return false;

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
        if (task == null) return false;

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
            query = query.Where(t => t.AssignedToId == userId ||
                                    (t.AssignedToRole != null && roles.Contains(t.AssignedToRole)));
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
}
