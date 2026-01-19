using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Authentication Service for handling user registration, login, and token management
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<OAuthToken> _oauthTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITotpService _totpService;

    public AuthenticationService(
        IRepository<User> userRepository,
        IRepository<OAuthToken> oauthTokenRepository,
        IJwtTokenService jwtTokenService,
        ITotpService totpService)
    {
        _userRepository = userRepository;
        _oauthTokenRepository = oauthTokenRepository;
        _jwtTokenService = jwtTokenService;
        _totpService = totpService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Email and password are required");

        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match");

        // Check if user already exists
        var existingUser = await _userRepository.GetAllAsync();
        if (existingUser.Any(u => u.Email == request.Email))
            throw new InvalidOperationException("User with this email already exists");

        // Create new user
        var user = new User
        {
            Username = request.Username ?? request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = HashPassword(request.Password),
            Role = (int)CRM.Core.Entities.UserRole.Sales, // Default role
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveAsync();

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Get user by email - trim both email from request and in database
        var users = await _userRepository.GetAllAsync();
        var normalizedEmail = request.Email?.Trim().ToLower() ?? "";
        var user = users.FirstOrDefault(u => (u.Email?.Trim().ToLower() ?? "") == normalizedEmail);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            // Add detailed logging for debugging
            if (user == null)
                Console.WriteLine($"[LOGIN] User not found for email: '{normalizedEmail}'. Available users: {string.Join(", ", users.Select(u => u.Email?.Trim().ToLower() ?? "null"))}");
            else
                Console.WriteLine($"[LOGIN] Password verification failed for user: '{user.Email}'. Hash from DB: {user.PasswordHash}");

            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive");

        // Update last login date
        user.LastLoginDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> OAuthLoginAsync(OAuthLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("Provider and token are required");

        // Validate and extract user info from the provider token
        var (providerUserId, email, firstName, lastName) = await ValidateProviderTokenAsync(request.Provider, request.Token);

        // Check if OAuth token already exists
        var oauthTokens = await _oauthTokenRepository.GetAllAsync();
        var existingToken = oauthTokens.FirstOrDefault(t =>
            t.Provider == request.Provider && t.ProviderUserId == providerUserId);

        User user;

        if (existingToken != null && existingToken.User != null)
        {
            user = existingToken.User;
        }
        else
        {
            // Check if user with this email exists
            var allUsers = await _userRepository.GetAllAsync();
            user = allUsers.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                // Create new user from OAuth provider
                user = new User
                {
                    Username = email.Split('@')[0] + "_" + providerUserId.Substring(0, 6),
                    Email = email,
                    FirstName = firstName ?? request.Provider,
                    LastName = lastName ?? "User",
                    PasswordHash = HashPassword(Guid.NewGuid().ToString()), // Random password
                    Role = (int)CRM.Core.Entities.UserRole.Sales, // Default role
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveAsync();
            }

            // Store OAuth token
            var newOAuthToken = new OAuthToken
            {
                UserId = user.Id,
                Provider = request.Provider,
                ProviderUserId = providerUserId,
                AccessToken = request.Token,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };

            await _oauthTokenRepository.AddAsync(newOAuthToken);
            await _oauthTokenRepository.SaveAsync();
        }

        user.LastLoginDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // In a production app, you would validate the refresh token against stored tokens
        // For now, this is a placeholder
        throw new NotImplementedException("Refresh token functionality is not yet implemented");
    }

    public async Task<bool> VerifyTokenAsync(string token)
    {
        return _jwtTokenService.ValidateToken(token);
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<User> UpdateUserAsync(int userId, User user)
    {
        var existingUser = await _userRepository.GetByIdAsync(userId);
        if (existingUser == null)
            throw new InvalidOperationException("User not found");

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;

        await _userRepository.UpdateAsync(existingUser);
        await _userRepository.SaveAsync();

        return existingUser;
    }

    // Helper methods
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        var matches = hashOfInput == hash;
        Console.WriteLine($"[PASSWORD_VERIFY] Input hash: {hashOfInput}");
        Console.WriteLine($"[PASSWORD_VERIFY] Stored hash: {hash}");
        Console.WriteLine($"[PASSWORD_VERIFY] Match: {matches}");
        return matches;
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Extract accessible pages from profile
        var accessiblePages = new List<string>();
        if (user.UserProfile != null)
        {
            try
            {
                accessiblePages = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.UserProfile.AccessiblePages) ?? new();
            }
            catch { }
        }

        // Build permissions object from profile
        var permissions = new CRM.Core.Dtos.UserPermissions();
        if (user.UserProfile != null)
        {
            permissions = new CRM.Core.Dtos.UserPermissions
            {
                CanCreateCustomers = user.UserProfile.CanCreateCustomers,
                CanEditCustomers = user.UserProfile.CanEditCustomers,
                CanDeleteCustomers = user.UserProfile.CanDeleteCustomers,
                CanCreateOpportunities = user.UserProfile.CanCreateOpportunities,
                CanEditOpportunities = user.UserProfile.CanEditOpportunities,
                CanDeleteOpportunities = user.UserProfile.CanDeleteOpportunities,
                CanCreateProducts = user.UserProfile.CanCreateProducts,
                CanEditProducts = user.UserProfile.CanEditProducts,
                CanDeleteProducts = user.UserProfile.CanDeleteProducts,
                CanManageCampaigns = user.UserProfile.CanManageCampaigns,
                CanViewReports = user.UserProfile.CanViewReports,
                CanManageUsers = user.UserProfile.CanManageUsers
            };
        }

        return new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = Enum.GetName(typeof(CRM.Core.Entities.UserRole), user.Role) ?? "Guest",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.Name,
            UserProfileId = user.UserProfileId,
            UserProfileName = user.UserProfile?.Name,
            AccessiblePages = accessiblePages,
            Permissions = permissions
        };
    }

    private string ExtractUserIdFromToken(string token, string provider)
    {
        // Simplified implementation - in production, verify the token with the actual provider
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{provider}_{Guid.NewGuid()}")).Substring(0, 16);
    }

    private async Task<(string userId, string email, string? firstName, string? lastName)> ValidateProviderTokenAsync(string provider, string token)
    {
        return await Task.Run(async () =>
        {
            try
            {
                switch (provider.ToLower())
                {
                    case "google":
                        return await ValidateGoogleTokenAsync(token);
                    case "microsoft":
                        return await ValidateMicrosoftTokenAsync(token);
                    default:
                        throw new InvalidOperationException($"Unsupported OAuth provider: {provider}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Token validation failed for provider '{provider}': {ex.Message}", ex);
            }
        });
    }

    private async Task<(string userId, string email, string? firstName, string? lastName)> ValidateGoogleTokenAsync(string token)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Note: In production, you would verify the JWT signature with Google's public keys
                // For now, we'll parse the token (JWT format: header.payload.signature)
                var parts = token.Split('.');
                if (parts.Length != 3)
                    throw new InvalidOperationException("Invalid token format");

                // Decode the payload (second part)
                var payload = parts[1];
                // Add padding if needed
                var padding = 4 - (payload.Length % 4);
                if (padding != 4)
                    payload += new string('=', padding);

                var decodedBytes = Convert.FromBase64String(payload);
                var json = Encoding.UTF8.GetString(decodedBytes);
                var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                var sub = root.GetProperty("sub").GetString() ?? throw new InvalidOperationException("Missing 'sub' claim");
                var email = root.GetProperty("email").GetString() ?? throw new InvalidOperationException("Missing 'email' claim");
                var firstName = root.TryGetProperty("given_name", out var gn) ? gn.GetString() : null;
                var lastName = root.TryGetProperty("family_name", out var fn) ? fn.GetString() : null;

                return (sub, email, firstName, lastName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to validate Google token", ex);
            }
        });
    }

    private async Task<(string userId, string email, string? firstName, string? lastName)> ValidateMicrosoftTokenAsync(string token)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Note: In production, you would verify the JWT signature with Microsoft's public keys
                var parts = token.Split('.');
                if (parts.Length != 3)
                    throw new InvalidOperationException("Invalid token format");

                // Decode the payload (second part)
                var payload = parts[1];
                // Add padding if needed
                var padding = 4 - (payload.Length % 4);
                if (padding != 4)
                    payload += new string('=', padding);

                var decodedBytes = Convert.FromBase64String(payload);
                var json = Encoding.UTF8.GetString(decodedBytes);
                var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                var oid = root.GetProperty("oid").GetString() ?? throw new InvalidOperationException("Missing 'oid' claim");
                var email = root.GetProperty("upn").GetString() ?? throw new InvalidOperationException("Missing 'upn' claim");
                var name = root.TryGetProperty("name", out var n) ? n.GetString() : null;

                var firstName = name?.Split(' ').FirstOrDefault();
                var lastName = name?.Split(' ').Skip(1).FirstOrDefault();

                return (oid, email, firstName, lastName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to validate Microsoft token", ex);
            }
        });
    }

    // Two-Factor Authentication Methods
    public async Task<TwoFactorSetupResponse> SetupTwoFactorAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var secret = _totpService.GenerateSecret();
        var backupCodes = _totpService.GenerateBackupCodes();
        var qrCodeUrl = _totpService.GetQrCodeUrl(secret, user.Email, "CRM Solution");

        // Don't save yet - user needs to verify the code first
        return new TwoFactorSetupResponse
        {
            QrCodeUrl = qrCodeUrl,
            Secret = secret,
            BackupCodes = backupCodes
        };
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(int userId, string code)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new InvalidOperationException("User or 2FA not configured");

        return _totpService.VerifyCode(user.TwoFactorSecret, code);
    }

    public async Task EnableTwoFactorAsync(int userId, string secret, List<string> backupCodes)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        user.TwoFactorSecret = secret;
        user.BackupCodes = System.Text.Json.JsonSerializer.Serialize(backupCodes);
        user.TwoFactorEnabled = true;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();
    }

    public async Task DisableTwoFactorAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.BackupCodes = null;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();
    }

    // Password Reset Methods
    public async Task<string> RequestPasswordResetAsync(string email)
    {
        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Email == email);

        if (user == null)
            throw new InvalidOperationException("User with this email not found");

        var resetToken = GenerateRandomToken();
        user.PasswordResetToken = HashPassword(resetToken);
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        // In production, send email with reset link
        return resetToken;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("Token and new password are required");

        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u =>
            u.PasswordResetTokenExpiry != null &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (user == null)
            throw new InvalidOperationException("Invalid or expired password reset token");

        // Verify token
        if (!VerifyPassword(token, user.PasswordResetToken ?? ""))
            throw new UnauthorizedAccessException("Invalid password reset token");

        user.PasswordHash = HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        return true;
    }

    // Helper Methods
    private string GenerateRandomToken()
    {
        var randomBytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }
}
