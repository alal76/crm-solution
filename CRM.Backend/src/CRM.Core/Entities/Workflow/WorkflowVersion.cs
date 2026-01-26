// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents a specific version of a workflow definition
/// </summary>
public class WorkflowVersion : BaseEntity
{
    /// <summary>
    /// Foreign key to the parent workflow definition
    /// </summary>
    public int WorkflowDefinitionId { get; set; }
    
    /// <summary>
    /// Navigation property to the workflow definition
    /// </summary>
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    
    /// <summary>
    /// Version number (incremental)
    /// </summary>
    public int VersionNumber { get; set; }
    
    /// <summary>
    /// Version label (e.g., "v1.0", "v2.0-beta")
    /// </summary>
    [MaxLength(50)]
    public string? Label { get; set; }
    
    /// <summary>
    /// Description of changes in this version
    /// </summary>
    [MaxLength(1000)]
    public string? ChangeLog { get; set; }
    
    /// <summary>
    /// Version status
    /// </summary>
    public WorkflowVersionStatus Status { get; set; } = WorkflowVersionStatus.Draft;
    
    /// <summary>
    /// Date when this version was published
    /// </summary>
    public DateTime? PublishedAt { get; set; }
    
    /// <summary>
    /// User who published this version
    /// </summary>
    public int? PublishedById { get; set; }
    
    /// <summary>
    /// Navigation property to publisher
    /// </summary>
    public virtual User? PublishedBy { get; set; }
    
    /// <summary>
    /// Date when this version was deprecated
    /// </summary>
    public DateTime? DeprecatedAt { get; set; }
    
    /// <summary>
    /// Canvas layout configuration (JSON)
    /// </summary>
    public string? CanvasLayout { get; set; }
    
    /// <summary>
    /// Nodes in this version
    /// </summary>
    public virtual ICollection<WorkflowNode> Nodes { get; set; } = new List<WorkflowNode>();
    
    /// <summary>
    /// Transitions in this version
    /// </summary>
    public virtual ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
}

/// <summary>
/// Workflow version status enumeration
/// </summary>
public enum WorkflowVersionStatus
{
    Draft = 0,
    Active = 1,
    Deprecated = 2
}
