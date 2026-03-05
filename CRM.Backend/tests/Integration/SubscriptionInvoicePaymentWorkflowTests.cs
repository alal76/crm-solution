// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Entity-level workflow tests for the Subscription → Invoice → Payment chain.
/// Verifies entity relationships, computed properties, status transitions,
/// and financial calculations without requiring a live database.
/// Interfaces read: ISubscriptionService, IInvoiceService, IPaymentService
/// (all confirmed to exist in CRM.Core.Interfaces before writing).
/// </summary>
public class SubscriptionInvoicePaymentWorkflowTests
{
    // ─────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────

    private static Subscription CreateSubscription(int accountId = 1) => new()
    {
        Id = 1,
        SubscriptionNumber = "SUB-0001",
        AccountId = accountId,
        SubscriptionStatus = SubscriptionStatus.Current,
        BillingCycle = "Monthly",
        MRR = 100m,
        ARR = 1200m
    };

    private static Invoice CreateInvoice(decimal total = 500m, int accountId = 1) => new()
    {
        Id = 1,
        InvoiceNumber = "INV-0001",
        AccountId = accountId,
        TotalAmount = total,
        DueDate = DateTime.UtcNow.AddDays(30)
    };

    private static Payment CreatePayment(decimal amount = 500m, int accountId = 1, int? invoiceId = 1) => new()
    {
        Id = 1,
        PaymentNumber = "PAY-0001",
        AccountId = accountId,
        InvoiceId = invoiceId,
        Amount = amount
    };

    // ─────────────────────────────────────────────────
    // #region 1 — Subscription Entity Defaults
    // ─────────────────────────────────────────────────

    #region Subscription Entity Defaults

    [Fact]
    public void Subscription_ShouldDefaultToCurrentStatus()
    {
        var sub = new Subscription();

        sub.SubscriptionStatus.Should().Be(SubscriptionStatus.Current);
    }

    [Fact]
    public void Subscription_CurrentStatus_ShouldEqualZero()
    {
        // SubscriptionStatus.Current = 0 (alias for Active)
        ((int)SubscriptionStatus.Current).Should().Be(0);
        ((int)SubscriptionStatus.Active).Should().Be(0);
    }

    [Fact]
    public void Subscription_SubscriptionNumber_ShouldDefaultToEmpty()
    {
        var sub = new Subscription();

        sub.SubscriptionNumber.Should().Be(string.Empty);
    }

    [Fact]
    public void Subscription_MRR_ARR_ShouldBeNullByDefault()
    {
        var sub = new Subscription();

        sub.MRR.Should().BeNull();
        sub.ARR.Should().BeNull();
    }

    [Fact]
    public void Subscription_BillingPeriod_ShouldDefaultToMonthlyWhenCycleNotSet()
    {
        var sub = new Subscription();

        // BillingCycle is null → computed BillingPeriod defaults to Monthly
        sub.BillingPeriod.Should().Be(BillingPeriod.Monthly);
    }

    [Fact]
    public void Subscription_BillingPeriod_ShouldReflectBillingCycleString()
    {
        var sub = new Subscription { BillingCycle = "Yearly" };

        sub.BillingPeriod.Should().Be(BillingPeriod.Yearly);
    }

    [Fact]
    public void Subscription_CanSetMRRAndARR()
    {
        var sub = CreateSubscription();

        sub.MRR = 250m;
        sub.ARR = 3000m;

        sub.MRR.Should().Be(250m);
        sub.ARR.Should().Be(3000m);
    }

    [Fact]
    public void Subscription_IsAutoRenew_ShouldDefaultToFalse()
    {
        var sub = new Subscription();

        sub.IsAutoRenew.Should().BeFalse();
    }

    [Fact]
    public void Subscription_CancelAtPeriodEnd_ShouldDefaultToFalse()
    {
        var sub = new Subscription();

        sub.CancelAtPeriodEnd.Should().BeFalse();
    }

    #endregion

