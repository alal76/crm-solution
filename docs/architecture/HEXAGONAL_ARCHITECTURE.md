# Hexagonal Architecture in CRM Solution

## Scope

This document describes how Hexagonal Architecture is currently applied in CRM Solution as of March 2026.

## Practical Position

The application is not a pure hexagon end-to-end. Instead, it uses a pragmatic hybrid:

- layered modular monolith for core runtime
- ports and adapters at integration boundaries
- provider factories to resolve runtime implementations

This gives integration flexibility without forcing full domain isolation refactors in every module.

## Where Hexagonal Patterns Are Strongest

### 1) Provider Abstractions

Ports in Core define capability contracts (search, chat, notifications, analytics, signatures, integrations, AI).

Adapters in Infrastructure implement these ports (BuiltIn and external provider variants).

Factories choose adapters from configuration and feature flags.

### 2) External Service Boundaries

Infrastructure services encapsulate calls to external products and APIs.

Application code depends on contracts, not vendor SDK surfaces.

### 3) Configuration-Driven Swapping

At runtime, providers can be switched through configuration with minimal consumer changes.

## Core Flow

```
Controller/Application Service
    -> Port Interface (Core)
    -> Factory Resolution (Infrastructure)
    -> Concrete Adapter (Infrastructure)
    -> External System
```

## Current Benefits

- reduced vendor lock-in at capability boundaries
- easier mocking and contract-level testing
- safer phased rollout of external integrations
- fallback paths via BuiltIn providers

## Current Limitations

- not every legacy service path is fully port-first
- some modules still have mixed concerns between app and infrastructure logic
- shared database model creates coupling independent of port boundaries

## Coding Guidance

- new external capability work should start with a Core port contract
- avoid direct controller-to-vendor coupling
- keep DTO contracts stable at API boundary
- register adapters in one place and validate fallback behavior

## Testing Guidance

- unit test adapters against port contract behavior
- integration test provider selection and fallback
- validate failure isolation for each provider channel

## Migration Guidance

When refactoring legacy paths:

1. define or refine port interface
2. map existing service behavior into adapter implementation
3. add factory resolution path
4. update consumers to contract-driven usage
5. add tests for default and external provider paths

## Related Documents

- docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md
- docs/architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md
- docs/development/ARCHITECTURE_OVERVIEW.md
