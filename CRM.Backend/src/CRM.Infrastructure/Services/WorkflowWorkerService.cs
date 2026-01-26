/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Workflow Worker Service - Background hosted service for processing workflow tasks
 * Implements competing consumers pattern with exponential backoff retry
 */

using System.Text.Json;
using CRM.Core.Entities.Workflow;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Configuration options for the workflow worker
/// </summary>
public class WorkflowWorkerOptions
{
    public string WorkerId { get; set; } = Environment.MachineName + "-" + Guid.NewGuid().ToString("N")[..8];
    public int MaxConcurrentTasks { get; set; } = 5;
    public int PollIntervalSeconds { get; set; } = 5;
    public int LockDurationMinutes { get; set; } = 15;
    public int MaxRetryCount { get; set; } = 3;
    public int BaseRetryDelaySeconds { get; set; } = 30;
    public bool EnableLLMActions { get; set; } = true;
    public string[] QueueNames { get; set; } = { "default", "priority", "background" };
}

/// <summary>
/// Background service that processes workflow tasks using a competing consumers pattern
/// </summary>
public class WorkflowWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowWorkerService> _logger;
    private readonly WorkflowWorkerOptions _options;
    private readonly SemaphoreSlim _semaphore;
    private int _activeTaskCount = 0;
    private readonly Dictionary<string, Func<WorkflowTask, CrmDbContext, CancellationToken, Task<TaskResult>>> _actionHandlers;

    public WorkflowWorkerService(
        IServiceProvider serviceProvider,
        ILogger<WorkflowWorkerService> logger,
        WorkflowWorkerOptions? options = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options ?? new WorkflowWorkerOptions();
        _semaphore = new SemaphoreSlim(_options.MaxConcurrentTasks, _options.MaxConcurrentTasks);
        _actionHandlers = InitializeActionHandlers();
    }

    /// <summary>
    /// Initialize the action handlers for different node types
    /// </summary>
    private Dictionary<string, Func<WorkflowTask, CrmDbContext, CancellationToken, Task<TaskResult>>> InitializeActionHandlers()
    {
        return new Dictionary<string, Func<WorkflowTask, CrmDbContext, CancellationToken, Task<TaskResult>>>
        {
            { "Automated", ExecuteAutomatedAction },
            { "Timer", ExecuteTimerAction },
            { "Event", ExecuteEventAction },
            { "LLM", ExecuteLLMAction },
            { "Notification", ExecuteNotificationAction },
            { "Integration", ExecuteIntegrationAction },
            { "DataOperation", ExecuteDataOperationAction },
            { "ZipCodeImport", ExecuteZipCodeImportAction },
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Workflow Worker {WorkerId} starting with {MaxConcurrent} concurrent tasks",
            _options.WorkerId, _options.MaxConcurrentTasks);

        // Initial delay to let the application start
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Try to acquire a slot
                if (await _semaphore.WaitAsync(TimeSpan.FromSeconds(1), stoppingToken))
                {
                    try
                    {
                        // Fetch and process a task
                        var task = await FetchNextTask(stoppingToken);
                        if (task != null)
                        {
                            // Process asynchronously
                            _ = ProcessTaskAsync(task, stoppingToken);
                        }
                        else
                        {
                            // No tasks available, release the slot and wait
                            _semaphore.Release();
                            await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _semaphore.Release();
                        _logger.LogError(ex, "Error fetching task");
                        await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in worker loop");
                await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
            }
        }

        _logger.LogInformation("Workflow Worker {WorkerId} stopping", _options.WorkerId);
    }

    /// <summary>
    /// Fetch the next available task using pessimistic locking (competing consumers)
    /// </summary>
    private async Task<WorkflowTask?> FetchNextTask(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();

        // Use transaction with row locking to implement competing consumers
        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var now = DateTime.UtcNow;
            var lockExpiry = now.AddMinutes(_options.LockDurationMinutes);

            // Find the next available task
            // Tasks are eligible if:
            // 1. Status is Pending and not locked
            // 2. Status is Running but lock has expired (worker died)
            // 3. Not in dead letter state
            // Priority: Lower number = higher priority
            var task = await dbContext.WorkflowTasks
                .Where(t => !t.IsDeleted && !t.IsDeadLetter &&
                    t.QueueName != null && _options.QueueNames.Contains(t.QueueName) &&
                    ((t.Status == WorkflowTaskStatus.Pending && (t.LockedByWorkerId == null || t.LockExpiresAt < now)) ||
                     (t.Status == WorkflowTaskStatus.Running && t.LockExpiresAt < now)) &&
                    (t.ScheduledAt == null || t.ScheduledAt <= now))
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.ScheduledAt ?? t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (task == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }

            // Lock the task
            task.Status = WorkflowTaskStatus.Running;
            task.LockedByWorkerId = _options.WorkerId;
            task.LockExpiresAt = lockExpiry;
            task.PickedAt = now;
            task.RetryCount++;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogDebug("Worker {WorkerId} locked task {TaskId}", _options.WorkerId, task.Id);
            return task;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error fetching next task");
            return null;
        }
    }

    /// <summary>
    /// Process a workflow task
    /// </summary>
    private async Task ProcessTaskAsync(WorkflowTask task, CancellationToken stoppingToken)
    {
        Interlocked.Increment(ref _activeTaskCount);
        var startTime = DateTime.UtcNow;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();

            _logger.LogInformation("Processing task {TaskId} of type {TaskType} for instance {InstanceId}",
                task.Id, task.TaskType, task.WorkflowInstanceId);

            // Log task start
            await LogWorkflowEvent(dbContext, task.WorkflowInstanceId, task.NodeInstanceId,
                WorkflowLogLevel.Info, "TaskExecution", $"Starting task {task.Id} ({task.TaskType})", stoppingToken);

            // Execute the task based on type
            TaskResult result;
            var taskTypeKey = task.TaskType.ToString();
            if (_actionHandlers.TryGetValue(taskTypeKey, out var handler))
            {
                result = await handler(task, dbContext, stoppingToken);
            }
            else
            {
                result = await ExecuteGenericAction(task, dbContext, stoppingToken);
            }

            // Update task based on result
            var trackedTask = await dbContext.WorkflowTasks.FindAsync(new object[] { task.Id }, stoppingToken);
            if (trackedTask != null)
            {
                trackedTask.CompletedAt = DateTime.UtcNow;
                trackedTask.OutputData = result.ResultData;

                if (result.Success)
                {
                    trackedTask.Status = WorkflowTaskStatus.Completed;
                    await LogWorkflowEvent(dbContext, task.WorkflowInstanceId, task.NodeInstanceId,
                        WorkflowLogLevel.Info, "TaskExecution", $"Task {task.Id} completed successfully", stoppingToken);
                }
                else
                {
                    if (trackedTask.RetryCount >= task.MaxRetries)
                    {
                        // Move to dead letter queue
                        trackedTask.Status = WorkflowTaskStatus.Failed;
                        trackedTask.IsDeadLetter = true;
                        trackedTask.DeadLetterReason = result.ErrorMessage;
                        trackedTask.DeadLetterAt = DateTime.UtcNow;
                        await LogWorkflowEvent(dbContext, task.WorkflowInstanceId, task.NodeInstanceId,
                            WorkflowLogLevel.Error, "TaskExecution", $"Task {task.Id} failed permanently: {result.ErrorMessage}", stoppingToken);
                    }
                    else
                    {
                        // Schedule retry with exponential backoff
                        var delay = TimeSpan.FromSeconds(_options.BaseRetryDelaySeconds * Math.Pow(2, trackedTask.RetryCount - 1));
                        trackedTask.Status = WorkflowTaskStatus.Pending;
                        trackedTask.ScheduledAt = DateTime.UtcNow.Add(delay);
                        trackedTask.NextRetryAt = trackedTask.ScheduledAt;
                        trackedTask.LockedByWorkerId = null;
                        trackedTask.LockExpiresAt = null;
                        trackedTask.ErrorMessage = result.ErrorMessage;
                        await LogWorkflowEvent(dbContext, task.WorkflowInstanceId, task.NodeInstanceId,
                            WorkflowLogLevel.Warning, "TaskExecution", $"Task {task.Id} failed, scheduling retry in {delay.TotalSeconds}s: {result.ErrorMessage}", stoppingToken);
                    }
                }

                // Update node instance if exists
                if (task.NodeInstanceId.HasValue)
                {
                    var nodeInstance = await dbContext.WorkflowNodeInstances.FindAsync(
                        new object[] { task.NodeInstanceId.Value }, stoppingToken);
                    if (nodeInstance != null)
                    {
                        nodeInstance.CompletedAt = trackedTask.CompletedAt;
                        nodeInstance.Status = result.Success 
                            ? WorkflowNodeInstanceStatus.Completed 
                            : (trackedTask.IsDeadLetter ? WorkflowNodeInstanceStatus.Failed : WorkflowNodeInstanceStatus.Running);
                        nodeInstance.DurationMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                        if (!result.Success)
                        {
                            nodeInstance.ErrorMessage = result.ErrorMessage;
                        }
                        nodeInstance.OutputData = result.ResultData;
                    }
                }

                // If task completed, potentially advance the workflow
                if (result.Success)
                {
                    await AdvanceWorkflowAsync(dbContext, task, result, stoppingToken);
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing task {TaskId}", task.Id);

            // Try to mark task as failed
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
                var trackedTask = await dbContext.WorkflowTasks.FindAsync(new object[] { task.Id }, stoppingToken);
                if (trackedTask != null)
                {
                    if (trackedTask.RetryCount >= task.MaxRetries)
                    {
                        trackedTask.Status = WorkflowTaskStatus.Failed;
                        trackedTask.IsDeadLetter = true;
                        trackedTask.DeadLetterReason = ex.Message;
                        trackedTask.DeadLetterAt = DateTime.UtcNow;
                        trackedTask.ErrorMessage = ex.Message;
                        trackedTask.ErrorStackTrace = ex.StackTrace;
                    }
                    else
                    {
                        trackedTask.Status = WorkflowTaskStatus.Pending;
                        trackedTask.ScheduledAt = DateTime.UtcNow.AddSeconds(
                            _options.BaseRetryDelaySeconds * Math.Pow(2, trackedTask.RetryCount - 1));
                        trackedTask.NextRetryAt = trackedTask.ScheduledAt;
                        trackedTask.LockedByWorkerId = null;
                        trackedTask.LockExpiresAt = null;
                        trackedTask.ErrorMessage = ex.Message;
                    }
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "Failed to mark task {TaskId} as failed", task.Id);
            }
        }
        finally
        {
            Interlocked.Decrement(ref _activeTaskCount);
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Advance the workflow to the next node after task completion
    /// </summary>
    private async Task AdvanceWorkflowAsync(CrmDbContext dbContext, WorkflowTask task, TaskResult result, CancellationToken cancellationToken)
    {
        if (task.WorkflowInstanceId == 0) return;

        var instance = await dbContext.WorkflowInstances
            .Include(i => i.WorkflowVersion)
            .ThenInclude(v => v!.Nodes)
            .ThenInclude(n => n.OutgoingTransitions)
            .FirstOrDefaultAsync(i => i.Id == task.WorkflowInstanceId, cancellationToken);

        if (instance == null || instance.Status != WorkflowInstanceStatus.Running) return;

        // Find the current node
        var currentNode = instance.WorkflowVersion?.Nodes
            .FirstOrDefault(n => n.Id == instance.CurrentNodeId);

        if (currentNode == null) return;

        // Evaluate transitions and find the next node
        var nextTransition = await EvaluateTransitionsAsync(dbContext, currentNode, instance, result, cancellationToken);

        if (nextTransition != null)
        {
            var nextNode = instance.WorkflowVersion?.Nodes
                .FirstOrDefault(n => n.Id == nextTransition.TargetNodeId);

            if (nextNode != null)
            {
                // Check if next node is an End node
                if (nextNode.IsEndNode)
                {
                    instance.Status = WorkflowInstanceStatus.Completed;
                    instance.CompletedAt = DateTime.UtcNow;
                    instance.CurrentNodeId = nextNode.Id;
                    instance.OutputData = result.ResultData;

                    await LogWorkflowEvent(dbContext, instance.Id, null,
                        WorkflowLogLevel.Info, "WorkflowEnd", "Workflow completed successfully", cancellationToken);
                }
                else
                {
                    // Create node instance and task for the next node
                    instance.CurrentNodeId = nextNode.Id;
                    await CreateNodeExecutionAsync(dbContext, instance, nextNode, result.ResultData, cancellationToken);
                }
            }
        }
        else if (currentNode.IsEndNode)
        {
            instance.Status = WorkflowInstanceStatus.Completed;
            instance.CompletedAt = DateTime.UtcNow;
            instance.OutputData = result.ResultData;
        }
    }

    /// <summary>
    /// Evaluate outgoing transitions to determine the next node
    /// </summary>
    private async Task<WorkflowTransition?> EvaluateTransitionsAsync(
        CrmDbContext dbContext,
        WorkflowNode currentNode,
        WorkflowInstance instance,
        TaskResult result,
        CancellationToken cancellationToken)
    {
        var transitions = currentNode.OutgoingTransitions
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Priority)
            .ToList();

        if (!transitions.Any()) return null;

        // Parse current state data
        var stateData = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(instance.StateData))
        {
            try
            {
                stateData = JsonSerializer.Deserialize<Dictionary<string, object>>(instance.StateData) ?? new Dictionary<string, object>();
            }
            catch { }
        }

        // Merge result data into state
        if (!string.IsNullOrEmpty(result.ResultData))
        {
            try
            {
                var resultData = JsonSerializer.Deserialize<Dictionary<string, object>>(result.ResultData);
                if (resultData != null)
                {
                    foreach (var kvp in resultData)
                    {
                        stateData[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch { }
        }

        instance.StateData = JsonSerializer.Serialize(stateData);

        foreach (var transition in transitions)
        {
            var conditionMet = transition.ConditionType switch
            {
                TransitionConditionType.Always => true,
                TransitionConditionType.Expression => EvaluateExpression(transition.ConditionExpression, stateData),
                TransitionConditionType.FieldMatch => EvaluateFieldMatch(transition.ConditionExpression, stateData),
                TransitionConditionType.UserChoice => EvaluateUserChoice(transition.ConditionExpression, result),
                _ => transition.IsDefault
            };

            if (conditionMet)
            {
                return transition;
            }
        }

        // Return default transition if no condition matched
        return transitions.FirstOrDefault(t => t.IsDefault);
    }

    private bool EvaluateExpression(string? expression, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(expression)) return true;

        // Simple expression evaluation (field == value, field > value, etc.)
        // In production, use a proper expression evaluator like NCalc
        try
        {
            var parts = expression.Split(new[] { "==", "!=", ">", "<", ">=", "<=" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var field = parts[0].Trim();
                var value = parts[1].Trim().Trim('"');

                if (data.TryGetValue(field, out var fieldValue))
                {
                    var fieldStr = fieldValue?.ToString() ?? "";

                    if (expression.Contains("==")) return fieldStr.Equals(value, StringComparison.OrdinalIgnoreCase);
                    if (expression.Contains("!=")) return !fieldStr.Equals(value, StringComparison.OrdinalIgnoreCase);

                    if (double.TryParse(fieldStr, out var numField) && double.TryParse(value, out var numValue))
                    {
                        if (expression.Contains(">=")) return numField >= numValue;
                        if (expression.Contains("<=")) return numField <= numValue;
                        if (expression.Contains(">")) return numField > numValue;
                        if (expression.Contains("<")) return numField < numValue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error evaluating expression: {Expression}", expression);
        }

        return false;
    }

    private bool EvaluateFieldMatch(string? expression, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(expression)) return true;

        try
        {
            var conditions = JsonSerializer.Deserialize<Dictionary<string, string>>(expression);
            if (conditions == null) return true;

            foreach (var condition in conditions)
            {
                if (!data.TryGetValue(condition.Key, out var value) ||
                    !value?.ToString()?.Equals(condition.Value, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool EvaluateUserChoice(string? expression, TaskResult result)
    {
        if (string.IsNullOrEmpty(expression) || string.IsNullOrEmpty(result.ResultData)) return false;

        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(result.ResultData);
            if (data?.TryGetValue("userChoice", out var choice) == true)
            {
                return choice?.ToString()?.Equals(expression, StringComparison.OrdinalIgnoreCase) == true;
            }
        }
        catch { }

        return false;
    }

    /// <summary>
    /// Create execution for a workflow node
    /// </summary>
    private async Task CreateNodeExecutionAsync(
        CrmDbContext dbContext,
        WorkflowInstance instance,
        WorkflowNode node,
        string? inputData,
        CancellationToken cancellationToken)
    {
        // Determine execution sequence
        var maxSequence = await dbContext.WorkflowNodeInstances
            .Where(ni => ni.WorkflowInstanceId == instance.Id)
            .MaxAsync(ni => (int?)ni.ExecutionSequence, cancellationToken) ?? 0;

        // Create node instance
        var nodeInstance = new WorkflowNodeInstance
        {
            WorkflowInstanceId = instance.Id,
            WorkflowNodeId = node.Id,
            Status = WorkflowNodeInstanceStatus.Pending,
            StartedAt = DateTime.UtcNow,
            RetryCount = 0,
            ExecutionSequence = maxSequence + 1,
            InputData = inputData
        };

        dbContext.WorkflowNodeInstances.Add(nodeInstance);
        await dbContext.SaveChangesAsync(cancellationToken);

        // If it's a human task, don't create automated task
        if (node.NodeType == WorkflowNodeType.HumanTask)
        {
            nodeInstance.Status = WorkflowNodeInstanceStatus.Waiting;
            instance.Status = WorkflowInstanceStatus.Waiting;

            // Create human task
            var humanTask = new WorkflowTask
            {
                WorkflowInstanceId = instance.Id,
                WorkflowNodeId = node.Id,
                NodeInstanceId = nodeInstance.Id,
                TaskType = WorkflowTaskType.Human,
                Name = node.Name,
                Status = WorkflowTaskStatus.Pending,
                Priority = 100,
                QueueName = "human",
                MaxRetries = 1,
                InputData = node.Configuration,
                FormSchema = node.Configuration // Contains form definition
            };
            dbContext.WorkflowTasks.Add(humanTask);
        }
        else if (node.NodeType == WorkflowNodeType.Wait)
        {
            // Parse wait configuration
            var waitMinutes = 60; // Default 1 hour
            if (!string.IsNullOrEmpty(node.Configuration))
            {
                try
                {
                    var config = JsonSerializer.Deserialize<Dictionary<string, int>>(node.Configuration);
                    if (config?.TryGetValue("waitMinutes", out var wm) == true)
                    {
                        waitMinutes = wm;
                    }
                }
                catch { }
            }

            nodeInstance.Status = WorkflowNodeInstanceStatus.Waiting;
            instance.Status = WorkflowInstanceStatus.Waiting;

            var timerTask = new WorkflowTask
            {
                WorkflowInstanceId = instance.Id,
                WorkflowNodeId = node.Id,
                NodeInstanceId = nodeInstance.Id,
                TaskType = WorkflowTaskType.Timer,
                Name = $"Wait: {node.Name}",
                Status = WorkflowTaskStatus.Pending,
                Priority = 200,
                QueueName = "default",
                MaxRetries = 1,
                ScheduledAt = DateTime.UtcNow.AddMinutes(waitMinutes),
                InputData = node.Configuration
            };
            dbContext.WorkflowTasks.Add(timerTask);
        }
        else
        {
            // Create automated task
            var taskType = node.NodeType == WorkflowNodeType.LLMAction 
                ? WorkflowTaskType.LLM 
                : WorkflowTaskType.Automated;
            var queueName = node.NodeType == WorkflowNodeType.LLMAction ? "llm" : "default";

            var task = new WorkflowTask
            {
                WorkflowInstanceId = instance.Id,
                WorkflowNodeId = node.Id,
                NodeInstanceId = nodeInstance.Id,
                TaskType = taskType,
                Name = node.Name,
                Status = WorkflowTaskStatus.Pending,
                Priority = 100,
                QueueName = queueName,
                MaxRetries = node.RetryCount > 0 ? node.RetryCount : 3,
                InputData = node.Configuration
            };

            if (node.TimeoutMinutes > 0)
            {
                task.TimeoutAt = DateTime.UtcNow.AddMinutes(node.TimeoutMinutes);
            }

            dbContext.WorkflowTasks.Add(task);
            nodeInstance.Status = WorkflowNodeInstanceStatus.Running;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await LogWorkflowEvent(dbContext, instance.Id, nodeInstance.Id,
            WorkflowLogLevel.Info, "NodeExecution", $"Started node {node.Name} ({node.NodeType})", cancellationToken);
    }

    /// <summary>
    /// Log a workflow event
    /// </summary>
    private async Task LogWorkflowEvent(
        CrmDbContext dbContext,
        int instanceId,
        int? nodeInstanceId,
        WorkflowLogLevel level,
        string category,
        string message,
        CancellationToken cancellationToken)
    {
        var log = new WorkflowLog
        {
            WorkflowInstanceId = instanceId,
            NodeInstanceId = nodeInstanceId,
            Level = level,
            Category = category,
            Message = message,
            Timestamp = DateTime.UtcNow,
            WorkerId = _options.WorkerId
        };

        dbContext.WorkflowLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Task Execution Handlers

    private async Task<TaskResult> ExecuteAutomatedAction(WorkflowTask task, CrmDbContext dbContext, CancellationToken ct)
    {
        // Parse configuration
        var config = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(task.InputData))
        {
            try
            {
                config = JsonSerializer.Deserialize<Dictionary<string, object>>(task.InputData) ?? new Dictionary<string, object>();
            }
            catch { }
        }

        // Get action type from config
        var actionType = config.GetValueOrDefault("actionType")?.ToString() ?? "log";

        return actionType switch
        {
            "log" => await ExecuteLogAction(config),
            "updateEntity" => await ExecuteUpdateEntityAction(config, dbContext, ct),
            "sendEmail" => await ExecuteSendEmailAction(config),
            "webhook" => await ExecuteWebhookAction(config, ct),
            _ => new TaskResult { Success = true, ResultData = JsonSerializer.Serialize(new { actionType, status = "completed" }) }
        };
    }

    private Task<TaskResult> ExecuteLogAction(Dictionary<string, object> config)
    {
        var message = config.GetValueOrDefault("message")?.ToString() ?? "Workflow action executed";
        _logger.LogInformation("Workflow Log Action: {Message}", message);
        return Task.FromResult(new TaskResult
        {
            Success = true,
            ResultData = JsonSerializer.Serialize(new { logged = true, message })
        });
    }

    private async Task<TaskResult> ExecuteUpdateEntityAction(Dictionary<string, object> config, CrmDbContext dbContext, CancellationToken ct)
    {
        // This would update an entity based on config
        // Implementation depends on entity type and fields
        await Task.Delay(100, ct); // Simulated operation
        return new TaskResult
        {
            Success = true,
            ResultData = JsonSerializer.Serialize(new { updated = true })
        };
    }

    private Task<TaskResult> ExecuteSendEmailAction(Dictionary<string, object> config)
    {
        // Would integrate with email service
        var to = config.GetValueOrDefault("to")?.ToString();
        var subject = config.GetValueOrDefault("subject")?.ToString();
        _logger.LogInformation("Would send email to {To} with subject {Subject}", to, subject);
        return Task.FromResult(new TaskResult
        {
            Success = true,
            ResultData = JsonSerializer.Serialize(new { emailSent = true, to, subject })
        });
    }

    private async Task<TaskResult> ExecuteWebhookAction(Dictionary<string, object> config, CancellationToken ct)
    {
        var url = config.GetValueOrDefault("url")?.ToString();
        if (string.IsNullOrEmpty(url))
        {
            return new TaskResult { Success = false, ErrorMessage = "Webhook URL not configured" };
        }

        try
        {
            // Get resilience service for circuit breaker and retry
            using var scope = _serviceProvider.CreateScope();
            var resilienceService = scope.ServiceProvider.GetService<IResilienceService>();
            
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            
            HttpResponseMessage response;
            if (resilienceService != null)
            {
                response = await resilienceService.ExecuteAsync(
                    "Webhook-Outbound",
                    async innerCt => await client.PostAsync(url,
                        new StringContent(JsonSerializer.Serialize(config), System.Text.Encoding.UTF8, "application/json"), innerCt),
                    ct);
            }
            else
            {
                response = await client.PostAsync(url,
                    new StringContent(JsonSerializer.Serialize(config), System.Text.Encoding.UTF8, "application/json"), ct);
            }

            return new TaskResult
            {
                Success = response.IsSuccessStatusCode,
                ResultData = JsonSerializer.Serialize(new { statusCode = (int)response.StatusCode }),
                ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new TaskResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private Task<TaskResult> ExecuteTimerAction(WorkflowTask task, CrmDbContext dbContext, CancellationToken ct)
    {
        // Timer already waited via ScheduledAt, just complete
        return Task.FromResult(new TaskResult
        {
            Success = true,
            ResultData = JsonSerializer.Serialize(new { timerCompleted = true, completedAt = DateTime.UtcNow })
        });
    }

    private Task<TaskResult> ExecuteEventAction(WorkflowTask task, CrmDbContext dbContext, CancellationToken ct)
    {
        // Event actions wait for external triggers
        // For now, just complete
        return Task.FromResult(new TaskResult
        {
            Success = true,
            ResultData = JsonSerializer.Serialize(new { eventProcessed = true })
        });
    }

    private async Task<TaskResult> ExecuteLLMAction(WorkflowTask task, CrmDbContext dbContext, CancellationToken ct)
    {
        if (!_options.EnableLLMActions)
        {
            return new TaskResult { Success = false, ErrorMessage = "LLM actions are disabled" };
        }

        var config = new LLMActionConfig();
        if (!string.IsNullOrEmpty(task.InputData))
        {
            try
            {
                config = JsonSerializer.Deserialize<LLMActionConfig>(task.InputData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new LLMActionConfig();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse LLM action config, using defaults");
            }
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(config.Prompt))
        {
            return new TaskResult { Success = false, ErrorMessage = "LLM prompt is required" };
        }

        _logger.LogInformation("Executing LLM action with provider {Provider}, model {Model}", 
            config.Provider, config.Model);

        try
        {
            // Get LLM service from DI
            using var scope = _serviceProvider.CreateScope();
            var llmService = scope.ServiceProvider.GetService<ILLMService>();
            var resilienceService = scope.ServiceProvider.GetService<IResilienceService>();

            if (llmService == null)
            {
                // Fall back to simulated response if LLM service not registered
                _logger.LogWarning("LLM service not registered, using simulated response");
                return new TaskResult
                {
                    Success = true,
                    ResultData = JsonSerializer.Serialize(new
                    {
                        llmResponse = "Simulated LLM response (service not configured)",
                        model = config.Model,
                        tokensUsed = 0,
                        simulated = true
                    })
                };
            }

            // Build the request
            var llmRequest = new LLMRequest
            {
                Provider = config.Provider,
                Model = config.Model,
                Prompt = config.Prompt,
                SystemPrompt = config.SystemPrompt,
                Temperature = config.Temperature,
                MaxTokens = config.MaxTokens,
                JsonMode = config.JsonMode,
                Variables = config.Variables
            };

            // Execute with resilience (circuit breaker + retry)
            LLMResponse response;
            if (resilienceService != null)
            {
                response = await resilienceService.ExecuteWithFallbackAsync(
                    $"llm-{config.Provider}",
                    async (innerCt) => await llmService.ChatAsync(llmRequest, innerCt),
                    (ex) => new LLMResponse 
                    { 
                        Success = false, 
                        Error = ex.Message 
                    },
                    ct);
            }
            else
            {
                response = await llmService.ChatAsync(llmRequest, ct);
            }

            if (!response.Success)
            {
                // Check if we should use fallback action
                if (!string.IsNullOrEmpty(config.FallbackAction))
                {
                    _logger.LogWarning("LLM call failed, executing fallback action: {FallbackAction}", config.FallbackAction);
                    return new TaskResult
                    {
                        Success = true,
                        ResultData = JsonSerializer.Serialize(new
                        {
                            fallback = true,
                            fallbackAction = config.FallbackAction,
                            originalError = response.Error
                        })
                    };
                }

                return new TaskResult 
                { 
                    Success = false, 
                    ErrorMessage = response.Error ?? "LLM call failed" 
                };
            }

            return new TaskResult
            {
                Success = true,
                ResultData = JsonSerializer.Serialize(new
                {
                    llmResponse = response.Content,
                    parsedJson = response.ParsedJson,
                    model = response.Model,
                    provider = response.Provider,
                    promptTokens = response.PromptTokens,
                    completionTokens = response.CompletionTokens,
                    totalTokens = response.TotalTokens,
                    durationMs = response.DurationMs
                })
            };
        }
        catch (ServiceUnavailableException ex)
        {
            _logger.LogError(ex, "LLM service unavailable (circuit open)");
            
            if (!string.IsNullOrEmpty(config.FallbackAction))
            {
                return new TaskResult
                {
                    Success = true,
                    ResultData = JsonSerializer.Serialize(new
                    {
                        fallback = true,
                        fallbackAction = config.FallbackAction,
                        reason = "service_unavailable"
                    })
                };
            }

            return new TaskResult { Success = false, ErrorMessage = ex.Message };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing LLM action");
            return new TaskResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Configuration for LLM action nodes
    /// </summary>
    public class LLMActionConfig
    {
        public string Provider { get; set; } = "openai";
        public string Model { get; set; } = "gpt-4";
        public string Prompt { get; set; } = "";
        public string? SystemPrompt { get; set; }
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 1000;
        public bool JsonMode { get; set; }
        public string? JsonSchema { get; set; }
        public Dictionary<string, object>? Variables { get; set; }
        public int TimeoutSeconds { get; set; } = 60;
        public string? FallbackAction { get; set; }
        public string? OutputVariableName { get; set; } = "llmResult";
    }

    private Task<TaskResult> ExecuteNotificationAction(WorkflowTask task, CrmDbContext dbContext, CancellationToken ct)
    {
        // Would send push notification, SMS, etc.
        return Task.FromResult(new TaskResult
        {
            Success = true,
            ResultData = JsonSerializer.Serialize(new { notificationSent = true })
        });
    }

    private Task<TaskResult> ExecuteIntegrationAction(WorkflowTask task, CrmDbContext dbContext, CancellationToken ct)
    {
        // Would call external integrations (Salesforce, HubSpot, etc.)
        return Task.FromResult(new TaskResult
        {
            Success = true,
            ResultData = JsonSerializer.Serialize(new { integrationCompleted = true })
        });
    }

    private Task<TaskResult> ExecuteDataOperationAction(WorkflowTask task, CrmDbContext dbContext, CancellationToken ct)
    {
        // Would perform data operations (aggregate, transform, etc.)
        return Task.FromResult(new TaskResult
        {
            Success = true,
            ResultData = JsonSerializer.Serialize(new { dataOperationCompleted = true })
        });
    }

    /// <summary>
    /// Execute ZIP code import from GeoNames or GitHub
    /// Configuration options in task InputData:
    /// - source: "GeoNames" or "GitHub" (default: "GeoNames")
    /// - countryCode: ISO 2-letter code (e.g., "US") or empty for all countries
    /// - gitHubUrl: Custom URL for GitHub import
    /// </summary>
    private async Task<TaskResult> ExecuteZipCodeImportAction(WorkflowTask task, CrmDbContext dbContext, CancellationToken ct)
    {
        _logger.LogInformation("Executing ZIP code import action for task {TaskId}", task.Id);

        try
        {
            // Parse configuration from task input data
            var config = new ZipCodeImportConfig();
            if (!string.IsNullOrEmpty(task.InputData))
            {
                try
                {
                    config = JsonSerializer.Deserialize<ZipCodeImportConfig>(task.InputData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new ZipCodeImportConfig();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse ZIP code import config, using defaults");
                }
            }

            // Get the import service
            using var scope = _serviceProvider.CreateScope();
            var importService = scope.ServiceProvider.GetService<IZipCodeImportService>();
            
            if (importService == null)
            {
                return new TaskResult
                {
                    Success = false,
                    ErrorMessage = "ZIP code import service is not configured"
                };
            }

            if (importService.IsImportRunning)
            {
                return new TaskResult
                {
                    Success = false,
                    ErrorMessage = "Another ZIP code import is already in progress"
                };
            }

            // Execute import based on configuration
            ZipCodeImportResult result;
            
            if (config.Source?.Equals("GitHub", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogInformation("Starting ZIP code import from GitHub: {Url}", config.GitHubUrl ?? "default");
                result = await importService.ImportFromGitHubAsync(config.GitHubUrl, ct);
            }
            else if (!string.IsNullOrEmpty(config.CountryCode))
            {
                _logger.LogInformation("Starting ZIP code import from GeoNames for {Country}", config.CountryCode);
                result = await importService.ImportCountryFromGeoNamesAsync(config.CountryCode, ct);
            }
            else
            {
                _logger.LogInformation("Starting ZIP code import from GeoNames (all countries)");
                result = await importService.ImportFromGeoNamesAsync(ct);
            }

            return new TaskResult
            {
                Success = result.Success,
                ResultData = JsonSerializer.Serialize(new
                {
                    result.RecordsImported,
                    result.RecordsSkipped,
                    result.RecordsFailed,
                    result.Source,
                    Duration = result.Duration.ToString(),
                    result.CompletedAt
                }),
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ZIP code import action");
            return new TaskResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private Task<TaskResult> ExecuteGenericAction(WorkflowTask task, CrmDbContext dbContext, CancellationToken ct)
    {
        _logger.LogInformation("Executing generic action for task {TaskId} of type {TaskType}", task.Id, task.TaskType);
        return Task.FromResult(new TaskResult
        {
            Success = true,
            ResultData = JsonSerializer.Serialize(new { genericActionCompleted = true, taskType = task.TaskType.ToString() })
        });
    }
}

/// <summary>
/// Result of a task execution
/// </summary>
public class TaskResult
{
    public bool Success { get; set; }
    public string? ResultData { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Configuration for ZIP code import workflow action
/// </summary>
public class ZipCodeImportConfig
{
    /// <summary>
    /// Import source: "GeoNames" or "GitHub"
    /// </summary>
    public string Source { get; set; } = "GeoNames";
    
    /// <summary>
    /// Country code for single-country import (e.g., "US", "CA")
    /// Leave empty to import all countries
    /// </summary>
    public string? CountryCode { get; set; }
    
    /// <summary>
    /// Custom GitHub URL for JSON ZIP code data
    /// </summary>
    public string? GitHubUrl { get; set; }
}
