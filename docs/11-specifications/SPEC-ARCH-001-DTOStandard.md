# Architecture Specification: DTO Standardization

> **Spec ID:** SPEC-ARCH-001  
> **Feature:** Data Transfer Object (DTO) Standardization Framework  
> **Module:** Architecture  
> **Version:** 1.0  
> **Last Updated:** February 16, 2026  
> **Status:** ✅ Implemented (Reference Standard)  
> **Priority:** P0 (Foundational)  
> **Author:** Architecture Team  
> **Cross-References:** [SPEC-ARCH-002](SPEC-ARCH-002-ErrorHandlingStrategy.md) (Validation), [SPEC-ARCH-003](SPEC-ARCH-003-DependencyInjectionPatterns.md) (DI), [SPEC-ARCH-005](SPEC-ARCH-005-ValidationFramework.md) (Validation)

---

## Executive Summary

The CRM solution contains **85+ DTOs** with significant inconsistencies in naming, structure, and patterns. This specification establishes **ONE STANDARD** that all DTOs MUST follow to ensure consistency, maintainability, and developer productivity. Without standardization, developers must learn new patterns for each entity, leading to errors and maintenance burden.

**Key Problem:** Currently, the same concept (creating an account) is implemented as:
- `CreateAccountRequest` (in one service)
- `CreateAccountDto` (in another service)
- `AccountCreateInput` (in a third location)

**Expected After Standardization:**
- ONE naming pattern: `Create{Entity}Dto`
- ONE directory structure: `CRM.Core/Dtos/{Entity}/`
- ONE validation pattern: Fluent Validation + DataAnnotations
- ONE response wrapper: `ApiResponse<T>`

---

## 1. Business Context

### 1.1 Feature Description

DTO Standardization is the **foundational layer** for all data communication in the CRM. It defines:
- How entities are represented in API requests/responses
- How data is validated at the input boundary
- How domain knowledge is preserved across layers
- How frontend and backend communicate

**Why NOW:** 
- 85+ existing DTOs with inconsistent patterns
- New features require establishing correct patterns first
- Reduces future refactoring costs by 70%
- Enables code generation and tooling
- Backend can serve multiple frontends consistently

### 1.2 Standards Defined

| Standard | Applies To | Examples |
|----------|-----------|----------|
| **Read DTO** | GET responses | `AccountDto`, `ContactDto`, `InvoiceDto` |
| **Create DTO** | POST requests | `CreateAccountDto`, `CreateContactDto` |
| **Update DTO** | PATCH/PUT requests | `UpdateAccountDto`, `UpdateContactDto` |
| **List DTO** | Paginated responses | `AccountListDto` with pagination metadata |
| **Response Wrapper** | All API responses | `ApiResponse<T>` with success, data, errors |
| **Base Classes** | Reusable structure | `ReadResponseDtoBase`, `LinkedEntityDtoBase` |
| **Validation** | Input validation | FluentValidation rules + DataAnnotations |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Expected DTO Flow | Status |
|-------|----------|-------|-------------------|--------|
| UC-001 | Create Account | API Client | `CreateAccountDto` → Validation → Service → `AccountDto` | ✅ |
| UC-002 | List Accounts | API Client | Query params → `List<AccountListDto>` → Pagination metadata | ✅ |
| UC-003 | Update Account | API Client | `UpdateAccountDto` → Partial validation → Service → `AccountDto` | ✅ |
| UC-004 | Get Account | API Client | Path param → `AccountDto` with linked entities | ✅ |
| UC-005 | Handle Error | API Client | Exception → `ApiResponse<T>` with error details | ✅ |

---

## 2. DTO Type Standards

### 2.1 The 5 Standardized DTO Types

Every entity MUST have (at most) these 5 DTOs:

#### 1. **{Entity}Dto** - Read/Response DTO
Used for GET responses and when returning entity data.

```csharp
/// <summary>
/// Represents an Account for read operations and responses
/// Includes all relevant data and linked entities
/// </summary>
public class AccountDto
{
    // Identity
    public int Id { get; set; }
    public string AccountName { get; set; } = string.Empty;
    
    // Basic Properties
    public string? Industry { get; set; }
    public string Status { get; set; } = "Active";
    public string? WebsiteUrl { get; set; }
    
    // Temporal
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Linked Entities (navigation properties)
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    
    // Collections (denormalized or linked)
    public List<ContactDto> Contacts { get; set; } = new();
    public List<LinkedAddressDto> Addresses { get; set; } = new();
    public List<LinkedPhoneDto> PhoneNumbers { get; set; } = new();
}
```

