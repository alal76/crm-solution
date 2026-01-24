# Hexagonal Architecture - CRM Solution

## Overview

The CRM Solution has been refactored to follow the Hexagonal Architecture (Ports and Adapters) pattern. This document explains the architecture and how to work with it.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              EXTERNAL ACTORS                                     │
│                     (Users, Frontend, External Systems)                          │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          PRIMARY ADAPTERS (Driving)                              │
│                                                                                  │
│   ┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐          │
│   │  API Controllers  │  │   CLI Commands    │  │   Background Jobs │          │
│   └───────────────────┘  └───────────────────┘  └───────────────────┘          │
│                                                                                  │
│   Controllers/  - REST API endpoints that handle HTTP requests                   │
│   These call INPUT PORTS to trigger application use cases                        │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          INPUT PORTS (Driving Ports)                             │
│                                                                                  │
│   Core/Ports/Input/IInputPorts.cs                                               │
│                                                                                  │
│   ICustomerInputPort    - Customer CRUD and management                           │
│   IContactInputPort     - Contact management                                     │
│   IOpportunityInputPort - Sales pipeline operations                              │
│   IProductInputPort     - Product catalog management                             │
│   ICampaignInputPort    - Marketing campaign operations                          │
│   IAuthInputPort        - Authentication and authorization                       │
│   IUserInputPort        - User management                                        │
│   IUserGroupInputPort   - Group/permission management                            │
│   ISystemSettingsInputPort - System configuration                                │
│   IServiceRequestInputPort - Service request handling                            │
│   IAccountInputPort     - Account management                                     │
│   IDatabaseBackupInputPort - Database operations                                 │
│                                                                                  │
│   These extend existing I*Service interfaces for backward compatibility          │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          APPLICATION SERVICES                                    │
│                                                                                  │
│   Infrastructure/Services/                                                       │
│                                                                                  │
│   CustomerService        implements ICustomerInputPort, ICustomerService         │
│   ContactsService        implements IContactInputPort, IContactsService          │
│   OpportunityService     implements IOpportunityInputPort, IOpportunityService   │
│   ProductService         implements IProductInputPort, IProductService           │
│   MarketingCampaignService implements ICampaignInputPort, IMarketingCampaignService │
│   AuthenticationService  implements IAuthInputPort, IAuthenticationService       │
│   UserService            implements IUserInputPort, IUserService                 │
│   UserGroupService       implements IUserGroupInputPort, IUserGroupService       │
│   SystemSettingsService  implements ISystemSettingsInputPort, ISystemSettingsService │
│   ServiceRequestService  implements IServiceRequestInputPort, IServiceRequestService │
│   AccountService         implements IAccountInputPort, IAccountService           │
│   DatabaseBackupService  implements IDatabaseBackupInputPort, IDatabaseBackupService │
│                                                                                  │
│   Services contain business logic and use OUTPUT PORTS for data access           │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          OUTPUT PORTS (Driven Ports)                             │
│                                                                                  │
│   Core/Ports/Output/IOutputPorts.cs                                             │
│                                                                                  │
│   Repository Ports:                                                              │
│   IRepositoryPort<T>     - Generic CRUD operations for entities                  │
│   ICustomerRepositoryPort - Customer-specific queries                            │
│   IOpportunityRepositoryPort - Opportunity-specific queries                      │
│   IProductRepositoryPort  - Product-specific queries                             │
│   ICampaignRepositoryPort - Campaign-specific queries                            │
│   IUserRepositoryPort     - User-specific queries                                │
│   IUserGroupRepositoryPort - Group-specific queries                              │
│                                                                                  │
│   External Service Ports:                                                        │
│   ITokenPort             - JWT token generation/validation                       │
│   IPasswordPort          - Password hashing/verification                         │
│   IEmailPort             - Email sending operations                              │
│   IExternalApiPort       - External HTTP API calls                               │
│   IFileStoragePort       - File storage operations                               │
│   ICachePort             - Caching operations                                    │
│   ITotpPort              - Two-factor authentication                             │
│   IUnitOfWorkPort        - Transaction coordination                              │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          SECONDARY ADAPTERS (Driven)                             │
│                                                                                  │
│   Infrastructure/Repositories/                                                   │
│   Repository<T>          - Generic repository implementation                     │
│                                                                                  │
│   Infrastructure/Data/                                                           │
│   CrmDbContext           - Entity Framework database context                     │
│                                                                                  │
│   Infrastructure/Services/                                                       │
│   JwtTokenService        - JWT token implementation                              │
│   TotpService            - TOTP implementation                                   │
│                                                                                  │
│   These implement OUTPUT PORTS and handle external integrations                   │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          EXTERNAL SYSTEMS                                        │
│                                                                                  │
│   ┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐          │
│   │     Database      │  │   Email Server    │  │   File Storage    │          │
│   │  (MariaDB/MySQL)  │  │     (SMTP)        │  │  (Local/Cloud)    │          │
│   └───────────────────┘  └───────────────────┘  └───────────────────┘          │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## Project Structure

