// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

#region Commission Enum Tests

public class CommissionTypeEnumTests
{
    [Fact]
    public void CommissionType_ShouldHave6Values()
    {
        // Assert
        Enum.GetValues<CommissionType>().Should().HaveCount(6);
    }

    [Theory]
    [InlineData(CommissionType.FlatPercentage, 0)]
    [InlineData(CommissionType.TieredPercentage, 1)]
    [InlineData(CommissionType.FixedAmount, 2)]
    [InlineData(CommissionType.TieredAmount, 3)]
    [InlineData(CommissionType.MarginBased, 4)]
    [InlineData(CommissionType.Custom, 5)]
    public void CommissionType_ShouldHaveCorrectIntValue(CommissionType type, int expected)
    {
        // Assert
        ((int)type).Should().Be(expected);
    }
}

public class CommissionTriggerEnumTests
{
    [Fact]
    public void CommissionTrigger_ShouldHave7Values()
    {
        // Assert
        Enum.GetValues<CommissionTrigger>().Should().HaveCount(7);
    }

    [Theory]
    [InlineData(CommissionTrigger.OnClose, 0)]
    [InlineData(CommissionTrigger.OnOrder, 1)]
    [InlineData(CommissionTrigger.OnInvoice, 2)]
    [InlineData(CommissionTrigger.OnPayment, 3)]
    [InlineData(CommissionTrigger.OnSubscriptionStart, 4)]
    [InlineData(CommissionTrigger.OnSignature, 5)]
    [InlineData(CommissionTrigger.Monthly, 6)]
    public void CommissionTrigger_ShouldHaveCorrectIntValue(CommissionTrigger trigger, int expected)
    {
        // Assert
        ((int)trigger).Should().Be(expected);
    }
}

public class CommissionStatusEnumTests
{
    [Fact]
    public void CommissionStatus_ShouldHave7Values()
    {
        // Assert
        Enum.GetValues<CommissionStatus>().Should().HaveCount(7);
    }

    [Theory]
    [InlineData(CommissionStatus.Pending, 0)]
    [InlineData(CommissionStatus.Approved, 1)]
    [InlineData(CommissionStatus.Held, 2)]
    [InlineData(CommissionStatus.Paid, 3)]
    [InlineData(CommissionStatus.ClawedBack, 4)]
    [InlineData(CommissionStatus.Adjusted, 5)]
    [InlineData(CommissionStatus.Cancelled, 6)]
    public void CommissionStatus_ShouldHaveCorrectIntValue(CommissionStatus status, int expected)
    {
        // Assert
        ((int)status).Should().Be(expected);
    }
}

public class CommissionPlanStatusEnumTests
{
    [Fact]
    public void CommissionPlanStatus_ShouldHave4Values()
    {
        // Assert
        Enum.GetValues<CommissionPlanStatus>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(CommissionPlanStatus.Draft, 0)]
    [InlineData(CommissionPlanStatus.Active, 1)]
    [InlineData(CommissionPlanStatus.Inactive, 2)]
    [InlineData(CommissionPlanStatus.Archived, 3)]
    public void CommissionPlanStatus_ShouldHaveCorrectIntValue(CommissionPlanStatus status, int expected)
    {
        // Assert
        ((int)status).Should().Be(expected);
    }
}

#endregion

#region CommissionPlan Entity Tests

public class CommissionPlanEntityTests
{
    [Fact]
    public void CommissionPlan_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var plan = new CommissionPlan();

