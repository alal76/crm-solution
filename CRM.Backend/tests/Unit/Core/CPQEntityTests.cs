// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

#region BundleItemType Enum Tests

public class BundleItemTypeEnumTests
{
    [Theory]
    [InlineData(BundleItemType.Required, 0)]
    [InlineData(BundleItemType.Optional, 1)]
    [InlineData(BundleItemType.Default, 2)]
    [InlineData(BundleItemType.Exclusive, 3)]
    public void BundleItemType_ShouldHaveCorrectValues(BundleItemType itemType, int expected)
    {
        ((int)itemType).Should().Be(expected);
    }

    [Fact]
    public void BundleItemType_ShouldHave4Values()
    {
        var values = Enum.GetValues(typeof(BundleItemType));
        values.Length.Should().Be(4);
    }
}

#endregion

#region BundlePricingType Enum Tests

public class BundlePricingTypeEnumTests
{
    [Theory]
    [InlineData(BundlePricingType.FixedPrice, 0)]
    [InlineData(BundlePricingType.ComponentSum, 1)]
    [InlineData(BundlePricingType.PercentDiscount, 2)]
    [InlineData(BundlePricingType.Custom, 3)]
    public void BundlePricingType_ShouldHaveCorrectValues(BundlePricingType pricingType, int expected)
    {
        ((int)pricingType).Should().Be(expected);
    }

    [Fact]
    public void BundlePricingType_ShouldHave4Values()
    {
        var values = Enum.GetValues(typeof(BundlePricingType));
        values.Length.Should().Be(4);
    }
}

#endregion

#region BundleStatus Enum Tests

public class BundleStatusEnumTests
{
    [Theory]
    [InlineData(BundleStatus.Draft, 0)]
    [InlineData(BundleStatus.Active, 1)]
    [InlineData(BundleStatus.Inactive, 2)]
    [InlineData(BundleStatus.Archived, 3)]
    public void BundleStatus_ShouldHaveCorrectValues(BundleStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    [Fact]
    public void BundleStatus_ShouldHave4Values()
    {
        var values = Enum.GetValues(typeof(BundleStatus));
        values.Length.Should().Be(4);
    }
}

#endregion

#region DiscountApprovalStatus Enum Tests

public class DiscountApprovalStatusEnumTests
{
    [Theory]
    [InlineData(DiscountApprovalStatus.NotSubmitted, 0)]
    [InlineData(DiscountApprovalStatus.Pending, 1)]
    [InlineData(DiscountApprovalStatus.Approved, 2)]
    [InlineData(DiscountApprovalStatus.Rejected, 3)]
    [InlineData(DiscountApprovalStatus.Recalled, 4)]
    [InlineData(DiscountApprovalStatus.Escalated, 5)]
    [InlineData(DiscountApprovalStatus.AutoApproved, 6)]
    public void DiscountApprovalStatus_ShouldHaveCorrectValues(DiscountApprovalStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    [Fact]
    public void DiscountApprovalStatus_ShouldHave7Values()
    {
        var values = Enum.GetValues(typeof(DiscountApprovalStatus));
        values.Length.Should().Be(7);
    }
}

#endregion

#region ApprovalThresholdType Enum Tests

public class ApprovalThresholdTypeEnumTests
{
    [Theory]
    [InlineData(ApprovalThresholdType.DiscountPercent, 0)]
    [InlineData(ApprovalThresholdType.DiscountAmount, 1)]
    [InlineData(ApprovalThresholdType.MarginPercent, 2)]
    [InlineData(ApprovalThresholdType.DealSize, 3)]
    [InlineData(ApprovalThresholdType.NonStandardTerms, 4)]
    [InlineData(ApprovalThresholdType.PaymentTerms, 5)]
    [InlineData(ApprovalThresholdType.Custom, 6)]
    public void ApprovalThresholdType_ShouldHaveCorrectValues(ApprovalThresholdType thresholdType, int expected)
    {
        ((int)thresholdType).Should().Be(expected);
    }

