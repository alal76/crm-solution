# Architecture Overview

## Introduction

CRM Solution is an enterprise-grade Customer Relationship Management system designed with flexibility and scalability in mind. The architecture supports two deployment modes:

1. **Monolithic** - Single deployable unit (recommended for small-medium deployments)
2. **Microservices** - Distributed services (recommended for large-scale, high-availability deployments)

Both architectures share the same codebase and database schema, allowing organizations to start with the monolith and migrate to microservices as needed.

---

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              PRESENTATION LAYER                          │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │                     React Frontend (SPA)                            │ │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐  │ │
│  │  │Dashboard │ │Customers │ │  Sales   │ │Marketing │ │ Settings │  │ │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘  │ │
│  │  ┌──────────────────────────────────────────────────────────────┐  │ │
│  │  │           SignalR Client (Real-time Updates)                  │  │ │
│  │  └──────────────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
                                     │
                              HTTP / WebSocket
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                              APPLICATION LAYER                           │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │                      ASP.NET Core Web API                           │ │
│  │  ┌──────────────────────────────────────────────────────────────┐  │ │
│  │  │                    REST Controllers                           │  │ │
│  │  │  Auth │ Customers │ Contacts │ Opportunities │ Products │...  │  │ │
│  │  └──────────────────────────────────────────────────────────────┘  │ │
│  │  ┌──────────────────────────────────────────────────────────────┐  │ │
│  │  │                    SignalR Hub                                │  │ │
│  │  │           (CrmNotificationHub - Real-time Events)             │  │ │
│  │  └──────────────────────────────────────────────────────────────┘  │ │
│  │  ┌──────────────────────────────────────────────────────────────┐  │ │
│  │  │                    Middleware Pipeline                        │  │ │
│  │  │  Auth │ Error Handling │ Logging │ CORS │ Compression        │  │ │
│  │  └──────────────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                              BUSINESS LAYER                              │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │                         Service Layer                               │ │
│  │  ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐       │ │
│  │  │ Customer   │ │ Contact    │ │ Sales      │ │ Marketing  │       │ │
│  │  │ Service    │ │ Service    │ │ Service    │ │ Service    │       │ │
│  │  └────────────┘ └────────────┘ └────────────┘ └────────────┘       │ │
│  │  ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐       │ │
│  │  │ Workflow   │ │ Relation.  │ │ Campaign   │ │ Monitoring │       │ │
│  │  │ Service    │ │ Service    │ │ Execution  │ │ Service    │       │ │
│  │  └────────────┘ └────────────┘ └────────────┘ └────────────┘       │ │
│  └────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                              DATA LAYER                                  │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │                    Entity Framework Core                            │ │
│  │  ┌──────────────────────────────────────────────────────────────┐  │ │
│  │  │                    CrmDbContext                               │  │ │
│  │  │  89 DbSets │ Configurations │ Query Filters │ Interceptors   │  │ │
│  │  └──────────────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │                    Database (MariaDB)                               │ │
│  │  89 Tables │ Indexes │ Foreign Keys │ Stored Procedures            │ │
│  └────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Layer Responsibilities

### Presentation Layer (Frontend)

**Technology**: React 18 + TypeScript + Material-UI

| Component | Responsibility |
|-----------|----------------|
| **Pages** | Route-level components with business logic |
| **Components** | Reusable UI elements |
| **Services** | API communication via Axios |
| **Contexts** | Global state management (Auth, Theme, SignalR) |
| **Hooks** | Shared logic (pagination, concurrency) |

**Key Features**:
- Single Page Application (SPA)
- Responsive design (mobile, tablet, desktop)
- Real-time updates via SignalR
- Client-side routing with React Router
- Form handling with Formik + Yup validation

### Application Layer (API)

**Technology**: ASP.NET Core 8.0

| Component | Responsibility |
|-----------|----------------|
| **Controllers** | HTTP request handling, input validation |
| **Hubs** | SignalR real-time communication |
| **Middleware** | Cross-cutting concerns (auth, logging, errors) |
| **Filters** | Action filters for validation, caching |

