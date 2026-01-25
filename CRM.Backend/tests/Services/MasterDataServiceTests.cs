// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Xunit;
using FluentAssertions;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for MasterDataService
/// 
/// FUNCTIONAL VIEW:
/// - Tests lookup category and value management
/// - Tests hierarchical data retrieval
/// - Tests CRUD operations on master data
/// 
/// TECHNICAL VIEW:
/// - Uses InMemory database for realistic testing
/// - Tests service layer business logic
/// </summary>
public class MasterDataServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly MasterDataService _service;
    private readonly Mock<ILogger<MasterDataService>> _loggerMock;

    public MasterDataServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_MasterData_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _loggerMock = new Mock<ILogger<MasterDataService>>();
        _service = new MasterDataService(_dbContext, _loggerMock.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Seed lookup categories
        _dbContext.LookupCategories.AddRange(
            new LookupCategory { Id = 1, Name = "CustomerType", Description = "Customer Types", IsActive = true, CreatedAt = DateTime.UtcNow },
            new LookupCategory { Id = 2, Name = "LeadSource", Description = "Lead Sources", IsActive = true, CreatedAt = DateTime.UtcNow },
            new LookupCategory { Id = 3, Name = "Industry", Description = "Industry Types", IsActive = true, CreatedAt = DateTime.UtcNow },
            new LookupCategory { Id = 4, Name = "Inactive", Description = "Inactive Category", IsActive = false, CreatedAt = DateTime.UtcNow }
        );

        // Seed lookup values
        _dbContext.LookupValues.AddRange(
            // Customer Types
            new LookupValue { Id = 1, CategoryId = 1, Value = "Enterprise", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new LookupValue { Id = 2, CategoryId = 1, Value = "SMB", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
            new LookupValue { Id = 3, CategoryId = 1, Value = "Startup", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            
            // Lead Sources
            new LookupValue { Id = 4, CategoryId = 2, Value = "Website", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new LookupValue { Id = 5, CategoryId = 2, Value = "Referral", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
            new LookupValue { Id = 6, CategoryId = 2, Value = "Trade Show", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            
            // Industries
            new LookupValue { Id = 7, CategoryId = 3, Value = "Technology", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new LookupValue { Id = 8, CategoryId = 3, Value = "Healthcare", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
            new LookupValue { Id = 9, CategoryId = 3, Value = "Finance", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            new LookupValue { Id = 10, CategoryId = 3, Value = "Inactive Industry", DisplayOrder = 4, IsActive = false, CreatedAt = DateTime.UtcNow }
        );

        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region GetLookupCategoriesAsync Tests

    [Fact]
    public async Task GetLookupCategoriesAsync_ReturnsActiveCategories()
    {
        // Act
        var result = await _service.GetLookupCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3); // Excludes inactive
        result.Should().NotContain(c => c.Name == "Inactive");
    }

    [Fact]
    public async Task GetLookupCategoriesAsync_IncludesValueCounts()
    {
        // Act
        var result = (await _service.GetLookupCategoriesAsync()).ToList();

        // Assert
        var customerTypeCategory = result.First(c => c.Name == "CustomerType");
        customerTypeCategory.ValueCount.Should().Be(3);
    }

    #endregion

    #region GetLookupValuesAsync Tests

    [Fact]
    public async Task GetLookupValuesAsync_WithValidCategory_ReturnsValues()
    {
        // Act
        var result = await _service.GetLookupValuesAsync("CustomerType");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(v => v.Value == "Enterprise");
        result.Should().Contain(v => v.Value == "SMB");
        result.Should().Contain(v => v.Value == "Startup");
    }

    [Fact]
    public async Task GetLookupValuesAsync_ReturnsOnlyActiveValues()
    {
        // Act
        var result = await _service.GetLookupValuesAsync("Industry");

        // Assert
        result.Should().HaveCount(3); // Excludes "Inactive Industry"
        result.Should().NotContain(v => v.Value == "Inactive Industry");
    }

    [Fact]
    public async Task GetLookupValuesAsync_ReturnsSortedByDisplayOrder()
    {
        // Act
        var result = (await _service.GetLookupValuesAsync("LeadSource")).ToList();

        // Assert
        result[0].Value.Should().Be("Website");
        result[1].Value.Should().Be("Referral");
        result[2].Value.Should().Be("Trade Show");
    }

    [Fact]
    public async Task GetLookupValuesAsync_WithInvalidCategory_ReturnsEmpty()
    {
        // Act
        var result = await _service.GetLookupValuesAsync("InvalidCategory");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateLookupValueAsync Tests

    [Fact]
    public async Task CreateLookupValueAsync_WithValidData_CreatesValue()
    {
        // Arrange
        var request = new CreateLookupValueRequest
        {
            Category = "CustomerType",
            Value = "Government",
            DisplayOrder = 4
        };

        // Act
        var result = await _service.CreateLookupValueAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be("Government");
        result.DisplayOrder.Should().Be(4);

        // Verify persistence
        var savedValue = await _dbContext.LookupValues.FirstOrDefaultAsync(v => v.Value == "Government");
        savedValue.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateLookupValueAsync_AutoAssignsDisplayOrder()
    {
        // Arrange
        var request = new CreateLookupValueRequest
        {
            Category = "CustomerType",
            Value = "Non-Profit"
            // DisplayOrder not specified
        };

        // Act
        var result = await _service.CreateLookupValueAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayOrder.Should().Be(4); // Next after existing 3
    }

    [Fact]
    public async Task CreateLookupValueAsync_WithInvalidCategory_ReturnsNull()
    {
        // Arrange
        var request = new CreateLookupValueRequest
        {
            Category = "InvalidCategory",
            Value = "Test Value"
        };

        // Act
        var result = await _service.CreateLookupValueAsync(request);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdateLookupValueAsync Tests

    [Fact]
    public async Task UpdateLookupValueAsync_WithValidData_UpdatesValue()
    {
        // Arrange
        var request = new UpdateLookupValueRequest
        {
            Id = 1,
            Value = "Large Enterprise",
            DisplayOrder = 1
        };

        // Act
        var result = await _service.UpdateLookupValueAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be("Large Enterprise");

        // Verify persistence
        var updatedValue = await _dbContext.LookupValues.FindAsync(1);
        updatedValue!.Value.Should().Be("Large Enterprise");
    }

    [Fact]
    public async Task UpdateLookupValueAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var request = new UpdateLookupValueRequest
        {
            Id = 999,
            Value = "Does Not Exist"
        };

        // Act
        var result = await _service.UpdateLookupValueAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLookupValueAsync_CanDeactivateValue()
    {
        // Arrange
        var request = new UpdateLookupValueRequest
        {
            Id = 1,
            Value = "Enterprise",
            IsActive = false
        };

        // Act
        var result = await _service.UpdateLookupValueAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    #endregion

    #region DeleteLookupValueAsync Tests

    [Fact]
    public async Task DeleteLookupValueAsync_WithValidId_SoftDeletesValue()
    {
        // Act
        var result = await _service.DeleteLookupValueAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        var deletedValue = await _dbContext.LookupValues.FindAsync(1);
        deletedValue!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteLookupValueAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteLookupValueAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAllValuesGroupedByCategory Tests

    [Fact]
    public async Task GetAllValuesGroupedByCategory_ReturnsGroupedData()
    {
        // Act
        var result = await _service.GetAllValuesGroupedByCategoryAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("CustomerType");
        result.Should().ContainKey("LeadSource");
        result.Should().ContainKey("Industry");
        result["CustomerType"].Should().HaveCount(3);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task ReorderLookupValues_UpdatesDisplayOrders()
    {
        // Arrange
        var reorderRequest = new ReorderLookupValuesRequest
        {
            CategoryName = "CustomerType",
            ValueIds = new List<int> { 3, 1, 2 } // Startup, Enterprise, SMB
        };

        // Act
        var result = await _service.ReorderLookupValuesAsync(reorderRequest);

        // Assert
        result.Should().BeTrue();

        var values = await _dbContext.LookupValues
            .Where(v => v.CategoryId == 1)
            .OrderBy(v => v.DisplayOrder)
            .ToListAsync();
        
        values[0].Id.Should().Be(3); // Startup first
        values[1].Id.Should().Be(1); // Enterprise second
        values[2].Id.Should().Be(2); // SMB third
    }

    #endregion
}
