// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs;
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Exceptions;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Opportunity service implementation.
///
/// HEXAGONAL ARCHITECTURE:
/// - Implements IOpportunityInputPort (primary/driving port)
/// - Implements IOpportunityService (backward compatibility)
/// - Uses IRepository for data access (secondary/driven port)
/// </summary>
public class OpportunityService : IOpportunityService, IOpportunityInputPort
{
    private readonly IRepository<Opportunity> _repository;
    private readonly IRepository<CRM.Core.Entities.EntityTag> _entityTagRepository;
    private readonly IRepository<CRM.Core.Entities.CustomField> _customFieldRepository;
    private readonly NormalizationService _normalizationService;
    private readonly IEntityEventDispatcher _eventDispatcher;
    private readonly IDuplicateDetectionService _duplicateDetection;
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<OpportunityService> _logger;

    public OpportunityService(IRepository<Opportunity> repository,
        IRepository<CRM.Core.Entities.EntityTag> entityTagRepository,
        IRepository<CRM.Core.Entities.CustomField> customFieldRepository,
        NormalizationService normalizationService,
        IEntityEventDispatcher eventDispatcher,
        IDuplicateDetectionService duplicateDetection,
        ICrmDbContext dbContext,
        ILogger<OpportunityService> logger)
    {
        _repository = repository;
        _entityTagRepository = entityTagRepository;
        _customFieldRepository = customFieldRepository;
        _normalizationService = normalizationService;
        _eventDispatcher = eventDispatcher;
        _duplicateDetection = duplicateDetection;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Opportunity?> GetOpportunityByIdAsync(int id)
    {
        var opp = await _repository.GetByIdAsync(id);
        return opp;
    }

    public async Task<IEnumerable<Opportunity>> GetOpportunitiesByAccountAsync(int accountId)
    {
        var items = await _repository.FindAsync(o => !o.IsDeleted && o.AccountId == accountId);
        return items;
    }

    /// <summary>
    /// Get opportunities by customer ID (alias for GetOpportunitiesByAccountAsync for backward compatibility)
    /// </summary>
    public async Task<List<Opportunity>> GetOpportunitiesByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var opportunities = await _repository.FindAsync(o => !o.IsDeleted && o.AccountId == customerId);
        return opportunities?.ToList() ?? new List<Opportunity>();
    }

    public async Task<IEnumerable<Opportunity>> GetOpenOpportunitiesAsync()
    {
        var items = await _repository.FindAsync(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost);
        return items;
    }

    public async Task<int> CreateOpportunityAsync(Opportunity opportunity)
    {
        // Duplicate detection check before creation
        var fieldValues = new Dictionary<string, string?>
        {
            ["Name"] = opportunity.Name,
            ["AccountId"] = opportunity.AccountId.ToString(),
            ["Amount"] = opportunity.Amount.ToString("F2")
        };
        var candidatesQueued = await DuplicateCheckHelper.CheckAndHandleDuplicatesAsync(
            _duplicateDetection, _dbContext, "Opportunity", fieldValues, _logger);

        await _repository.AddAsync(opportunity);
        await _repository.SaveAsync();

        // Update any queued duplicate candidates with the new entity ID
        if (candidatesQueued > 0)
            await DuplicateCheckHelper.UpdateCandidateSourceIdsAsync(_dbContext, "Opportunity", opportunity.Id);

        // Fire workflow triggers for entity creation
        _eventDispatcher.DispatchEntityEvent("Opportunity", opportunity.Id, WorkflowTriggerType.OnCreate);

        return opportunity.Id;
    }

    public async Task UpdateOpportunityAsync(Opportunity opportunity)
    {
        opportunity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(opportunity);
        await _repository.SaveAsync();

        // Fire workflow triggers for entity update
        _eventDispatcher.DispatchEntityEvent("Opportunity", opportunity.Id, WorkflowTriggerType.OnUpdate);
    }

    public async Task DeleteOpportunityAsync(int id)
    {
        var opportunity = await _repository.GetByIdAsync(id);
        if (opportunity != null)
        {
            await _repository.DeleteAsync(opportunity);
            await _repository.SaveAsync();

            // Fire workflow triggers for entity deletion
            _eventDispatcher.DispatchEntityEvent("Opportunity", id, WorkflowTriggerType.OnDelete);
        }
    }

