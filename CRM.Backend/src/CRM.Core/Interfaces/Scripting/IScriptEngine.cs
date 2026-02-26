// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Enums;

namespace CRM.Core.Interfaces.Scripting;

/// <summary>
/// Contract for executing sandboxed scripts in a specific language.
/// </summary>
public interface IScriptEngine
{
    /// <summary>Language handled by this engine.</summary>
    ScriptLanguage Language { get; }

    /// <summary>True when engine dependencies are available (e.g., Python runtime installed).</summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Execute a script with provided variables and context.
    /// </summary>
    Task<ScriptExecutionResult> ExecuteAsync(
        string code,
        Dictionary<string, object?> variables,
        Dictionary<string, object?> context,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate script syntax without executing it. Returns empty when valid.
    /// </summary>
    Task<IReadOnlyList<ScriptDiagnostic>> ValidateSyntaxAsync(
        string code,
        CancellationToken cancellationToken = default);
}

/// <summary>Result of a script execution.</summary>
/// <param name="Success">Whether execution completed successfully.</param>
/// <param name="ReturnValue">Result returned by the script (if any).</param>
/// <param name="Logs">Captured log entries emitted during execution.</param>
/// <param name="ErrorMessage">Error description when execution failed.</param>
/// <param name="ExecutionTime">Measured execution duration.</param>
public record ScriptExecutionResult(
    bool Success,
    object? ReturnValue,
    IReadOnlyList<string> Logs,
    string? ErrorMessage,
    TimeSpan ExecutionTime);

/// <summary>Represents a syntax or validation diagnostic for a script.</summary>
/// <param name="Line">Line number (1-based when available).</param>
/// <param name="Column">Column number (1-based when available).</param>
/// <param name="Message">Diagnostic text.</param>
/// <param name="Severity">Severity of the diagnostic.</param>
public record ScriptDiagnostic(int Line, int Column, string Message, DiagnosticSeverity Severity);

/// <summary>Severity levels for script diagnostics.</summary>
public enum DiagnosticSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2
}
