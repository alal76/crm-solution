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
/// Unit tests for Credit Memo, E-Signature, Order, and Sales Quota entities.
/// </summary>
public class CreditSignatureOrderEntityTests
{
    #region CreditMemoStatus Enum

    [Theory]
    [InlineData(CreditMemoStatus.Draft, 0)]
    [InlineData(CreditMemoStatus.PendingApproval, 1)]
    [InlineData(CreditMemoStatus.Approved, 2)]
    [InlineData(CreditMemoStatus.PartiallyApplied, 3)]
    [InlineData(CreditMemoStatus.Applied, 4)]
    [InlineData(CreditMemoStatus.Refunded, 5)]
    [InlineData(CreditMemoStatus.Voided, 6)]
    [InlineData(CreditMemoStatus.Expired, 7)]
    public void CreditMemoStatus_ShouldHaveCorrectValues(CreditMemoStatus status, int expectedValue)
    {
        // Assert
        ((int)status).Should().Be(expectedValue);
    }

    [Fact]
    public void CreditMemoStatus_ShouldHave8Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<CreditMemoStatus>();

        // Assert
        values.Should().HaveCount(8);
    }

    #endregion

    #region CreditMemoReason Enum

    [Theory]
    [InlineData(CreditMemoReason.Return, 0)]
    [InlineData(CreditMemoReason.BillingError, 1)]
    [InlineData(CreditMemoReason.PriceAdjustment, 2)]
    [InlineData(CreditMemoReason.Goodwill, 3)]
    [InlineData(CreditMemoReason.ServiceCredit, 4)]
    [InlineData(CreditMemoReason.DuplicateCharge, 5)]
    [InlineData(CreditMemoReason.CancelledOrder, 6)]
    [InlineData(CreditMemoReason.EarlyTermination, 7)]
    [InlineData(CreditMemoReason.Promotion, 8)]
    [InlineData(CreditMemoReason.Downgrade, 9)]
    [InlineData(CreditMemoReason.Referral, 10)]
    [InlineData(CreditMemoReason.Other, 11)]
    public void CreditMemoReason_ShouldHaveCorrectValues(CreditMemoReason reason, int expectedValue)
    {
        // Assert
        ((int)reason).Should().Be(expectedValue);
    }

    [Fact]
    public void CreditMemoReason_ShouldHave12Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<CreditMemoReason>();

        // Assert
        values.Should().HaveCount(12);
    }

    #endregion

    #region CreditMemo Entity Tests

    [Fact]
    public void CreditMemo_ShouldInitializeWithDefaults()
    {
        // Act
        var creditMemo = new CreditMemo();

        // Assert
        creditMemo.CreditMemoNumber.Should().BeEmpty();
        creditMemo.ExternalCreditMemoId.Should().BeNull();
        creditMemo.Status.Should().Be(CreditMemoStatus.Draft);
        creditMemo.Reason.Should().Be(CreditMemoReason.Other);
        creditMemo.Amount.Should().Be(0);
        creditMemo.AmountApplied.Should().Be(0);
        creditMemo.AmountRefunded.Should().Be(0);
        creditMemo.TaxAmount.Should().Be(0);
        creditMemo.CurrencyCode.Should().Be("USD");
        creditMemo.Applications.Should().NotBeNull();
        creditMemo.LineItems.Should().NotBeNull();
    }

    [Fact]
    public void CreditMemo_ShouldSetAllProperties()
    {
        // Arrange
        var account = new Account { Id = 1, Company = "Acme Corp" };
        var user = new User { Id = 1, Username = "admin" };

        // Act
        var creditMemo = new CreditMemo
        {
            Id = 1,
            CreditMemoNumber = "CM-2026-001",
            ExternalCreditMemoId = "EXT-123",
            Description = "Return credit",
            Status = CreditMemoStatus.Approved,
            Reason = CreditMemoReason.Return,
            ReasonDetails = "Product defect",
            CreditMemoDate = new DateTime(2026, 1, 15),
            ApprovedDate = new DateTime(2026, 1, 16),
            ExpirationDate = new DateTime(2026, 7, 15),
            Amount = 500.00m,
            AmountApplied = 200.00m,
            AmountRefunded = 0,
            TaxAmount = 40.00m,
            CurrencyCode = "EUR",
            AccountId = 1,
            Account = account,
            CreatedById = 1,
            CreatedBy = user,
            ApprovedById = 1,
            ApprovedBy = user,
            InternalNotes = "Internal note",
            CustomerNotes = "Customer note",
        };

        // Assert
        creditMemo.CreditMemoNumber.Should().Be("CM-2026-001");
        creditMemo.Status.Should().Be(CreditMemoStatus.Approved);
        creditMemo.Reason.Should().Be(CreditMemoReason.Return);
        creditMemo.Amount.Should().Be(500.00m);
        creditMemo.AmountApplied.Should().Be(200.00m);
        creditMemo.CurrencyCode.Should().Be("EUR");
    }

    [Fact]
    public void CreditMemo_BalanceRemaining_ShouldCalculateCorrectly()
    {
        // Arrange
        var creditMemo = new CreditMemo
        {
            Amount = 1000.00m,
            AmountApplied = 400.00m,
            AmountRefunded = 200.00m,
        };

        // Assert
        creditMemo.BalanceRemaining.Should().Be(400.00m);
    }

    [Fact]
    public void CreditMemo_FullyApplied_ShouldHaveZeroBalance()
    {
        // Arrange
        var creditMemo = new CreditMemo
        {
            Amount = 1000.00m,
            AmountApplied = 1000.00m,
            AmountRefunded = 0,
        };

        // Assert
        creditMemo.BalanceRemaining.Should().Be(0);
    }

    #endregion

    #region CreditMemoLineItem Entity Tests

    [Fact]
    public void CreditMemoLineItem_ShouldInitializeWithDefaults()
    {
        // Act
        var lineItem = new CreditMemoLineItem();

        // Assert
        lineItem.LineNumber.Should().Be(0);
        lineItem.Name.Should().BeEmpty();
        lineItem.Quantity.Should().Be(1);
        lineItem.UnitPrice.Should().Be(0);
        lineItem.Amount.Should().Be(0);
    }

    [Fact]
    public void CreditMemoLineItem_ShouldSetAllProperties()
    {
        // Arrange
        var creditMemo = new CreditMemo { Id = 1 };
        var product = new Product { Id = 1, Name = "Product A" };

        // Act
        var lineItem = new CreditMemoLineItem
        {
            Id = 1,
            LineNumber = 1,
            Name = "Credit for Product A",
            Description = "Returned item",
            Quantity = 2,
            UnitPrice = 50.00m,
            Amount = 100.00m,
            CreditMemoId = 1,
            CreditMemo = creditMemo,
            ProductId = 1,
            Product = product,
        };

        // Assert
        lineItem.LineNumber.Should().Be(1);
        lineItem.Name.Should().Be("Credit for Product A");
        lineItem.Quantity.Should().Be(2);
        lineItem.UnitPrice.Should().Be(50.00m);
        lineItem.Amount.Should().Be(100.00m);
    }

    #endregion

    #region CreditApplication Entity Tests

    [Fact]
    public void CreditApplication_ShouldInitializeWithDefaults()
    {
        // Act
        var application = new CreditApplication();

        // Assert
        application.Amount.Should().Be(0);
        application.Notes.Should().BeNull();
    }

    [Fact]
    public void CreditApplication_ShouldSetAllProperties()
    {
        // Arrange
        var creditMemo = new CreditMemo { Id = 1 };
        var invoice = new Invoice { Id = 1 };
        var user = new User { Id = 1, Username = "admin" };

        // Act
        var application = new CreditApplication
        {
            Id = 1,
            Amount = 250.00m,
            AppliedDate = new DateTime(2026, 1, 20),
            CreditMemoId = 1,
            CreditMemo = creditMemo,
            InvoiceId = 1,
            Invoice = invoice,
            AppliedById = 1,
            AppliedBy = user,
            Notes = "Applied to invoice",
        };

        // Assert
        application.Amount.Should().Be(250.00m);
        application.InvoiceId.Should().Be(1);
        application.AppliedById.Should().Be(1);
    }

    #endregion

    #region ESignatureStatus Enum

    [Theory]
    [InlineData(ESignatureStatus.Draft, 0)]
    [InlineData(ESignatureStatus.Sent, 1)]
    [InlineData(ESignatureStatus.Viewed, 2)]
    [InlineData(ESignatureStatus.PartiallySigned, 3)]
    [InlineData(ESignatureStatus.Completed, 4)]
    [InlineData(ESignatureStatus.Declined, 5)]
    [InlineData(ESignatureStatus.Voided, 6)]
    [InlineData(ESignatureStatus.Expired, 7)]
    [InlineData(ESignatureStatus.AuthenticationFailed, 8)]
    [InlineData(ESignatureStatus.DeliveryFailed, 9)]
    public void ESignatureStatus_ShouldHaveCorrectValues(ESignatureStatus status, int expectedValue)
    {
        // Assert
        ((int)status).Should().Be(expectedValue);
    }

    [Fact]
    public void ESignatureStatus_ShouldHave10Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<ESignatureStatus>();

        // Assert
        values.Should().HaveCount(10);
    }

    #endregion

    #region ESignatureProvider Enum

    [Theory]
    [InlineData(ESignatureProvider.DocuSign, 0)]
    [InlineData(ESignatureProvider.AdobeSign, 1)]
    [InlineData(ESignatureProvider.HelloSign, 2)]
    [InlineData(ESignatureProvider.PandaDoc, 3)]
    [InlineData(ESignatureProvider.SignNow, 4)]
    [InlineData(ESignatureProvider.BuiltIn, 5)]
    public void ESignatureProvider_ShouldHaveCorrectValues(ESignatureProvider provider, int expectedValue)
    {
        // Assert
        ((int)provider).Should().Be(expectedValue);
    }

    #endregion

    #region SignableDocumentType Enum

    [Theory]
    [InlineData(SignableDocumentType.Quote, 0)]
    [InlineData(SignableDocumentType.Contract, 1)]
    [InlineData(SignableDocumentType.OrderForm, 2)]
    [InlineData(SignableDocumentType.NDA, 3)]
    [InlineData(SignableDocumentType.SOW, 4)]
    [InlineData(SignableDocumentType.MSA, 5)]
    [InlineData(SignableDocumentType.Amendment, 6)]
    [InlineData(SignableDocumentType.Renewal, 7)]
    [InlineData(SignableDocumentType.Other, 8)]
    public void SignableDocumentType_ShouldHaveCorrectValues(SignableDocumentType docType, int expectedValue)
    {
        // Assert
        ((int)docType).Should().Be(expectedValue);
    }

    #endregion

    #region SignerStatus Enum

    [Theory]
    [InlineData(SignerStatus.Waiting, 0)]
    [InlineData(SignerStatus.Pending, 1)]
    [InlineData(SignerStatus.Sent, 2)]
    [InlineData(SignerStatus.Delivered, 3)]
    [InlineData(SignerStatus.Viewed, 4)]
    [InlineData(SignerStatus.Signed, 5)]
    [InlineData(SignerStatus.Declined, 6)]
    [InlineData(SignerStatus.DeliveryFailed, 7)]
    [InlineData(SignerStatus.AuthFailed, 8)]
    public void SignerStatus_ShouldHaveCorrectValues(SignerStatus status, int expectedValue)
    {
        // Assert
        ((int)status).Should().Be(expectedValue);
    }

    #endregion

    #region SignerRole Enum

    [Theory]
    [InlineData(SignerRole.Signer, 0)]
    [InlineData(SignerRole.CoSigner, 1)]
    [InlineData(SignerRole.CounterSigner, 2)]
    [InlineData(SignerRole.CarbonCopy, 3)]
    [InlineData(SignerRole.Witness, 4)]
    [InlineData(SignerRole.Approver, 5)]
    [InlineData(SignerRole.InPersonSigner, 6)]
    public void SignerRole_ShouldHaveCorrectValues(SignerRole role, int expectedValue)
    {
        // Assert
        ((int)role).Should().Be(expectedValue);
    }

    #endregion

    #region ESignatureRequest Entity Tests

    [Fact]
    public void ESignatureRequest_ShouldInitializeWithDefaults()
    {
        // Act
        var request = new ESignatureRequest();

        // Assert
        request.RequestNumber.Should().BeEmpty();
        request.ExternalEnvelopeId.Should().BeNull();
    }

    [Fact]
    public void ESignatureRequest_ShouldSetIdentification()
    {
        // Act
        var request = new ESignatureRequest
        {
            RequestNumber = "ESIGN-2026-001",
            ExternalEnvelopeId = "env-12345-abcd",
        };

        // Assert
        request.RequestNumber.Should().Be("ESIGN-2026-001");
        request.ExternalEnvelopeId.Should().Be("env-12345-abcd");
    }

    #endregion

    #region OrderStatus Enum

    [Theory]
    [InlineData(OrderStatus.Draft, 0)]
    [InlineData(OrderStatus.PendingApproval, 1)]
    [InlineData(OrderStatus.Approved, 2)]
    [InlineData(OrderStatus.Processing, 3)]
    [InlineData(OrderStatus.PartiallyFulfilled, 4)]
    [InlineData(OrderStatus.Fulfilled, 5)]
    [InlineData(OrderStatus.Delivered, 6)]
    [InlineData(OrderStatus.Completed, 7)]
    [InlineData(OrderStatus.Cancelled, 8)]
    [InlineData(OrderStatus.Returned, 9)]
    [InlineData(OrderStatus.Refunded, 10)]
    [InlineData(OrderStatus.OnHold, 11)]
    [InlineData(OrderStatus.ActionRequired, 12)]
    public void OrderStatus_ShouldHaveCorrectValues(OrderStatus status, int expectedValue)
    {
        // Assert
        ((int)status).Should().Be(expectedValue);
    }

    [Fact]
    public void OrderStatus_ShouldHave13Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<OrderStatus>();

        // Assert
        values.Should().HaveCount(13);
    }

    #endregion

    #region OrderType Enum

    [Theory]
    [InlineData(OrderType.Standard, 0)]
    [InlineData(OrderType.Renewal, 1)]
    [InlineData(OrderType.Upgrade, 2)]
    [InlineData(OrderType.Downgrade, 3)]
    [InlineData(OrderType.Amendment, 4)]
    [InlineData(OrderType.TrialConversion, 5)]
    [InlineData(OrderType.Trial, 6)]
    [InlineData(OrderType.Partner, 7)]
    [InlineData(OrderType.Internal, 8)]
    [InlineData(OrderType.Return, 9)]
    [InlineData(OrderType.Credit, 10)]
    [InlineData(OrderType.MultiYear, 11)]
    public void OrderType_ShouldHaveCorrectValues(OrderType orderType, int expectedValue)
    {
        // Assert
        ((int)orderType).Should().Be(expectedValue);
    }

    [Fact]
    public void OrderType_ShouldHave12Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<OrderType>();

        // Assert
        values.Should().HaveCount(12);
    }

    #endregion

    #region FulfillmentMethod Enum

    [Theory]
    [InlineData(FulfillmentMethod.Ship, 0)]
    [InlineData(FulfillmentMethod.Digital, 1)]
    [InlineData(FulfillmentMethod.Pickup, 2)]
    [InlineData(FulfillmentMethod.Provision, 3)]
    [InlineData(FulfillmentMethod.Activate, 4)]
    [InlineData(FulfillmentMethod.ServiceDelivery, 5)]
    [InlineData(FulfillmentMethod.ThirdParty, 6)]
    [InlineData(FulfillmentMethod.None, 7)]
    public void FulfillmentMethod_ShouldHaveCorrectValues(FulfillmentMethod method, int expectedValue)
    {
        // Assert
        ((int)method).Should().Be(expectedValue);
    }

    #endregion

    #region OrderPriority Enum

    [Theory]
    [InlineData(OrderPriority.Normal, 0)]
    [InlineData(OrderPriority.High, 1)]
    [InlineData(OrderPriority.Urgent, 2)]
    [InlineData(OrderPriority.Low, 3)]
    [InlineData(OrderPriority.Critical, 4)]
    public void OrderPriority_ShouldHaveCorrectValues(OrderPriority priority, int expectedValue)
    {
        // Assert
        ((int)priority).Should().Be(expectedValue);
    }

    #endregion

    #region Order Entity Tests

    [Fact]
    public void Order_ShouldInitializeWithDefaults()
    {
        // Act
        var order = new Order();

        // Assert
        order.OrderNumber.Should().BeEmpty();
        order.Name.Should().BeEmpty();
        order.Status.Should().Be(OrderStatus.Draft);
    }

    [Fact]
    public void Order_ShouldSetIdentification()
    {
        // Act
        var order = new Order
        {
            OrderNumber = "ORD-2026-001",
            ExternalOrderId = "EXT-ORD-123",
            CustomerPONumber = "PO-12345",
            ReferenceNumber = "REF-001",
            Name = "Annual Subscription Order",
            Description = "1-year software license",
            Status = OrderStatus.Approved,
        };

        // Assert
        order.OrderNumber.Should().Be("ORD-2026-001");
        order.ExternalOrderId.Should().Be("EXT-ORD-123");
        order.CustomerPONumber.Should().Be("PO-12345");
        order.Name.Should().Be("Annual Subscription Order");
        order.Status.Should().Be(OrderStatus.Approved);
    }

    #endregion

    #region QuotaPeriodType Enum

    [Theory]
    [InlineData(QuotaPeriodType.Monthly, 0)]
    [InlineData(QuotaPeriodType.Quarterly, 1)]
    [InlineData(QuotaPeriodType.Annual, 2)]
    [InlineData(QuotaPeriodType.SemiAnnual, 3)]
    [InlineData(QuotaPeriodType.Weekly, 4)]
    [InlineData(QuotaPeriodType.Custom, 5)]
    public void QuotaPeriodType_ShouldHaveCorrectValues(QuotaPeriodType periodType, int expectedValue)
    {
        // Assert
        ((int)periodType).Should().Be(expectedValue);
    }

    #endregion

    #region QuotaMetric Enum

    [Theory]
    [InlineData(QuotaMetric.Revenue, 0)]
    [InlineData(QuotaMetric.DealsCount, 1)]
    [InlineData(QuotaMetric.UnitsSold, 2)]
    [InlineData(QuotaMetric.NewCustomers, 3)]
    [InlineData(QuotaMetric.PipelineCreated, 4)]
    [InlineData(QuotaMetric.MeetingsBooked, 5)]
    [InlineData(QuotaMetric.QualifiedLeads, 6)]
    [InlineData(QuotaMetric.RecurringRevenue, 7)]
    [InlineData(QuotaMetric.CallsMade, 8)]
    [InlineData(QuotaMetric.Custom, 9)]
    public void QuotaMetric_ShouldHaveCorrectValues(QuotaMetric metric, int expectedValue)
    {
        // Assert
        ((int)metric).Should().Be(expectedValue);
    }

    #endregion

    #region ForecastCategory Enum

    [Theory]
    [InlineData(ForecastCategory.Pipeline, 0)]
    [InlineData(ForecastCategory.BestCase, 1)]
    [InlineData(ForecastCategory.Commit, 2)]
    [InlineData(ForecastCategory.ClosedWon, 3)]
    [InlineData(ForecastCategory.Omitted, 4)]
    [InlineData(ForecastCategory.MostLikely, 5)]
    public void ForecastCategory_ShouldHaveCorrectValues(ForecastCategory category, int expectedValue)
    {
        // Assert
        ((int)category).Should().Be(expectedValue);
    }

    #endregion

    #region SalesQuota Entity Tests

    [Fact]
    public void SalesQuota_ShouldInitializeWithDefaults()
    {
        // Act
        var quota = new SalesQuota();

        // Assert
        quota.Name.Should().BeEmpty();
        quota.PeriodType.Should().Be(QuotaPeriodType.Quarterly);
        quota.Metric.Should().Be(QuotaMetric.Revenue);
        quota.Period.Should().BeEmpty();
        quota.TargetAmount.Should().Be(0);
        quota.ActualAmount.Should().Be(0);
        quota.CurrencyCode.Should().Be("USD");
    }

    [Fact]
    public void SalesQuota_ShouldSetAllProperties()
    {
        // Act
        var quota = new SalesQuota
        {
            Id = 1,
            Name = "Q1 2026 Revenue Quota",
            PeriodType = QuotaPeriodType.Quarterly,
            Metric = QuotaMetric.Revenue,
            Period = "2026-Q1",
            FiscalYear = 2026,
            FiscalQuarter = 1,
            PeriodStartDate = new DateTime(2026, 1, 1),
            PeriodEndDate = new DateTime(2026, 3, 31),
            TargetAmount = 100000.00m,
            StretchTargetAmount = 120000.00m,
            MinimumTargetAmount = 80000.00m,
            ActualAmount = 75000.00m,
            CurrencyCode = "USD",
            NewBusinessAmount = 50000.00m,
            RenewalAmount = 20000.00m,
            ExpansionAmount = 5000.00m,
        };

        // Assert
        quota.Name.Should().Be("Q1 2026 Revenue Quota");
        quota.PeriodType.Should().Be(QuotaPeriodType.Quarterly);
        quota.FiscalYear.Should().Be(2026);
        quota.FiscalQuarter.Should().Be(1);
        quota.TargetAmount.Should().Be(100000.00m);
        quota.ActualAmount.Should().Be(75000.00m);
    }

    [Fact]
    public void SalesQuota_AttainmentPercent_ShouldCalculateCorrectly()
    {
        // Arrange
        var quota = new SalesQuota
        {
            TargetAmount = 100000.00m,
            ActualAmount = 75000.00m,
        };

        // Assert
        quota.AttainmentPercent.Should().Be(75m);
    }

    [Fact]
    public void SalesQuota_AttainmentPercent_ShouldReturnZero_WhenTargetIsZero()
    {
        // Arrange
        var quota = new SalesQuota
        {
            TargetAmount = 0,
            ActualAmount = 50000.00m,
        };

        // Assert
        quota.AttainmentPercent.Should().Be(0);
    }

    [Fact]
    public void SalesQuota_Variance_ShouldCalculateCorrectly()
    {
        // Arrange
        var quota = new SalesQuota
        {
            TargetAmount = 100000.00m,
            ActualAmount = 75000.00m,
        };

        // Assert
        quota.Variance.Should().Be(-25000.00m);
    }

    [Fact]
    public void SalesQuota_GapToTarget_ShouldCalculateCorrectly()
    {
        // Arrange
        var quota = new SalesQuota
        {
            TargetAmount = 100000.00m,
            ActualAmount = 75000.00m,
        };

        // Assert
        quota.GapToTarget.Should().Be(25000.00m);
    }

    [Fact]
    public void SalesQuota_GapToTarget_ShouldReturnZero_WhenExceeded()
    {
        // Arrange
        var quota = new SalesQuota
        {
            TargetAmount = 100000.00m,
            ActualAmount = 120000.00m,
        };

        // Assert
        quota.GapToTarget.Should().Be(0);
    }

    [Fact]
    public void SalesQuota_IsAchieved_ShouldReturnTrue_WhenTargetMet()
    {
        // Arrange
        var quotaExact = new SalesQuota
        {
            TargetAmount = 100000.00m,
            ActualAmount = 100000.00m,
        };

        var quotaExceeded = new SalesQuota
        {
            TargetAmount = 100000.00m,
            ActualAmount = 120000.00m,
        };

        // Assert
        quotaExact.IsAchieved.Should().BeTrue();
        quotaExceeded.IsAchieved.Should().BeTrue();
    }

    [Fact]
    public void SalesQuota_IsAchieved_ShouldReturnFalse_WhenBelowTarget()
    {
        // Arrange
        var quota = new SalesQuota
        {
            TargetAmount = 100000.00m,
            ActualAmount = 99999.00m,
        };

        // Assert
        quota.IsAchieved.Should().BeFalse();
    }

    [Theory]
    [InlineData(QuotaPeriodType.Monthly, "Monthly quota")]
    [InlineData(QuotaPeriodType.Quarterly, "Quarterly quota")]
    [InlineData(QuotaPeriodType.Annual, "Annual quota")]
    public void SalesQuota_ShouldSupportVariousPeriodTypes(QuotaPeriodType periodType, string description)
    {
        // Act
        var quota = new SalesQuota
        {
            Name = description,
            PeriodType = periodType,
        };

        // Assert
        quota.PeriodType.Should().Be(periodType);
    }

    [Theory]
    [InlineData(QuotaMetric.Revenue)]
    [InlineData(QuotaMetric.DealsCount)]
    [InlineData(QuotaMetric.NewCustomers)]
    [InlineData(QuotaMetric.RecurringRevenue)]
    public void SalesQuota_ShouldSupportVariousMetrics(QuotaMetric metric)
    {
        // Act
        var quota = new SalesQuota
        {
            Metric = metric,
            TargetAmount = 100m,
        };

        // Assert
        quota.Metric.Should().Be(metric);
    }

    #endregion
}
