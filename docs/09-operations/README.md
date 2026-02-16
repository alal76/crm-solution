# Operations & Maintenance

> **Last Updated:** February 1, 2026 | **Version:** 1.7.28

Operational runbooks, monitoring, troubleshooting guides, and maintenance procedures.

---

## Table of Contents

1. [System Health](#1-system-health)
2. [Monitoring](#2-monitoring)
3. [Logging](#3-logging)
4. [Troubleshooting](#4-troubleshooting)
5. [Maintenance Tasks](#5-maintenance-tasks)
6. [Runbooks](#6-runbooks)

---

## 1. System Health

### 1.1 Health Endpoints

| Endpoint | Purpose | Expected Response |
|----------|---------|-------------------|
| `/health` | Basic health check | `Healthy` |
| `/health/ready` | Readiness probe | `Ready` |
| `/health/live` | Liveness probe | `Alive` |
| `/api/status` | Detailed status | JSON with component status |

### 1.2 Health Check Response

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0234567"
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0012345"
    }
  }
}
```

### 1.3 Component Status Check

```bash
# Check all components
curl http://localhost:5000/api/status | jq

# Check database connectivity
curl http://localhost:5000/health/database

# Check from Kubernetes
kubectl exec -it deploy/crm-backend -n crm -- curl localhost:80/health
```

---

## 2. Monitoring

### 2.1 Key Metrics

#### Application Metrics

| Metric | Description | Alert Threshold |
|--------|-------------|-----------------|
| Response Time (P95) | 95th percentile response time | > 500ms |
| Request Rate | Requests per second | Context dependent |
| Error Rate | Percentage of 5xx responses | > 1% |
| Active Connections | Current open connections | > 1000 |

#### Database Metrics

| Metric | Description | Alert Threshold |
|--------|-------------|-----------------|
| Query Time (P95) | Slow query threshold | > 100ms |
| Connection Pool | Active connections | > 80% capacity |
| Disk Usage | Database storage | > 80% |
| Replication Lag | Secondary lag | > 10 seconds |

#### Infrastructure Metrics

| Metric | Description | Alert Threshold |
|--------|-------------|-----------------|
| CPU Usage | Container CPU | > 80% |
| Memory Usage | Container memory | > 85% |
| Pod Restarts | Unexpected restarts | > 0 in 1 hour |
| Disk I/O | Read/write operations | Context dependent |

### 2.2 Prometheus Metrics (if configured)

```yaml
# Example Prometheus scrape config
scrape_configs:
  - job_name: 'crm-backend'
    static_configs:
      - targets: ['crm-backend:80']
    metrics_path: /metrics
```

### 2.3 Dashboard Queries

```promql
# Request rate
rate(http_requests_total{app="crm-backend"}[5m])

# Error rate
rate(http_requests_total{app="crm-backend",status=~"5.."}[5m]) 
  / rate(http_requests_total{app="crm-backend"}[5m])

# Response time P95
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))
```

---

## 3. Logging

### 3.1 Log Locations

| Component | Location | Format |
|-----------|----------|--------|
| Backend API | stdout / `/var/log/crm/api.log` | JSON |
| Frontend | Browser console | Text |
| Database | `/var/log/mysql/error.log` | Text |
| Nginx | `/var/log/nginx/access.log` | Combined |

### 3.2 Log Levels

| Level | When to Use |
|-------|-------------|
| `Trace` | Detailed debugging (development only) |
| `Debug` | Diagnostic information |
| `Information` | General operational events |
| `Warning` | Unexpected but handled events |
| `Error` | Errors and exceptions |
| `Critical` | System failures |

### 3.3 Log Format (Backend)

```json
{
  "timestamp": "2026-02-01T12:00:00.000Z",
  "level": "Information",
  "message": "Request completed",
  "properties": {
    "method": "GET",
    "path": "/api/accounts",
    "statusCode": 200,
    "elapsed": 45,
    "userId": 1,
    "requestId": "abc-123-def"
  }
}
```

### 3.4 Viewing Logs

```bash
# Docker logs
docker logs crm-backend -f --tail 100

# Kubernetes logs
kubectl logs -f deploy/crm-backend -n crm

# Filter by level
kubectl logs deploy/crm-backend -n crm | grep '"level":"Error"'

# View recent errors
docker logs crm-backend 2>&1 | grep -i error | tail -20
```

---

## 4. Troubleshooting

### 4.1 Common Issues

#### API Not Responding

```bash
# Check if container is running
docker ps | grep crm-backend

# Check container health
docker inspect crm-backend --format='{{.State.Health.Status}}'

# Check logs for errors
docker logs crm-backend --tail 50

# Test connectivity
curl -v http://localhost:5000/health
```

#### Database Connection Failed

```bash
# Test database connectivity
docker exec -it crm-mariadb mysql -u root -p -e "SELECT 1"

# Check connection string
docker exec crm-backend printenv | grep ConnectionString

# Check database logs
docker logs crm-mariadb --tail 50

# Verify network
docker network inspect crm_default
```

#### Authentication Failures

```bash
# Check JWT configuration
docker exec crm-backend printenv | grep JWT

# Verify token expiration
# Decode JWT at jwt.io

# Check user exists
docker exec -it crm-mariadb mysql -u root -p crm \
  -e "SELECT Username, IsActive FROM Users WHERE Username='admin'"
```

#### Slow Performance

```bash
# Check resource usage
docker stats crm-backend crm-mariadb

# Check slow queries
docker exec -it crm-mariadb mysql -u root -p \
  -e "SHOW PROCESSLIST"

# Check connection pool
curl http://localhost:5000/api/status | jq '.database'

# Profile request
curl -w "@curl-format.txt" http://localhost:5000/api/accounts
```

### 4.2 Error Code Reference

| HTTP Code | Meaning | Common Causes |
|-----------|---------|---------------|
| 400 | Bad Request | Validation error, malformed JSON |
| 401 | Unauthorized | Missing/expired token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate key, concurrency conflict |
| 422 | Unprocessable | Business rule violation |
| 500 | Server Error | Unhandled exception |
| 503 | Unavailable | Service overloaded, database down |

### 4.3 Debug Mode

```bash
# Enable debug logging temporarily
docker exec crm-backend \
  sed -i 's/"Default": "Warning"/"Default": "Debug"/' appsettings.json

# Restart to apply
docker restart crm-backend

# Remember to revert in production!
```

---

## 5. Maintenance Tasks

### 5.1 Regular Maintenance Schedule

| Task | Frequency | Procedure |
|------|-----------|-----------|
| Database backup | Daily | [Runbook: Backup](#61-database-backup) |
| Log rotation | Weekly | Automated via logrotate |
| Security patches | Monthly | [Runbook: Updates](#63-applying-updates) |
| Performance review | Monthly | Review metrics dashboard |
| Certificate renewal | Before expiry | [Runbook: SSL](#64-ssl-certificate-renewal) |

### 5.2 Database Maintenance

```bash
# Analyze and optimize tables
docker exec -it crm-mariadb mysql -u root -p crm \
  -e "ANALYZE TABLE Accounts, Contacts, Opportunities;"

# Check table status
docker exec -it crm-mariadb mysql -u root -p crm \
  -e "SHOW TABLE STATUS;"

# Repair tables (if needed)
docker exec -it crm-mariadb mysql -u root -p crm \
  -e "REPAIR TABLE TableName;"
```

### 5.3 Cleanup Tasks

```bash
# Clean old sessions
docker exec -it crm-mariadb mysql -u root -p crm \
  -e "DELETE FROM Sessions WHERE ExpiresAt < NOW() - INTERVAL 30 DAY;"

# Archive old logs
find /var/log/crm -name "*.log" -mtime +30 -exec gzip {} \;

# Remove old Docker images
docker image prune -a --filter "until=168h"
```

---

## 6. Runbooks

### 6.1 Database Backup

**Frequency:** Daily  
**Estimated Time:** 5-15 minutes

```bash
#!/bin/bash
# backup-database.sh

BACKUP_DIR="/backups/crm"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/crm_backup_${TIMESTAMP}.sql.gz"

# Create backup
docker exec crm-mariadb mysqldump -u root -p${DB_PASSWORD} crm \
  | gzip > ${BACKUP_FILE}

# Verify backup
if [ -f ${BACKUP_FILE} ] && [ -s ${BACKUP_FILE} ]; then
    echo "Backup successful: ${BACKUP_FILE}"
    
    # Clean old backups (keep 30 days)
    find ${BACKUP_DIR} -name "*.sql.gz" -mtime +30 -delete
else
    echo "Backup FAILED!"
    exit 1
fi
```

### 6.2 Database Restore

**When:** Disaster recovery  
**Estimated Time:** 15-60 minutes

```bash
#!/bin/bash
# restore-database.sh

BACKUP_FILE=$1

if [ -z "$BACKUP_FILE" ]; then
    echo "Usage: restore-database.sh <backup_file>"
    exit 1
fi

echo "WARNING: This will replace all data in the CRM database!"
read -p "Are you sure? (yes/no): " confirm

if [ "$confirm" != "yes" ]; then
    echo "Restore cancelled."
    exit 0
fi

# Stop application
docker-compose stop backend

# Restore backup
zcat ${BACKUP_FILE} | docker exec -i crm-mariadb mysql -u root -p${DB_PASSWORD} crm

# Restart application
docker-compose start backend

echo "Restore complete. Verify data integrity."
```

### 6.3 Applying Updates

**Frequency:** As needed  
**Estimated Time:** 30-60 minutes

```bash
#!/bin/bash
# update-application.sh

echo "=== CRM Update Procedure ==="

# 1. Backup database
./backup-database.sh

# 2. Pull latest images
docker-compose pull

# 3. Stop services
docker-compose stop backend frontend

# 4. Apply database migrations
docker-compose run --rm backend dotnet ef database update

# 5. Start services
docker-compose up -d backend frontend

# 6. Verify health
sleep 10
curl -f http://localhost:5000/health || echo "Health check failed!"

# 7. Run smoke tests
./smoke-tests.sh

echo "Update complete. Monitor logs for errors."
```

### 6.4 SSL Certificate Renewal

**Frequency:** Before certificate expiry  
**Estimated Time:** 15 minutes

```bash
#!/bin/bash
# renew-ssl.sh

# Using Let's Encrypt/Certbot
certbot renew --quiet

# Copy new certificates
cp /etc/letsencrypt/live/crm.example.com/fullchain.pem /etc/ssl/certs/crm.crt
cp /etc/letsencrypt/live/crm.example.com/privkey.pem /etc/ssl/private/crm.key

# Reload nginx
docker exec crm-frontend nginx -s reload

# Verify
openssl s_client -connect crm.example.com:443 -servername crm.example.com \
  </dev/null 2>/dev/null | openssl x509 -noout -dates
```

### 6.5 Scaling Services

**When:** Load increases  
**Kubernetes:**

```bash
# Scale backend replicas
kubectl scale deployment crm-backend -n crm --replicas=4

# Verify pods
kubectl get pods -n crm -l app=crm-backend

# Check load distribution
kubectl top pods -n crm
```

**Docker Compose:**

```bash
# Scale backend
docker-compose up -d --scale backend=3

# Verify
docker-compose ps
```

### 6.6 Emergency Rollback

**When:** Failed deployment  
**Estimated Time:** 5-15 minutes

```bash
#!/bin/bash
# rollback.sh

PREVIOUS_VERSION=$1

if [ -z "$PREVIOUS_VERSION" ]; then
    echo "Usage: rollback.sh <version>"
    exit 1
fi

echo "Rolling back to version ${PREVIOUS_VERSION}..."

# Kubernetes
kubectl rollout undo deployment/crm-backend -n crm

# Or specify revision
kubectl rollout undo deployment/crm-backend -n crm --to-revision=${PREVIOUS_VERSION}

# Docker Compose
docker-compose stop backend
docker tag crm-backend:${PREVIOUS_VERSION} crm-backend:latest
docker-compose up -d backend

echo "Rollback complete. Verify application health."
```

---

## Emergency Contacts

| Role | Contact | Escalation |
|------|---------|------------|
| On-Call Engineer | [Internal] | First responder |
| Platform Team | [Internal] | Infrastructure issues |
| Database Admin | [Internal] | Database emergencies |
| Security Team | [Internal] | Security incidents |

---

## Related Documentation

- [DEPLOYMENT.md](../08-deployment/README.md)
- [TESTING.md](../07-testing/README.md)
- [INFRASTRUCTURE_GUIDE.md](../INFRASTRUCTURE_GUIDE.md)
