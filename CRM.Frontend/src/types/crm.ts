/**
 * Core CRM Types
 * Contacts, Leads, Opportunities, Products, Activities
 */

import { BaseEntity } from './common';

// ============================================================================
// CONTACT LINKED SUB-TYPES
// ============================================================================

export interface LinkedEmailDto {
  id?: number;
  email?: string;
  type?: string;
  isPrimary?: boolean;
}

export interface LinkedPhoneDto {
  id?: number;
  phone?: string;
  type?: string;
  isPrimary?: boolean;
}

export interface LinkedAddressDto {
  id?: number;
  street?: string;
  city?: string;
  state?: string;
  country?: string;
  zipCode?: string;
  type?: string;
}

export interface LinkedSocialMediaDto {
  id?: number;
  platform?: string;
  url?: string;
  handle?: string;
}

export interface SocialMediaLinkDto {
  id?: number;
  platform?: string;
  url?: string;
}

// ============================================================================
// CONTACTS
// ============================================================================

export interface Contact extends BaseEntity {
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  mobile?: string;
  fax?: string;
  jobTitle?: string;
  department?: string;
  reportingTo?: number; // Manager contact ID
  accountId?: number;
  accountName?: string;
  contactType?: 'Employee' | 'Partner' | 'Lead' | 'Customer' | 'Vendor' | 'Influencer' | 'Investor' | 'Media' | 'Other';
  status?: 'active' | 'inactive' | 'pending' | 'blocked' | 'archived';
  leadStatus?: string;
  emailPrimary?: string;
  phonePrimary?: string;
  emailSecondary?: string;
  phoneSecondary?: string;
  prefix?: string;
  middleName?: string;
  suffix?: string;
  birthDate?: string;
  website?: string;
  linkedInProfile?: string;
  twitterHandle?: string;
  preferredContactMethod?: 'email' | 'phone' | 'sms' | 'linkedin';
  doNotContact?: boolean;
  doNotEmail?: boolean;
  doNotPhone?: boolean;
  addresses?: ContactAddress[];
  notes?: string;

  // Scalar fields from DTO
  dateAdded?: string;
  lastModified?: string;
  modifiedBy?: string;

  // Collection fields
  emailAddresses?: LinkedEmailDto[];
  phoneNumbers?: LinkedPhoneDto[];
  socialMediaAccounts?: LinkedSocialMediaDto[];
  socialMediaLinks?: SocialMediaLinkDto[];
}

export interface ContactAddress {
  id?: number;
  type: 'home' | 'work' | 'billing' | 'shipping' | 'other';
  street1: string;
  street2?: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  isPrimary?: boolean;
}

export interface CreateContactDto {
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  jobTitle?: string;
  accountId?: number;
  contactType?: string;
  status?: string;
}

export interface UpdateContactDto {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  jobTitle?: string;
  contactType?: string;
  status?: string;
}

// ============================================================================
// LEADS
// ============================================================================
// REM-ORPHAN-003: Aligned with the real `Lead` entity/DTOs (CRM.Core.Entities.Lead,
// CRM.Core.Dtos.LeadDtos), not the legacy "Contacts-as-Leads" flow. Enum string
// values below mirror the backend enum member names exactly (C# `Enum.ToString()`
// on read, case-sensitive `Enum.TryParse<T>` on write) — see
// CRM.Backend/src/CRM.Core/Entities/Lead.cs for the source of truth.

export enum LeadStatus {
  New = 'New',
  Working = 'Working',
  Nurturing = 'Nurturing',
  Qualified = 'Qualified',
  Disqualified = 'Disqualified',
  Converted = 'Converted'
}

export enum LeadSource {
  Web = 'Web',
  Campaign = 'Campaign',
  Referral = 'Referral',
  Event = 'Event',
  Partner = 'Partner',
  Manual = 'Manual'
}

