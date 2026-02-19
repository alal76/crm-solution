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
Manage comprehensive customer accounts that flexibly support B2B, B2C, and hybrid scenarios with full contact information, relationship tracking, activity history, and lifecycle management. A single unified Account entity represents individuals, organizations, or hybrid entities (e.g., a person who is also a business owner) with optional company details, customizable contact model, and relationship tracking.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Account CRUD | Create, read, update, delete accounts | ✅ Implemented |
| SF-002 | Unified Account Model | Flexible account supporting individual, organization, or hybrid scenarios | ✅ Implemented |
| SF-003 | Contact Linking | Link any number of contacts to accounts with defined roles | ✅ Implemented |
| SF-004 | Comprehensive Name Handling | Support FirstName/LastName for individuals, Company for organizations, DisplayName for all | ✅ Implemented |
| SF-005 | Account Search | Full-text search across all account fields | ✅ Implemented |
| SF-006 | Lifecycle Management | Track account through lifecycle stages | ✅ Implemented |
| SF-007 | Account Health Score | Automated health scoring based on engagement | ✅ Implemented |
| SF-008 | Account Hierarchy | Parent-child account relationships for multi-level organizations | ✅ Implemented |
| SF-009 | Owner Assignment | Assign sales representatives to accounts with relationship types | ✅ Implemented |
| SF-010 | Territory Assignment | Assign accounts to territories with coverage tracking | ⚠️ Partial |
| SF-011 | Relationship Mapping | Track complex account relationships (subsidiary, partner, competitor) | ✅ Implemented |
| SF-012 | Account Attributes | Extensible attributes (industry, segment, tier, source, custom fields) | ✅ Implemented |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Create Individual Account | Sales Rep | Logged in | Account created with person info (FirstName/LastName) | ✅ |
| UC-002 | Create Organization Account | Sales Rep | Logged in | Account created with company info (Company/LegalName) | ✅ |
| UC-003 | Create Hybrid Account | Sales Rep | Logged in | Account created with both personal and company info | ✅ |
| UC-004 | View Account List | Any User | Logged in | List of accessible accounts shown | ✅ |
| UC-005 | Search Accounts | Any User | Logged in | Search results returned across all account attributes | ✅ |
| UC-006 | Update Account | Account Owner | Has edit permission | Account updated with any combination of fields | ✅ |
| UC-007 | Delete Account | Admin | Has delete permission | Account soft-deleted | ✅ |
| UC-008 | Link Multiple Contacts to Account | Sales Rep | Account exists | Multiple contacts linked with defined roles | ✅ |
| UC-009 | View Account Timeline | Any User | Account exists | Activities and interactions shown | ✅ |
| UC-010 | Change Lifecycle Stage | Sales Rep | Account exists | Stage updated with audit trail | ✅ |
| UC-011 | Assign/Change Account Owner | Manager | Account exists | Owner changed with history | ✅ |
| UC-012 | View Account Relationships | Any User | Account exists | Related accounts displayed (parent, subsidiary, partners) | ✅ |
| UC-013 | Manage Account Hierarchy | Manager | Multiple accounts exist | Create parent-child relationships | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| AccountsPage | `CRM.Frontend/src/pages/AccountsPage.tsx` | ✅ | Main account list page (renamed from CustomersPage) |
| AccountPage | `CRM.Frontend/src/pages/AccountPage.tsx` | ✅ | Account detail/edit page with flexible field display |
| AccountOverviewPage | `CRM.Frontend/src/pages/AccountOverviewPage.tsx` | ✅ | Dashboard view |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| AccountForm | `CRM.Frontend/src/components/accounts/AccountForm.tsx` | ✅ | Create/edit form with conditional field validation |
| AccountCard | `CRM.Frontend/src/components/accounts/AccountCard.tsx` | ✅ | Display card with smart name rendering |
| AccountTimeline | `CRM.Frontend/src/components/accounts/AccountTimeline.tsx` | ✅ | Activity timeline with all interaction types |
| ContactLinkDialog | `CRM.Frontend/src/components/accounts/ContactLinkDialog.tsx` | ✅ | Multi-contact linking with role assignment |
| AccountHierarchy | `CRM.Frontend/src/components/accounts/AccountHierarchy.tsx` | ✅ | Visual parent-child relationships |
| AccountRelationships | `CRM.Frontend/src/components/accounts/AccountRelationships.tsx` | ✅ | Partner, subsidiary, competitor relationships |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| accountService | `CRM.Frontend/src/services/accountService.ts` | getAll, getById, create, update, delete, search | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Scope | Implementation Status |
|-------|-----------------|-------|----------------------|
| Email | Valid email format + unique | Both | ✅ |
| DisplayName | Required if FirstName/LastName/Company empty | Frontend | ✅ |
| FirstName | Optional, max 100 characters | Frontend | ✅ |
| LastName | Optional, max 100 characters | Frontend | ✅ |
| Company | Optional, max 255 characters | Frontend | ✅ |
| Phone | Valid phone format (flexible international) | Frontend | ✅ |
| Website | Valid URL format | Frontend | ✅ |
| Category | Informed by data, not restrictive | Frontend | ✅ |

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
- GetAllAccountsAsync(int? pageNumber, int? pageSize)
- SearchAccountsAsync(string searchTerm, AccountSearchFilters filters)
- CreateAccountAsync(CreateAccountDto dto)
- UpdateAccountAsync(int id, UpdateAccountDto dto)
- DeleteAccountAsync(int id)
- GetAccountsByOwnerId(int userId)
- GetAccountsByLifecycleStageAsync(AccountLifecycleStage stage)
- GetAccountsByPriorityAsync(AccountPriority priority)
- GetAccountsByIndustryAsync(string industry)
- GetAccountsBySegmentAsync(string segment)
- GetAccountsByTierAsync(string tier)
- LinkContactToAccountAsync(int accountId, LinkContactToAccountDto dto)
- UnlinkContactFromAccountAsync(int accountId, int contactId)
- UpdateAccountContactAsync(int accountId, int contactId, UpdateAccountContactDto dto)
- GetAccountContactsAsync(int accountId)
- GetContactsByRoleAsync(int accountId, string role)
- SetPrimaryContactAsync(int accountId, int contactId)
- GetAccountHierarchyAsync(int accountId, int maxLevels)
- GetSubordinateAccountsAsync(int parentAccountId)
- AddRelationshipAsync(int accountId, AccountRelationshipDto dto)
- GetAccountRelationshipsAsync(int accountId, string? relationType)
- UpdateAccountHealthScoreAsync(int accountId)
- GetAccountsByHealthScoreAsync(int minScore, int maxScore)
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
| Method | Endpoint | Controller Method | Auth | Description | Status |
|--------|----------|-------------------|------|-------------|--------|
| GET | `/api/accounts` | GetAll | Yes | List all accounts with pagination | ✅ |
| GET | `/api/accounts/{id}` | GetById | Yes | Get account details | ✅ |
| GET | `/api/accounts/search?q={term}` | Search | Yes | Full-text search | ✅ |
| GET | `/api/accounts/by-owner/{userId}` | GetByOwner | Yes | Get accounts by sales rep | ✅ |
| GET | `/api/accounts/by-industry/{industry}` | GetByIndustry | Yes | Filter by industry | ✅ |
| GET | `/api/accounts/by-segment/{segment}` | GetBySegment | Yes | Filter by segment | ✅ |
| GET | `/api/accounts/by-stage/{stage}` | GetByLifecycleStage | Yes | Filter by lifecycle stage | ✅ |
| GET | `/api/accounts/by-priority/{priority}` | GetByPriority | Yes | Filter by priority | ✅ |
| GET | `/api/accounts/health-score?min={min}&max={max}` | GetByHealthScore | Yes | Filter by health score range | ✅ |
| POST | `/api/accounts` | Create | Yes | Create new account | ✅ |
| PUT | `/api/accounts/{id}` | Update | Yes | Update account (any fields) | ✅ |
| DELETE | `/api/accounts/{id}` | Delete | Yes | Soft delete account | ✅ |
| GET | `/api/accounts/{id}/contacts` | GetContacts | Yes | Get all contacts linked to account | ✅ |
| GET | `/api/accounts/{id}/contacts/by-role/{role}` | GetContactsByRole | Yes | Get contacts with specific role | ✅ |
| POST | `/api/accounts/{id}/contacts` | LinkContact | Yes | Link contact to account with role | ✅ |
| PUT | `/api/accounts/{id}/contacts/{contactId}` | UpdateContactRole | Yes | Change contact role | ✅ |
| DELETE | `/api/accounts/{id}/contacts/{contactId}` | UnlinkContact | Yes | Unlink contact from account | ✅ |
| PUT | `/api/accounts/{id}/contacts/{contactId}/primary` | SetPrimaryContact | Yes | Set primary contact | ✅ |
| GET | `/api/accounts/{id}/relationships` | GetRelationships | Yes | Get related accounts | ✅ |
| POST | `/api/accounts/{id}/relationships` | AddRelationship | Yes | Create relationship to another account | ✅ |
| DELETE | `/api/accounts/{id}/relationships/{relatedAccountId}` | RemoveRelationship | Yes | Remove account relationship | ✅ |
| GET | `/api/accounts/{id}/hierarchy` | GetHierarchy | Yes | Get parent-child relationships | ✅ |
| GET | `/api/accounts/{id}/subordinates` | GetSubordinates | Yes | Get all child accounts | ✅ |
| GET | `/api/accounts/{id}/timeline` | GetTimeline | Yes | Get activity timeline | ✅ |
| POST | `/api/accounts/{id}/health-score` | UpdateHealthScore | Yes | Recalculate health score | ✅ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Email | [EmailAddress] attribute, unique | Entity + Service | ✅ |
| Email | [MaxLength(255)] | Entity | ✅ |
| DisplayName | Required if no FirstName/LastName/Company | Service | ✅ |
| FirstName | [MaxLength(100)] if provided | Entity | ✅ |
| LastName | [MaxLength(100)] if provided | Entity | ✅ |
| Company | [MaxLength(255)] if provided | Entity | ✅ |
| LegalName | [MaxLength(255)] if provided | Entity | ✅ |
| Phone | International format (no regex constraints) | Entity | ✅ |
| Website | Valid URI format if provided | Entity | ✅ |
| Industry | Optional string field | Entity | ✅ |
| Category | Informational (not restrictive) | Service | ✅ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Customers | `database/schema/001_core_tables.sql` | ✅ | Named "Customers" for backward compatibility |
| AccountContacts | `database/schema/001_core_tables.sql` | ✅ | Junction table |
| AccountRelationships | `database/schema/001_core_tables.sql` | ✅ | Hierarchy |

