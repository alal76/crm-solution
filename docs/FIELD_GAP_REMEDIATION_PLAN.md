# CRM Field Gap Remediation Plan — v2.0

**Updated:** 2026-02-21
**Status:** Active — Post-Session 9 Comprehensive Re-Audit

## Context

Sessions 1–9 completed a full remediation pass across all 16 entities. All previously identified gaps were resolved and verified. This document replaces that history with a **fresh comprehensive re-audit** covering all 4 layers for all 16 entities.

> **Layers audited:** Backend Entity → Backend DTO → Frontend TypeScript Type → Frontend UI Form

---

## Coverage Dashboard

| Entity | DTO | FE Type | FE UI | Priority |
|--------|-----|---------|-------|----------|
| Account | ⚠️ Partial | ⚠️ Partial | ❌ Major gaps | P2 |
| Contact | ❌ Major gaps | ❌ Major gaps | ❌ Major gaps | P1 |
| User | ⚠️ Partial | ⚠️ Partial | ⚠️ Partial | P3 |
| Lead | ✅ Complete | ✅ Complete | ⚠️ Minor | P3 |
| Opportunity | ✅ Complete | ✅ Complete | ⚠️ Minor | P3 |
| Quote | ✅ Complete | ❌ Address gaps | ⚠️ Minor | P2 |
| Order | ✅ Complete | ❌ Address gaps | ⚠️ Minor | P2 |
| Invoice | ⚠️ Missing computed | ⚠️ Partial | ⚠️ Partial | P2 |
| Payment | ⚠️ Missing fraud fields | ⚠️ Partial | ⚠️ Partial | P2 |
| Contract | ⚠️ Missing computed | ⚠️ Partial | ⚠️ Partial | P3 |
| Activity | ✅ Complete | ✅ Complete | ⚠️ Minor | P3 |
| CrmTask | ❌ Major gaps | ✅ Complete | ✅ Complete | P1 |
| ServiceRequest | ❌ Expedite missing | ❌ Major gaps | ⚠️ Partial | P1 |
| Campaign | ✅ Complete | ❌ Severe gaps | ⚠️ Partial | P1 |

---

## P1 — DTO Deficiencies (Critical)

These fields cannot flow from backend to frontend at all until the DTO is updated.

---

### CrmTask — 15 Fields Missing from DTO

**File:** `CRM.Backend/src/CRM.Application/DTOs/CrmTaskDtos.cs`

> The FE TypeScript type already has all these fields. Once added to the DTO and controller, they will flow through without further FE type changes.

| Field | Entity Type | Note |
|-------|-------------|------|
| ReminderDate | DateTime? | FK reminder trigger |
| HasReminder | bool | Reminder enabled flag |
| PercentComplete | int (0-100) | Progress tracking |
| ActualMinutes | int? | Logged time |
| IsRecurring | bool | Recurrence flag |
| RecurrencePattern | string? (JSON) | Schedule JSON |
| RecurrenceEndDate | DateTime? | Recurrence stop date |
| ParentTaskId | int? | Subtask hierarchy |
| ContactId | int? | Contact FK |
| CampaignId | int? | Campaign FK |
| AssignedToGroupId | int? | Group assignment FK |
| Tags | string? | Comma-separated tags |
| Category | string? | Task category |
| Attachments | string? (JSON) | Attachment references |
| CustomFields | string? (JSON) | Custom field JSON |

Also: `Subject` in entity is mapped to `Title` in DTO — intentional rename, documented in P4.

---

### ServiceRequest — Expedite Feature Missing from ALL Layers

**Entity:** `CRM.Backend/src/CRM.Core/Entities/ServiceRequest.cs`
**DTO:** `CRM.Backend/src/CRM.Application/DTOs/ServiceRequestDtos.cs`
**FE Type:** `CRM.Frontend/src/types/itsm.ts`

> Full stack addition required: Entity → EF Migration → DTO → FE Type → FE UI.

| Field | Status |
|-------|--------|
| IsExpedited | Missing from Entity, DTO, FE Type, FE UI |
| ExpediteReason | Missing from Entity, DTO, FE Type, FE UI |
| ExpeditedByUserId | Missing from Entity, DTO, FE Type, FE UI |
| ExpeditedAt | Missing from Entity, DTO, FE Type, FE UI |

