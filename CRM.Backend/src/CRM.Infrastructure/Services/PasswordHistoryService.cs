// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using BCrypt.Net;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Tracks password history and prevents reuse of the last N passwords.
/// Uses BCrypt.Verify to compare plain-text passwords against stored hashes.
/// </summary>
public class PasswordHistoryService : IPasswordHistoryService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<PasswordHistoryService> _logger;

    public PasswordHistoryService(
        ICrmDbContext dbContext,
        ILogger<PasswordHistoryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task AddPasswordAsync(
        int userId,
        string passwordHash,
        CancellationToken cancellationToken = default)
    {
        var entry = new PasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.PasswordHistories.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Password history entry added for user {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task RecordNewPasswordAsync(
        int userId,
        string plainTextPassword,
        CancellationToken cancellationToken = default)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
        await AddPasswordAsync(userId, hash, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsPasswordReusedAsync(
        int userId,
        string plainTextPassword,
        int historyDepth = 5,
        CancellationToken cancellationToken = default)
    {
        var recentHashes = await _dbContext.PasswordHistories
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .Take(historyDepth)
            .Select(ph => ph.PasswordHash)
            .ToListAsync(cancellationToken);

        foreach (var hash in recentHashes)
        {
            if (BCrypt.Net.BCrypt.Verify(plainTextPassword, hash))
            {
                _logger.LogWarning("Password reuse detected for user {UserId}", userId);
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PasswordHistory>> GetPasswordHistoryAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PasswordHistories
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
