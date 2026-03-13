// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// TCOV2-D06 — ReportsController unit tests
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Dtos.Reports;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for ReportsController (TCOV2-D06).
/// Tests HTTP contract only.  GetCurrentUserId() is private to the controller
/// and reads from User claims — satisfied by the ControllerContext setup below.
/// [Authorize] attribute present; not exercised in unit tests.
/// </summary>
public class ReportsControllerTests
{
    private readonly Mock<IReportService> _mockReportService;
    private readonly Mock<IWinLossAnalysisService> _mockWinLossService;
    private readonly Mock<IOpportunityService> _mockOpportunityService;
    private readonly Mock<IReportSharingService> _mockSharingService;
    private readonly Mock<ILogger<ReportsController>> _mockLogger;
    private readonly ReportsController _controller;

    public ReportsControllerTests()
    {
        _mockReportService = new Mock<IReportService>();
        _mockWinLossService = new Mock<IWinLossAnalysisService>();
        _mockOpportunityService = new Mock<IOpportunityService>();
        _mockSharingService = new Mock<IReportSharingService>();
        _mockLogger = new Mock<ILogger<ReportsController>>();

        _controller = new ReportsController(
            _mockReportService.Object,
            _mockWinLossService.Object,
            _mockOpportunityService.Object,
            _mockSharingService.Object,
            _mockLogger.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
            }
        };
    }

    private static ReportDefinitionDto MakeReportDto(int id = 1) => new()
    {
        Id = id,
        Name = $"Report {id}",
        Category = "Sales"
    };

    // ── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithReports()
    {
        // Arrange
        var reports = new List<ReportDefinitionDto> { MakeReportDto(1), MakeReportDto(2) };
        _mockReportService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(reports);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().BeEquivalentTo(reports);
    }

    [Fact]
    public async Task GetAll_CallsServiceOnce()
    {
        _mockReportService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReportDefinitionDto>());

        await _controller.GetAll();

        _mockReportService.Verify(s => s.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetByCategory ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCategory_ShouldReturnOk()
    {
        // Arrange
        var reports = new List<ReportDefinitionDto> { MakeReportDto(1) };
        _mockReportService.Setup(s => s.GetByCategoryAsync("Sales", It.IsAny<CancellationToken>()))
            .ReturnsAsync(reports);

        // Act
        var result = await _controller.GetByCategory("Sales");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetById ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenReportExists()
    {
        // Arrange
        var dto = MakeReportDto(5);
        _mockReportService.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetById(5);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenReportMissing()
    {
        // Arrange
        _mockReportService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportDefinitionDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    // ── GetMyReports ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyReports_ShouldReturnOk()
    {
        _mockReportService.Setup(s => s.GetByUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReportDefinitionDto>());

        var result = await _controller.GetMyReports();

        result.Should().BeOfType<OkObjectResult>();
    }
}
