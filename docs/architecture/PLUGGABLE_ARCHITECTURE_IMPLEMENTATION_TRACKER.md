# Pluggable Architecture Implementation Tracker

## Status Summary (March 2026)

Overall implementation status: operational and in active use for OSS provider deployments.

The provider architecture is implemented and used in runtime deployments where external providers are enabled.

## Capability Coverage

### Search

- BuiltIn: available
- Meilisearch: available and commonly used
- Other provider stubs/paths: present by module maturity

### Chat

- BuiltIn: available
- Chatwoot: available and operational

### Notifications

- BuiltIn: available
- Novu: available and operational
- Additional channel providers: partial by environment and credential readiness

### Analytics

- BuiltIn: available
- Superset: available and operational

### Signatures

- BuiltIn: available
- DocuSeal: available and operational

### Integrations

- BuiltIn: available
- n8n: available and operational

### AI

- Ollama: available and operational
- cloud AI providers: available by configuration and key readiness

## Runtime Selection Model

Provider selection is controlled by:
- FeatureManagement flags (`UseExternal*`)
- Providers configuration (`Providers__<Category>__Type` and provider settings)

## Current Maturity Notes

- architecture supports provider swapping with minimal consumer change
- operational success depends on env var completeness and provider bootstrap state
- health semantics vary across providers (HTTP apps vs worker processes)

## Open Operational Work (Ongoing)

- continue hardening of first-run bootstrap procedures for provider stacks
- keep healthcheck strategy aligned with container behavior
- maintain integration tests for provider-specific paths in critical workflows
- keep docs/specs updated when provider contracts or settings change

## Verification Checklist

Use this checklist during deployment validation:

1. feature flags match intended provider usage
2. provider type config matches deployed containers
3. required credentials/secrets are present
4. provider endpoints resolve from API container network
5. fallback behavior verified when provider is disabled or unavailable
6. health and functional endpoint checks pass

## Related Documents

- docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md
- docs/development/ARCHITECTURE_OVERVIEW.md
- docs/development/SOLUTION_CONTEXT.md
