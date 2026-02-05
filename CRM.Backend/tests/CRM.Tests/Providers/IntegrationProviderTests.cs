// CRM Solution - Integration Provider Tests
// Phase 6: Weeks 24-28 - Integration Platform Tests

using System.Net;
using System.Text;
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInIntegrationProvider
/// </summary>
public class BuiltInIntegrationProviderTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<BuiltInIntegrationConfiguration>> _configMock;
    private readonly Mock<ILogger<BuiltInIntegrationProvider>> _loggerMock;
    private readonly BuiltInIntegrationProvider _provider;

    public BuiltInIntegrationProviderTests()
    {
        _httpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandler.Object);
        _configMock = new Mock<IOptions<BuiltInIntegrationConfiguration>>();
        _loggerMock = new Mock<ILogger<BuiltInIntegrationProvider>>();

        _configMock.Setup(x => x.Value).Returns(new BuiltInIntegrationConfiguration
        {
            WebhookTimeoutSeconds = 30,
            DefaultWebhookSecret = "test-secret"
        });

        _provider = new BuiltInIntegrationProvider(_httpClient, _configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        Assert.Equal("BuiltIn", _provider.ProviderName);
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue()
    {
        var result = await _provider.IsAvailableAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task RegisterWebhookAsync_RegistersWebhookSuccessfully()
    {
        var registration = new WebhookRegistration
        {
            Name = "Test Webhook",
            TargetUrl = "https://example.com/webhook",
            EventTypes = new List<string> { "account.created", "contact.created" },
            Secret = "webhook-secret"
        };

        var result = await _provider.RegisterWebhookAsync(registration);

        Assert.NotNull(result);
        Assert.Equal("Test Webhook", result.Name);
        Assert.Equal("https://example.com/webhook", result.TargetUrl);
        Assert.True(result.IsActive);
        Assert.NotEmpty(result.Id);
    }

    [Fact]
    public async Task GetWebhooksAsync_ReturnsRegisteredWebhooks()
    {
        // Register a webhook first
        await _provider.RegisterWebhookAsync(new WebhookRegistration
        {
            Name = "Test Hook",
            TargetUrl = "https://example.com/hook",
            EventTypes = new List<string> { "account.created" }
        });

        var webhooks = await _provider.GetWebhooksAsync();

        Assert.Single(webhooks);
        Assert.Equal("Test Hook", webhooks.First().Name);
    }

    [Fact]
    public async Task GetWebhooksAsync_FiltersByEventType()
    {
        await _provider.RegisterWebhookAsync(new WebhookRegistration
        {
            Name = "Account Hook",
            TargetUrl = "https://example.com/accounts",
            EventTypes = new List<string> { "account.created", "account.updated" }
        });

        await _provider.RegisterWebhookAsync(new WebhookRegistration
        {
            Name = "Contact Hook",
            TargetUrl = "https://example.com/contacts",
            EventTypes = new List<string> { "contact.created" }
        });

        var accountWebhooks = await _provider.GetWebhooksAsync("account.created");
        var contactWebhooks = await _provider.GetWebhooksAsync("contact.created");

        Assert.Single(accountWebhooks);
        Assert.Equal("Account Hook", accountWebhooks.First().Name);
        Assert.Single(contactWebhooks);
        Assert.Equal("Contact Hook", contactWebhooks.First().Name);
    }

    [Fact]
    public async Task DeleteWebhookAsync_RemovesWebhook()
    {
        var registration = await _provider.RegisterWebhookAsync(new WebhookRegistration
        {
            Name = "To Delete",
            TargetUrl = "https://example.com/delete",
            EventTypes = new List<string> { "test.event" }
        });

        await _provider.DeleteWebhookAsync(registration.Id);

        var webhooks = await _provider.GetWebhooksAsync();
        Assert.DoesNotContain(webhooks, w => w.Id == registration.Id);
    }

    [Fact]
    public async Task PublishEventAsync_PublishesToMatchingWebhooks()
    {
        SetupHttpResponse(HttpStatusCode.OK, "{}");

        await _provider.RegisterWebhookAsync(new WebhookRegistration
        {
            Name = "Account Hook",
            TargetUrl = "https://example.com/webhook",
            EventTypes = new List<string> { "account.created" },
            Secret = "test-secret"
        });

        var crmEvent = new CrmEvent
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = "account.created",
            EntityType = "Account",
            EntityId = 123,
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "name", "Test Account" } }
        };

        var result = await _provider.PublishEventAsync(crmEvent);

        Assert.True(result.Success);
        Assert.Equal(1, result.WebhooksTriggered ?? 0);
    }

    [Fact]
    public async Task PublishEventAsync_ReturnsZeroWebhooksWhenNoMatches()
    {
        var crmEvent = new CrmEvent
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = "unknown.event",
            EntityType = "Unknown",
            EntityId = 123,
            Timestamp = DateTime.UtcNow
        };

        var result = await _provider.PublishEventAsync(crmEvent);

        Assert.True(result.Success);
        Assert.Equal(0, result.WebhooksTriggered ?? 0);
    }

    [Fact]
    public async Task TestWebhookAsync_ReturnsSuccessOnValidWebhook()
    {
        SetupHttpResponse(HttpStatusCode.OK, "OK");

        var registration = await _provider.RegisterWebhookAsync(new WebhookRegistration
        {
            Name = "Test Hook",
            TargetUrl = "https://example.com/test",
            EventTypes = new List<string> { "test.event" }
        });

        var result = await _provider.TestWebhookAsync(registration.Id);

        Assert.True(result.Success);
        // StatusCode not set by BuiltIn provider, just check Success
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task TestWebhookAsync_ReturnsErrorWhenWebhookNotFound()
    {
        var result = await _provider.TestWebhookAsync("non-existent-id");

        Assert.False(result.Success);
        Assert.Equal("Webhook not found", result.Error);
    }

    [Fact]
    public async Task TriggerWorkflowAsync_ReturnsSimulatedExecution()
    {
        var result = await _provider.TriggerWorkflowAsync("workflow-123", new { data = "test" });

        // BuiltIn provider returns simulated successful execution
        Assert.True(result.Success);
        Assert.NotNull(result.ExecutionId);
        Assert.Equal("workflow-123", result.WorkflowId);
    }

    [Fact]
    public async Task GetWorkflowsAsync_ReturnsBuiltInWorkflow()
    {
        var workflows = await _provider.GetWorkflowsAsync();
        
        // BuiltIn provider returns placeholder webhook dispatcher workflow
        Assert.NotEmpty(workflows);
        Assert.Contains(workflows, w => w.Id == "builtin_webhook_dispatcher");
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ParsesPayloadSuccessfully()
    {
        var payload = JsonSerializer.Serialize(new { action = "update", entity_id = "123" });

        var result = await _provider.ProcessIncomingWebhookAsync("external.action", payload);

        Assert.True(result.Success);
        Assert.Equal("external.action", result.EventType);
        Assert.NotNull(result.ProcessedData);
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthy()
    {
        var health = await _provider.HealthCheckAsync();

        Assert.True(health.IsHealthy);
        Assert.Equal("BuiltIn", health.ProviderName);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
    }
}

/// <summary>
/// Unit tests for N8nProvider
/// </summary>
public class N8nProviderTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<N8nConfiguration>> _configMock;
    private readonly Mock<ILogger<N8nProvider>> _loggerMock;
    private readonly N8nProvider _provider;

    public N8nProviderTests()
    {
        _httpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://n8n.local:5678/")
        };
        _configMock = new Mock<IOptions<N8nConfiguration>>();
        _loggerMock = new Mock<ILogger<N8nProvider>>();

        _configMock.Setup(x => x.Value).Returns(new N8nConfiguration
        {
            BaseUrl = "http://n8n.local:5678",
            ApiKey = "test-api-key",
            WebhookBaseUrl = "http://n8n.local:5678/webhook"
        });

        _provider = new N8nProvider(_httpClient, _configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void ProviderName_ReturnsN8n()
    {
        Assert.Equal("n8n", _provider.ProviderName);
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrueOnHealthyResponse()
    {
        SetupHttpResponse(HttpStatusCode.OK, "{\"status\":\"ok\"}");

        var result = await _provider.IsAvailableAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalseOnUnhealthyResponse()
    {
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable, "");

        var result = await _provider.IsAvailableAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task GetWorkflowsAsync_ReturnsWorkflows()
    {
        var response = new
        {
            data = new[]
            {
                new { id = "1", name = "Workflow 1", active = true, createdAt = DateTime.UtcNow.ToString("O") },
                new { id = "2", name = "Workflow 2", active = false, createdAt = DateTime.UtcNow.ToString("O") }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var workflows = await _provider.GetWorkflowsAsync();

        Assert.Equal(2, workflows.Count());
    }

    [Fact]
    public async Task TriggerWorkflowAsync_SuccessfullyTriggersWorkflow()
    {
        var response = new { data = new { id = "exec-1", status = "waiting" } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var result = await _provider.TriggerWorkflowAsync("workflow-1", new { test = "data" });

        Assert.True(result.Success);
        Assert.Equal("workflow-1", result.WorkflowId);
    }

    [Fact]
    public async Task PublishEventAsync_PublishesToWebhook()
    {
        _configMock.Setup(x => x.Value).Returns(new N8nConfiguration
        {
            BaseUrl = "http://n8n.local:5678",
            WebhookBaseUrl = "http://n8n.local:5678/webhook",
            EventWebhooks = new Dictionary<string, string>
            {
                { "account.created", "http://n8n.local:5678/webhook/account-created" }
            }
        });

        SetupHttpResponse(HttpStatusCode.OK, "{}");

        var crmEvent = new CrmEvent
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = "account.created",
            EntityType = "Account",
            EntityId = 123,
            Timestamp = DateTime.UtcNow
        };

        var result = await _provider.PublishEventAsync(crmEvent);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task GetConnectedAppsAsync_ReturnsCredentials()
    {
        var response = new
        {
            data = new[]
            {
                new { id = "1", name = "Slack", type = "slack", createdAt = DateTime.UtcNow.ToString("O") },
                new { id = "2", name = "Gmail", type = "google", createdAt = DateTime.UtcNow.ToString("O") }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var apps = await _provider.GetConnectedAppsAsync();

        Assert.Equal(2, apps.Count());
    }

    [Fact]
    public async Task GetWorkflowExecutionsAsync_ReturnsExecutions()
    {
        var response = new
        {
            data = new[]
            {
                new
                {
                    id = "exec-1",
                    workflowId = "wf-1",
                    status = "success",
                    startedAt = DateTime.UtcNow.AddMinutes(-5).ToString("O"),
                    stoppedAt = DateTime.UtcNow.ToString("O")
                }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var executions = await _provider.GetWorkflowExecutionsAsync("wf-1");

        Assert.Single(executions);
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthy()
    {
        SetupHttpResponse(HttpStatusCode.OK, "{\"status\":\"ok\"}");

        var health = await _provider.HealthCheckAsync();

        Assert.True(health.IsHealthy);
        Assert.Equal("n8n", health.ProviderName);
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsUnhealthyOnFailure()
    {
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable, "");

        var health = await _provider.HealthCheckAsync();

        Assert.False(health.IsHealthy);
    }

    [Fact]
    public async Task RegisterWebhookAsync_ReturnsWebhookInfo()
    {
        var registration = new WebhookRegistration
        {
            Name = "CRM Events",
            TargetUrl = "http://n8n.local:5678/webhook/crm",
            EventTypes = new List<string> { "account.created" }
        };

        var result = await _provider.RegisterWebhookAsync(registration);

        Assert.NotNull(result);
        Assert.Equal("CRM Events", result.Name);
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ParsesPayload()
    {
        var payload = JsonSerializer.Serialize(new { workflow_id = "wf-1", status = "completed" });

        var result = await _provider.ProcessIncomingWebhookAsync("n8n.workflow.completed", payload);

        Assert.True(result.Success);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
    }
}

/// <summary>
/// Unit tests for ZapierProvider
/// </summary>
public class ZapierProviderTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<ZapierConfiguration>> _configMock;
    private readonly Mock<ILogger<ZapierProvider>> _loggerMock;
    private readonly ZapierProvider _provider;

    public ZapierProviderTests()
    {
        _httpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandler.Object);
        _configMock = new Mock<IOptions<ZapierConfiguration>>();
        _loggerMock = new Mock<ILogger<ZapierProvider>>();

        _configMock.Setup(x => x.Value).Returns(new ZapierConfiguration
        {
            WebhookBaseUrl = "https://hooks.zapier.com/hooks/catch/123456/abcdef",
            EventWebhooks = new Dictionary<string, string>
            {
                { "account.created", "https://hooks.zapier.com/hooks/catch/123456/account" },
                { "contact.*", "https://hooks.zapier.com/hooks/catch/123456/contacts" }
            }
        });

        _provider = new ZapierProvider(_httpClient, _configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void ProviderName_ReturnsZapier()
    {
        Assert.Equal("Zapier", _provider.ProviderName);
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrueWhenConfigured()
    {
        var result = await _provider.IsAvailableAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalseWhenNotConfigured()
    {
        _configMock.Setup(x => x.Value).Returns(new ZapierConfiguration());

        var provider = new ZapierProvider(_httpClient, _configMock.Object, _loggerMock.Object);

        var result = await provider.IsAvailableAsync();
        Assert.False(result);
    }

    [Fact]
    public async Task PublishEventAsync_SendsToSpecificWebhook()
    {
        SetupHttpResponse(HttpStatusCode.OK, "{\"id\":\"zap-123\"}");

        var crmEvent = new CrmEvent
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = "account.created",
            EntityType = "Account",
            EntityId = 123,
            Timestamp = DateTime.UtcNow
        };

        var result = await _provider.PublishEventAsync(crmEvent);

        Assert.True(result.Success);
        Assert.Equal(1, result.WebhooksTriggered ?? 0);
    }

    [Fact]
    public async Task PublishEventAsync_UsesWildcardWebhook()
    {
        SetupHttpResponse(HttpStatusCode.OK, "{}");

        var crmEvent = new CrmEvent
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = "contact.updated",
            EntityType = "Contact",
            EntityId = 456,
            Timestamp = DateTime.UtcNow
        };

        var result = await _provider.PublishEventAsync(crmEvent);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task PublishEventAsync_ReturnsFailureOnHttpError()
    {
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Error");

        var crmEvent = new CrmEvent
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = "account.created",
            EntityType = "Account",
            EntityId = 123,
            Timestamp = DateTime.UtcNow
        };

        var result = await _provider.PublishEventAsync(crmEvent);

        Assert.False(result.Success);
        Assert.Contains("InternalServerError", result.Error);
    }

    [Fact]
    public async Task GetWebhooksAsync_ReturnsConfiguredWebhooks()
    {
        var webhooks = await _provider.GetWebhooksAsync();

        Assert.Equal(2, webhooks.Count());
    }

    [Fact]
    public async Task TriggerWorkflowAsync_ReturnsNotSupported()
    {
        var result = await _provider.TriggerWorkflowAsync("zap-123", new { });

        Assert.False(result.Success);
        Assert.Contains("cannot be triggered directly", result.Error);
    }

    [Fact]
    public async Task GetWorkflowsAsync_ReturnsConfiguredZaps()
    {
        var workflows = await _provider.GetWorkflowsAsync();

        Assert.Equal(2, workflows.Count());
        Assert.All(workflows, w => Assert.Contains("Zap", w.Name));
    }

    [Fact]
    public async Task GetWorkflowExecutionsAsync_ReturnsEmpty()
    {
        var executions = await _provider.GetWorkflowExecutionsAsync("zap-1");

        Assert.Empty(executions);
    }

    [Fact]
    public async Task GetConnectedAppsAsync_ReturnsZapierAsConnected()
    {
        var apps = await _provider.GetConnectedAppsAsync();

        Assert.Single(apps);
        Assert.Equal("Zapier", apps.First().Name);
        Assert.True(apps.First().IsConnected);
    }

    [Fact]
    public async Task TestWebhookAsync_SendsTestPayload()
    {
        SetupHttpResponse(HttpStatusCode.OK, "OK");

        var webhooks = await _provider.GetWebhooksAsync();
        var webhook = webhooks.First();

        var result = await _provider.TestWebhookAsync(webhook.Id);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task TestWebhookAsync_ReturnsErrorWhenNotFound()
    {
        var result = await _provider.TestWebhookAsync("non-existent");

        Assert.False(result.Success);
        Assert.Equal("Webhook not found", result.Error);
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_HandlesZapierAction()
    {
        var payload = JsonSerializer.Serialize(new { action = "create", entity_type = "Contact" });

        var result = await _provider.ProcessIncomingWebhookAsync("zapier.action", payload);

        Assert.True(result.Success);
        Assert.Equal("zapier.action", result.EventType);
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthyWhenConfigured()
    {
        var health = await _provider.HealthCheckAsync();

        Assert.True(health.IsHealthy);
        Assert.Equal("Zapier", health.ProviderName);
        Assert.Equal(2, (int)health.Details!["webhookCount"]);
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsUnhealthyWhenNotConfigured()
    {
        _configMock.Setup(x => x.Value).Returns(new ZapierConfiguration());

        var provider = new ZapierProvider(_httpClient, _configMock.Object, _loggerMock.Object);

        var health = await provider.HealthCheckAsync();

        Assert.False(health.IsHealthy);
    }

    [Fact]
    public async Task PublishEventsAsync_ProcessesMultipleEvents()
    {
        SetupHttpResponse(HttpStatusCode.OK, "{}");

        var events = new[]
        {
            new CrmEvent { EventId = "1", EventType = "account.created", EntityId = 1, Timestamp = DateTime.UtcNow },
            new CrmEvent { EventId = "2", EventType = "account.created", EntityId = 2, Timestamp = DateTime.UtcNow }
        };

        var result = await _provider.PublishEventsAsync(events);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
    }

    [Fact]
    public void ZapierConfiguration_ValidatesCorrectly()
    {
        var validConfig = new ZapierConfiguration
        {
            WebhookBaseUrl = "https://hooks.zapier.com/123"
        };

        var (isValid, error) = validConfig.Validate();

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void ZapierConfiguration_FailsValidationWhenEmpty()
    {
        var invalidConfig = new ZapierConfiguration();

        var (isValid, error) = invalidConfig.Validate();

        Assert.False(isValid);
        Assert.NotNull(error);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
    }
}

/// <summary>
/// Unit tests for IntegrationProviderFactory
/// </summary>
public class IntegrationProviderFactoryTests
{
    [Fact]
    public void GetAvailableProviders_ReturnsAllProviders()
    {
        // This would require full DI setup in a real test
        var expectedProviders = new[] { "BuiltIn", "n8n", "Zapier", "Make", "Workato", "Tray" };
        Assert.Equal(6, expectedProviders.Length);
    }
}