**Key Features**:
- RESTful API design
- JWT authentication
- Swagger/OpenAPI documentation
- Request/response logging
- Global exception handling

### Business Layer (Services)

**Technology**: .NET Services with Dependency Injection

| Service | Responsibility |
|---------|----------------|
| **CustomerService** | Customer CRUD, lifecycle management |
| **ContactService** | Contact management, linking |
| **OpportunityService** | Sales pipeline, probability calculation |
| **CampaignService** | Campaign management, execution |
| **WorkflowService** | Workflow engine, automation |
| **RelationshipService** | Account relationships, mapping |
| **MonitoringService** | Health checks, metrics |

### Data Layer

**Technology**: Entity Framework Core 8.0

| Component | Responsibility |
|-----------|----------------|
| **CrmDbContext** | Database context with 89 DbSets |
| **Configurations** | Fluent API entity configurations |
| **Migrations** | Database schema versioning |
| **Query Filters** | Soft delete, multi-tenancy |

---

## Monolithic Architecture

The default deployment mode where all components run in a single process.

```
┌──────────────────────────────────────────┐
│              crm-frontend                 │
│           (Nginx + React SPA)             │
│              Port: 80                     │
└─────────────────┬────────────────────────┘
                  │
                  ▼
┌──────────────────────────────────────────┐
│               crm-api                     │
│         (ASP.NET Core 8.0)                │
│              Port: 5000                   │
│  ┌──────────────────────────────────────┐│
│  │  All Controllers & Services in one   ││
│  │  deployable unit                     ││
│  └──────────────────────────────────────┘│
└─────────────────┬────────────────────────┘
                  │
                  ▼
┌──────────────────────────────────────────┐
│             crm-mariadb                   │
│              (MariaDB)                    │
│              Port: 3306                   │
└──────────────────────────────────────────┘
```

**Advantages**:
- Simple deployment and operations
- Lower infrastructure costs
- Easier debugging and troubleshooting
- Suitable for most use cases

**Docker Compose File**: `docker/docker-compose.yml`

---

## Microservices Architecture

For large-scale deployments requiring independent scaling and high availability.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           crm-frontend (:80)                             │
└─────────────────────────────────┬───────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         crm-gateway (:5000)                              │
│                        (Ocelot API Gateway)                              │
│  Routes: /api/auth → Identity, /api/customers → Customer, etc.          │
└───────┬───────────┬───────────┬───────────┬───────────┬─────────────────┘
        │           │           │           │           │
        ▼           ▼           ▼           ▼           ▼
┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐
│ Identity  │ │ Customer  │ │   Sales   │ │ Marketing │ │ServiceDesk│
│  :5001    │ │  :5002    │ │   :5003   │ │   :5004   │ │   :5005   │
│           │ │           │ │           │ │           │ │           │
│ • Auth    │ │• Customers│ │• Opportun.│ │• Campaigns│ │• Requests │
│ • Users   │ │• Contacts │ │• Products │ │• Leads    │ │• Cases    │
│ • Groups  │ │• Accounts │ │• Quotes   │ │• Execution│ │• SLA      │
└───────────┘ └───────────┘ └───────────┘ └───────────┘ └───────────┘
        │           │           │           │           │
        └───────────┴───────────┴───────────┴───────────┘
                                  │
                                  ▼
                    ┌─────────────────────────┐
                    │       crm-core          │
                    │         :5006           │
                    │  • System Settings      │
                    │  • Workflows            │
                    │  • Monitoring           │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │      crm-mariadb        │
                    │         :3306           │
                    └─────────────────────────┘
