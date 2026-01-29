# CRM Solution - Microservices Architecture

**Version:** 0.0.25  
**Last Updated:** January 2025

## Overview

The CRM Solution supports both monolithic and microservices deployment architectures. The microservices architecture enables:

- **Faster builds**: Each service builds independently (~2-5 seconds vs 30+ seconds for monolith)
- **Modular deployments**: Update individual services without affecting others
- **Independent scaling**: Scale high-demand services (e.g., Customer, Sales) separately
- **Improved reliability**: Service isolation prevents cascading failures
- **Team autonomy**: Different teams can own different services

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Frontend                                    │
│                    (React SPA - Nginx on port 80)                        │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         API Gateway (Ocelot)                             │
│                            (port 5000)                                   │
│  Routes: /api/auth/* → Identity | /api/customers/* → Customer | etc.    │
└──────┬──────┬──────┬──────┬──────┬──────┬──────────────────────────────┘
       │      │      │      │      │      │
       ▼      ▼      ▼      ▼      ▼      ▼
┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
│ Identity │ │ Customer │ │  Sales   │ │Marketing │ │ServiceDsk│ │   Core   │
│  :5001   │ │  :5002   │ │  :5003   │ │  :5004   │ │  :5005   │ │  :5006   │
└────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘
     │            │            │            │            │            │
     └────────────┴────────────┴────────────┴────────────┴────────────┘
                                     │
                                     ▼
                        ┌────────────────────────┐
                        │    MariaDB Database    │
                        │        :3306           │
                        │  (89 Tables - crm_db)  │
                        └────────────────────────┘
```

---

## Services Overview

| Service | Port | Docker Image | Responsibilities |
|---------|------|--------------|------------------|
| **Gateway** | 5000 | `crm-gateway` | Ocelot reverse proxy, JWT pass-through, routing |
| **Identity** | 5001 | `crm-identity` | Authentication, users, user groups, roles, 2FA |
| **Customer** | 5002 | `crm-customer` | Customers, contacts, contact info, accounts |
| **Sales** | 5003 | `crm-sales` | Opportunities, quotes, products, pipelines, stages |
| **Marketing** | 5004 | `crm-marketing` | Campaigns, leads, communications, templates |
| **ServiceDesk** | 5005 | `crm-servicedesk` | Service requests, tasks, workflows |
| **Core** | 5006 | `crm-core` | Settings, backups, lookups, dashboard, monitoring |
| **Frontend** | 80 | `crm-frontend` | React SPA served by Nginx |

---

## API Routing

The Gateway routes requests based on URL paths:

### Identity Service (Port 5001)
| Path Pattern | Description |
|--------------|-------------|
| `/api/auth/*` | Authentication (login, logout, refresh) |
| `/api/users/*` | User management |
| `/api/usergroups/*` | User group management |
| `/api/userprofiles/*` | User profiles |

### Customer Service (Port 5002)
| Path Pattern | Description |
|--------------|-------------|
| `/api/customers/*` | Customer management |
| `/api/contacts/*` | Contact management |
| `/api/contactinfo/*` | Contact info (emails, phones, addresses) |
| `/api/accounts/*` | Account management |
| `/api/addresses/*` | Address management |

### Sales Service (Port 5003)
| Path Pattern | Description |
|--------------|-------------|
| `/api/opportunities/*` | Opportunity management |
| `/api/quotes/*` | Quote management |
| `/api/products/*` | Product catalog |
| `/api/pipelines/*` | Pipeline definitions |
| `/api/stages/*` | Pipeline stages |

### Marketing Service (Port 5004)
| Path Pattern | Description |
|--------------|-------------|
| `/api/campaigns/*` | Campaign management |
| `/api/leads/*` | Lead management |
| `/api/communications/*` | Communication tracking |

### ServiceDesk Service (Port 5005)
| Path Pattern | Description |
|--------------|-------------|
| `/api/servicerequests/*` | Service request management |
| `/api/tasks/*` | Task management |
| `/api/workflows/*` | Workflow engine |

### Core Service (Port 5006)
| Path Pattern | Description |
|--------------|-------------|
| `/api/*` (fallback) | All other endpoints |
| `/api/settings/*` | System settings |
| `/api/dashboard/*` | Dashboard data |
| `/api/monitoring/*` | Health and monitoring |

---

## Solution Structure

```
CRM.Backend/
├── CRM.Microservices.sln          # Microservices solution
├── CRM.sln                        # Monolith solution
└── src/
    ├── CRM.Core/                  # Shared domain entities & interfaces
    ├── CRM.Infrastructure/        # Shared data access & services
    ├── CRM.Api/                   # Monolithic API (production ready)
    └── Services/
        ├── CRM.ServiceDefaults/   # Shared configuration library
        ├── CRM.Gateway/           # Ocelot API Gateway
        ├── CRM.Identity/          # Identity microservice
        ├── CRM.CustomerService/   # Customer microservice
        ├── CRM.SalesService/      # Sales microservice
        ├── CRM.MarketingService/  # Marketing microservice
        ├── CRM.ServiceDeskService/# ServiceDesk microservice
        └── CRM.CoreService/       # Core microservice
```

---

## Build & Deployment

### Build All Microservices
```bash
cd CRM.Backend
dotnet build CRM.Microservices.sln -c Release
```

### Build Individual Service
```bash
./build-microservices.sh build identity
./build-microservices.sh build customer
./build-microservices.sh build gateway
```

### Build Docker Images
```bash
# Build all Docker images
./build-microservices.sh docker all

# Build specific service image
./build-microservices.sh docker identity
```

---

## Deployment Options

### Option 1: Docker Compose (Recommended for Development)
```bash
# Start all microservices
docker-compose -f docker-compose.microservices.yml up -d

# View logs
docker-compose -f docker-compose.microservices.yml logs -f gateway

# Stop all
docker-compose -f docker-compose.microservices.yml down
```

### Option 2: Kubernetes (Production)
```bash
# Apply all microservices manifests
kubectl apply -f kubernetes/microservices/

# Verify deployment
kubectl get pods -n crm-microservices

# Access via port-forward
kubectl port-forward -n crm-microservices svc/crm-gateway 5000:5000
```

### Option 3: Run Locally (Development)
```bash
# Terminal 1 - Gateway
cd CRM.Backend/src/Services/CRM.Gateway && dotnet run

# Terminal 2 - Identity
cd CRM.Backend/src/Services/CRM.Identity && dotnet run

# Terminal 3 - Customer
cd CRM.Backend/src/Services/CRM.CustomerService && dotnet run

# ... repeat for other services
```

---

## Docker Images

### Dockerfiles
All service Dockerfiles are located in the `docker/` directory:

| Service | Dockerfile |
|---------|------------|
| Gateway | `docker/Dockerfile.gateway` |
| Identity | `docker/Dockerfile.identity` |
| Customer | `docker/Dockerfile.customer` |
| Sales | `docker/Dockerfile.sales` |
| Marketing | `docker/Dockerfile.marketing` |
| ServiceDesk | `docker/Dockerfile.servicedesk` |
| Core | `docker/Dockerfile.core` |
| Frontend | `docker/Dockerfile.frontend` |

### Image Naming Convention
```
crm-{service}:latest
crm-{service}:{version}
```

---

## Kubernetes Architecture

### Namespace
All microservices run in the `crm-microservices` namespace.

### Resources per Service
- **Deployment**: 2 replicas by default
- **Service**: ClusterIP for internal communication
- **HorizontalPodAutoscaler**: Auto-scale 2-10 pods based on CPU (70%)
- **ConfigMap**: Environment-specific configuration
- **Secret**: Sensitive configuration (passwords, keys)

### Manifests Location
```
kubernetes/microservices/
├── 00-namespace.yaml
├── 01-configmap.yaml
├── 02-secrets.yaml
├── gateway/
│   ├── deployment.yaml
│   ├── service.yaml
│   └── hpa.yaml
├── identity/
├── customer/
├── sales/
├── marketing/
├── servicedesk/
├── core/
└── frontend/
```

### Scaling
```bash
# Manual scaling
kubectl scale deployment crm-customer -n crm-microservices --replicas=5

# View autoscaler status
kubectl get hpa -n crm-microservices
```

---

## Shared Libraries

### CRM.ServiceDefaults
Provides common configuration for all microservices:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddMariaDbContext<CrmDbContext>(builder.Configuration);

var app = builder.Build();
app.UseServiceDefaults();
app.Run();
```

Features:
- **AddServiceDefaults()**: Adds logging, auth, CORS, health checks, Swagger
- **UseServiceDefaults()**: Configures middleware pipeline
- **AddMariaDbContext<T>()**: Configures database context

### CRM.Core
Contains domain entities, interfaces, and ports (Hexagonal Architecture)

### CRM.Infrastructure
Contains repository implementations, EF Core configuration, and services

---

## Database Strategy

### Current: Shared Database
All services connect to the same MariaDB database (crm_db):
- Simplifies data consistency and transactions
- Services share the same CrmDbContext
- 89 tables across all domains

### Future Options (for Scale)
- Database per service (Customer DB, Sales DB, etc.)
- Event-driven synchronization (RabbitMQ, Kafka)
- Saga pattern for distributed transactions

---

## Health Checks

Each service exposes:
- `/health` - Overall health status
- `/health/ready` - Readiness probe for Kubernetes
- `/health/live` - Liveness probe for Kubernetes

Gateway provides aggregated health:
```bash
curl http://localhost:5000/health
```

---

## Environment Variables

All services support these environment variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `DB_HOST` | Database hostname | mariadb |
| `DB_PORT` | Database port | 3306 |
| `DB_NAME` | Database name | crm_db |
| `DB_USER` | Database username | crm_user |
| `DB_PASSWORD` | Database password | - |
| `Jwt__Secret` | JWT signing key | - |
| `Jwt__Issuer` | JWT issuer | CRMSolution |
| `ASPNETCORE_ENVIRONMENT` | Environment | Production |

---

## Inter-Service Communication

### Synchronous (HTTP)
Services communicate via HTTP through the gateway:
```
Service A → Gateway → Service B
```

### Patterns Used
- **Request/Response**: Standard REST API calls
- **Gateway Aggregation**: Gateway combines responses from multiple services
- **Pass-Through Auth**: JWT tokens forwarded to downstream services

---

## Monitoring & Logging

### Logging
All services use Serilog with structured logging:
```bash
# View logs in Kubernetes
kubectl logs -n crm-microservices -l app=crm-identity -f

# View all service logs
kubectl logs -n crm-microservices -l service=crm -f
```

### Metrics (Planned)
- Prometheus metrics endpoint at `/metrics`
- Grafana dashboards for visualization
- Distributed tracing with OpenTelemetry

---

## Performance Comparison

| Metric | Monolith | Microservices |
|--------|----------|---------------|
| Full Build | ~35 seconds | ~35 seconds (parallel) |
| Single Service Build | N/A | ~3-5 seconds |
| Docker Image (total) | ~300 MB | ~50 MB per service |
| Startup Time | ~8 seconds | ~2 seconds per service |
| Scaling Unit | Entire app | Individual services |
| Deployment Risk | Higher | Lower (isolated) |

---

## Migration Guide

### From Monolith to Microservices

1. **Deploy microservices** alongside monolith
2. **Configure gateway** to route to microservices
3. **Migrate traffic** gradually (canary deployment)
4. **Monitor** performance and errors
5. **Decommission** monolith when stable

### Controller Mapping

| Monolith Controller | Target Service |
|---------------------|----------------|
| AuthController | Identity |
| UsersController | Identity |
| UserGroupsController | Identity |
| CustomersController | Customer |
| ContactsController | Customer |
| AccountsController | Customer |
| OpportunitiesController | Sales |
| QuotesController | Sales |
| ProductsController | Sales |
| CampaignsController | Marketing |
| LeadsController | Marketing |
| ServiceRequestsController | ServiceDesk |
| WorkflowsController | ServiceDesk |
| SettingsController | Core |
| DashboardController | Core |

---

## Troubleshooting

### Service won't start
```bash
# Check logs
kubectl logs -n crm-microservices deployment/crm-identity

# Check pod status
kubectl describe pod -n crm-microservices -l app=crm-identity
```

### Database connection issues
```bash
# Verify database is running
kubectl get pods -n crm-microservices -l app=mariadb

# Test connectivity from a service
kubectl exec -n crm-microservices deployment/crm-identity -- nc -zv mariadb 3306
```

### Gateway routing issues
```bash
# Check gateway logs for routing errors
kubectl logs -n crm-microservices deployment/crm-gateway -f

# Verify service endpoints
kubectl get endpoints -n crm-microservices
```

---

## Related Documentation

- [ARCHITECTURE_OVERVIEW.md](ARCHITECTURE_OVERVIEW.md) - High-level system architecture
- [README.md](README.md) - Project overview and quick start
- [TESTING_SUMMARY.md](TESTING_SUMMARY.md) - Testing information
- [docs/deployment/](docs/deployment/) - Deployment guides
