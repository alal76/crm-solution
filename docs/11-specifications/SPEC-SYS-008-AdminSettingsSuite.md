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

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|------|
| AdminPageHeader | CRM.Frontend/src/components/admin/AdminPageHeader.tsx | ✅ | Reused header |
| ProviderHealthCard | CRM.Frontend/src/components/admin/ProviderHealthCard.tsx | ⚠️ Partial | Used in some pages |
| SalesSettingsPanel | CRM.Frontend/src/components/admin/SalesSettingsPanel.tsx | ❌ Not Implemented | Commission rates, discount rules, approval settings |
| ServiceDeskSettingsPanel | CRM.Frontend/src/components/admin/ServiceDeskSettingsPanel.tsx | ❌ Not Implemented | SLA policies, escalation rules, queue config |

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
| salesSettingsService | CRM.Frontend/src/services/salesSettingsService.ts | getCommissionRules, updateDiscounts, getApprovalConfig | ❌ Not Implemented |
| serviceDeskSettingsService | CRM.Frontend/src/services/serviceDeskSettingsService.ts | getSLAPolicies, getEscalationRules, getQueueConfig | ❌ Not Implemented |

### 2.4 Sales Module Pages
| Page | Component | Status | Dependencies | Notes |
|------|-----------|--------|--------------|-------|
| Sales Settings | SalesSettingsPage.tsx | ❌ Not Implemented | SPEC-SALES-001 | Commission rules, discount tiers, approval workflow |
| Commission Rules | CommissionRulesPage.tsx | ❌ Not Implemented | SPEC-SALES-007 | Configure rate tables, eligibility criteria |
| Discount Rules | DiscountRulesPage.tsx | ❌ Not Implemented | SPEC-SALES-001 | Volume discounts, promotional discounts, coupon settings |

### 2.5 Service Desk Module Pages
| Page | Component | Status | Dependencies | Notes |
|------|-----------|--------|--------------|-------|
| SLA Management | SLAManagementPage.tsx | ❌ Not Implemented | SPEC-SD-003 | Define SLA policies, priority-based timings |
| Escalation Rules | EscalationRulesPage.tsx | ❌ Not Implemented | SPEC-SD-005 | Configure escalation conditions, re-queue targets |
| Queue Configuration | QueueConfigPage.tsx | ❌ Not Implemented | SPEC-SD-001 | Manage support queues, skill-based routing |

---

## 3. Backend Implementation

### 3.1 Entities (New for Sales & Service Desk)

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| CommissionRule | `CRM.Core/Entities/CommissionRule.cs` | ❌ Not Implemented | Commission rate configuration |
| DiscountRule | `CRM.Core/Entities/DiscountRule.cs` | ❌ Not Implemented | Discount tier and eligibility rules |
| SLAPolicy | `CRM.Core/Entities/SLAPolicy.cs` | ❌ Not Implemented | SLA definitions with targets and escalation |
| EscalationRule | `CRM.Core/Entities/EscalationRule.cs` | ❌ Not Implemented | Service request escalation configuration |
| ServiceQueue | `CRM.Core/Entities/ServiceQueue.cs` | ❌ Not Implemented | Support queue definitions |
| SalesConfiguration | `CRM.Core/Entities/SalesConfiguration.cs` | ❌ Not Implemented | Module-level sales settings |

### 3.1.1 Sales Entities

#### CommissionRule
**Properties:**
- Id (PK)
- Name (string, required)
- Description (string)
- Type (enum: Percentage, Fixed, Tiered)
- BaseRate (decimal) — Commission percentage or amount
- MinAmount (decimal) — Minimum transaction value to qualify
- MaxAmount (decimal) — Maximum transaction value
- ApplicableProductIds (JSON array)
- ApplicableUserIds (JSON array)
- IsActive (boolean)
- CreatedAt, UpdatedAt
- RowVersion

#### DiscountRule
**Properties:**
- Id (PK)
- Name (string, required)
- Description (string)
- DiscountType (enum: Percentage, Fixed, Tiered)
- DiscountValue (decimal)
- MinQuantity (int)
- MaxQuantity (int)
- MinOrderAmount (decimal)
- PromotionalCode (string)
- ValidFrom (DateTime)
- ValidUntil (DateTime)
- ApplicableProductIds (JSON array)
- ApplicableUserIds (JSON array)
- CumulativeWithOther (boolean)
- MaxDiscountValue (decimal)
- IsActive (boolean)
- RowVersion

