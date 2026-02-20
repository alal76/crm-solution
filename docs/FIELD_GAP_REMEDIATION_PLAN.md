# CRM Solution Field Gap Remediation Plan

**Date:** 2026-02-20
**Last Updated:** 2026-02-20 (Session 2 — Full Analysis Complete)
**Prepared by:** Claude Sonnet 4.6

---

## Overview

This document tracks all field-level gaps and mismatches across the CRM solution, covering every major entity and module. It is updated continuously as remediations are applied.

**Scope:** Entity → DTO → Frontend Type → Frontend UI (4-layer coverage)
**Goal:** 100% field visibility across all non-security fields. Security-sensitive fields (passwords, tokens, secrets) are intentionally excluded from DTO/Frontend layers.

---

## Completed Remediations Log

### Session 1 — 2026-02-20

#### Account Entity
| Field | Change | Files Modified |
|-------|--------|----------------|
| `Industry` | Added to backend entity, DTO, and EF migration | `Account.cs`, `AccountDto.cs`, `20260219234030_AddIndustryToAccount.cs` |
| `IsDeleted` | Exposed in frontend type | `accounts.ts` |

**Account Status: ✅ All fields covered. Intentionally omitted: `RowVersion` (binary, no UI purpose).**

---

### Session 2 — 2026-02-20

#### User Entity
| Field | Layer Fixed | File Modified | Notes |
|-------|-------------|---------------|-------|
| `IsLocked` | Backend DTO | `UserDto.cs` | Added `public bool IsLocked { get; set; }` |
| `IsLocked` | Frontend Type (UserManagementPage) | `UserManagementPage.tsx` | Added `isLocked: boolean` to User interface |
| `IsLocked` | Frontend Type (UserManagementTab) | `UserManagementTab.tsx` | Added `isLocked?: boolean` to User interface |
| `HeaderColor` | Frontend Type | `UserManagementPage.tsx`, `UserManagementTab.tsx` | Added `headerColor?: string` |
| `PhotoUrl` | Frontend Type | `UserManagementPage.tsx`, `UserManagementTab.tsx` | Added `photoUrl?: string` |

**User intentionally omitted from DTO/Frontend:** `PasswordHash`, `TwoFactorSecret`, `BackupCodes`, `PasswordResetToken`, `PasswordResetTokenExpiry`, `EmailVerificationToken`, `RefreshTokens` (security), `FailedLoginAttempts`, `LockoutEnd` (security internal).

#### Role Entity
| Field | Layer Fixed | File Modified | Notes |
|-------|-------------|---------------|-------|
| `IsActive` | Backend DTO | `RBACAndAdminDtos.cs` | Added `public bool IsActive { get; set; }` to `RoleDto` |
| `IsSystemDefined` + `IsActive` | Frontend Type | `UserManagementTab.tsx` | New `Role` interface with all fields |

#### Permission Entity
| Field | Layer Fixed | File Modified | Notes |
|-------|-------------|---------------|-------|
| `IsActive` | Backend DTO | `RBACAndAdminDtos.cs` | Added `public bool IsActive { get; set; }` to `PermissionDto` |
| `IsSystemDefined` + `IsActive` | Frontend Type | `UserManagementTab.tsx` | New `Permission` interface with all fields |

#### Contact Entity
| Field | Layer Fixed | File Modified | Notes |
|-------|-------------|---------------|-------|
| `contactType` | Frontend Type | `crm.ts` | Added full string union matching `ContactType` enum |
| `status` | Frontend Type | `crm.ts` | Expanded from 2 to 5 values |
| `leadStatus` | Frontend Type | `crm.ts` | Added for Lead-type contacts |
| `emailPrimary`, `phonePrimary` | Frontend Type | `crm.ts` | Added as DTO-name aliases |
| `emailSecondary`, `phoneSecondary` | Frontend Type | `crm.ts` | Added to match backend DTO |
| `Salutation`, `Suffix`, `Nickname`, `Gender` | Backend DTO | `ContactDto.cs` | Added all personal detail fields |
| `PhoneMobile`, `PhoneFax` | Backend DTO | `ContactDto.cs` | Added additional phone fields |
| `Website`, `LinkedInUrl`, `TwitterHandle` | Backend DTO | `ContactDto.cs` | Added online presence fields |
| `DoNotContact`, `PreferredContactMethod` | Backend DTO | `ContactDto.cs` | Added communication preferences |
| `LeadStatus` | Backend DTO | `ContactDto.cs` | Added lead status field |

#### Lead Entity
| Field | Layer Fixed | File Modified | Notes |
|-------|-------------|---------------|-------|
| `fitScore`, `engagementScore` | Frontend Type | `crm.ts` | Added ML scoring fields |
| `qualificationNotes` | Frontend Type | `crm.ts` | Added SDR handoff notes |
| `region`, `campaignId`, `accountId`, `contactId` | Frontend Type | `crm.ts` | Added relationship/territory fields |
| `mqlDate`, `sqlDate`, `lastActivityDate` | Frontend Type | `crm.ts` | Added funnel date tracking |
| `tags` | Frontend Type | `crm.ts` | Added classification field |
| `region`, `campaignId`, `qualificationNotes` | Frontend CreateDto | `crm.ts` | Added to `CreateLeadDto` |
| `fitScore`, `engagementScore`, `qualificationNotes`, `region`, `campaignId` | Frontend UpdateDto | `crm.ts` | Added to `UpdateLeadDto` |

