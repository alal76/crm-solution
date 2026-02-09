import apiClient from './apiClient';

// ─── Enums ───────────────────────────────────────────────────────────────────

export enum InvoiceStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Sent = 3,
  Viewed = 4,
  PartiallyPaid = 5,
  Paid = 6,
  Overdue = 7,
  Disputed = 8,
  Voided = 9,
  WrittenOff = 10,
  Collections = 11,
  Refunded = 12,
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

export interface Invoice {
  id: number;
  invoiceNumber?: string;
  customerId?: number;
  orderId?: number;
  quoteId?: number;
  status: InvoiceStatus;
  issueDate?: string;
  dueDate?: string;
  subtotal?: number;
  taxAmount?: number;
  discountAmount?: number;
  totalAmount?: number;
  amountPaid?: number;
  amountDue?: number;
  notes?: string;
  terms?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface InvoiceLineItem {
  id: number;
  invoiceId: number;
  productId?: number;
  description?: string;
  quantity: number;
  unitPrice: number;
  discount?: number;
  taxRate?: number;
  totalPrice?: number;
}

export interface InvoiceStatistics {
  totalInvoices: number;
  paidInvoices: number;
  overdueInvoices: number;
  totalAmount: number;
  paidAmount: number;
  outstandingAmount: number;
  averageInvoiceAmount: number;
  averageDaysToPayment: number;
}

export interface RecordPaymentRequest {
  amount: number;
  method: PaymentMethod;
}

export interface DiscountRequest {
  discountAmount: number;
  reason?: string;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const getInvoiceStatusLabel = (status: InvoiceStatus): string => {
  const labels: Record<InvoiceStatus, string> = {
    [InvoiceStatus.Draft]: 'Draft',
    [InvoiceStatus.PendingApproval]: 'Pending Approval',
    [InvoiceStatus.Approved]: 'Approved',
    [InvoiceStatus.Sent]: 'Sent',
    [InvoiceStatus.Viewed]: 'Viewed',
    [InvoiceStatus.PartiallyPaid]: 'Partially Paid',
    [InvoiceStatus.Paid]: 'Paid',
    [InvoiceStatus.Overdue]: 'Overdue',
    [InvoiceStatus.Disputed]: 'Disputed',
    [InvoiceStatus.Voided]: 'Voided',
    [InvoiceStatus.WrittenOff]: 'Written Off',
    [InvoiceStatus.Collections]: 'Collections',
    [InvoiceStatus.Refunded]: 'Refunded',
  };
  return labels[status] ?? 'Unknown';
};

export const getInvoiceStatusColor = (status: InvoiceStatus): string => {
  const colors: Record<InvoiceStatus, string> = {
    [InvoiceStatus.Draft]: 'default',
    [InvoiceStatus.PendingApproval]: 'warning',
    [InvoiceStatus.Approved]: 'info',
    [InvoiceStatus.Sent]: 'info',
    [InvoiceStatus.Viewed]: 'info',
    [InvoiceStatus.PartiallyPaid]: 'warning',
    [InvoiceStatus.Paid]: 'success',
    [InvoiceStatus.Overdue]: 'error',
    [InvoiceStatus.Disputed]: 'error',
    [InvoiceStatus.Voided]: 'default',
    [InvoiceStatus.WrittenOff]: 'default',
    [InvoiceStatus.Collections]: 'error',
    [InvoiceStatus.Refunded]: 'warning',
  };
  return colors[status] ?? 'default';
};

// ─── Service ─────────────────────────────────────────────────────────────────

const invoiceService = {
  // CRUD
  getAll: (customerId?: number, status?: InvoiceStatus) => {
    const params = new URLSearchParams();
    if (customerId !== undefined) params.append('customerId', customerId.toString());
    if (status !== undefined) params.append('status', status.toString());
    const query = params.toString();
    return apiClient.get<Invoice[]>(`/api/invoices${query ? `?${query}` : ''}`);
  },
  getById: (id: number) => apiClient.get<Invoice>(`/api/invoices/${id}`),
  getByInvoiceNumber: (invoiceNumber: string) => apiClient.get<Invoice>(`/api/invoices/by-number/${invoiceNumber}`),
  create: (invoice: Partial<Invoice>) => apiClient.post<Invoice>('/api/invoices', invoice),
  update: (id: number, invoice: Partial<Invoice>) => apiClient.put<Invoice>(`/api/invoices/${id}`, invoice),
  delete: (id: number) => apiClient.delete(`/api/invoices/${id}`),

  // Operations
  createFromOrder: (orderId: number) => apiClient.post<Invoice>(`/api/invoices/from-order/${orderId}`, {}),
  createFromQuote: (quoteId: number) => apiClient.post<Invoice>(`/api/invoices/from-quote/${quoteId}`, {}),
  generateNumber: () => apiClient.get<string>('/api/invoices/next-number'),
  send: (id: number, recipientEmail?: string) => {
    const params = recipientEmail ? `?recipientEmail=${encodeURIComponent(recipientEmail)}` : '';
    return apiClient.post<Invoice>(`/api/invoices/${id}/send${params}`, {});
  },
  markAsViewed: (id: number) => apiClient.post<Invoice>(`/api/invoices/${id}/viewed`, {}),

  // Status Management
  updateStatus: (id: number, status: InvoiceStatus) => apiClient.patch<Invoice>(`/api/invoices/${id}/status`, { status }),
  approve: (id: number, approvedById: number) => apiClient.post<Invoice>(`/api/invoices/${id}/approve`, { approvedById }),
  void: (id: number, reason: string) => apiClient.post<Invoice>(`/api/invoices/${id}/void`, { reason }),
  markAsPaid: (id: number) => apiClient.post<Invoice>(`/api/invoices/${id}/mark-paid`, {}),

  // Payments
  recordPayment: (id: number, amount: number, method: PaymentMethod) =>
    apiClient.post(`/api/invoices/${id}/payments`, { amount, method }),
  getOutstandingBalance: (id: number) => apiClient.get<number>(`/api/invoices/${id}/balance`),
  getPayments: (id: number) => apiClient.get(`/api/invoices/${id}/payments`),

  // Queries
  getOverdue: (daysPastDue?: number) => {
    const params = daysPastDue !== undefined ? `?daysPastDue=${daysPastDue}` : '';
    return apiClient.get<Invoice[]>(`/api/invoices/overdue${params}`);
  },
  getDue: (fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const query = params.toString();
    return apiClient.get<Invoice[]>(`/api/invoices/due${query ? `?${query}` : ''}`);
  },
  getCustomerStatistics: (customerId: number) => apiClient.get<InvoiceStatistics>(`/api/invoices/statistics/${customerId}`),

  // Line Items
  addLineItem: (invoiceId: number, lineItem: Partial<InvoiceLineItem>) =>
    apiClient.post<InvoiceLineItem>(`/api/invoices/${invoiceId}/line-items`, lineItem),
  updateLineItem: (lineItemId: number, lineItem: Partial<InvoiceLineItem>) =>
    apiClient.put<InvoiceLineItem>(`/api/invoices/line-items/${lineItemId}`, lineItem),
  removeLineItem: (lineItemId: number) => apiClient.delete(`/api/invoices/line-items/${lineItemId}`),
  getLineItems: (invoiceId: number) => apiClient.get<InvoiceLineItem[]>(`/api/invoices/${invoiceId}/line-items`),

  // Calculations
  recalculate: (id: number) => apiClient.post<Invoice>(`/api/invoices/${id}/recalculate`, {}),
  applyDiscount: (id: number, discountAmount: number, reason?: string) =>
    apiClient.post<Invoice>(`/api/invoices/${id}/discount`, { discountAmount, reason }),
};

export default invoiceService;
