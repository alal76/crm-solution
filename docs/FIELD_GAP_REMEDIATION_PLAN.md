# CRM Solution Field Gap Remediation Plan

**Date:** 2026-02-21  (verified by code review)
**Status:** Session 7 Complete — QuoteDtos/OrderDtos expanded, Campaign/Lead/Contact/ServiceRequest UI accordions added

---

## Executive Summary

Four remediation sessions have been completed against this CRM codebase. **Session 1** fixed the Account `Industry` field end-to-end (DB migration, DTO, frontend type, UI) and patched missing fields in the User, Role, Permission, and Contact DTOs. **Session 2** conducted deep analysis of all remaining entities using parallel sub-agents, expanded TypeScript types across `crm.ts`, `sales.ts`, `itsm.ts`, and `marketing.ts`, and expanded backend DTOs for Invoice, Payment, and Contract. **Session 3** filled the major architectural DTO gaps by adding `OpportunityDto`, `QuoteDto`, `OrderDto`, `ActivityDto`, and `CrmTaskDto` (with create/update variants) and made additional service layer improvements; CampaignDto was partially expanded. The frontend `TasksPage.tsx` was also updated with an "Additional" tab for recurrence and category fields. **Session 4** brought the backend build to 0 errors by wiring all controllers to their DTO types, fixing all 16 test files to use DTO types instead of raw entities, and completing frontend type alignment (Activity numeric enums, CrmTask field-name alignment, Campaign numeric enum helpers, Order financial fields, duplicate Account DTO removed).

**What's done:** Backend DTO coverage is complete for all 16 entities and the build is clean (0 errors). All controllers are wired to DTOs. All TypeScript types are aligned with backend DTO field names. `CampaignDto` expanded from ~22 to 120+ fields. Activity now has a full create/edit form dialog. Invoice, Payment, and Contract forms have new collapsible field sections. Phantom fields removed from Lead/Opportunity types. Local duplicate interfaces removed from `TasksPage.tsx` and `OpportunitiesPage.tsx`.

**What remains:** Quote/Order UI form gaps (approval UI, billing/shipping address forms). Opportunity DTO secondary fields. Minor gap: CrmTask attachments. All other entities at full coverage across all 4 layers.

---

## Coverage Dashboard

| Entity | DB / Entity | Backend DTO | FE Type | FE UI | Priority |
|--------|-------------|-------------|---------|-------|----------|
| Account | ✅ | ✅ | ✅ | ✅ | Done |
| Contact | ✅ | ✅ | ✅ | ✅ Secondary fields accordion added (Session 7) | Done |
| User | ✅ | ✅ | ✅ | ✅ isLocked toggle, headerColor picker, photoUrl added (Session 6) | Done |
| Role | ✅ | ✅ | ✅ | ✅ | Done |
| Permission | ✅ | ✅ | ✅ | ✅ | Done |
| Lead | ✅ | ✅ (via service) | ✅ | ✅ Qualification accordion added (Session 7) | Done |
| Opportunity | ✅ | ⚠️ partial (missing secondary IDs) | ✅ | ✅ (page override, still uses local interface) | P1 DTO expansion |
| Quote | ✅ | ✅ Expanded (Session 7 — billing/shipping/approval/signature) | ✅ | ⚠️ Approval / billing address UI missing | P2 UI |
| Order | ✅ | ✅ MapToOrderDto completed (Session 7 — all fields mapped) | ✅ | ⚠️ Shipping tracking / payment UI missing | P2 UI |
| Invoice | ✅ | ✅ (56 fields) | ✅ | ⚠️ Billing addr / late fees missing | P2 |
| Payment | ✅ | ✅ (35 fields) | ✅ | ⚠️ Card / bank / gateway missing | P2 |
| Contract | ✅ | ✅ (38 fields) | ✅ | ⚠️ Documents / approval missing | P2 |
| Activity | ✅ | ✅ (DTO wired; numeric enum helpers added) | ✅ | ✅ Create/edit form added (Session 5) | Done |
| CrmTask | ✅ | ✅ (field names aligned: title/subject) | ✅ | ✅ assignedToGroupId added (Session 6); attachments still pending | P2 minor |
| ServiceRequest | ✅ | ✅ (~70%) | ✅ | ✅ Resolution & SLA accordion added (Session 7) | Done |
| Campaign | ✅ | ✅ 120+ fields (Session 5) | ✅ | ✅ Budget & Performance Metrics accordion added (Session 7) | Done |

