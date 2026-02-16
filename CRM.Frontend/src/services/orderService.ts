import apiClient from './apiClient';

// ─── Enums ───────────────────────────────────────────────────────────────────

export enum OrderStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Processing = 3,
  PartiallyFulfilled = 4,
  Fulfilled = 5,
  Delivered = 6,
  Completed = 7,
  Cancelled = 8,
  Returned = 9,
  Refunded = 10,
  OnHold = 11,
  ActionRequired = 12,
}

// ─── Interfaces ──────────────────────────────────────────────────────────────

export interface Order {
  id: number;
  orderNumber?: string;
  customerId?: number;
  quoteId?: number;
  opportunityId?: number;
  status: OrderStatus;
  orderDate?: string;
  shippingDate?: string;
  deliveryDate?: string;
  subtotal?: number;
  taxAmount?: number;
  shippingAmount?: number;
  discountAmount?: number;
  totalAmount?: number;
  shippingAddress?: string;
  billingAddress?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface OrderLineItem {
  id: number;
  orderId: number;
  productId?: number;
  description?: string;
  quantity: number;
  unitPrice: number;
  discount?: number;
  taxRate?: number;
  totalPrice?: number;
}

export interface OrderReturnItem {
  orderLineItemId: number;
  quantity: number;
  reason?: string;
  condition?: string;
}

export interface OrderStatistics {
  totalOrders: number;
  pendingOrders: number;
  fulfilledOrders: number;
  cancelledOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  ordersByStatus?: Record<string, number>;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const getOrderStatusLabel = (status: OrderStatus): string => {
  const labels: Record<OrderStatus, string> = {
    [OrderStatus.Draft]: 'Draft',
    [OrderStatus.PendingApproval]: 'Pending Approval',
    [OrderStatus.Approved]: 'Approved',
    [OrderStatus.Processing]: 'Processing',
    [OrderStatus.PartiallyFulfilled]: 'Partially Fulfilled',
    [OrderStatus.Fulfilled]: 'Fulfilled',
    [OrderStatus.Delivered]: 'Delivered',
    [OrderStatus.Completed]: 'Completed',
    [OrderStatus.Cancelled]: 'Cancelled',
    [OrderStatus.Returned]: 'Returned',
    [OrderStatus.Refunded]: 'Refunded',
    [OrderStatus.OnHold]: 'On Hold',
    [OrderStatus.ActionRequired]: 'Action Required',
  };
  return labels[status] ?? 'Unknown';
};

export const getOrderStatusColor = (status: OrderStatus): string => {
  const colors: Record<OrderStatus, string> = {
    [OrderStatus.Draft]: 'default',
    [OrderStatus.PendingApproval]: 'warning',
    [OrderStatus.Approved]: 'info',
    [OrderStatus.Processing]: 'info',
    [OrderStatus.PartiallyFulfilled]: 'warning',
    [OrderStatus.Fulfilled]: 'success',
    [OrderStatus.Delivered]: 'success',
    [OrderStatus.Completed]: 'success',
    [OrderStatus.Cancelled]: 'default',
    [OrderStatus.Returned]: 'warning',
    [OrderStatus.Refunded]: 'warning',
    [OrderStatus.OnHold]: 'warning',
    [OrderStatus.ActionRequired]: 'error',
  };
  return colors[status] ?? 'default';
};

// ─── Service ─────────────────────────────────────────────────────────────────

const orderService = {
  // CRUD
  getAll: (customerId?: number, status?: OrderStatus) => {
    const params = new URLSearchParams();
    if (customerId !== undefined) params.append('customerId', customerId.toString());
    if (status !== undefined) params.append('status', status.toString());
    const query = params.toString();
    return apiClient.get<Order[]>(`/api/orders${query ? `?${query}` : ''}`);
  },
  getById: (id: number) => apiClient.get<Order>(`/api/orders/${id}`),
  getByOrderNumber: (orderNumber: string) => apiClient.get<Order>(`/api/orders/by-number/${orderNumber}`),
  create: (order: Partial<Order>) => apiClient.post<Order>('/api/orders', order),
  update: (id: number, order: Partial<Order>) => apiClient.put<Order>(`/api/orders/${id}`, order),
  delete: (id: number) => apiClient.delete(`/orders/${id}`),

  // Order Operations
  createFromQuote: (quoteId: number) => apiClient.post<Order>(`/api/orders/from-quote/${quoteId}`, {}),
  createFromOpportunity: (opportunityId: number) => apiClient.post<Order>(`/api/orders/from-opportunity/${opportunityId}`, {}),
  generateNumber: () => apiClient.get<string>('/api/orders/generate-number'),
  clone: (id: number) => apiClient.post<Order>(`/api/orders/${id}/clone`, {}),

  // Status Management
  updateStatus: (id: number, status: OrderStatus) => apiClient.patch<Order>(`/api/orders/${id}/status`, { status }),
  submitForApproval: (id: number) => apiClient.post<Order>(`/api/orders/${id}/submit`, {}),
  approve: (id: number, approvedById: number) => apiClient.post<Order>(`/api/orders/${id}/approve`, { approvedById }),
  reject: (id: number, rejectedById: number, reason: string) =>
    apiClient.post<Order>(`/api/orders/${id}/reject`, { rejectedById, reason }),
  cancel: (id: number, reason: string) => apiClient.post<Order>(`/api/orders/${id}/cancel`, { reason }),
  putOnHold: (id: number, reason: string) => apiClient.post<Order>(`/api/orders/${id}/hold`, { reason }),
  releaseFromHold: (id: number) => apiClient.post<Order>(`/api/orders/${id}/release`, {}),

  // Fulfillment
  markAsFulfilled: (id: number) => apiClient.post<Order>(`/api/orders/${id}/fulfill`, {}),
  markAsPartiallyFulfilled: (id: number, fulfilledLineItemIds: number[]) =>
    apiClient.post<Order>(`/api/orders/${id}/partial-fulfill`, { fulfilledLineItemIds }),
  markAsDelivered: (id: number, deliveryDate?: string) => {
    const params = deliveryDate ? `?deliveryDate=${deliveryDate}` : '';
    return apiClient.post<Order>(`/api/orders/${id}/deliver${params}`, {});
  },
  processReturn: (id: number, returnItems: OrderReturnItem[], reason: string) =>
    apiClient.post<Order>(`/api/orders/${id}/return`, { returnItems, reason }),

  // Line Items
  addLineItem: (orderId: number, lineItem: Partial<OrderLineItem>) =>
    apiClient.post<OrderLineItem>(`/api/orders/${orderId}/line-items`, lineItem),
  updateLineItem: (lineItemId: number, lineItem: Partial<OrderLineItem>) =>
    apiClient.put<OrderLineItem>(`/api/orders/line-items/${lineItemId}`, lineItem),
  removeLineItem: (lineItemId: number) => apiClient.delete(`/orders/line-items/${lineItemId}`),
  getLineItems: (orderId: number) => apiClient.get<OrderLineItem[]>(`/api/orders/${orderId}/line-items`),

  // Queries
  getByStatus: (status: OrderStatus) => apiClient.get<Order[]>(`/api/orders/by-status/${status}`),
  getByDateRange: (fromDate: string, toDate: string) =>
    apiClient.get<Order[]>(`/api/orders/by-date-range?fromDate=${fromDate}&toDate=${toDate}`),
  getRequiringAction: () => apiClient.get<Order[]>('/api/orders/requiring-action'),
  getStatistics: (fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const query = params.toString();
    return apiClient.get<OrderStatistics>(`/api/orders/statistics${query ? `?${query}` : ''}`);
  },
  search: (query: string) => apiClient.get<Order[]>(`/api/orders/search?query=${encodeURIComponent(query)}`),

  // Calculations
  recalculate: (id: number) => apiClient.post<Order>(`/api/orders/${id}/recalculate`, {}),
  applyDiscount: (id: number, discountAmount: number, discountReason?: string) =>
    apiClient.post<Order>(`/api/orders/${id}/discount`, { discountAmount, discountReason }),
  applyCoupon: (id: number, couponCode: string) =>
    apiClient.post<Order>(`/api/orders/${id}/coupon`, { couponCode }),

  // Invoicing
  createInvoice: (orderId: number) => apiClient.post(`/orders/${orderId}/invoice`, {}),
  getInvoices: (orderId: number) => apiClient.get(`/orders/${orderId}/invoices`),
};

export default orderService;
