using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.WorkflowEngine;

/// <summary>
/// Immutable event log for workflow audit trail (Event Sourcing)
/// </summary>
public class WorkflowEvent : BaseEntity
{
    public int WorkflowInstanceId { get; set; }
    
    /// <summary>
    /// Event type: StepStarted, StepCompleted, StepFailed, WorkflowStarted, WorkflowCompleted, etc.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Step key if event is step-related
    /// </summary>
    [MaxLength(100)]
    public string? StepKey { get; set; }
    
    /// <summary>
    /// Timestamp of the event
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Actor type: User, System, Api, Worker
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ActorType { get; set; } = WorkflowActorTypes.System;
    
    /// <summary>
    /// Actor identifier (user ID, worker ID, API key name)
    /// </summary>
    [MaxLength(200)]
    public string? ActorId { get; set; }
    
    /// <summary>
    /// Actor display name
    /// </summary>
    [MaxLength(200)]
    public string? ActorName { get; set; }
    
    /// <summary>
    /// Input data JSON for the step/action
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? InputData { get; set; }
    
    /// <summary>
    /// Output data JSON from the step/action
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? OutputData { get; set; }
    
    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }
    
    /// <summary>
    /// Error details if failed
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? ErrorDetails { get; set; }
    
    /// <summary>
    /// Severity level: Info, Warning, Error
    /// </summary>
    [MaxLength(20)]
    public string Severity { get; set; } = "Info";
    
    /// <summary>
    /// Human-readable message
    /// </summary>
    [MaxLength(2000)]
    public string? Message { get; set; }
    
    /// <summary>
    /// Client IP address
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent string
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Correlation ID for distributed tracing
    /// </summary>
    [MaxLength(100)]
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Additional metadata JSON
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
}

/// <summary>
/// Event type constants
/// </summary>
public static class WorkflowEventTypes
{
    // Workflow-level events
    public const string WorkflowStarted = "WorkflowStarted";
    public const string WorkflowCompleted = "WorkflowCompleted";
    public const string WorkflowFailed = "WorkflowFailed";
    public const string WorkflowCancelled = "WorkflowCancelled";
    public const string WorkflowPaused = "WorkflowPaused";
    public const string WorkflowResumed = "WorkflowResumed";
    public const string WorkflowRetrying = "WorkflowRetrying";
    
    // Step-level events
    public const string StepStarted = "StepStarted";
    public const string StepCompleted = "StepCompleted";
    public const string StepFailed = "StepFailed";
    public const string StepSkipped = "StepSkipped";
    public const string StepTimedOut = "StepTimedOut";
    public const string StepRetrying = "StepRetrying";
    
    // Task-level events
    public const string TaskCreated = "TaskCreated";
    public const string TaskAssigned = "TaskAssigned";
    public const string TaskReassigned = "TaskReassigned";
    public const string TaskCompleted = "TaskCompleted";
    public const string TaskEscalated = "TaskEscalated";
    public const string TaskTimedOut = "TaskTimedOut";
    
    // Context events
    public const string VariableSet = "VariableSet";
    public const string ContextUpdated = "ContextUpdated";
    
    // API call events
    public const string ApiCallStarted = "ApiCallStarted";
    public const string ApiCallCompleted = "ApiCallCompleted";
    public const string ApiCallFailed = "ApiCallFailed";
    
    // Notification events
    public const string NotificationSent = "NotificationSent";
    public const string NotificationFailed = "NotificationFailed";
    
    // SLA events
    public const string SlaBreached = "SlaBreached";
    public const string SlaWarning = "SlaWarning";
    public const string TaskOverdue = "TaskOverdue";
}

/// <summary>
/// Actor type constants
/// </summary>
public static class WorkflowActorTypes
{
    public const string User = "User";
    public const string System = "System";
    public const string Api = "Api";
    public const string Worker = "Worker";
    public const string Scheduler = "Scheduler";
    public const string Webhook = "Webhook";
}
