# Architecture Specification: Dependency Injection Patterns

> **Spec ID:** SPEC-ARCH-003  
> **Feature:** Dependency Injection (DI) Patterns & Conventions  
> **Module:** Architecture  
> **Version:** 1.0  
> **Last Updated:** February 16, 2026  
> **Status:** ✅ Implemented (Reference Standard)  
> **Priority:** P0 (Foundational)  
> **Author:** Architecture Team  
> **Cross-References:** [SPEC-ARCH-001](SPEC-ARCH-001-DTOStandard.md), [SPEC-ARCH-004](SPEC-ARCH-004-CachingStrategy.md)

---

## Executive Summary

Dependency Injection is **THE mechanism** for managing object lifecycles, decoupling components, and enabling testability in the CRM. Inconsistent DI patterns lead to:
- Memory leaks (Singleton overuse)
- Stale data (Scoped misuse)
- Test failures (Transient confusion)
- Hard-to-debug issues

This specification establishes **ONE STANDARD** for registering services, choosing lifetimes, and organizing DI configuration.

**Key Principle:** "Let the container manage lifecycles; make dependencies explicit."

---

## 1. Business Context

### 1.1 Feature Description

Dependency Injection encompasses:
1. **Service Lifetime Patterns** - Transient, Scoped, Singleton
2. **Extension Methods** - Organized registration by feature
3. **Factory Pattern** - Complex object creation
4. **Decorator Pattern** - Cross-cutting concerns (logging, caching)
5. **Configuration** - Options pattern for settings
6. **Testing** - Mock injection for unit tests

### 1.2 Standards Defined

| Standard | Purpose | Examples |
|----------|---------|----------|
| **Scoped Services** | Per-request lifetime | Services, DbContext, UnitOfWork |
| **Singleton Services** | Application lifetime | Cache, Configuration, LoggerFactory |
| **Transient Services** | New instance each time | DTOs, Commands, Middleware |
| **Extension Methods** | Organized registration | `AddCrmServices()`, `AddSalesServices()` |
| **Factory Pattern** | Complex registration logic | `ProviderFactory` for pluggable providers |
| **Options Pattern** | Type-safe configuration | `IOptions<ServiceSettings>` |

---

## 2. Service Lifetime Patterns

### 2.1 Scoped Lifetime (✅ Default for Business Logic)

**Use for:** Database access, unit of work, services with request-scoped state.

```csharp
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

**Why Scoped:**
- One instance per HTTP request
- DbContext changes are isolated per request
- Changes from one request don't leak to another
- Efficient memory usage
- Perfect for transactional operations

**Lifetime Diagram:**
```
Request 1:  [Service Instance 1] ├─ Scoped
Request 2:  [Service Instance 2] ├─ Scoped
Request 3:  [Service Instance 3] ├─ Scoped
```

**Example:**
```csharp
public class AccountService : IAccountService
{
    private readonly ICrmDbContext _dbContext;
    
    // ✅ CORRECT - Scoped DbContext injected
    public AccountService(ICrmDbContext dbContext)
    {
        _dbContext = dbContext;  // Per-request instance
    }
    
    public async Task<AccountDto> CreateAsync(CreateAccountDto dto)
    {
        // ...changes in this request
        await _dbContext.SaveChangesAsync();  // Commits this request's changes only
    }
}
```

### 2.2 Singleton Lifetime (⚠️ Use Carefully!)

**Use for:** Immutable services, caches, configuration, thread-safe utilities.

```csharp
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<ILogger>(loggerFactory.CreateLogger("CRM"));
```

**Why Singleton:**
- One instance for entire application lifetime
- Minimal memory allocation
- Maximum performance (no re-instantiation)
- Perfect for stateless utilities

**⚠️ DANGER:**
```csharp
// ❌ WRONG - Storing request data in Singleton service
public class BadService : ISingleton
{
    public int UserId { get; set; }  // ❌ Shared across requests!
    // Request 1 sets UserId = 1
    // Request 2 reads UserId = 1 (should be 2!)
}

// ✅ CORRECT - Only immutable data or thread-safe collections
public class GoodService : ISingleton
{
    private readonly IConfiguration _config;  // Immutable
    private readonly ConcurrentDictionary<string, object> _cache;  // Thread-safe
}
```

**Singleton Caching Rule:**
- ✅ Static, unchanging data (configuration, lookup tables)
- ✅ Request-agnostic services (logging, metrics)
- ✅ Thread-safe collections
- ❌ Request-specific data
- ❌ DbContext (must be Scoped)
- ❌ Mutable state

### 2.3 Transient Lifetime (Rarely Used)

**Use for:** DTO factories, temporary workers, stateless utilities.

```csharp
builder.Services.AddTransient<CommandFactory>();
builder.Services.AddTransient<ReportGenerator>();
```

**Why Transient:**
- New instance every time it's requested
- No shared state
- Useful for temporary objects

**Example (Rare):**
```csharp
public class ReportGenerator
{
    // Called in multiple places - each gets fresh instance
    public Report Generate(ReportParameters param) { /* ... */ }
}

