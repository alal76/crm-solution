// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Dtos;
using System;

namespace CRM.Tests.Entities;

/// <summary>
/// Unit tests for core CRM entities
/// Tests entity creation, property setting, and validation
/// </summary>
public class CoreEntityTests
{
    #region Customer Entity Tests

    [Fact]
    public void Customer_CanBeCreated_WithDefaults()
    {
        // Act
        var account = new Account();

        // Assert
        account.Id.Should().Be(0);
        account.Category.Should().Be(AccountCategory.Individual);
        account.FirstName.Should().BeEmpty();
        account.LastName.Should().BeEmpty();
        account.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Customer_CanBeCreated_WithIndividualProperties()
    {
        // Act
        var account = new Account
        {
            Id = 1,
            Category = AccountCategory.Individual,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "555-1234",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "Male",
            Salutation = "Mr.",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        account.Id.Should().Be(1);
        account.Category.Should().Be(AccountCategory.Individual);
        account.FirstName.Should().Be("John");
        account.LastName.Should().Be("Doe");
        account.Email.Should().Be("john.doe@example.com");
        account.Phone.Should().Be("555-1234");
        account.Salutation.Should().Be("Mr.");
    }

    [Fact]
    public void Customer_CanBeCreated_AsOrganization()
    {
        // Act
        var account = new Account
        {
            Id = 2,
            Category = AccountCategory.Organization,
            Company = "Acme Corporation",
            Industry = "Technology",
            Website = "https://acme.com",
            AccountType = AccountType.Enterprise,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        account.Category.Should().Be(AccountCategory.Organization);
        account.Company.Should().Be("Acme Corporation");
        account.Industry.Should().Be("Technology");
        account.AccountType.Should().Be(AccountType.Enterprise);
    }

    [Fact]
    public void AccountCategory_HasExpectedValues()
    {
        Enum.IsDefined(typeof(AccountCategory), AccountCategory.Individual).Should().BeTrue();
        Enum.IsDefined(typeof(AccountCategory), AccountCategory.Organization).Should().BeTrue();
    }

    [Fact]
    public void AccountLifecycleStage_HasAllExpectedValues()
    {
        Enum.IsDefined(typeof(AccountLifecycleStage), AccountLifecycleStage.Other).Should().BeTrue();
        Enum.IsDefined(typeof(AccountLifecycleStage), AccountLifecycleStage.Lead).Should().BeTrue();
        Enum.IsDefined(typeof(AccountLifecycleStage), AccountLifecycleStage.Opportunity).Should().BeTrue();
        Enum.IsDefined(typeof(AccountLifecycleStage), AccountLifecycleStage.Active).Should().BeTrue();
        Enum.IsDefined(typeof(AccountLifecycleStage), AccountLifecycleStage.AtRisk).Should().BeTrue();
        Enum.IsDefined(typeof(AccountLifecycleStage), AccountLifecycleStage.Churned).Should().BeTrue();
        Enum.IsDefined(typeof(AccountLifecycleStage), AccountLifecycleStage.WinBack).Should().BeTrue();
    }

    [Fact]
    public void AccountType_HasAllExpectedValues()
    {
        Enum.IsDefined(typeof(AccountType), AccountType.Individual).Should().BeTrue();
        Enum.IsDefined(typeof(AccountType), AccountType.SmallBusiness).Should().BeTrue();
        Enum.IsDefined(typeof(AccountType), AccountType.MidMarket).Should().BeTrue();
        Enum.IsDefined(typeof(AccountType), AccountType.Enterprise).Should().BeTrue();
        Enum.IsDefined(typeof(AccountType), AccountType.Government).Should().BeTrue();
        Enum.IsDefined(typeof(AccountType), AccountType.NonProfit).Should().BeTrue();
    }

    [Fact]
    public void AccountPriority_HasAllExpectedValues()
    {
        Enum.IsDefined(typeof(AccountPriority), AccountPriority.Low).Should().BeTrue();
        Enum.IsDefined(typeof(AccountPriority), AccountPriority.Medium).Should().BeTrue();
        Enum.IsDefined(typeof(AccountPriority), AccountPriority.High).Should().BeTrue();
        Enum.IsDefined(typeof(AccountPriority), AccountPriority.Critical).Should().BeTrue();
    }

    #endregion

    #region User Entity Tests

    [Fact]
    public void User_CanBeCreated_WithDefaults()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().Be(0);
        user.Username.Should().BeEmpty();
        user.Email.Should().BeEmpty();
        user.FirstName.Should().BeEmpty();
        user.LastName.Should().BeEmpty();
        user.IsActive.Should().BeTrue();
        user.Role.Should().Be(0);
    }

    [Fact]
    public void User_CanBeCreated_WithProperties()
    {
        // Act
        var user = new User
        {
            Id = 1,
            Username = "jdoe",
            Email = "john.doe@company.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashedpassword123",
            Role = (int)UserRole.Sales,
            IsActive = true,
            LastLoginAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        user.Id.Should().Be(1);
        user.Username.Should().Be("jdoe");
        user.Email.Should().Be("john.doe@company.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Role.Should().Be((int)UserRole.Sales);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UserRole_HasAllExpectedValues()
    {
        Enum.IsDefined(typeof(UserRole), UserRole.Admin).Should().BeTrue();
        Enum.IsDefined(typeof(UserRole), UserRole.Manager).Should().BeTrue();
        Enum.IsDefined(typeof(UserRole), UserRole.Sales).Should().BeTrue();
        Enum.IsDefined(typeof(UserRole), UserRole.Support).Should().BeTrue();
        Enum.IsDefined(typeof(UserRole), UserRole.Guest).Should().BeTrue();
    }

    [Fact]
    public void User_DefaultRole_IsAdmin()
    {
        // Act
        var user = new User();

        // Assert
        user.Role.Should().Be((int)UserRole.Admin);
    }

    [Fact]
    public void User_CanSetDifferentRoles()
    {
        // Arrange
        var user = new User();

        // Act & Assert
        user.Role = (int)UserRole.Manager;
        user.Role.Should().Be((int)UserRole.Manager);

        user.Role = (int)UserRole.Sales;
        user.Role.Should().Be((int)UserRole.Sales);

        user.Role = (int)UserRole.Support;
        user.Role.Should().Be((int)UserRole.Support);

        user.Role = (int)UserRole.Guest;
        user.Role.Should().Be((int)UserRole.Guest);
    }

    #endregion

    #region Account Entity Tests

    [Fact]
    public void Account_CanBeCreated_WithDefaults()
    {
        // Act
        var account = new Account();

        // Assert
        account.Id.Should().Be(0);
        account.FirstName.Should().BeEmpty();
        account.LifecycleStage.Should().Be(AccountLifecycleStage.Other);
        account.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Account_CanBeCreated_WithProperties()
    {
        // Act
        var account = new Account
        {
            Id = 1,
            Category = AccountCategory.Organization,
            Company = "Acme Corp",
            Industry = "Technology",
            AnnualRevenue = 5000000m,
            NumberOfEmployees = 100,
            LifecycleStage = AccountLifecycleStage.Active,
            Website = "https://acme.com",
            Email = "contact@acme.com",
            Phone = "555-1234",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        account.Id.Should().Be(1);
        account.Company.Should().Be("Acme Corp");
        account.Industry.Should().Be("Technology");
        account.AnnualRevenue.Should().Be(5000000m);
        account.NumberOfEmployees.Should().Be(100);
        account.LifecycleStage.Should().Be(AccountLifecycleStage.Active);
        account.Website.Should().Be("https://acme.com");
    }

    #endregion

    #region Opportunity Entity Tests

    [Fact]
    public void Opportunity_CanBeCreated_WithDefaults()
    {
        // Act
        var opportunity = new Opportunity();

        // Assert
        opportunity.Id.Should().Be(0);
        opportunity.Name.Should().BeEmpty();
        opportunity.Stage.Should().Be(OpportunityStage.Discovery);
        opportunity.Probability.Should().Be(10);
        opportunity.Amount.Should().Be(0);
        opportunity.Currency.Should().Be("USD");
    }

    [Fact]
    public void Opportunity_CanBeCreated_WithProperties()
    {
        // Act
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Enterprise Deal - Acme Corp",
            Stage = OpportunityStage.Proposal,
            Probability = 50,
            Amount = 100000m,
            Currency = "USD",
            AccountId = 5,
            ExpectedCloseDate = DateTime.UtcNow.AddMonths(1),
            PricingModel = OpportunityPricingModel.Subscription,
            TermLengthMonths = 12,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        opportunity.Id.Should().Be(1);
        opportunity.Name.Should().Be("Enterprise Deal - Acme Corp");
        opportunity.Stage.Should().Be(OpportunityStage.Proposal);
        opportunity.Probability.Should().Be(50);
        opportunity.Amount.Should().Be(100000m);
        opportunity.AccountId.Should().Be(5);
    }

    [Fact]
    public void OpportunityStage_HasAllExpectedValues()
    {
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.Discovery).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.Qualification).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.Proposal).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.Negotiation).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.ClosedWon).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.ClosedLost).Should().BeTrue();
    }

    [Fact]
    public void Opportunity_IsOpen_ReturnsTrueForOpenStages()
    {
        // Arrange
        var discoveryOpp = new Opportunity { Stage = OpportunityStage.Discovery };
        var qualOpp = new Opportunity { Stage = OpportunityStage.Qualification };
        var proposalOpp = new Opportunity { Stage = OpportunityStage.Proposal };
        var negotiationOpp = new Opportunity { Stage = OpportunityStage.Negotiation };

        // Assert
        discoveryOpp.IsOpen.Should().BeTrue();
        qualOpp.IsOpen.Should().BeTrue();
        proposalOpp.IsOpen.Should().BeTrue();
        negotiationOpp.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_IsOpen_ReturnsFalseForClosedStages()
    {
        // Arrange
        var wonOpp = new Opportunity { Stage = OpportunityStage.ClosedWon };
        var lostOpp = new Opportunity { Stage = OpportunityStage.ClosedLost };

        // Assert
        wonOpp.IsOpen.Should().BeFalse();
        lostOpp.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Opportunity_IsWon_ReturnsCorrectly()
    {
        // Arrange
        var wonOpp = new Opportunity { Stage = OpportunityStage.ClosedWon };
        var lostOpp = new Opportunity { Stage = OpportunityStage.ClosedLost };
        var openOpp = new Opportunity { Stage = OpportunityStage.Discovery };

        // Assert
        wonOpp.IsWon.Should().BeTrue();
        lostOpp.IsWon.Should().BeFalse();
        openOpp.IsWon.Should().BeFalse();
    }

    [Fact]
    public void Opportunity_WeightedAmount_CalculatesCorrectly()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000m,
            Probability = 50
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(50000m);
    }

    [Fact]
    public void Opportunity_WeightedAmount_ZeroProbability_ReturnsZero()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000m,
            Probability = 0
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(0m);
    }

    [Fact]
    public void Opportunity_WeightedAmount_FullProbability_ReturnsFullAmount()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000m,
            Probability = 100
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(100000m);
    }