        // Assert
        plan.Name.Should().BeEmpty();
        plan.Code.Should().BeNull();
        plan.Description.Should().BeNull();
        plan.Status.Should().Be(CommissionPlanStatus.Draft);
        plan.CommissionType.Should().Be(CommissionType.FlatPercentage);
        plan.BaseRate.Should().Be(0);
        plan.Trigger.Should().Be(CommissionTrigger.OnClose);
        plan.ClawbackPeriodDays.Should().BeNull();
        plan.MinDealSize.Should().BeNull();
        plan.MaxCommissionPerDeal.Should().BeNull();
        plan.MaxCommissionPerPeriod.Should().BeNull();
        plan.AllowSplits.Should().BeTrue();
        plan.DefaultOverlayPercent.Should().BeNull();
        plan.ManagerOverridePercent.Should().BeNull();
        plan.TierRates.Should().BeNull();
        plan.AppliesToAllProducts.Should().BeTrue();
        plan.ProductCategories.Should().BeNull();
        plan.ProductIds.Should().BeNull();
        plan.ProductRates.Should().BeNull();
        plan.Tiers.Should().BeEmpty();
        plan.Assignments.Should().BeEmpty();
    }

    [Fact]
    public void CommissionPlan_ShouldSetIdentificationProperties()
    {
        // Arrange
        var plan = new CommissionPlan
        {
            Name = "Q1 2026 Sales Commission",
            Code = "COMM-2026-Q1",
            Description = "Quarterly commission plan for Q1 2026",
            Status = CommissionPlanStatus.Active
        };

        // Assert
        plan.Name.Should().Be("Q1 2026 Sales Commission");
        plan.Code.Should().Be("COMM-2026-Q1");
        plan.Description.Should().Be("Quarterly commission plan for Q1 2026");
        plan.Status.Should().Be(CommissionPlanStatus.Active);
    }

    [Fact]
    public void CommissionPlan_ShouldSetValidityPeriod()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 3, 31);

        var plan = new CommissionPlan
        {
            EffectiveStartDate = startDate,
            EffectiveEndDate = endDate,
            FiscalYear = 2026
        };

        // Assert
        plan.EffectiveStartDate.Should().Be(startDate);
        plan.EffectiveEndDate.Should().Be(endDate);
        plan.FiscalYear.Should().Be(2026);
    }

    [Fact]
    public void CommissionPlan_ShouldSetCommissionRules()
    {
        // Arrange
        var plan = new CommissionPlan
        {
            CommissionType = CommissionType.TieredPercentage,
            BaseRate = 5.0m,
            Trigger = CommissionTrigger.OnPayment,
            ClawbackPeriodDays = 90,
            MinDealSize = 1000m,
            MaxCommissionPerDeal = 5000m,
            MaxCommissionPerPeriod = 50000m
        };

        // Assert
        plan.CommissionType.Should().Be(CommissionType.TieredPercentage);
        plan.BaseRate.Should().Be(5.0m);
        plan.Trigger.Should().Be(CommissionTrigger.OnPayment);
        plan.ClawbackPeriodDays.Should().Be(90);
        plan.MinDealSize.Should().Be(1000m);
        plan.MaxCommissionPerDeal.Should().Be(5000m);
        plan.MaxCommissionPerPeriod.Should().Be(50000m);
    }

    [Fact]
    public void CommissionPlan_ShouldSetSplitSettings()
    {
        // Arrange
        var plan = new CommissionPlan
        {
            AllowSplits = true,
            DefaultOverlayPercent = 10m,
            ManagerOverridePercent = 5m
        };

        // Assert
        plan.AllowSplits.Should().BeTrue();
        plan.DefaultOverlayPercent.Should().Be(10m);
        plan.ManagerOverridePercent.Should().Be(5m);
    }

    [Fact]
    public void CommissionPlan_ShouldHaveTiersCollection()
    {
        // Arrange
        var plan = new CommissionPlan { Name = "Tiered Plan" };
        var tier1 = new CommissionTier { Name = "Tier 1", MinAttainmentPercent = 0, MaxAttainmentPercent = 100 };
        var tier2 = new CommissionTier { Name = "Tier 2", MinAttainmentPercent = 100, MaxAttainmentPercent = 150 };

        // Act
        plan.Tiers.Add(tier1);
        plan.Tiers.Add(tier2);

        // Assert
        plan.Tiers.Should().HaveCount(2);
    }

    [Fact]
    public void CommissionPlan_ShouldHaveAssignmentsCollection()
    {
        // Arrange
        var plan = new CommissionPlan { Name = "Sales Plan" };
        var assignment = new CommissionPlanAssignment { UserId = 1, IsActive = true };

        // Act
        plan.Assignments.Add(assignment);

        // Assert
        plan.Assignments.Should().HaveCount(1);
    }

    [Fact]
    public void CommissionPlan_ShouldInheritFromBaseEntity()
    {
        // Act
        var plan = new CommissionPlan();

        // Assert
        plan.Should().BeAssignableTo<BaseEntity>();
        plan.Id.Should().Be(0);
        plan.IsDeleted.Should().BeFalse();
    }
}

#endregion

#region CommissionTier Entity Tests

public class CommissionTierEntityTests
{
    [Fact]
    public void CommissionTier_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var tier = new CommissionTier();

