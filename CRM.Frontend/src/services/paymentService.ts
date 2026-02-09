import apiClient from './apiClient';

// ─── Enums ───────────────────────────────────────────────────────────────────

export enum PaymentStatus {
  Pending = 0,
  Processing = 1,
  Completed = 2,
  Failed = 3,
  Declined = 4,
  Cancelled = 5,
  Refunded = 6,
  PartiallyRefunded = 7,
  Disputed = 8,
  Voided = 9,
  OnHold = 10,
  Expired = 11,
}

export enum PaymentMethod {
  CreditCard = 0,
  DebitCard = 1,
  BankTransfer = 2,
  WireTransfer = 3,
  Check = 4,
  Cash = 5,
  PayPal = 6,
  Stripe = 7,
  ApplePay = 8,
  GooglePay = 9,
  Venmo = 10,
  Crypto = 11,
  StoreCredit = 12,
  GiftCard = 13,
  Financing = 14,
  PurchaseOrder = 15,
  Other = 16,
}

// ─── Interfaces ──────────────────────────────────────────────────────────────

export interface Payment {
  id: number;
  invoiceId?: number;
  customerId?: number;
  transactionId?: string;
  amount: number;
  paymentMethod: PaymentMethod;
  status: PaymentStatus;
  paymentDate?: string;
  referenceNumber?: string;
  notes?: string;
  failureReason?: string;
  externalReference?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface PaymentResult {
  success: boolean;
  payment?: Payment;
  transactionId?: string;
  errorCode?: string;
  errorMessage?: string;
  metadata?: Record<string, string>;
}

export interface PaymentDetails {
  cardNumber?: string;
  cardExpiry?: string;
  cardCvv?: string;
  cardHolderName?: string;
  bankAccountNumber?: string;
  bankRoutingNumber?: string;
  paymentToken?: string;
  externalReference?: string;
  additionalData?: Record<string, string>;
}

export interface PaymentAllocation {
  invoiceId: number;
  amount: number;
}

export interface PaymentStatistics {
  totalPayments: number;
  totalAmount: number;
  refundedAmount: number;
  successfulPayments: number;
  failedPayments: number;
  pendingPayments: number;
  successRate: number;
  averagePaymentAmount: number;
  amountByMethod?: Record<string, number>;
}

export interface ProcessPaymentRequest {
  invoiceId: number;
  amount: number;
  method: PaymentMethod;
  details?: PaymentDetails;
}

export interface SchedulePaymentRequest {
  invoiceId: number;
  amount: number;
  method: PaymentMethod;
  scheduledDate: string;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const getPaymentStatusLabel = (status: PaymentStatus): string => {
  const labels: Record<PaymentStatus, string> = {
    [PaymentStatus.Pending]: 'Pending',
    [PaymentStatus.Processing]: 'Processing',
    [PaymentStatus.Completed]: 'Completed',
    [PaymentStatus.Failed]: 'Failed',
    [PaymentStatus.Declined]: 'Declined',
    [PaymentStatus.Cancelled]: 'Cancelled',
    [PaymentStatus.Refunded]: 'Refunded',
    [PaymentStatus.PartiallyRefunded]: 'Partially Refunded',
    [PaymentStatus.Disputed]: 'Disputed',
    [PaymentStatus.Voided]: 'Voided',
    [PaymentStatus.OnHold]: 'On Hold',
    [PaymentStatus.Expired]: 'Expired',
  };
  return labels[status] ?? 'Unknown';
};

export const getPaymentStatusColor = (status: PaymentStatus): string => {
  const colors: Record<PaymentStatus, string> = {
    [PaymentStatus.Pending]: 'warning',
    [PaymentStatus.Processing]: 'info',
    [PaymentStatus.Completed]: 'success',
    [PaymentStatus.Failed]: 'error',
    [PaymentStatus.Declined]: 'error',
    [PaymentStatus.Cancelled]: 'default',
    [PaymentStatus.Refunded]: 'warning',
    [PaymentStatus.PartiallyRefunded]: 'warning',
    [PaymentStatus.Disputed]: 'error',
    [PaymentStatus.Voided]: 'default',
    [PaymentStatus.OnHold]: 'warning',
    [PaymentStatus.Expired]: 'default',
  };
  return colors[status] ?? 'default';
};

export const getPaymentMethodLabel = (method: PaymentMethod): string => {
  const labels: Record<PaymentMethod, string> = {
    [PaymentMethod.CreditCard]: 'Credit Card',
    [PaymentMethod.DebitCard]: 'Debit Card',
    [PaymentMethod.BankTransfer]: 'Bank Transfer',
    [PaymentMethod.WireTransfer]: 'Wire Transfer',
    [PaymentMethod.Check]: 'Check',
    [PaymentMethod.Cash]: 'Cash',
    [PaymentMethod.PayPal]: 'PayPal',
    [PaymentMethod.Stripe]: 'Stripe',
    [PaymentMethod.ApplePay]: 'Apple Pay',
    [PaymentMethod.GooglePay]: 'Google Pay',
    [PaymentMethod.Venmo]: 'Venmo',
    [PaymentMethod.Crypto]: 'Cryptocurrency',
    [PaymentMethod.StoreCredit]: 'Store Credit',
    [PaymentMethod.GiftCard]: 'Gift Card',
    [PaymentMethod.Financing]: 'Financing',
    [PaymentMethod.PurchaseOrder]: 'Purchase Order',
    [PaymentMethod.Other]: 'Other',
  };
  return labels[method] ?? 'Unknown';
};

// ─── Service ─────────────────────────────────────────────────────────────────

const paymentService = {
  // CRUD
  getAll: (customerId?: number, invoiceId?: number, status?: PaymentStatus) => {
    const params = new URLSearchParams();
    if (customerId !== undefined) params.append('customerId', customerId.toString());
    if (invoiceId !== undefined) params.append('invoiceId', invoiceId.toString());
    if (status !== undefined) params.append('status', status.toString());
    const query = params.toString();
    return apiClient.get<Payment[]>(`/api/payments${query ? `?${query}` : ''}`);
  },
  getById: (id: number) => apiClient.get<Payment>(`/api/payments/${id}`),
  getByTransactionId: (transactionId: string) => apiClient.get<Payment>(`/api/payments/by-transaction/${transactionId}`),
  create: (payment: Partial<Payment>) => apiClient.post<Payment>('/api/payments', payment),
  update: (id: number, payment: Partial<Payment>) => apiClient.put<Payment>(`/api/payments/${id}`, payment),
  delete: (id: number) => apiClient.delete(`/api/payments/${id}`),

  // Payment Processing
  processPayment: (request: ProcessPaymentRequest) => apiClient.post<PaymentResult>('/api/payments/process', request),
  processRefund: (id: number, amount: number, reason: string) =>
    apiClient.post<PaymentResult>(`/api/payments/${id}/refund`, { amount, reason }),
  voidPayment: (id: number, reason: string) => apiClient.post<Payment>(`/api/payments/${id}/void`, { reason }),
  capturePayment: (id: number, amount?: number) => {
    const params = amount !== undefined ? `?amount=${amount}` : '';
    return apiClient.post<PaymentResult>(`/api/payments/${id}/capture${params}`, {});
  },

  // Status Management
  updateStatus: (id: number, status: PaymentStatus) => apiClient.patch<Payment>(`/api/payments/${id}/status`, { status }),
  markAsCompleted: (id: number) => apiClient.post<Payment>(`/api/payments/${id}/complete`, {}),
  markAsFailed: (id: number, failureReason: string) =>
    apiClient.post<Payment>(`/api/payments/${id}/fail`, { failureReason }),

  // Queries
  getByDateRange: (fromDate: string, toDate: string) =>
    apiClient.get<Payment[]>(`/api/payments/by-date-range?fromDate=${fromDate}&toDate=${toDate}`),
  getPending: () => apiClient.get<Payment[]>('/api/payments/pending'),
  getFailed: (maxRetries?: number) => {
    const params = maxRetries !== undefined ? `?maxRetries=${maxRetries}` : '';
    return apiClient.get<Payment[]>(`/api/payments/failed${params}`);
  },
  getStatistics: (fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const query = params.toString();
    return apiClient.get<PaymentStatistics>(`/api/payments/statistics${query ? `?${query}` : ''}`);
  },
  getCustomerPaymentHistory: (customerId: number) => apiClient.get<Payment[]>(`/api/payments/customer/${customerId}/history`),

  // Reconciliation
  reconcile: (id: number, externalReference: string) =>
    apiClient.post<Payment>(`/api/payments/${id}/reconcile`, { externalReference }),
  getUnreconciled: () => apiClient.get<Payment[]>('/api/payments/unreconciled'),
  applyToInvoices: (id: number, allocations: PaymentAllocation[]) =>
    apiClient.post<PaymentAllocation[]>(`/api/payments/${id}/apply`, allocations),

  // Retry & Recovery
  retry: (id: number) => apiClient.post<PaymentResult>(`/api/payments/${id}/retry`, {}),
  schedule: (request: SchedulePaymentRequest) => apiClient.post<Payment>('/api/payments/schedule', request),
};

export default paymentService;
