/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Resilience Service - Circuit breaker, retry, and fallback patterns
 * Implements resilience patterns for external service calls (webhooks, LLM, integrations)
 */

using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Configuration for resilience patterns
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Default timeout for external calls in seconds
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of exceptions before circuit opens
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;

    /// <summary>
    /// Duration the circuit stays open
    /// </summary>
    public int CircuitBreakerDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay for exponential backoff in milliseconds
    /// </summary>
    public int RetryBaseDelayMs { get; set; } = 500;

    /// <summary>
    /// Maximum delay between retries in milliseconds
    /// </summary>
    public int RetryMaxDelayMs { get; set; } = 30000;

    /// <summary>
    /// Enable/disable circuit breaker
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;

    /// <summary>
    /// Enable/disable retry
    /// </summary>
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Service-specific configurations
    /// </summary>
    public Dictionary<string, ServiceResilienceConfig> Services { get; set; } = new();
}

public class ServiceResilienceConfig
{
    public int TimeoutSeconds { get; set; } = 30;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
}

/// <summary>
/// Circuit breaker state for monitoring
/// </summary>
public class CircuitBreakerState
{
    public string ServiceName { get; set; } = "";
    public CircuitState State { get; set; }
    public DateTime? LastStateChange { get; set; }
    public int FailureCount { get; set; }
    public int SuccessCount { get; set; }
    public DateTime? LastFailure { get; set; }
    public string? LastError { get; set; }
}

/// <summary>
/// Interface for resilience service
/// </summary>
public interface IResilienceService
{
    /// <summary>
    /// Execute an action with retry and circuit breaker
    /// </summary>
    Task<T> ExecuteAsync<T>(string serviceName, Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an action with retry, circuit breaker, and fallback
    /// </summary>
    Task<T> ExecuteWithFallbackAsync<T>(string serviceName, Func<CancellationToken, Task<T>> action, Func<Exception, T> fallback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current circuit breaker states
    /// </summary>
    IEnumerable<CircuitBreakerState> GetCircuitBreakerStates();

    /// <summary>
    /// Manually reset a circuit breaker
    /// </summary>
    void ResetCircuitBreaker(string serviceName);

    /// <summary>
    /// Check if a service's circuit is open
    /// </summary>
    bool IsCircuitOpen(string serviceName);
}

/// <summary>
/// Resilience service implementing circuit breaker, retry, and timeout patterns
/// </summary>
public class ResilienceService : IResilienceService
{
    private readonly ILogger<ResilienceService> _logger;
    private readonly ResilienceOptions _options;
    private readonly ConcurrentDictionary<string, PolicyHolder> _policies = new();
    private readonly ConcurrentDictionary<string, CircuitBreakerState> _states = new();

    private class PolicyHolder
    {
        public required AsyncCircuitBreakerPolicy CircuitBreaker { get; init; }
        public required AsyncPolicyWrap CombinedPolicy { get; init; }
    }

    public ResilienceService(
        ILogger<ResilienceService> logger,
        IOptions<ResilienceOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<T> ExecuteAsync<T>(
        string serviceName,
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        var policy = GetOrCreatePolicy(serviceName);

        try
        {
            var result = await policy.CombinedPolicy.ExecuteAsync(
                async ct => await action(ct),
                cancellationToken);

            // Record success
            UpdateState(serviceName, success: true);
            return result;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning("Circuit breaker open for service {ServiceName}", serviceName);
            UpdateState(serviceName, success: false, error: "Circuit breaker is open");
            throw new ServiceUnavailableException($"Service {serviceName} is temporarily unavailable (circuit open)", ex);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogWarning("Timeout for service {ServiceName}", serviceName);
            UpdateState(serviceName, success: false, error: "Request timed out");
            throw new ServiceUnavailableException($"Service {serviceName} timed out", ex);
        }
        catch (Exception ex)
        {
            UpdateState(serviceName, success: false, error: ex.Message);
            throw;
        }
    }

    public async Task<T> ExecuteWithFallbackAsync<T>(
        string serviceName,
        Func<CancellationToken, Task<T>> action,
        Func<Exception, T> fallback,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ExecuteAsync(serviceName, action, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Action failed for service {ServiceName}, executing fallback", serviceName);
            return fallback(ex);
        }
    }

    public IEnumerable<CircuitBreakerState> GetCircuitBreakerStates()
    {
        foreach (var kvp in _policies)
        {
            var state = _states.GetOrAdd(kvp.Key, _ => new CircuitBreakerState { ServiceName = kvp.Key });
            state.State = kvp.Value.CircuitBreaker.CircuitState;
            yield return state;
        }
    }

    public void ResetCircuitBreaker(string serviceName)
    {
        if (_policies.TryGetValue(serviceName, out var policy))
        {
            policy.CircuitBreaker.Reset();
            _logger.LogInformation("Circuit breaker reset for service {ServiceName}", serviceName);

            if (_states.TryGetValue(serviceName, out var state))
            {
                state.State = CircuitState.Closed;
                state.LastStateChange = DateTime.UtcNow;
                state.FailureCount = 0;
            }
        }
    }

    public bool IsCircuitOpen(string serviceName)
    {
        if (_policies.TryGetValue(serviceName, out var policy))
        {
            return policy.CircuitBreaker.CircuitState == CircuitState.Open ||
                   policy.CircuitBreaker.CircuitState == CircuitState.Isolated;
        }
        return false;
    }

    private PolicyHolder GetOrCreatePolicy(string serviceName)
    {
        return _policies.GetOrAdd(serviceName, name =>
        {
            var config = GetServiceConfig(name);

            // Timeout policy
            var timeoutPolicy = Policy.TimeoutAsync(
                TimeSpan.FromSeconds(config.TimeoutSeconds),
                TimeoutStrategy.Optimistic,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    _logger.LogWarning("Timeout triggered for {ServiceName} after {Timeout}s", name, timespan.TotalSeconds);
                    return Task.CompletedTask;
                });

            // Retry policy with exponential backoff
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    retryCount: _options.EnableRetry ? config.MaxRetryAttempts : 0,
                    sleepDurationProvider: attempt =>
                    {
                        var delay = Math.Min(
                            _options.RetryBaseDelayMs * Math.Pow(2, attempt - 1),
                            _options.RetryMaxDelayMs);
                        return TimeSpan.FromMilliseconds(delay);
                    },
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} for {ServiceName} after {Delay}ms due to: {Error}",
                            retryCount, name, timeSpan.TotalMilliseconds, exception.Message);
                    });

            // Circuit breaker policy
            var circuitBreaker = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .Or<TimeoutException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: _options.EnableCircuitBreaker ? config.CircuitBreakerThreshold : int.MaxValue,
                    durationOfBreak: TimeSpan.FromSeconds(config.CircuitBreakerDurationSeconds),
                    onBreak: (exception, duration) =>
                    {
                        _logger.LogWarning(
                            "Circuit breaker opened for {ServiceName} for {Duration}s due to: {Error}",
                            name, duration.TotalSeconds, exception.Message);

                        if (_states.TryGetValue(name, out var state))
                        {
                            state.State = CircuitState.Open;
                            state.LastStateChange = DateTime.UtcNow;
                        }
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset for {ServiceName}", name);

                        if (_states.TryGetValue(name, out var state))
                        {
                            state.State = CircuitState.Closed;
                            state.LastStateChange = DateTime.UtcNow;
                        }
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Circuit breaker half-open for {ServiceName}", name);

                        if (_states.TryGetValue(name, out var state))
                        {
                            state.State = CircuitState.HalfOpen;
                            state.LastStateChange = DateTime.UtcNow;
                        }
                    });

