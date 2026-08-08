// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Scripting.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Scripting;

/// <summary>
/// Configuration options for <see cref="PythonScriptEngine"/>.
/// </summary>
public class PythonScriptEngineOptions
{
    /// <summary>Base URL of the crm-python-script-runner sidecar (e.g. http://crm-python-script-runner:4001).</summary>
    public string BaseUrl { get; set; } = "http://localhost:4001"; // NOSONAR - S5332: localhost development URL for Python script runner sidecar, HTTPS not applicable

    /// <summary>Total HTTP timeout for the /execute call, including script execution time in the sidecar.</summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(45);

    /// <summary>Timeout used for the lightweight GET /health check that backs <see cref="PythonScriptEngine.IsAvailable"/>.</summary>
    public TimeSpan HealthCheckTimeout { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>Default script execution timeout when the caller does not supply one.</summary>
    public TimeSpan DefaultScriptTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Default per-execution memory ceiling (MB) enforced inside the sidecar's sandbox subprocess.</summary>
    public int DefaultMemoryLimitMb { get; set; } = 64;
}

/// <summary>
/// Python script engine implementation (REV-STUB-008).
/// <para>
/// Delegates execution to the <c>crm-python-script-runner</c> sidecar — a small,
/// standard-library-only Python HTTP server (see <c>python-script-runner/</c> at the
/// repository root) that statically rejects dangerous code via an AST-based denylist
/// (<c>denylist.py</c>) and then runs the remaining "pure computation" script inside a
/// short-lived, resource-limited (<c>resource.setrlimit</c>) OS subprocess
/// (<c>sandbox_runner.py</c>) with a parent-enforced wall-clock timeout.
/// </para>
/// <para>
/// This class mirrors the structure of <see cref="CRM.Infrastructure.Scripting.TypeScript.TypeScriptScriptEngine"/>,
/// which delegates TypeScript execution to the analogous <c>crm-script-runner</c> Node.js
/// sidecar. Unlike that class, this one implements <see cref="IScriptEngine"/> (the
/// simple code-string execution contract used by <see cref="CRM.Infrastructure.Factories.ScriptEngineFactory"/>,
/// <c>ScriptingController</c>, and <c>ScriptPluginService</c>) rather than
/// <see cref="ICompiledScriptEngine"/>.
/// </para>
/// <para>
/// Defense in depth: before ever contacting the sidecar, <see cref="ExecuteAsync"/> runs
/// the code through <see cref="ScriptSecurityPolicy.IsDataExfiltrationRisk"/> (T3) and
/// derives resource limits via <see cref="ScriptSecurityPolicy.GetResourceLimits"/> (T6).
/// A <see cref="SemaphoreSlim"/> ceiling of 10 concurrent executions matches
/// <c>RoslynScriptEngine</c>'s concurrency limiter for consistency across engines.
/// </para>
/// </summary>
public class PythonScriptEngine : IScriptEngine, IDisposable
{
    private const int MaxConcurrentExecutions = 10; // matches RoslynScriptEngine SemaphoreSlim and ScriptSecurityPolicy.GetResourceLimits

    private readonly HttpClient _httpClient;
    private readonly ILogger<PythonScriptEngine> _logger;
    private readonly PythonScriptEngineOptions _options;
    private readonly string _baseUrl;
    private readonly SemaphoreSlim _concurrencyLimiter = new(MaxConcurrentExecutions, MaxConcurrentExecutions);

    private static readonly JsonSerializerOptions RequestJsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly JsonSerializerOptions ResponseJsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PythonScriptEngine(
        IHttpClientFactory httpClientFactory,
        IOptions<PythonScriptEngineOptions> options,
        ILogger<PythonScriptEngine> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient("crm-python-script-runner");
        _baseUrl = _options.BaseUrl.TrimEnd('/');
    }

    /// <inheritdoc />
    public ScriptLanguage Language => ScriptLanguage.Python;

