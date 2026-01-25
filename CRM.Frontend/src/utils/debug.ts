// Debug utility for logging - disabled in production builds
const DEBUG = process.env.NODE_ENV !== 'production';

export const debugLog = (label: string, data?: any) => {
  if (DEBUG) {
    console.log(`[CRM] ${label}`, data !== undefined ? data : '');
  }
};

export const debugError = (label: string, error?: any) => {
  // Always log errors even in production
  console.error(`[CRM ERROR] ${label}`, error !== undefined ? error : '');
};

export const debugWarn = (label: string, data?: any) => {
  if (DEBUG) {
    console.warn(`[CRM WARN] ${label}`, data !== undefined ? data : '');
  }
};

export const debugInfo = (label: string, data?: any) => {
  if (DEBUG) {
    console.info(`[CRM INFO] ${label}`, data !== undefined ? data : '');
  }
};
