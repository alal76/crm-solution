# CRM Solution - Parameterized CI/CD Guide

This guide explains how to use the new parameterized build and deployment system that eliminates the need to customize scripts for each deployment instance.

## Overview

The parameterized CI/CD system allows you to:
- ✅ Deploy to any environment without modifying scripts
- ✅ Choose architecture mode (monolithic vs microservices)
- ✅ Select components to build and deploy
- ✅ Deploy to multiple platforms (Docker, Kubernetes, Cloud)
- ✅ Use environment-specific configurations
- ✅ Run in dry-run mode to preview changes

## Quick Start

### 1. Configure Your Environment

Copy the example environment file and customize it:

```bash
cp .env.example .env
nano .env  # Edit with your settings
```

Key variables to set:
```bash
# Architecture
ARCHITECTURE_MODE=monolithic  # or microservices

# Components
DEPLOY_FRONTEND=true
DEPLOY_API=true
DEPLOY_DATABASE=true
DEPLOY_REDIS=true

# Deployment
DEPLOY_PLATFORM=docker  # docker, kubernetes, or vm
CLOUD_PROVIDER=none    # none, aws, azure, or gcp
TARGET_ENV=development  # development, staging, or production

# Credentials (use actual values)
JWT_SECRET=<your-jwt-secret-32-chars-minimum>
DB_PASSWORD=<your-database-password>
ADMIN_PASSWORD=<your-admin-password>
```

### 2. Build Your Application

Use the parameterized build script:

```bash
# Build with defaults from .env
./scripts/build-parameterized.sh

# Build specific architecture
./scripts/build-parameterized.sh --arch microservices

# Build with specific build type
./scripts/build-parameterized.sh --build-type dev
./scripts/build-parameterized.sh --build-type test
./scripts/build-parameterized.sh --build-type integration
./scripts/build-parameterized.sh --build-type production

# Build for production
./scripts/build-parameterized.sh --env production --build-type production --tag v1.0.0

# Build and push to registry
./scripts/build-parameterized.sh --registry ghcr.io/myorg --push --tag latest

# Quick build without tests
./scripts/build-parameterized.sh --skip-tests --build-type dev
```

## Build Types

The system supports four build types, each optimized for different purposes:

### Development Build (`--build-type dev`)

**Purpose:** Extensive logging for identifying and fixing bugs

**Characteristics:**
- ✅ Debug .NET configuration
- ✅ Verbose logging (Debug level)
- ✅ Source maps enabled
- ✅ No minification
- ✅ Console statements retained
- ✅ SQL query logging enabled
- ✅ Request/response logging
- ✅ Performance metrics
- ✅ Detailed error messages with stack traces

**Use Cases:**
- Local development
- Debugging issues
- Understanding application flow
- Performance analysis

**Example:**
```bash
./scripts/build-parameterized.sh --build-type dev --arch monolithic
```

### Test Build (`--build-type test`)

**Purpose:** Full instrumentation for QA activities

**Characteristics:**
- ✅ Debug .NET configuration
- ✅ Debug logging
- ✅ Source maps enabled
- ✅ Code coverage hooks
- ✅ Performance monitoring
- ✅ Test instrumentation
- ✅ Test data generation
- ✅ Separate performance log file

**Use Cases:**
- Quality assurance testing
- Automated testing
- Performance benchmarking
- Code coverage analysis
- Integration testing

**Example:**
```bash
./scripts/build-parameterized.sh --build-type test --env staging
```

### Integration Build (`--build-type integration`)

**Purpose:** Focus on external integrations and API testing

**Characteristics:**
- ✅ Release .NET configuration
- ✅ Information log level (Debug for APIs)
- ✅ Source maps enabled
- ✅ API and middleware logging
- ✅ External API call logging
- ✅ HTTP request/response logging
- ✅ Separate API log file
- ✅ Circuit breaker and retry logging

**Use Cases:**
- Testing external integrations
- API endpoint testing
- Third-party service integration
- Middleware debugging
- OAuth/authentication flows

**Example:**
```bash
./scripts/build-parameterized.sh --build-type integration --arch microservices
```

### Production Build (`--build-type production`)

**Purpose:** Optimized, clean, fast code for production deployment

**Characteristics:**
- ✅ Release .NET configuration
- ✅ Warning/Error log level only
- ✅ Source maps disabled
- ✅ Full minification
- ✅ Console statements removed
- ✅ Minimal instrumentation
- ✅ Optimized bundle sizes
- ✅ No sensitive data logging

**Use Cases:**
- Production deployments
- Performance-critical environments
- Public-facing applications
- Minimal resource usage

**Example:**
```bash
./scripts/build-parameterized.sh --build-type production --env production --push --tag v1.2.3
```

## Build Type Comparison

