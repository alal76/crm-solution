// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.RegularExpressions;
using CRM.Core.Interfaces.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio.Rest.Api.V2010.Account;
using TwilioClient = Twilio.TwilioClient;
using TwilioPhoneNumber = Twilio.Types.PhoneNumber;

namespace CRM.Infrastructure.Providers.Twilio;

/// <summary>
/// Twilio implementation of <see cref="ISmsNotificationService"/>.
/// Sends SMS and escalation alerts via the Twilio REST API.
/// TODO-SD005-009: SMS notification channel for escalations.
/// </summary>
public partial class TwilioSmsService : ISmsNotificationService
{
    private readonly TwilioConfiguration _config;
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of <see cref="TwilioSmsService"/>.
    /// </summary>
    public TwilioSmsService(
        IOptions<TwilioConfiguration> options,
        ILogger<TwilioSmsService> logger)
    {
        _config = options.Value;
        _logger = logger;
        _isInitialized = TryInitialize();
    }

    private bool TryInitialize()
    {
        if (!_config.IsValid())
        {
            _logger.LogWarning("Twilio configuration is invalid or missing. TwilioSmsService will not be functional.");
            return false;
        }

        try
        {
            TwilioClient.Init(_config.AccountSid, _config.AuthToken);
            _logger.LogInformation("TwilioSmsService initialized successfully (AccountSid: {AccountSid})", _config.AccountSid[..6] + "***");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Twilio client");
            return false;
        }
    }

    /// <inheritdoc />
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

        if (!_isInitialized)
        {
            _logger.LogWarning("Twilio not configured – SMS skipped to {PhoneNumber}", phoneNumber);
            return false;
        }

        try
        {
            var from = string.IsNullOrEmpty(_config.MessagingServiceSid)
                ? _config.FromPhoneNumber
                : null;

            var createParams = new CreateMessageOptions(new TwilioPhoneNumber(phoneNumber))
            {
                Body = message
            };

            if (!string.IsNullOrEmpty(_config.MessagingServiceSid))
                createParams.MessagingServiceSid = _config.MessagingServiceSid;
            else
                createParams.From = new TwilioPhoneNumber(_config.FromPhoneNumber);

            var msg = await MessageResource.CreateAsync(createParams);

            _logger.LogInformation(
                "Twilio SMS sent to {PhoneNumber}. SID={Sid} Status={Status}",
                phoneNumber, msg.Sid, msg.Status);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Twilio SMS to {PhoneNumber}", phoneNumber);
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

        foreach (var number in phoneNumbers)
        {
            results[number] = await SendSmsAsync(number, message, cancellationToken);
        }

        var successCount = results.Count(r => r.Value);
        _logger.LogInformation(
            "Bulk Twilio SMS: {Success}/{Total} sent successfully",
            successCount, results.Count);

        return results;
    }

    /// <inheritdoc />
    public async Task<bool> SendEscalationSmsAsync(
        string phoneNumber,
        string serviceRequestNumber,
        int escalationLevel,
        string summary,
        CancellationToken cancellationToken = default)
    {
        var message = $"[ESCALATION L{escalationLevel}] {serviceRequestNumber}: {summary}";

        _logger.LogInformation(
            "Sending escalation SMS via Twilio to {PhoneNumber} for SR {ServiceRequestNumber} Level {Level}",
            phoneNumber, serviceRequestNumber, escalationLevel);

        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    /// <inheritdoc />
    public bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        return PhoneNumberRegex().IsMatch(phoneNumber);
    }

    [GeneratedRegex(@"^\+?[1-9]\d{6,14}$")]
    private static partial Regex PhoneNumberRegex();
}
