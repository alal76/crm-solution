// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Scripting;

/// <summary>
/// Provides scripts with keyed, isolated access to a persistent state store
/// (e.g., Redis) scoped to the current script execution context.
/// </summary>
public interface IStateAccessor
{
    /// <summary>Retrieves a value by key; returns <c>null</c> if the key does not exist.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Stores or overwrites a value for the given key.</summary>
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>Removes a key from the state store. No-op if the key does not exist.</summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
