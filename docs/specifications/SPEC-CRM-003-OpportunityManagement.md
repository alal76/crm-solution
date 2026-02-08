# Feature Specification: Opportunity Management

> **Spec ID:** SPEC-CRM-003  
> **Feature:** Opportunity Management  
> **Module:** Core CRM - Sales  
> **Version:** 1.0  
> **Last Updated:** February 8, 2026  
> **Status:** ✅ Implemented

---

## 1. Business Context

### 1.1 Feature Description
Manage sales opportunities through the pipeline from initial discovery to closed-won or closed-lost. Track probability, products, quotes, and revenue forecasting.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Opportunity CRUD | Create, read, update, delete opportunities | ✅ Implemented |
| SF-002 | Pipeline Stages | Manage deal progression | ✅ Implemented |
| SF-003 | Product Association | Link products to opportunities | ✅ Implemented |
| SF-004 | Win/Loss Analysis | Track close reasons | ✅ Implemented |
| SF-005 | Revenue Forecasting | Project revenue by close date | ✅ Implemented |
| SF-006 | Probability Calculation | Auto-calculate win probability | ⚠️ Partial |
| SF-007 | Quote Generation | Create quotes from opportunities | ✅ Implemented |
| SF-008 | Competitor Tracking | Track competition on deals | ⚠️ Partial |
| SF-009 | Team Selling | Multi-rep collaboration | ⚠️ Partial |
| SF-010 | AI Insights | Opportunity intelligence | ✅ Implemented |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Create Opportunity | Sales Rep | Account exists | Opp created | ✅ |
| UC-002 | Create from Lead | Sales Rep | Lead qualified | Opp+Account created | ✅ |
| UC-003 | Move Stage | Sales Rep | Opp exists | Stage updated | ✅ |
| UC-004 | Add Products | Sales Rep | Opp in pipeline | Products linked | ✅ |
| UC-005 | Update Amount | Sales Rep | Opp exists | Amount recalculated | ✅ |
| UC-006 | Close Won | Sales Rep | Opp negotiation | Status=ClosedWon | ✅ |
| UC-007 | Close Lost | Sales Rep | Opp in pipeline | Status=ClosedLost | ✅ |
| UC-008 | Generate Quote | Sales Rep | Opp has products | Quote created | ✅ |
| UC-009 | View Forecast | Sales Mgr | Opps exist | Revenue projected | ✅ |
| UC-010 | Get AI Insights | Sales Rep | Opp exists | Recommendations shown | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| OpportunitiesPage | `CRM.Frontend/src/pages/OpportunitiesPage.tsx` | ✅ | Pipeline view |
| PipelinePage | `CRM.Frontend/src/pages/PipelinePage.tsx` | ✅ | Kanban board |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| OpportunityCard | `CRM.Frontend/src/components/` | ❌ Not Found | Inline |
| OpportunityForm | `CRM.Frontend/src/components/` | ❌ Not Found | Inline |
| PipelineBoard | `CRM.Frontend/src/components/pipeline/` | ✅ | Kanban |
| DealInsights | `CRM.Frontend/src/components/` | ❌ Not Found | |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| apiService (opportunities) | `CRM.Frontend/src/services/apiService.ts` | opportunities.* | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Name | Required, max 255 chars | Both | ✅ |
| Amount | >= 0, number | Both | ✅ |
| Probability | 0-100 | Both | ✅ |
| ExpectedCloseDate | Valid date | Both | ✅ |
| AccountId | Required | Both | ✅ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Opportunity | `CRM.Core/Entities/Opportunity.cs` | ✅ | 280 lines |
| OpportunityProduct | `CRM.Core/Entities/OpportunityProduct.cs` | ✅ | Junction table |
| OpportunityInsight | `CRM.Core/Entities/OpportunityInsight.cs` | ✅ | AI insights |

### 3.2 Enums
| Enum | Values | Entity Property | Status |
|------|--------|-----------------|--------|
| OpportunityStage | Discovery=0, Qualification=1, Proposal=2, Negotiation=3, ClosedWon=4, ClosedLost=5 | Stage | ✅ |
| QualificationReason | Budget=0, Authority=1, Need=2, Timeline=3, Other=4 | QualificationReason | ✅ |
| OpportunityPricingModel | FixedPrice=0, TimeMaterial=1, Subscription=2, Hybrid=3 | PricingModel | ✅ |

### 3.3 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| OpportunityDto | `CRM.Core/DTOs/OpportunityDto.cs` | ✅ | |
| CreateOpportunityDto | `CRM.Core/DTOs/CreateOpportunityDto.cs` | ✅ | |
| UpdateOpportunityDto | `CRM.Core/DTOs/UpdateOpportunityDto.cs` | ✅ | |

### 3.4 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IOpportunityService | `CRM.Core/Interfaces/IOpportunityService.cs` | 15+ | ✅ |

### 3.5 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| OpportunityService | `CRM.Infrastructure/Services/OpportunityService.cs` | 20+ | ✅ |

### 3.6 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| OpportunitiesController | `CRM.Api/Controllers/OpportunitiesController.cs` | 15+ | ✅ |

