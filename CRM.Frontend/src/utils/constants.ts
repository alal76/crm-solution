/**
 * CRM Solution - Shared Constants and Enums
 * Centralized location for constants used across multiple components
 */

// ============================================================================
// Account & Contact Constants
// ============================================================================

/**
 * Account Lifecycle Stages
 * Flow: Other (default) → Lead → Opportunity → Active → At Risk → Churned → (Win-back) → Lead
 */
export const LIFECYCLE_STAGES = [
  'Other',
  'Lead',
  'Opportunity',
  'Active',
  'AtRisk',
  'Churned',
  'WinBack',
] as const;

export type LifecycleStage = typeof LIFECYCLE_STAGES[number];

/**
 * Lifecycle stage options for dropdowns with display labels and colors
 */
export const LIFECYCLE_STAGE_OPTIONS = [
  { value: 0, key: 'Other', label: 'Other', color: '#9e9e9e', description: 'Initial default value' },
  { value: 1, key: 'Lead', label: 'Lead', color: '#2196f3', description: 'Potential account showing interest' },
  { value: 2, key: 'Opportunity', label: 'Opportunity', color: '#ff9800', description: 'Qualified lead with active sales opportunity' },
  { value: 3, key: 'Active', label: 'Active', color: '#4caf50', description: 'Active paying account' },
  { value: 4, key: 'AtRisk', label: 'At Risk', color: '#f44336', description: 'Account at risk of churning' },
  { value: 5, key: 'Churned', label: 'Churned', color: '#607d8b', description: 'Former account who stopped doing business' },
  { value: 6, key: 'WinBack', label: 'Win-back', color: '#9c27b0', description: 'Churned account being re-engaged' },
] as const;

export const ACCOUNT_TYPES = [
  'Individual',
  'Business',
  'Enterprise',
  'Government',
] as const;

export type AccountType = typeof ACCOUNT_TYPES[number];

// Legacy alias for backward compatibility
export const CUSTOMER_TYPES = ACCOUNT_TYPES;
export type CustomerType = AccountType;

export const ACCOUNT_STATUSES = [
  'Active',
  'Inactive',
  'Pending',
  'Suspended',
] as const;

export type AccountStatus = typeof ACCOUNT_STATUSES[number];

// Legacy alias for backward compatibility
export const CUSTOMER_STATUSES = ACCOUNT_STATUSES;
export type CustomerStatus = AccountStatus;

// ============================================================================
// Address Type Constants
// ============================================================================

export const ADDRESS_TYPES = [
  'Primary',
  'Billing',
  'Shipping',
  'Work',
  'Home',
  'Office',
  'Headquarters',
  'Branch',
  'Store',
  'Factory',
  'Warehouse',
  'Other',
] as const;

export type AddressType = typeof ADDRESS_TYPES[number];

export const ADDRESS_TYPE_OPTIONS = [
  { value: 0, label: 'Primary', description: 'Main address' },
  { value: 1, label: 'Billing', description: 'Billing/invoice address' },
  { value: 2, label: 'Shipping', description: 'Shipping/delivery address' },
  { value: 3, label: 'Work', description: 'Work address' },
  { value: 4, label: 'Home', description: 'Home/residence address' },
  { value: 5, label: 'Office', description: 'Office location' },
  { value: 6, label: 'Headquarters', description: 'HQ/main office' },
  { value: 7, label: 'Branch', description: 'Branch office' },
  { value: 8, label: 'Store', description: 'Retail store location' },
  { value: 9, label: 'Factory', description: 'Manufacturing facility' },
  { value: 10, label: 'Warehouse', description: 'Storage/distribution center' },
  { value: 11, label: 'Other', description: 'Other address type' },
] as const;

// ============================================================================
// Contact Method Type Constants (Email, Phone, Social)
// ============================================================================

export const CONTACT_METHOD_TYPES = [
  'Work',
  'Home',
  'Mobile',
  'Personal',
  'Other',
] as const;

export type ContactMethodType = typeof CONTACT_METHOD_TYPES[number];

