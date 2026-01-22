using System.Diagnostics;
using System.Text.Json;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine;

/// <summary>
/// Main workflow engine implementation using State Machine pattern
/// </summary>
public class WorkflowEngineService : IWorkflowEngine
{
    private readonly IWorkflowDefinitionRepository _definitionRepo;
    private readonly IWorkflowInstanceRepository _instanceRepo;
    private readonly IWorkflowTaskRepository _taskRepo;
    private readonly IWorkflowEventRepository _eventRepo;
    private readonly IWorkflowJobQueue _jobQueue;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IEnumerable<IStepExecutor> _stepExecutors;
    private readonly ILogger<WorkflowEngineService> _logger;

    public WorkflowEngineService(
        IWorkflowDefinitionRepository definitionRepo,
        IWorkflowInstanceRepository instanceRepo,
        IWorkflowTaskRepository taskRepo,
        IWorkflowEventRepository eventRepo,
        IWorkflowJobQueue jobQueue,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IEnumerable<IStepExecutor> stepExecutors,
        ILogger<WorkflowEngineService> logger)
    {
        _definitionRepo = definitionRepo;
        _instanceRepo = instanceRepo;
        _taskRepo = taskRepo;
        _eventRepo = eventRepo;
        _jobQueue = jobQueue;
        _expressionEvaluator = expressionEvaluator;
        _stepExecutors = stepExecutors;
        _logger = logger;
    }

    public async Task<WorkflowInstanceDto> StartWorkflowAsync(
        StartWorkflowDto request, 
        int? userId = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting workflow {DefinitionId} for {EntityType}/{EntityId}", 
            request.WorkflowDefinitionId, request.EntityType, request.EntityId);

        var definition = await _definitionRepo.GetByIdAsync(request.WorkflowDefinitionId, true, cancellationToken);
        if (definition == null)
        {
            throw new InvalidOperationException($"Workflow definition {request.WorkflowDefinitionId} not found");
        }

        if (definition.Status != WorkflowDefinitionStatus.Published)
        {
            throw new InvalidOperationException($"Workflow {definition.Name} is not published");
        }

        var startStep = definition.Steps.FirstOrDefault(s => s.IsStartStep);
        if (startStep == null)
        {
            throw new InvalidOperationException($"Workflow {definition.Name} has no start step");
        }

