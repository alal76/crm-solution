using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
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
            var item = (await cRes.Content.ReadFromJsonAsync<dynamic>())!;

            item.ModuleName.Should().Be(create.ModuleName);
            item.IsEnabled.Should().Be(create.IsEnabled);
            item.Description.Should().Be(create.Description);
            item.IconName.Should().Be(create.IconName);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.TabsConfig.Should().Be(create.TabsConfig);
            item.LinkedEntitiesConfig.Should().Be(create.LinkedEntitiesConfig);
            item.ListViewConfig.Should().Be(create.ListViewConfig);
            item.DetailViewConfig.Should().Be(create.DetailViewConfig);
            item.QuickCreateConfig.Should().Be(create.QuickCreateConfig);
            item.SearchFilterConfig.Should().Be(create.SearchFilterConfig);
            item.ModuleSettings.Should().Be(create.ModuleSettings);
            item.ModuleName.Should().Be(create.ModuleName);
            item.IsEnabled.Should().Be(create.IsEnabled);
            item.Description.Should().Be(create.Description);
            item.IconName.Should().Be(create.IconName);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.TabsConfig.Should().Be(create.TabsConfig);
            item.LinkedEntitiesConfig.Should().Be(create.LinkedEntitiesConfig);
            item.ListViewConfig.Should().Be(create.ListViewConfig);
            item.DetailViewConfig.Should().Be(create.DetailViewConfig);
            item.QuickCreateConfig.Should().Be(create.QuickCreateConfig);
            item.SearchFilterConfig.Should().Be(create.SearchFilterConfig);
            item.ModuleSettings.Should().Be(create.ModuleSettings);
            item.IsEnabled.Should().Be(create.IsEnabled);
            item.Description.Should().Be(create.Description);
            item.IconName.Should().Be(create.IconName);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.TabsConfig.Should().Be(create.TabsConfig);
            item.LinkedEntitiesConfig.Should().Be(create.LinkedEntitiesConfig);
            item.ListViewConfig.Should().Be(create.ListViewConfig);
            item.DetailViewConfig.Should().Be(create.DetailViewConfig);
            item.QuickCreateConfig.Should().Be(create.QuickCreateConfig);
            item.SearchFilterConfig.Should().Be(create.SearchFilterConfig);
            item.ModuleSettings.Should().Be(create.ModuleSettings);
            item.Index.Should().Be(create.Index);
            item.Name.Should().Be(create.Name);
            item.Enabled.Should().Be(create.Enabled);
            item.Order.Should().Be(create.Order);
            item.Icon.Should().Be(create.Icon);
            item.EntityName.Should().Be(create.EntityName);
            item.RelationshipType.Should().Be(create.RelationshipType);
            item.Enabled.Should().Be(create.Enabled);
            item.TabName.Should().Be(create.TabName);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.ForeignKeyField.Should().Be(create.ForeignKeyField);
            item.Field.Should().Be(create.Field);
            item.Label.Should().Be(create.Label);
            item.Width.Should().Be(create.Width);
            item.Visible.Should().Be(create.Visible);
            item.Sortable.Should().Be(create.Sortable);
            item.Order.Should().Be(create.Order);
            item.Format.Should().Be(create.Format);
            item.IsEnabled.Should().Be(create.IsEnabled);
            item.IsRequired.Should().Be(create.IsRequired);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.GridSize.Should().Be(create.GridSize);
            item.FieldType.Should().Be(create.FieldType);
            item.FieldLabel.Should().Be(create.FieldLabel);
            item.Placeholder.Should().Be(create.Placeholder);
            item.HelpText.Should().Be(create.HelpText);
            item.Options.Should().Be(create.Options);
            item.ModuleName.Should().Be(create.ModuleName);
            item.IsEnabled.Should().Be(create.IsEnabled);
            item.DisplayOrder.Should().Be(create.DisplayOrder);

            var getRes = await _client.GetAsync($"/api/moduleuiconfig/{{item.Id}}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/moduleuiconfig/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/moduleuiconfig/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/moduleuiconfig/{{item.Id}}");
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

