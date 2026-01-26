// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents a task in the workflow queue for background processing
/// </summary>
public class WorkflowTask : BaseEntity
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
    /// Foreign key to the node instance
    /// </summary>
    public int? NodeInstanceId { get; set; }
    
    /// <summary>
    /// Navigation property to the node instance
    /// </summary>
    public virtual WorkflowNodeInstance? NodeInstance { get; set; }
    
    /// <summary>
    /// Task type
    /// </summary>
    public WorkflowTaskType TaskType { get; set; }
    
    /// <summary>
    /// Task name for display
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Task description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Current status of the task
    /// </summary>
    public WorkflowTaskStatus Status { get; set; } = WorkflowTaskStatus.Pending;
    
    /// <summary>
    /// Priority for task execution (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;
    
    /// <summary>
    /// Queue name for routing
    /// </summary>
    [MaxLength(100)]
    public string QueueName { get; set; } = "default";
    
    /// <summary>
    /// Scheduled execution time
    /// </summary>
    public DateTime? ScheduledAt { get; set; }
    
    /// <summary>
    /// When the task was picked up by a worker
    /// </summary>
    public DateTime? PickedAt { get; set; }
    
    /// <summary>
    /// When the task started executing
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// When the task completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Task due date (for human tasks)
    /// </summary>
    public DateTime? DueAt { get; set; }
    
    /// <summary>
    /// Timeout for this task
    /// </summary>
    public DateTime? TimeoutAt { get; set; }
    
    /// <summary>
    /// Worker ID that locked this task
    /// </summary>
    [MaxLength(100)]
    public string? LockedByWorkerId { get; set; }
    
    /// <summary>
    /// When the lock expires
    /// </summary>
    public DateTime? LockExpiresAt { get; set; }
    
    /// <summary>
    /// Assigned user ID (for human tasks)
    /// </summary>
    public int? AssignedToId { get; set; }
    
    /// <summary>
    /// Navigation property to assigned user
    /// </summary>
    public virtual User? AssignedTo { get; set; }
    
    /// <summary>
    /// Role that can claim this task (for human tasks)
    /// </summary>
    [MaxLength(100)]
    public string? AssignedToRole { get; set; }
    
    /// <summary>
    /// Input data for the task (JSON)
    /// </summary>
    public string? InputData { get; set; }
    
    /// <summary>
    /// Output data from the task (JSON)
    /// </summary>
    public string? OutputData { get; set; }
    
    /// <summary>
    /// Form schema for human tasks (JSON)
    /// </summary>
    public string? FormSchema { get; set; }
    
    /// <summary>
    /// Form data submitted by user (JSON)
    /// </summary>
    public string? FormData { get; set; }
    
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
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Next retry time
    /// </summary>
    public DateTime? NextRetryAt { get; set; }
    
    /// <summary>
    /// Whether this task is in the dead letter queue
    /// </summary>
    public bool IsDeadLetter { get; set; } = false;
    
    /// <summary>
    /// Dead letter reason
    /// </summary>
    [MaxLength(500)]
    public string? DeadLetterReason { get; set; }
    
    /// <summary>
    /// When the task was moved to dead letter
    /// </summary>
    public DateTime? DeadLetterAt { get; set; }
}

/// <summary>
/// Workflow task type enumeration
/// </summary>
public enum WorkflowTaskType
{
    /// <summary>
    /// Automated task - executed by workers
    /// </summary>
    Automated = 0,
    
    /// <summary>
    /// Human task - requires human input
    /// </summary>
    Human = 1,
    
    /// <summary>
    /// Timer task - scheduled execution
    /// </summary>
    Timer = 2,
    
    /// <summary>
    /// Event task - waiting for external event
    /// </summary>
    Event = 3,
    
    /// <summary>
    /// LLM task - AI-powered execution
    /// </summary>
    LLM = 4
}

/// <summary>
/// Workflow task status enumeration
/// </summary>
public enum WorkflowTaskStatus
{
    /// <summary>
    /// Pending - not yet picked up
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Locked - picked up by a worker
    /// </summary>
    Locked = 1,
    
    /// <summary>
    /// Running - currently executing
    /// </summary>
    Running = 2,
    
    /// <summary>
    /// Waiting - waiting for human input or external event
    /// </summary>
    Waiting = 3,
    
    /// <summary>
    /// Completed - successfully finished
    /// </summary>
    Completed = 4,
    
    /// <summary>
    /// Failed - terminated due to error
    /// </summary>
    Failed = 5,
    
    /// <summary>
    /// Retrying - waiting for retry
    /// </summary>
    Retrying = 6,
    
    /// <summary>
    /// Cancelled - cancelled by user
    /// </summary>
    Cancelled = 7,
    
    /// <summary>
    /// Skipped - skipped manually
    /// </summary>
    Skipped = 8,
    
    /// <summary>
    /// DeadLetter - moved to dead letter queue
    /// </summary>
    DeadLetter = 9
}
