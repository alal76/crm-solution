# Design Documentation

> **Last Updated:** February 1, 2026 | **Version:** 1.7.28

This document covers UI/UX design patterns, data models, entity relationships, and system workflows.

---

## Table of Contents

1. [Data Model Design](#1-data-model-design)
2. [Entity Relationship Diagrams](#2-entity-relationship-diagrams)
3. [UI/UX Patterns](#3-uiux-patterns)
4. [System Workflows](#4-system-workflows)
5. [State Management Design](#5-state-management-design)

---

## 1. Data Model Design

### 1.1 Entity Categories

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           ENTITY HIERARCHY                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────┐                                                    │
│  │   BaseEntity    │  ← All entities inherit from this                  │
│  │  - Id           │                                                    │
│  │  - CreatedAt    │                                                    │
│  │  - ModifiedAt   │                                                    │
│  │  - IsActive     │                                                    │
│  └────────┬────────┘                                                    │
│           │                                                              │
│  ┌────────┴────────────────────────────────────────────────────────┐    │
│  │                          CORE ENTITIES                           │    │
│  │                                                                  │    │
│  │  Account ─────────────────────────────────────────────────────┐ │    │
│  │    └── AccountContact (junction) ────────── ContactDetail ────┤ │    │
│  │                                                                │ │    │
│  │  User ────────────────────────────────────────────────────────┤ │    │
│  │    └── UserGroupMember (junction) ────────── UserGroup ───────┤ │    │
│  │                                                                │ │    │
│  │  Lead ────────────────────────────────────────────────────────┤ │    │
│  │    └── LeadRoutingRule                                        │ │    │
│  │                                                                │ │    │
│  │  Opportunity ─────────────────────────────────────────────────┤ │    │
│  │    └── Quote ──────── QuoteLineItem ──────── Product ─────────┘ │    │
│  └──────────────────────────────────────────────────────────────────┘    │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    CONTACT INFO ENTITIES                          │   │
│  │                                                                   │   │
│  │  EmailAddress ─────── EntityEmailLink ─────┬─────────────────────│   │
│  │  PhoneNumber ──────── EntityPhoneLink ─────┤  (linked to any     │   │
│  │  Address ──────────── EntityAddressLink ───┘   entity by type)   │   │
│  └───────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Core Entity Fields

#### BaseEntity (inherited by all)

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key, auto-increment |
| CreatedAt | DateTime | Record creation timestamp |
| ModifiedAt | DateTime? | Last modification timestamp |
| CreatedBy | int? | User who created |
| ModifiedBy | int? | User who last modified |
| IsActive | bool | Soft delete flag |

### 1.3 Key Entities

#### Account (Customer)

| Field | Type | Description |
|-------|------|-------------|
| AccountNumber | string | Unique identifier |
| CompanyName | string | Business name |
| AccountType | enum | Customer, Prospect, Partner |
| Industry | string | Industry sector |
| AnnualRevenue | decimal? | Revenue amount |
| EmployeeCount | int? | Number of employees |
| Website | string | Company website |
| OwnerId | int | Assigned sales rep |

#### Contact

| Field | Type | Description |
|-------|------|-------------|
| FirstName | string | First name |
| LastName | string | Last name |
| Title | string | Job title |
| Department | string | Department |
| IsPrimary | bool | Primary contact flag |

#### Lead

| Field | Type | Description |
|-------|------|-------------|
| FirstName | string | First name |
| LastName | string | Last name |
| Company | string | Company name |
| Email | string | Email address |
| Phone | string | Phone number |
| Status | enum | New, Contacted, Qualified, Converted, Lost |
| LeadScore | int | Qualification score |
| Source | string | Lead source |

#### Opportunity

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Opportunity name |
| AccountId | int | Related account |
| Stage | enum | Pipeline stage |
| Amount | decimal | Deal value |
| Probability | int | Win probability % |
| CloseDate | DateTime | Expected close |
| OwnerId | int | Assigned rep |

---

## 2. Entity Relationship Diagrams

### 2.1 Customer-Contact Relationships

```
┌───────────────────┐        ┌──────────────────┐        ┌─────────────────┐
│     Account       │        │  AccountContact  │        │  ContactDetail  │
├───────────────────┤        ├──────────────────┤        ├─────────────────┤
│ Id          (PK)  │◄──────┤│ AccountId   (FK) │        │ Id        (PK)  │
│ AccountNumber     │        │ ContactId   (FK) │───────►│ FirstName       │
│ CompanyName       │        │ IsPrimary        │        │ LastName        │
│ Industry          │        │ Role             │        │ Title           │
│ OwnerId      (FK) │        └──────────────────┘        │ Department      │
└───────────────────┘                                    └─────────────────┘
         │                                                        │
         │                                                        │
         ▼                                                        ▼
┌───────────────────────────────────────────────────────────────────────────┐
│                         CONSOLIDATED CONTACT INFO                          │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  EntityEmailLink          EntityPhoneLink         EntityAddressLink        │
│  ├─ EntityType (Account)  ├─ EntityType           ├─ EntityType            │
│  ├─ EntityId              ├─ EntityId             ├─ EntityId              │
│  ├─ EmailAddressId ──┐    ├─ PhoneNumberId ──┐    ├─ AddressId ──┐         │
│  └─ IsPrimary        │    └─ IsPrimary       │    └─ IsPrimary   │         │
│                      │                       │                    │         │
│                      ▼                       ▼                    ▼         │
│              EmailAddress            PhoneNumber              Address       │
│              ├─ Email                ├─ Number                ├─ Street1    │
│              └─ IsVerified           └─ Type                  ├─ City       │
│                                                               └─ Country    │
└───────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Sales Pipeline Relationships

```
┌─────────────┐         ┌───────────────────┐         ┌─────────────┐
│    Lead     │         │    Opportunity    │         │   Account   │
├─────────────┤         ├───────────────────┤         ├─────────────┤
│ Id     (PK) │──►──►───│ ConvertedFromId   │         │ Id     (PK) │
│ Status      │ convert │ AccountId    (FK) │────────►│             │
│ LeadScore   │         │ Stage             │         │             │
│ Source      │         │ Amount            │         │             │
└─────────────┘         │ Probability       │         └─────────────┘
                        │ CloseDate         │
                        └─────────┬─────────┘
                                  │
                                  ▼
                        ┌─────────────────────┐
                        │       Quote         │
                        ├─────────────────────┤
                        │ Id           (PK)   │
                        │ OpportunityId (FK)  │
                        │ QuoteNumber         │
                        │ TotalAmount         │
                        │ Status              │
                        └─────────┬───────────┘
                                  │
                                  ▼
                        ┌─────────────────────┐       ┌───────────────┐
                        │   QuoteLineItem     │       │   Product     │
                        ├─────────────────────┤       ├───────────────┤
                        │ QuoteId      (FK)   │       │ Id       (PK) │
                        │ ProductId    (FK)   │──────►│ Name          │
                        │ Quantity            │       │ Price         │
                        │ UnitPrice           │       │ SKU           │
                        │ Discount            │       │               │
                        └─────────────────────┘       └───────────────┘
```

### 2.3 User and Security Relationships

```
┌──────────────────────────────────────────────────────────────────────────┐
│                           USER & SECURITY MODEL                           │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│  ┌───────────────┐       ┌──────────────────┐       ┌─────────────────┐  │
│  │     User      │       │ UserGroupMember  │       │   UserGroup     │  │
│  ├───────────────┤       ├──────────────────┤       ├─────────────────┤  │
│  │ Id       (PK) │◄─────┤│ UserId      (FK) │       │ Id         (PK) │  │
│  │ Username      │       │ UserGroupId (FK) │──────►│ Name            │  │
│  │ Email         │       │ JoinedAt         │       │ Description     │  │
│  │ PasswordHash  │       └──────────────────┘       │                 │  │
│  │ RoleId   (FK) │                                  │ PasswordExpDays │  │
│  │ IsActive      │                                  │ RequireTwoFactor│  │
│  │               │                                  │ EnforceTwoFactor│  │
│  │ MustResetPwd  │                                  └─────────────────┘  │
│  │ PwdNeverSet   │                                                       │
│  │ TwoFactorKey  │                                                       │
│  └───────┬───────┘                                                       │
│          │                                                                │
│          ▼                                                                │
│  ┌───────────────┐       ┌──────────────────┐                            │
│  │     Role      │       │    Permission    │                            │
│  ├───────────────┤       ├──────────────────┤                            │
│  │ Id       (PK) │──────►│ RoleId      (FK) │                            │
│  │ Name          │       │ Resource         │                            │
│  │ Description   │       │ Action (CRUD)    │                            │
│  └───────────────┘       └──────────────────┘                            │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 3. UI/UX Patterns

### 3.1 Page Layout Structure

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              APP BAR                                     │
│  Logo  │  Module Nav  │                              │ Search │ Profile │
├────────┴───────────────────────────────────────────────────────┴────────┤
│        │                                                                 │
│        │                                                                 │
│        │                     CONTENT AREA                                │
│  SIDE  │                                                                 │
│  NAV   │     ┌─────────────────────────────────────────────────┐        │
│        │     │  Page Header                                     │        │
│        │     │  Title           │          Actions/Buttons      │        │
│        │     ├──────────────────────────────────────────────────┤        │
│        │     │                                                   │        │
│        │     │               MAIN CONTENT                        │        │
│        │     │                                                   │        │
│        │     │  (DataGrid / Form / Detail View / Dashboard)      │        │
│        │     │                                                   │        │
│        │     └───────────────────────────────────────────────────┘        │
│        │                                                                 │
└────────┴─────────────────────────────────────────────────────────────────┘
```

### 3.2 Component Patterns

#### List Page Pattern

```typescript
// Standard list page structure
const ListPage = () => {
  const [data, setData] = useState<Entity[]>([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState<Filters>({});
  
  useEffect(() => {
    fetchData(filters);
  }, [filters]);
  
  return (
    <PageContainer>
      <PageHeader
        title="Entities"
        actions={<CreateButton />}
      />
      <FilterBar filters={filters} onChange={setFilters} />
      <DataGrid
        data={data}
        columns={columns}
        loading={loading}
        onRowClick={navigateToDetail}
      />
    </PageContainer>
  );
};
```

#### Detail Page Pattern

```typescript
// Standard detail page structure
const DetailPage = ({ id }: { id: string }) => {
  const { data, loading } = useEntity(id);
  
  return (
    <PageContainer>
      <PageHeader
        title={data?.name}
        breadcrumbs={[{ label: 'List', href: '/entities' }]}
        actions={<EditButton />}
      />
      <TabContainer>
        <Tab label="Details"><DetailsTab data={data} /></Tab>
        <Tab label="Activities"><ActivitiesTab entityId={id} /></Tab>
        <Tab label="Notes"><NotesTab entityId={id} /></Tab>
      </TabContainer>
    </PageContainer>
  );
};
```

### 3.3 Form Patterns

#### Standard Form Structure

```typescript
const EntityForm = ({ entity, onSubmit }: Props) => {
  const { control, handleSubmit, formState } = useForm({
    defaultValues: entity ?? defaultValues,
    resolver: yupResolver(validationSchema),
  });
  
  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <FormSection title="Basic Information">
        <FormField name="name" control={control} label="Name" required />
        <FormField name="email" control={control} label="Email" type="email" />
      </FormSection>
      
      <FormSection title="Additional Details">
        <FormField name="description" control={control} multiline rows={4} />
      </FormSection>
      
      <FormActions>
        <CancelButton />
        <SubmitButton loading={formState.isSubmitting} />
      </FormActions>
    </form>
  );
};
```

### 3.4 Theme System

#### Color Palette

| Token | Light Theme | Dark Theme | Usage |
|-------|-------------|------------|-------|
| primary.main | #1976d2 | #90caf9 | Buttons, links |
| secondary.main | #dc004e | #f48fb1 | Accents |
| error.main | #f44336 | #f44336 | Errors, delete |
| warning.main | #ff9800 | #ff9800 | Warnings |
| success.main | #4caf50 | #4caf50 | Success states |
| background.default | #ffffff | #121212 | Page background |
| background.paper | #f5f5f5 | #1e1e1e | Cards, dialogs |

---

## 4. System Workflows

### 4.1 Lead Conversion Workflow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         LEAD CONVERSION FLOW                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────┐     ┌──────────────┐     ┌─────────────┐                  │
│  │   NEW    │────►│  CONTACTED   │────►│  QUALIFIED  │                  │
│  │   LEAD   │     │              │     │             │                  │
│  └──────────┘     └──────────────┘     └──────┬──────┘                  │
│                                               │                          │
│                          ┌────────────────────┴────────────────────┐    │
│                          │                                          │    │
│                          ▼                                          ▼    │
│                   ┌─────────────┐                            ┌──────────┐│
│                   │   CONVERT   │                            │   LOST   ││
│                   └──────┬──────┘                            └──────────┘│
│                          │                                               │
│        ┌─────────────────┼─────────────────┐                            │
│        │                 │                 │                             │
│        ▼                 ▼                 ▼                             │
│  ┌───────────┐    ┌───────────┐    ┌─────────────┐                      │
│  │  CREATE   │    │  CREATE   │    │   CREATE    │                      │
│  │  ACCOUNT  │    │  CONTACT  │    │ OPPORTUNITY │                      │
│  └───────────┘    └───────────┘    └─────────────┘                      │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Quote Approval Workflow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          QUOTE APPROVAL FLOW                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌───────────┐                                                          │
│  │   DRAFT   │ ◄─────────────────────────────────────────────┐          │
│  └─────┬─────┘                                               │          │
│        │ Submit                                              │          │
│        ▼                                                     │          │
│  ┌─────────────┐     ┌──────────────┐     ┌───────────────┐ │          │
│  │   PENDING   │────►│   REJECTED   │────►│   REVISION    │─┘          │
│  │   APPROVAL  │     │              │     │   REQUIRED    │            │
│  └──────┬──────┘     └──────────────┘     └───────────────┘            │
│         │                                                               │
│         │ Approved                                                      │
│         ▼                                                               │
│  ┌─────────────┐     ┌──────────────┐     ┌───────────────┐            │
│  │  APPROVED   │────►│    SENT      │────►│   ACCEPTED    │            │
│  │             │     │              │     │               │            │
│  └─────────────┘     └──────────────┘     └───────────────┘            │
│                              │                                          │
│                              ▼                                          │
│                      ┌──────────────┐                                   │
│                      │   DECLINED   │                                   │
│                      │              │                                   │
│                      └──────────────┘                                   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Service Request Workflow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                       SERVICE REQUEST FLOW                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────┐     ┌───────────────┐     ┌───────────────┐               │
│  │   NEW    │────►│   ASSIGNED    │────►│  IN PROGRESS  │               │
│  │  TICKET  │     │               │     │               │               │
│  └──────────┘     └───────────────┘     └───────┬───────┘               │
│                                                  │                       │
│                   ┌──────────────────────────────┤                       │
│                   │                              │                       │
│                   ▼                              ▼                       │
│           ┌───────────────┐              ┌───────────────┐              │
│           │   PENDING     │              │   RESOLVED    │              │
│           │   CUSTOMER    │              │               │              │
│           └───────┬───────┘              └───────┬───────┘              │
│                   │                              │                       │
│                   │ Customer responds            │ Customer confirms     │
│                   │                              │                       │
│                   └──────────────►───────────────┼──────────────────────►│
│                                                  ▼                       │
│                                          ┌───────────────┐              │
│                                          │    CLOSED     │              │
│                                          │               │              │
│                                          └───────────────┘              │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. State Management Design

### 5.1 Frontend State Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        STATE MANAGEMENT LAYERS                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                      GLOBAL STATE (Context)                          ││
│  │  ┌──────────────┐  ┌─────────────┐  ┌───────────────┐              ││
│  │  │ AuthContext  │  │ThemeContext │  │ SignalRContext│              ││
│  │  │ - user       │  │ - mode      │  │ - connection  │              ││
│  │  │ - token      │  │ - toggle()  │  │ - subscribe() │              ││
│  │  │ - login()    │  │             │  │ - emit()      │              ││
│  │  │ - logout()   │  │             │  │               │              ││
│  │  └──────────────┘  └─────────────┘  └───────────────┘              ││
│  └─────────────────────────────────────────────────────────────────────┘│
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                      LOCAL STATE (Component)                         ││
│  │  - useState for UI state (modals, selections)                       ││
│  │  - useReducer for complex form state                                ││
│  │  - useRef for mutable values (timers, DOM refs)                     ││
│  └─────────────────────────────────────────────────────────────────────┘│
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                     SERVER STATE (API Cache)                         ││
│  │  - Service functions for API calls                                  ││
│  │  - useEffect for data fetching                                      ││
│  │  - Local state for caching responses                                ││
│  └─────────────────────────────────────────────────────────────────────┘│
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Backend State Flow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         REQUEST STATE FLOW                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  HTTP Request                                                            │
│       │                                                                  │
│       ▼                                                                  │
│  ┌────────────────┐                                                     │
│  │   Middleware   │  JWT Validation, Request Logging                    │
│  └───────┬────────┘                                                     │
│          │                                                               │
│          ▼                                                               │
│  ┌────────────────┐                                                     │
│  │   Controller   │  Request/Response DTOs, Validation                  │
│  └───────┬────────┘                                                     │
│          │                                                               │
│          ▼                                                               │
│  ┌────────────────┐                                                     │
│  │    Service     │  Business Logic, Domain Rules                       │
│  └───────┬────────┘                                                     │
│          │                                                               │
│          ▼                                                               │
│  ┌────────────────┐                                                     │
│  │   Repository   │  Data Access, Queries                               │
│  └───────┬────────┘                                                     │
│          │                                                               │
│          ▼                                                               │
│  ┌────────────────┐                                                     │
│  │   DbContext    │  Entity Framework, Unit of Work                     │
│  └───────┬────────┘                                                     │
│          │                                                               │
│          ▼                                                               │
│      Database                                                            │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Design Resources

- **Figma**: Design mockups (if available)
- **Material-UI**: [https://mui.com/material-ui/](https://mui.com/material-ui/)
- **Design System**: See [06-standards/README.md](../06-standards/README.md)
