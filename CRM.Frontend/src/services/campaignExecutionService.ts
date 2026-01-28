/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Campaign Execution Service
 * Handles campaign workflow execution, recipient tracking, A/B testing, and analytics
 */

import apiClient from './apiClient';

// ============================================================================
// Enums
// ============================================================================

export enum RecipientStatus {
  Pending = 'Pending',
  Sent = 'Sent',
  Delivered = 'Delivered',
  Opened = 'Opened',
  Clicked = 'Clicked',
  Converted = 'Converted',
  Bounced = 'Bounced',
  Unsubscribed = 'Unsubscribed',
  Failed = 'Failed'
}

export enum CampaignWorkflowType {
  PreLaunch = 'PreLaunch',
  Execution = 'Execution',
  FollowUp = 'FollowUp',
  Nurture = 'Nurture',
  Reengagement = 'Reengagement'
}

export enum ABTestType {
  SubjectLine = 'SubjectLine',
  Content = 'Content',
  SendTime = 'SendTime',
  FromName = 'FromName',
  CallToAction = 'CallToAction'
}

export enum ABTestStatus {
  Draft = 'Draft',
  Running = 'Running',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum ConversionType {
  Purchase = 'Purchase',
  SignUp = 'SignUp',
  Download = 'Download',
  FormSubmission = 'FormSubmission',
  PageVisit = 'PageVisit',
  Custom = 'Custom'
}

export enum AttributionModel {
  FirstTouch = 'FirstTouch',
  LastTouch = 'LastTouch',
  Linear = 'Linear',
  TimeDecay = 'TimeDecay',
  PositionBased = 'PositionBased'
}

// ============================================================================
// Interfaces
// ============================================================================

export interface CampaignWorkflow {
  id: number;
  campaignId: number;
  campaignName?: string;
  workflowDefinitionId: number;
  workflowDefinitionName?: string;
  workflowType: CampaignWorkflowType;
  triggerEvent: string;
  isActive: boolean;
  priority: number;
  createdAt: string;
  updatedAt: string;
}

export interface CampaignRecipient {
  id: number;
  campaignId: number;
  contactId?: number;
  contactName?: string;
  email: string;
  status: RecipientStatus;
  abTestVariant?: string;
  deliveredAt?: string;
  firstOpenedAt?: string;
  lastOpenedAt?: string;
  openCount: number;
  firstClickedAt?: string;
  lastClickedAt?: string;
  clickCount: number;
  convertedAt?: string;
  bouncedAt?: string;
  bounceReason?: string;
  unsubscribedAt?: string;
  failedAt?: string;
  failureReason?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CampaignLinkClick {
  id: number;
  recipientId: number;
  linkUrl: string;
  linkName?: string;
  clickedAt: string;
  ipAddress?: string;
  userAgent?: string;
  deviceType?: string;
  country?: string;
  city?: string;
}

export interface CampaignABTest {
  id: number;
  campaignId: number;
  testName: string;
  testType: ABTestType;
  status: ABTestStatus;
  variantA: string;
  variantB: string;
  variantC?: string;
  trafficSplit: TrafficSplit;
  testMetric: string;
  sampleSize?: number;
  testStartedAt?: string;
  testCompletedAt?: string;
  winningVariant?: string;
  variantAMetric?: number;
  variantBMetric?: number;
  variantCMetric?: number;
  statisticalSignificance?: number;
  autoWinnerAfterHours?: number;
  createdAt: string;
  updatedAt: string;
}

export interface TrafficSplit {
  variantA: number;
  variantB: number;
  variantC?: number;
}

export interface CampaignConversion {
  id: number;
  campaignId: number;
  recipientId?: number;
  contactId?: number;
  contactName?: string;
  conversionType: ConversionType;
  conversionValue?: number;
  currency?: string;
  convertedAt: string;
  attributionModel: AttributionModel;
  attributionWeight: number;
  sourceChannel?: string;
  landingPage?: string;
  conversionData?: string;
  createdAt: string;
}

export interface CampaignAnalytics {
  campaignId: number;
  campaignName: string;
  totalRecipients: number;
  delivered: number;
  deliveryRate: number;
  opened: number;
  openRate: number;
  clicked: number;
  clickRate: number;
  converted: number;
  conversionRate: number;
  bounced: number;
  bounceRate: number;
  unsubscribed: number;
  unsubscribeRate: number;
  totalRevenue: number;
  averageOrderValue: number;
  roi: number;
  topLinks: LinkAnalytics[];
  abTestResults?: ABTestResults;
}

export interface LinkAnalytics {
  linkUrl: string;
  linkName?: string;
  clicks: number;
  uniqueClicks: number;
}

export interface ABTestResults {
  testId: number;
  testName: string;
  testType: ABTestType;
  status: ABTestStatus;
  winningVariant?: string;
  variants: VariantResults[];
}

export interface VariantResults {
  variant: string;
  recipients: number;
  opens: number;
  openRate: number;
  clicks: number;
  clickRate: number;
  conversions: number;
  conversionRate: number;
  revenue: number;
}

// Request interfaces
export interface LinkCampaignWorkflowRequest {
  campaignId: number;
  workflowDefinitionId: number;
  workflowType: CampaignWorkflowType;
  triggerEvent: string;
  priority?: number;
}

export interface RecordOpenRequest {
  recipientId: number;
  ipAddress?: string;
  userAgent?: string;
}

export interface RecordClickRequest {
  recipientId: number;
  linkUrl: string;
  linkName?: string;
  ipAddress?: string;
  userAgent?: string;
}

export interface RecordConversionRequest {
  campaignId: number;
  recipientId?: number;
  contactId?: number;
  conversionType: ConversionType;
  conversionValue?: number;
  currency?: string;
  attributionModel?: AttributionModel;
  sourceChannel?: string;
  landingPage?: string;
  conversionData?: Record<string, unknown>;
}

export interface CreateABTestRequest {
  campaignId: number;
  testName: string;
  testType: ABTestType;
  variantA: string;
  variantB: string;
  variantC?: string;
  trafficSplit: TrafficSplit;
  testMetric: string;
  sampleSize?: number;
  autoWinnerAfterHours?: number;
}

// ============================================================================
// API Methods
// ============================================================================

const BASE_URL = '/campaign-execution';

// Campaign Workflows
export const getCampaignWorkflows = async (
  campaignId: number
): Promise<CampaignWorkflow[]> => {
  const response = await apiClient.get(`${BASE_URL}/${campaignId}/workflows`);
  return response.data;
};

export const linkCampaignWorkflow = async (
  request: LinkCampaignWorkflowRequest
): Promise<CampaignWorkflow> => {
  const response = await apiClient.post(
    `${BASE_URL}/${request.campaignId}/workflows`,
    request
  );
  return response.data;
};

export const executeCampaignWorkflow = async (
  campaignId: number,
  workflowId: number
): Promise<{ workflowInstanceId: number }> => {
  const response = await apiClient.post(
    `${BASE_URL}/${campaignId}/workflows/${workflowId}/execute`
  );
  return response.data;
};

export const toggleWorkflowActive = async (
  campaignId: number,
  workflowId: number,
  isActive: boolean
): Promise<CampaignWorkflow> => {
  const response = await apiClient.patch(
    `${BASE_URL}/${campaignId}/workflows/${workflowId}`,
    { isActive }
  );
  return response.data;
};

// Recipients
export const getCampaignRecipients = async (
  campaignId: number,
  status?: RecipientStatus
): Promise<CampaignRecipient[]> => {
  const url = status
    ? `${BASE_URL}/${campaignId}/recipients?status=${status}`
    : `${BASE_URL}/${campaignId}/recipients`;
  const response = await apiClient.get(url);
  return response.data;
};

export const addCampaignRecipients = async (
  campaignId: number,
  emails: string[]
): Promise<{ added: number; duplicates: number }> => {
  const response = await apiClient.post(
    `${BASE_URL}/${campaignId}/recipients`,
    { emails }
  );
  return response.data;
};

export const recordEmailOpen = async (
  campaignId: number,
  request: RecordOpenRequest
): Promise<void> => {
  await apiClient.post(`${BASE_URL}/${campaignId}/opens`, request);
};

export const recordLinkClick = async (
  campaignId: number,
  request: RecordClickRequest
): Promise<void> => {
  await apiClient.post(`${BASE_URL}/${campaignId}/clicks`, request);
};

// Conversions
export const getCampaignConversions = async (
  campaignId: number
): Promise<CampaignConversion[]> => {
  const response = await apiClient.get(`${BASE_URL}/${campaignId}/conversions`);
  return response.data;
};

export const recordConversion = async (
  request: RecordConversionRequest
): Promise<CampaignConversion> => {
  const response = await apiClient.post(
    `${BASE_URL}/${request.campaignId}/conversions`,
    request
  );
  return response.data;
};

// A/B Testing
export const getCampaignABTests = async (
  campaignId: number
): Promise<CampaignABTest[]> => {
  const response = await apiClient.get(`${BASE_URL}/${campaignId}/ab-tests`);
  return response.data;
};

export const createABTest = async (
  request: CreateABTestRequest
): Promise<CampaignABTest> => {
  const response = await apiClient.post(
    `${BASE_URL}/${request.campaignId}/ab-tests`,
    request
  );
  return response.data;
};

export const startABTest = async (
  campaignId: number,
  testId: number
): Promise<CampaignABTest> => {
  const response = await apiClient.post(
    `${BASE_URL}/${campaignId}/ab-tests/${testId}/start`
  );
  return response.data;
};

export const completeABTest = async (
  campaignId: number,
  testId: number,
  winningVariant: string
): Promise<CampaignABTest> => {
  const response = await apiClient.post(
    `${BASE_URL}/${campaignId}/ab-tests/${testId}/complete`,
    { winningVariant }
  );
  return response.data;
};

// Analytics
export const getCampaignAnalytics = async (
  campaignId: number
): Promise<CampaignAnalytics> => {
  const response = await apiClient.get(`${BASE_URL}/${campaignId}/analytics`);
  return response.data;
};

// Default export as service object
const campaignExecutionService = {
  // Workflows
  getCampaignWorkflows,
  linkCampaignWorkflow,
  executeCampaignWorkflow,
  toggleWorkflowActive,
  // Recipients
  getCampaignRecipients,
  addCampaignRecipients,
  recordEmailOpen,
  recordLinkClick,
  // Conversions
  getCampaignConversions,
  recordConversion,
  // A/B Testing
  getCampaignABTests,
  createABTest,
  startABTest,
  completeABTest,
  // Analytics
  getCampaignAnalytics,
};

export default campaignExecutionService;
