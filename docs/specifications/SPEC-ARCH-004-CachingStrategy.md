# Architecture Specification: Caching Strategy

> **Spec ID:** SPEC-ARCH-004  
> **Feature:** Caching Architecture & Patterns  
> **Module:** Architecture  
> **Version:** 1.0  
> **Last Updated:** February 16, 2026  
> **Status:** ✅ Implemented (Reference Standard)  
> **Priority:** P1 (Performance Critical)  
> **Author:** Architecture Team  
> **Cross-References:** [SPEC-ARCH-003](SPEC-ARCH-003-DependencyInjectionPatterns.md) (DI), [SPEC-SYS-011](SPEC-SYS-011-NonFunctionalRequirements.md) (Performance)

---

## Executive Summary

Caching is **critical for CRM performance**. Without proper caching strategy:
- Database queries multiply (N+1 problems)
- Response times exceed SLAs (P95 > 500ms)
- User experience degrades under load
- API costs increase 10-100x

This specification establishes **ONE STANDARD** for when, where, and how to cache in the CRM.

**Key Principle:** "Cache strategically, invalidate religiously."

---

## 1. Business Context

### 1.1 Feature Description

Caching encompasses:
1. **Distributed Cache** (Redis) - Shared across all API instances
2. **Database Cache** (DbCacheService) - Static/Reference data
3. **In-Memory Cache** - Request-specific or local caching
4. **Cache Invalidation** - Event-based, time-based, manual
5. **TTL Management** - Per-entity-type expiration rules
6. **Cache Monitoring** - Hit/miss ratios, memory usage

### 1.2 Standards Defined

| Standard | Purpose | Implementation |
|----------|---------|-----------------|
| **Cache Layers** | Multi-layer caching strategy | Redis > DbCache > Memory |
| **TTL by Type** | Expiration rules per entity | Settings: ∞, Products: 1h, Leads: 30m |
| **Cache Keys** | Consistent key naming | Entity:Type:Id or Entity:Type:Filter |
| **Invalidation** | Event-driven invalidation | On create/update/delete |
| **Fallback** | Handle cache failures | Read from DB if cache missed |
| **Monitoring** | Track cache effectiveness | Hit ratio, memory usage |

---

## 2. Caching Architecture

### 2.1 Three-Layer Caching Strategy

```
┌─────────────────────────┐
│  In-Memory Cache (IMemoryCache)
│  - Per-request data
│  - View models
│  - Temporary results
│  TTL: 30 seconds
└─────────────────────────┘
           │
           ▼
┌─────────────────────────┐
│  Distributed Cache (Redis)
│  - Entities
│  - User permissions
│  - Configuration
│  TTL: 1 hour (typical)
└─────────────────────────┘
           │
           ▼
┌─────────────────────────┐
│  Database Cache (DbCacheService)
│  - Reference data
│  - System settings
│  - Static lookups
│  TTL: Application lifetime
└─────────────────────────┘
           │
           ▼
┌─────────────────────────┐
│  Database (CrmDbContext)
│  - Source of truth
│  - Transactional data
└─────────────────────────┘
```

### 2.2 Cache Layer Overview

#### Layer 1: In-Memory Cache (IMemoryCache)
**Purpose:** Speed up repeated requests in same request cycle
**Use For:** Request-scoped data, view models
**Lifetime:** Single request (~30 sec)
**Size:** Small (< 10MB)

```csharp
public class ReportService
{
    private readonly IMemoryCache _memoryCache;
    
    public async Task<ReportDto> GenerateAsync(ReportParameters params)
    {
        var cacheKey = $"report:{params.GetHashCode()}";
        
        if (_memoryCache.TryGetValue(cacheKey, out ReportDto? cached))
        {
            return cached!;  // Return from memory cache
        }
        
        var result = await _generateReport(params);  // Generate
        _memoryCache.Set(cacheKey, result, TimeSpan.FromSeconds(30));
        return result;
    }
}
```

#### Layer 2: Distributed Cache (Redis)
**Purpose:** Share cached data across all API instances
**Use For:** Entities, permissions, user data
**Lifetime:** 15 minutes to 24 hours
**Size:** Shared across all servers

