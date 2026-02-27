// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.IO.Compression;
using System.Text.Json;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for comprehensive GDPR data export functionality.
/// Collects all user data across entities for GDPR Article 15 compliance.
/// TODO-SYS006-005
/// </summary>
public class GdprDataExportService : IGdprDataExportService
{
    private readonly ICrmDbContext _context;
    private readonly IGdprService _gdprService;
    private readonly ILogger<GdprDataExportService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GdprDataExportService(
        ICrmDbContext context,
        IGdprService gdprService,
        ILogger<GdprDataExportService> logger)
    {
        _context = context;
        _gdprService = gdprService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<GdprExportResult> ExportUserDataAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting GDPR data export for user {UserId}", userId);

        var export = new GdprExportResult
        {
            UserId = userId,
            ExportedAt = DateTime.UtcNow,
            Data = new Dictionary<string, object>()
        };

        try
        {
            // 1. User profile data
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for GDPR export", userId);
                export.Success = false;
                export.ErrorMessage = $"User {userId} not found";
                return export;
            }

            export.Data["profile"] = new Dictionary<string, object?>
            {
                ["id"] = user.Id,
                ["username"] = user.Username,
                ["email"] = user.Email,
                ["firstName"] = user.FirstName,
                ["lastName"] = user.LastName,
                ["isActive"] = user.IsActive,
                ["createdAt"] = user.CreatedAt,
                ["lastLoginAt"] = user.LastLoginAt
            };

            // 2. Contacts owned by user
            var contacts = await _context.Contacts
                .AsNoTracking()
                .Where(c => c.OwnerId == userId)
                .Select(c => new
                {
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    Email = c.EmailPrimary,
                    c.PhonePrimary,
                    c.JobTitle
                })
                .ToListAsync(cancellationToken);

            export.Data["contacts"] = contacts;

            // 3. Leads owned by user
            var leads = await _context.Leads
                .AsNoTracking()
                .Where(l => l.OwnerId == userId && !l.IsDeleted)
                .Select(l => new
                {
                    l.Id,
                    l.FirstName,
                    l.LastName,
                    l.Email,
                    l.Phone,
                    Company = l.CompanyName,
                    l.Status,
                    l.CreatedAt
                })
                .ToListAsync(cancellationToken);

            export.Data["leads"] = leads;

            // 4. Opportunities owned by user
            var opportunities = await _context.Opportunities
                .AsNoTracking()
                .Where(o => o.SalesOwnerId == userId && !o.IsDeleted)
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.Amount,
                    o.Stage,
                    o.ExpectedCloseDate,
                    o.CreatedAt
                })
                .ToListAsync(cancellationToken);

            export.Data["opportunities"] = opportunities;

            // 5. Accounts owned by user
            var accounts = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.AssignedToUserId == userId && !a.IsDeleted)
                .Select(a => new
                {
                    a.Id,
                    Name = a.Company,
                    a.Industry,
                    a.Website,
                    a.CreatedAt
                })
                .ToListAsync(cancellationToken);

            export.Data["accounts"] = accounts;

            // 6. Tasks assigned to user
            var tasks = await _context.CrmTasks
                .AsNoTracking()
                .Where(t => t.AssignedToUserId == userId && !t.IsDeleted)
                .Select(t => new
                {
                    t.Id,
                    t.Subject,
                    t.Description,
                    t.DueDate,
                    t.Status,
                    t.CreatedAt
                })
                .ToListAsync(cancellationToken);

            export.Data["tasks"] = tasks;

            // 7. Notes created by user
            var notes = await _context.Notes
                .AsNoTracking()
                .Where(n => n.CreatedByUserId == userId && !n.IsDeleted)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Content,
                    n.EntityType,
                    n.EntityId,
                    n.CreatedAt
                })
                .ToListAsync(cancellationToken);

            export.Data["notes"] = notes;

            // 8. Activities by user
            var activities = await _context.Activities
                .AsNoTracking()
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .Select(a => new
                {
                    a.Id,
                    Type = a.ActivityType,
                    Subject = a.Title,
                    a.Description,
                    a.ActivityDate,
                    a.CreatedAt
                })
                .ToListAsync(cancellationToken);

            export.Data["activities"] = activities;

            // 9. Audit logs for this user
            var auditLogs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(1000) // Limit to most recent 1000 entries
                .Select(a => new
                {
                    a.Id,
                    a.Action,
                    a.EntityType,
                    a.EntityId,
                    a.IpAddress,
                    a.CreatedAt
                })
                .ToListAsync(cancellationToken);

            export.Data["auditLogs"] = auditLogs;

            // 10. User preferences
            var preferences = await _context.UIPreferences
                .AsNoTracking()
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.Theme,
                    p.SidebarPosition,
                    p.FontSize,
                    p.CreatedAt
                })
                .ToListAsync(cancellationToken);

            export.Data["preferences"] = preferences;

            // 11. Group memberships
            var groupMemberships = await _context.UserGroupMembers
                .AsNoTracking()
                .Include(ugm => ugm.UserGroup)
                .Where(ugm => ugm.UserId == userId && !ugm.IsDeleted)
                .Select(ugm => new
                {
                    GroupId = ugm.UserGroupId,
                    GroupName = ugm.UserGroup != null ? ugm.UserGroup.Name : null,
                    JoinedAt = ugm.CreatedAt
                })
                .ToListAsync(cancellationToken);

            export.Data["groupMemberships"] = groupMemberships;

            // 12. Sessions (if tracked)
            var sessions = await _context.UserSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(100)
                .Select(s => new
                {
                    s.Id,
                    s.IpAddress,
                    s.UserAgent,
                    s.CreatedAt,
                    s.ExpiresAt
                })
                .ToListAsync(cancellationToken);

            export.Data["sessions"] = sessions;

            export.Success = true;

            // Log the GDPR access event
            await _gdprService.LogAccessAsync(
                userId,
                "user",
                userId,
                "export",
                "system",
                "Full GDPR data export",
                cancellationToken);

            _logger.LogInformation("GDPR data export completed for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GDPR data export for user {UserId}", userId);
            export.Success = false;
            export.ErrorMessage = ex.Message;
        }

        return export;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportUserDataAsJsonAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var export = await ExportUserDataAsync(userId, cancellationToken);
        return JsonSerializer.SerializeToUtf8Bytes(export, JsonOptions);
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportUserDataAsZipAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var export = await ExportUserDataAsync(userId, cancellationToken);

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Main export file
            var mainEntry = archive.CreateEntry("gdpr_export.json");
            using (var writer = new StreamWriter(mainEntry.Open()))
            {
                await writer.WriteAsync(JsonSerializer.Serialize(export, JsonOptions));
            }

            // Export each section as separate file for easier navigation
            foreach (var section in export.Data)
            {
                var sectionEntry = archive.CreateEntry($"data/{section.Key}.json");
                using var sectionWriter = new StreamWriter(sectionEntry.Open());
                await sectionWriter.WriteAsync(JsonSerializer.Serialize(section.Value, JsonOptions));
            }

            // Add export metadata
            var metaEntry = archive.CreateEntry("export_metadata.json");
            using (var metaWriter = new StreamWriter(metaEntry.Open()))
            {
                var metadata = new
                {
                    export.UserId,
                    export.ExportedAt,
                    export.Success,
                    export.ErrorMessage,
                    SectionCount = export.Data.Count,
                    ExportVersion = "1.0"
                };
                await metaWriter.WriteAsync(JsonSerializer.Serialize(metadata, JsonOptions));
            }
        }

        return memoryStream.ToArray();
    }
}

/// <summary>
/// Result of a GDPR data export operation.
/// </summary>
public class GdprExportResult
{
    public int UserId { get; set; }
    public DateTime ExportedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Interface for GDPR data export service.
/// </summary>
public interface IGdprDataExportService
{
    /// <summary>
    /// Exports all data associated with a user across all CRM entities.
    /// </summary>
    Task<GdprExportResult> ExportUserDataAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports user data as a UTF-8 encoded JSON byte array.
    /// </summary>
    Task<byte[]> ExportUserDataAsJsonAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports user data as a ZIP archive containing JSON files.
    /// </summary>
    Task<byte[]> ExportUserDataAsZipAsync(int userId, CancellationToken cancellationToken = default);
}
