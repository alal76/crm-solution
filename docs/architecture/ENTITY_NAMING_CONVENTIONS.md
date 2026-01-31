# CRM Entity Naming Conventions

This document defines the standardized naming conventions for entity properties across the CRM solution. These conventions should be followed for all new development and considered during future refactoring.

## Current State vs Recommended

The codebase has some inconsistencies that evolved organically. This document captures the recommended patterns for future work.

---

## 1. Ownership & Assignment Fields

### Current Patterns (Legacy)
| Entity | Field Name | Description |
|--------|-----------|-------------|
| Lead | `OwnerId` | ✅ Preferred pattern |
| Opportunity | `SalesOwnerId` | Legacy naming |
| Customer | `AssignedToUserId` | Legacy naming |
| CrmTask | `AssignedToUserId` | Legacy naming |

### Recommended Standard
```csharp
// For primary ownership (sales rep, owner)
public int? OwnerId { get; set; }
public User? Owner { get; set; }

// For secondary assignment (account manager, current assignee)
public int? AssignedToId { get; set; }
public User? AssignedTo { get; set; }

// For record creator tracking
public int? CreatedById { get; set; }
public User? CreatedBy { get; set; }
```

### Reasoning
- `OwnerId` is concise and universally understood
- Avoids redundant "User" suffix (the FK relationship already implies User)
- Consistent with Salesforce and other major CRM platforms

---

## 2. Postal/ZIP Code Fields

### Current Patterns (Legacy)
| Entity | Field Name | Notes |
|--------|-----------|-------|
| Customer | `ZipCode` | US-centric |
| Customer | `ShippingZipCode` | US-centric |
| Account | `BillingZip` | Abbreviated |
| Contact (Model) | `ZipCode` | US-centric |

### Recommended Standard
```csharp
// Use "PostalCode" for international compatibility
public string PostalCode { get; set; } = string.Empty;
public string? ShippingPostalCode { get; set; }
public string? BillingPostalCode { get; set; }
public string? MailingPostalCode { get; set; }
```

### Reasoning
- "Postal code" is the international term
- Works for US ZIP codes, UK postcodes, Canadian postal codes, etc.
- More inclusive for global customers

---

## 3. Date/Time Fields

### Current Patterns
- `CreatedAt` (from BaseEntity) ✅
- `UpdatedAt` (from BaseEntity) ✅
- `DateAdded` (Contact Model) - Legacy
- `LastModified` (Contact Model) - Legacy

### Recommended Standard
```csharp
// BaseEntity provides these - all entities should inherit
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }

// For activity-specific dates, use descriptive names
public DateTime? LastActivityDate { get; set; }
public DateTime? NextFollowUpDate { get; set; }
public DateTime? FirstContactDate { get; set; }
public DateTime? ConversionDate { get; set; }
```

---

## 4. Contact Information

### Recommended Standard
```csharp
// Primary contact methods
public string Email { get; set; } = string.Empty;
public string Phone { get; set; } = string.Empty;

// Secondary/additional
public string? SecondaryEmail { get; set; }
public string? MobilePhone { get; set; }
public string? WorkPhone { get; set; }
public string? FaxNumber { get; set; }

// For normalized contact info (many addresses/phones/emails)
// Use ContactInfoLink entity pattern
```

---

## 5. Currency Fields

### Recommended Standard
```csharp
// Use 3-letter ISO currency code
[MaxLength(3)]
public string? Currency { get; set; } // USD, EUR, GBP, etc.

// Or use lookup for full currency info
public int? CurrencyLookupId { get; set; }
public LookupItem? CurrencyLookup { get; set; }
```

---

## 6. Status & Type Enums

### Recommended Patterns
```csharp
// Entity-specific enums with descriptive prefix
public enum CustomerLifecycleStage { Lead, Opportunity, Customer, Churned }
public enum OpportunityStage { Discovery, Qualification, Proposal, Negotiation, ClosedWon, ClosedLost }
public enum CrmTaskStatus { NotStarted, InProgress, Completed, Deferred, Cancelled }

// Priority enum (standardized across entities)
public enum Priority { Low = 0, Medium = 1, High = 2, Critical = 3 }
```

---

## 7. Validation Attributes

### Required Fields Pattern
```csharp
[Required]
[MaxLength(255)]
public string Name { get; set; } = string.Empty;

[Required]
[MaxLength(255)]
[EmailAddress]
public string Email { get; set; } = string.Empty;

[Required]
[MaxLength(30)]
[Phone]
public string Phone { get; set; } = string.Empty;
```

### Common MaxLength Values
| Field Type | MaxLength |
|-----------|-----------|
| Name/Title | 100-255 |
| Email | 255 |
| Phone | 30 |
| Address Line | 255 |
| City/State | 100 |
| PostalCode | 20 |
| Country | 100 |
| URL | 500 |
| Description/Notes | No limit (text) |
| Currency Code | 3 |
| Tags | 500 |

---

## 8. Navigation Properties

### Recommended Standard
```csharp
// FK property
public int CustomerId { get; set; }

// Navigation property (virtual for lazy loading)
[ForeignKey("CustomerId")]
public virtual Customer Customer { get; set; } = null!;

// Collection navigation
public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
```

---

## 9. Soft Delete Pattern

All entities inherit from BaseEntity which provides:
```csharp
public bool IsDeleted { get; set; } = false;
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
public byte[] RowVersion { get; set; } // Concurrency token
```

---

## Migration Path

When refactoring legacy fields to standardized names:

1. **Database Migration**: Create EF Core migration with column rename
2. **DTO Mapping**: Update AutoMapper/manual mappings
3. **API Contracts**: Consider backward compatibility
4. **Frontend**: Update form bindings and API calls
5. **Tests**: Update unit and integration tests

### Example Migration
```csharp
// In migration
migrationBuilder.RenameColumn(
    name: "ZipCode",
    table: "Customers",
    newName: "PostalCode");
```

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2024 | Initial | Documented current conventions and recommendations |