| Feature | Dev | Test | Integration | Production |
|---------|-----|------|-------------|------------|
| .NET Configuration | Debug | Debug | Release | Release |
| Log Level | Debug | Debug | Information | Warning |
| Source Maps | ✅ | ✅ | ✅ | ❌ |
| Minification | ❌ | ❌ | ✅ | ✅ |
| Console Logging | Full | Full | Partial | None |
| SQL Query Logging | ✅ | ✅ | ❌ | ❌ |
| Code Coverage | ❌ | ✅ | ❌ | ❌ |
| Performance Monitoring | ✅ | ✅ | ✅ | ❌ |
| API Logging | Basic | Basic | Detailed | Minimal |
| Optimization Level | None | Basic | Standard | Aggressive |



```bash
# Build with defaults from .env
./scripts/build-parameterized.sh

# Build specific architecture
./scripts/build-parameterized.sh --arch microservices

# Build for production
./scripts/build-parameterized.sh --env production --tag v1.0.0

# Build and push to registry
./scripts/build-parameterized.sh --registry ghcr.io/myorg --push --tag latest

# Quick build without tests
./scripts/build-parameterized.sh --skip-tests
```

### 3. Deploy Your Application

Use the parameterized deployment script:

```bash
# Deploy to local Docker
./scripts/deploy-parameterized.sh --platform docker --arch monolithic

# Deploy microservices to Kubernetes
./scripts/deploy-parameterized.sh \
  --platform kubernetes \
  --arch microservices \
  --env production \
  --namespace crm-prod

# Deploy to AWS EKS
./scripts/deploy-parameterized.sh \
  --platform kubernetes \
  --cloud aws \
  --env production \
  --location us-east-1

# Dry run (preview without executing)
./scripts/deploy-parameterized.sh --platform docker --dry-run
```

## Configuration Files

### `.env` - Environment Configuration
Contains all deployment-specific variables. Never commit this file to version control!

### `.cicd-config.yml` - CI/CD Configuration
Central configuration file defining:
- Architecture modes
- Component definitions
- Build options
- Deployment targets
- Cloud provider settings
- Testing preferences
- Security policies

## Common Deployment Scenarios

### Scenario 1: Local Development (Monolithic)

```bash
# In .env
ARCHITECTURE_MODE=monolithic
DEPLOY_PLATFORM=docker
TARGET_ENV=development

# Build and deploy
./scripts/build-parameterized.sh
./scripts/deploy-parameterized.sh --platform docker --arch monolithic
```

Access:
- Frontend: http://localhost:3000
- API: http://localhost:5000

### Scenario 2: Production Microservices on Kubernetes

```bash
# In .env
ARCHITECTURE_MODE=microservices
DEPLOY_PLATFORM=kubernetes
TARGET_ENV=production
PROD_DOMAIN=crm.company.com
K8S_NAMESPACE=crm-prod

# Build with version tag
./scripts/build-parameterized.sh \
  --arch microservices \
  --env production \
  --tag v1.2.3 \
  --push

# Deploy to Kubernetes
./scripts/deploy-parameterized.sh \
  --platform kubernetes \
  --arch microservices \
  --env production \
  --namespace crm-prod \
  --domain crm.company.com
```

### Scenario 3: AWS Cloud Deployment

```bash
# In .env
CLOUD_PROVIDER=aws
AWS_REGION=us-east-1
AWS_EKS_CLUSTER=crm-cluster
ARCHITECTURE_MODE=microservices

# Deploy to AWS
./scripts/deploy-parameterized.sh \
  --platform kubernetes \
  --cloud aws \
  --env production \
  --location us-east-1
```

### Scenario 4: Multi-Environment CI/CD

Set up different environment files:
- `.env.development` - Local dev settings
- `.env.staging` - Staging environment
- `.env.production` - Production settings

```bash
# Load specific environment
export $(cat .env.staging | grep -v '^#' | xargs)

# Build and deploy for staging
./scripts/build-parameterized.sh --env staging --tag staging-latest --push
./scripts/deploy-parameterized.sh --env staging
```

## GitHub Actions Integration

### Manual Workflow Trigger

1. Go to Actions tab in GitHub
2. Select "Parameterized CI/CD" workflow
3. Click "Run workflow"
4. Choose options:
   - Architecture: monolithic or microservices
   - Environment: development, staging, or production
   - Platform: docker or kubernetes
   - Cloud Provider: none, aws, azure, or gcp
   - Skip Tests: optional

### Automatic Triggers

The workflow automatically runs on:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop`

Default behavior:
- Builds monolithic architecture
- Runs all tests
- Creates Docker images
- Deploys on `main` branch only

## Azure DevOps Integration

### Using Pipeline Variables

Configure in Azure DevOps:

```yaml
variables:
  ARCHITECTURE_MODE: 'microservices'
  TARGET_ENV: 'staging'
  DEPLOY_PLATFORM: 'kubernetes'
  CLOUD_PROVIDER: 'azure'
