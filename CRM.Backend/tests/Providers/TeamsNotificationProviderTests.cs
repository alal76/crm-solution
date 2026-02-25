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
using CRM.Infrastructure.Providers.Teams;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for TeamsNotificationProvider.
/// Tests Adaptive Card formatting, webhook delivery, and not-configured/not-supported paths.
/// MANDATORY: All tests written after reading the actual source file.
/// Class: TeamsNotificationProvider, Namespace: CRM.Infrastructure.Providers.Teams
/// Constructor: (IOptions&lt;TeamsConfiguration&gt;, HttpClient, ILogger&lt;TeamsNotificationProvider&gt;)
/// </summary>
public class TeamsNotificationProviderTests
{
    private readonly Mock<ILogger<TeamsNotificationProvider>> _logger = new();

    /// <summary>
    /// Creates a TeamsNotificationProvider backed by a fake HttpMessageHandler
    /// that captures outbound requests and returns the supplied status/body.
    /// </summary>
    private static (TeamsNotificationProvider provider, List<HttpRequestMessage> capturedRequests)
        CreateProvider(
            TeamsConfiguration? config = null,
            HttpStatusCode responseStatus = HttpStatusCode.OK,
            string responseBody = "1")
    {
        var capturedRequests = new List<HttpRequestMessage>();

        var handlerMock = new MockHttpMessageHandler(capturedRequests, responseStatus, responseBody);
        var httpClient = new HttpClient(handlerMock);

        var effectiveConfig = config ?? new TeamsConfiguration
        {
            WebhookUrl = "https://example.webhook.office.com/test",
            ThemeColor = "0078D4",
            IncludeTimestamp = false
        };

        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<TeamsNotificationProvider>>();
        var provider = new TeamsNotificationProvider(options, httpClient, logger.Object);

        return (provider, capturedRequests);
    }

    // ── Constructor ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
    {
        var act = () => new TeamsNotificationProvider(
            null!,
            new HttpClient(),
            new Mock<ILogger<TeamsNotificationProvider>>().Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHttpClientIsNull()
    {
        var options = Options.Create(new TeamsConfiguration { WebhookUrl = "https://example.com" });

        var act = () => new TeamsNotificationProvider(
            options,
            null!,
            new Mock<ILogger<TeamsNotificationProvider>>().Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsTeams()
    {
        var (provider, _) = CreateProvider();
        provider.ProviderName.Should().Be("Teams");
    }

    [Fact]
    public void SupportedChannels_ContainsTeamsEmailAndInApp()
    {
        var (provider, _) = CreateProvider();
        provider.SupportedChannels.Should().Contain(new[] { "teams", "email", "in_app" });
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenWebhookUrlIsEmpty()
    {
        var config = new TeamsConfiguration { WebhookUrl = string.Empty };
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
        var config = new TeamsConfiguration { WebhookUrl = string.Empty };
        var (provider, _) = CreateProvider(config);

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@crm.local",
            Subject = "Test",
            Body = "Hello"
        });

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_CONFIGURED");
        result.Provider.Should().Be("Teams");
    }

    [Fact]
    public async Task SendEmailAsync_PostsToWebhookUrl_WhenConfigured()
    {
        var (provider, capturedRequests) = CreateProvider(responseStatus: HttpStatusCode.OK, responseBody: "1");

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "admin@crm.local",
            Subject = "Welcome",
            Body = "<b>Hello</b> World"
        });

        result.Success.Should().BeTrue();
        result.Provider.Should().Be("Teams");
        result.Channel.Should().Be("email");
        result.MessageId.Should().StartWith("teams_");
        capturedRequests.Should().HaveCount(1);
        capturedRequests[0].RequestUri!.ToString().Should().Be("https://example.webhook.office.com/test");
        capturedRequests[0].Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsFailure_WhenWebhookReturnsNonSuccess()
    {
        var (provider, _) = CreateProvider(
            responseStatus: HttpStatusCode.BadRequest,
            responseBody: "invalid_payload");

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@crm.local",
            Subject = "Test",
            Body = "Test body"
        });

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("400");
        result.Error.Should().Contain("400");
    }

