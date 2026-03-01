# ADR-010: Embeddable Script Engine as Library

**Date:** 2026-02-28
**Status:** Accepted
**Decision Makers:** Architecture Team

---

## Context

The scripting engine must be usable from:

1. The monolithic **CRM.Api** (current primary host).
2. Future domain microservices (`crm-sales`, `crm-servicedesk`, etc.) that need to execute
   workflow scripts in-process.
3. Potentially, an independent `crm-workflow-engine` microservice.

Duplicating the scripting infrastructure per service is not sustainable. A shared,
self-contained library with a single registration entry point is required.

---

## Decision

Package the scripting engine as a .NET class library project **`CRM.Infrastructure.Scripting`**
with a single extension method entry point:

```csharp
services.AddCrmScripting(options =>
{
    options.EnabledRuntimes = new[] { ScriptRuntime.DotNet, ScriptRuntime.TypeScript };
    options.DefaultMemoryLimitMb = 64;
    options.DefaultTimeout = TimeSpan.FromSeconds(30);
    options.TypeScriptSidecarSocketPath = "/tmp/crm-script-runner.sock";
});
```

---

## Registered Services

| Service | Scope | Notes |
|---------|-------|-------|
| `ICompiledScriptEngine` (keyed: DotNet) | Singleton | `RoslynScriptEngine` |
| `ICompiledScriptEngine` (keyed: TypeScript) | Singleton | `TypeScriptScriptEngine` |
| `ToolRegistry` | Singleton | Auto-scans `[ScriptTool]` attributes |
| `IToolInvoker` | Scoped | `ToolBridgeInvoker` with Polly pipeline |
| `ISecretAccessor` | Scoped | Platform-configured (Key Vault / env vars) |
| `IStateAccessor` | Scoped | Redis-backed `RedisStateAccessor` |
| `IMetricsRecorder` | Singleton | OTel-based `OpenTelemetryMetricsRecorder` |
| `IScriptLogger` | Scoped | `ExecutionContextScriptLogger` |

---

## Dependencies

The library has minimal external dependencies:

| Package | Purpose |
|---------|---------|
| `Microsoft.CodeAnalysis.CSharp.Scripting` | Roslyn .NET runtime |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | DI registration |
| `Microsoft.Extensions.Options` | `ScriptingOptions` configuration |
| `System.Security.Cryptography` | SHA-256 content hash |
| `Polly` | Circuit breaker + retry in `ToolBridgeInvoker` |

---

## Usage in CRM.Api

```csharp
// Program.cs
builder.Services.AddCrmScripting(options =>
    builder.Configuration.GetSection("Scripting").Bind(options));
```

---

## Usage in a Microservice

```csharp
// Add only DotNet runtime for a lightweight microservice
builder.Services.AddCrmScripting(options =>
{
    options.EnabledRuntimes = new[] { ScriptRuntime.DotNet };
});
```

---

## Consequences

- **Positive:** Single source of truth for scripting setup; reusable across all services.
- **Negative:** Any microservice hosting scripts takes a dependency on Roslyn (~15 MB).
- **Mitigation:** Runtime selection via `EnabledRuntimes` allows lightweight configurations
  (e.g., TypeScript-only sidecars).

---

## Related ADRs

- ADR-006: Roslyn as .NET Scripting Engine
- ADR-007: Tool Bridge Architecture
- ADR-008: YAML Workflow Definition Language
- ADR-009: TypeScript Sandbox via isolated-vm
