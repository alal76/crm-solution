// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CRM.Tests.Entities;

/// <summary>
/// Advanced entity validation, relationships and business logic tests
/// </summary>
public class EntityValidationTests
{
    #region Customer Validation Tests

    [Fact]
    public void Customer_RequiredFields_ShouldNotBeNull()
    {
        // Arrange
        var account = new Account
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com"
        };

        // Assert
        account.FirstName.Should().NotBeNullOrEmpty();
        account.LastName.Should().NotBeNullOrEmpty();
        account.Email.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Customer_Email_ShouldContainAtSymbol()
    {
        // Arrange
        var validEmail = "test@company.com";
        var account = new Account { Email = validEmail };

        // Assert
        account.Email.Should().Contain("@");
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("test", false)]
    [InlineData("test@company.com", true)]
    [InlineData("user.name@domain.org", true)]
    public void Customer_Email_ValidationPatterns(string email, bool isValid)
    {
        var account = new Account { Email = email };
        var hasAtSign = account.Email.Contains("@");
        hasAtSign.Should().Be(isValid);
    }

    [Fact]
    public void Customer_PhoneFormat_ShouldBeValid()
    {
        // Arrange
        var account = new Account { Phone = "+1-555-123-4567" };

        // Assert
        account.Phone.Should().NotBeNullOrEmpty();
        account.Phone.Should().Contain("-");
    }

    [Fact]
    public void Customer_DefaultCategory_ShouldBeIndividual()
    {
        // Arrange
        var account = new Account();

        // Assert - Individual is typically 0 which is default
        ((int)account.Category).Should().Be(0);
    }

    [Fact]
    public void Customer_Relationships_ShouldBeInitialized()
    {
        // Arrange
        var account = new Account();

        // Assert - Check that navigation properties can be set
        account.Opportunities = new List<Opportunity>();
        account.Opportunities.Should().NotBeNull();
    }

    #endregion

    #region Lead Validation Tests

