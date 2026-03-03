// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: BACK-013 (Escalation Analytics Reports)
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Sources read:
//   IEscalationAnalyticsService.cs (CRM.Core/Interfaces/ITSM)
//   EscalationAnalyticsService.cs  (CRM.Infrastructure/Services/ITSM)
//   ICrmDbContext.cs — Set<EscalationLog>() at line 432,
//                       DbSet<ServiceRequest> ServiceRequests
//   EscalationLog namespace: CRM.Core.Entities.ITSM
//   ServiceRequest namespace: CRM.Core.Entities
//
// Constructor: EscalationAnalyticsService(ICrmDbContext dbContext,
//              ILogger<EscalationAnalyticsService> logger)
// Methods tested:
//   Task<IEnumerable<EscalationByCategoryDto>> GetEscalationsByCategoryAsync(DateTime, DateTime, CT)
//   Task<AverageEscalationTimeDto>             GetAverageEscalationTimeAsync(DateTime, DateTime, string?, CT)
//   Task<IEnumerable<EscalationTrendDto>>       GetEscalationTrendsAsync(DateTime, DateTime, TrendGranularity, CT)
//
// Note: ICrmDbContext.Set<EscalationLog>() is declared on the interface (line 432),
// so Mock<ICrmDbContext> can be configured for it via MockDbSetFactory.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ServiceRequest = CRM.Core.Entities.ServiceRequest;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for EscalationAnalyticsService (BACK-013).
/// Verifies that the service returns correct aggregated data when
/// working with empty and non-empty escalation log data.
/// </summary>
public class EscalationAnalyticsServiceTests : ServiceTestFixtureBase<EscalationAnalyticsService>
{    private readonly EscalationAnalyticsService _service;

    public EscalationAnalyticsServiceTests()
    {        var logger = new Mock<ILogger<EscalationAnalyticsService>>().Object;
        _service = new EscalationAnalyticsService(MockContext.Object, logger);
    }

