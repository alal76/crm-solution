# ADR-002: Security Headers Middleware

## Status

Accepted

## Date

2026-02-02

## Context

The CRM Solution API was missing essential HTTP security headers that protect against common web vulnerabilities:

1. **Clickjacking attacks**: No X-Frame-Options header
2. **MIME type sniffing**: No X-Content-Type-Options header
3. **XSS attacks**: No X-XSS-Protection header
4. **Information leakage**: No Referrer-Policy header
5. **Resource injection**: No Content-Security-Policy
6. **Feature abuse**: No Permissions-Policy
7. **HTTPS downgrade**: No Strict-Transport-Security

These missing headers exposed the application to OWASP Top 10 vulnerabilities and would fail security audits.

## Decision

We will implement a custom `SecurityHeadersMiddleware` that adds the following headers to all HTTP responses:

### Security Headers Implemented

| Header | Value | Purpose |
|--------|-------|---------|
| X-Content-Type-Options | nosniff | Prevents MIME type sniffing |
| X-Frame-Options | SAMEORIGIN | Prevents clickjacking |
| X-XSS-Protection | 1; mode=block | Enables browser XSS filter |
| Referrer-Policy | strict-origin-when-cross-origin | Controls referrer info |
| Content-Security-Policy | [configured policy] | Restricts resource loading |
| Permissions-Policy | [restrictive policy] | Limits browser features |
| Strict-Transport-Security | max-age=31536000; includeSubDomains | Enforces HTTPS (production) |

### Additional Security Measures

1. **Rate Limiting**: Already implemented via `AspNetCoreRateLimit`
2. **Cache Control**: No-cache headers for API responses
3. **HSTS**: Only enabled for non-localhost environments

### Middleware Placement

The middleware is placed early in the pipeline, after HTTPS redirection but before static files and routing, ensuring all responses include security headers.

## Consequences

### Positive
- **Enhanced security**: Protection against common web attacks
- **Compliance ready**: Meets OWASP security recommendations
- **Audit friendly**: Headers visible in security scans
- **Zero runtime cost**: Headers added with minimal overhead
- **Customizable**: Easy to adjust policies as needed

### Negative
- **Iframe restrictions**: X-Frame-Options may break legitimate embeds
- **CSP complexity**: Content Security Policy may need tuning
- **HSTS commitment**: Once enabled, HTTPS is required
- **Browser variations**: Some headers have varying browser support

### Mitigations
- SAMEORIGIN allows same-origin iframes
- CSP configured with common CDN patterns
- HSTS disabled for localhost development
- Legacy headers included for older browsers

## Implementation

Files created:
1. `CRM.Api/Middleware/SecurityHeadersMiddleware.cs` - Main middleware
2. `CRM.Api/Middleware/RateLimitingMiddleware.cs` - Custom rate limiting (backup)

Integration:
```csharp
// In Program.cs
app.UseSecurityHeaders();
```

## Testing

Verify headers with:
```bash
curl -I https://api.example.com/health
```

Expected headers in response:
```
X-Content-Type-Options: nosniff
X-Frame-Options: SAMEORIGIN
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
Content-Security-Policy: default-src 'self'; ...
Permissions-Policy: accelerometer=(), camera=(), ...
```

## References

- [OWASP Secure Headers Project](https://owasp.org/www-project-secure-headers/)
- [MDN HTTP Headers](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers)
- [Content Security Policy](https://content-security-policy.com/)
- [HSTS Preload](https://hstspreload.org/)
