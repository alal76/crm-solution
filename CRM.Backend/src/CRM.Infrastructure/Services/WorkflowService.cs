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
/// Service for managing workflow definitions and versions
/// </summary>
public class WorkflowService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(CrmDbContext context, ILogger<WorkflowService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Workflow Definition Operations

    /// <summary>
    /// Get all workflow definitions with optional filtering
    /// </summary>
    public async Task<List<WorkflowDefinition>> GetWorkflowDefinitionsAsync(
        string? entityType = null,
        WorkflowStatus? status = null,
        string? category = null,
        string? search = null,
        int skip = 0,
        int take = 50)
    {
        var query = _context.WorkflowDefinitions
            .Include(w => w.Owner)
            .Where(w => !w.IsDeleted);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(w => w.EntityType == entityType);

        if (status.HasValue)
            query = query.Where(w => w.Status == status.Value);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(w => w.Category == category);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(w => w.Name.Contains(search) || (w.Description != null && w.Description.Contains(search)));

        return await query
            .OrderBy(w => w.Priority)
            .ThenBy(w => w.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <summary>
    /// Get a workflow definition by ID with full graph
    /// </summary>
    public async Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(int id)
    {
        return await _context.WorkflowDefinitions
            .Include(w => w.Owner)
            .Include(w => w.Versions.Where(v => !v.IsDeleted))
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }

    /// <summary>
    /// Get a workflow definition by key
    /// </summary>
    public async Task<WorkflowDefinition?> GetWorkflowByKeyAsync(string key)
    {
        return await _context.WorkflowDefinitions
            .Include(w => w.Owner)
            .FirstOrDefaultAsync(w => w.WorkflowKey == key && !w.IsDeleted);
    }

    /// <summary>
    /// Create a new workflow definition
    /// </summary>
    public async Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(WorkflowDefinition workflow)
    {
        workflow.CreatedAt = DateTime.UtcNow;
        workflow.CurrentVersion = 1;
        
        _context.WorkflowDefinitions.Add(workflow);
        await _context.SaveChangesAsync();
        
        // Create initial draft version
        var version = new WorkflowVersion
        {
            WorkflowDefinitionId = workflow.Id,
            VersionNumber = 1,
            Label = "v1.0",
            Status = WorkflowVersionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.WorkflowVersions.Add(version);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created workflow definition {WorkflowId}: {Name}", workflow.Id, workflow.Name);
        return workflow;
    }

    /// <summary>
    /// Update a workflow definition
    /// </summary>
    public async Task<WorkflowDefinition?> UpdateWorkflowDefinitionAsync(int id, WorkflowDefinition updates)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null || workflow.IsDeleted) return null;

        workflow.Name = updates.Name;
        workflow.Description = updates.Description;
        workflow.Category = updates.Category;
        workflow.EntityType = updates.EntityType;
        workflow.IconName = updates.IconName;
        workflow.Color = updates.Color;
        workflow.Priority = updates.Priority;
        workflow.MaxConcurrentInstances = updates.MaxConcurrentInstances;
        workflow.DefaultTimeoutHours = updates.DefaultTimeoutHours;
        workflow.Tags = updates.Tags;
        workflow.Metadata = updates.Metadata;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated workflow definition {WorkflowId}", id);
        return workflow;
    }

    /// <summary>
    /// Delete a workflow definition (soft delete)
    /// </summary>
    public async Task<bool> DeleteWorkflowDefinitionAsync(int id)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null || workflow.IsSystem) return false;

        workflow.IsDeleted = true;
        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted workflow definition {WorkflowId}", id);
        return true;
    }

    /// <summary>
    /// Activate a workflow
    /// </summary>
    public async Task<bool> ActivateWorkflowAsync(int id, int versionId)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        var version = await _context.WorkflowVersions.FindAsync(versionId);
        
        if (workflow == null || version == null || version.WorkflowDefinitionId != id) return false;

        // Deactivate any currently active version
        var activeVersions = await _context.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == id && v.Status == WorkflowVersionStatus.Active)
            .ToListAsync();
        
        foreach (var av in activeVersions)
        {
            av.Status = WorkflowVersionStatus.Deprecated;
            av.DeprecatedAt = DateTime.UtcNow;
        }

        // Activate the new version
        version.Status = WorkflowVersionStatus.Active;
        version.PublishedAt = DateTime.UtcNow;
        
        workflow.Status = WorkflowStatus.Active;
        workflow.CurrentVersion = version.VersionNumber;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Activated workflow {WorkflowId} version {VersionNumber}", id, version.VersionNumber);
        return true;
    }

    /// <summary>
    /// Pause a workflow
    /// </summary>
    public async Task<bool> PauseWorkflowAsync(int id)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null) return false;

        workflow.Status = WorkflowStatus.Paused;
        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Paused workflow {WorkflowId}", id);
        return true;
    }

    #endregion

    #region Workflow Version Operations

    /// <summary>
    /// Get a workflow version with full graph
    /// </summary>
    public async Task<WorkflowVersion?> GetWorkflowVersionAsync(int versionId)
    {
        return await _context.WorkflowVersions
            .Include(v => v.WorkflowDefinition)
            .Include(v => v.Nodes.Where(n => !n.IsDeleted))
            .Include(v => v.Transitions.Where(t => !t.IsDeleted))
            .Include(v => v.PublishedBy)
            .FirstOrDefaultAsync(v => v.Id == versionId && !v.IsDeleted);
    }

    /// <summary>
    /// Get the active version of a workflow
    /// </summary>
    public async Task<WorkflowVersion?> GetActiveVersionAsync(int workflowId)
    {
        return await _context.WorkflowVersions
            .Include(v => v.Nodes.Where(n => !n.IsDeleted))
            .Include(v => v.Transitions.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync(v => v.WorkflowDefinitionId == workflowId && v.Status == WorkflowVersionStatus.Active);
    }

    /// <summary>
    /// Get the draft version of a workflow
    /// </summary>
    public async Task<WorkflowVersion?> GetDraftVersionAsync(int workflowId)
    {
        return await _context.WorkflowVersions
            .Include(v => v.Nodes.Where(n => !n.IsDeleted))
            .Include(v => v.Transitions.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync(v => v.WorkflowDefinitionId == workflowId && v.Status == WorkflowVersionStatus.Draft);
    }

    /// <summary>
    /// Create a new draft version from an existing version
    /// </summary>
    public async Task<WorkflowVersion> CreateNewVersionAsync(int workflowId, int? sourceVersionId = null)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(workflowId);
        if (workflow == null) throw new ArgumentException("Workflow not found");

        // Get next version number
        var maxVersion = await _context.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == workflowId)
            .MaxAsync(v => (int?)v.VersionNumber) ?? 0;

        var newVersion = new WorkflowVersion
        {
            WorkflowDefinitionId = workflowId,
            VersionNumber = maxVersion + 1,
            Label = $"v{maxVersion + 1}.0",
            Status = WorkflowVersionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkflowVersions.Add(newVersion);
        await _context.SaveChangesAsync();

        // Clone from source version if specified
        if (sourceVersionId.HasValue)
        {
            var sourceVersion = await GetWorkflowVersionAsync(sourceVersionId.Value);
            if (sourceVersion != null)
            {
                await CloneVersionNodesAsync(sourceVersion, newVersion.Id);
            }
        }

        _logger.LogInformation("Created new version {VersionNumber} for workflow {WorkflowId}", newVersion.VersionNumber, workflowId);
        return newVersion;
    }

    /// <summary>
    /// Clone nodes and transitions from one version to another
    /// </summary>
    private async Task CloneVersionNodesAsync(WorkflowVersion source, int targetVersionId)
    {
        var nodeIdMapping = new Dictionary<int, int>();

        // Clone nodes
        foreach (var node in source.Nodes)
        {
            var newNode = new WorkflowNode
            {
                WorkflowVersionId = targetVersionId,
                NodeKey = node.NodeKey,
                Name = node.Name,
                Description = node.Description,
                NodeType = node.NodeType,
                NodeSubType = node.NodeSubType,
                PositionX = node.PositionX,
                PositionY = node.PositionY,
                Width = node.Width,
                Height = node.Height,
                IconName = node.IconName,
                Color = node.Color,
                IsStartNode = node.IsStartNode,
                IsEndNode = node.IsEndNode,
                Configuration = node.Configuration,
                TimeoutMinutes = node.TimeoutMinutes,
                RetryCount = node.RetryCount,
                RetryDelaySeconds = node.RetryDelaySeconds,
                UseExponentialBackoff = node.UseExponentialBackoff,
                ExecutionOrder = node.ExecutionOrder,
                CreatedAt = DateTime.UtcNow
            };
            _context.WorkflowNodes.Add(newNode);
            await _context.SaveChangesAsync();
            nodeIdMapping[node.Id] = newNode.Id;
        }

        // Clone transitions with updated node IDs
        foreach (var transition in source.Transitions)
        {
            var newTransition = new WorkflowTransition
            {
                WorkflowVersionId = targetVersionId,
                SourceNodeId = nodeIdMapping[transition.SourceNodeId],
                TargetNodeId = nodeIdMapping[transition.TargetNodeId],
                TransitionKey = transition.TransitionKey,
                Label = transition.Label,
                Description = transition.Description,
                ConditionType = transition.ConditionType,
                ConditionExpression = transition.ConditionExpression,
                IsDefault = transition.IsDefault,
                Priority = transition.Priority,
                SourceHandle = transition.SourceHandle,
                TargetHandle = transition.TargetHandle,
                LineStyle = transition.LineStyle,
                Color = transition.Color,
                AnimationStyle = transition.AnimationStyle,
                CreatedAt = DateTime.UtcNow
            };
            _context.WorkflowTransitions.Add(newTransition);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Save the canvas layout for a version
    /// </summary>
    public async Task<bool> SaveCanvasLayoutAsync(int versionId, string canvasLayout)
    {
        var version = await _context.WorkflowVersions.FindAsync(versionId);
        if (version == null || version.Status != WorkflowVersionStatus.Draft) return false;

        version.CanvasLayout = canvasLayout;
        version.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Node Operations

    /// <summary>
    /// Add a node to a workflow version
    /// </summary>
    public async Task<WorkflowNode> AddNodeAsync(WorkflowNode node)
    {
        var version = await _context.WorkflowVersions.FindAsync(node.WorkflowVersionId);
        if (version == null || version.Status != WorkflowVersionStatus.Draft)
            throw new InvalidOperationException("Can only add nodes to draft versions");

        node.CreatedAt = DateTime.UtcNow;
        _context.WorkflowNodes.Add(node);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Added node {NodeKey} to version {VersionId}", node.NodeKey, node.WorkflowVersionId);
        return node;
    }

    /// <summary>
    /// Update a node
    /// </summary>
    public async Task<WorkflowNode?> UpdateNodeAsync(int nodeId, WorkflowNode updates)
    {
        var node = await _context.WorkflowNodes
            .Include(n => n.WorkflowVersion)
            .FirstOrDefaultAsync(n => n.Id == nodeId);
            
        if (node == null) return null;
        if (node.WorkflowVersion.Status != WorkflowVersionStatus.Draft)
            throw new InvalidOperationException("Can only update nodes in draft versions");

        node.Name = updates.Name;
        node.Description = updates.Description;
        node.NodeType = updates.NodeType;
        node.NodeSubType = updates.NodeSubType;
        node.PositionX = updates.PositionX;
        node.PositionY = updates.PositionY;
        node.Width = updates.Width;
        node.Height = updates.Height;
        node.IconName = updates.IconName;
        node.Color = updates.Color;
        node.IsStartNode = updates.IsStartNode;
        node.IsEndNode = updates.IsEndNode;
        node.Configuration = updates.Configuration;
        node.TimeoutMinutes = updates.TimeoutMinutes;
        node.RetryCount = updates.RetryCount;
        node.RetryDelaySeconds = updates.RetryDelaySeconds;
        node.UseExponentialBackoff = updates.UseExponentialBackoff;
        node.ExecutionOrder = updates.ExecutionOrder;
        node.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return node;
    }

    /// <summary>
    /// Delete a node
    /// </summary>
    public async Task<bool> DeleteNodeAsync(int nodeId)
    {
        var node = await _context.WorkflowNodes
            .Include(n => n.WorkflowVersion)
            .FirstOrDefaultAsync(n => n.Id == nodeId);
            
        if (node == null) return false;
        if (node.WorkflowVersion.Status != WorkflowVersionStatus.Draft)
            throw new InvalidOperationException("Can only delete nodes from draft versions");

        // Delete connected transitions
        var transitions = await _context.WorkflowTransitions
            .Where(t => t.SourceNodeId == nodeId || t.TargetNodeId == nodeId)
            .ToListAsync();
        _context.WorkflowTransitions.RemoveRange(transitions);
        
        node.IsDeleted = true;
        node.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted node {NodeId}", nodeId);
        return true;
    }

    /// <summary>
    /// Update node positions (bulk)
    /// </summary>
    public async Task UpdateNodePositionsAsync(Dictionary<int, (double x, double y)> positions)
    {
        foreach (var (nodeId, (x, y)) in positions)
        {
            var node = await _context.WorkflowNodes.FindAsync(nodeId);
            if (node != null)
            {
                node.PositionX = x;
                node.PositionY = y;
                node.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Transition Operations

    /// <summary>
    /// Add a transition
    /// </summary>
    public async Task<WorkflowTransition> AddTransitionAsync(WorkflowTransition transition)
    {
        var version = await _context.WorkflowVersions.FindAsync(transition.WorkflowVersionId);
        if (version == null || version.Status != WorkflowVersionStatus.Draft)
            throw new InvalidOperationException("Can only add transitions to draft versions");

        transition.CreatedAt = DateTime.UtcNow;
        _context.WorkflowTransitions.Add(transition);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Added transition from {Source} to {Target}", transition.SourceNodeId, transition.TargetNodeId);
        return transition;
    }

    /// <summary>
    /// Update a transition
    /// </summary>
    public async Task<WorkflowTransition?> UpdateTransitionAsync(int transitionId, WorkflowTransition updates)
    {
        var transition = await _context.WorkflowTransitions
            .Include(t => t.WorkflowVersion)
            .FirstOrDefaultAsync(t => t.Id == transitionId);
            
        if (transition == null) return null;
        if (transition.WorkflowVersion.Status != WorkflowVersionStatus.Draft)
            throw new InvalidOperationException("Can only update transitions in draft versions");

        transition.Label = updates.Label;
        transition.Description = updates.Description;
        transition.ConditionType = updates.ConditionType;
        transition.ConditionExpression = updates.ConditionExpression;
        transition.IsDefault = updates.IsDefault;
        transition.Priority = updates.Priority;
        transition.SourceHandle = updates.SourceHandle;
        transition.TargetHandle = updates.TargetHandle;
        transition.LineStyle = updates.LineStyle;
        transition.Color = updates.Color;
        transition.AnimationStyle = updates.AnimationStyle;
        transition.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return transition;
    }

    /// <summary>
    /// Delete a transition
    /// </summary>
    public async Task<bool> DeleteTransitionAsync(int transitionId)
    {
        var transition = await _context.WorkflowTransitions
            .Include(t => t.WorkflowVersion)
            .FirstOrDefaultAsync(t => t.Id == transitionId);
            
        if (transition == null) return false;
        if (transition.WorkflowVersion.Status != WorkflowVersionStatus.Draft)
            throw new InvalidOperationException("Can only delete transitions from draft versions");

        transition.IsDeleted = true;
        transition.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted transition {TransitionId}", transitionId);
        return true;
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get workflow statistics
    /// </summary>
    public async Task<WorkflowStatistics> GetStatisticsAsync()
    {
        var stats = new WorkflowStatistics
        {
            TotalWorkflows = await _context.WorkflowDefinitions.CountAsync(w => !w.IsDeleted),
            ActiveWorkflows = await _context.WorkflowDefinitions.CountAsync(w => !w.IsDeleted && w.Status == WorkflowStatus.Active),
            DraftWorkflows = await _context.WorkflowDefinitions.CountAsync(w => !w.IsDeleted && w.Status == WorkflowStatus.Draft),
            TotalInstances = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted),
            RunningInstances = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Running),
            CompletedInstances = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Completed),
            FailedInstances = await _context.WorkflowInstances.CountAsync(i => !i.IsDeleted && i.Status == WorkflowInstanceStatus.Failed),
            PendingTasks = await _context.WorkflowTasks.CountAsync(t => !t.IsDeleted && t.Status == WorkflowTaskStatus.Pending),
            DeadLetterTasks = await _context.WorkflowTasks.CountAsync(t => !t.IsDeleted && t.IsDeadLetter)
        };

        // Get category breakdown
        stats.WorkflowsByCategory = await _context.WorkflowDefinitions
            .Where(w => !w.IsDeleted && w.Category != null)
            .GroupBy(w => w.Category!)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count);

        // Get entity type breakdown
        stats.WorkflowsByEntityType = await _context.WorkflowDefinitions
            .Where(w => !w.IsDeleted)
            .GroupBy(w => w.EntityType)
            .Select(g => new { EntityType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EntityType, x => x.Count);

        return stats;
    }

    #endregion
}

/// <summary>
/// Workflow statistics model
/// </summary>
public class WorkflowStatistics
{
    public int TotalWorkflows { get; set; }
    public int ActiveWorkflows { get; set; }
    public int DraftWorkflows { get; set; }
    public int TotalInstances { get; set; }
    public int RunningInstances { get; set; }
    public int CompletedInstances { get; set; }
    public int FailedInstances { get; set; }
    public int PendingTasks { get; set; }
    public int DeadLetterTasks { get; set; }
    public Dictionary<string, int> WorkflowsByCategory { get; set; } = new();
    public Dictionary<string, int> WorkflowsByEntityType { get; set; } = new();
}
