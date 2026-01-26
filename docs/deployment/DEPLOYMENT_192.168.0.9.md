# Deployment Guide for 192.168.0.9

## Overview

This guide provides complete instructions for deploying the CRM Solution to server 192.168.0.9 with the following architecture:

| Component | Platform | Port(s) |
|-----------|----------|---------|
| Frontend | Kubernetes | 80 |
| API | Kubernetes | 5000 |
| MariaDB | Docker | 3306 |
| PostgreSQL | Docker | 5432 |
| SQL Server | Docker | 1433 |
| Adminer (DB UI) | Docker | 8080 |

---

## Prerequisites

### On Your Local Machine
- Docker Desktop with Kubernetes enabled
- Git for version control
- SSH key configured for 192.168.0.9

### On Remote Server (192.168.0.9)
- Docker installed and running
- Docker Compose installed
- kubectl configured (installed automatically by deployment script)
- Kubernetes cluster running (k3s, microk8s, or similar)

---

## Quick Deployment

### One-Command Deployment

```bash
# Run the complete deployment script
cd scripts
chmod +x deploy-192.168.0.9.sh
./deploy-192.168.0.9.sh
```

This will:
1. ✅ Verify SSH connectivity
2. ✅ Deploy all database containers (MariaDB, PostgreSQL, SQL Server)
3. ✅ Initialize and seed databases
4. ✅ Deploy Frontend and API to Kubernetes
5. ✅ Verify deployment health

---

## Step-by-Step Deployment

### Step 1: Deploy Databases Only

```bash
./deploy-192.168.0.9.sh databases
```

This deploys:
- **MariaDB** on port 3306
- **PostgreSQL** on port 5432
- **SQL Server** on port 1433
- **Adminer** (database UI) on port 8080

### Step 2: Build and Push Images

```bash
# Build Docker images locally
./deploy-192.168.0.9.sh build

# Or manually:
docker build -f Dockerfile.backend -t ghcr.io/alal76/crm-api:latest .
docker build -f Dockerfile.frontend -t ghcr.io/alal76/crm-frontend:latest .
docker push ghcr.io/alal76/crm-api:latest
docker push ghcr.io/alal76/crm-frontend:latest
```

### Step 3: Deploy to Kubernetes

```bash
./deploy-192.168.0.9.sh kubernetes
```

This applies Kubernetes manifests for:
- CRM API deployment (2 replicas)
- CRM Frontend deployment (2 replicas)
- Services and ingress configuration

### Step 4: Verify Deployment

```bash
./deploy-192.168.0.9.sh test
```

---

## Database Configuration

### Supported Databases

The CRM solution supports multiple database backends. Configure the API to use any of these:

#### MariaDB (Default)
```yaml
DatabaseProvider: "mariadb"
ConnectionStrings__DefaultConnection: "Server=192.168.0.9;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024!;"
```

#### PostgreSQL
```yaml
DatabaseProvider: "postgresql"
ConnectionStrings__DefaultConnection: "Host=192.168.0.9;Port=5432;Database=crm_db;Username=crm_user;Password=CrmPass@Dev2024!"
```

#### SQL Server
```yaml
DatabaseProvider: "sqlserver"
ConnectionStrings__DefaultConnection: "Server=192.168.0.9,1433;Database=crm_db;User Id=sa;Password=CrmPass@Dev2024!;TrustServerCertificate=True"
```

#### SQLite (Development)
```yaml
DatabaseProvider: "sqlite"
ConnectionStrings__DefaultConnection: "Data Source=/data/crm.db"
```

### Database Credentials

| Database | User | Password | Port |
|----------|------|----------|------|
| MariaDB | crm_user | CrmPass@Dev2024! | 3306 |
| MariaDB (root) | root | RootPass@Dev2024! | 3306 |
| PostgreSQL | crm_user | CrmPass@Dev2024! | 5432 |
| SQL Server | sa | CrmPass@Dev2024! | 1433 |

### Accessing Databases

#### Via Adminer (Web UI)
Open: http://192.168.0.9:8080

#### Via CLI

```bash
# MariaDB
ssh root@192.168.0.9 "docker exec -it crm-mariadb mysql -u crm_user -pCrmPass@Dev2024! crm_db"

# PostgreSQL
ssh root@192.168.0.9 "docker exec -it crm-postgresql psql -U crm_user -d crm_db"

# SQL Server
ssh root@192.168.0.9 "docker exec -it crm-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P CrmPass@Dev2024!"
```

---

## Kubernetes Management

### View Resources

```bash
# SSH to server
ssh root@192.168.0.9

# View all CRM resources
kubectl get all -n crm-app

# View pods
kubectl get pods -n crm-app -o wide

# View services
kubectl get services -n crm-app

# View logs
kubectl logs -n crm-app -l app=crm,tier=application --tail=100
kubectl logs -n crm-app -l app=crm,tier=presentation --tail=100
```

