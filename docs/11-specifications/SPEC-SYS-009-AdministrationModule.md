# SPEC-SYS-009: Administration Module

> **Module:** System
> **Feature:** Administration Module
> **Version:** 1.0
> **Last Updated:** 2026-02-13
> **Status:** ⚠️ Partial

---

## 1. Business Context

### 1.1 Feature Description
Unified administration module that groups system, user, CRM, workflow, integration, analytics, and AI configuration into a consistent, role-aware settings experience with auditability and provider-aware navigation.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SYS009-SF01 | Admin Navigation | Admin categories + subcategories | ✅ |
| SYS009-SF02 | System Settings | Security, feature flags, database | ⚠️ Partial |
| SYS009-SF03 | User & Group Admin | Users, groups, approvals | ⚠️ Partial |
| SYS009-SF04 | CRM Admin | Branding, modules, duplicates, lead scoring | ⚠️ Partial |
| SYS009-SF05 | Service Desk Admin | Service request definitions | ⚠️ Partial |
| SYS009-SF06 | Workflow & Dashboard Admin | Workflows, monitors, dashboards | ⚠️ Partial |
| SYS009-SF07 | Integrations & Analytics | n8n, analytics providers | ⚠️ Partial |
| SYS009-SF08 | AI/LLM Administration | Provider configuration, agents | ⚠️ Partial |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Configure navigation | Admin | Admin authenticated | Nav config saved | ⚠️ |
| UC-002 | Manage users/groups | Admin | Admin authenticated | Users updated | ⚠️ |
| UC-003 | Update security settings | Admin | Admin authenticated | Policies applied | ⚠️ |
| UC-004 | Manage workflows | Admin | Admin authenticated | Workflow updated | ⚠️ |
| UC-005 | Configure AI providers | Admin | Admin authenticated | Provider config stored | ⚠️ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Page | File Path | Status | Notes |
|------|-----------|--------|------|
| NavigationSettingsPage | CRM.Frontend/src/pages/admin/NavigationSettingsPage.tsx | ✅ | Admin navigation editor |
| UserManagementSettingsPage | CRM.Frontend/src/pages/admin/UserManagementSettingsPage.tsx | ⚠️ Partial | Users CRUD |
| GroupManagementPage | CRM.Frontend/src/pages/admin/GroupManagementPage.tsx | ⚠️ Partial | Groups CRUD |
| UserApprovalPage | CRM.Frontend/src/pages/admin/UserApprovalPage.tsx | ⚠️ Partial | Approvals |
| SecuritySettingsPage | CRM.Frontend/src/pages/admin/SecuritySettingsPage.tsx | ⚠️ Partial | Policies |
| FeatureManagementPage | CRM.Frontend/src/pages/admin/FeatureManagementPage.tsx | ⚠️ Partial | Flags |
| DatabaseSettingsPage | CRM.Frontend/src/pages/admin/DatabaseSettingsPage.tsx | ⚠️ Partial | DB ops |
| BrandingSettingsPage | CRM.Frontend/src/pages/admin/BrandingSettingsPage.tsx | ⚠️ Partial | Branding |
| ModuleFieldSettingsPage | CRM.Frontend/src/pages/admin/ModuleFieldSettingsPage.tsx | ⚠️ Partial | Fields |
| DuplicateRulesPage | CRM.Frontend/src/pages/admin/DuplicateRulesPage.tsx | ⚠️ Partial | Duplicate rules |
| LeadScoreRulesPage | CRM.Frontend/src/pages/admin/LeadScoreRulesPage.tsx | ⚠️ Partial | Lead scoring |
| ServiceRequestDefinitionsPage | CRM.Frontend/src/pages/admin/ServiceRequestDefinitionsPage.tsx | ⚠️ Partial | SR definitions |
| WorkflowListPage | CRM.Frontend/src/pages/admin/WorkflowListPage.tsx | ⚠️ Partial | Workflow list |
| WorkflowDesignerPage | CRM.Frontend/src/pages/admin/WorkflowDesignerPage.tsx | ⚠️ Partial | Designer |
| WorkflowMonitorPage | CRM.Frontend/src/pages/admin/WorkflowMonitorPage.tsx | ⚠️ Partial | Monitor |
| DashboardSettingsPage | CRM.Frontend/src/pages/admin/DashboardSettingsPage.tsx | ⚠️ Partial | Dashboard config |
| IntegrationsSettingsPage | CRM.Frontend/src/pages/admin/IntegrationsSettingsPage.tsx | ⚠️ Partial | Integrations |
| AnalyticsSettingsPage | CRM.Frontend/src/pages/admin/AnalyticsSettingsPage.tsx | ⚠️ Partial | Analytics |
| LLMSettingsPage | CRM.Frontend/src/pages/admin/LLMSettingsPage.tsx | ⚠️ Partial | AI settings |
| TestResultsPage | CRM.Frontend/src/pages/admin/TestResultsPage.tsx | ⚠️ Partial | Test viewer |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|------|
| AdminPageHeader | CRM.Frontend/src/components/admin/AdminPageHeader.tsx | ✅ | Shared header |
| Navigation | CRM.Frontend/src/components/Navigation.tsx | ⚠️ Partial | Admin categories + RBAC |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| navigationConfigService | CRM.Frontend/src/services/navigationConfigService.ts | config + provider status | ⚠️ Partial |
| userService | CRM.Frontend/src/services/userService.ts | CRUD | ⚠️ Partial |
| systemSettingsService | CRM.Frontend/src/services/systemSettingsService.ts | update settings | ⚠️ Partial |
| workflowInstanceService | CRM.Frontend/src/services/workflowInstanceService.ts | instances, logs | ⚠️ Partial |

