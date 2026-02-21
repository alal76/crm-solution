// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
/// Unit tests for TeamService.
/// </summary>
public class TeamServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<TeamService>> _mockLogger;
    private readonly TeamService _service;

    private readonly List<Team> _teams;
    private readonly List<TeamMember> _teamMembers;
    private readonly List<AccountTerritory> _territories;
    private readonly List<Account> _customers;
    private readonly List<Opportunity> _opportunities;
    private readonly List<User> _users;

    public TeamServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<TeamService>>();

        _teams = new List<Team>();
        _teamMembers = new List<TeamMember>();
        _territories = new List<AccountTerritory>();
        _customers = new List<Account>();
        _opportunities = new List<Opportunity>();
        _users = new List<User>();

        var mockTeams = MockDbSetFactory.CreateMockDbSet(_teams);
        var mockTeamMembers = MockDbSetFactory.CreateMockDbSet(_teamMembers);
        var mockTerritories = MockDbSetFactory.CreateMockDbSet(_territories);
        var mockCustomers = MockDbSetFactory.CreateMockDbSet(_customers);
        var mockOpportunities = MockDbSetFactory.CreateMockDbSet(_opportunities);
        var mockUsers = MockDbSetFactory.CreateMockDbSet(_users);

        // Add FindAsync(object[], CancellationToken) overload - MockDbSetFactory only sets up FindAsync(object[])
        mockTeams.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                if (id == null)
                    return ValueTask.FromResult<Team?>(default);
                return ValueTask.FromResult(_teams.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });

        _mockContext.Setup(c => c.Teams).Returns(mockTeams.Object);
        _mockContext.Setup(c => c.TeamMembers).Returns(mockTeamMembers.Object);
        _mockContext.Setup(c => c.AccountTerritories).Returns(mockTerritories.Object);
        _mockContext.Setup(c => c.Customers).Returns(mockCustomers.Object);
        _mockContext.Setup(c => c.Opportunities).Returns(mockOpportunities.Object);
        _mockContext.Setup(c => c.Users).Returns(mockUsers.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new TeamService(_mockContext.Object, _mockLogger.Object);
    }

    // ========================================================================
    // GetAllAsync
    // ========================================================================
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedTeams()
    {
        // Arrange
        _teams.AddRange(new[]
        {
            new Team { Id = 1, Name = "Alpha", IsActive = true, IsDeleted = false },
            new Team { Id = 2, Name = "Beta", IsActive = true, IsDeleted = false },
            new Team { Id = 3, Name = "Deleted", IsActive = true, IsDeleted = true }
        });

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByIsActive()
    {
        // Arrange
        _teams.AddRange(new[]
        {
            new Team { Id = 1, Name = "Active1", IsActive = true, IsDeleted = false },
            new Team { Id = 2, Name = "Inactive", IsActive = false, IsDeleted = false },
            new Team { Id = 3, Name = "Active2", IsActive = true, IsDeleted = false }
        });

        // Act
        var result = await _service.GetAllAsync(isActive: true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.IsActive);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByManagerId()
    {
        // Arrange
        _teams.AddRange(new[]
        {
            new Team { Id = 1, Name = "Team1", ManagerId = 10, IsDeleted = false },
            new Team { Id = 2, Name = "Team2", ManagerId = 20, IsDeleted = false },
            new Team { Id = 3, Name = "Team3", ManagerId = 10, IsDeleted = false }
        });

        // Act
        var result = await _service.GetAllAsync(managerId: 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.ManagerId == 10);
    }

    // ========================================================================
    // GetByIdAsync / GetByNameAsync
    // ========================================================================
    [Fact]
    public async Task GetByIdAsync_ShouldReturnTeam_WhenExists()
    {
        // Arrange
        _teams.Add(new Team
        {
            Id = 1,
            Name = "Sales Team",
            IsDeleted = false,
            Members = new List<TeamMember>()
        });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Sales Team");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnTeamByName()
    {
        // Arrange
        _teams.Add(new Team { Id = 1, Name = "Engineering", IsDeleted = false });

        // Act
        var result = await _service.GetByNameAsync("Engineering");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Engineering");
    }

    // ========================================================================
    // CreateAsync / DeleteAsync
    // ========================================================================
    [Fact]
    public async Task CreateAsync_ShouldSetIsActiveTrue()
    {
        // Arrange
        var team = new Team { Name = "New Team" };

        // Act
        var result = await _service.CreateAsync(team);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _teams.Should().Contain(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        // Arrange
        _teams.Add(new Team { Id = 1, Name = "ToDelete", IsDeleted = false });

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _teams.First(t => t.Id == 1).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // AddMemberAsync / RemoveMemberAsync
    // ========================================================================
    [Fact]
    public async Task AddMemberAsync_ShouldCreateTeamMember()
    {
        // Arrange
        _teams.Add(new Team { Id = 1, Name = "Team A", IsDeleted = false });

        // Act
        var result = await _service.AddMemberAsync(1, userId: 5);

        // Assert
        result.Should().NotBeNull();
        result.TeamId.Should().Be(1);
        result.UserId.Should().Be(5);
        _teamMembers.Should().ContainSingle();
    }

    [Fact]
    public async Task AddMemberAsync_ShouldThrow_WhenAlreadyMember()
    {
        // Arrange
        _teams.Add(new Team { Id = 1, Name = "Team A", IsDeleted = false });
        _teamMembers.Add(new TeamMember { Id = 1, TeamId = 1, UserId = 5, IsDeleted = false });

        // Act
        var act = async () => await _service.AddMemberAsync(1, userId: 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already a member*");
    }

    [Fact]
    public async Task RemoveMemberAsync_ShouldSoftDeleteMember()
    {
        // Arrange
        _teamMembers.Add(new TeamMember { Id = 1, TeamId = 1, UserId = 5, IsDeleted = false });

        // Act
        var result = await _service.RemoveMemberAsync(1, 5);

        // Assert
        result.Should().BeTrue();
        _teamMembers.First().IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveMemberAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Act
        var result = await _service.RemoveMemberAsync(1, 999);

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // IsMemberAsync / GetTeamsForUserAsync
    // ========================================================================
    [Fact]
    public async Task IsMemberAsync_ShouldReturnTrue_WhenUserIsMember()
    {
        // Arrange
        _teamMembers.Add(new TeamMember { Id = 1, TeamId = 1, UserId = 5, IsDeleted = false });

        // Act
        var result = await _service.IsMemberAsync(1, 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsMemberAsync_ShouldReturnFalse_WhenUserIsNotMember()
    {
        // Act
        var result = await _service.IsMemberAsync(1, 5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetTeamsForUserAsync_ShouldReturnUserTeams()
    {
        // Arrange
        _teams.AddRange(new[]
        {
            new Team { Id = 1, Name = "Team A", IsDeleted = false },
            new Team { Id = 2, Name = "Team B", IsDeleted = false }
        });
        _teamMembers.AddRange(new[]
        {
            new TeamMember { Id = 1, TeamId = 1, UserId = 5, IsDeleted = false },
            new TeamMember { Id = 2, TeamId = 2, UserId = 5, IsDeleted = false }
        });

        // Act
        var result = await _service.GetTeamsForUserAsync(5);

        // Assert
        result.Should().HaveCount(2);
    }

    // ========================================================================
    // SetManagerAsync / Hierarchy
    // ========================================================================
    [Fact]
    public async Task SetManagerAsync_ShouldSetManagerId()
    {
        // Arrange
        _teams.Add(new Team { Id = 1, Name = "Team A", IsDeleted = false });

        // Act
        var result = await _service.SetManagerAsync(1, managerId: 42);

        // Assert
        result.Should().NotBeNull();
        result!.ManagerId.Should().Be(42);
    }

    [Fact]
    public async Task GetChildTeamsAsync_ShouldReturnChildTeams()
    {
        // Arrange
        _teams.AddRange(new[]
        {
            new Team { Id = 1, Name = "Parent", ParentTeamId = null, IsDeleted = false },
            new Team { Id = 2, Name = "Child1", ParentTeamId = 1, IsDeleted = false },
            new Team { Id = 3, Name = "Child2", ParentTeamId = 1, IsDeleted = false },
            new Team { Id = 4, Name = "Other", ParentTeamId = null, IsDeleted = false }
        });

        // Act
        var result = await _service.GetChildTeamsAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.ParentTeamId == 1);
    }

    [Fact]
    public async Task SetParentTeamAsync_ShouldPreventCircularReference()
    {
        // Arrange: Team2 is a child of Team1 — setting Team1's parent to Team2 is circular
        _teams.AddRange(new[]
        {
            new Team { Id = 1, Name = "Team1", ParentTeamId = null, IsDeleted = false },
            new Team { Id = 2, Name = "Team2", ParentTeamId = 1, IsDeleted = false }
        });

        // Act
        var act = async () => await _service.SetParentTeamAsync(1, parentTeamId: 2);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ircular*");
    }
}
