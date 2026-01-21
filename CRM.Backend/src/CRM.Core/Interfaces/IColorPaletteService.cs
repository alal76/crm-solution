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

/// <summary>
/// DTO for color palette
/// </summary>
public class ColorPaletteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public List<string> Colors { get; set; } = new();
    public bool IsUserDefined { get; set; }
}

/// <summary>
/// Request to create a custom palette
/// </summary>
public class CreateCustomPaletteRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Colors { get; set; } = new();
}
