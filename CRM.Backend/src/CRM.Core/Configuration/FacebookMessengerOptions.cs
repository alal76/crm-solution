// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Configuration;

/// <summary>
/// Configuration options for the Facebook Messenger messaging provider.
/// Binds to <c>Providers:Messaging:Facebook</c> in appsettings.json.
/// Uses the Facebook Graph API to send messages via a Facebook Page.
/// </summary>
public class FacebookMessengerOptions
{
    /// <summary>
    /// Configuration section path in appsettings.json.
    /// </summary>
    public const string SectionName = "Providers:Messaging:Facebook";

    /// <summary>
    /// Facebook Page Access Token used to authenticate Graph API calls.
    /// Obtained from the Facebook Developer portal for a specific Page.
    /// </summary>
    public string PageAccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Arbitrary string chosen during Facebook webhook setup in the Developer portal.
    /// The value sent by Facebook in the verification challenge must match this exactly.
    /// </summary>
    public string VerifyToken { get; set; } = string.Empty;

    /// <summary>
    /// Facebook App Secret used to validate the <c>X-Hub-Signature-256</c> HMAC header
    /// on inbound webhook events.
    /// </summary>
    public string AppSecret { get; set; } = string.Empty;

    /// <summary>
    /// Set to <c>true</c> to activate the provider. When <c>false</c> all send
    /// operations are no-ops that return <c>false</c> without making API calls.
    /// </summary>
    public bool Enabled { get; set; } = false;
}
