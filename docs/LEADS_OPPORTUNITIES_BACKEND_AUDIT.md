# Leads & Opportunities Backend Code Audit

> **Generated:** February 2026  
> **Scope:** `/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend`  
> **Modules:** Lead Management, Opportunity Management, Lead Routing, Duplicate Detection/Merge

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Entity Files](#2-entity-files)
3. [DTOs (Data Transfer Objects)](#3-dtos-data-transfer-objects)
4. [Service Interfaces](#4-service-interfaces)
5. [Service Implementations](#5-service-implementations)
6. [API Controllers](#6-api-controllers)
7. [DbContext Registrations](#7-dbcontext-registrations)
8. [Dependency Injection Registrations](#8-dependency-injection-registrations)
9. [GAP ANALYSIS](#9-gap-analysis)
10. [Recommendations](#10-recommendations)

---

## 1. Executive Summary

### Components Found

| Component Type | Leads | Opportunities | Lead Routing | Duplicates/Merge |
|----------------|-------|---------------|--------------|------------------|
| **Entity** | ✅ Lead.cs | ✅ Opportunity.cs | ✅ LeadRoutingRule.cs | ✅ DuplicateRule.cs |
| **Interface** | ❌ ILeadService | ✅ IOpportunityService | ✅ ILeadRoutingService | ✅ IDuplicateDetectionService, IMergeService |
| **Service** | ❌ LeadService | ✅ OpportunityService | ✅ LeadRoutingService | ✅ DuplicateDetectionService, MergeService |
| **Controller** | ✅ LeadsController | ✅ OpportunitiesController | ✅ LeadRoutingController | ✅ DuplicatesController |
| **DTOs** | ✅ Inline in Controller | ❌ No DTOs | ✅ Inline | ✅ Inline in Interface |
| **DbSets** | ✅ 4 DbSets | ✅ 2 DbSets | ✅ 4 DbSets | ✅ 4 DbSets |
| **DI Registration** | N/A | ✅ Program.cs | ✅ Program.cs | ✅ Program.cs |

### Key Findings

1. **❌ CRITICAL GAP:** No `ILeadService` interface or `LeadService` implementation exists
2. **⚠️ WARNING:** LeadsController uses direct DbContext access instead of service layer
3. **⚠️ WARNING:** OpportunitiesController takes raw `Opportunity` entity instead of DTOs
4. **✅ GOOD:** Lead Routing is fully implemented with interface, service, and controller
5. **✅ GOOD:** Duplicate Detection/Merge is fully implemented with comprehensive interfaces

---

## 2. Entity Files

### 2.1 Lead Entity

**File:** `src/CRM.Core/Entities/Lead.cs` (276 lines)

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Primary key (inherited from BaseEntity) |
| `FirstName` | string | Lead's first name |
| `LastName` | string | Lead's last name |
| `Email` | string | Primary email address |
| `Phone` | string? | Phone number |
| `Title` | string? | Job title |
| `CompanyName` | string | Company/organization name |
| `Website` | string? | Company website |
| `Status` | LeadLifecycleStatus | Lead lifecycle status (enum) |
| `Source` | LeadSource | Lead source (enum) |
| `Score` | int | Overall lead score (0-100) |
| `FitScore` | int | Fit score component |
| `EngagementScore` | int | Engagement score component |
| `LastScoreDecayDate` | DateTime? | Last score decay calculation |
| `LastActivityDate` | DateTime? | Last activity timestamp |
| `QualificationNotes` | string? | Qualification notes |
| `MqlDate` | DateTime? | Marketing Qualified Lead date |
| `SqlDate` | DateTime? | Sales Qualified Lead date |
| `Region` | string? | Geographic region |
| `Tags` | string? | Tags (JSON or comma-separated) |
| `OwnerId` | int? | Assigned user ID |
| `CampaignId` | int? | Source campaign ID |
| `AccountId` | int? | Linked account ID |
| `ContactId` | int? | Linked contact ID |
| `MergedIntoId` | int? | ID of master record if merged |
| `MergeGroupId` | int? | Merge group identifier |
| `IsMergedDuplicate` | bool | Whether this is a merged duplicate |
| `MergedAt` | DateTime? | When the record was merged |

**Enums Defined:**

```csharp
public enum LeadLifecycleStatus
{
    New = 0,
    Working = 1,
    Nurturing = 2,
    Qualified = 3,
    Disqualified = 4,
    Converted = 5
}

public enum LeadSource
{
    Web = 0,
    Campaign = 1,
    Referral = 2,
    Event = 3,
    Partner = 4,
    Manual = 5
}
```

**Navigation Properties:**
- `Owner` → User
- `Campaign` → MarketingCampaign
- `Account` → Account
- `Contact` → Contact
- `ProductInterests` → ICollection<LeadProductInterest>
- `Opportunities` → ICollection<Opportunity>

**Junction Class (Inline):** `LeadProductInterest`
- `Id`, `LeadId`, `ProductId`, `InterestLevel`, `Notes`, `CreatedAt`, `UpdatedAt`, `IsDeleted`

---

### 2.2 Opportunity Entity

**File:** `src/CRM.Core/Entities/Opportunity.cs` (281 lines)

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Primary key |
| `Name` | string | Opportunity name |
| `Stage` | OpportunityStage | Current stage (enum) |
| `Probability` | int | Win probability (0-100) |
| `Amount` | decimal | Deal value |
| `Currency` | string? | Currency code |
| `ExpectedCloseDate` | DateTime? | Expected close date |
| `PricingModel` | OpportunityPricingModel? | Pricing model (enum) |
| `TermLengthMonths` | int? | Contract term in months |
| `SolutionNotes` | string? | Solution notes |
| `QualificationReason` | QualificationReason? | Win/loss reason (enum) |
| `QualificationNotes` | string? | Qualification details |
| `Region` | string? | Geographic region |
| `AccountId` | int | Associated account ID |
| `PrimaryContactId` | int? | Primary contact ID |
| `SalesOwnerId` | int? | Sales owner user ID |
| `LeadId` | int? | Source lead ID (if converted) |

**Enums Defined:**

```csharp
public enum OpportunityStage
{
    Discovery = 0,
    Qualification = 1,
    Proposal = 2,
    Negotiation = 3,
    ClosedWon = 4,
    ClosedLost = 5
}

public enum QualificationReason
{
    Budget = 0,
    Need = 1,
    Timing = 2,
    Authority = 3,
    Fit = 4
}

public enum OpportunityPricingModel
{
    Subscription = 0,
    OneTime = 1,
    UsageBased = 2,
    Hybrid = 3
}
```

**Junction Class (Inline):** `OpportunityProduct`
- `Id`, `OpportunityId`, `ProductId`, `Quantity`, `UnitPrice`, `DiscountPercent`, `LineTotal`, `Notes`, `IsDeleted`

---

### 2.3 LeadRoutingRule Entity

**File:** `src/CRM.Core/Entities/LeadRoutingRule.cs` (326 lines)

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Primary key |
| `Name` | string | Rule name |
| `Description` | string? | Rule description |
| `Status` | RoutingRuleStatus | Active/Inactive/Draft |
| `Priority` | int | Rule priority (lower = higher) |
| `AssignmentType` | LeadAssignmentType | How to assign (enum) |
| `AssignToTeam` | bool | Assign to team vs individual |
| `TeamId` | int? | Target team ID |
| `FallbackOwnerId` | int? | Fallback user if no targets |
| `EffectiveStartDate` | DateTime? | Rule effective from |
| `EffectiveEndDate` | DateTime? | Rule effective until |
| `BusinessHoursOnly` | bool | Only during business hours |
| `Timezone` | string? | Timezone for business hours |
| `RoundRobinPosition` | int | Current round-robin position |
| `LastAssignmentDate` | DateTime? | Last assignment timestamp |
| `TotalLeadsAssigned` | int | Total leads assigned by rule |
| `SendNotification` | bool | Send notification on assignment |
| `NotificationTemplateId` | int? | Notification template ID |
| `NotifyManager` | bool | Also notify manager |

**Related Classes (Inline):**

1. **LeadRoutingCriteria** (~12 properties)
   - `Id`, `LeadRoutingRuleId`, `Order`, `FieldName`, `CriteriaType`, `Operator`, `Value`, `ValueJson`, `LogicalOperator`, `IsActive`, `CreatedAt`, `UpdatedAt`, `IsDeleted`

2. **LeadRoutingTarget** (~11 properties)
   - `Id`, `LeadRoutingRuleId`, `UserId`, `Weight`, `MaxLeadsPerDay`, `MaxLeadsPerWeek`, `LeadsAssignedToday`, `LeadsAssignedThisWeek`, `LastAssignmentDate`, `IsActive`, `CreatedAt`, `UpdatedAt`, `IsDeleted`

3. **LeadRoutingLog** (~12 properties)
   - `Id`, `LeadId`, `RuleId`, `PreviousOwnerId`, `NewOwnerId`, `AssignmentType`, `Reason`, `ResponseTimeMinutes`, `WasAccepted`, `RejectionReason`, `RoutedAt`, `CreatedAt`

**Enums:**

```csharp
public enum LeadAssignmentType
{
    RoundRobin = 0, Manual = 1, ByScore = 2, ByCapacity = 3,
    ByTerritory = 4, BySkills = 5, ByPerformance = 6,
    WeightedRandom = 7, LeastRecent = 8
}

public enum RoutingCriteriaType
{
    LeadScore = 0, Region = 1, Industry = 2, CompanySize = 3,
    Source = 4, Campaign = 5, ProductInterest = 6, LeadAge = 7,
    Custom = 8, Status = 9
}

public enum RoutingRuleStatus { Active = 0, Inactive = 1, Draft = 2 }
```

---

### 2.4 LeadScoreRule Entity

**File:** `src/CRM.Core/Entities/LeadScoreRule.cs` (215 lines)

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Primary key |
| `Name` | string | Rule name |
| `Description` | string? | Rule description |
| `RuleType` | LeadScoreRuleType | Type of scoring rule |
| `FieldName` | string? | Field to evaluate |
| `Operator` | RuleOperator | Comparison operator |
| `Value` | string? | Value to compare against |
| `ConditionsJson` | string? | Complex conditions in JSON |
| `ScoreImpact` | int | Points to add/subtract |
| `MaxApplications` | int? | Max times rule can apply |
| `DecayDaysThreshold` | int | Days before decay starts |
| `DecayPointsPerPeriod` | int | Points to decay per period |
| `DecayPeriodDays` | int | Days per decay period |
| `IsActive` | bool | Whether rule is active |
| `Priority` | int | Evaluation priority |
| `Category` | string? | Rule category |

---

## 3. DTOs (Data Transfer Objects)

### 3.1 Lead DTOs (Inline in LeadsController.cs)

**CreateLeadDto:**
```csharp
public class CreateLeadDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Source { get; set; }
    public string? Region { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string? Description { get; set; }
    public int? OwnerId { get; set; }
    public int? CampaignId { get; set; }
    public int? Status { get; set; }
}
```

**UpdateLeadDto:**
```csharp
public class UpdateLeadDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
    public string? Region { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public int? Score { get; set; }
    public int? OwnerId { get; set; }
    public int? CampaignId { get; set; }
}
```

**ConvertLeadDto:**
```csharp
public class ConvertLeadDto
{
    public string? OpportunityName { get; set; }
    public int? AccountId { get; set; }
    public decimal? EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
}
```

### 3.2 Opportunity DTOs

**❌ GAP:** No dedicated Opportunity DTOs exist. Controller accepts raw `Opportunity` entity.

DTOs found only in test file (`tests/Validators/OpportunityValidatorTests.cs`):
- `CreateOpportunityDto` (test-only)
- `OpportunityDto` (test-only)

### 3.3 Lead Routing DTOs (Inline in LeadRoutingController.cs)

**RerouteLeadRequest:**
```csharp
public class RerouteLeadRequest
{
    public string? Reason { get; set; }
}
```

### 3.4 Duplicate Detection DTOs (Inline in DuplicatesController.cs)

**DuplicateCheckRequest:**
```csharp
public class DuplicateCheckRequest
{
    public string EntityType { get; set; } = string.Empty;
    public Dictionary<string, string?> FieldValues { get; set; } = new();
    public int? ExcludeRecordId { get; set; }
    public int MatchThreshold { get; set; } = 70;
}
```

**ScanResult:**
```csharp
public class ScanResult
{
    public int TotalRecordsScanned { get; set; }
    public int DuplicateCandidatesFound { get; set; }
    public DateTime CompletedAt { get; set; }
}
```

---

## 4. Service Interfaces

### 4.1 ILeadService

**File:** ❌ **DOES NOT EXIST** (only mock in test file `tests/Validators/LeadValidatorTests.cs`)

### 4.2 IOpportunityService

**File:** `src/CRM.Core/Interfaces/IOpportunityService.cs` (37 lines)

| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetOpportunityByIdAsync(int id)` | Task<Opportunity?> | Get opportunity by ID |
| `GetOpportunitiesByCustomerAsync(int customerId)` | Task<IEnumerable<Opportunity>> | Get by customer |
| `GetOpenOpportunitiesAsync()` | Task<IEnumerable<Opportunity>> | Get all open opportunities |
| `CreateOpportunityAsync(Opportunity opportunity)` | Task<int> | Create opportunity, returns ID |
| `UpdateOpportunityAsync(Opportunity opportunity)` | Task | Update opportunity |
| `DeleteOpportunityAsync(int id)` | Task | Delete opportunity |
| `GetTotalPipelineAsync()` | Task<decimal> | Get total pipeline value |

### 4.3 ILeadRoutingService

**File:** `src/CRM.Core/Interfaces/ILeadRoutingService.cs` (315 lines)

**Rule Management (7 methods):**
| Method | Return Type |
|--------|-------------|
| `GetAllRulesAsync(status?, teamId?, ct)` | Task<IEnumerable<LeadRoutingRule>> |
| `GetRuleByIdAsync(id, ct)` | Task<LeadRoutingRule?> |
| `CreateRuleAsync(rule, ct)` | Task<LeadRoutingRule> |
| `UpdateRuleAsync(rule, ct)` | Task<LeadRoutingRule> |
| `DeleteRuleAsync(id, ct)` | Task<bool> |
| `ActivateRuleAsync(id, ct)` | Task<LeadRoutingRule> |
| `DeactivateRuleAsync(id, ct)` | Task<LeadRoutingRule> |

**Criteria Management (4 methods):**
| Method | Return Type |
|--------|-------------|
| `AddCriteriaAsync(ruleId, criteria, ct)` | Task<LeadRoutingCriteria> |
| `UpdateCriteriaAsync(criteria, ct)` | Task<LeadRoutingCriteria> |
| `RemoveCriteriaAsync(criteriaId, ct)` | Task<bool> |
| `GetCriteriaAsync(ruleId, ct)` | Task<IEnumerable<LeadRoutingCriteria>> |

**Target Management (5 methods):**
| Method | Return Type |
|--------|-------------|
| `AddTargetAsync(ruleId, target, ct)` | Task<LeadRoutingTarget> |
| `UpdateTargetAsync(target, ct)` | Task<LeadRoutingTarget> |
| `RemoveTargetAsync(targetId, ct)` | Task<bool> |
| `GetTargetsAsync(ruleId, ct)` | Task<IEnumerable<LeadRoutingTarget>> |
| `GetTargetCapacityAsync(targetId, ct)` | Task<TargetCapacity> |

**Lead Routing Operations (5 methods):**
| Method | Return Type |
|--------|-------------|
| `RouteLeadAsync(leadId, ct)` | Task<LeadRoutingResult> |
| `RouteLeadWithRuleAsync(leadId, ruleId, ct)` | Task<LeadRoutingResult> |
| `EvaluateMatchingRulesAsync(leadId, ct)` | Task<IEnumerable<LeadRoutingRule>> |
| `RouteLeadsBatchAsync(leadIds, ct)` | Task<IEnumerable<LeadRoutingResult>> |
| `RerouteLeadAsync(leadId, reason?, ct)` | Task<LeadRoutingResult> |

**Routing Logs (3 methods):**
| Method | Return Type |
|--------|-------------|
| `GetLeadRoutingHistoryAsync(leadId, ct)` | Task<IEnumerable<LeadRoutingLog>> |
| `GetRuleRoutingLogsAsync(ruleId, from?, to?, ct)` | Task<IEnumerable<LeadRoutingLog>> |
| `GetUserRoutingLogsAsync(userId, from?, to?, ct)` | Task<IEnumerable<LeadRoutingLog>> |

**Analytics (3 methods):**
| Method | Return Type |
|--------|-------------|
| `GetRuleStatisticsAsync(ruleId, from?, to?, ct)` | Task<LeadRoutingStatistics> |
| `GetOverallStatisticsAsync(from?, to?, ct)` | Task<LeadRoutingStatistics> |
| `GetResponseTimeStatisticsAsync(ruleId?, userId?, from?, to?, ct)` | Task<ResponseTimeStatistics> |

**Capacity Management (2 methods):**
| Method | Return Type |
|--------|-------------|
| `ResetDailyCountsAsync(ct)` | Task |
| `ResetWeeklyCountsAsync(ct)` | Task |

**Supporting Types:**
- `LeadRoutingResult`: Success, LeadId, AssignedToUserId, RuleId, ErrorMessage, LogId
- `TargetCapacity`: TargetId, UserId, UserName, MaxLeadsPerDay/Week, LeadsAssignedToday/ThisWeek
- `LeadRoutingStatistics`: Total, ByStatus, ByAssignmentType, ByUser, AvgResponseTime
- `ResponseTimeStatistics`: AvgMinutes, MedianMinutes, Min/MaxMinutes, ByHour, ByDayOfWeek

### 4.4 IDuplicateDetectionService

**File:** `src/CRM.Core/Interfaces/IDuplicateDetectionService.cs` (280 lines)

| Method | Return Type | Description |
|--------|-------------|-------------|
| `CheckForDuplicatesAsync(entityType, fieldValues, excludeId?, ct)` | Task<DuplicateCheckResult> | Check for duplicates |
| `GetActiveRulesAsync(entityType)` | Task<IEnumerable<DuplicateRule>> | Get active rules |
| `GetAllRulesAsync()` | Task<IEnumerable<DuplicateRule>> | Get all rules |
| `SaveRuleAsync(rule)` | Task<DuplicateRule> | Create/update rule |
| `DeleteRuleAsync(ruleId)` | Task<bool> | Delete rule |
| `ScanForDuplicatesAsync(entityType, ruleId?, ct)` | Task<IEnumerable<DuplicateCandidate>> | Batch scan |
| `GetPendingCandidatesAsync(entityType?, page, pageSize)` | Task<IEnumerable<DuplicateCandidate>> | Get pending |
| `UpdateCandidateStatusAsync(id, status, userId, notes?)` | Task<DuplicateCandidate?> | Update status |

### 4.5 IMergeService

**File:** `src/CRM.Core/Interfaces/IMergeService.cs` (300 lines)

| Method | Return Type | Description |
|--------|-------------|-------------|
| `MergeRecordsAsync(request, ct)` | Task<MergeResult> | Merge records |
| `UnmergeRecordsAsync(request, ct)` | Task<UnmergeResult> | Unmerge records |
| `PreviewMergeAsync(request, ct)` | Task<MergePreview> | Preview merge |
| `GetMergeHistoryAsync(recordId, entityType)` | Task<IEnumerable<MergeGroupInfo>> | Get history |
| `GetMergedRecordsAsync(masterId, entityType)` | Task<IEnumerable<MergedRecordInfo>> | Get merged |
| `GetMergeGroupAsync(mergeGroupId)` | Task<MergeGroupInfo?> | Get group |

---

## 5. Service Implementations

### 5.1 LeadService

**File:** ❌ **DOES NOT EXIST**

### 5.2 OpportunityService

**File:** `src/CRM.Infrastructure/Services/OpportunityService.cs` (108 lines)

**Implements:** `IOpportunityService`, `IOpportunityInputPort`

**Dependencies:**
- `IRepository<Opportunity>` - Generic repository
- `IRepository<EntityTag>` - Tags repository
- `IRepository<CustomField>` - Custom fields repository
- `NormalizationService` - Data normalization
- `IEntityEventDispatcher` - Workflow event dispatch

**Method Implementations:**
| Method | Lines | Notes |
|--------|-------|-------|
| `GetOpportunityByIdAsync` | ~5 | Uses repository.GetByIdAsync |
| `GetOpportunitiesByCustomerAsync` | ~7 | Filters by AccountId |
| `GetOpenOpportunitiesAsync` | ~8 | Excludes ClosedWon/ClosedLost |
| `CreateOpportunityAsync` | ~15 | Dispatches OnCreate event |
| `UpdateOpportunityAsync` | ~10 | Dispatches OnUpdate event |
| `DeleteOpportunityAsync` | ~5 | Dispatches OnDelete event |
| `GetTotalPipelineAsync` | ~10 | Sums Amount for open opps |

### 5.3 LeadRoutingService

**File:** `src/CRM.Infrastructure/Services/LeadRoutingService.cs` (875 lines)

**Implements:** `ILeadRoutingService`

**Dependencies:**
- `ICrmDbContext` - Direct database context
- `ILogger<LeadRoutingService>` - Logging

**Key Implementation Details:**
- All 29 interface methods implemented
- Uses EF Core directly (not repository pattern)
- Supports all assignment types (RoundRobin, ByCapacity, ByScore, etc.)
- Criteria evaluation with multiple operators
- Capacity tracking per target (daily/weekly limits)

### 5.4 DuplicateDetectionService

**File:** `src/CRM.Infrastructure/Services/DuplicateDetectionService.cs` (839 lines)

**Implements:** `IDuplicateDetectionService`

**Dependencies:**
- `ICrmDbContext` - Direct database context
- `ILogger<DuplicateDetectionService>` - Logging

**Key Features:**
- Fuzzy matching algorithms (Levenshtein distance)
- Configurable match thresholds
- Weighted field scoring
- Batch scanning capability

### 5.5 MergeService

**File:** `src/CRM.Infrastructure/Services/MergeService.cs` (997 lines)

**Implements:** `IMergeService`

**Dependencies:**
- `ICrmDbContext` - Direct database context
- `ILogger<MergeService>` - Logging

**Key Features:**
- Transactional merge operations
- Related record relinking
- Merge history/audit trail
- Unmerge capability (restore merged records)
- Field-level merge control

---

## 6. API Controllers

### 6.1 LeadsController

**File:** `src/CRM.Api/Controllers/LeadsController.cs` (425 lines)

**Route:** `/api/leads`

| Endpoint | Method | Description |
|----------|--------|-------------|
| `GET /` | GetAll | Get all leads (paginated) |
| `GET /{id}` | GetById | Get lead by ID |
| `POST /` | Create | Create new lead |
| `PUT /{id}` | Update | Update lead |
| `DELETE /{id}` | Delete | Soft delete lead |
| `POST /{id}/convert` | Convert | Convert lead to opportunity |
| `GET /status/{status}` | GetByStatus | Get leads by status |
| `GET /stats` | GetStats | Get lead statistics |

**Dependencies:**
- `ICrmDbContext` - ⚠️ Direct DbContext access (no service layer)
- `ILogger<LeadsController>` - Logging
- `IEntityEventDispatcher` - Workflow events

### 6.2 OpportunitiesController

**File:** `src/CRM.Api/Controllers/OpportunitiesController.cs` (162 lines)

**Route:** `/api/opportunities`

| Endpoint | Method | Description |
|----------|--------|-------------|
| `GET /` | GetOpen | Get all open opportunities |
| `GET /{id}` | GetById | Get opportunity by ID |
| `GET /customer/{customerId}` | GetByCustomerId | Get by customer |
| `GET /pipeline/total` | GetTotalPipeline | Get total pipeline value |
| `POST /` | Create | Create opportunity |
| `PUT /{id}` | Update | Update opportunity |
| `DELETE /{id}` | Delete | Delete opportunity |

**Dependencies:**
- `IOpportunityService` - ✅ Uses service layer
- `ILogger<OpportunitiesController>` - Logging
- `ICrmNotificationService` - SignalR notifications

**⚠️ Issues:**
- Create/Update accept raw `Opportunity` entity instead of DTOs

### 6.3 LeadRoutingController

**File:** `src/CRM.Api/Controllers/LeadRoutingController.cs` (442 lines)

**Route:** `/api/leadrouting`

**Rule Management:**
| Endpoint | Method |
|----------|--------|
| `GET /rules` | GetAllRules |
| `GET /rules/{id}` | GetRuleById |
| `POST /rules` | CreateRule |
| `PUT /rules/{id}` | UpdateRule |
| `DELETE /rules/{id}` | DeleteRule |
| `POST /rules/{id}/activate` | ActivateRule |
| `POST /rules/{id}/deactivate` | DeactivateRule |

**Criteria Management:**
| Endpoint | Method |
|----------|--------|
| `GET /rules/{ruleId}/criteria` | GetCriteria |
| `POST /rules/{ruleId}/criteria` | AddCriteria |
| `PUT /criteria/{criteriaId}` | UpdateCriteria |
| `DELETE /criteria/{criteriaId}` | RemoveCriteria |

**Target Management:**
| Endpoint | Method |
|----------|--------|
| `GET /rules/{ruleId}/targets` | GetTargets |
| `POST /rules/{ruleId}/targets` | AddTarget |
| `PUT /targets/{targetId}` | UpdateTarget |
| `DELETE /targets/{targetId}` | RemoveTarget |
| `GET /targets/{targetId}/capacity` | GetTargetCapacity |

**Lead Routing Operations:**
| Endpoint | Method |
|----------|--------|
| `POST /leads/{leadId}/route` | RouteLead |
| `POST /leads/{leadId}/route/{ruleId}` | RouteLeadWithRule |
| `POST /leads/{leadId}/evaluate` | EvaluateMatchingRules |
| `POST /leads/batch-route` | RouteLeadsBatch |
| `POST /leads/{leadId}/reroute` | RerouteLead |

**Routing Logs:**
| Endpoint | Method |
|----------|--------|
| `GET /leads/{leadId}/history` | GetLeadRoutingHistory |
| `GET /rules/{ruleId}/logs` | GetRuleRoutingLogs |
| `GET /users/{userId}/logs` | GetUserRoutingLogs |

**Statistics:**
| Endpoint | Method |
|----------|--------|
| `GET /rules/{ruleId}/statistics` | GetRuleStatistics |
| `GET /statistics` | GetOverallStatistics |
| `GET /statistics/response-time` | GetResponseTimeStatistics |

**Capacity Management:**
| Endpoint | Method |
|----------|--------|
| `POST /targets/reset-daily` | ResetDailyCounts |
| `POST /targets/reset-weekly` | ResetWeeklyCounts |

### 6.4 DuplicatesController

**File:** `src/CRM.Api/Controllers/DuplicatesController.cs` (337 lines)

**Route:** `/api/duplicates`

| Endpoint | Method | Description |
|----------|--------|-------------|
| `POST /check` | CheckForDuplicates | Check for duplicates |
| `GET /rules/{entityType}` | GetActiveRules | Get active rules |
| `POST /scan/{entityType}` | ScanForDuplicates | Full scan (Admin/Manager) |
| `GET /candidates/{entityType}` | GetPendingCandidates | Get pending duplicates |
| `POST /merge/preview` | PreviewMerge | Preview merge |
| `POST /merge` | MergeRecords | Merge records |
| `POST /unmerge` | UnmergeRecords | Unmerge records (Admin/Manager) |
| `GET /history/{entityType}/{recordId}` | GetMergeHistory | Get merge history |
| `GET /merged-into/{entityType}/{masterRecordId}` | GetMergedRecords | Get merged records |
| `GET /groups/{mergeGroupId}` | GetMergeGroup | Get merge group |

**Dependencies:**
- `IDuplicateDetectionService` - ✅ Duplicate detection
- `IMergeService` - ✅ Merge operations
- `ILogger<DuplicatesController>` - Logging

---

## 7. DbContext Registrations

**File:** `src/CRM.Core/Interfaces/ICrmDbContext.cs`

### Lead-related DbSets:
```csharp
DbSet<Lead> Leads { get; }
DbSet<LeadProductInterest> LeadProductInterests { get; }
DbSet<LeadRoutingRule> LeadRoutingRules { get; }
DbSet<LeadRoutingCriteria> LeadRoutingCriteria { get; }
DbSet<LeadRoutingTarget> LeadRoutingTargets { get; }
DbSet<LeadRoutingLog> LeadRoutingLogs { get; }
DbSet<LeadScoreRule> LeadScoreRules { get; }
```

### Opportunity-related DbSets:
```csharp
DbSet<Opportunity> Opportunities { get; }
DbSet<OpportunityProduct> OpportunityProducts { get; }
```

### Duplicate Detection DbSets:
```csharp
DbSet<DuplicateRule> DuplicateRules { get; }
DbSet<DuplicateMatchField> DuplicateMatchFields { get; }
DbSet<DuplicateCandidate> DuplicateCandidates { get; }
DbSet<DuplicateMergeHistory> DuplicateMergeHistories { get; }
```

---

## 8. Dependency Injection Registrations

**File:** `src/CRM.Api/Program.cs`

| Line | Registration |
|------|--------------|
| 344 | `builder.Services.AddScoped<IOpportunityService, OpportunityService>();` |
| 425 | `builder.Services.AddScoped<ILeadRoutingService, LeadRoutingService>();` |
| 457 | `builder.Services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();` |
| 458 | `builder.Services.AddScoped<IMergeService, MergeService>();` |

**Also in Microservice:**
- `src/Services/CRM.SalesService/Program.cs` line 45: `AddScoped<IOpportunityService, OpportunityService>()`

---

## 9. GAP ANALYSIS

### 9.1 Critical Gaps (High Priority)

| Gap | Impact | Recommendation |
|-----|--------|----------------|
| **No ILeadService interface** | Inconsistent architecture, LeadsController uses direct DbContext | Create `ILeadService` with full CRUD + business operations |
| **No LeadService implementation** | Business logic mixed in controller | Create `LeadService` implementing ILeadService |
| **LeadsController uses DbContext directly** | Bypasses service layer, no abstraction | Refactor to use ILeadService |

### 9.2 Moderate Gaps (Medium Priority)

| Gap | Impact | Recommendation |
|-----|--------|----------------|
| **No Opportunity DTOs** | Controller accepts raw entities | Create CreateOpportunityDto, UpdateOpportunityDto |
| **DTOs defined inline in controllers** | Code organization issues | Move to `CRM.Core/DTOs/` folder |
| **No ILeadScoreService** | Lead scoring logic scattered | Create dedicated service for scoring |

### 9.3 Minor Gaps (Low Priority)

| Gap | Impact | Recommendation |
|-----|--------|----------------|
| **Inconsistent return types** | Some use anonymous objects, some use DTOs | Standardize on DTOs |
| **Missing validation attributes** | No FluentValidation for Lead DTOs | Add validators |
| **No pagination in some endpoints** | Performance for large datasets | Add pagination support |

### 9.4 Architecture Inconsistencies

| Inconsistency | Details |
|---------------|---------|
| **Service Layer Usage** | OpportunityService ✅, LeadService ❌, LeadRoutingService ✅ |
| **Repository Pattern** | OpportunityService uses IRepository, LeadRoutingService uses ICrmDbContext |
| **DTO Usage** | Leads have DTOs, Opportunities don't |
| **Workflow Integration** | Leads dispatch events, Opportunities dispatch events |

---

## 10. Recommendations

### 10.1 Immediate Actions (Sprint 1)

1. **Create ILeadService Interface**
   - Location: `src/CRM.Core/Interfaces/ILeadService.cs`
   - Methods: GetAll, GetById, Create, Update, Delete, Convert, GetByStatus, GetStats, etc.

2. **Create LeadService Implementation**
   - Location: `src/CRM.Infrastructure/Services/LeadService.cs`
   - Move all logic from LeadsController
   - Use repository pattern or ICrmDbContext consistently

3. **Refactor LeadsController**
   - Inject ILeadService instead of ICrmDbContext
   - Keep controller thin (validation + service calls only)

### 10.2 Short-term Actions (Sprint 2)

4. **Create Opportunity DTOs**
   - Location: `src/CRM.Core/DTOs/OpportunityDtos.cs`
   - CreateOpportunityDto, UpdateOpportunityDto, OpportunityDto

5. **Refactor OpportunitiesController**
   - Accept DTOs instead of raw entities
   - Add validation

6. **Organize DTOs**
   - Move all DTOs from controllers to `CRM.Core/DTOs/` folder
   - Create: LeadDtos.cs, OpportunityDtos.cs, LeadRoutingDtos.cs

### 10.3 Medium-term Actions (Sprint 3-4)

7. **Create ILeadScoreService**
   - Consolidate scoring logic
   - Implement score decay
   - Add scoring analytics

8. **Add Validation**
   - FluentValidation for all DTOs
   - Consistent error responses

9. **Standardize Service Pattern**
   - Decide: Repository pattern vs DbContext
   - Apply consistently across all services

### 10.4 Implementation Priority Matrix

| Priority | Item | Effort | Impact |
|----------|------|--------|--------|
| P0 | Create ILeadService/LeadService | Medium | High |
| P0 | Refactor LeadsController | Medium | High |
| P1 | Create Opportunity DTOs | Low | Medium |
| P1 | Move DTOs to CRM.Core | Low | Medium |
| P2 | Add FluentValidation | Medium | Medium |
| P2 | Create ILeadScoreService | Medium | Medium |
| P3 | Standardize repository usage | High | Low |

---

**END OF AUDIT REPORT**
