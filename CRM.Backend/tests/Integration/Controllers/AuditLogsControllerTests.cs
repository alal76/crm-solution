using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class AuditLogsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public AuditLogsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_AuditLogs_Succeeds()
        {
            var create = new { Action = "Test", EntityType = "Test", EntityId = 1, UserId = 1, Details = "Test", Timestamp = DateTime.UtcNow };
            var cRes = await _client.PostAsJsonAsync("/api/auditlogs", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.Action.Should().Be(create.Action);
            item.EntityType.Should().Be(create.EntityType);
            item.EntityId.Should().Be(create.EntityId);
            item.UserId.Should().Be(create.UserId);
            item.Details.Should().Be(create.Details);
            item.Timestamp.Should().Be(create.Timestamp);

            var getRes = await _client.GetAsync($"/api/auditlogs/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new { Action = "Test2", EntityType = "Test", EntityId = 1, UserId = 1, Details = "Test", Timestamp = DateTime.UtcNow };
            var pRes = await _client.PatchAsJsonAsync($"/api/auditlogs/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/auditlogs/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/auditlogs/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/auditlogs/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

