// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.DTOs;
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
    [ProducesResponseType(typeof(IEnumerable<OpportunityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOpen()
    {
        try
        {
            var opportunities = await _opportunityService.GetOpenOpportunitiesAsync();
            var dtos = opportunities.Select(MapToDto).ToList();
            return Ok(dtos);
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
    [ProducesResponseType(typeof(OpportunityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
            if (opportunity == null)
                return NotFound(new { message = $"Opportunity with ID {id} not found" });
            return Ok(MapToDto(opportunity));
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
    [ProducesResponseType(typeof(IEnumerable<OpportunityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByAccountId(int accountId)
    {
        try
        {
            var opportunities = await _opportunityService.GetOpportunitiesByAccountAsync(accountId);
            var dtos = opportunities.Select(MapToDto).ToList();
            return Ok(dtos);
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
    [ProducesResponseType(typeof(List<OpportunityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        try
        {
            var opportunities = await _opportunityService.GetOpportunitiesByCustomerAsync(customerId);
            var dtos = opportunities.Select(MapToDto).ToList();
            return Ok(dtos);
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
    [ProducesResponseType(typeof(OpportunityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var opportunity = MapFromCreateDto(dto);
            var id = await _opportunityService.CreateOpportunityAsync(opportunity);
            opportunity.Id = id;

            // Notify connected clients about the new opportunity
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordCreatedAsync("Opportunity", id, MapToDto(opportunity), userId);

            return CreatedAtAction(nameof(GetById), new { id }, MapToDto(opportunity));
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
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOpportunityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
            if (opportunity == null)
                return NotFound();

            MapFromUpdateDto(dto, opportunity);
            await _opportunityService.UpdateOpportunityAsync(opportunity);

            // Notify connected clients about the update
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordUpdatedAsync("Opportunity", id, MapToDto(opportunity), userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating opportunity {OpportunityId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    // --- Mapping helpers ---
    private static OpportunityDto MapToDto(Opportunity entity)
    {
        return new OpportunityDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Stage = (int)entity.Stage,
            StageName = entity.Stage.ToString(),
            Probability = entity.Probability,
            Amount = entity.Amount,
            Currency = entity.Currency,
            ExpectedCloseDate = entity.ExpectedCloseDate?.ToString("o"),
            PricingModel = (int)entity.PricingModel,
            PricingModelName = entity.PricingModel.ToString(),
            TermLengthMonths = entity.TermLengthMonths,
            SolutionNotes = entity.SolutionNotes,
            QualificationReason = (int?)entity.QualificationReason,
            QualificationNotes = entity.QualificationNotes,
            Region = entity.Region,
            AccountId = entity.AccountId,
            AccountName = entity.Account?.DisplayName,
            PrimaryContactId = entity.PrimaryContactId,
            PrimaryContactName = entity.PrimaryContact != null
                ? $"{entity.PrimaryContact.FirstName} {entity.PrimaryContact.LastName}".Trim()
                : null,
            SalesOwnerId = entity.SalesOwnerId,
            SalesOwnerName = entity.SalesOwner?.FullName,
            LeadId = entity.LeadId,
            Products = entity.Products?.Select(MapProductToDto).ToList() ?? new(),
            CreatedAt = entity.CreatedAt.ToString("o"),
            UpdatedAt = entity.UpdatedAt?.ToString("o") ?? string.Empty,
            IsDeleted = entity.IsDeleted,
            RowVersion = entity.RowVersion,
            WeightedAmount = entity.WeightedAmount,
            WeightedValue = entity.WeightedAmount,
            IsOpen = entity.IsOpen,
            IsWon = entity.IsWon
        };
    }

    private static OpportunityProductDto MapProductToDto(OpportunityProduct p)
    {
        return new OpportunityProductDto
        {
            OpportunityId = p.OpportunityId,
            ProductId = p.ProductId,
            ProductName = p.Product?.Name,
            Quantity = p.Quantity,
            UnitPrice = p.UnitPrice,
            DiscountPercent = p.DiscountPercent,
            LineTotal = p.LineTotal,
            TotalPrice = p.TotalPrice,
            Notes = p.Notes,
            CreatedAt = p.CreatedAt.ToString("o"),
            IsDeleted = p.IsDeleted
        };
    }

    private static Opportunity MapFromCreateDto(CreateOpportunityDto dto)
    {
        var entity = new Opportunity
        {
            Name = dto.Name,
            Stage = (OpportunityStage)dto.Stage,
            Probability = dto.Probability,
            Amount = dto.Amount,
            Currency = dto.Currency,
            ExpectedCloseDate = string.IsNullOrEmpty(dto.ExpectedCloseDate) ? null : DateTime.Parse(dto.ExpectedCloseDate),
            PricingModel = (OpportunityPricingModel)dto.PricingModel,
            TermLengthMonths = dto.TermLengthMonths,
            SolutionNotes = dto.SolutionNotes,
            QualificationReason = (QualificationReason?)dto.QualificationReason,
            QualificationNotes = dto.QualificationNotes,
            Region = dto.Region,
            AccountId = dto.AccountId,
            PrimaryContactId = dto.PrimaryContactId,
            SalesOwnerId = dto.SalesOwnerId,
            LeadId = dto.LeadId,
            Products = dto.Products?.Select(MapProductFromCreateDto).ToList() ?? new List<OpportunityProduct>()
        };
        return entity;
    }

    private static OpportunityProduct MapProductFromCreateDto(CreateOpportunityProductDto dto)
    {
        return new OpportunityProduct
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            DiscountPercent = dto.DiscountPercent,
            Notes = dto.Notes
        };
    }

    private static void MapFromUpdateDto(UpdateOpportunityDto dto, Opportunity entity)
    {
        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.Stage.HasValue) entity.Stage = (OpportunityStage)dto.Stage.Value;
        if (dto.Probability.HasValue) entity.Probability = dto.Probability.Value;
        if (dto.Amount.HasValue) entity.Amount = dto.Amount.Value;
        if (dto.Currency != null) entity.Currency = dto.Currency;
        if (dto.ExpectedCloseDate != null) entity.ExpectedCloseDate = DateTime.Parse(dto.ExpectedCloseDate);
        if (dto.PricingModel.HasValue) entity.PricingModel = (OpportunityPricingModel)dto.PricingModel.Value;
        if (dto.TermLengthMonths.HasValue) entity.TermLengthMonths = dto.TermLengthMonths.Value;
        if (dto.SolutionNotes != null) entity.SolutionNotes = dto.SolutionNotes;
        if (dto.QualificationReason.HasValue) entity.QualificationReason = (QualificationReason)dto.QualificationReason.Value;
        if (dto.QualificationNotes != null) entity.QualificationNotes = dto.QualificationNotes;
        if (dto.Region != null) entity.Region = dto.Region;
        if (dto.AccountId.HasValue) entity.AccountId = dto.AccountId.Value;
        if (dto.PrimaryContactId.HasValue) entity.PrimaryContactId = dto.PrimaryContactId.Value;
        if (dto.SalesOwnerId.HasValue) entity.SalesOwnerId = dto.SalesOwnerId.Value;
        if (dto.LeadId.HasValue) entity.LeadId = dto.LeadId.Value;
        // Product update logic can be added here if needed
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
