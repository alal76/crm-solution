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
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// GDPR compliance service — implements Article 15 (access), Article 17 (erasure).
/// TODO-SYS006-004
/// </summary>
public class GdprService : IGdprService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<GdprService> _logger;

    public GdprService(ICrmDbContext context, ILogger<GdprService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task LogAccessAsync(
        int userId,
        string subjectType,
        int subjectId,
        string action,
        string ipAddress,
        string? notes = null,
        CancellationToken ct = default)
    {
        try
        {
            var log = new GdprAccessLog
            {
                RequestedByUserId = userId,
                SubjectType = subjectType.ToLowerInvariant(),
                SubjectId = subjectId,
                Action = action.ToLowerInvariant(),
                IpAddress = ipAddress,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.GdprAccessLogs.Add(log);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "GDPR: User {UserId} performed '{Action}' on {SubjectType}/{SubjectId} from {IpAddress}",
                userId, action, subjectType, subjectId, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging GDPR access event");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PersonalDataExport> ExportPersonalDataAsync(
        string subjectType,
        int subjectId,
        CancellationToken ct = default)
    {
        var export = new PersonalDataExport
        {
            SubjectType = subjectType,
            SubjectId = subjectId,
            ExportedAt = DateTime.UtcNow,
            Data = new Dictionary<string, Dictionary<string, object?>>()
        };

        var type = subjectType.ToLowerInvariant();
        try
        {
            switch (type)
            {
                case "contact":
                {
                    var contact = await _context.Contacts
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(c => c.Id == subjectId, ct);

                    if (contact != null)
                    {
                        export.Data["contact"] = new Dictionary<string, object?>
                        {
                            ["id"] = contact.Id,
                            ["firstName"] = contact.FirstName,
                            ["lastName"] = contact.LastName,
                            ["email"] = contact.Email,
                            ["phone"] = contact.PhonePrimary
                        };
                    }
                    break;
                }
                case "lead":
                {
                    var lead = await _context.Leads
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(l => l.Id == subjectId, ct);

                    if (lead != null)
                    {
                        export.Data["lead"] = new Dictionary<string, object?>
                        {
                            ["id"] = lead.Id,
                            ["firstName"] = lead.FirstName,
                            ["lastName"] = lead.LastName,
                            ["email"] = lead.Email,
                            ["phone"] = lead.Phone,
                            ["createdAt"] = lead.CreatedAt,
                            ["isDeleted"] = lead.IsDeleted
                        };
                    }
                    break;
                }
                case "account":
                {
                    var account = await _context.Accounts
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(a => a.Id == subjectId, ct);

                    if (account != null)
                    {
                        export.Data["account"] = new Dictionary<string, object?>
                        {
                            ["id"] = account.Id,
                            ["firstName"] = account.FirstName,
                            ["lastName"] = account.LastName,
                            ["email"] = account.Email,
                            ["phone"] = account.Phone,
                            ["company"] = account.Company,
                            ["createdAt"] = account.CreatedAt,
                            ["isDeleted"] = account.IsDeleted
                        };
                    }
                    break;
                }
                default:
                    _logger.LogWarning("GDPR export: unknown subject type '{SubjectType}'", subjectType);
                    break;
            }

            // Include GDPR access log history for this subject
            var logs = await _context.GdprAccessLogs
                .IgnoreQueryFilters()
                .Where(l => l.SubjectType == type && l.SubjectId == subjectId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync(ct);

            export.Data["accessHistory"] = logs.Select((l, i) => new
            {
                i,
                l.RequestedByUserId,
                l.Action,
                l.IpAddress,
                l.CreatedAt,
                l.Notes
            }).ToDictionary(x => x.i.ToString(), x => (object?)x);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting personal data for {SubjectType}/{SubjectId}", subjectType, subjectId);
            throw;
        }

        return export;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GdprAccessLogDto>> GetAccessLogsAsync(
        string subjectType,
        int subjectId,
        CancellationToken ct = default)
    {
        try
        {
            var logs = await _context.GdprAccessLogs
                .IgnoreQueryFilters()
                .Where(l => l.SubjectType == subjectType.ToLowerInvariant() && l.SubjectId == subjectId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync(ct);

            return logs.Select(l => new GdprAccessLogDto
            {
                Id = l.Id,
                RequestedByUserId = l.RequestedByUserId,
                SubjectType = l.SubjectType,
                SubjectId = l.SubjectId,
                Action = l.Action,
                IpAddress = l.IpAddress,
                Notes = l.Notes,
                CreatedAt = l.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GDPR access logs for {SubjectType}/{SubjectId}", subjectType, subjectId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ErasePersonalDataAsync(
        string subjectType,
        int subjectId,
        int requestingUserId,
        string ipAddress,
        CancellationToken ct = default)
    {
        var type = subjectType.ToLowerInvariant();
        try
        {
            bool erased = false;

            switch (type)
            {
                case "contact":
                {
                    var contact = await _context.Contacts
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(c => c.Id == subjectId, ct);

                    if (contact != null)
                    {
                        contact.FirstName = "ERASED";
                        contact.LastName = "ERASED";
                        contact.Email = $"erased-{contact.Id}@gdpr.invalid";
                        contact.PhonePrimary = null;
                        contact.PhoneSecondary = null;
                        contact.PhoneMobile = null;
                        _context.Contacts.Update(contact);
                        erased = true;
                    }
                    break;
                }
                case "lead":
                {
                    var lead = await _context.Leads
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(l => l.Id == subjectId, ct);

                    if (lead != null)
                    {
                        lead.FirstName = "ERASED";
                        lead.LastName = "ERASED";
                        lead.Email = $"erased-{lead.Id}@gdpr.invalid";
                        lead.Phone = null;
                        lead.IsDeleted = true;
                        _context.Leads.Update(lead);
                        erased = true;
                    }
                    break;
                }
                case "account":
                {
                    var account = await _context.Accounts
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(a => a.Id == subjectId, ct);

                    if (account != null)
                    {
                        account.FirstName = "ERASED";
                        account.LastName = "ERASED";
                        account.Email = $"erased-{account.Id}@gdpr.invalid";
                        account.Phone = string.Empty;
                        account.IsDeleted = true;
                        _context.Accounts.Update(account);
                        erased = true;
                    }
                    break;
                }
                default:
                    _logger.LogWarning("GDPR erase: unknown subject type '{SubjectType}'", subjectType);
                    return false;
            }

            if (erased)
            {
                await _context.SaveChangesAsync(ct);

                // Audit the erasure
                await LogAccessAsync(requestingUserId, type, subjectId, "anonymize", ipAddress,
                    "GDPR Article 17 erasure request", ct);

                _logger.LogInformation(
                    "GDPR: Erased personal data for {SubjectType}/{SubjectId} by user {UserId}",
                    subjectType, subjectId, requestingUserId);
            }

            return erased;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error erasing personal data for {SubjectType}/{SubjectId}", subjectType, subjectId);
            throw;
        }
    }
}
