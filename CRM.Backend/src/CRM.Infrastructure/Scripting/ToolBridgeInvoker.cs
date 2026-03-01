// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CRM.Core.Scripting;

namespace CRM.Infrastructure.Scripting;

/// <summary>
/// Implements <see cref="IToolInvoker"/> with:
/// <list type="bullet">
///   <item>Permission enforcement (per-tool, per-script permission check)</item>
///   <item>Audit logging of every tool invocation</item>
///   <item>Per-tool rate limiting (sliding window, 100 calls/minute)</item>
///   <item>Circuit breaker (fail-fast after 5 consecutive failures, 30s reset)</item>
///   <item>Separation of Duties (SoD) policy: blocked tool-pair combinations</item>
/// </list>
/// One instance is created per script execution; not thread-safe across concurrent executions.
/// </summary>
public class ToolBridgeInvoker : IToolInvoker
{
    private readonly ToolRegistry _registry;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ToolBridgeInvoker> _logger;
    private readonly IReadOnlyList<ScriptPermission> _scriptPermissions;

    // Per-tool rate limiting: sliding window of invocation timestamps (last 60 s)
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _rateLimitWindows = new();
    private const int ToolRateLimitPerMinute = 100;

    // Circuit breaker state per tool: (consecutive failures, time of last failure)
    private readonly ConcurrentDictionary<string, (int Failures, DateTime LastFailure)> _circuitState = new();
    private const int CircuitBreakerThreshold = 5;
    private static readonly TimeSpan CircuitBreakerTimeout = TimeSpan.FromSeconds(30);

    // Separation of Duties: tool pairs that cannot both be called in the same execution
    private static readonly HashSet<(string A, string B)> SodBlockedPairs =
    [
        ("EditCustomer", "DeleteCustomer"),
        ("ApproveInvoice", "CreateInvoice"),
    ];

    // Tools called so far in this execution session (for SoD enforcement)
    private readonly HashSet<string> _calledToolsThisSession = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Initialises a new <see cref="ToolBridgeInvoker"/> for a single script execution.</summary>
    public ToolBridgeInvoker(
        ToolRegistry registry,
        IServiceProvider serviceProvider,
        IReadOnlyList<ScriptPermission> scriptPermissions,
        ILogger<ToolBridgeInvoker> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _scriptPermissions = scriptPermissions ?? throw new ArgumentNullException(nameof(scriptPermissions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ToolResult<TResult>> CallAsync<TResult>(
        string toolName,
        object parameters,
        CancellationToken cancellationToken = default)
    {
        var callStart = DateTime.UtcNow;

        // 1. Registry lookup
        if (!_registry.TryGet(toolName, out var descriptor) || descriptor == null)
        {
            _logger.LogWarning("Tool '{Tool}' not found in registry", toolName);
            return FailResult<TResult>($"Tool '{toolName}' not found", callStart);
        }

        // 2. Permission check
        foreach (var required in descriptor.RequiredPermissions)
        {
            if (!HasPermission(required))
            {
                _logger.LogWarning("Script lacks permission '{Permission}' for tool '{Tool}'", required, toolName);
                return FailResult<TResult>($"Permission denied: {required}", callStart);
            }
        }

        // 3. Separation of Duties check
        foreach (var (a, b) in SodBlockedPairs)
        {
            bool isA = toolName.Equals(a, StringComparison.OrdinalIgnoreCase);
            bool isB = toolName.Equals(b, StringComparison.OrdinalIgnoreCase);
            if ((isA && _calledToolsThisSession.Contains(b)) ||
                (isB && _calledToolsThisSession.Contains(a)))
            {
                var blocker = isA ? b : a;
                _logger.LogWarning("SoD violation: '{Tool}' blocked because '{Blocker}' was already called", toolName, blocker);
                return FailResult<TResult>($"SoD violation: cannot call '{toolName}' after '{blocker}'", callStart);
            }
        }

        // 4. Rate limit check
        if (!CheckRateLimit(toolName))
        {
            _logger.LogWarning("Rate limit exceeded for tool '{Tool}'", toolName);
            return FailResult<TResult>($"Rate limit exceeded for tool '{toolName}'", callStart);
        }

        // 5. Circuit breaker check
        if (IsCircuitOpen(toolName))
        {
            _logger.LogWarning("Circuit breaker open for tool '{Tool}'", toolName);
            return FailResult<TResult>($"Circuit breaker open for tool '{toolName}'", callStart);
        }

        // 6. Audit log before invocation
        _logger.LogInformation(
            "Tool invocation: {Tool} by script (params length: {Len})",
            toolName,
            JsonSerializer.Serialize(parameters).Length);

        _calledToolsThisSession.Add(toolName);

        // 7. Resolve the tool from DI and invoke
        try
        {
            var toolInstance = _serviceProvider.GetService(descriptor.ImplementationType);
            if (toolInstance == null)
            {
                _logger.LogError("Tool '{Tool}' type '{Type}' is not registered in DI", toolName, descriptor.ImplementationType.Name);
                return FailResult<TResult>($"Tool '{toolName}' not registered in DI", callStart);
            }

            var resultObj = descriptor.InvokeMethod.Invoke(toolInstance, [parameters, cancellationToken]);

            if (resultObj is Task<TResult> typedTask)
            {
                var value = await typedTask.ConfigureAwait(false);
                ResetCircuit(toolName);
                return new ToolResult<TResult>
                {
                    Success = true,
                    Value = value,
                    Duration = DateTime.UtcNow - callStart,
                };
            }

            if (resultObj is Task baseTask)
            {
                await baseTask.ConfigureAwait(false);
            }

            ResetCircuit(toolName);
            return new ToolResult<TResult>
            {
                Success = true,
                Duration = DateTime.UtcNow - callStart,
            };
        }
        catch (Exception ex)
        {
            RecordFailure(toolName);
            _logger.LogError(ex, "Tool '{Tool}' invocation failed", toolName);
            return new ToolResult<TResult>
            {
                Success = false,
                Error = ex.Message,
                Duration = DateTime.UtcNow - callStart,
            };
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private bool HasPermission(string requiredPermission)
    {
        foreach (var p in _scriptPermissions)
        {
            if (p.Name.Equals(requiredPermission, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckRateLimit(string toolName)
    {
        var window = _rateLimitWindows.GetOrAdd(toolName, _ => new Queue<DateTime>());
        lock (window)
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(-1);
            while (window.Count > 0 && window.Peek() < cutoff)
                window.Dequeue();
            if (window.Count >= ToolRateLimitPerMinute)
            {
                return false;
            }
            window.Enqueue(now);
            return true;
        }
    }

    private bool IsCircuitOpen(string toolName)
    {
        if (!_circuitState.TryGetValue(toolName, out var state)) return false;
        if (state.Failures < CircuitBreakerThreshold) return false;
        if (DateTime.UtcNow - state.LastFailure > CircuitBreakerTimeout)
        {
            _circuitState.TryRemove(toolName, out _);
            return false;
        }
        return true;
    }

    private void RecordFailure(string toolName)
        => _circuitState.AddOrUpdate(
            toolName,
            _ => (1, DateTime.UtcNow),
            (_, s) => (s.Failures + 1, DateTime.UtcNow));

    private void ResetCircuit(string toolName)
        => _circuitState.TryRemove(toolName, out _);

    private static ToolResult<TResult> FailResult<TResult>(string error, DateTime start) =>
        new() { Success = false, Error = error, Duration = DateTime.UtcNow - start };
}
