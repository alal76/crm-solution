/**
 * File Upload Service — AP-044
 * Centralizes image/file upload and delete API calls from ImageUpload.tsx.
 * All endpoints are under /fileupload.
 */

import apiClient from './apiClient';

export type UploadEndpoint = 'logo' | 'user-photo' | 'customer-logo' | 'contact-photo';

export interface UploadResult {
  url: string;
}

const fileUploadService = {
  /**
   * Upload an image file to the specified endpoint.
   * Returns the server-assigned URL for the uploaded file.
   */
  uploadFile: async (endpoint: UploadEndpoint, file: File): Promise<UploadResult> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiClient.post<UploadResult>(`/fileupload/${endpoint}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  /**
   * Delete a previously uploaded file by its server path.
   * Silently ignores paths that are not server-managed (e.g. external URLs).
   */
  deleteFile: async (path: string): Promise<void> => {
    await apiClient.delete(`/fileupload?path=${encodeURIComponent(path)}`);
  },
};

export default fileUploadService;
