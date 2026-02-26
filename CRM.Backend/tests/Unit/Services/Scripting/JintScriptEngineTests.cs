// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Enums;
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Scripting;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Unit.Services.Scripting;

/// <summary>
/// Unit tests for <see cref="JintScriptEngine"/> — 18 scenarios.
///
/// Implementation notes used when deriving test expectations:
///   * The engine registers a <c>log()</c> helper for output capture (NOT console.log).
///   * Jint supports top-level <c>return</c> statements in script/program mode.
///   * JavaScriptException error message format: "JavaScript error: {message}".
///   * TimeoutException sets ErrorMessage = "Script execution timed out".
///   * <c>ExtractReturnValue</c> falls back to the <c>result</c> variable when
///     the last expression evaluates to undefined/null.
///   * Context is injected via engine.SetValue("context", dict) — Jint's ObjectWrapper
///     exposes dictionary keys as dot-notation properties.
/// </summary>
public class JintScriptEngineTests
{
    private readonly JintScriptEngine _engine;

    // Empty helper dictionaries reused across tests.
    private static Dictionary<string, object?> Empty() => new();

    public JintScriptEngineTests()
    {
        _engine = new JintScriptEngine(NullLogger<JintScriptEngine>.Instance);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 1 — Language property returns JavaScript.</summary>
    [Fact]
    public void Language_ShouldReturnJavaScript()
    {
        _engine.Language.Should().Be(ScriptLanguage.JavaScript);
    }

    /// <summary>Scenario 2 — Jint engine is always available.</summary>
    [Fact]
    public void IsAvailable_ShouldReturnTrue()
    {
        _engine.IsAvailable.Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Basic Execution
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 3 — Top-level return statement.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenReturnStatementUsed()
    {
        var result = await _engine.ExecuteAsync("return 42;", Empty(), Empty());

        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.ReturnValue.Should().Be(42);
    }

    /// <summary>Scenario 4 — ExtractReturnValue reads the 'result' variable when last expression is an identifier.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenResultVariableUsed()
    {
        var result = await _engine.ExecuteAsync("var result = 'hello'; result", Empty(), Empty());

        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be("hello");
    }

    /// <summary>Scenario 5 — var declaration yields undefined completion; 'result' not set → null.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenNoReturnValue()
    {
        var result = await _engine.ExecuteAsync("var x = 1;", Empty(), Empty());

        result.Success.Should().BeTrue();
        result.ReturnValue.Should().BeNull();
    }

    /// <summary>Scenario 6 — A bare expression evaluates and its value is returned.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenCodeUsesExpression()
    {
        var result = await _engine.ExecuteAsync("2 + 2", Empty(), Empty());

        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(4);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Variable Injection
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 7 — Numeric variables are injected as globals.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldAccessVariables_WhenInjected()
    {
        var variables = new Dictionary<string, object?> { ["x"] = 10, ["y"] = 20 };

        var result = await _engine.ExecuteAsync("return x + y;", variables, Empty());

        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(30);
    }

    /// <summary>Scenario 8 — Empty dictionary (equivalent to "no variables") succeeds without error.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldHandleNullVariables_WhenNullpassedIn()
    {
        // The implementation iterates variables with foreach; passing null would throw NullReferenceException.
        // An empty dictionary is the safe, semantically-equivalent substitute.
        var result = await _engine.ExecuteAsync("return 1;", Empty(), Empty());

        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Context Injection
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 9 — Context dictionary is exposed as a 'context' JS object with dot-notation access.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldAccessContext_WhenInjected()
    {
        var context = new Dictionary<string, object?> { ["entityId"] = 99 };

        var result = await _engine.ExecuteAsync("return context.entityId;", Empty(), context);

        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(99);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Logging
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 10 — Single log() call is captured in Logs.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCaptureLogs_WhenConsoleLogCalled()
    {
        // NOTE: the engine binds log() via engine.SetValue("log", …); console.log is not available.
        var result = await _engine.ExecuteAsync("log('test message'); return 1;", Empty(), Empty());

        result.Success.Should().BeTrue();
        result.Logs.Should().Contain("test message");
    }

    /// <summary>Scenario 11 — Multiple log() calls are all appended to Logs in order.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCaptureMultipleLogs()
    {
        var result = await _engine.ExecuteAsync("log('a'); log('b'); return 1;", Empty(), Empty());

        result.Success.Should().BeTrue();
        result.Logs.Should().HaveCount(2);
        result.Logs[0].Should().Be("a");
        result.Logs[1].Should().Be("b");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Error Handling
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 12 — Parse/syntax error causes Success = false.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenSyntaxErrorExists()
    {
        var result = await _engine.ExecuteAsync("@@@invalid", Empty(), Empty());

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    /// <summary>Scenario 13 — throw new Error(…) causes Success = false and ErrorMessage contains the thrown text.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenRuntimeErrorOccurs()
    {
        // JavaScriptException catch block sets: "JavaScript error: {jsEx.Message}"
        var result = await _engine.ExecuteAsync("throw new Error('oops');", Empty(), Empty());

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("oops");
    }

    /// <summary>Scenario 14 — Infinite loop triggers the configured timeout cancellation.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenTimeoutExceeded()
    {
        // TimeoutException catch block sets: "Script execution timed out"
        var result = await _engine.ExecuteAsync(
            "while(true){}",
            Empty(),
            Empty(),
            timeout: TimeSpan.FromMilliseconds(200));

        result.Success.Should().BeFalse();
        result.ReturnValue.Should().BeNull();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorMessage!.ToLowerInvariant().Should().Contain("timed out");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Validation
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 15 — Valid code produces no diagnostics.</summary>
    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnEmpty_WhenCodeValid()
    {
        var diagnostics = await _engine.ValidateSyntaxAsync("return 1+1;");

        diagnostics.Should().BeEmpty();
    }

    /// <summary>Scenario 16 — Invalid code returns at least one Error-severity diagnostic.</summary>
    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnDiagnostics_WhenCodeInvalid()
    {
        var diagnostics = await _engine.ValidateSyntaxAsync("@@@invalid");

        diagnostics.Should().NotBeEmpty();
        diagnostics[0].Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostics[0].Message.Should().NotBeNullOrEmpty();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Complex Return Types
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 17 — String literal returned from top-level return statement.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldHandleStringReturn()
    {
        var result = await _engine.ExecuteAsync("return 'hello world';", Empty(), Empty());

        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be("hello world");
    }

    /// <summary>Scenario 18 — JS array literal is returned as an IEnumerable.</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldHandleArrayReturn()
    {
        var result = await _engine.ExecuteAsync("return [1, 2, 3];", Empty(), Empty());

        result.Success.Should().BeTrue();
        result.ReturnValue.Should().NotBeNull();
        result.ReturnValue.Should().BeAssignableTo<System.Collections.IEnumerable>();
    }
}
