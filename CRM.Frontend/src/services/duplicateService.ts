// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Duplicate Detection Service - Frontend API service

import api from './apiClient';

// ==================== Types ====================

export interface FieldComparison {
  fieldName: string;
  displayName: string;
  currentValue: string | null;
  matchedValue: string | null;
  matchType: 'Exact' | 'Fuzzy' | 'Phonetic' | 'Partial' | 'None';
  matchConfidence: number;
}

export interface RecordSummary {
  id: number;
  displayName: string;
  createdAt: Date;
  modifiedAt?: Date;
  additionalInfo: Record<string, string>;
}

export interface DuplicateMatch {
  recordId: number;
  matchScore: number;
  matchedFields: FieldComparison[];
  recordSummary: RecordSummary;
  ruleId?: number;
  ruleName?: string;
}

export interface DuplicateCheckResult {
  hasDuplicates: boolean;
  highConfidenceMatch: boolean;
  matches: DuplicateMatch[];
  totalMatchCount: number;
  highestMatchScore: number;
  recommendation: 'CreateNew' | 'ReviewMatches' | 'UpdateExisting' | 'MergeRecords';
  recommendedRecordId?: number;
}

export interface DuplicateCheckRequest {
  entityType: 'Lead' | 'Contact' | 'Account';
  fieldValues: Record<string, string | null>;
  excludeRecordId?: number;
  matchThreshold?: number;
}

export interface MergeRequest {
  entityType: 'Lead' | 'Contact' | 'Account';
  masterRecordId: number;
  recordsToMerge: number[];
  fieldSourceOverrides?: Record<string, number>;
  relinkRelatedRecords?: boolean;
  notes?: string;
}

export interface MergeResult {
  success: boolean;
  masterRecordId: number;
  mergeGroupId?: number;
  recordsMerged: number;
  relatedRecordsRelinked: number;
  errorMessage?: string;
}

export interface UnmergeRequest {
  mergeGroupId: number;
  specificRecordsToRestore?: number[];
  restoreRelatedRecords?: boolean;
}

export interface UnmergeResult {
  success: boolean;
  restoredRecordIds: number[];
  errorMessage?: string;
}

export interface FieldMergePreview {
  fieldName: string;
  displayName: string;
  allValues: Record<number, string | null>;
  sourceRecordId: number;
  finalValue: string | null;
}

export interface RelatedRecordPreview {
  entityType: string;
  count: number;
  description: string;
}

export interface MergePreview {
  fieldPreviews: Record<string, FieldMergePreview>;
  previewRecord: Record<string, string | null>;
  relatedRecords: RelatedRecordPreview[];
  warnings: string[];
}

export interface MergedRecordInfo {
  recordId: number;
  entityType: string;
  isMaster: boolean;
  status: 'Merged' | 'Unmerged';
  mergedAt: Date;
  recordSnapshot?: Record<string, any>;
}

export interface MergeGroupInfo {
  id: number;
  groupIdentifier: string;
  entityType: string;
  masterRecordId: number;
  status: 'Active' | 'Unmerged' | 'PartialUnmerge';
  mergedAt: Date;
  mergedByName?: string;
  unmergedAt?: Date;
  notes?: string;
  members: MergedRecordInfo[];
}

export interface DuplicateRule {
  id: number;
  name: string;
  entityType: string;
  isActive: boolean;
  matchThreshold: number;
  description?: string;
  matchFields: DuplicateMatchField[];
}

export interface DuplicateMatchField {
  id: number;
  fieldName: string;
  matchType: string;
  weight: number;
  transformations?: string;
}

// ==================== API Functions ====================

/**
 * Check for potential duplicates before creating/updating a record
 */
export async function checkForDuplicates(
  request: DuplicateCheckRequest
): Promise<DuplicateCheckResult> {
  const response = await api.post<DuplicateCheckResult>('/api/duplicates/check', request);
  return response.data;
}

/**
 * Get active duplicate detection rules for an entity type
 */
export async function getActiveRules(
  entityType: 'Lead' | 'Contact' | 'Account'
): Promise<DuplicateRule[]> {
  const response = await api.get<DuplicateRule[]>(`/api/duplicates/rules/${entityType}`);
  return response.data;
}

/**
 * Trigger a full duplicate scan for an entity type
 */
export async function scanForDuplicates(
  entityType: 'Lead' | 'Contact' | 'Account',
  ruleId?: number
): Promise<{ totalRecordsScanned: number; duplicateCandidatesFound: number; completedAt: Date }> {
  const response = await api.post(`/api/duplicates/scan/${entityType}`, null, {
    params: ruleId ? { ruleId } : undefined,
  });
  return response.data;
}

/**
 * Get pending duplicate candidates for review
 */
export async function getPendingCandidates(
  entityType: 'Lead' | 'Contact' | 'Account',
  page: number = 1,
  pageSize: number = 20
): Promise<DuplicateMatch[]> {
  const response = await api.get<DuplicateMatch[]>(`/api/duplicates/candidates/${entityType}`, {
    params: { page, pageSize },
  });
  return response.data;
}

/**
 * Preview a merge operation before executing
 */
