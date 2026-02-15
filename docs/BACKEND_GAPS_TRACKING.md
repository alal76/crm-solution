# Backend Gaps - Detailed Tracking & Implementation Checklist

> **Purpose:** Detailed task-level checklist for implementation teams  
> **Last Updated:** February 15, 2026  
> **Status:** Ready for sprint planning  

---

## P0 BLOCKERS - IMPLEMENT IMMEDIATELY

### 🔴 SALES-007.001: Commission Management API Implementation

**Epic:** Complete commission management API layer  
**Effort:** 16 hours  
**Priority:** P0  
**Dependencies:** None  

#### Sprint Tasks

- [ ] **SALES-007.001.1** - Create CommissionPlanService (4h)
  - [ ] Design plan assignment logic
  - [ ] Implement CRUD methods
  - [ ] Add validation
  - [ ] Write unit tests (8 tests)
  - [ ] Estimate: 4 hours

- [ ] **SALES-007.001.2** - Create CommissionStatementService (3h)
  - [ ] Statement generation logic
  - [ ] Period calculation
  - [ ] Payout integration point
  - [ ] Write unit tests (6 tests)
  - [ ] Estimate: 3 hours

- [ ] **SALES-007.001.3** - Add CommissionsController endpoints (4h)
  - [ ] `POST /api/commission-plans` - Create plan
  - [ ] `GET /api/commission-plans` - List plans
  - [ ] `GET /api/commission-plans/{id}` - Get plan
  - [ ] `PUT /api/commission-plans/{id}` - Update plan
  - [ ] `POST /api/commission-plans/{id}/assign` - Assign plan
  - [ ] `GET /api/commission-statements` - List statements
  - [ ] `POST /api/commission-statements` - Generate statement
  - [ ] `POST /api/commission-statements/{id}/finalize` - Finalize
  - [ ] Write integration tests (8 tests)
  - [ ] Estimate: 4 hours

- [ ] **SALES-007.001.4** - Create DTOs (2h)
  - [ ] CommissionPlanDto
  - [ ] CreateCommissionPlanDto
  - [ ] UpdateCommissionPlanDto
  - [ ] CommissionTierDto
  - [ ] CommissionStatementDto
  - [ ] Write validation tests
  - [ ] Estimate: 2 hours

- [ ] **SALES-007.001.5** - Add validation rules (2h)
  - [ ] Commission amount >= 0
  - [ ] Plan rate 0-100%
  - [ ] Tier non-overlap detection
  - [ ] Effective date validation
  - [ ] Estimate: 2 hours

- [ ] **SALES-007.001.6** - Test & documentation (1h)
  - [ ] Swagger docs
  - [ ] API contract validation
  - [ ] README update
  - [ ] Estimate: 1 hour

---

### 🔴 ITSM-002.001: Problem Management Service Implementation

**Epic:** Complete problem management module  
**Effort:** 40 hours  
**Priority:** P0  
**Dependencies:** IncidentService (complete)  

#### Sprint 1 (20h): Foundation

- [ ] **ITSM-002.001.1** - Create IProblemService interface (2h)
  - [ ] GetProblemByIdAsync
  - [ ] GetAllProblemsAsync
  - [ ] CreateProblemAsync
  - [ ] UpdateProblemAsync
  - [ ] UpdateStatusAsync
  - [ ] DeleteProblemAsync
  - [ ] SearchProblemsAsync
  - [ ] MergeDuplicatesAsync
  - [ ] Estimate: 2 hours

- [ ] **ITSM-002.001.2** - Create Problem DTOs (3h)
  - [ ] ProblemDto
  - [ ] CreateProblemDto
  - [ ] UpdateProblemDto
  - [ ] ProblemDetailsDto (with RCA, KE, incidents)
  - [ ] ProblemFilterDto
  - [ ] Write schema validation tests
  - [ ] Estimate: 3 hours

- [ ] **ITSM-002.001.3** - Implement ProblemService (8h)
  - [ ] GetProblemByIdAsync
  - [ ] GetAllProblemsAsync with filtering
  - [ ] CreateProblemAsync with validation
  - [ ] UpdateProblemAsync
  - [ ] UpdateStatusAsync with state machine
  - [ ] DeleteProblemAsync (soft delete)
  - [ ] SearchProblemsAsync with Meilisearch
  - [ ] GetTrendProblemsAsync
  - [ ] Write unit tests (24 tests)
  - [ ] Estimate: 8 hours

