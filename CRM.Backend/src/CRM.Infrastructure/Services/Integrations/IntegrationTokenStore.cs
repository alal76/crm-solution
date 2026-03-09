// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Concurrent;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Simple in-memory store for OAuth2 integration tokens.
/// Registered as a singleton so tokens survive the request scope.
///
/// TODO (INT-001 persistence): Persist tokens to the database using a
/// dedicated IntegrationToken table (provider, access_token, refresh_token,
/// expires_at, tenant_id) to survive application restarts. See
/// docs/11-specifications/SPEC-INT-001-AccountingSync.md for tracking.
/// </summary>
public class IntegrationTokenStore
{
    private readonly ConcurrentDictionary<string, string> _tokens = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Stores or replaces a token entry.</summary>
    public void Set(string key, string value) => _tokens[key] = value;

    /// <summary>Retrieves a token entry, returning null if not present.</summary>
    public string? Get(string key) => _tokens.TryGetValue(key, out var value) ? value : null;

    /// <summary>Returns true if there is a non-empty value for the given key.</summary>
    public bool Has(string key) => !string.IsNullOrEmpty(_tokens.GetValueOrDefault(key));
}