```

### Service Responsibilities

| Service | Port | Domains |
|---------|------|---------|
| **Gateway** | 5000 | API routing, load balancing, auth forwarding |
| **Identity** | 5001 | Authentication, users, groups, profiles |
| **Customer** | 5002 | Customers, contacts, accounts, addresses |
| **Sales** | 5003 | Opportunities, products, quotes, line items |
| **Marketing** | 5004 | Campaigns, leads, execution, analytics |
| **ServiceDesk** | 5005 | Service requests, categories, SLA |
| **Core** | 5006 | Settings, workflows, monitoring, lookups |

**Docker Compose File**: `docker/docker-compose.microservices.unified.yml`

---

## Database Architecture

### Schema Organization

The 89 database tables are organized into logical domains:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           CRM Database Schema                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐          │
│  │    CUSTOMERS    │  │     SALES       │  │   MARKETING     │          │
│  ├─────────────────┤  ├─────────────────┤  ├─────────────────┤          │
│  │ Customers       │  │ Opportunities   │  │ MarketingCamp.  │          │
│  │ Contacts        │  │ Products        │  │ CampaignRecip.  │          │
│  │ CustomerContacts│  │ Quotes          │  │ CampaignABTests │          │
│  │ Accounts        │  │ QuoteLineItems  │  │ CampaignMetrics │          │
│  │ Addresses       │  │ Leads           │  │ CampaignConv.   │          │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘          │
│                                                                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐          │
│  │  SERVICE DESK   │  │   WORKFLOWS     │  │  RELATIONSHIPS  │          │
│  ├─────────────────┤  ├─────────────────┤  ├─────────────────┤          │
│  │ ServiceRequests │  │ WorkflowDef.    │  │ AccountRelation.│          │
│  │ Categories      │  │ WorkflowInst.   │  │ RelationshipMap │          │
│  │ Subcategories   │  │ WorkflowNodes   │  │ RelationshipType│          │
│  │ CustomFields    │  │ WorkflowTasks   │  │ RelationInteract│          │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘          │
│                                                                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐          │
│  │  CONTACT INFO   │  │    SYSTEM       │  │     OTHER       │          │
│  ├─────────────────┤  ├─────────────────┤  ├─────────────────┤          │
│  │ EmailAddresses  │  │ Users           │  │ Notes           │          │
│  │ PhoneNumbers    │  │ UserGroups      │  │ Tags            │          │
│  │ Addresses       │  │ SystemSettings  │  │ Dashboards      │          │
│  │ SocialMediaLinks│  │ ModuleConfigs   │  │ Activities      │          │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Key Entity Relationships

```
Customer (1) ───────< (N) CustomerContacts >─────── (1) Contact
    │                                                      │
    │ (1:N)                                               │ (1:N)
    ▼                                                     ▼
Accounts                                            EmailAddresses
    │                                               PhoneNumbers
    │ (1:N)                                         Addresses
    ▼                                               SocialMediaLinks
Opportunities ─────< (N:M) >───── Products
    │
    │ (1:N)
    ▼
Quotes ───────< (1:N) >───── QuoteLineItems
```

---

## Communication Patterns

### Synchronous (REST API)

Used for standard CRUD operations and queries.

```
Frontend ──HTTP──> API ──EF Core──> Database
         <─JSON──      <──Data────
```

### Real-time (SignalR)

Used for live notifications and concurrent editing detection.

```
Frontend <──SignalR WebSocket──> CrmNotificationHub
              │
              ├── EntityUpdated
              ├── EntityCreated
              ├── EntityDeleted
              └── UserEditing