/** Qualification framework type (TODO-CRM002-08) — mirrors CRM.Core.Entities.QualificationFramework. */
export enum QualificationFrameworkType {
  None = 'None',
  BANT = 'BANT',
  MEDDIC = 'MEDDIC',
  MEDDPICC = 'MEDDPICC',
  CHAMP = 'CHAMP',
  GPCTBA = 'GPCTBA',
  Custom = 'Custom'
}

export interface Lead extends BaseEntity {
  firstName: string;
  lastName: string;
  fullName?: string;
  email: string;
  phone?: string;
  companyName?: string;
  title?: string;              // Job title (backend field name is `Title`, not `jobTitle`)
  status: LeadStatus;
  statusId?: number;           // Configurable status FK (ENUM-MIG-001)
  source?: LeadSource;
  website?: string;
  score?: number;              // Combined lead score (fit + engagement)
  fitScore?: number;
  engagementScore?: number;
  ownerId?: number;
  ownerName?: string;
  qualificationNotes?: string; // SDR/Marketing qualification notes
  region?: string;             // Sales territory/region
  campaignId?: number;         // Source marketing campaign
  accountId?: number;          // Matched account (after conversion)
  contactId?: number;          // Matched contact (if applicable)
  mqlDate?: string;            // Date qualified as Marketing Qualified Lead
  sqlDate?: string;            // Date qualified as Sales Qualified Lead
  lastActivityDate?: string;
  tags?: string;               // Comma-separated tags (JSON array as string)
  territoryId?: number;        // Assigned territory (TODO-GAP-04)
  qualificationFrameworkType?: QualificationFrameworkType;
  nurtureCampaignId?: number;  // Active nurture campaign enrollment (TODO-CRM002-06)
  nurtureCampaignEnrolledAt?: string;
  lastContactedAt?: string;
  daysSinceLastContact?: number;

  // Source attribution (TODO-CRM002-03)
  leadSourceId?: number;
  originalSource?: string;
  firstTouchDate?: string;
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;

  // BANT qualification scoring (TODO-CRM002-08)
  budgetScore?: number;
  authorityScore?: number;
  needScore?: number;
  timelineScore?: number;

  // MEDDIC qualification scoring (TODO-CRM002-08)
  metricsScore?: number;
  economicBuyerScore?: number;
  decisionCriteriaScore?: number;
  decisionProcessScore?: number;
  identifyPainScore?: number;
  championScore?: number;
  customQualificationJson?: string;
  lastScoreDecayDate?: string;
}

/**
 * POST /api/leads body. NOTE: the backend request DTO (declared inline in
 * LeadsController.cs) does not currently accept tags/mqlDate/sqlDate/statusId
 * for write — those are read-only via this endpoint today.
 */
export interface CreateLeadDto {
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  company?: string;
  companyName?: string;
  title?: string;
  source?: LeadSource | string;
  region?: string;
  website?: string;
  notes?: string;              // Maps to Lead.QualificationNotes server-side
  description?: string;
  ownerId?: number;
  campaignId?: number;
}

/**
 * PUT /api/leads/{id} body. Same write-path limitation as CreateLeadDto above —
 * tags/mqlDate/sqlDate are not accepted.
 */
export interface UpdateLeadDto {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  companyName?: string;
  title?: string;
  status?: LeadStatus | string;
  source?: LeadSource | string;
  region?: string;
  website?: string;
  notes?: string;               // Maps to Lead.QualificationNotes server-side
  score?: number;
  ownerId?: number;
  campaignId?: number;
}

/** Result of POST /api/leads/{id}/convert. The backend creates an Opportunity only
 *  — it does NOT create a new Account, so callers must supply an existing accountId. */
export interface LeadConversionResult {
  message: string;
  opportunityId: number;
  leadId: number;
}

// ============================================================================
// OPPORTUNITIES
// ============================================================================

export enum OpportunityStage {
  Discovery = 0,
  Qualification = 1,
  Proposal = 2,
  Negotiation = 3,
  Won = 4,
  Lost = 5
}

