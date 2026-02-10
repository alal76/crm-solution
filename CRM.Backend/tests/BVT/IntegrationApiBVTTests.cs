using System.Net;
using System.Net.Http.Json;
using CRM.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.BVT;

public class IntegrationApiBVTTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public IntegrationApiBVTTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Endpoints_ReturnOk()
    {
        var health = await _client.GetAsync("/health");
        var ready = await _client.GetAsync("/health/ready");
        var live = await _client.GetAsync("/health/live");

        health.StatusCode.Should().Be(HttpStatusCode.OK);
        ready.StatusCode.Should().Be(HttpStatusCode.OK);
        live.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProviderHealth_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health/providers");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Features_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/admin/features");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task NovuWebhook_AcceptsPayload()
    {
        var payload = new
        {
            @event = "notification_sent",
            transactionId = "tx-1",
            subscriberId = "sub-1",
            notificationId = "notif-1",
            channel = "email",
            status = "sent"
        };

        var response = await _client.PostAsJsonAsync("/api/webhooks/novu/delivery", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
