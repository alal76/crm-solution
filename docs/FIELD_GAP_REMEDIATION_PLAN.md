# CRM Solution Field Gap Remediation Plan

**Date:** 2026-02-20  (verified by code review)
**Status:** Session 3 Analysis Complete — UI Surface Work In Progress

---

## Executive Summary

Three remediation sessions have been completed against this CRM codebase. **Session 1** fixed the Account `Industry` field end-to-end (DB migration, DTO, frontend type, UI) and patched missing fields in the User, Role, Permission, and Contact DTOs. **Session 2** conducted deep analysis of all remaining entities using parallel sub-agents, expanded TypeScript types across `crm.ts`, `sales.ts`, `itsm.ts`, and `marketing.ts`, and expanded backend DTOs for Invoice, Payment, and Contract. **Session 3** filled the major architectural DTO gaps by adding `OpportunityDto`, `QuoteDto`, `OrderDto`, `ActivityDto`, and `CrmTaskDto` (with create/update variants) and made additional service layer improvements; CampaignDto was partially expanded. The frontend `TasksPage.tsx` was also updated with an "Additional" tab for recurrence and category fields.

**What's done:** Backend DTO coverage is complete for Account, Contact, User, Invoice, Payment, and Contract. All 16 entity TypeScript types have been expanded to match backend field sets. A full four-layer analysis (DB → DTO → TS type → UI form) has been documented for every entity.

**What remains:** The backend now has DTOs for every major entity, but several of them are still incomplete: Quote, Order and Campaign DTOs expose only a subset of their underlying table columns and ActivityDto / OpportunityDto still omit secondary relationship fields. The frontend types are fully synchronised and most UI forms have been improved, yet gaps persist across nearly every page (approval/signature fields on quotes, billing/shipping information on orders/invoices, SLA/resolution on service requests, etc.). CampaignDto is still a significant hole with only basic fields exposed.

---

## Coverage Dashboard

| Entity | DB / Entity | Backend DTO | FE Type | FE UI | Priority |
|--------|-------------|-------------|---------|-------|----------|
| Account | ✅ | ✅ | ✅ | ✅ | Done |
| Contact | ✅ | ✅ | ✅ | ⚠️ Secondary fields not surfaced | P2 |
| User | ✅ | ✅ | ✅ | ⚠️ isLocked / headerColor not surfaced | P2 |
| Role | ✅ | ✅ | ✅ | ✅ | Done |
| Permission | ✅ | ✅ | ✅ | ✅ | Done |
| Lead | ✅ | ✅ (via service) | ✅ | ⚠️ fitScore / region / tags not surfaced | P2 |
| Opportunity | ✅ | ⚠️ partial (missing secondary IDs) | ✅ | ✅ (page override, still uses local interface) | P1 DTO expansion |
| Quote | ✅ | ⚠️ partial (core fields only) | ✅ | ⚠️ Approval / signature missing | P1 DTO expansion |
| Order | ✅ | ⚠️ partial (line‐items/mapping TODO) | ✅ | ⚠️ Shipping / payment missing | P1 DTO expansion |
| Invoice | ✅ | ✅ (56 fields) | ✅ | ⚠️ Billing addr / late fees missing | P2 |
| Payment | ✅ | ✅ (35 fields) | ✅ | ⚠️ Card / bank / gateway missing | P2 |
| Contract | ✅ | ✅ (38 fields) | ✅ | ⚠️ Documents / approval missing | P2 |
| Activity | ✅ | ✅ (DTO defined in controller/service) | ✅ | ❌ Read-only timeline only | P1 UI form |
| CrmTask | ✅ | ✅ | ✅ | ⚠️ 4-tab form (recurrence added) | P1 UI enhancements |
| ServiceRequest | ✅ | ✅ (~70%) | ✅ | ⚠️ SLA / resolution missing | P2 |
| Campaign | ✅ | ⚠️ ~15% only | ✅ | ⚠️ Budget / metrics missing | P1 DTO |

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
| Frontend UI | ⚠️ | `IsLocked` toggle, `HeaderColor` picker, `PhotoUrl` field not in form |

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
| Backend DTO (`QuoteDtos.cs`) | ⚠️ partial | Core header/line‑item data only; many fields still omitted (billing/shipping, workflow, approval flags) |
| Frontend Type (`sales.ts`) | ✅ | Expanded 18 → 62 fields Session 2 |
| Frontend UI (`QuotesPage.tsx`) | ⚠️ | 5‑tab form; approval, signature, address components missing |

**Critical gap:** DTO exists but requires expansion to match entity. UI also needs missing field surfacing.

---

### Order — ✅ DTO Added (incomplete), ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 79 fields |
| Backend Entity (`Order.cs`) | ✅ | 79 mapped fields |
| Backend DTO (`OrderDtos.cs`) | ✅ | Created Session 3; mapping helper still marked TODO and line-items not fully mapped |
| Frontend Type (`sales.ts`) | ✅ | Expanded 18 → 66 fields Session 2 |
| Frontend UI (`OrdersPage.tsx`) | ⚠️ | Missing shipping tracking, payment details, revenue recognition |

**Critical gap:** DTO exists but requires completion of mapping and additional fields. UI gaps remain.

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

**UI remaining gaps:** `attachments`, `assignedToGroupId`

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

