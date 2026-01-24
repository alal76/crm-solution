/**
 * CRM Solution - Shared Constants and Enums
 * Centralized location for constants used across multiple components
 */

// ============================================================================
// Customer & Contact Constants
// ============================================================================

export const LIFECYCLE_STAGES = [
  'Lead',
  'Prospect',
  'Qualified',
  'Customer',
  'Churned',
] as const;

export type LifecycleStage = typeof LIFECYCLE_STAGES[number];

export const CUSTOMER_TYPES = [
  'Individual',
  'Business',
  'Enterprise',
  'Government',
] as const;

export type CustomerType = typeof CUSTOMER_TYPES[number];

export const CUSTOMER_STATUSES = [
  'Active',
  'Inactive',
  'Pending',
  'Suspended',
] as const;

export type CustomerStatus = typeof CUSTOMER_STATUSES[number];

// ============================================================================
// Priority & Status Constants
// ============================================================================

export const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;
export type Priority = typeof PRIORITIES[number];

export const GENERIC_STATUSES = ['Active', 'Inactive', 'Pending', 'Completed', 'Cancelled'] as const;
export type GenericStatus = typeof GENERIC_STATUSES[number];

// Priority color mapping for consistent UI
export const PRIORITY_COLORS: Record<Priority, 'default' | 'info' | 'warning' | 'error'> = {
  Low: 'default',
  Medium: 'info',
  High: 'warning',
  Critical: 'error',
};

// Status color mapping for consistent UI
export const STATUS_COLORS: Record<string, 'default' | 'success' | 'warning' | 'error' | 'info'> = {
  Active: 'success',
  Inactive: 'default',
  Pending: 'warning',
  Completed: 'success',
  Cancelled: 'error',
  Open: 'info',
  Closed: 'default',
  'In Progress': 'info',
  Resolved: 'success',
  OnHold: 'warning',
};

// ============================================================================
// Service Request Constants
// ============================================================================

export const SERVICE_REQUEST_STATUSES = [
  'Open',
  'In Progress',
  'Pending',
  'Resolved',
  'Closed',
  'OnHold',
] as const;

export type ServiceRequestStatus = typeof SERVICE_REQUEST_STATUSES[number];

export const SERVICE_REQUEST_TYPES = [
  'Support',
  'Inquiry',
  'Complaint',
  'Feature Request',
  'Bug Report',
] as const;

export type ServiceRequestType = typeof SERVICE_REQUEST_TYPES[number];

// ============================================================================
// Quote & Opportunity Constants
// ============================================================================

export const QUOTE_STATUSES = [
  'Draft',
  'Pending',
  'Sent',
  'Accepted',
  'Rejected',
  'Expired',
] as const;

export type QuoteStatus = typeof QUOTE_STATUSES[number];

export const OPPORTUNITY_STAGES = [
  'Prospecting',
  'Qualification',
  'Needs Analysis',
  'Proposal',
  'Negotiation',
  'Closed Won',
  'Closed Lost',
] as const;

export type OpportunityStage = typeof OPPORTUNITY_STAGES[number];

// ============================================================================
// Task & Activity Constants
// ============================================================================

export const TASK_STATUSES = [
  'Pending',
  'In Progress',
  'Completed',
  'Cancelled',
  'Deferred',
] as const;

export type TaskStatus = typeof TASK_STATUSES[number];

export const TASK_PRIORITIES = ['Low', 'Medium', 'High', 'Urgent'] as const;
export type TaskPriority = typeof TASK_PRIORITIES[number];

export const ACTIVITY_TYPES = [
  'Call',
  'Email',
  'Meeting',
  'Task',
  'Note',
  'Follow-up',
] as const;

export type ActivityType = typeof ACTIVITY_TYPES[number];

// ============================================================================
// Entity Types (for multi-entity features)
// ============================================================================

export const ENTITY_TYPES = [
  'Customer',
  'Contact',
  'Product',
  'Opportunity',
  'Quote',
  'ServiceRequest',
  'Task',
  'User',
] as const;

export type EntityType = typeof ENTITY_TYPES[number];

// ============================================================================
// UI Constants
// ============================================================================

export const DEFAULT_PAGE_SIZE = 10;
export const PAGE_SIZE_OPTIONS = [5, 10, 25, 50, 100] as const;

export const DATE_FORMAT = 'MM/dd/yyyy';
export const DATE_TIME_FORMAT = 'MM/dd/yyyy HH:mm';
export const TIME_FORMAT = 'HH:mm';

// Debounce delays (in ms)
export const DEBOUNCE_DELAY = {
  SEARCH: 300,
  INPUT: 150,
  RESIZE: 100,
} as const;

// ============================================================================
// Chip/Badge Variant Helpers
// ============================================================================

/**
 * Get the appropriate MUI chip color for a status value
 */
export const getStatusColor = (status: string): 'default' | 'success' | 'warning' | 'error' | 'info' => {
  return STATUS_COLORS[status] || 'default';
};

/**
 * Get the appropriate MUI chip color for a priority value
 */
export const getPriorityColor = (priority: string): 'default' | 'info' | 'warning' | 'error' => {
  return PRIORITY_COLORS[priority as Priority] || 'default';
};

// ============================================================================
// Validation Constants
// ============================================================================

export const VALIDATION = {
  EMAIL_REGEX: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
  PHONE_REGEX: /^\+?[\d\s\-()]{7,20}$/,
  URL_REGEX: /^https?:\/\/.+/,
  MAX_NAME_LENGTH: 100,
  MAX_DESCRIPTION_LENGTH: 2000,
  MAX_EMAIL_LENGTH: 254,
  MIN_PASSWORD_LENGTH: 8,
} as const;