// Registration
builder.Services.AddTransient<ReportGenerator>();

// Usage - gets new instance
var report1 = serviceProvider.GetService<ReportGenerator>().Generate(param1);
var report2 = serviceProvider.GetService<ReportGenerator>().Generate(param2);
```

### 2.4 Lifetime Decision Tree

```
Does service maintain state?
├─ YES → Is it request-specific?
│   ├─ YES → SCOPED ✅
│   └─ NO → Is it thread-safe?
│       ├─ YES → SINGLETON ✅
│       └─ NO → ERROR ❌ (Design is wrong)
└─ NO → Is it expensive to create?
    ├─ YES → SINGLETON ✅
    └─ NO → SCOPED (default) or TRANSIENT (rare) ✅
```

---

## 3. Extension Method Organization

### 3.1 Naming Convention

All DI registration methods follow the pattern: **`Add{Feature}Services()`**

```
AddCrmServices()              // Core CRM entities (Accounts, Contacts, etc.)
AddSalesServices()            // Sales module (Invoices, Quotes, Orders)
AddMarketingServices()        // Marketing module (Campaigns, Email)
AddITSMServices()             // Service Desk module
AddAuthenticationServices()   // Authentication & Authorization
AddCachingServices()          // Caching layer (Redis, DbCache)
AddValidationServices()       // Validation (FluentValidation)
AddPluggableProviders()       // Provider factories (Search, Chat, etc.)
```

### 3.2 Creating an Extension Method

**Pattern:**
```csharp
namespace CRM.Infrastructure.DependencyInjection;

/// <summary>
/// Extension method to register Sales module services
/// </summary>
public static class SalesServiceCollectionExtensions
{
    public static IServiceCollection AddSalesServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register services
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        
        // Register validators
        services.AddScoped<IValidator<CreateInvoiceDto>, CreateInvoiceDtoValidator>();
        services.AddScoped<IValidator<CreateQuoteDto>, CreateQuoteDtoValidator>();
        
        // Register repositories
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        
        return services;
    }
}
```

**Usage in Program.cs:**
```csharp
// Add each module's services
builder.Services.AddCrmServices(builder.Configuration);
builder.Services.AddSalesServices(builder.Configuration);
builder.Services.AddMarketingServices(builder.Configuration);
builder.Services.AddITSMServices(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddCachingServices(builder.Configuration);
builder.Services.AddValidationServices(builder.Configuration);
builder.Services.AddPluggableProviders(builder.Configuration);
```

### 3.3 Real CRM Example

**CrmServiceCollectionExtensions.cs:**
```csharp
public static class CrmServiceCollectionExtensions
{
    public static IServiceCollection AddCrmServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Core CRM Services
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ILeadService, LeadService>();
        
        // Core Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        
        // Core Validators
        services.AddScoped<IValidator<CreateAccountDto>, CreateAccountDtoValidator>();
        services.AddScoped<IValidator<UpdateAccountDto>, UpdateAccountDtoValidator>();
        services.AddScoped<IValidator<CreateContactDto>, CreateContactDtoValidator>();
        
        // Database context
        services.AddScoped<ICrmDbContext, CrmDbContext>();
        
        return services;
    }
}
```

---

## 4. Factory Pattern for Complex Registration

### 4.1 Provider Factory Example

```csharp
namespace CRM.Infrastructure.Factories;

