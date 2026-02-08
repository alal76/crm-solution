// This file is part of the CRM Solution.
// Tests for ChangeManagementService - ITSM change management

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Services.ITSM;

public class ChangeManagementServiceTests
{
    private readonly Mock<IDbContextResolver> _mockContextResolver;
    private readonly Mock<ICMDBService> _mockCmdbService;
    private readonly Mock<ILogger<ChangeManagementService>> _mockLogger;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly ChangeManagementService _service;

    public ChangeManagementServiceTests()
    {
        _mockContextResolver = new Mock<IDbContextResolver>();
        _mockCmdbService = new Mock<ICMDBService>();
        _mockLogger = new Mock<ILogger<ChangeManagementService>>();
        _mockContext = new Mock<ICrmDbContext>();
        
        _mockContextResolver.Setup(x => x.ResolveContext()).Returns(_mockContext.Object);
        
        _service = new ChangeManagementService(
            _mockContextResolver.Object,
            _mockCmdbService.Object,
            _mockLogger.Object);
    }

    #region CreateChangeAsync Tests

    [Fact]
    public async Task CreateChangeAsync_CreatesChangeWithCorrectData()
    {
        // Arrange
        var dto = new CreateChangeDto
        {
            ShortDescription = "Upgrade production server",
            Description = "Upgrade server RAM and storage",
            Type = ChangeType.Standard,
            Risk = RiskLevel.Low,
            Impact = ImpactLevel.Low,
            ImplementationPlan = "1. Backup 2. Shutdown 3. Upgrade 4. Test",
            BackoutPlan = "Restore from backup",
            PlannedStartDate = DateTime.UtcNow.AddDays(7),
            PlannedEndDate = DateTime.UtcNow.AddDays(7).AddHours(4)
        };
        
        var changes = new List<Change>();
        var mockSet = CreateMockDbSet(changes);
        Change? capturedChange = null;
        mockSet.Setup(m => m.Add(It.IsAny<Change>()))
            .Callback<Change>(c => capturedChange = c);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.CreateChangeAsync(dto, createdById: 100);

        // Assert
        capturedChange.Should().NotBeNull();
        capturedChange!.ShortDescription.Should().Be("Upgrade production server");
        capturedChange.Type.Should().Be(ChangeType.Standard);
        capturedChange.Risk.Should().Be(RiskLevel.Low);
        capturedChange.State.Should().Be(ChangeState.New);
        capturedChange.RequestorId.Should().Be(100);
    }

    [Fact]
    public async Task CreateChangeAsync_GeneratesChangeNumber()
    {
        // Arrange
        var dto = new CreateChangeDto { ShortDescription = "Test change" };
        var changes = new List<Change>
        {
            new Change { ChangeId = 5, Number = "CHG0000005" }
        };
        var mockSet = CreateMockDbSet(changes);
        Change? capturedChange = null;
        mockSet.Setup(m => m.Add(It.IsAny<Change>()))
            .Callback<Change>(c => capturedChange = c);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CreateChangeAsync(dto, createdById: 1);

        // Assert
        capturedChange.Should().NotBeNull();
        capturedChange!.Number.Should().Be("CHG0000006");
    }

    [Fact]
    public async Task CreateChangeAsync_LogsCreation()
    {
        // Arrange
        var dto = new CreateChangeDto { ShortDescription = "Test" };
        var changes = new List<Change>();
        var mockSet = CreateMockDbSet(changes);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CreateChangeAsync(dto, createdById: 1);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created change")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetChangeByIdAsync Tests

    [Fact]
    public async Task GetChangeByIdAsync_WhenExists_ReturnsChange()
    {
        // Arrange
        var requestor = new User { Id = 100, Username = "john.doe" };
        var changes = new List<Change>
        {
            new Change 
            { 
                ChangeId = 1, 
                Number = "CHG0000001",
                ShortDescription = "Test change",
                Type = ChangeType.Normal,
                State = ChangeState.New,
                RequestorId = 100,
                Requestor = requestor
            }
        };

        var mockSet = CreateMockDbSet(changes);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        SetupImpactedCIsCount(1, 3);

        // Act
        var result = await _service.GetChangeByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Number.Should().Be("CHG0000001");
        result.RequestorName.Should().Be("john.doe");
    }

    [Fact]
    public async Task GetChangeByIdAsync_WhenDeleted_ReturnsNull()
    {
        // Arrange
        var changes = new List<Change>
        {
            new Change { ChangeId = 1, IsDeleted = true }
        };

        var mockSet = CreateMockDbSet(changes);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);

        // Act
        var result = await _service.GetChangeByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetChangeByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var changes = new List<Change>();
        var mockSet = CreateMockDbSet(changes);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);

