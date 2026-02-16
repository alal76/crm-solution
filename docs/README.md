# CRM Solution - Documentation

**Version:** 0.0.25  
**Last Updated:** January 2025

Welcome to the CRM Solution documentation. This directory contains comprehensive documentation for developers, administrators, and users.

---

## 📁 Documentation Structure

```
docs/
├── README.md                  # This file - documentation overview
├── INDEX.md                   # Quick navigation index
│
├── architecture/              # System architecture docs
│   ├── CLOUD_DEPLOYMENT_ARCHITECTURE.md
│   ├── DATABASE_CONFIGURATION.md
│   ├── HEXAGONAL_ARCHITECTURE.md
│   ├── KUBERNETES_ARCHITECTURE.md
│   └── PORT_CONFIGURATION.md
│
├── deployment/                # Deployment guides
│   ├── DEPLOYMENT_COMPLETE.md
│   ├── DEPLOYMENT_GUIDE.md
│   ├── DOCKER_*.md
│   ├── KUBERNETES_*.md
│   ├── PRODUCTION_SERVER_SETUP.md
│   ├── REMOTE_DOCKER_DEPLOYMENT.md
│   └── SSH_AUTHENTICATION_SETUP.md
│
├── features/                  # Feature documentation
│   ├── ADMIN_SETTINGS_*.md
│   ├── CONTACTS_IMPLEMENTATION.md
│   ├── MARKETING_CAMPAIGNS_*.md
│   ├── OAUTH_*.md
│   ├── RESPONSIVE_DESIGN*.md
│   ├── SIGNALR_*.md
│   └── USER_MANAGEMENT_*.md
│
├── guides/                    # User & developer guides
│   ├── FRONTEND_UPDATES.md
│   ├── LOGIN_DEBUG_*.md
│   └── QUICK_START.md
│
├── testing/                   # Testing documentation
│   ├── TEST_EXECUTION_GUIDE.md
│   ├── TESTING_GUIDE.md
│   └── TESTING_SUMMARY.md
│
├── BUILD_SYSTEM.md            # Build system documentation
├── DATABASE_SETUP.md          # Database configuration
├── DEVELOPMENT.md             # Developer guide
├── FEATURE_CHECKLIST.md       # Feature status
├── HOWTO.md                   # How-to tutorials
├── IMPLEMENTATION_SUMMARY.md  # Implementation details
├── PROJECT_SUMMARY.md         # Project overview
├── VERSIONING.md              # Version management
└── WORKFLOW_EXAMPLES.md       # Workflow engine examples
```

---

## 🚀 Quick Links

### Getting Started
| Document | Description |
|----------|-------------|
| [QUICK_START.md](guides/QUICK_START.md) | 5-minute quick start guide |
| [DEVELOPMENT.md](DEVELOPMENT.md) | Developer setup and guidelines |
| [DATABASE_SETUP.md](DATABASE_SETUP.md) | Database configuration |
| [HOWTO.md](HOWTO.md) | Step-by-step tutorials |

### Architecture
| Document | Description |
|----------|-------------|
| [HEXAGONAL_ARCHITECTURE.md](architecture/HEXAGONAL_ARCHITECTURE.md) | Clean architecture patterns |
| [KUBERNETES_ARCHITECTURE.md](architecture/KUBERNETES_ARCHITECTURE.md) | K8s deployment architecture |
| [PORT_CONFIGURATION.md](architecture/PORT_CONFIGURATION.md) | Service port mappings |
| [DATABASE_CONFIGURATION.md](architecture/DATABASE_CONFIGURATION.md) | Database design |

### Deployment
| Document | Description |
|----------|-------------|
| [DEPLOYMENT_GUIDE.md](deployment/DEPLOYMENT_GUIDE.md) | General deployment guide |
| [DOCKER_SETUP.md](deployment/DOCKER_SETUP.md) | Docker configuration |
| [KUBERNETES_DEPLOYMENT_GUIDE.md](deployment/KUBERNETES_DEPLOYMENT_GUIDE.md) | K8s deployment |
| [PRODUCTION_SERVER_SETUP.md](deployment/PRODUCTION_SERVER_SETUP.md) | Production setup |
| [REMOTE_DOCKER_DEPLOYMENT.md](deployment/REMOTE_DOCKER_DEPLOYMENT.md) | Remote deployment |

