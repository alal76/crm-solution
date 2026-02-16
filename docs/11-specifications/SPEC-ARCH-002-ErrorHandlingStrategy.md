# Architecture Specification: Error Handling Strategy

> **Spec ID:** SPEC-ARCH-002  
> **Feature:** Comprehensive Error Handling Strategy  
> **Module:** Architecture  
> **Version:** 1.0  
> **Last Updated:** February 16, 2026  
> **Status:** ✅ Implemented (Reference Standard)  
> **Priority:** P0 (Foundational)  
> **Author:** Architecture Team  
> **Cross-References:** [SPEC-ARCH-001](SPEC-ARCH-001-DTOStandard.md) (ApiResponse wrapper), [SPEC-ARCH-002](SPEC-ARCH-002-ErrorHandlingStrategy.md)

---

## Executive Summary

Error handling is a **critical cross-cutting concern** in enterprise APIs. Inconsistent error responses lead to:
- Confused frontend developers (unclear error codes)
- Poor user experiences (cryptic error messages)
- Difficult debugging (missing context)
- Integration failures (variable response formats)

This specification establishes **ONE STANDARD** exception hierarchy, HTTP status mapping, and error response format used throughout the CRM API.

**Key Principle:** "Make the error so obvious that the developer knows exactly what went wrong and how to fix it."

---

## 1. Business Context

### 1.1 Feature Description

Error handling encompasses:
1. **Exception Hierarchy** - Typed exceptions with semantic meaning
2. **HTTP Status Mapping** - Correct HTTP codes for each error condition
3. **Error Response Format** - Consistent JSON structure in all error responses
4. **Global Error Handler** - Middleware that catches ALL exceptions
5. **Logging** - Contextual information for debugging
6. **Client-Friendly Messages** - Clear guidance for error resolution

### 1.2 Standards Defined

| Standard | Purpose | Examples |
|----------|---------|----------|
| **Exception Hierarchy** | Typed exceptions for different error scenarios | `EntityNotFoundException`, `ValidationException`, `BusinessRuleException` |
| **HTTP Status Codes** | Correct HTTP response codes | 400, 401, 403, 404, 409, 422, 500 |
| **Error Response Structure** | Consistent JSON error format | `{ success: false, error: { code, message, details } }` |
| **Error Codes** | Machine-readable error identifiers | `VALIDATION_ERROR`, `ENTITY_NOT_FOUND`, `UNAUTHORIZED` |
| **Error Messages** | User-friendly error descriptions | "Account with ID 123 not found" |
| **Validation Errors** | Field-level error details | `{ fieldName: ["error1", "error2"] }` |

### 1.3 Use Cases

| UC-ID | Use Case | Exception | HTTP Code | Status |
|-------|----------|-----------|-----------|--------|
| UC-001 | Entity not found | `EntityNotFoundException` | 404 | ✅ |
| UC-002 | Validation failed | `ValidationException` | 400 | ✅ |
| UC-003 | Authorization denied | `AuthorizationException` | 403 | ✅ |
| UC-004 | Authentication failed | `AuthenticationException` | 401 | ✅ |
| UC-005 | Business rule violated | `BusinessRuleException` | 422 | ✅ |
| UC-006 | Duplicate entity | `DuplicateEntityException` | 409 | ✅ |
| UC-007 | Concurrency conflict | `ConcurrencyException` | 409 | ✅ |
| UC-008 | Rate limit exceeded | `RateLimitException` | 429 | ✅ |
| UC-009 | External service error | `ExternalServiceException` | 502 | ✅ |
| UC-010 | Internal error | Unhandled exception | 500 | ✅ |

---

## 2. Exception Type Hierarchy

### 2.1 Base Exception Class

All CRM exceptions inherit from `CrmException`:

```csharp
using System.Net;

namespace CRM.Core.Exceptions;

/// <summary>
/// Base exception for all CRM-specific exceptions
/// Provides HTTP status code and error codes for consistent API responses
/// </summary>
public abstract class CrmException : Exception
{
    /// <summary>
    /// HTTP status code to return for this exception type
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Machine-readable error code for client-side error handling
    /// Format: SCREAMING_SNAKE_CASE (e.g., ENTITY_NOT_FOUND)
    /// </summary>
    public string ErrorCode { get; }

    protected CrmException(
        string message,
        HttpStatusCode statusCode,
        string errorCode,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
```

