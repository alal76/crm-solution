# ADR-008: YAML Workflow Definition Language (WDL)

**Date:** 2026-02-28
**Status:** Accepted
**Decision Makers:** Architecture Team

---

## Context

Workflow definitions must be:

- **Human-readable and versionable** (stored in Git alongside application code).
- **Expressive** enough to model sequential, parallel, conditional, and looping control flows.
- **Embeddable** — parseable by the CRM backend without external services.
- **Extensible** — support user-defined script steps, tool calls, approvals, delays, and sub-workflows.

Evaluated formats: BPMN/XML, JSON, Temporal's Go DSL, and a custom YAML dialect.

---

## Decision

Use a **YAML-based Workflow Definition Language (WDL)** parsed by **YamlDotNet**, with
`${}` expression syntax resolved against **Common Expression Language (CEL)**.

---

## Step Types

| Type | Description |
|------|-------------|
| `script` | Executes a script from the registry via `ICompiledScriptEngine` |
| `tool` | Directly calls a registered Tool Bridge tool |
| `condition` | Branches on a CEL expression result |
| `parallel` | Fans out to N child steps, waits for all |
| `loop` | Iterates over a list with optional break condition |
| `delay` | Waits for a duration or until a timestamp |
| `subworkflow` | Invokes another WDL workflow by ID |
| `approval` | Suspends until a human approves via the portal |

---

## Expression Syntax

- `${steps.step_name.output.field}` — access a previous step's output field.
- `${input.fieldName}` — access the workflow's top-level input.
- `${env.tenantId}` — access execution environment properties.
- CEL functions: `size()`, `has()`, `matches()`, `now()`, arithmetic operators, ternary.

---

## Example WDL Fragment

```yaml
name: qualify-lead
version: "1.0.0"
input:
  schema: "#/schemas/LeadInput"
steps:
  - id: score
    type: script
    scriptId: lead-scorer-v2
    input:
      leadId: ${input.leadId}

  - id: route
    type: condition
    expression: ${steps.score.output.score} >= 75
    trueBranch:
      - id: fast-track
        type: tool
        tool: CreateOpportunity
        params:
          leadId: ${input.leadId}
    falseBranch:
      - id: nurture
        type: tool
        tool: AddToNurtureSequence
        params:
          leadId: ${input.leadId}
```

---

## Dependencies

| Package | Purpose |
|---------|---------|
| `YamlDotNet` | YAML parsing and deserialisation |
| `NJsonSchema` | JSON Schema validation for `input`/`output` schemas |
| CEL evaluator (SARCH-049) | Expression language runtime |

---

## Consequences

- **Positive:** Human-readable, Git-diffable workflow definitions.
- **Negative:** YAML indentation errors can be subtle; schema validation mitigates this.
- **Risk:** CEL evaluator must be sandboxed to prevent code injection via expressions.

---

## Related ADRs

- ADR-006: Roslyn as .NET Scripting Engine (`script` step executor)
- ADR-007: Tool Bridge Architecture (`tool` step executor)
