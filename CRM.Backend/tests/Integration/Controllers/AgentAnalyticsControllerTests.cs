using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class AgentAnalyticsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public AgentAnalyticsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task GetEndpoints_AgentAnalytics_Work()
        {
            var resp = await _client.GetAsync("/api/agentanalytics/usage");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var resp1 = await _client.GetAsync("/api/agentanalytics/accuracy");
            resp1.StatusCode.Should().Be(HttpStatusCode.OK);
            var resp2 = await _client.GetAsync("/api/agentanalytics/cost");
            resp2.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
