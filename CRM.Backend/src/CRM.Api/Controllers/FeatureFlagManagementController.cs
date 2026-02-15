// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Api.Authorization;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing feature flags, variants, targeting, and audit trails
/// </summary>
[ApiController]
[Route("api/feature-flags")]
[RequireRole(UserRole.Admin)]
public class FeatureFlagManagementController : ControllerBase
{
    private readonly IFeatureFlagManagementService _service;
    private readonly ILogger<FeatureFlagManagementController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FeatureFlagManagementController(
        IFeatureFlagManagementService service,
        ILogger<FeatureFlagManagementController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _service = service;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Get all feature flags
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FeatureFlagDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeatureFlagDto>>> GetAllFlags(CancellationToken cancellationToken = default)
    {
        try
        {
            var flags = await _service.GetAllFlagsAsync(cancellationToken);
            return Ok(flags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flags");
            return StatusCode(500, new { error = "Failed to retrieve feature flags" });
        }
    }

    /// <summary>
    /// Get a specific feature flag
    /// </summary>
    [HttpGet("{flagName}")]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeatureFlagDto>> GetFlag(string flagName, CancellationToken cancellationToken = default)
    {
        try
        {
            var flag = await _service.GetFlagAsync(flagName, cancellationToken);
            if (flag == null)
                return NotFound(new { error = $"Feature flag '{flagName}' not found" });

            return Ok(flag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flag {FlagName}", flagName);
            return StatusCode(500, new { error = "Failed to retrieve feature flag" });
        }
    }

    /// <summary>
    /// Check if a flag is enabled for the current user
    /// </summary>
    [HttpGet("{flagName}/check")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CheckFlagForUser(string flagName, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
                return Ok(false);

            var isEnabled = await _service.IsFlagEnabledForUserAsync(flagName, userId, cancellationToken);
            return Ok(isEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking flag for user");
            return Ok(false);
        }
    }

    /// <summary>
    /// Update a feature flag
    /// </summary>
    [HttpPut("{flagName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateFlag(string flagName, UpdateFeatureFlagDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
                return Unauthorized(new { error = "User context required" });

            var result = await _service.UpdateFlagAsync(flagName, dto, userId, cancellationToken);
            if (!result)
                return BadRequest(new { error = "Failed to update flag" });

            _logger.LogInformation("Feature flag {FlagName} updated by user {UserId}", flagName, userId);
            return Ok(new { message = "Flag updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature flag {FlagName}", flagName);
            return StatusCode(500, new { error = "Failed to update flag" });
        }
    }

    /// <summary>
    /// Set rollout percentage for a flag (0-100)
    /// </summary>
    [HttpPut("{flagName}/rollout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetRolloutPercentage(string flagName, [FromBody] int percentage, CancellationToken cancellationToken = default)
    {
        try
        {
            if (percentage < 0 || percentage > 100)
                return BadRequest(new { error = "Percentage must be between 0 and 100" });

            var userId = GetCurrentUserId();
            var result = await _service.SetRolloutPercentageAsync(flagName, percentage, userId, cancellationToken);

            if (!result)
                return BadRequest(new { error = "Failed to set rollout percentage" });

            _logger.LogInformation("Rollout percentage set for {FlagName}: {Percentage}%", flagName, percentage);
            return Ok(new { message = $"Rollout set to {percentage}%" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting rollout percentage");
            return StatusCode(500, new { error = "Failed to set rollout percentage" });
        }
    }

    /// <summary>
    /// Set A/B testing variants for a flag
    /// </summary>
    [HttpPost("{flagName}/variants")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetVariants(string flagName, [FromBody] FlagVariantDto[] variants, CancellationToken cancellationToken = default)
    {
        try
        {
            if (variants == null || variants.Length == 0)
                return BadRequest(new { error = "At least one variant is required" });

            var totalWeight = variants.Sum(v => v.Weight);
            if (totalWeight != 100)
                return BadRequest(new { error = "Variant weights must sum to 100" });

            var userId = GetCurrentUserId();
            var result = await _service.SetVariantsAsync(flagName, variants, userId, cancellationToken);

            if (!result)
                return BadRequest(new { error = "Failed to set variants" });

            _logger.LogInformation("Variants set for {FlagName}: {Count} variants", flagName, variants.Length);
            return Ok(new { message = $"Variants set successfully ({variants.Length} variants)" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting variants for {FlagName}", flagName);
            return StatusCode(500, new { error = "Failed to set variants" });
        }
    }

    /// <summary>
    /// Get variant assignment for current user
    /// </summary>
    [HttpGet("{flagName}/variant")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FlagVariantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlagVariantDto>> GetUserVariant(string flagName, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
                return NotFound(new { error = "User context required" });

            var variant = await _service.GetUserVariantAsync(flagName, userId, cancellationToken);
            if (variant == null)
                return NotFound(new { error = "No variant assigned for this user" });

            return Ok(variant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user variant");
            return StatusCode(500, new { error = "Failed to get variant" });
        }
    }

    /// <summary>
    /// Get available providers for a category
    /// </summary>
    [HttpGet("providers/{category}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetAvailableProviders(string category, CancellationToken cancellationToken = default)
    {
        try
        {
            var providers = await _service.GetAvailableProvidersAsync(category, cancellationToken);
            return Ok(providers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available providers for category {Category}", category);
            return StatusCode(500, new { error = "Failed to retrieve providers" });
        }
    }

    /// <summary>
    /// Get active provider for a category
    /// </summary>
    [HttpGet("providers/{category}/active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> GetActiveProvider(string category, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await _service.GetActiveProviderAsync(category, cancellationToken);
            return Ok(provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active provider for category {Category}", category);
            return StatusCode(500, new { error = "Failed to retrieve active provider" });
        }
    }

    /// <summary>
    /// Update active provider for a category
    /// </summary>
    [HttpPut("providers/{category}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProvider(string category, UpdateProviderTypeDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.UpdateProviderTypeAsync(category, dto.Type, userId, cancellationToken);

            if (!result)
                return BadRequest(new { error = "Failed to update provider" });

            _logger.LogInformation("Provider for {Category} changed to {Type}", category, dto.Type);
            return Ok(new { message = $"Provider updated to {dto.Type}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider for category {Category}", category);
            return StatusCode(500, new { error = "Failed to update provider" });
        }
    }

    /// <summary>
    /// Get feature flag audit log
    /// </summary>
    [HttpGet("audit")]
    [ProducesResponseType(typeof(IEnumerable<FeatureFlagAuditEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeatureFlagAuditEntryDto>>> GetAuditLog(int count = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = await _service.GetAuditLogAsync(count, cancellationToken);
            return Ok(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log");
            return StatusCode(500, new { error = "Failed to retrieve audit log" });
        }
    }

    /// <summary>
    /// Get audit log for a specific flag
    /// </summary>
    [HttpGet("{flagName}/audit")]
    [ProducesResponseType(typeof(IEnumerable<FeatureFlagAuditEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeatureFlagAuditEntryDto>>> GetFlagAuditLog(string flagName, int count = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = await _service.GetFlagAuditLogAsync(flagName, count, cancellationToken);
            return Ok(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log for flag {FlagName}", flagName);
            return StatusCode(500, new { error = "Failed to retrieve audit log" });
        }
    }

    /// <summary>
    /// Reset all flags to defaults
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetToDefaults(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.ResetToDefaultsAsync(userId, cancellationToken);

            if (!result)
                return BadRequest(new { error = "Failed to reset flags" });

            _logger.LogInformation("All feature flags reset to defaults");
            return Ok(new { message = "All flags reset to defaults" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting flags");
            return StatusCode(500, new { error = "Failed to reset flags" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("uid")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
