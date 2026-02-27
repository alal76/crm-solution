/**
 * CRM Solution - Configurable Enums TypeScript Types
 * Phase 3: Frontend Implementation (SPEC-GEN-002)
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
  validationSchema?: string;
  isActive: boolean;
  valuesCount?: number; // populated by API
  createdAt: string;
  updatedAt: string;
}

export interface EnumValue {
  id: number;
  categoryId: number;
  key: string;
  label: string;
  description?: string;
  sortOrder: number;
  isDefault: boolean;
  isSystemValue: boolean;
  color?: string; // hex color #RRGGBB
  icon?: string; // Material-UI icon name
  meta?: string; // JSON metadata
  validationRules?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEnumValueDto {
  categoryId: number;
  key: string;
  label: string;
  description?: string;
  color?: string;
  icon?: string;
  meta?: string;
}

export interface UpdateEnumValueDto {
  label?: string;
  description?: string;
  color?: string;
  icon?: string;
  meta?: string;
  isActive?: boolean;
}

export interface EnumTransition {
  id: number;
  categoryId: number;
  fromValueId?: number;
  toValueId: number;
  isAllowed: boolean;
  requiresApproval: boolean;
  allowedRoles?: string;
  validationExpression?: string;
}

export interface EnumValidationResult {
  isValid: boolean;
  errors: string[];
}

export interface EnumMetadata {
  probability?: number; // for OpportunityStage
  slaHours?: number; // for ServiceRequestPriority
  emailTemplate?: string;
  webhookUrl?: string;
  [key: string]: any;
}
