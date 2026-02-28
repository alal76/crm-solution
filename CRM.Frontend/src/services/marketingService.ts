/**
 * Marketing Service - Campaigns, Emails, Sequences, Landing Pages
 * Fully typed service layer for all marketing operations
 */

import apiClient from './apiClient';
import {
  Campaign,
  CampaignRecipient,
  CampaignMetrics,
  EmailTemplate,
  EmailSequence,
  LandingPage,
  MarketingAutomationWorkflow,
  CreateCampaignDto,
  UpdateCampaignDto,
  CreateEmailTemplateDto,
  UpdateEmailTemplateDto,
  CreateEmailSequenceDto,
  UpdateEmailSequenceDto,
  // New DTO types
  EmailSequenceDto,
  CreateEmailSequenceSimpDto,
  UpdateEmailSequenceSimpDto,
  NurtureEnrollmentDto,
  CampaignExecutionStatusDto,
  CampaignTrackingLinkDto,
  CreateTrackingLinkDto,
  UnsubscribeStatusDto,
} from '../types/marketing';
import { PaginatedResponse } from '../types/common';

const marketingService = {
  // =========================================================================
  // CAMPAIGNS
  // =========================================================================

  /**
   * Get all campaigns with pagination
   */
  getCampaigns: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Campaign>>('/campaigns', {
      params: { page, pageSize }
    });
  },

  /**
   * Get campaign by ID
   */
  getCampaignById: async (id: number) => {
    return apiClient.get<Campaign>(`/campaigns/${id}`);
  },

  /**
   * Create campaign
   */
  createCampaign: async (data: CreateCampaignDto) => {
    return apiClient.post<Campaign>('/campaigns', data);
  },

  /**
   * Update campaign
   */
  updateCampaign: async (id: number, data: UpdateCampaignDto) => {
    return apiClient.patch<Campaign>(`/campaigns/${id}`, data);
  },

  /**
   * Get campaigns by status
   */
  getCampaignsByStatus: async (status: string) => {
    return apiClient.get<Campaign[]>(`/campaigns/status/${status}`);
  },

  /**
   * Launch/activate campaign
   */
  launchCampaign: async (campaignId: number) => {
    return apiClient.post(`/campaigns/${campaignId}/launch`, {});
  },

  /**
   * Pause campaign
   */
  pauseCampaign: async (campaignId: number) => {
    return apiClient.post(`/campaigns/${campaignId}/pause`, {});
  },

  /**
   * Resume campaign
   */
  resumeCampaign: async (campaignId: number) => {
    return apiClient.post(`/campaigns/${campaignId}/resume`, {});
  },

  /**
   * End campaign
   */
  endCampaign: async (campaignId: number) => {
    return apiClient.post(`/campaigns/${campaignId}/end`, {});
  },

  /**
   * Get campaign recipients
   */
  getCampaignRecipients: async (campaignId: number) => {
    return apiClient.get<CampaignRecipient[]>(`/campaigns/${campaignId}/recipients`);
  },

  /**
   * Get campaign metrics/analytics
   */
  getCampaignMetrics: async (campaignId: number) => {
    return apiClient.get<CampaignMetrics>(`/campaigns/${campaignId}/metrics`);
  },

  /**
   * Add recipients to campaign
   */
  addCampaignRecipients: async (campaignId: number, recipientIds: number[]) => {
    return apiClient.post(`/campaigns/${campaignId}/recipients`, { recipientIds });
  },

  /**
   * Remove recipient from campaign
   */
  removeCampaignRecipient: async (campaignId: number, recipientId: number) => {
    return apiClient.delete(`/campaigns/${campaignId}/recipients/${recipientId}`);
  },

  /**
   * Send test email
   */
  sendCampaignTest: async (campaignId: number, email: string) => {
    return apiClient.post(`/campaigns/${campaignId}/send-test`, { email });
  },

  /**
   * Clone campaign
   */
  cloneCampaign: async (campaignId: number) => {
    return apiClient.post<Campaign>(`/campaigns/${campaignId}/clone`, {});
  },

  // =========================================================================
  // EMAIL TEMPLATES
  // =========================================================================

  /**
   * Get all email templates
   */
  getEmailTemplates: async () => {
    return apiClient.get<EmailTemplate[]>('/email-templates');
  },

  /**
   * Get email template by ID
   */
  getEmailTemplateById: async (id: number) => {
    return apiClient.get<EmailTemplate>(`/email-templates/${id}`);
  },

  /**
   * Create email template
   */
  createEmailTemplate: async (data: CreateEmailTemplateDto) => {
    return apiClient.post<EmailTemplate>('/email-templates', data);
  },

  /**
   * Update email template
   */
  updateEmailTemplate: async (id: number, data: UpdateEmailTemplateDto) => {
    return apiClient.patch<EmailTemplate>(`/email-templates/${id}`, data);
  },

  /**
   * Delete email template
   */
  deleteEmailTemplate: async (id: number) => {
    return apiClient.delete(`/email-templates/${id}`);
  },

  /**
   * Get templates by category
   */
  getEmailTemplatesByCategory: async (category: string) => {
    return apiClient.get<EmailTemplate[]>(`/email-templates/category/${category}`);
  },

  /**
   * Render template preview with variables
   */
  renderTemplatePreview: async (templateId: number, variables: Record<string, string>) => {
    return apiClient.post(`/email-templates/${templateId}/preview`, { variables });
  },

  /**
   * Clone template
   */
  cloneEmailTemplate: async (templateId: number) => {
    return apiClient.post<EmailTemplate>(`/email-templates/${templateId}/clone`, {});
  },

  // =========================================================================
  // EMAIL SEQUENCES
  // =========================================================================

  /**
   * Get all email sequences
   */
  getEmailSequences: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<EmailSequence>>('/email-sequences', {
      params: { page, pageSize }
    });
  },

  /**
   * Get email sequence by ID
   */
  getEmailSequenceById: async (id: number) => {
    return apiClient.get<EmailSequence>(`/email-sequences/${id}`);
  },

  /**
   * Create email sequence
   */
  createEmailSequence: async (data: CreateEmailSequenceDto) => {
    return apiClient.post<EmailSequence>('/email-sequences', data);
  },

  /**
   * Update email sequence
   */
  updateEmailSequence: async (id: number, data: UpdateEmailSequenceDto) => {
    return apiClient.patch<EmailSequence>(`/email-sequences/${id}`, data);
  },

  /**
   * Delete email sequence
   */
  deleteEmailSequence: async (id: number) => {
    return apiClient.delete(`/email-sequences/${id}`);
  },

  /**
   * Activate email sequence
   */
  activateEmailSequence: async (sequenceId: number) => {
    return apiClient.post(`/email-sequences/${sequenceId}/activate`, {});
  },

  /**
   * Pause email sequence
   */
  pauseEmailSequence: async (sequenceId: number) => {
    return apiClient.post(`/email-sequences/${sequenceId}/pause`, {});
  },

  /**
   * Get email sequence analysis
   */
  getEmailSequenceAnalysis: async (sequenceId: number) => {
    return apiClient.get(`/email-sequences/${sequenceId}/analysis`);
  },

  /**
   * Test email sequence step
   */
  testEmailSequenceStep: async (
    sequenceId: number,
    stepIndex: number,
    testEmail: string
  ) => {
    return apiClient.post(`/email-sequences/${sequenceId}/test-step`, {
      stepIndex,
      testEmail
    });
  },

  /**
   * Clone email sequence
   */
  cloneEmailSequence: async (sequenceId: number) => {
    return apiClient.post<EmailSequence>(`/email-sequences/${sequenceId}/clone`, {});
  },

  // =========================================================================
  // LANDING PAGES
  // =========================================================================

  /**
   * Get all landing pages
   */
  getLandingPages: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<LandingPage>>('/landing-pages', {
      params: { page, pageSize }
    });
  },

  /**
   * Get landing page by ID
   */
  getLandingPageById: async (id: number) => {
    return apiClient.get<LandingPage>(`/landing-pages/${id}`);
  },

  /**
   * Create landing page
   */
  createLandingPage: async (data: Partial<LandingPage>) => {
    return apiClient.post<LandingPage>('/landing-pages', data);
  },

  /**
   * Update landing page
   */
  updateLandingPage: async (id: number, data: Partial<LandingPage>) => {
    return apiClient.patch<LandingPage>(`/landing-pages/${id}`, data);
  },

  /**
   * Delete landing page
   */
  deleteLandingPage: async (id: number) => {
    return apiClient.delete(`/landing-pages/${id}`);
  },

  /**
   * Publish landing page
   */
  publishLandingPage: async (pageId: number) => {
    return apiClient.post(`/landing-pages/${pageId}/publish`, {});
  },

  /**
   * Unpublish landing page
   */
  unpublishLandingPage: async (pageId: number) => {
    return apiClient.post(`/landing-pages/${pageId}/unpublish`, {});
  },

  /**
   * Get landing page analytics
   */
  getLandingPageAnalytics: async (pageId: number) => {
    return apiClient.get(`/landing-pages/${pageId}/analytics`);
  },

  /**
   * Get landing page conversions
   */
  getLandingPageConversions: async (pageId: number) => {
    return apiClient.get(`/landing-pages/${pageId}/conversions`);
  },

  // =========================================================================
  // MARKETING AUTOMATION
  // =========================================================================

  /**
   * Get all marketing automation workflows
   */
  getMarketingWorkflows: async () => {
    return apiClient.get<MarketingAutomationWorkflow[]>('/marketing-workflows');
  },

  /**
   * Get workflow by ID
   */
  getMarketingWorkflowById: async (id: number) => {
    return apiClient.get<MarketingAutomationWorkflow>(`/marketing-workflows/${id}`);
  },

  /**
   * Create marketing automation workflow
   */
  createMarketingWorkflow: async (data: Partial<MarketingAutomationWorkflow>) => {
    return apiClient.post<MarketingAutomationWorkflow>('/marketing-workflows', data);
  },

  /**
   * Update marketing workflow
   */
  updateMarketingWorkflow: async (id: number, data: Partial<MarketingAutomationWorkflow>) => {
    return apiClient.patch<MarketingAutomationWorkflow>(`/marketing-workflows/${id}`, data);
  },

  /**
   * Activate workflow
   */
  activateMarketingWorkflow: async (workflowId: number) => {
    return apiClient.post(`/marketing-workflows/${workflowId}/activate`, {});
  },

  /**
   * Deactivate workflow
   */
  deactivateMarketingWorkflow: async (workflowId: number) => {
    return apiClient.post(`/marketing-workflows/${workflowId}/deactivate`, {});
  },

  // =========================================================================
  // MARKETING METRICS
  // =========================================================================

  /**
   * Get overall marketing metrics
   */
  getMarketingMetrics: async () => {
    return apiClient.get('/marketing/metrics');
  },

  /**
   * Get campaign performance report
   */
  getCampaignPerformanceReport: async (startDate: string, endDate: string) => {
    return apiClient.get('/marketing/campaign-performance', {
      params: { startDate, endDate }
    });
  },

  /**
   * Get lead attribution report
   */
  getLeadAttributionReport: async (startDate: string, endDate: string) => {
    return apiClient.get('/marketing/lead-attribution', {
      params: { startDate, endDate }
    });
  },

  /**
   * Get ROI report
   */
  getROIReport: async (startDate: string, endDate: string) => {
    return apiClient.get('/marketing/roi', {
      params: { startDate, endDate }
    });
  },

  // =========================================================================
  // EMAIL SEQUENCES (DTO-based, MKT-002)
  // =========================================================================

  /** List email sequences as simple DTOs */
  getEmailSequenceDtos: async (): Promise<EmailSequenceDto[]> => {
    const res = await apiClient.get<EmailSequenceDto[]>('/email-sequences');
    return res.data;
  },

  /** Create a new email sequence */
  createEmailSequenceSimp: async (data: CreateEmailSequenceSimpDto): Promise<EmailSequenceDto> => {
    const res = await apiClient.post<EmailSequenceDto>('/email-sequences', data);
    return res.data;
  },

  /** Update an existing email sequence */
  updateEmailSequenceSimp: async (id: number, data: UpdateEmailSequenceSimpDto): Promise<EmailSequenceDto> => {
    const res = await apiClient.patch<EmailSequenceDto>(`/email-sequences/${id}`, data);
    return res.data;
  },

  /** Delete an email sequence */
  deleteEmailSequenceById: async (id: number): Promise<void> => {
    await apiClient.delete(`/email-sequences/${id}`);
  },

  /** Enroll leads into a sequence */
  enrollLeads: async (sequenceId: number, leadIds: number[]): Promise<NurtureEnrollmentDto[]> => {
    const res = await apiClient.post<NurtureEnrollmentDto[]>(`/email-sequences/${sequenceId}/enroll`, { leadIds });
    return res.data;
  },

  // =========================================================================
  // CAMPAIGN EXECUTION STATUS (MKT-003)
  // =========================================================================

  /** Get campaign execution status with metrics */
  getCampaignExecutionStatus: async (campaignId: number): Promise<CampaignExecutionStatusDto> => {
    const res = await apiClient.get<CampaignExecutionStatusDto>(`/campaigns/${campaignId}/execution/status`);
    return res.data;
  },

  /** Schedule a campaign */
  scheduleCampaign: async (campaignId: number, scheduledAt: string): Promise<CampaignExecutionStatusDto> => {
    const res = await apiClient.post<CampaignExecutionStatusDto>(`/campaigns/${campaignId}/schedule`, { scheduledAt });
    return res.data;
  },

  /** Start a campaign immediately */
  startCampaign: async (campaignId: number): Promise<CampaignExecutionStatusDto> => {
    const res = await apiClient.post<CampaignExecutionStatusDto>(`/campaigns/${campaignId}/start`, {});
    return res.data;
  },

  /** Pause a running campaign */
  pauseCampaignExecution: async (campaignId: number): Promise<CampaignExecutionStatusDto> => {
    const res = await apiClient.post<CampaignExecutionStatusDto>(`/campaigns/${campaignId}/pause`, {});
    return res.data;
  },

  /** Cancel a campaign */
  cancelCampaign: async (campaignId: number): Promise<CampaignExecutionStatusDto> => {
    const res = await apiClient.post<CampaignExecutionStatusDto>(`/campaigns/${campaignId}/cancel`, {});
    return res.data;
  },

  // =========================================================================
  // UTM TRACKING LINKS (MKT-003)
  // =========================================================================

  /** Get all tracking links for a campaign */
  getCampaignLinks: async (campaignId: number): Promise<CampaignTrackingLinkDto[]> => {
    const res = await apiClient.get<CampaignTrackingLinkDto[]>(`/campaigns/${campaignId}/links`);
    return res.data;
  },

  /** Create a new UTM tracking link */
  createTrackingLink: async (campaignId: number, data: CreateTrackingLinkDto): Promise<CampaignTrackingLinkDto> => {
    const res = await apiClient.post<CampaignTrackingLinkDto>(`/campaigns/${campaignId}/links`, data);
    return res.data;
  },

  // =========================================================================
  // UNSUBSCRIBE MANAGEMENT
  // =========================================================================

  /** Check unsubscribe status for an email */
  getUnsubscribeStatus: async (email: string): Promise<UnsubscribeStatusDto> => {
    const res = await apiClient.get<UnsubscribeStatusDto>('/unsubscribe/status', { params: { email } });
    return res.data;
  },

  // =========================================================================
  // RECIPIENT PREVIEW (MKT-007)
  // =========================================================================

  /** Preview recipient count for a segment (returns 0 if backend returns 404) */
  previewRecipients: async (campaignId: number, segmentJson: string): Promise<number> => {
    try {
      const res = await apiClient.post<{ count: number }>(`/campaigns/${campaignId}/preview-recipients`, { segmentRulesJson: segmentJson });
      return res.data?.count ?? 0;
    } catch {
      return 0;
    }
  },
};

export default marketingService;
