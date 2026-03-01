// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Manages OAuth provider links for CRM users.
/// Prevents unlinking when the provider is the user's sole authentication method.
/// </summary>
public class UserOAuthLinkService : IUserOAuthLinkService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<UserOAuthLinkService> _logger;

    public UserOAuthLinkService(
        ICrmDbContext dbContext,
        ILogger<UserOAuthLinkService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserOAuthLink>> GetLinksAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserOAuthLinks
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.Provider)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserOAuthLink> LinkProviderAsync(
        int userId,
        string provider,
        string providerUserId,
        string? providerEmail = null,
        string? accessToken = null,
        CancellationToken cancellationToken = default)
    {
        provider = provider.ToLowerInvariant();

        var existing = await _dbContext.UserOAuthLinks
            .FirstOrDefaultAsync(
                l => l.UserId == userId && l.Provider == provider,
                cancellationToken);

        if (existing != null)
            throw new InvalidOperationException($"Provider '{provider}' is already linked to this account.");

        // Also check that no other user already claims this providerUserId
        var duplicate = await _dbContext.UserOAuthLinks
            .FirstOrDefaultAsync(
                l => l.Provider == provider && l.ProviderUserId == providerUserId,
                cancellationToken);

        if (duplicate != null)
            throw new InvalidOperationException($"This {provider} account is already linked to a different CRM user.");

        var link = new UserOAuthLink
        {
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            ProviderEmail = providerEmail,
            AccessToken = accessToken,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserOAuthLinks.Add(link);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("OAuth provider '{Provider}' linked for user {UserId}", provider, userId);
        return link;
    }

    /// <inheritdoc />
    public async Task UnlinkProviderAsync(
        int userId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        provider = provider.ToLowerInvariant();

        var link = await _dbContext.UserOAuthLinks
            .FirstOrDefaultAsync(
                l => l.UserId == userId && l.Provider == provider,
                cancellationToken);

        if (link == null)
            throw new KeyNotFoundException($"No '{provider}' OAuth link found for this user.");

        // Safety check: must not be the sole authentication method
        var hasLocalPassword = await HasLocalPasswordAsync(userId, cancellationToken);
        var otherLinks = await _dbContext.UserOAuthLinks
            .CountAsync(l => l.UserId == userId && l.Provider != provider, cancellationToken);

        if (!hasLocalPassword && otherLinks == 0)
        {
            throw new InvalidOperationException(
                "Cannot unlink the only remaining authentication method. " +
                "Please set a local password or link another provider first.");
        }

        _dbContext.UserOAuthLinks.Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("OAuth provider '{Provider}' unlinked for user {UserId}", provider, userId);
    }

    /// <inheritdoc />
    public async Task<bool> HasLocalPasswordAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user == null) return false;

        // A user has a local password when PasswordHash is set and PasswordNeverSet is false
        return !string.IsNullOrWhiteSpace(user.PasswordHash) && !user.PasswordNeverSet;
    }
}
