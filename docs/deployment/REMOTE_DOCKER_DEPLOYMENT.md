# Remote Docker Deployment Guide

## ✅ Configuration Complete

Your remote Docker server credentials have been securely configured:

- **Remote Host:** 192.168.0.9
- **Connection Type:** Docker Host (SSH)
- **Username:** root
- **Registry:** ghcr/alal76
- **Environment:** development

The configuration is saved in `.docker-remote-config.json` (excluded from git)

---

## 📋 Deployment Steps

### Step 1: Verify Remote Server Requirements

Before deploying, ensure your remote Docker server has:

1. **Docker installed and running**
   ```bash
   # SSH into remote server
   ssh root@192.168.0.9
   
   # Check Docker version
   docker --version
   
   # Check Docker daemon status
   docker ps
   ```

2. **Docker Compose installed**
   ```bash
   docker-compose --version
   ```

3. **Sufficient disk space**
   ```bash
   df -h
   ```

4. **Network connectivity to registry**
   ```bash
   # Test registry access
   docker pull hello-world
   ```

---

### Step 2: Build Docker Images (Local Machine)

Build the application images on your local machine:

#### Backend Image

```powershell
# Navigate to project root
cd "c:\Users\AbhishekLal\OneDrive - HSO\Documents\Work\Vibe\CRM"

# Build backend image
docker build -f Dockerfile.backend -t ghcr.io/alal76/crm-api:development .

# Verify build
docker images | grep crm-api
```

#### Frontend Image

```powershell
# Build frontend image
docker build -f Dockerfile.frontend -t ghcr.io/alal76/crm-frontend:development .

# Verify build
docker images | grep crm-frontend
```

---

### Step 3: Authenticate with Docker Registry

```powershell
# Login to GHCR (GitHub Container Registry)
docker login ghcr.io

# When prompted:
# Username: alal76
# Password: <your-personal-access-token>
```

---

### Step 4: Push Images to Registry

```powershell
# Push backend image
docker push ghcr.io/alal76/crm-api:development

# Push frontend image
docker push ghcr.io/alal76/crm-frontend:development

# Verify images in registry
docker images | grep ghcr.io
```

---

### Step 5: Configure docker-compose.yml

Update the `docker-compose.yml` to use your images:

```yaml
services:
  api:
    image: ghcr.io/alal76/crm-api:development
    # ... rest of configuration
    
  frontend:
    image: ghcr.io/alal76/crm-frontend:development
    # ... rest of configuration
```

---

### Step 6: Deploy to Remote Server

#### Option A: Using SSH (Recommended)

```powershell
# 1. Connect to remote server
ssh root@192.168.0.9

# 2. Navigate to application directory (or create one)
cd /opt/crm  # Create directory if needed: mkdir -p /opt/crm

# 3. Copy docker-compose.yml to remote server
# (Exit SSH first)
exit

# From local machine:
scp docker-compose.yml root@192.168.0.9:/opt/crm/

# 4. SSH back and deploy
ssh root@192.168.0.9

# 5. Login to registry on remote server
docker login ghcr.io
# Username: alal76
# Password: <personal-access-token>

# 6. Pull latest images
docker-compose pull

# 7. Start containers
docker-compose up -d

# 8. Verify deployment
docker-compose ps
docker ps
```

#### Option B: Using SSH with One Command

```powershell
# Copy compose file and deploy in one command
scp docker-compose.yml root@192.168.0.9:/opt/crm/ && `
ssh root@192.168.0.9 "cd /opt/crm && docker-compose pull && docker-compose up -d"
```

---

### Step 7: Verify Deployment

#### Check Container Status

```bash
# SSH to remote server
ssh root@192.168.0.9

# Check running containers
docker-compose ps

# Check all containers
docker ps -a

# Check container logs
docker-compose logs -f api
docker-compose logs -f frontend
docker-compose logs -f db
```

#### Test Application Access

```powershell
# From local machine, test connectivity
# API Health check
curl http://192.168.0.9:5000/health

# Frontend (if exposed)
curl http://192.168.0.9:3000
```

#### Monitor Resource Usage

```bash
# SSH to remote server
docker stats

# Check disk usage
df -h

# Check network
docker network ls
docker network inspect crm_crm-network
```

---

## 🔄 Updating Deployment

### Rebuild and Redeploy

```powershell
# 1. Rebuild images locally
docker build -f Dockerfile.backend -t ghcr.io/alal76/crm-api:development .
docker build -f Dockerfile.frontend -t ghcr.io/alal76/crm-frontend:development .

# 2. Push to registry
docker push ghcr.io/alal76/crm-api:development
docker push ghcr.io/alal76/crm-frontend:development

# 3. Update on remote server
ssh root@192.168.0.9 "cd /opt/crm && docker-compose pull && docker-compose up -d"

