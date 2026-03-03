// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: BACK-012 (Lead Aging Alerts) — ILeadAlertService
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Sources read:
//   ILeadAlertService.cs (CRM.Core/Interfaces) — defines StaleLeadAlert, AgingLeadAlert, etc.
//   LeadAlertService.cs  (CRM.Infrastructure/Services)
//   ICrmDbContext.cs — DbSet<Lead> Leads
//
// Constructor: LeadAlertService(ICrmDbContext context, ILogger<LeadAlertService> logger)
// Methods tested:
//   Task<IEnumerable<StaleLeadAlert>> CheckStaleLeadsAsync(int staleDaysThreshold, CT)
//   Task<IEnumerable<AgingLeadAlert>> GetAgingLeadsAsync(int agingDaysThreshold, CT)
//   Task<int> SendStaleLeadNotificationsAsync(int staleDaysThreshold, CT)
//
// Note: Include() and AsNoTracking() are no-ops on non-EF providers; the
// mock DbSet (TestAsyncQueryProvider) handles them transparently.

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
/// Unit tests for LeadAlertService (BACK-012).
/// </summary>
public class LeadAlertServiceTests : ServiceTestFixtureBase<LeadAlertService>
{    private readonly LeadAlertService _service;

    public LeadAlertServiceTests()
    {        _service = new LeadAlertService(MockContext.Object, MockLogger.Object);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private void SetupLeads(List<Lead> leads)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        MockContext.Setup(c => c.Leads).Returns(mockSet.Object);
    }

    private static Lead BuildLead(
        int id,
        LeadLifecycleStatus status = LeadLifecycleStatus.New,
        DateTime? lastContactedAt = null,
        DateTime? createdAt = null,
        bool isDeleted = false) => new()
    {
        Id = id,
        FirstName = "Test",
        LastName = $"Lead{id}",
        Email = $"lead{id}@example.com",
        Status = status,
        LastContactedAt = lastContactedAt,
        CreatedAt = createdAt ?? DateTime.UtcNow.AddDays(-20),
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = isDeleted,
    };

