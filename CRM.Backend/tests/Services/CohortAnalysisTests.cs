// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: SPEC-RPT-07 (Cohort Analysis)
// TODO-RPT-07: Cohort Analysis Endpoints — unit tests
//
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Source files read: ReportService.cs (ICrmDbContext, ILogger, IHttpContextAccessor),
//   Account.cs (Id, Industry, AnnualRevenue, LifecycleStage:AccountLifecycleStage, CreatedAt, IsDeleted),
//   Opportunity.cs (Id, AccountId:int, Amount:decimal, ExpectedCloseDate:DateTime?, IsDeleted),
//   ICrmDbContext.cs — DbSet<Account> Accounts, DbSet<Opportunity> Opportunities.

using CRM.Core.Dtos.Reports;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ReportService cohort analysis methods (TODO-RPT-07).
/// Validates GetCohortAnalysisAsync and GetCustomerSegmentsAsync.
/// </summary>
public class CohortAnalysisTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly ReportService _service;

    public CohortAnalysisTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _service = new ReportService(
            _mockContext.Object,
            Mock.Of<ILogger<ReportService>>(),
            Mock.Of<IHttpContextAccessor>());
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private static Account MakeAccount(int id, DateTime createdAt, string industry = "Tech") => new()
    {
        Id = id,
        Company = $"Acme{id}",
        Email = $"acme{id}@test.com",
        Phone = "555-0100",
        Industry = industry,
        AnnualRevenue = id * 100_000m,
        LifecycleStage = AccountLifecycleStage.Active,
        IsDeleted = false,
        CreatedAt = createdAt,
        UpdatedAt = createdAt
    };

    private static Opportunity MakeOpportunity(int id, int accountId, decimal amount, DateTime? closeDate) => new()
    {
        Id = id,
        Name = $"Opp{id}",
        AccountId = accountId,
        Amount = amount,
        ExpectedCloseDate = closeDate,
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private void SetupAccounts(List<Account> accounts)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(accounts);
        _mockContext.Setup(c => c.Accounts).Returns(mockSet.Object);
    }

    private void SetupOpportunities(List<Opportunity> opportunities)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(opportunities);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetCohortAnalysisAsync Tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCohortAnalysisAsync_ShouldReturnEmpty_WhenNoAccountsInRange()
    {
        // Arrange – accounts outside the requested range
        SetupAccounts([MakeAccount(1, DateTime.UtcNow.AddYears(-2))]);
        SetupOpportunities([]);

        var request = new CohortAnalysisRequestDto
        {
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = DateTime.UtcNow,
            CohortType = ReportCohortType.Monthly,
            MetricType = CohortMetricType.Retention
        };

        // Act
        var result = await _service.GetCohortAnalysisAsync(request);

        // Assert
        result.Cohorts.Should().BeEmpty();
        result.Periods.Should().NotBeEmpty(); // Period headers still built from date range
    }

    [Fact]
    public async Task GetCohortAnalysisAsync_ShouldReturnCohortRows_WhenAccountsExistInRange()
    {
        // Arrange — 3 accounts created in first month of range
        var base_ = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-2);
        SetupAccounts([
            MakeAccount(1, base_.AddDays(1)),
            MakeAccount(2, base_.AddDays(2)),
            MakeAccount(3, base_.AddDays(3))
        ]);
        SetupOpportunities([]);

        var request = new CohortAnalysisRequestDto
        {
            StartDate = base_,
            EndDate = base_.AddMonths(2),
            CohortType = ReportCohortType.Monthly,
            MetricType = CohortMetricType.Retention
        };

        // Act
        var result = await _service.GetCohortAnalysisAsync(request);

        // Assert
        result.Periods.Should().HaveCountGreaterThan(0);
        result.Cohorts.Should().HaveCountGreaterThan(0);

        var firstCohort = result.Cohorts.First();
        firstCohort.InitialCount.Should().Be(3);
        firstCohort.Values.Should().HaveCount(result.Periods.Count);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetCustomerSegmentsAsync Tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCustomerSegmentsAsync_ShouldGroupByIndustry_WhenSegmentByIndustry()
    {
        // Arrange
        SetupAccounts([
            MakeAccount(1, DateTime.UtcNow.AddMonths(-1), industry: "Tech"),
            MakeAccount(2, DateTime.UtcNow.AddMonths(-1), industry: "Tech"),
            MakeAccount(3, DateTime.UtcNow.AddMonths(-1), industry: "Finance")
        ]);
        SetupOpportunities([]);

        var criteria = new SegmentationCriteria { SegmentBy = SegmentBy.Industry };

        // Act
        var result = (await _service.GetCustomerSegmentsAsync(criteria)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(s => s.SegmentName == "Tech" && s.CustomerCount == 2);
        result.Should().ContainSingle(s => s.SegmentName == "Finance" && s.CustomerCount == 1);
    }

    [Fact]
    public async Task GetCustomerSegmentsAsync_ShouldGroupByRevenueBand_WhenSegmentByRevenue()
    {
        // Arrange — MakeAccount sets AnnualRevenue = id * 100_000
        // id=1 → $100K (border), id=5 → $500K, id=20 → $2M
        SetupAccounts([
            MakeAccount(1, DateTime.UtcNow.AddMonths(-1)),  // 100_000 → $100K–$1M
            MakeAccount(5, DateTime.UtcNow.AddMonths(-1)),  // 500_000 → $100K–$1M
            MakeAccount(20, DateTime.UtcNow.AddMonths(-1))  // 2_000_000 → $1M–$10M
        ]);
        SetupOpportunities([]);

        var criteria = new SegmentationCriteria { SegmentBy = SegmentBy.Revenue };

        // Act
        var result = (await _service.GetCustomerSegmentsAsync(criteria)).ToList();

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Should().ContainSingle(s => s.SegmentName == "$100K\u2013$1M" && s.CustomerCount == 2);
        result.Should().ContainSingle(s => s.SegmentName == "$1M\u2013$10M" && s.CustomerCount == 1);
    }
}