---

### Contact — 38 Fields Missing from DTO

**File:** `CRM.Backend/src/CRM.Application/DTOs/ContactDtos.cs`

**Mailing Address (5 fields):**

| Field | Type |
|-------|------|
| MailingAddress | string? |
| MailingCity | string? |
| MailingState | string? |
| MailingCountry | string? |
| MailingZipCode | string? |

**Contact Channels (2 fields):**

| Field | Type |
|-------|------|
| EmailWork | string? |
| PhoneWork | string? |

**Professional Hierarchy (4 fields):**

| Field | Type |
|-------|------|
| ReportsToContactId | int? |
| AssistantContactId | int? |
| AssistantName | string? |
| AssistantPhone | string? |

**Lead Information (7 fields):**

| Field | Type |
|-------|------|
| LeadSource | string? |
| LeadScore | int? (0-100) |
| IsQualified | bool? |
| QualifiedDate | DateTime? |
| ConvertedDate | DateTime? |
| ConvertedToAccountId | int? |
| LeadRating | string? |

**Communication Preferences (9 fields):**

| Field | Type |
|-------|------|
| PreferredContactTime | string? |
| Timezone | string? |
| PreferredLanguage | string? |
| OptInEmail | bool |
| OptInSms | bool |
| OptInPhone | bool |
| OptInMail | bool |
| LastOptInDate | DateTime? |
| LastOptOutDate | DateTime? |

**Social (3 fields):**

| Field | Type |
|-------|------|
| FacebookUrl | string? |
| InstagramHandle | string? |
| BlogUrl | string? |

**Assignment & Classification (4 fields):**

| Field | Type |
|-------|------|
| OwnerId | int? |
| AssignedToUserId | int? |
| Territory | string? |
| Tags | string? |

**Engagement Tracking (7 fields):**

| Field | Type |
|-------|------|
| LastActivityDate | DateTime? |
| LastContactedDate | DateTime? |
| NextFollowUpDate | DateTime? |
| TotalInteractions | int? |
| EmailsReceived | int? |
| EmailsOpened | int? |
| LinksClicked | int? |

**Other (3 fields):**

| Field | Type |
|-------|------|
| CustomFields | string? (JSON) |
| PhotoUrl | string? |
| Description | string? |

---

### Invoice — Missing Computed & Entity Fields from DTO

**File:** `CRM.Backend/src/CRM.Application/DTOs/InvoiceDtos.cs`

| Field | Type | Note |
|-------|------|------|
| IsPaid | bool (NotMapped) | Computed: AmountPaid >= TotalAmount |
| LateFeeTotal | decimal | Entity field |
| DaysOverdue | int (NotMapped) | Computed from DueDate |
| CollectionsReference | string? | Entity field |
| CollectionsDate | DateTime? | Entity field (also missing from FE type) |
| VoidedById | int? | FK to voiding user |
| PaymentTermsDescription | string? | Entity field |
| ExternalInvoiceId | string? | In FE type already, missing from DTO |
| ReferenceNumber | string? | In FE type already, missing from DTO |
| BatchNumber | string? | In FE type already, missing from DTO |

---

### Payment — Missing Fraud & Reconciliation Fields from DTO

**File:** `CRM.Backend/src/CRM.Application/DTOs/PaymentDtos.cs`

| Field | Type | Note |
|-------|------|------|
| AmountUnapplied | decimal (NotMapped) | Computed: Amount - AmountApplied |
| NetAmount | decimal (NotMapped) | Computed: Amount - ProcessingFee |
| FraudFlagged | bool | Entity field |
| AvsResponseCode | string? | AVS verification result |
| CvvResponseCode | string? | CVV verification result |
| RiskScore | decimal? | Fraud risk score |
| RefundReason | string? | Entity field |
| ProcessedById | int? | FK to processing user |
| RoutingNumberLast4 | string? | Masked bank data |

---

### Account — Missing Financial, Compliance & Partnership Fields from DTO

**File:** `CRM.Backend/src/CRM.Application/DTOs/AccountDtos.cs`

