/**
 * Webhook Service - Manages webhooks for system integrations
 */

import apiClient from './apiClient';

// ============================================================================
// Enums
// ============================================================================

export enum WebhookEvent {
  // Incident events
  IncidentCreated = 'incident.created',
  IncidentUpdated = 'incident.updated',
  IncidentStatusChanged = 'incident.status_changed',
  IncidentClosed = 'incident.closed',
  
  // Problem events
  ProblemCreated = 'problem.created',
  ProblemUpdated = 'problem.updated',
  ProblemResolved = 'problem.resolved',
  
  // Change events
  ChangeCreated = 'change.created',
  ChangeApproved = 'change.approved',
  ChangeRejected = 'change.rejected',
  ChangeImplemented = 'change.completed',
  
  // Sales events
  OpportunityCreated = 'opportunity.created',
  OpportunityWon = 'opportunity.won',
  OpportunityLost = 'opportunity.lost',
  OrderCreated = 'order.created',
  OrderFulfilled = 'order.fulfilled',
  
  // Account events
  AccountCreated = 'account.created',
  AccountUpdated = 'account.updated',
  ContactCreated = 'contact.created',
  ContactUpdated = 'contact.updated',
  
  // Campaign events
  CampaignStarted = 'campaign.started',
  CampaignCompleted = 'campaign.completed',
  
  // Custom event
  Custom = 'custom',
}

export enum WebhookStatus {
  Active = 0,
  Inactive = 1,
  Paused = 2,
  Disabled = 3,
}

export enum DeliveryStatus {
  Pending = 0,
  Delivered = 1,
  Failed = 2,
  Retrying = 3,
}

// ============================================================================
// Interfaces
// ============================================================================

export interface Webhook {
  id: number;
  name: string;
  description?: string;
  url: string;
  events: WebhookEvent[];
  status: WebhookStatus;
  isActive: boolean;
  secret?: string;
  headers?: Record<string, string>;
  retryPolicy?: RetryPolicy;
  createdAt: string;
  updatedAt: string;
  lastDeliveryAt?: string;
  lastDeliveryStatus?: DeliveryStatus;
  totalDeliveries: number;
  successfulDeliveries: number;
  failedDeliveries: number;
}

export interface WebhookDelivery {
  id: number;
  webhookId: number;
  event: WebhookEvent;
  payload: Record<string, any>;
  status: DeliveryStatus;
  statusCode?: number;
  responseBody?: string;
  errorMessage?: string;
  attemptCount: number;
  nextRetryAt?: string;
  deliveredAt: string;
  createdAt: string;
}

export interface RetryPolicy {
  maxRetries: number;
  retryDelayMs: number;
  backoffMultiplier: number;
}

export interface CreateWebhookRequest {
  name: string;
  description?: string;
  url: string;
  events: WebhookEvent[];
  secret?: string;
  headers?: Record<string, string>;
  retryPolicy?: RetryPolicy;
}

export interface UpdateWebhookRequest {
  name?: string;
  description?: string;
  url?: string;
  events?: WebhookEvent[];
  status?: WebhookStatus;
  secret?: string;
  headers?: Record<string, string>;
  retryPolicy?: RetryPolicy;
}

