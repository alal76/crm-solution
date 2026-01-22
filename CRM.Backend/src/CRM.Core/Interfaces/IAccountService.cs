using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

public interface IAccountService
{
    Task<object?> GetByIdAsync(int id);
    Task<object> CreateAsync(CreateAccountRequest request, string modifiedBy);
    Task<object?> UpdateAsync(int id, UpdateAccountRequest request, string modifiedBy);
}
