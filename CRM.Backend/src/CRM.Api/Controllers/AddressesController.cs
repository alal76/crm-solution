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

namespace CRM.Api.Controllers;

/// <summary>
/// REST API Controller for Address management operations.
///
/// FUNCTIONAL VIEW:
/// This controller provides HTTP endpoints for:
/// - Retrieving addresses for accounts
/// - Creating new addresses
/// - Updating address information
/// - Deleting addresses (soft delete)
/// - Managing primary billing and shipping addresses
///
/// TECHNICAL VIEW:
/// - Uses IAddressService for business logic (dependency injected)
/// - All endpoints require authentication (JWT Bearer token)
/// - Returns standardized JSON responses with appropriate HTTP status codes
/// - Implements proper error handling with logging
///
/// API ROUTES:
/// - GET    /api/addresses/{accountId}                               - Get all addresses for account
/// - GET    /api/addresses/{accountId}/{addressId}                   - Get specific address
/// - POST   /api/addresses                                           - Create new address
/// - PUT    /api/addresses/{accountId}/{addressId}                   - Update address
/// - DELETE /api/addresses/{accountId}/{addressId}                   - Delete address
/// - POST   /api/addresses/{accountId}/{addressId}/set-primary-billing    - Set primary billing
/// - POST   /api/addresses/{accountId}/{addressId}/set-primary-shipping   - Set primary shipping
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly IAccountService _accountService;
    private readonly ILogger<AddressesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddressesController"/> class.
    /// </summary>
    /// <param name="addressService">Service for address business logic.</param>
    /// <param name="accountService">Service for account validation.</param>
    /// <param name="logger">Logger for error and audit logging.</param>
    public AddressesController(
        IAddressService addressService,
        IAccountService accountService,
        ILogger<AddressesController> logger)
    {
        _addressService = addressService ?? throw new ArgumentNullException(nameof(addressService));
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Convert Address entity to AddressDto.
    /// </summary>
    private static AddressDto MapAddressToDto(Address address)
    {
        return new AddressDto
        {
            Id = address.Id,
            Label = address.Label,
            Line1 = address.Line1,
            Line2 = address.Line2,
            Line3 = address.Line3,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            County = address.County,
            CountryCode = address.CountryCode,
            Country = address.Country,
            ZipCodeId = address.ZipCodeId,
            LocalityId = address.LocalityId,
            Locality = address.Locality,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            GeocodeAccuracy = address.GeocodeAccuracy,
            IsVerified = address.IsVerified,
            VerifiedDate = address.VerifiedDate,
            VerificationSource = address.VerificationSource,
            IsResidential = address.IsResidential,
            DeliveryInstructions = address.DeliveryInstructions,
            AccessHours = address.AccessHours,
            SiteContactName = address.SiteContactName,
            SiteContactPhone = address.SiteContactPhone,
            Notes = address.Notes,
            FormattedAddress = address.FormattedAddress,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }

    /// <summary>
    /// Get all addresses for a specific account.
    ///
    /// FUNCTIONAL: Returns list of all addresses associated with an account.
    /// TECHNICAL: Filters out soft-deleted records, returns 200 OK with array.
    /// </summary>
    /// <param name="accountId">The account ID to retrieve addresses for.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Array of AddressDto objects for the account.</returns>
    /// <response code="200">Returns the list of addresses for the account</response>
    /// <response code="400">If accountId is invalid</response>
    /// <response code="404">If account not found</response>
    /// <response code="500">If there was an internal error</response>
    [HttpGet("{accountId}")]
    [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccountAddresses(
        int accountId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (accountId <= 0)
                return BadRequest(new { message = "Invalid account ID. Must be greater than 0." });

            // Verify account exists
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            var addresses = await _addressService.GetAddressesByAccountAsync(accountId, cancellationToken);
            var addressDtos = addresses.Select(MapAddressToDto).ToList();

            return Ok(addressDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving addresses for account {AccountId}", accountId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error retrieving addresses", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific address by ID for an account.
    ///
    /// FUNCTIONAL: Returns detailed address information for viewing/editing.
    /// TECHNICAL: Returns 404 if address not found, deleted, or doesn't belong to account.
    /// </summary>
    /// <param name="accountId">The account ID that owns the address.</param>
    /// <param name="addressId">The address ID to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>AddressDto if found.</returns>
    /// <response code="200">Returns the address</response>
    /// <response code="400">If IDs are invalid</response>
    /// <response code="404">If address or account not found</response>
    /// <response code="500">If there was an internal error</response>
    [HttpGet("{accountId}/{addressId}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAddressById(
        int accountId,
        int addressId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (accountId <= 0 || addressId <= 0)
                return BadRequest(new { message = "Invalid account ID or address ID. Both must be greater than 0." });

            // Verify account exists
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            // Get address
            var address = await _addressService.GetAddressByIdAsync(addressId, cancellationToken);
            if (address == null || address.IsDeleted)
                return NotFound(new { message = "Address not found." });

            var addressDto = MapAddressToDto(address);
            return Ok(addressDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving address {AddressId} for account {AccountId}", addressId, accountId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error retrieving address", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new address for an account.
    ///
    /// FUNCTIONAL: Creates a new address record and associates it with an account.
    /// TECHNICAL: Returns 201 Created with Location header and the created AddressDto.
    /// </summary>
    /// <param name="dto">The address data to create.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The created AddressDto with assigned ID.</returns>
    /// <response code="201">Address successfully created</response>
    /// <response code="400">If request is invalid or account doesn't exist</response>
    /// <response code="500">If there was an internal error</response>
    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAddress(
        [FromBody] CreateAddressDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (dto == null)
                return BadRequest(new { message = "Request body cannot be empty." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var address = new Address
            {
                Label = dto.Label ?? "Primary",
                Line1 = dto.Line1,
                Line2 = dto.Line2,
                Line3 = dto.Line3,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                County = dto.County,
                CountryCode = dto.CountryCode,
                Country = dto.Country,
                ZipCodeId = dto.ZipCodeId,
                LocalityId = dto.LocalityId,
                Locality = dto.Locality,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                IsResidential = dto.IsResidential,
                DeliveryInstructions = dto.DeliveryInstructions,
                AccessHours = dto.AccessHours,
                SiteContactName = dto.SiteContactName,
                SiteContactPhone = dto.SiteContactPhone,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdAddress = await _addressService.CreateAddressAsync(0, address, cancellationToken);
            var addressDto = MapAddressToDto(createdAddress);

            return CreatedAtAction(
                nameof(GetAddressById),
                new { accountId = 0, addressId = createdAddress.Id },
                addressDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating address");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error creating address", error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing address.
    ///
    /// FUNCTIONAL: Updates address information for a specific address belonging to an account.
    /// TECHNICAL: Returns 200 OK with updated AddressDto, 404 if not found.
    /// </summary>
    /// <param name="accountId">The account ID that owns the address.</param>
    /// <param name="addressId">The address ID to update.</param>
    /// <param name="dto">The updated address data.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The updated AddressDto.</returns>
    /// <response code="200">Address successfully updated</response>
    /// <response code="400">If request is invalid</response>
    /// <response code="404">If address or account not found</response>
    /// <response code="500">If there was an internal error</response>
    [HttpPut("{accountId}/{addressId}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAddress(
        int accountId,
        int addressId,
        [FromBody] UpdateAddressDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (accountId <= 0 || addressId <= 0)
                return BadRequest(new { message = "Invalid account ID or address ID. Both must be greater than 0." });

            if (dto == null)
                return BadRequest(new { message = "Request body cannot be empty." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify account exists
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            // Get existing address
            var address = await _addressService.GetAddressByIdAsync(addressId, cancellationToken);
            if (address == null || address.IsDeleted)
                return NotFound(new { message = "Address not found." });

            // Update only the provided fields
            if (!string.IsNullOrWhiteSpace(dto.Label))
                address.Label = dto.Label;
            if (!string.IsNullOrWhiteSpace(dto.Line1))
                address.Line1 = dto.Line1;
            if (dto.Line2 != null)
                address.Line2 = dto.Line2;
            if (dto.Line3 != null)
                address.Line3 = dto.Line3;
            if (!string.IsNullOrWhiteSpace(dto.City))
                address.City = dto.City;
            if (dto.State != null)
                address.State = dto.State;
            if (dto.PostalCode != null)
                address.PostalCode = dto.PostalCode;
            if (dto.County != null)
                address.County = dto.County;
            if (dto.CountryCode != null)
                address.CountryCode = dto.CountryCode;
            if (!string.IsNullOrWhiteSpace(dto.Country))
                address.Country = dto.Country;
            if (dto.ZipCodeId.HasValue)
                address.ZipCodeId = dto.ZipCodeId;
            if (dto.LocalityId.HasValue)
                address.LocalityId = dto.LocalityId;
            if (dto.Locality != null)
                address.Locality = dto.Locality;
            if (dto.Latitude.HasValue)
                address.Latitude = dto.Latitude;
            if (dto.Longitude.HasValue)
                address.Longitude = dto.Longitude;
            if (dto.GeocodeAccuracy != null)
                address.GeocodeAccuracy = dto.GeocodeAccuracy;
            if (dto.IsVerified.HasValue)
                address.IsVerified = dto.IsVerified.Value;
            if (dto.VerificationSource != null)
                address.VerificationSource = dto.VerificationSource;
            if (dto.IsResidential.HasValue)
                address.IsResidential = dto.IsResidential;
            if (dto.DeliveryInstructions != null)
                address.DeliveryInstructions = dto.DeliveryInstructions;
            if (dto.AccessHours != null)
                address.AccessHours = dto.AccessHours;
            if (dto.SiteContactName != null)
                address.SiteContactName = dto.SiteContactName;
            if (dto.SiteContactPhone != null)
                address.SiteContactPhone = dto.SiteContactPhone;
            if (dto.Notes != null)
                address.Notes = dto.Notes;

            address.UpdatedAt = DateTime.UtcNow;

            var updatedAddress = await _addressService.UpdateAddressAsync(accountId, addressId, address, cancellationToken);
            var addressDto = MapAddressToDto(updatedAddress);

            return Ok(addressDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId} for account {AccountId}", addressId, accountId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error updating address", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete (soft delete) an address.
    ///
    /// FUNCTIONAL: Soft-deletes an address, preserving audit history.
    /// TECHNICAL: Sets IsDeleted = true, returns 204 No Content.
    /// </summary>
    /// <param name="accountId">The account ID that owns the address.</param>
    /// <param name="addressId">The address ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <response code="204">Address successfully deleted</response>
    /// <response code="400">If IDs are invalid</response>
    /// <response code="404">If address or account not found</response>
    /// <response code="500">If there was an internal error</response>
    [HttpDelete("{accountId}/{addressId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAddress(
        int accountId,
        int addressId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (accountId <= 0 || addressId <= 0)
                return BadRequest(new { message = "Invalid account ID or address ID. Both must be greater than 0." });

            // Verify account exists
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            // Delete address
            var deleted = await _addressService.DeleteAddressAsync(accountId, addressId, cancellationToken);
            if (!deleted)
                return NotFound(new { message = "Address not found." });

            _logger.LogInformation("Address {AddressId} deleted for account {AccountId}", addressId, accountId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId} for account {AccountId}", addressId, accountId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error deleting address", error = ex.Message });
        }
    }

    /// <summary>
    /// Set an address as the primary billing address for an account.
    ///
    /// FUNCTIONAL: Designates the specified address as the primary billing address.
    /// TECHNICAL: Returns 200 OK with updated AddressDto.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="addressId">The address ID to set as primary billing.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The updated AddressDto.</returns>
    /// <response code="200">Successfully set as primary billing address</response>
    /// <response code="400">If IDs are invalid</response>
    /// <response code="404">If address or account not found</response>
    /// <response code="500">If there was an internal error</response>
    [HttpPost("{accountId}/{addressId}/set-primary-billing")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetPrimaryBillingAddress(
        int accountId,
        int addressId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (accountId <= 0 || addressId <= 0)
                return BadRequest(new { message = "Invalid account ID or address ID. Both must be greater than 0." });

            // Verify account exists
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            // Set primary billing address
            var success = await _addressService.SetPrimaryBillingAddressAsync(accountId, addressId, cancellationToken);
            if (!success)
                return NotFound(new { message = "Address not found." });

            // Retrieve and return the updated address
            var address = await _addressService.GetAddressByIdAsync(addressId, cancellationToken);
            var addressDto = MapAddressToDto(address);

            _logger.LogInformation("Address {AddressId} set as primary billing for account {AccountId}", addressId, accountId);
            return Ok(addressDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary billing address {AddressId} for account {AccountId}", addressId, accountId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error setting primary billing address", error = ex.Message });
        }
    }

    /// <summary>
    /// Set an address as the primary shipping address for an account.
    ///
    /// FUNCTIONAL: Designates the specified address as the primary shipping address.
    /// TECHNICAL: Returns 200 OK with updated AddressDto.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="addressId">The address ID to set as primary shipping.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The updated AddressDto.</returns>
    /// <response code="200">Successfully set as primary shipping address</response>
    /// <response code="400">If IDs are invalid</response>
    /// <response code="404">If address or account not found</response>
    /// <response code="500">If there was an internal error</response>
    [HttpPost("{accountId}/{addressId}/set-primary-shipping")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetPrimaryShippingAddress(
        int accountId,
        int addressId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (accountId <= 0 || addressId <= 0)
                return BadRequest(new { message = "Invalid account ID or address ID. Both must be greater than 0." });

            // Verify account exists
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            // Set primary shipping address
            var success = await _addressService.SetPrimaryShippingAddressAsync(accountId, addressId, cancellationToken);
            if (!success)
                return NotFound(new { message = "Address not found." });

            // Retrieve and return the updated address
            var address = await _addressService.GetAddressByIdAsync(addressId, cancellationToken);
            var addressDto = MapAddressToDto(address);

            _logger.LogInformation("Address {AddressId} set as primary shipping for account {AccountId}", addressId, accountId);
            return Ok(addressDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary shipping address {AddressId} for account {AccountId}", addressId, accountId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error setting primary shipping address", error = ex.Message });
        }
    }
}
