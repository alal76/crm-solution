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
/// Tests for Payment, Subscription, Contract entities and their related enums
/// </summary>
public class PaymentSubscriptionContractEntityTests
{
    #region PaymentStatus Enum Tests

    [Fact]
    public void PaymentStatus_ShouldHaveExpectedValues()
    {
        PaymentStatus.Pending.Should().Be((PaymentStatus)0);
        PaymentStatus.Processing.Should().Be((PaymentStatus)1);
        PaymentStatus.Completed.Should().Be((PaymentStatus)2);
        PaymentStatus.Failed.Should().Be((PaymentStatus)3);
        PaymentStatus.Declined.Should().Be((PaymentStatus)4);
        PaymentStatus.Cancelled.Should().Be((PaymentStatus)5);
        PaymentStatus.Refunded.Should().Be((PaymentStatus)6);
        PaymentStatus.PartiallyRefunded.Should().Be((PaymentStatus)7);
        PaymentStatus.Disputed.Should().Be((PaymentStatus)8);
        PaymentStatus.Voided.Should().Be((PaymentStatus)9);
        PaymentStatus.OnHold.Should().Be((PaymentStatus)10);
        PaymentStatus.Expired.Should().Be((PaymentStatus)11);
    }

    [Fact]
    public void PaymentStatus_ShouldHave12Values()
    {
        var values = Enum.GetValues<PaymentStatus>();
        values.Should().HaveCount(12);
    }

    [Theory]
    [InlineData(PaymentStatus.Pending, "Pending")]
    [InlineData(PaymentStatus.Completed, "Completed")]
    [InlineData(PaymentStatus.Failed, "Failed")]
    [InlineData(PaymentStatus.Refunded, "Refunded")]
    [InlineData(PaymentStatus.Disputed, "Disputed")]
    public void PaymentStatus_ShouldHaveCorrectName(PaymentStatus status, string expectedName)
    {
        status.ToString().Should().Be(expectedName);
    }

    #endregion

    #region PaymentMethod Enum Tests

    [Fact]
    public void PaymentMethod_ShouldHaveExpectedValues()
    {
        PaymentMethod.CreditCard.Should().Be((PaymentMethod)0);
        PaymentMethod.DebitCard.Should().Be((PaymentMethod)1);
        PaymentMethod.BankTransfer.Should().Be((PaymentMethod)2);
        PaymentMethod.WireTransfer.Should().Be((PaymentMethod)3);
        PaymentMethod.Check.Should().Be((PaymentMethod)4);
        PaymentMethod.Cash.Should().Be((PaymentMethod)5);
        PaymentMethod.PayPal.Should().Be((PaymentMethod)6);
        PaymentMethod.Stripe.Should().Be((PaymentMethod)7);
        PaymentMethod.ApplePay.Should().Be((PaymentMethod)8);
        PaymentMethod.GooglePay.Should().Be((PaymentMethod)9);
        PaymentMethod.Venmo.Should().Be((PaymentMethod)10);
        PaymentMethod.Crypto.Should().Be((PaymentMethod)11);
        PaymentMethod.StoreCredit.Should().Be((PaymentMethod)12);
        PaymentMethod.GiftCard.Should().Be((PaymentMethod)13);
        PaymentMethod.Financing.Should().Be((PaymentMethod)14);
        PaymentMethod.PurchaseOrder.Should().Be((PaymentMethod)15);
        PaymentMethod.Other.Should().Be((PaymentMethod)16);
    }

    [Fact]
    public void PaymentMethod_ShouldHave17Values()
    {
        var values = Enum.GetValues<PaymentMethod>();
        values.Should().HaveCount(17);
    }

    [Theory]
    [InlineData(PaymentMethod.CreditCard, "CreditCard")]
    [InlineData(PaymentMethod.PayPal, "PayPal")]
    [InlineData(PaymentMethod.Stripe, "Stripe")]
    [InlineData(PaymentMethod.Crypto, "Crypto")]
    public void PaymentMethod_ShouldHaveCorrectName(PaymentMethod method, string expectedName)
    {
        method.ToString().Should().Be(expectedName);
    }

    #endregion

    #region PaymentType Enum Tests

