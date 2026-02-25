// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: SPEC-CRM002-07 (Lead Aging Alerts)
// TODO-CRM002-07: Lead Aging Alerts — unit tests
//
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Source files read: LeadAgingAlertService.cs, ILeadAgingAlertService.cs,
//   Lead.cs, ICrmDbContext.cs

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for LeadAgingAlertService.GetStaledLeadsAsync (TODO-CRM002-07).
/// Verifies empty results, Warning/Critical staleness levels, and exclusion of converted/disqualified leads.
/// </summary>
public class LeadAgingAlertServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly LeadAgingAlertService _service;

    public LeadAgingAlertServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _service = new LeadAgingAlertService(_mockContext.Object, Mock.Of<ILogger<LeadAgingAlertService>>());
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private void SetupLeads(List<Lead> leads)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);
    }

    private static Lead CreateLead(
        int id,
        LeadLifecycleStatus status = LeadLifecycleStatus.New,
        DateTime? lastActivityDate = null,
        DateTime? createdAt = null) => new()
    {
        Id = id,
        FirstName = "Test",
        LastName = $"Lead{id}",
        Email = $"lead{id}@test.com",
        Status = status,
        LastActivityDate = lastActivityDate,
        CreatedAt = createdAt ?? DateTime.UtcNow.AddDays(-1),
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

    // ────────────────────────────────────────────────────────────────────────
    // Tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStaledLeadsAsync_ShouldReturnEmpty_WhenNoLeadsExceedThreshold()
    {
        // Arrange — lead was active only 5 days ago; threshold is 14 days
        var recentLead = CreateLead(1, lastActivityDate: DateTime.UtcNow.AddDays(-5));
        SetupLeads([recentLead]);

        // Act
        var result = await _service.GetStaledLeadsAsync(staleDaysThreshold: 14);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStaledLeadsAsync_ShouldReturnWarning_WhenLeadIsStaleButNotCritical()
    {
        // Arrange — lead was active 20 days ago; threshold=14 → stale; criticalDays=30 → Warning
        var staleLead = CreateLead(1, lastActivityDate: DateTime.UtcNow.AddDays(-20));
        SetupLeads([staleLead]);

        // Act
        var result = (await _service.GetStaledLeadsAsync(staleDaysThreshold: 14)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].LeadId.Should().Be(1);
        result[0].StalenessLevel.Should().Be("Warning");
        result[0].DaysSinceLastActivity.Should().BeGreaterThanOrEqualTo(19);
    }

    [Fact]
    public async Task GetStaledLeadsAsync_ShouldReturnCritical_WhenLeadExceedsCriticalThreshold()
    {
        // Arrange — staleDaysThreshold=14 → criticalDays=Max(30,28)=30; lead last active 35 days ago → Critical
        var criticalLead = CreateLead(2, lastActivityDate: DateTime.UtcNow.AddDays(-35));
        SetupLeads([criticalLead]);

        // Act
        var result = (await _service.GetStaledLeadsAsync(staleDaysThreshold: 14)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].StalenessLevel.Should().Be("Critical");
        result[0].DaysSinceLastActivity.Should().BeGreaterThanOrEqualTo(35);
    }

    [Fact]
    public async Task GetStaledLeadsAsync_ShouldExcludeConvertedAndDisqualifiedLeads()
    {
        // Arrange — three stale leads; only the New one should appear
        var staleDays = 14;
        var convertedLead = CreateLead(1, LeadLifecycleStatus.Converted, DateTime.UtcNow.AddDays(-30));
        var disqualifiedLead = CreateLead(2, LeadLifecycleStatus.Disqualified, DateTime.UtcNow.AddDays(-30));
        var openLead = CreateLead(3, LeadLifecycleStatus.New, DateTime.UtcNow.AddDays(-30));
        SetupLeads([convertedLead, disqualifiedLead, openLead]);

        // Act
        var result = (await _service.GetStaledLeadsAsync(staleDaysThreshold: staleDays)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].LeadId.Should().Be(3);
    }
}
