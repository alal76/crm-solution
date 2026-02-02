/**
 * Entity type definitions for common CRM entities
 * Provides strong typing to replace 'as any' casts
 */

import { BaseEntity } from './index';

/**
 * Base entity with common fields
 */
export interface Entity extends BaseEntity {
  createdBy?: string;
  updatedBy?: string;
}

/**
 * Customer entity
 */
export interface Customer extends Entity {
  company?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  industry?: string;
  annualRevenue?: number;
  numberOfEmployees?: number;
  website?: string;
  status?: string;
  type?: string;
  source?: string;
  rating?: string;
  assignedToUserId?: number;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
}

/**
 * Contact entity
 */
export interface Contact extends Entity {
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  title?: string;
  department?: string;
  customerId?: number;
  isPrimary?: boolean;
  status?: string;
}

/**
 * Opportunity entity
 */
export interface Opportunity extends Entity {
  name: string;
  customerId?: number;
  amount?: number;
  probability?: number;
  stage?: string;
  expectedCloseDate?: string;
  actualCloseDate?: string;
  type?: string;
  source?: string;
  assignedToUserId?: number;
  pipelineId?: number;
  stageId?: number;
}

/**
 * Lead entity
 */
export interface Lead extends Entity {
  firstName?: string;
  lastName?: string;
  company?: string;
  email?: string;
  phone?: string;
  status?: string;
  source?: string;
  rating?: string;
  assignedToUserId?: number;
  convertedToCustomerId?: number;
  convertedAt?: string;
}

/**
 * Product entity
 */
export interface Product extends Entity {
  name: string;
  code?: string;
  description?: string;
  category?: string;
  price?: number;
  cost?: number;
  isActive?: boolean;
  unit?: string;
  stockQuantity?: number;
}

/**
 * Quote entity
 */
export interface Quote extends Entity {
  quoteNumber?: string;
  customerId?: number;
  opportunityId?: number;
  totalAmount?: number;
  status?: string;
  validUntil?: string;
  expirationDate?: string;
  assignedToUserId?: number;
}

/**
 * Service Request entity
 */
export interface ServiceRequest extends Entity {
  title: string;
  description?: string;
  customerId?: number;
  contactId?: number;
  status?: string;
  priority?: string;
  categoryId?: number;
  subcategoryId?: number;
  assignedToUserId?: number;
  dueDate?: string;
  resolvedAt?: string;
}

/**
 * Campaign entity
 */
export interface Campaign extends Entity {
  name: string;
  type?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  budget?: number;
  actualCost?: number;
  expectedRevenue?: number;
  description?: string;
}

/**
 * User entity
 */
export interface User extends Entity {
  email: string;
  fullName: string;
  firstName?: string;
  lastName?: string;
  role?: string;
  isActive?: boolean;
  userGroupId?: number;
  lastLoginAt?: string;
}

/**
 * User Group entity
 */
export interface UserGroup extends Entity {
  name: string;
  description?: string;
  isActive?: boolean;
}

/**
 * Note entity
 */
export interface Note extends Entity {
  content: string;
  entityType?: string;
  entityId?: number;
  userId?: number;
  isPrivate?: boolean;
}

/**
 * Task entity
 */
export interface Task extends Entity {
  title: string;
  description?: string;
  dueDate?: string;
  status?: string;
  priority?: string;
  assignedToUserId?: number;
  entityType?: string;
  entityId?: number;
  completedAt?: string;
}

/**
 * Generic entity with ID for filtering operations
 */
export interface EntityWithId {
  id: number;
  [key: string]: unknown;
}

/**
 * Type guard to check if an object is an EntityWithId
 */
export function isEntityWithId(obj: unknown): obj is EntityWithId {
  return (
    typeof obj === 'object' &&
    obj !== null &&
    'id' in obj &&
    typeof (obj as Record<string, unknown>).id === 'number'
  );
}

/**
 * Type guard to check if an object is a Customer
 */
export function isCustomer(obj: unknown): obj is Customer {
  return (
    isEntityWithId(obj) &&
    ('company' in obj || 'firstName' in obj || 'lastName' in obj)
  );
}

/**
 * Type guard to check if an object is a Contact
 */
export function isContact(obj: unknown): obj is Contact {
  return (
    isEntityWithId(obj) &&
    'firstName' in obj &&
    'lastName' in obj
  );
}

/**
 * Union type for all entities
 */
export type AnyEntity =
  | Customer
  | Contact
  | Opportunity
  | Lead
  | Product
  | Quote
  | ServiceRequest
  | Campaign
  | User
  | UserGroup
  | Note
  | Task;

/**
 * Extract entity type name
 */
export type EntityTypeName =
  | 'Customer'
  | 'Contact'
  | 'Opportunity'
  | 'Lead'
  | 'Product'
  | 'Quote'
  | 'ServiceRequest'
  | 'Campaign'
  | 'User'
  | 'UserGroup'
  | 'Note'
  | 'Task';