export interface Opportunity extends BaseEntity {
  name: string;
  accountId: number;
  accountName?: string;
  primaryContactId?: number;
  primaryContactName?: string;
  stage: number;               // OpportunityStage numeric value
  stageName?: string;          // Human-readable stage label (populated by API)
  amount: number;
  probability: number;         // 0-100%
  expectedCloseDate?: string;
  products?: OpportunityProduct[];
  currency?: string;           // ISO currency code (default USD)
  pricingModel?: number;       // OpportunityPricingModel: 0=Subscription, 1=OneTime, 2=UsageBased, 3=Hybrid
  pricingModelName?: string;   // Human-readable pricing model label (populated by API)
  termLengthMonths?: number;   // Contract term length (1-120)
  solutionNotes?: string;      // Proposed solution description
  qualificationReason?: number; // BANT: 0=Budget, 1=Need, 2=Timing, 3=Authority, 4=Fit
  qualificationNotes?: string; // Qualification handoff notes
  region?: string;             // Sales territory/region
  leadId?: number;             // Source lead FK
  salesOwnerId?: number;       // Account executive (backend field name)
  salesOwnerName?: string;     // Account executive display name
  weightedAmount?: number;     // Amount * Probability / 100 (computed by backend)
  weightedValue?: number;      // Alias for weightedAmount
  isOpen?: boolean;            // True when stage is not ClosedWon or ClosedLost
  isWon?: boolean;             // True when stage is ClosedWon
  forecastCategory?: number;   // ForecastCategory: 0=Pipeline, 1=BestCase, 2=Commit, 3=Closed/ClosedWon, 4=Omitted, 5=MostLikely
  lossReasonCategory?: number; // LossReasonCategory: 0=None, 1=Price, 2=Features, 3=Competition, 4=NoDecision, 5=Budget, 6=Timing, 7=Relationship, 99=Other
  lossReason?: string;         // Detailed loss reason description
  competitorWinnerId?: number; // FK to Competitor who won (if lost to competition)
  winLossNotes?: string;       // Win/Loss analysis notes
  closedDate?: string;         // Date when deal was won or lost
}

export interface OpportunityProduct {
  id?: number;
  opportunityId?: number;
  productId: number;
  productName?: string;
  quantity: number;
  unitPrice?: number;
  lineTotal?: number;
  sequence?: number;
}

export interface CreateOpportunityDto {
  name: string;
  accountId: number;
  primaryContactId?: number;
  ownerId?: number;
  amount: number;
  expectedCloseDate?: string;
  stage?: number;              // OpportunityStage numeric value
  currency?: string;
  pricingModel?: number;
  termLengthMonths?: number;
  salesOwnerId?: number;
}

export interface UpdateOpportunityDto {
  stage?: number;              // OpportunityStage numeric value
  amount?: number;
  probability?: number;
  expectedCloseDate?: string;
  currency?: string;
  pricingModel?: number;
  termLengthMonths?: number;
  solutionNotes?: string;
  qualificationReason?: number;
  qualificationNotes?: string;
  region?: string;
}

// ============================================================================
// PRODUCTS
// ============================================================================

export interface Product extends BaseEntity {
  code?: string;
  name: string;
  description?: string;
  category?: string;
  subcategory?: string;
  unitPrice: number;
  standardCost?: number;
  family?: string;
  isActive?: boolean;
  productImage?: string;
  manufacturer?: string;
  sku?: string;
  specifications?: Record<string, any>;
  warranty?: number; // Months
  supportLevel?: 'basic' | 'standard' | 'premium';
  relatedProducts?: number[];
  taxRate?: number;
}

export interface CreateProductDto {
  name: string;
  description?: string;
  category?: string;
  unitPrice: number;
  standardCost?: number;
  sku?: string;
}

export interface UpdateProductDto {
  name?: string;
  description?: string;
  unitPrice?: number;
  category?: string;
}

