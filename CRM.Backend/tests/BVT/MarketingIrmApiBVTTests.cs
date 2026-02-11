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
using CRM.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.BVT;

public class MarketingIrmApiBVTTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public MarketingIrmApiBVTTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Campaigns_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/campaigns");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EmailTemplates_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/emailtemplates");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EmailSequences_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/emailsequences");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ServiceRequests_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/servicerequests");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Knowledge_GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/knowledge");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
