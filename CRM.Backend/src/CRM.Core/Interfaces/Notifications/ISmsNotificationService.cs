// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces.Notifications;

/// <summary>
/// Interface for SMS notification service.
/// This is a pluggable interface that can be implemented by different providers (e.g., Twilio).
/// TODO-SD005-009: SMS notification channel for escalations.
/// </summary>
public interface ISmsNotificationService
{
    /// <summary>
    /// Sends an SMS message to a phone number.
    /// </summary>
    /// <param name="phoneNumber">The recipient's phone number (E.164 format preferred).</param>
    /// <param name="message">The message content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the message was sent successfully, false otherwise.</returns>
    Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an SMS message to multiple recipients.
    /// </summary>
    /// <param name="phoneNumbers">List of recipient phone numbers.</param>
    /// <param name="message">The message content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of phone number to success status.</returns>
    Task<Dictionary<string, bool>> SendBulkSmsAsync(
        IEnumerable<string> phoneNumbers,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an escalation notification via SMS.
    /// </summary>
    /// <param name="phoneNumber">The recipient's phone number.</param>
    /// <param name="serviceRequestNumber">The service request number being escalated.</param>
    /// <param name="escalationLevel">The current escalation level.</param>
    /// <param name="summary">Brief summary of the issue.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendEscalationSmsAsync(
        string phoneNumber,
        string serviceRequestNumber,
        int escalationLevel,
        string summary,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a phone number format.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <returns>True if the format is valid.</returns>
    bool IsValidPhoneNumber(string phoneNumber);
}