// ============================================================================
// SALES ACTIVITIES
// ============================================================================

// Numeric enum mapping for API contract
export enum ActivityTypeEnum {
  Call = 0,
  Email = 1,
  Meeting = 2,
  Task = 3,
  Note = 4,
  Social = 5,
  Campaign = 6,
  Other = 7
}

// String enum for UI
export enum ActivityType {
  Call = 'call',
  Email = 'email',
  Meeting = 'meeting',
  Task = 'task',
  Note = 'note',
  Social = 'social',
  Campaign = 'campaign',
  Other = 'other'
}

// Helper: Map numeric API value to string enum
export function activityTypeFromApi(val: number): ActivityType {
  switch (val) {
    case ActivityTypeEnum.Call: return ActivityType.Call;
    case ActivityTypeEnum.Email: return ActivityType.Email;
    case ActivityTypeEnum.Meeting: return ActivityType.Meeting;
    case ActivityTypeEnum.Task: return ActivityType.Task;
    case ActivityTypeEnum.Note: return ActivityType.Note;
    case ActivityTypeEnum.Social: return ActivityType.Social;
    case ActivityTypeEnum.Campaign: return ActivityType.Campaign;
    case ActivityTypeEnum.Other: return ActivityType.Other;
    default: return ActivityType.Other;
  }
}

// Helper: Map string enum to numeric API value
export function activityTypeToApi(val: ActivityType): number {
  switch (val) {
    case ActivityType.Call: return ActivityTypeEnum.Call;
    case ActivityType.Email: return ActivityTypeEnum.Email;
    case ActivityType.Meeting: return ActivityTypeEnum.Meeting;
    case ActivityType.Task: return ActivityTypeEnum.Task;
    case ActivityType.Note: return ActivityTypeEnum.Note;
    case ActivityType.Social: return ActivityTypeEnum.Social;
    case ActivityType.Campaign: return ActivityTypeEnum.Campaign;
    case ActivityType.Other: return ActivityTypeEnum.Other;
    default: return ActivityTypeEnum.Other;
  }
}

export interface Activity extends BaseEntity {
  type: ActivityType;
  activityType?: number;       // Numeric API contract value (use ActivityTypeEnum)
  subject: string;
  title?: string;              // Activity title (alias: subject)
  description?: string;
  details?: string;            // Extended description / body text
  entityType: 'Account' | 'Contact' | 'Lead' | 'Opportunity';
  entityId: number;
  activityDate: string;
  dueDate?: string;
  status: 'open' | 'completed' | 'cancelled';
  priority?: 'low' | 'normal' | 'high';
  durationMinutes?: number;    // Duration in minutes
  ownerId?: number;
  ownerName?: string;
  userId?: number;             // User who performed activity (alias: ownerId)
  userName?: string;           // Display name of user
  entityName?: string;         // Display name of related entity
  callDuration?: number;       // Minutes (alias: durationMinutes for calls)
  direction?: 'inbound' | 'outbound';
  participants?: string[];
  location?: string;
  attachments?: string[];
  accountId?: number;          // Related account
  contactId?: number;          // Related contact
  opportunityId?: number;      // Related opportunity
  campaignId?: number;         // Related campaign
  secondaryEntityType?: string; // Secondary related entity type
  secondaryEntityId?: number;   // Secondary related entity ID
  secondaryEntityName?: string; // Secondary related entity display name
  productId?: number;          // Related product
  taskId?: number;             // Related CRM task
  quoteId?: number;            // Related quote
  interactionId?: number;      // Related interaction
  noteId?: number;             // Related note
  isSystem?: boolean;          // System-generated activity
  isPrivate?: boolean;         // Private/internal activity
  isImportant?: boolean;       // Flagged as important
  tags?: string;               // Comma-separated tags
  category?: string;           // Activity category
  source?: string;             // Activity source (API, Web, Mobile, Import)
  oldValue?: string;           // Previous value (for update activities)
  newValue?: string;           // New value (for update activities)
  fieldsChanged?: string[];    // Which fields changed (for update activities)
}

