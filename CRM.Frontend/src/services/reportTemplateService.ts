import apiClient from './apiClient';

/**
 * REV-FE-003 — Report Templates Marketplace.
 * Backed by GET /api/reports/templates and POST /api/reports/templates/{id}/apply
 * on ReportsController (CRM.Backend/src/CRM.Api/Controllers/ReportsController.cs,
 * "Report Templates Marketplace" region).
 */
export interface ReportTemplateDto {
  id: number;
  name: string;
  description: string;
  category: string;
  author: string;
  rating: number;
  downloads: number;
  tags: string[];
  previewImage?: string | null;
  reportConfig: Record<string, unknown>;
  createdAt: string;
}

export interface ApplyReportTemplateResultDto {
  templateId: number;
  templateName: string;
  reportConfig: Record<string, unknown>;
  downloads: number;
}

export const reportTemplateService = {
  getTemplates: () => apiClient.get<ReportTemplateDto[]>('/reports/templates'),
  applyTemplate: (id: number) =>
    apiClient.post<ApplyReportTemplateResultDto>(`/reports/templates/${id}/apply`),
};

export default reportTemplateService;
