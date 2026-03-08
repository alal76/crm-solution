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
    public class EscalationPoliciesControllerTests
    {
        private readonly HttpClient _client;
        public EscalationPoliciesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_EscalationPolicies_Succeeds()
        {
            var create = new
            {
                Name = "Test",
                Description = "Test",
                InitialAssignmentMinutes = 1,
                MaxEscalationLevels = 1,
                IsActive = true,
                NotifyDuringEscalation = true,
                PolicyId = 1,
                Level = 1,
                EscalationAfterMinutes = 1,
                EscalateToUserId = 1,
                EscalateToGroupId = 1,
                NotificationTemplate = "Test",
                SendNotification = true,
                TicketId = 1,
                EscalatedAt = DateTime.UtcNow,
                EscalatedToUserId = 1,
                EscalatedToGroupId = 1,
                Reason = "Test",
                Notes = "Test"
            };
            var cRes = await _client.PostAsJsonAsync("/api/itsm/escalation-policies", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/itsm/escalation-policies/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/itsm/escalation-policies/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
