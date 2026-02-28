// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.Extensions.DependencyInjection;
using CRM.Core.Scripting;
using CRM.Infrastructure.Scripting.Roslyn;
using CRM.Infrastructure.Scripting.Tools;
using CRM.Infrastructure.Scripting.Workflow;

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

        return services;
    }
}