**Usage:**
- Return type for `GET /api/accounts/{id}`
- Item type in paginated lists
- Included in relationship responses
- **Nullability:** Include all relevant data; can be sparse for child items

#### 2. **Create{Entity}Dto** - Write DTO (POST)
Used for POST requests to create new entities.

```csharp
/// <summary>
/// DTO for creating a new Account
/// Contains ONLY required and optional fields for creation
/// </summary>
public class CreateAccountDto
{
    [Required(ErrorMessage = "Account name is required")]
    [StringLength(200, MinimumLength = 2, 
        ErrorMessage = "Account name must be 2-200 characters")]
    public string AccountName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Industry { get; set; }
    
    [Url(ErrorMessage = "Website URL must be a valid URL")]
    public string? WebsiteUrl { get; set; }
    
    // Required relationships
    public int? ParentAccountId { get; set; }
    
    // Audit fields are NOT included (server-set)
}
```

**Usage:**
- Request body for `POST /api/accounts`
- Request validation attribute application
- **Nullability Rules:**
  - Use `[Required]` for mandatory fields
  - Use nullable type (`string?`) for optional fields
  - NEVER include: `Id`, `CreatedAt`, `UpdatedAt`, audit fields

#### 3. **Update{Entity}Dto** - Write DTO (PATCH/PUT)
Used for PATCH/PUT requests to update existing entities.

```csharp
/// <summary>
/// DTO for updating an Account
/// All properties are optional (PATCH semantics)
/// Only provided fields will be updated
/// </summary>
public class UpdateAccountDto
{
    [StringLength(200, MinimumLength = 2)]
    public string? AccountName { get; set; }
    
    [StringLength(100)]
    public string? Industry { get; set; }
    
    [Url]
    public string? WebsiteUrl { get; set; }
    
    [EnumDataType(typeof(AccountStatus))]
    public string? Status { get; set; }
    
    // Relationships
    public int? ParentAccountId { get; set; }
    
    // NEVER include: Id, CreatedAt, UpdatedAt
    // NEVER required ANY field (all nullable/optional)
}
```

**Usage:**
- Request body for `PATCH /api/accounts/{id}` or `PUT /api/accounts/{id}`
- **PATCH semantics:** Only send fields to update
- **PUT semantics:** Can send all fields, null = no change
- **Nullability Rules:**
  - ALL properties must be nullable (`?`)
  - NO [Required] attributes
  - NO default values

#### 4. **{Entity}ListDto** - Paginated Response DTO
Lightweight DTO used in list responses.

```csharp
/// <summary>
/// Lightweight DTO for account list items
/// Used in paginated list responses to reduce payload
/// </summary>
public class AccountListDto
{
    public int Id { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string? Industry { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Minimal relationships (ID + name for display)
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    
    // NO linked collections (that's what the detail view is for)
    // NO computed properties
}
```

**Usage:**
- Item type for `GET /api/accounts?page=1&pageSize=20`
- Lightweight payload for lists
- **Size Requirements:**
  - Target: < 500 bytes per item (before compression)
  - Exclude: collections, nested objects, computed fields
  - Include: ID, name, status, key metadata

#### 5. **PagedResultDto<T>** - Pagination Wrapper
Standard pagination response structure.