- [ ] **ITSM-002.001.4** - Create ProblemsController (4h)
  - [ ] [HttpGet] GetAll
  - [ ] [HttpGet("{id}")] GetById
  - [ ] [HttpPost] Create
  - [ ] [HttpPut("{id}")] Update
  - [ ] [HttpDelete("{id}")] Delete
  - [ ] [HttpPatch("{id}/status")] UpdateStatus
  - [ ] [HttpGet("search")] Search
  - [ ] [HttpPost("merge")] MergeDuplicates
  - [ ] Write integration tests (8 tests)
  - [ ] Estimate: 4 hours

- [ ] **ITSM-002.001.5** - Add validation rules (3h)
  - [ ] Title: 10-500 chars
  - [ ] Description: 20-5000 chars
  - [ ] Category must exist
  - [ ] Status transitions validated
  - [ ] Estimate: 3 hours

#### Sprint 2 (20h): RCA & Known Errors

- [ ] **ITSM-002.002.1** - Create IRCAConductor interface (1h)
  - [ ] StartRCAAsync
  - [ ] SaveInvestigationNodeAsync
  - [ ] SubmitRCAAsync
  - [ ] Estimate: 1 hour

- [ ] **ITSM-002.002.2** - Create RCAConductor service (10h)
  - [ ] RCA session management
  - [ ] Investigation tree builder
  - [ ] 5-Whys framework implementation
  - [ ] Evidence collection coordination
  - [ ] RCA confidence scoring
  - [ ] Report generation
  - [ ] Write unit tests (15 tests)
  - [ ] Estimate: 10 hours

- [ ] **ITSM-002.002.3** - Create IKnownErrorService interface (1h)
  - [ ] CreateKnownErrorAsync
  - [ ] GetKnownErrorByIdAsync
  - [ ] SearchByComponentAsync
  - [ ] PublishAsync
  - [ ] Estimate: 1 hour

- [ ] **ITSM-002.002.4** - Implement KnownErrorService (6h)
  - [ ] Full CRUD
  - [ ] Publication workflow
  - [ ] Component/version matching
  - [ ] Auto-linking to incidents
  - [ ] Deprecation handling
  - [ ] Write unit tests (12 tests)
  - [ ] Estimate: 6 hours

- [ ] **ITSM-002.002.5** - Create RCA DTOs (2h)
  - [ ] RCASessionDto
  - [ ] RCANodeDto
  - [ ] RCAResultDto
  - [ ] KnownErrorDto
  - [ ] Estimate: 2 hours

---

### 🔴 ITSM-003.001: Change Management Service Implementation

**Epic:** Complete change management module  
**Effort:** 48 hours  
**Priority:** P0  
**Dependencies:** ProblemService (must complete first), ServiceRequestService (complete)  

#### Sprint 1 (24h): Foundation

- [ ] **ITSM-003.001.1** - Create IChangeService interface (2h)
  - [ ] CreateChangeAsync
  - [ ] GetChangeByIdAsync
  - [ ] GetAllChangesAsync
  - [ ] UpdateChangeAsync
  - [ ] UpdateStatusAsync
  - [ ] ScheduleChangeAsync
  - [ ] ExecuteChangeAsync
  - [ ] RollbackChangeAsync
  - [ ] Estimate: 2 hours

- [ ] **ITSM-003.001.2** - Create Change DTOs (3h)
  - [ ] ChangeDto (all statuses)
  - [ ] CreateChangeDto
  - [ ] UpdateChangeDto
  - [ ] ChangeDetailDto (with impact, CAB votes, schedule)
  - [ ] ChangeImpactDto
  - [ ] SchedulingConflictDto
  - [ ] Estimate: 3 hours

- [ ] **ITSM-003.001.3** - Implement ChangeService (12h)
  - [ ] CreateChangeAsync with risk calculation
  - [ ] GetChangeByIdAsync
  - [ ] GetAllChangesAsync with filtering
  - [ ] UpdateChangeAsync
  - [ ] UpdateStatusAsync with state machine
  - [ ] ScheduleChangeAsync with conflict check
  - [ ] ExecuteChangeAsync with implementation tracking
  - [ ] Write unit tests (18 tests)
  - [ ] Estimate: 12 hours

- [ ] **ITSM-003.001.4** - Create IChangeConflictDetector interface (1h)
  - [ ] DetectConflictsAsync
  - [ ] GetAlternativeTimesAsync
  - [ ] CheckBlackoutWindowAsync
  - [ ] Estimate: 1 hour

- [ ] **ITSM-003.001.5** - Implement ChangeConflictDetector (4h)
  - [ ] Conflict detection algorithm
  - [ ] Alternative time suggestion
  - [ ] Blackout window checking
  - [ ] Write unit tests (8 tests)
  - [ ] Estimate: 4 hours