**Key Properties:**
- `StatusCode` - HTTP status code (automatically used by middleware)
- `ErrorCode` - Machine-readable code for client-side handling
- `Message` - User-friendly error description
- `InnerException` - Original exception (logged but not sent to client)

### 2.2 Exception Types

#### 1. EntityNotFoundException (404)
When a requested entity doesn't exist:

```csharp
/// <summary>
/// Exception thrown when an entity is not found
/// HTTP Response: 404 Not Found
/// </summary>
public class EntityNotFoundException : CrmException
{
    public string EntityType { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityType, object? entityId = null)
        : base(
            $"{entityType} not found" + (entityId != null ? $" with ID: {entityId}" : ""),
            HttpStatusCode.NotFound,
            "ENTITY_NOT_FOUND")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
```

**Usage:**
```csharp
// Throw when account not found
var account = await _repository.GetByIdAsync(id);
if (account == null)
{
    throw new EntityNotFoundException("Account", id);
}
```

#### 2. ValidationException (400)
When input validation fails:

```csharp
/// <summary>
/// Exception thrown when validation fails
/// HTTP Response: 400 Bad Request
/// Includes field-level error details
/// </summary>
public class ValidationException : CrmException
{
    /// <summary>
    /// Field-level validation errors
    /// Key: field name, Value: array of error messages for that field
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(
        string message,
        IDictionary<string, string[]>? errors = null)
        : base(message, HttpStatusCode.BadRequest, "VALIDATION_ERROR")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ValidationException(string field, string error)
        : this(
            $"Validation failed for {field}: {error}",
            new Dictionary<string, string[]> { { field, new[] { error } } })
    {
    }
}
```

**Usage:**
```csharp
// Single field error
throw new ValidationException("email", "Invalid email format");

// Multiple field errors
var errors = new Dictionary<string, string[]>
{
    { "firstName", new[] { "First name is required" } },
    { "email", new[] { "Invalid email format", "Email already in use" } }
};
throw new ValidationException("Validation failed", errors);

// From FluentValidation
var validationResult = await validator.ValidateAsync(dto);
if (!validationResult.IsValid)
{
    var errors = validationResult.Errors
        .GroupBy(x => x.PropertyName)
        .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray());
    throw new ValidationException("Validation failed", errors);
}
```

#### 3. BusinessRuleException (422)
When a business rule is violated:

```csharp
/// <summary>
/// Exception thrown when a business rule is violated
/// HTTP Response: 422 Unprocessable Entity
/// Used for constraint violations that aren't simple validation failures
/// </summary>
public class BusinessRuleException : CrmException
{
    /// <summary>
    /// Name of the business rule that was violated
    /// </summary>
    public string RuleName { get; }

    public BusinessRuleException(string ruleName, string message)
        : base(message, HttpStatusCode.UnprocessableEntity, "BUSINESS_RULE_VIOLATION")
    {
        RuleName = ruleName;
    }
}
```

**Usage:**
```csharp
// Invoice status transition rule
if (invoice.Status == "Paid" && amount > 0)
{
    throw new BusinessRuleException(
        "InvoiceStatusTransition",
        "Cannot modify amounts on paid invoices");
}

// Duplicate phone number
if (await _contactService.PhoneNumberExistsAsync(phoneNumber))
{
    throw new BusinessRuleException(
        "DuplicatePhoneNumber",
        "This phone number is already registered");
}

// Circular hierarchy
if (parentId == accountId || IsCircularHierarchy(parentId, accountId))
{
    throw new BusinessRuleException(
        "CircularHierarchy",
        "Cannot create circular account hierarchy");
}
```

#### 4. AuthenticationException (401)
When authentication fails:

```csharp
/// <summary>
/// Exception thrown when authentication fails
/// HTTP Response: 401 Unauthorized
/// Indicates user identity cannot be verified
/// </summary>
public class AuthenticationException : CrmException
{
    public AuthenticationException(string message)
        : base(message, HttpStatusCode.Unauthorized, "AUTHENTICATION_FAILED")
    {
    }
}
```

