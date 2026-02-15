// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// OPTIONAL Service for audit logging (feature-flagged).
/// 
/// **IMPORTANT:** This service is completely optional and feature-flagged with UseOptionalAuditLogging.
/// When the feature flag is disabled (default: false), this service is NOT registered and has ZERO OVERHEAD.
///
/// HEXAGONAL ARCHITECTURE:
/// - Port: Defines contract for optional audit logging
/// - Feature Flag: FeatureManagement:UseOptionalAuditLogging (default: false)
/// - When disabled: Adds no CPU, memory, or I/O overhead
/// - When enabled: Provides comprehensive audit trail functionality
///
/// USAGE:
/// - Only injected if feature flag enabled
/// - Should be used in controllers/services that need audit trail
/// - Example: IAuditLoggingService? _auditService (nullable) - check if not null
/// </summary>
public interface IOptionalAuditLoggingService
{
    #region Action Logging

    /// <summary>
    /// Log a user action (if feature enabled).
    /// Safe to call even if feature is disabled.
    /// </summary>
    /// <param name="userId">User ID who performed action</param>
    /// <param name="action">Action type (Create, Update, Delete, Export, etc.)</param>
    /// <param name="entityType">Entity type (Account, Contact, etc.)</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="oldValues">Old values (optional, for updates)</param>
    /// <param name="newValues">New values (optional)</param>
    /// <param name="reason">Reason for change (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audit log ID if saved, null if feature disabled</returns>
    Task<int?> LogActionAsync(
        int userId,
        string action,
        string entityType,
        int entityId,
        string? oldValues = null,
        string? newValues = null,
        string? reason = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Query & Export

    /// <summary>
    /// Get audit trail for an entity (if feature enabled).
    /// </summary>
    /// <param name="entityType">Entity type</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit entries, empty if feature disabled</returns>
    Task<IEnumerable<AuditLogEntryDto>> GetEntityAuditTrailAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit logs with filters (if feature enabled).
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered audit logs, empty if feature disabled</returns>
    Task<IEnumerable<AuditLogEntryDto>> GetAuditLogsAsync(
        AuditLogFilterDto filter,
        CancellationToken cancellationToken = default);

    #endregion

    #region Policies

    /// <summary>
    /// Get audit retention policy.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Retention policy DTO</returns>
    Task<AuditRetentionPolicyDto> GetRetentionPolicyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update audit retention policy.
    /// </summary>
    /// <param name="policy">Updated policy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task SetRetentionPolicyAsync(AuditRetentionPolicyDto policy, CancellationToken cancellationToken = default);

    #endregion

    #region Utilities

    /// <summary>
    /// Check if audit logging feature is enabled.
    /// Use this before logging to avoid unnecessary operations.
    /// </summary>
    /// <returns>True if feature flag enabled</returns>
    bool IsEnabled();

    #endregion
}

/// <summary>
/// DTO for audit log entry.
/// </summary>
public class AuditLogEntryDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for audit log filter.
/// </summary>
public class AuditLogFilterDto
{
    public int? UserId { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// DTO for audit retention policy.
/// </summary>
public class AuditRetentionPolicyDto
{
    /// <summary>
    /// Number of days to retain audit logs (0 = indefinite)
    /// </summary>
    public int RetentionDays { get; set; } = 365;

    /// <summary>
    /// Automatically archive logs older than this (0 = no archiving)
    /// </summary>
    public int ArchiveAfterDays { get; set; } = 90;

    /// <summary>
    /// Whether to compress archived logs
    /// </summary>
    public bool CompressArchives { get; set; } = true;

    /// <summary>
    /// Storage location for archives (S3, local disk, etc.)
    /// </summary>
    public string? ArchiveStorageLocation { get; set; }
}
