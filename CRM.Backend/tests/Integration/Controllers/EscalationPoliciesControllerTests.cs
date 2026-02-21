using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class EscalationPoliciesControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public EscalationPoliciesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_EscalationPolicies_Succeeds()
        {
            var create = new { Name = "Test", Description = "Test", InitialAssignmentMinutes = 1, MaxEscalationLevels = 1, IsActive = true, NotifyDuringEscalation = true, Name = "Test", Description = "Test", InitialAssignmentMinutes = 1, MaxEscalationLevels = 1, IsActive = true, NotifyDuringEscalation = true, Name = "Test", Description = "Test", InitialAssignmentMinutes = 1, MaxEscalationLevels = 1, IsActive = true, NotifyDuringEscalation = true, PolicyId = 1, Level = 1, EscalationAfterMinutes = 1, EscalateToUserId = 1, EscalateToGroupId = 1, NotificationTemplate = "Test", SendNotification = true, Level = 1, EscalationAfterMinutes = 1, EscalateToUserId = 1, EscalateToGroupId = 1, NotificationTemplate = "Test", SendNotification = true, TicketId = 1, PolicyId = 1, Level = 1, EscalatedAt = DateTime.UtcNow, EscalatedToUserId = 1, EscalatedToGroupId = 1, Reason = "Test", Notes = "Test" };
            var cRes = await _client.PostAsJsonAsync("/api/escalationpolicies", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.InitialAssignmentMinutes.Should().Be(create.InitialAssignmentMinutes);
            item.MaxEscalationLevels.Should().Be(create.MaxEscalationLevels);
            item.IsActive.Should().Be(create.IsActive);
            item.NotifyDuringEscalation.Should().Be(create.NotifyDuringEscalation);
            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.InitialAssignmentMinutes.Should().Be(create.InitialAssignmentMinutes);
            item.MaxEscalationLevels.Should().Be(create.MaxEscalationLevels);
            item.IsActive.Should().Be(create.IsActive);
            item.NotifyDuringEscalation.Should().Be(create.NotifyDuringEscalation);
            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.InitialAssignmentMinutes.Should().Be(create.InitialAssignmentMinutes);
            item.MaxEscalationLevels.Should().Be(create.MaxEscalationLevels);
            item.IsActive.Should().Be(create.IsActive);
            item.NotifyDuringEscalation.Should().Be(create.NotifyDuringEscalation);
            item.PolicyId.Should().Be(create.PolicyId);
            item.Level.Should().Be(create.Level);
            item.EscalationAfterMinutes.Should().Be(create.EscalationAfterMinutes);
            item.EscalateToUserId.Should().Be(create.EscalateToUserId);
            item.EscalateToGroupId.Should().Be(create.EscalateToGroupId);
            item.NotificationTemplate.Should().Be(create.NotificationTemplate);
            item.SendNotification.Should().Be(create.SendNotification);
            item.Level.Should().Be(create.Level);
            item.EscalationAfterMinutes.Should().Be(create.EscalationAfterMinutes);
            item.EscalateToUserId.Should().Be(create.EscalateToUserId);
            item.EscalateToGroupId.Should().Be(create.EscalateToGroupId);
            item.NotificationTemplate.Should().Be(create.NotificationTemplate);
            item.SendNotification.Should().Be(create.SendNotification);
            item.TicketId.Should().Be(create.TicketId);
            item.PolicyId.Should().Be(create.PolicyId);
            item.Level.Should().Be(create.Level);
            item.EscalatedAt.Should().Be(create.EscalatedAt);
            item.EscalatedToUserId.Should().Be(create.EscalatedToUserId);
            item.EscalatedToGroupId.Should().Be(create.EscalatedToGroupId);
            item.Reason.Should().Be(create.Reason);
            item.Notes.Should().Be(create.Notes);

            var getRes = await _client.GetAsync($"/api/escalationpolicies/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new { Name = "Test2", Description = "Test", InitialAssignmentMinutes = 1, MaxEscalationLevels = 1, IsActive = true, NotifyDuringEscalation = true, Name = "Test2", Description = "Test", InitialAssignmentMinutes = 1, MaxEscalationLevels = 1, IsActive = true, NotifyDuringEscalation = true, Name = "Test2", Description = "Test", InitialAssignmentMinutes = 1, MaxEscalationLevels = 1, IsActive = true, NotifyDuringEscalation = true, PolicyId = 1, Level = 1, EscalationAfterMinutes = 1, EscalateToUserId = 1, EscalateToGroupId = 1, NotificationTemplate = "Test", SendNotification = true, Level = 1, EscalationAfterMinutes = 1, EscalateToUserId = 1, EscalateToGroupId = 1, NotificationTemplate = "Test", SendNotification = true, TicketId = 1, PolicyId = 1, Level = 1, EscalatedAt = DateTime.UtcNow, EscalatedToUserId = 1, EscalatedToGroupId = 1, Reason = "Test", Notes = "Test" };
            var pRes = await _client.PatchAsJsonAsync($"/api/escalationpolicies/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/escalationpolicies/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/escalationpolicies/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/escalationpolicies/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

