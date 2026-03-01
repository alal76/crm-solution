// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Manages email unsubscribe records, preference-centre operations, and
/// HMAC-signed unsubscribe tokens for CAN-SPAM / GDPR compliance.
/// </summary>
public class UnsubscribeService : IUnsubscribeService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<UnsubscribeService> _logger;
    private readonly string _hmacSecret;

    /// <summary>Initializes a new instance of UnsubscribeService.</summary>
    public UnsubscribeService(ICrmDbContext context, IConfiguration configuration, ILogger<UnsubscribeService> logger)
    {
        _context = context;
        _logger = logger;
        _hmacSecret = configuration["Jwt:Secret"] ?? "CrmUnsubscribeTokenDefaultKey32!";
    }

    /// <inheritdoc />
    public async Task<UnsubscribeStatusDto> GetStatusAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new UnsubscribeStatusDto { Email = string.Empty, IsUnsubscribed = false, ReceiveTransactional = true };
        }

        var normalised = email.Trim().ToLowerInvariant();
        var record = await _context.UnsubscribeRecords
            .Where(r => r.Email == normalised && !r.IsDeleted)
            .OrderByDescending(r => r.UnsubscribedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (record == null)
        {
            return new UnsubscribeStatusDto { Email = normalised, IsUnsubscribed = false, ReceiveProductUpdates = false, ReceiveTransactional = true };
        }

        return new UnsubscribeStatusDto
        {
            Email = record.Email,
            IsUnsubscribed = true,
            ReceiveProductUpdates = record.ReceiveProductUpdates,
            ReceiveTransactional = record.ReceiveTransactional,
            UnsubscribedAt = record.UnsubscribedAt
        };
    }

    /// <inheritdoc />
    public async Task<UnsubscribeStatusDto> UnsubscribeAsync(UnsubscribeRequestDto dto, CancellationToken cancellationToken = default)
    {
        var normalised = dto.Email.Trim().ToLowerInvariant();

        // Upsert: update existing record or create new one
        var existing = await _context.UnsubscribeRecords
            .Where(r => r.Email == normalised && !r.IsDeleted)
            .OrderByDescending(r => r.UnsubscribedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
        {
            existing.Reason = dto.Reason;
            existing.ReasonNote = dto.ReasonNote;
            existing.ReceiveProductUpdates = dto.ReceiveProductUpdates;
            existing.UnsubscribedAt = DateTime.UtcNow;
        }
        else
        {
            var record = new UnsubscribeRecord
            {
                Email = normalised,
                Reason = dto.Reason,
                ReasonNote = dto.ReasonNote,
                ReceiveProductUpdates = dto.ReceiveProductUpdates,
                ReceiveTransactional = true,
                UnsubscribedAt = DateTime.UtcNow,
                Token = dto.Token,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.UnsubscribeRecords.Add(record);
        }

        // Mark all active nurture enrollments as opted out
        var activeEnrollments = await _context.NurtureEnrollments
            .Where(e => e.EnrolleeEmail == normalised && !e.IsUnsubscribed && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var enrollment in activeEnrollments)
        {
            enrollment.IsUnsubscribed = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email {Email} unsubscribed. {EnrollmentCount} nurture enrollments opted out.", normalised, activeEnrollments.Count);

        return await GetStatusAsync(normalised, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UnsubscribeStatusDto> UpdatePreferencesAsync(string email, UnsubscribeRequestDto dto, CancellationToken cancellationToken = default)
    {
        var normalised = email.Trim().ToLowerInvariant();

        var record = await _context.UnsubscribeRecords
            .Where(r => r.Email == normalised && !r.IsDeleted)
            .OrderByDescending(r => r.UnsubscribedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (record != null)
        {
            record.ReceiveProductUpdates = dto.ReceiveProductUpdates;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return await GetStatusAsync(normalised, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GenerateUnsubscribeTokenAsync(string email, int? campaignId, CancellationToken cancellationToken = default)
    {
        var payload = $"{email.Trim().ToLowerInvariant()}|{campaignId?.ToString() ?? "0"}|{DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds()}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_hmacSecret));
        var sig = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{payload}|{sig}"))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        return Task.FromResult(token);
    }

    /// <inheritdoc />
    public async Task<bool> IsUnsubscribedAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalised = email.Trim().ToLowerInvariant();
        return await _context.UnsubscribeRecords
            .AnyAsync(r => r.Email == normalised && !r.IsDeleted, cancellationToken);
    }
}
