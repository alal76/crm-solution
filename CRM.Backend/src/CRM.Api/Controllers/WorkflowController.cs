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
using System.Text.Json;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing workflow definitions, versions, nodes, and transitions
/// </summary>
[ApiController]
[Route("api/workflows")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly WorkflowService _workflowService;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        CrmDbContext context,
        WorkflowService workflowService,
        ILogger<WorkflowController> logger)
    {
        _context = context;
        _workflowService = workflowService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    #region Workflow Definitions

    /// <summary>
    /// Get all workflow definitions with filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkflows(
        [FromQuery] string? entityType = null,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        try
        {
            WorkflowStatus? statusFilter = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkflowStatus>(status, out var s))
                statusFilter = s;

            var workflows = await _workflowService.GetWorkflowDefinitionsAsync(
                entityType, statusFilter, category, search, skip, take);

            var result = workflows.Select(w => new WorkflowDefinitionDto
            {
                Id = w.Id,
                WorkflowKey = w.WorkflowKey,
                Name = w.Name,
                Description = w.Description,
                Category = w.Category,
                EntityType = w.EntityType,
                Status = w.Status.ToString(),
                CurrentVersion = w.CurrentVersion,
                IconName = w.IconName,
                Color = w.Color,
                IsSystem = w.IsSystem,
                Priority = w.Priority,
                MaxConcurrentInstances = w.MaxConcurrentInstances,
                DefaultTimeoutHours = w.DefaultTimeoutHours,
                OwnerId = w.OwnerId,
                OwnerName = w.Owner != null ? $"{w.Owner.FirstName} {w.Owner.LastName}" : null,
                Tags = w.Tags?.Split(',').ToList(),
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflows");
            return StatusCode(500, new { message = "An error occurred while retrieving workflows" });
        }
    }

    /// <summary>
    /// Get a specific workflow definition
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkflow(int id)
    {
        try
        {
            var workflow = await _workflowService.GetWorkflowDefinitionAsync(id);
            if (workflow == null) return NotFound(new { message = "Workflow not found" });

            var result = new WorkflowDefinitionDetailDto
            {
                Id = workflow.Id,
                WorkflowKey = workflow.WorkflowKey,
                Name = workflow.Name,
                Description = workflow.Description,
                Category = workflow.Category,
                EntityType = workflow.EntityType,
                Status = workflow.Status.ToString(),
                CurrentVersion = workflow.CurrentVersion,
                IconName = workflow.IconName,
                Color = workflow.Color,
                IsSystem = workflow.IsSystem,
                Priority = workflow.Priority,
                MaxConcurrentInstances = workflow.MaxConcurrentInstances,
                DefaultTimeoutHours = workflow.DefaultTimeoutHours,
                OwnerId = workflow.OwnerId,
                OwnerName = workflow.Owner != null ? $"{workflow.Owner.FirstName} {workflow.Owner.LastName}" : null,
                Tags = workflow.Tags?.Split(',').ToList(),
                Metadata = workflow.Metadata,
                CreatedAt = workflow.CreatedAt,
                UpdatedAt = workflow.UpdatedAt,
                Versions = workflow.Versions.Select(v => new WorkflowVersionSummaryDto
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    Label = v.Label,
                    Status = v.Status.ToString(),
                    PublishedAt = v.PublishedAt,
                    CreatedAt = v.CreatedAt
                }).OrderByDescending(v => v.VersionNumber).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the workflow" });
        }
    }

    /// <summary>
    /// Create a new workflow definition
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateWorkflow([FromBody] CreateWorkflowDto dto)
    {
        try
        {
            // Check for duplicate key
            var existing = await _workflowService.GetWorkflowByKeyAsync(dto.WorkflowKey);
            if (existing != null)
                return BadRequest(new { message = "A workflow with this key already exists" });

            var workflow = new WorkflowDefinition
            {
                WorkflowKey = dto.WorkflowKey,
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                EntityType = dto.EntityType,
                IconName = dto.IconName ?? "AccountTree",
                Color = dto.Color ?? "#6750A4",
                Priority = dto.Priority ?? 100,
                MaxConcurrentInstances = dto.MaxConcurrentInstances ?? 0,
                DefaultTimeoutHours = dto.DefaultTimeoutHours ?? 0,
                Tags = dto.Tags != null ? string.Join(",", dto.Tags) : null,
                Metadata = dto.Metadata,
                OwnerId = GetCurrentUserId()
            };

            var result = await _workflowService.CreateWorkflowDefinitionAsync(workflow);
            return CreatedAtAction(nameof(GetWorkflow), new { id = result.Id }, new { id = result.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow");
            return StatusCode(500, new { message = "An error occurred while creating the workflow" });
        }
    }

    /// <summary>
    /// Update a workflow definition
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateWorkflow(int id, [FromBody] UpdateWorkflowDto dto)
    {
        try
        {
            var workflow = await _context.WorkflowDefinitions.FindAsync(id);
            if (workflow == null) return NotFound(new { message = "Workflow not found" });
            if (workflow.IsSystem) return BadRequest(new { message = "Cannot modify system workflows" });

            workflow.Name = dto.Name ?? workflow.Name;
            workflow.Description = dto.Description;
            workflow.Category = dto.Category;
            workflow.EntityType = dto.EntityType ?? workflow.EntityType;
            workflow.IconName = dto.IconName ?? workflow.IconName;
            workflow.Color = dto.Color ?? workflow.Color;
            workflow.Priority = dto.Priority ?? workflow.Priority;
            workflow.MaxConcurrentInstances = dto.MaxConcurrentInstances ?? workflow.MaxConcurrentInstances;
            workflow.DefaultTimeoutHours = dto.DefaultTimeoutHours ?? workflow.DefaultTimeoutHours;
            workflow.Tags = dto.Tags != null ? string.Join(",", dto.Tags) : workflow.Tags;
            workflow.Metadata = dto.Metadata ?? workflow.Metadata;

            var result = await _workflowService.UpdateWorkflowDefinitionAsync(id, workflow);
            return Ok(new { message = "Workflow updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the workflow" });
        }
    }

    /// <summary>
    /// Delete a workflow definition
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteWorkflow(int id)
    {
        try
        {
            var success = await _workflowService.DeleteWorkflowDefinitionAsync(id);
            if (!success) return BadRequest(new { message = "Cannot delete this workflow" });
            return Ok(new { message = "Workflow deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the workflow" });
        }
    }

    /// <summary>
    /// Activate a workflow version
    /// </summary>
    [HttpPost("{id}/activate/{versionId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateWorkflow(int id, int versionId)
    {
        try
        {
            var success = await _workflowService.ActivateWorkflowAsync(id, versionId);
            if (!success) return BadRequest(new { message = "Cannot activate this workflow version" });
            return Ok(new { message = "Workflow activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating workflow {Id}", id);
            return StatusCode(500, new { message = "An error occurred while activating the workflow" });
        }
    }

    /// <summary>
    /// Pause a workflow
    /// </summary>
    [HttpPost("{id}/pause")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PauseWorkflow(int id)
    {
        try
        {
            var success = await _workflowService.PauseWorkflowAsync(id);
            if (!success) return BadRequest(new { message = "Cannot pause this workflow" });
            return Ok(new { message = "Workflow paused successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing workflow {Id}", id);
            return StatusCode(500, new { message = "An error occurred while pausing the workflow" });
        }
    }

    #endregion

    #region Workflow Versions

    /// <summary>
    /// Get a specific workflow version with full graph
    /// </summary>
    [HttpGet("versions/{versionId}")]
    public async Task<IActionResult> GetVersion(int versionId)
    {
        try
        {
            var version = await _workflowService.GetWorkflowVersionAsync(versionId);
            if (version == null) return NotFound(new { message = "Version not found" });

            var result = new WorkflowVersionDetailDto
            {
                Id = version.Id,
                WorkflowDefinitionId = version.WorkflowDefinitionId,
                WorkflowName = version.WorkflowDefinition.Name,
                VersionNumber = version.VersionNumber,
                Label = version.Label,
                ChangeLog = version.ChangeLog,
                Status = version.Status.ToString(),
                PublishedAt = version.PublishedAt,
                PublishedByName = version.PublishedBy != null 
                    ? $"{version.PublishedBy.FirstName} {version.PublishedBy.LastName}" 
                    : null,
                CanvasLayout = version.CanvasLayout,
                CreatedAt = version.CreatedAt,
                UpdatedAt = version.UpdatedAt,
                Nodes = version.Nodes.Select(n => new WorkflowNodeDto
                {
                    Id = n.Id,
                    NodeKey = n.NodeKey,
                    Name = n.Name,
                    Description = n.Description,
                    NodeType = n.NodeType.ToString(),
                    NodeSubType = n.NodeSubType,
                    PositionX = n.PositionX,
                    PositionY = n.PositionY,
                    Width = n.Width,
                    Height = n.Height,
                    IconName = n.IconName,
                    Color = n.Color,
                    IsStartNode = n.IsStartNode,
                    IsEndNode = n.IsEndNode,
                    Configuration = n.Configuration,
                    TimeoutMinutes = n.TimeoutMinutes,
                    RetryCount = n.RetryCount,
                    ExecutionOrder = n.ExecutionOrder
                }).ToList(),
                Transitions = version.Transitions.Select(t => new WorkflowTransitionDto
                {
                    Id = t.Id,
                    SourceNodeId = t.SourceNodeId,
                    TargetNodeId = t.TargetNodeId,
                    TransitionKey = t.TransitionKey,
                    Label = t.Label,
                    ConditionType = t.ConditionType.ToString(),
                    ConditionExpression = t.ConditionExpression,
                    IsDefault = t.IsDefault,
                    Priority = t.Priority,
                    SourceHandle = t.SourceHandle,
                    TargetHandle = t.TargetHandle,
                    LineStyle = t.LineStyle,
                    Color = t.Color,
                    AnimationStyle = t.AnimationStyle
                }).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving version {Id}", versionId);
            return StatusCode(500, new { message = "An error occurred while retrieving the version" });
        }
    }

    /// <summary>
    /// Create a new draft version
    /// </summary>
    [HttpPost("{workflowId}/versions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateVersion(int workflowId, [FromBody] CreateVersionDto? dto = null)
    {
        try
        {
            var version = await _workflowService.CreateNewVersionAsync(workflowId, dto?.SourceVersionId);
            return Ok(new { id = version.Id, versionNumber = version.VersionNumber });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating version for workflow {Id}", workflowId);
            return StatusCode(500, new { message = "An error occurred while creating the version" });
        }
    }

    /// <summary>
    /// Save canvas layout for a version
    /// </summary>
    [HttpPut("versions/{versionId}/layout")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SaveCanvasLayout(int versionId, [FromBody] SaveLayoutDto dto)
    {
        try
        {
            var success = await _workflowService.SaveCanvasLayoutAsync(versionId, dto.CanvasLayout);
            if (!success) return BadRequest(new { message = "Cannot update layout for this version" });
            return Ok(new { message = "Layout saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving layout for version {Id}", versionId);
            return StatusCode(500, new { message = "An error occurred while saving the layout" });
        }
    }

    #endregion

    #region Nodes

    /// <summary>
    /// Add a node to a workflow version
    /// </summary>
    [HttpPost("versions/{versionId}/nodes")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddNode(int versionId, [FromBody] CreateNodeDto dto)
    {
        try
        {
            if (!Enum.TryParse<WorkflowNodeType>(dto.NodeType, out var nodeType))
                return BadRequest(new { message = "Invalid node type" });

            var node = new WorkflowNode
            {
                WorkflowVersionId = versionId,
                NodeKey = dto.NodeKey ?? Guid.NewGuid().ToString("N")[..8],
                Name = dto.Name,
                Description = dto.Description,
                NodeType = nodeType,
                NodeSubType = dto.NodeSubType,
                PositionX = dto.PositionX,
                PositionY = dto.PositionY,
                Width = dto.Width ?? 200,
                Height = dto.Height ?? 80,
                IconName = dto.IconName ?? GetDefaultIconForNodeType(nodeType),
                Color = dto.Color ?? GetDefaultColorForNodeType(nodeType),
                IsStartNode = dto.IsStartNode,
                IsEndNode = dto.IsEndNode,
                Configuration = dto.Configuration,
                TimeoutMinutes = dto.TimeoutMinutes ?? 0,
                RetryCount = dto.RetryCount ?? 0,
                RetryDelaySeconds = dto.RetryDelaySeconds ?? 60,
                UseExponentialBackoff = dto.UseExponentialBackoff ?? true,
                ExecutionOrder = dto.ExecutionOrder ?? 0
            };

            var result = await _workflowService.AddNodeAsync(node);
            return Ok(new { id = result.Id, nodeKey = result.NodeKey });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding node to version {Id}", versionId);
            return StatusCode(500, new { message = "An error occurred while adding the node" });
        }
    }

    /// <summary>
    /// Update a node
    /// </summary>
    [HttpPut("nodes/{nodeId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNode(int nodeId, [FromBody] UpdateNodeDto dto)
    {
        try
        {
            var node = await _context.WorkflowNodes.FindAsync(nodeId);
            if (node == null) return NotFound(new { message = "Node not found" });

            if (!string.IsNullOrEmpty(dto.NodeType) && Enum.TryParse<WorkflowNodeType>(dto.NodeType, out var nodeType))
                node.NodeType = nodeType;

            node.Name = dto.Name ?? node.Name;
            node.Description = dto.Description ?? node.Description;
            node.NodeSubType = dto.NodeSubType ?? node.NodeSubType;
            node.PositionX = dto.PositionX ?? node.PositionX;
            node.PositionY = dto.PositionY ?? node.PositionY;
            node.Width = dto.Width ?? node.Width;
            node.Height = dto.Height ?? node.Height;
            node.IconName = dto.IconName ?? node.IconName;
            node.Color = dto.Color ?? node.Color;
            node.IsStartNode = dto.IsStartNode ?? node.IsStartNode;
            node.IsEndNode = dto.IsEndNode ?? node.IsEndNode;
            node.Configuration = dto.Configuration ?? node.Configuration;
            node.TimeoutMinutes = dto.TimeoutMinutes ?? node.TimeoutMinutes;
            node.RetryCount = dto.RetryCount ?? node.RetryCount;
            node.RetryDelaySeconds = dto.RetryDelaySeconds ?? node.RetryDelaySeconds;
            node.UseExponentialBackoff = dto.UseExponentialBackoff ?? node.UseExponentialBackoff;
            node.ExecutionOrder = dto.ExecutionOrder ?? node.ExecutionOrder;

            var result = await _workflowService.UpdateNodeAsync(nodeId, node);
            return Ok(new { message = "Node updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating node {Id}", nodeId);
            return StatusCode(500, new { message = "An error occurred while updating the node" });
        }
    }

    /// <summary>
    /// Delete a node
    /// </summary>
    [HttpDelete("nodes/{nodeId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteNode(int nodeId)
    {
        try
        {
            var success = await _workflowService.DeleteNodeAsync(nodeId);
            if (!success) return BadRequest(new { message = "Cannot delete this node" });
            return Ok(new { message = "Node deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting node {Id}", nodeId);
            return StatusCode(500, new { message = "An error occurred while deleting the node" });
        }
    }

    /// <summary>
    /// Bulk update node positions
    /// </summary>
    [HttpPut("versions/{versionId}/nodes/positions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNodePositions(int versionId, [FromBody] List<NodePositionDto> positions)
    {
        try
        {
            var positionDict = positions.ToDictionary(p => p.NodeId, p => (p.X, p.Y));
            await _workflowService.UpdateNodePositionsAsync(positionDict);
            return Ok(new { message = "Positions updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating node positions for version {Id}", versionId);
            return StatusCode(500, new { message = "An error occurred while updating positions" });
        }
    }

    #endregion

    #region Transitions

    /// <summary>
    /// Add a transition
    /// </summary>
    [HttpPost("versions/{versionId}/transitions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddTransition(int versionId, [FromBody] CreateTransitionDto dto)
    {
        try
        {
            TransitionConditionType conditionType = TransitionConditionType.Always;
            if (!string.IsNullOrEmpty(dto.ConditionType))
                Enum.TryParse(dto.ConditionType, out conditionType);

            var transition = new WorkflowTransition
            {
                WorkflowVersionId = versionId,
                SourceNodeId = dto.SourceNodeId,
                TargetNodeId = dto.TargetNodeId,
                TransitionKey = dto.TransitionKey,
                Label = dto.Label,
                Description = dto.Description,
                ConditionType = conditionType,
                ConditionExpression = dto.ConditionExpression,
                IsDefault = dto.IsDefault,
                Priority = dto.Priority ?? 100,
                SourceHandle = dto.SourceHandle ?? "right",
                TargetHandle = dto.TargetHandle ?? "left",
                LineStyle = dto.LineStyle ?? "solid",
                Color = dto.Color ?? "#888888",
                AnimationStyle = dto.AnimationStyle ?? "none"
            };

            var result = await _workflowService.AddTransitionAsync(transition);
            return Ok(new { id = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding transition to version {Id}", versionId);
            return StatusCode(500, new { message = "An error occurred while adding the transition" });
        }
    }

    /// <summary>
    /// Update a transition
    /// </summary>
    [HttpPut("transitions/{transitionId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTransition(int transitionId, [FromBody] UpdateTransitionDto dto)
    {
        try
        {
            var transition = await _context.WorkflowTransitions.FindAsync(transitionId);
            if (transition == null) return NotFound(new { message = "Transition not found" });

            if (!string.IsNullOrEmpty(dto.ConditionType) && 
                Enum.TryParse<TransitionConditionType>(dto.ConditionType, out var conditionType))
                transition.ConditionType = conditionType;

            transition.Label = dto.Label ?? transition.Label;
            transition.Description = dto.Description ?? transition.Description;
            transition.ConditionExpression = dto.ConditionExpression ?? transition.ConditionExpression;
            transition.IsDefault = dto.IsDefault ?? transition.IsDefault;
            transition.Priority = dto.Priority ?? transition.Priority;
            transition.SourceHandle = dto.SourceHandle ?? transition.SourceHandle;
            transition.TargetHandle = dto.TargetHandle ?? transition.TargetHandle;
            transition.LineStyle = dto.LineStyle ?? transition.LineStyle;
            transition.Color = dto.Color ?? transition.Color;
            transition.AnimationStyle = dto.AnimationStyle ?? transition.AnimationStyle;

            var result = await _workflowService.UpdateTransitionAsync(transitionId, transition);
            return Ok(new { message = "Transition updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transition {Id}", transitionId);
            return StatusCode(500, new { message = "An error occurred while updating the transition" });
        }
    }

    /// <summary>
    /// Delete a transition
    /// </summary>
    [HttpDelete("transitions/{transitionId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTransition(int transitionId)
    {
        try
        {
            var success = await _workflowService.DeleteTransitionAsync(transitionId);
            if (!success) return BadRequest(new { message = "Cannot delete this transition" });
            return Ok(new { message = "Transition deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transition {Id}", transitionId);
            return StatusCode(500, new { message = "An error occurred while deleting the transition" });
        }
    }

    #endregion

    #region Statistics & Lookups

    /// <summary>
    /// Get workflow statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var stats = await _workflowService.GetStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
        }
    }

    /// <summary>
    /// Get available entity types
    /// </summary>
    [HttpGet("entity-types")]
    public IActionResult GetEntityTypes()
    {
        var types = new[]
        {
            new { value = "Customer", label = "Customer" },
            new { value = "Lead", label = "Lead" },
            new { value = "Contact", label = "Contact" },
            new { value = "Opportunity", label = "Opportunity" },
            new { value = "Account", label = "Account" },
            new { value = "ServiceRequest", label = "Service Request" },
            new { value = "Quote", label = "Quote" },
            new { value = "Campaign", label = "Campaign" }
        };
        return Ok(types);
    }

    /// <summary>
    /// Get available node types
    /// </summary>
    [HttpGet("node-types")]
    public IActionResult GetNodeTypes()
    {
        var types = Enum.GetValues<WorkflowNodeType>()
            .Select(t => new 
            { 
                value = t.ToString(), 
                label = GetNodeTypeLabel(t),
                icon = GetDefaultIconForNodeType(t),
                color = GetDefaultColorForNodeType(t)
            })
            .ToList();
        return Ok(types);
    }

    /// <summary>
    /// Get workflow categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.WorkflowDefinitions
            .Where(w => !w.IsDeleted && w.Category != null)
            .Select(w => w.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }

    #endregion

    #region Helper Methods

    private string GetDefaultIconForNodeType(WorkflowNodeType nodeType)
    {
        return nodeType switch
        {
            WorkflowNodeType.Trigger => "PlayCircle",
            WorkflowNodeType.Condition => "CallSplit",
            WorkflowNodeType.Action => "FlashOn",
            WorkflowNodeType.HumanTask => "Person",
            WorkflowNodeType.Wait => "Schedule",
            WorkflowNodeType.ParallelGateway => "CallSplit",
            WorkflowNodeType.JoinGateway => "CallMerge",
            WorkflowNodeType.Subprocess => "AccountTree",
            WorkflowNodeType.LLMAction => "Psychology",
            WorkflowNodeType.End => "StopCircle",
            _ => "Circle"
        };
    }

    private string GetDefaultColorForNodeType(WorkflowNodeType nodeType)
    {
        return nodeType switch
        {
            WorkflowNodeType.Trigger => "#4CAF50",
            WorkflowNodeType.Condition => "#FF9800",
            WorkflowNodeType.Action => "#2196F3",
            WorkflowNodeType.HumanTask => "#9C27B0",
            WorkflowNodeType.Wait => "#607D8B",
            WorkflowNodeType.ParallelGateway => "#FF5722",
            WorkflowNodeType.JoinGateway => "#FF5722",
            WorkflowNodeType.Subprocess => "#795548",
            WorkflowNodeType.LLMAction => "#E91E63",
            WorkflowNodeType.End => "#F44336",
            _ => "#6750A4"
        };
    }

    private string GetNodeTypeLabel(WorkflowNodeType nodeType)
    {
        return nodeType switch
        {
            WorkflowNodeType.Trigger => "Trigger",
            WorkflowNodeType.Condition => "Condition",
            WorkflowNodeType.Action => "Action",
            WorkflowNodeType.HumanTask => "Human Task",
            WorkflowNodeType.Wait => "Wait/Timer",
            WorkflowNodeType.ParallelGateway => "Parallel Split",
            WorkflowNodeType.JoinGateway => "Parallel Join",
            WorkflowNodeType.Subprocess => "Subprocess",
            WorkflowNodeType.LLMAction => "AI/LLM Action",
            WorkflowNodeType.End => "End",
            _ => nodeType.ToString()
        };
    }

    #endregion
}

#region DTOs

public class WorkflowDefinitionDto
{
    public int Id { get; set; }
    public string WorkflowKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CurrentVersion { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public bool IsSystem { get; set; }
    public int Priority { get; set; }
    public int MaxConcurrentInstances { get; set; }
    public int DefaultTimeoutHours { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class WorkflowDefinitionDetailDto : WorkflowDefinitionDto
{
    public string? Metadata { get; set; }
    public List<WorkflowVersionSummaryDto> Versions { get; set; } = new();
}

public class WorkflowVersionSummaryDto
{
    public int Id { get; set; }
    public int VersionNumber { get; set; }
    public string? Label { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WorkflowVersionDetailDto : WorkflowVersionSummaryDto
{
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string? ChangeLog { get; set; }
    public string? PublishedByName { get; set; }
    public string? CanvasLayout { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<WorkflowNodeDto> Nodes { get; set; } = new();
    public List<WorkflowTransitionDto> Transitions { get; set; } = new();
}

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

public class CreateWorkflowDto
{
    public string WorkflowKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public int? Priority { get; set; }
    public int? MaxConcurrentInstances { get; set; }
    public int? DefaultTimeoutHours { get; set; }
    public List<string>? Tags { get; set; }
    public string? Metadata { get; set; }
}

public class UpdateWorkflowDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? EntityType { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public int? Priority { get; set; }
    public int? MaxConcurrentInstances { get; set; }
    public int? DefaultTimeoutHours { get; set; }
    public List<string>? Tags { get; set; }
    public string? Metadata { get; set; }
}

public class CreateVersionDto
{
    public int? SourceVersionId { get; set; }
}

public class SaveLayoutDto
{
    public string CanvasLayout { get; set; } = string.Empty;
}

public class CreateNodeDto
{
    public string? NodeKey { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string NodeType { get; set; } = string.Empty;
    public string? NodeSubType { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public bool IsStartNode { get; set; }
    public bool IsEndNode { get; set; }
    public string? Configuration { get; set; }
    public int? TimeoutMinutes { get; set; }
    public int? RetryCount { get; set; }
    public int? RetryDelaySeconds { get; set; }
    public bool? UseExponentialBackoff { get; set; }
    public int? ExecutionOrder { get; set; }
}

public class UpdateNodeDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? NodeType { get; set; }
    public string? NodeSubType { get; set; }
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
    public bool? IsStartNode { get; set; }
    public bool? IsEndNode { get; set; }
    public string? Configuration { get; set; }
    public int? TimeoutMinutes { get; set; }
    public int? RetryCount { get; set; }
    public int? RetryDelaySeconds { get; set; }
    public bool? UseExponentialBackoff { get; set; }
    public int? ExecutionOrder { get; set; }
}

public class NodePositionDto
{
    public int NodeId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}

public class CreateTransitionDto
{
    public int SourceNodeId { get; set; }
    public int TargetNodeId { get; set; }
    public string? TransitionKey { get; set; }
    public string? Label { get; set; }
    public string? Description { get; set; }
    public string? ConditionType { get; set; }
    public string? ConditionExpression { get; set; }
    public bool IsDefault { get; set; }
    public int? Priority { get; set; }
    public string? SourceHandle { get; set; }
    public string? TargetHandle { get; set; }
    public string? LineStyle { get; set; }
    public string? Color { get; set; }
    public string? AnimationStyle { get; set; }
}

public class UpdateTransitionDto
{
    public string? Label { get; set; }
    public string? Description { get; set; }
    public string? ConditionType { get; set; }
    public string? ConditionExpression { get; set; }
    public bool? IsDefault { get; set; }
    public int? Priority { get; set; }
    public string? SourceHandle { get; set; }
    public string? TargetHandle { get; set; }
    public string? LineStyle { get; set; }
    public string? Color { get; set; }
    public string? AnimationStyle { get; set; }
}

#endregion
