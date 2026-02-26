# CRM Solution — Master TODO List

> **Last Updated:** February 26, 2026  
> **Version:** 0.593.0  
> **Status:** ❌ 23 Pending | ✅ 1 Done (this session) | ⚠️ 0 Partial  
> **Historical Completion:** 502 items completed through Feb 25, 2026

---

## Active Features

### Feature: Scripting Language Support

**Goal:** Allow users to **write new** and **edit existing** Workflow Script nodes and Agent Script Plugins using JavaScript (Jint, always-on) and/or Python 3.x (Python.NET + RestrictedPython, feature-flagged).  

**Specifications:**
- [SPEC-SD-004-WorkflowEngine.md](11-specifications/SPEC-SD-004-WorkflowEngine.md) — SF11 + Section 3.9 (Workflow Script node language support)
- [SPEC-AI-006-AgentScripting.md](11-specifications/SPEC-AI-006-AgentScripting.md) — Full dual-language agent & workflow scripting architecture

**User Stories Covered:**
| Story | Component |
|-------|-----------|
| Edit existing Workflow Script nodes (change language, update code) | `ScriptNodeEditor` + `ScriptNodeConfigDto.language` field |
| Write new Workflow Script nodes from the workflow designer | `ScriptNodeEditor` + `ExecuteScriptAction` via `IScriptEngine` |
| Edit existing Agent Script Plugins | `ScriptPluginEditorPage` + `ScriptPluginService.UpdateAsync` |
| Write new Agent Script Plugins | `ScriptPluginsController POST` + `ScriptPluginLibraryPage` |

---

## Summary by Priority

| Priority | Count | Area |
|----------|-------|------|
| **P0** | 6 | Backend foundation (interface, enum, factory, refactor) + Testing (enum test) |
| **P1** | 14 | Backend engines, entity/DB, services, SK integration, API, frontend core |
| **P2** | 2 | Frontend optional components (TestPanel, VariableInspector) |
| **Done** | 1 | Enum reference documentation |
| **Total** | **23** | — |

---

## TODO Items

### Group 1 — Backend Foundation (P0)
> These must be done first — everything else depends on them.

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-001 | AI006-TODO01 / SD004-TODO01 | P0 | Create `IScriptEngine` interface + `ScriptExecutionResult` + `ScriptDiagnostic` records in `CRM.Core/Interfaces/Scripting/` | Not Started |
| SCRIPT-002 | AI006-TODO05 / SD004-TODO04 | P0 | Create `ScriptLanguage` enum at `CRM.Core/Enums/ScriptLanguage.cs` (JavaScript=0, Python=1, CSharp=2) | Not Started |
| SCRIPT-003 | AI006-TODO04 / SD004-TODO08 | P0 | Implement `ScriptEngineFactory` resolving `IScriptEngine` by `ScriptLanguage` from DI | Not Started |
| SCRIPT-004 | SD004-TODO02 | P0 | Refactor `ExecuteScriptAction` call-site in `WorkflowWorkerService` to resolve and invoke `IScriptEngine` via `ScriptEngineFactory` | Not Started |
| SCRIPT-005 | AI006-TODO02 / SD004-TODO03 | P0 | Extract existing inline Jint JavaScript logic from `WorkflowWorkerService` into `JintScriptEngine : IScriptEngine` (preserving timeout/memory sandbox) | Not Started |

### Group 2 — Backend Engine Implementations (P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-006 | AI006-TODO03 / SD004-TODO06,07 | P1 | Implement `PythonScriptEngine : IScriptEngine` using Python.NET + RestrictedPython sandbox (gated by `FeatureManagement:EnablePythonScripting` flag) | Not Started |

### Group 3 — Backend ScriptPlugin Entity & Persistence (P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-007 | AI006-TODO06 | P1 | Add `ScriptPlugin` entity and `DbSet<ScriptPlugin>` to `CrmDbContext` | Not Started |
| SCRIPT-008 | AI006-TODO07 | P1 | Create EF Core migration `AddScriptPlugins` and apply to `crm_db` | Not Started |
| SCRIPT-009 | AI006-TODO08 | P1 | Implement `IScriptPluginService` / `ScriptPluginService` (CRUD: Create, UpdateAsync, Delete, GetAll, GetById, TestExecute) | Not Started |

### Group 4 — Semantic Kernel Integration (P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-010 | AI006-TODO09 | P1 | Implement `ScriptPluginLoader` — reads enabled `ScriptPlugin` rows from DB and registers each as a `KernelPlugin` with a `KernelFunction` wrapper | Not Started |
| SCRIPT-011 | AI006-TODO10 | P1 | Update `CrmKernelFactory.CreateKernelAsync()` to call `ScriptPluginLoader.LoadDynamicPluginsAsync()` after static plugin registration | Not Started |
| SCRIPT-012 | AI006-TODO12 | P1 | Register `JintScriptEngine`, `PythonScriptEngine` (conditional), `ScriptEngineFactory`, `ScriptPluginLoader`, `ScriptPluginService` in `SemanticKernelServiceExtensions` | Not Started |

### Group 5 — Backend API Layer (P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-013 | AI006-TODO11 | P1 | Implement `ScriptPluginsController` with endpoints: GET /api/script-plugins, GET /{id}, POST, PUT /{id}, DELETE /{id}, POST /{id}/enable, POST /{id}/disable, POST /test, GET /languages | Not Started |
| SCRIPT-014 | SD004-TODO05 | P1 | Add `language` (ScriptLanguage enum) field to `ScriptNodeConfigDto` and persist/read from `WorkflowNodes.ConfigurationJson` | Not Started |

