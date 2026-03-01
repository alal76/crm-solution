// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API Controller for managing system changes and change management workflows.
/// Provides endpoints for change creation, approval, and tracking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChangesController : CrmControllerBase
{
    private const string ChangeNotFoundMessage = "Change with id {0} not found";
    private readonly IChangeService _service;
    private readonly ILogger<ChangesController> _logger;

    /// <summary>
    /// Initializes a new instance of the ChangesController.
    /// </summary>
    public ChangesController(
        IChangeService service,
        ILogger<ChangesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all changes with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedDto<ChangeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Getting all changes: page={Page}, pageSize={PageSize}, status={Status}",
            page, pageSize, status);
        var result = await _service.GetAllAsync(page, pageSize, status, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new change record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChangeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateChangeDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Creating new change: {ChangeTitle}", dto.Title);
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Gets a specific change by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ChangeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Getting change: id={Id}", id);
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(new { message = string.Format(ChangeNotFoundMessage, id) });

        return Ok(result);
    }

    /// <summary>
    /// Updates an existing change record.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ChangeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateChangeDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Updating change: id={Id}", id);
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (result == null)
            return NotFound(new { message = string.Format(ChangeNotFoundMessage, id) });

        return Ok(result);
    }

    /// <summary>
    /// Deletes a change record (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Deleting change: id={Id}", id);
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Submits a change for approval.
    /// </summary>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(ChangeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Submit(
        int id,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Submitting change for approval: id={Id}", id);
        var result = await _service.SubmitAsync(id, cancellationToken);
        if (result == null)
            return NotFound(new { message = string.Format(ChangeNotFoundMessage, id) });

        return Ok(result);
    }

    /// <summary>
    /// Approves a submitted change.
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(ChangeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Approve(
        int id,
        [FromBody] ChangeApprovalDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Approving change: id={Id}, approver={Approver}", id, dto.ApproverNotes);
        var result = await _service.ApproveAsync(id, dto, cancellationToken);
        if (result == null)
            return NotFound(new { message = string.Format(ChangeNotFoundMessage, id) });

        return Ok(result);
    }

    /// <summary>
    /// Rejects a submitted change.
    /// </summary>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(ChangeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Reject(
        int id,
        [FromBody] ChangeRejectionDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Rejecting change: id={Id}, reason={Reason}", id, dto.RejectionReason);
        var result = await _service.RejectAsync(id, dto, cancellationToken);
        if (result == null)
            return NotFound(new { message = string.Format(ChangeNotFoundMessage, id) });

        return Ok(result);
    }
}
