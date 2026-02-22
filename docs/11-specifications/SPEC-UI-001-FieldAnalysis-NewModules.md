# SPEC-UI-001 — Field Analysis for New Module Seed Data

> **Purpose:** Comprehensive analysis of ALL form fields across 7 pages to generate `ModuleFieldConfiguration` seed data.  
> **Generated:** 2026-02-23  
> **Status:** Analysis Complete — Ready for implementation in `GetDefaultFieldsForModule()`

---

## Summary

| Module | Page File | Form Fields | Tabs (proposed) | Has Existing Seed? | Notes |
|--------|-----------|------------|-----------------|-------------------|-------|
| **Quote** | QuotesPage.tsx (994 lines) | 43 | 4 | ✅ Yes (10 fields) — needs expansion | Expand from 10→43 fields |
| **Order** | OrdersPage.tsx (1081 lines) | 33 | 4 | ❌ No | New module |
| **Invoice** | InvoicesPage.tsx (1199 lines) | 37 | 5 | ❌ No | New module |
| **Contract** | ContractsPage.tsx (1211 lines) | 31 | 4 | ❌ No | New module |
| **ServiceRequest** | ServiceRequestsPage.tsx (1933 lines) | 25 | 3 | ❌ No | New module |
| **Payment** | PaymentsPage.tsx (906 lines) | 30 | 4 | ❌ No | New module |
| **Interaction** | InteractionsPage.tsx (1272 lines) | 0 (no form) | — | ❌ No | ⚠️ No create/edit dialog exists |

**Total NEW fields to seed:** ~199 (across 6 modules with forms)

---

## Module Names to Add

Add these to `ModuleNames` class and `All` array:

```csharp
public const string Orders = "Orders";
public const string Invoices = "Invoices";
public const string Contracts = "Contracts";
public const string ServiceRequests = "ServiceRequests";
public const string Payments = "Payments";
// public const string Interactions = "Interactions"; // No form dialog yet

public static readonly string[] All = new[]
{
    Accounts, Contacts, Leads, Opportunities, Products, Campaigns, Quotes,
    Orders, Invoices, Contracts, ServiceRequests, Payments
};
```

---

## 1. QUOTES (Expanded — Replace existing 10-field seed)

**Source:** `QuotesPage.tsx` → `QuoteForm` interface  
**Module Name:** `"Quotes"` (existing)  
**Existing seed:** 10 fields (Basic Info, Pricing, Terms) — **Replace with 43 fields below**

### Tab 0: "Details" (6 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options | notes |
|---|-----------|-----------|-----------|-----------|----------|---------|-------|
| 1 | `name` | Quote Title | text | ✅ | 12 | | |
| 2 | `accountId` | Account | lookup | ❌ | 6 | | EntitySelect component |
| 3 | `status` | Status | select | ❌ | 6 | `0:New,1:Draft,2:Under Approval,3:Approved,4:Shared,5:Viewed,6:Accepted,7:Rejected,8:Expired,9:Revised,10:Cancelled,11:Converted,12:End of Life` | QuoteStatus enum |
| 4 | `expirationDate` | Valid Until | date | ❌ | 6 | | |
| 5 | `description` | Description | textarea | ❌ | 12 | | rows=2 |
| 6 | `billingAddress` | Billing Address | textarea | ❌ | 12 | | Accordion "Additional Info", rows=3 |
| 7 | `shippingAddress` | Shipping Address | textarea | ❌ | 12 | | Accordion "Additional Info", rows=3 |
| 8 | `termsAndConditions` | Terms & Conditions | textarea | ❌ | 12 | | Accordion "Additional Info", rows=6 |
| 9 | `notes` | Notes | textarea | ❌ | 12 | | Accordion "Additional Info", rows=3 |

### Tab 1: "Pricing" (4 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 10 | `subtotal` | Subtotal ($) | currency | ❌ | 6 | |
| 11 | `discountPercent` | Discount (%) | number | ❌ | 6 | |
| 12 | `taxRate` | Tax (%) | number | ❌ | 6 | |
| 13 | `shippingCost` | Shipping ($) | currency | ❌ | 6 | |