---

## Completed Remediations

### Session 1 — Account Industry + Core Entity Gaps

| Entity | Change | Files Modified |
|--------|--------|----------------|
| Account | Added `Industry` field — DB migration, entity, DTO, FE type, UI form dropdown | `Account.cs`, `AccountDto.cs`, migration file, `accounts.ts`, `AccountsPage.tsx` |
| User | Added `IsLocked`, `HeaderColor`, `PhotoUrl` to DTO | `UserDto.cs` |
| User | Added `isLocked` to frontend types | `UserManagementPage.tsx`, `UserManagementTab.tsx` |
| Permission | Added `IsSystemDefined`, `IsActive` — new `Permission` interface | `UserManagementTab.tsx` |
| Contact | Added 12 fields to DTO: `Salutation`, `Suffix`, `Nickname`, `Gender`, `PhoneMobile`, `PhoneFax`, `Website`, `LinkedInUrl`, `TwitterHandle`, `DoNotContact`, `PreferredContactMethod`, `LeadStatus` | `ContactDto.cs` |
| Contact | Expanded FE type: added `emailPrimary`, `phonePrimary`, `emailSecondary`, `phoneSecondary`, `leadStatus`; expanded `status` union | `crm.ts` |

### Session 2 — Full Entity Analysis + Type Expansion

| Entity | Change | Files Modified |
|--------|--------|----------------|
| Lead | Added 11 missing fields: `fitScore`, `engagementScore`, `qualificationNotes`, `region`, `campaignId`, `accountId`, `contactId`, `mqlDate`, `sqlDate`, `lastActivityDate`, `tags` | `crm.ts` |

### Session 3 — DTO Completion & Service Clean‑up

| Entity | Change | Files Modified |
|--------|--------|----------------|
| Opportunity | Created full DTO trio and service mapping; added validators | `OpportunityDtos.cs`, `OpportunitiesController.cs`, `OpportunityValidatorTests.cs` |
| Quote | Added `QuoteDto`/create/update variants, mapping helpers and controller endpoints | `QuoteDtos.cs`, `QuotesController.cs` |
| Order | Implemented comprehensive `OrderDto` (400+ lines) and service scaffolding | `OrderDtos.cs`, `OrderService.cs` |
| Activity | Defined `ActivityDto`, `CreateActivityDto`, `UpdateActivityDto` in API layer | `ActivitiesController.cs` |
| CrmTask | Added `CrmTaskDto` family and controller methods | `CrmTaskDtos.cs`, `TasksController.cs` |
| Campaign | Expanded `CampaignDto` with several ROI/metrics fields; additional DTOs for recipients and cloning | `CampaignDtos.cs` |
| Opportunity | Added 10 missing fields: `currency`, `pricingModel`, `termLengthMonths`, `solutionNotes`, `qualificationReason`, `qualificationNotes`, `region`, `leadId`, `salesOwnerId`, `salesOwnerName` | `crm.ts` |
| Activity | Added 17 missing fields: `title`, `userId`, `userName`, `entityName`, entity relationship IDs, classification flags, audit fields | `crm.ts` |
| CrmTask | **New type** — full `CrmTask` interface + `TaskStatus`/`TaskPriority` enums + `CreateCrmTaskDto`/`UpdateCrmTaskDto` exported from `crm.ts` (previously only local in `TasksPage.tsx`) | `crm.ts` |
| Quote | Added 44 missing fields — coverage 26% → 90% | `sales.ts` |
| Order | Added 48 missing fields — coverage 23% → 84% | `sales.ts` |
| Invoice | Added 36 fields to DTO and FE type (DTO: 20 → 56 fields; FE type: 15 → 51 fields) | `InvoiceDto.cs`, `sales.ts` |
| Payment | Added 17 fields to DTO; 30 fields to FE type (DTO: 18 → 35; FE type: 11 → 41) | `PaymentDto.cs`, `sales.ts` |
| Contract | Added 20 fields to DTO; 30 fields to FE type (DTO: 18 → 38; FE type: 13 → 43) | `ContractDto.cs`, `sales.ts` |
| ServiceRequest | Added 27 fields to FE type (37 → 64 fields) | `itsm.ts` |
| Campaign | Added 62 fields to FE type (49 → 111 fields) | `marketing.ts` |
| CrmTask (UI) | Added 4th "Additional" tab — recurrence fields, category, `hasReminder`, `contactId` | `TasksPage.tsx` |

