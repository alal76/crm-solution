using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class LookupsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public LookupsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task GetCategoriesAndItems_WorkAsDesigned()
        {
            // categories endpoint should return 200 and array
            var cats = await _client.GetAsync("/api/lookups/categories");
            cats.StatusCode.Should().Be(HttpStatusCode.OK);

            // try a nonexistent category name for items should yield 404
            var items = await _client.GetAsync("/api/lookups/items/doesnotexist");
            items.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

