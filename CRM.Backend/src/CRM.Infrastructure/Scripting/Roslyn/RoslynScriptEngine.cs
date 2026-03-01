// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using CRM.Core.Scripting;
using CRM.Infrastructure.Scripting.Security;

namespace CRM.Infrastructure.Scripting.Roslyn;

/// <summary>
/// Roslyn-based implementation of <see cref="ICompiledScriptEngine"/> for the
/// <see cref="ScriptRuntime.DotNet"/> runtime. Compiles C# scripts using
/// <see cref="CSharpScript"/>, caches artefact sentinels in Redis via
/// <see cref="ScriptArtefactStore"/>, and limits concurrency via a
/// <see cref="SemaphoreSlim"/> ceiling.
/// </summary>
public class RoslynScriptEngine : ICompiledScriptEngine
{
    private readonly ScriptArtefactStore _artefactStore;
    private readonly ILogger<RoslynScriptEngine> _logger;
    private readonly SemaphoreSlim _concurrencyLimiter;
    private const int MaxConcurrentExecutions = 10;

    public ScriptRuntime Runtime => ScriptRuntime.DotNet;

    public RoslynScriptEngine(ScriptArtefactStore artefactStore, ILogger<RoslynScriptEngine> logger)
    {
        _artefactStore = artefactStore;
        _logger = logger;
        _concurrencyLimiter = new SemaphoreSlim(MaxConcurrentExecutions, MaxConcurrentExecutions);
    }

    /// <inheritdoc/>
    public async Task<CompilationResult> CompileAsync(
        ScriptDefinition definition,
        CompilationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var contentHash = ComputeHash(definition.Source);

        // Check artefact cache unless force-recompile is requested
        if (options?.ForceRecompile != true)
        {
            var cached = await _artefactStore.GetAsync(contentHash, cancellationToken);
            if (cached != null)
            {
                return new CompilationResult
                {
                    CompiledRef = new CompiledScriptRef(definition.Id, contentHash, null, DateTime.UtcNow),
                    ContentHash = contentHash,
                };
            }
        }

        try
        {
            var scriptOptions = ScriptOptions.Default
                .WithMetadataResolver(new SecureReferenceResolver())
                .WithEmitDebugInformation(false);

            var script = CSharpScript.Create(definition.Source, scriptOptions);
            var diagnostics = script.Compile(cancellationToken);

            var messages = diagnostics.Select(d => new DiagnosticMessage(
                MapSeverity(d.Severity), d.Id, d.GetMessage(),
                d.Location.GetLineSpan().StartLinePosition.Line + 1,
                d.Location.GetLineSpan().StartLinePosition.Character + 1)).ToList();

            if (messages.Any(m => m.Severity == DiagnosticSeverity.Error))
            {
                return new CompilationResult
                {
                    CompiledRef = null,
                    ContentHash = contentHash,
                    Diagnostics = messages,
                };
            }

            // Store sentinel in Redis so repeated compilations of the same source are skipped
            await _artefactStore.SetAsync(
                contentHash,
                Encoding.UTF8.GetBytes("compiled"),
                null,
                cancellationToken);

            return new CompilationResult
            {
                CompiledRef = new CompiledScriptRef(definition.Id, contentHash, null, DateTime.UtcNow),
                ContentHash = contentHash,
                Diagnostics = messages,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compilation failed for script {Id}.", definition.Id);
            return new CompilationResult
            {
                CompiledRef = null,
                ContentHash = contentHash,
                Diagnostics = new[]
                {
                    new DiagnosticMessage(DiagnosticSeverity.Error, "COMPILE_EX", ex.Message, null, null),
                },
            };
        }
    }

    /// <inheritdoc/>
    public async Task<ExecutionResult<TOut>> ExecuteAsync<TIn, TOut>(
        CompiledScriptRef compiledRef,
        IScriptContext<TIn> context,
        ExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await _concurrencyLimiter.WaitAsync(cancellationToken);
        var start = DateTime.UtcNow;
        try
        {
            // Full execution implementation would load from cache and run with typed globals.
            // This stub returns a success result to allow DI wiring and unit-test verification.
            await Task.Delay(0, cancellationToken);
            return new ExecutionResult<TOut>
            {
                Success = true,
                Output = default,
                Duration = DateTime.UtcNow - start,
                TraceId = Guid.NewGuid().ToString("N")[..16],
            };
        }
        catch (Exception ex)
        {
            return new ExecutionResult<TOut>
            {
                Success = false,
                Error = ex.Message,
                Duration = DateTime.UtcNow - start,
            };
        }
        finally
        {
            _concurrencyLimiter.Release();
        }
    }

    /// <inheritdoc/>
    public Task<ExecutionResult<TOut>> RunAsync<TIn, TOut>(
        string scriptId,
        TIn input,
        ExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "Use CompileAsync + ExecuteAsync for the full compile-then-execute workflow.");

    private static string ComputeHash(string source)
    {
        var bytes = Encoding.UTF8.GetBytes(source);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Maps Roslyn's <see cref="Microsoft.CodeAnalysis.DiagnosticSeverity"/>
    /// to the CRM scripting <see cref="DiagnosticSeverity"/> enum.
    /// Roslyn uses Info=1, Warning=2, Error=3; our enum uses Info=0, Warning=1, Error=2.
    /// </summary>
    private static DiagnosticSeverity MapSeverity(Microsoft.CodeAnalysis.DiagnosticSeverity roslyn)
        => roslyn switch
        {
            Microsoft.CodeAnalysis.DiagnosticSeverity.Error => DiagnosticSeverity.Error,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning => DiagnosticSeverity.Warning,
            _ => DiagnosticSeverity.Info,
        };
}
