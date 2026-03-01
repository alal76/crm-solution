// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Authorization;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing user UI preferences and customizations
/// </summary>
[ApiController]
[Route("api/ui-preferences")]
public class UIPreferencesController : CrmControllerBase
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
                var userId = GetCurrentUserId();
        var prefs = await _service.GetUserUIPreferencesAsync(userId, cancellationToken);

        if (prefs == null)
            return NotFound(new { error = "UI preferences not found" });

        return Ok(prefs);
    }

    /// <summary>
    /// Save UI preferences for the current user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UIPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UIPreferenceDto>> SaveUIPreferences(CreateUpdateUIPreferenceDto dto, CancellationToken cancellationToken = default)
    {
                var userId = GetCurrentUserId();
        var result = await _service.SaveUIPreferencesAsync(userId, dto, cancellationToken);

        _logger.LogInformation("UI preferences saved for user {UserId}", userId);
        return Ok(result);
    }

    /// <summary>
    /// Reset UI preferences to defaults
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetUIPreferences(CancellationToken cancellationToken = default)
    {
                var userId = GetCurrentUserId();
        var result = await _service.ResetUIPreferencesAsync(userId, cancellationToken);

        if (!result)
            return BadRequest(new { error = "Failed to reset preferences" });

        _logger.LogInformation("UI preferences reset for user {UserId}", userId);
        return Ok(new { message = "UI preferences reset to defaults" });
    }

    /// <summary>
    /// Get UI customization for a module/page
    /// </summary>
    [HttpGet("customizations/{moduleName}/{pageName}")]
    [ProducesResponseType(typeof(UICustomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UICustomizationDto>> GetUICustomization(string moduleName, string pageName, CancellationToken cancellationToken = default)
    {
                var userId = GetCurrentUserId();
        var customization = await _service.GetUICustomizationAsync(userId, moduleName, pageName, cancellationToken);

        if (customization == null)
            return NotFound(new { error = "Customization not found" });

        return Ok(customization);
    }

    /// <summary>
    /// Get all UI customizations for the current user
    /// </summary>
    [HttpGet("customizations")]
    [ProducesResponseType(typeof(IEnumerable<UICustomizationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UICustomizationDto>>> GetAllUICustomizations(CancellationToken cancellationToken = default)
    {
                var userId = GetCurrentUserId();
        var customizations = await _service.GetAllUICustomizationsAsync(userId, cancellationToken);

        return Ok(customizations);
    }

    /// <summary>
    /// Save UI customization for a module/page
    /// </summary>
    [HttpPost("customizations")]
    [ProducesResponseType(typeof(UICustomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UICustomizationDto>> SaveUICustomization(CreateUpdateUICustomizationDto dto, CancellationToken cancellationToken = default)
    {
                if (string.IsNullOrEmpty(dto.ModuleName) || string.IsNullOrEmpty(dto.PageName))
            return BadRequest(new { error = "ModuleName and PageName are required" });

        var userId = GetCurrentUserId();
        var result = await _service.SaveUICustomizationAsync(userId, dto, cancellationToken);

        _logger.LogInformation("UI customization saved for user {UserId}, module {Module}", userId, dto.ModuleName);
        return Ok(result);
    }

    /// <summary>
    /// Delete UI customization
    /// </summary>
    [HttpDelete("customizations/{moduleName}/{pageName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteUICustomization(string moduleName, string pageName, CancellationToken cancellationToken = default)
    {
                var userId = GetCurrentUserId();
        var result = await _service.DeleteUICustomizationAsync(userId, moduleName, pageName, cancellationToken);

        if (!result)
            return BadRequest(new { error = "Failed to delete customization" });

        _logger.LogInformation("UI customization deleted for user {UserId}, module {Module}", userId, moduleName);
        return Ok(new { message = "Customization deleted" });
    }

    /// <summary>
    /// Get dashboard customization
    /// </summary>
    [HttpGet("dashboards/{dashboardName}")]
    [ProducesResponseType(typeof(DashboardCustomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardCustomizationDto>> GetDashboardCustomization(string dashboardName, CancellationToken cancellationToken = default)
    {
                var userId = GetCurrentUserId();
        var dashboard = await _service.GetDashboardCustomizationAsync(userId, dashboardName, cancellationToken);

        if (dashboard == null)
            return NotFound(new { error = "Dashboard not found" });

        return Ok(dashboard);
    }

    /// <summary>
    /// Get all dashboard customizations
    /// </summary>
    [HttpGet("dashboards")]
    [ProducesResponseType(typeof(IEnumerable<DashboardCustomizationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DashboardCustomizationDto>>> GetAllDashboardCustomizations(CancellationToken cancellationToken = default)
    {
                var userId = GetCurrentUserId();
        var dashboards = await _service.GetAllDashboardCustomizationsAsync(userId, cancellationToken);

        return Ok(dashboards);
    }

    /// <summary>
    /// Save dashboard customization
    /// </summary>
    [HttpPost("dashboards")]
    [ProducesResponseType(typeof(DashboardCustomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DashboardCustomizationDto>> SaveDashboardCustomization(CreateUpdateDashboardCustomizationDto dto, CancellationToken cancellationToken = default)
    {
                if (string.IsNullOrEmpty(dto.DashboardName))
            return BadRequest(new { error = "DashboardName is required" });

        var userId = GetCurrentUserId();
        var result = await _service.SaveDashboardCustomizationAsync(userId, dto, cancellationToken);

        _logger.LogInformation("Dashboard saved for user {UserId}: {DashboardName}", userId, dto.DashboardName);
        return Ok(result);
    }

    /// <summary>
    /// Delete dashboard customization
    /// </summary>
    [HttpDelete("dashboards/{dashboardName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteDashboardCustomization(string dashboardName, CancellationToken cancellationToken = default)
    {
                var userId = GetCurrentUserId();
        var result = await _service.DeleteDashboardCustomizationAsync(userId, dashboardName, cancellationToken);

        if (!result)
            return BadRequest(new { error = "Failed to delete dashboard" });

        _logger.LogInformation("Dashboard deleted for user {UserId}: {DashboardName}", userId, dashboardName);
        return Ok(new { message = "Dashboard deleted" });
    }

    /// <summary>
    /// Set default dashboard
    /// </summary>
    [HttpPut("dashboards/{dashboardName}/default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDefaultDashboard(string dashboardName, CancellationToken cancellationToken = default)
    {
                var userId = GetCurrentUserId();
        var result = await _service.SetDefaultDashboardAsync(userId, dashboardName, cancellationToken);

        if (!result)
            return BadRequest(new { error = "Failed to set default dashboard" });

        return Ok(new { message = "Default dashboard set" });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
