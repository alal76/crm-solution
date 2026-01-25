// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Dtos;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.Entities;

/// <summary>
/// Comprehensive unit tests for Domain Entities
/// 
/// FUNCTIONAL VIEW:
/// - Tests entity property validation
/// - Tests business rule enforcement
/// - Tests entity relationships
/// 
/// TECHNICAL VIEW:
/// - Uses DataAnnotations validation
/// - Tests default values and computed properties
/// </summary>
public class DomainEntityTests
{
    #region Customer Entity Tests

    [Fact]
    public void Customer_NewCustomer_HasDefaultValues()
    {
        // Act
        var customer = new Customer();

        // Assert
        customer.Id.Should().Be(0);
        customer.IsDeleted.Should().BeFalse();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Customer_IndividualCategory_RequiresName()
    {
        // Arrange
        var customer = new Customer
        {
            Category = CustomerCategory.Individual,
            FirstName = "John",
            LastName = "Doe"
        };

        // Assert
        customer.FirstName.Should().NotBeNullOrEmpty();
        customer.LastName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Customer_OrganizationCategory_RequiresCompanyName()
    {
        // Arrange
        var customer = new Customer
        {
            Category = CustomerCategory.Organization,
            CompanyName = "Acme Corp"
        };

        // Assert
        customer.CompanyName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Customer_DisplayName_Individual_ReturnsFullName()
    {
        // Arrange
        var customer = new Customer
        {
            Category = CustomerCategory.Individual,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var displayName = customer.GetDisplayName();

        // Assert
        displayName.Should().Be("John Doe");
    }

    [Fact]
    public void Customer_DisplayName_Organization_ReturnsCompanyName()
    {
        // Arrange
        var customer = new Customer
        {
            Category = CustomerCategory.Organization,
            CompanyName = "Acme Corporation"
        };

        // Act
        var displayName = customer.GetDisplayName();

        // Assert
        displayName.Should().Be("Acme Corporation");
    }

    #endregion

    #region User Entity Tests

    [Fact]
    public void User_NewUser_HasDefaultValues()
    {
        // Act
        var user = new User();

        // Assert
        user.IsActive.Should().BeTrue();
        user.IsDeleted.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(0);
        user.TwoFactorEnabled.Should().BeFalse();
    }

    [Fact]
    public void User_Email_ShouldBeValidFormat()
    {
        // Arrange
        var validEmails = new[] { "test@example.com", "user.name@domain.org", "user+tag@test.co.uk" };
        var invalidEmails = new[] { "notanemail", "@nodomain", "missing.at.sign" };

        // Assert
        foreach (var email in validEmails)
        {
            IsValidEmail(email).Should().BeTrue($"{email} should be valid");
        }

        foreach (var email in invalidEmails)
        {
            IsValidEmail(email).Should().BeFalse($"{email} should be invalid");
        }
    }

    [Fact]
    public void User_PasswordComplexity_MeetsRequirements()
    {
        // Arrange
        var validPasswords = new[] { "Admin@123", "P@ssw0rd!", "Str0ng#Pass" };
        var invalidPasswords = new[] { "short", "nouppercase1!", "NOLOWERCASE1!", "NoNumbers!" };

        // Assert
        foreach (var password in validPasswords)
        {
            IsPasswordComplex(password).Should().BeTrue($"{password} should meet complexity");
        }

        foreach (var password in invalidPasswords)
        {
            IsPasswordComplex(password).Should().BeFalse($"{password} should not meet complexity");
        }
    }

    [Fact]
    public void User_LockAccount_AfterMaxFailedAttempts()
    {
        // Arrange
        var user = new User();
        var maxAttempts = 5;

        // Act
        for (int i = 0; i < maxAttempts; i++)
        {
            user.IncrementFailedAttempts();
        }

        // Assert
        user.FailedLoginAttempts.Should().Be(5);
        user.IsLocked.Should().BeTrue();
        user.LockoutEnd.Should().NotBeNull();
    }

    [Fact]
    public void User_ResetFailedAttempts_OnSuccessfulLogin()
    {
        // Arrange
        var user = new User { FailedLoginAttempts = 3 };

        // Act
        user.ResetFailedAttempts();

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
    }

    #endregion

    #region Address Entity Tests

    [Fact]
    public void Address_NewAddress_HasDefaultValues()
    {
        // Act
        var address = new Address();

        // Assert
        address.IsPrimary.Should().BeFalse();
        address.IsVerified.Should().BeFalse();
    }

    [Fact]
    public void Address_FormattedAddress_ReturnsCorrectFormat()
    {
        // Arrange
        var address = new Address
        {
            Street1 = "123 Main St",
            Street2 = "Suite 100",
            City = "Beverly Hills",
            State = "CA",
            PostalCode = "90210",
            Country = "USA"
        };

        // Act
        var formatted = address.GetFormattedAddress();

        // Assert
        formatted.Should().Contain("123 Main St");
        formatted.Should().Contain("Beverly Hills");
        formatted.Should().Contain("CA");
        formatted.Should().Contain("90210");
    }

    #endregion

    #region Opportunity Entity Tests

    [Fact]
    public void Opportunity_NewOpportunity_HasDefaultValues()
    {
        // Act
        var opportunity = new Opportunity();

        // Assert
        opportunity.Stage.Should().Be("Prospecting");
        opportunity.Probability.Should().Be(10);
    }

    [Fact]
    public void Opportunity_StageChange_UpdatesProbability()
    {
        // Arrange
        var opportunity = new Opportunity();

        // Act & Assert - Each stage has expected probability
        opportunity.SetStage("Prospecting");
        opportunity.Probability.Should().Be(10);

        opportunity.SetStage("Qualification");
        opportunity.Probability.Should().Be(20);

        opportunity.SetStage("Proposal");
        opportunity.Probability.Should().Be(50);

        opportunity.SetStage("Negotiation");
        opportunity.Probability.Should().Be(75);

        opportunity.SetStage("Closed Won");
        opportunity.Probability.Should().Be(100);

        opportunity.SetStage("Closed Lost");
        opportunity.Probability.Should().Be(0);
    }

    [Fact]
    public void Opportunity_WeightedAmount_CalculatesCorrectly()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000,
            Probability = 50
        };

        // Act
        var weightedAmount = opportunity.GetWeightedAmount();

        // Assert
        weightedAmount.Should().Be(50000);
    }

    #endregion

    #region Product Entity Tests

    [Fact]
    public void Product_GrossMargin_CalculatesCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Price = 100.00m,
            Cost = 60.00m
        };

        // Act
        var margin = product.GetGrossMargin();

        // Assert
        margin.Should().Be(40.00m);
    }

