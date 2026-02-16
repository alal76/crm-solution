# Architecture Specification: Validation Framework

> **Spec ID:** SPEC-ARCH-005  
> **Feature:** Validation Framework & Standards  
> **Module:** Architecture  
> **Version:** 1.0  
> **Last Updated:** February 16, 2026  
> **Status:** ✅ Implemented (Reference Standard)  
> **Priority:** P0 (Foundational)  
> **Author:** Architecture Team  
> **Cross-References:** [SPEC-ARCH-001](SPEC-ARCH-001-DTOStandard.md) (Validation in DTOs), [SPEC-ARCH-002](SPEC-ARCH-002-ErrorHandlingStrategy.md) (ValidationException)

---

## Executive Summary

Validation is the **first line of defense** against invalid data entering the system. Without proper validation:
- Database constraints violated (integrity issues)
- Business logic breaks (unexpected states)
- Security vulnerabilities introduced (injection, overflow)
- User experience degrades (unclear error messages)

This specification establishes **ONE STANDARD** for where, when, and how to validate data across the CRM.

**Key Principle:** "Fail fast with clarity."

---

## 1. Business Context

### 1.1 Feature Description

Validation encompasses:
1. **DataAnnotations** - Declarative, attribute-based validation
2. **FluentValidation** - Programmatic, business logic validation
3. **Entity Validation** - Domain rules at entity level
4. **Service Validation** - Business logic validation in services
5. **Async Validation** - Validation requiring database lookups
6. **Composite Validation** - Multi-step validation workflows
7. **Error Messages** - Clear, actionable error text

### 1.2 Standards Defined

| Standard | Purpose | Usage |
|----------|---------|-------|
| **DataAnnotations** | Simple, declarative validation | DTOs, required/format checks |
| **FluentValidation** | Complex business logic validation | Custom rules, async, rules engine |
| **Chain of Validation** | Multiple validators working together | DTO validator + service validator |
| **Error Messages** | User-friendly, actionable guidance | "email already in use" vs "validation failed" |
| **Async Validation** | Database lookups during validation | Foreign key existence, uniqueness |
| **Custom Rules** | Business-specific validation logic | Circular hierarchy, status transitions |

---

## 2. Validation Layers

### 2.1 Layer 1: DataAnnotations (DTO Level)

Used for simple, **declarative validations** on DTOs.

```csharp
public class CreateAccountDto
{
    [Required(ErrorMessage = "Account name is required")]
    [StringLength(200, MinimumLength = 2,
        ErrorMessage = "Account name must be 2-200 characters")]
    public string AccountName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Industry must be max 100 characters")]
    public string? Industry { get; set; }

    [Url(ErrorMessage = "Website URL must be a valid URL")]
    public string? WebsiteUrl { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string? ContactEmail { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? ContactPhone { get; set; }

    [Range(0, 999999999.99, ErrorMessage = "Annual revenue must be between 0 and 999,999,999.99")]
    public decimal? AnnualRevenue { get; set; }

    [EnumDataType(typeof(AccountStatus), ErrorMessage = "Invalid account status")]
    public string? Status { get; set; }
}
```

**DataAnnotations Supported:**
- `[Required]` - Field must have value
- `[StringLength(max, min)]` - String length bounds
- `[EmailAddress]` - Valid email format
- `[Phone]` - Valid phone format
- `[Url]` - Valid URL format
- `[Range(min, max)]` - Numeric range
- `[RegularExpression(pattern)]` - Regex pattern
- `[DataType(type)]` - Data type validation
- `[EnumDataType(enum)]` - Valid enum value
- `[CreditCard]` - Credit card number
- `[Timestamp]` - Valid timestamp format

### 2.2 Layer 2: FluentValidation (Service Level)

Used for **complex, business logic validations**.

