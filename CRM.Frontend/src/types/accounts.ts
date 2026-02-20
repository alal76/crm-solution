/**
 * Account/Customer Types
 * Industry-standard terminology for Customer management (Accounts)
 * 
 * The API uses /api/accounts and /api/customers interchangeably
 * These types support all account-related operations
 */

import { BaseEntity } from './common';

/**
 * Account (Customer) entity
 * Represents an organization or company in the CRM
 */
export interface Account extends BaseEntity {
  // Core
  id: number;
  category: string; // "Individual" | "Organization"
  isOrganization?: boolean;

  // Individual fields
  firstName?: string;
  lastName?: string;
  salutation?: string;
  suffix?: string;
  dateOfBirth?: string; // ISO string
  gender?: string;
  linkedContactId?: number;
  linkedContactName?: string;

  // Organization fields
  company?: string;
  legalName?: string;
  dbaName?: string;
  taxId?: string;
  registrationNumber?: string;
  yearFounded?: number;
  primaryContactId?: number;
  primaryContactName?: string;

  // Contact Information
  email?: string;
  secondaryEmail?: string;
  phone?: string;
  mobilePhone?: string;
  faxNumber?: string;
  jobTitle?: string;
  website?: string;

  // Address - Billing
  address?: string;
  address2?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;

  // Address - Shipping
  shippingAddress?: string;
  shippingAddress2?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingZipCode?: string;
  shippingCountry?: string;
  shippingSameAsBilling?: boolean;

  // Business Information
  industry?: string;
  subIndustry?: string;
  numberOfEmployees?: number;
  employeeRange?: string;
  annualRevenue?: number;
  revenueRange?: string;
  accountType?: string;
  priority?: string;
  stockSymbol?: string;
  ownership?: string;

  // Lifecycle & Status
  lifecycleStage?: string;
  leadSource?: string;
  firstContactDate?: string;
  conversionDate?: string;
  lastActivityDate?: string;
  nextFollowUpDate?: string;

  // Financial
  totalPurchases?: number;
  accountBalance?: number;
  creditLimit?: number;
  paymentTerms?: string;
  preferredPaymentMethod?: string;
  currency?: string;
  billingCycle?: string;

  // Scoring
  leadScore?: number;
  accountHealthScore?: number;
  npsScore?: number;
  satisfactionRating?: number;

  // Social & Communication
  linkedInUrl?: string;
  twitterHandle?: string;
  facebookUrl?: string;
  optInEmail?: boolean;
  optInSms?: boolean;
  optInPhone?: boolean;
  preferredContactMethod?: string;
  preferredContactTime?: string;
  timezone?: string;
  preferredLanguage?: string;

  // Assignment
  assignedToUserId?: number;
  assignedToUserName?: string;
  accountManagerId?: number;
  accountManagerName?: string;
  territory?: string;
  region?: string;

  // Classification
  tags?: string;
  segment?: string;
  referralSource?: string;
  referredByAccountId?: number;
  referredByAccountName?: string;
  parentAccountId?: number;
  parentAccountName?: string;

  // Documentation
  notes?: string;
  internalNotes?: string;
  description?: string;
  customFields?: string | Record<string, any>;

  // Metadata
  rowVersion?: string;

  // Display
  displayName?: string;

  // Linked contacts (for organizations)
  contacts?: AccountContact[];
  contactCount?: number;

  // Normalized Contact Info Collections
  emailAddresses?: LinkedEmail[];
  phoneNumbers?: LinkedPhone[];
  addresses?: LinkedAddress[];
  socialMediaAccounts?: LinkedSocialMedia[];

  // Soft delete
  isDeleted?: boolean;

  [key: string]: any; // Support for dynamic custom fields
}

export interface AccountContact {
  id: number;
  accountId: number;
  contactId: number;
  contactName: string;
  contactEmail?: string;
  contactPhone?: string;
  role: string;
  isPrimaryContact: boolean;
  isDecisionMaker: boolean;
  receivesBillingNotifications: boolean;
  receivesMarketingEmails: boolean;
  receivesTechnicalUpdates: boolean;
}

export interface LinkedEmail {
  id: number;
  email: string;
  type?: string;
  isPrimary?: boolean;
}

export interface LinkedPhone {
  id: number;
  phone: string;
  type?: string;
  isPrimary?: boolean;
}

export interface LinkedAddress {
  id: number;
  address: string;
  address2?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  type?: string;
  isPrimary?: boolean;
}

export interface LinkedSocialMedia {
  id: number;
  platform: string;
  handle?: string;
  url?: string;
}

/**
 * Account search result
 */
export interface AccountSearchResult {
  id: number;
  company?: string;
  email?: string;
  phone?: string;
  city?: string;
  highlight?: string; // Matched field for highlighting
}

/**
 * Account statistics
 */
export interface AccountStatistics {
  totalAccounts: number;
  activeAccounts: number;
  inactiveAccounts: number;
  leadsCount: number;
  customersCount: number;
  partnersCount: number;
}

/**
 * Account with related entities (detail view)
 */
export interface AccountDetail extends Account {
  contactSummaries?: ContactSummary[];
  opportunities?: OpportunitySummary[];
  quotes?: QuoteSummary[];
  orders?: OrderSummary[];
  invoices?: InvoiceSummary[];
  interactions?: InteractionSummary[];
  noteSummaries?: NoteSummary[];
  contractsCount?: number;
  subscriptionsCount?: number;
}

// Related entity summaries for account detail view
export interface ContactSummary {
  id: number;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  jobTitle?: string;
  isPrimary?: boolean;
}

export interface OpportunitySummary {
  id: number;
  name?: string;
  amount?: number;
  stage?: string;
  probability?: number;
  closeDate?: string;
}

export interface QuoteSummary {
  id: number;
  number?: string;
  total?: number;
  status?: string;
  expiryDate?: string;
}

export interface OrderSummary {
  id: number;
  number?: string;
  total?: number;
  status?: string;
  orderDate?: string;
}

export interface InvoiceSummary {
  id: number;
  number?: string;
  amount?: number;
  dueDate?: string;
  status?: string;
}

export interface InteractionSummary {
  id: number;
  type?: string;
  title?: string;
  date?: string;
}

export interface NoteSummary {
  id: number;
  content?: string;
  createdBy?: string;
  createdAt?: string;
}

/**
 * Account bulk operation response
 */
export interface AccountBulkOperationResult {
  successCount: number;
  failureCount: number;
  errors: Array<{
    accountId: number;
    error: string;
  }>;
}

/**
 * Account export format
 */
export interface AccountExport {
  id: number;
  company?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  industry?: string;
  annualRevenue?: number;
  lifecycleStage?: string;
  status?: string;
  createdAt?: string;
  updatedAt?: string;
}
