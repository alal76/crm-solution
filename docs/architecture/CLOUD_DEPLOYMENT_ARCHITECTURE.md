# Cloud Deployment Architecture

## Scope and Intent

This document describes the current cloud and server deployment architecture for CRM Solution as of March 2026.

Primary deployment model in active use:
- Containerized monolith runtime (frontend + api + data + optional OSS providers)
- Environment-driven configuration with Docker Compose
- Optional Kubernetes path for selected environments

## Current Runtime Topology

```
Internet / Internal Users
        |
        v
   crm-frontend (port 80)
        |
        v
     crm-api (port 5000)
        |
        +--> MariaDB (crm_db)
        +--> Redis
        +--> Provider endpoints (Meilisearch, Chatwoot, Novu, Superset, DocuSeal, n8n, Ollama)
```

## Deployment Targets

Supported target classes:
- On-prem Linux hosts (current dominant operational model)
- Cloud VM targets (AWS/Azure/GCP IaaS)
- Kubernetes clusters (for orchestrated environments)

The same application artifacts can be deployed to these targets with environment-specific compose/manifests and secrets.

## Current Operational Reality

- Monolith runtime is the production baseline.
- Microservices deployment is available but not the default support path.
- Provider stack can be enabled per environment via feature flags and provider type config.
- Health checks must be interpreted by service type (HTTP app vs worker process).

## Configuration Model

### 1) Core configuration

- ASPNETCORE_ENVIRONMENT
- ConnectionStrings__DefaultConnection
- Jwt__*
- Redis__*
- FeatureManagement__*
- Providers__* (category, endpoint, credentials)

### 2) Provider configuration categories

- Search
- Chat
- Notifications
- Analytics
- Signatures
- Integrations
- AI

### 3) Secrets

Secrets are environment-bound and injected via deployment configuration.
No secrets should be hard-coded in source.

## Deployment Pipeline Pattern

Typical deployment sequence:

1. Build backend and frontend images.
2. Tag images by version and latest strategy.
3. Push or transfer images to target runtime.
4. Apply compose or Kubernetes manifests with target env file.
5. Run startup/migration checks.
6. Run health verification and endpoint checks.

## Failure Domains and Recovery

### API startup/migrations

Risk:
- schema drift or migration history mismatch can block startup.

Recovery:
- align EF migration history and schema under controlled workflow.
- rerun startup after drift correction.

### Provider startup

Risk:
- third-party container requires first-run initialization
- missing provider environment variables

Recovery:
- run service-specific bootstrap (for example chat/superset init)
- verify endpoint behavior instead of relying on generic root paths only.

### Disk pressure

Risk:
- image accumulation can block rollout.

Recovery:
- remove obsolete runtime images before pulling new versions.
- keep deployment hosts under monitored disk usage thresholds.

## Network and Ports (Typical Full Stack)

- Frontend: 80
- API: 5000
- Meilisearch: 7700
- n8n: 5678
- Superset: 8088
- Chatwoot: 3003
- Novu API: 3000
- DocuSeal: 3004
- Ollama: 11434

## Governance Constraints

- EF Core model/migrations are source of truth for schema changes.
- Single database policy remains active for CRM runtime.
- DTO contracts must stay aligned with API output.
- Architecture docs and spec plans should be updated with material platform/runtime changes.

## Related Documents

- docs/development/ARCHITECTURE_OVERVIEW.md
- docs/development/MICROSERVICES_ARCHITECTURE.md
- docs/development/SOLUTION_CONTEXT.md
- docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md
- docs/architecture/ADR-002-EF-Core-Schema-Management.md
