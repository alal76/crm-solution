// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Escalation Policy Service — generated via createCrudService factory.
// All CRUD operations are provided by the factory; see
// src/services/crudServiceFactory.ts for implementation details.
//
// MIGRATION NOTE: getAll now returns PaginatedResponse<EscalationPolicyDto>
// (paginated, with .items / .totalCount etc.) instead of the previous flat
// EscalationPolicyDto[]. Callers should read .items to get the entity array.

import { createCrudService } from './crudServiceFactory';

// ── Types ───────────────────────────────────────────────────────────────────────────

export interface EscalationLevelDto {
  id?: number;
  levelNumber: number;
  name: string;
  timeThresholdMinutes: number;
  notifyRoles: string[];
  notifyUserIds: number[];
  actions: string[];
}

export interface EscalationPolicyDto {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  levels: EscalationLevelDto[];
  triggerConditions?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEscalationPolicyDto {
  name: string;
  description?: string;
  isActive?: boolean;
  levels: EscalationLevelDto[];
  triggerConditions?: string;
}

// ── Service ───────────────────────────────────────────────────────────────────────────

const escalationPolicyService = createCrudService<
  EscalationPolicyDto,
  CreateEscalationPolicyDto,
  Partial<CreateEscalationPolicyDto>
>('/escalationpolicies');

export default escalationPolicyService;
