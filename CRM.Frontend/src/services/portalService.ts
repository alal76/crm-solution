// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import apiClient from './apiClient';

// ── Types ─────────────────────────────────────────────────────────────────────

export interface PortalLoginDto {
  email: string;
  password: string;
}

export interface PortalRegisterDto {
  email: string;
  password: string;
  confirmPassword: string;
  displayName?: string;
  accessCode?: string;
}

export interface PortalTokenResponse {
  accessToken: string;
  expiresAt: string;
  portalUserId: number;
  email: string;
  displayName?: string;
}

export interface PortalUserDto {
  id: number;
  email: string;
  displayName?: string;
  contactId?: number;
  accountId?: number;
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
}

export interface PortalConfigDto {
  isEnabled: boolean;
  allowSelfRegistration: boolean;
  welcomeMessage?: string;
  supportEmail?: string;
  logoUrl?: string;
  primaryColor?: string;
  portalTitle?: string;
  allowedDomains?: string;
}

export interface UpdatePortalConfigDto {
  isEnabled?: boolean;
  allowSelfRegistration?: boolean;
  welcomeMessage?: string;
  supportEmail?: string;
  logoUrl?: string;
  primaryColor?: string;
  portalTitle?: string;
  allowedDomains?: string;
}

export interface PortalTicketDto {
  id: number;
  title: string;
  description?: string;
  status: string;
  priority: string;
  ticketNumber: string;
  createdAt: string;
  updatedAt: string;
  lastCommentAt?: string;
}

export interface PortalCreateTicketDto {
  title: string;
  description?: string;
  priority?: string;
}

export interface PortalCommentDto {
  id: number;
  content: string;
  authorName: string;
  isStaff: boolean;
  createdAt: string;
}

export interface PortalKBArticleDto {
  id: number;
  title: string;
  summary?: string;
  content: string;
  categoryName?: string;
  viewCount: number;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// ── Local Storage Keys ────────────────────────────────────────────────────────

const PORTAL_TOKEN_KEY = 'portal_access_token';
const PORTAL_USER_KEY = 'portal_user';

export const portalTokenStore = {
  get: () => localStorage.getItem(PORTAL_TOKEN_KEY),
  set: (token: string) => localStorage.setItem(PORTAL_TOKEN_KEY, token),
  clear: () => {
    localStorage.removeItem(PORTAL_TOKEN_KEY);
    localStorage.removeItem(PORTAL_USER_KEY);
  },
};

function portalHeaders(): Record<string, string> {
  const token = portalTokenStore.get();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

// ── Portal Auth Service ───────────────────────────────────────────────────────

export const portalAuthService = {
  async login(dto: PortalLoginDto): Promise<PortalTokenResponse> {
    const { data } = await apiClient.post<PortalTokenResponse>('/portal/auth/login', dto);
    portalTokenStore.set(data.accessToken);
    localStorage.setItem(PORTAL_USER_KEY, JSON.stringify({ email: data.email, displayName: data.displayName, id: data.portalUserId }));
    return data;
  },

  async register(dto: PortalRegisterDto): Promise<PortalUserDto> {
    const { data } = await apiClient.post<PortalUserDto>('/portal/auth/register', dto);
    return data;
  },

  async forgotPassword(email: string): Promise<{ message: string }> {
    const { data } = await apiClient.post('/portal/auth/forgot-password', { email });
    return data;
  },

  async resetPassword(token: string, newPassword: string): Promise<{ message: string }> {
    const { data } = await apiClient.post('/portal/auth/reset-password', { token, newPassword });
    return data;
  },

  async verifyEmail(token: string): Promise<{ message: string }> {
    const { data } = await apiClient.get('/portal/auth/verify-email', { params: { token } });
    return data;
  },

  logout() {
    portalTokenStore.clear();
  },

  getCurrentUser(): { email: string; displayName?: string; id: number } | null {
    try {
      const stored = localStorage.getItem(PORTAL_USER_KEY);
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  },

  isAuthenticated(): boolean {
    return !!portalTokenStore.get();
  },
};

// ── Portal Service (customer-facing) ─────────────────────────────────────────

export const portalService = {
  // Config
  async getConfig(): Promise<PortalConfigDto> {
    const { data } = await apiClient.get<PortalConfigDto>('/portal/config');
    return data;
  },

  // Tickets
  async getMyTickets(page = 1, pageSize = 20): Promise<PagedResult<PortalTicketDto>> {
    const { data } = await apiClient.get<PagedResult<PortalTicketDto>>('/portal/tickets', {
      params: { page, pageSize },
      headers: portalHeaders(),
    });
    return data;
  },

  async getTicket(id: number): Promise<PortalTicketDto> {
    const { data } = await apiClient.get<PortalTicketDto>(`/portal/tickets/${id}`, {
      headers: portalHeaders(),
    });
    return data;
  },

  async createTicket(dto: PortalCreateTicketDto): Promise<PortalTicketDto> {
    const { data } = await apiClient.post<PortalTicketDto>('/portal/tickets', dto, {
      headers: portalHeaders(),
    });
    return data;
  },

  async getTicketComments(ticketId: number): Promise<PortalCommentDto[]> {
    const { data } = await apiClient.get<PortalCommentDto[]>(`/portal/tickets/${ticketId}/comments`, {
      headers: portalHeaders(),
    });
    return data;
  },

  async addComment(ticketId: number, content: string): Promise<PortalCommentDto> {
    const { data } = await apiClient.post<PortalCommentDto>(
      `/portal/tickets/${ticketId}/comments`,
      { content },
      { headers: portalHeaders() }
    );
    return data;
  },

  // Knowledge Base
  async getKBArticles(search?: string, page = 1, pageSize = 20): Promise<PagedResult<PortalKBArticleDto>> {
    const { data } = await apiClient.get<PagedResult<PortalKBArticleDto>>('/portal/knowledge-base', {
      params: { search, page, pageSize },
    });
    return data;
  },

  async getKBArticle(id: number): Promise<PortalKBArticleDto> {
    const { data } = await apiClient.get<PortalKBArticleDto>(`/portal/knowledge-base/${id}`);
    return data;
  },
};

// ── Portal Admin Service (CRM staff) ─────────────────────────────────────────

export const portalAdminService = {
  async getConfig(): Promise<PortalConfigDto> {
    const { data } = await apiClient.get<PortalConfigDto>('/admin/portal/config');
    return data;
  },

  async updateConfig(dto: UpdatePortalConfigDto): Promise<PortalConfigDto> {
    const { data } = await apiClient.put<PortalConfigDto>('/admin/portal/config', dto);
    return data;
  },

  async getPortalUsers(page = 1, pageSize = 20): Promise<PagedResult<PortalUserDto>> {
    const { data } = await apiClient.get<PagedResult<PortalUserDto>>('/admin/portal/users', {
      params: { page, pageSize },
    });
    return data;
  },

  async activateUser(id: number): Promise<void> {
    await apiClient.post(`/admin/portal/users/${id}/activate`);
  },

  async deactivateUser(id: number): Promise<void> {
    await apiClient.post(`/admin/portal/users/${id}/deactivate`);
  },
};
