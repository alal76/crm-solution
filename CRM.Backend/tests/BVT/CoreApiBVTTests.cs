using System.Net;
using System.Net.Http.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.BVT;

public class CoreApiBVTTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public CoreApiBVTTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Accounts_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/accounts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Contacts_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/contacts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Leads_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/leads");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Opportunities_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/opportunities");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Users_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Accounts_Create_ReturnsCreated()
    {
        var payload = new CreateAccountDto
        {
            Category = AccountCategory.Individual,
            FirstName = "Test",
            LastName = "Account",
            Email = "test.account@crm.local",
            Phone = "555-0100"
        };

        var response = await _client.PostAsJsonAsync("/api/accounts", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<AccountDto>();
        created.Should().NotBeNull();
        created!.Email.Should().Be(payload.Email);
    }
}
