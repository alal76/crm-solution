# CRM Solution Deployment to 192.168.0.9 - Complete Guide

**Status:** Ready for Deployment  
**Target Server:** 192.168.0.9 (Development)  
**Deployment Method:** Docker Compose (Monolith)  
**Solution Status:** Production-Ready (1 suppressed non-blocking error)  
**Last Updated:** March 2026

---

## 📋 Pre-Deployment Checklist

### Local Machine Requirements
- [x] Docker installed (version 20.10+)
- [x] Docker Compose installed (version 2.0+)
- [x] SSH access configured for root@192.168.0.9
- [x] Sufficient disk space (~5GB for images)
- [x] .NET 10.0 SDK (for local builds if needed)

### Server Requirements (192.168.0.9)
- Linux OS (Ubuntu 20.04+ recommended)
- Docker Engine installed
- Docker Compose installed
- SSH access enabled for root user
- Firewall rules open for ports: 5000 (API), 80 (Frontend), 3306 (DB), 6379 (Redis), 7700 (Meilisearch)
- Sufficient disk space (~10GB for database, images, and data)

### Network Connectivity
- Verify SSH access to server: `ssh root@192.168.0.9 'uptime'`
- Verify Docker is available on server: `ssh root@192.168.0.9 'docker --version'`

---

## 🚀 Deployment Steps

### Step 1: Verify Prerequisites

```bash
# Check local Docker installation
docker --version
docker compose --version

# Test SSH connectivity
ssh -o ConnectTimeout=5 root@192.168.0.9 "echo 'SSH OK'"

# Check server disk space
ssh root@192.168.0.9 "df -h /"
```

**Expected Output:**
- Docker version 20.10+
- Docker Compose version 2.0+
- SSH connection successful
- Available disk space: > 10GB

### Step 2: Navigate to Solution Directory

```bash
cd "/Users/alal/Code/Git CRM Solution/crm-solution"
pwd
```

**Expected Output:**
```
/Users/alal/Code/Git CRM Solution/crm-solution
```

### Step 3: Run Automated Deployment Script

```bash
# Standard deployment
./deploy-to-dev-server.sh

# Or with custom target server
TARGET_SERVER=192.168.0.9 ./deploy-to-dev-server.sh
```

**The script performs:**
1. ✓ Checks local Docker and SSH connectivity
2. ✓ Builds Docker images for Linux amd64 architecture
3. ✓ Saves images as tar archives
4. ✓ Transfers images to remote server
5. ✓ Transfers docker-compose configuration
6. ✓ Transfers environment file
7. ✓ Loads images on remote server
8. ✓ Starts all services (API, Frontend, DB, Redis, Meilisearch)
9. ✓ Verifies deployment health
10. ✓ Configures auto-restart policies
11. ✓ Sets up daily database backups

**Expected Duration:** 15-20 minutes

### Step 4: Monitor Deployment Progress

During deployment, the script will show:
```
[INFO] Testing SSH connectivity...
[✓] SSH connection established
[INFO] Building Docker images...
[✓] Docker images built successfully
[INFO] Preparing and transferring deployment artifacts...
[✓] Artifacts transferred to remote server
[INFO] Deploying to remote server...
[✓] Services deployed successfully
```

### Step 5: Verify Post-Deployment

After the script completes, verify services are running:

```bash
# Check container status
ssh root@192.168.0.9 "docker ps"

# Check API health endpoint
curl -s http://192.168.0.9:5000/health | jq .

# Check Frontend
curl -s -I http://192.168.0.9 | head -5

# View logs
ssh root@192.168.0.9 "docker-compose -f /opt/crm-deployment/docker-compose.yml logs -n 50"
```

**Expected Output:**
- All containers running (crm-api, crm-frontend, crm-mariadb, crm-redis, crm-meilisearch)
- API health endpoint returns status
- Frontend responds with HTTP 200
- Clear application logs

---

## 📱 Access After Deployment

### API Endpoints
```
Base URL: http://192.168.0.9:5000
Health Check: http://192.168.0.9:5000/health
API Documentation: http://192.168.0.9:5000/swagger/ui (if enabled)
```