```

## Environment Variables Reference

### Required Variables

```bash
# Authentication
JWT_SECRET=<32+ character secret>
DB_PASSWORD=<strong password>
ADMIN_PASSWORD=<strong password>

# Database
DATABASE_PROVIDER=mariadb  # mariadb, mysql, postgres, sqlserver
DB_HOST=crm-mariadb
DB_PORT=3306
DB_NAME=crm_db
DB_USER=crm_user
```

### Optional Variables

```bash
# Redis Cache
REDIS_CONNECTION_STRING=crm-redis:6379
REDIS_ENABLED=true

# Build Options
BUILD_OPTIMIZATION=Release
SKIP_TESTS=false
BUILD_CACHE_ENABLED=true

# Deployment
DEPLOY_FRONTEND=true
DEPLOY_API=true
DEPLOY_DATABASE=true
DEPLOY_REDIS=true

# Microservices (when ARCHITECTURE_MODE=microservices)
DEPLOY_GATEWAY=true
DEPLOY_IDENTITY=true
DEPLOY_CUSTOMER=true
DEPLOY_SALES=true
DEPLOY_MARKETING=true
DEPLOY_SERVICEDESK=true
DEPLOY_CORE=true

# Cloud - AWS
AWS_REGION=us-east-1
AWS_EKS_CLUSTER=crm-cluster
AWS_ACCOUNT_ID=<your-account-id>

# Cloud - Azure
AZURE_SUBSCRIPTION_ID=<subscription-id>
AZURE_RESOURCE_GROUP=crm-solution-rg
AZURE_LOCATION=eastus
AZURE_AKS_CLUSTER=crm-cluster
AZURE_ACR_NAME=crmacr

# Cloud - GCP
GCP_PROJECT_ID=<project-id>
GCP_REGION=us-central1
GCP_GKE_CLUSTER=crm-cluster

# URLs and Domains
FRONTEND_URL=http://localhost:3000
STAGING_DOMAIN=staging.crm.local
PROD_DOMAIN=crm.company.com

# Networking
FRONTEND_PORT=3000
API_PORT=5000
DATABASE_PORT=3306
REDIS_PORT=6379
```

## Troubleshooting

### Build Issues

```bash
# Verify configuration
cat .env

# Test with dry run
./scripts/build-parameterized.sh --dry-run

# Build with verbose output
./scripts/build-parameterized.sh --verbose  # if implemented

# Clean build
docker system prune -a
./scripts/build-parameterized.sh
```

### Deployment Issues

```bash
# Check configuration
./scripts/deploy-parameterized.sh --dry-run

# Verify Docker Compose file
docker compose -f docker/docker-compose.unified.yml config

# Check Kubernetes manifests
kubectl apply -f kubernetes/ --dry-run=client

# View logs
docker compose logs -f  # for Docker
kubectl logs -n crm-prod -l app=crm-api  # for Kubernetes
```

### Common Problems

1. **"JWT_SECRET not set" error**
   - Solution: Set JWT_SECRET in .env (minimum 32 characters)

2. **"Connection refused" errors**
   - Solution: Ensure all services are healthy
   - Check: `docker compose ps` or `kubectl get pods`

3. **Image pull errors**
   - Solution: Login to registry first
   - `docker login ghcr.io` or configure cloud credentials

4. **Permission denied on scripts**
   - Solution: `chmod +x scripts/*.sh`

## Best Practices

1. **Never commit secrets**
   - Use `.env` files (already in `.gitignore`)
   - Use secrets management for production

2. **Use version tags**
   - Tag images with version numbers
   - Example: `--tag v1.2.3` or `--tag $(date +%Y%m%d)`

3. **Test in staging first**
   - Deploy to staging before production
   - Run smoke tests

4. **Use dry-run mode**
   - Preview changes before applying
   - Verify configuration

5. **Monitor deployments**
   - Check health endpoints after deployment
   - Review logs for errors

6. **Backup before production deploys**
   - Backup database
   - Tag current production images

## Migration from Old Scripts

If you're migrating from the old deployment scripts:

1. Review your current deployment commands
2. Identify hardcoded values
3. Move them to `.env` file
4. Test with new scripts using `--dry-run`
5. Perform actual deployment
6. Document any custom steps

Example migration:

**Old approach:**
```bash
# Edit script file to change IP
vim scripts/deploy-192.168.0.9.sh
./scripts/deploy-192.168.0.9.sh
```

**New approach:**
```bash
# Set in .env
echo "DEPLOY_LOCATION=192.168.0.9" >> .env
./scripts/deploy-parameterized.sh --platform docker --location 192.168.0.9
```

## Support

For issues or questions:
1. Check this documentation
2. Review `.cicd-config.yml` for available options
3. Use `--help` flag on scripts
4. Check GitHub Issues

## Examples Repository

See `examples/` directory for:
- Sample `.env` files for different scenarios
- CI/CD pipeline examples
- Custom deployment scripts
