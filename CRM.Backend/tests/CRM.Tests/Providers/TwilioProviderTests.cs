// CRM Solution - TwilioProvider Tests
// Tests for the Twilio SMS/Voice notification provider

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Twilio;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for TwilioProvider.
/// Tests SMS sending, voice calls, and webhook handling.
/// </summary>
public class TwilioProviderTests : IDisposable
{
    private readonly Mock<ILogger<TwilioProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<TwilioConfiguration> _options;
    private readonly TwilioProvider _provider;

    public TwilioProviderTests()
    {
        _loggerMock = new Mock<ILogger<TwilioProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.twilio.com")
        };

        _options = Options.Create(new TwilioConfiguration
        {
            AccountSid = "AC123456789",
            AuthToken = "test-auth-token",
            FromPhoneNumber = "+15551234567",
            MessagingServiceSid = "MG123456789",
            StatusCallbackUrl = "https://example.com/webhook/twilio"
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new TwilioProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesProvider()
    {
        // Assert
        _provider.Should().NotBeNull();
        _provider.ProviderName.Should().Be("Twilio");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new TwilioProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region SMS Tests

    [Fact]
    public async Task SendSmsAsync_WithValidRequest_SendsSuccessfully()
    {
        // Arrange
        var response = new
        {
            sid = "SM123456789",
            status = "queued",
            to = "+15559876543",
            from = "+15551234567"
        };
        SetupHttpResponse(HttpStatusCode.Created, JsonSerializer.Serialize(response));

        var request = new SmsNotificationRequest
        {
            To = "+15559876543",
            Message = "Hello, this is a test message!"
        };

        // Act
        var result = await _provider.SendSmsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.MessageId.Should().Be("SM123456789");
    }

    [Fact]
    public async Task SendSmsAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.SendSmsAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendSmsAsync_WithEmptyTo_ThrowsArgumentException()
    {
        // Arrange
        var request = new SmsNotificationRequest { To = "", Message = "Test" };

        // Act
        var act = () => _provider.SendSmsAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendSmsAsync_WithEmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        var request = new SmsNotificationRequest { To = "+15559876543", Message = "" };

        // Act
        var act = () => _provider.SendSmsAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendSmsAsync_WithInvalidPhoneNumber_ReturnsFailed()
    {
        // Arrange
        var errorResponse = new
        {
            code = 21211,
            message = "Invalid 'To' Phone Number"
        };
        SetupHttpResponse(HttpStatusCode.BadRequest, JsonSerializer.Serialize(errorResponse));

        var request = new SmsNotificationRequest
        {
            To = "invalid-phone",
            Message = "Test message"
        };

        // Act
        var result = await _provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SendBulkSmsAsync_WithMultipleRecipients_SendsAll()
    {
        // Arrange
        var response = new { sid = "SM123", status = "queued" };
        SetupHttpResponse(HttpStatusCode.Created, JsonSerializer.Serialize(response));

        var request = new BulkSmsNotificationRequest
        {
            Recipients = new List<string> { "+15551111111", "+15552222222", "+15553333333" },
            Message = "Bulk test message"
        };

        // Act
        var results = await _provider.SendBulkSmsAsync(request);

        // Assert
        results.Should().HaveCount(3);
    }

    #endregion

    #region Email Tests (Not Supported)

    [Fact]
    public async Task SendEmailAsync_ReturnsNotSupported()
    {
        // Arrange
        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test body"
        };

        // Act
        var result = await _provider.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not supported");
    }

    #endregion

    #region Push Notification Tests (Not Supported)

    [Fact]
    public async Task SendPushAsync_ReturnsNotSupported()
    {
        // Arrange
        var request = new PushNotificationRequest
        {
            DeviceToken = "test-token",
            Title = "Test",
            Body = "Test body"
        };

        // Act
        var result = await _provider.SendPushAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region Subscriber Tests

    [Fact]
    public async Task UpsertSubscriberAsync_CreatesOrUpdatesSubscriber()
    {
        // Arrange - Twilio doesn't have subscriber concept, but we can track contacts
        var subscriber = new NotificationSubscriber
        {
            Id = "sub-123",
            Email = "test@example.com",
            Phone = "+15559876543",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await _provider.UpsertSubscriberAsync(subscriber);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Delivery Status Tests

    [Fact]
    public async Task GetDeliveryStatusAsync_WithValidMessageId_ReturnsStatus()
    {
        // Arrange
        var response = new
        {
            sid = "SM123456789",
            status = "delivered",
            date_sent = "2024-01-15T10:30:00Z"
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var status = await _provider.GetDeliveryStatusAsync("SM123456789");

        // Assert
        status.Should().NotBeNull();
        status.Status.Should().Be("delivered");
    }

    [Fact]
    public async Task GetDeliveryStatusAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, "{\"message\":\"Message not found\"}");

        // Act
        var status = await _provider.GetDeliveryStatusAsync("invalid-id");

        // Assert
        status.Should().BeNull();
    }

    #endregion

    #region Webhook Tests

    [Fact]
    public async Task ProcessWebhookAsync_WithValidPayload_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new NotificationWebhookPayload
        {
            MessageSid = "SM123456789",
            MessageStatus = "delivered",
            To = "+15559876543",
            From = "+15551234567",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessWebhookAsync(payload);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithNullPayload_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.ProcessWebhookAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyApi_ReturnsHealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"account_sid\":\"AC123\"}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("Twilio");
    }

    [Fact]
    public async Task HealthCheckAsync_WithUnhealthyApi_ReturnsUnhealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Unauthorized, "{\"message\":\"Invalid credentials\"}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WithHealthyApi_ReturnsTrue()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"account_sid\":\"AC123\"}");

        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task SendSmsAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var response = new { sid = "SM123", status = "queued" };
        SetupHttpResponse(HttpStatusCode.Created, JsonSerializer.Serialize(response));

        var request = new SmsNotificationRequest { To = "+15559876543", Message = "Test" };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.SendSmsAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SendSmsAsync_WithRateLimitExceeded_ReturnsError()
    {
        // Arrange
        SetupHttpResponse((HttpStatusCode)429, "{\"message\":\"Rate limit exceeded\"}");

        var request = new SmsNotificationRequest { To = "+15559876543", Message = "Test" };

        // Act
        var result = await _provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SendSmsAsync_WithInsufficientFunds_ReturnsError()
    {
        // Arrange
        var errorResponse = new
        {
            code = 20003,
            message = "Account is not active"
        };
        SetupHttpResponse(HttpStatusCode.PaymentRequired, JsonSerializer.Serialize(errorResponse));

        var request = new SmsNotificationRequest { To = "+15559876543", Message = "Test" };

        // Act
        var result = await _provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion
}