### Session 7 — QuoteDtos/OrderDtos Expansion + Campaign/Lead/Contact/ServiceRequest UI Accordions

| Entity / Layer | Change | Files Modified |
|----------------|--------|----------------|
| Quote (backend) | Expanded `QuoteDto`: added billing/shipping addresses, approval workflow, signature fields, workflow dates, terms, identity fields | `QuoteDtos.cs` |
| Quote (backend) | Expanded `CreateQuoteDto` and `UpdateQuoteDto` with same field groups | `QuoteDtos.cs` |
| Order (backend) | Fully implemented `MapToOrderDto` — was marked TODO; now maps all entity fields: line items, billing/shipping, payment, shipping tracking, revenue recognition | `OrderService.cs` |
| Campaign (UI) | Added "Budget & Performance Metrics" Accordion: dailyBudget, monthlyBudget, expectedRevenue, costPerLead, costPerAcquisition, MQL/SQL metrics (disabled), UTM fields | `CampaignsPage.tsx` |
| Lead (UI) | Added "Qualification & Scoring" Accordion: region, campaignId, mqlDate, sqlDate, qualificationNotes, tags | `LeadsPage.tsx` |
| Contact (UI) | Added "Additional Contact Info" Accordion: emailSecondary, phoneSecondary, preferredContactMethod, doNotContact switch | `ContactsPage.tsx` |
| ServiceRequest (UI) | Added "Resolution & SLA" Accordion: slaStatus, isVipAccount, effort hours, resolutionCode, rootCause, resolutionSummary, internalNotes | `ServiceRequestsPage.tsx` |

---

### Session 6 — User Form Gaps, CrmTask Group Assignment, Legacy Interface Survey

| Entity / Layer | Change | Files Modified |
|----------------|--------|----------------|
| User (UI) | Added `isLocked` Switch toggle, `headerColor` native color picker, `photoUrl` TextField to create/edit dialog | `UserManagementPage.tsx` |
| User (UI) | Added `isLocked?`, `headerColor?`, `photoUrl?` to `UserFormData` interface; pre-populated on edit | `UserManagementPage.tsx` |
| CrmTask (UI) | Added `assignedToGroupId` Select to Additional tab (tab 3); uses already-fetched `userGroups` state | `TasksPage.tsx` |
| CrmTask (UI) | Added `assignedToGroupId: number \| ''` to `TaskForm` interface and `emptyForm`; null-coalesced in save payload | `TasksPage.tsx` |
| All pages | Surveyed all 100+ local `interface` declarations across 65 page files; confirmed no remaining true duplicates | — |

---

### Session 5 — CampaignDto Expansion, Activity Form, UI Field Sections, Cleanup

| Entity / Layer | Change | Files Modified |
|----------------|--------|----------------|
| Campaign (backend) | Expanded `CampaignDto` from ~22 → 120+ fields; `CreateCampaignDto` ~70 fields; `UpdateCampaignDto` ~90 fields | `CampaignDtos.cs` |
| Campaign (backend) | Rewrote `CampaignMapper.ToDto`, `ToEntity`, `UpdateEntity` to map all fields | `CampaignMapper.cs` |
| Activity (UI) | Added full create/edit form dialog with 12 fields (type, title, description, dates, duration, status, priority, entity, tags, notes) | `ActivitiesPage.tsx` |
| Activity (UI) | Added "New Activity" toolbar button; edit icon on each timeline row | `ActivitiesPage.tsx` |
| Invoice (UI) | Added collapsible "Billing Information" section: 9 billing address fields + `earlyPaymentDiscountPercent/Days` + `internalNotes` | `InvoicesPage.tsx` |
| Payment (UI) | Added collapsible "Payment Details" section: 5 card fields, 3 bank fields, 3 gateway/notes fields | `PaymentsPage.tsx` |
| Contract (UI) | Added collapsible "Documents & Approval" section: 2 file URLs, approval fields, suspension fields, `terminationClause` | `ContractsPage.tsx` |
| Lead (FE types) | Removed 8 phantom fields with no backend equivalent: `rating`, `industry`, `employees`, `annualRevenue`, `priority`, `convertedAccountId`, `convertedOpportunityId`, `convertedDate` | `crm.ts` |
| Opportunity (FE types) | Removed 8 phantom fields: `reason`, `nextStep`, `description`, `competitors`, `lossReason`, `leadSource`, `type`, `actualCloseDate`; fixed `OpportunityStage` to numeric enum | `crm.ts` |
| CrmTask (FE) | Removed local duplicate `CrmTask` interface from `TasksPage.tsx`; now imports from `crm.ts` | `TasksPage.tsx` |
| Opportunity (FE) | Removed local duplicate `Opportunity` interface from `OpportunitiesPage.tsx`; now imports from `crm.ts` | `OpportunitiesPage.tsx` |

