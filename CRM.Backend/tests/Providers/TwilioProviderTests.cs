// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Twilio;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for TwilioProvider (INotificationPort).
///
/// Twilio handling: The provider uses the Twilio SDK directly via
/// <c>TwilioClient.Init()</c> and <c>MessageResource.CreateAsync()</c>.
/// There is NO HttpClient injection. To avoid real API calls:
/// - Tests for SMS sending use <c>TestMode = true</c> which bypasses
///   <c>MessageResource.CreateAsync()</c> entirely.
/// - Tests for availability use the synchronous <c>_isInitialized</c> flag.
/// - Tests for unsupported channels (email, push, in-app) test pure logic.
/// - Tests for webhook parsing test pure data-parsing logic.
///
/// MANDATORY: Written after verifying source for:
///   Class: TwilioProvider, Namespace: CRM.Infrastructure.Providers.Twilio
///   Constructor: (IOptions&lt;TwilioConfiguration&gt;, ILogger&lt;TwilioProvider&gt;)
///   SDK: Twilio.TwilioClient.Init() + MessageResource.CreateAsync()
///   TestMode: skips SDK call, returns synthetic success result
/// </summary>
public class TwilioProviderTests
{
    // ── Factory helpers ─────────────────────────────────────────────────────

    private static TwilioConfiguration InvalidConfig() => new()
    {
        // Missing AccountSid/AuthToken → IsValid() returns false → _isInitialized = false
        AccountSid = string.Empty,
        AuthToken = string.Empty,
        FromPhoneNumber = string.Empty
    };

    private static TwilioConfiguration ValidTestModeConfig() => new()
    {
        AccountSid = "ACtest00000000000000000000000000",
        AuthToken = "auth_token_test_value_1234567890",
        FromPhoneNumber = "+12025550123",
        TestMode = true
    };

    private static TwilioProvider CreateProvider(TwilioConfiguration config)
    {
        var options = Options.Create(config);
        var logger = new Mock<ILogger<TwilioProvider>>();
        return new TwilioProvider(options, logger.Object);
    }

    // ── Provider metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsTwilio()
    {
        var provider = CreateProvider(InvalidConfig());
        provider.ProviderName.Should().Be("Twilio");
    }

    [Fact]
    public void SupportedChannels_ContainsSmsWhatsAppVoice()
    {
        var provider = CreateProvider(InvalidConfig());
        provider.SupportedChannels.Should().Contain(new[] { "sms", "whatsapp", "voice" });
    }

    // ── IsAvailableAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenConfigInvalid()
    {
        var provider = CreateProvider(InvalidConfig());

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenAccountSidMissing()
    {
        var config = new TwilioConfiguration
        {
            AccountSid = string.Empty,
            AuthToken = "some-token",
            FromPhoneNumber = "+12025550100"
        };
        var provider = CreateProvider(config);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenValidConfigAndInitialized()
    {
        var provider = CreateProvider(ValidTestModeConfig());

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    // ── SendSmsAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task SendSmsAsync_ReturnsFailure_WhenNotInitialized()
    {
        var provider = CreateProvider(InvalidConfig());

        var result = await provider.SendSmsAsync(new SmsNotificationRequest
        {
            To = "+12025550199",
            Message = "Test message"
        });

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not initialized");
        result.Provider.Should().Be("Twilio");
    }

    [Fact]
    public async Task SendSmsAsync_ReturnsSuccess_WhenTestModeEnabled()
    {
        var provider = CreateProvider(ValidTestModeConfig());

        var result = await provider.SendSmsAsync(new SmsNotificationRequest
        {
            To = "+12025550100",
            Message = "Hello from test mode"
        });

        result.Success.Should().BeTrue();
        result.MessageId.Should().StartWith("test_");
        result.Provider.Should().Be("Twilio");
        result.Channel.Should().Be("sms");
    }

    // ── SendBulkSmsAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task SendBulkSmsAsync_ReturnsAllSuccess_WhenTestModeEnabled()
    {
        var provider = CreateProvider(ValidTestModeConfig());
        var requests = new[]
        {
            new SmsNotificationRequest { To = "+12025550101", Message = "Bulk msg 1" },
            new SmsNotificationRequest { To = "+12025550102", Message = "Bulk msg 2" },
            new SmsNotificationRequest { To = "+12025550103", Message = "Bulk msg 3" }
        };

        var result = await provider.SendBulkSmsAsync(requests);

        result.TotalCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task SendBulkSmsAsync_ReturnsAllFailures_WhenNotInitialized()
    {
        var provider = CreateProvider(InvalidConfig());
        var requests = new[]
        {
            new SmsNotificationRequest { To = "+12025550101", Message = "Msg 1" },
            new SmsNotificationRequest { To = "+12025550102", Message = "Msg 2" }
        };

        var result = await provider.SendBulkSmsAsync(requests);

        result.TotalCount.Should().Be(2);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(2);
    }

    // ── Unsupported channel methods ─────────────────────────────────────────

    [Fact]
    public async Task SendEmailAsync_ReturnsFailure_WithNotSupportedError()
    {
        var provider = CreateProvider(ValidTestModeConfig());

        var result = await provider.SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@crm.local",
            Subject = "Test",
            Body = "Body"
        });

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
        result.Channel.Should().Be("email");
    }

    [Fact]
    public async Task SendPushAsync_ReturnsFailure_WithNotSupportedError()
    {
        var provider = CreateProvider(ValidTestModeConfig());

        var result = await provider.SendPushAsync(new PushNotificationRequest
        {
            To = "device-token",
            Title = "Push",
            Body = "Body"
        });

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
        result.Channel.Should().Be("push");
    }

    [Fact]
    public async Task SendInAppAsync_ReturnsFailure_WithNotSupportedError()
    {
        var provider = CreateProvider(ValidTestModeConfig());

        var result = await provider.SendInAppAsync(new InAppNotificationRequest
        {
            UserId = "user-123",
            Title = "Notification",
            Content = "A message"
        });

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
        result.Channel.Should().Be("in_app");
    }

    // ── ProcessDeliveryWebhookAsync ─────────────────────────────────────────

    [Fact]
    public async Task ProcessDeliveryWebhookAsync_ParsesPayload_WhenValidFormEncoded()
    {
        var provider = CreateProvider(ValidTestModeConfig());
        const string payload = "MessageSid=SM001&MessageStatus=delivered&To=%2B12025550100&From=%2B12025550001";

        var result = await provider.ProcessDeliveryWebhookAsync("status", payload);

        result.NotificationId.Should().Be("SM001");
        result.EventType.Should().Be("delivered");
        result.Channel.Should().Be("sms");
        result.Data.Should().ContainKey("twilioStatus");
    }

    [Fact]
    public async Task ProcessDeliveryWebhookAsync_HandlesFailedStatus()
    {
        var provider = CreateProvider(ValidTestModeConfig());
        const string payload = "MessageSid=SM002&MessageStatus=failed&ErrorCode=30001&ErrorMessage=Queue+overflow&To=%2B12025550100&From=%2B12025550001";

        var result = await provider.ProcessDeliveryWebhookAsync("status", payload);

        result.EventType.Should().Be("failed");
        result.Data["errorCode"].Should().Be("30001");
    }

    // ── Subscriber management (no-op) ───────────────────────────────────────

    [Fact]
    public async Task UpsertSubscriberAsync_ReturnsPhoneAsId_WhenPhoneProvided()
    {
        var provider = CreateProvider(ValidTestModeConfig());

        var id = await provider.UpsertSubscriberAsync(new SubscriberRequest
        {
            Phone = "+12025550199"
        });

        id.Should().Be("+12025550199");
    }

    // ── SendNotificationAsync ───────────────────────────────────────────────

    [Fact]
    public async Task SendNotificationAsync_ReturnsFailure_ForUnsupportedChannel()
    {
        var provider = CreateProvider(ValidTestModeConfig());

        var result = await provider.SendNotificationAsync(new MultiChannelNotificationRequest
        {
            Channels = new List<string> { "email_unsupported" },
            Content = new Dictionary<string, object> { ["body"] = "Hello" }
        });

        result.ChannelResults.Should().ContainKey("email_unsupported");
        result.ChannelResults["email_unsupported"].Success.Should().BeFalse();
    }
}
