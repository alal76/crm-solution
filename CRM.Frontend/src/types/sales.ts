/**
 * Sales Module Types
 * Quotes, Orders, Invoices, Payments, Contracts, Subscriptions
 */

import { BaseEntity } from './common';


// ============================================================================
// QUOTES
// ============================================================================

// Numeric enum mapping for API contract
export enum QuoteStatusEnum {
  New = 0,
  Draft = 1,
  UnderApproval = 2,
  Approved = 3,
  Shared = 4,
  Viewed = 5,
  Accepted = 6,
  Rejected = 7,
  Expired = 8,
  Revised = 9,
  Cancelled = 10,
  Converted = 11,
  EndOfLife = 12
}

// String enum for UI
export enum QuoteStatus {
  New = 'new',
  Draft = 'draft',
  UnderApproval = 'under_approval',
  Approved = 'approved',
  Shared = 'shared',
  Viewed = 'viewed',
  Accepted = 'accepted',
  Rejected = 'rejected',
  Expired = 'expired',
  Revised = 'revised',
  Cancelled = 'cancelled',
  Converted = 'converted',
  EndOfLife = 'end_of_life'
}

// Helper: Map numeric API value to string enum
export function quoteStatusFromApi(val: number): QuoteStatus {
  switch (val) {
    case QuoteStatusEnum.New: return QuoteStatus.New;
    case QuoteStatusEnum.Draft: return QuoteStatus.Draft;
    case QuoteStatusEnum.UnderApproval: return QuoteStatus.UnderApproval;
    case QuoteStatusEnum.Approved: return QuoteStatus.Approved;
    case QuoteStatusEnum.Shared: return QuoteStatus.Shared;
    case QuoteStatusEnum.Viewed: return QuoteStatus.Viewed;
    case QuoteStatusEnum.Accepted: return QuoteStatus.Accepted;
    case QuoteStatusEnum.Rejected: return QuoteStatus.Rejected;
    case QuoteStatusEnum.Expired: return QuoteStatus.Expired;
    case QuoteStatusEnum.Revised: return QuoteStatus.Revised;
    case QuoteStatusEnum.Cancelled: return QuoteStatus.Cancelled;
    case QuoteStatusEnum.Converted: return QuoteStatus.Converted;
    case QuoteStatusEnum.EndOfLife: return QuoteStatus.EndOfLife;
    default: return QuoteStatus.Draft;
  }
}

// Helper: Map string enum to numeric API value
export function quoteStatusToApi(val: QuoteStatus): number {
  switch (val) {
    case QuoteStatus.New: return QuoteStatusEnum.New;
    case QuoteStatus.Draft: return QuoteStatusEnum.Draft;
    case QuoteStatus.UnderApproval: return QuoteStatusEnum.UnderApproval;
    case QuoteStatus.Approved: return QuoteStatusEnum.Approved;
    case QuoteStatus.Shared: return QuoteStatusEnum.Shared;
    case QuoteStatus.Viewed: return QuoteStatusEnum.Viewed;
    case QuoteStatus.Accepted: return QuoteStatusEnum.Accepted;
    case QuoteStatus.Rejected: return QuoteStatusEnum.Rejected;
    case QuoteStatus.Expired: return QuoteStatusEnum.Expired;
    case QuoteStatus.Revised: return QuoteStatusEnum.Revised;
    case QuoteStatus.Cancelled: return QuoteStatusEnum.Cancelled;
    case QuoteStatus.Converted: return QuoteStatusEnum.Converted;
    case QuoteStatus.EndOfLife: return QuoteStatusEnum.EndOfLife;
    default: return QuoteStatusEnum.Draft;
  }
}

export interface Quote extends BaseEntity {
  number?: string;
  accountId: number;
  accountName?: string;
  contactId?: number;
  contactName?: string;
  opportunityId?: number;
  status: QuoteStatus; // Use mapping helpers for API contract
  grandTotal: number;   // Renamed from total to match DTO GrandTotal
  subtotal?: number;
  taxRate?: number;
  taxTotal?: number;    // Renamed from tax to match DTO TaxTotal
  discountTotal?: number; // Renamed from discount to match DTO DiscountTotal
  discountPercent?: number;
  shippingCost?: number;
  expirationDate?: string; // Renamed from expiryDate to match DTO ExpirationDate
  validFrom?: string;
  lineItems?: QuoteLineItem[];
  notes?: string;
  terms?: string;
  createdById?: number;
  createdByName?: string;
  currency?: string; // ISO 4217 code (USD, EUR, etc.)

