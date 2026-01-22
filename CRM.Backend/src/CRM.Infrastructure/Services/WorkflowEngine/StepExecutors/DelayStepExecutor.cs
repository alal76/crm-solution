using System.Text.Json;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.StepExecutors;

/// <summary>
/// Executor for Delay steps - pauses workflow for a specified duration
/// </summary>
public class DelayStepExecutor : IStepExecutor
{
    private readonly ILogger<DelayStepExecutor> _logger;

    public DelayStepExecutor(ILogger<DelayStepExecutor> logger)
    {
        _logger = logger;
    }

    public IEnumerable<string> SupportedStepTypes => new[] { WorkflowStepTypes.Delay };

    public async Task<StepExecutionResult> ExecuteAsync(
        StepExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Delay step {StepKey} for instance {InstanceId}",
            context.Step.StepKey, context.Instance.Id);

        try
        {
            // Parse configuration
            var config = !string.IsNullOrEmpty(context.Step.Configuration)
                ? JsonSerializer.Deserialize<DelayStepConfig>(context.Step.Configuration)
                : null;

            if (config == null)
            {
                config = new DelayStepConfig { DelayMinutes = 0 };
            }

            // Calculate delay duration
            var delayUntil = CalculateDelayUntil(config);
            
            if (delayUntil <= DateTime.UtcNow)
            {
                // Delay has passed or no delay needed
                return await CompleteWithNextStepAsync(context, cancellationToken);
            }

            _logger.LogInformation("Delay step {StepKey} scheduled until {DelayUntil}",
                context.Step.StepKey, delayUntil);

            // Return with scheduled resume time
            return new StepExecutionResult
            {
                Success = true,
                RequiresUserInput = false,
                RequiresScheduledResume = true,
                ScheduledResumeAt = delayUntil,
                OutputVariables = new Dictionary<string, object?>
                {
                    [$"{context.Step.StepKey}_delayUntil"] = delayUntil.ToString("O"),
                    [$"{context.Step.StepKey}_delayMinutes"] = config.DelayMinutes
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Delay step {StepKey}", context.Step.StepKey);
            return new StepExecutionResult
            {
                Success = false,
                ErrorMessage = $"Error calculating delay: {ex.Message}",
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
            result.Warnings.Add($"Step '{step.Name}' has no delay configuration - will proceed immediately");
            return Task.FromResult(result);
        }

        try
        {
            var config = JsonSerializer.Deserialize<DelayStepConfig>(step.Configuration);
            if (config == null)
            {
                result.Warnings.Add($"Step '{step.Name}' has null configuration");
            }
            else
            {
                var hasDelay = config.DelayMinutes > 0 
                    || config.DelayHours > 0 
                    || config.DelayDays > 0
                    || !string.IsNullOrEmpty(config.DelayUntilTime)
                    || config.DelayUntilDateTime.HasValue;

                if (!hasDelay)
                {
                    result.Warnings.Add($"Step '{step.Name}' has no delay duration - will proceed immediately");
                }

                if (!string.IsNullOrEmpty(config.DelayUntilTime))
                {
                    if (!TimeOnly.TryParse(config.DelayUntilTime, out _))
                    {
                        result.Errors.Add($"Step '{step.Name}' has invalid time format");
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

    private DateTime CalculateDelayUntil(DelayStepConfig config)
    {
        var now = DateTime.UtcNow;

        // If specific datetime is set, use it
        if (config.DelayUntilDateTime.HasValue)
        {
            return config.DelayUntilDateTime.Value;
        }

        // If specific time is set (for next occurrence of that time)
        if (!string.IsNullOrEmpty(config.DelayUntilTime))
        {
            if (TimeOnly.TryParse(config.DelayUntilTime, out var targetTime))
            {
                var targetDateTime = now.Date.Add(targetTime.ToTimeSpan());
                if (targetDateTime <= now)
                {
                    targetDateTime = targetDateTime.AddDays(1); // Next day
                }
                return targetDateTime;
            }
        }

        // Calculate relative delay
        var totalMinutes = config.DelayMinutes
            + (config.DelayHours * 60)
            + (config.DelayDays * 24 * 60);

        return now.AddMinutes(totalMinutes);
    }

    private async Task<StepExecutionResult> CompleteWithNextStepAsync(
        StepExecutionContext context,
        CancellationToken cancellationToken)
    {
        var nextStepKey = await GetNextStepAsync(context, cancellationToken);

        return new StepExecutionResult
        {
            Success = true,
            NextStepKey = nextStepKey,
            OutputVariables = new Dictionary<string, object?>
            {
                [$"{context.Step.StepKey}_completed"] = DateTime.UtcNow.ToString("O")
            }
        };
    }

    private Task<string?> GetNextStepAsync(
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
            var defaultTransition = transitions?.FirstOrDefault(t => string.IsNullOrEmpty(t.Condition));
            return Task.FromResult(defaultTransition?.NextStepKey);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }
}