#### Opportunity Entity
| Field | Layer Fixed | File Modified | Notes |
|-------|-------------|---------------|-------|
| `currency`, `pricingModel`, `termLengthMonths` | Frontend Type | `crm.ts` | Added core deal fields |
| `solutionNotes`, `qualificationReason`, `qualificationNotes` | Frontend Type | `crm.ts` | Added qualification fields |
| `region`, `leadId` | Frontend Type | `crm.ts` | Added territory and source |
| `salesOwnerId`, `salesOwnerName` | Frontend Type | `crm.ts` | Added (replaces `ownerId` mismatch) |
| Added 8 missing fields | Frontend CreateDto/UpdateDto | `crm.ts` | Full DTO alignment |

#### Quote Entity (Frontend Types)
| Change | File Modified | Notes |
|--------|---------------|-------|
| +44 fields added | `sales.ts` | Approval workflow, signature, addresses, dates, docs, classification |

#### Order Entity (Frontend Types)
| Change | File Modified | Notes |
|--------|---------------|-------|
| +48 fields added | `sales.ts` | Identity, type/fulfillment, shipping details, payment, revenue recognition |

#### Invoice Entity
| Change | File Modified | Notes |
|--------|---------------|-------|
| +36 fields to DTO | `InvoiceDto.cs` | Billing address, early payment, late fees, collections, docs |
| +36 fields to Frontend Type | `sales.ts` | Full alignment with expanded DTO |

#### Payment Entity
| Change | File Modified | Notes |
|--------|---------------|-------|
| +17 fields to DTO | `PaymentDto.cs` | Card details, bank details, gateway, reconciliation |
| +30 fields to Frontend Type | `sales.ts` | Full card/bank/gateway/reconciliation coverage |

#### Contract Entity
| Change | File Modified | Notes |
|--------|---------------|-------|
| +20 fields to DTO | `ContractDto.cs` | Renewal tracking, documents, approval, suspension |
| +30 fields to Frontend Type | `sales.ts` | Full lifecycle coverage |

#### Activity Entity
| Change | File Modified | Notes |
|--------|---------------|-------|
| +17 fields added | `crm.ts` | title, userId, userName, entityName, accountId, contactId, opportunityId, campaignId, isSystem, isPrivate, isImportant, tags, category, source, oldValue, newValue, fieldsChanged |

#### CrmTask Entity (new type)
| Change | File Modified | Notes |
|--------|---------------|-------|
| Added `TaskStatus` enum | `crm.ts` | 6 values: NotStarted, InProgress, Completed, Deferred, WaitingOnOthers, Cancelled |
| Added `TaskPriority` enum | `crm.ts` | 4 values: Low, Normal, High, Critical |
| Added `CrmTask` interface | `crm.ts` | 29 fields — full backend entity coverage |
| Added `CreateCrmTaskDto` | `crm.ts` | 13 fields |
| Added `UpdateCrmTaskDto` | `crm.ts` | 15 fields |

#### ServiceRequest Entity (Frontend Types)
| Change | File Modified | Notes |
|--------|---------------|-------|
| +27 fields added | `itsm.ts` | Created by, workflow, due dates, resolution fields, SLA status, VIP flag, effort hours |

#### Campaign Entity (Frontend Types)
| Change | File Modified | Notes |
|--------|---------------|-------|
| +62 fields added | `marketing.ts` | Budget, lead gen metrics, engagement, email metrics, UTM tracking, A/B testing, hierarchy, documentation |

---

## Current Field Coverage Status

### Account — ✅ Complete

All 43 fields fully covered across DB → Entity → DTO → Frontend Type → UI.

Intentionally hidden from UI: `IsDeleted` (admin filter only), `RowVersion` (ETag/concurrency, not user-visible).

---

### Contact — ✅ Types + DTO Complete, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | All fields in `Contacts` table |
| Backend Entity (`Contact.cs`) | ✅ | Rich model in `CRM.Core/Models/Contact.cs` |
| Backend DTO (`ContactDto.cs`) | ✅ | All key fields added in Session 2 |
| Frontend Type (`crm.ts`) | ✅ | All key fields present after Session 2 fix |
| Frontend UI (`ContactsPage.tsx`) | ⚠️ | Secondary fields not yet in form sections |

**Contact intentionally omitted:** `MergedIntoId`, `MergeGroupId`, `IsMergedDuplicate`, `MergedAt` (merge tracking — admin-only operations).

---

### User — ✅ Types Complete, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | Full schema |
| Backend Entity (`User.cs`) | ✅ | Complete |
| Backend DTO (`UserDto.cs`) | ✅ | `IsLocked`, `HeaderColor`, `PhotoUrl` all present after Session 2 |
| Frontend Type (`UserManagementPage.tsx`) | ✅ | All fields after Session 2 |
| Frontend Type (`UserManagementTab.tsx`) | ✅ | All fields after Session 2 |
| Frontend UI | ⚠️ | `IsLocked`, `HeaderColor`, `PhotoUrl` not yet in form controls |

---

### Role — ✅ Types Complete

All role fields present in DTO and frontend type after Session 2. UI badge for `IsSystemDefined` pending.

---

### Permission — ✅ Types Complete

All permission fields present in DTO and frontend type after Session 2.

---

### Lead — ✅ Frontend Types Complete, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | Complete |
| Backend Entity (`Lead.cs`) | ✅ | 27 mapped fields |
| Backend DTO (service response) | ✅ | GetById returns all fields |
| Frontend Type (`crm.ts`) | ✅ | Fixed in Session 2 — 9 fields added, obsolete fields kept for compatibility |
| Frontend UI (`LeadsPage.tsx`) | ⚠️ | Form shows 9 fields; fitScore, engagementScore, region, qualificationNotes not yet in form |

