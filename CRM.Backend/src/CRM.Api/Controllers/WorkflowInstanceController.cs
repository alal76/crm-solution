// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities.Workflow;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing workflow instances and monitoring
/// </summary>
[ApiController]
[Route("api/workflow-instances")]
[Authorize]
public class WorkflowInstanceController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly WorkflowInstanceService _instanceService;
    private readonly ILogger<WorkflowInstanceController> _logger;

    public WorkflowInstanceController(
        CrmDbContext context,
        WorkflowInstanceService instanceService,
        ILogger<WorkflowInstanceController> logger)
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

    private string[] GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToArray() ?? Array.Empty<string>();
    }

    #region Instance List & Details

    /// <summary>
    /// Get workflow instances with filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetInstances(
        [FromQuery] int? workflowDefinitionId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int? entityId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        try
        {
            WorkflowInstanceStatus? statusFilter = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkflowInstanceStatus>(status, out var s))
                statusFilter = s;

            var instances = await _instanceService.GetInstancesAsync(
                workflowDefinitionId, entityType, entityId, statusFilter, fromDate, toDate, skip, take);

            var result = instances.Select(i => new WorkflowInstanceDto
            {
                Id = i.Id,
                CorrelationId = i.CorrelationId,
                WorkflowDefinitionId = i.WorkflowDefinitionId,
                WorkflowName = i.WorkflowDefinition.Name,
                WorkflowVersionId = i.WorkflowVersionId,
                VersionNumber = i.WorkflowVersion.VersionNumber,
                EntityType = i.EntityType,
                EntityId = i.EntityId,
                Status = i.Status.ToString(),
                CurrentNodeId = i.CurrentNodeId,
                CurrentNodeName = i.CurrentNode?.Name,
                TriggerEvent = i.TriggerEvent,
                TriggeredByName = i.TriggeredBy != null 
                    ? $"{i.TriggeredBy.FirstName} {i.TriggeredBy.LastName}" 
                    : null,
                StartedAt = i.StartedAt,
                CompletedAt = i.CompletedAt,
                ScheduledAt = i.ScheduledAt,
                Priority = i.Priority,
                RetryCount = i.RetryCount,
                ErrorMessage = i.ErrorMessage,
                IsCancelled = i.IsCancelled,
                CreatedAt = i.CreatedAt
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
    /// Get a specific instance with full details
    /// </summary>
    [HttpGet("{id}")]
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
                WorkflowName = instance.WorkflowDefinition.Name,
                WorkflowVersionId = instance.WorkflowVersionId,
                VersionNumber = instance.WorkflowVersion.VersionNumber,
                EntityType = instance.EntityType,
                EntityId = instance.EntityId,
                Status = instance.Status.ToString(),
                CurrentNodeId = instance.CurrentNodeId,
                CurrentNodeName = instance.CurrentNode?.Name,
                TriggerEvent = instance.TriggerEvent,
                TriggeredById = instance.TriggeredById,
                TriggeredByName = instance.TriggeredBy != null 
                    ? $"{instance.TriggeredBy.FirstName} {instance.TriggeredBy.LastName}" 
                    : null,
                InputData = instance.InputData,
                StateData = instance.StateData,
                OutputData = instance.OutputData,
                StartedAt = instance.StartedAt,
                CompletedAt = instance.CompletedAt,
                ScheduledAt = instance.ScheduledAt,
                TimeoutAt = instance.TimeoutAt,
                Priority = instance.Priority,
                RetryCount = instance.RetryCount,
                MaxRetries = instance.MaxRetries,
                NextRetryAt = instance.NextRetryAt,
                ErrorMessage = instance.ErrorMessage,
                ErrorStackTrace = instance.ErrorStackTrace,
                IsCancelled = instance.IsCancelled,
                CancellationReason = instance.CancellationReason,
                ParentInstanceId = instance.ParentInstanceId,
                CreatedAt = instance.CreatedAt,
                UpdatedAt = instance.UpdatedAt,
                
                // Include workflow graph for visualization
                Nodes = instance.WorkflowVersion.Nodes.Select(n => new WorkflowNodeDto
                {
                    Id = n.Id,
                    NodeKey = n.NodeKey,
                    Name = n.Name,
                    NodeType = n.NodeType.ToString(),
                    PositionX = n.PositionX,
                    PositionY = n.PositionY,
                    Width = n.Width,
                    Height = n.Height,
                    IconName = n.IconName,
                    Color = n.Color,
                    IsStartNode = n.IsStartNode,
                    IsEndNode = n.IsEndNode
                }).ToList(),
                
                Transitions = instance.WorkflowVersion.Transitions.Select(t => new WorkflowTransitionDto
                {
                    Id = t.Id,
                    SourceNodeId = t.SourceNodeId,
                    TargetNodeId = t.TargetNodeId,
                    Label = t.Label,
                    SourceHandle = t.SourceHandle,
                    TargetHandle = t.TargetHandle,
                    LineStyle = t.LineStyle,
                    Color = t.Color
                }).ToList(),
                
                // Node execution history
                NodeInstances = instance.NodeInstances
                    .OrderBy(ni => ni.ExecutionSequence)
                    .Select(ni => new WorkflowNodeInstanceDto
                    {
                        Id = ni.Id,
                        NodeId = ni.WorkflowNodeId,
                        NodeName = ni.WorkflowNode.Name,
                        Status = ni.Status.ToString(),
                        StartedAt = ni.StartedAt,
                        CompletedAt = ni.CompletedAt,
                        DurationMs = ni.DurationMs,
                        RetryCount = ni.RetryCount,
                        ErrorMessage = ni.ErrorMessage,
                        IsSkipped = ni.IsSkipped,
                        SkipReason = ni.SkipReason,
                        ExecutionSequence = ni.ExecutionSequence,
                        WorkerId = ni.WorkerId
                    }).ToList(),
                
                // Pending tasks
                Tasks = instance.Tasks
                    .Where(t => t.Status != WorkflowTaskStatus.Completed)
                    .Select(t => new WorkflowTaskDto
                    {
                        Id = t.Id,
                        NodeId = t.WorkflowNodeId,
                        NodeName = t.WorkflowNode.Name,
                        TaskType = t.TaskType.ToString(),
                        Name = t.Name,
                        Status = t.Status.ToString(),
                        Priority = t.Priority,
                        DueAt = t.DueAt,
                        AssignedToId = t.AssignedToId,
                        AssignedToRole = t.AssignedToRole,
                        RetryCount = t.RetryCount,
                        IsDeadLetter = t.IsDeadLetter,
                        CreatedAt = t.CreatedAt
                    }).ToList(),
                
                // Recent logs
                RecentLogs = instance.Logs
                    .Take(50)
                    .Select(l => new WorkflowLogDto
                    {
                        Id = l.Id,
                        Level = l.Level.ToString(),
                        Category = l.Category,
                        Message = l.Message,
                        NodeName = l.WorkflowNode?.Name,
                        Timestamp = l.Timestamp,
                        DurationMs = l.DurationMs
                    }).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving instance {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the instance" });
        }
    }

    /// <summary>
    /// Get instances for a specific entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetInstancesForEntity(string entityType, int entityId)
    {
        try
        {
            var instances = await _instanceService.GetInstancesAsync(
                entityType: entityType, entityId: entityId, take: 100);

            var result = instances.Select(i => new WorkflowInstanceDto
            {
                Id = i.Id,
                CorrelationId = i.CorrelationId,
                WorkflowDefinitionId = i.WorkflowDefinitionId,
                WorkflowName = i.WorkflowDefinition.Name,
                Status = i.Status.ToString(),
                StartedAt = i.StartedAt,
                CompletedAt = i.CompletedAt,
                ErrorMessage = i.ErrorMessage,
                CreatedAt = i.CreatedAt
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving instances for entity {EntityType}:{EntityId}", entityType, entityId);
            return StatusCode(500, new { message = "An error occurred while retrieving instances" });
        }
    }

    #endregion

    #region Instance Actions

    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> StartWorkflow([FromBody] StartWorkflowDto dto)
    {
        try
        {
            var instance = await _instanceService.StartWorkflowAsync(
                dto.WorkflowDefinitionId,
                dto.EntityType,
                dto.EntityId,
                dto.TriggerEvent ?? "Manual",
                GetCurrentUserId(),
                dto.InputData,
                dto.ScheduledAt);

            return Ok(new { id = instance.Id, correlationId = instance.CorrelationId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workflow instance");
            return StatusCode(500, new { message = "An error occurred while starting the workflow" });
        }
    }

    /// <summary>
    /// Cancel a workflow instance
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelInstance(int id, [FromBody] CancelInstanceDto dto)
    {
        try
        {
            var success = await _instanceService.CancelInstanceAsync(id, dto.Reason, GetCurrentUserId());
            if (!success) return BadRequest(new { message = "Cannot cancel this instance" });
            return Ok(new { message = "Instance cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling instance {Id}", id);
            return StatusCode(500, new { message = "An error occurred while cancelling the instance" });
        }
    }

    /// <summary>
    /// Pause a workflow instance
    /// </summary>
    [HttpPost("{id}/pause")]
    public async Task<IActionResult> PauseInstance(int id)
    {
        try
        {
            var success = await _instanceService.PauseInstanceAsync(id, GetCurrentUserId());
            if (!success) return BadRequest(new { message = "Cannot pause this instance" });
            return Ok(new { message = "Instance paused successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing instance {Id}", id);
            return StatusCode(500, new { message = "An error occurred while pausing the instance" });
        }
    }

    /// <summary>
    /// Resume a paused workflow instance
    /// </summary>
    [HttpPost("{id}/resume")]
    public async Task<IActionResult> ResumeInstance(int id)
    {
        try
        {
            var success = await _instanceService.ResumeInstanceAsync(id, GetCurrentUserId());
            if (!success) return BadRequest(new { message = "Cannot resume this instance" });
            return Ok(new { message = "Instance resumed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming instance {Id}", id);
            return StatusCode(500, new { message = "An error occurred while resuming the instance" });
        }
    }

    /// <summary>
    /// Retry a failed workflow instance
    /// </summary>
    [HttpPost("{id}/retry")]
    public async Task<IActionResult> RetryInstance(int id)
    {
        try
        {
            var success = await _instanceService.RetryInstanceAsync(id, GetCurrentUserId());
            if (!success) return BadRequest(new { message = "Cannot retry this instance" });
            return Ok(new { message = "Instance retry scheduled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying instance {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrying the instance" });
        }
    }

    /// <summary>
    /// Skip a node in a workflow instance
    /// </summary>
    [HttpPost("{id}/skip-node/{nodeId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SkipNode(int id, int nodeId, [FromBody] SkipNodeDto dto)
    {
        try
        {
            var success = await _instanceService.SkipNodeAsync(id, nodeId, dto.Reason, GetCurrentUserId());
            if (!success) return BadRequest(new { message = "Cannot skip this node" });
            return Ok(new { message = "Node skipped successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping node {NodeId} in instance {Id}", nodeId, id);
            return StatusCode(500, new { message = "An error occurred while skipping the node" });
        }
    }

    #endregion

    #region Human Tasks

    /// <summary>
    /// Get human tasks for the current user
    /// </summary>
    [HttpGet("my-tasks")]
    public async Task<IActionResult> GetMyTasks()
    {
        try
        {
            var userId = GetCurrentUserId();
            var roles = GetCurrentUserRoles();
            
            var tasks = await _instanceService.GetHumanTasksForUserAsync(userId, roles);

            var result = tasks.Select(t => new HumanTaskDto
            {
                Id = t.Id,
                WorkflowInstanceId = t.WorkflowInstanceId,
                WorkflowName = t.WorkflowInstance.WorkflowDefinition.Name,
                NodeId = t.WorkflowNodeId,
                NodeName = t.WorkflowNode.Name,
                Name = t.Name,
                Description = t.Description,
                Priority = t.Priority,
                DueAt = t.DueAt,
                FormSchema = t.FormSchema,
                EntityType = t.WorkflowInstance.EntityType,
                EntityId = t.WorkflowInstance.EntityId,
                CreatedAt = t.CreatedAt
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving human tasks");
            return StatusCode(500, new { message = "An error occurred while retrieving tasks" });
        }
    }

    /// <summary>
    /// Claim a human task
    /// </summary>
    [HttpPost("tasks/{taskId}/claim")]
    public async Task<IActionResult> ClaimTask(int taskId)
    {
        try
        {
            var task = await _context.WorkflowTasks.FindAsync(taskId);
            if (task == null) return NotFound(new { message = "Task not found" });
            if (task.AssignedToId != null) return BadRequest(new { message = "Task already claimed" });

            task.AssignedToId = GetCurrentUserId();
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task claimed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming task {Id}", taskId);
            return StatusCode(500, new { message = "An error occurred while claiming the task" });
        }
    }

    /// <summary>
    /// Complete a human task
    /// </summary>
    [HttpPost("tasks/{taskId}/complete")]
    public async Task<IActionResult> CompleteTask(int taskId, [FromBody] CompleteTaskDto dto)
    {
        try
        {
            var task = await _context.WorkflowTasks.FindAsync(taskId);
            if (task == null) return NotFound(new { message = "Task not found" });
            
            var userId = GetCurrentUserId();
            if (task.AssignedToId != userId)
                return BadRequest(new { message = "Task not assigned to you" });

            task.FormData = dto.FormData;
            task.OutputData = dto.OutputData;
            task.Status = WorkflowTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Log the completion
            await _instanceService.LogAsync(
                task.WorkflowInstanceId,
                WorkflowLogLevel.Info,
                "HumanTask",
                $"Task completed by user",
                task.WorkflowNodeId,
                userId: userId);

            return Ok(new { message = "Task completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task {Id}", taskId);
            return StatusCode(500, new { message = "An error occurred while completing the task" });
        }
    }

    #endregion

    #region Logs

    /// <summary>
    /// Get logs for an instance
    /// </summary>
    [HttpGet("{id}/logs")]
    public async Task<IActionResult> GetLogs(
        int id,
        [FromQuery] string? minLevel = null,
        [FromQuery] string? category = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        try
        {
            WorkflowLogLevel? levelFilter = null;
            if (!string.IsNullOrEmpty(minLevel) && Enum.TryParse<WorkflowLogLevel>(minLevel, out var l))
                levelFilter = l;

            var logs = await _instanceService.GetLogsAsync(id, levelFilter, category, skip, take);

            var result = logs.Select(l => new WorkflowLogDto
            {
                Id = l.Id,
                Level = l.Level.ToString(),
                Category = l.Category,
                Message = l.Message,
                Details = l.Details,
                NodeName = l.WorkflowNode?.Name,
                UserName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : null,
                WorkerId = l.WorkerId,
                Timestamp = l.Timestamp,
                DurationMs = l.DurationMs
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for instance {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving logs" });
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get instance statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] int? workflowDefinitionId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var query = _context.WorkflowInstances.Where(i => !i.IsDeleted);

            if (workflowDefinitionId.HasValue)
                query = query.Where(i => i.WorkflowDefinitionId == workflowDefinitionId.Value);

            if (fromDate.HasValue)
                query = query.Where(i => i.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.CreatedAt <= toDate.Value);

            var stats = new
            {
                Total = await query.CountAsync(),
                Pending = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Pending),
                Running = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Running),
                Waiting = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Waiting),
                Completed = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Completed),
                Failed = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Failed),
                Cancelled = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.Cancelled),
                TimedOut = await query.CountAsync(i => i.Status == WorkflowInstanceStatus.TimedOut),
                
                // Average completion time (in minutes)
                AverageCompletionTimeMinutes = await query
                    .Where(i => i.Status == WorkflowInstanceStatus.Completed && 
                               i.StartedAt != null && i.CompletedAt != null)
                    .Select(i => (double)((i.CompletedAt!.Value - i.StartedAt!.Value).TotalMinutes))
                    .DefaultIfEmpty(0)
                    .AverageAsync(),
                
                // Status breakdown by workflow
                ByWorkflow = await query
                    .GroupBy(i => new { i.WorkflowDefinitionId, i.WorkflowDefinition.Name })
                    .Select(g => new
                    {
                        WorkflowId = g.Key.WorkflowDefinitionId,
                        WorkflowName = g.Key.Name,
                        Total = g.Count(),
                        Completed = g.Count(i => i.Status == WorkflowInstanceStatus.Completed),
                        Failed = g.Count(i => i.Status == WorkflowInstanceStatus.Failed)
                    })
                    .ToListAsync()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving instance statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
        }
    }

    #endregion
}

#region DTOs

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
    public int? CurrentNodeId { get; set; }
    public string? CurrentNodeName { get; set; }
    public string? TriggerEvent { get; set; }
    public string? TriggeredByName { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int Priority { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WorkflowInstanceDetailDto : WorkflowInstanceDto
{
    public int? TriggeredById { get; set; }
    public string? InputData { get; set; }
    public string? StateData { get; set; }
    public string? OutputData { get; set; }
    public DateTime? TimeoutAt { get; set; }
    public int MaxRetries { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public string? ErrorStackTrace { get; set; }
    public string? CancellationReason { get; set; }
    public int? ParentInstanceId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<WorkflowNodeDto> Nodes { get; set; } = new();
    public List<WorkflowTransitionDto> Transitions { get; set; } = new();
    public List<WorkflowNodeInstanceDto> NodeInstances { get; set; } = new();
    public List<WorkflowTaskDto> Tasks { get; set; } = new();
    public List<WorkflowLogDto> RecentLogs { get; set; } = new();
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
    public string? ErrorMessage { get; set; }
    public bool IsSkipped { get; set; }
    public string? SkipReason { get; set; }
    public int ExecutionSequence { get; set; }
    public string? WorkerId { get; set; }
}

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

public class WorkflowLogDto
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? NodeName { get; set; }
    public string? UserName { get; set; }
    public string? WorkerId { get; set; }
    public DateTime Timestamp { get; set; }
    public long? DurationMs { get; set; }
}

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
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StartWorkflowDto
{
    public int WorkflowDefinitionId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? TriggerEvent { get; set; }
    public object? InputData { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class CancelInstanceDto
{
    public string Reason { get; set; } = string.Empty;
}

public class SkipNodeDto
{
    public string Reason { get; set; } = string.Empty;
}

public class CompleteTaskDto
{
    public string? FormData { get; set; }
    public string? OutputData { get; set; }
}

#endregion
