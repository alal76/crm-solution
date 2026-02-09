/**
 * Contract Service - Manages contract lifecycle, renewals, amendments, and signatures
 *
 * Endpoints map to CRM.Api/Controllers/ContractsController.cs
 */
import apiClient from './apiClient';

// ============================================================================
// Types
// ============================================================================

export enum ContractStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Active = 3,
  Expired = 4,
  Terminated = 5,
  Renewed = 6,
  OnHold = 7,
}

export enum ContractType {
  Service = 0,
  License = 1,
  Subscription = 2,
  Support = 3,
  Maintenance = 4,
  NDA = 5,
  Master = 6,
  Amendment = 7,
  Other = 8,
}

export interface Contract {
  id: number;
  contractNumber?: string;
  title?: string;
  description?: string;
  accountId: number;
  account?: { id: number; company?: string; email?: string };
  contactId?: number;
  contractType: ContractType;
  contractStatus: ContractStatus;
  startDate?: string;
  endDate?: string;
  value?: number;
  currency?: string;
  billingFrequency?: string;
  paymentTerms?: string;
  autoRenew?: boolean;
  renewalTermMonths?: number;
  renewalNoticeDays?: number;
  terminationNoticeDays?: number;
  parentContractId?: number;
  originalContractId?: number;
  signedDate?: string;
  signedBy?: string;
  isSigned?: boolean;
  isExpiringSoon?: boolean;
  daysUntilExpiration?: number;
  notes?: string;
  terms?: string;
  ownerId?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface ContractCreateRequest {
  title: string;
  description?: string;
  accountId: number;
  contactId?: number;
  contractType: ContractType;
  startDate?: string;
  endDate?: string;
  value?: number;
  currency?: string;
  billingFrequency?: string;
  paymentTerms?: string;
  autoRenew?: boolean;
  renewalTermMonths?: number;
  renewalNoticeDays?: number;
  terminationNoticeDays?: number;
  notes?: string;
  terms?: string;
  ownerId?: number;
}

export interface ContractUpdateRequest extends Partial<ContractCreateRequest> {
  id?: number;
}

export interface ContractSigner {
  name: string;
  email: string;
  role?: string;
  order?: number;
}

export interface ContractSignatureStatus {
  contractId: number;
  allSigned: boolean;
  totalSigners: number;
  signedCount: number;
  signers: SignerStatus[];
}

export interface SignerStatus {
  signerId?: string;
  name: string;
  email: string;
  hasSigned: boolean;
  signedAt?: string;
  viewedAt?: string;
}

export interface ContractDocument {
  id: number;
  contractId: number;
  documentType: string;
  fileName: string;
  filePath: string;
  fileSize: number;
  uploadedAt: string;
  uploadedBy?: number;
}

export interface ContractStatistics {
  totalContracts: number;
  activeContracts: number;
  expiringContracts: number;
  expiredContracts: number;
  pendingRenewals: number;
  totalContractValue: number;
  activeContractValue: number;
  renewalRate: number;
  averageContractLength: number;
  contractsByType: Record<string, number>;
}

// ============================================================================
// Status Helpers
// ============================================================================

export const getStatusLabel = (status: ContractStatus): string => {
  const labels: Record<ContractStatus, string> = {
    [ContractStatus.Draft]: 'Draft',
    [ContractStatus.PendingApproval]: 'Pending Approval',
    [ContractStatus.Approved]: 'Approved',
    [ContractStatus.Active]: 'Active',
    [ContractStatus.Expired]: 'Expired',
    [ContractStatus.Terminated]: 'Terminated',
    [ContractStatus.Renewed]: 'Renewed',
    [ContractStatus.OnHold]: 'On Hold',
  };
  return labels[status] ?? 'Unknown';
};

export const getStatusColor = (status: ContractStatus): 'success' | 'warning' | 'error' | 'info' | 'default' => {
  const colors: Record<ContractStatus, 'success' | 'warning' | 'error' | 'info' | 'default'> = {
    [ContractStatus.Draft]: 'default',
    [ContractStatus.PendingApproval]: 'warning',
    [ContractStatus.Approved]: 'info',
    [ContractStatus.Active]: 'success',
    [ContractStatus.Expired]: 'error',
    [ContractStatus.Terminated]: 'error',
    [ContractStatus.Renewed]: 'success',
    [ContractStatus.OnHold]: 'warning',
  };
  return colors[status] ?? 'default';
};

export const getTypeLabel = (type: ContractType): string => {
  const labels: Record<ContractType, string> = {
    [ContractType.Service]: 'Service',
    [ContractType.License]: 'License',
    [ContractType.Subscription]: 'Subscription',
    [ContractType.Support]: 'Support',
    [ContractType.Maintenance]: 'Maintenance',
    [ContractType.NDA]: 'NDA',
    [ContractType.Master]: 'Master',
    [ContractType.Amendment]: 'Amendment',
    [ContractType.Other]: 'Other',
  };
  return labels[type] ?? 'Unknown';
};

// ============================================================================
// Service
// ============================================================================

const contractService = {
  // CRUD Operations
  getAll: (customerId?: number, status?: ContractStatus, contractType?: ContractType) => {
    const params = new URLSearchParams();
    if (customerId !== undefined) params.append('customerId', customerId.toString());
    if (status !== undefined) params.append('status', status.toString());
    if (contractType !== undefined) params.append('contractType', contractType.toString());
    const qs = params.toString();
    return apiClient.get<Contract[]>(`/contracts${qs ? `?${qs}` : ''}`);
  },

  getById: (id: number) => apiClient.get<Contract>(`/contracts/${id}`),

  getByContractNumber: (contractNumber: string) =>
    apiClient.get<Contract>(`/contracts/by-number/${encodeURIComponent(contractNumber)}`),

  create: (data: ContractCreateRequest) =>
    apiClient.post<Contract>('/contracts', data),

  update: (id: number, data: ContractUpdateRequest) =>
    apiClient.put<Contract>(`/contracts/${id}`, data),

  delete: (id: number) => apiClient.delete(`/contracts/${id}`),

  // Create from entities
  createFromQuote: (quoteId: number) =>
    apiClient.post<Contract>(`/contracts/from-quote/${quoteId}`, {}),

  createFromOrder: (orderId: number) =>
    apiClient.post<Contract>(`/contracts/from-order/${orderId}`, {}),

  // Status Management
  approve: (id: number) =>
    apiClient.post<Contract>(`/contracts/${id}/approve`, {}),

  reject: (id: number, reason: string) =>
    apiClient.post<Contract>(`/contracts/${id}/reject`, { reason }),

  activate: (id: number) =>
    apiClient.post<Contract>(`/contracts/${id}/activate`, {}),

  suspend: (id: number, reason: string) =>
    apiClient.post<Contract>(`/contracts/${id}/suspend`, { reason }),

  terminate: (id: number, reason: string, terminationDate?: string) =>
    apiClient.post<Contract>(`/contracts/${id}/terminate`, { reason, terminationDate }),

  expire: (id: number) =>
    apiClient.post<Contract>(`/contracts/${id}/expire`, {}),

  // Renewal
  renew: (id: number, newTermMonths?: number, newValue?: number) =>
    apiClient.post<Contract>(`/contracts/${id}/renew`, { newTermMonths, newValue }),

  getDueForRenewal: (withinDays: number = 30) =>
    apiClient.get<Contract[]>(`/contracts/due-for-renewal?withinDays=${withinDays}`),

  getRenewalHistory: (id: number) =>
    apiClient.get<Contract[]>(`/contracts/${id}/renewal-history`),

  // Amendments
  createAmendment: (id: number, data: ContractCreateRequest) =>
    apiClient.post<Contract>(`/contracts/${id}/amendments`, data),

  getAmendments: (id: number) =>
    apiClient.get<Contract[]>(`/contracts/${id}/amendments`),

  // Signatures
  sendForSignature: (id: number, signers: ContractSigner[]) =>
    apiClient.post<{ success: boolean }>(`/contracts/${id}/signature`, { signers }),

  getSignatureStatus: (id: number) =>
    apiClient.get<ContractSignatureStatus>(`/contracts/${id}/signature`),

  // Documents
  getDocuments: (id: number) =>
    apiClient.get<ContractDocument[]>(`/contracts/${id}/documents`),

  generatePdf: (id: number) =>
    apiClient.get(`/contracts/${id}/pdf`, { responseType: 'blob' }),

  // Queries
  getActiveContracts: (customerId: number) =>
    apiClient.get<Contract[]>(`/contracts/active/${customerId}`),

  getExpiringContracts: (days: number = 30) =>
    apiClient.get<Contract[]>(`/contracts/expiring?days=${days}`),

  getStatistics: (fromDate?: Date, toDate?: Date) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate.toISOString());
    if (toDate) params.append('toDate', toDate.toISOString());
    const qs = params.toString();
    return apiClient.get<ContractStatistics>(`/contracts/statistics${qs ? `?${qs}` : ''}`);
  },

  search: (query: string) =>
    apiClient.get<Contract[]>(`/contracts/search?q=${encodeURIComponent(query)}`),

  getTotalContractValue: (customerId: number) =>
    apiClient.get<number>(`/contracts/value/${customerId}`),
};

export default contractService;
