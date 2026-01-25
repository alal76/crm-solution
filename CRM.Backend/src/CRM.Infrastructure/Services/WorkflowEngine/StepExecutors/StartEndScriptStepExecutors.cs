using System.Text.Json;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.StepExecutors;

/// <summary>
/// Executor for Start/End steps - simple passthrough steps for workflow boundaries
/// </summary>
public class StartEndStepExecutor : IStepExecutor
{
    private readonly ILogger<StartEndStepExecutor> _logger;

    public StartEndStepExecutor(ILogger<StartEndStepExecutor> logger)
    {
        _logger = logger;
    }

    public IEnumerable<string> SupportedStepTypes => new[] 
    { 
        WorkflowStepTypes.Start, 
        WorkflowStepTypes.End 
    };

    public Task<StepExecutionResult> ExecuteAsync(
        StepExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        var isEnd = context.Step.StepType == WorkflowStepTypes.End || context.Step.IsEndStep;

        if (isEnd)
        {
            _logger.LogInformation("Executing End step {StepKey} for instance {InstanceId}",
                context.Step.StepKey, context.Instance.Id);

            return Task.FromResult(new StepExecutionResult
            {
                Success = true,
                NextStepKey = null, // End of workflow
                OutputVariables = new Dictionary<string, object?>
                {
                    [$"{context.Step.StepKey}_completedAt"] = DateTime.UtcNow.ToString("O")
                }
            });
        }
        else
        {
            _logger.LogInformation("Executing Start step {StepKey} for instance {InstanceId}",
                context.Step.StepKey, context.Instance.Id);

            var nextStepKey = GetNextStep(context.Step);

            return Task.FromResult(new StepExecutionResult
            {
                Success = true,
                NextStepKey = nextStepKey,
                OutputVariables = new Dictionary<string, object?>
                {
                    [$"{context.Step.StepKey}_startedAt"] = DateTime.UtcNow.ToString("O")
                }
            });
        }
    }

    public Task<WorkflowValidationResultDto> ValidateConfigurationAsync(
        WorkflowStep step, 
        CancellationToken cancellationToken = default)
    {
        var result = new WorkflowValidationResultDto { IsValid = true };

        var isStart = step.StepType == WorkflowStepTypes.Start || step.IsStartStep;
        var isEnd = step.StepType == WorkflowStepTypes.End || step.IsEndStep;

        if (isStart && string.IsNullOrEmpty(step.Transitions))
        {
            result.Warnings.Add($"Start step '{step.Name}' has no transitions defined");
        }

        if (isEnd && !string.IsNullOrEmpty(step.Transitions))
        {
            result.Warnings.Add($"End step '{step.Name}' has transitions that will be ignored");
        }

        return Task.FromResult(result);
    }

