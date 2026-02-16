# SPEC-SALES-005: Contract Management

> **Module:** Sales  
> **Feature:** Contract Management  
> **Status:** ✅ Complete  
> **Priority:** P1  
> **Created:** 2026-02-12  
> **Last Updated:** 2026-02-12  
> **Dependencies:** SPEC-CRM-001 (Account), SPEC-CRM-003 (Opportunity), SPEC-SALES-001 (Quote)

---

## 1. Business Context

### 1.1 Overview

Contract Management provides capabilities to create, track, and manage customer contracts throughout their lifecycle, from draft creation to renewal or termination. This module supports various contract types including service agreements, licenses, subscriptions, and NDAs with automated renewal notifications and expiration tracking.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Contract Creation | Create contracts manually or from quotes/opportunities | ✅ Implemented |
| SF-002 | Contract Types | Service, License, Subscription, Support, Maintenance, NDA, Master, Amendment | ✅ Implemented |
| SF-003 | Lifecycle Management | Draft → Approval → Active → Expired/Renewed/Terminated workflow | ✅ Implemented |
| SF-004 | Renewal Management | Auto-renewal settings, renewal notices, renewal creation | ✅ Implemented |
| SF-005 | Document Management | File upload/download for contract documents | ✅ Implemented |
| SF-006 | Signature Tracking | Track signed dates and signature status | ✅ Implemented |
| SF-007 | Amendment Support | Link contracts to parent contracts for amendments | ✅ Implemented |
| SF-008 | Expiration Alerts | Track and alert on expiring contracts | ✅ Implemented |

### 1.3 Key Functionalities

| ID | Functionality | Use Cases | Status |
|----|---------------|-----------|--------|
| F-001 | CRUD Operations | Create, read, update, delete contracts | ✅ Implemented |
| F-002 | Status Workflow | Move contracts through approval workflow | ✅ Implemented |
| F-003 | Contract Renewal | Clone and renew expiring contracts | ✅ Implemented |
| F-004 | File Attachment | Upload PDF/Word contract documents | ✅ Implemented |
| F-005 | Statistics | Dashboard metrics for contract portfolio | ✅ Implemented |
| F-006 | Expiration Reports | List contracts expiring within N days | ✅ Implemented |
| F-007 | Quote Conversion | Create contract from accepted quote | ⚠️ Service only |
| F-008 | E-Signature Integration | Send for external signature via provider | ⚠️ Service only |

### 1.4 Use Cases

| UC-ID | Actor | Action | Outcome |
|-------|-------|--------|---------|
| UC-001 | Sales Rep | Creates contract from quote | Contract inherits quote values and customer |
| UC-002 | Sales Manager | Reviews and approves contract | Contract status changes to Approved |
| UC-003 | System | Detects contract nearing expiration | Renewal notice sent if configured |
| UC-004 | Account Manager | Renews active contract | New contract created, original marked Renewed |
| UC-005 | Legal | Uploads signed contract document | Document attached with metadata |
| UC-006 | Sales Rep | Terminates contract early | Contract marked Terminated with reason |
| UC-007 | System | Contract reaches end date | Status changes to Expired |
| UC-008 | Sales Rep | Creates amendment | New contract linked as child of original |

---

## 2. Frontend Implementation

### 2.1 Pages

| Page | Route | Status | Notes |
|------|-------|--------|-------|
| ContractsPage | /contracts | ❌ Not Found | List with filtering by status/type |
| ContractDetailsPage | /contracts/:id | ❌ Not Found | Detail view with timeline |
| ContractFormPage | /contracts/new | ❌ Not Found | Create/edit form |

### 2.2 Components

| Component | Location | Status | Notes |
|-----------|----------|--------|-------|
| ContractList | components/contracts/ | ❌ Not Found | Filterable data grid |
| ContractForm | components/contracts/ | ❌ Not Found | Create/edit form |
| ContractCard | components/contracts/ | ❌ Not Found | Summary card component |
| ContractTimeline | components/contracts/ | ❌ Not Found | Status history timeline |
| ContractStatusBadge | components/contracts/ | ❌ Not Found | Status indicator |
| ContractRenewalDialog | components/contracts/ | ❌ Not Found | Renewal wizard |
| ContractFileUpload | components/contracts/ | ❌ Not Found | Document upload |
| ContractExpirationWidget | components/dashboard/ | ❌ Not Found | Expiring contracts widget |

