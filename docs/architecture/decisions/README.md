# Architecture Decision Records (ADR)

This directory contains Architecture Decision Records (ADRs) for the CRM Solution project.

## What is an ADR?

An Architecture Decision Record (ADR) is a document that captures an important architectural decision made along with its context and consequences.

## ADR Format

Each ADR follows this template:

```markdown
# ADR-XXX: Title

## Status
[Proposed | Accepted | Deprecated | Superseded by ADR-YYY]

## Context
What is the issue that we're seeing that is motivating this decision or change?

## Decision
What is the change that we're proposing and/or doing?

## Consequences
What becomes easier or more difficult to do because of this change?
```

## ADR Index

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [001](001-coding-standards-enforcement.md) | Coding Standards Enforcement | Accepted | 2026-02-02 |
| [002](002-security-headers-middleware.md) | Security Headers Middleware | Accepted | 2026-02-02 |
| [003](003-microservices-architecture.md) | Microservices Architecture | Accepted | 2026-02-02 |

## Guidelines for New ADRs

1. **Numbering**: Use sequential three-digit numbers (001, 002, 003...)
2. **File naming**: `XXX-short-title-with-dashes.md`
3. **Keep it concise**: Focus on the key points
4. **Include context**: Explain why this decision was needed
5. **Document consequences**: Both positive and negative
6. **Update status**: Keep ADR status current

## When to Write an ADR

Write an ADR when you:
- Make a significant architectural decision
- Choose between multiple viable options
- Change an existing architectural pattern
- Introduce a new technology or framework
- Deprecate a current approach

## References

- [ADR GitHub Organization](https://adr.github.io/)
- [Documenting Architecture Decisions - Michael Nygard](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
