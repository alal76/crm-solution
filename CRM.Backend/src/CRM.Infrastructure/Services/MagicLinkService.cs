// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Generates, validates, and emails passwordless magic-link tokens.
/// Tokens expire in 15 minutes and are single-use.
/// </summary>
public class MagicLinkService : IMagicLinkService
{
    private readonly ICrmDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly INotificationPort _notificationPort;
    private readonly ILogger<MagicLinkService> _logger;

    private const int TokenExpiryMinutes = 15;

    public MagicLinkService(
        ICrmDbContext dbContext,
        IJwtTokenService jwtTokenService,
        INotificationPort notificationPort,
        ILogger<MagicLinkService> logger)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _notificationPort = notificationPort;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<MagicLinkToken> GenerateMagicLinkAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"No active user found with email '{email}'.");
        }

        // Invalidate any existing unused, non-expired tokens for this email
        var existing = await _dbContext.MagicLinkTokens
            .Where(t => t.Email == email && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var old in existing)
            old.IsUsed = true;

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        // URL-safe transformation
        var urlSafeToken = rawToken.Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var magic = new MagicLinkToken
        {
            UserId = user.Id,
            Token = urlSafeToken,
            Email = email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(TokenExpiryMinutes),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.MagicLinkTokens.Add(magic);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Magic link generated for user {UserId}", user.Id);
        return magic;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> ValidateMagicLinkAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var magic = await _dbContext.MagicLinkTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

        if (magic == null)
        {
            throw new UnauthorizedAccessException("Invalid magic link token.");
        }

        if (magic.IsUsed)
        {
            throw new UnauthorizedAccessException("This magic link has already been used.");
        }

        if (magic.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("This magic link has expired.");
        }

        if (magic.User == null || magic.User.IsDeleted)
        {
            throw new UnauthorizedAccessException("The associated user account is no longer active.");
        }

        // Mark as used (single-use)
        magic.IsUsed = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var user = magic.User;
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _logger.LogInformation("Magic link successfully validated for user {UserId}", user.Id);

        return new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Role = user.Role.ToString(),  // int role mapped to string for AuthResponse
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    /// <inheritdoc />
    public async Task SendMagicLinkEmailAsync(
        string email,
        string magicLink,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailRequest = new EmailNotificationRequest
            {
                To = email,
                ToName = email,
                Subject = "CRM — Your Magic Login Link",
                IsHtml = true,
                Body = $@"<html><body style='font-family: Arial, sans-serif;'>
<h2>Passwordless Login</h2>
<p>Click the link below to sign in to your CRM account. This link expires in {TokenExpiryMinutes} minutes and can only be used once.</p>
<p><a href='{magicLink}' style='display:inline-block;padding:10px 20px;background-color:#1976d2;color:#ffffff;text-decoration:none;border-radius:4px;'>Sign In</a></p>
<p>Or copy and paste this URL into your browser:</p>
<p style='word-break:break-all;'>{magicLink}</p>
<p>If you did not request this link, please ignore this email.</p>
<br/><p>— CRM System</p>
</body></html>",
                PlainTextBody = $"Click the following link to sign in to your CRM account (expires in {TokenExpiryMinutes} minutes, single-use):\n\n{magicLink}\n\nIf you did not request this link, please ignore this email."
            };

            var result = await _notificationPort.SendEmailAsync(emailRequest, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Magic link email delivery failed to {Email}: {Error}", email, result.Error);
            }
        }
        catch (Exception ex)
        {
            // Log but do not rethrow — caller should still return the token for
            // environments where email is not configured
            _logger.LogWarning(ex, "Failed to send magic link email to {Email}", email);
        }
    }
}
