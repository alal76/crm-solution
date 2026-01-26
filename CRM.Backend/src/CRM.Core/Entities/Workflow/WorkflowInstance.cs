// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents a running instance of a workflow
/// </summary>
public class WorkflowInstance : BaseEntity
{
    /// <summary>
    /// Foreign key to the workflow definition
    /// </summary>
    public int WorkflowDefinitionId { get; set; }
    
    /// <summary>
    /// Navigation property to the workflow definition
    /// </summary>
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    
    /// <summary>
    /// Foreign key to the workflow version
    /// </summary>
    public int WorkflowVersionId { get; set; }
    
    /// <summary>
    /// Navigation property to the workflow version
    /// </summary>
    public virtual WorkflowVersion WorkflowVersion { get; set; } = null!;
    
    /// <summary>
    /// Unique correlation ID for tracking
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Entity type being processed
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity ID being processed
    /// </summary>
    public int EntityId { get; set; }
    
    /// <summary>
    /// Current status of the instance
    /// </summary>
    public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.Pending;
    
    /// <summary>
    /// Current node ID (if in progress)
    /// </summary>
    public int? CurrentNodeId { get; set; }
    
    /// <summary>
    /// Navigation property to current node
    /// </summary>
    public virtual WorkflowNode? CurrentNode { get; set; }
    
    /// <summary>
    /// When the instance started
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// When the instance completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Scheduled execution time (for delayed starts)
    /// </summary>
    public DateTime? ScheduledAt { get; set; }
    
    /// <summary>
    /// Priority for execution (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;
    
    /// <summary>
    /// Trigger event that started this instance
    /// </summary>
    [MaxLength(100)]
    public string? TriggerEvent { get; set; }
    
    /// <summary>
    /// User who triggered the workflow
    /// </summary>
    public int? TriggeredById { get; set; }
    
    /// <summary>
    /// Navigation property to trigger user
    /// </summary>
    public virtual User? TriggeredBy { get; set; }
    
    /// <summary>
    /// Input data for the workflow (JSON)
    /// </summary>
    public string? InputData { get; set; }
    
    /// <summary>
    /// Current state/context data (JSON)
    /// </summary>
    public string? StateData { get; set; }
    
    /// <summary>
    /// Output data from the workflow (JSON)
    /// </summary>
    public string? OutputData { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Stack trace if failed
    /// </summary>
    public string? ErrorStackTrace { get; set; }
    
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Next retry time
    /// </summary>
    public DateTime? NextRetryAt { get; set; }
    
    /// <summary>
    /// When the instance times out
    /// </summary>
    public DateTime? TimeoutAt { get; set; }
    
    /// <summary>
    /// Whether this instance was cancelled
    /// </summary>
    public bool IsCancelled { get; set; } = false;
    
    /// <summary>
    /// Cancellation reason
    /// </summary>
    [MaxLength(500)]
    public string? CancellationReason { get; set; }
    
    /// <summary>
    /// Parent instance ID (for subprocesses)
    /// </summary>
    public int? ParentInstanceId { get; set; }
    
    /// <summary>
    /// Navigation property to parent instance
    /// </summary>
    public virtual WorkflowInstance? ParentInstance { get; set; }
    
    /// <summary>
    /// Child instances (for subprocesses)
    /// </summary>
    public virtual ICollection<WorkflowInstance> ChildInstances { get; set; } = new List<WorkflowInstance>();
    
    /// <summary>
    /// Node instances for this workflow instance
    /// </summary>
    public virtual ICollection<WorkflowNodeInstance> NodeInstances { get; set; } = new List<WorkflowNodeInstance>();
    
    /// <summary>
    /// Tasks for this workflow instance
    /// </summary>
    public virtual ICollection<WorkflowTask> Tasks { get; set; } = new List<WorkflowTask>();
    
    /// <summary>
    /// Logs for this workflow instance
    /// </summary>
    public virtual ICollection<WorkflowLog> Logs { get; set; } = new List<WorkflowLog>();
}

/// <summary>
/// Workflow instance status enumeration
/// </summary>
public enum WorkflowInstanceStatus
{
    /// <summary>
    /// Pending - not yet started
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Running - currently executing
    /// </summary>
    Running = 1,
    
    /// <summary>
    /// Waiting - waiting for external event or timer
    /// </summary>
    Waiting = 2,
    
    /// <summary>
    /// Paused - manually paused
    /// </summary>
    Paused = 3,
    
    /// <summary>
    /// Completed - successfully finished
    /// </summary>
    Completed = 4,
    
    /// <summary>
    /// Failed - terminated due to error
    /// </summary>
    Failed = 5,
    
    /// <summary>
    /// Cancelled - manually cancelled
    /// </summary>
    Cancelled = 6,
    
    /// <summary>
    /// Timed out - exceeded time limit
    /// </summary>
    TimedOut = 7,
    
    /// <summary>
    /// Suspended - suspended for investigation
    /// </summary>
    Suspended = 8
}
