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
    public class AdminDashboardControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public AdminDashboardControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task GetEndpoint_AdminDashboard_ReturnsNon500()
        {
            var res = await _client.GetAsync("/api/admin");
            ((int)res.StatusCode).Should().BeLessThan(500, "GET /api/admin should not return a server error");
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/admin/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
