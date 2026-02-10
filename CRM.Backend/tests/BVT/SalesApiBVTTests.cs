using System.Net;
using CRM.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.BVT;

public class SalesApiBVTTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public SalesApiBVTTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Quotes_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/quotes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Orders_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/orders");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Invoices_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/invoices");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Payments_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/payments");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreditMemos_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/creditmemos");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
