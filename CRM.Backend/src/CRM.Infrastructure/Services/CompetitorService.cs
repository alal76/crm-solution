// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Competitor Service (TODO-CRM003-03)
/// Manages competitor data and opportunity-competitor relationships.
/// </summary>
public class CompetitorService : ICompetitorService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<CompetitorService> _logger;

    public CompetitorService(ICrmDbContext dbContext, ILogger<CompetitorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<Competitor>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Competitors.Where(c => !c.IsDeleted);
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<Competitor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Competitors
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<int> CreateAsync(Competitor competitor, CancellationToken cancellationToken = default)
    {
        competitor.CreatedAt = DateTime.UtcNow;
        _dbContext.Competitors.Add(competitor);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created competitor {Id}: {Name}", competitor.Id, competitor.Name);
        return competitor.Id;
    }

    public async Task<bool> UpdateAsync(Competitor competitor, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(competitor.Id, cancellationToken);
        if (existing == null) return false;

        existing.Name = competitor.Name;
        existing.Description = competitor.Description;
        existing.Website = competitor.Website;
        existing.Industry = competitor.Industry;
        existing.Strengths = competitor.Strengths;
        existing.Weaknesses = competitor.Weaknesses;
        existing.OurAdvantages = competitor.OurAdvantages;
        existing.PrimaryProducts = competitor.PrimaryProducts;
        existing.PricingTier = competitor.PricingTier;
        existing.MarketSharePercent = competitor.MarketSharePercent;
        existing.IsActive = competitor.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated competitor {Id}: {Name}", competitor.Id, competitor.Name);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing == null) return false;

        existing.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted competitor {Id}", id);
        return true;
    }

    public async Task<IEnumerable<OpportunityCompetitor>> GetOpportunityCompetitorsAsync(int opportunityId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.OpportunityCompetitors
            .Include(oc => oc.Competitor)
            .Where(oc => oc.OpportunityId == opportunityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<OpportunityCompetitor> AddToOpportunityAsync(
        int opportunityId,
        int competitorId,
        bool isPrimary = false,
        string? threatLevel = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        // Check if already exists
        var existing = await _dbContext.OpportunityCompetitors
            .FirstOrDefaultAsync(oc => oc.OpportunityId == opportunityId && oc.CompetitorId == competitorId, cancellationToken);

        if (existing != null)
        {
            // Reactivate if needed
            existing.ThreatLevel = threatLevel != null ? Enum.Parse<CompetitorThreatLevel>(threatLevel) : existing.ThreatLevel;
            existing.Notes = notes;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var oppCompetitor = new OpportunityCompetitor
        {
            OpportunityId = opportunityId,
            CompetitorId = competitorId,
            ThreatLevel = threatLevel != null ? Enum.Parse<CompetitorThreatLevel>(threatLevel) : CompetitorThreatLevel.Medium,
            Notes = notes,
            IdentifiedDate = DateTime.UtcNow
        };

        _dbContext.OpportunityCompetitors.Add(oppCompetitor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added competitor {CompetitorId} to opportunity {OpportunityId}", competitorId, opportunityId);
        return oppCompetitor;
    }

    public async Task<bool> RemoveFromOpportunityAsync(int opportunityId, int competitorId, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.OpportunityCompetitors
            .FirstOrDefaultAsync(oc => oc.OpportunityId == opportunityId && oc.CompetitorId == competitorId, cancellationToken);

        if (existing == null) return false;

        _dbContext.OpportunityCompetitors.Remove(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed competitor {CompetitorId} from opportunity {OpportunityId}", competitorId, opportunityId);
        return true;
    }

    public async Task<IEnumerable<Competitor>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await _dbContext.Competitors
            .Where(c => !c.IsDeleted && c.IsActive)
            .Where(c => c.Name.ToLower().Contains(term) ||
                       (c.Description != null && c.Description.ToLower().Contains(term)))
            .OrderBy(c => c.Name)
            .Take(20)
            .ToListAsync(cancellationToken);
    }
}
