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
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CRM.Backend.Tests.Unit.Services.Scripting;

/// <summary>
/// Unit tests for <see cref="JintScriptEngine"/> JavaScript execution engine.
/// </summary>
public class JintScriptEngineTests
{
    private readonly ILogger<JintScriptEngine> _mockLogger;
    private readonly JintScriptEngine _engine;

    public JintScriptEngineTests()
    {
        _mockLogger = Substitute.For<ILogger<JintScriptEngine>>();
        _engine = new JintScriptEngine(_mockLogger);
    }

    #region Property Tests

    [Fact]
    public void Language_ShouldReturnJavaScript_Always()
    {
        // Act
        var language = _engine.Language;

        // Assert
        language.Should().Be(ScriptLanguage.JavaScript);
    }

    [Fact]
    public void IsAvailable_ShouldReturnTrue_Always()
    {
        // Act
        var isAvailable = _engine.IsAvailable;

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Execution Tests - Simple Return Value

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSimpleValue_WhenCodeReturnsDirectly()
    {
        // Arrange
        var code = "42";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(42);
        result.ErrorMessage.Should().BeNull();
        result.Logs.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnString_WhenCodeReturnsString()
    {
        // Arrange
        var code = "\"Hello, World!\"";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be("Hello, World!");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnBoolean_WhenCodeReturnsBoolean()
    {
        // Arrange
        var code = "true";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(true);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnObject_WhenCodeReturnsObject()
    {
        // Arrange
        var code = "({name: 'John', age: 30})";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().NotBeNull();
    }

    #endregion

    #region Execution Tests - Log Output

    [Fact]
    public async Task ExecuteAsync_ShouldCaptureLogs_WhenCodeCallsLog()
    {
        // Arrange
        var code = @"
            log('First message');
            log('Second message');
            42
        ";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.Logs.Should().HaveCount(2);
        result.Logs[0].Should().Be("First message");
        result.Logs[1].Should().Be("Second message");
        result.ReturnValue.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCaptureLogOfNull_WhenCodeLogsNull()
    {
        // Arrange
        var code = "log(null); 42";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.Logs.Should().HaveCount(1);
        result.Logs[0].Should().Be("null");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCaptureLogOfObject_WhenCodeLogsObject()
    {
        // Arrange
        var code = "log({test: true}); 42";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.Logs.Should().HaveCount(1);
        result.Logs[0].Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Execution Tests - Variables Injection

    [Fact]
    public async Task ExecuteAsync_ShouldInjectVariables_WhenVariablesProvided()
    {
        // Arrange
        var code = "x + y";
        var variables = new Dictionary<string, object?>
        {
            { "x", 10 },
            { "y", 20 }
        };
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(30);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldInjectStringVariables_WhenStringVariablesProvided()
    {
        // Arrange
        var code = "name.toUpperCase()";
        var variables = new Dictionary<string, object?>
        {
            { "name", "john" }
        };
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be("JOHN");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldInjectMultipleVariables_WhenMultipleVariablesProvided()
    {
        // Arrange
        var code = "({sum: a + b, product: a * b})";
        var variables = new Dictionary<string, object?>
        {
            { "a", 5 },
            { "b", 3 }
        };
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldInjectNullVariable_WhenVariableIsNull()
    {
        // Arrange
        var code = "value === null";
        var variables = new Dictionary<string, object?>
        {
            { "value", null }
        };
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(true);
    }

    #endregion

    #region Execution Tests - Context Injection

    [Fact]
    public async Task ExecuteAsync_ShouldInjectContext_WhenContextProvided()
    {
        // Arrange
        var code = "context.userId";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>
        {
            { "userId", 42 }
        };

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAccessComplexContext_WhenContextWithObjectsProvided()
    {
        // Arrange
        var code = "context.user.name";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>
        {
            { "user", new { name = "Alice", role = "admin" } }
        };

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be("Alice");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHaveEmptyContext_WhenContextNotProvided()
    {
        // Arrange
        var code = "Object.keys(context).length";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(0);
    }

    #endregion

    #region Execution Tests - Timeout

    [Fact]
    public async Task ExecuteAsync_ShouldFail_WhenExecutionTimesOut()
    {
        // Arrange
        var code = "while(true) {}"; // Infinite loop
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();
        var shortTimeout = TimeSpan.FromMilliseconds(100);

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context, shortTimeout);

        // Assert
        result.Success.Should().BeFalse();
        result.ReturnValue.Should().BeNull();
        result.ErrorMessage.Should().Contain("timed out");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCompleteInTime_WhenExecutionWithinTimeout()
    {
        // Arrange
        var code = "1 + 1";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();
        var reasonablTimeout = TimeSpan.FromSeconds(10);

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context, reasonablTimeout);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(2);
    }

    #endregion

    #region Execution Tests - JavaScript Errors

    [Fact]
    public async Task ExecuteAsync_ShouldFail_WhenCodeHasSyntaxError()
    {
        // Arrange
        var code = "var x = {{invalid}}"; // Invalid syntax
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeFalse();
        result.ReturnValue.Should().BeNull();
        result.ErrorMessage.Should().Contain("error");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFail_WhenCodeThrowsError()
    {
        // Arrange
        var code = "throw new Error('Custom error message')";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().Contain("error");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFail_WhenCodeReferencesUndefinedVariable()
    {
        // Arrange
        var code = "undefinedVariable.property";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Execution Tests - Result Variable Fallback

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResultVariable_WhenDirectReturnIsUndefined()
    {
        // Arrange
        var code = "var result = 99; result;";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(99);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenNoReturnAndNoResultVariable()
    {
        // Arrange
        var code = "42; undefined;";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseResultVariable_WhenExpressionEvaluatesToUndefined()
    {
        // Arrange
        var code = @"
            var result = {status: 'success', value: 123};
            (function() { var x = 1; })();
        ";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().NotBeNull();
    }

    #endregion

    #region Execution Tests - Cancellation

    [Fact]
    public async Task ExecuteAsync_ShouldBeRespected_WhenCancellationTokenCancelled()
    {
        // Arrange
        var code = "var x = 0; for(var i = 0; i < 1000000000; i++) { x = i; }; x";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context, null, cts.Token);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cancelled");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldComplete_WhenCancellationTokenNotCancelled()
    {
        // Arrange
        var code = "1 + 1";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();
        var cts = new CancellationTokenSource();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context, null, cts.Token);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(2);
    }

    #endregion

    #region Execution Tests - ExecutionTime Tracking

    [Fact]
    public async Task ExecuteAsync_ShouldTrackExecutionTime_WhenCodeExecutes()
    {
        // Arrange
        var code = "1 + 1";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ExecutionTime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenCodeIsNull()
    {
        // Arrange
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        Func<Task> act = async () => await _engine.ExecuteAsync(null!, variables, context);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Validation Tests - Valid Syntax

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnEmptyList_WhenCodeIsValid()
    {
        // Arrange
        var code = "var x = 42; console.log(x);";

        // Act
        var diagnostics = await _engine.ValidateSyntaxAsync(code);

        // Assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnEmptyList_WhenSimpleExpressionIsValid()
    {
        // Arrange
        var code = "1 + 2";

        // Act
        var diagnostics = await _engine.ValidateSyntaxAsync(code);

        // Assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnEmptyList_WhenComplexCodeIsValid()
    {
        // Arrange
        var code = @"
            function add(a, b) {
                return a + b;
            }
            var result = add(10, 20);
        ";

        // Act
        var diagnostics = await _engine.ValidateSyntaxAsync(code);

        // Assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnEmptyList_WhenArrowFunctionIsValid()
    {
        // Arrange
        var code = "const add = (a, b) => a + b;";

        // Act
        var diagnostics = await _engine.ValidateSyntaxAsync(code);

        // Assert
        diagnostics.Should().BeEmpty();
    }

    #endregion

    #region Validation Tests - Invalid Syntax

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnDiagnostics_WhenCodeHasSyntaxError()
    {
        // Arrange
        var code = "var x = {{invalid}}";

        // Act
        var diagnostics = await _engine.ValidateSyntaxAsync(code);

        // Assert
        diagnostics.Should().NotBeEmpty();
        diagnostics.Should().ContainSingle();
        diagnostics[0].Severity.Should().Be(DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnDiagnostics_WhenCodeHasMissingBracket()
    {
        // Arrange
        var code = "function test() { return 42;";

        // Act
        var diagnostics = await _engine.ValidateSyntaxAsync(code);

        // Assert
        diagnostics.Should().NotBeEmpty();
        diagnostics.Should().ContainSingle();
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnDiagnosticWithLineInfo_WhenErrorOccurs()
    {
        // Arrange
        var code = "var x = ;;";

        // Act
        var diagnostics = await _engine.ValidateSyntaxAsync(code);

        // Assert
        diagnostics.Should().NotBeEmpty();
        var diagnostic = diagnostics[0];
        diagnostic.Line.Should().BeGreaterThanOrEqualTo(0);
        diagnostic.Column.Should().BeGreaterThanOrEqualTo(0);
        diagnostic.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldThrowArgumentNullException_WhenCodeIsNull()
    {
        // Act
        Func<Task> act = async () => await _engine.ValidateSyntaxAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Complex Integration Tests

    [Fact]
    public async Task ExecuteAsync_ShouldCombineVariablesContextAndLogs_InComplexScenario()
    {
        // Arrange
        var code = @"
            log('Processing user: ' + context.username);
            var total = items.reduce((sum, item) => {
                log('Adding ' + item);
                return sum + item;
            }, 0);
            total
        ";
        var variables = new Dictionary<string, object?>
        {
            { "items", new[] { 10, 20, 30 } }
        };
        var context = new Dictionary<string, object?>
        {
            { "username", "alice" }
        };

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(60);
        result.Logs.Should().NotBeEmpty();
        result.Logs[0].Should().Contain("Processing user");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowArrayManipulation_WhenArrayVariableProvided()
    {
        // Arrange
        var code = "numbers.map(n => n * 2).reduce((a, b) => a + b, 0)";
        var variables = new Dictionary<string, object?>
        {
            { "numbers", new[] { 1, 2, 3, 4, 5 } }
        };
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(30); // (1*2 + 2*2 + 3*2 + 4*2 + 5*2) = 30
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleObjectTransformation_WhenComplexCodeProvided()
    {
        // Arrange
        var code = @"
            ({
                doubled: value * 2,
                squared: value * value,
                isEven: value % 2 === 0
            })
        ";
        var variables = new Dictionary<string, object?>
        {
            { "value", 5 }
        };
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().NotBeNull();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ExecuteAsync_ShouldReturnZero_WhenCodeReturnsZero()
    {
        // Arrange
        var code = "0";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFalse_WhenCodeReturnsFalse()
    {
        // Arrange
        var code = "false";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(false);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyString_WhenCodeReturnsEmptyString()
    {
        // Arrange
        var code = "\"\"";
        var variables = new Dictionary<string, object?>();
        var context = new Dictionary<string, object?>();

        // Act
        var result = await _engine.ExecuteAsync(code, variables, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be("");
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnEmptyList_WhenCodeIsEmptyString()
    {
        // Arrange
        var code = "";

        // Act
        var diagnostics = await _engine.ValidateSyntaxAsync(code);

        // Assert
        diagnostics.Should().BeEmpty();
    }

    #endregion
}
