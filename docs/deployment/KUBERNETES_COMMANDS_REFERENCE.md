# Quick Reference - Docker & Kubernetes Commands

## Docker Compose (Local Development)

### Build & Run
```bash
# Build images
docker-compose build

# Start all services
docker-compose up -d

# Stop services
docker-compose down

# Remove volumes (clean slate)
docker-compose down -v

# View logs
docker-compose logs -f
docker-compose logs -f api      # Only API logs
docker-compose logs -f frontend # Only frontend logs

# Execute command in container
docker-compose exec api dotnet --version
docker-compose exec frontend npm --version
```

### View Services
```bash
# List containers
docker-compose ps

# Access container shell
docker-compose exec api /bin/bash
docker-compose exec frontend /bin/sh

# Rebuild after code changes
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## Docker Images (Manual Build)

### Build Images
```bash
# Build frontend image
docker build -f Dockerfile.frontend -t crm-frontend:latest .

# Build backend image
docker build -f Dockerfile.backend -t crm-api:latest .

# Build with buildkit (faster)
DOCKER_BUILDKIT=1 docker build -f Dockerfile.frontend -t crm-frontend:latest .
```

### Push to Registry
```bash
# Tag images
docker tag crm-api:latest your-registry/crm-api:latest
docker tag crm-frontend:latest your-registry/crm-frontend:latest

# Login to registry
docker login your-registry

# Push images
docker push your-registry/crm-api:latest
docker push your-registry/crm-frontend:latest

# Multi-platform build (for ARM64)
docker buildx build --platform linux/amd64,linux/arm64 \
  -f Dockerfile.backend \
  -t your-registry/crm-api:latest \
  --push .
```

## Kubernetes Deployment

### Using Deployment Scripts (Recommended)

#### Windows (PowerShell)
```powershell
# Deploy all services
.\deploy.ps1 deploy

# Verify deployment
.\deploy.ps1 verify

# Setup port forwarding
.\deploy.ps1 forward

# View logs
.\deploy.ps1 logs api 100
.\deploy.ps1 logs frontend 50

# Scale deployment
.\deploy.ps1 scale api 5
.\deploy.ps1 scale frontend 3

# Update images
$env:DOCKER_REGISTRY = "your-registry"
$env:IMAGE_TAG = "v1.0.0"
.\deploy.ps1 update-images

# Cleanup
.\deploy.ps1 cleanup
```

#### Linux/macOS (Bash)
```bash
# Deploy all services
bash deploy.sh deploy

# Verify deployment
bash deploy.sh verify

# Setup port forwarding
bash deploy.sh forward

# View logs
bash deploy.sh logs api 100
bash deploy.sh logs frontend 50

# Scale deployment
bash deploy.sh scale api 5
bash deploy.sh scale frontend 3

# Update images
export DOCKER_REGISTRY="your-registry"
export IMAGE_TAG="v1.0.0"
bash deploy.sh update-images

# Cleanup
bash deploy.sh cleanup
```

### Manual kubectl Commands

#### Deployment
```bash
# Apply all manifests
kubectl apply -f kubernetes/

# Apply specific tier
kubectl apply -f kubernetes/01-database-tier.yaml
kubectl apply -f kubernetes/02-application-tier.yaml
kubectl apply -f kubernetes/03-presentation-tier.yaml

# Delete deployment
kubectl delete namespace crm-app

# Dry run (see what would be applied)
kubectl apply -f kubernetes/ --dry-run=client -o yaml
```

#### Checking Status
```bash
# List all resources
kubectl get all -n crm-app

# List pods
kubectl get pods -n crm-app
kubectl get pods -n crm-app -o wide           # More details
kubectl get pods -n crm-app -o yaml           # Full definition
kubectl get pods -n crm-app -w                # Watch in real-time

# List deployments
kubectl get deployments -n crm-app
kubectl describe deployment crm-api -n crm-app

# List services
kubectl get svc -n crm-app

# List HPA
kubectl get hpa -n crm-app
kubectl describe hpa crm-api-hpa -n crm-app

# List PVC
kubectl get pvc -n crm-app
kubectl describe pvc crm-db-pvc -n crm-app
```

#### Logs & Debugging
```bash
# View logs
kubectl logs -n crm-app <pod-name>
kubectl logs -n crm-app <pod-name> --previous    # Previous crashed container
kubectl logs -n crm-app <pod-name> -f            # Follow logs
kubectl logs -n crm-app <pod-name> --tail=100   # Last 100 lines

# View logs from deployment
kubectl logs -n crm-app -l app=crm,tier=application   # All API pods
kubectl logs -n crm-app -l app=crm,tier=presentation  # All frontend pods

# Describe pod (shows events)
kubectl describe pod <pod-name> -n crm-app

# Get pod details
kubectl get pod <pod-name> -n crm-app -o yaml

# Port forward to pod
kubectl port-forward -n crm-app <pod-name> 5000:5000
kubectl port-forward -n crm-app <pod-name> 3000:3000

# Execute command in pod
kubectl exec -it <pod-name> -n crm-app -- /bin/bash
kubectl exec -it <pod-name> -n crm-app -- dotnet --version

# Get shell in pod (if no bash)
kubectl debug <pod-name> -n crm-app -it --image=busybox
```

#### Scaling
```bash
# Manual scaling
kubectl scale deployment crm-api --replicas=5 -n crm-app
kubectl scale deployment crm-frontend --replicas=3 -n crm-app

# Auto-scaling info
kubectl get hpa -n crm-app --watch
kubectl top pods -n crm-app                      # CPU/Memory usage
kubectl top nodes                                 # Node usage

