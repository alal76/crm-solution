// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TODO-SD005-010: Unit tests for the generic Slack/Teams webhook notification services.

using System.Net;
using System.Net.Http;
using System.Text;
using CRM.Core.Interfaces.Notifications;
using CRM.Infrastructure.Providers.Slack;
using CRM.Infrastructure.Providers.Teams;
using CRM.Infrastructure.Services.Notifications;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SlackNotificationService and TeamsNotificationService — real
/// Incoming Webhook delivery with graceful not-configured / failure fallback.
/// MANDATORY: written after reading the actual source files.
/// Class: SlackNotificationService, Namespace: CRM.Infrastructure.Services.Notifications
/// Constructor: (ILogger&lt;SlackNotificationService&gt;, IHttpClientFactory?, IOptions&lt;SlackConfiguration&gt;?)
/// Class: TeamsNotificationService, Namespace: CRM.Infrastructure.Services.Notifications
/// Constructor: (ILogger&lt;TeamsNotificationService&gt;, IHttpClientFactory?, IOptions&lt;TeamsConfiguration&gt;?)
/// </summary>
public class SlackTeamsNotificationServiceTests
{
    // ─────────────────────────────────────────────────────────────────────
    // Test helpers
    // ─────────────────────────────────────────────────────────────────────

    private static IHttpClientFactory CreateFactory(
        string clientName,
        HttpMessageHandler handler)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(clientName)).Returns(new HttpClient(handler));
        return factory.Object;
    }

    private static (SlackNotificationService service, List<HttpRequestMessage> captured) CreateSlackService(
        string? webhookUrl = "https://hooks.slack.com/services/T00/B00/XXX",
        HttpStatusCode responseStatus = HttpStatusCode.OK,
        string responseBody = "ok")
    {
        var captured = new List<HttpRequestMessage>();
        var handler = new RecordingHttpMessageHandler(captured, responseStatus, responseBody);
        var factory = CreateFactory("Slack", handler);
        var options = Options.Create(new SlackConfiguration { WebhookUrl = webhookUrl ?? string.Empty });

        var service = new SlackNotificationService(
            NullLoggerFactory.CreateLogger<SlackNotificationService>(),
            factory,
            options);

        return (service, captured);
    }

    private static (TeamsNotificationService service, List<HttpRequestMessage> captured) CreateTeamsService(
        string? defaultWebhookUrl = null,
        HttpStatusCode responseStatus = HttpStatusCode.OK,
        string responseBody = "1")
    {
        var captured = new List<HttpRequestMessage>();
        var handler = new RecordingHttpMessageHandler(captured, responseStatus, responseBody);
        var factory = CreateFactory("Teams", handler);
        var options = Options.Create(new TeamsConfiguration { WebhookUrl = defaultWebhookUrl ?? string.Empty });

        var service = new TeamsNotificationService(
            NullLoggerFactory.CreateLogger<TeamsNotificationService>(),
            factory,
            options);

        return (service, captured);
    }

    // ─────────────────────────────────────────────────────────────────────
    // SlackNotificationService — real POST when configured
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Slack_SendChannelMessageAsync_PostsToWebhook_WhenConfigured()
    {
        var (service, captured) = CreateSlackService();

        var result = await service.SendChannelMessageAsync("C012345", "Hello Slack");

        result.Should().BeTrue();
        captured.Should().HaveCount(1);
        captured[0].Method.Should().Be(HttpMethod.Post);
        captured[0].RequestUri!.ToString().Should().Be("https://hooks.slack.com/services/T00/B00/XXX");
    }

    [Fact]
    public async Task Slack_IsConfigured_ReturnsTrue_WhenWebhookUrlPresent()
    {
        var (service, _) = CreateSlackService();
        service.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public async Task Slack_SendEscalationAlertAsync_PostsBlockKitPayload_WhenConfigured()
    {
        var (service, captured) = CreateSlackService();

        var result = await service.SendEscalationAlertAsync("C012345", new SlackEscalationInfo
        {
            ServiceRequestNumber = "SR-1001",
            Title = "Prod outage",
            Priority = "P1",
            EscalationLevel = 2,
            SlaBreachTime = DateTime.UtcNow
        });

        result.Should().BeTrue();
        captured.Should().HaveCount(1);
        var body = await captured[0].Content!.ReadAsStringAsync();
        body.Should().Contain("SR-1001");
        body.Should().Contain("blocks");
    }

    [Fact]
    public async Task Slack_PostInteractiveMessageAsync_PostsActionsBlock_WhenConfigured()
    {
        var (service, captured) = CreateSlackService();

        var result = await service.PostInteractiveMessageAsync(
            "C012345",
            "Approve?",
            new[] { new SlackAction { ActionId = "approve", Text = "Approve", Style = "primary" } });

        result.Should().BeTrue();
        captured.Should().HaveCount(1);
    }

    // ─────────────────────────────────────────────────────────────────────
    // SlackNotificationService — not-configured fallback (log-only, no throw)
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Slack_SendChannelMessageAsync_FallsBackToLogOnly_WhenNotConfigured()
    {
        var (service, captured) = CreateSlackService(webhookUrl: string.Empty);

        var result = await service.SendChannelMessageAsync("C012345", "Hello");

        result.Should().BeTrue("not-configured is a legitimate state, not a failure");
        captured.Should().BeEmpty("no HTTP call should be attempted without a configured webhook");
    }

    [Fact]
    public void Slack_IsConfigured_ReturnsFalse_WhenWebhookUrlMissing()
    {
        var (service, _) = CreateSlackService(webhookUrl: string.Empty);
        service.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public async Task Slack_SendEscalationAlertAsync_FallsBackToLogOnly_WhenNotConfigured()
    {
        var (service, captured) = CreateSlackService(webhookUrl: null);

        var result = await service.SendEscalationAlertAsync("C012345", new SlackEscalationInfo
        {
            ServiceRequestNumber = "SR-2002",
            EscalationLevel = 1
        });

        result.Should().BeTrue();
        captured.Should().BeEmpty();
    }

    // ─────────────────────────────────────────────────────────────────────
    // SlackNotificationService — HTTP failure handling (no throw)
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Slack_SendChannelMessageAsync_ReturnsFalse_WhenWebhookReturnsNonSuccess()
    {
        var (service, captured) = CreateSlackService(responseStatus: HttpStatusCode.BadRequest, responseBody: "invalid_payload");

        var result = await service.SendChannelMessageAsync("C012345", "Hello");

        result.Should().BeFalse();
        captured.Should().HaveCount(1, "the POST should still have been attempted");
    }

    [Fact]
    public async Task Slack_SendChannelMessageAsync_ReturnsFalse_WhenHttpClientThrows()
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("Slack")).Returns(new HttpClient(new ThrowingHttpMessageHandler()));
        var options = Options.Create(new SlackConfiguration { WebhookUrl = "https://hooks.slack.com/services/T00/B00/XXX" });

        var service = new SlackNotificationService(
            NullLoggerFactory.CreateLogger<SlackNotificationService>(),
            factory.Object,
            options);

        var act = async () => await service.SendChannelMessageAsync("C012345", "Hello");

        var result = await act.Should().NotThrowAsync();
        result.Subject.Should().BeFalse();
    }

    [Fact]
    public async Task Slack_SendDirectMessageAsync_ReturnsFalse_NotSupportedViaWebhook()
    {
        var (service, captured) = CreateSlackService();

        var result = await service.SendDirectMessageAsync("U012345", "Hi there");

        result.Should().BeFalse("Slack Incoming Webhooks cannot target a specific user without the Web API");
        captured.Should().BeEmpty();
    }

    // ─────────────────────────────────────────────────────────────────────
    // TeamsNotificationService — real POST when webhookUrl supplied
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Teams_SendChannelMessageAsync_PostsToWebhook_WhenUrlSupplied()
    {
        var (service, captured) = CreateTeamsService();

        var result = await service.SendChannelMessageAsync(
            "https://example.webhook.office.com/test",
            "Hello Teams");

        result.Should().BeTrue();
        captured.Should().HaveCount(1);
        captured[0].RequestUri!.ToString().Should().Be("https://example.webhook.office.com/test");
    }

    [Fact]
    public async Task Teams_SendChannelMessageAsync_FallsBackToConfiguredDefault_WhenCallerUrlEmpty()
    {
        var (service, captured) = CreateTeamsService(defaultWebhookUrl: "https://example.webhook.office.com/default");

        var result = await service.SendChannelMessageAsync(string.Empty, "Hello Teams");

        result.Should().BeTrue();
        captured.Should().HaveCount(1);
        captured[0].RequestUri!.ToString().Should().Be("https://example.webhook.office.com/default");
    }

    [Fact]
    public async Task Teams_SendAdaptiveCardAsync_PostsWrappedCard_WhenConfigured()
    {
        var (service, captured) = CreateTeamsService();

        var result = await service.SendAdaptiveCardAsync(
            "https://example.webhook.office.com/test",
            "{\"type\":\"AdaptiveCard\",\"version\":\"1.4\"}");

        result.Should().BeTrue();
        captured.Should().HaveCount(1);
        var body = await captured[0].Content!.ReadAsStringAsync();
        body.Should().Contain("application/vnd.microsoft.card.adaptive");
    }

    [Fact]
    public async Task Teams_SendEscalationAlertAsync_PostsAdaptiveCard_WhenConfigured()
    {
        var (service, captured) = CreateTeamsService();

        var result = await service.SendEscalationAlertAsync(
            "https://example.webhook.office.com/test",
            new TeamsEscalationInfo
            {
                ServiceRequestNumber = "SR-3003",
                Title = "DB latency",
                Priority = "P2",
                EscalationLevel = 1,
                SlaBreachTime = DateTime.UtcNow
            });

        result.Should().BeTrue();
        captured.Should().HaveCount(1);
        var body = await captured[0].Content!.ReadAsStringAsync();
        body.Should().Contain("SR-3003");
    }

    [Fact]
    public void Teams_CreateEscalationCard_ReturnsAdaptiveCardJson()
    {
        var (service, _) = CreateTeamsService();

        var json = service.CreateEscalationCard(new TeamsEscalationInfo
        {
            ServiceRequestNumber = "SR-4004",
            Title = "Test",
            Priority = "P3",
            EscalationLevel = 1,
            SlaBreachTime = DateTime.UtcNow
        });

        json.Should().Contain("AdaptiveCard");
        json.Should().Contain("SR-4004");
    }

    // ─────────────────────────────────────────────────────────────────────
    // TeamsNotificationService — not-configured fallback (log-only, no throw)
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Teams_SendChannelMessageAsync_FallsBackToLogOnly_WhenNoUrlAnywhere()
    {
        var (service, captured) = CreateTeamsService(defaultWebhookUrl: string.Empty);

        var result = await service.SendChannelMessageAsync(string.Empty, "Hello");

        result.Should().BeTrue("not-configured is a legitimate state, not a failure");
        captured.Should().BeEmpty();
    }

    [Fact]
    public async Task Teams_SendAdaptiveCardAsync_FallsBackToLogOnly_WhenNoUrlAnywhere()
    {
        var (service, captured) = CreateTeamsService(defaultWebhookUrl: string.Empty);

        var result = await service.SendAdaptiveCardAsync(string.Empty, "{}");

        result.Should().BeTrue();
        captured.Should().BeEmpty();
    }

    // ─────────────────────────────────────────────────────────────────────
    // TeamsNotificationService — HTTP failure handling (no throw)
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Teams_SendChannelMessageAsync_ReturnsFalse_WhenWebhookReturnsNonSuccess()
    {
        var (service, captured) = CreateTeamsService(responseStatus: HttpStatusCode.ServiceUnavailable, responseBody: "down");

        var result = await service.SendChannelMessageAsync(
            "https://example.webhook.office.com/test",
            "Hello");

        result.Should().BeFalse();
        captured.Should().HaveCount(1);
    }

    [Fact]
    public async Task Teams_SendChannelMessageAsync_ReturnsFalse_WhenHttpClientThrows()
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("Teams")).Returns(new HttpClient(new ThrowingHttpMessageHandler()));
        var options = Options.Create(new TeamsConfiguration());

        var service = new TeamsNotificationService(
            NullLoggerFactory.CreateLogger<TeamsNotificationService>(),
            factory.Object,
            options);

        var act = async () => await service.SendChannelMessageAsync(
            "https://example.webhook.office.com/test",
            "Hello");

        var result = await act.Should().NotThrowAsync();
        result.Subject.Should().BeFalse();
    }

    [Fact]
    public async Task Teams_SendAdaptiveCardAsync_ReturnsFalse_WhenCardJsonInvalid()
    {
        var (service, captured) = CreateTeamsService();

        var result = await service.SendAdaptiveCardAsync(
            "https://example.webhook.office.com/test",
            "{not valid json");

        result.Should().BeFalse();
        captured.Should().BeEmpty("invalid JSON should be rejected before any HTTP call");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Test helpers — isolated in this file
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A fake HttpMessageHandler that records requests and returns a canned response.
/// </summary>
internal class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly List<HttpRequestMessage> _captured;
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public RecordingHttpMessageHandler(
        List<HttpRequestMessage> captured,
        HttpStatusCode status = HttpStatusCode.OK,
        string body = "ok")
    {
        _captured = captured;
        _status = status;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _captured.Add(request);
        return Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json")
        });
    }
}

/// <summary>
/// A fake HttpMessageHandler that always throws, to exercise catch-and-return-false paths.
/// </summary>
internal class ThrowingHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        throw new HttpRequestException("Simulated network failure");
    }
}

/// <summary>
/// Minimal helper to create a NullLogger&lt;T&gt; without pulling in a full DI container.
/// </summary>
internal static class NullLoggerFactory
{
    public static ILogger<T> CreateLogger<T>() => Microsoft.Extensions.Logging.Abstractions.NullLogger<T>.Instance;
}
