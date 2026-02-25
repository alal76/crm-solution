// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Pricing Rules API — provides CRUD for pricing rules and an ad-hoc price
/// calculation endpoint (TODO-GAP-07).
/// </summary>
[Authorize]
[ApiController]
[Route("api/pricingrules")]
[Produces("application/json")]
public class PricingRulesController : ControllerBase
{
    private readonly IPricingRulesService _service;
    private readonly ILogger<PricingRulesController> _logger;

    public PricingRulesController(
        IPricingRulesService service,
        ILogger<PricingRulesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─── GET /api/pricingrules ───────────────────────────────────────────────

    /// <summary>Returns all active pricing rules.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PricingRule>), 200)]
    public async Task<IActionResult> GetActiveRules(CancellationToken cancellationToken)
    {
        try
        {
            var rules = await _service.GetActiveRulesAsync(cancellationToken);
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active pricing rules");
            return StatusCode(500, new { error = "An error occurred retrieving pricing rules" });
        }
    }

    // ─── POST /api/pricingrules ──────────────────────────────────────────────

    /// <summary>Creates a new pricing rule.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PricingRule), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateRule(
        [FromBody] CreatePricingRuleDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var created = await _service.CreateRuleAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetActiveRules), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pricing rule");
            return StatusCode(500, new { error = "An error occurred creating the pricing rule" });
        }
    }

    // ─── PUT /api/pricingrules/{id} ──────────────────────────────────────────

    /// <summary>Updates an existing pricing rule.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PricingRule), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateRule(
        int id,
        [FromBody] UpdatePricingRuleDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.Id)
            return BadRequest(new { error = "Route id and body id must match" });

        try
        {
            var updated = await _service.UpdateRuleAsync(id, dto, cancellationToken);
            if (updated == null)
                return NotFound(new { error = $"Pricing rule {id} not found" });

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pricing rule {Id}", id);
            return StatusCode(500, new { error = "An error occurred updating the pricing rule" });
        }
    }

    // ─── DELETE /api/pricingrules/{id} ───────────────────────────────────────

    /// <summary>Soft-deletes a pricing rule.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteRule(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _service.DeleteRuleAsync(id, cancellationToken);
            if (!deleted)
                return NotFound(new { error = $"Pricing rule {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting pricing rule {Id}", id);
            return StatusCode(500, new { error = "An error occurred deleting the pricing rule" });
        }
    }

    // ─── POST /api/pricingrules/calculate ────────────────────────────────────

    /// <summary>
    /// Ad-hoc price calculation — returns a full price breakdown for a
    /// given product, quantity, customer and optional promo code.
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(PriceBreakdownDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Calculate(
        [FromBody] PriceCalculationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.ProductId <= 0)
            return BadRequest(new { error = "ProductId must be a positive integer" });

        if (request.Quantity <= 0)
            return BadRequest(new { error = "Quantity must be at least 1" });

        try
        {
            var breakdown = await _service.GetPriceBreakdownAsync(request, cancellationToken);
            return Ok(breakdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price for product {ProductId}", request.ProductId);
            return StatusCode(500, new { error = "An error occurred calculating the price" });
        }
    }
}
