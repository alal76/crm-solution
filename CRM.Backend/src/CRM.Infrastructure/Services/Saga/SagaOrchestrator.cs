// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Saga;

/// <summary>
/// Interface for saga pattern orchestration in distributed transactions.
/// TODO-INFRA-07
/// </summary>
public interface ISagaOrchestrator
{
    /// <summary>
    /// Starts a new saga with a sequence of steps.
    /// </summary>
    Task<SagaResult> StartSagaAsync(SagaDefinition definition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compensates (rolls back) a failed saga.
    /// </summary>
    Task<SagaResult> CompensateAsync(string sagaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current state of a saga.
    /// </summary>
    Task<SagaState?> GetSagaStateAsync(string sagaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists active sagas.
    /// </summary>
    Task<IEnumerable<SagaState>> GetActiveSagasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries a failed saga from the last successful step.
    /// </summary>
    Task<SagaResult> RetrySagaAsync(string sagaId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a saga with its steps and compensation actions.
/// </summary>
public class SagaDefinition
{
    public string Name { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public int? InitiatedByUserId { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public List<SagaStep> Steps { get; set; } = new();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// A single step in a saga.
/// </summary>
public class SagaStep
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public Func<SagaContext, CancellationToken, Task<SagaStepResult>>? Execute { get; set; }
    public Func<SagaContext, CancellationToken, Task>? Compensate { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Shared context passed between saga steps.
/// </summary>
public class SagaContext
{
    public string SagaId { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();

    public T Get<T>(string key) where T : class
    {
        return Data.TryGetValue(key, out var value)
            ? (value as T)!
            : throw new KeyNotFoundException($"Key '{key}' not found in saga context");
    }

    public void Set(string key, object value) => Data[key] = value;

    public T? TryGet<T>(string key) where T : class
    {
        return Data.TryGetValue(key, out var value) ? value as T : null;
    }
}

/// <summary>
/// Result of a saga step execution.
/// </summary>
public class SagaStepResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? OutputData { get; set; }

    public static SagaStepResult Succeeded(Dictionary<string, object>? output = null)
        => new() { Success = true, OutputData = output };

    public static SagaStepResult Failed(string error)
        => new() { Success = false, ErrorMessage = error };
}

/// <summary>
/// Result of a saga execution.
/// </summary>
public class SagaResult
{
    public string SagaId { get; set; } = string.Empty;
    public string SagaName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public SagaStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public int CompensatedSteps { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Current state of a saga.
/// </summary>
public class SagaState
{
    public string SagaId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public SagaStatus Status { get; set; }
    public int CurrentStepIndex { get; set; }
    public int TotalSteps { get; set; }
    public List<SagaStepState> StepStates { get; set; } = new();
    public SagaContext Context { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CorrelationId { get; set; }
    public int? InitiatedByUserId { get; set; }
}

/// <summary>
/// State of an individual saga step.
/// </summary>
public class SagaStepState
{
    public string StepName { get; set; } = string.Empty;
    public SagaStepStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum SagaStatus { Pending, Running, Completed, Failed, Compensating, Compensated }
public enum SagaStepStatus { Pending, Running, Completed, Failed, Compensated, Skipped }

/// <summary>
/// Orchestrates distributed transactions using the saga pattern.
/// Executes steps sequentially and compensates on failure.
/// </summary>
public class SagaOrchestrator : ISagaOrchestrator
{
    private readonly ILogger<SagaOrchestrator> _logger;

    // In-memory saga state store (in production, persist to DB/Redis)
    private static readonly Dictionary<string, SagaState> _sagaStates = new();
    private static readonly Dictionary<string, SagaDefinition> _sagaDefinitions = new();
    private static readonly object _lock = new();

    public SagaOrchestrator(ILogger<SagaOrchestrator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SagaResult> StartSagaAsync(
        SagaDefinition definition,
        CancellationToken cancellationToken = default)
    {
        var sagaId = Guid.NewGuid().ToString("N")[..12];
        var startedAt = DateTime.UtcNow;

        var state = new SagaState
        {
            SagaId = sagaId,
            Name = definition.Name,
            Status = SagaStatus.Running,
            TotalSteps = definition.Steps.Count,
            Context = new SagaContext { SagaId = sagaId, Data = new(definition.Context) },
            StartedAt = startedAt,
            CorrelationId = definition.CorrelationId,
            InitiatedByUserId = definition.InitiatedByUserId,
            StepStates = definition.Steps.Select(s => new SagaStepState
            {
                StepName = s.Name,
                Status = SagaStepStatus.Pending
            }).ToList()
        };

        lock (_lock)
        {
            _sagaStates[sagaId] = state;
            _sagaDefinitions[sagaId] = definition;
        }

        _logger.LogInformation(
            "Saga {SagaId} ({Name}) started with {StepCount} steps",
            sagaId, definition.Name, definition.Steps.Count);

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(definition.Timeout);

            for (int i = 0; i < definition.Steps.Count; i++)
            {
                cts.Token.ThrowIfCancellationRequested();

                var step = definition.Steps[i];
                state.CurrentStepIndex = i;
                state.StepStates[i].Status = SagaStepStatus.Running;
                state.StepStates[i].StartedAt = DateTime.UtcNow;

                var stepResult = await ExecuteStepWithRetryAsync(
                    step, state.Context, cts.Token);

                if (stepResult.Success)
                {
                    state.StepStates[i].Status = SagaStepStatus.Completed;
                    state.StepStates[i].CompletedAt = DateTime.UtcNow;

                    // Merge output data into context
                    if (stepResult.OutputData != null)
                    {
                        foreach (var kvp in stepResult.OutputData)
                        {
                            state.Context.Data[kvp.Key] = kvp.Value;
                        }
                    }

                    _logger.LogDebug(
                        "Saga {SagaId} step {StepName} completed successfully",
                        sagaId, step.Name);
                }
                else
                {
                    state.StepStates[i].Status = SagaStepStatus.Failed;
                    state.StepStates[i].ErrorMessage = stepResult.ErrorMessage;

                    _logger.LogWarning(
                        "Saga {SagaId} step {StepName} failed: {Error}. Starting compensation.",
                        sagaId, step.Name, stepResult.ErrorMessage);

                    // Start compensation from the previous completed step
                    var compensateResult = await CompensateFromStepAsync(sagaId, i - 1, cts.Token);

                    state.Status = SagaStatus.Compensated;
                    state.CompletedAt = DateTime.UtcNow;
                    state.ErrorMessage = stepResult.ErrorMessage;

                    return new SagaResult
                    {
                        SagaId = sagaId,
                        SagaName = definition.Name,
                        Success = false,
                        Status = SagaStatus.Compensated,
                        ErrorMessage = stepResult.ErrorMessage,
                        CompletedSteps = i,
                        TotalSteps = definition.Steps.Count,
                        CompensatedSteps = compensateResult,
                        Duration = DateTime.UtcNow - startedAt,
                        StartedAt = startedAt,
                        CompletedAt = DateTime.UtcNow
                    };
                }
            }

            state.Status = SagaStatus.Completed;
            state.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Saga {SagaId} ({Name}) completed successfully in {Duration}ms",
                sagaId, definition.Name, (DateTime.UtcNow - startedAt).TotalMilliseconds);

            return new SagaResult
            {
                SagaId = sagaId,
                SagaName = definition.Name,
                Success = true,
                Status = SagaStatus.Completed,
                CompletedSteps = definition.Steps.Count,
                TotalSteps = definition.Steps.Count,
                Duration = DateTime.UtcNow - startedAt,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (OperationCanceledException)
        {
            state.Status = SagaStatus.Failed;
            state.ErrorMessage = "Saga timed out";
            state.CompletedAt = DateTime.UtcNow;

            _logger.LogError("Saga {SagaId} ({Name}) timed out", sagaId, definition.Name);

            return new SagaResult
            {
                SagaId = sagaId,
                SagaName = definition.Name,
                Success = false,
                Status = SagaStatus.Failed,
                ErrorMessage = "Saga timed out",
                CompletedSteps = state.CurrentStepIndex,
                TotalSteps = definition.Steps.Count,
                Duration = DateTime.UtcNow - startedAt,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc />
    public async Task<SagaResult> CompensateAsync(
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        SagaState? state;
        lock (_lock)
        {
            _sagaStates.TryGetValue(sagaId, out state);
        }

        if (state == null)
        {
            return new SagaResult
            {
                SagaId = sagaId,
                Success = false,
                ErrorMessage = "Saga not found"
            };
        }

        // Find last completed step
        var lastCompleted = state.StepStates
            .Select((s, i) => new { State = s, Index = i })
            .LastOrDefault(x => x.State.Status == SagaStepStatus.Completed);

        if (lastCompleted == null)
        {
            return new SagaResult
            {
                SagaId = sagaId,
                SagaName = state.Name,
                Success = true,
                Status = SagaStatus.Compensated
            };
        }

        state.Status = SagaStatus.Compensating;
        var compensated = await CompensateFromStepAsync(sagaId, lastCompleted.Index, cancellationToken);

        state.Status = SagaStatus.Compensated;
        state.CompletedAt = DateTime.UtcNow;

        return new SagaResult
        {
            SagaId = sagaId,
            SagaName = state.Name,
            Success = true,
            Status = SagaStatus.Compensated,
            CompensatedSteps = compensated,
            TotalSteps = state.TotalSteps,
            CompletedAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public Task<SagaState?> GetSagaStateAsync(
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _sagaStates.TryGetValue(sagaId, out var state);
            return Task.FromResult(state);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<SagaState>> GetActiveSagasAsync(
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var active = _sagaStates.Values
                .Where(s => s.Status == SagaStatus.Running || s.Status == SagaStatus.Compensating)
                .OrderByDescending(s => s.StartedAt)
                .ToList();
            return Task.FromResult<IEnumerable<SagaState>>(active);
        }
    }

    /// <inheritdoc />
    public async Task<SagaResult> RetrySagaAsync(
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        SagaState? state;
        SagaDefinition? definition;

        lock (_lock)
        {
            _sagaStates.TryGetValue(sagaId, out state);
            _sagaDefinitions.TryGetValue(sagaId, out definition);
        }

        if (state == null || definition == null)
        {
            return new SagaResult
            {
                SagaId = sagaId,
                Success = false,
                ErrorMessage = "Saga not found"
            };
        }

        // Create a new saga from the same definition
        _logger.LogInformation("Retrying saga {SagaId} ({Name})", sagaId, state.Name);
        return await StartSagaAsync(definition, cancellationToken);
    }

    private async Task<SagaStepResult> ExecuteStepWithRetryAsync(
        SagaStep step,
        SagaContext context,
        CancellationToken cancellationToken)
    {
        if (step.Execute == null)
        {
            return SagaStepResult.Succeeded();
        }

        for (int attempt = 0; attempt <= step.MaxRetries; attempt++)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(step.Timeout);

                return await step.Execute(context, cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (attempt == step.MaxRetries)
                    return SagaStepResult.Failed($"Step '{step.Name}' timed out after {step.MaxRetries + 1} attempts");
            }
            catch (Exception ex) when (attempt < step.MaxRetries)
            {
                _logger.LogWarning(ex,
                    "Step '{StepName}' attempt {Attempt}/{Max} failed, retrying",
                    step.Name, attempt + 1, step.MaxRetries + 1);
                await Task.Delay(Math.Min(1000 * (int)Math.Pow(2, attempt), 10000), cancellationToken);
            }
            catch (Exception ex)
            {
                return SagaStepResult.Failed($"Step '{step.Name}' failed: {ex.Message}");
            }
        }

        return SagaStepResult.Failed($"Step '{step.Name}' exhausted retries");
    }

    private async Task<int> CompensateFromStepAsync(
        string sagaId,
        int fromStepIndex,
        CancellationToken cancellationToken)
    {
        SagaState? state;
        SagaDefinition? definition;

        lock (_lock)
        {
            _sagaStates.TryGetValue(sagaId, out state);
            _sagaDefinitions.TryGetValue(sagaId, out definition);
        }

        if (state == null || definition == null) return 0;

        var compensated = 0;

        // Compensate in reverse order
        for (int i = fromStepIndex; i >= 0; i--)
        {
            var step = definition.Steps[i];
            if (step.Compensate == null || state.StepStates[i].Status != SagaStepStatus.Completed)
                continue;

            try
            {
                _logger.LogDebug("Compensating step '{StepName}' for saga {SagaId}", step.Name, sagaId);
                await step.Compensate(state.Context, cancellationToken);
                state.StepStates[i].Status = SagaStepStatus.Compensated;
                compensated++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to compensate step '{StepName}' for saga {SagaId}",
                    step.Name, sagaId);
                // Continue compensating other steps even if one fails
            }
        }

        return compensated;
    }
}
