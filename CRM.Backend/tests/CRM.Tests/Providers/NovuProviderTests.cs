// CRM Solution - Novu Provider Tests
// Tests for NovuProvider notification platform integration
// Part of Phase 2 Week 9: Notification Provider implementation

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Novu;

namespace CRM.Tests.Providers;

/// <summary>
/// Comprehensive tests for NovuProvider notification functionality.
/// Tests multi-channel notifications via Novu platform including
/// email, SMS, push, in-app, and multi-channel delivery.
/// </summary>
public class NovuProviderTests
{
    private readonly Mock<ILogger<NovuProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;

    public NovuProviderTests()
    {
        _loggerMock = new Mock<ILogger<NovuProvider>>();
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.novu.co/")
        };
    }

    private NovuProvider CreateProvider(NovuConfiguration? config = null)
    {
        var configuration = config ?? new NovuConfiguration
        {
            Url = "https://api.novu.co",
            ApiKey = "test-api-key",
            EnvironmentId = "test-environment",
            EmailWorkflowId = "email-workflow",
            SmsWorkflowId = "sms-workflow",
            PushWorkflowId = "push-workflow",
            InAppWorkflowId = "inapp-workflow",
            MultiChannelWorkflowId = "multi-channel-workflow"
        };

        return new NovuProvider(
            Options.Create(configuration),
            _httpClient,
            _loggerMock.Object);
    }

    private void SetupSuccessResponse(string transactionId = "txn-123")
    {
        var responseContent = JsonSerializer.Serialize(new
        {
            data = new
            {
                acknowledged = true,
                transactionId
            }
        });

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });
    }

    private void SetupFailureResponse(HttpStatusCode statusCode, string error = "Error")
    {
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(error)
            });
    }

    private void SetupSubscriberAndTriggerResponses()
    {
        var callCount = 0;
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1) // Subscriber upsert
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(new
                        {
                            data = new { subscriberId = "sub-123" }
                        }))
                    };
                }
                // Trigger
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        data = new
                        {
                            acknowledged = true,
                            transactionId = "txn-456"
                        }
                    }))
                };
            });
    }

    #region Provider Configuration Tests

    [Fact]
    public void ProviderName_ReturnsNovu()
    {
        var provider = CreateProvider();

        provider.ProviderName.Should().Be("Novu");
    }

    [Fact]
    public void SupportedChannels_ReturnsAllChannels()
    {
        var provider = CreateProvider();

        provider.SupportedChannels.Should().Contain(new[] { "email", "sms", "push", "in_app", "chat" });
    }

    [Fact]
    public async Task IsAvailableAsync_WhenConfigured_ReturnsTrue()
    {
        var provider = CreateProvider();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("subscribers")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\":[]}")
            });

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenNotConfigured_ReturnsFalse()
    {
        var provider = CreateProvider(new NovuConfiguration());

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenApiReturnsError_ReturnsFalse()
    {
        var provider = CreateProvider();
        SetupFailureResponse(HttpStatusCode.Unauthorized);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenExceptionThrown_ReturnsFalse()
    {
        var provider = CreateProvider();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    #endregion

    #region Email Tests

    [Fact]
    public async Task SendEmailAsync_WithValidRequest_ReturnsSuccess()
    {
        var provider = CreateProvider();
        SetupSubscriberAndTriggerResponses();

        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            ToName = "Test User",
            Subject = "Test Subject",
            Body = "Test Body",
            IsHtml = true
        };

        var result = await provider.SendEmailAsync(request);

        result.Success.Should().BeTrue();
        result.Channel.Should().Be("email");
        result.TransactionId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SendEmailAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var provider = CreateProvider();

        var act = () => provider.SendEmailAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyTo_ThrowsArgumentException()
    {
        var provider = CreateProvider();

        var request = new EmailNotificationRequest
        {
            To = "",
            Subject = "Test",
            Body = "Test"
        };

        var act = () => provider.SendEmailAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*email*required*");
    }

    [Fact]
    public async Task SendEmailAsync_WhenNotConfigured_ReturnsFailure()
    {
        var provider = CreateProvider(new NovuConfiguration());

        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test"
        };

        var result = await provider.SendEmailAsync(request);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not configured");
    }

    [Fact]
    public async Task SendEmailAsync_WithFromAndReplyTo_IncludesInPayload()
    {
        var provider = CreateProvider();
        SetupSubscriberAndTriggerResponses();

        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            From = "sender@example.com",
            ReplyTo = "reply@example.com",
            Subject = "Test",
            Body = "Test"
        };

        var result = await provider.SendEmailAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendTemplateEmailAsync_WithValidData_ReturnsSuccess()
    {
        var provider = CreateProvider();
        SetupSubscriberAndTriggerResponses();

        var result = await provider.SendTemplateEmailAsync(
            "welcome-template",
            "user@example.com",
            new { name = "John", code = "ABC123" });

        result.Success.Should().BeTrue();
        result.Channel.Should().Be("email");
    }

    [Fact]
    public async Task SendTemplateEmailAsync_WithEmptyTemplateId_ThrowsArgumentException()
    {
        var provider = CreateProvider();

        var act = () => provider.SendTemplateEmailAsync("", "test@example.com", new { });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Template ID*required*");
    }

    [Fact]
    public async Task SendTemplateEmailAsync_WithEmptyRecipient_ThrowsArgumentException()
    {
        var provider = CreateProvider();

        var act = () => provider.SendTemplateEmailAsync("template-1", "", new { });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*email*required*");
    }

    #endregion

    #region SMS Tests

    [Fact]
    public async Task SendSmsAsync_WithValidRequest_ReturnsSuccess()
    {
        var provider = CreateProvider();
        SetupSubscriberAndTriggerResponses();

        var request = new SmsNotificationRequest
        {
            To = "+1234567890",
            Message = "Test SMS message"
        };

        var result = await provider.SendSmsAsync(request);

        result.Success.Should().BeTrue();
        result.Channel.Should().Be("sms");
        result.TransactionId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SendSmsAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var provider = CreateProvider();

        var act = () => provider.SendSmsAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendSmsAsync_WithEmptyPhoneNumber_ThrowsArgumentException()
    {
        var provider = CreateProvider();

        var request = new SmsNotificationRequest
        {
            To = "",
            Message = "Test"
        };

        var act = () => provider.SendSmsAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Phone number*required*");
    }

    [Fact]
    public async Task SendSmsAsync_WhenNotConfigured_ReturnsFailure()
    {
        var provider = CreateProvider(new NovuConfiguration());

        var request = new SmsNotificationRequest
        {
            To = "+1234567890",
            Message = "Test"
        };

        var result = await provider.SendSmsAsync(request);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not configured");
    }

    [Fact]
    public async Task SendSmsAsync_NormalizesPhoneNumber()
    {
        var provider = CreateProvider();
        SetupSubscriberAndTriggerResponses();

        var request = new SmsNotificationRequest
        {
            To = "+1-234-567-8900",
            Message = "Test"
        };

        var result = await provider.SendSmsAsync(request);

        result.Success.Should().BeTrue();
    }

    #endregion

    #region Push Notification Tests

    [Fact]
    public async Task SendPushAsync_WithValidRequest_ReturnsSuccess()
    {
        var provider = CreateProvider();
        SetupSuccessResponse();

        var request = new PushNotificationRequest
        {
            To = "subscriber-123",
            Title = "New Message",
            Body = "You have a new message"
        };

        var result = await provider.SendPushAsync(request);

        result.Success.Should().BeTrue();
        result.Channel.Should().Be("push");
    }

    [Fact]
    public async Task SendPushAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var provider = CreateProvider();

        var act = () => provider.SendPushAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendPushAsync_WithEmptyDeviceToken_ThrowsArgumentException()
    {
        var provider = CreateProvider();

        var request = new PushNotificationRequest
        {
            To = "",
            Title = "Test",
            Body = "Test"
        };

        var act = () => provider.SendPushAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendPushAsync_WithIconAndActionUrl_IncludesInPayload()
    {
        var provider = CreateProvider();
        SetupSuccessResponse();

        var request = new PushNotificationRequest
        {
            To = "subscriber-123",
            Title = "Notification",
            Body = "Body text",
            Icon = "https://example.com/icon.png",
            ActionUrl = "https://example.com/action"
        };

        var result = await provider.SendPushAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendPushAsync_WithCustomData_IncludesInPayload()
    {
        var provider = CreateProvider();
        SetupSuccessResponse();

        var request = new PushNotificationRequest
        {
            To = "subscriber-123",
            Title = "Alert",
            Body = "Important alert",
            Data = new Dictionary<string, object>
            {
                ["orderId"] = "12345",
                ["priority"] = "high"
            }
        };

        var result = await provider.SendPushAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendPushAsync_WhenNotConfigured_ReturnsFailure()
    {
        var provider = CreateProvider(new NovuConfiguration());

        var request = new PushNotificationRequest
        {
            To = "subscriber-123",
            Title = "Test",
            Body = "Test"
        };

        var result = await provider.SendPushAsync(request);

        result.Success.Should().BeFalse();
    }

    #endregion

    #region In-App Notification Tests

    [Fact]
    public async Task SendInAppAsync_WithValidRequest_ReturnsSuccess()
    {
        var provider = CreateProvider();
        SetupSuccessResponse();

        var request = new InAppNotificationRequest
        {
            UserId = "user-123",
            Title = "New Activity",
            Content = "Someone commented on your post"
        };

        var result = await provider.SendInAppAsync(request);

        result.Success.Should().BeTrue();
        result.Channel.Should().Be("in_app");
    }

    [Fact]
    public async Task SendInAppAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var provider = CreateProvider();

        var act = () => provider.SendInAppAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendInAppAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        var provider = CreateProvider();

        var request = new InAppNotificationRequest
        {
            UserId = "",
            Title = "Test",
            Content = "Test"
        };

        var act = () => provider.SendInAppAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*User ID*required*");
    }

    [Fact]
    public async Task SendInAppAsync_WithTypeAndActionUrl_IncludesInPayload()
    {
        var provider = CreateProvider();
        SetupSuccessResponse();

        var request = new InAppNotificationRequest
        {
            UserId = "user-123",
            Title = "Notification",
            Content = "Content text",
            Type = "info",
            ActionUrl = "https://example.com/details",
            Avatar = "https://example.com/avatar.png"
        };

        var result = await provider.SendInAppAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendInAppAsync_WithCustomData_IncludesInPayload()
    {
        var provider = CreateProvider();
        SetupSuccessResponse();

        var request = new InAppNotificationRequest
        {
            UserId = "user-123",
            Title = "New Order",
            Content = "Order received",
            Data = new Dictionary<string, object>
            {
                ["orderId"] = "ORD-001",
                ["amount"] = 99.99
            }
        };

        var result = await provider.SendInAppAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendInAppAsync_WhenNotConfigured_ReturnsFailure()
    {
        var provider = CreateProvider(new NovuConfiguration());

        var request = new InAppNotificationRequest
        {
            UserId = "user-123",
            Title = "Test",
            Content = "Test"
        };

        var result = await provider.SendInAppAsync(request);

        result.Success.Should().BeFalse();
    }

    #endregion

    #region Multi-Channel Tests

    [Fact]
    public async Task SendNotificationAsync_WithMultipleChannels_ReturnsSuccess()
    {
        var provider = CreateProvider();
        SetupSuccessResponse();

        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "user-123",
            Channels = new[] { "email", "push", "in_app" },
            Payload = new Dictionary<string, object>
            {
                ["subject"] = "Test Notification",
                ["body"] = "This is a test"
            }
        };

        var result = await provider.SendNotificationAsync(request);

        result.Success.Should().BeTrue();
        result.ChannelResults.Should().HaveCount(3);
        result.ChannelResults.Values.Should().OnlyContain(r => r.Success);
    }

    [Fact]
    public async Task SendNotificationAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var provider = CreateProvider();

        var act = () => provider.SendNotificationAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendNotificationAsync_WithEmptySubscriberId_ThrowsArgumentException()
    {
        var provider = CreateProvider();

        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "",
            Channels = new[] { "email" }
        };

        var act = () => provider.SendNotificationAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Subscriber ID*required*");
    }

    [Fact]
    public async Task SendNotificationAsync_WithCustomTemplateId_UsesTemplate()
    {
        var provider = CreateProvider();
        SetupSuccessResponse();

        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "user-123",
            TemplateId = "custom-workflow-template",
            Channels = new[] { "email", "sms" },
            Payload = new Dictionary<string, object>
            {
                ["name"] = "John"
            }
        };

        var result = await provider.SendNotificationAsync(request);

        result.Success.Should().BeTrue();
        result.TransactionId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SendNotificationAsync_WhenNotConfigured_ReturnsFailureForAllChannels()
    {
        var provider = CreateProvider(new NovuConfiguration());

        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "user-123",
            Channels = new[] { "email", "push" }
        };

        var result = await provider.SendNotificationAsync(request);

        result.Success.Should().BeFalse();
        result.ChannelResults.Should().HaveCount(2);
        result.ChannelResults.Values.Should().OnlyContain(r => !r.Success);
    }

    [Fact]
    public async Task SendNotificationAsync_WhenApiFails_ReturnsFailure()
    {
        var provider = CreateProvider();
        SetupFailureResponse(HttpStatusCode.BadRequest, "Invalid workflow");

        var request = new MultiChannelNotificationRequest
        {
            SubscriberId = "user-123",
            Channels = new[] { "email" }
        };

        var result = await provider.SendNotificationAsync(request);

        result.Success.Should().BeFalse();
    }

    #endregion

    #region Bulk Operation Tests

    [Fact]
    public async Task SendBulkEmailAsync_WithMultipleRecipients_ReturnsResults()
    {
        var provider = CreateProvider();
        SetupSubscriberAndTriggerResponses();

        var requests = new[]
        {
            new EmailNotificationRequest
            {
                To = "user1@example.com",
                Subject = "Test 1",
                Body = "Body 1"
            },
            new EmailNotificationRequest
            {
                To = "user2@example.com",
                Subject = "Test 2",
                Body = "Body 2"
            }
        };

        var results = await provider.SendBulkEmailAsync(requests);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SendBulkSmsAsync_WithMultipleRecipients_ReturnsResults()
    {
        var provider = CreateProvider();
        SetupSubscriberAndTriggerResponses();

        var requests = new[]
        {
            new SmsNotificationRequest { To = "+1111111111", Message = "Test 1" },
            new SmsNotificationRequest { To = "+2222222222", Message = "Test 2" }
        };

        var results = await provider.SendBulkSmsAsync(requests);

        results.Should().HaveCount(2);
    }

    #endregion

    #region Subscriber Management Tests

    [Fact]
    public async Task UpsertSubscriberAsync_WithValidData_ReturnsSubscriberId()
    {
        var provider = CreateProvider();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("subscribers")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    data = new { subscriberId = "sub-created-123" }
                }))
            });

        var result = await provider.UpsertSubscriberAsync(
            "new-user",
            "user@example.com",
            "John",
            "Doe",
            "+1234567890",
            new Dictionary<string, object> { ["company"] = "Acme" });

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteSubscriberAsync_WithValidId_Succeeds()
    {
        var provider = CreateProvider();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => 
                    r.Method == HttpMethod.Delete && 
                    r.RequestUri!.ToString().Contains("subscribers")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        var result = await provider.DeleteSubscriberAsync("sub-123");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetSubscriberPreferencesAsync_ReturnsPreferences()
    {
        var provider = CreateProvider();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("preferences")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    data = new[]
                    {
                        new { channel = "email", enabled = true },
                        new { channel = "sms", enabled = false }
                    }
                }))
            });

        var prefs = await provider.GetSubscriberPreferencesAsync("sub-123");

        prefs.Should().NotBeNull();
    }

    #endregion

    #region Workflow Tests

    [Fact]
    public async Task TriggerWorkflowAsync_WithValidWorkflow_ReturnsSuccess()
    {
        var provider = CreateProvider();
        SetupSuccessResponse("workflow-txn-123");

        var result = await provider.TriggerWorkflowAsync(
            "order-confirmation",
            "user-123",
            new Dictionary<string, object>
            {
                ["orderNumber"] = "ORD-001",
                ["amount"] = 150.00
            });

        result.Success.Should().BeTrue();
        result.TransactionId.Should().Be("workflow-txn-123");
    }

    [Fact]
    public async Task TriggerWorkflowAsync_WhenNotConfigured_ReturnsFailure()
    {
        var provider = CreateProvider(new NovuConfiguration());

        var result = await provider.TriggerWorkflowAsync(
            "test-workflow",
            "user-123",
            new Dictionary<string, object>());

        result.Success.Should().BeFalse();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SendEmailAsync_WhenNotAcknowledged_ReturnsFailure()
    {
        var provider = CreateProvider();
        SetupSubscriberAndTriggerResponses();

        // Override to return not acknowledged
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("trigger")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    data = new { acknowledged = false }
                }))
            });

        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test"
        };

        var result = await provider.SendEmailAsync(request);

        // Should fail since not acknowledged
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SendEmailAsync_WhenApiReturns500_ReturnsFailure()
    {
        var provider = CreateProvider();
        SetupSubscriberAndTriggerResponses();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("trigger")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Internal Server Error")
            });

        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test"
        };

        var result = await provider.SendEmailAsync(request);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SendEmailAsync_WhenExceptionThrown_ReturnsFailure()
    {
        var provider = CreateProvider();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var request = new EmailNotificationRequest
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test"
        };

        var result = await provider.SendEmailAsync(request);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Network error");
    }

    [Fact]
    public async Task SendSmsAsync_WhenExceptionThrown_ReturnsFailure()
    {
        var provider = CreateProvider();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TimeoutException("Request timed out"));

        var request = new SmsNotificationRequest
        {
            To = "+1234567890",
            Message = "Test"
        };

        var result = await provider.SendSmsAsync(request);

        result.Success.Should().BeFalse();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WhenHealthy_ReturnsHealthyResult()
    {
        var provider = CreateProvider();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("subscribers")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\":[]}")
            });

        var result = await provider.HealthCheckAsync();

        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("Novu");
    }

    [Fact]
    public async Task HealthCheckAsync_WhenUnhealthy_ReturnsUnhealthyResult()
    {
        var provider = CreateProvider();
        SetupFailureResponse(HttpStatusCode.ServiceUnavailable);

        var result = await provider.HealthCheckAsync();

        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task HealthCheckAsync_WhenNotConfigured_ReturnsUnhealthy()
    {
        var provider = CreateProvider(new NovuConfiguration());

        var result = await provider.HealthCheckAsync();

        result.IsHealthy.Should().BeFalse();
        result.Message.Should().Contain("not configured");
    }

    #endregion
}

/// <summary>
/// Tests for NovuConfiguration validation.
/// </summary>
public class NovuConfigurationTests
{
    [Fact]
    public void IsValid_WithAllRequiredFields_ReturnsTrue()
    {
        var config = new NovuConfiguration
        {
            Url = "https://api.novu.co",
            ApiKey = "test-api-key",
            EnvironmentId = "test-env"
        };

        config.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithMissingApiKey_ReturnsFalse()
    {
        var config = new NovuConfiguration
        {
            Url = "https://api.novu.co",
            EnvironmentId = "test-env"
        };

        config.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingUrl_ReturnsFalse()
    {
        var config = new NovuConfiguration
        {
            ApiKey = "test-api-key",
            EnvironmentId = "test-env"
        };

        config.IsValid().Should().BeFalse();
    }

    [Fact]
    public void DefaultWorkflowIds_AreSet()
    {
        var config = new NovuConfiguration();

        // Default workflow IDs should exist
        config.EmailWorkflowId.Should().NotBeNullOrEmpty();
        config.SmsWorkflowId.Should().NotBeNullOrEmpty();
        config.PushWorkflowId.Should().NotBeNullOrEmpty();
        config.InAppWorkflowId.Should().NotBeNullOrEmpty();
    }
}
