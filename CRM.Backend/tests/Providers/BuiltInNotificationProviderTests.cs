// Phase 2 Week 8: BuiltInNotificationProvider Unit Tests
// Tests for the SMTP-based notification provider

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInNotificationProvider.
/// Tests email sending, channel support verification, and health checks.
/// </summary>
public class BuiltInNotificationProviderTests
{
    private readonly Mock<ILogger<BuiltInNotificationProvider>> _loggerMock;
    private readonly IConfiguration _configurationWithSmtp;
    private readonly IConfiguration _configurationWithoutSmtp;

    public BuiltInNotificationProviderTests()
    {
        _loggerMock = new Mock<ILogger<BuiltInNotificationProvider>>();
        
        // Configuration with SMTP settings
        var smtpConfig = new Dictionary<string, string?>
        {
            { "Smtp:Host", "smtp.example.com" },
            { "Smtp:Port", "587" },
            { "Smtp:EnableSsl", "true" },
            { "Smtp:Username", "user@example.com" },
            { "Smtp:Password", "password123" },
            { "Smtp:FromEmail", "noreply@example.com" },
            { "Smtp:FromName", "Test CRM" },
            { "Smtp:TimeoutSeconds", "30" }
        };
        _configurationWithSmtp = new ConfigurationBuilder()
            .AddInMemoryCollection(smtpConfig)
            .Build();
            
        // Configuration without SMTP settings
        _configurationWithoutSmtp = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
    }

    #region Provider Identity Tests

    [Fact]
    public void ProviderName_ShouldReturnBuiltIn()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act
        var name = provider.ProviderName;

