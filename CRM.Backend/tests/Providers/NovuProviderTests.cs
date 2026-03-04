// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Text;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Novu;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for NovuProvider.
/// Verifies multi-channel notification delivery (email, SMS, push, in-app) via the
/// Novu HTTP API, subscriber management, health-checks, and error-path handling.
///
/// MANDATORY: Written after verifying source signature:
/// Class: NovuProvider, Namespace: CRM.Infrastructure.Providers.Novu
/// Constructor: (IOptions&lt;NovuConfiguration&gt;, HttpClient, ILogger&lt;NovuProvider&gt;)
/// _isConfigured is computed from config.IsValid() which requires ApiKey + Url.
/// </summary>
public class NovuProviderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static NovuConfiguration ValidConfig() => new()
    {
        ApiKey = "novu-test-api-key-123",
        Url = "https://api.novu.co",
        ApplicationId = "app-id-001",
        TimeoutSeconds = 30
    };

    private static NovuConfiguration EmptyConfig() => new()
    {
        ApiKey = "",
        Url = ""
    };

    private static (NovuProvider provider, NovuMockHandler handler) CreateProvider(
        NovuConfiguration? config = null,
        NovuMockHandler? customHandler = null)
    {
        var effectiveConfig = config ?? ValidConfig();
        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<NovuProvider>>();

        var handler = customHandler ?? new NovuMockHandler();
        var httpClient = new HttpClient(handler);

        // The provider uses relative URIs (e.g. "v1/events/trigger"), so BaseAddress is required.
        // In production this is wired by IHttpClientFactory; in tests we set it manually.
        if (!string.IsNullOrEmpty(effectiveConfig.Url))
        {
            var baseUrl = effectiveConfig.Url.TrimEnd('/') + "/";
            httpClient.BaseAddress = new Uri(baseUrl);
        }

        var provider = new NovuProvider(options, httpClient, logger.Object);
        return (provider, handler);
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsNovu()
    {
        var (provider, _) = CreateProvider();
        provider.ProviderName.Should().Be("Novu");
    }

    [Fact]
    public void SupportedChannels_ContainsEmailAndSms()
    {
        var (provider, _) = CreateProvider();
        provider.SupportedChannels.Should().Contain("email");
        provider.SupportedChannels.Should().Contain("sms");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenNotConfigured()
    {
        var (provider, _) = CreateProvider(EmptyConfig());

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenApiReturns200()
    {
        var handler = new NovuMockHandler();
        handler.SetSubscriberListResponse(HttpStatusCode.OK,
            """{"data": [], "page": 0, "pageSize": 1}""");

        var (provider, _) = CreateProvider(customHandler: handler);

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenApiReturnsUnauthorized()
    {
        var handler = new NovuMockHandler();
        handler.SetSubscriberListResponse(HttpStatusCode.Unauthorized, """{"message":"Unauthorized"}""");

        var (provider, _) = CreateProvider(customHandler: handler);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenConnectionThrows()
    {
        var throwingHandler = new NovuThrowingHandler();
        var config = ValidConfig();
        var options = Options.Create(config);
        var logger = new Mock<ILogger<NovuProvider>>();
        var httpClient = new HttpClient(throwingHandler)
        {
            BaseAddress = new Uri(config.Url.TrimEnd('/') + "/")
        };
        var provider = new NovuProvider(options, httpClient, logger.Object);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    // ── SendEmailAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task SendEmailAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var (provider, _) = CreateProvider();

        var act = async () => await provider.SendEmailAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsFailedResult_WhenNotConfigured()
    {
        var (provider, _) = CreateProvider(EmptyConfig());

        var request = new EmailNotificationRequest
        {
            To = "user@example.com",
            Subject = "Hello",
            Body = "World"
        };

        var result = await provider.SendEmailAsync(request);

        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsSuccessResult_WhenNovuAcknowledgesTrigger()
    {
        var handler = new NovuMockHandler();
        // Subscriber check → 404 → create → 201 → trigger → 201
        handler.SetSubscriberResponse(HttpStatusCode.NotFound, "{}");
        handler.SetCreateSubscriberResponse(HttpStatusCode.Created,
            """{"data":{"subscriberId":"sub-001"}}""");
        handler.SetTriggerResponse(HttpStatusCode.Created,
            """{"data":{"acknowledged":true,"transactionId":"tx-abc-001"}}""");

        var (provider, _) = CreateProvider(customHandler: handler);

        var request = new EmailNotificationRequest
        {
            To = "user@example.com",
            Subject = "Test Subject",
            Body = "<p>Test Body</p>"
        };

        var result = await provider.SendEmailAsync(request);

        result.Success.Should().BeTrue();
        result.Channel.Should().Be("email");
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsFailedResult_WhenApiReturnsError()
    {
        var handler = new NovuMockHandler();
        handler.SetSubscriberResponse(HttpStatusCode.OK,
            """{"data":{"subscriberId":"sub-999"}}""");
        handler.SetTriggerResponse(HttpStatusCode.InternalServerError,
            """{"message":"Internal Server Error"}""");

        var (provider, _) = CreateProvider(customHandler: handler);

        var request = new EmailNotificationRequest
        {
            To = "user@example.com",
            Subject = "Test Subject",
            Body = "Body"
        };
        var result = await provider.SendEmailAsync(request);

        result.Success.Should().BeFalse();
    }

    // ── SendSmsAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SendSmsAsync_ReturnsFailedResult_WhenNotConfigured()
    {
        var (provider, _) = CreateProvider(EmptyConfig());

        var result = await provider.SendSmsAsync(new SmsNotificationRequest
        {
            To = "+15555550100",
            Message = "Hello"
        });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SendSmsAsync_ThrowsArgumentException_WhenPhoneNumberIsEmpty()
    {
        var (provider, _) = CreateProvider();

        var act = async () => await provider.SendSmsAsync(new SmsNotificationRequest
        {
            To = "",
            Message = "Hello"
        });

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── SendPushAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SendPushAsync_ReturnsFailedResult_WhenNotConfigured()
    {
        var (provider, _) = CreateProvider(EmptyConfig());

        var result = await provider.SendPushAsync(new PushNotificationRequest
        {
            To = "device-token-abc",
            Title = "Alert",
            Body = "You have a message"
        });

        result.Success.Should().BeFalse();
    }

    // ── SendInAppAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task SendInAppAsync_ThrowsArgumentException_WhenUserIdIsEmpty()
    {
        var (provider, _) = CreateProvider();

        var act = async () => await provider.SendInAppAsync(new InAppNotificationRequest
        {
            UserId = "",
            Title = "Alert",
            Content = "New lead assigned"
        });

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── NovuConfiguration.Validate ────────────────────────────────────────────

    [Fact]
    public void NovuConfiguration_IsValid_ReturnsFalse_WhenApiKeyIsEmpty()
    {
        var config = new NovuConfiguration { ApiKey = "", Url = "https://api.novu.co" };
        config.IsValid().Should().BeFalse();
    }

    [Fact]
    public void NovuConfiguration_IsValid_ReturnsFalse_WhenUrlIsEmpty()
    {
        var config = new NovuConfiguration { ApiKey = "some-key", Url = "" };
        config.IsValid().Should().BeFalse();
    }

    [Fact]
    public void NovuConfiguration_IsValid_ReturnsTrue_WhenBothApiKeyAndUrlAreSet()
    {
        var config = ValidConfig();
        config.IsValid().Should().BeTrue();
    }
}

// ── Private handler helpers for Novu tests ────────────────────────────────────

/// <summary>
/// Stateful mock HTTP handler that routes Novu API requests by URL pattern,
/// allowing per-endpoint response configuration.
/// </summary>
internal class NovuMockHandler : HttpMessageHandler
{
    private HttpStatusCode _subscriberListStatus = HttpStatusCode.OK;
    private string _subscriberListBody = """{"data":[],"page":0,"pageSize":1}""";

    private HttpStatusCode _subscriberStatus = HttpStatusCode.OK;
    private string _subscriberBody = """{"data":{"subscriberId":"sub-default"}}""";

    private HttpStatusCode _createSubscriberStatus = HttpStatusCode.Created;
    private string _createSubscriberBody = """{"data":{"subscriberId":"sub-created"}}""";

    private HttpStatusCode _triggerStatus = HttpStatusCode.Created;
    private string _triggerBody = """{"data":{"acknowledged":true,"transactionId":"tx-001"}}""";

    public void SetSubscriberListResponse(HttpStatusCode status, string body)
    {
        _subscriberListStatus = status;
        _subscriberListBody = body;
    }

    public void SetSubscriberResponse(HttpStatusCode status, string body)
    {
        _subscriberStatus = status;
        _subscriberBody = body;
    }

    public void SetCreateSubscriberResponse(HttpStatusCode status, string body)
    {
        _createSubscriberStatus = status;
        _createSubscriberBody = body;
    }

    public void SetTriggerResponse(HttpStatusCode status, string body)
    {
        _triggerStatus = status;
        _triggerBody = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? string.Empty;
        var method = request.Method;

        HttpStatusCode status;
        string body;

        if (url.Contains("/events/trigger"))
        {
            status = _triggerStatus;
            body = _triggerBody;
        }
        else if (url.Contains("/subscribers") && method == HttpMethod.Post)
        {
            status = _createSubscriberStatus;
            body = _createSubscriberBody;
        }
        else if (url.Contains("/subscribers") && (url.Contains("page=") || url.Contains("limit=")))
        {
            status = _subscriberListStatus;
            body = _subscriberListBody;
        }
        else if (url.Contains("/subscribers/"))
        {
            status = _subscriberStatus;
            body = _subscriberBody;
        }
        else
        {
            status = HttpStatusCode.OK;
            body = "{}";
        }

        return Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });
    }
}

internal class NovuThrowingHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        throw new HttpRequestException("Network error (test)");
    }
}
