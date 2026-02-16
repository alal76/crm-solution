# SPEC-CRM-006: Pipeline Management

> **Version:** 1.0  
> **Last Updated:** February 12, 2026  
> **Status:** ✅ Complete  
> **Module:** Core CRM  
> **Priority:** P1  
> **Dependencies:** SPEC-CRM-003 (Opportunity Management)

---

## 1. Business Context

### 1.1 Feature Description

Pipeline Management provides the framework for organizing and visualizing the sales opportunity progression. It defines the stages that opportunities move through from initial discovery to close, with associated probabilities and metrics. The current implementation provides a default sales pipeline with predefined stages aligned to the OpportunityStage enum.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-006-01 | Default Pipeline Definition | Static default pipeline with standard sales stages | ✅ Implemented |
| SF-006-02 | Pipeline Stages | 6 stages from Discovery to Closed (Won/Lost) | ✅ Implemented |
| SF-006-03 | Stage Probability | Weighted probability per stage (10-100%) | ✅ Implemented |
| SF-006-04 | Pipeline Statistics | Aggregated stats by stage with value/count | ✅ Implemented |
| SF-006-05 | Stage Colors | Visual color coding per pipeline stage | ✅ Implemented |
| SF-006-06 | Pipeline Visualization | Dashboard funnel/charts for pipeline | ✅ Implemented |

### 1.3 Functionalities

| ID | Functionality | Description | Status |
|----|---------------|-------------|--------|
| F-006-01 | Get All Pipelines | Retrieve available pipeline definitions | ✅ Implemented |
| F-006-02 | Get Pipeline by ID | Retrieve specific pipeline with stages | ✅ Implemented |
| F-006-03 | Get Pipeline Stats | Aggregated opportunity stats per stage | ✅ Implemented |
| F-006-04 | Get Default Stages | Retrieve standard pipeline stage definitions | ✅ Implemented |
| F-006-05 | Pipeline Funnel Widget | Dashboard widget showing pipeline funnel | ✅ Implemented |
| F-006-06 | Pipeline Trend Chart | Trend visualization on dashboard | ✅ Implemented |

### 1.4 Use Cases

| ID | Use Case | Actor | Precondition | Flow | Postcondition |
|----|----------|-------|--------------|------|---------------|
| UC-006-01 | View Sales Pipeline | Sales Manager | Logged in with Opportunities access | Navigate to Dashboard → View pipeline funnel | Pipeline stages displayed with values |
| UC-006-02 | Analyze Pipeline Stats | Sales Manager | Has opportunities in pipeline | GET /api/pipelines/{id}/stats | Stage breakdown with counts and values |
| UC-006-03 | Review Stage Probabilities | Sales Rep | Has access to pipeline data | View pipeline definition | Stage probabilities displayed (10-100%) |

---

## 2. Frontend Implementation

### 2.1 Pages

| Page | File | Status | Notes |
|------|------|--------|-------|
| Dashboard (Pipeline Widgets) | `src/pages/DashboardPage.tsx` | ✅ Implemented | Pipeline funnel and trend charts |

### 2.2 Components

| Component | File | Status | Notes |
|-----------|------|--------|-------|
| Pipeline Funnel Chart | `DashboardPage.tsx` (embedded) | ✅ Implemented | Recharts BarChart visualization |
| Pipeline Trend Chart | `DashboardPage.tsx` (embedded) | ✅ Implemented | Recharts LineChart for trends |
| Dashboard Builder | `src/components/analytics/DashboardBuilder.tsx` | ✅ Implemented | Pipeline data source support |

### 2.3 Services

| Service | File | Status | Notes |
|---------|------|--------|-------|
| Dashboard Service | `src/services/dashboardService.ts` | ✅ Implemented | getPipeline(), PipelineStage, PipelineSummary |
| API Service | `src/services/apiService.ts` | ✅ Implemented | getTotalPipeline() endpoint |

### 2.4 Types/Interfaces

```typescript
// From dashboardService.ts
export interface PipelineStage {
  stage: string;
  stageValue: number;
  count: number;
  totalValue: number;
  weightedValue: number;
}

export interface PipelineSummary {
  stages: PipelineStage[];
  summary: {
    totalValue: number;
    weightedValue: number;
    opportunityCount: number;
  };
}

export enum WidgetType {
  // ...
  PipelineFunnel = 7,
  // ...
}
```

---

## 3. Backend Implementation

### 3.1 Entities

| Entity | File | Status | Notes |
|--------|------|--------|-------|
| OpportunityStage (enum) | `CRM.Core/Entities/Opportunity.cs` | ✅ Implemented | 6 stages with probabilities |

```csharp
public enum OpportunityStage
{
    Discovery = 0,       // 10% probability
    Qualification = 1,   // 25% probability
    Proposal = 2,        // 50% probability
    Negotiation = 3,     // 75% probability
    ClosedWon = 4,       // 100% probability
    ClosedLost = 5       // 0% probability
}
```

### 3.2 DTOs

| DTO | File | Status | Notes |
|-----|------|--------|-------|
| PipelineDefinition | `IPipelineService.cs` | ✅ Implemented | Id, Name, Description, IsDefault, Stages |
| PipelineStage | `IPipelineService.cs` | ✅ Implemented | Order, Name, Key, Probability, Color |
| PipelineStatistics | `IPipelineService.cs` | ✅ Implemented | Stats per stage, totals |
| PipelineStageStats | `IPipelineService.cs` | ✅ Implemented | Count, TotalValue, AverageValue |