### Features
| Document | Description |
|----------|-------------|
| [USER_MANAGEMENT_README.md](features/USER_MANAGEMENT_README.md) | User management |
| [CONTACTS_IMPLEMENTATION.md](features/CONTACTS_IMPLEMENTATION.md) | Contact system |
| [MARKETING_CAMPAIGNS.md](features/MARKETING_CAMPAIGNS.md) | Campaign features |
| [SIGNALR_IMPLEMENTATION.md](features/SIGNALR_IMPLEMENTATION.md) | Real-time updates |
| [ADMIN_SETTINGS_GUIDE.md](features/ADMIN_SETTINGS_GUIDE.md) | Admin settings |

### Testing
| Document | Description |
|----------|-------------|
| [TESTING_GUIDE.md](testing/TESTING_GUIDE.md) | Testing overview |
| [TEST_EXECUTION_GUIDE.md](testing/TEST_EXECUTION_GUIDE.md) | Running tests |
| [TESTING_SUMMARY.md](testing/TESTING_SUMMARY.md) | Test summary |

---

## 📊 System Overview

### Technology Stack

| Layer | Technology |
|-------|------------|
| **Frontend** | React 18, TypeScript, Material-UI |
| **Backend** | .NET 10.0, ASP.NET Core, Entity Framework Core |
| **Database** | MariaDB (default), PostgreSQL, SQL Server |
| **Real-time** | SignalR WebSockets |
| **Containerization** | Docker, Docker Compose |
| **Orchestration** | Kubernetes |

### Database Schema

The CRM database contains **89 tables** organized into domains:

| Domain | Tables | Description |
|--------|--------|-------------|
| **Customer** | 15+ | Customers, contacts, accounts, addresses |
| **Sales** | 10+ | Opportunities, quotes, products, pipelines |
| **Marketing** | 8+ | Campaigns, leads, communications |
| **Service Desk** | 6+ | Service requests, categories |
| **Workflow** | 10+ | Workflow definitions, instances, tasks |
| **Contact Info** | 6+ | Emails, phones, addresses, social |
| **System** | 10+ | Users, groups, settings, lookups |

### Deployment Architectures

| Mode | Description |
|------|-------------|
| **Monolith** | Single API deployment, simpler operations |
| **Microservices** | 8 independent services, scalable |

---

## 🔧 Development

### Prerequisites
- .NET 10.0 SDK
- Node.js 20+
- Docker & Docker Compose
- MariaDB or compatible database

### Quick Start
```bash
# Clone repository
git clone <repository-url>
cd crm-solution

# Start with Docker Compose
docker-compose -f docker/docker-compose.yml up -d

# Access the application
open http://localhost:80
```

### Building
```bash
# Build backend
cd CRM.Backend
dotnet build CRM.sln

# Build frontend
cd CRM.Frontend
npm install
npm run build
```

### Testing
```bash
# Backend tests
cd CRM.Backend/tests
dotnet test

# Frontend tests
cd CRM.Frontend
npm test

# E2E tests
cd e2e-tests
npx playwright test
```

---

## 📖 Root-Level Documentation

These files are located in the repository root:

| File | Description |
|------|-------------|
| [README.md](../README.md) | Main project README |
| [ARCHITECTURE_OVERVIEW.md](docs/development/ARCHITECTURE_OVERVIEW.md) | System architecture |
| [MICROSERVICES_ARCHITECTURE.md](docs/development/MICROSERVICES_ARCHITECTURE.md) | Microservices details |
| [TESTING_SUMMARY.md](docs/test/TESTING_SUMMARY.md) | Test documentation |
| [CONTACT_INFO_MIGRATION_PLAN.md](docs/development/CONTACT_INFO_MIGRATION_PLAN.md) | Data migration |

---

## 🤝 Contributing

1. Read [DEVELOPMENT.md](DEVELOPMENT.md) for coding standards
2. Check [FEATURE_CHECKLIST.md](FEATURE_CHECKLIST.md) for feature status
3. Follow the branching strategy in [VERSIONING.md](VERSIONING.md)
4. Write tests as documented in [testing/TESTING_GUIDE.md](testing/TESTING_GUIDE.md)

---

## 📞 Support

For issues or questions:
1. Check the relevant documentation section
2. Review [HOWTO.md](HOWTO.md) for common tasks
3. Check troubleshooting sections in deployment docs
