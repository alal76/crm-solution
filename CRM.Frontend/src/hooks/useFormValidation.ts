/**
 * CRM Solution - Form Validation Hook
 * Copyright (C) 2024-2026 Abhishek Lal
 * 
 * Provides inline form validation with support for various validation rules.
 */

import { useState, useCallback, useMemo } from 'react';

// Validation rule types
export interface ValidationRule {
  /** Field is required */
  required?: boolean | string;
  /** Minimum length for strings */
  minLength?: number | { value: number; message: string };
  /** Maximum length for strings */
  maxLength?: number | { value: number; message: string };
  /** Regex pattern to match */
  pattern?: RegExp | { value: RegExp; message: string };
  /** Minimum value for numbers */
  min?: number | { value: number; message: string };
  /** Maximum value for numbers */
  max?: number | { value: number; message: string };
  /** Email validation */
  email?: boolean | string;
  /** URL validation */
  url?: boolean | string;
  /** Custom validation function */
  validate?: (value: any, formValues: Record<string, any>) => string | null | true;
}

export type ValidationRules<T> = {
  [K in keyof T]?: ValidationRule;
};

export interface FieldValidation {
  isValid: boolean;
  error: string | null;
  touched: boolean;
}

export interface UseFormValidationReturn<T> {
  /** Current form values */
  values: T;
  /** Set a single form value */
  setValue: (name: keyof T, value: any) => void;
  /** Set multiple form values */
  setValues: React.Dispatch<React.SetStateAction<T>>;
  /** Validation state for each field */
  validations: Record<keyof T, FieldValidation>;
  /** Handle input change event */
  handleChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => void;
  /** Handle select change event */
  handleSelectChange: (e: any) => void;
  /** Handle field blur - triggers validation */
  handleBlur: (name: keyof T) => void;
  /** Validate a single field */
  validateField: (name: keyof T) => boolean;
  /** Validate all fields */
  validateAll: () => boolean;
  /** Check if entire form is valid */
  isFormValid: boolean;
  /** Get error props for TextField */
  getFieldProps: (name: keyof T) => {
    value: any;
    onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => void;
    onBlur: () => void;
    error: boolean;
    helperText: string | null;
  };
  /** Get error props for Select fields */
  getSelectProps: (name: keyof T) => {
    value: any;
    onChange: (e: any) => void;
    onBlur: () => void;
    error: boolean;
  };
  /** Reset form to initial values */
  reset: (newValues?: T) => void;
  /** Mark all fields as touched */
  touchAll: () => void;
  /** Check if form has been modified */
  isDirty: boolean;
  /** Get list of all errors */
  getAllErrors: () => Array<{ field: keyof T; message: string }>;
}

