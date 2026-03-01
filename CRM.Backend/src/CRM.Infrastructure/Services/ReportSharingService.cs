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
/// Service for managing report sharing functionality.
/// Allows users to share reports with specific users or groups.
/// TODO-RPT-03
/// </summary>
public class ReportSharingService : IReportSharingService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ReportSharingService> _logger;

    public ReportSharingService(
        ICrmDbContext context,
        ILogger<ReportSharingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ReportShareResult> ShareReportAsync(
        int reportId,
        IEnumerable<int> userIds,
        ReportSharePermission permission,
        int sharedByUserId,
        CancellationToken cancellationToken = default)
    {
        var result = new ReportShareResult
        {
            ReportId = reportId,
            SharedAt = DateTime.UtcNow
        };

        try
        {
            var report = await _context.ReportDefinitions
                .FirstOrDefaultAsync(r => r.Id == reportId && !r.IsDeleted, cancellationToken);

            if (report == null)
            {
                result.Success = false;
                result.ErrorMessage = $"Report {reportId} not found";
                return result;
            }

            var usersToShare = userIds.ToList();
            var existingShares = await _context.ReportShares
                .Where(rs => rs.ReportId == reportId && usersToShare.Contains(rs.UserId) && !rs.IsDeleted)
                .ToListAsync(cancellationToken);

            var newShareCount = 0;
            var updatedShareCount = 0;

            foreach (var userId in usersToShare)
            {
                var existingShare = existingShares.FirstOrDefault(s => s.UserId == userId);
                
                if (existingShare != null)
                {
                    // Update existing share
                    existingShare.Permission = permission.ToString();
                    updatedShareCount++;
                }
                else
                {
                    // Create new share
                    var share = new ReportShare
                    {
                        ReportId = reportId,
                        UserId = userId,
                        Permission = permission.ToString(),
                        SharedByUserId = sharedByUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ReportShares.Add(share);
                    newShareCount++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            result.Success = true;
            result.NewSharesCreated = newShareCount;
            result.SharesUpdated = updatedShareCount;

            _logger.LogInformation(
                "Report {ReportId} shared with {Count} users by user {UserId}. New: {New}, Updated: {Updated}",
                reportId, usersToShare.Count, sharedByUserId, newShareCount, updatedShareCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing report {ReportId}", reportId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SharedReportDto>> GetSharedReportsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var shares = await _context.ReportShares
            .Include(rs => rs.Report)
            .Include(rs => rs.SharedByUser)
            .Where(rs => rs.UserId == userId && !rs.IsDeleted && rs.Report != null && !rs.Report.IsDeleted)
            .OrderByDescending(rs => rs.CreatedAt)
            .ToListAsync(cancellationToken);

        return shares.Select(s => new SharedReportDto
        {
            ShareId = s.Id,
            ReportId = s.ReportId,
            ReportName = s.Report?.Name ?? "Unknown",
            ReportCategory = s.Report?.Category,
            Permission = s.Permission,
            SharedByUserId = s.SharedByUserId,
            SharedByUserName = s.SharedByUser?.Email,
            SharedAt = s.CreatedAt
        });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReportShareInfo>> GetReportShareInfoAsync(
        int reportId,
        CancellationToken cancellationToken = default)
    {
        var shares = await _context.ReportShares
            .Include(rs => rs.User)
            .Include(rs => rs.SharedByUser)
            .Where(rs => rs.ReportId == reportId && !rs.IsDeleted)
            .OrderBy(rs => rs.User != null ? rs.User.Email : string.Empty)
            .ToListAsync(cancellationToken);

        return shares.Select(s => new ReportShareInfo
        {
            ShareId = s.Id,
            UserId = s.UserId,
            UserEmail = s.User?.Email,
            UserName = s.User != null ? $"{s.User.FirstName} {s.User.LastName}".Trim() : null,
            Permission = s.Permission,
            SharedByUserId = s.SharedByUserId,
            SharedByUserName = s.SharedByUser?.Email,
            SharedAt = s.CreatedAt
        });
    }

    /// <inheritdoc />
    public async Task<bool> RevokeShareAsync(
        int reportId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var share = await _context.ReportShares
            .FirstOrDefaultAsync(s => s.ReportId == reportId && s.UserId == userId && !s.IsDeleted, cancellationToken);

        if (share == null)
        {
            return false;
        }

        share.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Revoked report share: Report {ReportId}, User {UserId}", reportId, userId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> HasAccessAsync(
        int reportId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        // Check if user is owner
        var report = await _context.ReportDefinitions
            .FirstOrDefaultAsync(r => r.Id == reportId && !r.IsDeleted, cancellationToken);

        if (report == null)
        {
            return false;
        }

        if (report.CreatedByUserId == userId)
        {
            return true;
        }

        // Check if report is shared with user
        return await _context.ReportShares
            .AnyAsync(s => s.ReportId == reportId && s.UserId == userId && !s.IsDeleted, cancellationToken);
    }
}

/// <summary>
/// Permission levels for shared reports.
/// </summary>
public enum ReportSharePermission
{
    View,
    Edit,
    Admin
}

/// <summary>
/// Result of a report share operation.
/// </summary>
public class ReportShareResult
{
    public int ReportId { get; set; }
    public DateTime SharedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int NewSharesCreated { get; set; }
    public int SharesUpdated { get; set; }
}

/// <summary>
/// DTO for reports shared with a user.
/// </summary>
public class SharedReportDto
{
    public int ShareId { get; set; }
    public int ReportId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string? ReportCategory { get; set; }
    public string Permission { get; set; } = string.Empty;
    public int SharedByUserId { get; set; }
    public string? SharedByUserName { get; set; }
    public DateTime SharedAt { get; set; }
}

/// <summary>
/// Information about a specific report share.
/// </summary>
public class ReportShareInfo
{
    public int ShareId { get; set; }
    public int UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string Permission { get; set; } = string.Empty;
    public int SharedByUserId { get; set; }
    public string? SharedByUserName { get; set; }
    public DateTime SharedAt { get; set; }
}

/// <summary>
/// Interface for report sharing service.
/// </summary>
public interface IReportSharingService
{
    /// <summary>
    /// Shares a report with specified users.
    /// </summary>
    Task<ReportShareResult> ShareReportAsync(int reportId, IEnumerable<int> userIds, ReportSharePermission permission, int sharedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reports shared with a user.
    /// </summary>
    Task<IEnumerable<SharedReportDto>> GetSharedReportsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets share information for a specific report.
    /// </summary>
    Task<IEnumerable<ReportShareInfo>> GetReportShareInfoAsync(int reportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a user's access to a report.
    /// </summary>
    Task<bool> RevokeShareAsync(int reportId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has access to a report.
    /// </summary>
    Task<bool> HasAccessAsync(int reportId, int userId, CancellationToken cancellationToken = default);
}