**Financial Metrics (9 fields):**

| Field | Type |
|-------|------|
| LifetimeValue | decimal? |
| MonthlyRecurringRevenue | decimal? |
| AnnualRecurringRevenue | decimal? |
| AverageOrderValue | decimal? |
| ContractValue | decimal? |
| LastPaymentDate | DateTime? |
| PaymentStatus | string? |
| ActiveSubscriptionCount | int? |
| TotalInvoiceCount | int? |

**Compliance & Verification (13 fields):**

| Field | Type |
|-------|------|
| VerificationStatus | string? |
| VerificationDate | DateTime? |
| VerificationMethod | string? |
| VerifiedByUserId | int? |
| RequiresNda | bool |
| NdaSigned | bool |
| NdaSignedDate | DateTime? |
| NdaReferenceId | string? |
| DataClassification | string? |
| DunsNumber | string? |
| BusinessLicense | string? |
| ComplianceCheckDate | DateTime? |
| ComplianceNotes | string? |

**Partnership & Reseller (10 fields):**

| Field | Type |
|-------|------|
| IsReseller | bool? |
| IsPartner | bool? |
| IsIntegrationPartner | bool? |
| PartnerTier | string? |
| PartnerEnrolledDate | DateTime? |
| PartnerStatus | string? |
| ParentResellerAccountId | int? |
| CompetitorAccountId | int? |
| TechStack | string? |
| IntegrationPartnerType | string? |

**Lead Conversion & Branding (5 fields):**

| Field | Type |
|-------|------|
| ConvertedFromLeadId | int? |
| SourceCampaignId | int? |
| LogoUrl | string? |
| CurrencyLookupId | int? |
| BillingCycleLookupId | int? |

---

### User — Missing Preference & Security Status Fields from DTO

**File:** `CRM.Backend/src/CRM.Application/DTOs/UserDtos.cs`

| Field | Type | Note |
|-------|------|------|
| TwoFactorEnabled | bool | Public status (not the secret) |
| PasswordLastChangedAt | DateTime? | Password age |
| MustResetPassword | bool | Force reset flag |
| EmailVerified | bool | Verification status |
| ThemePreference | string | UI theme |
| Language | string? | UI language |
| Timezone | string? (IANA) | User timezone |
| DateFormat | string? | Display format |
| TimeFormat | string? | 12h/24h preference |
| RowsPerPage | int? | Table pagination |
| EmailNotifications | bool? | Notification opt-in |
| DesktopNotifications | bool? | Desktop push opt-in |
| CompactMode | bool? | UI density |

---

### Contract — Missing Computed Fields from DTO

**File:** `CRM.Backend/src/CRM.Application/DTOs/ContractDtos.cs`

| Field | Type | Note |
|-------|------|------|
| DaysUntilExpiration | int? (NotMapped) | Computed from EndDate |
| IsExpiringSoon | bool (NotMapped) | Computed: DaysUntilExpiration <= 30 |
| IsSigned | bool (NotMapped) | Computed: SignedDate != null |
| SentForSignatureAt | DateTime? | Missing alias mapping |
| ContractFileMimeType | string? | Entity field |
| ContractFileSize | long? | Entity field (also missing from FE type) |
| SignedBy | string? | Entity field (also missing from FE type) |

---

## P2 — Frontend TypeScript Type Deficiencies

These fields exist in the DTO but are absent from the TypeScript interface.

---

### Campaign (`marketing.ts`) — ~90 Fields Missing

**File:** `CRM.Frontend/src/types/marketing.ts`

The `MarketingCampaign` interface covers approximately 40 of the 158 entity/DTO fields. Categories largely absent:

**Scheduling & Meta (9):**
`Type`, `PrimarySuccessMetric`, `Theme`, `ValueProposition`, `DurationDays`, `IsEvergreen`, `Timezone`, `Schedule`, `Objective` (string description field)

**Targeting (13):**
`TargetAudienceDescription`, `TargetDemographics`, `TargetFirmographics`, `TargetGeography`, `TargetIndustries`, `TargetSegments`, `TargetPersonas`, `TargetJobTitles`, `TargetSeniorityLevels`, `TargetAccounts`, `ExclusionCriteria`, `SuppressionLists`, `AudienceListId`

