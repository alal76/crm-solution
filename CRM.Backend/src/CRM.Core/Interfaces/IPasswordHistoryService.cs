// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for tracking password history to prevent reuse of recent passwords.
/// </summary>
public interface IPasswordHistoryService
{
    /// <summary>
    /// Adds the given password hash to the user's password history.
    /// </summary>
    Task AddPasswordAsync(int userId, string passwordHash,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hashes <paramref name="plainTextPassword"/> with BCrypt and records it
    /// in the user's password history. Use this from layers that only have the
    /// plain-text password available (e.g., controllers).
    /// </summary>
    Task RecordNewPasswordAsync(int userId, string plainTextPassword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the plain-text password matches any of the user's last
    /// <paramref name="historyDepth"/> stored hashes (default 5).
    /// Returns true if the password was previously used (reuse detected).
    /// </summary>
    Task<bool> IsPasswordReusedAsync(int userId, string plainTextPassword,
        int historyDepth = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the full password history records for a user, ordered descending
    /// by creation date.
    /// </summary>
    Task<IEnumerable<PasswordHistory>> GetPasswordHistoryAsync(int userId,
        CancellationToken cancellationToken = default);
}