    #endregion

    #region Lead Entity Tests

    [Fact]
    public void Lead_CanBeCreated_WithDefaults()
    {
        // Act
        var lead = new Lead();

        // Assert
        lead.Id.Should().Be(0);
        lead.Status.Should().Be(LeadLifecycleStatus.New);
        lead.Source.Should().Be(LeadSource.Web);
        lead.Score.Should().Be(0);
    }

    [Fact]
    public void Lead_CanBeCreated_WithProperties()
    {
        // Act
        var lead = new Lead
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@company.com",
            Phone = "555-9876",
            CompanyName = "Smith Enterprises",
            Title = "CTO",
            Status = LeadLifecycleStatus.Qualified,
            Source = LeadSource.Campaign,
            Score = 80,
            FitScore = 40,
            EngagementScore = 40,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        lead.Id.Should().Be(1);
        lead.FirstName.Should().Be("Jane");
        lead.LastName.Should().Be("Smith");
        lead.Email.Should().Be("jane.smith@company.com");
        lead.CompanyName.Should().Be("Smith Enterprises");
        lead.Status.Should().Be(LeadLifecycleStatus.Qualified);
        lead.Source.Should().Be(LeadSource.Campaign);
        lead.Score.Should().Be(80);
    }

    [Fact]
    public void LeadLifecycleStatus_HasAllExpectedValues()
    {
        Enum.IsDefined(typeof(LeadLifecycleStatus), LeadLifecycleStatus.New).Should().BeTrue();
        Enum.IsDefined(typeof(LeadLifecycleStatus), LeadLifecycleStatus.Working).Should().BeTrue();
        Enum.IsDefined(typeof(LeadLifecycleStatus), LeadLifecycleStatus.Nurturing).Should().BeTrue();
        Enum.IsDefined(typeof(LeadLifecycleStatus), LeadLifecycleStatus.Qualified).Should().BeTrue();
        Enum.IsDefined(typeof(LeadLifecycleStatus), LeadLifecycleStatus.Disqualified).Should().BeTrue();
        Enum.IsDefined(typeof(LeadLifecycleStatus), LeadLifecycleStatus.Converted).Should().BeTrue();
    }