```csharp
namespace CRM.Infrastructure.Validators;

/// <summary>
/// Validator for CreateAccountDto
/// Validates business rules, async rules, and complex conditions
/// </summary>
public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<CreateAccountDtoValidator> _logger;

    public CreateAccountDtoValidator(
        IAccountRepository accountRepository,
        ILogger<CreateAccountDtoValidator> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;

        // Basic rules
        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required")
            .Length(2, 200).WithMessage("Account name must be 2-200 characters")
            .Must(x => !ContainsInvalidCharacters(x))
                .WithMessage("Account name contains invalid characters");

        RuleFor(x => x.Industry)
            .Must(x => x == null || IsValidIndustry(x))
                .WithMessage("'{PropertyName}' is not a valid industry");

        // Async validation - check uniqueness
        RuleFor(x => x.AccountName)
            .MustAsync(async (name, ct) =>
            {
                var exists = await _accountRepository.ExistsByNameAsync(name, ct);
                return !exists;
            })
            .WithMessage("An account with this name already exists")
            .When(x => !string.IsNullOrEmpty(x.AccountName));

        // Parent account validation (async)
        RuleFor(x => x.ParentAccountId)
            .MustAsync(async (id, ct) =>
            {
                if (id == null) return true;  // Optional
                var exists = await _accountRepository.ExistsAsync(id.Value, ct);
                return exists;
            })
            .WithMessage("Parent account does not exist");

        // Circular hierarchy check
        RuleFor(x => x.ParentAccountId)
            .Custom(async (parentId, context) =>
            {
                if (!parentId.HasValue) return;

                var isCircular = await _accountRepository.IsCircularHierarchyAsync(
                    parentId.Value, context.InstanceToValidate.ParentAccountId, default);

                if (isCircular)
                {
                    context.AddFailure(
                        nameof(CreateAccountDto.ParentAccountId),
                        "Cannot create circular account hierarchy");
                }
            });
    }

    private bool ContainsInvalidCharacters(string name)
    {
        var invalidChars = new[] { '<', '>', '\\', '"', '|', '?', '*' };
        return name.Any(c => invalidChars.Contains(c));
    }

    private bool IsValidIndustry(string? industry)
    {
        var validIndustries = new[] { "Technology", "Finance", "Healthcare", "Manufacturing" };
        return string.IsNullOrEmpty(industry) || validIndustries.Contains(industry);
    }
}
```

### 2.3 Layer 3: Entity Validation (Domain Model)

Optional validation in entities:

```csharp
/// <summary>
/// Domain entity with embedded validation rules
/// These are BUSINESS RULES that must always be true
/// </summary>
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }

    /// <summary>
    /// Validates state transitions
    /// Can be called before SaveChangesAsync
    /// </summary>
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        // Business rule: Due date must be after invoice date
        if (DueDate < InvoiceDate)
        {
            errors.Add("Due date must be after invoice date");
        }

        // Business rule: Cannot modify paid invoices
        if (Status == InvoiceStatus.Paid && AmountPaid < TotalAmount)
        {
            errors.Add("Cannot change paid invoice amounts");
        }

        // Business rule: Amount paid cannot exceed total
        if (AmountPaid > TotalAmount)
        {
            errors.Add("Amount paid cannot exceed total amount");
        }

        if (errors.Count > 0)
        {
            return ValidationResult.Failure(errors);
        }

        return ValidationResult.Success();
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public List<string> Errors { get; private set; } = new();

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(params string[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };
}
```

### 2.4 Layer 4: Service Validation

Validation in service methods before persisting data:

```csharp
public class AccountService : IAccountService
{
    private readonly IValidator<CreateAccountDto> _validator;
    private readonly IRepository<Account> _repository;

    public AccountService(
        IValidator<CreateAccountDto> validator,
        IRepository<Account> repository)
    {
        _validator = validator;
        _repository = repository;
    }

    public async Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken ct)
    {
        // ✅ VALIDATE INPUT
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            // Throw with field-level errors
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray());
            throw new ValidationException("Validation failed", errors);
        }

        // ✅ VALIDATE BUSINESS RULES
        if (await _repository.ExistsByNameAsync(dto.AccountName, ct))
        {
            throw new ConflictException($"Account named '{dto.AccountName}' already exists");
        }

        if (dto.ParentAccountId.HasValue)
        {
            var parentExists = await _repository.ExistsAsync(dto.ParentAccountId.Value, ct);
            if (!parentExists)
            {
                throw new BusinessRuleException(
                    "ParentAccountMustExist",
                    "Parent account does not exist");
            }
        }

        // ✅ CREATE AND PERSIST
        var account = new Account
        {
            AccountName = dto.AccountName,
            Industry = dto.Industry,
            WebsiteUrl = dto.WebsiteUrl,
            ParentAccountId = dto.ParentAccountId,
            Status = dto.Status ?? "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(account, ct);

        return _mapper.Map<AccountDto>(account);
    }
}
```

