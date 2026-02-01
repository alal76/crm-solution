# CRM Solution - Documentation Hub

> **Version:** 1.7.28 | **Release:** Pre-Release | **Last Updated:** February 1, 2026

---

## 📖 Documentation Structure

This documentation provides complete coverage of the CRM Solution from architecture to implementation, with end-to-end traceability from business requirements to code.

### Quick Navigation

| Section | Description | Audience |
|---------|-------------|----------|
| [🏗️ Architecture](01-architecture/README.md) | System design, patterns, layers | Architects, Senior Devs |
| [🎨 Design](02-design/README.md) | UI/UX design, data models, workflows | Designers, Product |
| [⚙️ Backend](03-backend/README.md) | .NET Core API, services, entities | Backend Developers |
| [🔌 API Reference](04-api/README.md) | REST endpoints, schemas, examples | All Developers |
| [💻 Frontend](05-frontend/README.md) | React components, state, routing | Frontend Developers |
| [📏 Standards](06-standards/README.md) | Coding standards, conventions | All Developers |
| [🧪 Testing](07-testing/README.md) | Unit, integration, E2E tests | QA, Developers |
| [🚀 Deployment](08-deployment/README.md) | Docker, Kubernetes, CI/CD | DevOps, Ops |
| [📋 Operations](09-operations/README.md) | Monitoring, maintenance, runbooks | Ops, Support |
| [🔍 Traceability](10-traceability/README.md) | Feature-to-code mapping | All Teams |

---

## 🚀 Quick Start

### For New Developers

1. **Read First:** [Architecture Overview](01-architecture/README.md)
2. **Setup:** [Development Setup Guide](08-deployment/development-setup.md)
3. **Standards:** [Coding Standards](06-standards/README.md)
4. **Run Locally:** [Quick Start](08-deployment/quick-start.md)

### For AI Agents

📌 **Start Here:** [AGENT_CONTEXT.md](AGENT_CONTEXT.md)

This file provides:
- Solution context for AI assistants
- Key file locations and patterns
- Update instructions for documentation
- Change tracking requirements

---

## 📊 Solution Overview

### Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| **Frontend** | React + TypeScript + Material-UI | React 18, MUI 5 |
| **Backend** | ASP.NET Core Web API | .NET 8.0 |
| **Database** | MariaDB (primary), SQL Server, PostgreSQL | MariaDB 10.11+ |
| **ORM** | Entity Framework Core | EF Core 8 |
| **Real-time** | SignalR | ASP.NET SignalR |
| **Caching** | Redis | Redis 7+ |
| **Container** | Docker + Kubernetes | Docker 24+, K8s 1.28+ |
| **Testing** | xUnit, Playwright | Latest |

### Module Map

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           CRM SOLUTION MODULES                               │
├─────────────────┬─────────────────┬─────────────────┬───────────────────────┤
│   SALES         │   MARKETING     │   SERVICE       │   ADMINISTRATION      │
├─────────────────┼─────────────────┼─────────────────┼───────────────────────┤
│ • Customers     │ • Campaigns     │ • Service Req   │ • User Management     │
│ • Contacts      │ • Email         │ • Tickets       │ • Groups & Roles      │
│ • Leads         │ • Analytics     │ • Knowledge Base│ • Security Settings   │
│ • Opportunities │ • Templates     │ • SLA Tracking  │ • System Settings     │
│ • Quotes        │ • A/B Testing   │                 │ • Audit Logs          │
│ • Products      │                 │                 │ • Workflows           │
│ • Services      │                 │                 │                       │
└─────────────────┴─────────────────┴─────────────────┴───────────────────────┘
```

---

## 📁 Repository Structure

```
crm-solution/
├── 📁 CRM.Backend/                 # .NET Backend Solution
│   ├── 📁 src/
│   │   ├── 📁 CRM.Api/             # Web API (Controllers, Middleware)
│   │   ├── 📁 CRM.Core/            # Domain (Entities, DTOs, Interfaces)
│   │   ├── 📁 CRM.Infrastructure/  # Data Access (EF Core, Services)
│   │   └── 📁 CRM.DatabaseSeeder/  # Database Seeding Utility
│   ├── 📁 tests/                   # Unit & Integration Tests
│   └── 📁 migrations/              # SQL Server Migrations
│
├── 📁 CRM.Frontend/                # React Frontend
│   └── 📁 src/
│       ├── 📁 components/          # Reusable UI Components
│       ├── 📁 pages/               # Page Components
│       ├── 📁 services/            # API Service Layer
│       ├── 📁 contexts/            # React Contexts
│       ├── 📁 hooks/               # Custom Hooks
│       └── 📁 theme/               # MUI Theme Configuration
│
├── 📁 database/                    # Database Scripts (MariaDB)
│   ├── 📁 schema/                  # Table Definitions
│   ├── 📁 seed/                    # Seed Data
│   └── 📁 master_data/             # Reference Data
│
├── 📁 docker/                      # Docker Configuration
├── 📁 kubernetes/                  # Kubernetes Manifests
├── 📁 e2e-tests/                   # Playwright E2E Tests
├── 📁 scripts/                     # Build & Deployment Scripts
│
└── 📁 docs/                        # 📖 THIS DOCUMENTATION
    ├── 📁 01-architecture/         # System Architecture
    ├── 📁 02-design/               # Design Documentation
    ├── 📁 03-backend/              # Backend Documentation
    ├── 📁 04-api/                  # API Reference
    ├── 📁 05-frontend/             # Frontend Documentation
    ├── 📁 06-standards/            # Coding Standards
    ├── 📁 07-testing/              # Testing Documentation
    ├── 📁 08-deployment/           # Deployment Guides
    ├── 📁 09-operations/           # Operations & Runbooks
    ├── 📁 10-traceability/         # Feature Traceability
    └── 📄 AGENT_CONTEXT.md         # AI Agent Instructions
```

---

## 🔄 Documentation Maintenance

### For Developers

When making changes to the solution:

1. **Update traceability** in [10-traceability/](10-traceability/) when adding new features
2. **Update API docs** in [04-api/](04-api/) when changing endpoints
3. **Update standards** in [06-standards/](06-standards/) when introducing new patterns
4. **Log changes** in [CHANGELOG.md](../CHANGELOG.md)

### For AI Agents

See [AGENT_CONTEXT.md](AGENT_CONTEXT.md) for:
- How to read and understand this documentation
- How to update documentation when making changes
- Key files to reference for context
- Pattern matching for common operations

---

## 📚 Additional Resources

### Root-Level Documentation

| File | Description |
|------|-------------|
| [ARCHITECTURE_OVERVIEW.md](../ARCHITECTURE_OVERVIEW.md) | High-level architecture diagram |
| [SOLUTION_CONTEXT.md](../SOLUTION_CONTEXT.md) | Complete context reference |
| [CHANGELOG.md](../CHANGELOG.md) | Version history |
| [README.md](../README.md) | Project introduction |

### Legacy Documentation

Previous documentation has been consolidated into this structure. Original files are preserved in [legacy/](legacy/) for reference.

---

## 📞 Support

- **Repository Issues:** GitHub Issues
- **Documentation Issues:** Tag with `docs` label
- **Questions:** Check [09-operations/faq.md](09-operations/faq.md)
