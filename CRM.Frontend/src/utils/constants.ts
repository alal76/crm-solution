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

// ============================================================================
// Backend Enum Value Options (matching backend numeric enums)
// These are used in forms and tables where numeric values are required
// ============================================================================

export const LIFECYCLE_STAGE_OPTIONS = [
  { value: 0, label: 'Lead', color: '#9e9e9e' },
  { value: 1, label: 'Prospect', color: '#2196f3' },
  { value: 2, label: 'Opportunity', color: '#ff9800' },
  { value: 3, label: 'Customer', color: '#4caf50' },
  { value: 4, label: 'Churned', color: '#f44336' },
  { value: 5, label: 'Reactivated', color: '#9c27b0' },
] as const;

export const CUSTOMER_TYPE_OPTIONS = [
  { value: 0, label: 'Individual' },
  { value: 1, label: 'Small Business' },
  { value: 2, label: 'Mid-Market' },
  { value: 3, label: 'Enterprise' },
  { value: 4, label: 'Government' },
  { value: 5, label: 'Non-Profit' },
] as const;

export const PRIORITY_OPTIONS = [
  { value: 0, label: 'Low', color: '#9e9e9e' },
  { value: 1, label: 'Medium', color: '#2196f3' },
  { value: 2, label: 'High', color: '#ff9800' },
  { value: 3, label: 'Critical', color: '#f44336' },
] as const;

export const TASK_STATUS_OPTIONS = [
  { value: 0, label: 'Not Started', color: '#9e9e9e' },
  { value: 1, label: 'In Progress', color: '#2196f3' },
  { value: 2, label: 'Completed', color: '#4caf50' },
  { value: 3, label: 'Deferred', color: '#ff9800' },
  { value: 4, label: 'Waiting', color: '#607d8b' },
  { value: 5, label: 'Cancelled', color: '#f44336' },
] as const;

export const TASK_TYPE_OPTIONS = [
  { value: 0, label: 'Call', icon: '📞' },
  { value: 1, label: 'Email', icon: '📧' },
  { value: 2, label: 'Meeting', icon: '📅' },
  { value: 3, label: 'Follow-Up', icon: '🔄' },
  { value: 4, label: 'Demo', icon: '🖥️' },
  { value: 5, label: 'Proposal', icon: '📄' },
  { value: 6, label: 'Research', icon: '🔍' },
  { value: 7, label: 'Documentation', icon: '📝' },
  { value: 8, label: 'Review', icon: '✅' },
  { value: 9, label: 'Approval', icon: '👍' },
  { value: 10, label: 'Other', icon: '📋' },
] as const;

export const TASK_PRIORITY_OPTIONS = [
  { value: 0, label: 'Low', color: '#9e9e9e' },
  { value: 1, label: 'Normal', color: '#2196f3' },
  { value: 2, label: 'High', color: '#ff9800' },
  { value: 3, label: 'Urgent', color: '#f44336' },
] as const;

export const QUOTE_STATUS_OPTIONS = [
  { value: 0, label: 'Draft', color: '#9e9e9e' },
  { value: 1, label: 'Pending', color: '#ff9800' },
  { value: 2, label: 'Sent', color: '#2196f3' },
  { value: 3, label: 'Accepted', color: '#4caf50' },
  { value: 4, label: 'Rejected', color: '#f44336' },
  { value: 5, label: 'Expired', color: '#795548' },
] as const;

export const CAMPAIGN_STATUS_OPTIONS = [
  { value: 0, label: 'Draft', color: '#9e9e9e' },
  { value: 1, label: 'Scheduled', color: '#2196f3' },
  { value: 2, label: 'Active', color: '#4caf50' },
  { value: 3, label: 'Paused', color: '#ff9800' },
  { value: 4, label: 'Completed', color: '#9c27b0' },
  { value: 5, label: 'Cancelled', color: '#f44336' },
  { value: 6, label: 'Archived', color: '#607d8b' },
] as const;

export const CAMPAIGN_TYPE_OPTIONS = [
  { value: 0, label: 'Email', icon: '📧' },
  { value: 1, label: 'Social Media', icon: '📱' },
  { value: 2, label: 'Paid Search', icon: '🔍' },
  { value: 3, label: 'Display Ads', icon: '🖼️' },
  { value: 4, label: 'Content Marketing', icon: '📝' },
  { value: 5, label: 'SEO', icon: '🔎' },
  { value: 6, label: 'Events', icon: '🎪' },
  { value: 7, label: 'Webinar', icon: '💻' },
  { value: 8, label: 'Trade Show', icon: '🏢' },
  { value: 9, label: 'Direct Mail', icon: '✉️' },
  { value: 10, label: 'Referral', icon: '👥' },
  { value: 11, label: 'Partner', icon: '🤝' },
  { value: 12, label: 'PR', icon: '📰' },
  { value: 13, label: 'Video', icon: '🎬' },
  { value: 14, label: 'Podcast', icon: '🎙️' },
  { value: 15, label: 'Other', icon: '📋' },
] as const;

export const CONTACT_ROLE_OPTIONS = [
  { value: 0, label: 'Primary' },
  { value: 1, label: 'Secondary' },
  { value: 2, label: 'Billing' },
  { value: 3, label: 'Technical' },
  { value: 4, label: 'Decision Maker' },
  { value: 5, label: 'Influencer' },
  { value: 6, label: 'End User' },
  { value: 7, label: 'Executive' },
  { value: 8, label: 'Procurement' },
  { value: 9, label: 'Other' },
] as const;

// ============================================================================
// Helper Functions for Enum Options
// ============================================================================

/**
 * Get option by value from an options array
 */
export const getOptionByValue = <T extends { value: number; label: string }>(
  options: readonly T[],
  value: number | undefined
): T | undefined => options.find(opt => opt.value === value);

/**
 * Get label by value from an options array
 */
export const getLabelByValue = <T extends { value: number; label: string }>(
  options: readonly T[],
  value: number | undefined
): string => getOptionByValue(options, value)?.label || '';

/**
 * Get color by value from an options array (if color exists)
 */
export const getColorByValue = <T extends { value: number; color?: string }>(
  options: readonly T[],
  value: number | undefined
): string | undefined => {
  const opt = options.find(o => o.value === value);
  return opt && 'color' in opt ? opt.color : undefined;
};
