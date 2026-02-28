// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Scripting.AgentHooks;

/// <summary>Common context passed to all lifecycle hooks.</summary>
public class AgentHookContext
{
    public string AgentId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public Dictionary<string, object?> Variables { get; set; } = new();
    public AgentBudgetState BudgetState { get; set; } = new();
}

public class AgentBudgetState
{
    public int TokensUsed { get; set; }
    public int CallsThisHour { get; set; }
    public decimal CostToday { get; set; }
}

public class HookResult
{
    /// <summary>Gets or sets a value indicating whether agent execution should continue.</summary>
    public bool Continue { get; set; } = true;

    /// <summary>Gets or sets an optional overridden input to pass forward in the pipeline.</summary>
    public string? ModifiedInput { get; set; }

    /// <summary>Gets or sets the reason for halting execution when Continue is false.</summary>
    public string? Reason { get; set; }
    public Dictionary<string, object?> Metadata { get; set; } = new();
}

/// <summary>SARCH-062: Called when agent session begins.</summary>
public interface IOnActivateHook
{
    Task<HookResult> OnActivateAsync(AgentHookContext ctx, CancellationToken ct = default);
}

/// <summary>SARCH-063: Called when agent generates a plan / reasoning step.</summary>
public interface IOnPlanHook
{
    Task<HookResult> OnPlanAsync(AgentHookContext ctx, string plan, CancellationToken ct = default);
}

/// <summary>SARCH-064: Called before any tool invocation.</summary>
public interface IOnBeforeToolCallHook
{
    Task<HookResult> OnBeforeToolCallAsync(AgentHookContext ctx, string toolName, object parameters, CancellationToken ct = default);
}

/// <summary>SARCH-065: Called after any tool invocation.</summary>
public interface IOnAfterToolCallHook
{
    Task<HookResult> OnAfterToolCallAsync(AgentHookContext ctx, string toolName, object? toolResult, CancellationToken ct = default);
}

/// <summary>SARCH-066: Called before agent sends a response to user.</summary>
public interface IOnResponseHook
{
    Task<HookResult> OnResponseAsync(AgentHookContext ctx, string responseText, CancellationToken ct = default);
}

/// <summary>SARCH-067: Called when agent encounters an error.</summary>
public interface IOnErrorHook
{
    Task<HookResult> OnErrorAsync(AgentHookContext ctx, string errorMessage, System.Exception? exception, CancellationToken ct = default);
}

/// <summary>SARCH-068: Called when agent session ends.</summary>
public interface IOnDeactivateHook
{
    Task<HookResult> OnDeactivateAsync(AgentHookContext ctx, CancellationToken ct = default);
}

/// <summary>SARCH-069: Guardrail hook — run a safety script against input/output.</summary>
public interface IGuardrailHook
{
    Task<HookResult> CheckAsync(AgentHookContext ctx, string content, GuardrailCheckType checkType, CancellationToken ct = default);
}

public enum GuardrailCheckType
{
    Input = 0,
    Output = 1,
    PlanStep = 2
}
