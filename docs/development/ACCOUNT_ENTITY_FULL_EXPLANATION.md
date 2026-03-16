# CRM Solution - Account Entity: Complete Architecture & Implementation Guide

> **Last Updated:** March 2026  
> **Scope:** Backend Account Entity + Frontend Implementation  
> **Audience:** Developers, Architects, New Team Members

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Backend Architecture](#backend-architecture)
3. [Frontend Architecture](#frontend-architecture)
4. [Complete Data Flow](#complete-data-flow)
5. [Entity-to-Entity Relationships](#entity-to-entity-relationships)
6. [API Endpoints Reference](#api-endpoints-reference)
7. [Frontend Components & Pages](#frontend-components--pages)
8. [Common Workflows](#common-workflows)
9. [Implementation Patterns](#implementation-patterns)

---

## Executive Summary

### What is an Account?

An **Account** in the CRM system represents either:
- **Individual**: A person (with contact information)
- **Organization**: A company or business entity (with multiple contacts)

The system uses **hexagonal architecture** to separate business logic from data access, allowing the same Account entity to be accessed through multiple interfaces.

### Key Terms (Legacy → Current)

| Legacy Name | Current Name | Purpose |
|------------|--------------|---------|
| `Customer` | `Account` | Individual or Organization contact record |
| Database table | `Customers` | Remains `Customers` for backward compatibility |
| API endpoint | `/api/accounts` | REST API for account operations |
| Enum value | `AccountLifecycleStage` | Account status (Lead → Active → Churned) |

### Diagram: Account Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    ACCOUNT ENTITY                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Category: Individual OR Organization                            │
│                    │                                             │
│         ┌──────────┴──────────┐                                  │
│         ▼                     ▼                                  │
│    INDIVIDUAL               ORGANIZATION                         │
│    ─────────                ──────────                           │
│  • FirstName              • Company Name                        │
│  • LastName               • LegalName                           │
│  • Email                  • TaxId                               │
│  • Phone                  • Registrations                       │
│  • JobTitle               • Primary Contact ID                  │
│                           • Employees Count                     │
│                                                                  │
│  COMMON FIELDS (Both):                                          │
│  • Address (Billing & Shipping)                                │
│  • City, State, Country, ZipCode                               │
│  • Industry, AnnualRevenue                                      │
│  • Status (LifecycleStage): Lead → Active → Churned           │
│  • Priority: Low → Medium → High → Critical                    │
│  • AssignedToUser (Account Manager)                            │
│  • Tags, Notes, CustomFields                                   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Backend Architecture

### 1. Entity Definition (`Account.cs`)

The Account entity is defined in: `CRM.Backend/src/CRM.Core/Entities/Account.cs`

#### Class Declaration

```csharp
[Table("Customers")] // Backward compatibility - table name stays Customers
public class Account : BaseEntity
{
    // Inherits from BaseEntity:
    // - Id (int)
    // - CreatedAt (DateTime)
    // - UpdatedAt (DateTime)
    // - IsDeleted (bool) - Soft delete flag
    // - RowVersion (byte[]) - Optimistic concurrency
}
```

#### Key Property Groups

##### 1️⃣ Category & Type

```csharp
public enum AccountCategory
{
    Individual = 0,      // Person account
    Organization = 1     // Company account
}

public enum AccountType
{
    Individual = 0,
    SmallBusiness = 1,
    MidMarket = 2,
    Enterprise = 3,
    Government = 4,
    NonProfit = 5
}

public enum AccountLifecycleStage
{
    Other = 0,            // Default
    Lead = 1,            // Prospect, showing interest
    Opportunity = 2,     // Qualified with active deal
    Active = 3,          // Current paying customer
    AtRisk = 4,          // At risk of churning
    Churned = 5,         // Former customer
    WinBack = 6          // Re-engagement attempt
}

public AccountCategory Category { get; set; } = AccountCategory.Individual;
public AccountType AccountType { get; set; }
public AccountLifecycleStage LifecycleStage { get; set; }
public AccountPriority Priority { get; set; } = AccountPriority.Medium;
```

##### 2️⃣ Individual Account Fields

```csharp
[MaxLength(100)]
public string FirstName { get; set; } = string.Empty;

[MaxLength(100)]
public string LastName { get; set; } = string.Empty;

[MaxLength(20)]
public string? Salutation { get; set; }  // Mr., Mrs., Dr., etc.

[MaxLength(20)]
public string? Suffix { get; set; }      // Jr., Sr., III, etc.

public DateTime? DateOfBirth { get; set; }

[MaxLength(20)]
public string? Gender { get; set; }

public int? LinkedContactId { get; set; }  // Optional: Link to Contact entity
```

**When to Use Individual:**
- Single person records
- Freelancers/Independent contractors
- Personal contacts

**Note:** Individual accounts CAN be linked to a Contact record (1:1 optional relationship)

##### 3️⃣ Organization Account Fields

```csharp
[MaxLength(255)]
public string Company { get; set; } = string.Empty;  // Primary name

[MaxLength(255)]
public string? LegalName { get; set; }   // Registered name

[MaxLength(255)]
public string? DbaName { get; set; }     // Doing Business As

[MaxLength(50)]
public string? TaxId { get; set; }       // Tax ID / EIN / VAT

[MaxLength(50)]
public string? RegistrationNumber { get; set; }

public int? YearFounded { get; set; }

public int? PrimaryContactId { get; set; }  // From AccountContacts junction
```

**When to Use Organization:**
- Companies
- Departments within a company
- Partners
- Vendors

**Note:** Organizations CAN have multiple Contacts via the `AccountContacts` junction table

##### 4️⃣ Contact Information (Both Types)

```csharp
[Required]
[MaxLength(255)]
[EmailAddress]
public string Email { get; set; } = string.Empty;

[MaxLength(255)]
[EmailAddress]
public string? SecondaryEmail { get; set; }

[Required]
[MaxLength(30)]
[Phone]
public string Phone { get; set; } = string.Empty;

[MaxLength(30)]
[Phone]
public string? MobilePhone { get; set; }

[MaxLength(30)]
public string? FaxNumber { get; set; }

[MaxLength(100)]
public string? JobTitle { get; set; }    // Job title (individual) or position

[MaxLength(500)]
[Url]
public string? Website { get; set; }
```

##### 5️⃣ Address - Billing (Required)

```csharp
[Required]
[MaxLength(255)]
public string Address { get; set; } = string.Empty;

[MaxLength(255)]
public string? Address2 { get; set; }

[Required]
[MaxLength(100)]
public string City { get; set; } = string.Empty;

[Required]
[MaxLength(100)]
public string State { get; set; } = string.Empty;

[Required]
[MaxLength(20)]
public string ZipCode { get; set; } = string.Empty;

[Required]
[MaxLength(100)]
public string Country { get; set; } = string.Empty;
```

##### 6️⃣ Address - Shipping (Optional)

```csharp
[MaxLength(255)]
public string? ShippingAddress { get; set; }

[MaxLength(255)]
public string? ShippingAddress2 { get; set; }

[MaxLength(100)]
public string? ShippingCity { get; set; }

[MaxLength(100)]
public string? ShippingState { get; set; }

[MaxLength(20)]
public string? ShippingZipCode { get; set; }

[MaxLength(100)]
public string? ShippingCountry { get; set; }

public bool ShippingSameAsBilling { get; set; } = true;
```

##### 7️⃣ Business Information

```csharp
[MaxLength(100)]
public string? Industry { get; set; }      // e.g., "Technology", "Healthcare"

[MaxLength(100)]
public string? SubIndustry { get; set; }   // e.g., "SaaS", "Consulting"

public int? NumberOfEmployees { get; set; }

[MaxLength(50)]
public string? EmployeeRange { get; set; }  // "1-10", "11-50", "51-200", etc.

[Range(0, double.MaxValue)]
public decimal AnnualRevenue { get; set; } = 0;

[MaxLength(50)]
public string? RevenueRange { get; set; }   // "$0-1M", "$1M-10M", etc.

[MaxLength(50)]
public string? Ownership { get; set; }      // "Public", "Private", "Government"

[MaxLength(20)]
public string? StockSymbol { get; set; }    // For public companies
```

##### 8️⃣ Relationship & Assignment

```csharp
public int? OwnerId { get; set; }                    // User who owns/created account

public int? AssignedToUserId { get; set; }          // Sales rep responsible for account

public int? AccountManagerId { get; set; }          // Senior account manager (if different)

[MaxLength(100)]
public string? Territory { get; set; }              // Sales territory assignment

[MaxLength(100)]
public string? Region { get; set; }                 // Geographic region

[MaxLength(100)]
public string? Segment { get; set; }                // Market segment (SMB, Enterprise, etc.)

[MaxLength(50)]
public string? LeadSource { get; set; }             // How lead was acquired

public int? ReferredByAccountId { get; set; }       // Self-reference: referred by another account

public int? ParentAccountId { get; set; }           // Self-reference: parent company (hierarchy)
```

##### 9️⃣ Customer Preferences

```csharp
public bool OptInEmail { get; set; } = true;

public bool OptInSms { get; set; } = true;

public bool OptInPhone { get; set; } = true;

[MaxLength(50)]
public string? PreferredContactMethod { get; set; }  // "Email", "Phone", "SMS"

[MaxLength(50)]
public string? Timezone { get; set; }                // "UTC", "EST", etc.

[MaxLength(20)]
public string? PreferredLanguage { get; set; }       // "en", "es", "fr", etc.

[MaxLength(10)]
public string? Currency { get; set; }                // "USD", "EUR", etc.
```

##### 🔟 Account Health & Metadata

```csharp
[Range(0, 100)]
public int CustomerHealthScore { get; set; } = 50;  // 0-100 health indicator

[MaxLength(500)]
public string? Tags { get; set; }                    // Comma-separated tags

public DateTime? FirstContactDate { get; set; }

public decimal? CreditLimit { get; set; }

[MaxLength(100)]
public string? PaymentTerms { get; set; }            // "Net 30", "Net 60", etc.

[MaxLength(500)]
public string? Notes { get; set; }

[MaxLength(500)]
public string? InternalNotes { get; set; }           // Internal-only notes

[MaxLength(1000)]
public string? Description { get; set; }             // Long-form description
```

##### Social & Web Properties

```csharp
[MaxLength(200)]
public string? LinkedInUrl { get; set; }

[MaxLength(100)]
public string? TwitterHandle { get; set; }

[MaxLength(100)]
public string? FacebookUrl { get; set; }

[MaxLength(100)]
public string? InstagramHandle { get; set; }
```

---

### 2. Data Transfer Objects (DTOs)

DTOs handle serialization between frontend and backend:

#### AccountDto (Read Response)

```csharp
public class AccountDto
{
    public int Id { get; set; }
    public int Category { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int CustomerType { get; set; }
    public int Priority { get; set; }
    public int LifecycleStage { get; set; }
    public string DisplayName { get; set; }  // Computed: "John Doe" or "Acme Corp"
    public int ContactCount { get; set; }    // Count of linked contacts
    // ... other fields
}
```

#### CreateAccountDto (Write Request)

```csharp
public class CreateAccountDto
{
    [Required]
    public int Category { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    public string? Company { get; set; }
    public int? CustomerType { get; set; }
    public int? LifecycleStage { get; set; }
    // ... other optional fields
}
```

---

### 3. Service Layer Architecture

The `AccountService` implements **hexagonal architecture** with multiple port interfaces:

#### File: `CRM.Infrastructure/Services/AccountService.cs`

```csharp
public class AccountService : 
    IAccountService,           // Primary business operations
    IAccountInputPort,         // Hexagonal input port
    ICustomerInputPort         // Legacy backward compatibility
{
    // Dependencies injected via constructor
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<AccountContact> _accountContactRepository;
    private readonly IContactsService _contactsService;
    private readonly IContactInfoService _contactInfoService;
    // ... other repositories
}
```

#### Service Methods

**CRUD Operations:**

```csharp
// Get all active accounts
public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()

// Get single account by ID
public async Task<AccountDto?> GetAccountByIdAsync(int id)

// Search by name/email/company
public async Task<IEnumerable<AccountDto>> SearchAccountsAsync(string searchTerm)

// Create new account
public async Task<AccountDto> CreateAccountAsync(CreateAccountDto dto)

// Update existing account
public async Task<AccountDto> UpdateAccountAsync(int id, UpdateAccountDto dto)

// Soft delete account
public async Task<bool> DeleteAccountAsync(int id)
```

**Relationship Operations:**

```csharp
// Get all contacts linked to an account
public async Task<IEnumerable<AccountContactDto>> GetLinkedContactsAsync(int accountId)

// Link a contact to an account
public async Task<AccountContactDto> LinkContactAsync(int accountId, int contactId, string role)

// Unlink a contact from an account
public async Task<bool> UnlinkContactAsync(int accountId, int contactId)

// Get primary contact for organization
public async Task<Contact?> GetPrimaryContactAsync(int accountId)
```

**Query Operations:**

```csharp
// Get accounts by lifecycle stage
public async Task<IEnumerable<AccountDto>> GetByLifecycleStageAsync(int stage)

// Get accounts by priority
public async Task<IEnumerable<AccountDto>> GetByPriorityAsync(int priority)

// Get accounts assigned to user
public async Task<IEnumerable<AccountDto>> GetAccountsByAssignedUserAsync(int userId)

// Get accounts in a territory
public async Task<IEnumerable<AccountDto>> GetByTerritoryAsync(string territory)
```

**Statistics & Analytics:**

```csharp
// Get customer health score
public async Task<int> CalculateHealthScoreAsync(int accountId)

// Get account statistics
public async Task<AccountStatistics> GetStatisticsAsync()
```

---

## Frontend Architecture

### 1. Frontend Service Layer

**File:** `CRM.Frontend/src/services/accountService.ts`

The frontend uses an **API service wrapper** pattern:

```typescript
const accountService = {
  // === CRUD ===
  getAll: () => apiClient.get<Account[]>('/accounts'),
  getById: (id: number) => apiClient.get<Account>(`/accounts/${id}`),
  create: (data: CreateAccountDto) => apiClient.post<Account>('/accounts', data),
  update: (id: number, data: UpdateAccountDto) => 
    apiClient.put<Account>(`/accounts/${id}`, data),
  delete: (id: number) => apiClient.delete(`/accounts/${id}`),

  // === Search & Filter ===
  search: (term: string) => 
    apiClient.get<Account[]>(`/accounts/search/${encodeURIComponent(term)}`),
  getIndividuals: () => apiClient.get<Account[]>('/accounts/individuals'),
  getOrganizations: () => apiClient.get<Account[]>('/accounts/organizations'),
  getByLifecycleStage: (stage: number) => 
    apiClient.get<Account[]>(`/accounts/by-stage/${stage}`),

  // === Contact Relationships ===
  getContacts: (accountId: number) => 
    apiClient.get(`/accounts/${accountId}/contacts`),
  linkContact: (accountId: number, contactId: number, data?: any) =>
    apiClient.post(`/accounts/${accountId}/contacts`, { contactId, ...data }),
  unlinkContact: (accountId: number, contactId: number) =>
    apiClient.delete(`/accounts/${accountId}/contacts/${contactId}`),
};

export default accountService;
```

### 2. Frontend Interfaces/Types

```typescript
export interface Account {
  id?: number;
  category?: number;           // 0=Individual, 1=Organization
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  company?: string;
  legalName?: string;
  jobTitle?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  annualRevenue?: number;
  customerType?: number;       // Business type
  priority?: number;           // 0=Low, 1=Medium, 2=High, 3=Critical
  lifecycleStage?: number;     // 0=Other, 1=Lead, 2=Opp, 3=Active, etc.
  industry?: string;
  website?: string;
  displayName?: string;        // Computed display
  [key: string]: any;          // Allow dynamic fields
}

export interface CreateAccountDto {
  category: number;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  company?: string;
  customerType?: number;
  lifecycleStage?: number;
  [key: string]: any;
}

export interface UpdateAccountDto {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  company?: string;
  [key: string]: any;
}
```

### 3. Main Accounts Page Component

**File:** `CRM.Frontend/src/pages/AccountsPage.tsx` (~1,300 lines)

This is a **feature-rich data table component** with multiple capabilities:

#### Component Structure

```tsx
const AccountsPage: React.FC = () => {
  // State Management
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedAccount, setSelectedAccount] = useState<Account | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [formData, setFormData] = useState<AccountForm>(INITIAL_FORM_DATA);
  
  // Hooks
  const { pagination, handlePageChange, handleRowsPerPageChange } = usePagination();
  const { accounts: contextAccounts } = useAccountContext();
  const profile = useProfile();
  const { data: fieldConfig } = useFieldConfig('Accounts');
  
  // Lifecycle
  useEffect(() => {
    loadAccounts();
  }, []);
  
  // Handlers
  const loadAccounts = async () => { /* ... */ };
  const handleCreate = async () => { /* ... */ };
  const handleUpdate = async () => { /* ... */ };
  const handleDelete = async (id: number) => { /* ... */ };
  
  // Render
  return (
    <Container maxWidth="xl">
      <AccountsToolbar {...} />
      <AccountsTable {...} />
    </Container>
  );
};
```

#### Toolbar Features

- **Add Account Button** - Opens create dialog
- **Search Bar** - Real-time search
- **Advanced Filters** - Lifecycle stage, priority, account type
- **Bulk Actions** - Export, import, merge duplicates
- **View Options** - Toggle between table/grid views

#### Table Columns

| Column | Type | Notes |
|--------|------|-------|
| **Display Name** | Text | "John Doe" or "Acme Corp" |
| **Email** | Link | Clickable, copies to clipboard |
| **Phone** | Link | Clickable tel: link |
| **Company** | Text | Organization name (if org account) |
| **Lifecycle Stage** | Badge | Color-coded status |
| **Priority** | Badge | Color-coded importance |
| **Owner** | Text | Account manager name |
| **Created** | Date | Creation timestamp |
| **Actions** | Buttons | Edit, Delete, View Details |

#### Dialogs

**Create/Edit Dialog:**
- Dynamic field rendering based on `fieldConfig`
- Conditional fields (Individual vs Organization)
- Validation on submit
- Form data persistence in state

**Duplicate Detection:**
- Integrates with `DuplicateDetectionDialog` component
- Scans for duplicates on create
- Merge functionality via `MergeDialog`

**Advanced Search:**
- Filters by: Type, Name, Email, Stage, Industry, City
- Multi-criteria AND logic
- Real-time filtering

#### Real-time Updates (SignalR)

```tsx
// Subscribe to account changes
const { isConnected } = useEntityTypeSubscription('Account', (message) => {
  if (message.type === 'EntityUpdated') {
    // Reload single account
    setAccounts(prev => 
      prev.map(a => a.id === message.data.id ? message.data : a)
    );
  }
});
```

### 4. Account Details Page Component

**File:** `CRM.Frontend/src/pages/AccountDetailsPage.tsx`

Shows full account information in a detailed view:

```tsx
const AccountDetailsPage: React.FC = () => {
  const { accountId } = useParams<{ accountId: string }>();
  const [account, setAccount] = useState<Account | null>(null);
  const [activeTab, setActiveTab] = useState(0);
  
  // Tabs
  // 0: Overview (all fields)
  // 1: Contacts (linked AccountContacts)
  // 2: Opportunities (opportunities linked to this account)
  // 3: Activities (timeline of interactions)
  // 4: Notes (notes attached to account)
  // 5: Custom Fields (custom field values)
  
  return (
    <Box>
      <AccountHeader account={account} />
      <Tabs value={activeTab} onChange={(e, val) => setActiveTab(val)}>
        <Tab label="Overview" />
        <Tab label="Contacts" />
        <Tab label="Opportunities" />
        <Tab label="Activities" />
        <Tab label="Notes" />
      </Tabs>
      <TabPanel value={activeTab} index={0}>
        <OverviewTab account={account} />
      </TabPanel>
      {/* ... other tabs */}
    </Box>
  );
};
```

---

## Complete Data Flow

### Scenario 1: Create New Account

```
USER ACTION (Frontend):
Click "Add Account" button
         ↓
FORM DIALOG:
Opens AccountForm dialog with initial empty state
- User fills: Category (Org), Company, Email, Phone, Address, etc.
- Click Submit
         ↓
FRONTEND VALIDATION:
- Email format ✓
- Required fields ✓
- Phone format ✓
         ↓
API CALL:
POST /api/accounts
{
  "category": 1,              // Organization
  "company": "Acme Corp",
  "email": "contact@acme.com",
  "phone": "+1-555-0100",
  "address": "123 Main St",
  "city": "San Francisco",
  "state": "CA",
  "zipCode": "94105",
  "country": "USA"
}
         ↓
BACKEND PROCESSING (AccountService):
1. Map CreateAccountDto → Account entity
2. Set timestamps: CreatedAt = now
3. Validate: Email format, required fields
4. Normalize: Trim whitespace, uppercase country
5. Persist: Insert into Customers table
6. Generate linked Address record
7. Return: AccountDto (with computed displayName)
         ↓
FRONTEND UPDATE:
- Add new account to accounts[] state
- Close dialog
- Show success toast: "Account created successfully"
- Render new row in table
         ↓
REAL-TIME (SignalR):
- Other users subscribed to Account entities
  receive EntityCreated event
- Their tables auto-update with new account
```

### Scenario 2: Link Contact to Account

```
USER ACTION (Frontend):
Open AccountDetails → Contacts tab
Click "Add Contact"
         ↓
CONTACT SELECTION:
1. Show list of existing Contacts
2. User selects: "Jane Smith (jane@example.com)"
3. Set Role: "Sales Contact"
4. Click "Link"
         ↓
API CALL:
POST /api/accounts/5/contacts
{
  "contactId": 12,
  "role": "Sales Contact",
  "isPrimaryContact": true
}
         ↓
BACKEND PROCESSING (AccountService):
1. Validate: Account exists, Contact exists
2. Create: AccountContact record
   - accountId = 5
   - contactId = 12
   - role = "Sales Contact"
   - isPrimaryContact = true
3. If isPrimaryContact: Update Account.PrimaryContactId = 12
4. Persist AccountContact to database
5. Return: AccountContactDto
         ↓
FRONTEND UPDATE:
- Reload contacts list for this account
- Show contact in "Linked Contacts" section
- Show success toast
         ↓
REAL-TIME (SignalR):
- Other users see this account's contacts update
```

### Scenario 3: Update Account Details

```
USER ACTION (Frontend):
On AccountDetailsPage
Edit: Company Name "Acme Corp" → "Acme Corporation"
Click "Save"
         ↓
OPTIMISTIC UPDATE:
- Immediately update UI (no loading state for small changes)
         ↓
API CALL:
PUT /api/accounts/5
{
  "id": 5,
  "company": "Acme Corporation",
  ... other fields ...
}
         ↓
BACKEND PROCESSING (AccountService):
1. Get existing Account entity
2. Update changed fields
3. Set: UpdatedAt = now
4. Check: RowVersion for optimistic concurrency
5. Persist to database
6. Return: Updated AccountDto
         ↓
FRONTEND UPDATE:
- If success: Keep optimistic changes
- If conflict (version mismatch):
  Show conflict dialog, reload from server
- Show success toast: "Account updated"
         ↓
REAL-TIME (SignalR):
- Other users receive EntityUpdated event
- Their AccountDetails pages refresh
```

---

## Entity-to-Entity Relationships

### Direct Relationships

```
Account (1) ──→ (N) Contact
  via AccountContacts junction table
  
Account (1) ──→ (N) Opportunity
  via Opportunity.AccountId foreign key
  
Account (1) ──→ (N) Activity
  via Activity linked to account
  
Account (1) ──→ (1) User
  via Account.OwnerId / AssignedToUserId
  
Account (1) ──→ (1) Contact [Individual Only]
  via Account.LinkedContactId (optional)
  
Account (1) ──→ (N) CustomField values
  via CustomFieldValue.EntityId + EntityType
```

### Many-to-Many via Junction Table

**AccountContacts:**

```sql
CREATE TABLE AccountContacts (
  Id INT PRIMARY KEY,
  AccountId INT NOT NULL FOREIGN KEY,
  ContactId INT NOT NULL FOREIGN KEY,
  Role VARCHAR(100),
  IsPrimaryContact BOOLEAN,
  IsDecisionMaker BOOLEAN,
  PositionAtCustomer VARCHAR(200),
  CreatedAt DATETIME,
  UpdatedAt DATETIME,
  IsDeleted BOOLEAN,
  
  UNIQUE KEY (AccountId, ContactId)
);
```

**Example Data:**

| AccountId | ContactId | Role | IsPrimary | IsDecisionMaker |
|-----------|-----------|------|-----------|-----------------|
| 1 | 5 | CEO | true | true |
| 1 | 6 | VP Sales | false | true |
| 1 | 7 | Finance Lead | false | false |

---

## API Endpoints Reference

### Core Endpoints

```
GET    /api/accounts                    # List all accounts (paginated)
POST   /api/accounts                    # Create new account
GET    /api/accounts/{id}               # Get account by ID
PUT    /api/accounts/{id}               # Update account
PATCH  /api/accounts/{id}               # Partial update
DELETE /api/accounts/{id}               # Soft delete
```

### Search & Filter

```
GET    /api/accounts/search/{term}              # Full-text search
GET    /api/accounts/by-stage/{stage}           # Filter by lifecycle stage
GET    /api/accounts/by-priority/{priority}     # Filter by priority
GET    /api/accounts/by-user/{userId}           # Accounts assigned to user
GET    /api/accounts/individuals                # Only individual accounts
GET    /api/accounts/organizations              # Only organization accounts
```

### Contact Management

```
GET    /api/accounts/{id}/contacts             # Get linked contacts
POST   /api/accounts/{id}/contacts             # Add contact link
DELETE /api/accounts/{id}/contacts/{contactId} # Remove contact link
GET    /api/accounts/{id}/contacts/primary     # Get primary contact
```

### Related Data

```
GET    /api/accounts/{id}/opportunities        # Get linked opportunities
GET    /api/accounts/{id}/interactions         # Get activity timeline
GET    /api/accounts/{id}/details              # Get complete details
POST   /api/accounts/{id}/tags                 # Add/update tags
```

### Batch Operations

```
POST   /api/accounts/batch                # Create multiple accounts
DELETE /api/accounts/batch                # Delete multiple accounts
```

---

## Frontend Components & Pages

### Component Hierarchy

```
App
├── AccountsPage (Main list view)
│   ├── AccountsToolbar
│   │   ├── SearchBar
│   │   ├── FilterPanel
│   │   └── ActionButtons (Add, Export, Import, Merge)
│   ├── AccountsTable
│   │   ├── TableHead
│   │   ├── TableBody
│   │   │   └── TableRow (per account)
│   │   └── TablePagination
│   ├── CreateAccountDialog
│   │   └── AccountForm (dynamic fields)
│   ├── DuplicateDetectionDialog
│   └── MergeDialog
│
├── AccountDetailsPage (Detail view)
│   ├── AccountHeader
│   ├── Tabs
│   │   ├── OverviewTab
│   │   ├── ContactsTab
│   │   │   └── ContactInfoPanel
│   │   ├── OpportunitiesTab
│   │   ├── ActivitiesTab
│   │   └── NotesTab
│   └── RelatedEntitiesPanel
│
└── Account Related Pages
    ├── OpportunitiesPage (filtered by account)
    ├── ActivitiesPage (filtered by account)
    └── ContactsPage (filtered by account)
```

### Key Components Used

| Component | Purpose | File |
|-----------|---------|------|
| **DataGrid** | Table rendering | MUI DataGrid / Table |
| **Dialog** | Forms (create/edit) | MUI Dialog |
| **Tabs** | Multi-section views | MUI Tabs |
| **Chip** | Status badges | MUI Chip |
| **TextField** | Input fields | MUI TextField |
| **Select** | Dropdowns (lifecycle, priority) | MUI Select |
| **Autocomplete** | Contact/user selection | MUI Autocomplete |
| **AdvancedSearch** | Multi-criteria filtering | Custom component |
| **ContactInfoPanel** | Address/phone/email display | Custom component |
| **NotesTab** | Notes/comments | Custom component |
| **DuplicateDetectionDialog** | Duplicate scanning | Custom component |
| **MergeDialog** | Record merging | Custom component |

---

## Common Workflows

### Workflow 1: Create Organization Account with Contacts

```
1. User opens AccountsPage
2. Click "Add Account"
3. Dialog opens
4. Fill form:
   - Category: Organization
   - Company: "TechCorp Inc"
   - LegalName: "TechCorp International Inc"
   - Email: contact@techcorp.com
   - Phone: +1-555-1234
   - Address, City, State, Country
   - Industry: "Technology"
   - CustomerType: "Enterprise"
   - LifecycleStage: "Lead"
5. Click Submit
6. Backend creates Account + Address records
7. Frontend reloads accounts list
8. User clicks on new account → AccountDetailsPage
9. Go to "Contacts" tab
10. Click "Add Contact"
11. Select existing contacts or create new:
    - Jane Smith (CEO, IsPrimary: true, IsDecisionMaker: true)
    - John Doe (CTO, IsDecisionMaker: true)
    - Bob Wilson (Finance)
12. All contacts linked via AccountContacts table
```

### Workflow 2: Merge Duplicate Accounts

```
1. On AccountsPage, notice "Acme Corp" and "ACME Corporation"
2. Select both accounts (checkbox)
3. Click "Merge Duplicates"
4. MergeDialog opens showing:
   - Field comparison
   - Which values to keep for each field
   - Conflict resolution options
5. User chooses: Keep "Acme Corporation" as survivor
6. Click "Merge"
7. Backend:
   - Move all relationships from old → new account
   - Update AccountContacts (Old → New)
   - Update Opportunities (Old → New)
   - Soft-delete old account
   - Create audit log entry
8. Frontend reloads, shows merged result
9. Toast: "Merged successfully"
```

### Workflow 3: Update Account Territory & Assignment

```
1. Open AccountDetailsPage for Acme Corp
2. Edit fields:
   - Territory: "Pacific Northwest"
   - AssignedToUserId: "Sarah Johnson"
3. Click Save
4. Backend updates Account record
5. Persists change
6. Frontend shows success
7. Other users receive SignalR update
   → Their AccountsPage table auto-refreshes
```

---

## Implementation Patterns

### Pattern 1: Soft Delete (Logical Delete)

**Problem:** Can't physically delete accounts (data integrity, audit trails)

**Solution:** Mark as deleted without removing:

```csharp
// Backend
account.IsDeleted = true;
account.UpdatedAt = DateTime.UtcNow;
await _repository.SaveAsync();

// Queries automatically filter IsDeleted = false
var accounts = await _repository
    .FindAsync(a => !a.IsDeleted);
```

### Pattern 2: Optimistic Concurrency (RowVersion)

**Problem:** Concurrent updates could overwrite changes

**Solution:** Track version, detect conflicts:

```csharp
// EF Core handles this automatically
// On update: Check RowVersion (timestamp before/after)
// If mismatch: Throw DbUpdateConcurrencyException
// Frontend: Show conflict dialog, ask user to reload
```

### Pattern 3: Entity-to-DTO Mapping

**Problem:** Don't expose raw entities to frontend

**Solution:** Map through DTOs:

```csharp
// Backend
Account → AccountDto (serialize)
// Hides internal properties
// Ensures API contracts

// Frontend
AccountDto → Account (interface)
// Type-safe, but loose (optional properties for flexibility)
```

### Pattern 4: Polymorphic Display Name

**Problem:** Different display formats (Individual vs Organization)

**Solution:** Computed property in DTO:

```csharp
// Backend
public string GetDisplayName() => 
    Category == AccountCategory.Organization
        ? Company
        : $"{FirstName} {LastName}".Trim();

// Frontend
<td>{account.displayName}</td>
```

### Pattern 5: Hexagonal Architecture (Ports & Adapters)

**Problem:** Tight coupling between controllers and services

**Solution:** Define interfaces (ports):

```csharp
// Interface (Port)
public interface IAccountInputPort
{
    Task<AccountDto> CreateAccountAsync(CreateAccountDto dto);
}

// Implementation (Adapter)
public class AccountService : IAccountInputPort { }

// Usage (Controller)
public class AccountsController
{
    public AccountsController(IAccountInputPort accountService) { }
}
```

Benefits:
- Easy to test (mock IAccountInputPort)
- Easy to swap implementations
- Clear dependencies

---

## Development Checklist

When implementing Account features:

- [ ] Backend: Add property to Account.cs entity
- [ ] Backend: Add DTO properties (AccountDto, CreateAccountDto)
- [ ] Backend: Update AccountService methods
- [ ] Backend: Add validation (if needed)
- [ ] Backend: Create unit tests
- [ ] Frontend: Update Account interface in types
- [ ] Frontend: Update accountService.ts calls
- [ ] Frontend: Update form fields in AccountsPage
- [ ] Frontend: Add to table columns
- [ ] Frontend: Update detail view tabs
- [ ] Frontend: Add tests
- [ ] Database: Run migrations (if schema changes)
- [ ] Documentation: Update this guide

---

**END OF ACCOUNT ENTITY EXPLANATION**
