// CRM Solution - Pluggable Architecture
// TwilioProvider Unit Tests
// Week 10: Tests for SMS/WhatsApp notification provider

using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Twilio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for TwilioProvider.
/// Tests SMS notification capabilities and configuration validation.
/// </summary>
public class TwilioProviderTests
{
    private readonly Mock<ILogger<TwilioProvider>> _loggerMock;
    private readonly TwilioConfiguration _validConfig;
    private readonly TwilioConfiguration _invalidConfig;

    public TwilioProviderTests()
    {
        _loggerMock = new Mock<ILogger<TwilioProvider>>();
        
        _validConfig = new TwilioConfiguration
        {
            AccountSid = "ACtest123456789012345678901234",
            AuthToken = "test_auth_token_32_characters_ok",
            FromPhoneNumber = "+15551234567",
            MessagingServiceSid = "MGtest123456789012345678901234",
            WhatsAppFromNumber = "+15557654321",
            TrackDeliveryStatus = true,
            IsSandbox = true
        };

        _invalidConfig = new TwilioConfiguration
        {
            AccountSid = "",
            AuthToken = "",
            FromPhoneNumber = ""
        };
    }

    #region Provider Name Tests

    [Fact]
    public void ProviderName_ShouldReturn_Twilio()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);

        // Act
        var name = provider.ProviderName;

        // Assert
        Assert.Equal("Twilio", name);
    }

    #endregion

    #region Configuration Validation Tests

    [Fact]
    public void Configuration_IsValid_WithValidConfig_ReturnsTrue()
    {
        // Arrange & Act
        var isValid = _validConfig.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Configuration_IsValid_WithEmptyAccountSid_ReturnsFalse()
    {
        // Arrange
        var config = new TwilioConfiguration
        {
            AccountSid = "",
            AuthToken = "valid_token",
            FromPhoneNumber = "+15551234567"
        };

        // Act
        var isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Configuration_IsValid_WithEmptyAuthToken_ReturnsFalse()
    {
        // Arrange
        var config = new TwilioConfiguration
        {
            AccountSid = "ACtest123456789012345678901234",
            AuthToken = "",
            FromPhoneNumber = "+15551234567"
        };

        // Act
        var isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Configuration_IsValid_WithEmptyFromNumber_ReturnsFalse()
    {
        // Arrange
        var config = new TwilioConfiguration
        {
            AccountSid = "ACtest123456789012345678901234",
            AuthToken = "valid_token",
            FromPhoneNumber = ""
        };

        // Act
        var isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region SMS Request Tests

    [Fact]
    public async Task SendSmsAsync_WithInvalidConfig_ReturnsFailure()
    {
        // Arrange
        var options = Options.Create(_invalidConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);
        var request = new SmsNotificationRequest
        {
            To = "+15559876543",
            Message = "Test message"
        };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        // Twilio client won't be initialized with invalid config
        Assert.Contains("not initialized", result.Error.ToLower());
    }

    [Fact]
    public async Task SendSmsAsync_WithValidRequest_AttemptsToSend()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);
        var request = new SmsNotificationRequest
        {
            To = "+15559876543",
            Message = "Test message from CRM"
        };

        // Act - This will fail because we don't have real Twilio credentials
        var result = await provider.SendSmsAsync(request);

        // Assert - Without real credentials, it should fail gracefully
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    #endregion

    #region Bulk SMS Tests

    [Fact]
    public async Task SendBulkSmsAsync_WithInvalidConfig_ReturnsAllFailures()
    {
        // Arrange
        var options = Options.Create(_invalidConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);
        var requests = new List<SmsNotificationRequest>
        {
            new() { To = "+15559876543", Message = "Test 1" },
            new() { To = "+15559876544", Message = "Test 2" }
        };

        // Act
        var result = await provider.SendBulkSmsAsync(requests);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(2, result.FailureCount);
    }

    [Fact]
    public async Task SendBulkSmsAsync_WithEmptyList_ReturnsEmptyResult()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);
        var requests = new List<SmsNotificationRequest>();

        // Act
        var result = await provider.SendBulkSmsAsync(requests);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.SuccessCount);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithInvalidConfig_ReturnsUnhealthy()
    {
        // Arrange
        var options = Options.Create(_invalidConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsHealthy);
        Assert.Contains("invalid", result.Message?.ToLower() ?? "");
        Assert.Equal("Twilio", result.ProviderName);
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsProviderName()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.Equal("Twilio", result.ProviderName);
        Assert.NotNull(result.CheckedAt);
    }

    #endregion

    #region Unsupported Operations Tests

    [Fact]
    public async Task SendEmailAsync_ReturnsUnsupported()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);
        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test body"
        };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not supported", result.Error?.ToLower() ?? "");
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ReturnsUnsupported()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);

        // Act
        var result = await provider.SendTemplateEmailAsync(
            "template-1",
            "test@example.com",
            new { name = "Test" });

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not supported", result.Error?.ToLower() ?? "");
    }

    [Fact]
    public async Task SendPushAsync_ReturnsUnsupported()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);
        var request = new PushNotificationRequest
        {
            To = "test-token",
            Title = "Test",
            Body = "Test body"
        };

        // Act
        var result = await provider.SendPushAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not supported", result.Error?.ToLower() ?? "");
    }

    [Fact]
    public async Task SendInAppAsync_ReturnsUnsupported()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);
        var request = new InAppNotificationRequest
        {
            UserId = "user-1",
            Title = "Test",
            Content = "Test message"
        };

        // Act
        var result = await provider.SendInAppAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not supported", result.Error?.ToLower() ?? "");
    }

    #endregion

    #region Subscriber Management Tests

    [Fact]
    public async Task UpsertSubscriberAsync_ReturnsSubscriberId()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);
        var request = new SubscriberRequest
        {
            SubscriberId = "ext-123",
            Email = "test@example.com"
        };

        // Act
        var result = await provider.UpsertSubscriberAsync(request);

        // Assert
        // Twilio returns the subscriber ID (phone or externalId) since it doesn't manage subscribers
        Assert.NotNull(result);
        Assert.Equal("ext-123", result);
    }

    #endregion

    #region Multi-Channel Notification Tests

    [Fact]
    public async Task SendNotificationAsync_WithSmsChannel_ProcessesRequest()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);
        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "test-recipient",
            Channels = new List<string> { "sms" },
            Content = new Dictionary<string, object>
            {
                { "sms", "Test SMS content" },
                { "phone", "+15559876543" }
            }
        };

        // Act
        var result = await provider.SendNotificationAsync(request);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Supported Channels Tests

    [Fact]
    public void SupportedChannels_Contains_Sms()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);

        // Act
        var channels = provider.SupportedChannels;

        // Assert
        Assert.Contains("sms", channels);
    }

    [Fact]
    public void SupportedChannels_Contains_Whatsapp()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new TwilioProvider(options, _loggerMock.Object);

        // Act
        var channels = provider.SupportedChannels;

        // Assert
        Assert.Contains("whatsapp", channels);
    }

    #endregion
}
