/**
 * Account Validation Schema
 * Defines Yup schema for account form validation matching backend rules
 */
import * as Yup from 'yup';
import apiClient from '../services/apiClient';

/**
 * Validates email uniqueness against backend
 * Excludes provided accountId from check (for updates)
 */
const validateEmailUnique = async (email: string, accountId?: number): Promise<boolean> => {
  if (!email) return true;
  try {
    const response = await apiClient.get('/accounts', {
      params: { email, pageSize: 1 }
    });
    if (response.data.items && response.data.items.length > 0) {
      // If editing and email belongs to same account, it's valid
      if (accountId && response.data.items[0].id === accountId) {
        return true;
      }
      return false;
    }
    return true;
  } catch {
    // On error, allow submission (don't block on validation service failure)
    return true;
  }
};

/**
 * Phone number validation regex: +1-234-567-8900 or variants
 */
const PHONE_REGEX = /^\+?[0-9\s\-\(\)]{10,}$/;

/**
 * Base account validation schema (for both create and update)
 */
export const createAccountValidationSchema = (accountId?: number) => {
  return Yup.object().shape({
    firstName: Yup.string()
      .required('First name is required')
      .max(100, 'First name cannot exceed 100 characters'),
    lastName: Yup.string()
      .required('Last name is required')
      .max(100, 'Last name cannot exceed 100 characters'),
    email: Yup.string()
      .required('Email is required')
      .email('Email must be a valid email address')
      .test('unique-email', 'Email address is already in use', async (value) => {
        if (!value) return true;
        return await validateEmailUnique(value, accountId);
      }),
    phone: Yup.string()
      .nullable()
      .optional()
      .test('valid-phone', 'Phone number format is invalid', (value) => {
        if (!value) return true;
        return PHONE_REGEX.test(value);
      }),
    company: Yup.string()
      .nullable()
      .optional()
      .max(255, 'Company name cannot exceed 255 characters'),
  });
};

/**
 * Validation schema for bulk import
 * Lighter validation since rows are batch-processed
 */
export const createBulkAccountValidationSchema = () => {
  return Yup.object().shape({
    firstName: Yup.string()
      .required('First name is required')
      .max(100, 'First name exceeds 100 characters'),
    lastName: Yup.string()
      .required('Last name is required')
      .max(100, 'Last name exceeds 100 characters'),
    email: Yup.string()
      .required('Email is required')
      .email('Invalid email format'),
    phone: Yup.string()
      .nullable()
      .optional()
      .test('valid-phone', 'Invalid phone format', (value) => {
        if (!value) return true;
        return PHONE_REGEX.test(value);
      }),
    company: Yup.string()
      .nullable()
      .optional()
      .max(255, 'Company exceeds 255 characters'),
    category: Yup.string().nullable(),
    industry: Yup.string().nullable(),
  });
};

/**
 * Type definitions for validated form data
 */
export interface ValidatedAccountData {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string | null;
  company?: string | null;
  [key: string]: any;
}

/**
 * Type definitions for bulk import row
 */
export interface BulkImportRow {
  rowNumber: number;
  data: Record<string, string>;
  errors: string[];
  isValid: boolean;
}
