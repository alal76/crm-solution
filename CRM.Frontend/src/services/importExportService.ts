/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This software is source-available. Non-commercial use is permitted under
 * the terms of the LICENSE file. Commercial use requires a separate license.
 * See the LICENSE file in the root directory for full terms.
 */

import apiClient from './apiClient';

export interface ImportJobDto {
  id: number;
  entityType: string;
  fileName: string;
  status: string; // Pending, Validating, Processing, Completed, Failed
  totalRecords: number;
  processedRecords: number;
  failedRecords: number;
  errors?: ImportErrorDto[];
  createdAt: string;
  completedAt?: string;
}

export interface ImportErrorDto {
  rowNumber: number;
  field: string;
  message: string;
  value?: string;
}

export interface ExportJobDto {
  id: number;
  entityType: string;
  format: string; // csv, xlsx, json
  status: string;
  totalRecords: number;
  downloadUrl?: string;
  createdAt: string;
  completedAt?: string;
}

export interface ColumnMappingDto {
  sourceColumn: string;
  targetField: string;
  transform?: string;
}

export interface ImportValidationResult {
  valid: boolean;
  errors: ImportErrorDto[];
  previewRows: Record<string, string>[];
}

const importExportService = {
  // Import
  startImport: (entityType: string, file: File, mappings?: ColumnMappingDto[]): Promise<ImportJobDto> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', entityType);
    if (mappings) formData.append('mappings', JSON.stringify(mappings));
    return apiClient.post<ImportJobDto>('/importjobs', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    }).then(r => r.data);
  },

  getImportJob: (id: number): Promise<ImportJobDto> =>
    apiClient.get<ImportJobDto>(`/importjobs/${id}`).then(r => r.data),

  getImportJobs: (): Promise<ImportJobDto[]> =>
    apiClient.get<ImportJobDto[]>('/importjobs').then(r => r.data),

  validateImport: (entityType: string, file: File): Promise<ImportValidationResult> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', entityType);
    return apiClient.post<ImportValidationResult>('/importjobs/validate', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    }).then(r => r.data);
  },

  // Export
  startExport: (entityType: string, format: string, filters?: Record<string, string>): Promise<ExportJobDto> =>
    apiClient.post<ExportJobDto>('/exportjobs', { entityType, format, filters }).then(r => r.data),

  getExportJob: (id: number): Promise<ExportJobDto> =>
    apiClient.get<ExportJobDto>(`/exportjobs/${id}`).then(r => r.data),

  getExportJobs: (): Promise<ExportJobDto[]> =>
    apiClient.get<ExportJobDto[]>('/exportjobs').then(r => r.data),

  downloadExport: (id: number): Promise<Blob> =>
    apiClient.get<Blob>(`/exportjobs/${id}/download`, { responseType: 'blob' }).then(r => r.data),
};

export default importExportService;
