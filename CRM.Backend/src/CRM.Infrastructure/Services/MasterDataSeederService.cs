/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for seeding and caching master data (ZipCodes, ColorPalettes) in the database.
/// This data persists across deployments as it's stored in the database.
/// If tables are empty, the service will seed default data on startup.
/// </summary>
public interface IMasterDataSeederService
{
    /// <summary>
    /// Seeds master data if not already populated.
    /// Should be called during application startup.
    /// </summary>
    Task SeedIfEmptyAsync();
    
    /// <summary>
    /// Force re-seed all master data (clears existing and re-populates)
    /// </summary>
    Task ReseedAllAsync();
    
    /// <summary>
    /// Get the count of seeded master data records
    /// </summary>
    Task<MasterDataStats> GetStatsAsync();
}

public class MasterDataStats
{
    public int ZipCodeCount { get; set; }
    public int ColorPaletteCount { get; set; }
    public bool ZipCodesPopulated => ZipCodeCount > 0;
    public bool ColorPalettesPopulated => ColorPaletteCount > 0;
    public DateTime? LastSeededAt { get; set; }
}

public class MasterDataSeederService : IMasterDataSeederService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<MasterDataSeederService> _logger;

    public MasterDataSeederService(CrmDbContext context, ILogger<MasterDataSeederService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SeedIfEmptyAsync()
    {
        try
        {
            // Check and seed ColorPalettes if empty
            var colorPaletteCount = await _context.ColorPalettes.CountAsync();
            if (colorPaletteCount == 0)
            {
                _logger.LogInformation("ColorPalettes table is empty. Seeding default color palettes...");
                await SeedColorPalettesAsync();
            }
            else
            {
                _logger.LogInformation("ColorPalettes table has {Count} records. Skipping seed.", colorPaletteCount);
            }

            // Check and seed ZipCodes if empty
            var zipCodeCount = await _context.ZipCodes.CountAsync();
            if (zipCodeCount == 0)
            {
                _logger.LogInformation("ZipCodes table is empty. Seeding sample ZIP codes...");
                await SeedZipCodesAsync();
            }
            else
            {
                _logger.LogInformation("ZipCodes table has {Count} records. Skipping seed.", zipCodeCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during master data seeding");
        }
    }

    /// <inheritdoc />
    public async Task ReseedAllAsync()
    {
        try
        {
            _logger.LogWarning("Force re-seeding all master data...");

            // Clear and reseed ColorPalettes
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ColorPalettes WHERE IsDeleted = 0");
            await SeedColorPalettesAsync();

            // Note: ZipCodes is large, only reseed if explicitly requested
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ZipCodes WHERE IsActive = 1");
            await SeedZipCodesAsync();

            _logger.LogInformation("Master data re-seeding completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during master data re-seeding");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<MasterDataStats> GetStatsAsync()
    {
        return new MasterDataStats
        {
            ZipCodeCount = await _context.ZipCodes.CountAsync(),
            ColorPaletteCount = await _context.ColorPalettes.CountAsync()
        };
    }

    /// <summary>
    /// Seeds color palettes into the database
    /// </summary>
    private async Task SeedColorPalettesAsync()
    {
        var palettes = GetDefaultColorPalettes();
        _context.ColorPalettes.AddRange(palettes);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} color palettes.", palettes.Count);
    }

    /// <summary>
    /// Seeds ZIP codes into the database
    /// </summary>
    private async Task SeedZipCodesAsync()
    {
        var zipCodes = GetDefaultZipCodes();
        
        // Batch insert for performance
        const int batchSize = 1000;
        for (int i = 0; i < zipCodes.Count; i += batchSize)
        {
            var batch = zipCodes.Skip(i).Take(batchSize).ToList();
            _context.ZipCodes.AddRange(batch);
            await _context.SaveChangesAsync();
            _logger.LogDebug("Inserted ZIP code batch {Batch}/{Total}", i / batchSize + 1, (zipCodes.Count + batchSize - 1) / batchSize);
        }
        
        _logger.LogInformation("Seeded {Count} ZIP codes.", zipCodes.Count);
    }

    /// <summary>
    /// Returns the default color palettes (embedded in code for deployment persistence)
    /// </summary>
    private static List<ColorPalette> GetDefaultColorPalettes()
    {
        var now = DateTime.UtcNow;
        return new List<ColorPalette>
        {
            // Professional Palettes
            new() { Name = "Material Purple", Category = "professional", Color1 = "#6750A4", Color2 = "#625B71", Color3 = "#7D5260", Color4 = "#FFFBFE", Color5 = "#E8DEF8", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Ocean Blue", Category = "professional", Color1 = "#1976D2", Color2 = "#115293", Color3 = "#42A5F5", Color4 = "#E3F2FD", Color5 = "#BBDEFB", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Corporate Teal", Category = "professional", Color1 = "#00796B", Color2 = "#004D40", Color3 = "#26A69A", Color4 = "#E0F2F1", Color5 = "#B2DFDB", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Slate Gray", Category = "professional", Color1 = "#455A64", Color2 = "#263238", Color3 = "#78909C", Color4 = "#ECEFF1", Color5 = "#CFD8DC", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Royal Indigo", Category = "professional", Color1 = "#3F51B5", Color2 = "#303F9F", Color3 = "#5C6BC0", Color4 = "#E8EAF6", Color5 = "#C5CAE9", IsUserDefined = false, CreatedAt = now },

            // Nature Palettes
            new() { Name = "Forest Green", Category = "nature", Color1 = "#2E7D32", Color2 = "#1B5E20", Color3 = "#4CAF50", Color4 = "#E8F5E9", Color5 = "#C8E6C9", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Autumn Harvest", Category = "nature", Color1 = "#E65100", Color2 = "#BF360C", Color3 = "#FF9800", Color4 = "#FFF3E0", Color5 = "#FFE0B2", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Desert Sand", Category = "nature", Color1 = "#8D6E63", Color2 = "#5D4037", Color3 = "#A1887F", Color4 = "#EFEBE9", Color5 = "#D7CCC8", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Sky Dawn", Category = "nature", Color1 = "#0288D1", Color2 = "#01579B", Color3 = "#03A9F4", Color4 = "#E1F5FE", Color5 = "#B3E5FC", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Lavender Fields", Category = "nature", Color1 = "#7B1FA2", Color2 = "#4A148C", Color3 = "#9C27B0", Color4 = "#F3E5F5", Color5 = "#E1BEE7", IsUserDefined = false, CreatedAt = now },

            // Vibrant Palettes
            new() { Name = "Sunset Orange", Category = "vibrant", Color1 = "#F57C00", Color2 = "#E65100", Color3 = "#FF9800", Color4 = "#FFF8E1", Color5 = "#FFECB3", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Electric Blue", Category = "vibrant", Color1 = "#2196F3", Color2 = "#1565C0", Color3 = "#64B5F6", Color4 = "#E3F2FD", Color5 = "#90CAF9", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Hot Pink", Category = "vibrant", Color1 = "#C2185B", Color2 = "#880E4F", Color3 = "#E91E63", Color4 = "#FCE4EC", Color5 = "#F8BBD9", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Cyber Purple", Category = "vibrant", Color1 = "#9C27B0", Color2 = "#6A1B9A", Color3 = "#AB47BC", Color4 = "#F3E5F5", Color5 = "#CE93D8", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Neon Green", Category = "vibrant", Color1 = "#689F38", Color2 = "#33691E", Color3 = "#8BC34A", Color4 = "#F1F8E9", Color5 = "#DCEDC8", IsUserDefined = false, CreatedAt = now },

            // Warm Palettes
            new() { Name = "Terracotta", Category = "warm", Color1 = "#D84315", Color2 = "#BF360C", Color3 = "#FF5722", Color4 = "#FBE9E7", Color5 = "#FFCCBC", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Golden Amber", Category = "warm", Color1 = "#FFA000", Color2 = "#FF6F00", Color3 = "#FFC107", Color4 = "#FFFDE7", Color5 = "#FFECB3", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Rose Garden", Category = "warm", Color1 = "#AD1457", Color2 = "#880E4F", Color3 = "#D81B60", Color4 = "#FCE4EC", Color5 = "#F48FB1", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Burnt Sienna", Category = "warm", Color1 = "#795548", Color2 = "#4E342E", Color3 = "#8D6E63", Color4 = "#EFEBE9", Color5 = "#BCAAA4", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Coral Reef", Category = "warm", Color1 = "#FF7043", Color2 = "#E64A19", Color3 = "#FF8A65", Color4 = "#FBE9E7", Color5 = "#FFAB91", IsUserDefined = false, CreatedAt = now },

            // Cool Palettes
            new() { Name = "Arctic Blue", Category = "cool", Color1 = "#039BE5", Color2 = "#0277BD", Color3 = "#29B6F6", Color4 = "#E1F5FE", Color5 = "#81D4FA", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Midnight Navy", Category = "cool", Color1 = "#1A237E", Color2 = "#0D47A1", Color3 = "#3949AB", Color4 = "#E8EAF6", Color5 = "#9FA8DA", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Mint Fresh", Category = "cool", Color1 = "#00897B", Color2 = "#00695C", Color3 = "#26A69A", Color4 = "#E0F2F1", Color5 = "#80CBC4", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Steel Blue", Category = "cool", Color1 = "#546E7A", Color2 = "#37474F", Color3 = "#78909C", Color4 = "#ECEFF1", Color5 = "#B0BEC5", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Sapphire", Category = "cool", Color1 = "#283593", Color2 = "#1A237E", Color3 = "#5C6BC0", Color4 = "#E8EAF6", Color5 = "#9FA8DA", IsUserDefined = false, CreatedAt = now },

            // Dark Theme Palettes
            new() { Name = "Dark Material", Category = "dark", Color1 = "#BB86FC", Color2 = "#03DAC6", Color3 = "#CF6679", Color4 = "#121212", Color5 = "#1E1E1E", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Dark Ocean", Category = "dark", Color1 = "#64B5F6", Color2 = "#4DD0E1", Color3 = "#81C784", Color4 = "#0D1B2A", Color5 = "#1B263B", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Dark Forest", Category = "dark", Color1 = "#81C784", Color2 = "#AED581", Color3 = "#FFD54F", Color4 = "#1B2A1B", Color5 = "#2E3B2E", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Dark Sunset", Category = "dark", Color1 = "#FF8A80", Color2 = "#FFD180", Color3 = "#FF80AB", Color4 = "#2A1B1B", Color5 = "#3B2E2E", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Dark Amethyst", Category = "dark", Color1 = "#CE93D8", Color2 = "#B39DDB", Color3 = "#80DEEA", Color4 = "#1B1B2A", Color5 = "#2E2E3B", IsUserDefined = false, CreatedAt = now },

            // Pastel Palettes
            new() { Name = "Pastel Rainbow", Category = "pastel", Color1 = "#FFB3BA", Color2 = "#FFDFBA", Color3 = "#FFFFBA", Color4 = "#BAFFC9", Color5 = "#BAE1FF", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Soft Lavender", Category = "pastel", Color1 = "#E6E6FA", Color2 = "#D8BFD8", Color3 = "#DDA0DD", Color4 = "#EE82EE", Color5 = "#DA70D6", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Peachy Keen", Category = "pastel", Color1 = "#FFDAB9", Color2 = "#FFE4B5", Color3 = "#FFEFD5", Color4 = "#FFF8DC", Color5 = "#FFFACD", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Minty Fresh", Category = "pastel", Color1 = "#98FF98", Color2 = "#90EE90", Color3 = "#8FBC8F", Color4 = "#3CB371", Color5 = "#2E8B57", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Baby Blue", Category = "pastel", Color1 = "#89CFF0", Color2 = "#ADD8E6", Color3 = "#B0E0E6", Color4 = "#AFEEEE", Color5 = "#E0FFFF", IsUserDefined = false, CreatedAt = now },

            // Earthy Palettes
            new() { Name = "Earth Tones", Category = "earthy", Color1 = "#8B4513", Color2 = "#A0522D", Color3 = "#CD853F", Color4 = "#D2B48C", Color5 = "#DEB887", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Olive Grove", Category = "earthy", Color1 = "#556B2F", Color2 = "#6B8E23", Color3 = "#808000", Color4 = "#9ACD32", Color5 = "#BDB76B", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Terracotta Clay", Category = "earthy", Color1 = "#CC5500", Color2 = "#E2725B", Color3 = "#CB6D51", Color4 = "#D9603B", Color5 = "#CD5B45", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Stone Gray", Category = "earthy", Color1 = "#708090", Color2 = "#778899", Color3 = "#B0C4DE", Color4 = "#C0C0C0", Color5 = "#D3D3D3", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Rustic Brown", Category = "earthy", Color1 = "#5C4033", Color2 = "#6F4E37", Color3 = "#8B7355", Color4 = "#A0785A", Color5 = "#C4A484", IsUserDefined = false, CreatedAt = now },

            // Modern Palettes
            new() { Name = "Modern Minimal", Category = "modern", Color1 = "#2C3E50", Color2 = "#34495E", Color3 = "#95A5A6", Color4 = "#BDC3C7", Color5 = "#ECF0F1", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Tech Blue", Category = "modern", Color1 = "#0A192F", Color2 = "#172A45", Color3 = "#303C55", Color4 = "#8892B0", Color5 = "#64FFDA", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Gradient Purple", Category = "modern", Color1 = "#667EEA", Color2 = "#764BA2", Color3 = "#6B8DD6", Color4 = "#8E37D7", Color5 = "#EC6EAD", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Clean White", Category = "modern", Color1 = "#FFFFFF", Color2 = "#F8F9FA", Color3 = "#E9ECEF", Color4 = "#DEE2E6", Color5 = "#CED4DA", IsUserDefined = false, CreatedAt = now },
            new() { Name = "Neo Brutalism", Category = "modern", Color1 = "#FF6B6B", Color2 = "#4ECDC4", Color3 = "#45B7D1", Color4 = "#96CEB4", Color5 = "#FFEAA7", IsUserDefined = false, CreatedAt = now },
        };
    }

    /// <summary>
    /// Returns the default ZIP codes (sample data for common countries)
    /// This is embedded in code to persist across deployments.
    /// For full ZIP code data, import from GeoNames data file.
    /// </summary>
    private static List<ZipCode> GetDefaultZipCodes()
    {
        return new List<ZipCode>
        {
            // United States - Major Cities Sample
            new() { Country = "United States", CountryCode = "US", PostalCode = "10001", City = "New York", State = "New York", StateCode = "NY", Latitude = 40.7484m, Longitude = -73.9967m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "10002", City = "New York", State = "New York", StateCode = "NY", Latitude = 40.7157m, Longitude = -73.9863m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "10003", City = "New York", State = "New York", StateCode = "NY", Latitude = 40.7317m, Longitude = -73.9893m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "10004", City = "New York", State = "New York", StateCode = "NY", Latitude = 40.6993m, Longitude = -74.0384m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "10005", City = "New York", State = "New York", StateCode = "NY", Latitude = 40.7069m, Longitude = -74.0089m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "10006", City = "New York", State = "New York", StateCode = "NY", Latitude = 40.7094m, Longitude = -74.0132m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "10007", City = "New York", State = "New York", StateCode = "NY", Latitude = 40.7135m, Longitude = -74.0078m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "90210", City = "Beverly Hills", State = "California", StateCode = "CA", Latitude = 34.0901m, Longitude = -118.4065m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "90001", City = "Los Angeles", State = "California", StateCode = "CA", Latitude = 33.9425m, Longitude = -118.2551m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "90002", City = "Los Angeles", State = "California", StateCode = "CA", Latitude = 33.9493m, Longitude = -118.2474m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "60601", City = "Chicago", State = "Illinois", StateCode = "IL", Latitude = 41.8819m, Longitude = -87.6278m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "60602", City = "Chicago", State = "Illinois", StateCode = "IL", Latitude = 41.8832m, Longitude = -87.6282m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "77001", City = "Houston", State = "Texas", StateCode = "TX", Latitude = 29.7523m, Longitude = -95.3485m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "77002", City = "Houston", State = "Texas", StateCode = "TX", Latitude = 29.7578m, Longitude = -95.3600m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "85001", City = "Phoenix", State = "Arizona", StateCode = "AZ", Latitude = 33.4484m, Longitude = -112.0773m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "19101", City = "Philadelphia", State = "Pennsylvania", StateCode = "PA", Latitude = 39.9526m, Longitude = -75.1652m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "78201", City = "San Antonio", State = "Texas", StateCode = "TX", Latitude = 29.4568m, Longitude = -98.5254m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "92101", City = "San Diego", State = "California", StateCode = "CA", Latitude = 32.7194m, Longitude = -117.1628m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "75201", City = "Dallas", State = "Texas", StateCode = "TX", Latitude = 32.7942m, Longitude = -96.8024m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "95101", City = "San Jose", State = "California", StateCode = "CA", Latitude = 37.3361m, Longitude = -121.8906m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "78701", City = "Austin", State = "Texas", StateCode = "TX", Latitude = 30.2672m, Longitude = -97.7431m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "32801", City = "Orlando", State = "Florida", StateCode = "FL", Latitude = 28.5383m, Longitude = -81.3792m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "33101", City = "Miami", State = "Florida", StateCode = "FL", Latitude = 25.7617m, Longitude = -80.1918m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "98101", City = "Seattle", State = "Washington", StateCode = "WA", Latitude = 47.6062m, Longitude = -122.3321m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "80201", City = "Denver", State = "Colorado", StateCode = "CO", Latitude = 39.7392m, Longitude = -104.9903m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "02101", City = "Boston", State = "Massachusetts", StateCode = "MA", Latitude = 42.3601m, Longitude = -71.0589m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "30301", City = "Atlanta", State = "Georgia", StateCode = "GA", Latitude = 33.7490m, Longitude = -84.3880m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "94102", City = "San Francisco", State = "California", StateCode = "CA", Latitude = 37.7749m, Longitude = -122.4194m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "20001", City = "Washington", State = "District of Columbia", StateCode = "DC", Latitude = 38.9072m, Longitude = -77.0369m, IsActive = true },
            new() { Country = "United States", CountryCode = "US", PostalCode = "89101", City = "Las Vegas", State = "Nevada", StateCode = "NV", Latitude = 36.1699m, Longitude = -115.1398m, IsActive = true },

            // Canada - Major Cities Sample
            new() { Country = "Canada", CountryCode = "CA", PostalCode = "M5V 3L9", City = "Toronto", State = "Ontario", StateCode = "ON", Latitude = 43.6532m, Longitude = -79.3832m, IsActive = true },
            new() { Country = "Canada", CountryCode = "CA", PostalCode = "M5H 2N2", City = "Toronto", State = "Ontario", StateCode = "ON", Latitude = 43.6510m, Longitude = -79.3470m, IsActive = true },
            new() { Country = "Canada", CountryCode = "CA", PostalCode = "V6B 1A9", City = "Vancouver", State = "British Columbia", StateCode = "BC", Latitude = 49.2827m, Longitude = -123.1207m, IsActive = true },
            new() { Country = "Canada", CountryCode = "CA", PostalCode = "V6C 3L2", City = "Vancouver", State = "British Columbia", StateCode = "BC", Latitude = 49.2849m, Longitude = -123.1119m, IsActive = true },
            new() { Country = "Canada", CountryCode = "CA", PostalCode = "H2Y 1C6", City = "Montreal", State = "Quebec", StateCode = "QC", Latitude = 45.5017m, Longitude = -73.5673m, IsActive = true },
            new() { Country = "Canada", CountryCode = "CA", PostalCode = "T2P 1J9", City = "Calgary", State = "Alberta", StateCode = "AB", Latitude = 51.0447m, Longitude = -114.0719m, IsActive = true },
            new() { Country = "Canada", CountryCode = "CA", PostalCode = "K1P 1J1", City = "Ottawa", State = "Ontario", StateCode = "ON", Latitude = 45.4215m, Longitude = -75.6972m, IsActive = true },
            new() { Country = "Canada", CountryCode = "CA", PostalCode = "T5J 0N3", City = "Edmonton", State = "Alberta", StateCode = "AB", Latitude = 53.5461m, Longitude = -113.4938m, IsActive = true },
            new() { Country = "Canada", CountryCode = "CA", PostalCode = "R3C 0C4", City = "Winnipeg", State = "Manitoba", StateCode = "MB", Latitude = 49.8951m, Longitude = -97.1384m, IsActive = true },

            // United Kingdom - Major Cities Sample
            new() { Country = "United Kingdom", CountryCode = "GB", PostalCode = "EC1A 1BB", City = "London", State = "Greater London", StateCode = "LND", Latitude = 51.5074m, Longitude = -0.1278m, IsActive = true },
            new() { Country = "United Kingdom", CountryCode = "GB", PostalCode = "SW1A 1AA", City = "London", State = "Greater London", StateCode = "LND", Latitude = 51.5014m, Longitude = -0.1419m, IsActive = true },
            new() { Country = "United Kingdom", CountryCode = "GB", PostalCode = "W1A 0AX", City = "London", State = "Greater London", StateCode = "LND", Latitude = 51.5185m, Longitude = -0.1438m, IsActive = true },
            new() { Country = "United Kingdom", CountryCode = "GB", PostalCode = "M1 1AE", City = "Manchester", State = "Greater Manchester", StateCode = "MAN", Latitude = 53.4808m, Longitude = -2.2426m, IsActive = true },
            new() { Country = "United Kingdom", CountryCode = "GB", PostalCode = "B1 1AA", City = "Birmingham", State = "West Midlands", StateCode = "WMD", Latitude = 52.4862m, Longitude = -1.8904m, IsActive = true },
            new() { Country = "United Kingdom", CountryCode = "GB", PostalCode = "L1 0AA", City = "Liverpool", State = "Merseyside", StateCode = "MSY", Latitude = 53.4084m, Longitude = -2.9916m, IsActive = true },
            new() { Country = "United Kingdom", CountryCode = "GB", PostalCode = "EH1 1AA", City = "Edinburgh", State = "Scotland", StateCode = "SCT", Latitude = 55.9533m, Longitude = -3.1883m, IsActive = true },
            new() { Country = "United Kingdom", CountryCode = "GB", PostalCode = "G1 1AA", City = "Glasgow", State = "Scotland", StateCode = "SCT", Latitude = 55.8642m, Longitude = -4.2518m, IsActive = true },
            new() { Country = "United Kingdom", CountryCode = "GB", PostalCode = "CF10 1AA", City = "Cardiff", State = "Wales", StateCode = "WLS", Latitude = 51.4816m, Longitude = -3.1791m, IsActive = true },
            new() { Country = "United Kingdom", CountryCode = "GB", PostalCode = "BT1 1AA", City = "Belfast", State = "Northern Ireland", StateCode = "NIR", Latitude = 54.5973m, Longitude = -5.9301m, IsActive = true },

            // Australia - Major Cities Sample
            new() { Country = "Australia", CountryCode = "AU", PostalCode = "2000", City = "Sydney", State = "New South Wales", StateCode = "NSW", Latitude = -33.8688m, Longitude = 151.2093m, IsActive = true },
            new() { Country = "Australia", CountryCode = "AU", PostalCode = "3000", City = "Melbourne", State = "Victoria", StateCode = "VIC", Latitude = -37.8136m, Longitude = 144.9631m, IsActive = true },
            new() { Country = "Australia", CountryCode = "AU", PostalCode = "4000", City = "Brisbane", State = "Queensland", StateCode = "QLD", Latitude = -27.4698m, Longitude = 153.0251m, IsActive = true },
            new() { Country = "Australia", CountryCode = "AU", PostalCode = "6000", City = "Perth", State = "Western Australia", StateCode = "WA", Latitude = -31.9505m, Longitude = 115.8605m, IsActive = true },
            new() { Country = "Australia", CountryCode = "AU", PostalCode = "5000", City = "Adelaide", State = "South Australia", StateCode = "SA", Latitude = -34.9285m, Longitude = 138.6007m, IsActive = true },
            new() { Country = "Australia", CountryCode = "AU", PostalCode = "2600", City = "Canberra", State = "Australian Capital Territory", StateCode = "ACT", Latitude = -35.2809m, Longitude = 149.1300m, IsActive = true },

            // Germany - Major Cities Sample
            new() { Country = "Germany", CountryCode = "DE", PostalCode = "10115", City = "Berlin", State = "Berlin", StateCode = "BE", Latitude = 52.5200m, Longitude = 13.4050m, IsActive = true },
            new() { Country = "Germany", CountryCode = "DE", PostalCode = "80331", City = "Munich", State = "Bavaria", StateCode = "BY", Latitude = 48.1351m, Longitude = 11.5820m, IsActive = true },
            new() { Country = "Germany", CountryCode = "DE", PostalCode = "60311", City = "Frankfurt", State = "Hesse", StateCode = "HE", Latitude = 50.1109m, Longitude = 8.6821m, IsActive = true },
            new() { Country = "Germany", CountryCode = "DE", PostalCode = "20095", City = "Hamburg", State = "Hamburg", StateCode = "HH", Latitude = 53.5511m, Longitude = 9.9937m, IsActive = true },
            new() { Country = "Germany", CountryCode = "DE", PostalCode = "50667", City = "Cologne", State = "North Rhine-Westphalia", StateCode = "NW", Latitude = 50.9375m, Longitude = 6.9603m, IsActive = true },

            // France - Major Cities Sample
            new() { Country = "France", CountryCode = "FR", PostalCode = "75001", City = "Paris", State = "Île-de-France", StateCode = "IDF", Latitude = 48.8566m, Longitude = 2.3522m, IsActive = true },
            new() { Country = "France", CountryCode = "FR", PostalCode = "69001", City = "Lyon", State = "Auvergne-Rhône-Alpes", StateCode = "ARA", Latitude = 45.7640m, Longitude = 4.8357m, IsActive = true },
            new() { Country = "France", CountryCode = "FR", PostalCode = "13001", City = "Marseille", State = "Provence-Alpes-Côte d'Azur", StateCode = "PAC", Latitude = 43.2965m, Longitude = 5.3698m, IsActive = true },
            new() { Country = "France", CountryCode = "FR", PostalCode = "31000", City = "Toulouse", State = "Occitanie", StateCode = "OCC", Latitude = 43.6047m, Longitude = 1.4442m, IsActive = true },
            new() { Country = "France", CountryCode = "FR", PostalCode = "06000", City = "Nice", State = "Provence-Alpes-Côte d'Azur", StateCode = "PAC", Latitude = 43.7102m, Longitude = 7.2620m, IsActive = true },

            // Japan - Major Cities Sample
            new() { Country = "Japan", CountryCode = "JP", PostalCode = "100-0001", City = "Tokyo", State = "Tokyo", StateCode = "13", Latitude = 35.6762m, Longitude = 139.6503m, IsActive = true },
            new() { Country = "Japan", CountryCode = "JP", PostalCode = "530-0001", City = "Osaka", State = "Osaka", StateCode = "27", Latitude = 34.6937m, Longitude = 135.5023m, IsActive = true },
            new() { Country = "Japan", CountryCode = "JP", PostalCode = "600-8001", City = "Kyoto", State = "Kyoto", StateCode = "26", Latitude = 35.0116m, Longitude = 135.7681m, IsActive = true },
            new() { Country = "Japan", CountryCode = "JP", PostalCode = "460-0001", City = "Nagoya", State = "Aichi", StateCode = "23", Latitude = 35.1815m, Longitude = 136.9066m, IsActive = true },
            new() { Country = "Japan", CountryCode = "JP", PostalCode = "060-0001", City = "Sapporo", State = "Hokkaido", StateCode = "01", Latitude = 43.0618m, Longitude = 141.3545m, IsActive = true },

            // India - Major Cities Sample
            new() { Country = "India", CountryCode = "IN", PostalCode = "110001", City = "New Delhi", State = "Delhi", StateCode = "DL", Latitude = 28.6139m, Longitude = 77.2090m, IsActive = true },
            new() { Country = "India", CountryCode = "IN", PostalCode = "400001", City = "Mumbai", State = "Maharashtra", StateCode = "MH", Latitude = 19.0760m, Longitude = 72.8777m, IsActive = true },
            new() { Country = "India", CountryCode = "IN", PostalCode = "560001", City = "Bangalore", State = "Karnataka", StateCode = "KA", Latitude = 12.9716m, Longitude = 77.5946m, IsActive = true },
            new() { Country = "India", CountryCode = "IN", PostalCode = "600001", City = "Chennai", State = "Tamil Nadu", StateCode = "TN", Latitude = 13.0827m, Longitude = 80.2707m, IsActive = true },
            new() { Country = "India", CountryCode = "IN", PostalCode = "700001", City = "Kolkata", State = "West Bengal", StateCode = "WB", Latitude = 22.5726m, Longitude = 88.3639m, IsActive = true },
            new() { Country = "India", CountryCode = "IN", PostalCode = "500001", City = "Hyderabad", State = "Telangana", StateCode = "TG", Latitude = 17.3850m, Longitude = 78.4867m, IsActive = true },

            // Brazil - Major Cities Sample
            new() { Country = "Brazil", CountryCode = "BR", PostalCode = "01310-100", City = "São Paulo", State = "São Paulo", StateCode = "SP", Latitude = -23.5505m, Longitude = -46.6333m, IsActive = true },
            new() { Country = "Brazil", CountryCode = "BR", PostalCode = "20040-020", City = "Rio de Janeiro", State = "Rio de Janeiro", StateCode = "RJ", Latitude = -22.9068m, Longitude = -43.1729m, IsActive = true },
            new() { Country = "Brazil", CountryCode = "BR", PostalCode = "70040-010", City = "Brasília", State = "Federal District", StateCode = "DF", Latitude = -15.7942m, Longitude = -47.8822m, IsActive = true },
            new() { Country = "Brazil", CountryCode = "BR", PostalCode = "40020-000", City = "Salvador", State = "Bahia", StateCode = "BA", Latitude = -12.9714m, Longitude = -38.5014m, IsActive = true },

            // Mexico - Major Cities Sample
            new() { Country = "Mexico", CountryCode = "MX", PostalCode = "06600", City = "Mexico City", State = "Federal District", StateCode = "DF", Latitude = 19.4326m, Longitude = -99.1332m, IsActive = true },
            new() { Country = "Mexico", CountryCode = "MX", PostalCode = "44100", City = "Guadalajara", State = "Jalisco", StateCode = "JAL", Latitude = 20.6597m, Longitude = -103.3496m, IsActive = true },
            new() { Country = "Mexico", CountryCode = "MX", PostalCode = "64000", City = "Monterrey", State = "Nuevo León", StateCode = "NLE", Latitude = 25.6866m, Longitude = -100.3161m, IsActive = true },

            // China - Major Cities Sample
            new() { Country = "China", CountryCode = "CN", PostalCode = "100000", City = "Beijing", State = "Beijing", StateCode = "BJ", Latitude = 39.9042m, Longitude = 116.4074m, IsActive = true },
            new() { Country = "China", CountryCode = "CN", PostalCode = "200000", City = "Shanghai", State = "Shanghai", StateCode = "SH", Latitude = 31.2304m, Longitude = 121.4737m, IsActive = true },
            new() { Country = "China", CountryCode = "CN", PostalCode = "510000", City = "Guangzhou", State = "Guangdong", StateCode = "GD", Latitude = 23.1291m, Longitude = 113.2644m, IsActive = true },
            new() { Country = "China", CountryCode = "CN", PostalCode = "518000", City = "Shenzhen", State = "Guangdong", StateCode = "GD", Latitude = 22.5431m, Longitude = 114.0579m, IsActive = true },

            // Singapore
            new() { Country = "Singapore", CountryCode = "SG", PostalCode = "018956", City = "Singapore", State = "Singapore", StateCode = "SG", Latitude = 1.3521m, Longitude = 103.8198m, IsActive = true },
            new() { Country = "Singapore", CountryCode = "SG", PostalCode = "048619", City = "Singapore", State = "Singapore", StateCode = "SG", Latitude = 1.2897m, Longitude = 103.8501m, IsActive = true },

            // Netherlands - Major Cities Sample
            new() { Country = "Netherlands", CountryCode = "NL", PostalCode = "1012 AB", City = "Amsterdam", State = "North Holland", StateCode = "NH", Latitude = 52.3676m, Longitude = 4.9041m, IsActive = true },
            new() { Country = "Netherlands", CountryCode = "NL", PostalCode = "3011 AA", City = "Rotterdam", State = "South Holland", StateCode = "ZH", Latitude = 51.9225m, Longitude = 4.4792m, IsActive = true },
            new() { Country = "Netherlands", CountryCode = "NL", PostalCode = "2511 AA", City = "The Hague", State = "South Holland", StateCode = "ZH", Latitude = 52.0705m, Longitude = 4.3007m, IsActive = true },

            // Switzerland - Major Cities Sample
            new() { Country = "Switzerland", CountryCode = "CH", PostalCode = "8001", City = "Zurich", State = "Zurich", StateCode = "ZH", Latitude = 47.3769m, Longitude = 8.5417m, IsActive = true },
            new() { Country = "Switzerland", CountryCode = "CH", PostalCode = "1200", City = "Geneva", State = "Geneva", StateCode = "GE", Latitude = 46.2044m, Longitude = 6.1432m, IsActive = true },
            new() { Country = "Switzerland", CountryCode = "CH", PostalCode = "3000", City = "Bern", State = "Bern", StateCode = "BE", Latitude = 46.9480m, Longitude = 7.4474m, IsActive = true },

            // Spain - Major Cities Sample
            new() { Country = "Spain", CountryCode = "ES", PostalCode = "28001", City = "Madrid", State = "Community of Madrid", StateCode = "MD", Latitude = 40.4168m, Longitude = -3.7038m, IsActive = true },
            new() { Country = "Spain", CountryCode = "ES", PostalCode = "08001", City = "Barcelona", State = "Catalonia", StateCode = "CT", Latitude = 41.3851m, Longitude = 2.1734m, IsActive = true },
            new() { Country = "Spain", CountryCode = "ES", PostalCode = "46001", City = "Valencia", State = "Valencian Community", StateCode = "VC", Latitude = 39.4699m, Longitude = -0.3763m, IsActive = true },

            // Italy - Major Cities Sample
            new() { Country = "Italy", CountryCode = "IT", PostalCode = "00100", City = "Rome", State = "Lazio", StateCode = "RM", Latitude = 41.9028m, Longitude = 12.4964m, IsActive = true },
            new() { Country = "Italy", CountryCode = "IT", PostalCode = "20121", City = "Milan", State = "Lombardy", StateCode = "MI", Latitude = 45.4642m, Longitude = 9.1900m, IsActive = true },
            new() { Country = "Italy", CountryCode = "IT", PostalCode = "50100", City = "Florence", State = "Tuscany", StateCode = "FI", Latitude = 43.7696m, Longitude = 11.2558m, IsActive = true },
            new() { Country = "Italy", CountryCode = "IT", PostalCode = "30100", City = "Venice", State = "Veneto", StateCode = "VE", Latitude = 45.4408m, Longitude = 12.3155m, IsActive = true },

            // South Korea - Major Cities Sample
            new() { Country = "South Korea", CountryCode = "KR", PostalCode = "04524", City = "Seoul", State = "Seoul", StateCode = "11", Latitude = 37.5665m, Longitude = 126.9780m, IsActive = true },
            new() { Country = "South Korea", CountryCode = "KR", PostalCode = "48099", City = "Busan", State = "Busan", StateCode = "26", Latitude = 35.1796m, Longitude = 129.0756m, IsActive = true },

            // UAE - Major Cities Sample
            new() { Country = "United Arab Emirates", CountryCode = "AE", PostalCode = "00000", City = "Dubai", State = "Dubai", StateCode = "DU", Latitude = 25.2048m, Longitude = 55.2708m, IsActive = true },
            new() { Country = "United Arab Emirates", CountryCode = "AE", PostalCode = "00001", City = "Abu Dhabi", State = "Abu Dhabi", StateCode = "AZ", Latitude = 24.4539m, Longitude = 54.3773m, IsActive = true },

            // South Africa - Major Cities Sample
            new() { Country = "South Africa", CountryCode = "ZA", PostalCode = "2001", City = "Johannesburg", State = "Gauteng", StateCode = "GT", Latitude = -26.2041m, Longitude = 28.0473m, IsActive = true },
            new() { Country = "South Africa", CountryCode = "ZA", PostalCode = "8001", City = "Cape Town", State = "Western Cape", StateCode = "WC", Latitude = -33.9249m, Longitude = 18.4241m, IsActive = true },
            new() { Country = "South Africa", CountryCode = "ZA", PostalCode = "4001", City = "Durban", State = "KwaZulu-Natal", StateCode = "NL", Latitude = -29.8587m, Longitude = 31.0218m, IsActive = true },

            // New Zealand - Major Cities Sample
            new() { Country = "New Zealand", CountryCode = "NZ", PostalCode = "1010", City = "Auckland", State = "Auckland", StateCode = "AUK", Latitude = -36.8485m, Longitude = 174.7633m, IsActive = true },
            new() { Country = "New Zealand", CountryCode = "NZ", PostalCode = "6011", City = "Wellington", State = "Wellington", StateCode = "WGN", Latitude = -41.2865m, Longitude = 174.7762m, IsActive = true },
            new() { Country = "New Zealand", CountryCode = "NZ", PostalCode = "8011", City = "Christchurch", State = "Canterbury", StateCode = "CAN", Latitude = -43.5321m, Longitude = 172.6362m, IsActive = true },

            // Ireland - Major Cities Sample
            new() { Country = "Ireland", CountryCode = "IE", PostalCode = "D01 F5P2", City = "Dublin", State = "Leinster", StateCode = "L", Latitude = 53.3498m, Longitude = -6.2603m, IsActive = true },
            new() { Country = "Ireland", CountryCode = "IE", PostalCode = "T12 W8RP", City = "Cork", State = "Munster", StateCode = "M", Latitude = 51.8985m, Longitude = -8.4756m, IsActive = true },
            new() { Country = "Ireland", CountryCode = "IE", PostalCode = "H91 E2K3", City = "Galway", State = "Connacht", StateCode = "C", Latitude = 53.2707m, Longitude = -9.0568m, IsActive = true },
        };
    }
}
