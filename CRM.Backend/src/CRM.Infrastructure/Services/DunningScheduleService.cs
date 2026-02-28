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
/// Concrete CRUD implementation for <see cref="DunningSchedule"/> steps.
///
/// All mutating operations set <c>CreatedAt</c>/<c>UpdatedAt</c> automatically
/// and use soft-delete (<c>IsDeleted = true</c>) rather than hard deletes.
///
/// BACK-010: Dunning Scheduler CRUD
/// </summary>
public sealed class DunningScheduleService : IDunningScheduleService
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<DunningScheduleService> _logger;

    /// <summary>Initialises a new instance of <see cref="DunningScheduleService"/>.</summary>
    public DunningScheduleService(ICrmDbContext db, ILogger<DunningScheduleService> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DunningScheduleDto>> GetAllAsync(
        bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.DunningSchedules
            .Where(s => !s.IsDeleted);

        if (activeOnly.HasValue)
        {
            query = query.Where(s => s.IsActive == activeOnly.Value);
        }

        var items = await query
            .OrderBy(s => s.StepOrder)
            .ThenBy(s => s.DaysOverdue)
            .Select(s => ToDto(s))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _logger.LogDebug("GetAllAsync returned {Count} dunning schedule steps.", items.Count);
        return items;
    }

    /// <inheritdoc />
    public async Task<DunningScheduleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.DunningSchedules
            .Where(s => s.Id == id && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return entity is null ? null : ToDto(entity);
    }

    /// <inheritdoc />
    public async Task<DunningScheduleDto> CreateAsync(
        CreateDunningScheduleDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var now = DateTime.UtcNow;
        var entity = new DunningSchedule
        {
            Name = dto.Name,
            DaysOverdue = dto.DaysOverdue,
            EmailSubject = dto.EmailSubject,
            EmailBody = dto.EmailBody,
            IsActive = dto.IsActive,
            StepOrder = dto.StepOrder,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false,
        };

        _db.DunningSchedules.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "DunningSchedule '{Name}' (Id={Id}) created.", entity.Name, entity.Id);

        return ToDto(entity);
    }

    /// <inheritdoc />
    public async Task<DunningScheduleDto> UpdateAsync(
        int id,
        UpdateDunningScheduleDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var entity = await _db.DunningSchedules
            .Where(s => s.Id == id && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            throw new KeyNotFoundException($"DunningSchedule with id={id} was not found.");
        }

        if (dto.Name is not null)
        {
            entity.Name = dto.Name;
        }

        if (dto.DaysOverdue.HasValue)
        {
            entity.DaysOverdue = dto.DaysOverdue.Value;
        }

        if (dto.EmailSubject is not null)
        {
            entity.EmailSubject = dto.EmailSubject;
        }

        if (dto.EmailBody is not null)
        {
            entity.EmailBody = dto.EmailBody;
        }

        if (dto.IsActive.HasValue)
        {
            entity.IsActive = dto.IsActive.Value;
        }

        if (dto.StepOrder.HasValue)
        {
            entity.StepOrder = dto.StepOrder.Value;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "DunningSchedule '{Name}' (Id={Id}) updated.", entity.Name, entity.Id);

        return ToDto(entity);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.DunningSchedules
            .Where(s => s.Id == id && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("DunningSchedule Id={Id} soft-deleted.", id);
        return true;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static DunningScheduleDto ToDto(DunningSchedule s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        DaysOverdue = s.DaysOverdue,
        EmailSubject = s.EmailSubject,
        EmailBody = s.EmailBody,
        IsActive = s.IsActive,
        StepOrder = s.StepOrder,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt.GetValueOrDefault(s.CreatedAt),
    };
}
