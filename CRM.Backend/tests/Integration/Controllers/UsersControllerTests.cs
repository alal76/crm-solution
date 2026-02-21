using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
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
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.Username.Should().Be(create.Username);
            item.Email.Should().Be(create.Email);
            item.FirstName.Should().Be(create.FirstName);
            item.LastName.Should().Be(create.LastName);
            item.Role.Should().Be(create.Role);
            item.IsActive.Should().Be(create.IsActive);
            item.IsLocked.Should().Be(create.IsLocked);
            item.DepartmentId.Should().Be(create.DepartmentId);
            item.DepartmentName.Should().Be(create.DepartmentName);
            item.UserProfileId.Should().Be(create.UserProfileId);
            item.UserProfileName.Should().Be(create.UserProfileName);
            item.PrimaryGroupId.Should().Be(create.PrimaryGroupId);
            item.PrimaryGroupName.Should().Be(create.PrimaryGroupName);
            item.ContactId.Should().Be(create.ContactId);
            item.ContactName.Should().Be(create.ContactName);
            item.ContactEmail.Should().Be(create.ContactEmail);
            item.LastLoginDate.Should().Be(create.LastLoginDate);
            item.HeaderColor.Should().Be(create.HeaderColor);
            item.PhotoUrl.Should().Be(create.PhotoUrl);
            item.Email.Should().Be(create.Email);
            item.FirstName.Should().Be(create.FirstName);
            item.LastName.Should().Be(create.LastName);
            item.Password.Should().Be(create.Password);
            item.RoleId.Should().Be(create.RoleId);
            item.DepartmentId.Should().Be(create.DepartmentId);
            item.PrimaryGroupId.Should().Be(create.PrimaryGroupId);

            var getRes = await _client.GetAsync($"/api/users/{{item.Id}}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/users/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/users/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/users/{{item.Id}}");
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

