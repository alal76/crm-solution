// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents a node in a workflow (trigger, condition, action, or human task)
/// </summary>
public class WorkflowNode : BaseEntity
{
    /// <summary>
    /// Foreign key to the workflow version
    /// </summary>
    public int WorkflowVersionId { get; set; }
    
    /// <summary>
    /// Navigation property to the workflow version
    /// </summary>
    public virtual WorkflowVersion WorkflowVersion { get; set; } = null!;
    
    /// <summary>
    /// Unique key within the workflow
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string NodeKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name of the node
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Node description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Type of node
    /// </summary>
    public WorkflowNodeType NodeType { get; set; }
    
    /// <summary>
    /// Subtype for specific functionality (e.g., "OnCreate", "SendEmail", "LLMAction")
    /// </summary>
    [MaxLength(100)]
    public string? NodeSubType { get; set; }
    
    /// <summary>
    /// X position on the canvas
    /// </summary>
    public double PositionX { get; set; }
    
    /// <summary>
    /// Y position on the canvas
    /// </summary>
    public double PositionY { get; set; }
    
    /// <summary>
    /// Width on the canvas
    /// </summary>
    public double Width { get; set; } = 200;
    
    /// <summary>
    /// Height on the canvas
    /// </summary>
    public double Height { get; set; } = 80;
    
    /// <summary>
    /// Icon name for UI display
    /// </summary>
    [MaxLength(50)]
    public string IconName { get; set; } = "Circle";
    
    /// <summary>
    /// Color theme for the node
    /// </summary>
    [MaxLength(20)]
    public string Color { get; set; } = "#6750A4";
    
    /// <summary>
    /// Whether this is the start node
    /// </summary>
    public bool IsStartNode { get; set; } = false;
    
    /// <summary>
    /// Whether this is an end node
    /// </summary>
    public bool IsEndNode { get; set; } = false;
    
    /// <summary>
    /// Node configuration (JSON)
    /// </summary>
    public string? Configuration { get; set; }
    
    /// <summary>
    /// Timeout in minutes for this node (0 = use default)
    /// </summary>
    public int TimeoutMinutes { get; set; } = 0;
    
    /// <summary>
    /// Retry count on failure (0 = no retry)
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Retry delay in seconds
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 60;
    
    /// <summary>
    /// Whether to use exponential backoff for retries
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
    
    /// <summary>
    /// Order within the workflow (for sequential processing)
    /// </summary>
    public int ExecutionOrder { get; set; } = 0;
    
    /// <summary>
    /// Outgoing transitions
    /// </summary>
    public virtual ICollection<WorkflowTransition> OutgoingTransitions { get; set; } = new List<WorkflowTransition>();
    
    /// <summary>
    /// Incoming transitions
    /// </summary>
    public virtual ICollection<WorkflowTransition> IncomingTransitions { get; set; } = new List<WorkflowTransition>();
    
    /// <summary>
    /// Node instances (execution records)
    /// </summary>
    public virtual ICollection<WorkflowNodeInstance> NodeInstances { get; set; } = new List<WorkflowNodeInstance>();
}

/// <summary>
/// Workflow node type enumeration
/// </summary>
public enum WorkflowNodeType
{
    /// <summary>
    /// Trigger node - starts the workflow
    /// </summary>
    Trigger = 0,
    
    /// <summary>
    /// Condition node - branches based on conditions
    /// </summary>
    Condition = 1,
    
    /// <summary>
    /// Action node - performs automated actions
    /// </summary>
    Action = 2,
    
    /// <summary>
    /// Human task node - requires human intervention
    /// </summary>
    HumanTask = 3,
    
    /// <summary>
    /// Wait node - delays execution
    /// </summary>
    Wait = 4,
    
    /// <summary>
    /// Parallel gateway - splits into parallel paths
    /// </summary>
    ParallelGateway = 5,
    
    /// <summary>
    /// Join gateway - merges parallel paths
    /// </summary>
    JoinGateway = 6,
    
    /// <summary>
    /// Subprocess node - calls another workflow
    /// </summary>
    Subprocess = 7,
    
    /// <summary>
    /// LLM node - AI-powered action
    /// </summary>
    LLMAction = 8,
    
    /// <summary>
    /// End node - terminates workflow
    /// </summary>
    End = 9
}
