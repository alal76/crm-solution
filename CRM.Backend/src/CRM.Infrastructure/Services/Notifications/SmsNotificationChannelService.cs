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
/// Stub implementation of SMS notification channel.
/// Uses Twilio as the underlying provider (placeholder).
/// TODO-SD005-009: SMS escalation notifications via Twilio.
/// </summary>
public class SmsNotificationChannelService : ISmsNotificationChannel
{
    private readonly ILogger<SmsNotificationChannelService> _logger;
    
    // E.164 format regex: +[country code][subscriber number]
    private static readonly Regex E164Regex = new(@"^\+[1-9]\d{1,14}$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    public SmsNotificationChannelService(ILogger<SmsNotificationChannelService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        if (!ValidatePhoneNumber(phoneNumber))
        {
            _logger.LogWarning("Invalid phone number format: {PhoneNumber}", phoneNumber);
            return false;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("Cannot send empty SMS message");
            return false;
        }

        try
        {
            // TODO: Replace with actual Twilio SDK integration (TODO-SD005-009) // NOSONAR
            _logger.LogInformation("SMS stub: Would send to {PhoneNumber}: {Message}", 
                phoneNumber, message.Length > 50 ? message.Substring(0, 50) + "..." : message);

            // Simulate async operation
            await Task.Delay(10, cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, bool>> SendBulkSmsAsync(
        IEnumerable<string> phoneNumbers, 
        string message, 
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>();

        foreach (var phoneNumber in phoneNumbers)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var success = await SendSmsAsync(phoneNumber, message, cancellationToken);
            results[phoneNumber] = success;
        }

        _logger.LogInformation("Bulk SMS completed: {Success}/{Total} successful", 
            results.Values.Count(v => v), results.Count);

        return results;
    }

    /// <inheritdoc />
    public bool ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        return E164Regex.IsMatch(phoneNumber);
    }

    /// <inheritdoc />
    public Task<int> GetRemainingQuotaAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement Twilio account balance/quota check // NOSONAR
        // For now, return -1 (unlimited) as this is a stub
        _logger.LogDebug("SMS quota check - returning unlimited (stub implementation)");
        return Task.FromResult(-1);
    }
}
