// CRM Solution - Customer Relationship Management System
// Lookups Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for LookupsController
/// Covers: Lookup categories, items, CRUD operations
/// </summary>
public class LookupsControllerTests
{
    private readonly Mock<ILookupService> _mockLookupService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<LookupsController>> _mockLogger;
    private readonly LookupsController _controller;

    public LookupsControllerTests()
    {
        _mockLookupService = new Mock<ILookupService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<LookupsController>>();

        _controller = new LookupsController(
            _mockLookupService.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);

        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region Category Tests

    [Fact]
    public async Task GetCategories_ReturnsOkWithCategories()
    {
        // Arrange
        var categories = new List<LookupCategoryDto>
        {
            new LookupCategoryDto { Id = 1, Name = "Industry", Code = "INDUSTRY" },
            new LookupCategoryDto { Id = 2, Name = "Lead Source", Code = "LEAD_SOURCE" }
        };

        _mockLookupService.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCategories = okResult.Value.Should().BeAssignableTo<IEnumerable<LookupCategoryDto>>().Subject;
        returnedCategories.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCategoryById_ExistingCategory_ReturnsOk()
    {
        // Arrange
        var category = new LookupCategoryDto
        {
            Id = 1,
            Name = "Industry",
            Code = "INDUSTRY",
            Description = "Industry types"
        };

        _mockLookupService.Setup(s => s.GetCategoryByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result = await _controller.GetCategoryById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCategory = okResult.Value.Should().BeOfType<LookupCategoryDto>().Subject;
        returnedCategory.Code.Should().Be("INDUSTRY");
    }

    [Fact]
    public async Task GetCategoryById_NonExistingCategory_ReturnsNotFound()
    {
        // Arrange
        _mockLookupService.Setup(s => s.GetCategoryByIdAsync(999))
            .ReturnsAsync((LookupCategoryDto?)null);

        // Act
        var result = await _controller.GetCategoryById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetCategoryByCode_ExistingCategory_ReturnsOk()
    {
        // Arrange
        var category = new LookupCategoryDto
        {
            Id = 1,
            Name = "Industry",
            Code = "INDUSTRY"
        };

        _mockLookupService.Setup(s => s.GetCategoryByCodeAsync("INDUSTRY"))
            .ReturnsAsync(category);

        // Act
        var result = await _controller.GetCategoryByCode("INDUSTRY");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<LookupCategoryDto>();
    }

    [Fact]
    public async Task CreateCategory_ValidCategory_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateLookupCategoryDto
        {
            Name = "New Category",
            Code = "NEW_CATEGORY",
            Description = "A new lookup category"
        };

        var createdCategory = new LookupCategoryDto
        {
            Id = 3,
            Name = "New Category",
            Code = "NEW_CATEGORY"
        };

        _mockLookupService.Setup(s => s.CreateCategoryAsync(createDto))
            .ReturnsAsync(createdCategory);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateCategory(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetCategoryById));
    }

    [Fact]
    public async Task CreateCategory_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateLookupCategoryDto
        {
            Name = "Duplicate",
            Code = "EXISTING_CODE"
        };

        _mockLookupService.Setup(s => s.CreateCategoryAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Category with this code already exists"));

        // Act
        var result = await _controller.CreateCategory(createDto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task UpdateCategory_ValidCategory_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateLookupCategoryDto
        {
            Id = 1,
            Name = "Updated Category",
            Description = "Updated description"
        };

        var updatedCategory = new LookupCategoryDto
        {
            Id = 1,
            Name = "Updated Category"
        };

        _mockLookupService.Setup(s => s.UpdateCategoryAsync(updateDto))
            .ReturnsAsync(updatedCategory);

        // Act
        var result = await _controller.UpdateCategory(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<LookupCategoryDto>();
    }

    [Fact]
    public async Task DeleteCategory_ExistingCategory_ReturnsNoContent()
    {
        // Arrange
        _mockLookupService.Setup(s => s.DeleteCategoryAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCategory(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteCategory_CategoryWithItems_ReturnsConflict()
    {
        // Arrange
        _mockLookupService.Setup(s => s.DeleteCategoryAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot delete category with items"));

        // Act
        var result = await _controller.DeleteCategory(1);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    #endregion

    #region Lookup Items Tests

    [Fact]
    public async Task GetItemsByCategory_ReturnsItems()
    {
        // Arrange
        var items = new List<LookupItemDto>
        {
            new LookupItemDto { Id = 1, Code = "TECH", Name = "Technology", CategoryId = 1 },
            new LookupItemDto { Id = 2, Code = "FINANCE", Name = "Finance", CategoryId = 1 }
        };

        _mockLookupService.Setup(s => s.GetItemsByCategoryAsync(1))
            .ReturnsAsync(items);

        // Act
        var result = await _controller.GetItemsByCategory(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedItems = okResult.Value.Should().BeAssignableTo<IEnumerable<LookupItemDto>>().Subject;
        returnedItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetItemsByCategoryCode_ReturnsItems()
    {
        // Arrange
        var items = new List<LookupItemDto>
        {
            new LookupItemDto { Id = 1, Code = "TECH", Name = "Technology" }
        };

        _mockLookupService.Setup(s => s.GetItemsByCategoryCodeAsync("INDUSTRY"))
            .ReturnsAsync(items);

        // Act
        var result = await _controller.GetItemsByCategoryCode("INDUSTRY");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<LookupItemDto>>();
    }

    [Fact]
    public async Task GetItemById_ExistingItem_ReturnsOk()
    {
        // Arrange
        var item = new LookupItemDto
        {
            Id = 1,
            Code = "TECH",
            Name = "Technology",
            CategoryId = 1
        };

        _mockLookupService.Setup(s => s.GetItemByIdAsync(1))
            .ReturnsAsync(item);

        // Act
        var result = await _controller.GetItemById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedItem = okResult.Value.Should().BeOfType<LookupItemDto>().Subject;
        returnedItem.Code.Should().Be("TECH");
    }

    [Fact]
    public async Task GetItemById_NonExistingItem_ReturnsNotFound()
    {
        // Arrange
        _mockLookupService.Setup(s => s.GetItemByIdAsync(999))
            .ReturnsAsync((LookupItemDto?)null);

        // Act
        var result = await _controller.GetItemById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateItem_ValidItem_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateLookupItemDto
        {
            CategoryId = 1,
            Code = "NEW_ITEM",
            Name = "New Item",
            DisplayOrder = 1
        };

        var createdItem = new LookupItemDto
        {
            Id = 5,
            Code = "NEW_ITEM",
            Name = "New Item",
            CategoryId = 1
        };

        _mockLookupService.Setup(s => s.CreateItemAsync(createDto))
            .ReturnsAsync(createdItem);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateItem(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateItem_DuplicateCodeInCategory_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateLookupItemDto
        {
            CategoryId = 1,
            Code = "EXISTING_CODE",
            Name = "Item"
        };

        _mockLookupService.Setup(s => s.CreateItemAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Item with this code already exists in category"));

        // Act
        var result = await _controller.CreateItem(createDto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task UpdateItem_ValidItem_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateLookupItemDto
        {
            Id = 1,
            Name = "Updated Item",
            DisplayOrder = 5
        };

        var updatedItem = new LookupItemDto
        {
            Id = 1,
            Name = "Updated Item",
            DisplayOrder = 5
        };

        _mockLookupService.Setup(s => s.UpdateItemAsync(updateDto))
            .ReturnsAsync(updatedItem);

        // Act
        var result = await _controller.UpdateItem(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<LookupItemDto>();
    }

    [Fact]
    public async Task DeleteItem_ExistingItem_ReturnsNoContent()
    {
        // Arrange
        _mockLookupService.Setup(s => s.DeleteItemAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteItem(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteItem_ItemInUse_ReturnsConflict()
    {
        // Arrange
        _mockLookupService.Setup(s => s.DeleteItemAsync(1))
            .ThrowsAsync(new InvalidOperationException("Item is in use and cannot be deleted"));

        // Act
        var result = await _controller.DeleteItem(1);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public async Task ActivateItem_ValidItem_ReturnsOk()
    {
        // Arrange
        _mockLookupService.Setup(s => s.ActivateItemAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ActivateItem(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeactivateItem_ValidItem_ReturnsOk()
    {
        // Arrange
        _mockLookupService.Setup(s => s.DeactivateItemAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeactivateItem(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetActiveItems_ReturnsActiveItems()
    {
        // Arrange
        var items = new List<LookupItemDto>
        {
            new LookupItemDto { Id = 1, Name = "Active Item", IsActive = true }
        };

        _mockLookupService.Setup(s => s.GetActiveItemsByCategoryAsync(1))
            .ReturnsAsync(items);

        // Act
        var result = await _controller.GetActiveItems(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<LookupItemDto>>();
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task ReorderItems_ValidOrder_ReturnsOk()
    {
        // Arrange
        var reorderRequest = new ReorderLookupItemsDto
        {
            CategoryId = 1,
            ItemIds = new List<int> { 3, 1, 2 }
        };

        _mockLookupService.Setup(s => s.ReorderItemsAsync(1, reorderRequest.ItemIds))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ReorderItems(reorderRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ReorderItems_InvalidIds_ReturnsBadRequest()
    {
        // Arrange
        var reorderRequest = new ReorderLookupItemsDto
        {
            CategoryId = 1,
            ItemIds = new List<int> { 999 }
        };

        _mockLookupService.Setup(s => s.ReorderItemsAsync(1, reorderRequest.ItemIds))
            .ThrowsAsync(new ArgumentException("Invalid item IDs"));

        // Act
        var result = await _controller.ReorderItems(reorderRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkCreateItems_ValidItems_ReturnsOkWithCount()
    {
        // Arrange
        var items = new List<CreateLookupItemDto>
        {
            new CreateLookupItemDto { CategoryId = 1, Code = "ITEM1", Name = "Item 1" },
            new CreateLookupItemDto { CategoryId = 1, Code = "ITEM2", Name = "Item 2" }
        };

        _mockLookupService.Setup(s => s.BulkCreateItemsAsync(items))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.BulkCreateItems(items);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { CreatedCount = 2 });
    }

    [Fact]
    public async Task BulkDeleteItems_ValidIds_ReturnsOkWithCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockLookupService.Setup(s => s.BulkDeleteItemsAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDeleteItems(ids);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { DeletedCount = 3 });
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchItems_ValidQuery_ReturnsMatchingItems()
    {
        // Arrange
        var items = new List<LookupItemDto>
        {
            new LookupItemDto { Id = 1, Name = "Technology" }
        };

        _mockLookupService.Setup(s => s.SearchItemsAsync("Tech"))
            .ReturnsAsync(items);

        // Act
        var result = await _controller.SearchItems("Tech");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var searchResults = okResult.Value.Should().BeAssignableTo<IEnumerable<LookupItemDto>>().Subject;
        searchResults.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchItems_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchItems("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Usage Statistics Tests

    [Fact]
    public async Task GetItemUsageCount_ReturnsCount()
    {
        // Arrange
        _mockLookupService.Setup(s => s.GetItemUsageCountAsync(1))
            .ReturnsAsync(25);

        // Act
        var result = await _controller.GetItemUsageCount(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { UsageCount = 25 });
    }

    [Fact]
    public async Task GetCategoryStatistics_ReturnsStats()
    {
        // Arrange
        var stats = new LookupCategoryStatsDto
        {
            CategoryId = 1,
            TotalItems = 10,
            ActiveItems = 8,
            InactiveItems = 2
        };

        _mockLookupService.Setup(s => s.GetCategoryStatisticsAsync(1))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetCategoryStatistics(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<LookupCategoryStatsDto>();
    }

    #endregion

    #region Import/Export Tests

    [Fact]
    public async Task ExportCategory_ReturnsFile()
    {
        // Arrange
        var exportData = new byte[] { 0x50, 0x4B, 0x03, 0x04 };

        _mockLookupService.Setup(s => s.ExportCategoryAsync(1, "csv"))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.ExportCategory(1, "csv");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task ImportItems_ValidFile_ReturnsOkWithCount()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);

        _mockLookupService.Setup(s => s.ImportItemsAsync(1, fileMock.Object))
            .ReturnsAsync(5);

        // Act
        var result = await _controller.ImportItems(1, fileMock.Object);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { ImportedCount = 5 });
    }

    #endregion
}