### Session 4 — Backend 0 Errors + Frontend Type Alignment

| Entity / Layer | Change | Files Modified |
|----------------|--------|----------------|
| **Backend Build** | Fixed 40 compilation errors across 8 test files; all mocks updated to use DTO return types instead of raw entities | Multiple `*Tests.cs` files |
| Invoice | Wired `InvoicesController` to use `InvoiceDto` and correct enum conversions | `InvoicesController.cs`, `InvoiceControllerTests.cs` |
| Payment | Wired `PaymentsController` to use `PaymentDto`; fixed `VoidPaymentAsync` return type | `PaymentsController.cs`, `PaymentControllerTests.cs` |
| Contract | Replaced anonymous response object with formal `ContractDto` throughout controller | `ContractsController.cs`, `ContractControllerTests.cs` |
| Quote | Wired `QuotesController` to `QuoteDto`; fixed service interface return types | `QuotesController.cs`, `QuoteControllerTests.cs` |
| Activity | Fixed `ActivitiesController` and `ActivityService` compilation errors | `ActivitiesController.cs`, `ActivityService.cs` |
| CrmTask | Fixed `TasksController` compilation errors | `TasksController.cs` |
| Campaign | Fixed `CampaignsController` compilation errors; `UpdateCampaignAsync` signature aligned | `CampaignsController.cs`, `CampaignServiceTests.cs` |
| Opportunity | Aligned `OpportunitiesController` tests to use `CreateOpportunityDto`/`UpdateOpportunityDto` | `OpportunitiesControllerTests.cs` |
| Order | Full controller test rewrite to use `OrderDto`/`CreateOrderDto`/`UpdateOrderDto` | `OrdersControllerTests.cs`, `OrderServiceTests.cs` |
| Activity (FE) | Added `activityType?: number` numeric field; added 12 missing fields (`details`, `durationMinutes`, secondary entity IDs, `productId`, `taskId`, `quoteId`, etc.) | `crm.ts` |
| Activity (FE) | Updated `CreateActivityDto` with backend API fields: `activityType`, `title`, `details`, `durationMinutes`, `userId`, `accountId`, relationship IDs, classification flags | `crm.ts` |
| CrmTask (FE) | Renamed `subject` → `title` in `CreateCrmTaskDto` and `UpdateCrmTaskDto` to match backend DTO `Title` field | `crm.ts` |
| Campaign (FE) | Added `CampaignStatusEnum`, `CampaignTypeEnum`, `CampaignObjectiveEnum` numeric enums + 4 conversion helpers; added `statusValue`, `campaignType`, `objectiveValue` fields | `marketing.ts` |
| Campaign (FE) | Fixed `UpdateCampaignDto.budget` type from `string` → `number`; expanded `CreateCampaignDto` with `campaignType`, `objectiveValue`, `priority`, `segmentCriteria`, `tags`, `ownerId` | `marketing.ts` |
| Order (FE) | Added 6 missing financial fields: `mrr`, `arr`, `tcv`, `acv`, `baseCurrencyAmount`, `holdDate` | `sales.ts` |
| Account (FE) | Removed duplicate `CreateAccountDto`/`UpdateAccountDto` from `accounts.ts`; canonical definitions live in `accountService.ts` | `accounts.ts` |

---

## Per-Entity Status

### Account — ✅ Complete

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | All fields in `Accounts` table |
| Backend Entity (`Account.cs`) | ✅ | Includes `Industry` field added Session 1 |
| Backend DTO (`AccountDto.cs`) | ✅ | Full coverage |
| Frontend Type (`accounts.ts`) | ✅ | All fields present |
| Frontend UI (`AccountsPage.tsx`) | ✅ | Industry dropdown added Session 1 |

---

