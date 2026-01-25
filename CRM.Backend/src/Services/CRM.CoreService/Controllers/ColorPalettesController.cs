/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing color palettes from YourPalettes repository
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ColorPalettesController : ControllerBase
{
    private readonly IColorPaletteService _paletteService;
    private readonly ILogger<ColorPalettesController> _logger;

    public ColorPalettesController(
        IColorPaletteService paletteService,
        ILogger<ColorPalettesController> logger)
    {
        _paletteService = paletteService;
        _logger = logger;
    }

    /// <summary>
    /// Get all color palettes
    /// </summary>
    [HttpGet]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var palettes = await _paletteService.GetAllAsync();
            return Ok(palettes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting palettes");
            return StatusCode(500, new { message = "Error retrieving palettes" });
        }
    }

    /// <summary>
    /// Get palettes by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
    {
        try
        {
            var palettes = await _paletteService.GetByCategoryAsync(category);
            return Ok(palettes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting palettes by category");
            return StatusCode(500, new { message = "Error retrieving palettes" });
        }
    }

    /// <summary>
    /// Get all unique categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _paletteService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { message = "Error retrieving categories" });
        }
    }

    /// <summary>
    /// Search palettes by name or category
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 50)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { message = "Search term is required" });
            }
            
            var palettes = await _paletteService.SearchAsync(q, limit);
            return Ok(palettes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching palettes");
            return StatusCode(500, new { message = "Error searching palettes" });
        }
    }

    /// <summary>
    /// Get palette count
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        try
        {
            var count = await _paletteService.GetCountAsync();
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting palette count");
            return StatusCode(500, new { message = "Error retrieving count" });
        }
    }

    /// <summary>
    /// Refresh palettes from GitHub repository
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            _logger.LogInformation("Refreshing palettes from GitHub...");
            var count = await _paletteService.RefreshFromGitHubAsync();
            return Ok(new { 
                message = $"Successfully refreshed {count:N0} palettes from GitHub",
                count,
                refreshedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing palettes from GitHub");
            return StatusCode(500, new { message = "Error refreshing palettes from GitHub: " + ex.Message });
        }
    }

    /// <summary>
    /// Get user-defined custom palettes
    /// </summary>
    [HttpGet("user-defined")]
    public async Task<IActionResult> GetUserDefined()
    {
        try
        {
            var palettes = await _paletteService.GetUserDefinedPalettesAsync();
            return Ok(palettes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user-defined palettes");
            return StatusCode(500, new { message = "Error retrieving user-defined palettes" });
        }
    }

    /// <summary>
    /// Create a custom user-defined palette
    /// </summary>
    [HttpPost("custom")]
    public async Task<IActionResult> CreateCustomPalette([FromBody] CreateCustomPaletteRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user" });
            }
            
            var palette = await _paletteService.CreateCustomPaletteAsync(request, userId);
            return Ok(palette);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating custom palette");
            return StatusCode(500, new { message = "Error creating custom palette" });
        }
    }

    /// <summary>
    /// Delete a custom user-defined palette
    /// </summary>
    [HttpDelete("custom/{id}")]
    public async Task<IActionResult> DeleteCustomPalette(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user" });
            }
            
            var success = await _paletteService.DeleteCustomPaletteAsync(id, userId);
            if (!success)
            {
                return NotFound(new { message = "Palette not found or you don't have permission to delete it" });
            }
            return Ok(new { message = "Palette deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting custom palette");
            return StatusCode(500, new { message = "Error deleting custom palette" });
        }
    }
}
