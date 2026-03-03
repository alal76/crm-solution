// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Scripting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Scripting.Testing;

/// <summary>
/// Chaos middleware that wraps ICompiledScriptEngine to inject random failures, delays,
/// and resource pressure for resilience testing.
/// ONLY used in test/development environments — never production.
/// </summary>
public class ScriptChaosMiddleware : ICompiledScriptEngine
{
    private readonly ICompiledScriptEngine _inner;
    private readonly ILogger<ScriptChaosMiddleware> _logger;
    private readonly ChaosOptions _options;

    public ScriptRuntime Runtime => _inner.Runtime;

    public ScriptChaosMiddleware(
        ICompiledScriptEngine inner,
        ChaosOptions options,
        ILogger<ScriptChaosMiddleware> logger)
    {
        _inner = inner;
        _options = options;
        _logger = logger;
    }

    public async Task<CompilationResult> CompileAsync(
        ScriptDefinition definition,
        CompilationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await InjectChaosDelay(cancellationToken);
        if (ShouldFail())
        {
            throw new InvalidOperationException("[CHAOS] Random compilation failure injected");
        }
        return await _inner.CompileAsync(definition, options, cancellationToken);
    }

    public async Task<ExecutionResult<TOut>> ExecuteAsync<TIn, TOut>(
        CompiledScriptRef compiledRef,
        IScriptContext<TIn> context,
        ExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await InjectChaosDelay(cancellationToken);
        if (ShouldFail())
        {
            return new ExecutionResult<TOut> { Success = false, Error = "[CHAOS] Random execution failure injected" };
        }
        return await _inner.ExecuteAsync<TIn, TOut>(compiledRef, context, options, cancellationToken);
    }

    public Task<ExecutionResult<TOut>> RunAsync<TIn, TOut>(
        string scriptId,
        TIn input,
        ExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
        => _inner.RunAsync<TIn, TOut>(scriptId, input, options, cancellationToken);

    private async Task InjectChaosDelay(CancellationToken cancellationToken)
    {
        if (_options.LatencyInjectionRate > 0 && Random.Shared.NextDouble() < _options.LatencyInjectionRate) // NOSONAR - S2245: non-security RNG for chaos latency injection simulation
        {
            var delayMs = Random.Shared.Next(_options.MinLatencyMs, _options.MaxLatencyMs); // NOSONAR - S2245: non-security RNG for chaos delay range
            _logger.LogDebug("[CHAOS] Injecting {Delay}ms latency", delayMs);
            await Task.Delay(delayMs, cancellationToken);
        }
    }

    private bool ShouldFail()
    {
        var shouldFail = _options.FailureRate > 0 && Random.Shared.NextDouble() < _options.FailureRate; // NOSONAR - S2245: non-security RNG for chaos failure rate simulation
        if (shouldFail) _logger.LogWarning("[CHAOS] Injecting random failure");
        return shouldFail;
    }
}

public class ChaosOptions
{
    public double FailureRate { get; set; } = 0.0;          // 0.0 = off, 0.1 = 10% failure rate
    public double LatencyInjectionRate { get; set; } = 0.0; // 0.0 = off
    public int MinLatencyMs { get; set; } = 100;
    public int MaxLatencyMs { get; set; } = 2000;
}
