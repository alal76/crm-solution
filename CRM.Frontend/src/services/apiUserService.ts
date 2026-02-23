import apiClient from './apiClient';

// ── Types ──

export interface ApiUserDto {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isActive: boolean;
  apiKeyPrefix: string | null;
  apiKeyCreatedAt: string | null;
  apiKeyLastUsedAt: string | null;
  apiKeyExpiresAt: string | null;
  apiUserDescription: string | null;
  primaryGroupId: number | null;
  primaryGroupName: string | null;
  createdAt: string;
}

export interface CreateApiUserRequest {
  name: string;
  email: string;
  description?: string;
  roleId: number;
  primaryGroupId?: number | null;
  expiresAt?: string | null;
}

export interface ApiKeyResponse {
  userId: number;
  username: string;
  apiKey: string;
  apiKeyPrefix: string;
  createdAt: string;
  expiresAt: string | null;
}

// ── Service ──

export const apiUserService = {
  getAll: async (): Promise<ApiUserDto[]> => {
    const response = await apiClient.get<ApiUserDto[]>('/apiusers');
    return response.data;
  },

  getById: async (id: number): Promise<ApiUserDto> => {
    const response = await apiClient.get<ApiUserDto>(`/apiusers/${id}`);
    return response.data;
  },

  create: async (request: CreateApiUserRequest): Promise<ApiKeyResponse> => {
    const response = await apiClient.post<ApiKeyResponse>('/apiusers', request);
    return response.data;
  },

  update: async (id: number, request: CreateApiUserRequest): Promise<ApiUserDto> => {
    const response = await apiClient.put<ApiUserDto>(`/apiusers/${id}`, request);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/apiusers/${id}`);
  },

  regenerateKey: async (id: number): Promise<ApiKeyResponse> => {
    const response = await apiClient.post<ApiKeyResponse>(`/apiusers/${id}/regenerate-key`);
    return response.data;
  },

  revoke: async (id: number): Promise<void> => {
    await apiClient.post(`/apiusers/${id}/revoke`);
  },

  toggleStatus: async (id: number): Promise<ApiUserDto> => {
    const response = await apiClient.post<ApiUserDto>(`/apiusers/${id}/toggle-status`);
    return response.data;
  },
};

export default apiUserService;
