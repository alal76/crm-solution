// Debug utility for logging
const DEBUG = process.env.NODE_ENV === 'development' || true;

export const debugLog = (label: string, data?: any) => {
  if (DEBUG) {
    console.log(`[CRM DEBUG] ${label}`, data || '');
  }
};

export const debugError = (label: string, error?: any) => {
  if (DEBUG) {
    console.error(`[CRM ERROR] ${label}`, error || '');
  }
};

export const debugWarn = (label: string, data?: any) => {
  if (DEBUG) {
    console.warn(`[CRM WARN] ${label}`, data || '');
  }
};
