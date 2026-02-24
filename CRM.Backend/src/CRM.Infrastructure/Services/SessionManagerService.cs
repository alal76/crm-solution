// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Enforces concurrent session limits. When a user exceeds the configured
/// maximum number of active sessions the oldest sessions are revoked automatically.
/// Default maximum: 5 sessions per user (configurable via "Auth:MaxConcurrentSessions").
/// </summary>
public class SessionManagerService : ISessionManager
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<SessionManagerService> _logger;
    private readonly int _maxSessions;

    public SessionManagerService(
        ICrmDbContext dbContext,
        IConfiguration configuration,
        ILogger<SessionManagerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _maxSessions = configuration.GetValue<int>("Auth:MaxConcurrentSessions", 5);
    }

    /// <inheritdoc />
    public async Task<UserSession> CreateSessionAsync(
        int userId,
        string sessionToken,
        string ipAddress,
        string userAgent,
        DateTime expiresAt,
        string? deviceId = null,
        CancellationToken cancellationToken = default)
    {
        var session = new UserSession
        {
            UserId = userId,
            SessionToken = sessionToken,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            DeviceId = deviceId
        };

        _dbContext.UserSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Session created for user {UserId} from {IpAddress}", userId, ipAddress);
        return session;
    }

    /// <inheritdoc />
    public async Task<UserSession?> ValidateSessionAsync(
        string sessionToken,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserSessions
            .FirstOrDefaultAsync(
                s => s.SessionToken == sessionToken
                     && !s.IsRevoked
                     && s.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task RevokeSessionAsync(
        string sessionToken,
        CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken, cancellationToken);

        if (session == null) return;

        session.IsRevoked = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Session {Token} revoked for user {UserId}",
            sessionToken[..Math.Min(8, sessionToken.Length)], session.UserId);
    }

    /// <inheritdoc />
    public async Task RevokeAllSessionsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _dbContext.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
            session.IsRevoked = true;

        if (sessions.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Revoked {Count} sessions for user {UserId}", sessions.Count, userId);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task EnforceSessionLimitAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var activeSessions = await _dbContext.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .OrderBy(s => s.LastActivityAt)   // oldest first
            .ToListAsync(cancellationToken);

        if (activeSessions.Count < _maxSessions) return;

        int excessCount = activeSessions.Count - _maxSessions + 1; // +1 to make room for the new session
        var toRevoke = activeSessions.Take(excessCount).ToList();

        foreach (var session in toRevoke)
            session.IsRevoked = true;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Session limit ({Max}) enforced for user {UserId}: revoked {Count} oldest session(s)",
            _maxSessions, userId, toRevoke.Count);
    }
}
