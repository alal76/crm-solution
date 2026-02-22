// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class SalesQuotasControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public SalesQuotasControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_SalesQuotas_Succeeds()
        {
            var create = new { name = "Test" };
            var cRes = await _client.PostAsJsonAsync("/api/sales-quotas", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/sales-quotas/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/sales-quotas/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
