// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: SPEC-RPT-03 (Report Sharing)
// TODO-RPT-03: Report Sharing — unit tests
//
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Source files read: ReportSharingService.cs (namespace CRM.Infrastructure.Services),
//   ReportShare.cs (CRM.Core.Entities), ReportDefinition.cs (CRM.Core.Entities.Reports),
//   ICrmDbContext.cs — DbSet<ReportDefinition> ReportDefinitions, DbSet<ReportShare> ReportShares.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ReportDefinition = CRM.Core.Entities.Reports.ReportDefinition;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ReportSharingService (TODO-RPT-03).
/// Validates ShareReportAsync, GetReportShareInfoAsync, and RevokeShareAsync.
/// </summary>
public class ReportSharingServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly ReportSharingService _service;

    public ReportSharingServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _service = new ReportSharingService(
            _mockContext.Object,
            Mock.Of<ILogger<ReportSharingService>>());
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private static ReportDefinition MakeReport(int id, bool isDeleted = false) => new()
    {
        Id = id,
        Name = $"Report {id}",
        Category = "Sales",
        IsDeleted = isDeleted,
        CreatedByUserId = 1,
        CreatedAt = DateTime.UtcNow.AddDays(-10),
        UpdatedAt = DateTime.UtcNow
    };

    private static ReportShare MakeShare(int id, int reportId, int userId, string permission = "View") => new()
    {
        Id = id,
        ReportId = reportId,
        UserId = userId,
        Permission = permission,
        SharedByUserId = 1,
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow.AddDays(-1),
        UpdatedAt = DateTime.UtcNow,
        User = new User { Id = userId, Email = $"user{userId}@test.com", FirstName = "Test", LastName = $"User{userId}" },
        SharedByUser = new User { Id = 1, Email = "admin@test.com", FirstName = "Admin", LastName = "User" }
    };

    private void SetupReportDefinitions(List<ReportDefinition> reports)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(reports);
        _mockContext.Setup(c => c.ReportDefinitions).Returns(mockSet.Object);
    }

    private void SetupReportShares(List<ReportShare> shares)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(shares);
        mockSet.Setup(s => s.Add(It.IsAny<ReportShare>())).Verifiable();
        _mockContext.Setup(c => c.ReportShares).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // ────────────────────────────────────────────────────────────────────────
    // ShareReportAsync Tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ShareReportAsync_ShouldReturnFailure_WhenReportNotFound()
    {
        // Arrange – empty report definitions, so report 99 does not exist
        SetupReportDefinitions([]);
        SetupReportShares([]);

        // Act
        var result = await _service.ShareReportAsync(99, [10], ReportSharePermission.View, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("99");
    }

    [Fact]
    public async Task ShareReportAsync_ShouldCreateNewShare_WhenNoExistingShare()
    {
        // Arrange
        SetupReportDefinitions([MakeReport(1)]);
        SetupReportShares([]); // no existing shares

        // Act
        var result = await _service.ShareReportAsync(1, [2], ReportSharePermission.View, 1);

        // Assert
        result.Success.Should().BeTrue();
        result.NewSharesCreated.Should().Be(1);
        result.SharesUpdated.Should().Be(0);
    }

    [Fact]
    public async Task ShareReportAsync_ShouldUpdateExistingShare_WhenShareAlreadyExists()
    {
        // Arrange – existing share for user 2 with View
        SetupReportDefinitions([MakeReport(1)]);
        SetupReportShares([MakeShare(1, reportId: 1, userId: 2, permission: "View")]);

        // Act – re-share with Edit
        var result = await _service.ShareReportAsync(1, [2], ReportSharePermission.Edit, 1);

        // Assert
        result.Success.Should().BeTrue();
        result.SharesUpdated.Should().Be(1);
        result.NewSharesCreated.Should().Be(0);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetReportShareInfoAsync Tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetReportShareInfoAsync_ShouldReturnShares_WhenSharesExistForReport()
    {
        // Arrange
        var shares = new List<ReportShare>
        {
            MakeShare(1, reportId: 5, userId: 10, "View"),
            MakeShare(2, reportId: 5, userId: 11, "Edit"),
            MakeShare(3, reportId: 6, userId: 12, "View") // different report — should not appear
        };
        SetupReportShares(shares);

        // Act
        var result = (await _service.GetReportShareInfoAsync(5)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.UserId.Should().BeOneOf(10, 11));
    }

    // ────────────────────────────────────────────────────────────────────────
    // RevokeShareAsync Tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RevokeShareAsync_ShouldReturnFalse_WhenShareNotFound()
    {
        // Arrange – no shares in context
        SetupReportShares([]);

        // Act
        var revoked = await _service.RevokeShareAsync(reportId: 1, userId: 99);

        // Assert
        revoked.Should().BeFalse();
    }
}