---

## 3. Validation Rule Patterns

### 3.1 Required Field Pattern

```csharp
public class CreateContactDtoValidator : AbstractValidator<CreateContactDto>
{
    public CreateContactDtoValidator()
    {
        // ✅ REQUIRED - Field must have value
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .NotNull();  // Extra check for null

        // ✅ OPTIONAL - Field can be null/empty
        RuleFor(x => x.MiddleName)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.MiddleName));
    }
}
```

### 3.2 Conditional Validation

```csharp
public class UpdateInvoiceDtoValidator : AbstractValidator<UpdateInvoiceDto>
{
    public UpdateInvoiceDtoValidator(IInvoiceRepository invoiceRepository)
    {
        // Only validate due date if status is being set to something other than Paid
        RuleFor(x => x.DueDate)
            .NotEmpty()
            .When(x => x.Status != InvoiceStatus.Paid.ToString())
            .WithMessage("Due date is required for unpaid invoices");

        // Cannot modify amounts if status is Paid
        RuleFor(x => x.TotalAmount)
            .Must((dto, amount) => dto.Status != InvoiceStatus.Paid.ToString())
            .WithMessage("Cannot modify amounts on paid invoices");
    }
}
```

### 3.3 Cross-Field Validation

```csharp
public class CreateQuoteDtoValidator : AbstractValidator<CreateQuoteDto>
{
    public CreateQuoteDtoValidator()
    {
        // ExpiryDate must be after QuoteDate
        RuleFor(x => x)
            .Must(x => x.ExpiryDate > x.QuoteDate)
            .WithMessage("Expiry date must be after quote date")
            .WithName("ExpiryDate");

        // At least one line item is required
        RuleFor(x => x.LineItems)
            .NotEmpty().WithMessage("Quote must have at least one line item")
            .Must(items => items.Sum(i => i.Quantity) > 0)
            .WithMessage("Total quantity must be greater than zero");
    }
}
```

### 3.4 Async Validation Pattern

```csharp
public class CreateContactDtoValidator : AbstractValidator<CreateContactDto>
{
    private readonly IContactRepository _contactRepository;

    public CreateContactDtoValidator(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;

        // ✅ ASYNC CHECK - Verify email doesn't already exist
        RuleFor(x => x.EmailPrimary)
            .MustAsync(async (email, ct) =>
            {
                if (string.IsNullOrEmpty(email)) return true;  // Optional
                var exists = await _contactRepository.ExistsByEmailAsync(email, ct);
                return !exists;  // Return true if validation passes
            })
            .WithMessage("This email address is already registered")
            .When(x => !string.IsNullOrEmpty(x.EmailPrimary));

        // ✅ ASYNC CHECK - Verify account exists
        RuleFor(x => x.AccountId)
            .MustAsync(async (id, ct) =>
            {
                if (!id.HasValue) return true;  // Optional
                var exists = await _contactRepository.AccountExistsAsync(id.Value, ct);
                return exists;
            })
            .WithMessage("Account does not exist")
            .When(x => x.AccountId.HasValue);
    }
}
```

### 3.5 Collection Validation