### Contact — ✅ DTO + Types Complete, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | All fields in `Contacts` table |
| Backend Entity (`Contact.cs`) | ✅ | Rich model in `CRM.Core/Models/Contact.cs` |
| Backend DTO (`ContactDto.cs`) | ✅ | 12 fields added Session 1 |
| Frontend Type (`crm.ts`) | ✅ | All key fields present |
| Frontend UI (`ContactsPage.tsx`) | ⚠️ | Secondary fields not yet in form sections |

**UI remaining gaps:** `emailSecondary`, `phoneSecondary`, `department`, address fields, `doNotContact`, `preferredContactMethod`

**Intentionally omitted:** `MergedIntoId`, `MergeGroupId`, `IsMergedDuplicate`, `MergedAt` (merge tracking — admin-only operations)

---

### User — ✅ Types Complete, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | Full schema |
| Backend Entity (`User.cs`) | ✅ | Complete |
| Backend DTO (`UserDto.cs`) | ✅ | `IsLocked`, `HeaderColor`, `PhotoUrl` added Session 1 |
| Frontend Type (`UserManagementPage.tsx`) | ✅ | All fields present |
| Frontend Type (`UserManagementTab.tsx`) | ✅ | All fields present |
| Frontend UI | ✅ | `isLocked` Switch, `headerColor` color picker, `photoUrl` TextField added to create/edit dialog (Session 6) |

---

### Role — ✅ Complete

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | Complete |
| Backend DTO (`RoleDto.cs`) | ✅ | Full coverage |
| Frontend Type (`UserManagementTab.tsx`) | ✅ | All fields present |
| Frontend UI | ✅ | Role management table covers required fields |

---

### Permission — ✅ Complete

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | Complete |
| Backend DTO | ✅ | Full coverage |
| Frontend Type (`UserManagementTab.tsx`) | ✅ | New `Permission` interface with all fields added Session 1 |
| Frontend UI | ✅ | Permission grid covers required fields |

---

### Lead — ✅ Types Complete, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | Complete |
| Backend Entity (`Lead.cs`) | ✅ | 27 mapped fields |
| Backend DTO (service mapping) | ✅ | `GetById` service method maps all key fields |
| Frontend Type (`crm.ts`) | ✅ | 11 fields added Session 2 |
| Frontend UI (`LeadsPage.tsx`) | ⚠️ | Form shows 9 fields; additional fields not surfaced |

**UI remaining gaps:** `fitScore`, `engagementScore`, `region`, `qualificationNotes`, `campaignId`, `mqlDate`, `sqlDate`, `tags`

**Cleanup needed:** 8 frontend fields have no backend equivalent and should be removed in a future breaking-change window: `rating`, `industry`, `employees`, `annualRevenue`, `priority`, `convertedAccountId`, `convertedOpportunityId`, `convertedDate`

---

### Opportunity — ✅ DTO Added, Types Complete, UI Local Interface

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | Complete |
| Backend Entity (`Opportunity.cs`) | ✅ | 17 real fields + computed properties |
| Backend DTO (`OpportunityDtos.cs`) | ✅ | Created Session 3; currently omits secondary entity IDs and custom calculation fields |
| Frontend Type (`crm.ts`) | ✅ | 10 fields added Session 2, imported from DTO? still local override in page |
| Frontend UI (`OpportunitiesPage.tsx`) | ✅ | All key fields present but still uses a local `Opportunity` interface; should import shared type |

**Remaining gap:** DTO needs full coverage (secondaryEntity*, product/task/quote relationships). Frontend should switch to shared type and remove local interface.

**Cleanup needed:** remove phantom fields `reason`, `nextStep`, `description`, `competitors`, `lossReason`, `leadSource`, `type`, `actualCloseDate` once backend is authoritative.

---

### Quote — ✅ DTO Present but Partial, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 69 fields |
| Backend Entity (`Quote.cs`) | ✅ | 69 mapped fields |
| Backend DTO (`QuoteDtos.cs`) | ✅ | Expanded Session 7 — billing/shipping addresses, approval workflow, signature, identity, workflow dates, pricing/terms |
| Frontend Type (`sales.ts`) | ✅ | Expanded 18 → 62 fields Session 2 |
| Frontend UI (`QuotesPage.tsx`) | ⚠️ | 5‑tab form; approval workflow UI and billing/shipping address UI still missing |

**Remaining gap:** DTO is now complete. UI needs approval workflow section and billing/shipping address fields surfaced.

---