```csharp
public class AccountService
{
    private readonly IDistributedCache _cache;
    private readonly IRepository<Account> _repository;
    
    public async Task<AccountDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var cacheKey = $"account:{id}";
        
        // Try distributed cache first
        var cached = await _cache.GetStringAsync(cacheKey, token: ct);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<AccountDto>(cached)!;  // From Redis
        }
        
        // Cache miss - read from DB
        var account = await _repository.GetByIdAsync(id, ct);
        if (account == null)
            throw new EntityNotFoundException("Account", id);
        
        var dto = _mapper.Map<AccountDto>(account);
        
        // Store in cache for 1 hour
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            },
            token: ct);
        
        return dto;
    }
}
```

#### Layer 3: Database Cache (DbCacheService)
**Purpose:** Cache static reference data for app lifetime
**Use For:** System settings, lookup tables, products
**Lifetime:** Application startup to shutdown
**Size:** Small, manually managed

```csharp
public class ProductService
{
    private readonly IDbCacheService _dbCache;
    
    public async Task<List<ProductDto>> GetAllAsync(CancellationToken ct)
    {
        // DbCache loads once and never reloads (unless manually cleared)
        var products = await _dbCache.GetAllProductsAsync(ct);
        return _mapper.Map<List<ProductDto>>(products);
    }
}
```

---

## 3. Cache Key Naming Convention

### 3.1 Standard Patterns

**Single Entity:**
```
{EntityType}:{Id}
account:123
contact:456
invoice:789
```

**List/Collection:**
```
{EntityType}:list:{Criteria}
accounts:list:all
accounts:list:active
contacts:list:by-account:123
```

**With Parameters:**
```
{EntityType}:{Action}:{Param1}:{Param2}
invoice:summary:account:123
report:monthly:department:sales
```

**Permissions/User Data:**
```
user:{UserId}:permissions
user:{UserId}:roles
user:{UserId}:groups
tenant:{TenantId}:settings
```

### 3.2 Real Examples

```csharp
public class CacheKeys
{
    // Accounts
    public const string AccountPrefix = "account:";
    public static string Account(int id) => $"{AccountPrefix}{id}";
    public static string AccountsByStatus(string status) => $"{AccountPrefix}list:{status}";
    public static string AccountPermissions(int accountId) => $"{AccountPrefix}{accountId}:permissions";

    // Contacts
    public const string ContactPrefix = "contact:";
    public static string Contact(int id) => $"{ContactPrefix}{id}";
    public static string ContactsByAccount(int accountId) => $"{ContactPrefix}list:account:{accountId}";

    // Invoices
    public const string InvoicePrefix = "invoice:";
    public static string Invoice(int id) => $"{InvoicePrefix}{id}";
    public static string InvoicesByAccount(int accountId) => $"{InvoicePrefix}list:account:{accountId}";
    public static string InvoicesByStatus(string status) => $"{InvoicePrefix}list:{status}";

    // User & Auth
    public static string UserPermissions(int userId) => $"user:{userId}:permissions";
    public static string UserRoles(int userId) => $"user:{userId}:roles";

    // System
    public const string SystemSettingsPrefix = "settings:";
    public static string SystemSetting(string key) => $"{SystemSettingsPrefix}{key}";
}
```

---

## 4. TTL Guidelines by Entity Type

### 4.1 TTL Decision Table

| Entity Type | TTL | Reason | Invalidation |
|-------------|-----|--------|--------------|
| **System Settings** | ∞ (App lifetime) | Static, never change in normal ops | Manual only |
| **Products** | 1 day | Master data, changes rare | On update or manual clear |
| **Accounts** | 1 hour | Core entity, moderate changes | On create/update/delete |
| **Contacts** | 1 hour | Core entity, moderate changes | On create/update/delete |
| **Invoices** | 30 minutes | Transactional, frequent updates | On create/update/delete/status |
| **Permissions** | 15 minutes | Security-critical, frequent changes | On role/permission update |
| **User Roles** | 15 minutes | Security-critical | On role assignment |
| **Sessions** | 30 minutes | Security, expiration-based | On logout or timeout |
| **Reports** | 5 minutes | Frequently regenerated | Time-based only |
| **Search Results** | 2 minutes | User-specific, volatile | On entity update or time |
| **Feature Flags** | Not cached | Runtime decisions | Apply immediately |
| **Authentication Tokens** | 60 minutes | JWT expiration | Token lifetime |

### 4.2 Configuration Example

