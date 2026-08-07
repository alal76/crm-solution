/**
 * Common TypeScript Types and Interfaces
 * Shared types used across the CRM application
 * 
 * CRITICAL: All API responses and domain models are typed here to ensure
 * TypeScript strict mode compilation succeeds with 0 errors
 */

/**
 * Generic paginated response from API
 * Used for all list endpoints: /api/accounts, /api/contacts, etc.
 */
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Generic API response wrapper
 * Some endpoints may return this structure
 */
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

/**
 * Error response from API
 * Matches backend error structure
 */
export interface ApiErrorResponse {
  message: string;
  error?: string;
  statusCode?: number;
  errorCode?: string;
  details?: Record<string, string[]>;
}

/**
 * Base entity properties common to all domain models
 * All entities in the database inherit from BaseEntity
 */
export interface BaseEntity {
  id: number;
  createdAt: string; // ISO 8601 datetime string
  updatedAt: string; // ISO 8601 datetime string
  isDeleted?: boolean; // Soft delete flag
}

/**
 * User identity information
 */
export interface User extends BaseEntity {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  avatar?: string;
  status?: 'active' | 'inactive' | 'locked';
  lastLogin?: string;
  roles?: string[];
  groups?: string[];

  // Fields mirroring UserDto (CRM.Backend/src/CRM.Core/Dtos/UserDto.cs)
  username?: string;
  role?: string;
  isActive?: boolean;
  isLocked?: boolean;
  isApiUser?: boolean;
  apiKeyPrefix?: string;
  apiKeyCreatedAt?: string;
  apiKeyLastUsedAt?: string;
  apiKeyExpiresAt?: string;
  apiUserDescription?: string;
  departmentId?: number;
  departmentName?: string;
  userProfileId?: number;
  userProfileName?: string;
  primaryGroupId?: number;
  primaryGroupName?: string;
  contactId?: number;
  contactName?: string;
  contactEmail?: string;
  lastLoginDate?: string;
  headerColor?: string;
  photoUrl?: string;

  // Security / account status
  twoFactorEnabled?: boolean;
  passwordLastChangedAt?: string;
  mustResetPassword?: boolean;
  emailVerified?: boolean;
  passwordNeverSet?: boolean;
  commissionPlanId?: number;

  // Preferences
  themePreference?: string;
  language?: string;
  timezone?: string;
  dateFormat?: string;
  timeFormat?: string;
  rowsPerPage?: number;
  emailNotifications?: boolean;
  desktopNotifications?: boolean;
  compactMode?: boolean;
}

/**
 * Authenticated user with additional session info
 */
export interface AuthUser extends User {
  accessToken: string;
  refreshToken: string;
  expiresIn: number; // Seconds
}

/**
 * Authentication response
 */
export interface AuthResponse {
  user: AuthUser;
  token: string;
  refreshToken: string;
  expiresIn: number;
}

/**
 * Login request payload
 */
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

/**
 * Dashboard metrics and statistics
 */
export interface DashboardMetric {
  label: string;
  value: number | string;
  change?: number; // Percentage change
  trend?: 'up' | 'down' | 'stable';
  icon?: string;
  color?: string;
}

/**
 * Chart data point
 */
export interface ChartDataPoint {
  name: string;
  value: number;
  [key: string]: string | number;
}

/**
 * Validation error field
 */
export interface ValidationError {
  field: string;
  message: string;
}

/**
 * Address information (polymorphic - attached to multiple entity types)
 */
export interface Address extends BaseEntity {
  street1: string;
  street2?: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  addressType?: 'billing' | 'shipping' | 'physical';
  isDefault?: boolean;
}

/**
 * Phone number (polymorphic - attached to multiple entity types)
 */
export interface PhoneNumber extends BaseEntity {
  number: string;
  type?: 'mobile' | 'phone' | 'fax' | 'other';
  isDefault?: boolean;
  label?: string;
}

/**
 * Email address (polymorphic - attached to multiple entity types)
 */
export interface EmailAddress extends BaseEntity {
  address: string;
  type?: 'primary' | 'secondary' | 'personal' | 'business';
  isDefault?: boolean;
  label?: string;
}

/**
 * Social media account (polymorphic - attached to multiple entity types)
 */
export interface SocialMediaAccount extends BaseEntity {
  platform: string; // 'linkedin', 'twitter', 'facebook', 'instagram'
  handle: string;
  url?: string;
}

/**
 * Interaction/Activity record
 */
export interface Interaction extends BaseEntity {
  type: 'email' | 'call' | 'meeting' | 'task' | 'note' | 'social' | 'other';
  title: string;
  description?: string;
  entityType: 'Account' | 'Contact' | 'Lead' | 'Opportunity';
  entityId: number;
  relatedEntityType?: string;
  relatedEntityId?: number;
  duration?: number; // Minutes
  direction?: 'inbound' | 'outbound'; // For calls/emails
  status?: 'completed' | 'pending' | 'canceled';
  scheduledDate?: string;
  completedDate?: string;
  ownerId?: number;
  ownerName?: string;
  participants?: string[];
}

/**
 * File/Attachment record
 */
export interface Attachment extends BaseEntity {
  fileName: string;
  contentType: string;
  fileSize: number; // Bytes
  entityType: string;
  entityId: number;
  url?: string;
  uploadedBy?: string;
}

/**
 * Note/Comment record
 */
export interface Note extends BaseEntity {
  content: string;
  entityType: 'Account' | 'Contact' | 'Lead' | 'Opportunity' | 'Order' | 'ServiceRequest';
  entityId: number;
  createdBy?: string;
  updatedBy?: string;
  isPublic?: boolean;
}

/**
 * Relationship between entities
 */
export interface Relationship extends BaseEntity {
  fromEntityType: string;
  fromEntityId: number;
  toEntityType: string;
  toEntityId: number;
  relationshipType: string; // 'parent', 'related', 'connected'
}

/**
 * Enum for common statuses
 */
export enum CommonStatus {
  Active = 'active',
  Inactive = 'inactive',
  Pending = 'pending',
  Archived = 'archived',
  Deleted = 'deleted'
}

/**
 * Sorting configuration
 */
export interface SortConfig {
  field: string;
  direction: 'asc' | 'desc';
}

/**
 * Filter configuration
 */
export interface FilterConfig {
  field: string;
  operator: 'eq' | 'ne' | 'gt' | 'gte' | 'lt' | 'lte' | 'contains' | 'startsWith' | 'in';
  value: string | number | boolean | (string | number)[];
}

/**
 * Pagination parameters
 */
export interface PaginationParams {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * API request options
 */
export interface ApiRequestOptions {
  params?: Record<string, any>;
  headers?: Record<string, string>;
  timeout?: number;
  retryCount?: number;
}

/**
 * Notification payload
 */
export interface NotificationPayload {
  id?: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  title?: string;
  action?: {
    label: string;
    handler: () => void;
  };
  autoClose?: boolean;
  duration?: number; // Milliseconds
}

/**
 * Real-time update notification
 */
export interface RealtimeUpdateNotification<T> {
  eventType: 'created' | 'updated' | 'deleted';
  entityType: string;
  entityId: number;
  data: T;
  timestamp: string;
  userId?: number;
}