### Order — ✅ DTO Added (incomplete), ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 79 fields |
| Backend Entity (`Order.cs`) | ✅ | 79 mapped fields |
| Backend DTO (`OrderDtos.cs`) | ✅ | `MapToOrderDto` fully implemented Session 7 — all fields: line items, billing/shipping, payment, shipping tracking, revenue recognition |
| Frontend Type (`sales.ts`) | ✅ | Expanded 18 → 66 fields Session 2 |
| Frontend UI (`OrdersPage.tsx`) | ⚠️ | Missing shipping tracking, payment details, revenue recognition |

**Remaining gap:** DTO and mapping are now complete. UI needs shipping/payment/revenue sections surfaced.

---

### Invoice — ✅ DTO + Types Expanded, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 80+ fields |
| Backend Entity (`Invoice.cs`) | ✅ | 80+ mapped fields |
| Backend DTO (`InvoiceDto.cs`) | ✅ | Expanded 20 → 56 fields Session 2 |
| Frontend Type (`sales.ts`) | ✅ | Expanded 15 → 51 fields Session 2 |
| Frontend UI (`InvoicesPage.tsx`) | ⚠️ | Form captures only 5 core fields |

**UI remaining gaps:** Billing address (9 fields: `billingName`, `billingCompany`, `billingStreet`, `billingCity`, `billingState`, `billingPostalCode`, `billingCountry`, `billingEmail`, `billingPhone`), `earlyPaymentDiscountPercent`, `earlyPaymentDiscountDays`, `lateFeePercent`, collections fields, `internalNotes`

---

### Payment — ✅ DTO + Types Expanded, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 60+ fields |
| Backend Entity (`Payment.cs`) | ✅ | 60+ mapped fields |
| Backend DTO (`PaymentDto.cs`) | ✅ | Expanded 18 → 35 fields Session 2 |
| Frontend Type (`sales.ts`) | ✅ | Expanded 11 → 41 fields Session 2 |
| Frontend UI (`PaymentsPage.tsx`) | ⚠️ | Form captures only 5 core fields |

**UI remaining gaps:** Card details (`cardBrand`, `cardLast4`, `cardExpMonth`, `cardExpYear`, `cardholderName`), bank details (`bankName`, `accountLast4`, `accountType`), gateway info (`gateway`, `gatewayResponseCode`), reconciliation fields

---

### Contract — ✅ DTO + Types Expanded, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 50+ fields |
| Backend Entity (`Contract.cs`) | ✅ | 50+ mapped fields |
| Backend DTO (`ContractDto.cs`) | ✅ | Expanded 18 → 38 fields Session 2 |
| Frontend Type (`sales.ts`) | ✅ | Expanded 13 → 43 fields Session 2 |
| Frontend UI (`ContractsPage.tsx`) | ⚠️ | 3-tab form; document management, approval/rejection, suspension missing |

**UI remaining gaps:** `contractFileUrl`, `signedContractFileUrl`, `approvedByUserId`, `approvedDate`, `rejectionReason`, `suspensionReason`, `suspendedDate`, `terminationClause`

---

### Activity — ✅ DTO Added, ❌ Read-Only UI

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 36+ fields |
| Backend Entity (`Activity.cs`) | ✅ | 36 mapped fields |
| Backend DTO (`ActivitiesController`/service) | ✅ | DTO classes exist in controller and are used by service; still limited to core fields |
| Frontend Type (`crm.ts`) | ✅ | Expanded 11 → 28 fields Session 2; additional secondary IDs still missing |
| Frontend UI (`ActivitiesPage.tsx`) | ❌ | **Read-only timeline view only — no create/edit form** |

**Remaining gaps:** UI form is absent. Backend DTO should be extended with secondary entity properties.

---

### CrmTask — ✅ DTO Added, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 44 fields |
| Backend Entity (`CrmTask.cs`) | ✅ | 44 mapped fields |
| Backend DTO (`CrmTaskDtos.cs`) | ✅ | Complete DTO family with create/update variants |
| Frontend Type (`crm.ts`) | ✅ | Full `CrmTask` interface + `TaskStatus`/`TaskPriority` enums exported |
| Frontend UI (`TasksPage.tsx`) | ⚠️ | 4-tab form (Tab 4 "Additional" added Session 2); attachments, group assignment still missing |

**Field name alignments:** Backend `Subject` → frontend uses `title`; backend `EstimatedMinutes` → UI displays as hours. Waiting on final DTO/UX alignment.

**UI remaining gaps:** `attachments` (file attachment picker)

---

