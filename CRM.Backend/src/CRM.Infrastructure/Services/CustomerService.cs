using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Customer service implementation
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _repository;

    public CustomerService(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
    {
        return await _repository.FindAsync(c =>
            !c.IsDeleted && (
                c.FirstName.Contains(searchTerm) ||
                c.LastName.Contains(searchTerm) ||
                c.Email.Contains(searchTerm) ||
                c.Company.Contains(searchTerm)
            )
        );
    }

    public async Task<int> CreateCustomerAsync(Customer customer)
    {
        await _repository.AddAsync(customer);
        await _repository.SaveAsync();
        return customer.Id;
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(customer);
        await _repository.SaveAsync();
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer != null)
        {
            await _repository.DeleteAsync(customer);
            await _repository.SaveAsync();
        }
    }
}
