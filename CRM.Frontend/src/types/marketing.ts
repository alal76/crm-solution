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

// Numeric enum mapping for API contract (backend CampaignType int)
export enum CampaignTypeEnum {
  Email = 0,
  SMS = 1,
  Social = 2,
  WebPush = 3,
  InApp = 4,
  Direct = 5,
  Phone = 6,
  Other = 7
}

// Numeric enum mapping for API contract (backend CampaignStatus int)
export enum CampaignStatusEnum {
  Planning = 0,
  Scheduled = 1,
  Active = 2,
  Paused = 3,
  Completed = 4,
  Archived = 5,
  Cancelled = 6
}

// Numeric enum for Campaign objective (backend Objective int)
export enum CampaignObjectiveEnum {
  BrandAwareness = 0,
  LeadGeneration = 1,
  CustomerRetention = 2,
  Upsell = 3,
  ReEngagement = 4,
  Other = 5
}

// Helper: Map numeric API status to string enum
export function campaignStatusFromApi(val: number): CampaignStatus {
  switch (val) {
    case CampaignStatusEnum.Planning: return CampaignStatus.Planning;
    case CampaignStatusEnum.Scheduled: return CampaignStatus.Scheduled;
    case CampaignStatusEnum.Active: return CampaignStatus.Active;
    case CampaignStatusEnum.Paused: return CampaignStatus.Paused;
    case CampaignStatusEnum.Completed: return CampaignStatus.Completed;
    case CampaignStatusEnum.Archived: return CampaignStatus.Archived;
    case CampaignStatusEnum.Cancelled: return CampaignStatus.Cancelled;
    default: return CampaignStatus.Planning;
  }
}

// Helper: Map string enum to numeric API status
export function campaignStatusToApi(val: CampaignStatus): number {
  switch (val) {
    case CampaignStatus.Planning: return CampaignStatusEnum.Planning;
    case CampaignStatus.Scheduled: return CampaignStatusEnum.Scheduled;
    case CampaignStatus.Active: return CampaignStatusEnum.Active;
    case CampaignStatus.Paused: return CampaignStatusEnum.Paused;
    case CampaignStatus.Completed: return CampaignStatusEnum.Completed;
    case CampaignStatus.Archived: return CampaignStatusEnum.Archived;
    case CampaignStatus.Cancelled: return CampaignStatusEnum.Cancelled;
    default: return CampaignStatusEnum.Planning;
  }
}

// Helper: Map numeric API type to string channel enum
export function campaignTypeFromApi(val: number): CampaignChannel {
  switch (val) {
    case CampaignTypeEnum.Email: return CampaignChannel.Email;
    case CampaignTypeEnum.SMS: return CampaignChannel.SMS;
    case CampaignTypeEnum.Social: return CampaignChannel.Social;
    case CampaignTypeEnum.WebPush: return CampaignChannel.WebPush;
    case CampaignTypeEnum.InApp: return CampaignChannel.InApp;
    case CampaignTypeEnum.Direct: return CampaignChannel.Direct;
    case CampaignTypeEnum.Phone: return CampaignChannel.Phone;
    default: return CampaignChannel.Other;
  }
}

// Helper: Map string channel enum to numeric API type
export function campaignTypeToApi(val: CampaignChannel): number {
  switch (val) {
    case CampaignChannel.Email: return CampaignTypeEnum.Email;
    case CampaignChannel.SMS: return CampaignTypeEnum.SMS;
    case CampaignChannel.Social: return CampaignTypeEnum.Social;
    case CampaignChannel.WebPush: return CampaignTypeEnum.WebPush;
    case CampaignChannel.InApp: return CampaignTypeEnum.InApp;
    case CampaignChannel.Direct: return CampaignTypeEnum.Direct;
    case CampaignChannel.Phone: return CampaignTypeEnum.Phone;
    default: return CampaignTypeEnum.Other;
  }
}

export interface Campaign extends BaseEntity {
  name: string;
  description?: string;
  status: CampaignStatus;
  statusValue?: number;        // Numeric API value (use CampaignStatusEnum)
  channel: CampaignChannel;
  campaignType?: number;       // Numeric API value (use CampaignTypeEnum)
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
  objectiveValue?: number;     // Numeric API value (use CampaignObjectiveEnum)
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