### Tab 2: "Addresses" (12 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 14 | `billingName` | Billing Name | text | ❌ | 6 | |
| 15 | `shippingName` | Shipping Name | text | ❌ | 6 | |
| 16 | `billingAddress` | Billing Street | text | ❌ | 6 | **NOTE: Same fieldName as tab 0 — use `billingStreet` instead** |
| 17 | `shippingAddress` | Shipping Street | text | ❌ | 6 | **NOTE: Same fieldName as tab 0 — use `shippingStreet` instead** |
| 18 | `billingCity` | Billing City | text | ❌ | 4 | |
| 19 | `billingState` | Billing State | text | ❌ | 4 | |
| 20 | `billingZipCode` | Billing Zip | text | ❌ | 4 | |
| 21 | `shippingCity` | Shipping City | text | ❌ | 4 | |
| 22 | `shippingState` | Shipping State | text | ❌ | 4 | |
| 23 | `shippingZipCode` | Shipping Zip | text | ❌ | 4 | |
| 24 | `billingCountry` | Billing Country | text | ❌ | 6 | |
| 25 | `shippingCountry` | Shipping Country | text | ❌ | 6 | |

> ⚠️ **Dedup issue:** `billingAddress` and `shippingAddress` appear in both Tab 0 (textarea for legacy) and Tab 2 (text for structured). In the seed, place them only on Tab 0 as textarea fields. The structured address fields (billingCity, etc.) go on Tab 2.

### Tab 3: "Terms & Approval" (16 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 26 | `requiresApproval` | Requires Approval | checkbox | ❌ | 6 | |
| 27 | `isApproved` | Is Approved | checkbox | ❌ | 6 | |
| 28 | `approvedByUserId` | Approved By (User ID) | number | ❌ | 6 | |
| 29 | `approvalDate` | Approval Date | date | ❌ | 6 | |
| 30 | `submittedForApprovalDate` | Submitted for Approval | date | ❌ | 6 | |
| 31 | `approvalNotes` | Approval Notes | textarea | ❌ | 12 | |
| 32 | `isSigned` | Is Signed | checkbox | ❌ | 6 | |
| 33 | `signedDate` | Signed Date | date | ❌ | 6 | |
| 34 | `signedBy` | Signed By | text | ❌ | 6 | |
| 35 | `signatureUrl` | Signature URL | url | ❌ | 6 | |
| 36 | `contactEmail` | Contact Email | email | ❌ | 6 | |
| 37 | `contactPhone` | Contact Phone | phone | ❌ | 6 | |
| 38 | `paymentTerms` | Payment Terms | text | ❌ | 6 | |
| 39 | `deliveryTerms` | Delivery Terms | text | ❌ | 6 | |
| 40 | `warrantyMonths` | Warranty (months) | number | ❌ | 6 | |
| 41 | `internalNotes` | Internal Notes | textarea | ❌ | 12 | |

> **ExtraTabs (NOT seeded):** Line Items (100), Notes (102), Related (103)

**Total Quote fields: 41** (deduplicated — billingAddress/shippingAddress only on Tab 0)

---

## 2. ORDERS (New Module)

**Source:** `OrdersPage.tsx` → `OrderForm` interface  
**Module Name:** `"Orders"` (new)

### Tab 0: "Order Details" (9 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options | notes |
|---|-----------|-----------|-----------|-----------|----------|---------|-------|
| 1 | `accountId` | Account | lookup | ✅ | 6 | | EntitySelect |
| 2 | `status` | Status | select | ❌ | 6 | `0:Draft,1:Pending Approval,2:Approved,3:Processing,4:Partially Fulfilled,5:Fulfilled,6:Delivered,7:Completed,8:Cancelled,9:Returned,10:Refunded,11:On Hold,12:Action Required` | OrderStatus enum |
| 3 | `orderDate` | Order Date | date | ❌ | 6 | | |
| 4 | `requestedDate` | Requested Date | date | ❌ | 6 | | |
| 5 | `opportunityId` | Related Opportunity | lookup | ❌ | 6 | | EntitySelect |
| 6 | `quoteId` | Related Quote ID | number | ❌ | 6 | | |
| 7 | `notes` | Notes | textarea | ❌ | 12 | | Accordion "Additional Info" |
| 8 | `shippingAddress` | Shipping Address | textarea | ❌ | 12 | | Accordion "Additional Info" |
| 9 | `billingAddress` | Billing Address | textarea | ❌ | 12 | | Accordion "Additional Info" |

