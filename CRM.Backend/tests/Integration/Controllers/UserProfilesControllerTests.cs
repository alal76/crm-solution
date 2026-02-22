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
    public class UserProfilesControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public UserProfilesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_UserProfiles_Succeeds()
        {
            var create = new
            {
                Name = "Test",
                Description = "Test",
                DepartmentId = 1,
                CanCreateAccounts = true,
                CanEditAccounts = true,
                CanDeleteAccounts = true,
                CanCreateOpportunities = true,
                CanEditOpportunities = true,
                CanDeleteOpportunities = true,
                CanCreateProducts = true,
                CanEditProducts = true,
                CanDeleteProducts = true,
                CanManageCampaigns = true,
                CanViewReports = true,
                CanManageUsers = true,
                DepartmentName = "Test",
                IsActive = true,
                UserCount = 1
            };
            var cRes = await _client.PostAsJsonAsync("/api/userprofiles", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/userprofiles/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                Name = "Test2",
                Description = "Test",
                DepartmentId = 1,
                CanCreateAccounts = true,
                CanEditAccounts = true,
                CanDeleteAccounts = true,
                CanCreateOpportunities = true,
                CanEditOpportunities = true,
                CanDeleteOpportunities = true,
                CanCreateProducts = true,
                CanEditProducts = true,
                CanDeleteProducts = true,
                CanManageCampaigns = true,
                CanViewReports = true,
                CanManageUsers = true,
                DepartmentName = "Test",
                IsActive = true,
                UserCount = 1
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/userprofiles/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/userprofiles/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/userprofiles/{id}");
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
