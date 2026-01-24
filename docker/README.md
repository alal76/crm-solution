# Docker Configuration

This directory contains all Docker-related files for building and running the CRM Solution.

## Files

| File | Description |
|------|-------------|
| `Dockerfile.backend` | Multi-stage build for .NET 8 API |
| `Dockerfile.frontend` | Multi-stage build for React frontend |
| `docker-compose.yml` | Local development orchestration |
| `.dockerignore` | Files to exclude from Docker context |

## Quick Start

### Local Development with Docker Compose

```bash
# From project root
cd docker
docker-compose up -d
```

### Build Individual Images

```bash
# Build backend (from project root)
docker build -t crm-backend:latest -f docker/Dockerfile.backend .

# Build frontend (from project root)
docker build -t crm-frontend:latest -f docker/Dockerfile.frontend .
```

### Production Build

```bash
# Backend with specific tag
docker build -t crm-backend:v1.4.0 -f docker/Dockerfile.backend .

# Frontend with specific tag
docker build -t crm-frontend:v1.4.0 -f docker/Dockerfile.frontend .
```

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    docker-compose                           │
├─────────────────┬─────────────────┬───────────────────────┤
│   crm-frontend  │    crm-api      │      crm-db           │
│   (React)       │    (.NET 8)     │    (MariaDB)          │
│   Port: 3000    │    Port: 5000   │    Port: 3306         │
└─────────────────┴─────────────────┴───────────────────────┘
```

## Environment Variables

### Backend (Dockerfile.backend)

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Production` |
| `ConnectionStrings__DefaultConnection` | Database connection | - |

### Frontend (Dockerfile.frontend)

| Variable | Description | Default |
|----------|-------------|---------|
| `REACT_APP_API_URL` | Backend API URL | `http://localhost:5000` |

## Health Checks

Both containers include health checks:

- **Backend**: `GET /health`
- **Frontend**: `GET /` (nginx responds)

## Troubleshooting

### View Container Logs

```bash
docker logs crm-api
docker logs crm-frontend
docker logs crm-db
```

### Access Container Shell

```bash
docker exec -it crm-api /bin/bash
docker exec -it crm-frontend /bin/sh
docker exec -it crm-db mysql -u root -p
```
