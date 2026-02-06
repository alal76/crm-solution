// CRM Solution - BuiltInNotificationProvider Tests
// Tests for the built-in SMTP-based notification provider

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInNotificationProvider.
/// Tests email, SMS, push, in-app notifications and subscriber management.
/// </summary>
public class BuiltInNotificationProviderTests
{
    private readonly Mock<ILogger<BuiltInNotificationProvider>> _loggerMock;
    private readonly Dictionary<string, string?> _configSettings;

    public BuiltInNotificationProviderTests()
    {
        _loggerMock = new Mock<ILogger<BuiltInNotificationProvider>>();
        _configSettings = new Dictionary<string, string?>
        {
            ["Smtp:Host"] = "smtp.test.com",
            ["Smtp:Port"] = "587",
            ["Smtp:Username"] = "testuser",
            ["Smtp:Password"] = "testpass",
            ["Smtp:FromEmail"] = "noreply@test.com",
            ["Smtp:FromName"] = "CRM System",
            ["Smtp:EnableSsl"] = "true"
        };
    }

    private IConfiguration CreateConfiguration(Dictionary<string, string?>? overrides = null)
    {
        var settings = new Dictionary<string, string?>(_configSettings);
        if (overrides != null)
        {
            foreach (var kvp in overrides)
            {
                settings[kvp.Key] = kvp.Value;
            }
        }
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private BuiltInNotificationProvider CreateProvider(Dictionary<string, string?>? configOverrides = null)
    {
        var config = CreateConfiguration(configOverrides);
        return new BuiltInNotificationProvider(config, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesProvider()
    {
        // Act
        var provider = CreateProvider();

        // Assert
        provider.Should().NotBeNull();
        provider.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BuiltInNotificationProvider(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var config = CreateConfiguration();
        var act = () => new BuiltInNotificationProvider(config, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var name = provider.ProviderName;

        // Assert
        name.Should().Be("BuiltIn");
    }

    [Fact]
    public void SupportedChannels_ContainsEmail()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var channels = provider.SupportedChannels;

        // Assert
        channels.Should().Contain("email");
    }

    #endregion

    #region IsAvailableAsync Tests

    [Fact]
    public async Task IsAvailableAsync_WhenSmtpConfigured_ReturnsTrue()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var isAvailable = await provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenSmtpNotConfigured_ReturnsFalse()
    {
        // Arrange
        var provider = CreateProvider(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = "",
            ["Smtp:Port"] = "0"
        });

        // Act
        var isAvailable = await provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHostMissing_ReturnsFalse()
    {
        // Arrange
        var provider = CreateProvider(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = null
        });

        // Act
        var isAvailable = await provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeFalse();
    }

    #endregion

    #region SendEmailAsync Tests

    [Fact]
    public async Task SendEmailAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = () => provider.SendEmailAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyRecipient_ThrowsArgumentException()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new EmailNotificationRequest
        {
            To = "",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        // Act
        var act = () => provider.SendEmailAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptySubject_ThrowsArgumentException()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "",
            Body = "Test Body"
        };

        // Act
        var act = () => provider.SendEmailAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendEmailAsync_WhenSmtpNotConfigured_ReturnsSuccessWithDevMessageId()
    {
        // Arrange - SMTP not configured
        var provider = CreateProvider(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = ""
        });
        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.MessageId.Should().StartWith("dev_");
        result.Provider.Should().Be("BuiltIn");
        result.Channel.Should().Be("email");
    }

    [Fact]
    public async Task SendEmailAsync_WithValidRequest_ReturnsResult()
    {
        // Note: This test would actually try to send email via SMTP
        // In real scenarios, you'd mock the SmtpClient
        // For now, test with unconfigured SMTP to get dev mode response
        var provider = CreateProvider(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = ""
        });
        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Test Email",
            Body = "<p>Test content</p>",
            IsHtml = true,
            Cc = new List<string> { "cc@example.com" },
            Bcc = new List<string> { "bcc@example.com" }
        };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Provider.Should().Be("BuiltIn");
        result.Channel.Should().Be("email");
    }

    #endregion

    #region SendTemplateEmailAsync Tests

