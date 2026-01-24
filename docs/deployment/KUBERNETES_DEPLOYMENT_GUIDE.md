# Kubernetes Deployment Guide

## ✅ Build Status
- ✅ Backend built successfully (dotnet build)
- ✅ Frontend built successfully (npm run build)
- ⏳ Ready for Docker container and Kubernetes deployment

## Prerequisites

### 1. Docker Desktop with Kubernetes Enabled
You have Docker installed (v29.1.3). You need to enable Kubernetes:

**Windows:**
1. Open Docker Desktop
2. Go to Settings → Kubernetes
3. Check "Enable Kubernetes"
4. Click "Apply & Restart"
5. Wait for Kubernetes to start (5-10 minutes)

Or install **Minikube** for local Kubernetes:
```powershell
# Install Minikube
choco install minikube -y

# Start Minikube cluster
minikube start --driver=docker

# Verify it's running
kubectl cluster-info
```

### 2. Required Tools (Already Installed)
- ✅ Docker (v29.1.3)
- ✅ kubectl (v1.34.1)

### 3. Verify Prerequisites

```powershell
# Check Docker
docker ps

# Check Kubernetes cluster
kubectl cluster-info

# Check nodes
kubectl get nodes

# Check metrics-server (for HPA)
kubectl get deployment metrics-server -n kube-system
```

## Step 1: Build Docker Images

### Option A: Build and Push to Registry

```powershell
# Set your registry details
$REGISTRY = "ghcr.io/alal76"
$IMAGE_TAG = "latest"

# Build Backend Image
docker build -f Dockerfile.backend -t ${REGISTRY}/crm-api:${IMAGE_TAG} .

# Build Frontend Image
docker build -f Dockerfile.frontend -t ${REGISTRY}/crm-frontend:${IMAGE_TAG} .

# Login to registry (if using private registry)
docker login ghcr.io

# Push images
docker push ${REGISTRY}/crm-api:${IMAGE_TAG}
docker push ${REGISTRY}/crm-frontend:${IMAGE_TAG}
```

### Option B: Build for Local Kubernetes (Minikube)

```powershell
# Point Docker CLI to Minikube's Docker daemon
minikube -p minikube docker-env | Invoke-Expression

# Build images (they'll be available to Minikube)
docker build -f Dockerfile.backend -t crm-api:latest .
docker build -f Dockerfile.frontend -t crm-frontend:latest .

# Verify images in Minikube
minikube image ls
```

## Step 2: Update Kubernetes Manifests

Edit the following files to use your image registry:

### kubernetes/02-application-tier.yaml
```yaml
spec:
  template:
    spec:
      containers:
      - name: crm-api
        image: ghcr.io/alal76/crm-api:latest  # Update with your registry
```

### kubernetes/03-presentation-tier.yaml
```yaml
spec:
  template:
    spec:
      containers:
      - name: crm-frontend
        image: ghcr.io/alal76/crm-frontend:latest  # Update with your registry
```

**For Minikube (use local images):**
```yaml
# Change to:
image: crm-api:latest
imagePullPolicy: Never  # Use locally built image
```

## Step 3: Deploy to Kubernetes

### Using the Deployment Script

```powershell
# Navigate to project root
cd c:\Users\AbhishekLal\OneDrive - HSO\Documents\Work\Vibe\CRM

# Run deployment
.\deploy.ps1 deploy

# When prompted, enter your registry:
# Example: ghcr.io/alal76
```

### Or Deploy Manually

```powershell
# Create namespace
kubectl create namespace crm-app

# Apply manifests in order
kubectl apply -f kubernetes/00-namespace-config.yaml
kubectl apply -f kubernetes/01-database-tier.yaml
kubectl apply -f kubernetes/02-application-tier.yaml
kubectl apply -f kubernetes/03-presentation-tier.yaml
kubectl apply -f kubernetes/04-ingress-network.yaml

# Check deployment status
kubectl get pods -n crm-app
kubectl get svc -n crm-app
```

## Step 4: Access Your Application

### Port Forward (Recommended for Local Development)

```powershell
# Forward to API
kubectl port-forward -n crm-app svc/crm-api 5000:5000

# In another terminal, forward to Frontend
kubectl port-forward -n crm-app svc/crm-frontend 3000:3000

# Access at:
# API: http://localhost:5000
# Frontend: http://localhost:3000
# Swagger: http://localhost:5000/swagger
```

### Using the Script

```powershell
.\deploy.ps1 forward
```

## Deployment Monitoring

### Check Deployment Status

```powershell
# Watch deployments
kubectl get deployments -n crm-app -w

# Check pod status
kubectl get pods -n crm-app -o wide

# Check services
kubectl get svc -n crm-app

# Check persistent volumes
kubectl get pvc -n crm-app

# Check HPA status
kubectl get hpa -n crm-app
```

### View Logs

```powershell
# API logs
kubectl logs -n crm-app -l app=crm,tier=api --tail=100 -f

# Frontend logs
kubectl logs -n crm-app -l app=crm,tier=frontend --tail=100 -f

# Or use the script
.\deploy.ps1 logs api 100
.\deploy.ps1 logs frontend 100
```

### Describe Resources

