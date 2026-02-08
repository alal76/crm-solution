// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 CRM Solution Team
// Unit tests for Quote, Invoice, and Order entities

using CRM.Core.Entities;
using CRM.Core.Models;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Tests for Quote, Invoice, Order entities and their related enums
/// </summary>
public class QuoteInvoiceEntityTests
{
    #region QuoteStatus Enum Tests

    [Fact]
    public void QuoteStatus_ShouldHaveExpectedValues()
    {
        QuoteStatus.New.Should().Be((QuoteStatus)0);
        QuoteStatus.Draft.Should().Be((QuoteStatus)1);
        QuoteStatus.UnderApproval.Should().Be((QuoteStatus)2);
        QuoteStatus.Approved.Should().Be((QuoteStatus)3);
        QuoteStatus.Shared.Should().Be((QuoteStatus)4);
        QuoteStatus.Viewed.Should().Be((QuoteStatus)5);
        QuoteStatus.Accepted.Should().Be((QuoteStatus)6);
        QuoteStatus.Rejected.Should().Be((QuoteStatus)7);
        QuoteStatus.Expired.Should().Be((QuoteStatus)8);
        QuoteStatus.Revised.Should().Be((QuoteStatus)9);
        QuoteStatus.Cancelled.Should().Be((QuoteStatus)10);
        QuoteStatus.Converted.Should().Be((QuoteStatus)11);
        QuoteStatus.EndOfLife.Should().Be((QuoteStatus)12);
    }

    [Fact]
    public void QuoteStatus_ShouldHave13Values()
    {
        var values = Enum.GetValues<QuoteStatus>();
        values.Should().HaveCount(13);
    }

    [Theory]
    [InlineData(QuoteStatus.New, "New")]
    [InlineData(QuoteStatus.Draft, "Draft")]
    [InlineData(QuoteStatus.Accepted, "Accepted")]
    [InlineData(QuoteStatus.Rejected, "Rejected")]
    [InlineData(QuoteStatus.Expired, "Expired")]
    [InlineData(QuoteStatus.Converted, "Converted")]
    public void QuoteStatus_ShouldHaveCorrectName(QuoteStatus status, string expectedName)
    {
        status.ToString().Should().Be(expectedName);
    }

    #endregion

    #region LineItemDiscountType Enum Tests

    [Fact]
    public void LineItemDiscountType_ShouldHaveExpectedValues()
    {
        LineItemDiscountType.None.Should().Be((LineItemDiscountType)0);
        LineItemDiscountType.Percentage.Should().Be((LineItemDiscountType)1);
        LineItemDiscountType.FixedAmount.Should().Be((LineItemDiscountType)2);
    }

    [Fact]
    public void LineItemDiscountType_ShouldHave3Values()
    {
        var values = Enum.GetValues<LineItemDiscountType>();
        values.Should().HaveCount(3);
    }

    #endregion

    #region InvoiceStatus Enum Tests

    [Fact]
    public void InvoiceStatus_ShouldHaveExpectedValues()
    {
        InvoiceStatus.Draft.Should().Be((InvoiceStatus)0);
        InvoiceStatus.PendingApproval.Should().Be((InvoiceStatus)1);
        InvoiceStatus.Approved.Should().Be((InvoiceStatus)2);
        InvoiceStatus.Sent.Should().Be((InvoiceStatus)3);
        InvoiceStatus.Viewed.Should().Be((InvoiceStatus)4);
        InvoiceStatus.PartiallyPaid.Should().Be((InvoiceStatus)5);
        InvoiceStatus.Paid.Should().Be((InvoiceStatus)6);
        InvoiceStatus.Overdue.Should().Be((InvoiceStatus)7);
        InvoiceStatus.Disputed.Should().Be((InvoiceStatus)8);
        InvoiceStatus.Voided.Should().Be((InvoiceStatus)9);
        InvoiceStatus.WrittenOff.Should().Be((InvoiceStatus)10);
        InvoiceStatus.Collections.Should().Be((InvoiceStatus)11);
        InvoiceStatus.Refunded.Should().Be((InvoiceStatus)12);
    }

