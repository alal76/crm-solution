// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Infrastructure.Providers.Twilio;

/// <summary>
/// Configuration settings for Twilio notification provider.
/// Supports SMS, voice, and WhatsApp messaging.
/// </summary>
public class TwilioConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Providers:Notifications:Twilio";

    /// <summary>
    /// Twilio Account SID.
    /// </summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>
    /// Twilio Auth Token.
    /// </summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Default "From" phone number for SMS.
    /// Must be a Twilio phone number.
    /// </summary>
    public string FromPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Messaging Service SID (optional, alternative to FromPhoneNumber).
    /// </summary>
    public string? MessagingServiceSid { get; set; }

    /// <summary>
    /// WhatsApp "From" number (if WhatsApp is enabled).
    /// Format: whatsapp:+1234567890
    /// </summary>
    public string? WhatsAppFromNumber { get; set; }

    /// <summary>
    /// Webhook URL for delivery status callbacks.
    /// </summary>
    public string? StatusCallbackUrl { get; set; }

    /// <summary>
    /// Whether to enable delivery status tracking.
    /// </summary>
    public bool TrackDeliveryStatus { get; set; } = true;

    /// <summary>
    /// Whether this is a test/sandbox environment.
    /// </summary>
    public bool IsSandbox { get; set; }

    /// <summary>
    /// Maximum SMS segment length.
    /// Standard is 160 for GSM, 70 for Unicode.
    /// </summary>
    public int MaxSegmentLength { get; set; } = 160;

    /// <summary>
    /// Rate limit (messages per second).
    /// </summary>
    public int RateLimitPerSecond { get; set; } = 1;

    /// <summary>
    /// Whether to enable test mode (no actual messages sent).
    /// </summary>
    public bool TestMode { get; set; }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(AccountSid) &&
               !string.IsNullOrEmpty(AuthToken) &&
               (!string.IsNullOrEmpty(FromPhoneNumber) || !string.IsNullOrEmpty(MessagingServiceSid));
    }
}
