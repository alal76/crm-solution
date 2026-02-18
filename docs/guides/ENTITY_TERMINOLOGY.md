# CRM Entity Terminology Guide

This document clarifies the terminology used in the CRM Solution, particularly around commonly confused terms.

## Core Terminology

### Customer vs Account vs Company

| Term | Definition | Usage in CRM |
|------|------------|--------------|
| **Customer** | The primary entity representing who you do business with | Used throughout the system as the main entity |
| **Company** | Synonym for Organization-type Customer | `Customer.Category = Organization` |
| **Account** | A billing/subscription record tied to a Customer | Separate entity for contract management |

### Customer Entity

The `Customer` entity is the central entity representing who you sell to and support. It can be:

- **Individual** (`Category = 0`): A person (B2C)
- **Organization** (`Category = 1`): A company (B2B)

**Key fields:**
- `FirstName`, `LastName` (for Individuals)
- `Company`, `LegalName` (for Organizations)
- `CustomerType`: Prospect, Small Business, Mid-Market, Enterprise, Strategic
- `LifecycleStage`: Prospect → Lead → Opportunity → Customer → Churned

### Account Entity

The `Account` entity represents a **billing/subscription record** for a Customer. This is where you track:

- Contract terms (start date, end date)
- Billing frequency (Monthly, Quarterly, Yearly)
- Financial metrics (MRR, ARR)
- Contract documents

**Key fields:**
- `AccountNumber`: Unique billing identifier
- `CustomerId`: Links to the Customer
- `ProductId`: Which product they're subscribed to
- `MRR`, `ARR`: Monthly/Annual Recurring Revenue
- `ContractStartDate`, `ContractEndDate`

**One Customer can have multiple Accounts** (e.g., one for CRM subscription, one for support plan).

### Subscription Entity

The `Subscription` entity is a **more comprehensive subscription management** system that includes:

- Full subscription lifecycle management
- Billing automation
- Proration calculations
- Cancellation handling
- Pause/resume functionality

**Relationship to Account:**
- `Account` is simpler, focused on contract tracking
- `Subscription` is more advanced, for SaaS billing automation
- Choose based on your use case:
  - Simple contracts → Use `Account`
  - Complex SaaS billing → Use `Subscription`

## Comparison with Salesforce Terminology

| Salesforce Term | CRM Solution Equivalent |
|-----------------|------------------------|
| Account | Customer (Organization) |
| Contact | Contact |
| Lead | Lead |
| Opportunity | Opportunity |
| Contract | Account |
| Quote | Quote |
| Case | ServiceRequest |

## Field Naming Conventions

### Ownership Fields

| Field | Purpose | Used In |
|-------|---------|---------|
| `OwnerId` | Primary owner/sales rep | Customer, Lead, Opportunity |
| `AssignedToId` | Currently assigned user | ServiceRequest, Task |
| `AccountManagerId` | Account management owner | Customer |
| `CreatedById` | User who created record | All entities (via base class) |

### Address Fields

The standard address field names are:
- `Address` (street address)
- `City`
- `State`
- `PostalCode` (not ZipCode or Zip)
- `Country`

For billing addresses, use the `Billing` prefix:
- `BillingAddress`, `BillingCity`, `BillingState`, `BillingPostalCode`, `BillingCountry`

### Financial Fields

| Field | Format | Example |
|-------|--------|---------|
| `Amount` | Decimal | 50000.00 |
| `Currency` | 3-letter code | "USD", "EUR" |
| `MRR` | Monthly Recurring Revenue | 999.00 |
| `ARR` | Annual Recurring Revenue | 11988.00 |

## Data Model Diagram

```
┌─────────────────┐       ┌─────────────────┐
│    Customer     │       │     Product     │
│ (Company/Person)│       │                 │
├─────────────────┤       ├─────────────────┤
│ Id              │       │ Id              │
│ Category        │       │ Name            │
│ Company         │       │ SKU             │
│ FirstName       │       │ Price           │
│ LastName        │       │ ProductType     │
│ CustomerType    │       │ BillingFrequency│
│ Industry        │       └────────┬────────┘
│ AnnualRevenue   │                │
└────────┬────────┘                │
         │                         │
         │ 1:M                     │ 1:M
         ▼                         ▼
┌─────────────────┐       ┌─────────────────┐
│    Contact      │       │    Account      │
├─────────────────┤       │ (Billing Record)│
│ Id              │       ├─────────────────┤
│ FirstName       │       │ Id              │
│ LastName        │       │ AccountNumber   │
│ Email           │       │ CustomerId ─────┼──► Customer
│ Title           │       │ ProductId ──────┼──► Product
│ CustomerId      │       │ MRR, ARR        │
│ IsPrimary       │       │ ContractStartDate│
└─────────────────┘       │ ContractEndDate │
                          │ BillingFrequency│
                          └─────────────────┘
```

## API Endpoint Conventions

| Entity | Endpoint | Notes |
|--------|----------|-------|
| Customer | `/api/customers` | Primary customer CRUD |
| Contact | `/api/contacts` | Contacts linked to customers |
| Account | `/api/accounts` | Billing/contract records |
| Lead | `/api/leads` | Sales leads |
| Opportunity | `/api/opportunities` | Sales opportunities |

## Common Mistakes to Avoid

1. **Don't confuse Customer with Account**
   - Customer = who you're selling to
   - Account = their billing/subscription record

2. **Don't use "Account" when you mean "Customer"**
   - Say "Customer ABC" not "Account ABC"
   - Use "Account" only for billing contexts

3. **Don't mix address field names**
   - Use `PostalCode`, not `ZipCode`
   - Be consistent with `Billing*` prefix for billing addresses

4. **Don't confuse ownership fields**
   - `OwnerId` = sales owner
   - `AssignedToId` = current assignee (can change)

## License

Copyright (C) 2024-2026 Abhishek Lal  
Source-available — Commercial use requires a license. See LICENSE file.
