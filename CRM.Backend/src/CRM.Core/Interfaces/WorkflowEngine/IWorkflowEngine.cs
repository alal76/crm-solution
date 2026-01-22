using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;

namespace CRM.Core.Interfaces.WorkflowEngine;

/// <summary>
/// Main workflow engine interface for orchestrating workflow execution
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    Task<WorkflowInstanceDto> StartWorkflowAsync(StartWorkflowDto request, int? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Execute the next step(s) in a workflow instance
    /// </summary>
    Task<WorkflowInstanceDto> ProcessWorkflowAsync(int instanceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Pause a running workflow
    /// </summary>
    Task<WorkflowInstanceDto> PauseWorkflowAsync(int instanceId, int? userId = null, string? reason = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resume a paused workflow
    /// </summary>
    Task<WorkflowInstanceDto> ResumeWorkflowAsync(int instanceId, int? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancel a workflow
    /// </summary>
    Task<WorkflowInstanceDto> CancelWorkflowAsync(int instanceId, int? userId = null, string? reason = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retry a failed workflow from the last failed step
    /// </summary>
    Task<WorkflowInstanceDto> RetryWorkflowAsync(int instanceId, int? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get workflow instance with full details
    /// </summary>
    Task<WorkflowInstanceDetailDto?> GetInstanceDetailAsync(int instanceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Trigger workflows based on entity events
    /// </summary>
    Task TriggerWorkflowsAsync(string entityType, int entityId, string eventType, object? entityData = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Strategy interface for executing different step types
/// </summary>
public interface IStepExecutor
{
    /// <summary>
    /// Step types this executor handles
    /// </summary>
    IEnumerable<string> SupportedStepTypes { get; }
    
    /// <summary>
    /// Execute a workflow step
    /// </summary>
    Task<StepExecutionResult> ExecuteAsync(StepExecutionContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate step configuration
    /// </summary>
    Task<WorkflowValidationResultDto> ValidateConfigurationAsync(WorkflowStep step, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context passed to step executors
/// </summary>
public class StepExecutionContext
{
    public WorkflowInstance Instance { get; set; } = null!;
    public WorkflowStep Step { get; set; } = null!;
    public WorkflowDefinition Definition { get; set; } = null!;
    public Dictionary<string, object?> Variables { get; set; } = new();
    public int? ActorUserId { get; set; }
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Result of step execution
/// </summary>
public class StepExecutionResult
{
    public bool Success { get; set; }
    public string? NextStepKey { get; set; }
    public List<string>? NextStepKeys { get; set; } // For parallel execution
    public Dictionary<string, object?>? OutputVariables { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }
    public bool RequiresUserInput { get; set; }
    public int? CreatedTaskId { get; set; }
    public long DurationMs { get; set; }
    public bool ShouldRetry { get; set; }
    public DateTime? RetryAfter { get; set; }
    public bool RequiresScheduledResume { get; set; }
    public DateTime? ScheduledResumeAt { get; set; }
}

/// <summary>
/// Expression evaluator for workflow conditions
/// </summary>
public interface IWorkflowExpressionEvaluator
{
    /// <summary>
    /// Evaluate a condition expression
    /// </summary>
    Task<bool> EvaluateConditionAsync(string expression, Dictionary<string, object?> variables, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Evaluate an expression and return the result
    /// </summary>
    Task<object?> EvaluateExpressionAsync(string expression, Dictionary<string, object?> variables, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Replace template variables in a string
    /// </summary>
    Task<string> ReplaceVariablesAsync(string template, Dictionary<string, object?> variables, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate an expression syntax
    /// </summary>
    Task<WorkflowValidationResultDto> ValidateExpressionAsync(string expression, CancellationToken cancellationToken = default);
}

/// <summary>
/// Workflow definition repository interface
/// </summary>
public interface IWorkflowDefinitionRepository
{
    Task<WorkflowDefinition?> GetByIdAsync(int id, bool includeSteps = true, CancellationToken cancellationToken = default);
    Task<List<WorkflowDefinition>> GetAllAsync(string? status = null, string? triggerType = null, CancellationToken cancellationToken = default);
    Task<List<WorkflowDefinition>> GetByTriggerAsync(string triggerEntityType, string triggerEvent, CancellationToken cancellationToken = default);
    Task<WorkflowDefinition> CreateAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default);
    Task<WorkflowDefinition> UpdateAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<WorkflowDefinition> PublishAsync(int id, int? userId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Workflow instance repository interface
/// </summary>
public interface IWorkflowInstanceRepository
{
    Task<WorkflowInstance?> GetByIdAsync(int id, bool includeDetails = false, CancellationToken cancellationToken = default);
    Task<List<WorkflowInstance>> GetActiveInstancesAsync(int? limit = null, CancellationToken cancellationToken = default);
    Task<List<WorkflowInstance>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default);
    Task<WorkflowInstance> CreateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);
    Task<WorkflowInstance> UpdateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);
    Task<bool> TryUpdateWithOptimisticLockAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);
}

/// <summary>
/// Workflow task repository interface
/// </summary>
public interface IWorkflowTaskRepository
{
    Task<WorkflowTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<WorkflowTask>> GetPendingTasksAsync(int? userId = null, int? groupId = null, string? role = null, CancellationToken cancellationToken = default);
    Task<List<WorkflowTask>> GetByInstanceAsync(int instanceId, CancellationToken cancellationToken = default);
    Task<List<WorkflowTask>> GetOverdueTasksAsync(CancellationToken cancellationToken = default);
    Task<WorkflowTask> CreateAsync(WorkflowTask task, CancellationToken cancellationToken = default);
    Task<WorkflowTask> UpdateAsync(WorkflowTask task, CancellationToken cancellationToken = default);
    Task<WorkflowTask> CompleteAsync(int taskId, CompleteTaskDto request, int userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Workflow event repository interface (append-only)
/// </summary>
public interface IWorkflowEventRepository
{
    Task<List<WorkflowEvent>> GetByInstanceAsync(int instanceId, CancellationToken cancellationToken = default);
    Task<WorkflowEvent> LogEventAsync(WorkflowEvent @event, CancellationToken cancellationToken = default);
    Task<List<WorkflowEvent>> QueryEventsAsync(
        DateTime? from = null, 
        DateTime? to = null, 
        string? eventType = null, 
        int? limit = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Workflow job queue interface
/// </summary>
public interface IWorkflowJobQueue
{
    Task EnqueueAsync(WorkflowJob job, CancellationToken cancellationToken = default);
    Task<WorkflowJob?> DequeueAsync(string workerId, CancellationToken cancellationToken = default);
    Task<List<WorkflowJob>> DequeueMultipleAsync(string workerId, int count, CancellationToken cancellationToken = default);
    Task CompleteAsync(int jobId, string? resultData = null, CancellationToken cancellationToken = default);
    Task FailAsync(int jobId, string errorMessage, bool shouldRetry = true, CancellationToken cancellationToken = default);
    Task<List<WorkflowJob>> GetPendingJobsAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task CleanupCompletedAsync(TimeSpan olderThan, CancellationToken cancellationToken = default);
    Task<int> RecoverStuckJobsAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
}
