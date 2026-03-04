// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.SendGrid;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for SendGridProvider.
/// SendGrid uses the official SendGrid .NET SDK. ISendGridClient is an interface and
/// is injected via the test constructor, making full unit testing without real HTTP possible.
///
/// MANDATORY: Written after verifying source signature:
/// Class: SendGridProvider, Namespace: CRM.Infrastructure.Providers.SendGrid
/// Test constructor: (IOptions&lt;SendGridConfiguration&gt;, ISendGridClient, ILogger&lt;SendGridProvider&gt;)
/// IsAvailableAsync = Task.FromResult(_config.IsValid()) — no HTTP call at all.
/// IsValid() = !string.IsNullOrEmpty(ApiKey) &amp;&amp; !string.IsNullOrEmpty(FromEmail)
/// SMS / Push / InApp are unsupported and immediately return Success=false.
/// </summary>
public class SendGridProviderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static SendGridConfiguration ValidConfig() => new()
    {
        ApiKey = "SG.test-api-key-123456",
        FromEmail = "noreply@crm.example.com",
        FromName = "CRM System",
        TestMode = false,
        SandboxMode = false
    };

    private static Response CreateResponse(HttpStatusCode statusCode, string body = "{}")
    {
        // Note: do NOT use 'using' here – the HttpResponseMessage must outlive this helper
        // so that Response.Body (the StringContent) is not disposed before the provider reads it.
        var httpMsg = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
        };
        return new Response(httpMsg.StatusCode, httpMsg.Content, httpMsg.Headers);
    }

    private static (SendGridProvider provider, Mock<ISendGridClient> mockClient)
        CreateProvider(SendGridConfiguration? config = null)
    {
        var effectiveConfig = config ?? ValidConfig();
        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<SendGridProvider>>();
        var mockClient = new Mock<ISendGridClient>();
        var provider = new SendGridProvider(options, mockClient.Object, logger.Object);
        return (provider, mockClient);
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsSendGrid()
    {
        var (provider, _) = CreateProvider();
        provider.ProviderName.Should().Be("SendGrid");
    }

    [Fact]
    public void SupportedChannels_ContainsEmailOnly()
    {
        var (provider, _) = CreateProvider();
        provider.SupportedChannels.Should().ContainSingle().Which.Should().Be("email");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────
    // SendGrid's IsAvailableAsync does NOT make an HTTP call – it returns IsValid() directly.

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenConfigIsValid()
    {
        var (provider, _) = CreateProvider(ValidConfig());

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenApiKeyIsEmpty()
    {
        var config = ValidConfig();
        config.ApiKey = "";
        var (provider, _) = CreateProvider(config);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenFromEmailIsEmpty()
    {
        var config = ValidConfig();
        config.FromEmail = "";
        var (provider, _) = CreateProvider(config);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    // ── SendEmailAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task SendEmailAsync_ReturnsFailedResult_WhenConfigIsInvalid()
    {
        var config = ValidConfig();
        config.ApiKey = "";
        var (provider, mockClient) = CreateProvider(config);

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@example.com",
            Subject = "Hello",
            Body = "World"
        });

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("invalid");
        // Should NOT call the SDK when config is invalid
        mockClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_ReturnSuccessResult_WhenApiAccepts()
    {
        var (provider, mockClient) = CreateProvider();
        var sgResponse = CreateResponse(HttpStatusCode.Accepted);

        mockClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sgResponse);

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Test Email",
            Body = "<p>Hello</p>",
            IsHtml = true
        });

        result.Success.Should().BeTrue();
        result.Provider.Should().Be("SendGrid");
        result.Channel.Should().Be("email");
        result.MessageId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsFailedResult_WhenApiReturnsBadRequest()
    {
        var (provider, mockClient) = CreateProvider();
        var sgResponse = CreateResponse(HttpStatusCode.BadRequest, """{"errors":[{"message":"Invalid to address"}]}""");

        mockClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sgResponse);

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "bad@",
            Subject = "Fail",
            Body = "Body"
        });

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("BadRequest");
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsSuccessWithoutCallingApi_WhenTestModeEnabled()
    {
        var config = ValidConfig();
        config.TestMode = true;
        var (provider, mockClient) = CreateProvider(config);

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@example.com",
            Subject = "Test",
            Body = "Test body"
        });

        result.Success.Should().BeTrue();
        result.MessageId.Should().StartWith("test_");
        // SDK must NOT be called in test mode
        mockClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsFailedResult_WhenSdkThrowsException()
    {
        var (provider, mockClient) = CreateProvider();

        mockClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network unavailable (test)"));

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@example.com",
            Subject = "Fail",
            Body = "Body"
        });

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Network unavailable");
    }

    // ── SendTemplateEmailAsync ────────────────────────────────────────────────

    [Fact]
    public async Task SendTemplateEmailAsync_ReturnsFailedResult_WhenConfigIsInvalid()
    {
        var config = ValidConfig();
        config.ApiKey = "";
        var (provider, mockClient) = CreateProvider(config);

        var result = await provider.SendTemplateEmailAsync(
            "d-1234567890abcdefgh",
            "user@example.com",
            new { name = "Test User" });

        result.Success.Should().BeFalse();
        mockClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ReturnsSuccessResult_WhenApiAccepts()
    {
        var (provider, mockClient) = CreateProvider();
        var sgResponse = CreateResponse(HttpStatusCode.Accepted);

        mockClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sgResponse);

        var result = await provider.SendTemplateEmailAsync(
            "d-1234567890abcde",
            "user@example.com",
            new Dictionary<string, object> { ["name"] = "CRM User" });

        result.Success.Should().BeTrue();
        result.Channel.Should().Be("email");
    }

    // ── SMS / Push / InApp – Not Supported ────────────────────────────────────

    [Fact]
    public async Task SendSmsAsync_ReturnsFailedResult_WithNotSupportedError()
    {
        var (provider, mockClient) = CreateProvider();

        var result = await provider.SendSmsAsync(new SmsNotificationRequest
        {
            To = "+15555550100",
            Message = "Hello"
        });

        result.Success.Should().BeFalse();
        result.Channel.Should().Be("sms");
        result.Error.Should().Contain("Twilio");
        mockClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SendPushAsync_ReturnsFailedResult_WithNotSupportedError()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.SendPushAsync(new PushNotificationRequest
        {
            To = "device-token-abc",
            Title = "Alert",
            Body = "You have new leads"
        });

        result.Success.Should().BeFalse();
        result.Channel.Should().Be("push");
        result.Error.Should().Contain("not supported");
    }

    [Fact]
    public async Task SendInAppAsync_ReturnsFailedResult_WithNotSupportedError()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.SendInAppAsync(new InAppNotificationRequest
        {
            UserId = "user-001",
            Title = "New Task",
            Content = "You have a new task assigned"
        });

        result.Success.Should().BeFalse();
        result.Channel.Should().Be("in_app");
        result.Error.Should().Contain("not supported");
    }

    // ── SendGridConfiguration ────────────────────────────────────────────────

    [Fact]
    public void SendGridConfiguration_IsValid_ReturnsFalse_WhenApiKeyIsEmpty()
    {
        var config = new SendGridConfiguration { ApiKey = "", FromEmail = "from@example.com" };
        config.IsValid().Should().BeFalse();
    }

    [Fact]
    public void SendGridConfiguration_IsValid_ReturnsFalse_WhenFromEmailIsEmpty()
    {
        var config = new SendGridConfiguration { ApiKey = "SG.key", FromEmail = "" };
        config.IsValid().Should().BeFalse();
    }

    [Fact]
    public void SendGridConfiguration_IsValid_ReturnsTrue_WhenBothAreSet()
    {
        var config = ValidConfig();
        config.IsValid().Should().BeTrue();
    }
}
