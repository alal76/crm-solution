// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Exports audit logs to CSV or JSON byte arrays, respecting the
/// <see cref="AuditLogExportRequestDto"/> filter contract.
/// TODO-SYS006-008
/// </summary>
public class AuditLogExportService : IAuditLogExportService
{
    private const int MaxPageSize = 10_000;

    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<AuditLogExportService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initialises a new instance of <see cref="AuditLogExportService"/>.
    /// </summary>
    public AuditLogExportService(
        ICrmDbContext dbContext,
        ILogger<AuditLogExportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportToCsvAsync(
        AuditLogExportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var logs = await QueryLogsAsync(request, cancellationToken);

        _logger.LogInformation(
            "Exporting {Count} audit log records to CSV", logs.Count);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Timestamp,Action,EntityType,EntityId,EntityName,UserId,IpAddress,ChangedProperties");

        foreach (var log in logs)
        {
            sb.AppendLine(string.Join(',',
                log.Id,
                log.CreatedAt.ToString("o"),
                CsvEscape(log.Action),
                CsvEscape(log.EntityType ?? string.Empty),
                log.EntityId?.ToString() ?? string.Empty,
                CsvEscape(log.EntityName ?? string.Empty),
                log.UserId?.ToString() ?? string.Empty,
                CsvEscape(log.IpAddress ?? string.Empty),
                CsvEscape(log.ChangedProperties ?? string.Empty)));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportToJsonAsync(
        AuditLogExportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var logs = await QueryLogsAsync(request, cancellationToken);

        _logger.LogInformation(
            "Exporting {Count} audit log records to JSON", logs.Count);

        var exportItems = logs.Select(log => new
        {
            log.Id,
            timestamp = log.CreatedAt,
            log.Action,
            entityType = log.EntityType,
            entityId = log.EntityId,
            entityName = log.EntityName,
            userId = log.UserId,
            ipAddress = log.IpAddress,
            changedProperties = log.ChangedProperties,
            oldValues = log.OldValues,
            newValues = log.NewValues
        }).ToList();

        return JsonSerializer.SerializeToUtf8Bytes(exportItems, JsonOptions);
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private async Task<List<Core.Entities.AuditLog>> QueryLogsAsync(
        AuditLogExportRequestDto request,
        CancellationToken cancellationToken)
    {
        var effectivePageSize = Math.Min(
            request.PageSize > 0 ? request.PageSize : 5_000,
            MaxPageSize);

        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(a => a.CreatedAt >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(a => a.CreatedAt <= request.DateTo.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(effectivePageSize)
            .ToListAsync(cancellationToken);
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
