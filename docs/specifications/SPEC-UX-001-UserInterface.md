# Feature Specification

> **Spec ID:** SPEC-UX-001  
> **Feature:** User Interface (Overall UI)  
> **Module:** UX/UI  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ⚠️ Partial

---

## 1. Business Context

### 1.1 Feature Description
Defines the core CRM application shell, layout, navigation, theming, and shared UI patterns that provide a consistent user experience across all modules (CRM, Sales, Marketing, ITSM, Admin).

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | App Shell | Root layout, routing container, and global providers | ✅ |
| SF-002 | Navigation | Primary menu, breadcrumbs, and module routing | ✅ |
| SF-003 | Layout & Responsiveness | Layout context and responsive behavior | ✅ |
| SF-004 | Theme & Branding | Theme, color palette, and branding context | ⚠️ |
| SF-005 | Global UI Patterns | Dialog headers, empty states, toasts, loaders | ⚠️ |
| SF-006 | Accessibility Baseline | Keyboard, aria labeling, contrast targets | ❌ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Load CRM shell | Authenticated user | User authenticated | App shell renders with navigation | ✅ |
| UC-002 | Navigate between modules | Authenticated user | Navigation visible | Route updates + page loads | ✅ |
| UC-003 | Apply theme and branding | Admin/Setting | Theme config exists | Colors/branding applied | ⚠️ |
| UC-004 | See breadcrumb trail | Authenticated user | Route supports breadcrumb | Breadcrumb updates | ⚠️ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| App Shell | `CRM.Frontend/src/App.tsx` | ✅ | Root routing + providers |
| Dashboard | `CRM.Frontend/src/pages/DashboardPage.tsx` | ✅ | Default landing page |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Navigation | `CRM.Frontend/src/components/Navigation.tsx` | ✅ | Main side navigation |
| Breadcrumbs | `CRM.Frontend/src/components/Breadcrumbs.tsx` | ⚠️ | Partial routing coverage |
| Admin Header | `CRM.Frontend/src/components/admin/AdminPageHeader.tsx` | ✅ | Shared admin page header |
| Dialog Header | `CRM.Frontend/src/components/common/DialogHeader.tsx` | ✅ | Shared dialog header |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| Navigation Config | `CRM.Frontend/src/services/navigationConfigService.ts` | get/update | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Theme Color | Valid hex color format | Frontend | ⚠️ |
| Layout Mode | Allowed enum (compact/comfortable) | Frontend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| ModuleUIConfig | `CRM.Backend/src/CRM.Core/Entities/ModuleUIConfig.cs` | ✅ | Module UI flags |
| SystemSettings | `CRM.Backend/src/CRM.Core/Entities/SystemSetting.cs` | ✅ | Global settings |
| ColorPalette | `CRM.Backend/src/CRM.Core/Entities/ColorPalette.cs` | ✅ | Theme colors |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| ModuleUIConfigDto | `CRM.Backend/src/CRM.Core/Dtos/ModuleUIConfigDto.cs` | ✅ | UI module config |
| NavigationConfigDto | `CRM.Backend/src/CRM.Core/Dtos/NavigationConfigDto.cs` | ✅ | Navigation config |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| Module UI Config | `CRM.Backend/src/CRM.Core/Interfaces/IModuleUIConfigService.cs` | 12 | ✅ |
| Navigation Config | `CRM.Backend/src/CRM.Core/Interfaces/INavigationConfigService.cs` | 7 | ✅ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| ModuleUIConfigService | `CRM.Backend/src/CRM.Infrastructure/Services/ModuleUIConfigService.cs` | 12 | ✅ |
| NavigationConfigService | `CRM.Backend/src/CRM.Infrastructure/Services/NavigationConfigService.cs` | 7 | ✅ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| ModuleUIConfigController | `CRM.Backend/src/CRM.Api/Controllers/ModuleUIConfigController.cs` | 12 | ✅ |
| NavigationController | `CRM.Backend/src/CRM.Api/Controllers/NavigationController.cs` | 10 | ✅ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/navigations` | GetAll | Yes | ✅ |
| POST | `/api/navigations` | Save | Yes | ✅ |
| GET | `/api/moduleuiconfigs` | GetAll | Yes | ✅ |
| POST | `/api/moduleuiconfigs/initialize` | Initialize | Yes | ✅ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Module Name | Required, unique | Service | ⚠️ |
| Palette Name | Required, unique | Service | ⚠️ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| ModuleUIConfigs | `database/schema/000_baseline_schema.sql` | ✅ | UI module flags |
| SystemSettings | `database/schema/000_baseline_schema.sql` | ✅ | Global UI settings |
| ColorPalettes | `database/schema/000_baseline_schema.sql` | ✅ | Theme colors |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Name | VARCHAR | No | - | Unique | Name | ✅ |
| IsEnabled | BOOLEAN | No | true | - | IsEnabled | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| SystemSettings | ColorPalettes | 1:N | ThemePaletteId | ⚠️ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_ModuleUIConfigs_Name | ModuleUIConfigs | Name | NonClustered | ⚠️ |

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
| Admin Navigation | `e2e-tests/tests/admin/navigation.spec.ts` | 0 | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| SystemSettings.ThemePaletteId | ColorPalettes.Id | FK not enforced everywhere | TODO-UX001-003 |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Accessibility baseline | `CRM.Frontend/src/components/*` | Not standardized yet | TODO-UX001-001 |
| Global empty states/loading | `CRM.Frontend/src/components/common` | Inconsistent usage | TODO-UX001-002 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Theme color | No shared validation helper | TODO-UX001-004 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-UX001-001 | Define accessibility baseline for core UI components | P2 | UX/UI |
| TODO-UX001-002 | Add shared empty/loading state components and usage guidelines | P2 | UX/UI |
| TODO-UX001-003 | Enforce theme palette FK constraints in schema + API | P3 | UX/UI |
| TODO-UX001-004 | Add centralized theme color validation utilities | P3 | UX/UI |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 14, 2026 | GitHub Copilot | Initial specification |
