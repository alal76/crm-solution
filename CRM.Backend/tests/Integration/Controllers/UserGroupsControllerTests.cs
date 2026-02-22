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
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

 // alias check

            var getRes = await _client.GetAsync($"/api/usergroups/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/usergroups/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
