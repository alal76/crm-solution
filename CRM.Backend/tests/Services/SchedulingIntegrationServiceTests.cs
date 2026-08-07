// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Ports;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Services.Integrations;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SchedulingIntegrationService (REV-STUB-005) against Calendly's REST API.
/// All HTTP calls are mocked via a fake handler behind IHttpClientFactory — no real network
/// calls are made anywhere in this file.
///
/// MANDATORY: Written after verifying source for:
///   Class: SchedulingIntegrationService, Namespace: CRM.Infrastructure.Services.Integrations
///   Constructor: (IProviderConfigurationService, IHttpClientFactory, ILogger)
///   Every real call first resolves the user URI via GET /users/me, then calls the target endpoint.
/// </summary>
public class SchedulingIntegrationServiceTests
{
    private static Mock<IProviderConfigurationService> ConfigServiceMock(string? token)
    {
        var mock = new Mock<IProviderConfigurationService>();
        mock.Setup(m => m.GetConfigurationAsync("crm.scheduling.calendly", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token == null
                ? null
                : new ProviderConfigurationDto
                {
                    Id = 1,
                    ConfigurationKey = "crm.scheduling.calendly",
                    ConfigurationType = "crm",
                    ConfigurationData = JsonSerializer.Serialize(new { ApiKey = token }),
                    IsEncrypted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
        return mock;
    }

    private static SchedulingIntegrationService BuildService(
        Mock<IProviderConfigurationService> configService,
        QueueHttpMessageHandler? handler = null)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler ?? new QueueHttpMessageHandler()));

        return new SchedulingIntegrationService(
            configService.Object,
            factory.Object,
            Mock.Of<ILogger<SchedulingIntegrationService>>());
    }

    private const string UsersMeResponse = """{ "resource": { "uri": "https://api.calendly.com/users/ABC" } }""";

    // ── GetSchedulingLinksAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetSchedulingLinksAsync_ReturnsLinks_WhenCalendlyReturns200()
    {
        var configService = ConfigServiceMock("valid-pat");
        var handler = new QueueHttpMessageHandler();
        handler.Enqueue(HttpStatusCode.OK, UsersMeResponse);
        handler.Enqueue(HttpStatusCode.OK, """
        {
          "collection": [
            { "uri": "https://api.calendly.com/event_types/1", "name": "30 Min Meeting", "scheduling_url": "https://calendly.com/x/30min", "duration": 30, "active": true }
          ]
        }
        """);

        var svc = BuildService(configService, handler);

        var links = await svc.GetSchedulingLinksAsync();

        links.Should().ContainSingle();
        links[0].Name.Should().Be("30 Min Meeting");
        links[0].DurationMinutes.Should().Be(30);
        links[0].Provider.Should().Be("Calendly");
    }

    [Fact]
    public async Task GetSchedulingLinksAsync_ReturnsEmpty_WhenTokenNotConfigured()
    {
        var configService = ConfigServiceMock(token: null);
        var svc = BuildService(configService);

        var links = await svc.GetSchedulingLinksAsync();

        links.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSchedulingLinksAsync_ReturnsEmpty_WhenCalendlyReturnsError()
    {
        var configService = ConfigServiceMock("valid-pat");
        var handler = new QueueHttpMessageHandler();
        handler.Enqueue(HttpStatusCode.Unauthorized, """{"message":"Invalid token"}""");

        var svc = BuildService(configService, handler);

        var links = await svc.GetSchedulingLinksAsync();

        links.Should().BeEmpty();
    }

    // ── GetUpcomingMeetingsAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetUpcomingMeetingsAsync_ReturnsMeetings_WhenCalendlyReturns200()
    {
        var configService = ConfigServiceMock("valid-pat");
        var handler = new QueueHttpMessageHandler();
        handler.Enqueue(HttpStatusCode.OK, UsersMeResponse);
        handler.Enqueue(HttpStatusCode.OK, """
        {
          "collection": [
            {
              "uri": "https://api.calendly.com/scheduled_events/xyz",
              "name": "Intro Call",
              "status": "active",
              "start_time": "2026-09-01T15:00:00.000000Z",
              "end_time": "2026-09-01T15:30:00.000000Z",
              "location": { "join_url": "https://zoom.us/j/123" }
            }
          ]
        }
        """);

        var svc = BuildService(configService, handler);

        var meetings = await svc.GetUpcomingMeetingsAsync();

        meetings.Should().ContainSingle();
        meetings[0].Title.Should().Be("Intro Call");
        meetings[0].MeetingUrl.Should().Be("https://zoom.us/j/123");
    }

    // ── ProcessWebhookEventAsync (pure logic, no HTTP) ─────────────────────

    [Fact]
    public async Task ProcessWebhookEventAsync_ReturnsSuccess_ForValidEvent()
    {
        var svc = BuildService(ConfigServiceMock("valid-pat"));

        var result = await svc.ProcessWebhookEventAsync(new SchedulingWebhookEvent
        {
            EventType = "invitee.created",
            MeetingId = "meeting-1",
            InviteeEmail = "jane@example.com"
        });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookEventAsync_ReturnsFailure_WhenMeetingIdMissing()
    {
        var svc = BuildService(ConfigServiceMock("valid-pat"));

        var result = await svc.ProcessWebhookEventAsync(new SchedulingWebhookEvent
        {
            EventType = "invitee.created",
            MeetingId = string.Empty
        });

        result.Success.Should().BeFalse();
    }

    // ── TestConnectionAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task TestConnectionAsync_ReturnsTrue_WhenCalendlyReturns200()
    {
        var configService = ConfigServiceMock("valid-pat");
        var handler = new QueueHttpMessageHandler();
        handler.Enqueue(HttpStatusCode.OK, UsersMeResponse);

        var svc = BuildService(configService, handler);

        var connected = await svc.TestConnectionAsync();

        connected.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnectionAsync_ReturnsFalse_WhenTokenMissing()
    {
        var svc = BuildService(ConfigServiceMock(token: null));

        var connected = await svc.TestConnectionAsync();

        connected.Should().BeFalse();
    }

    // ── Test HTTP handler (FIFO queue of canned responses) ─────────────────

    private sealed class QueueHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<(HttpStatusCode Status, string Body)> _responses = new();

        public void Enqueue(HttpStatusCode status, string body) => _responses.Enqueue((status, body));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var (status, body) = _responses.Count > 0
                ? _responses.Dequeue()
                : (HttpStatusCode.OK, "{}");

            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
