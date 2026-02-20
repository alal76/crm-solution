/**
 * Marketing Module Types
 * Campaigns, Email Templates, Email Sequences, Landing Pages
 */

import { BaseEntity } from './common';

// ============================================================================
// CAMPAIGNS
// ============================================================================

export enum CampaignStatus {
  Planning = 'planning',
  Scheduled = 'scheduled',
  Active = 'active',
  Paused = 'paused',
  Completed = 'completed',
  Archived = 'archived',
  Cancelled = 'cancelled'
}

export enum CampaignChannel {
  Email = 'email',
  SMS = 'sms',
  Social = 'social',
  WebPush = 'web_push',
  InApp = 'in_app',
  Direct = 'direct',
  Phone = 'phone',
  Other = 'other'
}

export interface Campaign extends BaseEntity {
  name: string;
  description?: string;
  status: CampaignStatus;
  channel: CampaignChannel;
  startDate: string;
  endDate?: string;
  budget?: number;
  budgetSpent?: number;
  targetAudience?: string;
  goals?: string;
  templateId?: number;
  recipients?: CampaignRecipient[];
  metrics?: CampaignMetrics;
  conversions?: number;
  conversionValue?: number;

  // Identity
  campaignCode?: string;
  objective?: string;
  objectiveType?: string;
  priority?: number;

  // Extended dates
  actualStartDate?: string;
  actualEndDate?: string;
  timezone?: string;

  // Budget details
  dailyBudget?: number;
  monthlyBudget?: number;
  expectedRevenue?: number;
  costPerLead?: number;
  costPerAcquisition?: number;
  currencyCode?: string;

  // Lead generation metrics (display-only)
  mqlsGenerated?: number;
  sqlsGenerated?: number;
  opportunitiesCreated?: number;
  dealsWon?: number;
  accountsAcquired?: number;

  // Engagement metrics
  impressions?: number;
  reach?: number;
  clicks?: number;
  clickThroughRate?: number;
  landingPageVisits?: number;
  formSubmissions?: number;

  // Email metrics
  emailsDelivered?: number;
  deliveryRate?: number;
  bounceRate?: number;

  // UTM tracking
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;
  utmContent?: string;
  utmTerm?: string;

  // A/B testing
  isAbTest?: boolean;
  abTestVariants?: string;    // JSON

  // Assignment & hierarchy
  assignedToUserId?: number;
  parentCampaignId?: number;
  program?: string;
  fiscalQuarter?: string;
  fiscalYear?: number;
  region?: string;
  department?: string;

  // Classification
  category?: string;
  subCategory?: string;

  // Documentation
  internalNotes?: string;
  successCriteria?: string;
  lessonsLearned?: string;
  attachments?: string;

  // Integration
  externalCampaignId?: string;
  syncStatus?: string;
}

export interface CampaignRecipient {
  id?: number;
  campaignId?: number;
  contactId?: number;
  email?: string;
  status: 'pending' | 'sent' | 'bounced' | 'opened' | 'clicked' | 'converted' | 'unsubscribed';
  sentDate?: string;
  openedDate?: string;
  clickedDate?: string;
}

export interface CampaignMetrics {
  totalRecipients: number;
  sentCount: number;
  deliveredCount: number;
  bouncedCount: number;
  openedCount: number;
  openRate: number; // Percentage
  clickedCount: number;
  clickRate: number; // Percentage
  conversions: number;
  conversionRate: number; // Percentage
}

export interface CreateCampaignDto {
  name: string;
  description?: string;
  channel: CampaignChannel;
  startDate: string;
  endDate?: string;
  budget?: number;
  targetAudience?: string;
  templateId?: number;
}

export interface UpdateCampaignDto {
  status?: CampaignStatus;
  endDate?: string;
  budget?: string;
}

// ============================================================================
// EMAIL TEMPLATES
// ============================================================================

export interface EmailTemplate extends BaseEntity {
  name: string;
  description?: string;
  subject: string;
  htmlContent: string;
  textContent?: string;
  variables?: string[]; // {{firstName}}, {{email}}, etc.
  category?: string;
  isDefault?: boolean;
  previewUrl?: string;
  tags?: string[];
  language?: string; // en, es, fr, de, etc.
}

export interface CreateEmailTemplateDto {
  name: string;
  subject: string;
  htmlContent: string;
  textContent?: string;
  category?: string;
}

export interface UpdateEmailTemplateDto {
  name?: string;
  subject?: string;
  htmlContent?: string;
  textContent?: string;
}

// ============================================================================
// EMAIL SEQUENCES
// ============================================================================

export enum SequenceStepType {
  Email = 'email',
  Delay = 'delay',
  Condition = 'condition',
  Action = 'action'
}

