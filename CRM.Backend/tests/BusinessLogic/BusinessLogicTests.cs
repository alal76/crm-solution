// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Business Logic Tests for Entities

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.BusinessLogic;

/// <summary>
/// Business logic and computation tests
/// </summary>
public class BusinessLogicTests
{
    #region Opportunity Calculations

    [Fact]
    public void Opportunity_WeightedAmount_CorrectCalculation()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000m,
            Probability = 25
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(25000m);
    }

    [Fact]
    public void Opportunity_WeightedAmount_WithZeroProbability()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000m,
            Probability = 0
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(0);
    }

    [Fact]
    public void Opportunity_WeightedAmount_WithFullProbability()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 50000m,
            Probability = 100
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(50000m);
    }

    [Theory]
    [InlineData(10000, 10, 1000)]
    [InlineData(25000, 40, 10000)]
    [InlineData(50000, 50, 25000)]
    [InlineData(100000, 75, 75000)]
    [InlineData(200000, 90, 180000)]
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

    #endregion

    #region Opportunity Stage Logic

    [Fact]
    public void Opportunity_IsOpen_Discovery()
    {
        var opp = new Opportunity { Stage = OpportunityStage.Discovery };
        opp.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_IsOpen_Qualification()
    {
        var opp = new Opportunity { Stage = OpportunityStage.Qualification };
        opp.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_IsOpen_Proposal()
    {
        var opp = new Opportunity { Stage = OpportunityStage.Proposal };
        opp.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_IsOpen_Negotiation()
    {
        var opp = new Opportunity { Stage = OpportunityStage.Negotiation };
        opp.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_IsNotOpen_ClosedWon()
    {
        var opp = new Opportunity { Stage = OpportunityStage.ClosedWon };
        opp.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Opportunity_IsNotOpen_ClosedLost()
    {
        var opp = new Opportunity { Stage = OpportunityStage.ClosedLost };
        opp.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Opportunity_IsWon_ClosedWon()
    {
        var opp = new Opportunity { Stage = OpportunityStage.ClosedWon };
        opp.IsWon.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_IsWon_ClosedLost()
    {
        var opp = new Opportunity { Stage = OpportunityStage.ClosedLost };
        opp.IsWon.Should().BeFalse();
    }

    [Fact]
    public void Opportunity_IsWon_OpenStage()
    {
        var opp = new Opportunity { Stage = OpportunityStage.Proposal };
        opp.IsWon.Should().BeFalse();
    }

    #endregion

    #region Lead Logic

    [Fact]
    public void Lead_FullName_Computed()
    {
        var lead = new Lead { FirstName = "John", LastName = "Doe" };
        lead.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void Lead_IsOpen_New()
    {
        var lead = new Lead { Status = LeadLifecycleStatus.New };
        lead.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Lead_IsOpen_Working()
    {
        var lead = new Lead { Status = LeadLifecycleStatus.Working };
        lead.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Lead_IsOpen_Qualified()
    {
        var lead = new Lead { Status = LeadLifecycleStatus.Qualified };
        lead.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Lead_IsOpen_Nurturing()
    {
        var lead = new Lead { Status = LeadLifecycleStatus.Nurturing };
        lead.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Lead_IsNotOpen_Converted()
    {
        var lead = new Lead { Status = LeadLifecycleStatus.Converted };
        lead.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Lead_IsNotOpen_Disqualified()
    {
        var lead = new Lead { Status = LeadLifecycleStatus.Disqualified };
        lead.IsOpen.Should().BeFalse();
    }

    #endregion

    #region Lead Scoring

    [Fact]
    public void Lead_Score_DefaultIsZero()
    {
        var lead = new Lead();
        lead.Score.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public void Lead_Score_ValidRange(int score)
    {
        var lead = new Lead { Score = score };
        lead.Score.Should().Be(score);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Lead_FitScore_ValidRange(int score)
    {
        var lead = new Lead { FitScore = score };
        lead.FitScore.Should().Be(score);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Lead_EngagementScore_ValidRange(int score)
    {
        var lead = new Lead { EngagementScore = score };
        lead.EngagementScore.Should().Be(score);
    }

    #endregion

    #region Product Logic

    [Fact]
    public void Product_IsRecurring_Subscription()
    {
        var product = new Product
        {
            ProductType = ProductType.Subscription,
            BillingFrequency = BillingFrequency.Monthly
        };
        product.IsRecurring.Should().BeTrue();
    }

    [Fact]
    public void Product_IsNotRecurring_OneTime()
    {
        var product = new Product
        {
            ProductType = ProductType.Physical,
            BillingFrequency = BillingFrequency.OneTime
        };
        product.IsRecurring.Should().BeFalse();
    }

    #endregion

    #region Account Revenue Calculations

    [Fact]
    public void Account_MRR_CanBeSet()
    {
        var account = new Account { MRR = 1500m };
        account.MRR.Should().Be(1500m);
    }

    [Fact]
    public void Account_ARR_CanBeSet()
    {
        var account = new Account { ARR = 18000m };
        account.ARR.Should().Be(18000m);
    }

    [Fact]
    public void Account_MRRTimesARR_Relationship()
    {
        var account = new Account
        {
            MRR = 1000m,
            ARR = 12000m
        };
        account.ARR.Should().Be(account.MRR * 12);
    }

    [Fact]
    public void Account_OneTimeFee_CanBeSet()
    {
        var account = new Account { OneTimeFee = 5000m };
        account.OneTimeFee.Should().Be(5000m);
    }

    #endregion

    #region Customer Lifecycle

    [Theory]
    [InlineData(CustomerLifecycleStage.Lead)]
    [InlineData(CustomerLifecycleStage.Opportunity)]
    [InlineData(CustomerLifecycleStage.Customer)]
    [InlineData(CustomerLifecycleStage.CustomerAtRisk)]
    [InlineData(CustomerLifecycleStage.Churned)]
    [InlineData(CustomerLifecycleStage.WinBack)]
    [InlineData(CustomerLifecycleStage.Other)]
    public void Customer_Lifecycle_AllStages(CustomerLifecycleStage stage)
    {
        var customer = new Customer { LifecycleStage = stage };
        customer.LifecycleStage.Should().Be(stage);
    }

    #endregion

    #region Customer Priority

    [Theory]
    [InlineData(CustomerPriority.Low)]
    [InlineData(CustomerPriority.Medium)]
    [InlineData(CustomerPriority.High)]
    [InlineData(CustomerPriority.Critical)]
    public void Customer_Priority_AllLevels(CustomerPriority priority)
    {
        var customer = new Customer { Priority = priority };
        customer.Priority.Should().Be(priority);
    }

    #endregion

    #region Pipeline Value Calculations

    [Fact]
    public void Pipeline_TotalValue_Calculation()
    {
        var opportunities = new List<Opportunity>
        {
            new() { Amount = 10000m, Probability = 100 },
            new() { Amount = 20000m, Probability = 50 },
            new() { Amount = 30000m, Probability = 25 }
        };

        var totalWeightedValue = opportunities.Sum(o => o.WeightedAmount);
        // 10000 + 10000 + 7500 = 27500
        totalWeightedValue.Should().Be(27500m);
    }

    [Fact]
    public void Pipeline_TotalAmount_WithFiltering()
    {
        var opportunities = new List<Opportunity>
        {
            new() { Stage = OpportunityStage.Discovery, Amount = 10000m, Probability = 20 },
            new() { Stage = OpportunityStage.Proposal, Amount = 25000m, Probability = 60 },
            new() { Stage = OpportunityStage.ClosedWon, Amount = 15000m, Probability = 100 },
            new() { Stage = OpportunityStage.ClosedLost, Amount = 20000m, Probability = 0 }
        };

        var openOpportunities = opportunities.Where(o => o.IsOpen);
        var openValue = openOpportunities.Sum(o => o.Amount);
        openValue.Should().Be(35000m); // 10000 + 25000
    }

    [Fact]
    public void Pipeline_WonDeals_Calculation()
    {
        var opportunities = new List<Opportunity>
        {
            new() { Stage = OpportunityStage.ClosedWon, Amount = 10000m },
            new() { Stage = OpportunityStage.ClosedWon, Amount = 25000m },
            new() { Stage = OpportunityStage.ClosedLost, Amount = 15000m },
            new() { Stage = OpportunityStage.Proposal, Amount = 20000m }
        };

        var wonValue = opportunities.Where(o => o.IsWon).Sum(o => o.Amount);
        wonValue.Should().Be(35000m);
    }

    #endregion

    #region Lead Source Analysis

    [Theory]
    [InlineData(LeadSource.Web)]
    [InlineData(LeadSource.Campaign)]
    [InlineData(LeadSource.Referral)]
    [InlineData(LeadSource.Event)]
    [InlineData(LeadSource.Partner)]
    [InlineData(LeadSource.Manual)]
    public void Lead_Source_AllSources(LeadSource source)
    {
        var lead = new Lead { Source = source };
        lead.Source.Should().Be(source);
    }

    [Fact]
    public void Lead_GroupBySource_Analysis()
    {
        var leads = new List<Lead>
        {
            new() { Source = LeadSource.Web, Score = 80 },
            new() { Source = LeadSource.Web, Score = 60 },
            new() { Source = LeadSource.Campaign, Score = 70 },
            new() { Source = LeadSource.Referral, Score = 90 }
        };

        var webLeads = leads.Where(l => l.Source == LeadSource.Web);
        webLeads.Should().HaveCount(2);

        var avgWebScore = webLeads.Average(l => l.Score);
        avgWebScore.Should().Be(70);
    }

    #endregion

    #region Account Status Logic

    [Fact]
    public void Account_Status_Current()
    {
        var account = new Account { Status = AccountStatus.Current };
        account.Status.Should().Be(AccountStatus.Current);
    }

    [Fact]
    public void Account_Status_Churned()
    {
        var account = new Account { Status = AccountStatus.Churned };
        account.Status.Should().Be(AccountStatus.Churned);
    }

    #endregion

    #region Contract Date Logic

    [Fact]
    public void Account_ContractDates_Valid()
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddYears(1);

        var account = new Account
        {
            ContractStartDate = startDate,
            ContractEndDate = endDate
        };

        (account.ContractEndDate!.Value - account.ContractStartDate!.Value).TotalDays.Should().BeApproximately(365, 1);
    }

    [Fact]
    public void Account_IsAutoRenew_CanBeSet()
    {
        var account = new Account { IsAutoRenew = true };
        account.IsAutoRenew.Should().BeTrue();
    }

    #endregion
}
