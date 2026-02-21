// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.Workflow;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRM.ServiceDeskService.Controllers;

/// <summary>
/// Controller for managing workflow instances and executions
/// Routes to /api/workflowengine
/// </summary>
[ApiController]
[Route("api/workflowengine")]
[Authorize]
public class WorkflowEngineController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly WorkflowInstanceService _instanceService;
    private readonly ILogger<WorkflowEngineController> _logger;

    public WorkflowEngineController(
        CrmDbContext context,
        WorkflowInstanceService instanceService,
        ILogger<WorkflowEngineController> logger)
    {
        _context = context;
        _instanceService = instanceService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    #region Workflow Instances

    /// <summary>
    /// Get all workflow instances with filtering
    /// </summary>
    [HttpGet("instances")]
    public async Task<IActionResult> GetInstances(
        [FromQuery] int? workflowDefinitionId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int? entityId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        try
        {
            WorkflowInstanceStatus? statusFilter = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkflowInstanceStatus>(status, out var s))
                statusFilter = s;

            var instances = await _instanceService.GetInstancesAsync(
                workflowDefinitionId, entityType, entityId, statusFilter,
                startDateFrom, startDateTo, skip, take);

            var result = instances.Select(i => new WorkflowInstanceDto
            {
                Id = i.Id,
                CorrelationId = i.CorrelationId,
                WorkflowDefinitionId = i.WorkflowDefinitionId,
                WorkflowName = i.WorkflowDefinition?.Name ?? "Unknown",
                WorkflowVersionId = i.WorkflowVersionId,
                VersionNumber = i.WorkflowVersion?.VersionNumber ?? 0,
                EntityType = i.EntityType,
                EntityId = i.EntityId,
                Status = i.Status.ToString(),
                Priority = i.Priority,
                StartedAt = i.StartedAt,
                CompletedAt = i.CompletedAt,
                CurrentNodeId = i.CurrentNodeId,
                CurrentNodeName = i.CurrentNode?.Name,
                TriggeredByName = i.TriggeredBy != null
                    ? $"{i.TriggeredBy.FirstName} {i.TriggeredBy.LastName}"
                    : "System",
                ErrorMessage = i.ErrorMessage,
                IsCancelled = i.IsCancelled
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow instances");
            return StatusCode(500, new { message = "An error occurred while retrieving instances" });
        }
    }

    /// <summary>
    /// Get a specific workflow instance
    /// </summary>
    [HttpGet("instances/{id}")]
    public async Task<IActionResult> GetInstance(int id)
    {
        try
        {
            var instance = await _instanceService.GetInstanceAsync(id);
            if (instance == null) return NotFound(new { message = "Instance not found" });

            var result = new WorkflowInstanceDetailDto
            {
                Id = instance.Id,
                CorrelationId = instance.CorrelationId,
                WorkflowDefinitionId = instance.WorkflowDefinitionId,
                WorkflowName = instance.WorkflowDefinition?.Name ?? "Unknown",
                WorkflowVersionId = instance.WorkflowVersionId,
                VersionNumber = instance.WorkflowVersion?.VersionNumber ?? 0,
                EntityType = instance.EntityType,
                EntityId = instance.EntityId,
                Status = instance.Status.ToString(),
                Priority = instance.Priority,
                StartedAt = instance.StartedAt,
                CompletedAt = instance.CompletedAt,
                ScheduledAt = instance.ScheduledAt,
                TimeoutAt = instance.TimeoutAt,
                CurrentNodeId = instance.CurrentNodeId,
                CurrentNodeName = instance.CurrentNode?.Name,
                TriggerEvent = instance.TriggerEvent,
                TriggeredById = instance.TriggeredById,
                TriggeredByName = instance.TriggeredBy != null
                    ? $"{instance.TriggeredBy.FirstName} {instance.TriggeredBy.LastName}"
                    : "System",
                InputData = instance.InputData,
                StateData = instance.StateData,
                OutputData = instance.OutputData,
                ErrorMessage = instance.ErrorMessage,
                ErrorStackTrace = instance.ErrorStackTrace,
                RetryCount = instance.RetryCount,
                MaxRetries = instance.MaxRetries,
                NextRetryAt = instance.NextRetryAt,
                IsCancelled = instance.IsCancelled,
                CancellationReason = instance.CancellationReason,
                ParentInstanceId = instance.ParentInstanceId,
                CreatedAt = instance.CreatedAt,
                UpdatedAt = instance.UpdatedAt,
                NodeInstances = instance.NodeInstances?.OrderBy(ni => ni.ExecutionSequence).Select(ni => new WorkflowNodeInstanceDto
                {
                    Id = ni.Id,
                    NodeId = ni.WorkflowNodeId,
                    NodeName = ni.WorkflowNode?.Name ?? "Unknown",
                    Status = ni.Status.ToString(),
                    StartedAt = ni.StartedAt,
                    CompletedAt = ni.CompletedAt,
                    DurationMs = ni.DurationMs,
                    RetryCount = ni.RetryCount,
                    IsSkipped = ni.IsSkipped,
                    SkipReason = ni.SkipReason,
                    ErrorMessage = ni.ErrorMessage
                }).ToList() ?? new List<WorkflowNodeInstanceDto>(),
                Logs = instance.Logs?.Select(l => new WorkflowLogDto
                {
                    Id = l.Id,
                    Level = l.Level.ToString(),
                    Category = l.Category,
                    Message = l.Message,
                    NodeId = l.WorkflowNodeId,
                    Timestamp = l.Timestamp
                }).ToList() ?? new List<WorkflowLogDto>()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow instance {InstanceId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving instance" });
        }
    }

    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    [HttpPost("instances")]
    public async Task<IActionResult> StartWorkflow([FromBody] StartWorkflowRequest request)
    {
        try
        {
            var instance = await _instanceService.StartWorkflowAsync(
                request.WorkflowDefinitionId,
                request.EntityType,
                request.EntityId,
                request.TriggerEvent ?? "ManualStart",
                GetCurrentUserId(),
                request.InputData,
                request.ScheduledAt);

            return Ok(new
            {
                id = instance.Id,
                correlationId = instance.CorrelationId,
                status = instance.Status.ToString(),
                message = "Workflow started successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workflow");
            return StatusCode(500, new { message = "An error occurred while starting workflow" });
        }
    }

    /// <summary>
    /// Cancel a workflow instance
    /// </summary>
    [HttpPost("instances/{id}/cancel")]
    public async Task<IActionResult> CancelInstance(int id, [FromBody] CancelInstanceRequest request)
    {
        try
        {
            var success = await _instanceService.CancelInstanceAsync(id, request.Reason, GetCurrentUserId());
            if (!success) return NotFound(new { message = "Instance not found or already completed" });
            return Ok(new { message = "Workflow cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling workflow instance {InstanceId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Pause a workflow instance
    /// </summary>
    [HttpPost("instances/{id}/pause")]
    public async Task<IActionResult> PauseInstance(int id)
    {
        try
        {
            var success = await _instanceService.PauseInstanceAsync(id, GetCurrentUserId());
            if (!success) return NotFound(new { message = "Instance not found or not running" });
            return Ok(new { message = "Workflow paused successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing workflow instance {InstanceId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Resume a paused workflow instance
    /// </summary>
    [HttpPost("instances/{id}/resume")]
    public async Task<IActionResult> ResumeInstance(int id)
    {
        try
        {
            var success = await _instanceService.ResumeInstanceAsync(id, GetCurrentUserId());
            if (!success) return NotFound(new { message = "Instance not found or not paused" });
            return Ok(new { message = "Workflow resumed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming workflow instance {InstanceId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Retry a failed workflow instance
    /// </summary>
    [HttpPost("instances/{id}/retry")]
    public async Task<IActionResult> RetryInstance(int id)
    {
        try
        {
            var success = await _instanceService.RetryInstanceAsync(id, GetCurrentUserId());
            if (!success) return NotFound(new { message = "Instance not found or not failed" });
            return Ok(new { message = "Workflow retry initiated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying workflow instance {InstanceId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    #endregion

    #region Instance Statistics

    /// <summary>
    /// Get workflow engine statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = now.AddDays(-7);

            var stats = new
            {
                total = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted),
                running = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Running),
                pending = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Pending),
                waiting = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Waiting),
                paused = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Paused),
                completed = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Completed),
                failed = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Failed),
                cancelled = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Cancelled),
                completedToday = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.CompletedAt >= today),
                startedThisWeek = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.StartedAt >= weekAgo)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow stats");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get workflow definitions summary
    /// </summary>
    [HttpGet("definitions")]
    public async Task<IActionResult> GetDefinitions()
    {
        try
        {
            var definitions = await _context.WorkflowDefinitions
                .Where(d => !d.IsDeleted)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Description,
                    d.Category,
                    d.EntityType,
                    Status = d.Status.ToString(),
                    d.Priority,
                    InstanceCount = d.Instances.Count(i => !i.IsDeleted),
                    ActiveInstances = d.Instances.Count(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Running)
                })
                .ToListAsync();

            return Ok(definitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow definitions");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    #endregion

    #region Workflow Tasks

    /// <summary>
    /// Get pending tasks for current user
    /// </summary>
    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks([FromQuery] string? status = null, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        try
        {
            var userId = GetCurrentUserId();

            var query = _context.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                    .ThenInclude(i => i!.WorkflowDefinition)
                .Include(t => t.WorkflowNode)
                .Where(t => !t.IsDeleted && (t.AssignedToId == userId || t.AssignedToId == null));

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkflowTaskStatus>(status, out var s))
                query = query.Where(t => t.Status == s);

            var tasks = await query
                .OrderBy(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(t => new WorkflowTaskDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    TaskType = t.TaskType.ToString(),
                    Status = t.Status.ToString(),
                    Priority = t.Priority,
                    WorkflowInstanceId = t.WorkflowInstanceId,
                    WorkflowName = t.WorkflowInstance != null ? t.WorkflowInstance.WorkflowDefinition.Name : null,
                    NodeId = t.WorkflowNodeId,
                    NodeName = t.WorkflowNode != null ? t.WorkflowNode.Name : null,
                    AssignedToId = t.AssignedToId,
                    DueAt = t.DueAt,
                    TimeoutAt = t.TimeoutAt,
                    FormSchema = t.FormSchema,
                    FormData = t.FormData,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow tasks");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Complete a workflow task
    /// </summary>
    [HttpPost("tasks/{id}/complete")]
    public async Task<IActionResult> CompleteTask(int id, [FromBody] CompleteTaskRequest request)
    {
        try
        {
            var task = await _context.WorkflowTasks.FindAsync(id);
            if (task == null) return NotFound(new { message = "Task not found" });

            task.Status = WorkflowTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.OutputData = request.OutputData != null ? System.Text.Json.JsonSerializer.Serialize(request.OutputData) : null;
            task.FormData = request.FormData != null ? System.Text.Json.JsonSerializer.Serialize(request.FormData) : null;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Task completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing workflow task {TaskId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Claim a task for the current user
    /// </summary>
    [HttpPost("tasks/{id}/claim")]
    public async Task<IActionResult> ClaimTask(int id)
    {
        try
        {
            var task = await _context.WorkflowTasks.FindAsync(id);
            if (task == null) return NotFound(new { message = "Task not found" });

            if (task.AssignedToId.HasValue && task.AssignedToId != GetCurrentUserId())
                return BadRequest(new { message = "Task is already assigned" });

            task.AssignedToId = GetCurrentUserId();
            task.Status = WorkflowTaskStatus.Running;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Task claimed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming workflow task {TaskId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    #endregion

    #region Health Check

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "WorkflowEngine",
            timestamp = DateTime.UtcNow
        });
    }

    #endregion
}

#region Request DTOs

public class StartWorkflowRequest
{
    public int WorkflowDefinitionId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? TriggerEvent { get; set; }
    public object? InputData { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class CancelInstanceRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CompleteTaskRequest
{
    public object? OutputData { get; set; }
    public object? FormData { get; set; }
}

#endregion

#region Response DTOs

public class WorkflowInstanceDto
{
    public int Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public int WorkflowVersionId { get; set; }
    public int VersionNumber { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CurrentNodeId { get; set; }
    public string? CurrentNodeName { get; set; }
    public string? TriggeredByName { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsCancelled { get; set; }
}

public class WorkflowInstanceDetailDto : WorkflowInstanceDto
{
    public DateTime? ScheduledAt { get; set; }
    public DateTime? TimeoutAt { get; set; }
    public string? TriggerEvent { get; set; }
    public int? TriggeredById { get; set; }
    public string? InputData { get; set; }
    public string? StateData { get; set; }
    public string? OutputData { get; set; }
    public string? ErrorStackTrace { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public string? CancellationReason { get; set; }
    public int? ParentInstanceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<WorkflowNodeInstanceDto> NodeInstances { get; set; } = new();
    public List<WorkflowLogDto> Logs { get; set; } = new();
}

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
    public bool IsSkipped { get; set; }
    public string? SkipReason { get; set; }
    public string? ErrorMessage { get; set; }
}

public class WorkflowLogDto
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? NodeId { get; set; }
    public DateTime Timestamp { get; set; }
}

public class WorkflowTaskDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int WorkflowInstanceId { get; set; }  // Matches the assignment
    public string? WorkflowName { get; set; }
    public int? NodeId { get; set; }
    public string? NodeName { get; set; }
    public int? AssignedToId { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? TimeoutAt { get; set; }
    public string? FormSchema { get; set; }
    public string? FormData { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion
