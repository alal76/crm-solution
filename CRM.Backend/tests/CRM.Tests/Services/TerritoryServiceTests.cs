// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Licensed under the GNU Affero General Public License v3.0

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
/// Unit tests for TerritoryService.
/// </summary>
public class TerritoryServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly ITerritoryService _service;
    private readonly Mock<ILogger<TerritoryService>> _loggerMock;

    public TerritoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"TerritoryServiceTestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _loggerMock = new Mock<ILogger<TerritoryService>>();
        _service = new TerritoryService(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Territory CRUD Tests

    [Fact]
    public async Task GetAllTerritoriesAsync_ReturnsAllTerritories()
    {
        // Arrange
        var territories = new List<AccountTerritory>
        {
            new AccountTerritory { TerritoryName = "East Coast", TerritoryCode = "EC", IsActive = true },
            new AccountTerritory { TerritoryName = "West Coast", TerritoryCode = "WC", IsActive = true },
            new AccountTerritory { TerritoryName = "Inactive", TerritoryCode = "IN", IsActive = false }
        };
        _dbContext.AccountTerritories.AddRange(territories);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllTerritoriesAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllTerritoriesAsync_FiltersByIsActive()
    {
        // Arrange
        var territories = new List<AccountTerritory>
        {
            new AccountTerritory { TerritoryName = "Active1", IsActive = true },
            new AccountTerritory { TerritoryName = "Active2", IsActive = true },
            new AccountTerritory { TerritoryName = "Inactive", IsActive = false }
        };
        _dbContext.AccountTerritories.AddRange(territories);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllTerritoriesAsync(isActive: true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.IsActive);
    }

    [Fact]
    public async Task GetAllTerritoriesAsync_FiltersByOwnerId()
    {
        // Arrange
        var territories = new List<AccountTerritory>
        {
            new AccountTerritory { TerritoryName = "Owner1", PrimaryOwnerId = 1, IsActive = true },
            new AccountTerritory { TerritoryName = "Owner2", PrimaryOwnerId = 2, IsActive = true },
            new AccountTerritory { TerritoryName = "NoOwner", PrimaryOwnerId = null, IsActive = true }
        };
        _dbContext.AccountTerritories.AddRange(territories);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllTerritoriesAsync(ownerId: 1);

        // Assert
        result.Should().HaveCount(1);
        result.First().TerritoryName.Should().Be("Owner1");
    }

    [Fact]
    public async Task GetTerritoryByIdAsync_ReturnsTerritory_WhenExists()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", TerritoryCode = "TST", IsActive = true };
        _dbContext.AccountTerritories.Add(territory);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetTerritoryByIdAsync(territory.Id);

        // Assert
        result.Should().NotBeNull();
        result!.TerritoryName.Should().Be("Test");
    }

    [Fact]
    public async Task GetTerritoryByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetTerritoryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTerritoryByCodeAsync_ReturnsTerritory_WhenExists()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", TerritoryCode = "UNIQUE", IsActive = true };
        _dbContext.AccountTerritories.Add(territory);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetTerritoryByCodeAsync("UNIQUE");

        // Assert
        result.Should().NotBeNull();
        result!.TerritoryCode.Should().Be("UNIQUE");
    }

    [Fact]
    public async Task CreateTerritoryAsync_CreatesTerritory()
    {
        // Arrange
        var territory = new AccountTerritory
        {
            TerritoryName = "New Territory",
            TerritoryCode = "NEW",
            Description = "A new territory",
            IsActive = true
        };

        // Act
        var result = await _service.CreateTerritoryAsync(territory);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.TerritoryName.Should().Be("New Territory");

        // Verify in database
        var dbTerritory = await _dbContext.AccountTerritories.FindAsync(result.Id);
        dbTerritory.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateTerritoryAsync_UpdatesTerritory()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Original", TerritoryCode = "ORG", IsActive = true };
        _dbContext.AccountTerritories.Add(territory);
        await _dbContext.SaveChangesAsync();

        // Act
        territory.TerritoryName = "Updated";
        territory.Description = "Updated description";
        var result = await _service.UpdateTerritoryAsync(territory);

        // Assert
        result.TerritoryName.Should().Be("Updated");
        result.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task DeleteTerritoryAsync_SoftDeletesTerritory()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "ToDelete", TerritoryCode = "DEL", IsActive = true };
        _dbContext.AccountTerritories.Add(territory);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeleteTerritoryAsync(territory.Id);

        // Assert
        result.Should().BeTrue();
        var deletedTerritory = await _dbContext.AccountTerritories.FindAsync(territory.Id);
        deletedTerritory!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateTerritoryAsync_ActivatesTerritory()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Inactive", TerritoryCode = "INA", IsActive = false };
        _dbContext.AccountTerritories.Add(territory);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ActivateTerritoryAsync(territory.Id);

        // Assert
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateTerritoryAsync_DeactivatesTerritory()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Active", TerritoryCode = "ACT", IsActive = true };
        _dbContext.AccountTerritories.Add(territory);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeactivateTerritoryAsync(territory.Id);

        // Assert
        result.IsActive.Should().BeFalse();
    }

    #endregion

    #region Territory Assignment Tests

    [Fact]
    public async Task AssignAccountAsync_CreatesAssignment()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", IsActive = true };
        var account = new Account { Company = "Test Company", Email = "test@test.com" };
        _dbContext.AccountTerritories.Add(territory);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.AssignAccountAsync(account.Id, territory.Id, assignedById: 1, isPrimary: true);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(account.Id);
        result.TerritoryId.Should().Be(territory.Id);
        result.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task UnassignAccountAsync_RemovesAssignment()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", IsActive = true };
        var account = new Account { Company = "Test Company", Email = "test@test.com" };
        _dbContext.AccountTerritories.Add(territory);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        await _service.AssignAccountAsync(account.Id, territory.Id);

        // Act
        var result = await _service.UnassignAccountAsync(account.Id, territory.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAccountAssignmentsAsync_ReturnsAssignments()
    {
        // Arrange
        var territory1 = new AccountTerritory { TerritoryName = "Territory1", IsActive = true };
        var territory2 = new AccountTerritory { TerritoryName = "Territory2", IsActive = true };
        var account = new Account { Company = "Test Company", Email = "test@test.com" };
        _dbContext.AccountTerritories.AddRange(territory1, territory2);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        await _service.AssignAccountAsync(account.Id, territory1.Id, isPrimary: true);
        await _service.AssignAccountAsync(account.Id, territory2.Id, isPrimary: false);

        // Act
        var result = await _service.GetAccountAssignmentsAsync(account.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTerritoryAccountsAsync_ReturnsAccounts()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", IsActive = true };
        var account1 = new Account { Company = "Company1", Email = "test1@test.com" };
        var account2 = new Account { Company = "Company2", Email = "test2@test.com" };
        _dbContext.AccountTerritories.Add(territory);
        _dbContext.Accounts.AddRange(account1, account2);
        await _dbContext.SaveChangesAsync();

        await _service.AssignAccountAsync(account1.Id, territory.Id);
        await _service.AssignAccountAsync(account2.Id, territory.Id);

        // Act
        var result = await _service.GetTerritoryAccountsAsync(territory.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SetPrimaryTerritoryAsync_SetsPrimary()
    {
        // Arrange
        var territory1 = new AccountTerritory { TerritoryName = "Territory1", IsActive = true };
        var territory2 = new AccountTerritory { TerritoryName = "Territory2", IsActive = true };
        var account = new Account { Company = "Test Company", Email = "test@test.com" };
        _dbContext.AccountTerritories.AddRange(territory1, territory2);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        await _service.AssignAccountAsync(account.Id, territory1.Id, isPrimary: true);
        await _service.AssignAccountAsync(account.Id, territory2.Id, isPrimary: false);

        // Act
        var result = await _service.SetPrimaryTerritoryAsync(account.Id, territory2.Id);

        // Assert
        result.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task BulkAssignAccountsAsync_AssignsMultipleAccounts()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", IsActive = true };
        var accounts = new List<Account>
        {
            new Account { Company = "Company1", Email = "test1@test.com" },
            new Account { Company = "Company2", Email = "test2@test.com" },
            new Account { Company = "Company3", Email = "test3@test.com" }
        };
        _dbContext.AccountTerritories.Add(territory);
        _dbContext.Accounts.AddRange(accounts);
        await _dbContext.SaveChangesAsync();

        var accountIds = accounts.Select(a => a.Id).ToList();

        // Act
        var result = await _service.BulkAssignAccountsAsync(accountIds, territory.Id);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task TransferAccountsAsync_TransfersAccounts()
    {
        // Arrange
        var fromTerritory = new AccountTerritory { TerritoryName = "From", IsActive = true };
        var toTerritory = new AccountTerritory { TerritoryName = "To", IsActive = true };
        var account = new Account { Company = "Test Company", Email = "test@test.com" };
        _dbContext.AccountTerritories.AddRange(fromTerritory, toTerritory);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        await _service.AssignAccountAsync(account.Id, fromTerritory.Id);

        // Act
        var result = await _service.TransferAccountsAsync(fromTerritory.Id, toTerritory.Id);

        // Assert
        result.Should().Be(1);
    }

    #endregion

    #region Territory Ownership Tests

    [Fact]
    public async Task SetTerritoryOwnerAsync_SetsOwner()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", IsActive = true };
        var user = new User { Username = "owner", Email = "owner@test.com", FirstName = "Owner", LastName = "User", PasswordHash = "hash" };
        _dbContext.AccountTerritories.Add(territory);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.SetTerritoryOwnerAsync(territory.Id, user.Id);

        // Assert
        result.PrimaryOwnerId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetUserTerritoriesAsync_ReturnsTerritories()
    {
        // Arrange
        var user = new User { Username = "owner", Email = "owner@test.com", FirstName = "Owner", LastName = "User", PasswordHash = "hash" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var territories = new List<AccountTerritory>
        {
            new AccountTerritory { TerritoryName = "Territory1", PrimaryOwnerId = user.Id, IsActive = true },
            new AccountTerritory { TerritoryName = "Territory2", PrimaryOwnerId = user.Id, IsActive = true },
            new AccountTerritory { TerritoryName = "OtherTerritory", PrimaryOwnerId = 999, IsActive = true }
        };
        _dbContext.AccountTerritories.AddRange(territories);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetUserTerritoriesAsync(user.Id);

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task LinkToTeamAsync_LinksTeam()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", IsActive = true };
        var team = new Team { Name = "Sales Team" };
        _dbContext.AccountTerritories.Add(territory);
        _dbContext.Teams.Add(team);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.LinkToTeamAsync(territory.Id, team.Id);

        // Assert
        result.TeamId.Should().Be(team.Id);
    }

    #endregion

    #region Quota Management Tests

    [Fact]
    public async Task SetQuotaAsync_SetsQuota()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", IsActive = true };
        _dbContext.AccountTerritories.Add(territory);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.SetQuotaAsync(territory.Id, 1000000m, "USD");

        // Assert
        result.AnnualQuota.Should().Be(1000000m);
        result.QuotaCurrency.Should().Be("USD");
    }

    [Fact]
    public async Task GetQuotaStatusAsync_ReturnsStatus()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", AnnualQuota = 500000m, IsActive = true };
        _dbContext.AccountTerritories.Add(territory);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetQuotaStatusAsync(territory.Id);

        // Assert
        result.Should().NotBeNull();
        result.TerritoryId.Should().Be(territory.Id);
        result.Quota.Should().Be(500000m);
    }

    [Fact]
    public async Task GetAllQuotaStatusesAsync_ReturnsAllStatuses()
    {
        // Arrange
        var territories = new List<AccountTerritory>
        {
            new AccountTerritory { TerritoryName = "T1", AnnualQuota = 100000m, IsActive = true },
            new AccountTerritory { TerritoryName = "T2", AnnualQuota = 200000m, IsActive = true }
        };
        _dbContext.AccountTerritories.AddRange(territories);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllQuotaStatusesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTerritoryStatisticsAsync_ReturnsStatistics()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", IsActive = true };
        _dbContext.AccountTerritories.Add(territory);
        await _dbContext.SaveChangesAsync();

        var account = new Account { Company = "Test Company", Email = "test@test.com" };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        await _service.AssignAccountAsync(account.Id, territory.Id);

        // Act
        var result = await _service.GetTerritoryStatisticsAsync(territory.Id);

        // Assert
        result.Should().NotBeNull();
        result.TerritoryId.Should().Be(territory.Id);
        result.TotalAccounts.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetTerritoryRankingsAsync_ReturnsRankings()
    {
        // Arrange
        var territories = new List<AccountTerritory>
        {
            new AccountTerritory { TerritoryName = "T1", IsActive = true },
            new AccountTerritory { TerritoryName = "T2", IsActive = true }
        };
        _dbContext.AccountTerritories.AddRange(territories);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetTerritoryRankingsAsync(topN: 5);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAccountDistributionAsync_ReturnsDistribution()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", IsActive = true };
        _dbContext.AccountTerritories.Add(territory);

        var account = new Account { Company = "Test Company", Email = "test@test.com" };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        await _service.AssignAccountAsync(account.Id, territory.Id);

        // Act
        var result = await _service.GetAccountDistributionAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalAccounts.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Territory Matching Tests

    [Fact]
    public async Task FindMatchingTerritoriesAsync_FindsMatchingTerritories()
    {
        // Arrange
        var territory = new AccountTerritory
        {
            TerritoryName = "US East",
            Countries = "[\"US\"]",
            Regions = "[\"East\"]",
            IsActive = true
        };
        _dbContext.AccountTerritories.Add(territory);
        await _dbContext.SaveChangesAsync();

        var criteria = new TerritoryMatchCriteria
        {
            Country = "US",
            Region = "East"
        };

        // Act
        var result = await _service.FindMatchingTerritoriesAsync(criteria);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task IsAccountInTerritoryAsync_ReturnsTrue_WhenAssigned()
    {
        // Arrange
        var territory = new AccountTerritory { TerritoryName = "Test", IsActive = true };
        var account = new Account { Company = "Test Company", Email = "test@test.com" };
        _dbContext.AccountTerritories.Add(territory);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        await _service.AssignAccountAsync(account.Id, territory.Id);

        // Act
        var result = await _service.IsAccountInTerritoryAsync(account.Id, territory.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAccountInTerritoryAsync_ReturnsFalse_WhenCriteriaDoesNotMatch()
    {
        // Arrange - Create territory with specific criteria that won't match the account
        var territory = new AccountTerritory
        {
            TerritoryName = "Test",
            IsActive = true,
            Countries = "[\"Germany\"]",  // Territory is for Germany only
            Industries = "[\"Manufacturing\"]"  // And Manufacturing industry only
        };
        var account = new Account
        {
            Company = "Test Company",
            Email = "test@test.com",
            Country = "USA",  // Account is in USA, not Germany
            Industry = "Technology"  // And Technology, not Manufacturing
        };
        _dbContext.AccountTerritories.Add(territory);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.IsAccountInTerritoryAsync(account.Id, territory.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Territory Search Tests

    [Fact]
    public async Task SearchTerritoriesAsync_FindsTerritories()
    {
        // Arrange
        var territories = new List<AccountTerritory>
        {
            new AccountTerritory { TerritoryName = "East Coast Sales", TerritoryCode = "ECS", IsActive = true },
            new AccountTerritory { TerritoryName = "West Coast Sales", TerritoryCode = "WCS", IsActive = true },
            new AccountTerritory { TerritoryName = "International", TerritoryCode = "INT", IsActive = true }
        };
        _dbContext.AccountTerritories.AddRange(territories);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.SearchTerritoriesAsync("Coast");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTerritoriesByLocationAsync_FiltersByCountry()
    {
        // Arrange
        var territories = new List<AccountTerritory>
        {
            new AccountTerritory { TerritoryName = "US Territory", Countries = "[\"US\"]", IsActive = true },
            new AccountTerritory { TerritoryName = "UK Territory", Countries = "[\"UK\"]", IsActive = true }
        };
        _dbContext.AccountTerritories.AddRange(territories);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetTerritoriesByLocationAsync(country: "US");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