    [Fact]
    public void InvoiceStatus_ShouldHave13Values()
    {
        var values = Enum.GetValues<InvoiceStatus>();
        values.Should().HaveCount(13);
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft, "Draft")]
    [InlineData(InvoiceStatus.PendingApproval, "PendingApproval")]
    [InlineData(InvoiceStatus.Approved, "Approved")]
    [InlineData(InvoiceStatus.Sent, "Sent")]
    [InlineData(InvoiceStatus.Paid, "Paid")]
    [InlineData(InvoiceStatus.Overdue, "Overdue")]
    [InlineData(InvoiceStatus.Voided, "Voided")]
    [InlineData(InvoiceStatus.Collections, "Collections")]
    public void InvoiceStatus_ShouldHaveCorrectName(InvoiceStatus status, string expectedName)
    {
        status.ToString().Should().Be(expectedName);
    }

    #endregion

    #region InvoiceType Enum Tests

    [Fact]
    public void InvoiceType_ShouldHaveExpectedValues()
    {
        InvoiceType.Standard.Should().Be((InvoiceType)0);
        InvoiceType.Credit.Should().Be((InvoiceType)1);
        InvoiceType.Proforma.Should().Be((InvoiceType)2);
        InvoiceType.Recurring.Should().Be((InvoiceType)3);
        InvoiceType.Deposit.Should().Be((InvoiceType)4);
        InvoiceType.Progress.Should().Be((InvoiceType)5);
        InvoiceType.Final.Should().Be((InvoiceType)6);
        InvoiceType.Adjustment.Should().Be((InvoiceType)7);
        InvoiceType.DebitMemo.Should().Be((InvoiceType)8);
    }

    [Fact]
    public void InvoiceType_ShouldHave9Values()
    {
        var values = Enum.GetValues<InvoiceType>();
        values.Should().HaveCount(9);
    }

    #endregion

    #region PaymentTerms Enum Tests

    [Fact]
    public void PaymentTerms_ShouldHaveExpectedValues()
    {
        PaymentTerms.DueOnReceipt.Should().Be((PaymentTerms)0);
        PaymentTerms.Net7.Should().Be((PaymentTerms)1);
        PaymentTerms.Net10.Should().Be((PaymentTerms)2);
        PaymentTerms.Net15.Should().Be((PaymentTerms)3);
        PaymentTerms.Net30.Should().Be((PaymentTerms)4);
        PaymentTerms.Net45.Should().Be((PaymentTerms)5);
        PaymentTerms.Net60.Should().Be((PaymentTerms)6);
        PaymentTerms.Net90.Should().Be((PaymentTerms)7);
        PaymentTerms.TwoTenNet30.Should().Be((PaymentTerms)8);
        PaymentTerms.EndOfMonth.Should().Be((PaymentTerms)9);
        PaymentTerms.Custom.Should().Be((PaymentTerms)10);
    }

    [Fact]
    public void PaymentTerms_ShouldHave11Values()
    {
        var values = Enum.GetValues<PaymentTerms>();
        values.Should().HaveCount(11);
    }

    #endregion

    #region OrderStatus Enum Tests

    [Fact]
    public void OrderStatus_ShouldHaveExpectedValues()
    {
        OrderStatus.Draft.Should().Be((OrderStatus)0);
        OrderStatus.PendingApproval.Should().Be((OrderStatus)1);
        OrderStatus.Approved.Should().Be((OrderStatus)2);
        OrderStatus.Processing.Should().Be((OrderStatus)3);
        OrderStatus.PartiallyFulfilled.Should().Be((OrderStatus)4);
        OrderStatus.Fulfilled.Should().Be((OrderStatus)5);
        OrderStatus.Delivered.Should().Be((OrderStatus)6);
        OrderStatus.Completed.Should().Be((OrderStatus)7);
        OrderStatus.Cancelled.Should().Be((OrderStatus)8);
        OrderStatus.Returned.Should().Be((OrderStatus)9);
        OrderStatus.Refunded.Should().Be((OrderStatus)10);
        OrderStatus.OnHold.Should().Be((OrderStatus)11);
        OrderStatus.ActionRequired.Should().Be((OrderStatus)12);
    }

    [Fact]
    public void OrderStatus_ShouldHave13Values()
    {
        var values = Enum.GetValues<OrderStatus>();
        values.Should().HaveCount(13);
    }

    #endregion

    #region OrderType Enum Tests

    [Fact]
    public void OrderType_ShouldHaveExpectedValues()
    {
        OrderType.Standard.Should().Be((OrderType)0);
        OrderType.Renewal.Should().Be((OrderType)1);
        OrderType.Upgrade.Should().Be((OrderType)2);
        OrderType.Downgrade.Should().Be((OrderType)3);
        OrderType.Amendment.Should().Be((OrderType)4);
        OrderType.TrialConversion.Should().Be((OrderType)5);
        OrderType.Trial.Should().Be((OrderType)6);
        OrderType.Partner.Should().Be((OrderType)7);
        OrderType.Internal.Should().Be((OrderType)8);
        OrderType.Return.Should().Be((OrderType)9);
        OrderType.Credit.Should().Be((OrderType)10);
        OrderType.MultiYear.Should().Be((OrderType)11);
    }

    [Fact]
    public void OrderType_ShouldHave12Values()
    {
        var values = Enum.GetValues<OrderType>();
        values.Should().HaveCount(12);
    }

    #endregion

    #region FulfillmentMethod Enum Tests

    [Fact]
    public void FulfillmentMethod_ShouldHaveExpectedValues()
    {
        FulfillmentMethod.Ship.Should().Be((FulfillmentMethod)0);
        FulfillmentMethod.Digital.Should().Be((FulfillmentMethod)1);
        FulfillmentMethod.Pickup.Should().Be((FulfillmentMethod)2);
        FulfillmentMethod.Provision.Should().Be((FulfillmentMethod)3);
        FulfillmentMethod.Activate.Should().Be((FulfillmentMethod)4);
        FulfillmentMethod.ServiceDelivery.Should().Be((FulfillmentMethod)5);
        FulfillmentMethod.ThirdParty.Should().Be((FulfillmentMethod)6);
        FulfillmentMethod.None.Should().Be((FulfillmentMethod)7);
    }

    [Fact]
    public void FulfillmentMethod_ShouldHave8Values()
    {
        var values = Enum.GetValues<FulfillmentMethod>();
        values.Should().HaveCount(8);
    }

    #endregion

    #region OrderPriority Enum Tests

    [Fact]
    public void OrderPriority_ShouldHaveExpectedValues()
    {
        OrderPriority.Normal.Should().Be((OrderPriority)0);
        OrderPriority.High.Should().Be((OrderPriority)1);
        OrderPriority.Urgent.Should().Be((OrderPriority)2);
        OrderPriority.Low.Should().Be((OrderPriority)3);
        OrderPriority.Critical.Should().Be((OrderPriority)4);
    }

    [Fact]
    public void OrderPriority_ShouldHave5Values()
    {
        var values = Enum.GetValues<OrderPriority>();
        values.Should().HaveCount(5);
    }

    #endregion

    #region Quote Entity Tests

    [Fact]
    public void Quote_ShouldInitializeWithDefaultValues()
    {
        // Act
        var quote = new Quote();

        // Assert - Identification
        quote.QuoteNumber.Should().Be(string.Empty);
        quote.ExternalQuoteId.Should().BeNull();
        quote.Version.Should().Be(1);

        // Assert - Basic Information
        quote.Name.Should().Be(string.Empty);
        quote.Description.Should().BeNull();
        quote.Status.Should().Be(QuoteStatus.New);

        // Assert - Dates
        quote.QuoteDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        quote.ExpirationDate.Should().BeNull();
        quote.SentDate.Should().BeNull();
        quote.ViewedDate.Should().BeNull();
        quote.AcceptedDate.Should().BeNull();
        quote.RejectedDate.Should().BeNull();

        // Assert - Pricing
        quote.Subtotal.Should().Be(0);
        quote.Discount.Should().Be(0);
        quote.DiscountPercent.Should().Be(0);
        quote.DiscountReason.Should().BeNull();
        quote.Tax.Should().Be(0);
        quote.TaxRate.Should().Be(0);
        quote.ShippingCost.Should().Be(0);
        quote.Total.Should().Be(0);
        quote.CurrencyCode.Should().Be("USD");

        // Assert - Terms
        quote.PaymentTerms.Should().BeNull();
        quote.DeliveryTerms.Should().BeNull();
        quote.TermsAndConditions.Should().BeNull();
        quote.Warranty.Should().BeNull();
        quote.ValidityDays.Should().Be(30);

        // Assert - Approval
        quote.RequiresApproval.Should().BeFalse();
        quote.IsApproved.Should().BeFalse();
        quote.ApprovalDate.Should().BeNull();
        quote.ApprovalNotes.Should().BeNull();

        // Assert - Signature
        quote.IsSigned.Should().BeFalse();
        quote.SignedDate.Should().BeNull();
        quote.SignedBy.Should().BeNull();
        quote.SignatureUrl.Should().BeNull();
    }

    [Fact]
    public void Quote_CanSetAllProperties()
    {
        // Arrange
        var quote = new Quote();
        var now = DateTime.UtcNow;

        // Act - Identification
        quote.QuoteNumber = "Q-2025-001";
        quote.ExternalQuoteId = "EXT-001";
        quote.Version = 2;

        // Act - Basic Information
        quote.Name = "Enterprise Software License";
        quote.Description = "Annual enterprise license agreement";
        quote.Status = QuoteStatus.Approved;

        // Act - Dates
        quote.QuoteDate = now;
        quote.ExpirationDate = now.AddDays(30);
        quote.SentDate = now.AddDays(1);
        quote.ViewedDate = now.AddDays(2);
        quote.AcceptedDate = now.AddDays(3);

        // Act - Pricing
        quote.Subtotal = 10000m;
        quote.Discount = 1000m;
        quote.DiscountPercent = 10m;
        quote.DiscountReason = "Volume discount";
        quote.Tax = 900m;
        quote.TaxRate = 10m;
        quote.ShippingCost = 100m;
        quote.Total = 10000m;
        quote.CurrencyCode = "EUR";

        // Assert
        quote.QuoteNumber.Should().Be("Q-2025-001");
        quote.ExternalQuoteId.Should().Be("EXT-001");
        quote.Version.Should().Be(2);
        quote.Name.Should().Be("Enterprise Software License");
        quote.Status.Should().Be(QuoteStatus.Approved);
        quote.Subtotal.Should().Be(10000m);
        quote.Discount.Should().Be(1000m);
        quote.Total.Should().Be(10000m);
        quote.CurrencyCode.Should().Be("EUR");
    }

    [Fact]
    public void Quote_IsExpired_ShouldReturnTrue_WhenExpirationDatePassed()
    {
        // Arrange
        var quote = new Quote
        {
            ExpirationDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        quote.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void Quote_IsExpired_ShouldReturnFalse_WhenExpirationDateInFuture()
    {
        // Arrange
        var quote = new Quote
        {
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act & Assert
        quote.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Quote_IsExpired_ShouldReturnFalse_WhenNoExpirationDate()
    {
        // Arrange
        var quote = new Quote
        {
            ExpirationDate = null
        };

        // Act & Assert
        quote.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Quote_CanEdit_ShouldReturnTrue_ForNewQuote()
    {
        // Arrange
        var quote = new Quote { Status = QuoteStatus.New };

        // Act & Assert
        quote.CanEdit.Should().BeTrue();
    }

    [Fact]
    public void Quote_CanEdit_ShouldReturnTrue_ForDraftQuote()
    {
        // Arrange
        var quote = new Quote { Status = QuoteStatus.Draft };

        // Act & Assert
        quote.CanEdit.Should().BeTrue();
    }

    [Fact]
    public void Quote_CanEdit_ShouldReturnFalse_ForApprovedQuote()
    {
        // Arrange
        var quote = new Quote { Status = QuoteStatus.Approved };

        // Act & Assert
        quote.CanEdit.Should().BeFalse();
    }

    [Fact]
    public void Quote_CanSubmitForApproval_ShouldReturnTrue_WhenDraftAndRequiresApprovalAndNotApproved()
    {
        // Arrange
        var quote = new Quote
        {
            Status = QuoteStatus.Draft,
            RequiresApproval = true,
            IsApproved = false
        };

        // Act & Assert
        quote.CanSubmitForApproval.Should().BeTrue();
    }

    [Fact]
    public void Quote_CanSubmitForApproval_ShouldReturnFalse_WhenAlreadyApproved()
    {
        // Arrange
        var quote = new Quote
        {
            Status = QuoteStatus.Draft,
            RequiresApproval = true,
            IsApproved = true
        };

        // Act & Assert
        quote.CanSubmitForApproval.Should().BeFalse();
    }

    [Fact]
    public void Quote_CanShare_ShouldReturnTrue_WhenDraftAndNoApprovalRequired()
    {
        // Arrange
        var quote = new Quote
        {
            Status = QuoteStatus.Draft,
            RequiresApproval = false
        };

        // Act & Assert
        quote.CanShare.Should().BeTrue();
    }

    [Fact]
    public void Quote_CanShare_ShouldReturnTrue_WhenApproved()
    {
        // Arrange
        var quote = new Quote
        {
            Status = QuoteStatus.Approved
        };

        // Act & Assert
        quote.CanShare.Should().BeTrue();
    }

    [Fact]
    public void Quote_CanShare_ShouldReturnFalse_WhenDraftAndApprovalRequired()
    {
        // Arrange
        var quote = new Quote
        {
            Status = QuoteStatus.Draft,
            RequiresApproval = true
        };

        // Act & Assert
        quote.CanShare.Should().BeFalse();
    }

    [Fact]
    public void Quote_RecalculateFromLineItems_WithNoLineItems_ShouldSetTotalToShippingOnly()
    {
        // Arrange
        var quote = new Quote
        {
            ShippingCost = 50m,
            QuoteLineItems = new List<QuoteLineItem>()
        };

        // Act
        quote.RecalculateFromLineItems();

        // Assert
        quote.Subtotal.Should().Be(0);
        quote.Tax.Should().Be(0);
        quote.Total.Should().Be(50m);
    }

    [Fact]
    public void Quote_RecalculateFromLineItems_WithNullLineItems_ShouldSetTotalToShippingOnly()
    {
        // Arrange
        var quote = new Quote
        {
            ShippingCost = 25m,
            QuoteLineItems = null
        };

        // Act
        quote.RecalculateFromLineItems();

        // Assert
        quote.Subtotal.Should().Be(0);
        quote.Tax.Should().Be(0);
        quote.Total.Should().Be(25m);
    }

    #endregion

    #region QuoteLineItem Entity Tests

    [Fact]
    public void QuoteLineItem_ShouldInitializeWithDefaultValues()
    {
        // Act
        var lineItem = new QuoteLineItem();

        // Assert
        lineItem.QuoteId.Should().Be(0);
        lineItem.LineNumber.Should().Be(0);
        lineItem.ProductId.Should().BeNull();
        lineItem.SKU.Should().BeNull();
        lineItem.Name.Should().Be(string.Empty);
        lineItem.Description.Should().BeNull();
        lineItem.Category.Should().BeNull();
        lineItem.Quantity.Should().Be(1);
        lineItem.UnitOfMeasure.Should().Be("each");
        lineItem.UnitPrice.Should().Be(0);
        lineItem.ListPrice.Should().BeNull();
        lineItem.CostPrice.Should().BeNull();
        lineItem.DiscountType.Should().Be(LineItemDiscountType.None);
        lineItem.DiscountPercent.Should().Be(0);
        lineItem.DiscountAmount.Should().Be(0);
        lineItem.DiscountReason.Should().BeNull();
        lineItem.DiscountRequiresApproval.Should().BeFalse();
        lineItem.DiscountApproved.Should().BeFalse();
        lineItem.TaxRate.Should().Be(0);
        lineItem.IsTaxable.Should().BeTrue();
        lineItem.TaxCode.Should().BeNull();
        lineItem.Subtotal.Should().Be(0);
        lineItem.TotalDiscount.Should().Be(0);
        lineItem.TaxAmount.Should().Be(0);
        lineItem.Total.Should().Be(0);
        lineItem.Margin.Should().BeNull();
    }

    [Fact]
    public void QuoteLineItem_CanSetAllProperties()
    {
        // Arrange
        var lineItem = new QuoteLineItem();

        // Act
        lineItem.QuoteId = 1;
        lineItem.LineNumber = 1;
        lineItem.ProductId = 100;
        lineItem.SKU = "SKU-001";
        lineItem.Name = "Product One";
        lineItem.Description = "Description of product";
        lineItem.Category = "Software";
        lineItem.Quantity = 10;
        lineItem.UnitOfMeasure = "license";
        lineItem.UnitPrice = 100m;
        lineItem.ListPrice = 120m;
        lineItem.CostPrice = 50m;
        lineItem.DiscountType = LineItemDiscountType.Percentage;
        lineItem.DiscountPercent = 10m;
        lineItem.DiscountAmount = 100m;
        lineItem.DiscountReason = "Bulk discount";
        lineItem.DiscountRequiresApproval = true;
        lineItem.DiscountApproved = true;
        lineItem.TaxRate = 8.25m;
        lineItem.IsTaxable = true;
        lineItem.TaxCode = "TX001";

        // Assert
        lineItem.QuoteId.Should().Be(1);
        lineItem.LineNumber.Should().Be(1);
        lineItem.ProductId.Should().Be(100);
        lineItem.SKU.Should().Be("SKU-001");
        lineItem.Name.Should().Be("Product One");
        lineItem.Quantity.Should().Be(10);
        lineItem.UnitPrice.Should().Be(100m);
        lineItem.DiscountType.Should().Be(LineItemDiscountType.Percentage);
        lineItem.DiscountPercent.Should().Be(10m);
        lineItem.TaxRate.Should().Be(8.25m);
    }

    #endregion

    #region Invoice Entity Tests

    [Fact]
    public void Invoice_ShouldInitializeWithDefaultValues()
    {
        // Act
        var invoice = new Invoice();

        // Assert - Identification
        invoice.InvoiceNumber.Should().Be(string.Empty);
        invoice.ExternalInvoiceId.Should().BeNull();
        invoice.ReferenceNumber.Should().BeNull();
        invoice.BatchNumber.Should().BeNull();

        // Assert - Invoice Details
        invoice.Description.Should().BeNull();
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.InvoiceType.Should().Be(InvoiceType.Standard);
        invoice.PaymentTerms.Should().Be(PaymentTerms.Net30);
        invoice.PaymentTermsDescription.Should().BeNull();

        // Assert - Dates
        invoice.InvoiceDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        invoice.SentDate.Should().BeNull();
        invoice.ViewedDate.Should().BeNull();
        invoice.PaidDate.Should().BeNull();
        invoice.VoidedDate.Should().BeNull();

        // Assert - Amounts
        invoice.Subtotal.Should().Be(0);
        invoice.DiscountAmount.Should().Be(0);
        invoice.DiscountPercent.Should().Be(0);
        invoice.TaxAmount.Should().Be(0);
        invoice.TaxRate.Should().Be(0);
        invoice.ShippingAmount.Should().Be(0);
        invoice.FeesAmount.Should().Be(0);
        invoice.TotalAmount.Should().Be(0);
        invoice.AmountPaid.Should().Be(0);
        invoice.AmountCredited.Should().Be(0);
        invoice.CurrencyCode.Should().Be("USD");
        invoice.ExchangeRate.Should().BeNull();

        // Assert - Dunning
        invoice.ReminderCount.Should().Be(0);
        invoice.LastReminderDate.Should().BeNull();
        invoice.NextReminderDate.Should().BeNull();
        invoice.InCollections.Should().BeFalse();
        invoice.CollectionsDate.Should().BeNull();
    }

    [Fact]
    public void Invoice_BalanceDue_ShouldCalculateCorrectly()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 1000m,
            AmountPaid = 300m,
            AmountCredited = 100m
        };

        // Act & Assert
        invoice.BalanceDue.Should().Be(600m);
    }

    [Fact]
    public void Invoice_BalanceDue_ShouldBeZero_WhenFullyPaid()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 1000m,
            AmountPaid = 1000m,
            AmountCredited = 0m
        };

        // Act & Assert
        invoice.BalanceDue.Should().Be(0m);
    }

    [Fact]
    public void Invoice_IsPaid_ShouldReturnTrue_WhenBalanceDueIsZero()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 1000m,
            AmountPaid = 1000m,
            AmountCredited = 0m
        };

        // Act & Assert
        invoice.IsPaid.Should().BeTrue();
    }

    [Fact]
    public void Invoice_IsPaid_ShouldReturnTrue_WhenOverpaid()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 1000m,
            AmountPaid = 1200m,
            AmountCredited = 0m
        };

        // Act & Assert
        invoice.IsPaid.Should().BeTrue();
    }

    [Fact]
    public void Invoice_IsPaid_ShouldReturnFalse_WhenPartiallyPaid()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 1000m,
            AmountPaid = 500m,
            AmountCredited = 0m
        };

        // Act & Assert
        invoice.IsPaid.Should().BeFalse();
    }

    [Fact]
    public void Invoice_DaysOverdue_ShouldReturnZero_WhenPaid()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 1000m,
            AmountPaid = 1000m,
            DueDate = DateTime.UtcNow.AddDays(-10)
        };

        // Act & Assert
        invoice.DaysOverdue.Should().Be(0);
    }

    [Fact]
    public void Invoice_DaysOverdue_ShouldReturnZero_WhenNotYetDue()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 1000m,
            AmountPaid = 0m,
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        // Act & Assert
        invoice.DaysOverdue.Should().Be(0);
    }

    [Fact]
    public void Invoice_DaysOverdue_ShouldReturnDays_WhenOverdue()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 1000m,
            AmountPaid = 0m,
            DueDate = DateTime.UtcNow.AddDays(-5)
        };

        // Act & Assert
        invoice.DaysOverdue.Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public void Invoice_CanSetAllAmounts()
    {
        // Arrange
        var invoice = new Invoice();

        // Act
        invoice.InvoiceNumber = "INV-2025-001";
        invoice.Description = "Monthly service invoice";
        invoice.Status = InvoiceStatus.Sent;
        invoice.InvoiceType = InvoiceType.Recurring;
        invoice.PaymentTerms = PaymentTerms.Net30;
        invoice.Subtotal = 1000m;
        invoice.DiscountAmount = 100m;
        invoice.DiscountPercent = 10m;
        invoice.TaxAmount = 81m;
        invoice.TaxRate = 9m;
        invoice.ShippingAmount = 20m;
        invoice.FeesAmount = 5m;
        invoice.TotalAmount = 1006m;
        invoice.AmountPaid = 500m;
        invoice.AmountCredited = 50m;
        invoice.CurrencyCode = "EUR";
        invoice.ExchangeRate = 1.1m;

        // Assert
        invoice.InvoiceNumber.Should().Be("INV-2025-001");
        invoice.Status.Should().Be(InvoiceStatus.Sent);
        invoice.InvoiceType.Should().Be(InvoiceType.Recurring);
        invoice.Subtotal.Should().Be(1000m);
        invoice.TotalAmount.Should().Be(1006m);
        invoice.BalanceDue.Should().Be(456m);
    }

    [Fact]
    public void Invoice_Collections_ShouldInitializeEmpty()
    {
        // Arrange
        var invoice = new Invoice();

        // Assert
        invoice.CreditMemos.Should().NotBeNull();
        invoice.CreditMemos.Should().BeEmpty();
        invoice.LineItems.Should().NotBeNull();
        invoice.LineItems.Should().BeEmpty();
        invoice.Payments.Should().NotBeNull();
        invoice.Payments.Should().BeEmpty();
    }

    #endregion

    #region InvoiceLineItem Entity Tests

    [Fact]
    public void InvoiceLineItem_ShouldInitializeWithDefaultValues()
    {
        // Act
        var lineItem = new InvoiceLineItem();

        // Assert
        lineItem.LineNumber.Should().Be(0);
        lineItem.ExternalLineId.Should().BeNull();
        lineItem.Name.Should().Be(string.Empty);
        lineItem.Description.Should().BeNull();
        lineItem.SKU.Should().BeNull();
        lineItem.ProductCode.Should().BeNull();
        lineItem.Quantity.Should().Be(1);
        lineItem.UnitOfMeasure.Should().BeNull();
        lineItem.UnitPrice.Should().Be(0);
        lineItem.DiscountAmount.Should().Be(0);
        lineItem.DiscountPercent.Should().Be(0);
        lineItem.ExtendedAmount.Should().Be(0);
        lineItem.TaxAmount.Should().Be(0);
        lineItem.TaxRate.Should().BeNull();
        lineItem.TotalAmount.Should().Be(0);
        lineItem.ServiceStartDate.Should().BeNull();
        lineItem.ServiceEndDate.Should().BeNull();
        lineItem.RevenueRecognitionStartDate.Should().BeNull();
        lineItem.RevenueRecognitionEndDate.Should().BeNull();
        lineItem.DeferredRevenue.Should().BeNull();
        lineItem.RecognizedRevenue.Should().BeNull();
        lineItem.Notes.Should().BeNull();
    }

    [Fact]
    public void InvoiceLineItem_CanSetAllProperties()
    {
        // Arrange
        var lineItem = new InvoiceLineItem();
        var now = DateTime.UtcNow;

        // Act
        lineItem.LineNumber = 1;
        lineItem.ExternalLineId = "EXT-LINE-001";
        lineItem.Name = "Software License";
        lineItem.Description = "Annual subscription";
        lineItem.SKU = "SW-001";
        lineItem.ProductCode = "PROD-001";
        lineItem.Quantity = 5;
        lineItem.UnitOfMeasure = "license";
        lineItem.UnitPrice = 100m;
        lineItem.DiscountAmount = 50m;
        lineItem.DiscountPercent = 10m;
        lineItem.ExtendedAmount = 450m;
        lineItem.TaxAmount = 40.5m;
        lineItem.TaxRate = 9m;
        lineItem.TotalAmount = 490.5m;
        lineItem.ServiceStartDate = now;
        lineItem.ServiceEndDate = now.AddYears(1);
        lineItem.RevenueRecognitionStartDate = now;
        lineItem.RevenueRecognitionEndDate = now.AddYears(1);
        lineItem.DeferredRevenue = 400m;
        lineItem.RecognizedRevenue = 50m;
        lineItem.InvoiceId = 1;
        lineItem.ProductId = 100;

        // Assert
        lineItem.LineNumber.Should().Be(1);
        lineItem.Name.Should().Be("Software License");
        lineItem.Quantity.Should().Be(5);
        lineItem.UnitPrice.Should().Be(100m);
        lineItem.ExtendedAmount.Should().Be(450m);
        lineItem.TotalAmount.Should().Be(490.5m);
        lineItem.DeferredRevenue.Should().Be(400m);
        lineItem.RecognizedRevenue.Should().Be(50m);
    }

    #endregion

    #region Order Entity Tests

    [Fact]
    public void Order_ShouldInitializeWithDefaultValues()
    {
        // Act
        var order = new Order();

        // Assert - Identification
        order.OrderNumber.Should().Be(string.Empty);
        order.ExternalOrderId.Should().BeNull();
        order.CustomerPONumber.Should().BeNull();
        order.ReferenceNumber.Should().BeNull();

        // Assert - Order Details
        order.Name.Should().Be(string.Empty);
        order.Description.Should().BeNull();
        order.Status.Should().Be(OrderStatus.Draft);
    }

    [Fact]
    public void Order_CanSetIdentificationProperties()
    {
        // Arrange
        var order = new Order();

        // Act
        order.OrderNumber = "ORD-2025-001";
        order.ExternalOrderId = "EXT-001";
        order.CustomerPONumber = "PO-12345";
        order.ReferenceNumber = "REF-001";

        // Assert
        order.OrderNumber.Should().Be("ORD-2025-001");
        order.ExternalOrderId.Should().Be("EXT-001");
        order.CustomerPONumber.Should().Be("PO-12345");
        order.ReferenceNumber.Should().Be("REF-001");
    }

    [Fact]
    public void Order_CanSetOrderDetails()
    {
        // Arrange
        var order = new Order();

        // Act
        order.Name = "Enterprise License Order";
        order.Description = "Annual enterprise license renewal";
        order.Status = OrderStatus.Processing;

        // Assert
        order.Name.Should().Be("Enterprise License Order");
        order.Description.Should().Be("Annual enterprise license renewal");
        order.Status.Should().Be(OrderStatus.Processing);
    }

    #endregion

    #region BaseEntity Inheritance Tests

    [Fact]
    public void Quote_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var quote = new Quote();

        // Assert
        quote.Should().BeAssignableTo<BaseEntity>();
        quote.Id.Should().Be(0);
        quote.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void QuoteLineItem_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var lineItem = new QuoteLineItem();

        // Assert
        lineItem.Should().BeAssignableTo<BaseEntity>();
        lineItem.Id.Should().Be(0);
        lineItem.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Invoice_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var invoice = new Invoice();

        // Assert
        invoice.Should().BeAssignableTo<BaseEntity>();
        invoice.Id.Should().Be(0);
        invoice.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void InvoiceLineItem_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var lineItem = new InvoiceLineItem();

        // Assert
        lineItem.Should().BeAssignableTo<BaseEntity>();
        lineItem.Id.Should().Be(0);
        lineItem.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Order_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var order = new Order();

        // Assert
        order.Should().BeAssignableTo<BaseEntity>();
        order.Id.Should().Be(0);
        order.IsDeleted.Should().BeFalse();
    }

    #endregion
}