# 4. Verify
ssh root@192.168.0.9 "docker-compose ps"
```

### View Logs

```powershell
# View logs during deployment
ssh root@192.168.0.9 "cd /opt/crm && docker-compose logs -f"

# View specific service logs
ssh root@192.168.0.9 "cd /opt/crm && docker-compose logs api"
ssh root@192.168.0.9 "cd /opt/crm && docker-compose logs frontend"
```

### Stop Deployment

```powershell
# SSH to remote
ssh root@192.168.0.9

# Stop containers
cd /opt/crm
docker-compose down

# Or keep volumes
docker-compose down -v
```

---

## 🐛 Troubleshooting

### Connection Issues

```powershell
# Test SSH connection
ssh -v root@192.168.0.9

# Test connectivity
ping 192.168.0.9

# Check firewall (if applicable)
# Ensure ports 22 (SSH), 5000 (API), 3000 (Frontend) are open
```

### Docker Pull Authentication Errors

```bash
# On remote server, login again
docker login ghcr.io

# Verify credentials file
cat ~/.docker/config.json

# Check image accessibility
docker pull ghcr.io/alal76/crm-api:development
```

### Container Not Starting

```bash
# Check logs
docker-compose logs api

# Restart containers
docker-compose restart api

# Rebuild without cache
docker-compose build --no-cache
docker-compose up -d
```

### Database Connection Errors

```bash
# Check database container
docker ps | grep db

# Check database logs
docker-compose logs db

# Verify connection string in environment variables
docker-compose exec api env | grep -i connect
```

### Port Already in Use

```bash
# Check port usage
netstat -tlnp | grep 5000
netstat -tlnp | grep 3000

# Kill process using port (Linux)
lsof -i :5000
kill -9 <PID>

# Or change port in docker-compose.yml
```

---

## 📊 Monitoring

### Check System Resources

```bash
# SSH to remote
ssh root@192.168.0.9

# View Docker resource usage
docker stats

# View disk usage
du -sh /opt/crm

# Check system load
top
```

### View Application Logs

```bash
# API logs
ssh root@192.168.0.9 "docker-compose logs -n 100 api"

# Frontend logs
ssh root@192.168.0.9 "docker-compose logs -n 100 frontend"

# Follow logs
ssh root@192.168.0.9 "docker-compose logs -f"
```

### Health Checks

```bash
# API health
curl http://192.168.0.9:5000/health

# Frontend accessibility
curl -I http://192.168.0.9:3000

# Database connectivity (from API container)
ssh root@192.168.0.9 "docker-compose exec api curl http://localhost:5000/api/health"
```

---

## 🔐 Security Considerations

### 1. SSH Key Authentication
Instead of password, set up SSH keys:

```powershell
# Generate SSH key (if not already done)
ssh-keygen -t rsa -b 4096

# Add public key to remote server
# On remote server:
mkdir -p ~/.ssh
echo "your-public-key" >> ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys

# Test key-based auth
ssh -i C:\Users\YourUser\.ssh\id_rsa root@192.168.0.9
```

### 2. Update Configuration
```powershell
# Edit .docker-remote-config.json
# Change AuthMethod from "password" to "key"
# Update KeyPath to point to your SSH private key
```

### 3. Docker Registry Security
- Use **Personal Access Tokens** instead of passwords
- Tokens should be **read-only** for deployments
- Rotate tokens regularly
- Don't commit `.docker-remote-config.json` to git

### 4. Remote Server Security
- Keep Docker updated: `docker version`
- Use firewalls to restrict access
- Limit SSH access (disable root login if possible)
- Monitor logs: `docker-compose logs`
- Use HTTPS for exposed services

---

## 📝 Environment Configuration

Current deployment settings:

| Setting | Value |
|---------|-------|
| Remote Host | 192.168.0.9 |
| Connection Type | Docker SSH |
| Registry | ghcr.io/alal76 |
| Image Tag | development |
| Environment | development |
| Docker Compose | docker-compose.yml |

To change settings:
```powershell
cd c:\Users\AbhishekLal\OneDrive - HSO\Documents\Work\Vibe\CRM
.\deploy-remote.ps1 configure
```

---

## Quick Reference

```powershell
# View current configuration
.\deploy-remote.ps1 status

# Test connection
.\deploy-remote.ps1 test

# Show deployment plan
.\deploy-remote.ps1 deploy

# Help
.\deploy-remote.ps1 help
```

---

## Next Steps

1. ✅ **Configuration Complete** - Remote server credentials are set
2. **Verify Remote Server** - Ensure Docker is running
3. **Build Images** - Run `docker build` commands
4. **Push to Registry** - Push images to ghcr.io
5. **Deploy** - Copy docker-compose.yml and run on remote
6. **Monitor** - Check logs and container status

For detailed steps, see sections above.

---

**Last Updated:** January 19, 2026
**Status:** Ready for Deployment
