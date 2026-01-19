using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Opportunity service interface
/// </summary>
public interface IOpportunityService
{
    Task<Opportunity?> GetOpportunityByIdAsync(int id);
    Task<IEnumerable<Opportunity>> GetOpportunitiesByCustomerAsync(int customerId);
    Task<IEnumerable<Opportunity>> GetOpenOpportunitiesAsync();
    Task<int> CreateOpportunityAsync(Opportunity opportunity);
    Task UpdateOpportunityAsync(Opportunity opportunity);
    Task DeleteOpportunityAsync(int id);
    Task<decimal> GetTotalPipelineAsync();
}
