# CRM Solution - Build System Documentation

## Overview

The CRM solution uses a modular build system that supports:
- **Module-level builds** - Build only what changed
- **Source change detection** - Skip rebuilds when source hasn't changed  
- **Remote builds** - Offload builds to a dedicated server (192.168.0.9)
- **Container registry** - Push/pull images from remote registry
- **Kubernetes deployment** - Auto-deploy after successful builds

## Quick Start

```bash
# From project root
./build.sh                    # Build all and deploy
./build.sh backend            # Build backend only
./build.sh frontend           # Build frontend only
./build.sh remote             # Build on remote server
./build.sh status             # Show build status
```

## Build Scripts Location

```
scripts/build/
├── build-modular.sh          # Main modular build system
├── quick-build.sh            # Simplified wrapper
├── setup-remote-registry.sh  # One-time remote setup
├── build-config.env          # Build configuration
└── build-config.local.env    # Local overrides (git-ignored)
```

## Module-Level Builds

### Available Modules

| Module | Description | Dockerfile |
|--------|-------------|------------|
| `backend` | Monolithic API | docker/Dockerfile.backend |
| `frontend` | React SPA | docker/Dockerfile.frontend |
| `gateway` | API Gateway (microservices) | docker/Dockerfile.gateway |
| `identity` | Auth service | docker/Dockerfile.identity |
| `customer` | Customer service | docker/Dockerfile.customer |
| `sales` | Sales service | docker/Dockerfile.sales |
| `marketing` | Marketing service | docker/Dockerfile.marketing |
| `servicedesk` | Service desk | docker/Dockerfile.servicedesk |
| `core` | Core service | docker/Dockerfile.core |

### Build Commands

```bash
# Build specific module
./scripts/build/build-modular.sh backend
./scripts/build/build-modular.sh frontend

# Build with specific version
./scripts/build/build-modular.sh backend --version v50

# Build without cache
./scripts/build/build-modular.sh backend --no-cache

# Build and deploy
./scripts/build/build-modular.sh backend --deploy

# Dry run (show what would happen)
./scripts/build/build-modular.sh all --dry-run
```

## Remote Build Server

The build system can use **192.168.0.9** as a remote build server when:
- Local memory is below threshold (4GB)
- Local disk space is low
- `--remote` flag is specified

### Setup Remote Server

```bash
# Run once to configure the remote server
./scripts/build/setup-remote-registry.sh
```

This will:
1. Install Docker on remote server (if needed)
2. Start a Docker registry on port 5000
3. Configure local Docker for insecure registry access
4. Create build directories

### Remote Build Commands

```bash
# Force build on remote server
./scripts/build/build-modular.sh backend --remote

# Build locally, push to remote registry
./scripts/build/build-modular.sh backend --push

# Build on remote, then deploy locally
./scripts/build/build-modular.sh all --remote --deploy
```

## Container Registry

The remote registry runs at `192.168.0.9:5000`

```bash
# List images in registry
curl http://192.168.0.9:5000/v2/_catalog

# List tags for an image
curl http://192.168.0.9:5000/v2/crm-backend/tags/list

# Pull from registry
docker pull 192.168.0.9:5000/crm-backend:v50
```

## Configuration

### build-config.env

Main configuration file with defaults:

```bash
# Remote build server
REMOTE_BUILD_HOST=192.168.0.9
REMOTE_BUILD_USER=builder
REMOTE_REGISTRY=192.168.0.9:5000

# Resource thresholds
LOCAL_MEMORY_THRESHOLD_MB=4096
LOCAL_DISK_THRESHOLD_GB=10

# Build options
DOCKER_BUILDKIT=1
BUILD_PARALLEL_JOBS=4

# Kubernetes
K8S_NAMESPACE_MONOLITH=crm-app
K8S_NAMESPACE_MICROSERVICES=crm-microservices
AUTO_DEPLOY=true
```

### build-config.local.env (git-ignored)

Override settings for your local environment:

```bash
# Example local overrides
FORCE_LOCAL_BUILD=true
REMOTE_BUILD_HOST=my-server.local
```

## Build Caching

The build system uses multiple layers of caching:

### Docker Layer Caching
- Dependencies restored in separate stage
- Source changes don't invalidate dependency cache
- BuildKit cache mounts for NuGet/npm packages

### Source Hash Caching
- Calculates hash of source files
- Skips rebuild if hash unchanged
- Cache stored in `.build-cache/`

```bash
# View cached hashes
ls -la .build-cache/

# Clear cache
rm -rf .build-cache/
# or
./build.sh clean
```

## Dockerfile Optimization

### Backend (Dockerfile.backend)

```dockerfile
# Stage 1: Dependencies (cached)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS deps
COPY *.csproj files
RUN --mount=type=cache dotnet restore

# Stage 2: Build (uses cached deps)
FROM deps AS builder
COPY source files
RUN dotnet build && dotnet publish

# Stage 3: Runtime (minimal alpine image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
```

### Frontend (Dockerfile.frontend)

```dockerfile
# Stage 1: npm dependencies (cached)
FROM node:18-alpine AS deps
COPY package*.json
RUN --mount=type=cache npm ci

# Stage 2: Build React app
FROM deps AS builder
COPY source
RUN --mount=type=cache npm run build

# Stage 3: Nginx runtime
FROM nginx:alpine-slim
COPY --from=builder build files
```

## Kubernetes Deployment

After a successful build with `--deploy`:

```bash
# Check deployment status
kubectl get pods -n crm-app
kubectl get deployments -n crm-app

# View logs
kubectl logs deployment/crm-api -n crm-app
kubectl logs deployment/crm-frontend -n crm-app

# Manual rollback
kubectl rollout undo deployment/crm-api -n crm-app
```

## Troubleshooting

### Build Fails with Memory Error

```bash
# Use remote build
./scripts/build/build-modular.sh frontend --remote

# Or increase Docker memory in Docker Desktop
```

### Registry Connection Refused

```bash
# Check if registry is running
ssh builder@192.168.0.9 "docker ps | grep registry"

# Restart registry
ssh builder@192.168.0.9 "docker restart registry"

# Check Docker insecure-registries config
docker info | grep -A5 "Insecure Registries"
```

### Image Not Found in Minikube

```bash
# Load image into minikube
minikube image load crm-backend:v50

# Or use minikube's Docker daemon
eval $(minikube docker-env)
docker build -t crm-backend:v50 -f docker/Dockerfile.backend .
```

### Deployment Not Updating

```bash
# Force rollout
kubectl rollout restart deployment/crm-api -n crm-app

# Check image being used
kubectl get deployment crm-api -n crm-app -o jsonpath='{.spec.template.spec.containers[0].image}'
```

## CI/CD Integration

For GitHub Actions or other CI systems:

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build Backend
        run: |
          ./scripts/build/build-modular.sh backend \
            --version ${{ github.sha }} \
            --push
      
      - name: Build Frontend
        run: |
          ./scripts/build/build-modular.sh frontend \
            --version ${{ github.sha }} \
            --push
```

## Version Management

Versions are tracked in `version.json`:

```json
{
  "major": 1,
  "minor": 4,
  "patch": 4,
  "lastUpdate": "2026-01-25"
}
```

Build numbers are auto-incremented per module in `.build-cache/`:
- `backend-build-number`
- `frontend-build-number`
- `backend-version` (last built version)
- `frontend-version` (last built version)