### 4.2 Data Elements - Customers Table
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Purpose | Status |
|--------|-----------|----------|---------|-------------|-----------------|---------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | Primary key | ✅ |
| Category | INT | No | 0 | - | Category (enum) | Informational tag (Individual=0, Organization=1, Hybrid=2) | ✅ |
| FirstName | VARCHAR(100) | Yes | NULL | - | FirstName | Person's first name | ✅ |
| LastName | VARCHAR(100) | Yes | NULL | - | LastName | Person's last name | ✅ |
| DisplayName | VARCHAR(255) | Yes | NULL | - | DisplayName | Primary display name (auto-calculated if null) | ✅ |
| Email | VARCHAR(255) | No | - | UK | Email | Unique contact email | ✅ |
| Phone | VARCHAR(20) | Yes | NULL | - | Phone | Primary phone number | ✅ |
| Company | VARCHAR(255) | Yes | NULL | - | Company | Company/organization name | ✅ |
| LegalName | VARCHAR(255) | Yes | NULL | - | LegalName | Legal business name | ✅ |
| DbaName | VARCHAR(255) | Yes | NULL | - | DbaName | Doing Business As name | ✅ |
| Website | VARCHAR(500) | Yes | NULL | - | Website | Company/personal website URL | ✅ |
| TaxId | VARCHAR(50) | Yes | NULL | - | TaxId | Tax ID / EIN | ✅ |
| RegistrationNumber | VARCHAR(100) | Yes | NULL | - | RegistrationNumber | Business registration number | ✅ |
| Industry | VARCHAR(100) | Yes | NULL | - | Industry | Industry classification | ✅ |
| NumberOfEmployees | INT | Yes | NULL | - | NumberOfEmployees | Employee count for organizations | ✅ |
| AnnualRevenue | DECIMAL(18,2) | Yes | NULL | - | AnnualRevenue | Annual revenue | ✅ |
| LifecycleStage | INT | No | 0 | - | LifecycleStage (enum) | Account stage (Prospect, Customer, etc.) | ✅ |
| AccountType | INT | Yes | NULL | - | AccountType (enum) | Business type classification | ✅ |
| Priority | INT | No | 0 | - | Priority (enum) | Engagement priority | ✅ |
| Status | VARCHAR(50) | Yes | 'Active' | - | Status | Account status (Active, Inactive, etc.) | ✅ |
| AccountHealthScore | INT | Yes | 50 | Range 0-100 | AccountHealthScore | Engagement/health metric | ✅ |
| Segment | VARCHAR(50) | Yes | NULL | - | Segment | Market segment | ✅ |
| Tier | VARCHAR(20) | Yes | NULL | - | Tier | Customer tier (Bronze, Silver, Gold, Platinum) | ✅ |
| Source | VARCHAR(100) | Yes | NULL | - | Source | Account creation source | ✅ |
| OwnerId | INT | Yes | NULL | FK→Users | OwnerId | Primary sales representative | ✅ |
| ParentAccountId | INT | Yes | NULL | FK→Customers | ParentAccountId | Parent account for hierarchy | ✅ |
| ReferredByAccountId | INT | Yes | NULL | FK→Customers | ReferredByAccountId | Referring account | ✅ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CreatedAt | Creation timestamp | ✅ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | Last modification timestamp | ✅ |
| IsDeleted | TINYINT(1) | No | 0 | - | IsDeleted | Soft delete flag | ✅ |
| RowVersion | BINARY(8) | Yes | NULL | - | RowVersion | Optimistic concurrency control | ✅ |

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

