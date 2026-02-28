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

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for UTM link tracking and campaign click analytics (MKT-005)
/// </summary>
[ApiController]
[Authorize]
public class UtmTrackingController : ControllerBase
{
    private readonly IUtmTrackingService _utmTrackingService;
    private readonly ILogger<UtmTrackingController> _logger;

    public UtmTrackingController(
        IUtmTrackingService utmTrackingService,
        ILogger<UtmTrackingController> logger)
    {
        _utmTrackingService = utmTrackingService;
        _logger = logger;
    }

    #region Tracking Links (MKT-005)

    /// <summary>
    /// Get all UTM tracking links for a campaign
    /// </summary>
    [HttpGet("api/campaigns/{campaignId}/links")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCampaignLinks(int campaignId, CancellationToken cancellationToken)
    {
        try
        {
            var links = await _utmTrackingService.GetCampaignLinksAsync(campaignId, cancellationToken);
            return Ok(links);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tracking links for campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error retrieving tracking links" });
        }
    }

    /// <summary>
    /// Create a new UTM tracking link for a campaign
    /// </summary>
    [HttpPost("api/campaigns/{campaignId}/links")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTrackingLink(
        int campaignId,
        [FromBody] CreateTrackingLinkDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var link = await _utmTrackingService.CreateTrackingLinkAsync(campaignId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetCampaignLinks), new { campaignId }, link);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tracking link for campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error creating tracking link" });
        }
    }

    /// <summary>
    /// Associate a click token with a lead (e.g. when the lead fills a form)
    /// </summary>
    [HttpPost("api/campaigns/links/{token}/associate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssociateLead(
        string token,
        [FromQuery] int leadId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _utmTrackingService.AssociateLeadAsync(token, leadId, cancellationToken);
            return Ok(new { message = "Lead associated with UTM click" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error associating lead {LeadId} with token {Token}", leadId, token);
            return StatusCode(500, new { message = "Error associating lead" });
        }
    }

    #endregion

    #region Public Click Redirect (MKT-005)

    /// <summary>
    /// Public click-tracking redirect — resolves token, records click, then 302 to destination (no auth)
    /// </summary>
    [HttpGet("api/track/{token}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TrackAndRedirect(string token, CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();

        try
        {
            var destinationUrl = await _utmTrackingService.ResolveAndTrackAsync(token, ip, ua, cancellationToken);
            if (string.IsNullOrEmpty(destinationUrl))
                return NotFound(new { message = "Tracking link not found or expired" });

            return Redirect(destinationUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tracking token {Token}", token);
            return StatusCode(500, new { message = "Error processing tracking link" });
        }
    }

    #endregion
}
