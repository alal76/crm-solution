using System.Text.Json;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.StepExecutors;

/// <summary>
/// Executor for Conditional steps - evaluates conditions and branches workflow
/// </summary>
public class ConditionalStepExecutor : IStepExecutor
{
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly ILogger<ConditionalStepExecutor> _logger;

    public ConditionalStepExecutor(
        IWorkflowExpressionEvaluator expressionEvaluator,
        ILogger<ConditionalStepExecutor> logger)
    {
        _expressionEvaluator = expressionEvaluator;
        _logger = logger;
    }

    public IEnumerable<string> SupportedStepTypes => new[] { WorkflowStepTypes.Conditional };

    public async Task<StepExecutionResult> ExecuteAsync(
        StepExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Conditional step {StepKey} for instance {InstanceId}",
            context.Step.StepKey, context.Instance.Id);

        try
        {
            // Parse configuration
            var config = !string.IsNullOrEmpty(context.Step.Configuration)
                ? JsonSerializer.Deserialize<ConditionalStepConfig>(context.Step.Configuration)
                : null;

            if (config == null || config.Conditions == null || !config.Conditions.Any())
            {
                // Fall back to transitions if no conditions in config
                return await EvaluateTransitionsAsync(context, cancellationToken);
            }

            // Evaluate conditions in order
            foreach (var condition in config.Conditions.OrderByDescending(c => c.Priority))
            {
                var matches = await _expressionEvaluator.EvaluateConditionAsync(
                    condition.Expression, context.Variables, cancellationToken);

                if (matches)
                {
                    _logger.LogInformation("Condition '{Label}' matched, transitioning to {NextStep}",
                        condition.Label ?? condition.Expression, condition.NextStepKey);

                    return new StepExecutionResult
                    {
                        Success = true,
                        NextStepKey = condition.NextStepKey,
                        OutputVariables = new Dictionary<string, object?>
                        {
                            [$"{context.Step.StepKey}_matchedCondition"] = condition.Label ?? condition.Expression,
                            [$"{context.Step.StepKey}_result"] = true
                        }
                    };
                }
            }

            // No condition matched - use default or first transition
            var defaultNextStep = config.Conditions.FirstOrDefault(c => c.IsDefault)?.NextStepKey;
            
            if (string.IsNullOrEmpty(defaultNextStep))
            {
                defaultNextStep = await GetDefaultTransitionAsync(context, cancellationToken);
            }

            _logger.LogInformation("No condition matched, using default transition to {NextStep}",
                defaultNextStep ?? "none");

            return new StepExecutionResult
            {
                Success = true,
                NextStepKey = defaultNextStep,
                OutputVariables = new Dictionary<string, object?>
                {
                    [$"{context.Step.StepKey}_matchedCondition"] = "default",
                    [$"{context.Step.StepKey}_result"] = false
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Conditional step {StepKey}", context.Step.StepKey);
            return new StepExecutionResult
            {
                Success = false,
                ErrorMessage = $"Error evaluating conditions: {ex.Message}",
                ErrorDetails = ex.StackTrace
            };
        }
    }

    public Task<WorkflowValidationResultDto> ValidateConfigurationAsync(
        WorkflowStep step, 
        CancellationToken cancellationToken = default)
    {
        var result = new WorkflowValidationResultDto { IsValid = true };

        if (string.IsNullOrEmpty(step.Configuration) && string.IsNullOrEmpty(step.Transitions))
        {
            result.IsValid = false;
            result.Errors.Add($"Step '{step.Name}' requires either conditions configuration or transitions");
            return Task.FromResult(result);
        }

        if (!string.IsNullOrEmpty(step.Configuration))
        {
            try
            {
                var config = JsonSerializer.Deserialize<ConditionalStepConfig>(step.Configuration);
                if (config?.Conditions == null || !config.Conditions.Any())
                {
                    result.Warnings.Add($"Step '{step.Name}' has no conditions defined");
                }
                else
                {
                    foreach (var condition in config.Conditions)
                    {
                        if (string.IsNullOrEmpty(condition.Expression) && !condition.IsDefault)
                        {
                            result.IsValid = false;
                            result.Errors.Add($"Step '{step.Name}' has a condition without an expression");
                        }

                        if (string.IsNullOrEmpty(condition.NextStepKey))
                        {
                            result.IsValid = false;
                            result.Errors.Add($"Step '{step.Name}' has a condition without a target step");
                        }
                    }

                    var defaultCount = config.Conditions.Count(c => c.IsDefault);
                    if (defaultCount == 0)
                    {
                        result.Warnings.Add($"Step '{step.Name}' has no default condition");
                    }
                    else if (defaultCount > 1)
                    {
                        result.Warnings.Add($"Step '{step.Name}' has multiple default conditions");
                    }
                }
            }
            catch (JsonException ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Step '{step.Name}' configuration parse error: {ex.Message}");
            }
        }

        return Task.FromResult(result);
    }

    private async Task<StepExecutionResult> EvaluateTransitionsAsync(
        StepExecutionContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(context.Step.Transitions))
        {
            return new StepExecutionResult
            {
                Success = true,
                NextStepKey = null
            };
        }

        try
        {
            var transitions = JsonSerializer.Deserialize<List<StepTransition>>(context.Step.Transitions);
            if (transitions == null || !transitions.Any())
            {
                return new StepExecutionResult { Success = true };
            }

            foreach (var transition in transitions.OrderByDescending(t => t.Priority))
            {
                if (string.IsNullOrEmpty(transition.Condition))
                {
                    return new StepExecutionResult
                    {
                        Success = true,
                        NextStepKey = transition.NextStepKey
                    };
                }

                var matches = await _expressionEvaluator.EvaluateConditionAsync(
                    transition.Condition, context.Variables, cancellationToken);

                if (matches)
                {
                    return new StepExecutionResult
                    {
                        Success = true,
                        NextStepKey = transition.NextStepKey
                    };
                }
            }

            return new StepExecutionResult { Success = true };
        }
        catch
        {
            return new StepExecutionResult { Success = true };
        }
    }

    private Task<string?> GetDefaultTransitionAsync(
        StepExecutionContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(context.Step.Transitions))
        {
            return Task.FromResult<string?>(null);
        }

        try
        {
            var transitions = JsonSerializer.Deserialize<List<StepTransition>>(context.Step.Transitions);
            return Task.FromResult(transitions?.FirstOrDefault(t => string.IsNullOrEmpty(t.Condition))?.NextStepKey);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }
}
