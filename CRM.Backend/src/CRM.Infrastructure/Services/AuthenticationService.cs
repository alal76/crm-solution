// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Authentication Service for handling user registration, login, and token management
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<OAuthToken> _oauthTokenRepository;
    private readonly ICrmDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITotpService _totpService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IRepository<User> userRepository,
        IRepository<OAuthToken> oauthTokenRepository,
        ICrmDbContext dbContext,
        IJwtTokenService jwtTokenService,
        ITotpService totpService,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _oauthTokenRepository = oauthTokenRepository;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _totpService = totpService;
        _logger = logger;
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
        // Get user by email with navigation properties for permissions
        var normalizedEmail = request.Email?.Trim().ToLower() ?? "";
        var user = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .Include(u => u.Department)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => !u.IsDeleted && u.Email != null && u.Email.ToLower() == normalizedEmail);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            // Add detailed logging for debugging
            if (user == null)
                Console.WriteLine($"[LOGIN] User not found for email: '{normalizedEmail}'");
            else
                Console.WriteLine($"[LOGIN] Password verification failed for user: '{user.Email}'");

            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive");

        // Check if 2FA is enabled for this user
        if (user.TwoFactorEnabled && !string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            // Generate a temporary token for 2FA verification
            var tempToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            
            // Store the temp token (in production, use a cache with expiry)
            // For now, we'll use a simple approach with the password reset token field temporarily
            user.PasswordResetToken = tempToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(5); // 5 minute expiry
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveAsync();
            
            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RequiresTwoFactor = true,
                TwoFactorEnabled = true,
                TwoFactorToken = tempToken
            };
        }

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

        // Build group permissions from user's primary group or admin role
        GroupPermissionsDto? groupPermissions = null;
        
        // Check if user is Admin role - grant all permissions
        if (user.Role == (int)CRM.Core.Entities.UserRole.Admin)
        {
            groupPermissions = new GroupPermissionsDto
            {
                IsSystemAdmin = true,
                CanAccessDashboard = true,
                CanAccessCustomers = true,
                CanAccessContacts = true,
                CanAccessLeads = true,
                CanAccessOpportunities = true,
                CanAccessProducts = true,
                CanAccessServices = true,
                CanAccessCampaigns = true,
                CanAccessQuotes = true,
                CanAccessTasks = true,
                CanAccessActivities = true,
                CanAccessNotes = true,
                CanAccessWorkflows = true,
                CanAccessServiceRequests = true,
                CanAccessReports = true,
                CanAccessSettings = true,
                CanAccessUserManagement = true,
                // All CRUD permissions
                CanCreateCustomers = true, CanEditCustomers = true, CanDeleteCustomers = true, CanViewAllCustomers = true,
                CanCreateContacts = true, CanEditContacts = true, CanDeleteContacts = true,
                CanCreateLeads = true, CanEditLeads = true, CanDeleteLeads = true, CanConvertLeads = true,
                CanCreateOpportunities = true, CanEditOpportunities = true, CanDeleteOpportunities = true, CanCloseOpportunities = true,
                CanCreateProducts = true, CanEditProducts = true, CanDeleteProducts = true, CanManagePricing = true,
                CanCreateCampaigns = true, CanEditCampaigns = true, CanDeleteCampaigns = true, CanLaunchCampaigns = true,
                CanCreateQuotes = true, CanEditQuotes = true, CanDeleteQuotes = true, CanApproveQuotes = true,
                CanCreateTasks = true, CanEditTasks = true, CanDeleteTasks = true, CanAssignTasks = true,
                CanCreateWorkflows = true, CanEditWorkflows = true, CanDeleteWorkflows = true, CanActivateWorkflows = true,
                DataAccessScope = "all",
                CanExportData = true, CanImportData = true, CanBulkEdit = true, CanBulkDelete = true
            };
        }
        else if (user.PrimaryGroup != null)
        {
            // Get permissions from user's primary group
            var group = user.PrimaryGroup;
            groupPermissions = new GroupPermissionsDto
            {
                IsSystemAdmin = group.IsSystemAdmin,
                CanAccessDashboard = group.CanAccessDashboard,
                CanAccessCustomers = group.CanAccessCustomers,
                CanAccessContacts = group.CanAccessContacts,
                CanAccessLeads = group.CanAccessLeads,
                CanAccessOpportunities = group.CanAccessOpportunities,
                CanAccessProducts = group.CanAccessProducts,
                CanAccessServices = group.CanAccessServices,
                CanAccessCampaigns = group.CanAccessCampaigns,
                CanAccessQuotes = group.CanAccessQuotes,
                CanAccessTasks = group.CanAccessTasks,
                CanAccessActivities = group.CanAccessActivities,
                CanAccessNotes = group.CanAccessNotes,
                CanAccessWorkflows = group.CanAccessWorkflows,
                CanAccessServiceRequests = group.CanAccessServiceRequests,
                CanAccessReports = group.CanAccessReports,
                CanAccessSettings = group.CanAccessSettings,
                CanAccessUserManagement = group.CanAccessUserManagement,
                CanCreateCustomers = group.CanCreateCustomers,
                CanEditCustomers = group.CanEditCustomers,
                CanDeleteCustomers = group.CanDeleteCustomers,
                CanViewAllCustomers = group.CanViewAllCustomers,
                CanCreateContacts = group.CanCreateContacts,
                CanEditContacts = group.CanEditContacts,
                CanDeleteContacts = group.CanDeleteContacts,
                CanCreateLeads = group.CanCreateLeads,
                CanEditLeads = group.CanEditLeads,
                CanDeleteLeads = group.CanDeleteLeads,
                CanConvertLeads = group.CanConvertLeads,
                CanCreateOpportunities = group.CanCreateOpportunities,
                CanEditOpportunities = group.CanEditOpportunities,
                CanDeleteOpportunities = group.CanDeleteOpportunities,
                CanCloseOpportunities = group.CanCloseOpportunities,
                CanCreateProducts = group.CanCreateProducts,
                CanEditProducts = group.CanEditProducts,
                CanDeleteProducts = group.CanDeleteProducts,
                CanManagePricing = group.CanManagePricing,
                CanCreateCampaigns = group.CanCreateCampaigns,
                CanEditCampaigns = group.CanEditCampaigns,
                CanDeleteCampaigns = group.CanDeleteCampaigns,
                CanLaunchCampaigns = group.CanLaunchCampaigns,
                CanCreateQuotes = group.CanCreateQuotes,
                CanEditQuotes = group.CanEditQuotes,
                CanDeleteQuotes = group.CanDeleteQuotes,
                CanApproveQuotes = group.CanApproveQuotes,
                CanCreateTasks = group.CanCreateTasks,
                CanEditTasks = group.CanEditTasks,
                CanDeleteTasks = group.CanDeleteTasks,
                CanAssignTasks = group.CanAssignTasks,
                CanCreateWorkflows = group.CanCreateWorkflows,
                CanEditWorkflows = group.CanEditWorkflows,
                CanDeleteWorkflows = group.CanDeleteWorkflows,
                CanActivateWorkflows = group.CanActivateWorkflows,
                DataAccessScope = group.DataAccessScope,
                CanExportData = group.CanExportData,
                CanImportData = group.CanImportData,
                CanBulkEdit = group.CanBulkEdit,
                CanBulkDelete = group.CanBulkDelete
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
            PrimaryGroupId = user.PrimaryGroupId,
            PrimaryGroupName = user.PrimaryGroup?.Name,
            AccessiblePages = accessiblePages,
            Permissions = permissions,
            GroupPermissions = groupPermissions,
            HeaderColor = user.HeaderColor ?? (user.Role == 0 ? "#C62828" : null), // Red for admin
            PhotoUrl = user.PhotoUrl
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

    public async Task<AuthResponse> VerifyTwoFactorLoginAsync(string tempToken, string code)
    {
        // Find user by temp token
        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => 
            u.PasswordResetToken == tempToken && 
            u.PasswordResetTokenExpiry > DateTime.UtcNow);
        
        if (user == null)
            throw new UnauthorizedAccessException("Invalid or expired verification token");
        
        if (string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new InvalidOperationException("2FA not configured for this user");
        
        // Verify the TOTP code
        var isValid = _totpService.VerifyCode(user.TwoFactorSecret, code);
        
        // Check backup codes if TOTP fails
        if (!isValid && !string.IsNullOrEmpty(user.BackupCodes))
        {
            var backupCodes = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.BackupCodes) ?? new();
            if (backupCodes.Contains(code))
            {
                isValid = true;
                // Remove used backup code
                backupCodes.Remove(code);
                user.BackupCodes = System.Text.Json.JsonSerializer.Serialize(backupCodes);
            }
        }
        
        if (!isValid)
            throw new UnauthorizedAccessException("Invalid verification code");
        
        // Clear the temp token
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.LastLoginDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();
        
        // Load navigation properties for full response
        var fullUser = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .Include(u => u.Department)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == user.Id);
        
        return GenerateAuthResponse(fullUser ?? user);
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

    public async Task<bool> AdminResetPasswordAsync(int userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("New password is required");

        if (newPassword.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
            throw new InvalidOperationException("User not found");

        user.PasswordHash = HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        _logger.LogInformation($"Admin reset password for user {userId}");
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
