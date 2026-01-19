using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Opportunity service implementation
/// </summary>
public class OpportunityService : IOpportunityService
{
    private readonly IRepository<Opportunity> _repository;

    public OpportunityService(IRepository<Opportunity> repository)
    {
        _repository = repository;
    }

    public async Task<Opportunity?> GetOpportunityByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Opportunity>> GetOpportunitiesByCustomerAsync(int customerId)
    {
        return await _repository.FindAsync(o => !o.IsDeleted && o.CustomerId == customerId);
    }

    public async Task<IEnumerable<Opportunity>> GetOpenOpportunitiesAsync()
    {
        return await _repository.FindAsync(o => !o.IsDeleted && o.Stage != 4); // 4 = Closed
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
