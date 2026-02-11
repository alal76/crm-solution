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
using CRM.Api.Helpers;
using CRM.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
///
/// INDUSTRY STANDARD ALIAS:
/// All routes are also available under /api/accounts for industry-standard naming.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;
    private readonly ICrmNotificationService _notificationService;

    /// <summary>
    /// Initializes the controller with required services.
    /// </summary>
    /// <param name="accountService">Service for account business logic</param>
    /// <param name="logger">Logger for error and audit logging</param>
    /// <param name="notificationService">Service for SignalR real-time notifications</param>
    public AccountsController(
        IAccountService accountService,
        ILogger<AccountsController> logger,
        ICrmNotificationService notificationService)
    {
        _accountService = accountService;
        _logger = logger;
        _notificationService = notificationService;
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
    ///            Returns ETag header for optimistic concurrency control.
    ///            Supports If-None-Match header for cache validation.
    /// </summary>
    /// <param name="id">The unique account identifier</param>
    /// <returns>AccountDto if found</returns>
    /// <response code="200">Returns the account with ETag header</response>
    /// <response code="304">Not Modified - if If-None-Match matches current ETag</response>
    /// <response code="404">If account not found</response>
    /// <response code="500">If there was an internal error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromHeader(Name = "If-None-Match")] string? ifNoneMatch = null)
    {
        try
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound(new { message = "Account not found" });

            // Generate ETag from RowVersion
            var etag = ETagHelper.GenerateETag(account.RowVersion);

            // Check If-None-Match for cache validation
            if (!string.IsNullOrEmpty(ifNoneMatch) && !ETagHelper.IsNoneMatch(ifNoneMatch, account.RowVersion))
            {
                Response.Headers.ETag = etag;
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers.ETag = etag;
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {Id}", id);
            return StatusCode(500, new { message = "Error retrieving account", error = ex.Message });
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

            // Notify connected clients about the new account
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordCreatedAsync("Account", account.Id, account, userId);

            return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            return StatusCode(500, new { message = "Error creating account", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a account.
    ///
    /// FUNCTIONAL: Updates account information with conflict detection.
    /// TECHNICAL: Supports If-Match header for optimistic concurrency control.
    ///            Returns 409 Conflict if the record was modified by another user.
    ///            Returns updated ETag in response header.
    /// </summary>
    /// <param name="id">The account ID to update</param>
    /// <param name="dto">Updated account data</param>
    /// <param name="ifMatch">Optional ETag for optimistic concurrency check</param>
    /// <returns>Updated AccountDto with new ETag</returns>
    /// <response code="200">Returns the updated account with new ETag</response>
    /// <response code="404">If account not found</response>
    /// <response code="409">Conflict - record was modified by another user</response>
    /// <response code="412">Precondition Failed - If-Match ETag doesn't match</response>
    /// <response code="500">If there was an internal error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountDto dto,
        [FromHeader(Name = "If-Match")] string? ifMatch = null)
    {
        try
        {
            // If If-Match header provided, validate it first
            if (!string.IsNullOrEmpty(ifMatch))
            {
                var currentAccount = await _accountService.GetAccountByIdAsync(id);
                if (currentAccount == null)
                    return NotFound(new { message = "Account not found" });

                if (!ETagHelper.IsMatch(ifMatch, currentAccount.RowVersion))
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed, new
                    {
                        message = "The record has been modified by another user. Please refresh and try again.",
                        conflictType = "ETagMismatch",
                        currentETag = ETagHelper.GenerateETag(currentAccount.RowVersion)
                    });
                }
            }

            var account = await _accountService.UpdateAccountAsync(id, dto);
            if (account == null)
                return NotFound(new { message = "Account not found" });

            // Notify connected clients about the update
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordUpdatedAsync("Account", id, account, userId);

            // Return new ETag in response
            Response.Headers.ETag = ETagHelper.GenerateETag(account.RowVersion);
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

            // Notify connected clients about the deletion
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordDeletedAsync("Account", id, userId);

            return Ok(new { message = "Account deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account {Id}", id);
            return StatusCode(500, new { message = "Error deleting account", error = ex.Message });
        }
    }

    // === Direct Contact Management (One-to-Many) ===

    /// <summary>
    /// Get all contacts that directly belong to a account (via AccountId FK)
    /// </summary>
    [HttpGet("{id}/direct-contacts")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDirectContacts(int id)
    {
        try
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound(new { message = "Account not found" });

            var contacts = await _accountService.GetDirectContactsAsync(id);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving direct contacts for account {Id}", id);
            return StatusCode(500, new { message = "Error retrieving direct contacts", error = ex.Message });
        }
    }

    /// <summary>
    /// Assign a contact to a account (sets the contact's AccountId)
    /// </summary>
    [HttpPost("{id}/direct-contacts/{contactId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignContactToAccount(int id, int contactId)
    {
        try
        {
            var result = await _accountService.AssignContactToAccountAsync(id, contactId);
            if (!result)
                return NotFound(new { message = "Account or Contact not found" });

            return Ok(new { message = "Contact assigned to account successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning contact {ContactId} to account {Id}", contactId, id);
            return StatusCode(500, new { message = "Error assigning contact", error = ex.Message });
        }
    }

    /// <summary>
    /// Remove a contact from a account (clears the contact's AccountId)
    /// </summary>
    [HttpDelete("{id}/direct-contacts/{contactId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignContactFromAccount(int id, int contactId)
    {
        try
        {
            var result = await _accountService.UnassignContactFromAccountAsync(id, contactId);
            if (!result)
                return NotFound(new { message = "Account or Contact not found" });

            return Ok(new { message = "Contact removed from account successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning contact {ContactId} from account {Id}", contactId, id);
            return StatusCode(500, new { message = "Error unassigning contact", error = ex.Message });
        }
    }

    // === Contact Management for Organization Accounts (Many-to-Many via AccountContact) ===

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
