/**
 * Comprehensive permission type definitions
 * Replaces loose 'as any' casts throughout the application
 */

/**
 * Permission modules in the CRM system
 */
export type PermissionModule = 
  | 'Customers'
  | 'Contacts'
  | 'Opportunities'
  | 'Quotes'
  | 'Products'
  | 'Leads'
  | 'Campaigns'
  | 'ServiceRequests'
  | 'Workflows'
  | 'Users'
  | 'UserGroups'
  | 'Settings'
  | 'Dashboard'
  | 'Reports'
  | 'Notes'
  | 'Tasks'
  | 'Accounts'
  | 'Pipelines'
  | 'Stages';

/**
 * Permission actions
 */
export type PermissionAction = 'View' | 'Create' | 'Edit' | 'Delete';

/**
 * Full permission key format: Module.Action
 */
export type PermissionKey = `${PermissionModule}.${PermissionAction}`;

/**
 * Permission record mapping permission keys to boolean values
 */
export type PermissionRecord = Partial<Record<PermissionKey, boolean>>;

/**
 * User profile with typed permissions
 */
export interface UserProfile {
  id: number;
  email: string;
  fullName: string;
  role: string;
  userGroupId?: number;
  permissions: PermissionRecord;
}

/**
 * Type-safe permission checker
 */
export function hasPermission(
  profile: UserProfile | null,
  permission: PermissionKey
): boolean {
  if (!profile) return false;
  return profile.permissions[permission] ?? false;
}

/**
 * Helper to create permission key
 */
export function createPermissionKey(
  module: PermissionModule,
  action: PermissionAction
): PermissionKey {
  return `${module}.${action}`;
}
