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
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// REST API Controller for Campaign Conversion management operations.
///
/// FUNCTIONAL VIEW:
/// This controller provides HTTP endpoints for:
/// - Tracking marketing campaign conversions
/// - Attributing conversion value to campaigns
/// - Querying conversions by campaign
///
/// TECHNICAL VIEW:
/// - Uses ICampaignConversionService for business logic (dependency injected)
/// - All endpoints require authentication (JWT Bearer token)
/// - Returns standardized JSON responses with appropriate HTTP status codes
/// - Implements proper error handling with logging
///
/// API ROUTES:
/// - GET    /api/campaign-conversions              - Get all conversions with pagination
/// - GET    /api/campaign-conversions/{id}         - Get conversion by ID
/// - GET    /api/campaign-conversions/campaign/{campaignId} - Get conversions by campaign
/// - POST   /api/campaign-conversions              - Create new conversion
/// - PUT    /api/campaign-conversions/{id}         - Update conversion
/// - DELETE /api/campaign-conversions/{id}         - Delete conversion (soft delete)
/// </summary>
[ApiController]
[Route("api/campaign-conversions")]
[Authorize]
public class CampaignConversionsController : CrmControllerBase
{
    private const string ConversionNotFoundMessage = "Campaign conversion with ID {0} not found";
    private readonly ICampaignConversionService _campaignConversionService;
    private readonly ILogger<CampaignConversionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CampaignConversionsController"/> class.
    /// </summary>
    /// <param name="campaignConversionService">Service for campaign conversion business logic.</param>
    /// <param name="logger">Logger for error and audit logging.</param>
    public CampaignConversionsController(
        ICampaignConversionService campaignConversionService,
        ILogger<CampaignConversionsController> logger)
    {
        _campaignConversionService = campaignConversionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all campaign conversions with pagination.
    ///
    /// FUNCTIONAL: Returns paginated list of all campaign conversions.
    /// TECHNICAL: Filters out soft-deleted records, returns 200 OK with array.
    /// </summary>
    /// <param name="filter">Optional filter string for conversion type or external IDs.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated array of CampaignConversionDto objects.</returns>
    /// <response code="200">Returns the list of campaign conversions.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CampaignConversionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? filter = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
                var (items, totalCount) = await _campaignConversionService.GetAllAsync(
            filter, page, pageSize, cancellationToken);

        return Ok(new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    /// <summary>
    /// Get a specific campaign conversion by ID.
    ///
    /// FUNCTIONAL: Returns detailed conversion information for viewing/editing.
    /// TECHNICAL: Returns 404 if conversion not found or deleted.
    /// </summary>
    /// <param name="id">The unique conversion identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>CampaignConversionDto if found.</returns>
    /// <response code="200">Returns the campaign conversion.</response>
    /// <response code="404">If conversion not found.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CampaignConversionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
                var conversion = await _campaignConversionService.GetByIdAsync(id, cancellationToken);

        if (conversion == null)
        {
            return NotFound(new { message = string.Format(ConversionNotFoundMessage, id) });
        }

        return Ok(conversion);
    }

    /// <summary>
    /// Get all conversions for a specific campaign.
    ///
    /// FUNCTIONAL: Returns list of conversions associated with a campaign.
    /// TECHNICAL: Uses ICampaignConversionService for campaign-scoped queries.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of CampaignConversionDto objects for the campaign.</returns>
    /// <response code="200">Returns the list of campaign conversions.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpGet("campaign/{campaignId}")]
    [ProducesResponseType(typeof(IEnumerable<CampaignConversionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCampaignId(
        int campaignId,
        CancellationToken cancellationToken = default)
    {
                var conversions = await _campaignConversionService.GetByCampaignIdAsync(campaignId, cancellationToken);

        return Ok(new
        {
            items = conversions,
            totalCount = conversions.Count,
            campaignId
        });
    }

    /// <summary>
    /// Create a new campaign conversion.
    ///
    /// FUNCTIONAL: Records a new conversion attributed to a campaign.
    /// TECHNICAL: Creates CampaignConversion entity, returns 201 Created with location header.
    /// </summary>
    /// <param name="dto">The campaign conversion data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created CampaignConversionDto.</returns>
    /// <response code="201">Returns the newly created conversion.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CampaignConversionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCampaignConversionDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var conversion = await _campaignConversionService.CreateAsync(dto, cancellationToken);

            _logger.LogInformation("Created campaign conversion {Id} for campaign {CampaignId}",
                conversion.Id, conversion.CampaignId);

            return CreatedAtAction(nameof(GetById), new { id = conversion.Id }, conversion);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation creating campaign conversion");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing campaign conversion.
    ///
    /// FUNCTIONAL: Updates conversion information (value, type, attribution, etc.).
    /// TECHNICAL: Returns 404 if not found, 200 OK with updated entity.
    /// </summary>
    /// <param name="id">The conversion ID to update.</param>
    /// <param name="dto">The updated conversion data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated CampaignConversionDto.</returns>
    /// <response code="200">Returns the updated conversion.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If conversion not found.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CampaignConversionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCampaignConversionDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var conversion = await _campaignConversionService.UpdateAsync(id, dto, cancellationToken);

        if (conversion == null)
        {
            return NotFound(new { message = string.Format(ConversionNotFoundMessage, id) });
        }

        _logger.LogInformation("Updated campaign conversion {Id}", id);

        return Ok(conversion);
    }

    /// <summary>
    /// Delete a campaign conversion (soft delete).
    ///
    /// FUNCTIONAL: Removes a conversion from active records.
    /// TECHNICAL: Sets IsDeleted flag, returns 204 No Content on success.
    /// </summary>
    /// <param name="id">The conversion ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Conversion was deleted successfully.</response>
    /// <response code="404">If conversion not found.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
                var deleted = await _campaignConversionService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(new { message = string.Format(ConversionNotFoundMessage, id) });
        }

        _logger.LogInformation("Deleted campaign conversion {Id}", id);

        return NoContent();
    }
}
