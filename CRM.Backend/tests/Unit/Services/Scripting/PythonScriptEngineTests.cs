// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Scripting;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Unit.Services.Scripting;

/// <summary>
/// Unit tests for <see cref="PythonScriptEngine"/> stub behaviour (SCRIPT-006).
///
/// The engine is registered via <c>AddScriptingEngines()</c> but <c>IsAvailable</c>
/// returns <c>false</c> until pythonnet is integrated. These tests verify that the
/// stub behaves gracefully rather than throwing unhandled exceptions.
///
/// When Python.NET support is wired in, update <c>IsAvailable</c> to return <c>true</c>
/// when the runtime is present and enable the corresponding tests in
/// <see cref="CRM.Tests.Unit.Scripting.PythonScriptEngineFeatureTests"/>.
/// </summary>
public class PythonScriptEngineTests
{
    private readonly PythonScriptEngine _engine;

    private static Dictionary<string, object?> Empty() => new();

    public PythonScriptEngineTests()
    {
        _engine = new PythonScriptEngine(NullLogger<PythonScriptEngine>.Instance);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Language / availability
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 1 — Language property returns Python.</summary>
    [Fact]
    public void Language_ShouldReturnPython()
    {
        _engine.Language.Should().Be(ScriptLanguage.Python);
    }

    /// <summary>Scenario 2 — IsAvailable is false (Python.NET not wired).</summary>
    [Fact]
    public void IsAvailable_ShouldReturnFalse_WhenPythonNetNotIntegrated()
    {
        _engine.IsAvailable.Should().BeFalse(
            "pythonnet is not yet integrated; EnablePythonScripting must remain false");
    }

    /// <summary>Scenario 3 — Engine implements IScriptEngine interface.</summary>
    [Fact]
    public void PythonScriptEngine_Should_ImplementIScriptEngine()
    {
        _engine.Should().BeAssignableTo<IScriptEngine>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ExecuteAsync — graceful failure when unavailable
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 4 — ExecuteAsync returns failure result, not an exception.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenEngineIsUnavailable()
    {
        var result = await _engine.ExecuteAsync("print('hello')", Empty(), Empty());

        result.Success.Should().BeFalse();
        result.ReturnValue.Should().BeNull();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>Scenario 5 — ExecuteAsync error message mentions the feature flag.</summary>
    [Fact]
    public async Task ExecuteAsync_ErrorMessage_ShouldMentionFeatureFlag()
    {
        var result = await _engine.ExecuteAsync("x = 1", Empty(), Empty());

        result.ErrorMessage.Should().ContainEquivalentOf("EnablePythonScripting",
            because: "users need to know which flag to enable");
    }

    /// <summary>Scenario 6 — ExecuteAsync with empty code string returns failure (not null-ref).</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WithEmptyCode()
    {
        var result = await _engine.ExecuteAsync(string.Empty, Empty(), Empty());

        result.Success.Should().BeFalse();
    }

    /// <summary>Scenario 7 — ExecuteAsync with null code throws ArgumentNullException.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenCodeIsNull()
    {
        var act = () => _engine.ExecuteAsync(null!, Empty(), Empty());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>Scenario 8 — Cancellation token does not cause a throw (already unavailable fast-path).</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCompleteGracefully_WhenCancellationTokenIsAlreadyCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // The stub does synchronous work — it should still return a failure result
        // rather than throw OperationCanceledException.
        var result = await _engine.ExecuteAsync("pass", Empty(), Empty(), cancellationToken: cts.Token);

        result.Success.Should().BeFalse();
    }

    /// <summary>Scenario 9 — ExecutionTime is zero for the unavailable stub.</summary>
    [Fact]
    public async Task ExecuteAsync_ExecutionTime_ShouldBeZero_WhenEngineIsUnavailable()
    {
        var result = await _engine.ExecuteAsync("y = 2", Empty(), Empty());

        result.ExecutionTime.Should().Be(TimeSpan.Zero);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ValidateSyntaxAsync — returns diagnostic instead of exception
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 10 — ValidateSyntaxAsync returns at least one diagnostic.</summary>
    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnDiagnostic_WhenEngineIsUnavailable()
    {
        var diagnostics = await _engine.ValidateSyntaxAsync("def foo(): pass");

        diagnostics.Should().NotBeEmpty();
    }

    /// <summary>Scenario 11 — Diagnostic severity is Error (not Warning).</summary>
    [Fact]
    public async Task ValidateSyntaxAsync_Diagnostic_ShouldHaveErrorSeverity()
    {
        var diagnostics = await _engine.ValidateSyntaxAsync("def foo(): pass");

        diagnostics.Should().ContainSingle(d => d.Severity == DiagnosticSeverity.Error);
    }

    /// <summary>Scenario 12 — Null code in ValidateSyntaxAsync throws ArgumentNullException.</summary>
    [Fact]
    public async Task ValidateSyntaxAsync_ShouldThrow_WhenCodeIsNull()
    {
        var act = () => _engine.ValidateSyntaxAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