        // Assert
        tier.Name.Should().BeEmpty();
        tier.TierOrder.Should().Be(0);
        tier.MinAttainmentPercent.Should().Be(0);
        tier.MaxAttainmentPercent.Should().BeNull();
        tier.CommissionRate.Should().Be(0);
        tier.FixedAmount.Should().BeNull();
        tier.Multiplier.Should().Be(1);
        tier.CommissionPlanId.Should().Be(0);
        tier.CommissionPlan.Should().BeNull();
    }

    [Fact]
    public void CommissionTier_ShouldSetAllProperties()
    {
        // Arrange
        var tier = new CommissionTier
        {
            Name = "Accelerator Tier",
            TierOrder = 3,
            MinAttainmentPercent = 100m,
            MaxAttainmentPercent = 150m,
            CommissionRate = 12m,
            FixedAmount = 500m,
            Multiplier = 1.5m,
            CommissionPlanId = 5
        };

        // Assert
        tier.Name.Should().Be("Accelerator Tier");
        tier.TierOrder.Should().Be(3);
        tier.MinAttainmentPercent.Should().Be(100m);
        tier.MaxAttainmentPercent.Should().Be(150m);
        tier.CommissionRate.Should().Be(12m);
        tier.FixedAmount.Should().Be(500m);
        tier.Multiplier.Should().Be(1.5m);
        tier.CommissionPlanId.Should().Be(5);
    }

    [Fact]
    public void CommissionTier_ShouldHaveNavigationToPlan()
    {
        // Arrange
        var plan = new CommissionPlan { Id = 1, Name = "Plan 1" };
        var tier = new CommissionTier
        {
            CommissionPlanId = 1,
            CommissionPlan = plan
        };

        // Assert
        tier.CommissionPlan.Should().NotBeNull();
        tier.CommissionPlan!.Name.Should().Be("Plan 1");
    }
}

#endregion

#region CommissionPlanAssignment Entity Tests

public class CommissionPlanAssignmentEntityTests
{
    [Fact]
    public void CommissionPlanAssignment_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var assignment = new CommissionPlanAssignment();

        // Assert
        assignment.UserId.Should().Be(0);
        assignment.User.Should().BeNull();
        assignment.CommissionPlanId.Should().Be(0);
        assignment.CommissionPlan.Should().BeNull();
        assignment.EndDate.Should().BeNull();
        assignment.RateOverride.Should().BeNull();
        assignment.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CommissionPlanAssignment_ShouldSetAllProperties()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 12, 31);

        var assignment = new CommissionPlanAssignment
        {
            UserId = 10,
            CommissionPlanId = 5,
            StartDate = startDate,
            EndDate = endDate,
            RateOverride = 8.5m,
            IsActive = true
        };

        // Assert
        assignment.UserId.Should().Be(10);
        assignment.CommissionPlanId.Should().Be(5);
        assignment.StartDate.Should().Be(startDate);
        assignment.EndDate.Should().Be(endDate);
        assignment.RateOverride.Should().Be(8.5m);
        assignment.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CommissionPlanAssignment_ShouldHaveNavigationProperties()
    {
        // Arrange
        var user = new User { Id = 1, Username = "salesrep" };
        var plan = new CommissionPlan { Id = 1, Name = "Sales Plan" };

        var assignment = new CommissionPlanAssignment
        {
            UserId = 1,
            User = user,
            CommissionPlanId = 1,
            CommissionPlan = plan
        };

        // Assert
        assignment.User.Should().NotBeNull();
        assignment.CommissionPlan.Should().NotBeNull();
    }
}

#endregion

#region Commission Entity Tests

public class CommissionEntityTests
{
    [Fact]
    public void Commission_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var commission = new Commission();

