// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInNotificationProvider.
/// Tests SMTP email delivery, not-supported channels (SMS, Push), in-app stub,
/// bulk operations, subscriber management, and health-check paths.
///
/// MANDATORY pre-write verification:
///   Class     : BuiltInNotificationProvider
///   Namespace : CRM.Infrastructure.Providers.BuiltIn
///   Constructor: (IConfiguration configuration, ILogger&lt;BuiltInNotificationProvider&gt; logger)
///   ProviderName: "BuiltIn"
///   SupportedChannels: ["email"]
///   Source read: verified 2026-03-03
/// </summary>
public class BuiltInNotificationProviderTests
{
    // ── Factory helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Builds a real IConfiguration from an in-memory dictionary so that
    /// GetSection("Smtp").Bind(...) works correctly inside the constructor.
    /// </summary>
    private static IConfiguration BuildConfig(
        string smtpHost = "",
        int smtpPort = 0,
        bool enableSsl = true,
        string? username = null,
        string? password = null,
        string? fromEmail = null,
        string? fromName = null,
        int timeoutSeconds = 30)
    {
        var values = new Dictionary<string, string?>
        {
            ["Smtp:Host"] = smtpHost,
            ["Smtp:Port"] = smtpPort.ToString(),
            ["Smtp:EnableSsl"] = enableSsl.ToString(),
            ["Smtp:TimeoutSeconds"] = timeoutSeconds.ToString(),
        };

        if (username != null) values["Smtp:Username"] = username;
        if (password != null) values["Smtp:Password"] = password;
        if (fromEmail != null) values["Smtp:FromEmail"] = fromEmail;
        if (fromName != null) values["Smtp:FromName"] = fromName;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static BuiltInNotificationProvider CreateProvider(
        IConfiguration? config = null,
        Mock<ILogger<BuiltInNotificationProvider>>? loggerMock = null)
    {
        config ??= BuildConfig(); // no SMTP configured (dev mode)
        loggerMock ??= new Mock<ILogger<BuiltInNotificationProvider>>();
        return new BuiltInNotificationProvider(config, loggerMock.Object);
    }

    // ── Constructor Guards ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigurationIsNull()
    {
        // Arrange & Act
        var act = () => new BuiltInNotificationProvider(
            null!,
            new Mock<ILogger<BuiltInNotificationProvider>>().Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act
        var act = () => new BuiltInNotificationProvider(BuildConfig(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        // Arrange
        var provider = CreateProvider();

        // Assert
        provider.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public void SupportedChannels_ContainsEmail()
    {
        // Arrange
        var provider = CreateProvider();

        // Assert
        provider.SupportedChannels.Should().Contain("email");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenSmtpHostIsEmpty()
    {
        // Arrange - no SMTP host configured
        var provider = CreateProvider(BuildConfig(smtpHost: "", smtpPort: 0));

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenSmtpIsConfigured()
    {
        // Arrange
        var provider = CreateProvider(BuildConfig(smtpHost: "smtp.example.com", smtpPort: 587));

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenHostSetButPortIsZero()
    {
        // Arrange - host present but port == 0 (invalid)
        var provider = CreateProvider(BuildConfig(smtpHost: "smtp.example.com", smtpPort: 0));

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    // ── SendEmailAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task SendEmailAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.SendEmailAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendEmailAsync_ThrowsArgumentException_WhenToIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new EmailNotificationRequest
        {
            To = string.Empty,
            Subject = "Hello",
            Body = "World"
        };

        // Act
        var act = async () => await provider.SendEmailAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("request");
    }

    [Fact]
    public async Task SendEmailAsync_ThrowsArgumentException_WhenSubjectIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new EmailNotificationRequest
        {
            To = "user@example.com",
            Subject = string.Empty,
            Body = "World"
        };

        // Act
        var act = async () => await provider.SendEmailAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("request");
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsSuccessWithDevMessageId_WhenSmtpNotConfigured()
    {
        // Arrange - no SMTP host → dev/fallback mode
        var provider = CreateProvider(BuildConfig(smtpHost: "", smtpPort: 0));
        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Dev Test",
            Body = "Hello from dev"
        };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.MessageId.Should().StartWith("dev_");
        result.Provider.Should().Be("BuiltIn");
        result.Channel.Should().Be("email");
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsProviderAndChannel_WhenSuccessful()
    {
        // Arrange - dev mode (no SMTP) produces success path
        var provider = CreateProvider();
        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Subject",
            Body = "<p>Body</p>",
            IsHtml = true
        };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        result.Provider.Should().Be("BuiltIn");
        result.Channel.Should().Be("email");
    }

    // ── SendTemplateEmailAsync ───────────────────────────────────────────────

    [Fact]
    public async Task SendTemplateEmailAsync_ReturnsFailure_Always()
    {
        // Arrange - BuiltIn does not support templates
        var provider = CreateProvider();

        // Act
        var result = await provider.SendTemplateEmailAsync(
            templateId: "welcome_email",
            recipientEmail: "user@example.com",
            data: new { name = "Alice" });

        // Assert
        result.Success.Should().BeFalse();
        result.Provider.Should().Be("BuiltIn");
        result.Channel.Should().Be("email");
        result.Error.Should().NotBeNullOrEmpty();
    }

    // ── SendSmsAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SendSmsAsync_ReturnsFailure_WhenCalledOnBuiltIn()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new SmsNotificationRequest { To = "+15555555555", Message = "Hello" };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Channel.Should().Be("sms");
        result.Provider.Should().Be("BuiltIn");
        result.Error.Should().NotBeNullOrEmpty();
    }

    // ── SendPushAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task SendPushAsync_ReturnsFailure_WhenCalledOnBuiltIn()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new PushNotificationRequest { To = "device_token_xyz", Title = "Alert", Body = "Content" };

        // Act
        var result = await provider.SendPushAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Channel.Should().Be("push");
        result.Provider.Should().Be("BuiltIn");
        result.Error.Should().NotBeNullOrEmpty();
    }

    // ── SendInAppAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task SendInAppAsync_ReturnsSuccess_AsStubImplementation()
    {
        // Arrange - BuiltIn logs and returns success; real delivery is via SignalR/DB
        var provider = CreateProvider();
        var request = new InAppNotificationRequest
        {
            UserId = "user_42",
            Title = "New Lead Assigned",
            Content = "You have been assigned a new lead."
        };

        // Act
        var result = await provider.SendInAppAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Channel.Should().Be("in_app");
        result.MessageId.Should().StartWith("inapp_");
        result.Provider.Should().Be("BuiltIn");
    }

    // ── TriggerWorkflowAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task TriggerWorkflowAsync_ReturnsFailure_WhenCalledOnBuiltIn()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.TriggerWorkflowAsync(
            workflowId: "onboarding_flow",
            subscriberId: "sub_42",
            payload: new { eventName = "signup" });

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        result.Provider.Should().Be("BuiltIn");
    }

    // ── SendBulkEmailAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task SendBulkEmailAsync_ThrowsArgumentNullException_WhenRequestsIsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.SendBulkEmailAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendBulkEmailAsync_ReturnsCorrectCounts_WhenDevModeAllSucceed()
    {
        // Arrange - dev mode (no SMTP) → all succeed
        var provider = CreateProvider(BuildConfig(smtpHost: "", smtpPort: 0));
        var requests = new List<EmailNotificationRequest>
        {
            new() { To = "a@example.com", Subject = "One", Body = "Body1" },
            new() { To = "b@example.com", Subject = "Two", Body = "Body2" },
            new() { To = "c@example.com", Subject = "Three", Body = "Body3" }
        };

        // Act
        var result = await provider.SendBulkEmailAsync(requests);

        // Assert
        result.TotalCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);
        result.Results.Should().HaveCount(3);
    }

    [Fact]
    public async Task SendBulkEmailAsync_ReturnsEmptyResult_WhenNoRequests()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.SendBulkEmailAsync(new List<EmailNotificationRequest>());

        // Assert
        result.TotalCount.Should().Be(0);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
        result.Results.Should().BeEmpty();
    }

    // ── SendBulkSmsAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SendBulkSmsAsync_ReturnsAllFailures_WhenCalledOnBuiltIn()
    {
        // Arrange
        var provider = CreateProvider();
        var requests = new List<SmsNotificationRequest>
        {
            new() { To = "+15550001111", Message = "Hi" },
            new() { To = "+15550002222", Message = "Ho" }
        };

        // Act
        var result = await provider.SendBulkSmsAsync(requests);

        // Assert
        result.TotalCount.Should().Be(2);
        result.FailureCount.Should().Be(2);
        result.SuccessCount.Should().Be(0);
        result.Results.Should().AllSatisfy(r => r.Success.Should().BeFalse());
        result.Results.Should().AllSatisfy(r => r.Channel.Should().Be("sms"));
    }

    // ── SendNotificationAsync (multi-channel) ────────────────────────────────

    [Fact]
    public async Task SendNotificationAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.SendNotificationAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendNotificationAsync_ReturnsTransactionId_WithEmailChannel()
    {
        // Arrange - dev mode, email channel
        var provider = CreateProvider(BuildConfig(smtpHost: "", smtpPort: 0));
        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "user_10",
            Channels = new List<string> { "email" },
            Content = new Dictionary<string, object>
            {
                ["email"] = "notify@example.com",
                ["subject"] = "Alert for user",
                ["body"] = "You have a new notification"
            }
        };

        // Act
        var result = await provider.SendNotificationAsync(request);

        // Assert
        result.TransactionId.Should().StartWith("tx_");
        result.ChannelResults.Should().ContainKey("email");
    }

