# Backend Developer Guide

> **CRM Solution Backend Architecture & Development Guide**  
> Version: 0.614.84  
> Last Updated: March 3, 2026  
> Framework: ASP.NET Core 10.0, Entity Framework Core 9.0

---

## Table of Contents

1. [Overview](#overview)
2. [Solution Structure](#solution-structure)
3. [Getting Started](#getting-started)
4. [Architecture Overview](#architecture-overview)
5. [Core Components](#core-components)
6. [Data Layer](#data-layer)
7. [Service Layer](#service-layer)
8. [API Layer](#api-layer)
9. [AI Integration (Semantic Kernel)](#ai-integration)
10. [Testing](#testing)
11. [Common Patterns](#common-patterns)
12. [Best Practices](#best-practices)
13. [Troubleshooting](#troubleshooting)

---

## Overview

The CRM Backend is a **comprehensive enterprise-grade .NET solution** with:

- **762,433 lines of code** across 1,412 source files
- **178 entity models** with full audit tracking
- **206 service implementations** with dependency injection
- **175 API controllers** exposing REST endpoints
- **19 AI agents** powered by Microsoft Semantic Kernel
- **Multi-database support**: MariaDB, PostgreSQL, SQL Server, Oracle, SQLite, InMemory

### Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Framework** | ASP.NET Core | 10.0 |
| **ORM** | Entity Framework Core | 9.0.1 |
| **Language** | C# | 13.0 (.NET 10.0) |
| **AI/ML** | Microsoft Semantic Kernel | 1.72.0 |
| **Authentication** | JWT Bearer Tokens | ASP.NET Core Identity |
| **Caching** | StackExchange.Redis | 2.8.16 |
| **Validation** | FluentValidation | 11.10.0 |
| **Logging** | Serilog | 4.1.0 |
| **Testing** | xUnit | 2.6.2-2.6.4 |

---

## Solution Structure

```
CRM.Backend/
├── CRM.sln                    # Main solution (Monolith)
├── CRM.Microservices.sln      # Microservices solution
├── Directory.Build.props      # Shared MSBuild properties
├── stylecop.json              # Code quality rules
│
├── src/
│   ├── CRM.Api/               # REST API (Program.cs, Controllers, Middleware)
│   ├── CRM.Core/              # Domain layer (Entities, DTOs, Interfaces)
│   ├── CRM.Infrastructure/    # Persistence, Services, Providers
│   │
│   └── Services/              # Microservices (8 services)
│       ├── CRM.Gateway/       # YARP API Gateway
│       ├── CRM.Identity/      # Auth & User Management
│       ├── CRM.Customer/      # Accounts & Contacts
│       ├── CRM.Sales/         # Opportunities, Quotes, Orders
│       ├── CRM.Marketing/     # Campaigns, Leads
│       ├── CRM.ServiceDesk/   # ITSM (Tickets, Workflows)
│       ├── CRM.Core/          # Settings, System Configuration
│       └── CRM.Defaults/      # Default implementations
│
└── tests/
    ├── CRM.Tests/             # Main test project (3,489 tests)
    ├── CRM.Tests.Unit.Core/   # Entity & DTO tests (2,849 tests)
    ├── CRM.SystemModule.Tests/ # System module tests (83 tests)
    └── Helpers/               # Test fixtures and utilities
```

### Project Breakdown

| Project | Purpose | Files | Lines |
|---------|---------|-------|-------|
| **CRM.Api** | REST API, Controllers, Middleware | 227 | 95,147 |
| **CRM.Core** | Entities, DTOs, Interfaces | 534 | 267,891 |
| **CRM.Infrastructure** | Services, Repositories, Providers | 651 | 399,395 |
| **CRM.Gateway** | Microservices API Gateway | - | - |
| **CRM.Identity** | Authentication & Authorization | - | - |
| **CRM.Customer** | Customer domain microservice | - | - |
| **CRM.Sales** | Sales domain microservice | - | - |
| **CRM.Marketing** | Marketing domain microservice | - | - |
| **CRM.ServiceDesk** | ITSM domain microservice | - | - |

---

## Getting Started

### Prerequisites

```bash
# Required
- .NET 10.0 SDK
- Docker Desktop (for databases)
- Git

# Optional
- Visual Studio 2022 (17.12+) or VS Code
- Rider 2024.3+
```

### Initial Setup

1. **Clone the repository:**
   ```bash
   cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution
   ```

2. **Restore dependencies:**
   ```bash
   cd CRM.Backend
   dotnet restore CRM.sln
   ```

3. **Start database containers:**
   ```bash
   cd ..
   docker-compose -f docker/docker-compose.databases.yml up -d crm-mariadb crm-redis
   ```

4. **Configure database connection:**
   ```bash
   # Create appsettings.Development.json in CRM.Api/
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024;"
     }
   }
   ```

5. **Apply migrations:**
   ```bash
   cd CRM.Backend/src/CRM.Api
   dotnet ef database update
   ```

6. **Run the API:**
   ```bash
   dotnet run
   # API: http://localhost:5000
   # Swagger: http://localhost:5000/swagger
   ```

### Quick Start Script

```bash
# Use the root start script
cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution
./start-dev.sh
```

This script:
- Checks dependencies
- Starts API at http://localhost:5000
- Starts Frontend at http://localhost:3000
- Connects to remote database at 192.168.0.9:3306

---

## Architecture Overview

### Architectural Style

The backend uses **Hexagonal Architecture** (Ports & Adapters) with **Clean Architecture** principles:

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  CRM.Api (Controllers, Middleware, Filters)            │ │
│  │  - 175 Controllers                                      │ │
│  │  - Request/Response DTOs                                │ │
│  │  - Authorization Policies                               │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                       Domain Layer                           │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  CRM.Core (Entities, DTOs, Interfaces)                 │ │
│  │  - 178 Entities (with BaseEntity audit)                │ │
│  │  - 167 Interfaces (Ports)                              │ │
│  │  - Business logic & domain rules                        │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  CRM.Infrastructure (Adapters)                          │ │
│  │  - 206 Services (implementing 167 interfaces)           │ │
│  │  - 340 DbSets (via ICrmDbContext)                      │ │
│  │  - Provider implementations (Search, AI, Chat, etc.)    │ │
│  │  - External integrations                                │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Flow

```
Controllers → Services (Interface) → Domain Entities → Database
     ↓                                      ↓
  DTOs                              Entity Framework Core
                                           ↓
                                        DbContext
```

**Key Principle:** Dependencies point inward (Domain has no dependencies on Infrastructure)

---

## Core Components

### 1. Startup Configuration (`Program.cs`)

**Location:** `CRM.Backend/src/CRM.Api/Program.cs` (1,268 lines)

The main entry point orchestrates:

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Configuration Loading
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{env}.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets();

// 2. Service Registration (Dependency Injection)
builder.Services.AddControllers();
builder.Services.AddDbContext<CrmDbContext>();
builder.Services.AddApplicationServices();  // Custom extension
builder.Services.AddProviders();            // Pluggable providers
builder.Services.AddSemanticKernel();       // AI services

// 3. Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT config */ });

// 4. Middleware Pipeline
var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiting();      // Rate limiting middleware
app.MapControllers();

app.Run();
```

### 2. Dependency Injection Extensions

**Key Extension Methods:**

| Extension | Purpose | Services Registered |
|-----------|---------|---------------------|
| **AddApplicationServices()** | Core business services | 206+ services (Account, Contact, Lead, etc.) |
| **AddRepositories()** | Repository pattern | 178 repositories |
| **AddProviders()** | Pluggable providers | Search, AI, Chat, Analytics, etc. |
| **AddSemanticKernel()** | AI agents & plugins | 19 agents, 12 plugins |
| **AddCaching()** | Redis caching | IDistributedCache, IMemoryCache |
| **AddHealthChecks()** | Health monitoring | DB, Redis, Provider health |

**Example Service Registration:**

```csharp
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // Core CRM services
    services.AddScoped<IAccountService, AccountService>();
    services.AddScoped<IContactService, ContactService>();
    services.AddScoped<ILeadService, LeadService>();
    services.AddScoped<IOpportunityService, OpportunityService>();
    
    // ITSM services
    services.AddScoped<IServiceRequestService, ServiceRequestService>();
    services.AddScoped<IKnowledgeArticleService, KnowledgeArticleService>();
    
    // System services
    services.AddScoped<ISystemSettingsService, SystemSettingsService>();
    services.AddScoped<ISampleDataSeederService, SampleDataSeederService>();
    
    // Add 200+ more services...
    
    return services;
}
```

### 3. Health Checks

**Endpoints:**
- `GET /health` - Overall health
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe
- `GET /api/health/providers` - External provider health

**Configuration:**

```csharp
services.AddHealthChecks()
    .AddDbContextCheck<CrmDbContext>("database")
    .AddRedis(redisConnection, "redis")
    .AddCheck<ProviderHealthCheck>("providers")
    .AddCheck<SemanticKernelHealthCheck>("semantic-kernel");
```

---

## Data Layer

### Database Context (`ICrmDbContext`)

**Location:** `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs` (3,289 lines)

**340 DbSets** covering all domain entities:

```csharp
public class CrmDbContext : DbContext, ICrmDbContext
{
    // Core CRM
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<Product> Products { get; set; }
    
    // ITSM
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<ServiceRequestCategory> ServiceRequestCategories { get; set; }
    public DbSet<KnowledgeArticle> KnowledgeArticles { get; set; }
    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
    
    // Sales
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    
    // Marketing
    public DbSet<MarketingCampaign> MarketingCampaigns { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    
    // + 300+ more DbSets...
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Global query filters (soft delete)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                    Expression.Constant(false)
                );
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(Expression.Lambda(body, parameter));
            }
        }
    }
}
```

**Interface Definition:**

```csharp
public interface ICrmDbContext
{
    // DbSet properties for all 340 entities
    DbSet<Account> Accounts { get; set; }
    // ... all other DbSets ...
    
    // Lifecycle methods
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    int SaveChanges();
    
    // Entry access
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
```

### Entity Base Classes

**BaseEntity** - All entities inherit:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public byte[]? RowVersion { get; set; }  // Optimistic concurrency
}
```

**Key Entities:**

| Entity | Table | Purpose | Key Fields |
|--------|-------|---------|------------|
| **Account** | Customers | Organizations | Name, Email, Type, Status |
| **Contact** | Contacts | People | FirstName, LastName, Email, Phone |
| **Lead** | Leads | Sales leads | Name, Email, Source, Score |
| **Opportunity** | Opportunities | Sales pipeline | Name, Value, Stage, CloseDate |
| **ServiceRequest** | ServiceRequests | Support tickets | Title, Status, Priority, SLA |
| **MarketingCampaign** | MarketingCampaigns | Campaigns | Name, Type, StartDate, Budget |

### Entity Fluent Configuration

**Pattern:**

```csharp
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Customers");  // Note: Table named Customers
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(a => a.Email)
            .HasMaxLength(100);
        
        builder.HasIndex(a => a.Email);
        
        builder.HasMany(a => a.Contacts)
            .WithOne(c => c.Account)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### Multi-Database Provider Support

**Supported Databases:**

```csharp
public enum DatabaseProvider
{
    MySQL,          // MariaDB (primary)
    PostgreSQL,
    SQLServer,
    Oracle,
    SQLite,
    InMemory
}
```

**Configuration:**

```json
{
  "DatabaseProvider": "MySQL",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=crm_db;User=crm_user;Password=***"
  }
}
```

**Provider-Specific Setup:**

```csharp
services.AddDbContext<CrmDbContext>(options =>
{
    var provider = Configuration.GetValue<string>("DatabaseProvider");
    
    switch (provider)
    {
        case "MySQL":
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            break;
        case "PostgreSQL":
            options.UseNpgsql(connectionString);
            break;
        case "SQLServer":
            options.UseSqlServer(connectionString);
            break;
        case "Oracle":
            options.UseOracle(connectionString);
            break;
        case "SQLite":
            options.UseSqlite(connectionString);
            break;
        case "InMemory":
            options.UseInMemoryDatabase("CrmDbInMemory");
            break;
    }
});
```

---

## Service Layer

### Service Pattern

All services follow a consistent interface-first pattern:

```csharp
public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AccountDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken cancellationToken = default);
    Task<AccountDto> UpdateAsync(int id, UpdateAccountDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public class AccountService : IAccountService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<AccountService> _logger;
    
    public AccountService(ICrmDbContext context, ILogger<AccountService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<IEnumerable<AccountDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .Select(a => MapToDto(a))
            .ToListAsync(cancellationToken);
    }
    
    public async Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = new Account
        {
            Name = dto.Name,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Created account {Id}: {Name}", account.Id, account.Name);
        
        return MapToDto(account);
    }
}
```

### Service Categories

| Category | Count | Purpose | Examples |
|----------|-------|---------|----------|
| **CRM Services** | 8 | Core CRM operations | Account, Contact, Lead, Opportunity |
| **ITSM Services** | 12 | Service desk | ServiceRequest, KnowledgeArticle, WorkflowEngine |
| **Marketing Services** | 8 | Marketing automation | Campaign, EmailTemplate, EmailSequence |
| **Sales Services** | 10 | Sales cycle | Quote, Order, Invoice, Payment, Contract |
| **System Services** | 15 | System management | User, Group, Settings, AuditLog |
| **Provider Services** | 7 | External providers | Search, AI, Chat, Notifications |
| **Integration Services** | 6 | Third-party integrations | Webhook, API, OAuth |

### Special Services

#### SampleDataSeederService

**Location:** `CRM.Backend/src/CRM.Infrastructure/Services/SampleDataSeederService.cs` (1,913 lines)

Provides **idempotent sample data seeding** with structured logging:

```csharp
public class SampleDataSeederService : ISampleDataSeederService
{
    // 11 seeding steps
    public async Task<SeedAllResult> SeedAllSampleDataWithLogAsync(CancellationToken cancellationToken)
    {
        var steps = new List<SeedStepResult>();
        var stopwatch = Stopwatch.StartNew();
        
        // Step 1: System data
        steps.Add(await SeedSystemDataAsync(cancellationToken));
        
        // Step 2: Accounts
        steps.Add(await SeedAccountsAsync(cancellationToken));
        
        // ... steps 3-11 ...
        
        return new SeedAllResult
        {
            Steps = steps,
            TotalDurationMs = stopwatch.ElapsedMilliseconds,
            TotalSeeded = steps.Sum(s => s.SeededCount),
            TotalSkipped = steps.Sum(s => s.SkippedCount)
        };
    }
}
```

**Features:**
- Checks for duplicates before seeding
- Returns before/after counts per step
- Structured step results (Seeded/Skipped/Failed)
- Can be run multiple times safely

#### SystemSettingsService

Manages centralized application configuration:

```csharp
public interface ISystemSettingsService
{
    Task<SystemSettings> GetSettingsAsync(CancellationToken cancellationToken);
    Task<SystemSettings> UpdateSettingsAsync(UpdateSystemSettingsDto dto, CancellationToken cancellationToken);
    Task<T> GetSettingAsync<T>(string key, CancellationToken cancellationToken);
    Task SetSettingAsync<T>(string key, T value, CancellationToken cancellationToken);
}
```

#### WorkflowEngineService

**Location:** `CRM.Backend/src/CRM.Infrastructure/Services/WorkflowEngineService.cs`

Executes BPMN-style workflows:

```csharp
public interface IWorkflowEngineService
{
    Task<WorkflowInstance> StartWorkflowAsync(int definitionId, Dictionary<string, object> context);
    Task<WorkflowInstance> ExecuteStepAsync(int instanceId, string stepId);
    Task<WorkflowInstance> GetWorkflowInstanceAsync(int instanceId);
}
```

**Supports:**
- Sequential & parallel steps
- Conditional branching
- Approval gates
- Timers & escalations

---

## API Layer

### Controller Pattern

**Standard CRUD Controller:**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _service;
    private readonly ILogger<AccountsController> _logger;
    
    public AccountsController(IAccountService service, ILogger<AccountsController> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var accounts = await _service.GetAllAsync(cancellationToken);
        return Ok(accounts);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var account = await _service.GetByIdAsync(id, cancellationToken);
        return account != null ? Ok(account) : NotFound();
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateAccountDto dto, CancellationToken cancellationToken)
    {
        var account = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AccountDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountDto dto, CancellationToken cancellationToken)
    {
        var account = await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(account);
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
```

### Controller Categories

| Category | Count | Purpose |
|----------|-------|---------|
| **CRM Controllers** | 8 | Accounts, Contacts, Leads, Opportunities |
| **ITSM Controllers** | 12 | ServiceRequests, KnowledgeArticles, Workflows |
| **Marketing Controllers** | 8 | Campaigns, EmailTemplates, EmailSequences |
| **Sales Controllers** | 10 | Quotes, Orders, Invoices, Payments |
| **System Controllers** | 15 | Users, Groups, Settings, AuditLogs |
| **Provider Controllers** | 7 | Provider health & configuration |
| **Admin Controllers** | 10 | Feature flags, monitoring, diagnostics |
| **AI Controllers** | 5 | Agent chat, analytics, configuration |

### Rate Limiting

**Configuration:** `appsettings.json`

```json
{
  "RateLimiting": {
    "EnableEndpointRateLimiting": true,
    "HttpStatusCode": 429,
    "EndpointRules": {
      "/api/auth/login": { "Period": "1m", "Limit": 5 },
      "/api/accounts": { "Period": "1m", "Limit": 500 },
      "/api/llm": { "Period": "1m", "Limit": 60 }
    }
  }
}
```

**To disable for development:**

```bash
# In appsettings.Development.json
{
  "RateLimiting": {
    "EnableEndpointRateLimiting": false
  }
}
```

---

## AI Integration

### Semantic Kernel Architecture

**Location:** `CRM.Backend/src/CRM.Infrastructure/AI/SK/`

```
AI/SK/
├── Agents/                # 19 specialized AI agents
├── Plugins/               # 12 CRM domain plugins
├── Configuration/         # SK configuration & options
├── Connectors/            # Chat/Embedding connectors
├── Filters/               # Audit, Approval, Cost tracking
└── Services/              # AgentExecutionService
```

### 19 Specialized Agents

| Agent | Purpose | Plugin Dependencies |
|-------|---------|---------------------|
| **LeadScoringAgent** | Score leads based on profile | LeadPlugin, AccountPlugin |
| **SupportTriageAgent** | Triage support tickets | ServiceRequestPlugin, KBPlugin |
| **EmailDraftingAgent** | Draft customer emails | EmailPlugin, ContactPlugin |
| **OpportunityInsightsAgent** | Analyze sales opportunities | OpportunityPlugin, QuotePlugin |
| **CampaignOptimizationAgent** | Optimize marketing campaigns | CampaignPlugin, AnalyticsPlugin |
| **DocumentGenerationAgent** | Generate contracts/quotes | DocumentPlugin, TemplatePlugin |
| **CustomerSegmentationAgent** | Segment customers | AccountPlugin, AnalyticsPlugin |
| **ForecastingAgent** | Sales forecasting | OpportunityPlugin, AnalyticsPlugin |
| **SentimentAnalysisAgent** | Analyze customer sentiment | InteractionPlugin, EmailPlugin |
| **WorkflowRecommendationAgent** | Recommend next steps | WorkflowPlugin, ServiceRequestPlugin |
| **KnowledgeBaseAgent** | KB article suggestions | KBPlugin, ServiceRequestPlugin |
| **ChurnPredictionAgent** | Predict customer churn | AccountPlugin, InteractionPlugin |
| **ProductRecommendationAgent** | Recommend products | ProductPlugin, OpportunityPlugin |
| **PricingOptimizationAgent** | Optimize pricing | QuotePlugin, ProductPlugin |
| **ResourceAllocationAgent** | Allocate resources | UserPlugin, ServiceRequestPlugin |
| **ContractAnalysisAgent** | Analyze contracts | ContractPlugin, DocumentPlugin |
| **ComplianceCheckAgent** | Check compliance | AuditPlugin, DocumentPlugin |
| **DataEnrichmentAgent** | Enrich customer data | AccountPlugin, ContactPlugin |
| **OrchestratorAgent** | Orchestrate multi-agent workflows | All plugins |

### 12 CRM Domain Plugins

Each plugin exposes CRM operations as `[KernelFunction]` methods:

```csharp
public class LeadPlugin
{
    private readonly ILeadService _leadService;
    
    [KernelFunction("GetLeadById")]
    [Description("Retrieve a lead by ID")]
    public async Task<LeadDto?> GetLeadAsync(int id, CancellationToken cancellationToken)
    {
        return await _leadService.GetByIdAsync(id, cancellationToken);
    }
    
    [KernelFunction("ScoreLead")]
    [Description("Calculate lead score based on attributes")]
    public async Task<int> ScoreLeadAsync(int leadId, CancellationToken cancellationToken)
    {
        var lead = await _leadService.GetByIdAsync(leadId, cancellationToken);
        // Lead scoring logic...
        return score;
    }
}
```

**Available Plugins:**
1. AccountPlugin
2. ContactPlugin
3. LeadPlugin
4. OpportunityPlugin
5. ServiceRequestPlugin
6. KnowledgeArticlePlugin
7. EmailPlugin
8. CampaignPlugin
9. QuotePlugin
10. ProductPlugin
11. WorkflowPlugin
12. AnalyticsPlugin

### Agent API Endpoints

**Base Path:** `/api/agents`

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/agents` | GET | List all available agents |
| `/api/agents/{agentId}` | GET | Get agent details |
| `/api/agents/{agentId}/chat` | POST | Chat with specific agent |
| `/api/agents/{agentId}/execute` | POST | Execute agent function |
| `/api/agents/analytics/usage` | GET | Agent usage statistics |
| `/api/agents/admin/filters` | GET | List active filters (audit, cost) |

**Example Agent Chat:**

```bash
curl -X POST http://localhost:5000/api/agents/lead-scoring/chat \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Score lead ID 123 and suggest next actions",
    "conversationId": "conv-123"
  }'
```

**Response:**

```json
{
  "agentId": "lead-scoring",
  "conversationId": "conv-123",
  "message": "Based on the lead profile, I score this lead at 87/100. Recommended actions: 1) Schedule demo, 2) Send pricing proposal, 3) Assign to senior sales rep.",
  "functionCalls": [
    { "plugin": "LeadPlugin", "function": "GetLeadById", "args": { "id": 123 } },
    { "plugin": "LeadPlugin", "function": "ScoreLead", "args": { "leadId": 123 } }
  ],
  "costTracking": {
    "tokensUsed": 450,
    "estimatedCost": 0.0045
  }
}
```

### LLM Provider Configuration

**Supported Providers:**

```csharp
public enum AIProvider
{
    Ollama,         // Local LLM (default for dev)
    OpenAI,
    AzureOpenAI,
    Anthropic,
    AWSBedrock,
    OpenRouter,
    GoogleGemini
}
```

**Configuration:**

```json
{
  "Providers": {
    "AI": {
      "Type": "OpenAI",
      "OpenAI": {
        "ApiKey": "sk-...",
        "Model": "gpt-4o",
        "EmbeddingModel": "text-embedding-3-small"
      },
      "Ollama": {
        "Url": "http://localhost:11434",
        "Model": "llama3.1:8b",
        "EmbeddingModel": "nomic-embed-text"
      }
    }
  },
  "FeatureManagement": {
    "EnableSemanticKernel": true,
    "EnableLeadScoringAgent": true,
    "EnableSupportTriageAgent": true
  }
}
```

---

## Testing

### Test Architecture

**Total Test Coverage:**
- **7,466 backend test cases** across 504 test files
- **42 frontend test files**
- **76 E2E test files** (Playwright)

### Test Project Structure

```
CRM.Backend/tests/
├── CRM.Tests/                     # Main test project (3,489 tests)
│   ├── Services/                  # Service unit tests (92 files)
│   ├── Integration/               # Integration tests (141 files)
│   ├── Controllers/               # Controller tests (30 files)
│   ├── BVT/                       # Build Verification Tests (9 files)
│   └── Providers/                 # Provider tests (3 files)
│
├── CRM.Tests.Unit.Core/           # Entity & DTO tests (2,849 tests)
│   ├── Entities/                  # Entity validation tests
│   ├── Dtos/                      # DTO tests
│   └── Extensions/                # Extension method tests
│
├── CRM.SystemModule.Tests/        # System module tests (83 tests)
└── Helpers/                       # Test fixtures & utilities
    ├── ServiceTestFixtureBase.cs  # Base fixture for service tests
    ├── TestDbContextFactory.cs    # Test DB context creation
    ├── LoggedTestBase.cs          # Automatic logging base
    └── ApiTestFactory.cs          # API integration test helper
```

### Test Framework Stack

```xml
<!-- CRM.Tests.csproj -->
<ItemGroup>
  <PackageReference Include="xUnit" Version="2.6.2" />
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.1" />
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
  <PackageReference Include="coverlet.collector" Version="6.0.0" />
</ItemGroup>
```

### Test Naming Convention

```csharp
// Pattern: {Method}_Should{ExpectedBehavior}_When{Condition}

[Fact]
public async Task GetById_ShouldReturnAccount_WhenAccountExists()
{
    // Arrange
    var account = new Account { Id = 1, Name = "Test Account" };
    MockContext.SetupDbSet(new[] { account });
    
    // Act
    var result = await Service.GetByIdAsync(1, CancellationToken);
    
    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("Test Account");
}

[Fact]
public async Task Create_ShouldThrowValidationException_WhenNameIsEmpty()
{
    // Arrange
    var dto = new CreateAccountDto { Name = "" };
    
    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(() => 
        Service.CreateAsync(dto, CancellationToken));
}
```

### Service Test Base

Most service tests inherit from this fixture:

```csharp
public class AccountServiceTests : ServiceTestFixtureBase<AccountService>
{
    private AccountService Service => CreateService();
    
    private AccountService CreateService()
    {
        return new AccountService(MockContext.Object, MockLogger.Object);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsAllNonDeletedAccounts()
    {
        // Setup test data
        var accounts = new List<Account>
        {
            new Account { Id = 1, Name = "Account 1", IsDeleted = false },
            new Account { Id = 2, Name = "Account 2", IsDeleted = false },
            new Account { Id = 3, Name = "Account 3", IsDeleted = true }  // Deleted
        };
        MockContext.SetupDbSet(accounts);
        
        // Execute
        var results = await Service.GetAllAsync(CancellationToken);
        
        // Verify
        results.Should().HaveCount(2);  // Only non-deleted
        results.Should().NotContain(a => a.Id == 3);
    }
}
```

### Integration Test Setup

```csharp
public class AccountsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public AccountsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Replace real DB with in-memory
                services.RemoveAll<DbContextOptions<CrmDbContext>>();
                services.AddDbContext<CrmDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
        
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task GetAccounts_ReturnsOkResult()
    {
        var response = await _client.GetAsync("/api/accounts");
        
        response.EnsureSuccessStatusCode();
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountDto>>();
        accounts.Should().NotBeNull();
    }
}
```

### Running Tests

```bash
# All tests
cd CRM.Backend
dotnet test

# Specific project
dotnet test tests/CRM.Tests/CRM.Tests.csproj

# With filter
dotnet test --filter "FullyQualifiedName~AccountService"

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Using test script
./tests/run-tests.sh --timeout 300 --verbosity detailed
```

### Test Configuration

**xUnit Configuration:**

```json
// xunit.runner.json
{
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4,
  "methodDisplay": "classAndMethod",
  "diagnosticMessages": false,
  "longRunningTestSeconds": 30
}
```

**Code Coverage:**

```xml
<!-- coverage.runsettings -->
<RunSettings>
  <DataCollectors>
    <DataCollector friendlyName="XPlat Code Coverage">
      <Configuration>
        <Format>opencover</Format>
        <ExcludeByAttribute>Obsolete,GeneratedCodeAttribute</ExcludeByAttribute>
        <ExcludeByFile>**/Migrations/**</ExcludeByFile>
      </Configuration>
    </DataCollector>
  </DataCollectors>
</RunSettings>
```

---

## Common Patterns

### 1. Soft Delete Pattern

All entities use soft delete (never hard delete):

```csharp
public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
{
    var entity = await _context.Accounts.FindAsync(id, cancellationToken);
    if (entity == null) return false;
    
    entity.IsDeleted = true;
    entity.UpdatedAt = DateTime.UtcNow;
    
    await _context.SaveChangesAsync(cancellationToken);
    return true;
}
```

### 2. Audit Tracking Pattern

All entities automatically track `CreatedAt` and `UpdatedAt`:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
{
    var entries = ChangeTracker.Entries<BaseEntity>();
    
    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.CreatedAt = DateTime.UtcNow;
        }
        
        if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
    
    return await base.SaveChangesAsync(cancellationToken);
}
```

### 3. AsNoTracking for Read-Only

Always use `.AsNoTracking()` for read-only queries:

```csharp
public async Task<IEnumerable<AccountDto>> GetAllAsync(CancellationToken cancellationToken)
{
    return await _context.Accounts
        .AsNoTracking()  // Improves performance for read-only
        .Where(a => !a.IsDeleted)
        .Select(a => MapToDto(a))
        .ToListAsync(cancellationToken);
}
```

### 4. CancellationToken Pattern

Always pass `CancellationToken` through async methods:

```csharp
public interface IAccountService
{
    Task<AccountDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

public class AccountService : IAccountService
{
    public async Task<AccountDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);  // Pass token
    }
}
```

### 5. DTO Mapping Pattern

Always map entities to DTOs before returning:

```csharp
private static AccountDto MapToDto(Account entity)
{
    return new AccountDto
    {
        Id = entity.Id,
        Name = entity.Name,
        Email = entity.Email,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
```

**Or use AutoMapper:**

```csharp
services.AddAutoMapper(typeof(MappingProfile));

// Profile
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Account, AccountDto>();
        CreateMap<CreateAccountDto, Account>();
    }
}
```

### 6. FluentValidation Pattern

```csharp
public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
        
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");
    }
}
```

---

## Best Practices

### 1. Dependency Injection

✅ **DO:**
- Register services in `Program.cs` or extension methods
- Use interface-based dependencies
- Use scoped lifetime for services that access DbContext

```csharp
services.AddScoped<IAccountService, AccountService>();
```

❌ **DON'T:**
- Use `new` to instantiate services
- Register DbContext as singleton

### 2. Entity Framework

✅ **DO:**
- Use `.AsNoTracking()` for read-only queries
- Use `.Include()` for eager loading related entities
- Use async methods (`ToListAsync`, `FirstOrDefaultAsync`)
- Pass `CancellationToken` to all async operations

```csharp
var accounts = await _context.Accounts
    .AsNoTracking()
    .Include(a => a.Contacts)
    .Where(a => !a.IsDeleted)
    .ToListAsync(cancellationToken);
```

❌ **DON'T:**
- Track entities when only reading
- Use `.ToList()` (sync) instead of `.ToListAsync()` (async)
- Forget to filter `IsDeleted`

### 3. Error Handling

✅ **DO:**
- Use custom exceptions for business logic errors
- Log exceptions with structured logging
- Return appropriate HTTP status codes

```csharp
try
{
    return await _service.GetByIdAsync(id, cancellationToken);
}
catch (NotFoundException ex)
{
    _logger.LogWarning(ex, "Account {Id} not found", id);
    return NotFound();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error retrieving account {Id}", id);
    return StatusCode(500, "Internal server error");
}
```

### 4. Logging

✅ **DO:**
- Use structured logging with `ILogger<T>`
- Log at appropriate levels (Information, Warning, Error)
- Include context (entity IDs, user IDs)

```csharp
_logger.LogInformation("Creating account: {Name}", dto.Name);
_logger.LogWarning("Account {Id} not found", id);
_logger.LogError(ex, "Failed to create account: {Name}", dto.Name);
```

### 5. API Design

✅ **DO:**
- Use standard HTTP verbs (GET, POST, PUT, DELETE)
- Return appropriate status codes (200, 201, 404, 400, 500)
- Use DTOs for request/response
- Document with XML comments and `[ProducesResponseType]`

```csharp
/// <summary>
/// Creates a new account
/// </summary>
[HttpPost]
[ProducesResponseType(typeof(AccountDto), 201)]
[ProducesResponseType(400)]
public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
{
    var account = await _service.CreateAsync(dto, CancellationToken);
    return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
}
```

---

## Troubleshooting

### Common Issues

#### 1. Database Connection Failed

**Symptom:** "Unable to connect to any of the specified MySQL hosts"

**Solution:**
```bash
# Check if database container is running
docker ps | grep crm-mariadb

# Restart database
docker-compose -f docker/docker-compose.databases.yml restart crm-mariadb

# Check connection string in appsettings.Development.json
```

#### 2. Migration Failed

**Symptom:** "An error occurred while migrating the database"

**Solution:**
```bash
# Drop database and recreate from migrations
docker exec -it crm-mariadb mysql -u root -pRootPass@Dev2024 -e "DROP DATABASE crm_db; CREATE DATABASE crm_db;"

# Reapply migrations
cd CRM.Backend/src/CRM.Api
dotnet ef database update
```

#### 3. JWT Token Invalid

**Symptom:** 401 Unauthorized

**Solution:**
- Ensure `JWT_SECRET` is at least 32 characters
- Check token expiration (`exp` claim)
- Verify `Issuer` and `Audience` match configuration

```json
{
  "Jwt": {
    "Secret": "ThisIsASecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "CRM.Api",
    "Audience": "CRM.Client",
    "ExpirationMinutes": 60
  }
}
```

#### 4. Rate Limiting 429 Errors

**Symptom:** "API calls quota exceeded!"

**Solution:**
```json
// Disable in appsettings.Development.json
{
  "RateLimiting": {
    "EnableEndpointRateLimiting": false
  }
}
```

#### 5. Memory Usage High

**Solution:**
- Use `.AsNoTracking()` for read-only queries
- Dispose DbContext properly (automatic with DI)
- Implement pagination for large datasets

```csharp
var accounts = await _context.Accounts
    .AsNoTracking()
    .OrderBy(a => a.Id)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
```

### Debug Logging

Enable verbose logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

This will log all SQL queries executed by EF Core.

---

## Additional Resources

- **Copilot Instructions:** `.github/copilot-instructions.md`
- **Solution Context:** `docs/development/SOLUTION_CONTEXT.md`
- **Architecture Overview:** `docs/development/ARCHITECTURE_OVERVIEW.md`
- **Database Schema:** `database/DATABASE_SCHEMA.md`
- **Microservices Guide:** `docs/development/MICROSERVICES_ARCHITECTURE.md`
- **Feature Specifications:** `docs/11-specifications/`
- **API Documentation:** Swagger UI at `/swagger`

---

**Document Version:** 1.0  
**Last Updated:** March 3, 2026  
**Maintained By:** CRM Development Team
