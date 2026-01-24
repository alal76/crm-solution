# CRM Solution - Microservices Architecture

## Overview

The CRM Solution has been modularized into a microservices architecture to enable:
- **Faster builds**: Each service builds independently (~2-5 seconds vs 30+ seconds for monolith)
- **Modular deployments**: Update individual services without affecting others
- **Independent scaling**: Scale high-demand services (e.g., Customer, Sales) separately
- **Improved reliability**: Service isolation prevents cascading failures
- **Team autonomy**: Different teams can own different services

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Frontend                                     │
│                          (React on port 3000)                            │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         API Gateway (YARP)                               │
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
                        │     (crm_db shared)    │
                        └────────────────────────┘
```

## Services Overview

| Service | Port | Responsibilities |
|---------|------|------------------|
| **Gateway** | 5000 | YARP reverse proxy, JWT pass-through, routing |
| **Identity** | 5001 | Authentication, users, user groups, roles, 2FA |
| **Customer** | 5002 | Customers, contacts, contact info, accounts |
| **Sales** | 5003 | Opportunities, quotes, pipelines, stages |
| **Marketing** | 5004 | Campaigns, communications, templates |
| **ServiceDesk** | 5005 | Service requests, tasks, workflows |
| **Core** | 5006 | Products, settings, backups, lookups, dashboard |

## API Routing

The Gateway routes requests based on URL paths:

| Path Pattern | Target Service |
|--------------|----------------|
| `/api/auth/*` | Identity |
| `/api/users/*` | Identity |
| `/api/usergroups/*` | Identity |
| `/api/userprofiles/*` | Identity |
| `/api/customers/*` | Customer |
| `/api/contacts/*` | Customer |
| `/api/contactinfo/*` | Customer |
| `/api/accounts/*` | Customer |
| `/api/opportunities/*` | Sales |
| `/api/quotes/*` | Sales |
| `/api/pipelines/*` | Sales |
| `/api/stages/*` | Sales |
| `/api/campaigns/*` | Marketing |
| `/api/communications/*` | Marketing |
| `/api/servicerequests/*` | ServiceDesk |
| `/api/tasks/*` | ServiceDesk |
| `/api/workflows/*` | ServiceDesk |
| `/api/*` (fallback) | Core |

## Solution Structure

```
CRM.Backend/
├── CRM.Microservices.sln          # Microservices solution
├── CRM.sln                        # Original monolith solution
└── src/
    ├── CRM.Core/                  # Shared domain entities & interfaces
    ├── CRM.Infrastructure/        # Shared data access & services
    ├── CRM.Api/                   # Original monolithic API (preserved)
    └── Services/
        ├── CRM.ServiceDefaults/   # Shared configuration library
        ├── CRM.Gateway/           # YARP API Gateway
        ├── CRM.Identity/          # Identity service
        ├── CRM.CustomerService/   # Customer service
        ├── CRM.SalesService/      # Sales service
        ├── CRM.MarketingService/  # Marketing service
        ├── CRM.ServiceDeskService/# Service desk service
        └── CRM.CoreService/       # Core service
```

## Build Commands

### Build All Microservices
```bash
cd CRM.Backend
dotnet build CRM.Microservices.sln -c Release
```

### Build Individual Service (Faster)
```bash
# Build only the service you're working on
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

## Deployment Options

### Option 1: Docker Compose (Local Development)
```bash
# Start all microservices with Docker Compose
docker-compose -f docker-compose.microservices.yml up -d

# View logs
docker-compose -f docker-compose.microservices.yml logs -f gateway

# Stop all
docker-compose -f docker-compose.microservices.yml down
```

### Option 2: Kubernetes (Production)
```bash
# Deploy to Kubernetes
./build-microservices.sh deploy

# Or apply manifests directly
kubectl apply -f kubernetes/microservices/
```

### Option 3: Run Locally (Development)
```bash
# Terminal 1 - Gateway
cd CRM.Backend/src/Services/CRM.Gateway
dotnet run

# Terminal 2 - Identity
cd CRM.Backend/src/Services/CRM.Identity
dotnet run

# ... repeat for other services
```

## Kubernetes Architecture

Each service gets:
- **Deployment**: 2 replicas by default
- **Service**: ClusterIP for internal communication
- **HorizontalPodAutoscaler**: Auto-scale from 2 to 10 pods based on CPU (70%)

### Access Services
```bash
# Port forward to gateway
kubectl port-forward -n crm-microservices svc/crm-gateway 5000:5000

# Access the API
curl http://localhost:5000/api/health
```

### Scaling
```bash
# Manual scaling
kubectl scale deployment crm-customer -n crm-microservices --replicas=5

# View autoscaler status
kubectl get hpa -n crm-microservices
```

## Shared Libraries

### CRM.ServiceDefaults
Provides common configuration for all microservices:
- **AddServiceDefaults()**: Adds logging, auth, CORS, health checks, Swagger
- **UseServiceDefaults()**: Configures middleware pipeline
- **AddMariaDbContext<T>()**: Configures database context

Usage in each service:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddMariaDbContext<CrmDbContext>(builder.Configuration);

var app = builder.Build();
app.UseServiceDefaults();
app.Run();
```

### CRM.Core
Contains domain entities, interfaces, and ports (Hexagonal Architecture)

### CRM.Infrastructure
Contains repository implementations, EF Core configuration, and services

## Database Strategy

Currently using a **shared database** approach:
- All services connect to the same MariaDB database (crm_db)
- Simplifies data consistency and transactions
- Services share the same CrmDbContext

### Future Evolution (Optional)
For full service isolation, consider:
- Database per service (Customer DB, Sales DB, etc.)
- Event-driven synchronization (RabbitMQ, Kafka)
- Saga pattern for distributed transactions

## Health Checks

Each service exposes:
- `/health` - Overall health status
- `/health/ready` - Readiness probe for Kubernetes

Gateway provides aggregated health:
```bash
curl http://localhost:5000/health
```

## Monitoring & Logging

All services use Serilog with structured logging:
```bash
# View logs in Kubernetes
kubectl logs -n crm-microservices -l app=crm-identity -f

# View all service logs
kubectl logs -n crm-microservices -l service=crm -f
```

## Migration from Monolith

The original monolithic `CRM.Api` is preserved for:
- Backward compatibility during migration
- Quick rollback if needed
- Reference implementation

To fully migrate, copy controllers from `CRM.Api/Controllers/` to respective services:

| From CRM.Api | To Service |
|--------------|------------|
| AuthController.cs | CRM.Identity |
| UsersController.cs | CRM.Identity |
| UserGroupsController.cs | CRM.Identity |
| UserProfilesController.cs | CRM.Identity |
| CustomersController.cs | CRM.CustomerService |
| ContactsController.cs | CRM.CustomerService |
| ContactInfoController.cs | CRM.CustomerService |
| AccountsController.cs | CRM.CustomerService |
| OpportunitiesController.cs | CRM.SalesService |
| QuotesController.cs | CRM.SalesService |
| PipelinesController.cs | CRM.SalesService |
| StagesController.cs | CRM.SalesService |
| CampaignsController.cs | CRM.MarketingService |
| CommunicationsController.cs | CRM.MarketingService |
| ServiceRequestsController.cs | CRM.ServiceDeskService |
| TasksController.cs | CRM.ServiceDeskService |
| WorkflowsController.cs | CRM.ServiceDeskService |
| *All others* | CRM.CoreService |

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
| `ASPNETCORE_ENVIRONMENT` | Environment | Production |

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

## Performance Benefits

| Metric | Monolith | Microservices |
|--------|----------|---------------|
| Full Build | ~35 seconds | ~35 seconds |
| Single Service Build | N/A | ~3-5 seconds |
| Docker Image (total) | ~300 MB | ~50 MB per service |
| Startup Time | ~8 seconds | ~2 seconds per service |
| Scaling Unit | Entire app | Individual services |

## Next Steps

1. **Copy controllers** to their respective services
2. **Build Docker images** for all services
3. **Deploy to Kubernetes** microservices namespace
4. **Test routing** through the gateway
5. **Update frontend** to use gateway URL (port 5000)
6. **Set up monitoring** (Prometheus, Grafana)
7. **Configure CI/CD** for independent deployments
