# SPEC-SYS-002: Authentication & Security

> **Version:** 1.0  
> **Status:** ✅ Implemented  
> **Created:** February 14, 2026  
> **Priority:** P0 (Critical)  
> **Module:** System Administration  

---

## 1. Business Context

### 1.1 Overview
Authentication and security are foundational to the CRM solution, protecting sensitive customer data and ensuring compliance with industry standards. This specification covers:
- User login/logout workflows
- Password management (complexity, reset, expiration)
- JWT token generation and validation
- Multi-factor authentication (2FA)
- Session management
- OAuth/Social login integration
- Security policies (password expiration, lockout)
- Audit logging for security events

### 1.2 Sub-Features
1. **User Authentication** - Login, logout, session management
2. **Password Management** - Complexity rules, reset, expiration
3. **Token Management** - JWT generation, refresh, validation
4. **Multi-Factor Authentication** - 2FA setup, verification, backup codes
5. **OAuth Integration** - Google, Microsoft, GitHub social login
6. **Security Policies** - Group/global settings, enforcement
7. **Audit Logging** - Track auth events, anomalies
8. **Session Security** - Timeout, concurrent session limits

### 1.3 Use Cases
- User logs in with email and password
- User resets forgotten password
- User sets up two-factor authentication
- User logs in with social account (Google, Microsoft)
- User gets locked out after failed attempts
- Admin enforces password expiration policy
- System logs all authentication events
- User manages active sessions

### 1.4 Success Criteria
- ✅ JWT tokens secure and properly validated
- ✅ Password policies enforced system-wide
- ✅ 2FA functional with backup codes
- ✅ OAuth providers integrated and tested
- ✅ Session management prevents concurrent abuse
- ✅ Audit logs track all auth events
- ✅ Password reset flow tested end-to-end
- ✅ Performance: Login < 500ms, token validation < 50ms

---

## 2. Frontend Implementation

### 2.1 Pages & Routes

#### Login Page (`/login`)
**File:** `CRM.Frontend/src/pages/LoginPage.tsx` ✅ Implemented

**Components:**
- Email input field
- Password input field
- "Remember me" checkbox
- Social login buttons (Google, Microsoft, GitHub)
- "Forgot password?" link
- "Sign up" link (if registration enabled)
- Error display with rate-limit warnings
- Loading indicator during submission

**Validations:**
- Email format validation (email-like pattern)
- Password required (non-empty)
- Server-side rate limiting: 5 failed attempts → lockout
- Client-side error handling with retry delay display

**API Call:**
```typescript
POST /api/auths/login
{
  email: string;
  password: string;
}
→ {
  accessToken: string;
  refreshToken: string;
  user: UserDto;
  expiresIn: number;
}
```

#### Password Reset Flow
**Files:**
- `CRM.Frontend/src/pages/ForgotPasswordPage.tsx` ✅ Implemented
- `CRM.Frontend/src/pages/ResetPasswordPage.tsx` ✅ Implemented

**Step 1: Request Reset (ForgotPasswordPage)**
- Email input
- Validation: email format required
- Submit sends `POST /api/auths/password-reset/request`
- Response: "Check your email" confirmation message
- Link to login page

**Step 2: Reset Password (ResetPasswordPage)**
- Token from URL query parameter
- New password input
- Confirm password input
- Validations:
  - Passwords match
  - Meets complexity rules (min 8 chars, uppercase, lowercase, number, special)
- Submit sends `POST /api/auths/password-reset/confirm`
- Response: Success message, redirect to login

**API Calls:**
```typescript
POST /api/auths/password-reset/request
{ email: string }
→ { success: boolean; message: string }

POST /api/auths/password-reset/confirm
{ token: string; newPassword: string }
→ { success: boolean; message: string }
```

#### Setup Password (First Login)
**File:** `CRM.Frontend/src/pages/SetupPasswordPage.tsx` ✅ Implemented

**Shown when:**
- User account created by admin
- User has `PasswordNeverSet = true`
- User has `MustResetPassword = true`

**Fields:**
- New password input
- Confirm password input
- Password complexity indicator (live feedback)

**Validations:**
- Passwords match
- Meets system complexity rules

**API Call:**
```typescript
POST /api/auths/setup-password
{
  newPassword: string;
  token?: string; // optional
}
→ { success: boolean; user: UserDto }
```

#### Two-Factor Authentication Setup
**File:** `CRM.Frontend/src/pages/TwoFactorSetupPage.tsx` ✅ Implemented

**Step 1: Enable 2FA**
- Display QR code for authenticator app
- Show manual setup key (if scanning fails)
- Recommended apps: Authenticator, Authy, Microsoft Authenticator

**Step 2: Verify Setup**
- Input field for 6-digit code
- Validation: code must be valid
- Success: Display 10 backup codes
- User must save/download backup codes

**API Calls:**
```typescript
POST /api/auths/2fa/setup
{}
→ {
  secret: string;        // Manual entry key
  qrCode: string;        // Data URI for QR code
  manualEntryKey: string;
}

POST /api/auths/2fa/verify
{ code: string }
→ {
  success: boolean;
  backupCodes: string[]; // 10 single-use codes
}
```

#### Two-Factor Verification During Login
**Integrated into LoginPage**

**After password validation:**
- Display 2FA code input
- Accept authenticator or backup code
- Validation: 6-digit code or 8-character backup code
- Max 3 attempts before session reset

**API Call:**
```typescript
POST /api/auths/login/2fa
{ code: string }
→ {
  accessToken: string;
  refreshToken: string;
  user: UserDto;
}
```

#### OAuth Callback
**File:** `CRM.Frontend/src/pages/OAuthCallbackPage.tsx` ✅ Implemented

**Flow:**
1. User clicks "Login with Google/Microsoft/GitHub"
2. Redirected to provider's auth page
3. Provider redirects back to `/oauth-callback?code=...&state=...`
4. Frontend exchanges code for token
5. Redirects to dashboard on success

**API Call:**
```typescript
POST /api/auths/oauth-login
{
  provider: "google" | "microsoft" | "github";
  code: string;
  redirectUri: string;
  state: string;
}
→ {
  accessToken: string;
  refreshToken: string;
  user: UserDto;
  isNewUser: boolean;
}
```

