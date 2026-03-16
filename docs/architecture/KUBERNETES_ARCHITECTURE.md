# Kubernetes Architecture

## Status

Kubernetes support exists in this repository, but Kubernetes is not the primary runtime baseline as of March 2026.

Primary runtime remains Docker Compose based monolith deployments with optional OSS provider containers.

## Purpose of K8s Assets

Kubernetes manifests in this repo are intended for:
- staged environments requiring orchestrator controls
- autoscaling and policy-driven operations
- teams standardizing on cluster-first operations

They are not the canonical source of production behavior for all environments.

## Logical Workload Model

```
Ingress / Gateway
    |
    v
Frontend Pod(s)
    |
    v
API Pod(s)
    |
    +--> MariaDB service/stateful component
    +--> Redis service
    +--> Optional provider deployments (search/chat/notifications/analytics/signatures/integrations/AI)
```

## Core Patterns

- Namespace isolation by environment
- ConfigMaps for non-secret config
- Secrets for sensitive settings
- Deployments for stateless workloads
- Stateful patterns for persistence components
- Service discovery via cluster DNS
- Health probes aligned with container behavior

## Deployment Guidance

Recommended approach for Kubernetes adoption in this solution:

1. Start from monolith baseline behavior and parity-test in cluster.
2. Keep API contracts and feature flag behavior identical to compose deployments.
3. Validate provider dependencies one category at a time.
4. Ensure migrations and schema governance are explicitly handled before rollout.

## Operational Requirements

Before production K8s rollout:
- defined ingress and TLS strategy
- defined storage class and backup strategy for stateful components
- readiness/liveness probe validation for each workload type
- alerting and logs aggregation configured
- rollback path tested

## Known Caveats

- Worker containers may not expose HTTP health endpoints and need custom probe strategy.
- Third-party provider containers can require bootstrap workflows on first start.
- Shared database model still applies unless a specific service-bound data partitioning initiative is completed.

## Scaling Position

- Horizontal scaling is straightforward for frontend and api workloads.
- Scaling stateful/provider components depends on product-specific constraints and should be validated per provider.
- Do not assume all provider components are horizontally scalable by default.

## Security Position

- Use secrets for credentials and keys.
- Restrict cross-namespace communication where possible.
- Apply least-privilege service account and network policy posture.
- Keep image provenance and version pinning explicit in manifests.

## Related Documents

- docs/development/ARCHITECTURE_OVERVIEW.md
- docs/development/MICROSERVICES_ARCHITECTURE.md
- docs/architecture/CLOUD_DEPLOYMENT_ARCHITECTURE.md
- docs/architecture/PORT_CONFIGURATION.md
