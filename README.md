# CRM Solution

Enterprise CRM platform built with ASP.NET Core, React, and EF Core, with optional open-source pluggable components for search, chat, notifications, analytics, signatures, integrations, and AI.

## Current Scope (March 2026)

- Version: 0.625.0
- Primary runtime: monolith API + SPA frontend
- Production database model: single database policy (`crm_db`)
- Supported deployment modes:
  - Monolith (primary and production-ready)
  - Microservices split (available, not the primary runtime)
- Open-source pluggable stack available:
  - Meilisearch
  - Chatwoot
  - Novu
  - Apache Superset
  - DocuSeal
  - n8n
  - Ollama

## Technology Stack

- Backend: ASP.NET Core 10, C#, EF Core
- Frontend: React 18, TypeScript, Material UI
- Database: MariaDB primary (PostgreSQL and SQL Server support in selected scenarios)
- Caching: Redis
- Realtime: SignalR
- AI/Agents: Semantic Kernel and provider abstraction
- Packaging: Docker Compose, optional Kubernetes manifests

## Architecture Summary

- Core architecture style: modular monolith with Hexagonal patterns in service/provider boundaries
- API contracts: DTO-first, entities are not exposed directly
- Provider model: feature-flag and configuration-driven provider factories
- Data model governance:
  - EF Core model and migrations are source of truth
  - no direct schema edits in production paths
- Observability:
  - health endpoints for API and most provider containers
  - centralized logs via container runtime and optional infra tooling

More details:
- docs/development/ARCHITECTURE_OVERVIEW.md
- docs/development/MICROSERVICES_ARCHITECTURE.md
- docs/development/SOLUTION_CONTEXT.md

## Repository Structure

- CRM.Backend
  - src/CRM.Api: monolith API host
  - src/CRM.Core: domain contracts, entities, ports, feature flags
  - src/CRM.Infrastructure: EF, providers, factories, infrastructure services
  - src/Services: optional microservice hosts and gateway
  - tests: unit and integration tests
- CRM.Frontend
  - src/pages, src/components, src/services, src/contexts
- docker
  - monolith, microservices, provider and production compose files
- docs
  - architecture, specifications, guides, remediation plans
- scripts/data-loader
  - API batch data loader and CDT runner

## Local Development

Prerequisites:
- .NET SDK 10
- Node.js 20+
- Docker

Common flow:

1. Backend restore and build
- cd CRM.Backend
- dotnet restore
- dotnet build

2. Frontend install and run
- cd CRM.Frontend
- npm install
- npm start

3. Optional full stack via Docker
- docker compose -f docker/docker-compose.yml up -d

## Open-Source Full Stack Deployment (Example)

Typical full stack services exposed on a deployment host:

- CRM frontend: 80
- CRM API: 5000
- Meilisearch: 7700
- n8n: 5678
- Superset: 8088
- Chatwoot: 3003
- Novu API: 3000
- DocuSeal: 3004
- Ollama: 11434

Notes:
- keep feature flags aligned with provider type settings
- ensure provider API keys and secrets are configured in environment files
- run migrations from EF workflow before high-volume loader runs when schema changed

## Testing

Backend:
- cd CRM.Backend
- dotnet test

Frontend:
- cd CRM.Frontend
- npm test

Data loader:
- cd scripts/data-loader
- python3 run_all_batches.py --base-url http://<server-ip>:5000

## Documentation Index

- docs/11-specifications/INDEX.md
- docs/11-specifications/SOLUTION_GAPS_REMEDIATION_PLAN.md
- docs/11-specifications/FIELD_GAP_REMEDIATION_PLAN.md
- docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md
- docs/architecture/ADR-002-EF-Core-Schema-Management.md
- docs/PHASE4_SERVICE_SPECIFICATIONS.md

## Important Operating Rules

- EF Core is schema source of truth
- Single database policy for CRM runtime
- DTOs are the API contract boundary
- Keep feature specs and remediation plans updated with every material change
- Preserve backward-compatible API behavior unless a planned breaking change is documented