    [Fact]
    public void PaymentType_ShouldHaveExpectedValues()
    {
        PaymentType.Payment.Should().Be((PaymentType)0);
        PaymentType.Refund.Should().Be((PaymentType)1);
        PaymentType.Authorization.Should().Be((PaymentType)2);
        PaymentType.Capture.Should().Be((PaymentType)3);
        PaymentType.Void.Should().Be((PaymentType)4);
        PaymentType.Deposit.Should().Be((PaymentType)5);
        PaymentType.WriteOff.Should().Be((PaymentType)6);
        PaymentType.CreditApplication.Should().Be((PaymentType)7);
        PaymentType.Chargeback.Should().Be((PaymentType)8);
        PaymentType.ChargebackReversal.Should().Be((PaymentType)9);
    }

    [Fact]
    public void PaymentType_ShouldHave10Values()
    {
        var values = Enum.GetValues<PaymentType>();
        values.Should().HaveCount(10);
    }

    #endregion

    #region SubscriptionStatus Enum Tests

    [Fact]
    public void SubscriptionStatus_ShouldHaveExpectedValues()
    {
        SubscriptionStatus.Current.Should().Be((SubscriptionStatus)0);
        SubscriptionStatus.Churned.Should().Be((SubscriptionStatus)2);
    }

    [Fact]
    public void SubscriptionStatus_ShouldHave7DistinctValues()
    {
        var values = Enum.GetValues<SubscriptionStatus>();
        // 9 named values including aliases Current (=Active) and Churned (=Cancelled)
        values.Should().HaveCount(9);
        values.Distinct().Should().HaveCount(7);
    }

    #endregion

    #region ContractStatus Enum Tests

    [Fact]
    public void ContractStatus_ShouldHaveExpectedValues()
    {
        ContractStatus.Draft.Should().Be((ContractStatus)0);
        ContractStatus.PendingApproval.Should().Be((ContractStatus)1);
        ContractStatus.Approved.Should().Be((ContractStatus)2);
        ContractStatus.Active.Should().Be((ContractStatus)3);
        ContractStatus.Expired.Should().Be((ContractStatus)4);
        ContractStatus.Terminated.Should().Be((ContractStatus)5);
        ContractStatus.Renewed.Should().Be((ContractStatus)6);
        ContractStatus.OnHold.Should().Be((ContractStatus)7);
    }

    [Fact]
    public void ContractStatus_ShouldHave8Values()
    {
        var values = Enum.GetValues<ContractStatus>();
        values.Should().HaveCount(8);
    }

    [Theory]
    [InlineData(ContractStatus.Draft, "Draft")]
    [InlineData(ContractStatus.Active, "Active")]
    [InlineData(ContractStatus.Expired, "Expired")]
    [InlineData(ContractStatus.Terminated, "Terminated")]
    public void ContractStatus_ShouldHaveCorrectName(ContractStatus status, string expectedName)
    {
        status.ToString().Should().Be(expectedName);
    }

    #endregion

    #region ContractType Enum Tests

    [Fact]
    public void ContractType_ShouldHaveExpectedValues()
    {
        ContractType.Service.Should().Be((ContractType)0);
        ContractType.License.Should().Be((ContractType)1);
        ContractType.Subscription.Should().Be((ContractType)2);
        ContractType.Support.Should().Be((ContractType)3);
        ContractType.Maintenance.Should().Be((ContractType)4);
        ContractType.NDA.Should().Be((ContractType)5);
        ContractType.Master.Should().Be((ContractType)6);
        ContractType.Amendment.Should().Be((ContractType)7);
        ContractType.Other.Should().Be((ContractType)8);
    }

    [Fact]
    public void ContractType_ShouldHave9Values()
    {
        var values = Enum.GetValues<ContractType>();
        values.Should().HaveCount(9);
    }

    #endregion

    #region Payment Entity Tests

