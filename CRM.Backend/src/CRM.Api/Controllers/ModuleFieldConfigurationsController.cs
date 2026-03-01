// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ModuleFieldConfigurationsController : CrmControllerBase
{
    private const string FieldConfigNotFoundMessage = "Field configuration not found";

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
                var configs = await _service.GetFieldConfigurationsAsync(moduleName);
        return Ok(configs);
    }

    /// <summary>
    /// Get a specific field configuration by ID
    /// </summary>
    [HttpGet("config/{id}")]
    [ProducesResponseType(typeof(ModuleFieldConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFieldConfiguration(int id)
    {
                var config = await _service.GetFieldConfigurationAsync(id);
        if (config == null)
        {
            return NotFound(new { message = FieldConfigNotFoundMessage });
        }

        return Ok(config);
    }

    /// <summary>
    /// Create a new field configuration
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ModuleFieldConfigurationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateFieldConfiguration([FromBody] CreateModuleFieldConfigurationDto dto)
    {
                var result = await _service.CreateFieldConfigurationAsync(dto);
        return CreatedAtAction(nameof(GetFieldConfiguration), new { id = result.Id }, result);
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
                var result = await _service.UpdateFieldConfigurationAsync(id, dto);
        if (result == null)
        {
            return NotFound(new { message = FieldConfigNotFoundMessage });
        }

        return Ok(result);
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
                var result = await _service.DeleteFieldConfigurationAsync(id);
        if (!result)
        {
            return NotFound(new { message = FieldConfigNotFoundMessage });
        }

        return NoContent();
    }

    /// <summary>
    /// Bulk update field display order within a tab
    /// </summary>
    [HttpPost("bulk-update-order")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkUpdateFieldOrder([FromBody] BulkUpdateFieldOrderDto dto)
    {
                await _service.BulkUpdateFieldOrderAsync(dto);
        return Ok(new { message = "Field order updated successfully" });
    }

    /// <summary>
    /// Initialize default field configurations for a module
    /// </summary>
    [HttpPost("initialize/{moduleName}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InitializeDefaultConfigurations(string moduleName)
    {
                await _service.InitializeDefaultConfigurationsAsync(moduleName);
        return Ok(new { message = $"Default configurations initialized for module {moduleName}" });
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
                var results = await _service.InitializeAllModulesAsync();
        var totalInitialized = results.Sum(r => r.Value);
        _logger.LogInformation("Initialized field configurations for all modules. Total fields: {Count}", totalInitialized);
        return Ok(new
        {
            message = "Field configurations initialized for all modules",
            modules = results
        });
    }
}