  // Identity
  quoteNumber?: string;
  externalQuoteId?: string;
  version?: number;
  name?: string;
  description?: string;

  // Workflow dates
  sentDate?: string;
  viewedDate?: string;
  acceptedDate?: string;
  rejectedDate?: string;

  // Approval workflow
  requiresApproval?: boolean;
  isApproved?: boolean;
  approvalDate?: string;
  approvalNotes?: string;
  submittedForApprovalDate?: string;
  approvedByUserId?: number;
  approvedByName?: string;

  // Signature
  isSigned?: boolean;
  signedDate?: string;
  signedBy?: string;
  signatureUrl?: string;

  // Contact details
  contactEmail?: string;
  contactPhone?: string;

  // Pricing & terms
  discountReason?: string;
  paymentTerms?: string;
  deliveryTerms?: string;
  warrantyMonths?: number;
  warrantyEndDate?: string;
  termsAndConditions?: string;
  validityDays?: number;

  // Billing address
  billingName?: string;
  billingAddress?: string;
  billingCity?: string;
  billingState?: string;
  billingZipCode?: string;
  billingCountry?: string;

  // Shipping address
  shippingName?: string;
  shippingAddress?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingZipCode?: string;
  shippingCountry?: string;

  // Service/delivery dates
  expectedDeliveryDate?: string;
  actualDeliveryDate?: string;
  serviceStartDate?: string;
  serviceEndDate?: string;

  // Relationships
  assignedToUserId?: number;
  createdByUserId?: number;
  parentQuoteId?: number;

  // Classification
  tags?: string;
  category?: string;
  internalNotes?: string;
  attachments?: string;
  quotePdfUrl?: string;
  customFields?: string;
}

export interface QuoteLineItem {
  id?: number;
  quoteId?: number;
  productId: number;
  productName?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  lineTotal?: number;
  discount?: number;
  tax?: number;
  sequence?: number;
}

export interface CreateQuoteDto {
  accountId: number;
  contactId?: number;
  opportunityId?: number;
  expirationDate: string;
  lineItems: QuoteLineItem[];
  notes?: string;
  terms?: string;
  discountTotal?: number;
  currency?: string;
  name?: string;
  description?: string;
  externalQuoteId?: string;
  paymentTerms?: string;
  internalNotes?: string;
  assignedToUserId?: number;
}

export interface UpdateQuoteDto {
  expirationDate?: string;
  status?: QuoteStatus;
  discountTotal?: number;
  notes?: string;
  terms?: string;
  lineItems?: QuoteLineItem[];
}

// ============================================================================
// ORDERS
// ============================================================================

export enum OrderStatus {
  Draft = 0,
  Submitted = 1,
  Pending = 2,
  Processing = 3,
  Approved = 4,
  OnHold = 5,
  Shipped = 6,
  Delivered = 7,
  Completed = 8,
  Cancelled = 9,
  Refunded = 10,
  Returned = 11,
  ActionRequired = 12
}

