# Customer → Account Entity Migration Plan

## Executive Summary

This document outlines the comprehensive plan to migrate the CRM solution from "Customer" terminology to "Account" terminology. This change aligns with industry-standard CRM practices where:
- **Account** = Organization/Company entity
- **Contact** = Individual person associated with an Account
- **Lead** = Potential customer not yet converted

### Current State Issues
1. **Naming Confusion**: Frontend shows "Accounts" in menu but uses `Customer` internally
2. **Database Schema Mismatch**: Some tables use `CustomerId`, others use `AccountId` (e.g., Opportunities, Leads)
3. **API Inconsistency**: Both `/api/customers` and `/api/accounts` routes exist in some places
4. **Foreign Key Confusion**: 15+ tables reference `CustomerId` while entity model evolves

---

## Migration Scope

### Phase 1: Database Schema Migration (CRITICAL)

#### 1.1 Rename Tables
| Current Table | New Table |
|---------------|-----------|
| `Customers` | `Accounts` |
| `CustomerContacts` | `AccountContacts` |
| `CustomerTerritoryAssignments` | `AccountTerritoryAssignments` |

#### 1.2 Rename Foreign Key Columns
| Table | Current Column | New Column |
|-------|----------------|------------|
| `Opportunities` | `CustomerId` | `AccountId` (already exists, remove CustomerId) |
| `Opportunities` | `AccountId1` | Remove (duplicate) |
| `Quotes` | `CustomerId` | `AccountId` |
| `ServiceRequests` | `CustomerId` | `AccountId` |
| `Invoices` | `CustomerId` | `AccountId` |
| `Leads` | `CustomerId` | `AccountId` (already exists) |
| `Contacts` | `CustomerId` | `AccountId` |
| `Notes` | `CustomerId` | `AccountId` |
| `Activities` | `CustomerId` | `AccountId` |
| `CrmTasks` | `CustomerId` | `AccountId` |
| `MarketingCampaigns` | Various | Update entity refs |
| `ChurnRisks` | `CustomerId` | `AccountId` |
| `CustomerHealthMetrics` | (table rename) | `AccountHealthMetrics` |

#### 1.3 Migration Script Strategy
```sql
-- Step 1: Add new AccountId columns where missing
ALTER TABLE Quotes ADD COLUMN AccountId INT NULL;
ALTER TABLE ServiceRequests ADD COLUMN AccountId INT NULL;
-- ... etc

-- Step 2: Copy data from CustomerId to AccountId
UPDATE Quotes SET AccountId = CustomerId WHERE AccountId IS NULL;
UPDATE ServiceRequests SET AccountId = CustomerId WHERE AccountId IS NULL;
-- ... etc

-- Step 3: Rename Customers table
RENAME TABLE Customers TO Accounts;

-- Step 4: Update FK constraints
ALTER TABLE Opportunities DROP FOREIGN KEY FK_Opportunities_CustomerId;
ALTER TABLE Opportunities ADD CONSTRAINT FK_Opportunities_AccountId 
    FOREIGN KEY (AccountId) REFERENCES Accounts(Id);

-- Step 5: Drop old CustomerId columns (after verification)
ALTER TABLE Quotes DROP COLUMN CustomerId;
-- ... etc
```

---

### Phase 2: Backend Entity Changes

#### 2.1 Core Entity Renames
| Current File | New File |
|--------------|----------|
| `CRM.Core/Entities/Customer.cs` | `CRM.Core/Entities/Account.cs` |
| `CRM.Core/Entities/CustomerContact.cs` | `CRM.Core/Entities/AccountContact.cs` |
| `CRM.Core/Entities/CustomerTerritoryAssignment.cs` | `CRM.Core/Entities/AccountTerritoryAssignment.cs` |

#### 2.2 Enum Renames
| Current Enum | New Enum |
|--------------|----------|
| `CustomerCategory` | `AccountCategory` |
| `CustomerLifecycleStage` | `AccountLifecycleStage` |
| `CustomerType` | `AccountType` |
| `CustomerPriority` | `AccountPriority` |
| `CustomerContactRole` | `AccountContactRole` |
| `CustomerRiskLevel` (ChurnRisk) | `AccountRiskLevel` |

#### 2.3 Property Renames in Related Entities
All entities with `CustomerId` and `Customer` navigation properties need updating:

```csharp
// Before
public int CustomerId { get; set; }
public virtual Customer Customer { get; set; }

// After
public int AccountId { get; set; }
public virtual Account Account { get; set; }
```

**Affected Entities (20+):**
- Opportunity.cs
- Quote.cs
- ServiceRequest.cs
- Invoice.cs
- Lead.cs
- ContactModel.cs
- Note.cs
- Activity.cs
- CrmTask.cs
- ChurnRisk.cs
- MarketingCampaign.cs
- Payment.cs
- Subscription.cs
- Contract.cs
- Document.cs
- Order.cs
- SupportTicket.cs
- EmailAddress.cs
- PhoneNumber.cs
- Address.cs

---

### Phase 3: Service Layer Changes

