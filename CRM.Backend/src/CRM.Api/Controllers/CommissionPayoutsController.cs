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

namespace CRM.Api.Controllers;

/// <summary>
/// API Controller for managing commission payouts.
/// Provides endpoints for payout operations and reconciliation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommissionPayoutsController : ControllerBase
{
    private const string PayoutNotFoundMessage = "Commission payout with id {0} not found";
    private readonly ICommissionPayoutService _service;
    private readonly ILogger<CommissionPayoutsController> _logger;

    /// <summary>
    /// Initializes a new instance of the CommissionPayoutsController.
    /// </summary>
    public CommissionPayoutsController(
        ICommissionPayoutService service,
        ILogger<CommissionPayoutsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Marks a commission payout as paid.
    /// </summary>
    [HttpPost("{id}/mark-paid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkPaid(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Marking commission payout as paid: id={Id}", id);
            var result = await _service.MarkPaidAsync(id, paidDate: null, reference: null, cancellationToken: cancellationToken);
            if (!result)
                return NotFound(new { message = string.Format(PayoutNotFoundMessage, id) });

            return Ok(new { message = "Commission payout marked as paid successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payout as paid: id={Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Claws back a commission payout.
    /// </summary>
    [HttpPost("{id}/clawback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Clawback(
        int id,
        [FromBody] CommissionClawbackDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing clawback: payoutId={PayoutId}, reason={Reason}", id, dto.Reason);
            var result = await _service.ClawbackAsync(id, dto.Reason, dto.ClawbackAmount, cancellationToken);
            if (!result)
                return NotFound(new { message = string.Format(PayoutNotFoundMessage, id) });

            return Ok(new { message = "Commission payout clawed back successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing clawback: id={Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Generates a commission statement for a user.
    /// </summary>
    [HttpGet("{userId}/statement")]
    [ProducesResponseType(typeof(CommissionStatementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateStatement(
        int userId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating commission statement: userId={UserId}, startDate={StartDate}, endDate={EndDate}",
                userId, startDate, endDate);
            var from = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var to = endDate ?? DateTime.UtcNow;
            var result = await _service.GenerateStatementAsync(userId, from, to, cancellationToken);
            if (result == null)
                return NotFound(new { message = $"No commission data found for user {userId}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating statement: userId={UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Finalizes a commission payout.
    /// </summary>
    [HttpPost("{id}/finalize")]
    [ProducesResponseType(typeof(CommissionPayoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Finalize(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Finalizing commission payout: id={Id}", id);
            var result = await _service.FinalizeAsync(id, cancellationToken);
            if (result == null)
                return NotFound(new { message = string.Format(PayoutNotFoundMessage, id) });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing commission payout: id={Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reconciles a commission payout with accounting records.
    /// </summary>
    [HttpPost("{id}/reconcile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Reconcile(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Reconciling commission payout: id={Id}", id);
            var result = await _service.ReconcileAsync(id, cancellationToken);
            if (!result)
                return NotFound(new { message = string.Format(PayoutNotFoundMessage, id) });

            return Ok(new { message = "Commission payout reconciled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling commission payout: id={Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}