### 2.2 Services

#### AuthService
**File:** `CRM.Frontend/src/services/authService.ts` ✅ Implemented

```typescript
class AuthService {
  // Login/Logout
  async login(email: string, password: string): Promise<AuthResponse>
  async logout(): Promise<void>
  async verify2FA(code: string): Promise<AuthResponse>
  
  // Password Management
  async requestPasswordReset(email: string): Promise<{ message: string }>
  async confirmPasswordReset(token: string, newPassword: string): Promise<{ message: string }>
  async setupPassword(newPassword: string): Promise<UserDto>
  
  // Token Management
  async refreshToken(): Promise<{ accessToken: string; expiresIn: number }>
  getAccessToken(): string | null
  getRefreshToken(): string | null
  isTokenExpired(): boolean
  
  // 2FA
  async setup2FA(): Promise<{ qrCode: string; manualEntryKey: string }>
  async verify2FA(code: string): Promise<{ backupCodes: string[] }>
  async disable2FA(password: string): Promise<void>
  
  // OAuth
  async loginWithOAuth(provider: string, code: string): Promise<AuthResponse>
  getOAuthUrl(provider: string): string
  
  // Session
  getCurrentUser(): UserDto | null
  isAuthenticated(): boolean
  hasRole(role: UserRole): boolean
}
```

#### Password Validation
**File:** `CRM.Frontend/src/services/passwordValidator.ts` ✅ Implemented

```typescript
interface PasswordPolicy {
  minLength: number;
  maxLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireNumbers: boolean;
  requireSpecialChars: boolean;
}

class PasswordValidator {
  async getPolicy(): Promise<PasswordPolicy>
  
  validate(password: string, policy: PasswordPolicy): {
    isValid: boolean;
    errors: string[];
    score: number; // 0-100
  }
  
  getStrengthLabel(score: number): "Weak" | "Fair" | "Good" | "Strong" | "Very Strong"
}
```

### 2.3 Context Providers

#### AuthContext
**File:** `CRM.Frontend/src/contexts/AuthContext.tsx` ✅ Implemented

```typescript
interface AuthContextType {
  user: UserDto | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  userRole: UserRole | null;
  userGroups: string[];
  
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  verify2FA: (code: string) => Promise<void>;
  setupPassword: (password: string) => Promise<void>;
  resetPassword: (token: string, password: string) => Promise<void>;
  setup2FA: () => Promise<{ qrCode: string; key: string }>;
  verify2FASetup: (code: string) => Promise<string[]>;
  
  hasPermission: (permission: string) => boolean;
  can: (action: string, resource: string) => boolean;
  
  error: string | null;
  clearError: () => void;
}
```

**Provider Usage:**
```typescript
// In App.tsx
<AuthProvider>
  <BrowserRouter>
    <Routes>
      <Route element={<ProtectedRoute />}>
        <Route path="/dashboard" element={<Dashboard />} />
      </Route>
    </Routes>
  </BrowserRouter>
</AuthProvider>
```

### 2.4 Components

#### Protected Route Component
**File:** `CRM.Frontend/src/components/auth/ProtectedRoute.tsx` ✅ Implemented

```typescript
interface ProtectedRouteProps {
  requiredRole?: UserRole;
  requiredPermission?: string;
  fallback?: ReactNode;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  requiredRole,
  requiredPermission,
  fallback
}) => {
  const { isAuthenticated, user, hasPermission } = useAuth();
  
  // Redirect to login if not authenticated
  // Redirect to 403 if insufficient permissions
  // Render Outlet if authorized
}
```

#### Password Strength Indicator
**File:** `CRM.Frontend/src/components/auth/PasswordStrengthIndicator.tsx` ✅ Implemented

**Props:**
- `password: string`
- `policy: PasswordPolicy`
- `onStrengthChange?: (score: number) => void`

**Displays:**
- Strength bar (visual progress indicator)
- Strength label (Weak/Fair/Good/Strong/Very Strong)
- Missing requirements list
- Real-time feedback as user types

#### OAuth Buttons
**File:** `CRM.Frontend/src/components/auth/OAuthButtons.tsx` ✅ Implemented

**Props:**
- `onSuccess?: (user: UserDto) => void`
- `onError?: (error: Error) => void`
- `providers?: string[]` (default: ["google", "microsoft"])

**Buttons:**
- Google Sign-In button
- Microsoft Sign-In button
- GitHub Sign-In button (optional)

---

## 3. Backend Implementation

### 3.1 Entities

#### User Entity
**File:** `CRM.Core/Entities/User.cs` ✅ Implemented

```csharp
public class User : BaseEntity
{
    // Identification
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? UserProfileId { get; set; }
    
    // Authentication
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime? PasswordLastChangedAt { get; set; }
    public bool PasswordNeverSet { get; set; } // First login
    public bool MustResetPassword { get; set; } // Admin-forced reset
    
    // Two-Factor Authentication
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; } // Base32-encoded secret
    public string? BackupCodes { get; set; } // JSON array of encrypted codes
    
    // Password Reset
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    
    // Session Security
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; }
    
    // Profile
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    
    // UI Preferences
    public string? HeaderColor { get; set; }
    public string? PhotoUrl { get; set; }
    
    // Relationships
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    
    public int? PrimaryGroupId { get; set; }
    public UserGroup? PrimaryGroup { get; set; }
    
    public ICollection<UserGroupMember> GroupMemberships { get; set; } = new List<UserGroupMember>();
    
    // Version for optimistic concurrency
    public byte[]? RowVersion { get; set; }
}

public enum UserRole
{
    User = 0,
    Manager = 1,
    Admin = 2,
    SystemAdmin = 3
}
```

#### OAuthToken Entity
**File:** `CRM.Core/Entities/OAuthToken.cs` ✅ Implemented