### Priority 1 — Expand & Harden Backend DTOs

All major entities now have DTO classes, but several require field-by-field coverage audits and service mappings.

| Entity | Action | Notes |
|--------|--------|-------|
| Opportunity | Enhance `OpportunityDto.cs` with secondary entity fields, products, tasks, quotes | Service mapping TODO
| Quote | Add billing/shipping, approval workflow, signature, custom fields to `QuoteDto` family | Controller mapping exists but limited
| Order | Complete `OrderDto` mapping (line items, relationships, workflow fields) | `OrderService.MapToOrderDto` marked TODO
| Activity | Extend DTO to include all relationship IDs (`SecondaryEntity*`, `ProductId`, `TaskId`, etc.) | UI not yet able to create activities
| CrmTask | Review DTO/field name alignment and add missing properties if any | `TasksController` has full trio
| Campaign | Continue expanding `CampaignDto` toward parity with entity (>100 fields) | Most critical backend gap

### Priority 2 — Frontend UI: Surface New Fields in Form Dialogs

TypeScript types are now complete; the remaining work is wiring them into forms and removing local type definitions.

| Entity | Page | Fields to Add / Fix |
|--------|------|--------------------|
| Contact | `ContactsPage.tsx` | emailSecondary, phoneSecondary, department, address, doNotContact, preferredContactMethod |
| User | `UserManagementPage/Tab.tsx` | isLocked toggle, headerColor picker, photoUrl field |
| Lead | `LeadsPage.tsx` | fitScore, engagementScore, region, qualificationNotes, campaignId, mqlDate, sqlDate, tags; remove phantom legacy fields |
| Opportunity | `OpportunitiesPage.tsx` | none (core fields present) but import shared type and drop local interface |
| Quote | `QuotesPage.tsx` | approval workflow, signature section, billing/shipping address fields |
| Order | `OrdersPage.tsx` | shipping tracking, payment details, revenue recognition, convert to shared type |
| Invoice | `InvoicesPage.tsx` | Billing address (9 fields), earlyPaymentDiscountPercent/Days, lateFeePercent, internalNotes |
| Payment | `PaymentsPage.tsx` | cardBrand, cardLast4, cardExpMonth/Year, cardholderName, bankName, accountLast4, gateway details |
| Contract | `ContractsPage.tsx` | contractFileUrl, signedContractFileUrl, approvedByUserId, approvedDate, rejectionReason, suspensionReason |
| Activity | `ActivitiesPage.tsx` | **Create a create/edit form dialog** — currently read-only timeline |
| CrmTask | `TasksPage.tsx` | attachments, assignedToGroupId |
| ServiceRequest | `ServiceRequestsPage.tsx` | slaStatus, resolutionSummary, resolutionCode, rootCause, isVipAccount, effort hours |
| Campaign | `CampaignsPage.tsx` | Budget detail fields, advanced engagement metrics, content fields, hierarchy fields |

### Priority 3 — Cleanup & Consolidation

| Task | Description |
|------|-------------|
| Remove phantom Lead fields | `crm.ts` Lead interface: remove `rating`, `industry`, `employees`, `annualRevenue`, `priority`, `convertedAccountId`, `convertedOpportunityId`, `convertedDate` — no backend equivalent |
| Remove phantom Opportunity fields | `crm.ts` Opportunity: remove `reason`, `nextStep`, `description`, `competitors`, `lossReason`, `leadSource`, `type`, `actualCloseDate` — no backend equivalent |
| Consolidate CrmTask types | Remove local `CrmTask` interface from `TasksPage.tsx` — import from `crm.ts` (now possible since DTO exists) |
| Consolidate Opportunity types | Remove local `Opportunity` interface from `OpportunitiesPage.tsx` — import from `crm.ts` |
| Align CrmTask field names | Standardize `Subject`/`title` and `EstimatedMinutes`/estimated hours; DTO already created but mapping may need rename adjustments |
| Drop legacy UI interfaces | Replace other pages that still define their own types with shared types from `crm.ts`, `sales.ts`, `itsm.ts`, `marketing.ts` |

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

1. **Create DTO layer for all direct-entity endpoints.** Quote, Order, Opportunity, Activity, and CrmTask currently return raw EF entities from the API. DTOs should be the contract layer.
2. **Enum alignment.** All enum fields should use consistent `int` (numeric) values in API/DTO contracts. Frontend TypeScript should map numerics to string union types.
3. **Date serialization.** Standardize on ISO 8601 strings from the backend. Frontend should type all date fields as `string | null`.
4. **Field name conventions.** Avoid naming mismatches like `subject`/`title` and `estimatedMinutes`/`estimatedHours`. Define a DTO naming standard before creating the remaining DTOs.
5. **UI pattern: collapsible "Additional Information" section.** Use a collapsible accordion or extra tab at the bottom of each form dialog for secondary/advanced fields. Core workflow fields remain in the primary tab.
6. **Campaign DTO — highest single priority.** `CampaignDto` exposes only ~15 of 100+ entity fields. This is the most critical unresolved backend gap in the system.
7. **Maintain this document as a living spec.** Update when adding or changing fields. Consider adding a build-time contract test to verify DTO public properties are a superset of the entity fields returned by the API.

---

*Last updated: 2026-02-20 | Sessions completed: 3*