        // Assert
        commission.CommissionNumber.Should().BeEmpty();
        commission.Status.Should().Be(CommissionStatus.Pending);
        commission.CommissionPeriod.Should().BeEmpty();
        commission.DealAmount.Should().Be(0);
        commission.CommissionableAmount.Should().Be(0);
        commission.CommissionRate.Should().Be(0);
        commission.CommissionAmount.Should().Be(0);
        commission.SplitPercent.Should().Be(100);
        commission.FinalCommissionAmount.Should().Be(0);
        commission.CurrencyCode.Should().Be("USD");
        commission.AdjustmentAmount.Should().Be(0);
        commission.ClawbackAmount.Should().Be(0);
    }

    [Fact]
    public void Commission_ShouldSetDealDetails()
    {
        // Arrange
        var commission = new Commission
        {
            CommissionNumber = "COMM-2026-001",
            DealAmount = 50000m,
            CommissionableAmount = 45000m,
            CommissionRate = 10m,
            CommissionAmount = 4500m,
            SplitPercent = 50,
            FinalCommissionAmount = 2250m,
            CurrencyCode = "EUR"
        };

        // Assert
        commission.CommissionNumber.Should().Be("COMM-2026-001");
        commission.DealAmount.Should().Be(50000m);
        commission.CommissionableAmount.Should().Be(45000m);
        commission.CommissionRate.Should().Be(10m);
        commission.CommissionAmount.Should().Be(4500m);
        commission.SplitPercent.Should().Be(50);
        commission.FinalCommissionAmount.Should().Be(2250m);
        commission.CurrencyCode.Should().Be("EUR");
    }

    [Fact]
    public void Commission_ShouldSetQuotaContext()
    {
        // Arrange
        var commission = new Commission
        {
            QuotaAmount = 100000m,
            AttainmentPercent = 125m,
            TierName = "Accelerator",
            Multiplier = 1.5m
        };

        // Assert
        commission.QuotaAmount.Should().Be(100000m);
        commission.AttainmentPercent.Should().Be(125m);
        commission.TierName.Should().Be("Accelerator");
        commission.Multiplier.Should().Be(1.5m);
    }

    [Fact]
    public void Commission_ShouldSetDates()
    {
        // Arrange
        var earnedDate = DateTime.UtcNow;
        var approvedDate = earnedDate.AddDays(7);
        var paidDate = earnedDate.AddDays(30);
        var clawbackEndDate = earnedDate.AddDays(90);

        var commission = new Commission
        {
            EarnedDate = earnedDate,
            ApprovedDate = approvedDate,
            PaidDate = paidDate,
            ClawbackEndDate = clawbackEndDate
        };

        // Assert
        commission.EarnedDate.Should().Be(earnedDate);
        commission.ApprovedDate.Should().Be(approvedDate);
        commission.PaidDate.Should().Be(paidDate);
        commission.ClawbackEndDate.Should().Be(clawbackEndDate);
    }

    [Fact]
    public void Commission_ShouldSetAdjustments()
    {
        // Arrange
        var commission = new Commission
        {
            AdjustmentAmount = -500m,
            AdjustmentReason = "Product return adjustment",
            ClawbackAmount = 1000m,
            ClawbackReason = "Customer churn within clawback period",
            ClawbackDate = DateTime.UtcNow
        };

        // Assert
        commission.AdjustmentAmount.Should().Be(-500m);
        commission.AdjustmentReason.Should().Be("Product return adjustment");
        commission.ClawbackAmount.Should().Be(1000m);
        commission.ClawbackReason.Should().Be("Customer churn within clawback period");
        commission.ClawbackDate.Should().NotBeNull();
    }

    [Fact]
    public void Commission_ShouldHaveNavigationProperties()
    {
        // Arrange
        var commission = new Commission
        {
            UserId = 1,
            User = new User { Id = 1, Username = "salesrep" },
            CommissionPlanId = 1,
            CommissionPlan = new CommissionPlan { Id = 1, Name = "Plan" },
            OpportunityId = 10,
            Opportunity = new Opportunity { Id = 10, Name = "Big Deal" }
        };

        // Assert
        commission.User.Should().NotBeNull();
        commission.CommissionPlan.Should().NotBeNull();
        commission.Opportunity.Should().NotBeNull();
    }
}

#endregion

#region CommissionStatement Entity Tests

public class CommissionStatementEntityTests
{
    [Fact]
    public void CommissionStatement_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var statement = new CommissionStatement();

