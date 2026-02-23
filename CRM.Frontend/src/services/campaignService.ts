/**
 * Campaign Service - Convenience re-export of marketing campaign operations
 * Wraps marketingService with focused campaign + template API surface
 */

import apiClient from './apiClient';
import {
  Campaign,
  CampaignMetrics,
  CampaignRecipient,
  EmailTemplate,
  CreateCampaignDto,
  UpdateCampaignDto,
  CreateEmailTemplateDto,
  UpdateEmailTemplateDto,
} from '../types/marketing';
import { PaginatedResponse } from '../types/common';

const campaignService = {
  // =========================================================================
  // Campaigns CRUD
  // =========================================================================

  getAll: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Campaign>>('/campaigns', {
      params: { page, pageSize },
    });
  },

  getById: async (id: number) => {
    return apiClient.get<Campaign>(`/campaigns/${id}`);
  },

  create: async (data: CreateCampaignDto) => {
    return apiClient.post<Campaign>('/campaigns', data);
  },

  update: async (id: number, data: UpdateCampaignDto) => {
    return apiClient.patch<Campaign>(`/campaigns/${id}`, data);
  },

  delete: async (id: number) => {
    return apiClient.delete(`/campaigns/${id}`);
  },

  // =========================================================================
  // Campaign Actions
  // =========================================================================

  getMetrics: async (campaignId: number) => {
    return apiClient.get<CampaignMetrics>(`/campaigns/${campaignId}/metrics`);
  },

  getAudience: async (campaignId: number) => {
    return apiClient.get<CampaignRecipient[]>(`/campaigns/${campaignId}/recipients`);
  },

  sendTest: async (campaignId: number, email: string) => {
    return apiClient.post(`/campaigns/${campaignId}/send-test`, { email });
  },

  // =========================================================================
  // Email Templates
  // =========================================================================

  getTemplates: async () => {
    return apiClient.get<EmailTemplate[]>('/email-templates');
  },

  getTemplateById: async (id: number) => {
    return apiClient.get<EmailTemplate>(`/email-templates/${id}`);
  },

  createTemplate: async (data: CreateEmailTemplateDto) => {
    return apiClient.post<EmailTemplate>('/email-templates', data);
  },

  updateTemplate: async (id: number, data: UpdateEmailTemplateDto) => {
    return apiClient.patch<EmailTemplate>(`/email-templates/${id}`, data);
  },

  deleteTemplate: async (id: number) => {
    return apiClient.delete(`/email-templates/${id}`);
  },
};

export default campaignService;
