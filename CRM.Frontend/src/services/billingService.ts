/**
 * Billing Service - Manages subscription billing, usage tracking, and analytics
 *
 * Provides methods for:
 * - Billing history retrieval
 * - Usage record management
 * - Subscription analytics (MRR, ARR, churn)
 * - Invoice generation
 */
import apiClient from './apiClient';

// ============================================================================
// Types
// ============================================================================

export interface BillingHistoryDto {
  id: number;
  subscriptionId: number;
  invoiceNumber: string;
  amount: number;
  currency: string;
  status: string;
  billingDate: string;
  dueDate: string;
  paidDate?: string;
}

export interface UsageRecordDto {
  id: number;
  subscriptionId: number;
  metricName: string;
  quantity: number;
  unitPrice: number;
  total: number;
  recordedAt: string;
}

export interface SubscriptionAnalyticsDto {
  mrr: number;
  arr: number;
  churnRate: number;
  activeSubscriptions: number;
  trialSubscriptions: number;
  avgRevenuePerUser: number;
  revenueGrowthRate: number;
}

// ============================================================================
// Service
// ============================================================================

const billingService = {
  /**
   * Get billing history for a subscription
   */
  getBillingHistory: (subscriptionId: number) =>
    apiClient.get<BillingHistoryDto[]>(`/subscriptions/${subscriptionId}/billing`).then(r => r.data),

  /**
   * Get usage records for a subscription
   */
  getUsageRecords: (subscriptionId: number) =>
    apiClient.get<UsageRecordDto[]>(`/subscriptions/${subscriptionId}/usage`).then(r => r.data),

  /**
   * Get subscription analytics (MRR, ARR, churn, etc.)
   */
  getAnalytics: () =>
    apiClient.get<SubscriptionAnalyticsDto>('/subscriptions/analytics').then(r => r.data),

  /**
   * Generate an invoice for a subscription
   */
  generateInvoice: (subscriptionId: number) =>
    apiClient.post<BillingHistoryDto>(`/subscriptions/${subscriptionId}/invoice`).then(r => r.data),

  /**
   * Record usage for a subscription
   */
  recordUsage: (subscriptionId: number, data: { metricName: string; quantity: number }) =>
    apiClient.post<UsageRecordDto>(`/subscriptions/${subscriptionId}/usage`, data).then(r => r.data),
};

export default billingService;