# Generate load (for testing HPA)
kubectl run -n crm-app -i --tty load-generator --rm --image=busybox --restart=Never -- /bin/sh
# Inside the pod: while sleep 0.01; do wget -q -O- http://crm-frontend:3000; done
```

#### Updates & Rollouts
```bash
# Update image
kubectl set image deployment/crm-api crm-api=your-registry/crm-api:v1.0.0 -n crm-app
kubectl set image deployment/crm-frontend crm-frontend=your-registry/crm-frontend:v1.0.0 -n crm-app

# Watch rollout
kubectl rollout status deployment/crm-api -n crm-app --timeout=5m
kubectl rollout status deployment/crm-frontend -n crm-app --timeout=5m

# Check rollout history
kubectl rollout history deployment/crm-api -n crm-app

# Rollback to previous version
kubectl rollout undo deployment/crm-api -n crm-app
kubectl rollout undo deployment/crm-api --to-revision=2 -n crm-app

# Pause/Resume rollout
kubectl rollout pause deployment/crm-api -n crm-app
kubectl rollout resume deployment/crm-api -n crm-app
```

#### Configuration
```bash
# View ConfigMap
kubectl get configmap crm-config -n crm-app -o yaml

# View Secrets
kubectl get secret crm-secrets -n crm-app -o yaml

# Update secret
kubectl create secret generic crm-secrets \
  --from-literal=JWT_SECRET='new-secret' \
  -n crm-app --dry-run=client -o yaml | kubectl apply -f -

# Edit ConfigMap (opens editor)
kubectl edit configmap crm-config -n crm-app

# Edit Deployment
kubectl edit deployment crm-api -n crm-app
```

#### Resource Management
```bash
# View resource quotas
kubectl get resourcequota -n crm-app
kubectl describe resourcequota crm-quota -n crm-app

# View resource usage
kubectl top nodes
kubectl top pods -n crm-app --sort-by=memory
kubectl top pods -n crm-app --sort-by=cpu

# Get pod resource requests/limits
kubectl get pods -n crm-app -o custom-columns=NAME:.metadata.name,CPU_REQ:.spec.containers[*].resources.requests.cpu,CPU_LIM:.spec.containers[*].resources.limits.cpu
```

## Ingress & Networking

### Ingress Commands
```bash
# List ingress
kubectl get ingress -n crm-app

# Describe ingress
kubectl describe ingress crm-ingress -n crm-app

# Get ingress details
kubectl get ingress crm-ingress -n crm-app -o yaml

# Check ingress controller
kubectl get pods -n ingress-nginx   # NGINX ingress
kubectl get pods -n istio-system    # Istio ingress
```

### Port Forwarding
```bash
# Forward to service
kubectl port-forward svc/crm-api 5000:5000 -n crm-app &
kubectl port-forward svc/crm-frontend 3000:3000 -n crm-app &

# Kill port forwarding
kill %1  # Kill first background job
fg       # Bring to foreground if needed
```

## Monitoring & Events

### Events
```bash
# Get all events in namespace
kubectl get events -n crm-app

# Sort by timestamp
kubectl get events -n crm-app --sort-by='.lastTimestamp'

# Watch events in real-time
kubectl get events -n crm-app --watch

# Get pod-specific events
kubectl describe pod <pod-name> -n crm-app
```

### Resource Monitoring
```bash
# Get resource usage
kubectl top nodes
kubectl top pods -n crm-app

# Export metrics (for Prometheus)
kubectl get --raw /metrics
kubectl get --raw /apis/custom.metrics.k8s.io/v1beta1
```

## Cleanup & Deletion

### Delete Resources
```bash
# Delete entire namespace (deletes all resources)
kubectl delete namespace crm-app

# Delete specific resource
kubectl delete deployment crm-api -n crm-app
kubectl delete service crm-frontend -n crm-app
kubectl delete pvc crm-db-pvc -n crm-app

# Delete using manifest
kubectl delete -f kubernetes/

# Force delete stuck pod
kubectl delete pod <pod-name> -n crm-app --grace-period=0 --force
```

## Useful Aliases

```bash
# Add to ~/.bashrc or ~/.zshrc
alias k='kubectl'
alias kga='kubectl get all'
alias kn='kubectl config set-context --current --namespace'
alias kl='kubectl logs'
alias kd='kubectl describe'
alias ke='kubectl exec'
alias kaf='kubectl apply -f'
alias kdel='kubectl delete'
alias kgp='kubectl get pods'
alias kgs='kubectl get svc'
```

## Environment Variables

```bash
# Set default namespace
export KUBECONFIG=~/.kube/config
kubectl config set-context --current --namespace=crm-app

# For deployment scripts
export KUBE_NAMESPACE=crm-app
export DOCKER_REGISTRY=your-registry
export IMAGE_TAG=latest
export ENVIRONMENT=production
```

## Tips & Tricks

### Quick Health Check
```bash
# Check all pods are running
kubectl get pods -n crm-app -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.status.phase}{"\n"}{end}'

# Check restart count
kubectl get pods -n crm-app -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.status.containerStatuses[0].restartCount}{"\n"}{end}'
```

### Get Access Info
```bash
# Get service endpoint
kubectl get svc crm-api -n crm-app -o jsonpath='{.spec.clusterIP}:{.spec.ports[0].port}'

# Get pod IP
kubectl get pods <pod-name> -n crm-app -o jsonpath='{.status.podIP}'

# Get node IP
kubectl get nodes -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.status.addresses[?(@.type=="ExternalIP")].address}{"\n"}{end}'
```
