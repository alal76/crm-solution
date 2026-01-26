// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Account Entity Unit Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for Account entity functionality
/// </summary>
public class AccountServiceTests
{
    #region Create Account Tests

    [Fact]
    public void CreateAccount_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var account = new Account
        {
            AccountNumber = "ACC-NEW",
            CustomerId = 1,
            Status = AccountStatus.Current,
            MRR = 500m,
            ARR = 6000m
        };

        // Assert
        account.Should().NotBeNull();
        account.AccountNumber.Should().Be("ACC-NEW");
    }

    [Fact]
    public void CreateAccount_WithAllFields_SetsCorrectly()
    {
        // Arrange & Act
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddYears(1);

        var account = new Account
        {
            AccountNumber = "ACC-FULL",
            CustomerId = 1,
            Status = AccountStatus.Current,
            MRR = 1000m,
            ARR = 12000m,
            ContractStartDate = startDate,
            ContractEndDate = endDate,
            IsAutoRenew = true,
            BillingCycle = "Monthly"
        };

        // Assert
        account.AccountNumber.Should().Be("ACC-FULL");
        account.MRR.Should().Be(1000m);
        account.ARR.Should().Be(12000m);
        account.IsAutoRenew.Should().BeTrue();
    }

    #endregion

    #region Account Status Tests

    [Fact]
    public void Account_StatusTransition_CurrentToChurned()
    {
        // Arrange
        var account = new Account { Status = AccountStatus.Current };

        // Act
        account.Status = AccountStatus.Churned;

        // Assert
        account.Status.Should().Be(AccountStatus.Churned);
    }

    [Theory]
    [InlineData(AccountStatus.Current)]
    [InlineData(AccountStatus.Churned)]
    public void AccountStatus_AllStatusesValid(AccountStatus status)
    {
        // Arrange
        var account = new Account();

        // Act
        account.Status = status;

        // Assert
        account.Status.Should().Be(status);
    }

    #endregion

    #region Revenue Metrics Tests

    [Fact]
    public void Account_MRR_SetCorrectly()
    {
        // Arrange
        var account = new Account { MRR = 1000m };

        // Assert
        account.MRR.Should().Be(1000m);
    }

    [Fact]
    public void Account_ARR_SetCorrectly()
    {
        // Arrange
        var account = new Account { ARR = 12000m };

        // Assert
        account.ARR.Should().Be(12000m);
    }

    [Fact]
    public void Account_TotalRevenue_CanBeTracked()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, MRR = 500m, ARR = 6000m },
            new() { Id = 2, MRR = 1000m, ARR = 12000m },
            new() { Id = 3, MRR = 2500m, ARR = 30000m }
        };

        // Act
        var totalMRR = accounts.Sum(a => a.MRR ?? 0);
        var totalARR = accounts.Sum(a => a.ARR ?? 0);

        // Assert
        totalMRR.Should().Be(4000m);
        totalARR.Should().Be(48000m);
    }

    [Fact]
    public void Account_OneTimeFee_Tracked()
    {
        // Arrange
        var account = new Account
        {
            MRR = 1000m,
            OneTimeFee = 500m
        };

        // Assert
        account.OneTimeFee.Should().Be(500m);
    }

    #endregion

    #region Contract Management Tests

    [Fact]
    public void Account_ContractDates_SetCorrectly()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddYears(1);

        var account = new Account
        {
            ContractStartDate = startDate,
            ContractEndDate = endDate
        };

        // Assert
        account.ContractStartDate.Should().Be(startDate);
        account.ContractEndDate.Should().Be(endDate);
    }

    [Fact]
    public void Account_ContractDuration_Calculated()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2025, 1, 1);

        var account = new Account
        {
            ContractStartDate = startDate,
            ContractEndDate = endDate
        };

        // Act
        var duration = (account.ContractEndDate - account.ContractStartDate)?.Days;

        // Assert
        duration.Should().Be(366); // 2024 is a leap year
    }

    [Fact]
    public void Account_RenewalDate_Tracking()
    {
        // Arrange
        var renewalDate = DateTime.UtcNow.AddMonths(1);

        var account = new Account
        {
            ContractEndDate = renewalDate,
            IsAutoRenew = true
        };

        // Assert
        account.ContractEndDate.Should().BeCloseTo(renewalDate, TimeSpan.FromSeconds(1));
        account.IsAutoRenew.Should().BeTrue();
    }

    [Fact]
    public void Account_AutoRenewal_CanBeToggled()
    {
        // Arrange
        var account = new Account { IsAutoRenew = false };

        // Act
        account.IsAutoRenew = true;

        // Assert
        account.IsAutoRenew.Should().BeTrue();
    }

    #endregion

    #region Search and Filter Tests

    [Fact]
    public void FilterAccounts_ByStatus_ReturnsMatching()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, Status = AccountStatus.Current },
            new() { Id = 2, Status = AccountStatus.Churned },
            new() { Id = 3, Status = AccountStatus.Current }
        };

        // Act
        var currentAccounts = accounts.Where(a => a.Status == AccountStatus.Current);

        // Assert
        currentAccounts.Should().HaveCount(2);
    }

    [Fact]
    public void FilterAccounts_ByMinMRR_ReturnsMatching()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, MRR = 500m },
            new() { Id = 2, MRR = 1500m },
            new() { Id = 3, MRR = 2500m }
        };

        // Act
        var highValueAccounts = accounts.Where(a => a.MRR >= 1000m);

        // Assert
        highValueAccounts.Should().HaveCount(2);
    }

    [Fact]
    public void FilterAccounts_RenewingSoon_ReturnsMatching()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var accounts = new List<Account>
        {
            new() { Id = 1, ContractEndDate = now.AddDays(15) },
            new() { Id = 2, ContractEndDate = now.AddDays(45) },
            new() { Id = 3, ContractEndDate = now.AddDays(100) }
        };

        // Act
        var renewingSoon = accounts.Where(a => 
            a.ContractEndDate <= now.AddDays(30));

        // Assert
        renewingSoon.Should().HaveCount(1);
    }

    #endregion

    #region Update Account Tests

    [Fact]
    public void UpdateAccount_ChangeStatus_UpdatesCorrectly()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            AccountNumber = "ACC-001",
            Status = AccountStatus.Current
        };

        // Act
        account.Status = AccountStatus.Churned;

        // Assert
        account.Status.Should().Be(AccountStatus.Churned);
    }

    [Fact]
    public void UpdateAccount_ChangeMRR_UpdatesCorrectly()
    {
        // Arrange
        var account = new Account { Id = 1, MRR = 1000m };

        // Act
        account.MRR = 1500m;
        account.ARR = 18000m;

        // Assert
        account.MRR.Should().Be(1500m);
        account.ARR.Should().Be(18000m);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void SoftDelete_SetsIsDeletedFlag()
    {
        // Arrange
        var account = new Account { Id = 1, IsDeleted = false };

        // Act
        account.IsDeleted = true;

        // Assert
        account.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Account Health and Metrics Tests

    [Fact]
    public void Account_HealthScore_CanBeTracked()
    {
        // Arrange
        var account = new Account
        {
            Status = AccountStatus.Current,
            MRR = 1000m
        };

        // Assert - Account is healthy if Current with positive MRR
        account.Status.Should().Be(AccountStatus.Current);
        account.MRR.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Account_ChurnRisk_Identified()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, Status = AccountStatus.Current },
            new() { Id = 2, Status = AccountStatus.Churned },
            new() { Id = 3, Status = AccountStatus.Current }
        };

        // Act
        var churned = accounts.Where(a => 
            a.Status == AccountStatus.Churned);

        // Assert
        churned.Should().HaveCount(1);
    }

    [Fact]
    public void Account_ChurnedAnalysis_Works()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, Status = AccountStatus.Current, MRR = 1000m },
            new() { Id = 2, Status = AccountStatus.Churned, MRR = 500m },
            new() { Id = 3, Status = AccountStatus.Churned, MRR = 750m }
        };

        // Act
        var churned = accounts.Where(a => a.Status == AccountStatus.Churned);
        var churnedMRR = churned.Sum(a => a.MRR ?? 0);

        // Assert
        churned.Should().HaveCount(2);
        churnedMRR.Should().Be(1250m);
    }

    #endregion

    #region Pagination and Sorting Tests

    [Fact]
    public void GetAccounts_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var accounts = Enumerable.Range(1, 50)
            .Select(i => new Account { Id = i, AccountNumber = $"ACC-{i:D3}" })
            .ToList();

        // Act
        var page1 = accounts.Skip(0).Take(10).ToList();
        var page2 = accounts.Skip(10).Take(10).ToList();

        // Assert
        page1.Should().HaveCount(10);
        page2.Should().HaveCount(10);
        page2.First().Id.Should().Be(11);
    }

    [Fact]
    public void GetAccounts_SortByMRR_ReturnsSorted()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, MRR = 500m },
            new() { Id = 2, MRR = 2000m },
            new() { Id = 3, MRR = 1000m }
        };

        // Act
        var sorted = accounts.OrderByDescending(a => a.MRR).ToList();

        // Assert
        sorted[0].MRR.Should().Be(2000m);
        sorted[1].MRR.Should().Be(1000m);
        sorted[2].MRR.Should().Be(500m);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Account_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var account = new Account();

        // Assert
        account.Id.Should().Be(0);
        account.IsDeleted.Should().BeFalse();
        account.IsAutoRenew.Should().BeFalse();
    }

    [Fact]
    public void Account_Timestamps_Work()
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
    public void Account_Currency_CanBeSet()
    {
        // Arrange & Act
        var account = new Account
        {
            Currency = "USD"
        };

        // Assert
        account.Currency.Should().Be("USD");
    }

    [Fact]
    public void Account_BillingAddress_CanBeSet()
    {
        // Arrange & Act
        var account = new Account
        {
            BillingAddress = "123 Main St",
            BillingCity = "New York",
            BillingState = "NY",
            BillingZip = "10001",
            BillingCountry = "USA"
        };

        // Assert
        account.BillingAddress.Should().Be("123 Main St");
        account.BillingCity.Should().Be("New York");
        account.BillingCountry.Should().Be("USA");
    }

    [Fact]
    public void Account_ContractDocument_MetadataCanBeSet()
    {
        // Arrange & Act
        var account = new Account
        {
            ContractFileName = "contract.pdf",
            ContractFilePath = "/documents/contracts/contract.pdf",
            ContractContentType = "application/pdf",
            ContractFileSize = 1024000
        };

        // Assert
        account.ContractFileName.Should().Be("contract.pdf");
        account.ContractFilePath.Should().Contain("contract.pdf");
        account.ContractContentType.Should().Be("application/pdf");
        account.ContractFileSize.Should().Be(1024000);
    }

    #endregion
}
