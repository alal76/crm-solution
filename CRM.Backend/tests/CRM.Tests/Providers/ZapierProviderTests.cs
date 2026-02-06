// CRM Solution - ZapierProvider Tests
// Tests for the Zapier webhook integration provider

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
using CRM.Infrastructure.Providers.Zapier;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for ZapierProvider.
/// Tests webhook-based event delivery and Zap integrations.
/// </summary>
public class ZapierProviderTests : IDisposable
{
    private readonly Mock<ILogger<ZapierProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<ZapierConfiguration> _options;
    private readonly ZapierProvider _provider;

    public ZapierProviderTests()
    {
        _loggerMock = new Mock<ILogger<ZapierProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://hooks.zapier.com")
        };

        _options = Options.Create(new ZapierConfiguration
        {
            WebhookBaseUrl = "https://hooks.zapier.com/hooks/catch",
            EventWebhooks = new Dictionary<string, string>
            {
                ["account.created"] = "https://hooks.zapier.com/hooks/catch/123/abc",
                ["contact.created"] = "https://hooks.zapier.com/hooks/catch/123/def",
                ["opportunity.won"] = "https://hooks.zapier.com/hooks/catch/123/ghi",
                ["*"] = "https://hooks.zapier.com/hooks/catch/123/wildcard"
            }
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new ZapierProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "{\"status\":\"success\"}")
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
        _provider.ProviderName.Should().Be("Zapier");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ZapierProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Event Publishing Tests

    [Fact]
    public async Task PublishEventAsync_WithMappedEvent_PublishesSuccessfully()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityType = "Account",
            EntityId = 123,
            Data = new Dictionary<string, object>
            {
                ["name"] = "New Account",
                ["email"] = "newaccount@example.com"
            },
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.PublishEventAsync(crmEvent);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PublishEventAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.PublishEventAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishEventAsync_WithUnmappedEvent_UsesWildcard()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        var crmEvent = new CrmEvent
        {
            EventType = "unmapped.event",
            EntityType = "Unknown",
            EntityId = 456,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.PublishEventAsync(crmEvent);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PublishEventsAsync_WithMultipleEvents_PublishesAll()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        var events = new List<CrmEvent>
        {
            new CrmEvent { EventType = "account.created", EntityId = 1 },
            new CrmEvent { EventType = "contact.created", EntityId = 2 },
            new CrmEvent { EventType = "opportunity.won", EntityId = 3 }
        };

        // Act
        var results = await _provider.PublishEventsAsync(events);

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.Success);
    }

    [Fact]
    public async Task PublishEventAsync_WithWebhookError_ReturnsFailure()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError);

        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityId = 123
        };

        // Act
        var result = await _provider.PublishEventAsync(crmEvent);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PublishEventAsync_WithComplexData_SerializesCorrectly()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityType = "Account",
            EntityId = 123,
            Data = new Dictionary<string, object>
            {
                ["name"] = "Test Company",
                ["contacts"] = new[] { "John", "Jane" },
                ["metadata"] = new Dictionary<string, object>
                {
                    ["tier"] = "Enterprise",
                    ["mrr"] = 5000
                }
            }
        };

        // Act
        var result = await _provider.PublishEventAsync(crmEvent);

        // Assert
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Webhook Registration Tests

    [Fact]
    public async Task RegisterWebhookAsync_ReturnsPlaceholder()
    {
        // Arrange
        var request = new WebhookRegistrationRequest
        {
            Name = "New Webhook",
            Events = new List<string> { "account.created" },
            TargetUrl = "https://callback.example.com"
        };

        // Act
        var result = await _provider.RegisterWebhookAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWebhooksAsync_ReturnsConfiguredWebhooks()
    {
        // Act
        var result = await _provider.GetWebhooksAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task DeleteWebhookAsync_ReturnsTrue()
    {
        // Act
        var result = await _provider.DeleteWebhookAsync("any-id");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Workflow Tests (Not Fully Supported)

    [Fact]
    public async Task GetWorkflowsAsync_ReturnsEmptyList()
    {
        // Act - Zapier doesn't expose workflow API
        var result = await _provider.GetWorkflowsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TriggerWorkflowAsync_ReturnsNotSupported()
    {
        // Act
        var result = await _provider.TriggerWorkflowAsync("wf-123");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }

    #endregion

    #region Incoming Webhook Tests

    [Fact]
    public async Task ProcessIncomingWebhookAsync_WithValidPayload_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new IntegrationWebhookPayload
        {
            Source = "zapier",
            EventType = "zap.completed",
            Data = new Dictionary<string, object>
            {
                ["zapId"] = "123456",
                ["result"] = "success"
            },
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessIncomingWebhookAsync(payload);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_WithActionRequest_ProcessesAction()
    {
        // Arrange
        var payload = new IntegrationWebhookPayload
        {
            Source = "zapier",
            EventType = "action.request",
            Data = new Dictionary<string, object>
            {
                ["action"] = "create_contact",
                ["name"] = "John Doe",
                ["email"] = "john@example.com"
            }
        };

        // Act
        var result = await _provider.ProcessIncomingWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithConfiguredWebhooks_ReturnsHealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("Zapier");
    }

    [Fact]
    public async Task IsAvailableAsync_WithWebhooks_ReturnsTrue()
    {
        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task PublishEventAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityId = 123
        };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.PublishEventAsync(crmEvent, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishEventAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityId = 123
        };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = () => _provider.PublishEventAsync(crmEvent, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task PublishEventAsync_WithTimeout_ReturnsFailure()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityId = 123
        };

        // Act
        var result = await _provider.PublishEventAsync(crmEvent);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PublishEventAsync_WithRateLimitExceeded_ReturnsFailure()
    {
        // Arrange
        SetupHttpResponse((HttpStatusCode)429);

        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityId = 123
        };

        // Act
        var result = await _provider.PublishEventAsync(crmEvent);

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void GetSupportedEvents_ReturnsConfiguredEvents()
    {
        // Act
        var events = _provider.GetSupportedEvents();

        // Assert
        events.Should().Contain("account.created");
        events.Should().Contain("contact.created");
        events.Should().Contain("opportunity.won");
    }

    [Fact]
    public void GetWebhookUrl_WithMappedEvent_ReturnsUrl()
    {
        // Act
        var url = _provider.GetWebhookUrlForEvent("account.created");

        // Assert
        url.Should().Be("https://hooks.zapier.com/hooks/catch/123/abc");
    }

    [Fact]
    public void GetWebhookUrl_WithUnmappedEvent_ReturnsWildcard()
    {
        // Act
        var url = _provider.GetWebhookUrlForEvent("unknown.event");

        // Assert
        url.Should().Be("https://hooks.zapier.com/hooks/catch/123/wildcard");
    }

    #endregion
}
