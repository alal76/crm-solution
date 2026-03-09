// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Configuration;

/// <summary>
/// Configuration options for the Mailchimp contact-list sync integration.
/// Binds to <c>Integrations:Mailchimp</c> in appsettings.json.
/// Implements INT-002.
/// </summary>
public class MailchimpOptions
{
    /// <summary>Configuration section path in appsettings.json.</summary>
    public const string SectionName = "Integrations:Mailchimp";

    /// <summary>Mailchimp API key in the format <c>key-dcXX</c> (e.g., <c>abc123-us1</c>).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Mailchimp Audience / list ID to sync contacts into.</summary>
    public string ListId { get; set; } = string.Empty;

    /// <summary>
    /// Mailchimp data-centre server prefix (e.g., <c>us1</c>).
    /// When empty the service auto-parses it from the last segment of <see cref="ApiKey"/>.
    /// </summary>
    public string ServerPrefix { get; set; } = string.Empty;

    /// <summary>Set to true to activate the Mailchimp integration.</summary>
    public bool Enabled { get; set; } = false;
}
