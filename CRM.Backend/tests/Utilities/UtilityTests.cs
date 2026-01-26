// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Utility and Helper Function Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.Utilities;

/// <summary>
/// Tests for utility functions and helpers
/// </summary>
public class UtilityTests
{
    #region Email Validation Tests

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.org", true)]
    [InlineData("user+tag@company.com", true)]
    [InlineData("user@subdomain.domain.com", true)]
    [InlineData("", false)]
    [InlineData("invalid", false)]
    [InlineData("@domain.com", false)]
    [InlineData("user@", false)]
    public void Email_ValidationPattern(string email, bool expected)
    {
        // Simple email validation
        var isValid = !string.IsNullOrEmpty(email) && 
                      email.Contains("@") && 
                      email.IndexOf("@") > 0 &&
                      email.IndexOf("@") < email.Length - 1;
        isValid.Should().Be(expected);
    }

    #endregion

    #region Phone Formatting Tests

    [Theory]
    [InlineData("5551234567", "555-123-4567")]
    [InlineData("1234567890", "123-456-7890")]
    public void Phone_Formatting_USFormat(string input, string expected)
    {
        // Simple phone formatting
        if (input.Length == 10)
        {
            var formatted = $"{input[..3]}-{input[3..6]}-{input[6..]}";
            formatted.Should().Be(expected);
        }
    }

    [Fact]
    public void Phone_Formatting_WithDashes()
    {
        var phone = "555-123-4567";
        phone.Replace("-", "").Should().Be("5551234567");
    }

    #endregion

    #region Currency Formatting Tests

    [Theory]
    [InlineData(1000, "1,000.00")]
    [InlineData(100000, "100,000.00")]
    [InlineData(1234567.89, "1,234,567.89")]
    public void Currency_Formatting(decimal amount, string expected)
    {
        var formatted = amount.ToString("N2");
        formatted.Should().Be(expected);
    }

    [Fact]
    public void Currency_RoundingDown()
    {
        var amount = 99.994m;
        var rounded = Math.Round(amount, 2);
        rounded.Should().Be(99.99m);
    }

    [Fact]
    public void Currency_RoundingUp()
    {
        var amount = 99.995m;
        var rounded = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        rounded.Should().Be(100.00m);
    }

    #endregion

    #region Date/Time Formatting Tests

    [Fact]
    public void Date_ISO8601Format()
    {
        var date = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        var formatted = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
        formatted.Should().Be("2024-06-15T10:30:00Z");
    }

    [Fact]
    public void Date_ShortFormat()
    {
        var date = new DateTime(2024, 6, 15);
        var formatted = date.ToString("MM/dd/yyyy");
        formatted.Should().Be("06/15/2024");
    }

    [Fact]
    public void Date_RelativeTime_Today()
    {
        var now = DateTime.UtcNow;
        var difference = DateTime.UtcNow - now;
        difference.TotalSeconds.Should().BeLessThan(1);
    }

    [Fact]
    public void Date_DaysUntil_Future()
    {
        var future = DateTime.UtcNow.AddDays(30);
        var daysUntil = (future - DateTime.UtcNow).Days;
        daysUntil.Should().BeInRange(29, 31);
    }

    [Fact]
    public void Date_DaysSince_Past()
    {
        var past = DateTime.UtcNow.AddDays(-30);
        var daysSince = (DateTime.UtcNow - past).Days;
        daysSince.Should().BeInRange(29, 31);
    }

    #endregion

    #region Percentage Calculations

    [Theory]
    [InlineData(50, 200, 25)]
    [InlineData(100, 100, 100)]
    [InlineData(25, 100, 25)]
    [InlineData(0, 100, 0)]
    public void Percentage_Calculation(decimal part, decimal whole, decimal expected)
    {
        var percentage = (part / whole) * 100;
        percentage.Should().Be(expected);
    }

    [Fact]
    public void Percentage_WinRate()
    {
        var won = 25;
        var total = 100;
        var winRate = (decimal)won / total * 100;
        winRate.Should().Be(25m);
    }

    [Fact]
    public void Percentage_ConversionRate()
    {
        var converted = 10;
        var leads = 50;
        var conversionRate = (decimal)converted / leads * 100;
        conversionRate.Should().Be(20m);
    }

    #endregion

    #region String Manipulation Tests