    [Fact]
    public void ApprovalThresholdType_ShouldHave7Values()
    {
        var values = Enum.GetValues(typeof(ApprovalThresholdType));
        values.Length.Should().Be(7);
    }
}

#endregion

#region ProductBundle Entity Tests

public class ProductBundleEntityTests
{
    [Fact]
    public void ProductBundle_ShouldInitializeWithDefaults()
    {
        var bundle = new ProductBundle();

        bundle.Name.Should().Be(string.Empty);
        bundle.SKU.Should().BeNull();
        bundle.BundleCode.Should().BeNull();
        bundle.Description.Should().BeNull();
        bundle.Status.Should().Be(BundleStatus.Draft);
        bundle.PricingType.Should().Be(BundlePricingType.ComponentSum);
        bundle.CurrencyCode.Should().Be("USD");
        bundle.AllowQuantityChange.Should().BeTrue();
        bundle.ShowComponentPrices.Should().BeTrue();
        bundle.AllowPartialConfiguration.Should().BeFalse();
        bundle.DisplayOrder.Should().Be(0);
        bundle.IsFeatured.Should().BeFalse();
        bundle.Items.Should().BeEmpty();
        bundle.Rules.Should().BeEmpty();
    }

    [Fact]
    public void ProductBundle_IsValid_ShouldReturnTrueForActiveBundle()
    {
        var bundle = new ProductBundle
        {
            Status = BundleStatus.Active,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1)
        };

        bundle.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ProductBundle_IsValid_ShouldReturnFalseForDraftBundle()
    {
        var bundle = new ProductBundle
        {
            Status = BundleStatus.Draft,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1)
        };