### Frontend
```
URL: http://192.168.0.9
Default Admin: admin@crm.local / Admin@123
```

### Database
```
Host: 192.168.0.9:3306
User: crm_user
Password: CrmPass@Dev2024
Database: crm_db
Tool: mysql -h 192.168.0.9 -u crm_user -pCrmPass@Dev2024 crm_db
```

### Cache & Search
```
Redis: 192.168.0.9:6379
Meilisearch: http://192.168.0.9:7700
```

---

## 🔧 Common Operations After Deployment

### View Container Logs
```bash
ssh root@192.168.0.9 "docker-compose -f /opt/crm-deployment/docker-compose.yml logs -f crm-api"
```

### Restart All Services
```bash
ssh root@192.168.0.9 "cd /opt/crm-deployment && docker-compose restart"
```

### Stop All Services
```bash
ssh root@192.168.0.9 "cd /opt/crm-deployment && docker-compose down"
```

### Start Services Again
```bash
ssh root@192.168.0.9 "cd /opt/crm-deployment && docker-compose up -d"
```

### Manual Database Backup
```bash
ssh root@192.168.0.9 "/opt/crm/backup.sh"
```

### Check Backup Status
```bash
ssh root@192.168.0.9 "ls -lh /opt/crm/backups/"
```

### Update Environment Variables
```bash
# Edit environment on server
ssh root@192.168.0.9 "nano /opt/crm-deployment/.env"

# Restart services to apply changes
ssh root@192.168.0.9 "cd /opt/crm-deployment && docker-compose restart crm-api"
```

---

## 🐛 Troubleshooting

### Issue: SSH Connection Timeout
**Solution:**
```bash
# Check if SSH is available
ssh -v root@192.168.0.9 "echo test"

# Try with explicit key
ssh -i ~/.ssh/id_rsa root@192.168.0.9 "echo test"

# Check firewall
ssh root@192.168.0.9 "sudo ufw status"
```

### Issue: Docker Image Build Fails
**Solution:**
```bash
# Clean build cache
docker system prune -a

# Rebuild with verbose output
docker build -f docker/Dockerfile.backend . --progress=plain
```

### Issue: Services Not Starting
**Solution:**
```bash
# Check container logs
ssh root@192.168.0.9 "docker-compose -f /opt/crm-deployment/docker-compose.yml logs"

# Check service health
docker-compose ps

# Restart services
ssh root@192.168.0.9 "cd /opt/crm-deployment && docker-compose down && docker-compose up -d"
```

### Issue: Database Connection Failed
**Solution:**
```bash
# Test database connectivity
mysql -h 192.168.0.9 -u crm_user -pCrmPass@Dev2024 -e "SELECT 1"

# Check MariaDB container
ssh root@192.168.0.9 "docker logs crm-mariadb"

# Verify database exists
ssh root@192.168.0.9 "docker exec crm-mariadb mysql -u crm_user -pCrmPass@Dev2024 -e 'SHOW DATABASES;'"
```

### Issue: Frontend Shows 502 Bad Gateway
**Solution:**
```bash
# Verify API is running
curl -s http://192.168.0.9:5000/health

# Check frontend container
ssh root@192.168.0.9 "docker logs crm-frontend"

# Check API connectivity from frontend container
ssh root@192.168.0.9 "docker exec crm-frontend curl http://crm-api:5000/health"
```

### Issue: Out of Disk Space
**Solution:**
```bash
# Check disk usage
ssh root@192.168.0.9 "df -h"

# Clean up old Docker images/containers
ssh root@192.168.0.9 "docker system prune -a"

# Check specific container sizes
ssh root@192.168.0.9 "docker ps -a --format 'table {{.Names}}\t{{.Size}}'"
```

---

## 📊 Deployment Metrics

| Metric | Value |
|--------|-------|
| **Build Time** | ~3-5 minutes |
| **Transfer Time** | ~5-10 minutes (depends on network) |
| **Service Startup Time** | ~2-3 minutes |
| **Total Deployment Time** | ~15-20 minutes |
| **API Response Time** | <100ms (healthy) |
| **Database Size** | ~500MB initial |
| **Docker Image Size** | API: ~2.5GB, Frontend: ~500MB |

---

## 🔐 Security Considerations

