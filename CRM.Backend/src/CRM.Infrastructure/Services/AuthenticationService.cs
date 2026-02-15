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

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Input;
using CRM.Core.Ports.Output.Providers;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Authentication Service for handling user registration, login, and token management.
///
/// HEXAGONAL ARCHITECTURE:
/// - Implements IAuthInputPort (primary/driving port)
/// - Implements IAuthenticationService (backward compatibility)
/// - Uses IRepository and IJwtTokenService (secondary/driven ports)
///
/// NOTE: Authentication ALWAYS uses the production database context, regardless of demo mode.
/// This ensures admin users exist and can authenticate even when demo mode is active.
/// </summary>
public class AuthenticationService : IAuthenticationService, IAuthInputPort
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<OAuthToken> _oauthTokenRepository;
    private readonly CrmDbContext _dbContext; // Always use production context for auth
    private readonly IJwtTokenService _jwtTokenService;
    private readonly CRM.Core.Interfaces.ITotpService _totpService;
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly INotificationPort _notificationPort;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IRepository<User> userRepository,
        IRepository<OAuthToken> oauthTokenRepository,
        CrmDbContext dbContext, // Use concrete production context for auth
        IJwtTokenService jwtTokenService,
        CRM.Core.Interfaces.ITotpService totpService,
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory,
        INotificationPort notificationPort,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _oauthTokenRepository = oauthTokenRepository;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _totpService = totpService;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _notificationPort = notificationPort;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Persist a refresh token to the dedicated RefreshTokens table.
    /// Revokes all existing active tokens for the user to enforce single-session (can be relaxed later for multi-device).
    /// </summary>
    private async Task PersistRefreshTokenAsync(User user, string tokenString, string? ipAddress = null, string? deviceInfo = null)
    {
        // Revoke all existing active refresh tokens for this user
        var activeTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAt = DateTime.UtcNow;
            activeToken.RevokedReason = "Replaced by new login";
            activeToken.ReplacedByToken = tokenString;
        }

        // Create new refresh token record
        var refreshTokenEntity = new Core.Entities.RefreshToken
        {
            Token = tokenString,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Email and password are required");

        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match");

        // Check if user already exists
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        // Check if there's already a pending approval request for this email
        var existingRequest = await _dbContext.UserApprovalRequests
            .FirstOrDefaultAsync(r => r.Email == request.Email && r.Status == 0); // Status 0 = Pending
        if (existingRequest != null)
            throw new InvalidOperationException("A registration request for this email is already pending approval");

        // Check system settings for approval requirement
        var systemSettings = await _dbContext.SystemSettings.FirstOrDefaultAsync();
        var requireApproval = systemSettings?.RequireApprovalForNewUsers ?? true;

        if (requireApproval)
        {
            // Create approval request instead of user directly
            var approvalRequest = new UserApprovalRequest
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = HashPassword(request.Password), // Store password hash for later use when approved
                Status = 0, // Pending
                RequestedAt = DateTime.UtcNow
            };

            _dbContext.UserApprovalRequests.Add(approvalRequest);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Registration request created for {Email}, pending approval", request.Email);

            // Return a response indicating pending approval
            return new AuthResponse
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Message = "Your registration is pending approval. You will be notified when your account is activated.",
                RequiresApproval = true
            };
        }

        // If approval not required, create user directly (existing behavior)
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

        var response = GenerateAuthResponse(user);
        await PersistRefreshTokenAsync(user, response.RefreshToken);
        return response;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        // Get user by email with navigation properties for permissions
        var normalizedEmail = request.Email?.Trim().ToLower() ?? "";
        var user = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .Include(u => u.Department)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => !u.IsDeleted && u.Email != null && u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found for email: {Email}", normalizedEmail);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Check if password has never been set - allow login with any password to redirect to setup
        if (user.PasswordNeverSet)
        {
            _logger.LogInformation("User {Email} requires password setup - redirecting to password setup", normalizedEmail);

            if (!user.IsActive)
                throw new UnauthorizedAccessException("User account is inactive");

            var passwordSetupToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var cacheKey = $"password_setup_{passwordSetupToken}";
            _cache.Set(cacheKey, user.Id, TimeSpan.FromMinutes(15));

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RequiresPasswordSetup = true,
                MustChangePassword = false,
                PasswordSetupToken = passwordSetupToken
            };
        }

        // For users with passwords set, verify the password
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed - invalid password for email: {Email}", normalizedEmail);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive");

        // Check if password must be reset (admin-forced)
        if (user.MustResetPassword)
        {
            var passwordSetupToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var cacheKey = $"password_setup_{passwordSetupToken}";
            _cache.Set(cacheKey, user.Id, TimeSpan.FromMinutes(15));

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RequiresPasswordSetup = false,
                MustChangePassword = true,
                PasswordSetupToken = passwordSetupToken
            };
        }

        // Check password expiration based on group policy
        var passwordStatus = CheckPasswordExpiration(user);
        if (passwordStatus.isExpired)
        {
            var passwordSetupToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var cacheKey = $"password_setup_{passwordSetupToken}";
            _cache.Set(cacheKey, user.Id, TimeSpan.FromMinutes(15));

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PasswordExpired = true,
                PasswordSetupToken = passwordSetupToken
            };
        }

        // Check if 2FA is enabled for this user
        if (user.TwoFactorEnabled && !string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            // Generate a temporary token for 2FA verification
            var tempToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            // Store the temp token in memory cache with 5 minute expiry
            var cacheKey = $"2fa_token_{tempToken}";
            _cache.Set(cacheKey, user.Id, TimeSpan.FromMinutes(5));

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RequiresTwoFactor = true,
                TwoFactorEnabled = true,
                TwoFactorToken = tempToken,
                // Include password expiration warning even for 2FA flow
                PasswordExpirationWarning = passwordStatus.isWarning,
                DaysUntilPasswordExpiration = passwordStatus.daysRemaining
            };
        }

        // Update last login date
        user.LastLoginAt = DateTime.UtcNow;

        // Generate response with tokens
        var response = GenerateAuthResponse(user);
        _logger.LogInformation("LoginAsync tokens generated in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

        // Add password expiration warning to response
        response.PasswordExpirationWarning = passwordStatus.isWarning;
        response.DaysUntilPasswordExpiration = passwordStatus.daysRemaining;

        // Store refresh token in dedicated table
        await PersistRefreshTokenAsync(user, response.RefreshToken);
        _logger.LogInformation("LoginAsync refresh token persisted in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

        // Update last login date (already set above, save via repository)
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();
        _logger.LogInformation("LoginAsync user update saved in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

        _logger.LogInformation("LoginAsync completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        return response;
    }

    /// <summary>
    /// Check password expiration based on user's primary group policy
    /// </summary>
    private (bool isExpired, bool isWarning, int? daysRemaining) CheckPasswordExpiration(User user)
    {
        // If no group or no password last changed date, assume not expired
        if (user.PrimaryGroup == null || user.PasswordLastChangedAt == null)
            return (false, false, null);

        var group = user.PrimaryGroup;

        // If no expiration policy or expiration days, password doesn't expire
        if (group.PasswordExpirationPolicy == PasswordExpirationPolicy.None ||
            group.PasswordExpirationDays == null || group.PasswordExpirationDays <= 0)
            return (false, false, null);

        var passwordAge = (DateTime.UtcNow - user.PasswordLastChangedAt.Value).TotalDays;
        var daysRemaining = (int)(group.PasswordExpirationDays.Value - passwordAge);

        // Password has expired
        if (passwordAge >= group.PasswordExpirationDays.Value)
        {
            // If policy is MustChange, block login
            if (group.PasswordExpirationPolicy == PasswordExpirationPolicy.MustChange)
                return (true, false, 0);

            // For Alert policy, allow login but indicate expiration
            if (group.PasswordExpirationPolicy == PasswordExpirationPolicy.Alert)
                return (false, true, 0);
        }

        // Check for warning period
        var warningDays = group.PasswordExpirationWarningDays ?? 7;
        if (daysRemaining <= warningDays && daysRemaining > 0)
        {
            return (false, true, daysRemaining);
        }

        return (false, false, daysRemaining > 0 ? daysRemaining : null);
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
            user = allUsers.FirstOrDefault(u => u.Email == email)!;

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

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        var response = GenerateAuthResponse(user);
        await PersistRefreshTokenAsync(user, response.RefreshToken);
        return response;
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token is required");

        // Find the refresh token record in the dedicated table
        var storedToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Detect token reuse: if this token was already revoked, revoke ALL tokens for this user (potential theft)
        if (storedToken.IsRevoked)
        {
            _logger.LogWarning("Refresh token reuse detected for UserId {UserId}. Revoking all tokens.", storedToken.UserId);
            var allUserTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in allUserTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = "Revoked due to token reuse detection";
            }
            await _dbContext.SaveChangesAsync();

            throw new UnauthorizedAccessException("Token has been revoked — all sessions invalidated for security");
        }

        // Check if token is expired
        if (storedToken.IsExpired)
            throw new UnauthorizedAccessException("Refresh token has expired");

        // Verify the user is still active
        if (storedToken.User == null || storedToken.User.IsDeleted || !storedToken.User.IsActive)
            throw new UnauthorizedAccessException("User account is inactive or deleted");

        // Load full user data for response (with navigation properties)
        var fullUser = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .Include(u => u.Department)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == storedToken.UserId);

        if (fullUser == null)
            throw new UnauthorizedAccessException("User not found");

        // Generate new tokens (rotation)
        var response = GenerateAuthResponse(fullUser);

        // Revoke old token and create new one (token rotation)
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByToken = response.RefreshToken;
        storedToken.RevokedReason = "Rotated on refresh";

        var newRefreshToken = new Core.Entities.RefreshToken
        {
            Token = response.RefreshToken,
            UserId = fullUser.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IpAddress = storedToken.IpAddress,
            DeviceInfo = storedToken.DeviceInfo,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync();

        return response;
    }

    public Task<bool> VerifyTokenAsync(string token)
    {
        return Task.FromResult(_jwtTokenService.ValidateToken(token));
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
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            // Support BCrypt hashes (preferred)
            if (hash.StartsWith("$2"))
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }

            // Legacy support for old SHA-256 hashes (will be migrated on next password change)
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hashOfInput = Convert.ToBase64String(hashedBytes);
                return hashOfInput == hash;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Password verification failed");
            return false;
        }
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Extract accessible pages from profile
        var accessiblePages = new List<string>();
        if (user.UserProfile != null && !string.IsNullOrEmpty(user.UserProfile.AccessiblePages))
        {
            try
            {
                accessiblePages = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.UserProfile.AccessiblePages) ?? new();
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse AccessiblePages for user {UserId}", user.Id);
            }
        }

        // Build permissions object from profile
        var permissions = new CRM.Core.Dtos.UserPermissions();
        if (user.UserProfile != null)
        {
            permissions = new CRM.Core.Dtos.UserPermissions
            {
                CanCreateAccounts = user.UserProfile.CanCreateAccounts,
                CanEditAccounts = user.UserProfile.CanEditAccounts,
                CanDeleteAccounts = user.UserProfile.CanDeleteAccounts,
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
                CanAccessAccounts = true,
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
                CanAccessITSM = true,
                CanAccessReports = true,
                CanAccessSettings = true,
                CanAccessUserManagement = true,
                // All CRUD permissions
                CanCreateAccounts = true, CanEditAccounts = true, CanDeleteAccounts = true, CanViewAllAccounts = true,
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
                CanAccessAccounts = group.CanAccessAccounts,
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
                CanAccessITSM = group.CanAccessITSM,
                CanAccessReports = group.CanAccessReports,
                CanAccessSettings = group.CanAccessSettings,
                CanAccessUserManagement = group.CanAccessUserManagement,
                CanCreateAccounts = group.CanCreateAccounts,
                CanEditAccounts = group.CanEditAccounts,
                CanDeleteAccounts = group.CanDeleteAccounts,
                CanViewAllAccounts = group.CanViewAllAccounts,
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
            PhotoUrl = user.PhotoUrl,
            ThemePreference = user.ThemePreference ?? "system"
        };
    }

    private string ExtractUserIdFromToken(string token, string provider)
    {
        // Simplified implementation - in production, verify the token with the actual provider
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{provider}_{Guid.NewGuid()}")).Substring(0, 16);
    }

    private async Task<(string userId, string email, string? firstName, string? lastName)> ValidateProviderTokenAsync(string provider, string token)
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
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Token validation failed for provider '{provider}': {ex.Message}", ex);
        }
    }

    private async Task<(string userId, string email, string? firstName, string? lastName)> ValidateGoogleTokenAsync(string token)
    {
        var client = _httpClientFactory.CreateClient();

        // Verify the token with Google's tokeninfo endpoint — this validates the JWT signature server-side
        var response = await client.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(token)}");
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Google token validation failed with status {StatusCode}: {Error}", response.StatusCode, errorBody);
            throw new InvalidOperationException("Invalid Google token: verification failed");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(json);
        var root = jsonDoc.RootElement;

        var sub = root.TryGetProperty("sub", out var subProp) ? subProp.GetString() : null;
        var email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;

        if (string.IsNullOrEmpty(sub))
            throw new InvalidOperationException("Google token missing 'sub' claim");
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Google token missing 'email' claim");

        var firstName = root.TryGetProperty("given_name", out var gn) ? gn.GetString() : null;
        var lastName = root.TryGetProperty("family_name", out var fn) ? fn.GetString() : null;

        return (sub, email, firstName, lastName);
    }

    private async Task<(string userId, string email, string? firstName, string? lastName)> ValidateMicrosoftTokenAsync(string token)
    {
        var client = _httpClientFactory.CreateClient();

        // Validate the token by calling Microsoft Graph — if the token is invalid, Graph rejects it
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Microsoft token validation failed with status {StatusCode}: {Error}", response.StatusCode, errorBody);
            throw new InvalidOperationException("Invalid Microsoft token: verification failed");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(json);
        var root = jsonDoc.RootElement;

        var id = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        // Microsoft Graph returns 'mail' for most accounts, fallback to 'userPrincipalName'
        var email = root.TryGetProperty("mail", out var mailProp) ? mailProp.GetString() : null;
        if (string.IsNullOrEmpty(email))
            email = root.TryGetProperty("userPrincipalName", out var upnProp) ? upnProp.GetString() : null;

        if (string.IsNullOrEmpty(id))
            throw new InvalidOperationException("Microsoft token missing 'id' field");
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Microsoft token missing 'mail' or 'userPrincipalName' field");

        var firstName = root.TryGetProperty("givenName", out var gn) ? gn.GetString() : null;
        var lastName = root.TryGetProperty("surname", out var sn) ? sn.GetString() : null;

        return (id, email, firstName, lastName);
    }

    // Two-Factor Authentication Methods
    public async Task<TwoFactorSetupResponse> SetupTwoFactorAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var setup = await _totpService.InitializeSetupAsync(userId, user.Email);
        var backupCodes = await _totpService.CompleteSetupAsync(userId, setup.Secret);

        // Don't save yet - user needs to verify the code first
        return new TwoFactorSetupResponse
        {
            QrCodeUrl = setup.QrCodeUrl,
            Secret = setup.Secret,
            BackupCodes = backupCodes.Codes.ToList()
        };
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(int userId, string code)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new InvalidOperationException("User or 2FA not configured");

        return await _totpService.VerifySetupAsync(userId, code, user.TwoFactorSecret);
    }

    public async Task<AuthResponse> VerifyTwoFactorLoginAsync(string tempToken, string code)
    {
        // Retrieve user ID from cache using temp token
        var cacheKey = $"2fa_token_{tempToken}";
        if (!_cache.TryGetValue(cacheKey, out int userId))
            throw new UnauthorizedAccessException("Invalid or expired verification token");

        // Remove token from cache immediately (one-time use)
        _cache.Remove(cacheKey);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        if (string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new InvalidOperationException("2FA not configured for this user");

        // Verify the TOTP code
        var isValid = await _totpService.VerifySetupAsync(userId, code, user.TwoFactorSecret);

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

        user.LastLoginAt = DateTime.UtcNow;

        // Generate response with tokens
        var fullUser = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .Include(u => u.Department)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        var response = GenerateAuthResponse(fullUser ?? user);

        // Store refresh token in dedicated table
        await PersistRefreshTokenAsync(user, response.RefreshToken);

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        return response;
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

        // Send password reset email via notification provider
        try
        {
            var frontendUrl = _configuration.GetValue<string>("FrontendUrl")
                ?? _configuration.GetValue<string>("AllowedOrigins")
                ?? "http://localhost:3000";
            // Take first URL if AllowedOrigins contains multiple comma-separated values
            frontendUrl = frontendUrl.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? "http://localhost:3000";
            var resetUrl = $"{frontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email)}";

            var emailRequest = new EmailNotificationRequest
            {
                To = user.Email,
                ToName = $"{user.FirstName} {user.LastName}".Trim(),
                Subject = "CRM - Password Reset Request",
                IsHtml = true,
                Body = $@"<html><body style='font-family: Arial, sans-serif;'>
<h2>Password Reset Request</h2>
<p>Hi {user.FirstName},</p>
<p>We received a request to reset your password. Click the link below to set a new password:</p>
<p><a href='{resetUrl}' style='display:inline-block;padding:10px 20px;background-color:#1976d2;color:#ffffff;text-decoration:none;border-radius:4px;'>Reset Password</a></p>
<p>Or copy and paste this URL into your browser:</p>
<p style='word-break:break-all;'>{resetUrl}</p>
<p>This link will expire in 24 hours.</p>
<p>If you did not request a password reset, please ignore this email.</p>
<br/>
<p>— CRM System</p>
</body></html>",
                PlainTextBody = $"Hi {user.FirstName},\n\nWe received a request to reset your password. Visit the following link to set a new password:\n\n{resetUrl}\n\nThis link will expire in 24 hours.\n\nIf you did not request a password reset, please ignore this email.\n\n— CRM System"
            };

            var result = await _notificationPort.SendEmailAsync(emailRequest);
            if (result.Success)
            {
            }
            else
            {
                _logger.LogWarning("Password reset email delivery reported failure for {Email}: {Error}", user.Email, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send password reset email to {Email}. Token was still generated and saved — returning token for backward compatibility.", user.Email);
        }

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

    public async Task<AuthResponse> SetupPasswordAsync(SetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PasswordSetupToken))
            throw new ArgumentException("Password setup token is required");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new ArgumentException("New password is required");

        if (request.NewPassword != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match");

        // Validate password against complexity requirements
        await ValidatePasswordComplexityAsync(request.NewPassword);

        // Get user ID from cache token
        var cacheKey = $"password_setup_{request.PasswordSetupToken}";
        if (!_cache.TryGetValue(cacheKey, out int userId))
            throw new UnauthorizedAccessException("Invalid or expired password setup token");

        // Remove token from cache (one-time use)
        _cache.Remove(cacheKey);

        // Get user with navigation properties
        var user = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .Include(u => u.Department)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
            throw new InvalidOperationException("User not found");

        // Update password and reset flags
        user.PasswordHash = HashPassword(request.NewPassword);
        user.PasswordLastChangedAt = DateTime.UtcNow;
        user.PasswordNeverSet = false;
        user.MustResetPassword = false;
        user.LastLoginAt = DateTime.UtcNow;

        // Generate auth response with tokens
        var response = GenerateAuthResponse(user);

        // Store refresh token in dedicated table
        await PersistRefreshTokenAsync(user, response.RefreshToken);

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        _logger.LogInformation($"User {userId} set up password successfully");
        return response;
    }

    public async Task<PasswordComplexityRequirements> GetPasswordRequirementsAsync()
    {
        var settings = await _dbContext.SystemSettings.FirstOrDefaultAsync();

        return new PasswordComplexityRequirements
        {
            MinLength = settings?.MinPasswordLength ?? 8,
            MaxLength = settings?.MaxPasswordLength ?? 128,
            RequireUppercase = settings?.RequireUppercase ?? true,
            RequireLowercase = settings?.RequireLowercase ?? true,
            RequireNumbers = settings?.RequireNumbers ?? true,
            RequireSpecialChars = settings?.RequireSpecialChars ?? false
        };
    }

    private async Task ValidatePasswordComplexityAsync(string password)
    {
        var requirements = await GetPasswordRequirementsAsync();
        var errors = new List<string>();

        if (password.Length < requirements.MinLength)
            errors.Add($"Password must be at least {requirements.MinLength} characters");

        if (requirements.MaxLength > 0 && password.Length > requirements.MaxLength)
            errors.Add($"Password must be no more than {requirements.MaxLength} characters");

        if (requirements.RequireUppercase && !password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter");

        if (requirements.RequireLowercase && !password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter");

        if (requirements.RequireNumbers && !password.Any(char.IsDigit))
            errors.Add("Password must contain at least one number");

        if (requirements.RequireSpecialChars && !password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add("Password must contain at least one special character");

        if (errors.Any())
            throw new ArgumentException(string.Join(". ", errors));
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