### 2.3 Services

| Service | File | Status | Notes |
|---------|------|--------|-------|
| contractService | services/contractService.ts | ❌ Not Found | API client for contracts |

### 2.4 Frontend Validations

| Field | Rule | Status |
|-------|------|--------|
| Name | Required, max 200 chars | ❌ Not Implemented |
| CustomerId | Required, must exist | ❌ Not Implemented |
| StartDate | Required, valid date | ❌ Not Implemented |
| EndDate | Required, after StartDate | ❌ Not Implemented |
| Value | Required, >= 0 | ❌ Not Implemented |
| ContractType | Required, valid enum | ❌ Not Implemented |

---

## 3. Backend Implementation

### 3.1 Entities

#### Contract Entity

| Property | Type | Constraints | Status |
|----------|------|-------------|--------|
| **Identification** | | | |
| Id | int | PK, Auto-increment | ✅ Implemented |
| ContractNumber | string | Required, Unique, Format: CON-YYYYMMDD-NNNN | ✅ Implemented |
| Name | string | Required, max 500 | ✅ Implemented |
| Description | string? | max 2000 | ✅ Implemented |
| **Status & Type** | | | |
| Status | ContractStatus | Enum, default Draft | ✅ Implemented |
| ContractType | ContractType | Enum, default Service | ✅ Implemented |
| **Relationships** | | | |
| AccountId | int | FK → Accounts | ✅ Implemented |
| Account | Account? | Navigation property | ✅ Implemented |
| ContactId | int? | FK → Contacts | ✅ Implemented |
| Contact | Contact? | Navigation property | ✅ Implemented |
| OwnerId | int? | FK → Users | ✅ Implemented |
| Owner | User? | Navigation property | ✅ Implemented |
| ParentContractId | int? | FK → Contracts (self) | ✅ Implemented |
| ParentContract | Contract? | Navigation property | ✅ Implemented |
| ChildContracts | ICollection | Navigation property | ✅ Implemented |
| OpportunityId | int? | FK → Opportunities | ✅ Implemented |
| Opportunity | Opportunity? | Navigation property | ✅ Implemented |
| QuoteId | int? | FK → Quotes | ✅ Implemented |
| Quote | Quote? | Navigation property | ✅ Implemented |
| **Dates** | | | |
| StartDate | DateTime | Required | ✅ Implemented |
| EndDate | DateTime | Required | ✅ Implemented |
| SignedDate | DateTime? | Nullable | ✅ Implemented |
| ActivatedDate | DateTime? | Nullable | ✅ Implemented |
| TerminatedDate | DateTime? | Nullable | ✅ Implemented |
| **Financial** | | | |
| Value | decimal | Required, precision 18,2 | ✅ Implemented |
| CurrencyCode | string | Default "USD", max 3 | ✅ Implemented |
| BillingFrequency | string? | Monthly/Quarterly/Annual | ✅ Implemented |
| **Renewal Settings** | | | |
| AutoRenew | bool | Default false | ✅ Implemented |
| RenewalNoticeDays | int | Default 30 | ✅ Implemented |
| RenewalNoticeSent | bool | Default false | ✅ Implemented |
| RenewalNoticeSentDate | DateTime? | Nullable | ✅ Implemented |
| **Terms & Conditions** | | | |
| Terms | string? | Text | ✅ Implemented |
| SpecialConditions | string? | Text | ✅ Implemented |
| TerminationClause | string? | Text | ✅ Implemented |
| **Documents** | | | |
| ContractFileUrl | string? | max 500 | ✅ Implemented |
| ContractFileName | string? | max 255 | ✅ Implemented |
| ContractFileSize | long? | Nullable | ✅ Implemented |
| ContractFileMimeType | string? | max 100 | ✅ Implemented |
| SignedContractFileUrl | string? | max 500 | ✅ Implemented |
| SignedContractFileName | string? | max 255 | ✅ Implemented |
| **Approval** | | | |
| ApprovedByUserId | int? | FK → Users | ✅ Implemented |
| ApprovedByUser | User? | Navigation property | ✅ Implemented |
| ApprovedDate | DateTime? | Nullable | ✅ Implemented |
| RejectionReason | string? | max 1000 | ✅ Implemented |
| **Suspension** | | | |
| IsSuspended | bool | Default false | ✅ Implemented |
| SuspendedDate | DateTime? | Nullable | ✅ Implemented |
| SuspendedReason | string? | max 1000 | ✅ Implemented |
| **Computed Properties** | | | |
| DaysUntilExpiration | int | Calculated (EndDate - Now) | ✅ Implemented |
| IsExpiringSoon | bool | Calculated (Days <= 30) | ✅ Implemented |
| **Base Entity** | | | |
| CreatedAt | DateTime | Auto-set | ✅ Implemented |
| UpdatedAt | DateTime | Auto-set | ✅ Implemented |
| IsDeleted | bool | Soft delete | ✅ Implemented |
| RowVersion | byte[] | Concurrency | ✅ Implemented |

