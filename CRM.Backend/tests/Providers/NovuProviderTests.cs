// CRM Solution - Novu Provider Tests
// Phase 2 Week 9: Unit tests for NovuProvider
// Part of the Pluggable Architecture implementation

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Novu;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for NovuProvider.
/// Tests configuration validation, method signatures, and error handling.
/// Uses mocked HttpClient to avoid actual API calls.
/// </summary>
public class NovuProviderTests
{
    private readonly Mock<ILogger<NovuProvider>> _loggerMock;
    private readonly NovuConfiguration _validConfig;
    private readonly NovuConfiguration _invalidConfig;

    public NovuProviderTests()
    {
        _loggerMock = new Mock<ILogger<NovuProvider>>();
        
        _validConfig = new NovuConfiguration
        {
            Url = "https://api.novu.co",
            ApiKey = "test-api-key",
            ApplicationId = "test-app",
            EmailWorkflowId = "crm-email",
            SmsWorkflowId = "crm-sms",
            PushWorkflowId = "crm-push",
            InAppWorkflowId = "crm-inapp",
            MultiChannelWorkflowId = "crm-multi"
        };
        
        _invalidConfig = new NovuConfiguration
        {
            Url = "",
            ApiKey = ""
        };
    }

    private NovuProvider CreateProvider(NovuConfiguration config, HttpMessageHandler? handler = null)
    {
        var options = Options.Create(config);
        var httpClient = handler != null
            ? new HttpClient(handler) { BaseAddress = new Uri(config.Url.TrimEnd('/') + "/") }
            : new HttpClient { BaseAddress = new Uri("https://api.novu.co/") };
        
        return new NovuProvider(options, httpClient, _loggerMock.Object);
    }

    private Mock<HttpMessageHandler> CreateMockHandler(HttpStatusCode statusCode, object? responseContent = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = responseContent != null
                ? JsonContent.Create(responseContent)
                : new StringContent("")
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return handlerMock;
    }

    #region Provider Initialization Tests

