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
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CRM.Tests.Unit.Factories;

/// <summary>
/// Unit tests for <see cref="ScriptEngineFactory"/> — 6 scenarios.
///
/// Factory selection algorithm (from implementation):
///   1. Find first engine where Language == requested language.
///   2. If none found AND requested language != JavaScript → warn and find JS engine.
///   3. engine ??= first registered engine (any language).
///   4. If still null → throw InvalidOperationException("No script engines are registered").
///   5. If engine.IsAvailable == false → throw InvalidOperationException.
/// </summary>
public class ScriptEngineFactoryTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static Mock<IScriptEngine> MakeEngine(ScriptLanguage language, bool available = true)
    {
        var mock = new Mock<IScriptEngine>();
        mock.Setup(e => e.Language).Returns(language);
        mock.Setup(e => e.IsAvailable).Returns(available);
        return mock;
    }

    private static ScriptEngineFactory BuildFactory(params Mock<IScriptEngine>[] engines)
    {
        return new ScriptEngineFactory(
            engines.Select(m => m.Object),
            NullLogger<ScriptEngineFactory>.Instance);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1 — Exact match: JavaScript requested and registered
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetEngine_ShouldReturnJavaScriptEngine_WhenLanguageIsJavaScript()
    {
        var jsEngine = MakeEngine(ScriptLanguage.JavaScript);
        var factory = BuildFactory(jsEngine);

        var result = factory.GetEngine(ScriptLanguage.JavaScript);

        result.Should().BeSameAs(jsEngine.Object);
        result.Language.Should().Be(ScriptLanguage.JavaScript);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2 — Exact match: Python requested and registered (available)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetEngine_ShouldReturnPythonEngine_WhenLanguageIsPython()
    {
        var pythonEngine = MakeEngine(ScriptLanguage.Python, available: true);
        var factory = BuildFactory(pythonEngine);

        var result = factory.GetEngine(ScriptLanguage.Python);

        result.Should().BeSameAs(pythonEngine.Object);
        result.Language.Should().Be(ScriptLanguage.Python);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3 — Fallback: Python not registered → JavaScript returned
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetEngine_ShouldFallbackToJavaScript_WhenPythonUnavailable()
    {
        // Python engine is NOT in the list ("unavailable" = not registered).
        // Factory finds no Python engine, logs a warning, and falls back to JS.
        var jsEngine = MakeEngine(ScriptLanguage.JavaScript);
        var factory = BuildFactory(jsEngine);

        var result = factory.GetEngine(ScriptLanguage.Python);

        result.Should().BeSameAs(jsEngine.Object);
        result.Language.Should().Be(ScriptLanguage.JavaScript);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4 — Empty engine list → throws InvalidOperationException
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetEngine_ShouldThrow_WhenNoEnginesRegistered()
    {
        var factory = new ScriptEngineFactory(
            Enumerable.Empty<IScriptEngine>(),
            NullLogger<ScriptEngineFactory>.Instance);

        var act = () => factory.GetEngine(ScriptLanguage.JavaScript);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*No script engines are registered*");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5 — Requested language (CSharp) not registered → first available engine returned
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetEngine_ShouldReturnFirstAvailable_WhenRequestedLanguageUnavailable()
    {
        // CSharp is not registered. language != JavaScript triggers the JS fallback.
        // The JS engine is found and returned.
        var jsEngine = MakeEngine(ScriptLanguage.JavaScript);
        var factory = BuildFactory(jsEngine);

        var result = factory.GetEngine(ScriptLanguage.CSharp);

        result.Should().BeSameAs(jsEngine.Object);
        result.Language.Should().Be(ScriptLanguage.JavaScript);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6 — Same engine instance returned on repeated calls for the same language
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetEngine_ShouldReturnSameInstance_WhenCalledTwice()
    {
        var jsEngine = MakeEngine(ScriptLanguage.JavaScript);
        var factory = BuildFactory(jsEngine);

        var first = factory.GetEngine(ScriptLanguage.JavaScript);
        var second = factory.GetEngine(ScriptLanguage.JavaScript);

        first.Should().BeSameAs(second);
    }
}
