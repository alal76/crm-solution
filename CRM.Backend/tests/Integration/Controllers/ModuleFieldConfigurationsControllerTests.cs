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
    public class ModuleFieldConfigurationsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public ModuleFieldConfigurationsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_ModuleFieldConfigurations_Succeeds()
        {
            var create = new
            {
                ModuleName = "Test",
                FieldName = "Test",
                FieldLabel = "Test",
                FieldType = "Test",
                TabIndex = 1,
                TabName = "Test",
                DisplayOrder = 1,
                IsEnabled = true,
                IsRequired = true,
                GridSize = 1,
                Placeholder = "Test",
                HelpText = "Test",
                Options = "Test",
                ParentField = "Test",
                ParentFieldValue = "Test",
                IsReorderable = true,
                IsRequiredConfigurable = true,
                IsHideable = true
            };
            var cRes = await _client.PostAsJsonAsync("/api/modulefieldconfigurations", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/modulefieldconfigurations/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                ModuleName = "Test2",
                FieldName = "Test",
                FieldLabel = "Test",
                FieldType = "Test",
                TabIndex = 1,
                TabName = "Test",
                DisplayOrder = 1,
                IsEnabled = true,
                IsRequired = true,
                GridSize = 1,
                Placeholder = "Test",
                HelpText = "Test",
                Options = "Test",
                ParentField = "Test",
                ParentFieldValue = "Test",
                IsReorderable = true,
                IsRequiredConfigurable = true,
                IsHideable = true
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/modulefieldconfigurations/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/modulefieldconfigurations/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/modulefieldconfigurations/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/modulefieldconfigurations/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
