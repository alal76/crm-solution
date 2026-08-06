// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TCOV2-D10 — AnalyticsController unit tests
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for AnalyticsController (TCOV2-D10).
/// Route: /api/analytics
/// Tests: dashboards available (200), provider unavailable (503), dashboard not found (404).
/// </summary>
public class AnalyticsControllerTests
{
    private readonly Mock<IProviderFactory<IAnalyticsPort>> _mockFactory;
    private readonly Mock<IAnalyticsPort> _mockProvider;
    private readonly AnalyticsController _controller;

    public AnalyticsControllerTests()
    {
        _mockFactory = new Mock<IProviderFactory<IAnalyticsPort>>();
        _mockProvider = new Mock<IAnalyticsPort>();

        _mockFactory.Setup(f => f.GetProvider()).Returns(_mockProvider.Object);

        _controller = new AnalyticsController(_mockFactory.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Role, "Admin")
                    }, "TestAuth"))
            }
        };
    }

    // ── GetDashboards ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboards_ShouldReturnOk_WhenProviderAvailable()
    {
        // Arrange
        _mockProvider.Setup(p => p.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockProvider.Setup(p => p.GetDashboardsForUserAsync(
                It.IsAny<int>(), It.IsAny<IEnumerable<string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DashboardInfo>
            {
                new() { Id = "dash-1", Name = "Sales Dashboard", Url = "http://analytics/1" }
            });

        // Act
        var result = await _controller.GetDashboards();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDashboards_ShouldReturn503_WhenProviderUnavailable()
    {
        // Arrange
        _mockProvider.Setup(p => p.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _controller.GetDashboards();

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    // ── GetDashboard (by id) ──────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboard_ShouldReturnOk_WhenDashboardExists()
    {
        // Arrange
        var dashboard = new DashboardInfo { Id = "dash-1", Name = "Revenue" };
        _mockProvider.Setup(p => p.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockProvider.Setup(p => p.GetDashboardAsync("dash-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dashboard);

        // Act
        var result = await _controller.GetDashboard("dash-1");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().Be(dashboard);
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnNotFound_WhenDashboardMissing()
    {
        // Arrange
        _mockProvider.Setup(p => p.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockProvider.Setup(p => p.GetDashboardAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DashboardInfo?)null);

        // Act
        var result = await _controller.GetDashboard("missing");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetDashboard_ShouldReturn503_WhenProviderUnavailable()
    {
        // Arrange
        _mockProvider.Setup(p => p.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _controller.GetDashboard("dash-1");

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }
}
