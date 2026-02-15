import apiClient from './apiClient';

export interface BrandingConfigDto {
  id: number;
  solutionName: string;
  customLogoPath?: string | null;
  customLogoFileName?: string | null;
  faviconPath?: string | null;
  faviconFileName?: string | null;
  softwareLogoPath: string;
  isCustomBrandingEnabled: boolean;
  faviconDataUrl?: string | null;
  lastLogoUploadedAt?: string | null;
  lastLogoUploadedById?: number | null;
  lastFaviconUploadedAt?: string | null;
  lastFaviconUploadedById?: number | null;
}

export interface BrandingOperationResponse {
  success: boolean;
  message?: string;
  data?: BrandingConfigDto | null;
  validationErrors?: Record<string, string> | null;
}

export interface UploadLogoRequest {
  fileContent: string;
  fileName: string;
  mimeType: string;
  fileSizeBytes: number;
}

export interface UploadFaviconRequest {
  fileContent: string;
  fileName: string;
  mimeType: string;
  fileSizeBytes: number;
}

const brandingConfigService = {
  async getCurrent(): Promise<BrandingConfigDto> {
    const response = await apiClient.get<BrandingConfigDto>('/branding');
    return response.data;
  },

  async getById(id: number): Promise<BrandingConfigDto> {
    const response = await apiClient.get<BrandingConfigDto>(`/branding/${id}`);
    return response.data;
  },

  async updateSolutionName(solutionName: string): Promise<BrandingConfigDto> {
    const response = await apiClient.post<BrandingConfigDto>('/branding/solution-name', {
      solutionName,
    });
    return response.data;
  },

  async toggleCustomBranding(isEnabled: boolean): Promise<BrandingConfigDto> {
    const response = await apiClient.post<BrandingConfigDto>(`/branding/toggle-custom-branding?isEnabled=${encodeURIComponent(String(isEnabled))}`);
    return response.data;
  },

  async uploadLogo(request: UploadLogoRequest): Promise<BrandingOperationResponse> {
    const response = await apiClient.post<BrandingOperationResponse>('/branding/upload-logo', request);
    return response.data;
  },

  async uploadFavicon(request: UploadFaviconRequest): Promise<BrandingOperationResponse> {
    const response = await apiClient.post<BrandingOperationResponse>('/branding/upload-favicon', request);
    return response.data;
  },

  async deleteCustomLogo(): Promise<BrandingConfigDto> {
    const response = await apiClient.delete<BrandingConfigDto>('/branding/custom-logo');
    return response.data;
  },

  async deleteFavicon(): Promise<BrandingConfigDto> {
    const response = await apiClient.delete<BrandingConfigDto>('/branding/favicon');
    return response.data;
  },
};

export default brandingConfigService;
