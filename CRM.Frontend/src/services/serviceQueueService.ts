// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Service Queue Service — CRUD base generated via createCrudService factory.
// Domain-specific methods (assignToQueue, getQueueItems, getQueueStats) are
// appended via object spread and preserved unchanged.
//
// MIGRATION NOTE: getAll now returns PaginatedResponse<ServiceQueueDto>
// (paginated) instead of the previous flat ServiceQueueDto[].

import apiClient from './apiClient';
import { createCrudService } from './crudServiceFactory';

export interface ServiceQueueDto {
  id: number;
  name: string;
  description?: string;
  priority: number;
  isActive: boolean;
  assignmentGroup?: string;
  defaultSLAPolicyId?: number;
  maxQueueDepth?: number;
  routingConfiguration?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateServiceQueueDto {
  name: string;
  description?: string;
  priority?: number;
  isActive?: boolean;
  assignmentGroup?: string;
  defaultSLAPolicyId?: number;
  maxQueueDepth?: number;
  routingConfiguration?: string;
}

export interface UpdateServiceQueueDto extends Partial<CreateServiceQueueDto> {}

export interface ServiceRequestQueueItemDto {
  id: number;
  title: string;
  priority: string;
  status: string;
  assignedTo?: string;
  createdAt: string;
}

// The five standard CRUD methods come from the factory.
// Domain-specific methods are appended via object spread.
const serviceQueueService = {
  ...createCrudService<ServiceQueueDto, CreateServiceQueueDto, UpdateServiceQueueDto>('/servicequeues'),

  assignToQueue: async (serviceRequestId: number, queueId: number): Promise<void> => {
    await apiClient.post(`/servicequeues/${queueId}/assign/${serviceRequestId}`);
  },

  getQueueItems: async (queueId: number): Promise<ServiceRequestQueueItemDto[]> => {
    const response = await apiClient.get<ServiceRequestQueueItemDto[]>(`/servicequeues/${queueId}/items`);
    return response.data;
  },

  getQueueStats: async (queueId: number): Promise<ServiceQueueDto> => {
    const response = await apiClient.get<ServiceQueueDto>(`/servicequeues/${queueId}/stats`);
    return response.data;
  },
};

export default serviceQueueService;
