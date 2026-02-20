/**
 * Core CRM Types
 * Contacts, Leads, Opportunities, Products, Activities
 */

import { BaseEntity } from './common';

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

export enum LeadStatus {
  New = 'new',
  Contacted = 'contacted',
  Qualified = 'qualified',
  Unqualified = 'unqualified',
  Disqualified = 'disqualified',
  Converted = 'converted'
}

export enum LeadSource {
  Website = 'website',
  DirectMail = 'direct_mail',
  Email = 'email',
  Phone = 'phone',
  Referral = 'referral',
  Trade = 'trade',
  Social = 'social',
  Advertisement = 'advertisement',
  Other = 'other'
}

export interface Lead extends BaseEntity {
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  companyName: string;
  jobTitle?: string;
  status: LeadStatus;
  source?: LeadSource;
  rating?: number; // 1-5
  industry?: string;
  employees?: number;
  annualRevenue?: number;
  website?: string;
  leadScore?: number; // ML based lead scoring
  ownerId?: number;
  ownerName?: string;
  priority?: 'high' | 'medium' | 'low';
  notes?: string;
  convertedAccountId?: number;
  convertedOpportunityId?: number;
  convertedDate?: string;
  fitScore?: number;          // ML fit score from backend
  engagementScore?: number;   // ML engagement score from backend
  qualificationNotes?: string; // SDR qualification notes
  region?: string;            // Sales territory/region
  campaignId?: number;        // Source campaign
  accountId?: number;         // Matched account (after conversion)
  contactId?: number;         // Matched contact (after conversion)
  mqlDate?: string;           // Date qualified as Marketing Qualified Lead
  sqlDate?: string;           // Date qualified as Sales Qualified Lead
  lastActivityDate?: string;  // Last activity date
  tags?: string;              // Comma-separated tags
}

export interface CreateLeadDto {
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  companyName: string;
  jobTitle?: string;
  source?: LeadSource;
  industry?: string;
  region?: string;
  campaignId?: number;
  qualificationNotes?: string;
}

export interface UpdateLeadDto {
  status?: LeadStatus;
  rating?: number;
  ownerId?: number;
  priority?: string;
  notes?: string;
  fitScore?: number;
  engagementScore?: number;
  qualificationNotes?: string;
  region?: string;
  campaignId?: number;
}

export interface LeadConversionResult {
  accountId: number;
  contactId: number;
  opportunityId?: number;
  conversionDate: string;
}

// ============================================================================
// OPPORTUNITIES
// ============================================================================

export enum OpportunityStage {
  Prospecting = 'prospecting',
  Qualification = 'qualification',
  Proposal = 'proposal',
  Negotiation = 'negotiation',
  Won = 'won',
  Lost = 'lost'
}

export interface Opportunity extends BaseEntity {
  name: string;
  accountId: number;
  accountName?: string;
  primaryContactId?: number;
  primaryContactName?: string;
  ownerId?: number;
  ownerName?: string;
  stage: OpportunityStage;
  amount: number;
  probability?: number; // 0-100%
  expectedCloseDate: string;
  actualCloseDate?: string;
  reason?: string; // Reason for won/lost
  nextStep?: string;
  description?: string;
  products?: OpportunityProduct[];
  competitors?: string[];
  lossReason?: string;
  leadSource?: string;
  type?: 'new_business' | 'expansion' | 'renewal' | 'replacement';
  currency?: string;           // ISO currency code (default USD)
  pricingModel?: number;       // OpportunityPricingModel: 0=Subscription, 1=OneTime, 2=UsageBased, 3=Hybrid
  termLengthMonths?: number;   // Contract term length (1-120)
  solutionNotes?: string;      // Proposed solution description
  qualificationReason?: number; // BANT: 0=Budget, 1=Need, 2=Timing, 3=Authority, 4=Fit
  qualificationNotes?: string; // Qualification handoff notes
  region?: string;             // Sales territory/region
  leadId?: number;             // Source lead FK
  salesOwnerId?: number;       // Account executive (backend field name)
  salesOwnerName?: string;     // Account executive display name
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
  expectedCloseDate: string;
  stage?: OpportunityStage;
  currency?: string;
  pricingModel?: number;
  termLengthMonths?: number;
  salesOwnerId?: number;
}

export interface UpdateOpportunityDto {
  stage?: OpportunityStage;
  amount?: number;
  probability?: number;
  expectedCloseDate?: string;
  reason?: string;
  nextStep?: string;
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

export interface Activity extends BaseEntity {
  type: ActivityType;
  subject: string;
  description?: string;
  entityType: 'Account' | 'Contact' | 'Lead' | 'Opportunity';
  entityId: number;
  activityDate: string;
  dueDate?: string;
  status: 'open' | 'completed' | 'cancelled';
  priority?: 'low' | 'normal' | 'high';
  ownerId?: number;
  ownerName?: string;
  callDuration?: number; // Minutes
  direction?: 'inbound' | 'outbound';
  participants?: string[];
  location?: string;
  attachments?: string[];
  title?: string;              // Activity title (alias: subject)
  userId?: number;             // User who performed activity (alias: ownerId)
  userName?: string;           // Display name of user
  entityName?: string;         // Display name of related entity
  accountId?: number;          // Related account
  contactId?: number;          // Related contact
  opportunityId?: number;      // Related opportunity
  campaignId?: number;         // Related campaign
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
  type: ActivityType;
  subject: string;
  description?: string;
  entityType: 'Account' | 'Contact' | 'Lead' | 'Opportunity';
  entityId: number;
  activityDate: string;
  dueDate?: string;
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
  subject: string;             // Backend field name (frontend may alias as 'title')
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
  subject: string;
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
  subject?: string;
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