export interface PagedWebhookResult {
  items: Webhook[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PagedDeliveryResult {
  items: WebhookDelivery[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface WebhookTestRequest {
  event: WebhookEvent;
  payload?: Record<string, any>;
}

export interface WebhookTestResult {
  success: boolean;
  statusCode?: number;
  responseBody?: string;
  errorMessage?: string;
  deliveryTime: number;
}

// ============================================================================
// Service
// ============================================================================

const webhookService = {
  /**
   * Get all webhooks with pagination
   */
  getWebhooks: async (
    page: number = 1,
    pageSize: number = 20,
    filter?: {
      status?: WebhookStatus;
      search?: string;
    }
  ): Promise<PagedWebhookResult> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (filter) {
      if (filter.status !== undefined) params.append('status', filter.status.toString());
      if (filter.search) params.append('search', filter.search);
    }

    const response = await apiClient.get(`/api/webhooks?${params}`);
    return response.data;
  },

  /**
   * Get webhook by ID
   */
  getWebhook: async (id: number): Promise<Webhook> => {
    const response = await apiClient.get(`/api/webhooks/${id}`);
    return response.data;
  },

  /**
   * Create new webhook
   */
  createWebhook: async (data: CreateWebhookRequest): Promise<Webhook> => {
    const response = await apiClient.post('/api/webhooks', data);
    return response.data;
  },

  /**
   * Update webhook
   */
  updateWebhook: async (id: number, data: UpdateWebhookRequest): Promise<Webhook> => {
    const response = await apiClient.put(`/api/webhooks/${id}`, data);
    return response.data;
  },

  /**
   * Delete webhook
   */
  deleteWebhook: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/webhooks/${id}`);
  },

  /**
   * Enable webhook
   */
  enableWebhook: async (id: number): Promise<Webhook> => {
    const response = await apiClient.patch(`/api/webhooks/${id}/enable`, {});
    return response.data;
  },

  /**
   * Disable webhook
   */
  disableWebhook: async (id: number): Promise<Webhook> => {
    const response = await apiClient.patch(`/api/webhooks/${id}/disable`, {});
    return response.data;
  },

  /**
   * Pause webhook
   */
  pauseWebhook: async (id: number): Promise<Webhook> => {
    const response = await apiClient.patch(`/api/webhooks/${id}/pause`, {});
    return response.data;
  },

  /**
   * Resume paused webhook
   */
  resumeWebhook: async (id: number): Promise<Webhook> => {
    const response = await apiClient.patch(`/api/webhooks/${id}/resume`, {});
    return response.data;
  },

  /**
   * Test webhook with sample payload
   */
  testWebhook: async (id: number, testRequest: WebhookTestRequest): Promise<WebhookTestResult> => {
    const response = await apiClient.post(`/api/webhooks/${id}/test`, testRequest);
    return response.data;
  },

  /**
   * Get webhook delivery history
   */
  getDeliveries: async (
    webhookId: number,
    page: number = 1,
    pageSize: number = 20,
    filter?: {
      status?: DeliveryStatus;
      event?: WebhookEvent;
      startDate?: string;
      endDate?: string;
    }
  ): Promise<PagedDeliveryResult> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (filter) {
      if (filter.status !== undefined) params.append('status', filter.status.toString());
      if (filter.event) params.append('event', filter.event);
      if (filter.startDate) params.append('startDate', filter.startDate);
      if (filter.endDate) params.append('endDate', filter.endDate);
    }

    const response = await apiClient.get(`/api/webhooks/${webhookId}/deliveries?${params}`);
    return response.data;
  },

  /**
   * Get specific delivery details
   */
  getDelivery: async (webhookId: number, deliveryId: number): Promise<WebhookDelivery> => {
    const response = await apiClient.get(`/api/webhooks/${webhookId}/deliveries/${deliveryId}`);
    return response.data;
  },

  /**
   * Retry failed delivery
   */
  retryDelivery: async (webhookId: number, deliveryId: number): Promise<WebhookDelivery> => {
    const response = await apiClient.post(
      `/api/webhooks/${webhookId}/deliveries/${deliveryId}/retry`,
      {}
    );
    return response.data;
  },

  /**
   * Get available webhook events
   */
  getAvailableEvents: async (): Promise<WebhookEvent[]> => {
    const response = await apiClient.get('/api/webhooks/events');
    return response.data;
  },

  /**
   * Get webhook statistics
   */
  getStatistics: async (webhookId: number): Promise<{
    totalDeliveries: number;
    successfulDeliveries: number;
    failedDeliveries: number;
    successRate: number;
    avgDeliveryTime: number;
  }> => {
    const response = await apiClient.get(`/api/webhooks/${webhookId}/statistics`);
    return response.data;
  },

  /**
   * Validate webhook URL
   */
  validateUrl: async (url: string): Promise<{ valid: boolean; message: string }> => {
    const response = await apiClient.post('/api/webhooks/validate-url', { url });
    return response.data;
  },

  /**
   * Verify webhook signature
   */
  verifySignature: async (
    payload: string,
    signature: string,
    secret: string
  ): Promise<{ valid: boolean }> => {
    const response = await apiClient.post('/api/webhooks/verify-signature', {
      payload,
      signature,
      secret,
    });
    return response.data;
  },
};

export default webhookService;
