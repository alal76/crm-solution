// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for sales quota management.
/// </summary>
[ApiController]
[Route("api/sales-quotas")]
[Authorize]
[Produces("application/json")]
public class SalesQuotasController : ControllerBase
{
    private readonly ISalesQuotaService _service;
    private readonly ILogger<SalesQuotasController> _logger;

    public SalesQuotasController(ISalesQuotaService service, ILogger<SalesQuotasController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    /// <summary>Gets all sales quotas with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SalesQuota>>> GetAll(
        [FromQuery] int? userId = null,
        [FromQuery] int? teamId = null,
        [FromQuery] int? fiscalYear = null,
        [FromQuery] QuotaPeriodType? periodType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var quotas = await _service.GetAllAsync(userId, teamId, fiscalYear, periodType, cancellationToken);
            return Ok(quotas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales quotas");
            return Problem("An error occurred while retrieving sales quotas.");
        }
    }

    /// <summary>Gets a sales quota by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SalesQuota>> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var quota = await _service.GetByIdAsync(id, cancellationToken);
            if (quota == null)
                return NotFound($"Sales quota {id} not found");
            return Ok(quota);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales quota {QuotaId}", id);
            return Problem("An error occurred while retrieving the sales quota.");
        }
    }

    /// <summary>Creates a new sales quota.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SalesQuota>> Create([FromBody] SalesQuota quota, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var created = await _service.CreateAsync(quota, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sales quota");
            return Problem("An error occurred while creating the sales quota.");
        }
    }

    /// <summary>Updates an existing sales quota.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(int id, [FromBody] SalesQuota quota, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var updated = await _service.UpdateAsync(id, quota, cancellationToken);
            if (!updated)
                return NotFound($"Sales quota {id} not found");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sales quota {QuotaId}", id);
            return Problem("An error occurred while updating the sales quota.");
        }
    }

    /// <summary>Deletes a sales quota (soft delete).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return NotFound($"Sales quota {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sales quota {QuotaId}", id);
            return Problem("An error occurred while deleting the sales quota.");
        }
    }

    #endregion

    #region Quota-Specific Operations

    /// <summary>Gets quotas for a specific user and fiscal year.</summary>
    [HttpGet("by-user/{userId}/year/{fiscalYear}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SalesQuota>>> GetByUserAndYear(int userId, int fiscalYear, CancellationToken cancellationToken = default)
    {
        try
        {
            var quotas = await _service.GetByUserAndYearAsync(userId, fiscalYear, cancellationToken);
            return Ok(quotas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quotas for user {UserId} year {FiscalYear}", userId, fiscalYear);
            return Problem("An error occurred while retrieving quotas by user and year.");
        }
    }

    /// <summary>Gets quotas for a specific team and fiscal year.</summary>
    [HttpGet("by-team/{teamId}/year/{fiscalYear}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SalesQuota>>> GetByTeamAndYear(int teamId, int fiscalYear, CancellationToken cancellationToken = default)
    {
        try
        {
            var quotas = await _service.GetByTeamAndYearAsync(teamId, fiscalYear, cancellationToken);
            return Ok(quotas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quotas for team {TeamId} year {FiscalYear}", teamId, fiscalYear);
            return Problem("An error occurred while retrieving quotas by team and year.");
        }
    }

    /// <summary>Updates the actual attainment amount for a quota.</summary>
    [HttpPatch("{id}/attainment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateAttainment(int id, [FromBody] UpdateAttainmentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var updated = await _service.UpdateAttainmentAsync(id, request.ActualAmount, cancellationToken);
            if (!updated)
                return NotFound($"Sales quota {id} not found");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating attainment for quota {QuotaId}", id);
            return Problem("An error occurred while updating the quota attainment.");
        }
    }

    #endregion

    #region Request DTOs

    public class UpdateAttainmentRequest
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Actual amount must be non-negative")]
        public decimal ActualAmount { get; set; }
    }

    #endregion
}
