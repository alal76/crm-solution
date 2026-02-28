# CRM Consolidated Deployment Tool (CDT)

> **Version:** 0.610.1  
> **Python:** 3.10+  
> **Flask:** 3.x + Flask-SocketIO 5.x

A browser-based configuration wizard and Day-2 operations dashboard for deploying and managing the CRM Solution across Docker, Kubernetes, AWS, Azure, and GCP.

---

## Quick Start

### Linux / macOS
```bash
cd CRM.Infrastructure/deployment-tool
./start-cdt.sh            # auto-detects Python, creates venv, downloads kubectl/helm
```

### Windows (PowerShell 5.1+)
```powershell
cd CRM.Infrastructure\deployment-tool
.\start-cdt.ps1
```

Open **http://localhost:5050** in your browser.

| Page | URL |
|------|-----|
| Deployment Wizard | http://localhost:5050 |
| Profile Manager | http://localhost:5050/profiles |
| Day-2 Operations | http://localhost:5050/day2 |
| API Explorer | http://localhost:5050/api/ |

---

## Features

| Feature | Description |
|---------|-------------|
| **13-step Wizard** | Guided configuration: target platform → architecture → database → network → security → providers → feature flags → seeding → review → deploy |
| **Saved Profiles** | YAML-based configuration profiles stored in `~/.crm-cdt/profiles/`. 7 built-in quick-start templates. |
| **Encrypted Vault** | AES-256-GCM secrets vault (`~/.crm-cdt/secrets/`). Master-password unlocked per session. |
| **Environment Probe** | Parallel checks: Docker socket, SSH, disk/RAM, ports, DNS, internet, cloud auth (AWS/Azure/GCP) |
| **Component Detector** | Auto-detects running CRM components (MariaDB, Redis, Meilisearch, Ollama, etc.) |
| **Config Generator** | Jinja2-rendered outputs: `docker-compose.yml`, `appsettings.json`, `nginx.conf`, `.env`, Helm values, K8s manifests |
| **Live Deploy Log** | SSE stream + SocketIO room per deployment. Animated step cards. Abort button. |
| **Day-2 Dashboard** | Upgrade, rollback (snapshot-based), scale replicas, rotate secrets |
| **Feature Flags UI** | Toggle CRM module and provider flags before deployment |

---

## Architecture

```
deployment-tool/
├── gui/
│   ├── app.py               # Flask 3 application + SocketIO init + blueprint registration
│   ├── templates/
│   │   ├── index.html        # Home / landing page
│   │   ├── wizard.html       # 13-step deployment wizard (Bootstrap 5)
│   │   └── day2.html         # Day-2 operations dashboard
│   └── routes/
│       ├── profile_routes.py # /api/profiles — CRUD + vault
│       ├── probe_routes.py   # /api/probe, /api/detect
│       ├── wizard_routes.py  # /api/wizard/session
│       ├── deploy_routes.py  # /api/deploy — start, stop, SSE stream
│       └── day2_routes.py    # /api/day2 — upgrade, rollback, scale, secrets
│
├── core/
│   ├── vault.py              # AES-256-GCM encrypted vault (PBKDF2 key derivation)
│   ├── profile.py            # ProfileManager + 7 templates + SQLite run history
│   ├── session.py            # WizardSession + SessionStore (TTL-based)
│   ├── step_manifest.py      # YAML-driven step manifest loader
│   ├── probe.py              # EnvironmentProbe (11 async checks)
│   ├── detector.py           # ComponentDetector (9 detectors, parallel)
│   ├── validator.py          # WizardValidator (11 primitives, 6 step dispatchers)
│   ├── generator.py          # ConfigGenerator (Jinja2)
│   └── socket_helpers.py     # SocketIO singleton helpers (emit_log, emit_step, etc.)
│
├── config-templates/
│   ├── docker-compose.j2     # Full compose with conditional provider blocks
│   ├── appsettings.j2        # .NET appsettings.json
│   ├── nginx.conf.j2         # Reverse proxy + SSL
│   ├── .env.j2               # Environment variables
│   ├── helm-values.j2        # Helm chart values
│   └── k8s-deployment.j2     # Kubernetes manifests
│
├── deployers/
│   ├── docker_compose.py     # 12-step Docker Compose deployer
│   └── kubernetes.py         # 13-step Kubernetes deployer
│
├── day2/
│   ├── upgrade.py            # UpgradeManager (snapshot + backup + image pull)
│   ├── rollback.py           # RollbackManager (restore from snapshot)
│   ├── scale.py              # ScaleManager (docker-compose / kubectl)
│   └── rotate_secrets.py     # SecretRotator (JWT, DB password, provider keys)
│
├── tests/
│   ├── test_vault.py         # 9 tests
│   ├── test_profile.py       # 7 tests
│   ├── test_probe.py         # 18 tests
│   └── test_generator.py     # 15 tests
│
├── steps.yaml                # 13-step wizard manifest
├── cdt_versions.json         # Pinned tool versions (kubectl, helm, terraform)
├── requirements.txt          # Python dependencies
├── start-cdt.sh              # Linux/macOS self-bootstrap launcher
├── start-cdt.ps1             # Windows PowerShell launcher
└── CDT_TODO.md               # Development tracking
```

