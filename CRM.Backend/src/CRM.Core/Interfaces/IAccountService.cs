// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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

    // Address management (normalized)
    Task<List<LinkedAddressDto>> GetAccountAddressesAsync(int accountId);
    Task<LinkedAddressDto?> GetPrimaryBillingAddressAsync(int accountId);
    Task<LinkedAddressDto?> GetPrimaryShippingAddressAsync(int accountId);
    Task SetPrimaryBillingAddressAsync(int accountId, int addressId);
    Task SetPrimaryShippingAddressAsync(int accountId, int addressId);

    // Additional queries
    Task<IEnumerable<AccountDto>> GetAccountsByAssignedUserAsync(int userId);
    Task<IEnumerable<AccountDto>> GetAccountsByLifecycleStageAsync(AccountLifecycleStage stage);
    Task<IEnumerable<AccountDto>> GetAccountsByPriorityAsync(AccountPriority priority);
}