### 3.1.2 Service Desk Entities

#### SLAPolicy
**Properties:**
- Id (PK)
- Name (string, required)
- Description (string)
- Priority (enum: Critical, High, Medium, Low)
- InitialResponseTime (TimeSpan) — Time to first response in minutes
- ResolutionTime (TimeSpan) — Time to resolution in minutes
- WorkingHoursOnly (boolean)
- EscalationPath (string) — JSON array of user IDs in escalation order
- IsActive (boolean)
- CreatedAt, UpdatedAt
- RowVersion

#### EscalationRule
**Properties:**
- Id (PK)
- Name (string, required)
- Description (string)
- Condition (string) — JSON condition expression
- ConditionMetrics (enum: AgeMinutes, PriorityLevel, AssigneeGroup)
- ThresholdValue (int)
- EscalateToUserId (int, FK)
- EscalateToGroupId (int, FK)
- SendNotification (boolean)
- IsActive (boolean)
- RowVersion

### 3.2 DTOs (New for Sales & Service Desk)

| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| CommissionRuleDto | `CRM.Core/DTOs/CommissionRuleDto.cs` | ❌ Not Implemented | Commission rule DTO |
| DiscountRuleDto | `CRM.Core/DTOs/DiscountRuleDto.cs` | ❌ Not Implemented | Discount rule DTO |
| SLAPolicyDto | `CRM.Core/DTOs/SLAPolicyDto.cs` | ❌ Not Implemented | SLA policy DTO |
| EscalationRuleDto | `CRM.Core/DTOs/EscalationRuleDto.cs` | ❌ Not Implemented | Escalation rule DTO |
| SalesConfigurationDto | `CRM.Core/DTOs/SalesConfigurationDto.cs` | ❌ Not Implemented | Sales module configuration DTO |
| ServiceDeskConfigurationDto | `CRM.Core/DTOs/ServiceDeskConfigurationDto.cs` | ❌ Not Implemented | Service Desk module configuration DTO |

### 3.3 Interfaces (New for Sales & Service Desk)

| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| ICommissionRuleService | `CRM.Core/Interfaces/ICommissionRuleService.cs` | GetAll, GetById, Create, Update, Delete | ❌ Not Implemented |
| IDiscountRuleService | `CRM.Core/Interfaces/IDiscountRuleService.cs` | GetAll, GetById, Create, Update, Delete | ❌ Not Implemented |
| ISLAPolicyService | `CRM.Core/Interfaces/ISLAPolicyService.cs` | GetAll, GetById, Create, Update, Delete, FindByPriority | ❌ Not Implemented |
| IEscalationRuleService | `CRM.Core/Interfaces/IEscalationRuleService.cs` | GetAll, GetById, Create, Update, Delete, EvaluateRules | ❌ Not Implemented |

### 3.4 Controllers (New for Sales & Service Desk)

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| SalesSettingsController | `CRM.Api/Controllers/SalesSettingsController.cs` | GET/PUT /api/admin/settings/sales | ❌ Not Implemented |
| CommissionRulesController | `CRM.Api/Controllers/CommissionRulesController.cs` | CRUD /api/commission-rules | ❌ Not Implemented |
| DiscountRulesController | `CRM.Api/Controllers/DiscountRulesController.cs` | CRUD /api/discount-rules | ❌ Not Implemented |
| SLAPoliciesController | `CRM.Api/Controllers/SLAPoliciesController.cs` | CRUD /api/admin/sla-policies | ❌ Not Implemented |
| EscalationRulesController | `CRM.Api/Controllers/EscalationRulesController.cs` | CRUD /api/admin/escalation-rules | ❌ Not Implemented |

### 3.5 API Endpoints (New for Sales & Service Desk)

