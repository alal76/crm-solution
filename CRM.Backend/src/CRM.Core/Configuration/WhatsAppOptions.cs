// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Configuration;

/// <summary>
/// Configuration options for the WhatsApp Business messaging provider.
/// Binds to <c>Providers:Messaging:WhatsApp</c> in appsettings.json.
/// Uses the Twilio WhatsApp Sandbox as the backing service.
/// </summary>
public class WhatsAppOptions
{
    /// <summary>
    /// Configuration section path in appsettings.json.
    /// </summary>
    public const string SectionName = "Providers:Messaging:WhatsApp";

    /// <summary>
    /// Twilio Account SID (starts with "AC").
    /// </summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>
    /// Twilio Auth Token used for API authentication and webhook signature validation.
    /// </summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// WhatsApp-prefixed from number.
    /// Default is the Twilio Sandbox number.
    /// Format must be <c>whatsapp:+E164</c>.
    /// </summary>
    public string FromNumber { get; set; } = "whatsapp:+14155238886";

    /// <summary>
    /// Set to <c>true</c> to activate the provider. When <c>false</c> all send
    /// operations are no-ops that return <c>false</c> without making API calls.
    /// </summary>
    public bool Enabled { get; set; } = false;
}
