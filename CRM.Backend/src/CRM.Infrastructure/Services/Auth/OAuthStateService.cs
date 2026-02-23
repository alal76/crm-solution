// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Interface for OAuth state parameter generation and validation (CSRF protection).
/// Implements the state parameter pattern per OAuth 2.0 RFC 6749 § 10.12.
/// </summary>
public interface IOAuthStateService
{
    /// <summary>
    /// Generates a cryptographically random state token with optional return URL.
    /// </summary>
    /// <param name="returnUrl">Optional URL to redirect to after OAuth callback.</param>
    /// <returns>Base64-encoded state token.</returns>
    string GenerateState(string? returnUrl = null);

    /// <summary>
    /// Validates and consumes a state token (one-time use).
    /// </summary>
    /// <param name="state">The state token to validate.</param>
    /// <param name="returnUrl">The return URL embedded in the state, if any.</param>
    /// <returns>True if the state was valid and consumed; false otherwise.</returns>
    bool ValidateState(string state, out string? returnUrl);
}

/// <summary>
/// In-memory OAuth state service with cryptographically random tokens and 10-minute expiry.
/// Uses ConcurrentDictionary for thread-safe state storage.
/// Tokens are one-time use (consumed on validation) to prevent replay attacks.
/// </summary>
public class OAuthStateService : IOAuthStateService
{
    private readonly ConcurrentDictionary<string, OAuthStateEntry> _stateStore = new();
    private readonly ILogger<OAuthStateService> _logger;
    private readonly TimeSpan _stateExpiry = TimeSpan.FromMinutes(10);

    public OAuthStateService(ILogger<OAuthStateService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string GenerateState(string? returnUrl = null)
    {
        // Purge expired entries periodically
        PurgeExpiredEntries();

        // Generate 32 bytes of cryptographically random data
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var stateToken = Convert.ToBase64String(randomBytes);

        var entry = new OAuthStateEntry
        {
            ReturnUrl = returnUrl,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(_stateExpiry)
        };

        _stateStore.TryAdd(stateToken, entry);

        _logger.LogDebug("OAuth state token generated, expires at {ExpiresAt}", entry.ExpiresAt);

        return stateToken;
    }

    /// <inheritdoc />
    public bool ValidateState(string state, out string? returnUrl)
    {
        returnUrl = null;

        if (string.IsNullOrWhiteSpace(state))
        {
            _logger.LogWarning("OAuth state validation failed: empty state parameter");
            return false;
        }

        // Attempt to remove (consume) the state token — one-time use
        if (!_stateStore.TryRemove(state, out var entry))
        {
            _logger.LogWarning("OAuth state validation failed: state token not found (possibly already consumed or never issued)");
            return false;
        }

        // Check expiry
        if (DateTime.UtcNow > entry.ExpiresAt)
        {
            _logger.LogWarning("OAuth state validation failed: state token expired at {ExpiresAt}", entry.ExpiresAt);
            return false;
        }

        returnUrl = entry.ReturnUrl;
        _logger.LogDebug("OAuth state token validated and consumed successfully");
        return true;
    }

    /// <summary>
    /// Remove expired entries to prevent unbounded memory growth.
    /// </summary>
    private void PurgeExpiredEntries()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _stateStore
            .Where(kvp => now > kvp.Value.ExpiresAt)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _stateStore.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Purged {Count} expired OAuth state entries", expiredKeys.Count);
        }
    }

    /// <summary>
    /// Internal state entry with metadata.
    /// </summary>
    private sealed class OAuthStateEntry
    {
        public string? ReturnUrl { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime ExpiresAt { get; init; }
    }
}
