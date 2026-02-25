# Workflow: Setup External Integration

> **Persona:** System Admin / Admin
> **Outcome:** External provider configured and validated

## Steps
1) **Pick provider**: Identify category (Search, Chat, Notifications, AI, Analytics, Signatures, Integrations) and target (e.g., Meilisearch, Chatwoot, Novu, OpenAI, DocuSeal, N8n).
2) **Get credentials**: Collect API keys, base URLs, webhook secrets from provider console.
3) **Configure appsettings / env**:
   - `Providers:{Category}:Type` → provider name
   - Set provider-specific section (`Url`, `ApiKey`, etc.)
   - Ensure feature flags `UseExternal{Category}` are set appropriately.
4) **Set secrets**: Use env vars or key vault; avoid committing secrets.
5) **Validate connectivity**: Call `/api/health/providers` and provider-specific ping.
6) **Enable module**: Toggle feature flag; restart API if needed.
7) **Test E2E**: Run sample action (e.g., send notification, create index, generate AI response, send signature envelope).
8) **Monitor**: Check logs for errors; set rate limits/timeouts.

## Quick Examples
- **Search (Meilisearch)**: `Providers:Search:Type=Meilisearch`, `Url=http://crm-meilisearch:7700`, `ApiKey=masterKey`
- **AI (OpenAI)**: `Providers:AI:Type=OpenAI`, `ApiKey=sk-...`, `Model=gpt-4o`
- **Notifications (Novu)**: `Providers:Notifications:Type=Novu`, `ApiKey=...`, `BaseUrl=http://crm-novu:3000`

## Troubleshooting
- **401/403**: Verify API key and scopes.
- **Connection refused**: Check container hostname/port; update `Redis__ConnectionString` and provider URLs.
- **Timeouts**: Increase provider timeout settings; confirm DNS resolution inside container.
- **Webhook failures**: Confirm public callback URL, firewall rules, and shared secret.
