// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers.ITSM;

/// <summary>
/// Unit tests for EscalationRulesController.
/// Tests CRUD operations for escalation rules via IEscalationRuleAdminService.
/// </summary>
public class EscalationRulesControllerTests
{
    private readonly Mock<IEscalationRuleAdminService> _mockService;
    private readonly Mock<ILogger<EscalationRulesController>> _mockLogger;
    private readonly EscalationRulesController _controller;

    public EscalationRulesControllerTests()
    {
        _mockService = new Mock<IEscalationRuleAdminService>();
        _mockLogger = new Mock<ILogger<EscalationRulesController>>();
        _controller = new EscalationRulesController(_mockService.Object, _mockLogger.Object);
    }

    #region GET Tests

    [Fact]
    public async Task GetAll_ShouldReturnRules_WhenRulesExist()
    {
        // Arrange
        var rules = new List<EscalationRuleDto>
        {
            new EscalationRuleDto
            {
                Id = 1,
                Name = "Test Rule",
                Priority = "Critical",
                AgeInMinutes = 30,
                IsActive = true
            }
        };

        _mockService.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRules = Assert.IsType<List<EscalationRuleDto>>(okResult.Value);
        Assert.Single(returnedRules);
        _mockService.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldReturnRule_WhenRuleExists()
    {
        // Arrange
        var rule = new EscalationRuleDto
        {
            Id = 1,
            Name = "Test Rule",
            Priority = "Critical",
            IsActive = true
        };

        _mockService.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
        _mockService.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenRuleNotFound()
    {
        // Arrange
        _mockService.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EscalationRuleDto?)null);

        // Act
        var result = await _controller.GetById(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    #endregion

    #region CREATE Tests

    [Fact]
    public async Task Create_ShouldReturn201_WhenValid()
    {
        // Arrange
        var dto = new CreateEscalationRuleDto
        {
            Name = "New Rule",
            Priority = "High",
            AgeInMinutes = 60,
            TargetType = "User",
            IsActive = true
        };

        var createdRule = new EscalationRuleDto
        {
            Id = 1,
            Name = "New Rule",
            Priority = "High",
            AgeInMinutes = 60
        };

        _mockService.Setup(x => x.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRule);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(EscalationRulesController.GetById), createdResult.ActionName);
        _mockService.Verify(x => x.CreateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldReturn500_WhenServiceThrows()
    {
        // Arrange
        var dto = new CreateEscalationRuleDto
        {
            Name = "New Rule",
            Priority = "High",
            AgeInMinutes = 60
        };

        _mockService.Setup(x => x.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task Delete_ShouldReturn204_WhenRuleDeleted()
    {
        // Arrange
        _mockService.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenRuleNotFound()
    {
        // Arrange
        _mockService.Setup(x => x.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Rule not found"));

        // Act
        var result = await _controller.Delete(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion
}
