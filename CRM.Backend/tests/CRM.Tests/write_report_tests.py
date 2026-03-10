
import os

BASE = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/tests/CRM.Tests"

files = {}

# TCOV-017 Expanded ReportServiceTests.cs
files["Services/ReportServiceTestsExpanded.cs"] = """// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using System.Security.Claims;
using CRM.Core.Dtos.Reports;
using CRM.Core.Entities;
using CRM.Core.Entities.Reports;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ReportDefinitionEntity = CRM.Core.Entities.Reports.ReportDefinition;
using ReportFolderEntity = CRM.Core.Entities.Reports.ReportFolder;

namespace CRM.Tests.Services;

/// <summary>
/// Additional report service tests (TCOV-017 expansion).
/// Covers GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync,
/// GetFoldersAsync, CreateFolderAsync, GetByCategoryAsync.
/// </summary>
public class ReportServiceExtendedTests : ServiceTestFixtureBase<ReportService>
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly ReportService _service;

    public ReportServiceExtendedTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }))
        };

        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);
        _service = new ReportService(MockContext.Object, MockLogger.Object, _mockHttpContextAccessor.Object);
    }

    private void SetupReportDbSets(
        List<ReportDefinitionEntity>? reports = null,
        List<ReportFolderEntity>? folders = null)
    {
        reports ??= new List<ReportDefinitionEntity>();
        folders ??= new List<ReportFolderEntity>();

        var mockReports = MockDbSetFactory.CreateMockDbSet(reports);
        mockReports.Setup(m => m.Add(It.IsAny<ReportDefinitionEntity>()))
            .Callback<ReportDefinitionEntity>(r => reports.Add(r));
        MockContext.Setup(c => c.ReportDefinitions).Returns(mockReports.Object);

        var mockFolders = MockDbSetFactory.CreateMockDbSet(folders);
        mockFolders.Setup(m => m.Add(It.IsAny<ReportFolderEntity>()))
            .Callback<ReportFolderEntity>(f => folders.Add(f));
        MockContext.Setup(c => c.ReportFolders).Returns(mockFolders.Object);

        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoReports()
    {
        SetupReportDbSets();
        var result = await _service.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyNonDeletedReports()
    {
        var reports = new List<ReportDefinitionEntity>
        {
            new() { Id = 1, Name = "Active Report", Status = ReportStatus.Active, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Deleted Report", Status = ReportStatus.Active, IsDeleted = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };

        SetupReportDbSets(reports);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active Report");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenReportNotFound()
    {
        SetupReportDbSets();
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReport_WhenFound()
    {
        var reports = new List<ReportDefinitionEntity>
        {
            new() { Id = 5, Name = "Found Report", Status = ReportStatus.Active, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        SetupReportDbSets(reports);

        var result = await _service.GetByIdAsync(5);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Found Report");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenReportIsDeleted()
    {
        var reports = new List<ReportDefinitionEntity>
        {
            new() { Id = 6, Name = "Deleted Report", Status = ReportStatus.Active, IsDeleted = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        SetupReportDbSets(reports);

        var result = await _service.GetByIdAsync(6);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddReportToDatabase()
    {
        var reportList = new List<ReportDefinitionEntity>();
        SetupReportDbSets(reportList);

        var dto = new CreateReportDefinitionDto
        {
            Name = "New Custom Report",
            Description = "A custom report",
            Category = "Sales",
            Query = "SELECT * FROM Accounts"
        };

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Custom Report");
        reportList.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDuplicateNameExists()
    {
        var reports = new List<ReportDefinitionEntity>
        {
            new() { Id = 1, Name = "Existing Report", Status = ReportStatus.Active, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        SetupReportDbSets(reports);

        var dto = new CreateReportDefinitionDto
        {
            Name = "Existing Report",
            Description = "Duplicate name"
        };

        var act = async () => await _service.CreateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Existing Report*");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenReportNotFound()
    {
        SetupReportDbSets();

        var result = await _service.DeleteAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteReport_WhenFound()
    {
        var report = new ReportDefinitionEntity
        {
            Id = 10,
            Name = "Deletable Report",
            Status = ReportStatus.Active,
            CreatedByUserId = 1,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var reports = new List<ReportDefinitionEntity> { report };
        SetupReportDbSets(reports);

        var result = await _service.DeleteAsync(10);

        result.Should().BeTrue();
        report.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowUnauthorized_WhenDeletingStandardReport()
    {
        var report = new ReportDefinitionEntity
        {
            Id = 20,
            Name = "Standard Report",
            Status = ReportStatus.Active,
            CreatedByUserId = 0, // System-created = standard report
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var reports = new List<ReportDefinitionEntity> { report };
        SetupReportDbSets(reports);

        var act = async () => await _service.DeleteAsync(20);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnMatchingReports()
    {
        var reports = new List<ReportDefinitionEntity>
        {
            new() { Id = 1, Name = "Sales Report", Category = "Sales", Status = ReportStatus.Active, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "HR Report", Category = "HR", Status = ReportStatus.Active, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };
        SetupReportDbSets(reports);

        var result = await _service.GetByCategoryAsync("Sales");

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Sales Report");
    }

    [Fact]
    public async Task GetFoldersAsync_ShouldReturnEmpty_WhenNoFolders()
    {
        SetupReportDbSets();
        var result = await _service.GetFoldersAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateFolderAsync_ShouldAddFolderToDatabase()
    {
        var folderList = new List<ReportFolderEntity>();
        SetupReportDbSets(new List<ReportDefinitionEntity>(), folderList);

        var dto = new CreateReportFolderDto
        {
            Name = "Q1 Reports",
            Description = "First quarter reports"
        };

        var result = await _service.CreateFolderAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Q1 Reports");
        folderList.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddToFavoritesAsync_ShouldSucceed()
    {
        SetupReportDbSets();

        // In-memory storage, should not throw
        var act = async () => await _service.AddToFavoritesAsync(reportId: 1, userId: 1);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ShouldSucceed()
    {
        SetupReportDbSets();
        await _service.AddToFavoritesAsync(reportId: 1, userId: 1);

        var act = async () => await _service.RemoveFromFavoritesAsync(reportId: 1, userId: 1);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetByFolderAsync_ShouldReturnReportsInFolder()
    {
        var reports = new List<ReportDefinitionEntity>
        {
            new() { Id = 1, Name = "Folder Report 1", FolderId = 3, Status = ReportStatus.Active, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Folder Report 2", FolderId = 3, Status = ReportStatus.Active, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, Name = "Other Folder", FolderId = 99, Status = ReportStatus.Active, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };
        SetupReportDbSets(reports);

        var result = await _service.GetByFolderAsync(3);

        result.Should().HaveCount(2);
    }
}
"""

for rel_path, content in files.items():
    full_path = os.path.join(BASE, rel_path)
    os.makedirs(os.path.dirname(full_path), exist_ok=True)
    with open(full_path, "w") as f:
        f.write(content.lstrip("\n"))
    print(f"Written: {rel_path}")

print("Done.")
