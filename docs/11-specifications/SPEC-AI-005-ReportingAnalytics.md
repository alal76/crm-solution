# Feature Specification

> **Spec ID:** SPEC-AI-005  
> **Feature:** Reporting & Analytics  
> **Module:** AI & Analytics  
> **Version:** 1.0  
> **Last Updated:** 2026-02-17  
> **Status:** ✅ Complete

---

## 1. Business Context

### 1.1 Feature Description
Provides a two-tier analytics experience: (1) a fast, built-in dashboard for daily CRM metrics and (2) advanced reporting with external BI providers (Apache Superset/Power BI) embedded in the CRM for deep analysis. The feature also includes a report designer for ad hoc reports and scheduling.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Basic CRM Dashboard | Built-in dashboard with KPI cards, pipeline, activity widgets | ✅ |
| SF-002 | Dashboard Configuration | Admin-managed dashboards, widgets, layout, data sources | ✅ |
| SF-003 | Report Designer | UI for report definitions and ad-hoc report setup | ✅ |
| SF-004 | Reports Management | CRUD, execute, preview, export, schedules, folders | ✅ |
| SF-005 | External BI Embedding | Superset/Power BI embed via analytics provider | ✅ |
| SF-006 | Analytics Navigation | Dedicated Analytics + Reports navigation paths | ✅ |
| SF-007 | Analytics Settings | Admin page for BI providers (Superset/Power BI links) | ✅ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | View basic dashboard | Sales/User | Authenticated | Dashboard KPIs rendered | ✅ |
| UC-002 | Configure dashboard layout | Admin | Dashboard settings access | Dashboard/widgets updated | ✅ |
| UC-003 | Create a report | Analyst | Reports module enabled | Report definition saved | ✅ |
| UC-004 | Run/export a report | Analyst | Report exists | Execution result or export delivered | ✅ |
| UC-005 | Open embedded BI dashboard | User | External analytics configured | Superset/Power BI iframe loaded | ✅ |
| UC-006 | Navigate to analytics | User | Authenticated | Analytics page accessible | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| DashboardPage | CRM.Frontend/src/pages/DashboardPage.tsx | ✅ | Built-in KPIs + optional embedded analytics panel |
| AnalyticsPage | CRM.Frontend/src/pages/AnalyticsPage.tsx | ✅ | Uses dashboard stats/summary endpoints + embed |
| ReportsPage | CRM.Frontend/src/pages/ReportsPage.tsx | ✅ | Uses reportService with create/execute routing |
| DashboardSettingsPage | CRM.Frontend/src/pages/admin/DashboardSettingsPage.tsx | ✅ | Full CRUD for dashboards and widgets |
| AnalyticsSettingsPage | CRM.Frontend/src/pages/admin/AnalyticsSettingsPage.tsx | ✅ | BI provider launch/links (Superset/Power BI) |

