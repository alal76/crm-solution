// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Net.Http;
using System.Text;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Slack;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for SlackNotificationProvider.
/// Tests Block Kit message formatting, webhook delivery, and not-configured/not-supported paths.
/// MANDATORY: Written after verifying source signature:
/// Class: SlackNotificationProvider, Namespace: CRM.Infrastructure.Providers.Slack
/// Constructor: (IOptions&lt;SlackConfiguration&gt;, HttpClient, ILogger&lt;SlackNotificationProvider&gt;)
/// </summary>
public class SlackNotificationProviderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (SlackNotificationProvider provider, List<HttpRequestMessage> capturedRequests)
        CreateProvider(
            SlackConfiguration? config = null,
            HttpStatusCode responseStatus = HttpStatusCode.OK,
            string responseBody = "ok")
    {
        var capturedRequests = new List<HttpRequestMessage>();
        var handlerMock = new SlackMockHandler(capturedRequests, responseStatus, responseBody);
        var httpClient = new HttpClient(handlerMock);

        var effectiveConfig = config ?? new SlackConfiguration
        {
            WebhookUrl = "https://hooks.slack.com/services/T000/B000/xxxx",
            Username = "CRM Test Bot",
            IconEmoji = ":bell:"
        };

        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<SlackNotificationProvider>>();
        var provider = new SlackNotificationProvider(options, httpClient, logger.Object);

        return (provider, capturedRequests);
    }

    // ── Constructor Guards ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
    {
        var act = () => new SlackNotificationProvider(
            null!,
            new HttpClient(),
            new Mock<ILogger<SlackNotificationProvider>>().Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHttpClientIsNull()
    {
        var options = Options.Create(new SlackConfiguration { WebhookUrl = "https://hooks.slack.com/test" });

        var act = () => new SlackNotificationProvider(
            options,
            null!,
            new Mock<ILogger<SlackNotificationProvider>>().Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsSlack()
    {
        var (provider, _) = CreateProvider();
        provider.ProviderName.Should().Be("Slack");
    }

    [Fact]
    public void SupportedChannels_ContainsSlackEmailAndInApp()
    {
        var (provider, _) = CreateProvider();
        provider.SupportedChannels.Should().Contain(new[] { "slack", "email", "in_app" });
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenWebhookUrlIsEmpty()
    {
        var config = new SlackConfiguration { WebhookUrl = string.Empty };
        var (provider, _) = CreateProvider(config);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenWebhookUrlIsSet()
    {
        var (provider, _) = CreateProvider();
        var result = await provider.IsAvailableAsync();
        result.Should().BeTrue();
    }

    // ── SendEmailAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task SendEmailAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var (provider, _) = CreateProvider();
        var act = async () => await provider.SendEmailAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsNotConfigured_WhenWebhookUrlIsEmpty()
    {
        var config = new SlackConfiguration { WebhookUrl = string.Empty };
        var (provider, _) = CreateProvider(config);

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@crm.local",
            Subject = "Test",
            Body = "Hello"
        });

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_CONFIGURED");
        result.Provider.Should().Be("Slack");
    }

    [Fact]
    public async Task SendEmailAsync_PostsToWebhookUrl_WhenConfigured()
    {
        var (provider, capturedRequests) = CreateProvider();

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "admin@crm.local",
            Subject = "Ticket Created",
            Body = "<p>A new ticket was opened.</p>"
        });

        result.Success.Should().BeTrue();
        result.Provider.Should().Be("Slack");
        result.Channel.Should().Be("email");
        result.MessageId.Should().StartWith("slack_");
        capturedRequests.Should().HaveCount(1);
        capturedRequests[0].RequestUri!.ToString().Should().Be("https://hooks.slack.com/services/T000/B000/xxxx");
        capturedRequests[0].Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsFailure_WhenWebhookReturnsBadRequest()
    {
        var (provider, _) = CreateProvider(
            responseStatus: HttpStatusCode.BadRequest,
            responseBody: "no_service");

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@crm.local",
            Subject = "Test",
            Body = "Test body"
        });

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("400");
    }

    [Fact]
    public async Task SendEmailAsync_StripsHtmlFromBody_InPayload()
    {
        string? capturedJson = null;
        var handler = new SlackCapturingHandler(req =>
        {
            capturedJson = req.Content?.ReadAsStringAsync().Result;
        });

        var config = new SlackConfiguration
        {
            WebhookUrl = "https://hooks.slack.com/services/T000/B000/xxxx",
            Username = "CRM",
            IconEmoji = ":bell:"
        };
        var options = Options.Create(config);
        var logger = new Mock<ILogger<SlackNotificationProvider>>();
        var provider = new SlackNotificationProvider(options, new HttpClient(handler), logger.Object);

        await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@crm.local",
            Subject = "Test",
            Body = "<h1>Alert</h1><p>Details here</p>"
        });

        capturedJson.Should().NotBeNull();
        capturedJson.Should().NotContain("<h1>");
        capturedJson.Should().NotContain("<p>");
        capturedJson.Should().Contain("Alert");
        capturedJson.Should().Contain("Details here");
    }

    [Fact]
    public async Task SendEmailAsync_PayloadContainsBlockKitStructure()
    {
        string? capturedJson = null;
        var handler = new SlackCapturingHandler(req =>
        {
            capturedJson = req.Content?.ReadAsStringAsync().Result;
        });

        var config = new SlackConfiguration
        {
            WebhookUrl = "https://hooks.slack.com/services/T000/B000/xxxx",
            Username = "CRM Bot",
            IconEmoji = ":bell:"
        };
        var options = Options.Create(config);
        var logger = new Mock<ILogger<SlackNotificationProvider>>();
        var provider = new SlackNotificationProvider(options, new HttpClient(handler), logger.Object);

        await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "test@crm.local",
            Subject = "My Subject",
            Body = "My Body"
        });

        capturedJson.Should().NotBeNull();
        capturedJson.Should().Contain("\"blocks\"");
        capturedJson.Should().Contain("\"header\"");
        capturedJson.Should().Contain("My Subject");
        capturedJson.Should().Contain("username");
        capturedJson.Should().Contain("CRM Bot");
    }

    // ── SMS / Push — Not Supported ────────────────────────────────────────

    [Fact]
    public async Task SendSmsAsync_ReturnsNotSupported()
    {
        var (provider, _) = CreateProvider();
        var result = await provider.SendSmsAsync(new SmsNotificationRequest { To = "+15550000000", Message = "Test" });

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_SUPPORTED");
    }

    [Fact]
    public async Task SendPushAsync_ReturnsNotSupported()
    {
        var (provider, _) = CreateProvider();
        var result = await provider.SendPushAsync(new PushNotificationRequest { To = "token", Title = "T", Body = "B" });

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_SUPPORTED");
    }

    // ── SendNotificationAsync — Multi-Channel ─────────────────────────────

    [Fact]
    public async Task SendNotificationAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var (provider, _) = CreateProvider();
        var act = async () => await provider.SendNotificationAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendNotificationAsync_PostsToWebhook_WhenConfigured()
    {
        var (provider, capturedRequests) = CreateProvider();

        var result = await provider.SendNotificationAsync(new MultiChannelNotificationRequest
        {
            SubscriberId = "user-99",
            Channels = new List<string> { "slack" },
            TemplateId = "crm-notify",
            Payload = new Dictionary<string, object> { ["message"] = "Hello from CRM" }
        });

        result.Success.Should().BeTrue();
        result.ChannelResults.Should().ContainKey("slack");
        result.ChannelResults["slack"].Success.Should().BeTrue();
        result.ChannelResults["slack"].MessageId.Should().StartWith("slack_");
        capturedRequests.Should().HaveCount(1);
    }

    [Fact]
    public async Task SendNotificationAsync_ReturnsNotConfigured_WhenWebhookUrlIsEmpty()
    {
        var config = new SlackConfiguration { WebhookUrl = string.Empty };
        var (provider, _) = CreateProvider(config);

        var result = await provider.SendNotificationAsync(new MultiChannelNotificationRequest
        {
            SubscriberId = "user-1",
            Channels = new List<string> { "slack" }
        });

        result.Success.Should().BeFalse();
        result.ChannelResults["slack"].ErrorCode.Should().Be("NOT_CONFIGURED");
    }

    // ── Bulk Operations ───────────────────────────────────────────────────────

    [Fact]
    public async Task SendBulkEmailAsync_SendsEachRequestSeparately()
    {
        var (provider, capturedRequests) = CreateProvider();

        var requests = new[]
        {
            new EmailNotificationRequest { To = "a@crm.local", Subject = "A", Body = "Body A" },
            new EmailNotificationRequest { To = "b@crm.local", Subject = "B", Body = "Body B" }
        };

        var result = await provider.SendBulkEmailAsync(requests);

        result.TotalCount.Should().Be(2);
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(0);
        capturedRequests.Should().HaveCount(2);
    }

    // ── HealthCheckAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_ReturnsUnhealthy_WhenNotConfigured()
    {
        var config = new SlackConfiguration { WebhookUrl = string.Empty };
        var (provider, _) = CreateProvider(config);

        var health = await provider.HealthCheckAsync();

        health.IsHealthy.Should().BeFalse();
        health.ProviderName.Should().Be("Slack");
        health.Message.Should().Contain("not configured");
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthy_WhenWebhookRespondsOk()
    {
        var (provider, _) = CreateProvider(responseStatus: HttpStatusCode.OK, responseBody: "ok");

        var health = await provider.HealthCheckAsync();

        health.IsHealthy.Should().BeTrue();
        health.ProviderName.Should().Be("Slack");
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsUnhealthy_WhenWebhookResponseIsNotOk()
    {
        var (provider, _) = CreateProvider(responseStatus: HttpStatusCode.OK, responseBody: "no_service");

        var health = await provider.HealthCheckAsync();

        health.IsHealthy.Should().BeFalse();
    }

    // ── Subscriber Stubs ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpsertSubscriberAsync_ReturnsNonEmptyId()
    {
        var (provider, _) = CreateProvider();
        var id = await provider.UpsertSubscriberAsync(new SubscriberRequest { SubscriberId = "user-1" });
        id.Should().StartWith("slack_");
    }

    [Fact]
    public async Task GetSubscriberPreferencesAsync_ReturnsNull()
    {
        var (provider, _) = CreateProvider();
        var prefs = await provider.GetSubscriberPreferencesAsync("user-1");
        prefs.Should().BeNull();
    }
}

// ── Private handler helpers for Slack tests ─────────────────────────────────

internal class SlackMockHandler : HttpMessageHandler
{
    private readonly List<HttpRequestMessage> _captured;
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public SlackMockHandler(List<HttpRequestMessage> captured, HttpStatusCode status, string body)
    {
        _captured = captured;
        _status = status;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _captured.Add(request);
        return Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, Encoding.UTF8, "text/plain")
        });
    }
}

internal class SlackCapturingHandler : HttpMessageHandler
{
    private readonly Action<HttpRequestMessage> _callback;

    public SlackCapturingHandler(Action<HttpRequestMessage> callback) => _callback = callback;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _callback(request);
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("ok", Encoding.UTF8, "text/plain")
        });
    }
}
