# CRM Frontend Build & Deployment Guidelines

## Overview

This document outlines the correct procedures for building and deploying the CRM frontend to ensure it works correctly across all environments (local development, on-premises, cloud deployments).

## ⚠️ Critical: API URL Configuration

The CRM frontend is designed to **auto-detect the API URL at runtime** based on the browser's current location. This allows the same build to work on any deployment without configuration changes.

### How It Works

1. **Production builds**: `REACT_APP_API_URL` is set to **empty** in `.env.production`
2. **Runtime detection**: The frontend uses `window.location.origin` to determine the API base URL
3. **nginx proxy**: The `/api/*` requests are proxied by nginx to the backend service

### The Golden Rule

> **NEVER set a hardcoded URL in `.env.production`**

## Environment Files

| File | Purpose | Tracked in Git? |
|------|---------|-----------------|
| `.env` | Local development settings | ❌ No |
| `.env.local` | Local overrides | ❌ No |
| `.env.production` | Production build settings | ✅ Yes |
| `.env.example` | Template for developers | ✅ Yes |

### Correct `.env.production` Configuration

```env
# Production environment - API URL is empty to enable runtime detection
REACT_APP_API_URL=
NODE_ENV=production
```

### Incorrect (DO NOT DO THIS)

```env
# ❌ WRONG - This will break cloud deployments
REACT_APP_API_URL=http://localhost:5000/api
REACT_APP_API_URL=http://192.168.0.9:5000/api
REACT_APP_API_URL=http://myserver.com/api
```

## Build Process

### For Local Development

```bash
cd CRM.Frontend
npm start
# Uses .env which has localhost:5000
```

### For Production Build

```bash
cd CRM.Frontend

# Option 1: Standard build with validation
npm run build:prod

# Option 2: Manual build
npm run build
npm run validate:build
```

### For Docker Build

```bash
# Using pre-built assets (recommended)
docker build -f docker/Dockerfile.frontend.prebuilt -t crm-frontend:v1 .

# Using full Docker build (builds inside container)
docker build -f docker/Dockerfile.frontend -t crm-frontend:v1 .
```

**Note**: Never pass `--build-arg REACT_APP_API_URL=...` to Docker builds.

## ⚠️ Architecture Compatibility (amd64 vs arm64)

### Why This Matters

Docker images are architecture-specific. If you build on an Apple Silicon Mac (arm64) and deploy to Azure AKS or standard Linux servers (amd64), the container will fail to start with exec format errors.

### Check Your Build Machine Architecture

```bash
# On macOS
uname -m
# Returns: arm64 (Apple Silicon) or x86_64 (Intel)

# On Linux
dpkg --print-architecture
# Returns: amd64 or arm64
```

### Check Docker Image Architecture

```bash
# Inspect a local image
docker inspect crm-frontend:v1 --format='{{.Architecture}}'

# Inspect a remote image
docker manifest inspect crmdevacrdev.azurecr.io/crm-frontend:v8 | grep architecture
```

### Building for the Correct Architecture

#### Option 1: Build for amd64 on Apple Silicon (Cross-compile)

```bash
# Build specifically for amd64 (Linux servers, Azure, AWS, GCP)
docker build --platform linux/amd64 -f docker/Dockerfile.frontend -t crm-frontend:v1 .

# For multi-architecture builds (recommended for CI/CD)
docker buildx build --platform linux/amd64,linux/arm64 \
  -f docker/Dockerfile.frontend \
  -t crm-frontend:v1 \
  --push .
```

#### Option 2: Use Docker Buildx for Multi-Platform

```bash
# Create a builder that supports multi-platform
docker buildx create --name multiplatform --use

# Build and push for multiple architectures
docker buildx build --platform linux/amd64,linux/arm64 \
  -f docker/Dockerfile.frontend \
  -t crmdevacrdev.azurecr.io/crm-frontend:v1 \
  --push .
```

### Architecture Quick Reference