**Total: ~50 properties**

### 3.2 Enums

#### ContractStatus

| Value | Int | Description | Status |
|-------|-----|-------------|--------|
| Draft | 0 | Initial creation state | ✅ Implemented |
| PendingApproval | 1 | Awaiting approval | ✅ Implemented |
| Approved | 2 | Approved, ready to activate | ✅ Implemented |
| Active | 3 | Currently active contract | ✅ Implemented |
| Expired | 4 | Past end date | ✅ Implemented |
| Terminated | 5 | Ended early | ✅ Implemented |
| Renewed | 6 | Replaced by renewal | ✅ Implemented |
| OnHold | 7 | Temporarily suspended | ✅ Implemented |

#### ContractType

| Value | Int | Description | Status |
|-------|-----|-------------|--------|
| Service | 0 | General service agreement | ✅ Implemented |
| License | 1 | Software/product license | ✅ Implemented |
| Subscription | 2 | Recurring subscription | ✅ Implemented |
| Support | 3 | Support agreement | ✅ Implemented |
| Maintenance | 4 | Maintenance contract | ✅ Implemented |
| NDA | 5 | Non-disclosure agreement | ✅ Implemented |
| Master | 6 | Master service agreement | ✅ Implemented |
| Amendment | 7 | Amendment to existing | ✅ Implemented |
| Other | 8 | Other contract type | ✅ Implemented |

### 3.3 DTOs

#### Request DTOs (Controller-level)

| DTO | Properties | Status |
|-----|------------|--------|
| CreateContractRequest | Name, Description, Status, ContractType, CustomerId, ContactId, OwnerId, ParentContractId, OpportunityId, QuoteId, StartDate, EndDate, SignedDate, Value, CurrencyCode, BillingFrequency, AutoRenew, RenewalNoticeDays, Terms, SpecialConditions, TerminationClause | ✅ Implemented |
| UpdateContractRequest | All fields nullable for partial updates | ✅ Implemented |
| RejectContractRequest | Reason | ✅ Implemented |
| TerminateContractRequest | Reason | ✅ Implemented |
| RenewContractRequest | NewStartDate, NewEndDate, NewValue | ✅ Implemented |

### 3.4 Service Interface

**File:** `CRM.Core/Interfaces/IContractService.cs`  
**Status:** ✅ Implemented (~210 lines)

#### CRUD Operations

| Method | Signature | Status |
|--------|-----------|--------|
| GetAllAsync | (int? customerId, ContractStatus? status, CancellationToken) → IEnumerable<Contract> | ✅ Implemented |
| GetByIdAsync | (int id, CancellationToken) → Contract? | ✅ Implemented |
| GetByContractNumberAsync | (string contractNumber, CancellationToken) → Contract? | ✅ Implemented |
| CreateAsync | (Contract contract, CancellationToken) → Contract | ✅ Implemented |
| UpdateAsync | (Contract contract, CancellationToken) → Contract | ✅ Implemented |
| DeleteAsync | (int id, CancellationToken) → bool | ✅ Implemented |

