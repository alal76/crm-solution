/**
 * Standalone formatting utilities.
 *
 * These are used in places that cannot easily access the SettingsContext
 * (e.g. helper functions outside React trees, class components, utility files).
 *
 * Components inside the React tree should prefer `const { formatCurrency } = useSettings()`
 * so they automatically use the org-configured default currency.
 */

/**
 * Format a monetary amount.
 *
 * @param amount  - The numeric amount to format (null/undefined → '-')
 * @param currency - ISO 4217 currency code, e.g. "USD", "EUR".
 *                   Falls back to "USD" when null/undefined/empty.
 */
export function formatCurrency(
  amount: number | null | undefined,
  currency?: string | null
): string {
  if (amount == null) return '-';
  const currencyCode = currency || 'USD';
  try {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode,
    }).format(amount);
  } catch {
    // Invalid currency code — render plain number with code prefix
    return `${currencyCode} ${amount.toFixed(2)}`;
  }
}

/**
 * Format a percentage value.
 * @param value  - Raw ratio (e.g. 0.15 → "15.0%") or already-percent number.
 * @param asRatio - If true, multiplies by 100 first. Default: true.
 */
export function formatPercent(
  value: number | null | undefined,
  asRatio = true
): string {
  if (value == null) return '-';
  const pct = asRatio ? value * 100 : value;
  return `${pct.toFixed(1)}%`;
}

/**
 * Format a date string or Date object into a locale date string.
 * @param date - The date to format (null/undefined → '-')
 */
export function formatDate(date: string | Date | null | undefined): string {
  if (!date) return '-';
  try {
    return new Date(date).toLocaleDateString();
  } catch {
    return '-';
  }
}

/**
 * Format a date-time string or Date object.
 * @param date - The date to format (null/undefined → '-')
 */
export function formatDateTime(date: string | Date | null | undefined): string {
  if (!date) return '-';
  try {
    return new Date(date).toLocaleString();
  } catch {
    return '-';
  }
}
