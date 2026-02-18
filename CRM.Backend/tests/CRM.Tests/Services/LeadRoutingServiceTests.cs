// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for LeadRoutingService
/// Tests cover routing rule management, criteria, targets, and lead routing logic
/// </summary>
public class LeadRoutingServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<LeadRoutingService>> _mockLogger;
    private readonly LeadRoutingService _service;

    public LeadRoutingServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<LeadRoutingService>>();

        _service = new LeadRoutingService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    #region Helper Methods

    private LeadRoutingRule CreateTestRule(int id = 0, string name = "Test Rule")
    {
        return new LeadRoutingRule
        {
            Id = id,
            Name = name,
            Description = "Test routing rule description",
            Status = RoutingRuleStatus.Active,
            Priority = 1,
            AssignmentType = LeadAssignmentType.RoundRobin,
            AssignToTeam = false,
            BusinessHoursOnly = false,
            SendNotification = true,
            NotifyManager = false,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private Lead CreateTestLead(int id = 0, string email = "lead@test.com")
    {
        return new Lead
        {
            Id = id,
            Email = email,
            FirstName = "Test",
            LastName = "Lead",
            CompanyName = "Test Company",
            Status = LeadLifecycleStatus.New,
            Source = LeadSource.Web,
            Region = "North America",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private User CreateTestUser(int id = 0, string username = "testuser")
    {
        return new User
        {
            Id = id,
            Username = username,
            Email = $"{username}@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hashedpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private LeadRoutingTarget CreateTestTarget(int ruleId, int userId, int weight = 100)
    {
        return new LeadRoutingTarget
        {
            LeadRoutingRuleId = ruleId,
            UserId = userId,
            Weight = weight,
            IsActive = true,
            MaxLeadsPerDay = 10,
            MaxLeadsPerWeek = 50,
            LeadsAssignedToday = 0,
            LeadsAssignedThisWeek = 0,
            TotalLeadsAssigned = 0,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private LeadRoutingCriteria CreateTestCriteria(int ruleId, RoutingCriteriaType criteriaType = RoutingCriteriaType.LeadSource, string value = "Web")
    {
        return new LeadRoutingCriteria
        {
            LeadRoutingRuleId = ruleId,
            CriteriaType = criteriaType,
            Operator = "equals",
            Value = value,
            LogicalOperator = "AND",
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    #endregion

    #region Routing Rule Management Tests

    [Fact]
    public async Task GetAllRulesAsync_ReturnsAllActiveRules()
    {
        // Arrange
        var rule1 = CreateTestRule(name: "Rule 1");
        rule1.Priority = 1;
        var rule2 = CreateTestRule(name: "Rule 2");
        rule2.Priority = 2;
        var deletedRule = CreateTestRule(name: "Deleted Rule");
        deletedRule.IsDeleted = true;

        await _dbContext.LeadRoutingRules.AddRangeAsync(rule1, rule2, deletedRule);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllRulesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => !r.IsDeleted);
        result.First().Name.Should().Be("Rule 1"); // Ordered by priority
    }

    [Fact]
    public async Task GetAllRulesAsync_WithStatusFilter_ReturnsFilteredRules()
    {
        // Arrange
        var activeRule = CreateTestRule(name: "Active Rule");
        activeRule.Status = RoutingRuleStatus.Active;
        var inactiveRule = CreateTestRule(name: "Inactive Rule");
        inactiveRule.Status = RoutingRuleStatus.Inactive;

        await _dbContext.LeadRoutingRules.AddRangeAsync(activeRule, inactiveRule);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllRulesAsync(status: RoutingRuleStatus.Active);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active Rule");
    }

    [Fact]
    public async Task GetRuleByIdAsync_WhenRuleExists_ReturnsRule()
    {
        // Arrange
        var rule = CreateTestRule(name: "Test Rule");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetRuleByIdAsync(rule.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Rule");
    }

    [Fact]
    public async Task GetRuleByIdAsync_WhenRuleNotExists_ReturnsNull()
    {
        // Act
        var result = await _service.GetRuleByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRuleByIdAsync_WhenRuleIsDeleted_ReturnsNull()
    {
        // Arrange
        var rule = CreateTestRule(name: "Deleted Rule");
        rule.IsDeleted = true;
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetRuleByIdAsync(rule.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateRuleAsync_CreatesAndReturnsRule()
    {
        // Arrange
        var rule = CreateTestRule(name: "New Rule");

        // Act
        var result = await _service.CreateRuleAsync(rule);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Rule");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var saved = await _dbContext.LeadRoutingRules.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateRuleAsync_UpdatesExistingRule()
    {
        // Arrange
        var rule = CreateTestRule(name: "Original Name");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        // Act
        rule.Name = "Updated Name";
        rule.Description = "Updated description";
        var result = await _service.UpdateRuleAsync(rule);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated description");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateRuleAsync_WhenRuleNotExists_ThrowsException()
    {
        // Arrange
        var nonExistentRule = CreateTestRule(name: "Non-existent");
        nonExistentRule.Id = 999;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateRuleAsync(nonExistentRule));
    }

    [Fact]
    public async Task DeleteRuleAsync_SoftDeletesRule()
    {
        // Arrange
        var rule = CreateTestRule(name: "To Delete");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeleteRuleAsync(rule.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _dbContext.LeadRoutingRules.FindAsync(rule.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateRuleAsync_SetsStatusToActive()
    {
        // Arrange
        var rule = CreateTestRule(name: "Inactive Rule");
        rule.Status = RoutingRuleStatus.Inactive;
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ActivateRuleAsync(rule.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(RoutingRuleStatus.Active);
    }

    [Fact]
    public async Task DeactivateRuleAsync_SetsStatusToInactive()
    {
        // Arrange
        var rule = CreateTestRule(name: "Active Rule");
        rule.Status = RoutingRuleStatus.Active;
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeactivateRuleAsync(rule.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(RoutingRuleStatus.Inactive);
    }

    #endregion

    #region Criteria Management Tests

    [Fact]
    public async Task AddCriteriaAsync_AddsCriteriaToRule()
    {
        // Arrange
        var rule = CreateTestRule(name: "Rule with Criteria");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        var criteria = CreateTestCriteria(rule.Id, RoutingCriteriaType.LeadSource, "Web");

        // Act
        var result = await _service.AddCriteriaAsync(rule.Id, criteria);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.CriteriaType.Should().Be(RoutingCriteriaType.LeadSource);
        result.Value.Should().Be("Web");
        result.LeadRoutingRuleId.Should().Be(rule.Id);
    }

    [Fact]
    public async Task GetCriteriaAsync_ReturnsCriteriaForRule()
    {
        // Arrange
        var rule = CreateTestRule(name: "Rule with Criteria");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        var criteria1 = CreateTestCriteria(rule.Id, RoutingCriteriaType.Industry, "Technology");
        var criteria2 = CreateTestCriteria(rule.Id, RoutingCriteriaType.LeadSource, "Web");
        await _dbContext.LeadRoutingCriteria.AddRangeAsync(criteria1, criteria2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetCriteriaAsync(rule.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.CriteriaType == RoutingCriteriaType.Industry);
        result.Should().Contain(c => c.CriteriaType == RoutingCriteriaType.LeadSource);
    }

    [Fact]
    public async Task RemoveCriteriaAsync_DeletesCriteria()
    {
        // Arrange
        var rule = CreateTestRule(name: "Rule with Criteria");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        var criteria = CreateTestCriteria(rule.Id, RoutingCriteriaType.Industry, "Technology");
        await _dbContext.LeadRoutingCriteria.AddAsync(criteria);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RemoveCriteriaAsync(criteria.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _dbContext.LeadRoutingCriteria.FindAsync(criteria.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Target Management Tests

    [Fact]
    public async Task AddTargetAsync_AddsTargetToRule()
    {
        // Arrange
        var rule = CreateTestRule(name: "Rule with Target");
        var user = CreateTestUser(username: "targetuser");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var target = CreateTestTarget(rule.Id, user.Id);

        // Act
        var result = await _service.AddTargetAsync(rule.Id, target);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be(user.Id);
        result.LeadRoutingRuleId.Should().Be(rule.Id);
    }

    [Fact]
    public async Task GetTargetsAsync_ReturnsTargetsForRule()
    {
        // Arrange
        var rule = CreateTestRule(name: "Rule with Targets");
        var user1 = CreateTestUser(username: "user1");
        var user2 = CreateTestUser(username: "user2");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.Users.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();

        var target1 = CreateTestTarget(rule.Id, user1.Id, weight: 50);
        var target2 = CreateTestTarget(rule.Id, user2.Id, weight: 100);
        await _dbContext.LeadRoutingTargets.AddRangeAsync(target1, target2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetTargetsAsync(rule.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task RemoveTargetAsync_DeletesTarget()
    {
        // Arrange
        var rule = CreateTestRule(name: "Rule with Target");
        var user = CreateTestUser(username: "targetuser");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var target = CreateTestTarget(rule.Id, user.Id);
        await _dbContext.LeadRoutingTargets.AddAsync(target);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RemoveTargetAsync(target.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _dbContext.LeadRoutingTargets.FindAsync(target.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Lead Routing Tests

    [Fact]
    public async Task RouteLeadAsync_WhenNoMatchingRules_ReturnsNotRouted()
    {
        // Arrange
        var lead = CreateTestLead(email: "unmatched@test.com");
        await _dbContext.Leads.AddAsync(lead);
        await _dbContext.SaveChangesAsync();

        // No routing rules set up

        // Act
        var result = await _service.RouteLeadAsync(lead.Id);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.AssignedToUserId.Should().BeNull();
    }

    [Fact]
    public async Task RouteLeadAsync_WithMatchingRule_RoutesToTarget()
    {
        // Arrange
        var user = CreateTestUser(username: "salesrep");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var rule = CreateTestRule(name: "Web Leads");
        rule.AssignmentType = LeadAssignmentType.RoundRobin;
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        var criteria = CreateTestCriteria(rule.Id, RoutingCriteriaType.LeadSource, "Web");
        await _dbContext.LeadRoutingCriteria.AddAsync(criteria);

        var target = CreateTestTarget(rule.Id, user.Id);
        await _dbContext.LeadRoutingTargets.AddAsync(target);
        await _dbContext.SaveChangesAsync();

        var lead = CreateTestLead(email: "web@company.com");
        lead.Source = LeadSource.Web;
        await _dbContext.Leads.AddAsync(lead);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RouteLeadAsync(lead.Id);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.AssignedToUserId.Should().Be(user.Id);
        result.MatchedRuleId.Should().Be(rule.Id);
    }

    [Fact]
    public async Task EvaluateMatchingRulesAsync_ReturnsMatchingRules()
    {
        // Arrange
        var rule1 = CreateTestRule(name: "Rule 1");
        var rule2 = CreateTestRule(name: "Rule 2");
        await _dbContext.LeadRoutingRules.AddRangeAsync(rule1, rule2);
        await _dbContext.SaveChangesAsync();

        var criteria1 = CreateTestCriteria(rule1.Id, RoutingCriteriaType.LeadSource, "Web");
        var criteria2 = CreateTestCriteria(rule2.Id, RoutingCriteriaType.LeadSource, "Referral");
        await _dbContext.LeadRoutingCriteria.AddRangeAsync(criteria1, criteria2);
        await _dbContext.SaveChangesAsync();

        var lead = CreateTestLead(email: "web@company.com");
        lead.Source = LeadSource.Web;
        await _dbContext.Leads.AddAsync(lead);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.EvaluateMatchingRulesAsync(lead.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Rule 1");
    }

    [Fact]
    public async Task RerouteLeadAsync_ReassignsLead()
    {
        // Arrange
        var user1 = CreateTestUser(username: "oldrep");
        var user2 = CreateTestUser(username: "newrep");
        await _dbContext.Users.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();

        var rule = CreateTestRule(name: "Reroute Rule");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        var target1 = CreateTestTarget(rule.Id, user1.Id, weight: 50);
        target1.TotalLeadsAssigned = 100; // Already has many leads
        var target2 = CreateTestTarget(rule.Id, user2.Id, weight: 100);
        target2.TotalLeadsAssigned = 0; // Fresh target
        await _dbContext.LeadRoutingTargets.AddRangeAsync(target1, target2);
        await _dbContext.SaveChangesAsync();

        var lead = CreateTestLead(email: "reroute@test.com");
        lead.OwnerId = user1.Id; // Currently assigned to user1
        await _dbContext.Leads.AddAsync(lead);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RerouteLeadAsync(lead.Id, "Customer requested different rep");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Routing Logs Tests

    [Fact]
    public async Task GetLeadRoutingHistoryAsync_ReturnsHistoryForLead()
    {
        // Arrange
        var lead = CreateTestLead(email: "history@test.com");
        await _dbContext.Leads.AddAsync(lead);
        await _dbContext.SaveChangesAsync();

        var log1 = new LeadRoutingLog
        {
            LeadId = lead.Id,
            AssignedAt = DateTime.UtcNow.AddDays(-1),
            Success = true,
            AssignmentType = LeadAssignmentType.RoundRobin,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var log2 = new LeadRoutingLog
        {
            LeadId = lead.Id,
            AssignedAt = DateTime.UtcNow,
            Success = true,
            AssignmentType = LeadAssignmentType.RoundRobin,
            FailureReason = null,
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.LeadRoutingLogs.AddRangeAsync(log1, log2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetLeadRoutingHistoryAsync(lead.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(l => l.AssignedAt);
    }

    #endregion

    #region Analytics Tests

    [Fact]
    public async Task GetRuleStatisticsAsync_ReturnsStatistics()
    {
        // Arrange
        var rule = CreateTestRule(name: "Stats Rule");
        await _dbContext.LeadRoutingRules.AddAsync(rule);
        await _dbContext.SaveChangesAsync();

        // Add some routing logs
        var logs = new[]
        {
            new LeadRoutingLog { LeadId = 1, LeadRoutingRuleId = rule.Id, Success = true, AssignedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new LeadRoutingLog { LeadId = 2, LeadRoutingRuleId = rule.Id, Success = true, AssignedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new LeadRoutingLog { LeadId = 3, LeadRoutingRuleId = rule.Id, Success = false, AssignedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow }
        };
        await _dbContext.LeadRoutingLogs.AddRangeAsync(logs);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetRuleStatisticsAsync(rule.Id);

        // Assert
        result.Should().NotBeNull();
        result.TotalLeadsRouted.Should().Be(3);
        result.SuccessfulRoutes.Should().Be(2);
        result.FailedRoutes.Should().Be(1);
    }

    #endregion
}
