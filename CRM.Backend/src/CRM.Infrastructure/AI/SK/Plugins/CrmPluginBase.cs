// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Abstract base class for all CRM Semantic Kernel plugins.
/// Provides common functionality including logging, JSON serialization,
/// and a standardized error-result format for function outputs.
/// </summary>
public abstract class CrmPluginBase
{
    #region Fields

#pragma warning disable SA1401 // Fields should be private (protected field in abstract base class by design)

    /// <summary>
    /// Logger available to all derived plugins.
    /// </summary>
    protected readonly ILogger Logger;

#pragma warning restore SA1401

    /// <summary>
    /// Shared JSON serializer options for consistent output formatting.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CrmPluginBase"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for the derived plugin.</param>
    protected CrmPluginBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Abstract Members

    /// <summary>
    /// Gets the plugin name used for registration and identification within the kernel.
    /// </summary>
    public abstract string PluginName { get; }

    /// <summary>
    /// Gets the human-readable description of this plugin's capabilities.
    /// This is surfaced to the LLM for function-calling decisions.
    /// </summary>
    public abstract string Description { get; }

    #endregion

    #region Protected Helpers

    /// <summary>
    /// Safely serializes an object to a JSON string using CRM conventions (camelCase, compact).
    /// Returns <c>"{}"</c> on serialization failure to avoid breaking the agent conversation.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A JSON string representation.</returns>
    protected string ToJson(object obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj, _jsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to serialize object in plugin {PluginName}", PluginName);
            return "{}";
        }
    }

    /// <summary>
    /// Creates a standardized error result string for returning from SK functions
    /// when an operation fails gracefully.
    /// </summary>
    /// <param name="operation">The operation that failed (e.g. "GetAccount").</param>
    /// <param name="message">A human-readable error description.</param>
    /// <returns>A JSON error object string.</returns>
    protected string ErrorResult(string operation, string message)
    {
        Logger.LogWarning("Plugin {PluginName}.{Operation} failed: {Message}", PluginName, operation, message);
        return ToJson(new { error = true, operation, message });
    }

    /// <summary>
    /// Creates a standardized success result string wrapping any data payload.
    /// </summary>
    /// <param name="data">The data to include in the result.</param>
    /// <returns>A JSON success object string.</returns>
    protected string SuccessResult(object data)
    {
        return ToJson(new { error = false, data });
    }

    #endregion
}
