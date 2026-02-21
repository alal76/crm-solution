// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class NavigationControllerTests
{
    private readonly Mock<INavigationConfigService> _service = new();
    private readonly Mock<ILogger<NavigationController>> _logger = new();

    private NavigationController CreateControllerWithUser(int userId = 1)
    {
        var controller = new NavigationController(_service.Object, _logger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }, "Test"))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetNavigationConfig_ReturnsOk()
    {
        _service.Setup(s => s.GetNavigationConfigAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NavigationConfig());

        var controller = new NavigationController(_service.Object, _logger.Object);

        var result = await controller.GetNavigationConfig(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAvailableNavItems_ReturnsOk()
    {
        _service.Setup(s => s.GetAvailableNavItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NavigationItemConfig>());

        var controller = new NavigationController(_service.Object, _logger.Object);

        var result = await controller.GetAvailableNavItems(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUserNavigationConfig_ReturnsOk_WhenAuthenticated()
    {
        _service.Setup(s => s.GetNavigationConfigForUserAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NavigationConfig());

        var controller = CreateControllerWithUser(1);

        var result = await controller.GetUserNavigationConfig(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }
}