**Sales Admin Endpoints:**
```
GET    /api/admin/settings/sales — Get sales configuration
PUT    /api/admin/settings/sales — Update sales configuration
GET    /api/commission-rules — List commission rules
POST   /api/commission-rules — Create commission rule
PUT    /api/commission-rules/{id} — Update commission rule
DELETE /api/commission-rules/{id} — Delete commission rule
GET    /api/discount-rules — List discount rules
POST   /api/discount-rules — Create discount rule
PUT    /api/discount-rules/{id} — Update discount rule
DELETE /api/discount-rules/{id} — Delete discount rule
```

**Service Desk Admin Endpoints:**
```
GET    /api/admin/settings/service-desk — Get service desk configuration
GET    /api/admin/sla-policies — List SLA policies
POST   /api/admin/sla-policies — Create SLA policy
PUT    /api/admin/sla-policies/{id} — Update SLA policy
DELETE /api/admin/sla-policies/{id} — Delete SLA policy
GET    /api/admin/escalation-rules — List escalation rules
POST   /api/admin/escalation-rules — Create escalation rule
PUT    /api/admin/escalation-rules/{id} — Update escalation rule
DELETE /api/admin/escalation-rules/{id} — Delete escalation rule
POST   /api/admin/escalation-rules/{id}/evaluate — Evaluate rule against request
```

