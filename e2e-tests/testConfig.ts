const DEFAULT_WEB_BASE_URL = 'http://192.168.0.9';
const DEFAULT_ADMIN_EMAIL = 'admin@crm.local';
const DEFAULT_ADMIN_PASSWORD = 'Admin@123';

function trimTrailingSlash(value: string): string {
  return value.replace(/\/$/, '');
}

function deriveApiBaseUrl(webBaseUrl: string): string {
  const normalizedBaseUrl = trimTrailingSlash(webBaseUrl);
  return normalizedBaseUrl.includes(':5000')
    ? normalizedBaseUrl
    : `${normalizedBaseUrl.replace(/:80$/, '')}:5000`;
}

export const WEB_BASE_URL = trimTrailingSlash(
  process.env.BASE_URL || process.env.PLAYWRIGHT_BASE_URL || DEFAULT_WEB_BASE_URL
);

export const API_BASE_URL = trimTrailingSlash(
  process.env.API_URL || deriveApiBaseUrl(WEB_BASE_URL)
);

export const ADMIN_EMAIL =
  process.env.ADMIN_EMAIL || process.env.TEST_USERNAME || DEFAULT_ADMIN_EMAIL;

export const ADMIN_PASSWORD =
  process.env.ADMIN_PASSWORD || process.env.TEST_PASSWORD || DEFAULT_ADMIN_PASSWORD;

export function appUrl(path: string): string {
  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${WEB_BASE_URL}${normalizedPath}`;
}

export function apiUrl(path: string): string {
  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${API_BASE_URL}${normalizedPath}`;
}