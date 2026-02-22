using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class EmailSequencesControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "/api/email-sequences";
        public EmailSequencesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_EmailSequences_Succeeds()
        {
            // POST - Create (controller takes raw EmailSequence entity)
            var create = new { Name = "Test Sequence", Description = "Integration test", IsActive = true };
            var cRes = await _client.PostAsJsonAsync(BaseUrl, create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = await cRes.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var id = doc.RootElement.GetProperty("id").GetInt32();

            // GET by ID
            var getRes = await _client.GetAsync($"{BaseUrl}/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);

            // PUT - Update (controller uses PUT, not PATCH)
            var update = new { Name = "Updated Sequence", Description = "Updated", IsActive = true };
            var pRes = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", update);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);

            // DELETE
            var del = await _client.DeleteAsync($"{BaseUrl}/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Confirm deleted
            var nf = await _client.GetAsync($"{BaseUrl}/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync($"{BaseUrl}/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var res = await _client.GetAsync(BaseUrl);
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_WithMinimalPayload_ReturnsCreated()
        {
            var create = new { Name = "Minimal Sequence" };
            var res = await _client.PostAsJsonAsync(BaseUrl, create);
            res.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Create_WithNullBody_ReturnsBadRequest()
        {
            var res = await _client.PostAsJsonAsync<object?>(BaseUrl, null);
            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}