```csharp
public class CachingOptions
{
    public int SystemSettingsMinutes { get; set; } = int.MaxValue;  // App lifetime
    public int ProductsMinutes { get; set; } = 1440;  // 1 day
    public int AccountsMinutes { get; set; } = 60;    // 1 hour
    public int ContactsMinutes { get; set; } = 60;    // 1 hour
    public int InvoicesMinutes { get; set; } = 30;    // 30 minutes
    public int PermissionsMinutes { get; set; } = 15; // 15 minutes
    public int ReportsMinutes { get; set; } = 5;      // 5 minutes
}

// In appsettings.json
{
  "Caching": {
    "SystemSettingsMinutes": 2147483647,
    "ProductsMinutes": 1440,
    "AccountsMinutes": 60,
    "ContactsMinutes": 60,
    "InvoicesMinutes": 30,
    "PermissionsMinutes": 15,
    "ReportsMinutes": 5
  }
}
```

---

## 5. Cache Invalidation Strategies

### 5.1 Time-Based Invalidation

**Pattern:** Automatic expiration after TTL

```csharp
// AutoExpire after 1 hour
await _cache.SetStringAsync(
    key,
    value,
    new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    },
    token: ct);
```

**Best For:** Data that naturally becomes stale (reports, search results)

### 5.2 Event-Based Invalidation

**Pattern:** Clear cache when entity changes

```csharp
public class AccountService
{
    private readonly IDistributedCache _cache;
    
    public async Task<AccountDto> UpdateAsync(int id, UpdateAccountDto dto, CancellationToken ct)
    {
        // Update database
        var account = await _repository.GetByIdAsync(id, ct);
        _mapper.Map(dto, account);
        await _repository.UpdateAsync(account, ct);
        
        // ✅ Invalidate cache immediately
        await _cache.RemoveAsync(CacheKeys.Account(id), token: ct);
        
        // Invalidate list caches too
        await InvalidateAccountListCachesAsync(account, ct);
        
        return _mapper.Map<AccountDto>(account);
    }
    
    private async Task InvalidateAccountListCachesAsync(Account account, CancellationToken ct)
    {
        // Invalidate related caches
        var keysToInvalidate = new[]
        {
            CacheKeys.AccountsByStatus(account.Status),
            CacheKeys.ContactsByAccount(account.Id),
            CacheKeys.InvoicesByAccount(account.Id)
        };
        
        foreach (var key in keysToInvalidate)
        {
            await _cache.RemoveAsync(key, token: ct);
        }
    }
}
```

**Best For:** Master data that must be consistent

### 5.3 Manual Invalidation

**Pattern:** Admin manually clears cache

```csharp
public interface IAdminCacheService
{
    /// <summary>
    /// Clear all cache for a specific entity
    /// </summary>
    Task ClearEntityCacheAsync(string entityType, int? entityId = null, CancellationToken ct = default);
    
    /// <summary>
    /// Clear all caches for a user (permissions, roles, settings)
    /// </summary>
    Task ClearUserCacheAsync(int userId, CancellationToken ct = default);
    
    /// <summary>
    /// Clear entire cache (use sparingly)
    /// </summary>
    Task ClearAllCacheAsync(CancellationToken ct = default);
}

public class AdminCacheService : IAdminCacheService
{
    private readonly IDistributedCache _cache;
    
    public async Task ClearEntityCacheAsync(string entityType, int? entityId = null, CancellationToken ct = default)
    {
        if (entityId.HasValue)
        {
            await _cache.RemoveAsync($"{entityType}:{entityId}", token: ct);
        }
        else
        {
            // Clear all for type (pattern matching)
            // Note: Redis doesn't support pattern deletion, so store list of keys
            var pattern = $"{entityType}:*";
            // Implementation depends on Redis library used
        }
    }
    
    public async Task ClearUserCacheAsync(int userId, CancellationToken ct = default)
    {
        var keysToInvalidate = new[]
        {
            CacheKeys.UserPermissions(userId),
            CacheKeys.UserRoles(userId)
        };
        
        foreach (var key in keysToInvalidate)
        {
            await _cache.RemoveAsync(key, token: ct);
        }
    }
}
```

**Best For:** Emergency fixes, admin operations

### 5.4 Permission Change Invalidation

**Critical:** Always invalidate user caches when permissions change

```csharp
public class RoleService
{
    private readonly IDistributedCache _cache;
    
    public async Task AssignToUserAsync(int roleId, int userId, CancellationToken ct)
    {
        await _repository.AssignAsync(roleId, userId, ct);
        
        // ✅ CRITICAL: Invalidate user's permission cache
        await _cache.RemoveAsync(CacheKeys.UserPermissions(userId), token: ct);
        await _cache.RemoveAsync(CacheKeys.UserRoles(userId), token: ct);
    }
}
```