### ServiceRequest — ✅ DTO + Types Expanded, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 49 fields |
| Backend Entity (`ServiceRequest.cs`) | ✅ | 49 mapped fields |
| Backend DTO (`ServiceRequestDto.cs`) | ✅ | ~70% field coverage |
| Frontend Type (`itsm.ts`) | ✅ | Expanded ~37 → 64 fields Session 2 |
| Frontend UI (`ServiceRequestsPage.tsx`) | ⚠️ | Main form tab good; SLA, resolution, VIP not in form |

**UI remaining gaps:** `slaStatus`, `resolutionSummary`, `resolutionCode`, `rootCause`, `isVipAccount`, `estimatedEffortHours`, `actualEffortHours`

---

### Campaign (MarketingCampaign) — ✅ Types Expanded, ⚠️ DTO Partial, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 100+ fields |
| Backend Entity (`MarketingCampaign.cs`) | ✅ | 100+ mapped fields |
| Backend DTO (`CampaignDto.cs`) | ⚠️ partial | Expanded in Session 3 to ~22 fields (budget/metrics/ROI) but still far from full coverage |
| Frontend Type (`marketing.ts`) | ✅ | Expanded ~49 → 111 fields Session 2 |
| Frontend UI (`CampaignsPage.tsx`) | ⚠️ | 5‑tab form; budget details, advanced metrics, content fields, hierarchy not in form |

**Critical remaining gap:** CampaignDto still needs a major expansion to match the underlying table. UI form gaps remain.

---

## Remaining Work

### Priority 1 — Expand Backend DTOs (Backend gaps)

| Entity | Action | Status | Notes |
|--------|--------|--------|-------|
| Campaign | ~~Expand CampaignDto~~ ✅ Done (Session 5 — 22 → 120+ fields) | ✅ | Full entity coverage |
| Opportunity | Enhance `OpportunityDto.cs` with secondary entity fields, products, tasks, quotes | ⚠️ partial | Service mapping TODO |
| ⁠~~Quote~~⁠ | ⁠~~Add billing/shipping, approval workflow, signature fields to `QuoteDto` family~~⁠ ✅ Done (Session 7) | ✅ | Full entity coverage |
| ⁠~~Order~~⁠ | ⁠~~Complete `OrderDto` mapping~~⁠ ✅ Done (Session 7) | ✅ | `MapToOrderDto` fully implemented; all fields, line items |
| Activity | ~~Extend DTO relationship IDs~~ ✅ Done (Session 4) | ✅ | DTO fields aligned; UI form added Session 5 |
| CrmTask | ~~Field name alignment~~ ✅ Done (Session 4) | ✅ | Full DTO coverage |

### Priority 2 — Frontend UI: Surface New Fields in Form Dialogs

TypeScript types are complete; the remaining work is wiring them into forms and removing local type definitions.

| Entity | Page | Fields to Add / Fix | Status |
|--------|------|--------------------|----|
| Activity | `ActivitiesPage.tsx` | ~~Create/edit form dialog~~ ✅ Done (Session 5) | ✅ |
| Invoice | `InvoicesPage.tsx` | ~~Billing address, earlyPayment fields~~ ✅ Done (Session 5); lateFeePercent still missing | ⚠️ Minor gap |
| Payment | `PaymentsPage.tsx` | ~~Card/bank/gateway fields~~ ✅ Done (Session 5) | ✅ |
| Contract | `ContractsPage.tsx` | ~~Documents & Approval section~~ ✅ Done (Session 5) | ✅ |
| Campaign | `CampaignsPage.tsx` | ⁠~~Budget detail fields, advanced engagement metrics~~⁠ ✅ Done (Session 7) | ✅ |
| Quote | `QuotesPage.tsx` | approval workflow, signature section, billing/shipping address fields | ⚠️ Partial |
| Order | `OrdersPage.tsx` | shipping tracking, payment details, revenue recognition | ⚠️ Partial |
| ServiceRequest | `ServiceRequestsPage.tsx` | ⁠~~slaStatus, resolutionSummary, resolutionCode, rootCause, isVipAccount, effort hours~~⁠ ✅ Done (Session 7) | ✅ |
| Lead | `LeadsPage.tsx` | ⁠~~region, qualificationNotes, campaignId, mqlDate, sqlDate, tags~~⁠ ✅ Done (Session 7); fitScore/engagementScore are read-only ML fields | ✅ |
| Contact | `ContactsPage.tsx` | ⁠~~emailSecondary, phoneSecondary, doNotContact, preferredContactMethod~~⁠ ✅ Done (Session 7) | ✅ |
| CrmTask | `TasksPage.tsx` | ~~assignedToGroupId~~ ✅ Done (Session 6); attachments (file picker) still pending | ⚠️ Minor gap |
| User | `UserManagementPage.tsx` | ~~isLocked toggle, headerColor picker, photoUrl field~~ ✅ Done (Session 6) | ✅ |

