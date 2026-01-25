using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;

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

    public OpportunityService(IRepository<Opportunity> repository,
        IRepository<CRM.Core.Entities.EntityTag> entityTagRepository,
        IRepository<CRM.Core.Entities.CustomField> customFieldRepository,
        NormalizationService normalizationService)
    {
        _repository = repository;
        _entityTagRepository = entityTagRepository;
        _customFieldRepository = customFieldRepository;
        _normalizationService = normalizationService;
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

    public async Task<IEnumerable<Opportunity>> GetOpenOpportunitiesAsync()
    {
        var items = await _repository.FindAsync(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost);
        return items;
    }

    public async Task<int> CreateOpportunityAsync(Opportunity opportunity)
    {
        await _repository.AddAsync(opportunity);
        await _repository.SaveAsync();
        return opportunity.Id;
    }

    public async Task UpdateOpportunityAsync(Opportunity opportunity)
    {
        opportunity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(opportunity);
        await _repository.SaveAsync();
    }

    public async Task DeleteOpportunityAsync(int id)
    {
        var opportunity = await _repository.GetByIdAsync(id);
        if (opportunity != null)
        {
            await _repository.DeleteAsync(opportunity);
            await _repository.SaveAsync();
        }
    }

    public async Task<decimal> GetTotalPipelineAsync()
    {
        var opportunities = await GetOpenOpportunitiesAsync();
        return opportunities.Sum(o => o.Amount);
    }
}
