// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
/// Unit tests for LeadsController.
/// Tests lead management endpoints including CRUD, conversion, and pagination.
/// </summary>
public class LeadsControllerTests
{
    private readonly Mock<ILeadService> _mockLeadService;
    private readonly Mock<ILeadAgingAlertService> _mockLeadAgingAlertService;
    private readonly Mock<ILogger<LeadsController>> _mockLogger;
    private readonly LeadsController _controller;

    public LeadsControllerTests()
    {
        _mockLeadService = new Mock<ILeadService>();
        _mockLeadAgingAlertService = new Mock<ILeadAgingAlertService>();
        _mockLogger = new Mock<ILogger<LeadsController>>();
        _controller = new LeadsController(_mockLeadService.Object, _mockLeadAgingAlertService.Object, _mockLogger.Object);
    }

    private static LeadSummaryDto CreateLeadSummary(int id = 1) => new()
    {
        Id = id,
        FirstName = "Jane",
        LastName = "Smith",
        Email = $"jane.smith{id}@example.com",
        Status = "New",
        Source = "Web"
    };

    private static LeadDto CreateLeadDto(int id = 1) => new()
    {
        Id = id,
        FirstName = "Jane",
        LastName = "Smith",
        Email = $"jane.smith{id}@example.com",
        Status = "New",
        Source = "Web"
    };

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithDefaultPagination_ReturnsOkWithPaginatedResult()
    {
        // Arrange
        var items = new List<LeadSummaryDto>
        {
            CreateLeadSummary(1),
            CreateLeadSummary(2)
        };
        _mockLeadService
            .Setup(s => s.GetAllAsync(1, 25))
            .ReturnsAsync((items, 2, 1, 25, 1));

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithCustomPageSize_PassesPaginationToService()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.GetAllAsync(2, 10))
            .ReturnsAsync((new List<LeadSummaryDto>(), 0, 2, 10, 0));

        // Act
        var result = await _controller.GetAll(page: 2, pageSize: 10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockLeadService.Verify(s => s.GetAllAsync(2, 10), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithLeadDto()
    {
        // Arrange
        var leadDto = CreateLeadDto(1);
        _mockLeadService
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(leadDto);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<LeadDto>().Subject;
        dto.Id.Should().Be(1);
        dto.Email.Should().Be("jane.smith1@example.com");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((LeadDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateLeadDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Company = "Acme Corp",
            Source = "Web"
        };
        _mockLeadService
            .Setup(s => s.CreateAsync(It.IsAny<Lead>()))
            .ReturnsAsync(42);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(LeadsController.GetById));
        createdResult.StatusCode.Should().Be(201);
        createdResult.RouteValues!["id"].Should().Be(42);
    }

    [Fact]
    public async Task Create_VerifiesServiceCalledWithLeadData()
    {
        // Arrange
        var request = new CreateLeadDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };
        _mockLeadService
            .Setup(s => s.CreateAsync(It.Is<Lead>(l => l.FirstName == "John" && l.LastName == "Doe")))
            .ReturnsAsync(1);

        // Act
        await _controller.Create(request);

        // Assert
        _mockLeadService.Verify(
            s => s.CreateAsync(It.Is<Lead>(l => l.FirstName == "John" && l.LastName == "Doe")),
            Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockLeadService
            .Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Convert Tests

    [Fact]
    public async Task Convert_WithValidId_ReturnsOkWithOpportunityInfo()
    {
        // Arrange
        var request = new ConvertLeadDto
        {
            OpportunityName = "New Opportunity",
            AccountId = 10,
            EstimatedValue = 5000m
        };
        _mockLeadService
            .Setup(s => s.ConvertAsync(1, "New Opportunity", 10, 5000m, null))
            .ReturnsAsync((99, 1));

        // Act
        var result = await _controller.Convert(1, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Convert_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ConvertLeadDto { OpportunityName = "Test" };
        _mockLeadService
            .Setup(s => s.ConvertAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new InvalidOperationException("Lead not found"));

        // Act
        var result = await _controller.Convert(999, request);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion
}