**Note:** 8 frontend fields (`rating`, `industry`, `employees`, `annualRevenue`, `priority`, `convertedAccountId`, `convertedOpportunityId`, `convertedDate`) have no backend equivalent but were kept to avoid breaking changes. These should be removed in a future cleanup pass.

---

### Opportunity — ✅ Frontend Types Complete, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | Complete |
| Backend Entity (`Opportunity.cs`) | ✅ | 17 real fields + computed |
| Backend DTO | ⚠️ | No dedicated DTO — entity returned directly |
| Frontend Type (`crm.ts`) | ✅ | Fixed in Session 2 |
| Frontend UI (`OpportunitiesPage.tsx`) | ✅ | Page has local override that's correct — all key fields in form |

**Note:** `OpportunitiesPage.tsx` has a local `Opportunity` interface override that is more accurate than the global `crm.ts` type. The global type has been updated to match.

**Remaining gap:** No dedicated backend DTO for Opportunity — entity is returned directly from API. Create `OpportunityDto.cs` as a future task.

---

### Quote — ✅ Frontend Types Expanded, ⚠️ No Backend DTO, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 69 fields |
| Backend Entity (`Quote.cs`) | ✅ | 69 mapped fields |
| Backend DTO | ❌ | **NONE** — entity returned directly |
| Frontend Type (`sales.ts`) | ✅ | Expanded from 18 → 62 fields in Session 2 |
| Frontend UI (`QuotesPage.tsx`) | ⚠️ | Form has 5 tabs but missing approval, signature, individual address components |

**Critical gap:** No `QuoteDtos.cs` — create as a priority task.

---

### Order — ✅ Frontend Types Expanded, ⚠️ No Backend DTO, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 79 fields |
| Backend Entity (`Order.cs`) | ✅ | 79 mapped fields |
| Backend DTO | ❌ | **NONE** — entity returned directly |
| Frontend Type (`sales.ts`) | ✅ | Expanded from 18 → 66 fields in Session 2 |
| Frontend UI (`OrdersPage.tsx`) | ⚠️ | Missing shipping tracking, payment details, revenue recognition |

**Critical gap:** No `OrderDtos.cs` — create as a priority task.

---

### Invoice — ✅ DTO + Types Expanded, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 80+ fields |
| Backend Entity (`Invoice.cs`) | ✅ | 80+ mapped fields |
| Backend DTO (`InvoiceDto.cs`) | ✅ | Expanded from 20 → 56 fields in Session 2 |
| Frontend Type (`sales.ts`) | ✅ | Expanded from 15 → 51 fields in Session 2 |
| Frontend UI (`InvoicesPage.tsx`) | ⚠️ | Form only captures 5 core fields; billing address, early payment, late fees not in form |

---

### Payment — ✅ DTO + Types Expanded, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 60+ fields |
| Backend Entity (`Payment.cs`) | ✅ | 60+ mapped fields |
| Backend DTO (`PaymentDto.cs`) | ✅ | Expanded from 18 → 35 fields in Session 2 |
| Frontend Type (`sales.ts`) | ✅ | Expanded from 11 → 41 fields in Session 2 |
| Frontend UI (`PaymentsPage.tsx`) | ⚠️ | Form only captures 5 core fields; card/bank/gateway details not in form |

---

### Contract — ✅ DTO + Types Expanded, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 50+ fields |
| Backend Entity (`Contract.cs`) | ✅ | 50+ mapped fields |
| Backend DTO (`ContractDto.cs`) | ✅ | Expanded from 18 → 38 fields in Session 2 |
| Frontend Type (`sales.ts`) | ✅ | Expanded from 13 → 43 fields in Session 2 |
| Frontend UI (`ContractsPage.tsx`) | ⚠️ | 3-tab form; missing document management, approval/rejection, suspension fields |

---

### Activity — ✅ Frontend Types Expanded, ⚠️ No Backend DTO, ⚠️ Read-only UI

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 36+ fields |
| Backend Entity (`Activity.cs`) | ✅ | 36 mapped fields |
| Backend DTO | ❌ | **NONE** — entity returned directly |
| Frontend Type (`crm.ts`) | ✅ | Expanded from 11 → 28 fields in Session 2 |
| Frontend UI (`ActivitiesPage.tsx`) | ❌ | **Read-only timeline view only — no create/edit form** |

**Gaps:** No backend DTO. No create/edit form in UI (activities are auto-generated by system or need a dedicated create flow).

---

### CrmTask — ✅ Frontend Types Added, ⚠️ No Backend DTO, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 44 fields |
| Backend Entity (`CrmTask.cs`) | ✅ | 44 mapped fields |
| Backend DTO | ❌ | **NONE** — entity returned directly |
| Frontend Type (`crm.ts`) | ✅ | **NEW** — full `CrmTask` interface + enums + DTOs added in Session 2 |
| Frontend UI (`TasksPage.tsx`) | ⚠️ | 3-tab form; missing recurrence, category, attachments, group assignment |

**Note:** Previously, `CrmTask` was only defined locally in `TasksPage.tsx`. Now exported from `crm.ts`. Field naming alignment: backend uses `Subject`, frontend uses `title`. Backend stores time in minutes, frontend converts to hours.

---