    private string? GetNextStep(WorkflowStep step)
    {
        if (string.IsNullOrEmpty(step.Transitions))
            return null;

        try
        {
            var transitions = JsonSerializer.Deserialize<List<StepTransition>>(step.Transitions);
            return transitions?.FirstOrDefault()?.NextStepKey;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Executor for Script steps - evaluates expressions and sets variables
/// </summary>
public class ScriptStepExecutor : IStepExecutor
{
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly ILogger<ScriptStepExecutor> _logger;

    public ScriptStepExecutor(
        IWorkflowExpressionEvaluator expressionEvaluator,
        ILogger<ScriptStepExecutor> logger)
    {
        _expressionEvaluator = expressionEvaluator;
        _logger = logger;
    }

    public IEnumerable<string> SupportedStepTypes => new[] { WorkflowStepTypes.Script };

    public async Task<StepExecutionResult> ExecuteAsync(
        StepExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Script step {StepKey} for instance {InstanceId}",
            context.Step.StepKey, context.Instance.Id);

        try
        {
            var config = !string.IsNullOrEmpty(context.Step.Configuration)
                ? JsonSerializer.Deserialize<ScriptStepConfig>(context.Step.Configuration)
                : null;

            if (config == null)
            {
                return new StepExecutionResult
                {
                    Success = true,
                    NextStepKey = GetNextStep(context.Step)
                };
            }

            var outputVariables = new Dictionary<string, object?>();

            // Process variable assignments
            if (config.Assignments != null)
            {
                foreach (var assignment in config.Assignments)
                {
                    try
                    {
                        var value = await _expressionEvaluator.EvaluateExpressionAsync(
                            assignment.Expression, context.Variables, cancellationToken);
                        
                        outputVariables[assignment.VariableName] = value;
                        
                        _logger.LogDebug("Script assigned {Variable} = {Value}", 
                            assignment.VariableName, value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to evaluate assignment for {Variable}", 
                            assignment.VariableName);
                        
                        if (config.FailOnError)
                        {
                            throw;
                        }
                    }
                }
            }

            // Process transformations
            if (config.Transformations != null)
            {
                foreach (var transform in config.Transformations)
                {
                    try
                    {
                        var result = await ApplyTransformationAsync(
                            transform, context.Variables, cancellationToken);
                        
                        if (!string.IsNullOrEmpty(transform.OutputVariable))
                        {
                            outputVariables[transform.OutputVariable] = result;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to apply transformation {Type}", 
                            transform.Type);
                        
                        if (config.FailOnError)
                        {
                            throw;
                        }
                    }
                }
            }

            return new StepExecutionResult
            {
                Success = true,
                NextStepKey = GetNextStep(context.Step),
                OutputVariables = outputVariables
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Script step {StepKey}", context.Step.StepKey);
            return new StepExecutionResult
            {
                Success = false,
                ErrorMessage = $"Script execution error: {ex.Message}",
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
            result.Warnings.Add($"Step '{step.Name}' has no script configuration");
            return Task.FromResult(result);
        }

        try
        {
            var config = JsonSerializer.Deserialize<ScriptStepConfig>(step.Configuration);
            
            if (config?.Assignments != null)
            {
                foreach (var assignment in config.Assignments)
                {
                    if (string.IsNullOrEmpty(assignment.VariableName))
                    {
                        result.Errors.Add($"Step '{step.Name}' has assignment without variable name");
                        result.IsValid = false;
                    }
                    if (string.IsNullOrEmpty(assignment.Expression))
                    {
                        result.Errors.Add($"Step '{step.Name}' has assignment without expression");
                        result.IsValid = false;
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Step '{step.Name}' configuration parse error: {ex.Message}");
        }

        return Task.FromResult(result);
    }

    private Task<object?> ApplyTransformationAsync(
        ScriptTransformation transform,
        Dictionary<string, object?> variables,
        CancellationToken cancellationToken)
    {
        var inputValue = variables.TryGetValue(transform.InputVariable ?? "", out var val) ? val : null;

        object? result = transform.Type?.ToLower() switch
        {
            "uppercase" => inputValue?.ToString()?.ToUpperInvariant(),
            "lowercase" => inputValue?.ToString()?.ToLowerInvariant(),
            "trim" => inputValue?.ToString()?.Trim(),
            "length" => inputValue?.ToString()?.Length ?? 0,
            "toint" => int.TryParse(inputValue?.ToString(), out var i) ? i : 0,
            "tofloat" => float.TryParse(inputValue?.ToString(), out var f) ? f : 0f,
            "tobool" => bool.TryParse(inputValue?.ToString(), out var b) && b,
            "tostring" => inputValue?.ToString(),
            "now" => DateTime.UtcNow,
            "today" => DateTime.UtcNow.Date,
            "guid" => Guid.NewGuid().ToString(),
            "json" => inputValue != null ? JsonSerializer.Serialize(inputValue) : null,
            "parsejson" => inputValue != null ? JsonSerializer.Deserialize<object>(inputValue.ToString()!) : null,
            _ => inputValue
        };
        
        return Task.FromResult(result);
    }

    private string? GetNextStep(WorkflowStep step)
    {
        if (string.IsNullOrEmpty(step.Transitions))
            return null;

        try
        {
            var transitions = JsonSerializer.Deserialize<List<StepTransition>>(step.Transitions);
            return transitions?.FirstOrDefault()?.NextStepKey;
        }
        catch
        {
            return null;
        }
    }
}

public class ScriptStepConfig
{
    public List<ScriptAssignment>? Assignments { get; set; }
    public List<ScriptTransformation>? Transformations { get; set; }
    public bool FailOnError { get; set; } = false;
}

public class ScriptAssignment
{
    public string VariableName { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
}

public class ScriptTransformation
{
    public string? InputVariable { get; set; }
    public string? OutputVariable { get; set; }
    public string? Type { get; set; }
    public Dictionary<string, object>? Options { get; set; }
}
