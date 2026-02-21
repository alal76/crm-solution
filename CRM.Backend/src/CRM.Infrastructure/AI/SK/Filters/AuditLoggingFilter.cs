// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CRM.Infrastructure.AI.SK.Filters;

/// <summary>
/// Semantic Kernel function invocation filter that logs every function call
/// with timing information for audit and performance monitoring.
/// </summary>
/// <remarks>
/// This filter runs on ALL SK function invocations (plugins, prompts, etc.)
/// and produces structured log entries that can be ingested by log analytics platforms.
/// </remarks>
public class AuditLoggingFilter : IFunctionInvocationFilter
{
    #region Fields

    private readonly ILogger<AuditLoggingFilter> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLoggingFilter"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public AuditLoggingFilter(ILogger<AuditLoggingFilter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IFunctionInvocationFilter Implementation

    /// <inheritdoc />
    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        var pluginName = context.Function.PluginName ?? "Kernel";
        var functionName = context.Function.Name;
        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "SK Function invoked: {Plugin}.{Function} | Args: {ArgCount}",
            pluginName,
            functionName,
            context.Arguments?.Count ?? 0);

        try
        {
            await next(context);
            sw.Stop();

            _logger.LogInformation(
                "SK Function completed: {Plugin}.{Function} in {ElapsedMs}ms | HasResult: {HasResult}",
                pluginName,
                functionName,
                sw.ElapsedMilliseconds,
                context.Result != null);
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogError(ex,
                "SK Function failed: {Plugin}.{Function} after {ElapsedMs}ms | Error: {ErrorType}",
                pluginName,
                functionName,
                sw.ElapsedMilliseconds,
                ex.GetType().Name);

            throw;
        }
    }

    #endregion
}
