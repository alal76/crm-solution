# Feature Specification: Lead Management

> **Spec ID:** SPEC-CRM-002  
> **Feature:** Lead Management  
> **Module:** Core CRM  
> **Version:** 1.0  
> **Last Updated:** February 8, 2026  
> **Status:** ✅ Implemented

---

## 1. Business Context

### 1.1 Feature Description
Capture, qualify, score, and convert potential customers (leads) through the sales funnel. Includes automated lead scoring, routing, duplicate detection, and conversion to opportunities.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Lead CRUD | Create, read, update, delete leads | ✅ Implemented |
| SF-002 | Lead Scoring | Automated fit + engagement scoring | ✅ Implemented |
| SF-003 | Lead Routing | Rule-based lead assignment | ✅ Implemented |
| SF-004 | Lead Qualification | MQL/SQL workflow | ✅ Implemented |
| SF-005 | Lead Conversion | Convert to opportunity/account | ✅ Implemented |
| SF-006 | Duplicate Detection | Find potential duplicate leads | ✅ Implemented |
| SF-007 | Lead Import/Export | Bulk operations | ✅ Implemented |
| SF-008 | Lead Score Decay | Automatic score reduction over time | ✅ Implemented |
| SF-009 | Product Interests | Track lead interest in products | ✅ Implemented |
| SF-010 | Web Tracking | Track lead website behavior | ⚠️ Partial |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Create Lead Manually | Sales Rep | Logged in | Lead created | ✅ |
| UC-002 | Capture Lead from Form | System | Form submitted | Lead created with source | ✅ |
| UC-003 | Score Lead | System | Lead exists | Score calculated | ✅ |
| UC-004 | Route Lead to Rep | System | Lead created | Owner assigned | ✅ |
| UC-005 | Qualify Lead to MQL | Marketing | Lead score >= threshold | MQL date set | ✅ |
| UC-006 | Accept Lead as SQL | Sales Rep | Lead is MQL | SQL date set | ✅ |
| UC-007 | Convert Lead | Sales Rep | Lead qualified | Opportunity created | ✅ |
| UC-008 | Disqualify Lead | Sales Rep | Lead not fit | Status = Disqualified | ✅ |
| UC-009 | Detect Duplicates | System | Lead created | Duplicates flagged | ✅ |
| UC-010 | Merge Duplicates | Sales Rep | Duplicates identified | Single record remains | ⚠️ Partial |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| LeadsPage | `CRM.Frontend/src/pages/LeadsPage.tsx` | ✅ | Main lead list page |
| LeadRoutingPage | `CRM.Frontend/src/pages/LeadRoutingPage.tsx` | ✅ | Routing rules management |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| LeadForm | `CRM.Frontend/src/components/leads/` | ❌ Not Found | Inline in page |
| LeadScoreCard | `CRM.Frontend/src/components/leads/` | ❌ Not Found | |
| LeadTimeline | `CRM.Frontend/src/components/leads/` | ❌ Not Found | |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| leadRoutingService | `CRM.Frontend/src/services/leadRoutingService.ts` | 15+ | ✅ |
| apiService (leads) | `CRM.Frontend/src/services/apiService.ts` | leads.* | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Email | Valid email format, required | Both | ✅ |
| FirstName | Required, max 100 chars | Both | ✅ |
| LastName | Required, max 100 chars | Both | ✅ |
| Phone | Phone format | Frontend | ❌ Not Implemented |
| CompanyName | Max 255 chars | Both | ✅ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Lead | `CRM.Core/Entities/Lead.cs` | ✅ | 276 lines |
| LeadRoutingRule | `CRM.Core/Entities/LeadRoutingRule.cs` | ✅ | Routing rules |
| LeadScoreRule | `CRM.Core/Entities/LeadScoreRule.cs` | ✅ | Score rules |