    [Fact]
    public void LeadSource_HasAllExpectedValues()
    {
        Enum.IsDefined(typeof(LeadSource), LeadSource.Web).Should().BeTrue();
        Enum.IsDefined(typeof(LeadSource), LeadSource.Campaign).Should().BeTrue();
        Enum.IsDefined(typeof(LeadSource), LeadSource.Referral).Should().BeTrue();
        Enum.IsDefined(typeof(LeadSource), LeadSource.Event).Should().BeTrue();
        Enum.IsDefined(typeof(LeadSource), LeadSource.Partner).Should().BeTrue();
        Enum.IsDefined(typeof(LeadSource), LeadSource.Manual).Should().BeTrue();
    }

    [Fact]
    public void Lead_IsOpen_ReturnsTrueForOpenStatuses()
    {
        // Arrange
        var newLead = new Lead { Status = LeadLifecycleStatus.New };
        var workingLead = new Lead { Status = LeadLifecycleStatus.Working };
        var nurturingLead = new Lead { Status = LeadLifecycleStatus.Nurturing };
        var qualifiedLead = new Lead { Status = LeadLifecycleStatus.Qualified };

        // Assert
        newLead.IsOpen.Should().BeTrue();
        workingLead.IsOpen.Should().BeTrue();
        nurturingLead.IsOpen.Should().BeTrue();
        qualifiedLead.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Lead_IsOpen_ReturnsFalseForClosedStatuses()
    {
        // Arrange
        var convertedLead = new Lead { Status = LeadLifecycleStatus.Converted };
        var disqualifiedLead = new Lead { Status = LeadLifecycleStatus.Disqualified };

        // Assert
        convertedLead.IsOpen.Should().BeFalse();
        disqualifiedLead.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Lead_FullName_ReturnsCorrectValue()
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
    public void Lead_FullName_HandlesNullValues()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = null,
            LastName = "Doe"
        };

        // Assert
        lead.FullName.Should().Be("Doe");
    }