**Navigation & Routing**
- Main navigation: Dashboard (/), Reports (/reports), Analytics (/analytics)
- Admin navigation: Analytics settings (/admin/analytics), Dashboard settings (/admin/dashboards)
- Navigation source: CRM.Frontend/src/components/Navigation.tsx (static + provider-aware)

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| AnalyticsEmbed | CRM.Frontend/src/components/common/AnalyticsEmbed.tsx | ✅ | Embeds Superset/Power BI dashboards |
| DashboardBuilder | CRM.Frontend/src/components/analytics/DashboardBuilder.tsx | ✅ | Dashboard layout builder dialog |
| ReportDesigner | CRM.Frontend/src/components/analytics/ReportDesigner.tsx | ✅ | Visual report designer UI |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| dashboardService | CRM.Frontend/src/services/dashboardService.ts | dashboards, widgets, stats, pipeline | ✅ |
| navigationConfigService | CRM.Frontend/src/services/navigationConfigService.ts | provider-aware nav | ✅ |
| reportService | CRM.Frontend/src/services/reportService.ts | CRUD/execute/export | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Report Name | Required, unique | Frontend/Backend | ✅ Implemented |
| Report Query | Required | Frontend/Backend | ✅ Implemented (columns required) |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Dashboard | CRM.Backend/src/CRM.Core/Entities/Dashboard.cs | ✅ | Core dashboard entity |
| DashboardWidget | CRM.Backend/src/CRM.Core/Entities/DashboardWidget.cs | ✅ | Widget definitions |
| ReportDefinition | CRM.Backend/src/CRM.Core/Entities/Reports/ReportDefinition.cs | ✅ | Report metadata + query |
| ReportSchedule | CRM.Backend/src/CRM.Core/Entities/Reports/ReportSchedule.cs | ✅ | Schedule metadata |
| ReportWidgetConfig | CRM.Backend/src/CRM.Core/Entities/Reports/Dashboard.cs | ✅ | Report widget configuration |
| ReportFolder | CRM.Backend/src/CRM.Core/Entities/Reports/ReportDefinition.cs | ✅ | Folder entity nested in report file |
| ReportExecution | CRM.Backend/src/CRM.Core/Entities/Reports/ReportSchedule.cs | ✅ | Execution entity defined with schedule |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| ReportDtos | CRM.Backend/src/CRM.Core/Dtos/Reports/ReportDtos.cs | ✅ | Definitions, schedules, executions, folders |
| Analytics Port DTOs | CRM.Backend/src/CRM.Core/Ports/Output/Providers/IAnalyticsPort.cs | ✅ | Dashboards, charts, embeds |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IReportService | CRM.Backend/src/CRM.Core/Interfaces/IReportService.cs | 40+ | ✅ |
| IAnalyticsPort | CRM.Backend/src/CRM.Core/Ports/Output/Providers/IAnalyticsPort.cs | 15+ | ✅ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| ReportService | CRM.Backend/src/CRM.Infrastructure/Services/ReportService.cs | 40+ | ✅ |
| ReportBuilderService | CRM.Backend/src/CRM.Infrastructure/Services/ReportBuilderService.cs | 10+ | ✅ |
| BuiltInAnalyticsProvider | CRM.Backend/src/CRM.Infrastructure/Providers/BuiltIn/BuiltInAnalyticsProvider.cs | 10+ | ✅ |
| SupersetProvider | CRM.Backend/src/CRM.Infrastructure/Providers/Superset/SupersetProvider.cs | 10+ | ✅ |
| PowerBIProvider | CRM.Backend/src/CRM.Infrastructure/Providers/PowerBI/PowerBIProvider.cs | 10+ | ✅ |
| AnalyticsProviderFactory | CRM.Backend/src/CRM.Infrastructure/Factories/AnalyticsProviderFactory.cs | 5+ | ✅ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| DashboardController | CRM.Backend/src/CRM.Api/Controllers/DashboardController.cs | 15+ | ✅ |
| DashboardConfigController | CRM.Backend/src/CRM.Api/Controllers/DashboardConfigController.cs | 15+ | ✅ |
| ReportsController | CRM.Backend/src/CRM.Api/Controllers/ReportsController.cs | 30+ | ✅ |
| AnalyticsController | CRM.Backend/src/CRM.Api/Controllers/AnalyticsController.cs | 6+ | ✅ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | /api/dashboard/stats | GetStats | Yes | ✅ |
| GET | /api/dashboard/summary | GetSummary | Yes | ✅ |
| GET | /api/dashboard/pipeline | GetPipelineSummary | Yes | ✅ |
| GET | /api/dashboard-config/dashboards | GetDashboards | Yes | ✅ |
| GET | /api/dashboard-config/dashboards/{id} | GetDashboard | Yes | ✅ |
| POST | /api/dashboard-config/dashboards | CreateDashboard | Yes | ✅ |
| PUT | /api/dashboard-config/dashboards/{id} | UpdateDashboard | Yes | ✅ |
| GET | /api/reports | GetAll | Yes | ✅ |
| POST | /api/reports | Create | Yes | ✅ |
| POST | /api/reports/{id}/execute | Execute | Yes | ✅ |
| GET | /api/reports/{id}/preview | Preview | Yes | ✅ |
| GET | /api/analytics/dashboards | GetDashboards | Yes | ✅ |
| GET | /api/analytics/dashboards/{id}/embed | GetDashboardEmbed | Yes | ✅ |
| GET | /api/analytics/charts | GetCharts | Yes | ✅ |
| GET | /api/analytics/charts/{id}/embed | GetChartEmbed | Yes | ✅ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| ReportDefinition.Name | Unique per tenant | ReportService.CreateAsync | ✅ |
| ReportDefinition.Query | Required | ReportService.CreateAsync | ✅ |
| ReportSchedule.Recipients | Non-empty for enabled schedules | ReportService.CreateScheduleAsync | ✅ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Dashboards | database/DATABASE_SCHEMA.md | ✅ | Dashboard metadata |
| DashboardWidgets | database/DATABASE_SCHEMA.md | ✅ | Widget metadata |
| ReportDefinitions | database/DATABASE_SCHEMA.md | ✅ | Report definitions |
| ReportFolders | database/DATABASE_SCHEMA.md | ✅ | Folder hierarchy |
| ReportSchedules | database/DATABASE_SCHEMA.md | ✅ | Schedule metadata |
| ReportExecutions | database/DATABASE_SCHEMA.md | ✅ | Execution history |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Dashboards.Id | INT | No | AUTO_INCREMENT | PK | Dashboard.Id | ✅ |
| DashboardWidgets.DashboardId | INT | No | - | FK | DashboardWidget.DashboardId | ✅ |
| ReportDefinitions.Name | VARCHAR | No | - | Unique | ReportDefinition.Name | ✅ |
| ReportSchedules.ReportId | INT | No | - | FK | ReportSchedule.ReportDefinitionId | ✅ |
| ReportExecutions.ReportId | INT | No | - | FK | ReportExecution.ReportDefinitionId | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| DashboardWidgets | Dashboards | N:1 | DashboardId | ✅ |
| ReportSchedules | ReportDefinitions | N:1 | ReportId | ✅ |
| ReportExecutions | ReportDefinitions | N:1 | ReportId | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_Dashboards_IsDefault | Dashboards | IsDefault | NonClustered | ✅ |
| IX_ReportDefinitions_Name | ReportDefinitions | Name | NonClustered | ✅ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ReportBuilderServiceTests | CRM.Backend/tests/CRM.Tests/Services/ReportBuilderServiceTests.cs | 10+ | ✅ |
| ReportsEnumsEntityTests | CRM.Backend/tests/Unit/Core/ReportsEnumsEntityTests.cs | 10+ | ✅ |
| AnalyticsControllerTests | CRM.Backend/tests/Controllers/AnalyticsControllerTests.cs | 2 | ✅ |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ReportsControllerTests | CRM.Backend/tests/Controllers/ReportsControllerTests.cs | 10+ | ✅ |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| Reports E2E | e2e-tests/tests/reports/*.spec.ts | 0 | ⏭️ Deferred (frontend unit coverage) |

### 5.4 Frontend Unit Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ReportDesigner validations | CRM.Frontend/src/__tests__/components/ReportDesigner.test.tsx | 3 | ✅ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| ReportExecutions table | Missing entity | Entity not defined in CRM.Core | TODO-AI005-04 |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| *(No pending items)* |  |  |  |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Report name/description length | No frontend constraints | TODO-AI005-02 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| *(No pending items)* |  |  |  |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-14 | System | Initial specification |
| 1.1 | 2026-02-14 | System | Implemented analytics endpoints, frontend wiring, and tests |
| 1.2 | 2026-02-17 | System | Implemented real report execution logic in ReportService |
| 1.3 | 2026-02-17 | System | Added frontend validation coverage for report name uniqueness and query requirements |