        // Assert
        statement.StatementNumber.Should().BeEmpty();
        statement.Period.Should().BeEmpty();
        statement.UserId.Should().Be(0);
        statement.TotalEarned.Should().Be(0);
        statement.TotalAdjustments.Should().Be(0);
        statement.TotalClawbacks.Should().Be(0);
        statement.NetPayout.Should().Be(0);
        statement.CurrencyCode.Should().Be("USD");
        statement.IsPaid.Should().BeFalse();
        statement.PaidDate.Should().BeNull();
        statement.PaymentReference.Should().BeNull();
        statement.StatementUrl.Should().BeNull();
    }

    [Fact]
    public void CommissionStatement_ShouldCalculateNetPayout()
    {
        // Arrange
        var statement = new CommissionStatement
        {
            TotalEarned = 10000m,
            TotalAdjustments = -500m,
            TotalClawbacks = -1000m,
            NetPayout = 8500m
        };

        // Assert
        statement.TotalEarned.Should().Be(10000m);
        statement.TotalAdjustments.Should().Be(-500m);
        statement.TotalClawbacks.Should().Be(-1000m);
        statement.NetPayout.Should().Be(8500m);
    }

    [Fact]
    public void CommissionStatement_ShouldSetPaymentInfo()
    {
        // Arrange
        var paidDate = DateTime.UtcNow;
        var statement = new CommissionStatement
        {
            IsPaid = true,
            PaidDate = paidDate,
            PaymentReference = "PAY-2026-001-ACH",
            StatementUrl = "https://docs.example.com/statements/2026-Q1.pdf"
        };

        // Assert
        statement.IsPaid.Should().BeTrue();
        statement.PaidDate.Should().Be(paidDate);
        statement.PaymentReference.Should().Be("PAY-2026-001-ACH");
        statement.StatementUrl.Should().Be("https://docs.example.com/statements/2026-Q1.pdf");
    }
}

#endregion

#region Team Entity Tests

public class TeamEntityTests
{
    [Fact]
    public void Team_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var team = new Team();

        // Assert
        team.Name.Should().BeEmpty();
        team.Code.Should().BeNull();
        team.Description.Should().BeNull();
        team.IsActive.Should().BeTrue();
        team.ManagerId.Should().BeNull();
        team.Manager.Should().BeNull();
        team.ParentTeamId.Should().BeNull();
        team.ParentTeam.Should().BeNull();
        team.ChildTeams.Should().BeEmpty();
        team.Members.Should().BeEmpty();
        team.Quotas.Should().BeEmpty();
        team.Forecasts.Should().BeEmpty();
    }

    [Fact]
    public void Team_ShouldSetBasicProperties()
    {
        // Arrange
        var team = new Team
        {
            Name = "Enterprise Sales",
            Code = "ENT-SALES",
            Description = "Enterprise accounts team",
            IsActive = true
        };

        // Assert
        team.Name.Should().Be("Enterprise Sales");
        team.Code.Should().Be("ENT-SALES");
        team.Description.Should().Be("Enterprise accounts team");
        team.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Team_ShouldSupportHierarchy()
    {
        // Arrange
        var parentTeam = new Team { Id = 1, Name = "Sales Organization" };
        var childTeam = new Team
        {
            Id = 2,
            Name = "West Region",
            ParentTeamId = 1,
            ParentTeam = parentTeam
        };

        parentTeam.ChildTeams.Add(childTeam);

        // Assert
        childTeam.ParentTeam.Should().NotBeNull();
        childTeam.ParentTeam!.Name.Should().Be("Sales Organization");
        parentTeam.ChildTeams.Should().HaveCount(1);
    }

    [Fact]
    public void Team_ShouldHaveManager()
    {
        // Arrange
        var manager = new User { Id = 1, Username = "manager" };
        var team = new Team
        {
            Name = "Sales Team",
            ManagerId = 1,
            Manager = manager
        };

        // Assert
        team.ManagerId.Should().Be(1);
        team.Manager.Should().NotBeNull();
        team.Manager!.Username.Should().Be("manager");
    }

    [Fact]
    public void Team_ShouldHaveMembersCollection()
    {
        // Arrange
        var team = new Team { Name = "Sales Team" };
        var member1 = new TeamMember { UserId = 1, Role = "Rep" };
        var member2 = new TeamMember { UserId = 2, Role = "Lead", IsTeamLead = true };

        // Act
        team.Members.Add(member1);
        team.Members.Add(member2);

        // Assert
        team.Members.Should().HaveCount(2);
    }

    [Fact]
    public void Team_ShouldInheritFromBaseEntity()
    {
        // Act
        var team = new Team();

        // Assert
        team.Should().BeAssignableTo<BaseEntity>();
    }
}

#endregion

#region TeamMember Entity Tests

public class TeamMemberEntityTests
{
    [Fact]
    public void TeamMember_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var member = new TeamMember();

