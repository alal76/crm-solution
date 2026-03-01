// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using System.Text.RegularExpressions;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Loads user-authored <see cref="ScriptPluginDto"/> records from the database and
/// registers them as <see cref="KernelPlugin"/> instances inside a Semantic Kernel.
/// Each active script plugin becomes a standalone <see cref="KernelPlugin"/> containing
/// a single <see cref="KernelFunction"/> that executes the plugin's code via the
/// matching <see cref="IScriptEngine"/>.
/// </summary>
public sealed class ScriptPluginLoader
{
    private readonly IScriptPluginService _scriptPluginService;
    private readonly ScriptEngineFactory _scriptEngineFactory;
    private readonly ILogger<ScriptPluginLoader> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptPluginLoader"/> class.
    /// </summary>
    /// <param name="scriptPluginService">Service for querying persisted script plugins.</param>
    /// <param name="scriptEngineFactory">Factory that resolves script engines by language.</param>
    /// <param name="logger">Logger instance.</param>
    public ScriptPluginLoader(
        IScriptPluginService scriptPluginService,
        ScriptEngineFactory scriptEngineFactory,
        ILogger<ScriptPluginLoader> logger)
    {
        _scriptPluginService = scriptPluginService ?? throw new ArgumentNullException(nameof(scriptPluginService));
        _scriptEngineFactory = scriptEngineFactory ?? throw new ArgumentNullException(nameof(scriptEngineFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads all active script plugins from the database and returns them as an array of
    /// <see cref="KernelPlugin"/> instances ready to be added to a <see cref="Kernel"/>.
    /// Plugins whose language engine is unavailable are skipped with a warning log.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Array of <see cref="KernelPlugin"/> — empty when no active plugins are found or
    /// all engines are unavailable.
    /// </returns>
    public async Task<KernelPlugin[]> LoadActivePluginsAsync(CancellationToken ct = default)
    {
        var scriptPlugins = await _scriptPluginService
            .GetAllAsync(includeInactive: false, ct)
            .ConfigureAwait(false);

        if (scriptPlugins.Count == 0)
        {
            _logger.LogDebug("No active script plugins found");
            return Array.Empty<KernelPlugin>();
        }

        var kernelPlugins = new List<KernelPlugin>(scriptPlugins.Count);

        foreach (var plugin in scriptPlugins)
        {
            // Resolve script engine — skip plugin if no suitable engine is available
            IScriptEngine engine;
            try
            {
                engine = _scriptEngineFactory.GetEngine((ScriptLanguage)plugin.Language);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex,
                    "Skipping script plugin '{PluginName}' (id={Id}): no engine available for language {Language}",
                    plugin.Name, plugin.Id, plugin.Language);
                continue;
            }

            var sanitizedName = SanitizeName(plugin.Name);

            // Capture loop variables for the async closure
            var capturedPlugin = plugin;
            var capturedEngine = engine;

            // Create a kernel function whose delegate executes the plugin's code
            Func<string?, CancellationToken, Task<string?>> method =
                async (input, token) =>
                {
                    var variables = new Dictionary<string, object?>
                    {
                        ["input"] = input,
                    };

                    var result = await capturedEngine
                        .ExecuteAsync(
                            capturedPlugin.Code,
                            variables,
                            new Dictionary<string, object?>(),
                            timeout: null,
                            cancellationToken: token)
                        .ConfigureAwait(false);

                    if (!result.Success)
                    {
                        _logger.LogWarning(
                            "Script plugin '{PluginName}' execution failed: {Error}",
                            capturedPlugin.Name, result.ErrorMessage);
                        return result.ErrorMessage ?? "Script execution failed.";
                    }

                    return result.ReturnValue?.ToString() ?? string.Empty;
                };

            var kernelFunction = KernelFunctionFactory.CreateFromMethod(
                method,
                functionName: sanitizedName,
                description: plugin.Description ?? plugin.Name);

            kernelPlugins.Add(
                KernelPluginFactory.CreateFromFunctions(
                    sanitizedName,
                    plugin.Description ?? plugin.Name,
                    new[] { kernelFunction }));

            _logger.LogDebug(
                "Loaded script plugin '{PluginName}' (id={Id}) as kernel plugin/function '{FunctionName}'",
                plugin.Name, plugin.Id, sanitizedName);
        }

        _logger.LogDebug("Loaded {Count}/{Total} script plugin(s) as kernel plugins",
            kernelPlugins.Count, scriptPlugins.Count);

        return kernelPlugins.ToArray();
    }

    /// <summary>
    /// Loads all active script plugins and imports them into the supplied <paramref name="kernel"/>.
    /// </summary>
    /// <param name="kernel">The kernel to receive the script plugins.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ImportPluginsIntoKernelAsync(Kernel kernel, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(kernel);

        var plugins = await LoadActivePluginsAsync(ct).ConfigureAwait(false);

        foreach (var plugin in plugins)
        {
            kernel.Plugins.Add(plugin);
        }

        _logger.LogInformation("Imported {Count} script plugin(s) into kernel", plugins.Length);
    }

    /// <summary>
    /// Converts a raw plugin name into a valid Semantic Kernel function/plugin identifier:
    /// splits on non-alphanumeric characters, applies PascalCase, and ensures the result
    /// starts with a letter.
    /// </summary>
    /// <param name="name">Raw plugin display name.</param>
    /// <returns>Sanitized PascalCase identifier safe for use as a <see cref="KernelPlugin"/> name.</returns>
    private static string SanitizeName(string name)
    {
        var words = Regex.Split(name.Trim(), @"[^a-zA-Z0-9]+", RegexOptions.None, TimeSpan.FromSeconds(1))
            .Where(static w => w.Length > 0)
            .Select(static w => char.ToUpperInvariant(w[0]) + w[1..]);

        var sanitized = string.Concat(words);

        // Ensure the identifier starts with a letter
        if (sanitized.Length == 0 || !char.IsLetter(sanitized[0]))
        {
            sanitized = "Script" + sanitized;
        }

        return sanitized;
    }
}
