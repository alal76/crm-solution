// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities.Workflow;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
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
    private readonly ILLMService _llmService;
    private readonly ILLMSettingsService _llmSettingsService;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        CrmDbContext context,
        WorkflowService workflowService,
        ILLMService llmService,
        ILLMSettingsService llmSettingsService,
        ILogger<WorkflowController> logger)
    {
        _context = context;
        _workflowService = workflowService;
        _llmService = llmService;
        _llmSettingsService = llmSettingsService;
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
    /// Get comprehensive workflow configuration for frontend
    /// </summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetWorkflowConfig()
    {
        var categories = await _context.WorkflowDefinitions
            .Where(w => !w.IsDeleted && w.Category != null)
            .Select(w => w.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        // Get roles from the UserRole enum
        var roles = Enum.GetValues<CRM.Core.Entities.UserRole>()
            .Select(r => new ConfigOption { Value = r.ToString(), Label = GetRoleLabel(r) })
            .ToList();

        var config = new WorkflowConfigResponse
        {
            EntityTypes = GetEntityTypesInternal(),
            NodeTypes = GetNodeTypesInternal(),
            ActionTypes = GetActionTypesInternal(),
            TriggerTypes = GetTriggerTypesInternal(),
            ConditionOperators = GetConditionOperatorsInternal(),
            StatusOptions = GetStatusOptionsInternal(),
            LLMProviders = _llmService.GetAvailableProviders(),
            LLMModels = _llmService.GetAvailableModels(),
            Roles = roles,
            Categories = categories.Where(c => c != null).Select(c => c!).ToList(),
            IconOptions = GetIconOptionsInternal(),
            ColorOptions = GetColorOptionsInternal(),
            FallbackActions = GetFallbackActionsInternal(),
            EventTypes = GetEventTypesInternal()
        };

        return Ok(config);
    }

    #region LLM Settings Management

    /// <summary>
    /// Get LLM provider settings (database settings merged with config defaults)
    /// </summary>
    [HttpGet("llm-settings")]
    public async Task<IActionResult> GetLLMSettings()
    {
        try
        {
            var settings = await _llmSettingsService.GetSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LLM settings");
            return StatusCode(500, new { message = "An error occurred while retrieving LLM settings" });
        }
    }

    /// <summary>
    /// Update LLM provider settings (stores in database)
    /// </summary>
    [HttpPut("llm-settings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateLLMSettings([FromBody] UpdateLLMSettingsRequest request)
    {
        try
        {
            var settings = await _llmSettingsService.UpdateSettingsAsync(request);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating LLM settings");
            return StatusCode(500, new { message = "An error occurred while updating LLM settings" });
        }
    }

    /// <summary>
    /// Reset LLM settings to defaults from appsettings.json
    /// </summary>
    [HttpPost("llm-settings/reset")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetLLMSettings()
    {
        try
        {
            await _llmSettingsService.ResetToDefaultsAsync();
            var settings = await _llmSettingsService.GetSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting LLM settings");
            return StatusCode(500, new { message = "An error occurred while resetting LLM settings" });
        }
    }

    /// <summary>
    /// Initialize default LLM settings in database (admin only)
    /// </summary>
    [HttpPost("llm-settings/initialize")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> InitializeLLMSettings()
    {
        try
        {
            await _llmSettingsService.InitializeDefaultSettingsAsync();
            var settings = await _llmSettingsService.GetSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing LLM settings");
            return StatusCode(500, new { message = "An error occurred while initializing LLM settings" });
        }
    }

    #endregion

    private static string GetRoleLabel(CRM.Core.Entities.UserRole role) => role switch
    {
        CRM.Core.Entities.UserRole.Admin => "Administrator",
        CRM.Core.Entities.UserRole.Manager => "Manager",
        CRM.Core.Entities.UserRole.Sales => "Sales Representative",
        CRM.Core.Entities.UserRole.Support => "Support Agent",
        CRM.Core.Entities.UserRole.Guest => "Guest",
        _ => role.ToString()
    };

    private List<ConfigOption> GetEntityTypesInternal() => new()
    {
        new() { Value = "Customer", Label = "Customer" },
        new() { Value = "Lead", Label = "Lead" },
        new() { Value = "Contact", Label = "Contact" },
        new() { Value = "Opportunity", Label = "Opportunity" },
        new() { Value = "Account", Label = "Account" },
        new() { Value = "ServiceRequest", Label = "Service Request" },
        new() { Value = "Quote", Label = "Quote" },
        new() { Value = "Campaign", Label = "Campaign" }
    };

    private List<NodeTypeConfig> GetNodeTypesInternal() => Enum.GetValues<WorkflowNodeType>()
        .Select(t => new NodeTypeConfig
        {
            Value = t.ToString(),
            Label = GetNodeTypeLabel(t),
            Icon = GetDefaultIconForNodeType(t),
            Color = GetDefaultColorForNodeType(t),
            Description = GetNodeTypeDescription(t)
        }).ToList();

    private List<ActionTypeConfig> GetActionTypesInternal() => new()
    {
        new() { Value = "log", Label = "Log Message", Category = "Debug", Icon = "Message" },
        new() { Value = "updateEntity", Label = "Update Entity", Category = "Data", Icon = "Edit" },
        new() { Value = "createEntity", Label = "Create Entity", Category = "Data", Icon = "Add" },
        new() { Value = "deleteEntity", Label = "Delete Entity", Category = "Data", Icon = "Delete" },
        new() { Value = "sendEmail", Label = "Send Email", Category = "Communication", Icon = "Email" },
        new() { Value = "sendNotification", Label = "Send Notification", Category = "Communication", Icon = "Notifications" },
        new() { Value = "sendSMS", Label = "Send SMS", Category = "Communication", Icon = "Sms" },
        new() { Value = "webhook", Label = "Call Webhook", Category = "Integration", Icon = "Http" },
        new() { Value = "calculateField", Label = "Calculate Field", Category = "Data", Icon = "Calculate" },
        new() { Value = "assignUser", Label = "Assign User", Category = "Assignment", Icon = "PersonAdd" },
        new() { Value = "assignTeam", Label = "Assign to Team", Category = "Assignment", Icon = "Group" },
        new() { Value = "addToCampaign", Label = "Add to Campaign", Category = "Marketing", Icon = "Campaign" },
        new() { Value = "removeFromCampaign", Label = "Remove from Campaign", Category = "Marketing", Icon = "RemoveCircle" },
        new() { Value = "validateEntity", Label = "Validate Entity", Category = "Validation", Icon = "CheckCircle" },
        new() { Value = "createTask", Label = "Create Task", Category = "Tasks", Icon = "Task" },
        new() { Value = "createServiceRequest", Label = "Create Service Request", Category = "Service", Icon = "Support" },
        new() { Value = "escalate", Label = "Escalate", Category = "Service", Icon = "PriorityHigh" },
        new() { Value = "convertLead", Label = "Convert Lead", Category = "Sales", Icon = "Transform" },
        new() { Value = "createQuote", Label = "Create Quote", Category = "Sales", Icon = "RequestQuote" },
        new() { Value = "approvalRequest", Label = "Request Approval", Category = "Approval", Icon = "Approval" },
        new() { Value = "scheduleFollowUp", Label = "Schedule Follow-Up", Category = "Tasks", Icon = "EventRepeat" },
        new() { Value = "runScript", Label = "Run Custom Script", Category = "Advanced", Icon = "Code" },
        new() { Value = "callLLM", Label = "AI/LLM Action", Category = "AI", Icon = "Psychology" }
    };

    private List<TriggerTypeConfig> GetTriggerTypesInternal() => new()
    {
        new() { Value = "onCreate", Label = "On Create", Description = "Triggered when an entity is created", Icon = "Add" },
        new() { Value = "onUpdate", Label = "On Update", Description = "Triggered when an entity is updated", Icon = "Edit" },
        new() { Value = "onDelete", Label = "On Delete", Description = "Triggered when an entity is deleted", Icon = "Delete" },
        new() { Value = "onFieldChange", Label = "On Field Change", Description = "Triggered when specific field changes", Icon = "SwapHoriz" },
        new() { Value = "onStatusChange", Label = "On Status Change", Description = "Triggered when status changes", Icon = "ChangeCircle" },
        new() { Value = "onSchedule", Label = "Scheduled", Description = "Triggered on a schedule (cron)", Icon = "Schedule" },
        new() { Value = "onManual", Label = "Manual", Description = "Triggered manually by user", Icon = "TouchApp" },
        new() { Value = "onWebhook", Label = "On Webhook", Description = "Triggered by external webhook", Icon = "Http" },
        new() { Value = "onApproval", Label = "On Approval", Description = "Triggered when approval is granted", Icon = "Approval" },
        new() { Value = "onRejection", Label = "On Rejection", Description = "Triggered when request is rejected", Icon = "Cancel" },
        new() { Value = "onEscalation", Label = "On Escalation", Description = "Triggered when escalation occurs", Icon = "PriorityHigh" },
        new() { Value = "onAssignment", Label = "On Assignment", Description = "Triggered when entity is assigned", Icon = "PersonAdd" }
    };

    private List<OperatorConfig> GetConditionOperatorsInternal() => new()
    {
        new() { Value = "equals", Label = "equals", AppliesTo = new[] { "string", "number", "boolean", "date" } },
        new() { Value = "notEquals", Label = "does not equal", AppliesTo = new[] { "string", "number", "boolean", "date" } },
        new() { Value = "contains", Label = "contains", AppliesTo = new[] { "string" } },
        new() { Value = "notContains", Label = "does not contain", AppliesTo = new[] { "string" } },
        new() { Value = "startsWith", Label = "starts with", AppliesTo = new[] { "string" } },
        new() { Value = "endsWith", Label = "ends with", AppliesTo = new[] { "string" } },
        new() { Value = "greaterThan", Label = "is greater than", AppliesTo = new[] { "number", "date" } },
        new() { Value = "lessThan", Label = "is less than", AppliesTo = new[] { "number", "date" } },
        new() { Value = "greaterThanOrEqual", Label = "is greater than or equal", AppliesTo = new[] { "number", "date" } },
        new() { Value = "lessThanOrEqual", Label = "is less than or equal", AppliesTo = new[] { "number", "date" } },
        new() { Value = "isNull", Label = "is empty", AppliesTo = new[] { "string", "number", "date" } },
        new() { Value = "isNotNull", Label = "is not empty", AppliesTo = new[] { "string", "number", "date" } },
        new() { Value = "in", Label = "is in list", AppliesTo = new[] { "string", "number" } },
        new() { Value = "notIn", Label = "is not in list", AppliesTo = new[] { "string", "number" } },
        new() { Value = "between", Label = "is between", AppliesTo = new[] { "number", "date" } },
        new() { Value = "regex", Label = "matches pattern", AppliesTo = new[] { "string" } }
    };

    private List<StatusConfig> GetStatusOptionsInternal() => new()
    {
        new() { Value = "Draft", Label = "Draft", Color = "#9E9E9E", BgColor = "#F5F5F5", Icon = "Edit" },
        new() { Value = "Active", Label = "Active", Color = "#4CAF50", BgColor = "#E8F5E9", Icon = "PlayCircle" },
        new() { Value = "Paused", Label = "Paused", Color = "#FF9800", BgColor = "#FFF3E0", Icon = "Pause" },
        new() { Value = "Archived", Label = "Archived", Color = "#607D8B", BgColor = "#ECEFF1", Icon = "Archive" },
        new() { Value = "Pending", Label = "Pending", Color = "#2196F3", BgColor = "#E3F2FD", Icon = "Schedule" },
        new() { Value = "Running", Label = "Running", Color = "#4CAF50", BgColor = "#E8F5E9", Icon = "DirectionsRun" },
        new() { Value = "Completed", Label = "Completed", Color = "#4CAF50", BgColor = "#E8F5E9", Icon = "CheckCircle" },
        new() { Value = "Failed", Label = "Failed", Color = "#F44336", BgColor = "#FFEBEE", Icon = "Error" },
        new() { Value = "Cancelled", Label = "Cancelled", Color = "#9E9E9E", BgColor = "#F5F5F5", Icon = "Cancel" },
        new() { Value = "Suspended", Label = "Suspended", Color = "#FF9800", BgColor = "#FFF3E0", Icon = "PauseCircle" }
    };

    private List<string> GetIconOptionsInternal() => new()
    {
        "AccountTree", "Timeline", "CallSplit", "CompareArrows", "DeviceHub",
        "Schema", "Hub", "Shuffle", "Route", "AltRoute", "LinearScale", 
        "Workspaces", "Category", "Settings", "AutoAwesome", "PlayCircle",
        "StopCircle", "FlashOn", "Person", "Schedule", "Psychology",
        "Email", "Notifications", "Http", "Calculate", "Add", "Edit", "Delete"
    };

    private List<string> GetColorOptionsInternal() => new()
    {
        "#6750A4", "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", "#F44336",
        "#00BCD4", "#795548", "#607D8B", "#E91E63", "#3F51B5", "#009688",
        "#673AB7", "#8BC34A", "#03A9F4", "#FFC107", "#FF5722", "#CDDC39"
    };

    private List<ConfigOption> GetFallbackActionsInternal() => new()
    {
        new() { Value = "none", Label = "None - Stop Workflow" },
        new() { Value = "continue", Label = "Continue - Skip Failed Step" },
        new() { Value = "retry", Label = "Retry - Retry Failed Step" },
        new() { Value = "goto", Label = "Goto - Jump to Specific Node" },
        new() { Value = "default", Label = "Default - Use Default Value" },
        new() { Value = "manual", Label = "Manual - Require Human Review" }
    };

    private List<EventTypeConfig> GetEventTypesInternal() => new()
    {
        new() { Value = "WorkflowCreated", Label = "Created", Color = "success", Category = "Lifecycle" },
        new() { Value = "WorkflowUpdated", Label = "Updated", Color = "info", Category = "Lifecycle" },
        new() { Value = "WorkflowPublished", Label = "Published", Color = "success", Category = "Lifecycle" },
        new() { Value = "WorkflowActivated", Label = "Activated", Color = "success", Category = "Lifecycle" },
        new() { Value = "WorkflowPaused", Label = "Paused", Color = "warning", Category = "Lifecycle" },
        new() { Value = "WorkflowArchived", Label = "Archived", Color = "default", Category = "Lifecycle" },
        new() { Value = "NodeAdded", Label = "Node Added", Color = "info", Category = "Design" },
        new() { Value = "NodeUpdated", Label = "Node Updated", Color = "info", Category = "Design" },
        new() { Value = "NodeRemoved", Label = "Node Removed", Color = "warning", Category = "Design" },
        new() { Value = "TransitionAdded", Label = "Transition Added", Color = "info", Category = "Design" },
        new() { Value = "TransitionRemoved", Label = "Transition Removed", Color = "warning", Category = "Design" },
        new() { Value = "InstanceStarted", Label = "Instance Started", Color = "info", Category = "Execution" },
        new() { Value = "InstanceCompleted", Label = "Instance Completed", Color = "success", Category = "Execution" },
        new() { Value = "InstanceFailed", Label = "Instance Failed", Color = "error", Category = "Execution" },
        new() { Value = "StepExecuted", Label = "Step Executed", Color = "info", Category = "Execution" },
        new() { Value = "StepFailed", Label = "Step Failed", Color = "error", Category = "Execution" },
        new() { Value = "VersionCreated", Label = "Version Created", Color = "info", Category = "Versioning" },
        new() { Value = "VersionRolledBack", Label = "Version Rolled Back", Color = "warning", Category = "Versioning" },
        new() { Value = "PermissionChanged", Label = "Permission Changed", Color = "warning", Category = "Security" },
        new() { Value = "ConfigurationChanged", Label = "Configuration Changed", Color = "info", Category = "Configuration" }
    };

    private string GetNodeTypeDescription(WorkflowNodeType nodeType) => nodeType switch
    {
        WorkflowNodeType.Trigger => "Start the workflow based on an event or condition",
        WorkflowNodeType.Condition => "Branch the workflow based on conditions",
        WorkflowNodeType.Action => "Perform an automated action",
        WorkflowNodeType.HumanTask => "Require human intervention or approval",
        WorkflowNodeType.Wait => "Wait for a specified time or event",
        WorkflowNodeType.ParallelGateway => "Split into parallel execution paths",
        WorkflowNodeType.JoinGateway => "Wait for parallel paths to complete",
        WorkflowNodeType.Subprocess => "Execute another workflow as a step",
        WorkflowNodeType.LLMAction => "Execute an AI/LLM powered action",
        WorkflowNodeType.End => "End the workflow",
        // AI-Enhanced Node Types
        WorkflowNodeType.AIDecision => "Route workflow based on AI analysis of content",
        WorkflowNodeType.AIAgent => "Autonomous AI agent with tool access",
        WorkflowNodeType.AIContentGenerator => "Generate emails, summaries, reports using AI",
        WorkflowNodeType.AIDataExtractor => "Extract structured data from unstructured text",
        WorkflowNodeType.AIClassifier => "Categorize and tag content using AI",
        WorkflowNodeType.AISentimentAnalyzer => "Analyze sentiment and emotion in text",
        WorkflowNodeType.HumanReview => "Human-in-the-loop review for AI outputs",
        _ => ""
    };

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
            // AI-Enhanced Node Types
            WorkflowNodeType.AIDecision => "Route",
            WorkflowNodeType.AIAgent => "SmartToy",
            WorkflowNodeType.AIContentGenerator => "AutoAwesome",
            WorkflowNodeType.AIDataExtractor => "DataObject",
            WorkflowNodeType.AIClassifier => "Category",
            WorkflowNodeType.AISentimentAnalyzer => "SentimentSatisfied",
            WorkflowNodeType.HumanReview => "RateReview",
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
            // AI-Enhanced Node Types
            WorkflowNodeType.AIDecision => "#00BCD4",
            WorkflowNodeType.AIAgent => "#673AB7",
            WorkflowNodeType.AIContentGenerator => "#3F51B5",
            WorkflowNodeType.AIDataExtractor => "#009688",
            WorkflowNodeType.AIClassifier => "#8BC34A",
            WorkflowNodeType.AISentimentAnalyzer => "#FFEB3B",
            WorkflowNodeType.HumanReview => "#FF5722",
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
            // AI-Enhanced Node Types
            WorkflowNodeType.AIDecision => "AI Decision",
            WorkflowNodeType.AIAgent => "AI Agent",
            WorkflowNodeType.AIContentGenerator => "AI Content Generator",
            WorkflowNodeType.AIDataExtractor => "AI Data Extractor",
            WorkflowNodeType.AIClassifier => "AI Classifier",
            WorkflowNodeType.AISentimentAnalyzer => "AI Sentiment Analyzer",
            WorkflowNodeType.HumanReview => "Human Review",
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

/// <summary>
/// Comprehensive workflow configuration response
/// </summary>
public class WorkflowConfigResponse
{
    public List<ConfigOption> EntityTypes { get; set; } = new();
    public List<NodeTypeConfig> NodeTypes { get; set; } = new();
    public List<ActionTypeConfig> ActionTypes { get; set; } = new();
    public List<TriggerTypeConfig> TriggerTypes { get; set; } = new();
    public List<OperatorConfig> ConditionOperators { get; set; } = new();
    public List<StatusConfig> StatusOptions { get; set; } = new();
    public List<LLMProviderInfo> LLMProviders { get; set; } = new();
    public List<LLMModelInfo> LLMModels { get; set; } = new();
    public List<ConfigOption> Roles { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<string> IconOptions { get; set; } = new();
    public List<string> ColorOptions { get; set; } = new();
    public List<ConfigOption> FallbackActions { get; set; } = new();
    public List<EventTypeConfig> EventTypes { get; set; } = new();
}

public class ConfigOption
{
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";
}

public class NodeTypeConfig : ConfigOption
{
    public string Icon { get; set; } = "";
    public string Color { get; set; } = "";
    public string Description { get; set; } = "";
}

public class ActionTypeConfig : ConfigOption
{
    public string Category { get; set; } = "";
    public string Icon { get; set; } = "";
}

public class TriggerTypeConfig : ConfigOption
{
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
}

public class OperatorConfig : ConfigOption
{
    public string[] AppliesTo { get; set; } = Array.Empty<string>();
}

public class StatusConfig : ConfigOption
{
    public string Color { get; set; } = "";
    public string BgColor { get; set; } = "";
    public string Icon { get; set; } = "";
}

public class EventTypeConfig : ConfigOption
{
    public string Color { get; set; } = "";
    public string Category { get; set; } = "";
}

#endregion