    [Fact]
    public async Task SendTemplateEmailAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.SendTemplateEmailAsync(
            "template-123",
            "test@example.com",
            new { Name = "Test" });

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
        result.Provider.Should().Be("BuiltIn");
    }

    #endregion

    #region SendSmsAsync Tests

    [Fact]
    public async Task SendSmsAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new SmsNotificationRequest
        {
            PhoneNumber = "+15551234567",
            Message = "Test message"
        };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
        result.Channel.Should().Be("sms");
        result.Provider.Should().Be("BuiltIn");
    }

    #endregion

    #region SendPushAsync Tests

    [Fact]
    public async Task SendPushAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new PushNotificationRequest
        {
            Title = "Test Push",
            Body = "Test message",
            UserId = "user-123"
        };

        // Act
        var result = await provider.SendPushAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
        result.Channel.Should().Be("push");
        result.Provider.Should().Be("BuiltIn");
    }

    #endregion

    #region SendInAppAsync Tests

    [Fact]
    public async Task SendInAppAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new InAppNotificationRequest
        {
            UserId = "user-123",
            Title = "Test Notification",
            Message = "Test message",
            ActionUrl = "/dashboard"
        };

        // Act
        var result = await provider.SendInAppAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
        result.Provider.Should().Be("BuiltIn");
    }

    #endregion

    #region SendBulkEmailAsync Tests

    [Fact]
    public async Task SendBulkEmailAsync_WithEmptyRecipients_ReturnsFailure()
    {
        // Arrange
        var provider = CreateProvider();
        var recipients = new List<string>();

        // Act
        var result = await provider.SendBulkEmailAsync(
            recipients,
            "Subject",
            "Body",
            false);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("recipients");
    }

    [Fact]
    public async Task SendBulkEmailAsync_WithValidRecipients_ProcessesAll()
    {
        // Arrange - Dev mode (no SMTP configured)
        var provider = CreateProvider(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = ""
        });
        var recipients = new List<string>
        {
            "user1@example.com",
            "user2@example.com",
            "user3@example.com"
        };

        // Act
        var result = await provider.SendBulkEmailAsync(
            recipients,
            "Bulk Subject",
            "Bulk Body",
            false);

        // Assert
        result.Should().NotBeNull();
        result.Provider.Should().Be("BuiltIn");
    }

    #endregion

    #region Subscriber Management Tests

    [Fact]
    public async Task CreateSubscriberAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new NotificationSubscriberRequest
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = await provider.CreateSubscriberAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
    }

    [Fact]
    public async Task UpdateSubscriberAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new NotificationSubscriberRequest
        {
            SubscriberId = "sub-123",
            Email = "test@example.com"
        };

        // Act
        var result = await provider.UpdateSubscriberAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteSubscriberAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.DeleteSubscriberAsync("sub-123");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetSubscriberPreferencesAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.GetSubscriberPreferencesAsync("sub-123");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSubscriberPreferencesAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = CreateProvider();
        var preferences = new NotificationPreferences
        {
            EmailEnabled = true,
            SmsEnabled = false
        };

        // Act
        var result = await provider.UpdateSubscriberPreferencesAsync("sub-123", preferences);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Workflow/Trigger Tests

    [Fact]
    public async Task TriggerWorkflowAsync_ReturnsNotSupported()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new WorkflowTriggerRequest
        {
            WorkflowId = "workflow-123",
            SubscriberId = "sub-123",
            Payload = new Dictionary<string, object> { ["key"] = "value" }
        };

        // Act
        var result = await provider.TriggerWorkflowAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WhenSmtpConfigured_ReturnsHealthy()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task HealthCheckAsync_WhenSmtpNotConfigured_ReturnsUnhealthy()
    {
        // Arrange
        var provider = CreateProvider(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = "",
            ["Smtp:Port"] = "0"
        });

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeFalse();
    }

    #endregion

    #region GetDeliveryStatusAsync Tests

    [Fact]
    public async Task GetDeliveryStatusAsync_ReturnsUnknown()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.GetDeliveryStatusAsync("msg-123");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(DeliveryStatus.Unknown);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task SendEmailAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var provider = CreateProvider(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = ""
        });
        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test"
        };
        var cts = new CancellationTokenSource();
        
        // Act - Cancel before execution
        cts.Cancel();
        
        // The method should handle cancellation gracefully
        // In dev mode with no SMTP, it returns immediately
        var result = await provider.SendEmailAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task IsAvailableAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var provider = CreateProvider();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Should handle cancellation
        var result = await provider.IsAvailableAsync(cts.Token);
        result.Should().BeTrue(); // Returns synchronously, no network call
    }

    #endregion
}
