using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Account service interface
/// </summary>
public interface IAccountService
{
    // Basic CRUD
    Task<AccountDto?> GetAccountByIdAsync(int id);
    Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
    Task<IEnumerable<AccountDto>> SearchAccountsAsync(string searchTerm);
    Task<AccountDto> CreateAccountAsync(CreateAccountDto dto);
    Task<AccountDto?> UpdateAccountAsync(int id, UpdateAccountDto dto);
    Task<bool> DeleteAccountAsync(int id);
    
    // Category-based queries
    Task<IEnumerable<AccountDto>> GetIndividualAccountsAsync();
    Task<IEnumerable<AccountDto>> GetOrganizationAccountsAsync();
    
    // Contact management for organizations
    Task<AccountContactDto?> LinkContactToAccountAsync(int accountId, LinkContactToAccountDto dto);
    Task<bool> UnlinkContactFromAccountAsync(int accountId, int contactId);
    Task<AccountContactDto?> UpdateAccountContactAsync(int accountId, int contactId, UpdateAccountContactDto dto);
    Task<IEnumerable<AccountContactDto>> GetAccountContactsAsync(int accountId);
    Task<bool> SetPrimaryContactAsync(int accountId, int contactId);
    
    // Direct contact management (one-to-many via Contact.AccountId)
    Task<IEnumerable<object>> GetDirectContactsAsync(int accountId);
    Task<bool> AssignContactToAccountAsync(int accountId, int contactId);
    Task<bool> UnassignContactFromAccountAsync(int accountId, int contactId);
    
    // Additional queries
    Task<IEnumerable<AccountDto>> GetAccountsByAssignedUserAsync(int userId);
    Task<IEnumerable<AccountDto>> GetAccountsByLifecycleStageAsync(AccountLifecycleStage stage);
    Task<IEnumerable<AccountDto>> GetAccountsByPriorityAsync(AccountPriority priority);
}

