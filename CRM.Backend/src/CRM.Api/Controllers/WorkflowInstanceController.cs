// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.DTOs.Workflow;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Manages workflow instances - starting, monitoring, and controlling workflow executions
/// </summary>
[ApiController]
[Route("api/workflow-instances")]
[Route("api/workflows/instances")]
[Authorize]
public class WorkflowInstanceController : CrmControllerBase
{
    private readonly IWorkflowInstanceService _instanceService;
    private readonly IHttpCalloutService _calloutService;
    private readonly ILogger<WorkflowInstanceController> _logger;

    public WorkflowInstanceController(
        IWorkflowInstanceService instanceService,
        IHttpCalloutService calloutService,
        ILogger<WorkflowInstanceController> logger)
    {
        _instanceService = instanceService;
        _calloutService = calloutService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    private string[] GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
    }

    #region Instance Queries

    /// <summary>
    /// Get all workflow instances with filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInstances(
        [FromQuery] int? definitionId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int? entityId = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
                WorkflowInstanceStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkflowInstanceStatus>(status, true, out var parsed))
            statusFilter = parsed;

        var instances = await _instanceService.GetInstancesAsync(
            workflowDefinitionId: definitionId,
            entityType: entityType,
            entityId: entityId,
            status: statusFilter,
            fromDate: null,
            toDate: null,
            skip: skip,
            take: take);

        var result = instances.Select(i => new WorkflowInstanceDto
        {
            Id = i.Id,
            CorrelationId = i.CorrelationId,
            WorkflowDefinitionId = i.WorkflowDefinitionId,
            WorkflowName = i.WorkflowDefinition?.Name ?? string.Empty,
            Status = i.Status.ToString(),
            StartedAt = i.StartedAt,
            CompletedAt = i.CompletedAt,
            ErrorMessage = i.ErrorMessage,
            CreatedAt = i.CreatedAt
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Get a specific workflow instance with full details
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInstance(int id)
    {
                var instance = await _instanceService.GetInstanceAsync(id);
        if (instance == null)
            return NotFound(new { message = $"Workflow instance {id} not found" });

        var result = new WorkflowInstanceDetailDto
        {
            Id = instance.Id,
            CorrelationId = instance.CorrelationId,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            WorkflowName = instance.WorkflowDefinition?.Name ?? string.Empty,
            WorkflowVersionId = instance.WorkflowVersionId,
            VersionNumber = instance.WorkflowVersion?.VersionNumber ?? 0,
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
            Nodes = (instance.WorkflowVersion?.Nodes ?? new List<WorkflowNode>()).Select(n => new WorkflowNodeDto
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

            Transitions = (instance.WorkflowVersion?.Transitions ?? new List<WorkflowTransition>()).Select(t => new WorkflowTransitionDto
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
            NodeInstances = (instance.NodeInstances ?? new List<WorkflowNodeInstance>())
                .OrderBy(ni => ni.ExecutionSequence)
                .Select(ni => new WorkflowNodeInstanceDto
                {
                    Id = ni.Id,
                    NodeId = ni.WorkflowNodeId,
                    NodeName = ni.WorkflowNode?.Name ?? string.Empty,
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
            Tasks = (instance.Tasks ?? new List<WorkflowTask>())
                .Where(t => t.Status != WorkflowTaskStatus.Completed)
                .Select(t => new WorkflowTaskDto
                {
                    Id = t.Id,
                    NodeId = t.WorkflowNodeId,
                    NodeName = t.WorkflowNode?.Name ?? string.Empty,
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
            RecentLogs = (instance.Logs ?? new List<WorkflowLog>())
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

    /// <summary>
    /// Get instances for a specific entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInstancesForEntity(string entityType, int entityId)
    {
                var instances = await _instanceService.GetInstancesAsync(
            workflowDefinitionId: null,
            entityType: entityType,
            entityId: entityId,
            status: null,
            fromDate: null,
            toDate: null,
            skip: 0,
            take: 100);

        var result = instances.Select(i => new WorkflowInstanceDto
        {
            Id = i.Id,
            CorrelationId = i.CorrelationId,
            WorkflowDefinitionId = i.WorkflowDefinitionId,
            WorkflowName = i.WorkflowDefinition?.Name ?? string.Empty,
            Status = i.Status.ToString(),
            StartedAt = i.StartedAt,
            CompletedAt = i.CompletedAt,
            ErrorMessage = i.ErrorMessage,
            CreatedAt = i.CreatedAt
        }).ToList();

        return Ok(result);
    }

    #endregion

    #region Instance Actions

    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartWorkflow([FromBody] StartWorkflowDto dto)
    {
        try
        {
            var instance = await _instanceService.StartWorkflowAsync(
                dto.WorkflowDefinitionId,
                dto.EntityType ?? string.Empty,
                dto.EntityId ?? 0,
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
    }

    /// <summary>
    /// Cancel a workflow instance
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelInstance(int id, [FromBody] CancelInstanceDto dto)
    {
                var success = await _instanceService.CancelInstanceAsync(id, dto.Reason ?? string.Empty, GetCurrentUserId());
        if (!success)
            return BadRequest(new { message = "Cannot cancel this instance" });
        return Ok(new { message = "Instance cancelled successfully" });
    }

    /// <summary>
    /// Pause a workflow instance
    /// </summary>
    [HttpPost("{id}/pause")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PauseInstance(int id)
    {
                var success = await _instanceService.PauseInstanceAsync(id, GetCurrentUserId());
        if (!success)
            return BadRequest(new { message = "Cannot pause this instance" });
        return Ok(new { message = "Instance paused successfully" });
    }

    /// <summary>
    /// Resume a paused workflow instance
    /// </summary>
    [HttpPost("{id}/resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResumeInstance(int id)
    {
                var success = await _instanceService.ResumeInstanceAsync(id, GetCurrentUserId());
        if (!success)
            return BadRequest(new { message = "Cannot resume this instance" });
        return Ok(new { message = "Instance resumed successfully" });
    }

    /// <summary>
    /// Retry a failed workflow instance
    /// </summary>
    [HttpPost("{id}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RetryInstance(int id)
    {
                var success = await _instanceService.RetryInstanceAsync(id, GetCurrentUserId());
        if (!success)
            return BadRequest(new { message = "Cannot retry this instance" });
        return Ok(new { message = "Instance retry scheduled" });
    }

    /// <summary>
    /// Skip a node in a workflow instance
    /// </summary>
    [HttpPost("{id}/skip-node/{nodeId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SkipNode(int id, int nodeId, [FromBody] SkipNodeDto dto)
    {
                var success = await _instanceService.SkipNodeAsync(id, nodeId, dto.Reason ?? string.Empty, GetCurrentUserId());
        if (!success)
            return BadRequest(new { message = "Cannot skip this node" });
        return Ok(new { message = "Node skipped successfully" });
    }

    #endregion

    #region Human Tasks

    /// <summary>
    /// Get human tasks for the current user
    /// </summary>
    [HttpGet("my-tasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTasks()
    {
                var userId = GetCurrentUserId();
        var roles = GetCurrentUserRoles();

        var tasks = await _instanceService.GetHumanTasksForUserAsync(userId, roles);

        var result = tasks.Select(t => new HumanTaskDto
        {
            Id = t.Id,
            WorkflowInstanceId = t.WorkflowInstanceId,
            WorkflowName = t.WorkflowInstance?.WorkflowDefinition?.Name ?? string.Empty,
            NodeId = t.WorkflowNodeId,
            NodeName = t.WorkflowNode?.Name ?? string.Empty,
            Name = t.Name,
            Description = t.Description,
            Priority = t.Priority,
            DueAt = t.DueAt,
            FormSchema = t.FormSchema,
            EntityType = t.WorkflowInstance?.EntityType,
            EntityId = t.WorkflowInstance?.EntityId,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Claim a human task
    /// </summary>
    [HttpPost("tasks/{taskId}/claim")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ClaimTask(int taskId)
    {
                var userId = GetCurrentUserId();
        var success = await _instanceService.ClaimTaskAsync(taskId, userId);
        if (!success)
            return BadRequest(new { message = "Cannot claim this task. It may not exist or is already assigned." });
        return Ok(new { message = "Task claimed successfully" });
    }

    /// <summary>
    /// Complete a human task
    /// </summary>
    [HttpPost("tasks/{taskId}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteTask(int taskId, [FromBody] CompleteTaskDto dto)
    {
                var userId = GetCurrentUserId();
        var success = await _instanceService.CompleteHumanTaskAsync(taskId, userId, dto.FormData, dto.OutputData);
        if (!success)
            return BadRequest(new { message = "Cannot complete this task. It may not exist or is not assigned to you." });
        return Ok(new { message = "Task completed successfully" });
    }

    /// <summary>
    /// Reassign a human task to another user
    /// </summary>
    [HttpPost("tasks/{taskId}/reassign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReassignTask(int taskId, [FromQuery] int assignToUserId)
    {
                var success = await _instanceService.ClaimTaskAsync(taskId, assignToUserId);
        if (!success)
            return BadRequest(new { message = "Cannot reassign this task" });
        return Ok(new { message = "Task reassigned successfully" });
    }

    #endregion

    #region Logs

    /// <summary>
    /// Get logs for a specific workflow instance
    /// </summary>
    [HttpGet("{id}/logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(
        int id,
        [FromQuery] string? level = null,
        [FromQuery] string? category = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
                WorkflowLogLevel? minLevel = null;
        if (!string.IsNullOrEmpty(level) && Enum.TryParse<WorkflowLogLevel>(level, true, out var parsedLevel))
            minLevel = parsedLevel;

        var logs = await _instanceService.GetLogsAsync(id, minLevel, category, skip, take);

        var result = logs.Select(l => new WorkflowLogDto
        {
            Id = l.Id,
            WorkflowInstanceId = l.WorkflowInstanceId,
            Level = l.Level.ToString(),
            Category = l.Category,
            Message = l.Message,
            Details = l.Details,
            NodeName = l.WorkflowNode?.Name,
            NodeInstanceId = l.NodeInstanceId,
            Timestamp = l.Timestamp,
            DurationMs = l.DurationMs,
            WorkerId = l.WorkerId,
            UserId = l.UserId,
            UserName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : null,
            ExceptionType = l.ExceptionType
        }).ToList();

        return Ok(result);
    }

    #endregion

    #region Audit & Monitoring

    /// <summary>
    /// Get audit log for a workflow definition
    /// </summary>
    [HttpGet("definitions/{definitionId}/audit-log")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLog(
        int definitionId,
        [FromQuery] string? eventType = null,
        [FromQuery] string? eventCategory = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
                var (logs, hasMore) = await _instanceService.GetAuditLogAsync(
            definitionId, eventType, eventCategory, fromDate, toDate, skip, take);

        var result = logs.Select(l => new WorkflowAuditLogDto
        {
            Id = l.Id,
            WorkflowInstanceId = l.WorkflowInstanceId,
            Level = l.Level.ToString(),
            Category = l.Category,
            Message = l.Message,
            NodeName = l.WorkflowNode?.Name,
            Timestamp = l.Timestamp,
            DurationMs = l.DurationMs,
            Data = l.Details,
            UserId = l.UserId,
            UserName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : null,
            WorkerId = l.WorkerId,
            ExceptionType = l.ExceptionType
        }).ToList();

        return Ok(new { items = result, hasMore });
    }

    /// <summary>
    /// Export audit log as CSV
    /// </summary>
    [HttpGet("definitions/{definitionId}/audit-log/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportAuditLog(
        int definitionId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
                var csvBytes = await _instanceService.ExportAuditLogCsvAsync(definitionId, fromDate, toDate);
        var fileName = $"audit-log-{definitionId}-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(csvBytes, "text/csv", fileName);
    }

    /// <summary>
    /// Get execution timeline for a workflow instance
    /// </summary>
    [HttpGet("{id}/timeline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutionTimeline(int id)
    {
                var instance = await _instanceService.GetExecutionTimelineDataAsync(id);
        if (instance == null)
            return NotFound(new { message = $"Workflow instance {id} not found" });

        var nodeInstances = instance.NodeInstances ?? new List<WorkflowNodeInstance>();
        var taskList = instance.Tasks ?? new List<WorkflowTask>();

        var timelineEntries = new List<TimelineEntryDto>();

        timelineEntries.AddRange(nodeInstances.Select(ni => new TimelineEntryDto
        {
            Type = "Node",
            Id = ni.Id,
            Name = ni.WorkflowNode?.Name ?? string.Empty,
            NodeType = ni.WorkflowNode?.NodeType.ToString(),
            Status = ni.Status.ToString(),
            StartedAt = ni.StartedAt,
            CompletedAt = ni.CompletedAt,
            DurationMs = ni.DurationMs,
            IsSkipped = ni.IsSkipped,
            ErrorMessage = ni.ErrorMessage,
            Sequence = ni.ExecutionSequence
        }));

        timelineEntries.AddRange(taskList.Select(t => new TimelineEntryDto
        {
            Type = "Task",
            Id = t.Id,
            Name = t.Name,
            NodeType = "HumanTask",
            Status = t.Status.ToString(),
            StartedAt = t.CreatedAt,
            CompletedAt = t.CompletedAt,
            DurationMs = t.CompletedAt.HasValue
                ? (long?)(t.CompletedAt.Value - t.CreatedAt).TotalMilliseconds
                : null,
            AssignedTo = t.AssignedToId?.ToString(),
            Sequence = 0
        }));

        var result = new ExecutionTimelineDto
        {
            InstanceId = id,
            Entries = timelineEntries.OrderBy(e => e.StartedAt ?? DateTime.MaxValue).ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// Get comprehensive workflow execution dashboard data including status counts,
    /// success/failure rates, duration metrics (avg/median/p95), top-failing workflows,
    /// daily throughput trends, recent errors, and per-workflow breakdown.
    /// </summary>
    /// <param name="fromDate">Start of date range (default: 30 days ago)</param>
    /// <param name="toDate">End of date range (default: now)</param>
    /// <param name="topN">Number of items for top-N lists (default: 10)</param>
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int topN = 10)
    {
                var dashboard = await _instanceService.GetDashboardAsync(fromDate, toDate, topN);
        return Ok(dashboard);
    }

    /// <summary>
    /// Get workflow instance statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] int? definitionId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
                var stats = await _instanceService.GetInstanceStatisticsAsync(definitionId, fromDate, toDate);
        return Ok(stats);
    }

    #endregion

    #region Parallel Gateway & Sub-workflow

    /// <summary>
    /// Advance a workflow instance after a node completes.
    /// Evaluates outgoing transitions and handles parallel gateways, join gateways, and subprocesses.
    /// </summary>
    [HttpPost("{id}/advance/{nodeInstanceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdvanceWorkflow(int id, int nodeInstanceId)
    {
        try
        {
            var startedNodes = await _instanceService.AdvanceWorkflowAsync(id, nodeInstanceId);
            return Ok(new
            {
                message = $"Workflow advanced — {startedNodes.Count} node(s) started",
                startedNodeInstances = startedNodes.Select(ni => new
                {
                    ni.Id,
                    ni.WorkflowNodeId,
                    Status = ni.Status.ToString(),
                    ni.StartedAt
                })
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get parallel branch status for a workflow instance.
    /// Optionally scoped to branches from a specific gateway node.
    /// </summary>
    [HttpGet("{id}/parallel-branches")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParallelBranchStatus(
        int id,
        [FromQuery] int? gatewayNodeId = null)
    {
        try
        {
            var status = await _instanceService.GetParallelBranchStatusAsync(id, gatewayNodeId);
            return Ok(status);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get child workflow instances spawned by a parent instance (via Subprocess nodes).
    /// </summary>
    [HttpGet("{id}/child-instances")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChildInstances(int id)
    {
                var children = await _instanceService.GetChildInstancesAsync(id);
        return Ok(children.Select(c => new
        {
            c.Id,
            c.WorkflowDefinitionId,
            WorkflowName = c.WorkflowDefinition?.Name,
            c.CorrelationId,
            Status = c.Status.ToString(),
            c.EntityType,
            c.EntityId,
            c.StartedAt,
            c.CompletedAt,
            c.ParentInstanceId,
            c.ErrorMessage
        }));
    }

    #endregion

    #region HTTP Callout

    /// <summary>
    /// Test an HTTP callout configuration without running a full workflow.
    /// Useful for validating webhook/API configurations before saving them on a node.
    /// </summary>
    [HttpPost("callout/test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestHttpCallout([FromBody] HttpCalloutConfig config)
    {
                if (config == null)
            return BadRequest(new { message = "Request body is required" });

        var validation = _calloutService.Validate(config);
        if (!validation.IsValid)
            return BadRequest(new { message = "Invalid configuration", errors = validation.Errors });

        var result = await _calloutService.ExecuteAsync(config);
        return Ok(result);
    }

    /// <summary>
    /// Validate an HTTP callout configuration without executing it.
    /// </summary>
    [HttpPost("callout/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ValidateHttpCallout([FromBody] HttpCalloutConfig config)
    {
                if (config == null)
            return BadRequest(new { message = "Request body is required" });

        var validation = _calloutService.Validate(config);
        return Ok(validation);
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Start workflow instances for multiple entities in a single request.
    /// Individual failures do not abort the batch.
    /// </summary>
    [HttpPost("bulk-start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkStartWorkflow([FromBody] BulkStartWorkflowRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required" });

            if (request.EntityIds == null || request.EntityIds.Count == 0)
                return BadRequest(new { message = "At least one entity ID is required" });

            if (request.EntityIds.Count > 500)
                return BadRequest(new { message = "Maximum of 500 entities per bulk operation" });

            var result = await _instanceService.BulkStartWorkflowAsync(
                request.WorkflowDefinitionId,
                request.EntityType,
                request.EntityIds,
                request.TriggerEvent,
                request.TriggeredById,
                request.InputData);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Wait/Timer & Timeout

    /// <summary>
    /// Get all currently waiting (timer/delay) node instances.
    /// Optionally filter by workflow instance ID.
    /// </summary>
    [HttpGet("waiting-nodes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWaitingNodes([FromQuery] int? instanceId = null)
    {
                var nodes = await _instanceService.GetWaitingNodesAsync(instanceId);
        return Ok(nodes.Select(ni => new
        {
            ni.Id,
            ni.WorkflowInstanceId,
            NodeId = ni.WorkflowNode?.Id,
            NodeName = ni.WorkflowNode?.Name,
            Status = ni.Status.ToString(),
            ResumeAt = ni.NextRetryAt,
            ni.StartedAt,
            WorkflowName = ni.WorkflowInstance?.WorkflowDefinition?.Name,
            InstanceCorrelationId = ni.WorkflowInstance?.CorrelationId
        }));
    }

    /// <summary>
    /// Manually resume a waiting node, skipping the remaining wait time.
    /// </summary>
    [HttpPost("waiting-nodes/{nodeInstanceId}/resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResumeWaitingNode(int nodeInstanceId)
    {
        try
        {
            var result = await _instanceService.ResumeWaitingNodeAsync(nodeInstanceId);
            return Ok(new
            {
                result.Id,
                result.WorkflowInstanceId,
                Status = result.Status.ToString(),
                result.CompletedAt,
                Message = "Wait node resumed successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

}

/// <summary>
/// Request model for bulk starting workflow instances
/// </summary>
public class BulkStartWorkflowRequest
{
    /// <summary>Workflow definition to start</summary>
    public int WorkflowDefinitionId { get; set; }

    /// <summary>Entity type (e.g., "Lead", "Opportunity")</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>List of entity IDs to start workflows for</summary>
    public List<int> EntityIds { get; set; } = new();

    /// <summary>Trigger event name</summary>
    public string TriggerEvent { get; set; } = "BulkStart";

    /// <summary>User who triggered the bulk operation</summary>
    public int? TriggeredById { get; set; }

    /// <summary>Optional input data shared across all instances</summary>
    public object? InputData { get; set; }
}
