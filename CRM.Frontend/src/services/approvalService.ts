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
// DATA NORMALIZATION
// ============================================================================
// Backend uses Quote-specific approval while frontend expects generic. These helpers bridge the gap.

/** Raw API response data - JSON object with unknown field types */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type RawApiData = any;

const normalizeApprovalRequest = (raw: RawApiData): ApprovalRequest => ({
  ...(raw as unknown as ApprovalRequest),
  id: raw['id'] as number | undefined,
  entityType: ((raw['entityType']) ?? 'quote') as ApprovalEntityType,
  entityId: ((raw['entityId']) ?? raw['quoteId']) as number,
  entityName: ((raw['entityName'] ?? raw['quoteName']) ?? '') as string,
  requestedById: ((raw['requestedById']) ?? raw['submitterId']) as number,
  requestedByName: ((raw['requestedByName'] ?? raw['submitterName']) ?? '') as string,
  requestedAt: ((raw['requestedAt']) ?? raw['submittedAt']) as string,
  totalAmount: ((raw['totalAmount'] ?? raw['dealAmount']) ?? 0) as number,
  discountPercentage: ((raw['discountPercentage'] ?? raw['discountPercent']) ?? 0) as number,
  reason: ((raw['reason'] ?? raw['justification']) ?? '') as string,
  totalLevels: ((raw['totalLevels'] ?? raw['maxLevelRequired']) ?? 1) as number,
  status: ((raw['status']) ?? 'pending') as ApprovalStatus,
  urgency: ((raw['urgency']) ?? 'normal') as ApprovalUrgency,
  currentLevel: ((raw['currentLevel']) ?? 1) as number,
  approvers: ((raw['approvers'] ?? raw['steps']) ?? []) as ApprovalApprover[],
  history: ((raw['history']) ?? []) as ApprovalHistoryEntry[],
  comments: ((raw['comments']) ?? '') as string,
  attachments: ((raw['attachments']) ?? []) as ApprovalAttachment[],
});

const normalizeStatistics = (raw: RawApiData): ApprovalStatistics => ({
  ...(raw as unknown as ApprovalStatistics),
  totalRequests: ((raw['totalRequests']) ?? 0) as number,
  pendingRequests: ((raw['pendingRequests']) ?? 0) as number,
  approvedRequests: ((raw['approvedRequests']) ?? 0) as number,
  rejectedRequests: ((raw['rejectedRequests']) ?? 0) as number,
  avgApprovalTimeHours: ((raw['avgApprovalTimeHours'] ?? raw['averageTimeToApprovalHours']) ?? 0) as number,
  avgLevelsRequired: ((raw['avgLevelsRequired']) ?? 0) as number,
  requestsByType: ((raw['requestsByType']) ?? []) as ApprovalStatistics['requestsByType'],
  requestsByStatus: ((raw['requestsByStatus']) ?? []) as ApprovalStatistics['requestsByStatus'],
  topApprovers: ((raw['topApprovers']) ?? []) as ApprovalStatistics['topApprovers'],
  dailyTrend: ((raw['dailyTrend']) ?? []) as ApprovalStatistics['dailyTrend'],
  bottlenecks: ((raw['bottlenecks']) ?? []) as ApprovalStatistics['bottlenecks'],
});

const normalizeRequestList = (items: RawApiData[]): ApprovalRequest[] =>
  (items ?? []).map(normalizeApprovalRequest);

// ============================================================================
// Approval Workflow Service
// ============================================================================

