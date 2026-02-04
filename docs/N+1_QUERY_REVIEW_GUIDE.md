# N+1 Query Review and Optimization Guide

**Status**: Medium Priority  
**Category**: Performance Optimization  
**Last Updated**: 2024 (Session 13 - Medium/Low Priority Fixes)

## Overview

N+1 query problems occur when an application executes one query to fetch the primary records and then executes additional queries (N queries) to fetch related data for each of those records. This is a common performance anti-pattern in ORMs like Entity Framework Core.

## The N+1 Problem Explained

### Example of N+1 Query Problem

```csharp
// BAD: N+1 Query Problem
var accounts = await _context.Customers.ToListAsync();  // 1 query
foreach (var account in accounts)
{
    var contacts = account.AccountContacts;  // N queries (one per account)
    // Process contacts...
}
```

**Result**: If there are 100 accounts, this executes 101 database queries (1 + 100).

### Fixed Version with .Include()

```csharp
// GOOD: Single query with JOIN
var accounts = await _context.Customers
    .Include(a => a.AccountContacts)  // Eager loading
    .ToListAsync();  // 1 query with JOIN
    
foreach (var account in accounts)
{
    var contacts = account.AccountContacts;  // No additional query
    // Process contacts...
}
```

**Result**: Only 1 database query with a JOIN.

## Detection Strategies

### 1. Enable SQL Logging

Add to `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

This will log all SQL queries executed by EF Core.

### 2. Look for These Patterns in Code

#### Pattern 1: Missing .Include() for Navigation Properties

```csharp
// SUSPICIOUS: No .Include() but accessing navigation property
var accounts = await _context.Customers.ToListAsync();
foreach (var account in accounts)
{
    Console.WriteLine(account.AccountManager?.Name);  // Lazy loading!
}
```

#### Pattern 2: Accessing Collections in Loops

```csharp
// SUSPICIOUS: Accessing collection property without Include
var opportunities = await _context.Opportunities.ToListAsync();
foreach (var opp in opportunities)
{
    var productCount = opp.Products.Count();  // N+1 query
}
```

#### Pattern 3: Nested Navigation Properties

```csharp
// SUSPICIOUS: No ThenInclude for nested properties
var accounts = await _context.Customers
    .Include(a => a.Opportunities)  // Good
    .ToListAsync();
    
foreach (var account in accounts)
{
    foreach (var opp in account.Opportunities)
    {
        var ownerName = opp.SalesOwner?.Name;  // N+1 query!
    }
}
```

## Common Areas to Review

### High Priority (Frequently Accessed)

#### 1. Account/Customer Queries

**File**: `CRM.Backend/src/CRM.Infrastructure/Services/AccountService.cs`

**Check for**:
- Account.AccountContacts
- Account.Opportunities
- Account.AssignedToUser
- Account.AccountManager
- Account.ParentAccount

**Example Fix**:

```csharp
// Before
var accounts = await _accountRepository.GetAllAsync();

// After
var accounts = await _context.Customers
    .Include(a => a.AccountContacts)
        .ThenInclude(ac => ac.Contact)
    .Include(a => a.AssignedToUser)
    .Include(a => a.AccountManager)
    .Where(a => !a.IsDeleted)
    .ToListAsync();
```

#### 2. Opportunity Queries

**File**: `CRM.Backend/src/CRM.Infrastructure/Services/OpportunityService.cs`

**Check for**:
- Opportunity.Account
- Opportunity.PrimaryContact
- Opportunity.SalesOwner
- Opportunity.Products (OpportunityProduct junction)
- Opportunity.Lead

**Example Fix**:

```csharp
// Before
var opportunities = await _opportunityRepository.GetAllAsync();

// After
var opportunities = await _context.Opportunities
    .Include(o => o.Account)
    .Include(o => o.PrimaryContact)
    .Include(o => o.SalesOwner)
    .Include(o => o.Products)
        .ThenInclude(op => op.Product)
    .Where(o => !o.IsDeleted)
    .ToListAsync();
