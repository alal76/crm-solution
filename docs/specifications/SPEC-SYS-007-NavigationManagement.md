# SPEC-SYS-007: Navigation Management

> **Module:** System
> **Feature:** Navigation Management
> **Version:** 1.2
> **Last Updated:** 2026-02-13
> **Status:** ✅ Implemented

---

## 1. Business Context

### 1.1 Feature Description
Provides configurable, role-aware navigation with admin subcategories and provider-aware items. Ensures critical admin items (Workflows, Workflow Monitor, LLM Settings, Integrations) are always present in nav config, even when local or remote configuration is missing.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SYS007-SF01 | Static Navigation | Default nav items and categories | ✅ Implemented |
| SYS007-SF02 | Dynamic Navigation | Backend-provided nav config | ✅ Implemented |
| SYS007-SF03 | Auto-Heal Config | Merge defaults into saved config | ✅ Implemented |
| SYS007-SF04 | Admin Subcategories | Collapsible admin sections | ✅ Implemented |
| SYS007-SF05 | RBAC Visibility | Filter by permissions and module flags | ✅ Implemented |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Admin updates nav order | Admin | Admin authenticated | Nav order saved | ✅ |
| UC-002 | User loads nav on login | User | Authenticated | Visible items rendered | ✅ |
| UC-003 | Missing admin items auto-heal | Admin | Saved config missing items | Required items appear | ✅ |
| UC-004 | Provider-aware nav load | Admin | Provider config available | Items reflect provider status | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| NavigationSettingsPage | CRM.Frontend/src/pages/admin/NavigationSettingsPage.tsx | ✅ | Admin UI wrapper |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Navigation | CRM.Frontend/src/components/Navigation.tsx | ✅ | Auto-heal + dynamic config + RBAC filtering |
| NavigationSettingsTab | CRM.Frontend/src/components/settings/NavigationSettingsTab.tsx | ✅ | Edit categories + items |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| navigationConfigService | CRM.Frontend/src/services/navigationConfigService.ts | getNavigationConfig, getNavigationItems, getProviderStatus | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Nav Item Label | Required, 2–64 chars | Frontend | ✅ |
| Nav Item Path | Required, must be route or URL | Frontend | ✅ |
| Category ID | Required, unique | Frontend | ✅ |
| Admin Subcategory | Required for admin items | Frontend | ✅ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| SystemSettings | CRM.Core/Entities/SystemSettings.cs | ✅ | Stores NavOrderConfig |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| NavigationConfig | CRM.Core/Dtos/NavigationConfigDto.cs | ✅ | Provider-aware config |
| NavigationItemConfig | CRM.Core/Dtos/NavigationConfigDto.cs | ✅ | Includes menuName, provider flags |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| INavigationConfigService | CRM.Core/Interfaces/INavigationConfigService.cs | 6 | ✅ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| NavigationConfigService | CRM.Infrastructure/Services/NavigationConfigService.cs | 10+ | ✅ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| NavigationController | CRM.Api/Controllers/NavigationController.cs | 6 | ✅ |
| SystemSettingsController | CRM.Api/Controllers/SystemSettingsController.cs | 1 (Nav update) | ✅ |

### 3.6 API Endpoints
```
GET  /api/navigation/config
GET  /api/navigation/config/user
GET  /api/navigation/permissions
GET  /api/navigation/items
GET  /api/navigation/items/user
GET  /api/navigation/external-services
GET  /api/navigation/provider-status
POST /api/navigation/cache/invalidate
PUT  /api/systemsettings/navigation/order
```

---

## 4. Database

### 4.1 Tables

#### SystemSettings
| Column | Type | Description |
|--------|------|-------------|
| NavOrderConfig | TEXT | JSON config for categories and items |

---

## 5. Tests

### 5.1 Unit Tests
| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| NavigationConfigServiceTests | BuildNavItems_IncludesWorkflowItems | Default items enforced | ✅ |

### 5.2 Integration Tests
| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| NavigationControllerTests | GetConfig_ReturnsItems | Config endpoint | ✅ |

---

## 6. Issues & Inconsistencies

| ID | Issue | Severity | Description |
|----|-------|----------|-------------|
| SYS007-ISS01 | Dynamic config not merged | Medium | ✅ Resolved (dynamic config now applied in UI) |
| SYS007-ISS02 | Missing tests | Medium | ✅ Resolved (navigation tests added) |

---

## 7. TODO Items

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-SYS007-004 | Add audit logging for nav changes | P3 | Backend | ✅ |

---

## 8. Change History

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-02-13 | 1.0 | System | Initial specification |
| 2026-02-13 | 1.2 | System | Completed DTOs, interface, service, controller for navigation |
| 2026-02-13 | 1.3 | System | Added navigation tests and audit logging |

---

**END OF SPECIFICATION**
