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

/// <summary>
/// Unit tests for the SubscriptionRenewal entity, SubscriptionRenewalStatus enum,
/// and BillingCycle enum.
///
/// Spec: TODO-SALES006-011, TODO-SALES006-006
/// </summary>
public class SubscriptionRenewalEntityTests
{
    #region SubscriptionRenewalStatus Enum Tests

    [Fact]
    public void SubscriptionRenewalStatus_ShouldHaveExpectedValues()
    {
        SubscriptionRenewalStatus.Pending.Should().Be((SubscriptionRenewalStatus)0);
        SubscriptionRenewalStatus.Completed.Should().Be((SubscriptionRenewalStatus)1);
        SubscriptionRenewalStatus.Failed.Should().Be((SubscriptionRenewalStatus)2);
        SubscriptionRenewalStatus.Skipped.Should().Be((SubscriptionRenewalStatus)3);
    }

    [Fact]
    public void SubscriptionRenewalStatus_ShouldHave4Values()
    {
        var values = Enum.GetValues<SubscriptionRenewalStatus>();
        values.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(SubscriptionRenewalStatus.Pending, "Pending")]
    [InlineData(SubscriptionRenewalStatus.Completed, "Completed")]
    [InlineData(SubscriptionRenewalStatus.Failed, "Failed")]
    [InlineData(SubscriptionRenewalStatus.Skipped, "Skipped")]
    public void SubscriptionRenewalStatus_ShouldHaveCorrectName(SubscriptionRenewalStatus status, string expectedName)
    {
        status.ToString().Should().Be(expectedName);
    }

    #endregion

    #region BillingCycle Enum Tests

    [Fact]
    public void BillingCycle_ShouldHaveExpectedValues()
    {
        BillingCycle.Monthly.Should().Be((BillingCycle)1);
        BillingCycle.Quarterly.Should().Be((BillingCycle)2);
        BillingCycle.Annual.Should().Be((BillingCycle)3);
        BillingCycle.Weekly.Should().Be((BillingCycle)4);
        BillingCycle.Daily.Should().Be((BillingCycle)5);
        BillingCycle.Biannual.Should().Be((BillingCycle)6);
        BillingCycle.Custom.Should().Be((BillingCycle)99);
    }

    [Fact]
    public void BillingCycle_ShouldHave7Values()
    {
        var values = Enum.GetValues<BillingCycle>();
        values.Should().HaveCount(7);
    }

    [Theory]
    [InlineData(BillingCycle.Monthly, "Monthly")]
    [InlineData(BillingCycle.Quarterly, "Quarterly")]
    [InlineData(BillingCycle.Annual, "Annual")]
    [InlineData(BillingCycle.Weekly, "Weekly")]
    [InlineData(BillingCycle.Daily, "Daily")]
    [InlineData(BillingCycle.Biannual, "Biannual")]
    [InlineData(BillingCycle.Custom, "Custom")]
    public void BillingCycle_ShouldHaveCorrectName(BillingCycle cycle, string expectedName)
    {
        cycle.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void BillingCycle_CustomShouldHaveValue99()
    {
        ((int)BillingCycle.Custom).Should().Be(99);
    }

    #endregion

    #region SubscriptionRenewal Entity Tests

    [Fact]
    public void SubscriptionRenewal_ShouldInheritFromBaseEntity()
    {
        typeof(SubscriptionRenewal).BaseType.Should().Be(typeof(BaseEntity));
    }

    [Fact]
    public void SubscriptionRenewal_DefaultStatus_ShouldBePending()
    {
        var renewal = new SubscriptionRenewal();
        renewal.Status.Should().Be(SubscriptionRenewalStatus.Pending);
    }

    [Fact]
    public void SubscriptionRenewal_DefaultRenewalDate_ShouldBeUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var renewal = new SubscriptionRenewal();
        var after = DateTime.UtcNow.AddSeconds(1);

        renewal.RenewalDate.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void SubscriptionRenewal_InvoiceId_ShouldBeNullableByDefault()
    {
        var renewal = new SubscriptionRenewal();
        renewal.InvoiceId.Should().BeNull();
    }

    [Fact]
    public void SubscriptionRenewal_Notes_ShouldBeNullableByDefault()
    {
        var renewal = new SubscriptionRenewal();
        renewal.Notes.Should().BeNull();
    }

    [Fact]
    public void SubscriptionRenewal_ShouldSetAllPropertiesCorrectly()
    {
        var periodStart = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);
        var renewalDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        var renewal = new SubscriptionRenewal
        {
            SubscriptionId = 42,
            RenewalDate = renewalDate,
            Status = SubscriptionRenewalStatus.Completed,
            Amount = 99.99m,
            BillingPeriodStart = periodStart,
            BillingPeriodEnd = periodEnd,
            InvoiceId = 101,
            Notes = "Auto-renewed successfully"
        };

        renewal.SubscriptionId.Should().Be(42);
        renewal.RenewalDate.Should().Be(renewalDate);
        renewal.Status.Should().Be(SubscriptionRenewalStatus.Completed);
        renewal.Amount.Should().Be(99.99m);
        renewal.BillingPeriodStart.Should().Be(periodStart);
        renewal.BillingPeriodEnd.Should().Be(periodEnd);
        renewal.InvoiceId.Should().Be(101);
        renewal.Notes.Should().Be("Auto-renewed successfully");
    }