// Email regex pattern
const EMAIL_PATTERN = /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i;
// URL regex pattern
const URL_PATTERN = /^(https?:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*\/?$/i;

/**
 * useFormValidation - Hook for form validation with inline error display
 */
export function useFormValidation<T extends Record<string, any>>(
  initialValues: T,
  rules: ValidationRules<T>
): UseFormValidationReturn<T> {
  const [values, setValues] = useState<T>(initialValues);
  const [originalValues] = useState<T>(initialValues);
  
  // Initialize validation state for all fields
  const [validations, setValidations] = useState<Record<keyof T, FieldValidation>>(() => {
    const initial: Record<string, FieldValidation> = {};
    Object.keys(initialValues).forEach(key => {
      initial[key] = { isValid: true, error: null, touched: false };
    });
    return initial as Record<keyof T, FieldValidation>;
  });

  // Validate a single field
  const validateFieldValue = useCallback((name: keyof T, value: any): string | null => {
    const rule = rules[name];
    if (!rule) return null;

    // Required check
    if (rule.required) {
      const isEmpty = value === undefined || value === null || value === '' || 
                     (Array.isArray(value) && value.length === 0);
      if (isEmpty) {
        return typeof rule.required === 'string' 
          ? rule.required 
          : `This field is required`;
      }
    }

    // Skip other validations if value is empty and not required
    if (value === undefined || value === null || value === '') {
      return null;
    }

    // String validations
    if (typeof value === 'string') {
      // Min length
      if (rule.minLength) {
        const minLen = typeof rule.minLength === 'number' ? rule.minLength : rule.minLength.value;
        const message = typeof rule.minLength === 'number' 
          ? `Must be at least ${minLen} characters`
          : rule.minLength.message;
        if (value.length < minLen) return message;
      }

      // Max length
      if (rule.maxLength) {
        const maxLen = typeof rule.maxLength === 'number' ? rule.maxLength : rule.maxLength.value;
        const message = typeof rule.maxLength === 'number'
          ? `Must be no more than ${maxLen} characters`
          : rule.maxLength.message;
        if (value.length > maxLen) return message;
      }

      // Pattern
      if (rule.pattern) {
        const pattern = rule.pattern instanceof RegExp ? rule.pattern : rule.pattern.value;
        const message = rule.pattern instanceof RegExp
          ? 'Invalid format'
          : rule.pattern.message;
        if (!pattern.test(value)) return message;
      }

      // Email
      if (rule.email) {
        if (!EMAIL_PATTERN.test(value)) {
          return typeof rule.email === 'string' ? rule.email : 'Invalid email address';
        }
      }

      // URL
      if (rule.url) {
        if (!URL_PATTERN.test(value)) {
          return typeof rule.url === 'string' ? rule.url : 'Invalid URL';
        }
      }
    }

    // Number validations
    if (typeof value === 'number' || !isNaN(Number(value))) {
      const numValue = Number(value);

      // Min
      if (rule.min !== undefined) {
        const minVal = typeof rule.min === 'number' ? rule.min : rule.min.value;
        const message = typeof rule.min === 'number'
          ? `Must be at least ${minVal}`
          : rule.min.message;
        if (numValue < minVal) return message;
      }

      // Max
      if (rule.max !== undefined) {
        const maxVal = typeof rule.max === 'number' ? rule.max : rule.max.value;
        const message = typeof rule.max === 'number'
          ? `Must be no more than ${maxVal}`
          : rule.max.message;
        if (numValue > maxVal) return message;
      }
    }

    // Custom validation
    if (rule.validate) {
      const result = rule.validate(value, values);
      if (result !== true && result !== null) {
        return result;
      }
    }

    return null;
  }, [rules, values]);

  // Validate a single field and update state
  const validateField = useCallback((name: keyof T): boolean => {
    const error = validateFieldValue(name, values[name]);
    const isValid = error === null;
    
    setValidations(prev => ({
      ...prev,
      [name]: { isValid, error, touched: true }
    }));
    
    return isValid;
  }, [validateFieldValue, values]);

  // Handle input change
  const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const inputValue = type === 'checkbox' ? (e.target as HTMLInputElement).checked : value;
    
    setValues(prev => ({ ...prev, [name]: inputValue }));
    
    // Clear error when user starts typing (if field was touched)
    const validation = validations[name as keyof T];
    if (validation?.touched && validation?.error) {
      const error = validateFieldValue(name as keyof T, inputValue);
      setValidations(prev => ({
        ...prev,
        [name]: { ...prev[name as keyof T], error, isValid: error === null }
      }));
    }
  }, [validateFieldValue, validations]);

  // Handle select change
  const handleSelectChange = useCallback((e: any) => {
    const name = e.target.name;
    const value = e.target.value;
    
    setValues(prev => ({ ...prev, [name]: value }));
    
    // Validate if field was touched
    const validation = validations[name as keyof T];
    if (validation?.touched) {
      const error = validateFieldValue(name as keyof T, value);
      setValidations(prev => ({
        ...prev,
        [name]: { ...prev[name as keyof T], error, isValid: error === null }
      }));
    }
  }, [validateFieldValue, validations]);

  // Handle blur - trigger validation
  const handleBlur = useCallback((name: keyof T) => {
    const error = validateFieldValue(name, values[name]);
    setValidations(prev => ({
      ...prev,
      [name]: { isValid: error === null, error, touched: true }
    }));
  }, [validateFieldValue, values]);

  // Validate all fields
  const validateAll = useCallback((): boolean => {
    let isAllValid = true;
    const newValidations: Record<string, FieldValidation> = {};
    
    Object.keys(values).forEach(key => {
      const error = validateFieldValue(key as keyof T, values[key as keyof T]);
      const isValid = error === null;
      newValidations[key] = { isValid, error, touched: true };
      if (!isValid) isAllValid = false;
    });
    
    setValidations(newValidations as Record<keyof T, FieldValidation>);
    return isAllValid;
  }, [validateFieldValue, values]);

  // Check if form is valid (only checked touched fields)
  const isFormValid = useMemo(() => {
    return Object.values(validations).every(v => (v as FieldValidation).isValid);
  }, [validations]);

  // Check if form is dirty
  const isDirty = useMemo(() => {
    return Object.keys(values).some(key => values[key] !== originalValues[key]);
  }, [values, originalValues]);

  // Set single value
  const setValue = useCallback((name: keyof T, value: any) => {
    setValues(prev => ({ ...prev, [name]: value }));
  }, []);

  // Reset form
  const reset = useCallback((newValues?: T) => {
    setValues(newValues || initialValues);
    const resetValidations: Record<string, FieldValidation> = {};
    Object.keys(initialValues).forEach(key => {
      resetValidations[key] = { isValid: true, error: null, touched: false };
    });
    setValidations(resetValidations as Record<keyof T, FieldValidation>);
  }, [initialValues]);

  // Mark all fields as touched
  const touchAll = useCallback(() => {
    setValidations(prev => {
      const newValidations: Record<string, FieldValidation> = {};
      Object.keys(prev).forEach(key => {
        newValidations[key] = { ...prev[key as keyof T], touched: true };
      });
      return newValidations as Record<keyof T, FieldValidation>;
    });
  }, []);

  // Get field props for TextField
  const getFieldProps = useCallback((name: keyof T) => {
    const validation = validations[name];
    return {
      value: values[name] ?? '',
      onChange: handleChange,
      onBlur: () => handleBlur(name),
      error: validation?.touched && !validation?.isValid,
      helperText: validation?.touched ? validation?.error : null,
    };
  }, [values, validations, handleChange, handleBlur]);

  // Get props for Select fields
  const getSelectProps = useCallback((name: keyof T) => {
    const validation = validations[name];
    return {
      value: values[name] ?? '',
      onChange: handleSelectChange,
      onBlur: () => handleBlur(name),
      error: validation?.touched && !validation?.isValid,
    };
  }, [values, validations, handleSelectChange, handleBlur]);

  // Get all errors
  const getAllErrors = useCallback(() => {
    const errors: Array<{ field: keyof T; message: string }> = [];
    Object.keys(validations).forEach(key => {
      const validation = validations[key as keyof T];
      if (validation.error) {
        errors.push({ field: key as keyof T, message: validation.error });
      }
    });
    return errors;
  }, [validations]);

  return {
    values,
    setValue,
    setValues,
    validations,
    handleChange,
    handleSelectChange,
    handleBlur,
    validateField,
    validateAll,
    isFormValid,
    getFieldProps,
    getSelectProps,
    reset,
    touchAll,
    isDirty,
    getAllErrors,
  };
}

export default useFormValidation;
