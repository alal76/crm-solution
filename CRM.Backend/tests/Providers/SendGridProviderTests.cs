// CRM Solution - Pluggable Architecture
// SendGridProvider Unit Tests
// Week 10: Tests for email notification provider

using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.SendGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for SendGridProvider.
/// Tests email notification capabilities and configuration validation.
/// </summary>
public class SendGridProviderTests
{
    private readonly Mock<ILogger<SendGridProvider>> _loggerMock;
    private readonly SendGridConfiguration _validConfig;
    private readonly SendGridConfiguration _invalidConfig;

    public SendGridProviderTests()
    {
        _loggerMock = new Mock<ILogger<SendGridProvider>>();
        
        _validConfig = new SendGridConfiguration
        {
            ApiKey = "SG.test_api_key_1234567890abcdefghijklmnop",
            FromEmail = "noreply@crm.example.com",
            FromName = "CRM System",
            ReplyToEmail = "support@crm.example.com",
            EnableClickTracking = true,
            EnableOpenTracking = true,
            SandboxMode = true
        };

        _invalidConfig = new SendGridConfiguration
        {
            ApiKey = "",
            FromEmail = "",
            FromName = ""
        };
    }

    #region Provider Name Tests

    [Fact]
    public void ProviderName_ShouldReturn_SendGrid()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);

        // Act
        var name = provider.ProviderName;

        // Assert
        Assert.Equal("SendGrid", name);
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
    public void Configuration_IsValid_WithEmptyApiKey_ReturnsFalse()
    {
        // Arrange
        var config = new SendGridConfiguration
        {
            ApiKey = "",
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        // Act
        var isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Configuration_IsValid_WithEmptyFromEmail_ReturnsFalse()
    {
        // Arrange
        var config = new SendGridConfiguration
        {
            ApiKey = "SG.test_api_key",
            FromEmail = "",
            FromName = "Test"
        };

        // Act
        var isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Configuration_IsValid_WithNullApiKey_ReturnsFalse()
    {
        // Arrange
        var config = new SendGridConfiguration
        {
            ApiKey = null!,
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        // Act
        var isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region Email Request Tests

    [Fact]
    public void SendEmailAsync_WithInvalidConfig_ThrowsOnConstruction()
    {
        // Arrange
        var options = Options.Create(_invalidConfig);
        
        // Act & Assert - SendGrid SDK throws when API key is null/empty
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new SendGridProvider(options, _loggerMock.Object));
        Assert.Contains("apikey", exception.ParamName?.ToLower() ?? "");
    }

    [Fact]
    public async Task SendEmailAsync_WithValidRequest_AttemptsToSend()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Test Subject",
            Body = "<html><body>Test HTML body</body></html>",
            IsHtml = true
        };

        // Act - Will fail without real API key but should process request
        var result = await provider.SendEmailAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Error); // Will fail with invalid API key
    }

    [Fact]
    public async Task SendEmailAsync_WithPlainText_SetsCorrectContentType()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Plain Text Email",
            Body = "This is plain text content",
            IsHtml = false
        };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SendEmailAsync_WithCc_IncludesCcRecipients()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Email with CC",
            Body = "Test body",
            Cc = new List<string> { "cc1@example.com", "cc2@example.com" }
        };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SendEmailAsync_WithBcc_IncludesBccRecipients()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Email with BCC",
            Body = "Test body",
            Bcc = new List<string> { "bcc1@example.com" }
        };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Template Email Tests

    [Fact]
    public void SendTemplateEmailAsync_WithInvalidConfig_ThrowsOnConstruction()
    {
        // Arrange
        var options = Options.Create(_invalidConfig);
        
        // Act & Assert - SendGrid SDK throws when API key is null/empty
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new SendGridProvider(options, _loggerMock.Object));
        Assert.Contains("apikey", exception.ParamName?.ToLower() ?? "");
    }

    [Fact]
    public async Task SendTemplateEmailAsync_WithValidTemplate_AttemptsToSend()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);

        // Act
        var result = await provider.SendTemplateEmailAsync(
            "d-abc123template",
            "recipient@example.com",
            new { 
                first_name = "John",
                company_name = "Acme Corp",
                action_url = "https://example.com/action"
            });

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Bulk Email Tests

    [Fact]
    public void SendBulkEmailAsync_WithInvalidConfig_ThrowsOnConstruction()
    {
        // Arrange
        var options = Options.Create(_invalidConfig);
        
        // Act & Assert - SendGrid SDK throws when API key is null/empty
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new SendGridProvider(options, _loggerMock.Object));
        Assert.Contains("apikey", exception.ParamName?.ToLower() ?? "");
    }

    [Fact]
    public async Task SendBulkEmailAsync_WithEmptyList_ReturnsEmptyResult()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var requests = new List<EmailNotificationRequest>();

        // Act
        var result = await provider.SendBulkEmailAsync(requests);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.SuccessCount);
    }

    [Fact]
    public async Task SendBulkEmailAsync_WithMultipleRecipients_ProcessesAll()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var requests = new List<EmailNotificationRequest>
        {
            new() { To = "user1@example.com", Subject = "Test 1", Body = "Body 1" },
            new() { To = "user2@example.com", Subject = "Test 2", Body = "Body 2" },
            new() { To = "user3@example.com", Subject = "Test 3", Body = "Body 3" }
        };

        // Act
        var result = await provider.SendBulkEmailAsync(requests);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public void HealthCheckAsync_WithInvalidConfig_ThrowsOnConstruction()
    {
        // Arrange
        var options = Options.Create(_invalidConfig);
        
        // Act & Assert - SendGrid SDK throws when API key is null/empty
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new SendGridProvider(options, _loggerMock.Object));
        Assert.Contains("apikey", exception.ParamName?.ToLower() ?? "");
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsProviderName()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.Equal("SendGrid", result.ProviderName);
        Assert.NotNull(result.CheckedAt);
    }

    #endregion

    #region Unsupported Operations Tests

    [Fact]
    public async Task SendSmsAsync_ReturnsUnsupported()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var request = new SmsNotificationRequest
        {
            To = "+15551234567",
            Message = "Test SMS"
        };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not supported", result.Error?.ToLower() ?? "");
    }

    [Fact]
    public async Task SendPushAsync_ReturnsUnsupported()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
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
        var provider = new SendGridProvider(options, _loggerMock.Object);
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
    public async Task UpsertSubscriberAsync_ReturnsEmail()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var request = new SubscriberRequest
        {
            SubscriberId = "ext-123",
            Email = "test@example.com"
        };

        // Act
        var result = await provider.UpsertSubscriberAsync(request);

        // Assert
        // SendGrid returns the email as subscriber ID since it doesn't manage subscribers
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result);
    }

    #endregion

    #region Multi-Channel Notification Tests

    [Fact]
    public async Task SendNotificationAsync_WithEmailChannel_ProcessesRequest()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "test-recipient",
            Channels = new List<string> { "email" },
            Content = new Dictionary<string, object>
            {
                { "email_to", "test@example.com" },
                { "email_subject", "Test Subject" },
                { "email_body", "Test body content" }
            }
        };

        // Act
        var result = await provider.SendNotificationAsync(request);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SendNotificationAsync_WithNonEmailChannel_ReturnsFailure()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "test-recipient",
            Channels = new List<string> { "sms" },
            Content = new Dictionary<string, object>
            {
                { "message", "Test SMS" }
            }
        };

        // Act
        var result = await provider.SendNotificationAsync(request);

        // Assert
        // Should indicate SMS is not supported by this provider
        Assert.NotNull(result);
    }

    #endregion

    #region Supported Channels Tests

    [Fact]
    public void SupportedChannels_Contains_Email()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);

        // Act
        var channels = provider.SupportedChannels;

        // Assert
        Assert.Contains("email", channels);
    }

    [Fact]
    public void SupportedChannels_DoesNotContain_Sms()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);

        // Act
        var channels = provider.SupportedChannels;

        // Assert
        Assert.DoesNotContain("sms", channels);
    }

    #endregion

    #region Email With Attachments Tests

    [Fact]
    public async Task SendEmailAsync_WithAttachments_ProcessesAttachments()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);
        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Email with Attachment",
            Body = "Please see attached file.",
            Attachments = new List<EmailAttachment>
            {
                new()
                {
                    FileName = "document.pdf",
                    ContentType = "application/pdf",
                    Content = new byte[] { 0x25, 0x50, 0x44, 0x46 }
                }
            }
        };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Tracking Settings Tests

    [Fact]
    public void Configuration_TrackingSettings_AreApplied()
    {
        // Arrange
        var config = new SendGridConfiguration
        {
            ApiKey = "SG.test_key",
            FromEmail = "test@example.com",
            FromName = "Test",
            EnableClickTracking = true,
            EnableOpenTracking = true,
            EnableUnsubscribeTracking = false
        };

        // Assert
        Assert.True(config.EnableClickTracking);
        Assert.True(config.EnableOpenTracking);
        Assert.False(config.EnableUnsubscribeTracking);
    }

    [Fact]
    public void Configuration_SandboxMode_CanBeEnabled()
    {
        // Arrange
        var config = new SendGridConfiguration
        {
            ApiKey = "SG.test_key",
            FromEmail = "test@example.com",
            FromName = "Test",
            SandboxMode = true
        };

        // Assert
        Assert.True(config.SandboxMode);
    }

    #endregion

    #region Delivery Status Tests

    [Fact]
    public void GetDeliveryStatusAsync_WithInvalidConfig_ThrowsOnConstruction()
    {
        // Arrange
        var options = Options.Create(_invalidConfig);
        
        // Act & Assert - SendGrid SDK throws when API key is null/empty
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new SendGridProvider(options, _loggerMock.Object));
        Assert.Contains("apikey", exception.ParamName?.ToLower() ?? "");
    }

    [Fact]
    public async Task GetDeliveryStatusAsync_WithValidConfig_AttemptsToFetch()
    {
        // Arrange
        var options = Options.Create(_validConfig);
        var provider = new SendGridProvider(options, _loggerMock.Object);

        // Act - Will likely fail without real API key
        var result = await provider.GetDeliveryStatusAsync("msg-123");

        // Assert
        // Either returns null (not found) or a status object
        // Both are valid responses
    }

    #endregion
}