        bundle.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ProductBundle_IsValid_ShouldReturnFalseForExpiredBundle()
    {
        var bundle = new ProductBundle
        {
            Status = BundleStatus.Active,
            EffectiveEndDate = DateTime.UtcNow.AddDays(-1)
        };

        bundle.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ProductBundle_IsValid_ShouldReturnFalseForFutureBundle()
    {
        var bundle = new ProductBundle
        {
            Status = BundleStatus.Active,
            EffectiveStartDate = DateTime.UtcNow.AddDays(1)
        };

        bundle.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ProductBundle_ShouldSetPricingProperties()
    {
        var bundle = new ProductBundle
        {
            PricingType = BundlePricingType.FixedPrice,
            FixedPrice = 199.99m,
            MinimumPrice = 150.00m,
            MaxDiscountPercent = 20,
            ListPrice = 249.99m
        };

        bundle.PricingType.Should().Be(BundlePricingType.FixedPrice);
        bundle.FixedPrice.Should().Be(199.99m);
        bundle.MinimumPrice.Should().Be(150.00m);
        bundle.MaxDiscountPercent.Should().Be(20);
        bundle.ListPrice.Should().Be(249.99m);
    }

    [Fact]
    public void ProductBundle_ShouldSetConfigurationProperties()
    {
        var bundle = new ProductBundle
        {
            MinItems = 2,
            MaxItems = 10,
            AllowQuantityChange = false,
            ShowComponentPrices = false,
            AllowPartialConfiguration = true
        };

        bundle.MinItems.Should().Be(2);
        bundle.MaxItems.Should().Be(10);
        bundle.AllowQuantityChange.Should().BeFalse();
        bundle.ShowComponentPrices.Should().BeFalse();
        bundle.AllowPartialConfiguration.Should().BeTrue();
    }

    [Fact]
    public void ProductBundle_ShouldInheritFromBaseEntity()
    {
        var bundle = new ProductBundle();

        bundle.Should().BeAssignableTo<BaseEntity>();
        bundle.Id.Should().Be(0);
        bundle.IsDeleted.Should().BeFalse();
    }
}

#endregion

#region ProductBundleItem Entity Tests

public class ProductBundleItemEntityTests
{
    [Fact]
    public void ProductBundleItem_ShouldInitializeWithDefaults()
    {
        var item = new ProductBundleItem();

        item.ItemType.Should().Be(BundleItemType.Required);
        item.DefaultQuantity.Should().Be(1);
        item.MinQuantity.Should().Be(0);
        item.MaxQuantity.Should().BeNull();
        item.DisplayOrder.Should().Be(0);
        item.IsFree.Should().BeFalse();
        item.IsDefaultSelected.Should().BeFalse();
        item.AllowQuantityChange.Should().BeTrue();
        item.AllowRemoval.Should().BeTrue();
    }

    [Fact]
    public void ProductBundleItem_ShouldSetItemProperties()
    {
        var item = new ProductBundleItem
        {
            ItemType = BundleItemType.Optional,
            DefaultQuantity = 2,
            MinQuantity = 1,
            MaxQuantity = 5,
            OverridePrice = 49.99m,
            DiscountPercent = 10,
            IsFree = false,
            ExclusiveGroup = "accessories"
        };

        item.ItemType.Should().Be(BundleItemType.Optional);
        item.DefaultQuantity.Should().Be(2);
        item.MinQuantity.Should().Be(1);
        item.MaxQuantity.Should().Be(5);
        item.OverridePrice.Should().Be(49.99m);
        item.DiscountPercent.Should().Be(10);
        item.ExclusiveGroup.Should().Be("accessories");
    }

    [Fact]
    public void ProductBundleItem_ShouldInheritFromBaseEntity()
    {
        var item = new ProductBundleItem();

        item.Should().BeAssignableTo<BaseEntity>();
        item.Id.Should().Be(0);
    }
}

#endregion

#region ProductBundleRule Entity Tests

public class ProductBundleRuleEntityTests
{
    [Fact]
    public void ProductBundleRule_ShouldInitializeWithDefaults()
    {
        var rule = new ProductBundleRule();

        rule.Name.Should().Be(string.Empty);
        rule.RuleType.Should().Be("requires");
        rule.IsActive.Should().BeTrue();
        rule.Priority.Should().Be(0);
    }

    [Fact]
    public void ProductBundleRule_ShouldSetRuleProperties()
    {
        var rule = new ProductBundleRule
        {
            Name = "Warranty requires support",
            RuleType = "requires",
            SourceProductId = 1,
            TargetProductId = 2,
            ErrorMessage = "Extended warranty requires support package",
            Priority = 10,
            Condition = "{\"operator\": \"equals\", \"field\": \"quantity\", \"value\": 1}"
        };

        rule.Name.Should().Be("Warranty requires support");
        rule.RuleType.Should().Be("requires");
        rule.SourceProductId.Should().Be(1);
        rule.TargetProductId.Should().Be(2);
        rule.ErrorMessage.Should().Be("Extended warranty requires support package");
        rule.Priority.Should().Be(10);
        rule.Condition.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ProductBundleRule_ShouldInheritFromBaseEntity()
    {
        var rule = new ProductBundleRule();

        rule.Should().BeAssignableTo<BaseEntity>();
        rule.Id.Should().Be(0);
    }
}

#endregion

#region DiscountApprovalMatrix Entity Tests

public class DiscountApprovalMatrixEntityTests
{
    [Fact]
    public void DiscountApprovalMatrix_ShouldInitializeWithDefaults()
    {
        var matrix = new DiscountApprovalMatrix();

        matrix.Name.Should().Be(string.Empty);
        matrix.Description.Should().BeNull();
        matrix.IsActive.Should().BeTrue();
        matrix.Priority.Should().Be(0);
        matrix.AppliesToAllProducts.Should().BeTrue();
        matrix.RequireAllLevels.Should().BeFalse();
        matrix.AllowParallelApproval.Should().BeFalse();
        matrix.Levels.Should().BeEmpty();
    }

    [Fact]
    public void DiscountApprovalMatrix_ShouldSetMatrixProperties()
    {
        var matrix = new DiscountApprovalMatrix
        {
            Name = "Standard Discount Approval",
            Description = "Default approval matrix for all products",
            Priority = 1,
            ProductCategories = "Hardware,Software",
            CustomerSegments = "Enterprise,SMB",
            Regions = "NA,EMEA",
            RequireAllLevels = true,
            AutoEscalateHours = 24,
            ReminderHours = 8
        };

        matrix.Name.Should().Be("Standard Discount Approval");
        matrix.Priority.Should().Be(1);
        matrix.ProductCategories.Should().Be("Hardware,Software");
        matrix.CustomerSegments.Should().Be("Enterprise,SMB");
        matrix.Regions.Should().Be("NA,EMEA");
        matrix.RequireAllLevels.Should().BeTrue();
        matrix.AutoEscalateHours.Should().Be(24);
        matrix.ReminderHours.Should().Be(8);
    }

    [Fact]
    public void DiscountApprovalMatrix_ShouldInheritFromBaseEntity()
    {
        var matrix = new DiscountApprovalMatrix();

        matrix.Should().BeAssignableTo<BaseEntity>();
        matrix.Id.Should().Be(0);
    }
}

#endregion

#region ApprovalLevel Entity Tests

public class ApprovalLevelEntityTests
{
    [Fact]
    public void ApprovalLevel_ShouldInitializeWithDefaults()
    {
        var level = new ApprovalLevel();

        level.Name.Should().Be(string.Empty);
        level.ThresholdType.Should().Be(ApprovalThresholdType.DiscountPercent);
        level.MinValue.Should().Be(0);
        level.MaxValue.Should().BeNull();
        level.UseSubmitterManager.Should().BeFalse();
        level.ManagerLevelsUp.Should().Be(1);
        level.RequireAllGroupMembers.Should().BeFalse();
        level.CanSkip.Should().BeFalse();
        level.AutoApproveIfSelf.Should().BeTrue();
        level.SendEmailOnPending.Should().BeTrue();
        level.IncludeQuoteDetails.Should().BeTrue();
    }

    [Fact]
    public void ApprovalLevel_ShouldSetLevelProperties()
    {
        var level = new ApprovalLevel
        {
            Name = "Manager Approval",
            LevelOrder = 1,
            ThresholdType = ApprovalThresholdType.DiscountPercent,
            MinValue = 10,
            MaxValue = 25,
            UseSubmitterManager = true,
            ManagerLevelsUp = 2,
            TimeoutHours = 48
        };

        level.Name.Should().Be("Manager Approval");
        level.LevelOrder.Should().Be(1);
        level.ThresholdType.Should().Be(ApprovalThresholdType.DiscountPercent);
        level.MinValue.Should().Be(10);
        level.MaxValue.Should().Be(25);
        level.UseSubmitterManager.Should().BeTrue();
        level.ManagerLevelsUp.Should().Be(2);
        level.TimeoutHours.Should().Be(48);
    }

    [Fact]
    public void ApprovalLevel_ShouldInheritFromBaseEntity()
    {
        var level = new ApprovalLevel();

        level.Should().BeAssignableTo<BaseEntity>();
        level.Id.Should().Be(0);
    }
}

#endregion

#region ApprovalGroup Entity Tests

public class ApprovalGroupEntityTests
{
    [Fact]
    public void ApprovalGroup_ShouldInitializeWithDefaults()
    {
        var group = new ApprovalGroup();

        group.Name.Should().Be(string.Empty);
        group.Description.Should().BeNull();
        group.IsActive.Should().BeTrue();
        group.Members.Should().BeEmpty();
    }

    [Fact]
    public void ApprovalGroup_ShouldSetGroupProperties()
    {
        var group = new ApprovalGroup
        {
            Name = "Finance Approvers",
            Description = "Finance team members authorized to approve discounts",
            IsActive = true
        };

        group.Name.Should().Be("Finance Approvers");
        group.Description.Should().Be("Finance team members authorized to approve discounts");
        group.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ApprovalGroup_ShouldInheritFromBaseEntity()
    {
        var group = new ApprovalGroup();

        group.Should().BeAssignableTo<BaseEntity>();
        group.Id.Should().Be(0);
    }
}

#endregion

#region ApprovalGroupMember Entity Tests

public class ApprovalGroupMemberEntityTests
{
    [Fact]
    public void ApprovalGroupMember_ShouldInitializeWithDefaults()
    {
        var member = new ApprovalGroupMember();

        member.IsActive.Should().BeTrue();
        member.Order.Should().Be(0);
    }

    [Fact]
    public void ApprovalGroupMember_ShouldSetMemberProperties()
    {
        var member = new ApprovalGroupMember
        {
            ApprovalGroupId = 1,
            UserId = 10,
            IsActive = true,
            Order = 5
        };

        member.ApprovalGroupId.Should().Be(1);
        member.UserId.Should().Be(10);
        member.IsActive.Should().BeTrue();
        member.Order.Should().Be(5);
    }

    [Fact]
    public void ApprovalGroupMember_ShouldInheritFromBaseEntity()
    {
        var member = new ApprovalGroupMember();

        member.Should().BeAssignableTo<BaseEntity>();
        member.Id.Should().Be(0);
    }
}

#endregion

#region ApprovalRequest Entity Tests

public class ApprovalRequestEntityTests
{
    [Fact]
    public void ApprovalRequest_ShouldInitializeWithDefaults()
    {
        var request = new ApprovalRequest();

        request.RequestNumber.Should().Be(string.Empty);
        request.Status.Should().Be(DiscountApprovalStatus.NotSubmitted);
        request.CurrentLevel.Should().Be(0);
        request.Steps.Should().BeEmpty();
    }

    [Fact]
    public void ApprovalRequest_ShouldSetRequestProperties()
    {
        var submittedAt = DateTime.UtcNow;
        var request = new ApprovalRequest
        {
            RequestNumber = "APR-2026-001",
            Status = DiscountApprovalStatus.Pending,
            QuoteId = 100,
            DiscountPercent = 15,
            DiscountAmount = 1500,
            DealAmount = 10000,
            MarginPercent = 25,
            Justification = "Large enterprise deal, strategic account",
            CurrentLevel = 1,
            MaxLevelRequired = 2,
            SubmittedAt = submittedAt,
            SubmitterId = 5
        };

        request.RequestNumber.Should().Be("APR-2026-001");
        request.Status.Should().Be(DiscountApprovalStatus.Pending);
        request.QuoteId.Should().Be(100);
        request.DiscountPercent.Should().Be(15);
        request.DiscountAmount.Should().Be(1500);
        request.DealAmount.Should().Be(10000);
        request.MarginPercent.Should().Be(25);
        request.Justification.Should().Be("Large enterprise deal, strategic account");
        request.CurrentLevel.Should().Be(1);
        request.MaxLevelRequired.Should().Be(2);
        request.SubmittedAt.Should().Be(submittedAt);
        request.SubmitterId.Should().Be(5);
    }

    [Fact]
    public void ApprovalRequest_ShouldInheritFromBaseEntity()
    {
        var request = new ApprovalRequest();

        request.Should().BeAssignableTo<BaseEntity>();
        request.Id.Should().Be(0);
    }
}

#endregion

#region ApprovalStep Entity Tests

public class ApprovalStepEntityTests
{
    [Fact]
    public void ApprovalStep_ShouldInitializeWithDefaults()
    {
        var step = new ApprovalStep();

        step.Status.Should().Be(DiscountApprovalStatus.Pending);
        step.ReminderSent.Should().BeFalse();
        step.WasEscalated.Should().BeFalse();
    }

    [Fact]
    public void ApprovalStep_ShouldSetStepProperties()
    {
        var assignedAt = DateTime.UtcNow.AddHours(-2);
        var actedAt = DateTime.UtcNow;
        var step = new ApprovalStep
        {
            StepOrder = 1,
            ApprovalLevelId = 5,
            Status = DiscountApprovalStatus.Approved,
            AssignedToId = 10,
            ActedById = 10,
            AssignedAt = assignedAt,
            ActedAt = actedAt,
            Comments = "Approved - strategic account",
            ApprovalRequestId = 100
        };

        step.StepOrder.Should().Be(1);
        step.ApprovalLevelId.Should().Be(5);
        step.Status.Should().Be(DiscountApprovalStatus.Approved);
        step.AssignedToId.Should().Be(10);
        step.ActedById.Should().Be(10);
        step.AssignedAt.Should().Be(assignedAt);
        step.ActedAt.Should().Be(actedAt);
        step.Comments.Should().Be("Approved - strategic account");
        step.ApprovalRequestId.Should().Be(100);
    }

    [Fact]
    public void ApprovalStep_ShouldSetEscalationProperties()
    {
        var escalatedAt = DateTime.UtcNow;
        var step = new ApprovalStep
        {
            WasEscalated = true,
            EscalatedToId = 15,
            EscalatedAt = escalatedAt
        };

        step.WasEscalated.Should().BeTrue();
        step.EscalatedToId.Should().Be(15);
        step.EscalatedAt.Should().Be(escalatedAt);
    }

    [Fact]
    public void ApprovalStep_ShouldSetReminderProperties()
    {
        var reminderSentAt = DateTime.UtcNow.AddHours(-1);
        var step = new ApprovalStep
        {
            ReminderSent = true,
            ReminderSentAt = reminderSentAt,
            DueAt = DateTime.UtcNow.AddHours(24)
        };

        step.ReminderSent.Should().BeTrue();
        step.ReminderSentAt.Should().Be(reminderSentAt);
        step.DueAt.Should().NotBeNull();
    }

    [Fact]
    public void ApprovalStep_ShouldInheritFromBaseEntity()
    {
        var step = new ApprovalStep();

        step.Should().BeAssignableTo<BaseEntity>();
        step.Id.Should().Be(0);
    }
}

#endregion
