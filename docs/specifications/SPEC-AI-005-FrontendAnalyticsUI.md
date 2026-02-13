# Feature Specification

> **Spec ID:** SPEC-AI-005-FE  
> **Feature:** Frontend Analytics & Reporting UI  
> **Module:** AI & Analytics  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ⚠️ Partial

---

## 1. Business Context

### 1.1 Feature Description
Provides a unified analytics UI centered on external BI providers (Apache Superset and Power BI). Users can view KPIs and embedded dashboards backed by provider APIs. Self-built report designer and custom dashboard builder are explicitly out of scope and marked as Won't Fix in favor of Superset/Power BI.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Analytics Overview | KPI cards + embedded dashboard in Analytics page | ⚠️ Partial |
| SF-002 | Provider Dashboards | Embedded Superset/Power BI dashboards | ⚠️ Partial |
| SF-003 | Report Listing | Browse and manage provider-defined reports | ⚠️ Partial |
| SF-004 | Self-built Report Designer | Visual builder for report configuration | ❌ Not Implemented (Won't Fix) |
| SF-005 | Self-built Dashboard Builder | Custom dashboard widget builder | ❌ Not Implemented (Won't Fix) |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | View analytics overview | Sales Manager | User has access to analytics | KPIs and embed render | ⚠️ Partial |
| UC-002 | Switch dashboards | Analyst | Provider dashboards configured | Selected dashboard loads | ⚠️ Partial |
| UC-003 | View provider reports | Analyst | Provider reports available | Report list displayed | ⚠️ Partial |
| UC-004 | Run provider report | Analyst | Report exists | Execution result generated | ⚠️ Partial |
| UC-005 | Embed external BI | Admin | Analytics provider configured | Dashboard embedded | ⚠️ Partial |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| AnalyticsPage | CRM.Frontend/src/pages/AnalyticsPage.tsx | ⚠️ Partial | KPIs and tabs; embed uses provider APIs. Lead/AI sections are static placeholders. |
| DashboardPage | CRM.Frontend/src/pages/DashboardPage.tsx | ⚠️ Partial | Provider embed dialog present; self-built widgets are out of scope. |
| ReportsPage | CRM.Frontend/src/pages/ReportsPage.tsx | ⚠️ Partial | Report listing allowed; self-built designer is out of scope. |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| AnalyticsEmbed | CRM.Frontend/src/components/common/AnalyticsEmbed.tsx | ⚠️ Partial | Embeds external dashboards via provider endpoints; requires provider configuration. |
| ReportDesigner | CRM.Frontend/src/components/analytics/ReportDesigner.tsx | ❌ Not Implemented (Won't Fix) | Self-built reporting is deprecated in favor of Superset/Power BI. |
| DashboardBuilder | CRM.Frontend/src/components/analytics/DashboardBuilder.tsx | ❌ Not Implemented (Won't Fix) | Self-built dashboard builder is deprecated in favor of Superset/Power BI. |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| dashboardDataService | CRM.Frontend/src/services/dashboardService.ts | getStats, getSummary, getPipeline, getRecentActivities | ✅ |
| dashboardConfigService | CRM.Frontend/src/services/dashboardService.ts | getDashboards, getDashboard, getDefaultDashboard, widgets (read-only) | ⚠️ Partial |
| reportService | CRM.Frontend/src/services/reportService.ts | getReports, executeReport, previewReport, exportReport | ⚠️ Partial |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Report Name | Required; defaults to "Untitled Report" | Frontend | ✅ |
| Report Data Source | Provider-defined only | Frontend | ❌ Not Implemented (Won't Fix) |
| Dashboard Name | Provider-defined only | Frontend | ❌ Not Implemented (Won't Fix) |
| Widget Configuration | Self-built widgets out of scope | Frontend | ❌ Not Implemented (Won't Fix) |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| ReportDefinition | CRM.Backend/src/CRM.Core/Entities/Reports | ⚠️ Partial | Refer to SPEC-AI-005-ReportingAnalytics.md for backend coverage. |
| Dashboard | CRM.Backend/src/CRM.Core/Entities/Dashboards | ⚠️ Partial | Refer to SPEC-AI-005-ReportingAnalytics.md for backend coverage. |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| ReportDefinitionDto | CRM.Backend/src/CRM.Core/DTOs/Reports | ⚠️ Partial | Refer to SPEC-AI-005-ReportingAnalytics.md. |
| DashboardConfigDto | CRM.Backend/src/CRM.Core/DTOs/Dashboards | ⚠️ Partial | Refer to SPEC-AI-005-ReportingAnalytics.md. |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IAnalyticsPort | CRM.Backend/src/CRM.Core/Ports/Output/Providers/IAIPort.cs | 20+ | ✅ |
| IReportService | CRM.Backend/src/CRM.Core/Interfaces/IReportService.cs | 10+ | ⚠️ Partial |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| BuiltInAnalyticsProvider | CRM.Backend/src/CRM.Infrastructure/Providers/BuiltIn/BuiltInAnalyticsProvider.cs | 20+ | ✅ |
| SupersetProvider | CRM.Backend/src/CRM.Infrastructure/Providers/Superset/SupersetProvider.cs | 20+ | ✅ |
| PowerBIProvider | CRM.Backend/src/CRM.Infrastructure/Providers/PowerBI/PowerBIProvider.cs | 20+ | ✅ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| DashboardController | CRM.Backend/src/CRM.Api/Controllers/DashboardController.cs | 15 | ✅ |
| DashboardConfigController | CRM.Backend/src/CRM.Api/Controllers/DashboardConfigController.cs | 15 | ✅ |
| ReportsController | CRM.Backend/src/CRM.Api/Controllers/ReportsController.cs | 30 | ✅ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | /api/dashboard-config/dashboards | GetDashboards | Yes | ✅ |
| GET | /api/dashboard-config/dashboards/{id}/widgets | GetWidgets | Yes | ✅ |
| POST | /api/reports | CreateReport | Yes | ✅ |
| POST | /api/reports/{id}/execute | ExecuteReport | Yes | ✅ |
| GET | /api/analytics/dashboards | GetDashboards | Yes | ⚠️ Partial |
| GET | /api/analytics/dashboards/{id}/embed | GetEmbed | Yes | ⚠️ Partial |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Dashboard Name | Required | Service | ⚠️ Partial |
| Report Query | JSON configuration | Service | ⚠️ Partial |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Dashboards | database/schema | ✅ | Refer to DATABASE_SCHEMA.md for definitions. |
| DashboardWidgets | database/schema | ✅ | Refer to DATABASE_SCHEMA.md for definitions. |
| ReportDefinitions | database/schema | ✅ | Refer to DATABASE_SCHEMA.md for definitions. |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Name | VARCHAR | No | - | - | Dashboard.Name | ✅ |
| ConfigJson | TEXT | Yes | - | - | DashboardWidget.ConfigJson | ✅ |
| Query | TEXT | No | - | - | ReportDefinition.Query | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| DashboardWidgets | Dashboards | 1:N | DashboardId | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_Dashboards_Name | Dashboards | Name | NonClustered | ✅ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| DashboardConfigServiceTests | CRM.Backend/tests | - | ❌ |
| ReportsServiceTests | CRM.Backend/tests | - | ❌ |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| DashboardConfigControllerTests | CRM.Backend/tests | - | ❌ |
| ReportsControllerTests | CRM.Backend/tests | - | ❌ |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| analytics.spec.ts | e2e-tests/tests/analytics | - | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| ReportConfig query | ReportDefinition.Query | JSON string stored; lacks schema version | TODO-AI005-FE-002 |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| DashboardBuilder persistence | DashboardPage integration | Won't Fix: self-built dashboards deprecated | TODO-AI005-FE-003 |
| Report export scheduling | ReportsPage/ReportDesigner | Won't Fix: self-built reporting deprecated | TODO-AI005-FE-004 |
| Analytics embed endpoints | CRM.Api analytics controller | Align /analytics routes for provider embeds | TODO-AI005-FE-005 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Report Filters | Self-built filters deprecated | TODO-AI005-FE-006 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-AI005-FE-001 | Add end-to-end analytics tests for Superset/Power BI dashboards | P2 | Testing |
| TODO-AI005-FE-002 | Define JSON schema versioning for provider report query payloads | P2 | Data Contract |
| TODO-AI005-FE-003 | Won't Fix: self-built DashboardBuilder persistence | P3 | Won't Fix |
| TODO-AI005-FE-004 | Won't Fix: self-built report scheduling/export wiring | P3 | Won't Fix |
| TODO-AI005-FE-005 | Align analytics embed API routes with backend controllers | P2 | Integration |
| TODO-AI005-FE-006 | Won't Fix: self-built ReportDesigner validation | P3 | Won't Fix |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-18 | GitHub Copilot | Initial specification for frontend analytics UI |
