// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API Controller for managing ITSM change types.
/// Provides endpoints for CRUD operations on change type configurations.
/// </summary>
[ApiController]
[Route("api/change-types")]
[Authorize]
public class ChangeTypesController : ControllerBase
{
    private readonly IChangeTypeService _service;
    private readonly ILogger<ChangeTypesController> _logger;

    /// <summary>
    /// Initializes a new instance of the ChangeTypesController.
    /// </summary>
    public ChangeTypesController(
        IChangeTypeService service,
        ILogger<ChangeTypesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all change types.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive change types</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of change types</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ChangeTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all change types, includeInactive={IncludeInactive}", includeInactive);
            var result = await _service.GetAllAsync(includeInactive, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting change types");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific change type by ID.
    /// </summary>
    /// <param name="id">The change type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The change type</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ChangeTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting change type: id={Id}", id);
            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (result == null)
                return NotFound(new { message = $"Change type with id {id} not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting change type {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new change type.
    /// </summary>
    /// <param name="dto">The change type data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created change type</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ChangeTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateChangeTypeDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Creating new change type: {TypeName}", dto.TypeName);
            var result = await _service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating change type");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating change type");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing change type.
    /// </summary>
    /// <param name="id">The change type ID</param>
    /// <param name="dto">The update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated change type</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ChangeTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateChangeTypeDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Updating change type: id={Id}", id);
            var result = await _service.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating change type");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating change type {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a change type (soft delete).
    /// </summary>
    /// <param name="id">The change type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting change type: id={Id}", id);
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting change type {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}
