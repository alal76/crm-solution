// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Manages user sessions for concurrent session limit enforcement.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Creates a new session for the specified user.
    /// Returns the created UserSession.
    /// </summary>
    Task<UserSession> CreateSessionAsync(int userId, string sessionToken,
        string ipAddress, string userAgent, DateTime expiresAt,
        string? deviceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a session token is still active and not revoked.
    /// Returns the session if valid, null otherwise.
    /// </summary>
    Task<UserSession?> ValidateSessionAsync(string sessionToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a specific session by token.
    /// </summary>
    Task RevokeSessionAsync(string sessionToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all active sessions for the specified user.
    /// </summary>
    Task RevokeAllSessionsAsync(int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all active (non-revoked, non-expired) sessions for a user.
    /// </summary>
    Task<IEnumerable<UserSession>> GetActiveSessionsAsync(int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enforces the concurrent session limit by revoking the oldest sessions
    /// when the user has reached or exceeded the configured maximum.
    /// </summary>
    Task EnforceSessionLimitAsync(int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a session and enforces IP binding if enabled (TODO-AUTH-015).
    /// If the session has IP binding enabled and the current IP differs from
    /// the original IP, the session is revoked as a security measure.
    /// </summary>
    Task<UserSession?> ValidateSessionWithIpCheckAsync(string sessionToken,
        string currentIpAddress, CancellationToken cancellationToken = default);
}