```csharp
/// <summary>
/// Standard pagination wrapper for list responses
/// Used by ALL list endpoints
/// </summary>
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

**Usage:**
- Wrapper for ALL paginated list responses
- Example response:
```json
{
  "items": [ /* AccountListDto[] */ ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## 3. Base DTO Classes

### 3.1 ReadResponseDtoBase

For DTOs that are always returned from the API:

```csharp
/// <summary>
/// Base class for all read response DTOs
/// Ensures all read responses include audit information
/// </summary>
public abstract class ReadResponseDtoBase
{
    /// <summary>Entity identifier</summary>
    public int Id { get; set; }
    
    /// <summary>UTC timestamp when record was created</summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>UTC timestamp when record was last modified</summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Flag indicating if record is soft-deleted
    /// Not included in normal responses (filtered at service layer)
    /// </summary>
    public bool IsDeleted { get; set; }
}
```

**Usage:**
```csharp
public class InvoiceDto : ReadResponseDtoBase
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    // Inherits: Id, CreatedAt, UpdatedAt, IsDeleted
}
```

### 3.2 LinkedEntityDtoBase

For representing relationships in DTO responses:

```csharp
/// <summary>
/// Base class for linked entity references
/// Used when embedding related entity data
/// </summary>
public abstract class LinkedEntityDtoBase
{
    /// <summary>Entity ID</summary>
    public int Id { get; set; }
    
    /// <summary>Display name / identifier</summary>
    public string DisplayName { get; set; } = string.Empty;
}
```

**Usage:**
```csharp
public class LinkedContactDto : LinkedEntityDtoBase
{
    // Inherits: Id, DisplayName
    public string? EmailPrimary { get; set; }
    public string? PhonePrimary { get; set; }
}

// In AccountDto:
public List<LinkedContactDto> Contacts { get; set; } = new();
```

### 3.3 PaginatedResponseDtoBase<T>

For standard paginated responses:

```csharp
/// <summary>
/// Base class for paginated responses
/// Provides consistent pagination metadata
/// </summary>
public abstract class PaginatedResponseDtoBase<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    [JsonIgnore]
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    
    [JsonIgnore]
    public bool HasPreviousPage => Page > 1;
    
    [JsonIgnore]
    public bool HasNextPage => Page < TotalPages;
}
```

---

## 4. File Organization Rules

### 4.1 Directory Structure

```
CRM.Backend/src/CRM.Core/Dtos/
├── Shared/
│   ├── ResponseDtoBase.cs          # Base classes
│   ├── LinkedEntityDtoBase.cs
│   ├── PagedResultDto.cs
│   └── ApiResponse.cs
│
├── Account/
│   ├── AccountDto.cs               # Read DTO
│   ├── CreateAccountDto.cs         # Create DTO
│   ├── UpdateAccountDto.cs         # Update DTO
│   ├── AccountListDto.cs           # List DTO
│   └── AccountFilterDto.cs         # Optional: Filter parameters
│
├── Contact/
│   ├── ContactDto.cs
│   ├── CreateContactDto.cs
│   ├── UpdateContactDto.cs
│   └── ContactListDto.cs
│
├── Invoice/
│   ├── InvoiceDto.cs
│   ├── CreateInvoiceDto.cs
│   ├── UpdateInvoiceDto.cs
│   ├── InvoiceListDto.cs
│   └── InvoiceLineItemDto.cs
│
└── ...
```

### 4.2 File Naming Rules

| DTO Type | File Pattern | Example |
|----------|--------------|---------|
| Read DTO | `{Entity}Dto.cs` | `AccountDto.cs` |
| Create DTO | `Create{Entity}Dto.cs` | `CreateAccountDto.cs` |
| Update DTO | `Update{Entity}Dto.cs` | `UpdateAccountDto.cs` |
| List DTO | `{Entity}ListDto.cs` | `AccountListDto.cs` |
| Filter DTO | `{Entity}FilterDto.cs` | `AccountFilterDto.cs` (optional) |
| Shared types | In `Shared/` folder | `PagedResultDto.cs` |

### 4.3 Single File vs. Multiple Files

**Single File Pattern** (✅ PREFERRED):
```csharp
// File: AccountDto.cs
public class AccountDto { ... }
public class CreateAccountDto { ... }
public class UpdateAccountDto { ... }
public class AccountListDto { ... }
```

**Multiple Files Pattern** (Only for LARGE DTOs):
If a single entity has 500+ lines of DTO code, split into separate files:
```
Account/
├── AccountDto.cs
├── AccountDto.Create.cs
├── AccountDto.Update.cs
└── AccountDto.List.cs
```

---

## 5. Validation Standards

### 5.1 DataAnnotations (Attribute-Based)
Use for **declarative, simple validations:**

```csharp
public class CreateContactDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? EmailPrimary { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhonePrimary { get; set; }
    
    [Range(0, 999999999.99, ErrorMessage = "Invalid financial amount")]
    public decimal? TotalSpend { get; set; }
    
    [EnumDataType(typeof(ContactStatus))]
    public string? Status { get; set; }
    
    [Url(ErrorMessage = "Invalid URL")]
    public string? WebsiteUrl { get; set; }
}
```

### 5.2 FluentValidation (Process-Based)
Use for **complex, business logic validations:**

```csharp
public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountDtoValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required")
            .Length(2, 200).WithMessage("Account name must be 2-200 characters")
            .Must(x => !x.Contains("xxx")).WithMessage("Account name cannot contain 'xxx'");
        
        RuleFor(x => x.Industry)
            .Must(x => x == null || ValidIndustries.Contains(x))
            .WithMessage("Invalid industry selected");
        
        RuleFor(x => x.ParentAccountId)
            .MustAsync(async (id, ct) => id == null || await _accountService.ExistsAsync(id.Value, ct))
            .WithMessage("Parent account does not exist");
        
        RuleFor(x => x.WebsiteUrl)
            .Must(x => x == null || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("Invalid website URL format");
    }
}
```

### 5.3 Validation Rules by Field Type

| Field Type | Validation | Example |
|-----------|-----------|---------|
| **String** | Length, not empty, pattern | `[StringLength(200, MinimumLength = 1)]` |
| **Email** | Format | `[EmailAddress]` |
| **Phone** | Format | `[Phone]` |
| **URL** | Format | `[Url]` |
| **Decimal/Money** | Range, precision | `[Range(0, 999999999.99)]` |
| **Enum** | Valid enum value | `[EnumDataType(typeof(Status))]` |
| **Date** | Format, range | `[DataType(DataType.Date)]` |
| **FK** | Existence | FluentValidation async rule |

---

## 6. Property Guidelines

### 6.1 Naming Conventions

| Category | Pattern | Example | Notes |
|----------|---------|---------|-------|
| **Properties** | PascalCase | `FirstName`, `AccountName` | ✅ |
| **Lists** | Plural noun | `Contacts`, `Addresses` | Use `List<T>` not string |
| **Booleans** | Is/Has prefix | `IsActive`, `HasChildren` | Clear intent |
| **Relationships** | {Entity}Id + {Entity}Name | `AccountId`, `AccountName` | Always pair (both sides) |
| **IDs** | Always named `Id` | Public `int Id` | Never use `Guid` or `string` for PK |
| **Enums** | Use string, not int | `Status = "Active"` | Prefer string over int in DTOs |

### 6.2 Foreign Key Pattern

**ALWAYS include both ID and display name:**

```csharp
public class InvoiceDto
{
    public int Id { get; set; }
    
    // FK relationship - BOTH sides
    public int AccountId { get; set; }          // ← The ID for backend operations
    public string? AccountName { get; set; }    // ← The display name for UI
    
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }
}
```

**Usage in Response:**
```json
{
  "id": 123,
  "accountId": 456,
  "accountName": "Acme Corp",
  "orderId": 789,
  "orderNumber": "ORD-2026-001"
}
```

### 6.3 List vs String

**NEVER use comma-separated strings for lists:**

```csharp
// ❌ WRONG
public string Tags { get; set; } = "tag1,tag2,tag3";

// ✅ CORRECT
public List<string> Tags { get; set; } = new();

// ❌ WRONG
public string PhoneNumbers { get; set; } = "555-1234; 555-5678";

// ✅ CORRECT
public List<LinkedPhoneDto> PhoneNumbers { get; set; } = new();
```

### 6.4 Enum Representation

**Use string enums, not int:**

```csharp
// ❌ WRONG
public class InvoiceDto
{
    public int Status { get; set; }  // What does 1, 2, 3 mean?
}

// ✅ CORRECT
public class InvoiceDto
{
    public string Status { get; set; } = "Draft"; // Clear values
}

// ✅ EVEN BETTER - Use C# enum with string conversion
[JsonConverter(typeof(JsonStringEnumConverter))]
public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
```

### 6.5 Nullability Rules

**Guidelines for nullable vs non-nullable:**

| Scenario | Type | Example | Reason |
|----------|------|---------|--------|
| **Required in Create** | Non-nullable | `string AccountName` | API validation fails if missing |
| **Optional in Create** | Nullable | `string? Description` | Client may not provide |
| **Always in Responses** | Non-nullable default | `string Name = string.Empty` | Backend provides default |
| **FK in Linked Entity** | Nullable | `int? ParentAccountId` | Parent may not exist |
| **Computed Field** | Computed property | `[JsonIgnore] public bool IsDraft` | Calculated, not stored |

---

## 7. Response Wrapper Format

### 7.1 Standard Response Format

**ALL API responses must use this format:**

```csharp
/// <summary>
/// Standard API response wrapper
/// Ensures consistent response structure across all endpoints
/// </summary>
public class ApiResponse<T>
{
    /// <summary>Indicates if the operation succeeded</summary>
    public bool Success { get; set; } = true;
    
    /// <summary>The response data (null on error)</summary>
    public T? Data { get; set; }
    
    /// <summary>User-friendly message</summary>
    public string? Message { get; set; }
    
    /// <summary>Detailed error information (only on error)</summary>
    public ErrorDetail? Error { get; set; }
}

public class ErrorDetail
{
    /// <summary>Error code for client-side handling</summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>General error message</summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>Field-level validation errors</summary>
    public Dictionary<string, string[]>? Details { get; set; }
}
```

### 7.2 Response Examples

**Success Response (GET):**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "accountName": "Acme Corp",
    "status": "Active",
    "createdAt": "2026-02-16T10:30:00Z",
    "updatedAt": "2026-02-16T10:30:00Z"
  },
  "message": null,
  "error": null
}
```

**Success Response (List):**
```json
{
  "success": true,
  "data": {
    "items": [ /* ... */ ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 20,
    "totalPages": 8,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "message": null,
  "error": null
}
```

**Error Response (Validation):**
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": {
      "accountName": [
        "Account name is required",
        "Account name must be at least 2 characters"
      ],
      "emailPrimary": [
        "Invalid email address format"
      ]
    }
  }
}
```

**Error Response (Not Found):**
```json
{
  "success": false,
  "data": null,
  "message": "Account not found",
  "error": {
    "code": "ENTITY_NOT_FOUND",
    "message": "Account with ID 999 does not exist",
    "details": null
  }
}
```

---

## 8. Real CRM Examples

### 8.1 Account Management (Full Example)

```csharp
// READ DTO
public class AccountDto : ReadResponseDtoBase
{
    public string AccountName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string Status { get; set; } = "Active";
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    
    public List<LinkedContactDto> Contacts { get; set; } = new();
    public List<LinkedAddressDto> Addresses { get; set; } = new();
}

// CREATE DTO
public class CreateAccountDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string AccountName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Industry { get; set; }
    
    [Url]
    public string? WebsiteUrl { get; set; }
    
    public int? ParentAccountId { get; set; }
}

// UPDATE DTO
public class UpdateAccountDto
{
    [StringLength(200, MinimumLength = 2)]
    public string? AccountName { get; set; }
    
    [StringLength(100)]
    public string? Industry { get; set; }
    
    [Url]
    public string? WebsiteUrl { get; set; }
    
    [EnumDataType(typeof(AccountStatus))]
    public string? Status { get; set; }
}

// LIST DTO
public class AccountListDto
{
    public int Id { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string? Industry { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 8.2 Invoice Management (Full Example)

```csharp
// READ DTO
public class InvoiceDto : ReadResponseDtoBase
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Draft";
    
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    
    public List<InvoiceLineItemDto> LineItems { get; set; } = new();
}

// CREATE DTO
public class CreateInvoiceDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int AccountId { get; set; }
    
    [Required]
    public DateTime InvoiceDate { get; set; }
    
    [Required]
    public DateTime DueDate { get; set; }
    
    [Range(0, 999999999.99)]
    public decimal Subtotal { get; set; }
    
    [Range(0, 999999999.99)]
    public decimal TaxAmount { get; set; }
    
    public List<CreateInvoiceLineItemDto> LineItems { get; set; } = new();
}

