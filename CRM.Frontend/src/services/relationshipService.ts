/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Relationship Management Service
 * Handles B2B/B2C relationship tracking, health monitoring, and visualization
 */

import apiClient from './apiClient';

// ============================================================================
// Enums
// ============================================================================

export enum RelationshipCategory {
  B2B = 'B2B',
  B2C = 'B2C',
  Internal = 'Internal'
}

export enum RelationshipStatus {
  Active = 'Active',
  Inactive = 'Inactive',
  Pending = 'Pending',
  Terminated = 'Terminated'
}

export enum StrategicImportance {
  Critical = 'Critical',
  High = 'High',
  Medium = 'Medium',
  Low = 'Low'
}

export enum InteractionType {
  Email = 'Email',
  Call = 'Call',
  Meeting = 'Meeting',
  Visit = 'Visit',
  Contract = 'Contract',
  Referral = 'Referral',
  Support = 'Support',
  Other = 'Other'
}

export enum InteractionOutcome {
  Positive = 'Positive',
  Neutral = 'Neutral',
  Negative = 'Negative'
}

export enum HealthImpact {
  Significant = 'Significant',
  Moderate = 'Moderate',
  Minor = 'Minor',
  None = 'None'
}

export enum HealthTrend {
  Improving = 'Improving',
  Stable = 'Stable',
  Declining = 'Declining'
}

// ============================================================================
// Interfaces
// ============================================================================

