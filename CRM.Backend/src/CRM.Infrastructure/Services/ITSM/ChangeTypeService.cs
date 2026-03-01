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
/// Service for managing ITSM change types
/// </summary>
public class ChangeTypeService : IChangeTypeService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<ChangeTypeService> _logger;

    public ChangeTypeService(
        ICrmDbContext dbContext,
        ILogger<ChangeTypeService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ChangeTypeDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var entity = await _dbContext.Set<ChangeTypeEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting change type {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<ChangeTypeDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        try
        {
            var query = _dbContext.Set<ChangeTypeEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            var entities = await query
                .OrderBy(x => x.TypeName)
                .ToListAsync(ct);

            return entities.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all change types");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ChangeTypeDto> CreateAsync(CreateChangeTypeDto dto, CancellationToken ct = default)
    {
        try
        {
            // Check for duplicate name
            var existing = await _dbContext.Set<ChangeTypeEntity>()
                .FirstOrDefaultAsync(x => x.TypeName == dto.TypeName && !x.IsDeleted, ct);

            if (existing != null)
            {
                throw new InvalidOperationException($"A change type with name '{dto.TypeName}' already exists");
            }

            var entity = new ChangeTypeEntity
            {
                TypeName = dto.TypeName,
                Description = dto.Description,
                RequiresCAB = dto.RequiresCAB,
                RequiresApproval = dto.RequiresApproval,
                DefaultRiskLevel = dto.DefaultRiskLevel,
                LeadTimeDays = dto.LeadTimeDays,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Set<ChangeTypeEntity>().Add(entity);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Created change type {Id}: {TypeName}", entity.Id, entity.TypeName);

            return MapToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating change type {TypeName}", dto.TypeName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ChangeTypeDto> UpdateAsync(int id, UpdateChangeTypeDto dto, CancellationToken ct = default)
    {
        try
        {
            var entity = await _dbContext.Set<ChangeTypeEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Change type with id {id} not found");
            }

            // Check for duplicate name if name is being changed
            if (dto.TypeName != null && dto.TypeName != entity.TypeName)
            {
                var duplicate = await _dbContext.Set<ChangeTypeEntity>()
                    .FirstOrDefaultAsync(x => x.TypeName == dto.TypeName && x.Id != id && !x.IsDeleted, ct);

                if (duplicate != null)
                {
                    throw new InvalidOperationException($"A change type with name '{dto.TypeName}' already exists");
                }

                entity.TypeName = dto.TypeName;
            }

            if (dto.Description != null)
            {
                entity.Description = dto.Description;
            }
            if (dto.RequiresCAB.HasValue)
            {
                entity.RequiresCAB = dto.RequiresCAB.Value;
            }
            if (dto.RequiresApproval.HasValue)
            {
                entity.RequiresApproval = dto.RequiresApproval.Value;
            }
            if (dto.DefaultRiskLevel != null)
            {
                entity.DefaultRiskLevel = dto.DefaultRiskLevel;
            }
            if (dto.LeadTimeDays.HasValue)
            {
                entity.LeadTimeDays = dto.LeadTimeDays.Value;
            }
            if (dto.IsActive.HasValue)
            {
                entity.IsActive = dto.IsActive.Value;
            }


            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Updated change type {Id}: {TypeName}", entity.Id, entity.TypeName);

            return MapToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating change type {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var entity = await _dbContext.Set<ChangeTypeEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Change type with id {id} not found");
            }

            entity.IsDeleted = true;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted change type {Id}: {TypeName}", entity.Id, entity.TypeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting change type {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ChangeTypeDto?> GetByNameAsync(string typeName, CancellationToken ct = default)
    {
        try
        {
            var entity = await _dbContext.Set<ChangeTypeEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TypeName == typeName && !x.IsDeleted, ct);

            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting change type by name {TypeName}", typeName);
            throw;
        }
    }

    private static ChangeTypeDto MapToDto(ChangeTypeEntity entity)
    {
        return new ChangeTypeDto
        {
            Id = entity.Id,
            TypeName = entity.TypeName,
            Description = entity.Description,
            RequiresCAB = entity.RequiresCAB,
            RequiresApproval = entity.RequiresApproval,
            DefaultRiskLevel = entity.DefaultRiskLevel,
            LeadTimeDays = entity.LeadTimeDays,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
