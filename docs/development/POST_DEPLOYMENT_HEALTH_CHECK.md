# Post-Deployment Health Check Module

> **Version:** 1.0  
> **Last Updated:** March 16, 2026

## Overview

The Post-Deployment Health Check module performs comprehensive verification after CRM deployments to ensure:

1. **Schema Completion** - All expected database tables exist
2. **Connectivity** - Database, Redis, and network are accessible
3. **API Endpoints** - All required endpoints are responding
4. **Pluggable Providers** - Configured providers are healthy
5. **Initial Configuration** - Admin user, seed data, and settings exist

## Quick Start

### Basic Usage (Local)

```bash
# Check localhost deployment
./scripts/post-deployment-health-check.sh

# Check specific host
./scripts/post-deployment-health-check.sh -h api.example.com -p 5000
```

### Remote Server via SSH

```bash
# Check dev server
./scripts/post-deployment-health-check.sh -s root@192.168.0.9

# Check with verbose output
./scripts/post-deployment-health-check.sh -s root@192.168.0.9 --verbose
```

### JSON Output (for CI/CD)

```bash
# Output as JSON for parsing
./scripts/post-deployment-health-check.sh --json

# Parse with jq
./scripts/post-deployment-health-check.sh --json | jq '.summary'
```

## Command Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `-h, --host HOST` | Target API host | `localhost` |
| `-p, --port PORT` | API port | `5000` |
| `-s, --ssh USER@HOST` | Run via SSH on remote host | - |
| `--db-host HOST` | Database container name | `crm-mariadb` |
| `--db-user USER` | Database username | `crm_user` |
| `--db-pass PASS` | Database password | `CrmPass@Dev2024` |
| `--db-name NAME` | Database name | `crm_db` |
| `--skip-schema` | Skip schema validation | `false` |
| `--skip-providers` | Skip provider health checks | `false` |
| `--json` | Output results as JSON | `false` |
| `--verbose` | Show detailed output | `false` |
| `--help` | Show help message | - |

## Health Checks Performed

### 1. Docker Container Status
- Checks if required containers are running: `crm-api`, `crm-mariadb`, `crm-redis`, `crm-frontend`
- Verifies container health status

### 2. Database Connectivity
- Tests connection to MariaDB/MySQL
- Verifies database version

### 3. Redis Connectivity
- Tests Redis PING response
- Checks database size

### 4. Network Connectivity
- Verifies API can reach database container
- Verifies API can reach Redis container
- Checks Docker network existence

### 5. Schema Validation
- Counts total tables in database
- Verifies all expected core tables exist:
  - User management: `Users`, `UserGroups`, `UserGroupMembers`, `UserProfiles`, `Departments`
  - CRM entities: `Accounts`, `Contacts`, `Leads`, `Opportunities`, `Products`
  - Sales: `Quotes`, `Orders`, `Invoices`
  - Marketing: `MarketingCampaigns`, `EmailTemplates`, `EmailSequences`
  - Service Desk: `ServiceRequests`, `ServiceRequestCategories`, `KnowledgeArticles`
  - System: `SystemSettings`, `Tasks`, `Notes`, `AIAgents`
- Checks for applied EF Core migrations

### 6. API Health Endpoints
- `/health` - Basic health check
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe
- `/api/health/providers` - Provider health

### 7. API CRUD Endpoints
- Verifies all core API endpoints respond (returns 200 or 401)
- Checks: accounts, contacts, leads, opportunities, products, campaigns, servicerequests, users, usergroups, settings, dashboard

### 8. Provider Health
- Checks each pluggable provider category:
  - Search (Meilisearch, Algolia, etc.)
  - Chat (Chatwoot, Intercom, etc.)
  - Notifications (Novu, Twilio, etc.)
  - Analytics (Superset, PowerBI, etc.)
  - Signatures (DocuSeal, DocuSign, etc.)
  - AI (Ollama, OpenAI, etc.)
  - Integrations (N8n, Zapier, etc.)

### 9. Seed Data Verification
- Checks for admin user (Role = 0)
- Checks for SysAdmin group (IsSystemAdmin = 1)
- Verifies system settings exist
- Checks service request categories

