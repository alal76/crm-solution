using System.Net;
using CRM.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.BVT;

public class MarketingIrmApiBVTTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public MarketingIrmApiBVTTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Campaigns_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/campaigns");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EmailTemplates_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/emailtemplates");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EmailSequences_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/emailsequences");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ServiceRequests_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/servicerequests");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Knowledge_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/knowledge");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
