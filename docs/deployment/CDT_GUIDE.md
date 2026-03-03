# Configuration Deployment Tool (CDT) Guide

> **CRM Solution Deployment Wizard - Developer & Operator Guide**  
> Version: 0.614.84  
> Last Updated: March 3, 2026  
> Technology: Python 3.12 + Flask 3.0

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Getting Started](#getting-started)
4. [13-Step Wizard](#13-step-wizard)
5. [Day-2 Operations](#day-2-operations)
6. [API Reference](#api-reference)
7. [Configuration Files](#configuration-files)
8. [Testing](#testing)
9. [Troubleshooting](#troubleshooting)

---

## Overview

The **Configuration Deployment Tool (CDT)** is a Python Flask-based deployment wizard that simplifies CRM solution deployment across multiple environments (Docker, Kubernetes, Azure, AWS, GCP).

### Key Features

- 🧙 **13-Step Interactive Wizard** - Guided deployment configuration
- 🎛️ **Day-2 Operations Dashboard** - Post-deployment management
- 🔧 **Multi-Platform Support** - Docker Compose, Kubernetes, Cloud providers
- 📊 **Health Monitoring** - Real-time service health checks
- 🗄️ **Database Management** - Backup, migration, seeding
- 📦 **Config Generation** - Auto-generates deployment manifests
- ✅ **496 Tests Passing** - Comprehensive test coverage

### Technology Stack

```python
Flask==3.0.3                    # Web framework
Jinja2==3.1.4                   # Template engine
PyYAML==6.0.1                   # YAML parsing
requests==2.32.3                # HTTP client
pytest==8.3.4                   # Testing framework
```

### Statistics

- **~35,000 lines of Python code**
- **8 Flask blueprints** with 96 API endpoints
- **6 test files** with 496 passing tests
- **13-step wizard** defined in `steps.yaml`
- **14 Jinja2 templates** for config generation

---

## Architecture

### Project Structure

```
CRM.Infrastructure/deployment-tool/
├── gui/                          # Flask application
│   ├── app.py                    # Main Flask app (391 lines)
│   ├── routes/                   # 8 Flask blueprints
│   │   ├── main. py              # Core routes (15 endpoints)
│   │   ├── docker.py             # Docker operations (12 endpoints)
│   │   ├── kubernetes.py         # K8s operations (14 endpoints)
│   │   ├── azure.py              # Azure deployment (11 endpoints)
│   │   ├── aws.py                # AWS deployment (10 endpoints)
│   │   ├── gcp.py                # GCP deployment (9 endpoints)
│   │   ├── database.py           # DB management (15 endpoints)
│   │   └── day2.py               # Day-2 ops (10 endpoints)
│   │
│   ├── templates/                # Jinja2 HTML templates
│   │   ├── wizard.html           # Main wizard UI (1,289 lines)
│   │   ├── day2.html             # Day-2 dashboard (1,109 lines)
│   │   ├── step{1-13}.html       # Step-specific templates
│   │   └── ... (14 total templates)
│   │
│   ├── static/                   # CSS, JS, images
│   │   ├── css/style.css
│   │   ├── js/wizard.js
│   │   └── images/
│   │
│   └── config/                   # Configuration
│       ├── steps.yaml            # Wizard step definitions
│       ├── providers.yaml        # Cloud provider configs
│       └── defaults.yaml         # Default values
│
├── deployers/                    # Deployment executors
│   ├── docker_deployer.py        # Docker Compose deployment
│   ├── kubernetes_deployer.py   # K8s deployment
│   ├── azure_deployer.py         # Azure deployment
│   ├── aws_deployer.py           # AWS deployment
│   └── gcp_deployer.py           # GCP deployment
│
├── generators/                   # Config file generators
│   ├── docker_compose_generator.py  # Generate docker-compose.yml
│   ├── k8s_manifest_generator.py    # Generate K8s YAML
│   ├── helm_chart_generator.py      # Generate Helm charts
│   └── env_file_generator.py        # Generate .env files
│
├── tests/                        # Test suite (496 tests)
│   ├── test_wizard_flow.py       # Wizard integration tests
│   ├── test_docker_deployer.py   # Docker deployment tests
│   ├── test_k8s_deployer.py      # K8s deployment tests
│   ├── test_generators.py        # Config generation tests
│   ├── test_day2_ops.py          # Day-2 operations tests
│   └── test_api_endpoints.py     # API endpoint tests
│
├── requirements.txt              # Python dependencies
├── pytest.ini                    # Pytest configuration
└── README.md                     # CDT documentation
```

### 8 Flask Blueprints (96 API Endpoints)

| Blueprint | Prefix | Endpoints | Purpose |
|-----------|--------|-----------|---------|
| **main** | `/` | 15 | Core wizard routes, step navigation |
| **docker** | `/api/docker` | 12 | Docker operations, health checks |
| **kubernetes** | `/api/k8s` | 14 | K8s deployment, scaling |
| **azure** | `/api/azure` | 11 | Azure resource management |
| **aws** | `/api/aws` | 10 | AWS deployment |
| **gcp** | `/api/gcp` | 9 | GCP deployment |
| **database** | `/api/database` | 15 | DB mgmt, backups, migrations |
| **day2** | `/api/day2` | 10 | Day-2 ops, monitoring |

---

## Getting Started

### Installation

1. **Navigate to CDT directory:**
   ```bash
   cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution/CRM.Infrastructure/deployment-tool
   ```

2. **Create virtual environment:**
   ```bash
   python3 -m venv .venv
   source .venv/bin/activate
   ```

3. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

4. **Run CDT:**
   ```bash
   python gui/app.py
   # Access at: http://localhost:5050
   ```

### Quick Start Scripts

```bash
# Linux/macOS
./run-deployment-tool.sh

# Windows
.\run-deployment-tool.ps1
```

### Docker Mode

```bash
# Build CDT Docker image
docker build -t crm-deployment-tool -f Dockerfile.tool .

# Run as container
docker run -p 5050:5050 -v $(pwd)/config:/app/config crm-deployment-tool
```

---

## 13-Step Wizard

The deployment wizard guides users through configuration in **13 sequential steps** defined in `config/steps.yaml`:

### Step Overview

| Step | Name | Purpose | Key Fields |
|------|------|---------|------------|
| **1** | Environment Selection | Choose target environment | dev/staging/prod |
| **2** | Platform Selection | Docker/K8s/Cloud | platform type |
| **3** | Database Configuration | DB provider & connection | MySQL/PostgreSQL/SQLServer |
| **4** | Redis Configuration | Cache settings | host, port, password |
| **5** | Storage Configuration | File storage | local/S3/Azure Blob |
| **6** | Email Configuration | SMTP settings | host, port, credentials |
| **7** | Provider Selection | External providers | Search, AI, Chat, etc. |
| **8** | Security Configuration | JWT, secrets | JWT secret, encryption keys |
| **9** | Feature Flags | Enable/disable modules | ITSM, Marketing, etc. |
| **10** | Scaling Configuration | Replicas, resources | CPU, memory, replicas |
| **11** | Monitoring Configuration | Observability | Prometheus, Grafana |
| **12** | Backup Configuration | Backup strategy | schedule, retention |
| **13** | Review & Deploy | Final review | confirmation |

### Step 1: Environment Selection

**Template:** `templates/step1.html`

```html
<div class="form-group">
  <label>Select Environment:</label>
  <select name="environment" class="form-control">
    <option value="development">Development</option>
    <option value="staging">Staging</option>
    <option value="production">Production</option>
  </select>
</div>

<div class="form-group">
  <label>Environment Name:</label>
  <input type="text" name="env_name" class="form-control" 
         placeholder="e.g., crm-dev-001" required>
</div>
```

### Step 2: Platform Selection

```html
<div class="platform-selection">
  <div class="option" data-platform="docker-compose">
    <h3>Docker Compose</h3>
    <p>Simple, single-host deployment</p>
    <button>Select</button>
  </div>
  
  <div class="option" data-platform="kubernetes">
    <h3>Kubernetes</h3>
    <p>Scalable, multi-node orchestration</p>
    <button>Select</button>
  </div>
  
  <div class="option" data-platform="azure-aks">
    <h3>Azure AKS</h3>
    <p>Managed Kubernetes on Azure</p>
    <button>Select</button>
  </div>
  
  <!-- AWS ECS, GCP GKE options -->
</div>
```

### Step 3-4: Database & Cache

**Database Configuration:**
```python
# API Endpoint: POST /api/database/configure
{
  "provider": "mysql",  # mysql, postgresql, sqlserver
  "host": "localhost",
  "port": 3306,
  "database": "crm_db",
  "username": "crm_user",
  "password": "***",
  "ssl_enabled": false,
  "connection_pool_size": 100
}
```

**Redis Configuration:**
```python
# API Endpoint: POST /api/database/configure-redis
{
  "host": "localhost",
  "port": 6379,
  "password": "***",
  "database": 0,
  "ssl": false
}
```

### Step 7: Provider Selection

**External Providers:**

```python
# AI Provider
{
  "ai_provider": "ollama",  # ollama, openai, azure, anthropic
  "ollama_url": "http://localhost:11434",
  "model": "llama3.1:8b"
}

# Search Provider
{
  "search_provider": "meilisearch",
  "meilisearch_url": "http://localhost:7700",
  "api_key": "masterKey"
}

# Chat Provider
{
  "chat_provider": "chatwoot",
  "chatwoot_url": "http://localhost:3000",
  "api_key": "***"
}
```

### Step 13: Review & Deploy

**Final Configuration Review:**

1. Summary of all selections
2. Generated configuration preview
3. Deployment command display
4. One-click deploy button

**Deploy Action:**

```python
# API Endpoint: POST /api/deploy
{
  "platform": "docker-compose",
  "config_id": "cfg-20240303-001",
  "dry_run": false
}

# Response:
{
  "deployment_id": "dep-20240303-001",
  "status": "in_progress",
  "logs_url": "/api/deploy/dep-20240303-001/logs"
}
```

---

## Day-2 Operations

**Access:** `http://localhost:5050/day2`

### Dashboard Features

**Day-2 Operations Dashboard** (`templates/day2.html` - 1,109 lines):

1. **Service Health Panel**
   - API health status
   - Frontend health
   - Database connectivity
   - Redis cache status
   - External provider health

2. **Database Operations**
   - Create backup
   - Restore from backup
   - Run migrations
   - Seed sample data
   - View database stats

3. **Container Management**
   - Start/stop services
   - View logs
   - Restart containers
   - Scale replicas

4. **Configuration Management**
   - View current config
   - Update settings
   - Toggle feature flags
   - Rotate secrets

5. **Monitoring & Logs**
   - Real-time service logs
   - Performance metrics
   - Error tracking
   - Audit logs

### Key Day-2 API Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/day2/health` | GET | Overall health status |
| `/api/day2/services/status` | GET | All service statuses |
| `/api/day2/services/{name}/restart` | POST | Restart service |
| `/api/day2/database/backup` | POST | Create backup |
| `/api/day2/database/migrate` | POST | Run migrations |
| `/api/day2/postinstall/seed-sample-data` | POST | Seed data |
| `/api/day2/logs/{service}` | GET | Service logs |
| `/api/day2/metrics` | GET | Performance metrics |
| `/api/day2/config` | GET/PUT | Config management |
| `/api/day2/audit-logs` | GET | Audit trail |

### Seed Sample Data UI

Enhanced UI with structured logging (added in v0.614.82):

```javascript
// Day-2 dashboard - Seed Sample Data button
function seedSampleData() {
  const btn = document.getElementById('seed-sample-data-btn');
  btn.disabled = true;
  btn.textContent = 'Seeding...';
  
  fetch('/api/day2/postinstall/seed-sample-data', { method: 'POST' })
    .then(res => res.json())
    .then(data => {
      _renderSeedLog(data);  // Rich log rendering
    });
}

function _renderSeedLog(result) {
  // Renders:
  // 1. Summary badges (Seeded: X, Skipped: Y, Failed: Z)
  // 2. Detailed step table with before/after counts
  // 3. Database statistics panel
  // 4. Duration metrics
}
```

---

## API Reference

### Core Routes (`/`)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/` | GET | Wizard home page |
| `/wizard` | GET | Start wizard |
| `/wizard/step/{n}` | GET | Load wizard step |
| `/wizard/save` | POST | Save step data |
| `/wizard/back` | POST | Go to previous step |
| `/wizard/reset` | POST | Reset wizard |
| `/config/export` | GET | Export configuration |
| `/config/import` | POST | Import configuration |

### Docker Operations (`/api/docker`)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/docker/health` | GET | Docker daemon health |
| `/api/docker/images` | GET | List images |
| `/api/docker/containers` | GET | List containers |
| `/api/docker/start` | POST | Start containers |
| `/api/docker/stop` | POST | Stop containers |
| `/api/docker/logs/{container}` | GET | Container logs |
| `/api/docker/build` | POST | Build image |
| `/api/docker/compose/up` | POST | Docker Compose up |
| `/api/docker/compose/down` | POST | Docker Compose down |

### Kubernetes Operations (`/api/k8s`)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/k8s/contexts` | GET | List contexts |
| `/api/k8s/namespaces` | GET | List namespaces |
| `/api/k8s/deployments` | GET | List deployments |
| `/api/k8s/pods` | GET | List pods |
| `/api/k8s/services` | GET | List services |
| `/api/k8s/apply` | POST | Apply manifest |
| `/api/k8s/delete` | DELETE | Delete resource |
| `/api/k8s/scale` | PUT | Scale deployment |
| `/api/k8s/logs/{pod}` | GET | Pod logs |

### Database Operations (`/api/database`)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/database/configure` | POST | Set DB config |
| `/api/database/test-connection` | POST | Test connection |
| `/api/database/backup` | POST | Create backup |
| `/api/database/restore` | POST | Restore backup |
| `/api/database/migrate` | POST | Run migrations |
| `/api/database/seed` | POST | Seed data |
| `/api/database/stats` | GET | Database statistics |
| `/api/database/tables` | GET | List tables |
| `/api/database/backups` | GET | List backups |

---

## Configuration Files

### Generated Files

The CDT generates deployment-ready configuration files:

**Docker Compose:**
```yaml
# Generated: docker-compose.generated.yml
version: '3.8'
services:
  crm-api:
    image: crm-api:latest
    ports:
      - "5000:5000"
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - Jwt__Secret=${JWT_SECRET}
    depends_on:
      - crm-mariadb
  
  crm-frontend:
    image: crm-frontend:latest
    ports:
      - "80:80"
    depends_on:
      - crm-api
  
  crm-mariadb:
    image: mariadb:10.11
    ports:
      - "3306:3306"
    environment:
      - MYSQL_ROOT_PASSWORD=${DB_ROOT_PASSWORD}
      - MYSQL_DATABASE=crm_db
```

**Kubernetes Manifests:**
```yaml
# Generated: k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crm-api
  namespace: crm-prod
spec:
  replicas: 3
  selector:
    matchLabels:
      app: crm-api
  template:
    metadata:
      labels:
        app: crm-api
    spec:
      containers:
      - name: crm-api
        image: crm-api:latest
        ports:
        - containerPort: 5000
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: crm-secrets
              key: db-connection
```

**.env File:**
```bash
# Generated: .env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=...
Jwt__Secret=...
Redis__ConnectionString=...
```

---

## Testing

### Test Suite (496 Tests)

```
tests/
├── test_wizard_flow.py          # Wizard UI tests (87 tests)
├── test_docker_deployer.py      # Docker deployment (92 tests)
├── test_k8s_deployer.py         # K8s deployment (85 tests)
├── test_generators.py           # Config generation (121 tests)
├── test_day2_ops.py             # Day-2 operations (67 tests)
└── test_api_endpoints.py        # API tests (44 tests)
```

### Running Tests

```bash
# All tests
pytest

# With coverage
pytest --cov=gui --cov-report=html

# Specific test file
pytest tests/test_wizard_flow.py

# Verbose mode
pytest -v

# Watch mode
pytest tests/ --watch
```

### Test Output

```bash
======================== 496 passed in 12.34s ========================
Coverage: 87%
```

---

## Troubleshooting

### CDT Won't Start

**Issue:** "Port 5050 already in use"

**Solution:**
```bash
# Check what's using port 5050
lsof -i :5050

# Kill process or use different port
PORT=5051 python gui/app.py
```

### Docker Connection Failed

**Issue:** "Cannot connect to Docker daemon"

**Solution:**
```bash
# Start Docker Desktop or dockerd
sudo systemctl start docker

# Check Docker socket permissions
sudo chmod 666 /var/run/docker.sock
```

### Database Backup Fails

**Issue:** "Permission denied creating backup"

**Solution:**
```bash
# Ensure backup directory exists and is writable
mkdir -p /backups
chmod 777 /backups
```

### Kubernetes Context Not Found

**Issue:** "No kubeconfig found"

**Solution:**
```bash
# Set KUBECONFIG environment variable
export KUBECONFIG=~/.kube/config

# Or copy kubeconfig to default location
cp /path/to/kubeconfig ~/.kube/config
```

---

## Additional Resources

- **Docker Guide:** `docs/deployment/DOCKER_GUIDE.md`
- **Kubernetes Guide:** `docs/deployment/KUBERNETES_DEPLOYMENT_GUIDE.md`
- **Backend Guide:** `docs/backend/DEVELOPER_GUIDE.md`
- **Testing Guide:** `docs/testing/TESTING_GUIDE.md`

---

**Document Version:** 1.0  
**Last Updated:** March 3, 2026  
**Maintained By:** CRM Infrastructure Team
