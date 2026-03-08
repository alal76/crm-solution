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
    [Trait("Category", "Integration")]
    [Collection("IntegrationTests")]
    public class LookupsControllerTests
    {
        private readonly HttpClient _client;
        public LookupsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task GetCategoriesAndItems_WorkAsDesigned()
        {
            // categories endpoint should return 200 and array
            var cats = await _client.GetAsync("/api/lookups/categories");
            cats.StatusCode.Should().Be(HttpStatusCode.OK);

            // try a nonexistent category name for items should yield 404
            var items = await _client.GetAsync("/api/lookups/items/doesnotexist");
            items.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
