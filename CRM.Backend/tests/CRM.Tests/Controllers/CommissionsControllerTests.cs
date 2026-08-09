// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TCOV2-D13 — CommissionsController unit tests
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for CommissionsController (TCOV2-D13).
/// Route: /api/commissions
/// </summary>
public class CommissionsControllerTests
{
    private readonly Mock<ICommissionService> _mockService;
    private readonly Mock<ICommissionRulesEngine> _mockRulesEngine;
    private readonly Mock<ICommissionRuleService> _mockRuleService;
    private readonly Mock<IOpportunityService> _mockOpportunityService;
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly CommissionsController _controller;

    public CommissionsControllerTests()
    {
        _mockService = new Mock<ICommissionService>();
        _mockRulesEngine = new Mock<ICommissionRulesEngine>();
        _mockRuleService = new Mock<ICommissionRuleService>();
        _mockOpportunityService = new Mock<IOpportunityService>();
        _mockOrderService = new Mock<IOrderService>();
        var mockLogger = new Mock<ILogger<CommissionsController>>();
        _controller = new CommissionsController(
            _mockService.Object,
            _mockRulesEngine.Object,
            _mockRuleService.Object,
            _mockOpportunityService.Object,
            _mockOrderService.Object,
            mockLogger.Object);
    }

    private static Commission MakeCommission(int id = 1) => new()
    {
        Id = id,
        UserId = 1,
        CommissionNumber = $"COM-{id:D5}",
        DealAmount = 50_000m,
        CommissionAmount = 5_000m,
        Status = CommissionStatus.Pending
    };

    // ── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithCommissions()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int?>(), It.IsAny<CommissionStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Commission> { MakeCommission(1), MakeCommission(2) });

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenEmpty()
    {
        _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int?>(), It.IsAny<CommissionStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Commission>());

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetById ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCommissionExists()
    {
        // Arrange — commission with null User (handled by controller gracefully)
        var commission = MakeCommission(5);
        commission.User = null;
        _mockService.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(commission);

        // Act
        var result = await _controller.GetById(5, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCommissionMissing()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Commission?)null);

        // Act
        var result = await _controller.GetById(999, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── service interaction ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_CallsServiceWithCorrectId()
    {
        _mockService.Setup(s => s.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(MakeCommission(42));

        await _controller.GetById(42, CancellationToken.None);

        _mockService.Verify(s => s.GetByIdAsync(42, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Commission Rules ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetRules_ShouldReturnOk()
    {
        _mockRuleService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommissionRuleDto> { new() { Id = 1, Name = "Standard" } });

        var result = await _controller.GetRules(CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRuleById_ShouldReturnNotFound_WhenMissing()
    {
        _mockRuleService.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommissionRuleDto?)null);

        var result = await _controller.GetRuleById(99, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateRule_ShouldReturnCreated_WhenValid()
    {
        var dto = new CreateCommissionRuleDto { Name = "New Rule", SaleType = "Standard", Rate = 5m };
        _mockRuleService.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommissionRuleDto { Id = 10, Name = dto.Name });

        var result = await _controller.CreateRule(dto, CancellationToken.None);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task DeleteRule_ShouldReturnNotFound_WhenMissing()
    {
        _mockRuleService.Setup(s => s.DeleteAsync(123, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Commission rule with ID 123 not found"));

        var result = await _controller.DeleteRule(123, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