    // ────────────────────────────────────────────────────────────────────────
    // CheckStaleLeadsAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckStaleLeadsAsync_ShouldReturnEmpty_WhenNoLeadsExist()
    {
        SetupLeads([]);

        var result = await _service.CheckStaleLeadsAsync(7);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckStaleLeadsAsync_ShouldReturnStaleLead_WhenLastContactExceedsThreshold()
    {
        // Lead last contacted 30 days ago; threshold is 7 days.
        var staleLead = BuildLead(1, lastContactedAt: DateTime.UtcNow.AddDays(-30));
        SetupLeads([staleLead]);

        var result = await _service.CheckStaleLeadsAsync(staleDaysThreshold: 7);

        result.Should().ContainSingle(a => a.LeadId == 1);
    }

    [Fact]
    public async Task CheckStaleLeadsAsync_ShouldNotReturnLead_WhenContactedWithinThreshold()
    {
        // Lead last contacted 2 days ago; threshold is 7 days — not stale.
        var recentLead = BuildLead(2, lastContactedAt: DateTime.UtcNow.AddDays(-2));
        SetupLeads([recentLead]);

        var result = await _service.CheckStaleLeadsAsync(staleDaysThreshold: 7);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckStaleLeadsAsync_ShouldExcludeConvertedLeads()
    {
        var converted = BuildLead(3, LeadLifecycleStatus.Converted, lastContactedAt: DateTime.UtcNow.AddDays(-60));
        SetupLeads([converted]);

        var result = await _service.CheckStaleLeadsAsync(7);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckStaleLeadsAsync_ShouldExcludeDisqualifiedLeads()
    {
        var disqualified = BuildLead(4, LeadLifecycleStatus.Disqualified, lastContactedAt: DateTime.UtcNow.AddDays(-90));
        SetupLeads([disqualified]);

        var result = await _service.CheckStaleLeadsAsync(7);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckStaleLeadsAsync_ShouldExcludeSoftDeletedLeads()
    {
        var deleted = BuildLead(5, lastContactedAt: DateTime.UtcNow.AddDays(-30), isDeleted: true);
        SetupLeads([deleted]);

        var result = await _service.CheckStaleLeadsAsync(7);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckStaleLeadsAsync_ShouldSetAlertLevelCritical_WhenDaysExceedTripleThreshold()
    {
        // 22 days stale vs 7-day threshold → ratio = 22/7 ≈ 3.1 → Critical
        var staleLead = BuildLead(6, lastContactedAt: DateTime.UtcNow.AddDays(-22));
        SetupLeads([staleLead]);

        var result = await _service.CheckStaleLeadsAsync(7);

        result.Single().AlertLevel.Should().Be("Critical");
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetAgingLeadsAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAgingLeadsAsync_ShouldReturnEmpty_WhenNoLeadsExist()
    {
        SetupLeads([]);

        var result = await _service.GetAgingLeadsAsync(14);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAgingLeadsAsync_ShouldReturnAgingLead_WhenNewLeadCreatedBeforeThreshold()
    {
        // Lead created 30 days ago; threshold 14 days → aging.
        var agingLead = BuildLead(10, LeadLifecycleStatus.New, createdAt: DateTime.UtcNow.AddDays(-30));
        SetupLeads([agingLead]);

        var result = await _service.GetAgingLeadsAsync(14);

        result.Should().ContainSingle(a => a.LeadId == 10);
    }

    [Fact]
    public async Task GetAgingLeadsAsync_ShouldNotReturnLead_WhenCreatedWithinThreshold()
    {
        // Lead created 5 days ago; threshold 14 days → not aging.
        var newLead = BuildLead(11, LeadLifecycleStatus.New, createdAt: DateTime.UtcNow.AddDays(-5));
        SetupLeads([newLead]);

        var result = await _service.GetAgingLeadsAsync(14);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAgingLeadsAsync_ShouldOnlyReturnNewStatusLeads()
    {
        // Working leads aged > threshold but not in New status should be excluded.
        var contacted = BuildLead(12, LeadLifecycleStatus.Working, createdAt: DateTime.UtcNow.AddDays(-30));
        SetupLeads([contacted]);

        var result = await _service.GetAgingLeadsAsync(14);

        result.Should().BeEmpty();
    }

    // ────────────────────────────────────────────────────────────────────────
    // SendStaleLeadNotificationsAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SendStaleLeadNotificationsAsync_ShouldReturnZero_WhenNoStaleLeadsExist()
    {
        SetupLeads([]);

        var count = await _service.SendStaleLeadNotificationsAsync(7);

        count.Should().Be(0);
    }

    [Fact]
    public async Task SendStaleLeadNotificationsAsync_ShouldReturnOwnerCount_WhenStaleLeadsHaveOwners()
    {
        // Two stale leads with the SAME owner → 1 notification group.
        var lead1 = BuildLead(20, lastContactedAt: DateTime.UtcNow.AddDays(-30));
        lead1.OwnerId = 5;
        var lead2 = BuildLead(21, lastContactedAt: DateTime.UtcNow.AddDays(-25));
        lead2.OwnerId = 5;
        SetupLeads([lead1, lead2]);

        var count = await _service.SendStaleLeadNotificationsAsync(7);

        count.Should().Be(1); // 1 owner, 1 notification
    }

    [Fact]
    public async Task SendStaleLeadNotificationsAsync_ShouldReturnZero_WhenStaleLeadsHaveNoOwner()
    {
        var ownerless = BuildLead(30, lastContactedAt: DateTime.UtcNow.AddDays(-30));
        ownerless.OwnerId = null;
        SetupLeads([ownerless]);

        var count = await _service.SendStaleLeadNotificationsAsync(7);

        // Ownerless leads are skipped in notification grouping.
        count.Should().Be(0);
    }
}