        // Assert
        member.TeamId.Should().Be(0);
        member.Team.Should().BeNull();
        member.UserId.Should().Be(0);
        member.User.Should().BeNull();
        member.Role.Should().BeNull();
        member.IsTeamLead.Should().BeFalse();
        member.EndDate.Should().BeNull();
    }

    [Fact]
    public void TeamMember_ShouldSetAllProperties()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 12, 31);

        var member = new TeamMember
        {
            TeamId = 1,
            UserId = 5,
            Role = "Senior Account Executive",
            IsTeamLead = true,
            StartDate = startDate,
            EndDate = endDate
        };

        // Assert
        member.TeamId.Should().Be(1);
        member.UserId.Should().Be(5);
        member.Role.Should().Be("Senior Account Executive");
        member.IsTeamLead.Should().BeTrue();
        member.StartDate.Should().Be(startDate);
        member.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void TeamMember_ShouldHaveNavigationProperties()
    {
        // Arrange
        var team = new Team { Id = 1, Name = "Sales" };
        var user = new User { Id = 5, Username = "salesrep" };

        var member = new TeamMember
        {
            TeamId = 1,
            Team = team,
            UserId = 5,
            User = user
        };

        // Assert
        member.Team.Should().NotBeNull();
        member.User.Should().NotBeNull();
    }
}

#endregion

#region Web Visitor Enum Tests

public class VisitorIdentificationSourceEnumTests
{
    [Fact]
    public void VisitorIdentificationSource_ShouldHave9Values()
    {
        // Assert
        Enum.GetValues<VisitorIdentificationSource>().Should().HaveCount(9);
    }

    [Theory]
    [InlineData(VisitorIdentificationSource.Anonymous, 0)]
    [InlineData(VisitorIdentificationSource.FormSubmission, 1)]
    [InlineData(VisitorIdentificationSource.EmailClick, 2)]
    [InlineData(VisitorIdentificationSource.Login, 3)]
    [InlineData(VisitorIdentificationSource.Chat, 4)]
    [InlineData(VisitorIdentificationSource.Cookie, 5)]
    [InlineData(VisitorIdentificationSource.CompanyLookup, 6)]
    [InlineData(VisitorIdentificationSource.Social, 7)]
    [InlineData(VisitorIdentificationSource.Manual, 8)]
    public void VisitorIdentificationSource_ShouldHaveCorrectIntValue(VisitorIdentificationSource source, int expected)
    {
        // Assert
        ((int)source).Should().Be(expected);
    }
}

public class PageCategoryEnumTests
{
    [Fact]
    public void PageCategory_ShouldHave13Values()
    {
        // Assert
        Enum.GetValues<PageCategory>().Should().HaveCount(13);
    }

    [Theory]
    [InlineData(PageCategory.Home, 0)]
    [InlineData(PageCategory.Product, 1)]
    [InlineData(PageCategory.Pricing, 2)]
    [InlineData(PageCategory.Features, 3)]
    [InlineData(PageCategory.Blog, 4)]
    [InlineData(PageCategory.CaseStudy, 5)]
    [InlineData(PageCategory.Documentation, 6)]
    [InlineData(PageCategory.Demo, 7)]
    [InlineData(PageCategory.Contact, 8)]
    [InlineData(PageCategory.About, 9)]
    [InlineData(PageCategory.Careers, 10)]
    [InlineData(PageCategory.ThankYou, 11)]
    [InlineData(PageCategory.Other, 12)]
    public void PageCategory_ShouldHaveCorrectIntValue(PageCategory category, int expected)
    {
        // Assert
        ((int)category).Should().Be(expected);
    }

    [Fact]
    public void PageCategory_HighValuePages_ShouldBePricingAndDemo()
    {
        // High-value pages typically visited by serious prospects
        var highValuePages = new[] { PageCategory.Pricing, PageCategory.Demo };

        // Assert - Both should be in the range 0-12
        foreach (var page in highValuePages)
        {
            ((int)page).Should().BeGreaterThanOrEqualTo(0);
            ((int)page).Should().BeLessThanOrEqualTo(12);
        }
    }
}

#endregion

#region WebVisitor Entity Tests

public class WebVisitorEntityTests
{
    [Fact]
    public void WebVisitor_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var visitor = new WebVisitor();

