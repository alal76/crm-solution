// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// TCOV2-D07 — CampaignsController unit tests
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for CampaignsController (TCOV2-D07).
/// CampaignsController only injects IMarketingCampaignService.
/// [Authorize] attribute present; not exercised in unit tests.
/// </summary>
public class CampaignsControllerTests
{
    private readonly Mock<IMarketingCampaignService> _mockCampaignService;
    private readonly CampaignsController _controller;

    public CampaignsControllerTests()
    {
        _mockCampaignService = new Mock<IMarketingCampaignService>();
        _controller = new CampaignsController(_mockCampaignService.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private static CampaignDto MakeCampaignDto(int id = 1) => new()
    {
        Id = id,
        Name = $"Campaign {id}",
        Status = 0
    };

    // ── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignDto> { MakeCampaignDto(1), MakeCampaignDto(2) };
        _mockCampaignService.Setup(s => s.GetAllCampaignsAsync()).ReturnsAsync(campaigns);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().BeEquivalentTo(campaigns);
    }

    [Fact]
    public async Task GetAll_CallsServiceOnce()
    {
        _mockCampaignService.Setup(s => s.GetAllCampaignsAsync()).ReturnsAsync(new List<CampaignDto>());

        await _controller.GetAll();

        _mockCampaignService.Verify(s => s.GetAllCampaignsAsync(), Times.Once);
    }

    // ── GetActive ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetActive_ShouldReturnOk()
    {
        _mockCampaignService.Setup(s => s.GetActiveCampaignsAsync())
            .ReturnsAsync(new List<CampaignDto>());

        var result = await _controller.GetActive();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetById ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCampaignExists()
    {
        // Arrange
        var dto = MakeCampaignDto(3);
        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(3)).ReturnsAsync(dto);

        // Act
        var result = await _controller.GetById(3);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCampaignMissing()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(999)).ReturnsAsync((CampaignDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDtoIsValid()
    {
        // Arrange
        var createDto = new CreateCampaignDto { Name = "Summer Sale", Budget = 5000m };
        var created = MakeCampaignDto(10);
        _mockCampaignService.Setup(s => s.CreateCampaignAsync(It.IsAny<CreateCampaignDto>()))
            .ReturnsAsync(10);
        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(10)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)result).StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenServiceThrowsArgumentException()
    {
        // Arrange
        var createDto = new CreateCampaignDto { Name = "Bad Campaign" };
        _mockCampaignService.Setup(s => s.CreateCampaignAsync(It.IsAny<CreateCampaignDto>()))
            .ThrowsAsync(new ArgumentException("Invalid date range"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