**Usage:**
```csharp
// Invalid credentials
if (!BCrypt.Net.BCrypt.Verify(password, hashedPassword))
{
    throw new AuthenticationException("Invalid email or password");
}

// Token expired
if (jwtToken.Expiry < DateTime.UtcNow)
{
    throw new AuthenticationException("Token has expired");
}

// Missing token
if (string.IsNullOrEmpty(token))
{
    throw new AuthenticationException("Authorization header required");
}
```

#### 5. AuthorizationException (403)
When user lacks required permissions:

```csharp
/// <summary>
/// Exception thrown when authorization fails
/// HTTP Response: 403 Forbidden
/// Indicates user is authenticated but lacks required permissions
/// </summary>
public class AuthorizationException : CrmException
{
    /// <summary>
    /// Permission that was required but not granted
    /// </summary>
    public string? RequiredPermission { get; }

    public AuthorizationException(
        string message,
        string? requiredPermission = null)
        : base(message, HttpStatusCode.Forbidden, "AUTHORIZATION_FAILED")
    {
        RequiredPermission = requiredPermission;
    }
}
```

**Usage:**
```csharp
// Missing permission
if (!user.HasPermission("Invoice.Delete"))
{
    throw new AuthorizationException(
        "You do not have permission to delete invoices",
        "Invoice.Delete");
}

// Insufficient role
if (user.Role != "Admin" && user.Role != "Manager")
{
    throw new AuthorizationException("Admin access required");
}
```

#### 6. ConflictException (409)
When resource state conflicts with operation:

```csharp
/// <summary>
/// Exception thrown when a request conflicts with current state
/// HTTP Response: 409 Conflict
/// Used for duplicate resources or state conflicts
/// </summary>
public class ConflictException : CrmException
{
    /// <summary>
    /// The conflicting resource identifier
    /// </summary>
    public object? ConflictingResource { get; }

    public ConflictException(string message, object? conflictingResource = null)
        : base(message, HttpStatusCode.Conflict, "CONFLICT")
    {
        ConflictingResource = conflictingResource;
    }
}
```

**Usage:**
```csharp
// Duplicate resource
if (await _repository.ExistsByEmailAsync(email))
{
    throw new ConflictException($"Account with email {email} already exists");
}

// State conflict
if (order.Status == "Cancelled")
{
    throw new ConflictException("Cannot modify cancelled orders");
}
```

#### 7. ConcurrencyException (409)
When concurrent modification detected:

```csharp
/// <summary>
/// Exception thrown when optimistic concurrency check fails
/// HTTP Response: 409 Conflict
/// Indicates entity was modified by another user since read
/// </summary>
public class ConcurrencyException : CrmException
{
    public ConcurrencyException(string entityType, object entityId)
        : base(
            $"{entityType} (ID: {entityId}) was modified by another user. Please refresh and try again.",
            HttpStatusCode.Conflict,
            "CONCURRENCY_CONFLICT")
    {
    }
}
```

**Usage:**
```csharp
// EF Core concurrency exception
try
{
    await _dbContext.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    var entity = ex.Entries.First();
    throw new ConcurrencyException(entity.Entity.GetType().Name, entity.CurrentValues["Id"]);
}
```

#### 8. RateLimitException (429)
When rate limit exceeded:

```csharp
/// <summary>
/// Exception thrown when API rate limit is exceeded
/// HTTP Response: 429 Too Many Requests
/// </summary>
public class RateLimitException : CrmException
{
    /// <summary>
    /// Seconds until rate limit resets
    /// </summary>
    public int? RetryAfterSeconds { get; }

    public RateLimitException(int? retryAfterSeconds = null)
        : base(
            "API rate limit exceeded. Please try again later.",
            HttpStatusCode.TooManyRequests,
            "RATE_LIMIT_EXCEEDED")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
```

**Usage:**
```csharp
// In rate limiting middleware
if (remaining <= 0)
{
    var resetTime = bucket.ResetTime - DateTime.UtcNow;
    throw new RateLimitException((int?)resetTime.TotalSeconds);
}
```

#### 9. ExternalServiceException (502)
When external service fails:

```csharp
/// <summary>
/// Exception thrown when an external service call fails
/// HTTP Response: 502 Bad Gateway
/// </summary>
public class ExternalServiceException : CrmException
{
    /// <summary>
    /// Name of the external service
    /// </summary>
    public string ServiceName { get; }

    public ExternalServiceException(
        string serviceName,
        string message,
        Exception? innerException = null)
        : base(
            $"{serviceName} service error: {message}",
            HttpStatusCode.BadGateway,
            "EXTERNAL_SERVICE_ERROR",
            innerException)
    {
        ServiceName = serviceName;
    }
}
```

**Usage:**
```csharp
// External API call failed
try
{
    var result = await _paymentGateway.ChargeAsync(amount);
}
catch (HttpRequestException ex)
{
    throw new ExternalServiceException("PaymentGateway", "Payment processing failed", ex);
}

// Email service fail
try
{
    await _emailService.SendAsync(email);
}
catch (Exception ex)
{
    throw new ExternalServiceException("EmailService", "Failed to send email", ex);
}
```

#### 10. ServiceException (500)
For internal service errors:

```csharp
/// <summary>
/// Exception thrown when internal service operation fails
/// HTTP Response: 500 Internal Server Error
/// </summary>
public class ServiceException : CrmException
{
    public ServiceException(string message, Exception? innerException = null)
        : base(message, HttpStatusCode.InternalServerError, "SERVICE_ERROR", innerException)
    {
    }
}
```

**Usage:**
```csharp
// Unexpected service error
try
{
    var result = await _businessLogic.ProcessAsync();
}
catch (Exception ex)
{
    throw new ServiceException("Failed to process order. Please contact support.", ex);
}
```

#### 11. ConfigurationException (500)
For configuration errors:

```csharp
/// <summary>
/// Exception thrown when application configuration is invalid
/// HTTP Response: 500 Internal Server Error
/// </summary>
public class ConfigurationException : CrmException
{
    public ConfigurationException(string setting, string message)
        : base(
            $"Configuration error for '{setting}': {message}",
            HttpStatusCode.InternalServerError,
            "CONFIGURATION_ERROR")
    {
    }
}
```

**Usage:**
```csharp
// Missing configuration
var jwtSecret = configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new ConfigurationException("Jwt:Secret", "JWT secret not configured");
}
```

---

## 3. HTTP Status Code Mapping

### 3.1 Status Code Decision Tree

| HTTP Code | Semantics | When to Use | Exception |
|-----------|-----------|------------|-----------|
| **400 Bad Request** | Client error in request format/validation | Input data invalid | `ValidationException` |
| **401 Unauthorized** | User not authenticated | No/expired token, invalid credentials | `AuthenticationException` |
| **403 Forbidden** | User authenticated but lacks permission | User can't perform action | `AuthorizationException` |
| **404 Not Found** | Resource doesn't exist | Entity lookup fails | `EntityNotFoundException` |
| **409 Conflict** | Request conflicts with current state | Duplicate resource, state conflict | `ConflictException`, `ConcurrencyException` |
| **422 Unprocessable Entity** | Request well-formed but violates business rules | Semantic/business rule violation | `BusinessRuleException` |
| **429 Too Many Requests** | Client rate limit exceeded | API quota exceeded | `RateLimitException` |
| **500 Internal Server Error** | Server error (unexpected) | Unhandled exception | `ServiceException`, catch-all |
| **502 Bad Gateway** | Upstream service unavailable | External service fails | `ExternalServiceException` |
| **503 Service Unavailable** | Server temporarily unavailable | Maintenance, overload | Middleware handler |

### 3.2 Quick Reference

```csharp
// Common mappings
EntityNotFoundException       → 404 Not Found
ValidationException          → 400 Bad Request
AuthenticationException       → 401 Unauthorized
AuthorizationException        → 403 Forbidden
ConflictException            → 409 Conflict
ConcurrencyException         → 409 Conflict
BusinessRuleException        → 422 Unprocessable Entity
RateLimitException           → 429 Too Many Requests
ExternalServiceException     → 502 Bad Gateway
ServiceException             → 500 Internal Server Error
ConfigurationException       → 500 Internal Server Error
```

---

## 4. Error Response Format

### 4.1 Success Response

```json
{
  "success": true,
  "data": { /* response data */ },
  "message": null,
  "error": null
}
```

