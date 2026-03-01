// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Hubs;
using CRM.Core.DTOs;
using CRM.Core.Exceptions;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing sales opportunities.
/// Provides endpoints for CRUD operations and pipeline analytics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OpportunitiesController : CrmControllerBase
{
    private const string OpportunityNotFoundMessage = "Opportunity {0} not found";
    private readonly IOpportunityService _opportunityService;
    private readonly IWinLossAnalysisService _winLossService;
    private readonly ILogger<OpportunitiesController> _logger;
    private readonly ICrmNotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpportunitiesController"/> class.
    /// </summary>
    public OpportunitiesController(
        IOpportunityService opportunityService,
        IWinLossAnalysisService winLossService,
        ILogger<OpportunitiesController> logger,
        ICrmNotificationService notificationService)
    {
        _opportunityService = opportunityService;
        _winLossService = winLossService;
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
                var opportunities = await _opportunityService.GetOpenOpportunitiesAsync();
        var dtos = opportunities.Select(MapToDto).ToList();
        return Ok(dtos);
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
                var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
        if (opportunity == null)
        {
            return NotFound(new { message = string.Format(OpportunityNotFoundMessage, id) });
        }
        return Ok(MapToDto(opportunity));
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
                var opportunities = await _opportunityService.GetOpportunitiesByAccountAsync(accountId);
        var dtos = opportunities.Select(MapToDto).ToList();
        return Ok(dtos);
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
                var opportunities = await _opportunityService.GetOpportunitiesByCustomerAsync(customerId);
        var dtos = opportunities.Select(MapToDto).ToList();
        return Ok(dtos);
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
                var totalPipeline = await _opportunityService.GetTotalPipelineAsync();
        return Ok(new { totalPipeline });
    }

    /// <summary>
    /// Creates a new opportunity.
    /// </summary>
    /// <param name="dto">The opportunity to create</param>
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
            {
                return BadRequest(ModelState);
            }

            var opportunity = MapFromCreateDto(dto);

            // TODO-CRM003-02: Auto-set probability from stage when caller does not supply it explicitly
            var stageEnum = (OpportunityStage)dto.Stage;
            if (dto.Probability == 0 && OpportunityService.StageProbabilityDefaults.TryGetValue(stageEnum, out var defaultProb))
            {
                opportunity.Probability = defaultProb;
            }

            var id = await _opportunityService.CreateOpportunityAsync(opportunity);
            opportunity.Id = id;

            // Notify connected clients about the new opportunity
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordCreatedAsync("Opportunity", id, MapToDto(opportunity), userId);

            return CreatedAtAction(nameof(GetById), new { id }, MapToDto(opportunity));
        }
        catch (DuplicateExistsException dex)
        {
            return Conflict(new { message = dex.Message, entityType = dex.EntityType, existingRecordId = dex.ExistingRecordId, matchScore = dex.MatchScore });
        }
    }

    /// <summary>
    /// Updates an existing opportunity.
    /// </summary>
    /// <param name="id">The opportunity ID</param>
    /// <param name="dto">The updated opportunity data</param>
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
                if (!ModelState.IsValid)
                {
            return BadRequest(ModelState);
                }

        var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
        if (opportunity == null)
        {
            return NotFound();
        }

        MapFromUpdateDto(dto, opportunity);

        // TODO-CRM003-02: When stage changes and caller did NOT explicitly supply a new probability,
        // auto-update probability based on the stage defaults.
        if (dto.Stage.HasValue && !dto.Probability.HasValue)
        {
            var newStage = (OpportunityStage)dto.Stage.Value;
            if (OpportunityService.StageProbabilityDefaults.TryGetValue(newStage, out var autoProb))
            {
                opportunity.Probability = autoProb;
            }
        }

        await _opportunityService.UpdateOpportunityAsync(opportunity);

        // Notify connected clients about the update
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        await _notificationService.NotifyRecordUpdatedAsync("Opportunity", id, MapToDto(opportunity), userId);

        return NoContent();
    }
    // --- Opportunity Product Endpoints (TODO-CRM003-04) ---

    /// <summary>
    /// Lists all products on an opportunity.
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    [HttpGet("{id}/products")]
    [ProducesResponseType(typeof(IEnumerable<OpportunityProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProducts(int id)
    {
                var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
        if (opportunity == null)
        {
            return NotFound(new { message = string.Format(OpportunityNotFoundMessage, id) });
        }

        var products = await _opportunityService.GetOpportunityProductsAsync(id);
        return Ok(products.Select(MapProductToDto).ToList());
    }

    /// <summary>
    /// Adds a product to an opportunity.
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    /// <param name="dto">Product line item data</param>
    [HttpPost("{id}/products")]
    [ProducesResponseType(typeof(OpportunityProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddProduct(int id, [FromBody] CreateOpportunityProductDto dto)
    {
                if (!ModelState.IsValid)
                {
            return BadRequest(ModelState);
                }

        var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
        if (opportunity == null)
        {
            return NotFound(new { message = string.Format(OpportunityNotFoundMessage, id) });
        }

        var product = MapProductFromCreateDto(dto);
        var created = await _opportunityService.AddOpportunityProductAsync(id, product);
        return CreatedAtAction(nameof(GetProducts), new { id }, MapProductToDto(created));
    }

    /// <summary>
    /// Updates a product line item on an opportunity.
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    /// <param name="productId">Product ID</param>
    /// <param name="dto">Updated line item data</param>
    [HttpPut("{id}/products/{productId}")]
    [ProducesResponseType(typeof(OpportunityProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct(int id, int productId, [FromBody] UpdateOpportunityProductDto dto)
    {
                if (!ModelState.IsValid)
                {
            return BadRequest(ModelState);
                }

        var updated = new OpportunityProduct
        {
            Quantity = dto.Quantity ?? 1,
            UnitPrice = dto.UnitPrice,
            DiscountPercent = dto.DiscountPercent,
            Notes = dto.Notes
        };

        var result = await _opportunityService.UpdateOpportunityProductAsync(id, productId, updated);
        if (result == null)
        {
            return NotFound(new { message = $"Product {productId} not found on opportunity {id}" });
        }

        return Ok(MapProductToDto(result));
    }

    /// <summary>
    /// Removes a product from an opportunity.
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    /// <param name="productId">Product ID</param>
    [HttpDelete("{id}/products/{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveProduct(int id, int productId)
    {
                var removed = await _opportunityService.RemoveOpportunityProductAsync(id, productId);
        if (!removed)
        {
            return NotFound(new { message = $"Product {productId} not found on opportunity {id}" });
        }

        return NoContent();
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
        if (dto.Name != null)
        {
            entity.Name = dto.Name;
        }
        if (dto.Stage.HasValue)
        {
            entity.Stage = (OpportunityStage)dto.Stage.Value;
        }
        if (dto.Probability.HasValue)
        {
            entity.Probability = dto.Probability.Value;
        }
        if (dto.Amount.HasValue)
        {
            entity.Amount = dto.Amount.Value;
        }
        if (dto.Currency != null)
        {
            entity.Currency = dto.Currency;
        }
        if (dto.ExpectedCloseDate != null)
        {
            entity.ExpectedCloseDate = DateTime.Parse(dto.ExpectedCloseDate);
        }
        if (dto.PricingModel.HasValue)
        {
            entity.PricingModel = (OpportunityPricingModel)dto.PricingModel.Value;
        }
        if (dto.TermLengthMonths.HasValue)
        {
            entity.TermLengthMonths = dto.TermLengthMonths.Value;
        }
        if (dto.SolutionNotes != null)
        {
            entity.SolutionNotes = dto.SolutionNotes;
        }
        if (dto.QualificationReason.HasValue)
        {
            entity.QualificationReason = (QualificationReason)dto.QualificationReason.Value;
        }
        if (dto.QualificationNotes != null)
        {
            entity.QualificationNotes = dto.QualificationNotes;
        }
        if (dto.Region != null)
        {
            entity.Region = dto.Region;
        }
        if (dto.AccountId.HasValue)
        {
            entity.AccountId = dto.AccountId.Value;
        }
        if (dto.PrimaryContactId.HasValue)
        {
            entity.PrimaryContactId = dto.PrimaryContactId.Value;
        }
        if (dto.SalesOwnerId.HasValue)
        {
            entity.SalesOwnerId = dto.SalesOwnerId.Value;
        }
        if (dto.LeadId.HasValue)
        {
            entity.LeadId = dto.LeadId.Value;
        }
        // Product update logic can be added here if needed
    }

    private static OpportunityTeamMemberDto MapTeamMemberToDto(OpportunityTeamMember m)
    {
        return new OpportunityTeamMemberDto
        {
            Id = m.Id,
            OpportunityId = m.OpportunityId,
            UserId = m.UserId,
            UserName = m.User?.Username,
            Role = (int)m.Role,
            RoleName = m.Role.ToString(),
            SplitPercentage = m.SplitPercentage,
            IsPrimary = m.IsPrimary,
            CommissionPlanId = m.CommissionPlanId,
            Notes = m.Notes,
            CreatedAt = m.CreatedAt
        };
    }

    private static OpportunityCompetitorDto MapOpportunityCompetitorToDto(OpportunityCompetitor oc)
    {
        return new OpportunityCompetitorDto
        {
            OpportunityId = oc.OpportunityId,
            CompetitorId = oc.CompetitorId,
            CompetitorName = oc.Competitor?.Name,
            ThreatLevel = oc.ThreatLevel.ToString(),
            Status = oc.Status.ToString(),
            CompetitorPrice = oc.CompetitorPrice,
            WonAgainst = oc.WonAgainst ?? false,
            Notes = oc.Notes,
            CreatedAt = oc.IdentifiedDate
        };
    }

    // --- Win/Loss Analytics (TODO-CRM003-05) ---

    /// <summary>
    /// Gets win/loss analysis summary for opportunities.
    /// </summary>
    /// <param name="fromDate">Start date filter (ISO 8601)</param>
    /// <param name="toDate">End date filter (ISO 8601)</param>
    /// <returns>Win/Loss summary analytics</returns>
    [HttpGet("analytics/win-loss")]
    [ProducesResponseType(typeof(WinLossSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWinLossAnalytics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
                var summary = await _winLossService.GetSummaryAsync(fromDate, toDate, ct);
        return Ok(summary);
    }

    /// <summary>
    /// Gets win/loss analysis grouped by loss reason.
    /// </summary>
    [HttpGet("analytics/win-loss/by-reason")]
    [ProducesResponseType(typeof(IEnumerable<WinLossByReason>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWinLossByReason(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
                var result = await _winLossService.GetByReasonAsync(fromDate, toDate, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets win/loss analysis grouped by competitor.
    /// </summary>
    [HttpGet("analytics/win-loss/by-competitor")]
    [ProducesResponseType(typeof(IEnumerable<WinLossByCompetitor>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWinLossByCompetitor(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
                var result = await _winLossService.GetByCompetitorAsync(fromDate, toDate, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets win rate trends over time.
    /// </summary>
    [HttpGet("analytics/win-loss/trends")]
    [ProducesResponseType(typeof(IEnumerable<WinRateTrend>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWinRateTrends(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string period = "month",
        CancellationToken ct = default)
    {
                var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
        var to = toDate ?? DateTime.UtcNow;
        var result = await _winLossService.GetWinRateTrendsAsync(from, to, period, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets comprehensive loss analysis report.
    /// </summary>
    [HttpGet("analytics/loss-analysis")]
    [ProducesResponseType(typeof(LossAnalysisReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLossAnalysis(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
                var result = await _winLossService.GetLossAnalysisAsync(fromDate, toDate, ct);
        return Ok(result);
    }

    // --- Opportunity Cloning (TODO-CRM003-06) ---

    /// <summary>
    /// Clones an opportunity including products, team members, and competitors.
    /// </summary>
    /// <param name="id">The opportunity ID to clone</param>
    /// <param name="options">Clone options</param>
    [HttpPost("{id}/clone")]
    [ProducesResponseType(typeof(OpportunityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Clone(int id, [FromBody] OpportunityCloneOptions? options = null)
    {
        try
        {
            var original = await _opportunityService.GetOpportunityByIdAsync(id);
            if (original == null)
            {
                return NotFound(new { message = string.Format(OpportunityNotFoundMessage, id) });
            }

            var cloned = await _opportunityService.CloneAsync(id, options);
            var dto = MapToDto(cloned);

            _logger.LogInformation("Cloned opportunity {OriginalId} to {NewId}", id, cloned.Id);

            return CreatedAtAction(nameof(GetById), new { id = cloned.Id }, dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = string.Format(OpportunityNotFoundMessage, id) });
        }
    }

    // --- Team Member Management (TODO-CRM003-08) ---

    /// <summary>
    /// Gets all team members assigned to an opportunity.
    /// </summary>
    [HttpGet("{id}/team")]
    [ProducesResponseType(typeof(IEnumerable<OpportunityTeamMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeamMembers(int id, CancellationToken ct = default)
    {
                var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
        if (opportunity == null)
        {
            return NotFound(new { message = string.Format(OpportunityNotFoundMessage, id) });
        }

        var members = await _opportunityService.GetTeamMembersAsync(id, ct);
        var dtos = members.Select(MapTeamMemberToDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Adds a team member to an opportunity.
    /// </summary>
    [HttpPost("{id}/team")]
    [ProducesResponseType(typeof(OpportunityTeamMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddTeamMember(int id, [FromBody] CreateTeamMemberDto dto, CancellationToken ct = default)
    {
                if (!ModelState.IsValid)
                {
            return BadRequest(ModelState);
                }

        var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
        if (opportunity == null)
        {
            return NotFound(new { message = string.Format(OpportunityNotFoundMessage, id) });
        }

        var member = new OpportunityTeamMember
        {
            OpportunityId = id,
            UserId = dto.UserId,
            Role = (OpportunityTeamRole)dto.Role,
            SplitPercentage = dto.SplitPercentage,
            IsPrimary = dto.IsPrimary,
            CommissionPlanId = dto.CommissionPlanId,
            Notes = dto.Notes
        };

        var created = await _opportunityService.AddTeamMemberAsync(id, member, ct);
        return CreatedAtAction(nameof(GetTeamMembers), new { id }, MapTeamMemberToDto(created));
    }

    /// <summary>
    /// Updates a team member on an opportunity.
    /// </summary>
    [HttpPut("{id}/team/{memberId}")]
    [ProducesResponseType(typeof(OpportunityTeamMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTeamMember(int id, int memberId, [FromBody] UpdateTeamMemberDto dto, CancellationToken ct = default)
    {
                var updated = new OpportunityTeamMember
        {
            Role = dto.Role.HasValue ? (OpportunityTeamRole)dto.Role.Value : OpportunityTeamRole.AccountExecutive,
            SplitPercentage = dto.SplitPercentage ?? 0,
            IsPrimary = dto.IsPrimary ?? false,
            CommissionPlanId = dto.CommissionPlanId,
            Notes = dto.Notes
        };

        var result = await _opportunityService.UpdateTeamMemberAsync(id, memberId, updated, ct);
        if (result == null)
        {
            return NotFound(new { message = $"Team member {memberId} not found on opportunity {id}" });
        }

        return Ok(MapTeamMemberToDto(result));
    }

    /// <summary>
    /// Removes a team member from an opportunity.
    /// </summary>
    [HttpDelete("{id}/team/{memberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveTeamMember(int id, int memberId, CancellationToken ct = default)
    {
                var removed = await _opportunityService.RemoveTeamMemberAsync(id, memberId, ct);
        if (!removed)
        {
            return NotFound(new { message = $"Team member {memberId} not found on opportunity {id}" });
        }
        return NoContent();
    }

    // --- Competitor Management on Opportunities (TODO-CRM003-03) ---

    /// <summary>
    /// Gets all competitors linked to an opportunity.
    /// </summary>
    [HttpGet("{id}/competitors")]
    [ProducesResponseType(typeof(IEnumerable<OpportunityCompetitorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOpportunityCompetitors(int id, CancellationToken ct = default)
    {
                var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
        if (opportunity == null)
        {
            return NotFound(new { message = string.Format(OpportunityNotFoundMessage, id) });
        }

        var competitors = await _opportunityService.GetCompetitorsAsync(id, ct);
        var dtos = competitors.Select(MapOpportunityCompetitorToDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Adds a competitor to an opportunity.
    /// </summary>
    [HttpPost("{id}/competitors")]
    [ProducesResponseType(typeof(OpportunityCompetitorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddCompetitor(int id, [FromBody] CreateOpportunityCompetitorDto dto, CancellationToken ct = default)
    {
                if (!ModelState.IsValid)
                {
            return BadRequest(ModelState);
                }

        var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
        if (opportunity == null)
        {
            return NotFound(new { message = string.Format(OpportunityNotFoundMessage, id) });
        }

        var competitor = new OpportunityCompetitor
        {
            OpportunityId = id,
            CompetitorId = dto.CompetitorId,
            ThreatLevel = !string.IsNullOrEmpty(dto.ThreatLevel) ? Enum.Parse<CompetitorThreatLevel>(dto.ThreatLevel) : CompetitorThreatLevel.Medium,
            Status = !string.IsNullOrEmpty(dto.Status) ? Enum.Parse<OpportunityCompetitorStatus>(dto.Status) : OpportunityCompetitorStatus.Identified,
            CompetitorPrice = dto.CompetitorPrice,
            Notes = dto.Notes
        };

        var created = await _opportunityService.AddCompetitorAsync(id, competitor, ct);
        return CreatedAtAction(nameof(GetOpportunityCompetitors), new { id }, MapOpportunityCompetitorToDto(created));
    }

    /// <summary>
    /// Removes a competitor from an opportunity.
    /// </summary>
    [HttpDelete("{id}/competitors/{competitorId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveCompetitor(int id, int competitorId, CancellationToken ct = default)
    {
                var removed = await _opportunityService.RemoveCompetitorAsync(id, competitorId, ct);
        if (!removed)
        {
            return NotFound(new { message = $"Competitor {competitorId} not found on opportunity {id}" });
        }
        return NoContent();
    }

    /// <summary>
    /// Updates competitor details on an opportunity (TODO-CRM003-03).
    /// </summary>
    [HttpPut("{id}/competitors/{competitorId}")]
    [ProducesResponseType(typeof(OpportunityCompetitorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCompetitor(int id, int competitorId, [FromBody] UpdateOpportunityCompetitorDto dto, CancellationToken ct = default)
    {
                if (!ModelState.IsValid)
                {
            return BadRequest(ModelState);
                }

        var updated = new OpportunityCompetitor
        {
            ThreatLevel = !string.IsNullOrEmpty(dto.ThreatLevel) ? Enum.Parse<CompetitorThreatLevel>(dto.ThreatLevel) : CompetitorThreatLevel.Medium,
            Status = !string.IsNullOrEmpty(dto.Status) ? Enum.Parse<OpportunityCompetitorStatus>(dto.Status) : OpportunityCompetitorStatus.Active,
            CompetitorPrice = dto.CompetitorPrice,
            Notes = dto.Notes,
            WonAgainst = dto.WonAgainst
        };

        var result = await _opportunityService.UpdateCompetitorAsync(id, competitorId, updated, ct);
        if (result == null)
        {
            return NotFound(new { message = $"Competitor {competitorId} not found on opportunity {id}" });
        }

        return Ok(MapOpportunityCompetitorToDto(result));
    }

    // --- Forecast Category (TODO-CRM003-07) ---

    /// <summary>
    /// Updates the forecast category of an opportunity (TODO-CRM003-07).
    /// </summary>
    [HttpPatch("{id}/forecast-category")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchForecastCategory(int id, [FromBody] ForecastCategoryPatchDto dto, CancellationToken ct = default)
    {
                if (!ModelState.IsValid)
                {
            return BadRequest(ModelState);
                }

        if (!Enum.IsDefined(typeof(ForecastCategory), dto.ForecastCategory))
        {
            return BadRequest(new { message = $"Invalid ForecastCategory value: {dto.ForecastCategory}" });
        }

        var updated = await _opportunityService.PatchForecastCategoryAsync(id, (ForecastCategory)dto.ForecastCategory, ct);
        if (!updated)
        {
            return NotFound(new { message = string.Format(OpportunityNotFoundMessage, id) });
        }

        return NoContent();
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
                await _opportunityService.DeleteOpportunityAsync(id);

        // Notify connected clients about the deletion
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        await _notificationService.NotifyRecordDeletedAsync("Opportunity", id, userId);

        return NoContent();
    }
}