    [Fact]
    public async Task SendNotificationAsync_SetsInAppChannelSuccess_WhenChannelIsInApp()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "user_99",
            Channels = new List<string> { "in_app" },
            Content = new Dictionary<string, object>
            {
                ["title"] = "Test",
                ["body"] = "Body content"
            }
        };

        // Act
        var result = await provider.SendNotificationAsync(request);

        // Assert
        result.ChannelResults.Should().ContainKey("in_app");
        result.ChannelResults["in_app"].Success.Should().BeTrue();
    }

    // ── Subscriber Management ────────────────────────────────────────────────

    [Fact]
    public async Task UpsertSubscriberAsync_ReturnsPassthroughSubscriberId()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new SubscriberRequest
        {
            SubscriberId = "sub_abc",
            Email = "sub@example.com",
            FirstName = "Alice"
        };

        // Act
        var subscriberId = await provider.UpsertSubscriberAsync(request);

        // Assert
        subscriberId.Should().Be("sub_abc");
    }

    [Fact]
    public async Task DeleteSubscriberAsync_CompletesSuccessfully_AsNoOp()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.DeleteSubscriberAsync("sub_xyz");

        // Assert - should not throw
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetSubscriberPreferencesAsync_ReturnsDefaultPreferences()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var prefs = await provider.GetSubscriberPreferencesAsync("sub_42");

        // Assert
        prefs.Should().NotBeNull();
        prefs!.GlobalOptOut.Should().BeFalse();
        prefs.ChannelPreferences.Should().ContainKey("email");
        prefs.ChannelPreferences["email"].Should().BeTrue();
        prefs.ChannelPreferences.Should().ContainKey("in_app");
        prefs.ChannelPreferences["in_app"].Should().BeTrue();
        prefs.ChannelPreferences.Should().ContainKey("sms");
        prefs.ChannelPreferences["sms"].Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSubscriberPreferencesAsync_CompletesSuccessfully_AsNoOp()
    {
        // Arrange
        var provider = CreateProvider();
        var prefs = new SubscriberPreferences
        {
            GlobalOptOut = true,
            ChannelPreferences = new Dictionary<string, bool> { ["email"] = false }
        };

        // Act
        var act = async () => await provider.UpdateSubscriberPreferencesAsync("sub_1", prefs);

        // Assert - should not throw
        await act.Should().NotThrowAsync();
    }

    // ── Delivery Status ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetDeliveryStatusAsync_ReturnsNull_ForBuiltIn()
    {
        // Arrange - SMTP does not provide reliable delivery tracking
        var provider = CreateProvider();

        // Act
        var status = await provider.GetDeliveryStatusAsync("msg_someId");

        // Assert
        status.Should().BeNull();
    }

    [Fact]
    public async Task ProcessDeliveryWebhookAsync_ReturnsDeliveryEvent_WithNoSupportMessage()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var deliveryEvent = await provider.ProcessDeliveryWebhookAsync(
            eventType: "delivery",
            payload: "{}");

        // Assert
        deliveryEvent.Should().NotBeNull();
        deliveryEvent.EventType.Should().Be("delivery");
        deliveryEvent.Data.Should().ContainKey("error");
    }

    // ── HealthCheckAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_ReturnsUnhealthy_WhenSmtpNotConfigured()
    {
        // Arrange
        var provider = CreateProvider(BuildConfig(smtpHost: "", smtpPort: 0));

        // Act
        var health = await provider.HealthCheckAsync();

        // Assert
        health.IsHealthy.Should().BeFalse();
        health.ProviderName.Should().Be("BuiltIn");
        health.Message.Should().Contain("not configured");
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthy_WhenSmtpIsConfigured()
    {
        // Arrange
        var provider = CreateProvider(BuildConfig(smtpHost: "smtp.example.com", smtpPort: 587));

        // Act
        var health = await provider.HealthCheckAsync();

        // Assert
        health.IsHealthy.Should().BeTrue();
        health.ProviderName.Should().Be("BuiltIn");
        health.Message.Should().Contain("smtp.example.com");
    }
}
