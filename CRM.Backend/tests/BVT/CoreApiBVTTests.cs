// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
