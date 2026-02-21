using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
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
            var cRes = await _client.PostAsJsonAsync("/api/escalationrules", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.Priority.Should().Be(create.Priority);
            item.Category.Should().Be(create.Category);
            item.Queue.Should().Be(create.Queue);
            item.AgeInMinutes.Should().Be(create.AgeInMinutes);
            item.TargetType.Should().Be(create.TargetType);
            item.TargetId.Should().Be(create.TargetId);
            item.TargetName.Should().Be(create.TargetName);
            item.MaxAttempts.Should().Be(create.MaxAttempts);
            item.RetryIntervalMinutes.Should().Be(create.RetryIntervalMinutes);
            item.IsActive.Should().Be(create.IsActive);
            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.Priority.Should().Be(create.Priority);
            item.Category.Should().Be(create.Category);
            item.Queue.Should().Be(create.Queue);
            item.AgeInMinutes.Should().Be(create.AgeInMinutes);
            item.TargetType.Should().Be(create.TargetType);
            item.TargetId.Should().Be(create.TargetId);
            item.TargetName.Should().Be(create.TargetName);
            item.MaxAttempts.Should().Be(create.MaxAttempts);
            item.RetryIntervalMinutes.Should().Be(create.RetryIntervalMinutes);
            item.IsActive.Should().Be(create.IsActive);
            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.Priority.Should().Be(create.Priority);
            item.Category.Should().Be(create.Category);
            item.Queue.Should().Be(create.Queue);
            item.AgeInMinutes.Should().Be(create.AgeInMinutes);
            item.TargetType.Should().Be(create.TargetType);
            item.TargetId.Should().Be(create.TargetId);
            item.TargetName.Should().Be(create.TargetName);
            item.MaxAttempts.Should().Be(create.MaxAttempts);
            item.RetryIntervalMinutes.Should().Be(create.RetryIntervalMinutes);
            item.IsActive.Should().Be(create.IsActive);
            item.RuleId.Should().Be(create.RuleId);
            item.ServiceRequestId.Should().Be(create.ServiceRequestId);
            item.RuleMatched.Should().Be(create.RuleMatched);
            item.MatchReason.Should().Be(create.MatchReason);
            item.Rule.Should().Be(create.Rule);
            item.TestMessage.Should().Be(create.TestMessage);

            var getRes = await _client.GetAsync($"/api/escalationrules/{{item.Id}}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/escalationrules/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/escalationrules/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/escalationrules/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/escalationrules/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

