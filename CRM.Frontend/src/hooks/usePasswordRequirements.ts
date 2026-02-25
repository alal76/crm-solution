/**
 * usePasswordRequirements - Hook to fetch and cache password policy from the backend.
 * TODO-SYS001-001: Align ALL password forms with backend policy.
 *
 * Fetches GET /api/auth/password-requirements once per component mount, caches
 * the result in component state, and exposes a Yup-compatible validator and a
 * plain `validate(password)` helper so every password form can derive its rules
 * dynamically instead of using hard-coded constants.
 */

import { useState, useEffect, useCallback } from 'react';
import apiClient from '../services/apiClient';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface PasswordRequirements {
  minLength: number;
  maxLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireNumbers: boolean;
  requireSpecialChars: boolean;
}

export interface PasswordValidationResult {
  valid: boolean;
  errors: string[];
  checks: {
    length: boolean;
    uppercase: boolean;
    lowercase: boolean;
    number: boolean;
    special: boolean;
  };
}

export interface UsePasswordRequirementsReturn {
  requirements: PasswordRequirements;
  isLoading: boolean;
  /** Validate a password against the fetched requirements */
  validate: (password: string) => PasswordValidationResult;
  /** Human-readable helper text lines for hint display */
  hintLines: string[];
}

// ---------------------------------------------------------------------------
// Defaults used while loading or when the API is unavailable
// ---------------------------------------------------------------------------

const DEFAULT_REQUIREMENTS: PasswordRequirements = {
  minLength: 8,
  maxLength: 128,
  requireUppercase: true,
  requireLowercase: true,
  requireNumbers: true,
  requireSpecialChars: false,
};

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------

export function usePasswordRequirements(): UsePasswordRequirementsReturn {
  const [requirements, setRequirements] = useState<PasswordRequirements>(DEFAULT_REQUIREMENTS);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    const fetchRequirements = async () => {
      try {
        const response = await apiClient.get<PasswordRequirements>('/auth/password-requirements');
        if (!cancelled) {
          setRequirements(response.data);
        }
      } catch {
        // Use defaults when API is unavailable (public pages, network issues, etc.)
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    };

    fetchRequirements();

    return () => {
      cancelled = true;
    };
  }, []);

  const validate = useCallback(
    (password: string): PasswordValidationResult => {
      const checks = {
        length:
          password.length >= requirements.minLength &&
          (requirements.maxLength === 0 || password.length <= requirements.maxLength),
        uppercase: !requirements.requireUppercase || /[A-Z]/.test(password),
        lowercase: !requirements.requireLowercase || /[a-z]/.test(password),
        number: !requirements.requireNumbers || /\d/.test(password),
        special: !requirements.requireSpecialChars || /[^A-Za-z0-9]/.test(password),
      };

      const errors: string[] = [];
      if (!checks.length) {
        if (requirements.maxLength > 0) {
          errors.push(
            `Password must be between ${requirements.minLength} and ${requirements.maxLength} characters`
          );
        } else {
          errors.push(`Password must be at least ${requirements.minLength} characters`);
        }
      }
      if (!checks.uppercase) errors.push('Password must contain at least one uppercase letter');
      if (!checks.lowercase) errors.push('Password must contain at least one lowercase letter');
      if (!checks.number) errors.push('Password must contain at least one number');
      if (!checks.special) errors.push('Password must contain at least one special character');

      return {
        valid: Object.values(checks).every(Boolean) && password.length > 0,
        errors,
        checks,
      };
    },
    [requirements]
  );

  // Build human-readable hint lines shown beside the password field
  const hintLines: string[] = [];
  if (requirements.maxLength > 0) {
    hintLines.push(
      `${requirements.minLength}–${requirements.maxLength} characters`
    );
  } else {
    hintLines.push(`At least ${requirements.minLength} characters`);
  }
  if (requirements.requireUppercase) hintLines.push('One uppercase letter');
  if (requirements.requireLowercase) hintLines.push('One lowercase letter');
  if (requirements.requireNumbers) hintLines.push('One number');
  if (requirements.requireSpecialChars) hintLines.push('One special character');

  return { requirements, isLoading, validate, hintLines };
}

export default usePasswordRequirements;
