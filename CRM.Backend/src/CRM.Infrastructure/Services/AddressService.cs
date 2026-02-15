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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for managing addresses.
/// Provides CRUD operations for Address entities with support for soft deletes, optimistic concurrency, and address linking.
///
/// FUNCTIONAL VIEW:
/// - Creating and updating addresses for accounts
/// - Managing primary billing and shipping addresses
/// - Soft-deleting addresses (preserves audit history)
/// - Retrieving addresses by account
///
/// TECHNICAL VIEW:
/// - Uses ICrmDbContext for database access
/// - Managed entity lifecycle with CreatedAt/UpdatedAt timestamps
/// - Optimistic concurrency control via RowVersion
/// - Polymorphic address linking through EntityAddressLink
/// - Proper logging and error handling
/// </summary>
public class AddressService : IAddressService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<AddressService> _logger;

    /// <summary>
    /// Initializes a new instance of AddressService with required dependencies.
    /// </summary>
    /// <param name="context">The database context for data access</param>
    /// <param name="logger">The logger instance for diagnostic information</param>
    public AddressService(ICrmDbContext context, ILogger<AddressService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new address for an account.
    /// </summary>
    public async Task<Address> CreateAddressAsync(int accountId, Address address, CancellationToken cancellationToken = default)
    {
        // Verify account exists and is not deleted
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);
        
        if (account == null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found or is deleted.", nameof(accountId));
        }

        // Validate address
        if (string.IsNullOrWhiteSpace(address.Line1))
        {
            throw new ArgumentException("Address Line1 (street address) is required.", nameof(address));
        }

        if (string.IsNullOrWhiteSpace(address.City))
        {
            throw new ArgumentException("Address City is required.", nameof(address));
        }

        if (string.IsNullOrWhiteSpace(address.Country))
        {
            throw new ArgumentException("Address Country is required.", nameof(address));
        }

        // Create new address entity
        var newAddress = new Address
        {
            Label = address.Label ?? "Primary",
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Addresses.Add(newAddress);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} created for account {AccountId}", newAddress.Id, accountId);
        return newAddress;
    }

    /// <summary>
    /// Update an existing address.
    /// </summary>
    public async Task<Address> UpdateAddressAsync(int accountId, int addressId, Address address, CancellationToken cancellationToken = default)
    {
        // Verify account exists and is not deleted
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);
        
        if (account == null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found or is deleted.", nameof(accountId));
        }

        // Get existing address
        var existingAddress = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted, cancellationToken);
        
        if (existingAddress == null)
        {
            throw new ArgumentException($"Address with ID {addressId} not found or is deleted.", nameof(addressId));
        }

        // Verify address is linked to the account
        var addressLink = await _context.EntityAddressLinks
            .FirstOrDefaultAsync(l => l.AddressId == addressId && l.EntityType == EntityType.Account && l.EntityId == accountId && !l.IsDeleted, cancellationToken);
        
        if (addressLink == null)
        {
            throw new InvalidOperationException(
                $"Address {addressId} is not linked to account {accountId}.");
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(address.Line1))
        {
            throw new ArgumentException("Address Line1 (street address) is required.", nameof(address));
        }

        if (string.IsNullOrWhiteSpace(address.City))
        {
            throw new ArgumentException("Address City is required.", nameof(address));
        }

        if (string.IsNullOrWhiteSpace(address.Country))
        {
            throw new ArgumentException("Address Country is required.", nameof(address));
        }

        // Update address properties
        existingAddress.Label = address.Label ?? existingAddress.Label;
        existingAddress.Line1 = address.Line1;
        existingAddress.Line2 = address.Line2;
        existingAddress.Line3 = address.Line3;
        existingAddress.City = address.City;
        existingAddress.State = address.State;
        existingAddress.PostalCode = address.PostalCode;
        existingAddress.County = address.County;
        existingAddress.CountryCode = address.CountryCode;
        existingAddress.Country = address.Country;
        existingAddress.ZipCodeId = address.ZipCodeId;
        existingAddress.LocalityId = address.LocalityId;
        existingAddress.Locality = address.Locality;
        existingAddress.Latitude = address.Latitude;
        existingAddress.Longitude = address.Longitude;
        existingAddress.GeocodeAccuracy = address.GeocodeAccuracy;
        existingAddress.IsVerified = address.IsVerified;
        existingAddress.VerifiedDate = address.VerifiedDate;
        existingAddress.VerificationSource = address.VerificationSource;
        existingAddress.IsResidential = address.IsResidential;
        existingAddress.DeliveryInstructions = address.DeliveryInstructions;
        existingAddress.AccessHours = address.AccessHours;
        existingAddress.SiteContactName = address.SiteContactName;
        existingAddress.SiteContactPhone = address.SiteContactPhone;
        existingAddress.Notes = address.Notes;
        existingAddress.UpdatedAt = DateTime.UtcNow;

        _context.Addresses.Update(existingAddress);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} updated for account {AccountId}", addressId, accountId);
        return existingAddress;
    }

    /// <summary>
    /// Delete (soft delete) an address.
    /// </summary>
    public async Task<bool> DeleteAddressAsync(int accountId, int addressId, CancellationToken cancellationToken = default)
    {
        // Verify account exists and is not deleted
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);
        
        if (account == null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found or is deleted.", nameof(accountId));
        }

        // Get existing address
        var existingAddress = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted, cancellationToken);
        
        if (existingAddress == null)
        {
            return false;
        }

        // Verify address is linked to the account
        var addressLink = await _context.EntityAddressLinks
            .FirstOrDefaultAsync(l => l.AddressId == addressId && l.EntityType == EntityType.Account && l.EntityId == accountId && !l.IsDeleted, cancellationToken);
        
        if (addressLink == null)
        {
            return false;
        }

        // Soft delete the address
        existingAddress.IsDeleted = true;
        existingAddress.UpdatedAt = DateTime.UtcNow;

        _context.Addresses.Update(existingAddress);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} deleted for account {AccountId}", addressId, accountId);
        return true;
    }

    /// <summary>
    /// Get all addresses for an account.
    /// </summary>
    public async Task<IEnumerable<Address>> GetAddressesByAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var addresses = await _context.EntityAddressLinks
            .Where(l => l.EntityType == EntityType.Account && l.EntityId == accountId && !l.IsDeleted)
            .Join(
                _context.Addresses.Where(a => !a.IsDeleted),
                link => link.AddressId,
                address => address.Id,
                (link, address) => address
            )
            .OrderBy(a => a.IsPrimary ? 0 : 1)
            .ThenBy(a => a.Label)
            .ToListAsync(cancellationToken);

        return addresses;
    }

    /// <summary>
    /// Get a single address by ID.
    /// </summary>
    public async Task<Address?> GetAddressByIdAsync(int addressId, CancellationToken cancellationToken = default)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted, cancellationToken);
        
        return address;
    }

    /// <summary>
    /// Get primary billing address for an account.
    /// </summary>
    public async Task<Address?> GetPrimaryBillingAddressAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var address = await _context.EntityAddressLinks
            .Where(l => l.EntityType == EntityType.Account 
                && l.EntityId == accountId 
                && !l.IsDeleted 
                && (l.AddressType == AddressType.Billing || l.AddressType == AddressType.Primary)
                && l.IsPrimary)
            .Join(
                _context.Addresses.Where(a => !a.IsDeleted),
                link => link.AddressId,
                address => address.Id,
                (link, address) => address
            )
            .FirstOrDefaultAsync(cancellationToken);

        return address;
    }

    /// <summary>
    /// Get primary shipping address for an account.
    /// </summary>
    public async Task<Address?> GetPrimaryShippingAddressAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var address = await _context.EntityAddressLinks
            .Where(l => l.EntityType == EntityType.Account 
                && l.EntityId == accountId 
                && !l.IsDeleted 
                && l.AddressType == AddressType.Shipping
                && l.IsPrimary)
            .Join(
                _context.Addresses.Where(a => !a.IsDeleted),
                link => link.AddressId,
                address => address.Id,
                (link, address) => address
            )
            .FirstOrDefaultAsync(cancellationToken);

        return address;
    }

    /// <summary>
    /// Set an address as the primary billing address for an account.
    /// </summary>
    public async Task<bool> SetPrimaryBillingAddressAsync(int accountId, int addressId, CancellationToken cancellationToken = default)
    {
        // Verify account exists
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);
        
        if (account == null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found or is deleted.", nameof(accountId));
        }

        // Verify address exists
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted, cancellationToken);
        
        if (address == null)
        {
            return false;
        }

        // Find the link for this address to the account
        var addressLink = await _context.EntityAddressLinks
            .FirstOrDefaultAsync(l => l.AddressId == addressId 
                && l.EntityType == EntityType.Account 
                && l.EntityId == accountId 
                && !l.IsDeleted, cancellationToken);
        
        if (addressLink == null)
        {
            return false;
        }

        // Remove primary flag from other billing/primary addresses
        var otherPrimaryBillingAddresses = await _context.EntityAddressLinks
            .Where(l => l.EntityType == EntityType.Account 
                && l.EntityId == accountId 
                && !l.IsDeleted 
                && (l.AddressType == AddressType.Billing || l.AddressType == AddressType.Primary)
                && l.IsPrimary
                && l.Id != addressLink.Id)
            .ToListAsync(cancellationToken);

        foreach (var link in otherPrimaryBillingAddresses)
        {
            link.IsPrimary = false;
            link.UpdatedAt = DateTime.UtcNow;
            _context.EntityAddressLinks.Update(link);
        }

        // Set this as primary billing
        addressLink.AddressType = AddressType.Billing;
        addressLink.IsPrimary = true;
        addressLink.UpdatedAt = DateTime.UtcNow;
        _context.EntityAddressLinks.Update(addressLink);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} set as primary billing for account {AccountId}", addressId, accountId);
        return true;
    }

    /// <summary>
    /// Set an address as the primary shipping address for an account.
    /// </summary>
    public async Task<bool> SetPrimaryShippingAddressAsync(int accountId, int addressId, CancellationToken cancellationToken = default)
    {
        // Verify account exists
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);
        
        if (account == null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found or is deleted.", nameof(accountId));
        }

        // Verify address exists
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted, cancellationToken);
        
        if (address == null)
        {
            return false;
        }

        // Find the link for this address to the account
        var addressLink = await _context.EntityAddressLinks
            .FirstOrDefaultAsync(l => l.AddressId == addressId 
                && l.EntityType == EntityType.Account 
                && l.EntityId == accountId 
                && !l.IsDeleted, cancellationToken);
        
        if (addressLink == null)
        {
            return false;
        }

        // Remove primary flag from other shipping addresses
        var otherPrimaryShippingAddresses = await _context.EntityAddressLinks
            .Where(l => l.EntityType == EntityType.Account 
                && l.EntityId == accountId 
                && !l.IsDeleted 
                && l.AddressType == AddressType.Shipping
                && l.IsPrimary
                && l.Id != addressLink.Id)
            .ToListAsync(cancellationToken);

        foreach (var link in otherPrimaryShippingAddresses)
        {
            link.IsPrimary = false;
            link.UpdatedAt = DateTime.UtcNow;
            _context.EntityAddressLinks.Update(link);
        }

        // Set this as primary shipping
        addressLink.AddressType = AddressType.Shipping;
        addressLink.IsPrimary = true;
        addressLink.UpdatedAt = DateTime.UtcNow;
        _context.EntityAddressLinks.Update(addressLink);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} set as primary shipping for account {AccountId}", addressId, accountId);
        return true;
    }
}