### Tab 1: "Shipping & Fulfillment" (13 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 10 | `shippingName` | Shipping Name | text | ❌ | 6 | |
| 11 | `shippingMethod` | Shipping Method | text | ❌ | 6 | |
| 12 | `shippingAddress` | Shipping Street Address | text | ❌ | 12 | **NOTE: Duplicate field name — UI reuses. Seed only once on Tab 0 as textarea** |
| 13 | `shippingCity` | City | text | ❌ | 4 | |
| 14 | `shippingState` | State | text | ❌ | 4 | |
| 15 | `shippingZipCode` | Zip Code | text | ❌ | 4 | |
| 16 | `shippingCountry` | Country | text | ❌ | 6 | |
| 17 | `shippingCarrier` | Carrier | text | ❌ | 6 | |
| 18 | `trackingNumber` | Tracking Number | text | ❌ | 6 | |
| 19 | `trackingUrl` | Tracking URL | url | ❌ | 6 | |
| 20 | `shippedDate` | Shipped Date | date | ❌ | 4 | |
| 21 | `estimatedDeliveryDate` | Est. Delivery Date | date | ❌ | 4 | |
| 22 | `deliveredDate` | Delivered Date | date | ❌ | 4 | |

### Tab 2: "Billing & Payment" (13 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 23 | `billingName` | Billing Name | text | ❌ | 6 | |
| 24 | `billingAddress` | Billing Street | text | ❌ | 6 | **Duplicate — seed only on Tab 0** |
| 25 | `billingCity` | Billing City | text | ❌ | 4 | |
| 26 | `billingState` | Billing State | text | ❌ | 4 | |
| 27 | `billingZipCode` | Billing Zip | text | ❌ | 4 | |
| 28 | `billingCountry` | Billing Country | text | ❌ | 6 | |
| 29 | `paymentMethod` | Payment Method | select | ❌ | 6 | `bank_transfer,credit_card,check,cash,other` | |
| 30 | `paymentTerms` | Payment Terms | text | ❌ | 6 | |
| 31 | `paymentReference` | Payment Reference | text | ❌ | 6 | |
| 32 | `paymentDate` | Payment Date | date | ❌ | 6 | |

### Tab 3: "Revenue" (3 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 33 | `revenueRecognitionMethod` | Revenue Recognition | select | ❌ | 6 | `immediate,straight_line,milestone,usage` |
| 34 | `revenueStartDate` | Revenue Start Date | date | ❌ | 6 | |
| 35 | `revenueEndDate` | Revenue End Date | date | ❌ | 6 | |

> **ExtraTabs (NOT seeded):** Line Items (100), Notes (102)

**Total Order fields: 33** (deduplicated: shippingAddress on Tab 0 only, billingAddress on Tab 0 only; structured address fields on respective tabs)

**Deduplicated seed count: 33**

---

## 3. INVOICES (New Module)

**Source:** `InvoicesPage.tsx` → `InvoiceForm` interface  
**Module Name:** `"Invoices"` (new)

### Tab 0: "Invoice Details" (5 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options | notes |
|---|-----------|-----------|-----------|-----------|----------|---------|-------|
| 1 | `customerId` | Account | lookup | ✅ | 6 | | EntitySelect |
| 2 | `status` | Status | select | ❌ | 6 | `0:Draft,1:Pending Approval,2:Approved,3:Sent,4:Viewed,5:Partially Paid,6:Paid,7:Overdue,8:Disputed,9:Voided,10:Written Off,11:Collections,12:Refunded` | InvoiceStatus enum |
| 3 | `issueDate` | Issue Date | date | ❌ | 6 | | |
| 4 | `dueDate` | Due Date | date | ❌ | 6 | | |
| 5 | `notes` | Notes | textarea | ❌ | 12 | | |