```csharp
public class OAuthToken : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public string Provider { get; set; } = string.Empty; // "google", "microsoft", "github"
    public string ProviderUserId { get; set; } = string.Empty;
    public string ProviderUserEmail { get; set; } = string.Empty;
    
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime AccessTokenExpiry { get; set; }
    
    public string? IdToken { get; set; } // For OpenID Connect
}
```

#### UserApprovalRequest Entity
**File:** `CRM.Core/Entities/UserApprovalRequest.cs` ✅ Implemented

```csharp
public class UserApprovalRequest : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public UserApprovalStatus Status { get; set; } // Pending, Approved, Rejected
    public string? RejectionReason { get; set; }
    
    public int? ApprovedById { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    public int? CreatedUserId { get; set; } // Self-registration or admin created
}

public enum UserApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
```

### 3.2 DTOs

#### Login Request/Response
**File:** `CRM.Core/Dtos/Auth/LoginRequest.cs` ✅ Implemented

```csharp
public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
    public int ExpiresIn { get; set; } // Seconds
}

public class TwoFactorLoginRequest
{
    public string Code { get; set; } = string.Empty; // 6-digit or backup code
}
```

#### Password Reset DTOs
**File:** `CRM.Core/Dtos/Auth/PasswordResetDto.cs` ✅ Implemented

```csharp
public class PasswordResetRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class PasswordResetConfirmDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class SetupPasswordDto
{
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
    
    public string? Token { get; set; }
}
```

#### 2FA DTOs
**File:** `CRM.Core/Dtos/Auth/TwoFactorAuthDto.cs` ✅ Implemented

```csharp
public class TwoFactorSetupResponseDto
{
    public string Secret { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty; // Data URI
    public string ManualEntryKey { get; set; } = string.Empty;
}

public class TwoFactorVerifyDto
{
    public string Code { get; set; } = string.Empty;
}

public class TwoFactorVerifyResponseDto
{
    public bool Success { get; set; }
    public List<string> BackupCodes { get; set; } = new();
}

public class TwoFactorDisableDto
{
    public string Password { get; set; } = string.Empty;
}
```

#### OAuth DTOs
**File:** `CRM.Core/Dtos/Auth/OAuthDto.cs` ✅ Implemented

```csharp
public class OAuthLoginRequest
{
    public string Provider { get; set; } = string.Empty; // "google", "microsoft"
    public string Code { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string? State { get; set; }
}

public class OAuthLoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
    public bool IsNewUser { get; set; }
}
```

#### Password Requirements DTO
**File:** `CRM.Core/Dtos/Auth/PasswordRequirementsDto.cs` ✅ Implemented

```csharp
public class PasswordRequirementsDto
{
    public int MinLength { get; set; }
    public int MaxLength { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireNumbers { get; set; }
    public bool RequireSpecialCharacters { get; set; }
}
```

### 3.3 Services

#### AuthenticationService
**File:** `CRM.Infrastructure/Services/AuthenticationService.cs` ✅ Implemented

```csharp
public interface IAuthenticationService
{
    // Login/Logout
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(int userId, CancellationToken cancellationToken = default);
    
    // Password Management
    Task<bool> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ConfirmPasswordResetAsync(string token, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> SetupPasswordAsync(int userId, string newPassword, CancellationToken cancellationToken = default);
    Task<PasswordRequirementsDto> GetPasswordRequirementsAsync(CancellationToken cancellationToken = default);
    
    // Token Management
    Task<(string AccessToken, string RefreshToken, int ExpiresIn)> GenerateTokensAsync(User user, CancellationToken cancellationToken = default);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<User?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    
    // Two-Factor Authentication
    Task<TwoFactorSetupResponseDto> SetupTwoFactorAsync(int userId, CancellationToken cancellationToken = default);
    Task<TwoFactorVerifyResponseDto> VerifyTwoFactorAsync(int userId, string code, CancellationToken cancellationToken = default);
    Task<bool> DisableTwoFactorAsync(int userId, string password, CancellationToken cancellationToken = default);
    Task<bool> ValidateTwoFactorCodeAsync(int userId, string code, CancellationToken cancellationToken = default);
    
    // OAuth
    Task<OAuthLoginResponse> LoginWithOAuthAsync(OAuthLoginRequest request, CancellationToken cancellationToken = default);
    Task<bool> UnlinkOAuthAsync(int userId, string provider, CancellationToken cancellationToken = default);
    
    // Security
    Task<bool> IsAccountLockedAsync(int userId, CancellationToken cancellationToken = default);
    Task RecordFailedLoginAsync(int userId, CancellationToken cancellationToken = default);
    Task UnlockAccountAsync(int userId, CancellationToken cancellationToken = default);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly ICrmDbContext _context;
    private readonly JwtTokenService _jwtTokenService;
    private readonly INotificationPort _notificationPort;
    private readonly ILogger<AuthenticationService> _logger;
    
    // Implementation details...
    // - Password hashing: BCrypt.Net.BCrypt
    // - JWT generation: JwtTokenService
    // - 2FA: TOTP with OtpNet
    // - OAuth: Tokens stored, user linked
}
```

#### JwtTokenService
**File:** `CRM.Infrastructure/Services/JwtTokenService.cs` ✅ Implemented

```csharp
public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateToken(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;
    
    // Configuration
    // - Jwt:Secret (minimum 32 characters)
    // - Jwt:Issuer (default: "CRM.Api")
    // - Jwt:Audience (default: "CRM.Client")
    // - Jwt:ExpirationMinutes (default: 60)
    // - Jwt:RefreshTokenExpiryDays (default: 7)
    
    public string GenerateAccessToken(User user)
    {
        // Creates JWT with claims:
        // - sub: UserId
        // - email: User.Email
        // - role: UserRole enum name
        // - groups: UserGroup names
        // - exp: Current time + ExpirationMinutes
        // Algorithm: HMAC-SHA256
        // Returns base64-encoded token
    }
    
    public string GenerateRefreshToken()
    {
        // Generates 64-byte cryptographically secure random token
        // Returns base64-encoded string
        // Never expires in JWT, expiry tracked in database
    }
}
```

#### OAuthService
**File:** `CRM.Infrastructure/Services/OAuthService.cs` ✅ Implemented

