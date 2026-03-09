// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Configuration;

/// <summary>
/// Configuration options for QuickBooks Online OAuth2 integration.
/// Binds to <c>Integrations:QuickBooks</c> in appsettings.json.
/// </summary>
public class QuickBooksOptions
{
    /// <summary>Configuration section path in appsettings.json.</summary>
    public const string SectionName = "Integrations:QuickBooks";

    /// <summary>Intuit OAuth2 client ID.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Intuit OAuth2 client secret.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>OAuth2 redirect URI registered in the Intuit developer portal.</summary>
    public string RedirectUri { get; set; } = "http://localhost:5000/api/integrations/quickbooks/callback";

    /// <summary>
    /// Intuit environment: "sandbox" (default) or "production".
    /// Determines the QuickBooks API base URL.
    /// </summary>
    public string Environment { get; set; } = "sandbox";

    /// <summary>Set to true to activate the QuickBooks integration.</summary>
    public bool Enabled { get; set; } = false;
}
