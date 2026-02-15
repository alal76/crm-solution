// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing sales opportunities.
/// Provides endpoints for CRUD operations and pipeline analytics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OpportunitiesController : ControllerBase
{
    private readonly IOpportunityService _opportunityService;
    private readonly ILogger<OpportunitiesController> _logger;
    private readonly ICrmNotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpportunitiesController"/> class.
    /// </summary>
    public OpportunitiesController(
        IOpportunityService opportunityService,
        ILogger<OpportunitiesController> logger,
        ICrmNotificationService notificationService)
    {
        _opportunityService = opportunityService;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Gets all open opportunities.
    /// </summary>
    /// <returns>List of open opportunities</returns>
    /// <response code="200">Returns the list of open opportunities</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Opportunity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOpen()
    {
        try
        {
            var opportunities = await _opportunityService.GetOpenOpportunitiesAsync();
            return Ok(opportunities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving opportunities");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets an opportunity by its unique identifier.
    /// </summary>
    /// <param name="id">The opportunity ID</param>
    /// <returns>The opportunity if found</returns>
    /// <response code="200">Returns the opportunity</response>
    /// <response code="404">If the opportunity is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Opportunity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
            if (opportunity == null)
                return NotFound(new { message = $"Opportunity with ID {id} not found" });
            return Ok(opportunity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving opportunity {OpportunityId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets all opportunities for a specific account.
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <returns>List of opportunities for the account</returns>
    /// <response code="200">Returns the list of opportunities</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(typeof(IEnumerable<Opportunity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByAccountId(int accountId)
    {
        try
        {
            var opportunities = await _opportunityService.GetOpportunitiesByAccountAsync(accountId);
            return Ok(opportunities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving opportunities for account {AccountId}", accountId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets all opportunities for a specific customer ID (alias for GetByAccountId for backward compatibility).
    /// </summary>
    /// <param name="customerId">The customer/account ID</param>
    /// <returns>List of opportunities for the customer</returns>
    /// <response code="200">Returns the list of opportunities</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(List<Opportunity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        try
        {
            var opportunities = await _opportunityService.GetOpportunitiesByCustomerAsync(customerId);
            return Ok(opportunities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving opportunities for customer {CustomerId}", customerId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets the total pipeline value across all open opportunities.
    /// </summary>
    /// <returns>The total pipeline value</returns>
    /// <response code="200">Returns the total pipeline value</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("pipeline/total")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTotalPipeline()
    {
        try
        {
            var totalPipeline = await _opportunityService.GetTotalPipelineAsync();
            return Ok(new { totalPipeline });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total pipeline");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new opportunity.
    /// </summary>
    /// <param name="opportunity">The opportunity to create</param>
    /// <returns>The created opportunity</returns>
    /// <response code="201">Returns the newly created opportunity</response>
    /// <response code="400">If the opportunity data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(Opportunity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] Opportunity opportunity)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _opportunityService.CreateOpportunityAsync(opportunity);
            opportunity.Id = id;

            // Notify connected clients about the new opportunity
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordCreatedAsync("Opportunity", id, opportunity, userId);

            return CreatedAtAction(nameof(GetById), new { id }, opportunity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating opportunity");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates an existing opportunity.
    /// </summary>
    /// <param name="id">The opportunity ID</param>
    /// <param name="opportunity">The updated opportunity data</param>
    /// <returns>No content on success</returns>
    /// <response code="204">If the opportunity was updated successfully</response>
    /// <response code="400">If the opportunity data is invalid</response>
    /// <response code="404">If the opportunity is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] Opportunity opportunity)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            opportunity.Id = id;
            await _opportunityService.UpdateOpportunityAsync(opportunity);

            // Notify connected clients about the update
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordUpdatedAsync("Opportunity", id, opportunity, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating opportunity {OpportunityId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes an opportunity (soft delete).
    /// </summary>
    /// <param name="id">The opportunity ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">If the opportunity was deleted successfully</response>
    /// <response code="404">If the opportunity is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _opportunityService.DeleteOpportunityAsync(id);

            // Notify connected clients about the deletion
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordDeletedAsync("Opportunity", id, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting opportunity {OpportunityId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
