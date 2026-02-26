// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    private readonly ISystemSettingsService _settingsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MasterDataController"/> class.
    /// </summary>
    /// <param name="context">The CRM database context interface.</param>
    /// <param name="dbContext">The CRM database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="masterDataSeeder">The master data seeder service.</param>
    /// <param name="settingsService">The system settings service.</param>
    public MasterDataController(
        ICrmDbContext context,
        CrmDbContext dbContext,
        ILogger<MasterDataController> logger,
        IMasterDataSeederService masterDataSeeder,
        ISystemSettingsService settingsService)
    {
        _context = context;
        _dbContext = dbContext;
        _logger = logger;
        _masterDataSeeder = masterDataSeeder;
        _settingsService = settingsService;
    }

    /// <summary>
    /// Get master data seeding status (ZipCodes, ColorPalettes)
    /// </summary>
    [HttpGet("seed-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
                currencies = new
                {
                    count = stats.CurrencyCount,
                    populated = stats.CurrenciesPopulated
                },
                timezones = new
                {
                    count = stats.TimezoneCount,
                    populated = stats.TimezonesPopulated
                },
                allPopulated = stats.ZipCodesPopulated && stats.ColorPalettesPopulated && stats.CurrenciesPopulated && stats.TimezonesPopulated,
                message = stats.ZipCodesPopulated && stats.ColorPalettesPopulated && stats.CurrenciesPopulated && stats.TimezonesPopulated
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        try
        {
            var zipCodesCount = 0;
            try
            {
                zipCodesCount = await _dbContext.ZipCodes.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to count ZipCodes - table may not exist yet");
            }

            var serviceRequestCategoriesCount = 0;
            try
            {
                serviceRequestCategoriesCount = await _context.ServiceRequestCategories.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to count ServiceRequestCategories - table may not exist yet");
            }

            var serviceRequestTypesCount = 0;
            try
            {
                serviceRequestTypesCount = await _context.ServiceRequestTypes.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to count ServiceRequestTypes - table may not exist yet");
            }

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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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

    #region Branding & Company Profile

    /// <summary>
    /// Get company branding and customization settings
    /// </summary>
    [HttpGet("branding")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranding()
    {
        try
        {
            var settings = await _dbContext.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
            if (settings == null) return Ok(new { });
            return Ok(new
            {
                // Company Identity
                companyName = settings.CompanyName,
                companyFullName = settings.CompanyFullName,
                companyLegalName = settings.CompanyLegalName,
                companyWebsite = settings.CompanyWebsite,
                companyEmail = settings.CompanyEmail,
                companyPhone = settings.CompanyPhone,
                companyTaxId = settings.CompanyTaxId,
                companyRegistrationNumber = settings.CompanyRegistrationNumber,
                companyIndustry = settings.CompanyIndustry,
                companyDescription = settings.CompanyDescription,
                // Logos
                companyLogoUrl = settings.CompanyLogoUrl,
                companyLoginLogoUrl = settings.CompanyLoginLogoUrl,
                // Theme
                primaryColor = settings.PrimaryColor,
                secondaryColor = settings.SecondaryColor,
                tertiaryColor = settings.TertiaryColor,
                surfaceColor = settings.SurfaceColor,
                backgroundColor = settings.BackgroundColor,
                selectedPaletteId = settings.SelectedPaletteId,
                selectedPaletteName = settings.SelectedPaletteName,
                // Customization / Localization
                defaultCurrency = settings.DefaultCurrency,
                defaultTimezone = settings.DefaultTimezone,
                defaultLanguage = settings.DefaultLanguage,
                dateFormat = settings.DateFormat,
                timeFormat = settings.TimeFormat,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branding settings");
            return StatusCode(500, new { message = "Error getting branding settings" });
        }
    }

    /// <summary>
    /// Update company branding and customization settings
    /// </summary>
    [HttpPut("branding")]
    [Authorize(Roles = "Admin,SysAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateBranding([FromBody] UpdateBrandingRequest request)
    {
        try
        {
            var settings = await _dbContext.SystemSettings.FirstOrDefaultAsync();
            if (settings == null) return NotFound(new { message = "System settings not found" });

            // Update company identity fields (not in UpdateSystemSettingsRequest)
            if (request.CompanyFullName != null) settings.CompanyFullName = request.CompanyFullName;
            if (request.CompanyLegalName != null) settings.CompanyLegalName = request.CompanyLegalName;
            if (request.CompanyWebsite != null) settings.CompanyWebsite = request.CompanyWebsite;
            if (request.CompanyEmail != null) settings.CompanyEmail = request.CompanyEmail;
            if (request.CompanyPhone != null) settings.CompanyPhone = request.CompanyPhone;
            if (request.CompanyTaxId != null) settings.CompanyTaxId = request.CompanyTaxId;
            if (request.CompanyRegistrationNumber != null) settings.CompanyRegistrationNumber = request.CompanyRegistrationNumber;
            if (request.CompanyIndustry != null) settings.CompanyIndustry = request.CompanyIndustry;
            if (request.CompanyDescription != null) settings.CompanyDescription = request.CompanyDescription;
            settings.LastModified = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Delegate standard settings (name, logo, colors, currency, timezone) to settings service
            var stdRequest = new UpdateSystemSettingsRequest
            {
                CompanyName = request.CompanyName,
                CompanyLogoUrl = request.CompanyLogoUrl,
                CompanyLoginLogoUrl = request.CompanyLoginLogoUrl,
                PrimaryColor = request.PrimaryColor,
                SecondaryColor = request.SecondaryColor,
                TertiaryColor = request.TertiaryColor,
                SurfaceColor = request.SurfaceColor,
                BackgroundColor = request.BackgroundColor,
                DefaultCurrency = request.DefaultCurrency,
                DefaultTimezone = request.DefaultTimezone,
                DefaultLanguage = request.DefaultLanguage,
                DateFormat = request.DateFormat,
                TimeFormat = request.TimeFormat,
            };
            await _settingsService.UpdateSettingsAsync(stdRequest);

            return Ok(new { message = "Branding updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branding settings");
            return StatusCode(500, new { message = "Error updating branding settings", error = ex.Message });
        }
    }

    #endregion

    #region Currencies & Timezones

    /// <summary>
    /// Get all available currencies (from LookupCategory "Currencies")
    /// </summary>
    [HttpGet("currencies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrencies([FromQuery] string? search = null)
    {
        try
        {
            var category = await _context.LookupCategories
                .Include(c => c.Items.OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(c => c.Name == "Currencies");

            if (category == null || category.Items.Count == 0)
            {
                return Ok(new { items = Array.Empty<object>(), seeded = false, message = "Currencies not yet seeded. POST /api/masterdata/seed/currencies to seed." });
            }

            var items = category.Items.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                items = items.Where(i => i.Key.ToLower().Contains(term) || i.Value.ToLower().Contains(term));
            }

            // Get default currency from settings
            var settings = await _dbContext.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
            var defaultCurrency = settings?.DefaultCurrency ?? "USD";

            return Ok(new
            {
                items = items.Select(i =>
                {
                    var meta = i.Meta != null ? JsonSerializer.Deserialize<JsonElement>(i.Meta) : default;
                    return new
                    {
                        code = i.Key,
                        name = i.Value,
                        symbol = meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty("symbol", out var sym) ? sym.GetString() : "",
                        numericCode = meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty("numericCode", out var nc) ? nc.GetString() : "",
                        isDefault = i.Key == defaultCurrency
                    };
                }).ToList(),
                defaultCurrency,
                seeded = true,
                totalCount = category.Items.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting currencies");
            return StatusCode(500, new { message = "Error getting currencies" });
        }
    }

    /// <summary>
    /// Set the default currency for the system
    /// </summary>
    [HttpPut("currencies/default")]
    [Authorize(Roles = "Admin,SysAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDefaultCurrency([FromBody] SetDefaultValueRequest request)
    {
        try
        {
            await _settingsService.UpdateSettingsAsync(new UpdateSystemSettingsRequest { DefaultCurrency = request.Value });
            return Ok(new { message = $"Default currency set to {request.Value}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default currency");
            return StatusCode(500, new { message = "Error setting default currency" });
        }
    }

    /// <summary>
    /// Get all available timezones (from LookupCategory "Timezones")
    /// </summary>
    [HttpGet("timezones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimezones([FromQuery] string? search = null, [FromQuery] string? region = null)
    {
        try
        {
            var category = await _context.LookupCategories
                .Include(c => c.Items.OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(c => c.Name == "Timezones");

            if (category == null || category.Items.Count == 0)
            {
                return Ok(new { items = Array.Empty<object>(), seeded = false, message = "Timezones not yet seeded. POST /api/masterdata/seed/timezones to seed." });
            }

            var items = category.Items.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(region))
            {
                var regionLower = region.ToLower();
                items = items.Where(i =>
                {
                    if (i.Meta == null) return false;
                    try { var m = JsonSerializer.Deserialize<JsonElement>(i.Meta); return m.TryGetProperty("region", out var r) && (r.GetString() ?? "").ToLower() == regionLower; }
                    catch { return false; }
                });
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                items = items.Where(i => i.Key.ToLower().Contains(term) || i.Value.ToLower().Contains(term));
            }

            var settings = await _dbContext.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
            var defaultTz = settings?.DefaultTimezone ?? "America/New_York";

            return Ok(new
            {
                items = items.Select(i =>
                {
                    var meta = i.Meta != null ? JsonSerializer.Deserialize<JsonElement>(i.Meta) : default;
                    return new
                    {
                        ianaId = i.Key,
                        displayName = i.Value,
                        utcOffset = meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty("utcOffset", out var off) ? off.GetString() : "",
                        region = meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty("region", out var rgn) ? rgn.GetString() : "",
                        isDefault = i.Key == defaultTz
                    };
                }).ToList(),
                defaultTimezone = defaultTz,
                seeded = true,
                totalCount = category.Items.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timezones");
            return StatusCode(500, new { message = "Error getting timezones" });
        }
    }

    /// <summary>
    /// Set the default timezone for the system
    /// </summary>
    [HttpPut("timezones/default")]
    [Authorize(Roles = "Admin,SysAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDefaultTimezone([FromBody] SetDefaultValueRequest request)
    {
        try
        {
            await _settingsService.UpdateSettingsAsync(new UpdateSystemSettingsRequest { DefaultTimezone = request.Value });
            return Ok(new { message = $"Default timezone set to {request.Value}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default timezone");
            return StatusCode(500, new { message = "Error setting default timezone" });
        }
    }

    /// <summary>
    /// Seed currencies into the Currencies lookup category
    /// </summary>
    [HttpPost("seed/currencies")]
    [Authorize(Roles = "Admin,SysAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedCurrencies([FromQuery] bool force = false)
    {
        try
        {
            await _masterDataSeeder.SeedCurrenciesAsync(force);
            var stats = await _masterDataSeeder.GetStatsAsync();
            return Ok(new { message = $"Currencies seeded. Total: {stats.CurrencyCount}", count = stats.CurrencyCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding currencies");
            return StatusCode(500, new { message = "Error seeding currencies", error = ex.Message });
        }
    }

    /// <summary>
    /// Seed timezones into the Timezones lookup category
    /// </summary>
    [HttpPost("seed/timezones")]
    [Authorize(Roles = "Admin,SysAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedTimezones([FromQuery] bool force = false)
    {
        try
        {
            await _masterDataSeeder.SeedTimeZonesAsync(force);
            var stats = await _masterDataSeeder.GetStatsAsync();
            return Ok(new { message = $"Timezones seeded. Total: {stats.TimezoneCount}", count = stats.TimezoneCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding timezones");
            return StatusCode(500, new { message = "Error seeding timezones", error = ex.Message });
        }
    }

    #endregion

    #region Export
    [HttpGet("export/{dataType}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

/// <summary>
/// Request DTO for updating company branding and customization settings.
/// </summary>
public class UpdateBrandingRequest
{
    // Company Identity
    public string? CompanyName { get; set; }
    public string? CompanyFullName { get; set; }
    public string? CompanyLegalName { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyTaxId { get; set; }
    public string? CompanyRegistrationNumber { get; set; }
    public string? CompanyIndustry { get; set; }
    public string? CompanyDescription { get; set; }
    // Logos
    public string? CompanyLogoUrl { get; set; }
    public string? CompanyLoginLogoUrl { get; set; }
    // Theme
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? TertiaryColor { get; set; }
    public string? SurfaceColor { get; set; }
    public string? BackgroundColor { get; set; }
    // Localization
    public string? DefaultCurrency { get; set; }
    public string? DefaultTimezone { get; set; }
    public string? DefaultLanguage { get; set; }
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
}

/// <summary>
/// Simple request DTO for setting a single default value.
/// </summary>
public class SetDefaultValueRequest
{
    /// <summary>Gets or sets the value to set as default (e.g. currency code "USD" or timezone IANA id).</summary>
    public string Value { get; set; } = string.Empty;
}

#endregion
