// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for audit logging operations.
/// Tracks all changes to entities for compliance and troubleshooting.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<AuditLogService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public AuditLogService(ICrmDbContext dbContext, ILogger<AuditLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Audit Log Creation

    /// <inheritdoc />
    public async Task<int> LogCreateAsync(
        string entityType,
        int entityId,
        string entityName,
        int? userId,
        Dictionary<string, object> newValues,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Action = "Create",
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            UserId = userId,
            NewValues = SerializeValues(newValues),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Audit log created: {Action} on {EntityType} #{EntityId} by User #{UserId}",
            auditLog.Action, entityType, entityId, userId);

        return auditLog.Id;
    }

    /// <inheritdoc />
    public async Task<int> LogUpdateAsync(
        string entityType,
        int entityId,
        string entityName,
        int? userId,
        Dictionary<string, object> oldValues,
        Dictionary<string, object> newValues,
        List<string> changedProperties,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Action = "Update",
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            UserId = userId,
            OldValues = SerializeValues(oldValues),
            NewValues = SerializeValues(newValues),
            ChangedProperties = string.Join(",", changedProperties),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Audit log created: {Action} on {EntityType} #{EntityId} by User #{UserId}, changed: {Properties}",
            auditLog.Action, entityType, entityId, userId, auditLog.ChangedProperties);

        return auditLog.Id;
    }

    /// <inheritdoc />
    public async Task<int> LogDeleteAsync(
        string entityType,
        int entityId,
        string entityName,
        int? userId,
        Dictionary<string, object> oldValues,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Action = "Delete",
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            UserId = userId,
            OldValues = SerializeValues(oldValues),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Audit log created: {Action} on {EntityType} #{EntityId} by User #{UserId}",
            auditLog.Action, entityType, entityId, userId);

        return auditLog.Id;
    }

    /// <inheritdoc />
    public async Task<int> LogActionAsync(
        string action,
        string? entityType = null,
        int? entityId = null,
        int? userId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Audit log created: {Action} on {EntityType} #{EntityId} by User #{UserId}",
            action, entityType ?? "N/A", entityId, userId);

        return auditLog.Id;
    }

    #endregion

    #region Query Audit Logs

    /// <inheritdoc />
    public async Task<AuditLogPageDto> GetAuditLogsAsync(
        string? entityType = null,
        int? entityId = null,
        int? userId = null,
        string? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs
            .Include(a => a.User)
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (entityId.HasValue)
            query = query.Where(a => a.EntityId == entityId.Value);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        if (fromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.CreatedAt <= toDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync(cancellationToken);

        return new AuditLogPageDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLogDto>> GetEntityHistoryAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        var logs = await _dbContext.AuditLogs
            .Include(a => a.User)
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return logs.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<AuditLogPageDto> GetUserActivityAsync(
        int userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return await GetAuditLogsAsync(
            entityType: null,
            entityId: null,
            userId: userId,
            action: null,
            fromDate: fromDate,
            toDate: toDate,
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuditLogPageDto> SearchAsync(
        string query,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var searchTerm = query.ToLowerInvariant();

        var dbQuery = _dbContext.AuditLogs
            .Include(a => a.User)
            .AsNoTracking()
            .Where(a => !a.IsDeleted &&
                (a.Action.ToLower().Contains(searchTerm) ||
                 (a.EntityType != null && a.EntityType.ToLower().Contains(searchTerm)) ||
                 (a.EntityName != null && a.EntityName.ToLower().Contains(searchTerm)) ||
                 (a.Details != null && a.Details.ToLower().Contains(searchTerm))));

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync(cancellationToken);

        return new AuditLogPageDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    #endregion

    #region Statistics & Reports

    /// <inheritdoc />
    public async Task<AuditStatsDto> GetStatisticsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var logs = await _dbContext.AuditLogs
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.CreatedAt >= fromDate && a.CreatedAt <= toDate)
            .ToListAsync(cancellationToken);

        var stats = new AuditStatsDto
        {
            TotalActions = logs.Count,
            CreatedCount = logs.Count(l => l.Action == "Create"),
            UpdatedCount = logs.Count(l => l.Action == "Update"),
            DeletedCount = logs.Count(l => l.Action == "Delete"),
            UniqueUsers = logs.Where(l => l.UserId.HasValue).Select(l => l.UserId!.Value).Distinct().Count(),
            ActionsByType = logs.GroupBy(l => l.Action).ToDictionary(g => g.Key, g => g.Count()),
            ActionsByEntity = logs.Where(l => !string.IsNullOrEmpty(l.EntityType))
                .GroupBy(l => l.EntityType!)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }

    /// <inheritdoc />
    public async Task<EntityChangeHistoryDto> GetEntityChangeHistoryAsync(
        string entityType,
        int entityId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs
            .Include(a => a.User)
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.EntityType == entityType && a.EntityId == entityId);

        if (fromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.CreatedAt <= toDate.Value);

        var logs = await query.OrderBy(a => a.CreatedAt).ToListAsync(cancellationToken);

        var result = new EntityChangeHistoryDto
        {
            EntityType = entityType,
            EntityId = entityId,
            EntityName = logs.FirstOrDefault()?.EntityName,
            CreatedAt = logs.FirstOrDefault(l => l.Action == "Create")?.CreatedAt,
            LastModifiedAt = logs.LastOrDefault()?.CreatedAt,
            Changes = new List<ChangeEntry>()
        };

        foreach (var log in logs)
        {
            var changedProperties = string.IsNullOrEmpty(log.ChangedProperties)
                ? new List<string>()
                : log.ChangedProperties.Split(',').ToList();

            if (changedProperties.Count == 0 && (log.Action == "Create" || log.Action == "Delete"))
            {
                result.Changes.Add(new ChangeEntry
                {
                    Timestamp = log.CreatedAt,
                    Action = log.Action,
                    UserId = log.UserId,
                    UserName = log.User?.Email,
                    PropertyName = "*",
                    OldValue = log.Action == "Delete" ? log.OldValues : null,
                    NewValue = log.Action == "Create" ? log.NewValues : null
                });
            }
            else
            {
                var oldVals = DeserializeValues(log.OldValues);
                var newVals = DeserializeValues(log.NewValues);

                foreach (var prop in changedProperties.Where(p => !string.IsNullOrEmpty(p)))
                {
                    result.Changes.Add(new ChangeEntry
                    {
                        Timestamp = log.CreatedAt,
                        Action = log.Action,
                        UserId = log.UserId,
                        UserName = log.User?.Email,
                        PropertyName = prop,
                        OldValue = oldVals?.GetValueOrDefault(prop),
                        NewValue = newVals?.GetValueOrDefault(prop)
                    });
                }
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportToCsvAsync(
        string? entityType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs
            .Include(a => a.User)
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (fromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.CreatedAt <= toDate.Value);

        var logs = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Timestamp,UserId,UserEmail,Action,EntityType,EntityId,EntityName,ChangedProperties,IpAddress");

        foreach (var log in logs)
        {
            sb.AppendLine($"{log.Id},{log.CreatedAt:O},{log.UserId},{EscapeCsv(log.User?.Email)}," +
                $"{EscapeCsv(log.Action)},{EscapeCsv(log.EntityType)},{log.EntityId}," +
                $"{EscapeCsv(log.EntityName)},{EscapeCsv(log.ChangedProperties)},{EscapeCsv(log.IpAddress)}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    #endregion

    #region Cleanup & Maintenance

    /// <inheritdoc />
    public async Task<int> DeleteOldLogsAsync(
        DateTime beforeDate,
        CancellationToken cancellationToken = default)
    {
        var logsToDelete = await _dbContext.AuditLogs
            .Where(a => a.CreatedAt < beforeDate)
            .ToListAsync(cancellationToken);

        foreach (var log in logsToDelete)
        {
            log.IsDeleted = true;
            log.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Soft-deleted {Count} audit logs older than {BeforeDate}", logsToDelete.Count, beforeDate);

        return logsToDelete.Count;
    }

    /// <inheritdoc />
    public async Task<int> ArchiveLogsAsync(
        DateTime beforeDate,
        CancellationToken cancellationToken = default)
    {
        // For now, archiving is same as soft-delete
        // In a production system, this would move logs to a separate archive table or storage
        return await DeleteOldLogsAsync(beforeDate, cancellationToken);
    }

    #endregion

    #region Helper Methods

    private static AuditLogDto MapToDto(AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            EntityName = log.EntityName,
            UserId = log.UserId,
            UserName = log.User?.Email,
            OldValues = DeserializeValues(log.OldValues),
            NewValues = DeserializeValues(log.NewValues),
            ChangedProperties = string.IsNullOrEmpty(log.ChangedProperties)
                ? null
                : log.ChangedProperties.Split(',').ToList(),
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            Timestamp = log.CreatedAt
        };
    }

    private static string? SerializeValues(Dictionary<string, object>? values)
    {
        if (values == null || values.Count == 0)
            return null;

        return JsonSerializer.Serialize(values, JsonOptions);
    }

    private static Dictionary<string, object>? DeserializeValues(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    #endregion
}