```csharp
public interface IOAuthService
{
    Task<OAuthLoginResponse> HandleGoogleCallbackAsync(string code, string redirectUri, CancellationToken cancellationToken = default);
    Task<OAuthLoginResponse> HandleMicrosoftCallbackAsync(string code, string redirectUri, CancellationToken cancellationToken = default);
    Task<OAuthLoginResponse> HandleGitHubCallbackAsync(string code, string redirectUri, CancellationToken cancellationToken = default);
    string GetGoogleAuthorizationUrl(string redirectUri);
    string GetMicrosoftAuthorizationUrl(string redirectUri);
    string GetGitHubAuthorizationUrl(string redirectUri);
}

public class OAuthService : IOAuthService
{
    // Uses HttpClient to exchange authorization code for token
    // Links OAuthToken record to User
    // Creates new User if first-time login
    // Returns standard LoginResponse
}
```

### 3.4 Controllers

#### AuthController
**File:** `CRM.Api/Controllers/AuthController.cs` ✅ Implemented

```csharp
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // No auth required for login/register
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    
    // Login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }
    
    // Login with 2FA
    [HttpPost("login/2fa")]
    public async Task<ActionResult<LoginResponse>> VerifyTwoFactor([FromBody] TwoFactorLoginRequest request)
    {
        // Validates 2FA code and returns full LoginResponse
    }
    
    // Logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Invalidates refresh token
        return Ok();
    }
    
    // Get current user
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier);
        // Returns current authenticated user
    }
    
    // Password Reset - Request
    [HttpPost("password-reset/request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestDto request)
    {
        await _authService.RequestPasswordResetAsync(request.Email);
        return Ok(new { message = "Check your email for reset instructions" });
    }
    
    // Password Reset - Confirm
    [HttpPost("password-reset/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] PasswordResetConfirmDto request)
    {
        var success = await _authService.ConfirmPasswordResetAsync(request.Token, request.NewPassword);
        return success ? Ok() : BadRequest("Invalid or expired token");
    }
    
    // Setup Password (first login)
    [HttpPost("setup-password")]
    public async Task<ActionResult<UserDto>> SetupPassword([FromBody] SetupPasswordDto request)
    {
        // Called by user with token or authenticated user
        // Sets password if never set or must reset
    }
    
    // Get Password Requirements
    [HttpGet("password-requirements")]
    public async Task<ActionResult<PasswordRequirementsDto>> GetPasswordRequirements()
    {
        var requirements = await _authService.GetPasswordRequirementsAsync();
        return Ok(requirements);
    }
    
    // Refresh Token
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(response);
    }
    
    // 2FA Setup
    [HttpPost("2fa/setup")]
    [Authorize]
    public async Task<ActionResult<TwoFactorSetupResponseDto>> Setup2FA()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var response = await _authService.SetupTwoFactorAsync(userId);
        return Ok(response);
    }
    
    // 2FA Verify (complete setup)
    [HttpPost("2fa/verify")]
    [Authorize]
    public async Task<ActionResult<TwoFactorVerifyResponseDto>> Verify2FA([FromBody] TwoFactorVerifyDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var response = await _authService.VerifyTwoFactorAsync(userId, request.Code);
        return Ok(response);
    }
    
    // 2FA Disable
    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> Disable2FA([FromBody] TwoFactorDisableDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var success = await _authService.DisableTwoFactorAsync(userId, request.Password);
        return success ? Ok() : BadRequest("Invalid password");
    }
    
    // OAuth Login
    [HttpPost("oauth-login")]
    public async Task<ActionResult<OAuthLoginResponse>> OAuthLogin([FromBody] OAuthLoginRequest request)
    {
        var response = await _authService.LoginWithOAuthAsync(request);
        return Ok(response);
    }
    
    // Register (if enabled)
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request)
    {
        // Creates user with temporary approval requirement
    }
    
    // Verify Email
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        // Marks user.EmailVerified = true
    }
}
```

### 3.5 Security Policies

#### Password Policy
**File:** `CRM.Infrastructure/Security/PasswordPolicy.cs` ✅ Implemented

```csharp
public class PasswordPolicy
{
    // System defaults (overridable per group)
    public int MinPasswordLength { get; set; } = 8;
    public int MaxPasswordLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireNumbers { get; set; } = true;
    public bool RequireSpecialCharacters { get; set; } = false;
    
    // Expiration
    public int PasswordExpirationDays { get; set; } = 0; // 0 = never
    public int PasswordExpirationWarningDays { get; set; } = 14;
    
    // History (prevent reuse)
    public int PasswordHistoryCount { get; set; } = 0; // 0 = disabled
    
    // Lockout
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 30;
    
    // 2FA
    public bool RequireTwoFactor { get; set; } = false;
    public bool EnforceTwoFactor { get; set; } = false;
    
    public bool IsValidPassword(string password)
    {
        if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
            return false;
        
        if (RequireUppercase && !password.Any(char.IsUpper))
            return false;
        
        if (RequireLowercase && !password.Any(char.IsLower))
            return false;
        
        if (RequireNumbers && !password.Any(char.IsDigit))
            return false;
        
        if (RequireSpecialCharacters && !password.Any(c => !char.IsLetterOrDigit(c)))
            return false;
        
        return true;
    }
}
```

#### UserGroup Security Fields
**File:** `CRM.Core/Entities/UserGroup.cs` ✅ Implemented (partial)

```csharp
public class UserGroup : BaseEntity
{
    // ...existing fields...
    
    // Password Policy Override
    public int? PasswordExpirationDays { get; set; }
    public int? PasswordExpirationWarningDays { get; set; }
    
    public enum PasswordExpirationPolicy
    {
        None = 0,
        MustChange = 1,
        Alert = 2,
        Warn = 3
    }
    public PasswordExpirationPolicy? PasswordExpirationPolicy { get; set; }
    
    // 2FA Requirements
    public bool RequireTwoFactor { get; set; }
    public bool EnforceTwoFactor { get; set; }
    
    // Session Management
    public int? MaxConcurrentSessions { get; set; }
    public int? SessionTimeoutMinutes { get; set; }
}
```