### ServiceRequest — ✅ Frontend Types Expanded, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 49 fields |
| Backend Entity (`ServiceRequest.cs`) | ✅ | 49 mapped fields |
| Backend DTO | ✅ | Good coverage (~70%) |
| Frontend Type (`itsm.ts`) | ✅ | Expanded from ~37 → 64 fields in Session 2 |
| Frontend UI (`ServiceRequestsPage.tsx`) | ⚠️ | Main form tab good; SLA info, resolution fields, VIP status not in form |

---

### Campaign (MarketingCampaign) — ✅ Frontend Types Expanded, ⚠️ DTO Incomplete, ⚠️ UI Partial

| Layer | Status | Notes |
|-------|--------|-------|
| DB Schema | ✅ | 100+ fields |
| Backend Entity (`MarketingCampaign.cs`) | ✅ | 100+ mapped fields |
| Backend DTO | ⚠️ | ~15% coverage — only ~15 fields in `CampaignDto` |
| Frontend Type (`marketing.ts`) | ✅ | Expanded from ~49 → 111 fields in Session 2 |
| Frontend UI (`CampaignsPage.tsx`) | ⚠️ | 5-tab form; budget details, advanced metrics, content fields, hierarchy not exposed |

**Critical gap:** `CampaignDto` is severely limited — needs major expansion to expose the 100+ entity fields.

---

## Extended Entity Analysis Results

### Lead Entity — ✅ Analysis Complete (Agent aacc725)

**Entity:** 27 mapped fields in `Lead.cs`
**Backend DTO coverage:** GetById returns all key fields via service mapping
**Frontend type before fix:** 8 wrong fields (no backend equivalent), 9 missing fields
**Frontend type after fix:** ✅ All backend fields present

**Analysis Findings:**
| Entity Field | In DTO | In TS Type (before) | In TS Type (after) | In UI Form |
|---|---|---|---|---|
| FirstName | ✓ | ✓ | ✓ | ✓ |
| LastName | ✓ | ✓ | ✓ | ✓ |
| Email | ✓ | ✓ | ✓ | ✓ |
| Phone | ✓ | ✓ | ✓ | ✓ |
| CompanyName | ✓ | ✓ | ✓ | ✓ |
| Title | ✓ | ✓ (jobTitle) | ✓ | ✓ |
| Status | ✓ | ✓ | ✓ | ✓ |
| Source | ✓ | ✓ | ✓ | ✓ |
| Score | ✓ | ✓ (leadScore) | ✓ | Table only |
| FitScore | ✓ | ❌ | ✅ **Fixed** | ❌ |
| EngagementScore | ✓ | ❌ | ✅ **Fixed** | ❌ |
| QualificationNotes | ✓ | ❌ | ✅ **Fixed** | ❌ |
| Region | ✓ | ❌ | ✅ **Fixed** | ❌ |
| Website | ✓ | ✓ | ✓ | ❌ |
| OwnerId | ✓ | ✓ | ✓ | ❌ |
| CampaignId | ✓ | ❌ | ✅ **Fixed** | ❌ |
| AccountId | ✓ | ❌ | ✅ **Fixed** | ❌ |
| ContactId | ✓ | ❌ | ✅ **Fixed** | ❌ |
| MqlDate | ✓ | ❌ | ✅ **Fixed** | ❌ |
| SqlDate | ✓ | ❌ | ✅ **Fixed** | ❌ |
| LastActivityDate | ✓ | ❌ | ✅ **Fixed** | ❌ |
| Tags | ✓ | ❌ | ✅ **Fixed** | ❌ |
| rating | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |
| industry | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |
| employees | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |
| annualRevenue | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |
| priority | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |
| convertedAccountId | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |
| convertedOpportunityId | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |
| convertedDate | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |

**UI remaining gaps:** FitScore, EngagementScore, Region, QualificationNotes not in LeadsPage form.

---

### Opportunity Entity — ✅ Analysis Complete (Agent a9a187e)

**Entity:** 17 real mapped fields + computed properties in `Opportunity.cs`
**Backend DTO:** None — entity returned directly from controller
**Frontend type before fix:** 8 non-existent fields, 8 missing fields
**Frontend type after fix:** ✅ All backend fields present

**Analysis Findings:**
| Entity Field | In DTO | In TS Type (before) | In TS Type (after) | In UI Form |
|---|---|---|---|---|
| Name | Direct | ✓ | ✓ | ✓ |
| Stage | Direct | ✓ | ✓ | ✓ |
| Amount | Direct | ✓ | ✓ | ✓ |
| Probability | Direct | ✓ | ✓ | ✓ |
| Currency | Direct | ❌ | ✅ **Fixed** | ✓ (page local) |
| ExpectedCloseDate | Direct | ✓ | ✓ | ✓ |
| PricingModel | Direct | ❌ | ✅ **Fixed** | ✓ (page local) |
| TermLengthMonths | Direct | ❌ | ✅ **Fixed** | ✓ (page local) |
| SolutionNotes | Direct | ❌ | ✅ **Fixed** | ✓ (page local) |
| QualificationReason | Direct | ❌ | ✅ **Fixed** | ✓ (page local) |
| QualificationNotes | Direct | ❌ | ✅ **Fixed** | ✓ (page local) |
| Region | Direct | ❌ | ✅ **Fixed** | ✓ (page local) |
| SalesOwnerId | Direct | ✓ (ownerId mismatch) | ✅ **Fixed** (salesOwnerId) | ✓ (page local) |
| PrimaryContactId | Direct | ✓ | ✓ | ✓ |
| AccountId | Direct | ✓ | ✓ | ✓ |
| LeadId | Direct | ❌ | ✅ **Fixed** | ❌ |
| reason, nextStep, description | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |
| competitors, lossReason, leadSource, type | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |
| actualCloseDate | ❌ | ✓ (no backend) | ✓ (kept) | ❌ |

