# Backend Documentation

> **Last Updated:** February 1, 2026 | **Version:** 1.7.28

---

## Table of Contents

1. [Overview](#1-overview)
2. [Project Structure](#2-project-structure)
3. [Entities](#3-entities)
4. [Services](#4-services)
5. [Data Access](#5-data-access)
6. [Authentication](#6-authentication)
7. [Dependencies](#7-dependencies)

---

## 1. Overview

The backend is built with ASP.NET Core 8.0 following clean architecture principles:

- **CRM.Api** - Web API layer (Controllers, Middleware, Hubs)
- **CRM.Core** - Domain layer (Entities, DTOs, Interfaces)
- **CRM.Infrastructure** - Infrastructure layer (DbContext, Services)
- **CRM.DatabaseSeeder** - Database seeding utility

---

## 2. Project Structure

```
CRM.Backend/
├── src/
│   ├── CRM.Api/                        # Web API Project
│   │   ├── Controllers/                # 50+ REST Controllers
│   │   │   ├── AuthController.cs       # Authentication
│   │   │   ├── CustomersController.cs  # Customer CRUD
│   │   │   ├── ContactsController.cs   # Contact CRUD
│   │   │   ├── LeadsController.cs      # Lead management
│   │   │   ├── OpportunitiesController.cs
│   │   │   ├── ProductsController.cs
│   │   │   ├── CampaignsController.cs
│   │   │   ├── QuotesController.cs
│   │   │   ├── TasksController.cs
│   │   │   ├── NotesController.cs
│   │   │   ├── ServiceRequestsController.cs
│   │   │   ├── WorkflowController.cs
│   │   │   ├── SystemSettingsController.cs
│   │   │   ├── UserGroupsController.cs
│   │   │   └── ... (50+ controllers)
│   │   ├── Hubs/
│   │   │   └── CrmNotificationHub.cs   # SignalR Hub
│   │   ├── Middleware/
│   │   │   ├── ExceptionMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   └── Program.cs                  # Application entry
│   │
│   ├── CRM.Core/                       # Domain Layer
│   │   ├── Entities/                   # 90+ Entity classes
│   │   ├── DTOs/                       # Data Transfer Objects
│   │   ├── Interfaces/                 # Service interfaces
│   │   └── Enums/                      # Enumerations
│   │
│   └── CRM.Infrastructure/             # Infrastructure Layer
│       ├── Data/
│       │   ├── CrmDbContext.cs         # EF Core DbContext
│       │   └── Configurations/         # Entity configurations
│       └── Services/                   # Business services
│
├── tests/                              # Test projects
│   ├── CRM.Api.Tests/
│   ├── CRM.Core.Tests/
│   └── CRM.Infrastructure.Tests/
│
└── migrations/                         # SQL Server migrations
```

---

## 3. Entities

### 3.1 Core Entities

| Entity | File | Table | Description |
|--------|------|-------|-------------|
| **User** | `User.cs` | `Users` | System users with auth |
| **UserGroup** | `UserGroup.cs` | `UserGroups` | Permission groups |
| **UserGroupMember** | `UserGroupMember.cs` | `UserGroupMembers` | User-group mapping |
| **Customer** | *(Account.cs)* | `Customers` | B2B/B2C accounts |
| **Contact** | `ContactDetail.cs` | `Contacts` | Contact persons |
| **Lead** | `Lead.cs` | `Leads` | Sales leads |
| **Opportunity** | `Opportunity.cs` | `Opportunities` | Sales opportunities |
| **Product** | `Product.cs` | `Products` | Product catalog |
| **Quote** | `Quote.cs` | `Quotes` | Price quotes |
| **QuoteLineItem** | `QuoteLineItem.cs` | `QuoteLineItems` | Quote items |
| **Order** | `Order.cs` | `Orders` | Customer orders |
| **Invoice** | `Invoice.cs` | `Invoices` | Invoices |

### 3.2 Marketing Entities

| Entity | File | Table | Description |
|--------|------|-------|-------------|
| **MarketingCampaign** | `MarketingCampaign.cs` | `Campaigns` | Marketing campaigns |
| **CampaignRecipient** | `CampaignRecipient.cs` | `CampaignRecipients` | Campaign targets |
| **CampaignMetric** | `CampaignMetric.cs` | `CampaignMetrics` | Performance metrics |
| **CampaignABTest** | `CampaignABTest.cs` | `CampaignABTests` | A/B test variants |
| **CampaignWorkflow** | `CampaignWorkflow.cs` | `CampaignWorkflows` | Automated workflows |
| **EmailTemplate** | `EmailTemplate.cs` | `EmailTemplates` | Email templates |
| **EmailSequence** | `EmailSequence.cs` | `EmailSequences` | Drip campaigns |

### 3.3 Service & Support Entities

| Entity | File | Table | Description |
|--------|------|-------|-------------|
| **ServiceRequest** | `ServiceRequest.cs` | `ServiceRequests` | Support tickets |
| **CrmTask** | `CrmTask.cs` | `Tasks` | User tasks |
| **Activity** | `Activity.cs` | `Activities` | Activity tracking |
| **Note** | `Note.cs` | `Notes` | Entity notes |
| **Interaction** | `Interaction.cs` | `Interactions` | Customer interactions |

### 3.4 Workflow & Automation

| Entity | File | Table | Description |
|--------|------|-------|-------------|
| **Workflow** | `Workflow/Workflow.cs` | `Workflows` | Workflow definitions |
| **WorkflowStep** | `Workflow/WorkflowStep.cs` | `WorkflowSteps` | Workflow steps |
| **WorkflowTrigger** | `Workflow/WorkflowTrigger.cs` | `WorkflowTriggers` | Triggers |
| **WorkflowAction** | `Workflow/WorkflowAction.cs` | `WorkflowActions` | Actions |

### 3.5 Configuration Entities

| Entity | File | Table | Description |
|--------|------|-------|-------------|
| **SystemSettings** | `SystemSettings.cs` | `SystemSettings` | Global settings |
| **ModuleUIConfig** | `ModuleUIConfig.cs` | `ModuleUIConfigs` | UI configuration |
| **ColorPalette** | `ColorPalette.cs` | `ColorPalettes` | Theme palettes |
| **LookupCategory** | `LookupCategory.cs` | `LookupCategories` | Dropdown categories |
| **LookupItem** | `LookupItem.cs` | `LookupItems` | Dropdown values |

### 3.6 Base Entity

All entities inherit from `BaseEntity`:

```csharp
public class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
```

### 3.7 User Entity (Example)

```csharp
public class User : BaseEntity
{
    // Identity
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    
    // Profile
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    // Security
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    
    // Password Management (New in 1.7.28)
    public DateTime? PasswordLastChangedAt { get; set; }
    public bool MustResetPassword { get; set; }
    public bool PasswordNeverSet { get; set; }
    public string? BackupCodes { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    
    // Navigation
    public virtual ICollection<UserGroupMember> GroupMemberships { get; set; }
}
```

---

## 4. Services

### 4.1 Service Pattern

All services implement an interface and are registered via dependency injection:

```csharp
// Interface
public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetAllAsync(QueryParameters query);
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto);
    Task<bool> DeleteAsync(int id);
}

// Implementation
public class CustomerService : ICustomerService
{
    private readonly CrmDbContext _context;
    private readonly IMapper _mapper;
    
    public CustomerService(CrmDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    // Implementation...
}
```

### 4.2 Core Services

| Service | Interface | Purpose |
|---------|-----------|---------|
| `AuthenticationService` | `IAuthenticationService` | Login, JWT, password management |
| `JwtTokenService` | `IJwtTokenService` | Token generation/validation |
| `CustomerService` | `ICustomerService` | Customer CRUD |
| `ContactService` | `IContactService` | Contact management |
| `LeadService` | `ILeadService` | Lead pipeline |
| `OpportunityService` | `IOpportunityService` | Sales opportunities |
| `ProductService` | `IProductService` | Product catalog |
| `QuoteService` | `IQuoteService` | Quote management |
| `CampaignService` | `ICampaignService` | Campaign management |
| `CampaignExecutionService` | `ICampaignExecutionService` | Campaign delivery |
| `WorkflowService` | `IWorkflowService` | Workflow automation |
| `RelationshipService` | `IRelationshipService` | Account relationships |
| `SystemSettingsService` | `ISystemSettingsService` | Global settings |
| `UserGroupService` | `IUserGroupService` | User groups |
| `NotificationService` | `INotificationService` | SignalR notifications |

### 4.3 Authentication Service

Key methods for password and security:

```csharp
public interface IAuthenticationService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> SetupPasswordAsync(SetPasswordRequest request);
    Task<PasswordComplexityRequirements> GetPasswordRequirementsAsync();
    Task<bool> ValidatePasswordComplexityAsync(string password);
    Task<bool> EnableTwoFactorAsync(int userId);
    Task<bool> DisableTwoFactorAsync(int userId);
}
```

---

## 5. Data Access

### 5.1 DbContext

The `CrmDbContext` is the main EF Core context with 89+ DbSets:

```csharp
public class CrmDbContext : DbContext, ICrmDbContext
{
    // Core
    public DbSet<User> Users { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    
    // Sales
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<Product> Products { get; set; }
    
    // Marketing
    public DbSet<MarketingCampaign> Campaigns { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    
    // Support
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<CrmTask> Tasks { get; set; }
    
    // Configuration
    public DbSet<SystemSettings> SystemSettings { get; set; }
    
    // ... 89+ total DbSets
}
```

### 5.2 Query Patterns

**Paginated Query:**
```csharp
public async Task<PagedResult<T>> GetPagedAsync<T>(
    IQueryable<T> query,
    int page,
    int pageSize)
{
    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<T>
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
    };
}
```

**Soft Delete Filter:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Global query filter for soft deletes
    modelBuilder.Entity<BaseEntity>()
        .HasQueryFilter(e => !e.IsDeleted);
}
```

### 5.3 Database Providers

| Provider | Connection String Key | Notes |
|----------|----------------------|-------|
| **MariaDB** | `MariaDb` | Primary, production |
| **SQL Server** | `SqlServer` | Supported, migrations |
| **PostgreSQL** | `PostgreSql` | Supported |

---

## 6. Authentication

### 6.1 JWT Configuration

```csharp
// appsettings.json
{
  "Jwt": {
    "Secret": "...",           // Min 32 chars
    "Issuer": "CRMSolution",
    "Audience": "CRMSolution",
    "ExpirationMinutes": 60,
    "RefreshExpirationDays": 7
  }
}
```

### 6.2 Auth Flow

1. **Login:** `POST /api/auth/login` with email/password
2. **Token Response:** Access token (60 min) + Refresh token (7 days)
3. **Protected Requests:** `Authorization: Bearer {accessToken}`
4. **Refresh:** `POST /api/auth/refresh` with refresh token

### 6.3 Password Management (New in 1.7.28)

**Password Setup Flow:**
```
Login → Check PasswordNeverSet → Redirect to /setup-password
Login → Check MustResetPassword → Redirect to /setup-password
Login → Check Password Expired → Handle based on group policy
```

**Group Policies:**
- `PasswordExpirationDays` - Days until expiration
- `PasswordExpirationPolicy` - None(0), MustChange(1), Alert(2), Warn(3)
- `RequireTwoFactor` - 2FA suggested
- `EnforceTwoFactor` - 2FA mandatory

---

## 7. Dependencies

### 7.1 NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.* | 8.0.x | Web framework |
| Microsoft.EntityFrameworkCore | 8.0.x | ORM |
| Pomelo.EntityFrameworkCore.MySql | 8.0.x | MariaDB/MySQL provider |
| Microsoft.AspNetCore.SignalR | 8.0.x | Real-time |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.x | JWT auth |
| BCrypt.Net-Next | 4.0.x | Password hashing |
| AutoMapper | 12.0.x | Object mapping |
| FluentValidation | 11.x | Request validation |
| Serilog | 3.x | Logging |
| Swashbuckle.AspNetCore | 6.x | Swagger/OpenAPI |

### 7.2 Project References

```xml
<!-- CRM.Api.csproj -->
<ProjectReference Include="..\CRM.Core\CRM.Core.csproj" />
<ProjectReference Include="..\CRM.Infrastructure\CRM.Infrastructure.csproj" />

<!-- CRM.Infrastructure.csproj -->
<ProjectReference Include="..\CRM.Core\CRM.Core.csproj" />
```

---

## Related Documentation

- [API Reference](../04-api/README.md)
- [Database Schema](database.md)
- [Testing](../07-testing/README.md)
