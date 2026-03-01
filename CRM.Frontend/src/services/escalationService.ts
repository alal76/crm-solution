// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Escalation Rule Service — CRUD base generated via createCrudService factory.
// Domain-specific methods (testRule, getApplicable) are appended via
// object spread and preserved unchanged.
//
// MIGRATION NOTE: getAll now returns PaginatedResponse<EscalationRuleDto>
// (paginated) instead of the previous flat EscalationRuleDto[].

import apiClient from './apiClient';
import { createCrudService } from './crudServiceFactory';

export interface EscalationRuleDto {
  id: number;
  name: string;
  description?: string;
  priority: string;
  conditionType: string;
  conditionValue: string;
  targetType: string;
  targetId?: number;
  targetName?: string;
  escalationDelayMinutes: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEscalationRuleDto {
  name: string;
  description?: string;
  priority: string;
  conditionType: string;
  conditionValue: string;
  targetType: string;
  targetId?: number;
  targetName?: string;
  escalationDelayMinutes: number;
  isActive?: boolean;
}

export interface UpdateEscalationRuleDto extends Partial<CreateEscalationRuleDto> {}

export interface EscalationRuleTestResultDto {
  ruleId: number;
  serviceRequestId: number;
  wouldApply: boolean;
  reason: string;
  matchedConditions: string[];
}

// The five standard CRUD methods come from the factory.
// Domain-specific methods are appended via object spread.
const escalationService = {
  ...createCrudService<EscalationRuleDto, CreateEscalationRuleDto, UpdateEscalationRuleDto>('/escalationrules'),

  testRule: async (ruleId: number, serviceRequestId: number): Promise<EscalationRuleTestResultDto> => {
    const response = await apiClient.post<EscalationRuleTestResultDto>(`/escalationrules/${ruleId}/test/${serviceRequestId}`);
    return response.data;
  },

  getApplicable: async (priority: string): Promise<EscalationRuleDto[]> => {
    const response = await apiClient.get<EscalationRuleDto[]>(`/escalationrules/applicable?priority=${priority}`);
    return response.data;
  },
};

export default escalationService;