export interface Order extends BaseEntity {
  number?: string;
  accountId: number;
  accountName?: string;
  contactId?: number;
  contactName?: string;
  opportunityId?: number;
  orderDate: string;
  requiredDate?: string;
  shippedDate?: string;
  status: OrderStatus;
  total: number;
  subtotal?: number;
  tax?: number;
  taxRate?: number;
  shippingCost?: number;
  discount?: number;
  discountPercent?: number;
  lineItems?: OrderLineItem[];
  paymentTerms?: string;
  dueDate?: string;
  notes?: string;
  currency?: string;
  currencyCode?: string;
  createdById?: number;
  name?: string;
  description?: string;
  orderNumber?: string;
  externalOrderId?: string;
  customerPONumber?: string;
  referenceNumber?: string;
  orderType?: number;
  fulfillmentMethod?: number;
  priority?: number;
  approvedDate?: string;
  promisedDeliveryDate?: string;
  deliveredDate?: string;
  completedDate?: string;
  cancelledDate?: string;
  contractStartDate?: string;
  contractEndDate?: string;
  fulfilledDate?: string;
  submittedDate?: string;
  handlingAmount?: number;
  exchangeRate?: number;
  discountReason?: string;
  billingName?: string;
  billingCompany?: string;
  billingStreet?: string;
  billingCity?: string;
  billingState?: string;
  billingPostalCode?: string;
  billingCountry?: string;
  billingAddress2?: string;
  shippingName?: string;
  shippingCompany?: string;
  shippingStreet?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingPostalCode?: string;
  shippingCountry?: string;
  shippingAddress2?: string;
  shippingAddress?: string;
  billingAddress?: string;
  shippingMethod?: string;
  trackingNumber?: string;
  trackingUrl?: string;
  shippingWeight?: number;
  packageCount?: number;
  paymentMethod?: string;
  amountInvoiced?: number;
  amountPaid?: number;
  balanceDue?: number;
  isPaid?: boolean;
  quoteId?: number;
  ownerId?: number;
  approvedById?: number;
  parentOrderId?: number;
  internalNotes?: string;
  specialInstructions?: string;
  cancellationReason?: string;
  termsAndConditions?: string;
  holdReason?: string;
  rejectionReason?: string;
  returnReason?: string;
  discountCode?: string;
  couponCode?: string;
  mrr?: number;
  arr?: number;
  tcv?: number;
  acv?: number;
  baseCurrencyAmount?: number;
  // UI gap fields
  totalPurchases?: number;
  accountBalance?: number;
  creditLimit?: number;
  preferredPaymentMethod?: string;
  billingCycle?: string;
  leadScore?: number;
  npsScore?: number;
  satisfactionRating?: number;
  preferredContactTime?: string;
  optInEmail?: boolean;
  optInSms?: boolean;
  optInPhone?: boolean;
  preferredContactMethod?: string;
  timezone?: string;
  preferredLanguage?: string;
  accountManagerId?: number;
  territory?: string;
  region?: string;
  segment?: string;
  referralSource?: string;
  referredByAccountId?: number;
  parentAccountId?: number;
  customFields?: string;
  oneTimeRevenue?: number;     // One-time revenue component
  recurringRevenue?: number;   // Recurring revenue component

  // Additional workflow dates
  holdDate?: string;           // Date order was placed on hold

  // Classification
  tags?: string;
  category?: string;
}

export interface OrderLineItem {
  id?: number;
  orderId?: number;
  productId: number;
  productName?: string;
  quantity: number;
  unitPrice: number;
  lineTotal?: number;
  discount?: number;
  tax?: number;
  sequence?: number;
}

export interface CreateOrderDto {
  accountId: number;
  contactId?: number;
  orderDate: string;
  requiredDate?: string;
  lineItems: OrderLineItem[];
  shippingAddress?: string;
  billingAddress?: string;
  paymentTerms?: string;
  dueDate?: string;
  notes?: string;
  currency?: string;
}

export interface UpdateOrderDto {
  status?: OrderStatus;
  requiredDate?: string;
  shippedDate?: string;
  lineItems?: OrderLineItem[];
  notes?: string;
}

// ============================================================================
// INVOICES
// ============================================================================

export enum InvoiceStatus {
  Draft = 'draft',
  Sent = 'sent',
  ViewedByCustomer = 'viewed',
  PartiallyPaid = 'partially_paid',
  Paid = 'paid',
  Overdue = 'overdue',
  Cancelled = 'cancelled',
  WriteOff = 'write_off'
}

export interface Invoice extends BaseEntity {
  number?: string;
  accountId: number;
  accountName?: string;
  orderId?: number;
  orderNumber?: string;
  invoiceDate: string;
  dueDate: string;
  status: InvoiceStatus;
  amount: number;
  paidAmount?: number;
  remainingAmount?: number;
  subtotal?: number;
  tax?: number;
  taxRate?: number;
  discount?: number;
  shippingCost?: number;
  lineItems?: InvoiceLineItem[];
  paymentTerms?: string;
  notes?: string;
  currency?: string;
  sendViaEmail?: boolean;
  emailSentDate?: string;

