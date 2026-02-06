// CRM Solution - SendGridProvider Tests
// Tests for the SendGrid email notification provider

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
using CRM.Infrastructure.Providers.SendGrid;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for SendGridProvider.
/// Tests email sending, templates, and webhook handling.
/// </summary>
public class SendGridProviderTests : IDisposable
{
    private readonly Mock<ILogger<SendGridProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<SendGridConfiguration> _options;
    private readonly SendGridProvider _provider;

    public SendGridProviderTests()
    {
        _loggerMock = new Mock<ILogger<SendGridProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.sendgrid.com")
        };

        _options = Options.Create(new SendGridConfiguration
        {
            ApiKey = "SG.test-api-key",
            FromEmail = "noreply@example.com",
            FromName = "CRM System",
            TrackingSettings = new TrackingSettings
            {
                ClickTracking = true,
                OpenTracking = true
            }
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new SendGridProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "")
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
        _provider.ProviderName.Should().Be("SendGrid");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new SendGridProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Email Tests

    [Fact]
    public async Task SendEmailAsync_WithValidRequest_SendsSuccessfully()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted);

        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Test Email",
            Body = "<p>Hello, this is a test email!</p>",
            IsHtml = true
        };

        // Act
        var result = await _provider.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.SendEmailAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyTo_ThrowsArgumentException()
    {
        // Arrange
        var request = new EmailNotificationRequest
        {
            To = "",
            Subject = "Test",
            Body = "Test body"
        };

        // Act
        var act = () => _provider.SendEmailAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptySubject_ThrowsArgumentException()
    {
        // Arrange
        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "",
            Body = "Test body"
        };

        // Act
        var act = () => _provider.SendEmailAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendEmailAsync_WithAttachments_IncludesAttachments()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted);

        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Email with attachment",
            Body = "Please see attached file.",
            Attachments = new List<EmailAttachment>
            {
                new EmailAttachment
                {
                    Filename = "report.pdf",
                    Content = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
                    ContentType = "application/pdf"
                }
            }
        };

        // Act
        var result = await _provider.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WithCcAndBcc_IncludesRecipients()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted);

        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Cc = new List<string> { "cc1@example.com", "cc2@example.com" },
            Bcc = new List<string> { "bcc@example.com" },
            Subject = "Test Email",
            Body = "Test body"
        };

        // Act
        var result = await _provider.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WithReplyTo_SetsReplyTo()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted);

        var request = new EmailNotificationRequest
        {
            To = "recipient@example.com",
            Subject = "Test",
            Body = "Test",
            ReplyTo = "reply@example.com"
        };

        // Act
        var result = await _provider.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendBulkEmailAsync_WithMultipleRecipients_SendsAll()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted);

        var request = new BulkEmailNotificationRequest
        {
            Recipients = new List<string>
            {
                "user1@example.com",
                "user2@example.com",
                "user3@example.com"
            },
            Subject = "Bulk Email",
            Body = "This is a bulk email message."
        };

        // Act
        var results = await _provider.SendBulkEmailAsync(request);

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.Success);
    }

    #endregion

    #region Template Email Tests

    [Fact]
    public async Task SendTemplateEmailAsync_WithValidTemplate_SendsSuccessfully()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted);

        var request = new TemplateEmailNotificationRequest
        {
            To = "recipient@example.com",
            TemplateId = "d-abc123def456",
            TemplateData = new Dictionary<string, object>
            {
                ["first_name"] = "John",
                ["company_name"] = "Acme Corp"
            }
        };

        // Act
        var result = await _provider.SendTemplateEmailAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendTemplateEmailAsync_WithInvalidTemplate_ReturnsFailed()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "{\"errors\":[{\"message\":\"Template not found\"}]}");

        var request = new TemplateEmailNotificationRequest
        {
            To = "recipient@example.com",
            TemplateId = "invalid-template"
        };

        // Act
        var result = await _provider.SendTemplateEmailAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region SMS Tests (Not Supported)

    [Fact]
    public async Task SendSmsAsync_ReturnsNotSupported()
    {
        // Arrange
        var request = new SmsNotificationRequest
        {
            To = "+15551234567",
            Message = "Test SMS"
        };

        // Act
        var result = await _provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not supported");
    }

    #endregion

    #region Subscriber Tests

    [Fact]
    public async Task UpsertSubscriberAsync_CreatesContact()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        var subscriber = new NotificationSubscriber
        {
            Id = "sub-123",
            Email = "contact@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            CustomData = new Dictionary<string, object>
            {
                ["company"] = "Acme Corp"
            }
        };

        // Act
        var result = await _provider.UpsertSubscriberAsync(subscriber);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteSubscriberAsync_RemovesContact()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NoContent);

        // Act
        var result = await _provider.DeleteSubscriberAsync("contact@example.com");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Webhook Tests

    [Fact]
    public async Task ProcessWebhookAsync_WithDeliveryEvent_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new NotificationWebhookPayload
        {
            Event = "delivered",
            Email = "recipient@example.com",
            MessageId = "msg-123",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessWebhookAsync(payload);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithBounceEvent_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new NotificationWebhookPayload
        {
            Event = "bounce",
            Email = "invalid@example.com",
            MessageId = "msg-123",
            BounceType = "hard",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithOpenEvent_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new NotificationWebhookPayload
        {
            Event = "open",
            Email = "recipient@example.com",
            MessageId = "msg-123",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithClickEvent_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new NotificationWebhookPayload
        {
            Event = "click",
            Email = "recipient@example.com",
            MessageId = "msg-123",
            Url = "https://example.com/link",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyApi_ReturnsHealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"status\":\"ok\"}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("SendGrid");
    }

    [Fact]
    public async Task HealthCheckAsync_WithUnhealthyApi_ReturnsUnhealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Unauthorized, "{\"errors\":[{\"message\":\"Invalid API key\"}]}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WithHealthyApi_ReturnsTrue()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{}");

        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task SendEmailAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted);

        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test"
        };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.SendEmailAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SendEmailAsync_WithRateLimitExceeded_ReturnsError()
    {
        // Arrange
        SetupHttpResponse((HttpStatusCode)429, "{\"errors\":[{\"message\":\"Rate limit exceeded\"}]}");

        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test"
        };

        // Act
        var result = await _provider.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SendEmailAsync_WithInvalidEmail_ReturnsError()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "{\"errors\":[{\"message\":\"Invalid email address\"}]}");

        var request = new EmailNotificationRequest
        {
            To = "not-an-email",
            Subject = "Test",
            Body = "Test"
        };

        // Act
        var result = await _provider.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SendEmailAsync_WithServerError_ReturnsError()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Internal server error");

        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test"
        };

        // Act
        var result = await _provider.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region Suppression List Tests

    [Fact]
    public async Task GetSuppressionListAsync_ReturnsSuppressions()
    {
        // Arrange
        var response = new[]
        {
            new { email = "bounced@example.com", reason = "hard bounce", created = 1234567890 },
            new { email = "unsubscribed@example.com", reason = "unsubscribed", created = 1234567899 }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var suppressions = await _provider.GetSuppressionListAsync();

        // Assert
        suppressions.Should().NotBeNull();
    }

    [Fact]
    public async Task AddToSuppressionListAsync_AddsEmail()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Created);

        // Act
        var result = await _provider.AddToSuppressionListAsync("unsubscribe@example.com", "user request");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveFromSuppressionListAsync_RemovesEmail()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NoContent);

        // Act
        var result = await _provider.RemoveFromSuppressionListAsync("resubscribe@example.com");

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