#### 3.1 Interface Renames
| Current Interface | New Interface |
|-------------------|---------------|
| `ICustomerService` | `IAccountService` |
| `ICustomerRepository` | `IAccountRepository` |

#### 3.2 Implementation Renames
| Current Class | New Class |
|---------------|-----------|
| `CustomerService` | `AccountService` |
| `CustomerRepository` | `AccountRepository` |

#### 3.3 Method Signature Updates
```csharp
// Before
Task<CustomerDto> GetCustomerByIdAsync(int customerId);
Task<IEnumerable<CustomerDto>> GetCustomersAsync();

// After
Task<AccountDto> GetAccountByIdAsync(int accountId);
Task<IEnumerable<AccountDto>> GetAccountsAsync();
```

---

### Phase 4: API Controller Changes

#### 4.1 Controller Renames
| Current File | New File | Route Change |
|--------------|----------|--------------|
| `CustomersController.cs` | `AccountsController.cs` | `/api/customers` → `/api/accounts` |

#### 4.2 DTO Renames
| Current DTO | New DTO |
|-------------|---------|
| `CustomerDto` | `AccountDto` |
| `CustomerListDto` | `AccountListDto` |
| `CreateCustomerDto` | `CreateAccountDto` |
| `UpdateCustomerDto` | `UpdateAccountDto` |
| `CustomerSummaryDto` | `AccountSummaryDto` |
| `CustomerDetailsDto` | `AccountDetailsDto` |

#### 4.3 Backward Compatibility Route Aliases
```csharp
// Keep old routes working during transition
[Route("api/[controller]")]
[Route("api/customers")] // Legacy alias
public class AccountsController : ControllerBase
```

---

### Phase 5: DbContext Changes

#### 5.1 DbSet Renames
```csharp
// Before
public DbSet<Customer> Customers { get; set; }
public DbSet<CustomerContact> CustomerContacts { get; set; }

// After
public DbSet<Account> Accounts { get; set; }
public DbSet<AccountContact> AccountContacts { get; set; }
```

#### 5.2 Entity Configuration Updates
Update all `HasForeignKey(x => x.CustomerId)` to `HasForeignKey(x => x.AccountId)`

---

### Phase 6: Frontend Changes

#### 6.1 Type/Interface Renames
| Current | New |
|---------|-----|
| `Customer` type | `Account` type |
| `CustomerFormData` | `AccountFormData` |

#### 6.2 Service Renames
| Current File | New File |
|--------------|----------|
| `customerService.ts` | `accountService.ts` |

#### 6.3 Component/Page Renames
| Current | New |
|---------|-----|
| `CustomersPage.tsx` | `AccountsPage.tsx` |
| `CustomerDetailsPage.tsx` | `AccountDetailsPage.tsx` |

#### 6.4 Route Updates
```typescript
// Already using 'accounts' in menu, just update internals
{ path: 'accounts', component: AccountsPage }
```

#### 6.5 Context Property Renames
```typescript
// AppContext
customers → accounts
customerCount → accountCount
selectedCustomer → selectedAccount
```

---

### Phase 7: Test Updates

#### 7.1 Backend Tests
- `CustomerServiceTests.cs` → `AccountServiceTests.cs`
- Update all test data and assertions

#### 7.2 E2E Tests
- `customer-crud.spec.ts` → `account-crud.spec.ts`
- Update API endpoint references
- Update test assertions

#### 7.3 Frontend Tests
- Update component test imports
- Update mock data

---

### Phase 8: Microservice Updates

#### 8.1 Rename Customer Microservice
| Current | New |
|---------|-----|
| `CRM.CustomerService` project | `CRM.AccountService` |
| `Dockerfile.customer` | `Dockerfile.account` |
| Docker image `crm-customer-service` | `crm-account-service` |

#### 8.2 Kubernetes Manifest Updates
- Update deployment names
- Update service names
- Update ConfigMaps

---

### Phase 9: Configuration Updates

#### 9.1 Gateway Configuration
```yaml
# Update Ocelot routes
Routes:
  - DownstreamPathTemplate: "/api/accounts/{everything}"
    UpstreamPathTemplate: "/api/accounts/{everything}"
```

#### 9.2 Seed Data Updates
- Update module field configurations
- Update default data references

---

## Implementation Order (Recommended)

### Sprint 1: Database & Core Entities (Week 1-2)
1. ✅ Create database migration script
2. ✅ Rename `Customer.cs` → `Account.cs`
3. ✅ Update all entity FK references
4. ✅ Update DbContext
5. ✅ Run EF migrations
6. ✅ Verify data integrity

### Sprint 2: Services & Controllers (Week 2-3)
1. ✅ Rename service interfaces and implementations
2. ✅ Rename controllers
3. ✅ Update DTOs
4. ✅ Add backward-compatible route aliases
5. ✅ Update DI registrations

### Sprint 3: Frontend (Week 3-4)
1. ✅ Update TypeScript types
2. ✅ Rename services
3. ✅ Update components
4. ✅ Update API calls
5. ✅ Update tests

