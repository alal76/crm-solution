# Docker & Infrastructure Guide

> **CRM Solution Docker, Kubernetes & Cloud Deployment Guide**  
> Version: 0.614.84  
> Last Updated: March 3, 2026

---

## Table of Contents

1. [Overview](#overview)
2. [Docker Architecture](#docker-architecture)
3. [Docker Compose](#docker-compose)
4. [Kubernetes Deployment](#kubernetes-deployment)
5. [Build & Deploy Scripts](#build--deploy-scripts)
6. [Cloud Deployments](#cloud-deployments)
7. [Monitoring & Logging](#monitoring--logging)
8. [Best Practices](#best-practices)

---

## Overview

### Infrastructure Summary

| Component | Technology | Purpose |
|-----------|-----------|---------|
| **Orchestration** | Docker Compose, Kubernetes | Container management |
| **Registry** | GitHub Container Registry (ghcr.io) | Image storage |
| **Databases** | MariaDB, PostgreSQL, SQL Server, Redis | Data persistence |
| **Monitoring** | Prometheus, Grafana, Uptime Kuma | Observability |
| **Reverse Proxy** | Nginx | Frontend serving & API proxy |
| **Build Tool** | Docker Buildx | Multi-platform builds |

### Key Files

```
crm-solution/
├── docker/                           # Docker Compose files
│   ├── docker-compose.app.yml        # Main application services
│   ├── docker-compose.databases.yml  # Database services
│   ├── docker-compose.providers.yml  # External providers
│   ├── docker-compose.production.yml # Production overrides
│   └── docker-compose.monitoring.yml # Monitoring stack
│
├── kubernetes/                       # K8s manifests
│   ├── namespace.yaml
│   ├── deployments/
│   ├── services/
│   ├── configmaps/
│   └── secrets/
│
├── build.sh                          # Main build script
├── deploy-to-dev-server.sh           # Dev deployment
├── start-dev.sh                      # Local development
└── DEPLOY.sh                         # Production deployment
```

---

## Docker Architecture

### Network Topology

The solution uses **3 logical Docker networks**:

```
┌─────────────────────────────────────────────────────────────────┐
│                       CRM INFRASTRUCTURE                         │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│  │  crm-core   │  │   crm-db    │  │   crm-components        │ │
│  │ (App Layer) │  │ (Databases) │  │ (External Providers)    │ │
│  ├─────────────┤  ├─────────────┤  ├─────────────────────────┤ │
│  │ crm-api     │  │ crm-mariadb │  │ crm-meilisearch (7700)  │ │
│  │ (5000)      │  │ (3306)      │  │ crm-ollama (11434)      │ │
│  │             │  │ crm-redis   │  │ crm-chatwoot (3000)     │ │
│  │ crm-frontend│  │ (6379)      │  │ crm-novu (3000)         │ │
│  │ (80)        │  │ crm-postgres│  │ crm-superset (8088)     │ │
│  └─────────────┘  │ (5432)      │  │ crm-docuseal (3000)     │ │
│                   │ crm-sqlserver  │ crm-n8n (5678)          │ │
│                   │ (1433)      │  └─────────────────────────┘ │
│                   └─────────────┘                              │
└─────────────────────────────────────────────────────────────────┘
```

### Container Inventory

| Container | Image | Port | Network | Purpose |
|-----------|-------|------|---------|---------|
| **crm-api** | crm-api:latest | 5000 | crm-core | .NET API |
| **crm-frontend** | crm-frontend:latest | 80 | crm-core | React SPA |
| **crm-mariadb** | mariadb:10.11 | 3306 | crm-db | Primary database |
| **crm-redis** | redis:7-alpine | 6379 | crm-db | Cache & sessions |
| **crm-postgres** | postgres:16 | 5432 | crm-db | Alternative DB |
| **crm-sqlserver** | mcr.microsoft.com/mssql/server:2022 | 1433 | crm-db | SQL Server |
| **crm-meilisearch** | getmeili/meilisearch:v1.6 | 7700 | crm-components | Search |
| **crm-ollama** | ollama/ollama:latest | 11434 | crm-components | Local LLM |
| **crm-chatwoot** | chatwoot/chatwoot:latest | 3000 | crm-components | Chat |
| **crm-novu** | ghcr.io/novuhq/novu/api | 3000 | crm-components | Notifications |
| **crm-superset** | apache/superset:latest | 8088 | crm-components | Analytics |
| **crm-docuseal** | docuseal/docuseal:latest | 3000 | crm-components | E-signatures |
| **crm-n8n** | n8nio/n8n:latest | 5678 | crm-components | Automation |

---

## Docker Compose

### Main Application Stack

**File:** `docker/docker-compose.app.yml`

```yaml
version: '3.8'

services:
  crm-api:
    build:
      context: ../CRM.Backend
      dockerfile: Dockerfile
    image: crm-api:latest
    container_name: crm-api
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=crm-mariadb;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024;
      - Redis__ConnectionString=crm-redis:6379
      - Jwt__Secret=${JWT_SECRET}
    depends_on:
      - crm-mariadb
      - crm-redis
    networks:
      - crm-core
      - crm-db
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
  
  crm-frontend:
    build:
      context: ../CRM.Frontend
      dockerfile: Dockerfile
    image: crm-frontend:latest
    container_name: crm-frontend
    ports:
      - "80:80"
    environment:
      - REACT_APP_API_BASE_URL=http://crm-api:5000
    depends_on:
      - crm-api
    networks:
      - crm-core
    restart: unless-stopped
    volumes:
      - ../nginx-frontend.conf:/etc/nginx/conf.d/default.conf:ro

networks:
  crm-core:
    driver: bridge
  crm-db:
    driver: bridge
```

### Database Stack

**File:** `docker/docker-compose.databases.yml`

```yaml
version: '3.8'

services:
  crm-mariadb:
    image: mariadb:10.11
    container_name: crm-mariadb
    ports:
      - "3306:3306"
    environment:
      - MYSQL_ROOT_PASSWORD=${DB_ROOT_PASSWORD:-RootPass@Dev2024}
      - MYSQL_DATABASE=crm_db
      - MYSQL_USER=crm_user
      - MYSQL_PASSWORD=${DB_PASSWORD:-CrmPass@Dev2024}
    volumes:
      - mariadb_data:/var/lib/mysql
      - ../database/schema:/docker-entrypoint-initdb.d:ro
    networks:
      - crm-db
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "healthcheck.sh", "--connect", "--innodb_initialized"]
      interval: 10s
      timeout: 5s
      retries: 5
  
  crm-redis:
    image: redis:7-alpine
    container_name: crm-redis
    ports:
      - "6379:6379"
    command: redis-server --requirepass ${REDIS_PASSWORD:-RedisPass@Dev2024}
    volumes:
      - redis_data:/data
    networks:
      - crm-db
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 5

  crm-postgres:
    image: postgres:16-alpine
    container_name: crm-postgres
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_DB=crm_db
      - POSTGRES_USER=crm_user
      - POSTGRES_PASSWORD=${DB_PASSWORD:-CrmPass@Dev2024}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - crm-db
    restart: unless-stopped

volumes:
  mariadb_data:
  redis_data:
  postgres_data:

networks:
  crm-db:
    external: true
```

### Provider Stack

**File:** `docker/docker-compose.providers.yml`

```yaml
version: '3.8'

services:
  crm-meilisearch:
    image: getmeili/meilisearch:v1.6
    container_name: crm-meilisearch
    ports:
      - "7700:7700"
    environment:
      - MEILI_MASTER_KEY=masterKey
      - MEILI_ENV=production
    volumes:
      - meilisearch_data:/meili_data
    networks:
      - crm-components
    restart: unless-stopped
  
  crm-ollama:
    image: ollama/ollama:latest
    container_name: crm-ollama
    ports:
      - "11434:11434"
    volumes:
      - ollama_data:/root/.ollama
    networks:
      - crm-components
    restart: unless-stopped
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: all
              capabilities: [gpu]
  
  # Additional providers...

volumes:
  meilisearch_data:
  ollama_data:

networks:
  crm-components:
    driver: bridge
```

### Starting Services

```bash
# Start all services
docker-compose -f docker/docker-compose.app.yml \
               -f docker/docker-compose.databases.yml \
               -f docker/docker-compose.providers.yml \
               up -d

# Start specific stack
docker-compose -f docker/docker-compose.databases.yml up -d

# View logs
docker-compose -f docker/docker-compose.app.yml logs -f crm-api

# Stop all
docker-compose -f docker/docker-compose.app.yml down
```

---

## Kubernetes Deployment

### Namespace

**File:** `kubernetes/namespace.yaml`

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: crm-prod
  labels:
    name: crm-prod
    environment: production
```

### Deployment

**File:** `kubernetes/deployments/api-deployment.yaml`

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crm-api
  namespace: crm-prod
  labels:
    app: crm-api
    tier: backend
spec:
  replicas: 3
  selector:
    matchLabels:
      app: crm-api
  template:
    metadata:
      labels:
        app: crm-api
    spec:
      containers:
      - name: crm-api
        image: ghcr.io/your-org/crm-api:latest
        ports:
        - containerPort: 5000
          name: http
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: crm-secrets
              key: db-connection
        - name: Jwt__Secret
          valueFrom:
            secretKeyRef:
              name: crm-secrets
              key: jwt-secret
        livenessProbe:
          httpGet:
            path: /health/live
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5000
          initialDelaySeconds: 15
          periodSeconds: 5
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"
```

### Service

**File:** `kubernetes/services/api-service.yaml`

```yaml
apiVersion: v1
kind: Service
metadata:
  name: crm-api-svc
  namespace: crm-prod
spec:
  type: LoadBalancer
  selector:
    app: crm-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 5000
    name: http
```

### ConfigMap

**File:** `kubernetes/configmaps/api-config.yaml`

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: crm-api-config
  namespace: crm-prod
data:
  appsettings.Production.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information"
        }
      },
      "AllowedHosts": "*"
    }
```

### Secrets

**File:** `kubernetes/secrets/crm-secrets.yaml`

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: crm-secrets
  namespace: crm-prod
type: Opaque
stringData:
  db-connection: "Server=crm-mariadb-svc;Port=3306;Database=crm_db;User=crm_user;Password=***"
  jwt-secret: "YourSuperSecretKeyThatIsAtLeast32CharactersLong"
  redis-connection: "crm-redis-svc:6379,password=***"
```

### Deploying to Kubernetes

```bash
# Apply namespace
kubectl apply -f kubernetes/namespace.yaml

# Apply secrets (after encrypting)
kubectl apply -f kubernetes/secrets/

# Apply configmaps
kubectl apply -f kubernetes/configmaps/

# Apply deployments
kubectl apply -f kubernetes/deployments/

# Apply services
kubectl apply -f kubernetes/services/

# Check status
kubectl get pods -n crm-prod
kubectl get services -n crm-prod

# View logs
kubectl logs -f deployment/crm-api -n crm-prod
```

---

## Build & Deploy Scripts

### Cross-Platform Build (`build.sh`)

```bash
#!/bin/bash
# Builds Docker images for linux/amd64 (Mac → Linux server)

set -e

echo "Building CRM Docker images for linux/amd64..."

# Backend API
docker buildx build \
  --platform linux/amd64 \
  -t crm-api:latest \
  -f CRM.Backend/Dockerfile \
  --load \
  CRM.Backend/

# Frontend
docker buildx build \
  --platform linux/amd64 \
  -t crm-frontend:latest \
  -f CRM.Frontend/Dockerfile \
  --load \
  CRM.Frontend/

echo "✅ Build complete!"
```

### Dev Server Deployment (`deploy-to-dev-server.sh`)

**717 lines** - Complete deployment pipeline to 192.168.0.9:

```bash
#!/bin/bash
# Comprehensive deployment to development server

TARGET_SERVER="192.168.0.9"
SSH_USER="root"
BUILD_PLATFORM="linux/amd64"

# Step 1: Version management (from version.json)
# Step 2: Build Docker images
# Step 3: Save images as tar archives
# Step 4: Transfer to server via rsync
# Step 5: Load images on server
# Step 6: Deploy with docker-compose
# Step 7: Health verification
# Step 8: Setup monitoring

# Usage:
./deploy-to-dev-server.sh
```

### Local Development (`start-dev.sh`)

```bash
#!/bin/bash
# Start API + Frontend locally

echo "Starting CRM Solution..."

# Check dependencies
command -v dotnet >/dev/null || { echo "dotnet not found"; exit 1; }
command -v node >/dev/null || { echo "node not found"; exit 1; }

# Start API
cd CRM.Backend/src/CRM.Api
dotnet run &
API_PID=$!

# Start Frontend
cd ../../CRM.Frontend
npm start &
FRONTEND_PID=$!

echo "API: http://localhost:5000"
echo "Frontend: http://localhost:3000"

# Wait for Ctrl+C
trap "kill $API_PID $FRONTEND_PID; exit" INT
wait
```

---

## Cloud Deployments

### Azure AKS

**Resource Group:** `rg-crm-prod`

```bash
# Create AKS cluster
az aks create \
  --resource-group rg-crm-prod \
  --name aks-crm-prod \
  --node-count 3 \
  --node-vm-size Standard_D4s_v3 \
  --enable-managed-identity \
  --generate-ssh-keys

# Get credentials
az aks get-credentials --resource-group rg-crm-prod --name aks-crm-prod

# Deploy
kubectl apply -f kubernetes/
```

### AWS ECS

```bash
# Create ECS cluster
aws ecs create-cluster --cluster-name crm-prod-cluster

# Register task definition (from terraform/aws/ecs-task-definition.json)
aws ecs register-task-definition --cli-input-json file://ecs-task-definition.json

# Create service
aws ecs create-service \
  --cluster crm-prod-cluster \
  --service-name crm-api \
  --task-definition crm-api:1 \
  --desired-count 3
```

### GCP GKE

```bash
# Create GKE cluster
gcloud container clusters create gke-crm-prod \
  --zone us-central1-a \
  --num-nodes 3 \
  --machine-type n1-standard-4

# Get credentials
gcloud container clusters get-credentials gke-crm-prod --zone us-central1-a

# Deploy
kubectl apply -f kubernetes/
```

---

## Monitoring & Logging

### Prometheus + Grafana

**File:** `docker/docker-compose.monitoring.yml`

```yaml
version: '3.8'

services:
  prometheus:
    image: prom/prometheus:latest
    container_name: crm-prometheus
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
    networks:
      - crm-monitoring
  
  grafana:
    image: grafana/grafana:latest
    container_name: crm-grafana
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana_data:/var/lib/grafana
    networks:
      - crm-monitoring

volumes:
  prometheus_data:
  grafana_data:

networks:
  crm-monitoring:
    driver: bridge
```

### Uptime Kuma

```bash
# Setup monitoring
./scripts/setup-monitoring.sh

# Access: http://localhost:3001
# Configure monitors for all services
```

---

## Best Practices

### ✅ DO

- Use multi-stage Docker builds to minimize image size
- Set resource limits on all containers
- Use health checks for all services
- Store secrets in Kubernetes Secrets or Azure Key Vault
- Use named volumes for data persistence
- Implement proper logging with structured logs
- Tag images with semantic versions

### ❌ DON'T

- Don't store secrets in images or git
- Don't run containers as root
- Don't use `:latest` tag in production
- Don't expose unnecessary ports
- Don't skip health checks
- Don't ignore resource limits

---

## Additional Resources

- **Kubernetes Documentation:** https://kubernetes.io/docs/
- **Docker Documentation:** https://docs.docker.com/
- **CDT Guide:** `docs/deployment/CDT_GUIDE.md`
- **Backend Guide:** `docs/backend/DEVELOPER_GUIDE.md`

---

**Document Version:** 1.0  
**Last Updated:** March 3, 2026  
**Maintained By:** CRM Infrastructure Team
