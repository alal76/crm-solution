# Architecture Decision Records (ADR)

This document captures key architectural decisions made for the CRM Solution, including context, rationale, and consequences.

---

## ADR-001: Dual Architecture Support (Monolith + Microservices)

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

The CRM solution needs to serve different deployment scenarios:
- Small/medium businesses want simple, cost-effective deployment
- Large enterprises need scalability and service isolation
- Development teams need fast local development experience

### Decision

Implement both monolithic and microservices architectures that share the same core business logic:

1. **Monolithic API** (`CRM.Api`): Single application for simple deployments
2. **Microservices** (`Services/*`): Decomposed by business domain for scalability
3. **Shared Libraries** (`CRM.Core`, `CRM.Infrastructure`): Common domain logic

### Rationale

- **Flexibility**: Organizations can choose deployment model based on needs
- **Progressive Migration**: Start with monolith, migrate to microservices when needed
- **Code Reuse**: Shared libraries prevent duplication
- **Development Speed**: Monolith is faster for initial development

### Consequences

**Positive:**
- ✅ Deployment flexibility for different organization sizes
- ✅ Clear migration path from monolith to microservices
- ✅ Faster builds for microservices (3-5s vs 30s for monolith)
- ✅ Independent scaling of high-demand services

**Negative:**
- ❌ Additional complexity maintaining two architectures
- ❌ Increased build/test time for both modes
- ❌ Risk of feature parity issues between architectures

**Mitigation:**
- Share 90% of code between architectures via common libraries
- Use same database schema for both architectures
- Automated E2E tests run against both deployment modes

---

## ADR-002: Shared Database for Microservices

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

Microservices typically use database-per-service pattern for complete isolation. However, this adds complexity for:
- Cross-service transactions
- Data consistency
- Query performance for cross-domain operations

### Decision

Use a **shared database** (MariaDB) for all microservices in the initial implementation.

### Rationale

- **Simplicity**: Avoid distributed transaction complexity
- **Performance**: Direct database joins for cross-domain queries
- **Development Speed**: Faster feature delivery without event synchronization
- **Data Consistency**: ACID guarantees within single database

### Consequences

**Positive:**
- ✅ Simple data consistency model
- ✅ Efficient cross-domain queries
- ✅ Standard EF Core migrations
- ✅ Reduced operational complexity

**Negative:**
- ❌ Services not fully independent (database coupling)
- ❌ Schema changes affect multiple services
- ❌ Database becomes scaling bottleneck
- ❌ Harder to adopt polyglot persistence

**Future Options:**
- Migrate to database-per-service when scale requires it
- Implement event-driven synchronization (RabbitMQ, Kafka)
- Use Saga pattern for distributed transactions

---

## ADR-003: React with TypeScript for Frontend

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

Need to choose frontend framework and technology stack that provides:
- Type safety for large codebase
- Rich component ecosystem
- Strong community support
- Good developer experience

### Decision

Use **React 18 with TypeScript** and Material-UI (MUI) v5 for the frontend.

### Rationale

- **React**: Industry standard with largest ecosystem
- **TypeScript**: Compile-time type checking prevents bugs
- **MUI**: Professional UI components out-of-the-box
- **Community**: Extensive documentation and support

### Consequences

**Positive:**
- ✅ Type safety catches errors at compile time
- ✅ Rich component library (MUI) speeds development
- ✅ Strong hiring pool of React developers
- ✅ Excellent tooling (VS Code, ESLint, Prettier)

**Negative:**
- ❌ TypeScript learning curve for junior developers
- ❌ Bundle size larger than lighter alternatives
- ❌ MUI styling learning curve

---

## ADR-004: SignalR for Real-Time Features

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

CRM needs real-time features:
- Live notifications for workflow events
- Concurrent editing indicators
- Real-time dashboard updates

### Decision

Use **SignalR** for real-time bidirectional communication between server and clients.

### Rationale

- **Native .NET Integration**: First-class support in ASP.NET Core
- **Multiple Transports**: WebSockets, Server-Sent Events, Long Polling fallback
- **Connection Management**: Automatic reconnection and heartbeat
- **Scalability**: Built-in support for Redis backplane