- [ ] **ITSM-003.001.6** - Create ChangesController (2h)
  - [ ] Basic CRUD endpoints
  - [ ] Schedule endpoint
  - [ ] Execute endpoint
  - [ ] Write integration tests (6 tests)
  - [ ] Estimate: 2 hours

#### Sprint 2 (24h): CAB & Advanced Features

- [ ] **ITSM-003.002.1** - Create ICABApprovalService interface (1h)
  - [ ] SubmitForApprovalAsync
  - [ ] VoteAsync
  - [ ] GetVotingStatusAsync
  - [ ] GetPendingChangesAsync
  - [ ] Estimate: 1 hour

- [ ] **ITSM-003.002.2** - Implement CABApprovalService (8h)
  - [ ] Voting workflow
  - [ ] Decision calculation (majority, unanimous, etc.)
  - [ ] Comment/feedback tracking
  - [ ] Voting history
  - [ ] Write unit tests (12 tests)
  - [ ] Estimate: 8 hours

- [ ] **ITSM-003.002.3** - Create BlackoutWindowService (6h)
  - [ ] CRUD operations
  - [ ] Recurrence pattern handling
  - [ ] Blackout check algorithm
  - [ ] Calendar integration support
  - [ ] Write unit tests (10 tests)
  - [ ] Estimate: 6 hours

- [ ] **ITSM-003.002.4** - Create ImpactAnalysisService enhancements (4h)
  - [ ] Affected CI discovery
  - [ ] Service dependency mapping
  - [ ] Downtime estimation
  - [ ] Risk scoring
  - [ ] Write unit tests (8 tests)
  - [ ] Estimate: 4 hours

- [ ] **ITSM-003.002.5** - Add validation rules (3h)
  - [ ] Change title/description lengths
  - [ ] Status transition validation
  - [ ] CAB voting minimum requirement
  - [ ] Rollback plan mandatory for HIGH risk
  - [ ] Estimate: 3 hours

- [ ] **ITSM-003.002.6** - Create remaining DTOs (2h)
  - [ ] CABVoteDto
  - [ ] RollbackPlanDto
  - [ ] BlackoutWindowDto
  - [ ] Estimate: 2 hours

---

## P1 FEATURES - NEXT SPRINT

### 🟡 SALES-002.001: Order Management Frontend Components

**Note:** Backend is COMPLETE ✅  
**Effort:** 20 hours (FRONTEND ONLY - OUT OF SCOPE)  
**Priority:** P1  

**Frontend Components Needed:**
- [ ] OrderDetailsPage.tsx
- [ ] OrderForm.tsx
- [ ] OrderLineItemsTable.tsx
- [ ] OrderStatusBadge.tsx
- [ ] OrderTimeline.tsx
- [ ] OrderSummary.tsx
- [ ] OrderAddressCard.tsx
- [ ] OrderActionButtons.tsx
- [ ] OrderStatisticsCard.tsx

---

### 🟡 MKT-001.001: Marketing Campaign Execution

**Epic:** Complete campaign execution API  
**Effort:** 24 hours  
**Priority:** P1  
**Dependencies:** CampaignService (complete)  

#### Tasks

- [ ] **MKT-001.001.1** - Create CampaignMetricsService (6h)
  - [ ] GetMetricsAsync
  - [ ] AggregateMetricsAsync
  - [ ] CalculateROIAsync
  - [ ] TrackConversionAsync
  - [ ] GenerateReportAsync
  - [ ] Write unit tests (10 tests)
  - [ ] Estimate: 6 hours

- [ ] **MKT-001.001.2** - Create CampaignRecipientService (5h)
  - [ ] GetRecipientsAsync
  - [ ] AddRecipientsAsync
  - [ ] RemoveRecipientsAsync
  - [ ] FilterRecipientsAsync (by segment, status, etc.)
  - [ ] ValidateRecipientsAsync
  - [ ] Write unit tests (8 tests)
  - [ ] Estimate: 5 hours

- [ ] **MKT-001.001.3** - Add campaign execution endpoints (8h)
  - [ ] `GET /api/campaigns/{id}/metrics` - Get metrics
  - [ ] `POST /api/campaigns/{id}/metrics` - Record metric
  - [ ] `GET /api/campaigns/{id}/recipients` - List recipients
  - [ ] `POST /api/campaigns/{id}/recipients` - Add recipient
  - [ ] `POST /api/campaigns/{id}/launch` - Launch campaign
  - [ ] `POST /api/campaigns/{id}/pause` - Pause campaign
  - [ ] `POST /api/campaigns/{id}/resume` - Resume campaign
  - [ ] `GET /api/campaigns/{id}/performance` - Get performance
  - [ ] Write integration tests (8 tests)
  - [ ] Estimate: 8 hours