    #endregion

    #region Product Entity Tests

    [Fact]
    public void Product_CanBeCreated_WithDefaults()
    {
        // Act
        var product = new Product();

        // Assert
        product.Id.Should().Be(0);
        product.Status.Should().Be(ProductStatus.Active); // Default is Active for immediate availability
        product.ProductType.Should().Be(ProductType.Physical);
    }

    [Fact]
    public void Product_CanBeCreated_WithProperties()
    {
        // Act
        var product = new Product
        {
            Id = 1,
            Name = "Enterprise CRM Suite",
            Description = "Complete CRM package for enterprises",
            SKU = "CRM-ENT-001",
            Price = 999.99m,
            Cost = 200m,
            ProductType = ProductType.Subscription,
            Status = ProductStatus.Active,
            Category = "Software",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        product.Id.Should().Be(1);
        product.Name.Should().Be("Enterprise CRM Suite");
        product.SKU.Should().Be("CRM-ENT-001");
        product.Price.Should().Be(999.99m);
        product.ProductType.Should().Be(ProductType.Subscription);
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void ProductStatus_HasAllExpectedValues()
    {
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.Draft).Should().BeTrue();
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.Active).Should().BeTrue();
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.Discontinued).Should().BeTrue();
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.OutOfStock).Should().BeTrue();
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.ComingSoon).Should().BeTrue();
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.Archived).Should().BeTrue();
    }

    [Fact]
    public void ProductType_HasAllExpectedValues()
    {
        Enum.IsDefined(typeof(ProductType), ProductType.Physical).Should().BeTrue();
        Enum.IsDefined(typeof(ProductType), ProductType.Digital).Should().BeTrue();
        Enum.IsDefined(typeof(ProductType), ProductType.Service).Should().BeTrue();
        Enum.IsDefined(typeof(ProductType), ProductType.Subscription).Should().BeTrue();
        Enum.IsDefined(typeof(ProductType), ProductType.Bundle).Should().BeTrue();
        Enum.IsDefined(typeof(ProductType), ProductType.Consulting).Should().BeTrue();
        Enum.IsDefined(typeof(ProductType), ProductType.Training).Should().BeTrue();
    }

    #endregion

    #region DTO Tests

    [Fact]
    public void LoginRequest_CanBeCreated_WithProperties()
    {
        // Act
        var request = new LoginRequest
        {
            Email = "user@company.com",
            Password = "password123"
        };

        // Assert
        request.Email.Should().Be("user@company.com");
        request.Password.Should().Be("password123");
    }

    [Fact]
    public void AccountDto_CanBeCreated_WithProperties()
    {
        // Act
        var dto = new AccountDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.FirstName.Should().Be("John");
        dto.LastName.Should().Be("Doe");
    }

    [Fact]
    public void UserDto_CanBeCreated_WithProperties()
    {
        // Act
        var dto = new UserDto
        {
            Id = 1,
            Username = "jdoe",
            Email = "john@company.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Sales",
            IsActive = true
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Username.Should().Be("jdoe");
        dto.Role.Should().Be("Sales");
    }

    #endregion

    #region SystemSettings Entity Tests

    [Fact]
    public void SystemSettings_CanBeCreated_WithDefaults()
    {
        // Act
        var settings = new SystemSettings();

        // Assert
        settings.Id.Should().Be(0);
        settings.CustomersEnabled.Should().BeTrue();
        settings.ContactsEnabled.Should().BeTrue();
        settings.LeadsEnabled.Should().BeTrue();
        settings.OpportunitiesEnabled.Should().BeTrue();
    }

    [Fact]
    public void SystemSettings_CanDisableModules()
    {
        // Act
        var settings = new SystemSettings
        {
            CustomersEnabled = false,
            LeadsEnabled = false
        };

        // Assert
        settings.CustomersEnabled.Should().BeFalse();
        settings.LeadsEnabled.Should().BeFalse();
        settings.ContactsEnabled.Should().BeTrue();
    }

    #endregion

    #region Department Entity Tests

    [Fact]
    public void Department_CanBeCreated_WithDefaults()
    {
        // Act
        var department = new Department();

        // Assert
        department.Id.Should().Be(0);
        department.Name.Should().BeEmpty();
        department.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Department_CanBeCreated_WithProperties()
    {
        // Act
        var department = new Department
        {
            Id = 1,
            Name = "Sales",
            Description = "Sales and Business Development",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        department.Id.Should().Be(1);
        department.Name.Should().Be("Sales");
        department.Description.Should().Be("Sales and Business Development");
        department.IsActive.Should().BeTrue();
    }

    #endregion

    #region UserGroup Entity Tests

    [Fact]
    public void UserGroup_CanBeCreated_WithDefaults()
    {
        // Act
        var group = new UserGroup();

        // Assert
        group.Id.Should().Be(0);
        group.Name.Should().BeEmpty();
        group.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UserGroup_CanBeCreated_WithProperties()
    {
        // Act
        var group = new UserGroup
        {
            Id = 1,
            Name = "Sales Team",
            Description = "Main sales team",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        group.Id.Should().Be(1);
        group.Name.Should().Be("Sales Team");
        group.Description.Should().Be("Main sales team");
        group.IsActive.Should().BeTrue();
    }

    #endregion
}