| Build Machine | Target Platform | Docker Flag |
|---------------|-----------------|-------------|
| Apple Silicon Mac | Azure AKS / Linux | `--platform linux/amd64` |
| Apple Silicon Mac | AWS Graviton / ARM | `--platform linux/arm64` |
| Intel Mac / Linux | Azure AKS / Linux | (none needed) |
| Intel Mac / Linux | AWS Graviton / ARM | `--platform linux/arm64` |

### Common Error: Exec Format Error

If you see this error when starting a container:
```
exec /docker-entrypoint.sh: exec format error
```

**Cause**: Image built for wrong architecture (e.g., arm64 image on amd64 host).

**Fix**: Rebuild with correct `--platform` flag.

### CI/CD Best Practice

Always explicitly set the platform in CI/CD pipelines:

```yaml
# Azure Pipelines
- task: Docker@2
  inputs:
    command: 'build'
    arguments: '--platform linux/amd64'
    
# GitHub Actions
- name: Build and push
  uses: docker/build-push-action@v5
  with:
    platforms: linux/amd64
```

## Validation

### Pre-build Validation

Before building, the scripts automatically check:
- `.env.production` has `REACT_APP_API_URL=` (empty)

### Post-build Validation

After building, run:

```bash
# Using the validation script
./scripts/validate-build.sh

# Or using npm
npm run validate:build
```

This checks:
1. No `localhost:XXXX` URLs in the bundle
2. No private network IPs (192.168.x.x, 10.x.x.x) in the bundle
3. `REACT_APP_API_URL` is empty in the build

### Manual Check

```bash
# Check for hardcoded URLs in the build
grep -rE 'localhost:[0-9]{4}|192\.168\.[0-9]+\.[0-9]+' CRM.Frontend/build/static/js/*.js

# Should return nothing for a valid production build
```

## Deployment Checklist

Before deploying to any environment:

- [ ] `.env.production` has `REACT_APP_API_URL=` (empty)
- [ ] Production build created with `npm run build:prod`
- [ ] `validate-build.sh` passes with no errors
- [ ] Docker image built for correct architecture (`--platform linux/amd64` for cloud)
- [ ] Docker image built without `REACT_APP_API_URL` build arg
- [ ] nginx config has proper `/api/` proxy route

## nginx Configuration

The frontend nginx must proxy `/api/` requests to the backend:

```nginx
location ^~ /api/ {
    proxy_pass http://crm-api:5000;
    proxy_http_version 1.1;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
}
```

## Troubleshooting

### "Network Error" on Login

**Symptoms**: Login fails with "Network Error", browser console shows requests to wrong URL.

**Cause**: Hardcoded API URL was baked into the production bundle.

**Fix**:
1. Check `.env.production` - ensure `REACT_APP_API_URL=` is empty
2. Rebuild: `npm run build:prod`
3. Validate: `./scripts/validate-build.sh`
4. Redeploy

### API Calls Going to localhost

**Symptoms**: In production, API calls go to `http://localhost:5000` instead of the server.

**Cause**: `.env` values leaked into production build.

**Fix**: Same as above - ensure `.env.production` is correct and rebuild.

### CORS Errors

**Symptoms**: Browser console shows CORS policy errors.

**Cause**: API server not configured to allow the frontend origin.

**Fix**: Update backend CORS configuration to allow the frontend domain.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Browser                                   │
│  https://myapp.azurewebsites.net                                │
├─────────────────────────────────────────────────────────────────┤
│                    Frontend (nginx)                              │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Static Files:  /index.html, /static/*                   │   │
│  │  API Proxy:     /api/* → backend:5000                    │   │
│  └─────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────┤
│                    Backend API                                   │
│  http://crm-api:5000                                            │
└─────────────────────────────────────────────────────────────────┘
```

## Related Files

- [CRM.Frontend/.env.production](../CRM.Frontend/.env.production) - Production environment
- [CRM.Frontend/src/config/ports.ts](../CRM.Frontend/src/config/ports.ts) - URL detection logic
- [docker/nginx-frontend.conf](../docker/nginx-frontend.conf) - nginx proxy config
- [docker/Dockerfile.frontend](../docker/Dockerfile.frontend) - Docker build
- [scripts/validate-build.sh](./validate-build.sh) - Build validation
- [scripts/build-frontend.sh](./build-frontend.sh) - Build script with validation
