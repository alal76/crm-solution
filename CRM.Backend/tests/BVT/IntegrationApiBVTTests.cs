// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
