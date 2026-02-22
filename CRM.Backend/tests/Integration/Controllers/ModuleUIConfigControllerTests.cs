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
    public class ModuleUIConfigControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public ModuleUIConfigControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_ModuleUIConfig_Succeeds()
        {
            var create = new
            {
                ModuleName = "Test",
                IsEnabled = true,
                Description = "Test",
                IconName = "Test",
                DisplayOrder = 1,
                TabsConfig = "Test",
                LinkedEntitiesConfig = "Test",
                ListViewConfig = "Test",
                DetailViewConfig = "Test",
                QuickCreateConfig = "Test",
                SearchFilterConfig = "Test",
                ModuleSettings = "Test",
                Index = 1,
                Name = "Test",
                Enabled = true,
                Order = 1,
                Icon = "Test",
                EntityName = "Test",
                RelationshipType = "Test",
                TabName = "Test",
                ForeignKeyField = "Test",
                Field = "Test",
                Label = "Test",
                Width = 1,
                Visible = true,
                Sortable = true,
                Format = "Test",
                IsRequired = true,
                GridSize = 1,
                FieldType = "Test",
                FieldLabel = "Test",
                Placeholder = "Test",
                HelpText = "Test",
                Options = "Test"
            };
            var cRes = await _client.PostAsJsonAsync("/api/moduleuiconfig", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/moduleuiconfig/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                ModuleName = "Test2",
                IsEnabled = true,
                Description = "Test",
                IconName = "Test",
                DisplayOrder = 1,
                TabsConfig = "Test",
                LinkedEntitiesConfig = "Test",
                ListViewConfig = "Test",
                DetailViewConfig = "Test",
                QuickCreateConfig = "Test",
                SearchFilterConfig = "Test",
                ModuleSettings = "Test",
                Index = 1,
                Name = "Test",
                Enabled = true,
                Order = 1,
                Icon = "Test",
                EntityName = "Test",
                RelationshipType = "Test",
                TabName = "Test",
                ForeignKeyField = "Test",
                Field = "Test",
                Label = "Test",
                Width = 1,
                Visible = true,
                Sortable = true,
                Format = "Test",
                IsRequired = true,
                GridSize = 1,
                FieldType = "Test",
                FieldLabel = "Test",
                Placeholder = "Test",
                HelpText = "Test",
                Options = "Test"
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/moduleuiconfig/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/moduleuiconfig/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/moduleuiconfig/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/moduleuiconfig/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