        // Create instance
        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = definition.Id,
            WorkflowVersion = definition.Version,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            EntityReference = request.EntityReference,
            Status = WorkflowInstanceStatus.Running,
            CurrentStepKey = startStep.StepKey,
            Priority = request.Priority,
            StartedByUserId = userId,
            StartedAt = DateTime.UtcNow,
            DueAt = request.DueAt,
            LockVersion = 1
        };

        instance = await _instanceRepo.CreateAsync(instance, cancellationToken);

        // Set initial context variables
        if (request.InitialContext != null)
        {
            foreach (var (key, value) in request.InitialContext)
            {
                await SetContextVariableAsync(instance.Id, key, value, null, cancellationToken);
            }
        }

        // Log start event
        await LogEventAsync(instance.Id, WorkflowEventTypes.WorkflowStarted, null, 
            "User", userId?.ToString(), null, 
            $"Workflow '{definition.Name}' started", cancellationToken);

        // Queue the first step for execution
        await EnqueueStepExecutionAsync(instance.Id, startStep.StepKey, cancellationToken);

        return MapToDto(instance, definition);
    }

    public async Task<WorkflowInstanceDto> ProcessWorkflowAsync(
        int instanceId, 
        CancellationToken cancellationToken = default)
    {
        var instance = await _instanceRepo.GetByIdAsync(instanceId, true, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow instance {instanceId} not found");
        }

        if (instance.Status != WorkflowInstanceStatus.Running && 
            instance.Status != WorkflowInstanceStatus.WaitingForInput)
        {
            _logger.LogWarning("Cannot process instance {InstanceId} with status {Status}", 
                instanceId, instance.Status);
            return MapToDto(instance, instance.WorkflowDefinition);
        }

        var definition = await _definitionRepo.GetByIdAsync(instance.WorkflowDefinitionId, true, cancellationToken);
        if (definition == null)
        {
            throw new InvalidOperationException($"Workflow definition not found for instance {instanceId}");
        }

        var currentStep = definition.Steps.FirstOrDefault(s => s.StepKey == instance.CurrentStepKey);
        if (currentStep == null)
        {
            _logger.LogError("Current step {StepKey} not found in definition", instance.CurrentStepKey);
            await FailWorkflowAsync(instance, "Current step not found in definition", cancellationToken);
            return MapToDto(instance, definition);
        }

        // Execute the current step
        var result = await ExecuteStepAsync(instance, currentStep, definition, cancellationToken);

        // Handle result
        if (result.Success)
        {
            if (result.RequiresUserInput)
            {
                instance.Status = WorkflowInstanceStatus.WaitingForInput;
                await _instanceRepo.UpdateAsync(instance, cancellationToken);
            }
            else if (result.NextStepKey != null)
            {
                await TransitionToStepAsync(instance, result.NextStepKey, definition, cancellationToken);
            }
            else if (result.NextStepKeys?.Any() == true)
            {
                // Parallel execution - queue all next steps
                instance.ActiveStepKeys = string.Join(",", result.NextStepKeys);
                await _instanceRepo.UpdateAsync(instance, cancellationToken);
                
                foreach (var stepKey in result.NextStepKeys)
                {
                    await EnqueueStepExecutionAsync(instance.Id, stepKey, cancellationToken);
                }
            }
            else
            {
                // Check if current step is an end step
                if (currentStep.IsEndStep)
                {
                    await CompleteWorkflowAsync(instance, cancellationToken);
                }
            }

            // Set output variables
            if (result.OutputVariables != null)
            {
                foreach (var (key, value) in result.OutputVariables)
                {
                    await SetContextVariableAsync(instance.Id, key, value, currentStep.StepKey, cancellationToken);
                }
            }
        }
        else
        {
            if (result.ShouldRetry)
            {
                await ScheduleRetryAsync(instance, currentStep, result, cancellationToken);
            }
            else
            {
                await FailWorkflowAsync(instance, result.ErrorMessage ?? "Step execution failed", cancellationToken);
            }
        }

        return MapToDto(instance, definition);
    }

    public async Task<WorkflowInstanceDto> PauseWorkflowAsync(
        int instanceId, 
        int? userId = null, 
        string? reason = null, 
        CancellationToken cancellationToken = default)
    {
        var instance = await _instanceRepo.GetByIdAsync(instanceId, false, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow instance {instanceId} not found");
        }

        if (instance.Status != WorkflowInstanceStatus.Running && 
            instance.Status != WorkflowInstanceStatus.WaitingForInput)
        {
            throw new InvalidOperationException($"Cannot pause workflow in status {instance.Status}");
        }

        instance.Status = WorkflowInstanceStatus.Paused;
        instance.UpdatedAt = DateTime.UtcNow;
        await _instanceRepo.UpdateAsync(instance, cancellationToken);

        await LogEventAsync(instance.Id, WorkflowEventTypes.WorkflowPaused, null,
            "User", userId?.ToString(), null,
            reason ?? "Workflow paused", cancellationToken);

        var definition = await _definitionRepo.GetByIdAsync(instance.WorkflowDefinitionId, false, cancellationToken);
        return MapToDto(instance, definition!);
    }

    public async Task<WorkflowInstanceDto> ResumeWorkflowAsync(
        int instanceId, 
        int? userId = null, 
        CancellationToken cancellationToken = default)
    {
        var instance = await _instanceRepo.GetByIdAsync(instanceId, false, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow instance {instanceId} not found");
        }

        if (instance.Status != WorkflowInstanceStatus.Paused)
        {
            throw new InvalidOperationException($"Cannot resume workflow in status {instance.Status}");
        }

        instance.Status = WorkflowInstanceStatus.Running;
        instance.UpdatedAt = DateTime.UtcNow;
        await _instanceRepo.UpdateAsync(instance, cancellationToken);

        await LogEventAsync(instance.Id, WorkflowEventTypes.WorkflowResumed, null,
            "User", userId?.ToString(), null,
            "Workflow resumed", cancellationToken);

        // Queue the current step for execution
        if (instance.CurrentStepKey != null)
        {
            await EnqueueStepExecutionAsync(instance.Id, instance.CurrentStepKey, cancellationToken);
        }

        var definition = await _definitionRepo.GetByIdAsync(instance.WorkflowDefinitionId, false, cancellationToken);
        return MapToDto(instance, definition!);
    }

    public async Task<WorkflowInstanceDto> CancelWorkflowAsync(
        int instanceId, 
        int? userId = null, 
        string? reason = null, 
        CancellationToken cancellationToken = default)
    {
        var instance = await _instanceRepo.GetByIdAsync(instanceId, false, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow instance {instanceId} not found");
        }

        if (instance.Status == WorkflowInstanceStatus.Completed || 
            instance.Status == WorkflowInstanceStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot cancel workflow in status {instance.Status}");
        }

        instance.Status = WorkflowInstanceStatus.Cancelled;
        instance.CompletedAt = DateTime.UtcNow;
        instance.UpdatedAt = DateTime.UtcNow;
        await _instanceRepo.UpdateAsync(instance, cancellationToken);

        // Cancel any pending tasks
        var tasks = await _taskRepo.GetByInstanceAsync(instance.Id, cancellationToken);
        foreach (var task in tasks.Where(t => t.Status == WorkflowTaskStatus.Pending || 
                                              t.Status == WorkflowTaskStatus.InProgress))
        {
            task.Status = WorkflowTaskStatus.Cancelled;
            await _taskRepo.UpdateAsync(task, cancellationToken);
        }

        await LogEventAsync(instance.Id, WorkflowEventTypes.WorkflowCancelled, null,
            "User", userId?.ToString(), null,
            reason ?? "Workflow cancelled", cancellationToken);

        var definition = await _definitionRepo.GetByIdAsync(instance.WorkflowDefinitionId, false, cancellationToken);
        return MapToDto(instance, definition!);
    }

    public async Task<WorkflowInstanceDto> RetryWorkflowAsync(
        int instanceId, 
        int? userId = null, 
        CancellationToken cancellationToken = default)
    {
        var instance = await _instanceRepo.GetByIdAsync(instanceId, false, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow instance {instanceId} not found");
        }

        if (instance.Status != WorkflowInstanceStatus.Failed)
        {
            throw new InvalidOperationException($"Cannot retry workflow in status {instance.Status}");
        }

        instance.Status = WorkflowInstanceStatus.Running;
        instance.ErrorMessage = null;
        instance.RetryCount++;
        instance.UpdatedAt = DateTime.UtcNow;
        await _instanceRepo.UpdateAsync(instance, cancellationToken);

        await LogEventAsync(instance.Id, WorkflowEventTypes.WorkflowRetrying, null,
            "User", userId?.ToString(), null,
            $"Workflow retry attempt {instance.RetryCount}", cancellationToken);

        // Queue the current step for execution
        if (instance.CurrentStepKey != null)
        {
            await EnqueueStepExecutionAsync(instance.Id, instance.CurrentStepKey, cancellationToken);
        }

        var definition = await _definitionRepo.GetByIdAsync(instance.WorkflowDefinitionId, false, cancellationToken);
        return MapToDto(instance, definition!);
    }

    public async Task<WorkflowInstanceDetailDto?> GetInstanceDetailAsync(
        int instanceId, 
        CancellationToken cancellationToken = default)
    {
        var instance = await _instanceRepo.GetByIdAsync(instanceId, true, cancellationToken);
        if (instance == null) return null;

        var definition = await _definitionRepo.GetByIdAsync(instance.WorkflowDefinitionId, true, cancellationToken);
        if (definition == null) return null;

        var events = await _eventRepo.GetByInstanceAsync(instanceId, cancellationToken);
        var tasks = await _taskRepo.GetByInstanceAsync(instanceId, cancellationToken);

        var dto = new WorkflowInstanceDetailDto
        {
            Id = instance.Id,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            WorkflowName = definition.Name,
            WorkflowVersion = instance.WorkflowVersion,
            EntityType = instance.EntityType,
            EntityId = instance.EntityId,
            EntityReference = instance.EntityReference,
            Status = instance.Status,
            CurrentStepKey = instance.CurrentStepKey,
            CurrentStepName = definition.Steps.FirstOrDefault(s => s.StepKey == instance.CurrentStepKey)?.Name,
            Priority = instance.Priority,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            DueAt = instance.DueAt,
            ErrorMessage = instance.ErrorMessage,
            StartedByUserId = instance.StartedByUserId,
            StartedByUserName = instance.StartedByUser?.Email,
            CreatedAt = instance.CreatedAt,
            DurationMinutes = instance.CompletedAt.HasValue 
                ? (instance.CompletedAt.Value - instance.StartedAt!.Value).TotalMinutes 
                : null,
            Events = events.Select(MapEventToDto).ToList(),
            Tasks = tasks.Select(t => MapTaskToDto(t, definition.Name)).ToList(),
            Steps = definition.Steps.Select(MapStepToDto).ToList()
        };

        // Get context variables
        foreach (var variable in instance.ContextVariables)
        {
            dto.ContextVariables[variable.Key] = DeserializeValue(variable.Value, variable.ValueType);
        }

        return dto;
    }

    public async Task TriggerWorkflowsAsync(
        string entityType, 
        int entityId, 
        string eventType, 
        object? entityData = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Triggering workflows for {EntityType}/{EntityId} event {EventType}",
            entityType, entityId, eventType);

        var definitions = await _definitionRepo.GetByTriggerAsync(entityType, eventType, cancellationToken);

        foreach (var definition in definitions.OrderByDescending(d => d.Priority))
        {
            try
            {
                var request = new StartWorkflowDto
                {
                    WorkflowDefinitionId = definition.Id,
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityReference = $"{entityType}-{entityId}",
                    Priority = WorkflowPriority.Normal,
                    InitialContext = new Dictionary<string, object?>
                    {
                        ["entityType"] = entityType,
                        ["entityId"] = entityId,
                        ["triggerEvent"] = eventType,
                        ["entityData"] = entityData
                    }
                };

                await StartWorkflowAsync(request, null, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger workflow {WorkflowId} for {EntityType}/{EntityId}",
                    definition.Id, entityType, entityId);
            }
        }
    }

    #region Private Methods

    private async Task<StepExecutionResult> ExecuteStepAsync(
        WorkflowInstance instance,
        WorkflowStep step,
        WorkflowDefinition definition,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        await LogEventAsync(instance.Id, WorkflowEventTypes.StepStarted, step.StepKey,
            "System", null, null, $"Starting step '{step.Name}'", cancellationToken);

        try
        {
            // Find appropriate executor (Strategy Pattern)
            var executor = _stepExecutors.FirstOrDefault(e => e.SupportedStepTypes.Contains(step.StepType));
            if (executor == null)
            {
                return new StepExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"No executor found for step type '{step.StepType}'",
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }

            // Build execution context
            var variables = await GetContextVariablesAsync(instance.Id, cancellationToken);
            var context = new StepExecutionContext
            {
                Instance = instance,
                Step = step,
                Definition = definition,
                Variables = variables,
                CorrelationId = Guid.NewGuid().ToString()
            };

            // Execute step
            var result = await executor.ExecuteAsync(context, cancellationToken);
            result.DurationMs = stopwatch.ElapsedMilliseconds;

            if (result.Success)
            {
                await LogEventAsync(instance.Id, WorkflowEventTypes.StepCompleted, step.StepKey,
                    "System", null, null, $"Step '{step.Name}' completed",
                    cancellationToken, result.DurationMs);
            }
            else
            {
                await LogEventAsync(instance.Id, WorkflowEventTypes.StepFailed, step.StepKey,
                    "System", null, null, $"Step '{step.Name}' failed: {result.ErrorMessage}",
                    cancellationToken, result.DurationMs, result.ErrorDetails);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing step {StepKey} in instance {InstanceId}", 
                step.StepKey, instance.Id);

            return new StepExecutionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorDetails = ex.StackTrace,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private async Task TransitionToStepAsync(
        WorkflowInstance instance,
        string nextStepKey,
        WorkflowDefinition definition,
        CancellationToken cancellationToken)
    {
        var nextStep = definition.Steps.FirstOrDefault(s => s.StepKey == nextStepKey);
        if (nextStep == null)
        {
            await FailWorkflowAsync(instance, $"Next step '{nextStepKey}' not found", cancellationToken);
            return;
        }

        instance.CurrentStepKey = nextStepKey;
        instance.UpdatedAt = DateTime.UtcNow;
        await _instanceRepo.UpdateAsync(instance, cancellationToken);

        if (nextStep.IsEndStep)
        {
            await CompleteWorkflowAsync(instance, cancellationToken);
        }
        else
        {
            await EnqueueStepExecutionAsync(instance.Id, nextStepKey, cancellationToken);
        }
    }

    private async Task CompleteWorkflowAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        instance.Status = WorkflowInstanceStatus.Completed;
        instance.CompletedAt = DateTime.UtcNow;
        instance.UpdatedAt = DateTime.UtcNow;
        await _instanceRepo.UpdateAsync(instance, cancellationToken);

        await LogEventAsync(instance.Id, WorkflowEventTypes.WorkflowCompleted, null,
            "System", null, null, "Workflow completed successfully", cancellationToken);
    }

    private async Task FailWorkflowAsync(WorkflowInstance instance, string errorMessage, CancellationToken cancellationToken)
    {
        instance.Status = WorkflowInstanceStatus.Failed;
        instance.ErrorMessage = errorMessage;
        instance.CompletedAt = DateTime.UtcNow;
        instance.UpdatedAt = DateTime.UtcNow;
        await _instanceRepo.UpdateAsync(instance, cancellationToken);

        await LogEventAsync(instance.Id, WorkflowEventTypes.WorkflowFailed, null,
            "System", null, null, errorMessage, cancellationToken);
    }

    private async Task ScheduleRetryAsync(
        WorkflowInstance instance,
        WorkflowStep step,
        StepExecutionResult result,
        CancellationToken cancellationToken)
    {
        instance.RetryCount++;
        instance.NextRetryAt = result.RetryAfter ?? DateTime.UtcNow.AddMinutes(Math.Pow(2, instance.RetryCount));
        instance.UpdatedAt = DateTime.UtcNow;
        await _instanceRepo.UpdateAsync(instance, cancellationToken);

        await LogEventAsync(instance.Id, WorkflowEventTypes.StepRetrying, step.StepKey,
            "System", null, null, 
            $"Scheduling retry {instance.RetryCount} at {instance.NextRetryAt}", 
            cancellationToken);

        // Enqueue job for retry
        var job = new WorkflowJob
        {
            JobType = WorkflowJobTypes.ExecuteStep,
            Status = WorkflowJobStatus.Pending,
            WorkflowInstanceId = instance.Id,
            StepKey = step.StepKey,
            ScheduledAt = instance.NextRetryAt.Value,
            MaxAttempts = 1,
            Priority = GetPriorityValue(instance.Priority)
        };

        await _jobQueue.EnqueueAsync(job, cancellationToken);
    }

    private async Task EnqueueStepExecutionAsync(int instanceId, string stepKey, CancellationToken cancellationToken)
    {
        var job = new WorkflowJob
        {
            JobType = WorkflowJobTypes.ExecuteStep,
            Status = WorkflowJobStatus.Pending,
            WorkflowInstanceId = instanceId,
            StepKey = stepKey,
            ScheduledAt = DateTime.UtcNow,
            MaxAttempts = 3,
            CorrelationId = Guid.NewGuid().ToString()
        };

        await _jobQueue.EnqueueAsync(job, cancellationToken);
    }

    private async Task SetContextVariableAsync(
        int instanceId, 
        string key, 
        object? value, 
        string? stepKey,
        CancellationToken cancellationToken)
    {
        var instance = await _instanceRepo.GetByIdAsync(instanceId, true, cancellationToken);
        if (instance == null) return;

        var existing = instance.ContextVariables.FirstOrDefault(v => v.Key == key);
        var (serializedValue, valueType) = SerializeValue(value);

        if (existing != null)
        {
            existing.Value = serializedValue;
            existing.ValueType = valueType;
            existing.SetByStepKey = stepKey;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            instance.ContextVariables.Add(new WorkflowContextVariable
            {
                WorkflowInstanceId = instanceId,
                Key = key,
                Value = serializedValue,
                ValueType = valueType,
                SetByStepKey = stepKey
            });
        }

        await _instanceRepo.UpdateAsync(instance, cancellationToken);
    }

    private async Task<Dictionary<string, object?>> GetContextVariablesAsync(
        int instanceId, 
        CancellationToken cancellationToken)
    {
        var instance = await _instanceRepo.GetByIdAsync(instanceId, true, cancellationToken);
        if (instance == null) return new Dictionary<string, object?>();

        var variables = new Dictionary<string, object?>();
        foreach (var v in instance.ContextVariables)
        {
            variables[v.Key] = DeserializeValue(v.Value, v.ValueType);
        }
        return variables;
    }

    private async Task LogEventAsync(
        int instanceId,
        string eventType,
        string? stepKey,
        string actorType,
        string? actorId,
        string? actorName,
        string message,
        CancellationToken cancellationToken,
        long? durationMs = null,
        string? errorDetails = null)
    {
        var @event = new WorkflowEvent
        {
            WorkflowInstanceId = instanceId,
            EventType = eventType,
            StepKey = stepKey,
            Timestamp = DateTime.UtcNow,
            ActorType = actorType,
            ActorId = actorId,
            ActorName = actorName,
            Message = message,
            DurationMs = durationMs,
            ErrorDetails = errorDetails,
            Severity = errorDetails != null ? "Error" : "Info"
        };

        await _eventRepo.LogEventAsync(@event, cancellationToken);
    }

    private static (string? value, string type) SerializeValue(object? value)
    {
        if (value == null) return (null, WorkflowVariableTypes.String);

        return value switch
        {
            string s => (s, WorkflowVariableTypes.String),
            int or long or double or decimal => (value.ToString(), WorkflowVariableTypes.Number),
            bool b => (b.ToString().ToLower(), WorkflowVariableTypes.Boolean),
            DateTime dt => (dt.ToString("O"), WorkflowVariableTypes.DateTime),
            _ => (JsonSerializer.Serialize(value), WorkflowVariableTypes.Object)
        };
    }

    private static object? DeserializeValue(string? value, string valueType)
    {
        if (value == null) return null;

        return valueType switch
        {
            WorkflowVariableTypes.String => value,
            WorkflowVariableTypes.Number => double.TryParse(value, out var d) ? d : 0,
            WorkflowVariableTypes.Boolean => bool.TryParse(value, out var b) && b,
            WorkflowVariableTypes.DateTime => DateTime.TryParse(value, out var dt) ? dt : DateTime.MinValue,
            WorkflowVariableTypes.Object or WorkflowVariableTypes.Array => 
                JsonSerializer.Deserialize<object>(value),
            _ => value
        };
    }

    private static int GetPriorityValue(string priority) => priority switch
    {
        WorkflowPriority.Critical => 100,
        WorkflowPriority.High => 75,
        WorkflowPriority.Normal => 50,
        WorkflowPriority.Low => 25,
        _ => 50
    };

    private static WorkflowInstanceDto MapToDto(WorkflowInstance instance, WorkflowDefinition definition)
    {
        return new WorkflowInstanceDto
        {
            Id = instance.Id,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            WorkflowName = definition.Name,
            WorkflowVersion = instance.WorkflowVersion,
            EntityType = instance.EntityType,
            EntityId = instance.EntityId,
            EntityReference = instance.EntityReference,
            Status = instance.Status,
            CurrentStepKey = instance.CurrentStepKey,
            CurrentStepName = definition.Steps.FirstOrDefault(s => s.StepKey == instance.CurrentStepKey)?.Name,
            Priority = instance.Priority,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            DueAt = instance.DueAt,
            ErrorMessage = instance.ErrorMessage,
            StartedByUserId = instance.StartedByUserId,
            StartedByUserName = instance.StartedByUser?.Email,
            CreatedAt = instance.CreatedAt,
            DurationMinutes = instance.CompletedAt.HasValue && instance.StartedAt.HasValue
                ? (instance.CompletedAt.Value - instance.StartedAt.Value).TotalMinutes
                : null
        };
    }

    private static WorkflowEventDto MapEventToDto(WorkflowEvent e)
    {
        return new WorkflowEventDto
        {
            Id = e.Id,
            EventType = e.EventType,
            StepKey = e.StepKey,
            Timestamp = e.Timestamp,
            ActorType = e.ActorType,
            ActorId = e.ActorId,
            ActorName = e.ActorName,
            Message = e.Message,
            Severity = e.Severity,
            DurationMs = e.DurationMs,
            ErrorDetails = e.ErrorDetails
        };
    }

    private static WorkflowTaskDto MapTaskToDto(WorkflowTask t, string workflowName)
    {
        return new WorkflowTaskDto
        {
            Id = t.Id,
            WorkflowInstanceId = t.WorkflowInstanceId,
            WorkflowName = workflowName,
            StepKey = t.StepKey,
            Title = t.Title,
            Description = t.Description,
            Instructions = t.Instructions,
            Status = t.Status,
            AssignmentType = t.AssignmentType,
            AssignedToUserId = t.AssignedToUserId,
            AssignedToUserName = t.AssignedToUser?.Email,
            AssignedToGroupId = t.AssignedToGroupId,
            AssignedToGroupName = t.AssignedToGroup?.Name,
            AssignedToRole = t.AssignedToRole,
            Priority = t.Priority,
            DueAt = t.DueAt,
            ClaimedAt = t.ClaimedAt,
            ClaimedByUserId = t.ClaimedByUserId,
            ClaimedByUserName = t.ClaimedByUser?.Email,
            CompletedAt = t.CompletedAt,
            ActionTaken = t.ActionTaken,
            FormSchema = t.FormSchema,
            FormData = t.FormData,
            AvailableActions = t.AvailableActions,
            CreatedAt = t.CreatedAt
        };
    }

    private static WorkflowStepDto MapStepToDto(WorkflowStep s)
    {
        return new WorkflowStepDto
        {
            Id = s.Id,
            StepKey = s.StepKey,
            Name = s.Name,
            Description = s.Description,
            StepType = s.StepType,
            OrderIndex = s.OrderIndex,
            Configuration = s.Configuration,
            Transitions = s.Transitions,
            TimeoutMinutes = s.TimeoutMinutes,
            RetryPolicy = s.RetryPolicy,
            IsStartStep = s.IsStartStep,
            IsEndStep = s.IsEndStep,
            PositionX = s.PositionX,
            PositionY = s.PositionY
        };
    }

    #endregion
}
