// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// ENUM-TEST-001 to ENUM-TEST-005: Unit tests for EnumManagementService
using CRM.Core.DTOs;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for EnumManagementService using InMemory database.
/// Covers ENUM-TEST-001 through ENUM-TEST-005.
/// </summary>
public class EnumManagementServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<EnumManagementService>> _mockLogger;
    private readonly EnumManagementService _service;

    public EnumManagementServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"EnumManagementServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null!);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<EnumManagementService>>();
        _service = new EnumManagementService(_dbContext, _cache, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _cache.Dispose();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<EnumCategory> SeedCategoryAsync(
        string name = "TestCategory",
        bool isSystemManaged = false,
        bool allowCustomValues = true)
    {
        var category = new EnumCategory
        {
            Name = name,
            DisplayName = name,
            IsSystemManaged = isSystemManaged,
            AllowCustomValues = allowCustomValues,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _dbContext.EnumCategories.Add(category);
        await _dbContext.SaveChangesAsync();
        return category;
    }

    private async Task<EnumValue> SeedValueAsync(
        int categoryId,
        string key,
        string label,
        int sortOrder = 0,
        bool isActive = true,
        bool isSystemValue = false,
        bool isDeleted = false)
    {
        var value = new EnumValue
        {
            CategoryId = categoryId,
            Key = key,
            Label = label,
            SortOrder = sortOrder,
            IsActive = isActive,
            IsSystemValue = isSystemValue,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _dbContext.EnumValues.Add(value);
        await _dbContext.SaveChangesAsync();
        return value;
    }

    // ─── ENUM-TEST-001 ────────────────────────────────────────────────────────

    /// <summary>
    /// ENUM-TEST-001: GetValuesByCategoryNameAsync returns only active values,
    /// ordered by SortOrder ascending, ignoring inactive/deleted values.
    /// </summary>
    [Fact]
    public async Task GetValuesByCategoryNameAsync_ReturnsActiveValues_OrderedBySortOrder()
    {
        // Arrange
        var category = await SeedCategoryAsync("LeadStatus");

        await SeedValueAsync(category.Id, "cold", "Cold", sortOrder: 3, isActive: true);
        await SeedValueAsync(category.Id, "warm", "Warm", sortOrder: 1, isActive: true);
        await SeedValueAsync(category.Id, "hot", "Hot", sortOrder: 2, isActive: true);
        await SeedValueAsync(category.Id, "archived", "Archived", sortOrder: 0, isActive: false); // inactive — must be excluded

        // Act
        var result = (await _service.GetValuesByCategoryNameAsync("LeadStatus")).ToList();

        // Assert
        result.Should().HaveCount(3, "inactive values must be excluded");
        result.Select(v => v.SortOrder).Should().BeInAscendingOrder("values must be sorted by SortOrder");
        result.Select(v => v.Key).Should().NotContain("archived");
        result[0].Key.Should().Be("warm");
        result[1].Key.Should().Be("hot");
        result[2].Key.Should().Be("cold");
    }

    // ─── ENUM-TEST-002 ────────────────────────────────────────────────────────

    /// <summary>
    /// ENUM-TEST-002: CreateValueAsync throws InvalidOperationException when a value
    /// with the same key already exists in the category.
    /// </summary>
    [Fact]
    public async Task CreateValueAsync_WithDuplicateKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var category = await SeedCategoryAsync("OpportunityStage", allowCustomValues: true);
        await SeedValueAsync(category.Id, "existing", "Existing Value");

        var dto = new CreateEnumValueDto
        {
            Key = "existing",
            Label = "Duplicate Value",
        };

        // Act
        Func<Task> act = () => _service.CreateValueAsync(category.Id, dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*existing*", "error message must reference the duplicate key");
    }

    // ─── ENUM-TEST-003 ────────────────────────────────────────────────────────

    /// <summary>
    /// ENUM-TEST-003: DeleteValueAsync throws InvalidOperationException when the
    /// target value is a system value (system values are protected from deletion).
    /// </summary>
    [Fact]
    public async Task DeleteValueAsync_WhenSystemValue_ThrowsInvalidOperationException()
    {
        // Arrange
        var category = await SeedCategoryAsync("ServiceRequestStatus", isSystemManaged: true);
        var systemValue = await SeedValueAsync(
            category.Id, "open", "Open", isSystemValue: true);

        // Act
        Func<Task> act = () => _service.DeleteValueAsync(systemValue.Id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*System values cannot be deleted*");
    }

    // ─── ENUM-TEST-004 ────────────────────────────────────────────────────────

    /// <summary>
    /// ENUM-TEST-004: IsTransitionAllowedAsync returns the explicit rule value when
    /// a matching transition rule exists (IsAllowed = false means blocked).
    /// </summary>
    [Fact]
    public async Task IsTransitionAllowedAsync_WhenExplicitRuleExists_ReturnsRuleValue()
    {
        // Arrange
        var category = await SeedCategoryAsync("WorkflowStatus");
        var fromValue = await SeedValueAsync(category.Id, "closed", "Closed");
        var toValue = await SeedValueAsync(category.Id, "new", "New");

        // Explicit rule: Closed → New is NOT allowed
        var transition = new EnumTransition
        {
            CategoryId = category.Id,
            FromValueId = fromValue.Id,
            ToValueId = toValue.Id,
            IsAllowed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _dbContext.EnumTransitions.Add(transition);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.IsTransitionAllowedAsync("WorkflowStatus", fromValue.Id, toValue.Id);

        // Assert
        result.Should().BeFalse("the explicit rule marks this transition as forbidden");
    }

    /// <summary>
    /// ENUM-TEST-004b: IsTransitionAllowedAsync returns true (permissive default)
    /// when no explicit rule exists for the transition.
    /// </summary>
    [Fact]
    public async Task IsTransitionAllowedAsync_WhenNoRuleExists_ReturnsTrue()
    {
        // Arrange
        var category = await SeedCategoryAsync("TicketPriority");
        var fromValue = await SeedValueAsync(category.Id, "low", "Low");
        var toValue = await SeedValueAsync(category.Id, "high", "High");
        // No transition rules seeded

        // Act
        var result = await _service.IsTransitionAllowedAsync("TicketPriority", fromValue.Id, toValue.Id);

        // Assert
        result.Should().BeTrue("no rule means permissive default");
    }

    // ─── ENUM-TEST-005 ────────────────────────────────────────────────────────

    /// <summary>
    /// ENUM-TEST-005: ReorderValuesAsync updates SortOrder to reflect the
    /// supplied ordered id list (index = new SortOrder) and persists the changes.
    /// </summary>
    [Fact]
    public async Task ReorderValuesAsync_UpdatesSortOrders_InCorrectSequence()
    {
        // Arrange
        var category = await SeedCategoryAsync("CampaignStatus");
        var v1 = await SeedValueAsync(category.Id, "draft", "Draft", sortOrder: 0);
        var v2 = await SeedValueAsync(category.Id, "active", "Active", sortOrder: 1);
        var v3 = await SeedValueAsync(category.Id, "completed", "Completed", sortOrder: 2);

        // Reverse order: completed=0, active=1, draft=2
        var reorderedIds = new[] { v3.Id, v2.Id, v1.Id };

        // Act
        await _service.ReorderValuesAsync(category.Id, reorderedIds);

        // Assert — reload from DB
        var v1Updated = await _dbContext.EnumValues.FindAsync(v1.Id);
        var v2Updated = await _dbContext.EnumValues.FindAsync(v2.Id);
        var v3Updated = await _dbContext.EnumValues.FindAsync(v3.Id);

        v3Updated!.SortOrder.Should().Be(0, "completed was placed first");
        v2Updated!.SortOrder.Should().Be(1, "active was placed second");
        v1Updated!.SortOrder.Should().Be(2, "draft was placed last");
    }

    // ─── Additional edge-case tests ───────────────────────────────────────────

    /// <summary>
    /// GetValuesByCategoryNameAsync returns cached result on second call.
    /// </summary>
    [Fact]
    public async Task GetValuesByCategoryNameAsync_ReturnsCachedResult_OnSecondCall()
    {
        // Arrange
        var category = await SeedCategoryAsync("CachedCategory");
        await SeedValueAsync(category.Id, "val1", "Value 1", sortOrder: 0);

        // Act — first call populates cache
        var firstResult = (await _service.GetValuesByCategoryNameAsync("CachedCategory")).ToList();

        // Add a new value directly to DB (bypassing service)
        await SeedValueAsync(category.Id, "val2", "Value 2", sortOrder: 1);

        // Second call should return cached (stale) result
        var secondResult = (await _service.GetValuesByCategoryNameAsync("CachedCategory")).ToList();

        // Assert
        firstResult.Should().HaveCount(1);
        secondResult.Should().HaveCount(1, "cache is still warm — new DB row must not appear yet");
    }

    /// <summary>
    /// CreateCategory throws when a duplicate name is used.
    /// </summary>
    [Fact]
    public async Task CreateCategoryAsync_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        await SeedCategoryAsync("UniqueCategory");
        var dto = new CreateEnumCategoryDto { Name = "UniqueCategory", AllowCustomValues = true };

        // Act
        Func<Task> act = () => _service.CreateCategoryAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*UniqueCategory*");
    }

    /// <summary>
    /// ValidateValueAsync returns valid = true for a recognised key.
    /// </summary>
    [Fact]
    public async Task ValidateValueAsync_WithKnownKey_ReturnsValid()
    {
        // Arrange
        var category = await SeedCategoryAsync("ContactType", allowCustomValues: false);
        await SeedValueAsync(category.Id, "customer", "Customer");

        // Act
        var result = await _service.ValidateValueAsync("ContactType", "customer");

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// ValidateValueAsync returns valid = false for an unknown key when custom values are disallowed.
    /// </summary>
    [Fact]
    public async Task ValidateValueAsync_WithUnknownKey_WhenCustomValuesDisallowed_ReturnsInvalid()
    {
        // Arrange
        var category = await SeedCategoryAsync("ContactTypeStrict", allowCustomValues: false);
        await SeedValueAsync(category.Id, "customer", "Customer");

        // Act
        var result = await _service.ValidateValueAsync("ContactTypeStrict", "unknown_value");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
