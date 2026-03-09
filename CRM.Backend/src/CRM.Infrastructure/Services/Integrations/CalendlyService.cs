// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CRM.Core.Configuration;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Calendly scheduling integration service.
/// Implements outbound API calls for webhook registration and event retrieval.
/// Implements INT-004.
/// </summary>
public class CalendlyService : ICalendlyService
{
    private readonly CalendlyOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CalendlyService> _logger;

    private const string CalendlyApiBase = "https://api.calendly.com";

    /// <summary>
    /// Initializes a new instance of <see cref="CalendlyService"/>.
    /// </summary>
    public CalendlyService(
        IOptions<CalendlyOptions> options,
        HttpClient httpClient,
        ILogger<CalendlyService> logger)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> RegisterWebhookAsync(string webhookUrl, CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Calendly integration is disabled. Skipping webhook registration.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_options.PersonalAccessToken))
        {
            _logger.LogWarning(
                "Calendly PersonalAccessToken is not configured. Cannot register webhook.");
            return false;
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.PersonalAccessToken);

            var requestBody = JsonSerializer.Serialize(new
            {
                url = webhookUrl,
                events = new[] { "invitee.created", "invitee.canceled" },
                organization = "https://api.calendly.com/organizations/PLACEHOLDER",
                scope = "user"
            });

            using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(
                $"{CalendlyApiBase}/webhook_subscriptions", content, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Calendly webhook registered successfully at {WebhookUrl}.", webhookUrl);
                return true;
            }

            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning(
                "Calendly webhook registration failed (HTTP {StatusCode}): {Error}",
                (int)response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error registering Calendly webhook at {WebhookUrl}.", webhookUrl);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CalendlyEventDto>> GetUpcomingEventsAsync(CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Calendly integration is disabled. Returning empty event list.");
            return Enumerable.Empty<CalendlyEventDto>();
        }

        if (string.IsNullOrWhiteSpace(_options.PersonalAccessToken))
        {
            _logger.LogWarning(
                "Calendly PersonalAccessToken is not configured. Cannot retrieve events.");
            return Enumerable.Empty<CalendlyEventDto>();
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.PersonalAccessToken);

            // Step 1: Resolve the authenticated user's URI
            var meResponse = await _httpClient.GetAsync($"{CalendlyApiBase}/users/me", ct);
            if (!meResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to retrieve Calendly user info (HTTP {StatusCode}).",
                    (int)meResponse.StatusCode);
                return Enumerable.Empty<CalendlyEventDto>();
            }

            var meBody = await meResponse.Content.ReadAsStringAsync(ct);
            using var meDoc = JsonDocument.Parse(meBody);

            var userUri = string.Empty;
            if (meDoc.RootElement.TryGetProperty("resource", out var resource) &&
                resource.TryGetProperty("uri", out var uriProp))
            {
                userUri = uriProp.GetString() ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(userUri))
            {
                _logger.LogWarning("Could not resolve Calendly user URI from /users/me response.");
                return Enumerable.Empty<CalendlyEventDto>();
            }

            // Step 2: Retrieve upcoming scheduled events
            var eventsUrl =
                $"{CalendlyApiBase}/scheduled_events?user={Uri.EscapeDataString(userUri)}&count=10&status=active";
            var eventsResponse = await _httpClient.GetAsync(eventsUrl, ct);

            if (!eventsResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to retrieve Calendly scheduled events (HTTP {StatusCode}).",
                    (int)eventsResponse.StatusCode);
                return Enumerable.Empty<CalendlyEventDto>();
            }

            var eventsBody = await eventsResponse.Content.ReadAsStringAsync(ct);
            using var eventsDoc = JsonDocument.Parse(eventsBody);

            var events = new List<CalendlyEventDto>();

            if (eventsDoc.RootElement.TryGetProperty("collection", out var collection))
            {
                foreach (var evt in collection.EnumerateArray())
                {
                    var dto = new CalendlyEventDto();

                    if (evt.TryGetProperty("name", out var nameProp))
                        dto.EventName = nameProp.GetString();

                    if (evt.TryGetProperty("start_time", out var stProp) &&
                        DateTime.TryParse(
                            stProp.GetString(), null,
                            System.Globalization.DateTimeStyles.RoundtripKind, out var st))
                        dto.StartTime = st;

                    if (evt.TryGetProperty("end_time", out var etProp) &&
                        DateTime.TryParse(
                            etProp.GetString(), null,
                            System.Globalization.DateTimeStyles.RoundtripKind, out var et))
                        dto.EndTime = et;

                    if (evt.TryGetProperty("location", out var loc) &&
                        loc.TryGetProperty("join_url", out var juProp))
                        dto.JoinUrl = juProp.GetString();

                    events.Add(dto);
                }
            }

            _logger.LogInformation(
                "Retrieved {Count} upcoming Calendly events.", events.Count);
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming Calendly events.");
            return Enumerable.Empty<CalendlyEventDto>();
        }
    }
}
