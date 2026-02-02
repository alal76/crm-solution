// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// useDuplicateDetection Hook - Reusable hook for duplicate detection in forms

import { useState, useCallback, useRef } from 'react';
import {
  DuplicateCheckResult,
  DuplicateCheckRequest,
  checkForDuplicates,
  buildFieldValuesFromLead,
  buildFieldValuesFromContact,
  buildFieldValuesFromAccount,
} from '../services/duplicateService';

export interface UseDuplicateDetectionOptions {
  entityType: 'Lead' | 'Contact' | 'Account';
  debounceMs?: number;
  autoCheck?: boolean;
  matchThreshold?: number;
  excludeRecordId?: number;
}

export interface UseDuplicateDetectionResult {
  checkResult: DuplicateCheckResult | null;
  isChecking: boolean;
  error: string | null;
  showDialog: boolean;
  triggerCheck: (formData: Record<string, any>) => Promise<DuplicateCheckResult | null>;
  clearResult: () => void;
  openDialog: () => void;
  closeDialog: () => void;
}

/**
 * Custom hook for integrating duplicate detection into entity forms
 */
export function useDuplicateDetection(
  options: UseDuplicateDetectionOptions
): UseDuplicateDetectionResult {
  const { entityType, debounceMs = 500, matchThreshold = 70, excludeRecordId } = options;

  const [checkResult, setCheckResult] = useState<DuplicateCheckResult | null>(null);
  const [isChecking, setIsChecking] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showDialog, setShowDialog] = useState(false);

  const debounceTimer = useRef<NodeJS.Timeout | null>(null);
  const lastCheckRef = useRef<string | null>(null);

  /**
   * Build field values based on entity type
   */
  const buildFieldValues = useCallback(
    (formData: Record<string, any>): Record<string, string | null> => {
      switch (entityType) {
        case 'Lead':
          return buildFieldValuesFromLead(formData);
        case 'Contact':
          return buildFieldValuesFromContact(formData);
        case 'Account':
          return buildFieldValuesFromAccount(formData);
        default:
          return {};
      }
    },
    [entityType]
  );

  /**
   * Trigger duplicate check
   */
  const triggerCheck = useCallback(
    async (formData: Record<string, any>): Promise<DuplicateCheckResult | null> => {
      // Clear any pending debounced check
      if (debounceTimer.current) {
        clearTimeout(debounceTimer.current);
      }

      const fieldValues = buildFieldValues(formData);

      // Create a signature for this check to prevent duplicate requests
      const checkSignature = JSON.stringify(fieldValues);
      if (checkSignature === lastCheckRef.current) {
        return checkResult;
      }

      // Check if we have enough data to check for duplicates
      const hasData = Object.values(fieldValues).some((v) => v && v.trim() !== '');
      if (!hasData) {
        return null;
      }

      setIsChecking(true);
      setError(null);
      lastCheckRef.current = checkSignature;

      try {
        const request: DuplicateCheckRequest = {
          entityType,
          fieldValues,
          excludeRecordId,
          matchThreshold,
        };

        const result = await checkForDuplicates(request);
        setCheckResult(result);

        // Auto-show dialog if duplicates found
        if (result.hasDuplicates) {
          setShowDialog(true);
        }

        return result;
      } catch (err: any) {
        setError(err.message || 'Failed to check for duplicates');
        return null;
      } finally {
        setIsChecking(false);
      }
    },
    [entityType, excludeRecordId, matchThreshold, buildFieldValues, checkResult]
  );

  /**
   * Debounced check - useful for auto-checking as user types
   */
  const debouncedCheck = useCallback(
    (formData: Record<string, any>) => {
      if (debounceTimer.current) {
        clearTimeout(debounceTimer.current);
      }

      debounceTimer.current = setTimeout(() => {
        triggerCheck(formData);
      }, debounceMs);
    },
    [triggerCheck, debounceMs]
  );

  /**
   * Clear the check result
   */
  const clearResult = useCallback(() => {
    setCheckResult(null);
    setError(null);
    lastCheckRef.current = null;
  }, []);

  /**
   * Open the duplicate detection dialog
   */
  const openDialog = useCallback(() => {
    setShowDialog(true);
  }, []);

  /**
   * Close the duplicate detection dialog
   */
  const closeDialog = useCallback(() => {
    setShowDialog(false);
  }, []);

  return {
    checkResult,
    isChecking,
    error,
    showDialog,
    triggerCheck,
    clearResult,
    openDialog,
    closeDialog,
  };
}

export default useDuplicateDetection;
