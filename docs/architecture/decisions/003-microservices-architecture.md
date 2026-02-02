# ADR-003: Microservices Architecture

## Status

Accepted

## Date

2026-02-02

## Context

The CRM Solution initially started as a monolithic application. As the system grew, we faced challenges:

1. **Scalability**: Entire application had to scale together
2. **Deployment risk**: Single deployment affected all features
3. **Team independence**: Teams couldn't work independently
4. **Technology lock-in**: Entire codebase tied to same stack
5. **Resilience**: Single point of failure for all features

## Decision

We will implement a microservices architecture with the following services:

### Service Decomposition

| Service | Port | Responsibility |
|---------|------|----------------|
| Gateway | 5000 | API routing, authentication |
| Identity | 5001 | User management, JWT tokens |
| Customer | 5002 | Account/Contact management |
| Sales | 5003 | Opportunities, Quotes, Orders |
| Marketing | 5004 | Campaigns, Communications |
| ServiceDesk | 5005 | Support tickets, Tasks |
| Core | 5006 | Shared entities, Activities |

### Communication Patterns

1. **Synchronous**: REST APIs via Gateway
2. **Asynchronous**: Redis pub/sub for events
3. **Service Discovery**: Docker DNS / Kubernetes services

### Shared Components

- **CRM.Core**: Entities, DTOs, Interfaces
- **CRM.Infrastructure**: Data access, Services
- **CRM.ServiceDefaults**: Health checks, Telemetry

### Deployment Strategy

- **Local Development**: Docker Compose
- **Production**: Kubernetes on AMD64

## Consequences

### Positive
- **Independent scaling**: Scale services based on load
- **Fault isolation**: Failure in one service doesn't crash all
- **Team autonomy**: Teams own their services
- **Technology flexibility**: Services can use different stacks
- **Faster deployments**: Deploy services independently

### Negative
- **Operational complexity**: More services to manage
- **Network latency**: Inter-service communication overhead
- **Data consistency**: Eventual consistency challenges
- **Debugging difficulty**: Distributed tracing needed
- **Resource overhead**: Each service needs runtime

### Mitigations
- Docker Compose for local development simplicity
- Health checks for service monitoring
- Structured logging with correlation IDs
- Shared database for consistency (Phase 1)
- Gateway pattern for unified API

## Implementation

### Directory Structure
```
CRM.Backend/
├── src/
│   ├── CRM.Core/           # Shared domain
│   ├── CRM.Infrastructure/ # Data access
│   ├── CRM.Api/            # Monolith (development)
│   └── Services/
│       ├── CRM.Gateway/
│       ├── CRM.Identity/
│       ├── CRM.CustomerService/
│       ├── CRM.SalesService/
│       ├── CRM.MarketingService/
│       ├── CRM.ServiceDeskService/
│       └── CRM.CoreService/
```

### Docker Images
- `crm-gateway:latest`
- `crm-identity:latest`
- `crm-customer:latest`
- `crm-sales:latest`
- `crm-marketing:latest`
- `crm-servicedesk:latest`
- `crm-core:latest`
- `crm-frontend:latest`

### Kubernetes Manifests
Located in `kubernetes/microservices/`:
- Namespace and ConfigMap
- Deployments for each service
- Services for internal communication
- Ingress for external access

## Future Considerations

1. **Event Sourcing**: Consider for audit trails
2. **CQRS**: Separate read/write models if needed
3. **Service Mesh**: Istio for advanced traffic management
4. **Database per Service**: When data isolation required

## References

- [Microservices Patterns - Chris Richardson](https://microservices.io/patterns/)
- [Building Microservices - Sam Newman](https://www.oreilly.com/library/view/building-microservices-2nd/9781492034018/)
- [.NET Microservices Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/)