### 3.2 Enums
| Enum | Values | Entity Property | Status |
|------|--------|-----------------|--------|
| LeadLifecycleStatus | New=0, Working=1, Nurturing=2, Qualified=3, Disqualified=4, Converted=5 | Status | ✅ |
| LeadSource | Web=0, Campaign=1, Referral=2, Event=3, Partner=4, Manual=5 | Source | ✅ |

### 3.3 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| LeadDto | `CRM.Core/DTOs/LeadDto.cs` | ✅ | |
| CreateLeadDto | `CRM.Core/DTOs/CreateLeadDto.cs` | ✅ | |
| UpdateLeadDto | `CRM.Core/DTOs/UpdateLeadDto.cs` | ✅ | |
| ConvertLeadDto | `CRM.Core/DTOs/ConvertLeadDto.cs` | ✅ | |

### 3.4 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| ILeadRoutingService | `CRM.Core/Interfaces/ILeadRoutingService.cs` | 10+ | ✅ |

### 3.5 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| LeadRoutingService | `CRM.Infrastructure/Services/LeadRoutingService.cs` | 15+ | ✅ |
| LeadScoreDecayHostedService | `CRM.Infrastructure/Services/LeadScoreDecayHostedService.cs` | - | ✅ |

### 3.6 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| LeadsController | `CRM.Api/Controllers/LeadsController.cs` | 15+ | ✅ |
| LeadRoutingController | `CRM.Api/Controllers/LeadRoutingController.cs` | 20+ | ✅ |
| LeadScoreRulesController | `CRM.Api/Controllers/LeadScoreRulesController.cs` | 10+ | ✅ |

### 3.7 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/leads` | GetAll | Yes | ✅ |
| GET | `/api/leads/{id}` | GetById | Yes | ✅ |
| POST | `/api/leads` | Create | Yes | ✅ |
| PUT | `/api/leads/{id}` | Update | Yes | ✅ |
| DELETE | `/api/leads/{id}` | Delete | Yes | ✅ |
| POST | `/api/leads/{id}/convert` | Convert | Yes | ✅ |
| POST | `/api/leads/{id}/qualify` | Qualify | Yes | ✅ |
| POST | `/api/leads/{id}/disqualify` | Disqualify | Yes | ✅ |
| GET | `/api/lead-routing/rules` | GetRules | Yes | ✅ |
| POST | `/api/lead-routing/rules` | CreateRule | Yes | ✅ |
| GET | `/api/lead-score-rules` | GetScoreRules | Yes | ✅ |

### 3.8 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Email | [Required], [EmailAddress], [MaxLength(255)] | Entity | ✅ |
| FirstName | [Required], [MaxLength(100)] | Entity | ✅ |
| LastName | [Required], [MaxLength(100)] | Entity | ✅ |
| Score | [Range(0, 100)] | Entity | ✅ |
| FitScore | [Range(0, 100)] | Entity | ✅ |
| EngagementScore | [Range(0, 100)] | Entity | ✅ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Leads | `database/schema/001_core_tables.sql` | ✅ | |
| LeadRoutingRules | `database/schema/001_core_tables.sql` | ✅ | |
| LeadRoutingCriteria | `database/schema/001_core_tables.sql` | ✅ | |
| LeadRoutingTargets | `database/schema/001_core_tables.sql` | ✅ | |
| LeadRoutingLogs | `database/schema/001_core_tables.sql` | ✅ | |
| LeadProductInterests | `database/schema/001_core_tables.sql` | ✅ | Junction |

