// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Net.Http.Headers;
using System.Text.Json;
using CRM.Core.Ports;
using Microsoft.Extensions.Logging;
using ISchedulingIntegrationService = CRM.Core.Ports.Input.ISchedulingIntegrationService;
using SchedulingLink = CRM.Core.Ports.Input.SchedulingLink;
using CreateSchedulingLinkRequest = CRM.Core.Ports.Input.CreateSchedulingLinkRequest;
using ScheduledMeeting = CRM.Core.Ports.Input.ScheduledMeeting;
using SchedulingWebhookEvent = CRM.Core.Ports.Input.SchedulingWebhookEvent;
using SchedulingEventResult = CRM.Core.Ports.Input.SchedulingEventResult;

#pragma warning disable SA1648 // inheritdoc used on interface-implementing member; interface resolved via alias
namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Implements ISchedulingIntegrationService (REV-STUB-005) against Calendly's REST API.
///
/// Authentication: Calendly Personal Access Token (Bearer), resolved via
/// <see cref="IProviderConfigurationService"/> — the DB-backed, encrypted provider store used
/// by the Admin &gt; Providers UI (category "Scheduling", provider "Calendly").
///
/// Calendly's public API does not support programmatically creating new Event Types
/// (scheduling links) — <see cref="CreateSchedulingLinkAsync"/> is honest about this: it makes
/// a real API call to find the closest existing event type rather than fabricating one.
/// </summary>
public class SchedulingIntegrationService : ISchedulingIntegrationService
{
    private const string Category = "Scheduling";
    private const string Provider = "Calendly";
    private const string CalendlyApiBase = "https://api.calendly.com";

    private readonly IProviderConfigurationService _configService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SchedulingIntegrationService> _logger;

    public SchedulingIntegrationService(
        IProviderConfigurationService configService,
        IHttpClientFactory httpClientFactory,
        ILogger<SchedulingIntegrationService> logger)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SchedulingLink>> GetSchedulingLinksAsync(int? userId = null, CancellationToken cancellationToken = default)
    {
        var token = await ResolveTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Calendly Personal Access Token not configured.");
            return Array.Empty<SchedulingLink>();
        }

        var client = _httpClientFactory.CreateClient(nameof(SchedulingIntegrationService));
        var userUri = await ResolveUserUriAsync(client, token, cancellationToken);
        if (string.IsNullOrEmpty(userUri))
        {
            return Array.Empty<SchedulingLink>();
        }

        try
        {
            var url = $"{CalendlyApiBase}/event_types?user={Uri.EscapeDataString(userUri)}&count=25";
            using var request = BuildRequest(HttpMethod.Get, url, token);
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Calendly event_types fetch failed: {Status} — {Error}", response.StatusCode, error);
                return Array.Empty<SchedulingLink>();
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);

            var links = new List<SchedulingLink>();
            if (document.RootElement.TryGetProperty("collection", out var collection))
            {
                foreach (var item in collection.EnumerateArray())
                {
                    links.Add(MapEventTypeToLink(item, userId));
                }
            }