    [Fact]
    public void NovuProvider_ShouldHaveCorrectProviderName()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);

        // Act & Assert
        Assert.Equal("Novu", provider.ProviderName);
    }

    [Fact]
    public void NovuProvider_ShouldSupportExpectedChannels()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);

        // Act
        var channels = provider.SupportedChannels.ToList();

        // Assert
        Assert.Contains("email", channels);
        Assert.Contains("sms", channels);
        Assert.Contains("push", channels);
        Assert.Contains("in_app", channels);
        Assert.Contains("chat", channels);
        Assert.Equal(5, channels.Count);
    }

    [Fact]
    public void NovuProvider_WithInvalidConfig_ShouldInitializeWithoutException()
    {
        // Arrange & Act
        var options = Options.Create(_invalidConfig);
        var httpClient = new HttpClient();
        var provider = new NovuProvider(options, httpClient, _loggerMock.Object);

        // Assert - Should not throw, provider handles invalid config gracefully
        Assert.NotNull(provider);
        Assert.Equal("Novu", provider.ProviderName);
    }

    #endregion

    #region Configuration Validation Tests

    [Fact]
    public void NovuConfiguration_IsValid_ShouldReturnTrue_WhenConfigured()
    {
        // Arrange & Act
        var isValid = _validConfig.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void NovuConfiguration_IsValid_ShouldReturnFalse_WhenMissingApiKey()
    {
        // Arrange
        var config = new NovuConfiguration { Url = "https://api.novu.co", ApiKey = "" };

        // Act
        var isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void NovuConfiguration_IsValid_ShouldReturnFalse_WhenMissingUrl()
    {
        // Arrange
        var config = new NovuConfiguration { Url = "", ApiKey = "test-key" };

        // Act
        var isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region IsAvailableAsync Tests

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenNotConfigured()
    {
        // Arrange
        var provider = CreateProvider(_invalidConfig);

        // Act
        var isAvailable = await provider.IsAvailableAsync();

        // Assert
        Assert.False(isAvailable);
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenApiResponds()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new { data = new object[] { } });
        var provider = CreateProvider(_validConfig, handlerMock.Object);

        // Act
        var isAvailable = await provider.IsAvailableAsync();

        // Assert
        Assert.True(isAvailable);
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenApiFails()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.Unauthorized);
        var provider = CreateProvider(_validConfig, handlerMock.Object);

        // Act
        var isAvailable = await provider.IsAvailableAsync();

        // Assert
        Assert.False(isAvailable);
    }

    #endregion

    #region SendEmailAsync Tests

    [Fact]
    public async Task SendEmailAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.SendEmailAsync(null!));
    }

    [Fact]
    public async Task SendEmailAsync_ShouldThrowArgumentException_WhenToIsEmpty()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);
        var request = new EmailNotificationRequest { To = "", Subject = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.SendEmailAsync(request));
    }

    [Fact]
    public async Task SendEmailAsync_ShouldReturnFailure_WhenNotConfigured()
    {
        // Arrange
        var provider = CreateProvider(_invalidConfig);
        var request = new EmailNotificationRequest { To = "test@example.com", Subject = "Test", Body = "Body" };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not configured", result.Error);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldReturnSuccess_WhenApiAcknowledges()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new
        {
            data = new { acknowledged = true, transactionId = "txn-123" }
        });
        var provider = CreateProvider(_validConfig, handlerMock.Object);
        var request = new EmailNotificationRequest { To = "test@example.com", Subject = "Test", Body = "Body" };

        // Act
        var result = await provider.SendEmailAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("txn-123", result.MessageId);
        Assert.Equal("Novu", result.Provider);
        Assert.Equal("email", result.Channel);
    }

    #endregion

    #region SendSmsAsync Tests

    [Fact]
    public async Task SendSmsAsync_ShouldThrowArgumentException_WhenPhoneIsEmpty()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);
        var request = new SmsNotificationRequest { To = "", Message = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.SendSmsAsync(request));
    }

    [Fact]
    public async Task SendSmsAsync_ShouldReturnSuccess_WhenApiAcknowledges()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new
        {
            data = new { acknowledged = true, transactionId = "sms-txn-456" }
        });
        var provider = CreateProvider(_validConfig, handlerMock.Object);
        var request = new SmsNotificationRequest { To = "+1234567890", Message = "Test SMS" };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("sms-txn-456", result.MessageId);
        Assert.Equal("sms", result.Channel);
    }

    #endregion

    #region SendPushAsync Tests

    [Fact]
    public async Task SendPushAsync_ShouldThrowArgumentException_WhenToIsEmpty()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);
        var request = new PushNotificationRequest { To = "", Title = "Test", Body = "Body" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.SendPushAsync(request));
    }

    [Fact]
    public async Task SendPushAsync_ShouldReturnSuccess_WhenApiAcknowledges()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new
        {
            data = new { acknowledged = true, transactionId = "push-txn-789" }
        });
        var provider = CreateProvider(_validConfig, handlerMock.Object);
        var request = new PushNotificationRequest { To = "subscriber-id", Title = "Test", Body = "Body" };

        // Act
        var result = await provider.SendPushAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("push-txn-789", result.MessageId);
        Assert.Equal("push", result.Channel);
    }

    #endregion

    #region SendInAppAsync Tests

    [Fact]
    public async Task SendInAppAsync_ShouldThrowArgumentException_WhenUserIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);
        var request = new InAppNotificationRequest { UserId = "", Title = "Test", Content = "Content" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.SendInAppAsync(request));
    }

    [Fact]
    public async Task SendInAppAsync_ShouldReturnSuccess_WhenApiAcknowledges()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new
        {
            data = new { acknowledged = true, transactionId = "inapp-txn-012" }
        });
        var provider = CreateProvider(_validConfig, handlerMock.Object);
        var request = new InAppNotificationRequest { UserId = "user-123", Title = "Test", Content = "Content" };

        // Act
        var result = await provider.SendInAppAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("inapp-txn-012", result.MessageId);
        Assert.Equal("in_app", result.Channel);
    }

    #endregion

    #region TriggerWorkflowAsync Tests

    [Fact]
    public async Task TriggerWorkflowAsync_ShouldThrowArgumentException_WhenWorkflowIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            provider.TriggerWorkflowAsync("", "subscriber", new { data = "test" }));
    }

    [Fact]
    public async Task TriggerWorkflowAsync_ShouldThrowArgumentException_WhenSubscriberIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            provider.TriggerWorkflowAsync("workflow-id", "", new { data = "test" }));
    }

    [Fact]
    public async Task TriggerWorkflowAsync_ShouldReturnSuccess_WhenApiAcknowledges()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new
        {
            data = new { acknowledged = true, transactionId = "workflow-txn-345" }
        });
        var provider = CreateProvider(_validConfig, handlerMock.Object);

        // Act
        var result = await provider.TriggerWorkflowAsync("test-workflow", "subscriber-123", new { key = "value" });

        // Assert
        Assert.True(result.Success);
        Assert.Equal("workflow-txn-345", result.MessageId);
    }

    #endregion

    #region SendNotificationAsync (Multi-Channel) Tests

    [Fact]
    public async Task SendNotificationAsync_ShouldThrowArgumentException_WhenSubscriberIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);
        var request = new MultiChannelNotificationRequest { SubscriberId = "", Channels = new List<string> { "email" } };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.SendNotificationAsync(request));
    }

    [Fact]
    public async Task SendNotificationAsync_ShouldReturnSuccess_WhenApiAcknowledges()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new
        {
            data = new { acknowledged = true, transactionId = "multi-txn-678" }
        });
        var provider = CreateProvider(_validConfig, handlerMock.Object);
        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "subscriber-123",
            Channels = new List<string> { "email", "in_app" },
            Content = new Dictionary<string, object> { ["message"] = "Test" }
        };

        // Act
        var result = await provider.SendNotificationAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("multi-txn-678", result.TransactionId);
        Assert.Equal(2, result.ChannelResults.Count);
        Assert.True(result.ChannelResults["email"].Success);
        Assert.True(result.ChannelResults["in_app"].Success);
    }

    #endregion

    #region Subscriber Management Tests

    [Fact]
    public async Task UpsertSubscriberAsync_ShouldThrowArgumentException_WhenSubscriberIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);
        var request = new SubscriberRequest { SubscriberId = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.UpsertSubscriberAsync(request));
    }

    [Fact]
    public async Task UpsertSubscriberAsync_ShouldReturnSubscriberId_WhenSuccessful()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new { data = new { subscriberId = "sub-123" } });
        var provider = CreateProvider(_validConfig, handlerMock.Object);
        var request = new SubscriberRequest
        {
            SubscriberId = "sub-123",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = await provider.UpsertSubscriberAsync(request);

        // Assert
        Assert.Equal("sub-123", result);
    }

    [Fact]
    public async Task DeleteSubscriberAsync_ShouldThrowArgumentException_WhenSubscriberIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.DeleteSubscriberAsync(""));
    }

    [Fact]
    public async Task DeleteSubscriberAsync_ShouldNotThrow_WhenSubscriberNotFound()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.NotFound);
        var provider = CreateProvider(_validConfig, handlerMock.Object);

        // Act & Assert - Should not throw for 404
        await provider.DeleteSubscriberAsync("nonexistent-subscriber");
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task SendBulkEmailAsync_ShouldReturnResults_ForEachRequest()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new
        {
            data = new { acknowledged = true, transactionId = "bulk-txn", subscriberId = "test-sub" }
        });
        var provider = CreateProvider(_validConfig, handlerMock.Object);
        var requests = new List<EmailNotificationRequest>
        {
            new() { To = "user1@example.com", Subject = "Test 1", Body = "Body 1" },
            new() { To = "user2@example.com", Subject = "Test 2", Body = "Body 2" },
            new() { To = "user3@example.com", Subject = "Test 3", Body = "Body 3" }
        };

        // Act
        var result = await provider.SendBulkEmailAsync(requests);

        // Assert - verify total count matches input
        Assert.Equal(3, result.TotalCount);
        // Success + Failure should equal Total
        Assert.Equal(result.TotalCount, result.SuccessCount + result.FailureCount);
        // Results list should contain an entry for each request
        Assert.Equal(3, result.Results.Count);
    }

    [Fact]
    public async Task SendBulkSmsAsync_ShouldReturnResults_ForEachRequest()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new
        {
            data = new { acknowledged = true, transactionId = "sms-bulk-txn", subscriberId = "test-sub" }
        });
        var provider = CreateProvider(_validConfig, handlerMock.Object);
        var requests = new List<SmsNotificationRequest>
        {
            new() { To = "+1111111111", Message = "Test 1" },
            new() { To = "+2222222222", Message = "Test 2" }
        };

        // Act
        var result = await provider.SendBulkSmsAsync(requests);

        // Assert - verify total count matches input
        Assert.Equal(2, result.TotalCount);
        // Success + Failure should equal Total
        Assert.Equal(result.TotalCount, result.SuccessCount + result.FailureCount);
        // Results list should contain an entry for each request
        Assert.Equal(2, result.Results.Count);
    }

    #endregion

    #region HealthCheckAsync Tests

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnUnhealthy_WhenNotConfigured()
    {
        // Arrange
        var provider = CreateProvider(_invalidConfig);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Contains("not configured", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthy_WhenApiResponds()
    {
        // Arrange
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, new { data = new object[] { } });
        var provider = CreateProvider(_validConfig, handlerMock.Object);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Contains("accessible", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Novu", result.ProviderName);
    }

    #endregion

    #region ProcessDeliveryWebhookAsync Tests

    [Fact]
    public async Task ProcessDeliveryWebhookAsync_ShouldThrowArgumentException_WhenEventTypeIsEmpty()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            provider.ProcessDeliveryWebhookAsync("", "{}"));
    }

    [Fact]
    public async Task ProcessDeliveryWebhookAsync_ShouldParseWebhookPayload()
    {
        // Arrange
        var provider = CreateProvider(_validConfig);
        var payload = JsonSerializer.Serialize(new
        {
            notificationId = "notif-123",
            subscriberId = "sub-456",
            channel = "email"
        });

        // Act
        var result = await provider.ProcessDeliveryWebhookAsync("delivered", payload);

        // Assert
        Assert.Equal("delivered", result.EventType);
        Assert.Equal("notif-123", result.NotificationId);
        Assert.Equal("sub-456", result.SubscriberId);
        Assert.Equal("email", result.Channel);
    }

    #endregion
}
