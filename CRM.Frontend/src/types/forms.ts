/**
 * Form-related type definitions
 * Provides type safety for form handling throughout the application
 */

/**
 * Generic form data with index signature for dynamic field access
 */
export type FormData<T = unknown> = {
  [key: string]: T;
};

/**
 * Strongly typed form data for specific entities
 */
export interface TypedFormData {
  [key: string]: string | number | boolean | null | undefined | Date | string[] | number[];
}

/**
 * Form field value types
 */
export type FieldValue = string | number | boolean | null | undefined | Date | string[] | number[];

/**
 * Form change event handler type
 */
export interface FormChangeEvent {
  target: {
    name: string;
    value: FieldValue;
    type?: string;
    checked?: boolean;
  };
}

/**
 * Form validation error
 */
export interface FormValidationError {
  field: string;
  message: string;
}

/**
 * Form state
 */
export interface FormState<T = TypedFormData> {
  data: T;
  errors: FormValidationError[];
  isDirty: boolean;
  isValid: boolean;
  isSubmitting: boolean;
}

/**
 * Form field configuration with typed options
 */
export interface FormFieldConfig {
  name: string;
  label: string;
  type: 'text' | 'email' | 'number' | 'select' | 'checkbox' | 'date' | 'textarea' | 'multiselect';
  required?: boolean;
  disabled?: boolean;
  placeholder?: string;
  helpText?: string;
  options?: Array<{ value: string | number; label: string }>;
  validation?: {
    minLength?: number;
    maxLength?: number;
    min?: number;
    max?: number;
    pattern?: RegExp;
    custom?: (value: FieldValue) => boolean;
  };
}

/**
 * Type guard to check if a value is a valid form field value
 */
export function isFieldValue(value: unknown): value is FieldValue {
  return (
    typeof value === 'string' ||
    typeof value === 'number' ||
    typeof value === 'boolean' ||
    value === null ||
    value === undefined ||
    value instanceof Date ||
    (Array.isArray(value) && value.every(v => typeof v === 'string' || typeof v === 'number'))
  );
}

/**
 * Type-safe form data getter
 */
export function getFormValue<T = FieldValue>(
  formData: TypedFormData,
  fieldName: string,
  defaultValue?: T
): T {
  const value = formData[fieldName];
  if (value === undefined || value === null) {
    return defaultValue as T;
  }
  return value as T;
}

/**
 * Type-safe form data setter
 */
export function setFormValue(
  formData: TypedFormData,
  fieldName: string,
  value: FieldValue
): TypedFormData {
  return {
    ...formData,
    [fieldName]: value,
  };
}

/**
 * Entity with dynamic fields that can be used in forms
 */
export interface DynamicEntity {
  id?: number;
  [key: string]: FieldValue;
}

/**
 * Form mapper utility type
 */
export type FormMapper<TEntity, TFormData = TypedFormData> = {
  toForm: (entity: TEntity) => TFormData;
  toEntity: (formData: TFormData) => Partial<TEntity>;
};
