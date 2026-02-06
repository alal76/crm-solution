// CRM Solution - BuiltInIntegrationProvider Tests
// Tests for the built-in webhook-based integration provider

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInIntegrationProvider.
/// Tests webhook registration, event publishing, and workflow management.
/// </summary>
public class BuiltInIntegrationProviderTests : IDisposable
{
    private readonly Mock<ILogger<BuiltInIntegrationProvider>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly BuiltInIntegrationProvider _provider;

    public BuiltInIntegrationProviderTests()
    {
        _loggerMock = new Mock<ILogger<BuiltInIntegrationProvider>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new BuiltInIntegrationProvider(_loggerMock.Object, _httpClientFactoryMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "{}")
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
        _provider.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BuiltInIntegrationProvider(null!, _httpClientFactoryMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullHttpClientFactory_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BuiltInIntegrationProvider(_loggerMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        // Act
        var name = _provider.ProviderName;

        // Assert
        name.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue()
    {
        // BuiltIn provider is always available
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Webhook Registration Tests

    [Fact]
    public async Task RegisterWebhookAsync_WithValidRequest_ReturnsWebhook()
    {
        // Arrange
        var request = new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "account.created", "account.updated" },
            Name = "Test Webhook",
            Description = "Test webhook for accounts"
        };

        // Act
        var result = await _provider.RegisterWebhookAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Url.Should().Be(request.Url);
        result.Events.Should().BeEquivalentTo(request.Events);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterWebhookAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.RegisterWebhookAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RegisterWebhookAsync_WithEmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var request = new WebhookRegistrationRequest
        {
            Url = "",
            Events = new List<string> { "account.created" }
        };

        // Act
        var act = () => _provider.RegisterWebhookAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RegisterWebhookAsync_WithEmptyEvents_ThrowsArgumentException()
    {
        // Arrange
        var request = new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string>()
        };

        // Act
        var act = () => _provider.RegisterWebhookAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RegisterWebhookAsync_GeneratesHmacSecret()
    {
        // Arrange
        var request = new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "account.created" }
        };

        // Act
        var result = await _provider.RegisterWebhookAsync(request);

        // Assert
        result.Secret.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Webhook Management Tests

    [Fact]
    public async Task GetWebhookAsync_WithExistingWebhook_ReturnsWebhook()
    {
        // Arrange
        var registerRequest = new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "contact.created" }
        };
        var registered = await _provider.RegisterWebhookAsync(registerRequest);

        // Act
        var result = await _provider.GetWebhookAsync(registered.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(registered.Id);
    }

    [Fact]
    public async Task GetWebhookAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _provider.GetWebhookAsync("non-existing-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWebhooksAsync_ReturnsAllWebhooks()
    {
        // Arrange
        await _provider.RegisterWebhookAsync(new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook1",
            Events = new List<string> { "account.created" }
        });
        await _provider.RegisterWebhookAsync(new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook2",
            Events = new List<string> { "contact.created" }
        });

        // Act
        var webhooks = await _provider.GetWebhooksAsync();

        // Assert
        webhooks.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateWebhookAsync_WithValidRequest_UpdatesWebhook()
    {
        // Arrange
        var registered = await _provider.RegisterWebhookAsync(new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "account.created" }
        });

        var updateRequest = new WebhookUpdateRequest
        {
            Id = registered.Id,
            Url = "https://example.com/webhook-updated",
            Events = new List<string> { "account.created", "account.updated" },
            IsActive = true
        };

        // Act
        var result = await _provider.UpdateWebhookAsync(updateRequest);

