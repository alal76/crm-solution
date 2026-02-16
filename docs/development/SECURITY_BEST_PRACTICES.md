# CRM Solution - Security Best Practices

**Version:** 2.0.0  
**Last Updated:** February 2, 2026  
**Classification:** Internal Use Only

---

## Table of Contents

1. [Overview](#overview)
2. [Authentication & Authorization](#authentication--authorization)
3. [Data Protection](#data-protection)
4. [Input Validation](#input-validation)
5. [API Security](#api-security)
6. [Database Security](#database-security)
7. [Infrastructure Security](#infrastructure-security)
8. [Logging & Monitoring](#logging--monitoring)
9. [Security Headers](#security-headers)
10. [Incident Response](#incident-response)
11. [Compliance Checklist](#compliance-checklist)

---

## Overview

This document outlines security best practices for the CRM Solution. All developers must follow these guidelines to protect sensitive customer data and maintain system integrity.

### Security Principles

1. **Defense in Depth**: Multiple layers of security
2. **Least Privilege**: Minimum necessary access
3. **Fail Secure**: Safe defaults on failure
4. **Complete Mediation**: Verify every access
5. **Open Design**: Security through correct implementation, not obscurity

---

## Authentication & Authorization

### JWT Token Security

```csharp
// ✅ DO: Configure secure JWT settings
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.FromMinutes(1) // Reduce default 5 min skew
        };
    });
```

### Password Requirements

```csharp
// Minimum password requirements
public class PasswordPolicy
{
    public int MinimumLength = 12;        // At least 12 characters
    public bool RequireUppercase = true;   // At least one uppercase
    public bool RequireLowercase = true;   // At least one lowercase
    public bool RequireDigit = true;       // At least one number
    public bool RequireSpecialChar = true; // At least one special char
    public int MaxAge = 90;                // Days until expiration
    public int HistoryCount = 5;           // Prevent reuse of last 5
}

// ✅ DO: Use strong password hashing
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}
```

### Multi-Factor Authentication

```csharp
// ✅ DO: Implement TOTP-based 2FA
public bool VerifyTwoFactorCode(string secret, string code)
{
    var totp = new Totp(Base32Encoding.ToBytes(secret));
    return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
}

// ✅ DO: Rate limit 2FA attempts
public async Task<bool> ValidateTwoFactorAsync(string userId, string code)
{
    var attempts = await GetRecentAttemptsAsync(userId);
    if (attempts > 5)
    {
        throw new TooManyAttemptsException("Account temporarily locked");
    }
    // Validate code...
}
```

### Role-Based Access Control

```csharp
// Define roles
public enum UserRole
{
    Admin = 0,
    Manager = 1,
    SalesRep = 2,
    Support = 3,
    ReadOnly = 4
}

// ✅ DO: Use policy-based authorization
[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> DeleteUser(int id)
{
    // Only admins can delete users
}

// Configure policies
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("CanEditAccounts", policy => 
        policy.RequireRole("Admin", "Manager", "SalesRep"));
});
```

---

## Data Protection

### Encryption at Rest

```csharp
// ✅ DO: Encrypt sensitive data before storage
public class EncryptionService
{
    private readonly byte[] _key;
    
    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = encryptor.TransformFinalBlock(
            plaintextBytes, 0, plaintextBytes.Length);
        
        // Prepend IV to ciphertext
        return Convert.ToBase64String(aes.IV.Concat(ciphertext).ToArray());
    }
}
```

### Encryption in Transit

```yaml
# ✅ DO: Enforce HTTPS in production
# docker-compose.yml
environment:
  - ASPNETCORE_URLS=https://+:443;http://+:80
  - ASPNETCORE_Kestrel__Certificates__Default__Path=/certs/server.pfx
  - ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD}
```

### Sensitive Data Handling

```csharp
// ✅ DO: Mark sensitive fields
public class User
{
    public string Email { get; set; }
    
    [PersonalData]
    public string PhoneNumber { get; set; }
    
    [ProtectedPersonalData]
    public string SocialSecurityNumber { get; set; }
}

// ✅ DO: Mask sensitive data in logs
_logger.LogInformation(
    "Processing payment for user {UserId} card ending in {CardLast4}",
    userId,
    cardNumber[^4..]); // Only log last 4 digits
```

### Data Retention

```csharp
// ✅ DO: Implement data retention policies
public async Task CleanupExpiredDataAsync()
{
    var cutoffDate = DateTime.UtcNow.AddYears(-7); // 7 year retention
    
    // Soft delete expired records
    await _dbContext.AuditLogs
        .Where(l => l.CreatedAt < cutoffDate)
        .ExecuteUpdateAsync(s => s.SetProperty(
            l => l.IsDeleted, true));
}
```

---

## Input Validation

### Server-Side Validation

```csharp
// ✅ DO: Validate all inputs
public class AccountValidator : AbstractValidator<AccountDto>
{
    public AccountValidator()
    {
        RuleFor(a => a.Name)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[\w\s\-\.]+$")
            .WithMessage("Name contains invalid characters");
        
        RuleFor(a => a.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);
        
        RuleFor(a => a.Phone)
            .Matches(@"^\+?[\d\s\-\(\)]+$")
            .When(a => !string.IsNullOrEmpty(a.Phone));
    }
}
```

### SQL Injection Prevention

```csharp
// ✅ DO: Use parameterized queries
var accounts = await _dbContext.Accounts
    .Where(a => a.Name.Contains(searchTerm))
    .ToListAsync();

// ❌ DON'T: Use string concatenation
var query = $"SELECT * FROM Accounts WHERE Name LIKE '%{searchTerm}%'"; // VULNERABLE!
```

### XSS Prevention

```typescript
// ✅ DO: Sanitize HTML content
import DOMPurify from 'dompurify';

const SafeHtml: React.FC<{ content: string }> = ({ content }) => {
  const sanitized = DOMPurify.sanitize(content, {
    ALLOWED_TAGS: ['p', 'b', 'i', 'em', 'strong', 'a'],
    ALLOWED_ATTR: ['href'],
  });
  
  return <div dangerouslySetInnerHTML={{ __html: sanitized }} />;
};

// ✅ DO: Encode output (React does this by default)
const UserName: React.FC<{ name: string }> = ({ name }) => (
  <span>{name}</span> // Automatically escaped
);
```

### File Upload Security

```csharp
// ✅ DO: Validate file uploads
public async Task<IActionResult> UploadFile(IFormFile file)
{
    // Validate file size
    if (file.Length > 10 * 1024 * 1024) // 10MB max
        return BadRequest("File too large");
    
    // Validate file extension
    var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".jpg" };
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!allowedExtensions.Contains(extension))
        return BadRequest("Invalid file type");
    
    // Validate content type matches extension
    if (!IsValidContentType(file.ContentType, extension))
        return BadRequest("Content type mismatch");
    
    // Scan for malware (if available)
    if (!await _antivirusService.ScanFileAsync(file.OpenReadStream()))
        return BadRequest("File failed security scan");
    
    // Generate safe filename
    var safeFileName = $"{Guid.NewGuid()}{extension}";
    
    // Store file...
}
```

---

## API Security

### Rate Limiting

```csharp
// ✅ DO: Implement rate limiting
public class RateLimitOptions
{
    public int MaxRequestsPerMinute { get; set; } = 100;
    public int MaxRequestsPerHour { get; set; } = 1000;
    public int MaxLoginAttemptsPerHour { get; set; } = 10;
}

// Configure in Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueLimit = 10;
    });
});
```

### CORS Configuration

```csharp
// ✅ DO: Configure CORS properly
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
            "https://crm.example.com",
            "https://app.example.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ❌ DON'T: Allow all origins in production
policy.AllowAnyOrigin(); // Only for development!
```

### API Versioning

```csharp
// ✅ DO: Version your API
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class AccountsController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public IActionResult GetV1() => Ok(/* v1 format */);
    
    [HttpGet]
    [MapToApiVersion("2.0")]
    public IActionResult GetV2() => Ok(/* v2 format */);
}
```

---

## Database Security

### Connection Security

```json
// ✅ DO: Use secure connection strings
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=db;Database=crm;User Id=app_user;Password=***;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### Principle of Least Privilege

```sql
-- ✅ DO: Create application-specific database user
CREATE USER app_user WITH PASSWORD 'secure_password';

-- Grant only necessary permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_user;

-- ❌ DON'T: Use sa/root for application
-- ❌ DON'T: Grant ALL PRIVILEGES
```

### Audit Logging

```csharp
// ✅ DO: Log all data changes
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Modified ||
                    e.State == EntityState.Added ||
                    e.State == EntityState.Deleted);
    
    foreach (var entry in entries)
    {
        var auditLog = new AuditLog
        {
            EntityType = entry.Entity.GetType().Name,
            EntityId = GetPrimaryKey(entry),
            Action = entry.State.ToString(),
            OldValues = GetOriginalValues(entry),
            NewValues = GetCurrentValues(entry),
            UserId = _currentUser.Id,
            Timestamp = DateTime.UtcNow
        };
        AuditLogs.Add(auditLog);
    }
    
    return await base.SaveChangesAsync(ct);
}
```

---

## Infrastructure Security

### Container Security

```dockerfile
# ✅ DO: Use non-root user
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CRM.Api.dll"]
```

### Secrets Management

```yaml
# ✅ DO: Use Kubernetes secrets
apiVersion: v1
kind: Secret
metadata:
  name: crm-secrets
type: Opaque
data:
  db-password: <base64-encoded>
  jwt-key: <base64-encoded>

# Reference in deployment
env:
  - name: DB_PASSWORD
    valueFrom:
      secretKeyRef:
        name: crm-secrets
        key: db-password
```

### Network Security

```yaml
# ✅ DO: Use network policies
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: api-network-policy
spec:
  podSelector:
    matchLabels:
      app: crm-api
  ingress:
    - from:
        - podSelector:
            matchLabels:
              app: crm-frontend
      ports:
        - port: 5000
```

---

## Logging & Monitoring

### Secure Logging

```csharp
// ✅ DO: Log security events
_logger.LogWarning(
    "Failed login attempt for user {Username} from IP {IpAddress}",
    username,
    HttpContext.Connection.RemoteIpAddress);

// ✅ DO: Include correlation IDs
_logger.LogInformation(
    "Request {CorrelationId}: User {UserId} accessed {Resource}",
    correlationId, userId, resource);

// ❌ DON'T: Log sensitive data
_logger.LogInformation($"User logged in with password {password}"); // NEVER!
```

### Security Monitoring

```csharp
// ✅ DO: Alert on suspicious activity
public class SecurityMonitor
{
    public async Task CheckForAnomaliesAsync()
    {
        // Multiple failed logins
        var failedLogins = await GetFailedLoginsLastHourAsync();
        if (failedLogins > 50)
        {
            await _alertService.SendAlertAsync(
                "High number of failed login attempts detected");
        }
        
        // Unusual data access patterns
        var bulkExports = await GetBulkExportsAsync();
        if (bulkExports.Any(e => e.RecordCount > 10000))
        {
            await _alertService.SendAlertAsync(
                "Large data export detected");
        }
    }
}
```

---

## Security Headers

The following headers are implemented in `SecurityHeadersMiddleware.cs`:

| Header | Value | Purpose |
|--------|-------|---------|
| X-Content-Type-Options | nosniff | Prevents MIME type sniffing |
| X-Frame-Options | SAMEORIGIN | Prevents clickjacking |
| X-XSS-Protection | 1; mode=block | Enables browser XSS filter |
| Referrer-Policy | strict-origin-when-cross-origin | Controls referrer info |
| Content-Security-Policy | Configured policy | Restricts resource loading |
| Permissions-Policy | Restrictive policy | Limits browser features |
| Strict-Transport-Security | max-age=31536000 | Enforces HTTPS |

---

## Incident Response

### Response Procedure

1. **Identify**: Detect and confirm the security incident
2. **Contain**: Limit the damage and prevent spread
3. **Eradicate**: Remove the threat
4. **Recover**: Restore systems to normal operation
5. **Learn**: Document lessons learned

### Contact Information

| Role | Contact |
|------|---------|
| Security Team | security@example.com |
| On-Call Engineer | +1-xxx-xxx-xxxx |
| Management | management@example.com |

---

## Compliance Checklist

### OWASP Top 10 Coverage

- [x] **A01:2021 - Broken Access Control**: Role-based authorization
- [x] **A02:2021 - Cryptographic Failures**: TLS, encryption at rest
- [x] **A03:2021 - Injection**: Parameterized queries, input validation
- [x] **A04:2021 - Insecure Design**: Threat modeling, security reviews
- [x] **A05:2021 - Security Misconfiguration**: Secure defaults
- [x] **A06:2021 - Vulnerable Components**: Dependency scanning
- [x] **A07:2021 - Authentication Failures**: MFA, strong passwords
- [x] **A08:2021 - Software Integrity**: Signed packages, CI/CD security
- [x] **A09:2021 - Logging Failures**: Comprehensive audit logging
- [x] **A10:2021 - SSRF**: Input validation, allowlisting

### Security Review Checklist

Before each release:

- [ ] All dependencies updated
- [ ] Security scan passed
- [ ] Penetration test completed (quarterly)
- [ ] Access controls reviewed
- [ ] Secrets rotated
- [ ] Logs reviewed for anomalies
- [ ] Backup/recovery tested

---

**Document History**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2024-06-01 | A. Lal | Initial version |
| 2.0 | 2026-02-02 | A. Lal | Complete update with middleware |
