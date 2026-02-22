using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class AnalyticsEventsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public AnalyticsEventsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Create_AnalyticsEvent_ReturnsCreated()
        {
            var create = new { EventName = "TestEvent", EntityType = "Account", EntityId = 1, UserId = (int?)null, Timestamp = DateTime.UtcNow, Metadata = (string?)null };
            var cRes = await _client.PostAsJsonAsync("/api/analytics-events", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetAll_AnalyticsEvents_ReturnsOk()
        {
            var res = await _client.GetAsync("/api/analytics-events");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/analytics-events/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