  // Identity
  invoiceNumber?: string;
  externalInvoiceId?: string;
  referenceNumber?: string;
  batchNumber?: string;
  invoiceType?: string;

  // Dates
  sentDate?: string;
  viewedDate?: string;
  paidDate?: string;
  voidedDate?: string;
  servicePeriodStart?: string;
  servicePeriodEnd?: string;

  // Pricing details
  discountPercent?: number;
  feesAmount?: number;
  amountCredited?: number;
  exchangeRate?: number;

  // Early payment
  earlyPaymentDiscountPercent?: number;
  earlyPaymentDiscountDays?: number;
  earlyPaymentDiscountAmount?: number;

  // Late fees
  lateFeePercent?: number;
  lateFeeAmount?: number;

  // Billing address
  billingName?: string;
  billingCompany?: string;
  billingStreet?: string;
  billingCity?: string;
  billingState?: string;
  billingPostalCode?: string;
  billingCountry?: string;
  billingEmail?: string;
  billingPhone?: string;

  // Collections
  reminderCount?: number;
  lastReminderDate?: string;
  nextReminderDate?: string;
  inCollections?: boolean;
  collectionsDate?: string;

  // Documentation
  internalNotes?: string;
  footer?: string;
  termsAndConditions?: string;
  voidReason?: string;
  disputeReason?: string;
  pdfUrl?: string;

  // Relationships
  contactId?: number;
  subscriptionId?: number;
  originalInvoiceId?: number;
}

export interface InvoiceLineItem {
  id?: number;
  invoiceId?: number;
  description: string;
  quantity: number;
  unitPrice: number;
  lineTotal?: number;
  tax?: number;
  sequence?: number;
}

export interface CreateInvoiceDto {
  accountId: number;
  orderId?: number;
  invoiceDate: string;
  dueDate: string;
  lineItems: InvoiceLineItem[];
  paymentTerms?: string;
  notes?: string;
  currency?: string;
}

export interface UpdateInvoiceDto {
  status?: InvoiceStatus;
  dueDate?: string;
  notes?: string;
  lineItems?: InvoiceLineItem[];
}

// ============================================================================
// PAYMENTS
// ============================================================================

export enum PaymentMethod {
  CreditCard = 'credit_card',
  BankTransfer = 'bank_transfer',
  Check = 'check',
  PayPal = 'paypal',
  ACH = 'ach',
  Cash = 'cash',
  Other = 'other'
}

export enum PaymentStatus {
  Pending = 'pending',
  Processing = 'processing',
  Completed = 'completed',
  Failed = 'failed',
  Refunded = 'refunded'
}

export interface Payment extends BaseEntity {
  number?: string;
  invoiceId: number;
  accountId: number;
  accountName?: string;
  amount: number;
  paymentDate: string;
  paymentMethod: PaymentMethod;
  status: PaymentStatus;
  transactionId?: string;
  reference?: string;
  notes?: string;
  recordedBy?: string;
  currency?: string;

  // Identity
  paymentNumber?: string;
  externalPaymentId?: string;
  gatewayTransactionId?: string;
  checkNumber?: string;
  description?: string;

  // Type & status
  paymentType?: string;

  // Amounts
  processingFee?: number;
  netAmount?: number;
  exchangeRate?: number;

  // Dates
  processedDate?: string;
  settledDate?: string;
  depositDate?: string;

  // Card details
  cardBrand?: string;
  cardLast4?: string;
  cardExpMonth?: number;
  cardExpYear?: number;
  cardholderName?: string;

  // Bank details
  bankName?: string;
  accountLast4?: string;
  accountType?: string;

  // Gateway
  gateway?: string;
  gatewayResponseCode?: string;
  gatewayResponseMessage?: string;

  // Relationships
  orderId?: number;
  subscriptionId?: number;

