# Architecture Decision Records (Decisions Subfolder)

## Purpose

This folder stores architecture decision records for focused initiatives and legacy decision streams.

For main platform ADRs, use docs/architecture/ADR-001..ADR-005 as the primary source.

## Current Position (March 2026)

- Primary architecture baseline is documented in docs/development/ARCHITECTURE_OVERVIEW.md.
- Monolith runtime with pluggable provider stack is the active production baseline.
- Microservices remain an optional evolution path.

## ADR Format

Each ADR should include:

1. Title and identifier
2. Status
3. Context
4. Decision
5. Consequences
6. Supersession notes when applicable

## ADR Index in This Folder

- 001-coding-standards-enforcement.md
- 002-security-headers-middleware.md
- 003-microservices-architecture.md
- 004-architecture-review-remediation.md

## Status Guidance

Use one of:
- Proposed
- Accepted
- Deprecated
- Superseded

When an ADR is superseded, include a clear pointer to the replacing ADR.

## Authoring Rules

- Keep decisions concise and traceable.
- Reference concrete runtime behavior, not assumptions.
- Align decision statements with current deployment baseline docs.
- Update related architecture index files when adding or changing ADRs.

## Cross References

- docs/01-architecture/README.md
- docs/development/ARCHITECTURE_OVERVIEW.md
- docs/architecture/CLOUD_DEPLOYMENT_ARCHITECTURE.md
- docs/architecture/KUBERNETES_ARCHITECTURE.md
