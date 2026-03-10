// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for DashboardConfigController (TCOV-043).
/// </summary>
public class DashboardConfigControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly DashboardConfigController _controller;

    private static ClaimsPrincipal MakeUser(int userId = 1, string role = "User")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    public DashboardConfigControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"DashboardConfigTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        var logger = new Mock<ILogger<DashboardConfigController>>();
        _controller = new DashboardConfigController(_dbContext, logger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = MakeUser() }
        };
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task GetDashboards_ShouldReturnOk_WhenNoDashboards()
    {
        var result = await _controller.GetDashboards();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDashboards_ShouldReturnOk_WithPublicDashboard()
    {
        _dbContext.Dashboards.Add(new Dashboard
        {
            Name = "Main Dashboard", IsActive = true, IsDeleted = false,
            Visibility = DashboardVisibility.Public, OwnerId = 1,
            DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetDashboards();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnNotFound_WhenNotExists()
    {
        var result = await _controller.GetDashboard(9999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnOk_WhenExists()
    {
        var dashboard = new Dashboard
        {
            Name = "My Dashboard", IsActive = true, IsDeleted = false,
            Visibility = DashboardVisibility.Public, OwnerId = 1,
            DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Dashboards.Add(dashboard);
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetDashboard(dashboard.Id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetWidgetTypes_ShouldReturnOk()
    {
        var result = _controller.GetWidgetTypes();

        result.Should().BeOfType<OkObjectResult>();
    }
}
