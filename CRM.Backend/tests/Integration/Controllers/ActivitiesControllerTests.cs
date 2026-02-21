using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
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
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.ActivityType.Should().Be(create.ActivityType);
            item.Title.Should().Be(create.Title);
            item.Description.Should().Be(create.Description);
            item.Details.Should().Be(create.Details);
            item.ActivityDate.Should().Be(create.ActivityDate);
            item.DurationMinutes.Should().Be(create.DurationMinutes);
            item.UserId.Should().Be(create.UserId);
            item.UserName.Should().Be(create.UserName);
            item.UserEmail.Should().Be(create.UserEmail);
            item.EntityType.Should().Be(create.EntityType);
            item.EntityId.Should().Be(create.EntityId);
            item.EntityName.Should().Be(create.EntityName);
            item.SecondaryEntityType.Should().Be(create.SecondaryEntityType);
            item.SecondaryEntityId.Should().Be(create.SecondaryEntityId);
            item.SecondaryEntityName.Should().Be(create.SecondaryEntityName);
            item.AccountId.Should().Be(create.AccountId);
            item.ContactId.Should().Be(create.ContactId);
            item.OpportunityId.Should().Be(create.OpportunityId);
            item.CampaignId.Should().Be(create.CampaignId);
            item.ProductId.Should().Be(create.ProductId);
            item.TaskId.Should().Be(create.TaskId);
            item.QuoteId.Should().Be(create.QuoteId);
            item.InteractionId.Should().Be(create.InteractionId);
            item.NoteId.Should().Be(create.NoteId);
            item.OldValue.Should().Be(create.OldValue);
            item.NewValue.Should().Be(create.NewValue);
            item.FieldsChanged.Should().Be(create.FieldsChanged);
            item.IsSystem.Should().Be(create.IsSystem);
            item.IsPrivate.Should().Be(create.IsPrivate);
            item.IsImportant.Should().Be(create.IsImportant);
            item.Tags.Should().Be(create.Tags);
            item.Category.Should().Be(create.Category);
            item.Source.Should().Be(create.Source);
            item.IsDeleted.Should().Be(create.IsDeleted);

            var getRes = await _client.GetAsync($"/api/activities/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new { ActivityType = 1, Title = "Test2", Description = "Test", Details = "Test", ActivityDate = DateTime.UtcNow, DurationMinutes = 1, UserId = 1, UserName = "Test", UserEmail = "Test", EntityType = "Test", EntityId = 1, EntityName = "Test", SecondaryEntityType = "Test", SecondaryEntityId = 1, SecondaryEntityName = "Test", AccountId = 1, ContactId = 1, OpportunityId = 1, CampaignId = 1, ProductId = 1, TaskId = 1, QuoteId = 1, InteractionId = 1, NoteId = 1, OldValue = "Test", NewValue = "Test", FieldsChanged = "Test", IsSystem = true, IsPrivate = true, IsImportant = true, Tags = "Test", Category = "Test", Source = "Test", IsDeleted = true };
            var pRes = await _client.PatchAsJsonAsync($"/api/activities/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/activities/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/activities/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/activities/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

