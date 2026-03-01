// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Customer Portal authentication service.
/// Issues portal-scoped JWT tokens (claim: portal_user_id).
/// </summary>
public class PortalAuthService : IPortalAuthService
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<PortalAuthService> _logger;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpirationMinutes;

    public PortalAuthService(
        ICrmDbContext db,
        ILogger<PortalAuthService> logger,
        IConfiguration configuration)
    {
        _db = db;
        _logger = logger;
        _jwtSecret = configuration["Jwt:Secret"]
            ?? "development-only-jwt-secret-key-minimum-32-chars";
        _jwtIssuer = configuration["Jwt:Issuer"] ?? "CRMApp";
        _jwtAudience = configuration["Jwt:Audience"] ?? "CRMUsers";
        _jwtExpirationMinutes = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var mins) ? mins : 480;
    }

    // ─────────────────────────────────────────────────────────────────────────
    public async Task<PortalTokenResponseDto?> LoginAsync(PortalLoginDto dto, CancellationToken ct = default)
    {
        var user = await _db.PortalUsers
            .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted, ct);

        if (user == null)
        {
            _logger.LogWarning("Portal login failed: user not found for {Email}", dto.Email);
            return null;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Portal login denied: account inactive for {Email}", dto.Email);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Portal login failed: wrong password for {Email}", dto.Email);
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);
        var token = GeneratePortalToken(user, expiresAt);

        return new PortalTokenResponseDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            PortalUserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    public async Task<PortalUserDto> RegisterAsync(PortalRegisterDto dto, CancellationToken ct = default)
    {
        // Check portal is enabled and allows self-registration
        var config = await _db.PortalConfigs
            .FirstOrDefaultAsync(c => !c.IsDeleted, ct);

        if (config != null && !config.IsEnabled)
            throw new InvalidOperationException("Customer portal is currently disabled.");

        if (config != null && !config.AllowSelfRegistration)
            throw new InvalidOperationException("Self-registration is not allowed. Please contact support.");

        // Check email not already registered
        var existing = await _db.PortalUsers
            .AnyAsync(u => u.Email == dto.Email && !u.IsDeleted, ct);
        if (existing)
            throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");

        var now = DateTime.UtcNow;
        var portalUser = new PortalUser
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DisplayName = dto.DisplayName,
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationToken = Guid.NewGuid().ToString("N"),
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.PortalUsers.Add(portalUser);
        await _db.SaveChangesAsync(ct);

        // PORTAL-020: Email verification notification (best-effort)
        // TODO: Inject INotificationPort or email service and send verification email:
        // try
        // {
        //     var verificationUrl = $"/portal/verify-email?token={portalUser.EmailVerificationToken}";
        //     await _emailService.SendAsync(new EmailMessage { To = portalUser.Email,
        //         Subject = "Verify your portal email",
        //         Body = $"Please verify your email by clicking: {verificationUrl}" });
        // }
        // catch (Exception ex) { _logger.LogWarning(ex, "Failed to send verification email to {Email}", portalUser.Email); }

        _logger.LogInformation("New portal user registered: {Email} (Id={Id})", portalUser.Email, portalUser.Id);

        return MapToDto(portalUser);
    }

    // ─────────────────────────────────────────────────────────────────────────
    public async Task<bool> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var user = await _db.PortalUsers
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

        if (user == null)
            return false; // Do not reveal user existence

        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(2);
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Password reset token generated for portal user {Email}", email);
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    public async Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        var user = await _db.PortalUsers
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token && !u.IsDeleted, ct);

        if (user == null)
            return false;

        if (user.PasswordResetExpiry == null || user.PasswordResetExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset token expired for portal user Id={Id}", user.Id);
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    public async Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        var user = await _db.PortalUsers
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token && !u.IsDeleted, ct);

        if (user == null)
            return false;

        user.IsEmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        user.EmailVerificationToken = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    private string GeneratePortalToken(PortalUser user, DateTime expiresAt)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);

        var claims = new List<Claim>
        {
            new Claim("portal_user_id", user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("portal", "true")
        };

        if (!string.IsNullOrEmpty(user.DisplayName))
            claims.Add(new Claim(ClaimTypes.Name, user.DisplayName));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static PortalUserDto MapToDto(PortalUser u) => new PortalUserDto
    {
        Id = u.Id,
        Email = u.Email,
        DisplayName = u.DisplayName,
        ContactId = u.ContactId,
        AccountId = u.AccountId,
        IsActive = u.IsActive,
        LastLoginAt = u.LastLoginAt,
        CreatedAt = u.CreatedAt
    };
}
