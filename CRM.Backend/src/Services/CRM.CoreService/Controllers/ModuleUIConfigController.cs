using CRM.Core.Dtos;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ModuleUIConfigController : ControllerBase
{
    private readonly ModuleUIConfigService _service;
    private readonly ILogger<ModuleUIConfigController> _logger;

    public ModuleUIConfigController(
        ModuleUIConfigService service,
        ILogger<ModuleUIConfigController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all module UI configurations
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ModuleUIConfigDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllModuleConfigs()
    {
        try
        {
            var configs = await _service.GetAllModuleConfigsAsync();
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all module UI configurations");
            return StatusCode(500, new { message = "An error occurred while retrieving module configurations" });
        }
    }

    /// <summary>
    /// Get module UI configuration by module name
    /// </summary>
    [HttpGet("{moduleName}")]
    [ProducesResponseType(typeof(ModuleUIConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetModuleConfig(string moduleName)
    {
        try
        {
            var config = await _service.GetModuleConfigAsync(moduleName);
            if (config == null)
                return NotFound(new { message = $"Module configuration not found for {moduleName}" });

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting module UI configuration for {ModuleName}", moduleName);
            return StatusCode(500, new { message = "An error occurred while retrieving module configuration" });
        }
    }

    /// <summary>
    /// Get complete module configuration including field configurations
    /// </summary>
    [HttpGet("{moduleName}/complete")]
    [ProducesResponseType(typeof(CompleteModuleConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompleteModuleConfig(string moduleName)
    {
        try
        {
            var config = await _service.GetCompleteModuleConfigAsync(moduleName);
            if (config == null)
                return NotFound(new { message = $"Module configuration not found for {moduleName}" });

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting complete module configuration for {ModuleName}", moduleName);
            return StatusCode(500, new { message = "An error occurred while retrieving module configuration" });
        }
    }

    /// <summary>
    /// Create a new module UI configuration
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ModuleUIConfigDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateModuleConfig([FromBody] CreateModuleUIConfigDto dto)
    {
        try
        {
            var result = await _service.CreateModuleConfigAsync(dto);
            return CreatedAtAction(nameof(GetModuleConfig), new { moduleName = result.ModuleName }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating module UI configuration");
            return StatusCode(500, new { message = "An error occurred while creating module configuration" });
        }
    }

    /// <summary>
    /// Update an existing module UI configuration
    /// </summary>
    [HttpPut("{moduleName}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ModuleUIConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateModuleConfig(string moduleName, [FromBody] UpdateModuleUIConfigDto dto)
    {
        try
        {
            var result = await _service.UpdateModuleConfigAsync(moduleName, dto);
            if (result == null)
                return NotFound(new { message = $"Module configuration not found for {moduleName}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating module UI configuration for {ModuleName}", moduleName);
            return StatusCode(500, new { message = "An error occurred while updating module configuration" });
        }
    }

    /// <summary>
    /// Batch update module configurations (for enable/disable and reordering)
    /// </summary>
    [HttpPut("batch")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<ModuleUIConfigDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchUpdateModules([FromBody] BatchModuleUIConfigUpdateDto dto)
    {
        try
        {
            var result = await _service.BatchUpdateModulesAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch updating module configurations");
            return StatusCode(500, new { message = "An error occurred while updating module configurations" });
        }
    }

    /// <summary>
    /// Update linked entities configuration for a module
    /// </summary>
    [HttpPut("{moduleName}/linked-entities")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ModuleUIConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLinkedEntities(string moduleName, [FromBody] List<LinkedEntityConfigItem> linkedEntities)
    {
        try
        {
            var result = await _service.UpdateLinkedEntitiesAsync(moduleName, linkedEntities);
            if (result == null)
                return NotFound(new { message = $"Module configuration not found for {moduleName}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating linked entities for {ModuleName}", moduleName);
            return StatusCode(500, new { message = "An error occurred while updating linked entities" });
        }
    }

    /// <summary>
    /// Update tabs configuration for a module
    /// </summary>
    [HttpPut("{moduleName}/tabs")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ModuleUIConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTabsConfig(string moduleName, [FromBody] List<TabConfigItem> tabs)
    {
        try
        {
            var result = await _service.UpdateTabsConfigAsync(moduleName, tabs);
            if (result == null)
                return NotFound(new { message = $"Module configuration not found for {moduleName}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tabs config for {ModuleName}", moduleName);
            return StatusCode(500, new { message = "An error occurred while updating tabs configuration" });
        }
    }

    /// <summary>
    /// Save complete module configuration (tabs, fields, linked entities) in one call
    /// </summary>
    [HttpPut("{moduleName}/complete")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CompleteModuleConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveCompleteModuleConfig(string moduleName, [FromBody] SaveCompleteModuleConfigDto dto)
    {
        try
        {
            var result = await _service.SaveCompleteModuleConfigAsync(moduleName, dto);
            if (result == null)
                return NotFound(new { message = $"Module configuration not found for {moduleName}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving complete module configuration for {ModuleName}", moduleName);
            return StatusCode(500, new { message = "An error occurred while saving module configuration" });
        }
    }

    /// <summary>
    /// Reset module configuration to defaults
    /// </summary>
    [HttpPost("{moduleName}/reset-defaults")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CompleteModuleConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetModuleToDefaults(string moduleName)
    {
        try
        {
            var result = await _service.ResetModuleToDefaultsAsync(moduleName);
            if (result == null)
                return NotFound(new { message = $"Module configuration not found for {moduleName}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting module {ModuleName} to defaults", moduleName);
            return StatusCode(500, new { message = "An error occurred while resetting module to defaults" });
        }
    }

    /// <summary>
    /// Initialize default module configurations
    /// </summary>
    [HttpPost("initialize")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InitializeDefaultConfigs()
    {
        try
        {
            await _service.InitializeDefaultConfigsAsync();
            return Ok(new { message = "Default module configurations initialized successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default module configurations");
            return StatusCode(500, new { message = "An error occurred while initializing default configurations" });
        }
    }

    /// <summary>
    /// Toggle module enabled status
    /// </summary>
    [HttpPost("{moduleName}/toggle")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ModuleUIConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleModule(string moduleName, [FromQuery] bool enabled)
    {
        try
        {
            var result = await _service.ToggleModuleAsync(moduleName, enabled);
            if (result == null)
                return NotFound(new { message = $"Module configuration not found for {moduleName}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling module {ModuleName}", moduleName);
            return StatusCode(500, new { message = "An error occurred while toggling module" });
        }
    }
}
