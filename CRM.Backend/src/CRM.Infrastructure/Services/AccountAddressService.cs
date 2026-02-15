using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing addresses linked to accounts.
/// Handles address CRUD operations, validation, primary address logic, and soft deletes.
/// </summary>
public class AccountAddressService : IAccountAddressService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<AccountAddressService> _logger;

    public AccountAddressService(ICrmDbContext context, ILogger<AccountAddressService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all addresses for an account, optionally filtered by type.
    /// </summary>
    public async Task<IEnumerable<Address>> GetAddressesAsync(int accountId, AddressType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.EntityAddressLinks
            .Where(l => l.EntityType == EntityType.Account && l.EntityId == accountId && !l.IsDeleted)
            .Join(
                _context.Addresses.Where(a => !a.IsDeleted),
                l => l.AddressId,
                a => a.Id,
                (link, address) => new { link, address }
            );

        if (type.HasValue)
        {
            query = query.Where(x => x.link.AddressType == type.Value);
        }

        var results = await query.Select(x => x.address).ToListAsync(cancellationToken);
        return results;
    }

    /// <summary>
    /// Get the primary address for an account.
    /// </summary>
    public async Task<Address?> GetPrimaryAddressAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var result = await _context.EntityAddressLinks
            .Where(l => l.EntityType == EntityType.Account && l.EntityId == accountId && !l.IsDeleted && l.IsPrimary)
            .Join(
                _context.Addresses.Where(a => !a.IsDeleted),
                l => l.AddressId,
                a => a.Id,
                (link, address) => address
            )
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Add an address to an account.
    /// </summary>
    public async Task<EntityAddressLink> AddAddressAsync(int accountId, int addressId, bool isPrimary = false, CancellationToken cancellationToken = default)
    {
        // Verify account exists
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);
        if (account == null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found.", nameof(accountId));
        }

        // Verify address exists
        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted, cancellationToken);
        if (address == null)
        {
            throw new ArgumentException($"Address with ID {addressId} not found.", nameof(addressId));
        }

        // Check for duplicate
        var existing = await _context.EntityAddressLinks
            .FirstOrDefaultAsync(l => l.EntityType == EntityType.Account && l.EntityId == accountId && l.AddressId == addressId && !l.IsDeleted, cancellationToken);

        if (existing != null)
        {
            throw new InvalidOperationException($"Address {addressId} is already linked to account {accountId}.");
        }

        // If marking as primary, remove primary flag from other addresses
        if (isPrimary)
        {
            var otherPrimaries = await _context.EntityAddressLinks
                .Where(l => l.EntityType == EntityType.Account && l.EntityId == accountId && l.IsPrimary && !l.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var link in otherPrimaries)
            {
                link.IsPrimary = false;
                link.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Create new link
        var newLink = new EntityAddressLink
        {
            EntityType = EntityType.Account,
            EntityId = accountId,
            AddressId = addressId,
            IsPrimary = isPrimary,
            CreatedAt = DateTime.UtcNow
        };

        _context.EntityAddressLinks.Add(newLink);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} linked to account {AccountId}", addressId, accountId);
        return newLink;
    }

    /// <summary>
    /// Remove an address from an account (soft delete).
    /// </summary>
    public async Task<bool> RemoveAddressAsync(int accountId, int addressId, CancellationToken cancellationToken = default)
    {
        var link = await _context.EntityAddressLinks
            .FirstOrDefaultAsync(l => l.EntityType == EntityType.Account && l.EntityId == accountId && l.AddressId == addressId && !l.IsDeleted, cancellationToken);

        if (link == null)
        {
            return false;
        }

        link.IsDeleted = true;
        link.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} removed from account {AccountId}", addressId, accountId);
        return true;
    }

    /// <summary>
    /// Update an address.
    /// </summary>
    public async Task<Address> UpdateAddressAsync(Address address, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == address.Id && !a.IsDeleted, cancellationToken);
        if (existing == null)
        {
            throw new ArgumentException($"Address with ID {address.Id} not found.", nameof(address));
        }

        // Update fields
        existing.Line1 = address.Line1;
        existing.Line2 = address.Line2;
        existing.City = address.City;
        existing.State = address.State;
        existing.PostalCode = address.PostalCode;
        existing.Country = address.Country;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} updated", address.Id);
        return existing;
    }

    /// <summary>
    /// Set an address as primary for an account.
    /// </summary>
    public async Task<bool> SetPrimaryAddressAsync(int accountId, int addressId, CancellationToken cancellationToken = default)
    {
        var link = await _context.EntityAddressLinks
            .FirstOrDefaultAsync(l => l.EntityType == EntityType.Account && l.EntityId == accountId && l.AddressId == addressId && !l.IsDeleted, cancellationToken);

        if (link == null)
        {
            return false;
        }

        // Remove primary from other addresses
        var otherPrimaries = await _context.EntityAddressLinks
            .Where(l => l.EntityType == EntityType.Account && l.EntityId == accountId && l.IsPrimary && !l.IsDeleted && l.Id != link.Id)
            .ToListAsync(cancellationToken);

        foreach (var other in otherPrimaries)
        {
            other.IsPrimary = false;
            other.UpdatedAt = DateTime.UtcNow;
        }

        // Set this as primary
        link.IsPrimary = true;
        link.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} set as primary for account {AccountId}", addressId, accountId);
        return true;
    }

    /// <summary>
    /// Validate an address.
    /// </summary>
    public async Task<AddressValidationResult> ValidateAddressAsync(Address address, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(address.Line1))
        {
            errors.Add("Street address (Line1) is required.");
        }

        if (string.IsNullOrWhiteSpace(address.City))
        {
            errors.Add("City is required.");
        }

        if (string.IsNullOrWhiteSpace(address.Country))
        {
            errors.Add("Country is required.");
        }

        if (string.IsNullOrWhiteSpace(address.PostalCode))
        {
            errors.Add("Postal code is required.");
        }

        return new AddressValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

/// <summary>
/// Interface for address validation results.
/// </summary>
public interface IAccountAddressService
{
    Task<IEnumerable<Address>> GetAddressesAsync(int accountId, AddressType? type = null, CancellationToken cancellationToken = default);
    Task<Address?> GetPrimaryAddressAsync(int accountId, CancellationToken cancellationToken = default);
    Task<EntityAddressLink> AddAddressAsync(int accountId, int addressId, bool isPrimary = false, CancellationToken cancellationToken = default);
    Task<bool> RemoveAddressAsync(int accountId, int addressId, CancellationToken cancellationToken = default);
    Task<Address> UpdateAddressAsync(Address address, CancellationToken cancellationToken = default);
    Task<bool> SetPrimaryAddressAsync(int accountId, int addressId, CancellationToken cancellationToken = default);
    Task<AddressValidationResult> ValidateAddressAsync(Address address, CancellationToken cancellationToken = default);
}

/// <summary>
/// Address validation result DTO.
/// </summary>
public class AddressValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
