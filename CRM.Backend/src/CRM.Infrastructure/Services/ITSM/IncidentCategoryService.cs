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
/// Service implementation for managing ITSM incident categories.
/// </summary>
public class IncidentCategoryService : IIncidentCategoryService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<IncidentCategoryService> _logger;

    public IncidentCategoryService(ICrmDbContext dbContext, ILogger<IncidentCategoryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IncidentCategoryDto> CreateAsync(CreateIncidentCategoryDto dto, CancellationToken ct = default)
    {
        var entity = new IncidentCategory
        {
            CategoryName = dto.CategoryName,
            SubCategory = dto.SubCategory,
            Description = dto.Description,
            DefaultPriority = dto.DefaultPriority,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _dbContext.IncidentCategories.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Created incident category {Id}: {Name}/{Sub}", entity.Id, entity.CategoryName, entity.SubCategory);

        return MapToDto(entity);
    }

    public async Task<IncidentCategoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbContext.IncidentCategories
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<List<IncidentCategoryDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _dbContext.IncidentCategories.Where(e => !e.IsDeleted);
        if (!includeInactive)
            query = query.Where(e => e.IsActive);

        return await query
            .OrderBy(e => e.CategoryName)
            .ThenBy(e => e.SubCategory)
            .Select(e => MapToDto(e))
            .ToListAsync(ct);
    }

    private static IncidentCategoryDto MapToDto(IncidentCategory e) => new()
    {
        Id = e.Id,
        CategoryName = e.CategoryName,
        SubCategory = e.SubCategory,
        Description = e.Description,
        DefaultPriority = e.DefaultPriority,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
    };
}
