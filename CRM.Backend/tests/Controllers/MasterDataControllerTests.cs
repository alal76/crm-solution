// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Xunit;
using FluentAssertions;
using Moq;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for MasterDataController
/// 
/// FUNCTIONAL VIEW:
/// - Tests lookup data management endpoints
/// - Tests category and value CRUD operations
/// - Tests dropdown population for forms
/// 
/// TECHNICAL VIEW:
/// - Mocks IMasterDataService for isolated testing
/// - Validates hierarchical data structures
/// </summary>
public class MasterDataControllerTests
{
    private readonly Mock<IMasterDataService> _mockMasterDataService;
    private readonly Mock<ILogger<MasterDataController>> _mockLogger;
    private readonly MasterDataController _controller;

    public MasterDataControllerTests()
    {
        _mockMasterDataService = new Mock<IMasterDataService>();
        _mockLogger = new Mock<ILogger<MasterDataController>>();
        _controller = new MasterDataController(_mockMasterDataService.Object, _mockLogger.Object);
    }

    #region GetLookupCategories Tests

    /// <summary>
    /// FUNCTIONAL: Returns all lookup categories
    /// TECHNICAL: Returns OkObjectResult with category list
    /// </summary>
    [Fact]
    public async Task GetLookupCategories_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<LookupCategoryDto>
        {
            new LookupCategoryDto { Id = 1, Name = "CustomerType", Description = "Customer Types" },
            new LookupCategoryDto { Id = 2, Name = "LeadSource", Description = "Lead Sources" },
            new LookupCategoryDto { Id = 3, Name = "Industry", Description = "Industries" }
        };

        _mockMasterDataService.Setup(s => s.GetLookupCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetLookupCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategories = okResult.Value as IEnumerable<LookupCategoryDto>;
        returnedCategories.Should().HaveCount(3);
    }

    #endregion

    #region GetLookupValues Tests

    /// <summary>
    /// FUNCTIONAL: Returns values for specific category
    /// TECHNICAL: Filters by category name parameter
    /// </summary>
    [Fact]
    public async Task GetLookupValues_WithValidCategory_ReturnsValues()
    {
        // Arrange
        var values = new List<LookupValueDto>
        {
            new LookupValueDto { Id = 1, Value = "Enterprise", Category = "CustomerType", DisplayOrder = 1 },
            new LookupValueDto { Id = 2, Value = "SMB", Category = "CustomerType", DisplayOrder = 2 },
            new LookupValueDto { Id = 3, Value = "Startup", Category = "CustomerType", DisplayOrder = 3 }
        };

        _mockMasterDataService.Setup(s => s.GetLookupValuesAsync("CustomerType"))
            .ReturnsAsync(values);

        // Act
        var result = await _controller.GetLookupValues("CustomerType");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedValues = okResult.Value as IEnumerable<LookupValueDto>;
        returnedValues.Should().HaveCount(3);
    }

    /// <summary>
    /// FUNCTIONAL: Invalid category returns empty list
    /// TECHNICAL: Returns OkObjectResult with empty collection
    /// </summary>
    [Fact]
    public async Task GetLookupValues_WithInvalidCategory_ReturnsEmptyList()
    {
        // Arrange
        _mockMasterDataService.Setup(s => s.GetLookupValuesAsync("InvalidCategory"))
            .ReturnsAsync(new List<LookupValueDto>());

        // Act
        var result = await _controller.GetLookupValues("InvalidCategory");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedValues = okResult.Value as IEnumerable<LookupValueDto>;
        returnedValues.Should().BeEmpty();
    }

    #endregion

    #region CreateLookupValue Tests

    /// <summary>
    /// FUNCTIONAL: Creates new lookup value
    /// TECHNICAL: Returns CreatedAtActionResult with new value
    /// </summary>
    [Fact]
    public async Task CreateLookupValue_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createRequest = new CreateLookupValueRequest
        {
            Category = "CustomerType",
            Value = "Government",
            DisplayOrder = 4
        };
        var createdValue = new LookupValueDto
        {
            Id = 4,
            Value = "Government",
            Category = "CustomerType",
            DisplayOrder = 4
        };

        _mockMasterDataService.Setup(s => s.CreateLookupValueAsync(It.IsAny<CreateLookupValueRequest>()))
            .ReturnsAsync(createdValue);

