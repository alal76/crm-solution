using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
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
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.ModuleName.Should().Be(create.ModuleName);
            item.FieldName.Should().Be(create.FieldName);
            item.FieldLabel.Should().Be(create.FieldLabel);
            item.FieldType.Should().Be(create.FieldType);
            item.TabIndex.Should().Be(create.TabIndex);
            item.TabName.Should().Be(create.TabName);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.IsEnabled.Should().Be(create.IsEnabled);
            item.IsRequired.Should().Be(create.IsRequired);
            item.GridSize.Should().Be(create.GridSize);
            item.Placeholder.Should().Be(create.Placeholder);
            item.HelpText.Should().Be(create.HelpText);
            item.Options.Should().Be(create.Options);
            item.ParentField.Should().Be(create.ParentField);
            item.ParentFieldValue.Should().Be(create.ParentFieldValue);
            item.IsReorderable.Should().Be(create.IsReorderable);
            item.IsRequiredConfigurable.Should().Be(create.IsRequiredConfigurable);
            item.IsHideable.Should().Be(create.IsHideable);
            item.ModuleName.Should().Be(create.ModuleName);
            item.FieldName.Should().Be(create.FieldName);
            item.FieldLabel.Should().Be(create.FieldLabel);
            item.FieldType.Should().Be(create.FieldType);
            item.TabIndex.Should().Be(create.TabIndex);
            item.TabName.Should().Be(create.TabName);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.IsEnabled.Should().Be(create.IsEnabled);
            item.IsRequired.Should().Be(create.IsRequired);
            item.GridSize.Should().Be(create.GridSize);
            item.Placeholder.Should().Be(create.Placeholder);
            item.HelpText.Should().Be(create.HelpText);
            item.Options.Should().Be(create.Options);
            item.ParentField.Should().Be(create.ParentField);
            item.ParentFieldValue.Should().Be(create.ParentFieldValue);
            item.IsReorderable.Should().Be(create.IsReorderable);
            item.IsRequiredConfigurable.Should().Be(create.IsRequiredConfigurable);
            item.IsHideable.Should().Be(create.IsHideable);
            item.FieldLabel.Should().Be(create.FieldLabel);
            item.TabIndex.Should().Be(create.TabIndex);
            item.TabName.Should().Be(create.TabName);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.IsEnabled.Should().Be(create.IsEnabled);
            item.IsRequired.Should().Be(create.IsRequired);
            item.GridSize.Should().Be(create.GridSize);
            item.Placeholder.Should().Be(create.Placeholder);
            item.HelpText.Should().Be(create.HelpText);
            item.Options.Should().Be(create.Options);
            item.ModuleName.Should().Be(create.ModuleName);
            item.TabIndex.Should().Be(create.TabIndex);
            item.DisplayOrder.Should().Be(create.DisplayOrder);

            var getRes = await _client.GetAsync($"/api/modulefieldconfigurations/{{item.Id}}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/modulefieldconfigurations/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/modulefieldconfigurations/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/modulefieldconfigurations/{{item.Id}}");
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

