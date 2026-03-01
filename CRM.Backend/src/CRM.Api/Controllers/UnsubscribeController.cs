// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing email unsubscribes and communication preferences (MKT-006)
/// </summary>
[ApiController]
[Route("api/unsubscribe")]
public class UnsubscribeController : CrmControllerBase
{
    private readonly IUnsubscribeService _unsubscribeService;
    private readonly ILogger<UnsubscribeController> _logger;

    public UnsubscribeController(
        IUnsubscribeService unsubscribeService,
        ILogger<UnsubscribeController> logger)
    {
        _unsubscribeService = unsubscribeService;
        _logger = logger;
    }

    /// <summary>
    /// Check unsubscribe status for an email address (requires auth)
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStatus([FromQuery] string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email address is required" });

                var status = await _unsubscribeService.GetStatusAsync(email, cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Unsubscribe an email address — public endpoint, no auth required (MKT-006)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequestDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

                var status = await _unsubscribeService.UnsubscribeAsync(dto, cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Update communication preferences — public (requires valid token), no auth needed (MKT-006)
    /// </summary>
    [HttpPut("preferences")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePreferences(
        [FromQuery] string email,
        [FromBody] UnsubscribeRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email address is required" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

                var status = await _unsubscribeService.UpdatePreferencesAsync(email, dto, cancellationToken);
        if (status is null)
            return NotFound(new { message = "No subscription record found for this email" });

        return Ok(status);
    }

    /// <summary>
    /// Generate a signed unsubscribe token for one-click unsubscribe links (requires auth)
    /// </summary>
    [HttpPost("generate-token")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateToken(
        [FromQuery] string email,
        [FromQuery] int? campaignId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email address is required" });

                var token = await _unsubscribeService.GenerateUnsubscribeTokenAsync(email, campaignId, cancellationToken);
        return Ok(new { token, email, campaignId });
    }
}
