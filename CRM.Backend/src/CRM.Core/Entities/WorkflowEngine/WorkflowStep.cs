using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.WorkflowEngine;

/// <summary>
/// Workflow step entity - defines individual steps within a workflow
/// </summary>
public class WorkflowStep : BaseEntity
{
    public int WorkflowDefinitionId { get; set; }
    
    /// <summary>
    /// Unique identifier for the step within the workflow
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string StepKey { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Step type: UserAction, ApiCall, Conditional, Parallel, Delay, SubWorkflow, Start, End
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string StepType { get; set; } = WorkflowStepTypes.UserAction;
    
    /// <summary>
    /// Execution order within the workflow
    /// </summary>
    public int OrderIndex { get; set; } = 0;
    
    /// <summary>
    /// JSON configuration specific to step type
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? Configuration { get; set; }
    
    /// <summary>
    /// JSON array of transition rules
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? Transitions { get; set; }
    
    /// <summary>
    /// Timeout in minutes (0 = no timeout)
    /// </summary>
    public int TimeoutMinutes { get; set; } = 0;
    
    /// <summary>
    /// JSON retry policy configuration
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? RetryPolicy { get; set; }
    
    /// <summary>
    /// Whether this step is the start of the workflow
    /// </summary>
    public bool IsStartStep { get; set; } = false;
    
    /// <summary>
    /// Whether this step is an end step
    /// </summary>
    public bool IsEndStep { get; set; } = false;
    
    /// <summary>
    /// For visual designer - X coordinate
    /// </summary>
    public int? PositionX { get; set; }
    
    /// <summary>
    /// For visual designer - Y coordinate
    /// </summary>
    public int? PositionY { get; set; }
    
    // Navigation properties
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
}

/// <summary>
/// Step type constants
/// </summary>
public static class WorkflowStepTypes
{
    public const string Start = "Start";
    public const string End = "End";
    public const string UserAction = "UserAction";
    public const string ApiCall = "ApiCall";
    public const string Conditional = "Conditional";
    public const string Parallel = "Parallel";
    public const string Join = "Join";
    public const string Delay = "Delay";
    public const string SubWorkflow = "SubWorkflow";
    public const string Notification = "Notification";
    public const string SetVariable = "SetVariable";
    public const string Script = "Script";
}

/// <summary>
/// Configuration for UserAction step
/// </summary>
public class UserActionStepConfig
{
    /// <summary>
    /// How to assign: Role, User, Queue, PreviousStepActor
    /// </summary>
    public string AssignmentType { get; set; } = "Role";
    
    /// <summary>
    /// ID or name of assignee based on AssignmentType
    /// </summary>
    public string? AssignedTo { get; set; }
    
    /// <summary>
    /// User group ID for queue assignment
    /// </summary>
    public int? AssignedToGroupId { get; set; }
    
    /// <summary>
    /// Form schema JSON for data collection
    /// </summary>
    public string? FormSchema { get; set; }
    
    /// <summary>
    /// Validation rules JSON
    /// </summary>
    public string? ValidationRules { get; set; }
    
    /// <summary>
    /// Instructions for the user
    /// </summary>
    public string? Instructions { get; set; }
    
    /// <summary>
    /// Available actions for the user (e.g., ["Approve", "Reject", "RequestInfo"])
    /// </summary>
    public List<string>? AvailableActions { get; set; }
}

/// <summary>
/// Configuration for ApiCall step
/// </summary>
public class ApiCallStepConfig
{
    public string ApiEndpoint { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public Dictionary<string, string>? Headers { get; set; }
    public string? BodyTemplate { get; set; }
    public string? AuthenticationType { get; set; }
    public string? AuthenticationConfig { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public RetryPolicyConfig? RetryPolicy { get; set; }
    public string? ResponseMapping { get; set; }
}

/// <summary>
/// Configuration for Conditional step
/// </summary>
public class ConditionalStepConfig
{
    public List<ConditionBranch>? Branches { get; set; }
    public List<ConditionBranch>? Conditions { get; set; }
    public string? DefaultNextStepKey { get; set; }
}

public class ConditionBranch
{
    public string? Label { get; set; }
    public string Expression { get; set; } = string.Empty;
    public string NextStepKey { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Configuration for Delay step
/// </summary>
public class DelayStepConfig
{
    /// <summary>
    /// Delay type: Duration, UntilDateTime, Expression
    /// </summary>
    public string DelayType { get; set; } = "Duration";
    public int? DurationMinutes { get; set; }
    public int DelayMinutes { get; set; }
    public int DelayHours { get; set; }
    public int DelayDays { get; set; }
    public string? DelayUntilTime { get; set; }
    public DateTime? DelayUntilDateTime { get; set; }
    public DateTime? UntilDateTime { get; set; }
    public string? Expression { get; set; }
    public string? TimeZone { get; set; }
}

/// <summary>
/// Retry policy configuration
/// </summary>
public class RetryPolicyConfig
{
    public int MaxAttempts { get; set; } = 3;
    public List<int>? BackoffSeconds { get; set; }
    public bool ExponentialBackoff { get; set; } = true;
}

/// <summary>
/// Transition rule from one step to another
/// </summary>
public class StepTransition
{
    /// <summary>
    /// Condition expression that must evaluate to true
    /// </summary>
    public string? Condition { get; set; }
    
    /// <summary>
    /// Target step key
    /// </summary>
    public string NextStepKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Label for the transition (for visual designer)
    /// </summary>
    public string? Label { get; set; }
    
    /// <summary>
    /// Priority when multiple transitions match
    /// </summary>
    public int Priority { get; set; } = 0;
}

/// <summary>
/// Configuration for Notification steps
/// </summary>
public class NotificationStepConfig
{
    public string NotificationType { get; set; } = "Email"; // Email, InApp, Webhook
    public List<string>? Recipients { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? TemplateId { get; set; }
    public string? WebhookUrl { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Configuration for Parallel steps
/// </summary>
public class ParallelStepConfig
{
    public List<string> BranchStepKeys { get; set; } = new();
    public string JoinStepKey { get; set; } = string.Empty;
    public string JoinMode { get; set; } = "All"; // All, Any, N
    public int? JoinThreshold { get; set; }
}

/// <summary>
/// Configuration for Script steps
/// </summary>
public class ScriptStepConfig
{
    public string ScriptType { get; set; } = "Assignment"; // Assignment, Transform, Compute
    public List<VariableAssignment>? Assignments { get; set; }
    public string? Expression { get; set; }
}

/// <summary>
/// Variable assignment for script steps
/// </summary>
public class VariableAssignment
{
    public string VariableName { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
}