/// <summary>
/// Factory for creating pluggable providers at runtime
/// Enables feature-flag driven provider selection
/// </summary>
public class SearchProviderFactory
{
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<SearchProviderFactory> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SearchProviderFactory(
        IFeatureManager featureManager,
        ILogger<SearchProviderFactory> logger,
        IServiceProvider serviceProvider)
    {
        _featureManager = featureManager;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates the appropriate Search provider based on configuration
    /// </summary>
    public async Task<ISearchPort> CreateAsync()
    {
        // Check feature flag
        var useExternal = await _featureManager.IsEnabledAsync("UseExternalSearch");
        
        if (useExternal)
        {
            _logger.LogInformation("Creating external search provider (Meilisearch)");
            return _serviceProvider.GetRequiredService<MeilisearchSearchProvider>();
        }
        else
        {
            _logger.LogInformation("Creating built-in search provider");
            return _serviceProvider.GetRequiredService<BuiltInSearchProvider>();
        }
    }
}
```

**Registration:**
```csharp
services.AddScoped<SearchProviderFactory>();
services.AddScoped<BuiltInSearchProvider>();
services.AddScoped<MeilisearchSearchProvider>();
services.AddScoped<ISearchPort>(sp => sp.GetRequiredService<SearchProviderFactory>().CreateAsync().Result);
```

### 4.2 Conditional Service Registration

```csharp
public static IServiceCollection AddCachingServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var cacheConfig = configuration.GetSection("Caching");
    var cacheType = cacheConfig.GetValue<string>("Type", "Memory"); // Memory|Redis
    
    if (cacheType == "Redis")
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = cacheConfig.GetValue<string>("ConnectionString");
        });
        services.AddSingleton<ICacheService, RedisCacheService>();
        _logger.LogInformation("Configured Redis caching");
    }
    else
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        _logger.LogInformation("Configured in-memory caching");
    }
    
    return services;
}
```

---

## 5. Decorator Pattern for Cross-Cutting Concerns

### 5.1 Logging Decorator Example

```csharp
/// <summary>
/// Decorator that adds logging to service methods
/// Usage: Wrap services with logging behavior without modifying originals
/// </summary>
public class LoggingDecorator<T> : IAccountService
    where T : IAccountService
{
    private readonly T _inner;
    private readonly ILogger<LoggingDecorator<T>> _logger;

    public LoggingDecorator(T inner, ILogger<LoggingDecorator<T>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<AccountDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting account {AccountId}", id);
        try
        {
            var result = await _inner.GetByIdAsync(id, ct);
            _logger.LogInformation("Successfully retrieved account {AccountId}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {AccountId}", id);
            throw;
        }
    }

    // Other methods...
}
```

**Registration:**
```csharp
services.AddScoped<AccountService>();
services.AddScoped<IAccountService>(sp =>
{
    var service = sp.GetRequiredService<AccountService>();
    var logger = sp.GetRequiredService<ILogger<LoggingDecorator<AccountService>>>();
    return new LoggingDecorator<AccountService>(service, logger);
});
```

### 5.2 Caching Decorator Example

```csharp
/// <summary>
/// Decorator that adds caching to read operations
/// </summary>
public class CachingDecorator<T> : IProductService
    where T : IProductService
{
    private readonly T _inner;
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string CacheKeyPattern = "products:";

    public CachingDecorator(T inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var cacheKey = $"{CacheKeyPattern}{id}";
        
        // Try cache first
        var cached = await _cache.GetStringAsync(cacheKey, token: ct);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<ProductDto>(cached, _jsonOptions)!;
        }
        
        // Get from service
        var result = await _inner.GetByIdAsync(id, ct);
        
        // Store in cache for 1 hour
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result, _jsonOptions),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) },
            token: ct);
        
        return result;
    }
}
```

---

## 6. Options Pattern for Configuration

### 6.1 Creating Options Classes

```csharp
namespace CRM.Core.Options;

/// <summary>
/// Options for JWT token configuration
/// Injected via: IOptions<JwtOptions>
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";
    
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CRM.Api";
    public string Audience { get; set; } = "CRM.Client";
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshExpirationDays { get; set; } = 7;
    public string Algorithm { get; set; } = "HS256";
}

/// <summary>
/// Options for Redis caching
/// </summary>
public class RedisCacheOptions
{
    public const string SectionName = "Redis";
    
    public bool Enabled { get; set; } = true;
    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "crm_";
    public int DefaultExpirationMinutes { get; set; } = 60;
}
```

### 6.2 Registering Options

```csharp
// In Program.cs
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);

var redisSection = builder.Configuration.GetSection(RedisCacheOptions.SectionName);
builder.Services.Configure<RedisCacheOptions>(redisSection);
```

### 6.3 Consuming Options

```csharp
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    // ✅ CORRECT - Inject IOptions<T>
    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;  // Extract the actual options
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(key, _jwtOptions.Algorithm);
        
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

---

## 7. Generic Service Registration

### 7.1 Generic Repository Pattern

