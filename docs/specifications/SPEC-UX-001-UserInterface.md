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
| SF-005 | Logo Management | Software logo, user-uploadable branding logo, browser tab icon | ❌ |
| SF-006 | Solution Naming | Custom solution name for end-user branding in header/tabs | ❌ |
| SF-007 | Global UI Patterns | Dialog headers, empty states, toasts, loaders | ⚠️ |
| SF-008 | Accessibility Baseline | Keyboard, aria labeling, contrast targets | ❌ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Load CRM shell | Authenticated user | User authenticated | App shell renders with navigation | ✅ |
| UC-002 | Navigate between modules | Authenticated user | Navigation visible | Route updates + page loads | ✅ |
| UC-003 | Apply theme and branding | Admin/Setting | Theme config exists | Colors/branding applied | ⚠️ |
| UC-004 | See breadcrumb trail | Authenticated user | Route supports breadcrumb | Breadcrumb updates | ⚠️ |
| UC-005 | Upload custom logo | Admin/Setting | Admin access to branding settings | Logo stored and applied to UI | ❌ |
| UC-006 | View software logo | Any user | App loaded | CRM software logo displays in header/footer | ✅ |
| UC-007 | Customize solution name | Admin/Setting | Admin access to branding settings | Custom solution name appears in header/tabs | ❌ |
| UC-008 | View branded browser tab | Any user | Browser tab visible | Custom logo and solution name in browser tab | ❌ |

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
| Logo Display | `CRM.Frontend/src/components/common/LogoDisplay.tsx` | ❌ | Software + user-uploaded logo display |
| BrandingSettings | `CRM.Frontend/src/components/admin/BrandingSettings.tsx` | ❌ | Admin interface for logo/solution name |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| Navigation Config | `CRM.Frontend/src/services/navigationConfigService.ts` | get/update | ✅ |
| Branding Config | `CRM.Frontend/src/services/brandingConfigService.ts` | get/update/uploadLogo | ❌ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Theme Color | Valid hex color format | Frontend | ⚠️ |
| Layout Mode | Allowed enum (compact/comfortable) | Frontend | ❌ |
| Custom Logo | File type (PNG/JPG), max size 2MB, dimensions 200x200 to 500x500px | Frontend | ❌ |
| Solution Name | Required, max 100 characters, alphanumeric + spaces | Frontend | ❌ |
| Favicon | ICO/PNG format, 32x32 or 64x64px | Frontend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| ModuleUIConfig | `CRM.Backend/src/CRM.Core/Entities/ModuleUIConfig.cs` | ✅ | Module UI flags |
| SystemSettings | `CRM.Backend/src/CRM.Core/Entities/SystemSetting.cs` | ✅ | Global settings |
| ColorPalette | `CRM.Backend/src/CRM.Core/Entities/ColorPalette.cs` | ✅ | Theme colors |
| BrandingConfig | `CRM.Backend/src/CRM.Core/Entities/BrandingConfig.cs` | ❌ | Custom logo, solution name, favicon settings |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| ModuleUIConfigDto | `CRM.Backend/src/CRM.Core/Dtos/ModuleUIConfigDto.cs` | ✅ | UI module config |
| NavigationConfigDto | `CRM.Backend/src/CRM.Core/Dtos/NavigationConfigDto.cs` | ✅ | Navigation config |
| BrandingConfigDto | `CRM.Backend/src/CRM.Core/Dtos/BrandingConfigDto.cs` | ❌ | Custom logo, solution name, favicon URLs |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| Module UI Config | `CRM.Backend/src/CRM.Core/Interfaces/IModuleUIConfigService.cs` | 12 | ✅ |
| Navigation Config | `CRM.Backend/src/CRM.Core/Interfaces/INavigationConfigService.cs` | 7 | ✅ |
| Branding Config | `CRM.Backend/src/CRM.Core/Interfaces/IBrandingConfigService.cs` | 8 | ❌ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| ModuleUIConfigService | `CRM.Backend/src/CRM.Infrastructure/Services/ModuleUIConfigService.cs` | 12 | ✅ |
| NavigationConfigService | `CRM.Backend/src/CRM.Infrastructure/Services/NavigationConfigService.cs` | 7 | ✅ |
| BrandingConfigService | `CRM.Backend/src/CRM.Infrastructure/Services/BrandingConfigService.cs` | 8 | ❌ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| ModuleUIConfigController | `CRM.Backend/src/CRM.Api/Controllers/ModuleUIConfigController.cs` | 12 | ✅ |
| NavigationController | `CRM.Backend/src/CRM.Api/Controllers/NavigationController.cs` | 10 | ✅ |
| BrandingController | `CRM.Backend/src/CRM.Api/Controllers/BrandingController.cs` | 6 | ❌ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/navigations` | GetAll | Yes | ✅ |
| POST | `/api/navigations` | Save | Yes | ✅ |
| GET | `/api/moduleuiconfigs` | GetAll | Yes | ✅ |
| POST | `/api/moduleuiconfigs/initialize` | Initialize | Yes | ✅ |
| GET | `/api/branding` | GetBrandingConfig | Yes | ❌ |
| POST | `/api/branding/upload-logo` | UploadCustomLogo | Yes | ❌ |
| POST | `/api/branding/upload-favicon` | UploadFavicon | Yes | ❌ |
| POST | `/api/branding/solution-name` | UpdateSolutionName | Yes | ❌ |
| DELETE | `/api/branding/custom-logo` | DeleteCustomLogo | Yes | ❌ |
| DELETE | `/api/branding/favicon` | DeleteFavicon | Yes | ❌ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Module Name | Required, unique | Service | ⚠️ |
| Palette Name | Required, unique | Service | ⚠️ |
| Custom Logo | File type PNG/JPG, max 2MB, dimensions 200x200 to 500x500px | BrandingConfigService | ❌ |
| Solution Name | Required, max 100 chars, alphanumeric + spaces, no special chars | BrandingConfigService | ❌ |
| Favicon | File type ICO/PNG, max 500KB, dimensions 32x32 or 64x64px | BrandingConfigService | ❌ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| ModuleUIConfigs | `database/schema/000_baseline_schema.sql` | ✅ | UI module flags |
| SystemSettings | `database/schema/000_baseline_schema.sql` | ✅ | Global UI settings |
| ColorPalettes | `database/schema/000_baseline_schema.sql` | ✅ | Theme colors |
| BrandingConfigs | `database/schema/000_baseline_schema.sql` | ❌ | Custom logo, solution name, favicon paths |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Name | VARCHAR | No | - | Unique | Name | ✅ |
| IsEnabled | BOOLEAN | No | true | - | IsEnabled | ✅ |
| SolutionName | VARCHAR(100) | No | 'CRM Solution' | - | SolutionName | ❌ |
| CustomLogoPath | VARCHAR(500) | Yes | NULL | - | CustomLogoPath | ❌ |
| CustomLogoFileName | VARCHAR(255) | Yes | NULL | - | CustomLogoFileName | ❌ |
| FaviconPath | VARCHAR(500) | Yes | NULL | - | FaviconPath | ❌ |
| FaviconFileName | VARCHAR(255) | Yes | NULL | - | FaviconFileName | ❌ |
| SoftwareLogoPath | VARCHAR(500) | No | '/assets/logo.png' | - | SoftwareLogoPath | ❌ |

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
| BrandingConfig paths | File storage location | Storage strategy undefined | TODO-UX001-005 |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|--------|
| Accessibility baseline | `CRM.Frontend/src/components/*` | Not standardized yet | TODO-UX001-001 |
| Global empty states/loading | `CRM.Frontend/src/components/common` | Inconsistent usage | TODO-UX001-002 |
| Logo upload/storage | Backend file service | Not yet implemented | TODO-UX001-006 |
| Browser tab favicon | App.tsx head | Dynamic favicon not yet applied | TODO-UX001-007 |
| Solution name display | Header, browser tab | Dynamic solution name not yet applied | TODO-UX001-008 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Theme color | No shared validation helper | TODO-UX001-004 |
| Custom logo file | No file type/size/dimension validation | TODO-UX001-009 |
| Favicon file | No file type/size/dimension validation | TODO-UX001-010 |
| Solution name | No character/length validation | TODO-UX001-011 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-UX001-001 | Define accessibility baseline for core UI components | P2 | UX/UI |
| TODO-UX001-002 | Add shared empty/loading state components and usage guidelines | P2 | UX/UI |
| TODO-UX001-003 | Enforce theme palette FK constraints in schema + API | P3 | UX/UI |
| TODO-UX001-004 | Add centralized theme color validation utilities | P3 | UX/UI |
| TODO-UX001-005 | Define file storage strategy for custom logos and favicons (cloud, local, CDN) | P2 | UX/UI |
| TODO-UX001-006 | Implement logo upload service with file validation and storage | P2 | UX/UI |
| TODO-UX001-007 | Apply custom favicon to browser tab dynamically via meta tags | P2 | UX/UI |
| TODO-UX001-008 | Apply custom solution name to header and browser tab title | P2 | UX/UI |
| TODO-UX001-009 | Implement file validation for custom logo (type, size, dimensions) | P2 | UX/UI |
| TODO-UX001-010 | Implement file validation for favicon (type, size, dimensions) | P2 | UX/UI |
| TODO-UX001-011 | Implement character/length validation for solution name | P3 | UX/UI |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 14, 2026 | GitHub Copilot | Initial specification |
