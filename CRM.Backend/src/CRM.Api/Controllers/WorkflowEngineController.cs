using System.Text.Json;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for the workflow engine
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkflowEngineController : ControllerBase
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IWorkflowTaskRepository _taskRepository;
    private readonly IWorkflowEventRepository _eventRepository;
    private readonly ILogger<WorkflowEngineController> _logger;

    public WorkflowEngineController(
        IWorkflowEngine workflowEngine,
        IWorkflowDefinitionRepository definitionRepository,
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowTaskRepository taskRepository,
        IWorkflowEventRepository eventRepository,
        ILogger<WorkflowEngineController> logger)
    {
        _workflowEngine = workflowEngine;
        _definitionRepository = definitionRepository;
        _instanceRepository = instanceRepository;
        _taskRepository = taskRepository;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    #region Workflow Definitions

    /// <summary>
    /// Get all workflow definitions
    /// </summary>
    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinitionDto>>> GetDefinitions(
        [FromQuery] string? status = null,
        [FromQuery] string? triggerType = null)
    {
        var definitions = await _definitionRepository.GetAllAsync(status, triggerType);
        var dtos = definitions.Select(MapToDto);
        return Ok(dtos);
    }

    /// <summary>
    /// Get a workflow definition by ID
    /// </summary>
    [HttpGet("definitions/{id}")]
    public async Task<ActionResult<WorkflowDefinitionDetailDto>> GetDefinition(int id)
    {
        var definition = await _definitionRepository.GetByIdAsync(id, includeSteps: true);
        if (definition == null)
        {
            return NotFound();
        }

        return Ok(MapToDetailDto(definition));
    }

    /// <summary>
    /// Create a new workflow definition
    /// </summary>
    [HttpPost("definitions")]
    public async Task<ActionResult<WorkflowDefinitionDto>> CreateDefinition(
        [FromBody] CreateWorkflowDefinitionDto dto)
    {
        var userId = GetCurrentUserId();

        var definition = new WorkflowDefinition
        {
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            Version = "1.0.0",
            VersionNumber = 1,
            TriggerType = dto.TriggerType,
            TriggerEntityType = dto.TriggerEntityType,
            TriggerEvents = dto.TriggerEvents,
            ScheduleCron = dto.ScheduleCron,
            Status = WorkflowDefinitionStatus.Draft,
            Priority = dto.Priority,
            ErrorHandlingConfig = dto.ErrorHandlingConfig,
            NotificationConfig = dto.NotificationConfig,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add steps
        if (dto.Steps != null)
        {
            foreach (var stepDto in dto.Steps)
            {
                definition.Steps.Add(new WorkflowStep
                {
                    StepKey = stepDto.StepKey,
                    Name = stepDto.Name,
                    Description = stepDto.Description,
                    StepType = stepDto.StepType,
                    OrderIndex = stepDto.OrderIndex,
                    Configuration = stepDto.Configuration,
                    Transitions = stepDto.Transitions,
                    TimeoutMinutes = stepDto.TimeoutMinutes,
                    RetryPolicy = stepDto.RetryPolicy,
                    IsStartStep = stepDto.IsStartStep,
                    IsEndStep = stepDto.IsEndStep,
                    PositionX = stepDto.PositionX,
                    PositionY = stepDto.PositionY,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        var created = await _definitionRepository.CreateAsync(definition);
        
        _logger.LogInformation("Created workflow definition {Id}: {Name}", created.Id, created.Name);
        
        return CreatedAtAction(nameof(GetDefinition), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>
    /// Update a workflow definition
    /// </summary>
    [HttpPut("definitions/{id}")]
    public async Task<ActionResult<WorkflowDefinitionDto>> UpdateDefinition(
        int id, 
        [FromBody] UpdateWorkflowDefinitionDto dto)
    {
        var definition = await _definitionRepository.GetByIdAsync(id, includeSteps: true);
        if (definition == null)
        {
            return NotFound();
        }

        if (definition.Status == WorkflowDefinitionStatus.Published)
        {
            return BadRequest("Cannot modify a published workflow. Create a new version instead.");
        }

        definition.Name = dto.Name;
        definition.Description = dto.Description ?? string.Empty;
        definition.TriggerType = dto.TriggerType;
        definition.TriggerEntityType = dto.TriggerEntityType;
        definition.TriggerEvents = dto.TriggerEvents;
        definition.ScheduleCron = dto.ScheduleCron;
        definition.Priority = dto.Priority;
        definition.ErrorHandlingConfig = dto.ErrorHandlingConfig;
        definition.NotificationConfig = dto.NotificationConfig;
        definition.UpdatedAt = DateTime.UtcNow;

        // Update steps
        if (dto.Steps != null)
        {
            definition.Steps.Clear();
            foreach (var stepDto in dto.Steps)
            {
                definition.Steps.Add(new WorkflowStep
                {
                    StepKey = stepDto.StepKey,
                    Name = stepDto.Name,
                    Description = stepDto.Description,
                    StepType = stepDto.StepType,
                    OrderIndex = stepDto.OrderIndex,
                    Configuration = stepDto.Configuration,
                    Transitions = stepDto.Transitions,
                    TimeoutMinutes = stepDto.TimeoutMinutes,
                    RetryPolicy = stepDto.RetryPolicy,
                    IsStartStep = stepDto.IsStartStep,
                    IsEndStep = stepDto.IsEndStep,
                    PositionX = stepDto.PositionX,
                    PositionY = stepDto.PositionY,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        var updated = await _definitionRepository.UpdateAsync(definition);
        
        _logger.LogInformation("Updated workflow definition {Id}", id);
        
        return Ok(MapToDto(updated));
    }

    /// <summary>
    /// Publish a workflow definition
    /// </summary>
    [HttpPost("definitions/{id}/publish")]
    public async Task<ActionResult<WorkflowDefinitionDto>> PublishDefinition(int id)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var published = await _definitionRepository.PublishAsync(id, userId);
            _logger.LogInformation("Published workflow definition {Id}", id);
            return Ok(MapToDto(published));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a workflow definition
    /// </summary>
    [HttpDelete("definitions/{id}")]
    public async Task<ActionResult> DeleteDefinition(int id)
    {
        var definition = await _definitionRepository.GetByIdAsync(id);
        if (definition == null)
        {
            return NotFound();
        }

        await _definitionRepository.DeleteAsync(id);
        
        _logger.LogInformation("Deleted workflow definition {Id}", id);
        
        return NoContent();
    }

    #endregion

    #region Workflow Instances

    /// <summary>
    /// Get workflow instances
    /// </summary>
    [HttpGet("instances")]
    public async Task<ActionResult<IEnumerable<WorkflowInstanceDto>>> GetInstances(
        [FromQuery] int? limit = 100)
    {
        var instances = await _instanceRepository.GetActiveInstancesAsync(limit);
        var dtos = instances.Select(MapInstanceToDto);
        return Ok(dtos);
    }

    /// <summary>
    /// Get a workflow instance by ID
    /// </summary>
    [HttpGet("instances/{id}")]
    public async Task<ActionResult<WorkflowInstanceDetailDto>> GetInstance(int id)
    {
        var detailDto = await _workflowEngine.GetInstanceDetailAsync(id);
        if (detailDto == null)
        {
            return NotFound();
        }

        return Ok(detailDto);
    }

    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    [HttpPost("instances/start")]
    public async Task<ActionResult<WorkflowInstanceDto>> StartWorkflow([FromBody] StartWorkflowDto dto)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var instance = await _workflowEngine.StartWorkflowAsync(dto, userId);
            _logger.LogInformation("Started workflow instance {Id} from definition {DefId}", 
                instance.Id, dto.WorkflowDefinitionId);
            return CreatedAtAction(nameof(GetInstance), new { id = instance.Id }, instance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workflow");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Pause a running workflow instance
    /// </summary>
    [HttpPost("instances/{id}/pause")]
    public async Task<ActionResult<WorkflowInstanceDto>> PauseWorkflow(int id, [FromBody] WorkflowActionDto? dto = null)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var instance = await _workflowEngine.PauseWorkflowAsync(id, userId, dto?.Reason);
            return Ok(instance);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Resume a paused workflow instance
    /// </summary>
    [HttpPost("instances/{id}/resume")]
    public async Task<ActionResult<WorkflowInstanceDto>> ResumeWorkflow(int id)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var instance = await _workflowEngine.ResumeWorkflowAsync(id, userId);
            return Ok(instance);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cancel a workflow instance
    /// </summary>
    [HttpPost("instances/{id}/cancel")]
    public async Task<ActionResult<WorkflowInstanceDto>> CancelWorkflow(int id, [FromBody] WorkflowActionDto? dto = null)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var instance = await _workflowEngine.CancelWorkflowAsync(id, userId, dto?.Reason);
            return Ok(instance);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retry a failed workflow instance
    /// </summary>
    [HttpPost("instances/{id}/retry")]
    public async Task<ActionResult<WorkflowInstanceDto>> RetryWorkflow(int id)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var instance = await _workflowEngine.RetryWorkflowAsync(id, userId);
            return Ok(instance);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get events for a workflow instance
    /// </summary>
    [HttpGet("instances/{id}/events")]
    public async Task<ActionResult<IEnumerable<WorkflowEventDto>>> GetInstanceEvents(int id)
    {
        var events = await _eventRepository.GetByInstanceAsync(id);
        var dtos = events.Select(MapEventToDto);
        return Ok(dtos);
    }

    #endregion

    #region Workflow Tasks

    /// <summary>
    /// Get pending tasks for the current user
    /// </summary>
    [HttpGet("tasks")]
    public async Task<ActionResult<IEnumerable<WorkflowTaskDto>>> GetTasks(
        [FromQuery] int? userId = null,
        [FromQuery] int? groupId = null,
        [FromQuery] string? role = null)
    {
        var currentUserId = GetCurrentUserId();
        var tasks = await _taskRepository.GetPendingTasksAsync(userId ?? currentUserId, groupId, role);
        var dtos = tasks.Select(MapTaskToDto);
        return Ok(dtos);
    }

    /// <summary>
    /// Get a task by ID
    /// </summary>
    [HttpGet("tasks/{id}")]
    public async Task<ActionResult<WorkflowTaskDto>> GetTask(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        return Ok(MapTaskToDto(task));
    }

    /// <summary>
    /// Complete a task
    /// </summary>
    [HttpPost("tasks/{id}/complete")]
    public async Task<ActionResult<WorkflowTaskDto>> CompleteTask(int id, [FromBody] CompleteTaskDto dto)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var task = await _taskRepository.CompleteAsync(id, dto, userId);
            
            // Continue workflow processing
            if (task.WorkflowInstanceId > 0)
            {
                await _workflowEngine.ProcessWorkflowAsync(task.WorkflowInstanceId);
            }
            
            _logger.LogInformation("Completed task {TaskId} with action {Action}", id, dto.ActionTaken);
            
            return Ok(MapTaskToDto(task));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get tasks for a workflow instance
    /// </summary>
    [HttpGet("instances/{instanceId}/tasks")]
    public async Task<ActionResult<IEnumerable<WorkflowTaskDto>>> GetInstanceTasks(int instanceId)
    {
        var tasks = await _taskRepository.GetByInstanceAsync(instanceId);
        var dtos = tasks.Select(MapTaskToDto);
        return Ok(dtos);
    }

    #endregion

    #region Analytics

    /// <summary>
    /// Get workflow analytics
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<WorkflowAnalyticsDto>> GetAnalytics()
    {
        var activeInstances = await _instanceRepository.GetActiveInstancesAsync();
        var pendingTasks = await _taskRepository.GetPendingTasksAsync();
        var recentEvents = await _eventRepository.QueryEventsAsync(
            from: DateTime.UtcNow.AddDays(-1),
            limit: 20);

        var analytics = new WorkflowAnalyticsDto
        {
            ActiveWorkflows = activeInstances.Count,
            PendingTasks = pendingTasks.Count,
            CompletedToday = activeInstances.Count(i => 
                i.Status == WorkflowInstanceStatus.Completed && 
                i.CompletedAt?.Date == DateTime.UtcNow.Date),
            FailedOrStalled = activeInstances.Count(i => 
                i.Status == WorkflowInstanceStatus.Failed),
            RecentActivity = recentEvents.Select(MapEventToDto).ToList(),
            StatusDistribution = activeInstances
                .GroupBy(i => i.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return Ok(analytics);
    }

    #endregion

    #region Private Helpers

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
    }

    private WorkflowDefinitionDto MapToDto(WorkflowDefinition definition)
    {
        return new WorkflowDefinitionDto
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            Version = definition.Version,
            TriggerType = definition.TriggerType,
            TriggerEntityType = definition.TriggerEntityType,
            TriggerEvents = definition.TriggerEvents,
            Status = definition.Status,
            Priority = definition.Priority,
            PublishedAt = definition.PublishedAt,
            CreatedAt = definition.CreatedAt,
            UpdatedAt = definition.UpdatedAt,
            CreatedByUserName = definition.CreatedByUser != null ? $"{definition.CreatedByUser.FirstName} {definition.CreatedByUser.LastName}".Trim() : null,
            StepCount = definition.Steps?.Count ?? 0,
            ActiveInstanceCount = definition.Instances?.Count(i => 
                i.Status != WorkflowInstanceStatus.Completed && 
                i.Status != WorkflowInstanceStatus.Cancelled) ?? 0
        };
    }

    private WorkflowDefinitionDetailDto MapToDetailDto(WorkflowDefinition definition)
    {
        return new WorkflowDefinitionDetailDto
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            Version = definition.Version,
            VersionNumber = definition.VersionNumber,
            TriggerType = definition.TriggerType,
            TriggerEntityType = definition.TriggerEntityType,
            TriggerEvents = definition.TriggerEvents,
            ScheduleCron = definition.ScheduleCron,
            Status = definition.Status,
            Priority = definition.Priority,
            ErrorHandlingConfig = definition.ErrorHandlingConfig,
            NotificationConfig = definition.NotificationConfig,
            PublishedAt = definition.PublishedAt,
            CreatedAt = definition.CreatedAt,
            UpdatedAt = definition.UpdatedAt,
            CreatedByUserId = definition.CreatedByUserId,
            CreatedByUserName = definition.CreatedByUser != null ? $"{definition.CreatedByUser.FirstName} {definition.CreatedByUser.LastName}".Trim() : null,
            Steps = definition.Steps?.Select(MapStepToDto).ToList() ?? new List<WorkflowStepDto>()
        };
    }

    private WorkflowStepDto MapStepToDto(WorkflowStep step)
    {
        return new WorkflowStepDto
        {
            Id = step.Id,
            StepKey = step.StepKey,
            Name = step.Name,
            Description = step.Description,
            StepType = step.StepType,
            OrderIndex = step.OrderIndex,
            Configuration = step.Configuration,
            Transitions = step.Transitions,
            TimeoutMinutes = step.TimeoutMinutes,
            RetryPolicy = step.RetryPolicy,
            IsStartStep = step.IsStartStep,
            IsEndStep = step.IsEndStep,
            PositionX = step.PositionX,
            PositionY = step.PositionY
        };
    }

    private WorkflowInstanceDto MapInstanceToDto(WorkflowInstance instance)
    {
        return new WorkflowInstanceDto
        {
            Id = instance.Id,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            WorkflowName = instance.WorkflowDefinition?.Name ?? string.Empty,
            WorkflowVersion = instance.WorkflowVersion,
            EntityType = instance.EntityType,
            EntityId = instance.EntityId,
            EntityReference = instance.EntityReference,
            Status = instance.Status,
            CurrentStepKey = instance.CurrentStepKey,
            Priority = instance.Priority,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            DueAt = instance.DueAt,
            ErrorMessage = instance.ErrorMessage,
            StartedByUserId = instance.StartedByUserId,
            StartedByUserName = instance.StartedByUser != null ? $"{instance.StartedByUser.FirstName} {instance.StartedByUser.LastName}".Trim() : null,
            CreatedAt = instance.CreatedAt,
            DurationMinutes = instance.CompletedAt.HasValue && instance.StartedAt.HasValue
                ? (instance.CompletedAt.Value - instance.StartedAt.Value).TotalMinutes
                : null
        };
    }

    private WorkflowEventDto MapEventToDto(WorkflowEvent evt)
    {
        return new WorkflowEventDto
        {
            Id = evt.Id,
            EventType = evt.EventType,
            StepKey = evt.StepKey,
            Timestamp = evt.Timestamp,
            ActorType = evt.ActorType,
            ActorId = evt.ActorId,
            ActorName = evt.ActorName,
            Message = evt.Message,
            Severity = evt.Severity,
            DurationMs = evt.DurationMs,
            ErrorDetails = evt.ErrorDetails
        };
    }

    private WorkflowTaskDto MapTaskToDto(WorkflowTask task)
    {
        return new WorkflowTaskDto
        {
            Id = task.Id,
            WorkflowInstanceId = task.WorkflowInstanceId,
            WorkflowName = task.WorkflowInstance?.WorkflowDefinition?.Name ?? string.Empty,
            StepKey = task.StepKey,
            Title = task.Title,
            Description = task.Description,
            Instructions = task.Instructions,
            Status = task.Status,
            AssignmentType = task.AssignmentType,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToUserName = task.AssignedToUser != null ? $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}".Trim() : null,
            AssignedToGroupId = task.AssignedToGroupId,
            AssignedToRole = task.AssignedToRole,
            Priority = task.Priority,
            DueAt = task.DueAt,
            ClaimedAt = task.ClaimedAt,
            ClaimedByUserId = task.ClaimedByUserId,
            CompletedAt = task.CompletedAt,
            ActionTaken = task.ActionTaken,
            FormSchema = task.FormSchema,
            FormData = task.FormData,
            AvailableActions = task.AvailableActions,
            CreatedAt = task.CreatedAt,
            EntityType = task.WorkflowInstance?.EntityType,
            EntityId = task.WorkflowInstance?.EntityId,
            EntityReference = task.WorkflowInstance?.EntityReference
        };
    }

    #endregion
}

/// <summary>
/// DTO for workflow actions (pause, cancel)
/// </summary>
public class WorkflowActionDto
{
    public string? Reason { get; set; }
}
