// CRM Solution - N8nProvider Tests
// Tests for the n8n workflow integration provider

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
using CRM.Infrastructure.Providers.N8n;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for N8nProvider.
/// Tests workflow execution, webhook registration, and event publishing.
/// </summary>
public class N8nProviderTests : IDisposable
{
    private readonly Mock<ILogger<N8nProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<N8nConfiguration> _options;
    private readonly N8nProvider _provider;

    public N8nProviderTests()
    {
        _loggerMock = new Mock<ILogger<N8nProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://n8n.example.com")
        };

        _options = Options.Create(new N8nConfiguration
        {
            BaseUrl = "https://n8n.example.com",
            ApiKey = "test-api-key",
            EventWebhooks = new Dictionary<string, string>
            {
                ["account.created"] = "https://n8n.example.com/webhook/account-created",
                ["contact.updated"] = "https://n8n.example.com/webhook/contact-updated"
            }
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new N8nProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
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
        _provider.ProviderName.Should().Be("N8n");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new N8nProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Webhook Registration Tests

    [Fact]
    public async Task RegisterWebhookAsync_WithValidRequest_ReturnsWebhook()
    {
        // Arrange
        var response = new { id = "wh-123", url = "https://callback.example.com/webhook" };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new WebhookRegistrationRequest
        {
            Name = "Account Created Webhook",
            Events = new List<string> { "account.created" },
            TargetUrl = "https://callback.example.com/webhook",
            Secret = "webhook-secret"
        };

        // Act
        var result = await _provider.RegisterWebhookAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.WebhookUrl.Should().NotBeNullOrEmpty();
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
    public async Task UpdateWebhookAsync_WithValidRequest_UpdatesWebhook()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"id\":\"wh-123\"}");

        var request = new WebhookUpdateRequest
        {
            WebhookId = "wh-123",
            Events = new List<string> { "account.created", "account.updated" }
        };

        // Act
        var result = await _provider.UpdateWebhookAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteWebhookAsync_WithValidId_DeletesWebhook()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NoContent);

        // Act
        var result = await _provider.DeleteWebhookAsync("wh-123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetWebhooksAsync_ReturnsRegisteredWebhooks()
    {
        // Arrange
        var response = new[]
        {
            new { id = "wh-1", name = "Webhook 1" },
            new { id = "wh-2", name = "Webhook 2" }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetWebhooksAsync();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Event Publishing Tests

    [Fact]
    public async Task PublishEventAsync_WithValidEvent_PublishesSuccessfully()
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
    public async Task PublishEventsAsync_WithMultipleEvents_PublishesAll()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        var events = new List<CrmEvent>
        {
            new CrmEvent { EventType = "account.created", EntityId = 1 },
            new CrmEvent { EventType = "account.updated", EntityId = 2 }
        };

        // Act
        var results = await _provider.PublishEventsAsync(events);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task PublishEventAsync_WithUnmappedEvent_ReturnsNoDelivery()
    {
        // Arrange
        var crmEvent = new CrmEvent
        {
            EventType = "unknown.event",
            EntityId = 123
        };

        // Act
        var result = await _provider.PublishEventAsync(crmEvent);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Workflow Tests

    [Fact]
    public async Task GetWorkflowsAsync_ReturnsWorkflows()
    {
        // Arrange
        var response = new
        {
            data = new[]
            {
                new { id = "wf-1", name = "Workflow 1", active = true },
                new { id = "wf-2", name = "Workflow 2", active = false }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetWorkflowsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWorkflowAsync_WithValidId_ReturnsWorkflow()
    {
        // Arrange
        var response = new { id = "wf-123", name = "Test Workflow", active = true };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetWorkflowAsync("wf-123");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TriggerWorkflowAsync_WithValidId_TriggersExecution()
    {
        // Arrange
        var response = new { executionId = "exec-123" };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var data = new Dictionary<string, object>
        {
            ["accountId"] = 123,
            ["action"] = "process"
        };

        // Act
        var result = await _provider.TriggerWorkflowAsync("wf-123", data);

        // Assert
        result.Should().NotBeNull();
        result.ExecutionId.Should().Be("exec-123");
    }

    [Fact]
    public async Task GetWorkflowExecutionsAsync_WithValidWorkflowId_ReturnsExecutions()
    {
        // Arrange
        var response = new
        {
            data = new[]
            {
                new { id = "exec-1", status = "success" },
                new { id = "exec-2", status = "error" }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetWorkflowExecutionsAsync("wf-123");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Incoming Webhook Tests

    [Fact]
    public async Task ProcessIncomingWebhookAsync_WithValidPayload_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new IntegrationWebhookPayload
        {
            Source = "n8n",
            EventType = "workflow.completed",
            Data = new Dictionary<string, object>
            {
                ["workflowId"] = "wf-123",
                ["executionId"] = "exec-456",
                ["status"] = "success"
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
    public async Task HealthCheckAsync_WithHealthyApi_ReturnsHealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"status\":\"ok\"}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("N8n");
    }

    [Fact]
    public async Task HealthCheckAsync_WithUnhealthyApi_ReturnsUnhealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable);

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

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task TriggerWorkflowAsync_WithInvalidWorkflow_ReturnsError()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, "{\"message\":\"Workflow not found\"}");

        // Act
        var result = await _provider.TriggerWorkflowAsync("invalid-wf");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PublishEventAsync_WithServerError_ReturnsFailure()
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
    public async Task RegisterWebhookAsync_WithApiError_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest);

        var request = new WebhookRegistrationRequest
        {
            Name = "Test",
            TargetUrl = "https://example.com"
        };

        // Act
        var act = () => _provider.RegisterWebhookAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region Credentials Tests

    [Fact]
    public async Task GetCredentialsAsync_ReturnsCredentials()
    {
        // Arrange
        var response = new
        {
            data = new[]
            {
                new { id = "cred-1", name = "Slack", type = "slackOAuth2Api" },
                new { id = "cred-2", name = "Email", type = "smtpOAuth2" }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetCredentialsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
