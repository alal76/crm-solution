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
/// Records authentication and security events to the AuthAuditLogs table.
/// All write operations swallow non-critical exceptions to ensure audit failures
/// do not interrupt the main authentication flow.
/// </summary>
public class AuthAuditService : IAuthAuditService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<AuthAuditService> _logger;

    public AuthAuditService(
        ICrmDbContext dbContext,
        ILogger<AuthAuditService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task LogLoginAttemptAsync(
        int? userId,
        string ipAddress,
        string userAgent,
        bool success,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            userId,
            success ? "Login" : "LoginFailed",
            ipAddress,
            userAgent,
            success,
            failureReason,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task LogLogoutAsync(
        int userId,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(userId, "Logout", ipAddress, userAgent, true, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task LogPasswordChangeAsync(
        int userId,
        string ipAddress,
        string userAgent,
        bool success,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(userId, "PasswordChange", ipAddress, userAgent, success, failureReason, cancellationToken);
    }

    /// <inheritdoc />
    public async Task LogTwoFactorEventAsync(
        int userId,
        string action,
        string ipAddress,
        string userAgent,
        bool success,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(userId, action, ipAddress, userAgent, success, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<AuthAuditLog> Items, int TotalCount)> GetUserAuditLogsAsync(
        int? userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuthAuditLogs.AsNoTracking();

        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private async Task LogAsync(
        int? userId,
        string action,
        string ipAddress,
        string userAgent,
        bool success,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        try
        {
            var entry = new AuthAuditLog
            {
                UserId = userId,
                Action = action,
                IpAddress = ipAddress ?? string.Empty,
                UserAgent = userAgent ?? string.Empty,
                Success = success,
                FailureReason = failureReason,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.AuthAuditLogs.Add(entry);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Audit logging must never break the auth flow
            _logger.LogError(ex, "Failed to write auth audit log: action={Action}, userId={UserId}", action, userId);
        }
    }
}