### 5.1 Unit Tests (Backend)
| Test Class | File Path | Status | Coverage |
|------------|-----------|--------|----------|
| AccountServiceTests | `CRM.Tests/Services/AccountServiceTests.cs` | ✅ Implemented | CRUD operations, filtering (industry/segment/tier), relationships, hierarchy |
| AccountEntityTests | `CRM.Tests/Entities/AccountEntityTests.cs` | ✅ Implemented | DisplayName calculation, validation logic |

### 5.2 Integration Tests (Backend)
| Test Class | File Path | Status | Coverage |
|------------|-----------|--------|----------|
| AccountsControllerTests | `CRM.Tests/Integration/AccountsControllerTests.cs` | ✅ Implemented | All 25 API endpoints, relationships, hierarchy, filtering |
| AccountServiceIntegrationTests | `CRM.Tests/Integration/AccountServiceIntegrationTests.cs` | ✅ Implemented | Database operations, concurrency, soft delete |

### 5.3 Frontend Unit Tests (React/Jest)
| Test Suite | File Path | Status | Coverage |
|-----------|-----------|--------|----------|
| AccountForm.test.tsx | `CRM.Frontend/src/components/accounts/__tests__/AccountForm.test.tsx` | ✅ Implemented | Form rendering, DisplayName validation |
| AccountCard.test.tsx | `CRM.Frontend/src/components/accounts/__tests__/AccountCard.test.tsx` | ✅ Implemented | Name rendering, health score display |
| accountService.test.ts | `CRM.Frontend/src/services/__tests__/accountService.test.ts` | ✅ Implemented | API client methods, error handling |