**Key finding:** `OpportunitiesPage.tsx` has a local interface override that is correct — the UI form already surfaces all backend fields. The global `crm.ts` type was the only gap.

---

### Quote Entity — ✅ Analysis Complete (Agent a32d301)

**Entity:** 69 mapped fields in `Quote.cs`
**Backend DTO:** ❌ NONE — entity returned directly (CRITICAL ARCHITECTURAL GAP)
**Frontend type coverage before:** 18/69 fields (26%)
**Frontend type coverage after:** 62/69 fields (90%)

**Gap Summary:**
- Approval workflow: `requiresApproval`, `isApproved`, `approvalDate`, `approvalNotes`, `submittedForApprovalDate`, `approvedByUserId`
- Signature: `isSigned`, `signedDate`, `signedBy`, `signatureUrl`
- Dates: `sentDate`, `viewedDate`, `acceptedDate`, `rejectedDate`, `actualDeliveryDate`, `expectedDeliveryDate`
- Address components: `BillingName/Address/City/State/ZipCode/Country`, `ShippingName/Address/City/State/ZipCode/Country`
- Contact details: `contactEmail`, `contactPhone`
- Relationships: `assignedToUserId`, `createdByUserId`, `parentQuoteId`, `relationshipManagerId`
- Terms: `deliveryTerms`, `warranty`, `warrantyMonths`, `warrantyEndDate`
- Service dates: `serviceStartDate`, `serviceEndDate`
- Documentation: `internalNotes`, `attachments`, `quotePdfUrl`
- Classification: `tags`, `category`, `customFields`

**52 fields added to `sales.ts` Quote interface in Session 2.**

---

### Order Entity — ✅ Analysis Complete (Agent a32d301)

**Entity:** 79 mapped fields in `Order.cs`
**Backend DTO:** ❌ NONE — entity returned directly (CRITICAL ARCHITECTURAL GAP)
**Frontend type coverage before:** 18/79 fields (23%)
**Frontend type coverage after:** 66/79 fields (84%)

**Gap Summary:**
- Identity: `orderNumber`, `externalOrderId`, `customerPONumber`, `referenceNumber`
- Type/Fulfillment: `orderType`, `fulfillmentMethod`, `priority`
- Revenue recognition: `mrr`, `arr`, `tcv`, `acv`, `oneTimeRevenue`, `recurringRevenue`
- Full billing address (9 fields), full shipping address (9 fields)
- Shipping details: `shippingMethod`, `trackingNumber`, `trackingUrl`, `shippingWeight`, `packageCount`
- Payment: `paymentMethod`, `amountInvoiced`, `amountPaid`, `isPaid`
- Hold/rejection: `holdReason`, `holdDate`, `rejectionReason`, `returnReason`
- Discount codes: `discountCode`, `couponCode`
- Notes: `internalNotes`, `specialInstructions`, `cancellationReason`

**48 fields added to `sales.ts` Order interface in Session 2.**

---

### Invoice Entity — ✅ Analysis Complete (Agent a7ddf05)

**Entity:** 80+ mapped fields in `Invoice.cs`
**Backend DTO before:** 20 fields (InvoiceDto.cs)
**Backend DTO after:** 56 fields (+36 fields added in Session 2)
**Frontend type before:** 15 fields
**Frontend type after:** 51 fields (+36 fields added in Session 2)

**Key fields added to DTO and Frontend Type:**
- Billing address: `billingName/Company/Street/City/State/PostalCode/Country/Email/Phone` (9 fields)
- Early payment: `earlyPaymentDiscountPercent`, `earlyPaymentDiscountDays`, `earlyPaymentDiscountAmount`
- Late fees: `lateFeePercent`, `lateFeeAmount`, `lateFeeTotal`
- Collections: `reminderCount`, `lastReminderDate`, `nextReminderDate`, `inCollections`, `collectionsDate`, `collectionsReference`
- Dates: `sentDate`, `viewedDate`, `voidedDate`
- Documentation: `internalNotes`, `footer`, `termsAndConditions`, `voidReason`, `disputeReason`, `pdfUrl`
- Relationships: `contactId`, `subscriptionId`, `originalInvoiceId`

**UI remaining gaps:** `InvoicesPage.tsx` form only captures 5 core fields. Billing address, early payment, late fees, collections fields need accordion sections.

---

### Payment Entity — ✅ Analysis Complete (Agent a7ddf05)

**Entity:** 60+ mapped fields in `Payment.cs`
**Backend DTO before:** 18 fields (PaymentDto.cs)
**Backend DTO after:** 35 fields (+17 fields added in Session 2)
**Frontend type before:** 11 fields
**Frontend type after:** 41 fields (+30 fields added in Session 2)

**Key fields added to DTO and Frontend Type:**
- Card details: `cardBrand`, `cardLast4`, `cardExpMonth`, `cardExpYear`, `cardholderName`
- Bank details: `bankName`, `accountLast4`, `accountType`
- Gateway: `gateway`, `gatewayResponseCode`, `gatewayResponseMessage`
- Reconciliation: `isReconciled`, `reconciledDate`, `bankReference`
- Dates: `processedDate`, `settledDate`, `depositDate`
- Notes: `internalNotes`, `failureReason`, `refundReason`
- Relationships: `orderId`, `subscriptionId`
- Amounts: `processingFee`, `netAmount`, `exchangeRate`

