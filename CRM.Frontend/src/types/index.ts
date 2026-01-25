/**
 * Base entity interface that matches the backend BaseEntity class.
 * All domain entities should extend this interface.
 */
export interface BaseEntity {
  id: number;
  createdAt?: string;
  updatedAt?: string | null;
  isDeleted?: boolean;
}

/**
 * Audit metadata for entities - use for display components
 */
export interface AuditMetadata {
  createdAt?: string;
  updatedAt?: string | null;
  createdBy?: string;
  updatedBy?: string;
}

/**
 * Pagination request parameters
 */
export interface PaginationRequest {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

/**
 * Paginated response wrapper
 */
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Standard API response wrapper
 */
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

/**
 * Select option for dropdowns
 */
export interface SelectOption {
  value: string | number;
  label: string;
}

/**
 * Form field configuration (matches backend ModuleFieldConfiguration)
 */
export interface FieldConfiguration {
  id: number;
  moduleName: string;
  fieldName: string;
  fieldLabel: string;
  fieldType: string;
  isRequired: boolean;
  isEnabled: boolean;
  isReadOnly: boolean;
  displayOrder: number;
  tabName?: string;
  tabIndex?: number;
  groupName?: string;
  defaultValue?: string;
  placeholder?: string;
  helpText?: string;
  validationRules?: string;
  lookupSource?: string;
  maxLength?: number;
  minValue?: number;
  maxValue?: number;
}
