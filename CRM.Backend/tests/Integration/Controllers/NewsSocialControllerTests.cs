// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class NewsSocialControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public NewsSocialControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_NewsSocial_Succeeds()
        {
            var create = new
            {
                Title = "Test",
                Source = "Test",
                Author = "Test",
                Url = "Test",
                ImageUrl = "Test",
                PublishedAt = DateTime.UtcNow,
                Summary = "Test",
                Sentiment = "Test",
                Platform = "Test",
                Content = "Test",
                AuthorHandle = "Test",
                AuthorImageUrl = "Test",
                EngagementCount = 1,
                LikeCount = 1,
                ShareCount = 1,
                CommentCount = 1,
                LastUpdated = DateTime.UtcNow,
                Error = "Test",
                IsFromCache = true,
                AccountId = 1,
                CompanyName = "Test",
                LinkedInUrl = "Test",
                TwitterHandle = "Test",
                FacebookUrl = "Test",
                RefreshCache = true,
                MaxNewsItems = 1,
                MaxSocialItems = 1
            };
            var cRes = await _client.PostAsJsonAsync("/api/news-social", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/news-social/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                Title = "Test2",
                Source = "Test",
                Author = "Test",
                Url = "Test",
                ImageUrl = "Test",
                PublishedAt = DateTime.UtcNow,
                Summary = "Test",
                Sentiment = "Test",
                Platform = "Test",
                Content = "Test",
                AuthorHandle = "Test",
                AuthorImageUrl = "Test",
                EngagementCount = 1,
                LikeCount = 1,
                ShareCount = 1,
                CommentCount = 1,
                LastUpdated = DateTime.UtcNow,
                Error = "Test",
                IsFromCache = true,
                AccountId = 1,
                CompanyName = "Test",
                LinkedInUrl = "Test",
                TwitterHandle = "Test",
                FacebookUrl = "Test",
                RefreshCache = true,
                MaxNewsItems = 1,
                MaxSocialItems = 1
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/news-social/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/news-social/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/news-social/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/news-social/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