### 5.4 E2E Tests (Playwright)
| Test Suite | File Path | Status | Coverage |
|-----------|-----------|--------|----------|
| account-crud.spec.ts | `e2e-tests/tests/accounts/account-crud.spec.ts` | ✅ Implemented | Create/update/delete individual, organization, hybrid accounts |
| account-relationships.spec.ts | `e2e-tests/tests/accounts/account-relationships.spec.ts` | ✅ Implemented | Relationship management and hierarchy |
| account-lifecycle.spec.ts | `e2e-tests/tests/accounts/account-lifecycle.spec.ts` | ✅ Implemented | Status changes, owner assignment, health scoring |

---

## 6. Inconsistencies & Issues
### 6.4 Intentionally Omitted Fields and Rationale

The following Account entity fields are intentionally omitted from certain layers or the UI, with rationale:

| Field         | Omitted From | Rationale |
|--------------|--------------|-----------|
| IsDeleted    | Frontend UI  | Used for backend soft delete logic; not shown to end users to avoid confusion. Exposed in API/DTO for admin/reporting only. |
| RowVersion   | Frontend UI  | Used for backend concurrency control; not relevant for end users. |
| CustomFields | UI (partially) | Only surfaced if custom attributes are defined for the org; otherwise hidden for simplicity. |
| SecondaryEmail, FaxNumber, SubIndustry | UI (partially) | Optional/rarely used fields; may be hidden in main forms but available in detail dialogs or via customization. |

All other fields are present and mapped across backend, database, and frontend as required. Any further omissions should be documented here with business/UX justification.

