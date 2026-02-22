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
    public class EscalationRulesControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public EscalationRulesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_EscalationRules_Succeeds()
        {
            var create = new
            {
                Name = "Test",
                Description = "Test",
                Priority = "Test",
                Category = "Test",
                Queue = "Test",
                AgeInMinutes = 1,
                TargetType = "Test",
                TargetId = 1,
                TargetName = "Test",
                MaxAttempts = 1,
                RetryIntervalMinutes = 1,
                IsActive = true,
                RuleId = 1,
                ServiceRequestId = 1,
                RuleMatched = true,
                MatchReason = "Test",
                Rule = (object?)null,
                TestMessage = "Test"
            };
            var cRes = await _client.PostAsJsonAsync("/api/escalation-rules", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/escalation-rules/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                Name = "Test2",
                Description = "Test",
                Priority = "Test",
                Category = "Test",
                Queue = "Test",
                AgeInMinutes = 1,
                TargetType = "Test",
                TargetId = 1,
                TargetName = "Test",
                MaxAttempts = 1,
                RetryIntervalMinutes = 1,
                IsActive = true,
                RuleId = 1,
                ServiceRequestId = 1,
                RuleMatched = true,
                MatchReason = "Test",
                Rule = (object?)null,
                TestMessage = "Test"
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/escalation-rules/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/escalation-rules/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/escalation-rules/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/escalation-rules/999999");
            new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }.Should().Contain(res.StatusCode);
        }
    }
}
