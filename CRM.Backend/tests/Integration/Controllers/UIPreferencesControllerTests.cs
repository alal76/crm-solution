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
            var cRes = await _client.PostAsJsonAsync("/api/ui-preferences", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/ui-preferences/{id}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/ui-preferences/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/ui-preferences/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/ui-preferences/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/ui-preferences/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
