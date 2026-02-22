using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class UserGroupsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public UserGroupsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_UserGroups_Succeeds()
        {
            var create = new
            {
                Name = "Test",
                Description = "Test",
                IsActive = true,
                IsDefault = true,
                DisplayOrder = 1,
                HeaderColor = "Test",
                IsSystemAdmin = true,
                MemberCount = 1,
                CanAccessDashboard = true,
                CanAccessAccounts = true,
                // Customers alias handled automatically
                CanAccessContacts = true,
                CanAccessLeads = true,
                CanAccessOpportunities = true,
                CanAccessProducts = true,
                CanAccessServices = true,
                CanAccessCampaigns = true,
                CanAccessQuotes = true,
                CanAccessTasks = true,
                CanAccessActivities = true,
                CanAccessNotes = true,
                CanAccessWorkflows = true,
                CanAccessServiceRequests = true,
                CanAccessITSM = true,
                CanAccessReports = true,
                CanAccessSettings = true,
                CanAccessUserManagement = true,
                CanCreateAccounts = true,
                CanEditAccounts = true,
                CanDeleteAccounts = true,
                CanViewAllAccounts = true,
                CanCreateContacts = true,
                CanEditContacts = true,
                CanDeleteContacts = true,
                CanCreateLeads = true,
                CanEditLeads = true,
                CanDeleteLeads = true,
                CanConvertLeads = true,
                CanCreateOpportunities = true,
                CanEditOpportunities = true,
                CanDeleteOpportunities = true,
                CanCloseOpportunities = true,
                CanCreateProducts = true,
                CanEditProducts = true,
                CanDeleteProducts = true,
                CanManagePricing = true,
                CanCreateCampaigns = true,
                CanEditCampaigns = true,
                CanDeleteCampaigns = true,
                CanLaunchCampaigns = true,
                CanCreateQuotes = true,
                CanEditQuotes = true,
                CanDeleteQuotes = true,
                CanApproveQuotes = true,
                CanCreateTasks = true,
                CanEditTasks = true,
                CanDeleteTasks = true,
                CanAssignTasks = true,
                CanCreateWorkflows = true,
                CanEditWorkflows = true,
                CanDeleteWorkflows = true,
                CanActivateWorkflows = true,
                DataAccessScope = "Test",
                CanExportData = true,
                CanImportData = true,
                CanBulkEdit = true,
                CanBulkDelete = true,
                UserId = 1,
                Email = "Test",
                FullName = "Test",
                GroupId = 1,
                AddedAt = DateTime.UtcNow
            };
            var cRes = await _client.PostAsJsonAsync("/api/usergroups", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = (await cRes.Content.ReadFromJsonAsync<dynamic>())!;

            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.IsActive.Should().Be(create.IsActive);
            item.IsDefault.Should().Be(create.IsDefault);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.HeaderColor.Should().Be(create.HeaderColor);
            item.IsSystemAdmin.Should().Be(create.IsSystemAdmin);
            item.MemberCount.Should().Be(create.MemberCount);
            item.CanAccessDashboard.Should().Be(create.CanAccessDashboard);
            item.CanAccessAccounts.Should().Be(create.CanAccessAccounts);
            item.CanAccessCustomers.Should().Be(item.CanAccessAccounts); // alias check
            item.CanAccessContacts.Should().Be(create.CanAccessContacts);
            item.CanAccessLeads.Should().Be(create.CanAccessLeads);
            item.CanAccessOpportunities.Should().Be(create.CanAccessOpportunities);
            item.CanAccessProducts.Should().Be(create.CanAccessProducts);
            item.CanAccessServices.Should().Be(create.CanAccessServices);
            item.CanAccessCampaigns.Should().Be(create.CanAccessCampaigns);
            item.CanAccessQuotes.Should().Be(create.CanAccessQuotes);
            item.CanAccessTasks.Should().Be(create.CanAccessTasks);
            item.CanAccessActivities.Should().Be(create.CanAccessActivities);
            item.CanAccessNotes.Should().Be(create.CanAccessNotes);
            item.CanAccessWorkflows.Should().Be(create.CanAccessWorkflows);
            item.CanAccessServiceRequests.Should().Be(create.CanAccessServiceRequests);
            item.CanAccessITSM.Should().Be(create.CanAccessITSM);
            item.CanAccessReports.Should().Be(create.CanAccessReports);
            item.CanAccessSettings.Should().Be(create.CanAccessSettings);
            item.CanAccessUserManagement.Should().Be(create.CanAccessUserManagement);
            item.CanCreateAccounts.Should().Be(create.CanCreateAccounts);
            item.CanEditAccounts.Should().Be(create.CanEditAccounts);
            item.CanDeleteAccounts.Should().Be(create.CanDeleteAccounts);
            item.CanViewAllAccounts.Should().Be(create.CanViewAllAccounts);
            item.CanCreateContacts.Should().Be(create.CanCreateContacts);
            item.CanEditContacts.Should().Be(create.CanEditContacts);
            item.CanDeleteContacts.Should().Be(create.CanDeleteContacts);
            item.CanCreateLeads.Should().Be(create.CanCreateLeads);
            item.CanEditLeads.Should().Be(create.CanEditLeads);
            item.CanDeleteLeads.Should().Be(create.CanDeleteLeads);
            item.CanConvertLeads.Should().Be(create.CanConvertLeads);
            item.CanCreateOpportunities.Should().Be(create.CanCreateOpportunities);
            item.CanEditOpportunities.Should().Be(create.CanEditOpportunities);
            item.CanDeleteOpportunities.Should().Be(create.CanDeleteOpportunities);
            item.CanCloseOpportunities.Should().Be(create.CanCloseOpportunities);
            item.CanCreateProducts.Should().Be(create.CanCreateProducts);
            item.CanEditProducts.Should().Be(create.CanEditProducts);
            item.CanDeleteProducts.Should().Be(create.CanDeleteProducts);
            item.CanManagePricing.Should().Be(create.CanManagePricing);
            item.CanCreateCampaigns.Should().Be(create.CanCreateCampaigns);
            item.CanEditCampaigns.Should().Be(create.CanEditCampaigns);
            item.CanDeleteCampaigns.Should().Be(create.CanDeleteCampaigns);
            item.CanLaunchCampaigns.Should().Be(create.CanLaunchCampaigns);
            item.CanCreateQuotes.Should().Be(create.CanCreateQuotes);
            item.CanEditQuotes.Should().Be(create.CanEditQuotes);
            item.CanDeleteQuotes.Should().Be(create.CanDeleteQuotes);
            item.CanApproveQuotes.Should().Be(create.CanApproveQuotes);
            item.CanCreateTasks.Should().Be(create.CanCreateTasks);
            item.CanEditTasks.Should().Be(create.CanEditTasks);
            item.CanDeleteTasks.Should().Be(create.CanDeleteTasks);
            item.CanAssignTasks.Should().Be(create.CanAssignTasks);
            item.CanCreateWorkflows.Should().Be(create.CanCreateWorkflows);
            item.CanEditWorkflows.Should().Be(create.CanEditWorkflows);
            item.CanDeleteWorkflows.Should().Be(create.CanDeleteWorkflows);
            item.CanActivateWorkflows.Should().Be(create.CanActivateWorkflows);
            item.DataAccessScope.Should().Be(create.DataAccessScope);
            item.CanExportData.Should().Be(create.CanExportData);
            item.CanImportData.Should().Be(create.CanImportData);
            item.CanBulkEdit.Should().Be(create.CanBulkEdit);
            item.CanBulkDelete.Should().Be(create.CanBulkDelete);
            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.IsActive.Should().Be(create.IsActive);
            item.IsDefault.Should().Be(create.IsDefault);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.HeaderColor.Should().Be(create.HeaderColor);
            item.IsSystemAdmin.Should().Be(create.IsSystemAdmin);
            item.CanAccessDashboard.Should().Be(create.CanAccessDashboard);
            item.CanAccessAccounts.Should().Be(create.CanAccessAccounts);
            item.CanAccessCustomers.Should().Be(item.CanAccessAccounts);
            item.CanAccessContacts.Should().Be(create.CanAccessContacts);
            item.CanAccessLeads.Should().Be(create.CanAccessLeads);
            item.CanAccessOpportunities.Should().Be(create.CanAccessOpportunities);
            item.CanAccessProducts.Should().Be(create.CanAccessProducts);
            item.CanAccessServices.Should().Be(create.CanAccessServices);
            item.CanAccessCampaigns.Should().Be(create.CanAccessCampaigns);
            item.CanAccessQuotes.Should().Be(create.CanAccessQuotes);
            item.CanAccessTasks.Should().Be(create.CanAccessTasks);
            item.CanAccessActivities.Should().Be(create.CanAccessActivities);
            item.CanAccessNotes.Should().Be(create.CanAccessNotes);
            item.CanAccessWorkflows.Should().Be(create.CanAccessWorkflows);
            item.CanAccessServiceRequests.Should().Be(create.CanAccessServiceRequests);
            item.CanAccessITSM.Should().Be(create.CanAccessITSM);
            item.CanAccessReports.Should().Be(create.CanAccessReports);
            item.CanAccessSettings.Should().Be(create.CanAccessSettings);
            item.CanAccessUserManagement.Should().Be(create.CanAccessUserManagement);
            item.CanCreateAccounts.Should().Be(create.CanCreateAccounts);
            item.CanEditAccounts.Should().Be(create.CanEditAccounts);
            item.CanDeleteAccounts.Should().Be(create.CanDeleteAccounts);
            item.CanViewAllAccounts.Should().Be(create.CanViewAllAccounts);
            item.CanCreateContacts.Should().Be(create.CanCreateContacts);
            item.CanEditContacts.Should().Be(create.CanEditContacts);
            item.CanDeleteContacts.Should().Be(create.CanDeleteContacts);
            item.CanCreateLeads.Should().Be(create.CanCreateLeads);
            item.CanEditLeads.Should().Be(create.CanEditLeads);
            item.CanDeleteLeads.Should().Be(create.CanDeleteLeads);
            item.CanConvertLeads.Should().Be(create.CanConvertLeads);
            item.CanCreateOpportunities.Should().Be(create.CanCreateOpportunities);
            item.CanEditOpportunities.Should().Be(create.CanEditOpportunities);
            item.CanDeleteOpportunities.Should().Be(create.CanDeleteOpportunities);
            item.CanCloseOpportunities.Should().Be(create.CanCloseOpportunities);
            item.CanCreateProducts.Should().Be(create.CanCreateProducts);
            item.CanEditProducts.Should().Be(create.CanEditProducts);
            item.CanDeleteProducts.Should().Be(create.CanDeleteProducts);
            item.CanManagePricing.Should().Be(create.CanManagePricing);
            item.CanCreateCampaigns.Should().Be(create.CanCreateCampaigns);
            item.CanEditCampaigns.Should().Be(create.CanEditCampaigns);
            item.CanDeleteCampaigns.Should().Be(create.CanDeleteCampaigns);
            item.CanLaunchCampaigns.Should().Be(create.CanLaunchCampaigns);
            item.CanCreateQuotes.Should().Be(create.CanCreateQuotes);
            item.CanEditQuotes.Should().Be(create.CanEditQuotes);
            item.CanDeleteQuotes.Should().Be(create.CanDeleteQuotes);
            item.CanApproveQuotes.Should().Be(create.CanApproveQuotes);
            item.CanCreateTasks.Should().Be(create.CanCreateTasks);
            item.CanEditTasks.Should().Be(create.CanEditTasks);
            item.CanDeleteTasks.Should().Be(create.CanDeleteTasks);
            item.CanAssignTasks.Should().Be(create.CanAssignTasks);
            item.CanCreateWorkflows.Should().Be(create.CanCreateWorkflows);
            item.CanEditWorkflows.Should().Be(create.CanEditWorkflows);
            item.CanDeleteWorkflows.Should().Be(create.CanDeleteWorkflows);
            item.CanActivateWorkflows.Should().Be(create.CanActivateWorkflows);
            item.DataAccessScope.Should().Be(create.DataAccessScope);
            item.CanExportData.Should().Be(create.CanExportData);
            item.CanImportData.Should().Be(create.CanImportData);
            item.CanBulkEdit.Should().Be(create.CanBulkEdit);
            item.CanBulkDelete.Should().Be(create.CanBulkDelete);
            item.UserId.Should().Be(create.UserId);
            item.Email.Should().Be(create.Email);
            item.FullName.Should().Be(create.FullName);
            item.GroupId.Should().Be(create.GroupId);
            item.AddedAt.Should().Be(create.AddedAt);

            var getRes = await _client.GetAsync($"/api/usergroups/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                Name = "Test2",
                Description = "Test",
                IsActive = true,
                IsDefault = true,
                DisplayOrder = 1,
                HeaderColor = "Test",
                IsSystemAdmin = true,
                MemberCount = 1,
                CanAccessDashboard = true,
                CanAccessAccounts = true,
                // legacy Customers flag omitted
                CanAccessContacts = true,
                CanAccessLeads = true,
                CanAccessOpportunities = true,
                CanAccessProducts = true,
                CanAccessServices = true,
                CanAccessCampaigns = true,
                CanAccessQuotes = true,
                CanAccessTasks = true,
                CanAccessActivities = true,
                CanAccessNotes = true,
                CanAccessWorkflows = true,
                CanAccessServiceRequests = true,
                CanAccessITSM = true,
                CanAccessReports = true,
                CanAccessSettings = true,
                CanAccessUserManagement = true,
                CanCreateAccounts = true,
                CanEditAccounts = true,
                CanDeleteAccounts = true,
                CanViewAllAccounts = true,
                CanCreateContacts = true,
                CanEditContacts = true,
                CanDeleteContacts = true,
                CanCreateLeads = true,
                CanEditLeads = true,
                CanDeleteLeads = true,
                CanConvertLeads = true,
                CanCreateOpportunities = true,
                CanEditOpportunities = true,
                CanDeleteOpportunities = true,
                CanCloseOpportunities = true,
                CanCreateProducts = true,
                CanEditProducts = true,
                CanDeleteProducts = true,
                CanManagePricing = true,
                CanCreateCampaigns = true,
                CanEditCampaigns = true,
                CanDeleteCampaigns = true,
                CanLaunchCampaigns = true,
                CanCreateQuotes = true,
                CanEditQuotes = true,
                CanDeleteQuotes = true,
                CanApproveQuotes = true,
                CanCreateTasks = true,
                CanEditTasks = true,
                CanDeleteTasks = true,
                CanAssignTasks = true,
                CanCreateWorkflows = true,
                CanEditWorkflows = true,
                CanDeleteWorkflows = true,
                CanActivateWorkflows = true,
                DataAccessScope = "Test",
                CanExportData = true,
                CanImportData = true,
                CanBulkEdit = true,
                CanBulkDelete = true,
                UserId = 1,
                Email = "Test",
                FullName = "Test",
                GroupId = 1,
                AddedAt = DateTime.UtcNow
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/usergroups/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/usergroups/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/usergroups/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/usergroups/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

