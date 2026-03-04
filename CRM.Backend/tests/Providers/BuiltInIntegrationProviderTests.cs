// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// MANDATORY: Written after verifying actual source:
//   Class:       BuiltInIntegrationProvider
//   Namespace:   CRM.Infrastructure.Providers.Integration
//   Constructor: (HttpClient, IOptions<BuiltInIntegrationConfiguration>, ILogger<BuiltInIntegrationProvider>)
//   _webhooks / _executionHistory are per-instance → new instance per test = full isolation.
//   _webhookCounter / _executionCounter are static → IDs are monotonically-increasing but we
//   only assert prefix ("webhook_") not exact integer values.
//   IsAvailableAsync always returns true (no HTTP call).
//   TriggerWorkflowAsync never makes an HTTP call.
//   TestWebhookAsync requires webhook to already be registered before it can deliver.

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
/// Unit tests for <see cref="BuiltInIntegrationProvider"/>.
/// HttpClient is mocked via <see cref="IntegrationMockHttpHandler"/> so no real network calls are made.
/// </summary>
public class BuiltInIntegrationProviderTests
{
    // ── Factory helpers ──────────────────────────────────────────────────────

    private static (BuiltInIntegrationProvider provider, List<HttpRequestMessage> capturedRequests)
        CreateProvider(
            BuiltInIntegrationConfiguration? config = null,
            HttpStatusCode httpStatus = HttpStatusCode.OK,
            string httpResponseBody = "ok")
    {
        var captured = new List<HttpRequestMessage>();
        var handler = new IntegrationMockHttpHandler(captured, httpStatus, httpResponseBody);
        var httpClient = new HttpClient(handler);

        var effectiveConfig = config ?? new BuiltInIntegrationConfiguration
        {
            WebhookTimeoutSeconds = 5,
            DefaultWebhookSecret = null,
            MaxRetryAttempts = 3
        };

        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<BuiltInIntegrationProvider>>();
        var provider = new BuiltInIntegrationProvider(httpClient, options, logger.Object);

        return (provider, captured);
    }

    private static WebhookRegistration MakeRegistration(
        string name = "Test Hook",
        string url = "https://receiver.example.com/hook",
        bool isActive = true,
        params string[] eventTypes) =>
        new()
        {
            Name = name,
            TargetUrl = url,
            IsActive = isActive,
            EventTypes = eventTypes.Length > 0
                ? new List<string>(eventTypes)
                : new List<string> { "account.created" }
        };

