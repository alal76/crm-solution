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
    public class AgentAnalyticsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public AgentAnalyticsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task GetEndpoints_AgentAnalytics_Work()
        {
            var resp = await _client.GetAsync("/api/agents/analytics/usage");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var resp1 = await _client.GetAsync("/api/agents/analytics/accuracy");
            resp1.StatusCode.Should().Be(HttpStatusCode.OK);
            var resp2 = await _client.GetAsync("/api/agents/analytics/cost");
            resp2.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
