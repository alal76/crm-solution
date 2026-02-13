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

using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class AnalyticsControllerTests
{
    private readonly Mock<IProviderFactory<IAnalyticsPort>> _factory;
    private readonly Mock<IAnalyticsPort> _provider;
    private readonly AnalyticsController _controller;

    public AnalyticsControllerTests()
    {
        _factory = new Mock<IProviderFactory<IAnalyticsPort>>();
        _provider = new Mock<IAnalyticsPort>();

        _factory.Setup(f => f.GetProvider()).Returns(_provider.Object);
        _provider.Setup(p => p.ProviderName).Returns("Superset");
        _provider.Setup(p => p.SupportsEmbedding).Returns(true);
        _provider.Setup(p => p.IsAvailableAsync(default)).ReturnsAsync(true);

        _controller = new AnalyticsController(_factory.Object, new Mock<ILogger<AnalyticsController>>().Object);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "user@crm.local"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task GetDashboards_ReturnsOk_WithDashboards()
    {
        _provider.Setup(p => p.GetDashboardsForUserAsync(1, It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync(new[] { new DashboardInfo { Id = "1", Name = "Executive" } });

        var result = await _controller.GetDashboards();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDashboardEmbed_ReturnsOk_WithEmbedPayload()
    {
        _provider.Setup(p => p.GetEmbedAsync(It.IsAny<EmbedRequest>(), default))
            .ReturnsAsync(new EmbedResult { EmbedUrl = "https://embed", Token = "token" });

        var result = await _controller.GetDashboardEmbed("1");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }
}
