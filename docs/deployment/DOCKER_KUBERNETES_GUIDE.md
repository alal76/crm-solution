# Docker & Kubernetes Deployment Guide

## Architecture Overview

The CRM application follows a **three-tier architecture**:

1. **Presentation Tier (Frontend)**: React application
2. **Application Tier (API)**: .NET 8 ASP.NET Core backend
3. **Data Tier (Database)**: SQLite with persistent storage

Each tier has:
- Independent Docker containers
- Kubernetes deployments with replica sets
- Horizontal Pod Autoscaling (HPA)
- Health checks and resource management
- Networking and service discovery

## Prerequisites

### For Docker:
- Docker 20.10+
- Docker Compose 2.0+

### For Kubernetes:
- Kubernetes 1.24+
- kubectl configured to access your cluster
- Metrics Server (for HPA)
- NGINX Ingress Controller (optional, for external access)

## Local Development with Docker Compose

### Build and Run

```bash
# Build images
docker-compose build

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

### Access Application

- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger API Docs: http://localhost:5000/swagger

### Environment Variables (docker-compose.yml)

- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `DatabaseProvider`: sqlite
- `REACT_APP_API_URL`: API endpoint for frontend

## Kubernetes Deployment

### Prerequisites Setup

```bash
# 1. Create container registry secret (if using private registry)
kubectl create secret docker-registry regcred \
  --docker-server=your-registry \
  --docker-username=your-username \
  --docker-password=your-password \
  -n crm-app

# 2. Install Metrics Server (for HPA to work)
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml

# 3. Install NGINX Ingress Controller (optional)
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm install nginx-ingress ingress-nginx/ingress-nginx
```

### Deploy to Kubernetes

```bash
# Apply all manifests in order
kubectl apply -f kubernetes/00-namespace-config.yaml
kubectl apply -f kubernetes/01-database-tier.yaml
kubectl apply -f kubernetes/02-application-tier.yaml
kubectl apply -f kubernetes/03-presentation-tier.yaml
kubectl apply -f kubernetes/04-ingress-network.yaml

# Verify deployment
kubectl get all -n crm-app
kubectl get hpa -n crm-app
kubectl get pvc -n crm-app
```

### Configuration Management

**ConfigMap (kubernetes/00-namespace-config.yaml)**:
- Non-sensitive application configuration
- Environment settings

**Secrets (kubernetes/00-namespace-config.yaml)**:
- JWT secrets
- Database connection strings
- API credentials

Update secrets:
```bash
# Update secrets
kubectl create secret generic crm-secrets \
  --from-literal=JWT_SECRET='your-new-secret' \
  --from-literal=JWT_ISSUER='CRMApp' \
  --from-literal=JWT_AUDIENCE='CRMUsers' \
  --from-literal=DB_CONNECTION_STRING='Data Source=/data/crm.db' \
  -n crm-app --dry-run=client -o yaml | kubectl apply -f -
```

## Autoscaling Configuration

### Horizontal Pod Autoscaler (HPA)

The deployment includes HPA for both frontend and API:

**API Tier (crm-api-hpa)**:
- Min replicas: 2
- Max replicas: 10
- CPU threshold: 70%
- Memory threshold: 80%
- Scale-up: +100% per 30 seconds
- Scale-down: -50% per 60 seconds (stable window)

**Frontend Tier (crm-frontend-hpa)**:
- Min replicas: 2
- Max replicas: 8
- CPU threshold: 75%
- Memory threshold: 80%

Monitor HPA:
```bash
# Watch HPA status
kubectl get hpa -n crm-app --watch

# Detailed HPA info
kubectl describe hpa crm-api-hpa -n crm-app

# View HPA events
kubectl get events -n crm-app --sort-by='.lastTimestamp'
```

## Building and Pushing Docker Images

### Build Images

```bash
# Build frontend image
docker build -f Dockerfile.frontend -t your-registry/crm-frontend:latest .

# Build backend image
docker build -f Dockerfile.backend -t your-registry/crm-api:latest .
```

### Push to Registry

```bash
# Login to registry
docker login your-registry