### Tab 1: "Additional & Terms" (4 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 6 | `discountPercent` | Discount % | number | ❌ | 4 | |
| 7 | `taxRate` | Tax Rate % | number | ❌ | 4 | |
| 8 | `paymentTerms` | Payment Terms | text | ❌ | 4 | |
| 9 | `internalNotes` | Internal Notes | textarea | ❌ | 12 | |

### Tab 2: "Classification & Service" (4 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 10 | `description` | Description | textarea | ❌ | 12 | |
| 11 | `invoiceType` | Invoice Type | select | ❌ | 6 | `standard,proforma,credit,debit,recurring` |
| 12 | `servicePeriodStart` | Service Period Start | date | ❌ | 6 | |
| 13 | `servicePeriodEnd` | Service Period End | date | ❌ | 6 | |

### Tab 3: "Financial Details" (10 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options | notes |
|---|-----------|-----------|-----------|-----------|----------|---------|-------|
| 14 | `subtotal` | Subtotal | currency | ❌ | 4 | | readOnly |
| 15 | `discountAmount` | Discount Amount | currency | ❌ | 4 | | |
| 16 | `taxAmount` | Tax Amount | currency | ❌ | 4 | | |
| 17 | `shippingAmount` | Shipping Amount | currency | ❌ | 4 | | |
| 18 | `currencyCode` | Currency Code | text | ❌ | 4 | | default "USD" |
| 19 | `earlyPaymentDiscountAmount` | Early Payment Discount | currency | ❌ | 6 | | readOnly |
| 20 | `lateFeeAmount` | Late Fee Amount | currency | ❌ | 6 | | readOnly |
| 21 | `inCollections` | In Collections | checkbox | ❌ | 6 | | Switch |
| 22 | `collectionReference` | Collection Reference | text | ❌ | 6 | | |
| 23 | `voidReason` | Void Reason | text | ❌ | 6 | | readOnly |

### Tab 4: "Billing & Relations" (14 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 24 | `contactId` | Contact ID | number | ❌ | 6 | |
| 25 | `originalInvoiceId` | Original Invoice ID | number | ❌ | 6 | |
| 26 | `footer` | Footer | textarea | ❌ | 12 | |
| 27 | `termsAndConditions` | Terms & Conditions | textarea | ❌ | 12 | |
| 28 | `billingName` | Billing Name | text | ❌ | 6 | |
| 29 | `billingCompany` | Billing Company | text | ❌ | 6 | |
| 30 | `billingStreet` | Street | text | ❌ | 12 | |
| 31 | `billingCity` | City | text | ❌ | 4 | |
| 32 | `billingState` | State | text | ❌ | 4 | |
| 33 | `billingPostalCode` | Postal Code | text | ❌ | 4 | |
| 34 | `billingCountry` | Country | text | ❌ | 6 | |
| 35 | `billingEmail` | Billing Email | email | ❌ | 6 | |
| 36 | `billingPhone` | Billing Phone | phone | ❌ | 6 | |
| 37 | `earlyPaymentDiscountPercent` | Early Payment Discount % | number | ❌ | 6 | |
| 38 | `earlyPaymentDiscountDays` | Early Payment Discount Days | number | ❌ | 6 | |
| 39 | `lateFeePercent` | Late Fee % | number | ❌ | 6 | |

> **ExtraTabs (NOT seeded):** Line Items (100)

**Total Invoice fields: 39** (note: `taxRate` appears on Tab 1 and implicitly on Tab 3 but we seed it once on Tab 1)

**Deduplicated seed count: 37**

---

## 4. CONTRACTS (New Module)

**Source:** `ContractsPage.tsx` → `ContractForm` interface  
**Module Name:** `"Contracts"` (new)

