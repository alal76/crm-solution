// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1206 // C# 11 'required' modifier is not supported by this StyleCop version
namespace CRM.Core.Scripting;

/// <summary>
/// Core scripting engine abstraction. Supports multi-runtime (Roslyn/.NET, TypeScript/V8).
/// Implementations: RoslynScriptEngine, TypeScriptScriptEngine.
/// </summary>
/// <remarks>
/// This interface is distinct from <see cref="CRM.Core.Interfaces.Scripting.IScriptEngine"/>,
/// which handles simple code-string execution. This contract governs the full
/// compile → cache → typed-execute lifecycle introduced in SARCH-004.
/// </remarks>
public interface ICompiledScriptEngine
{
    /// <summary>Compiles a script definition, runs security analysis, caches artefact.</summary>
    Task<CompilationResult> CompileAsync(
        ScriptDefinition definition,
        CompilationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Executes a previously compiled script with the given input context.</summary>
    Task<ExecutionResult<TOut>> ExecuteAsync<TIn, TOut>(
        CompiledScriptRef compiledRef,
        IScriptContext<TIn> context,
        ExecutionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience: looks up script by ID in registry, compiles if necessary, and executes.
    /// </summary>
    Task<ExecutionResult<TOut>> RunAsync<TIn, TOut>(
        string scriptId,
        TIn input,
        ExecutionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the runtime this engine handles.</summary>
    ScriptRuntime Runtime { get; }
}

// ─────────────────────────────────────────────────────────────────────────────
// SARCH-004 option types
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Options that control the compilation behaviour.</summary>
/// <param name="ForceRecompile">When <c>true</c>, bypasses the compiled-artefact cache.</param>
/// <param name="EmitDiagnostics">When <c>true</c>, full diagnostic messages are returned.</param>
public record CompilationOptions(bool ForceRecompile = false, bool EmitDiagnostics = true);

/// <summary>Options that control a single script execution.</summary>
/// <param name="EnableTracing">When <c>true</c>, emits OpenTelemetry spans per tool call.</param>
/// <param name="DryRun">When <c>true</c>, tool calls are intercepted and not forwarded.</param>
public record ExecutionOptions(bool EnableTracing = true, bool DryRun = false);

// ─────────────────────────────────────────────────────────────────────────────
// SARCH-005 — CompilationResult
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Represents the outcome of compiling a <see cref="ScriptDefinition"/>.
/// </summary>
public record CompilationResult
{
    /// <summary><c>true</c> when no error-level diagnostics were produced.</summary>
    public bool Success => Diagnostics.All(d => d.Severity != DiagnosticSeverity.Error);

    /// <summary>Reference to the compiled artefact; <c>null</c> when compilation failed.</summary>
    public required CompiledScriptRef? CompiledRef { get; init; }

    /// <summary>SHA-256 hex digest of the script source used for cache key derivation.</summary>
    public required string ContentHash { get; init; }

    /// <summary>Diagnostics produced by the compiler and security analyser.</summary>
    public IReadOnlyList<DiagnosticMessage> Diagnostics { get; init; } = Array.Empty<DiagnosticMessage>();

    /// <summary>UTC timestamp when compilation completed.</summary>
    public DateTime CompiledAt { get; init; } = DateTime.UtcNow;

    /// <summary>File-system or blob-storage path where the compiled artefact is cached.</summary>
    public string? CachePath { get; init; }
}

/// <summary>
/// Lightweight reference to a compiled script artefact; used to invoke execution
/// without re-parsing or re-compiling the source.
/// </summary>
/// <param name="ScriptId">Registry ID of the originating <see cref="ScriptDefinition"/>.</param>
/// <param name="ContentHash">SHA-256 hex digest that identifies the artefact version.</param>
/// <param name="CachePath">Optional path to the persisted artefact.</param>
/// <param name="CompiledAt">UTC timestamp of compilation.</param>
public record CompiledScriptRef(
    string ScriptId,
    string ContentHash,
    string? CachePath,
    DateTime CompiledAt);

/// <summary>
/// A single compiler or security-analyser diagnostic produced during compilation.
/// </summary>
/// <param name="Severity">Severity level of the message.</param>
/// <param name="Code">Compiler/analyser diagnostic code (e.g., "CS0103", "SA001").</param>
/// <param name="Message">Human-readable description of the diagnostic.</param>
/// <param name="Line">1-based source line, when available.</param>
/// <param name="Column">1-based source column, when available.</param>
public record DiagnosticMessage(
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    int? Line,
    int? Column);

/// <summary>
/// Severity levels for diagnostics produced by the scripting engine.
/// </summary>
public enum DiagnosticSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
}

// ─────────────────────────────────────────────────────────────────────────────
// SARCH-006 — ExecutionResult<TOut>
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Represents the outcome of executing a compiled script.
/// </summary>
/// <typeparam name="TOut">Type of the value returned by the script.</typeparam>
public record ExecutionResult<TOut>
{
    /// <summary><c>true</c> when the script completed without throwing an exception.</summary>
    public required bool Success { get; init; }

    /// <summary>The value produced by the script; <c>null</c> on failure or <c>void</c> scripts.</summary>
    public TOut? Output { get; init; }

    /// <summary>Error message when <see cref="Success"/> is <c>false</c>.</summary>
    public string? Error { get; init; }

    /// <summary>OpenTelemetry Activity span ID for distributed tracing correlation.</summary>
    public string? TraceId { get; init; }

    /// <summary>CPU/memory resources consumed during execution.</summary>
    public ResourceUsage? ResourceUsage { get; init; }

    /// <summary>Wall-clock duration of the execution.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>SHA-256 hex digest of the serialised input, for audit purposes.</summary>
    public string? InputHash { get; init; }

    /// <summary>SHA-256 hex digest of the serialised output, for audit purposes.</summary>
    public string? OutputHash { get; init; }

    /// <summary>Non-fatal warnings raised during execution (e.g., deprecated tool usage).</summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Resource consumption statistics recorded during a single script execution.
/// </summary>
/// <param name="CpuMs">Approximate CPU time consumed in milliseconds.</param>
/// <param name="MemoryPeakBytes">Peak heap allocation in bytes during execution.</param>
/// <param name="ToolCallCount">Number of Tool Bridge calls made by the script.</param>
public record ResourceUsage(long CpuMs, long MemoryPeakBytes, int ToolCallCount = 0);
