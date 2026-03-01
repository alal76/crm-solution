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

using CommissionTierDto = CRM.Core.Dtos.CommissionTierDto;
namespace CRM.Api.Controllers;

/// <summary>
/// API Controller for managing commission plans.
/// Provides endpoints for CRUD operations and plan management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommissionPlansController : CrmControllerBase
{
    private const string PlanNotFoundMessage = "Commission plan with id {0} not found";

    private readonly ICommissionPlanService _service;
    private readonly ILogger<CommissionPlansController> _logger;

    /// <summary>
    /// Initializes a new instance of the CommissionPlansController.
    /// </summary>
    public CommissionPlansController(
        ICommissionPlanService service,
        ILogger<CommissionPlansController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all commission plans with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CommissionPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Getting all commission plans: page={Page}, pageSize={PageSize}", page, pageSize);
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific commission plan by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CommissionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Getting commission plan: id={Id}", id);
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = string.Format(PlanNotFoundMessage, id) });
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new commission plan.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CommissionPlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCommissionPlanDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Creating new commission plan: {PlanName}", dto.Name);
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing commission plan.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CommissionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCommissionPlanDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Updating commission plan: id={Id}", id);
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = string.Format(PlanNotFoundMessage, id) });
        }

        return Ok(result);
    }

    /// <summary>
    /// Deletes a commission plan (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Deleting commission plan: id={Id}", id);
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Activates a commission plan.
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Activate(
        int id,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Activating commission plan: id={Id}", id);
        var result = await _service.ActivateAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(new { message = string.Format(PlanNotFoundMessage, id) });
        }

        return Ok(new { message = "Commission plan activated successfully" });
    }

    /// <summary>
    /// Deactivates a commission plan.
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Deactivate(
        int id,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Deactivating commission plan: id={Id}", id);
        var result = await _service.DeactivateAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(new { message = string.Format(PlanNotFoundMessage, id) });
        }

        return Ok(new { message = "Commission plan deactivated successfully" });
    }

    /// <summary>
    /// Assigns a commission plan to a user.
    /// </summary>
    [HttpPost("{id}/assign/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignToUser(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Assigning commission plan to user: planId={PlanId}, userId={UserId}", id, userId);
        await _service.AssignToUserAsync(id, userId, effectiveDate: null, cancellationToken: cancellationToken);
        return Ok(new { message = "Commission plan assigned successfully" });
    }

    /// <summary>
    /// Unassigns a commission plan from a user.
    /// </summary>
    [HttpDelete("{id}/assign/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnassignFromUser(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Unassigning commission plan from user: planId={PlanId}, userId={UserId}", id, userId);
        await _service.UnassignFromUserAsync(id, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets all tiers for a commission plan.
    /// </summary>
    [HttpGet("{id}/tiers")]
    [ProducesResponseType(typeof(List<CommissionTierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTiers(
        int id,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Getting tiers for commission plan: planId={PlanId}", id);
        var result = await _service.GetTiersAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = string.Format(PlanNotFoundMessage, id) });
        }

        return Ok(result);
    }

    /// <summary>
    /// Adds a new tier to a commission plan.
    /// </summary>
    [HttpPost("{id}/tiers")]
    [ProducesResponseType(typeof(CommissionTierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddTier(
        int id,
        [FromBody] CreateCommissionTierDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Adding tier to commission plan: planId={PlanId}", id);
        var result = await _service.AddTierAsync(id, dto, cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = string.Format(PlanNotFoundMessage, id) });
        }

        return CreatedAtAction(nameof(GetTiers), new { id }, result);
    }

    /// <summary>
    /// Updates a tier in a commission plan.
    /// </summary>
    [HttpPut("{id}/tiers/{tierId}")]
    [ProducesResponseType(typeof(CommissionTierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTier(
        int id,
        int tierId,
        [FromBody] UpdateCommissionTierDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Updating tier: planId={PlanId}, tierId={TierId}", id, tierId);
        var result = await _service.UpdateTierAsync(tierId, dto, cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Tier with id {tierId} not found in plan {id}" });
        }

        return Ok(result);
    }
}
