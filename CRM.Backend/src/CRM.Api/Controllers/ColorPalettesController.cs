// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Core.Dtos;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing color palettes from YourPalettes repository
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ColorPalettesController : CrmControllerBase
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
                var palettes = await _paletteService.GetAllAsync();
        return Ok(palettes);
    }

    /// <summary>
    /// Get palettes by category
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string category)
    {
                var palettes = await _paletteService.GetByCategoryAsync(category);
        return Ok(palettes);
    }

    /// <summary>
    /// Get all unique categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
                var categories = await _paletteService.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Search palettes by name or category
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 50)
    {
                if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "Search term is required" });
        }

        var palettes = await _paletteService.SearchAsync(q, limit);
        return Ok(palettes);
    }

    /// <summary>
    /// Get palette count
    /// </summary>
    [HttpGet("count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount()
    {
                var count = await _paletteService.GetCountAsync();
        return Ok(new { count });
    }

    /// <summary>
    /// Refresh palettes from GitHub repository
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh()
    {
                _logger.LogInformation("Refreshing palettes from GitHub...");
        var count = await _paletteService.RefreshFromGitHubAsync();
        return Ok(new
        {
            message = $"Successfully refreshed {count:N0} palettes from GitHub",
            count,
            refreshedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get user-defined custom palettes
    /// </summary>
    [HttpGet("user-defined")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserDefined()
    {
                var palettes = await _paletteService.GetUserDefinedPalettesAsync();
        return Ok(palettes);
    }

    /// <summary>
    /// Create a custom user-defined palette
    /// </summary>
    [HttpPost("custom")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCustomPalette([FromBody] CreateCustomPaletteRequest request)
    {
                var userIdClaim = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user" });
        }

        var palette = await _paletteService.CreateCustomPaletteAsync(request, userId);
        return Ok(palette);
    }

    /// <summary>
    /// Delete a custom user-defined palette
    /// </summary>
    [HttpDelete("custom/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCustomPalette(int id)
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
}
