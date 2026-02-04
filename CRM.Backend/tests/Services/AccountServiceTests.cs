// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Customer Entity Unit Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for Customer entity functionality
/// </summary>
public class CustomerServiceTests
{
    #region Create Customer Tests

    [Fact]
    public void CreateCustomer_ValidIndividual_CreatesCorrectly()
    {
        // Arrange & Act
        var account = new Account
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Category = AccountCategory.Individual
        };

        // Assert
        account.Should().NotBeNull();
        account.Category.Should().Be(AccountCategory.Individual);
        account.FirstName.Should().Be("John");
    }

    [Fact]
    public void CreateCustomer_ValidOrganization_CreatesCorrectly()
    {
        // Arrange & Act
        var account = new Account
        {
            Category = AccountCategory.Organization,
            Company = "Acme Corporation",
            Industry = "Technology",
            Email = "info@acme.com"
        };

        // Assert
        account.Should().NotBeNull();
        account.Category.Should().Be(AccountCategory.Organization);
        account.Company.Should().Be("Acme Corporation");
    }

    [Fact]
    public void CreateCustomer_WithAllFields_SetsCorrectly()
    {
        // Arrange & Act
        var account = new Account
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-1234",
            Category = AccountCategory.Individual,
            AccountType = AccountType.Enterprise,
            Priority = AccountPriority.High,
            LifecycleStage = AccountLifecycleStage.Active,
            Website = "https://example.com",
            Address = "123 Main St",
            City = "New York",
            Country = "USA",
            ZipCode = "10001"
        };

        // Assert
        account.FirstName.Should().Be("John");
        account.AccountType.Should().Be(AccountType.Enterprise);
        account.Priority.Should().Be(AccountPriority.High);
        account.Website.Should().Be("https://example.com");
    }

    #endregion

    #region Update Customer Tests

    [Fact]
    public void UpdateCustomer_ChangeCategory_UpdatesCorrectly()
    {
        // Arrange
        var account = new Account
        {
            Category = AccountCategory.Individual,
            FirstName = "John"
        };

        // Act
        account.Category = AccountCategory.Organization;
        account.Company = "John Doe Inc";

        // Assert
        account.Category.Should().Be(AccountCategory.Organization);
        account.Company.Should().Be("John Doe Inc");
    }

    [Fact]
    public void UpdateCustomer_ChangeLifecycleStage_UpdatesCorrectly()
    {
        // Arrange
        var account = new Account
        {
            LifecycleStage = AccountLifecycleStage.Lead
        };

        // Act
        account.LifecycleStage = AccountLifecycleStage.Opportunity;

        // Assert
        account.LifecycleStage.Should().Be(AccountLifecycleStage.Opportunity);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void SoftDeleteCustomer_SetsIsDeletedFlag()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            IsDeleted = false
        };

        // Act
        account.IsDeleted = true;

        // Assert
        account.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Search and Filter Tests

    [Fact]
    public void SearchCustomers_ByName_ReturnsMatching()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe" },
            new() { Id = 2, FirstName = "Jane", LastName = "Doe" },
            new() { Id = 3, FirstName = "Bob", LastName = "Smith" }
        };

        // Act
        var searchResult = accounts.Where(c =>
            c.LastName?.Contains("Doe", StringComparison.OrdinalIgnoreCase) ?? false);

        // Assert
        searchResult.Should().HaveCount(2);
    }

    [Fact]
    public void FilterCustomers_ByCategory_ReturnsMatching()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, Category = AccountCategory.Individual },
            new() { Id = 2, Category = AccountCategory.Organization },
            new() { Id = 3, Category = AccountCategory.Individual }
        };

        // Act
        var individuals = accounts.Where(c => c.Category == AccountCategory.Individual);

        // Assert
        individuals.Should().HaveCount(2);
    }

    [Fact]
    public void FilterCustomers_ByLifecycleStage_ReturnsMatching()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, LifecycleStage = AccountLifecycleStage.Lead },
            new() { Id = 2, LifecycleStage = AccountLifecycleStage.Active },
            new() { Id = 3, LifecycleStage = AccountLifecycleStage.Active }
        };

        // Act
        var activeCustomers = accounts.Where(c =>
            c.LifecycleStage == AccountLifecycleStage.Active);

        // Assert
        activeCustomers.Should().HaveCount(2);
    }

    [Fact]
    public void FilterCustomers_ByIndustry_ReturnsMatching()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, Industry = "Technology" },
            new() { Id = 2, Industry = "Healthcare" },
            new() { Id = 3, Industry = "Technology" }
        };

        // Act
        var techCustomers = accounts.Where(c => c.Industry == "Technology");

        // Assert
        techCustomers.Should().HaveCount(2);
    }

    [Fact]
    public void FilterCustomers_ByPriority_ReturnsMatching()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, Priority = AccountPriority.High },
            new() { Id = 2, Priority = AccountPriority.Medium },
            new() { Id = 3, Priority = AccountPriority.High }
        };

        // Act
        var highPriorityCustomers = accounts.Where(c =>
            c.Priority == AccountPriority.High);

        // Assert
        highPriorityCustomers.Should().HaveCount(2);
    }

    #endregion

    #region Customer Type Tests

    [Theory]
    [InlineData(AccountType.Individual)]
    [InlineData(AccountType.SmallBusiness)]
    [InlineData(AccountType.MidMarket)]
    [InlineData(AccountType.Enterprise)]
    [InlineData(AccountType.Government)]
    [InlineData(AccountType.NonProfit)]
    public void AccountType_AllTypesValid(AccountType type)
    {
        // Arrange
        var account = new Account();

        // Act
        account.AccountType = type;

        // Assert
        account.AccountType.Should().Be(type);
    }

    #endregion

    #region Customer Priority Tests

    [Theory]
    [InlineData(AccountPriority.Low)]
    [InlineData(AccountPriority.Medium)]
    [InlineData(AccountPriority.High)]
    [InlineData(AccountPriority.Critical)]
    public void AccountPriority_AllPrioritiesValid(AccountPriority priority)
    {
        // Arrange
        var account = new Account();

        // Act
        account.Priority = priority;

        // Assert
        account.Priority.Should().Be(priority);
    }

    #endregion

    #region Lifecycle Stage Tests

    [Theory]
    [InlineData(AccountLifecycleStage.Other)]
    [InlineData(AccountLifecycleStage.Lead)]
    [InlineData(AccountLifecycleStage.Opportunity)]
    [InlineData(AccountLifecycleStage.Active)]
    [InlineData(AccountLifecycleStage.AtRisk)]
    [InlineData(AccountLifecycleStage.Churned)]
    [InlineData(AccountLifecycleStage.WinBack)]
    public void AccountLifecycleStage_AllStagesValid(AccountLifecycleStage stage)
    {
        // Arrange
        var account = new Account();

        // Act
        account.LifecycleStage = stage;

        // Assert
        account.LifecycleStage.Should().Be(stage);
    }

    #endregion

    #region Edge Cases and Validation

    [Fact]
    public void Customer_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var account = new Account();

        // Assert
        account.Id.Should().Be(0);
        account.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Customer_WithNullEmail_Allowed()
    {
        // Arrange & Act
        var account = new Account
        {
            FirstName = "Test",
            Email = null
        };

        // Assert
        account.Email.Should().BeNull();
    }

    [Fact]
    public void Customer_Timestamps_Work()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var account = new Account
        {
            CreatedAt = now,
            UpdatedAt = now
        };

        // Assert
        account.CreatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        account.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Customer_LongValues_Handled()
    {
        // Arrange
        var longName = new string('A', 100);
        var longEmail = $"{new string('a', 50)}@{new string('b', 45)}.com";

        // Act
        var account = new Account
        {
            FirstName = longName,
            Email = longEmail
        };

        // Assert
        account.FirstName.Should().HaveLength(100);
        account.Email.Should().Contain("@");
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public void GetCustomers_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var accounts = Enumerable.Range(1, 100)
            .Select(i => new Account { Id = i, FirstName = $"Customer{i}" })
            .ToList();

        // Act
        var page1 = accounts.Skip(0).Take(10).ToList();
        var page2 = accounts.Skip(10).Take(10).ToList();

        // Assert
        page1.Should().HaveCount(10);
        page1.First().Id.Should().Be(1);
        page2.Should().HaveCount(10);
        page2.First().Id.Should().Be(11);
    }

    [Fact]
    public void GetCustomers_SortByName_ReturnsSorted()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, FirstName = "Charlie" },
            new() { Id = 2, FirstName = "Alice" },
            new() { Id = 3, FirstName = "Bob" }
        };

        // Act
        var sorted = accounts.OrderBy(c => c.FirstName).ToList();

        // Assert
        sorted[0].FirstName.Should().Be("Alice");
        sorted[1].FirstName.Should().Be("Bob");
        sorted[2].FirstName.Should().Be("Charlie");
    }

    #endregion

    #region Customer DTO Tests

    [Fact]
    public void AccountDto_Mapping_FromEntity()
    {
        // Arrange - Customer entity
        var account = new Account
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act - Create DTO from entity (simulated)
        var dto = new AccountDto
        {
            Id = account.Id,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Email = account.Email
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.FirstName.Should().Be("John");
        dto.LastName.Should().Be("Doe");
    }

    #endregion
}