---

## Configuration Profiles

Profiles are stored as YAML files in `~/.crm-cdt/profiles/`.

### Built-in Quick-Start Templates

| ID | Name | Target | Use Case |
|----|------|--------|---------|
| `local-dev` | Local Development | Docker Compose | Laptop / CI |
| `aws-ecs-monolith` | AWS ECS Monolith | AWS / ECS | Small AWS deployments |
| `aws-eks-microservices` | AWS EKS Microservices | AWS / EKS | Scalable AWS |
| `azure-aks-microservices` | Azure AKS Microservices | Azure / AKS | Azure-native |
| `gcp-gke-microservices` | GCP GKE Microservices | GCP / GKE | Google Cloud |
| `on-prem-k8s` | On-Premises Kubernetes | On-Prem / K8s | Private DC |
| `on-prem-docker` | On-Premises Docker Compose | On-Prem / Docker | Simple server |

### Profile API

```bash
GET  /api/profiles                         # List all profiles
POST /api/profiles                         # Create profile
GET  /api/profiles/{name}                  # Load profile
PUT  /api/profiles/{name}                  # Update profile
DELETE /api/profiles/{name}                # Delete profile
POST /api/profiles/from-template/{id}      # Create from template
GET  /api/profiles/templates               # List templates
GET  /api/profiles/{name}/history          # Run history
```

---

## Secrets Vault

The vault uses AES-256-GCM encryption with PBKDF2-HMAC-SHA256 key derivation.

```bash
POST /api/vault/unlock        {"password": "..."}
POST /api/vault/lock
POST /api/vault/set           {"key": "db_password", "value": "secret"}
GET  /api/vault/{key}
DELETE /api/vault/{key}
POST /api/vault/rotate
POST /api/vault/export        {"password": "bundle_password"}
POST /api/vault/import        {"bundle": "...", "password": "..."}
```

---

## Deployment API

### Start Deployment
```bash
POST /api/deploy/start
Content-Type: application/json

{
  "profile": { ... },         # Wizard configuration
  "target": "docker_compose"  # or "kubernetes"
}
```

Returns: `{"run_id": "run_1234567890", "message": "Deployment started"}`

### Live Log Stream (SSE)
```bash
GET /api/deploy/{run_id}/stream
Accept: text/event-stream
```

### SocketIO Live Logs
```javascript
const socket = io();
socket.emit('join_deploy', {run_id: 'run_1234567890'});
socket.on('log', (data) => console.log(data.level, data.message));
socket.on('deploy_step', (data) => updateStepCard(data.step, data.state));
socket.on('deploy_progress', (data) => updateProgressBar(data.pct));
socket.on('deploy_done', (data) => showDonePanel(data));
```

### Stop Deployment
```bash
POST /api/deploy/{run_id}/stop
```

---