  // Scheduling & Meta
  type?: string;
  primarySuccessMetric?: number;
  theme?: string;
  valueProposition?: string;
  durationDays?: number;
  isEvergreen?: boolean;
  schedule?: string;

  // Targeting
  targetAudienceDescription?: string;
  targetDemographics?: string;
  targetFirmographics?: string;
  targetGeography?: string;
  targetIndustries?: string;
  targetSegments?: string;
  targetPersonas?: string;
  targetJobTitles?: string;
  targetSeniorityLevels?: string;
  targetAccounts?: string;
  exclusionCriteria?: string;
  suppressionLists?: string;
  audienceListId?: string;

  // Revenue
  actualRevenue?: number;
  actualCost?: number;
  pipelineInfluenced?: number;
  pipelineCreated?: number;

  // Cost Metrics
  costPerMql?: number;
  costPerSql?: number;
  costPerOpportunity?: number;

  // Conversion Funnel
  salsGenerated?: number;
  opportunitiesInfluenced?: number;
  leadToMqlRate?: number;
  mqlToSqlRate?: number;
  sqlToOpportunityRate?: number;
  opportunityToWinRate?: number;
  averageLeadScore?: number;
  leadQualityDistribution?: string;

  // Digital/Email Metrics
  frequency?: number;
  formConversionRate?: number;
  contentDownloads?: number;
  videoViews?: number;
  videoCompletionRate?: number;
  demoRequests?: number;
  trialSignups?: number;
  emailClickRate?: number;
  clickToOpenRate?: number;
  hardBounces?: number;
  softBounces?: number;
  unsubscribes?: number;
  unsubscribeRate?: number;
  spamComplaints?: number;
  complaintRate?: number;
  emailForwards?: number;
  listGrowth?: number;

  // Social Metrics
  socialEngagementRate?: number;
  socialComments?: number;
  socialLikes?: number;
  socialSaves?: number;
  newFollowers?: number;
  profileVisits?: number;
  mentions?: number;
  sentimentScore?: number;

  // Paid Advertising
  adSpend?: number;
  costPerClick?: number;
  costPerMille?: number;
  roas?: number;
  qualityScore?: number;
  averagePosition?: number;
  impressionShare?: number;

  // Event/Webinar
  registrations?: number;
  attendanceRate?: number;
  onDemandViews?: number;
  pollResponses?: number;
  questionsAsked?: number;
  eventSatisfactionScore?: number;
  webinarPlatform?: string;
  webinarRecordingUrl?: string;

  // A/B Testing
  winningVariant?: string;
  statisticalSignificance?: number;
  abTestResults?: string;

  // Goal Tracking
  targetConversions?: number;
  goalAchievementPercent?: number;
  campaignHealthScore?: number;
  benchmarkComparison?: string;

  // Content
  messageSubject?: string;
  preheaderText?: string;
  messageBody?: string;
  fromName?: string;
  fromEmail?: string;
  replyToEmail?: string;
  callToAction?: string;
  ctaUrl?: string;
  trackingUrl?: string;
  creativeAssets?: string;

  // Admin
  approvedByUserId?: number;
  approvedDate?: string;
  relatedCampaigns?: string;
  initiative?: string;
  teamMembers?: string;
  tags?: string;
  notes?: string;
  briefUrl?: string;
  reportUrl?: string;
  customFields?: string;

  // Integration channels
  channels?: string;
  platforms?: string;
  socialNetworks?: string;
  adPlatforms?: string;
  externalCampaignIds?: string;

  // Keywords
  keywords?: string;
  negativeKeywords?: string;
}

// Alias: MarketingCampaign is the same shape as Campaign
export type MarketingCampaign = Campaign;

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
  channel?: CampaignChannel;   // UI string enum (convert with campaignTypeToApi() before sending)
  campaignType?: number;       // Numeric API value sent to backend (use CampaignTypeEnum)
  objectiveValue?: number;     // Numeric objective sent to backend (use CampaignObjectiveEnum)
  priority?: number;
  startDate: string;
  endDate?: string;
  budget?: number;
  targetAudience?: string;
  segmentCriteria?: string;
  tags?: string;
  ownerId?: number;
  templateId?: number;
}

export interface UpdateCampaignDto {
  status?: CampaignStatus;
  statusValue?: number;        // Numeric API value (use campaignStatusToApi())
  objectiveValue?: number;
  endDate?: string;
  budget?: number;             // Fixed: was incorrectly typed as string
  name?: string;
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
