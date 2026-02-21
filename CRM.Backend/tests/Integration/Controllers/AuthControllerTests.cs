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
    public class AuthControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public AuthControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            var dto = new { email = "admin@crm.local", password = "Admin@123" };
            var res = await _client.PostAsJsonAsync("/api/auth/login", dto);
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await res.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.False(string.IsNullOrEmpty(payload?.AccessToken));
        }

        [Fact]
        public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
        {
            var res = await _client.PostAsJsonAsync("/api/auth/refresh", new { token = "bad" });
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }
    }
}