```csharp
public class CreateInvoiceDtoValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceDtoValidator()
    {
        // All line items must be valid
        RuleForEach(x => x.LineItems)
            .SetValidator(new CreateInvoiceLineItemDtoValidator());

        // Total items must match sum of line items
        RuleFor(x => x)
            .Must(x => ValidateLineItemsTotal(x))
            .WithMessage("Invoice total must equal sum of line items");
    }

    private bool ValidateLineItemsTotal(CreateInvoiceDto dto)
    {
        if (dto.LineItems == null || dto.LineItems.Count == 0)
            return false;

        var calculatedTotal = dto.LineItems.Sum(li =>
            (li.Quantity * li.UnitPrice) - li.DiscountAmount + li.TaxAmount);

        return Math.Abs(calculatedTotal - dto.Subtotal) < 0.01m;  // Allow for rounding
    }
}

public class CreateInvoiceLineItemDtoValidator : AbstractValidator<CreateInvoiceLineItemDto>
{
    public CreateInvoiceLineItemDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Line item description is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");
    }
}
```

---

## 4. Custom Validation Rules

### 4.1 Custom Validator Method

```csharp
public class AccountSummaryDtoValidator : AbstractValidator<CreateAccountDto>
{
    public AccountSummaryDtoValidator()
    {
        // ✅ Custom validation method
        RuleFor(x => x.AccountName)
            .Must(BeValidAccountName)
            .WithMessage("Account name contains invalid characters");

        RuleFor(x => x.Industry)
            .Must(x => x == null || BeValidIndustry(x))
            .WithMessage("'{PropertyValue}' is not a recognized industry");
    }

    private bool BeValidAccountName(string name)
    {
        // Check for SQL injection patterns
        var dangerousPatterns = new[] { "--", "/*", "*/", ";", "'" };
        return !dangerousPatterns.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private bool BeValidIndustry(string industry)
    {
        var validIndustries = new[]
        {
            "Technology", "Finance", "Healthcare", "Manufacturing",
            "Retail", "Telecommunications", "Energy", "Other"
        };
        return validIndustries.Contains(industry);
    }
}
```

### 4.2 Custom AsyncValidator Method

```csharp
public class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
{
    private readonly IAccountRepository _accountRepository;

    public UpdateAccountDtoValidator(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;

        // ✅ Custom async validation
        RuleFor(x => x.ParentAccountId)
            .MustAsync(NotCreateCircularHierarchy)
            .WithMessage("Cannot create circular account hierarchy")
            .When(x => x.ParentAccountId.HasValue);
    }

    private async Task<bool> NotCreateCircularHierarchy(int? parentId, CancellationToken ct)
    {
        if (!parentId.HasValue) return true;

        // Check if setting parentId would create a cycle
        var isCircular = await _accountRepository.IsCircularHierarchyAsync(
            parentId.Value,
            // Current account ID - need from context
            cancellationToken: ct);

        return !isCircular;
    }
}
```

---

## 5. Error Message Standards

### 5.1 Clear, Actionable Messages

| ❌ Bad Message | ✅ Good Message | Why Better |
|---|---|---|
| "Validation failed" | "Email address is required" | Specific, tells what's wrong |
| "Invalid input" | "Email must be a valid email address" | Clear action to fix |
| "Error in field" | "Phone number must be in format (XXX) XXX-XXXX" | Provides expected format |
| "Cannot proceed" | "Account with this name already exists" | Explains reason |

### 5.2 Error Message Template

```
{FieldName} {condition} {details}.
```

**Examples:**
- "Email must be a valid email address."
- "Account name must be 2-200 characters."
- "Account with this name already exists."
- "Parent account does not exist."
- "Due date must be after invoice date."

### 5.3 Implementation

```csharp
public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountDtoValidator()
    {
        RuleFor(x => x.AccountName)
            // ❌ BAD
            // .NotEmpty().WithMessage("name required")

            // ✅ GOOD
            .NotEmpty()
                .WithMessage("Account name is required")
            .Length(2, 200)
                .WithMessage("Account name must be 2-200 characters")
            .Must(x => !x.StartsWith(" ") && !x.EndsWith(" "))
                .WithMessage("Account name cannot start or end with spaces");

        RuleFor(x => x.WebsiteUrl)
            .Url()
                .WithMessage("Website URL must be a valid URL (e.g., https://example.com)");
    }
}
```

---

## 6. Validation Middleware

### 6.1 Auto-Validation in Controllers

