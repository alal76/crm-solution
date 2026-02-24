/**
 * Service Desk Configuration Service
 *
 * Provides API methods for managing service desk settings:
 * - Service request categories, subcategories, and types
 * - Custom field definitions
 * - SLA policies and instances
 *
 * Backend controllers:
 * - ServiceRequestSettingsController: api/service-request-settings
 * - SLAController:                    api/itsm/sla
 */

import apiClient from './apiClient';

// ── Types ────────────────────────────────────────────────────────────────────

export interface ServiceRequestCategoryDto {
  id: number;
  name: string;
  description?: string;
  icon?: string;
  isActive: boolean;
  sortOrder: number;
  defaultPriority?: number;
  slaResponseHours?: number;
  slaResolutionHours?: number;
  subcategories?: ServiceRequestSubcategoryDto[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateServiceRequestCategoryDto {
  name: string;
  description?: string;
  icon?: string;
  isActive?: boolean;
  sortOrder?: number;
  defaultPriority?: number;
  slaResponseHours?: number;
  slaResolutionHours?: number;
}

export interface ServiceRequestSubcategoryDto {
  id: number;
  name: string;
  description?: string;
  categoryId: number;
  isActive: boolean;
  sortOrder: number;
  slaResponseHours?: number;
  slaResolutionHours?: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateServiceRequestSubcategoryDto {
  name: string;
  description?: string;
  categoryId: number;
  isActive?: boolean;
  sortOrder?: number;
  slaResponseHours?: number;
  slaResolutionHours?: number;
}

export interface ServiceRequestTypeDto {
  id: number;
  name: string;
  description?: string;
  categoryId: number;
  subcategoryId?: number;
  categoryName?: string;
  subcategoryName?: string;
  icon?: string;
  isActive: boolean;
  sortOrder: number;
  defaultPriority?: number;
  requiresApproval: boolean;
  estimatedMinutes?: number;
  formTemplate?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ServiceRequestTypeGroupedDto {
  categoryId: number;
  categoryName: string;
  subcategories: {
    subcategoryId: number;
    subcategoryName: string;
    types: ServiceRequestTypeDto[];
  }[];
}

export interface CreateServiceRequestTypeDto {
  name: string;
  description?: string;
  categoryId: number;
  subcategoryId?: number;
  icon?: string;
  isActive?: boolean;
  sortOrder?: number;
  defaultPriority?: number;
  requiresApproval?: boolean;
  estimatedMinutes?: number;
  formTemplate?: string;
}

export interface UpdateServiceRequestTypeDto extends CreateServiceRequestTypeDto {
  id?: number;
}

export interface ServiceRequestCustomFieldDto {
  id: number;
  name: string;
  label: string;
  fieldType: string;
  isRequired: boolean;
  isActive: boolean;
  sortOrder: number;
  defaultValue?: string;
  options?: string;
  validationRegex?: string;
  helpText?: string;
  categoryId?: number;
  subcategoryId?: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateServiceRequestCustomFieldDto {
  name: string;
  label: string;
  fieldType: string;
  isRequired?: boolean;
  isActive?: boolean;
  sortOrder?: number;
  defaultValue?: string;
  options?: string;
  validationRegex?: string;
  helpText?: string;
  categoryId?: number;
  subcategoryId?: number;
}

// ── SLA Types ────────────────────────────────────────────────────────────────

export interface SLAPolicyDto {
  slaPolicyId: number;
  name: string;
  description?: string;
  targetType: number; // 0 = Incident, 1 = ServiceRequest
  priority?: number;
  responseTimeMinutes: number;
  resolutionTimeMinutes: number;
  isActive: boolean;
  businessHoursOnly: boolean;
  escalationEnabled: boolean;
  notifyOnBreach: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface SLAInstanceDto {
  id: number;
  slaPolicyId: number;
  targetId: number;
  targetType: number;
  responseDeadline: string;
  resolutionDeadline: string;
  isResponseBreached: boolean;
  isResolutionBreached: boolean;
  isPaused: boolean;
  pauseReason?: string;
  startedAt: string;
  respondedAt?: string;
  resolvedAt?: string;
}

// ── Service ──────────────────────────────────────────────────────────────────

export const serviceDeskConfigService = {
  // ── Categories ──────────────────────────────────────────────────────────

  /**
   * Get all service request categories.
   */
  getCategories: async (includeInactive: boolean = false): Promise<ServiceRequestCategoryDto[]> => {
    const response = await apiClient.get<ServiceRequestCategoryDto[]>(
      '/service-request-settings/categories',
      { params: { includeInactive } }
    );
    return response.data;
  },

  /**
   * Get a single category by ID.
   */
  getCategoryById: async (id: number): Promise<ServiceRequestCategoryDto> => {
    const response = await apiClient.get<ServiceRequestCategoryDto>(
      `/service-request-settings/categories/${id}`
    );
    return response.data;
  },

  /**
   * Create a new category.
   */
  createCategory: async (dto: CreateServiceRequestCategoryDto): Promise<ServiceRequestCategoryDto> => {
    const response = await apiClient.post<ServiceRequestCategoryDto>(
      '/service-request-settings/categories',
      dto
    );
    return response.data;
  },

  /**
   * Update an existing category.
   */
  updateCategory: async (
    id: number,
    dto: Partial<CreateServiceRequestCategoryDto>
  ): Promise<ServiceRequestCategoryDto> => {
    const response = await apiClient.put<ServiceRequestCategoryDto>(
      `/service-request-settings/categories/${id}`,
      dto
    );
    return response.data;
  },

  /**
   * Delete a category.
   */
  deleteCategory: async (id: number): Promise<void> => {
    await apiClient.delete(`/service-request-settings/categories/${id}`);
  },

  /**
   * Reorder categories.
   */
  reorderCategories: async (categoryIds: number[]): Promise<void> => {
    await apiClient.post('/service-request-settings/categories/reorder', categoryIds);
  },

  // ── Subcategories ──────────────────────────────────────────────────────

  /**
   * Get all subcategories (optionally filtered).
   */
  getSubcategories: async (
    includeInactive: boolean = false
  ): Promise<ServiceRequestSubcategoryDto[]> => {
    const response = await apiClient.get<ServiceRequestSubcategoryDto[]>(
      '/service-request-settings/subcategories',
      { params: { includeInactive } }
    );
    return response.data;
  },

  /**
   * Get subcategories for a specific category.
   */
  getSubcategoriesByCategory: async (
    categoryId: number,
    includeInactive: boolean = false
  ): Promise<ServiceRequestSubcategoryDto[]> => {
    const response = await apiClient.get<ServiceRequestSubcategoryDto[]>(
      `/service-request-settings/categories/${categoryId}/subcategories`,
      { params: { includeInactive } }
    );
    return response.data;
  },

  /**
   * Get a single subcategory by ID.
   */
  getSubcategoryById: async (id: number): Promise<ServiceRequestSubcategoryDto> => {
    const response = await apiClient.get<ServiceRequestSubcategoryDto>(
      `/service-request-settings/subcategories/${id}`
    );
    return response.data;
  },

  /**
   * Create a new subcategory.
   */
  createSubcategory: async (
    dto: CreateServiceRequestSubcategoryDto
  ): Promise<ServiceRequestSubcategoryDto> => {
    const response = await apiClient.post<ServiceRequestSubcategoryDto>(
      '/service-request-settings/subcategories',
      dto
    );
    return response.data;
  },

  /**
   * Update a subcategory.
   */
  updateSubcategory: async (
    id: number,
    dto: Partial<CreateServiceRequestSubcategoryDto>
  ): Promise<ServiceRequestSubcategoryDto> => {
    const response = await apiClient.put<ServiceRequestSubcategoryDto>(
      `/service-request-settings/subcategories/${id}`,
      dto
    );
    return response.data;
  },

  /**
   * Delete a subcategory.
   */
  deleteSubcategory: async (id: number): Promise<void> => {
    await apiClient.delete(`/service-request-settings/subcategories/${id}`);
  },

  /**
   * Reorder subcategories within a category.
   */
  reorderSubcategories: async (categoryId: number, subcategoryIds: number[]): Promise<void> => {
    await apiClient.post(
      `/service-request-settings/categories/${categoryId}/subcategories/reorder`,
      subcategoryIds
    );
  },

  // ── Types ──────────────────────────────────────────────────────────────

  /**
   * Get all service request types.
   */
  getTypes: async (includeInactive: boolean = false): Promise<ServiceRequestTypeDto[]> => {
    const response = await apiClient.get<ServiceRequestTypeDto[]>(
      '/service-request-settings/types',
      { params: { includeInactive } }
    );
    return response.data;
  },

  /**
   * Get types grouped by category and subcategory.
   */
  getTypesGrouped: async (
    includeInactive: boolean = false
  ): Promise<ServiceRequestTypeGroupedDto[]> => {
    const response = await apiClient.get<ServiceRequestTypeGroupedDto[]>(
      '/service-request-settings/types/grouped',
      { params: { includeInactive } }
    );
    return response.data;
  },

  /**
   * Get types by category.
   */
  getTypesByCategory: async (
    categoryId: number,
    includeInactive: boolean = false
  ): Promise<ServiceRequestTypeDto[]> => {
    const response = await apiClient.get<ServiceRequestTypeDto[]>(
      `/service-request-settings/types/by-category/${categoryId}`,
      { params: { includeInactive } }
    );
    return response.data;
  },

  /**
   * Get types by subcategory.
   */
  getTypesBySubcategory: async (
    subcategoryId: number,
    includeInactive: boolean = false
  ): Promise<ServiceRequestTypeDto[]> => {
    const response = await apiClient.get<ServiceRequestTypeDto[]>(
      `/service-request-settings/types/by-subcategory/${subcategoryId}`,
      { params: { includeInactive } }
    );
    return response.data;
  },

  /**
   * Get a single type by ID.
   */
  getTypeById: async (id: number): Promise<ServiceRequestTypeDto> => {
    const response = await apiClient.get<ServiceRequestTypeDto>(
      `/service-request-settings/types/${id}`
    );
    return response.data;
  },

  /**
   * Create a new service request type.
   */
  createType: async (dto: CreateServiceRequestTypeDto): Promise<ServiceRequestTypeDto> => {
    const response = await apiClient.post<ServiceRequestTypeDto>(
      '/service-request-settings/types',
      dto
    );
    return response.data;
  },

  /**
   * Update a service request type.
   */
  updateType: async (
    id: number,
    dto: UpdateServiceRequestTypeDto
  ): Promise<ServiceRequestTypeDto> => {
    const response = await apiClient.put<ServiceRequestTypeDto>(
      `/service-request-settings/types/${id}`,
      dto
    );
    return response.data;
  },

  /**
   * Delete a service request type.
   */
  deleteType: async (id: number): Promise<void> => {
    await apiClient.delete(`/service-request-settings/types/${id}`);
  },

  /**
   * Reorder types within a subcategory.
   */
  reorderTypes: async (subcategoryId: number, typeIds: number[]): Promise<void> => {
    await apiClient.post(
      `/service-request-settings/types/reorder/${subcategoryId}`,
      typeIds
    );
  },

  // ── Custom Fields ──────────────────────────────────────────────────────

  /**
   * Get all custom field definitions.
   */
  getCustomFields: async (
    includeInactive: boolean = false
  ): Promise<ServiceRequestCustomFieldDto[]> => {
    const response = await apiClient.get<ServiceRequestCustomFieldDto[]>(
      '/service-request-settings/custom-fields',
      { params: { includeInactive } }
    );
    return response.data;
  },

  /**
   * Get applicable custom fields for a category/subcategory context.
   */
  getApplicableCustomFields: async (
    categoryId?: number,
    subcategoryId?: number
  ): Promise<ServiceRequestCustomFieldDto[]> => {
    const response = await apiClient.get<ServiceRequestCustomFieldDto[]>(
      '/service-request-settings/custom-fields/applicable',
      { params: { categoryId, subcategoryId } }
    );
    return response.data;
  },

  /**
   * Get a single custom field by ID.
   */
  getCustomFieldById: async (id: number): Promise<ServiceRequestCustomFieldDto> => {
    const response = await apiClient.get<ServiceRequestCustomFieldDto>(
      `/service-request-settings/custom-fields/${id}`
    );
    return response.data;
  },

  /**
   * Create a custom field definition.
   */
  createCustomField: async (
    dto: CreateServiceRequestCustomFieldDto
  ): Promise<ServiceRequestCustomFieldDto> => {
    const response = await apiClient.post<ServiceRequestCustomFieldDto>(
      '/service-request-settings/custom-fields',
      dto
    );
    return response.data;
  },

  /**
   * Update a custom field definition.
   */
  updateCustomField: async (
    id: number,
    dto: Partial<CreateServiceRequestCustomFieldDto>
  ): Promise<ServiceRequestCustomFieldDto> => {
    const response = await apiClient.put<ServiceRequestCustomFieldDto>(
      `/service-request-settings/custom-fields/${id}`,
      dto
    );
    return response.data;
  },

  /**
   * Delete a custom field definition.
   */
  deleteCustomField: async (id: number): Promise<void> => {
    await apiClient.delete(`/service-request-settings/custom-fields/${id}`);
  },

  /**
   * Reorder custom fields.
   */
  reorderCustomFields: async (fieldIds: number[]): Promise<void> => {
    await apiClient.post('/service-request-settings/custom-fields/reorder', fieldIds);
  },

  /**
   * Get active custom field count and maximum allowed.
   */
  getCustomFieldCount: async (): Promise<{ activeCount: number; maxAllowed: number }> => {
    const response = await apiClient.get<{ activeCount: number; maxAllowed: number }>(
      '/service-request-settings/custom-fields/count'
    );
    return response.data;
  },

  // ── SLA Policies ───────────────────────────────────────────────────────

  /**
   * Get all SLA policies, optionally filtered by target type.
   */
  getSLAPolicies: async (targetType?: number): Promise<SLAPolicyDto[]> => {
    const response = await apiClient.get<SLAPolicyDto[]>('/itsm/sla/policies', {
      params: targetType !== undefined ? { targetType } : undefined,
    });
    return response.data;
  },

  /**
   * Get a single SLA policy by ID.
   */
  getSLAPolicyById: async (id: number): Promise<SLAPolicyDto> => {
    const response = await apiClient.get<SLAPolicyDto>(`/itsm/sla/policies/${id}`);
    return response.data;
  },

  /**
   * Create a new SLA policy.
   */
  createSLAPolicy: async (dto: SLAPolicyDto): Promise<SLAPolicyDto> => {
    const response = await apiClient.post<SLAPolicyDto>('/itsm/sla/policies', dto);
    return response.data;
  },

  /**
   * Get the active SLA instance for a target entity.
   */
  getActiveSLA: async (targetId: number, targetType: number): Promise<SLAInstanceDto | null> => {
    try {
      const response = await apiClient.get<SLAInstanceDto>(
        `/itsm/sla/instances/${targetId}/${targetType}`
      );
      return response.data;
    } catch (err: unknown) {
      const axiosError = err as { response?: { status: number } };
      if (axiosError?.response?.status === 404) return null;
      throw err;
    }
  },

  /**
   * Get all breached SLAs.
   */
  getBreachedSLAs: async (): Promise<SLAInstanceDto[]> => {
    const response = await apiClient.get<SLAInstanceDto[]>('/itsm/sla/breached');
    return response.data;
  },

  /**
   * Trigger SLA breach check.
   */
  checkSLABreaches: async (): Promise<void> => {
    await apiClient.post('/itsm/sla/check-breaches');
  },

  /**
   * Pause SLA tracking for a target.
   */
  pauseSLA: async (targetId: number, targetType: number, reason: string): Promise<void> => {
    await apiClient.post(`/itsm/sla/instances/${targetId}/${targetType}/pause`, { reason });
  },

  /**
   * Resume paused SLA tracking for a target.
   */
  resumeSLA: async (targetId: number, targetType: number): Promise<void> => {
    await apiClient.post(`/itsm/sla/instances/${targetId}/${targetType}/resume`);
  },
};

export default serviceDeskConfigService;
