using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.WorkflowEngine;

/// <summary>
/// Workflow instance - tracks individual workflow executions
/// </summary>
public class WorkflowInstance : BaseEntity
{
    public int WorkflowDefinitionId { get; set; }
    
    /// <summary>
    /// Version of workflow at time of execution
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string WorkflowVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of entity this workflow is processing
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of entity this workflow is processing
    /// </summary>
    public int? EntityId { get; set; }
    
    /// <summary>
    /// External reference (e.g., "SR-12345")
    /// </summary>
    [MaxLength(200)]
    public string? EntityReference { get; set; }
    
    /// <summary>
    /// Status: Pending, Running, Completed, Failed, Cancelled, Paused
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = WorkflowInstanceStatus.Pending;
    
    /// <summary>
    /// Current step key being executed
    /// </summary>
    [MaxLength(100)]
    public string? CurrentStepKey { get; set; }
    
    /// <summary>
    /// For parallel execution - comma-separated list of active step keys
    /// </summary>
    [MaxLength(1000)]
    public string? ActiveStepKeys { get; set; }
    
    /// <summary>
    /// Priority: Low, Normal, High, Critical
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = WorkflowPriority.Normal;
    
    /// <summary>
    /// User who started the workflow
    /// </summary>
    public int? StartedByUserId { get; set; }
    
    /// <summary>
    /// When workflow started executing
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// When workflow completed/failed/cancelled
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Due date/SLA for workflow completion
    /// </summary>
    public DateTime? DueAt { get; set; }
    
    /// <summary>
    /// Last error message if failed
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Next scheduled retry time
    /// </summary>
    public DateTime? NextRetryAt { get; set; }
    
    /// <summary>
    /// Version for optimistic locking
    /// </summary>
    public int LockVersion { get; set; } = 1;
    
    /// <summary>
    /// Worker ID that is currently processing this instance
    /// </summary>
    [MaxLength(100)]
    public string? ProcessingWorkerId { get; set; }
    
    /// <summary>
    /// When the current worker started processing
    /// </summary>
    public DateTime? ProcessingStartedAt { get; set; }
    
    // Navigation properties
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public virtual User? StartedByUser { get; set; }
    public virtual ICollection<WorkflowEvent> Events { get; set; } = new List<WorkflowEvent>();
    public virtual ICollection<WorkflowTask> Tasks { get; set; } = new List<WorkflowTask>();
    public virtual ICollection<WorkflowContextVariable> ContextVariables { get; set; } = new List<WorkflowContextVariable>();
}

/// <summary>
/// Workflow instance status values
/// </summary>
public static class WorkflowInstanceStatus
{
    public const string Pending = "Pending";
    public const string Running = "Running";
    public const string WaitingForInput = "WaitingForInput";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string Paused = "Paused";
    public const string Suspended = "Suspended";
}

/// <summary>
/// Workflow priority levels
/// </summary>
public static class WorkflowPriority
{
    public const string Low = "Low";
    public const string Normal = "Normal";
    public const string High = "High";
    public const string Critical = "Critical";
}