**Revenue (4):**
`ActualRevenue`, `ActualCost`, `PipelineInfluenced`, `PipelineCreated`

**Cost Metrics (4):**
`CostPerMql`, `CostPerSql`, `CostPerOpportunity`, `CostPerAcquisition`

**Conversion Funnel (10):**
`SalsGenerated`, `OpportunitiesInfluenced`, `AccountsAcquired`, `LeadToMqlRate`, `MqlToSqlRate`, `SqlToOpportunityRate`, `OpportunityToWinRate`, `ConversionRate`, `AverageLeadScore`, `LeadQualityDistribution`

**Digital/Email Metrics (17):**
`Frequency`, `FormSubmissions`, `FormConversionRate`, `ContentDownloads`, `VideoViews`, `VideoCompletionRate`, `DemoRequests`, `TrialSignups`, `EmailClickRate`, `ClickToOpenRate`, `HardBounces`, `SoftBounces`, `Unsubscribes`, `UnsubscribeRate`, `SpamComplaints`, `ComplaintRate`, `EmailForwards`, `ListGrowth`

**Social Metrics (8):**
`SocialEngagementRate`, `SocialComments`, `SocialLikes`, `SocialSaves`, `NewFollowers`, `ProfileVisits`, `Mentions`, `SentimentScore`

**Paid Advertising (7):**
`AdSpend`, `CostPerClick`, `CostPerMille`, `Roas`, `QualityScore`, `AveragePosition`, `ImpressionShare`

**Event/Webinar (7):**
`Registrations`, `AttendanceRate`, `OnDemandViews`, `PollResponses`, `QuestionsAsked`, `EventSatisfactionScore`, `WebinarPlatform`, `WebinarRecordingUrl`

**A/B Testing (3):**
`WinningVariant`, `StatisticalSignificance`, `ABTestResults`

**Goal Tracking (4):**
`TargetConversions`, `GoalAchievementPercent`, `CampaignHealthScore`, `BenchmarkComparison`

**Content (10):**
`MessageSubject`, `PreheaderText`, `MessageBody`, `FromName`, `FromEmail`, `ReplyToEmail`, `CallToAction`, `CtaUrl`, `TrackingUrl`, `CreativeAssets`

**Admin (13):**
`ApprovedByUserId`, `ApprovedDate`, `RelatedCampaigns`, `Initiative`, `TeamMembers`, `Tags`, `Notes`, `SuccessCriteria`, `LessonsLearned`, `Attachments`, `BriefUrl`, `ReportUrl`, `CustomFields`

**Integration (5):**
`Channels`, `Platforms`, `SocialNetworks`, `AdPlatforms`, `ExternalCampaignIds`

**Keywords (2):**
`Keywords`, `NegativeKeywords`

---

### ServiceRequest (`itsm.ts`) — 21 Fields Missing

**File:** `CRM.Frontend/src/types/itsm.ts`

| Missing Field | In DTO? |
|---------------|---------|
| TicketNumber | Yes |
| CategoryId | Yes |
| SubcategoryId | Yes |
| RequesterName | Yes |
| RequesterEmail | Yes |
| RequesterPhone | Yes |
| AssignedToGroupId | Yes |
| CreatedByUserId | Yes |
| LastModifiedByUserId | Yes |
| ResponseDueDate | Yes |
| ResolutionDueDate | Yes |
| StatusCode | Yes |
| ResponseSlaBreached | Yes |
| ResolutionSlaBreached | Yes |
| SourcePhoneNumber | Yes |
| ConversationId | Yes |
| RelatedOpportunityId | Yes |
| RelatedProductId | Yes |
| ParentServiceRequestId | Yes |
| SourceInteractionId | Yes |
| CustomFieldValues | Yes |

---

### Order (`sales.ts`) — 22 Fields Missing

**File:** `CRM.Frontend/src/types/sales.ts`

**Billing Address (7):**
`BillingName`, `BillingCompany`, `BillingStreet`, `BillingCity`, `BillingState`, `BillingPostalCode`, `BillingCountry`

