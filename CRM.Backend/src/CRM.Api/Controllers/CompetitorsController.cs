// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing competitors.
/// TODO-CRM003-03: Implement competitor tracking on opportunities
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompetitorsController : ControllerBase
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CompetitorsController> _logger;

    public CompetitorsController(ICrmDbContext context, ILogger<CompetitorsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all competitors.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CompetitorDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false, CancellationToken ct = default)
    {
        try
        {
            var query = _context.Competitors.AsNoTracking().Where(c => !c.IsDeleted);

            if (activeOnly)
                query = query.Where(c => c.IsActive);

            var competitors = await query
                .OrderBy(c => c.Name)
                .Select(c => new CompetitorDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Website = c.Website,
                    Industry = c.Industry,
                    Strengths = c.Strengths,
                    Weaknesses = c.Weaknesses,
                    OurAdvantages = c.OurAdvantages,
                    PrimaryProducts = c.PrimaryProducts,
                    PricingTier = c.PricingTier,
                    MarketSharePercent = c.MarketSharePercent,
                    IsActive = c.IsActive,
                    WinRateAgainst = c.WinRateAgainst,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt ?? c.CreatedAt
                })
                .ToListAsync(ct);

            return Ok(competitors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving competitors");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get competitor by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CompetitorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        try
        {
            var competitor = await _context.Competitors
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

            if (competitor == null)
                return NotFound(new { message = $"Competitor with ID {id} not found" });

            return Ok(new CompetitorDto
            {
                Id = competitor.Id,
                Name = competitor.Name,
                Description = competitor.Description,
                Website = competitor.Website,
                Industry = competitor.Industry,
                Strengths = competitor.Strengths,
                Weaknesses = competitor.Weaknesses,
                OurAdvantages = competitor.OurAdvantages,
                PrimaryProducts = competitor.PrimaryProducts,
                PricingTier = competitor.PricingTier,
                MarketSharePercent = competitor.MarketSharePercent,
                IsActive = competitor.IsActive,
                WinRateAgainst = competitor.WinRateAgainst,
                Notes = competitor.Notes,
                CreatedAt = competitor.CreatedAt,
                UpdatedAt = competitor.UpdatedAt ?? competitor.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving competitor {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new competitor.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCompetitorDto request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Name is required" });

            var competitor = new Competitor
            {
                Name = request.Name,
                Description = request.Description,
                Website = request.Website,
                Industry = request.Industry,
                Strengths = request.Strengths,
                Weaknesses = request.Weaknesses,
                OurAdvantages = request.OurAdvantages,
                PrimaryProducts = request.PrimaryProducts,
                PricingTier = request.PricingTier,
                MarketSharePercent = request.MarketSharePercent,
                IsActive = request.IsActive,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Competitors.Add(competitor);
            await (_context as DbContext)!.SaveChangesAsync(ct);

            _logger.LogInformation("Created competitor: {Name} (ID: {Id})", competitor.Name, competitor.Id);
            return CreatedAtAction(nameof(GetById), new { id = competitor.Id }, new { id = competitor.Id, message = "Competitor created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating competitor");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a competitor.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompetitorDto request, CancellationToken ct = default)
    {
        try
        {
            var competitor = await _context.Competitors.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
            if (competitor == null)
                return NotFound(new { message = $"Competitor with ID {id} not found" });

            if (!string.IsNullOrWhiteSpace(request.Name))
                competitor.Name = request.Name;
            if (request.Description != null)
                competitor.Description = request.Description;
            if (request.Website != null)
                competitor.Website = request.Website;
            if (request.Industry != null)
                competitor.Industry = request.Industry;
            if (request.Strengths != null)
                competitor.Strengths = request.Strengths;
            if (request.Weaknesses != null)
                competitor.Weaknesses = request.Weaknesses;
            if (request.OurAdvantages != null)
                competitor.OurAdvantages = request.OurAdvantages;
            if (request.PrimaryProducts != null)
                competitor.PrimaryProducts = request.PrimaryProducts;
            if (request.PricingTier != null)
                competitor.PricingTier = request.PricingTier;
            if (request.MarketSharePercent.HasValue)
                competitor.MarketSharePercent = request.MarketSharePercent;
            if (request.IsActive.HasValue)
                competitor.IsActive = request.IsActive.Value;
            if (request.Notes != null)
                competitor.Notes = request.Notes;

            competitor.UpdatedAt = DateTime.UtcNow;
            await (_context as DbContext)!.SaveChangesAsync(ct);

            _logger.LogInformation("Updated competitor: {Id}", id);
            return Ok(new { message = "Competitor updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating competitor {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a competitor.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        try
        {
            var competitor = await _context.Competitors.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
            if (competitor == null)
                return NotFound(new { message = $"Competitor with ID {id} not found" });

            competitor.IsDeleted = true;
            competitor.UpdatedAt = DateTime.UtcNow;
            await (_context as DbContext)!.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted competitor: {Id}", id);
            return Ok(new { message = "Competitor deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting competitor {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get win/loss statistics against a competitor.
    /// </summary>
    [HttpGet("{id}/winloss")]
    [ProducesResponseType(typeof(CompetitorWinLossStats), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWinLossStats(int id, CancellationToken ct = default)
    {
        try
        {
            var competitor = await _context.Competitors
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

            if (competitor == null)
                return NotFound(new { message = $"Competitor with ID {id} not found" });

            var opportunities = await _context.OpportunityCompetitors
                .AsNoTracking()
                .Include(oc => oc.Opportunity)
                .Where(oc => oc.CompetitorId == id && !oc.Opportunity.IsDeleted)
                .ToListAsync(ct);

            var wins = opportunities.Count(o => o.Opportunity.Stage == OpportunityStage.ClosedWon);
            var losses = opportunities.Count(o => o.Opportunity.Stage == OpportunityStage.ClosedLost && o.WonAgainst == false);
            var total = wins + losses;

            var stats = new CompetitorWinLossStats
            {
                CompetitorId = id,
                CompetitorName = competitor.Name,
                TotalDeals = opportunities.Count,
                ClosedDeals = total,
                Wins = wins,
                Losses = losses,
                WinRate = total > 0 ? Math.Round((decimal)wins / total * 100, 2) : 0,
                TotalDealValue = opportunities.Sum(o => o.Opportunity.Amount),
                WonDealValue = opportunities.Where(o => o.Opportunity.Stage == OpportunityStage.ClosedWon).Sum(o => o.Opportunity.Amount),
                LostDealValue = opportunities.Where(o => o.Opportunity.Stage == OpportunityStage.ClosedLost).Sum(o => o.Opportunity.Amount)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving win/loss stats for competitor {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Links a competitor to an opportunity (TODO-CRM003-03).
    /// </summary>
    [HttpPost("{id}/opportunities/{opportunityId}")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> LinkToOpportunity(
        int id,
        int opportunityId,
        [FromBody] LinkCompetitorToOpportunityDto? dto = null,
        CancellationToken ct = default)
    {
        try
        {
            var competitor = await _context.Competitors.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
            if (competitor == null)
                return NotFound(new { message = $"Competitor with ID {id} not found" });

            var opportunity = await _context.Opportunities.FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, ct);
            if (opportunity == null)
                return NotFound(new { message = $"Opportunity with ID {opportunityId} not found" });

            // Check if already linked
            var existing = await _context.OpportunityCompetitors
                .FirstOrDefaultAsync(oc => oc.CompetitorId == id && oc.OpportunityId == opportunityId, ct);
            if (existing != null)
                return Conflict(new { message = "Competitor is already linked to this opportunity" });

            var link = new OpportunityCompetitor
            {
                OpportunityId = opportunityId,
                CompetitorId = id,
                ThreatLevel = !string.IsNullOrEmpty(dto?.ThreatLevel) ? Enum.Parse<CompetitorThreatLevel>(dto.ThreatLevel) : CompetitorThreatLevel.Medium,
                Status = !string.IsNullOrEmpty(dto?.Status) ? Enum.Parse<OpportunityCompetitorStatus>(dto.Status) : OpportunityCompetitorStatus.Identified,
                CompetitorPrice = dto?.CompetitorPrice,
                Notes = dto?.Notes
            };

            _context.OpportunityCompetitors.Add(link);
            await (_context as DbContext)!.SaveChangesAsync(ct);

            _logger.LogInformation("Linked competitor {CompetitorId} to opportunity {OpportunityId}", id, opportunityId);
            return StatusCode(201, new { message = "Competitor linked to opportunity successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking competitor {CompetitorId} to opportunity {OpportunityId}", id, opportunityId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Unlinks a competitor from an opportunity.
    /// </summary>
    [HttpDelete("{id}/opportunities/{opportunityId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnlinkFromOpportunity(int id, int opportunityId, CancellationToken ct = default)
    {
        try
        {
            var link = await _context.OpportunityCompetitors
                .FirstOrDefaultAsync(oc => oc.CompetitorId == id && oc.OpportunityId == opportunityId, ct);

            if (link == null)
                return NotFound(new { message = "Competitor is not linked to this opportunity" });

            _context.OpportunityCompetitors.Remove(link);
            await (_context as DbContext)!.SaveChangesAsync(ct);

            _logger.LogInformation("Unlinked competitor {CompetitorId} from opportunity {OpportunityId}", id, opportunityId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking competitor {CompetitorId} from opportunity {OpportunityId}", id, opportunityId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets all opportunities linked to a competitor.
    /// </summary>
    [HttpGet("{id}/opportunities")]
    [ProducesResponseType(typeof(IEnumerable<CompetitorOpportunityDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOpportunities(int id, CancellationToken ct = default)
    {
        try
        {
            var competitor = await _context.Competitors
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

            if (competitor == null)
                return NotFound(new { message = $"Competitor with ID {id} not found" });

            var opportunities = await _context.OpportunityCompetitors
                .AsNoTracking()
                .Include(oc => oc.Opportunity)
                .Where(oc => oc.CompetitorId == id && !oc.Opportunity.IsDeleted)
                .Select(oc => new CompetitorOpportunityDto
                {
                    OpportunityId = oc.OpportunityId,
                    OpportunityName = oc.Opportunity.Name,
                    Stage = oc.Opportunity.Stage.ToString(),
                    Amount = oc.Opportunity.Amount,
                    ThreatLevel = oc.ThreatLevel.ToString(),
                    Status = oc.Status.ToString(),
                    CompetitorPrice = oc.CompetitorPrice,
                    WonAgainst = oc.WonAgainst ?? false,
                    Notes = oc.Notes
                })
                .ToListAsync(ct);

            return Ok(opportunities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving opportunities for competitor {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

#region DTOs

public class CompetitorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public string? OurAdvantages { get; set; }
    public string? PrimaryProducts { get; set; }
    public string? PricingTier { get; set; }
    public decimal? MarketSharePercent { get; set; }
    public bool IsActive { get; set; }
    public decimal? WinRateAgainst { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCompetitorDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public string? OurAdvantages { get; set; }
    public string? PrimaryProducts { get; set; }
    public string? PricingTier { get; set; }
    public decimal? MarketSharePercent { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateCompetitorDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public string? OurAdvantages { get; set; }
    public string? PrimaryProducts { get; set; }
    public string? PricingTier { get; set; }
    public decimal? MarketSharePercent { get; set; }
    public bool? IsActive { get; set; }
    public string? Notes { get; set; }
}

public class CompetitorWinLossStats
{
    public int CompetitorId { get; set; }
    public string CompetitorName { get; set; } = string.Empty;
    public int TotalDeals { get; set; }
    public int ClosedDeals { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalDealValue { get; set; }
    public decimal WonDealValue { get; set; }
    public decimal LostDealValue { get; set; }
}

/// <summary>
/// DTO for linking a competitor to an opportunity.
/// </summary>
public class LinkCompetitorToOpportunityDto
{
    public string? ThreatLevel { get; set; }
    public string? Status { get; set; }
    public decimal? CompetitorPrice { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for a competitor's linked opportunity.
/// </summary>
public class CompetitorOpportunityDto
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ThreatLevel { get; set; }
    public string? Status { get; set; }
    public decimal? CompetitorPrice { get; set; }
    public bool WonAgainst { get; set; }
    public string? Notes { get; set; }
}

#endregion