    [Fact]
    public void SubscriptionRenewal_Amount_ShouldSupportHighPrecisionDecimal()
    {
        var renewal = new SubscriptionRenewal
        {
            Amount = 1234567890.1234m
        };

        renewal.Amount.Should().Be(1234567890.1234m);
    }

    [Theory]
    [InlineData(SubscriptionRenewalStatus.Pending)]
    [InlineData(SubscriptionRenewalStatus.Completed)]
    [InlineData(SubscriptionRenewalStatus.Failed)]
    [InlineData(SubscriptionRenewalStatus.Skipped)]
    public void SubscriptionRenewal_ShouldAcceptAllStatusValues(SubscriptionRenewalStatus status)
    {
        var renewal = new SubscriptionRenewal { Status = status };
        renewal.Status.Should().Be(status);
    }

    [Fact]
    public void SubscriptionRenewal_IsDeleted_ShouldDefaultToFalse()
    {
        var renewal = new SubscriptionRenewal();
        renewal.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void SubscriptionRenewal_CreatedAt_ShouldDefaultToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var renewal = new SubscriptionRenewal();
        var after = DateTime.UtcNow.AddSeconds(1);

        renewal.CreatedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void SubscriptionRenewal_UpdatedAt_ShouldBeNullByDefault()
    {
        var renewal = new SubscriptionRenewal();
        renewal.UpdatedAt.Should().BeNull();
    }

    #endregion

    #region BillingHistory Enum Tests

    [Fact]
    public void BillingEventType_ShouldHaveExpectedValues()
    {
        BillingEventType.Created.Should().Be((BillingEventType)0);
        BillingEventType.Activated.Should().Be((BillingEventType)1);
        BillingEventType.PlanChanged.Should().Be((BillingEventType)2);
        BillingEventType.Invoiced.Should().Be((BillingEventType)3);
        BillingEventType.Cancelled.Should().Be((BillingEventType)4);
        BillingEventType.Renewed.Should().Be((BillingEventType)5);
        BillingEventType.Paused.Should().Be((BillingEventType)6);
        BillingEventType.Resumed.Should().Be((BillingEventType)7);
        BillingEventType.Suspended.Should().Be((BillingEventType)8);
        BillingEventType.PaymentCollected.Should().Be((BillingEventType)9);
        BillingEventType.PaymentFailed.Should().Be((BillingEventType)10);
        BillingEventType.ProrationApplied.Should().Be((BillingEventType)11);
        BillingEventType.UsageChargeApplied.Should().Be((BillingEventType)12);
    }

    [Fact]
    public void BillingEventType_ShouldHave13Values()
    {
        var values = Enum.GetValues<BillingEventType>();
        values.Should().HaveCount(13);
    }

    #endregion

    #region DunningStatus Enum Tests

    [Fact]
    public void DunningStatus_ShouldHaveExpectedValues()
    {
        DunningStatus.Active.Should().Be((DunningStatus)0);
        DunningStatus.Resolved.Should().Be((DunningStatus)1);
        DunningStatus.Exhausted.Should().Be((DunningStatus)2);
        DunningStatus.WrittenOff.Should().Be((DunningStatus)3);
        DunningStatus.GracePeriod.Should().Be((DunningStatus)4);
    }

    [Fact]
    public void DunningStatus_ShouldHave5Values()
    {
        var values = Enum.GetValues<DunningStatus>();
        values.Should().HaveCount(5);
    }

    #endregion
}
