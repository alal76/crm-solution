// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.RegularExpressions;
using CRM.Core.Interfaces.Notifications;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Notifications;

/// <summary>
/// Stub implementation of SMS notification service.
/// Can be replaced with Twilio provider when configured.
/// TODO-SD005-009: SMS notification channel for escalations.
/// </summary>
public partial class SmsNotificationService : ISmsNotificationService
{
    private readonly ILogger<SmsNotificationService> _logger;

    public SmsNotificationService(ILogger<SmsNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidPhoneNumber(phoneNumber))
        {
            _logger.LogWarning("Invalid phone number format: {PhoneNumber}", phoneNumber);
            return false;
        }

        // Stub implementation - logs the message
        _logger.LogInformation(
            "SMS (stub): To={PhoneNumber}, Message={Message}",
            phoneNumber,
            message.Length > 100 ? message[..100] + "..." : message);

        await Task.Delay(10, cancellationToken); // Simulate async operation
        return true;
    }

    public async Task<Dictionary<string, bool>> SendBulkSmsAsync(
        IEnumerable<string> phoneNumbers,
        string message,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>();

        foreach (var phoneNumber in phoneNumbers)
        {
            var success = await SendSmsAsync(phoneNumber, message, cancellationToken);
            results[phoneNumber] = success;
        }

        _logger.LogInformation(
            "Bulk SMS (stub): Sent to {Count} recipients, {SuccessCount} successful",
            results.Count,
            results.Count(r => r.Value));

        return results;
    }

    public async Task<bool> SendEscalationSmsAsync(
        string phoneNumber,
        string serviceRequestNumber,
        int escalationLevel,
        string summary,
        CancellationToken cancellationToken = default)
    {
        var message = $"[ESCALATION L{escalationLevel}] {serviceRequestNumber}: {summary}";

        _logger.LogInformation(
            "Escalation SMS (stub): To={PhoneNumber}, SR={ServiceRequestNumber}, Level={Level}",
            phoneNumber,
            serviceRequestNumber,
            escalationLevel);

        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // E.164 format: +[country code][subscriber number], 7-15 digits
        return PhoneNumberRegex().IsMatch(phoneNumber);
    }

    [GeneratedRegex(@"^\+?[1-9]\d{6,14}$")]
    private static partial Regex PhoneNumberRegex();
}
