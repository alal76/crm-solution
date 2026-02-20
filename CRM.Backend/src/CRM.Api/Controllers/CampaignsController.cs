// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing marketing campaigns.
/// Provides endpoints for CRUD operations and campaign metrics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CampaignsController : ControllerBase
{
    private readonly IMarketingCampaignService _campaignService;
    private readonly ILogger<CampaignsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CampaignsController"/> class.
    /// </summary>
    public CampaignsController(IMarketingCampaignService campaignService, ILogger<CampaignsController> logger)
    {
        _campaignService = campaignService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all marketing campaigns.
    /// </summary>
    /// <returns>List of all campaigns</returns>
    /// <response code="200">Returns the list of campaigns</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var dtos = await _campaignService.GetAllCampaignsAsync();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaigns");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets all currently active marketing campaigns.
    /// </summary>
    /// <returns>List of active campaigns</returns>
    /// <response code="200">Returns the list of active campaigns</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<CampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var dtos = await _campaignService.GetActiveCampaignsAsync();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active campaigns");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets a marketing campaign by its unique identifier.
    /// </summary>
    /// <param name="id">The campaign ID</param>
    /// <returns>The campaign if found</returns>
    /// <response code="200">Returns the campaign</response>
    /// <response code="404">If the campaign is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound(new { message = $"Campaign with ID {id} not found" });
            }
            return Ok(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new marketing campaign.
    /// </summary>
    /// <param name="campaign">The campaign to create</param>
    /// <returns>The created campaign</returns>
    /// <response code="201">Returns the newly created campaign</response>
    /// <response code="400">If the campaign data is invalid (e.g., end date before start date, negative budget)</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var id = await _campaignService.CreateCampaignAsync(dto);
            var created = await _campaignService.GetCampaignByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates an existing marketing campaign.
    /// </summary>
    /// <param name="id">The campaign ID</param>
    /// <param name="campaign">The updated campaign data</param>
    /// <returns>No content on success</returns>
    /// <response code="204">If the campaign was updated successfully</response>
    /// <response code="400">If the campaign data is invalid</response>
    /// <response code="404">If the campaign is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCampaignDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await _campaignService.GetCampaignByIdAsync(id);
            if (entity == null)
            {
                return NotFound(new { message = $"Campaign with ID {id} not found" });
            }
            await _campaignService.UpdateCampaignAsync(id, dto);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes a marketing campaign (soft delete).
    /// </summary>
    /// <param name="id">The campaign ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">If the campaign was deleted successfully</response>
    /// <response code="404">If the campaign is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _campaignService.DeleteCampaignAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Adds a metric to a marketing campaign.
    /// </summary>
    /// <param name="id">The campaign ID</param>
    /// <param name="metric">The metric to add</param>
    /// <returns>No content on success</returns>
    /// <response code="204">If the metric was added successfully</response>
    /// <response code="400">If the metric data is invalid</response>
    /// <response code="404">If the campaign is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/metrics")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddMetric(int id, [FromBody] CampaignMetric metric)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            metric.CampaignId = id;
            await _campaignService.AddCampaignMetricAsync(metric);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding metric to campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
