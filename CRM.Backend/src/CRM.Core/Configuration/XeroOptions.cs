// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Configuration;

/// <summary>
/// Configuration options for Xero OAuth2 integration.
/// Binds to <c>Integrations:Xero</c> in appsettings.json.
/// </summary>
public class XeroOptions
{
    /// <summary>Configuration section path in appsettings.json.</summary>
    public const string SectionName = "Integrations:Xero";

    /// <summary>Xero OAuth2 client ID.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Xero OAuth2 client secret.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>OAuth2 redirect URI registered in the Xero developer portal.</summary>
    public string RedirectUri { get; set; } = "http://localhost:5000/api/integrations/xero/callback";

    /// <summary>Set to true to activate the Xero integration.</summary>
    public bool Enabled { get; set; } = false;
}
