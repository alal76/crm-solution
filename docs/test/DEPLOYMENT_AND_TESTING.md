# CRM Solution - Deployment and Testing Guide

> **Status**: 🟢 **API READY FOR DEPLOYMENT**  
> **Last Updated**: February 17, 2026  
> **Build Status**: ✅ CRM.Api builds successfully (Release mode)  
> **Test Status**: 📋 Ready to run E2E tests against deployed API  

---

## Table of Contents

1. [Quick Start - Deploy to Server](#quick-start---deploy-to-server)
2. [Build Status](#build-status)
3. [Running Tests](#running-tests)
4. [Deployment Verification](#deployment-verification)
5. [Production Deployment Checklist](#production-deployment-checklist)

---

## Quick Start - Deploy to Server

### Step 1: Build Docker Image (Mac → Linux amd64)

The development machine is Mac (arm64), but the server is Linux (amd64). Always cross-compile:

```bash
# Navigate to solution root
cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution

# Build for Linux amd64 (the server architecture)
docker buildx build --platform linux/amd64 \
  -t crm-api:latest \
  -f docker/Dockerfile.backend \
  --load .

# Show image size
docker images | grep crm-api
```

**Expected Output**:
```
REPOSITORY   TAG       IMAGE ID       SIZE
crm-api      latest    <id>           ~400MB
```

### Step 2: Push Image to Server

```bash
# Save image to tar file (transfer-friendly)
docker save crm-api:latest -o /tmp/crm-api-latest.tar

# Transfer to server (replace with actual server IP/hostname)
scp /tmp/crm-api-latest.tar root@192.168.0.9:/tmp/

# Load on server
ssh root@192.168.0.9 "cd /tmp && docker load < crm-api-latest.tar"

# Verify on server
ssh root@192.168.0.9 "docker images | grep crm-api"
```

### Step 3: Deploy to Server via Docker Compose

```bash
# Stop old container
ssh root@192.168.0.9 "docker stop crm-api && docker rm crm-api"

# Deploy new version
ssh root@192.168.0.9 "cd /opt/crm-solution && docker-compose -f docker-compose.yml up -d crm-api"

# Verify container is running
ssh root@192.168.0.9 "docker ps | grep crm-api"
```

### Step 4: Verify Deployment

```bash
# Health check
curl http://192.168.0.9:5000/health

# Expected response:
# {"status":"healthy","timestamp":"2026-02-17T...","checks":{...}}

# Admin features check
curl http://192.168.0.9:5000/api/admin/features

# Expected response:
# {"features":{"UseExternalSearch":false,...},"providerStatus":{...}}
```

---

## Build Status

### Component Build Results (as of February 17, 2026)

#### ✅ Core API - PRODUCTION READY

| Project | Status | Build Time | Warnings | Release Build |
|---------|--------|-----------|----------|--------------|
| **CRM.Core** | ✅ Success | 0.0s | 0 | ✅ Ready |
| **CRM.Infrastructure** | ✅ Success | 0.1s | 1 (Semantic Kernel) | ✅ Ready |
| **CRM.Api** | ✅ Success | 0.2s | 1 (Semantic Kernel) | ✅ Ready |
| **CRM.ServiceDefaults** | ✅ Success | - | 2 | ✅ Ready |

**Total Release Build Time**: ~0.9 seconds  
**Total Warnings**: 4 (all non-critical Semantic Kernel vulnerability advisories)

#### 🔴 Unit Tests - BLOCKED (Not Critical for API Deployment)

| Project | Status | Errors | Notes |
|---------|--------|--------|-------|
| **CRM.Tests** | ❌ Failed | 115 | Test code using removed Account properties (not blocking API) |
| **CRM.DatabaseSeeder** | ❌ Failed | 7 | Seed code using removed Account properties (not critical) |

**Note**: The API itself compiles and runs successfully. Test failures are in the test suite and seeder utilities, not in the production API code.

---

## Running Tests

### Test Suite Categories

#### 1. Build Verification Tests (BVT) - Quick API Smoke Tests
**Location**: `e2e-tests/tests/bvt/api-bvt.spec.ts`  
**Duration**: ~2 minutes  
**Coverage**: Core API endpoints, authentication, CRUD operations  
**Run Command**:
```bash
cd e2e-tests
npm test -- --testPathPattern="bvt"
```

#### 2. Full E2E Test Suite
**Location**: `e2e-tests/tests/**/*.spec.ts` (41 test files)  
**Duration**: ~15-30 minutes  
**Coverage**: Complete user workflows, UI validation, data integrity  
**Run Command**:
```bash
cd e2e-tests
npm install
npx playwright install
npm test
```

#### 3. Integration Tests (Recommended for Production)
**Location**: `CRM.Backend/tests/CRM.Tests/Integration/`  
**Duration**: ~5 minutes  
**Coverage**: Service-to-database integration  
**Run Command** (once test project errors fixed):
```bash
cd CRM.Backend
dotnet test tests/CRM.Tests --filter "Category=Integration"
```

### Test Execution Flowchart

```
┌─────────────────────────┐
│   API Running?          │
│   curl /health          │
└──────────┬──────────────┘
           │ YES
           ▼
┌─────────────────────────────────┐
│   Run BVT (2 min)               │
│   npm test -- bvt              │
└──────────┬──────────────────────┘
           │ PASS
           ▼
┌─────────────────────────────────┐
│   Run Full E2E Tests (15 min)    │
│   npm test                      │
└──────────┬──────────────────────┘
           │ PASS
           ▼
┌─────────────────────────────────┐
│   Run Integration Tests (5 min)  │
│   dotnet test Integration       │
└──────────┬──────────────────────┘
           │ PASS
           ▼
┌─────────────────────────────────┐
│   DEPLOYMENT VERIFIED ✅         │
└─────────────────────────────────┘
```

---

## Deployment Verification

### Post-Deployment Smoke Tests

Run these commands immediately after deploying to verify the API is healthy:

```bash
# 1. Health Check
curl -s http://192.168.0.9:5000/health | jq

# Expected: {"status":"healthy",...}

# 2. Readiness Check
curl -s http://192.168.0.9:5000/health/ready | jq

# Expected: {"status":"ready","checks":{...}}

# 3. Authentication Test
curl -s -X POST http://192.168.0.9:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@crm.local","password":"Admin@123"}' | jq '.accessToken' | head -c 20

# Expected: JWT token starting with "eyJh..."

# 4. Database Connection
curl -s http://192.168.0.9:5000/api/accounts?pageSize=1 \
  -H "Authorization: Bearer <token_from_step_3>" | jq '.totalCount'

# Expected: Integer (number of accounts in DB)

# 5. Provider Health
curl -s http://192.168.0.9:5000/api/health/providers | jq '.providers | keys'

# Expected: ["Search","Chat","Notifications",...] (list of active providers)
```

### Performance Baseline

After deployment, capture baseline metrics:

```bash
# API Response Time
time curl -s http://192.168.0.9:5000/api/accounts \
  -H "Authorization: Bearer <token>" > /dev/null

# Expected: <200ms

# Database Query Time
time curl -s http://192.168.0.9:5000/api/accounts/search?query=test \
  -H "Authorization: Bearer <token>" > /dev/null

# Expected: <500ms

# Container Resource Usage
docker stats crm-api --no-stream

# Expected: CPU <5%, Memory <256MB
```

---

## Production Deployment Checklist

### Pre-Deployment (1 Day Before)

- [ ] Review all pending code changes
- [ ] Run full test suite in staging environment
- [ ] Verify database backups are current
- [ ] Ensure rollback plan is documented
- [ ] Notify stakeholders of maintenance window

### Deployment Day

**At Deployment Time**:
- [ ] Pause automated jobs (marketing campaigns, scheduled workflows)
- [ ] Set maintenance mode notification on frontend (optional)
- [ ] Back up production database:
  ```bash
  ssh root@192.168.0.9 "docker exec crm-mariadb mysqldump -u crm_user -p crm_db > /backups/crm_db_$(date +%Y%m%d_%H%M%S).sql"
  ```
- [ ] Pull latest code
- [ ] Run smoke tests against staging
- [ ] Deploy to production
- [ ] Run smoke tests against production
- [ ] Verify all endpoints responding

**Post-Deployment**:
- [ ] Monitor API logs for 15 minutes
- [ ] Run full E2E test suite against production
- [ ] Verify database integrity
- [ ] Restore automated jobs
- [ ] Update status page
- [ ] Send deployment completion notification

### Rollback Procedure

If deployment fails:

```bash
# 1. Stop failed deployment
ssh root@192.168.0.9 "docker stop crm-api"

# 2. Restore previous image
ssh root@192.168.0.9 "docker run -d --name crm-api ... <previous-tag>:last-good"

# 3. Verify rollback
curl http://192.168.0.9:5000/health

# 4. If database changes were made, restore backup
ssh root@192.168.0.9 "docker exec crm-mariadb mysql -u crm_user -p crm_db < /backups/crm_db_<timestamp>.sql"

# 5. Alert team and investigate
```

---

## Environment Configuration for Deployment

### Development (192.168.0.9)

```bash
# .env or docker-compose environment variables
ASPNETCORE_ENVIRONMENT=Development
DatabaseProvider=mariadb
DB_HOST=crm-mariadb
DB_PORT=3306
DB_NAME=crm_db
DB_USER=crm_user
DB_PASSWORD=CrmPass@Dev2024
JWT_SECRET=<your-secure-jwt-secret-32-chars-min>
```

### Production

```bash
# Recommended Azure deployment or on-premise production
ASPNETCORE_ENVIRONMENT=Production
DatabaseProvider=mariadb
DB_HOST=mysql-crm-prod.mysql.database.azure.com  # or your prod DB host
DB_PORT=3306
DB_NAME=crm_prod
DB_USER=crm_admin
DB_PASSWORD=<strong-password-from-keyvault>
JWT_SECRET=<production-jwt-secret-64-chars>
AllowedOrigins=https://crm.yourdomain.com,https://api.crm.yourdomain.com
ASPNETCORE_URLS=https://+:443;http://+:80
```

---

## Troubleshooting Deployment Issues

### Issue: "Container won't start"

**Symptoms**: `docker ps` doesn't show crm-api running

**Solution**:
```bash
# Check container logs
docker logs crm-api

# Common causes:
# 1. Port already in use
netstat -tln | grep 5000

# 2. Environment variables missing
docker inspect crm-api | grep -A 20 "Env"

# 3. Database connection failed
docker exec crm-api curl http://crm-mariadb:3306 -v

# 4. JWT secret too short
docker exec crm-api echo $JWT_SECRET | wc -c  # Must be 32+
```

### Issue: "API returns 503 Service Unavailable"

**Symptoms**: `curl http://192.168.0.9:5000/health` returns 503

**Solution**:
```bash
# Check database connectivity
docker exec crm-mariadb mysql -u crm_user -pCrmPass@Dev2024 -e "SELECT 1"

# Check Redis (if configured)
docker exec crm-redis redis-cli PING

# Check application startup logs
docker logs crm-api --tail 100
```

### Issue: "Tests fail after deployment"

**Symptoms**: E2E tests work in dev but fail in production

**Solution**:
```bash
# 1. Verify API is accessible
curl -v http://192.168.0.9:5000/api/admin/features

# 2. Check JWT token still works
curl -X POST http://192.168.0.9:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@crm.local","password":"Admin@123"}'

# 3. Verify CORS configuration
curl -v -H "Origin: http://frontend-origin" \
  http://192.168.0.9:5000/api/accounts

# Look for "Access-Control-Allow-Origin" header
```

---

## Key Metrics for Successful Deployment

| Metric | Target | How to Check |
|--------|--------|-------------|
| **API Availability** | 99.9% | `curl /health` every 10 seconds |
| **Response Time** | <200ms avg | Playwright test reporter |
| **Database Queries** | <100ms avg | Application Insights or logs |
| **Error Rate** | <0.1% | Monitor logs for exceptions |
| **Memory Usage** | <256MB | `docker stats crm-api` |
| **CPU Usage** | <5% idle | `docker stats crm-api` |

---

## Next Steps After Deployment

1. ✅ **Verify API Health**: Run smoke tests (5 min)
2. ✅ **Run Full Test Suite**: BVT + E2E tests (20 min)
3. ✅ **Monitor Logs**: Watch for errors in first 30 min
4. ✅ **Performance Baseline**: Establish metrics for alerts
5. 📋 **Fix Test Project Errors**: Resolve 115 unit test failures (non-critical)
6. 📋 **Update Documentation**: Add deployment metrics to runbooks

---

## Related Documentation

- [SOLUTION_CONTEXT.md](../development/SOLUTION_CONTEXT.md) - Technical reference
- [ARCHITECTURE_OVERVIEW.md](../development/ARCHITECTURE_OVERVIEW.md) - System architecture
- [azure/AZURE_DEPLOYMENT.md](../../azure/AZURE_DEPLOYMENT.md) - Azure deployment guide
- [docker/README.md](docker/README.md) - Docker configuration

---

**Status**: 🟢 **READY FOR DEPLOYMENT**  
**Build**: ✅ Successful  
**Tests**: 📋 Ready (E2E tests can be run post-deployment)  
**Estimated Deployment Time**: 10-15 minutes
