using System.Text.Json;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.StepExecutors;

/// <summary>
/// Executor for Parallel steps - executes multiple branches concurrently
/// </summary>
public class ParallelStepExecutor : IStepExecutor
{
    private readonly ILogger<ParallelStepExecutor> _logger;

    public ParallelStepExecutor(ILogger<ParallelStepExecutor> logger)
    {
        _logger = logger;
    }

    public IEnumerable<string> SupportedStepTypes => new[] { WorkflowStepTypes.Parallel };

    public Task<StepExecutionResult> ExecuteAsync(
        StepExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Parallel step {StepKey} for instance {InstanceId}",
            context.Step.StepKey, context.Instance.Id);

        try
        {
            // Parse configuration
            var config = !string.IsNullOrEmpty(context.Step.Configuration)
                ? JsonSerializer.Deserialize<ParallelStepConfig>(context.Step.Configuration)
                : null;

            var branches = config?.Branches ?? new List<string>();

            if (!branches.Any())
            {
                // Try to get branches from transitions
                if (!string.IsNullOrEmpty(context.Step.Transitions))
                {
                    var transitions = JsonSerializer.Deserialize<List<StepTransition>>(context.Step.Transitions);
                    branches = transitions?.Select(t => t.NextStepKey).Where(s => !string.IsNullOrEmpty(s)).ToList()!
                        ?? new List<string>();
                }
            }

            if (!branches.Any())
            {
                _logger.LogWarning("Parallel step {StepKey} has no branches to execute", context.Step.StepKey);
                return Task.FromResult(new StepExecutionResult
                {
                    Success = true,
                    OutputVariables = new Dictionary<string, object?>
                    {
                        [$"{context.Step.StepKey}_branches"] = branches
                    }
                });
            }

            _logger.LogInformation("Parallel step {StepKey} launching {Count} branches: {Branches}",
                context.Step.StepKey, branches.Count, string.Join(", ", branches));

            // Return multiple next steps to execute in parallel
            return Task.FromResult(new StepExecutionResult
            {
                Success = true,
                NextStepKeys = branches,
                OutputVariables = new Dictionary<string, object?>
                {
                    [$"{context.Step.StepKey}_branches"] = branches,
                    [$"{context.Step.StepKey}_branchCount"] = branches.Count,
                    [$"{context.Step.StepKey}_startedAt"] = DateTime.UtcNow.ToString("O")
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Parallel step {StepKey}", context.Step.StepKey);
            return Task.FromResult(new StepExecutionResult
            {
                Success = false,
                ErrorMessage = $"Error parsing parallel configuration: {ex.Message}",
                ErrorDetails = ex.StackTrace
            });
        }
    }

    public Task<WorkflowValidationResultDto> ValidateConfigurationAsync(
        WorkflowStep step, 
        CancellationToken cancellationToken = default)
    {
        var result = new WorkflowValidationResultDto { IsValid = true };

        var branches = new List<string>();

        if (!string.IsNullOrEmpty(step.Configuration))
        {
            try
            {
                var config = JsonSerializer.Deserialize<ParallelStepConfig>(step.Configuration);
                branches = config?.Branches ?? new List<string>();
            }
            catch (JsonException ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Step '{step.Name}' configuration parse error: {ex.Message}");
            }
        }

        if (!branches.Any() && !string.IsNullOrEmpty(step.Transitions))
        {
            try
            {
                var transitions = JsonSerializer.Deserialize<List<StepTransition>>(step.Transitions);
                branches = transitions?.Select(t => t.NextStepKey).Where(s => !string.IsNullOrEmpty(s)).ToList()!
                    ?? new List<string>();
            }
            catch { }
        }

        if (!branches.Any())
        {
            result.Warnings.Add($"Step '{step.Name}' has no parallel branches configured");
        }

        if (branches.Count == 1)
        {
            result.Warnings.Add($"Step '{step.Name}' has only one branch - consider using a simple transition instead");
        }

        return Task.FromResult(result);
    }
}

public class ParallelStepConfig
{
    public List<string> Branches { get; set; } = new();
    public string? JoinStepKey { get; set; }
    public bool WaitForAll { get; set; } = true;
    public int? RequiredCompletions { get; set; }
}

/// <summary>
/// Executor for Join steps - waits for parallel branches to complete
/// </summary>
public class JoinStepExecutor : IStepExecutor
{
    private readonly ILogger<JoinStepExecutor> _logger;

    public JoinStepExecutor(ILogger<JoinStepExecutor> logger)
    {
        _logger = logger;
    }

    public IEnumerable<string> SupportedStepTypes => new[] { WorkflowStepTypes.Join };

    public Task<StepExecutionResult> ExecuteAsync(
        StepExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Join step {StepKey} for instance {InstanceId}",
            context.Step.StepKey, context.Instance.Id);

        try
        {
            // Parse configuration
            var config = !string.IsNullOrEmpty(context.Step.Configuration)
                ? JsonSerializer.Deserialize<JoinStepConfig>(context.Step.Configuration)
                : null;

            // Check if all expected branches have completed
            var expectedBranches = config?.ExpectedBranches ?? new List<string>();
            var completedBranches = new List<string>();

            foreach (var branch in expectedBranches)
            {
                var completedKey = $"{branch}_completed";
                if (context.Variables.ContainsKey(completedKey))
                {
                    completedBranches.Add(branch);
                }
            }

            var requiredCount = config?.RequiredCompletions ?? expectedBranches.Count;
            var allComplete = completedBranches.Count >= requiredCount;

            if (!allComplete && expectedBranches.Any())
            {
                _logger.LogInformation("Join step {StepKey} waiting: {Completed}/{Required} branches complete",
                    context.Step.StepKey, completedBranches.Count, requiredCount);

                return Task.FromResult(new StepExecutionResult
                {
                    Success = true,
                    RequiresUserInput = false,
                    OutputVariables = new Dictionary<string, object?>
                    {
                        [$"{context.Step.StepKey}_waiting"] = true,
                        [$"{context.Step.StepKey}_completedBranches"] = completedBranches,
                        [$"{context.Step.StepKey}_expectedBranches"] = expectedBranches
                    }
                });
            }

            // All branches complete, proceed
            var nextStepKey = GetNextStep(context.Step);

            _logger.LogInformation("Join step {StepKey} complete, proceeding to {NextStep}",
                context.Step.StepKey, nextStepKey);

            return Task.FromResult(new StepExecutionResult
            {
                Success = true,
                NextStepKey = nextStepKey,
                OutputVariables = new Dictionary<string, object?>
                {
                    [$"{context.Step.StepKey}_completed"] = DateTime.UtcNow.ToString("O"),
                    [$"{context.Step.StepKey}_completedBranches"] = completedBranches
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Join step {StepKey}", context.Step.StepKey);
            return Task.FromResult(new StepExecutionResult
            {
                Success = false,
                ErrorMessage = $"Error in join step: {ex.Message}",
                ErrorDetails = ex.StackTrace
            });
        }
    }

    public Task<WorkflowValidationResultDto> ValidateConfigurationAsync(
        WorkflowStep step, 
        CancellationToken cancellationToken = default)
    {
        var result = new WorkflowValidationResultDto { IsValid = true };

        if (!string.IsNullOrEmpty(step.Configuration))
        {
            try
            {
                var config = JsonSerializer.Deserialize<JoinStepConfig>(step.Configuration);
                if (config?.ExpectedBranches == null || !config.ExpectedBranches.Any())
                {
                    result.Warnings.Add($"Step '{step.Name}' has no expected branches configured");
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

    private string? GetNextStep(WorkflowStep step)
    {
        if (string.IsNullOrEmpty(step.Transitions))
            return null;

        try
        {
            var transitions = JsonSerializer.Deserialize<List<StepTransition>>(step.Transitions);
            return transitions?.FirstOrDefault(t => string.IsNullOrEmpty(t.Condition))?.NextStepKey;
        }
        catch
        {
            return null;
        }
    }
}

public class JoinStepConfig
{
    public List<string> ExpectedBranches { get; set; } = new();
    public int? RequiredCompletions { get; set; }
    public int? TimeoutMinutes { get; set; }
}