```csharp
// ✅ CORRECT - Generic registration for all entities
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Now IRepository<Account>, IRepository<Contact>, etc. all work automatically
public class AccountService
{
    private readonly IRepository<Account> _accountRepository;
    
    public AccountService(IRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;  // Resolves IRepository<Account>
    }
}

public class ContactService
{
    private readonly IRepository<Contact> _contactRepository;
    
    public ContactService(IRepository<Contact> contactRepository)
    {
        _contactRepository = contactRepository;  // Resolves IRepository<Contact>
    }
}
```

### 7.2 Generic Validator Registration

```csharp
// Register all validators from assembly
services.AddValidatorsFromAssemblyContaining<CreateAccountDtoValidator>(
    includeInternalTypes: true);

//  Now all IValidator<T> implementations are registered
// IValidator<CreateAccountDto>, IValidator<CreateContactDto>, etc.
```

---

## 8. Real CRM DI Configuration

**Full Program.cs Example:**

```csharp
// Feature Management
builder.Services.AddFeatureManagement(builder.Configuration.GetSection("FeatureManagement"));

// Authentication
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Caching
var redisEnabled = builder.Configuration.GetValue("Redis:Enabled", true);
if (redisEnabled)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetValue<string>("Redis:ConnectionString");
        options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName");
    });
    builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
}
else
{
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();  // Memory fallback
}

// Database
builder.Services.AddScoped<ICrmDbContext, CrmDbContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services - organized by module
builder.Services.AddCrmServices(builder.Configuration);
builder.Services.AddSalesServices(builder.Configuration);
builder.Services.AddMarketingServices(builder.Configuration);
builder.Services.AddITSMServices(builder.Configuration);

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<CreateAccountDtoValidator>();

// Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Information);
});

// HttpClients for external services
builder.Services.AddHttpClient<IPaymentGateway, PaymentGateway>();
builder.Services.AddHttpClient<IEmailService, EmailService>();

// Pluggable Providers
builder.Services.AddPluggableProviders(builder.Configuration);
```

---

## 9. Anti-Patterns (What NOT to Do)

### ❌ Anti-Pattern 1: DbContext as Singleton
```csharp
// ❌ WRONG - DbContext MUST be Scoped
services.AddSingleton<CrmDbContext>();

// ❌ WRONG - Changes from request 1 visible in request 2
public void Method1(CrmDbContext db) { /* Request 1 */ }
public void Method2(CrmDbContext db) { /* Request 2 sees Request 1's changes! */ }

// ✅ CORRECT - Scoped per request
services.AddScoped<CrmDbContext>();
```

### ❌ Anti-Pattern 2: Service with Request State in Singleton
```csharp
// ❌ WRONG - Stores request-specific data
public class SingletonService
{
    public int UserId { get; set; }  // Shared across requests!
    public string CurrentTenant { get; set; }  // Shared across requests!
}

// Request 1: service.UserId = 1
// Request 2: service.UserId = 2
// Request 3: await Task.Delay(500); Assert(service.UserId == 3);  // FAILS!

// ✅ CORRECT - Request-specific data in Scoped service
public class ScopedService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public int UserId => _httpContextAccessor.HttpContext?.User.GetUserId() ?? 0;
    public string CurrentTenant => _httpContextAccessor.HttpContext?.GetTenant() ?? "";
}
```

### ❌ Anti-Pattern 3: ServiceLocator Pattern
```csharp
// ❌ WRONG - Using ServiceLocator anti-pattern
public class AccountService
{
    public AccountService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;  // ❌ Anti-pattern
    }
    
    public async Task<ContactDto> GetContactAsync(int id)
    {
        // ❌ Hiding dependencies in method
        var contactService = _serviceProvider.GetService<IContactService>();
        return await contactService.GetByIdAsync(id);
    }
}

// ✅ CORRECT - Explicit constructor dependency
public class AccountService
{
    private readonly IContactService _contactService;
    
    public AccountService(IContactService contactService)  // ✅ Explicit
    {
        _contactService = contactService;
    }
    
    public async Task<ContactDto> GetContactAsync(int id)
    {
        return await _contactService.GetByIdAsync(id);  // ✅ Clear dependency
    }
}
```

### ❌ Anti-Pattern 4: Circular Dependency
```csharp
// ❌ WRONG - Circular dependency
public interface IAccountService { Task<ContactDto> GetContactAsync(int id); }
public interface IContactService { Task<AccountDto> GetAccountAsync(int id); }

public class AccountService
{
    public AccountService(IContactService contactService) { }  // ❌ Circular!
}

public class ContactService
{
    public ContactService(IAccountService accountService) { }  // ❌ Circular!
}

// ✅ CORRECT - Extract common dependency
public interface IRelationshipService
{
    Task<ContactDto> GetRelatedContactAsync(int accountId);
    Task<AccountDto> GetRelatedAccountAsync(int contactId);
}

public class AccountService
{
    public AccountService(IRelationshipService relationshipService) { }  // ✅ No cycle
}
```

