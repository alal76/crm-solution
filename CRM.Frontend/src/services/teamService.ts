import apiClient from './apiClient';

// ─── Enums ───────────────────────────────────────────────────────────────────

export enum TeamRole {
  Member = 0,
  Lead = 1,
  Manager = 2,
  Admin = 3,
}

// ─── Interfaces ──────────────────────────────────────────────────────────────

export interface Team {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  managerId?: number;
  managerName?: string;
  parentTeamId?: number;
  memberCount?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface TeamMember {
  id: number;
  teamId: number;
  userId: number;
  userName?: string;
  email?: string;
  role: TeamRole;
  joinedAt?: string;
}

export interface TeamPerformance {
  teamId: number;
  teamName: string;
  fromDate: string;
  toDate: string;
  totalRevenue: number;
  totalQuotaValue: number;
  quotaAttainment: number;
  dealsWon: number;
  dealsLost: number;
  winRate: number;
  averageDealSize: number;
  newAccounts: number;
  activeOpportunities: number;
  pipelineValue: number;
}

export interface TeamStatistics {
  teamId: number;
  totalMembers: number;
  activeMembers: number;
  assignedAccounts: number;
  activeOpportunities: number;
  assignedTerritories: number;
  createdAt?: string;
}

export interface TeamRanking {
  rank: number;
  teamId: number;
  teamName: string;
  revenue: number;
  dealsWon: number;
  quotaAttainment: number;
}

export interface MemberPerformance {
  userId: number;
  userName: string;
  revenue: number;
  dealsWon: number;
  dealsLost: number;
  winRate: number;
  pipelineValue: number;
  quotaAttainment: number;
}

export interface TeamHierarchy {
  teamId: number;
  teamName: string;
  managerId?: number;
  managerName?: string;
  memberCount: number;
  children: TeamHierarchy[];
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const getTeamRoleLabel = (role: TeamRole): string => {
  const labels: Record<TeamRole, string> = {
    [TeamRole.Member]: 'Member',
    [TeamRole.Lead]: 'Lead',
    [TeamRole.Manager]: 'Manager',
    [TeamRole.Admin]: 'Admin',
  };
  return labels[role] ?? 'Unknown';
};

export const getTeamRoleColor = (role: TeamRole): string => {
  const colors: Record<TeamRole, string> = {
    [TeamRole.Member]: 'default',
    [TeamRole.Lead]: 'info',
    [TeamRole.Manager]: 'warning',
    [TeamRole.Admin]: 'error',
  };
  return colors[role] ?? 'default';
};

// ─── Service ─────────────────────────────────────────────────────────────────

const teamService = {
  // CRUD
  getAll: (isActive?: boolean, managerId?: number) => {
    const params = new URLSearchParams();
    if (isActive !== undefined) params.append('isActive', isActive.toString());
    if (managerId !== undefined) params.append('managerId', managerId.toString());
    const query = params.toString();
    return apiClient.get<Team[]>(`/api/teams${query ? `?${query}` : ''}`);
  },
  getById: (id: number) => apiClient.get<Team>(`/api/teams/${id}`),
  getByName: (name: string) => apiClient.get<Team>(`/api/teams/by-name/${encodeURIComponent(name)}`),
  create: (team: Partial<Team>) => apiClient.post<Team>('/teams', team),
  update: (id: number, team: Partial<Team>) => apiClient.put<Team>(`/api/teams/${id}`, team),
  delete: (id: number) => apiClient.delete(`/teams/${id}`),

  // Member Management
  addMember: (teamId: number, userId: number, role: TeamRole = TeamRole.Member) =>
    apiClient.post<TeamMember>(`/api/teams/${teamId}/members`, { userId, role }),
  removeMember: (teamId: number, userId: number) => apiClient.delete(`/teams/${teamId}/members/${userId}`),
  updateMemberRole: (teamId: number, userId: number, role: TeamRole) =>
    apiClient.put<TeamMember>(`/api/teams/${teamId}/members/${userId}/role`, { role }),
  getMembers: (teamId: number) => apiClient.get<TeamMember[]>(`/api/teams/${teamId}/members`),
  getTeamsForUser: (userId: number) => apiClient.get<Team[]>(`/api/teams/by-user/${userId}`),
  isMember: (teamId: number, userId: number) =>
    apiClient.get<{ teamId: number; userId: number; isMember: boolean }>(`/api/teams/${teamId}/members/${userId}/check`),

  // Team Manager
  setManager: (teamId: number, managerId: number) =>
    apiClient.put<Team>(`/api/teams/${teamId}/manager`, { managerId }),
  getManagedTeams: (managerId: number) => apiClient.get<Team[]>(`/api/teams/managed-by/${managerId}`),

  // Territory Management
  assignTerritory: (teamId: number, territoryId: number) =>
    apiClient.post(`/teams/${teamId}/territories`, { territoryId }),
  removeTerritory: (teamId: number, territoryId: number) =>
    apiClient.delete(`/teams/${teamId}/territories/${territoryId}`),
  getTerritories: (teamId: number) => apiClient.get(`/teams/${teamId}/territories`),
  getTeamByTerritory: (territoryId: number) => apiClient.get<Team>(`/api/teams/by-territory/${territoryId}`),

  // Account Assignment
  assignAccount: (teamId: number, accountId: number) =>
    apiClient.post(`/teams/${teamId}/accounts`, { accountId }),
  removeAccount: (teamId: number, accountId: number) =>
    apiClient.delete(`/teams/${teamId}/accounts/${accountId}`),
  getAssignedAccounts: (teamId: number) => apiClient.get(`/teams/${teamId}/accounts`),
  getTeamByAccount: (accountId: number) => apiClient.get<Team>(`/api/teams/by-account/${accountId}`),
  bulkAssignAccounts: (teamId: number, accountIds: number[]) =>
    apiClient.post(`/teams/${teamId}/accounts/bulk`, { accountIds }),

  // Performance & Stats
  getPerformance: (teamId: number, fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const query = params.toString();
    return apiClient.get<TeamPerformance>(`/api/teams/${teamId}/performance${query ? `?${query}` : ''}`);
  },
  getStatistics: (teamId: number) => apiClient.get<TeamStatistics>(`/api/teams/${teamId}/statistics`),
  getLeaderboard: (topN: number = 10, fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    params.append('topN', topN.toString());
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    return apiClient.get<TeamRanking[]>(`/api/teams/leaderboard?${params.toString()}`);
  },
  getMemberPerformance: (teamId: number, fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const query = params.toString();
    return apiClient.get<MemberPerformance[]>(`/api/teams/${teamId}/members/performance${query ? `?${query}` : ''}`);
  },

  // Hierarchy
  getChildTeams: (teamId: number) => apiClient.get<Team[]>(`/api/teams/${teamId}/children`),
  getParentTeam: (teamId: number) => apiClient.get<Team>(`/api/teams/${teamId}/parent`),
  setParentTeam: (teamId: number, parentTeamId: number | null) =>
    apiClient.put<Team>(`/api/teams/${teamId}/parent`, { parentTeamId }),
  getHierarchy: (rootTeamId?: number) => {
    const params = rootTeamId !== undefined ? `?rootTeamId=${rootTeamId}` : '';
    return apiClient.get<TeamHierarchy>(`/api/teams/hierarchy${params}`);
  },
};

export default teamService;
