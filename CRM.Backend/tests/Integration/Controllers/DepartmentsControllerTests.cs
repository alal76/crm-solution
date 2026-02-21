using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class DepartmentsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public DepartmentsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Departments_Succeeds()
        {
            var create = new
            {
                Name = "Test",
                Description = "Test",
                DepartmentCode = "Test",
                ParentDepartmentId = 1,
                IsActive = true,
                UserCount = 1
            };
            var cRes = await _client.PostAsJsonAsync("/api/departments", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.DepartmentCode.Should().Be(create.DepartmentCode);
            item.ParentDepartmentId.Should().Be(create.ParentDepartmentId);
            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.DepartmentCode.Should().Be(create.DepartmentCode);
            item.IsActive.Should().Be(create.IsActive);
            item.ParentDepartmentId.Should().Be(create.ParentDepartmentId);
            item.UserCount.Should().Be(create.UserCount);

            var getRes = await _client.GetAsync($"/api/departments/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                Name = "Test2",
                Description = "Test",
                DepartmentCode = "Test",
                ParentDepartmentId = 1,
                IsActive = true,
                UserCount = 1
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/departments/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/departments/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/departments/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/departments/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

