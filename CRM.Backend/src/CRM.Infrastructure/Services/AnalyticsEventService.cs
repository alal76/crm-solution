// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for managing analytics events.
/// </summary>
public class AnalyticsEventService : IAnalyticsEventService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<AnalyticsEventService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsEventService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    public AnalyticsEventService(ICrmDbContext context, ILogger<AnalyticsEventService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AnalyticsEventDto> CreateAsync(CreateAnalyticsEventDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new AnalyticsEvent
        {
            EventName = dto.EventName,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            UserId = dto.UserId,
            Timestamp = dto.Timestamp ?? DateTime.UtcNow,
            Metadata = dto.Metadata,
            CreatedAt = DateTime.UtcNow,
        };

        _context.AnalyticsEvents.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created analytics event {Id}: {EventName} for {EntityType}:{EntityId}",
            entity.Id, entity.EventName, entity.EntityType, entity.EntityId);

        return await MapToDtoAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AnalyticsEventDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.AnalyticsEvents
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);

        return entity != null ? await MapToDtoAsync(entity, cancellationToken) : null;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AnalyticsEventDto>> GetAllAsync(AnalyticsEventFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AnalyticsEvents
            .Include(e => e.User)
            .Where(e => !e.IsDeleted)
            .AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.EventName))
            {
                query = query.Where(e => e.EventName == filter.EventName);
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityType))
            {
                query = query.Where(e => e.EntityType == filter.EntityType);
            }

            if (filter.EntityId.HasValue)
            {
                query = query.Where(e => e.EntityId == filter.EntityId.Value);
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(e => e.UserId == filter.UserId.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(e => e.Timestamp >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(e => e.Timestamp <= filter.ToDate.Value);
            }
        }

        var page = filter?.Page ?? 1;
        var pageSize = filter?.PageSize ?? 50;

        var entities = await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var results = new List<AnalyticsEventDto>();
        foreach (var entity in entities)
        {
            results.Add(await MapToDtoAsync(entity, cancellationToken));
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AnalyticsEventDto>> GetByEntityAsync(string entityType, int entityId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var entities = await _context.AnalyticsEvents
            .Include(e => e.User)
            .Where(e => e.EntityType == entityType && e.EntityId == entityId && !e.IsDeleted)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var results = new List<AnalyticsEventDto>();
        foreach (var entity in entities)
        {
            results.Add(await MapToDtoAsync(entity, cancellationToken));
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AnalyticsEventDto>> GetByUserAsync(int userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var entities = await _context.AnalyticsEvents
            .Include(e => e.User)
            .Where(e => e.UserId == userId && !e.IsDeleted)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var results = new List<AnalyticsEventDto>();
        foreach (var entity in entities)
        {
            results.Add(await MapToDtoAsync(entity, cancellationToken));
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.AnalyticsEvents
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Soft-deleted analytics event {Id}", id);
        return true;
    }

    private Task<AnalyticsEventDto> MapToDtoAsync(AnalyticsEvent entity, CancellationToken cancellationToken)
    {
        var dto = new AnalyticsEventDto
        {
            Id = entity.Id,
            EventName = entity.EventName,
            EntityType = entity.EntityType,
            EntityId = entity.EntityId,
            UserId = entity.UserId,
            UserName = entity.User != null ? $"{entity.User.FirstName} {entity.User.LastName}".Trim() : null,
            Timestamp = entity.Timestamp,
            Metadata = entity.Metadata,
            CreatedAt = entity.CreatedAt,
        };

        return Task.FromResult(dto);
    }
}
