// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for DuplicateDetectionController.
/// Tests duplicate detection endpoints for various entity types.
/// TODO-SYS008-002
/// </summary>
public class DuplicateDetectionControllerTests
{
    private readonly Mock<IDuplicateDetectionService> _mockDuplicateService;
    private readonly Mock<ILogger<DuplicateDetectionController>> _mockLogger;
    private readonly DuplicateDetectionController _controller;

    public DuplicateDetectionControllerTests()
    {
        _mockDuplicateService = new Mock<IDuplicateDetectionService>();
        _mockLogger = new Mock<ILogger<DuplicateDetectionController>>();
        _controller = new DuplicateDetectionController(
            _mockDuplicateService.Object,
            _mockLogger.Object);
    }

    #region FindDuplicates Tests

    [Fact]
    public async Task FindDuplicates_WithValidEntityType_ReturnsOkResult()
    {
        // Arrange
        var entityType = "contact";
        var duplicates = new List<DuplicateDto>
        {
            new() { EntityType = entityType, EntityId = 1, MatchingEntityId = 2, ConfidenceScore = 0.95m }
        };

        _mockDuplicateService
            .Setup(s => s.FindDuplicatesAsync(entityType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicates);

        // Act
        var result = await _controller.FindDuplicates(entityType);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task FindDuplicates_WithNoDuplicates_ReturnsEmptyList()
    {
        // Arrange
        var entityType = "account";
        _mockDuplicateService
            .Setup(s => s.FindDuplicatesAsync(entityType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DuplicateDto>());

        // Act
        var result = await _controller.FindDuplicates(entityType);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task FindDuplicates_WithInvalidEntityType_ReturnsBadRequest()
    {
        // Arrange
        var entityType = "invalid-entity";
        _mockDuplicateService
            .Setup(s => s.FindDuplicatesAsync(entityType, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid entity type"));

        // Act
        var result = await _controller.FindDuplicates(entityType);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region FindDuplicatesForEntity Tests

    [Fact]
    public async Task FindDuplicatesForEntity_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var entityType = "contact";
        var entityId = 1;
        var matches = new List<DuplicateMatchDto>
        {
            new() { MatchingEntityId = 2, ConfidenceScore = 0.90m, MatchedFields = new[] { "email" } }
        };

        _mockDuplicateService
            .Setup(s => s.FindDuplicatesForEntityAsync(entityType, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        // Act
        var result = await _controller.FindDuplicatesForEntity(entityType, entityId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task FindDuplicatesForEntity_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var entityType = "contact";
        var entityId = 999;
        _mockDuplicateService
            .Setup(s => s.FindDuplicatesForEntityAsync(entityType, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<DuplicateMatchDto>?)null);

        // Act
        var result = await _controller.FindDuplicatesForEntity(entityType, entityId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region GetDuplicateRules Tests

    [Fact]
    public async Task GetDuplicateRules_ReturnsOkResult()
    {
        // Arrange
        var rules = new List<DuplicateRuleDto>
        {
            new() { Id = 1, EntityType = "contact", IsActive = true }
        };

        _mockDuplicateService
            .Setup(s => s.GetDuplicateRulesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _controller.GetDuplicateRules();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region UpdateDuplicateRule Tests

    [Fact]
    public async Task UpdateDuplicateRule_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var ruleId = 1;
        var updateDto = new UpdateDuplicateRuleDto { IsActive = false };

        _mockDuplicateService
            .Setup(s => s.UpdateDuplicateRuleAsync(ruleId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateRuleDto { Id = ruleId, IsActive = false });

        // Act
        var result = await _controller.UpdateDuplicateRule(ruleId, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateDuplicateRule_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var ruleId = 999;
        var updateDto = new UpdateDuplicateRuleDto { IsActive = false };

        _mockDuplicateService
            .Setup(s => s.UpdateDuplicateRuleAsync(ruleId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DuplicateRuleDto?)null);

        // Act
        var result = await _controller.UpdateDuplicateRule(ruleId, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region MarkAsNotDuplicate Tests

    [Fact]
    public async Task MarkAsNotDuplicate_WithValidIds_ReturnsOkResult()
    {
        // Arrange
        var request = new MarkNotDuplicateRequest
        {
            EntityType = "contact",
            EntityId = 1,
            MatchingEntityId = 2
        };

        _mockDuplicateService
            .Setup(s => s.MarkAsNotDuplicateAsync(request.EntityType, request.EntityId, request.MatchingEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkAsNotDuplicate(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}

// DTOs used in tests (may already exist in production code)
public class DuplicateDto
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int MatchingEntityId { get; set; }
    public decimal ConfidenceScore { get; set; }
}

public class DuplicateMatchDto
{
    public int MatchingEntityId { get; set; }
    public decimal ConfidenceScore { get; set; }
    public string[]? MatchedFields { get; set; }
}

public class DuplicateRuleDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpdateDuplicateRuleDto
{
    public bool IsActive { get; set; }
}

public class MarkNotDuplicateRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int MatchingEntityId { get; set; }
}
