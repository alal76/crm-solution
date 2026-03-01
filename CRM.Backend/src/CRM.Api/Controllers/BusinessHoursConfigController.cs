// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.API.Controllers;

/// <summary>
/// API for managing business hours configurations.
/// TODO-SYS005-001
/// </summary>
[ApiController]
[Route("api/system/business-hours")]
[Authorize]
public class BusinessHoursConfigController : CrmControllerBase
{
    private const string ConfigNotFoundMessage = "Business hours configuration {0} not found";
    private readonly IBusinessHoursConfigService _service;
    private readonly ILogger<BusinessHoursConfigController> _logger;

    public BusinessHoursConfigController(
        IBusinessHoursConfigService service,
        ILogger<BusinessHoursConfigController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>List all business hours configurations.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BusinessHoursConfigDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BusinessHoursConfigDto>>> GetAll(CancellationToken ct)
    {
                var configs = await _service.GetAllAsync(ct);
        return Ok(configs);
    }

    /// <summary>Get a specific business hours configuration.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BusinessHoursConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessHoursConfigDto>> GetById(int id, CancellationToken ct)
    {
                var config = await _service.GetByIdAsync(id, ct);
        if (config == null)
        {
            return NotFound(string.Format(ConfigNotFoundMessage, id));
        }
        return Ok(config);
    }

    /// <summary>Create a new business hours configuration.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BusinessHoursConfigDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BusinessHoursConfigDto>> Create(
        [FromBody] BusinessHoursConfigRequest request, CancellationToken ct)
    {
                var config = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = config.Id }, config);
    }

    /// <summary>Update an existing business hours configuration.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BusinessHoursConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessHoursConfigDto>> Update(
        int id, [FromBody] BusinessHoursConfigRequest request, CancellationToken ct)
    {
        try
        {
            var config = await _service.UpdateAsync(id, request, ct);
            if (config == null)
            {
                return NotFound(string.Format(ConfigNotFoundMessage, id));
            }
            return Ok(config);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation updating business hours {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Soft-delete a business hours configuration.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
                var deleted = await _service.DeleteAsync(id, ct);
        if (!deleted)
        {
            return NotFound(string.Format(ConfigNotFoundMessage, id));
        }
        return NoContent();
    }

    /// <summary>Set a configuration as the default.</summary>
    [HttpPost("{id:int}/set-default")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BusinessHoursConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessHoursConfigDto>> SetDefault(int id, CancellationToken ct)
    {
                var config = await _service.SetDefaultAsync(id, ct);
        if (config == null)
        {
            return NotFound(string.Format(ConfigNotFoundMessage, id));
        }
        return Ok(config);
    }
}