#### Contract Operations

| Method | Signature | Status |
|--------|-----------|--------|
| CreateFromQuoteAsync | (int quoteId, CancellationToken) → Contract | ✅ Implemented |
| CreateFromOrderAsync | (int orderId, CancellationToken) → Contract | ✅ Implemented |
| GenerateContractNumberAsync | (CancellationToken) → string | ✅ Implemented |
| CloneForRenewalAsync | (int contractId, CancellationToken) → Contract | ✅ Implemented |

#### Status Management

| Method | Signature | Status |
|--------|-----------|--------|
| UpdateStatusAsync | (int contractId, ContractStatus status, CancellationToken) → Contract | ✅ Implemented |
| ActivateAsync | (int contractId, CancellationToken) → Contract | ✅ Implemented |
| SuspendAsync | (int contractId, string reason, CancellationToken) → Contract | ✅ Implemented |
| TerminateAsync | (int contractId, string reason, DateTime? terminationDate, CancellationToken) → Contract | ✅ Implemented |
| ExpireAsync | (int contractId, CancellationToken) → Contract | ✅ Implemented |

#### Renewal Management

| Method | Signature | Status |
|--------|-----------|--------|
| InitiateRenewalAsync | (int contractId, CancellationToken) → Contract | ✅ Implemented |
| CompleteRenewalAsync | (int contractId, int newContractId, CancellationToken) → Contract | ✅ Implemented |
| GetContractsDueForRenewalAsync | (int withinDays, CancellationToken) → IEnumerable<Contract> | ✅ Implemented |
| GetRenewalHistoryAsync | (int contractId, CancellationToken) → IEnumerable<Contract> | ✅ Implemented |

#### Amendment Operations

| Method | Signature | Status |
|--------|-----------|--------|
| CreateAmendmentAsync | (int contractId, Contract amendment, CancellationToken) → Contract | ✅ Implemented |
| GetAmendmentsAsync | (int contractId, CancellationToken) → IEnumerable<Contract> | ✅ Implemented |

#### Signature Management

| Method | Signature | Status |
|--------|-----------|--------|
| SendForSignatureAsync | (int contractId, IEnumerable<ContractSigner> signers, CancellationToken) → Contract | ✅ Implemented |
| RecordSignatureAsync | (int contractId, string signerId, string signatureData, CancellationToken) → Contract | ✅ Implemented |
| GetSignatureStatusAsync | (int contractId, CancellationToken) → ContractSignatureStatus | ✅ Implemented |

#### Queries

| Method | Signature | Status |
|--------|-----------|--------|
| GetActiveContractsAsync | (int customerId, CancellationToken) → IEnumerable<Contract> | ✅ Implemented |
| GetExpiringContractsAsync | (DateTime fromDate, DateTime toDate, CancellationToken) → IEnumerable<Contract> | ✅ Implemented |
| GetStatisticsAsync | (DateTime? fromDate, DateTime? toDate, CancellationToken) → ContractStatistics | ✅ Implemented |
| SearchAsync | (string query, CancellationToken) → IEnumerable<Contract> | ✅ Implemented |
| GetTotalContractValueAsync | (int customerId, CancellationToken) → decimal | ✅ Implemented |

#### Document Operations

| Method | Signature | Status |
|--------|-----------|--------|
| AttachDocumentAsync | (int contractId, string documentPath, string documentType, CancellationToken) → ContractDocument | ✅ Implemented |
| GetDocumentsAsync | (int contractId, CancellationToken) → IEnumerable<ContractDocument> | ✅ Implemented |
| GenerateContractPdfAsync | (int contractId, CancellationToken) → byte[] | ✅ Implemented |

**Total: 27 methods**

### 3.5 Supporting Types (Service Layer)