        // Assert
        Assert.Equal("BuiltIn", name);
    }

    [Fact]
    public void SupportedChannels_ShouldContainEmail()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act
        var channels = provider.SupportedChannels.ToList();

        // Assert
        Assert.Single(channels);
        Assert.Contains("email", channels);
    }

    #endregion

    #region Availability Tests

    [Fact]
    public async Task IsAvailableAsync_WithSmtpConfigured_ReturnsTrue()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act
        var isAvailable = await provider.IsAvailableAsync();

        // Assert
        Assert.True(isAvailable);
    }

    [Fact]
    public async Task IsAvailableAsync_WithoutSmtpConfigured_ReturnsFalse()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithoutSmtp, _loggerMock.Object);

        // Act
        var isAvailable = await provider.IsAvailableAsync();

        // Assert
        Assert.False(isAvailable);
    }

    #endregion

    #region Email Tests

    [Fact]
    public async Task SendEmailAsync_WithoutSmtp_ReturnsSuccessInDevMode()
    {
        // Arrange - no SMTP configured means dev mode
        var provider = new BuiltInNotificationProvider(_configurationWithoutSmtp, _loggerMock.Object);
        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "<p>Test body</p>",
            IsHtml = true
        };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.MessageId);
        Assert.StartsWith("dev_", result.MessageId);
        Assert.Equal("BuiltIn", result.Provider);
        Assert.Equal("email", result.Channel);
    }

    [Fact]
    public async Task SendEmailAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.SendEmailAsync(null!));
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyTo_ThrowsArgumentException()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);
        var request = new EmailNotificationRequest
        {
            To = "",
            Subject = "Test Subject",
            Body = "Test body"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.SendEmailAsync(request));
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptySubject_ThrowsArgumentException()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);
        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "",
            Body = "Test body"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.SendEmailAsync(request));
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act
        var result = await provider.SendTemplateEmailAsync("template123", "test@example.com", new { });

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not supported", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region SMS Tests

    [Fact]
    public async Task SendSmsAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);
        var request = new SmsNotificationRequest
        {
            To = "+1234567890",
            Message = "Test message"
        };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("sms", result.Channel);
        Assert.Contains("not supported", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Push Notification Tests

    [Fact]
    public async Task SendPushAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);
        var request = new PushNotificationRequest
        {
            To = "device_token",
            Title = "Test",
            Body = "Test body"
        };

        // Act
        var result = await provider.SendPushAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("push", result.Channel);
        Assert.Contains("not supported", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region In-App Notification Tests

    [Fact]
    public async Task SendInAppAsync_ReturnsSuccess()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);
        var request = new InAppNotificationRequest
        {
            UserId = "user123",
            Title = "Test Notification",
            Content = "Test content"
        };

        // Act
        var result = await provider.SendInAppAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.MessageId);
        Assert.StartsWith("inapp_", result.MessageId);
        Assert.Equal("in_app", result.Channel);
    }

    #endregion

    #region Multi-Channel Tests

    [Fact]
    public async Task SendNotificationAsync_WithEmailChannel_SendsEmail()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithoutSmtp, _loggerMock.Object);
        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "sub123",
            Channels = new List<string> { "email" },
            Content = new Dictionary<string, object>
            {
                { "email", "test@example.com" },
                { "subject", "Test Subject" },
                { "body", "Test body" }
            }
        };

        // Act
        var result = await provider.SendNotificationAsync(request);

        // Assert
        Assert.NotNull(result.TransactionId);
        Assert.True(result.ChannelResults.ContainsKey("email"));
    }

    [Fact]
    public async Task SendNotificationAsync_WithInAppChannel_SendsInApp()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);
        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "sub123",
            Channels = new List<string> { "in_app" },
            Content = new Dictionary<string, object>
            {
                { "title", "Test Title" },
                { "body", "Test body" }
            }
        };

        // Act
        var result = await provider.SendNotificationAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.ChannelResults.ContainsKey("in_app"));
        Assert.True(result.ChannelResults["in_app"].Success);
    }

    [Fact]
    public async Task TriggerWorkflowAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act
        var result = await provider.TriggerWorkflowAsync("workflow123", "sub123", new { });

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not supported", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task SendBulkEmailAsync_SendsMultipleEmails()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithoutSmtp, _loggerMock.Object);
        var requests = new List<EmailNotificationRequest>
        {
            new() { To = "user1@example.com", Subject = "Test 1", Body = "Body 1" },
            new() { To = "user2@example.com", Subject = "Test 2", Body = "Body 2" },
            new() { To = "user3@example.com", Subject = "Test 3", Body = "Body 3" }
        };

        // Act
        var result = await provider.SendBulkEmailAsync(requests);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Equal(3, result.Results.Count);
    }

    [Fact]
    public async Task SendBulkSmsAsync_ReturnsAllFailed()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);
        var requests = new List<SmsNotificationRequest>
        {
            new() { To = "+1111111111", Message = "Test 1" },
            new() { To = "+2222222222", Message = "Test 2" }
        };

        // Act
        var result = await provider.SendBulkSmsAsync(requests);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(2, result.FailureCount);
    }

    #endregion

    #region Subscriber Management Tests

    [Fact]
    public async Task UpsertSubscriberAsync_ReturnsSubscriberId()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);
        var request = new SubscriberRequest
        {
            SubscriberId = "user123",
            Email = "user@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = await provider.UpsertSubscriberAsync(request);

        // Assert
        Assert.Equal("user123", result);
    }

    [Fact]
    public async Task DeleteSubscriberAsync_CompletesWithoutError()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act & Assert - should not throw
        await provider.DeleteSubscriberAsync("user123");
    }

    [Fact]
    public async Task GetSubscriberPreferencesAsync_ReturnsDefaultPreferences()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act
        var result = await provider.GetSubscriberPreferencesAsync("user123");

        // Assert
        Assert.NotNull(result);
        Assert.False(result!.GlobalOptOut);
        Assert.True(result.ChannelPreferences["email"]);
        Assert.True(result.ChannelPreferences["in_app"]);
        Assert.False(result.ChannelPreferences["sms"]);
        Assert.False(result.ChannelPreferences["push"]);
    }

    [Fact]
    public async Task UpdateSubscriberPreferencesAsync_CompletesWithoutError()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);
        var preferences = new SubscriberPreferences { GlobalOptOut = true };

        // Act & Assert - should not throw
        await provider.UpdateSubscriberPreferencesAsync("user123", preferences);
    }

    #endregion

    #region Delivery Status Tests

    [Fact]
    public async Task GetDeliveryStatusAsync_ReturnsNull()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act
        var result = await provider.GetDeliveryStatusAsync("notification123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessDeliveryWebhookAsync_ReturnsEventWithError()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act
        var result = await provider.ProcessDeliveryWebhookAsync("delivered", "{}");

        // Assert
        Assert.Equal("delivered", result.EventType);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.ContainsKey("error"));
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithSmtpConfigured_ReturnsHealthy()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("BuiltIn", result.ProviderName);
        Assert.Contains("smtp.example.com", result.Message!);
    }

    [Fact]
    public async Task HealthCheckAsync_WithoutSmtpConfigured_ReturnsUnhealthy()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithoutSmtp, _loggerMock.Object);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Equal("BuiltIn", result.ProviderName);
        Assert.Contains("not configured", result.Message!);
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsResponseTime()
    {
        // Arrange
        var provider = new BuiltInNotificationProvider(_configurationWithSmtp, _loggerMock.Object);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.True(result.ResponseTimeMs >= 0);
    }

    #endregion
}
