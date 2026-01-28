# Deployment Guide for 192.168.0.9

This guide covers deploying the CRM Solution to 192.168.0.9 using:
- **Kubernetes** for Frontend and Backend API
- **Docker** for SQL Server database and Redis cache

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                        192.168.0.9                                   │
├─────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    Kubernetes Cluster                        │   │
│  │  ┌──────────────────┐      ┌──────────────────┐            │   │
│  │  │   crm-frontend   │      │     crm-api      │            │   │
│  │  │   (2 replicas)   │      │   (2 replicas)   │            │   │
│  │  │     Port: 80     │      │    Port: 5000    │            │   │
│  │  └──────────────────┘      └──────────────────┘            │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                    │                                 │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    Docker Containers                          │   │
│  │  ┌──────────────────┐      ┌──────────────────┐            │   │
│  │  │   crm-sqlserver  │      │    crm-redis     │            │   │
│  │  │    Port: 1433    │      │    Port: 6379    │            │   │
│  │  └──────────────────┘      └──────────────────┘            │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

## Prerequisites

On the deployment server (192.168.0.9):

1. **Docker** installed and running
2. **Kubernetes** cluster (k3s, minikube, or full K8s)
3. **kubectl** configured
4. SSH access from build machine

## Step 1: Deploy Database Infrastructure (Docker)

### Copy configuration files to server

```bash
# From local machine
scp docker/docker-compose.sqlserver.yml root@192.168.0.9:/opt/crm/docker/
scp docker/.env.192.168.0.9 root@192.168.0.9:/opt/crm/docker/.env
```

### Start SQL Server and Redis containers

```bash
# On 192.168.0.9
cd /opt/crm/docker
docker compose -f docker-compose.sqlserver.yml up -d
```

### Verify containers are running

```bash
docker ps | grep crm
# Expected output:
# crm-sqlserver   ... Up ...   0.0.0.0:1433->1433/tcp
# crm-redis       ... Up ...   0.0.0.0:6379->6379/tcp
```

## Step 2: Build Container Images

### Build on the build server (192.168.0.9)

```bash
cd /path/to/crm-solution

# Build API image
docker build -f docker/Dockerfile.backend -t 192.168.0.9:5000/crm-api:latest .

# Build Frontend image  
docker build -f docker/Dockerfile.frontend -t 192.168.0.9:5000/crm-frontend:latest .

# Push to local registry (if using local registry)
docker push 192.168.0.9:5000/crm-api:latest
docker push 192.168.0.9:5000/crm-frontend:latest
```

### Alternative: Load images directly into k3s

```bash
# If using k3s without registry
docker save 192.168.0.9:5000/crm-api:latest | sudo k3s ctr images import -
docker save 192.168.0.9:5000/crm-frontend:latest | sudo k3s ctr images import -
```

## Step 3: Deploy to Kubernetes

### Copy Kubernetes manifests

```bash
scp kubernetes/00-namespace-config-192.168.0.9.yaml root@192.168.0.9:/opt/crm/kubernetes/
scp kubernetes/02-application-tier.yaml root@192.168.0.9:/opt/crm/kubernetes/
scp kubernetes/03-presentation-tier.yaml root@192.168.0.9:/opt/crm/kubernetes/
scp kubernetes/04-ingress-network.yaml root@192.168.0.9:/opt/crm/kubernetes/
```

### Apply Kubernetes configuration

```bash
# On 192.168.0.9
kubectl apply -f /opt/crm/kubernetes/00-namespace-config-192.168.0.9.yaml
kubectl apply -f /opt/crm/kubernetes/02-application-tier.yaml
kubectl apply -f /opt/crm/kubernetes/03-presentation-tier.yaml
kubectl apply -f /opt/crm/kubernetes/04-ingress-network.yaml
```

### Verify deployment

```bash
kubectl get pods -n crm-app
# Expected output:
# NAME                           READY   STATUS    RESTARTS   AGE
# crm-api-xxxxx                  1/1     Running   0          1m
# crm-api-yyyyy                  1/1     Running   0          1m
# crm-frontend-xxxxx             1/1     Running   0          1m
# crm-frontend-yyyyy             1/1     Running   0          1m
```

## Step 4: Automated Deployment (Recommended)

Use the provided deployment script:

```bash
# From local machine
./scripts/deploy-192.168.0.9.sh
```

The script handles:
- SSH connectivity check
- Docker container deployment
- Kubernetes manifest application
- Health verification

## Configuration Files

### Environment Variables (.env.192.168.0.9)

Key settings configured:

| Variable | Value | Description |
|----------|-------|-------------|
| `DATABASE_PROVIDER` | sqlserver | Database type |
| `DB_HOST` | 192.168.0.9 | SQL Server host |
| `DB_PORT` | 1433 | SQL Server port |
| `DEPLOYMENT_TYPE` | kubernetes | Deployment mode |
| `BUILD_SERVER` | 192.168.0.9 | Build server IP |
| `REDIS_HOST` | 192.168.0.9 | Redis host |
| `FRONTEND_URL` | http://192.168.0.9 | Frontend URL |

### Kubernetes ConfigMap

The ConfigMap includes:
- Database connection settings (SQL Server)
- Monitoring configuration
- Redis connection
- CORS allowed origins

### Secrets

Stored in Kubernetes Secrets:
- Database credentials
- JWT secret
- Connection strings

## Monitoring

The monitoring module provides:

### Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /api/monitoring/health-summary` | Complete system health |
| `GET /api/monitoring/endpoints` | Discovered endpoints |
| `GET /api/monitoring/endpoints/health` | Endpoint health status |
| `GET /api/monitoring/containers` | Docker container health |
| `GET /api/monitoring/pods` | Kubernetes pod health |
| `GET /api/monitoring/api-metrics` | API performance metrics |

### Configuration

Monitoring is configured via environment variables:

```bash
Monitoring__DeploymentType=kubernetes
Monitoring__BuildServer=192.168.0.9
Monitoring__EnableDockerMonitoring=true
Monitoring__EnableK8sMonitoring=true
Monitoring__KubernetesNamespace=crm-app
```

## Troubleshooting

### Check API logs

```bash
kubectl logs -f deployment/crm-api -n crm-app
```

### Check frontend logs

```bash
kubectl logs -f deployment/crm-frontend -n crm-app
```

### Check database connectivity

```bash
docker exec crm-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'CrmPass@Dev2024!' -Q "SELECT 1" -C
```

### Check Redis connectivity

```bash
docker exec crm-redis redis-cli ping
```

### Restart services

```bash
# Restart API
kubectl rollout restart deployment/crm-api -n crm-app

# Restart Frontend
kubectl rollout restart deployment/crm-frontend -n crm-app
```

## Access URLs

After deployment:

| Service | URL |
|---------|-----|
| Frontend | http://192.168.0.9 |
| API | http://192.168.0.9:5000 |
| API Health | http://192.168.0.9:5000/health |
| Monitoring | http://192.168.0.9:5000/api/monitoring/health-summary |