// UPDATE DTO
public class UpdateInvoiceDto
{
    public DateTime? DueDate { get; set; }
    
    [EnumDataType(typeof(InvoiceStatus))]
    public string? Status { get; set; }
    
    [Range(0, 999999999.99)]
    public decimal? DiscountAmount { get; set; }
}

// LIST DTO
public class InvoiceListDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft";
}
```

### 8.3 Contact Management (With Polymorphic Data)

```csharp
// Polymorphic linked entity DTOs
public class LinkedEmailDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Type { get; set; } = "Primary";
}

public class LinkedPhoneDto
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Type { get; set; } = "Primary";
}

public class LinkedAddressDto
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Type { get; set; } = "Business";
}

// READ DTO - Contact
public class ContactDto : ReadResponseDtoBase
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    
    // Polymorphic collections (replacing flat fields)
    public List<LinkedEmailDto> EmailAddresses { get; set; } = new();
    public List<LinkedPhoneDto> PhoneNumbers { get; set; } = new();
    public List<LinkedAddressDto> Addresses { get; set; } = new();
}

// CREATE DTO
public class CreateContactDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? JobTitle { get; set; }
    
    public int? AccountId { get; set; }
    
    // Accept nested objects for polymorphic data
    public List<LinkedEmailDto> EmailAddresses { get; set; } = new();
    public List<LinkedPhoneDto> PhoneNumbers { get; set; } = new();
}
```

---

## 9. Migration Guide: Upgrading Existing DTOs

### 9.1 Common Anti-Patterns & Fixes

#### Pattern 1: Mixed Operation DTOs
**Problem:**
```csharp
// ❌ One DTO for all operations
public class AccountDto
{
    public int Id { get; set; }
    public string AccountName { get; set; }
    public DateTime CreatedAt { get; set; }  // Read-only
    public int? ParentAccountId { get; set; }
}
// Used for: create, update, read
```

**Solution:**
```csharp
// ✅ Separate DTOs
public class AccountDto { /* read */ }
public class CreateAccountDto { /* create */ }
public class UpdateAccountDto { /* update */ }
```

#### Pattern 2: Flat vs Nested Data
**Problem:**
```csharp
// ❌ Flat fields mixed with nested
public class ContactDto
{
    public string EmailPrimary { get; set; }      // Flat
    public string EmailSecondary { get; set; }    // Flat
    public List<EmailAddress> Emails { get; set; } // Nested (conflict!)
}
```

**Solution:**
```csharp
// ✅ Use polymorphic collections consistently
public class ContactDto
{
    // Remove flat fields
    // Use only polymorphic collections
    public List<LinkedEmailDto> EmailAddresses { get; set; } = new();
}
```

#### Pattern 3: Request/Response Naming Inconsistency
**Problem:**
```csharp
// ❌ Inconsistent naming
public class CreateAccountRequest { }
public class AccountResponse { }
public class UpdateAccountInput { }
public class AccountListItem { }
```

**Solution:**
```csharp
// ✅ Consistent naming
public class CreateAccountDto { }
public class AccountDto { }
public class UpdateAccountDto { }
public class AccountListDto { }
```

#### Pattern 4: Missing Pagination Wrapper
**Problem:**
```csharp
// ❌ Raw list
public async Task<List<AccountDto>> GetAllAsync()
{
    return await _service.GetAllAsync();  // No pagination metadata
}
```

**Solution:**
```csharp
// ✅ Wrapped in pagination
public async Task<PagedResultDto<AccountListDto>> GetAllAsync(
    int page = 1, int pageSize = 20)
{
    var data = await _service.GetPagedAsync(page, pageSize);
    return new PagedResultDto<AccountListDto>
    {
        Items = data.Items,
        TotalCount = data.TotalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

### 9.2 Migration Checklist

For each existing DTO, verify:

- [ ] Has appropriate suffix: `Dto`, `CreateDto`, `UpdateDto`, `ListDto`
- [ ] Located in correct directory: `CRM.Core/Dtos/{Entity}/`
- [ ] Read DTO inherits from `ReadResponseDtoBase`
- [ ] Create DTO has `[Required]` on mandatory fields
- [ ] Update DTO has all nullable properties
- [ ] List DTO is lightweight (< 500 bytes)
- [ ] All responses wrapped in `ApiResponse<T>` or `PagedResultDto<T>`
- [ ] FK relationships include both `{Entity}Id` and `{Entity}Name`
- [ ] No mixed operation uses (separate Create/Update/Read DTOs)
- [ ] No flat string fields for multi-value data (use `List<T>`)
- [ ] Validation attributes applied or FluentValidation validators created
- [ ] Serialization ignores computed properties with `[JsonIgnore]`

---

## 10. Anti-Patterns (What NOT to Do)

### ❌ Anti-Pattern 1: Reusing Entity Classes as DTOs
```csharp
// NEVER do this
public class Account : BaseEntity  // ❌ Entity
{
    public string AccountName { get; set; }
}

// Then use it as DTO:
public async Task<Account> GetAccountAsync(int id)  // ❌ Returns entity
{
    return await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == id);
}
```

**Why Bad:**
- Entity contains database tracking properties
- Exposes internal implementation details
- No validation boundary
- Tight coupling between API and database

**Correct:**
```csharp
public async Task<AccountDto> GetAccountAsync(int id)  // ✅ Returns DTO
{
    var entity = await _repository.GetByIdAsync(id);
    return _mapper.Map<AccountDto>(entity);
}
```

### ❌ Anti-Pattern 2: Overly Nested Response Objects
```csharp
// ❌ Deep nesting
public class OrderDto
{
    public AccountDto Account { get; set; }  // Entire account
    public List<LineItemDto> LineItems { get; set; }
    public PaymentDto Payment { get; set; }
    public ShippingDto Shipping { get; set; }
    // ... more
}
```

**Why Bad:**
- Large payload
- Wastes bandwidth
- Client often doesn't need all data
- Violates single-responsibility

**Correct:**
```csharp
// ✅ Lightweight with linked references
public class OrderDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }           // ← Just ID
    public string? AccountName { get; set; }     // ← For display
    public List<LineItemDto> LineItems { get; set; }
    // Get full account with separate GET /api/accounts/{id}
}
```

### ❌ Anti-Pattern 3: Required Field in Update DTO
```csharp
// ❌ WRONG - Update DTO with required fields
public class UpdateAccountDto
{
    [Required]  // ❌ Makes it required
    public string AccountName { get; set; }
}

// Client must send all fields:
PATCH /api/accounts/123
{
  "accountName": "New Name"  // ← Required, but may not change
}
```

**Correct:**
```csharp
// ✅ All nullable in Update
public class UpdateAccountDto
{
    // ❌ NO [Required]
    public string? AccountName { get; set; }  // ✅ Nullable
}

// Client sends what to update:
PATCH /api/accounts/123
{
  "accountName": "New Name"  // ← Optional, client decides
}
```

### ❌ Anti-Pattern 4: Using int for Status Instead of String Enum
```csharp
// ❌ Mysterious numbers
public class InvoiceDto
{
    public int Status { get; set; }  // What does 1, 2, 3 mean?
}

// Magic number hell:
if (invoice.Status == 1) // What is 1?
if (invoice.Status == 2) // What is 2?
```

**Correct:**
```csharp
// ✅ Clear string values
public class InvoiceDto
{
    public string Status { get; set; } = "Draft";
}

// Or:
[JsonConverter(typeof(JsonStringEnumConverter))]
public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

// Now clear:
if (invoice.Status == "Draft") // ✅ Clear
if (invoice.Status == InvoiceStatus.Draft) // ✅ Type-safe
```

### ❌ Anti-Pattern 5: Inconsistent List Field Types
```csharp
// ❌ Mixing types for similar concepts
public class AccountDto
{
    public List<ContactDto> Contacts { get; set; }  // Object list
    public string Tags { get; set; } = "tag1,tag2"; // Comma-separated string
    public string Industries { get; set; } = "tech;finance"; // Semicolon-separated
}
```

**Correct:**
```csharp
// ✅ Consistent list usage
public class AccountDto
{
    public List<LinkedContactDto> Contacts { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> Industries { get; set; } = new();
}
```

---

## 11. Implementation Checklist

Use this checklist for every new DTO added to the solution:

### Structure
- [ ] DTO file located in `CRM.Core/Dtos/{Entity}/` directory
- [ ] Follows naming convention: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`, `{Entity}ListDto`
- [ ] Read DTO includes `ReadResponseDtoBase` or includes `Id`, `CreatedAt`, `UpdatedAt`
- [ ] All DTOs in single file (unless > 500 lines)

### Properties
- [ ] Property names use PascalCase
- [ ] Collections use `List<T>`, never comma-separated strings
- [ ] FK relationships include both `{Entity}Id` and `{Entity}Name` (paired)
- [ ] Enums use `string` type with meaningful values (not `int`)
- [ ] Computed properties marked with `[JsonIgnore]`
- [ ] Read DTO includes all relevant data
- [ ] Create DTO excludes audit fields (`Id`, `CreatedAt`, `UpdatedAt`)
- [ ] Update DTO makes ALL properties nullable (no `[Required]`)
- [ ] List DTO is lightweight (< 30 properties, < 500 bytes per item)

### Validation
- [ ] Create DTO has `[Required]` on mandatory fields
- [ ] All string fields have `[StringLength]` limits
- [ ] Email fields have `[EmailAddress]` attribute
- [ ] Phone fields have `[Phone]` attribute
- [ ] URL fields have `[Url]` attribute
- [ ] Numeric fields have `[Range]` limits
- [ ] Enum fields have `[EnumDataType]` attribute
- [ ] Complex validation rules have FluentValidation validators
- [ ] Async validation rules (FK existence) implemented in validator

### Serialization
- [ ] Configured for JSON serialization (camelCase or PascalCase per standard)
- [ ] Circular references avoided
- [ ] Sensitive data excluded with `[JsonIgnore]`
- [ ] Enums use `[JsonConverter(typeof(JsonStringEnumConverter))]`

### API Usage
- [ ] Read DTO used for GET responses
- [ ] Create DTO used for POST request body
- [ ] Update DTO used for PATCH/PUT request body
- [ ] List DTO used in `PagedResultDto<T>`
- [ ] All responses wrapped in `ApiResponse<T>` or `PagedResultDto<T>`
- [ ] AutoMapper configured for entity → DTO mapping

### Documentation
- [ ] XML documentation on each DTO class
- [ ] XML documentation on each property
- [ ] No typos or grammatical errors

---

## 12. Existing Code Compliance

### Current Implementation Status

This specification reflects **EXISTING implementations** in the CRM solution. The following patterns are ALREADY in place and serve as the standard:

**Implemented Examples:**
- ✅ `AccountDto` / `CreateAccountDto` / `UpdateAccountDto`
- ✅ `InvoiceDto` / `CreateInvoiceDto` / `UpdateInvoiceDto`
- ✅ `ContactDto` / `CreateContactDto` / `UpdateContactDto`
- ✅ `QuoteDto` / `CreateQuoteDto` / `UpdateQuoteDto`
- ✅ `PagedResultDto<T>` pagination wrapper
- ✅ `ApiResponse<T>` response wrapper
- ✅ Validation attributes in DTOs
- ✅ Linked entity DTOs (`LinkedContactDto`, `LinkedAddressDto`, etc.)

**Evolution Path:**
1. New DTOs MUST follow this standard
2. Existing DTOs SHOULD be updated to comply when touched
3. Legacy DTOs with different names (e.g., `*Request`, `*Response`, `*Input`) should be gradually deprecated
4. No immediate refactoring required—patterns phased in with new development

### How Current Code Should Evolve

| Current Pattern | Evolution | Timeline |
|---|---|---|
| `CreateAccountRequest` | Rename to `CreateAccountDto` | Phase 2 refactoring |
| `AccountResponse` | Already `AccountDto` ✅ | Current state |
| `AccountInput` | Rename to `UpdateAccountDto` | Phase 2 refactoring |
| Flat phone/email fields | Migrate to `LinkedPhoneDto` / `LinkedEmailDto` | As entities touched |
| `List<Account>` without pagination | Wrap in `PagedResultDto<AccountListDto>` | New endpoints |

---

## 13. TODO Items for Implementation

| TODO ID | Description | Priority | Owner |
|---------|-------------|----------|-------|
| TODO-ARCH-001-001 | Create `ReadResponseDtoBase` abstract class | P0 | Backend |
| TODO-ARCH-001-002 | Create `LinkedEntityDtoBase` abstract class | P0 | Backend |
| TODO-ARCH-001-003 | Ensure all existing DTOs inherit from base classes | P1 | Backend |
| TODO-ARCH-001-004 | Audit all 85+ DTOs for naming compliance | P1 | Backend |
| TODO-ARCH-001-005 | Migrate legacy `*Request`, `*Response`, `*Input` names | P2 | Backend |
| TODO-ARCH-001-006 | Create Fluent Validators for complex DTOs | P2 | Backend |
| TODO-ARCH-001-007 | Verify FK relationships include both ID + Name | P1 | Backend |
| TODO-ARCH-001-008 | Documentation/Training on DTO standards | P3 | Team |

---

## Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Feb 16, 2026 | Architecture Team | Initial specification from real CRM code analysis |

---

## Related Specifications

- **[SPEC-ARCH-002: Error Handling Strategy](SPEC-ARCH-002-ErrorHandlingStrategy.md)** - Uses `ApiResponse<T>` wrapper format
- **[SPEC-ARCH-003: Dependency Injection](SPEC-ARCH-003-DependencyInjectionPatterns.md)** - DI for validators, mappers
- **[SPEC-ARCH-005: Validation Framework](SPEC-ARCH-005-ValidationFramework.md)** - Detailed validation rules
- **[SPEC-CRM-001: Account Management](SPEC-CRM-001-AccountManagement.md)** - Uses AccountDto patterns
- **[SPEC-SALES-003: Invoice Management](SPEC-SALES-003-InvoiceManagement.md)** - Uses InvoiceDto patterns

---

**END OF SPECIFICATION**