---

## 6. DbCacheService Pattern

### 6.1 Overview

DbCacheService caches **static reference data** loaded once at startup:

```csharp
namespace CRM.Infrastructure.Services;

/// <summary>
/// Caches static reference data for application lifetime
/// Data loaded once and never reloaded (unless manually cleared)
/// Used for: Products, Settings, Lookup tables
/// </summary>
public interface IDbCacheService
{
    // Products
    Task<List<Product>> GetAllProductsAsync(CancellationToken ct = default);
    Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default);
    
    // System Settings
    Task<SystemSetting?> GetSettingAsync(string key, CancellationToken ct = default);
    Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken ct = default);
    
    // Invalidation
    Task InvalidateAsync(string entityType, int? entityId = null, CancellationToken ct = default);
    Task WarmupCacheAsync(CancellationToken ct = default);
}

public class DbCacheService : IDbCacheService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<DbCacheService> _logger;
    private Dictionary<string, Product> _products = new();
    private Dictionary<string, SystemSetting> _settings = new();

    public async Task<List<Product>> GetAllProductsAsync(CancellationToken ct = default)
    {
        // First call loads from DB
        if (_products.Count == 0)
        {
            var products = await _dbContext.Products
                .Where(p => !p.IsDeleted)
                .ToListAsync(ct);
            
            _products = products.ToDictionary(p => p.Id.ToString(), p => p);
            _logger.LogInformation("Loaded {Count} products into cache", _products.Count);
        }
        
        return _products.Values.ToList();
    }

    public async Task WarmupCacheAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Warming up DbCache...");
        await GetAllProductsAsync(ct);
        await GetAllSettingsAsync(ct);
        _logger.LogInformation("DbCache warmed up");
    }

    public async Task InvalidateAsync(string entityType, int? entityId = null, CancellationToken ct = default)
    {
        switch (entityType)
        {
            case "Product":
                _products.Clear();
                await GetAllProductsAsync(ct);  // Reload
                break;
            case "Setting":
                _settings.Clear();
                await GetAllSettingsAsync(ct);  // Reload
                break;
        }
    }
}
```

### 6.2 Usage

```csharp
public class ProductService
{
    private readonly IDbCacheService _dbCache;
    
    public async Task<List<ProductDto>> GetAllAsync(CancellationToken ct)
    {
        // Cached in memory after first call
        var products = await _dbCache.GetAllProductsAsync(ct);
        return _mapper.Map<List<ProductDto>>(products);
    }
}
```

---

## 7. Performance Monitoring

### 7.1 Cache Hit/Miss Tracking

```csharp
public class CacheMonitoringService
{
    private long _hits = 0;
    private long _misses = 0;

    public void RecordHit() => Interlocked.Increment(ref _hits);
    public void RecordMiss() => Interlocked.Increment(ref _misses);

    public double GetHitRatio()
    {
        long total = _hits + _misses;
        if (total == 0) return 0;
        return (double)_hits / total;
    }

    public (long Hits, long Misses, double Ratio) GetStatistics()
    {
        return (_hits, _misses, GetHitRatio());
    }
}
```

### 7.2 Endpoint for Cache Statistics

```csharp
[HttpGet("admin/cache/statistics")]
[Authorize("Admin")]
public async Task<ActionResult<CacheStatisticsDto>> GetCacheStatistics(CancellationToken ct)
{
    var stats = await _monitoringService.GetCacheStatisticsAsync(ct);
    return Ok(new ApiResponse<CacheStatisticsDto> { Data = stats });
}
```

---

## 8. anti-Patterns (What NOT to Do)

### ❌ Anti-Pattern 1: Cache Stampede
```csharp
// ❌ WRONG - All requests try to recreate cache simultaneously
public async Task<DataDto> GetDataAsync(int id)
{
    var cached = await _cache.GetAsync(key);
    if (cached == null)
    {
        // 1000 concurrent requests all reach here at same time!
        var data = await _expensiveOperation();  // Thundering herd
        await _cache.SetAsync(key, data, ...);
    }
    return cached;
}

// ✅ CORRECT - Lock to prevent stampede
private readonly SemaphoreSlim _lock = new(1);

public async Task<DataDto> GetDataAsync(int id)
{
    var cached = await _cache.GetAsync(key);
    if (cached != null) return cached;
    
    using (await _lock.WaitAsync())
    {
        // Double-check after acquiring lock
        cached = await _cache.GetAsync(key);
        if (cached != null) return cached;
        
        // Now safe - only one request does the expensive operation
        var data = await _expensiveOperation();
        await _cache.SetAsync(key, data, ...);
        return data;
    }
}
```