export async function previewMerge(request: MergeRequest): Promise<MergePreview> {
  const response = await api.post<MergePreview>('/api/duplicates/merge/preview', request);
  return response.data;
}

/**
 * Merge multiple records into a master record
 */
export async function mergeRecords(request: MergeRequest): Promise<MergeResult> {
  const response = await api.post<MergeResult>('/api/duplicates/merge', request);
  return response.data;
}

/**
 * Unmerge records (restore previously merged duplicates)
 */
export async function unmergeRecords(request: UnmergeRequest): Promise<UnmergeResult> {
  const response = await api.post<UnmergeResult>('/api/duplicates/unmerge', request);
  return response.data;
}

/**
 * Get merge history for a specific record
 */
export async function getMergeHistory(
  entityType: 'Lead' | 'Contact' | 'Account',
  recordId: number
): Promise<MergeGroupInfo[]> {
  const response = await api.get<MergeGroupInfo[]>(
    `/api/duplicates/history/${entityType}/${recordId}`
  );
  return response.data;
}

/**
 * Get records that were merged into a master record
 */
export async function getMergedRecords(
  entityType: 'Lead' | 'Contact' | 'Account',
  masterRecordId: number
): Promise<MergedRecordInfo[]> {
  const response = await api.get<MergedRecordInfo[]>(
    `/api/duplicates/merged-into/${entityType}/${masterRecordId}`
  );
  return response.data;
}

/**
 * Get details of a specific merge group
 */
export async function getMergeGroup(mergeGroupId: number): Promise<MergeGroupInfo | null> {
  try {
    const response = await api.get<MergeGroupInfo>(`/api/duplicates/groups/${mergeGroupId}`);
    return response.data;
  } catch {
    return null;
  }
}

// ==================== Helper Functions ====================

/**
 * Get recommendation text based on duplicate check result
 */
export function getRecommendationText(result: DuplicateCheckResult): string {
  switch (result.recommendation) {
    case 'CreateNew':
      return 'No significant duplicates found. Safe to create new record.';
    case 'ReviewMatches':
      return `Found ${result.totalMatchCount} potential duplicate(s). Please review before proceeding.`;
    case 'UpdateExisting':
      return 'High confidence match found. Consider updating the existing record instead.';
    case 'MergeRecords':
      return 'Multiple high-confidence matches found. Consider merging these records.';
    default:
      return '';
  }
}

/**
 * Get match type display text
 */
export function getMatchTypeText(matchType: string): string {
  switch (matchType) {
    case 'Exact':
      return 'Exact match';
    case 'Fuzzy':
      return 'Similar (fuzzy match)';
    case 'Phonetic':
      return 'Sounds like (phonetic)';
    case 'Partial':
      return 'Partial match';
    case 'None':
      return 'No match';
    default:
      return matchType;
  }
}

/**
 * Get color for match score
 */
export function getMatchScoreColor(score: number): 'error' | 'warning' | 'success' | 'default' {
  if (score >= 90) return 'error'; // High confidence - likely duplicate
  if (score >= 70) return 'warning'; // Medium confidence - needs review
  if (score >= 50) return 'success'; // Low confidence - probably different
  return 'default';
}

/**
 * Format match score as percentage
 */
export function formatMatchScore(score: number): string {
  return `${Math.round(score)}%`;
}

/**
 * Build field values object from entity for duplicate check
 */
export function buildFieldValuesFromLead(lead: any): Record<string, string | null> {
  return {
    FirstName: lead.firstName || null,
    LastName: lead.lastName || null,
    Email: lead.email || null,
    Phone: lead.phone || null,
    CompanyName: lead.companyName || null,
    Title: lead.title || null,
    Website: lead.website || null,
  };
}

export function buildFieldValuesFromContact(contact: any): Record<string, string | null> {
  return {
    FirstName: contact.firstName || null,
    LastName: contact.lastName || null,
    EmailPrimary: contact.emailPrimary || contact.email || null,
    PhonePrimary: contact.phonePrimary || contact.phone || null,
    Company: contact.company || null,
    JobTitle: contact.jobTitle || null,
    Address: contact.address || null,
    City: contact.city || null,
    State: contact.state || null,
    ZipCode: contact.zipCode || null,
  };
}

export function buildFieldValuesFromAccount(account: any): Record<string, string | null> {
  return {
    FirstName: account.firstName || null,
    LastName: account.lastName || null,
    Company: account.company || null,
    Email: account.email || null,
    Phone: account.phone || null,
    Website: account.website || null,
    Address: account.address || null,
    City: account.city || null,
    State: account.state || null,
    ZipCode: account.zipCode || null,
    Industry: account.industry || null,
  };
}

export default {
  checkForDuplicates,
  getActiveRules,
  scanForDuplicates,
  getPendingCandidates,
  previewMerge,
  mergeRecords,
  unmergeRecords,
  getMergeHistory,
  getMergedRecords,
  getMergeGroup,
  getRecommendationText,
  getMatchTypeText,
  getMatchScoreColor,
  formatMatchScore,
  buildFieldValuesFromLead,
  buildFieldValuesFromContact,
  buildFieldValuesFromAccount,
};
