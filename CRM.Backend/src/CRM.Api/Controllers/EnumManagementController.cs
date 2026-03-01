// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Full CRUD admin API for managing lookup categories and their items.
/// Restricted to Admin role.
/// </summary>
[ApiController]
[Route("api/enum-management")]
[Authorize(Roles = "Admin")]
public class EnumManagementController : CrmControllerBase
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<EnumManagementController> _logger;

    public EnumManagementController(ICrmDbContext context, ILogger<EnumManagementController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────
    // CATEGORIES
    // ─────────────────────────────────────────────────────────────────

    /// <summary>List all lookup categories (with item count).</summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<LookupCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] string? entityType, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.LookupCategories.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => !c.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(c => c.EntityType == entityType);
        }

        var categories = await query
            .OrderBy(c => c.EntityType)
            .ThenBy(c => c.Name)
            .Select(c => new LookupCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                IsSystemManaged = c.IsSystemManaged,
                AllowCustomValues = c.AllowCustomValues,
                EntityType = c.EntityType,
                PropertyName = c.PropertyName,
                ValidationSchema = c.ValidationSchema,
                ItemCount = c.Items.Count(i => !i.IsDeleted),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
            })
            .ToListAsync(ct);

        return Ok(categories);
    }

    /// <summary>Get a single category with its items.</summary>
    [HttpGet("categories/{id:int}")]
    [ProducesResponseType(typeof(LookupCategoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(int id, CancellationToken ct = default)
    {
        var cat = await _context.LookupCategories
            .Include(c => c.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

        if (cat == null)
        {
            return NotFound();
        }

        return Ok(MapToDetailDto(cat));
    }

    /// <summary>Create a new lookup category.</summary>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(LookupCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateLookupCategoryDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await _context.LookupCategories.AnyAsync(c => c.Name == dto.Name && !c.IsDeleted, ct))
        {
            return Conflict(new { message = $"A category named '{dto.Name}' already exists." });
        }

        var category = new LookupCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            EntityType = dto.EntityType,
            PropertyName = dto.PropertyName,
            IsActive = dto.IsActive,
            AllowCustomValues = dto.AllowCustomValues,
            ValidationSchema = dto.ValidationSchema,
            IsSystemManaged = false,
            CreatedAt = DateTime.UtcNow,
        };

        _context.LookupCategories.Add(category);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Created LookupCategory {Id} '{Name}'", category.Id, category.Name);

        var result = await GetCategoryDto(category.Id, ct);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, result);
    }

    /// <summary>Update an existing lookup category.</summary>
    [HttpPut("categories/{id:int}")]
    [ProducesResponseType(typeof(LookupCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateLookupCategoryDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var cat = await _context.LookupCategories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        if (cat == null)
        {
            return NotFound();
        }

        var duplicate = await _context.LookupCategories.AnyAsync(c => c.Name == dto.Name && c.Id != id && !c.IsDeleted, ct);
        if (duplicate)
        {
            return Conflict(new { message = $"A category named '{dto.Name}' already exists." });
        }

        cat.Name = dto.Name;
        cat.Description = dto.Description;
        cat.EntityType = dto.EntityType;
        cat.PropertyName = dto.PropertyName;
        cat.IsActive = dto.IsActive;
        cat.AllowCustomValues = dto.AllowCustomValues;
        cat.ValidationSchema = dto.ValidationSchema;
        cat.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Updated LookupCategory {Id} '{Name}'", cat.Id, cat.Name);

        return Ok(await GetCategoryDto(id, ct));
    }

    /// <summary>Soft-delete a lookup category (blocks if system-managed).</summary>
    [HttpDelete("categories/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct = default)
    {
        var cat = await _context.LookupCategories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

        if (cat == null)
        {
            return NotFound();
        }

        if (cat.IsSystemManaged)
        {
            return Conflict(new { message = "System-managed categories cannot be deleted." });
        }

        cat.IsDeleted = true;
        cat.UpdatedAt = DateTime.UtcNow;

        // Soft-delete all items
        foreach (var item in cat.Items.Where(i => !i.IsDeleted))
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted LookupCategory {Id} '{Name}'", cat.Id, cat.Name);

        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────────
    // ITEMS
    // ─────────────────────────────────────────────────────────────────

    /// <summary>List all items within a category.</summary>
    [HttpGet("categories/{categoryId:int}/items")]
    [ProducesResponseType(typeof(List<LookupItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItems(int categoryId, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var exists = await _context.LookupCategories.AnyAsync(c => c.Id == categoryId && !c.IsDeleted, ct);
        if (!exists)
        {
            return NotFound();
        }

        var query = _context.LookupItems.Where(i => i.LookupCategoryId == categoryId && !i.IsDeleted);
        if (!includeInactive)
        {
            query = query.Where(i => i.IsActive);
        }

        var items = await query
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Value)
            .Select(i => MapToItemDto(i))
            .ToListAsync(ct);

        return Ok(items);
    }

    /// <summary>Create a new item in a category.</summary>
    [HttpPost("categories/{categoryId:int}/items")]
    [ProducesResponseType(typeof(LookupItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateItem(int categoryId, [FromBody] CreateLookupItemDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var cat = await _context.LookupCategories.FirstOrDefaultAsync(c => c.Id == categoryId && !c.IsDeleted, ct);
        if (cat == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (await _context.LookupItems.AnyAsync(i => i.LookupCategoryId == categoryId && i.Key == dto.Key && !i.IsDeleted, ct))
        {
            return Conflict(new { message = $"An item with key '{dto.Key}' already exists in this category." });
        }

        // Ensure only one default per category
        if (dto.IsDefault)
        {
            await ClearDefaultFlagAsync(categoryId, ct);
        }

        var item = new LookupItem
        {
            LookupCategoryId = categoryId,
            Key = dto.Key,
            Value = dto.Value,
            Meta = dto.Meta,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            IsDefault = dto.IsDefault,
            IsSystemValue = false,
            Color = dto.Color,
            Icon = dto.Icon,
            ValidationRules = dto.ValidationRules,
            CreatedAt = DateTime.UtcNow,
        };

        _context.LookupItems.Add(item);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetItems), new { categoryId }, MapToItemDto(item));
    }

    /// <summary>Get a single lookup item by ID.</summary>
    [HttpGet("items/{id:int}")]
    [ProducesResponseType(typeof(LookupItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItem(int id, CancellationToken ct = default)
    {
        var item = await _context.LookupItems.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, ct);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(MapToItemDto(item));
    }

    /// <summary>Update a lookup item.</summary>
    [HttpPut("items/{id:int}")]
    [ProducesResponseType(typeof(LookupItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateLookupItemDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var item = await _context.LookupItems.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, ct);
        if (item == null)
        {
            return NotFound();
        }

        if (item.IsSystemValue)
        {
            return Conflict(new { message = "System values cannot be modified." });
        }

        // Unique key check within category
        var duplicate = await _context.LookupItems.AnyAsync(i => i.LookupCategoryId == item.LookupCategoryId && i.Key == dto.Key && i.Id != id && !i.IsDeleted, ct);
        if (duplicate)
        {
            return Conflict(new { message = $"An item with key '{dto.Key}' already exists in this category." });
        }

        // Ensure only one default per category
        if (dto.IsDefault && !item.IsDefault)
        {
            await ClearDefaultFlagAsync(item.LookupCategoryId, ct);
        }

        item.Key = dto.Key;
        item.Value = dto.Value;
        item.Meta = dto.Meta;
        item.SortOrder = dto.SortOrder;
        item.IsActive = dto.IsActive;
        item.IsDefault = dto.IsDefault;
        item.Color = dto.Color;
        item.Icon = dto.Icon;
        item.ValidationRules = dto.ValidationRules;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Ok(MapToItemDto(item));
    }

    /// <summary>Soft-delete a lookup item.</summary>
    [HttpDelete("items/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteItem(int id, CancellationToken ct = default)
    {
        var item = await _context.LookupItems.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, ct);
        if (item == null)
        {
            return NotFound();
        }

        if (item.IsSystemValue)
        {
            return Conflict(new { message = "System values cannot be deleted." });
        }

        item.IsDeleted = true;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>Reorder items within a category by providing an ordered list of IDs.</summary>
    [HttpPost("categories/{categoryId:int}/items/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderItems(int categoryId, [FromBody] ReorderItemsDto dto, CancellationToken ct = default)
    {
        var exists = await _context.LookupCategories.AnyAsync(c => c.Id == categoryId && !c.IsDeleted, ct);
        if (!exists)
        {
            return NotFound();
        }

        var items = await _context.LookupItems
            .Where(i => i.LookupCategoryId == categoryId && !i.IsDeleted)
            .ToListAsync(ct);

        for (int i = 0; i < dto.OrderedIds.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.Id == dto.OrderedIds[i]);
            if (item != null)
            {
                item.SortOrder = i + 1;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────

    private async Task ClearDefaultFlagAsync(int categoryId, CancellationToken ct)
    {
        var currentDefaults = await _context.LookupItems
            .Where(i => i.LookupCategoryId == categoryId && i.IsDefault && !i.IsDeleted)
            .ToListAsync(ct);

        foreach (var d in currentDefaults)
        {
            d.IsDefault = false;
        }
    }

    private async Task<LookupCategoryDto?> GetCategoryDto(int id, CancellationToken ct)
    {
        return await _context.LookupCategories
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new LookupCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                IsSystemManaged = c.IsSystemManaged,
                AllowCustomValues = c.AllowCustomValues,
                EntityType = c.EntityType,
                PropertyName = c.PropertyName,
                ValidationSchema = c.ValidationSchema,
                ItemCount = c.Items.Count(i => !i.IsDeleted),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    private static LookupCategoryDetailDto MapToDetailDto(LookupCategory cat) => new()
    {
        Id = cat.Id,
        Name = cat.Name,
        Description = cat.Description,
        IsActive = cat.IsActive,
        IsSystemManaged = cat.IsSystemManaged,
        AllowCustomValues = cat.AllowCustomValues,
        EntityType = cat.EntityType,
        PropertyName = cat.PropertyName,
        ValidationSchema = cat.ValidationSchema,
        ItemCount = cat.Items.Count,
        CreatedAt = cat.CreatedAt,
        UpdatedAt = cat.UpdatedAt,
        Items = cat.Items.OrderBy(i => i.SortOrder).ThenBy(i => i.Value).Select(MapToItemDto).ToList(),
    };

    private static LookupItemDto MapToItemDto(LookupItem i) => new()
    {
        Id = i.Id,
        LookupCategoryId = i.LookupCategoryId,
        Key = i.Key,
        Value = i.Value,
        Meta = i.Meta,
        SortOrder = i.SortOrder,
        IsActive = i.IsActive,
        IsDefault = i.IsDefault,
        IsSystemValue = i.IsSystemValue,
        Color = i.Color,
        Icon = i.Icon,
        ValidationRules = i.ValidationRules,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt,
    };
}