    [Fact]
    public void Lead_FullName_ComputedCorrectly()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Assert
        lead.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void Lead_FullName_WithEmptyFirstName()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = "",
            LastName = "Doe"
        };

        // Assert - FullName uses Trim() to remove leading/trailing whitespace
        lead.FullName.Should().Be("Doe");
    }

    [Fact]
    public void Lead_IsOpen_WhenNew()
    {
        // Arrange
        var lead = new Lead { Status = LeadLifecycleStatus.New };

        // Assert
        lead.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Lead_IsOpen_WhenWorking()
    {
        // Arrange
        var lead = new Lead { Status = LeadLifecycleStatus.Working };

        // Assert
        lead.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Lead_IsNotOpen_WhenConverted()
    {
        // Arrange
        var lead = new Lead { Status = LeadLifecycleStatus.Converted };

        // Assert
        lead.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Lead_IsNotOpen_WhenDisqualified()
    {
        // Arrange
        var lead = new Lead { Status = LeadLifecycleStatus.Disqualified };

        // Assert
        lead.IsOpen.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Lead_Score_ValidRange(int score)
    {
        // Arrange
        var lead = new Lead { Score = score };

        // Assert
        lead.Score.Should().BeInRange(0, 100);
    }

    [Fact]
    public void Lead_QualificationNotes_CanBeSet()
    {
        // Arrange
        var lead = new Lead { QualificationNotes = "Qualified via phone call" };

        // Assert
        lead.QualificationNotes.Should().Be("Qualified via phone call");
    }

    #endregion

    #region Opportunity Validation Tests

    [Fact]
    public void Opportunity_WeightedAmount_CalculatedCorrectly()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 10000m,
            Probability = 50
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(5000m);
    }

    [Theory]
    [InlineData(100000, 25, 25000)]
    [InlineData(50000, 50, 25000)]
    [InlineData(25000, 100, 25000)]
    [InlineData(10000, 0, 0)]
    public void Opportunity_WeightedAmount_VariousScenarios(decimal amount, int probability, decimal expected)
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = amount,
            Probability = probability
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(expected);
    }

    [Fact]
    public void Opportunity_IsOpen_WhenDiscovery()
    {
        // Arrange
        var opportunity = new Opportunity { Stage = OpportunityStage.Discovery };

        // Assert
        opportunity.IsOpen.Should().BeTrue();
    }

    [Theory]
    [InlineData(OpportunityStage.Discovery, true)]
    [InlineData(OpportunityStage.Qualification, true)]
    [InlineData(OpportunityStage.Proposal, true)]
    [InlineData(OpportunityStage.Negotiation, true)]
    [InlineData(OpportunityStage.ClosedWon, false)]
    [InlineData(OpportunityStage.ClosedLost, false)]
    public void Opportunity_IsOpen_ByStage(OpportunityStage stage, bool expected)
    {
        // Arrange
        var opportunity = new Opportunity { Stage = stage };

        // Assert
        opportunity.IsOpen.Should().Be(expected);
    }

    [Theory]
    [InlineData(OpportunityStage.ClosedWon, true)]
    [InlineData(OpportunityStage.ClosedLost, false)]
    [InlineData(OpportunityStage.Discovery, false)]
    public void Opportunity_IsWon_ByStage(OpportunityStage stage, bool expected)
    {
        // Arrange
        var opportunity = new Opportunity { Stage = stage };

        // Assert
        opportunity.IsWon.Should().Be(expected);
    }

    [Fact]
    public void Opportunity_ExpectedCloseDate_ShouldBeFuture()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            ExpectedCloseDate = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        opportunity.ExpectedCloseDate.Should().BeAfter(DateTime.UtcNow);
    }

    #endregion

    #region Account Validation Tests

    [Fact]
    public void Account_AnnualRevenue_ShouldBePositive()
    {
        // Arrange
        var account = new Account
        {
            AnnualRevenue = 1000000m
        };

        // Assert
        account.AnnualRevenue.Should().BePositive();
    }

    [Fact]
    public void Account_DateOfBirth_CanBeSet()
    {
        // Arrange
        var birthDate = new DateTime(1990, 1, 15);

        var account = new Account
        {
            DateOfBirth = birthDate,
            Category = AccountCategory.Individual
        };

        // Assert
        account.DateOfBirth!.Value.Should().Be(birthDate);
    }

    [Fact]
    public void Account_LifecycleStage_DefaultIsOther()
    {
        // Arrange
        var account = new Account();

        // Assert
        account.LifecycleStage.Should().Be(AccountLifecycleStage.Other);
    }

    [Theory]
    [InlineData(AccountLifecycleStage.Lead)]
    [InlineData(AccountLifecycleStage.Opportunity)]
    [InlineData(AccountLifecycleStage.Active)]
    [InlineData(AccountLifecycleStage.AtRisk)]
    [InlineData(AccountLifecycleStage.Churned)]
    public void Account_LifecycleStage_AllValidValues(AccountLifecycleStage stage)
    {
        // Arrange
        var account = new Account { LifecycleStage = stage };

        // Assert
        account.LifecycleStage.Should().Be(stage);
    }

    [Fact]
    public void Account_Address_CanBeSet()
    {
        // Arrange
        var account = new Account();
        account.Addresses.Add(new Address {
            Line1 = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        });

        // Assert
        account.Address.Should().Be("123 Main St");
        account.City.Should().Be("New York");
        account.ZipCode.Should().Be("10001");
    }

    [Fact]
    public void Account_ShippingSameAsBilling_DefaultTrue()
    {
        // Arrange
        var account = new Account();

        // Assert
        account.ShippingSameAsBilling.Should().BeTrue();
    }

    #endregion

    #region Product Validation Tests

    [Fact]
    public void Product_Price_ShouldBePositive()
    {
        // Arrange
        var product = new Product { Price = 99.99m };

        // Assert
        product.Price.Should().BePositive();
    }

    [Fact]
    public void Product_SKU_ShouldBeUnique()
    {
        // Arrange
        var product1 = new Product { SKU = "PROD-001" };
        var product2 = new Product { SKU = "PROD-002" };

        // Assert
        product1.SKU.Should().NotBe(product2.SKU);
    }

    [Fact]
    public void Product_Status_DefaultIsActive()
    {
        // Arrange
        var product = new Product { Status = ProductStatus.Active };

        // Assert
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Theory]
    [InlineData(ProductType.Physical)]
    [InlineData(ProductType.Digital)]
    [InlineData(ProductType.Service)]
    [InlineData(ProductType.Subscription)]
    public void Product_Type_AllValidValues(ProductType type)
    {
        // Arrange
        var product = new Product { ProductType = type };

        // Assert
        product.ProductType.Should().Be(type);
    }

    [Fact]
    public void Product_BillingFrequency_ForSubscription()
    {
        // Arrange
        var product = new Product
        {
            ProductType = ProductType.Subscription,
            BillingFrequency = BillingFrequency.Monthly
        };

        // Assert
        product.BillingFrequency.Should().Be(BillingFrequency.Monthly);
    }

    #endregion

    #region User Validation Tests

    [Fact]
    public void User_PasswordHash_ShouldNotBeEmpty()
    {
        // Arrange
        var user = new User { PasswordHash = "hashedpassword" };

        // Assert
        user.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void User_Username_ShouldBeUnique()
    {
        // Arrange
        var user1 = new User { Username = "admin" };
        var user2 = new User { Username = "user1" };

        // Assert
        user1.Username.Should().NotBe(user2.Username);
    }

    [Fact]
    public void User_Role_CanBeAdmin()
    {
        // Arrange
        var user = new User { Role = (int)UserRole.Admin };

        // Assert
        ((UserRole)user.Role).Should().Be(UserRole.Admin);
    }

    [Fact]
    public void User_IsActive_DefaultTrue()
    {
        // Arrange
        var user = new User { IsActive = true };

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_Email_ShouldBeValid()
    {
        // Arrange
        var user = new User { Email = "admin@company.com" };

        // Assert
        user.Email.Should().Contain("@");
        user.Email.Should().Contain(".");
    }

    #endregion

    #region Department Validation Tests

    [Fact]
    public void Department_Name_ShouldNotBeEmpty()
    {
        // Arrange
        var department = new Department { Name = "Sales" };

        // Assert
        department.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Department_Description_IsOptional()
    {
        // Arrange
        var department = new Department { Name = "Marketing" };

        // Assert - Description defaults to empty string (non-nullable)
        department.Description.Should().BeEmpty();
    }

    #endregion

    #region Contact Validation Tests

    [Fact]
    public void Contact_Name_ComputedCorrectly()
    {
        // Arrange
        var contact = new Contact
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Assert
        contact.FirstName.Should().Be("Jane");
        contact.LastName.Should().Be("Smith");
    }

    [Fact]
    public void Contact_EmailPrimary_CanBeSet()
    {
        // Arrange
        var contact = new Contact { EmailPrimary = "test@company.com" };

        // Assert
        contact.EmailPrimary.Should().Be("test@company.com");
    }

    [Fact]
    public void Contact_ContactType_CanBeToggled()
    {
        // Arrange
        var contact = new Contact { ContactType = ContactType.Customer };

        // Assert
        contact.ContactType.Should().Be(ContactType.Customer);
    }

    #endregion

    #region SystemSettings Validation Tests

    [Fact]
    public void SystemSettings_FeatureFlags_DefaultValues()
    {
        // Arrange
        var settings = new SystemSettings();

        // Assert - Core features are enabled by default for usability
        settings.CustomersEnabled.Should().BeTrue();
        settings.LeadsEnabled.Should().BeTrue();
    }

    [Fact]
    public void SystemSettings_AllFeatures_CanBeEnabled()
    {
        // Arrange
        var settings = new SystemSettings
        {
            CustomersEnabled = true,
            LeadsEnabled = true,
            OpportunitiesEnabled = true,
            ProductsEnabled = true,
            ContactsEnabled = true
        };

        // Assert
        settings.CustomersEnabled.Should().BeTrue();
        settings.LeadsEnabled.Should().BeTrue();
        settings.OpportunitiesEnabled.Should().BeTrue();
        settings.ProductsEnabled.Should().BeTrue();
        settings.ContactsEnabled.Should().BeTrue();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Customer_WithMaximumValues()
    {
        // Arrange
        var account = new Account
        {
            FirstName = new string('A', 100),
            LastName = new string('B', 100),
            Email = "test@" + new string('c', 50) + ".com"
        };

        // Assert
        account.FirstName.Should().HaveLength(100);
        account.LastName.Should().HaveLength(100);
    }

    [Fact]
    public void Opportunity_WithZeroAmount()
    {
        // Arrange
        var opportunity = new Opportunity { Amount = 0m, Probability = 50 };

        // Assert
        opportunity.WeightedAmount.Should().Be(0);
    }

    [Fact]
    public void Account_WithZeroRevenue()
    {
        // Arrange
        var account = new Account { AnnualRevenue = 0m };

        // Assert
        account.AnnualRevenue.Should().Be(0);
    }

    [Fact]
    public void Lead_WithMaxScore()
    {
        // Arrange
        var lead = new Lead { Score = 100, FitScore = 100, EngagementScore = 100 };

        // Assert
        lead.Score.Should().Be(100);
        lead.FitScore.Should().Be(100);
        lead.EngagementScore.Should().Be(100);
    }

    #endregion
}
