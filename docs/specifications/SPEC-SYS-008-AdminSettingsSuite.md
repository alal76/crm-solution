# SPEC-SYS-008: Admin Settings Suite

> **Module:** System
> **Feature:** Admin Settings Suite
> **Version:** 1.0
> **Last Updated:** 2026-02-13
> **Status:** ⚠️ Partial

---

## 1. Business Context

### 1.1 Feature Description
Provides administrative configuration pages for system-level CRM controls, including database settings, duplicate rules, lead score rules, workflow monitoring, integrations, analytics, and AI/LLM configuration.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SYS008-SF01 | Database Settings | Backup/migrate/health | ⚠️ Partial |
| SYS008-SF02 | Duplicate Rules | Manage duplicate detection | ⚠️ Partial |
| SYS008-SF03 | Lead Score Rules | Configure scoring weights | ⚠️ Partial |
| SYS008-SF04 | Workflow Monitor | Monitor execution | ⚠️ Partial |
| SYS008-SF05 | Integrations | n8n/Zapier settings | ⚠️ Partial |
| SYS008-SF06 | Analytics Settings | Superset/Power BI | ⚠️ Partial |
| SYS008-SF07 | LLM Settings | AI provider configuration | ⚠️ Partial |
| SYS008-SF08 | Test Results | Build/test visibility | ⚠️ Partial |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Update database settings | Admin | Admin authenticated | Settings saved | ⚠️ |
| UC-002 | Configure duplicate rules | Admin | Admin authenticated | Rule active | ⚠️ |
| UC-003 | Configure lead score rules | Admin | Admin authenticated | Rules apply to leads | ⚠️ |
| UC-004 | Monitor workflow execution | Admin | Admin authenticated | Instance data visible | ⚠️ |
| UC-005 | Configure integrations | Admin | Admin authenticated | Provider settings saved | ⚠️ |
| UC-006 | Configure analytics providers | Admin | Admin authenticated | Embed/health config stored | ⚠️ |
| UC-007 | Configure AI providers | Admin | Admin authenticated | Provider config stored | ⚠️ |
| UC-008 | View test results | Admin | Admin authenticated | Results visible | ⚠️ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Page | File Path | Status | Notes |
|------|-----------|--------|------|
| DatabaseSettingsPage | CRM.Frontend/src/pages/admin/DatabaseSettingsPage.tsx | ⚠️ Partial | Uses API health/ops |
| DuplicateRulesPage | CRM.Frontend/src/pages/admin/DuplicateRulesPage.tsx | ⚠️ Partial | Admin-only |
| LeadScoreRulesPage | CRM.Frontend/src/pages/admin/LeadScoreRulesPage.tsx | ⚠️ Partial | Admin-only |
| WorkflowMonitorPage | CRM.Frontend/src/pages/admin/WorkflowMonitorPage.tsx | ⚠️ Partial | Instances + logs |
| IntegrationsSettingsPage | CRM.Frontend/src/pages/admin/IntegrationsSettingsPage.tsx | ⚠️ Partial | n8n/zapier |
| AnalyticsSettingsPage | CRM.Frontend/src/pages/admin/AnalyticsSettingsPage.tsx | ⚠️ Partial | Superset/PowerBI |
| LLMSettingsPage | CRM.Frontend/src/pages/admin/LLMSettingsPage.tsx | ⚠️ Partial | AI providers |
| TestResultsPage | CRM.Frontend/src/pages/admin/TestResultsPage.tsx | ⚠️ Partial | Test viewer |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|------|
| AdminPageHeader | CRM.Frontend/src/components/admin/AdminPageHeader.tsx | ✅ | Reused header |
| ProviderHealthCard | CRM.Frontend/src/components/admin/ProviderHealthCard.tsx | ⚠️ Partial | Used in some pages |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| databaseService | CRM.Frontend/src/services/databaseService.ts | getStatus, migrate, backup | ⚠️ Partial |
| duplicatesService | CRM.Frontend/src/services/duplicateService.ts | rules, scan | ⚠️ Partial |
| leadScoreRulesService | CRM.Frontend/src/services/leadScoreRulesService.ts | rules, preview | ⚠️ Partial |
| workflowInstanceService | CRM.Frontend/src/services/workflowInstanceService.ts | instances, logs | ⚠️ Partial |
| integrationsService | CRM.Frontend/src/services/integrationsService.ts | providers | ⚠️ Partial |
| analyticsService | CRM.Frontend/src/services/analyticsService.ts | config | ⚠️ Partial |
| aiProviderService | CRM.Frontend/src/services/aiProviderService.ts | config | ⚠️ Partial |

---

## 3. Backend Implementation

### 3.1 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| DatabaseController | CRM.Api/Controllers/DatabaseController.cs | 18 | ⚠️ Partial |
| DuplicatesController | CRM.Api/Controllers/DuplicatesController.cs | 10 | ⚠️ Partial |
| LeadScoreRulesController | CRM.Api/Controllers/LeadScoreRulesController.cs | 11 | ⚠️ Partial |
| WorkflowInstanceController | CRM.Api/Controllers/WorkflowInstanceController.cs | 27 | ⚠️ Partial |
| Integration Controllers | CRM.Api/Controllers/* | 10+ | ⚠️ Partial |
| AIAnalyticsController | CRM.Api/Controllers/AIAnalyticsController.cs | 21 | ⚠️ Partial |

### 3.2 API Endpoints (Admin)
```
GET  /api/databases/status
POST /api/databases/backup
POST /api/duplicates/scan
GET  /api/admin/leadscorerules
GET  /api/workflow-instances
GET  /api/health/providers
GET  /api/ai/providers
```

---

## 4. Tests

### 4.1 Unit Tests
| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| DatabaseControllerTests | GetStatus_ReturnsOk | Health endpoint | ❌ Not Found |
| DuplicateRulesTests | CreateRule_Saves | Duplicate rules | ❌ Not Found |

### 4.2 E2E Tests
| Test File | Test | Description | Status |
|-----------|------|-------------|--------|
| admin-settings.spec.ts | Navigate admin settings pages | Navigation coverage | ❌ Not Found |

---

## 5. Issues & Inconsistencies

| ID | Issue | Severity | Description |
|----|-------|----------|-------------|
| SYS008-ISS02 | Missing tests | Medium | Coverage gap for admin pages |

---

## 6. TODO Items

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-SYS008-001 | Add admin settings navigation E2E tests | P2 | Testing |
| TODO-SYS008-002 | Add unit tests for database/duplicate/lead-score controllers | P2 | Testing |
| TODO-SYS008-003 | Validate admin pages against API contract | P2 | Backend |
| TODO-SYS008-004 | Add missing UI empty states + loading UX | P3 | Frontend |

---

## 7. Change History

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-02-13 | 1.1 | System | Resolved SYS008-ISS01 (admin items now spec’d) |

---

**END OF SPECIFICATION**
