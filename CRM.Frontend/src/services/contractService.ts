/**
 * Contract Service - Manages contract lifecycle, renewals, amendments, and signatures
 *
 * Endpoints map to CRM.Api/Controllers/ContractsController.cs
 */
import apiClient from './apiClient';
import { Contract, ContractStatus, ContractType } from '../types/sales';

// ============================================================================
// Types
// ============================================================================

// Contract, ContractStatus and ContractType are the single source of truth
// (types/sales.ts), kept in sync with backend CRM.Core.Dtos.ContractDto and
// CRM.Core.Entities.ContractStatus / ContractType. Re-exported here so
// existing imports from this service keep working.
export { ContractStatus, ContractType };
export type { Contract };

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
  // Field names must match backend RenewContractRequest (NewStartDate/NewEndDate/NewValue).
  renew: (id: number, newStartDate?: string, newEndDate?: string, newValue?: number) =>
    apiClient.post<Contract>(`/contracts/${id}/renew`, { newStartDate, newEndDate, newValue }),

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
    apiClient.post<{ success: boolean }>(`/contracts/${id}/send-for-signature`, { signers }),

  getSignatureStatus: (id: number) =>
    apiClient.get<ContractSignatureStatus>(`/contracts/${id}/signature-status`),

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
