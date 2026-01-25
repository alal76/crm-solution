using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
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
    private readonly IMasterDataSeederService _masterDataSeeder;

    public MasterDataController(
        ICrmDbContext context, 
        CrmDbContext dbContext, 
        ILogger<MasterDataController> logger,
        IMasterDataSeederService masterDataSeeder)
    {
        _context = context;
        _dbContext = dbContext;
        _logger = logger;
        _masterDataSeeder = masterDataSeeder;
    }

    /// <summary>
    /// Get master data seeding status (ZipCodes, ColorPalettes)
    /// </summary>
    [HttpGet("seed-status")]
    public async Task<IActionResult> GetSeedStatus()
    {
        try
        {
            var stats = await _masterDataSeeder.GetStatsAsync();
            return Ok(new
            {
                zipCodes = new
                {
                    count = stats.ZipCodeCount,
                    populated = stats.ZipCodesPopulated
                },
                colorPalettes = new
                {
                    count = stats.ColorPaletteCount,
                    populated = stats.ColorPalettesPopulated
                },
                allPopulated = stats.ZipCodesPopulated && stats.ColorPalettesPopulated,
                message = stats.ZipCodesPopulated && stats.ColorPalettesPopulated 
                    ? "All master data is populated and will persist across deployments." 
                    : "Some master data needs to be seeded. Data persists in the database across deployments."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seed status");
            return StatusCode(500, new { message = "Error getting seed status" });
        }
    }

    /// <summary>
    /// Seed master data (ZipCodes, ColorPalettes) if not already populated
    /// </summary>
    [HttpPost("seed")]
    [Authorize(Roles = "Admin,SysAdmin")]
    public async Task<IActionResult> SeedMasterData()
    {
        try
        {
            var beforeStats = await _masterDataSeeder.GetStatsAsync();
            await _masterDataSeeder.SeedIfEmptyAsync();
            var afterStats = await _masterDataSeeder.GetStatsAsync();

            return Ok(new
            {
                message = "Master data seeding completed",
                before = new { zipCodes = beforeStats.ZipCodeCount, colorPalettes = beforeStats.ColorPaletteCount },
                after = new { zipCodes = afterStats.ZipCodeCount, colorPalettes = afterStats.ColorPaletteCount },
                note = "Data is cached in the database and persists across deployments."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding master data");
            return StatusCode(500, new { message = "Error seeding master data", error = ex.Message });
        }
    }

    /// <summary>
    /// Force reseed all master data (clears existing and re-populates)
    /// WARNING: This will delete all existing master data
    /// </summary>
    [HttpPost("reseed")]
    [Authorize(Roles = "Admin,SysAdmin")]
    public async Task<IActionResult> ReseedMasterData()
    {
        try
        {
            var beforeStats = await _masterDataSeeder.GetStatsAsync();
            await _masterDataSeeder.ReseedAllAsync();
            var afterStats = await _masterDataSeeder.GetStatsAsync();

            return Ok(new
            {
                message = "Master data reseeded successfully",
                cleared = new { zipCodes = beforeStats.ZipCodeCount, colorPalettes = beforeStats.ColorPaletteCount },
                seeded = new { zipCodes = afterStats.ZipCodeCount, colorPalettes = afterStats.ColorPaletteCount },
                note = "Data is cached in the database and persists across deployments."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reseeding master data");
            return StatusCode(500, new { message = "Error reseeding master data", error = ex.Message });
        }
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

    #region ZIP Codes

    /// <summary>
    /// Get paginated ZIP codes for the Master Data management UI
    /// Optimized for large datasets with proper indexing hints
    /// </summary>
    [HttpGet("zipcodes")]
    public async Task<IActionResult> GetZipCodes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? country = null)
    {
        try
        {
            // Limit page size to prevent excessive data transfer
            pageSize = Math.Min(pageSize, 100);
            
            var query = _dbContext.ZipCodes.AsNoTracking().AsQueryable();

            // Apply country filter first (most selective)
            if (!string.IsNullOrWhiteSpace(country))
            {
                query = query.Where(z => z.CountryCode == country);
            }

            // Apply search filter - optimized for indexed columns
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                
                // For postal code searches, use StartsWith for better index usage
                if (searchTerm.Length <= 10 && searchTerm.All(c => char.IsLetterOrDigit(c) || c == '-' || c == ' '))
                {
                    // Likely a postal code search - use prefix matching
                    query = query.Where(z => 
                        z.PostalCode.StartsWith(searchTerm) ||
                        z.City.StartsWith(searchTerm) ||
                        (z.State != null && z.State.StartsWith(searchTerm)) ||
                        (z.StateCode != null && z.StateCode == searchTerm));
                }
                else
                {
                    // Full text search for longer terms
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(z => 
                        z.PostalCode.ToLower().Contains(searchLower) ||
                        z.City.ToLower().Contains(searchLower) ||
                        (z.State != null && z.State.ToLower().Contains(searchLower)));
                }
            }

            // Get total count using a more efficient approach
            var totalCount = await query.CountAsync();
            
            // Use efficient ordering based on filters applied
            IOrderedQueryable<ZipCode> orderedQuery;
            if (!string.IsNullOrWhiteSpace(search))
            {
                // When searching, order by relevance (postal code match first)
                orderedQuery = query.OrderBy(z => z.PostalCode);
            }
            else if (!string.IsNullOrWhiteSpace(country))
            {
                // When filtering by country, order by state then city
                orderedQuery = query.OrderBy(z => z.State).ThenBy(z => z.City).ThenBy(z => z.PostalCode);
            }
            else
            {
                // Default: order by country, state, city
                orderedQuery = query.OrderBy(z => z.CountryCode).ThenBy(z => z.State).ThenBy(z => z.City);
            }
            
            var items = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(z => new
                {
                    z.Id,
                    z.PostalCode,
                    z.City,
                    z.State,
                    z.StateCode,
                    z.County,
                    z.CountryCode,
                    z.Latitude,
                    z.Longitude
                })
                .ToListAsync();

            return Ok(new { items, totalCount, page, pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ZIP codes");
            return StatusCode(500, new { message = "Error getting ZIP codes" });
        }
    }

    /// <summary>
    /// Fast global ZIP code search - returns top matches quickly
    /// </summary>
    [HttpGet("zipcodes/search")]
    public async Task<IActionResult> SearchZipCodes(
        [FromQuery] string q,
        [FromQuery] int limit = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Ok(new { items = Array.Empty<object>(), message = "Enter at least 2 characters" });
            }

            limit = Math.Min(limit, 50); // Cap at 50 results
            var searchTerm = q.Trim();
            
            var query = _dbContext.ZipCodes.AsNoTracking();

            // Optimized search using StartsWith for better index usage
            var items = await query
                .Where(z => 
                    z.PostalCode.StartsWith(searchTerm) ||
                    z.City.StartsWith(searchTerm))
                .OrderBy(z => z.PostalCode.StartsWith(searchTerm) ? 0 : 1)
                .ThenBy(z => z.PostalCode)
                .Take(limit)
                .Select(z => new
                {
                    z.Id,
                    z.PostalCode,
                    z.City,
                    z.State,
                    z.StateCode,
                    z.CountryCode,
                    display = $"{z.PostalCode} - {z.City}, {z.State ?? ""} ({z.CountryCode})"
                })
                .ToListAsync();

            return Ok(new { items, count = items.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching ZIP codes");
            return StatusCode(500, new { message = "Error searching ZIP codes" });
        }
    }

    /// <summary>
    /// Get list of distinct country codes that have ZIP codes
    /// </summary>
    [HttpGet("zipcodes/countries")]
    public async Task<IActionResult> GetZipCodeCountries()
    {
        try
        {
            var countries = await _dbContext.ZipCodes
                .Select(z => z.CountryCode)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(countries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ZIP code countries");
            return StatusCode(500, new { message = "Error getting countries" });
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