- [ ] **MKT-001.001.4** - Create DTOs (3h)
  - [ ] CampaignMetricsDto
  - [ ] CampaignRecipientDto
  - [ ] CampaignPerformanceDto
  - [ ] Estimate: 3 hours

- [ ] **MKT-001.001.5** - Add validation rules (2h)
  - [ ] Campaign status transitions
  - [ ] Recipient filtering validation
  - [ ] Metrics data validation
  - [ ] Estimate: 2 hours

---

### 🟡 INT-001.001: Webhook Delivery & Retry

**Epic:** Complete webhook delivery infrastructure  
**Effort:** 28 hours  
**Priority:** P1  
**Dependencies:** WebhookService (exists, needs enhancement)  

#### Tasks

- [ ] **INT-001.001.1** - Create WebhookDeliveryService (12h)
  - [ ] SendWebhookAsync
  - [ ] RetryWebhookAsync with exponential backoff
  - [ ] GetDeliveryHistoryAsync
  - [ ] MarkDeliverySuccessAsync
  - [ ] MarkDeliveryFailedAsync
  - [ ] DetectDeadWebhooksAsync (auto-disable after 5 failures)
  - [ ] Write unit tests (15 tests)
  - [ ] Estimate: 12 hours

- [ ] **INT-001.001.2** - Create WebhookSignatureService (3h)
  - [ ] GenerateSignatureAsync (HMAC-SHA256)
  - [ ] VerifySignatureAsync
  - [ ] Write unit tests (6 tests)
  - [ ] Estimate: 3 hours

- [ ] **INT-001.001.3** - Create EventFilteringService (4h)
  - [ ] ParseEventFilterAsync
  - [ ] MatchEventAsync
  - [ ] TransformPayloadAsync
  - [ ] Write unit tests (6 tests)
  - [ ] Estimate: 4 hours

- [ ] **INT-001.001.4** - Add webhook endpoints (6h)
  - [ ] `GET /api/webhooks/{id}/deliveries` - Delivery history
  - [ ] `GET /api/webhooks/{id}/deliveries/{deliveryId}` - Get delivery
  - [ ] `POST /api/webhooks/{id}/deliveries/{deliveryId}/retry` - Retry
  - [ ] `POST /api/webhooks/{id}/test` - Test webhook
  - [ ] `GET /api/webhooks/{id}/analytics` - Analytics
  - [ ] `POST /api/webhooks/{id}/disable` - Disable webhook
  - [ ] Write integration tests (6 tests)
  - [ ] Estimate: 6 hours

- [ ] **INT-001.001.5** - Create DTOs (2h)
  - [ ] WebhookDeliveryDto
  - [ ] WebhookTestResponseDto
  - [ ] WebhookAnalyticsDto
  - [ ] Estimate: 2 hours

- [ ] **INT-001.001.6** - Add validation (1h)
  - [ ] URL format and HTTPS enforcement
  - [ ] Signature verification
  - [ ] Event type validation
  - [ ] Estimate: 1 hour

---

### 🟡 MKT-002/003.001: Email Templates & Sequences

**Epic:** Complete email automation framework  
**Effort:** 16 hours  
**Priority:** P1  
**Dependencies:** EmailTemplateService (partial)  

#### Tasks

- [ ] **MKT-002.001.1** - Enhance EmailTemplateService (6h)
  - [ ] GetVersionsAsync
  - [ ] PublishAsync
  - [ ] PreviewAsync
  - [ ] CloneAsync
  - [ ] GetTemplateVariantsAsync (for A/B)
  - [ ] Write unit tests (10 tests)
  - [ ] Estimate: 6 hours

- [ ] **MKT-003.001.1** - Enhance EmailSequenceService (6h)
  - [ ] ExecuteSequenceAsync
  - [ ] GetSequenceProgressAsync
  - [ ] UpdateStepAsync (conditional branching)
  - [ ] GetSequenceStatsAsync
  - [ ] Write unit tests (10 tests)
  - [ ] Estimate: 6 hours

- [ ] **MKT-002/003.001.2** - Add template/sequence endpoints (3h)
  - [ ] `GET /api/email-templates/{id}/versions` - Versions
  - [ ] `POST /api/email-templates/{id}/publish` - Publish
  - [ ] `POST /api/email-templates/{id}/preview` - Preview
  - [ ] `GET /api/email-sequences/{id}/progress` - Progress
  - [ ] `GET /api/email-sequences/{id}/stats` - Stats
  - [ ] Write integration tests (5 tests)
  - [ ] Estimate: 3 hours

