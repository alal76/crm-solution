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
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing user UI preferences and customizations
/// </summary>
[ApiController]
[Route("api/ui-preferences")]
public class UIPreferencesController : ControllerBase
{
    private readonly IUserInterfaceService _service;
    private readonly ILogger<UIPreferencesController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UIPreferencesController(
        IUserInterfaceService service,
        ILogger<UIPreferencesController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _service = service;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Get UI preferences for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(UIPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UIPreferenceDto>> GetUIPreferences(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var prefs = await _service.GetUserUIPreferencesAsync(userId, cancellationToken);

            if (prefs == null)
                return NotFound(new { error = "UI preferences not found" });

            return Ok(prefs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving UI preferences");
            return StatusCode(500, new { error = "Failed to retrieve UI preferences" });
        }
    }

    /// <summary>
    /// Save UI preferences for the current user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UIPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UIPreferenceDto>> SaveUIPreferences(CreateUpdateUIPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.SaveUIPreferencesAsync(userId, dto, cancellationToken);

            _logger.LogInformation("UI preferences saved for user {UserId}", userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving UI preferences");
            return StatusCode(500, new { error = "Failed to save UI preferences" });
        }
    }

    /// <summary>
    /// Reset UI preferences to defaults
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetUIPreferences(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.ResetUIPreferencesAsync(userId, cancellationToken);

            if (!result)
                return BadRequest(new { error = "Failed to reset preferences" });

            _logger.LogInformation("UI preferences reset for user {UserId}", userId);
            return Ok(new { message = "UI preferences reset to defaults" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting UI preferences");
            return StatusCode(500, new { error = "Failed to reset preferences" });
        }
    }

    /// <summary>
    /// Get UI customization for a module/page
    /// </summary>
    [HttpGet("customizations/{moduleName}/{pageName}")]
    [ProducesResponseType(typeof(UICustomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UICustomizationDto>> GetUICustomization(string moduleName, string pageName, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var customization = await _service.GetUICustomizationAsync(userId, moduleName, pageName, cancellationToken);

            if (customization == null)
                return NotFound(new { error = "Customization not found" });

            return Ok(customization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving UI customization");
            return StatusCode(500, new { error = "Failed to retrieve customization" });
        }
    }

    /// <summary>
    /// Get all UI customizations for the current user
    /// </summary>
    [HttpGet("customizations")]
    [ProducesResponseType(typeof(IEnumerable<UICustomizationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UICustomizationDto>>> GetAllUICustomizations(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var customizations = await _service.GetAllUICustomizationsAsync(userId, cancellationToken);

            return Ok(customizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving UI customizations");
            return StatusCode(500, new { error = "Failed to retrieve customizations" });
        }
    }

    /// <summary>
    /// Save UI customization for a module/page
    /// </summary>
    [HttpPost("customizations")]
    [ProducesResponseType(typeof(UICustomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UICustomizationDto>> SaveUICustomization(CreateUpdateUICustomizationDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.ModuleName) || string.IsNullOrEmpty(dto.PageName))
                return BadRequest(new { error = "ModuleName and PageName are required" });

            var userId = GetCurrentUserId();
            var result = await _service.SaveUICustomizationAsync(userId, dto, cancellationToken);

            _logger.LogInformation("UI customization saved for user {UserId}, module {Module}", userId, dto.ModuleName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving UI customization");
            return StatusCode(500, new { error = "Failed to save customization" });
        }
    }

    /// <summary>
    /// Delete UI customization
    /// </summary>
    [HttpDelete("customizations/{moduleName}/{pageName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteUICustomization(string moduleName, string pageName, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.DeleteUICustomizationAsync(userId, moduleName, pageName, cancellationToken);

            if (!result)
                return BadRequest(new { error = "Failed to delete customization" });

            _logger.LogInformation("UI customization deleted for user {UserId}, module {Module}", userId, moduleName);
            return Ok(new { message = "Customization deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting UI customization");
            return StatusCode(500, new { error = "Failed to delete customization" });
        }
    }

    /// <summary>
    /// Get dashboard customization
    /// </summary>
    [HttpGet("dashboards/{dashboardName}")]
    [ProducesResponseType(typeof(DashboardCustomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardCustomizationDto>> GetDashboardCustomization(string dashboardName, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var dashboard = await _service.GetDashboardCustomizationAsync(userId, dashboardName, cancellationToken);

            if (dashboard == null)
                return NotFound(new { error = "Dashboard not found" });

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard customization");
            return StatusCode(500, new { error = "Failed to retrieve dashboard" });
        }
    }

    /// <summary>
    /// Get all dashboard customizations
    /// </summary>
    [HttpGet("dashboards")]
    [ProducesResponseType(typeof(IEnumerable<DashboardCustomizationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DashboardCustomizationDto>>> GetAllDashboardCustomizations(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var dashboards = await _service.GetAllDashboardCustomizationsAsync(userId, cancellationToken);

            return Ok(dashboards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard customizations");
            return StatusCode(500, new { error = "Failed to retrieve dashboards" });
        }
    }

    /// <summary>
    /// Save dashboard customization
    /// </summary>
    [HttpPost("dashboards")]
    [ProducesResponseType(typeof(DashboardCustomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DashboardCustomizationDto>> SaveDashboardCustomization(CreateUpdateDashboardCustomizationDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.DashboardName))
                return BadRequest(new { error = "DashboardName is required" });

            var userId = GetCurrentUserId();
            var result = await _service.SaveDashboardCustomizationAsync(userId, dto, cancellationToken);

            _logger.LogInformation("Dashboard saved for user {UserId}: {DashboardName}", userId, dto.DashboardName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving dashboard customization");
            return StatusCode(500, new { error = "Failed to save dashboard" });
        }
    }

    /// <summary>
    /// Delete dashboard customization
    /// </summary>
    [HttpDelete("dashboards/{dashboardName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteDashboardCustomization(string dashboardName, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.DeleteDashboardCustomizationAsync(userId, dashboardName, cancellationToken);

            if (!result)
                return BadRequest(new { error = "Failed to delete dashboard" });

            _logger.LogInformation("Dashboard deleted for user {UserId}: {DashboardName}", userId, dashboardName);
            return Ok(new { message = "Dashboard deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dashboard customization");
            return StatusCode(500, new { error = "Failed to delete dashboard" });
        }
    }

    /// <summary>
    /// Set default dashboard
    /// </summary>
    [HttpPut("dashboards/{dashboardName}/default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDefaultDashboard(string dashboardName, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.SetDefaultDashboardAsync(userId, dashboardName, cancellationToken);

            if (!result)
                return BadRequest(new { error = "Failed to set default dashboard" });

            return Ok(new { message = "Default dashboard set" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default dashboard");
            return StatusCode(500, new { error = "Failed to set default dashboard" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