# Push images
docker push your-registry/crm-frontend:latest
docker push your-registry/crm-api:latest
```

### Update Kubernetes Deployment

Update image references in `02-application-tier.yaml` and `03-presentation-tier.yaml`:

```yaml
image: your-registry/crm-api:latest
image: your-registry/crm-frontend:latest
```

## Health Checks and Monitoring

### Liveness & Readiness Probes

All containers have:
- **Liveness Probe**: Restarts unhealthy containers
- **Readiness Probe**: Removes pod from load balancer if not ready

### Resource Limits

**API Container**:
- Request: 500m CPU, 512Mi memory
- Limit: 1000m CPU, 1Gi memory

**Frontend Container**:
- Request: 250m CPU, 256Mi memory
- Limit: 500m CPU, 512Mi memory

**Database Container**:
- Request: 250m CPU, 256Mi memory
- Limit: 500m CPU, 512Mi memory

Monitor resource usage:
```bash
# View resource usage
kubectl top nodes -n crm-app
kubectl top pods -n crm-app

# View pod resource limits
kubectl describe pod -n crm-app
```

## Persistence and Storage

### Database Storage

- **PersistentVolume**: 10Gi storage class
- **Claim**: crm-db-pvc
- **Mount Path**: /data
- **Database File**: /data/crm.db

Verify storage:
```bash
kubectl get pv,pvc -n crm-app
kubectl describe pvc crm-db-pvc -n crm-app
```

## Networking

### Services

1. **crm-api**: ClusterIP service on port 5000
2. **crm-frontend**: ClusterIP service on port 3000
3. **crm-db**: Headless service for database

### Ingress

Maps domain to services:
- `crm.example.com/api/*` → crm-api:5000
- `crm.example.com/` → crm-frontend:3000

Configure domain in `04-ingress-network.yaml`:
```yaml
- host: your-domain.com
```

### Network Policy

Restricts pod-to-pod communication:
- Pods can communicate within crm-app namespace
- External DNS queries allowed
- Egress restricted to namespace and DNS

## Troubleshooting

### Check Deployment Status

```bash
# View deployment status
kubectl get deployments -n crm-app
kubectl describe deployment crm-api -n crm-app

# View pod status
kubectl get pods -n crm-app -o wide
kubectl describe pod <pod-name> -n crm-app

# View pod logs
kubectl logs -n crm-app <pod-name>
kubectl logs -n crm-app <pod-name> --previous
```

### Debug Failed Pods

```bash
# Get pod details
kubectl get events -n crm-app

# Execute command in pod
kubectl exec -it <pod-name> -n crm-app -- /bin/sh

# Port forward to pod
kubectl port-forward -n crm-app <pod-name> 5000:5000
```

### Check HPA Status

```bash
kubectl get hpa -n crm-app
kubectl describe hpa crm-api-hpa -n crm-app

# Check metrics availability
kubectl get --raw /apis/custom.metrics.k8s.io/v1beta1
```

## Cleanup

### Delete Kubernetes Deployment

```bash
kubectl delete namespace crm-app
```

### Delete Docker Containers

```bash
docker-compose down -v
```

## Performance Optimization

1. **Container Images**: Use Alpine/lightweight base images
2. **Resource Requests**: Set appropriate CPU/memory requests
3. **Pod Disruption Budgets**: Ensure availability during updates
4. **Anti-affinity**: Spreads pods across nodes
5. **Health Checks**: Quick failure detection
6. **Caching**: Implement HTTP caching headers

## Security Best Practices

1. **Secrets Management**: Use Kubernetes Secrets for sensitive data
2. **RBAC**: Configure service accounts and role bindings
3. **Network Policies**: Restrict pod communication
4. **Image Scanning**: Scan images for vulnerabilities
5. **Resource Quotas**: Prevent resource exhaustion
6. **SSL/TLS**: Use cert-manager for automatic certificate management

## CI/CD Integration

For automated deployment, integrate with:
- GitHub Actions
- GitLab CI/CD
- Jenkins
- ArgoCD

Example GitLab CI pipeline:
```yaml
build:
  script:
    - docker build -f Dockerfile.backend -t registry/crm-api:$CI_COMMIT_SHA .
    - docker push registry/crm-api:$CI_COMMIT_SHA

deploy:
  script:
    - kubectl set image deployment/crm-api crm-api=registry/crm-api:$CI_COMMIT_SHA -n crm-app
```
