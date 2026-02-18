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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ApprovalWorkflowService covering matrix management, level management,
/// group management, request management, workflow operations, and statistics.
/// </summary>
public class ApprovalWorkflowServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<ApprovalWorkflowService>> _loggerMock;
    private readonly ApprovalWorkflowService _service;

    public ApprovalWorkflowServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"ApprovalWorkflowTests_{Guid.NewGuid()}")
            .Options;

        _context = new CrmDbContext(options, null!);
        _loggerMock = new Mock<ILogger<ApprovalWorkflowService>>();
        _service = new ApprovalWorkflowService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Helper Methods

    private DiscountApprovalMatrix CreateTestMatrix(string name = "Test Matrix", bool isActive = true)
    {
        return new DiscountApprovalMatrix
        {
            Name = name,
            Description = "Test matrix description",
            IsActive = isActive,
            Priority = 1,
            AppliesToAllProducts = true,
            RequireAllLevels = false,
            AllowParallelApproval = false,
            AutoEscalateHours = 24,
            ReminderHours = 8,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ApprovalLevel CreateTestLevel(int matrixId, int order = 1, string name = "Level 1")
    {
        return new ApprovalLevel
        {
            Name = name,
            LevelOrder = order,
            DiscountApprovalMatrixId = matrixId,
            ThresholdType = ApprovalThresholdType.DiscountPercent,
            MinValue = 5,
            MaxValue = 15,
            SendEmailOnPending = true,
            IncludeQuoteDetails = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ApprovalGroup CreateTestGroup(string name = "Test Group", bool isActive = true)
    {
        return new ApprovalGroup
        {
            Name = name,
            Description = "Test group description",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    private User CreateTestUser(string username = "testuser")
    {
        return new User
        {
            Username = username,
            Email = $"{username}@test.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private Account CreateTestAccount(string name = "Test Account")
    {
        return new Account
        {
            Company = name,
            Email = "account@test.com",
            LifecycleStage = AccountLifecycleStage.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    private Quote CreateTestQuote(int accountId, decimal discountPercent = 10m)
    {
        return new Quote
        {
            QuoteNumber = $"Q-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}",
            Name = "Test Quote",
            AccountId = accountId,
            Status = QuoteStatus.Draft,
            DiscountPercent = discountPercent,
            DiscountAmount = 100,
            Subtotal = 1000,
            TotalAmount = 900,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
    }

    private ApprovalRequest CreateTestRequest(int quoteId, int submitterId, int matrixId)
    {
        return new ApprovalRequest
        {
            RequestNumber = $"APR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}",
            Status = DiscountApprovalStatus.Pending,
            QuoteId = quoteId,
            SubmitterId = submitterId,
            DiscountApprovalMatrixId = matrixId,
            DiscountPercent = 10,
            DiscountAmount = 100,
            DealAmount = 1000,
            CurrentLevel = 1,
            MaxLevelRequired = 2,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Matrix Management Tests

    [Fact]
    public async Task GetAllMatricesAsync_ReturnsAllMatrices_WhenNoFilter()
    {
        // Arrange
        var matrix1 = CreateTestMatrix("Matrix 1");
        var matrix2 = CreateTestMatrix("Matrix 2");
        _context.DiscountApprovalMatrices.AddRange(matrix1, matrix2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllMatricesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllMatricesAsync_FiltersActiveMatrices_WhenIsActiveTrue()
    {
        // Arrange
        var activeMatrix = CreateTestMatrix("Active", isActive: true);
        var inactiveMatrix = CreateTestMatrix("Inactive", isActive: false);
        _context.DiscountApprovalMatrices.AddRange(activeMatrix, inactiveMatrix);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllMatricesAsync(isActive: true);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active");
    }

    [Fact]
    public async Task GetAllMatricesAsync_ExcludesDeletedMatrices()
    {
        // Arrange
        var matrix1 = CreateTestMatrix("Matrix 1");
        var deletedMatrix = CreateTestMatrix("Deleted");
        deletedMatrix.IsDeleted = true;
        _context.DiscountApprovalMatrices.AddRange(matrix1, deletedMatrix);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllMatricesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Matrix 1");
    }

    [Fact]
    public async Task GetMatrixByIdAsync_ReturnsMatrix_WhenExists()
    {
        // Arrange
        var matrix = CreateTestMatrix("Test Matrix");
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMatrixByIdAsync(matrix.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Matrix");
    }

    [Fact]
    public async Task GetMatrixByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetMatrixByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateMatrixAsync_CreatesMatrix_WithCorrectData()
    {
        // Arrange
        var matrix = CreateTestMatrix("New Matrix");

        // Act
        var result = await _service.CreateMatrixAsync(matrix);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Matrix");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateMatrixAsync_UpdatesMatrix_WhenExists()
    {
        // Arrange
        var matrix = CreateTestMatrix("Original");
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        matrix.Name = "Updated";
        matrix.Description = "Updated description";

        // Act
        var result = await _service.UpdateMatrixAsync(matrix);

        // Assert
        result.Name.Should().Be("Updated");
        result.Description.Should().Be("Updated description");
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateMatrixAsync_ThrowsException_WhenNotExists()
    {
        // Arrange
        var matrix = CreateTestMatrix();
        matrix.Id = 999;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateMatrixAsync(matrix));
    }

    [Fact]
    public async Task DeleteMatrixAsync_SoftDeletes_WhenExists()
    {
        // Arrange
        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteMatrixAsync(matrix.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.DiscountApprovalMatrices.FindAsync(matrix.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteMatrixAsync_ReturnsFalse_WhenNotExists()
    {
        // Act
        var result = await _service.DeleteMatrixAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateMatrixAsync_SetsIsActiveTrue()
    {
        // Arrange
        var matrix = CreateTestMatrix(isActive: false);
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ActivateMatrixAsync(matrix.Id);

        // Assert
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateMatrixAsync_SetsIsActiveFalse()
    {
        // Arrange
        var matrix = CreateTestMatrix(isActive: true);
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeactivateMatrixAsync(matrix.Id);

        // Assert
        result.IsActive.Should().BeFalse();
    }

    #endregion

    #region Approval Level Management Tests

    [Fact]
    public async Task GetMatrixLevelsAsync_ReturnsLevelsOrdered()
    {
        // Arrange
        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var level1 = CreateTestLevel(matrix.Id, 1, "Level 1");
        var level2 = CreateTestLevel(matrix.Id, 2, "Level 2");
        var level3 = CreateTestLevel(matrix.Id, 3, "Level 3");
        _context.ApprovalLevels.AddRange(level3, level1, level2); // Add out of order
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMatrixLevelsAsync(matrix.Id);

        // Assert
        result.Should().HaveCount(3);
        result.First().LevelOrder.Should().Be(1);
        result.Last().LevelOrder.Should().Be(3);
    }

    [Fact]
    public async Task GetLevelByIdAsync_ReturnsLevel_WhenExists()
    {
        // Arrange
        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var level = CreateTestLevel(matrix.Id);
        _context.ApprovalLevels.Add(level);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLevelByIdAsync(level.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Level 1");
    }

    [Fact]
    public async Task AddLevelAsync_AddsLevel_WithAutoOrder()
    {
        // Arrange
        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var existingLevel = CreateTestLevel(matrix.Id, 1, "Existing");
        _context.ApprovalLevels.Add(existingLevel);
        await _context.SaveChangesAsync();

        var newLevel = new ApprovalLevel
        {
            Name = "New Level",
            ThresholdType = ApprovalThresholdType.DiscountPercent,
            MinValue = 10,
            MaxValue = 20
        };

        // Act
        var result = await _service.AddLevelAsync(matrix.Id, newLevel);

        // Assert
        result.LevelOrder.Should().Be(2);
        result.DiscountApprovalMatrixId.Should().Be(matrix.Id);
    }

    [Fact]
    public async Task UpdateLevelAsync_UpdatesLevel_WhenExists()
    {
        // Arrange
        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var level = CreateTestLevel(matrix.Id);
        _context.ApprovalLevels.Add(level);
        await _context.SaveChangesAsync();

        level.Name = "Updated Level";
        level.MinValue = 10;

        // Act
        var result = await _service.UpdateLevelAsync(level);

        // Assert
        result.Name.Should().Be("Updated Level");
        result.MinValue.Should().Be(10);
    }

    [Fact]
    public async Task RemoveLevelAsync_SoftDeletes_WhenExists()
    {
        // Arrange
        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var level = CreateTestLevel(matrix.Id);
        _context.ApprovalLevels.Add(level);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RemoveLevelAsync(level.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.ApprovalLevels.FindAsync(level.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task ReorderLevelsAsync_ReordersCorrectly()
    {
        // Arrange
        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var level1 = CreateTestLevel(matrix.Id, 1, "Level 1");
        var level2 = CreateTestLevel(matrix.Id, 2, "Level 2");
        var level3 = CreateTestLevel(matrix.Id, 3, "Level 3");
        _context.ApprovalLevels.AddRange(level1, level2, level3);
        await _context.SaveChangesAsync();

        // Reorder: 3, 1, 2
        var newOrder = new[] { level3.Id, level1.Id, level2.Id };

        // Act
        var result = await _service.ReorderLevelsAsync(matrix.Id, newOrder);

        // Assert
        var levels = result.ToList();
        levels[0].Id.Should().Be(level3.Id);
        levels[0].LevelOrder.Should().Be(1);
        levels[1].Id.Should().Be(level1.Id);
        levels[1].LevelOrder.Should().Be(2);
        levels[2].Id.Should().Be(level2.Id);
        levels[2].LevelOrder.Should().Be(3);
    }

    #endregion

    #region Approval Group Management Tests

    [Fact]
    public async Task GetAllGroupsAsync_ReturnsAllGroups_WhenNoFilter()
    {
        // Arrange
        var group1 = CreateTestGroup("Group 1");
        var group2 = CreateTestGroup("Group 2");
        _context.ApprovalGroups.AddRange(group1, group2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllGroupsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllGroupsAsync_FiltersActiveGroups()
    {
        // Arrange
        var activeGroup = CreateTestGroup("Active", isActive: true);
        var inactiveGroup = CreateTestGroup("Inactive", isActive: false);
        _context.ApprovalGroups.AddRange(activeGroup, inactiveGroup);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllGroupsAsync(isActive: true);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active");
    }

    [Fact]
    public async Task GetGroupByIdAsync_ReturnsGroup_WithMembers()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var group = CreateTestGroup();
        _context.ApprovalGroups.Add(group);
        await _context.SaveChangesAsync();

        var member = new ApprovalGroupMember
        {
            ApprovalGroupId = group.Id,
            UserId = user.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.ApprovalGroupMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetGroupByIdAsync(group.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Members.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateGroupAsync_CreatesGroup_WithCorrectData()
    {
        // Arrange
        var group = CreateTestGroup("New Group");

        // Act
        var result = await _service.CreateGroupAsync(group);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Group");
    }

    [Fact]
    public async Task UpdateGroupAsync_UpdatesGroup_WhenExists()
    {
        // Arrange
        var group = CreateTestGroup("Original");
        _context.ApprovalGroups.Add(group);
        await _context.SaveChangesAsync();

        group.Name = "Updated";

        // Act
        var result = await _service.UpdateGroupAsync(group);

        // Assert
        result.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteGroupAsync_SoftDeletes_WhenExists()
    {
        // Arrange
        var group = CreateTestGroup();
        _context.ApprovalGroups.Add(group);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteGroupAsync(group.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.ApprovalGroups.FindAsync(group.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task AddGroupMemberAsync_AddsMember_WhenNew()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var group = CreateTestGroup();
        _context.ApprovalGroups.Add(group);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AddGroupMemberAsync(group.Id, user.Id);

        // Assert
        result.ApprovalGroupId.Should().Be(group.Id);
        result.UserId.Should().Be(user.Id);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AddGroupMemberAsync_ReactivatesMember_WhenPreviouslyRemoved()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var group = CreateTestGroup();
        _context.ApprovalGroups.Add(group);
        await _context.SaveChangesAsync();

        var member = new ApprovalGroupMember
        {
            ApprovalGroupId = group.Id,
            UserId = user.Id,
            IsActive = false,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.ApprovalGroupMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AddGroupMemberAsync(group.Id, user.Id);

        // Assert
        result.IsActive.Should().BeTrue();
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveGroupMemberAsync_RemovesMember_WhenExists()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var group = CreateTestGroup();
        _context.ApprovalGroups.Add(group);
        await _context.SaveChangesAsync();

        var member = new ApprovalGroupMember
        {
            ApprovalGroupId = group.Id,
            UserId = user.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.ApprovalGroupMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RemoveGroupMemberAsync(group.Id, user.Id);

        // Assert
        result.Should().BeTrue();
        var removed = await _context.ApprovalGroupMembers.FindAsync(member.Id);
        removed!.IsActive.Should().BeFalse();
        removed.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetGroupMembersAsync_ReturnsActiveMembers()
    {
        // Arrange
        var user1 = CreateTestUser("user1");
        var user2 = CreateTestUser("user2");
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var group = CreateTestGroup();
        _context.ApprovalGroups.Add(group);
        await _context.SaveChangesAsync();

        var member1 = new ApprovalGroupMember
        {
            ApprovalGroupId = group.Id,
            UserId = user1.Id,
            IsActive = true,
            Order = 1,
            CreatedAt = DateTime.UtcNow
        };
        var member2 = new ApprovalGroupMember
        {
            ApprovalGroupId = group.Id,
            UserId = user2.Id,
            IsActive = false, // Inactive
            Order = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.ApprovalGroupMembers.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetGroupMembersAsync(group.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(user1.Id);
    }

    #endregion

    #region Approval Request Management Tests

    [Fact]
    public async Task GetAllRequestsAsync_ReturnsAllRequests_WhenNoFilter()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote1 = CreateTestQuote(account.Id);
        var quote2 = CreateTestQuote(account.Id);
        _context.Quotes.AddRange(quote1, quote2);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var request1 = CreateTestRequest(quote1.Id, user.Id, matrix.Id);
        var request2 = CreateTestRequest(quote2.Id, user.Id, matrix.Id);
        _context.ApprovalRequests.AddRange(request1, request2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllRequestsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllRequestsAsync_FiltersByStatus()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote1 = CreateTestQuote(account.Id);
        var quote2 = CreateTestQuote(account.Id);
        _context.Quotes.AddRange(quote1, quote2);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var pendingRequest = CreateTestRequest(quote1.Id, user.Id, matrix.Id);
        pendingRequest.Status = DiscountApprovalStatus.Pending;

        var approvedRequest = CreateTestRequest(quote2.Id, user.Id, matrix.Id);
        approvedRequest.Status = DiscountApprovalStatus.Approved;

        _context.ApprovalRequests.AddRange(pendingRequest, approvedRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllRequestsAsync(status: DiscountApprovalStatus.Pending);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(DiscountApprovalStatus.Pending);
    }

    [Fact]
    public async Task GetRequestByIdAsync_ReturnsRequest_WithRelations()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote = CreateTestQuote(account.Id);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var request = CreateTestRequest(quote.Id, user.Id, matrix.Id);
        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetRequestByIdAsync(request.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Quote.Should().NotBeNull();
        result.Submitter.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRequestByNumberAsync_ReturnsRequest_WhenExists()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote = CreateTestQuote(account.Id);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var request = CreateTestRequest(quote.Id, user.Id, matrix.Id);
        request.RequestNumber = "APR-TEST-001";
        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetRequestByNumberAsync("APR-TEST-001");

        // Assert
        result.Should().NotBeNull();
        result!.RequestNumber.Should().Be("APR-TEST-001");
    }

    [Fact]
    public async Task GetPendingApprovalsForUserAsync_ReturnsPendingRequests()
    {
        // Arrange
        var approver = CreateTestUser("approver");
        var submitter = CreateTestUser("submitter");
        _context.Users.AddRange(approver, submitter);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote = CreateTestQuote(account.Id);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var request = CreateTestRequest(quote.Id, submitter.Id, matrix.Id);
        request.Status = DiscountApprovalStatus.Pending;
        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync();

        var step = new ApprovalStep
        {
            ApprovalRequestId = request.Id,
            StepOrder = 1,
            Status = DiscountApprovalStatus.Pending,
            AssignedToId = approver.Id,
            AssignedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.ApprovalSteps.Add(step);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPendingApprovalsForUserAsync(approver.Id);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetRequestsBySubmitterAsync_ReturnsSubmitterRequests()
    {
        // Arrange
        var user1 = CreateTestUser("user1");
        var user2 = CreateTestUser("user2");
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote1 = CreateTestQuote(account.Id);
        var quote2 = CreateTestQuote(account.Id);
        _context.Quotes.AddRange(quote1, quote2);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var request1 = CreateTestRequest(quote1.Id, user1.Id, matrix.Id);
        var request2 = CreateTestRequest(quote2.Id, user2.Id, matrix.Id);
        _context.ApprovalRequests.AddRange(request1, request2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetRequestsBySubmitterAsync(user1.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().SubmitterId.Should().Be(user1.Id);
    }

    #endregion

    #region Workflow Operations Tests

    [Fact]
    public async Task RecallRequestAsync_RecallsRequest_WhenPending()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote = CreateTestQuote(account.Id);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var request = CreateTestRequest(quote.Id, user.Id, matrix.Id);
        request.Status = DiscountApprovalStatus.Pending;
        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RecallRequestAsync(request.Id, user.Id);

        // Assert
        result.Status.Should().Be(DiscountApprovalStatus.Recalled);
        result.CompletedAt.Should().NotBeNull();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectStats()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote1 = CreateTestQuote(account.Id);
        var quote2 = CreateTestQuote(account.Id);
        var quote3 = CreateTestQuote(account.Id);
        _context.Quotes.AddRange(quote1, quote2, quote3);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var request1 = CreateTestRequest(quote1.Id, user.Id, matrix.Id);
        request1.Status = DiscountApprovalStatus.Pending;

        var request2 = CreateTestRequest(quote2.Id, user.Id, matrix.Id);
        request2.Status = DiscountApprovalStatus.Approved;
        request2.CompletedAt = DateTime.UtcNow;
        request2.TimeToApprovalHours = 24;

        var request3 = CreateTestRequest(quote3.Id, user.Id, matrix.Id);
        request3.Status = DiscountApprovalStatus.Rejected;
        request3.CompletedAt = DateTime.UtcNow;

        _context.ApprovalRequests.AddRange(request1, request2, request3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TotalRequests.Should().Be(3);
        result.ApprovedRequests.Should().Be(1);
        result.RejectedRequests.Should().Be(1);
        result.PendingRequests.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_FiltersbyDateRange()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote1 = CreateTestQuote(account.Id);
        var quote2 = CreateTestQuote(account.Id);
        _context.Quotes.AddRange(quote1, quote2);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var oldRequest = CreateTestRequest(quote1.Id, user.Id, matrix.Id);
        oldRequest.SubmittedAt = DateTime.UtcNow.AddDays(-60);

        var recentRequest = CreateTestRequest(quote2.Id, user.Id, matrix.Id);
        recentRequest.SubmittedAt = DateTime.UtcNow.AddDays(-5);

        _context.ApprovalRequests.AddRange(oldRequest, recentRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStatisticsAsync(
            fromDate: DateTime.UtcNow.AddDays(-30),
            toDate: DateTime.UtcNow);

        // Assert
        result.TotalRequests.Should().Be(1); // Only the recent one
    }

    [Fact]
    public async Task GetApproverPerformanceAsync_ReturnsPerformanceMetrics()
    {
        // Arrange
        var approver = CreateTestUser("approver");
        var submitter = CreateTestUser("submitter");
        _context.Users.AddRange(approver, submitter);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote = CreateTestQuote(account.Id);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var request = CreateTestRequest(quote.Id, submitter.Id, matrix.Id);
        request.Status = DiscountApprovalStatus.Approved;
        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync();

        var step = new ApprovalStep
        {
            ApprovalRequestId = request.Id,
            StepOrder = 1,
            Status = DiscountApprovalStatus.Approved,
            AssignedToId = approver.Id,
            ActedById = approver.Id,
            AssignedAt = DateTime.UtcNow.AddHours(-2),
            ActedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.ApprovalSteps.Add(step);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetApproverPerformanceAsync();

        // Assert
        result.Should().NotBeEmpty();
        var approverStats = result.FirstOrDefault(r => r.UserId == approver.Id);
        approverStats.Should().NotBeNull();
        approverStats!.TotalApproved.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetQuoteApprovalHistoryAsync_ReturnsHistory()
    {
        // Arrange
        var user = CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote = CreateTestQuote(account.Id);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        var matrix = CreateTestMatrix();
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var request1 = CreateTestRequest(quote.Id, user.Id, matrix.Id);
        request1.Status = DiscountApprovalStatus.Recalled;
        request1.SubmittedAt = DateTime.UtcNow.AddDays(-10);

        var request2 = CreateTestRequest(quote.Id, user.Id, matrix.Id);
        request2.Status = DiscountApprovalStatus.Approved;
        request2.SubmittedAt = DateTime.UtcNow.AddDays(-5);

        _context.ApprovalRequests.AddRange(request1, request2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetQuoteApprovalHistoryAsync(quote.Id);

        // Assert
        result.Should().HaveCount(2);
        result.First().SubmittedAt.Should().BeAfter(result.Last().SubmittedAt!.Value);
    }

    #endregion

    #region Matrix Selection Tests

    [Fact]
    public async Task RequiresApprovalAsync_ReturnsFalse_WhenNoDiscount()
    {
        // Arrange
        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote = CreateTestQuote(account.Id, discountPercent: 0);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RequiresApprovalAsync(quote.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task FindApplicableMatrixAsync_ReturnsMatchingMatrix()
    {
        // Arrange
        var matrix = CreateTestMatrix();
        matrix.Priority = 1;
        matrix.AppliesToAllProducts = true;
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var level = CreateTestLevel(matrix.Id);
        level.MinValue = 5;
        level.MaxValue = 50;
        _context.ApprovalLevels.Add(level);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote = CreateTestQuote(account.Id, discountPercent: 10);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.FindApplicableMatrixAsync(quote.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(matrix.Id);
    }

    [Fact]
    public async Task FindApplicableMatrixAsync_ReturnsNull_WhenNoMatrixActive()
    {
        // Arrange
        var matrix = CreateTestMatrix(isActive: false);
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync();

        var account = CreateTestAccount();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var quote = CreateTestQuote(account.Id, discountPercent: 10);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.FindApplicableMatrixAsync(quote.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
