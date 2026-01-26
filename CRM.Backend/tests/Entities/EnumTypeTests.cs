// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Comprehensive Enum and Type Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using System;
using System.Linq;

namespace CRM.Tests.Entities;

/// <summary>
/// Unit tests for all enums and type definitions
/// </summary>
public class EnumTypeTests
{
    #region CustomerCategory Tests

    [Theory]
    [InlineData(CustomerCategory.Individual, 0)]
    [InlineData(CustomerCategory.Organization, 1)]
    public void CustomerCategory_HasCorrectValues(CustomerCategory category, int expected)
    {
        ((int)category).Should().Be(expected);
    }

    [Fact]
    public void CustomerCategory_HasExpectedCount()
    {
        var values = Enum.GetValues<CustomerCategory>();
        values.Should().HaveCount(2);
    }

    [Fact]
    public void CustomerCategory_AllValuesAreDefined()
    {
        Enum.IsDefined(typeof(CustomerCategory), 0).Should().BeTrue();
        Enum.IsDefined(typeof(CustomerCategory), 1).Should().BeTrue();
    }

    #endregion

    #region CustomerType Tests

    [Fact]
    public void CustomerType_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<CustomerType>();
        values.Should().Contain(CustomerType.Individual);
        values.Should().Contain(CustomerType.SmallBusiness);
        values.Should().Contain(CustomerType.MidMarket);
        values.Should().Contain(CustomerType.Enterprise);
        values.Should().Contain(CustomerType.Government);
        values.Should().Contain(CustomerType.NonProfit);
    }

    [Fact]
    public void CustomerType_HasExpectedCount()
    {
        Enum.GetValues<CustomerType>().Should().HaveCount(6);
    }

    [Theory]
    [InlineData("Individual")]
    [InlineData("SmallBusiness")]
    [InlineData("MidMarket")]
    [InlineData("Enterprise")]
    [InlineData("Government")]
    [InlineData("NonProfit")]
    public void CustomerType_ParseFromString(string value)
    {
        var parsed = Enum.Parse<CustomerType>(value);
        parsed.ToString().Should().Be(value);
    }

    #endregion

    #region CustomerPriority Tests

    [Fact]
    public void CustomerPriority_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<CustomerPriority>();
        values.Should().Contain(CustomerPriority.Low);
        values.Should().Contain(CustomerPriority.Medium);
        values.Should().Contain(CustomerPriority.High);
        values.Should().Contain(CustomerPriority.Critical);
    }

    [Fact]
    public void CustomerPriority_HasExpectedCount()
    {
        Enum.GetValues<CustomerPriority>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(CustomerPriority.Low)]
    [InlineData(CustomerPriority.Medium)]
    [InlineData(CustomerPriority.High)]
    [InlineData(CustomerPriority.Critical)]
    public void CustomerPriority_CanBeCompared(CustomerPriority priority)
    {
        var all = Enum.GetValues<CustomerPriority>().ToList();
        all.Should().Contain(priority);
    }

    #endregion

    #region CustomerLifecycleStage Tests

    [Fact]
    public void CustomerLifecycleStage_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<CustomerLifecycleStage>();
        values.Should().Contain(CustomerLifecycleStage.Other);
        values.Should().Contain(CustomerLifecycleStage.Lead);
        values.Should().Contain(CustomerLifecycleStage.Opportunity);
        values.Should().Contain(CustomerLifecycleStage.Customer);
        values.Should().Contain(CustomerLifecycleStage.CustomerAtRisk);
        values.Should().Contain(CustomerLifecycleStage.Churned);
        values.Should().Contain(CustomerLifecycleStage.WinBack);
    }

    [Fact]
    public void CustomerLifecycleStage_HasExpectedCount()
    {
        Enum.GetValues<CustomerLifecycleStage>().Should().HaveCount(7);
    }

    #endregion

    #region AccountStatus Tests

    [Fact]
    public void AccountStatus_ContainsOnlyExpectedValues()
    {
        var values = Enum.GetValues<AccountStatus>();
        values.Should().Contain(AccountStatus.Current);
        values.Should().Contain(AccountStatus.Churned);
    }

    [Fact]
    public void AccountStatus_HasExpectedCount()
    {
        Enum.GetValues<AccountStatus>().Should().HaveCount(2);
    }

    [Theory]
    [InlineData("Current")]
    [InlineData("Churned")]
    public void AccountStatus_ParseFromString(string value)
    {
        var parsed = Enum.Parse<AccountStatus>(value);
        parsed.ToString().Should().Be(value);
    }

    #endregion

    #region LeadSource Tests

    [Fact]
    public void LeadSource_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<LeadSource>();
        values.Should().Contain(LeadSource.Web);
        values.Should().Contain(LeadSource.Campaign);
        values.Should().Contain(LeadSource.Referral);
        values.Should().Contain(LeadSource.Event);
        values.Should().Contain(LeadSource.Partner);
        values.Should().Contain(LeadSource.Manual);
    }

    [Fact]
    public void LeadSource_HasExpectedCount()
    {
        Enum.GetValues<LeadSource>().Should().HaveCount(6);
    }

    #endregion

    #region LeadLifecycleStatus Tests

    [Fact]
    public void LeadLifecycleStatus_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<LeadLifecycleStatus>();
        values.Should().Contain(LeadLifecycleStatus.New);
        values.Should().Contain(LeadLifecycleStatus.Working);
        values.Should().Contain(LeadLifecycleStatus.Qualified);
        values.Should().Contain(LeadLifecycleStatus.Nurturing);
        values.Should().Contain(LeadLifecycleStatus.Converted);
        values.Should().Contain(LeadLifecycleStatus.Disqualified);
    }

    [Fact]
    public void LeadLifecycleStatus_HasExpectedCount()
    {
        Enum.GetValues<LeadLifecycleStatus>().Should().HaveCount(6);
    }

    #endregion

    #region OpportunityStage Tests

    [Fact]
    public void OpportunityStage_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<OpportunityStage>();
        values.Should().Contain(OpportunityStage.Discovery);
        values.Should().Contain(OpportunityStage.Qualification);
        values.Should().Contain(OpportunityStage.Proposal);
        values.Should().Contain(OpportunityStage.Negotiation);
        values.Should().Contain(OpportunityStage.ClosedWon);
        values.Should().Contain(OpportunityStage.ClosedLost);
    }

    [Fact]
    public void OpportunityStage_HasExpectedCount()
    {
        Enum.GetValues<OpportunityStage>().Should().HaveCount(6);
    }

    [Fact]
    public void OpportunityStage_ClosedStages()
    {
        // Verify closed stages
        OpportunityStage.ClosedWon.ToString().Should().Contain("Closed");
        OpportunityStage.ClosedLost.ToString().Should().Contain("Closed");
    }

    #endregion

    #region ProductType Tests

    [Fact]
    public void ProductType_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<ProductType>();
        values.Should().Contain(ProductType.Subscription);
        values.Should().Contain(ProductType.Physical);
        values.Should().Contain(ProductType.Digital);
        values.Should().Contain(ProductType.Service);
    }

    [Fact]
    public void ProductType_HasExpectedCount()
    {
        Enum.GetValues<ProductType>().Should().HaveCountGreaterThanOrEqualTo(4);
    }

    #endregion

    #region ProductStatus Tests

    [Fact]
    public void ProductStatus_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<ProductStatus>();
        values.Should().Contain(ProductStatus.Active);
        values.Should().Contain(ProductStatus.Draft);
        values.Should().Contain(ProductStatus.Discontinued);
    }

    [Fact]
    public void ProductStatus_HasExpectedCount()
    {
        Enum.GetValues<ProductStatus>().Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion

    #region UserRole Tests

    [Theory]
    [InlineData(UserRole.Admin, 0)]
    [InlineData(UserRole.Manager, 1)]
    [InlineData(UserRole.Sales, 2)]
    public void UserRole_HasCorrectValues(UserRole role, int expected)
    {
        ((int)role).Should().Be(expected);
    }

    [Fact]
    public void UserRole_HasExpectedCount()
    {
        Enum.GetValues<UserRole>().Should().HaveCountGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void UserRole_AdminHasLowestValue()
    {
        // Admin is 0, which is the lowest value
        ((int)UserRole.Admin).Should().Be(0);
    }

    #endregion

    #region Enum Conversion Tests

    [Theory]
    [InlineData("Web", LeadSource.Web)]
    [InlineData("Campaign", LeadSource.Campaign)]
    [InlineData("Referral", LeadSource.Referral)]
    [InlineData("Event", LeadSource.Event)]
    [InlineData("Partner", LeadSource.Partner)]
    [InlineData("Manual", LeadSource.Manual)]
    public void LeadSource_CanParseCaseInsensitive(string input, LeadSource expected)
    {
        var parsed = Enum.Parse<LeadSource>(input, ignoreCase: true);
        parsed.Should().Be(expected);
    }

    [Fact]
    public void Enum_InvalidParse_ThrowsException()
    {
        Action act = () => Enum.Parse<LeadSource>("InvalidValue");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Enum_TryParse_ReturnsFalseForInvalid()
    {
        var result = Enum.TryParse<CustomerType>("InvalidType", out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void Enum_TryParse_ReturnsTrueForValid()
    {
        var result = Enum.TryParse<CustomerType>("Enterprise", out var value);
        result.Should().BeTrue();
        value.Should().Be(CustomerType.Enterprise);
    }

    #endregion

    #region Enum to String Tests

    [Theory]
    [InlineData(OpportunityStage.Discovery, "Discovery")]
    [InlineData(OpportunityStage.Qualification, "Qualification")]
    [InlineData(OpportunityStage.Proposal, "Proposal")]
    [InlineData(OpportunityStage.Negotiation, "Negotiation")]
    [InlineData(OpportunityStage.ClosedWon, "ClosedWon")]
    [InlineData(OpportunityStage.ClosedLost, "ClosedLost")]
    public void OpportunityStage_ToString_ReturnsCorrectValue(OpportunityStage stage, string expected)
    {
        stage.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData(CustomerLifecycleStage.Other, "Other")]
    [InlineData(CustomerLifecycleStage.Lead, "Lead")]
    [InlineData(CustomerLifecycleStage.Customer, "Customer")]
    [InlineData(CustomerLifecycleStage.CustomerAtRisk, "CustomerAtRisk")]
    [InlineData(CustomerLifecycleStage.Churned, "Churned")]
    [InlineData(CustomerLifecycleStage.WinBack, "WinBack")]
    public void CustomerLifecycleStage_ToString_ReturnsCorrectValue(CustomerLifecycleStage stage, string expected)
    {
        stage.ToString().Should().Be(expected);
    }

    #endregion
}
