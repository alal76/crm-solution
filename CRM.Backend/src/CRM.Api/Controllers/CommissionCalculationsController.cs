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
/// API Controller for calculating commissions.
/// Provides endpoints for various commission calculation scenarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommissionCalculationsController : CrmControllerBase
{
    private readonly ICommissionCalculationService _service;
    private readonly ILogger<CommissionCalculationsController> _logger;

    /// <summary>
    /// Initializes a new instance of the CommissionCalculationsController.
    /// </summary>
    public CommissionCalculationsController(
        ICommissionCalculationService service,
        ILogger<CommissionCalculationsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates commission for a deal (opportunity).
    /// </summary>
    [HttpPost("deal")]
    [ProducesResponseType(typeof(CommissionCalculationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CalculateForDeal(
        [FromBody] CommissionDealCalculationDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Calculating commission for deal: opportunityId={OpportunityId}", dto.OpportunityId);
        var result = await _service.CalculateForDealAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Calculates commission for an order.
    /// </summary>
    [HttpPost("order")]
    [ProducesResponseType(typeof(CommissionCalculationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CalculateForOrder(
        [FromBody] CommissionOrderCalculationDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Calculating commission for order: orderId={OrderId}", dto.OrderId);
        var result = await _service.CalculateForOrderAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Calculates commissions for a period (monthly, quarterly, yearly).
    /// </summary>
    [HttpPost("period")]
    [ProducesResponseType(typeof(CommissionPeriodCalculationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CalculateForPeriod(
        [FromBody] CommissionPeriodCalculationDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Calculating commissions for period: startDate={StartDate}, endDate={EndDate}",
            dto.StartDate, dto.EndDate);
        var result = await _service.CalculateForPeriodAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Validates commission calculation parameters before processing.
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(CommissionValidationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Validate(
        [FromBody] CommissionCalculationValidationDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Validating commission calculation parameters");
        var result = await _service.ValidateAsync(dto, cancellationToken);
        return Ok(result);
    }
}