### Scale Deployments

```bash
# Scale API to 3 replicas
kubectl scale deployment crm-api -n crm-app --replicas=3

# Scale Frontend to 3 replicas
kubectl scale deployment crm-frontend -n crm-app --replicas=3
```

### Restart Deployments

```bash
# Restart API
kubectl rollout restart deployment/crm-api -n crm-app

# Restart Frontend
kubectl rollout restart deployment/crm-frontend -n crm-app
```

---

## Testing the Deployment

### API Health Check

```bash
curl http://192.168.0.9:5000/health
curl http://192.168.0.9:5000/health/live
curl http://192.168.0.9:5000/health/ready
```

### Frontend Access

Open in browser: http://192.168.0.9

### Database Connectivity Test

```bash
# Test MariaDB
ssh root@192.168.0.9 "docker exec crm-mariadb mariadb-admin ping -h localhost -u root -pRootPass@Dev2024!"

# Test PostgreSQL
ssh root@192.168.0.9 "docker exec crm-postgresql pg_isready -U crm_user -d crm_db"

# Test SQL Server
ssh root@192.168.0.9 "docker exec crm-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P CrmPass@Dev2024! -Q 'SELECT 1'"
```

---

## Switching Databases

To switch the API to use a different database:

### 1. Update Kubernetes ConfigMap

Edit the config or create a new one:

```yaml
# For PostgreSQL
kubectl patch configmap crm-config -n crm-app --type merge -p '{"data":{"DatabaseProvider":"postgresql"}}'

# Update secret with new connection string
kubectl create secret generic crm-secrets -n crm-app \
  --from-literal=DB_CONNECTION_STRING="Host=192.168.0.9;Port=5432;Database=crm_db;Username=crm_user;Password=CrmPass@Dev2024!" \
  --dry-run=client -o yaml | kubectl apply -f -
```

### 2. Restart API Deployment

```bash
kubectl rollout restart deployment/crm-api -n crm-app
```

### 3. Verify New Connection

```bash
kubectl logs -n crm-app -l app=crm,tier=application --tail=50 | grep -i database
```

---

## Troubleshooting

### Database Container Not Starting

```bash
# Check container logs
ssh root@192.168.0.9 "docker logs crm-mariadb"
ssh root@192.168.0.9 "docker logs crm-postgresql"
ssh root@192.168.0.9 "docker logs crm-mssql"

# Check disk space
ssh root@192.168.0.9 "df -h"

# Check memory
ssh root@192.168.0.9 "free -h"
```

### API Cannot Connect to Database

```bash
# Verify database is running
ssh root@192.168.0.9 "docker ps | grep crm-"

# Test network connectivity from Kubernetes pod
kubectl exec -n crm-app $(kubectl get pods -n crm-app -l app=crm,tier=application -o jsonpath='{.items[0].metadata.name}') -- curl -v 192.168.0.9:3306
```

### Kubernetes Pod Not Starting

```bash
# Check pod events
kubectl describe pod -n crm-app -l app=crm

# Check pod logs
kubectl logs -n crm-app -l app=crm --previous

# Check resources
kubectl top pods -n crm-app
```

---

## Files Reference

| File | Description |
|------|-------------|
| `scripts/deploy-192.168.0.9.sh` | Main deployment script |
| `docker/docker-compose.databases.yml` | Multi-database Docker Compose |
| `kubernetes/00-namespace-config-192.168.0.9.yaml` | Kubernetes config for 192.168.0.9 |
| `kubernetes/02-application-tier.yaml` | API deployment |
| `kubernetes/03-presentation-tier.yaml` | Frontend deployment |

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        192.168.0.9                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    KUBERNETES                            │   │
│  │  ┌─────────────┐         ┌─────────────────────────┐    │   │
│  │  │  Frontend   │         │         API             │    │   │
│  │  │   (React)   │  ─────▶ │       (.NET 8)          │    │   │
│  │  │   :80       │         │        :5000            │    │   │
│  │  └─────────────┘         └───────────┬─────────────┘    │   │
│  └──────────────────────────────────────┼──────────────────┘   │
│                                         │                       │
│  ┌──────────────────────────────────────▼──────────────────┐   │
│  │                      DOCKER                              │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐   │   │
│  │  │   MariaDB    │  │  PostgreSQL  │  │  SQL Server  │   │   │
│  │  │    :3306     │  │    :5432     │  │    :1433     │   │   │
│  │  └──────────────┘  └──────────────┘  └──────────────┘   │   │
│  │                                                          │   │
│  │  ┌──────────────┐                                        │   │
│  │  │   Adminer    │  ← Database Management UI              │   │
│  │  │    :8080     │                                        │   │
│  │  └──────────────┘                                        │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Support

For issues or questions:
1. Check container/pod logs first
2. Verify network connectivity
3. Review this documentation
4. Check main [DEPLOYMENT_README.md](../DEPLOYMENT_README.md)
