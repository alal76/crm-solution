// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Authentication;

/// <summary>
/// Authentication handler that validates API keys sent via the X-Api-Key header.
/// Works alongside JWT Bearer authentication — requests with a valid API key are
/// authenticated as the corresponding API user with their role claims.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    private readonly ICrmDbContext _dbContext;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ICrmDbContext dbContext)
        : base(options, logger, encoder)
    {
        _dbContext = dbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var apiKeyHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        // Hash the provided key
        var keyHash = HashApiKey(providedApiKey);

        // Find the API user by key hash
        var user = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .FirstOrDefaultAsync(u =>
                !u.IsDeleted &&
                u.IsApiUser &&
                u.ApiKeyHash == keyHash);

        if (user == null)
        {
            Logger.LogWarning("API key authentication failed — no matching user found");
            return AuthenticateResult.Fail("Invalid API key");
        }

        if (!user.IsActive)
        {
            Logger.LogWarning("API key authentication failed — user {UserId} is inactive", user.Id);
            return AuthenticateResult.Fail("API user is inactive");
        }

        // Check expiration
        if (user.ApiKeyExpiresAt.HasValue && user.ApiKeyExpiresAt.Value < DateTime.UtcNow)
        {
            Logger.LogWarning("API key authentication failed — key expired for user {UserId}", user.Id);
            return AuthenticateResult.Fail("API key has expired");
        }

        // Update last used timestamp (fire and forget to avoid slowing auth)
        try
        {
            user.ApiKeyLastUsedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to update ApiKeyLastUsedAt for user {UserId}", user.Id);
        }

        // Build claims identity matching JWT claims structure
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, Enum.GetName(typeof(UserRole), user.Role) ?? "Guest"),
            new Claim("auth_method", "api_key"),
            new Claim("is_api_user", "true"),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        Logger.LogInformation("API key authenticated user {UserId} ({Username})", user.Id, user.Username);
        return AuthenticateResult.Success(ticket);
    }

    /// <summary>
    /// Hash an API key using SHA-256.
    /// </summary>
    public static string HashApiKey(string apiKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Generate a new API key with the "crm_" prefix.
    /// </summary>
    public static (string rawKey, string hash, string prefix) GenerateApiKey()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var rawKey = "crm_" + Convert.ToBase64String(randomBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        var hash = HashApiKey(rawKey);
        var prefix = rawKey[..12]; // "crm_" + first 8 random chars

        return (rawKey, hash, prefix);
    }
}

/// <summary>
/// Options for API key authentication (empty for now, extensible).
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
}