        // Assert
        visitor.VisitorId.Should().BeEmpty();
        visitor.FingerprintId.Should().BeNull();
        visitor.IsIdentified.Should().BeFalse();
        visitor.IdentificationSource.Should().Be(VisitorIdentificationSource.Anonymous);
        visitor.IdentifiedAt.Should().BeNull();
        visitor.Email.Should().BeNull();
        visitor.FirstName.Should().BeNull();
        visitor.LastName.Should().BeNull();
        visitor.Phone.Should().BeNull();
        visitor.Company.Should().BeNull();
    }

    [Fact]
    public void WebVisitor_ShouldSetIdentification()
    {
        // Arrange
        var visitor = new WebVisitor
        {
            VisitorId = "visitor-abc123",
            FingerprintId = "fp-xyz789",
            IsIdentified = true,
            IdentificationSource = VisitorIdentificationSource.FormSubmission,
            IdentifiedAt = DateTime.UtcNow
        };

        // Assert
        visitor.VisitorId.Should().Be("visitor-abc123");
        visitor.FingerprintId.Should().Be("fp-xyz789");
        visitor.IsIdentified.Should().BeTrue();
        visitor.IdentificationSource.Should().Be(VisitorIdentificationSource.FormSubmission);
        visitor.IdentifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void WebVisitor_ShouldSetContactInfo()
    {
        // Arrange
        var visitor = new WebVisitor
        {
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            Phone = "+1-555-123-4567",
            Company = "Acme Corp"
        };

        // Assert
        visitor.Email.Should().Be("john.doe@example.com");
        visitor.FirstName.Should().Be("John");
        visitor.LastName.Should().Be("Doe");
        visitor.Phone.Should().Be("+1-555-123-4567");
        visitor.Company.Should().Be("Acme Corp");
    }

    [Fact]
    public void WebVisitor_ShouldSupportAnonymousToIdentifiedTransition()
    {
        // Arrange - Start as anonymous
        var visitor = new WebVisitor
        {
            VisitorId = "visitor-anonymous-123",
            IsIdentified = false,
            IdentificationSource = VisitorIdentificationSource.Anonymous
        };

        // Act - Visitor submits a form
        visitor.IsIdentified = true;
        visitor.IdentificationSource = VisitorIdentificationSource.FormSubmission;
        visitor.IdentifiedAt = DateTime.UtcNow;
        visitor.Email = "converted@example.com";
        visitor.FirstName = "Converted";
        visitor.LastName = "User";

        // Assert
        visitor.IsIdentified.Should().BeTrue();
        visitor.IdentificationSource.Should().Be(VisitorIdentificationSource.FormSubmission);
        visitor.Email.Should().Be("converted@example.com");
    }

    [Fact]
    public void WebVisitor_ShouldInheritFromBaseEntity()
    {
        // Act
        var visitor = new WebVisitor();

        // Assert
        visitor.Should().BeAssignableTo<BaseEntity>();
    }
}

#endregion

#region BaseEntity Inheritance Tests

public class CommissionTeamWebBaseEntityTests
{
    [Theory]
    [InlineData(typeof(CommissionPlan))]
    [InlineData(typeof(CommissionTier))]
    [InlineData(typeof(CommissionPlanAssignment))]
    [InlineData(typeof(Commission))]
    [InlineData(typeof(CommissionStatement))]
    [InlineData(typeof(Team))]
    [InlineData(typeof(TeamMember))]
    [InlineData(typeof(WebVisitor))]
    public void Entity_ShouldInheritFromBaseEntity(Type entityType)
    {
        // Assert
        entityType.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void AllEntities_ShouldHaveDefaultId0()
    {
        // Act & Assert
        new CommissionPlan().Id.Should().Be(0);
        new CommissionTier().Id.Should().Be(0);
        new CommissionPlanAssignment().Id.Should().Be(0);
        new Commission().Id.Should().Be(0);
        new CommissionStatement().Id.Should().Be(0);
        new Team().Id.Should().Be(0);
        new TeamMember().Id.Should().Be(0);
        new WebVisitor().Id.Should().Be(0);
    }

    [Fact]
    public void AllEntities_ShouldHaveDefaultIsDeletedFalse()
    {
        // Act & Assert
        new CommissionPlan().IsDeleted.Should().BeFalse();
        new CommissionTier().IsDeleted.Should().BeFalse();
        new CommissionPlanAssignment().IsDeleted.Should().BeFalse();
        new Commission().IsDeleted.Should().BeFalse();
        new CommissionStatement().IsDeleted.Should().BeFalse();
        new Team().IsDeleted.Should().BeFalse();
        new TeamMember().IsDeleted.Should().BeFalse();
        new WebVisitor().IsDeleted.Should().BeFalse();
    }
}

#endregion