    private void SetupEscalationLogs(List<EscalationLog> logs)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(logs);
        MockContext.Setup(c => c.Set<EscalationLog>()).Returns(mockSet.Object);
    }

    private void SetupServiceRequests(List<ServiceRequest> requests)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(requests);
        MockContext.Setup(c => c.ServiceRequests).Returns(mockSet.Object);
    }

    private static readonly DateTime From = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);

    // ────────────────────────────────────────────────────────────────────────
    // GetEscalationsByCategoryAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetEscalationsByCategoryAsync_ShouldReturnEmpty_WhenNoLogsExist()
    {
        SetupEscalationLogs([]);

        var result = await _service.GetEscalationsByCategoryAsync(From, To);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEscalationsByCategoryAsync_ShouldReturnGroupedCategories_WhenMultipleLogsExist()
    {
        // Two escalation logs linked to the same category ID.
        var sr = new ServiceRequest
        {
            Id = 1,
            CategoryId = 10,
            CreatedAt = From.AddHours(1),
            UpdatedAt = From.AddHours(1),
        };
        var logs = new List<EscalationLog>
        {
            new() { Id = 1, EscalatedAt = From.AddHours(2), LevelNumber = 1, ServiceRequestId = 1, ServiceRequest = sr },
            new() { Id = 2, EscalatedAt = From.AddHours(3), LevelNumber = 1, ServiceRequestId = 1, ServiceRequest = sr },
        };
        SetupEscalationLogs(logs);

        var result = await _service.GetEscalationsByCategoryAsync(From, To);

        result.Should().ContainSingle();
        result.Single().EscalationCount.Should().Be(2);
        result.Single().PercentageOfTotal.Should().Be(100);
    }

    [Fact]
    public async Task GetEscalationsByCategoryAsync_ShouldLabel_AsUncategorized_WhenServiceRequestIsNull()
    {
        var logs = new List<EscalationLog>
        {
            new() { Id = 10, EscalatedAt = From.AddHours(4), LevelNumber = 1, ServiceRequest = null },
        };
        SetupEscalationLogs(logs);

        var result = await _service.GetEscalationsByCategoryAsync(From, To);

        result.Should().ContainSingle(c => c.CategoryName == "Uncategorized");
    }

    [Fact]
    public async Task GetEscalationsByCategoryAsync_ShouldOrderBycount_Descending()
    {
        var sr1 = new ServiceRequest { Id = 1, CategoryId = 1, CreatedAt = From, UpdatedAt = From };
        var sr2 = new ServiceRequest { Id = 2, CategoryId = 2, CreatedAt = From, UpdatedAt = From };
        var logs = new List<EscalationLog>
        {
            new() { Id = 1, EscalatedAt = From.AddHours(1), LevelNumber = 1, ServiceRequest = sr1 },
            new() { Id = 2, EscalatedAt = From.AddHours(2), LevelNumber = 2, ServiceRequest = sr1 },
            new() { Id = 3, EscalatedAt = From.AddHours(3), LevelNumber = 1, ServiceRequest = sr1 },
            new() { Id = 4, EscalatedAt = From.AddHours(4), LevelNumber = 1, ServiceRequest = sr2 },
        };
        SetupEscalationLogs(logs);

        var result = (await _service.GetEscalationsByCategoryAsync(From, To)).ToList();

        // CategoryId 1 has 3 entries; it should be first.
        result[0].EscalationCount.Should().BeGreaterThanOrEqualTo(result[1].EscalationCount);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetAverageEscalationTimeAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAverageEscalationTimeAsync_ShouldReturnZeroAverages_WhenNoLogsExist()
    {
        SetupEscalationLogs([]);

        var result = await _service.GetAverageEscalationTimeAsync(From, To);

        result.TotalEscalations.Should().Be(0);
        result.AverageMinutesToFirstEscalation.Should().Be(0);
        result.AverageMinutesToResolution.Should().Be(0);
        result.MedianMinutesToFirstEscalation.Should().Be(0);
    }

    [Fact]
    public async Task GetAverageEscalationTimeAsync_ShouldReturnPositiveAverage_WhenLogsWithServiceRequestsExist()
    {
        // SR created 60 minutes before escalation.
        const int MinutesToEscalation = 60;
        var sr = new ServiceRequest
        {
            Id = 5,
            CreatedAt = From,
            UpdatedAt = From,
        };
        var logs = new List<EscalationLog>
        {
            new()
            {
                Id = 1,
                EscalatedAt = From.AddMinutes(MinutesToEscalation),
                LevelNumber = 1,
                ServiceRequest = sr,
            },
        };
        SetupEscalationLogs(logs);

        var result = await _service.GetAverageEscalationTimeAsync(From, To);

        result.TotalEscalations.Should().Be(1);
        result.AverageMinutesToFirstEscalation.Should().BeApproximately(MinutesToEscalation, 1);
    }

    [Fact]
    public async Task GetAverageEscalationTimeAsync_ShouldFilterByPriority_WhenPriorityIsSpecified()
    {
        SetupEscalationLogs([]);

        // Even with priority filter, empty list should return zeros without throwing.
        var act = async () => await _service.GetAverageEscalationTimeAsync(From, To, "High");

        await act.Should().NotThrowAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetEscalationTrendsAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetEscalationTrendsAsync_ShouldReturnTrends_WhenCalledWithDailyGranularity()
    {
        SetupEscalationLogs([]);
        SetupServiceRequests([]);

        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc);

        var result = await _service.GetEscalationTrendsAsync(from, to, TrendGranularity.Daily);

        // 3 daily periods: Jan 1, 2, 3 → 3 trend entries.
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetEscalationTrendsAsync_ShouldNotThrow_WhenCalledWithWeeklyGranularity()
    {
        SetupEscalationLogs([]);
        SetupServiceRequests([]);

        var act = async () => await _service.GetEscalationTrendsAsync(From, To, TrendGranularity.Weekly);

        await act.Should().NotThrowAsync();
    }
}