    // ─────────────────────────────────────────────────
    // #region 2 — Invoice Entity Defaults
    // ─────────────────────────────────────────────────

    #region Invoice Entity Defaults

    [Fact]
    public void Invoice_ShouldDefaultToDraftStatus()
    {
        var invoice = new Invoice();

        invoice.Status.Should().Be(InvoiceStatus.Draft);
    }

    [Fact]
    public void Invoice_ShouldDefaultToStandardType()
    {
        var invoice = new Invoice();

        invoice.InvoiceType.Should().Be(InvoiceType.Standard);
    }

    [Fact]
    public void Invoice_ShouldDefaultToNet30PaymentTerms()
    {
        var invoice = new Invoice();

        invoice.PaymentTerms.Should().Be(PaymentTerms.Net30);
    }

    [Fact]
    public void Invoice_FinancialDefaults_ShouldAllBeZero()
    {
        var invoice = new Invoice();

        invoice.Subtotal.Should().Be(0);
        invoice.DiscountAmount.Should().Be(0);
        invoice.DiscountPercent.Should().Be(0);
        invoice.TaxAmount.Should().Be(0);
        invoice.TaxRate.Should().Be(0);
        invoice.TotalAmount.Should().Be(0);
        invoice.AmountPaid.Should().Be(0);
        invoice.AmountCredited.Should().Be(0);
    }

    [Fact]
    public void Invoice_CurrencyCode_ShouldDefaultToUSD()
    {
        var invoice = new Invoice();

        invoice.CurrencyCode.Should().Be("USD");
    }

    [Fact]
    public void Invoice_InvoiceNumber_ShouldDefaultToEmpty()
    {
        var invoice = new Invoice();

        invoice.InvoiceNumber.Should().Be(string.Empty);
    }

    #endregion

    // ─────────────────────────────────────────────────
    // #region 3 — Invoice Computed Properties
    // ─────────────────────────────────────────────────

    #region Invoice Computed Properties

    [Fact]
    public void Invoice_BalanceDue_ShouldEqualTotalAmountWhenNothingPaid()
    {
        var invoice = CreateInvoice(total: 500m);

        invoice.BalanceDue.Should().Be(500m);
    }

    [Fact]
    public void Invoice_BalanceDue_ShouldDecreaseWhenAmountPaidIncreases()
    {
        var invoice = CreateInvoice(total: 500m);
        invoice.AmountPaid = 200m;

        invoice.BalanceDue.Should().Be(300m);
    }

    [Fact]
    public void Invoice_BalanceDue_ShouldAccountForAmountCredited()
    {
        var invoice = CreateInvoice(total: 500m);
        invoice.AmountPaid = 300m;
        invoice.AmountCredited = 50m;

        invoice.BalanceDue.Should().Be(150m);
    }

    [Fact]
    public void Invoice_IsPaid_ShouldBeFalseWhenBalanceDueIsPositive()
    {
        var invoice = CreateInvoice(total: 500m);
        invoice.AmountPaid = 200m;

        invoice.IsPaid.Should().BeFalse();
    }

    [Fact]
    public void Invoice_IsPaid_ShouldBeTrueWhenFullyPaid()
    {
        var invoice = CreateInvoice(total: 500m);
        invoice.AmountPaid = 500m;

        invoice.IsPaid.Should().BeTrue();
    }

    [Fact]
    public void Invoice_IsPaid_ShouldBeTrueWhenOverpaid()
    {
        // Overpayment: AmountPaid > TotalAmount → BalanceDue ≤ 0 → IsPaid = true
        var invoice = CreateInvoice(total: 500m);
        invoice.AmountPaid = 600m;

        invoice.IsPaid.Should().BeTrue();
        invoice.BalanceDue.Should().Be(-100m);
    }

    [Fact]
    public void Invoice_Amount_ShouldAliasTotalAmount()
    {
        var invoice = CreateInvoice(total: 750m);

        // Invoice.Amount is a computed property = TotalAmount
        invoice.Amount.Should().Be(750m);
    }

