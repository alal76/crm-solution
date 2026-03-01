// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Options for email sync service configuration.
/// </summary>
public class EmailSyncOptions
{
    public const string SectionName = "EmailSync";

    public int DefaultSyncIntervalMinutes { get; set; } = 15;
    public int MaxEmailsPerSync { get; set; } = 200;
    public int SyncLookbackDays { get; set; } = 30;
}

/// <summary>
/// Interface for email sync service.
/// </summary>
public interface IEmailSyncService
{
    Task<IEnumerable<EmailIntegration>> GetUserIntegrationsAsync(int userId);
    Task<EmailIntegration?> GetIntegrationAsync(int userId, EmailProvider provider, string emailAddress);
    Task<EmailIntegration> CreateOrUpdateIntegrationAsync(EmailIntegration integration);
    Task<bool> DisconnectAsync(int integrationId);

    Task<EmailSyncLog> SyncAsync(int integrationId);
    Task<EmailSyncLog> SyncNowAsync(int userId, int integrationId);
    Task SyncAllDueAsync();
}

/// <summary>
/// Email sync service implementation. Provides scaffolding for IMAP and OAuth-based sync.
/// Part of Marketing & Sales gap analysis implementation (G5).
/// </summary>
public class EmailSyncService : IEmailSyncService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<EmailSyncService> _logger;
    private readonly EmailSyncOptions _options;

    public EmailSyncService(CrmDbContext context, ILogger<EmailSyncService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _options = new EmailSyncOptions();
        configuration.GetSection(EmailSyncOptions.SectionName).Bind(_options);
    }

    public async Task<IEnumerable<EmailIntegration>> GetUserIntegrationsAsync(int userId)
    {
        return await _context.EmailIntegrations
            .Where(i => i.UserId == userId && !i.IsDeleted)
            .OrderBy(i => i.EmailAddress)
            .ToListAsync();
    }

    public async Task<EmailIntegration?> GetIntegrationAsync(int userId, EmailProvider provider, string emailAddress)
    {
        return await _context.EmailIntegrations
            .FirstOrDefaultAsync(i => i.UserId == userId && i.Provider == provider && i.EmailAddress == emailAddress && !i.IsDeleted);
    }

    public async Task<EmailIntegration> CreateOrUpdateIntegrationAsync(EmailIntegration integration)
    {
        EmailIntegration? existing = null;

        if (integration.Id > 0)
        {
            existing = await _context.EmailIntegrations
                .FirstOrDefaultAsync(i => i.Id == integration.Id && i.UserId == integration.UserId);
        }

        existing ??= await _context.EmailIntegrations
            .FirstOrDefaultAsync(i => i.UserId == integration.UserId && i.Provider == integration.Provider && i.EmailAddress == integration.EmailAddress);

        if (existing == null)
        {
            integration.CreatedAt = DateTime.UtcNow;
            integration.NextSyncAt = DateTime.UtcNow.AddMinutes(integration.SyncIntervalMinutes > 0 ? integration.SyncIntervalMinutes : _options.DefaultSyncIntervalMinutes);
            _context.EmailIntegrations.Add(integration);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(integration.EmailAddress))
            {
                existing.EmailAddress = integration.EmailAddress;
            }

            if (integration.Provider != existing.Provider)
            {
                existing.Provider = integration.Provider;
            }

            existing.AccessToken = integration.AccessToken ?? existing.AccessToken;
            existing.RefreshToken = integration.RefreshToken ?? existing.RefreshToken;
            existing.TokenExpiresAt = integration.TokenExpiresAt ?? existing.TokenExpiresAt;
            existing.ImapServer = integration.ImapServer ?? existing.ImapServer;
            existing.ImapPort = integration.ImapPort ?? existing.ImapPort;
            existing.ImapUsername = integration.ImapUsername ?? existing.ImapUsername;
            existing.ImapPassword = integration.ImapPassword ?? existing.ImapPassword;
            existing.UseSsl = integration.UseSsl;
            existing.SettingsJson = integration.SettingsJson ?? existing.SettingsJson;
            existing.SyncIntervalMinutes = integration.SyncIntervalMinutes > 0 ? integration.SyncIntervalMinutes : existing.SyncIntervalMinutes;
            existing.IsActive = integration.IsActive;
        }

        await _context.SaveChangesAsync();
        return existing ?? integration;
    }

    public async Task<bool> DisconnectAsync(int integrationId)
    {
        var integration = await _context.EmailIntegrations.FindAsync(integrationId);
        if (integration == null)
        {
            return false;
        }

        integration.IsDeleted = true;
        integration.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EmailSyncLog> SyncAsync(int integrationId)
    {
        var integration = await _context.EmailIntegrations
            .FirstOrDefaultAsync(i => i.Id == integrationId && i.IsActive && !i.IsDeleted)
            ?? throw new KeyNotFoundException($"Integration {integrationId} not found or inactive");

        var log = new EmailSyncLog
        {
            EmailIntegrationId = integrationId,
            StartedAt = DateTime.UtcNow,
            Status = EmailSyncStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailSyncLogs.Add(log);
        await _context.SaveChangesAsync();

        try
        {
            // NOTE: This is a scaffolding implementation. Actual IMAP/OAuth sync should be implemented here.
            // For now, we just mark the sync as successful and schedule the next run.

            log.Status = EmailSyncStatus.Success;
            log.CompletedAt = DateTime.UtcNow;

            integration.LastSyncAt = DateTime.UtcNow;
            integration.LastSyncStatus = EmailSyncStatus.Success;
            integration.LastSyncError = null;
            integration.NextSyncAt = DateTime.UtcNow.AddMinutes(integration.SyncIntervalMinutes > 0 ? integration.SyncIntervalMinutes : _options.DefaultSyncIntervalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sync failed for integration {IntegrationId}", integrationId);

            log.Status = EmailSyncStatus.Failed;
            log.CompletedAt = DateTime.UtcNow;
            log.ErrorMessage = ex.Message;
            log.ErrorStackTrace = ex.StackTrace;

            integration.LastSyncStatus = EmailSyncStatus.Failed;
            integration.LastSyncError = ex.Message;
            integration.NextSyncAt = DateTime.UtcNow.AddMinutes(5);
        }

        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<EmailSyncLog> SyncNowAsync(int userId, int integrationId)
    {
        var integration = await _context.EmailIntegrations
            .FirstOrDefaultAsync(i => i.Id == integrationId && i.UserId == userId && i.IsActive && !i.IsDeleted)
            ?? throw new KeyNotFoundException($"Integration {integrationId} not found or inactive");

        return await SyncAsync(integration.Id);
    }

    public async Task SyncAllDueAsync()
    {
        var now = DateTime.UtcNow;
        var dueIntegrations = await _context.EmailIntegrations
            .Where(i => i.IsActive && !i.IsDeleted && (i.NextSyncAt == null || i.NextSyncAt <= now))
            .ToListAsync();

        _logger.LogInformation("Found {Count} email integrations due for sync", dueIntegrations.Count);

        foreach (var integration in dueIntegrations)
        {
            try
            {
                await SyncAsync(integration.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync email integration {IntegrationId}", integration.Id);
            }
        }
    }
}