    public async Task<decimal> GetTotalPipelineAsync()
    {
        var opportunities = await GetOpenOpportunitiesAsync();
        return opportunities.Sum(o => o.Amount);
    }

    // --- Stage-to-Probability mapping (TODO-CRM003-02) ---

    /// <summary>
    /// Default probabilities per stage (percent).
    /// </summary>
    public static readonly Dictionary<OpportunityStage, int> StageProbabilityDefaults = new()
    {
        { OpportunityStage.Discovery,     10 },
        { OpportunityStage.Qualification, 25 },
        { OpportunityStage.Proposal,      50 },
        { OpportunityStage.Negotiation,   75 },
        { OpportunityStage.ClosedWon,    100 },
        { OpportunityStage.ClosedLost,     0 },
    };

    // --- Product management (TODO-CRM003-04) ---

    public async Task<IEnumerable<OpportunityProduct>> GetOpportunityProductsAsync(int opportunityId, CancellationToken ct = default)
    {
        return await _dbContext.Set<OpportunityProduct>()
            .Include(p => p.Product)
            .Where(p => p.OpportunityId == opportunityId && !p.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<OpportunityProduct> AddOpportunityProductAsync(int opportunityId, OpportunityProduct product, CancellationToken ct = default)
    {
        product.OpportunityId = opportunityId;
        product.CreatedAt = DateTime.UtcNow;
        product.IsDeleted = false;
        product.LineTotal = CalculateLineTotal(product);

        _dbContext.Set<OpportunityProduct>().Add(product);
        await _dbContext.SaveChangesAsync(ct);

        await RecalculateOpportunityAmountAsync(opportunityId, ct);
        return product;
    }

    public async Task<OpportunityProduct?> UpdateOpportunityProductAsync(int opportunityId, int productId, OpportunityProduct updated, CancellationToken ct = default)
    {
        var existing = await _dbContext.Set<OpportunityProduct>()
            .FirstOrDefaultAsync(p => p.OpportunityId == opportunityId && p.ProductId == productId && !p.IsDeleted, ct);

        if (existing == null)
            return null;

        existing.Quantity = updated.Quantity;
        existing.UnitPrice = updated.UnitPrice;
        existing.DiscountPercent = updated.DiscountPercent;
        existing.Notes = updated.Notes;
        existing.LineTotal = CalculateLineTotal(existing);

        await _dbContext.SaveChangesAsync(ct);
        await RecalculateOpportunityAmountAsync(opportunityId, ct);
        return existing;
    }

    public async Task<bool> RemoveOpportunityProductAsync(int opportunityId, int productId, CancellationToken ct = default)
    {
        var product = await _dbContext.Set<OpportunityProduct>()
            .FirstOrDefaultAsync(p => p.OpportunityId == opportunityId && p.ProductId == productId && !p.IsDeleted, ct);

        if (product == null)
            return false;

        product.IsDeleted = true;
        await _dbContext.SaveChangesAsync(ct);
        await RecalculateOpportunityAmountAsync(opportunityId, ct);
        return true;
    }

    private async Task RecalculateOpportunityAmountAsync(int opportunityId, CancellationToken ct)
    {
        var opportunity = await _repository.GetByIdAsync(opportunityId);
        if (opportunity == null)
            return;

        var products = await _dbContext.Set<OpportunityProduct>()
            .Where(p => p.OpportunityId == opportunityId && !p.IsDeleted)
            .ToListAsync(ct);

        var total = products.Sum(p => p.LineTotal ?? 0);
        if (total > 0)
        {
            opportunity.Amount = total;
            opportunity.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(opportunity);
            await _repository.SaveAsync();
        }
    }

    private static decimal? CalculateLineTotal(OpportunityProduct p)
    {
        if (p.UnitPrice == null)
            return null;

        var lineTotal = p.Quantity * p.UnitPrice.Value;
        if (p.DiscountPercent.HasValue && p.DiscountPercent.Value > 0)
            lineTotal = lineTotal * (1 - p.DiscountPercent.Value / 100);

        return Math.Round(lineTotal, 2);
    }

    // =============================================================================
    // Opportunity Cloning (TODO-CRM003-06)
    // =============================================================================

    public async Task<Opportunity> CloneAsync(int opportunityId, OpportunityCloneOptions? options = null, CancellationToken ct = default)
    {
        options ??= new OpportunityCloneOptions();

        var original = await _dbContext.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, ct);

        if (original == null)
            throw new NotFoundException($"Opportunity with ID {opportunityId} not found");

        // Create the cloned opportunity
        var cloned = new Opportunity
        {
            Name = options.NewName ?? $"Copy of {original.Name}",
            Description = original.Description,
            AccountId = options.NewAccountId ?? original.AccountId,
            Amount = original.Amount,
            Stage = options.ResetStage ? OpportunityStage.Discovery : original.Stage,
            Probability = options.ResetStage ? StageProbabilityDefaults[OpportunityStage.Discovery] : original.Probability,
            ExpectedCloseDate = options.NewExpectedCloseDate ?? original.ExpectedCloseDate,
            OwnerId = original.OwnerId,
            Source = original.Source,
            Type = original.Type,
            Priority = original.Priority,
            ForecastCategory = options.ResetStage ? ForecastCategory.Pipeline : original.ForecastCategory,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _dbContext.Opportunities.Add(cloned);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Cloned opportunity {OriginalId} to {ClonedId}: {Name}", 
            opportunityId, cloned.Id, cloned.Name);

        // Clone products if requested
        if (options.CloneProducts)
        {
            var products = await _dbContext.OpportunityProducts
                .AsNoTracking()
                .Where(p => p.OpportunityId == opportunityId && !p.IsDeleted)
                .ToListAsync(ct);

            foreach (var product in products)
            {
                var clonedProduct = new OpportunityProduct
                {
                    OpportunityId = cloned.Id,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                    UnitPrice = product.UnitPrice,
                    DiscountPercent = product.DiscountPercent,
                    LineTotal = product.LineTotal,
                    Notes = product.Notes,
                    SortOrder = product.SortOrder,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _dbContext.OpportunityProducts.Add(clonedProduct);
            }

            _logger.LogDebug("Cloned {Count} products to opportunity {Id}", products.Count, cloned.Id);
        }

        // Clone team members if requested
        if (options.CloneTeamMembers)
        {
            var teamMembers = await _dbContext.OpportunityTeamMembers
                .AsNoTracking()
                .Where(tm => tm.OpportunityId == opportunityId && !tm.IsDeleted)
                .ToListAsync(ct);

            foreach (var member in teamMembers)
            {
                var clonedMember = new OpportunityTeamMember
                {
                    OpportunityId = cloned.Id,
                    UserId = member.UserId,
                    Role = member.Role,
                    SplitPercentage = member.SplitPercentage,
                    IsPrimary = member.IsPrimary,
                    CommissionPlanId = member.CommissionPlanId,
                    Notes = member.Notes,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _dbContext.OpportunityTeamMembers.Add(clonedMember);
            }

            _logger.LogDebug("Cloned {Count} team members to opportunity {Id}", teamMembers.Count, cloned.Id);
        }

        // Clone competitors if requested
        if (options.CloneCompetitors)
        {
            var competitors = await _dbContext.OpportunityCompetitors
                .AsNoTracking()
                .Where(c => c.OpportunityId == opportunityId)
                .ToListAsync(ct);

            foreach (var competitor in competitors)
            {
                var clonedCompetitor = new OpportunityCompetitor
                {
                    OpportunityId = cloned.Id,
                    CompetitorId = competitor.CompetitorId,
                    ThreatLevel = competitor.ThreatLevel,
                    Status = CompetitorStatus.Active, // Reset status for new opportunity
                    StrengthsAgainstUs = competitor.StrengthsAgainstUs,
                    WeaknessesAgainstUs = competitor.WeaknessesAgainstUs,
                    Strategy = competitor.Strategy,
                    Notes = competitor.Notes
                };
                _dbContext.OpportunityCompetitors.Add(clonedCompetitor);
            }

            _logger.LogDebug("Cloned {Count} competitors to opportunity {Id}", competitors.Count, cloned.Id);
        }

        await _dbContext.SaveChangesAsync(ct);
        return cloned;
    }

    // =============================================================================
    // Team Member Management (TODO-CRM003-08)
    // =============================================================================

    public async Task<IEnumerable<OpportunityTeamMember>> GetTeamMembersAsync(int opportunityId, CancellationToken ct = default)
    {
        return await _dbContext.OpportunityTeamMembers
            .Include(tm => tm.User)
            .Where(tm => tm.OpportunityId == opportunityId && !tm.IsDeleted)
            .OrderByDescending(tm => tm.IsPrimary)
            .ThenBy(tm => tm.User.FirstName)
            .ToListAsync(ct);
    }

    public async Task<OpportunityTeamMember> AddTeamMemberAsync(int opportunityId, OpportunityTeamMember member, CancellationToken ct = default)
    {
        // Verify opportunity exists
        var opportunity = await _dbContext.Opportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, ct);

        if (opportunity == null)
            throw new NotFoundException($"Opportunity with ID {opportunityId} not found");

        // If this is primary, remove primary flag from others
        if (member.IsPrimary)
        {
            var existingPrimary = await _dbContext.OpportunityTeamMembers
                .Where(tm => tm.OpportunityId == opportunityId && tm.IsPrimary && !tm.IsDeleted)
                .ToListAsync(ct);

            foreach (var existing in existingPrimary)
            {
                existing.IsPrimary = false;
            }
        }

        member.OpportunityId = opportunityId;
        member.CreatedAt = DateTime.UtcNow;
        member.IsDeleted = false;

        _dbContext.OpportunityTeamMembers.Add(member);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Added team member {UserId} to opportunity {OpportunityId} with role {Role}", 
            member.UserId, opportunityId, member.Role);

        return member;
    }

    public async Task<OpportunityTeamMember?> UpdateTeamMemberAsync(int opportunityId, int memberId, OpportunityTeamMember updated, CancellationToken ct = default)
    {
        var existing = await _dbContext.OpportunityTeamMembers
            .FirstOrDefaultAsync(tm => tm.Id == memberId && tm.OpportunityId == opportunityId && !tm.IsDeleted, ct);

        if (existing == null)
            return null;

        // If setting as primary, remove primary flag from others
        if (updated.IsPrimary && !existing.IsPrimary)
        {
            var otherPrimary = await _dbContext.OpportunityTeamMembers
                .Where(tm => tm.OpportunityId == opportunityId && tm.IsPrimary && tm.Id != memberId && !tm.IsDeleted)
                .ToListAsync(ct);

            foreach (var other in otherPrimary)
            {
                other.IsPrimary = false;
            }
        }

        existing.Role = updated.Role;
        existing.SplitPercentage = updated.SplitPercentage;
        existing.IsPrimary = updated.IsPrimary;
        existing.CommissionPlanId = updated.CommissionPlanId;
        existing.Notes = updated.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Updated team member {MemberId} on opportunity {OpportunityId}", memberId, opportunityId);

        return existing;
    }

    public async Task<bool> RemoveTeamMemberAsync(int opportunityId, int memberId, CancellationToken ct = default)
    {
        var member = await _dbContext.OpportunityTeamMembers
            .FirstOrDefaultAsync(tm => tm.Id == memberId && tm.OpportunityId == opportunityId && !tm.IsDeleted, ct);

        if (member == null)
            return false;

        member.IsDeleted = true;
        member.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Removed team member {MemberId} from opportunity {OpportunityId}", memberId, opportunityId);

        return true;
    }

    // =============================================================================
    // Competitor Management (TODO-CRM003-03)
    // =============================================================================

    public async Task<IEnumerable<OpportunityCompetitor>> GetCompetitorsAsync(int opportunityId, CancellationToken ct = default)
    {
        return await _dbContext.OpportunityCompetitors
            .Include(c => c.Competitor)
            .Where(c => c.OpportunityId == opportunityId)
            .OrderByDescending(c => c.ThreatLevel)
            .ThenBy(c => c.Competitor.Name)
            .ToListAsync(ct);
    }

    public async Task<OpportunityCompetitor> AddCompetitorAsync(int opportunityId, OpportunityCompetitor competitor, CancellationToken ct = default)
    {
        // Verify opportunity exists
        var opportunity = await _dbContext.Opportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, ct);

        if (opportunity == null)
            throw new NotFoundException($"Opportunity with ID {opportunityId} not found");

        // Check if competitor already exists on this opportunity
        var existing = await _dbContext.OpportunityCompetitors
            .FirstOrDefaultAsync(c => c.OpportunityId == opportunityId && c.CompetitorId == competitor.CompetitorId, ct);

        if (existing != null)
            throw new ValidationException($"Competitor {competitor.CompetitorId} already exists on this opportunity");

        competitor.OpportunityId = opportunityId;
        _dbContext.OpportunityCompetitors.Add(competitor);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Added competitor {CompetitorId} to opportunity {OpportunityId}", 
            competitor.CompetitorId, opportunityId);

