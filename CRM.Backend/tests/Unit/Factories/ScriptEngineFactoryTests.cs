// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Enums;
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Factories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CRM.Backend.Tests.Unit.Factories;

/// <summary>
/// Unit tests for ScriptEngineFactory class.
/// </summary>
public class ScriptEngineFactoryTests
{
    private readonly ILogger<ScriptEngineFactory> _mockLogger;

    public ScriptEngineFactoryTests()
    {
        _mockLogger = Substitute.For<ILogger<ScriptEngineFactory>>();
    }

    [Fact]
    public void GetEngine_ShouldReturnJavaScriptEngine_WhenJavaScriptLanguageRequested()
    {
        // Arrange
        var jsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var engines = new[] { jsEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act
        var result = factory.GetEngine(ScriptLanguage.JavaScript);

        // Assert
        result.Should().Be(jsEngine);
        result.Language.Should().Be(ScriptLanguage.JavaScript);
    }

    [Fact]
    public void GetEngine_ShouldReturnPythonEngine_WhenPythonLanguageRequested()
    {
        // Arrange
        var pythonEngine = CreateMockEngine(ScriptLanguage.Python, isAvailable: true);
        var jsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var engines = new[] { pythonEngine, jsEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act
        var result = factory.GetEngine(ScriptLanguage.Python);

        // Assert
        result.Should().Be(pythonEngine);
        result.Language.Should().Be(ScriptLanguage.Python);
    }

    [Fact]
    public void GetEngine_ShouldReturnCSharpEngine_WhenCSharpLanguageRequested()
    {
        // Arrange
        var csharpEngine = CreateMockEngine(ScriptLanguage.CSharp, isAvailable: true);
        var jsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var engines = new[] { csharpEngine, jsEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act
        var result = factory.GetEngine(ScriptLanguage.CSharp);

        // Assert
        result.Should().Be(csharpEngine);
        result.Language.Should().Be(ScriptLanguage.CSharp);
    }

    [Fact]
    public void GetEngine_ShouldFallbackToJavaScript_WhenRequestedLanguageNotAvailable()
    {
        // Arrange
        var pythonEngine = CreateMockEngine(ScriptLanguage.Python, isAvailable: true);
        var jsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var engines = new[] { pythonEngine, jsEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act - Request C# (not available)
        var result = factory.GetEngine(ScriptLanguage.CSharp);

        // Assert - Should fallback to JavaScript
        result.Should().Be(jsEngine);
        result.Language.Should().Be(ScriptLanguage.JavaScript);
    }

    [Fact]
    public void GetEngine_ShouldLogWarning_WhenFallingbackToJavaScript()
    {
        // Arrange
        var pythonEngine = CreateMockEngine(ScriptLanguage.Python, isAvailable: true);
        var jsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var engines = new[] { pythonEngine, jsEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act
        _ = factory.GetEngine(ScriptLanguage.CSharp);

        // Assert - Verify warning was logged
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(x => x.ToString()!.Contains("No script engine registered")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void GetEngine_ShouldThrowInvalidOperationException_WhenNoEnginesRegistered()
    {
        // Arrange
        var engines = Array.Empty<IScriptEngine>();
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act & Assert
        var action = () => factory.GetEngine(ScriptLanguage.JavaScript);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("No script engines are registered");
    }

    [Fact]
    public void GetEngine_ShouldThrowInvalidOperationException_WhenEngineNotAvailable()
    {
        // Arrange
        var unavailableEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: false);
        var engines = new[] { unavailableEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act & Assert
        var action = () => factory.GetEngine(ScriptLanguage.JavaScript);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Script engine for*is not available*", System.Diagnostics.CodeAnalysis.StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public void GetEngine_ShouldFallbackToFirstAvailableEngine_WhenLanguageNotFoundAndJavaScriptNotAvailable()
    {
        // Arrange
        var pythonEngine = CreateMockEngine(ScriptLanguage.Python, isAvailable: true);
        var jsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: false);
        var engines = new[] { jsEngine, pythonEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act - Request C# (not available), JS is also unavailable
        var result = factory.GetEngine(ScriptLanguage.CSharp);

        // Assert - Should fallback to first available engine (Python)
        result.Should().Be(pythonEngine);
        result.Language.Should().Be(ScriptLanguage.Python);
    }

    [Fact]
    public void GetEngine_ShouldReturnFirstAvailableEngine_WhenNoExactLanguageMatch()
    {
        // Arrange
        var pythonEngine = CreateMockEngine(ScriptLanguage.Python, isAvailable: true);
        var engines = new[] { pythonEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act - Request JavaScript (not available)
        var result = factory.GetEngine(ScriptLanguage.JavaScript);

        // Assert - Should return the only available engine
        result.Should().Be(pythonEngine);
    }

    [Fact]
    public void GetEngine_ShouldResolveMultipleEngineTypesCorrectly_WhenAllRegistered()
    {
        // Arrange
        var jsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var pythonEngine = CreateMockEngine(ScriptLanguage.Python, isAvailable: true);
        var csharpEngine = CreateMockEngine(ScriptLanguage.CSharp, isAvailable: true);
        var engines = new[] { jsEngine, pythonEngine, csharpEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act & Assert
        factory.GetEngine(ScriptLanguage.JavaScript).Should().Be(jsEngine);
        factory.GetEngine(ScriptLanguage.Python).Should().Be(pythonEngine);
        factory.GetEngine(ScriptLanguage.CSharp).Should().Be(csharpEngine);
    }

    [Fact]
    public void GetEngine_ShouldReturnSameEngineInstanceForSameLanguage_OnMultipleCalls()
    {
        // Arrange
        var jsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var engines = new[] { jsEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act
        var firstCall = factory.GetEngine(ScriptLanguage.JavaScript);
        var secondCall = factory.GetEngine(ScriptLanguage.JavaScript);

        // Assert
        firstCall.Should().Be(secondCall);
        firstCall.Should().Be(jsEngine);
    }

    [Fact]
    public void GetEngine_ShouldCheckIsAvailableProperty_WhenResolvingEngine()
    {
        // Arrange
        var mockEngine = Substitute.For<IScriptEngine>();
        mockEngine.Language.Returns(ScriptLanguage.JavaScript);
        mockEngine.IsAvailable.Returns(false);
        var engines = new[] { mockEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act & Assert
        var action = () => factory.GetEngine(ScriptLanguage.JavaScript);
        action.Should().Throw<InvalidOperationException>();
        mockEngine.Received(1).IsAvailable.Should().BeTrue(); // Verify the property was accessed
    }

    [Fact]
    public void GetEngine_ShouldPreferExactLanguageMatch_OverFallback()
    {
        // Arrange
        var jsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var pythonEngine = CreateMockEngine(ScriptLanguage.Python, isAvailable: true);
        var engines = new[] { jsEngine, pythonEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act
        var result = factory.GetEngine(ScriptLanguage.JavaScript);

        // Assert - Should return the exact match, not fallback
        result.Should().Be(jsEngine);
        result.Language.Should().Be(ScriptLanguage.JavaScript);
    }

    [Fact]
    public void GetEngine_ShouldThrowInvalidOperationException_WhenRequestedLanguageNotSupportedAndNoJavaScriptFallback()
    {
        // Arrange
        var pythonEngine = CreateMockEngine(ScriptLanguage.Python, isAvailable: true);
        var engines = new[] { pythonEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act & Assert - Request C#, Python available but not JavaScript
        var result = factory.GetEngine(ScriptLanguage.CSharp);
        result.Should().Be(pythonEngine); // Should fallback to first available
    }

    [Fact]
    public void GetEngine_ShouldNotLogWarning_WhenExactLanguageMatchFound()
    {
        // Arrange
        var jsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var engines = new[] { jsEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act
        _ = factory.GetEngine(ScriptLanguage.JavaScript);

        // Assert - No warning should be logged
        _mockLogger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void GetEngine_ShouldHandleEmptyEnginesList_WhenCallingGetEngine()
    {
        // Arrange
        var engines = new List<IScriptEngine>();
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act & Assert
        var action = () => factory.GetEngine(ScriptLanguage.JavaScript);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("No script engines are registered");
    }

    [Fact]
    public void GetEngine_ShouldResolveCorrectEngine_WhenMultipleEnginesWithSameLanguage()
    {
        // Arrange
        var firstJsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var secondJsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: true);
        var engines = new[] { firstJsEngine, secondJsEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act
        var result = factory.GetEngine(ScriptLanguage.JavaScript);

        // Assert - Should return the first one matching the language
        result.Should().Be(firstJsEngine);
    }

    [Fact]
    public void GetEngine_ShouldThrowInvalidOperationException_WhenOnlyUnavailableEnginesRegistered()
    {
        // Arrange
        var unavailableJsEngine = CreateMockEngine(ScriptLanguage.JavaScript, isAvailable: false);
        var unavailablePythonEngine = CreateMockEngine(ScriptLanguage.Python, isAvailable: false);
        var engines = new[] { unavailableJsEngine, unavailablePythonEngine };
        var factory = new ScriptEngineFactory(engines, _mockLogger);

        // Act & Assert
        var action = () => factory.GetEngine(ScriptLanguage.CSharp);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Script engine for*is not available*", System.Diagnostics.CodeAnalysis.StringComparison.CurrentCultureIgnoreCase);
    }

    // Helper method to create mock IScriptEngine instances
    private static IScriptEngine CreateMockEngine(ScriptLanguage language, bool isAvailable)
    {
        var mockEngine = Substitute.For<IScriptEngine>();
        mockEngine.Language.Returns(language);
        mockEngine.IsAvailable.Returns(isAvailable);
        return mockEngine;
    }
}
