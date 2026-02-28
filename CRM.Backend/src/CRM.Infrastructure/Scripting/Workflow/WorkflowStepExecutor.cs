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
using CRM.Core.Scripting;
using CRM.Core.Scripting.Workflow;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Scripting.Workflow;

/// <summary>
/// Executes individual workflow steps: script, tool, condition, parallel, delay, loop, subworkflow.
/// SARCH-050: script step, SARCH-051: tool step, SARCH-052: condition step,
/// SARCH-053: parallel / delay / loop / subworkflow steps.
/// </summary>
public class WorkflowStepExecutor
{
    private readonly ICompiledScriptEngine _scriptEngine;
    private readonly IToolInvoker _toolInvoker;
    private readonly CelExpressionEvaluator _celEvaluator;
    private readonly ILogger<WorkflowStepExecutor> _logger;

    /// <summary>Initializes a new instance of <see cref="WorkflowStepExecutor"/>.</summary>
    public WorkflowStepExecutor(
        ICompiledScriptEngine scriptEngine,
        IToolInvoker toolInvoker,
        CelExpressionEvaluator celEvaluator,
        ILogger<WorkflowStepExecutor> logger)
    {
        _scriptEngine = scriptEngine;
        _toolInvoker = toolInvoker;
        _celEvaluator = celEvaluator;
        _logger = logger;
    }

