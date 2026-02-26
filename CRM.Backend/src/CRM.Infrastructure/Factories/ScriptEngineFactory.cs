// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Enums;
using CRM.Core.Interfaces.Scripting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Factories;

/// <summary>
/// Resolves script engines by language with availability checks.
/// </summary>
public class ScriptEngineFactory
{
    private readonly IEnumerable<IScriptEngine> _scriptEngines;
    private readonly ILogger<ScriptEngineFactory> _logger;

    public ScriptEngineFactory(IEnumerable<IScriptEngine> scriptEngines, ILogger<ScriptEngineFactory> logger)
    {
        _scriptEngines = scriptEngines;
        _logger = logger;
    }

    /// <summary>Get an engine for the requested language; falls back to JavaScript when possible.</summary>
    /// <exception cref="InvalidOperationException">Thrown when no suitable engine is available.</exception>
    public IScriptEngine GetEngine(ScriptLanguage language)
    {
        var engine = _scriptEngines.FirstOrDefault(e => e.Language == language);

        if (engine == null && language != ScriptLanguage.JavaScript)
        {
            _logger.LogWarning("No script engine registered for {Language}; falling back to JavaScript", language);
            engine = _scriptEngines.FirstOrDefault(e => e.Language == ScriptLanguage.JavaScript);
        }

        engine ??= _scriptEngines.FirstOrDefault();

        if (engine == null)
        {
            throw new InvalidOperationException("No script engines are registered");
        }

        if (!engine.IsAvailable)
        {
            throw new InvalidOperationException($"Script engine for {engine.Language} is not available");
        }

        return engine;
    }
}