    /// <summary>
    /// Performs a synchronous health check (<c>GET /health</c>) against the
    /// crm-python-script-runner sidecar, bounded by <see cref="PythonScriptEngineOptions.HealthCheckTimeout"/>.
    /// Returns <c>false</c> — never throws — when the sidecar is unreachable, slow to respond,
    /// or returns a non-success status. <see cref="CRM.Infrastructure.Factories.ScriptEngineFactory"/>
    /// checks this before dispatching a script to this engine.
    /// </summary>
    /// <remarks>
    /// <see cref="IScriptEngine.IsAvailable"/> is a synchronous property by contract (mirroring
    /// <see cref="JintScriptEngine.IsAvailable"/>'s constant <c>true</c>), so this performs a
    /// bounded synchronous wait over the async HTTP call rather than exposing an async check.
    /// ASP.NET Core has no ambient <see cref="SynchronizationContext"/>, so this does not risk
    /// the classic sync-over-async deadlock; the bounded <see cref="CancellationTokenSource"/>
    /// guarantees the property never blocks longer than <see cref="PythonScriptEngineOptions.HealthCheckTimeout"/>.
    /// </remarks>
    public bool IsAvailable
    {
        get
        {
            try
            {
                using var cts = new CancellationTokenSource(_options.HealthCheckTimeout);
                using var response = _httpClient.GetAsync($"{_baseUrl}/health", cts.Token).GetAwaiter().GetResult();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
            {
                _logger.LogDebug(ex, "Python script sidecar health check failed at {BaseUrl}; treating engine as unavailable", _baseUrl);
                return false;
            }
        }
    }

    /// <inheritdoc />
    public async Task<ScriptExecutionResult> ExecuteAsync(
        string code,
        Dictionary<string, object?> variables,
        Dictionary<string, object?> context,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(variables);
        ArgumentNullException.ThrowIfNull(context);

        var stopwatch = Stopwatch.StartNew();
        var effectiveTimeout = timeout ?? _options.DefaultScriptTimeout;

        // ── Defense in depth, layer 1: C#-side pre-flight check BEFORE the sidecar is ever
        // contacted. The sidecar's AST denylist (python-script-runner/denylist.py) is the
        // primary, Python-aware gate; this is a coarse, language-agnostic backstop reusing
        // the same policy the Roslyn/C# engine uses. ─────────────────────────────────────
        var definition = new CRM.Core.Scripting.ScriptDefinition
        {
            Name = "python-inline-script",
            Kind = CRM.Core.Scripting.ScriptKind.Transform,
            Source = code,
            Timeout = effectiveTimeout,
            MemoryLimitMb = _options.DefaultMemoryLimitMb,
        };

        if (ScriptSecurityPolicy.IsDataExfiltrationRisk(definition))
        {
            stopwatch.Stop();
            _logger.LogWarning("PythonScriptEngine: rejected script on pre-flight data-exfiltration check (ScriptSecurityPolicy)");
            return new ScriptExecutionResult(
                Success: false,
                ReturnValue: null,
                Logs: Array.Empty<string>(),
                ErrorMessage: "Script rejected by pre-flight security policy: source references a networking/data-exfiltration primitive that is not permitted (e.g. socket, HttpClient, WebClient, TcpClient).",
                ExecutionTime: stopwatch.Elapsed);
        }

        var resourceLimits = ScriptSecurityPolicy.GetResourceLimits(_options.DefaultMemoryLimitMb, effectiveTimeout);

        // ── Layer 2: concurrency ceiling, matching RoslynScriptEngine's SemaphoreSlim(10). ──
        try
        {
            await _concurrencyLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Covers the already-cancelled-token case: WaitAsync throws before the try/finally
            // below is even entered, so this must be caught separately to keep the "always
            // return a ScriptExecutionResult, never throw for cancellation" contract that
            // JintScriptEngine also upholds.
            stopwatch.Stop();
            return new ScriptExecutionResult(false, null, Array.Empty<string>(), "Script execution was cancelled", stopwatch.Elapsed);
        }

        try
        {
            var requestBody = new
            {
                code,
                variables,
                context,
                timeoutMs = resourceLimits.TimeoutMs,
                memoryLimitMb = resourceLimits.MemoryLimitBytes / (1024 * 1024),
            };

            var json = JsonSerializer.Serialize(requestBody, RequestJsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(_options.HttpTimeout);

            HttpResponseMessage response;
            try
            {
                _logger.LogDebug("Delegating Python execution to sidecar at {BaseUrl}", _baseUrl);
                response = await _httpClient.PostAsync($"{_baseUrl}/execute", content, linkedCts.Token).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex, "Python script sidecar unavailable at {BaseUrl}", _baseUrl);
                return new ScriptExecutionResult(false, null, Array.Empty<string>(),
                    $"Python scripting sidecar is unavailable at {_baseUrl}. Ensure crm-python-script-runner is running.", stopwatch.Elapsed);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // linkedCts fired due to CancelAfter, not the caller's token — this is our timeout.
                stopwatch.Stop();
                _logger.LogWarning("Python script execution timed out after {Timeout}", _options.HttpTimeout);
                return new ScriptExecutionResult(false, null, Array.Empty<string>(), "Script execution timed out", stopwatch.Elapsed);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                return new ScriptExecutionResult(false, null, Array.Empty<string>(), "Script execution was cancelled", stopwatch.Elapsed);
            }

            using (response)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    stopwatch.Stop();
                    _logger.LogWarning("Python script sidecar returned {StatusCode}: {Body}", (int)response.StatusCode, responseBody);
                    var message = TryExtractError(responseBody) ?? $"Sidecar returned HTTP {(int)response.StatusCode}";
                    return new ScriptExecutionResult(false, null, Array.Empty<string>(), message, stopwatch.Elapsed);
                }

                SidecarExecuteResponse? sidecarResult;
                try
                {
                    sidecarResult = JsonSerializer.Deserialize<SidecarExecuteResponse>(responseBody, ResponseJsonOptions);
                }
                catch (JsonException ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Failed to parse Python sidecar response: {Body}", responseBody);
                    return new ScriptExecutionResult(false, null, Array.Empty<string>(), "Failed to parse sidecar response", stopwatch.Elapsed);
                }

                stopwatch.Stop();

                if (sidecarResult is null)
                {
                    return new ScriptExecutionResult(false, null, Array.Empty<string>(), "Sidecar returned an empty response", stopwatch.Elapsed);
                }

                var returnValue = sidecarResult.Result.HasValue ? ConvertJsonElement(sidecarResult.Result.Value) : null;
                var executionTime = sidecarResult.DurationMs > 0
                    ? TimeSpan.FromMilliseconds(sidecarResult.DurationMs)
                    : stopwatch.Elapsed;

                return new ScriptExecutionResult(
                    Success: sidecarResult.Success,
                    ReturnValue: returnValue,
                    Logs: (IReadOnlyList<string>?)sidecarResult.Logs ?? Array.Empty<string>(),
                    ErrorMessage: sidecarResult.Error,
                    ExecutionTime: executionTime);
            }
        }
        finally
        {
            _concurrencyLimiter.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScriptDiagnostic>> ValidateSyntaxAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);