| Type | Properties | Status |
|------|------------|--------|
| ContractSigner | Email, Name, Role, Order | ✅ Implemented |
| ContractSignatureStatus | ContractId, IsFullySigned, TotalSigners, SignedCount, Signers (list) | ✅ Implemented |
| SignerStatus | Email, Name, HasSigned, SignedAt | ✅ Implemented |
| ContractDocument | Id, ContractId, DocumentType, FileName, FilePath, FileSize, UploadedAt, UploadedById | ✅ Implemented |
| ContractStatistics | TotalContracts, ActiveContracts, ExpiringContracts, ExpiredContracts, PendingRenewals, TotalContractValue, ActiveContractValue, RenewalRate, AverageContractLength, ContractsByType | ✅ Implemented |

### 3.6 Service Implementation

**File:** `CRM.Infrastructure/Services/ContractService.cs`  
**Status:** ✅ Implemented (688 lines)

All 27 interface methods fully implemented with:
- Comprehensive CRUD operations with includes
- Status transition validation
- Renewal workflow with parent-child linking
- Amendment creation
- Statistics aggregation
- Expiration queries

### 3.7 Controller

**File:** `CRM.Backend/src/Services/CRM.SalesService/Controllers/ContractsController.cs`  
**Status:** ✅ Implemented (841 lines)

#### Endpoints

| Method | Route | Description | Status |
|--------|-------|-------------|--------|
| GET | /api/contracts | List with pagination/filtering | ✅ Implemented |
| GET | /api/contracts/{id} | Get by ID with includes | ✅ Implemented |
| POST | /api/contracts | Create contract | ✅ Implemented |
| PUT | /api/contracts/{id} | Update contract | ✅ Implemented |
| DELETE | /api/contracts/{id} | Soft delete | ✅ Implemented |
| POST | /api/contracts/{id}/approve | Approve contract | ✅ Implemented |
| POST | /api/contracts/{id}/reject | Reject with reason | ✅ Implemented |
| POST | /api/contracts/{id}/activate | Activate contract | ✅ Implemented |
| POST | /api/contracts/{id}/terminate | Terminate with reason | ✅ Implemented |
| POST | /api/contracts/{id}/renew | Create renewal | ✅ Implemented |
| POST | /api/contracts/{id}/upload | Upload document | ✅ Implemented |
| GET | /api/contracts/{id}/download | Download document | ✅ Implemented |
| GET | /api/contracts/expiring | Get expiring contracts | ✅ Implemented |
| GET | /api/contracts/statistics | Get statistics | ✅ Implemented |

**Total: 14 endpoints**

#### Query Parameters (GetAll)

| Parameter | Type | Description |
|-----------|------|-------------|
| page | int | Page number (default 1) |
| pageSize | int | Items per page (default 20) |
| search | string | Search name/number |
| customerId | int? | Filter by customer |
| status | string? | Filter by status enum name |
| contractType | string? | Filter by type enum name |
| ownerId | int? | Filter by owner |
| expiringSoon | bool | Expiring within 30 days |
| sortBy | string | Sort field |
| sortOrder | string | asc/desc |

### 3.8 Backend Validations

| Validation | Location | Status |
|------------|----------|--------|
| Name required | Controller | ✅ Implemented |
| CustomerId required | Controller | ✅ Implemented |
| StartDate required | Controller | ✅ Implemented |
| EndDate required | Controller | ✅ Implemented |
| Value required | Controller | ✅ Implemented |
| Contract exists for update | Controller | ✅ Implemented |
| Status = PendingApproval for approve/reject | Controller | ✅ Implemented |
| Status = Approved/OnHold for activate | Controller | ✅ Implemented |
| File type validation (PDF, Word) | Controller | ✅ Implemented |
| EndDate > StartDate | ❌ Not Implemented | Missing |
| Value >= 0 | ❌ Not Implemented | Missing |

---

## 4. Database

### 4.1 Tables

#### Contracts Table

