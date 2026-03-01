// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<ServiceCatalogService> _logger;

    public ServiceCatalogService(IDbContextResolver dbContextResolver, ILogger<ServiceCatalogService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<IEnumerable<CatalogItemDto>> GetCatalogItemsAsync(int? categoryId, bool? featuredOnly)
    {
        var context = _dbContextResolver.ResolveContext();
        var query = context.CatalogItems
            .Include(c => c.Category)
            .Where(c => !c.IsDeleted && c.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        if (featuredOnly.HasValue && featuredOnly.Value)
        {
            query = query.Where(c => c.IsFeatured);
        }

        var items = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return items.Select(MapToDto);
    }

    public async Task<CatalogItemDto?> GetCatalogItemByIdAsync(int itemId)
    {
        var context = _dbContextResolver.ResolveContext();
        var item = await context.CatalogItems
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.CatalogItemId == itemId && !c.IsDeleted);

        return item == null ? null : MapToDto(item);
    }

    public async Task<int> CreateCatalogRequestAsync(CreateCatalogRequestDto dto, int requestedById)
    {
        var context = _dbContextResolver.ResolveContext();

        var request = new CatalogRequest
        {
            CatalogItemId = dto.CatalogItemId,
            RequestedById = requestedById,
            RequestedForId = dto.RequestedForId,
            VariableValues = System.Text.Json.JsonSerializer.Serialize(dto.VariableValues),
            State = CatalogRequestState.Requested,
            CreatedAt = DateTime.UtcNow
        };

        context.CatalogRequests.Add(request);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created catalog request {RequestId} for item {ItemId}", request.RequestId, dto.CatalogItemId);

        return request.RequestId;
    }

    public async Task<IEnumerable<CatalogItemDto>> SearchCatalogAsync(string searchTerm)
    {
        var context = _dbContextResolver.ResolveContext();
        var items = await context.CatalogItems
            .Include(c => c.Category)
            .Where(c => !c.IsDeleted && c.IsActive &&
                       (c.Name.Contains(searchTerm) ||
                        (c.ShortDescription != null && c.ShortDescription.Contains(searchTerm)) ||
                        (c.LongDescription != null && c.LongDescription.Contains(searchTerm))))
            .OrderBy(c => c.DisplayOrder)
            .Take(20)
            .ToListAsync();

        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<CatalogRequest>> GetMyRequestsAsync(int userId)
    {
        var context = _dbContextResolver.ResolveContext();
        return await context.CatalogRequests
            .Where(r => r.RequestedById == userId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CatalogCategoryInfo>> GetCategoriesAsync()
    {
        var context = _dbContextResolver.ResolveContext();
        var categories = await context.CatalogCategories
            .Where(c => !c.IsDeleted && c.IsActive)
            .Select(c => new CatalogCategoryInfo
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                Icon = c.IconName ?? string.Empty,
                ItemCount = c.CatalogItems != null ? c.CatalogItems.Count(i => i.IsActive && !i.IsDeleted) : 0
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories;
    }

    public async Task<int> CreateCatalogRequestForOthersAsync(CreateCatalogRequestForOthersDto dto, int requestedById)
    {
        var context = _dbContextResolver.ResolveContext();

        var request = new CatalogRequest
        {
            CatalogItemId = dto.CatalogItemId,
            RequestedById = requestedById,
            RequestedForId = dto.RequestedForUserId,
            VariableValues = dto.FormData != null ? System.Text.Json.JsonSerializer.Serialize(dto.FormData) : null,
            State = CatalogRequestState.Requested,
            CreatedAt = DateTime.UtcNow
        };

        context.CatalogRequests.Add(request);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created catalog request {RequestId} for item {ItemId} on behalf of user {UserId}",
            request.RequestId, dto.CatalogItemId, dto.RequestedForUserId);

        return request.RequestId;
    }

    public async Task<CatalogRequest?> GetRequestByIdAsync(int requestId)
    {
        var context = _dbContextResolver.ResolveContext();
        return await context.CatalogRequests
            .Include(r => r.CatalogItem)
            .FirstOrDefaultAsync(r => r.RequestId == requestId && !r.IsDeleted);
    }

    public async Task<bool> CancelRequestAsync(int requestId, int userId)
    {
        var context = _dbContextResolver.ResolveContext();
        var request = await context.CatalogRequests
            .FirstOrDefaultAsync(r => r.RequestId == requestId && !r.IsDeleted);

        if (request == null)
        {
            return false;
        }

        // Only allow cancellation by requester or admin, and only if in initial states
        if (request.RequestedById != userId)
        {
            return false;
        }

        if (request.State != CatalogRequestState.Requested && request.State != CatalogRequestState.PendingApproval)
        {
            return false;
        }

        request.State = CatalogRequestState.Cancelled;
        request.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        _logger.LogInformation("Cancelled catalog request {RequestId} by user {UserId}", requestId, userId);

        return true;
    }

    private CatalogItemDto MapToDto(CatalogItem item)
    {
        return new CatalogItemDto
        {
            CatalogItemId = item.CatalogItemId,
            Name = item.Name,
            ShortDescription = item.ShortDescription,
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.Name,
            Price = item.Price,
            IsFeatured = item.IsFeatured,
            IsActive = item.IsActive,
            RequestCount = 0 // This would need to be calculated from CatalogRequests count
        };
    }
}
