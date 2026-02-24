// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for generating, validating, and sending passwordless magic-link tokens.
/// </summary>
public interface IMagicLinkService
{
    /// <summary>
    /// Generates a one-time magic-link token for the given email.
    /// Returns the generated <see cref="MagicLinkToken"/> record.
    /// Throws <see cref="KeyNotFoundException"/> if the email does not belong to any user.
    /// </summary>
    Task<MagicLinkToken> GenerateMagicLinkAsync(string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a magic-link token and, if valid, exchanges it for a JWT auth response.
    /// Marks the token as used on success.
    /// Throws <see cref="UnauthorizedAccessException"/> if the token is invalid, expired, or already used.
    /// </summary>
    Task<AuthResponse> ValidateMagicLinkAsync(string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the magic-link email to the specified address.
    /// </summary>
    Task SendMagicLinkEmailAsync(string email, string magicLink,
        CancellationToken cancellationToken = default);
}
