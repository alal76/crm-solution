# ADR-006: Roslyn as .NET Scripting Engine

**Date:** 2026-02-28
**Status:** Accepted
**Decision Makers:** Architecture Team

---

## Context

The CRM platform requires a sandboxed execution environment for user-defined .NET/C# scripts (workflow steps, guardrails, agent hooks). Scripts must be isolated from the host process, subject to compile-time security analysis, and amenable to artefact caching to avoid re-compilation on every invocation.

Candidates evaluated:

| Option | Sandboxing | Compile-time analysis | C# support | Notes |
|--------|-----------|----------------------|------------|-------|
| **Roslyn CSharp.Scripting** | Via `AssemblyLoadContext` | `DiagnosticAnalyzer` | Full | Microsoft-maintained |
| **dotnet-script** | None | None | Full | No isolation |
| **Jint** | Process-level V8 | None | JavaScript only | No C# |
| **Python.NET + RestrictedPython** | Partial | None | No | Already used for Python |

---

## Decision

Use **Microsoft.CodeAnalysis.CSharp.Scripting** (Roslyn) as the .NET/C# runtime engine,
registered as `ICompiledScriptEngine` with `Runtime == ScriptRuntime.DotNet`.

---

## Rationale

- **Full C# language support** with strong typing — enables IDE-quality tooling in the script editor.
- **Custom `DiagnosticAnalyzer`** blocks dangerous APIs (reflection, P/Invoke, file I/O) at compile
  time before any code executes (SARCH-022).
- **`AssemblyLoadContext` (collectible)** provides isolated, unloadable execution contexts per
  invocation, preventing host-process pollution (SARCH-025).
- **SHA-256 compiled-artefact caching** (see `CompiledScriptRef.ContentHash`) avoids re-parsing and
  re-compilation when source is unchanged.
- Native integration with .NET DI, OpenTelemetry, and `CancellationToken`-based timeouts.

---

## Implementation Notes

- NuGet: `Microsoft.CodeAnalysis.CSharp.Scripting` (≥ 4.9.x)
- Allow-list approach for assembly references — only CRM contracts and approved BCL subsets
  (SARCH-023).
- Active `MemoryWatchdog` required to enforce `ScriptDefinition.MemoryLimitMb` (SARCH-026).
- `ICompiledScriptEngine` (DotNet) registered in DI via `services.AddCrmScripting()`.

---

## Consequences

- **Positive:** Compile-time guarantees, strong type safety, caching, collectible ALCs.
- **Negative:** `Microsoft.CodeAnalysis.CSharp.Scripting` adds ~15 MB to the binary.
- **Risk:** Memory pressure if ALCs are not collected promptly — mitigated by `MemoryWatchdog`.

---

## Rejected Alternatives

| Alternative | Reason for rejection |
|-------------|----------------------|
| Jint | JavaScript-only; no C# language support |
| dotnet-script | No sandboxing or compile-time analysis |
| AssemblyLoadContext alone | Provides isolation but no compile-time security analysis |

---

## Related ADRs

- ADR-007: Tool Bridge Architecture
- ADR-009: TypeScript Sandbox (isolated-vm)
- ADR-010: Embeddable Script Engine as Library
