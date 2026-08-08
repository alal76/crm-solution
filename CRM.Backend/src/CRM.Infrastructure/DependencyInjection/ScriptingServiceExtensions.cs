// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Infrastructure.Factories;
using CRM.Infrastructure.Scripting;
using CRM.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Infrastructure.DependencyInjection;

/// <summary>
/// DI helpers for registering scripting engines and related factories.
/// </summary>
public static class ScriptingServiceExtensions
{
    public static IServiceCollection AddScriptingEngines(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IScriptEngine, JintScriptEngine>();

        // REV-STUB-008: PythonScriptEngine delegates execution to the crm-python-script-runner
        // sidecar (python-script-runner/ at the repo root) over HTTP. IsAvailable performs a
        // live health check against the sidecar, so no feature flag gate is required here —
        // the engine reports itself unavailable automatically when the sidecar isn't reachable.
        services.Configure<PythonScriptEngineOptions>(configuration.GetSection("Scripting:Python"));
        services.AddHttpClient("crm-python-script-runner", (sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PythonScriptEngineOptions>>().Value;
            client.Timeout = options.HttpTimeout + TimeSpan.FromSeconds(5); // slack beyond the per-call CancellationTokenSource
        });
        services.AddSingleton<IScriptEngine, PythonScriptEngine>();

        services.AddSingleton<ScriptEngineFactory>();
        services.AddScoped<IScriptPluginService, ScriptPluginService>();
        services.AddScoped<ScriptPluginLoader>();
        return services;
    }
}
