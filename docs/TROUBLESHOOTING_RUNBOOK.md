# Troubleshooting Runbook

## CRM Solution Provider Troubleshooting Guide

This runbook provides step-by-step troubleshooting procedures for common issues with the CRM pluggable provider architecture.

---

## Table of Contents

1. [Quick Diagnostics](#quick-diagnostics)
2. [Provider Connection Issues](#provider-connection-issues)
3. [Search Provider Issues](#search-provider-issues)
4. [Notification Provider Issues](#notification-provider-issues)
5. [Chat Provider Issues](#chat-provider-issues)
6. [E-Signature Provider Issues](#e-signature-provider-issues)
7. [Analytics Provider Issues](#analytics-provider-issues)
8. [AI/LLM Provider Issues](#aillm-provider-issues)
9. [Performance Issues](#performance-issues)
10. [Recovery Procedures](#recovery-procedures)

---

## Quick Diagnostics

### Health Check Commands

```bash
# Check overall system health
curl http://localhost:5000/api/health

# Check provider health
curl http://localhost:5000/api/health/providers

# Check active feature flags
curl http://localhost:5000/api/admin/features

# View recent logs
docker logs crm-api --tail 200 | grep -E "(Error|Warning|Exception)"
```

### Common Status Codes

| Code | Meaning | Action |
|------|---------|--------|
| `200` | Healthy | No action needed |
| `503` | Provider unavailable | Check provider connectivity |
| `500` | Internal error | Check logs for stack trace |
| `401` | Authentication failed | Verify API keys |
| `429` | Rate limited | Reduce request rate |

---

## Provider Connection Issues

### Symptoms
- Health check returns `503` for a provider
- Timeout errors in logs
- `Connection refused` errors

### Diagnostic Steps

1. **Verify provider is running**
   ```bash
   # Docker
   docker ps | grep <provider-name>
   
   # Kubernetes
   kubectl get pods -l app=<provider-name>
   ```

2. **Test network connectivity**
   ```bash
   # From CRM container
   docker exec crm-api ping <provider-hostname>
   docker exec crm-api curl -v http://<provider>:<port>/health
   ```

3. **Check configuration**
   ```bash
   # View environment variables
   docker exec crm-api env | grep -i "Providers"
   ```

4. **Verify API credentials**
   ```bash
   # Test API key directly
   curl -H "Authorization: Bearer <api-key>" http://<provider>/api/test
   ```

### Resolution

| Issue | Resolution |
|-------|------------|
| Provider not running | Start the provider container/service |
| Network unreachable | Check Docker network configuration |
| DNS resolution failed | Verify hostname in config matches service name |
| Wrong port | Update port in configuration |
| Invalid API key | Regenerate and update API key |

---

## Search Provider Issues

### Issue: Search Returns No Results

**Symptoms:** Queries return empty results even when data exists

**Diagnostic Steps:**

1. Check if index exists
   ```bash
   # Meilisearch
   curl http://meilisearch:7700/indexes
   
   # Algolia
   # Check via Algolia dashboard
   ```

2. Verify documents are indexed
   ```bash
   # Meilisearch
   curl http://meilisearch:7700/indexes/crm_accounts/stats
   ```

3. Check index sync status
   ```bash
   curl http://localhost:5000/api/admin/search/status
   ```

**Resolution:**
- Trigger reindex: `POST /api/admin/search/reindex`
- Check index mapping matches entity schema
- Verify searchable attributes configuration

### Issue: Search Index Out of Sync

**Symptoms:** New/updated records not appearing in search

**Diagnostic Steps:**

1. Check indexing queue
   ```bash
   # View pending index operations
   docker logs crm-api | grep "IndexAsync"
   ```

2. Check for indexing errors
   ```bash
   docker logs crm-api | grep -E "(index|Index).*(error|Error|failed)"
   ```

**Resolution:**
```bash
# Full reindex
curl -X POST http://localhost:5000/api/admin/search/reindex \
  -H "Authorization: Bearer $TOKEN"

# Reindex specific entity type
curl -X POST http://localhost:5000/api/admin/search/reindex/accounts \
  -H "Authorization: Bearer $TOKEN"
```

### Issue: Slow Search Performance

**Diagnostic Steps:**

1. Check response times
   ```bash
   curl -w "@curl-format.txt" "http://localhost:5000/api/search?q=test"
   ```

2. Check index size
   ```bash
   # Meilisearch
   curl http://meilisearch:7700/stats
   ```

**Resolution:**
- Review and optimize searchable attributes
- Increase Meilisearch memory allocation
- Consider using Algolia for large datasets

---

## Notification Provider Issues

### Issue: Emails Not Sending

**Diagnostic Steps:**

1. Check notification logs
   ```bash
   docker logs crm-api | grep -E "(Email|Notification|Send)"
   ```

2. Verify SMTP/provider settings
   ```bash
   # Test SMTP connection
   docker exec crm-api /bin/bash -c "nc -zv smtp.example.com 587"
   ```

3. Check provider health
   ```bash
   curl http://localhost:5000/api/health/providers | jq '.notifications'
   ```

**Resolution by Provider:**

**BuiltIn (SMTP):**
- Verify SMTP credentials
- Check if port 587/465 is open
- Test with different SMTP server

**Novu:**
- Verify workflow is active
- Check subscriber exists
- Review Novu dashboard for delivery status

**Twilio/SendGrid:**
- Check account balance
- Verify sender is verified
- Check for rate limiting

### Issue: SMS Not Delivering

**Diagnostic Steps:**

1. Verify phone number format
   ```bash
   # Must be E.164 format: +1234567890
   ```

2. Check Twilio logs
   ```bash
   # Via Twilio Console or API
   curl -u "$TWILIO_SID:$TWILIO_TOKEN" \
     "https://api.twilio.com/2010-04-01/Accounts/$TWILIO_SID/Messages.json?PageSize=10"
   ```

**Resolution:**
- Verify recipient number is valid
- Check country permissions in Twilio
- Ensure sender number is SMS-capable

---

## Chat Provider Issues

### Issue: Chat Messages Not Syncing

**Symptoms:** Messages in Chatwoot/Intercom not appearing in CRM timeline

**Diagnostic Steps:**

1. Check webhook configuration
   ```bash
   # Verify webhook URL is correct in Chatwoot
   # Webhook URL should be: https://crm.example.com/api/webhooks/chatwoot
   ```

2. Check webhook delivery
   ```bash
   # In Chatwoot: Settings > Integrations > Webhooks
   # Check delivery status
   ```

3. Check CRM webhook logs
   ```bash
   docker logs crm-api | grep -E "(webhook|Webhook|chat)"
   ```

**Resolution:**
- Update webhook URL in Chatwoot
- Verify webhook secret matches
- Check firewall allows inbound webhook traffic

### Issue: Contact Not Linking

**Symptoms:** Chat contacts not matching CRM contacts

**Diagnostic Steps:**

1. Check contact matching criteria
   ```bash
   # CRM matches by email first, then phone
   docker logs crm-api | grep "ContactMatcher"
   ```

**Resolution:**
- Ensure email is captured in chat
- Manually link contact via CRM UI
- Check for duplicate contacts

---

## E-Signature Provider Issues

### Issue: Signature Request Not Created

**Diagnostic Steps:**

1. Check provider health
   ```bash
   curl http://localhost:5000/api/health/providers | jq '.signatures'
   ```

2. Verify template exists
   ```bash
   # DocuSeal
   curl http://docuseal:3000/api/templates \
     -H "X-Auth-Token: $DOCUSEAL_KEY"
   ```

3. Check request payload
   ```bash
   docker logs crm-api | grep "CreateSignatureRequest"
   ```

**Resolution:**
- Verify template ID is correct
- Check document format is supported (PDF)
- Ensure all required signers are provided

### Issue: Webhook Not Receiving Events

**Diagnostic Steps:**

1. Verify webhook URL in provider
   ```bash
   # DocuSign Connect webhook
   # DocuSeal webhook settings
   ```

2. Test webhook endpoint
   ```bash
   curl -X POST http://localhost:5000/api/webhooks/docusign \
     -H "Content-Type: application/json" \
     -d '{"event": "test"}'
   ```

**Resolution:**
- Update webhook URL in provider settings
- Verify SSL certificate is valid (required for DocuSign)
- Check HMAC signature validation

---

## Analytics Provider Issues

### Issue: Dashboard Not Loading

**Diagnostic Steps:**

1. Check Superset/Power BI health
   ```bash
   # Superset
   curl http://superset:8088/health
   
   # Power BI - check token
   curl http://localhost:5000/api/health/providers | jq '.analytics'
   ```

2. Verify guest token generation (Superset)
   ```bash
   docker logs crm-api | grep "GuestToken"
   ```

3. Check embedding configuration
   ```bash
   # Superset: PUBLIC_ROLE_LIKE_GAMMA must be True
   # Power BI: Workspace must have embedding enabled
   ```

**Resolution:**
- Regenerate guest token
- Verify dashboard ID is correct
- Check CORS configuration allows embedding

### Issue: Data Not Refreshing

**Diagnostic Steps:**

1. Check data source connection
   ```bash
   # Superset: Test database connection
   # Power BI: Check gateway status
   ```

2. Verify sync schedule
   ```bash
   # Check if ETL/sync is running
   ```

**Resolution:**
- Refresh database credentials
- Trigger manual refresh
- Check ETL job status

---

## AI/LLM Provider Issues

### Issue: Slow Response Times

**Diagnostic Steps:**

1. Check model load status (Ollama)
   ```bash
   curl http://ollama:11434/api/tags
   ```

2. Monitor resource usage
   ```bash
   docker stats ollama
   ```

3. Check request queue
   ```bash
   docker logs ollama | grep "queue"
   ```

**Resolution:**
- Use smaller model (llama3:8b vs llama3:70b)
- Increase memory allocation
- Add request timeout configuration

### Issue: Model Not Found (Azure OpenAI)

**Diagnostic Steps:**

1. Verify deployment exists
   ```bash
   # Check Azure Portal > Azure OpenAI > Deployments
   ```

2. Check API version
   ```bash
   # Ensure ApiVersion matches deployed model capabilities
   ```

**Resolution:**
- Create model deployment in Azure Portal
- Update DeploymentName in configuration
- Use correct API version for model

### Issue: Rate Limiting

**Symptoms:** `429 Too Many Requests` errors

**Resolution:**
- Implement request queuing
- Increase tier/quota in provider dashboard
- Add exponential backoff retry logic

---

## Performance Issues

### Issue: High Latency

**Diagnostic Steps:**

1. Identify slow providers
   ```bash
   curl http://localhost:5000/api/health/providers | jq '.[] | {name, responseTime}'
   ```

2. Check for network issues
   ```bash
   # Measure latency to provider
   docker exec crm-api time curl -o /dev/null -s http://meilisearch:7700/health
   ```

3. Review application metrics
   ```bash
   curl http://localhost:5000/metrics | grep "provider_latency"
   ```

**Resolution:**
- Enable provider response caching
- Move to geographically closer region
- Upgrade to faster tier

### Issue: Memory Issues

**Diagnostic Steps:**

1. Check container memory
   ```bash
   docker stats crm-api
   ```

2. Review GC metrics
   ```bash
   curl http://localhost:5000/metrics | grep "gc_"
   ```

**Resolution:**
- Increase memory limits
- Review and optimize large data operations
- Enable memory-efficient streaming

---

## Recovery Procedures

### Provider Failover to BuiltIn

If an external provider fails and you need to fall back to BuiltIn:

1. **Temporary Override**
   ```bash
   # Update environment variable
   docker exec crm-api /bin/bash -c \
     "export FeatureManagement__UseExternalSearch=false && dotnet CRM.Api.dll"
   ```

2. **Permanent Change**
   ```bash
   # Update docker-compose or Kubernetes config
   FeatureManagement__UseExternalSearch: "false"
   
   # Restart
   docker-compose restart crm-api
   ```

### Reindex All Data

```bash
# Trigger full reindex for all entity types
curl -X POST http://localhost:5000/api/admin/search/reindex/all \
  -H "Authorization: Bearer $TOKEN"

# Monitor progress
curl http://localhost:5000/api/admin/search/reindex/status
```

### Reset Provider Connection

```bash
# Force reconnection
curl -X POST http://localhost:5000/api/admin/providers/reset \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"provider": "Meilisearch"}'
```

### Clear Provider Cache

```bash
curl -X DELETE http://localhost:5000/api/admin/cache/providers \
  -H "Authorization: Bearer $TOKEN"
```

---

## Escalation

If issues persist after following this runbook:

1. **Collect diagnostic information:**
   ```bash
   # Export logs
   docker logs crm-api > crm-api.log 2>&1
   
   # Export health status
   curl http://localhost:5000/api/health/providers > health.json
   
   # Export configuration (redact secrets)
   docker exec crm-api env | grep -E "^(Providers|FeatureManagement)" > config.txt
   ```

2. **Open support ticket** with:
   - Diagnostic files
   - Steps to reproduce
   - Expected vs actual behavior

---

**Last Updated:** 2024-02-05  
**Version:** 1.0.0