        return competitor;
    }

    public async Task<bool> RemoveCompetitorAsync(int opportunityId, int competitorId, CancellationToken ct = default)
    {
        var competitor = await _dbContext.OpportunityCompetitors
            .FirstOrDefaultAsync(c => c.OpportunityId == opportunityId && c.CompetitorId == competitorId, ct);

        if (competitor == null)
            return false;

        _dbContext.OpportunityCompetitors.Remove(competitor);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Removed competitor {CompetitorId} from opportunity {OpportunityId}", competitorId, opportunityId);

        return true;
    }

    /// <inheritdoc />
    public async Task<Opportunity> CloneAsync(int opportunityId, OpportunityCloneOptions? options = null, CancellationToken ct = default)
    {
        options ??= new OpportunityCloneOptions();

        var original = await _dbContext.Opportunities
            .Include(o => o.Products)
            .Include(o => o.TeamMembers)
            .Include(o => o.Competitors)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, ct);

        if (original == null)
            throw new NotFoundException($"Opportunity with ID {opportunityId} not found");

        var cloned = new Opportunity
        {
            Name = options.NewName ?? $"Copy of {original.Name}",
            AccountId = original.AccountId,
            ContactId = original.ContactId,
            OwnerId = original.OwnerId,
            Amount = original.Amount,
            Stage = OpportunityStage.Prospecting, // Reset to first stage
            Probability = 10, // Reset probability
            ExpectedCloseDate = DateTime.UtcNow.AddMonths(3), // New close date
            Description = original.Description,
            LeadSource = original.LeadSource,
            Type = original.Type,
            ForecastCategory = ForecastCategory.Pipeline,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _dbContext.Opportunities.Add(cloned);
        await _dbContext.SaveChangesAsync(ct);

        // Clone products if requested
        if (options.CloneProducts && original.Products != null)
        {
            foreach (var product in original.Products.Where(p => !p.IsDeleted))
            {
                var clonedProduct = new OpportunityProduct
                {
                    OpportunityId = cloned.Id,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                    UnitPrice = product.UnitPrice,
                    Discount = product.Discount,
                    TotalPrice = product.TotalPrice,
                    Description = product.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.OpportunityProducts.Add(clonedProduct);
            }
        }

        // Clone team members if requested
        if (options.CloneTeamMembers && original.TeamMembers != null)
        {
            foreach (var member in original.TeamMembers.Where(m => !m.IsDeleted))
            {
                var clonedMember = new OpportunityTeamMember
                {
                    OpportunityId = cloned.Id,
                    UserId = member.UserId,
                    Role = member.Role,
                    AccessLevel = member.AccessLevel,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.OpportunityTeamMembers.Add(clonedMember);
            }
        }

        // Clone competitors if requested
        if (options.CloneCompetitors && original.Competitors != null)
        {
            foreach (var competitor in original.Competitors.Where(c => !c.IsDeleted))
            {
                var clonedCompetitor = new OpportunityCompetitor
                {
                    OpportunityId = cloned.Id,
                    CompetitorId = competitor.CompetitorId,
                    ThreatLevel = competitor.ThreatLevel,
                    Strengths = competitor.Strengths,
                    Weaknesses = competitor.Weaknesses,
                    Notes = competitor.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.OpportunityCompetitors.Add(clonedCompetitor);
            }
        }

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Cloned opportunity {OriginalId} to new opportunity {ClonedId}", opportunityId, cloned.Id);

        return cloned;
    }
}
