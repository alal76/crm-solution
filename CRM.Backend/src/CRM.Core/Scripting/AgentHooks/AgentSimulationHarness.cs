// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Scripting.AgentHooks;

/// <summary>
/// Test harness for simulating agent lifecycle events in unit/integration tests.
/// Allows injecting mock hook responses without a real agent runtime.
/// SARCH-072.
/// </summary>
public class AgentSimulationHarness
{
    private readonly List<string> _log = new List<string>();
    private readonly Dictionary<string, HookResult> _hookOverrides = new Dictionary<string, HookResult>();
    private readonly string _agentId;

    private AgentSimulationHarness(string agentId) => _agentId = agentId;

    public static AgentSimulationHarness Create(string agentId = "test-agent") =>
        new AgentSimulationHarness(agentId);

    public AgentSimulationHarness WithHookResult(string hookName, HookResult result)
    {
        _hookOverrides[hookName] = result;
        return this;
    }

    public async Task<SimulationResult> SimulateAsync(
        string userMessage,
        Func<AgentHookContext, Task>? onEachHook = null,
        CancellationToken ct = default)
    {
        var ctx = new AgentHookContext
        {
            AgentId = _agentId,
            SessionId = Guid.NewGuid().ToString("N"),
        };

        await FireHookAsync("OnActivate", ctx, onEachHook);
        await FireHookAsync("OnPlan", ctx, onEachHook);
        await FireHookAsync("OnBeforeToolCall", ctx, onEachHook);
        await FireHookAsync("OnAfterToolCall", ctx, onEachHook);
        await FireHookAsync("OnResponse", ctx, onEachHook);
        await FireHookAsync("OnDeactivate", ctx, onEachHook);

        return new SimulationResult(_agentId, _log.AsReadOnly(), true);
    }

    private async Task FireHookAsync(
        string hookName,
        AgentHookContext ctx,
        Func<AgentHookContext, Task>? callback)
    {
        _log.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Hook fired: {hookName}");

        if (callback != null)
        {
            await callback(ctx);
        }

        if (_hookOverrides.TryGetValue(hookName, out var result) && !result.Continue)
        {
            _log.Add($"  → Hook '{hookName}' HALTED agent: {result.Reason}");
        }
    }
}

public record SimulationResult(
    string AgentId,
    IReadOnlyList<string> Log,
    bool Completed);
