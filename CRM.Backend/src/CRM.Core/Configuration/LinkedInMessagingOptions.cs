// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Configuration;

/// <summary>
/// Configuration options for the LinkedIn Messaging provider.
/// Binds to <c>Providers:Messaging:LinkedIn</c> in appsettings.json.
/// <para>
/// COMM-004: LinkedIn outbound messaging requires LinkedIn Sales Navigator
/// ($1,600+/year). This provider is <b>mock-only</b>. <see cref="MockMode"/>
/// is always <c>true</c>. Inbound events are simulated via Mockoon webhook
/// simulation only.
/// </para>
/// </summary>
public class LinkedInMessagingOptions
{
    /// <summary>
    /// Configuration section path in appsettings.json.
    /// </summary>
    public const string SectionName = "Providers:Messaging:LinkedIn";

    /// <summary>
    /// LinkedIn OAuth 2.0 Client ID from the Developer Portal.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// LinkedIn OAuth 2.0 Client Secret.
    /// Used to validate inbound webhook signatures when configured.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// LinkedIn OAuth 2.0 Access Token for API calls.
    /// Outbound messaging via the Messages API requires Sales Navigator — not used in mock mode.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Set to <c>true</c> to activate the provider configuration.
    /// Even when <c>true</c>, outbound messaging is not available without Sales Navigator.
    /// All send operations are no-ops in this implementation.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Indicates that this provider operates in mock mode only.
    /// LinkedIn outbound messaging requires Sales Navigator ($1,600+/year).
    /// This flag is always <c>true</c> in this implementation.
    /// </summary>
    public bool MockMode { get; set; } = true;
}
