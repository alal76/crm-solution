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
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/colorpalettes/{id}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/colorpalettes/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/colorpalettes/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/colorpalettes/{id}");
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
