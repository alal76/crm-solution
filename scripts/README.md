# Scripts Directory

This directory contains all automation scripts for the CRM Solution.

## Structure

```
scripts/
├── build-and-deploy.sh    # Main build & deploy pipeline
├── deploy.sh              # Unified deployment script
├── setup-monitoring.sh    # Monitoring services setup
├── build-frontend.sh      # Frontend-only build
├── deploy-192.168.0.9.sh  # Deploy to 192.168.0.9 server
├── deploy-production.sh   # Production deployment
├── seed-test-data.sh      # Seed test data
├── create-test-customers.sh # Create test customers
├── port-forward.sh        # Kubernetes port forwarding
├── start-local-access.sh  # Start local access
└── stop-local-access.sh   # Stop local access
```

## Quick Reference

### Main Deployment Scripts

| Script | Purpose |
|--------|---------|
| `build-and-deploy.sh` | Complete build & deploy pipeline with versioning |
| `deploy.sh` | Unified deployment with architecture selection |
| `setup-monitoring.sh` | Deploy and configure monitoring services |

## Usage

### Build and Deploy

```bash
# Deploy with patch version bump (default)
./scripts/build-and-deploy.sh

# Deploy with minor version bump
./scripts/build-and-deploy.sh minor

# Check deployment status
./scripts/build-and-deploy.sh status
```

### Unified Deployment Script

```bash
# Deploy everything (monolith mode)
./scripts/deploy.sh --env=dev --arch=monolith

# Deploy microservices
./scripts/deploy.sh --env=dev --arch=microservices

# Deploy specific target
./scripts/deploy.sh api              # API only
./scripts/deploy.sh frontend         # Frontend only
./scripts/deploy.sh monitoring       # Monitoring services only
```

### Monitoring Setup

```bash
# Deploy monitoring containers
./scripts/setup-monitoring.sh --deploy

# Full setup (deploy + configure)
./scripts/setup-monitoring.sh --full

# Show monitoring status
./scripts/setup-monitoring.sh --status

# Reset monitoring (removes data)
./scripts/setup-monitoring.sh --reset
```

### Monitoring Services

| Service | Port | URL | Description |
|---------|------|-----|-------------|
| **Uptime Kuma** | 3001 | http://192.168.0.9:3001 | Service health monitoring |
| **Portainer** | 9000 | http://192.168.0.9:9000 | Container management |

Default credentials: `admin` / `CrmAdmin2024!`

### Database Scripts

```bash
# Seed test data
./scripts/seed-test-data.sh

# Create test customers
./scripts/create-test-customers.sh
```
