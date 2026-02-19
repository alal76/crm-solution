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
}

export interface UpdateLeadDto {
  status?: LeadStatus;
  rating?: number;
  ownerId?: number;
  priority?: string;
  notes?: string;
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
}

export interface UpdateOpportunityDto {
  stage?: OpportunityStage;
  amount?: number;
  probability?: number;
  expectedCloseDate?: string;
  reason?: string;
  nextStep?: string;
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
