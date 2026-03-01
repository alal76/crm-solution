# ADR-007: Tool Bridge Architecture

**Date:** 2026-02-28
**Status:** Accepted
**Decision Makers:** Architecture Team

---

## Context

Scripts executing inside sandboxed environments need to interact with CRM platform services:
look up customers, create service requests, send notifications, trigger workflows, etc.
Without a controlled gateway, scripts could:

1. Directly access the DI container and bypass authorisation checks.
2. Execute arbitrary database queries.
3. Exfiltrate sensitive data via unconstrained HTTP calls.

A controlled, audited bridge between the sandbox and the platform is required.

---

## Decision

Implement the **Tool Bridge** pattern: scripts interact with the CRM platform exclusively through
`IToolInvoker.CallAsync("ToolName", parameters)`. No other platform access is permitted inside
the sandbox.

---

## Design

```
 Script Sandbox
 ┌────────────────────────────────────┐
 │  context.Tools.CallAsync(...)      │
 └───────────────┬────────────────────┘
                 │
 ┌───────────────▼────────────────────┐
 │       ToolBridgeInvoker            │
 │  (implements IToolInvoker)         │
 │                                    │
 │  1. Permission check               │
 │     (ScriptDefinition.Permissions) │
 │  2. SoD rule enforcement           │
 │  3. Per-tool rate limiting         │
 │  4. Circuit breaker (Polly)        │
 │  5. Audit log entry                │
 │  6. Delegate to ToolDescriptor     │
 └───────────────┬────────────────────┘
                 │
 ┌───────────────▼────────────────────┐
 │  ToolRegistry                      │
 │  (Dictionary<string,ToolDescriptor>│
 │   decorated with [ScriptTool])     │
 └────────────────────────────────────┘
```

### Key Types

- `ToolDescriptor` — metadata + delegate for a registered tool.
- `[ScriptTool]` attribute — decorates methods exposed to scripts.
- `ToolRegistry` — holds all registered descriptors; queried by name.
- `ToolBridgeInvoker` — concrete `IToolInvoker` with security middleware.

---

## Security Properties

| Property | Implementation |
|----------|---------------|
| No direct DI access | Sandbox receives only `IToolInvoker`; no `IServiceProvider` |
| No direct DB access | Tools use services; scripts never see `ICrmDbContext` |
| No unbounded HTTP | HTTP calls go via tools only; `HttpClient` is not injected |
| Permission enforcement | Each call checks `ScriptDefinition.Permissions` allow-list |
| Data exfiltration prevention | Rate limits + circuit breaker cap egress volume (T3) |
| Full auditing | Every tool call logged with tenantId, callerId, duration |

---

## Consequences

- **Positive:** Predictable, auditable, permission-scoped platform access from scripts.
- **Negative:** Every new CRM capability exposed to scripts requires explicit tool registration.
- **Mitigation:** `[ScriptTool]` attribute on existing service methods minimises boilerplate.

---

## Related ADRs

- ADR-006: Roslyn as .NET Scripting Engine
- ADR-009: TypeScript Sandbox (same Tool Bridge used for TS scripts)