        // Act
        var result = await _controller.CreateLookupValue(createRequest);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        createdResult.StatusCode.Should().Be(201);
    }

    /// <summary>
    /// FUNCTIONAL: Duplicate value returns conflict
    /// TECHNICAL: Returns ConflictObjectResult
    /// </summary>
    [Fact]
    public async Task CreateLookupValue_WithDuplicateValue_ReturnsConflict()
    {
        // Arrange
        var createRequest = new CreateLookupValueRequest
        {
            Category = "CustomerType",
            Value = "Enterprise", // Already exists
            DisplayOrder = 1
        };

        _mockMasterDataService.Setup(s => s.CreateLookupValueAsync(It.IsAny<CreateLookupValueRequest>()))
            .ReturnsAsync((LookupValueDto?)null);

        // Act
        var result = await _controller.CreateLookupValue(createRequest);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    #endregion

    #region UpdateLookupValue Tests

    /// <summary>
    /// FUNCTIONAL: Updates existing lookup value
    /// TECHNICAL: Returns OkObjectResult with updated value
    /// </summary>
    [Fact]
    public async Task UpdateLookupValue_WithValidData_ReturnsOk()
    {
        // Arrange
        var updateRequest = new UpdateLookupValueRequest
        {
            Id = 1,
            Value = "Large Enterprise",
            DisplayOrder = 1
        };
        var updatedValue = new LookupValueDto
        {
            Id = 1,
            Value = "Large Enterprise",
            Category = "CustomerType",
            DisplayOrder = 1
        };

        _mockMasterDataService.Setup(s => s.UpdateLookupValueAsync(It.IsAny<UpdateLookupValueRequest>()))
            .ReturnsAsync(updatedValue);

        // Act
        var result = await _controller.UpdateLookupValue(1, updateRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// FUNCTIONAL: Update non-existent value returns 404
    /// TECHNICAL: Returns NotFoundObjectResult
    /// </summary>
    [Fact]
    public async Task UpdateLookupValue_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateLookupValueRequest
        {
            Id = 999,
            Value = "Does Not Exist"
        };

        _mockMasterDataService.Setup(s => s.UpdateLookupValueAsync(It.IsAny<UpdateLookupValueRequest>()))
            .ReturnsAsync((LookupValueDto?)null);

        // Act
        var result = await _controller.UpdateLookupValue(999, updateRequest);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region DeleteLookupValue Tests

    /// <summary>
    /// FUNCTIONAL: Soft deletes lookup value
    /// TECHNICAL: Returns NoContentResult
    /// </summary>
    [Fact]
    public async Task DeleteLookupValue_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockMasterDataService.Setup(s => s.DeleteLookupValueAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteLookupValue(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    /// <summary>
    /// FUNCTIONAL: Delete non-existent value returns 404
    /// TECHNICAL: Returns NotFoundObjectResult
    /// </summary>
    [Fact]
    public async Task DeleteLookupValue_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockMasterDataService.Setup(s => s.DeleteLookupValueAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteLookupValue(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region GetIndustries Tests

    /// <summary>
    /// FUNCTIONAL: Returns all industries for dropdown
    /// TECHNICAL: Returns OkObjectResult with industry list
    /// </summary>
    [Fact]
    public async Task GetIndustries_ReturnsAllIndustries()
    {
        // Arrange
        var industries = new List<LookupValueDto>
        {
            new LookupValueDto { Id = 1, Value = "Technology" },
            new LookupValueDto { Id = 2, Value = "Healthcare" },
            new LookupValueDto { Id = 3, Value = "Finance" },
            new LookupValueDto { Id = 4, Value = "Manufacturing" },
            new LookupValueDto { Id = 5, Value = "Retail" }
        };

        _mockMasterDataService.Setup(s => s.GetLookupValuesAsync("Industry"))
            .ReturnsAsync(industries);

        // Act
        var result = await _controller.GetIndustries();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedIndustries = okResult.Value as IEnumerable<LookupValueDto>;
        returnedIndustries.Should().HaveCount(5);
    }

    #endregion

    #region GetLeadSources Tests

    /// <summary>
    /// FUNCTIONAL: Returns all lead sources for dropdown
    /// TECHNICAL: Returns OkObjectResult with lead source list
    /// </summary>
    [Fact]
    public async Task GetLeadSources_ReturnsAllLeadSources()
    {
        // Arrange
        var leadSources = new List<LookupValueDto>
        {
            new LookupValueDto { Id = 1, Value = "Website" },
            new LookupValueDto { Id = 2, Value = "Referral" },
            new LookupValueDto { Id = 3, Value = "Trade Show" },
            new LookupValueDto { Id = 4, Value = "Cold Call" }
        };

        _mockMasterDataService.Setup(s => s.GetLookupValuesAsync("LeadSource"))
            .ReturnsAsync(leadSources);

        // Act
        var result = await _controller.GetLeadSources();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSources = okResult.Value as IEnumerable<LookupValueDto>;
        returnedSources.Should().HaveCount(4);
    }

    #endregion
}
