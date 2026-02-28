# SPEC-GEN-001: Enum Reference — Central Enum Registry

> **Module:** Core / General
> **Feature:** Centralised reference for all public C# enums in the CRM Solution
> **Version:** 1.2
> **Last Updated:** 2026-02-28
> **Status:** ✅ Active — Living Document (update on every enum addition or change)
> **Location:** `CRM.Backend/src/CRM.Core/Enums/` and `CRM.Backend/src/CRM.Core/Scripting/`

---

## Purpose

This document is the single source of truth for all public `enum` types in the CRM Solution.

**Rules:**
- Every new enum **MUST** be added here before or immediately after implementation.
- Numeric values are the API/DTO contract and **MUST NOT** change once deployed.
- Frontend TypeScript enums or union types must mirror these integer values.
- Test files that assert enum counts (e.g., `Enum.GetValues().Length`) must be updated whenever a value is added/removed.

---

## 1. Scripting Engine Enums

**Namespace:** `CRM.Core.Scripting`
**File:** `CRM.Backend/src/CRM.Core/Scripting/ScriptEnums.cs`
**Status:** ✅ Implemented (SARCH-001, SARCH-003)

### 1.1 ScriptKind

Classification of what a script does and where it runs within the CRM platform.

| Value | Int | Description |
|-------|-----|-------------|
| WorkflowStep | 0 | Executes as a step within a YAML workflow definition (WDL) |
| AgentHook | 1 | Registered as a hook in an AI agent's lifecycle (pre/post-action) |
| Guardrail | 2 | Pre/post-action guard that can abort or redirect execution |
| Transform | 3 | Transforms data from one shape to another (map/filter/enrich) |
| Validation | 4 | Validates an input or output payload against business rules |
| ToolAdapter | 5 | Wraps an external platform service as a Tool Bridge tool |

**Test assertion:** `Enum.GetValues<ScriptKind>().Length == 6`

### 1.2 ScriptRuntime

The sandboxed language runtime used to execute a script.

| Value | Int | Description |
|-------|-----|-------------|
| DotNet | 0 | Roslyn C# scripting runtime (see ADR-006) |
| TypeScript | 1 | isolated-vm V8 TypeScript runtime via sidecar (see ADR-009) |

**Test assertion:** `Enum.GetValues<ScriptRuntime>().Length == 2`

### 1.3 ScriptLifecycleState

Lifecycle state of a script in the registry approvals workflow.
Scripts must progress through states in sequence; regression to `Draft` is permitted for fixes.

| Value | Int | Description |
|-------|-----|-------------|
| Draft | 0 | Being authored; not yet submitted for review |
| Review | 1 | Submitted for peer / security review |
| Approved | 2 | Approved for deployment but not yet deployed |
| Staged | 3 | Deployed to the staging environment |
| Deployed | 4 | Live in the production environment |
| Retired | 5 | No longer active; kept for audit/history purposes |

**Test assertion:** `Enum.GetValues<ScriptLifecycleState>().Length == 6`

**Valid transitions:**

```
Draft → Review → Approved → Staged → Deployed → Retired
           ↑                                   ↓
           └──────── (re-review after fix) ────┘
Draft ← (regress from any pre-Deployed state)
```

---

## 2. Compilation Diagnostics Enums

**Namespace:** `CRM.Core.Scripting`
**File:** `CRM.Backend/src/CRM.Core/Scripting/ICompiledScriptEngine.cs`
**Status:** ✅ Implemented (SARCH-005)

### 2.1 DiagnosticSeverity (Scripting namespace)

Severity of a diagnostic message produced by the scripting engine compiler or security analyser.

> **Note:** A separate `DiagnosticSeverity` enum exists in `CRM.Core.Interfaces.Scripting`
> (legacy simple script engine). Both enums share the same integer values for forward compatibility.

| Value | Int | Description |
|-------|-----|-------------|
| Info | 0 | Informational; no action required |
| Warning | 1 | Potential issue; script can still be executed |
| Error | 2 | Compilation or security check failed; script cannot be executed |

**Test assertion:** `Enum.GetValues<CRM.Core.Scripting.DiagnosticSeverity>().Length == 3`

---

## 3. Legacy Script Language Enum

**Namespace:** `CRM.Core.Enums`
**File:** `CRM.Backend/src/CRM.Core/Enums/ScriptLanguage.cs`
**Status:** ✅ Implemented (pre-SARCH; retained for backward compatibility with `ScriptPlugin` entity)

### 3.1 ScriptLanguage

Scripting language for `ScriptPlugin` entities (Semantic Kernel plugin scripts).
Used by the original `IScriptEngine` in `CRM.Core.Interfaces.Scripting`.

| Value | Int | Description |
|-------|-----|-------------|
| JavaScript | 0 | JavaScript executed via Jint engine (default for ScriptPlugin) |
| Python | 1 | Python executed via Python.NET + RestrictedPython sandbox |
| CSharp | 2 | C# scripting (reserved for future Roslyn integration with ScriptPlugin) |

**Test assertion:** `Enum.GetValues<ScriptLanguage>().Length == 3`

---

## 4. Field Gap Notes

| Enum | Gap | Resolution |
|------|-----|------------|
| `ScriptKind` | No corresponding `LookupCategory` in DB yet | Will be added in SARCH-020 when ScriptRegistry entity is created |
| `ScriptLifecycleState` | No approval workflow entity yet | Will be wired to `ApprovalWorkflow` in SARCH-031 |
| `ScriptRuntime` | No DB storage yet | Stored in `ScriptDefinition` JSON when persisted |

---

## 5. Changelog

| Date | Version | Change |
|------|---------|--------|
| 2026-02-28 | 1.0 | Initial creation — scripting engine enums (SARCH-001, SARCH-003, SARCH-005) |
| 2026-02-28 | 1.1 | Added legacy `ScriptLanguage` documentation |
| 2026-02-28 | 1.2 | Added field gap notes, `DiagnosticSeverity` entry |
| 2026-02-28 | 1.3 | Added Marketing Module enums: SequenceStepType, EmailTrackingEvent, NurtureEnrollmentTrigger, UnsubscribeReason (MKT-001/004/005/006) |

---

## 6. Marketing Module Enums (CampaignEnums.cs — MKT-001, MKT-004, MKT-005, MKT-006)

### `SequenceStepType`
| Value | Int | Description |
|---|---|---|
| Email | 0 | Send an email step |
| Wait | 1 | Delay/wait step |
| Condition | 2 | Branch on condition |
| Tag | 3 | Apply a tag to the lead |

### `EmailTrackingEvent`
| Value | Int | Description |
|---|---|---|
| Sent | 0 | Message sent to provider |
| Delivered | 1 | Confirmed delivery |
| Opened | 2 | Recipient opened email |
| Clicked | 3 | Recipient clicked a link |
| Bounced | 4 | Hard or soft bounce |
| Unsubscribed | 5 | Recipient opted out |
| SpamReported | 6 | Reported as spam |

### `NurtureEnrollmentTrigger`
| Value | Int | Description |
|---|---|---|
| LeadCreated | 0 | New lead created |
| LeadStatusChanged | 1 | Lead status transition |
| ManualEnroll | 2 | Manual enrolment by agent |
| WebFormSubmit | 3 | Web form submission |

### `UnsubscribeReason`
| Value | Int | Description |
|---|---|---|
| NotInterested | 0 | No longer interested |
| TooFrequent | 1 | Too many emails |
| Irrelevant | 2 | Content not relevant |
| NeverSubscribed | 3 | Never consented |
| Other | 4 | Other reason |
