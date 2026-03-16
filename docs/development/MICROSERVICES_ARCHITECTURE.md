# Microservices Architecture

## Status

Current status: available in codebase, not the primary production runtime.

The CRM solution includes a microservices split implementation and deployment assets, but the active baseline remains the monolith architecture with pluggable provider services.

This document defines what is implemented, what is stable, and how to adopt microservices safely.

## Service Landscape

Target service set in the repository:

- Gateway
- Identity
- Customer
- Sales
- Marketing
- ServiceDesk
- Core
- Frontend

Shared libraries used across service hosts:
- CRM.Core
- CRM.Infrastructure
- CRM.ServiceDefaults

## Current Maturity Model

### Production Baseline

- Monolith API + frontend
- Single MariaDB database (`crm_db`)
- Provider containers optional by feature flag

### Microservices Baseline

- Service host projects and compose assets exist
- Suitable for controlled environments and incremental domain isolation
- Requires additional readiness work for broad production standardization

## Architecture Shape (Microservices Mode)

```
Client
  |
  v
Frontend
  |
  v
Gateway
  |
  +--> Identity
  +--> Customer
  +--> Sales
  +--> Marketing
  +--> ServiceDesk
  +--> Core

Data and integration dependencies remain shared/controlled by environment strategy.
```

## Data Strategy Reality

Current strategy used in practice:
- Shared database model remains dominant
- EF model and migration chain are authoritative

Implications:
- cross-service coupling remains through shared schema unless partitioning is introduced
- deployment ordering and migration governance are critical

## Cross-Cutting Concerns

Implemented patterns across monolith and microservice hosts:

- JWT authentication
- environment-driven config
- health endpoints
- structured logging
- feature management
- provider abstraction for integrations

## Gateway and Routing

Gateway role:
- centralized route ingress
- service routing and edge concerns
- auth token pass-through and policy hosting

Routing strategy should remain contract-first and avoid ad-hoc endpoint drift.

## Operational Constraints

For microservices activation in a given environment, validate:

- migration order and schema state
- service-level env var completeness
- provider dependency readiness
- healthcheck semantics (especially non-HTTP workers)
- observability and alerting coverage

## Adoption Path (Recommended)

1. Keep monolith as source of truth for feature delivery.
2. Select one low-risk domain and run dual verification (monolith vs service host).
3. Gate traffic through gateway with explicit route ownership.
4. Introduce service-level SLOs and rollout criteria.
5. Expand domain by domain only after test, data, and ops criteria pass.

## Exit Criteria for "Microservices Primary"

Before declaring microservices as primary runtime:

- clear domain ownership boundaries documented
- contract tests in place for inter-service APIs
- migration governance automated and stable
- independent deployability demonstrated repeatedly
- rollback path tested per service
- production observability baseline completed

## Recommended Position (Current)

- Keep monolith as primary production architecture.
- Maintain microservices as strategic evolution capability.
- Use provider stack modularity to realize most scaling/integration benefits without forcing early service fragmentation.

## Related Documents

- docs/development/ARCHITECTURE_OVERVIEW.md
- docs/development/SOLUTION_CONTEXT.md
- docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md
- docs/architecture/ADR-002-EF-Core-Schema-Management.md