**UI remaining gaps:** `PaymentsPage.tsx` form captures only 5 fields. Card/bank/gateway details need accordion sections.

---

### Contract Entity — ✅ Analysis Complete (Agent a7ddf05)

**Entity:** 50+ mapped fields in `Contract.cs`
**Backend DTO before:** 18 fields (ContractDto.cs)
**Backend DTO after:** 38 fields (+20 fields added in Session 2)
**Frontend type before:** 13 fields
**Frontend type after:** 43 fields (+30 fields added in Session 2)

**Key fields added to DTO and Frontend Type:**
- Renewal tracking: `renewalNoticeSent`, `renewalNoticeSentDate`, `renewalInitiatedAt`, `renewalCompletedAt`, `renewalTermMonthsOverride`
- Documents: `contractFileUrl`, `contractFileName`, `signedContractFileUrl`, `signedContractFileName`
- Approval: `approvedByUserId`, `approvedByName`, `approvedDate`, `rejectionReason`
- Suspension: `suspensionReason`, `suspendedDate`
- Dates: `activatedDate`, `terminatedDate`
- Terms: `specialConditions`, `terminationClause`, `terminationReason`
- Relationships: `quoteId`

---

### Activity Entity — ✅ Analysis Complete (Agent ab63dfc)

**Entity:** 36 mapped fields in `Activity.cs`
**Backend DTO:** ❌ NONE — entity returned directly
**Frontend type before:** 11 fields (31% coverage)
**Frontend type after:** 28 fields (+17 fields added in Session 2)

**Key fields added to Frontend Type:**
- `title` (alias for Title), `userId`, `userName`
- `entityName` (denormalized display name)
- Relationships: `accountId`, `contactId`, `opportunityId`, `campaignId`
- Classification: `isSystem`, `isPrivate`, `isImportant`, `tags`, `category`
- Audit: `source`, `oldValue`, `newValue`, `fieldsChanged`

**Critical remaining gaps:**
- No backend DTO — entity exposed directly
- `ActivitiesPage.tsx` is **read-only timeline view only** — no create/edit form
- Missing in frontend type: `SecondaryEntityType/Id/Name`, `ProductId`, `TaskId`, `QuoteId`, `InteractionId`, `NoteId`, `IpAddress`, `UserAgent`, `CustomFields`

---

### CrmTask Entity — ✅ Analysis Complete (Agent ab63dfc)

**Entity:** 44 mapped fields in `CrmTask.cs`
**Backend DTO:** ❌ NONE — entity returned directly
**Frontend type before:** 15 local fields in `TasksPage.tsx` (not exported)
**Frontend type after:** Full `CrmTask` interface with enums exported from `crm.ts`

**Key fields added to Frontend Type (`crm.ts`):**
- `TaskStatus` enum (6 values), `TaskPriority` enum (4 values)
- `CrmTask` interface with 29 fields
- `CreateCrmTaskDto` (13 fields), `UpdateCrmTaskDto` (15 fields)

**Field name alignments required:**
- Backend: `Subject` → Frontend: `subject` (aliased as `title` in UI)
- Backend: `EstimatedMinutes` → Frontend: `estimatedMinutes` (UI converts to hours for display)

**UI remaining gaps:** Recurrence fields (`isRecurring`, `recurrencePattern`, `recurrenceEndDate`), `category`, `attachments`, `assignedToGroupId`, `contactId`, `campaignId` not in `TasksPage.tsx` form.

---

### ServiceRequest Entity — ✅ Analysis Complete (Agent a82d5b9)

**Entity:** 49 mapped fields in `ServiceRequest.cs`
**Backend DTO coverage:** ~70%
**Frontend type before:** 37 fields (~60%)
**Frontend type after:** 64 fields (~90%+)

**Key fields added to Frontend Type (`itsm.ts`):**
- `createdByUserId`, `createdByUserName`
- Workflow: `workflowId`, `workflowName`, `currentWorkflowStep`
- Dates: `dueDate`, `firstResponseDate`, `closedDate`, `resolvedDate`
- Resolution: `resolutionSummary`, `resolutionCode`, `rootCause`
- Product: `relatedProductId`, `relatedProductName`
- Feedback: `customerFeedback`, `satisfactionRating` (existing)
- Classification: `tags`, `internalNotes`, `escalationLevel`, `reopenCount`, `isVipAccount`
- Effort: `estimatedEffortHours`, `actualEffortHours`, `slaStatus`
- External: `externalReferenceId`, `sourceEmailAddress`, `isEscalated`

**UI remaining gaps:** SLA info, resolution fields (filled in by agent), VIP status flag not shown in create form.

---

### Campaign (MarketingCampaign) Entity — ✅ Analysis Complete (Agent a82d5b9)

**Entity:** 100+ mapped fields in `MarketingCampaign.cs`
**Backend DTO coverage:** ~15% (MOST SEVERE GAP)
**Frontend type before:** ~49 fields (~10%)
**Frontend type after:** ~111 fields

