// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing workflow definitions and versions
/// </summary>
public class WorkflowService : IWorkflowService
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
        if (workflow == null || workflow.IsDeleted)
            return null;

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
    /// Clone an entire workflow definition including its active/latest version, nodes, and transitions.
    /// </summary>
    public async Task<WorkflowDefinition> CloneWorkflowAsync(int sourceWorkflowId, string? newName = null, int? ownerId = null)
    {
        var source = await _context.WorkflowDefinitions
            .Include(w => w.Versions.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Nodes.Where(n => !n.IsDeleted))
            .Include(w => w.Versions.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Transitions.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync(w => w.Id == sourceWorkflowId && !w.IsDeleted);

        if (source == null)
            throw new KeyNotFoundException($"Workflow definition with ID {sourceWorkflowId} not found");

        // Find the best version to clone: active first, then latest draft
        var sourceVersion = source.Versions
            .Where(v => v.Status == WorkflowVersionStatus.Active)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefault()
            ?? source.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();

        // Generate a unique key
        var baseKey = $"{source.WorkflowKey}-copy";
        var suffix = 1;
        var newKey = baseKey;
        while (await _context.WorkflowDefinitions.AnyAsync(w => w.WorkflowKey == newKey && !w.IsDeleted))
        {
            newKey = $"{baseKey}-{suffix++}";
        }

        // Create the cloned definition
        var cloned = new WorkflowDefinition
        {
            WorkflowKey = newKey,
            Name = newName ?? $"{source.Name} (Copy)",
            Description = source.Description,
            Category = source.Category,
            EntityType = source.EntityType,
            Status = WorkflowStatus.Draft,
            CurrentVersion = 1,
            IconName = source.IconName,
            Color = source.Color,
            IsSystem = false,
            Priority = source.Priority,
            MaxConcurrentInstances = source.MaxConcurrentInstances,
            DefaultTimeoutHours = source.DefaultTimeoutHours,
            OwnerId = ownerId,
            Tags = source.Tags,
            Metadata = source.Metadata,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkflowDefinitions.Add(cloned);
        await _context.SaveChangesAsync();

        // Create an initial draft version
        var clonedVersion = new WorkflowVersion
        {
            WorkflowDefinitionId = cloned.Id,
            VersionNumber = 1,
            Label = "v1.0",
            ChangeLog = $"Cloned from \"{source.Name}\" (ID {source.Id})",
            Status = WorkflowVersionStatus.Draft,
            CanvasLayout = sourceVersion?.CanvasLayout,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkflowVersions.Add(clonedVersion);
        await _context.SaveChangesAsync();

        // Clone nodes and transitions from the source version
        if (sourceVersion != null)
        {
            await CloneVersionNodesAsync(sourceVersion, clonedVersion.Id);
        }

        _logger.LogInformation(
            "Cloned workflow {SourceId} ({SourceName}) → {ClonedId} ({ClonedName})",
            source.Id, source.Name, cloned.Id, cloned.Name);

        return cloned;
    }

    /// <summary>
    /// Delete a workflow definition (soft delete)
    /// </summary>
    public async Task<bool> DeleteWorkflowDefinitionAsync(int id)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null || workflow.IsSystem)
            return false;

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

        if (workflow == null || version == null || version.WorkflowDefinitionId != id)
            return false;

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
        if (workflow == null)
            return false;

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
        if (workflow == null)
            throw new ArgumentException("Workflow not found");

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
        if (version == null || version.Status != WorkflowVersionStatus.Draft)
            return false;

        version.CanvasLayout = canvasLayout;
        version.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Get all versions for a workflow
    /// </summary>
    public async Task<List<WorkflowVersion>> GetVersionsAsync(int workflowId)
    {
        return await _context.WorkflowVersions
            .Include(v => v.PublishedBy)
            .Where(v => v.WorkflowDefinitionId == workflowId && !v.IsDeleted)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Update a draft version's metadata
    /// </summary>
    public async Task<WorkflowVersion?> UpdateVersionMetadataAsync(int versionId, string? label, string? changeLog)
    {
        var version = await _context.WorkflowVersions.FindAsync(versionId);
        if (version == null || version.IsDeleted || version.Status != WorkflowVersionStatus.Draft)
            return null;

        if (label != null)
            version.Label = label;
        if (changeLog != null)
            version.ChangeLog = changeLog;
        version.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated metadata for version {VersionId}: Label='{Label}'", versionId, version.Label);
        return version;
    }

    /// <summary>
    /// Publish a draft version — activates it, deprecates previous active, records publisher
    /// </summary>
    public async Task<bool> PublishVersionAsync(int versionId, int publishedById)
    {
        var version = await _context.WorkflowVersions
            .Include(v => v.WorkflowDefinition)
            .FirstOrDefaultAsync(v => v.Id == versionId && !v.IsDeleted);
        if (version == null || version.Status != WorkflowVersionStatus.Draft)
            return false;

        var workflowId = version.WorkflowDefinitionId;

        // Deprecate any currently active versions
        var activeVersions = await _context.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == workflowId && v.Status == WorkflowVersionStatus.Active)
            .ToListAsync();
        foreach (var av in activeVersions)
        {
            av.Status = WorkflowVersionStatus.Deprecated;
            av.DeprecatedAt = DateTime.UtcNow;
            av.UpdatedAt = DateTime.UtcNow;
        }

        // Publish the version
        version.Status = WorkflowVersionStatus.Active;
        version.PublishedAt = DateTime.UtcNow;
        version.PublishedById = publishedById;
        version.UpdatedAt = DateTime.UtcNow;

        // Update the workflow definition
        version.WorkflowDefinition.Status = WorkflowStatus.Active;
        version.WorkflowDefinition.CurrentVersion = version.VersionNumber;
        version.WorkflowDefinition.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation(
            "Published version {VersionId} (v{VersionNumber}) for workflow {WorkflowId} by user {UserId}",
            versionId, version.VersionNumber, workflowId, publishedById);
        return true;
    }

    /// <summary>
    /// Delete a draft version (soft delete)
    /// </summary>
    public async Task<bool> DeleteVersionAsync(int versionId)
    {
        var version = await _context.WorkflowVersions.FindAsync(versionId);
        if (version == null || version.IsDeleted || version.Status != WorkflowVersionStatus.Draft)
            return false;

        version.IsDeleted = true;
        version.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted draft version {VersionId} for workflow {WorkflowId}",
            versionId, version.WorkflowDefinitionId);
        return true;
    }

    /// <summary>
    /// Create a new draft version cloned from a previous version (rollback)
    /// </summary>
    public async Task<WorkflowVersion> RollbackToVersionAsync(int workflowId, int sourceVersionId)
    {
        var sourceVersion = await GetWorkflowVersionAsync(sourceVersionId);
        if (sourceVersion == null || sourceVersion.WorkflowDefinitionId != workflowId)
            throw new ArgumentException("Source version not found or belongs to a different workflow");

        var newVersion = await CreateNewVersionAsync(workflowId, sourceVersionId);
        newVersion.Label = $"Rollback to v{sourceVersion.VersionNumber}";
        newVersion.ChangeLog = $"Rolled back from version {sourceVersion.VersionNumber} ({sourceVersion.Label})";
        newVersion.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created rollback version {VersionId} (v{VersionNumber}) from source v{SourceVersion} for workflow {WorkflowId}",
            newVersion.Id, newVersion.VersionNumber, sourceVersion.VersionNumber, workflowId);
        return newVersion;
    }

    /// <summary>
    /// Compare two versions and return node/transition differences
    /// </summary>
    public async Task<VersionComparisonResult> CompareVersionsAsync(int versionId1, int versionId2)
    {
        var v1 = await GetWorkflowVersionAsync(versionId1);
        var v2 = await GetWorkflowVersionAsync(versionId2);
        if (v1 == null || v2 == null)
            throw new ArgumentException("One or both versions not found");

        var result = new VersionComparisonResult
        {
            Version1Id = v1.Id,
            Version1Label = v1.Label,
            Version1Number = v1.VersionNumber,
            Version2Id = v2.Id,
            Version2Label = v2.Label,
            Version2Number = v2.VersionNumber,
        };

        // Compare nodes by NodeKey
        var v1Nodes = v1.Nodes.ToDictionary(n => n.NodeKey, n => n);
        var v2Nodes = v2.Nodes.ToDictionary(n => n.NodeKey, n => n);

        var addedKeys = v2Nodes.Keys.Except(v1Nodes.Keys);
        var removedKeys = v1Nodes.Keys.Except(v2Nodes.Keys);
        var commonKeys = v1Nodes.Keys.Intersect(v2Nodes.Keys);

        result.AddedNodes = addedKeys.Select(k => new NodeDiffItem
        {
            NodeKey = k,
            Name = v2Nodes[k].Name,
            NodeType = v2Nodes[k].NodeType.ToString()
        }).ToList();

        result.RemovedNodes = removedKeys.Select(k => new NodeDiffItem
        {
            NodeKey = k,
            Name = v1Nodes[k].Name,
            NodeType = v1Nodes[k].NodeType.ToString()
        }).ToList();

        foreach (var key in commonKeys)
        {
            var n1 = v1Nodes[key];
            var n2 = v2Nodes[key];
            var changes = new List<string>();
            if (n1.Name != n2.Name)
                changes.Add($"Name: '{n1.Name}' -> '{n2.Name}'");
            if (n1.NodeType != n2.NodeType)
                changes.Add($"Type: {n1.NodeType} -> {n2.NodeType}");
            if (n1.Configuration != n2.Configuration)
                changes.Add("Configuration changed");
            if (n1.IsStartNode != n2.IsStartNode)
                changes.Add($"IsStartNode: {n1.IsStartNode} -> {n2.IsStartNode}");
            if (n1.IsEndNode != n2.IsEndNode)
                changes.Add($"IsEndNode: {n1.IsEndNode} -> {n2.IsEndNode}");
            if (System.Math.Abs(n1.PositionX - n2.PositionX) > 0.1 || System.Math.Abs(n1.PositionY - n2.PositionY) > 0.1)
                changes.Add("Position changed");
            if (changes.Count > 0)
            {
                result.ModifiedNodes.Add(new NodeDiffItem
                {
                    NodeKey = key,
                    Name = n2.Name,
                    NodeType = n2.NodeType.ToString(),
                    Changes = changes
                });
            }
        }

        // Compare transitions by TransitionKey
        string TransKey(WorkflowTransition t) => t.TransitionKey ?? $"{t.SourceNodeId}->{t.TargetNodeId}";
        var v1Trans = v1.Transitions.GroupBy(TransKey).ToDictionary(g => g.Key, g => g.First());
        var v2Trans = v2.Transitions.GroupBy(TransKey).ToDictionary(g => g.Key, g => g.First());

        result.AddedTransitions = v2Trans.Keys.Except(v1Trans.Keys).Count();
        result.RemovedTransitions = v1Trans.Keys.Except(v2Trans.Keys).Count();
        result.ModifiedTransitions = v1Trans.Keys.Intersect(v2Trans.Keys)
            .Count(k =>
            {
                var t1 = v1Trans[k];
                var t2 = v2Trans[k];
                return t1.Label != t2.Label
                    || t1.ConditionExpression != t2.ConditionExpression
                    || t1.IsDefault != t2.IsDefault
                    || t1.Priority != t2.Priority;
            });

        result.TotalChanges = result.AddedNodes.Count + result.RemovedNodes.Count
            + result.ModifiedNodes.Count + result.AddedTransitions
            + result.RemovedTransitions + result.ModifiedTransitions;

        return result;
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

        if (node == null)
            return null;
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

        if (node == null)
            return false;
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

        if (transition == null)
            return null;
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

        if (transition == null)
            return false;
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
