// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Net.Http.Json;
using CRM.Core.DTOs;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class AccountsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public AccountsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Account_Succeeds()
        {
            var create = new { name = "Test Co" };
            var cRes = await _client.PostAsJsonAsync("/api/accounts", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var account = await cRes.Content.ReadFromJsonAsync<AccountDto>();

            var getRes = await _client.GetAsync($"/api/accounts/{account.Id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);

            var patch = new { name = "Test Co 2" };
            var pRes = await _client.PatchAsJsonAsync($"/api/accounts/{account.Id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);

            var del = await _client.DeleteAsync($"/api/accounts/{account.Id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var nf = await _client.GetAsync($"/api/accounts/{account.Id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/accounts/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