### 10. Authentication Flow
- Tests login endpoint responds correctly
- Optionally tests default admin credentials

### 11. API Documentation
- Checks Swagger UI availability
- Verifies OpenAPI spec endpoint

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | All checks passed |
| `1` | One or more checks failed |

## Example Output

### Terminal Output

```
═══════════════════════════════════════════════════════════════════
  CRM Solution - Post-Deployment Health Check
═══════════════════════════════════════════════════════════════════

  Target: root@192.168.0.9
  Database: crm-mariadb / crm_db
  Started: Tue Feb 17 18:43:10 CET 2026

▶ Docker Container Status
───────────────────────────────────────────────────────────────────
  ✓ Container crm-api
  ✓ Container crm-mariadb
  ✓ Container crm-redis
  ✓ Container crm-frontend

▶ Database Connectivity
───────────────────────────────────────────────────────────────────
  ✓ Database connection
  ✓ Database version

...

═══════════════════════════════════════════════════════════════════
  Health Check Summary
═══════════════════════════════════════════════════════════════════

  Target: root@192.168.0.9
  Time: Tue Feb 17 18:43:19 CET 2026

  Passed:   57
  Failed:   0
  Warnings: 2
  Total:    59

  ═══════════════════════════════════════════════════════════════════
    ✓ ALL HEALTH CHECKS PASSED - Deployment is healthy!
  ═══════════════════════════════════════════════════════════════════
```

### JSON Output

```json
{
  "timestamp": "2026-02-17T17:43:19Z",
  "target": "root@192.168.0.9",
  "summary": {
    "total": 59,
    "passed": 57,
    "failed": 0,
    "warnings": 2
  },
  "overallHealthy": true,
  "results": [
    {"check": "Container crm-api", "status": "pass", "details": "Status: running, Health: healthy"},
    ...
  ]
}
```

## Integration with CI/CD

### GitHub Actions

```yaml
- name: Post-Deployment Health Check
  run: |
    ./scripts/post-deployment-health-check.sh \
      -s ${{ secrets.DEPLOY_HOST }} \
      --json > health-check-results.json
    
    # Check if deployment is healthy
    if [ "$(jq -r '.overallHealthy' health-check-results.json)" != "true" ]; then
      echo "::error::Deployment health check failed"
      jq -r '.results[] | select(.status == "fail") | .check' health-check-results.json
      exit 1
    fi

- name: Upload Health Check Report
  uses: actions/upload-artifact@v3
  with:
    name: health-check-report
    path: health-check-results.json
```

### Azure DevOps

```yaml
- script: |
    ./scripts/post-deployment-health-check.sh \
      -s $(DEPLOY_HOST) \
      --json > $(Build.ArtifactStagingDirectory)/health-check.json
  displayName: 'Run Post-Deployment Health Check'
  
- task: PublishBuildArtifacts@1
  inputs:
    pathtoPublish: '$(Build.ArtifactStagingDirectory)/health-check.json'
    artifactName: 'health-check-report'
```

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| Connection refused | Ensure API container is running and port is exposed |
| SSH timeout | Check SSH access and firewall rules |
| Missing tables | Run database migration: `docker exec crm-api dotnet ef database update` |
| No admin user | Seed data: `docker exec crm-api dotnet CRM.Api.dll --seed` |
| Provider unhealthy | Check provider configuration in appsettings.json |

### Debug Mode

```bash
# Run with bash debug mode
bash -x ./scripts/post-deployment-health-check.sh -s root@192.168.0.9 --verbose
```

## Related Files

- [scripts/post-deployment-health-check.sh](../scripts/post-deployment-health-check.sh) - Main health check script
- [scripts/expected-schema-tables.txt](../scripts/expected-schema-tables.txt) - Full list of expected tables
- [docs/development/SOLUTION_CONTEXT.md](SOLUTION_CONTEXT.md) - Full solution documentation
- [.github/copilot-instructions.md](../../.github/copilot-instructions.md) - Development guidelines

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-02-17 | Initial release |
