/**
 * Sales Module Types
 * Quotes, Orders, Invoices, Payments, Contracts, Subscriptions
 */

import { BaseEntity } from './common';

// ============================================================================
// QUOTES
// ============================================================================

export enum QuoteStatus {
  Draft = 'draft',
  Sent = 'sent',
  Accepted = 'accepted',
  Rejected = 'rejected',
  Expired = 'expired',
  ConvertedToOrder = 'converted_to_order'
}

export interface Quote extends BaseEntity {
  number?: string;
  accountId: number;
  accountName?: string;
  contactId?: number;
  contactName?: string;
  opportunityId?: number;
  status: QuoteStatus;
  total: number;
  subtotal?: number;
  taxRate?: number;
  tax?: number;
  discount?: number;
  discountPercent?: number;
  shippingCost?: number;
  expiryDate?: string;
  validFrom?: string;
  lineItems?: QuoteLineItem[];
  notes?: string;
  terms?: string;
  createdById?: number;
  createdByName?: string;
  currency?: string; // ISO 4217 code (USD, EUR, etc.)
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
  expiryDate: string;
  lineItems: QuoteLineItem[];
  notes?: string;
  terms?: string;
  discount?: number;
  currency?: string;
}

export interface UpdateQuoteDto {
  expiryDate?: string;
  status?: QuoteStatus;
  discount?: number;
  notes?: string;
  terms?: string;
  lineItems?: QuoteLineItem[];
}

// ============================================================================
// ORDERS
// ============================================================================

export enum OrderStatus {
  Draft = 'draft',
  Quote = 'quote',
  Order = 'order',
  Confirmed = 'confirmed',
  Processing = 'processing',
  Shipped = 'shipped',
  Delivered = 'delivered',
  Cancelled = 'cancelled',
  OnHold = 'on_hold'
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
  shippingAddress?: string;
  billingAddress?: string;
  paymentTerms?: string;
  dueDate?: string;
  notes?: string;
  currency?: string;
  createdById?: number;
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