### Group 6 — Frontend Core Components (P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-015 | AI006-TODO13 / SD004-TODO09 | P1 | Build `ScriptNodeEditor` React component with `@monaco-editor/react`, language selector (JS/Python), and workflow context variable hints — used in both new workflow creation and editing existing Script nodes | Not Started |
| SCRIPT-016 | AI006-TODO16 | P1 | Build `ScriptPluginLibraryPage` (list view for creating new agent scripts) and `ScriptPluginEditorPage` (Monaco editor for editing existing and creating new agent script plugins) | Not Started |
| SCRIPT-017 | AI006-TODO17 | P1 | Add `scriptPluginService.ts` TypeScript service (axios calls for all `ScriptPluginsController` endpoints, typed DTOs) | Not Started |

### Group 7 — Frontend Optional Components (P2)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-018 | AI006-TODO14 / SD004-TODO10 | P2 | Build `ScriptTestPanel` React component — inline test runner accepting mock context JSON and showing stdout / return value / errors | Not Started |
| SCRIPT-019 | AI006-TODO15 / SD004-TODO11 | P2 | Build `ScriptVariableInspector` React component — sidebar listing available workflow context variables with types and sample values | Not Started |

### Group 8 — Testing (P0/P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-020 | AI006-TODO21 / SD004-TODO14 | P0 | Add `ScriptLanguageEnumTests` unit test — assert count=3 and values JavaScript=0, Python=1, CSharp=2 | Not Started |
| SCRIPT-021 | AI006-TODO18 / SD004-TODO12 | P1 | Write unit tests for `JintScriptEngine`: timeout enforcement, memory limit, context variable injection, `log()` capture, error propagation | Not Started |
| SCRIPT-022 | AI006-TODO19 / SD004-TODO13 | P1 | Write unit tests for `PythonScriptEngine`: sandbox restriction (import block), context injection, timeout, basic evaluation | Not Started |
| SCRIPT-023 | AI006-TODO20 | P1 | Write unit tests for `ScriptPluginLoader` (dynamic kernel plugin registration) and `ScriptPluginService` (CRUD + validation) | Not Started |

### Group 9 — Documentation (Done)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-024 | AI006-TODO22 | P0 | Update `SPEC-GEN-001-EnumReference.md` with `ScriptLanguage` enum (section 2.8) | Done (Feb 26, 2026) |

---

## Recommended Implementation Order

Phase 1 — Foundation (do first, unblocks everything):
  SCRIPT-001  IScriptEngine interface + result types
  SCRIPT-002  ScriptLanguage enum (.cs file)
  SCRIPT-003  ScriptEngineFactory
  SCRIPT-005  JintScriptEngine (extract from WorkflowWorkerService)
  SCRIPT-004  Refactor ExecuteScriptAction call-site
  SCRIPT-020  ScriptLanguage enum unit test (validate values immediately)

Phase 2 — Python Engine (adds second language):
  SCRIPT-006  PythonScriptEngine + RestrictedPython sandbox
  SCRIPT-021  JintScriptEngine unit tests
  SCRIPT-022  PythonScriptEngine unit tests

Phase 3 — ScriptPlugin Entity & Service (enables agent scripts):
  SCRIPT-007  ScriptPlugin entity + DbSet
  SCRIPT-008  EF migration AddScriptPlugins
  SCRIPT-009  ScriptPluginService
  SCRIPT-013  ScriptPluginsController
  SCRIPT-014  language field in ScriptNodeConfigDto

Phase 4 — Semantic Kernel Integration:
  SCRIPT-010  ScriptPluginLoader
  SCRIPT-011  CrmKernelFactory update
  SCRIPT-012  DI registration in SemanticKernelServiceExtensions
  SCRIPT-023  ScriptPluginLoader + ScriptPluginService unit tests

Phase 5 — Frontend:
  SCRIPT-017  scriptPluginService.ts (must exist before UI)
  SCRIPT-015  ScriptNodeEditor with Monaco (workflow new + edit)
  SCRIPT-016  ScriptPluginLibraryPage + ScriptPluginEditorPage (agent plugin new + edit)
  SCRIPT-018  ScriptTestPanel (optional, add after core UI works)
  SCRIPT-019  ScriptVariableInspector (optional, add after core UI works)

---

## Key Implementation Notes

### Feature Flag
Python engine is gated by `FeatureManagement:EnablePythonScripting`. When false, `ScriptEngineFactory` throws `NotSupportedException` for `ScriptLanguage.Python` and the frontend language selector hides the Python option.

### Enum File to Create
```csharp
// CRM.Core/Enums/ScriptLanguage.cs
namespace CRM.Core.Enums;

public enum ScriptLanguage
{
    JavaScript = 0,
    Python = 1,
    CSharp = 2
}
```

### ScriptPlugin Entity Summary
Fields: Id, Name, Description, Language (ScriptLanguage), Code (TEXT), IsEnabled, Parameters (JSON), ReturnType, AgentId (nullable), CreatedAt, UpdatedAt, IsDeleted, RowVersion

### Frontend Package Required
```bash
npm install @monaco-editor/react monaco-editor
```

### Python.NET NuGet Package
```xml
<PackageReference Include="pythonnet" Version="3.0.3" />
```

---

## Stats

| Metric | Value |
|--------|-------|
| Total pending items | 23 |
| Total done this session | 1 |
| Total historically completed | 502 |
| Specs covering this feature | 2 (SPEC-SD-004 v1.3, SPEC-AI-006 v1.0) |
| New enum | ScriptLanguage (SPEC-GEN-001 section 2.8) |
| Feature branch suggested | feature/scripting-language-support |

---

**Document Maintained By:** GitHub Copilot  
**Next Review:** After Phase 1 completion

**END OF MASTER TODO LIST**
