// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

/// <summary>
/// Unit tests for AdminConfigurationController.
/// Tests commission rules, discount rules, and SLA policy endpoints.
/// Covers TODO-SYS008-002.
/// </summary>
public class AdminConfigurationControllerTests
{
    private readonly Mock<IAdminConfigurationService> _mockAdminConfigService;
    private readonly Mock<ILogger<AdminConfigurationController>> _mockLogger;
    private readonly AdminConfigurationController _controller;

    public AdminConfigurationControllerTests()
    {
        _mockAdminConfigService = new Mock<IAdminConfigurationService>();
        _mockLogger = new Mock<ILogger<AdminConfigurationController>>();
        _controller = new AdminConfigurationController(
            _mockAdminConfigService.Object,
            _mockLogger.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region Commission Rules Tests

    [Fact]
    public async Task GetCommissionRules_ShouldReturnOk_WithRuleList()
    {
        // Arrange
        var rules = new List<CommissionRuleDto>
        {
            new CommissionRuleDto { Id = 1, Name = "Standard Rate", Rate = 5m },
            new CommissionRuleDto { Id = 2, Name = "Premium Rate", Rate = 8m }
        };
        _mockAdminConfigService
            .Setup(s => s.GetCommissionRulesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _controller.GetCommissionRules(cancellationToken: CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRules = okResult.Value as IEnumerable<CommissionRuleDto>;
        returnedRules.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCommissionRuleById_ShouldReturnOk_WhenRuleExists()
    {
        // Arrange
        var rule = new CommissionRuleDto { Id = 1, Name = "Standard Rate", Rate = 5m };
        _mockAdminConfigService
            .Setup(s => s.GetCommissionRuleByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        var result = await _controller.GetCommissionRuleById(1, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRule = okResult.Value as CommissionRuleDto;
        returnedRule!.Id.Should().Be(1);
        returnedRule.Name.Should().Be("Standard Rate");
    }

    [Fact]
    public async Task GetCommissionRuleById_ShouldReturnNotFound_WhenRuleDoesNotExist()
    {
        // Arrange
        _mockAdminConfigService
            .Setup(s => s.GetCommissionRuleByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommissionRuleDto?)null);

        // Act
        var result = await _controller.GetCommissionRuleById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateCommissionRule_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateCommissionRuleDto
        {
            Name = "New Rule",
            Rate = 7m
        };
        var created = new CommissionRuleDto { Id = 5, Name = "New Rule", Rate = 7m };
        _mockAdminConfigService
            .Setup(s => s.CreateCommissionRuleAsync(request, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.CreateCommissionRule(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var returnedRule = createdResult.Value as CommissionRuleDto;
        returnedRule!.Id.Should().Be(5);
    }

    [Fact]
    public async Task CreateCommissionRule_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Act
        var result = await _controller.CreateCommissionRule(null!, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateCommissionRule_ShouldReturnOk_WhenRuleExists()
    {
        // Arrange
        var request = new UpdateCommissionRuleDto { Name = "Updated Rule", Rate = 9m };
        var updated = new CommissionRuleDto { Id = 1, Name = "Updated Rule", Rate = 9m };
        _mockAdminConfigService
            .Setup(s => s.UpdateCommissionRuleAsync(1, request, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.UpdateCommissionRule(1, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRule = okResult.Value as CommissionRuleDto;
        returnedRule!.Rate.Should().Be(9m);
    }

    [Fact]
    public async Task UpdateCommissionRule_ShouldReturnNotFound_WhenRuleDoesNotExist()
    {
        // Arrange
        var request = new UpdateCommissionRuleDto { Name = "Updated Rule" };
        _mockAdminConfigService
            .Setup(s => s.UpdateCommissionRuleAsync(999, request, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommissionRuleDto?)null);

        // Act
        var result = await _controller.UpdateCommissionRule(999, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteCommissionRule_ShouldReturnNoContent_WhenRuleExists()
    {
        // Arrange
        _mockAdminConfigService
            .Setup(s => s.DeleteCommissionRuleAsync(1, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCommissionRule(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteCommissionRule_ShouldReturnNotFound_WhenRuleDoesNotExist()
    {
        // Arrange
        _mockAdminConfigService
            .Setup(s => s.DeleteCommissionRuleAsync(999, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCommissionRule(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Discount Rules Tests

    [Fact]
    public async Task GetDiscountRules_ShouldReturnOk_WithRuleList()
    {
        // Arrange
        var rules = new List<DiscountRuleDto>
        {
            new DiscountRuleDto { Id = 1, Name = "Volume Discount" },
            new DiscountRuleDto { Id = 2, Name = "Partner Discount" }
        };
        _mockAdminConfigService
            .Setup(s => s.GetDiscountRulesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _controller.GetDiscountRules(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRules = okResult.Value as IEnumerable<DiscountRuleDto>;
        returnedRules.Should().HaveCount(2);
    }

    #endregion

    #region SLA Policy Tests

    [Fact]
    public async Task GetSLAPolicies_ShouldReturnOk_WithPolicyList()
    {
        // Arrange
        var policies = new List<SLAPolicyDto>
        {
            new SLAPolicyDto { Id = 1, Name = "Gold SLA" },
            new SLAPolicyDto { Id = 2, Name = "Silver SLA" }
        };
        _mockAdminConfigService
            .Setup(s => s.GetSLAPoliciesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);

        // Act
        var result = await _controller.GetSLAPolicies(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPolicies = okResult.Value as IEnumerable<SLAPolicyDto>;
        returnedPolicies.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSLAPolicyById_ShouldReturnNotFound_WhenPolicyDoesNotExist()
    {
        // Arrange
        _mockAdminConfigService
            .Setup(s => s.GetSLAPolicyByIdAsync(404, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SLAPolicyDto?)null);

        // Act
        var result = await _controller.GetSLAPolicyById(404, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
