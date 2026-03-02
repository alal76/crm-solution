/**
 * CRM Solution — Date Utility Functions
 *
 * Architecture Recommendation #3: Standardize date handling across the application.
 *
 * CONVENTIONS:
 * - All API dates are ISO 8601 strings (e.g. "2026-02-21T00:00:00Z")
 * - Date-only fields use the "YYYY-MM-DD" portion (split on 'T')[0])
 * - All UI display uses locale-aware formatting via toLocaleDateString()
 * - Never store or compare raw Date objects — always use string representations
 */

/**
 * Parse a raw API date string and format it for display.
 * Returns '-' for null/empty values.
 */
export function formatDate(dateString: string | null | undefined): string {
  if (!dateString) return '-';
  const d = new Date(dateString);
  return Number.isNaN(d.getTime()) ? '-' : d.toLocaleDateString();
}

/**
 * Parse a raw API datetime string and format it with time for display.
 * Returns '-' for null/empty values.
 */
export function formatDateTime(dateString: string | null | undefined): string {
  if (!dateString) return '-';
  const d = new Date(dateString);
  return Number.isNaN(d.getTime()) ? '-' : d.toLocaleString();
}

/**
 * Extract the date-only part (YYYY-MM-DD) from an ISO 8601 string.
 * Safe to use as the value of a <input type="date" />.
 * Returns empty string for null/empty values.
 */
export function toDateInputValue(dateString: string | null | undefined): string {
  if (!dateString) return '';
  return dateString.split('T')[0] ?? '';
}

/**
 * Format a number as USD currency.
 * Returns '$0.00' for null/undefined/NaN values.
 */
export function formatCurrency(value: number | null | undefined): string {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value ?? 0);
}

/**
 * Returns today's date as a YYYY-MM-DD string — safe for <input type="date" /> default.
 */
export function todayAsInputValue(): string {
  return new Date().toISOString().split('T')[0];
}

/**
 * Returns true if dateString is in the past (relative to now).
 * Null/empty dates are not considered past.
 */
export function isPastDate(dateString: string | null | undefined): boolean {
  if (!dateString) return false;
  return new Date(dateString) < new Date();
}