### 4.2 Validation Error Response (400)

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": {
      "firstName": [
        "First name is required",
        "First name must be at least 2 characters"
      ],
      "emailPrimary": [
        "Invalid email address format"
      ],
      "phone": [
        "Phone number format is invalid"
      ]
    }
  }
}
```

**Response Code:** `400 Bad Request`
**Headers:** `Content-Type: application/json`

### 4.3 Entity Not Found Response (404)

```json
{
  "success": false,
  "data": null,
  "message": "Account not found",
  "error": {
    "code": "ENTITY_NOT_FOUND",
    "message": "Account not found with ID: 999",
    "details": null
  }
}
```

**Response Code:** `404 Not Found`

### 4.4 Authorization Error Response (403)

```json
{
  "success": false,
  "data": null,
  "message": "Access denied",
  "error": {
    "code": "AUTHORIZATION_FAILED",
    "message": "You do not have permission to delete invoices. Required permission: Invoice.Delete",
    "details": null
  }
}
```

**Response Code:** `403 Forbidden`

### 4.5 Business Rule Violation Response (422)

```json
{
  "success": false,
  "data": null,
  "message": "Business rule violated",
  "error": {
    "code": "BUSINESS_RULE_VIOLATION",
    "message": "Cannot modify amounts on paid invoices",
    "details": {
      "rule": "InvoiceStatusTransition"
    }
  }
}
```

**Response Code:** `422 Unprocessable Entity`

### 4.6 Rate Limit Response (429)

```json
{
  "success": false,
  "data": null,
  "message": "Rate limit exceeded",
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "API rate limit exceeded. Please try again in 45 seconds.",
    "details": {
      "retryAfterSeconds": 45
    }
  }
}
```

**Response Code:** `429 Too Many Requests`
**Headers:** `Retry-After: 45`

### 4.7 External Service Error Response (502)

```json
{
  "success": false,
  "data": null,
  "message": "Service temporarily unavailable",
  "error": {
    "code": "EXTERNAL_SERVICE_ERROR",
    "message": "PaymentGateway service error: Connection timeout",
    "details": {
      "service": "PaymentGateway",
      "originalError": "Connection timeout"
    }
  }
}
```

**Response Code:** `502 Bad Gateway`

### 4.8 Internal Server Error Response (500)

```json
{
  "success": false,
  "data": null,
  "message": "Internal server error occurred",
  "error": {
    "code": "INTERNAL_ERROR",
    "message": "An unexpected error occurred. Please contact support.",
    "details": null
  }
}
```

**Response Code:** `500 Internal Server Error`
**Note:** Detailed error info logged but NOT sent to client (security)

---

## 5. Global Exception Handling Middleware

### 5.1 Global Exception Handler Middleware

```csharp
namespace CRM.Api.Middleware;

/// <summary>
/// Global exception handler middleware
/// Catches all exceptions and returns consistent error responses
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {ExceptionMessage}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>();

        // Handle CRM-specific exceptions
        if (exception is CrmException crmEx)
        {
            context.Response.StatusCode = (int)crmEx.StatusCode;
            response.Success = false;
            response.Error = new ErrorDetail
            {
                Code = crmEx.ErrorCode,
                Message = crmEx.Message
            };

            // Add validation details if applicable
            if (crmEx is ValidationException validationEx)
            {
                response.Error.Details = validationEx.Errors
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }
        // Handle Entity Framework concurrency
        else if (exception is DbUpdateConcurrencyException)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            response.Success = false;
            response.Error = new ErrorDetail
            {
                Code = "CONCURRENCY_CONFLICT",
                Message = "The record was modified by another user. Please refresh and try again."
            };
        }
        // Handle validation from FluentValidation
        else if (exception is ValidationException fluentEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            response.Success = false;
            response.Error = new ErrorDetail
            {
                Code = "VALIDATION_ERROR",
                Message = "One or more validation errors occurred",
                Details = fluentEx.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray())
            };
        }
        // Unhandled exceptions
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Success = false;
            response.Error = new ErrorDetail
            {
                Code = "INTERNAL_ERROR",
                Message = "An unexpected error occurred. Please contact support."
            };
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extension method to register the global exception handler
/// </summary>
public static class GlobalExceptionHandlingExtensions
{
    public static void UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}
