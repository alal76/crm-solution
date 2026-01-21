using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Customer service interface
/// </summary>
public interface ICustomerService
{
    // Basic CRUD
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
    Task<bool> DeleteCustomerAsync(int id);
    
    // Category-based queries
    Task<IEnumerable<CustomerDto>> GetIndividualCustomersAsync();
    Task<IEnumerable<CustomerDto>> GetOrganizationCustomersAsync();
    
    // Contact management for organizations
    Task<CustomerContactDto?> LinkContactToCustomerAsync(int customerId, LinkContactToCustomerDto dto);
    Task<bool> UnlinkContactFromCustomerAsync(int customerId, int contactId);
    Task<CustomerContactDto?> UpdateCustomerContactAsync(int customerId, int contactId, UpdateCustomerContactDto dto);
    Task<IEnumerable<CustomerContactDto>> GetCustomerContactsAsync(int customerId);
    Task<bool> SetPrimaryContactAsync(int customerId, int contactId);
    
    // Additional queries
    Task<IEnumerable<CustomerDto>> GetCustomersByAssignedUserAsync(int userId);
    Task<IEnumerable<CustomerDto>> GetCustomersByLifecycleStageAsync(CustomerLifecycleStage stage);
    Task<IEnumerable<CustomerDto>> GetCustomersByPriorityAsync(CustomerPriority priority);
}

