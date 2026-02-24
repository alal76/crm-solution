// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces.Notifications;

/// <summary>
/// Interface for SMS notification channel.
/// Used for sending escalation notifications via SMS (Twilio, etc.).
/// TODO-SD005-009: SMS escalation notification interface.
/// </summary>
public interface ISmsNotificationChannel
{
    /// <summary>
    /// Sends an SMS notification to the specified phone number.
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number in E.164 format</param>
    /// <param name="message">Message content (max 160 chars for single SMS)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an SMS notification to multiple recipients.
    /// </summary>
    /// <param name="phoneNumbers">List of recipient phone numbers</param>
    /// <param name="message">Message content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of phone number to success status</returns>
    Task<Dictionary<string, bool>> SendBulkSmsAsync(IEnumerable<string> phoneNumbers, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a phone number format.
    /// </summary>
    /// <param name="phoneNumber">Phone number to validate</param>
    /// <returns>True if valid E.164 format</returns>
    bool ValidatePhoneNumber(string phoneNumber);

    /// <summary>
    /// Gets the remaining SMS quota for the current account.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Remaining SMS count, or -1 if unlimited</returns>
    Task<int> GetRemainingQuotaAsync(CancellationToken cancellationToken = default);
}