- [ ] **MKT-002/003.001.3** - Create DTOs (1h)
  - [ ] EmailTemplateVersionDto
  - [ ] EmailSequenceProgressDto
  - [ ] EmailSequenceStatsDto
  - [ ] Estimate: 1 hour

---

## P2 FEATURES - LATER SPRINTS

### 🟠 ITSM-001.001: Incident Validation & Escalation Enhancement

**Effort:** 20 hours  
**Priority:** P2  

**Tasks:**
- [ ] Enhanced impact analysis validation (3h)
- [ ] Skill-based assignment suggestion algorithm (4h)
- [ ] SLA breach calculation (3h)
- [ ] Escalation rule engine (6h)
- [ ] Auto-incident linking (2h)
- [ ] Unit tests (all above)
- [ ] Integration tests

---

### 🟠 MKT-004/005.001: Web Forms & Tracking (Not Started)

**Effort:** 24 hours  
**Priority:** P2  

**Services Needed:**
- FormBuilderService
- WebTrackingService
- PixelTrackingService

**Endpoints:** 15+

---

## P3 ENHANCEMENTS - FUTURE

### 🟢 SALES-007.002: Advanced Commission Features

**Effort:** 12 hours  
**Priority:** P3  

- [ ] Commission tier-based calculations
- [ ] Multi-rep commission splits
- [ ] Clawback automation
- [ ] Commission forecasting

### 🟢 ITSM-004.001: CMDB Advanced Features

**Effort:** 16 hours  
**Priority:** P3  

- [ ] Service map generation
- [ ] Dependency graph calculation
- [ ] CI health scoring

---

## SUMMARY TABLE

| Gap ID | Module | Feature | Status | Effort | P | Tasks |
|--------|--------|---------|--------|--------|---|-------|
| SALES-007.001 | Sales | Commission API | ❌ | 16h | P0 | 6 |
| ITSM-002.001 | ITSM | Problem Mgmt | ❌ | 40h | P0 | 10 |
| ITSM-003.001 | ITSM | Change Mgmt | ❌ | 48h | P0 | 12 |
| SALES-002.001 | Sales | Order UI | ❌ | 20h | P1 | 9 |
| MKT-001.001 | Mkt | Campaign Exec | ⚠️ | 24h | P1 | 5 |
| INT-001.001 | Int | Webhook | ⚠️ | 28h | P1 | 6 |
| MKT-002/003 | Mkt | Email | ⚠️ | 16h | P1 | 3 |
| ITSM-001.001 | ITSM | Incident Enh | ⚠️ | 20h | P2 | 5 |
| MKT-004/005 | Mkt | Forms/Track | ❌ | 24h | P2 | 5 |
| SALES-007.002 | Sales | Comm Adv | ❌ | 12h | P3 | 4 |
| ITSM-004.001 | ITSM | CMDB Adv | ❌ | 16h | P3 | 3 |

**Total:** 264 hours

---

## IMPLEMENTATION TEAMS

### Phase 1 Teams (Weeks 1-2)

**Team A - Sales Commission (4 people, 16h)**
- 1 Senior backend engineer
- 1 Mid-level engineer
- 1 QA engineer
- 1 Tech writer
- Delivers: CommissionService, DTOs, API, tests

**Team B - ITSM Problem (5 people, 40h)**
- 2 Senior backend engineers
- 2 Mid-level engineers
- 1 QA engineer
- Delivers: ProblemService, RCA, KnownError, API, tests

**Team C - Marketing Campaign (3 people, 24h)**
- 1 Senior backend engineer
- 1 Mid-level engineer
- 1 QA engineer
- Delivers: CampaignServices, endpoints, tests

---

## Sign-Off Checklist

When implementation is complete:

- [ ] All tasks marked complete
- [ ] All unit tests passing (100% coverage)
- [ ] All integration tests passing
- [ ] All DTOs implemented
- [ ] All validation rules added
- [ ] API documentation updated
- [ ] Database migrations applied
- [ ] Feature flags added to config
- [ ] Seed data created
- [ ] Code reviewed and merged
- [ ] Deployed to staging
- [ ] E2E tests passing
- [ ] Performance tested
- [ ] Security reviewed

---

**Generated:** February 15, 2026  
**Next Update:** After Phase 1 completion  
**Maintained By:** Engineering Lead  