### 3.6 Controllers (Original - Updated)

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| DatabaseController | CRM.Api/Controllers/DatabaseController.cs | 18 | ⚠️ Partial |
| DuplicatesController | CRM.Api/Controllers/DuplicatesController.cs | 10 | ⚠️ Partial |
| LeadScoreRulesController | CRM.Api/Controllers/LeadScoreRulesController.cs | 11 | ⚠️ Partial |
| WorkflowInstanceController | CRM.Api/Controllers/WorkflowInstanceController.cs | 27 | ⚠️ Partial |
| Integration Controllers | CRM.Api/Controllers/* | 10+ | ⚠️ Partial |
| AIAnalyticsController | CRM.Api/Controllers/AIAnalyticsController.cs | 21 | ⚠️ Partial |

### 3.7 API Endpoints (Admin - System)
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

## 4. Database Implementation

### 4.1 Tables (New for Sales & Service Desk)

| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| CommissionRules | `database/schema/050_sales_configuration.sql` | ❌ Not Implemented | Commission rates and eligibility |
| DiscountRules | `database/schema/050_sales_configuration.sql` | ❌ Not Implemented | Discount tier configuration |
| SLAPolicies | `database/schema/050_service_desk_configuration.sql` | ❌ Not Implemented | SLA definitions |
| EscalationRules | `database/schema/050_service_desk_configuration.sql` | ❌ Not Implemented | Escalation path configuration |
| ServiceQueues | `database/schema/050_service_desk_configuration.sql` | ❌ Not Implemented | Support queue management |

---

## 5. Tests

### 5.1 Unit Tests (Original)
| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| DatabaseControllerTests | GetStatus_ReturnsOk | Health endpoint | ❌ Not Found |
| DuplicateRulesTests | CreateRule_Saves | Duplicate rules | ❌ Not Found |

### 5.2 Unit Tests (New - Sales & Service Desk)
| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| CommissionRulesControllerTests | CreateRule_Saves | Commission rule creation | ❌ Not Implemented |
| DiscountRulesControllerTests | UpdateRule_Saves | Discount rule update | ❌ Not Implemented |
| SLAPoliciesControllerTests | CreatePolicy_Saves | SLA policy creation | ❌ Not Implemented |
| EscalationRulesControllerTests | EvaluateRules_Matches | Escalation rule evaluation | ❌ Not Implemented |

### 5.3 E2E Tests
| Test File | Test | Description | Status |
|-----------|------|-------------|--------|
| admin-settings.spec.ts | Navigate admin settings pages | Navigation coverage | ❌ Not Found |
| sales-admin.spec.ts | Manage commission and discount rules | Sales configuration | ❌ Not Implemented |
| servicedesk-admin.spec.ts | Manage SLA and escalation rules | Service Desk configuration | ❌ Not Implemented |

---

## 6. Issues & Inconsistencies

| ID | Issue | Severity | Description |
|----|-------|----------|-------------|
| SYS008-ISS01 | Missing Sales/SD pages | High | Sales and Service Desk admin pages not implemented |
| SYS008-ISS02 | Missing tests | Medium | Coverage gap for admin pages |

---

## 7. TODO Items

### Original TODO Items

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-SYS008-001 | ✅ Add admin settings navigation E2E tests | P2 | Testing — 5 @smoke + 5 advanced tests in `e2e-tests/tests/admin/admin-settings.spec.ts` |
| TODO-SYS008-002 | ✅ Add unit tests for database/duplicate/lead-score controllers | P2 | Testing — AdminConfigurationControllerTests (11), LeadScoreRulesControllerTests (11), DuplicateDetectionControllerTests (8) |
| TODO-SYS008-003 | Validate admin pages against API contract | P2 | Backend |
| TODO-SYS008-004 | Add missing UI empty states + loading UX | P3 | Frontend |

### New TODO Items - Sales Admin

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-SYS008-005 | Implement CommissionRule entity and service | P1 | Backend |
| TODO-SYS008-006 | Implement DiscountRule entity and service | P1 | Backend |
| TODO-SYS008-007 | Create SalesSettingsController with commission/discount endpoints | P1 | Backend |
| TODO-SYS008-008 | Implement commission rule calculator service | P2 | Backend |
| TODO-SYS008-009 | Create SalesSettingsPage React component | P1 | Frontend |
| TODO-SYS008-010 | Create CommissionRulesPanel React component | P1 | Frontend |
| TODO-SYS008-011 | Create DiscountRulesPanel React component | P1 | Frontend |
| TODO-SYS008-012 | Integrate SalesSettingsPage into admin navigation | P2 | Frontend |
| TODO-SYS008-013 | Add sales settings E2E tests | P2 | Testing |
| TODO-SYS008-014 | ✅ Add commission rule unit tests | P2 | Testing — CommissionRulesEngineTests (13 tests: ApplyCap, CalculateSplit, CalculateTieredCommission) |

### New TODO Items - Service Desk Admin

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-SYS008-015 | Implement SLAPolicy entity and service | P1 | Backend |
| TODO-SYS008-016 | Implement EscalationRule entity and service | P1 | Backend |
| TODO-SYS008-017 | Implement ServiceQueue entity and service | P1 | Backend |
| TODO-SYS008-018 | Create SLAPoliciesController endpoint | P1 | Backend |
| TODO-SYS008-019 | Create EscalationRulesController endpoint | P1 | Backend |
| TODO-SYS008-020 | Implement SLA matching service for service requests | P2 | Backend |
| TODO-SYS008-021 | Create SLAManagementPage React component | P1 | Frontend |
| TODO-SYS008-022 | Create EscalationRulesPanel React component | P1 | Frontend |
| TODO-SYS008-023 | Create QueueConfigPanel React component | P1 | Frontend |
| TODO-SYS008-024 | Integrate Service Desk admin pages into navigation | P2 | Frontend |
| TODO-SYS008-025 | Add SLA policy E2E tests | P2 | Testing |
| TODO-SYS008-026 | Add escalation rule unit tests | P2 | Testing |

---

## 8. Change History

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-02-13 | 1.1 | System | Resolved SYS008-ISS01 (admin items now spec'd) |
| 2026-02-15 | 1.2 | System | Added Sales and Service Desk admin configuration sections |

---

**END OF SPECIFICATION**

| ID | Issue | Severity | Description |
|----|-------|----------|-------------|
| SYS008-ISS02 | Missing tests | Medium | Coverage gap for admin pages |

---

## 6. TODO Items

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-SYS008-001 | ✅ Add admin settings navigation E2E tests | P2 | Testing — 5 @smoke + 5 advanced tests added |
| TODO-SYS008-002 | ✅ Add unit tests for database/duplicate/lead-score controllers | P2 | Testing — AdminConfigurationControllerTests (11), DuplicateDetectionControllerTests (8) |
| TODO-SYS008-003 | Validate admin pages against API contract | P2 | Backend |
| TODO-SYS008-004 | Add missing UI empty states + loading UX | P3 | Frontend |

---

## 7. Change History

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-02-13 | 1.1 | System | Resolved SYS008-ISS01 (admin items now spec’d) |

---

**END OF SPECIFICATION**
