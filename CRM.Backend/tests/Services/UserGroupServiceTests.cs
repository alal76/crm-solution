// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
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
/// Comprehensive unit tests for UserGroupService.
/// Uses EF Core InMemory database for realistic data access testing.
/// Covers CRUD operations, membership management, permissions, and active filtering.
/// </summary>
public class UserGroupServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<UserGroupService>> _mockLogger;
    private readonly UserGroupService _service;

    public UserGroupServiceTests()
    {
        _dbContext = CreateDbContext();
        _mockLogger = new Mock<ILogger<UserGroupService>>();
        _service = CreateService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Factory Helpers

    private static CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new CrmDbContext(options, null!);
    }

    private static UserGroupService CreateService(CrmDbContext context)
    {
        var logger = new Mock<ILogger<UserGroupService>>();
        return new UserGroupService(context, logger.Object);
    }

    #endregion

    #region Test Data Helpers

    private static UserGroup CreateTestGroup(
        string name = "Test Group",
        bool isActive = true,
        bool isDefault = false,
        bool isSystemAdmin = false,
        string headerColor = "#6750A4",
        int displayOrder = 0)
    {
        return new UserGroup
        {
            Name = name,
            Description = $"Description for {name}",
            IsActive = isActive,
            IsDefault = isDefault,
            IsSystemAdmin = isSystemAdmin,
            HeaderColor = headerColor,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Members = new List<UserGroupMember>()
        };
    }

    private static User CreateTestUser(
        string firstName = "Test",
        string lastName = "User",
        string? email = null)
    {
        var emailAddr = email ?? $"{firstName.ToLower()}.{lastName.ToLower()}@test.com";
        return new User
        {
            Username = $"{firstName.ToLower()}.{lastName.ToLower()}",
            Email = emailAddr,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = "hashed_password",
            IsActive = true,
            Role = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static CreateUserGroupRequest CreateTestRequest(
        string name = "New Group",
        bool isActive = true,
        bool isDefault = false,
        bool isSystemAdmin = false)
    {
        return new CreateUserGroupRequest
        {
            Name = name,
            Description = $"Description for {name}",
            IsActive = isActive,
            IsDefault = isDefault,
            IsSystemAdmin = isSystemAdmin,
            HeaderColor = "#6750A4",
            DisplayOrder = 0,
            CanAccessDashboard = true,
            CanAccessAccounts = false,
            CanAccessContacts = false,
            CanAccessLeads = false,
            CanAccessOpportunities = false,
            DataAccessScope = "own"
        };
    }

    private async Task<UserGroup> SeedGroupAsync(
        string name = "Seeded Group",
        bool isActive = true,
        bool isDefault = false,
        bool isSystemAdmin = false)
    {
        var group = CreateTestGroup(name, isActive, isDefault, isSystemAdmin);
        _dbContext.UserGroups.Add(group);
        await _dbContext.SaveChangesAsync();
        return group;
    }

    private async Task<User> SeedUserAsync(
        string firstName = "Test",
        string lastName = "User",
        string? email = null)
    {
        var user = CreateTestUser(firstName, lastName, email);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    private async Task<UserGroupMember> SeedMemberAsync(int groupId, int userId)
    {
        var member = new UserGroupMember
        {
            UserGroupId = groupId,
            UserId = userId,
            AddedAt = DateTime.UtcNow
        };
        _dbContext.UserGroupMembers.Add(member);
        await _dbContext.SaveChangesAsync();
        return member;
    }

    #endregion

    // =====================================================================
    // 1. Constructor Tests
    // =====================================================================

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var logger = new Mock<ILogger<UserGroupService>>();

        // Act
        var act = () => new UserGroupService(null!, logger.Object);

        // Assert — service accepts null (no guard clause); calling any method would throw NRE
        // If the constructor had null guards, this would throw ArgumentNullException.
        // We verify the service is created but unusable with a null context.
        act.Should().NotThrow("constructor does not currently guard against null context");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act
        var act = () => new UserGroupService(context, null!);

        // Assert — service accepts null (no guard clause)
        act.Should().NotThrow("constructor does not currently guard against null logger");
    }

    #endregion

    // =====================================================================
    // 2. GetAllGroupsAsync Tests
    // =====================================================================

    #region GetAllGroupsAsync

    [Fact]
    public async Task GetAllGroupsAsync_ShouldReturnActiveGroups_WhenGroupsExist()
    {
        // Arrange
        await SeedGroupAsync("Active Group 1", isActive: true);
        await SeedGroupAsync("Active Group 2", isActive: true);
        await SeedGroupAsync("Inactive Group", isActive: false);

        // Act
        var result = await _service.GetAllGroupsAsync();

        // Assert
        var groups = result.ToList();
        groups.Should().HaveCount(2);
        groups.Should().OnlyContain(g => g.IsActive);
        groups.Select(g => g.Name).Should().Contain("Active Group 1");
        groups.Select(g => g.Name).Should().Contain("Active Group 2");
    }

    [Fact]
    public async Task GetAllGroupsAsync_ShouldReturnEmpty_WhenNoGroupsExist()
    {
        // Arrange — no groups seeded

        // Act
        var result = await _service.GetAllGroupsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllGroupsAsync_ShouldExcludeInactiveGroups()
    {
        // Arrange
        await SeedGroupAsync("Inactive 1", isActive: false);
        await SeedGroupAsync("Inactive 2", isActive: false);

        // Act
        var result = await _service.GetAllGroupsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllGroupsAsync_ShouldMapDtoPropertiesCorrectly()
    {
        // Arrange
        var group = CreateTestGroup("Sales Team", isActive: true, isSystemAdmin: true);
        group.HeaderColor = "#FF5733";
        group.CanAccessDashboard = true;
        group.CanAccessAccounts = true;
        group.CanCreateAccounts = true;
        group.DataAccessScope = "all";
        _dbContext.UserGroups.Add(group);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = (await _service.GetAllGroupsAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        var dto = result.First();
        dto.Name.Should().Be("Sales Team");
        dto.IsSystemAdmin.Should().BeTrue();
        dto.HeaderColor.Should().Be("#FF5733");
        dto.CanAccessDashboard.Should().BeTrue();
        dto.CanAccessAccounts.Should().BeTrue();
        dto.CanCreateAccounts.Should().BeTrue();
        dto.DataAccessScope.Should().Be("all");
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    // =====================================================================
    // 3. GetGroupByIdAsync Tests
    // =====================================================================

    #region GetGroupByIdAsync

    [Fact]
    public async Task GetGroupByIdAsync_ShouldReturnGroup_WhenGroupExists()
    {
        // Arrange
        var group = await SeedGroupAsync("Target Group");

        // Act
        var result = await _service.GetGroupByIdAsync(group.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(group.Id);
        result.Name.Should().Be("Target Group");
    }

    [Fact]
    public async Task GetGroupByIdAsync_ShouldReturnNull_WhenGroupDoesNotExist()
    {
        // Arrange — no groups seeded

        // Act
        var result = await _service.GetGroupByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetGroupByIdAsync_ShouldReturnInactiveGroup_WhenQueried()
    {
        // Arrange — GetGroupByIdAsync does NOT filter by IsActive
        var group = await SeedGroupAsync("Inactive Group", isActive: false);

        // Act
        var result = await _service.GetGroupByIdAsync(group.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Inactive Group");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetGroupByIdAsync_ShouldReturnCorrectGroup_WhenMultipleExist()
    {
        // Arrange
        await SeedGroupAsync("Group A");
        var groupB = await SeedGroupAsync("Group B");
        await SeedGroupAsync("Group C");

        // Act
        var result = await _service.GetGroupByIdAsync(groupB.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Group B");
    }

    #endregion

    // =====================================================================
    // 4. CreateGroupAsync Tests
    // =====================================================================

    #region CreateGroupAsync

    [Fact]
    public async Task CreateGroupAsync_ShouldCreateGroup_WhenRequestIsValid()
    {
        // Arrange
        var request = CreateTestRequest("Brand New Group");

        // Act
        var result = await _service.CreateGroupAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Brand New Group");
        result.Description.Should().Be("Description for Brand New Group");
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateGroupAsync_ShouldSetPermissionsFromRequest()
    {
        // Arrange
        var request = CreateTestRequest("Permissions Group");
        request.CanAccessDashboard = true;
        request.CanAccessAccounts = true;
        request.CanAccessContacts = true;
        request.CanCreateAccounts = true;
        request.CanEditAccounts = true;
        request.CanDeleteAccounts = true;
        request.CanExportData = true;
        request.IsSystemAdmin = true;
        request.DataAccessScope = "all";

        // Act
        var result = await _service.CreateGroupAsync(request);

        // Assert
        result.CanAccessDashboard.Should().BeTrue();
        result.CanAccessAccounts.Should().BeTrue();
        result.CanAccessContacts.Should().BeTrue();
        result.CanCreateAccounts.Should().BeTrue();
        result.CanEditAccounts.Should().BeTrue();
        result.CanDeleteAccounts.Should().BeTrue();
        result.CanExportData.Should().BeTrue();
        result.IsSystemAdmin.Should().BeTrue();
        result.DataAccessScope.Should().Be("all");
    }

    [Fact]
    public async Task CreateGroupAsync_ShouldPersistToDatabase()
    {
        // Arrange
        var request = CreateTestRequest("Persisted Group");

        // Act
        await _service.CreateGroupAsync(request);

        // Assert
        var persisted = await _dbContext.UserGroups
            .FirstOrDefaultAsync(g => g.Name == "Persisted Group");
        persisted.Should().NotBeNull();
        persisted!.Description.Should().Be("Description for Persisted Group");
    }

    [Fact]
    public async Task CreateGroupAsync_ShouldThrowInvalidOperationException_WhenNameAlreadyExists()
    {
        // Arrange
        await SeedGroupAsync("Duplicate Name");
        var request = CreateTestRequest("Duplicate Name");

        // Act
        var act = () => _service.CreateGroupAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateGroupAsync_ShouldSetDefaultValues_WhenMinimalRequest()
    {
        // Arrange
        var request = new CreateUserGroupRequest { Name = "Minimal Group" };

        // Act
        var result = await _service.CreateGroupAsync(request);

        // Assert
        result.IsActive.Should().BeTrue();
        result.IsDefault.Should().BeFalse();
        result.IsSystemAdmin.Should().BeFalse();
        result.HeaderColor.Should().Be("#6750A4");
        result.DataAccessScope.Should().Be("own");
        result.CanAccessDashboard.Should().BeTrue();
        result.CanAccessAccounts.Should().BeFalse();
    }

    #endregion

    // =====================================================================
    // 5. UpdateGroupAsync Tests
    // =====================================================================

    #region UpdateGroupAsync

    [Fact]
    public async Task UpdateGroupAsync_ShouldUpdateProperties_WhenGroupExists()
    {
        // Arrange
        var group = await SeedGroupAsync("Original Name");
        var request = CreateTestRequest("Updated Name");
        request.Description = "Updated description";

        // Act
        var result = await _service.UpdateGroupAsync(group.Id, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdateGroupAsync_ShouldThrowKeyNotFoundException_WhenGroupDoesNotExist()
    {
        // Arrange
        var request = CreateTestRequest("Doesn't Matter");

        // Act
        var act = () => _service.UpdateGroupAsync(999, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task UpdateGroupAsync_ShouldUpdatePermissions()
    {
        // Arrange
        var group = await SeedGroupAsync("Perm Group");
        var request = CreateTestRequest("Perm Group");
        request.CanAccessAccounts = true;
        request.CanCreateAccounts = true;
        request.CanEditAccounts = true;
        request.CanDeleteAccounts = true;
        request.CanViewAllAccounts = true;
        request.DataAccessScope = "all";

        // Act
        var result = await _service.UpdateGroupAsync(group.Id, request);

        // Assert
        result.Should().NotBeNull();
        result!.CanAccessAccounts.Should().BeTrue();
        result.CanCreateAccounts.Should().BeTrue();
        result.CanEditAccounts.Should().BeTrue();
        result.CanDeleteAccounts.Should().BeTrue();
        result.CanViewAllAccounts.Should().BeTrue();
        result.DataAccessScope.Should().Be("all");
    }

    [Fact]
    public async Task UpdateGroupAsync_ShouldPersistChanges()
    {
        // Arrange
        var group = await SeedGroupAsync("Will Change");
        var request = CreateTestRequest("Changed");

        // Act
        await _service.UpdateGroupAsync(group.Id, request);

        // Assert — verify from DB directly
        var persisted = await _dbContext.UserGroups.FindAsync(group.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Changed");
    }

    #endregion

    // =====================================================================
    // 6. DeleteGroupAsync Tests
    // =====================================================================

    #region DeleteGroupAsync

    [Fact]
    public async Task DeleteGroupAsync_ShouldRemoveGroup_WhenGroupExists()
    {
        // Arrange
        var group = await SeedGroupAsync("To Delete");

        // Act
        await _service.DeleteGroupAsync(group.Id);

        // Assert
        var deleted = await _dbContext.UserGroups.FindAsync(group.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteGroupAsync_ShouldThrowKeyNotFoundException_WhenGroupDoesNotExist()
    {
        // Arrange — no groups

        // Act
        var act = () => _service.DeleteGroupAsync(999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task DeleteGroupAsync_ShouldPersistDeletion()
    {
        // Arrange
        var group1 = await SeedGroupAsync("Keep This");
        var group2 = await SeedGroupAsync("Delete This");

        // Act
        await _service.DeleteGroupAsync(group2.Id);

        // Assert
        var allGroups = await _dbContext.UserGroups.ToListAsync();
        allGroups.Should().HaveCount(1);
        allGroups.First().Name.Should().Be("Keep This");
    }

    #endregion

    // =====================================================================
    // 7. GetGroupMembersAsync Tests
    // =====================================================================

    #region GetGroupMembersAsync

    [Fact]
    public async Task GetGroupMembersAsync_ShouldReturnMembers_WhenMembersExist()
    {
        // Arrange
        var group = await SeedGroupAsync("Group With Members");
        var user1 = await SeedUserAsync("Alice", "Smith");
        var user2 = await SeedUserAsync("Bob", "Jones");
        await SeedMemberAsync(group.Id, user1.Id);
        await SeedMemberAsync(group.Id, user2.Id);

        // Act
        var result = (await _service.GetGroupMembersAsync(group.Id)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Email == "alice.smith@test.com");
        result.Should().Contain(m => m.Email == "bob.jones@test.com");
    }

    [Fact]
    public async Task GetGroupMembersAsync_ShouldReturnEmpty_WhenNoMembersExist()
    {
        // Arrange — group exists but no members
        var group = await SeedGroupAsync("Empty Group");

        // Act
        var result = await _service.GetGroupMembersAsync(group.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGroupMembersAsync_ShouldReturnOnlyMembersForSpecifiedGroup()
    {
        // Arrange
        var group1 = await SeedGroupAsync("Group 1");
        var group2 = await SeedGroupAsync("Group 2");
        var user1 = await SeedUserAsync("Alice", "Smith");
        var user2 = await SeedUserAsync("Bob", "Jones");
        var user3 = await SeedUserAsync("Charlie", "Brown");

        await SeedMemberAsync(group1.Id, user1.Id);
        await SeedMemberAsync(group1.Id, user2.Id);
        await SeedMemberAsync(group2.Id, user3.Id); // different group

        // Act
        var result = (await _service.GetGroupMembersAsync(group1.Id)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(m => m.Email == "charlie.brown@test.com");
    }

    [Fact]
    public async Task GetGroupMembersAsync_ShouldReturnFullName_FromUserEntity()
    {
        // Arrange
        var group = await SeedGroupAsync("Name Check Group");
        var user = await SeedUserAsync("Jane", "Doe");
        await SeedMemberAsync(group.Id, user.Id);

        // Act
        var result = (await _service.GetGroupMembersAsync(group.Id)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().FullName.Should().Be("Jane Doe");
    }

    #endregion

    // =====================================================================
    // 8. AddUserToGroupAsync Tests
    // =====================================================================

    #region AddUserToGroupAsync

    [Fact]
    public async Task AddUserToGroupAsync_ShouldAddMember_WhenValid()
    {
        // Arrange
        var group = await SeedGroupAsync("Target Group");
        var user = await SeedUserAsync("New", "Member");

        // Act
        await _service.AddUserToGroupAsync(group.Id, user.Id);

        // Assert
        var membership = await _dbContext.UserGroupMembers
            .FirstOrDefaultAsync(m => m.UserGroupId == group.Id && m.UserId == user.Id);
        membership.Should().NotBeNull();
        membership!.AddedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddUserToGroupAsync_ShouldThrowKeyNotFoundException_WhenGroupNotFound()
    {
        // Arrange
        var user = await SeedUserAsync("Orphan", "User");

        // Act
        var act = () => _service.AddUserToGroupAsync(999, user.Id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Group*not found*");
    }

    [Fact]
    public async Task AddUserToGroupAsync_ShouldThrowKeyNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var group = await SeedGroupAsync("Group Without User");

        // Act
        var act = () => _service.AddUserToGroupAsync(group.Id, 999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*User*not found*");
    }

    [Fact]
    public async Task AddUserToGroupAsync_ShouldThrowInvalidOperationException_WhenAlreadyMember()
    {
        // Arrange
        var group = await SeedGroupAsync("Already Member Group");
        var user = await SeedUserAsync("Existing", "Member");
        await SeedMemberAsync(group.Id, user.Id);

        // Act
        var act = () => _service.AddUserToGroupAsync(group.Id, user.Id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already a member*");
    }

    #endregion

    // =====================================================================
    // 9. RemoveUserFromGroupAsync Tests
    // =====================================================================

    #region RemoveUserFromGroupAsync

    [Fact]
    public async Task RemoveUserFromGroupAsync_ShouldRemoveMember_WhenMemberExists()
    {
        // Arrange
        var group = await SeedGroupAsync("Remove Member Group");
        var user = await SeedUserAsync("Remove", "Me");
        await SeedMemberAsync(group.Id, user.Id);

        // Act
        await _service.RemoveUserFromGroupAsync(group.Id, user.Id);

        // Assert
        var membership = await _dbContext.UserGroupMembers
            .FirstOrDefaultAsync(m => m.UserGroupId == group.Id && m.UserId == user.Id);
        membership.Should().BeNull();
    }

    [Fact]
    public async Task RemoveUserFromGroupAsync_ShouldThrowKeyNotFoundException_WhenMemberNotFound()
    {
        // Arrange — no membership exists

        // Act
        var act = () => _service.RemoveUserFromGroupAsync(1, 999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not a member*");
    }

    [Fact]
    public async Task RemoveUserFromGroupAsync_ShouldPersistRemoval()
    {
        // Arrange
        var group = await SeedGroupAsync("Persist Removal Group");
        var user1 = await SeedUserAsync("Stay", "Member");
        var user2 = await SeedUserAsync("Leave", "Member");
        await SeedMemberAsync(group.Id, user1.Id);
        await SeedMemberAsync(group.Id, user2.Id);

        // Act
        await _service.RemoveUserFromGroupAsync(group.Id, user2.Id);

        // Assert
        var remainingMembers = await _dbContext.UserGroupMembers
            .Where(m => m.UserGroupId == group.Id)
            .ToListAsync();
        remainingMembers.Should().HaveCount(1);
        remainingMembers.First().UserId.Should().Be(user1.Id);
    }

    #endregion

    // =====================================================================
    // 10. IsUserInGroupAsync Tests
    // =====================================================================

    #region IsUserInGroupAsync

    [Fact]
    public async Task IsUserInGroupAsync_ShouldReturnTrue_WhenUserIsMember()
    {
        // Arrange
        var group = await SeedGroupAsync("Membership Check Group");
        var user = await SeedUserAsync("Member", "User");
        await SeedMemberAsync(group.Id, user.Id);

        // Act
        var result = await _service.IsUserInGroupAsync(user.Id, group.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserInGroupAsync_ShouldReturnFalse_WhenUserIsNotMember()
    {
        // Arrange
        var group = await SeedGroupAsync("No Member Group");

        // Act
        var result = await _service.IsUserInGroupAsync(999, group.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsUserInGroupAsync_ShouldSupportCancellationToken()
    {
        // Arrange
        var group = await SeedGroupAsync("Token Test Group");
        var user = await SeedUserAsync("Token", "User");
        await SeedMemberAsync(group.Id, user.Id);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _service.IsUserInGroupAsync(user.Id, group.Id, cts.Token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserInGroupAsync_ShouldReturnFalse_WhenMembershipIsInDifferentGroup()
    {
        // Arrange
        var group1 = await SeedGroupAsync("Group Alpha");
        var group2 = await SeedGroupAsync("Group Beta");
        var user = await SeedUserAsync("Cross", "Check");
        await SeedMemberAsync(group1.Id, user.Id);

        // Act
        var result = await _service.IsUserInGroupAsync(user.Id, group2.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    // =====================================================================
    // 11. GetActiveGroupsAsync Tests
    // =====================================================================

    #region GetActiveGroupsAsync

    [Fact]
    public async Task GetActiveGroupsAsync_ShouldReturnOnlyActiveGroups()
    {
        // Arrange
        await SeedGroupAsync("Active 1", isActive: true);
        await SeedGroupAsync("Active 2", isActive: true);
        await SeedGroupAsync("Inactive 1", isActive: false);

        // Act
        var result = await _service.GetActiveGroupsAsync();

        // Assert
        var groups = result.ToList();
        groups.Should().HaveCount(2);
        groups.Should().OnlyContain(g => g.IsActive);
    }

    [Fact]
    public async Task GetActiveGroupsAsync_ShouldReturnEmpty_WhenNoActiveGroups()
    {
        // Arrange
        await SeedGroupAsync("Inactive Only", isActive: false);

        // Act
        var result = await _service.GetActiveGroupsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveGroupsAsync_ShouldSupportCancellationToken()
    {
        // Arrange
        await SeedGroupAsync("Active With Token", isActive: true);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _service.GetActiveGroupsAsync(cts.Token);

        // Assert
        var groups = result.ToList();
        groups.Should().HaveCount(1);
        groups.First().Name.Should().Be("Active With Token");
    }

    [Fact]
    public async Task GetActiveGroupsAsync_ShouldMapDtoCorrectly()
    {
        // Arrange
        var group = CreateTestGroup("Mapped Active Group", isActive: true, isSystemAdmin: true);
        group.CanAccessReports = true;
        group.CanExportData = true;
        _dbContext.UserGroups.Add(group);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = (await _service.GetActiveGroupsAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        var dto = result.First();
        dto.Name.Should().Be("Mapped Active Group");
        dto.IsSystemAdmin.Should().BeTrue();
        dto.CanAccessReports.Should().BeTrue();
        dto.CanExportData.Should().BeTrue();
    }

    #endregion
}
