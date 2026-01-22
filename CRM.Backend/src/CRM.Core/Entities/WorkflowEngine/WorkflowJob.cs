using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.WorkflowEngine;

/// <summary>
/// Job queue for background workflow processing
/// </summary>
public class WorkflowJob : BaseEntity
{
    /// <summary>
    /// Job type: ExecuteStep, CheckTimeout, SendNotification, CleanupInstances
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string JobType { get; set; } = string.Empty;
    
    /// <summary>
    /// Job status: Pending, Processing, Completed, Failed, Cancelled
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = WorkflowJobStatus.Pending;
    
    /// <summary>
    /// Priority (higher = more urgent)
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Related workflow instance ID
    /// </summary>
    public int? WorkflowInstanceId { get; set; }
    
    /// <summary>
    /// Step key to execute (for step jobs)
    /// </summary>
    [MaxLength(100)]
    public string? StepKey { get; set; }
    
    /// <summary>
    /// Task ID (for task-related jobs)
    /// </summary>
    public int? WorkflowTaskId { get; set; }
    
    /// <summary>
    /// JSON payload for the job
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? Payload { get; set; }
    
    /// <summary>
    /// Scheduled execution time
    /// </summary>
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When job started processing
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// When job completed/failed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Worker ID processing this job
    /// </summary>
    [MaxLength(100)]
    public string? ProcessingWorkerId { get; set; }
    
    /// <summary>
    /// Number of attempts made
    /// </summary>
    public int AttemptCount { get; set; } = 0;
    
    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxAttempts { get; set; } = 3;
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Result data JSON
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? ResultData { get; set; }
    
    /// <summary>
    /// Visibility timeout for job locking
    /// </summary>
    public DateTime? VisibilityTimeoutAt { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    [MaxLength(100)]
    public string? CorrelationId { get; set; }
    
    // Navigation properties
    public virtual WorkflowInstance? WorkflowInstance { get; set; }
    public virtual WorkflowTask? WorkflowTask { get; set; }
}

/// <summary>
/// Job status values
/// </summary>
public static class WorkflowJobStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string DeadLetter = "DeadLetter";
}

/// <summary>
/// Job type constants
/// </summary>
public static class WorkflowJobTypes
{
    public const string ExecuteStep = "ExecuteStep";
    public const string CheckTimeout = "CheckTimeout";
    public const string SendNotification = "SendNotification";
    public const string CleanupInstances = "CleanupInstances";
    public const string ProcessEscalation = "ProcessEscalation";
    public const string ExecuteApiCall = "ExecuteApiCall";
    public const string EvaluateCondition = "EvaluateCondition";
}