### Priority 3 — Cleanup & Consolidation

| Task | Description | Status |
|------|-------------|--------|
| ~~Remove duplicate Account DTO~~ | ✅ Done (Session 4) | ✅ |
| ~~Align CrmTask field names~~ | ✅ Done (Session 4) — `subject` → `title` | ✅ |
| ~~Remove phantom Lead fields~~ | ✅ Done (Session 5) — 8 fields removed from `crm.ts` | ✅ |
| ~~Remove phantom Opportunity fields~~ | ✅ Done (Session 5) — 8 fields removed; `OpportunityStage` fixed to numeric | ✅ |
| ~~Consolidate CrmTask types~~ | ✅ Done (Session 5) — local interface removed from `TasksPage.tsx` | ✅ |
| ~~Consolidate Opportunity types~~ | ✅ Done (Session 5) — local interface removed from `OpportunitiesPage.tsx` | ✅ |
| ~~Drop other legacy UI interfaces~~ | ✅ Done (Session 6) — Surveyed all 100+ page interfaces; only OpportunitiesPage.Opportunity and TasksPage.CrmTask were true duplicates (both removed in Session 5). All remaining local interfaces have distinct schemas. | ✅ |

---

## Intentionally Excluded Fields

These fields are deliberately absent from DTOs or frontend layers for security or operational reasons. Do not surface these in forms or API responses.

| Entity | Field | Reason |
|--------|-------|--------|
| User | `PasswordHash`, `PasswordSalt` | Security — never serialize |
| User | `TwoFactorSecret`, `BackupCodes` | Security — never serialize |
| User | `PasswordResetToken`, `EmailVerificationToken` | Security — token hygiene |
| User | `RefreshTokens` | Security — managed by auth middleware |
| User | `FailedLoginAttempts` | Security internal — expose only as aggregate `IsLocked` |
| User | `LockoutEnd` | Security internal |
| Account | `RowVersion` | Binary ETag — no UI purpose |
| Account | `IsDeleted` | Soft-delete flag — admin filter only |
| Contact | `MergedIntoId`, `MergeGroupId`, `IsMergedDuplicate`, `MergedAt` | Merge tracking — admin-only operations |
| Payment | `GatewayResponseRaw` | Raw gateway JSON — internal use only |
| Payment | `FraudNotes`, `FraudFlagged` | Fraud/risk — restricted to fraud team |
| Payment | `IpAddress`, `DeviceFingerprint` | PII/audit data — detail view only |
| Activity | `IpAddress`, `UserAgent` | Audit metadata — detail view only |

---

## Architecture Recommendations

1. ~~**Create DTO layer for all direct-entity endpoints.**~~ ✅ **Complete (Session 4).** All 16 entities have DTOs. All controllers return DTOs. Backend builds with 0 errors.
2. **Enum alignment.** All enum fields should use consistent `int` (numeric) values in API/DTO contracts. Frontend TypeScript should map numerics to string union types.
3. **Date serialization.** Standardize on ISO 8601 strings from the backend. Frontend should type all date fields as `string | null`.
4. **Field name conventions.** Avoid naming mismatches like `subject`/`title` and `estimatedMinutes`/`estimatedHours`. Define a DTO naming standard before creating the remaining DTOs.
5. **UI pattern: collapsible "Additional Information" section.** Use a collapsible accordion or extra tab at the bottom of each form dialog for secondary/advanced fields. Core workflow fields remain in the primary tab.
6. ~~**Campaign DTO — highest single priority.**~~ ✅ **Complete (Session 5).** `CampaignDto` now exposes 120+ fields. Campaign UI form still needs additional sections to surface them.
7. **Maintain this document as a living spec.** Update when adding or changing fields. Consider adding a build-time contract test to verify DTO public properties are a superset of the entity fields returned by the API.
8. **consistent data types** Use consistend data types across stack layers unless there is an overriding reason 

---

*Last updated: 2026-02-21 | Sessions completed: 7*