export interface CreateActivityDto {
  // Legacy UI fields (kept for backward compatibility with existing forms)
  type?: ActivityType;
  subject?: string;
  description?: string;
  entityType?: 'Account' | 'Contact' | 'Lead' | 'Opportunity';
  entityId?: number;
  // Backend API fields
  activityType?: number;       // Numeric ActivityType (use ActivityTypeEnum)
  title?: string;              // Activity title (maps to backend Title)
  details?: string;            // Extended description
  activityDate: string;
  dueDate?: string;
  durationMinutes?: number;
  userId?: number;
  accountId?: number;
  contactId?: number;
  opportunityId?: number;
  isSystem?: boolean;
  isPrivate?: boolean;
  isImportant?: boolean;
  tags?: string;
  source?: string;
}

// ============================================================================
// TASKS
// ============================================================================

export enum TaskStatus {
  NotStarted = 0,
  InProgress = 1,
  Completed = 2,
  Deferred = 3,
  WaitingOnOthers = 4,
  Cancelled = 5
}

export enum TaskPriority {
  Low = 0,
  Normal = 1,
  High = 2,
  Critical = 3
}

export interface CrmTask extends BaseEntity {
  // Backend field: Title (alias: subject)
  title: string;
  description?: string;
  taskType: number;            // Enum: 0=Task, 1=Email, 2=Call, 3=Meeting, 4=Reminder, 5=Other
  status: number;              // TaskStatus enum
  priority: number;            // TaskPriority enum
  dueDate?: string;
  startDate?: string;
  completedDate?: string;
  reminderDate?: string;
  hasReminder?: boolean;
  percentComplete: number;     // 0-100
  estimatedMinutes?: number;   // Backend uses minutes
  actualMinutes?: number;      // Backend uses minutes
  isRecurring?: boolean;
  recurrencePattern?: string;  // JSON recurrence config
  recurrenceEndDate?: string;
  parentTaskId?: number;
  accountId?: number;
  contactId?: number;
  opportunityId?: number;
  campaignId?: number;
  assignedToUserId?: number;
  assignedToGroupId?: number;
  createdByUserId?: number;
  tags?: string;
  category?: string;
  attachments?: string;        // JSON array of attachment paths
  customFields?: string;       // JSON custom fields
}

export interface CreateCrmTaskDto {
  title: string;               // Maps to backend DTO Title (entity: Subject)
  description?: string;
  taskType: number;
  priority?: number;
  dueDate?: string;
  startDate?: string;
  reminderDate?: string;
  estimatedMinutes?: number;
  accountId?: number;
  contactId?: number;
  opportunityId?: number;
  assignedToUserId?: number;
  tags?: string;
}

export interface UpdateCrmTaskDto {
  title?: string;              // Maps to backend DTO Title (entity: Subject)
  description?: string;
  status?: number;
  priority?: number;
  dueDate?: string;
  startDate?: string;
  completedDate?: string;
  percentComplete?: number;
  estimatedMinutes?: number;
  actualMinutes?: number;
  isRecurring?: boolean;
  recurrencePattern?: string;
  recurrenceEndDate?: string;
  tags?: string;
  category?: string;
}

// ============================================================================
// SALES PIPELINE
// ============================================================================

export interface SalesPipeline {
  totalOpportunities: number;
  totalValue: number;
  opportunitiesByStage: Record<OpportunityStage, number>;
  valueByStage: Record<OpportunityStage, number>;
  conversionRates: Record<string, number>;
  averageDealSize: number;
  salesCycle: number; // Days
}

export interface SalesMetrics {
  totalRevenue: number;
  quotaAttainment: number; // Percentage
  pipelineValue: number;
  winRate: number; // Percentage
  averageDealSize: number;
  conversionsThisMonth: number;
  conversionsThisYear: number;
}