```csharp
// ✅ RECOMMENDED - Automatic validation via behavior

// Register validation behavior
services.AddValidatorsFromAssemblyContaining<CreateAccountDtoValidator>();
services.AddMediatR(config =>
{
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// Validation behavior automatically runs validators
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validationContext = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));
        var failures = validationResults.Where(r => !r.IsValid).SelectMany(r => r.Errors).ToList();

        if (failures.Count != 0)
        {
            var errors = failures
                .GroupBy(x => x.PropertyName)
                .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray());
            throw new ValidationException("Validation failed", errors);
        }

        return await next();
    }
}
```

---

## 7. Anti-Patterns (What NOT to Do)

### ❌ Anti-Pattern 1: Validation Only in Database
```csharp
// ❌ WRONG - No validation, rely on database
public async Task<AccountDto> CreateAsync(CreateAccountDto dto)
{
    var account = _mapper.Map<Account>(dto);
    await _dbContext.Accounts.AddAsync(account);  // ❌ No validation!
    await _dbContext.SaveChangesAsync();
    return _mapper.Map<AccountDto>(account);
}

// ✅ CORRECT - Validate before persistence
public async Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken ct)
{
    var validationResult = await _validator.ValidateAsync(dto, ct);
    if (!validationResult.IsValid)
        throw new ValidationException("Validation failed", errors);

    // ...rest of logic
}
```

### ❌ Anti-Pattern 2: Validation in Entity Constructor
```csharp
// ❌ WRONG - Validation exceptions in constructors
public class Account
{
    public Account(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name required");  // Unexpected!
    }
}

// ✅ CORRECT - Factory method or service handles validation
public class Account
{
    public string AccountName { get; set; } = string.Empty;

    // Factory method (optional)
    public static Account Create(string accountName)
    {
        if (string.IsNullOrEmpty(accountName))
            throw new ArgumentException("Name required");
        return new Account { AccountName = accountName };
    }
}

// Service validates
public async Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken ct)
{
    var validationResult = await _validator.ValidateAsync(dto, ct);
    if (!validationResult.IsValid)
        throw new ValidationException("Validation failed", errors);
    
    var account = new Account { AccountName = dto.AccountName };
    await _repository.AddAsync(account, ct);
    return _mapper.Map<AccountDto>(account);
}
```

### ❌ Anti-Pattern 3: Cryptic Error Messages
```csharp
// ❌ WRONG - Unhelpful errors
WithMessage("Invalid value")
WithMessage("Failed")
WithMessage("Error in input")

// ✅ CORRECT - Clear, actionable messages
WithMessage("Email address must be a valid email format (e.g., user@example.com)")
WithMessage("Account name is required")
WithMessage("Annual revenue must be between 0 and 999,999,999.99")
```

### ❌ Anti-Pattern 4: Silent Validation Failures
```csharp
// ❌ WRONG - Validation fails silently, no error thrown
var validationResult = await _validator.ValidateAsync(dto, ct);
if (!validationResult.IsValid)
{
    // Just log, don't throw
    _logger.LogError("Validation failed");
    // Continues with invalid data!
}

// ✅ CORRECT - Throw exception to stop processing
var validationResult = await _validator.ValidateAsync(dto, ct);
if (!validationResult.IsValid)
{
    var errors = validationResult.Errors
        .GroupBy(x => x.PropertyName)
        .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray());
    throw new ValidationException("Validation failed", errors);
}
```

### ❌ Anti-Pattern 5: Validation in Database Queries
```csharp
// ❌ WRONG - Duplicating validation logic
var isValid = await _dbContext.Accounts
    .Where(a => a.AccountName.Length >= 2 && a.AccountName.Length <= 200)
    .AnyAsync();

// ✅ CORRECT - Single validation rule
RuleFor(x => x.AccountName)
    .Length(2, 200)
    .WithMessage("Account name must be 2-200 characters");
```

---

## 8. Real CRM Examples

### 8.1 Account DTO Validators