### 4.2 Data Elements - Leads Table
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| Status | INT | No | 0 | - | Status (enum) | ✅ |
| Source | INT | No | 0 | - | Source (enum) | ✅ |
| Score | INT | No | 0 | - | Score | ✅ |
| FitScore | INT | No | 0 | - | FitScore | ✅ |
| EngagementScore | INT | No | 0 | - | EngagementScore | ✅ |
| FirstName | VARCHAR(100) | No | - | - | FirstName | ✅ |
| LastName | VARCHAR(100) | No | - | - | LastName | ✅ |
| Email | VARCHAR(255) | No | - | UK | Email | ✅ |
| Phone | VARCHAR(30) | Yes | NULL | - | Phone | ✅ |
| Title | VARCHAR(100) | Yes | NULL | - | Title | ✅ |
| CompanyName | VARCHAR(255) | Yes | NULL | - | CompanyName | ✅ |
| Website | VARCHAR(500) | Yes | NULL | - | Website | ✅ |
| OwnerId | INT | Yes | NULL | FK→Users | OwnerId | ✅ |
| AccountId | INT | Yes | NULL | FK→Customers | AccountId | ✅ |
| CampaignId | INT | Yes | NULL | FK→Campaigns | CampaignId | ✅ |
| MqlDate | DATETIME | Yes | NULL | - | MqlDate | ✅ |
| SqlDate | DATETIME | Yes | NULL | - | SqlDate | ✅ |
| LastActivityDate | DATETIME | Yes | NULL | - | LastActivityDate | ✅ |
| LastScoreDecayDate | DATETIME | Yes | NULL | - | LastScoreDecayDate | ✅ |
| QualificationNotes | VARCHAR(4000) | Yes | NULL | - | QualificationNotes | ✅ |
| Region | VARCHAR(100) | Yes | NULL | - | Region | ✅ |
| Tags | VARCHAR(2000) | Yes | NULL | - | Tags (JSON) | ✅ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | ✅ |
| IsDeleted | TINYINT(1) | No | 0 | - | IsDeleted | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| Leads | Users | N:1 | OwnerId | ✅ |
| Leads | Customers | N:1 | AccountId | ✅ |
| Leads | MarketingCampaigns | N:1 | CampaignId | ✅ |
| LeadProductInterests | Leads | N:1 | LeadId | ✅ |
| LeadProductInterests | Products | N:1 | ProductId | ✅ |
| LeadRoutingCriteria | LeadRoutingRules | N:1 | RuleId | ✅ |
| LeadRoutingTargets | LeadRoutingRules | N:1 | RuleId | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| PK_Leads | Leads | Id | Clustered | ✅ |
| IX_Leads_Email | Leads | Email | NonClustered | ✅ |
| IX_Leads_Status | Leads | Status | NonClustered | ✅ |
| IX_Leads_OwnerId | Leads | OwnerId | NonClustered | ✅ |
| IX_Leads_Score | Leads | Score | NonClustered | ✅ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| LeadRoutingServiceTests | `CRM.Tests/Services/LeadRoutingServiceTests.cs` | 23 | ✅ |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| LeadRoutingControllerTests | `CRM.Tests/Integration/` | - | ❌ Not Found |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| leads.spec.ts | `e2e-tests/tests/leads/` | - | ❌ Not Found |

---

## 6. Inconsistencies & Issues

### 6.1 Naming Inconsistencies
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| Entity: CompanyName | Some UI: Company | Inconsistent naming | TODO-CRM002-001 |
| Entity: Score | Frontend may use LeadScore | Alias exists | ✅ OK |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Lead merge UI | Frontend | Complex feature | TODO-CRM002-002 |
| Web tracking integration | Frontend | Phase 2 | TODO-CRM002-003 |
| Lead import UI | Frontend | Partial | TODO-CRM002-004 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Phone | No format validation | TODO-CRM002-005 |
| Website | URL format not enforced | TODO-CRM002-006 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-CRM002-001 | Standardize CompanyName vs Company naming | P3 | Consistency |
| TODO-CRM002-002 | Implement lead merge UI | P2 | Frontend |
| TODO-CRM002-003 | Complete web tracking integration | P2 | Backend |
| TODO-CRM002-004 | Complete lead import/export UI | P2 | Frontend |
| TODO-CRM002-005 | Add phone format validation | P2 | Validation |
| TODO-CRM002-006 | Add website URL validation | P3 | Validation |
| TODO-CRM002-007 | Create E2E tests for leads | P2 | Testing |
| TODO-CRM002-008 | Create integration tests for LeadRoutingController | P2 | Testing |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 8, 2026 | System | Initial specification |

