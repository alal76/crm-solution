// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Scripting;

/// <summary>
/// Provides scripts with gated, audited access to CRM platform tools.
/// All egress from a script sandbox MUST go through this invoker;
/// direct access to the DI container, <c>DbContext</c>, or HTTP is prohibited.
/// </summary>
public interface IToolInvoker
{
    /// <summary>
    /// Invokes a named tool registered in the Tool Bridge registry.
    /// Implementations enforce permissions, SoD rules, rate limits, and audit logging.
    /// </summary>
    /// <typeparam name="TResult">Expected return type of the tool.</typeparam>
    /// <param name="toolName">Tool name as registered in the <c>ToolRegistry</c>.</param>
    /// <param name="parameters">Input parameters for the tool (serialised via System.Text.Json).</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    Task<ToolResult<TResult>> CallAsync<TResult>(
        string toolName,
        object parameters,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result returned by a Tool Bridge invocation.
/// </summary>
/// <typeparam name="TResult">Type of value returned by the tool on success.</typeparam>
public record ToolResult<TResult>
{
    /// <summary><c>true</c> when the tool completed without an unhandled exception or timeout.</summary>
    public required bool Success { get; init; }

    /// <summary>The tool's return value; <c>null</c> on failure or for <c>void</c> tools.</summary>
    public TResult? Value { get; init; }

    /// <summary>Error message when <see cref="Success"/> is <c>false</c>.</summary>
    public string? Error { get; init; }

    /// <summary>Wall-clock duration of the tool invocation, including network round-trips.</summary>
    public TimeSpan Duration { get; init; }
}
