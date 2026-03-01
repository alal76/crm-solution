// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Scripting;

/// <summary>
/// Structured logging interface exposed to scripts inside the sandbox.
/// Log entries are captured by the platform's audit/observability pipeline
/// and associated with the current execution context.
/// </summary>
public interface IScriptLogger
{
    /// <summary>Logs a debug-level message.</summary>
    void LogDebug(string message, params object?[] args);

    /// <summary>Logs an informational message.</summary>
    void LogInfo(string message, params object?[] args);

    /// <summary>Logs a warning-level message.</summary>
    void LogWarning(string message, params object?[] args);

    /// <summary>Logs an error-level message.</summary>
    void LogError(string message, params object?[] args);
}
