// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import apiClient from './apiClient';

// ── Types ─────────────────────────────────────────────────────────────────────

export interface PartnerDealDto {
  id: number;
  name: string;
  stage: string;
  amount: number;
  currency: string;
  expectedCloseDate: string | null;
  createdAt: string;
}

export interface PartnerLeadDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  companyName: string | null;
  status: string;
  createdAt: string;
}

export interface PartnerCommissionDto {
  id: number;
  commissionNumber: string;
  commissionPeriod: string;
  commissionAmount: number;
  finalCommissionAmount: number;
  currency: string;
  status: string;
  earnedDate: string;
  paidDate: string | null;
}

export interface PartnerDashboardDto {
  partnerName: string;
  activeDealCount: number;
  totalLeadCount: number;
  commissionEarnedThisMonth: number;
  pipelineValue: number;
  recentDeals: PartnerDealDto[];
  recentLeads: PartnerLeadDto[];
}

// ── Service ───────────────────────────────────────────────────────────────────

const BASE = '/api/partner-portal';

const partnerPortalService = {
  getDashboard(): Promise<PartnerDashboardDto> {
    return apiClient.get<PartnerDashboardDto>(`${BASE}/dashboard`).then(r => r.data);
  },

  getLeads(page = 1, pageSize = 20): Promise<PartnerLeadDto[]> {
    return apiClient
      .get<PartnerLeadDto[]>(`${BASE}/leads`, { params: { page, pageSize } })
      .then(r => r.data);
  },

  getDeals(partnerAccountId: number): Promise<PartnerDealDto[]> {
    return apiClient
      .get<PartnerDealDto[]>(`${BASE}/deals`, { params: { partnerAccountId } })
      .then(r => r.data);
  },

  getCommissions(): Promise<PartnerCommissionDto[]> {
    return apiClient.get<PartnerCommissionDto[]>(`${BASE}/commissions`).then(r => r.data);
  },
};

export default partnerPortalService;