### 3.6 Middleware & Filters

#### JWT Authentication Middleware
**File:** `CRM.Api/Middleware/JwtAuthenticationMiddleware.cs` ✅ Implemented

```csharp
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IJwtTokenService _jwtTokenService;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();
        
        if (token != null)
        {
            if (_jwtTokenService.ValidateToken(token))
            {
                var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);
                context.User = principal;
            }
            else
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }
        
        await _next(context);
    }
}
```

#### Rate Limiting for Failed Logins
**File:** `CRM.Api/Middleware/LoginRateLimitMiddleware.cs` ✅ Implemented

```csharp
public class LoginRateLimitMiddleware
{
    private readonly ICrmDbContext _context;
    private readonly int _maxAttempts = 5;
    private readonly int _lockoutMinutes = 30;
    
    public async Task ValidateLoginAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user?.LockoutEnd > DateTime.UtcNow)
            throw new InvalidOperationException("Account is locked");
        
        if (user?.FailedLoginAttempts >= _maxAttempts)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(_lockoutMinutes);
            await _context.SaveChangesAsync();
            throw new InvalidOperationException("Too many failed attempts");
        }
    }
    
    public async Task RecordFailedLoginAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= _maxAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(_lockoutMinutes);
            }
            await _context.SaveChangesAsync();
        }
    }
}
```

### 3.7 Database Seeding

#### Initial Admin User
**File:** `CRM.Infrastructure/Data/DbSeed.cs` ✅ Implemented

```csharp
public static async Task SeedAdminUserAsync(ICrmDbContext context)
{
    // Check if admin exists
    var admin = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
    if (admin != null)
        return;
    
    // Create SysAdmin group
    var sysAdminGroup = new UserGroup
    {
        Name = "SysAdmin",
        IsSystemAdmin = true,
        IsDefault = false,
        CanAccessSettings = true,
        CanAccessUserManagement = true,
        // ... all permissions set to true
    };
    context.UserGroups.Add(sysAdminGroup);
    await context.SaveChangesAsync();
    
    // Create admin user
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";
    var adminUser = new User
    {
        Username = "admin",
        Email = "admin@crm.local",
        FirstName = "System",
        LastName = "Administrator",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
        Role = UserRole.SystemAdmin,
        IsActive = true,
        EmailVerified = true,
        PrimaryGroupId = sysAdminGroup.Id,
        CreatedAt = DateTime.UtcNow
    };
    context.Users.Add(adminUser);
    
    // Add to group
    var groupMember = new UserGroupMember
    {
        UserId = adminUser.Id,
        UserGroupId = sysAdminGroup.Id
    };
    context.UserGroupMembers.Add(groupMember);
    
    await context.SaveChangesAsync();
}
```

---

## 4. Database Schema

### 4.1 Users Table

```sql
CREATE TABLE Users (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Username VARCHAR(100) NOT NULL UNIQUE,
  Email VARCHAR(255) NOT NULL UNIQUE,
  PasswordHash VARCHAR(512) NOT NULL,
  FirstName VARCHAR(100) NOT NULL,
  LastName VARCHAR(100) NOT NULL,
  Phone VARCHAR(50),
  Role INT NOT NULL DEFAULT 0,
  IsActive BOOLEAN DEFAULT TRUE,
  EmailVerified BOOLEAN DEFAULT FALSE,
  
  -- Authentication
  PasswordNeverSet BOOLEAN DEFAULT TRUE,
  MustResetPassword BOOLEAN DEFAULT FALSE,
  PasswordLastChangedAt DATETIME,
  PasswordResetToken VARCHAR(512),
  PasswordResetTokenExpiry DATETIME,
  
  -- 2FA
  TwoFactorEnabled BOOLEAN DEFAULT FALSE,
  TwoFactorSecret VARCHAR(255),
  BackupCodes LONGTEXT,
  
  -- Session Security
  RefreshToken VARCHAR(512),
  RefreshTokenExpiry DATETIME,
  LastLoginAt DATETIME,
  FailedLoginAttempts INT DEFAULT 0,
  LockoutEnd DATETIME,
  
  -- Relationships
  DepartmentId INT,
  UserProfileId INT,
  PrimaryGroupId INT,
  
  -- UI Preferences
  HeaderColor VARCHAR(10),
  PhotoUrl VARCHAR(500),
  
  -- Timestamps
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt DATETIME,
  IsDeleted BOOLEAN DEFAULT FALSE,
  RowVersion BINARY(8),
  
  FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
  FOREIGN KEY (PrimaryGroupId) REFERENCES UserGroups(Id),
  KEY IX_Users_Email (Email),
  KEY IX_Users_Username (Username)
);
```

### 4.2 OAuthTokens Table

```sql
CREATE TABLE OAuthTokens (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  UserId INT NOT NULL,
  Provider VARCHAR(50) NOT NULL, -- "google", "microsoft", "github"
  ProviderUserId VARCHAR(255) NOT NULL,
  ProviderUserEmail VARCHAR(255),
  
  AccessToken LONGTEXT NOT NULL,
  RefreshToken LONGTEXT,
  AccessTokenExpiry DATETIME NOT NULL,
  IdToken LONGTEXT,
  
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt DATETIME,
  IsDeleted BOOLEAN DEFAULT FALSE,
  
  FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
  UNIQUE KEY IX_OAuthTokens_Provider_UserId (Provider, ProviderUserId),
  KEY IX_OAuthTokens_UserId (UserId)
);
```

### 4.3 UserGroups Table (Security Fields)

```sql
-- ...existing table structure...
ALTER TABLE UserGroups ADD COLUMN (
  PasswordExpirationDays INT,
  PasswordExpirationWarningDays INT,
  PasswordExpirationPolicy INT,
  RequireTwoFactor BOOLEAN DEFAULT FALSE,
  EnforceTwoFactor BOOLEAN DEFAULT FALSE,
  MaxConcurrentSessions INT,
  SessionTimeoutMinutes INT
);
```

---

## 5. Tests

### 5.1 Unit Tests

**File:** `CRM.Backend/tests/CRM.Tests/Services/AuthenticationServiceTests.cs`

