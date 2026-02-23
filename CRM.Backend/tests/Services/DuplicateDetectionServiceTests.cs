// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

// Resolve ambiguity between CRM.Core.Entities.MatchType and System.IO.MatchType
using MatchType = CRM.Core.Entities.MatchType;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for DuplicateDetectionService using InMemory database.
/// Tests rule CRUD, active rules filtering, candidate management,
/// status updates, and duplicate checking.
/// </summary>
public class DuplicateDetectionServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<DuplicateDetectionService>> _mockLogger;
    private readonly DuplicateDetectionService _service;

    public DuplicateDetectionServiceTests()
    {
        _dbContext = CreateDbContext();
        _mockLogger = new Mock<ILogger<DuplicateDetectionService>>();
        _service = new DuplicateDetectionService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    private CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new CrmDbContext(options, null!);
    }

    private DuplicateRule CreateTestRule(
        string name = "Test Rule",
        DuplicateEntityType entityType = DuplicateEntityType.Lead,
        bool isActive = true,
        int matchThreshold = 80,
        DuplicateAction action = DuplicateAction.Warn,
        int priority = 100,
        bool isDeleted = false)
    {
        return new DuplicateRule
        {
            Name = name,
            Description = $"Description for {name}",
            IsActive = isActive,
            EntityType = entityType,
            MatchThreshold = matchThreshold,
            Action = action,
            RunOnCreate = true,
            RunOnUpdate = true,
            Priority = priority,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task<DuplicateRule> SeedRuleAsync(
        string name = "Seeded Rule",
        DuplicateEntityType entityType = DuplicateEntityType.Lead,
        bool isActive = true,
        int matchThreshold = 80,
        DuplicateAction action = DuplicateAction.Warn,
        int priority = 100,
        bool isDeleted = false)
    {
        var rule = CreateTestRule(name, entityType, isActive, matchThreshold, action, priority, isDeleted);
        _dbContext.DuplicateRules.Add(rule);
        await _dbContext.SaveChangesAsync();
        return rule;
    }

    private async Task<DuplicateRule> SeedRuleWithFieldsAsync(
        string name = "Rule With Fields",
        DuplicateEntityType entityType = DuplicateEntityType.Lead,
        int matchThreshold = 80,
        params (string fieldName, MatchType matchType, int weight)[] fields)
    {
        var rule = await SeedRuleAsync(name, entityType, matchThreshold: matchThreshold);
        foreach (var (fieldName, matchType, weight) in fields)
        {
            var field = new DuplicateMatchField
            {
                DuplicateRuleId = rule.Id,
                FieldName = fieldName,
                DisplayName = fieldName,
                MatchType = matchType,
                Weight = weight,
                IsRequired = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.DuplicateMatchFields.Add(field);
        }
        await _dbContext.SaveChangesAsync();
        return rule;
    }

    private async Task<DuplicateCandidate> SeedCandidateAsync(
        DuplicateEntityType entityType = DuplicateEntityType.Lead,
        int sourceRecordId = 1,
        int targetRecordId = 2,
        int matchScore = 90,
        DuplicateCandidateStatus status = DuplicateCandidateStatus.Pending,
        int? ruleId = null,
        bool isDeleted = false)
    {
        var candidate = new DuplicateCandidate
        {
            EntityType = entityType,
            SourceRecordId = sourceRecordId,
            SourceRecordType = entityType.ToString(),
            TargetRecordId = targetRecordId,
            TargetRecordType = entityType.ToString(),
            MatchScore = matchScore,
            Status = status,
            DetectedAt = DateTime.UtcNow,
            DuplicateRuleId = ruleId,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.DuplicateCandidates.Add(candidate);
        await _dbContext.SaveChangesAsync();
        return candidate;
    }

    #endregion

    #region GetAllRulesAsync Tests

    [Fact]
    public async Task GetAllRulesAsync_ShouldReturnAllNonDeletedRules()
    {
        // Arrange
        await SeedRuleAsync("Rule 1", DuplicateEntityType.Lead);
        await SeedRuleAsync("Rule 2", DuplicateEntityType.Contact);
        await SeedRuleAsync("Deleted Rule", DuplicateEntityType.Lead, isDeleted: true);

        // Act
        var result = (await _service.GetAllRulesAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(r => r.Name == "Deleted Rule");
    }

    [Fact]
    public async Task GetAllRulesAsync_ShouldReturnRulesOrderedByEntityTypeThenPriority()
    {
        // Arrange
        await SeedRuleAsync("Contact Rule", DuplicateEntityType.Contact, priority: 50);
        await SeedRuleAsync("Lead Rule High Priority", DuplicateEntityType.Lead, priority: 10);
        await SeedRuleAsync("Lead Rule Low Priority", DuplicateEntityType.Lead, priority: 200);

        // Act
        var result = (await _service.GetAllRulesAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].EntityType.Should().Be(DuplicateEntityType.Lead);
        result[0].Priority.Should().Be(10);
        result[1].EntityType.Should().Be(DuplicateEntityType.Lead);
        result[1].Priority.Should().Be(200);
        result[2].EntityType.Should().Be(DuplicateEntityType.Contact);
    }

    [Fact]
    public async Task GetAllRulesAsync_ShouldReturnEmpty_WhenNoRulesExist()
    {
        // Act
        var result = await _service.GetAllRulesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllRulesAsync_ShouldIncludeMatchFields()
    {
        // Arrange
        await SeedRuleWithFieldsAsync("With Fields", DuplicateEntityType.Lead, 80,
            ("Email", MatchType.Exact, 100),
            ("FirstName", MatchType.Fuzzy, 50));

        // Act
        var result = (await _service.GetAllRulesAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].MatchFields.Should().HaveCount(2);
    }

    #endregion

    #region GetActiveRulesAsync Tests

    [Fact]
    public async Task GetActiveRulesAsync_ShouldReturnOnlyActiveRulesForEntityType()
    {
        // Arrange
        await SeedRuleAsync("Active Lead Rule", DuplicateEntityType.Lead, isActive: true);
        await SeedRuleAsync("Inactive Lead Rule", DuplicateEntityType.Lead, isActive: false);
        await SeedRuleAsync("Active Contact Rule", DuplicateEntityType.Contact, isActive: true);

        // Act
        var result = (await _service.GetActiveRulesAsync(DuplicateEntityType.Lead)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active Lead Rule");
    }

    [Fact]
    public async Task GetActiveRulesAsync_ShouldExcludeDeletedRules()
    {
        // Arrange
        await SeedRuleAsync("Active But Deleted", DuplicateEntityType.Lead, isActive: true, isDeleted: true);
        await SeedRuleAsync("Active Not Deleted", DuplicateEntityType.Lead, isActive: true);

        // Act
        var result = (await _service.GetActiveRulesAsync(DuplicateEntityType.Lead)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active Not Deleted");
    }

    [Fact]
    public async Task GetActiveRulesAsync_ShouldReturnRulesOrderedByPriority()
    {
        // Arrange
        await SeedRuleAsync("Low Priority", DuplicateEntityType.Lead, priority: 200);
        await SeedRuleAsync("High Priority", DuplicateEntityType.Lead, priority: 10);
        await SeedRuleAsync("Medium Priority", DuplicateEntityType.Lead, priority: 100);

        // Act
        var result = (await _service.GetActiveRulesAsync(DuplicateEntityType.Lead)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Priority.Should().Be(10);
        result[1].Priority.Should().Be(100);
        result[2].Priority.Should().Be(200);
    }

    [Fact]
    public async Task GetActiveRulesAsync_ShouldReturnEmpty_WhenNoActiveRulesForType()
    {
        // Arrange
        await SeedRuleAsync("Contact Rule", DuplicateEntityType.Contact, isActive: true);

        // Act
        var result = await _service.GetActiveRulesAsync(DuplicateEntityType.Lead);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region SaveRuleAsync Tests

    [Fact]
    public async Task SaveRuleAsync_ShouldCreateNewRule_WhenIdIsZero()
    {
        // Arrange
        var rule = CreateTestRule("New Rule", DuplicateEntityType.Lead);

        // Act
        var result = await _service.SaveRuleAsync(rule);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Rule");

        var fromDb = await _dbContext.DuplicateRules.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveRuleAsync_ShouldUpdateExistingRule_WhenIdIsNonZero()
    {
        // Arrange
        var rule = await SeedRuleAsync("Original Name");
        rule.Name = "Updated Name";
        rule.MatchThreshold = 90;

        // Act
        var result = await _service.SaveRuleAsync(rule);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.MatchThreshold.Should().Be(90);
    }

    [Fact]
    public async Task SaveRuleAsync_ShouldPersistAllProperties()
    {
        // Arrange
        var rule = new DuplicateRule
        {
            Name = "Full Rule",
            Description = "Full description",
            IsActive = true,
            EntityType = DuplicateEntityType.Contact,
            MatchThreshold = 75,
            Action = DuplicateAction.Block,
            RunOnCreate = true,
            RunOnUpdate = false,
            Priority = 50,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.SaveRuleAsync(rule);

        // Assert
        var fromDb = await _dbContext.DuplicateRules.FindAsync(result.Id);
        fromDb!.Name.Should().Be("Full Rule");
        fromDb.Description.Should().Be("Full description");
        fromDb.EntityType.Should().Be(DuplicateEntityType.Contact);
        fromDb.MatchThreshold.Should().Be(75);
        fromDb.Action.Should().Be(DuplicateAction.Block);
        fromDb.RunOnCreate.Should().BeTrue();
        fromDb.RunOnUpdate.Should().BeFalse();
        fromDb.Priority.Should().Be(50);
    }

    [Fact]
    public async Task SaveRuleAsync_ShouldHandleAllEntityTypes()
    {
        // Arrange & Act
        var leadRule = await _service.SaveRuleAsync(CreateTestRule("Lead Rule", DuplicateEntityType.Lead));
        var contactRule = await _service.SaveRuleAsync(CreateTestRule("Contact Rule", DuplicateEntityType.Contact));
        var accountRule = await _service.SaveRuleAsync(CreateTestRule("Account Rule", DuplicateEntityType.Account));

        // Assert
        leadRule.EntityType.Should().Be(DuplicateEntityType.Lead);
        contactRule.EntityType.Should().Be(DuplicateEntityType.Contact);
        accountRule.EntityType.Should().Be(DuplicateEntityType.Account);
    }

    [Fact]
    public async Task SaveRuleAsync_ShouldHandleAllDuplicateActions()
    {
        // Arrange & Act
        var warnRule = await _service.SaveRuleAsync(CreateTestRule("Warn", action: DuplicateAction.Warn));
        var blockRule = await _service.SaveRuleAsync(CreateTestRule("Block", action: DuplicateAction.Block));
        var autoMergeRule = await _service.SaveRuleAsync(CreateTestRule("AutoMerge", action: DuplicateAction.AutoMerge));
        var reviewRule = await _service.SaveRuleAsync(CreateTestRule("Review", action: DuplicateAction.QueueForReview));
        var logRule = await _service.SaveRuleAsync(CreateTestRule("LogOnly", action: DuplicateAction.LogOnly));

        // Assert
        warnRule.Action.Should().Be(DuplicateAction.Warn);
        blockRule.Action.Should().Be(DuplicateAction.Block);
        autoMergeRule.Action.Should().Be(DuplicateAction.AutoMerge);
        reviewRule.Action.Should().Be(DuplicateAction.QueueForReview);
        logRule.Action.Should().Be(DuplicateAction.LogOnly);
    }

    #endregion

    #region DeleteRuleAsync Tests

    [Fact]
    public async Task DeleteRuleAsync_ShouldSoftDeleteRule_WhenRuleExists()
    {
        // Arrange
        var rule = await SeedRuleAsync("To Delete");

        // Act
        var result = await _service.DeleteRuleAsync(rule.Id);

        // Assert
        result.Should().BeTrue();
        var fromDb = await _dbContext.DuplicateRules.FindAsync(rule.Id);
        fromDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRuleAsync_ShouldReturnFalse_WhenRuleNotFound()
    {
        // Act
        var result = await _service.DeleteRuleAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteRuleAsync_ShouldNotHardDelete()
    {
        // Arrange
        var rule = await SeedRuleAsync("Soft Delete Check");

        // Act
        await _service.DeleteRuleAsync(rule.Id);

        // Assert
        var fromDb = await _dbContext.DuplicateRules.FindAsync(rule.Id);
        fromDb.Should().NotBeNull();
        fromDb!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region GetPendingCandidatesAsync Tests

    [Fact]
    public async Task GetPendingCandidatesAsync_ShouldReturnOnlyPendingCandidates()
    {
        // Arrange
        await SeedCandidateAsync(status: DuplicateCandidateStatus.Pending, sourceRecordId: 1, targetRecordId: 2);
        await SeedCandidateAsync(status: DuplicateCandidateStatus.Confirmed, sourceRecordId: 3, targetRecordId: 4);
        await SeedCandidateAsync(status: DuplicateCandidateStatus.Pending, sourceRecordId: 5, targetRecordId: 6);

        // Act
        var result = (await _service.GetPendingCandidatesAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.Status.Should().Be(DuplicateCandidateStatus.Pending));
    }

    [Fact]
    public async Task GetPendingCandidatesAsync_ShouldFilterByEntityType_WhenProvided()
    {
        // Arrange
        await SeedCandidateAsync(DuplicateEntityType.Lead, sourceRecordId: 1, targetRecordId: 2);
        await SeedCandidateAsync(DuplicateEntityType.Contact, sourceRecordId: 3, targetRecordId: 4);

        // Act
        var result = (await _service.GetPendingCandidatesAsync(DuplicateEntityType.Lead)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].EntityType.Should().Be(DuplicateEntityType.Lead);
    }

    [Fact]
    public async Task GetPendingCandidatesAsync_ShouldOrderByMatchScoreDescending()
    {
        // Arrange
        await SeedCandidateAsync(matchScore: 70, sourceRecordId: 1, targetRecordId: 2);
        await SeedCandidateAsync(matchScore: 95, sourceRecordId: 3, targetRecordId: 4);
        await SeedCandidateAsync(matchScore: 85, sourceRecordId: 5, targetRecordId: 6);

        // Act
        var result = (await _service.GetPendingCandidatesAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].MatchScore.Should().Be(95);
        result[1].MatchScore.Should().Be(85);
        result[2].MatchScore.Should().Be(70);
    }

    [Fact]
    public async Task GetPendingCandidatesAsync_ShouldRespectPagination()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            await SeedCandidateAsync(matchScore: 90 - i, sourceRecordId: i * 2 + 1, targetRecordId: i * 2 + 2);
        }

        // Act
        var page1 = (await _service.GetPendingCandidatesAsync(page: 1, pageSize: 2)).ToList();
        var page2 = (await _service.GetPendingCandidatesAsync(page: 2, pageSize: 2)).ToList();

        // Assert
        page1.Should().HaveCount(2);
        page2.Should().HaveCount(2);
        page1[0].MatchScore.Should().BeGreaterThan(page2[0].MatchScore);
    }

    [Fact]
    public async Task GetPendingCandidatesAsync_ShouldExcludeDeletedCandidates()
    {
        // Arrange
        await SeedCandidateAsync(sourceRecordId: 1, targetRecordId: 2);
        await SeedCandidateAsync(sourceRecordId: 3, targetRecordId: 4, isDeleted: true);

        // Act
        var result = (await _service.GetPendingCandidatesAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPendingCandidatesAsync_ShouldReturnEmpty_WhenNoPendingCandidates()
    {
        // Arrange
        await SeedCandidateAsync(status: DuplicateCandidateStatus.Confirmed, sourceRecordId: 1, targetRecordId: 2);

        // Act
        var result = await _service.GetPendingCandidatesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateCandidateStatusAsync Tests

    [Fact]
    public async Task UpdateCandidateStatusAsync_ShouldUpdateStatus_WhenCandidateExists()
    {
        // Arrange
        var candidate = await SeedCandidateAsync(sourceRecordId: 1, targetRecordId: 2);

        // Act
        var result = await _service.UpdateCandidateStatusAsync(
            candidate.Id, DuplicateCandidateStatus.Confirmed, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(DuplicateCandidateStatus.Confirmed);
        result.ReviewedById.Should().Be(1);
        result.ReviewedAt.Should().NotBeNull();
        result.ReviewedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateCandidateStatusAsync_ShouldReturnNull_WhenCandidateNotFound()
    {
        // Act
        var result = await _service.UpdateCandidateStatusAsync(999, DuplicateCandidateStatus.Confirmed, userId: 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCandidateStatusAsync_ShouldSetNotes_WhenProvided()
    {
        // Arrange
        var candidate = await SeedCandidateAsync(sourceRecordId: 1, targetRecordId: 2);

        // Act
        var result = await _service.UpdateCandidateStatusAsync(
            candidate.Id, DuplicateCandidateStatus.Rejected, userId: 1, notes: "Not a real duplicate");

        // Assert
        result.Should().NotBeNull();
        result!.Notes.Should().Be("Not a real duplicate");
    }

    [Fact]
    public async Task UpdateCandidateStatusAsync_ShouldUpdateToMerged()
    {
        // Arrange
        var candidate = await SeedCandidateAsync(sourceRecordId: 1, targetRecordId: 2);

        // Act
        var result = await _service.UpdateCandidateStatusAsync(
            candidate.Id, DuplicateCandidateStatus.Merged, userId: 5, notes: "Merged by admin");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(DuplicateCandidateStatus.Merged);
        result.ReviewedById.Should().Be(5);
    }

    [Fact]
    public async Task UpdateCandidateStatusAsync_ShouldUpdateToIgnored()
    {
        // Arrange
        var candidate = await SeedCandidateAsync(sourceRecordId: 1, targetRecordId: 2);

        // Act
        var result = await _service.UpdateCandidateStatusAsync(
            candidate.Id, DuplicateCandidateStatus.Ignored, userId: 3);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(DuplicateCandidateStatus.Ignored);
    }

    [Fact]
    public async Task UpdateCandidateStatusAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var candidate = await SeedCandidateAsync(sourceRecordId: 1, targetRecordId: 2);

        // Act
        await _service.UpdateCandidateStatusAsync(
            candidate.Id, DuplicateCandidateStatus.Confirmed, userId: 1, notes: "Confirmed duplicate");

        // Assert
        var fromDb = await _dbContext.DuplicateCandidates.FindAsync(candidate.Id);
        fromDb!.Status.Should().Be(DuplicateCandidateStatus.Confirmed);
        fromDb.ReviewedById.Should().Be(1);
        fromDb.Notes.Should().Be("Confirmed duplicate");
    }

    #endregion

    #region CheckForDuplicatesAsync Tests

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnEmptyResult_WhenNoRulesExist()
    {
        // Arrange
        var fieldValues = new Dictionary<string, string?>
        {
            ["Email"] = "test@example.com",
            ["FirstName"] = "John",
            ["LastName"] = "Doe"
        };

        // Act
        var result = await _service.CheckForDuplicatesAsync("Lead", fieldValues);

        // Assert
        result.HasDuplicates.Should().BeFalse();
        result.Duplicates.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnEmptyResult_WhenInvalidEntityType()
    {
        // Arrange
        var fieldValues = new Dictionary<string, string?>
        {
            ["Email"] = "test@example.com"
        };

        // Act
        var result = await _service.CheckForDuplicatesAsync("InvalidType", fieldValues);

        // Assert
        result.HasDuplicates.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnAppliedRule_WhenRuleExists()
    {
        // Arrange
        await SeedRuleWithFieldsAsync("Email Rule", DuplicateEntityType.Lead, 80,
            ("Email", MatchType.Exact, 100));

        var fieldValues = new Dictionary<string, string?>
        {
            ["Email"] = "test@example.com",
            ["FirstName"] = "John"
        };

        // Act
        var result = await _service.CheckForDuplicatesAsync("Lead", fieldValues);

        // Assert
        result.AppliedRule.Should().NotBeNull();
        result.AppliedRule!.Name.Should().Be("Email Rule");
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldTrackDetectionTime()
    {
        // Arrange
        await SeedRuleWithFieldsAsync("Timed Rule", DuplicateEntityType.Lead, 80,
            ("Email", MatchType.Exact, 100));

        var fieldValues = new Dictionary<string, string?>
        {
            ["Email"] = "test@example.com"
        };

        // Act
        var result = await _service.CheckForDuplicatesAsync("Lead", fieldValues);

        // Assert
        result.DetectionTimeMs.Should().BeGreaterOrEqualTo(0);
    }

    #endregion

    #region Enum Value Tests

    [Fact]
    public void DuplicateEntityType_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)DuplicateEntityType.Lead).Should().Be(0);
        ((int)DuplicateEntityType.Contact).Should().Be(1);
        ((int)DuplicateEntityType.Account).Should().Be(2);
        ((int)DuplicateEntityType.LeadContact).Should().Be(3);
        ((int)DuplicateEntityType.AllPersons).Should().Be(4);
    }

    [Fact]
    public void DuplicateAction_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)DuplicateAction.Warn).Should().Be(0);
        ((int)DuplicateAction.Block).Should().Be(1);
        ((int)DuplicateAction.AutoMerge).Should().Be(2);
        ((int)DuplicateAction.QueueForReview).Should().Be(3);
        ((int)DuplicateAction.LogOnly).Should().Be(4);
    }

    [Fact]
    public void DuplicateCandidateStatus_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)DuplicateCandidateStatus.Pending).Should().Be(0);
        ((int)DuplicateCandidateStatus.Confirmed).Should().Be(1);
        ((int)DuplicateCandidateStatus.Rejected).Should().Be(2);
        ((int)DuplicateCandidateStatus.Merged).Should().Be(3);
        ((int)DuplicateCandidateStatus.Ignored).Should().Be(4);
    }

    [Fact]
    public void MatchType_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)MatchType.Exact).Should().Be(0);
        ((int)MatchType.Fuzzy).Should().Be(1);
        ((int)MatchType.Phonetic).Should().Be(2);
        ((int)MatchType.Contains).Should().Be(3);
        ((int)MatchType.StartsWith).Should().Be(4);
        ((int)MatchType.Normalized).Should().Be(5);
        ((int)MatchType.EmailDomain).Should().Be(6);
    }

    #endregion
}
