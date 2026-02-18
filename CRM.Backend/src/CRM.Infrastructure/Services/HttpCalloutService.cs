// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Executes HTTP callouts from workflow action nodes.
/// </summary>
public class HttpCalloutService : IHttpCalloutService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpCalloutService> _logger;
    private static readonly HashSet<string> AllowedMethods = new(StringComparer.OrdinalIgnoreCase)
        { "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS" };

    public HttpCalloutService(IHttpClientFactory httpClientFactory, ILogger<HttpCalloutService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<HttpCalloutResult> ExecuteAsync(HttpCalloutConfig config, CancellationToken cancellationToken = default)
    {
        var validation = Validate(config);
        if (!validation.IsValid)
        {
            return new HttpCalloutResult
            {
                Success = false,
                ErrorMessage = string.Join("; ", validation.Errors)
            };
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var maxAttempts = Math.Max(1, config.RetryCount + 1);
        HttpCalloutResult? lastResult = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                lastResult = await ExecuteSingleAsync(config, attempt, cancellationToken);
                lastResult.Attempts = attempt;
                lastResult.ElapsedMs = sw.ElapsedMilliseconds;

                if (lastResult.Success)
                {
                    _logger.LogInformation(
                        "HTTP callout {Name} succeeded: {Method} {Url} → {Status} in {Elapsed}ms (attempt {Attempt})",
                        config.Name ?? "unnamed", config.Method, config.Url,
                        lastResult.StatusCode, lastResult.ElapsedMs, attempt);
                    return lastResult;
                }

                // Non-retryable status codes
                if (lastResult.StatusCode >= 400 && lastResult.StatusCode < 500 && lastResult.StatusCode != 429)
                {
                    _logger.LogWarning(
                        "HTTP callout {Name} failed with client error {Status}, not retrying",
                        config.Name ?? "unnamed", lastResult.StatusCode);
                    return lastResult;
                }
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout
                lastResult = new HttpCalloutResult
                {
                    Success = false,
                    ErrorMessage = $"Request timed out after {config.TimeoutSeconds}s",
                    Attempts = attempt,
                    ElapsedMs = sw.ElapsedMilliseconds
                };
            }
            catch (HttpRequestException ex)
            {
                lastResult = new HttpCalloutResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Attempts = attempt,
                    ElapsedMs = sw.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during HTTP callout {Name}", config.Name ?? "unnamed");
                lastResult = new HttpCalloutResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Attempts = attempt,
                    ElapsedMs = sw.ElapsedMilliseconds
                };
                return lastResult; // Don't retry unknown errors
            }

            if (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    "HTTP callout {Name} attempt {Attempt} failed, retrying in {Delay}s...",
                    config.Name ?? "unnamed", attempt, config.RetryDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(config.RetryDelaySeconds), cancellationToken);
            }
        }

        sw.Stop();
        if (lastResult != null)
            lastResult.ElapsedMs = sw.ElapsedMilliseconds;

        return lastResult ?? new HttpCalloutResult
        {
            Success = false,
            ErrorMessage = "No attempts were made"
        };
    }

    /// <inheritdoc />
    public HttpCalloutValidation Validate(HttpCalloutConfig config)
    {
        var result = new HttpCalloutValidation { IsValid = true };

        if (string.IsNullOrWhiteSpace(config.Url))
        {
            result.Errors.Add("URL is required");
            result.IsValid = false;
        }
        else if (!Uri.TryCreate(config.Url, UriKind.Absolute, out var uri) ||
                 (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            result.Errors.Add("URL must be a valid absolute HTTP/HTTPS URL");
            result.IsValid = false;
        }

        if (!AllowedMethods.Contains(config.Method))
        {
            result.Errors.Add($"Method must be one of: {string.Join(", ", AllowedMethods)}");
            result.IsValid = false;
        }

        if (config.TimeoutSeconds < 1 || config.TimeoutSeconds > 300)
        {
            result.Errors.Add("TimeoutSeconds must be between 1 and 300");
            result.IsValid = false;
        }

        if (config.RetryCount < 0 || config.RetryCount > 5)
        {
            result.Errors.Add("RetryCount must be between 0 and 5");
            result.IsValid = false;
        }

        return result;
    }

    private async Task<HttpCalloutResult> ExecuteSingleAsync(
        HttpCalloutConfig config, int attempt, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient("WorkflowCallout");
        client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

        var method = new HttpMethod(config.Method.ToUpperInvariant());
        using var request = new HttpRequestMessage(method, config.Url);

        // Set headers
        foreach (var (key, value) in config.Headers)
        {
            if (key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                continue; // Content headers set on content object
            request.Headers.TryAddWithoutValidation(key, value);
        }

        // Set body for methods that support it
        if (config.Body != null && (method == HttpMethod.Post || method == HttpMethod.Put ||
            method.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase)))
        {
            var json = config.Body is string s ? s : JsonSerializer.Serialize(config.Body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // Apply content headers
            foreach (var (key, value) in config.Headers)
            {
                if (key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase) &&
                    !key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    request.Content.Headers.TryAddWithoutValidation(key, value);
                }
            }
        }

        var response = await client.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var responseHeaders = new Dictionary<string, string>();
        foreach (var header in response.Headers)
            responseHeaders[header.Key] = string.Join(", ", header.Value);

        var isSuccess = config.AcceptAny2xx
            ? (int)response.StatusCode >= 200 && (int)response.StatusCode < 300
            : (int)response.StatusCode >= 200 && (int)response.StatusCode <= 204;

        return new HttpCalloutResult
        {
            Success = isSuccess,
            StatusCode = (int)response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            ResponseBody = responseBody,
            ResponseHeaders = responseHeaders,
            ErrorMessage = isSuccess ? null : $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"
        };
    }
}