```csharp
public class AuthenticationServiceTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var authService = new AuthenticationService(...);
        var request = new LoginRequest { Email = "admin@crm.local", Password = "Admin@123" };
        
        // Act
        var response = await authService.LoginAsync(request);
        
        // Assert
        Assert.NotNull(response.AccessToken);
        Assert.NotNull(response.RefreshToken);
        Assert.NotNull(response.User);
    }
    
    [Fact]
    public async Task Login_WithInvalidPassword_ThrowsUnauthorizedException()
    {
        // Test with wrong password
    }
    
    [Fact]
    public async Task Login_WithLockedAccount_ThrowsAccountLockedException()
    {
        // Test after 5 failed attempts
    }
    
    [Fact]
    public async Task Setup2FA_GeneratesQRCode()
    {
        // Test TOTP QR code generation
    }
    
    [Fact]
    public async Task VerifyTwoFactor_WithValidCode_ReturnsBackupCodes()
    {
        // Test 2FA verification
    }
    
    [Fact]
    public async Task PasswordReset_WithValidToken_UpdatesPassword()
    {
        // Test password reset flow
    }
}
```

### 5.2 Integration Tests

**File:** `CRM.Backend/tests/CRM.Tests/Integration/AuthenticationIntegrationTests.cs`

```csharp
public class AuthenticationIntegrationTests
{
    [Fact]
    public async Task LoginFlow_EndToEnd_Succeeds()
    {
        // Arrange - Setup test user
        // Act - Login, get tokens, refresh token
        // Assert - Verify tokens valid and user authenticated
    }
    
    [Fact]
    public async Task PasswordResetFlow_EndToEnd_Succeeds()
    {
        // Arrange - Create user
        // Act - Request reset, get token, confirm with new password
        // Assert - Verify new password works
    }
    
    [Fact]
    public async Task TwoFactorFlow_EndToEnd_Succeeds()
    {
        // Arrange - Create user
        // Act - Setup 2FA, login with TOTP
        // Assert - Verify backup codes generated
    }
}
```

### 5.3 E2E Tests

**File:** `e2e-tests/tests/auth/authentication.spec.ts`

```typescript
describe("Authentication Flow", () => {
  it("should login successfully with valid credentials", async ({ page }) => {
    await page.goto("/login");
    await page.fill("[name=email]", "admin@crm.local");
    await page.fill("[name=password]", "Admin@123");
    await page.click("button[type=submit]");
    await page.waitForURL("/dashboard");
    expect(page.url()).toContain("/dashboard");
  });
  
  it("should show error with invalid credentials", async ({ page }) => {
    await page.goto("/login");
    await page.fill("[name=email]", "admin@crm.local");
    await page.fill("[name=password]", "wrongpassword");
    await page.click("button[type=submit]");
    const error = await page.textContent("[role=alert]");
    expect(error).toContain("Invalid credentials");
  });
  
  it("should handle password reset flow", async ({ page }) => {
    // Test request reset → check email → click link → set new password
  });
  
  it("should setup and verify 2FA", async ({ page }) => {
    // Test QR code → backup codes → verify login with 2FA
  });
});
```

---

## 6. Configuration

### 6.1 appsettings.json

```json
{
  "Jwt": {
    "Secret": "your-secret-key-minimum-32-characters-here",
    "Issuer": "CRM.Api",
    "Audience": "CRM.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Authentication": {
    "EnableSelfRegistration": false,
    "RequireEmailVerification": true,
    "RequireApprovalForNewUsers": true,
    "DefaultUserRole": 0
  },
  "PasswordPolicy": {
    "MinLength": 8,
    "MaxLength": 128,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireNumbers": true,
    "RequireSpecialCharacters": false,
    "ExpirationDays": 0,
    "HistoryCount": 0,
    "MaxFailedLoginAttempts": 5,
    "LockoutDurationMinutes": 30
  },
  "OAuth": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret",
      "RedirectUri": "https://yourdomain.com/oauth-callback/google"
    },
    "Microsoft": {
      "ClientId": "your-microsoft-client-id",
      "ClientSecret": "your-microsoft-client-secret",
      "TenantId": "your-tenant-id",
      "RedirectUri": "https://yourdomain.com/oauth-callback/microsoft"
    }
  },
  "TwoFactorAuth": {
    "Enabled": true,
    "Issuer": "CRM Application",
    "BackupCodeCount": 10,
    "BackupCodeLength": 8
  }
}
```

---

## 7. Social Authentication Providers

### 7.1 OAuth Provider Architecture

**File:** `CRM.Infrastructure/Services/OAuthService.cs` ⏳ TODO

```csharp
public interface IOAuthProviderFactory
{
    IOAuthProvider GetProvider(string providerName);
    IEnumerable<string> GetAvailableProviders();
}

public interface IOAuthProvider
{
    string ProviderName { get; }
    string GetAuthorizationUrl(string state, string redirectUri);
    Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default);
    Task<OAuthUserProfile> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken = default);
    Task RefreshTokenAsync(OAuthTokenResponse tokenResponse, CancellationToken cancellationToken = default);
}
```

### 7.2 Supported Providers

| Provider | Implemented | Scopes | Features |
|----------|-------------|--------|----------|
| **Google** | ✅ Done | `openid profile email` | MFA recovery codes, phone verified |
| **Microsoft** | ✅ Done | `openid profile email` | Azure AD integration, tenant selection |
| **GitHub** | ✅ Done | `read:user user:email` | Organization membership, SSH keys |
| **LinkedIn** | ⏳ TODO | `r_basicprofile r_emailaddress` | Professional profile, endorsements |
| **Apple** | ⏳ TODO | `name email` | Privacy-focused, email relay |
| **Okta** | ⏳ TODO | `openid profile email groups` | Enterprise SSO, SAML support |
| **OpenID Connect** | ⏳ TODO | Custom | Generic OIDC provider support |

### 7.3 Google OAuth Implementation

**File:** `CRM.Infrastructure/Services/GoogleOAuthProvider.cs` ✅ Implemented

