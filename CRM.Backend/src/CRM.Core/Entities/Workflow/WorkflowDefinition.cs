// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents a workflow definition that can be applied to entities
/// </summary>
public class WorkflowDefinition : BaseEntity
{
    /// <summary>
    /// Unique identifier for the workflow
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string WorkflowKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name of the workflow
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the workflow purpose
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Category for grouping workflows (e.g., Sales, Marketing, Service)
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }
    
    /// <summary>
    /// Entity type this workflow applies to (Customer, Lead, Opportunity, etc.)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the workflow
    /// </summary>
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    
    /// <summary>
    /// Current active version number
    /// </summary>
    public int CurrentVersion { get; set; } = 1;
    
    /// <summary>
    /// Icon name for UI display
    /// </summary>
    [MaxLength(50)]
    public string IconName { get; set; } = "AccountTree";
    
    /// <summary>
    /// Color theme for the workflow
    /// </summary>
    [MaxLength(20)]
    public string Color { get; set; } = "#6750A4";
    
    /// <summary>
    /// Whether this is a system workflow that cannot be deleted
    /// </summary>
    public bool IsSystem { get; set; } = false;
    
    /// <summary>
    /// Priority for workflow execution order (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;
    
    /// <summary>
    /// Maximum concurrent instances allowed (0 = unlimited)
    /// </summary>
    public int MaxConcurrentInstances { get; set; } = 0;
    
    /// <summary>
    /// Default timeout for workflow instances in hours (0 = no timeout)
    /// </summary>
    public int DefaultTimeoutHours { get; set; } = 0;
    
    /// <summary>
    /// Owner user ID
    /// </summary>
    public int? OwnerId { get; set; }
    
    /// <summary>
    /// Navigation property to owner
    /// </summary>
    public virtual User? Owner { get; set; }
    
    /// <summary>
    /// Tags for filtering and categorization
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    /// <summary>
    /// JSON metadata for additional configuration
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Workflow versions
    /// </summary>
    public virtual ICollection<WorkflowVersion> Versions { get; set; } = new List<WorkflowVersion>();
    
    /// <summary>
    /// Workflow instances
    /// </summary>
    public virtual ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
}

/// <summary>
/// Workflow status enumeration
/// </summary>
public enum WorkflowStatus
{
    Draft = 0,
    Active = 1,
    Paused = 2,
    Archived = 3,
    Deprecated = 4
}
