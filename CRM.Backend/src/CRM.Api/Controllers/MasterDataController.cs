using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing master data (lookup categories, lookup items, color palettes, etc.)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MasterDataController : ControllerBase
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<MasterDataController> _logger;

    public MasterDataController(ICrmDbContext context, ILogger<MasterDataController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Master Data Overview

    /// <summary>
    /// Get overview of all master data types and their counts
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<MasterDataOverviewDto>> GetOverview()
    {
        try
        {
            var overview = new MasterDataOverviewDto
            {
                LookupCategoriesCount = await _context.LookupCategories.CountAsync(),
                LookupItemsCount = await _context.LookupItems.CountAsync(),
                ColorPalettesCount = await _context.ColorPalettes.CountAsync(),
                ZipCodesCount = await _context.ZipCodes.CountAsync(),
                ServiceRequestCategoriesCount = await _context.ServiceRequestCategories.CountAsync(),
                ServiceRequestTypesCount = await _context.ServiceRequestTypes.CountAsync(),
                DataTypes = new List<MasterDataTypeInfo>
                {
                    new() { Name = "Lookup Categories", TableName = "LookupCategories", Count = await _context.LookupCategories.CountAsync(), CanImportExport = true },
                    new() { Name = "Lookup Items", TableName = "LookupItems", Count = await _context.LookupItems.CountAsync(), CanImportExport = true },
                    new() { Name = "Color Palettes", TableName = "ColorPalettes", Count = await _context.ColorPalettes.CountAsync(), CanImportExport = true },
                    new() { Name = "ZIP Codes", TableName = "ZipCodes", Count = await _context.ZipCodes.CountAsync(), CanImportExport = true },
                    new() { Name = "Service Request Categories", TableName = "ServiceRequestCategories", Count = await _context.ServiceRequestCategories.CountAsync(), CanImportExport = true },
                    new() { Name = "Service Request Types", TableName = "ServiceRequestTypes", Count = await _context.ServiceRequestTypes.CountAsync(), CanImportExport = true },
                }
            };
            return Ok(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting master data overview");
            return StatusCode(500, new { message = "Error getting master data overview" });
        }
    }

    #endregion

    #region Lookup Categories

    /// <summary>
    /// Get all lookup categories with their items
    /// </summary>
    [HttpGet("lookup-categories")]
    public async Task<ActionResult<List<LookupCategoryDto>>> GetLookupCategories([FromQuery] string? search = null)
    {
        try
        {
            var query = _context.LookupCategories.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(search) || 
                                        (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            var categories = await query
                .Include(c => c.Items)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(categories.Select(c => new LookupCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ItemCount = c.Items.Count,
                Items = c.Items.OrderBy(i => i.SortOrder).Select(i => new LookupItemDto
                {
                    Id = i.Id,
                    Key = i.Key,
                    Value = i.Value,
                    Meta = i.Meta,
                    SortOrder = i.SortOrder,
                    IsActive = i.IsActive,
                    ParentItemId = i.ParentItemId
                }).ToList()
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lookup categories");
            return StatusCode(500, new { message = "Error getting lookup categories" });
        }
    }

    /// <summary>
    /// Create a new lookup category
    /// </summary>
    [HttpPost("lookup-categories")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LookupCategoryDto>> CreateLookupCategory([FromBody] CreateLookupCategoryRequest request)
    {
        try
        {
            var category = new LookupCategory
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = true
            };

            _context.LookupCategories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new LookupCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                ItemCount = 0,
                Items = new List<LookupItemDto>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lookup category");
            return StatusCode(500, new { message = "Error creating lookup category" });
        }
    }

    /// <summary>
    /// Update a lookup category
    /// </summary>
    [HttpPut("lookup-categories/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateLookupCategory(int id, [FromBody] UpdateLookupCategoryRequest request)
    {
        try
        {
            var category = await _context.LookupCategories.FindAsync(id);
            if (category == null) return NotFound();

            category.Name = request.Name ?? category.Name;
            category.Description = request.Description ?? category.Description;
            category.IsActive = request.IsActive ?? category.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Category updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lookup category");
            return StatusCode(500, new { message = "Error updating lookup category" });
        }
    }

    /// <summary>
    /// Delete a lookup category
    /// </summary>
    [HttpDelete("lookup-categories/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteLookupCategory(int id)
    {
        try
        {
            var category = await _context.LookupCategories.FindAsync(id);
            if (category == null) return NotFound();

            _context.LookupCategories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lookup category");
            return StatusCode(500, new { message = "Error deleting lookup category" });
        }
    }

    #endregion

    #region Lookup Items

    /// <summary>
    /// Create a new lookup item
    /// </summary>
    [HttpPost("lookup-items")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LookupItemDto>> CreateLookupItem([FromBody] CreateLookupItemRequest request)
    {
        try
        {
            var item = new LookupItem
            {
                LookupCategoryId = request.CategoryId,
                Key = request.Key,
                Value = request.Value,
                Meta = request.Meta,
                SortOrder = request.SortOrder ?? 0,
                IsActive = true,
                ParentItemId = request.ParentItemId
            };

            _context.LookupItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new LookupItemDto
            {
                Id = item.Id,
                Key = item.Key,
                Value = item.Value,
                Meta = item.Meta,
                SortOrder = item.SortOrder,
                IsActive = item.IsActive,
                ParentItemId = item.ParentItemId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lookup item");
            return StatusCode(500, new { message = "Error creating lookup item" });
        }
    }

    /// <summary>
    /// Update a lookup item
    /// </summary>
    [HttpPut("lookup-items/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateLookupItem(int id, [FromBody] UpdateLookupItemRequest request)
    {
        try
        {
            var item = await _context.LookupItems.FindAsync(id);
            if (item == null) return NotFound();

            item.Key = request.Key ?? item.Key;
            item.Value = request.Value ?? item.Value;
            item.Meta = request.Meta ?? item.Meta;
            item.SortOrder = request.SortOrder ?? item.SortOrder;
            item.IsActive = request.IsActive ?? item.IsActive;
            item.ParentItemId = request.ParentItemId;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Item updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lookup item");
            return StatusCode(500, new { message = "Error updating lookup item" });
        }
    }

    /// <summary>
    /// Delete a lookup item
    /// </summary>
    [HttpDelete("lookup-items/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteLookupItem(int id)
    {
        try
        {
            var item = await _context.LookupItems.FindAsync(id);
            if (item == null) return NotFound();

            _context.LookupItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Item deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lookup item");
            return StatusCode(500, new { message = "Error deleting lookup item" });
        }
    }

    #endregion

    #region Color Palettes

    /// <summary>
    /// Get all color palettes
    /// </summary>
    [HttpGet("color-palettes")]
    public async Task<ActionResult<List<ColorPaletteDto>>> GetColorPalettes([FromQuery] string? search = null)
    {
        try
        {
            var query = _context.ColorPalettes.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search) || 
                                        (p.Category != null && p.Category.ToLower().Contains(search)));
            }

            var palettes = await query.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();

            return Ok(palettes.Select(p => new ColorPaletteDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Colors = JsonSerializer.Deserialize<List<string>>(p.Colors) ?? new List<string>(),
                IsUserDefined = p.IsUserDefined
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting color palettes");
            return StatusCode(500, new { message = "Error getting color palettes" });
        }
    }

    /// <summary>
    /// Create a new color palette
    /// </summary>
    [HttpPost("color-palettes")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ColorPaletteDto>> CreateColorPalette([FromBody] CreateColorPaletteRequest request)
    {
        try
        {
            var palette = new ColorPalette
            {
                Name = request.Name,
                Category = request.Category,
                Colors = JsonSerializer.Serialize(request.Colors),
                IsUserDefined = true
            };

            _context.ColorPalettes.Add(palette);
            await _context.SaveChangesAsync();

            return Ok(new ColorPaletteDto
            {
                Id = palette.Id,
                Name = palette.Name,
                Category = palette.Category,
                Colors = request.Colors,
                IsUserDefined = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating color palette");
            return StatusCode(500, new { message = "Error creating color palette" });
        }
    }

    /// <summary>
    /// Delete a color palette
    /// </summary>
    [HttpDelete("color-palettes/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteColorPalette(int id)
    {
        try
        {
            var palette = await _context.ColorPalettes.FindAsync(id);
            if (palette == null) return NotFound();

            _context.ColorPalettes.Remove(palette);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Palette deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting color palette");
            return StatusCode(500, new { message = "Error deleting color palette" });
        }
    }

    #endregion

    #region ZIP Codes

    /// <summary>
    /// Search ZIP codes with pagination
    /// </summary>
    [HttpGet("zipcodes")]
    public async Task<ActionResult<PagedResult<ZipCodeDto>>> GetZipCodes(
        [FromQuery] string? search = null,
        [FromQuery] string? country = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.ZipCodes.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(z => z.PostalCode.ToLower().Contains(search) || 
                                        z.City.ToLower().Contains(search) ||
                                        (z.State != null && z.State.ToLower().Contains(search)));
            }
            
            if (!string.IsNullOrWhiteSpace(country))
            {
                query = query.Where(z => z.CountryCode == country.ToUpper());
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(z => z.CountryCode)
                .ThenBy(z => z.PostalCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<ZipCodeDto>
            {
                Items = items.Select(z => new ZipCodeDto
                {
                    Id = z.Id,
                    PostalCode = z.PostalCode,
                    City = z.City,
                    State = z.State,
                    StateCode = z.StateCode,
                    County = z.County,
                    CountryCode = z.CountryCode,
                    Latitude = z.Latitude,
                    Longitude = z.Longitude
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ZIP codes");
            return StatusCode(500, new { message = "Error getting ZIP codes" });
        }
    }

    /// <summary>
    /// Get unique countries in ZIP code database
    /// </summary>
    [HttpGet("zipcodes/countries")]
    public async Task<ActionResult<List<string>>> GetZipCodeCountries()
    {
        try
        {
            var countries = await _context.ZipCodes
                .Select(z => z.CountryCode)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            return Ok(countries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ZIP code countries");
            return StatusCode(500, new { message = "Error getting ZIP code countries" });
        }
    }

    #endregion

    #region Export/Import

    /// <summary>
    /// Export master data as JSON
    /// </summary>
    [HttpGet("export/{dataType}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportData(string dataType, [FromQuery] string? format = "json")
    {
        try
        {
            object data = dataType.ToLower() switch
            {
                "lookup-categories" => await _context.LookupCategories.Include(c => c.Items).ToListAsync(),
                "color-palettes" => await _context.ColorPalettes.ToListAsync(),
                "service-request-categories" => await _context.ServiceRequestCategories.ToListAsync(),
                "service-request-types" => await _context.ServiceRequestTypes.ToListAsync(),
                _ => throw new ArgumentException($"Unknown data type: {dataType}")
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"{dataType}-export-{DateTime.UtcNow:yyyyMMdd}.json");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting data");
            return StatusCode(500, new { message = "Error exporting data" });
        }
    }

    #endregion
}

#region DTOs

public class MasterDataOverviewDto
{
    public int LookupCategoriesCount { get; set; }
    public int LookupItemsCount { get; set; }
    public int ColorPalettesCount { get; set; }
    public int ZipCodesCount { get; set; }
    public int ServiceRequestCategoriesCount { get; set; }
    public int ServiceRequestTypesCount { get; set; }
    public List<MasterDataTypeInfo> DataTypes { get; set; } = new();
}

public class MasterDataTypeInfo
{
    public string Name { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool CanImportExport { get; set; }
}

public class LookupCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ItemCount { get; set; }
    public List<LookupItemDto> Items { get; set; } = new();
}

public class LookupItemDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Meta { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int? ParentItemId { get; set; }
}

public class CreateLookupCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateLookupCategoryRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateLookupItemRequest
{
    public int CategoryId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Meta { get; set; }
    public int? SortOrder { get; set; }
    public int? ParentItemId { get; set; }
}

public class UpdateLookupItemRequest
{
    public string? Key { get; set; }
    public string? Value { get; set; }
    public string? Meta { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
    public int? ParentItemId { get; set; }
}

public class ColorPaletteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public List<string> Colors { get; set; } = new();
    public bool IsUserDefined { get; set; }
}

public class CreateColorPaletteRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public List<string> Colors { get; set; } = new();
}

public class ZipCodeDto
{
    public int Id { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? StateCode { get; set; }
    public string? County { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

#endregion