  // Reconciliation
  isReconciled?: boolean;
  reconciledDate?: string;
  bankReference?: string;

  // Notes
  internalNotes?: string;
  failureReason?: string;
}

export interface CreatePaymentDto {
  invoiceId: number;
  amount: number;
  paymentDate: string;
  paymentMethod: PaymentMethod;
  transactionId?: string;
  reference?: string;
  notes?: string;
}

export interface UpdatePaymentDto {
  status?: PaymentStatus;
  amount?: number;
  paymentDate?: string;
  notes?: string;
}

// ============================================================================
// CONTRACTS
// ============================================================================

export enum ContractStatus {
  Draft = 'draft',
  Pending = 'pending',
  Active = 'active',
  Expired = 'expired',
  Cancelled = 'cancelled',
  Renewed = 'renewed'
}

export interface Contract extends BaseEntity {
  number?: string;
  accountId: number;
  accountName?: string;
  contractName: string;
  startDate: string;
  endDate: string;
  status: ContractStatus;
  value?: number;
  description?: string;
  terms?: string;
  renewalDate?: string;
  autoRenew?: boolean;
  allowedDowntime?: number; // SLA uptime percentage
  supportLevel?: string; // Premium, Standard, Basic
  attachments?: string[];
  currency?: string;

  // Identity
  contractNumber?: string;
  contractType?: string;

  // Relationships
  ownerId?: number;
  ownerName?: string;
  parentContractId?: number;
  quoteId?: number;

  // Dates
  activatedDate?: string;
  terminatedDate?: string;

  // Renewal
  renewalNoticeDays?: number;
  renewalNoticeSent?: boolean;
  renewalNoticeSentDate?: string;
  renewalInitiatedAt?: string;
  renewalCompletedAt?: string;
  renewalTermMonths?: number;

  // Financial
  billingFrequency?: string;

  // Terms
  specialConditions?: string;
  terminationClause?: string;
  terminationReason?: string;

  // Documents
  contractFileUrl?: string;
  contractFileName?: string;
  signedContractFileUrl?: string;
  signedContractFileName?: string;

  // Approval
  approvedByUserId?: number;
  approvedDate?: string;
  rejectionReason?: string;

  // Suspension
  suspensionReason?: string;
  suspendedDate?: string;
}

export interface CreateContractDto {
  accountId: number;
  contractName: string;
  startDate: string;
  endDate: string;
  value?: number;
  description?: string;
  terms?: string;
  autoRenew?: boolean;
  currency?: string;
}

export interface UpdateContractDto {
  contractName?: string;
  endDate?: string;
  status?: ContractStatus;
  terms?: string;
  renewalDate?: string;
  autoRenew?: boolean;
}

// ============================================================================
// SUBSCRIPTIONS
// ============================================================================

export enum SubscriptionStatus {
  Active = 'active',
  Paused = 'paused',
  Cancelled = 'cancelled',
  Expired = 'expired',
  Renewing = 'renewing'
}

export enum BillingInterval {
  Monthly = 'monthly',
  Quarterly = 'quarterly',
  Annual = 'annual',
  Biennial = 'biennial'
}

export interface Subscription extends BaseEntity {
  accountId: number;
  accountName?: string;
  planName: string;
  planId?: number;
  status: SubscriptionStatus;
  amount: number;
  billingInterval: BillingInterval;
  startDate: string;
  nextBillingDate?: string;
  endDate?: string;
  autoRenew?: boolean;
  features?: string[];
  maxSeats?: number;
  currentSeats?: number;
  currency?: string;
}

export interface CreateSubscriptionDto {
  accountId: number;
  planId: number;
  startDate: string;
  billingInterval: BillingInterval;
  autoRenew?: boolean;
}

export interface UpdateSubscriptionDto {
  status?: SubscriptionStatus;
  billingInterval?: BillingInterval;
  autoRenew?: boolean;
  endDate?: string;
}

/**
 * Sales Dashboard Summary
 */
export interface SalesDashboardSummary {
  totalRevenue: number;
  totalOrders: number;
  totalInvoices: number;
  overdueAmount: number;
  quotesAwaitingApproval: number;
  ordersInProgress: number;
}