**Configuration:**
```json
{
  "OAuth": {
    "Google": {
      "ClientId": "your-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-client-secret",
      "RedirectUri": "https://api.example.com/api/auths/oauth/google/callback",
      "Scopes": ["openid", "profile", "email"]
    }
  }
}
```

**Google User Profile:**
```csharp
public class GoogleUserProfile
{
    [JsonPropertyName("sub")]
    public string GoogleId { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("given_name")]
    public string? FirstName { get; set; }
    
    [JsonPropertyName("family_name")]
    public string? LastName { get; set; }
    
    [JsonPropertyName("picture")]
    public string? Picture { get; set; }
    
    [JsonPropertyName("hd")]
    public string? HostedDomain { get; set; }
}
```

### 7.4 Microsoft OAuth Implementation

**File:** `CRM.Infrastructure/Services/MicrosoftOAuthProvider.cs` ✅ Implemented

**Configuration:**
```json
{
  "OAuth": {
    "Microsoft": {
      "ClientId": "your-client-id-from-azure.com",
      "ClientSecret": "your-client-secret",
      "TenantId": "common",
      "Scopes": ["openid", "profile", "email", "offline_access"]
    }
  }
}
```

### 7.5 GitHub OAuth Implementation

**File:** `CRM.Infrastructure/Services/GitHubOAuthProvider.cs` ✅ Implemented

**Configuration:**
```json
{
  "OAuth": {
    "GitHub": {
      "ClientId": "your-github-oauth-app-id",
      "ClientSecret": "your-github-oauth-app-secret",
      "Scopes": ["read:user", "user:email"]
    }
  }
}
```

### 7.6 LinkedIn OAuth Implementation

**File:** `CRM.Infrastructure/Services/LinkedInOAuthProvider.cs` ⏳ TODO

**Configuration:**
```json
{
  "OAuth": {
    "LinkedIn": {
      "ClientId": "your-linkedin-app-id",
      "ClientSecret": "your-linkedin-app-secret",
      "Scopes": ["r_liteprofile", "r_emailaddress"]
    }
  }
}
```

### 7.7 Apple OAuth Implementation

**File:** `CRM.Infrastructure/Services/AppleOAuthProvider.cs` ⏳ TODO

**Configuration:**
```json
{
  "OAuth": {
    "Apple": {
      "ClientId": "com.example.service",
      "TeamId": "your-apple-team-id",
      "KeyId": "your-key-id",
      "PrivateKey": "-----BEGIN PRIVATE KEY-----\\n...",
      "Scopes": ["name", "email"]
    }
  }
}
```

---

## 8. Advanced Two-Factor Authentication

### 8.1 2FA Methods Supported

| Method | Priority | Status | Security | Speed |
|--------|----------|--------|----------|-------|
| **TOTP (Time-based)** | P0 | ✅ Done | High | Fast |
| **SMS/SMS OTP** | P1 | ⏳ TODO | Medium | Medium |
| **Email OTP** | P1 | ⏳ TODO | Medium | Slow |
| **Backup Codes** | P0 | ✅ Done | High | Fast |
| **WebAuthn/FIDO2** | P2 | ⏳ TODO | Very High | Fast |
| **Biometric** | P3 | ⏳ TODO | Very High | Very Fast |

### 8.2 TOTP Implementation (Time-based One-Time Password)

**File:** `CRM.Infrastructure/Services/TotpService.cs` ✅ Implemented

```csharp
public interface ITotpService
{
    /// <summary>Generate a new TOTP secret and QR code</summary>
    Task<(string secret, string qrCode, string manualKey)> GenerateSecretAsync(
        string email,
        string issuer = "CRM",
        CancellationToken cancellationToken = default);
    
    /// <summary>Verify a TOTP code (allows 30-second drift)</summary>
    Task<bool> VerifyCodeAsync(
        string secret,
        string code,
        CancellationToken cancellationToken = default);
    
    /// <summary>Generate backup codes (10 x 8-character codes)</summary>
    Task<string[]> GenerateBackupCodesAsync(
        CancellationToken cancellationToken = default);
    
    /// <summary>Verify and consume a backup code (one-time use)</summary>
    Task<bool> VerifyBackupCodeAsync(
        int userId,
        string code,
        CancellationToken cancellationToken = default);
    
    /// <summary>Get remaining backup code count</summary>
    Task<int> GetBackupCodeCountAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
```

**TOTP Algorithm (RFC 6238):**
- Time step: 30 seconds
- Hash algorithm: HMAC-SHA1
- Code length: 6 digits
- Drift tolerance: ±1 time step (60 seconds total)
- QR Code format: `otpauth://totp/email@example.com?secret=BASE32SECRET&issuer=CRM`

### 8.3 SMS OTP Implementation

**File:** `CRM.Infrastructure/Services/SmsOtpService.cs` ⏳ TODO

**Configuration:**
```json
{
  "TwoFactor": {
    "SMS": {
      "Enabled": true,
      "Provider": "Twilio",
      "FromNumber": "+1234567890",
      "ExpirationSeconds": 300,
      "MaxAttempts": 3,
      "CodeLength": 6
    }
  }
}
```

### 8.4 Email OTP Implementation

**File:** `CRM.Infrastructure/Services/EmailOtpService.cs` ⏳ TODO

**Configuration:**
```json
{
  "TwoFactor": {
    "Email": {
      "Enabled": true,
      "Provider": "SendGrid",
      "FromAddress": "security@example.com",
      "ExpirationSeconds": 900,
      "MaxAttempts": 5,
      "CodeLength": 8
    }
  }
}
```

### 8.5 WebAuthn/FIDO2 Implementation

**File:** `CRM.Infrastructure/Services/WebAuthnService.cs` ⏳ TODO

**NuGet Package:** `Fido2.AspNet` v3.0+

**Configuration:**
```json
{
  "TwoFactor": {
    "WebAuthn": {
      "Enabled": true,
      "RelyingPartyId": "example.com",
      "RelyingPartyName": "CRM Solution",
      "UserVerificationRequirement": "preferred"
    }
  }
}
```

---

## 9. Issues & Known Gaps

