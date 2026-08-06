# Claude workspace instructions for this repository

This repository is a full-stack CRM solution with a .NET backend, React frontend, and extensive documentation/spec workflows. Follow these rules when working here:

## Core expectations
- Read the existing repository guidance in [.github/copilot-instructions.md](.github/copilot-instructions.md) before making changes.
- Prefer spec-first work: if a change affects a feature, check the matching document in [docs/11-specifications](docs/11-specifications) first.
- Keep the single-database policy intact: use the production database only and avoid reintroducing demo database flows.
- Preserve existing architecture patterns and do not delete code without confirming it is unused.

## Development workflow
1. Investigate the root cause before changing behavior.
2. Prefer small, focused edits over large rewrites.
3. Add or update tests for behavior changes.
4. Verify builds/tests relevant to the touched area before claiming completion.
5. Update documentation when behavior or process changes.

## Backend guidance
- Follow the existing .NET conventions under CRM.Backend/src.
- Keep DTO/API contracts aligned with the documented schemas.
- Use EF Core migrations for schema changes rather than ad hoc SQL.

## Frontend guidance
- Keep React and TypeScript changes aligned with the existing component and service patterns.
- Preserve route structure and shared state conventions.

## Useful anchors
- [.github/copilot-instructions.md](.github/copilot-instructions.md)
- [docs/11-specifications](docs/11-specifications)
- [docs/common_development_issues.md](docs/common_development_issues.md)
