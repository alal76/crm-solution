/**
 * Account Service - Industry-standard terminology for Customer management
 * 
 * This service provides a wrapper around the accounts/customers API using
 * the industry-standard "accounts" terminology (Salesforce, HubSpot, Dynamics).
 * 
 * The /api/accounts endpoint routes to the same controller as /api/customers,
 * so these are functionally equivalent.
 */

import apiClient from './apiClient';
import {
  Account,
  AccountContact,
  LinkedEmail,
  LinkedPhone,
  LinkedAddress,
  LinkedSocialMedia
} from '../types/accounts';

export interface CreateAccountDto {
  category: string;
  firstName?: string;
  lastName?: string;
  salutation?: string;
  suffix?: string;
  dateOfBirth?: string;
  gender?: string;
  linkedContactId?: number;
  company?: string;
  legalName?: string;
  dbaName?: string;
  taxId?: string;
  registrationNumber?: string;
  yearFounded?: number;
  primaryContactId?: number;
  email?: string;
  secondaryEmail?: string;
  phone?: string;
  mobilePhone?: string;
  faxNumber?: string;
  jobTitle?: string;
  website?: string;
  address?: string;
  address2?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  shippingAddress?: string;
  shippingAddress2?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingZipCode?: string;
  shippingCountry?: string;
  shippingSameAsBilling?: boolean;
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
  lifecycleStage?: string;
  leadSource?: string;
  firstContactDate?: string;
  conversionDate?: string;
  lastActivityDate?: string;
  nextFollowUpDate?: string;
  totalPurchases?: number;
  accountBalance?: number;
  creditLimit?: number;
  paymentTerms?: string;
  preferredPaymentMethod?: string;
  currency?: string;
  billingCycle?: string;
  leadScore?: number;
  accountHealthScore?: number;
  npsScore?: number;
  satisfactionRating?: number;
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
  assignedToUserId?: number;
  assignedToUserName?: string;
  accountManagerId?: number;
  accountManagerName?: string;
  territory?: string;
  region?: string;
  tags?: string;
  segment?: string;
  referralSource?: string;
  referredByAccountId?: number;
  referredByAccountName?: string;
  parentAccountId?: number;
  parentAccountName?: string;
  notes?: string;
  internalNotes?: string;
  description?: string;
  customFields?: string | Record<string, any>;
  displayName?: string;
  contacts?: AccountContact[];
  contactCount?: number;
  emailAddresses?: LinkedEmail[];
  phoneNumbers?: LinkedPhone[];
  addresses?: LinkedAddress[];
  socialMediaAccounts?: LinkedSocialMedia[];
  isDeleted?: boolean;
  [key: string]: any;
}

export interface UpdateAccountDto extends Partial<CreateAccountDto> {}

const accountService = {
  // === CRUD Operations ===
  
  /**
   * Get all accounts
   */
  getAll: () => apiClient.get<Account[]>('/accounts'),
  
  /**
   * Get all accounts with optional limit
   */
  getAllWithLimit: (limit: number = 100) => 
    apiClient.get<Account[]>(`/accounts?limit=${limit}`),
  
  /**
   * Get account by ID
   */
  getById: (id: number) => apiClient.get<Account>(`/accounts/${id}`),
  
  /**
   * Create a new account
   */
  create: (data: CreateAccountDto) => {
    // Convert customFields object to JSON string if needed
    const payload = {
      ...data,
      customFields: typeof data.customFields === 'object' && data.customFields !== null
        ? JSON.stringify(data.customFields)
        : data.customFields
    };
    return apiClient.post<Account>('/accounts', payload);
  },
  
  /**
   * Update an existing account
   */
  update: (id: number, data: UpdateAccountDto) => {
    const payload = {
      ...data,
      customFields: typeof data.customFields === 'object' && data.customFields !== null
        ? JSON.stringify(data.customFields)
        : data.customFields
    };
    return apiClient.put<Account>(`/accounts/${id}`, payload);
  },
  
  /**
   * Delete an account (soft delete)
   */
  delete: (id: number) => apiClient.delete(`/accounts/${id}`),
  
  // === Search & Filtering ===
  
  /**
   * Search accounts by term
   */
  search: (term: string) => apiClient.get<Account[]>(`/accounts/search/${encodeURIComponent(term)}`),
  
  /**
   * Get individual accounts only
   */
  getIndividuals: () => apiClient.get<Account[]>('/accounts/individuals'),
  
  /**
   * Get organization accounts only
   */
  getOrganizations: () => apiClient.get<Account[]>('/accounts/organizations'),
  
  /**
   * Get accounts by lifecycle stage
   */
  getByLifecycleStage: (stage: number) => 
    apiClient.get<Account[]>(`/accounts/by-stage/${stage}`),
  
  /**
   * Get accounts by priority
   */
  getByPriority: (priority: number) => 
    apiClient.get<Account[]>(`/accounts/by-priority/${priority}`),
  
  /**
   * Get accounts assigned to a specific user
   */
  getByAssignedUser: (userId: number) => 
    apiClient.get<Account[]>(`/accounts/by-user/${userId}`),
  
  // === Contact Management ===
  
  /**
   * Get contacts linked to an account (many-to-many)
   */
  getContacts: (accountId: number) => 
    apiClient.get(`/accounts/${accountId}/contacts`),
  
  /**
   * Link a contact to an account
   */
  linkContact: (accountId: number, contactId: number, data?: { role?: number; isPrimary?: boolean }) =>
    apiClient.post(`/accounts/${accountId}/contacts`, { contactId, ...data }),
  
  /**
   * Unlink a contact from an account
   */
  unlinkContact: (accountId: number, contactId: number) =>
    apiClient.delete(`/accounts/${accountId}/contacts/${contactId}`),
  
  /**
   * Get direct contacts (one-to-many via AccountId FK)
   */
  getDirectContacts: (accountId: number) =>
    apiClient.get(`/accounts/${accountId}/direct-contacts`),
  
  /**
   * Assign a contact directly to an account
   */
  assignDirectContact: (accountId: number, contactId: number) =>
    apiClient.post(`/accounts/${accountId}/direct-contacts/${contactId}`),
  
  /**
   * Unassign a direct contact from an account
   */
  unassignDirectContact: (accountId: number, contactId: number) =>
    apiClient.delete(`/accounts/${accountId}/direct-contacts/${contactId}`),
  
  // === Contract Management ===
  
  /**
   * Upload a contract file for an account
   */
  uploadContract: (accountId: number, file: File) => {
    const form = new FormData();
    form.append('file', file, file.name);
    return apiClient.post(`/accounts/${accountId}/upload-contract`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },

  /**
   * Get the URL to download a contract
   */
  downloadContractUrl: (accountId: number) => {
    return `${(apiClient.defaults.baseURL || '').replace(/\/api$/, '')}/api/accounts/${accountId}/contract`;
  },

  /**
   * Delete a contract from an account
   */
  deleteContract: (accountId: number) => apiClient.delete(`/accounts/${accountId}/contract`),
};

export default accountService;
