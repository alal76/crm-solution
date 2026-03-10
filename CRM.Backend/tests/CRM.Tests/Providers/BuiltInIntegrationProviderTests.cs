// CRM Solution — CRM Test Suite
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Integration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="BuiltInIntegrationProvider"/> (TCOV-057).</summary>
public class BuiltInIntegrationProviderTests
{
    private readonly Mock<ILogger<BuiltInIntegrationProvider>> _loggerMock = new();

    private BuiltInIntegrationProvider Create(HttpClient? httpClient = null)
    {
        var options = Options.Create(new BuiltInIntegrationConfiguration());
        return new BuiltInIntegrationProvider(
            httpClient ?? new HttpClient(),
            options,
            _loggerMock.Object);
    }

    // ─── Properties ─────────────────────────────────────────────────────────────
    [Fact]
    public void ProviderName_ShouldReturnBuiltIn()
    {
        Create().ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnTrue()
    {
        (await Create().IsAvailableAsync()).Should().BeTrue();
    }

    // ─── Event Publishing ─────────────────────────────────────────────────────────
    [Fact]
    public async Task PublishEventAsync_NoWebhooks_ShouldSucceedWithZeroDeliveries()
    {
        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityType = "Account",
            EntityId = 1
        };
        var result = await Create().PublishEventAsync(crmEvent);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(0);
    }

    [Fact]
    public async Task PublishEventsAsync_EmptyList_ShouldReturnEmptyBatch()
    {
        var result = await Create().PublishEventsAsync(Enumerable.Empty<CrmEvent>());
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task PublishEventsAsync_SingleEvent_ShouldCountCorrectly()
    {
        var events = new[]
        {
            new CrmEvent { EventType = "contact.updated", EntityType = "Contact", EntityId = 2 }
        };
        var result = await Create().PublishEventsAsync(events);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetWebhooksAsync_NewInstance_ShouldReturnEmptyOrList()
    {
        var webhooks = (await Create().GetWebhooksAsync());
        webhooks.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterWebhookAsync_ValidRequest_ShouldReturnWebhook()
    {
        var req = new WebhookRegistration
        {
            Name = "Test Webhook",
            TargetUrl = "https://webhook.example.com/events",
            EventTypes = new List<string> { "account.created" }
        };
        var webhook = await Create().RegisterWebhookAsync(req);
        webhook.Should().NotBeNull();
        webhook.Name.Should().Be("Test Webhook");
    }
}
