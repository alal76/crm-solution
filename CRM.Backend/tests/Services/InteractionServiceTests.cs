// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for InteractionService using InMemory database.
/// Tests CRUD, filtering, completion, logging, statistics, and history.
/// </summary>
public class InteractionServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<InteractionService>> _mockLogger;
    private readonly InteractionService _service;

    public InteractionServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"InteractionServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<InteractionService>>();
        _service = new InteractionService(_dbContext, _mockLogger.Object, Mock.Of<IDuplicateDetectionService>());
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    private Interaction CreateTestInteraction(
        string subject = "Test Interaction",
        InteractionType type = InteractionType.Note,
        InteractionOutcome outcome = InteractionOutcome.None,
        int? accountId = null,
        int? opportunityId = null,
        int? assignedToUserId = null,
        DateTime? interactionDate = null,
        int? durationMinutes = null,
        bool isCompleted = false)
    {
        return new Interaction
        {
            Subject = subject,
            Description = $"Description for {subject}",
            InteractionType = type,
            Direction = InteractionDirection.Outbound,
            Outcome = outcome,
            AccountId = accountId,
            OpportunityId = opportunityId,
            AssignedToUserId = assignedToUserId,
            InteractionDate = interactionDate ?? DateTime.UtcNow,
            DurationMinutes = durationMinutes,
            IsCompleted = isCompleted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task<Interaction> SeedInteractionAsync(
        string subject = "Seeded Interaction",
        InteractionType type = InteractionType.Note,
        InteractionOutcome outcome = InteractionOutcome.None,
        int? accountId = null,
        int? opportunityId = null,
        int? assignedToUserId = null,
        DateTime? interactionDate = null,
        int? durationMinutes = null,
        bool isCompleted = false,
        bool isDeleted = false,
        DateTime? followUpDate = null)
    {
        var interaction = CreateTestInteraction(subject, type, outcome, accountId, opportunityId,
            assignedToUserId, interactionDate, durationMinutes, isCompleted);
        interaction.IsDeleted = isDeleted;
        interaction.FollowUpDate = followUpDate;
        _dbContext.Interactions.Add(interaction);
        await _dbContext.SaveChangesAsync();
        return interaction;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDbContextIsNull()
    {
        var act = () => new InteractionService(null!, _mockLogger.Object, Mock.Of<IDuplicateDetectionService>());
        act.Should().Throw<ArgumentNullException>().WithParameterName("dbContext");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new InteractionService(_dbContext, null!, Mock.Of<IDuplicateDetectionService>());
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldCreateInteraction_WhenValid()
    {
        var interaction = CreateTestInteraction("New Interaction");
        var result = await _service.CreateAsync(interaction);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Subject.Should().Be("New Interaction");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_ShouldSetTimestamps()
    {
        var interaction = CreateTestInteraction("Timestamp Test");
        var result = await _service.CreateAsync(interaction);

        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistInDatabase()
    {
        var interaction = CreateTestInteraction("Persist Test");
        await _service.CreateAsync(interaction);

        var dbInteraction = await _dbContext.Interactions.FirstOrDefaultAsync(i => i.Subject == "Persist Test");
        dbInteraction.Should().NotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnInteraction_WhenExists()
    {
        var seeded = await SeedInteractionAsync("Find Me");
        var result = await _service.GetByIdAsync(seeded.Id);

        result.Should().NotBeNull();
        result!.Subject.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDeleted()
    {
        var seeded = await SeedInteractionAsync("Deleted", isDeleted: true);
        var result = await _service.GetByIdAsync(seeded.Id);
        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenExists()
    {
        var seeded = await SeedInteractionAsync("Original");
        var update = CreateTestInteraction("Updated");

        var result = await _service.UpdateAsync(seeded.Id, update);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProperties()
    {
        var seeded = await SeedInteractionAsync("Original", type: InteractionType.Note);
        var update = CreateTestInteraction("Updated", type: InteractionType.Phone, outcome: InteractionOutcome.Successful);

        await _service.UpdateAsync(seeded.Id, update);

        var updated = await _dbContext.Interactions.FindAsync(seeded.Id);
        updated!.Subject.Should().Be("Updated");
        updated.InteractionType.Should().Be(InteractionType.Phone);
        updated.Outcome.Should().Be(InteractionOutcome.Successful);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNotFound()
    {
        var update = CreateTestInteraction("Update");
        var result = await _service.UpdateAsync(999, update);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenDeleted()
    {
        var seeded = await SeedInteractionAsync("Deleted", isDeleted: true);
        var update = CreateTestInteraction("Update");

        var result = await _service.UpdateAsync(seeded.Id, update);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTimestamp()
    {
        var seeded = await SeedInteractionAsync("Original");
        var originalUpdatedAt = seeded.UpdatedAt;
        await Task.Delay(10);

        var update = CreateTestInteraction("Updated");
        await _service.UpdateAsync(seeded.Id, update);

        var updated = await _dbContext.Interactions.FindAsync(seeded.Id);
        updated!.UpdatedAt.Should().NotBeNull();
        updated.UpdatedAt!.Value.Should().BeAfter(originalUpdatedAt!.Value);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenExists()
    {
        var seeded = await SeedInteractionAsync("Delete Me");
        var result = await _service.DeleteAsync(seeded.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var seeded = await SeedInteractionAsync("Soft Delete");
        await _service.DeleteAsync(seeded.Id);

        var deleted = await _dbContext.Interactions.FindAsync(seeded.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.DeleteAsync(999);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenAlreadyDeleted()
    {
        var seeded = await SeedInteractionAsync("Already Deleted", isDeleted: true);
        var result = await _service.DeleteAsync(seeded.Id);
        result.Should().BeFalse();
    }

    #endregion

    #region CompleteAsync Tests

    [Fact]
    public async Task CompleteAsync_ShouldReturnInteraction_WhenExists()
    {
        var seeded = await SeedInteractionAsync("Complete Me");
        var result = await _service.CompleteAsync(seeded.Id);

        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeTrue();
        result.CompletedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.CompleteAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_ShouldApplyOutcome_WhenRequestProvided()
    {
        var seeded = await SeedInteractionAsync("Complete With Outcome");
        var request = new InteractionCompletionRequest
        {
            Outcome = InteractionOutcome.Successful,
            Notes = "Great call"
        };

        var result = await _service.CompleteAsync(seeded.Id, request);

        result!.Outcome.Should().Be(InteractionOutcome.Successful);
        result.MeetingNotes.Should().Contain("Great call");
    }

    [Fact]
    public async Task CompleteAsync_ShouldAppendNotes_WhenExistingNotesPresent()
    {
        var seeded = await SeedInteractionAsync("Notes Append");
        var dbItem = await _dbContext.Interactions.FindAsync(seeded.Id);
        dbItem!.MeetingNotes = "Existing notes";
        await _dbContext.SaveChangesAsync();

        var request = new InteractionCompletionRequest { Notes = "New notes" };
        var result = await _service.CompleteAsync(seeded.Id, request);

        result!.MeetingNotes.Should().Contain("Existing notes");
        result.MeetingNotes.Should().Contain("New notes");
    }

    [Fact]
    public async Task CompleteAsync_ShouldReturnNull_WhenDeleted()
    {
        var seeded = await SeedInteractionAsync("Deleted", isDeleted: true);
        var result = await _service.CompleteAsync(seeded.Id);
        result.Should().BeNull();
    }

    #endregion

    #region GetInteractionsAsync Filtering Tests

    [Fact]
    public async Task GetInteractionsAsync_ShouldReturnAllNonDeleted_WhenNoFilters()
    {
        await SeedInteractionAsync("I1");
        await SeedInteractionAsync("I2");
        await SeedInteractionAsync("Deleted", isDeleted: true);

        var result = await _service.GetInteractionsAsync();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInteractionsAsync_ShouldFilterByAccountId()
    {
        await SeedInteractionAsync("Acct 1", accountId: 1);
        await SeedInteractionAsync("Acct 2", accountId: 2);

        var result = await _service.GetInteractionsAsync(accountId: 1);
        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Acct 1");
    }

    [Fact]
    public async Task GetInteractionsAsync_ShouldFilterByOpportunityId()
    {
        await SeedInteractionAsync("Opp 1", opportunityId: 10);
        await SeedInteractionAsync("Opp 2", opportunityId: 20);

        var result = await _service.GetInteractionsAsync(opportunityId: 10);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetInteractionsAsync_ShouldFilterByAssignedToUserId()
    {
        await SeedInteractionAsync("User 1", assignedToUserId: 100);
        await SeedInteractionAsync("User 2", assignedToUserId: 200);

        var result = await _service.GetInteractionsAsync(assignedToUserId: 100);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetInteractionsAsync_ShouldFilterByInteractionType()
    {
        await SeedInteractionAsync("Phone Call", type: InteractionType.Phone);
        await SeedInteractionAsync("Email", type: InteractionType.Email);
        await SeedInteractionAsync("Meeting", type: InteractionType.Meeting);

        var result = await _service.GetInteractionsAsync(interactionType: InteractionType.Phone);
        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Phone Call");
    }

    [Fact]
    public async Task GetInteractionsAsync_ShouldFilterByOutcome()
    {
        await SeedInteractionAsync("Successful", outcome: InteractionOutcome.Successful);
        await SeedInteractionAsync("No Response", outcome: InteractionOutcome.NoResponse);

        var result = await _service.GetInteractionsAsync(outcome: InteractionOutcome.Successful);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetInteractionsAsync_ShouldFilterByDateRange()
    {
        var jan1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var feb1 = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var mar1 = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        await SeedInteractionAsync("Jan", interactionDate: jan1);
        await SeedInteractionAsync("Feb", interactionDate: feb1);
        await SeedInteractionAsync("Mar", interactionDate: mar1);

        var result = await _service.GetInteractionsAsync(fromDate: jan1, toDate: feb1);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInteractionsAsync_ShouldCombineFilters()
    {
        await SeedInteractionAsync("Match", type: InteractionType.Phone, accountId: 1);
        await SeedInteractionAsync("Wrong Type", type: InteractionType.Email, accountId: 1);
        await SeedInteractionAsync("Wrong Account", type: InteractionType.Phone, accountId: 2);

        var result = await _service.GetInteractionsAsync(accountId: 1, interactionType: InteractionType.Phone);
        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Match");
    }

    #endregion

    #region LogAsync Tests

    [Fact]
    public async Task LogAsync_ShouldCreateCompletedInteraction()
    {
        var request = new InteractionLogRequest
        {
            AccountId = 1,
            InteractionType = InteractionType.Phone,
            Direction = InteractionDirection.Outbound,
            Subject = "Logged Call",
            Description = "Quick follow-up call",
            DurationMinutes = 15,
            Outcome = InteractionOutcome.Successful,
            UserId = 10
        };

        var result = await _service.LogAsync(request);

        result.Should().NotBeNull();
        result.IsCompleted.Should().BeTrue();
        result.CompletedDate.Should().NotBeNull();
        result.Subject.Should().Be("Logged Call");
        result.DurationMinutes.Should().Be(15);
        result.AccountId.Should().Be(1);
        result.AssignedToUserId.Should().Be(10);
    }

    [Fact]
    public async Task LogAsync_ShouldSetDefaultOutcome_WhenNull()
    {
        var request = new InteractionLogRequest
        {
            AccountId = 1,
            InteractionType = InteractionType.Note,
            Direction = InteractionDirection.Internal,
            Subject = "Note",
            Outcome = null
        };

        var result = await _service.LogAsync(request);
        result.Outcome.Should().Be(InteractionOutcome.None);
    }

    #endregion

    #region GetStatisticsAsync Tests

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectCounts()
    {
        await SeedInteractionAsync("Phone 1", type: InteractionType.Phone);
        await SeedInteractionAsync("Phone 2", type: InteractionType.Phone);
        await SeedInteractionAsync("Email 1", type: InteractionType.Email);
        await SeedInteractionAsync("Meeting 1", type: InteractionType.Meeting);
        await SeedInteractionAsync("Deleted", type: InteractionType.Phone, isDeleted: true);

        var stats = await _service.GetStatisticsAsync();

        stats.TotalInteractions.Should().Be(4);
        stats.Calls.Should().Be(2);
        stats.Emails.Should().Be(1);
        stats.Meetings.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldCountSuccessful()
    {
        await SeedInteractionAsync("Success", outcome: InteractionOutcome.Successful);
        await SeedInteractionAsync("Fail", outcome: InteractionOutcome.Unsuccessful);

        var stats = await _service.GetStatisticsAsync();
        stats.Successful.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldCountFollowUpRequired()
    {
        await SeedInteractionAsync("Follow Up", followUpDate: DateTime.UtcNow.AddDays(3), isCompleted: false);
        await SeedInteractionAsync("No Follow Up");

        var stats = await _service.GetStatisticsAsync();
        stats.FollowUpRequired.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldCalculateAverageDuration()
    {
        await SeedInteractionAsync("Short", durationMinutes: 10);
        await SeedInteractionAsync("Long", durationMinutes: 30);
        await SeedInteractionAsync("No Duration"); // null duration

        var stats = await _service.GetStatisticsAsync();
        stats.AverageDurationMinutes.Should().BeApproximately(20, 0.1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldFilterByAccountId()
    {
        await SeedInteractionAsync("Acct 1", accountId: 1);
        await SeedInteractionAsync("Acct 2", accountId: 2);

        var stats = await _service.GetStatisticsAsync(accountId: 1);
        stats.TotalInteractions.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnZeros_WhenNoInteractions()
    {
        var stats = await _service.GetStatisticsAsync();

        stats.TotalInteractions.Should().Be(0);
        stats.Calls.Should().Be(0);
        stats.Emails.Should().Be(0);
        stats.Meetings.Should().Be(0);
    }

    #endregion

    #region GetAccountHistoryAsync Tests

    [Fact]
    public async Task GetAccountHistoryAsync_ShouldReturnInteractionsForAccount()
    {
        await SeedInteractionAsync("Acct 1 I1", accountId: 1);
        await SeedInteractionAsync("Acct 1 I2", accountId: 1);
        await SeedInteractionAsync("Acct 2 I1", accountId: 2);

        var result = await _service.GetAccountHistoryAsync(1);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAccountHistoryAsync_ShouldRespectLimit()
    {
        for (int i = 0; i < 10; i++)
        {
            await SeedInteractionAsync($"I{i}", accountId: 1);
        }

        var result = await _service.GetAccountHistoryAsync(1, limit: 5);
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAccountHistoryAsync_ShouldExcludeDeleted()
    {
        await SeedInteractionAsync("Active", accountId: 1);
        await SeedInteractionAsync("Deleted", accountId: 1, isDeleted: true);

        var result = await _service.GetAccountHistoryAsync(1);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAccountHistoryAsync_ShouldReturnEmpty_WhenNoInteractions()
    {
        var result = await _service.GetAccountHistoryAsync(999);
        result.Should().BeEmpty();
    }

    #endregion
}
