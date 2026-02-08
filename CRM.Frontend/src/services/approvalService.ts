/**
 * Approval Workflow Service
 * 
 * Provides API operations for approval workflow functionality including:
 * - Approval requests management
 * - Approval matrices configuration
 * - Approval actions (approve/reject/delegate)
 * - Approval history and statistics
 */
import apiClient from './apiClient';

// ============================================================================
// Types and Interfaces
// ============================================================================

export interface ApprovalRequest {
  id?: number;
  entityType: ApprovalEntityType;
  entityId: number;
  entityName?: string;
  entityDescription?: string;
  requestedById: number;
  requestedByName?: string;
  requestedAt: string;
  status: ApprovalStatus;
  totalAmount?: number;
  discountPercentage?: number;
  discountAmount?: number;
  reason?: string;
  urgency: ApprovalUrgency;
  dueDate?: string;
  currentLevel: number;
  totalLevels: number;
  approvers: ApprovalApprover[];
  history: ApprovalHistoryEntry[];
  comments?: string;
  attachments?: ApprovalAttachment[];
  metadata?: Record<string, unknown>;
  createdAt?: string;
  updatedAt?: string;
  completedAt?: string;
}

export enum ApprovalEntityType {
  Quote = 'quote',
  Discount = 'discount',
  Contract = 'contract',
  Order = 'order',
  Expense = 'expense',
  PurchaseOrder = 'purchaseOrder',
  Leave = 'leave',
  Custom = 'custom',
}

export enum ApprovalStatus {
  Pending = 'pending',
  InProgress = 'inProgress',
  Approved = 'approved',
  Rejected = 'rejected',
  Cancelled = 'cancelled',
  Delegated = 'delegated',
  Expired = 'expired',
}

export enum ApprovalUrgency {
  Low = 'low',
  Normal = 'normal',
  High = 'high',
  Critical = 'critical',
}

export interface ApprovalApprover {
  id?: number;
  userId: number;
  userName?: string;
  userEmail?: string;
  level: number;
  status: ApproverStatus;
  assignedAt?: string;
  respondedAt?: string;
  response?: 'approved' | 'rejected' | 'delegated';
  comments?: string;
  delegatedToId?: number;
  delegatedToName?: string;
  isRequired: boolean;
  canDelegate: boolean;
}

export enum ApproverStatus {
  Pending = 'pending',
  Waiting = 'waiting',
  Approved = 'approved',
  Rejected = 'rejected',
  Delegated = 'delegated',
  Skipped = 'skipped',
}

export interface ApprovalHistoryEntry {
  id?: number;
  action: string;
  userId: number;
  userName?: string;
  timestamp: string;
  comments?: string;
  previousStatus?: ApprovalStatus;
  newStatus?: ApprovalStatus;
  level?: number;
}

export interface ApprovalAttachment {
  id?: number;
  fileName: string;
  fileSize: number;
  contentType: string;
  uploadedAt: string;
  uploadedById: number;
  uploadedByName?: string;
  url?: string;
}

export interface ApprovalMatrix {
  id?: number;
  name: string;
  description?: string;
  entityType: ApprovalEntityType;
  isActive: boolean;
  levels: ApprovalMatrixLevel[];
  conditions?: ApprovalCondition[];
  settings: ApprovalMatrixSettings;
  createdAt?: string;
  updatedAt?: string;
}

export interface ApprovalMatrixLevel {
  level: number;
  name?: string;
  approverType: ApproverType;
  approverId?: number;
  approverName?: string;
  approverGroupId?: number;
  approverGroupName?: string;
  requiredApprovers: number;
  anyCanApprove: boolean;
  canDelegate: boolean;
  autoApproveAfterHours?: number;
  escalateAfterHours?: number;
  escalateToId?: number;
}

export enum ApproverType {
  User = 'user',
  Group = 'group',
  Role = 'role',
  Manager = 'manager',
  DepartmentHead = 'departmentHead',
  Custom = 'custom',
}

export interface ApprovalCondition {
  field: string;
  operator: string;
  value: string | number;
  minAmount?: number;
  maxAmount?: number;
}

export interface ApprovalMatrixSettings {
  allowParallelApprovals: boolean;
  requireAllLevels: boolean;
  allowSelfApproval: boolean;
  notifyOnSubmit: boolean;
  notifyOnApproval: boolean;
  notifyOnRejection: boolean;
  reminderIntervalHours?: number;
  expirationHours?: number;
}

export interface ApprovalStatistics {
  totalRequests: number;
  pendingRequests: number;
  approvedRequests: number;
  rejectedRequests: number;
  avgApprovalTimeHours: number;
  avgLevelsRequired: number;
  requestsByType: { type: ApprovalEntityType; count: number }[];
  requestsByStatus: { status: ApprovalStatus; count: number }[];
  topApprovers: { userId: number; userName: string; count: number; avgTimeHours: number }[];
  dailyTrend: { date: string; submitted: number; approved: number; rejected: number }[];
  bottlenecks: { level: number; avgWaitTimeHours: number; pendingCount: number }[];
}

