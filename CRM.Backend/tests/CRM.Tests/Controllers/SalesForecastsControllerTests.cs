// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TCOV2-D12 — SalesForecastsController unit tests
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
/// Unit tests for SalesForecastsController (TCOV2-D12).
/// Route: /api/sales-forecasts
/// </summary>
public class SalesForecastsControllerTests
{
    private readonly Mock<ISalesForecastService> _mockService;
    private readonly SalesForecastsController _controller;

    public SalesForecastsControllerTests()
    {
        _mockService = new Mock<ISalesForecastService>();
        var mockLogger = new Mock<ILogger<SalesForecastsController>>();
        _controller = new SalesForecastsController(_mockService.Object, mockLogger.Object);
    }

    private static SalesForecast MakeForecast(int id = 1) => new()
    {
        Id = id,
        Name = $"Q1 2025 Forecast {id}",
        FiscalYear = 2025,
        FiscalQuarter = 1,
        QuotaAmount = 100_000m
    };

    // ── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithForecasts()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesForecast> { MakeForecast(1) });

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenEmpty()
    {
        _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesForecast>());

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetById ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenForecastExists()
    {
        // Arrange
        var forecast = MakeForecast(5);
        _mockService.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(forecast);

        // Act
        var result = await _controller.GetById(5);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result.Result!).Value.Should().Be(forecast);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesForecast?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenForecastIsValid()
    {
        // Arrange
        var forecast = MakeForecast(0);
        var created = MakeForecast(20);
        _mockService.Setup(s => s.CreateAsync(forecast, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(forecast);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)result.Result!).StatusCode.Should().Be(201);
    }

    // ── service interaction ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_CallsServiceWithCorrectId()
    {
        _mockService.Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(MakeForecast(7));

        await _controller.GetById(7);

        _mockService.Verify(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }
}