### Tab 0: "Basic Info" (11 core + accordion fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options | notes |
|---|-----------|-----------|-----------|-----------|----------|---------|-------|
| 1 | `name` | Contract Name | text | ✅ | 8 | | |
| 2 | `status` | Status | select | ❌ | 4 | `0:Draft,1:Pending Approval,2:Approved,3:Active,4:Expired,5:Terminated,6:Renewed,7:On Hold` | ContractStatus enum |
| 3 | `description` | Description | textarea | ❌ | 12 | | rows=2 |
| 4 | `contractType` | Contract Type | select | ❌ | 6 | `0:Service Agreement,1:License Agreement,2:Subscription,3:Support Contract,4:Maintenance,5:NDA,6:Master Agreement,7:Amendment,8:Other` | ContractType enum |
| 5 | `accountId` | Account | lookup | ✅ | 6 | | EntitySelect |
| 6 | `contactId` | Contact | lookup | ❌ | 6 | | EntitySelect |
| 7 | `value` | Contract Value | currency | ❌ | 6 | | startAdornment "$" |
| 8 | `startDate` | Start Date | date | ❌ | 4 | | |
| 9 | `endDate` | End Date | date | ❌ | 4 | | |
| 10 | `signedDate` | Signed Date | date | ❌ | 4 | | |
| 11 | `billingFrequency` | Billing Frequency | select | ❌ | 6 | `monthly,quarterly,semi-annual,annual,one-time` | |

### Tab 0 continued: Accordion "Additional Information" (3 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 12 | `specialConditions` | Special Conditions | textarea | ❌ | 12 | |
| 13 | `parentContractId` | Parent Contract ID | number | ❌ | 6 | |
| 14 | `renewalNoticeDays` | Renewal Notice (days) | number | ❌ | 3 | |
| 15 | `autoRenew` | Auto Renew | checkbox | ❌ | 3 | |

### Tab 0 continued: Accordion "Documents & Approval" (7 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 16 | `contractFileUrl` | Contract File URL | url | ❌ | 6 | |
| 17 | `signedContractFileUrl` | Signed Contract URL | url | ❌ | 6 | |
| 18 | `approvedByUserId` | Approved By (User ID) | number | ❌ | 6 | |
| 19 | `approvedDate` | Approved Date | date | ❌ | 6 | |
| 20 | `rejectionReason` | Rejection Reason | text | ❌ | 6 | |
| 21 | `suspensionReason` | Suspension Reason | text | ❌ | 6 | |
| 22 | `suspendedDate` | Suspended Date | date | ❌ | 6 | |
| 23 | `terminationClause` | Termination Clause | textarea | ❌ | 12 | |

### Tab 0 continued: Accordion "Currency & Renewal" (5 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 24 | `currencyCode` | Currency Code | text | ❌ | 4 | | default "USD" |
| 25 | `renewalNoticeSent` | Renewal Notice Sent | checkbox | ❌ | 6 | |
| 26 | `renewalNoticeSentDate` | Notice Sent Date | date | ❌ | 6 | |
| 27 | `renewalInitiatedAt` | Renewal Initiated | date | ❌ | 6 | | readOnly |
| 28 | `renewalCompletedAt` | Renewal Completed | date | ❌ | 6 | | readOnly |

### Tab 1: "Terms & Conditions" (2 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 29 | `terms` | Terms & Conditions | textarea | ❌ | 12 | | rows=8 |
| 30 | `specialConditions` | Special Conditions | textarea | ❌ | 12 | | **Duplicate of #12 — seed once on Tab 0** |

### Tab 2: "Related Records" (3 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 31 | `parentContractId` | Parent Contract ID | number | ❌ | 6 | | **Duplicate of #13 — seed once on Tab 0** |
| 32 | `opportunityId` | Related Opportunity | lookup | ❌ | 6 | | EntitySelect |
| 33 | `quoteId` | Related Quote ID | number | ❌ | 6 | |

> **ExtraTabs (NOT seeded):** Related (103), Notes (102)

**Deduplicated seed count: 31** (specialConditions on Tab 0 only, parentContractId on Tab 0 only; add `terms` on Tab 1, `opportunityId` and `quoteId` on Tab 2)

---

## 5. SERVICE REQUESTS (New Module)

**Source:** `ServiceRequestsPage.tsx` → `CreateServiceRequest` + `ResolutionSlaFields` interfaces  
**Module Name:** `"ServiceRequests"` (new)

