// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// MANDATORY: Written after verifying actual source:
//   Class:       N8nProvider
//   Namespace:   CRM.Infrastructure.Providers.Integration
//   Constructor: (HttpClient, IOptions<N8nConfiguration>, ILogger<N8nProvider>)
//   Constructor side effects:
//     - Sets _httpClient.BaseAddress from config.BaseUrl
//     - Adds "X-N8N-API-KEY" default request header from config.ApiKey
//   ProviderName => "n8n"
//   IsAvailableAsync         → GET /healthz
//   PublishEventAsync        → POST to EventWebhooks[eventType] (no http call if no mapping found)
//   RegisterWebhookAsync     → returns info, NO http call
//   GetWebhooksAsync         → calls GetWorkflowsAsync (GET /api/v1/workflows), filters webhook triggers
//   UpdateWebhookAsync       → logs only, NO http call
//   DeleteWebhookAsync       → logs only, NO http call
//   TestWebhookAsync         → POST {WebhookBaseUrl}/webhook/{webhookId}
//   TriggerWorkflowAsync     → POST {WebhookBaseUrl}/webhook/{id} first, then POST /api/v1/workflows/{id}/execute
//   GetWorkflowsAsync        → GET /api/v1/workflows
//   GetWorkflowExecutionsAsync → GET /api/v1/executions?workflowId=...&limit=...
//   GetConnectedAppsAsync    → GET /api/v1/credentials
//   TestConnectionAsync      → POST /api/v1/credentials/{id}/test
//   ProcessIncomingWebhookAsync → JSON parse only
//   HealthCheckAsync         → GET /healthz + GetWorkflowsAsync

using System.Net;
using System.Text;
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Integration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="N8nProvider"/>.
/// Uses a URL-routed mock handler so each endpoint can return realistic JSON.
/// No real HTTP calls are made.
/// </summary>
public class N8nProviderTests
{
    // ── JSON constants ───────────────────────────────────────────────────────

    private const string EmptyWorkflowsJson = """{"data":[]}""";
    private const string SingleWebhookWorkflowJson = """{"data":[{"id":"wf-1","name":"My Webhook Workflow","active":true,"nodes":[{"type":"n8n-nodes-base.webhook","name":"Webhook Trigger"}]}]}""";
    private const string NonWebhookWorkflowJson = """{"data":[{"id":"wf-2","name":"Manual Workflow","active":true,"nodes":[{"type":"n8n-nodes-base.set","name":"Set"}]}]}""";
    private const string ExecutionsJson = """{"data":[{"id":"exec-1","startedAt":"2026-01-01T10:00:00Z","stoppedAt":"2026-01-01T10:00:01Z","finished":true,"mode":"webhook"},{"id":"exec-2","startedAt":"2026-01-01T11:00:00Z","stoppedAt":"2026-01-01T11:00:02Z","finished":true,"mode":"webhook"}]}""";
    private const string CredentialsJson = """{"data":[{"id":"cred-1","name":"Slack","type":"slackApi","createdAt":"2026-01-01T00:00:00Z"}]}""";
    private const string TriggerResultJson = """{"data":{"id":"exec-99","status":"running","data":{}}}""";

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (N8nProvider provider, N8nRoutedMockHandler handler) CreateProvider(
        N8nConfiguration? config = null,
        string? healthzResponse = "OK",
        HttpStatusCode healthzStatus = HttpStatusCode.OK,
        string workflowsJson = EmptyWorkflowsJson,
        string executionsJson = ExecutionsJson,
        string credentialsJson = CredentialsJson,
        string postResponseBody = """{"status":"success"}""",
        HttpStatusCode postStatus = HttpStatusCode.OK,
        string credentialTestBody = """{"status":"success"}""",
        HttpStatusCode credentialTestStatus = HttpStatusCode.OK)
    {
        var handlerObj = new N8nRoutedMockHandler(
            healthzStatus, healthzResponse ?? "OK",
            workflowsJson,
            executionsJson,
            credentialsJson,
            postResponseBody, postStatus,
            credentialTestBody, credentialTestStatus);

        var httpClient = new HttpClient(handlerObj);

        var effectiveConfig = config ?? DefaultConfig();
        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<N8nProvider>>();
        var provider = new N8nProvider(httpClient, options, logger.Object);

        return (provider, handlerObj);
    }

