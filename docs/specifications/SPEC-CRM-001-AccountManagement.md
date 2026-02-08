# Feature Specification: Account Management

> **Spec ID:** SPEC-CRM-001  
> **Feature:** Account Management  
> **Module:** Core CRM  
> **Version:** 1.0  
> **Last Updated:** February 8, 2026  
> **Status:** ✅ Implemented

---

## 1. Business Context

### 1.1 Feature Description
Manage B2B and B2C customer accounts with full contact information, relationship tracking, activity history, and lifecycle management. Supports both Individual (person) and Organization (company) account types with category-specific fields.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Account CRUD | Create, read, update, delete accounts | ✅ Implemented |
| SF-002 | Individual Accounts | Person accounts with FirstName/LastName | ✅ Implemented |
| SF-003 | Organization Accounts | Company accounts with Company/LegalName | ✅ Implemented |
| SF-004 | Contact Linking | Link contacts to organization accounts | ✅ Implemented |
| SF-005 | Account Search | Full-text search across account fields | ✅ Implemented |
| SF-006 | Lifecycle Management | Track account through lifecycle stages | ✅ Implemented |
| SF-007 | Account Health Score | Automated health scoring | ✅ Implemented |
| SF-008 | Account Hierarchy | Parent-child account relationships | ✅ Implemented |
| SF-009 | Owner Assignment | Assign sales rep to accounts | ✅ Implemented |
| SF-010 | Territory Assignment | Assign accounts to territories | ⚠️ Partial |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Create Individual Account | Sales Rep | Logged in | Account created with person info | ✅ |
| UC-002 | Create Organization Account | Sales Rep | Logged in | Account created with company info | ✅ |
| UC-003 | View Account List | Any User | Logged in | List of accessible accounts shown | ✅ |
| UC-004 | Search Accounts | Any User | Logged in | Search results returned | ✅ |
| UC-005 | Update Account | Account Owner | Has edit permission | Account updated | ✅ |
| UC-006 | Delete Account | Admin | Has delete permission | Account soft-deleted | ✅ |
| UC-007 | Link Contact to Account | Sales Rep | Organization account exists | Contact linked with role | ✅ |
| UC-008 | View Account Timeline | Any User | Account exists | Activities shown | ✅ |
| UC-009 | Change Lifecycle Stage | Sales Rep | Account exists | Stage updated | ✅ |
| UC-010 | Assign Account Owner | Manager | Account exists | Owner changed | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| CustomersPage | `CRM.Frontend/src/pages/CustomersPage.tsx` | ✅ | Main account list page |
| AccountPage | `CRM.Frontend/src/pages/AccountPage.tsx` | ✅ | Account detail/edit page |
| CustomerOverviewPage | `CRM.Frontend/src/pages/CustomerOverviewPage.tsx` | ✅ | Dashboard view |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| AccountForm | `CRM.Frontend/src/components/accounts/AccountForm.tsx` | ❌ Not Found | May be inline in page |
| AccountCard | `CRM.Frontend/src/components/accounts/AccountCard.tsx` | ❌ Not Found | May be inline |
| AccountTimeline | `CRM.Frontend/src/components/accounts/AccountTimeline.tsx` | ❌ Not Found | |
| ContactLinkDialog | `CRM.Frontend/src/components/accounts/ContactLinkDialog.tsx` | ❌ Not Found | |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| accountService | `CRM.Frontend/src/services/accountService.ts` | getAll, getById, create, update, delete, search | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Email | Valid email format | Both | ✅ |
| FirstName | Required for Individual | Frontend | ⚠️ Not enforced |
| LastName | Required for Individual | Frontend | ⚠️ Not enforced |
| Company | Required for Organization | Frontend | ⚠️ Not enforced |
| Phone | Valid phone format | Frontend | ❌ Not Implemented |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Account | `CRM.Core/Entities/Account.cs` | ✅ | 638 lines, maps to Customers table |
| AccountContact | `CRM.Core/Entities/AccountContact.cs` | ✅ | Junction table entity |
| AccountRelationship | `CRM.Core/Entities/AccountRelationship.cs` | ✅ | Parent-child relationships |
| AccountTerritory | `CRM.Core/Entities/AccountTerritory.cs` | ✅ | Territory assignment |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| AccountDto | `CRM.Core/DTOs/AccountDto.cs` | ✅ | Response DTO |
| CreateAccountDto | `CRM.Core/DTOs/CreateAccountDto.cs` | ✅ | Create request |
| UpdateAccountDto | `CRM.Core/DTOs/UpdateAccountDto.cs` | ✅ | Update request |
| AccountContactDto | `CRM.Core/DTOs/AccountContactDto.cs` | ✅ | Contact link DTO |
| LinkContactToAccountDto | `CRM.Core/DTOs/LinkContactToAccountDto.cs` | ✅ | Link request |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IAccountService | `CRM.Core/Interfaces/IAccountService.cs` | 21 | ✅ |

