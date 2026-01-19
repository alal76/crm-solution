# CRM Application - Docker & Kubernetes Three-Tier Architecture

## Overview

Your CRM application has been enhanced with complete Docker containerization and Kubernetes orchestration support, including a three-tier architecture with autoscaling capabilities.

## Architecture Components

### Three-Tier Architecture

```
┌─────────────────────────────────────────────────┐
│         PRESENTATION TIER (Frontend)             │
│  React App - Deployment: 2-8 replicas HPA       │
│  Service: ClusterIP on port 3000                │
│  Autoscaling: CPU 75% / Memory 80%              │
└────────────┬────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────┐
│        APPLICATION TIER (API)                    │
│  .NET 8 ASP.NET Core - Deployment: 2-10         │
│  Service: ClusterIP on port 5000                │
│  Autoscaling: CPU 70% / Memory 80%              │
└────────────┬────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────┐
│          DATA TIER (Database)                    │
│  SQLite - StatefulSet: 1 replica                │
│  PersistentVolume: 10Gi storage                 │
│  Mount: /data/crm.db                            │
└─────────────────────────────────────────────────┘
```

## Files Created

### Docker Files
- **Dockerfile.frontend**: Multi-stage React build with health checks
- **Dockerfile.backend**: .NET SDK + Runtime with health checks
- **docker-compose.yml**: Local development environment with all three tiers
- **.dockerignore**: Optimized Docker build context

### Kubernetes Manifests (kubernetes/ directory)
- **00-namespace-config.yaml**: Namespace, ConfigMaps, and Secrets
- **01-database-tier.yaml**: StatefulSet, PersistentVolume, Service for SQLite
- **02-application-tier.yaml**: API Deployment, Service, HPA, ServiceAccount
- **03-presentation-tier.yaml**: Frontend Deployment, Service, HPA, ServiceAccount
- **04-ingress-network.yaml**: Ingress, NetworkPolicy, PDB, ResourceQuota

### Deployment Scripts
- **deploy.sh**: Bash script for Linux/macOS deployment automation
- **deploy.ps1**: PowerShell script for Windows deployment automation

### Documentation
- **DOCKER_KUBERNETES_GUIDE.md**: Comprehensive deployment guide
- **Backend Health Controller**: New HealthController.cs for liveness/readiness probes

## Key Features

### Containerization
✅ Multi-stage Docker builds for optimized images
✅ Health checks for all containers
✅ Resource limits and requests
✅ Proper signal handling and graceful shutdown
✅ Optimized .dockerignore

### Kubernetes Orchestration
✅ Namespace isolation
✅ ConfigMaps for application configuration
✅ Secrets for sensitive data (JWT, DB connection strings)
✅ PersistentVolumes for data persistence
✅ StatefulSet for database with headless service

### High Availability
✅ Multiple replicas for frontend and API (minimum 2)
✅ Pod anti-affinity to spread across nodes
✅ Pod Disruption Budgets (PDB) to maintain availability during updates
✅ Rolling updates with zero downtime
✅ Liveness and readiness probes

### Auto-Scaling
✅ Horizontal Pod Autoscaler (HPA) on both frontend and API
✅ CPU and memory-based scaling
✅ Separate scale-up and scale-down policies
✅ Aggressive scale-up (double per 30s)
✅ Conservative scale-down (50% per 60s)

### Networking
✅ ClusterIP services for internal communication
✅ Ingress for external access with TLS support
✅ NetworkPolicy for pod-to-pod communication control
✅ Service discovery using Kubernetes DNS

### Monitoring & Logging
✅ Resource limits prevent node saturation
✅ Health check endpoints (/health, /health/ready)
✅ Container logs accessible via kubectl
✅ Pod events for troubleshooting

### Security
✅ ServiceAccounts for RBAC
✅ Kubernetes Secrets for sensitive data
✅ NetworkPolicy restricts communication
✅ ResourceQuota prevents resource exhaustion
✅ Read-only file systems where possible

## Quick Start

### Local Development with Docker Compose

```bash
# Build images
docker-compose build

# Start all services
docker-compose up -d

# Access application
# Frontend: http://localhost:3000
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Deploy to Kubernetes

```bash
# Using PowerShell (Windows)
.\deploy.ps1 deploy

# Using Bash (Linux/macOS)
bash deploy.sh deploy