    [Fact]
    public void Product_MarginPercentage_CalculatesCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Price = 100.00m,
            Cost = 60.00m
        };

        // Act
        var marginPercent = product.GetMarginPercentage();

        // Assert
        marginPercent.Should().BeApproximately(40.0, 0.01);
    }

    #endregion

    #region Campaign Entity Tests

    [Fact]
    public void Campaign_IsActive_BasedOnDates()
    {
        // Arrange
        var activeCampaign = new Campaign
        {
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow.AddDays(5),
            Status = "Active"
        };

        var expiredCampaign = new Campaign
        {
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-1),
            Status = "Active"
        };

        // Assert
        activeCampaign.IsCurrentlyActive().Should().BeTrue();
        expiredCampaign.IsCurrentlyActive().Should().BeFalse();
    }

    [Fact]
    public void Campaign_ROI_CalculatesCorrectly()
    {
        // Arrange
        var campaign = new Campaign
        {
            Budget = 10000,
            ActualCost = 8000,
            Revenue = 24000
        };

        // Act
        var roi = campaign.CalculateROI();

        // Assert
        roi.Should().BeApproximately(200.0, 0.01); // (24000 - 8000) / 8000 * 100 = 200%
    }

    #endregion

    #region SystemSettings Entity Tests

    [Fact]
    public void SystemSettings_DefaultValues_AreCorrect()
    {
        // Act
        var settings = new SystemSettings();

        // Assert - All modules enabled by default
        settings.CustomersEnabled.Should().BeTrue();
        settings.ContactsEnabled.Should().BeTrue();
        settings.LeadsEnabled.Should().BeTrue();
        settings.OpportunitiesEnabled.Should().BeTrue();
        settings.ProductsEnabled.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsPasswordComplex(string password)
    {
        if (password.Length < 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsLower)) return false;
        if (!password.Any(char.IsDigit)) return false;
        if (!password.Any(c => !char.IsLetterOrDigit(c))) return false;
        return true;
    }

    #endregion
}