### Tab 0: "Request Info" (9 core fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options | notes |
|---|-----------|-----------|-----------|-----------|----------|---------|-------|
| 1 | `subject` | Subject | text | ✅ | 12 | | |
| 2 | `description` | Description | textarea | ❌ | 12 | | rows=4 |
| 3 | `channel` | Channel | select | ✅ | 6 | `0:WhatsApp,1:Email,2:Phone,3:In Person,4:Self Service Portal,5:Social Media,6:Live Chat,7:API` | ServiceRequestChannel enum |
| 4 | `priority` | Priority | select | ❌ | 6 | `0:Low,1:Medium,2:High,3:Critical,4:Urgent` | ServiceRequestPriority enum |
| 5 | `categoryId` | Category | select | ❌ | 6 | *(dynamic from API)* | Loaded from categories endpoint |
| 6 | `accountId` | Account | lookup | ❌ | 6 | | EntitySelect |
| 7 | `contactId` | Contact | lookup | ❌ | 6 | | EntitySelect |
| 8 | `assignedToUserId` | Assign to User | lookup | ❌ | 6 | | EntitySelect (users) |
| 9 | `assignedToGroupId` | Assign to Group | select | ❌ | 6 | *(dynamic from API)* | Loaded from groups endpoint |

### Tab 0 continued: Accordion "Additional Information" (2 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 10 | `subcategoryId` | Subcategory | select | ❌ | 6 | *(dynamic from API)* |
| 11 | `workflowId` | Workflow | select | ❌ | 12 | *(dynamic from API)* |

### Tab 1: "Resolution & SLA" (8 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 12 | `slaStatus` | SLA Status | select | ❌ | 6 | `on_track,at_risk,breached` |
| 13 | `isVipAccount` | VIP Account | checkbox | ❌ | 6 | |
| 14 | `estimatedEffortHours` | Estimated Effort (hrs) | number | ❌ | 6 | |
| 15 | `actualEffortHours` | Actual Effort (hrs) | number | ❌ | 6 | |
| 16 | `resolutionCode` | Resolution Code | text | ❌ | 6 | |
| 17 | `rootCause` | Root Cause | text | ❌ | 6 | |
| 18 | `resolutionSummary` | Resolution Summary | textarea | ❌ | 12 | |
| 19 | `internalNotes` | Internal Notes | textarea | ❌ | 12 | |

### Tab 1 continued: Accordion "Expedite" (2 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 20 | `isExpedited` | Expedited | checkbox | ❌ | 12 | |
| 21 | `expediteReason` | Expedite Reason | text | ❌ | 12 | | parentField: `isExpedited`, parentFieldValue: `true` |

### Tab 2: "Feedback & Reference" (3 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 22 | `satisfactionRating` | Satisfaction Rating | select | ❌ | 6 | `1:1 - Very Unsatisfied,2:2 - Unsatisfied,3:3 - Neutral,4:4 - Satisfied,5:5 - Very Satisfied` |
| 23 | `customerFeedback` | Customer Feedback | textarea | ❌ | 12 | |
| 24 | `externalReferenceId` | External Reference ID | text | ❌ | 12 | |

> **ExtraTabs (NOT seeded):** Related (101), Notes (102)

**Total ServiceRequest fields: 24**

---

## 6. PAYMENTS (New Module)

**Source:** `PaymentsPage.tsx` → `PaymentForm` interface  
**Module Name:** `"Payments"` (new)

### Tab 0: "Payment Info" (5 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options | notes |
|---|-----------|-----------|-----------|-----------|----------|---------|-------|
| 1 | `invoiceId` | Invoice ID | number | ❌ | 12 | | Effectively required |
| 2 | `amount` | Amount | currency | ✅ | 6 | | startAdornment "$" |
| 3 | `paymentMethod` | Payment Method | select | ❌ | 6 | `0:Credit Card,1:Debit Card,2:Bank Transfer,3:Wire Transfer,4:Check,5:Cash,6:PayPal,7:Stripe,8:Apple Pay,9:Google Pay,10:Venmo,11:Crypto,12:Store Credit,13:Gift Card,14:Financing,15:Purchase Order,16:Other` | PaymentMethod enum |
| 4 | `reference` | Reference / Transaction ID | text | ❌ | 12 | | Accordion "Additional" |
| 5 | `notes` | Notes | textarea | ❌ | 12 | | Accordion "Additional" |

