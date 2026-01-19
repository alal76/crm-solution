using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Customer service interface
/// </summary>
public interface ICustomerService
{
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
    Task<int> CreateCustomerAsync(Customer customer);
    Task UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(int id);
}
