// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing color palettes from YourPalettes GitHub repository
/// </summary>
public class ColorPaletteService : IColorPaletteService
{
    private const string GITHUB_PALETTES_URL =
        "https://raw.githubusercontent.com/NSTechBytes/yourpalettes-website/main/api/colorpalettes.json";

    private readonly ICrmDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ColorPaletteService> _logger;
    private readonly IResilienceService? _resilienceService;

    public ColorPaletteService(
        ICrmDbContext context,
        HttpClient httpClient,
        ILogger<ColorPaletteService> logger,
        IResilienceService? resilienceService = null)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
        _resilienceService = resilienceService;
    }

    public async Task<IEnumerable<ColorPaletteDto>> GetAllAsync()
    {
        var palettes = await _context.ColorPalettes
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return palettes.Select(ToDto);
    }

    public async Task<IEnumerable<ColorPaletteDto>> GetByCategoryAsync(string category)
    {
        var palettes = await _context.ColorPalettes
            .Where(p => p.Category == category)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return palettes.Select(ToDto);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.ColorPalettes
            .Where(p => p.Category != null)
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.ColorPalettes.CountAsync();
    }

    public async Task<IEnumerable<ColorPaletteDto>> SearchAsync(string searchTerm, int limit = 50)
    {
        var normalizedSearch = searchTerm.ToLower();

        var palettes = await _context.ColorPalettes
            .Where(p => p.Name.ToLower().Contains(normalizedSearch) ||
                       (p.Category != null && p.Category.ToLower().Contains(normalizedSearch)))
            .OrderBy(p => p.Name)
            .Take(limit)
            .ToListAsync();

        return palettes.Select(ToDto);
    }

    public async Task<int> RefreshFromGitHubAsync()
    {
        try
        {
            _logger.LogInformation("Starting palette refresh from GitHub...");

            string json;
            if (_resilienceService != null)
            {
                json = await _resilienceService.ExecuteAsync(
                    "ColorPalette-GitHub",
                    async ct =>
                    {
                        var response = await _httpClient.GetAsync(GITHUB_PALETTES_URL, ct);
                        response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsStringAsync(ct);
                    });
            }
            else
            {
                var response = await _httpClient.GetAsync(GITHUB_PALETTES_URL);
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync();
            }

            var palettes = JsonSerializer.Deserialize<List<GitHubPalette>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (palettes == null || palettes.Count == 0)
            {
                _logger.LogWarning("No palettes found in GitHub response");
                return 0;
            }

            _logger.LogInformation("Fetched {Count} palettes from GitHub", palettes.Count);

            // Clear existing non-user-defined palettes only
            await _context.ColorPalettes.Where(p => !p.IsUserDefined).ExecuteDeleteAsync();

            // Insert new palettes in batches
            var entities = palettes.Select(p => new ColorPalette
            {
                Name = p.Name ?? "Unnamed",
                Category = p.Category,
                Color1 = p.Colors?.ElementAtOrDefault(0) ?? "#000000",
                Color2 = p.Colors?.ElementAtOrDefault(1) ?? "#333333",
                Color3 = p.Colors?.ElementAtOrDefault(2) ?? "#666666",
                Color4 = p.Colors?.ElementAtOrDefault(3) ?? "#999999",
                Color5 = p.Colors?.ElementAtOrDefault(4) ?? "#CCCCCC",
                IsUserDefined = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            // Batch insert for better performance
            const int batchSize = 1000;
            for (int i = 0; i < entities.Count; i += batchSize)
            {
                var batch = entities.Skip(i).Take(batchSize);
                await _context.ColorPalettes.AddRangeAsync(batch);
                await _context.SaveChangesAsync();
            }

            // Update last refresh timestamp in system settings
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings != null)
            {
                settings.PalettesLastRefreshed = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Successfully cached {Count} palettes", entities.Count);
            return entities.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing palettes from GitHub");
            throw;
        }
    }

    public async Task<ColorPaletteDto> CreateCustomPaletteAsync(CreateCustomPaletteRequest request, int userId)
    {
        var palette = new ColorPalette
        {
            Name = request.Name,
            Category = "User Defined",
            Color1 = request.Colors.ElementAtOrDefault(0) ?? "#000000",
            Color2 = request.Colors.ElementAtOrDefault(1) ?? "#333333",
            Color3 = request.Colors.ElementAtOrDefault(2) ?? "#666666",
            Color4 = request.Colors.ElementAtOrDefault(3) ?? "#999999",
            Color5 = request.Colors.ElementAtOrDefault(4) ?? "#CCCCCC",
            IsUserDefined = true,
            CreatedByUserId = userId
        };

        _context.ColorPalettes.Add(palette);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created custom palette '{Name}' by user {UserId}", request.Name, userId);
        return ToDto(palette);
    }

    public async Task<bool> DeleteCustomPaletteAsync(int paletteId, int userId)
    {
        var palette = await _context.ColorPalettes
            .FirstOrDefaultAsync(p => p.Id == paletteId && p.IsUserDefined && p.CreatedByUserId == userId);

        if (palette == null)
            return false;

        _context.ColorPalettes.Remove(palette);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted custom palette {PaletteId} by user {UserId}", paletteId, userId);
        return true;
    }

    public async Task<IEnumerable<ColorPaletteDto>> GetUserDefinedPalettesAsync()
    {
        var palettes = await _context.ColorPalettes
            .Where(p => p.IsUserDefined)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return palettes.Select(ToDto);
    }

    private static ColorPaletteDto ToDto(ColorPalette palette) => new()
    {
        Id = palette.Id,
        Name = palette.Name,
        Description = palette.Category,  // Category from entity maps to Description in DTO
        PrimaryColor = palette.Color1,
        SecondaryColor = palette.Color2,
        SuccessColor = palette.Color3,
        WarningColor = palette.Color4,
        ErrorColor = palette.Color5,
        InfoColor = palette.Color1,
        BackgroundLight = "#FFFFFF",
        BackgroundDark = "#F5F5F5",
        TextLight = "#000000",
        TextDark = "#FFFFFF",
        BorderColor = "#CCCCCC",
        IsDefault = false,  // Entity doesn't have this property
        IsActive = true,    // Entity doesn't have this property
        CreatedAt = palette.CreatedAt,
        UpdatedAt = palette.UpdatedAt
    };

    /// <summary>
    /// Internal class for deserializing GitHub palette JSON
    /// </summary>
    private class GitHubPalette
    {
        public string? Name { get; set; }
        public List<string>? Colors { get; set; }
        public string? Category { get; set; }
    }
}