```

### 5.2 Register in Program.cs

```csharp
// In Program.cs, add before app.Run()
app.UseGlobalExceptionHandling();
```

### 5.3 Response Classes

```csharp
namespace CRM.Core.Responses;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public string? Message { get; set; }
    public ErrorDetail? Error { get; set; }
}

/// <summary>
/// Error detail included in responses
/// </summary>
public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Details { get; set; }
}
```

---

## 6. Usage Examples

### 6.1 In Service Methods

```csharp
public class AccountService : IAccountService
{
    public async Task<AccountDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(id, ct);
        
        // Throw EntityNotFoundException if not found
        if (account == null)
        {
            throw new EntityNotFoundException("Account", id);
        }
        
        return _mapper.Map<AccountDto>(account);
    }

    public async Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken ct = default)
    {
        // Validate input
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray());
            throw new ValidationException("Validation failed", errors);
        }

        // Check business rule: parent account must exist
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

        // Check for duplicate  
        if (await _repository.ExistsByNameAsync(dto.AccountName, ct))
        {
            throw new ConflictException($"Account named '{dto.AccountName}' already exists");
        }

        var account = _mapper.Map<Account>(dto);
        await _repository.AddAsync(account, ct);
        
        return _mapper.Map<AccountDto>(account);
    }
}
```

### 6.2 In Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> GetById(int id, CancellationToken ct)
    {
        // Service throws EntityNotFoundException if not found
        // Middleware catches it and returns 404 response
        var account = await _accountService.GetByIdAsync(id, ct);
        
        return Ok(new ApiResponse<AccountDto> { Data = account });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AccountDto>>> Create(
        CreateAccountDto dto,
        CancellationToken ct)
    {
        // Service throws ValidationException if invalid
        // Service throws BusinessRuleException if business rules violated
        // Middleware catches and returns appropriate error response
        var account = await _accountService.CreateAsync(dto, ct);
        
        return CreatedAtAction(nameof(GetById), new { id = account.Id },
            new ApiResponse<AccountDto> { Data = account });
    }
}
```

### 6.3 Wrapping External Service Calls

```csharp
public async Task ProcessPaymentAsync(PaymentDto dto, CancellationToken ct)
{
    try
    {
        var result = await _paymentGateway.ChargeAsync(
            amount: dto.Amount,
            cardToken: dto.CardToken,
            ct: ct);
        
        // ... process result
    }
    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unavailable)
    {
        // Wrap external service error
        throw new ExternalServiceException(
            "PaymentGateway",
            "Service temporarily unavailable",
            ex);
    }
    catch (HttpRequestException ex)
    {
        throw new ExternalServiceException(
            "PaymentGateway",
            $"Payment processing failed: {ex.Message}",
            ex);
    }
    catch (TaskCanceledException ex)
    {
        throw new ExternalServiceException(
            "PaymentGateway",
            "Request timeout",
            ex);
    }
}
```

---

## 7. Anti-Patterns (What NOT to Do)

### ❌ Anti-Pattern 1: Generic Exception Messages
```csharp
// ❌ WRONG - Tells client almost nothing
throw new Exception("Error");

// ❌ WRONG - Generic message
if (account == null)
    throw new Exception("Failed to fetch data");

// ✅ CORRECT - Specific and actionable
if (account == null)
    throw new EntityNotFoundException("Account", id);
```

### ❌ Anti-Pattern 2: Leaking Exception Details
```csharp
// ❌ WRONG - Sends SQL error to client (security issue)
try
{
    await _dbContext.SaveChangesAsync();
}
catch (SqlException ex)
{
    throw new Exception(ex.Message); // Sends SQL error details!
}

// ✅ CORRECT - Wraps and hides details
try
{
    await _dbContext.SaveChangesAsync();
}
catch (SqlException ex)
{
    _logger.LogError(ex, "Database error"); // Log for debugging
    throw new ServiceException("Failed to save changes", ex); // Client-friendly
}
```

### ❌ Anti-Pattern 3: Incorrect HTTP Status Codes
```csharp
// ❌ WRONG - Business rule violation with 400 (validation)
if (!CanCancelInvoice(invoice))
    return BadRequest("Cannot cancel paid invoice");

// ✅ CORRECT - Use 422 (unprocessable entity)
if (!CanCancelInvoice(invoice))
    throw new BusinessRuleException(
        "InvoiceCancellation",
        "Cannot cancel paid invoice");
```

