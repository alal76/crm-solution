// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Core.Scripting;

namespace CRM.Infrastructure.Scripting.TypeScript;

/// <summary>
/// Configuration options for the <see cref="TypeScriptScriptEngine"/>.
/// </summary>
public class TypeScriptScriptEngineOptions
{
    /// <summary>Base URL of the crm-script-runner sidecar (e.g. http://crm-script-runner:4000).</summary>
    public string BaseUrl { get; set; } = "http://localhost:4000"; // NOSONAR - S5332: localhost development URL for TypeScript script runner, HTTPS not applicable

    /// <summary>Total HTTP timeout for sidecar calls, including script execution time.</summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(60);
}

/// <summary>
/// C# wrapper that delegates TypeScript script execution to the crm-script-runner Node.js sidecar.
/// Communication: HTTP JSON to crm-script-runner on port 4000 (or Unix socket in production).
/// Implements <see cref="ICompiledScriptEngine"/> for <see cref="ScriptRuntime.TypeScript"/>.
/// </summary>
/// <remarks>
/// SARCH-035: TypeScriptScriptEngine HTTP wrapper.
/// The sidecar (SARCH-030→034) runs TypeScript in an isolated-vm V8 Isolate,
/// enforcing memory limits, timeouts, and AST security scanning before execution.
/// </remarks>
public class TypeScriptScriptEngine : ICompiledScriptEngine
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TypeScriptScriptEngine> _logger;
    private readonly string _baseUrl;

    /// <inheritdoc/>
    public ScriptRuntime Runtime => ScriptRuntime.TypeScript;

    public TypeScriptScriptEngine(
        IHttpClientFactory httpClientFactory,
        IOptions<TypeScriptScriptEngineOptions> options,
        ILogger<TypeScriptScriptEngine> logger)
    {
        _httpClient = httpClientFactory.CreateClient("crm-script-runner");
        _logger = logger;
        _baseUrl = options.Value.BaseUrl.TrimEnd('/');
    }

    /// <inheritdoc/>
    /// <remarks>
    /// For TypeScript, full compilation happens in the Node.js sidecar at execution time.
    /// This method performs a lightweight source hash and returns a compilation sentinel.
    /// </remarks>
    public Task<CompilationResult> CompileAsync(
        ScriptDefinition definition,
        CompilationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var contentHash = ComputeHash(definition.Source);
        _logger.LogDebug("TypeScript script {Id} compilation deferred to sidecar (hash: {Hash})", definition.Id, contentHash[..8]);

        var result = new CompilationResult
        {
            CompiledRef = new CompiledScriptRef(definition.Id, contentHash, null, DateTime.UtcNow),
            ContentHash = contentHash,
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<ExecutionResult<TOut>> ExecuteAsync<TIn, TOut>(
        CompiledScriptRef compiledRef,
        IScriptContext<TIn> context,
        ExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow;
        try
        {
            var requestBody = new
            {
                scriptId = compiledRef.ScriptId,
                source = string.Empty, // Script source is already referenced by compiledRef
                input = context.Input,
                timeoutMs = 30_000,
                memoryLimitMb = 64,
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(requestBody, jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Delegating TypeScript execution for script {Id} to sidecar at {Url}", compiledRef.ScriptId, _baseUrl);

            var response = await _httpClient.PostAsync($"{_baseUrl}/execute", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var sidecarResult = JsonSerializer.Deserialize<SidecarExecutionResponse<TOut>>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new ExecutionResult<TOut>
            {
                Success = sidecarResult?.Success ?? false,
                Output = sidecarResult != null ? sidecarResult.Output : default,
                Error = sidecarResult?.Error,
                TraceId = sidecarResult?.TraceId,
                Duration = DateTime.UtcNow - start,
                ResourceUsage = sidecarResult?.MemoryPeakBytes is > 0
                    ? new ResourceUsage(0, sidecarResult.MemoryPeakBytes.Value)
                    : null,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TypeScript engine execution failed for script {Id}", compiledRef.ScriptId);
            return new ExecutionResult<TOut>
            {
                Success = false,
                Error = ex.Message,
                Duration = DateTime.UtcNow - start,
            };
        }
    }

    /// <inheritdoc/>
    // PRA-018: Not implemented — post-GA roadmap item. See SPEC-SCRIPT-001.md
    // RunAsync is a convenience shortcut that compiles + executes in one call.
    // The TypeScript engine routes execution through the crm-script-runner Node.js
    // sidecar via HTTP. A unified RunAsync requires additional sidecar API support
    // for compile-cache keying. Scheduled for post-GA. Use CompileAsync + ExecuteAsync.
    public Task<ExecutionResult<TOut>> RunAsync<TIn, TOut>(
        string scriptId,
        TIn input,
        ExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Use CompileAsync + ExecuteAsync for the TypeScript engine.");

    private static string ComputeHash(string source)
    {
        var bytes = Encoding.UTF8.GetBytes(source);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>JSON response shape returned by the crm-script-runner sidecar /execute endpoint.</summary>
    private sealed class SidecarExecutionResponse<TOut>
    {
        public bool Success { get; init; }
        public TOut? Output { get; init; }
        public string? Error { get; init; }
        public string? TraceId { get; init; }
        public long DurationMs { get; init; }
        public long? MemoryPeakBytes { get; init; }
    }
}
