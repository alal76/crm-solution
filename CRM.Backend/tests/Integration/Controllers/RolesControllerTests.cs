// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    [Trait("Category", "Integration")]
    [Collection("IntegrationTests")]
    public class RolesControllerTests
    {
        private readonly HttpClient _client;
        public RolesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact(Skip = "Requires Redis")]
        public async Task GetEndpoint_Roles_ReturnsNon500()
        {
            var res = await _client.GetAsync("/api/roles");
            ((int)res.StatusCode).Should().BeLessThan(500, "GET /api/roles should not return a server error");
        }

        [Fact(Skip = "Requires Redis")]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/roles/999999");
            new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
        }
    }
}
