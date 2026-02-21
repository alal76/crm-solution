// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
