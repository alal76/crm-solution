using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class UserProfilesControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public UserProfilesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_UserProfiles_Succeeds()
        {
            var create = new { Name = "Test", Description = "Test", DepartmentId = 1, CanCreateAccounts = true, CanEditAccounts = true, CanDeleteAccounts = true, CanCreateOpportunities = true, CanEditOpportunities = true, CanDeleteOpportunities = true, CanCreateProducts = true, CanEditProducts = true, CanDeleteProducts = true, CanManageCampaigns = true, CanViewReports = true, CanManageUsers = true, Name = "Test", Description = "Test", DepartmentId = 1, DepartmentName = "Test", IsActive = true, CanCreateAccounts = true, CanEditAccounts = true, CanDeleteAccounts = true, CanCreateOpportunities = true, CanEditOpportunities = true, CanDeleteOpportunities = true, CanCreateProducts = true, CanEditProducts = true, CanDeleteProducts = true, CanManageCampaigns = true, CanViewReports = true, CanManageUsers = true, UserCount = 1 };
            var cRes = await _client.PostAsJsonAsync("/api/userprofiles", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.DepartmentId.Should().Be(create.DepartmentId);
            item.CanCreateAccounts.Should().Be(create.CanCreateAccounts);
            item.CanEditAccounts.Should().Be(create.CanEditAccounts);
            item.CanDeleteAccounts.Should().Be(create.CanDeleteAccounts);
            item.CanCreateOpportunities.Should().Be(create.CanCreateOpportunities);
            item.CanEditOpportunities.Should().Be(create.CanEditOpportunities);
            item.CanDeleteOpportunities.Should().Be(create.CanDeleteOpportunities);
            item.CanCreateProducts.Should().Be(create.CanCreateProducts);
            item.CanEditProducts.Should().Be(create.CanEditProducts);
            item.CanDeleteProducts.Should().Be(create.CanDeleteProducts);
            item.CanManageCampaigns.Should().Be(create.CanManageCampaigns);
            item.CanViewReports.Should().Be(create.CanViewReports);
            item.CanManageUsers.Should().Be(create.CanManageUsers);
            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.DepartmentId.Should().Be(create.DepartmentId);
            item.DepartmentName.Should().Be(create.DepartmentName);
            item.IsActive.Should().Be(create.IsActive);
            item.CanCreateAccounts.Should().Be(create.CanCreateAccounts);
            item.CanEditAccounts.Should().Be(create.CanEditAccounts);
            item.CanDeleteAccounts.Should().Be(create.CanDeleteAccounts);
            item.CanCreateOpportunities.Should().Be(create.CanCreateOpportunities);
            item.CanEditOpportunities.Should().Be(create.CanEditOpportunities);
            item.CanDeleteOpportunities.Should().Be(create.CanDeleteOpportunities);
            item.CanCreateProducts.Should().Be(create.CanCreateProducts);
            item.CanEditProducts.Should().Be(create.CanEditProducts);
            item.CanDeleteProducts.Should().Be(create.CanDeleteProducts);
            item.CanManageCampaigns.Should().Be(create.CanManageCampaigns);
            item.CanViewReports.Should().Be(create.CanViewReports);
            item.CanManageUsers.Should().Be(create.CanManageUsers);
            item.UserCount.Should().Be(create.UserCount);

            var getRes = await _client.GetAsync($"/api/userprofiles/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new { Name = "Test2", Description = "Test", DepartmentId = 1, CanCreateAccounts = true, CanEditAccounts = true, CanDeleteAccounts = true, CanCreateOpportunities = true, CanEditOpportunities = true, CanDeleteOpportunities = true, CanCreateProducts = true, CanEditProducts = true, CanDeleteProducts = true, CanManageCampaigns = true, CanViewReports = true, CanManageUsers = true, Name = "Test2", Description = "Test", DepartmentId = 1, DepartmentName = "Test", IsActive = true, CanCreateAccounts = true, CanEditAccounts = true, CanDeleteAccounts = true, CanCreateOpportunities = true, CanEditOpportunities = true, CanDeleteOpportunities = true, CanCreateProducts = true, CanEditProducts = true, CanDeleteProducts = true, CanManageCampaigns = true, CanViewReports = true, CanManageUsers = true, UserCount = 1 };
            var pRes = await _client.PatchAsJsonAsync($"/api/userprofiles/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/userprofiles/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/userprofiles/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/userprofiles/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

