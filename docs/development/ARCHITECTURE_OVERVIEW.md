# Architecture Overview

## Purpose

This document defines the current architecture baseline of the CRM solution for delivery, operations, and future evolution.

Scope date: March 2026

## Runtime Position

The solution is currently operated as a modular monolith with optional sidecar/provider services.

- Primary runtime:
  - crm-api (ASP.NET Core monolith)
  - crm-frontend (React SPA via Nginx)
- Primary data services:
  - MariaDB (single database policy)
  - Redis
- Optional OSS provider services:
  - Meilisearch, Chatwoot, Novu, Superset, DocuSeal, n8n, Ollama

Microservices code exists and can be deployed, but the production baseline remains the monolith path.

## High-Level Architecture

```
Users/Browsers
    |
    v
crm-frontend (React SPA)
    |
    v
crm-api (ASP.NET Core)
    |
    +--> crm-mariadb (System of record)
    +--> crm-redis (cache/session/realtime helpers)
    +--> provider ports resolved through factories
          |
          +--> Meilisearch (search)
          +--> Chatwoot (chat)
          +--> Novu (notifications)
          +--> Superset (analytics)
          +--> DocuSeal (e-signatures)
          +--> n8n (integrations/workflows)
          +--> Ollama (local AI)
```

## Architectural Style

- Core style: layered modular monolith
- Boundary style: ports and adapters for external capability providers
- Contract style: DTO-first API contracts with explicit mapping between entities and DTOs
- Persistence style: EF Core code-first with migrations

## Core Layers and Responsibilities

### 1) Presentation Layer

- React SPA in CRM.Frontend
- Route-level pages with service wrappers
- Context providers for auth/theme/realtime

### 2) API Layer

- ASP.NET Core host in CRM.Api
- REST controllers
- auth, validation, logging, exception middleware
- health endpoints and provider status endpoints

### 3) Domain/Application Layer

- Service classes in Core and Infrastructure
- business workflows for CRM, ITSM, marketing, and admin features
- event and background processing for selected features

### 4) Infrastructure Layer

- EF Core DbContext and mapping configuration
- provider factories and provider adapters
- external integrations and hosted services

## Data Architecture

- Database policy: single database (`crm_db`)
- Primary engine: MariaDB in deployment baseline
- EF Core model and migrations are the source of truth
- No direct DDL edits as normal workflow

Governance requirements:
- DTOs must remain aligned with entity and API response shapes
- enum changes must be reflected in specs and enum references
- field-gap remediation plan must be updated for model/contract drift

## Provider Architecture

Provider usage is controlled by feature flags and provider type settings.

### Capability Categories

- Search: BuiltIn, Meilisearch, other adapters
- Chat: BuiltIn, Chatwoot, other adapters
- Notifications: BuiltIn, Novu, other adapters
- Analytics: BuiltIn, Superset, other adapters
- Signatures: BuiltIn, DocuSeal, other adapters
- Integrations: BuiltIn, n8n, other adapters
- AI: Ollama and cloud providers

### Resolution Pattern

- Application depends on port interfaces
- Factory resolves concrete implementation using config
- Feature flags and provider settings decide path at runtime

## Deployment Topologies

### A) Monolith + OSS Providers (Current primary)

- frontend + api + mariadb + redis + selected/all provider containers
- most common deployment for internal environments

### B) Monolith only

- frontend + api + mariadb + redis
- built-in providers where applicable

### C) Microservices split

- gateway + domain services + shared/segmented data strategy
- available for staged adoption; not the default baseline

## Security and Reliability Controls

- JWT auth for API access
- role and permission controls in service layer
- rate limiting configurable by environment
- health checks for core and provider services
- restart policies and containerized isolation
- audit logging for key admin/system actions

## Known Operational Realities

- Migration drift can happen if historical migration state is inconsistent; repair via controlled updates to migration history and migration workflow
- Some third-party containers expose redirects or non-200 root responses while still being operational (for example onboarding/login redirects)
- Worker-type containers may not map cleanly to HTTP health semantics and need explicit healthcheck strategy

## Current Architecture Decision Summary

- Keep monolith as the default delivery and support path
- Use provider abstraction for external capability modularity
- Maintain strict schema governance under EF migrations
- Treat microservices as an incremental evolution path, not an immediate replacement path

## Related Documents

- docs/development/MICROSERVICES_ARCHITECTURE.md
- docs/development/SOLUTION_CONTEXT.md
- docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md
- docs/architecture/ADR-002-EF-Core-Schema-Management.md
- docs/11-specifications/SOLUTION_GAPS_REMEDIATION_PLAN.md
