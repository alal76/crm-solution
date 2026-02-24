// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for recording and querying authentication audit logs.
/// </summary>
public interface IAuthAuditService
{
    /// <summary>
    /// Logs a login attempt (success or failure).
    /// </summary>
    Task LogLoginAttemptAsync(int? userId, string ipAddress, string userAgent,
        bool success, string? failureReason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a logout event.
    /// </summary>
    Task LogLogoutAsync(int userId, string ipAddress, string userAgent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a password change event (ChangePassword or PasswordReset).
    /// </summary>
    Task LogPasswordChangeAsync(int userId, string ipAddress, string userAgent,
        bool success, string? failureReason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a two-factor authentication event (setup, verify, enable, disable).
    /// </summary>
    Task LogTwoFactorEventAsync(int userId, string action, string ipAddress,
        string userAgent, bool success,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of audit log entries for the specified user
    /// (or all users if userId is null — admin use case).
    /// </summary>
    Task<(IEnumerable<AuthAuditLog> Items, int TotalCount)> GetUserAuditLogsAsync(
        int? userId, int page, int pageSize,
        CancellationToken cancellationToken = default);
}