### 3.7 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/opportunities` | GetAll | Yes | ✅ |
| GET | `/api/opportunities/{id}` | GetById | Yes | ✅ |
| POST | `/api/opportunities` | Create | Yes | ✅ |
| PUT | `/api/opportunities/{id}` | Update | Yes | ✅ |
| DELETE | `/api/opportunities/{id}` | Delete | Yes | ✅ |
| PATCH | `/api/opportunities/{id}/stage` | UpdateStage | Yes | ✅ |
| POST | `/api/opportunities/{id}/products` | AddProduct | Yes | ✅ |
| DELETE | `/api/opportunities/{id}/products/{productId}` | RemoveProduct | Yes | ✅ |
| POST | `/api/opportunities/{id}/close-won` | CloseWon | Yes | ✅ |
| POST | `/api/opportunities/{id}/close-lost` | CloseLost | Yes | ✅ |
| GET | `/api/opportunities/pipeline` | GetPipelineView | Yes | ✅ |
| GET | `/api/opportunities/forecast` | GetForecast | Yes | ✅ |

### 3.8 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Name | [Required], [MaxLength(255)] | Entity | ✅ |
| Amount | >= 0 | Service | ✅ |
| Probability | [Range(0, 100)] | Entity | ✅ |
| AccountId | [Required] | Entity | ✅ |
| Stage | Valid enum | Entity | ✅ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Opportunities | `database/schema/001_core_tables.sql` | ✅ | |
| OpportunityProducts | `database/schema/001_core_tables.sql` | ✅ | Junction |
| OpportunityInsights | `database/schema/001_core_tables.sql` | ✅ | AI data |

### 4.2 Data Elements - Opportunities Table
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| Name | VARCHAR(255) | No | - | - | Name | ✅ |
| Stage | INT | No | 0 | - | Stage (enum) | ✅ |
| Amount | DECIMAL(18,2) | No | 0 | - | Amount | ✅ |
| Probability | INT | No | 0 | - | Probability | ✅ |
| ExpectedCloseDate | DATE | Yes | NULL | - | ExpectedCloseDate | ✅ |
| ActualCloseDate | DATE | Yes | NULL | - | ActualCloseDate | ✅ |
| AccountId | INT | No | - | FK→Customers | AccountId | ✅ |
| ContactId | INT | Yes | NULL | FK→Contacts | ContactId | ✅ |
| SalesOwnerId | INT | Yes | NULL | FK→Users | SalesOwnerId | ✅ |
| LeadId | INT | Yes | NULL | FK→Leads | LeadId | ✅ |
| LeadSource | VARCHAR(50) | Yes | NULL | - | LeadSource | ✅ |
| PricingModel | INT | No | 0 | - | PricingModel (enum) | ✅ |
| Description | TEXT | Yes | NULL | - | Description | ✅ |
| LossReason | VARCHAR(500) | Yes | NULL | - | LossReason | ✅ |
| NextSteps | VARCHAR(1000) | Yes | NULL | - | NextSteps | ✅ |
| Competitors | VARCHAR(2000) | Yes | NULL | JSON | Competitors | ✅ |
| DecisionMakers | VARCHAR(2000) | Yes | NULL | JSON | DecisionMakers | ✅ |
| QualificationScore | INT | Yes | NULL | - | QualificationScore | ✅ |
| QualificationReason | INT | Yes | NULL | - | QualificationReason (enum) | ✅ |
| ForecastCategory | VARCHAR(50) | Yes | NULL | - | ForecastCategory | ✅ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | ✅ |
| IsDeleted | TINYINT(1) | No | 0 | - | IsDeleted | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| Opportunities | Customers | N:1 | AccountId | ✅ |
| Opportunities | Contacts | N:1 | ContactId | ✅ |
| Opportunities | Users | N:1 | SalesOwnerId | ✅ |
| Opportunities | Leads | N:1 | LeadId | ✅ |
| OpportunityProducts | Opportunities | N:1 | OpportunityId | ✅ |
| OpportunityProducts | Products | N:1 | ProductId | ✅ |
| OpportunityInsights | Opportunities | N:1 | OpportunityId | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| PK_Opportunities | Opportunities | Id | Clustered | ✅ |
| IX_Opportunities_AccountId | Opportunities | AccountId | NonClustered | ✅ |
| IX_Opportunities_Stage | Opportunities | Stage | NonClustered | ✅ |
| IX_Opportunities_CloseDate | Opportunities | ExpectedCloseDate | NonClustered | ✅ |
| IX_Opportunities_SalesOwnerId | Opportunities | SalesOwnerId | NonClustered | ✅ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| OpportunityServiceTests | `CRM.Tests/Services/` | - | ❌ Not Found |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| - | - | - | ❌ Not Found |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| opportunities.spec.ts | `e2e-tests/tests/` | - | ❌ Not Found |

---

## 6. Inconsistencies & Issues

### 6.1 Naming Inconsistencies
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| Controller: /api/opportunities | Some UI uses "deals" | Terminology | TODO-CRM003-001 |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Competitor tracking UI | Frontend | Partial backend | TODO-CRM003-002 |
| Team selling UI | Frontend | Partial | TODO-CRM003-003 |
| Auto-probability calculation | Backend | Rule-based | TODO-CRM003-004 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Amount | No max limit | TODO-CRM003-005 |
| Stage transitions | Not enforced | TODO-CRM003-006 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-CRM003-001 | Standardize Opportunity vs Deal terminology | P3 | Consistency |
| TODO-CRM003-002 | Complete competitor tracking UI | P2 | Frontend |
| TODO-CRM003-003 | Complete team selling UI | P2 | Frontend |
| TODO-CRM003-004 | Implement rule-based probability calculation | P2 | Backend |
| TODO-CRM003-005 | Add Amount max limit validation | P3 | Validation |
| TODO-CRM003-006 | Enforce valid stage transitions | P2 | Validation |
| TODO-CRM003-007 | Create OpportunityServiceTests | P2 | Testing |
| TODO-CRM003-008 | Create E2E tests for opportunities | P2 | Testing |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 8, 2026 | System | Initial specification |

