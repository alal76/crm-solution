// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for MergeService (TCOV-007).
/// </summary>
public class MergeServiceTests : ServiceTestFixtureBase<MergeService>
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly MergeService _service;

    public MergeServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        SetupDatabase();
        _service = new MergeService(_mockDbContext.Object, MockLogger.Object);
    }

    private void SetupDatabase()
    {
        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMergeHistoryAsync_ShouldReturnEmpty_WhenNoMergeGroupsForRecord()
    {
        _mockDbContext.Setup(c => c.Set<DuplicateMergeGroup>())
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DuplicateMergeGroup>()).Object);

        var result = await _service.GetMergeHistoryAsync(42, "Lead");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMergeHistoryAsync_ShouldReturnEmpty_WhenNoMergeGroupsExist()
    {
        _mockDbContext.Setup(c => c.Set<DuplicateMergeGroup>())
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DuplicateMergeGroup>()).Object);

        var result = await _service.GetMergeHistoryAsync(1, "Lead");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMergedRecordsAsync_ShouldReturnEmpty_WhenNoMergedRecords()
    {
        _mockDbContext.Setup(c => c.Set<DuplicateMergeGroup>())
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DuplicateMergeGroup>()).Object);
        _mockDbContext.Setup(c => c.Set<DuplicateMergeGroupMember>())
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DuplicateMergeGroupMember>()).Object);

        var result = await _service.GetMergedRecordsAsync(1, "Lead");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMergeGroupAsync_ShouldReturnNull_WhenGroupNotFound()
    {
        _mockDbContext.Setup(c => c.Set<DuplicateMergeGroup>())
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DuplicateMergeGroup>()).Object);

        var result = await _service.GetMergeGroupAsync(999);

        result.Should().BeNull();
    }
}
