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
/// Service implementation for managing ITSM catalog categories.
/// </summary>
public class CatalogCategoryService : ICatalogCategoryService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<CatalogCategoryService> _logger;

    public CatalogCategoryService(ICrmDbContext dbContext, ILogger<CatalogCategoryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CatalogCategoryDto> CreateAsync(CreateCatalogCategoryDto dto, CancellationToken ct = default)
    {
        var entity = new CatalogCategory
        {
            Name = dto.CategoryName,
            Description = dto.Description,
            IconName = dto.IconName,
            DisplayOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.CatalogCategories.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Created catalog category {Id}: {Name}", entity.CategoryId, entity.Name);

        return MapToDto(entity);
    }

    public async Task<CatalogCategoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbContext.CatalogCategories
            .FirstOrDefaultAsync(e => e.CategoryId == id && !e.IsDeleted, ct);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<List<CatalogCategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _dbContext.CatalogCategories
            .Where(e => !e.IsDeleted && e.IsActive)
            .OrderBy(e => e.DisplayOrder)
            .ToListAsync(ct);
        return entities.Select(MapToDto).ToList();
    }

    private static CatalogCategoryDto MapToDto(CatalogCategory e) => new()
    {
        CategoryId = e.CategoryId,
        Name = e.Name,
        Description = e.Description,
        IconName = e.IconName,
        DisplayOrder = e.DisplayOrder,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
    };
}