```

### Event Types

| Event | Description |
|-------|-------------|
| `EntityUpdated` | Entity was modified |
| `EntityCreated` | New entity created |
| `EntityDeleted` | Entity was deleted |
| `UserEditing` | User started editing (concurrency) |
| `UserStoppedEditing` | User finished editing |

---

## Security Architecture

### Authentication Flow

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

### JWT Token Structure

```json
{
  "sub": "1",
  "email": "user@example.com",
  "role": "Admin",
  "groups": ["Sales", "Marketing"],
  "exp": 1706536800,
  "iss": "CRMSolution"
}
```

### Authorization Levels

| Level | Description |
|-------|-------------|
| **Anonymous** | Public endpoints (health, version) |
| **Authenticated** | Any logged-in user |
| **Role-based** | Specific role required |
| **Group-based** | Membership in specific group |

---

## Deployment Architecture

### Docker Deployment

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

### Kubernetes Deployment

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Kubernetes Cluster                                │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                         Ingress                                    │  │
│  │              (nginx-ingress-controller)                           │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                              │                                           │
│         ┌────────────────────┼────────────────────┐                     │
│         ▼                    ▼                    ▼                     │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────┐             │
│  │ frontend    │      │ api         │      │ mariadb     │             │
│  │ Deployment  │      │ Deployment  │      │ StatefulSet │             │
│  │ (3 replicas)│      │ (3 replicas)│      │ (1 replica) │             │
│  └─────────────┘      └─────────────┘      └─────────────┘             │
│         │                    │                    │                     │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────┐             │
│  │ frontend    │      │ api         │      │ mariadb     │             │
│  │ Service     │      │ Service     │      │ Service     │             │
│  │ (ClusterIP) │      │ (ClusterIP) │      │ (ClusterIP) │             │
│  └─────────────┘      └─────────────┘      └─────────────┘             │
│                                                    │                    │
│                              ┌─────────────────────┘                    │
│                              ▼                                          │
│                       ┌─────────────┐                                   │
│                       │ PersistentV │                                   │
│                       │   Claim     │                                   │
│                       └─────────────┘                                   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Performance Considerations

### Caching Strategy

| Layer | Type | Purpose |
|-------|------|---------|
| **Frontend** | Browser Cache | Static assets, API responses |
| **API** | In-Memory Cache | Lookups, settings, ZipCodes |
| **Database** | Query Cache | Frequent queries |

### Optimization Techniques

1. **Pagination** - All list endpoints support pagination
2. **Lazy Loading** - EF Core lazy loading for relationships
3. **Projection** - Select only needed columns
4. **Indexing** - Database indexes on frequently queried columns
5. **Compression** - Gzip response compression
6. **Connection Pooling** - Database connection reuse

---

## Scalability

### Horizontal Scaling (Microservices)

```
                    Load Balancer
                         │
         ┌───────────────┼───────────────┐
         ▼               ▼               ▼
   ┌──────────┐    ┌──────────┐    ┌──────────┐
   │ Gateway  │    │ Gateway  │    │ Gateway  │
   │ Instance │    │ Instance │    │ Instance │
   │    1     │    │    2     │    │    3     │
   └──────────┘    └──────────┘    └──────────┘
```

### Database Scaling Options

- **Read Replicas** - Route read queries to replicas
- **Connection Pooling** - PgBouncer / ProxySQL
- **Sharding** - For very large datasets

---

## Monitoring & Observability

### Health Checks

| Endpoint | Description |
|----------|-------------|
| `/health` | Basic health check |
| `/health/live` | Liveness probe |
| `/health/ready` | Readiness probe |
| `/api/monitoring/status` | Detailed system status |

### Logging

- **Serilog** for structured logging
- Console and file sinks
- Request/response logging middleware
- Correlation IDs for request tracing

---

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Backend Framework** | ASP.NET Core 8 | Performance, cross-platform, mature ecosystem |
| **Frontend Framework** | React 18 | Component model, large ecosystem, TypeScript support |
| **ORM** | Entity Framework Core | Productivity, LINQ support, migrations |
| **Database** | MariaDB (default) | Open source, MySQL compatible, performant |
| **Real-time** | SignalR | Integrated with ASP.NET, WebSocket + fallbacks |
| **API Gateway** | Ocelot | .NET native, easy configuration |
| **Container** | Docker | Industry standard, K8s compatible |

---

## Future Considerations

1. **Event Sourcing** - For audit trails and temporal queries
2. **CQRS** - Separate read/write models for complex domains
3. **Message Queue** - RabbitMQ/Redis for async processing
4. **GraphQL** - For flexible client queries
5. **Multi-tenancy** - Schema or database per tenant
