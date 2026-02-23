# CRM Demo Environment - Quick Start Guide

## 🚀 Access URLs

| Component | URL | Purpose |
|-----------|-----|---------|
| **Frontend UI** | http://192.168.0.9 | Main CRM application |
| **API Documentation** | http://192.168.0.9:5000/swagger/index.html | Interactive API docs |
| **Health Status** | http://192.168.0.9:5000/health | API health check |
| **Notifications** | http://192.168.0.9:4200 | Novu notification center |
| **Workflows** | http://192.168.0.9:5678 | n8n automation platform |
| **Analytics** | http://192.168.0.9:8088 | Superset BI dashboard |

## 👤 Default Credentials

```
Username: admin
Email: admin@crm.local
Password: Admin@123
```

## 🧪 Quick Demo Flow

### 1. Login
- Navigate to http://192.168.0.9
- Enter credentials above
- Click Login

### 2. Create a Test Incident
- Go to **ITSM** > **Incidents**
- Click **Create New Incident**
- Fill in:
  - Title: "System Down - Test"
  - Priority: High
  - Category: System
  - Assigned to: Select user
- Click Create

### 3. View Dashboard
- Navigate to **Dashboard**
- Show metrics:
  - Open Incidents
  - Response times
  - SLA compliance

### 4. Search Functionality
- Click search bar
- Search for incident/customer
- Show Meilisearch results

### 5. API Testing
Open a terminal:
```bash
# Get all incidents
curl http://192.168.0.9:5000/api/servicerequests \
  -H "Authorization: Bearer YOUR_TOKEN"

# Create a new incident
curl -X POST http://192.168.0.9:5000/api/servicerequests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "title": "API Test Incident",
    "description": "Created via API",
    "priority": 1
  }'
```

## 🔧 Key Features to Demo

### ITSM Module (✅ Available)
- Incident Management
- SLA Policies & Monitoring
- Business Hours Configuration
- Service Level Agreements
- Incident Search & Filtering

### CRM Module (✅ Available)
- Accounts (Companies)
- Contacts (Individuals)
- Opportunities (Sales Pipeline)
- Products & Catalog

### Integrations (✅ Available)
- Full-Text Search (Meilisearch)
- Notifications (Novu)
- Workflows (n8n)
- BI/Analytics (Superset)
- Signatures (DocuSeal)
- Chat (Chatwoot)

## 📊 System Status

Check service health:
```bash
# SSH into server
ssh root@192.168.0.9

# View all services
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# View API logs
docker logs -f crm-api

# View Frontend logs
docker logs -f crm-frontend
```

## 🐛 Troubleshooting

**Frontend blank?**
```bash
ssh root@192.168.0.9 'docker restart crm-frontend'
```

**API not responding?**
```bash
ssh root@192.168.0.9 'docker restart crm-api'
```

**Database connection error?**
```bash
# Check DB logs
ssh root@192.168.0.9 'docker logs crm-mariadb'

# Connect directly
mysql -h 192.168.0.9 -u crm_user -p crm_db
# Password: CrmPass@Dev2024
```

## 📝 API Endpoints

### Authentication
```
POST /api/auth/login          - User login
POST /api/auth/refresh        - Refresh token
GET  /api/auth/profile        - Current user info
```

### ITSM
```
GET  /api/servicerequests     - List incidents
POST /api/servicerequests     - Create incident
GET  /api/servicerequests/{id} - Get incident details
PUT  /api/servicerequests/{id} - Update incident
```

### CRM
```
GET  /api/accounts            - List accounts
POST /api/accounts            - Create account
GET  /api/contacts            - List contacts
GET  /api/opportunities       - List opportunities
```

### System
```
GET  /health                   - Health status
GET  /swagger                  - API documentation
```

## 🎯 Demo Talking Points

✅ **What's Working:**
- Full ITSM incident management
- CRM account/contact management
- Real-time incident search
- SLA tracking and metrics
- Multi-provider architecture
- Pluggable providers (Meilisearch, Novu, n8n)

⏳ **Coming Soon:**
- Change Management (Phase 2)
- Problem Management (Phase 3)
- Advanced Workflows (Phase 4)
- Advanced Analytics

## 🎬 Record a Demo

To record the demo session:
```bash
# On server, enable debug logs
ssh root@192.168.0.9 'export ASPNETCORE_ENVIRONMENT=Development'

# Start recording via OBS or similar tool
# Recommended resolution: 1920x1080
# Duration: 15-20 minutes
```

## 📞 Support

**Issues?** Check the logs:
```bash
# SSH to server
ssh root@192.168.0.9

# Real-time logs
docker logs -f crm-api
docker logs -f crm-frontend

# Historical logs (last 100 lines)
docker logs --tail 100 crm-api
```

---

**Demo Environment Status:** ✅ **READY**  
**Deployment Date:** February 17, 2026  
**Uptime:** Monitoring via Docker health checks