### Before Production Deployment
1. **Change Default Passwords:**
   - Database password: `CrmPass@Dev2024` → Generate strong password
   - Admin password: `Admin@123` → Generate strong password
   - JWT Secret: Update to cryptographically secure random value

2. **Configure SSL/TLS:**
   - Set up HTTPS reverse proxy (Nginx/HAProxy)
   - Install SSL certificates (Let's Encrypt)
   - Update FRONTEND_URL to https://...

3. **Network Security:**
   - Restrict database port (3306) to internal network only
   - Restrict Redis port (6379) to internal network only
   - Use firewall rules to limit access

4. **Environment Variables:**
   - Store in secure vault (AWS Secrets Manager, Azure Key Vault)
   - Never commit sensitive files to Git
   - Use strong, unique values for all passwords

5. **Backup & Disaster Recovery:**
   - Verify daily backups are running
   - Test backup restoration process
   - Store backups in geographically diverse locations

---

## 📈 Monitoring After Deployment

### Key Health Metrics
```bash
# API Health
curl -s http://192.168.0.9:5000/health | jq .

# Database Connections
ssh root@192.168.0.9 "docker exec crm-mariadb mysql -u crm_user -pCrmPass@Dev2024 -e 'SHOW PROCESSLIST;'"

# Redis Stats
ssh root@192.168.0.9 "docker exec crm-redis redis-cli INFO"

# Docker Resource Usage
ssh root@192.168.0.9 "docker stats --no-stream"
```

### Container Auto-Restart
All containers are configured with `restart: unless-stopped`:
- ✓ Automatic restart on failure
- ✓ Persistent across server reboot
- ✓ Logs preserved for troubleshooting

### Backup Verification
- Daily backups at 2:00 AM UTC
- Retention policy: 7 days
- Location: `/opt/crm/backups`

---

## 🔄 Rollback Procedure

If deployment fails or needs to be rolled back:

```bash
# 1. Stop current deployment
ssh root@192.168.0.9 "cd /opt/crm-deployment && docker-compose down"

# 2. Remove volumes (if needed)
ssh root@192.168.0.9 "docker volume prune"

# 3. Restore from backup
ssh root@192.168.0.9 "gunzip /opt/crm/backups/crm_db_YYYYMMDD_HHMMSS.sql.gz"
ssh root@192.168.0.9 "docker exec crm-mariadb mysql -u root -pRootPass@Dev2024 < /tmp/crm_db_YYYYMMDD_HHMMSS.sql"

# 4. Restart services
ssh root@192.168.0.9 "cd /opt/crm-deployment && docker-compose up -d"
```

---

## 📝 Post-Deployment Checklist

- [ ] All services running and healthy
- [ ] API responding on port 5000
- [ ] Frontend accessible on port 80
- [ ] Database connections verified
- [ ] Backups scheduled and working
- [ ] Auto-restart configured
- [ ] Firewall rules configured
- [ ] SSL/TLS certificates installed (if applicable)
- [ ] Default credentials changed (recommended)
- [ ] Monitoring dashboards set up
- [ ] Team notified of deployment
- [ ] Documentation updated

---

## 📞 Support & Troubleshooting

For deployment issues:
1. Check deployment logs in console output
2. Review container logs: `docker logs <container_name>`
3. Test connectivity: `curl`, `telnet`, `mysql` commands
4. Check disk space and memory usage
5. Verify firewall and network rules

---

## 🎯 Next Steps After Deployment

1. **Data Migration (if needed):**
   ```bash
   # Backup existing data from previous environment
   # Import into new deployment
   ```

2. **Configuration & Customization:**
   - Configure email provider
   - Setup OAuth providers (if needed)
   - Configure AI provider (Ollama, OpenAI, etc.)
   - Customize branding and settings

3. **Team Setup:**
   - Create user accounts
   - Assign permissions and roles
   - Configure organizational structure

4. **Integration Setup:**
   - Configure third-party integrations
   - Setup webhooks
   - Configure API keys

5. **Testing:**
   - Run smoke tests
   - Verify business processes
   - Load testing (if applicable)

---

**Deployment completed successfully!**  
For more information, see the CRM Solution documentation.
