# AI Agent: System Health Monitor

> **Persona:** System Admin / DevOps
> **Purpose:** Continuously watch API, DB, cache, and provider health; suggest fixes.

## What It Does
- Polls `/health` and `/health/providers` for status changes
- Tracks error rates (HTTP 5xx), latency spikes, and DB connectivity
- Checks Redis reachability (e.g., `crm-redis:6379`) and flags reconnect storms
- Monitors feature-flag toggles affecting providers
- Recommends remediation steps with command snippets

## Typical Recommendations
- Restart unhealthy service/container
- Clear stuck Hangfire jobs; restart workers
- Validate connection strings and DNS (e.g., redis hostname)
- Temporarily disable external provider flags if down
- Increase timeouts/retries for failing providers

## Usage
1) Open **Help → Agents → System Health Monitor** in the admin console (or API `/api/agents/health-monitor` if exposed).
2) Select scope (API only, DB/cache, external providers).
3) Run a health scan; review findings and suggested fixes.
4) Apply recommended actions; rerun scan to confirm green status.

## Inputs
- Optional: Provider overrides, timeout thresholds, target endpoints.

## Outputs
- Status summary (pass/fail per subsystem)
- Top 5 issues with severity
- Suggested commands and config keys to fix
- Links to troubleshooting docs

## Troubleshooting
- **Agent offline**: Ensure AI feature flag is enabled and provider configured.
- **False positives**: Tune thresholds (latency/error-rate) in agent settings.
- **Redis errors**: Confirm `Redis__ConnectionString=crm-redis:6379` and `AbortOnConnectFail=false`.
