# CRM Solution - Documentation Index

**Version:** 0.0.25  
**Last Updated:** January 2025

---

## 🚀 Quick Navigation

### Start Here
| Priority | Document | Description |
|----------|----------|-------------|
| ⭐ 1st | [guides/QUICK_START.md](guides/QUICK_START.md) | 5-minute setup guide |
| ⭐ 2nd | [DEVELOPMENT.md](DEVELOPMENT.md) | Developer guide |
| ⭐ 3rd | [DATABASE_SETUP.md](DATABASE_SETUP.md) | Database configuration |

---

## 📚 All Documentation

### Architecture (`architecture/`)

| File | Description |
|------|-------------|
| [CLOUD_DEPLOYMENT_ARCHITECTURE.md](architecture/CLOUD_DEPLOYMENT_ARCHITECTURE.md) | Cloud deployment patterns |
| [DATABASE_CONFIGURATION.md](architecture/DATABASE_CONFIGURATION.md) | Database design and configuration |
| [HEXAGONAL_ARCHITECTURE.md](architecture/HEXAGONAL_ARCHITECTURE.md) | Clean/Hexagonal architecture patterns |
| [KUBERNETES_ARCHITECTURE.md](architecture/KUBERNETES_ARCHITECTURE.md) | Kubernetes cluster design |
| [PORT_CONFIGURATION.md](architecture/PORT_CONFIGURATION.md) | Service port mappings |

### Deployment (`deployment/`)

| File | Description |
|------|-------------|
| [DEPLOYMENT_COMPLETE.md](deployment/DEPLOYMENT_COMPLETE.md) | Deployment completion checklist |
| [DEPLOYMENT_GUIDE.md](deployment/DEPLOYMENT_GUIDE.md) | General deployment instructions |
| [DOCKER_ARCHITECTURE.md](deployment/DOCKER_ARCHITECTURE.md) | Docker container architecture |
| [DOCKER_COMMANDS.md](deployment/DOCKER_COMMANDS.md) | Docker command reference |
| [DOCKER_IMAGE_UPDATE.md](deployment/DOCKER_IMAGE_UPDATE.md) | Updating Docker images |
| [DOCKER_SETUP.md](deployment/DOCKER_SETUP.md) | Docker environment setup |
| [KUBERNETES_DEPLOYMENT_GUIDE.md](deployment/KUBERNETES_DEPLOYMENT_GUIDE.md) | K8s deployment guide |
| [KUBERNETES_SETUP_COMPLETE.md](deployment/KUBERNETES_SETUP_COMPLETE.md) | K8s setup checklist |
| [LOCAL_DEVELOPMENT_PORTS.md](deployment/LOCAL_DEVELOPMENT_PORTS.md) | Local dev port configuration |
| [PRODUCTION_SERVER_SETUP.md](deployment/PRODUCTION_SERVER_SETUP.md) | Production server configuration |
| [REMOTE_DOCKER_DEPLOYMENT.md](deployment/REMOTE_DOCKER_DEPLOYMENT.md) | Remote Docker deployment |
| [SSH_AUTHENTICATION_SETUP.md](deployment/SSH_AUTHENTICATION_SETUP.md) | SSH key authentication |

### Features (`features/`)

| File | Description |
|------|-------------|
| [ADMIN_SETTINGS_GUIDE.md](features/ADMIN_SETTINGS_GUIDE.md) | Admin settings usage |
| [ADMIN_SETTINGS_REFACTORING.md](features/ADMIN_SETTINGS_REFACTORING.md) | Settings refactoring notes |
| [CONTACTS_IMPLEMENTATION.md](features/CONTACTS_IMPLEMENTATION.md) | Contact system implementation |
| [MARKETING_CAMPAIGNS.md](features/MARKETING_CAMPAIGNS.md) | Marketing campaign features |
| [MARKETING_CAMPAIGNS_REFACTORING.md](features/MARKETING_CAMPAIGNS_REFACTORING.md) | Campaign refactoring |
| [OAUTH_IMPLEMENTATION.md](features/OAUTH_IMPLEMENTATION.md) | OAuth2 integration |
| [OAUTH_REFACTORING.md](features/OAUTH_REFACTORING.md) | OAuth refactoring notes |
| [RESPONSIVE_DESIGN.md](features/RESPONSIVE_DESIGN.md) | Responsive UI implementation |
| [RESPONSIVE_DESIGN_PATTERNS.md](features/RESPONSIVE_DESIGN_PATTERNS.md) | Responsive design patterns |
| [SIGNALR_IMPLEMENTATION.md](features/SIGNALR_IMPLEMENTATION.md) | SignalR real-time features |
| [SIGNALR_REFACTORING.md](features/SIGNALR_REFACTORING.md) | SignalR refactoring notes |
| [USER_MANAGEMENT_README.md](features/USER_MANAGEMENT_README.md) | User management guide |
| [USER_MANAGEMENT_REFACTORING.md](features/USER_MANAGEMENT_REFACTORING.md) | User management refactoring |

