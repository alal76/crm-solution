// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Configuration;

/// <summary>
/// Configuration options for the HubSpot bidirectional contact/deal sync integration.
/// Binds to <c>Integrations:HubSpot</c> in appsettings.json.
/// Implements INT-002.
/// </summary>
public class HubSpotOptions
{
    /// <summary>Configuration section path in appsettings.json.</summary>
    public const string SectionName = "Integrations:HubSpot";

    /// <summary>HubSpot private-app access token.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Set to true to activate the HubSpot integration.</summary>
    public bool Enabled { get; set; } = false;
}