        try
        {
            var requestBody = new { code };
            var json = JsonSerializer.Serialize(requestBody, RequestJsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(_options.HttpTimeout);

            using var response = await _httpClient.PostAsync($"{_baseUrl}/validate", content, linkedCts.Token).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var message = TryExtractError(responseBody) ?? $"Sidecar returned HTTP {(int)response.StatusCode}";
                return new[] { new ScriptDiagnostic(0, 0, message, DiagnosticSeverity.Error) };
            }

            var sidecarResult = JsonSerializer.Deserialize<SidecarValidateResponse>(responseBody, ResponseJsonOptions);
            if (sidecarResult?.Diagnostics is null || sidecarResult.Diagnostics.Count == 0)
            {
                return Array.Empty<ScriptDiagnostic>();
            }

            return sidecarResult.Diagnostics
                .Select(d => new ScriptDiagnostic(d.Line, d.Column, d.Message, ParseSeverity(d.Severity)))
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Python script sidecar unavailable at {BaseUrl} during syntax validation", _baseUrl);
            return new[]
            {
                new ScriptDiagnostic(0, 0,
                    $"Python scripting sidecar is unavailable at {_baseUrl}. Ensure crm-python-script-runner is running.",
                    DiagnosticSeverity.Error),
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new[] { new ScriptDiagnostic(0, 0, "Syntax validation timed out", DiagnosticSeverity.Error) };
        }
        catch (OperationCanceledException)
        {
            return new[] { new ScriptDiagnostic(0, 0, "Syntax validation was cancelled", DiagnosticSeverity.Error) };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Python sidecar /validate response");
            return new[] { new ScriptDiagnostic(0, 0, "Failed to parse sidecar response", DiagnosticSeverity.Error) };
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _concurrencyLimiter.Dispose();
        GC.SuppressFinalize(this);
    }

    private static DiagnosticSeverity ParseSeverity(string? severity) =>
        severity?.ToUpperInvariant() switch
        {
            "ERROR" => DiagnosticSeverity.Error,
            "WARNING" => DiagnosticSeverity.Warning,
            _ => DiagnosticSeverity.Info,
        };

    private static string? TryExtractError(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("error", out var errorProp) && errorProp.ValueKind == JsonValueKind.String)
            {
                return errorProp.GetString();
            }
        }
        catch (JsonException)
        {
            // Not JSON — fall through and return null so callers substitute a generic message.
        }

        return null;
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            // NOTE: the ternary's two branches (long / double) would otherwise unify to a
            // `double` result type (long implicitly widens to double), silently discarding
            // integer identity for every whole-number result. Boxing the long branch to
            // `object` up front forces the ternary's static type to `object` and preserves it.
            JsonValueKind.Number => element.TryGetInt64(out var l) ? (object)l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToArray(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
            _ => element.ToString(),
        };
    }

    /// <summary>JSON response shape returned by the crm-python-script-runner sidecar's <c>POST /execute</c>.</summary>
    private sealed class SidecarExecuteResponse
    {
        public bool Success { get; init; }
        public JsonElement? Result { get; init; }
        public List<string>? Logs { get; init; }
        public string? Error { get; init; }
        public long DurationMs { get; init; }
    }

    /// <summary>JSON response shape returned by the crm-python-script-runner sidecar's <c>POST /validate</c>.</summary>
    private sealed class SidecarValidateResponse
    {
        public bool Valid { get; init; }
        public List<SidecarDiagnostic>? Diagnostics { get; init; }
    }

    private sealed class SidecarDiagnostic
    {
        public int Line { get; init; }
        public int Column { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? Severity { get; init; }
    }
}