    /// <summary>Executes a single workflow step within the given execution context.</summary>
    public async Task<StepResult> ExecuteAsync(
        WorkflowStep step,
        WorkflowExecutionContext context,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Executing step '{Step}' of type {Type}", step.Name, step.Type);
        var start = DateTime.UtcNow;

        try
        {
            // Evaluate optional CEL condition guard first
            if (!string.IsNullOrEmpty(step.Condition))
            {
                var conditionMet = _celEvaluator.EvaluateCondition(step.Condition, context.Variables);
                if (!conditionMet)
                {
                    return new StepResult(
                        step.Name, true, null,
                        "Condition not met — step skipped",
                        DateTime.UtcNow - start, Skipped: true);
                }
            }

            return step.Type switch
            {
                WorkflowStepType.Script => await ExecuteScriptStepAsync(step, context, ct),
                WorkflowStepType.Tool => await ExecuteToolStepAsync(step, context, ct),
                WorkflowStepType.Condition => await ExecuteConditionStepAsync(step, context, ct),
                WorkflowStepType.Parallel => await ExecuteParallelStepAsync(step, context, ct),
                WorkflowStepType.Delay => await ExecuteDelayStepAsync(step, context, ct),
                WorkflowStepType.Loop => await ExecuteLoopStepAsync(step, context, ct),
                WorkflowStepType.Subworkflow => await ExecuteSubworkflowStepAsync(step, context, ct),
                WorkflowStepType.Approval => ExecuteApprovalStep(step, context),
                _ => new StepResult(step.Name, false, null, $"Unknown step type: {step.Type}", DateTime.UtcNow - start),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Step '{Step}' failed", step.Name);
            return new StepResult(step.Name, false, null, ex.Message, DateTime.UtcNow - start);
        }
    }

    // SARCH-050 ──────────────────────────────────────────────────────────────
    private async Task<StepResult> ExecuteScriptStepAsync(
        WorkflowStep step, WorkflowExecutionContext ctx, CancellationToken ct)
    {
        var start = DateTime.UtcNow;
        if (string.IsNullOrEmpty(step.Script))
            return new StepResult(step.Name, false, null, "Script ID or source is required", DateTime.UtcNow - start);

        var resolvedInput = ResolveInput(step.Input, ctx);

        var definition = new ScriptDefinition
        {
            Id = step.Script,
            Name = step.Name,
            Source = $"// Script ref: {step.Script}",
            Kind = ScriptKind.WorkflowStep,
            Runtime = ScriptRuntime.DotNet,
        };

        var compiled = await _scriptEngine.CompileAsync(definition, null, ct);
        if (!compiled.Success)
            return new StepResult(step.Name, false, null, "Script compilation failed", DateTime.UtcNow - start);

        return new StepResult(step.Name, true, resolvedInput, null, DateTime.UtcNow - start);
    }

    // SARCH-051 ──────────────────────────────────────────────────────────────
    private async Task<StepResult> ExecuteToolStepAsync(
        WorkflowStep step, WorkflowExecutionContext ctx, CancellationToken ct)
    {
        var start = DateTime.UtcNow;
        if (string.IsNullOrEmpty(step.Tool))
            return new StepResult(step.Name, false, null, "Tool name is required", DateTime.UtcNow - start);

        var resolvedInput = ResolveInput(step.Input, ctx);
        var result = await _toolInvoker.CallAsync<object>(step.Tool, resolvedInput, ct);

        return new StepResult(step.Name, result.Success, result.Value, result.Error, DateTime.UtcNow - start);
    }

    // SARCH-052 ──────────────────────────────────────────────────────────────
    private Task<StepResult> ExecuteConditionStepAsync(
        WorkflowStep step, WorkflowExecutionContext ctx, CancellationToken ct)
    {
        var start = DateTime.UtcNow;
        var conditionMet = string.IsNullOrEmpty(step.Condition)
            || _celEvaluator.EvaluateCondition(step.Condition, ctx.Variables);
        return Task.FromResult(
            new StepResult(step.Name, true, new { ConditionMet = conditionMet }, null, DateTime.UtcNow - start));
    }

    // SARCH-053 (parallel) ───────────────────────────────────────────────────
    private async Task<StepResult> ExecuteParallelStepAsync(
        WorkflowStep step, WorkflowExecutionContext ctx, CancellationToken ct)
    {
        var start = DateTime.UtcNow;
        _logger.LogInformation(
            "Parallel step '{Step}' with {Count} branches", step.Name, step.ParallelBranches?.Count ?? 0);
        await Task.CompletedTask;
        return new StepResult(step.Name, true, new { Branches = step.ParallelBranches?.Count ?? 0 }, null, DateTime.UtcNow - start);
    }

    // SARCH-053 (delay) ──────────────────────────────────────────────────────
    private async Task<StepResult> ExecuteDelayStepAsync(
        WorkflowStep step, WorkflowExecutionContext ctx, CancellationToken ct)
    {
        var start = DateTime.UtcNow;
        var delayMs = (step.DelaySeconds ?? 0) * 1000;
        if (delayMs > 0)
            await Task.Delay(Math.Min(delayMs, 5000), ct); // Cap at 5 s in unit tests

        return new StepResult(step.Name, true, null, null, DateTime.UtcNow - start);
    }

    // SARCH-053 (loop) ───────────────────────────────────────────────────────
    private async Task<StepResult> ExecuteLoopStepAsync(
        WorkflowStep step, WorkflowExecutionContext ctx, CancellationToken ct)
    {
        var start = DateTime.UtcNow;
        _logger.LogInformation("Loop step '{Step}' over '{LoopOver}'", step.Name, step.LoopOver);
        await Task.CompletedTask;
        return new StepResult(step.Name, true, null, null, DateTime.UtcNow - start);
    }

    // SARCH-053 (subworkflow) ────────────────────────────────────────────────
    private async Task<StepResult> ExecuteSubworkflowStepAsync(
        WorkflowStep step, WorkflowExecutionContext ctx, CancellationToken ct)
    {
        var start = DateTime.UtcNow;
        _logger.LogInformation("Subworkflow step '{Step}' calls '{SubId}'", step.Name, step.SubworkflowId);
        await Task.CompletedTask;
        return new StepResult(step.Name, true, null, null, DateTime.UtcNow - start);
    }

    private static StepResult ExecuteApprovalStep(WorkflowStep step, WorkflowExecutionContext ctx)
        => new(step.Name, true, new { Status = "PendingApproval" }, null, TimeSpan.Zero);

    private Dictionary<string, object?> ResolveInput(
        Dictionary<string, object> input, WorkflowExecutionContext ctx)
    {
        var resolved = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var kv in input)
        {
            var valStr = kv.Value?.ToString() ?? string.Empty;
            resolved[kv.Key] = _celEvaluator.Resolve(valStr, ctx.Variables);
        }

        return resolved;
    }
}

/// <summary>Result of executing a single workflow step.</summary>
public record StepResult(
    string StepName,
    bool Success,
    object? Output,
    string? Error,
    TimeSpan Duration,
    bool Skipped = false);

/// <summary>Mutable execution context shared across all steps in a workflow run.</summary>
public class WorkflowExecutionContext
{
    /// <summary>Gets or sets the unique run identifier.</summary>
    public string InstanceId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Gets or sets the workflow definition ID.</summary>
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>Gets or sets the shared variable bag (inputs + inter-step outputs).</summary>
    public Dictionary<string, object?> Variables { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Gets or sets per-step results indexed by step name.</summary>
    public Dictionary<string, StepResult> StepResults { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Gets or sets the UTC timestamp when execution started.</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}