**Shipping Address (7):**
`ShippingName`, `ShippingCompany`, `ShippingStreet`, `ShippingCity`, `ShippingState`, `ShippingPostalCode`, `ShippingCountry`

**Revenue & Financial (5):**
`CurrencyCode`, `OneTimeRevenue`, `RecurringRevenue`, `SubmittedDate`, `FulfilledDate`

**Other (3):**
`Name`, `Description`, `CompletedDate`

---

### Quote (`sales.ts`) — 15 Fields Missing

**File:** `CRM.Frontend/src/types/sales.ts`

**Billing Address (6):**
`BillingName`, `BillingAddress`, `BillingCity`, `BillingState`, `BillingZipCode`, `BillingCountry`

**Shipping Address (6):**
`ShippingName`, `ShippingAddress`, `ShippingCity`, `ShippingState`, `ShippingZipCode`, `ShippingCountry`

**Other (3):**
`TermsAndConditions`, `ValidityDays`, `ApprovalNotes`

---

### Contact (`crm.ts`) — Normalized Collections & Scalar Fields Missing

**File:** `CRM.Frontend/src/types/crm.ts`

**DTO Collection Sub-types (not in FE type):**

| DTO Field | Sub-type |
|-----------|----------|
| EmailAddresses | LinkedEmailDto[] |
| PhoneNumbers | LinkedPhoneDto[] |
| Addresses | LinkedAddressDto[] |
| SocialMediaAccounts | LinkedSocialMediaDto[] |
| SocialMediaLinks | SocialMediaLinkDto[] |

**Scalar Fields from DTO missing in FE type (3):**
`DateAdded`, `LastModified`, `ModifiedBy`

---

### Contract (`sales.ts`) — Field Gaps

**File:** `CRM.Frontend/src/types/sales.ts`

| Issue | Detail |
|-------|--------|
| ContractFileSize | In DTO, missing from FE type |
| SignedBy | In DTO, missing from FE type |
| TotalValue | In DTO (use as primary), FE type has `value` instead |
| IsExpiringSoon | FE type has it, DTO does not (gap in both directions) |

---

## P3 — Enum Mismatches

These mismatches cause incorrect numeric values to be sent to the API or incorrect status rendering on the frontend.

---

### Lead — Status & Source

**Backend `LeadLifecycleStatus`:** New=0, Contacted=1, Qualified=2, Disqualified=3, Nurturing=4, Converted=5
**Backend `LeadSource`:** Numeric enum 0–5
**Frontend:** Custom string enum values that **do not correspond to backend numeric values**

**Risk:** Every Lead create/update with status or source sends the wrong integer to the API.

**Fix:** Align `CRM.Frontend/src/constants.ts` LeadStatus and LeadSource maps to use correct numeric values 0–5.

---

### Quote — Status (13 backend vs 6 frontend)

**Backend `QuoteStatus`:**
New=0, Draft=1, UnderApproval=2, Approved=3, Shared=4, Viewed=5, Accepted=6, Rejected=7, Expired=8, Revised=9, Cancelled=10, Converted=11, EndOfLife=12

**Frontend:** draft, pending, approved, sent, accepted, rejected

**Missing from frontend:** New, UnderApproval, Viewed, Expired, Revised, Cancelled, Converted, EndOfLife

**Fix:** Expand QuoteStatus in `CRM.Frontend/src/constants.ts` and `sales.ts` to all 13 values.

---

### Order — Status (13 backend vs 9 frontend)

**Backend `OrderStatus`:** 13 values including Draft, Submitted, Pending, Processing, Approved, OnHold, Shipped, Delivered, Completed, Cancelled, Refunded, Returned, ActionRequired
**Frontend:** 9 values — missing ActionRequired and several transition states

**Fix:** Expand OrderStatus in `CRM.Frontend/src/constants.ts` and `sales.ts` to all 13 values.

---

## P4 — Field Name Mismatches

These mismatches cause the frontend to send or receive the wrong JSON key names.