### Sprint 4: Cleanup & Verification (Week 4)
1. ✅ Run full test suite
2. ✅ Remove deprecated aliases
3. ✅ Update documentation
4. ✅ Update microservices
5. ✅ Deploy and verify

---

## Risk Mitigation

### 1. Backward Compatibility
- Keep `/api/customers` routes as aliases for 30 days
- Use database column aliases during transition
- Maintain both DTO names temporarily

### 2. Data Integrity
- Create database backup before migration
- Run migration in staging first
- Verify FK relationships post-migration

### 3. Rollback Plan
- Keep Customer.cs.bak files
- Maintain rollback migration script
- Document all changes for reversal

---

## Validation Checklist

- [ ] Database migration completed
- [ ] All 20+ entity files updated
- [ ] All services renamed and functional
- [ ] All controllers updated
- [ ] All DTOs renamed
- [ ] Frontend fully updated
- [ ] All tests passing
- [ ] API documentation updated
- [ ] Microservices updated
- [ ] Gateway configuration updated
- [ ] Kubernetes manifests updated
- [ ] Seed data updated

---

## File Inventory

### Backend Files to Modify (~50 files)

#### Core Entities (CRM.Core/Entities/)
1. Customer.cs → Account.cs
2. CustomerContact.cs → AccountContact.cs  
3. CustomerTerritoryAssignment.cs → AccountTerritoryAssignment.cs
4. Opportunity.cs (CustomerId → AccountId)
5. Quote.cs (CustomerId → AccountId)
6. ServiceRequest.cs (CustomerId → AccountId)
7. Invoice.cs (CustomerId → AccountId)
8. Lead.cs (CustomerId → AccountId)
9. ContactModel.cs (CustomerId → AccountId)
10. Note.cs (CustomerId → AccountId)
11. Activity.cs (CustomerId → AccountId)
12. CrmTask.cs (CustomerId → AccountId)
13. ChurnRisk.cs (CustomerId → AccountId)
14. MarketingCampaign.cs
15. Payment.cs
16. Subscription.cs
17. Contract.cs
18. Document.cs
19. Order.cs

#### Services (CRM.Infrastructure/Services/)
20. CustomerService.cs → AccountService.cs
21. OpportunityService.cs
22. QuoteService.cs
23. ServiceRequestService.cs

#### Interfaces (CRM.Core/Interfaces/)
24. ICustomerService.cs → IAccountService.cs
25. ICustomerRepository.cs → IAccountRepository.cs

#### Controllers (CRM.Api/Controllers/)
26. CustomersController.cs → AccountsController.cs
27. OpportunitiesController.cs
28. QuotesController.cs
29. ServiceRequestsController.cs
30. DashboardController.cs
31. ReportsController.cs

#### DTOs (CRM.Api/DTOs/ or inline)
32. CustomerDtos.cs → AccountDtos.cs
33. OpportunityDtos.cs
34. QuoteDtos.cs

#### DbContext
35. CrmDbContext.cs
36. ICrmDbContext.cs

### Frontend Files to Modify (~20 files)

#### Pages (CRM.Frontend/src/pages/)
37. CustomersPage.tsx → AccountsPage.tsx
38. CustomerDetailsPage.tsx → AccountDetailsPage.tsx

#### Services (CRM.Frontend/src/services/)
39. customerService.ts → accountService.ts
40. api.ts

#### Types (CRM.Frontend/src/types/)
41. index.ts (Customer types)
42. forms.ts

#### Components
43. Sidebar.tsx
44. App.tsx (routes)

#### Contexts
45. AppContext.tsx
46. EntityContext.tsx

### Test Files (~10 files)
47. CustomerServiceTests.cs → AccountServiceTests.cs
48. CustomersControllerTests.cs → AccountsControllerTests.cs
49. customer-crud.spec.ts → account-crud.spec.ts
50. api-bvt.spec.ts

### Database Migrations (~3 files)
51. XXX_rename_customer_to_account.sql
52. XXX_update_fk_references.sql
53. XXX_cleanup_legacy_columns.sql

### Configuration Files (~5 files)
54. appsettings.json
55. ocelot.json
56. docker-compose files
57. kubernetes manifests

---

## Estimated Effort

| Phase | Effort (Hours) | Risk |
|-------|----------------|------|
| Phase 1: Database | 8 | High |
| Phase 2: Entities | 16 | Medium |
| Phase 3: Services | 8 | Medium |
| Phase 4: Controllers | 8 | Medium |
| Phase 5: DbContext | 4 | Low |
| Phase 6: Frontend | 16 | Medium |
| Phase 7: Tests | 8 | Low |
| Phase 8: Microservices | 8 | Medium |
| Phase 9: Config | 4 | Low |
| **Total** | **80 hours** | |

---

## Next Steps

1. **Review and approve this plan**
2. **Create feature branch**: `feature/customer-to-account-migration`
3. **Start with database migration script**
4. **Implement incrementally with tests**
5. **Deploy to staging for validation**
6. **Production deployment with rollback ready**

---

*Document created: January 31, 2026*
*Last updated: January 31, 2026*
*Author: CRM Development Team*
