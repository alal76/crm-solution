// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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
    private readonly Mock<IReportTemplateService> _mockTemplateService;
    private readonly Mock<ILogger<ReportsController>> _mockLogger;
    private readonly ReportsController _controller;

    public ReportsControllerTests()
    {
        _mockReportService = new Mock<IReportService>();
        _mockWinLossService = new Mock<IWinLossAnalysisService>();
        _mockOpportunityService = new Mock<IOpportunityService>();
        _mockSharingService = new Mock<IReportSharingService>();
        _mockTemplateService = new Mock<IReportTemplateService>();
        _mockLogger = new Mock<ILogger<ReportsController>>();

        _controller = new ReportsController(
            _mockReportService.Object,
            _mockWinLossService.Object,
            _mockOpportunityService.Object,
            _mockSharingService.Object,
            _mockTemplateService.Object,
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

    // ── Report Templates Marketplace (REV-FE-003) ───────────────────────────────

    [Fact]
    public async Task GetReportTemplates_ShouldReturnOk_WithTemplates()
    {
        // Arrange
        var templates = new List<ReportTemplateDto>
        {
            new() { Id = 1, Name = "Sales Pipeline Report", Category = "Sales" }
        };
        _mockTemplateService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        // Act
        var result = await _controller.GetReportTemplates();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().BeEquivalentTo(templates);
    }

    [Fact]
    public async Task ApplyReportTemplate_ShouldReturnOk_WhenTemplateExists()
    {
        // Arrange
        var applyResult = new ApplyReportTemplateResultDto
        {
            TemplateId = 1,
            TemplateName = "Sales Pipeline Report",
            Downloads = 1524
        };
        _mockTemplateService.Setup(s => s.ApplyAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(applyResult);

        // Act
        var result = await _controller.ApplyReportTemplate(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().BeEquivalentTo(applyResult);
    }

    [Fact]
    public async Task ApplyReportTemplate_ShouldReturnNotFound_WhenTemplateMissing()
    {
        // Arrange
        _mockTemplateService.Setup(s => s.ApplyAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplyReportTemplateResultDto?)null);

        // Act
        var result = await _controller.ApplyReportTemplate(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
