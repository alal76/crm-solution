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
    public class UsersControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public UsersControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Users_Succeeds()
        {
            var create = new
            {
                Username = "Test",
                Email = "Test",
                FirstName = "Test",
                LastName = "Test",
                Role = "Test",
                IsActive = true,
                IsLocked = true,
                DepartmentId = 1,
                DepartmentName = "Test",
                UserProfileId = 1,
                UserProfileName = "Test",
                PrimaryGroupId = 1,
                PrimaryGroupName = "Test",
                ContactId = 1,
                ContactName = "Test",
                ContactEmail = "Test",
                LastLoginDate = DateTime.UtcNow,
                HeaderColor = "Test",
                PhotoUrl = "Test",
                Password = "Test",
                RoleId = 1
            };
            var cRes = await _client.PostAsJsonAsync("/api/users", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/users/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                Username = "Test2",
                Email = "Test",
                FirstName = "Test",
                LastName = "Test",
                Role = "Test",
                IsActive = true,
                IsLocked = true,
                DepartmentId = 1,
                DepartmentName = "Test",
                UserProfileId = 1,
                UserProfileName = "Test",
                PrimaryGroupId = 1,
                PrimaryGroupName = "Test",
                ContactId = 1,
                ContactName = "Test",
                ContactEmail = "Test",
                LastLoginDate = DateTime.UtcNow,
                HeaderColor = "Test",
                PhotoUrl = "Test",
                Password = "Test",
                RoleId = 1
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/users/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/users/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/users/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/users/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