### ❌ Anti-Pattern 5: Not Using Configuration Options
```csharp
// ❌ WRONG - Hardcoded configuration
public class JwtTokenService
{
    private const string Secret = "my-secret-key";  // ❌ Hardcoded!
    private const int ExpirationMinutes = 60;
}

// ✅ CORRECT - Inject configuration
public class JwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    
    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;  // ✅ From appsettings.json
    }
}
```

---

## 10. Testing with DI

### 10.1 Mocking in Unit Tests

```csharp
[TestFixture]
public class AccountServiceTests
{
    private Mock<IRepository<Account>> _mockRepository;
    private Mock<IValidator<CreateAccountDto>> _mockValidator;
    private Mock<IMapper> _mockMapper;
    private AccountService _service;

    [SetUp]
    public void Setup()
    {
        // Arrange - Create mocks
        _mockRepository = new Mock<IRepository<Account>>();
        _mockValidator = new Mock<IValidator<CreateAccountDto>>();
        _mockMapper = new Mock<IMapper>();

        // Act - Inject mocks into service
        _service = new AccountService(
            _mockRepository.Object,
            _mockValidator.Object,
            _mockMapper.Object);
    }

    [Test]
    public async Task CreateAsync_ShouldCreateAccount_WhenValidInputProvided()
    {
        // Arrange
        var dto = new CreateAccountDto { AccountName = "Test Co" };
        var entity = new Account { Id = 1, AccountName = "Test Co" };
        var response = new AccountDto { Id = 1, AccountName = "Test Co" };

        _mockValidator.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());  // Valid
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Account>(), default))
            .Returns(Task.CompletedTask);
        _mockMapper.Setup(x => x.Map<AccountDto>(It.IsAny<Account>()))
            .Returns(response);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.That(result.Id, Is.EqualTo(1));
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Account>(), default), Times.Once);
    }
}
```

---

## 11. Implementation Checklist

- [ ] All services registered in extension methods
- [ ] Extension methods follow `Add{Feature}Services()` naming
- [ ] Correct lifetime chosen for each service (Scoped > Singleton > Transient)
- [ ] DbContext registered as Scoped
- [ ] Repositories registered as Scoped
- [ ] Cache services registered as Singleton
- [ ] All interfaces/implementations registered
- [ ] Generic registrations for generic services
- [ ] Validators registered (FluentValidation)
- [ ] Options classes created and registered
- [ ] Factories implemented for complex registration
- [ ] Decorators used for cross-cutting concerns
- [ ] No circular dependencies
- [ ] No ServiceLocator anti-pattern
- [ ] Unit tests use constructor injection of mocks
- [ ] Integration tests use DI container properly

---

## 12. Existing Code Compliance

The CRM solution **already implements** this DI pattern:

**Implemented:**
- ✅ Multiple extension methods (`AddCrmServices`, `AddSalesServices`, `AddPluggableProviders`)
- ✅ Scoped DbContext and repositories
- ✅ Singleton cache services (Redis, memory)
- ✅ Options pattern for JWT, Redis, Feature Management
- ✅ Factory pattern for providers
- ✅ Generic IRepository<T> registration

**To Evolve:**
- [ ] Verify all services registered in extension methods
- [ ] Document DI configuration per module
- [ ] Add decorators for logging/caching patterns

---

## 13. TODO Items

| TODO ID | Description | Priority |
|---------|-------------|----------|
| TODO-ARCH-003-001 | Audit all service lifetimes for correctness | P1 |
| TODO-ARCH-003-002 | Document DI configuration per feature | P2 |
| TODO-ARCH-003-003 | Add decorators for logging/performance | P2 |
| TODO-ARCH-003-004 | Remove ServiceLocator anti-patterns if any | P1 |
| TODO-ARCH-003-005 | Ensure all factories follow pattern | P1 |

---

## Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Feb 16, 2026 | Architecture Team | Initial specification from CRM DI configuration |

---

## Related Specifications

- **[SPEC-ARCH-001: DTO Standardization](SPEC-ARCH-001-DTOStandard.md)** - Validators injected via DI
- **[SPEC-ARCH-004: Caching Strategy](SPEC-ARCH-004-CachingStrategy.md)** - Cache services registered
- **[SPEC-ARCH-005: Validation Framework](SPEC-ARCH-005-ValidationFramework.md)** - Validators in DI

---

**END OF SPECIFICATION**