const approvalService = {
  // === Approval Requests ===

  /**
   * Get all approval requests with optional filtering
   */
  getAllRequests: async (
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
    try {
      const res = await apiClient.get<{ items: RawApiData[]; totalCount: number }>(
        `/approvals/requests?${params.toString()}`
      );
      return { ...res, data: { items: normalizeRequestList(res.data?.items), totalCount: res.data?.totalCount ?? 0 } };
    } catch {
      return { data: { items: [] as ApprovalRequest[], totalCount: 0 } };
    }
  },

  /**
   * Get pending approval requests for current user
   */
  getMyPendingApprovals: async (page: number = 1, pageSize: number = 20) => {
    try {
      const res = await apiClient.get<{ items: RawApiData[]; totalCount: number }>(
        `/approvals/requests/pending?page=${page}&pageSize=${pageSize}`
      );
      return { ...res, data: { items: normalizeRequestList(res.data?.items), totalCount: res.data?.totalCount ?? 0 } };
    } catch {
      return { data: { items: [] as ApprovalRequest[], totalCount: 0 } };
    }
  },

  /**
   * Get approval requests submitted by current user
   */
  getMySubmittedRequests: async (status?: ApprovalStatus, page: number = 1, pageSize: number = 20) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (status) params.append('status', status);
    try {
      const res = await apiClient.get<{ items: RawApiData[]; totalCount: number }>(
        `/approvals/requests/submitted?${params.toString()}`
      );
      return { ...res, data: { items: normalizeRequestList(res.data?.items), totalCount: res.data?.totalCount ?? 0 } };
    } catch {
      return { data: { items: [] as ApprovalRequest[], totalCount: 0 } };
    }
  },

  /**
   * Get approval request by ID
   */
  getRequestById: async (id: number) => {
    try {
      const res = await apiClient.get<RawApiData>(`/approvals/requests/${id}`);
      return { ...res, data: normalizeApprovalRequest(res.data) };
    } catch {
      return { data: null as ApprovalRequest | null };
    }
  },

  /**
   * Get approval requests for a specific entity
   * Note: No direct backend endpoint - returns empty for now
   */
  getEntityApprovals: async (_entityType: ApprovalEntityType, _entityId: number) => {
    // No direct endpoint - return empty for now
    return { data: [] as ApprovalRequest[] };
  },

  /**
   * Create a new approval request
   */
  createRequest: async (data: CreateApprovalRequestDto) => {
    try {
      const res = await apiClient.post<RawApiData>('/approvals/requests', data);
      return { ...res, data: normalizeApprovalRequest(res.data) };
    } catch (err) {
      throw err;
    }
  },

  /**
   * Cancel/recall an approval request
   */
  cancelRequest: async (id: number, reason?: string) => {
    try {
      const res = await apiClient.post<RawApiData>(`/approvals/requests/${id}/recall`, { reason });
      return { ...res, data: normalizeApprovalRequest(res.data) };
    } catch (err) {
      throw err;
    }
  },

  /**
   * Resubmit a rejected/cancelled approval request
   */
  resubmitRequest: async (id: number, comments?: string) => {
    try {
      const res = await apiClient.post<RawApiData>(`/approvals/requests/${id}/resubmit`, { comments });
      return { ...res, data: normalizeApprovalRequest(res.data) };
    } catch (err) {
      throw err;
    }
  },

  // === Approval Actions ===

  /**
   * Approve a request
   */
  approve: async (id: number, data?: ApprovalActionDto) => {
    try {
      const res = await apiClient.post<RawApiData>(`/approvals/requests/${id}/approve`, data || {});
      return { ...res, data: normalizeApprovalRequest(res.data) };
    } catch (err) {
      throw err;
    }
  },

  /**
   * Reject a request
   */
  reject: async (id: number, data: ApprovalActionDto) => {
    try {
      const res = await apiClient.post<RawApiData>(`/approvals/requests/${id}/reject`, data);
      return { ...res, data: normalizeApprovalRequest(res.data) };
    } catch (err) {
      throw err;
    }
  },

  /**
   * Delegate approval to another user
   */
  delegate: async (id: number, data: DelegateDto) => {
    try {
      const res = await apiClient.post<RawApiData>(`/approvals/requests/${id}/delegate`, data);
      return { ...res, data: normalizeApprovalRequest(res.data) };
    } catch (err) {
      throw err;
    }
  },

  /**
   * Add a comment to an approval request
   */
  addComment: async (id: number, comment: string) => {
    try {
      return await apiClient.post(`/approvals/requests/${id}/comments`, { comment });
    } catch {
      return { data: null };
    }
  },

  /**
   * Request more information from submitter
   * Note: No direct backend endpoint - stub for now
   */
  requestInfo: async (_id: number, _questions: string) => {
    // No direct endpoint available
    return { data: null as ApprovalRequest | null };
  },

  /**
   * Provide requested information
   * Note: No direct backend endpoint - stub for now
   */
  provideInfo: async (_id: number, _response: string) => {
    // No direct endpoint available
    return { data: null as ApprovalRequest | null };
  },

  // === Bulk Actions ===

  /**
   * Bulk approve multiple requests
   */
  bulkApprove: async (ids: number[], comments?: string) => {
    try {
      return await apiClient.post<{ successful: number[]; failed: { id: number; error: string }[] }>(
        '/approvals/requests/bulk/approve',
        { ids, comments }
      );
    } catch {
      return { data: { successful: [] as number[], failed: ids.map(id => ({ id, error: 'Request failed' })) } };
    }
  },

  /**
   * Bulk reject multiple requests
   */
  bulkReject: async (ids: number[], reason: string) => {
    try {
      return await apiClient.post<{ successful: number[]; failed: { id: number; error: string }[] }>(
        '/approvals/requests/bulk/reject',
        { ids, reason }
      );
    } catch {
      return { data: { successful: [] as number[], failed: ids.map(id => ({ id, error: 'Request failed' })) } };
    }
  },

  // === Approval Matrices ===

  /**
   * Get all approval matrices
   */
  getMatrices: async (entityType?: ApprovalEntityType) => {
    const params = entityType ? `?entityType=${entityType}` : '';
    try {
      return await apiClient.get<ApprovalMatrix[]>(`/approvals/matrices${params}`);
    } catch {
      return { data: [] as ApprovalMatrix[] };
    }
  },

  /**
   * Get approval matrix by ID
   */
  getMatrixById: async (id: number) => {
    try {
      return await apiClient.get<ApprovalMatrix>(`/approvals/matrices/${id}`);
    } catch {
      return { data: null as ApprovalMatrix | null };
    }
  },

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
   * Note: No direct backend endpoint - stub for now
   */
  getApplicableMatrix: async (_entityType: ApprovalEntityType, _entityId: number) => {
    // No direct endpoint available
    return { data: null as ApprovalMatrix | null };
  },

  // === Approval Requirements ===

  /**
   * Check if entity requires approval
   * Note: No direct backend endpoint - stub for now
   */
  checkRequiresApproval: async (_entityType: ApprovalEntityType, _entityId: number) => {
    // No direct endpoint available
    return { data: { requiresApproval: false, reason: 'Not implemented' } };
  },

  /**
   * Get estimated approval time
   * Note: No direct backend endpoint - stub for now
   */
  getEstimatedTime: async (_entityType: ApprovalEntityType, _entityId: number) => {
    // No direct endpoint available
    return { data: { estimatedHours: 0, estimatedLevels: 0 } };
  },

  // === History & Statistics ===

  /**
   * Get approval history for a request
   */
  getHistory: async (id: number) => {
    try {
      return await apiClient.get<ApprovalHistoryEntry[]>(`/approvals/requests/${id}/history`);
    } catch {
      return { data: [] as ApprovalHistoryEntry[] };
    }
  },

  /**
   * Get approval statistics
   */
  getStatistics: async (fromDate?: string, toDate?: string, entityType?: ApprovalEntityType) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    if (entityType) params.append('entityType', entityType);
    const query = params.toString();
    try {
      const res = await apiClient.get<RawApiData>(`/approvals/requests/statistics${query ? `?${query}` : ''}`);
      return { ...res, data: normalizeStatistics(res.data ?? {}) };
    } catch {
      return { data: normalizeStatistics({}) };
    }
  },

  /**
   * Get approver performance metrics
   * Note: No direct backend endpoint - stub for now
   */
  getApproverPerformance: async (_userId?: number, _fromDate?: string, _toDate?: string) => {
    // No direct endpoint available
    return { data: [] as ApproverPerformance[] };
  },

  // === Reminders & Notifications ===

  /**
   * Send reminder for a pending approval
   */
  sendReminder: async (id: number) => {
    try {
      return await apiClient.post(`/approvals/requests/${id}/remind`);
    } catch {
      return { data: null };
    }
  },

  /**
   * Get overdue approval requests
   */
  getOverdueRequests: async () => {
    try {
      const res = await apiClient.get<any[]>('/approvals/requests/overdue');
      return { ...res, data: normalizeRequestList(res.data) };
    } catch {
      return { data: [] as ApprovalRequest[] };
    }
  },

  // === Attachments ===

  /**
   * Upload attachment to approval request
   */
  uploadAttachment: async (id: number, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    try {
      return await apiClient.post<ApprovalAttachment>(`/approvals/requests/${id}/attachments`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    } catch (err) {
      throw err;
    }
  },

  /**
   * Delete attachment from approval request
   */
  deleteAttachment: (id: number, attachmentId: number) =>
    apiClient.delete(`/approvals/requests/${id}/attachments/${attachmentId}`),

  /**
   * Download attachment
   */
  downloadAttachment: (id: number, attachmentId: number) =>
    apiClient.get(`/approvals/requests/${id}/attachments/${attachmentId}/download`, {
      responseType: 'blob',
    }),

  // === Templates ===

  /**
   * Get approval request templates
   * Note: No direct backend endpoint - stub for now
   */
  getTemplates: async (_entityType?: ApprovalEntityType) => {
    // No direct endpoint available
    return { data: [] as { id: number; name: string; entityType: ApprovalEntityType; defaultReason?: string; defaultUrgency: ApprovalUrgency }[] };
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
    return apiClient.get(`/approvals/requests/export${query ? `?${query}` : ''}`, {
      responseType: 'blob',
    });
  },
};

export default approvalService;