### 2.4 Page-to-API Contract Map

| Page | Primary API Calls | Notes |
|------|-------------------|-------|
| NavigationSettingsPage | GET /api/navigation/config, PUT /api/systemsettings/navigation/order | Persist nav order + categories |
| UserManagementSettingsPage | GET/POST/PUT/DELETE /api/users | CRUD + pagination |
| GroupManagementPage | GET/POST/PUT/DELETE /api/usergroups | Group permissions |
| UserApprovalPage | GET/POST /api/user-approvals | Approval flow |
| SecuritySettingsPage | GET/PUT /api/systemsettings | Password/2FA policies |
| FeatureManagementPage | GET /api/admin/features | Feature flags view |
| DatabaseSettingsPage | GET /api/databases/status, POST /api/databases/* | Backup/migrate/optimize |
| BrandingSettingsPage | GET/PUT /api/systemsettings | Brand assets |
| ModuleFieldSettingsPage | GET/PUT /api/modulefieldconfigurations | Module field visibility |
| DuplicateRulesPage | GET/POST/DELETE /api/duplicates/rules | Rule CRUD |
| LeadScoreRulesPage | GET/POST/PUT /api/admin/leadscorerules | Scoring rules |
| ServiceRequestDefinitionsPage | GET/POST/PUT /api/service-request-settings/* | SR categories/types |
| WorkflowListPage | GET /api/workflows, POST /api/workflows | Workflow CRUD |
| WorkflowDesignerPage | GET /api/workflows/versions/{id}, POST /api/workflows/versions/{id}/nodes | Designer graph |
| WorkflowMonitorPage | GET /api/workflow-instances, GET /api/workflow-instances/{id}/logs | Instance tracking |
| DashboardSettingsPage | GET/POST/PUT /api/dashboard-config/* | Widgets/layout |
| IntegrationsSettingsPage | GET/POST /api/integrations | Provider config |
| AnalyticsSettingsPage | GET/POST /api/analytics/providers | BI config |
| LLMSettingsPage | GET/PUT /api/ai/providers | AI provider config |
| TestResultsPage | GET /api/test-results | CI/test summaries |

### 2.5 Frontend Validations

| Area | Validation | Rule | Status |
|------|------------|------|--------|
| Navigation | Category IDs unique | No duplicates | ⚠️ Partial |
| Navigation | Admin item must have subcategory | Required when category=admin | ✅ |
| Security | Password policy min length | >= 8 | ⚠️ Partial |
| Database | Backup name required | Non-empty | ⚠️ Partial |
| Integrations | Provider API key | Required for external providers | ⚠️ Partial |

---

## 3. Backend Implementation

### 3.1 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| NavigationController | CRM.Api/Controllers/NavigationController.cs | 6 | ⚠️ Partial |
| SystemSettingsController | CRM.Api/Controllers/SystemSettingsController.cs | 19 | ⚠️ Partial |
| UsersController | CRM.Api/Controllers/UsersController.cs | 13 | ⚠️ Partial |
| UserGroupsController | CRM.Api/Controllers/UserGroupsController.cs | 8 | ⚠️ Partial |
| AuthController | CRM.Api/Controllers/AuthController.cs | 15 | ⚠️ Partial |
| FeaturesController | CRM.Api/Controllers/FeaturesController.cs | 4 | ⚠️ Partial |
| DatabaseController | CRM.Api/Controllers/DatabaseController.cs | 18 | ⚠️ Partial |
| WorkflowController | CRM.Api/Controllers/WorkflowController.cs | 32 | ⚠️ Partial |

### 3.2 API Endpoints (Admin)
```
GET  /api/navigation/config
GET  /api/navigation/config/user
GET  /api/navigation/items
GET  /api/navigation/provider-status
PUT  /api/systemsettings/navigation/order
GET  /api/admin/features
GET  /api/databases/status
GET  /api/workflows
```

### 3.3 Backend Validations

| Area | Validation | Rule | Status |
|------|------------|------|--------|
| Navigation | Nav item id unique | No duplicates | ⚠️ Partial |
| Navigation | Admin subcategory valid | Must exist | ⚠️ Partial |
| Security | Jwt secret length | >= 32 chars | ✅ |
| Database | Backup path allowed | Whitelisted paths | ⚠️ Partial |
| Workflow | Role required | Admin only | ✅ |

---

## 4. Database

### 4.1 Tables
| Table | Description |
|-------|-------------|
| SystemSettings | Stores NavOrderConfig, feature flags, system settings |
| UserGroups | Admin permissions and menu access |

---

## 5. Tests

### 5.1 Unit Tests
| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| NavigationConfigServiceTests | BuildNavItems_IncludesAdmin | Default items enforced | ❌ Not Found |
| SystemSettingsControllerTests | UpdateNavigationOrder_Ok | Nav order update | ❌ Not Found |

### 5.2 E2E Tests
| Test File | Test | Description | Status |
|-----------|------|-------------|--------|
| admin-settings.spec.ts | Admin nav and page loads | Admin coverage | ❌ Not Found |

### 5.3 Acceptance Criteria

- Admin users can see all admin categories and items.
- Non-admin users cannot see admin categories.
- Navigation updates persist and survive reload.
- Workflow admin pages load without console errors.
- Provider-aware items appear when providers are enabled.

---

## 6. Issues & Inconsistencies

| ID | Issue | Severity | Description |
|----|-------|----------|-------------|
| SYS009-ISS01 | Incomplete admin test coverage | Medium | Missing admin flow tests |
| SYS009-ISS02 | Provider-aware nav not fully merged | Medium | Dynamic config not fully applied |

---

## 7. TODO Items

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-SYS009-001 | Add admin settings end-to-end tests | P2 | Testing |
| TODO-SYS009-002 | ✅ Add unit tests for navigation + system settings | P2 | Testing — NavigationControllerTests (11 tests), SystemSettingsControllerTests (8 tests) |
| TODO-SYS009-003 | Complete provider-aware navigation merge | P2 | Frontend |
| TODO-SYS009-004 | Add audit logging for admin changes | P3 | Backend |

---

## 8. Change History

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-02-13 | 1.0 | System | Initial specification |

---

**END OF SPECIFICATION**
