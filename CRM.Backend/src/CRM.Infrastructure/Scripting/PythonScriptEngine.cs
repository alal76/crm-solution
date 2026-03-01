// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Scripting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Scripting;

/// <summary>
/// Python script engine implementation using Python.NET (pythonnet).
/// <para>
/// <b>Status: IMPLEMENTATION STUB (BACK-003 / SCRIPT-006)</b><br/>
/// Full implementation requires:
/// <list type="bullet">
/// <item><description>NuGet: <c>pythonnet</c> (Python.NET host)</description></item>
/// <item><description>NuGet: <c>pythonnet</c> &gt;= 3.0 with CPython 3.x runtime present on the host</description></item>
/// <item><description>Sandbox: RestrictedPython or custom import whitelist to prevent file system / network access</description></item>
/// <item><description>Feature flag: <c>FeatureManagement:EnablePythonScripting</c> must be <c>true</c></description></item>
/// </list>
/// </para>
/// <para>
/// When <see cref="IsAvailable"/> returns <c>false</c>, the script engine factory
/// will reject requests for <see cref="ScriptLanguage.Python"/> with a descriptive error rather than
/// silently falling back. All unit tests exercise this engine through mocks.
/// </para>
/// </summary>
public class PythonScriptEngine : IScriptEngine
{
    private readonly ILogger<PythonScriptEngine> _logger;

    /// <summary>Initializes a new instance of <see cref="PythonScriptEngine"/>.</summary>
    /// <param name="logger">Logger instance.</param>
    public PythonScriptEngine(ILogger<PythonScriptEngine> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public ScriptLanguage Language => ScriptLanguage.Python;

    /// <summary>
    /// Returns <c>false</c> until Python.NET (pythonnet NuGet) is integrated.
    /// Checked by the script engine factory before dispatching a script to this engine.
    /// </summary>
    public bool IsAvailable => false;

    /// <inheritdoc />
    public Task<ScriptExecutionResult> ExecuteAsync(
        string code,
        Dictionary<string, object?> variables,
        Dictionary<string, object?> context,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);

        _logger.LogWarning(
            "PythonScriptEngine: Python script execution requested but the engine is not available. " +
            "Add the 'pythonnet' NuGet package and set FeatureManagement:EnablePythonScripting=true " +
            "to enable Python scripting. (SCRIPT-006 / MASTER_TODO_LIST.md)");

        var errorMessage =
            "Python scripting is not available in this deployment. " +
            "Contact your administrator to enable the EnablePythonScripting feature flag.";

        return Task.FromResult(
            new ScriptExecutionResult(
                Success: false,
                ReturnValue: null,
                Logs: Array.Empty<string>(),
                ErrorMessage: errorMessage,
                ExecutionTime: TimeSpan.Zero));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ScriptDiagnostic>> ValidateSyntaxAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);

        _logger.LogWarning(
            "PythonScriptEngine: Python syntax validation requested but the engine is not available. (SCRIPT-006)");

        const string diagMessage = "Python scripting is not enabled in this deployment (FeatureManagement:EnablePythonScripting=false). " +
            "Install the 'pythonnet' NuGet package and enable the feature flag to use Python scripts.";
        IReadOnlyList<ScriptDiagnostic> diagnostics =
        [
            new ScriptDiagnostic(0, 0, diagMessage, DiagnosticSeverity.Error)
        ];

        return Task.FromResult(diagnostics);
    }
}
