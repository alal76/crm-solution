# SPEC-CRM-008 - Account Data Normalization

> **Spec ID:** SPEC-CRM-008  
> **Feature:** Account Data Normalization  
> **Module:** Core CRM  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ⚠️ Partial (backend + frontend complete, tests pending)

---

## 1. Business Context

### 1.1 Feature Description
Normalize Account communication preferences and addresses to eliminate denormalized fields, improve data reuse, and enable per-contact preference overrides with account defaults.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Preferences Entity | Shared preferences for Account defaults + Contact overrides | ✅ |
| SF-002 | Preferences API | CRUD endpoints for account and contact preferences | ✅ |
| SF-003 | Address Normalization | Move account address data to Address + EntityAddressLinks | ✅ |
| SF-004 | Address UI | UI components for account address management | ✅ |
| SF-005 | Preferences UI | UI for account communication preferences | ✅ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Update account default preferences | Admin/User | Account exists | Preferences saved | ✅ |
| UC-002 | Override contact preferences | User | Contact exists | Contact uses custom prefs | ✅ |
| UC-003 | Add account address | User | Account exists | Address linked to account | ✅ |
| UC-004 | Manage address UI | User | Address linked | UI supports CRUD | ❌ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| CustomersPage (Account dialog) | CRM.Frontend/src/pages/CustomersPage.tsx | ✅ | Contact Info + Preferences tabs |
| AccountsPage (Account dialog) | CRM.Frontend/src/pages/AccountsPage.tsx | ✅ | Contact Info + Preferences tabs |
| CustomerOverviewPage | CRM.Frontend/src/pages/CustomerOverviewPage.tsx | ✅ | Displays normalized primary address |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| AddressManager | CRM.Frontend/src/components/ContactInfo/AddressManager.tsx | ✅ | Used by ContactInfoPanel (Account) |
| AddressModalComponent | CRM.Frontend/src/components/common/AddressModalComponent.tsx | ✅ | Shared modal wrapper |
| Preferences Form | CRM.Frontend/src/pages/CustomersPage.tsx + AccountsPage.tsx | ✅ | Communication preferences tab |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| contactInfoService | CRM.Frontend/src/services/contactInfoService.ts | address CRUD | ⚠️ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Address.Line1 | Required | Frontend | ✅ |
| Address.City | Required | Frontend | ✅ |
| DoNotCallDate | Must be future or null | Frontend | ✅ |
| DoNotEmailDate | Must be future or null | Frontend | ✅ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Preferences | CRM.Backend/src/CRM.Core/Entities/Preferences.cs | ✅ | Shared for Account + Contact |
| Account | CRM.Backend/src/CRM.Core/Entities/Account.cs | ✅ | Preferences FK, address fields removed |
| Contact | CRM.Backend/src/CRM.Core/Models/Contact.cs | ✅ | Preferences FK + UseCustomPreferences |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| PreferencesDto | CRM.Backend/src/CRM.Core/Dtos/PreferencesDto.cs | ✅ | Includes timestamps |
| ContactPreferencesDto | CRM.Backend/src/CRM.Core/Dtos/PreferencesDto.cs | ✅ | Includes UseCustomPreferences |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IPreferencesService | CRM.Backend/src/CRM.Core/Interfaces/IPreferencesService.cs | 10 | ✅ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| PreferencesService | CRM.Backend/src/CRM.Infrastructure/Services/PreferencesService.cs | 10 | ✅ |
| AccountService | CRM.Backend/src/CRM.Infrastructure/Services/AccountService.cs | Address + preferences integration | ✅ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| PreferencesController | CRM.Backend/src/CRM.Api/Controllers/PreferencesController.cs | 8 | ✅ |
| AccountsController | CRM.Backend/src/CRM.Api/Controllers/AccountsController.cs | Address endpoints | ✅ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | /api/accounts/{accountId}/preferences | GetAccountPreferences | Yes | ✅ |
| PUT | /api/accounts/{accountId}/preferences | UpdateAccountPreferences | Yes | ✅ |
| GET | /api/contacts/{contactId}/preferences | GetContactPreferences | Yes | ✅ |
| GET | /api/contacts/{contactId}/preferences/effective | GetEffectiveContactPreferences | Yes | ✅ |
| PUT | /api/contacts/{contactId}/preferences | UpdateContactPreferences | Yes | ✅ |
| POST | /api/contacts/{contactId}/preferences/use-custom | UseCustomPreferences | Yes | ✅ |
| POST | /api/contacts/{contactId}/preferences/reset-to-account | ResetToAccountDefaults | Yes | ✅ |
| GET | /api/preferences/{id} | GetById | Yes | ✅ |
| GET | /api/accounts/{id}/addresses | GetAddresses | Yes | ✅ |
| GET | /api/accounts/{id}/addresses/primary-billing | GetPrimaryBillingAddress | Yes | ✅ |
| GET | /api/accounts/{id}/addresses/primary-shipping | GetPrimaryShippingAddress | Yes | ✅ |
| POST | /api/accounts/{id}/addresses | AddAddress | Yes | ✅ |
| PUT | /api/accounts/{id}/addresses/{addressId} | UpdateAddress | Yes | ✅ |
| DELETE | /api/accounts/{id}/addresses/{addressId} | RemoveAddress | Yes | ✅ |
| POST | /api/accounts/{id}/addresses/{addressId}/set-primary-billing | SetPrimaryBilling | Yes | ✅ |
| POST | /api/accounts/{id}/addresses/{addressId}/set-primary-shipping | SetPrimaryShipping | Yes | ✅ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| DoNotCallDate | Must be future or null | PreferencesService | ✅ |
| DoNotEmailDate | Must be future or null | PreferencesService | ✅ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Preferences | CRM.Backend/src/CRM.Infrastructure/Migrations/Auto/20260214090000_AddPreferencesEntity.cs | ✅ | New table |
| EntityAddressLinks | Existing | ✅ | Used for account addresses |
| Addresses | Existing | ✅ | Used for account addresses |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| PreferencesId | INT | Yes | NULL | FK -> Preferences | Account.PreferencesId | ✅ |
| UseCustomPreferences | BOOL | No | false | - | Contact.UseCustomPreferences | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| Customers | Preferences | N:1 | PreferencesId | ✅ |
| Contacts | Preferences | N:1 | PreferencesId | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_Preferences_Composite | Preferences | OptInEmail, OptInSms, OptInPhone, OptInPostal, PreferredContactMethod, PreferredLanguage, Timezone | NonClustered | ✅ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| PreferencesServiceTests | CRM.Backend/tests/CRM.Tests/Services/PreferencesServiceTests.cs | 5 | ✅ |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| AccountAddressServiceTests | CRM.Backend/tests/CRM.Tests/Services/AccountAddressServiceTests.cs | - | ❌ |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| account-addresses.spec.ts | e2e-tests/tests/customers/account-addresses.spec.ts | - | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| Customers.Address columns | Account entity | Columns removed in code, still in DB | Migration NormalizeAccountAddresses |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Address tests | CRM.Backend/tests/CRM.Tests/Services/AccountAddressServiceTests.cs | Not implemented | TODO-CRM008-003 |
| Address E2E | e2e-tests/tests/customers/account-addresses.spec.ts | Not implemented | TODO-CRM008-004 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Address fields | Frontend validation missing | ✅ Resolved |

---

### 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-CRM008-003 | Add account address unit tests | P2 | Tests |
| TODO-CRM008-004 | Add account address E2E tests | P3 | Tests |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-14 | System | Initial specification |
