/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Order Returns service — wraps the /api/orderreturns endpoints.
 */

import apiClient from './apiClient';

// ─── Enums ─────────────────────────────────────────────────────────────────

export enum OrderReturnStatus {
  Pending = 0,
  Approved = 1,
  InTransit = 2,
  Received = 3,
  Inspecting = 4,
  Refunded = 5,
  Rejected = 6,
  Cancelled = 7,
  Completed = 8,
}

export enum OrderReturnReason {
  Defective = 0,
  WrongItem = 1,
  NotAsDescribed = 2,
  ChangedMind = 3,
  DamagedInShipping = 4,
  Other = 5,
}

// ─── Interfaces ─────────────────────────────────────────────────────────────

export interface CreateReturnLineItemDto {
  orderLineItemId: number;
  productId: number;
  quantity: number;
  reason?: string;
}

export interface CreateOrderReturnRequest {
  orderId: number;
  /** OrderReturnReason numeric value */
  reason: number;
  reasonDescription?: string;
  notes?: string;
  refundAmount: number;
  restockingFee: number;
  shippingRefund: number;
  lineItems?: CreateReturnLineItemDto[];
}

export interface UpdateReturnStatusRequest {
  status: number;
  notes?: string;
  refundTransactionId?: string;
  returnTrackingNumber?: string;
  returnCarrier?: string;
}

export interface OrderReturn {
  id: number;
  returnNumber: string;
  rmaNumber?: string;
  orderId: number;
  orderNumber?: string;
  accountId?: number;
  accountName?: string;
  status: number;
  statusName: string;
  reason: number;
  reasonName: string;
  reasonDescription?: string;
  notes?: string;
  originalAmount: number;
  refundAmount: number;
  restockingFee: number;
  shippingRefund: number;
  netRefundAmount: number;
  currency: string;
  requestedAt: string;
  approvedAt?: string;
  receivedAt?: string;
  refundedAt?: string;
  completedAt?: string;
  returnTrackingNumber?: string;
  returnCarrier?: string;
  refundTransactionId?: string;
  createdAt: string;
  updatedAt: string;
  lineItems?: Array<{
    orderLineItemId: number;
    productId: number;
    productName: string;
    quantity: number;
    unitPrice: number;
    refundAmount: number;
    reason?: string;
    itemCondition?: string;
  }>;
}

export interface GetReturnsParams {
  orderId?: number;
  accountId?: number;
  status?: number;
}

// ─── Service functions ──────────────────────────────────────────────────────

/** Creates a new order return / RMA request. */
export const createReturn = (data: CreateOrderReturnRequest): Promise<OrderReturn> =>
  apiClient.post('/orderreturns', data).then(r => r.data);

/** Lists all order returns with optional filters. */
export const getReturns = (params?: GetReturnsParams): Promise<OrderReturn[]> =>
  apiClient.get('/orderreturns', { params }).then(r => r.data);

/** Gets a single order return by ID. */
export const getReturnById = (id: number): Promise<OrderReturn> =>
  apiClient.get(`/orderreturns/${id}`).then(r => r.data);

/** Updates the status of an order return. */
export const updateReturnStatus = (id: number, payload: UpdateReturnStatusRequest): Promise<OrderReturn> =>
  apiClient.put(`/orderreturns/${id}`, payload).then(r => r.data);

/** Returns a label for a return reason enum value. */
export const getReturnReasonLabel = (reason: OrderReturnReason): string => {
  const labels: Record<OrderReturnReason, string> = {
    [OrderReturnReason.Defective]: 'Defective Product',
    [OrderReturnReason.WrongItem]: 'Wrong Item Received',
    [OrderReturnReason.NotAsDescribed]: 'Not as Described',
    [OrderReturnReason.ChangedMind]: 'Changed Mind',
    [OrderReturnReason.DamagedInShipping]: 'Damaged in Shipping',
    [OrderReturnReason.Other]: 'Other',
  };
  return labels[reason] ?? 'Unknown';
};

/** Returns a label for a return status enum value. */
export const getReturnStatusLabel = (status: OrderReturnStatus): string => {
  const labels: Record<OrderReturnStatus, string> = {
    [OrderReturnStatus.Pending]: 'Pending',
    [OrderReturnStatus.Approved]: 'Approved',
    [OrderReturnStatus.InTransit]: 'In Transit',
    [OrderReturnStatus.Received]: 'Received',
    [OrderReturnStatus.Inspecting]: 'Inspecting',
    [OrderReturnStatus.Refunded]: 'Refunded',
    [OrderReturnStatus.Rejected]: 'Rejected',
    [OrderReturnStatus.Cancelled]: 'Cancelled',
    [OrderReturnStatus.Completed]: 'Completed',
  };
  return labels[status] ?? 'Unknown';
};

export default {
  createReturn,
  getReturns,
  getReturnById,
  updateReturnStatus,
  getReturnReasonLabel,
  getReturnStatusLabel,
};
