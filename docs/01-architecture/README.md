# Architecture Documentation Index

## Scope

This index is the entry point for architecture documentation in the CRM solution.

Current baseline date: March 2026

## Current Baseline

- Primary runtime: modular monolith
- Primary deployment: Docker Compose
- Data policy: single database model for CRM runtime
- External capabilities: pluggable providers enabled by feature flags and provider type configuration
- Microservices: available as an evolution path, not the default production baseline

## Read First

1. docs/development/ARCHITECTURE_OVERVIEW.md
2. docs/development/SOLUTION_CONTEXT.md
3. docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md
4. docs/architecture/ADR-002-EF-Core-Schema-Management.md

## Architecture Documents

### Runtime and Topology

- docs/development/ARCHITECTURE_OVERVIEW.md
- docs/development/MICROSERVICES_ARCHITECTURE.md
- docs/architecture/CLOUD_DEPLOYMENT_ARCHITECTURE.md
- docs/architecture/KUBERNETES_ARCHITECTURE.md

### Structural Patterns

- docs/architecture/HEXAGONAL_ARCHITECTURE.md
- docs/architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md
- docs/architecture/ITSM_ARCHITECTURE.md

### Operational and Configuration References

- docs/architecture/PORT_CONFIGURATION.md
- docs/architecture/DATABASE_CONFIGURATION.md
- docs/architecture/SECRETS_MANAGEMENT.md

## ADR Collections

There are two ADR collections in the repository today.

### Main ADRs

- docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md
- docs/architecture/ADR-002-EF-Core-Schema-Management.md
- docs/architecture/ADR-003-Runtime-Platform-Evaluation.md
- docs/architecture/ADR-004-Semantic-Kernel-Integration.md
- docs/architecture/ADR-005-Knowledge-Base-Dual-System-Architecture.md

### Secondary ADR Set

- docs/01-architecture/ADR-005-large-file-refactoring-strategy.md
- docs/01-architecture/ADR-006-Roslyn-ScriptEngine.md
- docs/01-architecture/ADR-007-Script-Tool-Bridge.md
- docs/01-architecture/ADR-008-YAML-WDL.md
- docs/01-architecture/ADR-009-TypeScript-Sandbox.md
- docs/01-architecture/ADR-010-Embeddable-Script-Library.md
- docs/01-architecture/ADR-011-domain-model-enrichment-strategy.md
- docs/01-architecture/ADR-012-full-domain-driven-design-strategy.md

## Documentation Rules

- Keep architecture docs aligned with actual running deployment baseline.
- Do not describe microservices as the primary runtime unless platform ownership changes and this index is updated.
- Update this index when architecture files are added, renamed, or removed.
- Keep runtime claims consistent with version.json and deployment reality.

## Related Governance Docs

- docs/11-specifications/SOLUTION_GAPS_REMEDIATION_PLAN.md
- docs/11-specifications/FIELD_GAP_REMEDIATION_PLAN.md
- docs/11-specifications/SPEC-GEN-001-EnumReference.md
