// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// TypeScript type definitions for Record Comments & @Mentions (FEAT-COLLAB).
// Re-exported from recordCommentService for convenience.

export type {
  RecordCommentItem,
  CreateRecordCommentPayload,
  UpdateRecordCommentPayload,
} from '../services/recordCommentService';

// ── Standalone types (extended, not in service) ──────────────────────────────

/** Lightweight user hint used by @mention autocomplete. */
export interface MentionUserHint {
  id: number;
  fullName: string;
  username: string;
  avatarUrl?: string | null;
}

/** Props for the RecordComments widget. */
export interface RecordCommentsProps {
  entityType: string;
  entityId: number;
}
