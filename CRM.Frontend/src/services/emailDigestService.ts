import apiClient from './apiClient';

// ── Types ──
// Mirrors EmailDigestConfig in pages/EmailDigestPage.tsx and EmailDigestConfigDto on the backend
// (CRM.Backend/src/CRM.Core/Dtos/EmailDigestConfigDto.cs).

export interface EmailDigestSectionsDto {
  newLeads: boolean;
  openOpportunities: boolean;
  recentActivities: boolean;
  upcomingTasks: boolean;
  overdueTasks: boolean;
  teamPerformance: boolean;
  kpiSummary: boolean;
}

export interface EmailDigestConfigDto {
  enabled: boolean;
  frequency: 'daily' | 'weekly' | 'monthly';
  dayOfWeek?: number;
  dayOfMonth?: number;
  timeOfDay: string;
  timezone: string;
  sections: EmailDigestSectionsDto;
}

// ── Service ──

export const emailDigestService = {
  getConfig: async (): Promise<EmailDigestConfigDto> => {
    const response = await apiClient.get<EmailDigestConfigDto>('/users/me/email-digest');
    return response.data;
  },

  updateConfig: async (config: EmailDigestConfigDto): Promise<EmailDigestConfigDto> => {
    const response = await apiClient.put<EmailDigestConfigDto>('/users/me/email-digest', config);
    return response.data;
  },

  sendPreview: async (): Promise<void> => {
    await apiClient.post('/users/me/email-digest/preview');
  },
};

export default emailDigestService;