# Or manually
kubectl apply -f kubernetes/
```

## Autoscaling Configuration

### API Tier (crm-api-hpa)
- **Min Replicas**: 2
- **Max Replicas**: 10
- **Scale Triggers**:
  - CPU > 70%
  - Memory > 80%
- **Scale-Up**: +100% every 30 seconds
- **Scale-Down**: -50% every 60 seconds (5-min stability)

### Frontend Tier (crm-frontend-hpa)
- **Min Replicas**: 2
- **Max Replicas**: 8
- **Scale Triggers**:
  - CPU > 75%
  - Memory > 80%

Monitor HPA:
```bash
kubectl get hpa -n crm-app --watch
kubectl describe hpa crm-api-hpa -n crm-app
```

## Updating Container Images

After building and pushing new images to your registry:

```bash
# Option 1: Using deployment script
.\deploy.ps1 update-images  # PowerShell
bash deploy.sh update-images  # Bash

# Option 2: Manual kubectl command
kubectl set image deployment/crm-api crm-api=your-registry/crm-api:v1.0.0 -n crm-app
kubectl set image deployment/crm-frontend crm-frontend=your-registry/crm-frontend:v1.0.0 -n crm-app

# Monitor rollout
kubectl rollout status deployment/crm-api -n crm-app
```

## Resource Usage

### Requested Resources
- **API**: 500m CPU, 512Mi memory (per pod)
- **Frontend**: 250m CPU, 256Mi memory (per pod)
- **Database**: 250m CPU, 256Mi memory

### Resource Limits
- **API**: 1000m CPU, 1Gi memory
- **Frontend**: 500m CPU, 512Mi memory
- **Database**: 500m CPU, 512Mi memory

### Total Quota
- CPU: 10 cores
- Memory: 20Gi
- Pods: 50

## Database Persistence

The database runs in a StatefulSet with persistent storage:
- **Storage**: 10Gi PersistentVolume
- **Location**: /data (in container)
- **File**: crm.db
- **Retention**: Data persists across pod restarts

Backup database:
```bash
kubectl exec -n crm-app crm-db-0 -- cp /data/crm.db /data/crm.db.backup
```

## Health Checks

### Endpoints Created
- `GET /health` - Basic health status
- `GET /health/ready` - Readiness with dependency checks
- `GET /health/live` - Liveness status

### Probe Configuration
- **Liveness**: Detects and restarts dead containers
- **Readiness**: Removes unhealthy pods from load balancing
- **Initial Delay**: 10-30 seconds
- **Period**: 10 seconds
- **Timeout**: 3 seconds
- **Failure Threshold**: 2-3 attempts

## CI/CD Integration

GitHub Actions workflow included (.github/workflows/docker-build-deploy.yml):
- Automatic Docker image builds on push
- Backend and frontend tests
- Automatic push to container registry
- Automatic deployment on merge to main branch

Configure secrets in GitHub:
- `KUBE_CONFIG`: Base64-encoded kubeconfig

## Troubleshooting

### Check Pod Status
```bash
kubectl get pods -n crm-app -o wide
kubectl describe pod <pod-name> -n crm-app
```

### View Logs
```bash
kubectl logs -n crm-app <pod-name>
kubectl logs -n crm-app <pod-name> --previous  # Previous container
```

### Port Forwarding
```bash
# Windows PowerShell
.\deploy.ps1 forward

# Linux/macOS
bash deploy.sh forward
```

### HPA Not Scaling
1. Check metrics availability:
   ```bash
   kubectl get --raw /apis/custom.metrics.k8s.io/v1beta1
   ```
2. Ensure metrics-server is installed:
   ```bash
   kubectl get deployment metrics-server -n kube-system
   ```

### Database Connection Issues
```bash
# Check database pod logs
kubectl logs -n crm-app crm-db-0

# Check PVC status
kubectl get pvc -n crm-app
kubectl describe pvc crm-db-pvc -n crm-app
```

## Next Steps

1. **Update Image Registry**: Replace `your-registry` in manifests
2. **Configure Domain**: Update Ingress hostname in 04-ingress-network.yaml
3. **Setup Secrets**: Update JWT and database secrets
4. **Install Metrics Server**: Required for HPA to function
5. **Setup NGINX Ingress**: For external access
6. **Configure TLS**: Install cert-manager and letsencrypt issuer
7. **Setup Monitoring**: Add Prometheus/Grafana
8. **Setup Logging**: Add ELK Stack or similar

## Reference Documentation

See `DOCKER_KUBERNETES_GUIDE.md` for:
- Detailed setup instructions
- Configuration management
- Scaling and monitoring
- Security best practices
- CI/CD integration examples
- Troubleshooting guide

## Support

For issues or questions:
1. Check pod logs: `kubectl logs -n crm-app <pod-name>`
2. Verify resource availability: `kubectl top nodes`
3. Check HPA status: `kubectl describe hpa crm-api-hpa -n crm-app`
4. Review manifests in kubernetes/ directory
