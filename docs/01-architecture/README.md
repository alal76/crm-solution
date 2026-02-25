# System Architecture

> **Last Updated:** February 1, 2026 | **Version:** 1.7.28

---

## Table of Contents

1. [Overview](#1-overview)
2. [Architecture Principles](#2-architecture-principles)
3. [System Layers](#3-system-layers)
4. [Data Flow](#4-data-flow)
5. [Security Architecture](#5-security-architecture)
6. [Integration Patterns](#6-integration-patterns)
7. [Deployment Architecture](#7-deployment-architecture)
8. [Code Organization](#8-code-organization)

---

## 1. Overview

### 1.1 System Summary

CRM Solution is an enterprise-grade Customer Relationship Management system supporting:

- **Two Deployment Modes:** Monolithic and Microservices
- **Multi-Database Support:** MariaDB (primary), SQL Server, PostgreSQL
- **Real-time Updates:** SignalR WebSocket communication
- **Multi-tenancy Ready:** Group-based data isolation

### 1.2 High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              PRESENTATION LAYER                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                     React Frontend (SPA)                                │ │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐      │ │
│  │  │Dashboard │ │Customers │ │  Sales   │ │Marketing │ │ Settings │      │ │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘      │ │
│  │  ┌──────────────────────────────────────────────────────────────────┐  │ │
│  │  │           SignalR Client (Real-time Updates)                      │  │ │
│  │  └──────────────────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                              HTTP / WebSocket
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              APPLICATION LAYER                               │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                      ASP.NET Core Web API                               │ │
│  │  ┌──────────────────────────────────────────────────────────────────┐  │ │
│  │  │                    REST Controllers                               │  │ │
│  │  │  Auth │ Customers │ Contacts │ Opportunities │ Products │ ...    │  │ │
│  │  └──────────────────────────────────────────────────────────────────┘  │ │
│  │  ┌──────────────────────────────────────────────────────────────────┐  │ │
│  │  │                    SignalR Hub                                    │  │ │
│  │  │           (CrmNotificationHub - Real-time Events)                 │  │ │
│  │  └──────────────────────────────────────────────────────────────────┘  │ │
│  │  ┌──────────────────────────────────────────────────────────────────┐  │ │
│  │  │                    Middleware Pipeline                            │  │ │
│  │  │  Auth │ Error Handling │ Logging │ CORS │ Compression            │  │ │
│  │  └──────────────────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              BUSINESS LAYER                                  │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                         Service Layer                                   │ │
│  │  ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐           │ │
│  │  │ Customer   │ │ Contact    │ │ Sales      │ │ Marketing  │           │ │
│  │  │ Service    │ │ Service    │ │ Service    │ │ Service    │           │ │
│  │  └────────────┘ └────────────┘ └────────────┘ └────────────┘           │ │
│  │  ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐           │ │
│  │  │ Workflow   │ │ Relation.  │ │ Campaign   │ │ Auth       │           │ │
│  │  │ Service    │ │ Service    │ │ Execution  │ │ Service    │           │ │
│  │  └────────────┘ └────────────┘ └────────────┘ └────────────┘           │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DATA LAYER                                      │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                    Entity Framework Core                                │ │
│  │  ┌──────────────────────────────────────────────────────────────────┐  │ │
│  │  │                    CrmDbContext                                   │  │ │
│  │  │  89 DbSets │ Configurations │ Query Filters │ Interceptors       │  │ │
│  │  └──────────────────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                    Database (MariaDB / SQL Server)                      │ │
│  │  89+ Tables │ Indexes │ Foreign Keys │ Views                           │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Architecture Principles

### 2.1 Core Principles

| Principle | Implementation |
|-----------|----------------|
| **Separation of Concerns** | Layered architecture with clear boundaries |
| **Dependency Injection** | All services registered via DI container |
| **Interface-Based Design** | Services implement interfaces for testability |
| **Repository Pattern** | Generic repository via EF Core DbContext |
| **CQRS-Light** | Separate DTOs for read/write operations |
| **Soft Deletes** | `IsDeleted` flag on all entities |
| **Audit Trail** | `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` on all entities |

### 2.2 Design Patterns Used

| Pattern | Location | Purpose |
|---------|----------|---------|
| **Repository** | `CrmDbContext` | Data access abstraction |
| **Service Layer** | `CRM.Infrastructure/Services/` | Business logic encapsulation |
| **Factory** | `ServiceCollectionExtensions` | Service registration |
| **Strategy** | `ICampaignChannelHandler` | Multi-channel campaign delivery |
| **Observer** | SignalR Hub | Real-time notifications |
| **Decorator** | Middleware pipeline | Cross-cutting concerns |
| **Builder** | Quote/Campaign builders | Complex object construction |

---

## 3. System Layers

### 3.1 Presentation Layer (Frontend)

**Technology:** React 18 + TypeScript + Material-UI 5

| Component Type | Location | Responsibility |
|----------------|----------|----------------|
| **Pages** | `src/pages/` | Route-level components with business logic |
| **Components** | `src/components/` | Reusable UI elements |
| **Services** | `src/services/` | API communication via Axios |
| **Contexts** | `src/contexts/` | Global state (Auth, Theme, SignalR) |
| **Hooks** | `src/hooks/` | Shared logic (pagination, debounce) |

**Key Files:**
- `src/App.tsx` - Main application routing
- `src/contexts/AuthContext.tsx` - Authentication state
- `src/contexts/SignalRContext.tsx` - Real-time updates
- `src/theme/theme.ts` - MUI theme configuration

### 3.2 Application Layer (API)

**Technology:** ASP.NET Core 10.0

| Component | Location | Responsibility |
|-----------|----------|----------------|
| **Controllers** | `CRM.Api/Controllers/` | HTTP endpoint handling |
| **Middleware** | `CRM.Api/Middleware/` | Cross-cutting concerns |
| **Hubs** | `CRM.Api/Hubs/` | SignalR real-time |
| **DTOs** | `CRM.Core/DTOs/` | Data transfer objects |

**Controller Pattern:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll([FromQuery] QueryParameters query)
    
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerDto dto)
    
    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] UpdateCustomerDto dto)
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
}
```

### 3.3 Business Layer (Services)

**Location:** `CRM.Infrastructure/Services/`

| Service | Interface | Responsibility |
|---------|-----------|----------------|
| `CustomerService` | `ICustomerService` | Customer CRUD, relationships |
| `ContactService` | `IContactService` | Contact management |
| `LeadService` | `ILeadService` | Lead pipeline management |
| `OpportunityService` | `IOpportunityService` | Sales opportunity tracking |
| `CampaignService` | `ICampaignService` | Campaign management |
| `CampaignExecutionService` | `ICampaignExecutionService` | Campaign delivery |
| `AuthenticationService` | `IAuthenticationService` | Login, JWT, password management |
| `WorkflowService` | `IWorkflowService` | Workflow automation |

### 3.4 Data Layer

**Technology:** Entity Framework Core 8

| Component | Location | Responsibility |
|-----------|----------|----------------|
| **DbContext** | `CRM.Infrastructure/Data/CrmDbContext.cs` | Database access |
| **Entities** | `CRM.Core/Entities/` | Domain models |
| **Configurations** | `CRM.Infrastructure/Data/Configurations/` | EF fluent config |
| **Migrations** | `CRM.Backend/migrations/` | Schema migrations |

---

## 4. Data Flow

### 4.1 Standard Request Flow

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│ Frontend │────>│ API      │────>│ Service  │────>│ DbContext│────>│ Database │
│ (React)  │     │ Controller│    │ Layer    │     │ (EF Core)│     │ (MariaDB)│
└──────────┘     └──────────┘     └──────────┘     └──────────┘     └──────────┘
     │                │                │                │                │
     │  HTTP Request  │   DTO          │   Entity       │   SQL          │
     │  (JSON)        │   Mapping      │   Operations   │   Queries      │
     │                │                │                │                │
     │<───────────────│<───────────────│<───────────────│<───────────────│
     │  HTTP Response │   DTO          │   Entity       │   Results      │
```

### 4.2 Real-time Update Flow (SignalR)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                              Server-Side Event                                │
│                                                                              │
│  Service Layer ────> NotificationService ────> SignalR Hub ────> All Clients │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
                                                        │
                                                        ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                              Client-Side Handling                             │
│                                                                              │
│  SignalRContext ────> Event Handler ────> State Update ────> UI Re-render   │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

**SignalR Event Types:**
| Event | Payload | Description |
|-------|---------|-------------|
| `EntityCreated` | `{entityType, entityId, data}` | New entity created |
| `EntityUpdated` | `{entityType, entityId, data}` | Entity modified |
| `EntityDeleted` | `{entityType, entityId}` | Entity deleted |
| `UserEditing` | `{entityType, entityId, userId}` | User started editing |
| `CampaignProgress` | `{campaignId, progress, status}` | Campaign execution progress |

---

## 5. Security Architecture

### 5.1 Authentication Flow

```
┌────────────┐     POST /api/auth/login      ┌────────────┐
│            │ ─────────────────────────────>│            │
│  Frontend  │     {email, password}         │    API     │
│            │ <─────────────────────────────│            │
└────────────┘     {accessToken, refresh}    └────────────┘
      │                                            │
      │  Subsequent requests:                      │
      │  Authorization: Bearer <token>             ▼
      │                                     ┌────────────┐
      └────────────────────────────────────>│   Verify   │
                                            │    JWT     │
                                            └────────────┘
```

### 5.2 Password Management

| Setting | Location | Description |
|---------|----------|-------------|
| `MinPasswordLength` | SystemSettings | Minimum length (default: 8) |
| `MaxPasswordLength` | SystemSettings | Maximum length (default: 128) |
| `RequireUppercase` | SystemSettings | At least one uppercase |
| `RequireLowercase` | SystemSettings | At least one lowercase |
| `RequireNumbers` | SystemSettings | At least one digit |
| `RequireSpecialChars` | SystemSettings | At least one special char |
| `DefaultPasswordExpirationDays` | SystemSettings | Password expiry (0=never) |

### 5.3 Group Security Policies

| Policy | Description |
|--------|-------------|
| `PasswordExpirationDays` | Days until password expires |
| `PasswordExpirationPolicy` | None(0), MustChange(1), Alert(2), Warn(3) |
| `RequireTwoFactor` | 2FA recommended for group |
| `EnforceTwoFactor` | 2FA mandatory for group |

### 5.4 JWT Token Structure

```json
{
  "sub": "1",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "Admin",
  "groups": ["Sales", "Marketing"],
  "permissions": ["CanEditCustomers", "CanAccessReports"],
  "exp": 1706536800,
  "iss": "CRMSolution"
}
```

### 5.5 Authorization Levels

| Level | Description | Example |
|-------|-------------|---------|
| **Anonymous** | No auth required | `/api/health`, `/api/version` |
| **Authenticated** | Valid JWT | Any logged-in user |
| **Role-based** | Specific role | `[Authorize(Roles = "Admin")]` |
| **Permission-based** | Specific permission | `CanDeleteCustomers` |
| **Group-based** | Group membership | Sales team only |

---

## 6. Integration Patterns

### 6.1 External Integrations

| Integration | Protocol | Purpose |
|-------------|----------|---------|
| **Email (SMTP)** | SMTP/TLS | Campaign emails, notifications |
| **OAuth Providers** | OAuth 2.0 | Google, Microsoft, Azure AD, LinkedIn, Facebook |
| **Redis** | TCP | Session caching, rate limiting |

### 6.2 Internal Communication

| Pattern | Technology | Use Case |
|---------|------------|----------|
| **REST API** | HTTP/JSON | Primary frontend-backend |
| **WebSocket** | SignalR | Real-time updates |
| **Message Queue** | In-memory (future: RabbitMQ) | Async campaign delivery |

---

## 7. Deployment Architecture

### 7.1 Docker Deployment

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           Docker Host                                    │
│  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐               │
│  │ crm-frontend  │  │   crm-api     │  │ crm-mariadb   │               │
│  │    :80        │  │    :5000      │  │    :3306      │               │
│  │   (Nginx)     │  │ (ASP.NET)     │  │  (MariaDB)    │               │
│  └───────────────┘  └───────────────┘  └───────────────┘               │
│              └──────────────────────────────┘                           │
│                        crm-network (bridge)                              │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Kubernetes Deployment

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Kubernetes Cluster                                │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                         Ingress                                    │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│         ┌────────────────────┼────────────────────┐                     │
│         ▼                    ▼                    ▼                     │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────┐             │
│  │ frontend    │      │ api         │      │ mariadb     │             │
│  │ Deployment  │      │ Deployment  │      │ StatefulSet │             │
│  │ (3 replicas)│      │ (3 replicas)│      │ (1 replica) │             │
│  └─────────────┘      └─────────────┘      └─────────────┘             │
│         │                    │                    │                     │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────┐             │
│  │ Service     │      │ Service     │      │ Service     │             │
│  │ (ClusterIP) │      │ (ClusterIP) │      │ (ClusterIP) │             │
│  └─────────────┘      └─────────────┘      └─────────────┘             │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 8. Code Organization

### 8.1 Backend Project Structure

```
CRM.Backend/
├── src/
│   ├── CRM.Api/                    # Web API Layer
│   │   ├── Controllers/            # 25+ REST Controllers
│   │   ├── Hubs/                   # SignalR Hubs
│   │   ├── Middleware/             # Custom Middleware
│   │   └── Program.cs              # Application Entry
│   │
│   ├── CRM.Core/                   # Domain Layer
│   │   ├── Entities/               # 89+ Entity Classes
│   │   ├── DTOs/                   # Data Transfer Objects
│   │   ├── Interfaces/             # Service Interfaces
│   │   └── Enums/                  # Enumerations
│   │
│   └── CRM.Infrastructure/         # Infrastructure Layer
│       ├── Data/                   # EF Core DbContext
│       │   ├── CrmDbContext.cs     # Main DbContext
│       │   └── Configurations/     # Entity Configurations
│       └── Services/               # Business Services
│
├── tests/                          # Test Projects
│   ├── CRM.Api.Tests/
│   ├── CRM.Core.Tests/
│   └── CRM.Infrastructure.Tests/
│
└── migrations/                     # SQL Migrations
```

### 8.2 Frontend Project Structure

```
CRM.Frontend/
└── src/
    ├── components/                 # Reusable Components
    │   ├── common/                 # Generic UI (Buttons, Cards)
    │   ├── forms/                  # Form Components
    │   ├── layout/                 # Layout (Sidebar, Header)
    │   └── modules/                # Module-specific
    │
    ├── pages/                      # Page Components
    │   ├── Dashboard/
    │   ├── Customers/
    │   ├── Contacts/
    │   ├── Leads/
    │   ├── Opportunities/
    │   ├── Campaigns/
    │   ├── Settings/
    │   └── ...
    │
    ├── services/                   # API Services
    │   ├── api.ts                  # Base Axios Instance
    │   ├── customerService.ts
    │   ├── contactService.ts
    │   └── ...
    │
    ├── contexts/                   # React Contexts
    │   ├── AuthContext.tsx
    │   ├── ThemeContext.tsx
    │   └── SignalRContext.tsx
    │
    ├── hooks/                      # Custom Hooks
    │   ├── usePagination.ts
    │   ├── useDebounce.ts
    │   └── ...
    │
    └── theme/                      # MUI Theme
        └── theme.ts
```

---

## Module Architecture Diagrams

Detailed service, data-flow, and component-map diagrams for individual modules:

| Module | Document |
|--------|----------|
| **ITSM / Service Desk** | [ITSM_ARCHITECTURE.md](ITSM_ARCHITECTURE.md) |

---

## Related Documentation

- [Backend Details](../03-backend/README.md)
- [API Reference](../04-api/README.md)
- [Frontend Details](../05-frontend/README.md)
- [Deployment Guide](../08-deployment/README.md)
