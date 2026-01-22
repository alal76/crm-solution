using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.StepExecutors;

/// <summary>
/// Executor for API Call steps - makes HTTP requests with retry and circuit breaker
/// </summary>
public class ApiCallStepExecutor : IStepExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly ILogger<ApiCallStepExecutor> _logger;

    // Simple in-memory circuit breaker state (in production, use Polly or distributed state)
    private static readonly Dictionary<string, CircuitBreakerState> _circuitBreakers = new();
    private static readonly object _lockObj = new();

    public ApiCallStepExecutor(
        IHttpClientFactory httpClientFactory,
        IWorkflowExpressionEvaluator expressionEvaluator,
        ILogger<ApiCallStepExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _expressionEvaluator = expressionEvaluator;
        _logger = logger;
    }

    public IEnumerable<string> SupportedStepTypes => new[] { WorkflowStepTypes.ApiCall };

    public async Task<StepExecutionResult> ExecuteAsync(
        StepExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing ApiCall step {StepKey} for instance {InstanceId}",
            context.Step.StepKey, context.Instance.Id);

        try
        {
            // Parse configuration
            var config = !string.IsNullOrEmpty(context.Step.Configuration)
                ? JsonSerializer.Deserialize<ApiCallStepConfig>(context.Step.Configuration)
                : null;

            if (config == null || string.IsNullOrEmpty(config.ApiEndpoint))
            {
                return new StepExecutionResult
                {
                    Success = false,
                    ErrorMessage = "Invalid API call configuration - endpoint is required"
                };
            }

            // Check circuit breaker
            var circuitKey = GetCircuitBreakerKey(config.ApiEndpoint);
            if (IsCircuitOpen(circuitKey))
            {
                _logger.LogWarning("Circuit breaker is open for {Endpoint}", config.ApiEndpoint);
                return new StepExecutionResult
                {
                    Success = false,
                    ErrorMessage = "Circuit breaker is open - API endpoint is unavailable",
                    ShouldRetry = true,
                    RetryAfter = DateTime.UtcNow.AddMinutes(1)
                };
            }

            // Replace variables in endpoint and body
            var endpoint = await _expressionEvaluator.ReplaceVariablesAsync(
                config.ApiEndpoint, context.Variables, cancellationToken);
            
            var body = !string.IsNullOrEmpty(config.BodyTemplate)
                ? await _expressionEvaluator.ReplaceVariablesAsync(config.BodyTemplate, context.Variables, cancellationToken)
                : null;

            // Execute with retry logic
            var retryPolicy = config.RetryPolicy ?? new RetryPolicyConfig { MaxAttempts = 3 };
            var (success, response, error) = await ExecuteWithRetryAsync(
                config, endpoint, body, retryPolicy, circuitKey, cancellationToken);

            if (success)
            {
                RecordSuccess(circuitKey);

                // Parse response and determine next step
                var responseData = await ParseResponseAsync(response, cancellationToken);
                var nextStepKey = await DetermineNextStepAsync(context, responseData, cancellationToken);

                return new StepExecutionResult
                {
                    Success = true,
                    NextStepKey = nextStepKey,
                    OutputVariables = new Dictionary<string, object?>
                    {
                        [$"{context.Step.StepKey}_response"] = responseData,
                        [$"{context.Step.StepKey}_statusCode"] = response?.StatusCode.ToString()
                    }
                };
            }
            else
            {
                RecordFailure(circuitKey);

                return new StepExecutionResult
                {
                    Success = false,
                    ErrorMessage = error,
                    ShouldRetry = true,
                    RetryAfter = DateTime.UtcNow.AddSeconds(
                        retryPolicy.BackoffSeconds?.LastOrDefault() ?? 60)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ApiCall step {StepKey}", context.Step.StepKey);
            return new StepExecutionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorDetails = ex.StackTrace,
                ShouldRetry = true,
                RetryAfter = DateTime.UtcNow.AddMinutes(1)
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
            result.IsValid = false;
            result.Errors.Add($"Step '{step.Name}' requires API call configuration");
            return Task.FromResult(result);
        }

        try
        {
            var config = JsonSerializer.Deserialize<ApiCallStepConfig>(step.Configuration);
            if (config == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Step '{step.Name}' has invalid configuration JSON");
            }
            else
            {
                if (string.IsNullOrEmpty(config.ApiEndpoint))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Step '{step.Name}' must specify an API endpoint");
                }

                if (!Uri.TryCreate(config.ApiEndpoint?.Replace("{{", "x").Replace("}}", "x"), UriKind.Absolute, out _))
                {
                    result.Warnings.Add($"Step '{step.Name}' endpoint may not be a valid URL");
                }

                var validMethods = new[] { "GET", "POST", "PUT", "PATCH", "DELETE" };
                if (!validMethods.Contains(config.Method?.ToUpper() ?? "GET"))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Step '{step.Name}' has invalid HTTP method");
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

    private async Task<(bool success, HttpResponseMessage? response, string? error)> ExecuteWithRetryAsync(
        ApiCallStepConfig config,
        string endpoint,
        string? body,
        RetryPolicyConfig retryPolicy,
        string circuitKey,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("WorkflowApiClient");
        client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds > 0 ? config.TimeoutSeconds : 30);

        int attempt = 0;
        var backoffSeconds = retryPolicy.BackoffSeconds ?? new List<int> { 10, 30, 60 };

        while (attempt < retryPolicy.MaxAttempts)
        {
            attempt++;

            try
            {
                var request = new HttpRequestMessage(
                    new HttpMethod(config.Method ?? "GET"), 
                    endpoint);

                // Add headers
                if (config.Headers != null)
                {
                    foreach (var header in config.Headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Add body
                if (!string.IsNullOrEmpty(body))
                {
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }

                // Add authentication
                ApplyAuthentication(request, config);

                _logger.LogDebug("API call attempt {Attempt}/{Max} to {Endpoint}",
                    attempt, retryPolicy.MaxAttempts, endpoint);

                var response = await client.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return (true, response, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("API call failed with status {StatusCode}: {Error}",
                    response.StatusCode, errorContent);

                // Don't retry on client errors (4xx)
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return (false, response, $"API returned {response.StatusCode}: {errorContent}");
                }
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("API call timeout on attempt {Attempt}", attempt);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "API call failed on attempt {Attempt}", attempt);
            }

            // Wait before retry
            if (attempt < retryPolicy.MaxAttempts)
            {
                var delay = backoffSeconds.ElementAtOrDefault(attempt - 1) * 1000;
                if (retryPolicy.ExponentialBackoff)
                {
                    delay = (int)(delay * Math.Pow(2, attempt - 1));
                }
                await Task.Delay(Math.Min(delay, 300000), cancellationToken); // Max 5 minutes
            }
        }

        return (false, null, $"API call failed after {retryPolicy.MaxAttempts} attempts");
    }

    private void ApplyAuthentication(HttpRequestMessage request, ApiCallStepConfig config)
    {
        switch (config.AuthenticationType?.ToLower())
        {
            case "bearer":
                if (!string.IsNullOrEmpty(config.AuthenticationConfig))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.AuthenticationConfig);
                }
                break;
            case "basic":
                if (!string.IsNullOrEmpty(config.AuthenticationConfig))
                {
                    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(config.AuthenticationConfig));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
                }
                break;
            case "apikey":
                if (!string.IsNullOrEmpty(config.AuthenticationConfig))
                {
                    var parts = config.AuthenticationConfig.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        request.Headers.TryAddWithoutValidation(parts[0], parts[1]);
                    }
                }
                break;
        }
    }

    private async Task<object?> ParseResponseAsync(
        HttpResponseMessage? response, 
        CancellationToken cancellationToken)
    {
        if (response == null) return null;

        try
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrEmpty(content)) return null;

            return JsonSerializer.Deserialize<object>(content);
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> DetermineNextStepAsync(
        StepExecutionContext context,
        object? responseData,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(context.Step.Transitions))
        {
            return null;
        }

        try
        {
            var transitions = JsonSerializer.Deserialize<List<StepTransition>>(context.Step.Transitions);
            if (transitions == null || !transitions.Any())
            {
                return null;
            }

            // Add response to variables for condition evaluation
            var variables = new Dictionary<string, object?>(context.Variables)
            {
                ["response"] = responseData
            };

            foreach (var transition in transitions.OrderByDescending(t => t.Priority))
            {
                if (string.IsNullOrEmpty(transition.Condition))
                {
                    return transition.NextStepKey; // Default transition
                }

                var matches = await _expressionEvaluator.EvaluateConditionAsync(
                    transition.Condition, variables, cancellationToken);

                if (matches)
                {
                    return transition.NextStepKey;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    #region Circuit Breaker

    private static string GetCircuitBreakerKey(string endpoint)
    {
        try
        {
            var uri = new Uri(endpoint.Replace("{{", "x").Replace("}}", "x"));
            return $"{uri.Scheme}://{uri.Host}";
        }
        catch
        {
            return endpoint;
        }
    }

    private static bool IsCircuitOpen(string key)
    {
        lock (_lockObj)
        {
            if (!_circuitBreakers.TryGetValue(key, out var state))
            {
                return false;
            }

            if (state.State == "Open" && DateTime.UtcNow > state.ResetAt)
            {
                state.State = "HalfOpen";
            }

            return state.State == "Open";
        }
    }

    private static void RecordSuccess(string key)
    {
        lock (_lockObj)
        {
            if (_circuitBreakers.TryGetValue(key, out var state))
            {
                state.FailureCount = 0;
                state.State = "Closed";
            }
        }
    }

    private static void RecordFailure(string key)
    {
        lock (_lockObj)
        {
            if (!_circuitBreakers.ContainsKey(key))
            {
                _circuitBreakers[key] = new CircuitBreakerState();
            }

            var state = _circuitBreakers[key];
            state.FailureCount++;

            if (state.FailureCount >= 5) // Trip after 5 failures
            {
                state.State = "Open";
                state.ResetAt = DateTime.UtcNow.AddMinutes(1);
            }
        }
    }

    private class CircuitBreakerState
    {
        public string State { get; set; } = "Closed";
        public int FailureCount { get; set; }
        public DateTime ResetAt { get; set; }
    }

    #endregion
}
