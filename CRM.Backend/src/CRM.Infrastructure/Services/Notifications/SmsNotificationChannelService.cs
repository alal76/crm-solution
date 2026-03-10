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
/// SMS notification channel implementation.
/// Delegates to ISmsNotificationService (Twilio) when available, falls back to logging stub.
/// </summary>
public class SmsNotificationChannelService : ISmsNotificationChannel
{
    private readonly ILogger<SmsNotificationChannelService> _logger;
    private readonly ISmsNotificationService? _smsService;

    // E.164 format regex: +[country code][subscriber number]
    private static readonly Regex E164Regex = new(@"^\+[1-9]\d{1,14}$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    public SmsNotificationChannelService(
        ILogger<SmsNotificationChannelService> logger,
        ISmsNotificationService? smsService = null)
    {
        _logger = logger;
        _smsService = smsService;
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
            if (_smsService != null)
            {
                return await _smsService.SendSmsAsync(phoneNumber, message, cancellationToken);
            }

            _logger.LogInformation("SMS (no provider configured): Would send to {PhoneNumber}: {Message}",
                phoneNumber, message.Length > 50 ? message[..50] + "..." : message);
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
    public async Task<int> GetRemainingQuotaAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SMS quota check - returning unlimited (quota tracking not implemented)");
        return await Task.FromResult(-1);
    }
}
