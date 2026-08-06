// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TCOV2-D11 — TerritoriesController unit tests
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
/// Unit tests for TerritoriesController (TCOV2-D11).
/// Route: /api/territories
/// </summary>
public class TerritoriesControllerTests
{
    private readonly Mock<ITerritoryService> _mockService;
    private readonly TerritoriesController _controller;

    public TerritoriesControllerTests()
    {
        _mockService = new Mock<ITerritoryService>();
        var mockLogger = new Mock<ILogger<TerritoriesController>>();
        _controller = new TerritoriesController(_mockService.Object, mockLogger.Object);
    }

    private static AccountTerritory MakeTerritory(int id = 1) => new()
    {
        Id = id,
        TerritoryName = $"Territory {id}",
        TerritoryCode = $"T{id:D3}",
        IsActive = true
    };

    // ── GetAllTerritories ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllTerritories_ShouldReturnOk_WithTerritories()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllTerritoriesAsync(
                It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AccountTerritory> { MakeTerritory(1), MakeTerritory(2) });

        // Act
        var result = await _controller.GetAllTerritories();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllTerritories_ShouldReturnOk_WhenEmpty()
    {
        _mockService.Setup(s => s.GetAllTerritoriesAsync(
                It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AccountTerritory>());

        var result = await _controller.GetAllTerritories();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetTerritoryById ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetTerritoryById_ShouldReturnOk_WhenFound()
    {
        // Arrange
        var territory = MakeTerritory(5);
        _mockService.Setup(s => s.GetTerritoryByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(territory);

        // Act
        var result = await _controller.GetTerritoryById(5, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result.Result!).Value.Should().Be(territory);
    }

    [Fact]
    public async Task GetTerritoryById_ShouldReturnNotFound_WhenMissing()
    {
        // Arrange
        _mockService.Setup(s => s.GetTerritoryByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountTerritory?)null);

        // Act
        var result = await _controller.GetTerritoryById(999, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── UpdateTerritory ID mismatch ───────────────────────────────────────────

    [Fact]
    public async Task UpdateTerritory_ShouldReturnBadRequest_WhenIdMismatch()
    {
        // Arrange — URL id (10) does not match body id (99)
        var territory = MakeTerritory(99);

        // Act
        var result = await _controller.UpdateTerritory(10, territory, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── CreateTerritory ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTerritory_ShouldReturnCreated()
    {
        // Arrange
        var territory = MakeTerritory(0); // Id = 0 before creation
        var created = MakeTerritory(11);
        _mockService.Setup(s => s.CreateTerritoryAsync(territory, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        // Act
        var result = await _controller.CreateTerritory(territory, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)result.Result!).StatusCode.Should().Be(201);
    }
}
