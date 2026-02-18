// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service implementation for managing Configuration Item Types.
/// </summary>
public class CITypeService : ICITypeService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<CITypeService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CITypeService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    public CITypeService(ICrmDbContext dbContext, ILogger<CITypeService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CITypeDto> CreateAsync(CreateCITypeDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new CITypeDefinition
        {
            TypeName = dto.TypeName,
            TypeCategory = dto.TypeCategory,
            Description = dto.Description,
            IconName = dto.IconName,
            Color = dto.Color,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.CITypes.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created CI Type {Id}: {TypeName}", entity.Id, entity.TypeName);

        return MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<CITypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.CITypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CITypeDto>> GetAllAsync(string? category = null, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CITypes
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.TypeCategory == category);
        }

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.TypeName)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<CITypeDto?> UpdateAsync(int id, UpdateCITypeDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.CITypes
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        if (dto.TypeName != null)
        {
            entity.TypeName = dto.TypeName;
        }

        if (dto.TypeCategory != null)
        {
            entity.TypeCategory = dto.TypeCategory;
        }

        if (dto.Description != null)
        {
            entity.Description = dto.Description;
        }

        if (dto.IconName != null)
        {
            entity.IconName = dto.IconName;
        }

        if (dto.Color != null)
        {
            entity.Color = dto.Color;
        }

        if (dto.SortOrder.HasValue)
        {
            entity.SortOrder = dto.SortOrder.Value;
        }

        if (dto.IsActive.HasValue)
        {
            entity.IsActive = dto.IsActive.Value;
        }

        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated CI Type {Id}: {TypeName}", entity.Id, entity.TypeName);

        return MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.CITypes
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted CI Type {Id}: {TypeName}", entity.Id, entity.TypeName);

        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.CITypes
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => x.TypeCategory)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    private static CITypeDto MapToDto(CITypeDefinition entity)
    {
        return new CITypeDto
        {
            Id = entity.Id,
            TypeName = entity.TypeName,
            TypeCategory = entity.TypeCategory,
            Description = entity.Description,
            IconName = entity.IconName,
            Color = entity.Color,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
