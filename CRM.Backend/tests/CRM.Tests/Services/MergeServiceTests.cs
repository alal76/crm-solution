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
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for MergeService (TCOV-007).
/// </summary>
public class MergeServiceTests : ServiceTestFixtureBase<MergeService>
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly MergeService _service;

    public MergeServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        SetupDatabase();
        _service = new MergeService(_mockDbContext.Object, MockLogger.Object);
    }

    private void SetupDatabase()
    {
        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    /// <summary>
    /// MergeRecordsAsync/UnmergeRecordsAsync/PreviewMergeAsync exercise real transactions
    /// (_context.Database.BeginTransactionAsync) and reflection-based field copy/restore that
    /// the Mock&lt;ICrmDbContext&gt; + MockDbSetFactory pattern used by the existing tests above
    /// cannot represent (DatabaseFacade.BeginTransactionAsync has no in-memory mock story here).
    /// For these methods we use a real EF Core InMemory-backed CrmDbContext as the ICrmDbContext
    /// implementation instead. The InMemory provider's TransactionIgnoredWarning is configured
    /// to log-not-throw since InMemory has no real transaction support (it still executes writes
    /// immediately, which is fine for these unit tests — MergeService's atomicity guarantee
    /// itself is not what's under test here).
    /// </summary>
    private static CrmDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"MergeServiceTests_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new CrmDbContext(options, new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMergeHistoryAsync_ShouldReturnEmpty_WhenNoMergeGroupsForRecord()
    {
        _mockDbContext.Setup(c => c.Set<DuplicateMergeGroup>())
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DuplicateMergeGroup>()).Object);

        var result = await _service.GetMergeHistoryAsync(42, "Lead");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMergeHistoryAsync_ShouldReturnEmpty_WhenNoMergeGroupsExist()
    {
        _mockDbContext.Setup(c => c.Set<DuplicateMergeGroup>())
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DuplicateMergeGroup>()).Object);

        var result = await _service.GetMergeHistoryAsync(1, "Lead");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMergedRecordsAsync_ShouldReturnEmpty_WhenNoMergedRecords()
    {
        _mockDbContext.Setup(c => c.Set<DuplicateMergeGroup>())
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DuplicateMergeGroup>()).Object);
        _mockDbContext.Setup(c => c.Set<DuplicateMergeGroupMember>())
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DuplicateMergeGroupMember>()).Object);

        var result = await _service.GetMergedRecordsAsync(1, "Lead");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMergeGroupAsync_ShouldReturnNull_WhenGroupNotFound()
    {
        _mockDbContext.Setup(c => c.Set<DuplicateMergeGroup>())
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DuplicateMergeGroup>()).Object);

        var result = await _service.GetMergeGroupAsync(999);

        result.Should().BeNull();
    }

    #region MergeRecordsAsync

    [Fact]
    public async Task MergeRecordsAsync_ShouldMergeSuccessfully_WhenValidLeadsProvided()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", CompanyName = "Acme" };
        var dup1 = new Lead { Id = 2, FirstName = "Jon", LastName = "Doe", Email = "jon@example.com", CompanyName = "Acme Corp" };
        var dup2 = new Lead { Id = 3, FirstName = "J", LastName = "Doe", Email = "j@example.com" };
        db.Leads.AddRange(master, dup1, dup2);
        await db.SaveChangesAsync();

        var request = new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2, 3 },
            UserId = 99
        };

        var result = await service.MergeRecordsAsync(request);

        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.RecordsMerged.Should().Be(2);
        result.MergeGroupId.Should().BeGreaterThan(0);

        var masterAfter = await db.Leads.FindAsync(1);
        masterAfter!.IsDeleted.Should().BeFalse();
        masterAfter.IsMergedDuplicate.Should().BeFalse();

        var dup1After = await db.Leads.FindAsync(2);
        dup1After!.IsDeleted.Should().BeTrue();
        dup1After.IsMergedDuplicate.Should().BeTrue();
        dup1After.MergedIntoId.Should().Be(1);
        dup1After.MergeGroupId.Should().Be(result.MergeGroupId);

        var dup2After = await db.Leads.FindAsync(3);
        dup2After!.IsDeleted.Should().BeTrue();
        dup2After.IsMergedDuplicate.Should().BeTrue();

        var group = await db.DuplicateMergeGroups
            .Include(g => g.Members)
            .FirstAsync(g => g.Id == result.MergeGroupId);
        group.Members.Should().HaveCount(3);
        group.Members.Count(m => m.IsMaster).Should().Be(1);
        group.Status.Should().Be(MergeGroupStatus.Active);
    }

    [Fact]
    public async Task MergeRecordsAsync_ShouldApplyFieldOverride_WhenFieldSourceOverrideSpecified()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@old.com", Phone = null };
        var dup = new Lead { Id = 2, FirstName = "John", LastName = "Doe", Email = "john@new.com", Phone = "555-2222" };
        db.Leads.AddRange(master, dup);
        await db.SaveChangesAsync();

        var request = new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 },
            FieldSourceOverrides = new Dictionary<string, int> { ["Email"] = 2, ["Phone"] = 2 },
            UserId = 1
        };

        var result = await service.MergeRecordsAsync(request);

        result.Success.Should().BeTrue();

        var masterAfter = await db.Leads.FindAsync(1);
        masterAfter!.Email.Should().Be("john@new.com");
        masterAfter.Phone.Should().Be("555-2222");
    }

    /// <summary>
    /// Regression test for a fixed bug in MergeService.MergeRecordsAsync: unlike
    /// UnmergeRecordsAsync (which explicitly calls _context.SaveChangesAsync() right before
    /// transaction.CommitAsync()), MergeRecordsAsync did not. The only SaveChangesAsync calls in
    /// the merge path were (a) right after the DuplicateMergeGroup is created, and (b) the one
    /// buried inside RelinkRelatedRecordsAsync — which runs *before* ProcessMergedRecordAsync
    /// creates the DuplicateMergeGroupMember audit row and *before* MarkRecordAsMergedAsync sets
    /// the merged record's soft-delete flags. Neither of those mutations was followed by another
    /// SaveChangesAsync call, so for a single-record merge they were never flushed to the store
    /// before the transaction committed. MergeRecordsAsync now calls SaveChangesAsync right
    /// before the transaction commits, mirroring UnmergeRecordsAsync.
    ///
    /// Querying with AsNoTracking() (which bypasses the identity map and reflects only what is
    /// actually in the backing store, rather than locally tracked-but-unsaved CLR objects) proves
    /// the mutations were actually flushed, not just tracked in memory.
    /// </summary>
    [Fact]
    public async Task MergeRecordsAsync_ShouldPersistMemberRowAndSoftDeleteFlag_WhenSingleRecordMerged()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var dup = new Lead { Id = 2, FirstName = "Jon", LastName = "Doe", Email = "jon@example.com" };
        db.Leads.AddRange(master, dup);
        await db.SaveChangesAsync();

        var result = await service.MergeRecordsAsync(new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 },
            UserId = 1
        });

        result.Success.Should().BeTrue();

        var persistedMember = await db.DuplicateMergeGroupMembers.AsNoTracking()
            .Where(m => m.RecordId == 2 && !m.IsMaster)
            .ToListAsync();
        persistedMember.Should().ContainSingle(
            "MergeRecordsAsync must flush the audit row for the merged record to the store before committing");

        // IgnoreQueryFilters() is required here: Lead has a global soft-delete query filter
        // (!IsDeleted), and this record's persisted IsDeleted=true is exactly what we're
        // verifying, so the default filter would hide it and make the assertion meaningless.
        var persistedDup = await db.Leads.IgnoreQueryFilters().AsNoTracking().FirstAsync(l => l.Id == 2);
        persistedDup.IsDeleted.Should().BeTrue(
            "the merged record's soft-delete flag must actually be persisted, not just tracked in memory");
        persistedDup.IsMergedDuplicate.Should().BeTrue();
    }

    [Fact]
    public async Task MergeRecordsAsync_ShouldRelinkRelatedRecords_WhenRelinkRelatedRecordsIsTrue()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var dup = new Lead { Id = 2, FirstName = "Jon", LastName = "Doe", Email = "jon@example.com" };
        db.Leads.AddRange(master, dup);
        db.Notes.Add(new Note { Id = 1, Title = "t", Content = "c", EntityType = "Lead", EntityId = 2 });
        db.Opportunities.Add(new Opportunity { Id = 1, Name = "Opp1", Currency = "USD", AccountId = 1, LeadId = 2 });
        await db.SaveChangesAsync();

        // Clear the change tracker so the service's own queries re-fetch fresh, untracked
        // instances instead of reusing these already-tracked ones. Without this, EF Core's
        // automatic relationship fixup links Opportunity.Lead <-> Lead.Opportunities because
        // both are tracked in the same context, and GetRecordSnapshotAsync's plain
        // JsonSerializer.Serialize(record) call on the Lead then walks into a real cycle
        // (Lead -> Opportunities -> Lead -> ...), throwing and failing the whole merge. That's
        // a separate, real fragility in MergeService's snapshot serialization (it has no cycle
        // handling), not something this test is meant to exercise.
        db.ChangeTracker.Clear();

        var request = new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 },
            RelinkRelatedRecords = true,
            UserId = 1
        };

        var result = await service.MergeRecordsAsync(request);

        result.Success.Should().BeTrue(result.ErrorMessage);
        result.RelatedRecordsRelinked.Should().Be(2);

        (await db.Notes.FindAsync(1))!.EntityId.Should().Be(1);
        (await db.Opportunities.FindAsync(1))!.LeadId.Should().Be(1);
    }

    [Fact]
    public async Task MergeRecordsAsync_ShouldFail_WhenEntityTypeIsInvalid()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var request = new MergeRequest
        {
            EntityType = "NotARealType",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 }
        };

        var result = await service.MergeRecordsAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid entity type");
    }

    [Fact]
    public async Task MergeRecordsAsync_ShouldFail_WhenRecordAlreadyMerged()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var dup = new Lead
        {
            Id = 2,
            FirstName = "Jon",
            LastName = "Doe",
            Email = "jon@example.com",
            IsMergedDuplicate = true
        };
        db.Leads.AddRange(master, dup);
        await db.SaveChangesAsync();

        var request = new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 }
        };

        var result = await service.MergeRecordsAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already merged");
    }

    /// <summary>
    /// Regression test for a fixed bug in MergeService.ValidateMergeRequestAsync: it did not
    /// check whether MasterRecordId also appears in RecordsToMerge. When it did, the master
    /// record passed the "record to merge" existence check (it exists and is not yet a merged
    /// duplicate), so ProcessMergedRecordAsync ran against the master's own ID and
    /// MarkRecordAsMergedAsync soft-deleted the master into itself (MergedIntoId == its own Id).
    /// ValidateMergeRequestAsync now rejects such requests before any mutation happens.
    /// </summary>
    [Fact]
    public async Task MergeRecordsAsync_ShouldFail_WhenRecordsToMergeIncludesMasterId()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        db.Leads.Add(master);
        await db.SaveChangesAsync();

        var request = new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 1 },
            UserId = 1
        };

        var result = await service.MergeRecordsAsync(request);

        result.Success.Should().BeFalse("self-merge must be rejected before any mutation happens");
        result.ErrorMessage.Should().Contain("cannot also appear in the list of records to merge");

        var masterAfter = await db.Leads.FindAsync(1);
        masterAfter!.IsDeleted.Should().BeFalse("the master record must be untouched when the request is rejected");
        masterAfter.IsMergedDuplicate.Should().BeFalse();
        masterAfter.MergedIntoId.Should().BeNull();
    }

    #endregion

    #region PreviewMergeAsync

    [Fact]
    public async Task PreviewMergeAsync_ShouldReturnFieldPreviews_WhenValidLeadsProvided()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@a.com", CompanyName = "Acme" };
        var dup = new Lead { Id = 2, FirstName = "Jon", LastName = "Doe", Email = "jon@b.com", CompanyName = "Acme Inc" };
        db.Leads.AddRange(master, dup);
        await db.SaveChangesAsync();

        var request = new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 }
        };

        var preview = await service.PreviewMergeAsync(request);

        preview.Warnings.Should().BeEmpty();
        preview.FieldPreviews.Should().ContainKey("FirstName");
        preview.FieldPreviews["FirstName"].AllValues[1].Should().Be("John");
        preview.FieldPreviews["FirstName"].AllValues[2].Should().Be("Jon");
        preview.FieldPreviews["FirstName"].FinalValue.Should().Be("John", "master's value wins by default");
        preview.PreviewRecord["FirstName"].Should().Be("John");
    }

    [Fact]
    public async Task PreviewMergeAsync_ShouldUseOverrideSource_WhenFieldSourceOverrideProvided()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@a.com" };
        var dup = new Lead { Id = 2, FirstName = "Jon", LastName = "Doe", Email = "jon@b.com" };
        db.Leads.AddRange(master, dup);
        await db.SaveChangesAsync();

        var request = new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 },
            FieldSourceOverrides = new Dictionary<string, int> { ["FirstName"] = 2 }
        };

        var preview = await service.PreviewMergeAsync(request);

        preview.FieldPreviews["FirstName"].SourceRecordId.Should().Be(2);
        preview.FieldPreviews["FirstName"].FinalValue.Should().Be("Jon");
    }

    [Fact]
    public async Task PreviewMergeAsync_ShouldNotPersistChanges_WhenCalled()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@a.com" };
        var dup = new Lead { Id = 2, FirstName = "Jon", LastName = "Doe", Email = "jon@b.com" };
        db.Leads.AddRange(master, dup);
        await db.SaveChangesAsync();

        var groupsBefore = await db.DuplicateMergeGroups.CountAsync();

        var request = new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 }
        };

        var preview = await service.PreviewMergeAsync(request);

        preview.Should().NotBeNull();
        (await db.DuplicateMergeGroups.CountAsync()).Should().Be(groupsBefore);

        var dupAfter = await db.Leads.FindAsync(2);
        dupAfter!.IsMergedDuplicate.Should().BeFalse();
        dupAfter.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task PreviewMergeAsync_ShouldReturnWarning_WhenEntityTypeIsInvalid()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var request = new MergeRequest
        {
            EntityType = "NotARealType",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 }
        };

        var preview = await service.PreviewMergeAsync(request);

        preview.Warnings.Should().Contain(w => w.Contains("Invalid entity type"));
    }

    #endregion

    #region UnmergeRecordsAsync

    [Fact]
    public async Task UnmergeRecordsAsync_ShouldRestoreRecord_WhenValidMergeGroupProvided()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var dup = new Lead { Id = 2, FirstName = "Jon", LastName = "Doe", Email = "jon@example.com" };
        db.Leads.AddRange(master, dup);
        await db.SaveChangesAsync();

        var mergeResult = await service.MergeRecordsAsync(new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 },
            UserId = 1
        });
        mergeResult.Success.Should().BeTrue();

        var unmergeResult = await service.UnmergeRecordsAsync(new UnmergeRequest
        {
            MergeGroupId = mergeResult.MergeGroupId,
            UserId = 1
        });

        unmergeResult.Success.Should().BeTrue();
        unmergeResult.RestoredRecordIds.Should().ContainSingle().Which.Should().Be(2);

        var dupAfter = await db.Leads.FindAsync(2);
        dupAfter!.IsDeleted.Should().BeFalse();
        dupAfter.IsMergedDuplicate.Should().BeFalse();
        dupAfter.MergedIntoId.Should().BeNull();
        dupAfter.FirstName.Should().Be("Jon");
        dupAfter.Email.Should().Be("jon@example.com");

        var group = await db.DuplicateMergeGroups.FirstAsync(g => g.Id == mergeResult.MergeGroupId);
        group.Status.Should().Be(MergeGroupStatus.Unmerged);
    }

    [Fact]
    public async Task UnmergeRecordsAsync_ShouldFail_WhenMergeGroupNotFound()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var result = await service.UnmergeRecordsAsync(new UnmergeRequest { MergeGroupId = 9999, UserId = 1 });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task UnmergeRecordsAsync_ShouldFail_WhenGroupAlreadyUnmerged()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        db.DuplicateMergeGroups.Add(new DuplicateMergeGroup
        {
            Id = 1,
            EntityType = DuplicateEntityType.Lead,
            MasterRecordId = 1,
            Status = MergeGroupStatus.Unmerged
        });
        await db.SaveChangesAsync();

        var result = await service.UnmergeRecordsAsync(new UnmergeRequest { MergeGroupId = 1, UserId = 1 });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already been unmerged");
    }

    [Fact]
    public async Task UnmergeRecordsAsync_ShouldSetPartialUnmergeStatus_WhenOnlySpecificRecordRestored()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var dup1 = new Lead { Id = 2, FirstName = "Jon", LastName = "Doe", Email = "jon@example.com" };
        var dup2 = new Lead { Id = 3, FirstName = "J", LastName = "Doe", Email = "j@example.com" };
        db.Leads.AddRange(master, dup1, dup2);
        await db.SaveChangesAsync();

        var mergeResult = await service.MergeRecordsAsync(new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2, 3 },
            UserId = 1
        });
        mergeResult.Success.Should().BeTrue();

        var unmergeResult = await service.UnmergeRecordsAsync(new UnmergeRequest
        {
            MergeGroupId = mergeResult.MergeGroupId,
            SpecificRecordsToRestore = new List<int> { 2 },
            UserId = 1
        });

        unmergeResult.Success.Should().BeTrue();
        unmergeResult.RestoredRecordIds.Should().ContainSingle().Which.Should().Be(2);

        var dup2After = await db.Leads.FindAsync(3);
        dup2After!.IsDeleted.Should().BeTrue("record 3 was not part of the restore request");

        var group = await db.DuplicateMergeGroups.FirstAsync(g => g.Id == mergeResult.MergeGroupId);
        group.Status.Should().Be(MergeGroupStatus.PartialUnmerge);
    }

    [Fact]
    public async Task UnmergeRecordsAsync_ShouldReverseRelinkedRecords_WhenRestoreRelatedRecordsIsTrue()
    {
        await using var db = CreateInMemoryContext();
        var service = new MergeService(db, MockLogger.Object);

        var master = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var dup = new Lead { Id = 2, FirstName = "Jon", LastName = "Doe", Email = "jon@example.com" };
        db.Leads.AddRange(master, dup);
        db.Notes.Add(new Note { Id = 1, Title = "t", Content = "c", EntityType = "Lead", EntityId = 2 });
        await db.SaveChangesAsync();

        var mergeResult = await service.MergeRecordsAsync(new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2 },
            RelinkRelatedRecords = true,
            UserId = 1
        });
        mergeResult.Success.Should().BeTrue();
        (await db.Notes.FindAsync(1))!.EntityId.Should().Be(1, "merge should have relinked the note to the master");

        var unmergeResult = await service.UnmergeRecordsAsync(new UnmergeRequest
        {
            MergeGroupId = mergeResult.MergeGroupId,
            RestoreRelatedRecords = true,
            UserId = 1
        });

        unmergeResult.Success.Should().BeTrue();
        (await db.Notes.FindAsync(1))!.EntityId.Should().Be(2, "unmerge should move the note back to the restored source record");
    }

    #endregion
}
