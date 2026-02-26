// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using System.Text.Json;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Scripting;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Scripting;

/// <summary>
/// Jint-based JavaScript engine implementation of <see cref="IScriptEngine"/>.
/// </summary>
public class JintScriptEngine : IScriptEngine
{
    private readonly ILogger<JintScriptEngine> _logger;
    private const int DefaultMemoryLimitBytes = 16 * 1024 * 1024; // 16 MB
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    public JintScriptEngine(ILogger<JintScriptEngine> logger)
    {
        _logger = logger;
    }

    public ScriptLanguage Language => ScriptLanguage.JavaScript;

    public bool IsAvailable => true;

    public async Task<ScriptExecutionResult> ExecuteAsync(
        string code,
        Dictionary<string, object?> variables,
        Dictionary<string, object?> context,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);

        var logs = new List<string>();
        var stopwatch = Stopwatch.StartNew();
        var effectiveTimeout = timeout ?? DefaultTimeout;

        try
        {
            var engine = new Engine(options =>
            {
                options.TimeoutInterval(effectiveTimeout);
                options.LimitMemory(DefaultMemoryLimitBytes);
                options.Strict();
            });

            InjectVariables(engine, variables);
            InjectContext(engine, context);
            engine.SetValue("log", new Action<object?>(msg => logs.Add(msg?.ToString() ?? "null")));

            var jsResult = await Task.Run(() => engine.Evaluate(code), cancellationToken);
            var returnValue = ExtractReturnValue(engine, jsResult);

            stopwatch.Stop();
            return new ScriptExecutionResult(true, returnValue, logs, null, stopwatch.Elapsed);
        }
        catch (TimeoutException tex)
        {
            _logger.LogWarning(tex, "JavaScript execution timed out after {Timeout}s", effectiveTimeout.TotalSeconds);
            stopwatch.Stop();
            return new ScriptExecutionResult(false, null, logs, "Script execution timed out", stopwatch.Elapsed);
        }
        catch (JavaScriptException jsEx)
        {
            _logger.LogWarning(jsEx, "JavaScript error: {Error}", jsEx.Message);
            stopwatch.Stop();
            return new ScriptExecutionResult(false, null, logs, $"JavaScript error: {jsEx.Message}", stopwatch.Elapsed);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return new ScriptExecutionResult(false, null, logs, "Script execution was cancelled", stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JavaScript execution failed");
            stopwatch.Stop();
            return new ScriptExecutionResult(false, null, logs, $"Script execution failed: {ex.Message}", stopwatch.Elapsed);
        }
    }

    public Task<IReadOnlyList<ScriptDiagnostic>> ValidateSyntaxAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);

        try
        {
            var engine = new Engine(options => options.Strict());
            engine.Execute(code);
            return Task.FromResult<IReadOnlyList<ScriptDiagnostic>>(Array.Empty<ScriptDiagnostic>());
        }
        catch (JavaScriptException ex)
        {
            var diagnostic = new ScriptDiagnostic(0, 0, ex.Message, DiagnosticSeverity.Error);
            return Task.FromResult<IReadOnlyList<ScriptDiagnostic>>(new[] { diagnostic });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error while validating JavaScript syntax");
            var diagnostic = new ScriptDiagnostic(0, 0, ex.Message, DiagnosticSeverity.Error);
            return Task.FromResult<IReadOnlyList<ScriptDiagnostic>>(new[] { diagnostic });
        }
    }

    private static void InjectVariables(Engine engine, IReadOnlyDictionary<string, object?> variables)
    {
        foreach (var kvp in variables)
        {
            engine.SetValue(kvp.Key, NormalizeValue(kvp.Value));
        }
    }

    private static void InjectContext(Engine engine, IReadOnlyDictionary<string, object?> context)
    {
        engine.SetValue("context", context);
    }

    private static object? ExtractReturnValue(Engine engine, JsValue jsResult)
    {
        if (!jsResult.IsNull() && !jsResult.IsUndefined())
        {
            return jsResult.ToObject();
        }

        var resultVar = engine.GetValue("result");
        return resultVar.IsNull() || resultVar.IsUndefined() ? null : resultVar.ToObject();
    }

    private static object? NormalizeValue(object? value)
    {
        return value switch
        {
            JsonElement element => ConvertJsonElement(element),
            _ => value
        };
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToArray(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
            _ => element.ToString()
        };
    }
}