### Tab 1: "Identifiers" (5 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options | notes |
|---|-----------|-----------|-----------|-----------|----------|---------|-------|
| 6 | `paymentNumber` | Payment Number | text | ❌ | 6 | | readOnly |
| 7 | `externalPaymentId` | External Payment ID | text | ❌ | 6 | | |
| 8 | `gatewayTransactionId` | Gateway Transaction ID | text | ❌ | 6 | | |
| 9 | `gatewayReference` | Gateway Reference | text | ❌ | 6 | | |
| 10 | `checkNumber` | Check Number | text | ❌ | 6 | | |

### Tab 2: "Financial & Dates" (9 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options |
|---|-----------|-----------|-----------|-----------|----------|---------|
| 11 | `amountApplied` | Amount Applied | currency | ❌ | 4 | |
| 12 | `processingFee` | Processing Fee | currency | ❌ | 4 | |
| 13 | `exchangeRate` | Exchange Rate | number | ❌ | 4 | |
| 14 | `processedDate` | Processed Date | date | ❌ | 6 | |
| 15 | `settledDate` | Settled Date | date | ❌ | 6 | |
| 16 | `depositDate` | Deposit Date | date | ❌ | 6 | |
| 17 | `scheduledDate` | Scheduled Date | date | ❌ | 6 | |
| 18 | `accountId` | Account ID | number | ❌ | 6 | |
| 19 | `originalPaymentId` | Original Payment ID | number | ❌ | 6 | |

### Tab 3: "Payment Details" (11 fields)

| # | fieldName | fieldLabel | fieldType | isRequired | gridSize | options | notes |
|---|-----------|-----------|-----------|-----------|----------|---------|-------|
| 20 | `cardBrand` | Card Brand | text | ❌ | 6 | | Card section |
| 21 | `cardLast4` | Card Last 4 | text | ❌ | 6 | | maxLength=4 |
| 22 | `cardExpMonth` | Exp Month | number | ❌ | 4 | | |
| 23 | `cardExpYear` | Exp Year | number | ❌ | 4 | | |
| 24 | `cardholderName` | Cardholder Name | text | ❌ | 4 | | |
| 25 | `bankName` | Bank Name | text | ❌ | 4 | | Bank section |
| 26 | `accountLast4` | Account Last 4 | text | ❌ | 4 | | maxLength=4 |
| 27 | `accountType` | Account Type | select | ❌ | 4 | `checking,savings,business` | |
| 28 | `gateway` | Gateway | text | ❌ | 6 | | Gateway section |
| 29 | `gatewayResponseCode` | Response Code | text | ❌ | 6 | | |
| 30 | `internalNotes` | Internal Notes | textarea | ❌ | 12 | | |

**Total Payment fields: 30**

---

## 7. INTERACTIONS (⚠️ No Create/Edit Form)

**Source:** `InteractionsPage.tsx`  
**Module Name:** `"Interactions"` (proposed)

**IMPORTANT:** InteractionsPage does NOT have a standard create/edit dialog. The page displays interactions in a read-only details view with action dialogs (Link, Note, Tag, Create Contact, Create Service Request). Interactions are typically ingested via API, not created through the UI form.

**Recommendation:** Skip seeding for now. If a DynamicEntityForm is needed in the future, the `Interaction` interface fields are:

<details>
<summary>Potential fields for future form (click to expand)</summary>

