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
  // Basic Information
  category?: number;
  firstName?: string;
  lastName?: string;
  company?: string;
  legalName?: string;
  email?: string;
  phone?: string;
  website?: string;
  
  // Location
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  
  // Business Information
  industry?: string;
  annualRevenue?: number;
  jobTitle?: string;
  customerType?: number; // Enum: 'enterprise', 'mid-market', 'small-business'
  accountType?: string;
  
  // Status & Lifecycle
  lifecycleStage?: number; // Enum: 'lead', 'customer', 'partner'
  priority?: number; // 1: Low, 2: Medium, 3: High, 4: Critical
  status?: string; // 'active', 'inactive', 'prospect'
  
  // Relationships
  parentAccountId?: number;
  ownerUserId?: number;
  ownerName?: string;
  parentAccountName?: string;
  
  // Custom Fields
  customField1?: string;
  customField2?: string;
  [key: string]: any; // Support for dynamic custom fields
}

/**
 * Create Account DTO
 * Payload for POST /api/accounts
 */
export interface CreateAccountDto {
  category?: number;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  company?: string;
  customerType?: number;
  lifecycleStage?: number;
  industry?: string;
  website?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  [key: string]: any;
}

/**
 * Update Account DTO
 * Payload for PUT/PATCH /api/accounts/{id}
 */
export interface UpdateAccountDto {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  company?: string;
  industry?: string;
  website?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  jobTitle?: string;
  lifecycleStage?: number;
  priority?: number;
  status?: string;
  [key: string]: any;
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
  contacts?: ContactSummary[];
  opportunities?: OpportunitySummary[];
  quotes?: QuoteSummary[];
  orders?: OrderSummary[];
  invoices?: InvoiceSummary[];
  interactions?: InteractionSummary[];
  notes?: NoteSummary[];
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
