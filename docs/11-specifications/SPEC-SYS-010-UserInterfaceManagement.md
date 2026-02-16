# Feature Specification

> **Spec ID:** SPEC-SYS-010  
> **Feature:** User Interface Management  
> **Module:** System  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ⚠️ Partial

---

## 1. Business Context

### 1.1 Feature Description
Provides administrative configuration for UI navigation, module visibility, layout defaults, and UI metadata used across the CRM. This includes navigation settings, module UI configuration, and UI-related system settings.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Navigation Config | Configure menu items and ordering | ✅ |
| SF-002 | Module UI Config | Enable/disable modules + field layouts | ✅ |
| SF-003 | System Settings UI | Configure UI-related settings | ⚠️ |
| SF-004 | Branding & Theme | Palette selection and branding fields | ⚠️ |
| SF-005 | Audit & Change History | Track UI config changes | ✅ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Reorder navigation | Admin | Access to admin settings | Navigation order updated | ✅ |
| UC-002 | Toggle module visibility | Admin | Module UI configs initialized | Module enabled/disabled | ✅ |
| UC-003 | Update theme defaults | Admin | Theme palette exists | New palette applied | ⚠️ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Navigation Settings | `CRM.Frontend/src/pages/admin/NavigationSettingsPage.tsx` | ✅ | Navigation management page |
| User Management Settings | `CRM.Frontend/src/pages/admin/UserManagementSettingsPage.tsx` | ⚠️ | Partial UI settings |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Navigation Settings Tab | `CRM.Frontend/src/components/settings/NavigationSettingsTab.tsx` | ✅ | Menu configuration |
| Module Field Settings | `CRM.Frontend/src/components/settings/ModuleFieldSettingsTabNew.tsx` | ✅ | Module UI config |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| Navigation Config | `CRM.Frontend/src/services/navigationConfigService.ts` | get/update | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Module Name | Required | Frontend | ✅ |
| Display Order | Non-negative integer | Frontend | ✅ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| NavigationConfig | `CRM.Backend/src/CRM.Core/Entities/NavigationConfig.cs` | ✅ | Menu config entity |
| ModuleUIConfig | `CRM.Backend/src/CRM.Core/Entities/ModuleUIConfig.cs` | ✅ | Module config |
| SystemSetting | `CRM.Backend/src/CRM.Core/Entities/SystemSetting.cs` | ✅ | UI settings |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| NavigationConfigDto | `CRM.Backend/src/CRM.Core/Dtos/NavigationConfigDto.cs` | ✅ | Navigation config |
| ModuleUIConfigDto | `CRM.Backend/src/CRM.Core/Dtos/ModuleUIConfigDto.cs` | ✅ | UI config |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| INavigationConfigService | `CRM.Backend/src/CRM.Core/Interfaces/INavigationConfigService.cs` | 7 | ✅ |
| IModuleUIConfigService | `CRM.Backend/src/CRM.Core/Interfaces/IModuleUIConfigService.cs` | 12 | ✅ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| NavigationConfigService | `CRM.Backend/src/CRM.Infrastructure/Services/NavigationConfigService.cs` | 7 | ✅ |
| ModuleUIConfigService | `CRM.Backend/src/CRM.Infrastructure/Services/ModuleUIConfigService.cs` | 12 | ✅ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| NavigationController | `CRM.Backend/src/CRM.Api/Controllers/NavigationController.cs` | 10 | ✅ |
| ModuleUIConfigController | `CRM.Backend/src/CRM.Api/Controllers/ModuleUIConfigController.cs` | 12 | ✅ |
| SystemSettingsController | `CRM.Backend/src/CRM.Api/Controllers/SystemSettingsController.cs` | 19 | ✅ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/navigations` | GetAll | Yes | ✅ |
| POST | `/api/navigations` | Save | Yes | ✅ |
| GET | `/api/moduleuiconfigs` | GetAll | Yes | ✅ |
| POST | `/api/moduleuiconfigs/initialize` | Initialize | Yes | ✅ |
| GET | `/api/systemsettings` | GetAll | Yes | ✅ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Module Name | Required | Service | ✅ |
| Navigation Key | Required/unique | Service | ✅ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| NavigationConfigs | `database/schema/000_baseline_schema.sql` | ✅ | Menu items |
| ModuleUIConfigs | `database/schema/000_baseline_schema.sql` | ✅ | Module flags |
| SystemSettings | `database/schema/000_baseline_schema.sql` | ✅ | UI defaults |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Key | VARCHAR | No | - | Unique | Key | ✅ |
| Value | TEXT | Yes | - | - | Value | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| ModuleUIConfigs | SystemSettings | N:1 | SystemSettingId | ⚠️ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_NavigationConfigs_Key | NavigationConfigs | Key | NonClustered | ⚠️ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| NavigationConfigServiceTests | `CRM.Backend/tests/Services/NavigationConfigServiceTests.cs` | 12 | ✅ |
| ModuleUIConfigServiceTests | `CRM.Backend/tests/CRM.Tests/Services/ModuleUIConfigServiceTests.cs` | 10 | ✅ |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| NavigationControllerTests | `CRM.Backend/tests/Controllers/NavigationControllerTests.cs` | 6 | ✅ |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| Navigation Settings | `e2e-tests/tests/admin/navigation.spec.ts` | 0 | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| NavigationConfig.SortOrder | Frontend ordering | Not validated consistently | TODO-SYS010-002 |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| UI config audit log | `CRM.Backend/src/CRM.Infrastructure` | Not implemented | TODO-SYS010-003 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Module Key | Missing centralized validation | ✅ |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| *(No pending items)* |  |  |  |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 14, 2026 | GitHub Copilot | Initial specification |
