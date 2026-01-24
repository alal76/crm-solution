import apiClient from './apiClient';

export interface FieldMasterDataLink {
  id: number;
  fieldConfigurationId: number;
  sourceType: string;
  sourceName: string;
  displayField?: string;
  valueField?: string;
  filterExpression?: string;
  dependsOnField?: string;
  dependsOnSourceColumn?: string;
  allowFreeText: boolean;
  validationType?: string;
  validationPattern?: string;
  validationMessage?: string;
  sortOrder: number;
  isActive: boolean;
}

export interface CreateFieldMasterDataLink {
  fieldConfigurationId: number;
  sourceType: string;
  sourceName: string;
  displayField?: string;
  valueField?: string;
  filterExpression?: string;
  dependsOnField?: string;
  dependsOnSourceColumn?: string;
  allowFreeText?: boolean;
  validationType?: string;
  validationPattern?: string;
  validationMessage?: string;
  sortOrder?: number;
  isActive?: boolean;
}

export interface MasterDataSource {
  sourceType: string;
  sourceName: string;
  displayName: string;
  availableFields: string[];
}

export interface MasterDataLookupResult {
  value: string;
  display: string;
  metadata?: Record<string, unknown>;
}

export interface ValidationResult {
  isValid: boolean;
  errorMessage?: string;
}

const fieldMasterDataService = {
  /**
   * Get all master data links for a specific field
   */
  getLinksForField: async (fieldConfigurationId: number): Promise<FieldMasterDataLink[]> => {
    const response = await apiClient.get<FieldMasterDataLink[]>(
      `/fieldmasterdata/field/${fieldConfigurationId}`
    );
    return response.data;
  },

  /**
   * Get all master data links for a module
   */
  getLinksForModule: async (moduleName: string): Promise<Record<number, FieldMasterDataLink[]>> => {
    const response = await apiClient.get<Record<number, FieldMasterDataLink[]>>(
      `/fieldmasterdata/module/${encodeURIComponent(moduleName)}`
    );
    return response.data;
  },

  /**
   * Get a specific master data link by ID
   */
  getLinkById: async (id: number): Promise<FieldMasterDataLink> => {
    const response = await apiClient.get<FieldMasterDataLink>(
      `/fieldmasterdata/${id}`
    );
    return response.data;
  },

  /**
   * Create a new master data link
   */
  createLink: async (link: CreateFieldMasterDataLink): Promise<FieldMasterDataLink> => {
    const response = await apiClient.post<FieldMasterDataLink>(
      `/fieldmasterdata`,
      link
    );
    return response.data;
  },

  /**
   * Update an existing master data link
   */
  updateLink: async (id: number, link: CreateFieldMasterDataLink): Promise<FieldMasterDataLink> => {
    const response = await apiClient.put<FieldMasterDataLink>(
      `/fieldmasterdata/${id}`,
      link
    );
    return response.data;
  },

  /**
   * Delete a master data link
   */
  deleteLink: async (id: number): Promise<void> => {
    await apiClient.delete(
      `/fieldmasterdata/${id}`
    );
  },

  /**
   * Get all available master data sources
   */
  getAvailableSources: async (): Promise<MasterDataSource[]> => {
    const response = await apiClient.get<MasterDataSource[]>(
      `/fieldmasterdata/sources`
    );
    return response.data;
  },

  /**
   * Get master data lookup values for a field
   */
  getLookupData: async (
    fieldConfigurationId: number,
    options?: {
      search?: string;
      limit?: number;
      dependentValues?: Record<string, string>;
    }
  ): Promise<MasterDataLookupResult[]> => {
    const params = new URLSearchParams();
    if (options?.search) params.append('search', options.search);
    if (options?.limit) params.append('limit', options.limit.toString());
    if (options?.dependentValues) {
      Object.entries(options.dependentValues).forEach(([key, value]) => {
        if (value) params.append(key, value);
      });
    }

    const response = await apiClient.get<MasterDataLookupResult[]>(
      `/fieldmasterdata/lookup/${fieldConfigurationId}?${params.toString()}`
    );
    return response.data;
  },

  /**
   * Validate a value against field's master data constraints
   */
  validateValue: async (
    fieldConfigurationId: number,
    value: string,
    dependentValues?: Record<string, string>
  ): Promise<ValidationResult> => {
    const response = await apiClient.post<ValidationResult>(
      `/fieldmasterdata/validate/${fieldConfigurationId}`,
      { value, dependentValues }
    );
    return response.data;
  },

  /**
   * Get source type display name
   */
  getSourceTypeDisplayName: (sourceType: string): string => {
    switch (sourceType) {
      case 'LookupCategory':
        return 'Lookup Category';
      case 'Table':
        return 'Database Table';
      case 'Api':
        return 'API Endpoint';
      default:
        return sourceType;
    }
  },

  /**
   * Get validation type display name
   */
  getValidationTypeDisplayName: (validationType: string): string => {
    switch (validationType) {
      case 'regex':
        return 'Regular Expression';
      case 'required':
        return 'Required';
      case 'custom':
        return 'Custom Validation';
      case 'range':
        return 'Range Validation';
      case 'length':
        return 'Length Validation';
      default:
        return validationType || 'None';
    }
  }
};

export default fieldMasterDataService;
