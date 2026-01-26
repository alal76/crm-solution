// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents a log entry for workflow execution
/// </summary>
public class WorkflowLog : BaseEntity
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
    /// Foreign key to the workflow node (optional)
    /// </summary>
    public int? WorkflowNodeId { get; set; }
    
    /// <summary>
    /// Navigation property to the workflow node
    /// </summary>
    public virtual WorkflowNode? WorkflowNode { get; set; }
    
    /// <summary>
    /// Foreign key to the node instance (optional)
    /// </summary>
    public int? NodeInstanceId { get; set; }
    
    /// <summary>
    /// Navigation property to the node instance
    /// </summary>
    public virtual WorkflowNodeInstance? NodeInstance { get; set; }
    
    /// <summary>
    /// Log level
    /// </summary>
    public WorkflowLogLevel Level { get; set; } = WorkflowLogLevel.Info;
    
    /// <summary>
    /// Log category
    /// </summary>
    [MaxLength(100)]
    public string Category { get; set; } = "General";
    
    /// <summary>
    /// Log message
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed data (JSON)
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// Timestamp of the log entry
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Worker ID that generated the log
    /// </summary>
    [MaxLength(100)]
    public string? WorkerId { get; set; }
    
    /// <summary>
    /// User ID if action was user-initiated
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Navigation property to user
    /// </summary>
    public virtual User? User { get; set; }
    
    /// <summary>
    /// Duration in milliseconds (for performance logging)
    /// </summary>
    public long? DurationMs { get; set; }
    
    /// <summary>
    /// Exception type if error
    /// </summary>
    [MaxLength(200)]
    public string? ExceptionType { get; set; }
    
    /// <summary>
    /// Stack trace if error
    /// </summary>
    public string? StackTrace { get; set; }
}

/// <summary>
/// Workflow log level enumeration
/// </summary>
public enum WorkflowLogLevel
{
    /// <summary>
    /// Debug - detailed diagnostic information
    /// </summary>
    Debug = 0,
    
    /// <summary>
    /// Info - general information
    /// </summary>
    Info = 1,
    
    /// <summary>
    /// Warning - potential issues
    /// </summary>
    Warning = 2,
    
    /// <summary>
    /// Error - error conditions
    /// </summary>
    Error = 3,
    
    /// <summary>
    /// Critical - critical failures
    /// </summary>
    Critical = 4
}
