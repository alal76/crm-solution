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
        public async Task Create_AuditLog_ReturnsCreated()
        {
            var create = new { Action = "TestAction", EntityType = "Account", EntityId = 1, UserId = 1, Details = "Integration test" };
            var cRes = await _client.PostAsJsonAsync("/api/audit-logs", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetAll_AuditLogs_ReturnsOk()
        {
            var res = await _client.GetAsync("/api/audit-logs");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