    [Fact]
    public void Payment_ShouldInitializeWithDefaultValues()
    {
        // Act
        var payment = new Payment();

        // Assert - Identification
        payment.PaymentNumber.Should().Be(string.Empty);
        payment.ExternalPaymentId.Should().BeNull();
        payment.GatewayTransactionId.Should().BeNull();
        payment.GatewayReference.Should().BeNull();
        payment.AuthorizationCode.Should().BeNull();
        payment.CheckNumber.Should().BeNull();

        // Assert - Payment Details
        payment.Description.Should().BeNull();
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
        payment.PaymentType.Should().Be(PaymentType.Payment);

        // Assert - Amounts
        payment.Amount.Should().Be(0);
        payment.AmountApplied.Should().Be(0);
        payment.ProcessingFee.Should().Be(0);
        payment.RefundedAmount.Should().Be(0);
        payment.CurrencyCode.Should().Be("USD");
        payment.ExchangeRate.Should().BeNull();

        // Assert - Fraud & Risk
        payment.FraudFlagged.Should().BeFalse();
        payment.FraudNotes.Should().BeNull();
        payment.IpAddress.Should().BeNull();
        payment.DeviceFingerprint.Should().BeNull();

        // Assert - Card Details
        payment.CardBrand.Should().BeNull();
        payment.CardLast4.Should().BeNull();
        payment.CardExpMonth.Should().BeNull();
        payment.CardExpYear.Should().BeNull();
        payment.CardholderName.Should().BeNull();

        // Assert - Bank Details
        payment.BankName.Should().BeNull();
        payment.AccountLast4.Should().BeNull();
        payment.AccountType.Should().BeNull();
        payment.RoutingNumberLast4.Should().BeNull();

        // Assert - Notes
        payment.Notes.Should().BeNull();
        payment.InternalNotes.Should().BeNull();
        payment.FailureReason.Should().BeNull();
        payment.RefundReason.Should().BeNull();
    }