**Key fields added to Frontend Type (`marketing.ts`):**
- Budget details: `dailyBudget`, `monthlyBudget`, `expectedRevenue`, `costPerLead`, `costPerMql`, `costPerSql`, `costPerOpportunity`, `costPerAcquisition`, `currencyCode`
- Lead gen metrics: `mqLsGenerated`, `sqlsGenerated`, `salsGenerated`, `opportunitiesCreated`, `opportunitiesInfluenced`, `dealsWon`, `accountsAcquired`
- Engagement: `impressions`, `reach`, `frequency`, `clickThroughRate`, `landingPageVisits`, `formSubmissions`, `videoViews`, `videoCompletions`
- Email metrics: `emailsSent`, `emailsDelivered`, `deliveryRate`, `emailsOpened`, `openRate`, `unsubscribes`, `bounces`
- UTM: `utmSource`, `utmMedium`, `utmCampaign`, `utmContent`, `utmTerm`
- A/B testing: `isABTest`, `variantAName`, `variantBName`, `variantAConversions`, `variantBConversions`
- Assignment: `ownerId`, `assignedToUserId`, `department`, `approvedBy`
- Hierarchy: `parentCampaignId`, `programId`, `initiativeId`, `fiscalQuarter`, `fiscalYear`
- Classification: `category`, `subCategory`, `region`
- Documentation: `internalNotes`, `successCriteria`, `lessonsLearned`, `attachments`

**Critical remaining gap:** `CampaignDto` in backend only exposes ~15 fields. The `MarketingCampaign` entity has 100+ fields. This is the largest single backend DTO gap in the system.

---

## Remaining Work Summary

### Priority 1: Create Missing Backend DTOs (Architectural Gaps)

| Entity | Current State | Action Required | Files |
|--------|---------------|-----------------|-------|
| Quote | Entity returned directly | Create `QuoteDtos.cs` with `QuoteDto`, `CreateQuoteDto`, `UpdateQuoteDto` | New file |
| Order | Entity returned directly | Create `OrderDtos.cs` with `OrderDto`, `CreateOrderDto`, `UpdateOrderDto` | New file |
| Opportunity | Entity returned directly | Create `OpportunityDto.cs` | New file |
| Activity | Entity returned directly | Create `ActivityDto.cs` | New file |
| CrmTask | Entity returned directly | Create `CrmTaskDtos.cs` | New file |
| Campaign | DTO has ~15% coverage | Massively expand `CampaignDto.cs` | Existing file |

### Priority 2: Frontend UI — Add Accordion Sections for All Expanded Entities

The TypeScript types are now complete. The next step is wiring the new fields into the UI. Per user's request to *"show all except those deliberately hidden"*, each entity's form dialog should have an "Additional Information" accordion at the bottom.

| Entity | Page | Fields to Add to Accordion |
|--------|------|---------------------------|
| Lead | `LeadsPage.tsx` | fitScore, engagementScore, region, qualificationNotes, campaignId, mqlDate, sqlDate, tags |
| Contact | `ContactsPage.tsx` | emailSecondary, phoneSecondary, department, address fields, doNotContact, preferredContactMethod |
| User | `UserManagementPage/Tab.tsx` | isLocked toggle, headerColor picker, photoUrl field |
| Invoice | `InvoicesPage.tsx` | Billing address (9 fields), early payment discount, late fees, collections, internalNotes |
| Payment | `PaymentsPage.tsx` | Card details, bank details, gateway info, reconciliation fields |
| Contract | `ContractsPage.tsx` | Document URLs, approval fields, suspension reason, terminationClause |
| CrmTask | `TasksPage.tsx` | Recurrence fields, category, attachments, group assignment, contactId |
| Activity | `ActivitiesPage.tsx` | **Create a form dialog** (currently read-only) |
| ServiceRequest | `ServiceRequestsPage.tsx` | SLA status, resolution fields, VIP flag, effort hours |
| Campaign | `CampaignsPage.tsx` | Budget details, advanced metrics, content fields, hierarchy |

### Priority 3: Cleanup Pass

| Task | Description |
|------|-------------|
| Remove phantom Lead fields | `crm.ts` Lead interface has 8 fields (`rating`, `industry`, `employees`, etc.) with no backend equivalent — remove in future breaking-change window |
| Remove Opportunity phantom fields | `crm.ts` Opportunity has `reason`, `nextStep`, `description`, `competitors`, `lossReason`, `leadSource`, `type`, `actualCloseDate` with no backend equivalent |
| Align `CrmTask.subject` vs `title` | Backend uses `Subject`, frontend UI uses `title`. Standardize in backend DTO when created |
| Align `estimatedMinutes` conversion | Backend stores minutes, UI displays hours — document conversion or fix in DTO |

---

## Intentionally Excluded Fields (Do Not Fix)

These fields are deliberately absent from DTO or Frontend layers for security or architectural reasons:

| Entity | Field | Reason |
|--------|-------|--------|
| User | `PasswordHash` | Security — never serialize |
| User | `TwoFactorSecret` | Security — never serialize |
| User | `BackupCodes` | Security — never serialize |
| User | `PasswordResetToken` | Security — never serialize |
| User | `EmailVerificationToken` | Security — never serialize |
| User | `RefreshTokens` | Security — managed by auth layer |
| User | `FailedLoginAttempts` | Security internal — expose only as "IsLocked" |
| User | `LockoutEnd` | Security internal |
| Account | `RowVersion` | Binary ETag — no UI purpose |
| Account | `IsDeleted` | Soft-delete flag — shown only in admin filter |
| Contact | `MergedIntoId`, `MergeGroupId`, `IsMergedDuplicate` | Merge tracking — admin-only |
| Payment | `GatewayResponseRaw` | Raw gateway JSON — internal only |
| Payment | `FraudNotes`, `FraudFlagged` | Fraud/risk — show only to fraud team |
| Payment | `IpAddress`, `DeviceFingerprint` | PII/audit data — not user-visible |
| Activity | `IpAddress`, `UserAgent` | Audit metadata — read-only in detail view only |

