// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

import { AxiosError } from 'axios';

/**
 * Shape of an error response body from the CRM API.
 * Matches the backend's `ApiErrorResponse` object (see types/common.ts).
 */
interface ApiErrorBody {
  message?: string;
  error?: string;
  title?: string;          // ASP.NET Core ProblemDetails
  detail?: string;         // ASP.NET Core ProblemDetails detail field
}

/**
 * Extracts a human-readable error message from any thrown value.
 *
 * Priority order:
 *  1. `response.data.message` from an Axios error (CRM API standard field)
 *  2. `response.data.title` or `.detail` (ASP.NET Core ProblemDetails)
 *  3. `error.message` from Axios (e.g. "Network Error", "timeout of Xms exceeded")
 *  4. `error.message` from a plain `Error`
 *  5. Generic fallback string
 *
 * @example
 * catch (err) {
 *   setError(getApiErrorMessage(err));
 * }
 */
export function getApiErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const body = error.response?.data as ApiErrorBody | undefined;
    return (
      body?.message ??
      body?.detail ??
      body?.title ??
      body?.error ??
      error.message ??
      'An error occurred'
    );
  }
  if (error instanceof Error) {
    return error.message;
  }
  return 'An unexpected error occurred';
}

/**
 * Standard catch-block handler for React components and service wrappers.
 *
 * Extracts a human-readable message with `getApiErrorMessage`, logs it to the
 * console (with optional context label), and passes it to the provided setter.
 *
 * @param error   - The caught value (usually `unknown` in TypeScript 4+).
 * @param setError - React state setter or any callback that receives the message.
 * @param context  - Optional label prepended to the console log, e.g. `'[LeadsPage]'`.
 *
 * @example
 * // In a component:
 * catch (err) {
 *   handleApiError(err, setError, 'LeadsPage.loadLeads');
 * }
 *
 * @example
 * // With useLoadingState — error is already handled internally, but you can
 * // still use getApiErrorMessage in execute's catch if you need custom logic.
 */
export function handleApiError(
  error: unknown,
  setError: (message: string) => void,
  context?: string,
): void {
  const message = getApiErrorMessage(error);
  if (context) {
    console.error(`[${context}] ${message}`, error);
  } else {
    console.error(message, error);
  }
  setError(message);
}
