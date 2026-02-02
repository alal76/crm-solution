# CRM Solution - Security Best Practices

**Version:** 1.0  
**Last Updated:** February 2, 2026  
**Status:** Active

---

## Table of Contents

1. [Overview](#overview)
2. [Authentication & Authorization](#authentication--authorization)
3. [Data Protection](#data-protection)
4. [Input Validation](#input-validation)
5. [API Security](#api-security)
6. [Database Security](#database-security)
7. [Frontend Security](#frontend-security)
8. [Dependency Management](#dependency-management)
9. [Logging & Monitoring](#logging--monitoring)
10. [Security Checklist](#security-checklist)

---

## Overview

This document outlines security best practices for the CRM Solution. Security is a critical aspect of the application and must be considered at every layer of the stack.

### Security Principles

1. **Defense in Depth**: Multiple layers of security controls
2. **Least Privilege**: Grant minimum required permissions
3. **Secure by Default**: Security enabled out of the box
4. **Fail Securely**: Fail in a way that doesn't compromise security
5. **Keep it Simple**: Complex security is hard to maintain
6. **Never Trust Input**: Always validate and sanitize

---

## Authentication & Authorization

### JWT Token Security

```csharp
// ✅ GOOD - Strong JWT configuration
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
                Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
            ClockSkew = TimeSpan.Zero // No grace period
        };
    });

// Token generation with proper claims
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role),
    new Claim("TenantId", user.TenantId.ToString())
};

var token = new JwtSecurityToken(
    issuer: _configuration["Jwt:Issuer"],
    audience: _configuration["Jwt:Audience"],
    claims: claims,
    expires: DateTime.UtcNow.AddHours(1), // Short expiration
    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
);
```

### Password Security

```csharp
// ✅ GOOD - Use proper password hashing
public class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100000;
    
    public static string HashPassword(string password)
    {
        using var algorithm = new Rfc2898DeriveBytes(
            password,
            SaltSize,
            Iterations,
            HashAlgorithmName.SHA256);
            
        var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
        var salt = Convert.ToBase64String(algorithm.Salt);
        
        return $"{Iterations}.{salt}.{key}";
    }
    
    public static bool VerifyPassword(string hash, string password)
    {
        var parts = hash.Split('.', 3);
        
        if (parts.Length != 3)
        {
            throw new FormatException("Unexpected hash format");
        }
        
        var iterations = Convert.ToInt32(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var key = Convert.FromBase64String(parts[2]);
        
        using var algorithm = new Rfc2898DeriveBytes(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256);
            
        var keyToCheck = algorithm.GetBytes(KeySize);
        
        return keyToCheck.SequenceEqual(key);
    }
}

// ❌ BAD - Never store plain text passwords
user.Password = password; // NEVER DO THIS!

// ❌ BAD - Don't use weak hashing
var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password)); // Weak!
```

### Role-Based Access Control

```csharp
// ✅ GOOD - Use authorization attributes
[Authorize(Roles = "Admin,Manager")]
[HttpPost("customers")]
public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto dto)
{
    // Only admins and managers can create customers
}

// ✅ GOOD - Policy-based authorization
services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageCustomers", policy =>
        policy.RequireClaim("Permission", "customers.write"));
        
    options.AddPolicy("CanViewReports", policy =>
        policy.RequireRole("Admin", "Manager", "Analyst"));
});

[Authorize(Policy = "CanManageCustomers")]
[HttpPut("customers/{id}")]
public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerDto dto)
{
    // Implementation
}
```

---

## Data Protection

### Sensitive Data Encryption

```csharp
// ✅ GOOD - Encrypt sensitive data at rest
public class EncryptionService
{
    private readonly byte[] _key;
    
    public EncryptionService(IConfiguration configuration)
    {
        _key = Convert.FromBase64String(configuration["Encryption:Key"]!);
    }
    
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();
        msEncrypt.Write(aes.IV, 0, aes.IV.Length);
        
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }
        
        return Convert.ToBase64String(msEncrypt.ToArray());
    }
    
    public string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);
        
        using var aes = Aes.Create();
        var iv = new byte[aes.IV.Length];
        var cipher = new byte[fullCipher.Length - iv.Length];
        
        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);
        
        aes.Key = _key;
        aes.IV = iv;
        
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        
        return srDecrypt.ReadToEnd();
    }
}
```

### Protect Sensitive Configuration

```json
// ❌ BAD - Don't commit secrets
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=crm;User=sa;Password=MyPassword123;"
  },
  "Jwt": {
    "Secret": "my-super-secret-key"
  }
}

// ✅ GOOD - Use environment variables or Azure Key Vault
{
  "ConnectionStrings": {
    "DefaultConnection": "${CONNECTION_STRING}"
  },
  "Jwt": {
    "Secret": "${JWT_SECRET}"
  }
}
```

---

## Input Validation

### Server-Side Validation

```csharp
// ✅ GOOD - Always validate on server
public class CustomerValidator : AbstractValidator<CustomerDto>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Company)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[a-zA-Z0-9\s\-\.]+$") // Only allow safe characters
            .WithMessage("Company name contains invalid characters");
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);
            
        RuleFor(x => x.Website)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("Invalid website URL");
    }
    
    private bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

// ❌ BAD - Don't trust client-side validation only
// Always validate on server, even if client validates
```

### SQL Injection Prevention

```csharp
// ✅ GOOD - Use parameterized queries (EF Core does this automatically)
var customer = await _context.Customers
    .Where(c => c.Email == email)
    .FirstOrDefaultAsync();

// ✅ GOOD - If using raw SQL, use parameters
var customers = await _context.Customers
    .FromSqlRaw("SELECT * FROM Customers WHERE Email = {0}", email)
    .ToListAsync();

// ❌ BAD - String concatenation is vulnerable to SQL injection
var query = $"SELECT * FROM Customers WHERE Email = '{email}'"; // NEVER!
```

### XSS Prevention

```typescript
// ✅ GOOD - React automatically escapes JSX content
<div>{customer.company}</div> // Safe - React escapes this

// ❌ BAD - Using dangerouslySetInnerHTML without sanitization
<div dangerouslySetInnerHTML={{ __html: userInput }} /> // Dangerous!

// ✅ GOOD - If you must use HTML, sanitize it first
import DOMPurify from 'dompurify';

<div dangerouslySetInnerHTML={{ 
  __html: DOMPurify.sanitize(userInput) 
}} />
```

---

## API Security

### Rate Limiting

```csharp
// ✅ GOOD - Implement rate limiting
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private const int RequestLimit = 100; // per minute
    
    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = context.User.Identity?.Name ?? 
                      context.Connection.RemoteIpAddress?.ToString();
        
        if (clientId == null)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }
        
        var cacheKey = $"rate_limit_{clientId}";
        var requestCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
            return 0;
        });
        
        if (requestCount >= RequestLimit)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        
        _cache.Set(cacheKey, requestCount + 1, TimeSpan.FromMinutes(1));
        await _next(context);
    }
}
```

### Security Headers

```csharp
// ✅ GOOD - Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
    
    await next();
});
```

### CORS Configuration

```csharp
// ✅ GOOD - Restrictive CORS policy
services.AddCors(options =>
{
    options.AddPolicy("Production", builder =>
    {
        builder
            .WithOrigins(
                "https://yourdomain.com",
                "https://www.yourdomain.com")
            .AllowedMethods("GET", "POST", "PUT", "DELETE")
            .AllowedHeaders("Authorization", "Content-Type")
            .AllowCredentials()
            .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// ❌ BAD - Allowing all origins in production
builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); // Only for dev!
```

---

## Database Security

### Connection Strings

```csharp
// ✅ GOOD - Use Windows Authentication or Azure AD when possible
"Server=myserver;Database=crm;Integrated Security=true;"

// ✅ GOOD - If using SQL auth, use strong passwords and rotate regularly
"Server=myserver;Database=crm;User Id=crm_app;Password=${DB_PASSWORD};"

// ✅ GOOD - Enable encryption
"Server=myserver;Database=crm;Encrypt=true;TrustServerCertificate=false;"
```

### Least Privilege

```sql
-- ✅ GOOD - Create app-specific user with minimal permissions
CREATE USER crm_app WITH PASSWORD = 'strong_password';
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO crm_app;

-- ❌ BAD - Don't use 'sa' or 'root' for application
-- ❌ BAD - Don't grant db_owner or sysadmin roles
```

---

## Frontend Security

### Secure Storage

```typescript
// ✅ GOOD - Store JWT in httpOnly cookie (server-side)
// Or use sessionStorage for tokens (cleared on tab close)
sessionStorage.setItem('token', token);

// ❌ BAD - Don't store sensitive data in localStorage
localStorage.setItem('token', token); // Vulnerable to XSS

// ❌ BAD - Never store passwords client-side
localStorage.setItem('password', password); // NEVER!
```

### API Calls Security

```typescript
// ✅ GOOD - Include CSRF token
const csrfToken = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');

axios.post('/api/customers', data, {
  headers: {
    'X-CSRF-Token': csrfToken,
    'Authorization': `Bearer ${token}`
  }
});

// ✅ GOOD - Timeout for requests
axios.get('/api/customers', {
  timeout: 5000 // 5 seconds
});
```

---

## Dependency Management

### Regular Updates

```bash
# ✅ GOOD - Regularly check for vulnerabilities
npm audit
dotnet list package --vulnerable

# Fix vulnerabilities
npm audit fix
dotnet add package <PackageName> --version <SafeVersion>
```

### Trusted Sources

```xml
<!-- ✅ GOOD - Use official NuGet packages -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />

<!-- ❌ BAD - Be cautious with unknown/unmaintained packages -->
```

---

## Logging & Monitoring

### Security Logging

```csharp
// ✅ GOOD - Log security events
_logger.LogWarning(
    "Failed login attempt for user {Email} from IP {IpAddress}",
    email,
    context.Connection.RemoteIpAddress);

_logger.LogInformation(
    "User {UserId} accessed customer {CustomerId}",
    userId,
    customerId);

// ❌ BAD - Never log sensitive data
_logger.LogInformation($"Password: {password}"); // NEVER!
_logger.LogDebug($"Credit Card: {creditCard}"); // NEVER!
```

### What to Log

**DO LOG:**
- Authentication attempts (success/failure)
- Authorization failures
- Input validation failures
- Critical business operations
- System errors and exceptions
- Configuration changes
- Data access patterns

**DON'T LOG:**
- Passwords
- API keys/secrets
- Credit card numbers
- Social security numbers
- Personal health information
- Full session tokens

---

## Security Checklist

### Development Phase

- [ ] Use HTTPS everywhere
- [ ] Implement proper authentication
- [ ] Enforce strong password requirements
- [ ] Use JWT with short expiration
- [ ] Validate all inputs server-side
- [ ] Use parameterized queries
- [ ] Implement CSRF protection
- [ ] Add security headers
- [ ] Configure CORS properly
- [ ] Enable rate limiting
- [ ] Encrypt sensitive data at rest
- [ ] Use secure password hashing
- [ ] Implement proper error handling
- [ ] Never expose stack traces
- [ ] Use Content Security Policy

### Code Review

- [ ] No hardcoded secrets
- [ ] No SQL injection vulnerabilities
- [ ] No XSS vulnerabilities
- [ ] Proper input validation
- [ ] Secure session management
- [ ] Proper error handling
- [ ] No sensitive data in logs
- [ ] Proper authorization checks

### Deployment

- [ ] Update all dependencies
- [ ] Run security scans
- [ ] Configure firewall rules
- [ ] Enable database encryption
- [ ] Set up monitoring/alerts
- [ ] Review access controls
- [ ] Backup and disaster recovery plan
- [ ] Security incident response plan

### Ongoing Maintenance

- [ ] Regular dependency updates
- [ ] Security patch management
- [ ] Log monitoring and analysis
- [ ] Penetration testing
- [ ] Security training for team
- [ ] Review and update policies

---

## Security Contacts

**Security Issues**: Report to security@yourdomain.com  
**Emergency Contact**: +1-XXX-XXX-XXXX

---

## References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP API Security](https://owasp.org/www-project-api-security/)
- [Microsoft Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [React Security Best Practices](https://react.dev/learn/react-security)

---

**Last Review:** February 2, 2026  
**Next Review:** May 2, 2026
