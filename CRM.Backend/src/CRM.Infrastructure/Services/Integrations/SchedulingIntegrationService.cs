// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using ISchedulingIntegrationService = CRM.Core.Ports.Input.ISchedulingIntegrationService;
using SchedulingLink = CRM.Core.Ports.Input.SchedulingLink;
using CreateSchedulingLinkRequest = CRM.Core.Ports.Input.CreateSchedulingLinkRequest;
using ScheduledMeeting = CRM.Core.Ports.Input.ScheduledMeeting;
using SchedulingWebhookEvent = CRM.Core.Ports.Input.SchedulingWebhookEvent;
using SchedulingEventResult = CRM.Core.Ports.Input.SchedulingEventResult;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Stub implementation of ISchedulingIntegrationService for
/// Calendly / Microsoft Bookings integration.
/// Implements TODO-INT-11.
///
/// Manages scheduling links, meeting bookings, and incoming
/// webhook events from external scheduling providers.
/// </summary>
public class SchedulingIntegrationService : ISchedulingIntegrationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SchedulingIntegrationService> _logger;

    // In-memory link store for development/testing
    private static readonly Dictionary<string, SchedulingLink> _links = new(StringComparer.OrdinalIgnoreCase);

    public SchedulingIntegrationService(
        IConfiguration configuration,
        ILogger<SchedulingIntegrationService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<SchedulingLink> CreateSchedulingLinkAsync(
        CreateSchedulingLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var provider = GetProvider();
        _logger.LogInformation(
            "Create scheduling link '{Name}' for user {CrmUserId} via {Provider} (stub)",
            request.Name, request.CrmUserId, provider);

        var link = new SchedulingLink
        {
            Id = Guid.NewGuid().ToString("N"),
            Url = $"https://{provider.ToLowerInvariant()}.example.com/schedule/{Guid.NewGuid():N}",
            Name = request.Name.Length > 0 ? request.Name : "CRM Meeting",
            DurationMinutes = request.DurationMinutes > 0 ? request.DurationMinutes : 30,
            CrmUserId = request.CrmUserId,
            Provider = provider,
            IsActive = true
        };

        _links[link.Id] = link;
        return Task.FromResult(link);
    }

    /// <inheritdoc />
    public Task<SchedulingLink?> GetSchedulingLinkAsync(
        string linkId,
        CancellationToken cancellationToken = default)
    {
        _links.TryGetValue(linkId, out var link);
        return Task.FromResult(link);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ScheduledMeeting>> GetUpcomingMeetingsAsync(
        string entityType,
        int entityId,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Get upcoming meetings for {EntityType}#{EntityId} (stub)",
            entityType, entityId);

        // Return empty until provider is connected
        return Task.FromResult<IReadOnlyCollection<ScheduledMeeting>>(
            Array.Empty<ScheduledMeeting>());
    }

    /// <inheritdoc />
    public Task<SchedulingEventResult> HandleWebhookEventAsync(
        SchedulingWebhookEvent webhookEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);

        _logger.LogInformation(
            "Handle scheduling webhook: type={EventType}, meetingId={MeetingId}",
            webhookEvent.EventType, webhookEvent.MeetingId);

        // In production this would:
        //  1. Validate the webhook signature
        //  2. Parse event (created / canceled / rescheduled)
        //  3. Create or update Interaction / Activity in CRM
        //  4. Send notification to assigned user

        return Task.FromResult(new SchedulingEventResult
        {
            Success = true,
            ErrorMessage = null
        });
    }

    /// <inheritdoc />
    public Task<bool> CancelMeetingAsync(
        string externalMeetingId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Cancel meeting {ExternalMeetingId}, reason={Reason} (stub)",
            externalMeetingId, reason ?? "(none)");

        // Stub: always report success
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        var apiKey = _configuration[$"Integrations:Scheduling:{provider}:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("{Provider} API key not configured at Integrations:Scheduling:{Provider}:ApiKey",
                provider, provider);
            return Task.FromResult(false);
        }

        _logger.LogInformation("Scheduling ({Provider}) test connection (stub)", provider);
        return Task.FromResult(false);
    }

    private string GetProvider()
    {
        return _configuration["Integrations:Scheduling:Provider"] ?? "Calendly";
    }

    public Task<IReadOnlyList<SchedulingLink>> GetSchedulingLinksAsync(int? userId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("GetSchedulingLinksAsync not implemented - requires scheduling platform API");
        return Task.FromResult<IReadOnlyList<SchedulingLink>>(new List<SchedulingLink>());
    }

    public Task<IReadOnlyList<ScheduledMeeting>> GetUpcomingMeetingsAsync(int? userId = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("GetUpcomingMeetingsAsync not implemented - requires scheduling platform API");
        return Task.FromResult<IReadOnlyList<ScheduledMeeting>>(new List<ScheduledMeeting>());
    }

    public Task LinkMeetingToEntityAsync(string meetingId, string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("LinkMeetingToEntityAsync not implemented - requires scheduling platform API");
        return Task.CompletedTask;
    }

    public Task<SchedulingEventResult> ProcessWebhookEventAsync(SchedulingWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("ProcessWebhookEventAsync not implemented - requires scheduling platform API");
        return Task.FromResult(SchedulingEventResult.Failed("Not implemented"));
    }

}
