using System.Text.Json;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.StepExecutors;

/// <summary>
/// Executor for User Action steps - creates tasks for user input
/// </summary>
public class UserActionStepExecutor : IStepExecutor
{
    private readonly IWorkflowTaskRepository _taskRepo;
    private readonly ILogger<UserActionStepExecutor> _logger;

    public UserActionStepExecutor(
        IWorkflowTaskRepository taskRepo,
        ILogger<UserActionStepExecutor> logger)
    {
        _taskRepo = taskRepo;
        _logger = logger;
    }

    public IEnumerable<string> SupportedStepTypes => new[] { WorkflowStepTypes.UserAction };

    public async Task<StepExecutionResult> ExecuteAsync(
        StepExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing UserAction step {StepKey} for instance {InstanceId}",
            context.Step.StepKey, context.Instance.Id);

        try
        {
            // Parse configuration
            var config = !string.IsNullOrEmpty(context.Step.Configuration)
                ? JsonSerializer.Deserialize<UserActionStepConfig>(context.Step.Configuration)
                : new UserActionStepConfig();

            if (config == null)
            {
                return new StepExecutionResult
                {
                    Success = false,
                    ErrorMessage = "Invalid step configuration"
                };
            }

            // Check if there's already a pending task for this step
            var existingTasks = await _taskRepo.GetByInstanceAsync(context.Instance.Id, cancellationToken);
            var pendingTask = existingTasks.FirstOrDefault(t => 
                t.StepKey == context.Step.StepKey && 
                (t.Status == WorkflowTaskStatus.Pending || t.Status == WorkflowTaskStatus.InProgress));

            if (pendingTask != null)
            {
                // Task already exists, check if it's completed
                if (pendingTask.Status == WorkflowTaskStatus.Completed)
                {
                    // Determine next step based on action taken
                    var nextStepKey = await DetermineNextStepAsync(context, pendingTask.ActionTaken, cancellationToken);
                    
                    return new StepExecutionResult
                    {
                        Success = true,
                        NextStepKey = nextStepKey,
                        OutputVariables = new Dictionary<string, object?>
                        {
                            [$"{context.Step.StepKey}_action"] = pendingTask.ActionTaken,
                            [$"{context.Step.StepKey}_comments"] = pendingTask.Comments,
                            [$"{context.Step.StepKey}_formData"] = pendingTask.FormData
                        }
                    };
                }
                else
                {
                    // Still waiting for user input
                    return new StepExecutionResult
                    {
                        Success = true,
                        RequiresUserInput = true,
                        CreatedTaskId = pendingTask.Id
                    };
                }
            }

            // Create new task
            var task = new WorkflowTask
            {
                WorkflowInstanceId = context.Instance.Id,
                StepKey = context.Step.StepKey,
                Title = context.Step.Name,
                Description = context.Step.Description,
                Instructions = config.Instructions,
                Status = WorkflowTaskStatus.Pending,
                AssignmentType = config.AssignmentType,
                AssignedToUserId = config.AssignmentType == "User" && int.TryParse(config.AssignedTo, out var userId) 
                    ? userId : null,
                AssignedToGroupId = config.AssignedToGroupId,
                AssignedToRole = config.AssignmentType == "Role" ? config.AssignedTo : null,
                Priority = context.Instance.Priority,
                DueAt = context.Step.TimeoutMinutes > 0 
                    ? DateTime.UtcNow.AddMinutes(context.Step.TimeoutMinutes) 
                    : null,
                FormSchema = config.FormSchema,
                AvailableActions = config.AvailableActions != null 
                    ? JsonSerializer.Serialize(config.AvailableActions) 
                    : null
            };

            var createdTask = await _taskRepo.CreateAsync(task, cancellationToken);

            _logger.LogInformation("Created task {TaskId} for step {StepKey}", createdTask.Id, context.Step.StepKey);

            return new StepExecutionResult
            {
                Success = true,
                RequiresUserInput = true,
                CreatedTaskId = createdTask.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing UserAction step {StepKey}", context.Step.StepKey);
            return new StepExecutionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorDetails = ex.StackTrace
            };
        }
    }

    public Task<WorkflowValidationResultDto> ValidateConfigurationAsync(
        WorkflowStep step, 
        CancellationToken cancellationToken = default)
    {
        var result = new WorkflowValidationResultDto { IsValid = true };

        if (string.IsNullOrEmpty(step.Configuration))
        {
            result.Warnings.Add($"Step '{step.Name}' has no configuration");
            return Task.FromResult(result);
        }

        try
        {
            var config = JsonSerializer.Deserialize<UserActionStepConfig>(step.Configuration);
            if (config == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Step '{step.Name}' has invalid configuration JSON");
            }
            else if (string.IsNullOrEmpty(config.AssignmentType))
            {
                result.IsValid = false;
                result.Errors.Add($"Step '{step.Name}' must specify an assignment type");
            }
        }
        catch (JsonException ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Step '{step.Name}' configuration parse error: {ex.Message}");
        }

        return Task.FromResult(result);
    }

    private Task<string?> DetermineNextStepAsync(
        StepExecutionContext context, 
        string? actionTaken, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(context.Step.Transitions))
        {
            return Task.FromResult<string?>(null);
        }

        try
        {
            var transitions = JsonSerializer.Deserialize<List<StepTransition>>(context.Step.Transitions);
            if (transitions == null || !transitions.Any())
            {
                return Task.FromResult<string?>(null);
            }

            // Find matching transition based on action
            var matchingTransition = transitions
                .OrderByDescending(t => t.Priority)
                .FirstOrDefault(t => 
                    string.IsNullOrEmpty(t.Condition) || 
                    t.Condition.Contains(actionTaken ?? "", StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(matchingTransition?.NextStepKey);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }
}
