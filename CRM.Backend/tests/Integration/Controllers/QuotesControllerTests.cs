// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Net.Http.Json;
using Xunit;
using CRM.Tests.Helpers;
using FluentAssertions;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class QuotesControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public QuotesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Quotes_Succeeds()
        {
            var create = new { title = "Test" };
            var cRes = await _client.PostAsJsonAsync("/api/quotes", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            var getRes = await _client.GetAsync($"/api/quotes/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new { title = "Test2" };
            var pRes = await _client.PatchAsJsonAsync($"/api/quotes/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/quotes/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/quotes/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/quotes/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