| Entity | Entity Field | DTO Field | FE Type Field | Layer with mismatch |
|--------|-------------|-----------|---------------|---------------------|
| Quote | Discount | DiscountTotal | discount | FE key differs from DTO serialization |
| Quote | Tax | TaxTotal | tax | FE key differs from DTO serialization |
| Quote | Total | GrandTotal | total | FE key differs from DTO serialization |
| Quote | ExpirationDate | ExpirationDate | expiryDate | FE key differs from DTO |
| Order | AccountId | AccountId | customerId | FE key differs from DTO |
| CrmTask | Subject | Title | title | Intentional rename in DTO |
| ServiceRequest | Subject | Subject | title | FE uses different key |

**Note:** `GrandTotal` serializes as `grandTotal` in camelCase JSON — the FE `total` field will not bind correctly.

---

## P5 — FE UI Form Gaps

*Fields present in DTO and FE type but not exposed in any UI form.*

---

### Account UI Gaps (~27 fields)

| Group | Missing Fields |
|-------|---------------|
| Shipping Address | ShippingAddress, ShippingAddress2, ShippingCity, ShippingState, ShippingZipCode, ShippingCountry, ShippingSameAsBilling |
| Business Detail | TaxId, RegistrationNumber, SubIndustry, NumberOfEmployees, EmployeeRange, RevenueRange, StockSymbol, Ownership |
| Lifecycle Dates | FirstContactDate, ConversionDate, LastActivityDate |
| Financial | TotalPurchases, AccountBalance, CreditLimit, PaymentTerms, PreferredPaymentMethod, Currency, BillingCycle |
| Scoring | LeadScore, NpsScore, SatisfactionRating |
| Preferences | PreferredContactTime, OptInEmail, OptInSms, OptInPhone, PreferredContactMethod, Timezone, PreferredLanguage |
| Assignment | AccountManagerId, Territory, Region |
| Classification | Segment, ReferralSource, ReferredByAccountId, ParentAccountId |
| Documentation | InternalNotes, Description, CustomFields |

---

### Contact UI Gaps (~20 fields)

| Group | Missing Fields |
|-------|---------------|
| Personal | MiddleName, Salutation, Suffix, Nickname, Gender, DateOfBirth |
| Phones | PhoneMobile, PhoneFax |
| Address | Address2 |
| Status & Prefs | LeadStatus, DoNotContact, PreferredContactMethod |

---

### Activity UI Gaps (~12 fields)

| Group | Missing Fields |
|-------|---------------|
| Content | Details (JSON), Category, Source |
| Duration | DurationMinutes |
| Secondary Entity | SecondaryEntityType, SecondaryEntityId, SecondaryEntityName |
| Relationships | ContactId, OpportunityId, CampaignId, QuoteId, ProductId, TaskId |

---

### Invoice UI Gaps (~15 fields)

| Group | Missing Fields |
|-------|---------------|
| Classification | Description, InvoiceType |
| Dates | ServicePeriodStart, ServicePeriodEnd |
| Financials | Subtotal, DiscountAmount, TaxAmount, TaxRate, ShippingAmount, Amount, CurrencyCode, EarlyPaymentDiscountAmount, LateFeeAmount |
| Relations | ContactId, OriginalInvoiceId |
| Admin | Footer, TermsAndConditions, VoidReason, InCollections |

---

### Payment UI Gaps (~14 fields)

| Group | Missing Fields |
|-------|---------------|
| Identifiers | PaymentNumber, ExternalPaymentId, GatewayTransactionId, GatewayReference, CheckNumber |
| Financials | AmountApplied, ProcessingFee, ExchangeRate |
| Dates | ProcessedDate, SettledDate, DepositDate, ScheduledDate |
| Relations | AccountId, OriginalPaymentId |

---

### Contract UI Gaps (~5 fields)

| Group | Missing Fields |
|-------|---------------|
| Currency | CurrencyCode |
| Renewal Tracking | RenewalNoticeSent, RenewalNoticeSentDate, RenewalInitiatedAt, RenewalCompletedAt |

---

### Campaign UI Gaps (~35 fields)

