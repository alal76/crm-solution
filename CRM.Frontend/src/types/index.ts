/**
 * Central Type Exports
 * Import all types from this file to maintain consistency
 * and avoid circular dependencies
 */

// Re-export from modules
export * from './common';
export * from './accounts';
export * from './sales';
export * from './itsm';
export * from './crm';
export * from './marketing';
export * from './auth';
export * from './address.types';
export * from './workflows';

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
