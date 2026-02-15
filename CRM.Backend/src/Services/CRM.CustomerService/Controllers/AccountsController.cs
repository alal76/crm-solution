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
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CRM.Api.Controllers;

/// <summary>
/// REST API Controller for Account management operations.
///
/// FUNCTIONAL VIEW:
/// This controller provides HTTP endpoints for:
/// - Viewing and searching accounts (individuals and organizations)
/// - Creating new accounts with category-specific validation
/// - Updating account information
/// - Linking contacts to organization accounts
/// - Soft-deleting accounts
///
/// TECHNICAL VIEW:
/// - Uses IAccountService for business logic (dependency injected)
/// - All endpoints require authentication (JWT Bearer token)
/// - Returns standardized JSON responses with appropriate HTTP status codes
/// - Implements proper error handling with logging
///
/// API ROUTES:
/// - GET    /api/accounts              - Get all accounts
/// - GET    /api/accounts/{id}         - Get account by ID
/// - GET    /api/accounts/individuals  - Get individual accounts only
/// - GET    /api/accounts/organizations - Get organization accounts only
/// - GET    /api/accounts/search/{term} - Search accounts
/// - POST   /api/accounts              - Create new account
/// - PUT    /api/accounts/{id}         - Update account
/// - DELETE /api/accounts/{id}         - Delete account
/// - POST   /api/accounts/{id}/contacts - Link contact to organization
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;
    private readonly IContactInfoService _contactInfoService;

    /// <summary>
    /// Initializes the controller with required services.
    /// </summary>
    /// <param name="accountService">Service for account business logic</param>
    /// <param name="contactInfoService">Service for contact information</param>
    /// <param name="logger">Logger for error and audit logging</param>
    public AccountsController(IAccountService accountService, IContactInfoService contactInfoService, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _contactInfoService = contactInfoService;
        _logger = logger;
    }

    /// <summary>
    /// Get all accounts (both individuals and organizations).
    ///
    /// FUNCTIONAL: Returns list of all active accounts for dashboard views.
    /// TECHNICAL: Filters out soft-deleted records, returns 200 OK with array.
    /// </summary>
    /// <returns>Array of AccountDto objects</returns>
    /// <response code="200">Returns the list of accounts</response>
    /// <response code="500">If there was an internal error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts");
            return StatusCode(500, new { message = "Error retrieving accounts", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific account by their unique ID.
    ///
    /// FUNCTIONAL: Returns detailed account information for viewing/editing.
    /// TECHNICAL: Returns 404 if account not found or deleted.
    /// </summary>
    /// <param name="id">The unique account identifier</param>
    /// <returns>AccountDto if found</returns>
    /// <response code="200">Returns the account</response>
    /// <response code="404">If account not found</response>
    /// <response code="500">If there was an internal error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound(new { message = "Account not found" });
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {Id}", id);
            return StatusCode(500, new { message = "Error retrieving account", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all addresses linked to an account.
    /// </summary>
    [HttpGet("{id}/addresses")]
    [ProducesResponseType(typeof(IEnumerable<LinkedAddressDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAddresses(int id)
    {
        try
        {
            var addresses = await _accountService.GetAccountAddressesAsync(id);
            return Ok(addresses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving addresses for account {Id}", id);
            return StatusCode(500, new { message = "Error retrieving addresses", error = ex.Message });
        }
    }

    /// <summary>
    /// Get the primary billing address for an account.
    /// </summary>
    [HttpGet("{id}/addresses/primary-billing")]
    [ProducesResponseType(typeof(LinkedAddressDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrimaryBillingAddress(int id)
    {
        try
        {
            var address = await _accountService.GetPrimaryBillingAddressAsync(id);
            if (address == null)
                return NotFound(new { message = "Primary billing address not found" });

            return Ok(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary billing address for account {Id}", id);
            return StatusCode(500, new { message = "Error retrieving primary billing address", error = ex.Message });
        }
    }

    /// <summary>
    /// Get the primary shipping address for an account.
    /// </summary>
    [HttpGet("{id}/addresses/primary-shipping")]
    [ProducesResponseType(typeof(LinkedAddressDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrimaryShippingAddress(int id)
    {
        try
        {
            var address = await _accountService.GetPrimaryShippingAddressAsync(id);
            if (address == null)
                return NotFound(new { message = "Primary shipping address not found" });

            return Ok(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary shipping address for account {Id}", id);
            return StatusCode(500, new { message = "Error retrieving primary shipping address", error = ex.Message });
        }
    }

    /// <summary>
    /// Link a new or existing address to an account.
    /// </summary>
    [HttpPost("{id}/addresses")]
    [ProducesResponseType(typeof(LinkedAddressDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddAddress(int id, [FromBody] LinkAddressDto dto)
    {
        try
        {
            dto.EntityType = EntityType.Account.ToString();
            dto.EntityId = id;

            if (!dto.AddressId.HasValue && dto.NewAddress == null)
                return BadRequest(new { message = "AddressId or NewAddress is required." });

            var address = await _contactInfoService.LinkAddressAsync(dto);
            return CreatedAtAction(nameof(GetAddresses), new { id }, address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding address for account {Id}", id);
            return StatusCode(500, new { message = "Error adding address", error = ex.Message });
        }
    }

    /// <summary>
    /// Update an address linked to an account.
    /// </summary>
    [HttpPut("{id}/addresses/{addressId}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAddress(int id, int addressId, [FromBody] CreateAddressDto dto)
    {
        try
        {
            var address = await _contactInfoService.UpdateAddressAsync(addressId, dto);
            return Ok(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId} for account {Id}", addressId, id);
            return StatusCode(500, new { message = "Error updating address", error = ex.Message });
        }
    }

    /// <summary>
    /// Unlink an address from an account.
    /// </summary>
    [HttpDelete("{id}/addresses/{addressId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveAddress(int id, int addressId)
    {
        try
        {
            var links = await _accountService.GetAccountAddressesAsync(id);
            var link = links.FirstOrDefault(a => a.Id == addressId);
            if (link == null)
                return NotFound(new { message = "Address link not found" });

            await _contactInfoService.UnlinkAddressAsync(link.LinkId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing address {AddressId} from account {Id}", addressId, id);
            return StatusCode(500, new { message = "Error removing address", error = ex.Message });
        }
    }

    /// <summary>
    /// Set an address as the primary billing address for an account.
    /// </summary>
    [HttpPost("{id}/addresses/{addressId}/set-primary-billing")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetPrimaryBilling(int id, int addressId)
    {
        try
        {
            await _accountService.SetPrimaryBillingAddressAsync(id, addressId);
            return Ok(new { message = "Primary billing address updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary billing address for account {Id}", id);
            return StatusCode(500, new { message = "Error setting primary billing address", error = ex.Message });
        }
    }

    /// <summary>
    /// Set an address as the primary shipping address for an account.
    /// </summary>
    [HttpPost("{id}/addresses/{addressId}/set-primary-shipping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetPrimaryShipping(int id, int addressId)
    {
        try
        {
            await _accountService.SetPrimaryShippingAddressAsync(id, addressId);
            return Ok(new { message = "Primary shipping address updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary shipping address for account {Id}", id);
            return StatusCode(500, new { message = "Error setting primary shipping address", error = ex.Message });
        }
    }

    /// <summary>
    /// Get only individual (non-organization) accounts.
    ///
    /// FUNCTIONAL: Filters to show only person-type accounts.
    /// TECHNICAL: Filters by AccountCategory.Individual.
    /// </summary>
    /// <returns>Array of individual AccountDto objects</returns>
    /// <response code="200">Returns the list of individual accounts</response>
    [HttpGet("individuals")]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIndividuals()
    {
        try
        {
            var accounts = await _accountService.GetIndividualAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving individual accounts");
            return StatusCode(500, new { message = "Error retrieving accounts", error = ex.Message });
        }
    }

    /// <summary>
    /// Get only organization (company) accounts.
    ///
    /// FUNCTIONAL: Filters to show only company-type accounts.
    /// TECHNICAL: Filters by AccountCategory.Organization.
    /// </summary>
    /// <returns>Array of organization AccountDto objects</returns>
    /// <response code="200">Returns the list of organization accounts</response>
    [HttpGet("organizations")]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizations()
    {
        try
        {
            var accounts = await _accountService.GetOrganizationAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization accounts");
            return StatusCode(500, new { message = "Error retrieving accounts", error = ex.Message });
        }
    }

    /// <summary>
    /// Search accounts
    /// </summary>
    [HttpGet("search/{searchTerm}")]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(string searchTerm)
    {
        try
        {
            var accounts = await _accountService.SearchAccountsAsync(searchTerm);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching accounts for {SearchTerm}", searchTerm);
            return StatusCode(500, new { message = "Error searching accounts", error = ex.Message });
        }
    }

    /// <summary>
    /// Get accounts by lifecycle stage
    /// </summary>
    [HttpGet("by-stage/{stage}")]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByLifecycleStage(AccountLifecycleStage stage)
    {
        try
        {
            var accounts = await _accountService.GetAccountsByLifecycleStageAsync(stage);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts by stage {Stage}", stage);
            return StatusCode(500, new { message = "Error retrieving accounts", error = ex.Message });
        }
    }

    /// <summary>
    /// Get accounts by priority
    /// </summary>
    [HttpGet("by-priority/{priority}")]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPriority(AccountPriority priority)
    {
        try
        {
            var accounts = await _accountService.GetAccountsByPriorityAsync(priority);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts by priority {Priority}", priority);
            return StatusCode(500, new { message = "Error retrieving accounts", error = ex.Message });
        }
    }

    /// <summary>
    /// Get accounts assigned to a user
    /// </summary>
    [HttpGet("by-user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAssignedUser(int userId)
    {
        try
        {
            var accounts = await _accountService.GetAccountsByAssignedUserAsync(userId);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts for user {UserId}", userId);
            return StatusCode(500, new { message = "Error retrieving accounts", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
    {
        try
        {
            // Validate based on category
            if (dto.Category == AccountCategory.Individual)
            {
                if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
                    return BadRequest(new { message = "FirstName and LastName are required for Individual accounts" });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.Company))
                    return BadRequest(new { message = "Company name is required for Organization accounts" });
            }

            var account = await _accountService.CreateAccountAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            return StatusCode(500, new { message = "Error creating account", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a account
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountDto dto)
    {
        try
        {
            var account = await _accountService.UpdateAccountAsync(id, dto);
            if (account == null)
                return NotFound(new { message = "Account not found" });
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account {Id}", id);
            return StatusCode(500, new { message = "Error updating account", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a account (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _accountService.DeleteAccountAsync(id);
            if (!result)
                return NotFound(new { message = "Account not found" });
            return Ok(new { message = "Account deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account {Id}", id);
            return StatusCode(500, new { message = "Error deleting account", error = ex.Message });
        }
    }

    // === Contact Management for Organization Accounts ===

    /// <summary>
    /// Get all contacts linked to a account (organization)
    /// </summary>
    [HttpGet("{id}/contacts")]
    [ProducesResponseType(typeof(IEnumerable<AccountContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountContacts(int id)
    {
        try
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound(new { message = "Account not found" });

            var contacts = await _accountService.GetAccountContactsAsync(id);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contacts for account {Id}", id);
            return StatusCode(500, new { message = "Error retrieving contacts", error = ex.Message });
        }
    }

    /// <summary>
    /// Link a contact to a account (organization)
    /// </summary>
    [HttpPost("{id}/contacts")]
    [ProducesResponseType(typeof(AccountContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LinkContact(int id, [FromBody] LinkContactToAccountDto dto)
    {
        try
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound(new { message = "Account not found" });

            // Allow linking contacts to any account type (Individual or Organization)
            var result = await _accountService.LinkContactToAccountAsync(id, dto);
            if (result == null)
                return BadRequest(new { message = "Failed to link contact. Contact may not exist or is already linked." });

            _logger.LogInformation("Contact {ContactId} linked to account {AccountId}", dto.ContactId, id);
            return CreatedAtAction(nameof(GetAccountContacts), new { id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking contact to account {Id}", id);
            return StatusCode(500, new { message = "Error linking contact", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a account contact relationship
    /// </summary>
    [HttpPut("{id}/contacts/{contactId}")]
    [ProducesResponseType(typeof(AccountContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAccountContact(int id, int contactId, [FromBody] UpdateAccountContactDto dto)
    {
        try
        {
            var result = await _accountService.UpdateAccountContactAsync(id, contactId, dto);
            if (result == null)
                return NotFound(new { message = "Account contact relationship not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact {ContactId} for account {Id}", contactId, id);
            return StatusCode(500, new { message = "Error updating contact", error = ex.Message });
        }
    }

    /// <summary>
    /// Unlink a contact from a account
    /// </summary>
    [HttpDelete("{id}/contacts/{contactId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkContact(int id, int contactId)
    {
        try
        {
            var result = await _accountService.UnlinkContactFromAccountAsync(id, contactId);
            if (!result)
                return NotFound(new { message = "Account contact relationship not found" });

            _logger.LogInformation("Contact {ContactId} unlinked from account {AccountId}", contactId, id);
            return Ok(new { message = "Contact unlinked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking contact {ContactId} from account {Id}", contactId, id);
            return StatusCode(500, new { message = "Error unlinking contact", error = ex.Message });
        }
    }

    /// <summary>
    /// Set primary contact for a account
    /// </summary>
    [HttpPost("{id}/contacts/{contactId}/set-primary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimaryContact(int id, int contactId)
    {
        try
        {
            var result = await _accountService.SetPrimaryContactAsync(id, contactId);
            if (!result)
                return NotFound(new { message = "Account contact relationship not found" });

            _logger.LogInformation("Contact {ContactId} set as primary for account {AccountId}", contactId, id);
            return Ok(new { message = "Primary contact set successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary contact {ContactId} for account {Id}", contactId, id);
            return StatusCode(500, new { message = "Error setting primary contact", error = ex.Message });
        }
    }
}