        // Act
        var result = await _service.GetChangeByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetChangesAsync Tests

    [Fact]
    public async Task GetChangesAsync_WithNoFilters_ReturnsAllChanges()
    {
        // Arrange
        var changes = new List<Change>
        {
            new Change { ChangeId = 1, Number = "CHG0000001", ShortDescription = "Change 1", CreatedAt = DateTime.UtcNow },
            new Change { ChangeId = 2, Number = "CHG0000002", ShortDescription = "Change 2", CreatedAt = DateTime.UtcNow }
        };

        var mockSet = CreateMockDbSet(changes);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        SetupImpactedCIsCount(1, 0);
        SetupImpactedCIsCount(2, 0);

        var filter = new ChangeFilterDto { PageNumber = 1, PageSize = 10 };

        // Act
        var (items, count) = await _service.GetChangesAsync(filter);

        // Assert
        items.Should().HaveCount(2);
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetChangesAsync_WithSearchTerm_FiltersByNumberOrDescription()
    {
        // Arrange
        var changes = new List<Change>
        {
            new Change { ChangeId = 1, Number = "CHG0000001", ShortDescription = "Upgrade server" },
            new Change { ChangeId = 2, Number = "CHG0000002", ShortDescription = "Network change" },
            new Change { ChangeId = 3, Number = "CHG0000003", ShortDescription = "Database patch" }
        };

        var mockSet = CreateMockDbSet(changes);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        SetupImpactedCIsCount(1, 0);
        SetupImpactedCIsCount(3, 0);

        var filter = new ChangeFilterDto { SearchTerm = "CHG0000001", PageNumber = 1, PageSize = 10 };

        // Act
        var (items, count) = await _service.GetChangesAsync(filter);

        // Assert
        items.Should().HaveCount(1);
        items.First().Number.Should().Be("CHG0000001");
    }

    [Fact]
    public async Task GetChangesAsync_WithStateFilter_FiltersByState()
    {
        // Arrange
        var changes = new List<Change>
        {
            new Change { ChangeId = 1, Number = "CHG0000001", State = ChangeState.New },
            new Change { ChangeId = 2, Number = "CHG0000002", State = ChangeState.Scheduled },
            new Change { ChangeId = 3, Number = "CHG0000003", State = ChangeState.New }
        };

        var mockSet = CreateMockDbSet(changes);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        SetupImpactedCIsCount(1, 0);
        SetupImpactedCIsCount(3, 0);

        var filter = new ChangeFilterDto { State = ChangeState.New, PageNumber = 1, PageSize = 10 };

        // Act
        var (items, count) = await _service.GetChangesAsync(filter);

        // Assert
        items.Should().HaveCount(2);
        items.All(i => i.State == ChangeState.New).Should().BeTrue();
    }

    [Fact]
    public async Task GetChangesAsync_WithTypeFilter_FiltersByType()
    {
        // Arrange
        var changes = new List<Change>
        {
            new Change { ChangeId = 1, Number = "CHG0000001", Type = ChangeType.Standard },
            new Change { ChangeId = 2, Number = "CHG0000002", Type = ChangeType.Emergency },
            new Change { ChangeId = 3, Number = "CHG0000003", Type = ChangeType.Standard }
        };

        var mockSet = CreateMockDbSet(changes);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        SetupImpactedCIsCount(1, 0);
        SetupImpactedCIsCount(3, 0);

        var filter = new ChangeFilterDto { Type = ChangeType.Standard, PageNumber = 1, PageSize = 10 };

        // Act
        var (items, count) = await _service.GetChangesAsync(filter);

        // Assert
        items.Should().HaveCount(2);
        items.All(i => i.Type == ChangeType.Standard).Should().BeTrue();
    }

    [Fact]
    public async Task GetChangesAsync_WithDateRange_FiltersCorrectly()
    {
        // Arrange
        var startRange = DateTime.UtcNow.AddDays(5);
        var endRange = DateTime.UtcNow.AddDays(10);
        var changes = new List<Change>
        {
            new Change { ChangeId = 1, PlannedStartDate = DateTime.UtcNow.AddDays(7) }, // In range
            new Change { ChangeId = 2, PlannedStartDate = DateTime.UtcNow.AddDays(1) }, // Before range
            new Change { ChangeId = 3, PlannedStartDate = DateTime.UtcNow.AddDays(15) } // After range
        };

        var mockSet = CreateMockDbSet(changes);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        SetupImpactedCIsCount(1, 0);

        var filter = new ChangeFilterDto 
        { 
            PlannedStartFrom = startRange, 
            PlannedStartTo = endRange,
            PageNumber = 1, 
            PageSize = 10 
        };

        // Act
        var (items, count) = await _service.GetChangesAsync(filter);

        // Assert
        items.Should().HaveCount(1);
        items.First().PlannedStartDate.Should().BeWithin(TimeSpan.FromDays(3)).Before(DateTime.UtcNow.AddDays(10));
    }

    [Fact]
    public async Task GetChangesAsync_SupportsPagination()
    {
        // Arrange
        var changes = Enumerable.Range(1, 20)
            .Select(i => new Change { ChangeId = i, Number = $"CHG{i:D7}", CreatedAt = DateTime.UtcNow.AddMinutes(-i) })
            .ToList();

        var mockSet = CreateMockDbSet(changes);
        _mockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        for (int i = 1; i <= 20; i++) SetupImpactedCIsCount(i, 0);

        var filter = new ChangeFilterDto { PageNumber = 2, PageSize = 5 };

        // Act
        var (items, count) = await _service.GetChangesAsync(filter);

        // Assert
        items.Should().HaveCount(5);
        count.Should().Be(20);
    }

    #endregion

    #region SubmitForApprovalAsync Tests

    [Fact]
    public async Task SubmitForApprovalAsync_WhenNew_SubmitsSuccessfully()
    {
        // Arrange
        var change = new Change { ChangeId = 1, Number = "CHG0000001", State = ChangeState.New };
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.SubmitForApprovalAsync(1, modifiedById: 100);

        // Assert
        result.Should().BeTrue();
        change.State.Should().Be(ChangeState.Assess);
    }

    [Fact]
    public async Task SubmitForApprovalAsync_WhenNotNew_ThrowsException()
    {
        // Arrange
        var change = new Change { ChangeId = 1, State = ChangeState.Scheduled };
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);

        // Act & Assert
        var act = () => _service.SubmitForApprovalAsync(1, modifiedById: 100);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*New state*");
    }

