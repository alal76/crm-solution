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

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing addresses.
/// Provides CRUD operations for Address entities with support for soft deletes and optimistic concurrency.
/// </summary>
public interface IAddressService
{
    /// <summary>
    /// Create a new address for an account.
    /// </summary>
    /// <param name="accountId">The account ID to associate with the address</param>
    /// <param name="address">The address entity to create</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The created address with assigned ID</returns>
    Task<Address> CreateAddressAsync(int accountId, Address address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing address.
    /// </summary>
    /// <param name="accountId">The account ID that owns the address</param>
    /// <param name="addressId">The address ID to update</param>
    /// <param name="address">The updated address data</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The updated address</returns>
    Task<Address> UpdateAddressAsync(int accountId, int addressId, Address address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete (soft delete) an address.
    /// </summary>
    /// <param name="accountId">The account ID that owns the address</param>
    /// <param name="addressId">The address ID to delete</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>True if address was successfully deleted, false if not found</returns>
    Task<bool> DeleteAddressAsync(int accountId, int addressId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all addresses for an account.
    /// </summary>
    /// <param name="accountId">The account ID to retrieve addresses for</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Collection of addresses for the account</returns>
    Task<IEnumerable<Address>> GetAddressesByAccountAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set an address as the primary billing address for an account.
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <param name="addressId">The address ID to set as primary billing</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>True if successfully set, false if address not found</returns>
    Task<bool> SetPrimaryBillingAddressAsync(int accountId, int addressId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set an address as the primary shipping address for an account.
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <param name="addressId">The address ID to set as primary shipping</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>True if successfully set, false if address not found</returns>
    Task<bool> SetPrimaryShippingAddressAsync(int accountId, int addressId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a single address by ID.
    /// </summary>
    /// <param name="addressId">The address ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The address if found and not deleted, null otherwise</returns>
    Task<Address?> GetAddressByIdAsync(int addressId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get primary billing address for an account.
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The primary billing address if found, null otherwise</returns>
    Task<Address?> GetPrimaryBillingAddressAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get primary shipping address for an account.
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The primary shipping address if found, null otherwise</returns>
    Task<Address?> GetPrimaryShippingAddressAsync(int accountId, CancellationToken cancellationToken = default);
}