| Group | Missing Fields |
|-------|---------------|
| Scheduling | ActualStartDate, ActualEndDate, ObjectiveType |
| Budget | DailyBudget, MonthlyBudget, ExpectedRevenue, CostPerLead, CostPerAcquisition |
| Audience | AudienceType, TargetAudience (count field) |
| Lead Metrics | LeadsGenerated, MqlsGenerated, SqlsGenerated, OpportunitiesCreated, DealsWon |
| Email Metrics | EmailsSent, EmailsDelivered, DeliveryRate, EmailsOpened, OpenRate, EmailClicks, BounceRate, Bounces |
| Digital | Impressions, Reach, Clicks, ClickThroughRate, LandingPageVisits |
| Social | SocialReach, SocialEngagement, SocialShares |
| Event | Attendance, NoShows, EventCapacity, EventLocation, EventDateTime |
| Admin | CostCenter, ParentCampaignId, ExternalId, SyncStatus, LastSyncDate, ABTestMetric |

---

### ServiceRequest UI Gaps (~5 fields)

| Group | Missing Fields |
|-------|---------------|
| Resolution | ResolutionCode, RootCause |
| Feedback | SatisfactionRating, CustomerFeedback |
| Reference | ExternalReferenceId |

---

### Lead UI Gaps (~3 fields)

| Group | Missing Fields |
|-------|---------------|
| Relationships | OwnerId, AccountId, ContactId |

---

### Opportunity UI Gaps (~4 fields)

| Group | Missing Fields |
|-------|---------------|
| Relationships | LeadId |
| Computed (read-only display) | WeightedAmount, IsOpen, IsWon |

---

### User UI Gaps (~3 fields)

| Group | Missing Fields |
|-------|---------------|
| Display | ContactName |
| Assignment | PrimaryGroupId, PrimaryGroupName |

---

## Intentionally Excluded Fields

*These should never be added to DTOs or FE types.*

### Security — Never Expose

| Entity | Field | Reason |
|--------|-------|--------|
| User | PasswordHash | Credential hash |
| User | TwoFactorSecret | TOTP secret |
| User | BackupCodes | Recovery secrets |
| User | PasswordResetToken | One-time token |
| User | EmailVerificationToken | One-time token |
| Payment | GatewayResponseRaw | Raw gateway data (PCI scope) |
| Payment | FraudNotes | Investigation data (PII) |
| Payment | IpAddress | PII |
| Payment | DeviceFingerprint | PII |
| Activity | IpAddress | Audit log only (PII) |
| Activity | UserAgent | Audit log only (PII) |

### Soft Delete / Merge Tracking — Admin-Only

| Entity | Fields |
|--------|--------|
| Account | IsDeleted, MergedIntoId, MergeGroupId, IsMergedDuplicate, MergedAt |
| Contact | IsDeleted, MergedIntoId, MergeGroupId, IsMergedDuplicate, MergedAt |
| Lead | IsDeleted, MergedIntoId, MergeGroupId, IsMergedDuplicate, MergedAt |

### Navigation Properties — EF Core Only

Navigation property objects (e.g., `Account`, `Contact`, `Owner`) are intentionally excluded from all DTOs to prevent circular serialization and maintain stable API contracts. Only FK integer IDs are surfaced.

---

## Implementation Notes

### Fix Order by Dependency

| Scenario | Required Order |
|----------|---------------|
| ServiceRequest expedite fields | Entity → EF Migration → DTO → FE Type → FE UI |
| CrmTask DTO gaps | DTO update → Controller update (FE type already ready) |
| Invoice/Payment/Contract computed fields | DTO update → verify controller mapping |
| Campaign FE type | FE Type expansion → FE UI (DTO is complete) |
| ServiceRequest FE type | FE Type expansion (DTO is complete) |
| Quote/Order address fields | FE Type expansion (DTO is complete) |
| Lead/Quote/Order enum mismatches | `constants.ts` enum map alignment |
| Field name mismatches (Quote, Order) | FE type rename + verify API serialization binding |

### Notes on Field Name Mismatches

When fixing P4 mismatches, update both:
1. The TypeScript interface property name
2. The `handleSave` payload construction in the page component

Do not rely on camelCase auto-mapping to fix `GrandTotal → total` — the DTO property `GrandTotal` serializes as `grandTotal`, not `total`. The FE `total` key will silently fail to bind.

---

*Last updated: 2026-02-21 — Post-Session 9 comprehensive re-audit of all 16 entities*