    [Fact]
    public void String_FullName_Concatenation()
    {
        var firstName = "John";
        var lastName = "Doe";
        var fullName = $"{firstName} {lastName}";
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void String_FullName_WithMiddle()
    {
        var firstName = "John";
        var middleName = "Michael";
        var lastName = "Doe";
        var fullName = $"{firstName} {middleName} {lastName}";
        fullName.Should().Be("John Michael Doe");
    }

    [Fact]
    public void String_Initials()
    {
        var firstName = "John";
        var lastName = "Doe";
        var initials = $"{firstName[0]}{lastName[0]}";
        initials.Should().Be("JD");
    }

    [Fact]
    public void String_Truncation()
    {
        var text = "This is a very long description that needs to be truncated";
        var maxLength = 20;
        var truncated = text.Length > maxLength ? text[..maxLength] + "..." : text;
        truncated.Should().HaveLength(23);
        truncated.Should().EndWith("...");
    }

    #endregion

    #region Collection Operations

    [Fact]
    public void Collection_Filtering_OpenOpportunities()
    {
        var opportunities = new List<Opportunity>
        {
            new() { Stage = OpportunityStage.Discovery },
            new() { Stage = OpportunityStage.Proposal },
            new() { Stage = OpportunityStage.ClosedWon },
            new() { Stage = OpportunityStage.ClosedLost }
        };

        var openOpps = opportunities.Where(o => o.IsOpen).ToList();
        openOpps.Should().HaveCount(2);
    }

    [Fact]
    public void Collection_Sum_TotalAmount()
    {
        var opportunities = new List<Opportunity>
        {
            new() { Amount = 10000m },
            new() { Amount = 25000m },
            new() { Amount = 15000m }
        };

        var total = opportunities.Sum(o => o.Amount);
        total.Should().Be(50000m);
    }

    [Fact]
    public void Collection_Average_Score()
    {
        var leads = new List<Lead>
        {
            new() { Score = 80 },
            new() { Score = 60 },
            new() { Score = 70 }
        };

        var avgScore = leads.Average(l => l.Score);
        avgScore.Should().Be(70);
    }

    [Fact]
    public void Collection_GroupBy_Status()
    {
        var leads = new List<Lead>
        {
            new() { Status = LeadLifecycleStatus.New },
            new() { Status = LeadLifecycleStatus.New },
            new() { Status = LeadLifecycleStatus.Qualified },
            new() { Status = LeadLifecycleStatus.Converted }
        };

        var grouped = leads.GroupBy(l => l.Status)
                          .ToDictionary(g => g.Key, g => g.Count());

        grouped[LeadLifecycleStatus.New].Should().Be(2);
        grouped[LeadLifecycleStatus.Qualified].Should().Be(1);
    }

    [Fact]
    public void Collection_OrderBy_Amount()
    {
        var opportunities = new List<Opportunity>
        {
            new() { Amount = 25000m },
            new() { Amount = 10000m },
            new() { Amount = 50000m }
        };

        var ordered = opportunities.OrderByDescending(o => o.Amount).ToList();
        ordered[0].Amount.Should().Be(50000m);
        ordered[2].Amount.Should().Be(10000m);
    }

    #endregion

    #region SKU/Account Number Generation

    [Fact]
    public void SKU_Generation_Pattern()
    {
        var category = "PROD";
        var id = 123;
        var sku = $"{category}-{id:D6}";
        sku.Should().Be("PROD-000123");
    }

    [Fact]
    public void AccountNumber_Generation_Pattern()
    {
        var prefix = "ACC";
        var id = 456;
        var year = DateTime.UtcNow.Year;
        var accountNumber = $"{prefix}-{year}-{id:D5}";
        accountNumber.Should().StartWith($"ACC-{year}-");
    }

    #endregion

    #region Revenue Calculations

    [Fact]
    public void MRR_ToARR_Conversion()
    {
        var mrr = 5000m;
        var arr = mrr * 12;
        arr.Should().Be(60000m);
    }

    [Fact]
    public void ARR_ToMRR_Conversion()
    {
        var arr = 120000m;
        var mrr = arr / 12;
        mrr.Should().Be(10000m);
    }

    [Fact]
    public void TotalContractValue_Calculation()
    {
        var mrr = 5000m;
        var contractMonths = 24;
        var setupFee = 10000m;
        var tcv = (mrr * contractMonths) + setupFee;
        tcv.Should().Be(130000m);
    }

    [Fact]
    public void LTV_Calculation()
    {
        var mrr = 1000m;
        var avgLifetimeMonths = 36;
        var ltv = mrr * avgLifetimeMonths;
        ltv.Should().Be(36000m);
    }

    #endregion

    #region Date Range Calculations

    [Fact]
    public void ContractDuration_Months()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2025, 1, 1);
        var months = ((end.Year - start.Year) * 12) + end.Month - start.Month;
        months.Should().Be(12);
    }

    [Fact]
    public void ContractDuration_Days()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);
        var days = (end - start).Days;
        days.Should().Be(365);
    }

    [Fact]
    public void IsWithinRange_True()
    {
        var date = new DateTime(2024, 6, 15);
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);
        var isWithin = date >= start && date <= end;
        isWithin.Should().BeTrue();
    }

    [Fact]
    public void IsWithinRange_False()
    {
        var date = new DateTime(2025, 1, 15);
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);
        var isWithin = date >= start && date <= end;
        isWithin.Should().BeFalse();
    }

    #endregion

    #region Status Transitions

    [Fact]
    public void Lead_CanTransition_NewToWorking()
    {
        var lead = new Lead { Status = LeadLifecycleStatus.New };
        lead.Status = LeadLifecycleStatus.Working;
        lead.Status.Should().Be(LeadLifecycleStatus.Working);
    }

    [Fact]
    public void Opportunity_CanTransition_DiscoveryToProposal()
    {
        var opp = new Opportunity { Stage = OpportunityStage.Discovery };
        opp.Stage = OpportunityStage.Proposal;
        opp.Stage.Should().Be(OpportunityStage.Proposal);
    }

    [Fact]
    public void Account_CanTransition_CurrentToChurned()
    {
        var account = new Account { Status = AccountStatus.Current };
        account.Status = AccountStatus.Churned;
        account.Status.Should().Be(AccountStatus.Churned);
    }

    #endregion
}