export interface RelationshipType {
  id: number;
  typeName: string;
  typeCategory: RelationshipCategory;
  description?: string;
  isBidirectional: boolean;
  reverseTypeName?: string;
  color?: string;
  icon?: string;
  isSystem: boolean;
  sortOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface AccountRelationship {
  id: number;
  sourceCustomerId: number;
  sourceCustomerName?: string;
  targetCustomerId: number;
  targetCustomerName?: string;
  relationshipTypeId: number;
  relationshipTypeName?: string;
  status: RelationshipStatus;
  strengthScore: number;
  strategicImportance: StrategicImportance;
  startDate?: string;
  endDate?: string;
  notes?: string;
  contractValue?: number;
  annualRevenue?: number;
  lastInteractionDate?: string;
  nextScheduledInteraction?: string;
  createdAt: string;
  updatedAt: string;
}

export interface RelationshipInteraction {
  id: number;
  accountRelationshipId: number;
  interactionType: InteractionType;
  subject: string;
  description?: string;
  interactionDate: string;
  outcome: InteractionOutcome;
  healthImpact: HealthImpact;
  strengthChange: number;
  participantIds?: string;
  attachmentUrls?: string;
  followUpRequired: boolean;
  followUpDate?: string;
  followUpNotes?: string;
  createdAt: string;
}

export interface AccountHealthSnapshot {
  id: number;
  customerId: number;
  customerName?: string;
  snapshotDate: string;
  overallHealthScore: number;
  engagementScore: number;
  satisfactionScore: number;
  riskScore: number;
  growthScore: number;
  activeRelationships: number;
  totalInteractions: number;
  recentInteractions: number;
  openOpportunities: number;
  openServiceRequests: number;
  recentRevenue?: number;
  trend: HealthTrend;
  trendPercentage: number;
  riskFactors?: string;
  growthOpportunities?: string;
  recommendations?: string;
  createdAt: string;
}

export interface RelationshipMapVisualization {
  nodes: RelationshipNode[];
  edges: RelationshipEdge[];
  centralCustomerId?: number;
  generatedAt: string;
}

export interface RelationshipNode {
  id: number;
  customerId: number;
  name: string;
  company?: string;
  category: string;
  healthScore?: number;
  relationshipCount: number;
  isCenter: boolean;
  depth: number;
}

export interface RelationshipEdge {
  id: number;
  sourceId: number;
  targetId: number;
  relationshipTypeId: number;
  relationshipTypeName: string;
  strengthScore: number;
  status: RelationshipStatus;
  color?: string;
}

export interface CreateRelationshipTypeRequest {
  typeName: string;
  typeCategory: RelationshipCategory;
  description?: string;
  isBidirectional: boolean;
  reverseTypeName?: string;
  color?: string;
  icon?: string;
  sortOrder?: number;
}

export interface CreateAccountRelationshipRequest {
  sourceCustomerId: number;
  targetCustomerId: number;
  relationshipTypeId: number;
  status?: RelationshipStatus;
  strategicImportance?: StrategicImportance;
  startDate?: string;
  notes?: string;
  contractValue?: number;
  annualRevenue?: number;
}

export interface CreateInteractionRequest {
  accountRelationshipId: number;
  interactionType: InteractionType;
  subject: string;
  description?: string;
  interactionDate?: string;
  outcome: InteractionOutcome;
  healthImpact?: HealthImpact;
  strengthChange?: number;
  followUpRequired?: boolean;
  followUpDate?: string;
  followUpNotes?: string;
}

// ============================================================================
// API Methods
// ============================================================================

const BASE_URL = '/relationships';

// Relationship Types
export const getRelationshipTypes = async (): Promise<RelationshipType[]> => {
  const response = await apiClient.get(`${BASE_URL}/types`);
  return response.data;
};

export const getRelationshipType = async (id: number): Promise<RelationshipType> => {
  const response = await apiClient.get(`${BASE_URL}/types/${id}`);
  return response.data;
};

export const createRelationshipType = async (
  request: CreateRelationshipTypeRequest
): Promise<RelationshipType> => {
  const response = await apiClient.post(`${BASE_URL}/types`, request);
  return response.data;
};

export const updateRelationshipType = async (
  id: number,
  request: Partial<CreateRelationshipTypeRequest>
): Promise<RelationshipType> => {
  const response = await apiClient.put(`${BASE_URL}/types/${id}`, request);
  return response.data;
};

export const deleteRelationshipType = async (id: number): Promise<void> => {
  await apiClient.delete(`${BASE_URL}/types/${id}`);
};

// Account Relationships
export const getAccountRelationships = async (
  customerId?: number
): Promise<AccountRelationship[]> => {
  const url = customerId
    ? `${BASE_URL}?customerId=${customerId}`
    : BASE_URL;
  const response = await apiClient.get(url);
  return response.data;
};

export const getAccountRelationship = async (id: number): Promise<AccountRelationship> => {
  const response = await apiClient.get(`${BASE_URL}/${id}`);
  return response.data;
};

export const createAccountRelationship = async (
  request: CreateAccountRelationshipRequest
): Promise<AccountRelationship> => {
  const response = await apiClient.post(BASE_URL, request);
  return response.data;
};

export const updateAccountRelationship = async (
  id: number,
  request: Partial<CreateAccountRelationshipRequest>
): Promise<AccountRelationship> => {
  const response = await apiClient.put(`${BASE_URL}/${id}`, request);
  return response.data;
};

export const deleteAccountRelationship = async (id: number): Promise<void> => {
  await apiClient.delete(`${BASE_URL}/${id}`);
};

// Relationship Interactions
export const getRelationshipInteractions = async (
  relationshipId: number
): Promise<RelationshipInteraction[]> => {
  const response = await apiClient.get(`${BASE_URL}/${relationshipId}/interactions`);
  return response.data;
};

export const createRelationshipInteraction = async (
  request: CreateInteractionRequest
): Promise<RelationshipInteraction> => {
  const response = await apiClient.post(
    `${BASE_URL}/${request.accountRelationshipId}/interactions`,
    request
  );
  return response.data;
};

// Relationship Map Visualization
export const getRelationshipMap = async (
  customerId: number,
  depth: number = 2,
  includeInactive: boolean = false
): Promise<RelationshipMapVisualization> => {
  const response = await apiClient.get(
    `${BASE_URL}/map/${customerId}?depth=${depth}&includeInactive=${includeInactive}`
  );
  return response.data;
};

// Health Snapshots
export const getHealthSnapshots = async (
  customerId: number
): Promise<AccountHealthSnapshot[]> => {
  const response = await apiClient.get(`${BASE_URL}/health/${customerId}`);
  return response.data;
};

export const createHealthSnapshot = async (
  customerId: number
): Promise<AccountHealthSnapshot> => {
  const response = await apiClient.post(`${BASE_URL}/health/${customerId}`);
  return response.data;
};

// Default export as service object
const relationshipService = {
  // Relationship Types
  getRelationshipTypes,
  getRelationshipType,
  createRelationshipType,
  updateRelationshipType,
  deleteRelationshipType,
  // Account Relationships
  getAccountRelationships,
  getAccountRelationship,
  createAccountRelationship,
  updateAccountRelationship,
  deleteAccountRelationship,
  // Interactions
  getRelationshipInteractions,
  createRelationshipInteraction,
  // Map
  getRelationshipMap,
  // Health
  getHealthSnapshots,
  createHealthSnapshot,
};

export default relationshipService;