    [Fact]
    public async Task SubmitForApprovalAsync_WhenNotFound_ThrowsException()
    {
        // Arrange
        _mockContext.Setup(c => c.Changes.FindAsync(It.IsAny<int>())).ReturnsAsync((Change?)null);

        // Act & Assert
        var act = () => _service.SubmitForApprovalAsync(999, modifiedById: 100);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SubmitForApprovalAsync_LogsSubmission()
    {
        // Arrange
        var change = new Change { ChangeId = 1, Number = "CHG0000001", State = ChangeState.New };
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.SubmitForApprovalAsync(1, modifiedById: 100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Submitted change")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ApproveChangeAsync Tests

    [Fact]
    public async Task ApproveChangeAsync_CreatesApprovalRecord()
    {
        // Arrange
        var change = new Change { ChangeId = 1, State = ChangeState.Assess };
        var approvals = new List<ChangeApproval>();
        var mockApprovalSet = CreateMockDbSet(approvals);
        ChangeApproval? capturedApproval = null;
        mockApprovalSet.Setup(m => m.Add(It.IsAny<ChangeApproval>()))
            .Callback<ChangeApproval>(a => capturedApproval = a);
        
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);
        _mockContext.Setup(c => c.ChangeApprovals).Returns(mockApprovalSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.ApproveChangeAsync(1, approverId: 50, comments: "Approved for deployment");

        // Assert
        result.Should().BeTrue();
        capturedApproval.Should().NotBeNull();
        capturedApproval!.ApproverId.Should().Be(50);
        capturedApproval.ApprovalStatus.Should().Be(CRM.Core.Entities.ITSM.ApprovalStatus.Approved);
        capturedApproval.Comments.Should().Be("Approved for deployment");
    }

    [Fact]
    public async Task ApproveChangeAsync_WhenInAssess_TransitionsToAuthorize()
    {
        // Arrange
        var change = new Change { ChangeId = 1, State = ChangeState.Assess };
        var approvals = new List<ChangeApproval>();
        var mockApprovalSet = CreateMockDbSet(approvals);
        
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);
        _mockContext.Setup(c => c.ChangeApprovals).Returns(mockApprovalSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.ApproveChangeAsync(1, approverId: 50, comments: "OK");

        // Assert
        change.State.Should().Be(ChangeState.Authorize);
    }

    #endregion

    #region ScheduleChangeAsync Tests

    [Fact]
    public async Task ScheduleChangeAsync_SchedulesSuccessfully()
    {
        // Arrange
        var plannedStart = DateTime.UtcNow.AddDays(7);
        var plannedEnd = DateTime.UtcNow.AddDays(7).AddHours(4);
        var change = new Change { ChangeId = 1, State = ChangeState.Authorize };
        var blackouts = new List<ChangeBlackout>();
        var mockBlackoutSet = CreateMockDbSet(blackouts);
        
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);
        _mockContext.Setup(c => c.ChangeBlackouts).Returns(mockBlackoutSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.ScheduleChangeAsync(1, plannedStart, plannedEnd, modifiedById: 100);

        // Assert
        result.Should().BeTrue();
        change.State.Should().Be(ChangeState.Scheduled);
        change.PlannedStartDate.Should().Be(plannedStart);
        change.PlannedEndDate.Should().Be(plannedEnd);
    }

    [Fact]
    public async Task ScheduleChangeAsync_WhenBlackoutConflict_ThrowsException()
    {
        // Arrange
        var plannedStart = DateTime.UtcNow.AddDays(7);
        var plannedEnd = DateTime.UtcNow.AddDays(7).AddHours(4);
        var change = new Change { ChangeId = 1, State = ChangeState.Authorize };
        var blackouts = new List<ChangeBlackout>
        {
            new ChangeBlackout 
            { 
                BlackoutId = 1, 
                Name = "Year-end freeze",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(14)
            }
        };
        var mockBlackoutSet = CreateMockDbSet(blackouts);
        
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);
        _mockContext.Setup(c => c.ChangeBlackouts).Returns(mockBlackoutSet.Object);

        // Act & Assert
        var act = () => _service.ScheduleChangeAsync(1, plannedStart, plannedEnd, modifiedById: 100);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*blackout*");
    }

    [Fact]
    public async Task ScheduleChangeAsync_WhenNotFound_ThrowsException()
    {
        // Arrange
        _mockContext.Setup(c => c.Changes.FindAsync(It.IsAny<int>())).ReturnsAsync((Change?)null);

        // Act & Assert
        var act = () => _service.ScheduleChangeAsync(999, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 100);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region CheckConflictsAsync Tests

    [Fact]
    public async Task CheckConflictsAsync_WhenNoConflicts_ReturnsFalse()
    {
        // Arrange
        var change = new Change 
        { 
            ChangeId = 1, 
            PlannedStartDate = DateTime.UtcNow.AddDays(7),
            PlannedEndDate = DateTime.UtcNow.AddDays(7).AddHours(4)
        };
        var changes = new List<Change> { change };
        var blackouts = new List<ChangeBlackout>();
        
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);
        _mockContext.Setup(c => c.Changes).Returns(CreateMockDbSet(changes).Object);
        _mockContext.Setup(c => c.ChangeBlackouts).Returns(CreateMockDbSet(blackouts).Object);

        // Act
        var result = await _service.CheckConflictsAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckConflictsAsync_WhenOverlappingChanges_ReturnsTrue()
    {
        // Arrange
        var change1 = new Change 
        { 
            ChangeId = 1, 
            State = ChangeState.New,
            PlannedStartDate = DateTime.UtcNow.AddDays(7),
            PlannedEndDate = DateTime.UtcNow.AddDays(7).AddHours(4)
        };
        var change2 = new Change 
        { 
            ChangeId = 2, 
            State = ChangeState.Scheduled,
            PlannedStartDate = DateTime.UtcNow.AddDays(7).AddHours(2),
            PlannedEndDate = DateTime.UtcNow.AddDays(7).AddHours(6)
        };
        var changes = new List<Change> { change1, change2 };
        var blackouts = new List<ChangeBlackout>();
        
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change1);
        _mockContext.Setup(c => c.Changes).Returns(CreateMockDbSet(changes).Object);
        _mockContext.Setup(c => c.ChangeBlackouts).Returns(CreateMockDbSet(blackouts).Object);

        // Act
        var result = await _service.CheckConflictsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckConflictsAsync_WhenNoPlannedDates_ReturnsFalse()
    {
        // Arrange
        var change = new Change { ChangeId = 1, PlannedStartDate = null, PlannedEndDate = null };
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);

        // Act
        var result = await _service.CheckConflictsAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddImpactedCIAsync Tests

    [Fact]
    public async Task AddImpactedCIAsync_AddsNewCI()
    {
        // Arrange
        var impactedCIs = new List<ChangeImpactedCI>();
        var mockSet = CreateMockDbSet(impactedCIs);
        ChangeImpactedCI? capturedCI = null;
        mockSet.Setup(m => m.Add(It.IsAny<ChangeImpactedCI>()))
            .Callback<ChangeImpactedCI>(ci => capturedCI = ci);
        
        _mockContext.Setup(c => c.ChangeImpactedCIs).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.AddImpactedCIAsync(changeId: 1, ciId: 100, createdById: 50);

        // Assert
        result.Should().BeTrue();
        capturedCI.Should().NotBeNull();
        capturedCI!.ChangeId.Should().Be(1);
        capturedCI.CIId.Should().Be(100);
    }

    [Fact]
    public async Task AddImpactedCIAsync_WhenAlreadyExists_ReturnsFalse()
    {
        // Arrange
        var impactedCIs = new List<ChangeImpactedCI>
        {
            new ChangeImpactedCI { ChangeId = 1, CIId = 100 }
        };
        var mockSet = CreateMockDbSet(impactedCIs);
        _mockContext.Setup(c => c.ChangeImpactedCIs).Returns(mockSet.Object);

        // Act
        var result = await _service.AddImpactedCIAsync(changeId: 1, ciId: 100, createdById: 50);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetImpactedCIsAsync Tests

    [Fact]
    public async Task GetImpactedCIsAsync_ReturnsCIsForChange()
    {
        // Arrange
        var owner = new User { Id = 10, Username = "sysadmin" };
        var impactedCIs = new List<ChangeImpactedCI>
        {
            new ChangeImpactedCI { ChangeId = 1, CIId = 100 },
            new ChangeImpactedCI { ChangeId = 1, CIId = 101 }
        };
        var cis = new List<ConfigurationItem>
        {
            new ConfigurationItem 
            { 
                CIId = 100, 
                CIName = "PROD-WEB-01", 
                CINumber = "CI0000100",
                CIType = "Server",
                OperationalStatus = "Operational",
                OwnerId = 10,
                Owner = owner
            },
            new ConfigurationItem 
            { 
                CIId = 101, 
                CIName = "PROD-DB-01", 
                CINumber = "CI0000101",
                CIType = "Database",
                OperationalStatus = "Operational",
                OwnerId = 10,
                Owner = owner
            }
        };

        _mockContext.Setup(c => c.ChangeImpactedCIs).Returns(CreateMockDbSet(impactedCIs).Object);
        _mockContext.Setup(c => c.ConfigurationItems).Returns(CreateMockDbSet(cis).Object);

        // Act
        var result = (await _service.GetImpactedCIsAsync(1)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(ci => ci.CIName == "PROD-WEB-01");
        result.Should().Contain(ci => ci.CIName == "PROD-DB-01");
    }

    [Fact]
    public async Task GetImpactedCIsAsync_ExcludesDeletedCIs()
    {
        // Arrange
        var impactedCIs = new List<ChangeImpactedCI>
        {
            new ChangeImpactedCI { ChangeId = 1, CIId = 100 },
            new ChangeImpactedCI { ChangeId = 1, CIId = 101 }
        };
        var cis = new List<ConfigurationItem>
        {
            new ConfigurationItem { CIId = 100, CIName = "Active CI", IsDeleted = false },
            new ConfigurationItem { CIId = 101, CIName = "Deleted CI", IsDeleted = true }
        };

        _mockContext.Setup(c => c.ChangeImpactedCIs).Returns(CreateMockDbSet(impactedCIs).Object);
        _mockContext.Setup(c => c.ConfigurationItems).Returns(CreateMockDbSet(cis).Object);

        // Act
        var result = await _service.GetImpactedCIsAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().CIName.Should().Be("Active CI");
    }

    #endregion

    #region UpdateChangeAsync Tests

    [Fact]
    public async Task UpdateChangeAsync_UpdatesAllFields()
    {
        // Arrange
        var change = new Change 
        { 
            ChangeId = 1, 
            ShortDescription = "Old description",
            Description = "Old long description",
            Type = ChangeType.Standard
        };
        var dto = new CreateChangeDto
        {
            ShortDescription = "New description",
            Description = "New long description",
            Type = ChangeType.Normal,
            Risk = RiskLevel.Medium,
            Impact = ImpactLevel.High,
            ImplementationPlan = "New plan",
            BackoutPlan = "New backout",
            PlannedStartDate = DateTime.UtcNow.AddDays(5),
            PlannedEndDate = DateTime.UtcNow.AddDays(5).AddHours(2)
        };

        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
        SetupImpactedCIsCount(1, 0);

        // Act
        var result = await _service.UpdateChangeAsync(1, dto, modifiedById: 50);

        // Assert
        change.ShortDescription.Should().Be("New description");
        change.Description.Should().Be("New long description");
        change.Type.Should().Be(ChangeType.Normal);
        change.Risk.Should().Be(RiskLevel.Medium);
        change.Impact.Should().Be(ImpactLevel.High);
    }

    [Fact]
    public async Task UpdateChangeAsync_WhenNotFound_ThrowsException()
    {
        // Arrange
        _mockContext.Setup(c => c.Changes.FindAsync(It.IsAny<int>())).ReturnsAsync((Change?)null);

        // Act & Assert
        var act = () => _service.UpdateChangeAsync(999, new CreateChangeDto(), modifiedById: 50);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region RejectChangeAsync Tests

    [Fact]
    public async Task RejectChangeAsync_RejectsChange()
    {
        // Arrange
        var change = new Change { ChangeId = 1, State = ChangeState.Assess };
        var approvals = new List<ChangeApproval>();
        var mockApprovalSet = CreateMockDbSet(approvals);
        
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);
        _mockContext.Setup(c => c.ChangeApprovals).Returns(mockApprovalSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.RejectChangeAsync(1, approverId: 50, comments: "Not approved");

        // Assert
        result.Should().BeTrue();
        change.State.Should().Be(ChangeState.Cancelled);
    }

    [Fact]
    public async Task RejectChangeAsync_CreatesRejectionRecord()
    {
        // Arrange
        var change = new Change { ChangeId = 1, State = ChangeState.Assess };
        var approvals = new List<ChangeApproval>();
        var mockApprovalSet = CreateMockDbSet(approvals);
        ChangeApproval? capturedApproval = null;
        mockApprovalSet.Setup(m => m.Add(It.IsAny<ChangeApproval>()))
            .Callback<ChangeApproval>(a => capturedApproval = a);
        
        _mockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);
        _mockContext.Setup(c => c.ChangeApprovals).Returns(mockApprovalSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.RejectChangeAsync(1, approverId: 50, comments: "Rejected");

        // Assert
        capturedApproval.Should().NotBeNull();
        capturedApproval!.ApprovalStatus.Should().Be(CRM.Core.Entities.ITSM.ApprovalStatus.Rejected);
    }

    [Fact]
    public async Task RejectChangeAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        _mockContext.Setup(c => c.Changes.FindAsync(It.IsAny<int>())).ReturnsAsync((Change?)null);

        // Act
        var result = await _service.RejectChangeAsync(999, approverId: 50, comments: null);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetBlackoutPeriodsAsync Tests

    [Fact]
    public async Task GetBlackoutPeriodsAsync_ReturnsPeriodsInRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddMonths(1);
        var blackouts = new List<ChangeBlackout>
        {
            new ChangeBlackout 
            { 
                BlackoutId = 1, 
                Name = "Holiday Freeze",
                Reason = "No deployments during holidays",
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(10)
            },
            new ChangeBlackout 
            { 
                BlackoutId = 2, 
                Name = "Outside Range",
                StartDate = DateTime.UtcNow.AddMonths(2),
                EndDate = DateTime.UtcNow.AddMonths(2).AddDays(5)
            }
        };

        _mockContext.Setup(c => c.ChangeBlackouts).Returns(CreateMockDbSet(blackouts).Object);

        // Act
        var result = (await _service.GetBlackoutPeriodsAsync(startDate, endDate)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Holiday Freeze");
    }

    [Fact]
    public async Task GetBlackoutPeriodsAsync_ExcludesDeletedPeriods()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddMonths(1);
        var blackouts = new List<ChangeBlackout>
        {
            new ChangeBlackout { BlackoutId = 1, Name = "Active", StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(10), IsDeleted = false },
            new ChangeBlackout { BlackoutId = 2, Name = "Deleted", StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(10), IsDeleted = true }
        };

        _mockContext.Setup(c => c.ChangeBlackouts).Returns(CreateMockDbSet(blackouts).Object);

        // Act
        var result = await _service.GetBlackoutPeriodsAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active");
    }

    #endregion

    #region CreateBlackoutPeriodAsync Tests

    [Fact]
    public async Task CreateBlackoutPeriodAsync_CreatesBlackout()
    {
        // Arrange
        var dto = new CreateBlackoutPeriodInfo
        {
            Name = "Year End Freeze",
            Reason = "No changes during audit period",
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(45)
        };
        var blackouts = new List<ChangeBlackout>();
        var mockSet = CreateMockDbSet(blackouts);
        ChangeBlackout? captured = null;
        mockSet.Setup(m => m.Add(It.IsAny<ChangeBlackout>()))
            .Callback<ChangeBlackout>(b => captured = b);
        
        _mockContext.Setup(c => c.ChangeBlackouts).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.CreateBlackoutPeriodAsync(dto, createdById: 100);

        // Assert
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Year End Freeze");
        captured.Reason.Should().Be("No changes during audit period");
        captured.CreatedById.Should().Be(100);
        result.Name.Should().Be("Year End Freeze");
    }

    [Fact]
    public async Task CreateBlackoutPeriodAsync_LogsCreation()
    {
        // Arrange
        var dto = new CreateBlackoutPeriodInfo
        {
            Name = "Test Freeze",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1)
        };
        var blackouts = new List<ChangeBlackout>();
        var mockSet = CreateMockDbSet(blackouts);
        _mockContext.Setup(c => c.ChangeBlackouts).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CreateBlackoutPeriodAsync(dto, createdById: 100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created blackout period")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupImpactedCIsCount(int changeId, int count)
    {
        var impactedCIs = Enumerable.Range(1, count)
            .Select(i => new ChangeImpactedCI { ChangeId = changeId, CIId = i })
            .ToList();
        _mockContext.Setup(c => c.ChangeImpactedCIs).Returns(CreateMockDbSet(impactedCIs).Object);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(queryable.Expression);
        
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(queryable.ElementType);
        
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(queryable.GetEnumerator());

        return mockSet;
    }

    #endregion
}
