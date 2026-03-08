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
    public class AIAgentUsageControllerTests
    {
        private readonly HttpClient _client;
        public AIAgentUsageControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Create_AIAgentUsage_ReturnsCreated()
        {
            var create = new { AgentId = "test-agent", UserId = (int?)null, RequestCount = 5, Tokens = 1000, Cost = 0.50m, UsageDate = "2026-02-10" };
            var cRes = await _client.PostAsJsonAsync("/api/ai-agent-usage", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetAll_AIAgentUsage_ReturnsOk()
        {
            var res = await _client.GetAsync("/api/ai-agent-usage");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/ai-agent-usage/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