| Column | Type | Constraints | Status |
|--------|------|-------------|--------|
| Id | int | PK, Identity | ✅ Exists |
| ContractNumber | varchar(50) | Unique | ✅ Exists |
| Name | varchar(500) | Not null | ✅ Exists |
| Description | text | Nullable | ✅ Exists |
| Status | int | Not null, FK enum | ✅ Exists |
| ContractType | int | Not null, FK enum | ✅ Exists |
| AccountId | int | FK → Customers | ✅ Exists |
| ContactId | int | FK → Contacts, nullable | ✅ Exists |
| OwnerId | int | FK → Users, nullable | ✅ Exists |
| ParentContractId | int | FK → Contracts (self), nullable | ✅ Exists |
| OpportunityId | int | FK → Opportunities, nullable | ✅ Exists |
| QuoteId | int | FK → Quotes, nullable | ✅ Exists |
| StartDate | datetime | Not null | ✅ Exists |
| EndDate | datetime | Not null | ✅ Exists |
| SignedDate | datetime | Nullable | ✅ Exists |
| ActivatedDate | datetime | Nullable | ✅ Exists |
| TerminatedDate | datetime | Nullable | ✅ Exists |
| Value | decimal(18,2) | Not null | ✅ Exists |
| CurrencyCode | varchar(3) | Default 'USD' | ✅ Exists |
| BillingFrequency | varchar(50) | Nullable | ✅ Exists |
| AutoRenew | bit | Default 0 | ✅ Exists |
| RenewalNoticeDays | int | Default 30 | ✅ Exists |
| RenewalNoticeSent | bit | Default 0 | ✅ Exists |
| RenewalNoticeSentDate | datetime | Nullable | ✅ Exists |
| Terms | text | Nullable | ✅ Exists |
| SpecialConditions | text | Nullable | ✅ Exists |
| TerminationClause | text | Nullable | ✅ Exists |
| ContractFileUrl | varchar(500) | Nullable | ✅ Exists |
| ContractFileName | varchar(255) | Nullable | ✅ Exists |
| ContractFileSize | bigint | Nullable | ✅ Exists |
| ContractFileMimeType | varchar(100) | Nullable | ✅ Exists |
| SignedContractFileUrl | varchar(500) | Nullable | ✅ Exists |
| SignedContractFileName | varchar(255) | Nullable | ✅ Exists |
| ApprovedByUserId | int | FK → Users, nullable | ✅ Exists |
| ApprovedDate | datetime | Nullable | ✅ Exists |
| RejectionReason | varchar(1000) | Nullable | ✅ Exists |
| IsSuspended | bit | Default 0 | ✅ Exists |
| SuspendedDate | datetime | Nullable | ✅ Exists |
| SuspendedReason | varchar(1000) | Nullable | ✅ Exists |
| CreatedAt | datetime | Default GETUTCDATE() | ✅ Exists |
| UpdatedAt | datetime | Nullable | ✅ Exists |
| IsDeleted | bit | Default 0 | ✅ Exists |
| RowVersion | timestamp | Concurrency | ✅ Exists |

### 4.2 Indexes

| Index | Columns | Type | Status |
|-------|---------|------|--------|
| PK_Contracts | Id | Primary | ✅ Exists |
| IX_Contracts_ContractNumber | ContractNumber | Unique | ✅ Exists |
| IX_Contracts_AccountId | AccountId | Foreign Key | ✅ Exists |
| IX_Contracts_OwnerId | OwnerId | Foreign Key | ✅ Exists |
| IX_Contracts_Status | Status | Index | ✅ Exists |
| IX_Contracts_EndDate | EndDate | Index | ✅ Exists |
| IX_Contracts_IsDeleted | IsDeleted | Index | ✅ Exists |

### 4.3 Foreign Keys

| FK Name | Column | References | Status |
|---------|--------|------------|--------|
| FK_Contracts_Accounts | AccountId | Customers(Id) | ✅ Exists |
| FK_Contracts_Contacts | ContactId | Contacts(Id) | ✅ Exists |
| FK_Contracts_Users_Owner | OwnerId | Users(Id) | ✅ Exists |
| FK_Contracts_Users_Approved | ApprovedByUserId | Users(Id) | ✅ Exists |
| FK_Contracts_Contracts | ParentContractId | Contracts(Id) | ✅ Exists |
| FK_Contracts_Opportunities | OpportunityId | Opportunities(Id) | ✅ Exists |
| FK_Contracts_Quotes | QuoteId | Quotes(Id) | ✅ Exists |

