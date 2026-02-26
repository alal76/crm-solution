// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Factories;
using CRM.Infrastructure.Scripting;
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
        services.AddSingleton<ScriptEngineFactory>();
        return services;
    }
}