export interface ApproverPerformance {
  userId: number;
  userName: string;
  totalAssigned: number;
  totalApproved: number;
  totalRejected: number;
  totalDelegated: number;
  avgResponseTimeHours: number;
  pendingCount: number;
  overdueCount: number;
}

export interface CreateApprovalRequestDto {
  entityType: ApprovalEntityType;
  entityId: number;
  reason?: string;
  urgency?: ApprovalUrgency;
  dueDate?: string;
  comments?: string;
  matrixId?: number;
}

export interface ApprovalActionDto {
  comments?: string;
  attachments?: File[];
}

export interface DelegateDto {
  delegateToUserId: number;
  comments?: string;
}

// ============================================================================
// Approval Workflow Service
// ============================================================================

const approvalService = {
  // === Approval Requests ===

  /**
   * Get all approval requests with optional filtering
   */
  getAllRequests: (
    status?: ApprovalStatus,
    entityType?: ApprovalEntityType,
    page: number = 1,
    pageSize: number = 20
  ) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (status) params.append('status', status);
    if (entityType) params.append('entityType', entityType);
    return apiClient.get<{ items: ApprovalRequest[]; totalCount: number }>(
      `/approvals?${params.toString()}`
    );
  },

  /**
   * Get pending approval requests for current user
   */
  getMyPendingApprovals: (page: number = 1, pageSize: number = 20) =>
    apiClient.get<{ items: ApprovalRequest[]; totalCount: number }>(
      `/approvals/pending?page=${page}&pageSize=${pageSize}`
    ),

  /**
   * Get approval requests submitted by current user
   */
  getMySubmittedRequests: (status?: ApprovalStatus, page: number = 1, pageSize: number = 20) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (status) params.append('status', status);
    return apiClient.get<{ items: ApprovalRequest[]; totalCount: number }>(
      `/approvals/submitted?${params.toString()}`
    );
  },

  /**
   * Get approval request by ID
   */
  getRequestById: (id: number) =>
    apiClient.get<ApprovalRequest>(`/approvals/${id}`),

  /**
   * Get approval requests for a specific entity
   */
  getEntityApprovals: (entityType: ApprovalEntityType, entityId: number) =>
    apiClient.get<ApprovalRequest[]>(`/approvals/entity/${entityType}/${entityId}`),

  /**
   * Create a new approval request
   */
  createRequest: (data: CreateApprovalRequestDto) =>
    apiClient.post<ApprovalRequest>('/approvals', data),

  /**
   * Cancel an approval request
   */
  cancelRequest: (id: number, reason?: string) =>
    apiClient.post<ApprovalRequest>(`/approvals/${id}/cancel`, { reason }),

  /**
   * Resubmit a rejected/cancelled approval request
   */
  resubmitRequest: (id: number, comments?: string) =>
    apiClient.post<ApprovalRequest>(`/approvals/${id}/resubmit`, { comments }),

  // === Approval Actions ===

  /**
   * Approve a request
   */
  approve: (id: number, data?: ApprovalActionDto) =>
    apiClient.post<ApprovalRequest>(`/approvals/${id}/approve`, data || {}),

  /**
   * Reject a request
   */
  reject: (id: number, data: ApprovalActionDto) =>
    apiClient.post<ApprovalRequest>(`/approvals/${id}/reject`, data),

  /**
   * Delegate approval to another user
   */
  delegate: (id: number, data: DelegateDto) =>
    apiClient.post<ApprovalRequest>(`/approvals/${id}/delegate`, data),

  /**
   * Add a comment to an approval request
   */
  addComment: (id: number, comment: string) =>
    apiClient.post(`/approvals/${id}/comments`, { comment }),

  /**
   * Request more information from submitter
   */
  requestInfo: (id: number, questions: string) =>
    apiClient.post<ApprovalRequest>(`/approvals/${id}/request-info`, { questions }),

  /**
   * Provide requested information
   */
  provideInfo: (id: number, response: string) =>
    apiClient.post<ApprovalRequest>(`/approvals/${id}/provide-info`, { response }),

  // === Bulk Actions ===

  /**
   * Bulk approve multiple requests
   */
  bulkApprove: (ids: number[], comments?: string) =>
    apiClient.post<{ successful: number[]; failed: { id: number; error: string }[] }>(
      '/approvals/bulk/approve',
      { ids, comments }
    ),

  /**
   * Bulk reject multiple requests
   */
  bulkReject: (ids: number[], reason: string) =>
    apiClient.post<{ successful: number[]; failed: { id: number; error: string }[] }>(
      '/approvals/bulk/reject',
      { ids, reason }
    ),

  // === Approval Matrices ===

  /**
   * Get all approval matrices
   */
  getMatrices: (entityType?: ApprovalEntityType) => {
    const params = entityType ? `?entityType=${entityType}` : '';
    return apiClient.get<ApprovalMatrix[]>(`/approvals/matrices${params}`);
  },

  /**
   * Get approval matrix by ID
   */
  getMatrixById: (id: number) =>
    apiClient.get<ApprovalMatrix>(`/approvals/matrices/${id}`),

  /**
   * Create a new approval matrix
   */
  createMatrix: (data: Omit<ApprovalMatrix, 'id' | 'createdAt' | 'updatedAt'>) =>
    apiClient.post<ApprovalMatrix>('/approvals/matrices', data),

  /**
   * Update an approval matrix
   */
  updateMatrix: (id: number, data: Partial<ApprovalMatrix>) =>
    apiClient.put<ApprovalMatrix>(`/approvals/matrices/${id}`, data),

  /**
   * Delete an approval matrix
   */
  deleteMatrix: (id: number) =>
    apiClient.delete(`/approvals/matrices/${id}`),

  /**
   * Activate/Deactivate an approval matrix
   */
  setMatrixActive: (id: number, isActive: boolean) =>
    apiClient.patch<ApprovalMatrix>(`/approvals/matrices/${id}/active`, { isActive }),

  /**
   * Clone an approval matrix
   */
  cloneMatrix: (id: number, newName: string) =>
    apiClient.post<ApprovalMatrix>(`/approvals/matrices/${id}/clone`, { newName }),

  /**
   * Get the applicable matrix for an entity
   */
  getApplicableMatrix: (entityType: ApprovalEntityType, entityId: number) =>
    apiClient.get<ApprovalMatrix>(`/approvals/matrices/applicable/${entityType}/${entityId}`),

  // === Approval Requirements ===

  /**
   * Check if entity requires approval
   */
  checkRequiresApproval: (entityType: ApprovalEntityType, entityId: number) =>
    apiClient.get<{
      requiresApproval: boolean;
      matrix?: ApprovalMatrix;
      reason?: string;
      estimatedLevels?: number;
    }>(`/approvals/check/${entityType}/${entityId}`),

  /**
   * Get estimated approval time
   */
  getEstimatedTime: (entityType: ApprovalEntityType, entityId: number) =>
    apiClient.get<{
      estimatedHours: number;
      estimatedLevels: number;
      bottleneckLevel?: number;
    }>(`/approvals/estimate/${entityType}/${entityId}`),

  // === History & Statistics ===

  /**
   * Get approval history for a request
   */
  getHistory: (id: number) =>
    apiClient.get<ApprovalHistoryEntry[]>(`/approvals/${id}/history`),

  /**
   * Get approval statistics
   */
  getStatistics: (fromDate?: string, toDate?: string, entityType?: ApprovalEntityType) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    if (entityType) params.append('entityType', entityType);
    const query = params.toString();
    return apiClient.get<ApprovalStatistics>(`/approvals/statistics${query ? `?${query}` : ''}`);
  },

  /**
   * Get approver performance metrics
   */
  getApproverPerformance: (userId?: number, fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (userId) params.append('userId', userId.toString());
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const query = params.toString();
    return apiClient.get<ApproverPerformance[]>(
      `/approvals/performance${query ? `?${query}` : ''}`
    );
  },

  // === Reminders & Notifications ===

  /**
   * Send reminder for a pending approval
   */
  sendReminder: (id: number) =>
    apiClient.post(`/approvals/${id}/remind`),

  /**
   * Get overdue approval requests
   */
  getOverdueRequests: () =>
    apiClient.get<ApprovalRequest[]>('/approvals/overdue'),

  // === Attachments ===

  /**
   * Upload attachment to approval request
   */
  uploadAttachment: (id: number, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.post<ApprovalAttachment>(`/approvals/${id}/attachments`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },

  /**
   * Delete attachment from approval request
   */
  deleteAttachment: (id: number, attachmentId: number) =>
    apiClient.delete(`/approvals/${id}/attachments/${attachmentId}`),

  /**
   * Download attachment
   */
  downloadAttachment: (id: number, attachmentId: number) =>
    apiClient.get(`/approvals/${id}/attachments/${attachmentId}/download`, {
      responseType: 'blob',
    }),

  // === Templates ===

  /**
   * Get approval request templates
   */
  getTemplates: (entityType?: ApprovalEntityType) => {
    const params = entityType ? `?entityType=${entityType}` : '';
    return apiClient.get<{
      id: number;
      name: string;
      entityType: ApprovalEntityType;
      defaultReason?: string;
      defaultUrgency: ApprovalUrgency;
    }[]>(`/approvals/templates${params}`);
  },

  // === Export ===

  /**
   * Export approval requests to CSV
   */
  exportRequests: (
    fromDate?: string,
    toDate?: string,
    status?: ApprovalStatus,
    entityType?: ApprovalEntityType
  ) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    if (status) params.append('status', status);
    if (entityType) params.append('entityType', entityType);
    const query = params.toString();
    return apiClient.get(`/approvals/export${query ? `?${query}` : ''}`, {
      responseType: 'blob',
    });
  },
};

export default approvalService;
