/**
 * Lead Service - API service for lead management
 */

import apiClient from './apiClient';
import { PaginatedResponse } from '../types/common';

export interface Lead {
  id: number;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  companyName?: string;
  status?: string;
  source?: string;
  score?: number;
  ownerId?: number;
  createdAt?: string;
  updatedAt?: string;
}

const leadService = {
  getAll: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Lead>>('/leads', {
      params: { page, pageSize },
    });
  },

  getById: async (id: number) => {
    return apiClient.get<Lead>(`/leads/${id}`);
  },

  create: async (data: Partial<Lead>) => {
    return apiClient.post<Lead>('/leads', data);
  },

  update: async (id: number, data: Partial<Lead>) => {
    return apiClient.patch<Lead>(`/leads/${id}`, data);
  },

  delete: async (id: number) => {
    return apiClient.delete(`/leads/${id}`);
  },
};

export { leadService };
export default leadService;
