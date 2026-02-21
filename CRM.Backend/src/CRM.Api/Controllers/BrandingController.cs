// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing branding configuration.
/// Handles custom logos, solution names, favicons, and white-label settings.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BrandingController : ControllerBase
{
    private readonly IBrandingConfigService _brandingConfigService;
    private readonly ILogger<BrandingController> _logger;

    public BrandingController(IBrandingConfigService brandingConfigService, ILogger<BrandingController> logger)
    {
        _brandingConfigService = brandingConfigService ?? throw new ArgumentNullException(nameof(brandingConfigService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current branding configuration.
    /// </summary>
    /// <returns>Current branding configuration.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BrandingConfigDto>> GetBrandingConfig(CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _brandingConfigService.GetCurrentBrandingAsync(cancellationToken);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branding configuration");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving branding configuration" });
        }
    }

    /// <summary>
    /// Gets branding configuration by ID (admin only).
    /// </summary>
    /// <param name="id">Branding configuration ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Branding configuration or not found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BrandingConfigDto>> GetBrandingById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _brandingConfigService.GetByIdAsync(id, cancellationToken);
            if (config == null)
                return NotFound();

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branding configuration {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving branding configuration" });
        }
    }

    /// <summary>
    /// Updates the solution name (admin only).
    /// </summary>
    /// <param name="request">Update request containing new solution name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated branding configuration.</returns>
    [HttpPost("solution-name")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BrandingConfigDto>> UpdateSolutionName(
        [FromBody] UpdateSolutionNameRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SolutionName))
                return BadRequest(new { message = "Solution name is required" });

            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var config = await _brandingConfigService.UpdateSolutionNameAsync(
                request.SolutionName, userId, cancellationToken);

            return Ok(config);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid solution name provided");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating solution name");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating solution name" });
        }
    }

    /// <summary>
    /// Uploads a custom branding logo (admin only).
    /// </summary>
    /// <param name="request">Upload request containing file data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result with updated configuration.</returns>
    [HttpPost("upload-logo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BrandingOperationResponse>> UploadCustomLogo(
        [FromBody] UploadLogoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FileContent))
            {
                return BadRequest(new BrandingOperationResponse
                {
                    Success = false,
                    Message = "File content is required"
                });
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var response = await _brandingConfigService.UploadCustomLogoAsync(request, userId, cancellationToken);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading custom logo");
            return StatusCode(StatusCodes.Status500InternalServerError, new BrandingOperationResponse
            {
                Success = false,
                Message = "Error uploading logo"
            });
        }
    }

    /// <summary>
    /// Uploads a favicon for the browser tab (admin only).
    /// </summary>
    /// <param name="request">Upload request containing file data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result with updated configuration.</returns>
    [HttpPost("upload-favicon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BrandingOperationResponse>> UploadFavicon(
        [FromBody] UploadFaviconRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FileContent))
            {
                return BadRequest(new BrandingOperationResponse
                {
                    Success = false,
                    Message = "File content is required"
                });
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var response = await _brandingConfigService.UploadFaviconAsync(request, userId, cancellationToken);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading favicon");
            return StatusCode(StatusCodes.Status500InternalServerError, new BrandingOperationResponse
            {
                Success = false,
                Message = "Error uploading favicon"
            });
        }
    }

    /// <summary>
    /// Deletes the custom branding logo (admin only).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated branding configuration.</returns>
    [HttpDelete("custom-logo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BrandingConfigDto>> DeleteCustomLogo(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var config = await _brandingConfigService.DeleteCustomLogoAsync(userId, cancellationToken);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting custom logo");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error deleting logo" });
        }
    }

    /// <summary>
    /// Deletes the uploaded favicon (admin only).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated branding configuration.</returns>
    [HttpDelete("favicon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BrandingConfigDto>> DeleteFavicon(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var config = await _brandingConfigService.DeleteFaviconAsync(userId, cancellationToken);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting favicon");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error deleting favicon" });
        }
    }

    /// <summary>
    /// Toggles custom branding on or off (admin only).
    /// </summary>
    /// <param name="isEnabled">Whether custom branding should be enabled.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated branding configuration.</returns>
    [HttpPost("toggle-custom-branding")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BrandingConfigDto>> ToggleCustomBranding(
        [FromQuery] bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var config = await _brandingConfigService.SetCustomBrandingEnabledAsync(isEnabled, userId, cancellationToken);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling custom branding");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error toggling custom branding" });
        }
    }

    /// <summary>
    /// Gets the current user ID from claims.
    /// </summary>
    /// <returns>User ID or 0 if not found.</returns>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
    }
}
