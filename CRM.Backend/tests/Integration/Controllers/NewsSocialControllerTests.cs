using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
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
            var cRes = await _client.PostAsJsonAsync("/api/newssocial", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = (await cRes.Content.ReadFromJsonAsync<dynamic>())!;

            item.Title.Should().Be(create.Title);
            item.Source.Should().Be(create.Source);
            item.Author.Should().Be(create.Author);
            item.Url.Should().Be(create.Url);
            item.ImageUrl.Should().Be(create.ImageUrl);
            item.PublishedAt.Should().Be(create.PublishedAt);
            item.Summary.Should().Be(create.Summary);
            item.Sentiment.Should().Be(create.Sentiment);
            item.Platform.Should().Be(create.Platform);
            item.Content.Should().Be(create.Content);
            item.Author.Should().Be(create.Author);
            item.AuthorHandle.Should().Be(create.AuthorHandle);
            item.AuthorImageUrl.Should().Be(create.AuthorImageUrl);
            item.PublishedAt.Should().Be(create.PublishedAt);
            item.Url.Should().Be(create.Url);
            item.EngagementCount.Should().Be(create.EngagementCount);
            item.LikeCount.Should().Be(create.LikeCount);
            item.ShareCount.Should().Be(create.ShareCount);
            item.CommentCount.Should().Be(create.CommentCount);
            item.LastUpdated.Should().Be(create.LastUpdated);
            item.Error.Should().Be(create.Error);
            item.IsFromCache.Should().Be(create.IsFromCache);
            item.AccountId.Should().Be(create.AccountId);
            item.CompanyName.Should().Be(create.CompanyName);
            item.LinkedInUrl.Should().Be(create.LinkedInUrl);
            item.TwitterHandle.Should().Be(create.TwitterHandle);
            item.FacebookUrl.Should().Be(create.FacebookUrl);
            item.RefreshCache.Should().Be(create.RefreshCache);
            item.MaxNewsItems.Should().Be(create.MaxNewsItems);
            item.MaxSocialItems.Should().Be(create.MaxSocialItems);

            var getRes = await _client.GetAsync($"/api/newssocial/{{item.Id}}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/newssocial/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/newssocial/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/newssocial/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/newssocial/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