### ❌ Anti-Pattern 4: Inconsistent Error Response Format
```csharp
// ❌ WRONG - Different error formats
if (error1) return Ok(new { success = false, error = "message" });
if (error2) return BadRequest(new { errors = new[] { "message" } });
if (error3) return Json(new { message = "error" });

// ✅ CORRECT - Consistent format via middleware
throw new ValidationException("field", "error");
// Middleware returns:
// { success: false, error: { code, message, details: { field: [error] } } }
```

### ❌ Anti-Pattern 5: Throwing from Constructors/Properties
```csharp
// ❌ WRONG - Unpredictable places to throw
public class Account
{
    public Account(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ValidationException("Name is required"); // Unexpected!
    }
}

// ✅ CORRECT - Throw from service methods
public class AccountService
{
    public async Task<AccountDto> CreateAsync(CreateAccountDto dto)
    {
        if (string.IsNullOrEmpty(dto.AccountName))
            throw new ValidationException("accountName", "Name is required");
        // ...
    }
}
```

---

## 8. Implementation Checklist

- [ ] All custom exceptions inherit from `CrmException`
- [ ] Each exception type has correct `ErrorCode` (SCREAMING_SNAKE_CASE)
- [ ] Each exception type maps to correct HTTP status code
- [ ] `CrmException` includes `StatusCode` and `ErrorCode` properties
- [ ] `ValidationException` includes `Errors` dictionary
- [ ] `BusinessRuleException` includes `RuleName` property
- [ ] `AuthorizationException` includes `RequiredPermission` property
- [ ] Global exception handling middleware implemented
- [ ] Middleware catches all `CrmException` types
- [ ] Middleware handles `DbUpdateConcurrencyException`
- [ ] Middleware handles `FluentValidation` exceptions
- [ ] Middleware handles unhandled exceptions (500)
- [ ] All error responses use consistent format
- [ ] Validation errors include field-level details
- [ ] External service errors wrapped in `ExternalServiceException`
- [ ] Sensitive error details logged, not sent to client
- [ ] All endpoints return `ApiResponse<T>` wrapper
- [ ] Rate limiting exceptions return 429 with `Retry-After` header
- [ ] Documentation on exception hierarchy
- [ ] Team training on exception standards

---

## 9. Existing Code Compliance

### Current Implementation

The CRM solution **already implements** this error handling pattern:

**Implemented:**
- ✅ Complete exception hierarchy in `CRM.Core/Exceptions/CrmExceptions.cs`
- ✅ Global exception handling middleware
- ✅ Correct HTTP status code mapping
- ✅ `ApiResponse<T>` wrapper used in all endpoints
- ✅ Field-level validation errors in responses
- ✅ External service error wrapping

**In Evolution:**
- Some legacy endpoints may still use raw exceptions
- Gradual migration of external service calls to wrap errors

---

## 10. TODO Items

| TODO ID | Description | Priority |
|---------|-------------|----------|
| TODO-ARCH-002-001 | Document API error scenarios per endpoint | P2 |
| TODO-ARCH-002-002 | Add Swagger/OpenAPI error response documentation | P2 |
| TODO-ARCH-002-003 | Create client-side error handler utilities | P2 |
| TODO-ARCH-002-004 | Setup error tracking/monitoring (Sentry, etc.) | P2 |
| TODO-ARCH-002-005 | Team training on error handling standards | P3 |

---

## Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Feb 16, 2026 | Architecture Team | Initial specification from existing CRM error handling |

---

## Related Specifications

- **[SPEC-ARCH-001: DTO Standardization](SPEC-ARCH-001-DTOStandard.md)** - Uses `ApiResponse<T>` wrapper
- **[SPEC-ARCH-003: Dependency Injection](SPEC-ARCH-003-DependencyInjectionPatterns.md)** - Register validators/loggers
- **[SPEC-ARCH-005: Validation Framework](SPEC-ARCH-005-ValidationFramework.md)** - Validation throws exceptions

---

**END OF SPECIFICATION**
