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

export interface Account {
  id?: number;
  category?: number;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  company?: string;
  legalName?: string;
  jobTitle?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  annualRevenue?: number;
  customerType?: number;  // API compatibility - maps to accountType
  priority?: number;
  lifecycleStage?: number;
  industry?: string;
  website?: string;
  [key: string]: any;
}

export interface CreateAccountDto {
  category: number;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  company?: string;
  customerType?: number;
  lifecycleStage?: number;
  [key: string]: any;
}

export interface UpdateAccountDto {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  company?: string;
  [key: string]: any;
}

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
  create: (data: CreateAccountDto) => apiClient.post<Account>('/accounts', data),
  
  /**
   * Update an existing account
   */
  update: (id: number, data: UpdateAccountDto) => 
    apiClient.put<Account>(`/accounts/${id}`, data),
  
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
   * Get direct contacts (one-to-many via CustomerId FK)
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