### Consequences

**Positive:**
- ✅ Seamless integration with .NET backend
- ✅ Automatic connection management
- ✅ Efficient real-time updates
- ✅ Good TypeScript client library

**Negative:**
- ❌ Vendor lock-in to Microsoft stack
- ❌ More complex deployment (need sticky sessions or Redis)

---

## ADR-005: Entity Framework Core for Data Access

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

Need ORM solution for data access that provides:
- Strong typing
- LINQ query support
- Migration management
- Multi-database support

### Decision

Use **Entity Framework Core 8.0** as the primary ORM.

### Rationale

- **Type Safety**: Compile-time query validation
- **LINQ Support**: Familiar C# syntax for queries
- **Migrations**: Code-first schema management
- **Multi-DB**: Support for MariaDB, PostgreSQL, SQL Server

### Consequences

**Positive:**
- ✅ Strongly typed queries
- ✅ Automatic change tracking
- ✅ Built-in migration tools
- ✅ LINQ expressiveness

**Negative:**
- ❌ Performance overhead vs raw SQL
- ❌ N+1 query problems if not careful
- ❌ Complex queries sometimes harder to optimize

**Mitigation:**
- Use `.AsNoTracking()` for read-only queries
- Explicit `.Include()` to prevent N+1 issues
- Raw SQL for complex reporting queries

---

## ADR-006: Repository Pattern for Data Access

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

Need abstraction layer between business logic and data access to:
- Enable unit testing with mocks
- Centralize query logic
- Support potential database changes

### Decision

Implement **Generic Repository Pattern** with `IRepository<T>` interface.

### Rationale

- **Testability**: Easy to mock repositories in unit tests
- **Consistency**: Standardized CRUD operations
- **Flexibility**: Can swap implementation (e.g., caching layer)

### Consequences

**Positive:**
- ✅ Improved testability
- ✅ Consistent data access patterns
- ✅ Easier to add cross-cutting concerns (caching, logging)

**Negative:**
- ❌ Additional abstraction layer
- ❌ Can lead to "leaky abstraction" if not careful
- ❌ May hide EF Core capabilities

---

## ADR-007: JWT for Authentication

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

Need authentication mechanism that:
- Works with REST APIs
- Supports stateless authentication
- Enables distributed systems

### Decision

Use **JWT (JSON Web Tokens)** for authentication with:
- Access tokens (short-lived, 1 hour)
- Refresh tokens (long-lived, 7 days)
- HttpOnly cookies for token storage

### Rationale

- **Stateless**: No server-side session storage required
- **Scalable**: Works across multiple service instances
- **Standard**: Industry-standard specification
- **Claims-Based**: Embed user permissions in token

### Consequences

**Positive:**
- ✅ Scalable across multiple servers
- ✅ No session state to manage
- ✅ Works with microservices architecture
- ✅ Mobile-friendly

**Negative:**
- ❌ Cannot revoke tokens before expiry
- ❌ Token size larger than session ID
- ❌ Need refresh token mechanism

**Mitigation:**
- Short access token lifetime (1 hour)
- Token blacklist for critical cases
- Refresh token rotation

---

## ADR-008: Docker Compose for Development, Kubernetes for Production

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

Need deployment strategy that:
- Simplifies local development
- Supports production scalability
- Enables CI/CD automation

### Decision

Use **Docker Compose** for local development and **Kubernetes** for production deployments.

### Rationale

- **Docker Compose**: Simple, fast local development environment
- **Kubernetes**: Production-grade orchestration, auto-scaling, self-healing
- **Containers**: Consistent environment dev → prod

### Consequences

**Positive:**
- ✅ Fast local development setup
- ✅ Production-grade scalability
- ✅ Environment parity
- ✅ Easy CI/CD integration

**Negative:**
- ❌ Kubernetes learning curve
- ❌ Different tools for dev vs prod
- ❌ More complex troubleshooting

---

## ADR-009: Multi-Database Support (MariaDB Primary)

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