    [Fact]
    public void Payment_AmountUnapplied_ShouldCalculateCorrectly()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = 1000m,
            AmountApplied = 800m
        };

        // Act & Assert
        payment.AmountUnapplied.Should().Be(200m);
    }

    [Fact]
    public void Payment_AmountUnapplied_ShouldBeZero_WhenFullyApplied()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = 1000m,
            AmountApplied = 1000m
        };

        // Act & Assert
        payment.AmountUnapplied.Should().Be(0m);
    }

    [Fact]
    public void Payment_NetAmount_ShouldCalculateCorrectly()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = 1000m,
            ProcessingFee = 29m
        };

        // Act & Assert
        payment.NetAmount.Should().Be(971m);
    }

    [Fact]
    public void Payment_NetAmount_ShouldEqualAmount_WhenNoFee()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = 500m,
            ProcessingFee = 0
        };

        // Act & Assert
        payment.NetAmount.Should().Be(500m);
    }

    [Fact]
    public void Payment_CanSetAllProperties()
    {
        // Arrange
        var payment = new Payment();
        var now = DateTime.UtcNow;

        // Act
        payment.PaymentNumber = "PAY-2025-001";
        payment.ExternalPaymentId = "ext-pay-001";
        payment.GatewayTransactionId = "ch_1234567890";
        payment.GatewayReference = "REF123";
        payment.AuthorizationCode = "AUTH123";
        payment.CheckNumber = "1234";
        payment.Description = "Invoice payment";
        payment.Status = PaymentStatus.Completed;
        payment.PaymentMethod = PaymentMethod.Stripe;
        payment.PaymentType = PaymentType.Payment;
        payment.Amount = 1000m;
        payment.AmountApplied = 1000m;
        payment.ProcessingFee = 29m;
        payment.CurrencyCode = "EUR";
        payment.ExchangeRate = 1.1m;
        payment.PaymentDate = now;
        payment.ProcessedDate = now.AddMinutes(1);
        payment.SettledDate = now.AddDays(1);
        payment.CardBrand = "Visa";
        payment.CardLast4 = "4242";
        payment.CardExpMonth = 12;
        payment.CardExpYear = 2027;
        payment.CardholderName = "John Doe";
        payment.Gateway = "Stripe";
        payment.GatewayResponseCode = "approved";
        payment.GatewayResponseMessage = "Payment successful";
        payment.FraudFlagged = false;
        payment.IpAddress = "192.168.1.1";
        payment.AccountId = 1;
        payment.InvoiceId = 10;

        // Assert
        payment.PaymentNumber.Should().Be("PAY-2025-001");
        payment.GatewayTransactionId.Should().Be("ch_1234567890");
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.PaymentMethod.Should().Be(PaymentMethod.Stripe);
        payment.Amount.Should().Be(1000m);
        payment.NetAmount.Should().Be(971m);
        payment.CardBrand.Should().Be("Visa");
        payment.CardLast4.Should().Be("4242");
        payment.AccountId.Should().Be(1);
    }

    [Fact]
    public void Payment_Refunds_ShouldInitializeEmpty()
    {
        // Arrange
        var payment = new Payment();

        // Assert
        payment.Refunds.Should().NotBeNull();
        payment.Refunds.Should().BeEmpty();
    }

    [Fact]
    public void Payment_CanSetDates()
    {
        // Arrange
        var payment = new Payment();
        var now = DateTime.UtcNow;

        // Act
        payment.PaymentDate = now;
        payment.ProcessedDate = now.AddMinutes(5);
        payment.SettledDate = now.AddDays(2);
        payment.RefundDate = now.AddDays(5);
        payment.DepositDate = now.AddDays(3);

        // Assert
        payment.PaymentDate.Should().Be(now);
        payment.ProcessedDate.Should().Be(now.AddMinutes(5));
        payment.SettledDate.Should().Be(now.AddDays(2));
        payment.RefundDate.Should().Be(now.AddDays(5));
        payment.DepositDate.Should().Be(now.AddDays(3));
    }

    [Fact]
    public void Payment_CanSetBankDetails()
    {
        // Arrange
        var payment = new Payment();

        // Act
        payment.BankName = "Chase Bank";
        payment.AccountLast4 = "6789";
        payment.AccountType = "Checking";
        payment.RoutingNumberLast4 = "1234";
        payment.PaymentMethod = PaymentMethod.BankTransfer;

        // Assert
        payment.BankName.Should().Be("Chase Bank");
        payment.AccountLast4.Should().Be("6789");
        payment.AccountType.Should().Be("Checking");
        payment.RoutingNumberLast4.Should().Be("1234");
        payment.PaymentMethod.Should().Be(PaymentMethod.BankTransfer);
    }

    [Fact]
    public void Payment_CanSetGatewayResponse()
    {
        // Arrange
        var payment = new Payment();

        // Act
        payment.Gateway = "Stripe";
        payment.GatewayResponseCode = "succeeded";
        payment.GatewayResponseMessage = "Payment successful";
        payment.AvsResponseCode = "Y";
        payment.CvvResponseCode = "M";
        payment.RiskScore = 0.1m;
        payment.GatewayResponseRaw = "{\"id\":\"ch_123\",\"status\":\"succeeded\"}";

        // Assert
        payment.Gateway.Should().Be("Stripe");
        payment.GatewayResponseCode.Should().Be("succeeded");
        payment.AvsResponseCode.Should().Be("Y");
        payment.CvvResponseCode.Should().Be("M");
        payment.RiskScore.Should().Be(0.1m);
        payment.GatewayResponseRaw.Should().Contain("succeeded");
    }

    #endregion

    #region Subscription Entity Tests

    [Fact]
    public void Subscription_ShouldInitializeWithDefaultValues()
    {
        // Act
        var subscription = new Subscription();

        // Assert
        subscription.SubscriptionNumber.Should().Be(string.Empty);
        subscription.SubscriptionStatus.Should().Be(SubscriptionStatus.Current);
        subscription.IsAutoRenew.Should().BeFalse();
        subscription.IsActive.Should().BeTrue();

        // Financial
        subscription.MRR.Should().BeNull();
        subscription.ARR.Should().BeNull();
        subscription.OneTimeFee.Should().BeNull();
        subscription.Currency.Should().BeNull();
        subscription.BillingCycle.Should().BeNull();
        subscription.BillingStartDate.Should().BeNull();
        subscription.BillingEndDate.Should().BeNull();

        // Contract
        subscription.ContractReference.Should().BeNull();
        subscription.ContractStartDate.Should().BeNull();
        subscription.ContractEndDate.Should().BeNull();
        subscription.SLA.Should().BeNull();
        subscription.ContractNotes.Should().BeNull();

        // Billing Address
        subscription.BillingAddress.Should().BeNull();
        subscription.BillingCity.Should().BeNull();
        subscription.BillingState.Should().BeNull();
        subscription.BillingZip.Should().BeNull();
        subscription.BillingCountry.Should().BeNull();
        subscription.BillingContactName.Should().BeNull();
        subscription.BillingContactEmail.Should().BeNull();
        subscription.BillingContactPhone.Should().BeNull();

        // Contract Document
        subscription.ContractFileName.Should().BeNull();
        subscription.ContractFilePath.Should().BeNull();
        subscription.ContractContentType.Should().BeNull();
        subscription.ContractFileSize.Should().BeNull();
    }

    [Fact]
    public void Subscription_CanSetAllProperties()
    {
        // Arrange
        var subscription = new Subscription();
        var now = DateTime.UtcNow;

        // Act
        subscription.SubscriptionNumber = "SUB-2025-001";
        subscription.AccountId = 1;
        subscription.ProductId = 10;
        subscription.SubscriptionStatus = SubscriptionStatus.Current;
        subscription.MRR = 99.99m;
        subscription.ARR = 1199.88m;
        subscription.OneTimeFee = 50m;
        subscription.Currency = "USD";
        subscription.BillingCycle = "Monthly";
        subscription.BillingStartDate = now;
        subscription.BillingEndDate = now.AddYears(1);
        subscription.ContractReference = "CON-REF-001";
        subscription.ContractStartDate = now;
        subscription.ContractEndDate = now.AddYears(1);
        subscription.SLA = "Premium Support";
        subscription.ContractNotes = "Special terms apply";
        subscription.BillingAddress = "123 Main St";
        subscription.BillingCity = "Seattle";
        subscription.BillingState = "WA";
        subscription.BillingZip = "98101";
        subscription.BillingCountry = "USA";
        subscription.BillingContactName = "John Doe";
        subscription.BillingContactEmail = "billing@example.com";
        subscription.BillingContactPhone = "+1-555-123-4567";
        subscription.IsAutoRenew = true;
        subscription.RenewalDate = now.AddYears(1);
        subscription.IsActive = true;
        subscription.SubscriptionOwner = "Sales Team";
        subscription.SubscriptionManagerId = 5;
        subscription.Tags = "enterprise,premium";
        subscription.ExternalReference = "EXT-REF-001";

        // Assert
        subscription.SubscriptionNumber.Should().Be("SUB-2025-001");
        subscription.AccountId.Should().Be(1);
        subscription.ProductId.Should().Be(10);
        subscription.MRR.Should().Be(99.99m);
        subscription.ARR.Should().Be(1199.88m);
        subscription.BillingCycle.Should().Be("Monthly");
        subscription.SLA.Should().Be("Premium Support");
        subscription.BillingCity.Should().Be("Seattle");
        subscription.IsAutoRenew.Should().BeTrue();
        subscription.Tags.Should().Be("enterprise,premium");
    }

    [Fact]
    public void Subscription_CanSetContractDocument()
    {
        // Arrange
        var subscription = new Subscription();

        // Act
        subscription.ContractFileName = "contract_2025.pdf";
        subscription.ContractFilePath = "/uploads/contracts/contract_2025.pdf";
        subscription.ContractContentType = "application/pdf";
        subscription.ContractFileSize = 1048576;

        // Assert
        subscription.ContractFileName.Should().Be("contract_2025.pdf");
        subscription.ContractFilePath.Should().Contain("contract_2025.pdf");
        subscription.ContractContentType.Should().Be("application/pdf");
        subscription.ContractFileSize.Should().Be(1048576);
    }

    [Fact]
    public void Subscription_StatusChurned_ShouldSetCorrectly()
    {
        // Arrange
        var subscription = new Subscription();

        // Act
        subscription.SubscriptionStatus = SubscriptionStatus.Churned;

        // Assert
        subscription.SubscriptionStatus.Should().Be(SubscriptionStatus.Churned);
    }

    #endregion

    #region SubscriptionItem Entity Tests

    [Fact]
    public void SubscriptionItem_ShouldInitializeWithDefaultValues()
    {
        // Act
        var item = new SubscriptionItem();

        // Assert
        item.SubscriptionId.Should().Be(0);
        item.ProductId.Should().BeNull();
        item.ItemName.Should().BeNull();
        item.Description.Should().BeNull();
        item.Quantity.Should().Be(1);
        item.UnitPrice.Should().Be(0);
        item.Amount.Should().Be(0);
        item.StartDate.Should().BeNull();
        item.EndDate.Should().BeNull();
    }

    [Fact]
    public void SubscriptionItem_CanSetAllProperties()
    {
        // Arrange
        var item = new SubscriptionItem();
        var now = DateTime.UtcNow;

        // Act
        item.SubscriptionId = 1;
        item.ProductId = 10;
        item.ItemName = "Premium Feature";
        item.Description = "Additional premium feature access";
        item.Quantity = 5;
        item.UnitPrice = 9.99m;
        item.Amount = 49.95m;
        item.StartDate = now;
        item.EndDate = now.AddMonths(12);

        // Assert
        item.SubscriptionId.Should().Be(1);
        item.ProductId.Should().Be(10);
        item.ItemName.Should().Be("Premium Feature");
        item.Quantity.Should().Be(5);
        item.UnitPrice.Should().Be(9.99m);
        item.Amount.Should().Be(49.95m);
    }

    #endregion

    #region Contract Entity Tests

    [Fact]
    public void Contract_ShouldInitializeWithDefaultValues()
    {
        // Act
        var contract = new Contract();

        // Assert
        contract.ContractNumber.Should().StartWith("CON-");
        contract.Name.Should().Be(string.Empty);
        contract.Description.Should().BeNull();
        contract.Status.Should().Be(ContractStatus.Draft);
        contract.ContractType.Should().Be(ContractType.Service);
        contract.AutoRenew.Should().BeFalse();
        contract.RenewalNoticeDays.Should().Be(30);
        contract.RenewalNoticeSent.Should().BeFalse();
        contract.RenewalNoticeSentDate.Should().BeNull();
        contract.Terms.Should().BeNull();
        contract.SpecialConditions.Should().BeNull();
        contract.TerminationClause.Should().BeNull();
        contract.ContractFileUrl.Should().BeNull();
        contract.ContractFileName.Should().BeNull();
        contract.ContractFileSize.Should().BeNull();
        contract.ContractFileMimeType.Should().BeNull();
        contract.SignedContractFileUrl.Should().BeNull();
        contract.SignedContractFileName.Should().BeNull();
        contract.ApprovedByUserId.Should().BeNull();
        contract.ApprovedDate.Should().BeNull();
        contract.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Contract_CanSetAllProperties()
    {
        // Arrange
        var contract = new Contract();
        var now = DateTime.UtcNow;

        // Act
        contract.ContractNumber = "CON-2025-001";
        contract.Name = "Enterprise License Agreement";
        contract.Description = "Annual enterprise software license";
        contract.Status = ContractStatus.Active;
        contract.ContractType = ContractType.License;
        contract.AccountId = 1;
        contract.ContactId = 5;
        contract.OwnerId = 10;
        contract.ParentContractId = null;
        contract.OpportunityId = 20;
        contract.QuoteId = 30;
        contract.StartDate = now;
        contract.EndDate = now.AddYears(1);
        contract.SignedDate = now.AddDays(-5);
        contract.ActivatedDate = now;
        contract.Value = 50000m;
        contract.CurrencyCode = "EUR";
        contract.BillingFrequency = "Annual";
        contract.AutoRenew = true;
        contract.RenewalNoticeDays = 60;
        contract.Terms = "Standard terms and conditions apply";
        contract.SpecialConditions = "Price locked for 2 years";
        contract.TerminationClause = "30 days notice required";
        contract.ContractFileUrl = "https://storage.example.com/contracts/con-2025-001.pdf";
        contract.ContractFileName = "con-2025-001.pdf";
        contract.ContractFileSize = 2097152;
        contract.ContractFileMimeType = "application/pdf";

        // Assert
        contract.ContractNumber.Should().Be("CON-2025-001");
        contract.Name.Should().Be("Enterprise License Agreement");
        contract.Status.Should().Be(ContractStatus.Active);
        contract.ContractType.Should().Be(ContractType.License);
        contract.AccountId.Should().Be(1);
        contract.Value.Should().Be(50000m);
        contract.CurrencyCode.Should().Be("EUR");
        contract.AutoRenew.Should().BeTrue();
        contract.RenewalNoticeDays.Should().Be(60);
        contract.ContractFileSize.Should().Be(2097152);
    }

    [Fact]
    public void Contract_DaysUntilExpiration_ShouldCalculateCorrectly_WhenNotExpired()
    {
        // Arrange
        var contract = new Contract
        {
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        // Act & Assert
        contract.DaysUntilExpiration.Should().NotBeNull();
        contract.DaysUntilExpiration!.Value.Should().BeInRange(29, 31);
    }

    [Fact]
    public void Contract_DaysUntilExpiration_ShouldReturnNull_WhenExpired()
    {
        // Arrange
        var contract = new Contract
        {
            EndDate = DateTime.UtcNow.AddDays(-5)
        };

        // Act & Assert
        contract.DaysUntilExpiration.Should().BeNull();
    }

    [Fact]
    public void Contract_IsExpiringSoon_ShouldReturnTrue_WhenActiveAndWithinNoticeDays()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Active,
            EndDate = DateTime.UtcNow.AddDays(20),
            RenewalNoticeDays = 30
        };

        // Act & Assert
        contract.IsExpiringSoon.Should().BeTrue();
    }

    [Fact]
    public void Contract_IsExpiringSoon_ShouldReturnFalse_WhenNotActive()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Draft,
            EndDate = DateTime.UtcNow.AddDays(20),
            RenewalNoticeDays = 30
        };

        // Act & Assert
        contract.IsExpiringSoon.Should().BeFalse();
    }

    [Fact]
    public void Contract_IsExpiringSoon_ShouldReturnFalse_WhenFarFromExpiration()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Active,
            EndDate = DateTime.UtcNow.AddDays(90),
            RenewalNoticeDays = 30
        };

        // Act & Assert
        contract.IsExpiringSoon.Should().BeFalse();
    }

    [Fact]
    public void Contract_ChildContracts_ShouldInitializeEmpty()
    {
        // Arrange
        var contract = new Contract();

        // Assert
        contract.ChildContracts.Should().NotBeNull();
        contract.ChildContracts.Should().BeEmpty();
    }

    [Fact]
    public void Contract_CanSetApprovalInfo()
    {
        // Arrange
        var contract = new Contract();
        var now = DateTime.UtcNow;

        // Act
        contract.Status = ContractStatus.Approved;
        contract.ApprovedByUserId = 5;
        contract.ApprovedDate = now;

        // Assert
        contract.Status.Should().Be(ContractStatus.Approved);
        contract.ApprovedByUserId.Should().Be(5);
        contract.ApprovedDate.Should().Be(now);
    }

    [Fact]
    public void Contract_CanSetRejection()
    {
        // Arrange
        var contract = new Contract();

        // Act
        contract.Status = ContractStatus.Draft;
        contract.RejectionReason = "Terms not acceptable";

        // Assert
        contract.RejectionReason.Should().Be("Terms not acceptable");
    }

    [Fact]
    public void Contract_CanSetSignedDocument()
    {
        // Arrange
        var contract = new Contract();
        var now = DateTime.UtcNow;

        // Act
        contract.SignedDate = now;
        contract.SignedContractFileUrl = "https://storage.example.com/signed/contract.pdf";
        contract.SignedContractFileName = "signed_contract.pdf";

        // Assert
        contract.SignedDate.Should().Be(now);
        contract.SignedContractFileUrl.Should().Contain("signed");
        contract.SignedContractFileName.Should().Be("signed_contract.pdf");
    }

    #endregion

    #region BaseEntity Inheritance Tests

    [Fact]
    public void Payment_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var payment = new Payment();

        // Assert
        payment.Should().BeAssignableTo<BaseEntity>();
        payment.Id.Should().Be(0);
        payment.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Subscription_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var subscription = new Subscription();

        // Assert
        subscription.Should().BeAssignableTo<BaseEntity>();
        subscription.Id.Should().Be(0);
        subscription.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void SubscriptionItem_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var item = new SubscriptionItem();

        // Assert
        item.Should().BeAssignableTo<BaseEntity>();
        item.Id.Should().Be(0);
        item.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Contract_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var contract = new Contract();

        // Assert
        contract.Should().BeAssignableTo<BaseEntity>();
        contract.Id.Should().Be(0);
        contract.IsDeleted.Should().BeFalse();
    }

    #endregion
}