            // Combine policies: Timeout -> Retry -> CircuitBreaker
            var combinedPolicy = Policy.WrapAsync(timeoutPolicy, retryPolicy, circuitBreaker);

            // Initialize state
            _states[name] = new CircuitBreakerState
            {
                ServiceName = name,
                State = CircuitState.Closed,
                LastStateChange = DateTime.UtcNow
            };

            return new PolicyHolder
            {
                CircuitBreaker = circuitBreaker,
                CombinedPolicy = combinedPolicy
            };
        });
    }

    private ServiceResilienceConfig GetServiceConfig(string serviceName)
    {
        if (_options.Services.TryGetValue(serviceName, out var config))
        {
            return config;
        }

        // Return default config
        return new ServiceResilienceConfig
        {
            TimeoutSeconds = _options.DefaultTimeoutSeconds,
            CircuitBreakerThreshold = _options.CircuitBreakerThreshold,
            CircuitBreakerDurationSeconds = _options.CircuitBreakerDurationSeconds,
            MaxRetryAttempts = _options.MaxRetryAttempts
        };
    }

    private void UpdateState(string serviceName, bool success, string? error = null)
    {
        var state = _states.GetOrAdd(serviceName, _ => new CircuitBreakerState { ServiceName = serviceName });

        if (success)
        {
            state.SuccessCount++;
        }
        else
        {
            state.FailureCount++;
            state.LastFailure = DateTime.UtcNow;
            state.LastError = error;
        }
    }
}

/// <summary>
/// Exception thrown when a service is unavailable
/// </summary>
public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(string message) : base(message) { }
    public ServiceUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
