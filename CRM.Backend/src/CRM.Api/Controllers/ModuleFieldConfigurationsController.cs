using CRM.Core.Dtos;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ModuleFieldConfigurationsController : ControllerBase
{
    private readonly ModuleFieldConfigurationService _service;
    private readonly ILogger<ModuleFieldConfigurationsController> _logger;

    public ModuleFieldConfigurationsController(
        ModuleFieldConfigurationService service,
        ILogger<ModuleFieldConfigurationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all field configurations for a specific module
    /// </summary>
    [HttpGet("{moduleName}")]
    [ProducesResponseType(typeof(List<ModuleFieldConfigurationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFieldConfigurations(string moduleName)
    {
        try
        {
            var configs = await _service.GetFieldConfigurationsAsync(moduleName);
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field configurations for module {ModuleName}", moduleName);
            return StatusCode(500, new { message = "An error occurred while retrieving field configurations" });
        }
    }

    /// <summary>
    /// Get a specific field configuration by ID
    /// </summary>
    [HttpGet("config/{id}")]
    [ProducesResponseType(typeof(ModuleFieldConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFieldConfiguration(int id)
    {
        try
        {
            var config = await _service.GetFieldConfigurationAsync(id);
            if (config == null)
                return NotFound(new { message = "Field configuration not found" });

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field configuration {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the field configuration" });
        }
    }

    /// <summary>
    /// Create a new field configuration
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ModuleFieldConfigurationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateFieldConfiguration([FromBody] CreateModuleFieldConfigurationDto dto)
    {
        try
        {
            var result = await _service.CreateFieldConfigurationAsync(dto);
            return CreatedAtAction(nameof(GetFieldConfiguration), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating field configuration");
            return StatusCode(500, new { message = "An error occurred while creating the field configuration" });
        }
    }

    /// <summary>
    /// Update an existing field configuration
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ModuleFieldConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFieldConfiguration(int id, [FromBody] UpdateModuleFieldConfigurationDto dto)
    {
        try
        {
            var result = await _service.UpdateFieldConfigurationAsync(id, dto);
            if (result == null)
                return NotFound(new { message = "Field configuration not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating field configuration {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the field configuration" });
        }
    }

    /// <summary>
    /// Delete a field configuration
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFieldConfiguration(int id)
    {
        try
        {
            var result = await _service.DeleteFieldConfigurationAsync(id);
            if (!result)
                return NotFound(new { message = "Field configuration not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting field configuration {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the field configuration" });
        }
    }

    /// <summary>
    /// Bulk update field display order within a tab
    /// </summary>
    [HttpPost("bulk-update-order")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkUpdateFieldOrder([FromBody] BulkUpdateFieldOrderDto dto)
    {
        try
        {
            await _service.BulkUpdateFieldOrderAsync(dto);
            return Ok(new { message = "Field order updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating field order");
            return StatusCode(500, new { message = "An error occurred while updating field order" });
        }
    }

    /// <summary>
    /// Initialize default field configurations for a module
    /// </summary>
    [HttpPost("initialize/{moduleName}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InitializeDefaultConfigurations(string moduleName)
    {
        try
        {
            await _service.InitializeDefaultConfigurationsAsync(moduleName);
            return Ok(new { message = $"Default configurations initialized for module {moduleName}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default configurations for module {ModuleName}", moduleName);
            return StatusCode(500, new { message = "An error occurred while initializing default configurations" });
        }
    }

    /// <summary>
    /// Initialize default field configurations for all modules at once.
    /// This ensures fields are available without requiring users to visit each entity first.
    /// </summary>
    [HttpPost("initialize-all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InitializeAllModules()
    {
        try
        {
            var results = await _service.InitializeAllModulesAsync();
            var totalInitialized = results.Sum(r => r.Value);
            _logger.LogInformation("Initialized field configurations for all modules. Total fields: {Count}", totalInitialized);
            return Ok(new { 
                message = "Field configurations initialized for all modules",
                modules = results 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing field configurations for all modules");
            return StatusCode(500, new { message = "An error occurred while initializing field configurations" });
        }
    }
}
