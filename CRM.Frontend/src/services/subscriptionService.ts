/**
 * Subscription Service - Manages subscription lifecycle, billing, and usage tracking
 * 
 * This service provides methods for managing subscriptions including:
 * - CRUD operations
 * - Lifecycle actions (activate, pause, resume, cancel, suspend, renew)
 * - Plan changes and add-ons
 * - Billing and invoicing
 * - Usage tracking and limits
 */
import apiClient from './apiClient';

// ============================================================================
// Types
// ============================================================================

export enum SubscriptionStatus {
  Active = 0,
  Paused = 1,
  Cancelled = 2,
  Suspended = 3,
  PendingCancellation = 4,
  Expired = 5,
  Trial = 6,
}

export enum SubscriptionChangeType {
  Immediate = 0,
  EndOfPeriod = 1,
  NextBillingCycle = 2,
}

export interface Subscription {
  id: number;
  subscriptionNumber: string;
  accountId: number;
  account?: {
    id: number;
    company?: string;
    email?: string;
  };
  productId?: number;
  product?: {
    id: number;
    name: string;
    price?: number;
  };
  subscriptionStatus: SubscriptionStatus;
  amount: number;
  mrr?: number;
  arr?: number;
  oneTimeFee?: number;
  currency?: string;
  billingCycle?: string;
  billingStartDate?: string;
  billingEndDate?: string;
  startDate?: string;
  endDate?: string;
  nextBillingDate?: string;
  currentPeriodStart?: string;
  currentPeriodEnd?: string;
  renewalDate?: string;
  isAutoRenew: boolean;
  cancelledAt?: string;
  cancellationReason?: string;
  cancelAtPeriodEnd: boolean;
  pausedAt?: string;
  pauseReason?: string;
  // Billing address
  billingAddress?: string;
  billingCity?: string;
  billingState?: string;
  billingZip?: string;
  billingCountry?: string;
  billingContactName?: string;
  billingContactEmail?: string;
  billingContactPhone?: string;
  // Contract
  contractReference?: string;
  contractNotes?: string;
  contractStartDate?: string;
  contractEndDate?: string;
  // Metadata
  subscriptionOwner?: string;
  subscriptionManagerId?: number;
  orderId?: number;
  tags?: string;
  externalReference?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface SubscriptionCreateRequest {
  accountId: number;
  productId?: number;
  amount: number;
  billingCycle: string;
  startDate?: string;
  endDate?: string;
  billingStartDate?: string;
  billingEndDate?: string;
  isAutoRenew?: boolean;
  currency?: string;
  billingAddress?: string;
  billingCity?: string;
  billingState?: string;
  billingZip?: string;
  billingCountry?: string;
  billingContactName?: string;
  billingContactEmail?: string;
  billingContactPhone?: string;
  contractReference?: string;
  contractNotes?: string;
  cancelAtPeriodEnd?: boolean;
  mrr?: number;
  arr?: number;
  subscriptionManagerId?: number;
  orderId?: number;
  tags?: string;
  status?: SubscriptionStatus;
}

export interface SubscriptionUpdateRequest extends SubscriptionCreateRequest {
  id?: number;
}

export interface Invoice {
  id: number;
  invoiceNumber: string;
  subscriptionId?: number;
  accountId: number;
  amount: number;
  status: string;
  dueDate?: string;
  paidDate?: string;
  createdAt?: string;
}

export interface UsageRecord {
  metricName: string;
  quantity: number;
  timestamp?: string;
}

export interface UsageLimit {
  metricName: string;
  limit: number;
  used: number;
  remaining: number;
  usagePercentage: number;
}

export interface SubscriptionUsageData {
  subscriptionId: number;
  fromDate: string;
  toDate: string;
  metrics: UsageMetric[];
}

export interface UsageMetric {
  metricName: string;
  totalUsage: number;
  unit?: string;
  records: UsageRecord[];
}

export interface SubscriptionStatistics {
  totalSubscriptions: number;
  activeSubscriptions: number;
  trialSubscriptions: number;
  cancelledSubscriptions: number;
  pausedSubscriptions: number;
  mrr: number;
  arr: number;
  churnRate: number;
  conversionRate: number;
  averageRevenuePerUser: number;
  newSubscriptionsThisMonth: number;
  cancellationsThisMonth: number;
  subscriptionsByPlan: Record<string, number>;
}

export interface BillingDetails {
  billingEmail?: string;
  billingName?: string;
  billingAddress?: string;
  billingCity?: string;
  billingState?: string;
  billingZip?: string;
  billingCountry?: string;
  paymentMethodId?: string;
}

// ============================================================================
// Status Helpers
// ============================================================================

export const getStatusLabel = (status: SubscriptionStatus): string => {
  const labels: Record<SubscriptionStatus, string> = {
    [SubscriptionStatus.Active]: 'Active',
    [SubscriptionStatus.Paused]: 'Paused',
    [SubscriptionStatus.Cancelled]: 'Cancelled',
    [SubscriptionStatus.Suspended]: 'Suspended',
    [SubscriptionStatus.PendingCancellation]: 'Pending Cancellation',
    [SubscriptionStatus.Expired]: 'Expired',
    [SubscriptionStatus.Trial]: 'Trial',
  };
  return labels[status] ?? 'Unknown';
};

export const getStatusColor = (status: SubscriptionStatus): 'success' | 'warning' | 'error' | 'info' | 'default' => {
  const colors: Record<SubscriptionStatus, 'success' | 'warning' | 'error' | 'info' | 'default'> = {
    [SubscriptionStatus.Active]: 'success',
    [SubscriptionStatus.Paused]: 'warning',
    [SubscriptionStatus.Cancelled]: 'error',
    [SubscriptionStatus.Suspended]: 'error',
    [SubscriptionStatus.PendingCancellation]: 'warning',
    [SubscriptionStatus.Expired]: 'default',
    [SubscriptionStatus.Trial]: 'info',
  };
  return colors[status] ?? 'default';
};

export const BILLING_CYCLES = ['Weekly', 'Monthly', 'Quarterly', 'Yearly'] as const;

// ============================================================================
// Service
// ============================================================================

const subscriptionService = {
  // =========================================================================
  // CRUD Operations
  // =========================================================================

  /**
   * Get all subscriptions with optional filtering
   */
  getAll: (accountId?: number, status?: SubscriptionStatus) => {
    const params = new URLSearchParams();
    if (accountId !== undefined) params.append('accountId', accountId.toString());
    if (status !== undefined) params.append('status', status.toString());
    const queryString = params.toString();
    return apiClient.get<Subscription[]>(`/subscriptions${queryString ? `?${queryString}` : ''}`);
  },

  /**
   * Get subscription by ID
   */
  getById: (id: number) => apiClient.get<Subscription>(`/subscriptions/${id}`),

  /**
   * Create a new subscription
   */
  create: (data: SubscriptionCreateRequest) =>
    apiClient.post<Subscription>('/subscriptions', data),

  /**
   * Update a subscription
   */
  update: (id: number, data: SubscriptionUpdateRequest) =>
    apiClient.put<Subscription>(`/subscriptions/${id}`, data),

  /**
   * Delete (soft delete) a subscription
   */
  delete: (id: number) => apiClient.delete(`/subscriptions/${id}`),

  // =========================================================================
  // Lifecycle Actions
  // =========================================================================

  /**
   * Activate a subscription
   */
  activate: (id: number) =>
    apiClient.post<Subscription>(`/subscriptions/${id}/activate`, {}),

  /**
   * Pause a subscription
   */
  pause: (id: number, reason?: string) =>
    apiClient.post<Subscription>(`/subscriptions/${id}/pause`, { reason }),

  /**
   * Resume a paused subscription
   */
  resume: (id: number) =>
    apiClient.post<Subscription>(`/subscriptions/${id}/resume`, {}),

  /**
   * Cancel a subscription
   */
  cancel: (id: number, reason: string, immediate: boolean = false) =>
    apiClient.post<Subscription>(`/subscriptions/${id}/cancel`, { reason, immediate }),

  /**
   * Suspend a subscription
   */
  suspend: (id: number, reason: string) =>
    apiClient.post<Subscription>(`/subscriptions/${id}/suspend`, { reason }),

  /**
   * Reactivate a suspended or cancelled subscription
   */
  reactivate: (id: number) =>
    apiClient.post<Subscription>(`/subscriptions/${id}/reactivate`, {}),

  /**
   * Renew a subscription
   */
  renew: (id: number) =>
    apiClient.post<Subscription>(`/subscriptions/${id}/renew`, {}),

  // =========================================================================
  // Plan Changes & Add-ons
  // =========================================================================

  /**
   * Change the subscription plan
   */
  changePlan: (id: number, newPlanId: number, changeType: SubscriptionChangeType = SubscriptionChangeType.Immediate) =>
    apiClient.post<Subscription>(`/subscriptions/${id}/plan`, { newPlanId, changeType }),

  /**
   * Add an addon to subscription
   */
  addAddon: (id: number, addonId: number, quantity: number = 1) =>
    apiClient.post<Subscription>(`/subscriptions/${id}/addons`, { addonId, quantity }),

  /**
   * Remove an addon from subscription
   */
  removeAddon: (id: number, addonId: number) =>
    apiClient.delete<Subscription>(`/subscriptions/${id}/addons/${addonId}`),

  // =========================================================================
  // Billing
  // =========================================================================

  /**
   * Generate an invoice for the subscription
   */
  generateInvoice: (id: number) =>
    apiClient.post<Invoice>(`/subscriptions/${id}/invoice`, {}),

  /**
   * Get billing history for a subscription
   */
  getBillingHistory: (id: number) =>
    apiClient.get<Invoice[]>(`/subscriptions/${id}/billing-history`),

  /**
   * Update billing details
   */
  updateBillingDetails: (id: number, details: BillingDetails) =>
    apiClient.post<Subscription>(`/subscriptions/${id}/billing-details`, details),

  // =========================================================================
  // Usage Tracking
  // =========================================================================

  /**
   * Record usage for a subscription
   */
  recordUsage: (id: number, metricName: string, quantity: number, timestamp?: Date) =>
    apiClient.post(`/subscriptions/${id}/usage`, {
      metricName,
      quantity,
      timestamp: timestamp?.toISOString(),
    }),

  /**
   * Get usage data for a subscription
   */
  getUsage: (id: number, fromDate: Date, toDate: Date) => {
    const params = new URLSearchParams({
      fromDate: fromDate.toISOString(),
      toDate: toDate.toISOString(),
    });
    return apiClient.get<SubscriptionUsageData>(`/subscriptions/${id}/usage?${params}`);
  },

  /**
   * Get usage limits for a subscription
   */
  getUsageLimits: (id: number) =>
    apiClient.get<UsageLimit[]>(`/subscriptions/${id}/usage-limits`),

  // =========================================================================
  // Queries
  // =========================================================================

  /**
   * Get subscriptions due for renewal within specified days
   */
  getDueForRenewal: (withinDays: number = 30) =>
    apiClient.get<Subscription[]>(`/subscriptions/renewals?withinDays=${withinDays}`),

  /**
   * Get subscription statistics
   */
  getStatistics: (fromDate?: Date, toDate?: Date) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate.toISOString());
    if (toDate) params.append('toDate', toDate.toISOString());
    const queryString = params.toString();
    return apiClient.get<SubscriptionStatistics>(`/subscriptions/statistics${queryString ? `?${queryString}` : ''}`);
  },

  /**
   * Get active subscriptions for an account
   */
  getActiveForAccount: (accountId: number) =>
    apiClient.get<Subscription[]>(`/subscriptions/active/${accountId}`),
};

export default subscriptionService;
