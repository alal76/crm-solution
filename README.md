# CRM Solution

<div align="center">

![Version](https://img.shields.io/badge/version-0.0.25-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![React](https://img.shields.io/badge/React-18-61DAFB)
![License](https://img.shields.io/badge/license-MIT-green)
![Build](https://img.shields.io/badge/build-passing-brightgreen)

**Enterprise-Grade Customer Relationship Management System**

[Features](#-features) • [Quick Start](#-quick-start) • [Architecture](#-architecture) • [Documentation](#-documentation) • [API Reference](#-api-reference)

</div>

---

## 📋 Overview

CRM Solution is a comprehensive, full-stack Customer Relationship Management platform built with modern technologies. It supports both **monolithic** and **microservices** deployment architectures, offering flexibility for organizations of any size.

This is still very much a work in progress - totally unested and an experiment to see how far vibe coding can be used to build an enterprise grade solution . 

Comments feedback and enhacement requests along with your how it worked in your situation are welcome. 
I plan to use this the learning to do a writeup regarding this , and will incorporate the feedback into that writeup. 

This is a hobby side project not related to my day job - and built on weekends - no commitment on any time or effort will be put into this going forward - this is an experiment . Feel free to clone , extend enhance or change this as you feel free to.



### Key Highlights

- 🏢 **Multi-tenant Ready** - Support for multiple organizations and user groups
- 🔄 **Dual Architecture** - Deploy as monolith or microservices
- 📱 **Responsive Design** - Works seamlessly on desktop, tablet, and mobile
- 🔐 **Enterprise Security** - JWT authentication, role-based access control
- 🌐 **Multi-Database** - Supports MariaDB, MySQL, PostgreSQL, SQL Server
- 🚀 **Cloud Native** - Docker and Kubernetes ready
- 🤖 **AI Integration** - LLM provider support for intelligent features
- ⚡ **Real-time Updates** - SignalR for live notifications and concurrent editing

---

## 🎯 Features

### Core CRM Modules

| Module | Description |
|--------|-------------|
| **Customer Management** | Complete customer profiles, lifecycle tracking, organization/individual types |
| **Contact Management** | Multi-channel contact info (email, phone, social), relationship mapping |
| **Opportunity Management** | Sales pipeline, stage tracking, probability forecasting, win/loss analysis |
| **Account Management** | Business accounts, territory management, health scoring |
| **Lead Management** | Lead capture, scoring (fit/engagement), qualification workflow |
| **Quote Management** | Quote generation, line items, pricing, approval workflow |
| **Product Catalog** | Products, categories, pricing, SKU tracking |

### Marketing & Campaigns

| Feature | Description |
|---------|-------------|
| **Campaign Management** | Multi-channel campaigns (Email, Social, Event, Webinar) |
| **Campaign Execution** | A/B testing, recipient management, conversion tracking |
| **Marketing Analytics** | ROI, engagement metrics, conversion funnels |
| **Lead Scoring** | Automatic lead qualification based on engagement |

### Service & Support

| Feature | Description |
|---------|-------------|
| **Service Requests** | Ticketing system with SLA tracking |
| **Case Categories** | Hierarchical categorization and routing |
| **Priority Management** | Escalation levels, VIP customer handling |
| **Resolution Tracking** | Resolution codes, root cause analysis |

### Automation & Workflow

| Feature | Description |
|---------|-------------|
| **Workflow Engine** | Visual workflow designer with conditional logic |
| **Task Automation** | Automated task creation and assignment |
| **Approval Workflows** | Multi-level approval processes |
| **Notifications** | Real-time alerts via SignalR |

### Relationships & Mapping

| Feature | Description |
|---------|-------------|
| **Relationship Types** | Parent/child, partner, competitor relationships |
| **Account Relationships** | Complex B2B relationship mapping |
| **Territory Management** | Geographic and account-based territories |
| **Interaction Tracking** | Communication history across relationships |

### Analytics & Reporting

| Feature | Description |
|---------|-------------|
| **Dashboards** | Customizable widgets and KPIs |
| **Sales Analytics** | Pipeline analysis, forecasting, win/loss |
| **Marketing Reports** | Campaign performance, ROI analysis |
| **Activity Tracking** | User activity logs and audit trails |

### System Administration

| Feature | Description |
|---------|-------------|
| **User Management** | Users, groups, roles, permissions |
| **Field Configuration** | Custom fields per module |
| **System Settings** | Global configuration options |
| **LLM Integration** | AI provider configuration (OpenAI, Anthropic, etc.) |
| **Monitoring** | Health checks, performance metrics |
| **Theme Customization** | Light/dark modes, color palettes |

---

## 🛠 Tech Stack

### Backend (.NET 8.0)

| Component | Technology |
|-----------|------------|
| **Framework** | ASP.NET Core 8.0 |
| **ORM** | Entity Framework Core 8.0 |
| **Real-time** | SignalR |
| **Logging** | Serilog (structured logging) |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |
| **API Docs** | Swagger / OpenAPI 3.0 |
| **Caching** | In-Memory / Redis |

### Frontend (React 18)

| Component | Technology |
|-----------|------------|
| **Framework** | React 18 + TypeScript |
| **UI Library** | Material-UI (MUI) v5 |
| **Routing** | React Router v6 |
| **HTTP Client** | Axios |
| **Charts** | Recharts |
| **Forms** | Formik + Yup |
| **Real-time** | @microsoft/signalr |
| **State** | React Context + Hooks |

### Infrastructure

| Component | Technology |
|-----------|------------|
| **Containerization** | Docker 24+ |
| **Orchestration** | Docker Compose / Kubernetes |
| **Reverse Proxy** | Nginx |
| **Databases** | MariaDB, PostgreSQL, SQL Server |
| **Caching** | Redis (optional) |

---

## 🚀 Quick Start

### Prerequisites

- **Docker** 24.0+ and Docker Compose 2.0+
- **Node.js** 18+ (for local frontend development)
- **.NET SDK** 8.0+ (for local backend development)

### Option 1: Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/alal76/crm-solution.git
cd crm-solution

# Start all services
docker compose -f docker/docker-compose.yml up -d

# Wait for services to be healthy (about 30 seconds)
docker compose -f docker/docker-compose.yml ps

# Access the application
# Frontend: http://localhost
# API:      http://localhost:5000
# Swagger:  http://localhost:5000/swagger
```

### Option 2: Local Development

```bash
# Terminal 1: Start Database
docker compose -f docker/docker-compose.databases.yml up -d

# Terminal 2: Start Backend
cd CRM.Backend/src/CRM.Api
dotnet run

# Terminal 3: Start Frontend
cd CRM.Frontend
npm install
npm start
```

### Option 3: Microservices Mode

```bash
# Start microservices architecture
docker compose -f docker/docker-compose.microservices.unified.yml up -d

# Services will be available:
# Gateway:    http://localhost:5000
# Identity:   http://localhost:5001
# Customer:   http://localhost:5002
# Sales:      http://localhost:5003
# Marketing:  http://localhost:5004
# ServiceDesk: http://localhost:5005
# Core:       http://localhost:5006
```

### Default Login Credentials

| Email | Password | Role |
|-------|----------|------|
| abhi.lal@gmail.com | Admin@123 | Administrator |

---

## 🏗 Architecture

### Monolithic Architecture (Default)

```
┌─────────────────────────────────────────────────────────┐
│                     Frontend (React)                     │
│                   http://localhost:80                    │
└────────────────────────────┬────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                      CRM.Api                             │
│                http://localhost:5000                     │
│  ┌─────────────────────────────────────────────────────┐│
│  │  Controllers │ Services │ SignalR Hub │ Middleware  ││
│  └─────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────┐│
│  │              Entity Framework Core                   ││
│  └─────────────────────────────────────────────────────┘│
└────────────────────────────┬────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                   MariaDB Database                       │
│                   (89 Tables)                            │
└─────────────────────────────────────────────────────────┘
```

### Microservices Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Frontend (React)                     │
│                       Port: 80                           │
└────────────────────────────┬────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                    API Gateway                           │
│                (CRM.Gateway:5000)                        │
└──────┬──────┬──────┬──────┬──────┬──────┬───────────────┘
       │      │      │      │      │      │
       ▼      ▼      ▼      ▼      ▼      ▼
┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐
│Ident.│ │Custom│ │Sales │ │Market│ │Serv. │ │Core  │
│:5001 │ │:5002 │ │:5003 │ │:5004 │ │:5005 │ │:5006 │
└──────┘ └──────┘ └──────┘ └──────┘ └──────┘ └──────┘
       │      │      │      │      │      │
       └──────┴──────┴──────┴──────┴──────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                   Shared Database                        │
│                     MariaDB                              │
└─────────────────────────────────────────────────────────┘
```

### Database Schema (89 Tables)

| Domain | Count | Key Tables |
|--------|-------|------------|
| **Customer/Contact** | 12 | Customers, Contacts, CustomerContacts, Addresses |
| **Sales** | 10 | Opportunities, Quotes, QuoteLineItems, Leads, Products |
| **Marketing** | 12 | MarketingCampaigns, CampaignRecipients, CampaignABTests |
| **Service Desk** | 8 | ServiceRequests, Categories, Subcategories |
| **Relationships** | 6 | AccountRelationships, RelationshipMaps, RelationshipTypes |
| **Workflow** | 8 | WorkflowDefinitions, WorkflowInstances, WorkflowTasks |
| **Contact Info** | 8 | EmailAddresses, PhoneNumbers, SocialMediaLinks |
| **System** | 15 | Users, UserGroups, SystemSettings, ModuleConfigs |
| **Other** | 10 | Notes, Tags, Dashboards, LLMProviderSettings |

---

## 📁 Project Structure

```
crm-solution/
├── CRM.Backend/                    # .NET Backend
│   ├── src/
│   │   ├── CRM.Api/               # Main API (Monolith)
│   │   │   ├── Controllers/       # REST API Controllers
│   │   │   ├── Hubs/              # SignalR Hubs
│   │   │   ├── Middleware/        # Custom Middleware
│   │   │   └── Helpers/           # Utility Classes
│   │   ├── CRM.Core/              # Domain Layer
│   │   │   ├── Entities/          # Domain Entities
│   │   │   ├── Dtos/              # Data Transfer Objects
│   │   │   └── Interfaces/        # Service Interfaces
│   │   ├── CRM.Infrastructure/    # Data Access Layer
│   │   │   ├── Data/              # DbContext & Configurations
│   │   │   └── Services/          # Service Implementations
│   │   └── Services/              # Microservices
│   │       ├── CRM.Gateway/       # API Gateway (Ocelot)
│   │       ├── CRM.Identity/      # Auth Service
│   │       ├── CRM.CustomerService/
│   │       ├── CRM.SalesService/
│   │       ├── CRM.MarketingService/
│   │       ├── CRM.ServiceDeskService/
│   │       └── CRM.CoreService/
│   ├── tests/                     # Test Projects
│   └── migrations/                # SQL Migration Scripts
│
├── CRM.Frontend/                   # React Frontend
│   ├── src/
│   │   ├── components/            # Reusable Components
│   │   │   ├── common/            # Shared UI Components
│   │   │   ├── ContactInfo/       # Contact Info Components
│   │   │   └── settings/          # Settings Components
│   │   ├── pages/                 # Page Components
│   │   ├── services/              # API Service Clients
│   │   ├── contexts/              # React Context Providers
│   │   ├── hooks/                 # Custom Hooks
│   │   ├── config/                # Configuration
│   │   └── theme/                 # MUI Theme Config
│   └── public/                    # Static Assets
│
├── docker/                         # Docker Configuration
│   ├── docker-compose.yml         # Main (Monolith)
│   ├── docker-compose.databases.yml
│   ├── docker-compose.microservices.unified.yml
│   ├── docker-compose.unified.yml
│   └── Dockerfile.*               # Service Dockerfiles
│
├── kubernetes/                     # K8s Manifests
│   ├── 00-namespace-config.yaml
│   ├── 01-database-tier.yaml
│   ├── 02-application-tier.yaml
│   ├── 03-presentation-tier.yaml
│   ├── 04-ingress-network.yaml
│   ├── local/                     # Local K8s configs
│   ├── microservices/             # Microservices K8s
│   └── production/                # Production K8s
│
├── e2e-tests/                      # E2E Tests (Playwright)
│   ├── playwright.config.ts
│   └── tests/
│       ├── auth.setup.ts
│       ├── customers/
│       ├── contacts/
│       ├── data/
│       ├── bvt/
│       └── functional/
│
├── scripts/                        # Automation Scripts
│   ├── deploy.sh                  # Main deploy script
│   ├── deploy-192.168.0.9.sh      # Production deploy
│   ├── build-and-deploy.sh        # Build + deploy
│   └── seed-test-data.sh          # Data seeding
│
├── docs/                           # Documentation
│   ├── architecture/
│   ├── deployment/
│   ├── features/
│   ├── guides/
│   └── testing/
│
├── config/                         # Configuration Files
├── ARCHITECTURE_OVERVIEW.md
├── MICROSERVICES_ARCHITECTURE.md
├── TESTING_SUMMARY.md
├── CHANGELOG.md
└── version.json
```

---

## 📚 Documentation

### Architecture

| Document | Description |
|----------|-------------|
| [Architecture Overview](ARCHITECTURE_OVERVIEW.md) | High-level system design |
| [Microservices Architecture](MICROSERVICES_ARCHITECTURE.md) | Service decomposition |
| [Database Configuration](docs/architecture/DATABASE_CONFIGURATION.md) | Multi-database support |

### Deployment

| Document | Description |
|----------|-------------|
| [Docker Setup](docs/deployment/DOCKER_SETUP.md) | Docker Compose guide |
| [Kubernetes Guide](docs/deployment/KUBERNETES_DEPLOYMENT_GUIDE.md) | K8s deployment |
| [Production Deploy](docs/deployment/DEPLOY_192.168.0.9.md) | Server deployment |
| [Infrastructure Guide](docs/INFRASTRUCTURE_GUIDE.md) | Infrastructure overview |

### Features

| Document | Description |
|----------|-------------|
| [User Management](docs/features/USER_MANAGEMENT_README.md) | Users & permissions |
| [Multi-User Capability](docs/features/MULTI_USER_CAPABILITY.md) | Concurrent editing |
| [Workflow Engine](docs/WORKFLOW_EXAMPLES.md) | Automation examples |
| [Contact Info System](docs/features/CONSOLIDATED_CONTACT_INFO.md) | Contact management |
| [Navigation Config](docs/guides/NAVIGATION_CONFIGURATION.md) | Menu customization |

### Development

| Document | Description |
|----------|-------------|
| [Development Guide](docs/DEVELOPMENT.md) | Local setup |
| [How-To Guide](docs/HOWTO.md) | Common tasks |
| [Testing Summary](TESTING_SUMMARY.md) | Test strategies |

---

## 🔌 API Reference

### Base URLs

| Environment | URL |
|-------------|-----|
| Development | http://localhost:5000/api |
| Production | http://192.168.0.9:5000/api |
| Swagger UI | http://localhost:5000/swagger |

### Authentication

```bash
# Login
POST /api/auth/login
Content-Type: application/json
{
  "email": "abhi.lal@gmail.com",
  "password": "Admin@123"
}

# Response
{
  "accessToken": "eyJhbG...",
  "refreshToken": "...",
  "expiresIn": 3600
}
```

### Using the Token

```bash
curl -H "Authorization: Bearer <token>" \
     http://localhost:5000/api/customers
```

### Core Endpoints

| Resource | GET | POST | PUT | DELETE |
|----------|-----|------|-----|--------|
| `/api/customers` | ✅ | ✅ | ✅ | ✅ |
| `/api/contacts` | ✅ | ✅ | ✅ | ✅ |
| `/api/opportunities` | ✅ | ✅ | ✅ | ✅ |
| `/api/products` | ✅ | ✅ | ✅ | ✅ |
| `/api/leads` | ✅ | ✅ | ✅ | ✅ |
| `/api/quotes` | ✅ | ✅ | ✅ | ✅ |
| `/api/campaigns` | ✅ | ✅ | ✅ | ✅ |
| `/api/servicerequests` | ✅ | ✅ | ✅ | ✅ |

### Pagination

```
GET /api/customers?page=1&pageSize=25&sortBy=company&sortOrder=asc
```

### Response Format

```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 25,
  "totalPages": 4
}
```

---

## 🧪 Testing

### E2E Tests (Playwright)

```bash
cd e2e-tests
npm install
npx playwright install

# Run all tests
BASE_URL=http://localhost npx playwright test

# Run specific suite
BASE_URL=http://localhost npx playwright test tests/customers

# Run with UI
BASE_URL=http://localhost npx playwright test --ui

# Run against production
BASE_URL=http://192.168.0.9 npx playwright test
```

### Backend Tests

```bash
cd CRM.Backend/tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories

| Category | Location | Description |
|----------|----------|-------------|
| **BVT** | `e2e-tests/tests/bvt/` | Build Verification Tests |
| **Functional** | `e2e-tests/tests/functional/` | UI Functional Tests |
| **Data** | `e2e-tests/tests/data/` | Data Creation Tests |
| **Unit** | `CRM.Backend/tests/` | Backend Unit Tests |

---

## 🚢 Production Deployment

### Deploy Script

```bash
# Deploy to 192.168.0.9
./scripts/deploy-192.168.0.9.sh

# Build with version bump and deploy
./scripts/build-and-deploy.sh patch
```

### Environment Variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Database connection |
| `Jwt__Secret` | JWT signing key (min 32 chars) |
| `Jwt__Issuer` | JWT issuer name |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` |
| `AllowedOrigins` | CORS allowed origins |

### Docker Images

| Service | Image | Port |
|---------|-------|------|
| Frontend | crm-frontend | 80 |
| API | crm-api | 5000 |
| Gateway | crm-gateway | 5000 |
| Identity | crm-identity | 5001 |
| Customer | crm-customer | 5002 |
| Sales | crm-sales | 5003 |
| Marketing | crm-marketing | 5004 |
| ServiceDesk | crm-servicedesk | 5005 |
| Core | crm-core | 5006 |

---

## 📈 Current Statistics

| Metric | Value |
|--------|-------|
| **Version** | 0.0.25 |
| **Database Tables** | 89 |
| **API Controllers** | 25+ |
| **React Components** | 50+ |
| **Microservices** | 8 |
| **E2E Test Files** | 25+ |
| **Lines of Code** | 50,000+ |

---

## 🔄 Recent Updates (v0.0.25)

- ✅ Microservices architecture with 8 services
- ✅ SignalR real-time notifications
- ✅ Multi-user concurrent editing support
- ✅ Campaign execution with A/B testing
- ✅ Relationship management module
- ✅ Notes system with rich text
- ✅ Theme customization (light/dark)
- ✅ LLM provider integration
- ✅ Production deployment scripts

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

---

## 👥 Authors

- **Abhi Lal** - *Lead Developer* - [@alal76](https://github.com/alal76)

---

<div align="center">

**Built with ❤️ using .NET Core 8 and React 18**

[Report Bug](https://github.com/alal76/crm-solution/issues) • [Request Feature](https://github.com/alal76/crm-solution/issues)

</div>
