using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.API.Controllers;

/// <summary>
/// Controller for managing system settings
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _settingsService;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(ISystemSettingsService settingsService, ILogger<SystemSettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Get current system settings (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> GetSettings()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system settings");
            return StatusCode(500, "Error retrieving system settings");
        }
    }

    /// <summary>
    /// Get module status for permission checking (all authenticated users)
    /// </summary>
    [HttpGet("modules")]
    public async Task<ActionResult<ModuleStatusDto>> GetModuleStatus()
    {
        try
        {
            var moduleStatus = await _settingsService.GetModuleStatusAsync();
            return Ok(moduleStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module status");
            return StatusCode(500, "Error retrieving module status");
        }
    }

    /// <summary>
    /// Update system settings (admin only)
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> UpdateSettings([FromBody] UpdateSystemSettingsRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;
            
            var settings = await _settingsService.UpdateSettingsAsync(request, userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system settings");
            return StatusCode(500, "Error updating system settings");
        }
    }

    /// <summary>
    /// Toggle a specific module on/off (admin only)
    /// </summary>
    [HttpPost("modules/{moduleName}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> ToggleModule(string moduleName, [FromQuery] bool enabled)
    {
        try
        {
            var request = new UpdateSystemSettingsRequest();
            
            switch (moduleName.ToLower())
            {
                case "customers": request.CustomersEnabled = enabled; break;
                case "contacts": request.ContactsEnabled = enabled; break;
                case "leads": request.LeadsEnabled = enabled; break;
                case "opportunities": request.OpportunitiesEnabled = enabled; break;
                case "products": request.ProductsEnabled = enabled; break;
                case "services": request.ServicesEnabled = enabled; break;
                case "campaigns": request.CampaignsEnabled = enabled; break;
                case "quotes": request.QuotesEnabled = enabled; break;
                case "tasks": request.TasksEnabled = enabled; break;
                case "activities": request.ActivitiesEnabled = enabled; break;
                case "notes": request.NotesEnabled = enabled; break;
                case "workflows": request.WorkflowsEnabled = enabled; break;
                case "reports": request.ReportsEnabled = enabled; break;
                case "dashboard": request.DashboardEnabled = enabled; break;
                default:
                    return BadRequest($"Unknown module: {moduleName}");
            }
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;
            
            var settings = await _settingsService.UpdateSettingsAsync(request, userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling module {ModuleName}", moduleName);
            return StatusCode(500, $"Error toggling module {moduleName}");
        }
    }
}
