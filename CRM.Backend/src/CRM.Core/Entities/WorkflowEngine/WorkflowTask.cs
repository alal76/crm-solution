using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.WorkflowEngine;

/// <summary>
/// Pending user task for workflow user action steps
/// </summary>
public class WorkflowTask : BaseEntity
{
    public int WorkflowInstanceId { get; set; }
    
    /// <summary>
    /// Step key that created this task
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string StepKey { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [MaxLength(2000)]
    public string? Instructions { get; set; }
    
    /// <summary>
    /// Task status: Pending, InProgress, Completed, Cancelled, Expired
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = WorkflowTaskStatus.Pending;
    
    /// <summary>
    /// Assignment type: User, Role, Group, Queue
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string AssignmentType { get; set; } = "User";
    
    /// <summary>
    /// Assigned user ID (if assigned to specific user)
    /// </summary>
    public int? AssignedToUserId { get; set; }
    
    /// <summary>
    /// Assigned user group ID (if assigned to group/queue)
    /// </summary>
    public int? AssignedToGroupId { get; set; }
    
    /// <summary>
    /// Role name (if assigned by role)
    /// </summary>
    [MaxLength(100)]
    public string? AssignedToRole { get; set; }
    
    /// <summary>
    /// Priority: Low, Normal, High, Critical
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = WorkflowPriority.Normal;
    
    /// <summary>
    /// Due date for task completion
    /// </summary>
    public DateTime? DueAt { get; set; }
    
    /// <summary>
    /// When task was claimed by a user
    /// </summary>
    public DateTime? ClaimedAt { get; set; }
    
    /// <summary>
    /// User who claimed/is working on the task
    /// </summary>
    public int? ClaimedByUserId { get; set; }
    
    /// <summary>
    /// When task was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// User who completed the task
    /// </summary>
    public int? CompletedByUserId { get; set; }
    
    /// <summary>
    /// Action taken (e.g., "Approve", "Reject", "RequestInfo")
    /// </summary>
    [MaxLength(100)]
    public string? ActionTaken { get; set; }
    
    /// <summary>
    /// Comments from user
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? Comments { get; set; }
    
    /// <summary>
    /// Form schema JSON for data collection
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? FormSchema { get; set; }
    
    /// <summary>
    /// Form data JSON submitted by user
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? FormData { get; set; }
    
    /// <summary>
    /// Available actions JSON array
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? AvailableActions { get; set; }
    
    /// <summary>
    /// Number of escalation levels triggered
    /// </summary>
    public int EscalationLevel { get; set; } = 0;
    
    /// <summary>
    /// Reminder count sent
    /// </summary>
    public int ReminderCount { get; set; } = 0;
    
    /// <summary>
    /// Last reminder sent at
    /// </summary>
    public DateTime? LastReminderAt { get; set; }
    
    // Navigation properties
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
    public virtual User? AssignedToUser { get; set; }
    public virtual UserGroup? AssignedToGroup { get; set; }
    public virtual User? ClaimedByUser { get; set; }
    public virtual User? CompletedByUser { get; set; }
}

/// <summary>
/// Task status values
/// </summary>
public static class WorkflowTaskStatus
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";
    public const string Escalated = "Escalated";
}
