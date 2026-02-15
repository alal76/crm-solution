// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Dtos;
using CRM.Core.Interfaces;
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
    private bool? _cachedFeatureEnabled;

    private const string FEATURE_FLAG_NAME = "UseOptionalAuditLogging";

    public OptionalAuditLoggingService(
        IFeatureManager featureManager,
        ICrmDbContext context,
        ILogger<OptionalAuditLoggingService> logger)
    {
        _featureManager = featureManager;
        _context = context;
        _logger = logger;
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
            // In a real implementation, would save to AuditLog table
            // For now, just log
            _logger.LogInformation(
                $"Audit: User {userId} {action} {entityType}#{entityId}. Reason: {reason ?? "(none)"}");

            return null; // Would return audit log ID if saved
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error logging action for {entityType}#{entityId}");
            return null;
        }
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
        // Use cached value if available
        _cachedFeatureEnabled ??= _featureManager.IsEnabledAsync(FEATURE_FLAG_NAME).GetAwaiter().GetResult();
        return _cachedFeatureEnabled.Value;
    }

    private async Task<bool> IsEnabledAsync(CancellationToken cancellationToken)
    {
        return await _featureManager.IsEnabledAsync(FEATURE_FLAG_NAME, cancellationToken);
    }

    #endregion
}