```

#### 3. Quote Queries

**Files**: `CRM.Backend/src/CRM.Infrastructure/Services/*Service.cs`

**Check for**:
- Quote.Account
- Quote.Contact
- Quote.Opportunity
- Quote.AssignedToUser
- Quote.LineItems (QuoteLineItem collection)

**Example Fix**:

```csharp
var quotes = await _context.Quotes
    .Include(q => q.Account)
    .Include(q => q.Contact)
    .Include(q => q.Opportunity)
    .Include(q => q.AssignedToUser)
    .Include(q => q.LineItems)
        .ThenInclude(li => li.Product)
    .Where(q => !q.IsDeleted)
    .ToListAsync();
```

#### 4. Service Request Queries

**Check for**:
- ServiceRequest.Account
- ServiceRequest.Contact
- ServiceRequest.AssignedToUser
- ServiceRequest.CreatedByUser
- ServiceRequest.ChildServiceRequests

#### 5. Marketing Campaign Queries

**Check for**:
- MarketingCampaign.GeneratedLeads
- MarketingCampaign.CampaignMetrics
- Lead.Campaign
- Lead.Owner
- Lead.Account
- Lead.Contact

### Medium Priority (Moderately Accessed)

#### 6. Task Queries

**Check for**:
- CrmTask.Account
- CrmTask.Opportunity
- CrmTask.AssignedToUser
- CrmTask.CreatedByUser

#### 7. Note Queries

**Check for**:
- Note.CreatedByUser
- Note.LastModifiedByUser

#### 8. Activity Queries

**Check for**:
- Activity.User
- Activity.Account
- Activity.Opportunity

### Low Priority (Less Frequently Accessed)

#### 9. Workflow Queries
- WorkflowInstance.WorkflowDefinition
- WorkflowInstance.ExecutingUser

#### 10. Relationship Queries
- AccountRelationship.FromAccount
- AccountRelationship.ToAccount
- AccountRelationship.RelationshipType

## Optimization Techniques

### 1. Use .Include() for Required Navigation Properties

```csharp
var accounts = await _context.Customers
    .Include(a => a.AssignedToUser)
    .Include(a => a.AccountManager)
    .ToListAsync();
```

### 2. Use .ThenInclude() for Nested Properties

```csharp
var accounts = await _context.Customers
    .Include(a => a.AccountContacts)
        .ThenInclude(ac => ac.Contact)
    .ToListAsync();
```

### 3. Use Projections with Select() for Read-Only Data

When you only need a few fields, use projections instead of loading entire entities:

```csharp
// Better for performance if you only need names
var accountNames = await _context.Customers
    .Where(a => !a.IsDeleted)
    .Select(a => new
    {
        a.Id,
        a.CompanyName,
        ManagerName = a.AccountManager != null ? a.AccountManager.Name : null
    })
    .ToListAsync();
```

### 4. Use .AsSplitQuery() for Multiple Collections

When including multiple collections, use split queries to avoid cartesian explosion:

```csharp
var accounts = await _context.Customers
    .Include(a => a.Opportunities)
    .Include(a => a.Interactions)
    .AsSplitQuery()  // Prevents cartesian explosion
    .ToListAsync();
```

### 5. Use Explicit Loading for Conditional Access

If you only sometimes need related data:

```csharp
var account = await _context.Customers.FindAsync(id);

if (includeContacts)
{
    await _context.Entry(account)
        .Collection(a => a.AccountContacts)
        .LoadAsync();
}
```

### 6. Avoid Loading Large Collections

For very large collections, use paging or separate queries:

```csharp
// Instead of loading all opportunities
var account = await _context.Customers.FindAsync(id);
var recentOpportunities = await _context.Opportunities
    .Where(o => o.AccountId == id)
    .OrderByDescending(o => o.CreatedAt)
    .Take(10)
    .ToListAsync();
```

## Performance Benchmarking

### Before Optimization

```csharp
// Measure query performance
var stopwatch = Stopwatch.StartNew();
var accounts = await _accountRepository.GetAllAsync();
stopwatch.Stop();
_logger.LogInformation("Query took {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
```

### After Optimization

```csharp
var stopwatch = Stopwatch.StartNew();
var accounts = await _context.Customers
    .Include(a => a.AccountContacts)
    .Include(a => a.AssignedToUser)
    .ToListAsync();
stopwatch.Stop();
_logger.LogInformation("Optimized query took {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
```

## Testing Strategy

### 1. Unit Tests with InMemory Database

```csharp
[Fact]
public async Task GetAllAccounts_IncludesNavigationProperties()
{
    // Arrange
    var options = new DbContextOptionsBuilder<CrmDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
        
    using var context = new CrmDbContext(options);
    // Seed test data
    
    // Act
    var accounts = await context.Customers
        .Include(a => a.AccountManager)
        .ToListAsync();
    
    // Assert
    Assert.NotNull(accounts[0].AccountManager);
}
```

### 2. Integration Tests with SQL Logging

```csharp
[Fact]
public async Task GetAllAccounts_ExecutesSingleQuery()
{
    // Arrange
    var queryCount = 0;
    _context.Database.Log = log =>
    {
        if (log.Contains("SELECT")) queryCount++;
    };
    
    // Act
    var accounts = await _context.Customers
        .Include(a => a.AccountContacts)
        .ToListAsync();
    
    // Assert
    Assert.Equal(1, queryCount);  // Should be 1, not N+1
}
```

## Action Items

### Phase 1: High Priority Services (Week 1-2)

- [ ] **AccountService.cs**
  - [ ] Review `GetAllAccountsAsync()` - add Include for AccountManager, AssignedToUser
  - [ ] Review `GetAccountByIdAsync()` - add Include for AccountContacts with Contact
  - [ ] Review mapping methods - ensure no lazy loading in DTOs

- [ ] **OpportunityService.cs**
  - [ ] Review `GetAllOpportunitiesAsync()` - add Include for Account, PrimaryContact, SalesOwner
  - [ ] Review `GetOpportunityByIdAsync()` - add ThenInclude for Products.Product
  - [ ] Review opportunity-to-DTO mapping

- [ ] **QuoteService.cs**
  - [ ] Review all quote queries - add Include for Account, Contact, Opportunity
  - [ ] Review line items - ensure LineItems.Product is included

### Phase 2: Medium Priority Services (Week 3-4)

- [ ] **ServiceRequestService.cs**
  - [ ] Review service request queries
  - [ ] Add Include for AssignedToUser, CreatedByUser

- [ ] **MarketingCampaignService.cs**
  - [ ] Review campaign queries
  - [ ] Add Include for GeneratedLeads, CampaignMetrics

- [ ] **TasksController.cs**
  - [ ] Review task queries
  - [ ] Add Include for AssignedToUser, Account, Opportunity

### Phase 3: Optimization and Refactoring (Week 5-6)

- [ ] Add projection DTOs for list views (lighter queries)
- [ ] Implement AsSplitQuery() where cartesian explosion occurs
- [ ] Add query performance logging middleware
- [ ] Create benchmark tests for before/after comparisons

### Phase 4: Documentation and Standards (Week 7)

- [ ] Update coding standards with EF Core best practices
- [ ] Add EF Core query guidelines to onboarding docs
- [ ] Create code review checklist for N+1 detection
- [ ] Document performance benchmarks

## Common Mistakes to Avoid

### 1. ❌ Enabling Lazy Loading Globally

```csharp
// DON'T DO THIS - causes N+1 everywhere
optionsBuilder.UseLazyLoadingProxies();
```

### 2. ❌ Including Too Much Data

```csharp
// BAD: Loading unnecessary data
var accounts = await _context.Customers
    .Include(a => a.Opportunities)  // Might be 1000s
    .Include(a => a.Interactions)   // Might be 1000s
    .ToListAsync();  // Loads everything into memory
```

### 3. ❌ Using Include() in Loops

```csharp
// BAD: Still N+1, just with Include in loop
foreach (var accountId in accountIds)
{
    var account = await _context.Customers
        .Include(a => a.Opportunities)
        .FirstAsync(a => a.Id == accountId);
}
```

### 4. ❌ Not Using Projections for Simple Queries

```csharp
// INEFFICIENT: Loading full entities for simple list
var names = (await _context.Customers.ToListAsync())
    .Select(a => a.CompanyName)
    .ToList();

// BETTER: Use projection in database
var names = await _context.Customers
    .Select(a => a.CompanyName)
    .ToListAsync();
```

## Success Metrics

### Query Performance

| Metric | Before | Target | Status |
|--------|--------|--------|--------|
| Avg Account List Query | TBD | <100ms | ⏳ Pending |
| Avg Opportunity Detail Query | TBD | <50ms | ⏳ Pending |
| Avg Quote with Line Items | TBD | <75ms | ⏳ Pending |

### Database Metrics

| Metric | Before | Target | Status |
|--------|--------|--------|--------|
| Queries per Page Load | TBD | <20 | ⏳ Pending |
| Total DB Connections | TBD | <50 | ⏳ Pending |

## Resources

### EF Core Documentation
- [Loading Related Data](https://learn.microsoft.com/en-us/ef/core/querying/related-data/)
- [Performance Best Practices](https://learn.microsoft.com/en-us/ef/core/performance/)
- [Split Queries](https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries)

### Tools
- **MiniProfiler**: Identify N+1 queries in development
- **Application Insights**: Monitor query performance in production
- **SQL Profiler**: Analyze actual SQL queries
- **EF Core Query Tags**: Add tags to identify queries

### SQL Profiler Setup

```csharp
// Add query tags to identify queries
var accounts = await _context.Customers
    .TagWith("GetAllAccounts - Dashboard")
    .Include(a => a.AccountManager)
    .ToListAsync();
```

## Conclusion

N+1 query problems are common in applications using ORMs like Entity Framework Core. They can significantly impact performance, especially as data volume grows. By proactively reviewing queries, adding `.Include()` statements, using projections, and implementing proper testing, we can ensure optimal database performance.

**Next Steps**:
1. Start with Phase 1 (High Priority Services)
2. Enable SQL logging in development environment
3. Run performance benchmarks before and after changes
4. Update this document with actual performance metrics

---

**Maintenance**: This document should be updated quarterly or whenever significant database changes are made.

**Owner**: Backend Team  
**Last Review**: 2024 (Session 13)  
**Next Review**: Q2 2024
