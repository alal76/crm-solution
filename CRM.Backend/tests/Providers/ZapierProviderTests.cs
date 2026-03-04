// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// MANDATORY: Written after verifying actual source:
//   Class:       ZapierProvider
//   Namespace:   CRM.Infrastructure.Providers.Integration
//   Constructor: (HttpClient, IOptions<ZapierConfiguration>, ILogger<ZapierProvider>)
//   Constructor side effects: NONE (no BaseAddress set, no default headers)
//   ProviderName => "Zapier"
//   IsAvailableAsync     → config-only check, NO http call
//   PublishEventAsync    → POST to EventWebhooks lookup (exact → wildcard → default → WebhookBaseUrl)
//   RegisterWebhookAsync → returns info, NO http call
//   GetWebhooksAsync     → maps config EventWebhooks to list, NO http call
//                          IDs are "zapier_hook_0", "zapier_hook_1", ...
//   UpdateWebhookAsync   → logs only, NO http call
//   DeleteWebhookAsync   → logs only, NO http call
//   TestWebhookAsync     → calls GetWebhooksAsync to find hook by ID, then POST to webhook.TargetUrl
//                          returns "Webhook not found" error when id not present
//   TriggerWorkflowAsync → always returns failure (not supported)
//   GetWorkflowsAsync    → maps config EventWebhooks, NO http call
//   GetWorkflowExecutionsAsync → always returns empty, NO http call
//   GetConnectedAppsAsync → returns single "Zapier" ConnectedApp, NO http call
//   TestConnectionAsync  → calls TestWebhookAsync on first webhook
//   ProcessIncomingWebhookAsync → JSON parse only
//   HealthCheckAsync     → config-only check, NO http call