export const CONTACT_METHOD_TYPE_OPTIONS = [
  { value: 0, label: 'Work', icon: '🏢' },
  { value: 1, label: 'Home', icon: '🏠' },
  { value: 2, label: 'Mobile', icon: '📱' },
  { value: 3, label: 'Personal', icon: '👤' },
  { value: 4, label: 'Other', icon: '📋' },
] as const;

export const CONTACT_PRIORITY_OPTIONS = [
  { value: 0, label: 'Primary', icon: '⭐' },
  { value: 1, label: 'Secondary', icon: '✦' },
  { value: 2, label: 'Other', icon: '○' },
] as const;

// ============================================================================
// Account Location Type Constants (for corporate accounts)
// ============================================================================

export const ACCOUNT_LOCATION_TYPES = [
  'Office',
  'Headquarters',
  'Regional HQ',
  'Branch',
  'Store',
  'Factory',
  'Warehouse',
  'Distribution Center',
  'Data Center',
  'R&D Center',
  'Other',
] as const;

export type AccountLocationType = typeof ACCOUNT_LOCATION_TYPES[number];

export const ACCOUNT_LOCATION_TYPE_OPTIONS = [
  { value: 0, label: 'Office', icon: '🏢', description: 'General office location' },
  { value: 1, label: 'Headquarters', icon: '🏛️', description: 'Main headquarters' },
  { value: 2, label: 'Regional HQ', icon: '🌍', description: 'Regional headquarters' },
  { value: 3, label: 'Branch', icon: '🏬', description: 'Branch office' },
  { value: 4, label: 'Store', icon: '🛒', description: 'Retail store' },
  { value: 5, label: 'Factory', icon: '🏭', description: 'Manufacturing plant' },
  { value: 6, label: 'Warehouse', icon: '📦', description: 'Storage warehouse' },
  { value: 7, label: 'Distribution Center', icon: '🚚', description: 'Distribution hub' },
  { value: 8, label: 'Data Center', icon: '🖥️', description: 'Data center facility' },
  { value: 9, label: 'R&D Center', icon: '🔬', description: 'Research & development' },
  { value: 10, label: 'Other', icon: '📍', description: 'Other location type' },
] as const;

// ============================================================================
// Social Media Platform Constants
// ============================================================================

export const SOCIAL_MEDIA_PLATFORMS = [
  'LinkedIn',
  'Twitter',
  'Facebook',
  'Instagram',
  'YouTube',
  'TikTok',
  'GitHub',
  'Website',
  'Other',
] as const;

export type SocialMediaPlatform = typeof SOCIAL_MEDIA_PLATFORMS[number];

export const SOCIAL_MEDIA_PLATFORM_OPTIONS = [
  { value: 0, label: 'LinkedIn', icon: '💼', urlPrefix: 'https://linkedin.com/in/' },
  { value: 1, label: 'Twitter', icon: '🐦', urlPrefix: 'https://twitter.com/' },
  { value: 2, label: 'Facebook', icon: '📘', urlPrefix: 'https://facebook.com/' },
  { value: 3, label: 'Instagram', icon: '📷', urlPrefix: 'https://instagram.com/' },
  { value: 4, label: 'YouTube', icon: '🎬', urlPrefix: 'https://youtube.com/@' },
  { value: 5, label: 'TikTok', icon: '🎵', urlPrefix: 'https://tiktok.com/@' },
  { value: 6, label: 'GitHub', icon: '💻', urlPrefix: 'https://github.com/' },
  { value: 7, label: 'Website', icon: '🌐', urlPrefix: '' },
  { value: 8, label: 'Other', icon: '🔗', urlPrefix: '' },
] as const;

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
  'Account',
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
// Note: LIFECYCLE_STAGE_OPTIONS is defined earlier in this file
// ============================================================================

export const ACCOUNT_TYPE_OPTIONS = [
  { value: 0, label: 'Individual' },
  { value: 1, label: 'Small Business' },
  { value: 2, label: 'Mid-Market' },
  { value: 3, label: 'Enterprise' },
  { value: 4, label: 'Government' },
  { value: 5, label: 'Non-Profit' },
] as const;

// Legacy alias for backward compatibility
export const CUSTOMER_TYPE_OPTIONS = ACCOUNT_TYPE_OPTIONS;

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