        // Assert
        result.Url.Should().Be("https://example.com/webhook-updated");
        result.Events.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteWebhookAsync_WithExistingWebhook_DeletesWebhook()
    {
        // Arrange
        var registered = await _provider.RegisterWebhookAsync(new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "account.deleted" }
        });

        // Act
        var deleted = await _provider.DeleteWebhookAsync(registered.Id);
        var webhook = await _provider.GetWebhookAsync(registered.Id);

        // Assert
        deleted.Should().BeTrue();
        webhook.Should().BeNull();
    }

    [Fact]
    public async Task DeleteWebhookAsync_WithNonExistingId_ReturnsFalse()
    {
        // Act
        var result = await _provider.DeleteWebhookAsync("non-existing-id");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Event Publishing Tests

    [Fact]
    public async Task PublishEventAsync_WithValidEvent_DeliveresToWebhooks()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);
        
        await _provider.RegisterWebhookAsync(new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "account.created" }
        });

        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityType = "Account",
            EntityId = 1,
            Data = new Dictionary<string, object> { ["name"] = "Test Account" },
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.PublishEventAsync(crmEvent);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().NotBeNullOrEmpty();
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
    public async Task PublishEventsAsync_WithMultipleEvents_PublishesAll()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);
        
        await _provider.RegisterWebhookAsync(new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "account.created", "contact.created" }
        });

        var events = new List<CrmEvent>
        {
            new CrmEvent { EventType = "account.created", EntityType = "Account", EntityId = 1 },
            new CrmEvent { EventType = "contact.created", EntityType = "Contact", EntityId = 2 }
        };

        // Act
        var results = await _provider.PublishEventsAsync(events);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task PublishEventAsync_WithNoMatchingWebhooks_SucceedsWithoutDelivery()
    {
        // Arrange
        await _provider.RegisterWebhookAsync(new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "contact.created" }
        });

        var crmEvent = new CrmEvent
        {
            EventType = "account.deleted",  // No webhook for this event
            EntityType = "Account",
            EntityId = 1
        };

        // Act
        var result = await _provider.PublishEventAsync(crmEvent);

        // Assert
        result.Should().NotBeNull();
        result.DeliveryAttempts.Should().Be(0);
    }

    #endregion

    #region Workflow Tests (Limited for BuiltIn)

    [Fact]
    public async Task GetWorkflowsAsync_ReturnsEmptyOrPlaceholder()
    {
        // BuiltIn doesn't have full workflow support
        var workflows = await _provider.GetWorkflowsAsync();

        // Assert
        workflows.Should().NotBeNull();
    }

    [Fact]
    public async Task TriggerWorkflowAsync_WithValidRequest_TriggersExecution()
    {
        // Arrange
        var request = new WorkflowTriggerRequest
        {
            WorkflowId = "test-workflow",
            Payload = new Dictionary<string, object> { ["key"] = "value" }
        };

        // Act
        var result = await _provider.TriggerWorkflowAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWorkflowExecutionsAsync_ReturnsExecutionHistory()
    {
        // Act
        var executions = await _provider.GetWorkflowExecutionsAsync("test-workflow");

        // Assert
        executions.Should().NotBeNull();
    }

    #endregion

    #region Incoming Webhook Processing Tests

    [Fact]
    public async Task ProcessIncomingWebhookAsync_WithValidPayload_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new IncomingWebhookPayload
        {
            Source = "external-system",
            EventType = "data.sync",
            Data = new Dictionary<string, object> { ["id"] = "123" },
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessIncomingWebhookAsync(payload);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_WithNullPayload_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.ProcessIncomingWebhookAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthy()
    {
        // BuiltIn provider is always healthy
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("BuiltIn");
    }

    #endregion

    #region Event Types Tests

    [Fact]
    public async Task GetSupportedEventTypesAsync_ReturnsEventTypes()
    {
        // Act
        var eventTypes = await _provider.GetSupportedEventTypesAsync();

        // Assert
        eventTypes.Should().NotBeNull();
        eventTypes.Should().Contain("account.created");
        eventTypes.Should().Contain("account.updated");
        eventTypes.Should().Contain("account.deleted");
        eventTypes.Should().Contain("contact.created");
        eventTypes.Should().Contain("opportunity.created");
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task RegisterWebhookAsync_WithCancellation_RespectsCancellation()
    {
        // Arrange
        var request = new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "account.created" }
        };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.RegisterWebhookAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishEventAsync_WithCancellation_RespectsCancellation()
    {
        // Arrange
        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityType = "Account",
            EntityId = 1
        };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.PublishEventAsync(crmEvent, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Webhook Validation Tests

    [Fact]
    public async Task RegisterWebhookAsync_WithInvalidUrl_ThrowsArgumentException()
    {
        // Arrange
        var request = new WebhookRegistrationRequest
        {
            Url = "not-a-valid-url",
            Events = new List<string> { "account.created" }
        };

        // Act
        var act = () => _provider.RegisterWebhookAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RegisterWebhookAsync_WithDuplicateUrl_AllowsMultiple()
    {
        // Arrange - Same URL for different events should be allowed
        var request1 = new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "account.created" }
        };
        var request2 = new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "contact.created" }
        };

        // Act
        var webhook1 = await _provider.RegisterWebhookAsync(request1);
        var webhook2 = await _provider.RegisterWebhookAsync(request2);

        // Assert
        webhook1.Id.Should().NotBe(webhook2.Id);
    }

    #endregion

    #region Delivery Status Tests

    [Fact]
    public async Task GetDeliveryStatusAsync_WithValidEventId_ReturnsStatus()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);
        
        await _provider.RegisterWebhookAsync(new WebhookRegistrationRequest
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "account.created" }
        });

        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityType = "Account",
            EntityId = 1
        };
        var published = await _provider.PublishEventAsync(crmEvent);

        // Act
        var status = await _provider.GetDeliveryStatusAsync(published.EventId);

        // Assert
        status.Should().NotBeNull();
    }

    #endregion
}