    [Fact]
    public async Task SendEmailAsync_StripsHtmlFromBody_BeforeSendingToTeams()
    {
        string? capturedJson = null;
        var handlerMock = new CapturingHttpHandler(req =>
        {
            capturedJson = req.Content?.ReadAsStringAsync().Result;
        });

        var config = new TeamsConfiguration
        {
            WebhookUrl = "https://example.webhook.office.com/test",
            IncludeTimestamp = false
        };
        var options = Options.Create(config);
        var logger = new Mock<ILogger<TeamsNotificationProvider>>();
        var provider = new TeamsNotificationProvider(options, new HttpClient(handlerMock), logger.Object);

        await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@crm.local",
            Subject = "Test",
            Body = "<p>Hello <strong>World</strong></p>"
        });

        capturedJson.Should().NotBeNull();
        capturedJson.Should().NotContain("<p>");
        capturedJson.Should().NotContain("<strong>");
        capturedJson.Should().Contain("Hello World");
    }

    // ── SendSmsAsync / SendPushAsync — Not Supported ─────────────────────────

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
        var result = await provider.SendPushAsync(new PushNotificationRequest { To = "token", Title = "Test", Body = "Test" });

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_SUPPORTED");
    }

    // ── SendNotificationAsync — Multi-Channel ──────────────────────────────

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
            SubscriberId = "user-42",
            Channels = new List<string> { "teams" },
            TemplateId = "crm-alert",
            Payload = new Dictionary<string, object> { ["ticketId"] = 99 }
        });

        result.Success.Should().BeTrue();
        result.ChannelResults.Should().ContainKey("teams");
        result.ChannelResults["teams"].Success.Should().BeTrue();
        capturedRequests.Should().HaveCount(1);
    }

    [Fact]
    public async Task SendNotificationAsync_ReturnsNotConfigured_WhenWebhookUrlIsEmpty()
    {
        var config = new TeamsConfiguration { WebhookUrl = string.Empty };
        var (provider, _) = CreateProvider(config);

        var result = await provider.SendNotificationAsync(new MultiChannelNotificationRequest
        {
            SubscriberId = "user-1",
            Channels = new List<string> { "teams" }
        });

        result.Success.Should().BeFalse();
        result.ChannelResults["teams"].ErrorCode.Should().Be("NOT_CONFIGURED");
    }

    // ── Bulk Operations ───────────────────────────────────────────────────────

    [Fact]
    public async Task SendBulkEmailAsync_SendsEachEmailAsSeparatePost()
    {
        var (provider, capturedRequests) = CreateProvider();

        var requests = new[]
        {
            new EmailNotificationRequest { To = "a@crm.local", Subject = "A", Body = "Body A" },
            new EmailNotificationRequest { To = "b@crm.local", Subject = "B", Body = "Body B" },
            new EmailNotificationRequest { To = "c@crm.local", Subject = "C", Body = "Body C" }
        };

        var result = await provider.SendBulkEmailAsync(requests);

        result.TotalCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);
        capturedRequests.Should().HaveCount(3);
    }

    // ── HealthCheckAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_ReturnsUnhealthy_WhenNotConfigured()
    {
        var config = new TeamsConfiguration { WebhookUrl = string.Empty };
        var (provider, _) = CreateProvider(config);

        var health = await provider.HealthCheckAsync();

        health.IsHealthy.Should().BeFalse();
        health.ProviderName.Should().Be("Teams");
        health.Message.Should().Contain("not configured");
    }

    // ── Subscriber Management Stubs ────────────────────────────────────────

    [Fact]
    public async Task UpsertSubscriberAsync_ReturnsNonEmptyId()
    {
        var (provider, _) = CreateProvider();
        var id = await provider.UpsertSubscriberAsync(new SubscriberRequest { SubscriberId = "user-1" });
        id.Should().StartWith("teams_");
    }

    [Fact]
    public async Task GetSubscriberPreferencesAsync_ReturnsNull()
    {
        var (provider, _) = CreateProvider();
        var prefs = await provider.GetSubscriberPreferencesAsync("user-1");
        prefs.Should().BeNull();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Test helpers — isolated in this file
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A fake HttpMessageHandler that records requests and returns a canned response.
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly List<HttpRequestMessage> _captured;
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public MockHttpMessageHandler(
        List<HttpRequestMessage> captured,
        HttpStatusCode status = HttpStatusCode.OK,
        string body = "1")
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
/// A fake HttpMessageHandler that invokes a callback with the request for content inspection.
/// </summary>
internal class CapturingHttpHandler : HttpMessageHandler
{
    private readonly Action<HttpRequestMessage> _callback;

    public CapturingHttpHandler(Action<HttpRequestMessage> callback) => _callback = callback;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _callback(request);
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("1", Encoding.UTF8, "application/json")
        });
    }
}
