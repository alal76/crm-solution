using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing master data (Lookups, Color Palettes)
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MasterDataController : ControllerBase
{
    private readonly ICrmDbContext _context;
    private readonly CrmDbContext _dbContext;
    private readonly ILogger<MasterDataController> _logger;

    public MasterDataController(ICrmDbContext context, CrmDbContext dbContext, ILogger<MasterDataController> logger)
    {
        _context = context;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get overview of all master data
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        try
        {
            var zipCodesCount = 0;
            try { zipCodesCount = await _dbContext.ZipCodes.CountAsync(); } catch { }
            
            var serviceRequestCategoriesCount = 0;
            try { serviceRequestCategoriesCount = await _context.ServiceRequestCategories.CountAsync(); } catch { }
            
            var serviceRequestTypesCount = 0;
            try { serviceRequestTypesCount = await _context.ServiceRequestTypes.CountAsync(); } catch { }

            var overview = new
            {
                lookupCategoriesCount = await _context.LookupCategories.CountAsync(),
                lookupItemsCount = await _context.LookupItems.CountAsync(),
                colorPalettesCount = await _dbContext.ColorPalettes.CountAsync(),
                zipCodesCount = zipCodesCount,
                serviceRequestCategoriesCount = serviceRequestCategoriesCount,
                serviceRequestTypesCount = serviceRequestTypesCount,
                dataTypes = new[]
                {
                    new { name = "Lookup Categories", tableName = "LookupCategories", count = await _context.LookupCategories.CountAsync(), canImportExport = true },
                    new { name = "Lookup Items", tableName = "LookupItems", count = await _context.LookupItems.CountAsync(), canImportExport = true },
                    new { name = "Color Palettes", tableName = "ColorPalettes", count = await _dbContext.ColorPalettes.CountAsync(), canImportExport = true },
                    new { name = "ZIP Codes", tableName = "ZipCodes", count = zipCodesCount, canImportExport = true },
                    new { name = "Service Categories", tableName = "ServiceRequestCategories", count = serviceRequestCategoriesCount, canImportExport = false },
                    new { name = "Service Types", tableName = "ServiceRequestTypes", count = serviceRequestTypesCount, canImportExport = false }
                }
            };

            return Ok(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting master data overview");
            return StatusCode(500, new { message = "Error getting overview" });
        }
    }

    #region Lookup Categories

    /// <summary>
    /// Get all lookup categories with their items
    /// </summary>
    [HttpGet("lookup-categories")]
    public async Task<IActionResult> GetLookupCategories()
    {
        try
        {
            var categories = await _context.LookupCategories
                .Include(c => c.Items)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
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
    public async Task<IActionResult> CreateLookupCategory([FromBody] CreateLookupCategoryRequest request)
    {
        try
        {
            var category = new LookupCategory
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive ?? true
            };

            _context.LookupCategories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(category);
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
    public async Task<IActionResult> UpdateLookupCategory(int id, [FromBody] CreateLookupCategoryRequest request)
    {
        try
        {
            var category = await _context.LookupCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            category.Name = request.Name;
            category.Description = request.Description;
            category.IsActive = request.IsActive ?? category.IsActive;

            await _context.SaveChangesAsync();

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lookup category {Id}", id);
            return StatusCode(500, new { message = "Error updating lookup category" });
        }
    }

    /// <summary>
    /// Delete a lookup category
    /// </summary>
    [HttpDelete("lookup-categories/{id}")]
    public async Task<IActionResult> DeleteLookupCategory(int id)
    {
        try
        {
            var category = await _context.LookupCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            _context.LookupCategories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lookup category {Id}", id);
            return StatusCode(500, new { message = "Error deleting lookup category" });
        }
    }

    #endregion

    #region Lookup Items

    /// <summary>
    /// Create a new lookup item
    /// </summary>
    [HttpPost("lookup-items")]
    public async Task<IActionResult> CreateLookupItem([FromBody] CreateLookupItemRequest request)
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
                IsActive = request.IsActive ?? true
            };

            _context.LookupItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(item);
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
    public async Task<IActionResult> UpdateLookupItem(int id, [FromBody] CreateLookupItemRequest request)
    {
        try
        {
            var item = await _context.LookupItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }

            item.Key = request.Key;
            item.Value = request.Value;
            item.Meta = request.Meta;
            item.SortOrder = request.SortOrder ?? item.SortOrder;
            item.IsActive = request.IsActive ?? item.IsActive;

            await _context.SaveChangesAsync();

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lookup item {Id}", id);
            return StatusCode(500, new { message = "Error updating lookup item" });
        }
    }

    /// <summary>
    /// Delete a lookup item
    /// </summary>
    [HttpDelete("lookup-items/{id}")]
    public async Task<IActionResult> DeleteLookupItem(int id)
    {
        try
        {
            var item = await _context.LookupItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }

            _context.LookupItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lookup item {Id}", id);
            return StatusCode(500, new { message = "Error deleting lookup item" });
        }
    }

    #endregion

    #region Color Palettes

    /// <summary>
    /// Get all color palettes
    /// </summary>
    [HttpGet("color-palettes")]
    public async Task<IActionResult> GetColorPalettes()
    {
        try
        {
            var palettes = await _dbContext.ColorPalettes
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return Ok(palettes);
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
    public async Task<IActionResult> CreateColorPalette([FromBody] CreateColorPaletteRequest request)
    {
        try
        {
            var palette = new ColorPalette
            {
                Name = request.Name,
                Category = request.Category,
                Color1 = request.Color1 ?? "#000000",
                Color2 = request.Color2 ?? "#333333",
                Color3 = request.Color3 ?? "#666666",
                Color4 = request.Color4 ?? "#999999",
                Color5 = request.Color5 ?? "#CCCCCC",
                IsUserDefined = true
            };

            _dbContext.ColorPalettes.Add(palette);
            await _dbContext.SaveChangesAsync();

            return Ok(palette);
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
    public async Task<IActionResult> DeleteColorPalette(int id)
    {
        try
        {
            var palette = await _dbContext.ColorPalettes.FindAsync(id);
            if (palette == null)
            {
                return NotFound(new { message = "Palette not found" });
            }

            _dbContext.ColorPalettes.Remove(palette);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Palette deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting color palette {Id}", id);
            return StatusCode(500, new { message = "Error deleting color palette" });
        }
    }

    #endregion

    #region Export

    /// <summary>
    /// Export master data
    /// </summary>
    [HttpGet("export/{dataType}")]
    public async Task<IActionResult> ExportData(string dataType)
    {
        try
        {
            object? data = dataType.ToLowerInvariant() switch
            {
                "lookup-categories" => await _context.LookupCategories.Include(c => c.Items).ToListAsync(),
                "lookup-items" => await _context.LookupItems.Include(i => i.Category).ToListAsync(),
                "color-palettes" => await _dbContext.ColorPalettes.ToListAsync(),
                _ => null
            };

            if (data == null)
            {
                return BadRequest(new { message = $"Unknown data type: {dataType}" });
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
            var json = JsonSerializer.Serialize(data, options);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            return File(jsonBytes, "application/json", $"{dataType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting {DataType}", dataType);
            return StatusCode(500, new { message = $"Error exporting data: {ex.Message}" });
        }
    }

    #endregion
}

#region Request DTOs

public class CreateLookupCategoryRequest
{
    public string Name { get; set; } = string.Empty;
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
    public bool? IsActive { get; set; }
}

public class CreateColorPaletteRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Color1 { get; set; }
    public string? Color2 { get; set; }
    public string? Color3 { get; set; }
    public string? Color4 { get; set; }
    public string? Color5 { get; set; }
}

#endregion
