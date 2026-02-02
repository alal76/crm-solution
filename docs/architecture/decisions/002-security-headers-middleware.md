# ADR-002: Implement Security Headers Middleware

**Date:** 2026-02-02  
**Status:** Accepted  
**Deciders:** Architecture Review Team, Security Team  
**Technical Story:** Security Enhancement Initiative

## Context

The CRM application handles sensitive customer data and must protect against common web vulnerabilities. Security audits identified missing security headers that leave the application vulnerable to:

- **Clickjacking**: Malicious sites embedding our app in iframes
- **XSS (Cross-Site Scripting)**: Injection of malicious scripts
- **MIME Type Sniffing**: Browsers executing files as different types
- **Information Disclosure**: Server headers revealing technology stack
- **Man-in-the-Middle**: Unencrypted connections being intercepted

Current state:
- No security headers configured
- Default ASP.NET Core headers expose server information
- No Content Security Policy
- No protection against clickjacking
- HTTPS not enforced

## Decision

We will implement a custom `SecurityHeadersMiddleware` that adds the following security headers to all HTTP responses:

### Headers to Add

1. **X-Content-Type-Options: nosniff**
   - Prevents MIME type sniffing
   - Forces browsers to respect Content-Type header

2. **X-Frame-Options: DENY**
   - Prevents clickjacking attacks
   - Blocks all iframe embedding

3. **X-XSS-Protection: 1; mode=block**
   - Enables XSS filter in older browsers
   - Blocks page if XSS detected

4. **Referrer-Policy: strict-origin-when-cross-origin**
   - Controls referrer information sent
   - Balances privacy and functionality

5. **Content-Security-Policy**
   - Restricts resource loading
   - Mitigates XSS attacks
   - Policy: `default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; ...`

6. **Permissions-Policy**
   - Controls browser feature access
   - Disables geolocation, camera, microphone, payment APIs

7. **Strict-Transport-Security** (HTTPS only)
   - Forces HTTPS for 1 year
   - Includes subdomains
   - Only added when request is HTTPS

### Headers to Remove

- `Server`: Hides ASP.NET Core version
- `X-Powered-By`: Hides technology information

### Implementation

```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        // ... (see implementation)
        
        await _next(context);
    }
}
```

### Registration in Program.cs

```csharp
app.UseRouting();
app.UseMiddleware<SecurityHeadersMiddleware>(); // Add after routing
app.UseCors();
```

## Consequences

### Positive Consequences

- **Security**: Protection against common web vulnerabilities
  - Clickjacking prevention
  - XSS mitigation
  - MIME sniffing protection
  - Information disclosure prevention

- **Compliance**: Meets security best practices
  - OWASP Top 10 recommendations
  - Industry standard headers
  - Security audit requirements

- **Trust**: Demonstrates security commitment
  - Customer confidence
  - Enterprise readiness
  - Professional image

- **Minimal Impact**: 
  - No performance overhead
  - No code changes required
  - Drop-in solution

### Negative Consequences

- **Compatibility**: May break some scenarios
  - Cannot be embedded in iframes (X-Frame-Options)
  - External scripts may be blocked (CSP)
  - Older browsers may have issues

- **CSP Tuning**: 
  - May need adjustments for specific features
  - `unsafe-inline` and `unsafe-eval` still required
  - Will need updates as app evolves

- **Testing**: 
  - Need to verify all functionality still works
  - May need CSP exceptions for third-party integrations

## Alternatives Considered

### Alternative 1: NWebsec Package
**Pros:**
- Well-tested middleware library
- Easy configuration
- Additional security features

**Cons:**
- External dependency
- Less control over implementation
- Adds package overhead

**Why Rejected:** Custom middleware gives us full control and no dependencies

### Alternative 2: Configure in Web Server (Nginx/IIS)
**Pros:**
- Centralized configuration
- Works for all applications
- No code changes

**Cons:**
- Deployment complexity
- Environment-specific configuration
- Not portable across environments

**Why Rejected:** Want consistent behavior regardless of hosting environment

### Alternative 3: No Security Headers
**Pros:**
- No implementation effort
- No compatibility issues
- No testing needed

**Cons:**
- Vulnerable to attacks
- Fails security audits
- Not enterprise-ready

**Why Rejected:** Unacceptable security posture

## Implementation Plan

### Phase 1: Development (Day 1)
- [x] Create SecurityHeadersMiddleware.cs
- [x] Implement header logic
- [x] Add conditional HSTS for HTTPS
- [x] Remove information disclosure headers

### Phase 2: Integration (Day 1)
- [x] Register middleware in Program.cs
- [x] Position correctly in pipeline (after routing)
- [x] Test in development environment

### Phase 3: Testing (Days 2-3)
- [ ] Test all application features
- [ ] Verify headers in browser DevTools
- [ ] Test iframe embedding (should fail)
- [ ] Test CSP with inline scripts
- [ ] Security scanner verification

### Phase 4: Tuning (Days 4-5)
- [ ] Adjust CSP for known safe scripts
- [ ] Whitelist trusted domains if needed
- [ ] Handle any compatibility issues
- [ ] Document exceptions and why

### Phase 5: Deployment (Day 6)
- [ ] Deploy to staging environment
- [ ] Run security scans
- [ ] Deploy to production
- [ ] Monitor for issues

## CSP Tuning Strategy

Start restrictive, then relax as needed:

1. **Initial Policy**: Very strict
   ```
   default-src 'self'
   ```

2. **Add exceptions as discovered**:
   - `script-src 'self' 'unsafe-inline'` - For inline React scripts
   - `style-src 'self' 'unsafe-inline'` - For MUI styles
   - `img-src 'self' data: https:` - For images and data URIs

3. **Monitor violations**: Use CSP report-uri in future

## Security Headers Verification

Use these tools to verify:
- [SecurityHeaders.com](https://securityheaders.com/)
- [Mozilla Observatory](https://observatory.mozilla.org/)
- Browser DevTools Network tab
- OWASP ZAP security scanner

## Success Metrics

- A+ rating on SecurityHeaders.com
- Pass OWASP security scan
- Zero XSS vulnerabilities
- Zero clickjacking vulnerabilities
- All headers present in production

## References

- [OWASP Secure Headers Project](https://owasp.org/www-project-secure-headers/)
- [MDN Web Security](https://developer.mozilla.org/en-US/docs/Web/Security)
- [Content Security Policy Reference](https://content-security-policy.com/)
- [SecurityHeaders.com](https://securityheaders.com/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)

## Review History

- **2026-02-02**: Initial decision - Accepted
