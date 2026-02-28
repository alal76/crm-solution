// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Scripting;
using CRM.Core.Scripting.MultiAgent;
using CRM.Infrastructure.Scripting.AgentHooks;
using CRM.Infrastructure.Scripting.MultiAgent;
using CRM.Infrastructure.Scripting.Roslyn;
using CRM.Infrastructure.Scripting.Tools;
using CRM.Infrastructure.Scripting.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Infrastructure.Scripting;

/// <summary>
/// Extension methods that register the full CRM scripting stack with the DI container.
/// Call <c>builder.Services.AddCrmScripting()</c> in <c>Program.cs</c>.
/// </summary>
public static class ScriptingServiceExtensions
{
    public static IServiceCollection AddCrmScripting(this IServiceCollection services)
    {
        services.AddSingleton<ScriptArtefactStore>();
        services.AddSingleton<ScriptBreakingChangeDetector>();
        services.AddSingleton<MemoryWatchdog>();
        services.AddSingleton<ICompiledScriptEngine, RoslynScriptEngine>();

        // Tool Bridge
        services.AddSingleton<ToolRegistry>();
        services.AddSingleton<IMetricsRecorder, OtelMetricsRecorder>();

        // CRM platform tools (transient — resolved per-invocation)
        services.AddTransient<GetCustomerTool>();
        services.AddTransient<SendEmailTool>();

        // SARCH-048→059: YAML WDL workflow engine
        services.AddSingleton<YamlWdlParser>();
        services.AddSingleton<CelExpressionEvaluator>();
        services.AddScoped<WorkflowStepExecutor>();
        services.AddScoped<WorkflowOrchestrator>();

        // SARCH-061→074: Agent lifecycle hooks + guardrails
        services.AddSingleton<GuardrailPipeline>();
        services.AddSingleton<AgentBudgetEnforcer>();

        // SARCH-083: Inter-agent messaging bus
        services.AddSingleton<IAgentMessageBus, InMemoryAgentMessageBus>();

        // SARCH-084: Episodic agent memory store
        services.AddSingleton<IAgentMemoryStore, InMemoryAgentMemoryStore>();

        return services;
    }
}