## Day-2 Operations

### Upgrade
```bash
POST /api/day2/upgrade
{"target_version": "0.610.1", "backup": true, "dry_run": false}
```

### Rollback
```bash
GET  /api/day2/snapshots
POST /api/day2/rollback    {"snapshot_id": "snap_20260228_120000"}
DELETE /api/day2/snapshots/{snapshot_id}
```

### Scale
```bash
POST /api/day2/scale
{"service": "crm-api", "replicas": 3}
```

### Rotate Secrets
```bash
POST /api/day2/rotate-secret
{"secret_type": "jwt"}              # jwt | db_password | all
```

---

## Environment Probe

Before deployment, CDT runs 11 parallel checks:

| Check | Pass Condition |
|-------|---------------|
| Docker socket | `docker info` returns OK |
| SSH access | TCP reachable on port 22 |
| Disk space | >= 10 GB free |
| RAM | >= 4 GB available |
| Ports free | 80, 443, 5000, 3306, 6379 not in use |
| DNS resolution | Target hostname resolves |
| Internet connectivity | reachable |
| kubectl | binary present + cluster responds |
| AWS auth | `aws sts get-caller-identity` succeeds |
| Azure auth | `az account show` succeeds |
| GCP auth | `gcloud auth list` shows active account |

---

## Generated Config Files

After wizard completion, CDT generates:

| File | Template | Purpose |
|------|----------|---------|
| `docker-compose.yml` | `docker-compose.j2` | Full stack with conditional provider blocks |
| `appsettings.Production.json` | `appsettings.j2` | .NET API configuration |
| `nginx.conf` | `nginx.conf.j2` | Reverse proxy + SSL/HSTS |
| `.env` | `.env.j2` | Environment variables |
| `helm-values.yml` | `helm-values.j2` | Kubernetes Helm chart |
| `k8s-deployment.yml` | `k8s-deployment.j2` | Raw K8s manifests |

---

## Development

### Run tests
```bash
cd CRM.Infrastructure/deployment-tool
pip install -r requirements.txt
python -m pytest tests/ -v
```

### Run server manually
```bash
python gui/app.py --port 5050
```

### Add a wizard step
1. Add step definition to `steps.yaml`
2. Add HTML panel `<div class="step-panel" id="step-{id}">` to `wizard.html`
3. Add validation handler in `core/validator.py`
4. Update step indicator in `wizard.html` JS `STEPS` array

---

## CLI Tool Versions

Tool versions are pinned in `cdt_versions.json`:

| Tool | Version | Purpose |
|------|---------|---------|
| kubectl | v1.31.4 | Kubernetes CLI |
| helm | v3.17.0 | Kubernetes package manager |
| terraform | 1.10.3 | Infrastructure-as-code (optional) |

CDT downloads these automatically to `~/.crm-cdt/bin/` on first run.

---

## Security Notes

- Vault master password is **never stored on disk** — only the derived key hash
- All secrets are encrypted at rest (AES-256-GCM)
- The CDT server should only be accessible on `localhost` (default `0.0.0.0:5050` — restrict with `--host 127.0.0.1` in production)
- Admin credentials on the Done screen appear **once only** — save them before closing
- Rate limiting is not enabled in CDT (it is a local tool)

---

## Troubleshooting

| Problem | Solution |
|---------|---------|
| Port 5050 in use | `./start-cdt.sh --port 5051` |
| `flask_socketio` import error | `pip install flask-socketio>=5.3` |
| `cryptography` import error | `pip install cryptography>=42.0` |
| kubectl not found | CDT downloads it to `~/.crm-cdt/bin/` — check internet access |
| Vault locked error | `POST /api/vault/unlock {"password": "..."}` |
| Probe fails on Docker | Ensure Docker daemon is running: `sudo systemctl start docker` |
| `venv` Python version mismatch | `./start-cdt.sh --reset-venv` |

See also: [docs/common_development_issues.md](../../docs/common_development_issues.md)

---

*Part of the [CRM Solution](../../README.md) — Enterprise CRM Platform.*