---

## Architecture Recommendations

1. **Create DTO layer for all direct-entity endpoints**: Quote, Order, Opportunity, Activity, CrmTask all return raw entities. DTOs should be the contract layer, not entities.
2. **Enum alignment**: All enum fields should use `int` (numeric) values consistently in API/DTO contracts. Frontend should use TypeScript `const enum` or string union mapped from numeric values.
3. **Date serialization**: Standardize on ISO 8601 strings from backend. Frontend should handle `string | null` for all date fields.
4. **Field name conventions**: Avoid field name mismatches (subject/title, estimatedMinutes/estimatedHours). Define a DTO naming standard.
5. **UI tab strategy**: Forms should use a two-section layout: core fields in the main form, optional/secondary fields in a collapsible "Additional Information" accordion at the bottom.
6. **Campaign DTO expansion**: CampaignDto is the most critical single gap — expand from ~15 fields to cover the full 100+ entity fields.
7. **Field mapping matrix**: Maintain this document as a living spec — update when adding/changing fields.
8. **Automated alignment checks**: Consider adding a build-time test that verifies DTO public properties are a superset of the fields returned by the API (contract testing).

---

## Remediation Checklist

### Session 1
- [x] Account: `Industry` — Backend, DB, Frontend ✅
- [x] Account: `IsDeleted` — Frontend type ✅

### Session 2 — Backend DTOs
- [x] User: `IsLocked` — Backend DTO + Frontend types ✅
- [x] Role: `IsActive` — Backend DTO + Frontend type ✅
- [x] Permission: `IsActive` — Backend DTO + Frontend type ✅
- [x] Contact: `Salutation`, `Suffix`, `Nickname`, `Gender` — Backend DTO ✅
- [x] Contact: `PhoneMobile`, `PhoneFax` — Backend DTO ✅
- [x] Contact: `Website`, `LinkedInUrl`, `TwitterHandle` — Backend DTO ✅
- [x] Contact: `DoNotContact`, `PreferredContactMethod`, `LeadStatus` — Backend DTO ✅
- [x] Invoice: +36 fields — Backend DTO expanded ✅
- [x] Payment: +17 fields — Backend DTO expanded ✅
- [x] Contract: +20 fields — Backend DTO expanded ✅

### Session 2 — Frontend Types
- [x] User: `isLocked`, `headerColor`, `photoUrl` — Frontend types ✅
- [x] Role: `IsSystemDefined` + `isActive` — Frontend type ✅
- [x] Permission: `isSystemDefined` + `isActive` — Frontend type ✅
- [x] Contact: `contactType`, `status` expansion, `leadStatus`, `emailPrimary/Secondary`, `phonePrimary/Secondary` — `crm.ts` ✅
- [x] Lead: 9 missing fields added, `CreateLeadDto` + `UpdateLeadDto` expanded — `crm.ts` ✅
- [x] Opportunity: 9 missing fields added, `salesOwnerId` fix, DTOs expanded — `crm.ts` ✅
- [x] Activity: 17 fields added — `crm.ts` ✅
- [x] CrmTask: New type with enums, full interface, DTOs — `crm.ts` ✅
- [x] Quote: +44 fields — `sales.ts` ✅
- [x] Order: +48 fields — `sales.ts` ✅
- [x] Invoice: +36 fields — `sales.ts` ✅
- [x] Payment: +30 fields — `sales.ts` ✅
- [x] Contract: +30 fields — `sales.ts` ✅
- [x] ServiceRequest: +27 fields — `itsm.ts` ✅
- [x] Campaign: +62 fields — `marketing.ts` ✅

### Remaining Work
- [ ] Create backend `QuoteDtos.cs` with full DTO layer
- [ ] Create backend `OrderDtos.cs` with full DTO layer
- [ ] Create backend `OpportunityDto.cs`
- [ ] Create backend `ActivityDto.cs`
- [ ] Create backend `CrmTaskDtos.cs`
- [ ] Expand `CampaignDto.cs` from ~15 to 100+ fields
- [ ] Add accordion sections to `LeadsPage.tsx` for secondary fields
- [ ] Add accordion sections to `ContactsPage.tsx` for secondary fields
- [ ] Add accordion sections to `InvoicesPage.tsx` for billing address, early payment, late fees
- [ ] Add accordion sections to `PaymentsPage.tsx` for card/bank/gateway details
- [ ] Add accordion sections to `ContractsPage.tsx` for documents, approval, suspension
- [ ] Add accordion sections to `TasksPage.tsx` for recurrence, category, attachments
- [ ] Add accordion sections to `ServiceRequestsPage.tsx` for SLA, resolution, VIP
- [ ] Add accordion sections to `CampaignsPage.tsx` for budget details, metrics, hierarchy
- [ ] Add create/edit form dialog to `ActivitiesPage.tsx` (currently read-only)
- [ ] Add UI controls for `isLocked`, `headerColor`, `photoUrl` to User management forms
- [ ] Cleanup: Remove phantom fields from Lead and Opportunity in `crm.ts`

---

*Last updated: 2026-02-20 (Session 2 complete). Next: Priority 2 — Frontend UI accordion sections.*