| fieldName | fieldLabel | fieldType | gridSize | options |
|-----------|-----------|-----------|----------|---------|
| `interactionType` | Type | select | 6 | `0:Email,1:Phone Call,2:Meeting,3:Note,4:Task,5:SMS,6:WhatsApp,7:Social Media,8:Live Chat,9:Video Call,10:Webinar,11:In Person,12:Letter,13:Fax,14:Support Ticket,15:Other` |
| `direction` | Direction | select | 6 | `0:Inbound,1:Outbound,2:Internal` |
| `subject` | Subject | text | 12 | |
| `description` | Description | textarea | 12 | |
| `interactionDate` | Date | datetime | 6 | |
| `endTime` | End Time | datetime | 6 | |
| `durationMinutes` | Duration (min) | number | 6 | |
| `outcome` | Outcome | select | 6 | `0:Completed,1:No Answer,2:Left Voicemail,3:Busy,4:Wrong Number,5:Callback Requested,6:Interested,7:Not Interested` |
| `sentiment` | Sentiment | number | 6 | |
| `priority` | Priority | number | 6 | |
| `isCompleted` | Completed | checkbox | 6 | |
| `isPrivate` | Private | checkbox | 6 | |
| `phoneNumber` | Phone Number | phone | 6 | |
| `emailAddress` | Email Address | email | 6 | |
| `location` | Location | text | 6 | |
| `meetingLink` | Meeting Link | url | 6 | |
| `followUpDate` | Follow-up Date | date | 6 | |
| `followUpNotes` | Follow-up Notes | textarea | 12 | |
| `tags` | Tags | text | 12 | |
| `category` | Category | text | 6 | |
| `accountId` | Account | lookup | 6 | |
| `contactId` | Contact | lookup | 6 | |
| `opportunityId` | Opportunity | lookup | 6 | |
| `assignedToUserId` | Assigned To | lookup | 6 | |
| `scheduledDate` | Scheduled Date | date | 6 | |
| `completedDate` | Completed Date | date | 6 | |

</details>

---

## Implementation Checklist

### Step 1: Update `ModuleNames` class
- [ ] Add `Orders`, `Invoices`, `Contracts`, `ServiceRequests`, `Payments` constants
- [ ] Update `All` array to include new modules

### Step 2: Add `GetDefault*Fields()` methods to `ModuleFieldConfigurationService`
- [ ] `GetDefaultQuoteFields()` — **Replace** existing 10-field version with 41-field version
- [ ] `GetDefaultOrderFields()` — New method (33 fields)
- [ ] `GetDefaultInvoiceFields()` — New method (37 fields)
- [ ] `GetDefaultContractFields()` — New method (31 fields)
- [ ] `GetDefaultServiceRequestFields()` — New method (24 fields)
- [ ] `GetDefaultPaymentFields()` — New method (30 fields)

### Step 3: Update `GetDefaultFieldsForModule()` switch
- [ ] Add cases for `ModuleNames.Orders`, `ModuleNames.Invoices`, `ModuleNames.Contracts`, `ModuleNames.ServiceRequests`, `ModuleNames.Payments`

### Step 4: Force reseed
- [ ] Call `ForceReseedModuleFieldConfigurationsAsync()` to regenerate all configs

### Step 5: Migrate frontend pages to `DynamicEntityForm`
- [ ] OrdersPage → `DynamicEntityForm moduleName="Orders"`
- [ ] InvoicesPage → `DynamicEntityForm moduleName="Invoices"`
- [ ] ContractsPage → `DynamicEntityForm moduleName="Contracts"`
- [ ] ServiceRequestsPage → `DynamicEntityForm moduleName="ServiceRequests"`
- [ ] PaymentsPage → `DynamicEntityForm moduleName="Payments"`
- [ ] QuotesPage → `DynamicEntityForm moduleName="Quotes"` (after seed expansion)

### Step 6: Update tests
- [ ] Update `CoreDataSeederServiceTests` to verify new module counts
- [ ] Add field count assertions for each new module

---

## Field Type Reference

| FieldType | MUI Component | Notes |
|-----------|--------------|-------|
| `text` | `TextField` | Standard text input |
| `email` | `TextField type="email"` | Email validation |
| `phone` | `TextField` | Phone number |
| `number` | `TextField type="number"` | Numeric input |
| `currency` | `TextField` with `$` adornment | Monetary values |
| `date` | `TextField type="date"` | Date picker |
| `datetime` | `TextField type="datetime-local"` | Datetime picker |
| `select` | `Select` + `MenuItem` | Dropdown; options comma-separated |
| `multiselect` | Multi-select | Multiple options |
| `checkbox` | `Switch` or `Checkbox` | Boolean toggle |
| `textarea` | `TextField multiline` | Multi-line text |
| `url` | `TextField type="url"` | URL validation |
| `lookup` | `EntitySelect` | FK reference to another entity |
