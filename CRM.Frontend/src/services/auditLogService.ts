/**
 * Audit Log Service
 *
 * Provides API methods for viewing and managing audit logs:
 * - Querying audit logs with filtering and pagination
 * - Viewing entity change history
 * - Viewing user activity
 * - Searching logs
 * - Getting audit statistics
 * - Exporting logs to CSV
 *
 * Backend controller:
 * - AuditLogsController: api/audit-logs
 */

import apiClient from './apiClient';

// ── Types ────────────────────────────────────────────────────────────────────

export interface AuditLogDto {
  id: number;
  action: string;
  entityType?: string;
  entityId?: number;
  entityName?: string;
  userId?: number;
  userName?: string;
  oldValues?: Record<string, any>;
  newValues?: Record<string, any>;
  changedProperties?: string[];
  ipAddress?: string;
  userAgent?: string;
  timestamp: string;
}

export interface AuditLogPageDto {
  items: AuditLogDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface AuditStatsDto {
  totalActions: number;
  createdCount: number;
  updatedCount: number;
  deletedCount: number;
  uniqueUsers: number;
  actionsByType: Record<string, number>;
  actionsByEntity: Record<string, number>;
}

export interface EntityChangeHistoryDto {
  entityType: string;
  entityId: number;
  changes: AuditLogDto[];
}

export interface CreateAuditLogDto {
  action: string;
  entityType?: string;
  entityId?: number;
  userId?: number;
  details?: string;
  timestamp?: string;
}

export interface AuditLogQueryParams {
  entityType?: string;
  entityId?: number;
  userId?: number;
  action?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

// ── Service ──────────────────────────────────────────────────────────────────

export const auditLogService = {
  /**
   * Get all audit logs with optional filtering and pagination.
   */
  getAuditLogs: async (params: AuditLogQueryParams = {}): Promise<AuditLogPageDto> => {
    const response = await apiClient.get<AuditLogPageDto>('/audit-logs', {
      params: {
        entityType: params.entityType,
        entityId: params.entityId,
        userId: params.userId,
        action: params.action,
        fromDate: params.fromDate,
        toDate: params.toDate,
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 50,
      },
    });
    return response.data;
  },

  /**
   * Create a new audit log entry (for custom/external events).
   */
  createAuditLog: async (dto: CreateAuditLogDto): Promise<{ id: number }> => {
    const response = await apiClient.post<{ id: number }>('/audit-logs', dto);
    return response.data;
  },

  /**
   * Get audit history for a specific entity.
   */
  getEntityHistory: async (entityType: string, entityId: number): Promise<AuditLogDto[]> => {
    const response = await apiClient.get<AuditLogDto[]>(
      `/audit-logs/entity/${encodeURIComponent(entityType)}/${entityId}`
    );
    return response.data;
  },

  /**
   * Get activity logs for a specific user.
   */
  getUserActivity: async (
    userId: number,
    params?: {
      fromDate?: string;
      toDate?: string;
      pageNumber?: number;
      pageSize?: number;
    }
  ): Promise<AuditLogPageDto> => {
    const response = await apiClient.get<AuditLogPageDto>(`/audit-logs/user/${userId}`, {
      params: {
        fromDate: params?.fromDate,
        toDate: params?.toDate,
        pageNumber: params?.pageNumber ?? 1,
        pageSize: params?.pageSize ?? 50,
      },
    });
    return response.data;
  },

  /**
   * Search audit logs by free-text query.
   */
  searchAuditLogs: async (
    query: string,
    pageNumber: number = 1,
    pageSize: number = 50
  ): Promise<AuditLogPageDto> => {
    const response = await apiClient.get<AuditLogPageDto>('/audit-logs/search', {
      params: { query, pageNumber, pageSize },
    });
    return response.data;
  },

  /**
   * Get audit statistics for a date range.
   */
  getAuditStats: async (fromDate: string, toDate: string): Promise<AuditStatsDto> => {
    const response = await apiClient.get<AuditStatsDto>('/audit-logs/statistics', {
      params: { fromDate, toDate },
    });
    return response.data;
  },

  /**
   * Get entity change history with before/after comparison.
   */
  getEntityChangeHistory: async (
    entityType: string,
    entityId: number,
    fromDate?: string,
    toDate?: string
  ): Promise<EntityChangeHistoryDto> => {
    const response = await apiClient.get<EntityChangeHistoryDto>(
      `/audit-logs/changes/${encodeURIComponent(entityType)}/${entityId}`,
      { params: { fromDate, toDate } }
    );
    return response.data;
  },

  /**
   * Export audit logs to CSV. Returns a Blob for download.
   */
  exportAuditLogs: async (params?: {
    entityType?: string;
    fromDate?: string;
    toDate?: string;
  }): Promise<Blob> => {
    const response = await apiClient.get('/audit-logs/export', {
      params: {
        entityType: params?.entityType,
        fromDate: params?.fromDate,
        toDate: params?.toDate,
      },
      responseType: 'blob',
    });
    return response.data;
  },
};

export default auditLogService;
