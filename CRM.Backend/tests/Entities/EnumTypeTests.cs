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
    #region AccountCategory Tests

    [Theory]
    [InlineData(AccountCategory.Individual, 0)]
    [InlineData(AccountCategory.Organization, 1)]
    public void AccountCategory_HasCorrectValues(AccountCategory category, int expected)
    {
        ((int)category).Should().Be(expected);
    }

    [Fact]
    public void AccountCategory_HasExpectedCount()
    {
        var values = Enum.GetValues<AccountCategory>();
        values.Should().HaveCount(2);
    }

    [Fact]
    public void AccountCategory_AllValuesAreDefined()
    {
        Enum.IsDefined(typeof(AccountCategory), 0).Should().BeTrue();
        Enum.IsDefined(typeof(AccountCategory), 1).Should().BeTrue();
    }

    #endregion

    #region AccountType Tests

    [Fact]
    public void AccountType_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<AccountType>();
        values.Should().Contain(AccountType.Individual);
        values.Should().Contain(AccountType.SmallBusiness);
        values.Should().Contain(AccountType.MidMarket);
        values.Should().Contain(AccountType.Enterprise);
        values.Should().Contain(AccountType.Government);
        values.Should().Contain(AccountType.NonProfit);
    }

    [Fact]
    public void AccountType_HasExpectedCount()
    {
        Enum.GetValues<AccountType>().Should().HaveCount(6);
    }

    [Theory]
    [InlineData("Individual")]
    [InlineData("SmallBusiness")]
    [InlineData("MidMarket")]
    [InlineData("Enterprise")]
    [InlineData("Government")]
    [InlineData("NonProfit")]
    public void AccountType_ParseFromString(string value)
    {
        var parsed = Enum.Parse<AccountType>(value);
        parsed.ToString().Should().Be(value);
    }

    #endregion

    #region AccountPriority Tests

    [Fact]
    public void AccountPriority_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<AccountPriority>();
        values.Should().Contain(AccountPriority.Low);
        values.Should().Contain(AccountPriority.Medium);
        values.Should().Contain(AccountPriority.High);
        values.Should().Contain(AccountPriority.Critical);
    }

    [Fact]
    public void AccountPriority_HasExpectedCount()
    {
        Enum.GetValues<AccountPriority>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(AccountPriority.Low)]
    [InlineData(AccountPriority.Medium)]
    [InlineData(AccountPriority.High)]
    [InlineData(AccountPriority.Critical)]
    public void AccountPriority_CanBeCompared(AccountPriority priority)
    {
        var all = Enum.GetValues<AccountPriority>().ToList();
        all.Should().Contain(priority);
    }

    #endregion

    #region AccountLifecycleStage Tests

    [Fact]
    public void AccountLifecycleStage_ContainsAllExpectedValues()
    {
        var values = Enum.GetValues<AccountLifecycleStage>();
        values.Should().Contain(AccountLifecycleStage.Other);
        values.Should().Contain(AccountLifecycleStage.Lead);
        values.Should().Contain(AccountLifecycleStage.Opportunity);
        values.Should().Contain(AccountLifecycleStage.Active);
        values.Should().Contain(AccountLifecycleStage.AtRisk);
        values.Should().Contain(AccountLifecycleStage.Churned);
        values.Should().Contain(AccountLifecycleStage.WinBack);
    }

    [Fact]
    public void AccountLifecycleStage_HasExpectedCount()
    {
        Enum.GetValues<AccountLifecycleStage>().Should().HaveCount(7);
    }

    #endregion

    #region SubscriptionStatus Tests

    [Fact]
    public void SubscriptionStatus_ContainsOnlyExpectedValues()
    {
        var values = Enum.GetValues<SubscriptionStatus>();
        values.Should().Contain(SubscriptionStatus.Current);
        values.Should().Contain(SubscriptionStatus.Churned);
    }

    [Fact]
    public void SubscriptionStatus_HasExpectedCount()
    {
        // 9 named values: Active, Paused, Cancelled, Suspended, PendingCancellation, Expired, Trial + aliases Current (=Active), Churned (=Cancelled)
        Enum.GetValues<SubscriptionStatus>().Should().HaveCount(9);
        Enum.GetValues<SubscriptionStatus>().Distinct().Should().HaveCount(7);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Paused")]
    [InlineData("Cancelled")]
    [InlineData("Suspended")]
    [InlineData("PendingCancellation")]
    [InlineData("Expired")]
    [InlineData("Trial")]
    public void SubscriptionStatus_ParseFromString(string value)
    {
        var parsed = Enum.Parse<SubscriptionStatus>(value);
        parsed.ToString().Should().Be(value);
    }

    [Fact]
    public void SubscriptionStatus_AliasesShouldParseCorrectly()
    {
        // "Current" is an alias for Active (both = 0)
        Enum.Parse<SubscriptionStatus>("Current").Should().Be(SubscriptionStatus.Active);
        // "Churned" is an alias for Cancelled (both = 2)
        Enum.Parse<SubscriptionStatus>("Churned").Should().Be(SubscriptionStatus.Cancelled);
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
        var result = Enum.TryParse<AccountType>("InvalidType", out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void Enum_TryParse_ReturnsTrueForValid()
    {
        var result = Enum.TryParse<AccountType>("Enterprise", out var value);
        result.Should().BeTrue();
        value.Should().Be(AccountType.Enterprise);
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
    [InlineData(AccountLifecycleStage.Other, "Other")]
    [InlineData(AccountLifecycleStage.Lead, "Lead")]
    [InlineData(AccountLifecycleStage.Active, "Active")]
    [InlineData(AccountLifecycleStage.AtRisk, "AtRisk")]
    [InlineData(AccountLifecycleStage.Churned, "Churned")]
    [InlineData(AccountLifecycleStage.WinBack, "WinBack")]
    public void AccountLifecycleStage_ToString_ReturnsCorrectValue(AccountLifecycleStage stage, string expected)
    {
        stage.ToString().Should().Be(expected);
    }

    #endregion
}
