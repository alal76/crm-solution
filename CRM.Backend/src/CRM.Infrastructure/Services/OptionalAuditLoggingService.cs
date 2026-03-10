// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace CRM.Infrastructure.Services;

/// <summary>
/// OPTIONAL Service for audit logging (feature-flagged).
///
/// This service is completely optional and controlled bythe UseOptionalAuditLogging feature flag (default: false).
/// When disabled, it adds ZERO overhead as it's not even registered in the DI container.
///
/// When enabled:
/// - Logs user actions (Create, Update, Delete, Export, etc.)
/// - Provides audit trail querying
/// - Manages retention policies
/// </summary>
public class OptionalAuditLoggingService : IOptionalAuditLoggingService
{
    private readonly IFeatureManager _featureManager;
    private readonly ICrmDbContext _context;
    private readonly ILogger<OptionalAuditLoggingService> _logger;
    private readonly IRedisStreamService _streamService;
    private bool? _cachedFeatureEnabled;

    internal const string StreamName = "crm:audit:stream";
    private const string FEATURE_FLAG_NAME = "UseOptionalAuditLogging";

    public OptionalAuditLoggingService(
        IFeatureManager featureManager,
        ICrmDbContext context,
        ILogger<OptionalAuditLoggingService> logger,
        IRedisStreamService streamService)
    {
        _featureManager = featureManager;
        _context = context;
        _logger = logger;
        _streamService = streamService;
        // AP-016: Cache feature flag once at construction (per-scope) to remove per-call blocking
        _cachedFeatureEnabled = featureManager.IsEnabledAsync(FEATURE_FLAG_NAME).GetAwaiter().GetResult(); // NOSONAR S4462 -- called once per scope; sync interface, no async alternative
    }

    #region Action Logging

    public async Task<int?> LogActionAsync(
        int userId,
        string action,
        string entityType,
        int entityId,
        string? oldValues = null,
        string? newValues = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        // Check if feature is enabled
        if (!await IsEnabledAsync(cancellationToken))
        {
            return null;
        }

        try
        {
            var auditEvent = new AuditEvent
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            };

            await PublishAuditEventAsync(auditEvent, cancellationToken);

            _logger.LogInformation(
                "Audit event queued: User {UserId} {Action} {EntityType}#{EntityId}",
                userId, action, entityType, entityId);

            return null; // ID not known yet — consumer will assign it on DB insert
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging action for {EntityType}#{EntityId}", entityType, entityId);
            return null;
        }
    }

    #endregion

    #region Async Queue (FLAG-005)

    /// <inheritdoc />
    public async Task PublishAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        if (!await IsEnabledAsync(cancellationToken))
        {
            return;
        }

        try
        {
            var data = new Dictionary<string, string>
            {
                ["userId"] = auditEvent.UserId?.ToString() ?? string.Empty,
                ["action"] = auditEvent.Action,
                ["entityType"] = auditEvent.EntityType ?? string.Empty,
                ["entityId"] = auditEvent.EntityId?.ToString() ?? string.Empty,
                ["oldValues"] = auditEvent.OldValues ?? string.Empty,
                ["newValues"] = auditEvent.NewValues ?? string.Empty,
                ["reason"] = auditEvent.Reason ?? string.Empty,
                ["ipAddress"] = auditEvent.IpAddress ?? string.Empty,
                ["userAgent"] = auditEvent.UserAgent ?? string.Empty,
                ["timestamp"] = auditEvent.Timestamp.ToString("O")
            };

            var messageId = await _streamService.PublishAsync(
                StreamName, "AuditEvent", data, cancellationToken);

            if (string.IsNullOrEmpty(messageId))
            {
                // Redis not available — fall back to synchronous DB write
                _logger.LogWarning("Redis stream unavailable; writing audit event directly to DB");
                await SaveDirectlyToDbAsync(auditEvent, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueuing audit event to Redis stream; falling back to direct DB write");
            await SaveDirectlyToDbAsync(auditEvent, cancellationToken);
        }
    }

    private async Task SaveDirectlyToDbAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog
        {
            UserId = auditEvent.UserId,
            Action = auditEvent.Action,
            EntityType = auditEvent.EntityType,
            EntityId = auditEvent.EntityId,
            OldValues = auditEvent.OldValues,
            NewValues = auditEvent.NewValues,
            Details = auditEvent.Reason,
            IpAddress = auditEvent.IpAddress,
            UserAgent = auditEvent.UserAgent,
            CreatedAt = auditEvent.Timestamp,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Query & Export

    public async Task<IEnumerable<AuditLogEntryDto>> GetEntityAuditTrailAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        // Check if feature is enabled
        if (!await IsEnabledAsync(cancellationToken))
        {
            return Enumerable.Empty<AuditLogEntryDto>();
        }

        try
        {
            // In a real implementation, would query AuditLog table
            // For now, return empty
            _logger.LogDebug($"Querying audit trail for {entityType}#{entityId}");
            return Enumerable.Empty<AuditLogEntryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting audit trail for {entityType}#{entityId}");
            return Enumerable.Empty<AuditLogEntryDto>();
        }
    }

    public async Task<IEnumerable<AuditLogEntryDto>> GetAuditLogsAsync(
        AuditLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        // Check if feature is enabled
        if (!await IsEnabledAsync(cancellationToken))
        {
            return Enumerable.Empty<AuditLogEntryDto>();
        }

        try
        {
            // In a real implementation, would query AuditLog table with filters
            _logger.LogDebug($"Getting audit logs, filter: {filter.EntityType}, user: {filter.UserId}");
            return Enumerable.Empty<AuditLogEntryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return Enumerable.Empty<AuditLogEntryDto>();
        }
    }

    #endregion

    #region Policies

    public async Task<AuditRetentionPolicyDto> GetRetentionPolicyAsync(CancellationToken cancellationToken = default)
    {
        // Even if feature disabled, can return default policy
        return new AuditRetentionPolicyDto
        {
            RetentionDays = 365,
            ArchiveAfterDays = 90,
            CompressArchives = true
        };
    }

    public async Task SetRetentionPolicyAsync(AuditRetentionPolicyDto policy, CancellationToken cancellationToken = default)
    {
        // Check if feature is enabled
        if (!await IsEnabledAsync(cancellationToken))
        {
            _logger.LogWarning("Attempted to set retention policy but audit logging feature is disabled");
            return;
        }

        try
        {
            _logger.LogInformation($"Setting audit retention policy: {policy.RetentionDays} days");
            // Would save to SystemSettings
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting retention policy");
        }
    }

    #endregion

    #region Utilities

    public bool IsEnabled()
    {
        // Value is always set in constructor (AP-016 fix)
        return _cachedFeatureEnabled!.Value;
    }

    private async Task<bool> IsEnabledAsync(CancellationToken cancellationToken)
    {
        return await _featureManager.IsEnabledAsync(FEATURE_FLAG_NAME, cancellationToken);
    }

    #endregion
}
