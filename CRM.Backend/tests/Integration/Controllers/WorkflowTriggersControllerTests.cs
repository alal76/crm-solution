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
    public class WorkflowTriggersControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public WorkflowTriggersControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task GetEndpoint_WorkflowTriggers_ReturnsNon500()
        {
            var res = await _client.GetAsync("/api/workflow-triggers");
            ((int)res.StatusCode).Should().BeLessThan(500, "GET /api/workflow-triggers should not return a server error");
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/workflow-triggers/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }

        [Fact]
        public async Task GetTriggers_WithFilters_ReturnsOk()
        {
            var res = await _client.GetAsync("/api/workflow-triggers?entityType=Lead&isActive=true");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetMatchingTriggers_ReturnsOk()
        {
            var res = await _client.GetAsync("/api/workflow-triggers/matching?entityType=Lead&triggerType=1");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetStatistics_ReturnsOk()
        {
            var res = await _client.GetAsync("/api/workflow-triggers/statistics");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ValidateCron_ValidExpression_ReturnsOk()
        {
            var req = new { cronExpression = "0 8 * * *" };
            var res = await _client.PostAsJsonAsync("/api/workflow-triggers/validate/cron", req);
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetScheduledDue_ReturnsOk()
        {
            var res = await _client.GetAsync("/api/workflow-triggers/scheduled/due");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
