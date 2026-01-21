import React from 'react';
import { Navigate } from 'react-router-dom';
import { Alert, Box } from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { useProfile } from '../contexts/ProfileContext';

interface RoleBasedRouteProps {
  children: React.ReactNode;
  requiredPage?: string;
  requiredPermission?: string;
  moduleName?: string; // Check if module is enabled globally
}

/**
 * Component that protects routes based on:
 * 1. User authentication
 * 2. Module enabled status (system-wide)
 * 3. Group-based menu access permissions
 * 4. Specific page/permission requirements
 */
function RoleBasedRoute({ 
  children, 
  requiredPage,
  requiredPermission,
  moduleName
}: RoleBasedRouteProps) {
  const { isAuthenticated } = useAuth();
  const { profile, canAccessPage, canAccessMenu, hasPermission, isModuleEnabled } = useProfile();

  // Not authenticated - redirect to login
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Check if module is enabled globally
  if (moduleName && !isModuleEnabled(moduleName)) {
    return (
      <Box sx={{ p: 4 }}>
        <Alert severity="warning">
          <strong>Module Disabled</strong>
          <p>This module has been disabled by the system administrator.</p>
        </Alert>
      </Box>
    );
  }

  // If no profile is set, allow access (fallback for users without assigned profiles)
  // Admin role (based on JWT) should have full access
  if (!profile) {
    return <>{children}</>;
  }
  
  // System admins bypass all permission checks
  if (profile.groupPermissions?.isSystemAdmin) {
    return <>{children}</>;
  }

  // Check menu access from group permissions
  if (requiredPage) {
    // Map page names to menu names for canAccessMenu check
    const pageToMenuMap: Record<string, string> = {
      'Dashboard': 'Dashboard',
      'Customers': 'Customers',
      'Contacts': 'Contacts',
      'Leads': 'Leads',
      'Opportunities': 'Opportunities',
      'Products': 'Products',
      'Services': 'Services',
      'Campaigns': 'Campaigns',
      'Quotes': 'Quotes',
      'Tasks': 'Tasks',
      'Activities': 'Activities',
      'Notes': 'Notes',
      'Workflows': 'Workflows',
      'Reports': 'Reports',
      'Settings': 'Settings',
    };
    
    const menuName = pageToMenuMap[requiredPage];
    if (menuName && !canAccessMenu(menuName)) {
      return (
        <Box sx={{ p: 4 }}>
          <Alert severity="error">
            <strong>Access Denied</strong>
            <p>Your user group does not have permission to access this page.</p>
            <p>Please contact your administrator to request access.</p>
          </Alert>
        </Box>
      );
    }
    
    // Also check legacy page access
    if (!canAccessPage(requiredPage)) {
      return (
        <Box sx={{ p: 4 }}>
          <Alert severity="error">
            <strong>Access Denied</strong>
            <p>You do not have permission to access this page.</p>
            <p>Your profile: {profile.userProfileName || 'Not assigned'}</p>
          </Alert>
        </Box>
      );
    }
  }

  // Check permission
  if (requiredPermission) {
    const permissionKey = requiredPermission as any;
    if (!hasPermission(permissionKey)) {
      return (
        <Box sx={{ p: 4 }}>
          <Alert severity="error">
            <strong>Access Denied</strong>
            <p>You do not have the required permission: {requiredPermission}</p>
          </Alert>
        </Box>
      );
    }
  }

  return <>{children}</>;
}

export default RoleBasedRoute;
