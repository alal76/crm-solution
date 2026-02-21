// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

#region Payment Enumerations

/// <summary>
/// FUNCTIONAL: Payment transaction status.
/// TECHNICAL: Controls payment workflow and reconciliation.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Payment initiated, pending processing</summary>
    Pending = 0,

    /// <summary>Payment being processed by gateway</summary>
    Processing = 1,

    /// <summary>Payment completed successfully</summary>
    Completed = 2,

    /// <summary>Payment failed</summary>
    Failed = 3,

    /// <summary>Payment declined by gateway/bank</summary>
    Declined = 4,

    /// <summary>Payment cancelled</summary>
    Cancelled = 5,

    /// <summary>Payment refunded</summary>
    Refunded = 6,

    /// <summary>Partial refund issued</summary>
    PartiallyRefunded = 7,

    /// <summary>Payment disputed/chargeback</summary>
    Disputed = 8,

    /// <summary>Payment voided before settlement</summary>
    Voided = 9,

    /// <summary>Payment held for review</summary>
    OnHold = 10,

    /// <summary>Payment expired</summary>
    Expired = 11
}

/// <summary>
/// FUNCTIONAL: Method of payment.
/// TECHNICAL: Determines processing gateway and rules.
/// </summary>
public enum PaymentMethod
{
    /// <summary>Credit card payment</summary>
    CreditCard = 0,

    /// <summary>Debit card payment</summary>
    DebitCard = 1,

    /// <summary>Bank transfer / ACH</summary>
    BankTransfer = 2,

    /// <summary>Wire transfer</summary>
    WireTransfer = 3,

    /// <summary>Check payment</summary>
    Check = 4,

    /// <summary>Cash payment</summary>
    Cash = 5,

    /// <summary>PayPal</summary>
    PayPal = 6,

    /// <summary>Stripe</summary>
    Stripe = 7,

    /// <summary>Apple Pay</summary>
    ApplePay = 8,

    /// <summary>Google Pay</summary>
    GooglePay = 9,

    /// <summary>Venmo</summary>
    Venmo = 10,

    /// <summary>Cryptocurrency</summary>
    Crypto = 11,

    /// <summary>Store credit</summary>
    StoreCredit = 12,

    /// <summary>Gift card</summary>
    GiftCard = 13,

    /// <summary>Financing/installment plan</summary>
    Financing = 14,

    /// <summary>Purchase order (billed later)</summary>
    PurchaseOrder = 15,

    /// <summary>Other payment method</summary>
    Other = 16
}

/// <summary>
/// FUNCTIONAL: Type of payment transaction.
/// TECHNICAL: Determines accounting treatment.
/// </summary>
public enum PaymentType
{
    /// <summary>Standard payment</summary>
    Payment = 0,

    /// <summary>Refund to customer</summary>
    Refund = 1,

    /// <summary>Pre-authorization hold</summary>
    Authorization = 2,

    /// <summary>Capture of previous authorization</summary>
    Capture = 3,

    /// <summary>Void of previous transaction</summary>
    Void = 4,

    /// <summary>Deposit/advance payment</summary>
    Deposit = 5,

    /// <summary>Write-off adjustment</summary>
    WriteOff = 6,

    /// <summary>Credit application</summary>
    CreditApplication = 7,

    /// <summary>Chargeback</summary>
    Chargeback = 8,

    /// <summary>Chargeback reversal</summary>
    ChargebackReversal = 9
}

#endregion

/// <summary>
/// Payment entity representing a monetary transaction.
/// Links to Invoice for AR tracking.
/// </summary>
public class Payment : BaseEntity
{
    #region Identification

    /// <summary>System-generated payment number</summary>
    public string PaymentNumber { get; set; } = string.Empty;

    /// <summary>External payment reference</summary>
    public string? ExternalPaymentId { get; set; }

    /// <summary>Gateway transaction ID</summary>
    public string? GatewayTransactionId { get; set; }

    /// <summary>Gateway reference number</summary>
    public string? GatewayReference { get; set; }

    /// <summary>Authorization code</summary>
    public string? AuthorizationCode { get; set; }

    /// <summary>Check number (for check payments)</summary>
    public string? CheckNumber { get; set; }

    #endregion

    #region Payment Details

    /// <summary>Payment description/memo</summary>
    public string? Description { get; set; }

    /// <summary>Current payment status</summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>Payment method used</summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

    /// <summary>Type of payment transaction</summary>
    public PaymentType PaymentType { get; set; } = PaymentType.Payment;

    #endregion

    #region Amounts

    /// <summary>Payment amount</summary>
    public decimal Amount { get; set; } = 0;

    /// <summary>Amount applied to invoice</summary>
    public decimal AmountApplied { get; set; } = 0;

    /// <summary>Unapplied/overpayment amount</summary>
    public decimal AmountUnapplied => Amount - AmountApplied;

    /// <summary>Processing fee</summary>
    public decimal ProcessingFee { get; set; } = 0;

    /// <summary>Net amount after fees</summary>
    public decimal NetAmount => Amount - ProcessingFee;

    /// <summary>Refunded amount (if refund)</summary>
    public decimal RefundedAmount { get; set; } = 0;