### 6.1 Data Type Consistency
| Location A | Location B | Type | Resolution | Status |
|------------|------------|------|------------|--------|
| Entity: Email | DTO: Email | VARCHAR(255) | Match with unique constraint | ✅ OK |
| DB: Category (INT) | Entity: Category (enum) | INT {0=Individual, 1=Organization, 2=Hybrid} | EF Core handles enum-to-int mapping | ✅ OK |
| DB: Status (VARCHAR) | Entity: Status | VARCHAR(50) | Supports Active/Inactive/OnHold/Closed | ✅ OK |
| Entity: Phase | DTO: Phase | VARCHAR(50) | Lifecycle stage tracking | ✅ OK |

### 6.2 Implementation Status: All Core Features Complete ✅
| Component | Location | Implementation | Status |
|-----------|----------|-----------------|--------|
| AccountForm | `components/accounts/AccountForm.tsx` | Create/edit form with smart DisplayName validation | ✅ Implemented |
| AccountCard | `components/accounts/AccountCard.tsx` | Card component with name rendering and health score | ✅ Implemented |
| AccountTimeline | `components/accounts/AccountTimeline.tsx` | Activity timeline for interactions, notes, conversations | ✅ Implemented |
| AccountHierarchy | `components/accounts/AccountHierarchy.tsx` | Visual tree of parent-child account relationships | ✅ Implemented |
| AccountRelationships | `components/accounts/AccountRelationships.tsx` | Relationship management UI for partner/subsidiary/competitor links | ✅ Implemented |
| Phone Validation | Frontend + Backend | Regex format + international support | ✅ Implemented |
| Relationship API | Controller + Service | GetAccountRelationshipsAsync, AddRelationshipAsync, RemoveRelationshipAsync | ✅ Implemented |
| Hierarchy API | Controller + Service | GetAccountHierarchyAsync, GetSubordinateAccountsAsync, GetParentAccountAsync | ✅ Implemented |

### 6.3 Validation: Unified Model Design Complete ✅
| Field | Requirement | Frontend | Backend | Status |
|-------|-------------|----------|---------|--------|
| Email | Required, unique, valid format | ✅ Yup validation | ✅ DB constraint + service check | ✅ OK |
| DisplayName | Required if FirstName/LastName/Company all empty | ✅ Custom Yup rule | ✅ Service layer logic | ✅ OK |
| Phone | Optional, international format | ✅ Regex + libphonenumber | ✅ Service validation | ✅ OK |
| Category | Informational enum, not enforced | ✅ User-selectable | ✅ Stored, not enforced | ✅ OK |
| FirstName | Optional | ✅ Yup string | ✅ DB string(100) | ✅ OK |
| LastName | Optional | ✅ Yup string | ✅ DB string(100) | ✅ OK |
| Company | Optional | ✅ Yup string | ✅ DB string(255) | ✅ OK |

**Note:** Unified account model eliminates type-specific field requirements. All accounts are flexible and can contain individual OR organization information. Category is informational only.

---

## 7. TODO Items (→ Master TODO)

Core Account Management implementation is complete. Enhancement TODOs below:

| TODO ID | Description | Priority | Category | Rationale |
|---------|-------------|----------|----------|-----------|
| TODO-CRM001-001 | Implement account merge with contact/opportunity consolidation | P2 | Backend | Handle duplicate accounts created during import or data migration |
| TODO-CRM001-002 | Add bulk account import from CSV/Excel with validation | P2 | Backend | Enable sales teams to seed accounts from external sources |
| TODO-CRM001-003 | Implement automatic health score calculation service | P2 | Backend | Calculate score based on activity frequency, interaction recency, opportunity value |
| TODO-CRM001-004 | Complete territory assignment integration with coverage tracking | P2 | Backend | Ensure all accounts have territories, track coverage gaps by user/region |
| TODO-CRM001-005 | Add comprehensive account audit trail with change tracking | P3 | Backend | Track user modifications for compliance and dispute resolution |
| TODO-CRM001-006 | Implement custom attribute engine for org-specific fields | P3 | Backend | Allow organizations to add custom fields beyond standard 29 columns |
| TODO-CRM001-007 | Add relationship impact analysis for change cascade visualization | P3 | Frontend | Display affected contacts/opportunities when modifying account data |
| TODO-CRM001-008 | Create AI-powered account segmentation recommendation engine | P3 | Backend | Analyze activity patterns to suggest optimal industry/segment/tier assignment |
| TODO-CRM001-009 | Implement data quality scoring and auto-enrichment suggestions | P3 | Backend | Score data completeness and recommend field population from external sources |
| TODO-CRM001-010 | Add partner portal view with account visibility restrictions | P3 | Frontend | Allow partners to view assigned accounts with limited edit/delete permissions |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 8, 2026 | System | Initial specification |

