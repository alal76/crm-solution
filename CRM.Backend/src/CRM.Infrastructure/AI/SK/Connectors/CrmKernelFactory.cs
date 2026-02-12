// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Configuration;
using CRM.Infrastructure.AI.SK.Filters;

namespace CRM.Infrastructure.AI.SK.Connectors;

/// <summary>
/// Factory for creating configured Semantic Kernel instances with CRM connectors, plugins, and filters.
/// Each kernel is assembled on demand with the appropriate chat connector, plugins, and audit filters.
/// </summary>
public class CrmKernelFactory
{
    #region Fields

    private readonly IServiceProvider _serviceProvider;
    private readonly SemanticKernelOptions _options;
    private readonly ILogger<CrmKernelFactory> _logger;

    /// <summary>
    /// Cache of resolved plugin types to avoid repeated assembly scanning.
    /// </summary>
    private static readonly Dictionary<string, Type?> _pluginTypeCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _pluginTypeCacheLock = new();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CrmKernelFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">Application service provider for resolving dependencies.</param>
    /// <param name="options">Semantic Kernel configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public CrmKernelFactory(
        IServiceProvider serviceProvider,
        IOptions<SemanticKernelOptions> options,
        ILogger<CrmKernelFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Creates a new <see cref="Kernel"/> configured with CRM connectors and the specified plugins.
    /// </summary>
    /// <param name="pluginNames">Optional list of plugin names to import (e.g. "Account", "Lead").</param>
    /// <returns>A fully configured Semantic Kernel instance.</returns>
    public Kernel CreateKernel(IEnumerable<string>? pluginNames = null)
    {
        _logger.LogDebug("Creating new Semantic Kernel instance");

        var builder = Kernel.CreateBuilder();

        // Add CRM chat completion connector (bridges to IAIPort)
        var chatConnector = _serviceProvider.GetRequiredService<CrmChatCompletionConnector>();
        builder.Services.AddSingleton<IChatCompletionService>(chatConnector);

        // Add logging infrastructure
        builder.Services.AddSingleton(_serviceProvider.GetRequiredService<ILoggerFactory>());

        // Add filters for auditing, approval, and cost tracking
        RegisterFilters(builder);

        var kernel = builder.Build();

        // Import requested plugins into the kernel
        if (pluginNames != null)
        {
            foreach (var pluginName in pluginNames)
            {
                ImportPlugin(kernel, pluginName);
            }
        }

        _logger.LogInformation("Semantic Kernel created with {PluginCount} plugin(s)",
            kernel.Plugins.Count);

        return kernel;
    }

    /// <summary>
    /// Creates a kernel configured for a specific <see cref="AIAgent"/>, importing only
    /// the plugins the agent is allowed to use.
    /// </summary>
    /// <param name="agent">The AI agent definition.</param>
    /// <returns>A Kernel scoped to the agent's allowed plugins.</returns>
    public Kernel CreateKernelForAgent(AIAgent agent)
    {
        ArgumentNullException.ThrowIfNull(agent);

        var allowedPlugins = agent.AllowedPlugins?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        _logger.LogDebug("Creating kernel for agent '{AgentName}' with plugins: [{Plugins}]",
            agent.Name,
            agent.AllowedPlugins ?? "none");

        return CreateKernel(allowedPlugins);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Registers SK function invocation filters on the kernel builder.
    /// </summary>
    private void RegisterFilters(IKernelBuilder builder)
    {
        var auditFilter = _serviceProvider.GetService<AuditLoggingFilter>();
        if (auditFilter != null)
        {
            builder.Services.AddSingleton<IFunctionInvocationFilter>(auditFilter);
        }

        var approvalFilter = _serviceProvider.GetService<HumanApprovalFilter>();
        if (approvalFilter != null)
        {
            builder.Services.AddSingleton<IFunctionInvocationFilter>(approvalFilter);
        }

        var costFilter = _serviceProvider.GetService<CostTrackingFilter>();
        if (costFilter != null)
        {
            builder.Services.AddSingleton<IFunctionInvocationFilter>(costFilter);
        }
    }

    /// <summary>
    /// Resolves and imports a named plugin into the kernel.
    /// </summary>
    private void ImportPlugin(Kernel kernel, string pluginName)
    {
        var pluginInstance = ResolvePlugin(pluginName);
        if (pluginInstance != null)
        {
            kernel.ImportPluginFromObject(pluginInstance, pluginName);
            _logger.LogDebug("Imported plugin: {PluginName}", pluginName);
        }
        else
        {
            _logger.LogWarning("Plugin '{PluginName}' could not be resolved — skipping", pluginName);
        }
    }

    /// <summary>
    /// Resolves a plugin instance by name from the DI container.
    /// Plugin names map to types following the convention <c>{Name}Plugin</c>
    /// in the <c>CRM.Infrastructure.AI.SK.Plugins</c> namespace.
    /// </summary>
    private object? ResolvePlugin(string pluginName)
    {
        var pluginType = GetPluginType(pluginName);
        if (pluginType == null)
        {
            return null;
        }

        return _serviceProvider.GetService(pluginType)
            ?? ActivatorUtilities.CreateInstance(_serviceProvider, pluginType);
    }

    /// <summary>
    /// Looks up the CLR type for a plugin name, caching results to avoid repeated assembly scanning.
    /// </summary>
    private static Type? GetPluginType(string pluginName)
    {
        var expectedTypeName = $"{pluginName}Plugin";

        lock (_pluginTypeCacheLock)
        {
            if (_pluginTypeCache.TryGetValue(expectedTypeName, out var cached))
            {
                return cached;
            }
        }

        // Search loaded assemblies for the plugin type
        var pluginType = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .FirstOrDefault(t => t.Name == expectedTypeName
                && t.Namespace != null
                && t.Namespace.Contains("SK.Plugins", StringComparison.Ordinal));

        lock (_pluginTypeCacheLock)
        {
            _pluginTypeCache[expectedTypeName] = pluginType;
        }

        return pluginType;
    }

    #endregion
}
