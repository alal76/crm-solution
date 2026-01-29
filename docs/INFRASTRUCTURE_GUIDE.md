# CRM Solution - Infrastructure Configuration Guide

## Overview

This document describes the unified infrastructure configuration and naming conventions used across the CRM Solution. All containers, networks, volumes, and services follow a consistent naming pattern to ensure maintainability and prevent DNS/connectivity issues.

## Master Configuration

All infrastructure configuration is defined in a single source of truth:

```
config/infrastructure.env
```

This file contains:
- Project identifiers and namespaces
- Container/pod naming conventions
- Network configurations
- Port allocations
- Database connection settings
- Health check configurations
- Service dependency ordering

## Naming Conventions

### Container Names

All containers follow the pattern: `crm-{service}`

| Service | Container Name | Description |
|---------|---------------|-------------|
| Database | `crm-mariadb` | MariaDB database server |
| Cache | `crm-redis` | Redis cache server |
| API | `crm-api` | Backend API (monolith) |
| Frontend | `crm-frontend` | React frontend (Nginx) |

### Network Names

| Network | Purpose |
|---------|---------|
| `crm-network` | Main application network |
| Subnet: `172.28.0.0/16` | Predictable IP addressing |

### Network Aliases

Each container has multiple DNS aliases for flexibility:

| Container | Aliases |
|-----------|---------|
| crm-mariadb | `mariadb`, `db` |
| crm-redis | `redis`, `cache` |
| crm-api | `api`, `backend` |
| crm-frontend | `frontend`, `web` |

### Volume Names

| Volume | Container Mount | Purpose |
|--------|-----------------|---------|
| `crm_db_data` | `/var/lib/mysql` | Database persistence |
| `crm_api_data` | `/app/data` | API file storage |
| `crm_redis_data` | `/data` | Redis persistence |

### Port Allocations

| Service | Internal Port | External Port | Protocol |
|---------|--------------|---------------|----------|
| MariaDB | 3306 | 3306 | TCP |
| Redis | 6379 | 6379 | TCP |
| API | 5000 | 5000 | HTTP |
| Frontend | 80 | 80 | HTTP |

## Architecture Modes

### Monolith Architecture

Single API container serving all functionality.

**Docker Compose file:** `docker/docker-compose.unified.yml`

```
┌─────────────────────────────────────────────────────────────┐
│                       crm-network                           │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ crm-mariadb  │  │  crm-redis   │  │     crm-api      │  │
│  │   (db)       │  │   (cache)    │  │    (backend)     │  │
│  │  :3306       │  │    :6379     │  │     :5000        │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
│         ▲                 ▲                  ▲              │
│         │                 │                  │              │
│         └─────────────────┼──────────────────┘              │
│                           │                                 │
│                   ┌───────┴────────┐                        │
│                   │  crm-frontend  │                        │
│                   │    (web)       │                        │
│                   │     :80        │                        │
│                   └────────────────┘                        │
└─────────────────────────────────────────────────────────────┘
```

### Microservices Architecture

Multiple service containers behind an API gateway.

**Docker Compose file:** `docker/docker-compose.microservices.unified.yml`

| Service | Port | Description |
|---------|------|-------------|
| crm-gateway | 5000 | API Gateway |
| crm-identity | 5001 | Authentication |
| crm-customer | 5002 | Customer management |
| crm-sales | 5003 | Sales module |
| crm-marketing | 5004 | Marketing module |
| crm-servicedesk | 5005 | Service desk |
| crm-core | 5006 | Core services |

## Service Dependencies

Services start in dependency order using Docker Compose health checks:

```
Tier 0: crm-mariadb, crm-redis (no dependencies)
    │
    ▼
Tier 1: crm-api (depends on: mariadb, redis)
    │
    ▼
Tier 2: crm-frontend (depends on: api)
```

## Health Checks

All services have configured health checks:

| Service | Endpoint | Interval | Timeout | Retries |
|---------|----------|----------|---------|---------|
| MariaDB | `mariadb-admin ping` | 10s | 5s | 10 |
| Redis | `redis-cli ping` | 10s | 5s | 5 |
| API | `GET /health` | 30s | 10s | 3 |
| Frontend | `GET /health.html` | 30s | 10s | 3 |

## Deployment Script

Use the unified deployment script:

```bash
# Deploy monolith to dev server
./scripts/deploy.sh --env=dev --arch=monolith

# Deploy microservices to staging
./scripts/deploy.sh --env=staging --arch=microservices

# Build only (no deploy)
./scripts/deploy.sh --build-only --env=dev

# Deploy specific service
./scripts/deploy.sh --env=dev api

# Show help
./scripts/deploy.sh --help
```

### Options

| Option | Values | Default | Description |
|--------|--------|---------|-------------|
| `--env` | local, dev, staging, prod | dev | Target environment |
| `--arch` | monolith, microservices | monolith | Architecture mode |
| `--version` | patch, minor, major | patch | Version increment |
| `--build-only` | - | false | Build without deploying |
| `--deploy-only` | - | false | Deploy without rebuilding |
| `--skip-sync` | - | false | Skip source code sync |
| `--debug` | - | false | Enable debug output |

### Targets

| Target | Description |
|--------|-------------|
| `all` | Build and deploy everything (default) |
| `api` | Build and deploy API only |
| `frontend` | Build and deploy frontend only |
| `db` | Deploy database only |

## Environment Configuration

### Development Server (192.168.0.9)

```bash
BUILD_SERVER=192.168.0.9
BUILD_USER=root
DATABASE_PROVIDER=mariadb
```

### Local Development

```bash
BUILD_SERVER=localhost
DATABASE_PROVIDER=mariadb
```

### Environment Variables

Set these environment variables to override defaults:

```bash
# Database
DB_PASSWORD=YourSecurePassword
DB_ROOT_PASSWORD=YourRootPassword

# JWT
JWT_SECRET=YourJwtSecret

# Ports (if non-standard)
PORT_API_EXTERNAL=5000
PORT_FRONTEND_EXTERNAL=80
```

## Troubleshooting

### DNS Resolution Issues

If containers can't reach each other by hostname:

1. Verify network is created with proper name:
   ```bash
   docker network ls | grep crm
   ```

2. Check network aliases:
   ```bash
   docker exec crm-api getent hosts crm-mariadb mariadb db
   ```

3. Recreate network if needed:
   ```bash
   docker compose -f docker/docker-compose.unified.yml down
   docker network rm crm-network
   docker compose -f docker/docker-compose.unified.yml up -d
   ```

### Health Check Failures

If services fail health checks:

1. Check container logs:
   ```bash
   docker logs crm-api
   ```

2. Verify service is responding:
   ```bash
   curl http://localhost:5000/health
   ```

3. Check dependencies are healthy:
   ```bash
   docker ps --filter "name=crm-"
   ```

### Connection Timeouts

If API can't connect to database:

1. Verify database is healthy:
   ```bash
   docker exec crm-mariadb mariadb-admin ping -h localhost -u root -pRootPass@Dev2024
   ```

2. Check connection string in API:
   ```bash
   docker exec crm-api printenv | grep Connection
   ```

## File Locations

| File | Purpose |
|------|---------|
| `config/infrastructure.env` | Master configuration |
| `docker/docker-compose.unified.yml` | Monolith deployment |
| `docker/docker-compose.microservices.unified.yml` | Microservices deployment |
| `scripts/deploy.sh` | Unified deployment script |
| `CRM.Frontend/public/health.html` | Frontend health check |
| `version.json` | Version tracking |

## Version Management

The deployment script automatically manages versions:

- Version stored in `version.json`
- Copied to frontend for display
- Incremented on each deploy (patch by default)
- Can specify `--version=minor` or `--version=major`