using System.Net;
using System.Text;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Integration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="ZapierProvider"/>.
/// Most methods never make HTTP calls (config-driven). Only PublishEventAsync,
/// TestWebhookAsync, and TestConnectionAsync hit the HttpClient.
/// </summary>
public class ZapierProviderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (ZapierProvider provider, List<HttpRequestMessage> capturedRequests)
        CreateProvider(
            ZapierConfiguration? config = null,
            HttpStatusCode httpStatus = HttpStatusCode.OK,
            string httpResponseBody = """{"status":"queued"}""")
    {
        var captured = new List<HttpRequestMessage>();
        var handler = new ZapierMockHttpHandler(captured, httpStatus, httpResponseBody);
        var httpClient = new HttpClient(handler);

        var effectiveConfig = config ?? DefaultConfig();
        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<ZapierProvider>>();
        var provider = new ZapierProvider(httpClient, options, logger.Object);

        return (provider, captured);
    }

    private static ZapierConfiguration DefaultConfig(
        string? webhookBaseUrl = null,
        Dictionary<string, string>? eventWebhooks = null) =>
        new()
        {
            WebhookBaseUrl = webhookBaseUrl,
            EventWebhooks = eventWebhooks ?? new Dictionary<string, string>
            {
                { "account.created", "https://hooks.zapier.com/hooks/catch/111/acc-hook" }
            }
        };

    private static ZapierConfiguration EmptyConfig() =>
        new()
        {
            WebhookBaseUrl = null,
            EventWebhooks = null
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
    public void ProviderName_ReturnsZapier()
    {
        var (provider, _) = CreateProvider();
        provider.ProviderName.Should().Be("Zapier");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenWebhookBaseUrlIsSet()
    {
        var config = DefaultConfig(webhookBaseUrl: "https://hooks.zapier.com/hooks/catch/123");
        var (provider, captured) = CreateProvider(config);

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
        captured.Should().BeEmpty("IsAvailableAsync is config-only, no HTTP call");
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenEventWebhooksAreConfigured()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/abc" }
        });
        var (provider, captured) = CreateProvider(config);

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
        captured.Should().BeEmpty();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenNeitherWebhookBaseUrlNorEventWebhooksConfigured()
    {
        var (provider, captured) = CreateProvider(EmptyConfig());

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
        captured.Should().BeEmpty();
    }

    // ── PublishEventAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task PublishEventAsync_PostsToExactMatch_WhenEventTypeConfigured()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/acc" }
        });
        var (provider, captured) = CreateProvider(config, httpStatus: HttpStatusCode.OK);

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(1);
        captured.Should().HaveCount(1);
        captured[0].RequestUri!.ToString().Should().Be("https://hooks.zapier.com/hooks/catch/111/acc");
        captured[0].Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task PublishEventAsync_UsesWildcard_WhenExactMappingMissing()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.*", "https://hooks.zapier.com/hooks/catch/111/account-wildcard" }
        });
        var (provider, captured) = CreateProvider(config, httpStatus: HttpStatusCode.OK);

        var result = await provider.PublishEventAsync(MakeEvent("account.deleted"));

        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(1);
        captured[0].RequestUri!.ToString().Should().Be("https://hooks.zapier.com/hooks/catch/111/account-wildcard");
    }

    [Fact]
    public async Task PublishEventAsync_UsesDefaultMapping_WhenNoSpecificOrWildcardMatch()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "*", "https://hooks.zapier.com/hooks/catch/111/default" }
        });
        var (provider, captured) = CreateProvider(config, httpStatus: HttpStatusCode.OK);

        var result = await provider.PublishEventAsync(MakeEvent("some.unknown.event"));

        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(1);
        captured[0].RequestUri!.ToString().Should().Be("https://hooks.zapier.com/hooks/catch/111/default");
    }

    [Fact]
    public async Task PublishEventAsync_UsesWebhookBaseUrl_WhenNoOtherMappingMatches()
    {
        var config = new ZapierConfiguration
        {
            WebhookBaseUrl = "https://hooks.zapier.com/hooks/catch/999/global",
            EventWebhooks = null
        };
        var (provider, captured) = CreateProvider(config, httpStatus: HttpStatusCode.OK);

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Success.Should().BeTrue();
        captured.Should().HaveCount(1);
        captured[0].RequestUri!.ToString().Should().Be("https://hooks.zapier.com/hooks/catch/999/global");
    }

    [Fact]
    public async Task PublishEventAsync_ReturnsSuccessWithZeroTriggers_WhenNoWebhookResolved()
    {
        var (provider, captured) = CreateProvider(EmptyConfig());

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(0);
        captured.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishEventAsync_ReturnsFailure_WhenZapierReturns500()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/acc" }
        });
        var (provider, _) = CreateProvider(config, httpStatus: HttpStatusCode.InternalServerError, httpResponseBody: "error");

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Success.Should().BeFalse();
        result.WebhooksTriggered.Should().Be(0);
        result.Error.Should().Contain("InternalServerError");
    }

    [Fact]
    public async Task PublishEventAsync_ExtractsMessageId_FromZapierResponse()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/acc" }
        });
        var (provider, _) = CreateProvider(
            config,
            httpStatus: HttpStatusCode.OK,
            httpResponseBody: """{"id":"zap-req-abc123","status":"queued"}""");

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Success.Should().BeTrue();
        result.MessageId.Should().Be("zap-req-abc123");
    }

    // ── PublishEventsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task PublishEventsAsync_AggregatesResults_ForMultipleEvents()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/acc" },
            { "contact.deleted", "https://hooks.zapier.com/hooks/catch/111/contact" }
        });
        var (provider, captured) = CreateProvider(config, httpStatus: HttpStatusCode.OK);

        var events = new[]
        {
            MakeEvent("account.created"),
            MakeEvent("contact.deleted"),
            MakeEvent("unknown.event")    // no mapping → success, 0 triggers
        };

        var batch = await provider.PublishEventsAsync(events);

        batch.TotalCount.Should().Be(3);
        batch.SuccessCount.Should().Be(3);
        batch.FailureCount.Should().Be(0);
        captured.Should().HaveCount(2, "only 2 events have a webhook mapping");
    }

    // ── RegisterWebhookAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task RegisterWebhookAsync_ReturnsWebhookInfo_WithoutHttpCall()
    {
        var (provider, captured) = CreateProvider();

        var result = await provider.RegisterWebhookAsync(new WebhookRegistration
        {
            Name = "My Zap Hook",
            TargetUrl = "https://hooks.zapier.com/hooks/catch/222/recv",
            EventTypes = new List<string> { "account.created" }
        });

        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Name.Should().Be("My Zap Hook");
        result.TargetUrl.Should().Be("https://hooks.zapier.com/hooks/catch/222/recv");
        result.IsActive.Should().BeTrue();
        captured.Should().BeEmpty("Zapier webhooks are configured via UI, no API call");
    }

    // ── GetWebhooksAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetWebhooksAsync_ReturnsEmpty_WhenNoEventWebhooksConfigured()
    {
        var (provider, _) = CreateProvider(EmptyConfig());

        var webhooks = await provider.GetWebhooksAsync();

        webhooks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWebhooksAsync_ReturnsOneEntryPerEventWebhook()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/a" },
            { "contact.updated", "https://hooks.zapier.com/hooks/catch/111/b" }
        });
        var (provider, captured) = CreateProvider(config);

        var webhooks = (await provider.GetWebhooksAsync()).ToList();

        webhooks.Should().HaveCount(2);
        captured.Should().BeEmpty("GetWebhooksAsync is config-only");
    }

    [Fact]
    public async Task GetWebhooksAsync_AssignsSequentialIds_StartingAtZero()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/a" },
            { "contact.updated", "https://hooks.zapier.com/hooks/catch/111/b" }
        });
        var (provider, _) = CreateProvider(config);

        var webhooks = (await provider.GetWebhooksAsync()).ToList();

        webhooks[0].Id.Should().Be("zapier_hook_0");
        webhooks[1].Id.Should().Be("zapier_hook_1");
    }

    [Fact]
    public async Task GetWebhooksAsync_FiltersCorrectly_ByEventType()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/a" },
            { "contact.updated", "https://hooks.zapier.com/hooks/catch/111/b" }
        });
        var (provider, _) = CreateProvider(config);

        var filtered = (await provider.GetWebhooksAsync("account.created")).ToList();

        filtered.Should().HaveCount(1);
        filtered[0].EventTypes.Should().Contain("account.created");
    }

    [Fact]
    public async Task GetWebhooksAsync_WildcardEntry_HasAllEventType()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "*", "https://hooks.zapier.com/hooks/catch/111/all" }
        });
        var (provider, _) = CreateProvider(config);

        var webhooks = (await provider.GetWebhooksAsync()).ToList();

        webhooks.Should().HaveCount(1);
        webhooks[0].EventTypes.Should().Contain("all");
    }

    // ── UpdateWebhookAsync / DeleteWebhookAsync ───────────────────────────────

    [Fact]
    public async Task UpdateWebhookAsync_DoesNotThrow_AndMakesNoHttpCalls()
    {
        var (provider, captured) = CreateProvider();
        var act = async () => await provider.UpdateWebhookAsync(
            "zapier_hook_0",
            new WebhookRegistration { Name = "Updated", TargetUrl = "https://new.url", EventTypes = new List<string> { "account.updated" } });

        await act.Should().NotThrowAsync();
        captured.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteWebhookAsync_DoesNotThrow_AndMakesNoHttpCalls()
    {
        var (provider, captured) = CreateProvider();
        var act = async () => await provider.DeleteWebhookAsync("zapier_hook_0");

        await act.Should().NotThrowAsync();
        captured.Should().BeEmpty();
    }

    // ── TestWebhookAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task TestWebhookAsync_ReturnsFailure_WhenWebhookIdNotFound()
    {
        var (provider, _) = CreateProvider(EmptyConfig());

        var result = await provider.TestWebhookAsync("zapier_hook_0");

        result.Success.Should().BeFalse();
        result.Error.Should().Be("Webhook not found");
    }

    [Fact]
    public async Task TestWebhookAsync_PostsToWebhookTargetUrl_WhenFound()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/test" }
        });
        var (provider, captured) = CreateProvider(config, httpStatus: HttpStatusCode.OK);

        var result = await provider.TestWebhookAsync("zapier_hook_0");

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.ResponseTimeMs.Should().BeGreaterThanOrEqualTo(0);
        captured.Should().HaveCount(1);
        captured[0].RequestUri!.ToString().Should().Contain("hooks.zapier.com");
        captured[0].Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task TestWebhookAsync_ReturnsFailure_WhenZapierReturns404()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/gone" }
        });
        var (provider, _) = CreateProvider(config, httpStatus: HttpStatusCode.NotFound);

        var result = await provider.TestWebhookAsync("zapier_hook_0");

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Error.Should().Contain("NotFound");
    }

    // ── TriggerWorkflowAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task TriggerWorkflowAsync_AlwaysReturnsFail_WithExplanatoryError()
    {
        var (provider, captured) = CreateProvider();

        var result = await provider.TriggerWorkflowAsync("zap-abc", new { key = "value" });

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.WorkflowId.Should().Be("zap-abc");
        result.Error.Should().Contain("Catch Hook");
        captured.Should().BeEmpty("Zapier does not support direct workflow triggering via API");
    }

    // ── GetWorkflowsAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowsAsync_ReturnsOneZapPerEventWebhookMapping()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/a" },
            { "contact.updated", "https://hooks.zapier.com/hooks/catch/111/b" }
        });
        var (provider, captured) = CreateProvider(config);

        var workflows = (await provider.GetWorkflowsAsync()).ToList();

        workflows.Should().HaveCount(2);
        workflows.Should().AllSatisfy(w =>
        {
            w.TriggerType.Should().Be("webhook");
            w.IsActive.Should().BeTrue();
        });
        captured.Should().BeEmpty("GetWorkflowsAsync is config-only for Zapier");
    }

    [Fact]
    public async Task GetWorkflowsAsync_ReturnsEmpty_WhenNoEventWebhooksConfigured()
    {
        var (provider, _) = CreateProvider(EmptyConfig());

        var workflows = await provider.GetWorkflowsAsync();

        workflows.Should().BeEmpty();
    }

    // ── GetWorkflowExecutionsAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowExecutionsAsync_AlwaysReturnsEmpty_WithoutHttpCall()
    {
        var (provider, captured) = CreateProvider();

        var executions = await provider.GetWorkflowExecutionsAsync("zap-abc");

        executions.Should().BeEmpty();
        captured.Should().BeEmpty();
    }

    // ── GetConnectedAppsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetConnectedAppsAsync_ReturnsSingleZapierEntry_WhenConfigured()
    {
        var (provider, captured) = CreateProvider();

        var apps = (await provider.GetConnectedAppsAsync()).ToList();

        apps.Should().HaveCount(1);
        apps[0].Id.Should().Be("zapier");
        apps[0].Name.Should().Be("Zapier");
        apps[0].Type.Should().Be("automation");
        apps[0].IsConnected.Should().BeTrue();
        captured.Should().BeEmpty();
    }

    [Fact]
    public async Task GetConnectedAppsAsync_ShowsNotConnected_WhenNotConfigured()
    {
        var (provider, _) = CreateProvider(EmptyConfig());

        var apps = (await provider.GetConnectedAppsAsync()).ToList();

        apps.Should().HaveCount(1);
        apps[0].IsConnected.Should().BeFalse();
    }

    // ── TestConnectionAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task TestConnectionAsync_ReturnsFailure_WhenNoWebhooksConfigured()
    {
        var (provider, _) = CreateProvider(EmptyConfig());

        var result = await provider.TestConnectionAsync("zapier");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("No webhooks");
    }

    [Fact]
    public async Task TestConnectionAsync_ReturnsSuccess_WhenFirstWebhookTestSucceeds()
    {
        var config = DefaultConfig(eventWebhooks: new Dictionary<string, string>
        {
            { "account.created", "https://hooks.zapier.com/hooks/catch/111/recv" }
        });
        var (provider, captured) = CreateProvider(config, httpStatus: HttpStatusCode.OK);

        var result = await provider.TestConnectionAsync("zapier");

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("success");
        captured.Should().HaveCount(1);
    }

    // ── ProcessIncomingWebhookAsync ──────────────────────────────────────────

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsSuccessWithCreateAction_ForCreateEvent()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.ProcessIncomingWebhookAsync("createAccount", """{"companyName":"Acme"}""");

        result.Success.Should().BeTrue();
        result.EventType.Should().Be("createAccount");
        result.Action.Should().Be("create");
        result.ProcessedData.Should().ContainKey("companyName");
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsUpdateAction_ForUpdateEvent()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.ProcessIncomingWebhookAsync("updateContact", """{"id":1}""");

        result.Success.Should().BeTrue();
        result.Action.Should().Be("update");
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsDeleteAction_ForDeleteEvent()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.ProcessIncomingWebhookAsync("deleteOpportunity", """{"id":5}""");

        result.Success.Should().BeTrue();
        result.Action.Should().Be("delete");
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsActionFromPayload_WhenEventTypeIsNeutral()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.ProcessIncomingWebhookAsync(
            "zapier.callback",
            """{"action":"notify","message":"done"}""");

        result.Success.Should().BeTrue();
        result.Action.Should().Be("notify");
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsFailure_WhenPayloadIsInvalidJson()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.ProcessIncomingWebhookAsync("account.created", "NOT_JSON");

        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    // ── HealthCheckAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthy_WhenConfigured()
    {
        var (provider, captured) = CreateProvider();

        var health = await provider.HealthCheckAsync();

        health.IsHealthy.Should().BeTrue();
        health.ProviderName.Should().Be("Zapier");
        health.Message.Should().Contain("configured");
        health.Details.Should().ContainKey("webhookCount");
        captured.Should().BeEmpty("HealthCheckAsync is config-only for Zapier");
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsUnhealthy_WhenNotConfigured()
    {
        var (provider, captured) = CreateProvider(EmptyConfig());

        var health = await provider.HealthCheckAsync();

        health.IsHealthy.Should().BeFalse();
        health.ProviderName.Should().Be("Zapier");
        health.Message.Should().Contain("not configured");
        captured.Should().BeEmpty();
    }
}

// ── Mock HTTP handler for Zapier tests ───────────────────────────────────────

/// <summary>
/// Captures outgoing HTTP requests and returns a fixed status code + body.
/// Used for ZapierProvider tests where all HTTP calls are outbound webhooks.
/// </summary>
internal class ZapierMockHttpHandler : HttpMessageHandler
{
    private readonly List<HttpRequestMessage> _captured;
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public ZapierMockHttpHandler(
        List<HttpRequestMessage> captured,
        HttpStatusCode status,
        string body)
    {
        _captured = captured;
        _status = status;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _captured.Add(request);
        return Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json")
        });
    }
}