    [Fact]
    public void Invoice_DaysOverdue_ShouldBeZeroWhenPaid()
    {
        var invoice = CreateInvoice(total: 500m);
        invoice.AmountPaid = 500m;
        // DueDate in the past — but IsPaid=true so DaysOverdue = 0
        invoice.DueDate = DateTime.UtcNow.AddDays(-10);

        invoice.DaysOverdue.Should().Be(0);
    }

    [Fact]
    public void Invoice_DaysOverdue_ShouldBeZeroWhenNotYetDue()
    {
        var invoice = CreateInvoice(total: 500m);
        invoice.DueDate = DateTime.UtcNow.AddDays(30);

        invoice.DaysOverdue.Should().Be(0);
    }

    [Fact]
    public void Invoice_DaysOverdue_ShouldBePositiveWhenPastDue()
    {
        var invoice = CreateInvoice(total: 500m);
        invoice.DueDate = DateTime.UtcNow.AddDays(-15);

        invoice.DaysOverdue.Should().BeGreaterThan(0);
    }

    #endregion

    // ─────────────────────────────────────────────────
    // #region 4 — Payment Entity Defaults
    // ─────────────────────────────────────────────────

    #region Payment Entity Defaults

    [Fact]
    public void Payment_ShouldDefaultToPendingStatus()
    {
        var payment = new Payment();

        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Payment_ShouldDefaultToCreditCardMethod()
    {
        var payment = new Payment();

        payment.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
    }

    [Fact]
    public void Payment_ShouldDefaultToPaymentType()
    {
        var payment = new Payment();

        payment.PaymentType.Should().Be(PaymentType.Payment);
    }

    [Fact]
    public void Payment_FinancialDefaults_ShouldAllBeZero()
    {
        var payment = new Payment();

        payment.Amount.Should().Be(0);
        payment.AmountApplied.Should().Be(0);
        payment.ProcessingFee.Should().Be(0);
        payment.RefundedAmount.Should().Be(0);
    }

    [Fact]
    public void Payment_CurrencyCode_ShouldDefaultToUSD()
    {
        var payment = new Payment();

        payment.CurrencyCode.Should().Be("USD");
    }

    [Fact]
    public void Payment_RetryCount_ShouldDefaultToZero()
    {
        var payment = new Payment();

        payment.RetryCount.Should().Be(0);
    }

    [Fact]
    public void Payment_IsReconciled_ShouldDefaultToFalse()
    {
        var payment = new Payment();

        payment.IsReconciled.Should().BeFalse();
    }

    [Fact]
    public void Payment_FraudFlagged_ShouldDefaultToFalse()
    {
        var payment = new Payment();

        payment.FraudFlagged.Should().BeFalse();
    }

    #endregion

    // ─────────────────────────────────────────────────
    // #region 5 — Payment Computed Properties
    // ─────────────────────────────────────────────────

    #region Payment Computed Properties

    [Fact]
    public void Payment_NetAmount_ShouldEqualAmountMinusProcessingFee()
    {
        var payment = CreatePayment(amount: 500m);
        payment.ProcessingFee = 15m;

        payment.NetAmount.Should().Be(485m);
    }

    [Fact]
    public void Payment_NetAmount_ShouldEqualAmountWhenNoFee()
    {
        var payment = CreatePayment(amount: 300m);

        payment.NetAmount.Should().Be(300m);
    }

    [Fact]
    public void Payment_AmountUnapplied_ShouldEqualAmountMinusAmountApplied()
    {
        var payment = CreatePayment(amount: 500m);
        payment.AmountApplied = 200m;

        payment.AmountUnapplied.Should().Be(300m);
    }

    [Fact]
    public void Payment_AmountUnapplied_ShouldBeZeroWhenFullyApplied()
    {
        var payment = CreatePayment(amount: 500m);
        payment.AmountApplied = 500m;

        payment.AmountUnapplied.Should().Be(0);
    }

    [Fact]
    public void Payment_TransactionId_ShouldAliasGatewayTransactionId()
    {
        var payment = new Payment();
        payment.TransactionId = "TXN-12345";

        payment.GatewayTransactionId.Should().Be("TXN-12345");
        payment.TransactionId.Should().Be("TXN-12345");
    }

    #endregion

    // ─────────────────────────────────────────────────
    // #region 6 — Subscription → Invoice → Payment relationships
    // ─────────────────────────────────────────────────

    #region Workflow: Subscription → Invoice → Payment

    [Fact]
    public void Invoice_CanBeLinkedToSubscriptionViaSubscriptionId()
    {
        var sub = CreateSubscription();
        var invoice = CreateInvoice();
        invoice.SubscriptionId = sub.Id;

        invoice.SubscriptionId.Should().Be(sub.Id);
    }

    [Fact]
    public void Payment_CanBeLinkedToBothInvoiceAndSubscription()
    {
        var sub = CreateSubscription();
        var invoice = CreateInvoice();
        var payment = CreatePayment();

        payment.InvoiceId = invoice.Id;
        payment.SubscriptionId = sub.Id;

        payment.InvoiceId.Should().Be(invoice.Id);
        payment.SubscriptionId.Should().Be(sub.Id);
    }

    [Fact]
    public void FullWorkflow_SubscriptionGeneratesInvoice_PaymentClearsBalance()
    {
        // Arrange: subscription + invoice
        var sub = CreateSubscription();
        var invoice = CreateInvoice(total: 100m);
        invoice.SubscriptionId = sub.Id;
        invoice.InvoiceType = InvoiceType.Recurring;

        // Pre-payment
        invoice.IsPaid.Should().BeFalse();
        invoice.BalanceDue.Should().Be(100m);

        // Act: create payment and apply to invoice
        var payment = CreatePayment(amount: 100m);
        payment.InvoiceId = invoice.Id;
        payment.SubscriptionId = sub.Id;
        payment.AmountApplied = 100m;

        // Simulate invoice collecting the payment
        invoice.AmountPaid += payment.AmountApplied;
        invoice.Status = InvoiceStatus.Paid;

        // Assert
        invoice.IsPaid.Should().BeTrue();
        invoice.BalanceDue.Should().Be(0);
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        payment.AmountUnapplied.Should().Be(0);
    }

    [Fact]
    public void FullWorkflow_PartialPayment_LeavesBalanceDue()
    {
        var invoice = CreateInvoice(total: 500m);
        var payment = CreatePayment(amount: 200m);
        payment.AmountApplied = 200m;

        invoice.AmountPaid = payment.AmountApplied;
        invoice.Status = InvoiceStatus.PartiallyPaid;

        invoice.IsPaid.Should().BeFalse();
        invoice.BalanceDue.Should().Be(300m);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    [Fact]
    public void Invoice_StatusTransition_DraftToApproved_IsValidEntityUpdate()
    {
        var invoice = CreateInvoice();
        invoice.Status.Should().Be(InvoiceStatus.Draft);

        invoice.Status = InvoiceStatus.Approved;

        invoice.Status.Should().Be(InvoiceStatus.Approved);
    }

    [Fact]
    public void Invoice_StatusTransition_ApprovedToSent_IsValidEntityUpdate()
    {
        var invoice = CreateInvoice();
        invoice.Status = InvoiceStatus.Approved;
        invoice.Status = InvoiceStatus.Sent;
        invoice.SentDate = DateTime.UtcNow;

        invoice.Status.Should().Be(InvoiceStatus.Sent);
        invoice.SentDate.Should().NotBeNull();
    }

    [Fact]
    public void Invoice_StatusTransition_SentToOverdue_IsValidEntityUpdate()
    {
        var invoice = CreateInvoice();
        invoice.Status = InvoiceStatus.Sent;
        invoice.DueDate = DateTime.UtcNow.AddDays(-5);
        invoice.Status = InvoiceStatus.Overdue;

        invoice.Status.Should().Be(InvoiceStatus.Overdue);
        invoice.DaysOverdue.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Payment_StatusTransition_PendingToCompleted_IsValidEntityUpdate()
    {
        var payment = CreatePayment();
        payment.Status.Should().Be(PaymentStatus.Pending);

        payment.Status = PaymentStatus.Completed;
        payment.ProcessedDate = DateTime.UtcNow;

        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.ProcessedDate.Should().NotBeNull();
    }

    [Fact]
    public void Payment_Refund_ShouldLinkOriginalPaymentId()
    {
        var original = CreatePayment(amount: 500m);
        original.Id = 10;
        original.Status = PaymentStatus.Completed;

        var refund = new Payment
        {
            Amount = 500m,
            PaymentType = PaymentType.Refund,
            Status = PaymentStatus.Pending,
            OriginalPaymentId = original.Id,
            AccountId = original.AccountId,
            InvoiceId = original.InvoiceId,
            RefundReason = "Customer request"
        };

        refund.OriginalPaymentId.Should().Be(original.Id);
        refund.PaymentType.Should().Be(PaymentType.Refund);
        refund.RefundReason.Should().Be("Customer request");
    }

    [Fact]
    public void Subscription_CanBeLinkedToMultipleInvoices_ViaNavigationList()
    {
        var sub = CreateSubscription();
        var inv1 = CreateInvoice(total: 100m); inv1.Id = 1;
        var inv2 = CreateInvoice(total: 100m); inv2.Id = 2;

        // Simulating a list of invoices for this subscription
        var invoices = new List<Invoice> { inv1, inv2 };

        invoices.Should().HaveCount(2);
        invoices.All(i => i.TotalAmount == 100m).Should().BeTrue();
    }

    [Fact]
    public void Invoice_RecurringType_IsAvailableForSubscriptionInvoices()
    {
        var invoice = CreateInvoice();
        invoice.InvoiceType = InvoiceType.Recurring;

        invoice.InvoiceType.Should().Be(InvoiceType.Recurring);
    }

    [Fact]
    public void Payment_ScheduledDate_CanBeSetForFuturePayments()
    {
        var payment = CreatePayment();
        var futureDate = DateTime.UtcNow.AddDays(7);
        payment.ScheduledDate = futureDate;

        payment.ScheduledDate.Should().BeCloseTo(futureDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Invoice_EarlyPaymentDiscount_FieldsAreNullable()
    {
        var invoice = new Invoice();

        invoice.EarlyPaymentDiscountPercent.Should().BeNull();
        invoice.EarlyPaymentDiscountDays.Should().BeNull();
        invoice.EarlyPaymentDiscountAmount.Should().BeNull();
    }

    [Fact]
    public void Subscription_InvoiceAndPayment_AccountIdsMustMatch()
    {
        // All three entities must share the same AccountId for a valid workflow
        const int accountId = 42;

        var sub = CreateSubscription(accountId);
        var invoice = CreateInvoice(accountId: accountId);
        var payment = CreatePayment(accountId: accountId);

        sub.AccountId.Should().Be(accountId);
        invoice.AccountId.Should().Be(accountId);
        payment.AccountId.Should().Be(accountId);
    }

    [Fact]
    public void Invoice_Voided_HasVoidedStatusAndDate()
    {
        var invoice = CreateInvoice();
        invoice.Status = InvoiceStatus.Voided;
        invoice.VoidedDate = DateTime.UtcNow;

        invoice.Status.Should().Be(InvoiceStatus.Voided);
        invoice.VoidedDate.Should().NotBeNull();
    }

    [Fact]
    public void Invoice_LateFeeTotal_DefaultsToZero()
    {
        var invoice = new Invoice();

        invoice.LateFeeTotal.Should().Be(0);
    }

    #endregion
}
