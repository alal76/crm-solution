/**
 * ENUM-FE-001: Type definitions for the Configurable Enums feature.
 * These types define the contracts for enum categories, values, transitions,
 * and DTOs used by enumService and enumCacheService.
 */

export interface EnumCategory {
  id: number;
  name: string;
  displayName?: string;
  description?: string;
  entityType?: string;
  propertyName?: string;
  isSystemManaged: boolean;
  allowCustomValues: boolean;
  valueCount?: number;
}

export interface EnumValue {
  id: number;
  categoryId: number;
  categoryName?: string;
  key: string;
  label: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
  isDefault: boolean;
  isSystemValue: boolean;
  color?: string;
  icon?: string;
  metadata?: Record<string, unknown>;
}

export interface EnumMetadata {
  probability?: number;
  slaHours?: number;
  [key: string]: unknown;
}

export interface EnumTransition {
  id: number;
  categoryId: number;
  fromValueId?: number;
  fromValueLabel?: string;
  toValueId: number;
  toValueLabel?: string;
  isAllowed: boolean;
  requiresApproval: boolean;
  allowedRoles?: string;
}

export interface CreateEnumValueDto {
  key: string;
  label: string;
  description?: string;
  color?: string;
  icon?: string;
  metadata?: string;
  isDefault?: boolean;
}

export interface UpdateEnumValueDto {
  key?: string;
  label: string;
  description?: string;
  color?: string;
  icon?: string;
  metadata?: string;
  isActive: boolean;
  isDefault: boolean;
  sortOrder: number;
}

export interface EnumValidationResult {
  isValid: boolean;
  errorMessage?: string;
  warningMessage?: string;
}
