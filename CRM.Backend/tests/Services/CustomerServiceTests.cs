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
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Category = CustomerCategory.Individual
        };

        // Assert
        customer.Should().NotBeNull();
        customer.Category.Should().Be(CustomerCategory.Individual);
        customer.FirstName.Should().Be("John");
    }

    [Fact]
    public void CreateCustomer_ValidOrganization_CreatesCorrectly()
    {
        // Arrange & Act
        var customer = new Customer
        {
            Category = CustomerCategory.Organization,
            Company = "Acme Corporation",
            Industry = "Technology",
            Email = "info@acme.com"
        };

        // Assert
        customer.Should().NotBeNull();
        customer.Category.Should().Be(CustomerCategory.Organization);
        customer.Company.Should().Be("Acme Corporation");
    }

    [Fact]
    public void CreateCustomer_WithAllFields_SetsCorrectly()
    {
        // Arrange & Act
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-1234",
            Category = CustomerCategory.Individual,
            CustomerType = CustomerType.Enterprise,
            Priority = CustomerPriority.High,
            LifecycleStage = CustomerLifecycleStage.Customer,
            Website = "https://example.com",
            Address = "123 Main St",
            City = "New York",
            Country = "USA",
            ZipCode = "10001"
        };

        // Assert
        customer.FirstName.Should().Be("John");
        customer.CustomerType.Should().Be(CustomerType.Enterprise);
        customer.Priority.Should().Be(CustomerPriority.High);
        customer.Website.Should().Be("https://example.com");
    }

    #endregion

    #region Update Customer Tests

    [Fact]
    public void UpdateCustomer_ChangeCategory_UpdatesCorrectly()
    {
        // Arrange
        var customer = new Customer
        {
            Category = CustomerCategory.Individual,
            FirstName = "John"
        };

        // Act
        customer.Category = CustomerCategory.Organization;
        customer.Company = "John Doe Inc";

        // Assert
        customer.Category.Should().Be(CustomerCategory.Organization);
        customer.Company.Should().Be("John Doe Inc");
    }

    [Fact]
    public void UpdateCustomer_ChangeLifecycleStage_UpdatesCorrectly()
    {
        // Arrange
        var customer = new Customer
        {
            LifecycleStage = CustomerLifecycleStage.Lead
        };

        // Act
        customer.LifecycleStage = CustomerLifecycleStage.Opportunity;

        // Assert
        customer.LifecycleStage.Should().Be(CustomerLifecycleStage.Opportunity);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void SoftDeleteCustomer_SetsIsDeletedFlag()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            IsDeleted = false
        };

        // Act
        customer.IsDeleted = true;

        // Assert
        customer.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Search and Filter Tests

    [Fact]
    public void SearchCustomers_ByName_ReturnsMatching()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe" },
            new() { Id = 2, FirstName = "Jane", LastName = "Doe" },
            new() { Id = 3, FirstName = "Bob", LastName = "Smith" }
        };

        // Act
        var searchResult = customers.Where(c => 
            c.LastName?.Contains("Doe", StringComparison.OrdinalIgnoreCase) ?? false);

        // Assert
        searchResult.Should().HaveCount(2);
    }

    [Fact]
    public void FilterCustomers_ByCategory_ReturnsMatching()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, Category = CustomerCategory.Individual },
            new() { Id = 2, Category = CustomerCategory.Organization },
            new() { Id = 3, Category = CustomerCategory.Individual }
        };

        // Act
        var individuals = customers.Where(c => c.Category == CustomerCategory.Individual);

        // Assert
        individuals.Should().HaveCount(2);
    }

    [Fact]
    public void FilterCustomers_ByLifecycleStage_ReturnsMatching()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, LifecycleStage = CustomerLifecycleStage.Lead },
            new() { Id = 2, LifecycleStage = CustomerLifecycleStage.Customer },
            new() { Id = 3, LifecycleStage = CustomerLifecycleStage.Customer }
        };

        // Act
        var activeCustomers = customers.Where(c => 
            c.LifecycleStage == CustomerLifecycleStage.Customer);

        // Assert
        activeCustomers.Should().HaveCount(2);
    }

    [Fact]
    public void FilterCustomers_ByIndustry_ReturnsMatching()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, Industry = "Technology" },
            new() { Id = 2, Industry = "Healthcare" },
            new() { Id = 3, Industry = "Technology" }
        };

        // Act
        var techCustomers = customers.Where(c => c.Industry == "Technology");

        // Assert
        techCustomers.Should().HaveCount(2);
    }

    [Fact]
    public void FilterCustomers_ByPriority_ReturnsMatching()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, Priority = CustomerPriority.High },
            new() { Id = 2, Priority = CustomerPriority.Medium },
            new() { Id = 3, Priority = CustomerPriority.High }
        };

        // Act
        var highPriorityCustomers = customers.Where(c => 
            c.Priority == CustomerPriority.High);

        // Assert
        highPriorityCustomers.Should().HaveCount(2);
    }

    #endregion

    #region Customer Type Tests

    [Theory]
    [InlineData(CustomerType.Individual)]
    [InlineData(CustomerType.SmallBusiness)]
    [InlineData(CustomerType.MidMarket)]
    [InlineData(CustomerType.Enterprise)]
    [InlineData(CustomerType.Government)]
    [InlineData(CustomerType.NonProfit)]
    public void CustomerType_AllTypesValid(CustomerType type)
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.CustomerType = type;

        // Assert
        customer.CustomerType.Should().Be(type);
    }

    #endregion

    #region Customer Priority Tests

    [Theory]
    [InlineData(CustomerPriority.Low)]
    [InlineData(CustomerPriority.Medium)]
    [InlineData(CustomerPriority.High)]
    [InlineData(CustomerPriority.Critical)]
    public void CustomerPriority_AllPrioritiesValid(CustomerPriority priority)
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.Priority = priority;

        // Assert
        customer.Priority.Should().Be(priority);
    }

    #endregion

    #region Lifecycle Stage Tests

    [Theory]
    [InlineData(CustomerLifecycleStage.Other)]
    [InlineData(CustomerLifecycleStage.Lead)]
    [InlineData(CustomerLifecycleStage.Opportunity)]
    [InlineData(CustomerLifecycleStage.Customer)]
    [InlineData(CustomerLifecycleStage.CustomerAtRisk)]
    [InlineData(CustomerLifecycleStage.Churned)]
    [InlineData(CustomerLifecycleStage.WinBack)]
    public void CustomerLifecycleStage_AllStagesValid(CustomerLifecycleStage stage)
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.LifecycleStage = stage;

        // Assert
        customer.LifecycleStage.Should().Be(stage);
    }

    #endregion

    #region Edge Cases and Validation

    [Fact]
    public void Customer_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert
        customer.Id.Should().Be(0);
        customer.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Customer_WithNullEmail_Allowed()
    {
        // Arrange & Act
        var customer = new Customer
        {
            FirstName = "Test",
            Email = null
        };

        // Assert
        customer.Email.Should().BeNull();
    }

    [Fact]
    public void Customer_Timestamps_Work()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var customer = new Customer
        {
            CreatedAt = now,
            UpdatedAt = now
        };

        // Assert
        customer.CreatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        customer.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Customer_LongValues_Handled()
    {
        // Arrange
        var longName = new string('A', 100);
        var longEmail = $"{new string('a', 50)}@{new string('b', 45)}.com";

        // Act
        var customer = new Customer
        {
            FirstName = longName,
            Email = longEmail
        };

        // Assert
        customer.FirstName.Should().HaveLength(100);
        customer.Email.Should().Contain("@");
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public void GetCustomers_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var customers = Enumerable.Range(1, 100)
            .Select(i => new Customer { Id = i, FirstName = $"Customer{i}" })
            .ToList();

        // Act
        var page1 = customers.Skip(0).Take(10).ToList();
        var page2 = customers.Skip(10).Take(10).ToList();

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
        var customers = new List<Customer>
        {
            new() { Id = 1, FirstName = "Charlie" },
            new() { Id = 2, FirstName = "Alice" },
            new() { Id = 3, FirstName = "Bob" }
        };

        // Act
        var sorted = customers.OrderBy(c => c.FirstName).ToList();

        // Assert
        sorted[0].FirstName.Should().Be("Alice");
        sorted[1].FirstName.Should().Be("Bob");
        sorted[2].FirstName.Should().Be("Charlie");
    }

    #endregion

    #region Customer DTO Tests

    [Fact]
    public void CustomerDto_Mapping_FromEntity()
    {
        // Arrange - Customer entity
        var customer = new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act - Create DTO from entity (simulated)
        var dto = new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.FirstName.Should().Be("John");
        dto.LastName.Should().Be("Doe");
    }

    #endregion
}
