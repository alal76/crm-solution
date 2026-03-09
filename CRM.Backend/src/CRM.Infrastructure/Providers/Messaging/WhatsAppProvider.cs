// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Headers;
using System.Text;
using CRM.Core.Configuration;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Messaging;

/// <summary>
/// WhatsApp Business messaging provider backed by the Twilio REST API.
/// Uses <see cref="HttpClient"/> with HTTP Basic Auth — no Twilio SDK dependency.
/// Gracefully degrades: when not configured, all operations log a warning and return
/// <c>false</c> without throwing exceptions.
/// </summary>
public class WhatsAppProvider : IWhatsAppProvider
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppOptions _options;
    private readonly ILogger<WhatsAppProvider> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="WhatsAppProvider"/>.
    /// </summary>
    public WhatsAppProvider(
        HttpClient httpClient,
        IOptions<WhatsAppOptions> options,
        ILogger<WhatsAppProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        ApplyBasicAuth();
    }

    /// <inheritdoc />
    public bool IsAvailable =>
        _options.Enabled &&
        !string.IsNullOrWhiteSpace(_options.AccountSid) &&
        !string.IsNullOrWhiteSpace(_options.AuthToken);

    private void ApplyBasicAuth()
    {
        if (!string.IsNullOrWhiteSpace(_options.AccountSid) &&
            !string.IsNullOrWhiteSpace(_options.AuthToken))
        {
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_options.AccountSid}:{_options.AuthToken}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendMessageAsync(
        string toNumber,
        string message,
        CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            _logger.LogWarning(
                "WhatsApp provider is not configured or disabled. Message to {To} skipped.", toNumber);
            return false;
        }

        var formData = new Dictionary<string, string>
        {
            ["From"] = NormalizeNumber(_options.FromNumber),
            ["To"] = NormalizeNumber(toNumber),
            ["Body"] = message
        };

        return await PostToTwilioAsync(formData, toNumber, ct);
    }

    /// <inheritdoc />
    public async Task<bool> SendTemplateAsync(
        string toNumber,
        string templateName,
        Dictionary<string, string> parameters,
        CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            _logger.LogWarning(
                "WhatsApp provider is not configured or disabled. Template '{Template}' to {To} skipped.",
                templateName, toNumber);
            return false;
        }

        // For Twilio WhatsApp Sandbox, templates are sent as a formatted plain-text body.
        // Production deployments with approved templates should supply a ContentSid instead.
        var sb = new StringBuilder();
        sb.AppendLine(templateName);
        foreach (var kv in parameters)
        {
            sb.AppendLine($"{kv.Key}: {kv.Value}");
        }

        return await SendMessageAsync(toNumber, sb.ToString().Trim(), ct);
    }

    private async Task<bool> PostToTwilioAsync(
        Dictionary<string, string> formData,
        string toDisplay,
        CancellationToken ct)
    {
        var url = $"https://api.twilio.com/2010-04-01/Accounts/{_options.AccountSid}/Messages.json";

        try
        {
            using var content = new FormUrlEncodedContent(formData);
            using var response = await _httpClient.PostAsync(url, content, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WhatsApp message sent to {To}.", toDisplay);
                return true;
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning(
                "Twilio API returned {StatusCode} for WhatsApp message to {To}. Body: {Body}",
                (int)response.StatusCode, toDisplay, body);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {To}.", toDisplay);
            return false;
        }
    }

    /// <summary>
    /// Ensures the number has the <c>whatsapp:</c> URI prefix required by Twilio.
    /// </summary>
    private static string NormalizeNumber(string number) =>
        number.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase)
            ? number
            : $"whatsapp:{number}";
}
