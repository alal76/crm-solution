// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using System.Text.Json;
using CRM.Core.Configuration;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Messaging;

/// <summary>
/// Facebook Messenger messaging provider backed by the Facebook Graph API v18.0.
/// Uses <see cref="HttpClient"/> with a Page Access Token — no Facebook SDK dependency.
/// Gracefully degrades: when not configured, all operations log a warning and return
/// <c>false</c> without throwing exceptions.
/// </summary>
public class FacebookMessengerProvider : IFacebookMessengerProvider
{
    private const string GraphApiUrl =
        "https://graph.facebook.com/v18.0/me/messages";

    private readonly HttpClient _httpClient;
    private readonly FacebookMessengerOptions _options;
    private readonly ILogger<FacebookMessengerProvider> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="FacebookMessengerProvider"/>.
    /// </summary>
    public FacebookMessengerProvider(
        HttpClient httpClient,
        IOptions<FacebookMessengerOptions> options,
        ILogger<FacebookMessengerProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsAvailable =>
        _options.Enabled &&
        !string.IsNullOrWhiteSpace(_options.PageAccessToken);

    /// <inheritdoc />
    public async Task<bool> SendMessageAsync(
        string recipientPsid,
        string message,
        CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            _logger.LogWarning(
                "Facebook Messenger provider is not configured or disabled. " +
                "Message to PSID {Psid} skipped.", recipientPsid);
            return false;
        }

        var payload = new
        {
            recipient = new { id = recipientPsid },
            message = new { text = message }
        };

        var json = JsonSerializer.Serialize(payload);
        var url = $"{GraphApiUrl}?access_token={_options.PageAccessToken}";

        try
        {
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, ct).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Facebook Messenger: message sent to PSID {Psid}.", recipientPsid);
                return true;
            }

            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            _logger.LogWarning(
                "Facebook Graph API returned {Status} for PSID {Psid}. Body: {Body}",
                (int)response.StatusCode, recipientPsid, body);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex,
                "Network error sending Facebook Messenger message to PSID {Psid}.", recipientPsid);
            return false;
        }
    }
}
