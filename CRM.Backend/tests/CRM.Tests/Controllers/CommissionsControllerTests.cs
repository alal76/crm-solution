// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TCOV2-D13 — CommissionsController unit tests
using CRM.Api.Controllers;
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
    private readonly Mock<IOpportunityService> _mockOpportunityService;
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly CommissionsController _controller;

    public CommissionsControllerTests()
    {
        _mockService = new Mock<ICommissionService>();
        _mockRulesEngine = new Mock<ICommissionRulesEngine>();
        _mockOpportunityService = new Mock<IOpportunityService>();
        _mockOrderService = new Mock<IOrderService>();
        var mockLogger = new Mock<ILogger<CommissionsController>>();
        _controller = new CommissionsController(
            _mockService.Object,
            _mockRulesEngine.Object,
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
}
