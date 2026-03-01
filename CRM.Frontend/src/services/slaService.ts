// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// SLA Policy Service — CRUD base generated via createCrudService factory.
// Domain-specific methods (assignPolicy, getApplicable) are appended via
// object spread and preserved unchanged.
//
// MIGRATION NOTE: getAll now returns PaginatedResponse<SLAPolicyDto> (paginated)
// instead of the previous flat SLAPolicyDto[]. Callers should read .items to
// get the entity array.

import apiClient from './apiClient';
import { createCrudService } from './crudServiceFactory';

export interface SLAPolicyDto {
  id: number;
  name: string;
  description?: string;
  priority: string;
  responseTimeMinutes: number;
  resolutionTimeMinutes: number;
  escalationEnabled: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSLAPolicyDto {
  name: string;
  description?: string;
  priority: string;
  responseTimeMinutes: number;
  resolutionTimeMinutes: number;
  escalationEnabled?: boolean;
  isActive?: boolean;
}

export interface UpdateSLAPolicyDto extends Partial<CreateSLAPolicyDto> {}

export interface SLAInstanceDto {
  id: number;
  policyId: number;
  serviceRequestId: number;
  responseDeadline: string;
  resolutionDeadline: string;
  status: string;
}

// The five standard CRUD methods come from the factory.
// Domain-specific methods are appended via object spread.
const slaService = {
  ...createCrudService<SLAPolicyDto, CreateSLAPolicyDto, UpdateSLAPolicyDto>('/slapolicies'),

  assignPolicy: async (policyId: number, serviceRequestId: number): Promise<SLAInstanceDto> => {
    const response = await apiClient.post<SLAInstanceDto>(`/slapolicies/${policyId}/assign/${serviceRequestId}`);
    return response.data;
  },

  getApplicable: async (priority?: string, category?: string): Promise<SLAPolicyDto[]> => {
    const response = await apiClient.get<SLAPolicyDto[]>('/slapolicies/applicable', { params: { priority, category } });
    return response.data;
  },
};

export default slaService;