Different organizations have different database preferences and existing infrastructure.

### Decision

Support **multiple database providers** with MariaDB as the primary/default:
- MariaDB (default)
- MySQL
- PostgreSQL
- SQL Server

### Rationale

- **Flexibility**: Organizations can use existing database infrastructure
- **Open Source**: MariaDB is fully open source
- **Performance**: MariaDB has good performance characteristics
- **EF Core Support**: All databases supported by EF Core

### Consequences

**Positive:**
- ✅ Deployment flexibility
- ✅ No vendor lock-in
- ✅ Can use existing database infrastructure

**Negative:**
- ❌ Must test with multiple databases
- ❌ Can't use database-specific features
- ❌ SQL migration scripts need multiple versions

---

## ADR-010: React Context for State Management (Initial)

**Date:** 2025-01  
**Status:** Accepted (Review in Q2 2026)  
**Decision Makers:** Development Team

### Context

Need state management solution for:
- Authentication state
- Theme preferences
- Global application settings

### Decision

Use **React Context API** for initial global state management.

### Rationale

- **Built-in**: No additional dependencies
- **Sufficient**: Current state management needs are simple
- **Type-Safe**: Works well with TypeScript

### Consequences

**Positive:**
- ✅ No additional dependencies
- ✅ Simple mental model
- ✅ Good TypeScript support

**Negative:**
- ❌ Can cause unnecessary re-renders
- ❌ No dev tools for debugging
- ❌ Limited middleware support

**Future Consideration:**
- Evaluate Zustand or Redux if state management becomes complex
- Current threshold: > 5 contexts or complex state interactions

---

## ADR-011: FluentValidation for Backend Validation

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

Need validation library that:
- Separates validation logic from entities
- Provides clear error messages
- Supports complex validation rules

### Decision

Use **FluentValidation** library for server-side validation.

### Rationale

- **Separation of Concerns**: Validation rules separate from entities
- **Expressiveness**: Fluent API is readable and maintainable
- **Integration**: Works seamlessly with ASP.NET Core
- **Reusable**: Rules can be shared across services

### Consequences

**Positive:**
- ✅ Clean, testable validation logic
- ✅ Consistent error messages
- ✅ Easy to unit test validators
- ✅ Supports complex cross-field validation

**Negative:**
- ❌ Additional dependency
- ❌ Learning curve for team

---

## ADR-012: Serilog for Structured Logging

**Date:** 2025-01  
**Status:** Accepted  
**Decision Makers:** Development Team

### Context

Need logging solution that:
- Supports structured logging
- Works across all services
- Enables log aggregation

### Decision

Use **Serilog** with structured logging to console and file sinks.

### Rationale

- **Structured**: Log events with properties, not just strings
- **Flexible**: Multiple sinks (console, file, Elasticsearch, etc.)
- **Performance**: Efficient logging with async writes
- **Enrichment**: Automatic context enrichment

### Consequences

**Positive:**
- ✅ Rich, structured log data
- ✅ Easy to add new log destinations
- ✅ Good performance
- ✅ Searchable logs

**Negative:**
- ❌ Slightly more complex than basic logging

---

## Decision Review Schedule

| ADR | Next Review | Status |
|-----|-------------|--------|
| ADR-001 | Q2 2026 | Active |
| ADR-002 | Q2 2026 | Active - Consider database-per-service if scaling issues |
| ADR-003 | Q4 2026 | Active |
| ADR-004 | Q3 2026 | Active |
| ADR-005 | Q4 2026 | Active |
| ADR-006 | Q2 2026 | Active |
| ADR-007 | Q2 2026 | Active |
| ADR-008 | Q3 2026 | Active |
| ADR-009 | Q4 2026 | Active |
| ADR-010 | Q2 2026 | **Under Review** - Consider Zustand |
| ADR-011 | Q4 2026 | Active |
| ADR-012 | Q4 2026 | Active |

---

## Deprecated Decisions

_None yet_

---

**Document Maintainer:** Architecture Team  
**Review Frequency:** Quarterly  
**Last Reviewed:** February 2026
