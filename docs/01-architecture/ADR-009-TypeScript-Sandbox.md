# ADR-009: TypeScript Script Sandbox via isolated-vm

**Date:** 2026-02-28
**Status:** Accepted
**Decision Makers:** Architecture Team

---

## Context

TypeScript scripts must run with strong isolation guarantees:

1. A compromised script must not be able to access the Node.js host process globals.
2. Memory limits must be enforced at the hardware/V8 level, not just via timeouts.
3. The sidecar process itself must have no network access (all egress via Tool Bridge).
4. The TypeScript type-checker must be run before execution to catch type errors early.

---

## Decision

Use the **isolated-vm** npm package to create a separate V8 Isolate per execution,
running inside a dedicated **`crm-script-runner`** Node.js 20 sidecar process.
C# communicates with the sidecar via **HTTP over a named Unix socket**.

---

## Architecture

```
 CRM.Api / CRM.Infrastructure
 ┌────────────────────────────────────┐
 │  TypeScriptScriptEngine            │
 │  (implements ICompiledScriptEngine │
 │   with Runtime == TypeScript)      │
 │                                    │
 │  CompileAsync: POST /compile       │──── Unix socket ────►
 │  ExecuteAsync: POST /execute       │──── Unix socket ────►
 └────────────────────────────────────┘                      │
                                                             │
                                        ┌────────────────────▼────┐
                                        │  crm-script-runner       │
                                        │  (Node.js 20, no network)│
                                        │                          │
                                        │  1. SWC AST scan –        │
                                        │     block eval, import,   │
                                        │     globalThis, Proxy,    │
                                        │     Reflect               │
                                        │                          │
                                        │  2. tsc strict type-check │
                                        │                          │
                                        │  3. isolated-vm Isolate   │
                                        │     (memoryLimit from     │
                                        │      ScriptDefinition)    │
                                        │                          │
                                        │  4. @engine/stdlib        │
                                        │     (http → Tool Bridge,  │
                                        │      encoding, date,      │
                                        │      crypto hash only)    │
                                        └──────────────────────────┘
```

---

## Contract Generation

- `@engine/contracts`: TypeScript `.d.ts` files auto-generated from C# `IScriptContext<TIn>` types
  using **NSwag** on every build. Ensures C# and TypeScript contracts stay in sync.
- `@engine/stdlib`: audited, tree-shakeable utility library; all HTTP calls tunnelled via
  `context.tools.callAsync()` (maps to `IToolInvoker`).

---

## Security Controls

| Control | Implementation |
|---------|---------------|
| V8 memory isolation | `isolated-vm` `MemoryLimit` derived from `ScriptDefinition.MemoryLimitMb` |
| No `eval()` / `Function()` | SWC AST scan blocks at compile time |
| No dynamic `import()` | SWC AST scan blocks at compile time |
| No access to `globalThis` / `process` | isolated-vm context sealing |
| No direct HTTP | `fetch` not present in `@engine/stdlib`; only Tool Bridge tunnel |
| CPU timeout | `isolate.compileScript()` + `script.run()` with `timeout` ms param |

---

## Communication Protocol

- **Transport:** HTTP/1.1 over named Unix socket (`/tmp/crm-script-runner.sock`).
- **Serialisation:** `application/json` with `System.Text.Json` on the C# side.
- **Endpoints:**
  - `POST /compile` — SWC scan + tsc type-check; returns `CompilationResult`-compatible JSON.
  - `POST /execute` — run in Isolate; returns `ExecutionResult`-compatible JSON.
  - `GET /health` — readiness probe for the sidecar container.

---

## Deployment

- `crm-script-runner` ships as a Docker image alongside `crm-api`.
- In Kubernetes, runs as a sidecar container sharing a `emptyDir` volume for the Unix socket.
- No external network policy required — sidecar communicates only via Unix socket.

---

## Consequences

- **Positive:** Hardware-level V8 memory isolation; no risk of TypeScript scripts escaping the sandbox.
- **Negative:** Requires Node.js 20 sidecar process/container; adds operational complexity.
- **Risk:** Cold-start latency on first Isolate creation (~50 ms); mitigated by warm Isolate pool.

---

## Related ADRs

- ADR-006: Roslyn as .NET Scripting Engine (C# counterpart)
- ADR-007: Tool Bridge Architecture (same bridge used for TS scripts)
- ADR-010: Embeddable Script Engine as Library