```
CRM.Backend/
├── src/
│   ├── CRM.Core/                      # Domain Layer
│   │   ├── Entities/                  # Domain entities
│   │   ├── Dtos/                      # Data Transfer Objects
│   │   ├── Interfaces/                # Service interfaces (legacy)
│   │   ├── Models/                    # Domain models
│   │   └── Ports/                     # Hexagonal ports
│   │       ├── Input/                 # Driving ports (use cases)
│   │       │   └── IInputPorts.cs
│   │       └── Output/                # Driven ports (external)
│   │           └── IOutputPorts.cs
│   │
│   ├── CRM.Infrastructure/            # Infrastructure Layer
│   │   ├── Data/                      # Database context
│   │   ├── Repositories/              # Repository implementations
│   │   └── Services/                  # Service implementations
│   │
│   └── CRM.Api/                       # Presentation Layer
│       ├── Controllers/               # API controllers (primary adapters)
│       └── Program.cs                 # DI configuration
│
└── tests/                             # Test projects
```

## Key Principles

### 1. Dependency Rule
Dependencies flow inward. External layers depend on inner layers, never the reverse.

```
Controllers → Input Ports → Services → Output Ports → Repositories
```

### 2. Port Interfaces
- **Input Ports**: Define what the application *can do* (use cases)
- **Output Ports**: Define what the application *needs* (dependencies)

### 3. Adapter Pattern
- **Primary Adapters**: Drive the application (Controllers, CLI)
- **Secondary Adapters**: Are driven by the application (Repositories, Email)

### 4. Backward Compatibility
Input ports extend existing service interfaces:
```csharp
public interface ICustomerInputPort : ICustomerService { }
```

This allows gradual migration from `ICustomerService` to `ICustomerInputPort`.

## DI Registration

Both legacy and hexagonal interfaces are registered:

```csharp
// Legacy registration (backward compatibility)
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Hexagonal registration (new code)
builder.Services.AddScoped<ICustomerInputPort, CustomerService>();
```

## Usage in Controllers

**Legacy approach:**
```csharp
public class CustomersController(ICustomerService customerService)
```

**Hexagonal approach:**
```csharp
public class CustomersController(ICustomerInputPort customerPort)
```

## Benefits

1. **Testability**: Easy to mock ports for unit testing
2. **Flexibility**: Swap implementations without changing domain logic
3. **Maintainability**: Clear boundaries between layers
4. **Scalability**: Easy to add new adapters (CLI, GraphQL, etc.)

## Migration Path

1. ✅ Define Input Ports (extend existing interfaces)
2. ✅ Define Output Ports (repository and service ports)
3. ✅ Update services to implement Input Ports
4. ✅ Register Input Ports in DI
5. ⏳ Gradually update controllers to use Input Ports
6. ⏳ Implement specialized Output Port adapters as needed
7. ⏳ Add Output Port implementations for external services

## Related Documentation

- [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) - Overall implementation details
- [CONTACTS_IMPLEMENTATION.md](CONTACTS_IMPLEMENTATION.md) - Contact module details
- [TESTING_GUIDE.md](TESTING_GUIDE.md) - Testing approach