**IAccountService Methods:**
```
- GetAccountByIdAsync(int id)
- GetAllAccountsAsync()
- SearchAccountsAsync(string searchTerm)
- CreateAccountAsync(CreateAccountDto dto)
- UpdateAccountAsync(int id, UpdateAccountDto dto)
- DeleteAccountAsync(int id)
- GetIndividualAccountsAsync()
- GetOrganizationAccountsAsync()
- LinkContactToAccountAsync(int accountId, LinkContactToAccountDto dto)
- UnlinkContactFromAccountAsync(int accountId, int contactId)
- UpdateAccountContactAsync(int accountId, int contactId, UpdateAccountContactDto dto)
- GetAccountContactsAsync(int accountId)
- SetPrimaryContactAsync(int accountId, int contactId)
- GetDirectContactsAsync(int accountId)
- AssignContactToAccountAsync(int accountId, int contactId)
- UnassignContactFromAccountAsync(int accountId, int contactId)
- GetAccountsByAssignedUserAsync(int userId)
- GetAccountsByLifecycleStageAsync(AccountLifecycleStage stage)
- GetAccountsByPriorityAsync(AccountPriority priority)
```

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| AccountService | `CRM.Infrastructure/Services/AccountService.cs` | 21 | ✅ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| AccountsController | `CRM.Api/Controllers/AccountsController.cs` | 15+ | ✅ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/accounts` | GetAll | Yes | ✅ |
| GET | `/api/accounts/{id}` | GetById | Yes | ✅ |
| GET | `/api/accounts/individuals` | GetIndividuals | Yes | ✅ |
| GET | `/api/accounts/organizations` | GetOrganizations | Yes | ✅ |
| GET | `/api/accounts/search/{term}` | Search | Yes | ✅ |
| GET | `/api/accounts/by-user/{userId}` | GetByUser | Yes | ✅ |
| GET | `/api/accounts/by-stage/{stage}` | GetByStage | Yes | ✅ |
| GET | `/api/accounts/by-priority/{priority}` | GetByPriority | Yes | ✅ |
| POST | `/api/accounts` | Create | Yes | ✅ |
| PUT | `/api/accounts/{id}` | Update | Yes | ✅ |
| DELETE | `/api/accounts/{id}` | Delete | Yes | ✅ |
| GET | `/api/accounts/{id}/contacts` | GetContacts | Yes | ✅ |
| POST | `/api/accounts/{id}/contacts` | LinkContact | Yes | ✅ |
| DELETE | `/api/accounts/{id}/contacts/{contactId}` | UnlinkContact | Yes | ✅ |
| PUT | `/api/accounts/{id}/contacts/{contactId}/primary` | SetPrimary | Yes | ✅ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Email | [EmailAddress] attribute | Entity | ✅ |
| Email | [MaxLength(255)] | Entity | ✅ |
| FirstName | [MaxLength(100)] | Entity | ✅ |
| LastName | [MaxLength(100)] | Entity | ✅ |
| Company | [MaxLength(255)] | Entity | ✅ |
| Phone | [MaxLength(20)] | Entity | ✅ |
| Category | Required | Service | ⚠️ Business rule not enforced |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Customers | `database/schema/001_core_tables.sql` | ✅ | Named "Customers" for backward compatibility |
| AccountContacts | `database/schema/001_core_tables.sql` | ✅ | Junction table |
| AccountRelationships | `database/schema/001_core_tables.sql` | ✅ | Hierarchy |

### 4.2 Data Elements - Customers Table
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| Category | INT | No | 0 | - | Category (enum) | ✅ |
| FirstName | VARCHAR(100) | Yes | NULL | - | FirstName | ✅ |
| LastName | VARCHAR(100) | Yes | NULL | - | LastName | ✅ |
| Email | VARCHAR(255) | No | - | UK | Email | ✅ |
| Phone | VARCHAR(20) | Yes | NULL | - | Phone | ✅ |
| Company | VARCHAR(255) | Yes | NULL | - | Company | ✅ |
| LegalName | VARCHAR(255) | Yes | NULL | - | LegalName | ✅ |
| DbaName | VARCHAR(255) | Yes | NULL | - | DbaName | ✅ |
| TaxId | VARCHAR(50) | Yes | NULL | - | TaxId | ✅ |
| RegistrationNumber | VARCHAR(100) | Yes | NULL | - | RegistrationNumber | ✅ |
| Industry | VARCHAR(100) | Yes | NULL | - | Industry | ✅ |
| Website | VARCHAR(500) | Yes | NULL | - | Website | ✅ |
| NumberOfEmployees | INT | Yes | NULL | - | NumberOfEmployees | ✅ |
| AnnualRevenue | DECIMAL(18,2) | Yes | NULL | - | AnnualRevenue | ✅ |
| LifecycleStage | INT | No | 0 | - | LifecycleStage (enum) | ✅ |
| AccountType | INT | Yes | NULL | - | AccountType (enum) | ✅ |
| Priority | INT | No | 0 | - | Priority (enum) | ✅ |
| Status | VARCHAR(50) | Yes | 'Active' | - | Status | ✅ |
| AccountHealthScore | INT | Yes | 50 | - | AccountHealthScore | ✅ |
| OwnerId | INT | Yes | NULL | FK→Users | OwnerId | ✅ |
| ParentAccountId | INT | Yes | NULL | FK→Customers | ParentAccountId | ✅ |
| ReferredByAccountId | INT | Yes | NULL | FK→Customers | ReferredByAccountId | ✅ |
| Tier | VARCHAR(20) | Yes | NULL | - | Tier | ✅ |
| Segment | VARCHAR(50) | Yes | NULL | - | Segment | ✅ |
| Source | VARCHAR(100) | Yes | NULL | - | Source | ✅ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | ✅ |
| IsDeleted | TINYINT(1) | No | 0 | - | IsDeleted | ✅ |
| RowVersion | BINARY(8) | Yes | NULL | - | RowVersion | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| Customers | Users | N:1 | OwnerId | ✅ |
| Customers | Customers | N:1 (self) | ParentAccountId | ✅ |
| Customers | Customers | N:1 (self) | ReferredByAccountId | ✅ |
| AccountContacts | Customers | N:1 | AccountId | ✅ |
| AccountContacts | Contacts | N:1 | ContactId | ✅ |
| Opportunities | Customers | N:1 | AccountId | ✅ |
| Leads | Customers | N:1 | AccountId | ✅ |
| ServiceRequests | Customers | N:1 | AccountId | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| PK_Customers | Customers | Id | Clustered | ✅ |
| IX_Customers_Email | Customers | Email | NonClustered | ✅ |
| IX_Customers_Company | Customers | Company | NonClustered | ✅ |
| IX_Customers_OwnerId | Customers | OwnerId | NonClustered | ✅ |
| IX_Customers_Category | Customers | Category | NonClustered | ✅ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| AccountServiceTests | `CRM.Tests/Services/AccountServiceTests.cs` | ~15 | ⚠️ Partial |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| AccountsControllerTests | `CRM.Tests/Integration/AccountsControllerTests.cs` | ~10 | ❌ Not Found |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| customer-crud.spec.ts | `e2e-tests/tests/customers/customer-crud.spec.ts` | ~8 | ⚠️ Partial |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| Entity: Email | DTO: Email | Match | ✅ OK |
| DB: Category (INT) | Entity: Category (enum) | EF handles | ✅ OK |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| AccountForm component | `components/accounts/` | Inline in page | TODO-CRM001-001 |
| AccountTimeline component | `components/accounts/` | Not created | TODO-CRM001-002 |
| Phone validation | Frontend | Not enforced | TODO-CRM001-003 |
| Category validation | Service | Business rule gap | TODO-CRM001-004 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| FirstName | Not required when Category=Individual | TODO-CRM001-005 |
| LastName | Not required when Category=Individual | TODO-CRM001-006 |
| Company | Not required when Category=Organization | TODO-CRM001-007 |
| Phone | No format validation | TODO-CRM001-003 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-CRM001-001 | Extract AccountForm as reusable component | P3 | Frontend |
| TODO-CRM001-002 | Create AccountTimeline component for activity view | P2 | Frontend |
| TODO-CRM001-003 | Add phone number format validation (frontend & backend) | P2 | Validation |
| TODO-CRM001-004 | Enforce category-specific required fields in service layer | P1 | Backend |
| TODO-CRM001-005 | Require FirstName when Category=Individual | P1 | Validation |
| TODO-CRM001-006 | Require LastName when Category=Individual | P1 | Validation |
| TODO-CRM001-007 | Require Company when Category=Organization | P1 | Validation |
| TODO-CRM001-008 | Create AccountsControllerTests integration tests | P2 | Testing |
| TODO-CRM001-009 | Complete customer-crud E2E tests | P2 | Testing |
| TODO-CRM001-010 | Complete territory assignment integration | P2 | Backend |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 8, 2026 | System | Initial specification |