---

## 5. Tests

### 5.1 Unit Tests

| Test File | Test Cases | Status |
|-----------|------------|--------|
| ContractServiceTests.cs | CRUD, status transitions, renewal | ❌ Not Found |

### 5.2 Integration Tests

| Test File | Test Cases | Status |
|-----------|------------|--------|
| ContractsControllerTests.cs | API endpoint tests | ❌ Not Found |

### 5.3 E2E Tests

| Test File | Test Cases | Status |
|-----------|------------|--------|
| contracts.spec.ts | UI workflow tests | ❌ Not Found |

---

## 6. Issues & Gaps

### 6.1 Naming Inconsistencies

| Location | Current | Expected | Impact |
|----------|---------|----------|--------|
| Entity | AccountId | CustomerId (in request) | Low - aliased |
| API Request | CustomerId | AccountId | Low - controller maps |

### 6.2 Validation Gaps

| Validation | Location | Priority |
|------------|----------|----------|
| EndDate > StartDate | Backend | Medium |
| Value >= 0 | Backend | Medium |
| Status transition validation | Backend | Medium |
| Contract number format validation | Backend | Low |

### 6.3 Missing Features

| Feature | Impact | Priority |
|---------|--------|----------|
| Frontend pages/components | High | P1 |
| Frontend service layer | High | P1 |
| Unit tests | Medium | P2 |
| Integration tests | Medium | P2 |
| E2E tests | Medium | P2 |
| Bulk operations | Low | P3 |
| Export functionality | Low | P3 |
| Automated expiration job | Low | P3 |

---

## 7. TODO Items

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SALES005-001 | Create ContractsPage.tsx frontend page | P1 | Frontend |
| TODO-SALES005-002 | Create ContractDetailsPage.tsx frontend page | P1 | Frontend |
| TODO-SALES005-003 | Create ContractForm.tsx component | P1 | Frontend |
| TODO-SALES005-004 | Create contractService.ts API client | P1 | Frontend |
| TODO-SALES005-005 | Add EndDate > StartDate validation | P2 | Validation |
| TODO-SALES005-006 | Add Value >= 0 validation | P2 | Validation |
| TODO-SALES005-007 | Add status transition validation rules | P2 | Validation |
| TODO-SALES005-008 | Create ContractServiceTests.cs | P2 | Testing |
| TODO-SALES005-009 | Create ContractsControllerTests.cs | P2 | Testing |
| TODO-SALES005-010 | Create contracts.spec.ts E2E tests | P2 | Testing |
| TODO-SALES005-011 | Create ContractRenewalDialog component | P2 | Frontend |
| TODO-SALES005-012 | Create ContractExpirationWidget for dashboard | P2 | Frontend |
| TODO-SALES005-013 | Add bulk status update operations | P3 | Backend |
| TODO-SALES005-014 | Add contract export (PDF, Excel) | P3 | Backend |
| TODO-SALES005-015 | Implement automated expiration background job | P3 | Backend |
| TODO-SALES005-016 | Add contract versioning/change history | P3 | Backend |

---

## 8. Change History

| Date | Author | Changes |
|------|--------|---------|
| 2026-02-12 | AI Assistant | Initial specification created |

---

## 9. References

- [Contract.cs](../../CRM.Backend/src/CRM.Core/Entities/Contract.cs) - Entity definition
- [IContractService.cs](../../CRM.Backend/src/CRM.Core/Interfaces/IContractService.cs) - Service interface
- [ContractService.cs](../../CRM.Backend/src/CRM.Infrastructure/Services/ContractService.cs) - Service implementation
- [ContractsController.cs](../../CRM.Backend/src/Services/CRM.SalesService/Controllers/ContractsController.cs) - API controller
- [PHASE4_SERVICE_SPECIFICATIONS.md](../PHASE4_SERVICE_SPECIFICATIONS.md) - Service specifications
- [DATABASE_SCHEMA.md](../../database/DATABASE_SCHEMA.md) - Database schema reference
