// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for managing color palettes from YourPalettes repository
/// </summary>
public interface IColorPaletteService
{
    /// <summary>
    /// Get all cached color palettes
    /// </summary>
    Task<IEnumerable<ColorPaletteDto>> GetAllAsync();

    /// <summary>
    /// Get palettes by category
    /// </summary>
    Task<IEnumerable<ColorPaletteDto>> GetByCategoryAsync(string category);

    /// <summary>
    /// Get all unique categories
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync();

    /// <summary>
    /// Refresh palettes from GitHub repository
    /// </summary>
    Task<int> RefreshFromGitHubAsync();

    /// <summary>
    /// Get palette count
    /// </summary>
    Task<int> GetCountAsync();

    /// <summary>
    /// Search palettes by name
    /// </summary>
    Task<IEnumerable<ColorPaletteDto>> SearchAsync(string searchTerm, int limit = 50);

    /// <summary>
    /// Create a user-defined custom palette
    /// </summary>
    Task<ColorPaletteDto> CreateCustomPaletteAsync(CreateCustomPaletteRequest request, int userId);

    /// <summary>
    /// Delete a user-defined custom palette
    /// </summary>
    Task<bool> DeleteCustomPaletteAsync(int paletteId, int userId);

    /// <summary>
    /// Get user-defined palettes
    /// </summary>
    Task<IEnumerable<ColorPaletteDto>> GetUserDefinedPalettesAsync();
}