export enum ConditionOperator {
  Equals = 'equals',
  NotEquals = 'not_equals',
  GreaterThan = 'greater_than',
  LessThan = 'less_than',
  Contains = 'contains',
  NotContains = 'not_contains',
  In = 'in',
  NotIn = 'not_in'
}

export interface EmailSequence extends BaseEntity {
  name: string;
  description?: string;
  status: 'draft' | 'active' | 'paused' | 'archived';
  steps: SequenceStep[];
  triggerType: 'manual' | 'automatic' | 'event_based';
  triggerEvent?: string; // lead_created, opportunity_created, etc.
  recipientFilter?: RecipientFilter;
  sendTime?: string; // HH:mm format for send time optimization
  timezone?: string;
  unsubscribeLink?: boolean;
  preheader?: string;
}

export interface SequenceStep {
  id?: string;
  sequence: number;
  type: SequenceStepType;
  emailTemplateId?: number;
  delayDays?: number;
  delayHours?: number;
  condition?: SequenceCondition;
  name?: string;
  description?: string;
}

export interface SequenceCondition {
  id?: string;
  field: string; // Contact field like 'status', 'customField1', etc.
  operator: ConditionOperator;
  value: string | number | boolean | (string | number)[];
  logicalOperator?: 'AND' | 'OR'; // For multiple conditions
  nextStepOnTrue?: number;
  nextStepOnFalse?: number;
}

export interface RecipientFilter {
  id?: string;
  filterType: 'all' | 'segment' | 'dynamic';
  segmentId?: number;
  conditions?: FilterCondition[];
  dynamicQuery?: string;
}

export interface FilterCondition {
  id?: string;
  field: string;
  operator: ConditionOperator;
  value: string | number | boolean | (string | number)[];
}

export interface CreateEmailSequenceDto {
  name: string;
  description?: string;
  steps: SequenceStep[];
  triggerType: 'manual' | 'automatic' | 'event_based';
  triggerEvent?: string;
  recipientFilter?: RecipientFilter;
}

export interface UpdateEmailSequenceDto {
  status?: 'draft' | 'active' | 'paused' | 'archived';
  steps?: SequenceStep[];
  recipientFilter?: RecipientFilter;
}

// ============================================================================
// LANDING PAGES
// ============================================================================

export interface LandingPage extends BaseEntity {
  name: string;
  url: string;
  status: 'draft' | 'published' | 'archived';
  headline: string;
  subheading?: string;
  content: string;
  image?: string;
  ctaButtonText?: string;
  ctaButtonUrl?: string;
  formElements?: FormElement[];
  campaigns?: number[]; // Related campaign IDs
  views?: number;
  conversions?: number;
}

export interface FormElement {
  id?: string;
  type: 'text' | 'email' | 'phone' | 'checkbox' | 'radio' | 'select' | 'textarea';
  label: string;
  name: string;
  required: boolean;
  options?: string[]; // For select, radio
  placeholder?: string;
}

// ============================================================================
// MARKETING AUTOMATION
// ============================================================================

export interface MarketingAutomationWorkflow extends BaseEntity {
  name: string;
  description?: string;
  status: 'draft' | 'active' | 'paused' | 'archived';
  trigger: AutomationTrigger;
  actions: AutomationAction[];
  conditions?: AutomationCondition[];
}

export interface AutomationTrigger {
  type: 'event' | 'time_based' | 'manual';
  event?: string; // lead_created, opportunity_won, etc.
  segment?: string;
  timeBasedRule?: string;
}

export interface AutomationAction {
  id?: string;
  type: 'send_email' | 'assign_lead' | 'create_task' | 'add_to_segment' | 'update_field' | 'webhook';
  templateId?: number;
  assignTo?: number;
  taskDescription?: string;
  segmentId?: number;
  fieldName?: string;
  fieldValue?: string;
  webhookUrl?: string;
  sequence?: number;
}

export interface AutomationCondition {
  id?: string;
  field: string;
  operator: ConditionOperator;
  value: string | number | boolean;
  sequence?: number;
}

// ============================================================================
// MARKETING METRICS
// ============================================================================

export interface MarketingMetrics {
  totalCampaigns: number;
  activeCampaigns: number;
  totalLeads: number;
  leadsThisMonth: number;
  conversionRate: number; // Percentage
  campaignROI: number; // Percentage
  emailOpenRate: number; // Percentage
  emailClickRate: number; // Percentage
  averageDealSize: number;
  pipelineValue: number;
}

export interface CampaignAnalytics {
  campaignId: number;
  campaignName: string;
  totalRecipients: number;
  deliverRate: number;
  openRate: number;
  clickRate: number;
  conversionRate: number;
  revenue: number;
  roi: number;
  cost: number;
}