| Issue ID | Description | Status | Resolution |
|----------|-------------|--------|-----------|
| AUTH-001 | Legacy SHA-256 password hashes still supported | ⚠️ Partial | Only for migration; new passwords use BCrypt |
| AUTH-002 | Concurrent session limits not enforced | ⏳ TODO | Implement MaxConcurrentSessions check |
| AUTH-003 | Password history validation not implemented | ⏳ TODO | Prevent reuse of last N passwords |
| AUTH-004 | Session timeout not enforced on frontend | ⏳ TODO | Auto-logout after inactivity |
| AUTH-005 | LinkedIn OAuth not implemented | ⏳ TODO | Missing provider implementation |
| AUTH-006 | Apple OAuth not implemented | ⏳ TODO | Missing provider implementation |
| AUTH-007 | SMS OTP not implemented | ⏳ TODO | Missing Twilio integration |
| AUTH-008 | WebAuthn/FIDO2 not implemented | ⏳ TODO | Missing FIDO2 support |

---

## 10. TODOs

### High Priority - Social Providers
- [ ] TODO-AUTH-001: Implement LinkedIn OAuth provider
- [ ] TODO-AUTH-002: Implement Apple OAuth provider (privacy-focused)
- [ ] TODO-AUTH-003: Add Okta/Enterprise SSO support
- [ ] TODO-AUTH-004: Implement generic OpenID Connect provider
- [ ] TODO-AUTH-005: Add OAuth provider state validation and CSRF protection
- [ ] TODO-AUTH-006: Implement OAuth token refresh for long-lived sessions

### High Priority - 2FA
- [ ] TODO-AUTH-007: Implement SMS OTP via Twilio integration
- [ ] TODO-AUTH-008: Implement Email OTP via SendGrid
- [ ] TODO-AUTH-009: Implement WebAuthn/FIDO2 support
- [ ] TODO-AUTH-010: Add biometric login (platform-specific)
- [ ] TODO-AUTH-011: Add 2FA enforcement policies per user group
- [ ] TODO-AUTH-012: Implement backup code regeneration

### Medium Priority
- [ ] TODO-AUTH-013: Add concurrent session limit enforcement
- [ ] TODO-AUTH-014: Implement password history validation (last 5 passwords)
- [ ] TODO-AUTH-015: Implement IP-based session binding
- [ ] TODO-AUTH-016: Add audit logging for all auth events
- [ ] TODO-AUTH-017: Implement passwordless login (magic links)
- [ ] TODO-AUTH-018: Add OAuth provider account linking/unlinking
- [ ] TODO-AUTH-019: Implement 2FA device trust (remember device)

### Low Priority
- [ ] TODO-AUTH-020: Implement session activity tracking dashboard
- [ ] TODO-AUTH-021: Add login analytics and anomaly detection
- [ ] TODO-AUTH-022: Implement risk-based authentication
- [ ] TODO-AUTH-023: Add OAuth provider device flow support
- [ ] TODO-AUTH-024: Implement geolocation-based login alerts

---

## 11. Provider Setup Guides

### Google OAuth Setup
1. Go to https://console.cloud.google.com/
2. Create new project
3. Enable Google+ API
4. Create OAuth 2.0 credential (Web application)
5. Add Authorized redirect URIs: `https://api.example.com/api/auths/oauth/google/callback`
6. Copy Client ID and Client Secret to appsettings.json

### Microsoft OAuth Setup
1. Go to https://portal.azure.com/
2. Register application in Azure AD
3. Add Web redirect URI: `https://api.example.com/api/auths/oauth/microsoft/callback`
4. Create client secret
5. Copy Application (client) ID and secret to appsettings.json

### GitHub OAuth Setup
1. Go to https://github.com/settings/developers
2. Create new OAuth App
3. Set Authorization callback URL: `https://api.example.com/api/auths/oauth/github/callback`
4. Copy Client ID and Client Secret to appsettings.json

### LinkedIn OAuth Setup
1. Go to https://www.linkedin.com/developers/apps
2. Create new app
3. Request access to Sign In with LinkedIn
4. Add Authorized redirect URLs: `https://api.example.com/api/auths/oauth/linkedin/callback`
5. Copy Client ID and Client Secret to appsettings.json

### Apple OAuth Setup
1. Go to https://developer.apple.com/
2. Create App ID (Service ID)
3. Configure Sign in with Apple
4. Register return URLs: `https://api.example.com/api/auths/oauth/apple/callback`
5. Create private key (download .p8 file)
6. Copy key details to appsettings.json

---

## 12. 2FA Provider Setup Guides

### Twilio SMS OTP Setup
1. Create Twilio account at https://www.twilio.com/
2. Get Account SID and Auth Token
3. Get Twilio phone number for SMS
4. Add configuration to appsettings.json
5. Test SMS delivery with test credentials

### SendGrid Email OTP Setup
1. Create SendGrid account at https://sendgrid.com/
2. Create API key
3. Create email template for OTP
4. Add configuration to appsettings.json
5. Test email delivery

### WebAuthn/FIDO2 Setup
1. Install Fido2.AspNet NuGet package
2. Configure Relying Party settings (domain, origin)
3. Implement registration and authentication flows
4. Test with Windows Hello, Touch ID, or security key

---

## 13. References

- [SOLUTION_CONTEXT.md](../development/SOLUTION_CONTEXT.md#6-authentication--security) - Authentication section
- [ARCHITECTURE_OVERVIEW.md](../development/ARCHITECTURE_OVERVIEW.md#security-architecture) - Security architecture
- [PHASE4_SERVICE_SPECIFICATIONS.md](../PHASE4_SERVICE_SPECIFICATIONS.md) - Related services
- RFC 6234: US Secure Hash and Signature Algorithms
- RFC 7519: JSON Web Token (JWT)
- RFC 6238: Time-Based One-Time Password Algorithm (TOTP)
- OAuth 2.0 Authorization Framework (RFC 6749)
- OpenID Connect Core 1.0
- FIDO2 WebAuthn Specification
- NIST SP 800-63B: Digital Identity Guidelines

---

**END OF SPECIFICATION**