### ❌ Anti-Pattern 2: Cache Coherence Violations
```csharp
// ❌ WRONG - Updating database but forgetting to invalidate cache
public async Task UpdateAsync(Account account)
{
    await _dbContext.SaveChangesAsync();  // Updated DB
    // ❌ FORGOT TO INVALIDATE CACHE!
    // Now cache has stale data
}

// ✅ CORRECT - Always invalidate cache after updates
public async Task UpdateAsync(Account account)
{
    await _dbContext.SaveChangesAsync();
    
    // ✅ Immediately invalidate cache
    await _cache.RemoveAsync(CacheKeys.Account(account.Id));
}
```

### ❌ Anti-Pattern 3: Caching Mutable Objects
```csharp
// ❌ WRONG - Object cached and then mutated
var cachedList = await cache.GetAsync<List<Account>>(key);
cachedList.RemoveAt(0);  // ❌ Mutates cached data!

// ✅ CORRECT - Return immutable copy
var cachedList = await cache.GetAsync<List<Account>>(key);
var copy = new List<Account>(cachedList);  // ✅ Copy before mutating
copy.RemoveAt(0);
```

### ❌ Anti-Pattern 4: Caching Too Much
```csharp
// ❌ WRONG - Caching everything indefinitely
await _cache.SetAsync(key, largeObject);  // ❌ No TTL, memory leak risk

// ✅ CORRECT - Set appropriate TTL
await _cache.SetAsync(
    key,
    largeObject,
    new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)  // ✅ Must have TTL
    });
```

### ❌ Anti-Pattern 5: Caching Sensitive Data
```csharp
// ❌ WRONG - Caching passwords, PII
await _cache.SetAsync(
    $"user:{userId}",
    new { UserId = userId, Password = hash, SSN = "xxx-xx-xxxx" },  // ❌ PII!
    ...);

// ✅ CORRECT - Cache only necessary, non-sensitive data
await _cache.SetAsync(
    $"user:{userId}:profile",
    new { UserId = userId, DisplayName = user.DisplayName, Email = user.Email },
    ...);
```

---

## 9. Implementation Checklist

- [ ] Redis configured and tested
- [ ] IDistributedCache injected in services needing to cache
- [ ] Cache key naming follows convention
- [ ] TTL configured per entity type
- [ ] Cache invalidation implemented on create/update/delete
- [ ] Permission cache cleared on role/permission changes
- [ ] No cache coherence violations (update without invalidate)
- [ ] Cache stampede prevention implemented (if high traffic)
- [ ] Monitoring/logging of cache operations
- [ ] Admin endpoints for cache management
- [ ] Fallback implemented if cache unavailable
- [ ] No caching of mutable objects
- [ ] No caching of sensitive data (passwords, PII)
- [ ] Performance tests verify cache effectiveness
- [ ] Documentation of cached entities and TTL

---

## 10. Existing Code Compliance

The CRM solution **already implements** comprehensive caching:

**Implemented:**
- ✅ Redis caching layer (distributed)
- ✅ DbCacheService (static reference data)
- ✅ Multiple cache invalidation strategies
- ✅ Cache monitoring and statistics
- ✅ Admin cache management endpoints
- ✅ Proper TTL configuration per entity

**To Evolve:**
- [ ] Verify all update operations invalidate cache
- [ ] Check for cache stampede in high-load scenarios
- [ ] Ensure sensitive data not cached

---

## 11. TODO Items

| TODO ID | Description | Priority |
|---------|-------------|----------|
| TODO-ARCH-004-001 | Implement cache stampede prevention for high-load operations | P2 |
| TODO-ARCH-004-002 | Add distributed tracing for cache operations | P3 |
| TODO-ARCH-004-003 | Create cache warming strategy documentation | P2 |
| TODO-ARCH-004-004 | Audit all cached data for sensitive information | P1 |

---

## Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Feb 16, 2026 | Architecture Team | Initial specification from CRM caching implementation |

---

## Related Specifications

- **[SPEC-ARCH-003: Dependency Injection](SPEC-ARCH-003-DependencyInjectionPatterns.md)** - Cache services registered
- **[SPEC-SYS-011: Non-Functional Requirements](SPEC-SYS-011-NonFunctionalRequirements.md)** - Performance SLAs

---

**END OF SPECIFICATION**
