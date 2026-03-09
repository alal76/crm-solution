// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Partner portal API — exposes deal pipeline and shared resources to partner organisations.
/// Requires standard CRM authentication and the EnablePartnerPortal feature flag. PORTAL-025 / FLAG-002.
/// </summary>
[ApiController]
[Route("api/partner-portal")]
[Authorize]
[FeatureGate(FeatureFlags.EnablePartnerPortal)]
public sealed class PartnerPortalController : CrmControllerBase
{
    private readonly IPartnerPortalService _partnerPortal;
    private readonly ILogger<PartnerPortalController> _logger;

    public PartnerPortalController(
        IPartnerPortalService partnerPortal,
        ILogger<PartnerPortalController> logger)
    {
        _partnerPortal = partnerPortal;
        _logger = logger;
    }

    // ── Dashboard ────────────────────────────────────────────────────────────

    /// <summary>Returns dashboard summary for the current partner user. FLAG-002.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(PartnerDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _partnerPortal.GetDashboardAsync(userId, ct);
        return Ok(result);
    }

    // ── Deals ────────────────────────────────────────────────────────────────

    /// <summary>Returns all deals (opportunities) for the specified partner account.</summary>
    [HttpGet("deals")]
    [ProducesResponseType(typeof(IEnumerable<OpportunityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartnerDeals(
        [FromQuery] int partnerAccountId,
        CancellationToken ct)
    {
        if (partnerAccountId <= 0)
        {
            return BadRequest("partnerAccountId must be a positive integer.");
        }

        var result = await _partnerPortal.GetPartnerDealsAsync(partnerAccountId, ct);
        return Ok(result);
    }

    /// <summary>Registers a new deal on behalf of a partner.</summary>
    [HttpPost("deals")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterDeal(
        [FromBody] RegisterPartnerDealDto dto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _partnerPortal.RegisterDealAsync(dto, ct);
        _logger.LogInformation("Partner deal registered by {User}", User.Identity?.Name);
        return StatusCode(StatusCodes.Status201Created);
    }

    // ── Opportunities ────────────────────────────────────────────────────────

    /// <summary>Returns open opportunities for the specified partner account.</summary>
    [HttpGet("opportunities")]
    [ProducesResponseType(typeof(IEnumerable<OpportunityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartnerOpportunities(
        [FromQuery] int partnerAccountId,
        CancellationToken ct)
    {
        if (partnerAccountId <= 0)
        {
            return BadRequest("partnerAccountId must be a positive integer.");
        }

        var result = await _partnerPortal.GetPartnerOpportunitiesAsync(partnerAccountId, ct);
        return Ok(result);
    }

    // ── Resources ────────────────────────────────────────────────────────────

    /// <summary>Returns shared partner resources (documents, guides, links).</summary>
    [HttpGet("resources")]
    [ProducesResponseType(typeof(IEnumerable<PartnerResourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResources(CancellationToken ct)
    {
        var result = await _partnerPortal.GetResourcesAsync(ct);
        return Ok(result);
    }

    // ── Leads ────────────────────────────────────────────────────────────────

    /// <summary>Returns paginated leads owned by the current partner user. FLAG-002.</summary>
    [HttpGet("leads")]
    [ProducesResponseType(typeof(IEnumerable<PartnerLeadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeads(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var result = await _partnerPortal.GetLeadsAsync(userId, page, pageSize, ct);
        return Ok(result);
    }

    // ── Commissions ──────────────────────────────────────────────────────────

    /// <summary>Returns commission history for the current partner user. FLAG-002.</summary>
    [HttpGet("commissions")]
    [ProducesResponseType(typeof(IEnumerable<PartnerCommissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommissions(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _partnerPortal.GetCommissionsAsync(userId, ct);
        return Ok(result);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private int GetCurrentUserId() // NOSONAR
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
