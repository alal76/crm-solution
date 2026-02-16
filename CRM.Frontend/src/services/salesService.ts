/**
 * Sales Service - Quotes, Orders, Invoices, Payments
 * Fully typed service layer for all sales operations
 */

import apiClient from './apiClient';
import {
  Quote,
  QuoteLineItem,
  Order,
  OrderLineItem,
  Invoice,
  InvoiceLineItem,
  Payment,
  Contract,
  Subscription,
  CreateQuoteDto,
  UpdateQuoteDto,
  CreateOrderDto,
  UpdateOrderDto,
  CreateInvoiceDto,
  UpdateInvoiceDto,
  CreatePaymentDto,
  UpdatePaymentDto,
  CreateContractDto,
  UpdateContractDto,
  CreateSubscriptionDto,
  UpdateSubscriptionDto,
  PaginatedResponse,
  QuoteStatus,
  OrderStatus,
  InvoiceStatus,
  PaymentStatus,
} from '../types/sales';

const salesService = {
  // =========================================================================
  // QUOTES
  // =========================================================================

  /**
   * Get all quotes with pagination
   */
  getQuotes: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Quote>>('/quotes', {
      params: { page, pageSize }
    });
  },

  /**
   * Get quote by ID
   */
  getQuoteById: async (id: number) => {
    return apiClient.get<Quote>(`/quotes/${id}`);
  },

  /**
   * Create quote
   */
  createQuote: async (data: CreateQuoteDto) => {
    return apiClient.post<Quote>('/quotes', data);
  },

  /**
   * Update quote
   */
  updateQuote: async (id: number, data: UpdateQuoteDto) => {
    return apiClient.patch<Quote>(`/quotes/${id}`, data);
  },

  /**
   * Get quotes for account
   */
  getAccountQuotes: async (accountId: number) => {
    return apiClient.get<Quote[]>(`/accounts/${accountId}/quotes`);
  },

  /**
   * Send quote to customer
   */
  sendQuote: async (quoteId: number, email: string) => {
    return apiClient.post(`/quotes/${quoteId}/send`, { email });
  },

  /**
   * Convert quote to order
   */
  convertQuoteToOrder: async (quoteId: number) => {
    return apiClient.post<Order>(`/quotes/${quoteId}/convert-to-order`, {});
  },

  /**
   * Update quote line items
   */
  updateQuoteLineItems: async (quoteId: number, lineItems: QuoteLineItem[]) => {
    return apiClient.put<Quote>(`/quotes/${quoteId}/line-items`, { lineItems });
  },

  /**
   * Delete quote line item
   */
  deleteQuoteLineItem: async (quoteId: number, lineItemId: number) => {
    return apiClient.delete(`/quotes/${quoteId}/line-items/${lineItemId}`);
  },

  // =========================================================================
  // ORDERS
  // =========================================================================

  /**
   * Get all orders with pagination
   */
  getOrders: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Order>>('/orders', {
      params: { page, pageSize }
    });
  },

  /**
   * Get order by ID
   */
  getOrderById: async (id: number) => {
    return apiClient.get<Order>(`/orders/${id}`);
  },

  /**
   * Create order
   */
  createOrder: async (data: CreateOrderDto) => {
    return apiClient.post<Order>('/orders', data);
  },

  /**
   * Update order
   */
  updateOrder: async (id: number, data: UpdateOrderDto) => {
    return apiClient.patch<Order>(`/orders/${id}`, data);
  },

  /**
   * Get orders for account
   */
  getAccountOrders: async (accountId: number) => {
    return apiClient.get<Order[]>(`/accounts/${accountId}/orders`);
  },

  /**
   * Get orders by status
   */
  getOrdersByStatus: async (status: OrderStatus) => {
    return apiClient.get<Order[]>(`/orders/status/${status}`);
  },

  /**
   * Confirm order
   */
  confirmOrder: async (orderId: number) => {
    return apiClient.post(`/orders/${orderId}/confirm`, {});
  },

  /**
   * Ship order
   */
  shipOrder: async (orderId: number, trackingNumber?: string) => {
    return apiClient.post(`/orders/${orderId}/ship`, { trackingNumber });
  },

  /**
   * Cancel order
   */
  cancelOrder: async (orderId: number, reason?: string) => {
    return apiClient.post(`/orders/${orderId}/cancel`, { reason });
  },

  /**
   * Create invoice from order
   */
  createInvoiceFromOrder: async (orderId: number) => {
    return apiClient.post<Invoice>(`/orders/${orderId}/create-invoice`, {});
  },

  /**
   * Update order line items
   */
  updateOrderLineItems: async (orderId: number, lineItems: OrderLineItem[]) => {
    return apiClient.put<Order>(`/orders/${orderId}/line-items`, { lineItems });
  },

  // =========================================================================
  // INVOICES
  // =========================================================================

  /**
   * Get all invoices with pagination
   */
  getInvoices: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Invoice>>('/invoices', {
      params: { page, pageSize }
    });
  },

  /**
   * Get invoice by ID
   */
  getInvoiceById: async (id: number) => {
    return apiClient.get<Invoice>(`/invoices/${id}`);
  },

  /**
   * Create invoice
   */
  createInvoice: async (data: CreateInvoiceDto) => {
    return apiClient.post<Invoice>('/invoices', data);
  },

  /**
   * Update invoice
   */
  updateInvoice: async (id: number, data: UpdateInvoiceDto) => {
    return apiClient.patch<Invoice>(`/invoices/${id}`, data);
  },

  /**
   * Get invoices for account
   */
  getAccountInvoices: async (accountId: number) => {
    return apiClient.get<Invoice[]>(`/accounts/${accountId}/invoices`);
  },

  /**
   * Get invoices by status
   */
  getInvoicesByStatus: async (status: InvoiceStatus) => {
    return apiClient.get<Invoice[]>(`/invoices/status/${status}`);
  },

  /**
   * Send invoice to customer
   */
  sendInvoice: async (invoiceId: number, email: string) => {
    return apiClient.post(`/invoices/${invoiceId}/send`, { email });
  },

  /**
   * Mark invoice as paid
   */
  markInvoiceAsPaid: async (invoiceId: number) => {
    return apiClient.post(`/invoices/${invoiceId}/mark-paid`, {});
  },

  /**
   * Get overdue invoices
   */
  getOverdueInvoices: async () => {
    return apiClient.get<Invoice[]>('/invoices/overdue');
  },

  /**
   * Update invoice line items
   */
  updateInvoiceLineItems: async (invoiceId: number, lineItems: InvoiceLineItem[]) => {
    return apiClient.put<Invoice>(`/invoices/${invoiceId}/line-items`, { lineItems });
  },

  /**
   * Generate invoice PDF
   */
  generateInvoicePDF: async (invoiceId: number) => {
    return apiClient.get(`/invoices/${invoiceId}/export/pdf`, {
      responseType: 'blob'
    });
  },

  // =========================================================================
  // PAYMENTS
  // =========================================================================

  /**
   * Get all payments with pagination
   */
  getPayments: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Payment>>('/payments', {
      params: { page, pageSize }
    });
  },

  /**
   * Get payment by ID
   */
  getPaymentById: async (id: number) => {
    return apiClient.get<Payment>(`/payments/${id}`);
  },

  /**
   * Record payment for invoice
   */
  createPayment: async (data: CreatePaymentDto) => {
    return apiClient.post<Payment>('/payments', data);
  },

  /**
   * Update payment record
   */
  updatePayment: async (id: number, data: UpdatePaymentDto) => {
    return apiClient.patch<Payment>(`/payments/${id}`, data);
  },

  /**
   * Get payments for invoice
   */
  getInvoicePayments: async (invoiceId: number) => {
    return apiClient.get<Payment[]>(`/invoices/${invoiceId}/payments`);
  },

  /**
   * Get payments by status
   */
  getPaymentsByStatus: async (status: PaymentStatus) => {
    return apiClient.get<Payment[]>(`/payments/status/${status}`);
  },

  /**
   * Refund payment
   */
  refundPayment: async (paymentId: number, amount: number, reason?: string) => {
    return apiClient.post(`/payments/${paymentId}/refund`, { amount, reason });
  },

  // =========================================================================
  // CONTRACTS
  // =========================================================================

  /**
   * Get all contracts
   */
  getContracts: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Contract>>('/contracts', {
      params: { page, pageSize }
    });
  },

  /**
   * Get contract by ID
   */
  getContractById: async (id: number) => {
    return apiClient.get<Contract>(`/contracts/${id}`);
  },

  /**
   * Create contract
   */
  createContract: async (data: CreateContractDto) => {
    return apiClient.post<Contract>('/contracts', data);
  },

  /**
   * Update contract
   */
  updateContract: async (id: number, data: UpdateContractDto) => {
    return apiClient.patch<Contract>(`/contracts/${id}`, data);
  },

  /**
   * Get contracts for account
   */
  getAccountContracts: async (accountId: number) => {
    return apiClient.get<Contract[]>(`/accounts/${accountId}/contracts`);
  },

  // =========================================================================
  // SUBSCRIPTIONS
  // =========================================================================

  /**
   * Get all subscriptions
   */
  getSubscriptions: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Subscription>>('/subscriptions', {
      params: { page, pageSize }
    });
  },

  /**
   * Get subscription by ID
   */
  getSubscriptionById: async (id: number) => {
    return apiClient.get<Subscription>(`/subscriptions/${id}`);
  },

  /**
   * Create subscription
   */
  createSubscription: async (data: CreateSubscriptionDto) => {
    return apiClient.post<Subscription>('/subscriptions', data);
  },

  /**
   * Update subscription
   */
  updateSubscription: async (id: number, data: UpdateSubscriptionDto) => {
    return apiClient.patch<Subscription>(`/subscriptions/${id}`, data);
  },

  /**
   * Get subscriptions for account
   */
  getAccountSubscriptions: async (accountId: number) => {
    return apiClient.get<Subscription[]>(`/accounts/${accountId}/subscriptions`);
  },

  /**
   * Cancel subscription
   */
  cancelSubscription: async (subscriptionId: number, reason?: string) => {
    return apiClient.post(`/subscriptions/${subscriptionId}/cancel`, { reason });
  },

  /**
   * Renew subscription
   */
  renewSubscription: async (subscriptionId: number) => {
    return apiClient.post(`/subscriptions/${subscriptionId}/renew`, {});
  }
};

export default salesService;