    private static N8nConfiguration DefaultConfig(
        Dictionary<string, string>? eventWebhooks = null) =>
        new()
        {
            BaseUrl = "http://n8n.test:5678",
            ApiKey = "test-n8n-api-key",
            WebhookBaseUrl = "http://n8n.test:5678",
            EventWebhooks = eventWebhooks
        };

    private static CrmEvent MakeEvent(string eventType = "account.created") =>
        new()
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = eventType,
            EntityType = "account",
            EntityId = 1,
            Timestamp = DateTime.UtcNow
        };

    // ── ProviderName ─────────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_Returns_n8n()
    {
        var (provider, _) = CreateProvider();
        provider.ProviderName.Should().Be("n8n");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenHealthzReturns200()
    {
        var (provider, handler) = CreateProvider(healthzStatus: HttpStatusCode.OK);

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
        handler.HealthzCallCount.Should().Be(1);
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenHealthzReturns503()
    {
        var (provider, _) = CreateProvider(healthzStatus: HttpStatusCode.ServiceUnavailable);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenHttpThrows()
    {
        // Simulate network failure by using a throwing handler
        var throwingHandler = new ThrowingHttpHandler();
        var httpClient = new HttpClient(throwingHandler);
        var options = Options.Create(DefaultConfig());
        var logger = new Mock<ILogger<N8nProvider>>();
        var provider = new N8nProvider(httpClient, options, logger.Object);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    // ── Constructor behaviour ────────────────────────────────────────────────

    [Fact]
    public void Constructor_SetsBaseAddress_FromConfig()
    {
        var handler = new N8nRoutedMockHandler(HttpStatusCode.OK, "OK", EmptyWorkflowsJson, ExecutionsJson, CredentialsJson, "{}", HttpStatusCode.OK, "{}", HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var config = new N8nConfiguration { BaseUrl = "http://custom-n8n-host:9999", ApiKey = "key" };
        var options = Options.Create(config);
        var logger = new Mock<ILogger<N8nProvider>>();

        _ = new N8nProvider(httpClient, options, logger.Object);

        httpClient.BaseAddress.Should().Be(new Uri("http://custom-n8n-host:9999"));
    }

    // ── PublishEventAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task PublishEventAsync_ReturnsSuccessWithZeroWebhooks_WhenNoEventWebhookConfigured()
    {
        // No EventWebhooks configured → no HTTP call, returns success with 0 triggered
        var config = DefaultConfig(eventWebhooks: null);
        var (provider, handler) = CreateProvider(config: config);

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(0);
        handler.PostCallCount.Should().Be(0);
    }

    [Fact]
    public async Task PublishEventAsync_PostsToConfiguredWebhookUrl_WhenEventMappingExists()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "http://n8n.test:5678/webhook/acc-hook" }
        });
        var (provider, handler) = CreateProvider(config: config, postStatus: HttpStatusCode.OK);

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(1);
        result.EventId.Should().NotBeNullOrEmpty();
        handler.LastPostUrl.Should().Be("http://n8n.test:5678/webhook/acc-hook");
    }

    [Fact]
    public async Task PublishEventAsync_UsesWildcardMapping_WhenExactMappingMissing()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.*", "http://n8n.test:5678/webhook/account-wildcard" }
        });
        var (provider, handler) = CreateProvider(config: config, postStatus: HttpStatusCode.OK);

        var result = await provider.PublishEventAsync(MakeEvent("account.updated"));

        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(1);
        handler.LastPostUrl.Should().Be("http://n8n.test:5678/webhook/account-wildcard");
    }

    [Fact]
    public async Task PublishEventAsync_UsesDefaultMapping_WhenNoSpecificOrWildcardMatch()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "*", "http://n8n.test:5678/webhook/default" }
        });
        var (provider, handler) = CreateProvider(config: config, postStatus: HttpStatusCode.OK);

        var result = await provider.PublishEventAsync(MakeEvent("opportunity.won"));

        result.Success.Should().BeTrue();
        handler.LastPostUrl.Should().Be("http://n8n.test:5678/webhook/default");
    }

    [Fact]
    public async Task PublishEventAsync_ReturnsFailure_WhenN8nReturnsError()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "http://n8n.test:5678/webhook/acc-hook" }
        });
        var (provider, _) = CreateProvider(config: config, postStatus: HttpStatusCode.InternalServerError, postResponseBody: "error");

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Success.Should().BeFalse();
        result.WebhooksTriggered.Should().Be(0);
        result.Error.Should().NotBeNullOrEmpty();
    }

    // ── PublishEventsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task PublishEventsAsync_AggregatesResults_ForMultipleEvents()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "http://n8n.test:5678/webhook/acc-hook" }
        });
        var (provider, handler) = CreateProvider(config: config, postStatus: HttpStatusCode.OK);

        var events = new[]
        {
            MakeEvent("account.created"),
            MakeEvent("account.created"),
            MakeEvent("opportunity.won")   // No mapping → success with 0 triggers
        };

        var batch = await provider.PublishEventsAsync(events);

        batch.TotalCount.Should().Be(3);
        batch.SuccessCount.Should().Be(3);
        batch.FailureCount.Should().Be(0);
        handler.PostCallCount.Should().Be(2, "only 2 events have a configured webhook mapping");
    }

    // ── RegisterWebhookAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task RegisterWebhookAsync_ReturnsWebhookInfo_WithoutMakingHttpCall()
    {
        var (provider, handler) = CreateProvider();

        var result = await provider.RegisterWebhookAsync(new WebhookRegistration
        {
            Name = "My n8n Hook",
            TargetUrl = "https://customer.example.com/recv",
            EventTypes = new List<string> { "account.created" }
        });

        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Name.Should().Be("My n8n Hook");
        result.TargetUrl.Should().Be("https://customer.example.com/recv");
        result.IsActive.Should().BeTrue();
        handler.PostCallCount.Should().Be(0, "RegisterWebhookAsync should not make HTTP calls for n8n");
    }

    // ── GetWebhooksAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetWebhooksAsync_ReturnsEmpty_WhenNoWorkflowsHaveWebhookTrigger()
    {
        var (provider, _) = CreateProvider(workflowsJson: NonWebhookWorkflowJson);

        var webhooks = await provider.GetWebhooksAsync();

        webhooks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWebhooksAsync_ReturnsWebhookInfo_WhenWorkflowHasWebhookTrigger()
    {
        var (provider, _) = CreateProvider(workflowsJson: SingleWebhookWorkflowJson);

        var webhooks = (await provider.GetWebhooksAsync()).ToList();

        webhooks.Should().HaveCount(1);
        webhooks[0].Id.Should().Be("wf-1");
        webhooks[0].Name.Should().Be("My Webhook Workflow");
        webhooks[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetWebhooksAsync_FiltersCorrectly_ByEventType()
    {
        var (provider, _) = CreateProvider(workflowsJson: SingleWebhookWorkflowJson);

        // n8n webhooks are mapped to event type "custom" by default
        var webhooksForCustom = (await provider.GetWebhooksAsync("custom")).ToList();
        var webhooksForAccount = (await provider.GetWebhooksAsync("account.created")).ToList();

        webhooksForCustom.Should().HaveCount(1);
        webhooksForAccount.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWebhooksAsync_ReturnsEmpty_WhenWorkflowApiFails()
    {
        // Override workflows to return a 500 error so GetWorkflowsAsync returns []
        var (provider, _) = CreateProvider(workflowsJson: null!);

        var webhooks = await provider.GetWebhooksAsync();

        // Should not throw, just return empty
        webhooks.Should().BeEmpty();
    }

    // ── UpdateWebhookAsync / DeleteWebhookAsync ───────────────────────────────

    [Fact]
    public async Task UpdateWebhookAsync_DoesNotThrow_AndMakesNoHttpCalls()
    {
        var (provider, handler) = CreateProvider();
        var act = async () => await provider.UpdateWebhookAsync("wf-1", new WebhookRegistration
        {
            Name = "Updated",
            TargetUrl = "https://new.url",
            EventTypes = new List<string> { "contact.updated" }
        });

        await act.Should().NotThrowAsync();
        handler.PostCallCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteWebhookAsync_DoesNotThrow_AndMakesNoHttpCalls()
    {
        var (provider, handler) = CreateProvider();
        var act = async () => await provider.DeleteWebhookAsync("wf-1");

        await act.Should().NotThrowAsync();
        handler.PostCallCount.Should().Be(0);
    }

    // ── TestWebhookAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task TestWebhookAsync_PostsToWebhookUrl_AndReturnsSuccess()
    {
        var config = new N8nConfiguration
        {
            BaseUrl = "http://n8n.test:5678",
            ApiKey = "key",
            WebhookBaseUrl = "http://n8n.test:5678"
        };
        var (provider, handler) = CreateProvider(config: config, postStatus: HttpStatusCode.OK);

        var result = await provider.TestWebhookAsync("my-workflow-id");

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.ResponseTimeMs.Should().BeGreaterThanOrEqualTo(0);
        handler.LastPostUrl.Should().Contain("my-workflow-id");
    }

    [Fact]
    public async Task TestWebhookAsync_ReturnsFailure_WhenHttpReturns500()
    {
        var (provider, _) = CreateProvider(postStatus: HttpStatusCode.InternalServerError);

        var result = await provider.TestWebhookAsync("my-workflow-id");

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Error.Should().Contain("InternalServerError");
    }

    // ── TriggerWorkflowAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task TriggerWorkflowAsync_ReturnsSuccess_WhenWebhookEndpointResponds()
    {
        // WebhookBaseUrl is configured → POST {WebhookBaseUrl}/webhook/{workflowId}
        var config = new N8nConfiguration
        {
            BaseUrl = "http://n8n.test:5678",
            ApiKey = "key",
            WebhookBaseUrl = "http://n8n.test:5678"
        };
        var (provider, handler) = CreateProvider(
            config: config,
            postStatus: HttpStatusCode.OK,
            postResponseBody: TriggerResultJson);

        var result = await provider.TriggerWorkflowAsync("my-workflow", new { key = "value" });

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.WorkflowId.Should().Be("my-workflow");
        handler.PostCallCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task TriggerWorkflowAsync_FallsBackToApiEndpoint_WhenWebhookFails()
    {
        // The first POST (webhook) fails → should fall back to /api/v1/workflows/{id}/execute
        var config = new N8nConfiguration
        {
            BaseUrl = "http://n8n.test:5678",
            ApiKey = "key",
            WebhookBaseUrl = "http://n8n.test:5678"
        };

        // Use a handler that returns 404 for webhook calls but 200 for API calls
        var twoPhaseHandler = new N8nTwoPhaseHandler(
            firstStatus: HttpStatusCode.NotFound,
            secondStatus: HttpStatusCode.OK,
            secondBody: TriggerResultJson);

        var httpClient = new HttpClient(twoPhaseHandler);
        var options = Options.Create(config);
        var logger = new Mock<ILogger<N8nProvider>>();
        var provider = new N8nProvider(httpClient, options, logger.Object);

        var result = await provider.TriggerWorkflowAsync("my-workflow", new { key = "value" });

        result.Success.Should().BeTrue();
        twoPhaseHandler.CallCount.Should().Be(2, "first call fails (webhook), second succeeds (API)");
    }

    [Fact]
    public async Task TriggerWorkflowAsync_ReturnsFailure_WhenBothEndpointsFail()
    {
        var config = new N8nConfiguration
        {
            BaseUrl = "http://n8n.test:5678",
            ApiKey = "key",
            WebhookBaseUrl = "http://n8n.test:5678"
        };
        var (provider, _) = CreateProvider(
            config: config,
            postStatus: HttpStatusCode.InternalServerError,
            postResponseBody: "error");

        var result = await provider.TriggerWorkflowAsync("my-workflow", new { key = "value" });

        result.Success.Should().BeFalse();
        result.WorkflowId.Should().Be("my-workflow");
        result.Error.Should().NotBeNullOrEmpty();
    }

    // ── GetWorkflowsAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowsAsync_ReturnsEmpty_WhenApiFails()
    {
        // Use a 500-returning handler
        var (provider, _) = CreateProvider(workflowsJson: null!);

        var workflows = await provider.GetWorkflowsAsync();

        workflows.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorkflowsAsync_MapsWorkflowPropertiesCorrectly()
    {
        var (provider, _) = CreateProvider(workflowsJson: SingleWebhookWorkflowJson);

        var workflows = (await provider.GetWorkflowsAsync()).ToList();

        workflows.Should().HaveCount(1);
        workflows[0].Id.Should().Be("wf-1");
        workflows[0].Name.Should().Be("My Webhook Workflow");
        workflows[0].IsActive.Should().BeTrue();
        workflows[0].TriggerType.Should().Be("webhook");
    }

    // ── GetWorkflowExecutionsAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowExecutionsAsync_ReturnsExecutions_WhenApiSucceeds()
    {
        var (provider, _) = CreateProvider(executionsJson: ExecutionsJson);

        var executions = (await provider.GetWorkflowExecutionsAsync("wf-1", limit: 10)).ToList();

        executions.Should().HaveCount(2);
        executions[0].ExecutionId.Should().Be("exec-1");
        executions[0].WorkflowId.Should().Be("wf-1");
    }

    [Fact]
    public async Task GetWorkflowExecutionsAsync_ReturnsEmpty_WhenApiFails()
    {
        var (provider, _) = CreateProvider(executionsJson: null!);

        var executions = await provider.GetWorkflowExecutionsAsync("wf-1");

        executions.Should().BeEmpty();
    }

    // ── GetConnectedAppsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetConnectedAppsAsync_ReturnsCredentials_WhenApiSucceeds()
    {
        var (provider, _) = CreateProvider(credentialsJson: CredentialsJson);

        var apps = (await provider.GetConnectedAppsAsync()).ToList();

        apps.Should().HaveCount(1);
        apps[0].Id.Should().Be("cred-1");
        apps[0].Name.Should().Be("Slack");
        apps[0].Type.Should().Be("slackApi");
        apps[0].IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task GetConnectedAppsAsync_ReturnsEmpty_WhenApiFails()
    {
        var (provider, _) = CreateProvider(credentialsJson: null!);

        var apps = await provider.GetConnectedAppsAsync();

        apps.Should().BeEmpty();
    }

    // ── TestConnectionAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task TestConnectionAsync_ReturnsSuccess_WhenApiReturns200()
    {
        var (provider, _) = CreateProvider(credentialTestStatus: HttpStatusCode.OK);

        var result = await provider.TestConnectionAsync("cred-1");

        result.Success.Should().BeTrue();
        result.ConnectionId.Should().Be("cred-1");
        result.Message.Should().Contain("success");
    }

    [Fact]
    public async Task TestConnectionAsync_ReturnsFailure_WhenApiReturns500()
    {
        var (provider, _) = CreateProvider(credentialTestStatus: HttpStatusCode.InternalServerError);

        var result = await provider.TestConnectionAsync("cred-1");

        result.Success.Should().BeFalse();
        result.ConnectionId.Should().Be("cred-1");
    }

    // ── ProcessIncomingWebhookAsync ──────────────────────────────────────────

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsSuccess_WithCallbackAction()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.ProcessIncomingWebhookAsync("execution.finished", """{"workflowId":"wf-1","status":"success"}""");

        result.Success.Should().BeTrue();
        result.EventType.Should().Be("execution.finished");
        result.Action.Should().Be("callback");
        result.ProcessedData.Should().ContainKey("workflowId");
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsFailure_WhenPayloadIsInvalidJson()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.ProcessIncomingWebhookAsync("execution.finished", "NOT_JSON");

        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    // ── HealthCheckAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthy_WhenN8nResponds200()
    {
        var (provider, _) = CreateProvider(healthzStatus: HttpStatusCode.OK, workflowsJson: SingleWebhookWorkflowJson);

        var health = await provider.HealthCheckAsync();

        health.IsHealthy.Should().BeTrue();
        health.ProviderName.Should().Be("n8n");
        health.Message.Should().Contain("healthy");
        health.Details.Should().ContainKey("baseUrl");
        health.Details.Should().ContainKey("activeWorkflows");
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsUnhealthy_WhenHealthzFails()
    {
        var (provider, _) = CreateProvider(healthzStatus: HttpStatusCode.ServiceUnavailable);

        var health = await provider.HealthCheckAsync();

        health.IsHealthy.Should().BeFalse();
        health.ProviderName.Should().Be("n8n");
        health.Message.Should().Contain("failed");
    }
}

// ── Internal mock handler helpers ────────────────────────────────────────────

/// <summary>
/// Routes requests to different JSON responses based on URL patterns.
/// Tracks calls for assertion.
/// </summary>
internal class N8nRoutedMockHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _healthzStatus;
    private readonly string _healthzBody;
    private readonly string? _workflowsJson;
    private readonly string? _executionsJson;
    private readonly string? _credentialsJson;
    private readonly string _postBody;
    private readonly HttpStatusCode _postStatus;
    private readonly string _credTestBody;
    private readonly HttpStatusCode _credTestStatus;

    public int HealthzCallCount { get; private set; }
    public int PostCallCount { get; private set; }
    public string? LastPostUrl { get; private set; }

    public N8nRoutedMockHandler(
        HttpStatusCode healthzStatus, string healthzBody,
        string? workflowsJson,
        string? executionsJson,
        string? credentialsJson,
        string postBody, HttpStatusCode postStatus,
        string credTestBody, HttpStatusCode credTestStatus)
    {
        _healthzStatus = healthzStatus;
        _healthzBody = healthzBody;
        _workflowsJson = workflowsJson;
        _executionsJson = executionsJson;
        _credentialsJson = credentialsJson;
        _postBody = postBody;
        _postStatus = postStatus;
        _credTestBody = credTestBody;
        _credTestStatus = credTestStatus;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? "";

        if (request.Method == HttpMethod.Get)
        {
            if (url.Contains("/healthz"))
            {
                HealthzCallCount++;
                return Respond(_healthzStatus, _healthzBody, "text/plain");
            }

            if (url.Contains("/api/v1/workflows") && !url.Contains("executions"))
            {
                if (_workflowsJson == null)
                {
                    return Respond(HttpStatusCode.InternalServerError, "error");
                }
                return Respond(HttpStatusCode.OK, _workflowsJson);
            }

            if (url.Contains("/api/v1/executions"))
            {
                if (_executionsJson == null)
                {
                    return Respond(HttpStatusCode.InternalServerError, "error");
                }
                return Respond(HttpStatusCode.OK, _executionsJson);
            }

            if (url.Contains("/api/v1/credentials") && !url.Contains("/test"))
            {
                if (_credentialsJson == null)
                {
                    return Respond(HttpStatusCode.InternalServerError, "error");
                }
                return Respond(HttpStatusCode.OK, _credentialsJson);
            }
        }

        if (request.Method == HttpMethod.Post)
        {
            PostCallCount++;
            LastPostUrl = url;

            if (url.Contains("/api/v1/credentials") && url.Contains("/test"))
            {
                return Respond(_credTestStatus, _credTestBody);
            }

            return Respond(_postStatus, _postBody);
        }

        return Respond(HttpStatusCode.NotFound, "not found");
    }

    private static Task<HttpResponseMessage> Respond(
        HttpStatusCode status,
        string body,
        string contentType = "application/json") =>
        Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, contentType)
        });
}

/// <summary>
/// Returns different status codes for successive calls (first, then second).
/// Used to test TriggerWorkflowAsync fallback behaviour.
/// </summary>
internal class N8nTwoPhaseHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _firstStatus;
    private readonly HttpStatusCode _secondStatus;
    private readonly string _secondBody;

    public int CallCount { get; private set; }

    public N8nTwoPhaseHandler(
        HttpStatusCode firstStatus,
        HttpStatusCode secondStatus,
        string secondBody)
    {
        _firstStatus = firstStatus;
        _secondStatus = secondStatus;
        _secondBody = secondBody;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        if (CallCount == 1)
        {
            return Task.FromResult(new HttpResponseMessage(_firstStatus)
            {
                Content = new StringContent("first-call", Encoding.UTF8, "application/json")
            });
        }

        return Task.FromResult(new HttpResponseMessage(_secondStatus)
        {
            Content = new StringContent(_secondBody, Encoding.UTF8, "application/json")
        });
    }
}

/// <summary>
/// Throws <see cref="HttpRequestException"/> for every request.
/// Used to test IsAvailableAsync when the n8n instance is unreachable.
/// </summary>
internal class ThrowingHttpHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        throw new HttpRequestException("Simulated network failure");
}