```csharp
public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    private readonly IAccountRepository _repository;

    public CreateAccountDtoValidator(IAccountRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required")
            .Length(2, 200).WithMessage("Account name must be 2-200 characters")
            .MustAsync(async (name, ct) => !await _repository.ExistsByNameAsync(name, ct))
                .WithMessage("Account with this name already exists");

        RuleFor(x => x.Industry)
            .IsInEnum().WithMessage("Invalid industry selection");

        RuleFor(x => x.WebsiteUrl)
            .Url().WithMessage("Website URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl));

        RuleFor(x => x.ParentAccountId)
            .MustAsync(async (id, ct) => id == null || await _repository.ExistsAsync(id.Value, ct))
                .WithMessage("Parent account does not exist");
    }
}

public class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
{
    private readonly IAccountRepository _repository;

    public UpdateAccountDtoValidator(IAccountRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.AccountName)
            .Length(2, 200).WithMessage("Account name must be 2-200 characters")
            .When(x => !string.IsNullOrEmpty(x.AccountName));

        RuleFor(x => x.WebsiteUrl)
            .Url().WithMessage("Website URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl));
    }
}
```

### 8.2 Invoice DTO Validators

```csharp
public class CreateInvoiceDtoValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceDtoValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account is required")
            .GreaterThan(0);

        RuleFor(x => x.InvoiceDate)
            .NotEmpty().WithMessage("Invoice date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Invoice date cannot be in the future");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required")
            .GreaterThan(x => x.InvoiceDate).WithMessage("Due date must be after invoice date");

        RuleFor(x => x.Subtotal)
            .GreaterThanOrEqualTo(0).WithMessage("Subtotal cannot be negative");

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative");

        RuleForEach(x => x.LineItems)
            .SetValidator(new CreateInvoiceLineItemDtoValidator());

        RuleFor(x => x)
            .Must(ValidateLineItemsPresent)
            .WithMessage("Invoice must have at least one line item");
    }

    private bool ValidateLineItemsPresent(CreateInvoiceDto dto)
    {
        return dto.LineItems != null && dto.LineItems.Count > 0;
    }
}
```

---

## 9. Implementation Checklist

- [ ] All DTOs have DataAnnotations for basic validation
- [ ] FluentValidation validators created for complex rules
- [ ] Async validators for database-dependent checks
- [ ] Custom validation rules for business logic
- [ ] Cross-field validation where needed
- [ ] Collection validation for list properties
- [ ] Clear, actionable error messages
- [ ] Validators registered in DI container
- [ ] Services call validators before persistence
- [ ] Validation exceptions thrown to middleware
- [ ] No validation in constructors or entities
- [ ] No silent validation failures
- [ ] Integration tests verify validation behavior
- [ ] Documentation of validation rules per DTO

---

## 10. Existing Code Compliance

The CRM solution **already implements** comprehensive validation:

**Implemented:**
- ✅ DataAnnotations on all DTOs
- ✅ FluentValidation validators for complex rules
- ✅ Async validators for database checks
- ✅ Custom validation rules for business logic
- ✅ Validators registered in DI container
- ✅ Clear error messages

**To Evolve:**
- [ ] Ensure all services validate before persistence
- [ ] Document validation rules per module
- [ ] Add more async validators for FK checks

---

## 11. TODO Items

| TODO ID | Description | Priority |
|---------|-------------|----------|
| TODO-ARCH-005-001 | Create shared validation rule library | P2 |
| TODO-ARCH-005-002 | Add validation error localization support | P3 |
| TODO-ARCH-005-003 | Document validation rules per DTO | P2 |
| TODO-ARCH-005-004 | Add validation performance tests | P2 |

---

## Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Feb 16, 2026 | Architecture Team | Initial specification from CRM validation implementation |

---

## Related Specifications

- **[SPEC-ARCH-001: DTO Standardization](SPEC-ARCH-001-DTOStandard.md)** - Validation in DTOs
- **[SPEC-ARCH-002: Error Handling](SPEC-ARCH-002-ErrorHandlingStrategy.md)** - Throws ValidationException
- **[SPEC-ARCH-003: Dependency Injection](SPEC-ARCH-003-DependencyInjectionPatterns.md)** - Validators registered

---

**END OF SPECIFICATION**