### Guides (`guides/`)

| File | Description |
|------|-------------|
| [FRONTEND_UPDATES.md](guides/FRONTEND_UPDATES.md) | Frontend update procedures |
| [LOGIN_DEBUG_FIX.md](guides/LOGIN_DEBUG_FIX.md) | Login debugging |
| [LOGIN_DEBUG_SUMMARY.md](guides/LOGIN_DEBUG_SUMMARY.md) | Login issue summary |
| [QUICK_START.md](guides/QUICK_START.md) | Quick start guide |

### Testing (`testing/`)

| File | Description |
|------|-------------|
| [TEST_EXECUTION_GUIDE.md](testing/TEST_EXECUTION_GUIDE.md) | How to run tests |
| [TESTING_GUIDE.md](testing/TESTING_GUIDE.md) | Testing overview |
| [TESTING_SUMMARY.md](testing/TESTING_SUMMARY.md) | Test summary |

### Root-Level Docs

| File | Description |
|------|-------------|
| [BUILD_SYSTEM.md](BUILD_SYSTEM.md) | Build system documentation |
| [DATABASE_SETUP.md](DATABASE_SETUP.md) | Database setup instructions |
| [DEVELOPMENT.md](DEVELOPMENT.md) | Development guide |
| [FEATURE_CHECKLIST.md](FEATURE_CHECKLIST.md) | Feature status tracking |
| [FILE_LISTING.md](FILE_LISTING.md) | Project file listing |
| [HOWTO.md](HOWTO.md) | How-to tutorials |
| [IMPLEMENTATION_COMPLETE.md](IMPLEMENTATION_COMPLETE.md) | Implementation status |
| [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) | Implementation details |
| [INFRASTRUCTURE_GUIDE.md](INFRASTRUCTURE_GUIDE.md) | Infrastructure setup |
| [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) | Project overview |
| [SESSION_12_COMPLETION.md](SESSION_12_COMPLETION.md) | Session notes |
| [TECHNOLOGY_ACKNOWLEDGEMENTS.md](TECHNOLOGY_ACKNOWLEDGEMENTS.md) | Technology credits |
| [TEST_REPORT.md](TEST_REPORT.md) | Test results |
| [VERSIONING.md](VERSIONING.md) | Version management |
| [WORKFLOW_EXAMPLES.md](WORKFLOW_EXAMPLES.md) | Workflow engine examples |
| [ZIPCODE_IMPORT.md](ZIPCODE_IMPORT.md) | Zip code data import |
| [ADDRESS_UI_FEATURE.md](ADDRESS_UI_FEATURE.md) | Address UI features |

---

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         FRONTEND                                 │
│                React 18 + TypeScript + MUI                       │
│                     (Port 80/3000)                               │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                          BACKEND                                 │
│               ASP.NET Core 8.0 Web API                          │
│                    (Port 5000)                                   │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐               │
│  │ Controllers │ │  Services   │ │   SignalR   │               │
│  └─────────────┘ └─────────────┘ └─────────────┘               │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                        DATABASE                                  │
│                MariaDB (89 Tables)                              │
│                    (Port 3306)                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Common Tasks

### Start Development Environment
```bash
docker-compose -f docker/docker-compose.yml up -d
```

### Run Tests
```bash
# Backend
cd CRM.Backend/tests && dotnet test

# Frontend
cd CRM.Frontend && npm test

# E2E
cd e2e-tests && npx playwright test
```

### Build for Production
```bash
./build.sh
```

### Deploy to Production
```bash
./scripts/deploy-production.sh
```

---

## 📊 Current Statistics

| Metric | Value |
|--------|-------|
| **Version** | 0.0.25 |
| **Database Tables** | 89 |
| **Backend Tests** | 700+ |
| **API Endpoints** | 50+ |
| **Frontend Components** | 100+ |

---

## 🔗 External Links

- **Production Server:** `http://192.168.0.9`
- **API Documentation:** `http://192.168.0.9:5000/swagger`

---

## 📝 Document Updates

To update documentation:
1. Edit Markdown files directly
2. Run verification: `npm run docs:verify` (if available)
3. Commit with message: `docs: description of change`

---

*Last generated: January 2025*
