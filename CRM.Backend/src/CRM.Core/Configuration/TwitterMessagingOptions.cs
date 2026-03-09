// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Configuration;

/// <summary>
/// Configuration options for the Twitter/X Direct Messaging provider.
/// Binds to <c>Providers:Messaging:Twitter</c> in appsettings.json.
/// <para>
/// COMM-003: Twitter outbound DMs require the $100/month Basic API tier or higher.
/// This provider is <b>mock-only</b>. <see cref="MockMode"/> is always <c>true</c>.
/// Inbound events are simulated via Mockoon webhook simulation only.
/// </para>
/// </summary>
public class TwitterMessagingOptions
{
    /// <summary>
    /// Configuration section path in appsettings.json.
    /// </summary>
    public const string SectionName = "Providers:Messaging:Twitter";

    /// <summary>
    /// Twitter API v2 Bearer Token for read-only access.
    /// Not used for outbound DMs (requires user-context auth and paid tier).
    /// </summary>
    public string BearerToken { get; set; } = string.Empty;

    /// <summary>
    /// Twitter App Consumer Key (API Key).
    /// Used to validate inbound webhook CRC challenges.
    /// </summary>
    public string ConsumerKey { get; set; } = string.Empty;

    /// <summary>
    /// Twitter App Consumer Secret (API Secret).
    /// Used as HMAC key for CRC challenge responses and inbound webhook signature validation.
    /// </summary>
    public string ConsumerSecret { get; set; } = string.Empty;

    /// <summary>
    /// OAuth 1.0a Access Token for user-context operations.
    /// Required for outbound DMs on paid API tier — not used in mock mode.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// OAuth 1.0a Access Token Secret for user-context operations.
    /// Required for outbound DMs on paid API tier — not used in mock mode.
    /// </summary>
    public string AccessTokenSecret { get; set; } = string.Empty;

    /// <summary>
    /// Set to <c>true</c> to activate the provider configuration.
    /// Even when <c>true</c>, outbound DMs are not available without a paid API tier.
    /// All send operations are no-ops in this implementation.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Indicates that this provider operates in mock mode only.
    /// Twitter outbound DMs require a $100/month Basic API tier minimum.
    /// This flag is always <c>true</c> in this implementation.
    /// </summary>
    public bool MockMode { get; set; } = true;
}
