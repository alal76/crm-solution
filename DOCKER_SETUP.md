# Docker Setup and Image Building Guide

## Prerequisites

### 1. Install Docker Desktop

**For Windows:**
- Download Docker Desktop from: https://www.docker.com/products/docker-desktop
- Install and restart your machine
- Verify installation:
  ```powershell
  docker --version
  docker run hello-world
  ```

**System Requirements:**
- Windows 10/11 Pro, Enterprise, or Education edition
- Hyper-V enabled
- At least 4GB RAM
- WSL 2 backend recommended

---

## Building Docker Images

Once Docker is installed, follow these steps:

### 1. Build Backend Image

```powershell
cd "c:\Users\AbhishekLal\OneDrive - HSO\Documents\Work\Vibe\CRM"

docker build -f Dockerfile.backend -t ghcr.io/alal76/crm-api:latest -t ghcr.io/alal76/crm-api:v1.0.0 .
```

This will:
- Use the `Dockerfile.backend` file
- Tag it as `ghcr.io/alal76/crm-api:latest` and `ghcr.io/alal76/crm-api:v1.0.0`
- Build the .NET API image (takes 2-5 minutes)

### 2. Build Frontend Image

```powershell
docker build -f Dockerfile.frontend -t ghcr.io/alal76/crm-frontend:latest -t ghcr.io/alal76/crm-frontend:v1.0.0 .
```

This will:
- Use the `Dockerfile.frontend` file
- Tag it as `ghcr.io/alal76/crm-frontend:latest` and `ghcr.io/alal76/crm-frontend:v1.0.0`
- Build the React app image (takes 3-8 minutes)

### 3. Verify Images Built Successfully

```powershell
docker images | Select-String "crm-api|crm-frontend"
```

You should see output like:
```
ghcr.io/alal76/crm-api        latest    abc123def456   2 minutes ago   180MB
ghcr.io/alal76/crm-frontend   latest    def456ghi789   5 minutes ago   95MB
```

---

## Pushing Images to GitHub Container Registry (GHCR)

### 1. Generate GitHub Personal Access Token (PAT)

1. Go to: https://github.com/settings/tokens
2. Click "Generate new token" → "Generate new token (classic)"
3. Name: `Docker Push Token`
4. Select scopes:
   - ✅ `write:packages` - Push packages
   - ✅ `delete:packages` - Delete packages (optional)
   - ✅ `read:packages` - Read packages
5. Click "Generate token"
6. **Copy the token** (you won't see it again!)

### 2. Authenticate Docker with GHCR

```powershell
# Option A: Using GitHub token
$token = "ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
$token | docker login ghcr.io -u alal76 --password-stdin

# Option B: Interactive (safer)
docker login ghcr.io
# Then enter:
# Username: alal76
# Password: <paste your PAT token>
```

### 3. Push Backend Image

```powershell
docker push ghcr.io/alal76/crm-api:latest
docker push ghcr.io/alal76/crm-api:v1.0.0
```

### 4. Push Frontend Image

```powershell
docker push ghcr.io/alal76/crm-frontend:latest
docker push ghcr.io/alal76/crm-frontend:v1.0.0
```

### 5. Verify Images in Registry

1. Go to: https://github.com/users/alal76/packages
2. You should see:
   - `crm-api` package
   - `crm-frontend` package

---

## Alternative Registries

If you prefer other registries:

### Docker Hub
```powershell
docker tag ghcr.io/alal76/crm-api:latest alal76/crm-api:latest
docker push alal76/crm-api:latest
```

### Azure Container Registry (ACR)
```powershell
docker tag ghcr.io/alal76/crm-api:latest myregistry.azurecr.io/crm-api:latest
docker login myregistry.azurecr.io
docker push myregistry.azurecr.io/crm-api:latest
```

### AWS Elastic Container Registry (ECR)
```powershell
# First authenticate
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin 123456789.dkr.ecr.us-east-1.amazonaws.com

# Then tag and push
docker tag ghcr.io/alal76/crm-api:latest 123456789.dkr.ecr.us-east-1.amazonaws.com/crm-api:latest
docker push 123456789.dkr.ecr.us-east-1.amazonaws.com/crm-api:latest
```

---

## Docker Commands Reference

```powershell
# View all images
docker images

# View image details
docker inspect ghcr.io/alal76/crm-api:latest

# Run image locally for testing
docker run -p 5000:5000 ghcr.io/alal76/crm-api:latest
docker run -p 3000:3000 ghcr.io/alal76/crm-frontend:latest

# Remove images
docker rmi ghcr.io/alal76/crm-api:latest
docker rmi ghcr.io/alal76/crm-frontend:latest

# Clean up all unused Docker resources
docker system prune
```

---

## Next Steps

Once images are pushed:

1. Update Kubernetes manifests if using different registry
2. Install kubectl (if not already installed)
3. Setup Kubernetes cluster (Docker Desktop, Minikube, or cloud cluster)
4. Run deployment: `.\deploy.ps1 deploy`

---

## Troubleshooting

### Build fails with "npm install" errors
- Ensure `CRM.Frontend/build` directory was removed before building
- Try: `docker build --no-cache -f Dockerfile.frontend -t ghcr.io/alal76/crm-frontend:latest .`

### Push fails with authentication error
- Verify PAT token has correct scopes
- Try logout and login again: `docker logout ghcr.io && docker login ghcr.io`

### Image is too large
- Both images should be ~50-200MB each
- Multi-stage builds ensure production images are slim

### Docker daemon not running
- On Windows: Ensure Docker Desktop is running (check system tray)
- Restart Docker Desktop if needed