```powershell
# Check pod details
kubectl describe pod <pod-name> -n crm-app

# Check deployment
kubectl describe deployment crm-api -n crm-app

# Check events
kubectl get events -n crm-app --sort-by='.lastTimestamp'
```

## Scaling

### Manual Scaling

```powershell
# Scale API to 3 replicas
kubectl scale deployment/crm-api -n crm-app --replicas=3

# Scale Frontend to 2 replicas
kubectl scale deployment/crm-frontend -n crm-app --replicas=2

# Or use the script
.\deploy.ps1 scale api 3
.\deploy.ps1 scale frontend 2
```

### Auto-Scaling (HPA)

The configuration includes Horizontal Pod Autoscaler:
- **API**: Scales 2-10 replicas at 70% CPU usage
- **Frontend**: Scales 2-5 replicas at 70% CPU usage

View HPA status:
```powershell
kubectl get hpa -n crm-app
kubectl describe hpa crm-api -n crm-app
```

## Database Management

### Run Migrations

```powershell
# Before deploying, ensure migrations are applied to your database

# Local migration (SQLite in this example)
cd CRM.Backend
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api

# Or if using external database, update connection string in:
# kubernetes/02-application-tier.yaml (Environment variables)
```

### Access Database in Pod

```powershell
# Exec into API pod to run migrations
kubectl exec -it <crm-api-pod-name> -n crm-app -- /bin/sh

# Inside pod:
cd /app && dotnet ef database update
```

## Updating Application

### Update Images

```powershell
# Rebuild and push new images
docker build -f Dockerfile.backend -t ghcr.io/alal76/crm-api:v2.0 .
docker push ghcr.io/alal76/crm-api:v2.0

# Update deployment
kubectl set image deployment/crm-api crm-api=ghcr.io/alal76/crm-api:v2.0 -n crm-app

# Or use script
.\deploy.ps1 update-images
```

### Rollback Deployment

```powershell
# Check rollout history
kubectl rollout history deployment/crm-api -n crm-app

# Rollback to previous version
kubectl rollout undo deployment/crm-api -n crm-app
```

## Cleanup

### Remove Deployment

```powershell
# Delete namespace (removes all resources)
kubectl delete namespace crm-app

# Or use script
.\deploy.ps1 cleanup
```

### Stop Minikube

```powershell
minikube stop
minikube delete  # To completely remove
```

## Troubleshooting

### Kubernetes cluster not found
```powershell
# Ensure Kubernetes is running
# Docker Desktop: Check Settings → Kubernetes
# Minikube: Run `minikube start`

# Test connection
kubectl cluster-info
```

### ImagePullBackOff Error
```powershell
# Check if image exists in registry
docker image ls

# For Minikube, use local images with imagePullPolicy: Never
# Or push to registry and ensure credentials are configured

# Check image pull details
kubectl describe pod <pod-name> -n crm-app
```

### Pod not starting
```powershell
# Check pod events
kubectl describe pod <pod-name> -n crm-app

# Check pod logs
kubectl logs <pod-name> -n crm-app
```

### Database connection errors
```powershell
# Verify database is running
kubectl get pod -n crm-app -l tier=database

# Check connection string in environment variables
kubectl describe deployment crm-api -n crm-app

# Exec into pod and test connection
kubectl exec -it <crm-api-pod-name> -n crm-app -- /bin/sh
```

### Persistent Volume issues
```powershell
# Check PVC status
kubectl get pvc -n crm-app

# Check PV status
kubectl get pv

# Describe PVC for details
kubectl describe pvc crm-db-pvc -n crm-app
```

## Environment Variables

The deployment uses these configurable environment variables:

| Variable | Default | Purpose |
|----------|---------|---------|
| KUBE_NAMESPACE | crm-app | Kubernetes namespace |
| DOCKER_REGISTRY | (none) | Docker registry URL |
| IMAGE_TAG | latest | Container image tag |
| ENVIRONMENT | production | Deployment environment |

### Set Environment Variables

```powershell
$env:KUBE_NAMESPACE = "crm-app"
$env:DOCKER_REGISTRY = "ghcr.io/alal76"
$env:IMAGE_TAG = "v1.0.0"
$env:ENVIRONMENT = "production"

# Then run deployment
.\deploy.ps1 deploy
```

## Quick Reference

```powershell
# Start Minikube
minikube start --driver=docker

# Build images
docker build -f Dockerfile.backend -t ghcr.io/alal76/crm-api:latest .
docker build -f Dockerfile.frontend -t ghcr.io/alal76/crm-frontend:latest .

# Deploy
.\deploy.ps1 deploy

# Port forward
.\deploy.ps1 forward

# Check status
.\deploy.ps1 verify

# View logs
.\deploy.ps1 logs api 100
.\deploy.ps1 logs frontend 100

# Scale
.\deploy.ps1 scale api 3

# Update images
.\deploy.ps1 update-images

# Cleanup
.\deploy.ps1 cleanup
```

---

**Next Steps:**
1. Start Docker Desktop or Minikube
2. Build Docker images
3. Update Kubernetes manifests with your registry
4. Run `.\deploy.ps1 deploy`
5. Monitor with `.\deploy.ps1 verify`
6. Access application with `.\deploy.ps1 forward`