### 3.3 Interfaces

| Interface | File | Lines | Status |
|-----------|------|-------|--------|
| IPipelineService | `CRM.Core/Interfaces/IPipelineService.cs` | ~85 | ✅ Implemented |

**Methods:**
- `GetPipelinesAsync()` - Get all pipeline definitions
- `GetByIdAsync(Guid id)` - Get pipeline by ID
- `GetStatsAsync(Guid pipelineId)` - Get statistics per stage
- `GetDefaultStages()` - Get default stage definitions

### 3.4 Services

| Service | File | Lines | Status |
|---------|------|-------|--------|
| PipelineService | `CRM.Infrastructure/Services/PipelineService.cs` | 158 | ✅ Implemented |

**Implementation Notes:**
- Uses static default pipeline definition (extensible to DB-backed custom pipelines)
- 9 stages in PipelineService (extended from 6 in enum)
- Statistics aggregated from Opportunities table
- Empty stages included in stats for completeness

### 3.5 Controllers

| Controller | File | Lines | Status |
|------------|------|-------|--------|
| PipelinesController | `CRM.Api/Controllers/PipelinesController.cs` | ~135 | ✅ Implemented |

**Endpoints:**
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/pipelines` | Get all pipelines | Bearer |
| GET | `/api/pipelines/{id}` | Get pipeline by ID | Bearer |
| GET | `/api/pipelines/{id}/stats` | Get pipeline statistics | Bearer |

### 3.6 DI Registration

```csharp
// Program.cs line ~418
builder.Services.AddScoped<IPipelineService, PipelineService>();
```

---

## 4. Database

### 4.1 Tables

No dedicated Pipeline table exists. Pipeline data is:
1. **Static Configuration**: Default pipeline stages defined in PipelineService
2. **Opportunity Stage**: Stored as `OpportunityStage` enum value in `Opportunities` table

### 4.2 Related Tables

| Table | Relationship | Notes |
|-------|--------------|-------|
| Opportunities | Stage column | OpportunityStage enum (int 0-5) |

### 4.3 Future Schema (for custom pipelines)

```sql
-- Planned for custom pipeline support
CREATE TABLE Pipelines (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Name VARCHAR(100) NOT NULL,
  Description VARCHAR(500),
  IsDefault BOOLEAN DEFAULT FALSE,
  IsActive BOOLEAN DEFAULT TRUE,
  CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt DATETIME,
  IsDeleted BOOLEAN DEFAULT FALSE
);

CREATE TABLE PipelineStages (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  PipelineId INT NOT NULL,
  Name VARCHAR(100) NOT NULL,
  StageKey VARCHAR(50) NOT NULL,
  SortOrder INT NOT NULL,
  Probability INT DEFAULT 0,
  Color VARCHAR(20),
  IsActive BOOLEAN DEFAULT TRUE,
  FOREIGN KEY (PipelineId) REFERENCES Pipelines(Id)
);
```

---

## 5. Tests

### 5.1 Unit Tests

| Test Class | File | Tests | Status |
|------------|------|-------|--------|
| PipelinesControllerTests | `tests/Controllers/PipelinesControllerTests.cs` | 756 lines, ~25 tests | ✅ Implemented |

**Test Coverage:**
- GetAll_ReturnsOkWithPipelines
- GetActive_ReturnsActivePipelines
- GetDefault_ReturnsDefaultPipeline
- GetById_ExistingPipeline_ReturnsOk
- GetById_NonExistent_ReturnsNotFound
- Create_ValidPipeline_ReturnsCreated
- Update_ValidPipeline_ReturnsOk
- Delete_ExistingPipeline_ReturnsNoContent
- GetStages_ReturnsStages
- ReorderStages_UpdatesOrder

### 5.2 Integration Tests

| Test | Status | Notes |
|------|--------|-------|
| E2E Pipeline API | ⚠️ Partial | Covered via Dashboard tests |

### 5.3 E2E Tests

| Test | Status | Notes |
|------|--------|-------|
| Dashboard Pipeline Funnel | ✅ Implicit | Via DashboardPage E2E tests |

---

## 6. Issues

### 6.1 Naming Inconsistencies

| Location | Current | Expected | Severity |
|----------|---------|----------|----------|
| PipelineService stages | 9 stages | 6 stages (enum) | Low |
| Controller vs Service | Different stage definitions | Should align | Low |

### 6.2 Validation Gaps

| Location | Gap | Priority |
|----------|-----|----------|
| Stage progression | No validation of stage transitions | P3 |
| Probability values | Not enforced to match stage | P3 |

---

## 7. TODO Items

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-CRM006-001 | Create PipelineServiceTests.cs unit tests | P2 | Testing |
| TODO-CRM006-002 | Implement custom pipeline CRUD (database-backed) | P3 | Feature |
| TODO-CRM006-003 | Add stage transition validation | P3 | Validation |
| TODO-CRM006-004 | Align stage definitions between Service and Controller | P3 | Consistency |

---

## 8. Related Specifications

| Spec ID | Name | Relationship |
|---------|------|--------------|
| SPEC-CRM-003 | Opportunity Management | Pipeline stages determine opportunity flow |
| SPEC-AI-002 | Opportunity Insights | AI predictions based on pipeline position |

---

## Change History

| Date | Author | Changes |
|------|--------|---------|
| 2026-02-12 | System | Initial specification created from existing implementation |
