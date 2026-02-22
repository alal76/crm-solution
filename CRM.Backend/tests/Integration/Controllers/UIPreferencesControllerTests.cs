using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class UIPreferencesControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public UIPreferencesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_UIPreferences_Succeeds()
        {
            var create = new
            {
                UserId = 1,
                Theme = "Test",
                SidebarPosition = "Test",
                SidebarWidth = 1,
                FontSize = "Test",
                ShowBreadcrumbs = true,
                ShowStatusBar = true,
                ShowTopNavigation = true,
                DefaultPageSize = 1,
                DateFormat = "Test",
                TimeFormat = "Test",
                CustomColorScheme = "Test",
                LastPreferenceUpdate = DateTime.UtcNow
            };
            var cRes = await _client.PostAsJsonAsync("/api/uipreferences", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = (await cRes.Content.ReadFromJsonAsync<dynamic>())!;

            item.UserId.Should().Be(create.UserId);
            item.Theme.Should().Be(create.Theme);
            item.SidebarPosition.Should().Be(create.SidebarPosition);
            item.SidebarWidth.Should().Be(create.SidebarWidth);
            item.FontSize.Should().Be(create.FontSize);
            item.ShowBreadcrumbs.Should().Be(create.ShowBreadcrumbs);
            item.ShowStatusBar.Should().Be(create.ShowStatusBar);
            item.ShowTopNavigation.Should().Be(create.ShowTopNavigation);
            item.DefaultPageSize.Should().Be(create.DefaultPageSize);
            item.DateFormat.Should().Be(create.DateFormat);
            item.TimeFormat.Should().Be(create.TimeFormat);
            item.CustomColorScheme.Should().Be(create.CustomColorScheme);
            item.LastPreferenceUpdate.Should().Be(create.LastPreferenceUpdate);
            item.Theme.Should().Be(create.Theme);
            item.SidebarPosition.Should().Be(create.SidebarPosition);
            item.SidebarWidth.Should().Be(create.SidebarWidth);
            item.FontSize.Should().Be(create.FontSize);
            item.ShowBreadcrumbs.Should().Be(create.ShowBreadcrumbs);
            item.ShowStatusBar.Should().Be(create.ShowStatusBar);
            item.ShowTopNavigation.Should().Be(create.ShowTopNavigation);
            item.DefaultPageSize.Should().Be(create.DefaultPageSize);
            item.DateFormat.Should().Be(create.DateFormat);
            item.TimeFormat.Should().Be(create.TimeFormat);
            item.CustomColorScheme.Should().Be(create.CustomColorScheme);

            var getRes = await _client.GetAsync($"/api/uipreferences/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                UserId = 1,
                Theme = "Test2",
                SidebarPosition = "Test",
                SidebarWidth = 1,
                FontSize = "Test",
                ShowBreadcrumbs = true,
                ShowStatusBar = true,
                ShowTopNavigation = true,
                DefaultPageSize = 1,
                DateFormat = "Test",
                TimeFormat = "Test",
                CustomColorScheme = "Test",
                LastPreferenceUpdate = DateTime.UtcNow
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/uipreferences/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/uipreferences/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/uipreferences/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/uipreferences/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

