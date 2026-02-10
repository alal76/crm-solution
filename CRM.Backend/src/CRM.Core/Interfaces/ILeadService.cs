using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for lead operations used by API controllers and other services.
/// </summary>
public interface ILeadService
{
    Task<(IEnumerable<object> Items, int TotalCount, int Page, int PageSize, int TotalPages)> GetAllAsync(int page = 1, int pageSize = 25);
    Task<object?> GetByIdAsync(int id);
    Task<int> CreateAsync(Lead lead);
    Task<bool> UpdateAsync(int id, Action<Lead> applyChanges);
    Task<bool> DeleteAsync(int id);
    Task<(int OpportunityId, int LeadId)> ConvertAsync(int id, string? opportunityName, int? accountId, decimal? estimatedValue, DateTime? expectedCloseDate);
    Task<IEnumerable<object>> GetByStatusAsync(LeadLifecycleStatus status);
    Task<object> GetStatsAsync();
    // Search and assignment helpers
    Task<IEnumerable<object>> SearchAsync(string searchTerm);
    Task<bool> AssignOwnerAsync(int leadId, int ownerId);
}
