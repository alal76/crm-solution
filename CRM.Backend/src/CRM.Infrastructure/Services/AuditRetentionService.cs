// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for audit log retention management including archiving and purging.
/// Supports compliance requirements for data retention periods.
/// TODO-SYS006-006
/// </summary>
public class AuditRetentionService : IAuditRetentionService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<AuditRetentionService> _logger;

    public AuditRetentionService(
        ICrmDbContext context,
        ILogger<AuditRetentionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuditArchiveResult> ArchiveOldLogsAsync(
        int olderThanDays,
        CancellationToken cancellationToken = default)
    {
        var result = new AuditArchiveResult
        {
            StartedAt = DateTime.UtcNow,
            OlderThanDays = olderThanDays
        };

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            _logger.LogInformation(
                "Starting audit log archival for logs older than {CutoffDate} ({Days} days)",
                cutoffDate, olderThanDays);

            // Find logs to archive (not already archived and not deleted)
            var logsToArchive = await _context.AuditLogs
                .Where(a => a.CreatedAt < cutoffDate && a.ArchivedAt == null && !a.IsDeleted)
                .ToListAsync(cancellationToken);

            result.TotalLogsFound = logsToArchive.Count;

            if (logsToArchive.Count == 0)
            {
                _logger.LogInformation("No audit logs found to archive");
                result.Success = true;
                result.CompletedAt = DateTime.UtcNow;
                return result;
            }

            var archiveTime = DateTime.UtcNow;
            foreach (var log in logsToArchive)
            {
                log.ArchivedAt = archiveTime;
                log.UpdatedAt = archiveTime;
            }

            await _context.SaveChangesAsync(cancellationToken);

            result.LogsArchived = logsToArchive.Count;
            result.Success = true;
            result.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Successfully archived {Count} audit logs older than {CutoffDate}",
                result.LogsArchived, cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving audit logs");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<AuditPurgeResult> PurgeArchivedLogsAsync(
        int olderThanDays,
        CancellationToken cancellationToken = default)
    {
        var result = new AuditPurgeResult
        {
            StartedAt = DateTime.UtcNow,
            OlderThanDays = olderThanDays
        };

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            _logger.LogInformation(
                "Starting purge of archived audit logs older than {CutoffDate} ({Days} days)",
                cutoffDate, olderThanDays);

            // Find archived logs to purge (archived before cutoff date)
            var logsToPurge = await _context.AuditLogs
                .Where(a => a.ArchivedAt != null && a.ArchivedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            result.TotalLogsFound = logsToPurge.Count;

            if (logsToPurge.Count == 0)
            {
                _logger.LogInformation("No archived audit logs found to purge");
                result.Success = true;
                result.CompletedAt = DateTime.UtcNow;
                return result;
            }

            // Hard delete the archived logs (they've already been archived)
            _context.AuditLogs.RemoveRange(logsToPurge);
            await _context.SaveChangesAsync(cancellationToken);

            result.LogsPurged = logsToPurge.Count;
            result.Success = true;
            result.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Successfully purged {Count} archived audit logs",
                result.LogsPurged);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging archived audit logs");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<AuditRetentionStats> GetRetentionStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var stats = new AuditRetentionStats
        {
            CalculatedAt = DateTime.UtcNow
        };

        try
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
            var oneYearAgo = DateTime.UtcNow.AddDays(-365);

            stats.TotalLogs = await _context.AuditLogs
                .CountAsync(cancellationToken);

            stats.ActiveLogs = await _context.AuditLogs
                .Where(a => a.ArchivedAt == null && !a.IsDeleted)
                .CountAsync(cancellationToken);

            stats.ArchivedLogs = await _context.AuditLogs
                .Where(a => a.ArchivedAt != null)
                .CountAsync(cancellationToken);

            stats.LogsLast30Days = await _context.AuditLogs
                .Where(a => a.CreatedAt >= thirtyDaysAgo)
                .CountAsync(cancellationToken);

            stats.LogsOlderThan90Days = await _context.AuditLogs
                .Where(a => a.CreatedAt < ninetyDaysAgo && a.ArchivedAt == null)
                .CountAsync(cancellationToken);

            stats.LogsOlderThan1Year = await _context.AuditLogs
                .Where(a => a.CreatedAt < oneYearAgo)
                .CountAsync(cancellationToken);

            if (stats.TotalLogs > 0)
            {
                var oldestLog = await _context.AuditLogs
                    .OrderBy(a => a.CreatedAt)
                    .Select(a => a.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                stats.OldestLogDate = oldestLog;

                var newestLog = await _context.AuditLogs
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => a.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                stats.NewestLogDate = newestLog;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating audit retention stats");
        }

        return stats;
    }
}

/// <summary>
/// Result of an audit log archive operation.
/// </summary>
public class AuditArchiveResult
{
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int OlderThanDays { get; set; }
    public int TotalLogsFound { get; set; }
    public int LogsArchived { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of an audit log purge operation.
/// </summary>
public class AuditPurgeResult
{
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int OlderThanDays { get; set; }
    public int TotalLogsFound { get; set; }
    public int LogsPurged { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Statistics about audit log retention.
/// </summary>
public class AuditRetentionStats
{
    public DateTime CalculatedAt { get; set; }
    public int TotalLogs { get; set; }
    public int ActiveLogs { get; set; }
    public int ArchivedLogs { get; set; }
    public int LogsLast30Days { get; set; }
    public int LogsOlderThan90Days { get; set; }
    public int LogsOlderThan1Year { get; set; }
    public DateTime? OldestLogDate { get; set; }
    public DateTime? NewestLogDate { get; set; }
}

/// <summary>
/// Interface for audit log retention service.
/// </summary>
public interface IAuditRetentionService
{
    /// <summary>
    /// Archives audit logs older than the specified number of days.
    /// Sets the ArchivedAt timestamp on matching logs.
    /// </summary>
    Task<AuditArchiveResult> ArchiveOldLogsAsync(int olderThanDays, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes archived audit logs that were archived more than the specified days ago.
    /// </summary>
    Task<AuditPurgeResult> PurgeArchivedLogsAsync(int olderThanDays, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about the current audit log retention state.
    /// </summary>
    Task<AuditRetentionStats> GetRetentionStatsAsync(CancellationToken cancellationToken = default);
}
