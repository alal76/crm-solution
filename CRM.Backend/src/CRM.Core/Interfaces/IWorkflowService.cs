// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities.Workflow;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for managing workflow definitions and versions
/// </summary>
public interface IWorkflowService
{
    #region Workflow Definition Operations

    /// <summary>
    /// Get all workflow definitions with optional filtering
    /// </summary>
    Task<List<WorkflowDefinition>> GetWorkflowDefinitionsAsync(
        string? entityType = null,
        WorkflowStatus? status = null,
        string? category = null,
        string? search = null,
        int skip = 0,
        int take = 50);

    /// <summary>
    /// Get a workflow definition by ID with full graph
    /// </summary>
    Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(int id);

    /// <summary>
    /// Get a workflow definition by key
    /// </summary>
    Task<WorkflowDefinition?> GetWorkflowByKeyAsync(string key);

    /// <summary>
    /// Create a new workflow definition
    /// </summary>
    Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(WorkflowDefinition workflow);

    /// <summary>
    /// Update an existing workflow definition
    /// </summary>
    Task<WorkflowDefinition?> UpdateWorkflowDefinitionAsync(int id, WorkflowDefinition updates);

    /// <summary>
    /// Delete a workflow definition (soft delete)
    /// </summary>
    Task<bool> DeleteWorkflowDefinitionAsync(int id);

    /// <summary>
    /// Activate a workflow with a specific version
    /// </summary>
    Task<bool> ActivateWorkflowAsync(int id, int versionId);

    /// <summary>
    /// Pause a workflow
    /// </summary>
    Task<bool> PauseWorkflowAsync(int id);

    #endregion

    #region Version Operations

    /// <summary>
    /// Get a workflow version by ID
    /// </summary>
    Task<WorkflowVersion?> GetWorkflowVersionAsync(int versionId);

    /// <summary>
    /// Get the active version for a workflow
    /// </summary>
    Task<WorkflowVersion?> GetActiveVersionAsync(int workflowId);

    /// <summary>
    /// Get the draft version for a workflow
    /// </summary>
    Task<WorkflowVersion?> GetDraftVersionAsync(int workflowId);

    /// <summary>
    /// Create a new version of a workflow
    /// </summary>
    Task<WorkflowVersion> CreateNewVersionAsync(int workflowId, int? sourceVersionId = null);

    /// <summary>
    /// Save canvas layout for a version
    /// </summary>
    Task<bool> SaveCanvasLayoutAsync(int versionId, string canvasLayout);

    /// <summary>
    /// Get all versions for a workflow, ordered by version number descending
    /// </summary>
    Task<List<WorkflowVersion>> GetVersionsAsync(int workflowId);

    /// <summary>
    /// Update a draft version's metadata (label, changelog)
    /// </summary>
    Task<WorkflowVersion?> UpdateVersionMetadataAsync(int versionId, string? label, string? changeLog);

    /// <summary>
    /// Publish a draft version — sets it Active, deprecates previous active, records publisher
    /// </summary>
    Task<bool> PublishVersionAsync(int versionId, int publishedById);

    /// <summary>
    /// Delete a draft version (soft delete — only Draft versions can be deleted)
    /// </summary>
    Task<bool> DeleteVersionAsync(int versionId);

    /// <summary>
    /// Create a new draft version cloned from a previous version for rollback
    /// </summary>
    Task<WorkflowVersion> RollbackToVersionAsync(int workflowId, int sourceVersionId);

    /// <summary>
    /// Compare two versions and return node/transition differences
    /// </summary>
    Task<VersionComparisonResult> CompareVersionsAsync(int versionId1, int versionId2);

    #endregion

    #region Node Operations

    /// <summary>
    /// Add a node to a workflow version
    /// </summary>
    Task<WorkflowNode> AddNodeAsync(WorkflowNode node);

    /// <summary>
    /// Update a workflow node
    /// </summary>
    Task<WorkflowNode?> UpdateNodeAsync(int nodeId, WorkflowNode updates);

    /// <summary>
    /// Delete a workflow node
    /// </summary>
    Task<bool> DeleteNodeAsync(int nodeId);

    /// <summary>
    /// Update positions of multiple nodes
    /// </summary>
    Task UpdateNodePositionsAsync(Dictionary<int, (double x, double y)> positions);

    #endregion

    #region Transition Operations

    /// <summary>
    /// Add a transition between nodes
    /// </summary>
    Task<WorkflowTransition> AddTransitionAsync(WorkflowTransition transition);

    /// <summary>
    /// Update a transition
    /// </summary>
    Task<WorkflowTransition?> UpdateTransitionAsync(int transitionId, WorkflowTransition updates);

    /// <summary>
    /// Delete a transition
    /// </summary>
    Task<bool> DeleteTransitionAsync(int transitionId);

    #endregion

    #region Statistics

    /// <summary>
    /// Get workflow statistics
    /// </summary>
    Task<WorkflowStatistics> GetStatisticsAsync();

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

/// <summary>
/// Result of comparing two workflow versions
/// </summary>
public class VersionComparisonResult
{
    public int Version1Id { get; set; }
    public string? Version1Label { get; set; }
    public int Version1Number { get; set; }
    public int Version2Id { get; set; }
    public string? Version2Label { get; set; }
    public int Version2Number { get; set; }
    public int TotalChanges { get; set; }
    public List<NodeDiffItem> AddedNodes { get; set; } = new();
    public List<NodeDiffItem> RemovedNodes { get; set; } = new();
    public List<NodeDiffItem> ModifiedNodes { get; set; } = new();
    public int AddedTransitions { get; set; }
    public int RemovedTransitions { get; set; }
    public int ModifiedTransitions { get; set; }
}

/// <summary>
/// Represents a node difference between two versions
/// </summary>
public class NodeDiffItem
{
    public string NodeKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public List<string> Changes { get; set; } = new();
}
