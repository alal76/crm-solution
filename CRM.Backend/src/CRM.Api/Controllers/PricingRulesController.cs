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
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Pricing Rules API — provides CRUD for pricing rules and an ad-hoc price
/// calculation endpoint (TODO-GAP-07).
/// </summary>
[Authorize]
[ApiController]
[Route("api/pricingrules")]
[Produces("application/json")]
public class PricingRulesController : CrmControllerBase
{
    private readonly IPricingRulesService _service;

    public PricingRulesController(
        IPricingRulesService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    // ─── GET /api/pricingrules ───────────────────────────────────────────────

    /// <summary>Returns all active pricing rules.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PricingRule>), 200)]
    public async Task<IActionResult> GetActiveRules(CancellationToken cancellationToken)
    {
                var rules = await _service.GetActiveRulesAsync(cancellationToken);
        return Ok(rules);
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
        {
            return BadRequest(ModelState);
        }

                var created = await _service.CreateRuleAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetActiveRules), new { id = created.Id }, created);
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
        {
            return BadRequest(ModelState);
        }

        if (id != dto.Id)
        {
            return BadRequest(new { error = "Route id and body id must match" });
        }

                var updated = await _service.UpdateRuleAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Pricing rule {id} not found" });
        }

        return Ok(updated);
    }

    // ─── DELETE /api/pricingrules/{id} ───────────────────────────────────────

    /// <summary>Soft-deletes a pricing rule.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteRule(int id, CancellationToken cancellationToken)
    {
                var deleted = await _service.DeleteRuleAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { error = $"Pricing rule {id} not found" });
        }

        return NoContent();
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
        {
            return BadRequest(ModelState);
        }

        if (request.ProductId <= 0)
        {
            return BadRequest(new { error = "ProductId must be a positive integer" });
        }

        if (request.Quantity <= 0)
        {
            return BadRequest(new { error = "Quantity must be at least 1" });
        }

                var breakdown = await _service.GetPriceBreakdownAsync(request, cancellationToken);
        return Ok(breakdown);
    }
}
