// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="LeadHistoryContinuityService"/> (REM-LEAD-HISTORY-CONTINUITY)
/// using an EF Core InMemory database. This tool is never run against real data in these
/// tests — every DbContext instance is a fresh, isolated InMemory database per test.
/// </summary>
public class LeadHistoryContinuityServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<LeadHistoryContinuityService>> _mockLogger;
    private readonly ILeadHistoryContinuityService _service;

    public LeadHistoryContinuityServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"LeadHistoryContinuityServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null!);
        _mockLogger = new Mock<ILogger<LeadHistoryContinuityService>>();
        _service = new LeadHistoryContinuityService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    private async Task<Contact> SeedContactAsync(string firstName = "Jane", string lastName = "Doe")
    {
        var contact = new Contact
        {
            ContactType = ContactType.Lead,
            FirstName = firstName,
            LastName = lastName,
            EmailPrimary = $"{firstName}.{lastName}@example.com".ToLowerInvariant(),
            DateAdded = DateTime.UtcNow,
        };
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();
        return contact;
    }

    private async Task<Lead> SeedMigratedLeadAsync(int? contactId, string firstName = "Jane", string lastName = "Doe")
    {
        var lead = new Lead
        {
            ContactId = contactId,
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName}.{lastName}@example.com".ToLowerInvariant(),
            Status = LeadLifecycleStatus.New,
            Source = LeadSource.Web,
        };
        _dbContext.Leads.Add(lead);
        await _dbContext.SaveChangesAsync();
        return lead;
    }

    private async Task<Activity> SeedActivityAsync(string entityType, int? entityId, string title = "Call made")
    {
        var activity = new Activity
        {
            ActivityType = ActivityType.CallMade,
            Title = title,
            EntityType = entityType,
            EntityId = entityId,
            ActivityDate = DateTime.UtcNow,
        };
        _dbContext.Activities.Add(activity);
        await _dbContext.SaveChangesAsync();
        return activity;
    }

    private async Task<RecordComment> SeedCommentAsync(string entityType, int entityId, string content = "A comment")
    {
        var comment = new RecordComment
        {
            EntityType = entityType,
            EntityId = entityId,
            Content = content,
            AuthorId = 1,
        };
        _dbContext.RecordComments.Add(comment);
        await _dbContext.SaveChangesAsync();
        return comment;
    }

    #endregion

    #region Re-parenting

    [Fact]
    public async Task RunAsync_ShouldReparentActivitiesAndComments_WhenLeadHasMatchingOldContactHistory()
    {
        // Arrange
        var contact = await SeedContactAsync();
        var lead = await SeedMigratedLeadAsync(contact.Id);
        var activity = await SeedActivityAsync("Contact", contact.Id);
        var comment = await SeedCommentAsync("Contact", contact.Id);

        // Act
        var result = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        result.DryRun.Should().BeFalse();
        result.TotalMigratedLeadsFound.Should().Be(1);
        result.LeadsProcessedCount.Should().Be(1);
        result.LeadsSkippedNoHistoryCount.Should().Be(0);
        result.ActivitiesReparentedCount.Should().Be(1);
        result.CommentsReparentedCount.Should().Be(1);

        var reloadedActivity = await _dbContext.Activities.SingleAsync(a => a.Id == activity.Id);
        reloadedActivity.EntityType.Should().Be("Lead");
        reloadedActivity.EntityId.Should().Be(lead.Id);

        var reloadedComment = await _dbContext.RecordComments.SingleAsync(c => c.Id == comment.Id);
        reloadedComment.EntityType.Should().Be("Lead");
        reloadedComment.EntityId.Should().Be(lead.Id);
    }

    #endregion

    #region Idempotency

    [Fact]
    public async Task RunAsync_ShouldNotDoubleCountOrCorrupt_WhenRunTwice()
    {
        // Arrange
        var contact = await SeedContactAsync();
        var lead = await SeedMigratedLeadAsync(contact.Id);
        await SeedActivityAsync("Contact", contact.Id);
        await SeedCommentAsync("Contact", contact.Id);

        // Act
        var firstRun = await _service.RunAsync(dryRun: false, CancellationToken.None);
        var secondRun = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        firstRun.ActivitiesReparentedCount.Should().Be(1);
        firstRun.CommentsReparentedCount.Should().Be(1);

        // Second run finds nothing left under the old Contact reference - the UPDATE
        // from the first run makes the old-Contact query naturally return no rows.
        secondRun.ActivitiesReparentedCount.Should().Be(0);
        secondRun.CommentsReparentedCount.Should().Be(0);
        secondRun.LeadsProcessedCount.Should().Be(0);
        secondRun.LeadsSkippedNoHistoryCount.Should().Be(1);

        (await _dbContext.Activities.CountAsync()).Should().Be(1);
        (await _dbContext.RecordComments.CountAsync()).Should().Be(1);

        var activity = await _dbContext.Activities.SingleAsync();
        activity.EntityType.Should().Be("Lead");
        activity.EntityId.Should().Be(lead.Id);

        var comment = await _dbContext.RecordComments.SingleAsync();
        comment.EntityType.Should().Be("Lead");
        comment.EntityId.Should().Be(lead.Id);
    }

    #endregion

    #region Dry run

    [Fact]
    public async Task RunAsync_ShouldMakeNoDatabaseWrites_WhenDryRunIsTrue()
    {
        // Arrange
        var contact = await SeedContactAsync();
        await SeedMigratedLeadAsync(contact.Id);
        await SeedActivityAsync("Contact", contact.Id);
        await SeedCommentAsync("Contact", contact.Id);

        // Act
        var result = await _service.RunAsync(dryRun: true, CancellationToken.None);

        // Assert
        result.DryRun.Should().BeTrue();
        result.ActivitiesReparentedCount.Should().Be(1);
        result.CommentsReparentedCount.Should().Be(1);
        result.LeadsProcessedCount.Should().Be(1);

        var activity = await _dbContext.Activities.SingleAsync();
        activity.EntityType.Should().Be("Contact");
        activity.EntityId.Should().Be(contact.Id);

        var comment = await _dbContext.RecordComments.SingleAsync();
        comment.EntityType.Should().Be("Contact");
        comment.EntityId.Should().Be(contact.Id);

        // Running again after a dry run should report identical results (nothing changed).
        var secondDryRun = await _service.RunAsync(dryRun: true, CancellationToken.None);
        secondDryRun.ActivitiesReparentedCount.Should().Be(1);
        secondDryRun.CommentsReparentedCount.Should().Be(1);
    }

    #endregion

    #region Leads without ContactId are skipped

    [Fact]
    public async Task RunAsync_ShouldSkipLead_WhenContactIdIsNull()
    {
        // Arrange: a Lead created directly (not via backfill) has no ContactId.
        var lead = await SeedMigratedLeadAsync(contactId: null);
        // Even if an Activity happens to reference this Lead's Id under EntityType "Contact",
        // it must not be touched because the Lead was never migrated from a Contact.
        await SeedActivityAsync("Contact", lead.Id);

        // Act
        var result = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        result.TotalMigratedLeadsFound.Should().Be(0);
        result.LeadsProcessedCount.Should().Be(0);
        result.LeadsSkippedNoHistoryCount.Should().Be(0);
        result.ActivitiesReparentedCount.Should().Be(0);
        result.CommentsReparentedCount.Should().Be(0);

        var activity = await _dbContext.Activities.SingleAsync();
        activity.EntityType.Should().Be("Contact");
        activity.EntityId.Should().Be(lead.Id);
    }

    #endregion

    #region Non-matching history is left untouched

    [Fact]
    public async Task RunAsync_ShouldLeaveNonMatchingActivitiesAndComments_Untouched()
    {
        // Arrange
        var contact = await SeedContactAsync();
        var lead = await SeedMigratedLeadAsync(contact.Id);

        // Matching history - should be moved.
        await SeedActivityAsync("Contact", contact.Id);

        // Non-matching: different EntityType.
        var otherTypeActivity = await SeedActivityAsync("Account", contact.Id);
        // Non-matching: EntityType Contact but a different EntityId (unrelated Contact).
        var otherContactActivity = await SeedActivityAsync("Contact", contact.Id + 999);
        // Non-matching: EntityId is null.
        var nullEntityIdActivity = await SeedActivityAsync("Contact", null);
        // Non-matching comment: different EntityType.
        var otherTypeComment = await SeedCommentAsync("Opportunity", contact.Id);

        // Act
        var result = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        result.ActivitiesReparentedCount.Should().Be(1);
        result.CommentsReparentedCount.Should().Be(0);
        result.LeadsProcessedCount.Should().Be(1);

        var reloadedOtherType = await _dbContext.Activities.SingleAsync(a => a.Id == otherTypeActivity.Id);
        reloadedOtherType.EntityType.Should().Be("Account");
        reloadedOtherType.EntityId.Should().Be(contact.Id);

        var reloadedOtherContact = await _dbContext.Activities.SingleAsync(a => a.Id == otherContactActivity.Id);
        reloadedOtherContact.EntityType.Should().Be("Contact");
        reloadedOtherContact.EntityId.Should().Be(contact.Id + 999);

        var reloadedNullEntityId = await _dbContext.Activities.SingleAsync(a => a.Id == nullEntityIdActivity.Id);
        reloadedNullEntityId.EntityType.Should().Be("Contact");
        reloadedNullEntityId.EntityId.Should().BeNull();

        var reloadedOtherTypeComment = await _dbContext.RecordComments.SingleAsync(c => c.Id == otherTypeComment.Id);
        reloadedOtherTypeComment.EntityType.Should().Be("Opportunity");
        reloadedOtherTypeComment.EntityId.Should().Be(contact.Id);
    }

    #endregion

    #region No history to move

    [Fact]
    public async Task RunAsync_ShouldReportLeadSkippedNoHistory_WhenMigratedLeadHasNoOldContactHistory()
    {
        // Arrange
        var contact = await SeedContactAsync();
        await SeedMigratedLeadAsync(contact.Id);

        // Act
        var result = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        result.TotalMigratedLeadsFound.Should().Be(1);
        result.LeadsProcessedCount.Should().Be(0);
        result.LeadsSkippedNoHistoryCount.Should().Be(1);
        result.ActivitiesReparentedCount.Should().Be(0);
        result.CommentsReparentedCount.Should().Be(0);
    }

    #endregion

    #region Constructor guards

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        var act = () => new LeadHistoryContinuityService(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new LeadHistoryContinuityService(_dbContext, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    #endregion
}