    private static CrmEvent MakeEvent(string eventType = "account.created") =>
        new()
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = eventType,
            EntityType = "account",
            EntityId = 1,
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "name", "Acme" } }
        };

    // ── ProviderName ─────────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        var (provider, _) = CreateProvider();
        provider.ProviderName.Should().Be("BuiltIn");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_Always()
    {
        // BuiltIn provider needs no external system — always available
        var (provider, _) = CreateProvider();
        var result = await provider.IsAvailableAsync();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_EvenWhenHttpWouldFail()
    {
        // IsAvailableAsync makes NO http call — it must return true regardless of http mock status
        var (provider, captured) = CreateProvider(httpStatus: HttpStatusCode.ServiceUnavailable);
        var result = await provider.IsAvailableAsync();
        result.Should().BeTrue();
        captured.Should().BeEmpty("IsAvailableAsync should not make any HTTP call");
    }

    // ── RegisterWebhookAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task RegisterWebhookAsync_ReturnsWebhookInfo_WithExpectedFields()
    {
        var (provider, _) = CreateProvider();
        var registration = MakeRegistration("My Hook", "https://hook.example.com/recv", true, "contact.created");

        var result = await provider.RegisterWebhookAsync(registration);

        result.Should().NotBeNull();
        result.Id.Should().StartWith("webhook_");
        result.Name.Should().Be("My Hook");
        result.TargetUrl.Should().Be("https://hook.example.com/recv");
        result.IsActive.Should().BeTrue();
        result.EventTypes.Should().Contain("contact.created");
        result.TotalDeliveries.Should().Be(0);
        result.FailedDeliveries.Should().Be(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RegisterWebhookAsync_CanRegisterMultipleWebhooks_WithDistinctIds()
    {
        var (provider, _) = CreateProvider();

        var w1 = await provider.RegisterWebhookAsync(MakeRegistration("Hook A", "https://a.example.com", true, "account.created"));
        var w2 = await provider.RegisterWebhookAsync(MakeRegistration("Hook B", "https://b.example.com", true, "account.updated"));

        w1.Id.Should().NotBe(w2.Id);
    }

    // ── GetWebhooksAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetWebhooksAsync_ReturnsEmpty_WhenNoWebhooksRegistered()
    {
        var (provider, _) = CreateProvider();

        var webhooks = await provider.GetWebhooksAsync();

        webhooks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWebhooksAsync_ReturnsAllWebhooks_WhenNoFilterProvided()
    {
        var (provider, _) = CreateProvider();
        await provider.RegisterWebhookAsync(MakeRegistration("A", "https://a.example.com", true, "account.created"));
        await provider.RegisterWebhookAsync(MakeRegistration("B", "https://b.example.com", true, "contact.updated"));

        var webhooks = (await provider.GetWebhooksAsync()).ToList();

        webhooks.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetWebhooksAsync_FiltersCorrectly_ByEventType()
    {
        var (provider, _) = CreateProvider();
        await provider.RegisterWebhookAsync(MakeRegistration("A", "https://a.example.com", true, "account.created"));
        await provider.RegisterWebhookAsync(MakeRegistration("B", "https://b.example.com", true, "contact.updated"));

        var filtered = (await provider.GetWebhooksAsync("account.created")).ToList();

        filtered.Should().HaveCount(1);
        filtered[0].Name.Should().Be("A");
    }

    // ── UpdateWebhookAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateWebhookAsync_UpdatesFields_WhenWebhookExists()
    {
        var (provider, _) = CreateProvider();
        var registered = await provider.RegisterWebhookAsync(MakeRegistration("Old Name", "https://old.example.com", true, "account.created"));

        var update = MakeRegistration("New Name", "https://new.example.com", false, "account.updated");
        await provider.UpdateWebhookAsync(registered.Id, update);

        var webhooks = (await provider.GetWebhooksAsync()).ToList();
        webhooks.Should().HaveCount(1);
        webhooks[0].Name.Should().Be("New Name");
        webhooks[0].TargetUrl.Should().Be("https://new.example.com");
        webhooks[0].IsActive.Should().BeFalse();
        webhooks[0].EventTypes.Should().Contain("account.updated");
    }

    [Fact]
    public async Task UpdateWebhookAsync_DoesNotThrow_WhenWebhookIdNotFound()
    {
        var (provider, _) = CreateProvider();

        var act = async () => await provider.UpdateWebhookAsync("nonexistent_id", MakeRegistration());

        await act.Should().NotThrowAsync();
    }

    // ── DeleteWebhookAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task DeleteWebhookAsync_RemovesWebhook_WhenExists()
    {
        var (provider, _) = CreateProvider();
        var registered = await provider.RegisterWebhookAsync(MakeRegistration());

        await provider.DeleteWebhookAsync(registered.Id);

        var webhooks = await provider.GetWebhooksAsync();
        webhooks.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteWebhookAsync_DoesNotThrow_WhenWebhookIdNotFound()
    {
        var (provider, _) = CreateProvider();

        var act = async () => await provider.DeleteWebhookAsync("does_not_exist");

        await act.Should().NotThrowAsync();
    }

    // ── PublishEventAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task PublishEventAsync_ReturnsSuccessWithZeroWebhooks_WhenNoSubscribers()
    {
        var (provider, captured) = CreateProvider();
        var evt = MakeEvent("account.created");

        var result = await provider.PublishEventAsync(evt);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(0);
        result.EventId.Should().Be(evt.EventId);
        captured.Should().BeEmpty("no webhooks registered, so no HTTP delivery should occur");
    }

    [Fact]
    public async Task PublishEventAsync_DeliversToMatchingWebhooks_WhenSubscribersExist()
    {
        var (provider, captured) = CreateProvider(httpStatus: HttpStatusCode.OK);
        await provider.RegisterWebhookAsync(MakeRegistration("Hook", "https://recv.example.com", true, "account.created"));

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(1);
        captured.Should().HaveCount(1);
        captured[0].Method.Should().Be(HttpMethod.Post);
        captured[0].RequestUri!.ToString().Should().StartWith("https://recv.example.com");
    }

    [Fact]
    public async Task PublishEventAsync_DoesNotDeliverToInactiveWebhooks()
    {
        var (provider, captured) = CreateProvider(httpStatus: HttpStatusCode.OK);
        await provider.RegisterWebhookAsync(MakeRegistration("Inactive", "https://recv.example.com", isActive: false, "account.created"));

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(0);
        captured.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishEventAsync_DoesNotDeliverToWebhooksWithDifferentEventType()
    {
        var (provider, captured) = CreateProvider(httpStatus: HttpStatusCode.OK);
        await provider.RegisterWebhookAsync(MakeRegistration("Hook", "https://recv.example.com", true, "contact.created"));

        // Publish an event that does NOT match the registered hook's event type
        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(0);
        captured.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishEventAsync_CountsDeliveryFailure_WhenHttpReturnsError()
    {
        var (provider, _) = CreateProvider(httpStatus: HttpStatusCode.InternalServerError);
        await provider.RegisterWebhookAsync(MakeRegistration("Hook", "https://recv.example.com", true, "account.created"));

        var result = await provider.PublishEventAsync(MakeEvent("account.created"));

        // Success should be false when the single delivery fails
        result.Success.Should().BeFalse();
        result.WebhooksTriggered.Should().Be(0);
    }

    [Fact]
    public async Task PublishEventAsync_AddsSignatureHeader_WhenSecretIsConfigured()
    {
        var config = new BuiltInIntegrationConfiguration
        {
            WebhookTimeoutSeconds = 5,
            DefaultWebhookSecret = "my-super-secret"
        };
        var (provider, captured) = CreateProvider(config: config, httpStatus: HttpStatusCode.OK);
        await provider.RegisterWebhookAsync(MakeRegistration("Hook", "https://recv.example.com", true, "account.created"));

        await provider.PublishEventAsync(MakeEvent("account.created"));

        captured.Should().HaveCount(1);
        var request = captured[0];
        request.Headers.TryGetValues("X-Webhook-Signature", out var sigValues).Should().BeTrue();
        var sig = sigValues!.First();
        sig.Should().StartWith("sha256=");
    }

    // ── PublishEventsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task PublishEventsAsync_PublishesAllEvents_AndAggregatesResults()
    {
        var (provider, captured) = CreateProvider(httpStatus: HttpStatusCode.OK);
        await provider.RegisterWebhookAsync(MakeRegistration("Hook", "https://recv.example.com", true, "account.created", "contact.updated"));

        var events = new[]
        {
            MakeEvent("account.created"),
            MakeEvent("contact.updated"),
            MakeEvent("opportunity.won")   // No subscriber for this one
        };

        var batch = await provider.PublishEventsAsync(events);

        batch.TotalCount.Should().Be(3);
        batch.SuccessCount.Should().Be(3); // All succeed (two deliver + one has no sub = still "success")
        batch.FailureCount.Should().Be(0);
        batch.Results.Should().HaveCount(3);
        captured.Should().HaveCount(2, "only 2 of 3 events had a registered subscriber");
    }

    // ── TestWebhookAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task TestWebhookAsync_ReturnsFailureWithError_WhenWebhookNotFound()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.TestWebhookAsync("nonexistent_id");

        result.Success.Should().BeFalse();
        result.Error.Should().Be("Webhook not found");
    }

    [Fact]
    public async Task TestWebhookAsync_ReturnsSuccess_WhenWebhookExistsAndHttpReturns200()
    {
        var (provider, captured) = CreateProvider(httpStatus: HttpStatusCode.OK);
        var registered = await provider.RegisterWebhookAsync(MakeRegistration("Hook", "https://recv.example.com", true, "account.created"));

        var result = await provider.TestWebhookAsync(registered.Id);

        result.Success.Should().BeTrue();
        result.Error.Should().BeNullOrEmpty();
        captured.Should().HaveCount(1);
        captured[0].Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task TestWebhookAsync_ReturnsFailure_WhenHttpCallFails()
    {
        var (provider, _) = CreateProvider(httpStatus: HttpStatusCode.BadGateway);
        var registered = await provider.RegisterWebhookAsync(MakeRegistration());

        var result = await provider.TestWebhookAsync(registered.Id);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("Delivery failed");
    }

    // ── TriggerWorkflowAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task TriggerWorkflowAsync_ReturnsSuccess_WithoutMakingHttpCalls()
    {
        var (provider, captured) = CreateProvider();

        var result = await provider.TriggerWorkflowAsync("wf_123", new { key = "value" });

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.WorkflowId.Should().Be("wf_123");
        result.Status.Should().Be("success");
        result.ExecutionId.Should().StartWith("exec_");
        captured.Should().BeEmpty("BuiltIn provider simulates workflow execution locally");
    }

    // ── GetWorkflowsAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowsAsync_ReturnsBuiltInDispatcherWorkflow()
    {
        var (provider, _) = CreateProvider();

        var workflows = (await provider.GetWorkflowsAsync()).ToList();

        workflows.Should().HaveCount(1);
        workflows[0].Id.Should().Be("builtin_webhook_dispatcher");
        workflows[0].Name.Should().Contain("BuiltIn");
        workflows[0].IsActive.Should().BeTrue();
        workflows[0].TriggerType.Should().Be("event");
    }

    // ── GetWorkflowExecutionsAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowExecutionsAsync_ReturnsEmpty_WhenWorkflowNeverTriggered()
    {
        var (provider, _) = CreateProvider();

        var executions = await provider.GetWorkflowExecutionsAsync("unknown_workflow");

        executions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorkflowExecutionsAsync_ReturnsHistory_AfterTrigger()
    {
        var (provider, _) = CreateProvider();
        await provider.TriggerWorkflowAsync("my_workflow", new { key = "value" });

        var executions = (await provider.GetWorkflowExecutionsAsync("my_workflow")).ToList();

        executions.Should().HaveCount(1);
        executions[0].WorkflowId.Should().Be("my_workflow");
        executions[0].Status.Should().Be("success");
    }

    [Fact]
    public async Task GetWorkflowExecutionsAsync_RespectsLimit()
    {
        var (provider, _) = CreateProvider();

        for (var i = 0; i < 5; i++)
        {
            await provider.TriggerWorkflowAsync("limited_workflow", new { i });
        }

        var executions = (await provider.GetWorkflowExecutionsAsync("limited_workflow", limit: 3)).ToList();

        executions.Should().HaveCountLessOrEqualTo(3);
    }

    // ── GetConnectedAppsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetConnectedAppsAsync_ReturnsEmpty_WhenNoWebhooksRegistered()
    {
        var (provider, _) = CreateProvider();

        var apps = await provider.GetConnectedAppsAsync();

        apps.Should().BeEmpty();
    }

    [Fact]
    public async Task GetConnectedAppsAsync_ReturnsOneAppPerWebhook()
    {
        var (provider, _) = CreateProvider();
        await provider.RegisterWebhookAsync(MakeRegistration("App A", "https://a.example.com", true, "account.created"));
        await provider.RegisterWebhookAsync(MakeRegistration("App B", "https://b.example.com", false, "contact.updated"));

        var apps = (await provider.GetConnectedAppsAsync()).ToList();

        apps.Should().HaveCount(2);
        apps.Should().Contain(a => a.Name == "App A" && a.IsConnected);
        apps.Should().Contain(a => a.Name == "App B" && !a.IsConnected);
    }

    // ── ProcessIncomingWebhookAsync ──────────────────────────────────────────

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsSuccess_WhenPayloadIsValidJson()
    {
        var (provider, _) = CreateProvider();
        const string payload = """{"accountId": 42, "name": "Acme"}""";

        var result = await provider.ProcessIncomingWebhookAsync("account.created", payload);

        result.Success.Should().BeTrue();
        result.EventType.Should().Be("account.created");
        result.Action.Should().Be("create");
        result.ProcessedData.Should().ContainKey("accountId");
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsUpdateAction_WhenEventTypeContainsUpdated()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.ProcessIncomingWebhookAsync("contact.updated", """{"id": 1}""");

        result.Success.Should().BeTrue();
        result.Action.Should().Be("update");
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsDeleteAction_WhenEventTypeContainsDeleted()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.ProcessIncomingWebhookAsync("account.deleted", """{"id": 7}""");

        result.Success.Should().BeTrue();
        result.Action.Should().Be("delete");
    }

    [Fact]
    public async Task ProcessIncomingWebhookAsync_ReturnsFailure_WhenPayloadIsInvalidJson()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.ProcessIncomingWebhookAsync("account.created", "NOT_VALID_JSON");

        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    // ── HealthCheckAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_IsAlwaysHealthy_WithNoWebhooks()
    {
        var (provider, _) = CreateProvider();

        var health = await provider.HealthCheckAsync();

        health.IsHealthy.Should().BeTrue();
        health.ProviderName.Should().Be("BuiltIn");
        health.Message.Should().Contain("healthy");
        health.Details.Should().ContainKey("activeWebhooks");
        health.Details.Should().ContainKey("totalDeliveries");
    }

    [Fact]
    public async Task HealthCheckAsync_ReflectsRegisteredWebhookStats()
    {
        var (provider, _) = CreateProvider();
        await provider.RegisterWebhookAsync(MakeRegistration("Active", "https://a.example.com", true, "account.created"));
        await provider.RegisterWebhookAsync(MakeRegistration("Inactive", "https://b.example.com", false, "contact.updated"));

        var health = await provider.HealthCheckAsync();

        health.IsHealthy.Should().BeTrue();
        health.Details["activeWebhooks"].Should().Be(1);
        health.Details["totalWebhooks"].Should().Be(2);
    }
}

// ── Shared mock handler ──────────────────────────────────────────────────────

/// <summary>
/// Reusable mock HttpMessageHandler for integration provider tests.
/// Captures all outbound requests and returns a configurable response.
/// </summary>
internal class IntegrationMockHttpHandler : HttpMessageHandler
{
    private readonly List<HttpRequestMessage> _captured;
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public IntegrationMockHttpHandler(
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
