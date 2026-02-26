import apiClient from './apiClient';

// ── Types ────────────────────────────────────────────────────────────────────

export interface RecordCommentItem {
  id: number;
  entityType: string;
  entityId: number;
  content: string;
  authorId: number;
  authorName: string;
  authorAvatarUrl?: string | null;
  parentCommentId?: number | null;
  mentionedUserIds?: string | null;
  replies: RecordCommentItem[];
  createdAt: string;
  updatedAt?: string | null;
  canEdit: boolean;
  canDelete: boolean;
}

export interface CreateRecordCommentPayload {
  entityType: string;
  entityId: number;
  content: string;
  parentCommentId?: number | null;
  mentionedUserIds?: string | null;
}

export interface UpdateRecordCommentPayload {
  content: string;
  mentionedUserIds?: string | null;
}

// ── Service ──────────────────────────────────────────────────────────────────

/**
 * Get all top-level comments (with replies) for a given entity.
 */
export const getCommentsByEntity = async (
  entityType: string,
  entityId: number,
): Promise<RecordCommentItem[]> => {
  const response = await apiClient.get<RecordCommentItem[]>('/comments', {
    params: { entityType, entityId },
  });
  return response.data;
};

/**
 * Get a single comment by ID.
 */
export const getCommentById = async (id: number): Promise<RecordCommentItem> => {
  const response = await apiClient.get<RecordCommentItem>(`/comments/${id}`);
  return response.data;
};

/**
 * Create a new comment.
 */
export const createComment = async (
  payload: CreateRecordCommentPayload,
): Promise<RecordCommentItem> => {
  const response = await apiClient.post<RecordCommentItem>('/comments', payload);
  return response.data;
};

/**
 * Update an existing comment (owner only).
 */
export const updateComment = async (
  id: number,
  payload: UpdateRecordCommentPayload,
): Promise<RecordCommentItem> => {
  const response = await apiClient.put<RecordCommentItem>(`/comments/${id}`, payload);
  return response.data;
};

/**
 * Soft-delete a comment (owner or admin).
 */
export const deleteComment = async (id: number): Promise<void> => {
  await apiClient.delete(`/comments/${id}`);
};

/**
 * Get all replies for a parent comment.
 */
export const getThread = async (parentCommentId: number): Promise<RecordCommentItem[]> => {
  const response = await apiClient.get<RecordCommentItem[]>(`/comments/${parentCommentId}/thread`);
  return response.data;
};

const recordCommentService = {
  getCommentsByEntity,
  getCommentById,
  createComment,
  updateComment,
  deleteComment,
  getThread,
};

export default recordCommentService;
