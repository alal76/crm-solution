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
    public class ActivitiesControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public ActivitiesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Activities_Succeeds()
        {
            var create = new { ActivityType = 1, Title = "Test", Description = "Test", Details = "Test", ActivityDate = DateTime.UtcNow, DurationMinutes = 1, UserId = 1, UserName = "Test", UserEmail = "Test", EntityType = "Test", EntityId = 1, EntityName = "Test", SecondaryEntityType = "Test", SecondaryEntityId = 1, SecondaryEntityName = "Test", AccountId = 1, ContactId = 1, OpportunityId = 1, CampaignId = 1, ProductId = 1, TaskId = 1, QuoteId = 1, InteractionId = 1, NoteId = 1, OldValue = "Test", NewValue = "Test", FieldsChanged = "Test", IsSystem = true, IsPrivate = true, IsImportant = true, Tags = "Test", Category = "Test", Source = "Test", IsDeleted = true };
            var cRes = await _client.PostAsJsonAsync("/api/activities", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/activities/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/activities/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
