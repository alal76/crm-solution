using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class ImportJobsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public ImportJobsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Create_ImportJob_ReturnsCreated()
        {
            var create = new { Entity = "Accounts", Source = "CSV", Status = "Completed", SubmittedByUserId = (int?)null, SubmittedDate = (string?)null };
            var cRes = await _client.PostAsJsonAsync("/api/import-jobs", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetAll_ImportJobs_ReturnsOk()
        {
            var res = await _client.GetAsync("/api/import-jobs");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/import-jobs/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

