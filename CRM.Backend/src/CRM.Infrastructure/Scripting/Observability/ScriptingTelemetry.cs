// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using DiagActivity = System.Diagnostics.Activity;
using DiagActivitySource = System.Diagnostics.ActivitySource;

namespace CRM.Infrastructure.Scripting.Observability;

/// <summary>
/// OpenTelemetry instrumentation for the scripting engine.
/// SARCH-075: Activity spans per execution
/// SARCH-076: Counters (invocations, successes, failures)
/// SARCH-077: Histograms (duration, memory usage)
/// </summary>
public static class ScriptingTelemetry
{
    public static readonly string ActivitySourceName = "CRM.Scripting";
    public static readonly DiagActivitySource ActivitySource = new(ActivitySourceName, "1.0");

    private static readonly Meter ScriptMeter = new("CRM.Scripting", "1.0");

    // SARCH-076: Counters
    public static readonly Counter<long> CompilationCount =
        ScriptMeter.CreateCounter<long>("crm.scripting.compilations.total", description: "Total script compilations");

    public static readonly Counter<long> ExecutionCount =
        ScriptMeter.CreateCounter<long>("crm.scripting.executions.total", description: "Total script executions");

    public static readonly Counter<long> ExecutionSuccess =
        ScriptMeter.CreateCounter<long>("crm.scripting.executions.success", description: "Successful executions");

    public static readonly Counter<long> ExecutionFailures =
        ScriptMeter.CreateCounter<long>("crm.scripting.executions.failure", description: "Failed executions");

    public static readonly Counter<long> GuardrailBlocked =
        ScriptMeter.CreateCounter<long>("crm.scripting.guardrail.blocked", description: "Requests blocked by guardrails");

    public static readonly Counter<long> ToolBridgeCalls =
        ScriptMeter.CreateCounter<long>("crm.scripting.tool_bridge.calls", description: "Tool Bridge invocations");

    // SARCH-077: Histograms
    public static readonly Histogram<double> ExecutionDuration =
        ScriptMeter.CreateHistogram<double>(
            "crm.scripting.execution.duration_ms",
            unit: "ms",
            description: "Script execution duration in milliseconds");

    public static readonly Histogram<long> MemoryUsage =
        ScriptMeter.CreateHistogram<long>(
            "crm.scripting.execution.memory_bytes",
            unit: "bytes",
            description: "Peak memory usage per script execution");

    public static readonly Histogram<long> CompilationDuration =
        ScriptMeter.CreateHistogram<long>(
            "crm.scripting.compilation.duration_ms",
            unit: "ms",
            description: "Script compilation duration");

    /// <summary>Start a tracing span for a script compile operation.</summary>
    public static DiagActivity? StartCompileSpan(string scriptId, string runtime)
    {
        var activity = ActivitySource.StartActivity("script.compile");
        activity?.SetTag("script.id", scriptId);
        activity?.SetTag("script.runtime", runtime);
        return activity;
    }

    /// <summary>Start a tracing span for a script execute operation.</summary>
    public static DiagActivity? StartExecuteSpan(string scriptId, string runtime, string kind)
    {
        var activity = ActivitySource.StartActivity("script.execute");
        activity?.SetTag("script.id", scriptId);
        activity?.SetTag("script.runtime", runtime);
        activity?.SetTag("script.kind", kind);
        return activity;
    }

    /// <summary>Record execution outcome metrics.</summary>
    public static void RecordExecution(bool success, double durationMs, long memoryBytes, string runtime)
    {
        ExecutionCount.Add(1, new TagList { { "runtime", runtime } });

        if (success)
        {
            ExecutionSuccess.Add(1, new TagList { { "runtime", runtime } });
        }
        else
        {
            ExecutionFailures.Add(1, new TagList { { "runtime", runtime } });
        }

        ExecutionDuration.Record(durationMs, new TagList { { "runtime", runtime } });
        MemoryUsage.Record(memoryBytes, new TagList { { "runtime", runtime } });
    }
}
