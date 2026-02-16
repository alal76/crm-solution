# CRM Solution - Documentation

**Version:** 0.0.25  
**Last Updated:** February 2026

Welcome to the CRM Solution documentation. This directory contains comprehensive documentation for developers, administrators, and users.

---

## 📁 Documentation Structure

```
docs/
├── README.md                         # This file - documentation overview
├── INDEX.md                          # Quick navigation index
├── 01-architecture/                  # System architecture docs
├── 02-design/                        # UI/UX design docs
├── 03-backend/                       # Backend docs
├── 04-api/                           # API reference
├── 05-frontend/                      # Frontend docs
├── 06-standards/                     # Coding standards
├── 07-testing/                       # Testing docs
├── 08-deployment/                    # Deployment guides
├── 09-operations/                    # Operations & runbooks
├── 10-traceability/                  # Feature traceability
├── 11-specifications/                # Feature specs and templates
├── 12- Enhancements planned/         # Future enhancements backlog
├── development/                      # Architecture, build, infra, DB
├── status/                           # Status, reports, checklists
├── summary/                          # Summaries and executive reports
├── test/                             # Test and QA reports
└── legacy/                           # Archived session and legacy docs
```

---

## 🚀 Quick Links

### Getting Started
| Document | Description |
|----------|-------------|
| [INDEX.md](INDEX.md) | Documentation hub and navigation |
| [DEVELOPMENT.md](DEVELOPMENT.md) | Developer setup and guidelines |
| [DATABASE_SETUP.md](DATABASE_SETUP.md) | Database configuration |
| [HOWTO.md](HOWTO.md) | Step-by-step tutorials |

### Architecture
| Document | Description |
|----------|-------------|
| [ARCHITECTURE_OVERVIEW.md](development/ARCHITECTURE_OVERVIEW.md) | System architecture overview |
| [MICROSERVICES_ARCHITECTURE.md](development/MICROSERVICES_ARCHITECTURE.md) | Microservices reference |
| [SOLUTION_CONTEXT.md](development/SOLUTION_CONTEXT.md) | Complete solution context |

### Deployment
| Document | Description |
|----------|-------------|
| [DEPLOYMENT_GUIDE_192.168.0.9.md](development/DEPLOYMENT_GUIDE_192.168.0.9.md) | Canonical deployment guide |
| [OPERATOR_DEPLOYMENT_GUIDE.md](OPERATOR_DEPLOYMENT_GUIDE.md) | Operator runbook |

### Features
| Document | Description |
|----------|-------------|
| [SOLUTION_CONTEXT.md](development/SOLUTION_CONTEXT.md) | Feature and module overview |
| [11-specifications/INDEX.md](11-specifications/INDEX.md) | Feature specifications |

### Testing
| Document | Description |
|----------|-------------|
| [TESTING_SUMMARY.md](test/TESTING_SUMMARY.md) | Test summary |
| [TEST_SUITE_QUICKSTART.md](test/TEST_SUITE_QUICKSTART.md) | Test quickstart |
| [TEST_SUITE_MASTER_INDEX.md](test/TEST_SUITE_MASTER_INDEX.md) | Test suite index |

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
| [ARCHITECTURE_OVERVIEW.md](development/ARCHITECTURE_OVERVIEW.md) | System architecture |
| [MICROSERVICES_ARCHITECTURE.md](development/MICROSERVICES_ARCHITECTURE.md) | Microservices details |
| [TESTING_SUMMARY.md](docs/test/TESTING_SUMMARY.md) | Test documentation |
| [CONTACT_INFO_MIGRATION_PLAN.md](development/CONTACT_INFO_MIGRATION_PLAN.md) | Data migration |

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
