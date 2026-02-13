import apiClient from './apiClient';

export interface ReportDefinitionDto {
  id: number;
  name: string;
  description: string;
  category: string;
  folderId?: number;
  createdById: number;
  isStandard: boolean;
  createdAt: string;
}

export interface CreateReportDefinitionDto {
  name: string;
  description: string;
  query: string;
  category: string;
  folderId?: number;
}

export interface UpdateReportDefinitionDto extends CreateReportDefinitionDto {
  id: number;
}

export interface ReportExecutionResultDto {
  reportId: number;
  executionId: number;
  columns: string[];
  data: Array<Record<string, object>>;
  executedAt: string;
  rowCount: number;
  executionTimeMs: number;
}

export interface ReportParametersDto {
  startDate?: string;
  endDate?: string;
  filters?: Record<string, object>;
}

export const reportService = {
  getReports: () => apiClient.get<ReportDefinitionDto[]>('/reports'),
  getReport: (id: number) => apiClient.get<ReportDefinitionDto>(`/reports/${id}`),
  createReport: (dto: CreateReportDefinitionDto) => apiClient.post<ReportDefinitionDto>('/reports', dto),
  updateReport: (id: number, dto: UpdateReportDefinitionDto) => apiClient.put<ReportDefinitionDto>(`/reports/${id}`, dto),
  deleteReport: (id: number) => apiClient.delete(`/reports/${id}`),
  executeReport: (id: number, parameters?: ReportParametersDto) =>
    apiClient.post<ReportExecutionResultDto>(`/reports/${id}/execute`, parameters ?? {}),
  previewReport: (id: number, limit = 10) =>
    apiClient.get<ReportExecutionResultDto>(`/reports/${id}/preview?limit=${limit}`),
  exportReport: (id: number, format: string, parameters?: ReportParametersDto) =>
    apiClient.get(`/reports/${id}/export/${format}`, { params: parameters, responseType: 'arraybuffer' }),
};

export default reportService;