    /// <summary>Currency code (ISO 4217)</summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>Exchange rate if foreign currency</summary>
    public decimal? ExchangeRate { get; set; }

    #endregion

    #region Dates

    /// <summary>Date payment was made</summary>
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    /// <summary>Date payment was processed</summary>
    public DateTime? ProcessedDate { get; set; }

    /// <summary>Date payment was settled</summary>
    public DateTime? SettledDate { get; set; }

    /// <summary>Date of refund (if refunded)</summary>
    public DateTime? RefundDate { get; set; }

    /// <summary>Deposit date (for bank deposits)</summary>
    public DateTime? DepositDate { get; set; }

    #endregion

    #region Card Details (Masked)

    /// <summary>Card brand (Visa, Mastercard, etc.)</summary>
    public string? CardBrand { get; set; }

    /// <summary>Last 4 digits of card</summary>
    public string? CardLast4 { get; set; }

    /// <summary>Card expiration month</summary>
    public int? CardExpMonth { get; set; }

    /// <summary>Card expiration year</summary>
    public int? CardExpYear { get; set; }

    /// <summary>Cardholder name</summary>
    public string? CardholderName { get; set; }

    #endregion

    #region Bank Details (Masked)

    /// <summary>Bank name</summary>
    public string? BankName { get; set; }

    /// <summary>Last 4 digits of account</summary>
    public string? AccountLast4 { get; set; }

    /// <summary>Account type (checking, savings)</summary>
    public string? AccountType { get; set; }

    /// <summary>Routing number (masked)</summary>
    public string? RoutingNumberLast4 { get; set; }

    #endregion

    #region Gateway Response

    /// <summary>Payment gateway used</summary>
    public string? Gateway { get; set; }

    /// <summary>Gateway response code</summary>
    public string? GatewayResponseCode { get; set; }

    /// <summary>Gateway response message</summary>
    public string? GatewayResponseMessage { get; set; }

    /// <summary>AVS response code</summary>
    public string? AvsResponseCode { get; set; }

    /// <summary>CVV response code</summary>
    public string? CvvResponseCode { get; set; }

    /// <summary>Risk score from gateway</summary>
    public decimal? RiskScore { get; set; }

    /// <summary>Raw gateway response (JSON)</summary>
    public string? GatewayResponseRaw { get; set; }

    #endregion

    #region Fraud & Risk

    /// <summary>Whether flagged for fraud review</summary>
    public bool FraudFlagged { get; set; } = false;

    /// <summary>Fraud review notes</summary>
    public string? FraudNotes { get; set; }

    /// <summary>IP address of payment</summary>
    public string? IpAddress { get; set; }

    /// <summary>Device fingerprint</summary>
    public string? DeviceFingerprint { get; set; }

    #endregion

    #region Relationships

    /// <summary>Customer account ID</summary>
    public int AccountId { get; set; }

    /// <summary>Navigation to customer account</summary>
    public Account? Account { get; set; }

    /// <summary>Related invoice ID</summary>
    public int? InvoiceId { get; set; }

    /// <summary>Navigation to invoice</summary>
    public Invoice? Invoice { get; set; }

    /// <summary>Related order ID (for direct payments)</summary>
    public int? OrderId { get; set; }

    /// <summary>Navigation to order</summary>
    public Order? Order { get; set; }

    /// <summary>Related subscription ID</summary>
    public int? SubscriptionId { get; set; }

    /// <summary>Navigation to subscription</summary>
    public Subscription? Subscription { get; set; }

    /// <summary>Original payment (for refunds)</summary>
    public int? OriginalPaymentId { get; set; }

    /// <summary>Navigation to original payment</summary>
    public Payment? OriginalPayment { get; set; }

    /// <summary>Refund payments linked to this payment</summary>
    public ICollection<Payment> Refunds { get; set; } = new List<Payment>();

    /// <summary>User who processed the payment</summary>
    public int? ProcessedById { get; set; }

    /// <summary>Navigation to processing user</summary>
    public User? ProcessedBy { get; set; }

    #endregion

    #region Scheduling & Retry

    /// <summary>Scheduled payment date</summary>
    public DateTime? ScheduledDate { get; set; }

    /// <summary>Number of retry attempts</summary>
    public int RetryCount { get; set; } = 0;

    #endregion

    #region Reconciliation

    /// <summary>Bank reference number for reconciliation</summary>
    public string? BankReference { get; set; }

    /// <summary>Whether payment has been reconciled</summary>
    public bool IsReconciled { get; set; } = false;

    /// <summary>Date payment was reconciled</summary>
    public DateTime? ReconciledDate { get; set; }

    #endregion

    #region Notes

    /// <summary>Payment notes</summary>
    public string? Notes { get; set; }

    /// <summary>Internal notes</summary>
    public string? InternalNotes { get; set; }

    /// <summary>Failure reason</summary>
    public string? FailureReason { get; set; }

    /// <summary>Refund reason</summary>
    public string? RefundReason { get; set; }

    #endregion

    #region Aliases

    /// <summary>Alias for GatewayTransactionId for service compatibility</summary>
    public string? TransactionId
    {
        get => GatewayTransactionId;
        set => GatewayTransactionId = value;
    }

    #endregion
}
