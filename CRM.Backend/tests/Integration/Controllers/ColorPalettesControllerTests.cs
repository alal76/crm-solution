using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class ColorPalettesControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public ColorPalettesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_ColorPalettes_Succeeds()
        {
            var create = new
            {
                Name = "Test",
                Description = "Test",
                PrimaryColor = "Test",
                SecondaryColor = "Test",
                SuccessColor = "Test",
                WarningColor = "Test",
                ErrorColor = "Test",
                InfoColor = "Test",
                BackgroundLight = "Test",
                BackgroundDark = "Test",
                TextLight = "Test",
                TextDark = "Test",
                BorderColor = "Test",
                IsDefault = true,
                IsActive = true,
                Category = "Test",
                IsUserDefined = true
            };
            var cRes = await _client.PostAsJsonAsync("/api/colorpalettes", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = (await cRes.Content.ReadFromJsonAsync<dynamic>())!;

            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.PrimaryColor.Should().Be(create.PrimaryColor);
            item.SecondaryColor.Should().Be(create.SecondaryColor);
            item.SuccessColor.Should().Be(create.SuccessColor);
            item.WarningColor.Should().Be(create.WarningColor);
            item.ErrorColor.Should().Be(create.ErrorColor);
            item.InfoColor.Should().Be(create.InfoColor);
            item.BackgroundLight.Should().Be(create.BackgroundLight);
            item.BackgroundDark.Should().Be(create.BackgroundDark);
            item.TextLight.Should().Be(create.TextLight);
            item.TextDark.Should().Be(create.TextDark);
            item.BorderColor.Should().Be(create.BorderColor);
            item.IsDefault.Should().Be(create.IsDefault);
            item.IsActive.Should().Be(create.IsActive);
            item.Category.Should().Be(create.Category);
            item.IsUserDefined.Should().Be(create.IsUserDefined);
            item.Name.Should().Be(create.Name);

            var getRes = await _client.GetAsync($"/api/colorpalettes/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                Name = "Test2",
                Description = "Test",
                PrimaryColor = "Test",
                SecondaryColor = "Test",
                SuccessColor = "Test",
                WarningColor = "Test",
                ErrorColor = "Test",
                InfoColor = "Test",
                BackgroundLight = "Test",
                BackgroundDark = "Test",
                TextLight = "Test",
                TextDark = "Test",
                BorderColor = "Test",
                IsDefault = true,
                IsActive = true,
                Category = "Test",
                IsUserDefined = true
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/colorpalettes/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/colorpalettes/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/colorpalettes/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/colorpalettes/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