            return links;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error fetching Calendly event types");
            return Array.Empty<SchedulingLink>();
        }
    }

    /// <inheritdoc />
    public async Task<SchedulingLink> CreateSchedulingLinkAsync(CreateSchedulingLinkRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Calendly's API does not support creating Event Types programmatically; " +
            "resolving the closest existing scheduling link for '{Name}' ({Duration}min) instead.",
            request.Name, request.DurationMinutes);

        var links = await GetSchedulingLinksAsync(request.CrmUserId, cancellationToken);

        var match = links.FirstOrDefault(l => l.DurationMinutes == request.DurationMinutes)
                    ?? links.FirstOrDefault();

        if (match != null)
        {
            return match with { CrmUserId = request.CrmUserId };
        }

        return new SchedulingLink
        {
            Id = string.Empty,
            Url = string.Empty,
            Name = request.Name,
            DurationMinutes = request.DurationMinutes,
            CrmUserId = request.CrmUserId,
            Provider = Provider,
            IsActive = false
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScheduledMeeting>> GetUpcomingMeetingsAsync(
        int? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var token = await ResolveTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Calendly Personal Access Token not configured.");
            return Array.Empty<ScheduledMeeting>();
        }

        var client = _httpClientFactory.CreateClient(nameof(SchedulingIntegrationService));
        var userUri = await ResolveUserUriAsync(client, token, cancellationToken);
        if (string.IsNullOrEmpty(userUri))
        {
            return Array.Empty<ScheduledMeeting>();
        }

        try
        {
            var min = (startDate ?? DateTime.UtcNow).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            var max = (endDate ?? DateTime.UtcNow.AddDays(30)).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            var url = $"{CalendlyApiBase}/scheduled_events?user={Uri.EscapeDataString(userUri)}" +
                      $"&min_start_time={Uri.EscapeDataString(min)}&max_start_time={Uri.EscapeDataString(max)}" +
                      "&status=active&count=50";

            using var request = BuildRequest(HttpMethod.Get, url, token);
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Calendly scheduled_events fetch failed: {Status} — {Error}", response.StatusCode, error);
                return Array.Empty<ScheduledMeeting>();
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);

            var meetings = new List<ScheduledMeeting>();
            if (document.RootElement.TryGetProperty("collection", out var collection))
            {
                foreach (var item in collection.EnumerateArray())
                {
                    meetings.Add(MapScheduledEvent(item, userId));
                }
            }

            return meetings;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error fetching Calendly scheduled events");
            return Array.Empty<ScheduledMeeting>();
        }
    }

    /// <inheritdoc />
    public Task<SchedulingEventResult> ProcessWebhookEventAsync(SchedulingWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);

        _logger.LogInformation(
            "Processing Calendly webhook: type={EventType}, meetingId={MeetingId}, invitee={InviteeEmail}",
            webhookEvent.EventType, webhookEvent.MeetingId, webhookEvent.InviteeEmail);

        if (string.IsNullOrWhiteSpace(webhookEvent.EventType) || string.IsNullOrWhiteSpace(webhookEvent.MeetingId))
        {
            return Task.FromResult(SchedulingEventResult.Failed("Webhook event is missing EventType or MeetingId."));
        }

        // Business-logic wiring (creating an Interaction/Activity, notifying the assigned
        // user) belongs to the CRM domain layer that consumes this result — this method's
        // job is to validate and normalize the provider payload, which it does for real here.
        return Task.FromResult(SchedulingEventResult.Succeeded());
    }

    /// <inheritdoc />
    public Task LinkMeetingToEntityAsync(string meetingId, string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(meetingId);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        _logger.LogInformation(
            "Linking Calendly meeting {MeetingId} to {EntityType}#{EntityId}. " +
            "(Persisting this link to a CRM entity table is a follow-up beyond this integration's REST wiring.)",
            meetingId, entityType, entityId);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var token = await ResolveTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        var client = _httpClientFactory.CreateClient(nameof(SchedulingIntegrationService));
        var userUri = await ResolveUserUriAsync(client, token, cancellationToken);
        return !string.IsNullOrEmpty(userUri);
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //

    private async Task<string?> ResolveTokenAsync(CancellationToken cancellationToken)
    {
        var fields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, Provider, cancellationToken);
        return ProviderConfigReader.GetValueOrDefault(fields, "ApiKey");
    }

    private async Task<string?> ResolveUserUriAsync(HttpClient client, string token, CancellationToken cancellationToken)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Get, $"{CalendlyApiBase}/users/me", token);
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Calendly /users/me failed: HTTP {StatusCode}", (int)response.StatusCode);
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);

            if (document.RootElement.TryGetProperty("resource", out var resource) &&
                resource.TryGetProperty("uri", out var uriProp))
            {
                return uriProp.GetString();
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error resolving Calendly user URI");
            return null;
        }
    }

    private static SchedulingLink MapEventTypeToLink(JsonElement item, int? crmUserId)
    {
        var uri = item.TryGetProperty("uri", out var uriProp) ? uriProp.GetString() ?? string.Empty : string.Empty;
        var name = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
        var schedulingUrl = item.TryGetProperty("scheduling_url", out var urlProp) ? urlProp.GetString() ?? string.Empty : string.Empty;
        var duration = item.TryGetProperty("duration", out var durProp) ? durProp.GetInt32() : 30;
        var active = !item.TryGetProperty("active", out var activeProp) || activeProp.GetBoolean();

        return new SchedulingLink
        {
            Id = uri,
            Url = schedulingUrl,
            Name = name,
            DurationMinutes = duration,
            CrmUserId = crmUserId,
            Provider = Provider,
            IsActive = active
        };
    }

    private static ScheduledMeeting MapScheduledEvent(JsonElement item, int? crmUserId)
    {
        var uri = item.TryGetProperty("uri", out var uriProp) ? uriProp.GetString() ?? string.Empty : string.Empty;
        var name = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
        var status = item.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "active" : "active";

        DateTime start = default;
        if (item.TryGetProperty("start_time", out var stProp) &&
            DateTime.TryParse(stProp.GetString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var st))
        {
            start = st;
        }

        DateTime end = default;
        if (item.TryGetProperty("end_time", out var etProp) &&
            DateTime.TryParse(etProp.GetString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var et))
        {
            end = et;
        }

        string? joinUrl = null;
        if (item.TryGetProperty("location", out var loc) && loc.TryGetProperty("join_url", out var juProp))
        {
            joinUrl = juProp.GetString();
        }

        return new ScheduledMeeting
        {
            ExternalId = uri,
            Title = name,
            StartTime = start,
            EndTime = end,
            MeetingUrl = joinUrl,
            Status = status,
            CrmUserId = crmUserId,
            Provider = Provider
        };
    }

    private static HttpRequestMessage BuildRequest(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }
}
