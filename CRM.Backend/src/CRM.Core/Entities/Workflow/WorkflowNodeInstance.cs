// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents an instance of a node execution
/// </summary>
public class WorkflowNodeInstance : BaseEntity
{
    /// <summary>
    /// Foreign key to the workflow instance
    /// </summary>
    public int WorkflowInstanceId { get; set; }
    
    /// <summary>
    /// Navigation property to the workflow instance
    /// </summary>
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
    
    /// <summary>
    /// Foreign key to the workflow node
    /// </summary>
    public int WorkflowNodeId { get; set; }
    
    /// <summary>
    /// Navigation property to the workflow node
    /// </summary>
    public virtual WorkflowNode WorkflowNode { get; set; } = null!;
    
    /// <summary>
    /// Current status of the node instance
    /// </summary>
    public WorkflowNodeInstanceStatus Status { get; set; } = WorkflowNodeInstanceStatus.Pending;
    
    /// <summary>
    /// When the node started executing
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// When the node completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Duration of execution in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }
    
    /// <summary>
    /// Input data for this node (JSON)
    /// </summary>
    public string? InputData { get; set; }
    
    /// <summary>
    /// Output data from this node (JSON)
    /// </summary>
    public string? OutputData { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Error stack trace if failed
    /// </summary>
    public string? ErrorStackTrace { get; set; }
    
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Next retry time
    /// </summary>
    public DateTime? NextRetryAt { get; set; }
    
    /// <summary>
    /// Whether this node was skipped
    /// </summary>
    public bool IsSkipped { get; set; } = false;
    
    /// <summary>
    /// Skip reason
    /// </summary>
    [MaxLength(500)]
    public string? SkipReason { get; set; }
    
    /// <summary>
    /// Execution sequence number
    /// </summary>
    public int ExecutionSequence { get; set; }
    
    /// <summary>
    /// Worker ID that processed this node
    /// </summary>
    [MaxLength(100)]
    public string? WorkerId { get; set; }
    
    /// <summary>
    /// Transition taken from this node
    /// </summary>
    public int? TransitionTakenId { get; set; }
    
    /// <summary>
    /// Navigation property to transition taken
    /// </summary>
    public virtual WorkflowTransition? TransitionTaken { get; set; }
}

/// <summary>
/// Workflow node instance status enumeration
/// </summary>
public enum WorkflowNodeInstanceStatus
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
    /// Waiting - waiting for human input or external event
    /// </summary>
    Waiting = 2,
    
    /// <summary>
    /// Completed - successfully finished
    /// </summary>
    Completed = 3,
    
    /// <summary>
    /// Failed - terminated due to error
    /// </summary>
    Failed = 4,
    
    /// <summary>
    /// Skipped - skipped due to condition or manual action
    /// </summary>
    Skipped = 5,
    
    /// <summary>
    /// Cancelled - cancelled by user
    /// </summary>
    Cancelled = 6,
    
    /// <summary>
    /// Retrying - failed and waiting for retry
    /// </summary>
    Retrying = 7
}
