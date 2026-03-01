// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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

        _controller = new AnalyticsController(_factory.Object);

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
