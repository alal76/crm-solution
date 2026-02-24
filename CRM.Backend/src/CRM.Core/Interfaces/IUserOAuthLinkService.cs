// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for linking and unlinking OAuth provider accounts to CRM user accounts.
/// </summary>
public interface IUserOAuthLinkService
{
    /// <summary>
    /// Returns all OAuth provider links for the specified user.
    /// </summary>
    Task<IEnumerable<UserOAuthLink>> GetLinksAsync(int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Links an OAuth provider account to the CRM user.
    /// Throws <see cref="InvalidOperationException"/> if the link already exists.
    /// </summary>
    Task<UserOAuthLink> LinkProviderAsync(int userId, string provider,
        string providerUserId, string? providerEmail = null,
        string? accessToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlinks the specified OAuth provider from the CRM user.
    /// Throws <see cref="InvalidOperationException"/> if it is the sole auth method.
    /// Throws <see cref="KeyNotFoundException"/> if the link does not exist.
    /// </summary>
    Task UnlinkProviderAsync(int userId, string provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the user has a local password set (i.e., can authenticate
    /// without any linked OAuth provider).
    /// </summary>
    Task<bool> HasLocalPasswordAsync(int userId,
        CancellationToken cancellationToken = default);
}
